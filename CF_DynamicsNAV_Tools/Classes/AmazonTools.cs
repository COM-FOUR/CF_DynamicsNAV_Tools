using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace CF_DynamicsNAV_Tools
{
    /// <summary>
    /// Tools for Amazon-Integration into Dynamics NAV
    /// </summary>
    [ClassInterface(ClassInterfaceType.None)]
    [Guid("907E2820-81F4-45E0-B151-8311453ED37D")]
    public class AmazonTools : IAmazonTools
    {
        string requestURL;
        string signature;

        string[] requestparts;
        MSXML2.ServerXMLHTTP60 xmlhttp;

        public AmazonTools()
        {
            requestURL = "";
            signature = "";
            //requestparts = new string[1];
        }

        /// <summary>
        /// sorting string by BigEndian, for calculating AmazonMWS signature
        /// </summary>
        /// <param name="inStr">string to be sorted</param>
        /// <returns>sorted string</returns>
        public string GetByteOrder(string inStr)
        {

            ParamComparer pc = new ParamComparer();
            SortedDictionary<string, string> slist = new SortedDictionary<string, string>(pc);

            while (inStr.IndexOf("&", 0) >= 0)
            {
                int i = inStr.IndexOf("&", 0);

                slist.Add(inStr.Substring(0, i), "");
                inStr = inStr.Remove(0, i + 1);
            }

            slist.Add(inStr, "");

            string result = "";

            foreach (KeyValuePair<string, string> item in slist)
            {
                if (result == "")
                {
                    result = (item.Key);
                }
                else
                {
                    result += "&" + (item.Key);
                }
            }

            return result;
        }
        /// <summary>
        /// calculating the hash value for AmazonMWS signature
        /// </summary>
        /// <param name="key">secretkey for building the hash</param>
        /// <param name="text">string for the hash to be calculated</param>
        /// <returns>hash string</returns>
        public string GetAmazonSHA256Hash(string key, string text)
        {
            UnicodeEncoding UE = new UnicodeEncoding();
            byte[] hashValue;
            byte[] keybytes = Encoding.ASCII.GetBytes(key);
            byte[] message = Encoding.ASCII.GetBytes(text);

            HMACSHA256 hashstring = new HMACSHA256(keybytes);
            string result = "";

            hashValue = hashstring.ComputeHash(message);

            result = UTF8toASCII(System.Convert.ToBase64String(hashValue));

            return result;
        }
        class ParamComparer : IComparer<string>
        {
            public int Compare(string p1, string p2)
            {
                //Amazon Special Kacke-->

                bool specialkacke = false;

                if (p1.Contains("SellerSkus.member.") & p2.Contains("SellerSkus.member."))
                {
                    p1 = p1.Remove(0, 18);
                    p2 = p2.Remove(0, 18);

                    if (p1[0] == p2[0])
                    {
                        specialkacke = true;
                    }
                }

                //Amazon Special Kacke die ZWEITE-->
                if (p1.Contains("SellerSKUList.SellerSKU.") & p2.Contains("SellerSKUList.SellerSKU."))
                {
                    p1 = p1.Remove(0, 24);
                    p2 = p2.Remove(0, 24);

                    if (p1[0] == p2[0])
                    {
                        specialkacke = true;
                    }
                }
                //<--
                //Amazon Special Kacke die DRITTE-->
                if (p1.Contains("ASINList.ASIN.") & p2.Contains("ASINList.ASIN."))
                {
                    p1 = p1.Remove(0, 14);
                    p2 = p2.Remove(0, 14);

                    if (p1[0] == p2[0])
                    {
                        specialkacke = true;
                    }
                }
                //<--
                //Amazon Special Kacke die VIERTE-->
                if (p1.Contains("IdList.Id.") & p2.Contains("IdList.Id."))
                {
                    p1 = p1.Remove(0, 10);
                    p2 = p2.Remove(0, 10);

                    if (p1[0] == p2[0])
                    {
                        specialkacke = true;
                    }
                }
                //<--

                //Amazon Special Kacke die FÜNFTE-->
                if (p1.Contains("SellerSKUList.Id.") & p2.Contains("SellerSKUList.Id."))
                {
                    p1 = p1.Remove(0, 17);
                    p2 = p2.Remove(0, 17);

                    if (p1[0] == p2[0])
                    {
                        specialkacke = true;
                    }
                }

                if (specialkacke)
                {
                    int int1 = int.Parse(p1.Substring(0, p1.IndexOf("=")));
                    int int2 = int.Parse(p2.Substring(0, p2.IndexOf("=")));

                    Console.WriteLine("{0} vs. {1}", int1, int2);

                    return int1 == int2 ? 0 : (Math.Max(int1, int2) == int1 ? 1 : -1);
                }
                else
                {
                    return string.CompareOrdinal(p1, p2);
                }
            }
        }

        static string UTF8toASCII(string text)
        {
            System.Text.Encoding utf8 = System.Text.Encoding.UTF8;
            Byte[] encodedBytes = utf8.GetBytes(text);
            Byte[] convertedBytes =
                    Encoding.Convert(Encoding.UTF8, Encoding.ASCII, encodedBytes);
            System.Text.Encoding ascii = System.Text.Encoding.ASCII;

            return ascii.GetString(convertedBytes);
        }

        public string GetUTCTimeString(string dtnow)
        {
            string result = DateTime.UtcNow.ToString();
            DateTime utcnow = new DateTime();
            if (DateTime.TryParse(dtnow, out utcnow))
            {
                result = utcnow.ToUniversalTime().ToString();
            }

            return result;
        }
        public string URLEncode(string inStr)
        {
            string outStr = "";

            outStr = System.Web.HttpUtility.UrlEncode(inStr);

            return outStr;
        }

        public string GetRequestURL()
        {
            return requestURL;
        }
        public void SetRequestURL(string intext)
        {
            requestURL = intext;
        }
        public void AddToRequestURL(string intext)
        {
            requestURL += intext;
        }
        public string GetSignature()
        {
            return signature;
        }
        public void SetSignature(string intext)
        {
            signature = intext;
        }
        public void AddRightToSignature(string intext)
        {
            signature += intext;
        }
        public void AddLeftToSignature(string intext)
        {
            signature = intext + signature;
        }

        public void InitSignature()
        {
            signature = requestURL.Remove(0, requestURL.IndexOf("?") + 1);
        }
        public void ReplaceInSignature(string fromtext, string totext)
        {
            signature = signature.Replace(fromtext, totext);
        }

        public void SortSignature()
        {
            signature = GetByteOrder(signature);
        }
        public void SignatureToHash(string key)
        {
            signature = GetAmazonSHA256Hash(key, signature);
        }
        public string RequestPart(int entryno)
        {
            string result = "";

            if (entryno <= requestparts.Length)
                result = requestparts[entryno - 1];

            return result;
        }

        public int CreateRequestParts()
        {
            string result = requestURL + signature;

            List<string> parts = new List<string>();

            while (result != "")
            {
                if (result.Length > 1000)
                {
                    string part = result.Substring(0, 1000);
                    parts.Add(part);
                    result = result.Remove(0, 1000);
                }
                else
                {
                    parts.Add(result);
                    result = "";
                }
            }

            requestparts = parts.ToArray();

            requestURL = requestURL + signature;
            return requestparts.Length;
        }

        public void SetXMLHTTP(MSXML2.ServerXMLHTTP60 xml)
        {
            xmlhttp = xml;
        }
        public MSXML2.ServerXMLHTTP60 GetXMLHTTP()
        {
            return xmlhttp;
        }

        public bool DoOpen(string method, string url, bool async, string user, string password)
        {
            bool result = false;

            if (url == "")
            {
                url = requestURL;
            }

            try
            {
                xmlhttp.open(method, url, async, user, password);
                result = true;
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }
        public bool DoSend(string body)
        {
            bool result = false;

            try
            {
                xmlhttp.send(body);
                result = true;
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }
        public void ShowXMLHTTPResponseText(MSXML2.ServerXMLHTTP60 xml)
        {
            MessageBox.Show(xml.responseText);
        }
        public int GetByteCount(string inString)
        {
            return UTF8Encoding.UTF8.GetByteCount(inString);
        }
    }
}
