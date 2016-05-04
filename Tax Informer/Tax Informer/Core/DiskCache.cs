using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using System.IO;

namespace Tax_Informer.Core
{
    interface ICache
    {
        long CacheSize { get; }
        Bitmap GetBitmap(string url);
        string GetString(string url);
        bool Put(string url, Bitmap value, bool update = false);
        bool Put(string url, string value, bool update = false);
        bool IsKeyExist(string url);
        bool IsKeyExist(string url, out Bitmap value);
        bool IsKeyExist(string url, out string value);
        bool Remove(string url);
    }
    //TODO: Run DiskCache is different thread to get boost in IOfflineModule
    class DiskCache : ICache
    {

        public string CachePhysicalLocation { get; }
        public long CacheSize { get; }

        public Bitmap GetBitmap(string url)
        {
            try
            {
                string path = CachePhysicalLocation + encodeUrl(url);
                if (!File.Exists(path)) return null;
                //BitmapFactory.Options options = new BitmapFactory.Options();
                //options.inPreferredConfig = Bitmap.Config.ARGB_8888;
                //Bitmap bitmap = BitmapFactory.decodeFile(photoPath, options);
                return BitmapFactory.DecodeFile(path);
            }
            catch (Exception)
            {
                return null;
            }            
        }

        public string GetString(string url)
        {
            StreamReader fStream = null;
            string result = string.Empty;
            try
            {
                string path = CachePhysicalLocation + encodeUrl(url);
                if (!File.Exists(path)) return string.Empty;

                fStream = new StreamReader(path);
                result = fStream.ReadToEnd();
            }
            catch (Exception) { }
            finally
            {
                fStream?.Close();
            }
            return result;
        }

        public bool IsKeyExist(string url)
        {
            string path = CachePhysicalLocation + encodeUrl(url);
            return File.Exists(path);
        }

        public bool IsKeyExist(string url, out string value)
        {
            if (IsKeyExist(url))
            {
                value = GetString(url);
                return true;
            }
            else
            {
                value = string.Empty;
                return false;
            }
        }

        public bool IsKeyExist(string url, out Bitmap value)
        {
            if (IsKeyExist(url))
            {
                value = GetBitmap(url);
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        public bool Put(string url, string value, bool update = false)
        {
            StreamWriter fStream = null;
            bool result = true;
            try
            {
                string path = CachePhysicalLocation + encodeUrl(url);
                if (File.Exists(path))
                {
                    if (update) File.Delete(path);
                    else return false;
                }

                fStream = new StreamWriter(path, false);
                fStream.Write(value);
                fStream.Flush();
            }
            catch (Exception)
            {
                result = false;
            }
            finally
            {
                fStream?.Close();
            }
            return result;
        }

        public bool Put(string url, Bitmap value, bool update = false)
        {
            FileStream fStream = null;
            bool result = true;
            try
            {
                string path = CachePhysicalLocation + encodeUrl(url);
                if (File.Exists(path))
                {
                    if (update) File.Delete(path);
                    else return false;
                }

                fStream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
                value.Compress(Bitmap.CompressFormat.Jpeg, 100, fStream);
                fStream.Flush();
            }
            catch (Exception)
            {
                result = false;
            }
            finally
            {
                fStream?.Close();
            }
            return result;
        }

        public DiskCache(string CachePhysicalLocation, long CachePhysicalSize)
        {
            this.CachePhysicalLocation = CachePhysicalLocation.EndsWith("/") ? CachePhysicalLocation : (CachePhysicalLocation + "/");
            this.CacheSize = CachePhysicalSize;

            if (!Directory.Exists(CachePhysicalLocation)) Directory.CreateDirectory(CachePhysicalLocation);
        }

        private string encodeUrl(string key) => key.Replace('/', '-').Replace(':','-');

        public bool Remove(string url)
        {
            try
            {
                var path = CachePhysicalLocation + encodeUrl(url);
                if (File.Exists(path)) File.Delete(url);
                else return false;
                return true;
            }
            catch (Exception)
            {
                return false;
            }            
        }
    }
}