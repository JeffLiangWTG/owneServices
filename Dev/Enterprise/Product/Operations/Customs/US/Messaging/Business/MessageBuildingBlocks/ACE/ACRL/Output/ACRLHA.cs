// Use CRLHA instead
//namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
//{
//    using CargoWise.Types;

//    [OutputBlock("HA")]
//    public partial class ACRLHA : MessageBlock
//    {
//        public ACRLHA()
//            : base("HA")
//        {
//        }

//        /// <summary>
//        /// ACE AE data element: In-bond/In-transit Number.
//        /// The in-bond/in-transit number as listed on the manifest.
//        /// </summary>
//        [MessageBlockString(12, 3, "C")]
//        public ZString InbondNumber;

//        /// <summary>
//        /// ACE AE data element: Master Bill Number.
//        /// The master bill number as listed on the manifest.
//        /// </summary>
//        [MessageBlockString(12, 15, "C")]
//        public ZString MasterBillNumber;

//        /// <summary>
//        /// ACE AE data element: House Bill Number.
//        /// The house bill number as listed on the manifest.
//        /// </summary>
//        [MessageBlockString(12, 27, "C")]
//        public ZString HouseBillNumber;

//        /// <summary>
//        /// ACE AE data element: Sub-House Bill Number.
//        /// The sub-house bill number as listed on the manifest.
//        /// </summary>
//        [MessageBlockString(12, 39, "C")]
//        public ZString SubHouseBillNumber;

//        /// <summary>
//        /// ACE AE data element: Manifested Quantity.
//        /// Total number of units manifested on the lowest bill level reported; total number of units that correspond to the Manifested Quantity Unit of Measure Code.
//        /// </summary>
//        [MessageBlockInt(8, 51, "M")]
//        public ZInt Quantity;

//        /// <summary>
//        /// ACE AE data element: Manifested Quantity Unit of Measure Code.
//        /// A unit of measure code that corresponds to the manifested quantity.
//        /// </summary>
//        [MessageBlockString(5, 59, "M")]
//        public ZString Unit;

//        /// <summary>
//        /// ACE AE data element: In-Bond/In-Transit Date.
//        /// A numeric date in MMDDYY (month, day, year) format representing the in-bond movement date related to the in-bond number.
//        /// </summary>
//        [MessageBlockDate(64, "C", "MMddyy")]
//        public ZDate ImmediateTransportationITDate;

//        /// <summary>
//        /// ACE AE data element: Issuer Code of Master Bill Number.
//        /// A code representing the Standard Carrier Alpha Code (SCAC) of the party who actually issued the ocean bill of lading.
//        /// </summary>
//        [MessageBlockString(4, 70, "C")]
//        public ZString IssuerCodeOfMasterBillNumber;

//        /// <summary>
//        /// ACE AE data element: Issuer Code of House Bill Number.
//        /// A code representing the SCAC of the party who issued the automated ocean/rail house bill of lading.
//        /// </summary>
//        [MessageBlockString(4, 74, "C")]
//        public ZString IssuerCodeOfHouseBillNumber;
//    }
//}
