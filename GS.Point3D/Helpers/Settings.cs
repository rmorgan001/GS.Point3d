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
using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Media.Media3D;

namespace GS.Point3D.Helpers
{
    public static class GeneralSettings
    {
        #region Events

        public static event PropertyChangedEventHandler StaticPropertyChanged;

        #endregion

        #region Settings

        private static bool _firstRun;
        public static bool FirstRun
        {
            get => _firstRun;
            set
            {
                if (_firstRun == value) return;
                _firstRun = value;
                Domain.General.Default.FirstRun = value;
                LogSetting(MethodBase.GetCurrentMethod().Name, $"{value}");
                OnStaticPropertyChanged();
            }
        }

        private static bool _sopVis;
        public static bool SopVis
        {
            get => _sopVis;
            set
            {
                if (_sopVis == value) return;
                _sopVis = value;
                Domain.General.Default.SopVis = value;
                LogSetting(MethodBase.GetCurrentMethod().Name, $"{value}");
                OnStaticPropertyChanged();
            }
        }

        private static bool _lhaVis;
        public static bool LhaVis
        {
            get => _lhaVis;
            set
            {
                if (_lhaVis == value) return;
                _lhaVis = value;
                Domain.General.Default.LhaVis = value;
                LogSetting(MethodBase.GetCurrentMethod().Name, $"{value}");
                OnStaticPropertyChanged();
            }
        }

        private static bool _descriptionVis;
        public static bool DescriptionVis
        {
            get => _descriptionVis;
            set
            {
                if (_descriptionVis == value) return;
                _descriptionVis = value;
                Domain.General.Default.DescriptionVis = value;
                LogSetting(MethodBase.GetCurrentMethod().Name, $"{value}");
                OnStaticPropertyChanged();
            }
        }

        private static bool _flipCardVis;
        public static bool FlipCardVis
        {
            get => _flipCardVis;
            set
            {
                if (_flipCardVis == value) return;
                _flipCardVis = value;
                Domain.General.Default.FlipCardVis = value;
                LogSetting(MethodBase.GetCurrentMethod().Name, $"{value}");
                OnStaticPropertyChanged();
            }
        }

        private static bool _raVis;
        public static bool RaVis
        {
            get => _raVis;
            set
            {
                if (_raVis == value) return;
                _raVis = value;
                Domain.General.Default.RaVis = value;
                LogSetting(MethodBase.GetCurrentMethod().Name, $"{value}");
                OnStaticPropertyChanged();
            }
        }

        private static bool _decVis;
        public static bool DecVis
        {
            get => _decVis;
            set
            {
                if (_decVis == value) return;
                _decVis = value;
                Domain.General.Default.DecVis = value;
                LogSetting(MethodBase.GetCurrentMethod().Name, $"{value}");
                OnStaticPropertyChanged();
            }
        }

        private static bool _azVis;
        public static bool AzVis
        {
            get => _azVis;
            set
            {
                if (_azVis == value) return;
                _azVis = value;
                Domain.General.Default.AzVis = value;
                LogSetting(MethodBase.GetCurrentMethod().Name, $"{value}");
                OnStaticPropertyChanged();
            }
        }

        private static bool _altVis;
        public static bool AltVis
        {
            get => _altVis;
            set
            {
                if (_altVis == value) return;
                _altVis = value;
                Domain.General.Default.AltVis = value;
                LogSetting(MethodBase.GetCurrentMethod().Name, $"{value}");
                OnStaticPropertyChanged();
            }
        }

        private static bool _sideVis;
        public static bool SideVis
        {
            get => _sideVis;
            set
            {
                if (_sideVis == value) return;
                _sideVis = value;
                Domain.General.Default.SideVis = value;
                LogSetting(MethodBase.GetCurrentMethod().Name, $"{value}");
                OnStaticPropertyChanged();
            }
        }

        private static bool _cameraVis;
        public static bool CameraVis
        {
            get => _cameraVis;
            set
            {
                if (_cameraVis == value) return;
                _cameraVis = value;
                Domain.General.Default.CameraVis = value;
                LogSetting(MethodBase.GetCurrentMethod().Name, $"{value}");
                OnStaticPropertyChanged();
            }
        }

        private static bool _raAxisVis;
        public static bool RaAxisVis
        {
            get => _raAxisVis;
            set
            {
                if (_raAxisVis == value) return;
                _raAxisVis = value;
                Domain.General.Default.RaAxisVis = value;
                LogSetting(MethodBase.GetCurrentMethod().Name, $"{value}");
                OnStaticPropertyChanged();
            }
        }

        private static bool _decAxisVis;
        public static bool DecAxisVis
        {
            get => _decAxisVis;
            set
            {
                if (_decAxisVis == value) return;
                _decAxisVis = value;
                Domain.General.Default.DecAxisVis = value;
                LogSetting(MethodBase.GetCurrentMethod().Name, $"{value}");
                OnStaticPropertyChanged();
            }
        }


        private static string _accentColor;
        public static string AccentColor
        {
            get => _accentColor;
            set
            {
                if (_accentColor == value) return;
                _accentColor = value;
                Domain.General.Default.AccentColor = value;
                LogSetting(MethodBase.GetCurrentMethod().Name, $"{value}");
                OnStaticPropertyChanged();
            }
        }

        private static bool _autoConnect;
        public static bool AutoConnect
        {
            get => _autoConnect;
            set
            {
                if (_autoConnect == value) return;
                _autoConnect = value;
                Domain.General.Default.AutoConnect = value;
                LogSetting(MethodBase.GetCurrentMethod().Name, $"{value}");
                OnStaticPropertyChanged();
            }
        }

        private static bool _darkTheme;
        public static bool DarkTheme
        {
            get => _darkTheme;
            set
            {
                if (_darkTheme == value) return;
                _darkTheme = value;
                Domain.General.Default.DarkTheme = value;
                LogSetting(MethodBase.GetCurrentMethod().Name, $"{value}");
                OnStaticPropertyChanged();
            }
        }

        private static string _language;
        public static string Language
        {
            get => _language;
            set
            {
                if (_language == value) return;
                _language = value;
                Domain.General.Default.Language = value;
                LogSetting(MethodBase.GetCurrentMethod().Name, $"{value}");
                OnStaticPropertyChanged();
            }
        }

        private static bool _log;
        public static bool Log
        {
            get => _log;
            private set
            {
                if (_log == value) return;
                _log = value;
                Domain.General.Default.Log = value;
                LogSetting(MethodBase.GetCurrentMethod().Name, $"{value}");
                OnStaticPropertyChanged();
            }
        }

        private static Model3DType _modelType;
        public static Model3DType ModelType
        {
            get => _modelType;
            set
            {
                if (_modelType == value) return;
                _modelType = value;
                Domain.General.Default.ModelType = value.ToString();
                LogSetting(MethodBase.GetCurrentMethod().Name, $"{value}");
                OnStaticPropertyChanged();
            }
        }

        private static Vector3D _modelLookDirection;
        public static Vector3D ModelLookDirection
        {
            get => _modelLookDirection;
            set
            {
                _modelLookDirection = value;
                Domain.General.Default.ModelLookDirection = value.ToString(CultureInfo.InvariantCulture);
                LogSetting(MethodBase.GetCurrentMethod().Name, $"{value}");
            }
        }

        private static System.Windows.Media.Media3D.Point3D _modelPosition;
        public static System.Windows.Media.Media3D.Point3D ModelPosition
        {
            get => _modelPosition;
            set
            {
                _modelPosition = value;
                Domain.General.Default.ModelPosition = value.ToString(CultureInfo.InvariantCulture);
                LogSetting(MethodBase.GetCurrentMethod().Name, $"{value}");
            }
        }

        private static Vector3D _modelUpDirection;
        public static Vector3D ModelUpDirection
        {
            get => _modelUpDirection;
            set
            {
                _modelUpDirection = value;
                Domain.General.Default.ModelUpDirection = value.ToString(CultureInfo.InvariantCulture);
                LogSetting(MethodBase.GetCurrentMethod().Name, $"{value}");
            }
        }

        private static double _pollTime;
        public static double PollTime
        {
            get => _pollTime;
            set
            {
                if (Math.Abs(_pollTime - value) < .000001) return;
                _pollTime = value;
                Domain.General.Default.PollTime = value;
                LogSetting(MethodBase.GetCurrentMethod().Name, $"{value}");
                OnStaticPropertyChanged();
            }
        }

        private static string _primaryColor;
        public static string PrimaryColor
        {
            get => _primaryColor;
            set
            {
                if (_primaryColor == value) return;
                _primaryColor = value;
                Domain.General.Default.PrimaryColor = value;
                LogSetting(MethodBase.GetCurrentMethod().Name, $"{value}");
                OnStaticPropertyChanged();
            }
        }

        private static string _telescopeID;
        public static string TelescopeID
        {
            get => _telescopeID;
            set
            {
                if (_telescopeID == value) return;
                _telescopeID = value;
                Domain.General.Default.TelescopeID = value;
                LogSetting(MethodBase.GetCurrentMethod().Name, $"{value}");
                OnStaticPropertyChanged();
            }
        }
        
        private static bool _topMost;
        public static bool TopMost
        {
            get => _topMost;
            set
            {
                if (_topMost == value) return;
                _topMost = value;
                Domain.General.Default.TopMost = value;
                LogSetting(MethodBase.GetCurrentMethod().Name, $"{value}");
                OnStaticPropertyChanged();
            }
        }

        private static string _version;
        private static string Version
        {
            get => _version;
            set
            {
                if (_version == value) return;
                _version = value;
                Domain.General.Default.Version = value;
                LogSetting(MethodBase.GetCurrentMethod().Name, $"{value}");
                OnStaticPropertyChanged();
            }
        }

        private static WindowState _windowState;
        public static WindowState WindowState
        {
            get => _windowState;
            set
            {
                _windowState = value;
                Domain.General.Default.WindowState = value;
                LogSetting(MethodBase.GetCurrentMethod().Name, value.ToString());
                OnStaticPropertyChanged();
            }
        }

        private static double _windowHeight;
        public static double WindowHeight
        {
            get => _windowHeight;
            set
            {
                if (Math.Abs(_windowHeight - value) < 0.1) return;
                _windowHeight = value;
                Domain.General.Default.WindowHeight = value;
                LogSetting(MethodBase.GetCurrentMethod().Name, $"{value}");
                OnStaticPropertyChanged();
            }
        }

        private static double _windowWidth;
        public static double WindowWidth
        {
            get => _windowWidth;
            set
            {
                if (Math.Abs(_windowWidth - value) < 0.1) return;
                _windowWidth = value;
                Domain.General.Default.WindowWidth = value;
                LogSetting(MethodBase.GetCurrentMethod().Name, $"{value}");
                OnStaticPropertyChanged();
            }
        }

        private static double _windowLeft;
        public static double WindowLeft
        {
            get => _windowLeft;
            set
            {
                if (Math.Abs(_windowLeft - value) < 0.1) return;
                _windowLeft = value;
                Domain.General.Default.WindowLeft = value;
                LogSetting(MethodBase.GetCurrentMethod().Name, $"{value}");
                OnStaticPropertyChanged();
            }
        }

        private static double _windowTop;
        public static double WindowTop
        {
            get => _windowTop;
            set
            {
                if (Math.Abs(_windowTop - value) < 0.1) return;
                _windowTop = value;
                Domain.General.Default.WindowTop = value;
                LogSetting(MethodBase.GetCurrentMethod().Name, $"{value}");
                OnStaticPropertyChanged();
            }
        }

        private static double _xOffset;
        public static double XOffset
        {
            get => _xOffset;
            set
            {
                if (Math.Abs(_xOffset - value) < 0.1) return;
                _xOffset = value;
                Domain.General.Default.xAxisOffset = value;
                LogSetting(MethodBase.GetCurrentMethod().Name, $"{value}");
                OnStaticPropertyChanged();
            }
        }

        private static double _yOffset;
        public static double YOffset
        {
            get => _yOffset;
            set
            {
                if (Math.Abs(_yOffset - value) < 0.1) return;
                _yOffset = value;
                Domain.General.Default.yAxisOffset = value;
                LogSetting(MethodBase.GetCurrentMethod().Name, $"{value}");
                OnStaticPropertyChanged();
            }
        }

        private static double _zOffset;
        public static double ZOffset
        {
            get => _zOffset;
            set
            {
                if (Math.Abs(_zOffset - value) < 0.1) return;
                _zOffset = value;
                Domain.General.Default.zAxisOffset = value;
                LogSetting(MethodBase.GetCurrentMethod().Name, $"{value}");
                OnStaticPropertyChanged();
            }
        }
        
        #endregion

        #region Methods      

        /// <summary>
        /// will upgrade if necessary
        /// </summary>
        public static void Load()
        {
            Log = Domain.General.Default.Log;
            Version = Domain.General.Default.Version;

            Upgrade();

            FirstRun = Domain.General.Default.FirstRun;
            FlipCardVis = Domain.General.Default.FlipCardVis;
            LhaVis = Domain.General.Default.LhaVis;
            DescriptionVis = Domain.General.Default.DescriptionVis;
            RaVis = Domain.General.Default.RaVis;
            DecVis = Domain.General.Default.DecVis;
            AzVis = Domain.General.Default.AzVis;
            AltVis = Domain.General.Default.AltVis;
            SideVis = Domain.General.Default.SideVis;
            CameraVis = Domain.General.Default.CameraVis;
            RaAxisVis = Domain.General.Default.RaAxisVis;
            DecAxisVis = Domain.General.Default.DecAxisVis;
            SopVis = Domain.General.Default.SopVis;

            XOffset = Domain.General.Default.xAxisOffset;
            YOffset = Domain.General.Default.yAxisOffset;
            ZOffset = Domain.General.Default.zAxisOffset;

            AccentColor = Domain.General.Default.AccentColor;
            AutoConnect = Domain.General.Default.AutoConnect;
            DarkTheme = Domain.General.Default.DarkTheme;
            Language = Domain.General.Default.Language;
            ModelLookDirection = Vector3D.Parse(Domain.General.Default.ModelLookDirection);
            ModelPosition = System.Windows.Media.Media3D.Point3D.Parse(Domain.General.Default.ModelPosition);
            ModelUpDirection = Vector3D.Parse(Domain.General.Default.ModelUpDirection);
            PollTime = Domain.General.Default.PollTime;
            PrimaryColor = Domain.General.Default.PrimaryColor;
            TelescopeID = Domain.General.Default.TelescopeID;
            TopMost = Domain.General.Default.TopMost;
            WindowHeight = Domain.General.Default.WindowHeight;
            WindowWidth = Domain.General.Default.WindowWidth;
            WindowLeft = Domain.General.Default.WindowLeft;
            WindowTop = Domain.General.Default.WindowTop;
            WindowState = Domain.General.Default.WindowState;

            Enum.TryParse<Model3DType>(Domain.General.Default.ModelType, true, out var aparse);
            ModelType = aparse;
        }

        /// <summary>
        /// upgrade and set new version
        /// </summary>
        public static void Upgrade()
        {
            var assembly = Assembly.GetExecutingAssembly().GetName().Version;
            if (Version == assembly?.ToString()) return;
            Domain.General.Default.Upgrade();
            Version = assembly?.ToString();
            Save();
        }

        /// <summary>
        /// save and reload
        /// </summary>
        public static void Save()
        {
            Domain.General.Default.Save();
            Domain.General.Default.Reload();
        }

        /// <summary>
        /// output to session log
        /// </summary>
        /// <param name="method"></param>
        /// <param name="value"></param>
        private static void LogSetting(string method, string value)
        {
            var monitorItem = new MonitorEntry
            { Datetime = DateTime.Now, Device = MonitorDevice.Program, Category = MonitorCategory.Program, Type = MonitorType.Information, Method = $"{method}", Thread = Thread.CurrentThread.ManagedThreadId, Message = $"{value}" };
            Monitor.LogToMonitor(monitorItem);
        }

        /// <summary>
        /// property event notification
        /// </summary>
        /// <param name="propertyName"></param>
        private static void OnStaticPropertyChanged([CallerMemberName] string propertyName = null)
        {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
