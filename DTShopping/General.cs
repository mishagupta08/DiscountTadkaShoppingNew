using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;

using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace DTShopping
{
    public class General
    {
        public string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
        public string GetIPAddress()
        {
            System.Web.HttpContext context = System.Web.HttpContext.Current;
            string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    return addresses[0];
                }
            }

            return context.Request.ServerVariables["REMOTE_ADDR"];
        }
        public DataSet getdataset(string jsonString)
        {
            DataSet ds = new DataSet();
            try
            {

                XmlDocument xd = new XmlDocument();
                jsonString = ("{ \"rootNode\": {"
                            + (jsonString.Trim().TrimStart('{').TrimEnd('}') + "} }"));
                xd = ((XmlDocument)(JsonConvert.DeserializeXmlNode(jsonString)));

                ds.ReadXml(new XmlNodeReader(xd));
                return ds;
            }
            catch (Exception ex)
            {
            }
            //ds.ReadXml(XmlReader.Create(new StringReader(jsonString)));
            return ds;
        }
        public DataSet getdatasetXML(string jsonString)
        {
            DataSet ds = new DataSet();
            ds.ReadXml(XmlReader.Create(new StringReader(jsonString)));
            return ds;
        }
        public void ErrorLog(string sPathName, string sErrMsg)
        {
            string sLogFormat = (DateTime.Now.ToShortDateString().ToString() + (" "
                        + (DateTime.Now.ToLongTimeString().ToString() + " ==> ")));
            string sYear = DateTime.Now.Year.ToString();
            string sMonth = DateTime.Now.Month.ToString();
            string sDay = DateTime.Now.Day.ToString();
            string sErrorTime = (sYear
                        + (sMonth + sDay));
            StreamWriter sw = new StreamWriter((sPathName + sErrorTime), true);
            sw.WriteLine((sLogFormat + sErrMsg));
            sw.Flush();
            sw.Close();
        }
        public DataSet convertJsonStringToDataSet(string jsonString)
        {
            XmlDocument xd = new XmlDocument();
            jsonString = "{ \"rootNode\": {" + jsonString.Trim().TrimStart('{').TrimEnd('}') + "} }";
            xd = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonString);
            DataSet ds = new DataSet();
            ds.ReadXml(new XmlNodeReader(xd));
            return ds;
        }
        public DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Defining type of data column gives proper data table 
                var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name, type);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }
        public HttpWebRequest PostJSON(string JsonData, string URL)
        {
            HttpWebRequest objhttpWebRequest;
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(URL);
                httpWebRequest.ContentType = "text/json";

                httpWebRequest.Method = "POST";
                httpWebRequest.Timeout = 300000;
                httpWebRequest.ServicePoint.Expect100Continue = false;
                //                ServicePointManager.ServerCertificateValidationCallback = new
                //RemoteCertificateValidationCallback
                //(
                //   delegate { return true; }
                //);
                //httpWebRequest.KeepAlive = false;
                using (var streamWriter = new System.IO.StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(JsonData);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                objhttpWebRequest = httpWebRequest;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Send Request Error[{0}]", ex.Message);
                return null/* TODO Change to default(_) if this is not a reference type */;
            }

            return objhttpWebRequest;
        }


        public HttpWebRequest RechargePostJSON(string JsonData, string URL)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HttpWebRequest objhttpWebRequest;
            try
            {


                var httpWebRequest = (HttpWebRequest)WebRequest.Create(URL);

                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Accept = "application/json";
                httpWebRequest.Method = "POST";
                httpWebRequest.Timeout = 300000;
                httpWebRequest.ServicePoint.Expect100Continue = false;
                //                ServicePointManager.ServerCertificateValidationCallback = new
                //RemoteCertificateValidationCallback
                //(
                //   delegate { return true; }
                //);
                //httpWebRequest.KeepAlive = false;
                using (var streamWriter = new System.IO.StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(JsonData);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                objhttpWebRequest = httpWebRequest;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Send Request Error[{0}]", ex.Message);
                return null/* TODO Change to default(_) if this is not a reference type */;
            }

            return objhttpWebRequest;
        }


        public string invokeGetRequest(string requestUrl)
        {
            string completeUrl = requestUrl;
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                HttpWebRequest request1 = WebRequest.Create(completeUrl) as HttpWebRequest;
                request1.ContentType = @"application/xml";
                request1.Method = @"GET";
                // header = formHeader(completeUrl, "GET");
                //request1.Headers.Add(HttpRequestHeader.Authorization, header);
                //request1.Timeout = (4 * 60 * 1000);
                HttpWebResponse httpWebResponse = (HttpWebResponse)request1.GetResponse();
                StreamReader reader = new
                StreamReader(httpWebResponse.GetResponseStream());
                string responseString = reader.ReadToEnd();
                return responseString;
            }
            catch (WebException ex)
            {
                //reading the custom messages sent by the server
                using (var reader = new StreamReader(ex.Response.GetResponseStream()))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                return "Failed with exception message:" + ex.Message;
            }

        }
        public HttpWebRequest JSON(string JsonData, string URL)
        {
            HttpWebRequest objhttpWebRequest;
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(URL);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Accept = "application/json";
                httpWebRequest.Method = "POST";
                httpWebRequest.Timeout = 300000;
                //httpWebRequest.KeepAlive = false;
                using (var streamWriter = new System.IO.StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(JsonData);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                objhttpWebRequest = httpWebRequest;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Send Request Error[{0}]", ex.Message);
                return null/* TODO Change to default(_) if this is not a reference type */;
            }

            return objhttpWebRequest;
        }
        public string GetResponse(HttpWebRequest httpWebRequest)
        {
            string strResponse = "Bad Request:400";
            try
            {
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                using (var streamReader = new System.IO.StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    strResponse = result.ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetResponse Error[{0}]", ex.Message);

                return ex.Message;
            }

            return strResponse;
        }
        public XmlNode GetResponseXML(HttpWebRequest httpWebRequest)
        {
            string strResponse = "Bad Request:400";
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                xmlDoc.Load(httpResponse.GetResponseStream());
                //return xmlDoc.DocumentElement.SelectSingleNode("results");
                //using (var streamReader = new System.IO.StreamReader(httpResponse.GetResponseStream()))
                //{
                //    var result = streamReader.ReadToEnd();
                //    strResponse = result.ToString();
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetResponse Error[{0}]", ex.Message);

                //return ex.Message;
            }

            return xmlDoc.DocumentElement.SelectSingleNode("results"); ;
        }
        //public string GetResponse(string requestData, string url)
        //{
        //    string responseXML = string.Empty;
        //    try
        //    {
        //        byte[] data = Encoding.UTF8.GetBytes(requestData);
        //        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        //        request.Method = "POST";
        //        request.ContentType = "application/json";
        //        //request.Headers.Add("Accept-Encoding", "gzip");
        //        Stream dataStream = request.GetRequestStream();
        //        dataStream.Write(data, 0, data.Length);
        //        dataStream.Close();
        //        WebResponse webResponse = request.GetResponse();
        //        var rsp = webResponse.GetResponseStream();
        //        //if (rsp == null)
        //        //{
        //        //    //throw exception
        //        //}
        //        //using (StreamReader readStream = new StreamReader(new GZipStream(rsp, CompressionMode.Decompress)))
        //        //{
        //        //    responseXML = JsonConvert.DeserializeXmlNode(readStream.ReadToEnd()).InnerXml;
        //        //}
        //        return responseXML;
        //    }
        //    catch (WebException webEx)
        //    {
        //        WebResponse response = webEx.Response;
        //        Stream stream = response.GetResponseStream();
        //        String responseMessage = new StreamReader(stream).ReadToEnd();
        //    }
        //    return responseXML;
        //}
        public string GetResponse(string requestData, string url)
        {
            string responseXML = string.Empty;
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(requestData);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/json";
                //request.Headers.Add("Accept-Encoding", "gzip");
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(data, 0, data.Length);
                dataStream.Close();
                WebResponse webResponse = request.GetResponse();
                var rsp = webResponse.GetResponseStream();
                if (rsp == null)
                {
                    //throw exception
                }
                using (StreamReader readStream = new StreamReader(new GZipStream(rsp, CompressionMode.Decompress)))
                {
                    responseXML = JsonConvert.DeserializeXmlNode(readStream.ReadToEnd()).InnerXml;
                }
                return responseXML;
            }
            catch (WebException webEx)
            {
                //get the response stream
                WebResponse response = webEx.Response;
                Stream stream = response.GetResponseStream();
                String responseMessage = new StreamReader(stream).ReadToEnd();
            }
            return responseXML;
        }
        //public DataSet getdataset(string jsonString)
        //{
        //    try
        //    {
        //        XmlDocument xd = new XmlDocument();
        //        jsonString = "{ \"rootNode\": {" + jsonString.Trim().TrimStart("{").TrimEnd("}") + "} }";
        //        xd = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonString);
        //        DataSet ds = new DataSet();
        //        ds.ReadXml(new XmlNodeReader(xd));
        //        return ds;
        //    }
        //    catch (Exception ex)
        //    {
        //    }
        //}
        public DataTable TopDataRow(DataTable dt, int count)
        {
            DataTable dtn = dt.Clone();
            int i = 0;
            foreach (DataRow row in dt.Rows)
            {
                if (i < count)
                {
                    dtn.ImportRow(row);
                    i++;
                }
                if (i > count)
                    break;
            }
            return dtn;
        }
        public string OTP()
        {
            string numbers = "1234567890";
            string characters = numbers;
            int length = 5;
            string otp = string.Empty;
            for (int i = 0; i < length; i++)
            {
                string character = string.Empty;
                do
                {
                    int index = new Random().Next(0, characters.Length);
                    character = characters.ToCharArray()[index].ToString();
                } while (otp.IndexOf(character) != -1);
                otp += character;
            }
            return otp;
        }
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static string TokenID()
        {
            string numbers = "1234567890";
            string characters = numbers;
            int length = 10;
            string otp = string.Empty;
            for (int i = 0; i < length; i++)
            {
                string character = string.Empty;
                do
                {
                    int index = new Random().Next(0, characters.Length);
                    character = characters.ToCharArray()[index].ToString();
                } while (otp.IndexOf(character) != -1);
                otp += character;
            }
            return otp;
        }
        public static string getHeader(string ReqJson, string url)
        {
            string result = "";
            string s = "";
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(ReqJson);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    result = streamReader.ReadToEnd();
                }
                s = httpResponse.Headers.Get("token");
                return s;
            }
            catch (Exception ex)
            {
            }
            return s;
        }
        public string getjsondata(string url, string Token)
        {
            string result = "";
            string s = "";
            int i = 0;
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
                httpWebRequest.Headers.Add("token", Token);

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    // streamWriter.Write(ReqJson)
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    result = streamReader.ReadToEnd();
                    //s = "insert into ReqType(reqtype) values('" + Strings.Replace(result, "//n", @"\n") + "')";
                    //i = obj.SaveData(s);
                }
                return result;
            }
            // s = httpResponse.Headers.Get("token")
            catch (Exception ex)
            {
            }
            return result;
        }

        public bool IsMemberValid(string name, string memberName)
        {
            bool IsMemberValid = true;
            if (Convert.ToString(memberName.ToLower()) == name.ToLower())
            {
                IsMemberValid = true;
            }
            else
            {
                IsMemberValid = false;
            }
            return IsMemberValid;
        }

        public static byte[] Encrypt(string plainText, byte[] Key, byte[] IV)
        {
            byte[] encrypted;
            // Create a new AesManaged.    
            using (AesManaged aes = new AesManaged())
            {
                // Create encryptor    
                ICryptoTransform encryptor = aes.CreateEncryptor(Key, IV);
                // Create MemoryStream    
                using (MemoryStream ms = new MemoryStream())
                {
                    // Create crypto stream using the CryptoStream class. This class is the key to encryption    
                    // and encrypts and decrypts data from any given stream. In this case, we will pass a memory stream    
                    // to encrypt    
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        // Create StreamWriter and write data to a stream    
                        using (StreamWriter sw = new StreamWriter(cs))
                            sw.Write(plainText);
                        encrypted = ms.ToArray();
                    }
                }
            }
            // Return encrypted data    
            return encrypted;
        }
        public static string Decrypt(byte[] cipherText, byte[] Key, byte[] IV)
        {
            string plaintext = null;
            // Create AesManaged    
            using (AesManaged aes = new AesManaged())
            {
                // Create a decryptor    
                ICryptoTransform decryptor = aes.CreateDecryptor(Key, IV);
                // Create the streams used for decryption.    
                using (MemoryStream ms = new MemoryStream(cipherText))
                {
                    // Create crypto stream    
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        // Read crypto stream    
                        using (StreamReader reader = new StreamReader(cs))
                            plaintext = reader.ReadToEnd();
                    }
                }
            }
            return plaintext;
        }
        private byte[] IV = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
        private int BlockSize = 128;
        public string encrypt(string txtData, string encryptionKey)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(txtData);
            //Encrypt
            SymmetricAlgorithm crypt = Aes.Create();
            HashAlgorithm hash = MD5.Create();
            crypt.BlockSize = BlockSize;
            crypt.Key = hash.ComputeHash(Encoding.Unicode.GetBytes(encryptionKey));
            crypt.IV = IV;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (CryptoStream cryptoStream =
                   new CryptoStream(memoryStream, crypt.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cryptoStream.Write(bytes, 0, bytes.Length);
                }

                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }
        public byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key)
        {
            byte[] encrypted;
            byte[] IV;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;

                aesAlg.GenerateIV();
                IV = aesAlg.IV;

                aesAlg.Mode = CipherMode.CBC;
                aesAlg.BlockSize = BlockSize;
                var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption. 
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            var combinedIvCt = new byte[IV.Length + encrypted.Length];
            Array.Copy(IV, 0, combinedIvCt, 0, IV.Length);
            Array.Copy(encrypted, 0, combinedIvCt, IV.Length, encrypted.Length);

            // Return the encrypted bytes from the memory stream. 
            return combinedIvCt;
        }
        public string DecryptStringFromBytes_Aes(byte[] cipherTextCombined, byte[] Key)
        {

            // Declare the string used to hold 
            // the decrypted text. 
            string plaintext = null;

            // Create an Aes object 
            // with the specified key and IV. 
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;

                byte[] IV = new byte[aesAlg.BlockSize / 8];
                byte[] cipherText = new byte[cipherTextCombined.Length - IV.Length];

                Array.Copy(cipherTextCombined, IV, IV.Length);
                Array.Copy(cipherTextCombined, IV.Length, cipherText, 0, cipherText.Length);

                aesAlg.IV = IV;

                aesAlg.Mode = CipherMode.CBC;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption. 
                using (var msDecrypt = new MemoryStream(cipherText))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }

            return plaintext;

        }



        public string EncryptData(string textData, string Encryptionkey)
        {
            RijndaelManaged objrij = new RijndaelManaged();
            //set the mode for operation of the algorithm
            objrij.Mode = CipherMode.CBC;
            //set the padding mode used in the algorithm.
            //objrij.Padding = PaddingMode.PKCS7;
            //set the size, in bits, for the secret key.
            objrij.KeySize = 0x80;
            ////set the block size in bits for the cryptographic operation.
            //objrij.BlockSize = 0x80;
            ////set the symmetric key that is used for encryption & decryption.
            byte[] passBytes = Encoding.UTF8.GetBytes(Encryptionkey);
            ////set the initialization vector (IV) for the symmetric algorithm
            //byte[] EncryptionkeyBytes = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            //int len = passBytes.Length;
            //if (len > EncryptionkeyBytes.Length)
            //{
            //    len = EncryptionkeyBytes.Length;
            //}
            //Array.Copy(passBytes, EncryptionkeyBytes, len);
            objrij.Key = passBytes;
            //objrij.IV = EncryptionkeyBytes;
            //Creates symmetric AES object with the current key and initialization vector IV.
            ICryptoTransform objtransform = objrij.CreateEncryptor();
            byte[] textDataByte = Encoding.UTF8.GetBytes(textData);
            //Final transform the test string.
            return Convert.ToBase64String(objtransform.TransformFinalBlock(textDataByte, 0, textDataByte.Length));
        }
        public string DecryptData(string EncryptedText, string Encryptionkey)
        {
            RijndaelManaged objrij = new RijndaelManaged();
            objrij.Mode = CipherMode.CBC;
            objrij.Padding = PaddingMode.PKCS7;
            objrij.KeySize = 0x80;
            objrij.BlockSize = 0x80;
            byte[] encryptedTextByte = Convert.FromBase64String(EncryptedText);
            byte[] passBytes = Encoding.UTF8.GetBytes(Encryptionkey);
            byte[] EncryptionkeyBytes = new byte[0x10];
            int len = passBytes.Length;
            if (len > EncryptionkeyBytes.Length)
            {
                len = EncryptionkeyBytes.Length;
            }
            Array.Copy(passBytes, EncryptionkeyBytes, len);
            objrij.Key = EncryptionkeyBytes;
            objrij.IV = EncryptionkeyBytes;
            byte[] TextByte = objrij.CreateDecryptor().TransformFinalBlock(encryptedTextByte, 0, encryptedTextByte.Length);
            return Encoding.UTF8.GetString(TextByte);  //it will return readable string
        }

        public static string XmlHttpRequest(string urlString, string xmlContent)
        {
            string response = null;
            HttpWebRequest httpWebRequest = null;//Declare an HTTP-specific implementation of the WebRequest class.
            HttpWebResponse httpWebResponse = null;//Declare an HTTP-specific implementation of the WebResponse class

            //Creates an HttpWebRequest for the specified URL.
            httpWebRequest = (HttpWebRequest)WebRequest.Create(urlString);

            try
            {
                byte[] bytes;
                bytes = System.Text.Encoding.ASCII.GetBytes(xmlContent);
                //Set HttpWebRequest properties
                httpWebRequest.Method = "POST";
                httpWebRequest.ContentLength = bytes.Length;
                httpWebRequest.ContentType = "text/xml; encoding='utf-8'";
                ServicePointManager.ServerCertificateValidationCallback = new
                RemoteCertificateValidationCallback
                (
                   delegate { return true; }
                );
                using (Stream requestStream = httpWebRequest.GetRequestStream())
                {
                    //Writes a sequence of bytes to the current stream 
                    requestStream.Write(bytes, 0, bytes.Length);
                    requestStream.Close();//Close stream
                }

                //Sends the HttpWebRequest, and waits for a response.
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                if (httpWebResponse.StatusCode == HttpStatusCode.OK)
                {
                    //Get response stream into StreamReader
                    using (Stream responseStream = httpWebResponse.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(responseStream))
                            response = reader.ReadToEnd();
                    }
                }
                httpWebResponse.Close();//Close HttpWebResponse
            }
            catch (WebException we)
            {   //TODO: Add custom exception handling
                throw new Exception(we.Message);
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
            finally
            {
                httpWebResponse.Close();
                //Release objects
                httpWebResponse = null;
                httpWebRequest = null;
            }
            return response;
        }
        private readonly static string reservedCharacters = "!*'();:@&=+$,/?%#[]";

        public static string UrlEncode(string value)
        {
            if (String.IsNullOrEmpty(value))
                return String.Empty;

            var sb = new StringBuilder();

            foreach (char @char in value)
            {
                if (reservedCharacters.IndexOf(@char) == -1)
                    sb.Append(@char);
                else
                    sb.AppendFormat("%{0:X2}", (int)@char);
            }
            return sb.ToString();
        }
        public static string byteToHexString(byte[] byData)
        {
            StringBuilder sb = new StringBuilder((byData.Length * 2));
            int i = 0;

            while ((i < byData.Length))
            {
                int v = (byData[i] & 255);

                if ((v < 16))
                    sb.Append('0');

                sb.Append(v.ToString("X"));
                i += 1;
            }

            return sb.ToString();
        }
        public static string Encrypt(string text, string key)
        {
            using (var md5 = new MD5CryptoServiceProvider())
            {
                using (var tdes = new TripleDESCryptoServiceProvider())
                {
                    tdes.Key = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                    tdes.Mode = CipherMode.ECB;
                    tdes.Padding = PaddingMode.PKCS7;

                    using (var transform = tdes.CreateEncryptor())
                    {
                        byte[] textBytes = UTF8Encoding.UTF8.GetBytes(text);
                        byte[] bytes = transform.TransformFinalBlock(textBytes, 0, textBytes.Length);
                        return Convert.ToBase64String(bytes, 0, bytes.Length);
                    }
                }
            }
        }

        public static string Decrypt(string cipher, string key)
        {
            using (var md5 = new MD5CryptoServiceProvider())
            {
                using (var tdes = new TripleDESCryptoServiceProvider())
                {
                    tdes.Key = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                    tdes.Mode = CipherMode.ECB;
                    tdes.Padding = PaddingMode.PKCS7;

                    using (var transform = tdes.CreateDecryptor())
                    {
                        byte[] cipherBytes = Convert.FromBase64String(cipher);
                        byte[] bytes = transform.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                        return UTF8Encoding.UTF8.GetString(bytes);
                    }
                }
            }
        }
        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
        public string GetUserIp(HttpRequestMessage request)
        {
            if (request.Properties.ContainsKey("MS_HttpContext"))
            {
                var ctx = request.Properties["MS_HttpContext"] as HttpContextBase;
                if (ctx != null)
                {
                    return ctx.Request.UserHostAddress;
                }
            }

            return null;
        }
        public string Generatehash512(string text)
        {
            byte[] message = Encoding.UTF8.GetBytes(text);

            UnicodeEncoding UE = new UnicodeEncoding();
            byte[] hashValue;
            SHA512Managed hashString = new SHA512Managed();
            string hex = "";
            hashValue = hashString.ComputeHash(message);
            foreach (byte x in hashValue)
                hex += String.Format("{0:x2}", x);
            return hex;
        }
        public string PreparePOSTForm(string url, System.Collections.Hashtable data)
        {
            // post form
            // Set a name for the form
            string formID = "PostForm";
            // Build the form using the specified data to be posted.
            StringBuilder strForm = new StringBuilder();
            strForm.Append("<form id=\"" + formID + "\" name=\"" + formID + "\" action=\"" + url + "\" method=\"POST\">");

            foreach (System.Collections.DictionaryEntry key in data)

                strForm.Append("<input type=\"hidden\" name=\"" + Convert.ToString(key.Key) + "\" value=\"" + Convert.ToString(key.Value) + "\">");


            strForm.Append("</form>");
            // Build the JavaScript which will do the Posting operation.
            StringBuilder strScript = new StringBuilder();
            strScript.Append("<script language='javascript'>");
            strScript.Append("var v" + formID + " = document." + formID + ";");
            strScript.Append("v" + formID + ".submit();");
            strScript.Append("</script>");
            // Return the form and the script concatenated.
            // (The order is important, Form then JavaScript)
            return strForm.ToString() + strScript.ToString();
        }
    }
}