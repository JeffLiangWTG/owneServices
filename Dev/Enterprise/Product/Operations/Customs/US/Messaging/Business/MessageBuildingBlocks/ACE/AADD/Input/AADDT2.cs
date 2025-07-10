// Use ADDT2 instead
//namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
//{
//    using CargoWise.Types;

//    [InputBlock("T2")]
//    public partial class AADDT2 : MessageBlock
//    {
//        public AADDT2()
//            : base("T2")
//        {
//        }

//        /// <summary>
//        /// The second line of the importer's address.
//        /// 
//        ///  Space fill if the address is foreign.
//        /// </summary>
//        [MessageBlockString(32, 15, "O")]
//        public ZString LineTwoOfTheMailingAddress;

//        /// <summary>
//        /// The city where the importer is located.
//        /// </summary>
//        [MessageBlockString(21, 47, "M")]
//        public ZString CityMailingAddress;

//        /// <summary>
//        /// The code representing the state where the importer is located.
//        /// </summary>
//        [MessageBlockString(2, 68, "M")]
//        public ZString StateCodeMailingAddress;

//        /// <summary>
//        /// The postal code of where the importer is located.
//        /// 
//        /// Required for U.S., Canadian, and Mexican addresses. If available, provide in valid format for other foreign address, space fill if not provided.
//        /// </summary>
//        [MessageBlockString(9, 70, "C")]
//        public ZString PostalCodeMailingAddress;

//        /// <summary>
//        /// The International Organization for Standardization (ISO) country code representing the country where the importer is located. Provide the appropriate two-position ISO code for all foreign addresses including Canada. Valid ISO country codes are listed in ACS ABI CATAIR Appendix B.
//        /// 
//        /// Space fill for a U.S. address.
//        /// </summary>
//        [MessageBlockString(2, 79, "C")]
//        public ZString ISOCountryCodeMailingAddress;
//    }
//}
