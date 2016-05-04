﻿using Android.Util;
using HtmlAgilityPack;
using Java.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

//http://stackoverflow.com/questions/3028306/download-a-file-with-android-and-showing-the-progress-in-a-progressdialog

namespace Tax_Informer
{
    public static class Helper
    {
        public static string DownloadFile(string srcUrl, string desPath)
        {
            Stream input = null;
            FileStream output = null;
            HttpURLConnection connection = null;
            string result = string.Empty;

            try
            {
                URL url = new URL(srcUrl);
                connection = (HttpURLConnection)url.OpenConnection();
                connection.Connect();

                // expect HTTP 200 OK, so we don't mistakenly save error report
                // instead of the file
                if (connection.ResponseCode != HttpStatus.Ok)
                {
                    throw new Exception("Server returned HTTP " + connection.ResponseCode
                            + " " + connection.ResponseMessage);
                }

                // this will be useful to display download percentage
                // might be -1: server did not report the length
                int fileLength = connection.ContentLength;

                // download the file
                input = connection.InputStream;
                output = new FileStream(desPath, FileMode.CreateNew, FileAccess.Write, FileShare.Read);//"/sdcard/file_name.extension"

                byte[] data = new byte[4096];
                long total = 0;
                int count = 1;
                while (count > 0)
                {
                    // allow canceling with back button
                    //if (isCancelled())
                    //{
                    //    input.Close();
                    //    return null;
                    //}
                    Log.Debug("State=Read", "Reading from web");
                    count = input.Read(data, 0, data.Length - 1);   //read data from web

                    total += count;
                    // publishing the progress....
                    //if (fileLength > 0) // only if total length is known
                    //    publishProgress((int)(total * 100 / fileLength));

                    Log.Debug("State=Write", "Writing to disk");
                    output.Write(data, 0, count);

                    Log.Debug("=====", "===============================");
                    Thread.Sleep(10);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                try
                {
                    if (input != null) input.Close();
                    if (output != null) output.Close();
                }
                catch (Exception) { }
                if (connection != null) connection.Disconnect();
            }
            return result;
        }

        public static Stream DownloadFileInMemory(string srcUrl)
        {
            Stream input = null;
            MemoryStream output = null;
            HttpURLConnection connection = null;

            try
            {
                URL url = new URL(srcUrl);
                connection = (HttpURLConnection)url.OpenConnection();
                connection.Connect();

                // expect HTTP 200 OK, so we don't mistakenly save error report
                // instead of the file
                if (connection.ResponseCode != HttpStatus.Ok)
                {
                    throw new Exception("Server returned HTTP " + connection.ResponseCode
                            + " " + connection.ResponseMessage);
                }

                // this will be useful to display download percentage
                // might be -1: server did not report the length
                int fileLength = connection.ContentLength;

                // download the file
                input = connection.InputStream;
                output = new MemoryStream();

                byte[] data = new byte[4096];
                long total = 0;
                int count = 1;
                while (count > 0)
                {
                    // allow canceling with back button
                    //if (isCancelled())
                    //{
                    //    input.Close();
                    //    return null;
                    //}
                    Log.Debug("State=Read", "Reading from web");
                    count = input.Read(data, 0, data.Length - 1);   //read data from web

                    total += count;
                    // publishing the progress....
                    //if (fileLength > 0) // only if total length is known
                    //    publishProgress((int)(total * 100 / fileLength));

                    Log.Debug("State=Write", "Writing to disk");
                    output.Write(data, 0, count);

                    Log.Debug("=====", "===============================");
                    Thread.Sleep(1);
                }
            }
            catch (Exception ex) { throw ex; }
            finally
            {
                try
                {
                    if (input != null) input.Close();
                    //if (output != null) output.Close();
                }
                catch (Exception) { }
                if (connection != null) connection.Disconnect();
            }
            return output;
        }

        public static string DownloadFile(string srcUrl)
        {
            StreamReader input = null;
            HttpURLConnection connection = null;
            string result = string.Empty;            
            try
            {
                URL url = new URL(srcUrl);
                connection = (HttpURLConnection)url.OpenConnection();
                connection.Connect();

                // expect HTTP 200 OK, so we don't mistakenly save error report
                // instead of the file
                if (connection.ResponseCode != HttpStatus.Ok)
                {
                    throw new Exception("Server returned HTTP " + connection.ResponseCode
                            + " " + connection.ResponseMessage);
                }

                // this will be useful to display download percentage
                // might be -1: server did not report the length
                int fileLength = connection.ContentLength;

                // download the file
                input = new StreamReader(connection.InputStream);
                StringBuilder output = new StringBuilder();

                char[] data = new char[4096];
                long total = 0;
                int count = 1;
                while (count > 0)
                {
                    Log.Debug("State=Read", "Reading from web");
                    count = input.Read(data, 0, data.Length);   //read data from web

                    total += count;
                    // publishing the progress....
                    //if (fileLength > 0) // only if total length is known
                    //    publishProgress((int)(total * 100 / fileLength));

                    Log.Debug("State=Write", "Writing to memory");
                    output.Append(data, 0, count);

                    Log.Debug("=====", "===============================");
                    Thread.Sleep(10);
                }
                result = output.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                try
                {
                    if (input != null) input.Close();
                }
                catch (Exception) { }
                if (connection != null) connection.Disconnect();
            }
            return result;
        }
        

        public static bool DumpDataToFile(string data, string desPath)
        {
            StreamWriter writer = null;
            bool result = true;
            try
            {
                if (File.Exists(desPath)) File.Delete(desPath);
                writer = new StreamWriter(desPath, false);
                writer.Write(data);
                result = true;
            }
            catch (Exception)
            {
                result = false;
            }
            finally
            {
                if (writer != null) writer.Close();
            }
            return result;
        }

        #region Core

        public static HtmlNode AnyChild(HtmlNode parent, string name, Dictionary<string, string> attributeValues, bool ignoreCase = true)
        {
            if (parent == null) return null;

            if (!parent.HasChildNodes) return null;

            foreach (var item in parent.ChildNodes)
            {
                if (ignoreCase)
                {
                    if (item.Name.ToLower() == name.ToLower())
                        if (isAttributesSame(item, attributeValues, ignoreCase))//confirm attributes
                            return item;
                }
                else
                {
                    if (item.Name == name)
                        if (isAttributesSame(item, attributeValues, ignoreCase))//confirm attributes
                            return item;
                }
                if (item.HasChildNodes)
                {
                    var result = AnyChild(item, name, attributeValues, ignoreCase);//call inner level
                    if (result != null) return result;//filter inner level call result
                }
            }
            return null;
        }

        public static List<HtmlNode> AllChild(HtmlNode parent, string name, Dictionary<string, string> attributeValues, bool ignoreCase = true)
        {
            if (parent == null) return null;
            if (!parent.HasChildNodes) return null;

            List<HtmlNode> list = new List<HtmlNode>();

            foreach (var item in parent.ChildNodes)
            {
                if (ignoreCase)
                {
                    if (item.Name.ToLower() == name.ToLower())
                        if (isAttributesSame(item, attributeValues, ignoreCase))//confirm attributes
                            list.Add(item);
                }
                else
                {
                    if (item.Name == name)
                        if (isAttributesSame(item, attributeValues, ignoreCase))//confirm attributes
                            list.Add(item);
                }
                if (item.HasChildNodes)
                {
                    var result = AllChild(item, name, attributeValues, ignoreCase);//call inner level
                    if (result != null) list.AddRange(result);//filter inner level call result
                }
            }
            if (list.Count > 0) return list;
            return null;
        }

        private static bool isAttributesSame(HtmlNode node, Dictionary<string, string> attributeValues, bool ignoreCase = true)
        {
            try
            {
                if (attributeValues == null) return true;

                if (!node.HasAttributes) return false;

                foreach (var request in attributeValues)
                {
                    if (ignoreCase)
                    {
                        if (node.Attributes[request.Key].Value.ToLower() != request.Value.ToLower())
                            return false;
                    }
                    else
                    {
                        if (node.Attributes[request.Key].Value != request.Value)
                            return false;
                    }
                }
                return true;
            }
            catch (Exception) { return false; }
        }

        #endregion Core

        public static HtmlNode AnyChild(HtmlNode parent, string name)
        {
            return AnyChild(parent, name, null, true);
        }

        public static HtmlNode AnyChild(HtmlNode parent, string name, string className)
        {
            Dictionary<string, string> attritubeValues = new Dictionary<string, string>();
            attritubeValues.Add("class", className);
            return AnyChild(parent, name, attritubeValues, true);
        }

        public static List<HtmlNode> AllChild(HtmlNode parent, string name)
        {
            return AllChild(parent, name, null, true);
        }

        public static List<HtmlNode> AllChild(HtmlNode parent, string name, string className)
        {
            Dictionary<string, string> attritubeValues = new Dictionary<string, string>();
            attritubeValues.Add("class", className);
            return AllChild(parent, name, attritubeValues, true);
        }

        public static string TrimToEntry(string st)
        {
            int i = 0;
            foreach (var s in st)
            {
                if (s == '<') break;
                i++;
            }
            return st.Substring(i);
        }
    }
    public class SearchAttributeBuilder
    {
        private Dictionary<string, string> att = new Dictionary<string, string>();

        public Dictionary<string, string> Build() => att;
        public SearchAttributeBuilder Add(string attribute, string value)
        {
            att.Add(attribute, value);
            return this;
        }

        public static SearchAttributeBuilder CreateNew() => new SearchAttributeBuilder();
    }
}