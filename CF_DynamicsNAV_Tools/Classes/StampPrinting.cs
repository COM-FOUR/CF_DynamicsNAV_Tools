using System;
using System.Runtime.InteropServices;
using CF.Shipping;
using CF.Shipping.DataTypes;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;

namespace CF_DynamicsNAV_Tools
{
    /// <summary>
    /// Tools for Printing Labels for integration into DynamicsNAV 
    /// </summary>
    [ClassInterface(ClassInterfaceType.None)]
    [Guid("E0AC58B8-EC88-4B4B-A026-E7C78DDBDF98")]
    public class StampPrinting : IStampPrinting
    {
        internal DP dp;
        internal string lastMessage = "";

        public void Init(string pId, string kPhase, string sKey, string uId, string pword)
        {
            dp = new DP(pId, kPhase, sKey, uId, pword);
        }
        public void SetSender(string name, string name2, string street, string houseNo, string postCode, string city, string countryCode)
        {
            dp.AddSenderAddress(new CF.Shipping.DataTypes.Address() {
                Name = name,
                Name2 = name2,
                Street = street,
                HouseNo = houseNo,
                PostCode = postCode,
                City = city,
                CountryCode = countryCode
            });
        }
        public void AddReceiver(string name, string name2, string street, string houseNo, string postCode, string city, string countryCode, string ref1, string ref2, int prodId, int value)
        {
            dp.AddReceiverAddress(new CF.Shipping.DataTypes.Address()
            {
                Name = name,
                Name2 = name2,
                Street = street,
                HouseNo = houseNo,
                PostCode = postCode,
                City = city,
                CountryCode = countryCode,
                Reference = ref1,
                Reference2 = ref2,
                StampProductCode = prodId,
                StampValue = value
            });
        }
        public bool RetrieveAndPrintStamps(string printerName)
        {
            bool result = false;

            if (dp.AuthenticateUser() &&
                dp.CreateShopOrder() &&
                dp.CheckoutShoppingCart(StampType.PNG) &&
                dp.DownloadStampFiles())
            {
                result = dp.PrintPNGStampFiles(printerName);
            }
            if (!result)
            {
                lastMessage = dp.LastMessage;
            }

            return result;
        }
        public bool RetrieveAndExportStamps(string folderPath)
        {
            bool result = false;

            if (dp.AuthenticateUser() &&
                dp.CreateShopOrder() &&
                dp.CheckoutShoppingCart(StampType.PNG) &&
                dp.DownloadStampFiles())
            {
                result = dp.ExportPNGStampFiles(folderPath);
            }
            if (!result)
            {
                lastMessage = dp.LastMessage;
            }

            return result;
        }
        public void Reset()
        {
            dp.Init(false);
        }
        public string GetLastMessage()
        {
            return lastMessage;
        }
        public double GetWalletBalance()
        {
            double result = -1;

            if (dp.AuthenticateUser())
            {
                result = dp.WalletBalance;
            }

            return result;
        }
        public string GetDownloadUrl()
        {
            return dp.StampFilesArchiveUrl;
        }
        public bool DownloadAndExportStamps(string url,string folderPath,string refNo)
        {
            bool result = false;

            if (dp.DownloadStampFiles(url,refNo))
            {
                result = dp.ExportPNGStampFiles(folderPath);
            }
            if (!result)
            {
                lastMessage = dp.LastMessage;
            }

            return result;
        }
    }
}
