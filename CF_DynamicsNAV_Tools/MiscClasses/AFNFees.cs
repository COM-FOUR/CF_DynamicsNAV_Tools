using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Net;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Globalization;
using System.Windows.Forms;

namespace CF_DynamicsNAV_Tools.MiscClasses
{
    public class AFNFees
    {
        public bool ErrorOccurred { get; set; }
        public string ErrorMessage { get; set; }

        public SortedList<string, double> Fees;

        internal string ProfitCalcToken = "";
        internal CookieContainer Cookies = new CookieContainer();

        public string Language { get; set; }
        public string MarketPlaceID { get; set; }
        public string Currency { get; set; }

        static string MainUrl = "https://sellercentral-europe.amazon.com/fba/profitabilitycalculator/index?lang={0}";
        static string MatchesUrl = "https://sellercentral-europe.amazon.com/fba/profitabilitycalculator/productmatches?searchKey={0}&language={1}&profitcalcToken={2}";
        static string FeeUrl = "https://sellercentral-europe.amazon.com/fba/profitabilitycalculator/getafnfee?profitcalcToken={0}";

        public AFNFees() { }

        public AFNFees(string language, string marketPlaceID, string currency)
        {
            Language = language;
            MarketPlaceID = marketPlaceID;
            Currency = currency;
        }

        public bool GotFees()
        {
            return (Fees != null & Fees.Count != 0);
        }

        private bool GetProfitCalcToken()
        {
            ProfitCalcToken = "";
            string responsestring = GetResponseString("GET", String.Format(MainUrl, Language), "");

            if (!ErrorOccurred)
                try
                {
                    responsestring = responsestring.Remove(0, responsestring.IndexOf("name='profitcalcToken' value='") + 30);
                    ProfitCalcToken = responsestring.Remove(responsestring.IndexOf("'"));
                }
                catch (Exception e)
                {
                    ErrorOccurred = true;
                    ErrorMessage = e.Message;
                }

            return (ProfitCalcToken != "");
        }
        private string GetResponseString(string method, string requesturl, string requeststring)
        {
            string result = "";

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(requesturl);
            wr.Method = method;

            wr.CookieContainer = Cookies;
            wr.ContentType = "application/json;charset=UTF-8";

            Random rnd = new Random();

            switch (rnd.Next(1, 8))
            {
                case 1: wr.UserAgent = "Mozilla / 5.0(compatible; Googlebot / 2.1; +http://www.google.com/bot.html)";  break;
                case 2: wr.UserAgent = "Mozilla / 5.0(compatible; Yahoo!Slurp; http://help.yahoo.com/help/us/ysearch/slurp)"; break;
                case 3: wr.UserAgent = "Mozilla / 5.0(Windows NT 10.0; Win64; x64) AppleWebKit / 537.36(KHTML, like Gecko) Chrome / 42.0.2311.135 Safari / 537.36 Edge / 12.246"; break;
                case 4: wr.UserAgent = "Mozilla / 5.0(Windows NT 6.1; Trident / 7.0; rv: 11.0) like Gecko"; break;
                case 5: wr.UserAgent = "Mozilla / 5.0(Windows NT 6.1; WOW64) AppleWebKit / 537.36(KHTML, like Gecko) Chrome / 47.0.2526.111 Safari / 537.36"; break;
                case 6: wr.UserAgent = "Mozilla / 5.0(Windows NT 6.3; Win64, x64; Trident / 7.0; Touch; rv: 11.0) like Gecko"; break;
                case 7:  wr.UserAgent = "Mozilla / 5.0(X11; Ubuntu; Linux x86_64; rv: 15.0) Gecko / 20100101 Firefox / 15.0.1"; break;
                    
                default:
                    break;
            }
            
            if (requeststring != "")
            {
                Stream s = wr.GetRequestStream();

                byte[] requestbytes = Encoding.Default.GetBytes(requeststring);

                s.Write(requestbytes, 0, requestbytes.Length);
                s.Flush();
                s.Close();
            }

            try
            {
                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Stream s = response.GetResponseStream();
                StreamReader sr = new StreamReader(s);
                result = sr.ReadToEnd();
            }
            catch (Exception e)
            {
                ErrorOccurred = true;
                ErrorMessage = e.Message;
            }

            return result;
        }
        public int GetFees(string asin, double price)
        {
            string tempstring = "";

            if (price == 0)
                price = 100;
            else
                Math.Round(price, 0, MidpointRounding.AwayFromZero);

            Fees = new SortedList<string, double>();

            try
            {
                if (ProfitCalcToken=="" && !GetProfitCalcToken())
                {
                    ErrorOccurred = true;
                    ErrorMessage = "Es konnte kein Token ermittelt werden";
                    return 0;
                }

                tempstring = GetResponseString("GET", String.Format(MatchesUrl, asin, Language, ProfitCalcToken), "");

                if ((tempstring.IndexOf("succeed\":\"false") > 0) | (tempstring.IndexOf("data\":[]") > 0))
                {
                    ErrorOccurred = true;
                    ErrorMessage = "Keine ASIN gefunden";
                    return 0;
                }

                JSONProductMatchesResponse jpmr = new JSONProductMatchesResponse(tempstring);

                JSONASINFeesRequest jfrequest = new JSONASINFeesRequest(jpmr.Data[0], MarketPlaceID, price, Currency);

                tempstring = GetResponseString("POST", String.Format(FeeUrl, ProfitCalcToken), jfrequest.ToString());

                if (tempstring.IndexOf("succeed\":\"false") > 0)
                {
                    ErrorOccurred = true;
                    ErrorMessage = "Gebühren konnten nicht abgerufen werden";
                    return 0;
                }

                //MessageBox.Show(tempstring);

                JSONASINFeeResponse jfresponse = new JSONASINFeeResponse(tempstring);


                SortedList<string, double> fees = jfresponse.Data.AfnFees.ToASINFeeList(asin, Language);

                foreach (var item in fees)
                {
                    Fees.Add(item.Key,item.Value);
                }

                fees = jfresponse.Data.MfnFees.ToASINFeeList(asin, Language);
                foreach (var item in fees)
                {
                    Fees.Add(item.Key, item.Value);
                }

            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
                ErrorOccurred = true;
            }

            return Fees.Count;
        }
        public bool GetDimensions(string asin, ref double length, ref double width, ref double height, ref double weight)
        {
            bool result = false;

            string tempstring = "";

            try
            {
                if (ProfitCalcToken == "" && !GetProfitCalcToken())
                {
                    ErrorOccurred = true;
                    ErrorMessage = "Es konnte kein Token ermittelt werden";
                    return false;
                }

                tempstring = GetResponseString("GET", String.Format(MatchesUrl, asin, Language, ProfitCalcToken), "");

                if ((tempstring.IndexOf("succeed\":\"false") > 0) | (tempstring.IndexOf("data\":[]") > 0))
                {
                    ErrorOccurred = true;
                    ErrorMessage = "Keine ASIN gefunden";
                    return false;
                }

                JSONProductMatchesResponse jpmr = new JSONProductMatchesResponse(tempstring);

                length = jpmr.Data[0].Length;
                width = jpmr.Data[0].Width;
                height = jpmr.Data[0].Height;
                weight = jpmr.Data[0].Weight;
                if (jpmr.Data[0].WeightUnit == "gramms")
                    weight*=1000;

                result = true;

            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
                ErrorOccurred = true;
            }

            return result;
        }

        public class ASINFee
        {
            public string ASIN { get; set; }
            public string FeeName { get; set; }
            public string Language { get; set; }
            public decimal Value { get; set; }

            public ASINFee(string asin, string name, string language, decimal value)
            {
                ASIN = asin;
                FeeName = name;
                Language = language;
                Value = value;
            }
            public override string ToString()
            {
                CultureInfo ci = new CultureInfo(Language.Replace("_", "-"));
                //TODO Translate(FulfillmentChannel) 
                return String.Format("{0}: {1}", FeeName, Value.ToString("C", ci));
            }
        }
        public static T DeserializeJSon<T>(string jsonString)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
            T obj = (T)ser.ReadObject(stream);
            return obj;
        }
        public static string SerializeJSon<T>(T t)
        {
            MemoryStream stream = new MemoryStream();
            DataContractJsonSerializer ds = new DataContractJsonSerializer(typeof(T));
            DataContractJsonSerializerSettings s = new DataContractJsonSerializerSettings();
            ds.WriteObject(stream, t);
            string jsonString = Encoding.UTF8.GetString(stream.ToArray());
            stream.Close();
            return jsonString;
        }

        [DataContract]
        internal class JSONProductMatchesResponse
        {
            [DataMember(Name = "data")]
            public List<JSONASIN> Data { get; set; }

            //[DataMember(Name = "errorMessage")]
            //public string ErrorMessage { get; set; }

            [DataMember(Name = "processedDate")]
            public string ProcessedDate { get; set; }

            [DataMember(Name = "succeed")]
            public bool Succeed { get; set; }

            public JSONProductMatchesResponse(string jsonstring)
            {
                JSONProductMatchesResponse jsonpmr = DeserializeJSon<JSONProductMatchesResponse>(jsonstring);
                this.Data = jsonpmr.Data;
                this.ProcessedDate = jsonpmr.ProcessedDate;
                this.Succeed = jsonpmr.Succeed;
                //this.ErrorMessage = jsonpmr.ErrorMessage;
            }

            public override string ToString()
            {
                return SerializeJSon<JSONProductMatchesResponse>(this);
            }
        }

        [DataContract]
        internal class JSONASIN
        {
            [DataMember(Name = "asin")]
            public string ASIN { get; set; }

            [DataMember(Name = "binding")]
            public string Binding { get; set; }

            [DataMember(Name = "dimensionUnit")]
            public string DimensionUnit { get; set; }

            [DataMember(Name = "dimensionUnitString")]
            public string DimensionUnitString { get; set; }

            [DataMember(Name = "encryptedMarketplaceId")]
            public string EncryptedMarketplaceId { get; set; }

            [DataMember(Name = "gl")]
            public string GL { get; set; }

            [DataMember(Name = "height")]
            public double Height { get; set; }

            [DataMember(Name = "imageUrl")]
            public string ImageUrl { get; set; }

            [DataMember(Name = "isAsinLimits")]
            public string IsAsinLimits { get; set; }

            [DataMember(Name = "isWhiteGloveRequired")]
            public string IsWhiteGloveRequired { get; set; }

            [DataMember(Name = "length")]
            public double Length { get; set; }

            [DataMember(Name = "link")]
            public string Link { get; set; }

            [DataMember(Name = "originalUrl")]
            public string OriginalUrl { get; set; }

            [DataMember(Name = "productGroup")]
            public string ProductGroup { get; set; }

            [DataMember(Name = "subCategory")]
            public string SubCategory { get; set; }

            [DataMember(Name = "thumbStringUrl")]
            public string ThumbStringUrl { get; set; }

            [DataMember(Name = "title")]
            public string Title { get; set; }

            [DataMember(Name = "weight")]
            public double Weight { get; set; }

            [DataMember(Name = "weightUnit")]
            public string WeightUnit { get; set; }

            [DataMember(Name = "weightUnitString")]
            public string WeightUnitString { get; set; }

            [DataMember(Name = "width")]
            public double Width { get; set; }

            public override string ToString()
            {
                return SerializeJSon<JSONASIN>(this);
            }
        }

        [DataContract]
        internal class JSONASINFeesRequest
        {
            [DataMember(Name = "afnPriceStr")]
            public double AfnPriceStr { get; set; }

            [DataMember(Name = "currency")]
            public string Currency { get; set; }

            [DataMember(Name = "futureFeeDate")]
            public string FutureFeeDate { get; set; }

            [DataMember(Name = "hasFutureFee")]
            public bool HasFutureFee { get; set; }

            [DataMember(Name = "hasTaxPage")]
            public bool HasTaxPage { get; set; }

            [DataMember(Name = "marketPlaceId")]
            public string MarketPlaceId { get; set; }
            
            [DataMember(Name = "mfnPriceStr")]
            public double MfnPriceStr { get; set; }

            [DataMember(Name = "mfnShippingPriceStr")]
            public double MfnShippingPriceStr { get; set; }

            [DataMember(Name = "productInfoMapping")]
            public JSONASIN ProductInfoMapping { get; set; }

            
            public JSONASINFeesRequest(JSONASIN data, string marketplaceid, double price, string currency)
            {
                data.Title = HttpUtility.UrlEncode(data.Title);

                if (data.Height == 0)
                    data.Height = 1;
                if (data.Width == 0)
                    data.Width = 1;
                if (data.Length == 0)
                    data.Length = 1;
                if (data.Weight == 0)
                    data.Weight = 0.1;

                data.DimensionUnitString = HttpUtility.UrlEncode(data.DimensionUnitString);
                data.WeightUnitString = HttpUtility.UrlEncode(data.WeightUnitString);

                this.ProductInfoMapping = data;
                this.AfnPriceStr = price;
                this.MfnPriceStr = 0;
                this.MfnShippingPriceStr = 0;
                this.Currency = currency;
                this.MarketPlaceId = marketplaceid;
                this.HasFutureFee = false;
                this.FutureFeeDate = "2015-08-05 00:00:00";
                this.HasTaxPage = true;

            }
            public override string ToString()
            {
                return (SerializeJSon<JSONASINFeesRequest>(this)).Replace(@"\/", @"/");
            }
        }
        [DataContract]
        internal class JSONASINFeeResponse
        {
            [DataMember(Name = "data")]
            public JSONASINFee Data { get; set; }

            [DataMember(Name = "processedDate")]
            public string ProcessedDate { get; set; }

            [DataMember(Name = "succeed")]
            public string Succeed { get; set; }
            
            public JSONASINFeeResponse(string jsonstring)
            {
                JSONASINFeeResponse jsonafr = DeserializeJSon<JSONASINFeeResponse>(jsonstring);
                this.Data = jsonafr.Data;
                this.ProcessedDate = jsonafr.ProcessedDate;
                this.Succeed = jsonafr.Succeed;

            }
            public override string ToString()
            {
                return SerializeJSon<JSONASINFeeResponse>(this);
            }
        }

        [DataContract]
        internal class JSONASINAFNFee
        {
            [DataMember(Name = "fixedClosingFee")]
            public JSONASINSingleFee FixedClosingFee { get; set; }

            [DataMember(Name = "pickAndPackFee")]
            public JSONASINSingleFee PickAndPackFee { get; set; }

            [DataMember(Name = "referralFee")]
            public JSONASINSingleFee ReferralFee { get; set; }

            [DataMember(Name = "storageFee")]
            public JSONASINSingleFee StorageFee { get; set; }

            [DataMember(Name = "variableClosingFee")]
            public JSONASINSingleFee VariableClosingFee { get; set; }
            
           // [DataMember(Name = "weightHandlingFee")]
           // public JSONASINSingleFee WeightHandlingFee { get; set; }

            public SortedList<string, double> ToASINFeeList(string asin, string language)
            {
                //CultureInfo ci = new CultureInfo(language.Replace("_", "-"));
                CultureInfo ci = new CultureInfo("en-GB");

                SortedList<string, double> result = new SortedList<string, double>();

                if (PickAndPackFee.amount != null)
                    //result.Add("afnweightHandlingFee", Double.Parse(WeightHandlingFee.amount, ci));
                result.Add("afnfixedClosingFee", Double.Parse(FixedClosingFee.amount, ci));
                result.Add("afnreferralFee", Double.Parse(ReferralFee.amount, ci));
                result.Add("afnpickAndPackFee", Double.Parse(PickAndPackFee.amount, ci));
                result.Add("afnstorageFee", Double.Parse(StorageFee.amount, ci));
                result.Add("afnvariableClosingFee", Double.Parse(VariableClosingFee.amount, ci));

                return result;
            }

            public override string ToString()
            {
                return SerializeJSon<JSONASINAFNFee>(this);
            }
        }
        [DataContract]
        internal class JSONASINMFNFee
        {
            [DataMember(Name = "fixedClosingFee")]
            public JSONASINSingleFee FixedClosingFee { get; set; }

            [DataMember(Name = "variableClosingFee")]
            public JSONASINSingleFee VariableClosingFee { get; set; }
            
            [DataMember(Name = "referralFee")]
            public JSONASINSingleFee ReferralFee { get; set; }

            public SortedList<string, double> ToASINFeeList(string asin, string language)
            {
                //CultureInfo ci = new CultureInfo(language.Replace("_", "-"));
                CultureInfo ci = new CultureInfo("en-GB");

                SortedList<string, double> result = new SortedList<string, double>
                {
                    { "mfnreferralFee", Double.Parse(ReferralFee.amount, ci) },
                    { "mfnfixedClosingFee", Double.Parse(FixedClosingFee.amount, ci) },
                    { "mfnvariableClosingFee", Double.Parse(VariableClosingFee.amount, ci) }
                };

                return result;
            }

            public override string ToString()
            {
                return SerializeJSon<JSONASINMFNFee>(this);
            }
        }

        [DataContract]
        internal class JSONASINFee
        {
            [DataMember(Name = "afnFees")]
            public JSONASINAFNFee AfnFees { get; set; }

            [DataMember(Name = "afnFutureFees")]
            public JSONASINAFNFee AfnFutureFees { get; set; }

            [DataMember(Name = "mfnFees")]
            public JSONASINMFNFee MfnFees { get; set; }

            [DataMember(Name = "mfnFutureFees")]
            public JSONASINMFNFee MfnFutureFees { get; set; }

            public override string ToString()
            {
                return SerializeJSon<JSONASINFee>(this);
            }
        }

        [DataContract]
        internal class JSONASINSingleFee
        {
            [DataMember(Name = "amount")]
            public string amount { get; set; }

            [DataMember(Name = "taxAmount")]
            public string taxAmount { get; set; }
            
            public override string ToString()
            {
                return SerializeJSon<JSONASINSingleFee>(this);
            }
        }
    }
}
