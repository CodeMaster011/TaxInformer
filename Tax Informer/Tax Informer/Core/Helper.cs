using Android.Util;
using HtmlAgilityPack;
using Java.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

//http://stackoverflow.com/questions/3028306/download-a-file-with-android-and-showing-the-progress-in-a-progressdialog

namespace Tax_Informer
{
    public static class Helper
    {
        public static bool DownloadFile(string srcUrl, string desPath)
        {
            Stream input = null;
            FileStream output = null;
            HttpURLConnection connection = null;
            bool result = true;

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
                    count = input.Read(data, 0, data.Length);   //read data from web

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
                result = false;
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
                    count = input.Read(data, 0, data.Length);   //read data from web

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

        public static string DownloadFileWithPostRequest(string url, string postParameters, string userAgent = "chrome")
        {
            try
            {
                var stream = DownloadFileWithPostRequestInMemory(url, postParameters, userAgent);
                StreamReader responseReader = new StreamReader(stream);
                string fullResponse = responseReader.ReadToEnd();
                stream.Close();

                return fullResponse;
            }
            catch (Exception ex)
            {                
                throw ex;
            }            
        }
        public static Stream DownloadFileWithPostRequestInMemory(string url, string postParameters, string userAgent = "chrome")
        {
            try
            {
                Dictionary<string, object> _postParameters = new Dictionary<string, object>();//TODO: Convert the string to parameters

                HttpWebResponse webResponse = FormUpload.MultipartFormDataPost(url, userAgent, _postParameters);

                return webResponse.GetResponseStream();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string CreatePostRequest(string postUrl, Dictionary<string, string> parmeters, string contentType = "multipart/form-data;", string userAgent = "chrome")
        {
            string paramData = "";
            foreach (var p in parmeters)            
                paramData += $"{p.Key}={p.Value}&";
            paramData = paramData.Substring(0, paramData.Length - 1);

            return StringSequentialSerilizer("post", postUrl, contentType, userAgent, paramData);
        }

        public static string ConvertDirectoryToString(Dictionary<string,object> directory)
        {
            string paramData = "";
            foreach (var p in directory)
                paramData += $"{p.Key}={p.Value}&";
            return paramData.Substring(0, paramData.Length - 1);
        }

        public static Dictionary<string, object> ConvertStringToDirectory(string value)
        {
            var ss = value.Split(new string[] { "&" }, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, object> result = new Dictionary<string, object>();
            foreach (var item in ss)
            {
                var ddd = item.Split('=');
                result.Add(ddd[0], ddd[1]);
            }
            return result.Keys.Count > 0 ? result : null;
        }
        public static string StringSequentialSerilizer(params string[] contents)
        {
            string result = "";
            foreach (var item in contents)
                result += $"{item.Length.ToString("000")}{item}";
            return result;
        }

        public static string[] StringSequentialDeserilizer(string serilizedData)
        {
            List<string> result = new List<string>();
            for (int i = 0; i < serilizedData.Length; i++)
            {
                var dataLength = int.Parse(serilizedData.Substring(i, 3));
                var data = serilizedData.Substring(i + 3, dataLength);
                result.Add(data);

                i += 3 + dataLength - 1;                
            }
            return result.Count > 0 ? result.ToArray() : null;
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

        public static List<HtmlNode> AllChild(HtmlNode parent, string name, SearchCritria critria)
        {
            var fullResult = AllChild(parent, name);
            if (fullResult == null) return null;

            var finalResult = new List<HtmlNode>();

            foreach (var item in fullResult)
            {
                if (isAttributesSame(item, critria.HasAttributeList))
                {
                    if (critria.NotHasAttributeList == null || !isAttributesSame(item, critria.NotHasAttributeList))
                    {
                        bool isOk = true;
                        if (critria.HasChildren != null)
                        {
                            foreach (var includedItem in critria.HasChildren)
                            {
                                if (AnyChild(item, includedItem.Name, includedItem.Attribute) == null)
                                {
                                    isOk = false;
                                    break;
                                }
                            }
                        }                        
                        if (isOk)
                        {
                            isOk = true;
                            if (critria.NotHasChildren != null)
                            {
                                foreach (var notIncludedItem in critria.NotHasChildren)
                                {
                                    if (AnyChild(item, notIncludedItem.Name, notIncludedItem.Attribute) != null)
                                    {
                                        isOk = false;
                                        break;
                                    }
                                }
                            }                            
                        }
                        if (isOk)
                        {
                            finalResult.Add(item);
                        }
                    }
                }
            }

            return finalResult;
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

        public static string[] monthArray = new string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

        public static int GetMonthIndex(string month)
        {
            string abb = month.Substring(0, 3).ToLower();
            for (int i = 0; i < monthArray.Length; i++)
            {
                if (abb == monthArray[i].ToLower()) return i + 1;
            }
            return 0;
        }

        public static string CombindUrl(string baseUrl, string other)
        {
            if (other.StartsWith("http://") || other.StartsWith("https://")) return other;
            if (baseUrl != null && !baseUrl.EndsWith("/")) baseUrl += "/";
            return $"{baseUrl ?? ""}{other ?? ""}";
        }
    }

    public static class FormUpload
    {
        private static readonly Encoding encoding = Encoding.UTF8;
        public static HttpWebResponse MultipartFormDataPost(string postUrl, string userAgent, Dictionary<string, object> postParameters)
        {
            string formDataBoundary = String.Format("----------{0:N}", Guid.NewGuid());
            string contentType = "multipart/form-data; boundary=" + formDataBoundary;

            byte[] formData = GetMultipartFormData(postParameters, formDataBoundary);

            return PostForm(postUrl, userAgent, contentType, formData);
        }
        private static HttpWebResponse PostForm(string postUrl, string userAgent, string contentType, byte[] formData)
        {
            HttpWebRequest request = WebRequest.Create(postUrl) as HttpWebRequest;

            if (request == null)
            {
                throw new NullReferenceException("request is not a http request");
            }

            // Set up the request properties.
            request.Method = "POST";
            request.ContentType = contentType;
            request.UserAgent = userAgent;
            request.CookieContainer = new CookieContainer();
            request.ContentLength = formData.Length;

            // You could add authentication here as well if needed:
            // request.PreAuthenticate = true;
            // request.AuthenticationLevel = System.Net.Security.AuthenticationLevel.MutualAuthRequested;
            // request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.Default.GetBytes("username" + ":" + "password")));

            // Send the form data to the request.
            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(formData, 0, formData.Length);
                requestStream.Close();
            }

            return request.GetResponse() as HttpWebResponse;
        }

        private static byte[] GetMultipartFormData(Dictionary<string, object> postParameters, string boundary)
        {
            Stream formDataStream = new System.IO.MemoryStream();
            bool needsCLRF = false;

            foreach (var param in postParameters)
            {
                // Thanks to feedback from commenters, add a CRLF to allow multiple parameters to be added.
                // Skip it on the first parameter, add it to subsequent parameters.
                if (needsCLRF)
                    formDataStream.Write(encoding.GetBytes("\r\n"), 0, encoding.GetByteCount("\r\n"));

                needsCLRF = true;

                if (param.Value is FileParameter)
                {
                    FileParameter fileToUpload = (FileParameter)param.Value;

                    // Add just the first part of this param, since we will write the file data directly to the Stream
                    string header = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: {3}\r\n\r\n",
                        boundary,
                        param.Key,
                        fileToUpload.FileName ?? param.Key,
                        fileToUpload.ContentType ?? "application/octet-stream");

                    formDataStream.Write(encoding.GetBytes(header), 0, encoding.GetByteCount(header));

                    // Write the file data directly to the Stream, rather than serializing it to a string.
                    formDataStream.Write(fileToUpload.File, 0, fileToUpload.File.Length);
                }
                else
                {
                    string postData = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}",
                        boundary,
                        param.Key,
                        param.Value);
                    formDataStream.Write(encoding.GetBytes(postData), 0, encoding.GetByteCount(postData));
                }
            }

            // Add the end of the request.  Start with a newline
            string footer = "\r\n--" + boundary + "--\r\n";
            formDataStream.Write(encoding.GetBytes(footer), 0, encoding.GetByteCount(footer));

            // Dump the Stream into a byte[]
            formDataStream.Position = 0;
            byte[] formData = new byte[formDataStream.Length];
            formDataStream.Read(formData, 0, formData.Length);
            formDataStream.Close();

            return formData;
        }

        public class FileParameter
        {
            public byte[] File { get; set; }
            public string FileName { get; set; }
            public string ContentType { get; set; }
            public FileParameter(byte[] file) : this(file, null) { }
            public FileParameter(byte[] file, string filename) : this(file, filename, null) { }
            public FileParameter(byte[] file, string filename, string contenttype)
            {
                File = file;
                FileName = filename;
                ContentType = contenttype;
            }
        }
    }

    public class SearchCritriaBuilder
    {
        private SearchCritria critria = new SearchCritria();

        public SearchCritria Build() => critria;
        public SearchCritriaBuilder AddHasAttribute(string attribute, string value)
        {
            if (critria.HasAttributeList == null) critria.HasAttributeList = new Dictionary<string, string>();
            critria.HasAttributeList.Add(attribute, value);
            return this;
        }
        public SearchCritriaBuilder AddNotHasAttribute(string attribute, string value)
        {
            if (critria.NotHasAttributeList == null) critria.NotHasAttributeList = new Dictionary<string, string>();
            critria.NotHasAttributeList.Add(attribute, value);
            return this;
        }
        public SearchCritriaBuilder AddHasChild(ChildNode child)
        {
            if (critria.HasChildren == null) critria.HasChildren = new List<ChildNode>();
            critria.HasChildren.Add(child);
            return this;
        }
        public SearchCritriaBuilder AddNotHasChild(ChildNode child)
        {
            if (critria.NotHasChildren == null) critria.NotHasChildren = new List<ChildNode>();
            critria.NotHasChildren.Add(child);
            return this;
        }

        public static SearchCritriaBuilder CreateNew() => new SearchCritriaBuilder();
    }
    public sealed class SearchCritria
    {
        public Dictionary<string, string> HasAttributeList = null;
        public Dictionary<string, string> NotHasAttributeList = null;
        public List<ChildNode> HasChildren = null;
        public List<ChildNode> NotHasChildren = null;
    }
    public sealed class ChildNode
    {
        public string Name { get; set; } = null;
        public Dictionary<string, string> Attribute { get; set; } = null;
    }

    public class PostRequestBuilder
    {
        public string PostUrl { get; protected set; } = null;
        public Dictionary<string, object> Params { get; protected set; } = null;
        public string ContentType { get; protected set; } = "multipart/form-data;";
        public string UserAgent { get; protected set; } = "chrome";

        private PostRequestBuilder() { }

        public static PostRequestBuilder New(string postUrl) => new PostRequestBuilder() { PostUrl = postUrl };

        
        public PostRequestBuilder AddPrams(string key,string value)
        {
            Params.Add(key, value);
            return this;
        }
        public PostRequestBuilder SetContentType(string contentType)
        {
            this.ContentType = contentType;
            return this;
        }
        public PostRequestBuilder SetUserAgent(string userAgent)
        {
            this.UserAgent = userAgent;
            return this;
        }
        public string Build()
        {
            var paramString = Helper.ConvertDirectoryToString(Params);
            return Helper.StringSequentialSerilizer("post", PostUrl, ContentType, UserAgent, paramString);
        }
        public static PostRequestBuilder FromSerilizedData(string serilizedData)
        {
            var data = Helper.StringSequentialDeserilizer(serilizedData);
            var parms = Helper.ConvertStringToDirectory(data[4]);
            return new PostRequestBuilder()
            {
                Params = parms,
                PostUrl = data[1],
                ContentType = data[2],
                UserAgent = data[3]
            };
        }
    }
}