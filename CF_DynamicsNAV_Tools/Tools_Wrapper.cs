using System;
using System.Runtime.InteropServices;

namespace CF_DynamicsNAV_Tools
{
    /// <summary>
    /// (deprecated)Tools for retrieving Amazon related Fees, (deprecated)
    /// </summary>
    [System.Obsolete("Method of retrieving FBAFees is deprecated", false)]
    [ClassInterface(ClassInterfaceType.None)]
    [Guid("F2EE235B-3E92-4636-80B6-F82D7FD77B41")]
    public class FBAFees : IFBAFees
    {
        private MiscClasses.AFNFees fees;

        public FBAFees() { }

        public void Initialize(string language, string marketPlaceId, string currency)
        {
            fees = new MiscClasses.AFNFees(language, marketPlaceId, currency);
        }

        public int GetFees(string asin, double price)
        {
            return fees.GetFees(asin, price);
        }

        public double GetFee(string feename)
        {
            double result = 0;

            if (fees.Fees.ContainsKey(feename))
                result = fees.Fees[feename];

            return result;
        }

        public bool GetDimensions(string asin, ref double length, ref double width, ref double height, ref double weight)
        {
            return fees.GetDimensions(asin, ref length, ref width, ref height, ref weight);
        }

        public string ErrorMessage()
        {
            string result = "";

            if (fees.ErrorOccurred)
            {
                result = fees.ErrorMessage;
            }

            return result;
        }

    }
}

