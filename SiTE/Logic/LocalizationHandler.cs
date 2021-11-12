using System;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace SiTE.Logic
{
    public class LocalizationHandler
    {
        private static void SetLocalization(CultureInfo cultureInfo)
        {
            string filePath = Refs.dataBank.DefaultLanguagePath;

            var dict = new ResourceDictionary
            {
                Source = new Uri(string.Format("pack://{0}{1}.xaml", filePath, cultureInfo.Name))
            };

            var existingDict = Application.Current.Resources.MergedDictionaries.FirstOrDefault(
                rd => rd.Source.OriginalString.StartsWith("pack://" + filePath));

            if (existingDict != null)
            {
                Application.Current.Resources.MergedDictionaries.Remove(existingDict);
            }

            Application.Current.Resources.MergedDictionaries.Add(dict);
        }

        public void SwitchLanguage(string cultureCode)
        {
            CultureInfo cultureInfo = CultureInfo.GetCultureInfo(cultureCode);
            SetLocalization(cultureInfo);
        }
    }
}
