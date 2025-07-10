// Use ADDT1 instead
//namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
//{
//    using CargoWise.Types;

//    [InputBlock("T1")]
//    public partial class AADDT1 : MessageBlock
//    {
//        public AADDT1()
//            : base("T1")
//        {
//        }

//        /// <summary>
//        /// Valid codes are:
//        /// A = add an importer number, 
//        /// U = update an existing importer number, or 
//        /// N = apply for a CBP assigned number.
//        /// </summary>
//        [MessageBlockString(1, 3, "M")]
//        public ZString UpdateActionCode;

//        /// <summary>
//        /// A code identifying the importer. 
//        /// If the update action code is N, space fill.
//        /// </summary>
//        [MessageBlockString(12, 4, "M")]
//        public ZString ImporterNumber;

//        /// <summary>
//        /// The first line of the importer's name.
//        /// </summary>
//        [MessageBlockString(32, 16, "M")]
//        public ZString LineOneOfTheImporterName;

//        /// <summary>
//        /// The first line of the importer's mailing address. If the importer has separate mailing and business addresses, the mailing address must be shown as the first address. If the mailing address is a post office box, a second address must be provided. If the mailing address is a rural route and box number or HC (Highway Contract) and box number, a second address is not required.
//        /// </summary>
//        [MessageBlockString(32, 48, "M")]
//        public ZString LineOneOfTheMailingAddress;

//        /// <summary>
//        /// A code identifying the type of importer. This code is required for all U.S. importers. Valid Importer Type Codes are:
//        /// 
//        /// C = Corporation
//        /// P = Partnership
//        /// I = Individual
//        /// S = Sole Proprietor
//        /// F = Foreign Government
//        /// G = US Government
//        /// L = State Government
//        /// </summary>
//        [MessageBlockString(1, 80, "C")]
//        public ZString ImporterType;
//    }
//}
