using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace GmailShot
{
    public static class Utilities
    {
        //these are the keys for the app settings
        public const string USERNAME_CONFIG_KEY = "username";
        public const string PASSWORD_CONFIG_KEY = "pass";

        /// <summary>
        /// Safely retrieve an appsetting by key, returning a default value if it doesn't exist
        /// </summary>
        /// <returns></returns>
        public static string GetAppSetting(string key, string defaultValue = "")
        {
            if (ConfigurationManager.AppSettings.AllKeys.Contains(key))
            {
                return ConfigurationManager.AppSettings[key];
            }
            return defaultValue;
        }

        /// <summary>
        /// Save a bitmap to the file system; returns the full path to which it was saved
        /// </summary>
        public static string SaveBitmapToFile(string filenameNoExtension, Bitmap bmp)
        {
            //temp path seems a reasonable place to save these
            string path = Path.GetTempPath();
            string filenameRoot = filenameNoExtension;
            string extension = ".bmp";
            int i = 0;
            string tempFilename = filenameRoot;

            //this is to save as "xxx1.bmp" if "xxx.bmp" already exists, etc
            while (File.Exists(path + @"\" + tempFilename + extension))
            {
                if (i == 0)
                {
                    tempFilename += "1";
                }
                else
                {
                    tempFilename = filenameRoot + (i);
                }
                i++;
            }

            string fullpath = path + @"\" + tempFilename + extension;
            bmp.Save(fullpath, ImageFormat.Bmp);
            return fullpath;
        }
    }
}
