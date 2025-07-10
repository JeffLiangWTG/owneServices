// use C:\Dev\Enterprise\Product\Operations\Customs\US\Messaging\Business\MessageBuildingBlocks\CATAIR\INB\Output\INBNS40.cs
//namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
//{
//    using CargoWise.Types;

//    [OutputBlock("40")]
//    public partial class INBNS40 : MessageBlock
//    {
//        public INBNS40()
//            : base("40")
//        {
//        }

//        /// <summary>
//        /// The code representing the entry category. Entry Type codes are listed in CATAIR Appendix B.
//        /// </summary>
//        [MessageBlockString(2, 3, "C")]
//        public ZString EntryType;

//        /// <summary>
//        /// A code representing the USCBP entry number, form number, or a regulatory provision.
//        /// </summary>
//        [MessageBlockString(15, 5, "C")]
//        public ZString EntryNumber;

//        /// <summary>
//        /// The Census Schedule D port code representing the location at which the action occurred.
//        /// </summary>
//        [MessageBlockString(4, 20, "M")]
//        public ZString DistrictPortOfTransaction;

//        /// <summary>
//        /// A Facilities Information and Resources Management Systems (FIRMS) code representing the location of the goods. If the FIRMS code is provided in NS30 for issuer code of master bill number and in-bond carrier code, it will match the FIRMS code provided here.
//        /// </summary>
//        [MessageBlockString(4, 24, "C")]
//        public ZString FIRMSCode;

//        /// <summary>
//        /// A valid container number associated with the bill of lading. The container number must reflect the number exactly as it physically appears on the container.
//        /// </summary>
//        [MessageBlockString(14, 28, "C")]
//        public ZString ContainerNumber;
//    }
//}
