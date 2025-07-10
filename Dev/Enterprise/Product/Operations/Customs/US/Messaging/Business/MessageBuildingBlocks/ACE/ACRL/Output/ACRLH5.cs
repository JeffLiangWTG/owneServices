// Use CRLH5 instead
//namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
//{
//    using CargoWise.Types;

//    [OutputBlock("H5")]
//    public partial class ACRLH5 : MessageBlock
//    {
//        public ACRLH5()
//            : base("H5")
//        {
//        }

//        /// <summary>
//        /// The record control number begins with 001 and is incremented by one each time Record Identifier H5 is repeated.
//        /// </summary>
//        [MessageBlockInt(3, 3, "M")]
//        public ZInt RecordControlNumber;

//        /// <summary>
//        /// ACE AE data element: Country of Origin Code.
//        /// The International Organization for Standardization (ISO) country code representing the country of origin.
//        /// </summary>
//        [MessageBlockString(2, 6, "M")]
//        public ZString CountryOfOriginCode;

//        /// <summary>
//        /// ACE AE data element: HTS Number.
//        /// The valid commodity number under which the article is classified in the Harmonized Tariff Schedule of the United States Annotated (HTS).
//        /// </summary>
//        [MessageBlockString(10, 8, "M")]
//        public ZString TariffNumber;

//        /// <summary>
//        /// ACE AE data element: Manufacturer/ Supplier Code.
//        /// A code representing the manufacturer / supplier.
//        /// </summary>
//        [MessageBlockString(15, 18, "M")]
//        public ZString ManufacturerShipperCode;

//        /// <summary>
//        /// ACE AE data element: Ultimate Consignee Number.
//        /// Identification of the US party or other entity to whom the overseas shipper sold the imported merchandise.
//        /// </summary>
//        [MessageBlockString(12, 33, "M")]
//        public ZString LineItemUltimateConsignee;

//        /// <summary>
//        /// ACE AE data element: Value of Goods Amount.
//        /// The line item value in whole dollars.
//        /// </summary>
//        [MessageBlockDecimal(10, 45, "M", 0)]
//        public ZDecimal LineItemValue;
//    }
//}
