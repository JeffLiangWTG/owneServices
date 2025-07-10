// Use ADDTA instead
//namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
//{
//    using CargoWise.Types;

//    [InputBlock("TA")]
//    public partial class AADDTA : MessageBlock
//    {
//        public AADDTA()
//            : base("TA")
//        {
//        }

//        /// <summary>
//        /// A code qualifying the second line of the importer name. Valid Name Qualifier Codes are:
//        /// 
//        /// DIV = The name of the parent company if the importer is a division under the parent company.
//        /// DBA = The name the importer is doing business as.
//        /// AKA = The name the importer is also known as.
//        /// </summary>
//        [MessageBlockString(3, 3, "M")]
//        public ZString NameQualifier;

//        /// <summary>
//        /// The second line of the importer name is required if there is a Name Qualifier (positions 3-5). If there is not a Name Qualifier in positions 3-5, an error message is system generated.
//        /// </summary>
//        [MessageBlockString(32, 6, "M")]
//        public ZString LineTwoOfTheImporterName;
//    }
//}
