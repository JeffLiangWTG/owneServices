// Use ADDTC instead
//namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
//{
//    using CargoWise.Types;

//    [InputBlock("TC")]
//    public partial class AADDTC : MessageBlock
//    {
//        public AADDTC()
//            : base("TC")
//        {
//        }

//        /// <summary>
//        /// The city name of the secondary address associated with the importer.
//        /// </summary>
//        [MessageBlockString(21, 3, "M")]
//        public ZString CitySecondaryAddress;

//        /// <summary>
//        /// The state code for the secondary address.
//        /// </summary>
//        [MessageBlockString(2, 24, "M")]
//        public ZString StateCodeSecondaryAddress;

//        /// <summary>
//        /// The postal code for the secondary address of the importer.
//        /// 
//        /// Required for U.S., Canadian, and Mexican addresses. If available, provide in valid format for other foreign address, space fill if not provided.
//        /// </summary>
//        [MessageBlockString(9, 26, "C")]
//        public ZString PostalCodeSecondaryAddress;

//        /// <summary>
//        /// The International Organization for Standardization (ISO) country code representing the country of the secondary address. Provide the appropriate two-position ISO code for all foreign addresses including Canada. Valid ISO country codes are listed in ACS ABI CATAIR Appendix B.
//        /// 
//        /// Space fill for a U.S. address.
//        /// </summary>
//        [MessageBlockString(2, 35, "C")]
//        public ZString ISOCountryCodeSecondaryAddress;
//    }
//}
