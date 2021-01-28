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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Markup;

namespace GS.Point3D.Helpers
{
    public static class Languages
    {
        private static readonly string _directoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\LanguageFiles\\";

        static Languages()
        {
            var langs = new List<string>() { "en-US", "fr-FR" };
            SupportedLanguages = langs;
        }
        public static void SetLanguageDictionary(bool local, LanguageApp app)
        {
            if (local)
            {
                SetLanguageDictionary(Thread.CurrentThread.CurrentCulture.ToString(), app);
            }
            else
            {
                var lang = Domain.General.Default.Language;
                SetLanguageDictionary(DoesCultureExist(lang) ? lang : "en-US", app);
            }
        }
        private static bool DoesCultureExist(string cultureName)
        {
            return CultureInfo.GetCultures(CultureTypes.AllCultures).Any(culture => string.Equals(culture.Name, cultureName, StringComparison.CurrentCultureIgnoreCase));
        }
        private static void SetLanguageDictionary(string cultureInfo, LanguageApp app)
        {
            var dict = new ResourceDictionary();
            Uri uri = null;
            var localpath = string.Empty;
            var filenotfound = false;
            switch (app)
            {
                case LanguageApp.GSPoint3D:
                    switch (cultureInfo)
                    {
                        case "en-US":
                            uri = new Uri("GS.Point3D;component/Domain/StringResPoint3D_en-US.xaml", UriKind.Relative);
                            break;
                        default:
                            localpath = new Uri(Path.Combine(_directoryPath, $"{Application.Current.Resources["FileName"]}_{cultureInfo}.xaml")).LocalPath;
                            if (!File.Exists(localpath))
                            {
                                filenotfound = true;
                                uri = new Uri("GS.Point3D;component/Domain/StringResPoint3D_en-US.xaml", UriKind.Relative);
                            }
                            break;
                    }

                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(app), app, null);
            }

            switch (cultureInfo)
            {
                case "en-US":
                    dict.Source = uri;
                    if (dict.Source == null) { throw new Exception("Language source missing"); }
                    Application.Current.Resources.MergedDictionaries.Add(dict);
                    break;
                default:
                    if (filenotfound)
                    {
                        dict.Source = uri;
                        if (dict.Source == null)
                        {
                            throw new Exception("Language source missing");
                        }
                        Application.Current.Resources.MergedDictionaries.Add(dict);
                    }
                    else
                    {
                        using (var fs = new FileStream(localpath, FileMode.Open, FileAccess.Read))
                        {
                            var dic = (ResourceDictionary)XamlReader.Load(fs);
                            //Resources.MergedDictionaries.Clear();
                            Application.Current.Resources.MergedDictionaries.Add(dic);
                        }
                    }
                    break;
            }
        }
        public static List<string> SupportedLanguages { get; }
        public static string Language
        {
            get => GeneralSettings.Language;
            set => GeneralSettings.Language = DoesCultureExist(value) ? value : "en-US";
        }
    }

    public enum LanguageApp
    {
        GSPoint3D = 1,
    }
}
