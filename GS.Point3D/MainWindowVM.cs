/* Copyright(C) 2021  Rob Morgan (robert.morgan.e@gmail.com)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
using ASCOM.DriverAccess;
using ASCOM.Utilities;
using GS.Point3D.Classes;
using GS.Point3D.Controls;
using GS.Point3D.Helpers;
using HelixToolkit.Wpf;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using ASCOM.DeviceInterface;
using Model3D = GS.Point3D.Classes.Model3D;
using Timer = System.Timers.Timer;

namespace GS.Point3D
{
    public sealed class MainWindowVM : ObservableObject, IDisposable
    {
        #region Fields

        public static MainWindowVM _mainWindowVM;
        private Telescope _telescope;
        private Timer _timer;
        private readonly object _timerLock = new object();
        private static readonly Util _util = new Util();
        private DateTime? TimerTimeStamp;

        #endregion

        public MainWindowVM()
        {
            try
            {
                using (new WaitCursor())
                {
                    _mainWindowVM = this;
                    GeneralSettings.Load();
                    var monitorItem = new MonitorEntry
                    {
                        Datetime = DateTime.Now,
                        Device = MonitorDevice.Program,
                        Category = MonitorCategory.Interface,
                        Type = MonitorType.Information,
                        Method = MethodBase.GetCurrentMethod().Name,
                        Thread = Thread.CurrentThread.ManagedThreadId,
                        Message = $"Starting"
                    };
                    Helpers.Monitor.LogToMonitor(monitorItem);

                    Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
                    Title = Application.Current.Resources["titleName"].ToString();
                    IntervalList = new List<double>(Numbers.InclusiveRange(20, 5000, 10));

                    //offsets
                    XAxisOffsetList = new List<double>(Numbers.InclusiveRange(-90, 90, 1));
                    YAxisOffsetList = new List<double>(Numbers.InclusiveRange(-90, 90, 1));
                    //ZAxisOffsetList = new List<double>(Numbers.InclusiveRange(0, 90, 1));

                    ClearFlipCard();
                    LoadSpecialSettings();

                    // Theme Colors
                    PrimaryColors = (IList<Swatch>)new SwatchesProvider().Swatches;
                    var primaryColors = PrimaryColors as Swatch[] ?? PrimaryColors.ToArray();
                    AccentColors = primaryColors.Where(item => item.IsAccented).ToList();
                    PrimaryColor = primaryColors.First(item => item.Name.Equals(GeneralSettings.PrimaryColor));
                    AccentColor = primaryColors.First(item => item.Name.Equals(GeneralSettings.AccentColor));
                    var paletteHelper = new PaletteHelper();
                    var theme = paletteHelper.GetTheme();
                    theme.SetBaseTheme(GeneralSettings.DarkTheme ? Theme.Dark : Theme.Light);
                    paletteHelper.SetTheme(theme);

                    if(GeneralSettings.AutoConnect){Connect(true);}

                    if (!GeneralSettings.FirstRun) return;
                    var msg = $"{Application.Current.Resources["FirstMsg"]}" + Environment.NewLine;
                    msg += $"{Application.Current.Resources["FirstMsg1"]}" + Environment.NewLine;
                    msg += $"{Application.Current.Resources["FirstMsg2"]}" + Environment.NewLine + Environment.NewLine;
                    msg += $"{Application.Current.Resources["FirstMsg3"]}" + Environment.NewLine;
                    msg += $"{Application.Current.Resources["FirstMsg4"]}" + Environment.NewLine;
                    OpenDialog(msg);
                    GeneralSettings.FirstRun = false;
                }
            }
            catch (Exception e)
            {
                var monitorItem = new MonitorEntry
                {
                    Datetime = DateTime.Now,
                    Device = MonitorDevice.Telescope,
                    Category = MonitorCategory.Interface,
                    Type = MonitorType.Error,
                    Method = MethodBase.GetCurrentMethod().Name,
                    Thread = Thread.CurrentThread.ManagedThreadId,
                    Message = $"{e.Message},{e.StackTrace?.Trim()}"
                };
                Helpers.Monitor.LogToMonitor(monitorItem);
                OpenDialog(e.Message, $"{Application.Current.Resources["exError"]}");
            }
        }

        #region Settings

        public static List<string> Languages => Helpers.Languages.SupportedLanguages;

        public string Lang
        {
            get => Helpers.Languages.Language;
            set
            {
                Helpers.Languages.Language = value;
                OnPropertyChanged();
                OpenDialog($"{Application.Current.Resources["exRestart"]}");
            }
        }

        private ICommand _clickDonateCmd;

        public ICommand ClickDonateCmd
        {
            get
            {
                var cmd = _clickDonateCmd;
                if (cmd != null)
                {
                    return cmd;
                }

                return _clickDonateCmd = new RelayCmd(
                    param => ClickDonate()
                );
            }
        }

        private void ClickDonate()
        {
            try
            {
                Process.Start(new ProcessStartInfo("https://www.greenswamp.org/GSpoint3d"));
            }
            catch (Exception ex)
            {
                OpenDialog(ex.Message);
            }
        }

        private void ClearFlipCard()
        {

            //RightAscension = $"00h 00:00.00";
            //Declination = $"000° 00:00.00";
            //Azimuth = $"000° 00:00.00";
            //Altitude = $"000° 00:00.00";
            //Lha = $"00:00:00";
            //DegX = $"0";
            //DegY = $"0";
            //SiderealTime = $"00:00:00";
            //PierSide = SOP.pierUnknown;

            RightAscension = string.Empty;
            Declination = string.Empty;
            Azimuth = string.Empty;
            Altitude = string.Empty;
            Lha = string.Empty;
            PierSide = SOP.pierUnknown;
            DegX = string.Empty;
            DegY = string.Empty;
            SiderealTime = string.Empty;
        }

        private void SaveSpecialSettings()
        {
            GeneralSettings.ModelPosition = Position;
            GeneralSettings.ModelLookDirection = LookDirection;
            GeneralSettings.ModelUpDirection = UpDirection;

            GeneralSettings.WindowTop = WindowTop;
            GeneralSettings.WindowHeight = WindowHeight;
            GeneralSettings.WindowLeft = WindowLeft;
            GeneralSettings.WindowWidth = WindowWidth;
        }

        private void LoadSpecialSettings()
        {
            LookDirection = GeneralSettings.ModelLookDirection;
            UpDirection = GeneralSettings.ModelUpDirection;
            Position = GeneralSettings.ModelPosition;

            WindowTop = GeneralSettings.WindowTop;
            WindowHeight = GeneralSettings.WindowHeight;
            WindowLeft = GeneralSettings.WindowLeft;
            WindowWidth = GeneralSettings.WindowWidth;
        }

        #endregion

        #region Viewport3D
        
        public IList<double> XAxisOffsetList { get; }

        public double XOffset
        {
            get => GeneralSettings.XOffset;
            set
            {
                GeneralSettings.XOffset = value;
                OnPropertyChanged();
            }
        }

        public IList<double> YAxisOffsetList { get; }

        public double YOffset
        {
            get => GeneralSettings.YOffset;
            set
            {
                GeneralSettings.YOffset = value;
                OnPropertyChanged();
            }
        }

        //public IList<double> ZAxisOffsetList { get; }

        public double ZOffset
        {
            get => GeneralSettings.ZOffset;
            set
            {
                GeneralSettings.ZOffset = value;
                OnPropertyChanged();
            }
        }

        private bool _modelOn;
        public bool ModelOn
        {
            get => _modelOn;
            set
            {
                _modelOn = value;
                if (value)
                {
                    Rotate();
                    LoadGEM();
                }
                OnPropertyChanged();
            }
        }

        private System.Windows.Media.Media3D.Point3D _position;
        public System.Windows.Media.Media3D.Point3D Position
        {
            get => _position;
            set
            {
                if (_position == value) return;
                _position = value;
                OnPropertyChanged();
            }
        }

        private Vector3D _lookDirection;
        public Vector3D LookDirection
        {
            get => _lookDirection;
            set
            {
                if (_lookDirection == value) return;
                _lookDirection = value;
                OnPropertyChanged();
            }
        }

        private Vector3D _upDirection;
        public Vector3D UpDirection
        {
            get => _upDirection;
            set
            {
                if (_upDirection == value) return;
                _upDirection = value;
                OnPropertyChanged();
            }
        }
        
        public bool FlipCardVis
        {
            get => GeneralSettings.FlipCardVis;
            set
            {
                GeneralSettings.FlipCardVis = value;
                OnPropertyChanged();
            }
        }
        
        public bool RaVis
        {
            get => GeneralSettings.RaVis;
            set
            {
                GeneralSettings.RaVis = value;
                OnPropertyChanged();
            }
        }

        public bool DecVis
        {
            get => GeneralSettings.DecVis;
            set
            {
                GeneralSettings.DecVis = value;
                OnPropertyChanged();
            }
        }

        public bool AzVis
        {
            get => GeneralSettings.AltVis;
            set
            {
                GeneralSettings.AltVis = value;
                OnPropertyChanged();
            }
        }

        public bool AltVis
        {
            get => GeneralSettings.AltVis;
            set
            {
                GeneralSettings.AltVis = value;
                OnPropertyChanged();
            }
        }

        public bool LhaVis
        {
            get => GeneralSettings.LhaVis;
            set
            {
                GeneralSettings.LhaVis = value;
                OnPropertyChanged();
            }
        }

        public bool DescriptionVis
        {
            get => GeneralSettings.DescriptionVis;
            set
            {
                GeneralSettings.DescriptionVis = value;
                //Name = value ? Description : null;
                OnPropertyChanged();
            }
        }

        public bool SideVis
        {
            get => GeneralSettings.SideVis;
            set
            {
                GeneralSettings.SideVis = value;
                OnPropertyChanged();
            }
        }

        public bool SopVis
        {
            get => GeneralSettings.SopVis;
            set
            {
                GeneralSettings.SopVis = value;
                OnPropertyChanged();
            }
        }

        public bool CameraVis
        {
            get => GeneralSettings.CameraVis;
            set
            {
                GeneralSettings.CameraVis = value;
                OnPropertyChanged();
            }
        }

        public bool RaAxisVis
        {
            get => GeneralSettings.RaAxisVis;
            set
            {
                GeneralSettings.RaAxisVis = value;
                OnPropertyChanged();
            }
        }

        public bool DecAxisVis
        {
            get => GeneralSettings.DecAxisVis;
            set
            {
                GeneralSettings.DecAxisVis = value;
                OnPropertyChanged();
            }
        }

        private string _siderealTime;
        public string SiderealTime
        {
            get => _siderealTime;
            set
            {
                if (value == _siderealTime) return;
                _siderealTime = value;
                OnPropertyChanged();
            }
        }

        private System.Windows.Media.Media3D.Model3D _model;
        public System.Windows.Media.Media3D.Model3D Model
        {
            get => _model;
            set
            {
                if (_model == value) return;
                _model = value;
                OnPropertyChanged();
            }
        }

        private double _xAxis;
        public double XAxis
        {
            get => _xAxis;
            set
            {
                _xAxis = value;
                XAxisOffset = value + XOffset;
                OnPropertyChanged();
            }
        }

        private double _yAxis;
        public double YAxis
        {
            get => _yAxis;
            set
            {
                _yAxis = value;
                YAxisOffset = value + YOffset;
                OnPropertyChanged();
            }
        }

        private double _zAxis;
        public double ZAxis
        {
            get => _zAxis;
            set
            {
                _zAxis = value;
                ZAxisOffset = ZOffset - value;
                OnPropertyChanged();
            }
        }

        private double _xAxisOffset;
        public double XAxisOffset
        {
            get => _xAxisOffset;
            set
            {
                _xAxisOffset = value;
                OnPropertyChanged();
            }
        }

        private double _yAxisOffset;
        public double YAxisOffset
        {
            get => _yAxisOffset;
            set
            {
                _yAxisOffset = value;
                OnPropertyChanged();
            }
        }

        private double _zAxisOffset;
        public double ZAxisOffset
        {
            get => _zAxisOffset;
            set
            {
                _zAxisOffset = value;
                OnPropertyChanged();
            }
        }

        public Model3DType ModelType
        {
            get => GeneralSettings.ModelType;
            set
            {
                GeneralSettings.ModelType = value;
                LoadGEM();
                Rotate();
                OnPropertyChanged();
            }
        }

        private Material _compass;
        public Material Compass
        {
            get => _compass;
            set
            {
                _compass = value;
                OnPropertyChanged();
            }
        }

        private void LoadGEM()
        {
            try
            {
                CameraVis = false;

                //camera direction
                LookDirection = GeneralSettings.ModelLookDirection;
                UpDirection = GeneralSettings.ModelUpDirection;
                Position = GeneralSettings.ModelPosition;

                ZAxis = Math.Round(Math.Abs(Latitude), 2);

                //load model and compass
                var import = new ModelImporter();
                var model = import.Load(Model3D.GetModelFile(GeneralSettings.ModelType));
                Compass = MaterialHelper.CreateImageMaterial(Model3D.GetCompassFile(SouthernHemisphere), 100);

                //color OTA
                var accentColor = GeneralSettings.AccentColor;
                if (!string.IsNullOrEmpty(accentColor))
                {
                    var swatches = new SwatchesProvider().Swatches;
                    foreach (var swatch in swatches)
                    {
                        if (swatch.Name != GeneralSettings.AccentColor) continue;
                        var converter = new BrushConverter();
                        var accentbrush = (Brush)converter.ConvertFromString(swatch.ExemplarHue.Color.ToString());

                        var materialota = MaterialHelper.CreateMaterial(accentbrush);
                        if (model.Children[0] is GeometryModel3D ota) ota.Material = materialota;
                    }
                }
                //color weights
                var materialweights = MaterialHelper.CreateMaterial(new SolidColorBrush(Color.FromRgb(64, 64, 64)));
                if (model.Children[1] is GeometryModel3D weights) weights.Material = materialweights;
                //color bar
                var materialbar = MaterialHelper.CreateMaterial(Brushes.Gainsboro);
                if (model.Children[2] is GeometryModel3D bar) bar.Material = materialbar;

                Model = model;
            }
            catch (Exception ex)
            {
                var monitorItem = new MonitorEntry
                {
                    Datetime = DateTime.Now,
                    Device = MonitorDevice.Telescope,
                    Category = MonitorCategory.Interface,
                    Type = MonitorType.Error,
                    Method = MethodBase.GetCurrentMethod().Name,
                    Thread = Thread.CurrentThread.ManagedThreadId,
                    Message = $"{ex.Message},{ex.StackTrace}"
                };
                Helpers.Monitor.LogToMonitor(monitorItem);
                OpenDialog(ex.Message, $"{Application.Current.Resources["exError"]}");
            }
        }
        private void Rotate()
        {
            var axes = Model3D.RotateModel( Axis0, Axis1, SouthernHemisphere);
            YAxis = axes[0];
            XAxis = axes[1];
        }

        private ICommand _openResetViewCmd;
        public ICommand OpenResetViewCmd
        {
            get
            {
                var cmd = _openResetViewCmd;
                if (cmd != null)
                {
                    return cmd;
                }

                return _openResetViewCmd = new RelayCmd(param => OpenResetView());
            }
        }
        private void OpenResetView()
        {
            try
            {
                GeneralSettings.ModelLookDirection = new Vector3D(-2616, -3167, -1170);
                GeneralSettings.ModelUpDirection = new Vector3D(.35, .43, .82);
                GeneralSettings.ModelPosition = new System.Windows.Media.Media3D.Point3D(2523, 3000, 1379);
                LoadGEM();
            }
            catch (Exception ex)
            {
                var monitorItem = new MonitorEntry
                {
                    Datetime = DateTime.Now,
                    Device = MonitorDevice.Telescope,
                    Category = MonitorCategory.Interface,
                    Type = MonitorType.Error,
                    Method = MethodBase.GetCurrentMethod().Name,
                    Thread = Thread.CurrentThread.ManagedThreadId,
                    Message = $"{ex.Message},{ex.StackTrace}"
                };
                Helpers.Monitor.LogToMonitor(monitorItem);
                OpenDialog(ex.Message, $"{Application.Current.Resources["exError"]}");
            }
        }

        #endregion

        #region Polling

        private bool LogFirstPoll { get; set; }
        
        private string _altitude;
        public string Altitude
        {
            get => _altitude;
            set
            {
                if (value == _altitude) return;
                _altitude = value;
                OnPropertyChanged();
            }
        }

        private string _azimuth;
        public string Azimuth
        {
            get => _azimuth;
            set
            {
                if (value == _azimuth) return;
                _azimuth = value;
                OnPropertyChanged();
            }
        }

        private string _declination;
        public string Declination
        {
            get => _declination;
            set
            {
                if (value == _declination) return;
                _declination = value;
                OnPropertyChanged();
            }
        }

        private string _lha;
        public string Lha
        {
            get => _lha;
            set
            {
                if (value == _lha) return;
                _lha = value;
                OnPropertyChanged();
            }
        }

        private string _rightAscension;
        public string RightAscension
        {
            get => _rightAscension;
            set
            {
                if (value == _rightAscension) return;
                _rightAscension = value;
                OnPropertyChanged();
            }
        }

        private string _degX;
        public string DegX
        {
            get => _degX;
            private set
            {
                if (_degX == value) return;
                _degX = value;
                OnPropertyChanged();
            }
        }

        private string _degY;
        public string DegY
        {
            get => _degY;
            private set
            {
                if (_degY == value) return;
                _degY = value;
                OnPropertyChanged();
            }
        }
        
        public AlignMode AlignmentMode { get; set; }

        private double _axis0;
        public double Axis0
        {
            get => _axis0;
            set
            {
                _axis0 = value;
                OnPropertyChanged();
            }
        }
        
        private double _axis1;
        public double Axis1
        {
            get => _axis1;
            set
            {
                _axis1 = value;
                OnPropertyChanged();
            }
        }

        private double _axis2;
        public double Axis2
        {
            get => _axis2;
            set
            {
                _axis2 = value;
                OnPropertyChanged();
            }
        }

        private double _axis3;
        public double Axis3
        {
            get => _axis3;
            set
            {
                _axis3 = value;
                OnPropertyChanged();
            }
        }

        private double _axis4;
        public double Axis4
        {
            get => _axis4;
            set
            {
                _axis4 = value;
                OnPropertyChanged();
            }
        }

        private double _axis5;
        public double Axis5
        {
            get => _axis5;
            set
            {
                _axis5 = value;
                OnPropertyChanged();
            }
        }

        private string _description;
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public string Sop
        {
            get
            {
                var a = (Sop) PierSide;
                return a == Helpers.Sop.None ? string.Empty : a.ToString();
            }
        }

        private double Ra { get; set; }

        private double Dec { get; set; }

        public double Latitude { get; set; }

        private double Longitude { get; set; }

        private SOP _pierSide;
        public SOP PierSide
        {
            get => _pierSide;
            private set
            {
                _pierSide = value;
                // ReSharper disable once ExplicitCallerInfoArgument
                OnPropertyChanged($"Sop");
            }
        }

        public double SideRealtime { get; set; }

        public bool SouthernHemisphere => Latitude < 0;

        public IList<double> IntervalList { get; }

        private bool IsPollRunning { get; set; }

        public double PollTime
        {
            get => GeneralSettings.PollTime;
            set
            {
                GeneralSettings.PollTime = value;
                if (_timer != null)
                {
                    if (_timer.Enabled)
                    {
                        _timer.Stop();
                        _timer.Interval = (int) value;
                        _timer.Start();
                    }
                }
                OnPropertyChanged();
            }
        }

        private void StartPolling()
        {
            if (IsPollRunning) return;

            LogFirstPoll = false;

            Latitude =_telescope.SiteLatitude;
            Longitude = _telescope.SiteLongitude;
            Description = _telescope.Description;
            var alignModeResulTryParse = Enum.TryParse(_telescope.AlignmentMode.ToString(), out AlignMode pAlignMode);
            AlignmentMode = alignModeResulTryParse ? pAlignMode : AlignMode.algUnknown;
            if (AlignmentMode != AlignMode.algGermanPolar)
            {
               throw new Exception($"{Application.Current.Resources["exNoAlignmentMode"]}");
            }

            var monitorItem = new MonitorEntry
            {
                Datetime = DateTime.Now,
                Device = MonitorDevice.Program,
                Category = MonitorCategory.Interface,
                Type = MonitorType.Information,
                Method = MethodBase.GetCurrentMethod().Name,
                Thread = Thread.CurrentThread.ManagedThreadId,
                Message = $"{Description}, {Latitude}, {Longitude}, {AlignmentMode}"
            };
            Helpers.Monitor.LogToMonitor(monitorItem);

            ModelOn = true;
            TimerTimeStamp = DateTime.Now;
            ZAxis = Math.Round(Math.Abs(Latitude), 2);

            if (_timer == null)
            {
                _timer = new Timer((int)PollTime) { Enabled = true };
                _timer.Elapsed += PollEvent;
            }
            
            IsPollRunning = true;
        }

        private void StopPolling()
        {
            if (_timer != null)
            {
                _timer.Elapsed -= PollEvent;
            }
            _timer?.Stop();
            _timer = null;
            IsPollRunning = false;
            Description = null;
            ModelOn = false;

            if (TimerTimeStamp == null){return;}
            var monitorItem = new MonitorEntry
            {
                Datetime = DateTime.Now,
                Device = MonitorDevice.Program,
                Category = MonitorCategory.Interface,
                Type = MonitorType.Information,
                Method = MethodBase.GetCurrentMethod().Name,
                Thread = Thread.CurrentThread.ManagedThreadId,
                Message = $"{DateTime.Now - TimerTimeStamp}"
            };
            Helpers.Monitor.LogToMonitor(monitorItem);
            
            TimerTimeStamp = null;
            ClearFlipCard();
            SaveSpecialSettings();
        }

        private void PollEvent(object sender, EventArgs e)
        {
            var hasLock = false;
            try
            {
                System.Threading.Monitor.TryEnter(_timerLock, ref hasLock);
                if (!hasLock) { return; }
                IsPollRunning = true;

                // get needed data
                var sopResulTryParse = Enum.TryParse(_telescope.SideOfPier.ToString(), out SOP pSop);
                PierSide = sopResulTryParse ? pSop : SOP.pierUnknown;
                SideRealtime = _telescope.SiderealTime;
                Ra = _telescope.RightAscension;
                Dec = _telescope.Declination;

                //convert axes to model
                var raDec = Axes.RaDecToAxesXY(AlignmentMode, new[]{Ra, Dec});
                Axis0 = raDec[0];
                Axis1 = raDec[1];

                Rotate();

                // get other info
                if (FlipCardVis)
                {
                    if (AltVis){Altitude = _util.DegreesToDMS(_telescope.Altitude, "° ", ":", "", 2);}
                    if (AzVis){Azimuth = _util.DegreesToDMS(_telescope.Azimuth, "° ", ":", "", 2);}
                    if (DecVis){Declination = _util.DegreesToDMS(Dec, "° ", ":", "", 2);}
                    if (RaVis){RightAscension = _util.HoursToHMS(Ra, "h ", ":", "", 2);}
                    if (RaAxisVis){DegX = $"{Math.Round(Axis0, 2)}°";}
                    if (DecAxisVis){DegY = $"{Math.Round(Axis1, 2)}°";}
                    if (SideVis){SiderealTime = _util.HoursToHMS(SideRealtime);}
                    if (LhaVis){Lha = _util.HoursToHMS(Numbers.Ra2Ha12(Ra, SideRealtime));}
                }

                if (LogFirstPoll){return;}
                
                var msg = $"Latitude={_util.DegreesToDMS(_telescope.SiteLatitude, "° ", ":", "", 2)}, ";
                msg += $"Longitude={_util.DegreesToDMS(_telescope.SiteLongitude, "° ", ":", "", 2)}, ";
                msg += $"Altitude={_util.DegreesToDMS(_telescope.Altitude, "° ", ":", "", 2)}, ";
                msg += $"Azimuth = {_util.DegreesToDMS(_telescope.Azimuth, "° ", ":", "", 2)}, ";
                msg += $"Declination = {_util.DegreesToDMS(Dec, "° ", ":", "", 2)}, ";
                msg += $"RightAscension = {_util.HoursToHMS(Ra, "h ", ":", "", 2)}, ";
                msg += $"RightAscension = {_util.HoursToHMS(Ra, "h ", ":", "", 2)}, ";
                msg += $"DegX = {Math.Round(Axis0, 2)}°, ";
                msg += $"DegY = {Math.Round(Axis1, 2)}°, ";
                msg += $"SiderealTime = {_util.HoursToHMS(SideRealtime)}, ";
                msg += $"LHA = {_util.HoursToHMS(Numbers.Ra2Ha12(Ra, SideRealtime))}";

                var monitorItem = new MonitorEntry
                {
                    Datetime = DateTime.Now,
                    Device = MonitorDevice.Telescope,
                    Category = MonitorCategory.Interface,
                    Type = MonitorType.Error,
                    Method = MethodBase.GetCurrentMethod().Name,
                    Thread = Thread.CurrentThread.ManagedThreadId,
                    Message = msg
                };
                Helpers.Monitor.LogToMonitor(monitorItem);
                LogFirstPoll = true;

            }
            catch (Exception ex)
            {
                Connect(false);
                var monitorItem = new MonitorEntry
                {
                    Datetime = DateTime.Now,
                    Device = MonitorDevice.Telescope,
                    Category = MonitorCategory.Interface,
                    Type = MonitorType.Error,
                    Method = MethodBase.GetCurrentMethod().Name,
                    Thread = Thread.CurrentThread.ManagedThreadId,
                    Message = $"{ex.Message},{ex.StackTrace?.Trim()}"
                };
                Helpers.Monitor.LogToMonitor(monitorItem);

                ThreadContext.BeginInvokeOnUiThread(delegate { OpenDialog(ex.Message, $"{Application.Current.Resources["exError"]}"); });
            }
            finally
            {
                if (hasLock) { System.Threading.Monitor.Exit(_timerLock); }
            }
        }

        #endregion

        #region Colors

        public bool DarkTheme
        {
            get => GeneralSettings.DarkTheme;
            set
            {
                GeneralSettings.DarkTheme = value;
                OnPropertyChanged();
            }
        }

        public IList<Swatch> PrimaryColors { get; }
        private Swatch _primaryColor;

        public Swatch PrimaryColor
        {
            get => _primaryColor;
            set
            {
                _primaryColor = value;
                var paletteHelper = new PaletteHelper();
                var theme = paletteHelper.GetTheme();
                theme.SetPrimaryColor(_primaryColor.ExemplarHue.Color);
                paletteHelper.SetTheme(theme);
                GeneralSettings.PrimaryColor = _primaryColor.Name;
                OnPropertyChanged();
            }
        }

        public IList<Swatch> AccentColors { get; }
        private Swatch _accentColor;

        public Swatch AccentColor
        {
            get => _accentColor;
            set
            {
                _accentColor = value;
                var paletteHelper = new PaletteHelper();
                var theme = paletteHelper.GetTheme();
                theme.SetSecondaryColor(_accentColor.ExemplarHue.Color);
                paletteHelper.SetTheme(theme);
                GeneralSettings.AccentColor = _accentColor.Name;
                LoadGEM();
                OnPropertyChanged();
            }
        }


        private ICommand _clickBaseCommand;

        public ICommand ClickBaseCommand
        {
            get
            {
                var command = _clickBaseCommand;
                if (command != null)
                {
                    return command;
                }

                return _clickBaseCommand = new RelayCmd(
                    param => ClickBase((bool)param)
                );
            }
        }

        private void ClickBase(bool isDark)
        {
            try
            {
                using (new WaitCursor())
                {
                    var paletteHelper = new PaletteHelper();
                    var theme = paletteHelper.GetTheme();
                    theme.SetBaseTheme(isDark ? Theme.Dark : Theme.Light);
                    paletteHelper.SetTheme(theme);
                }
            }
            catch (Exception e)
            {
                var monitorItem = new MonitorEntry
                {
                    Datetime = DateTime.Now,
                    Device = MonitorDevice.Telescope,
                    Category = MonitorCategory.Interface,
                    Type = MonitorType.Error,
                    Method = MethodBase.GetCurrentMethod().Name,
                    Thread = Thread.CurrentThread.ManagedThreadId,
                    Message = $"{e.Message}"
                };
                Helpers.Monitor.LogToMonitor(monitorItem);

                OpenDialog(e.Message, $"{Application.Current.Resources["exError"]}");
            }
        }

        #endregion

        #region Connect

        public bool AutoConnect
        {
            get => GeneralSettings.AutoConnect;
            set
            {
                GeneralSettings.AutoConnect = value;
                OnPropertyChanged();
            }
        }

        public string TelescopeID
        {
            get => GeneralSettings.TelescopeID;
            set
            {
                GeneralSettings.TelescopeID = value;
                OnPropertyChanged();
            }
        }

        private bool _connected;
        public bool Connected
        {
            get => _connected;
            set
            {
                _connected = value;
                if (value)
                {
                    StartPolling();
                }
                else
                {
                    StopPolling();
                }
                OnPropertyChanged();
            }
        }

        private string _connectBadgeContent;
        public string ConnectBadgeContent
        {
            get => _connectBadgeContent;
            set
            {
                if (_connectBadgeContent == value) return;
                _connectBadgeContent = value;
                OnPropertyChanged();
            }
        }

        private ICommand _clickChooserCmd;
        public ICommand ClickChooserCmd
        {
            get
            {
                var command = _clickChooserCmd;
                if (command != null)
                {
                    return command;
                }

                return _clickChooserCmd = new RelayCmd(
                    param => ClickChooser()
                );
            }
        }
        private void ClickChooser()
        {
            try
            {
                var chooser = new Chooser {DeviceType = @"Telescope"};
                var id = chooser.Choose(TelescopeID);
                chooser.Dispose();
                if (string.IsNullOrEmpty(id)) { return; }
                TelescopeID = id;
            }
            catch (Exception e)
            {
                var monitorItem = new MonitorEntry
                {
                    Datetime = DateTime.Now,
                    Device = MonitorDevice.Telescope,
                    Category = MonitorCategory.Interface,
                    Type = MonitorType.Error,
                    Method = MethodBase.GetCurrentMethod().Name,
                    Thread = Thread.CurrentThread.ManagedThreadId,
                    Message = $"{e.Message},{e.StackTrace?.Trim()}"
                };
                Helpers.Monitor.LogToMonitor(monitorItem);
                OpenDialog(e.Message, $"{Application.Current.Resources["exError"]}");
            }
        }

        private ICommand _clickConnectCmd;
        public ICommand ClickConnectCmd
        {
            get
            {
                var command = _clickConnectCmd;
                if (command != null)
                {
                    return command;
                }

                return _clickConnectCmd = new RelayCmd(
                    param => ClickConnect()
                );
            }
        }
        private void ClickConnect()
        {
            try
            {
                using (new WaitCursor())
                {
                    Connect(!Connected);
                }
            }
            catch (Exception e)
            {
                var monitorItem = new MonitorEntry
                {
                    Datetime = DateTime.Now,
                    Device = MonitorDevice.Telescope,
                    Category = MonitorCategory.Interface,
                    Type = MonitorType.Error,
                    Method = MethodBase.GetCurrentMethod().Name,
                    Thread = Thread.CurrentThread.ManagedThreadId,
                    Message = $"{e.Message},{e.StackTrace?.Trim()}"
                };
                Helpers.Monitor.LogToMonitor(monitorItem);
                Connect(false);
                OpenDialog(e.Message, $"{Application.Current.Resources["exError"]}");
            }

        }

        private void Connect(bool con)
        {
            try
            {
                if (string.IsNullOrEmpty(TelescopeID))
                {
                    OpenDialog($"{Application.Current.Resources["exNoSelected"]}", $"{Application.Current.Resources["exError"]}");
                    return;
                }

                var monitorItem = new MonitorEntry
                {
                    Datetime = DateTime.Now,
                    Device = MonitorDevice.Program,
                    Category = MonitorCategory.Interface,
                    Type = MonitorType.Information,
                    Method = MethodBase.GetCurrentMethod().Name,
                    Thread = Thread.CurrentThread.ManagedThreadId,
                    Message = $"{con},{TelescopeID}"
                };
                Helpers.Monitor.LogToMonitor(monitorItem);

                if (con)
                {
                    if (_telescope == null)
                    {
                        _telescope = new Telescope(TelescopeID);
                    }
                    _telescope.Connected = true;
                    Connected = true;
                    ConnectBadgeContent = $"{Application.Current.Resources["On"]}";
                    return;
                }

                Connected = false;
                ConnectBadgeContent = "";

                try
                {
                    if (_telescope != null)
                    {
                        _telescope.Connected = false;
                        _telescope?.Dispose();
                    }
                }
                catch
                {
                    // ignored
                }

                try
                {
                    if (_telescope != null) Marshal.ReleaseComObject(_telescope);
                }
                catch
                {
                    // ignored
                }
                _telescope = null;
                GC.Collect();
            }
            catch (Exception e)
            {
                _telescope?.Dispose();
                _telescope = null;
                Console.WriteLine(e);
                throw;
            }
        }

        #endregion

        #region WindowState

        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        private string _version;
        public string Version
        {
            get => _version;
            set
            {
                _version = value;
                OnPropertyChanged();
            }
        }

        public bool TopMost
        {
            get => GeneralSettings.TopMost;
            set
            {
                GeneralSettings.TopMost = value;
                OnPropertyChanged();
            }
        }

        private double _windowHeight;
        public double WindowHeight
        {
            get => _windowHeight;
            set
            {
                if (Math.Abs(value - _windowHeight) < 10) return;
                _windowHeight = value;
                OnPropertyChanged();
            }
        }

        private double _windowWidth;
        public double WindowWidth
        {
            get => _windowWidth;
            set
            {
                if (Math.Abs(value - _windowWidth) < 10) return;
                _windowWidth = value;
                OnPropertyChanged();
            }
        }

        private double _windowLeft;
        public double WindowLeft
        {
            get => _windowLeft;
            set
            {
                if (Math.Abs(value - _windowLeft) < 10) return;
                _windowLeft = value;
                OnPropertyChanged();
            }
        }

        private double _windowTop;
        public double WindowTop
        {
            get => _windowTop;
            set
            {
                if (Math.Abs(value - _windowTop) < 10) return;
                _windowTop = value;
                OnPropertyChanged();
            }
        }

        private ICommand _minimizeWindowCmd;
        public ICommand MinimizeWindowCmd
        {
            get
            {
                var command = _minimizeWindowCmd;
                if (command != null)
                {
                    return command;
                }

                return _minimizeWindowCmd = new RelayCmd(
                    param => MinimizeWindow()
                );
            }
        }
        private void MinimizeWindow()
        {
            WindowStates = WindowState.Minimized;
        }

        private ICommand _maximizeWindowCmd;
        public ICommand MaximizeWindowCmd
        {
            get
            {
                var command = _maximizeWindowCmd;
                if (command != null)
                {
                    return command;
                }

                return _maximizeWindowCmd = new RelayCmd(
                    param => MaximizeWindow()
                );
            }
        }
        private void MaximizeWindow()
        {
            WindowStates = WindowStates != WindowState.Maximized ? WindowState.Maximized : WindowState.Normal;
        }

        private ICommand _closeWindowCmd;
        public ICommand CloseWindowCmd
        {
            get
            {
                var cmd = _closeWindowCmd;
                if (cmd != null)
                {
                    return cmd;
                }

                return _closeWindowCmd = new RelayCmd(
                    param => CloseWindow()
                );
            }
        }
        private void CloseWindow()
        {
            Connect(false);
            var win = Application.Current.Windows.OfType<MainWindowV>().FirstOrDefault();
            win?.Close();
        }

        private WindowState _windowState;
        public WindowState WindowStates
        {
            get => _windowState;
            set
            {
                _windowState = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Dialog  

        private string _DialogMsg;
        public string DialogMsg
        {
            get => _DialogMsg;
            set
            {
                if (_DialogMsg == value) return;
                _DialogMsg = value;
                OnPropertyChanged();
            }
        }

        private bool _isDialogOpen;
        public bool IsDialogOpen
        {
            get => _isDialogOpen;
            set
            {
                if (_isDialogOpen == value) return;
                _isDialogOpen = value;
                OnPropertyChanged();
            }
        }

        private string _dialogCaption;
        public string DialogCaption
        {
            get => _dialogCaption;
            set
            {
                if (_dialogCaption == value) return;
                _dialogCaption = value;
                OnPropertyChanged();
            }
        }

        private object _dialogContent;
        public object DialogContent
        {
            get => _dialogContent;
            set
            {
                if (_dialogContent == value) return;
                _dialogContent = value;
                OnPropertyChanged();
            }
        }

        private ICommand _openDialogCmd;
        public ICommand OpenDialogCmd
        {
            get
            {

                var command = _openDialogCmd;
                if (command != null)
                {
                    return command;
                }

                return _openDialogCmd = new RelayCmd(
                    param => OpenDialog(null)
                );
            }
        }
        private void OpenDialog(string msg, string caption = null)
        {
            if (msg != null) DialogMsg = msg;
            DialogCaption = caption ?? Application.Current.Resources["titleName"].ToString();
            DialogContent = new DialogOk();
            IsDialogOpen = true;

            var monitorItem = new MonitorEntry
            {
                Datetime = DateTime.Now,
                Device = MonitorDevice.Program,
                Category = MonitorCategory.Interface,
                Type = MonitorType.Information,
                Method = MethodBase.GetCurrentMethod().Name,
                Thread = Thread.CurrentThread.ManagedThreadId,
                Message = $"{msg}"
            };
            Helpers.Monitor.LogToMonitor(monitorItem);
        }

        private ICommand _clickOkDialogCmd;
        public ICommand ClickOkDialogCmd
        {
            get
            {
                var command = _clickOkDialogCmd;
                if (command != null)
                {
                    return command;
                }

                return _clickOkDialogCmd = new RelayCmd(
                    param => ClickOkDialog()
                );
            }
        }
        private void ClickOkDialog()
        {
            IsDialogOpen = false;
        }

        private ICommand _cancelDialogCmd;
        public ICommand CancelDialogCmd
        {
            get
            {
                var command = _cancelDialogCmd;
                if (command != null)
                {
                    return command;
                }

                return _cancelDialogCmd = new RelayCmd(
                    param => CancelDialog()
                );
            }
        }
        private void CancelDialog()
        {
            IsDialogOpen = false;
        }

        private ICommand _runMessageDialog;

        public ICommand RunMessageDialogCmd
        {
            get
            {
                var command = _runMessageDialog;
                if (command != null)
                {
                    return command;
                }

                return _runMessageDialog = new RelayCmd(
                    param => ExecuteMessageDialog()
                );
            }
        }
        private async void ExecuteMessageDialog()
        {
            var view = new ErrorMessageDialog
            {
                DataContext = new ErrorMessageDialogVM()
            };

            //show the dialog
            await DialogHost.Show(view, "RootDialog", ClosingMessageEventHandler);
        }
        private static void ClosingMessageEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            Console.WriteLine(@"You can intercept the closing event, and cancel here.");
        }

        #endregion

        #region Dispose
        public void Dispose()
        {
            Dispose(true);
        }
        // NOTE: Leave out the finalizer altogether if this class doesn't
        // own unmanaged resources itself, but leave the other methods
        // exactly as they are.
        ~MainWindowVM()
        {
            SaveSpecialSettings();
            GeneralSettings.Save();

            // Finalizer calls Dispose(false)
            Dispose(false);
        }
        // The bulk of the clean-up code is implemented in Dispose(bool)
        private void Dispose(bool disposing)
        {
            if (!disposing) return;
            _telescope?.Dispose();
            try
            {
                if (_telescope != null) _telescope.Connected = false;
            }
            catch
            {
                // ignored
            }

            _telescope?.Dispose();

            try
            {
                Marshal.ReleaseComObject(_telescope ?? throw new InvalidOperationException());
            }
            catch
            {
                // ignored
            }

            // free native resources if there are any.
            //if (nativeResource != IntPtr.Zero)
            //{
            //    Marshal.FreeHGlobal(nativeResource);
            //    nativeResource = IntPtr.Zero;
            //}
        }
        #endregion

        #region Test

        private ICommand _clickTestCmd;
        public ICommand ClickTestCmd
        {
            get
            {
                var command = _clickTestCmd;
                if (command != null)
                {
                    return command;
                }

                return _clickTestCmd = new RelayCmd(
                    param => ClickTest()
                );
            }
        }
        private void ClickTest()
        {
            try
            {
                using (new WaitCursor())
                {
                 //   _telescope.MoveAxis(TelescopeAxes.axisPrimary, 1);
                    _telescope.MoveAxis(TelescopeAxes.axisPrimary, 0);
                }
            }
            catch (Exception e)
            {
                var monitorItem = new MonitorEntry
                {
                    Datetime = DateTime.Now,
                    Device = MonitorDevice.Telescope,
                    Category = MonitorCategory.Interface,
                    Type = MonitorType.Error,
                    Method = MethodBase.GetCurrentMethod().Name,
                    Thread = Thread.CurrentThread.ManagedThreadId,
                    Message = $"{e.Message},{e.StackTrace?.Trim()}"
                };
                Helpers.Monitor.LogToMonitor(monitorItem);
                Connect(false);
                OpenDialog(e.Message, $"{Application.Current.Resources["exError"]}");
            }

        }

        #endregion
    }
}
