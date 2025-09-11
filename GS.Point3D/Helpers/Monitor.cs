/* Copyright(C) 2025  Rob Morgan (robert.morgan.e@gmail.com)

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
using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using GS.Point3D.Settings;

namespace GS.Point3D.Helpers
{
    public static class Monitor
    {
        #region Fields
        public static event PropertyChangedEventHandler StaticPropertyChanged;
        private static readonly BlockingCollection<MonitorEntry> MonitorBlockingCollection;
        private static int _sessionIndex;
        private static readonly string InstanceFileName;
        private static readonly string LogPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private static readonly string Point3DFile = Path.Combine(LogPath, "GSPoint3D\\GSPoint3DLog");
        private static readonly SemaphoreSlim LockFile = new SemaphoreSlim(1);
        private const string Fmt = "000000#";
        #endregion

        static Monitor()
        {
            InstanceFileName = $"{DateTime.Now:yyyy-MM-dd-HH}.txt";
            DeleteFiles("GSPoint3DLog", 7, LogPath);

            MonitorBlockingCollection = new BlockingCollection<MonitorEntry>();
            Task.Factory.StartNew(() =>
            {
                foreach (var monitorEntry in MonitorBlockingCollection.GetConsumingEnumerable())
                {
                    ProcessEntryQueueItem(monitorEntry);
                }
            });
        }

        #region Properties

        // UI indicator for warnings
        private static bool _warningState;
        public static bool WarningState
        {
            get => _warningState;
            set
            {
                if (_warningState == value) return;
                _warningState = value;
                OnStaticPropertyChanged();
                FlipOffWarningState();
            }
        }

        private static async void FlipOffWarningState()
        {
            if (!WarningState) return;
            await Task.Delay(100);
            WarningState = false;
        }

        #endregion
        

        #region Methods

        /// <summary>
        /// Send a MonitorEntry to the queue to be processed
        /// </summary>
        public static void LogToMonitor(MonitorEntry entry)
        {
            entry.Message = entry.Message.Trim();
            entry.Method = entry.Method.Trim();

            // don't add to queue if not needed
            switch (entry.Type)
            {
                case MonitorType.Warning:
                case MonitorType.Error:
                case MonitorType.Information:
                case MonitorType.Data:
                    AddEntry(entry);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// reset the count from the UI
        /// </summary>
        public static void ResetMonitorIndex()
        {
            _sessionIndex = 0;
        }

        /// <summary>
        /// adds a monitor item to a blocking queue
        /// </summary>
        /// <param name="entry"></param>
        public static void AddEntry(MonitorEntry entry)
        {
             MonitorBlockingCollection.TryAdd(entry);
        }

        /// <summary>
        /// trigger the property event for the UI to pick up the property
        /// </summary>
        /// <param name="propertyName"></param>
        private static void OnStaticPropertyChanged([CallerMemberName] string propertyName = null)
        {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Process queue monitor items to the appropriate logs
        /// </summary>
        /// <param name="entry"></param>
        private static void ProcessEntryQueueItem(MonitorEntry entry)
        {
            WriteOutSession(entry);
        }

        /// <summary>
        /// Writes out the monitor type Information to the session log
        /// </summary>
        /// <param name="entry"></param>
        private static void WriteOutSession(MonitorEntry entry)
        {
            try
            {
                if (!GeneralSettings.Log) return;
                ++_sessionIndex;
                FileWriteAsync(Point3DFile + InstanceFileName, $"{entry.Datetime.ToLocalTime():yyyy-MM-dd HH:mm:ss.fff},{_sessionIndex.ToString(Fmt)},{entry.Category},{entry.Type},{entry.Thread},{entry.Method},{entry.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                //throw;
            }
        }
        
        /// <summary>
        /// Send monitor entries to a file async
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="message"></param>
        /// <param name="append"></param>
        private static async void FileWriteAsync(string filePath, string message, bool append = true)
        {
            try
            {
                await LockFile.WaitAsync();

                Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? throw new InvalidOperationException());
                using (var stream = new FileStream(filePath, append ? FileMode.Append : FileMode.Create,
                    FileAccess.Write, FileShare.None, 4096, true))
                using (var sw = new StreamWriter(stream))
                {
                    await sw.WriteLineAsync(message);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                //throw;
            }
            finally
            {
                LockFile.Release();
            }
        }

        /// <summary>
        /// Deletes files by name, how old, and dir path
        /// </summary>
        /// <param name="name"></param>
        /// <param name="daysOld"></param>
        /// <param name="path"></param>
        private static void DeleteFiles(string name, int daysOld, string path)
        {
            try
            {
                var files = Directory.GetFiles(path);
                foreach (var file in files)
                {
                    var fi = new FileInfo(file);
                    if (fi.Name.Contains(name) && fi.CreationTime < (DateTime.Now - new TimeSpan(daysOld, 0, 0, 0))) fi.Delete();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                //throw;
            }
        }

        #endregion
    }

    public class MonitorEntry
    {
        public DateTime Datetime { get; set; }
        public int Index { get; set; }
        public MonitorDevice Device { get; set; }
        public MonitorCategory Category { get; set; }
        public MonitorType Type { get; set; }
        public string Method { get; set; }
        public int Thread { get; set; }
        public string Message { get; set; }
    }

    #region Enums

    /// <summary>
    /// List of Driver Devices 
    /// </summary>
    public enum MonitorDevice
    {
        Program,
        Telescope
    }

    /// <summary>
    /// Levels of monitor entries
    /// </summary>
    public enum MonitorType
    {
        Information,
        Data,
        Warning,
        Error
    }

    /// <summary>
    /// used to define where or what process monitor items are being logged
    /// </summary>
    public enum MonitorCategory
    {
        Other,
        Driver,
        Interface,
        Program,
        Mount,
        Notes
    }
    #endregion
}
