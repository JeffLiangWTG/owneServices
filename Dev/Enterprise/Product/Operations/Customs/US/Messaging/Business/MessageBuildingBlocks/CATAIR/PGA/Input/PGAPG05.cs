//use Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input.AEPAPG05 instead 

//namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input.Abstract
//{
//    using CargoWise.Types;

//    [InputBlock("PG05")]
//    public abstract partial class PGAPG05 : MessageBlock // Need to add interface for BIRD System
//    {
//        public PGAPG05()
//            : base("PG05")
//        {
//        }

//        /// <summary>
//        /// Scientific Genus Name of the PGA’s merchandise being entered.
//        /// </summary>
//        [MessageBlockString(22, 5, "C")]
//        public ZString ScientificGenusName;

//        /// <summary>
//        /// Scientific Species Name of the PGA’s merchandise being entered.
//        /// </summary>
//        [MessageBlockString(22, 27, "C")]
//        public ZString ScientificSpeciesName;

//        /// <summary>
//        /// Scientific Sub Species Name of the PGA’s merchandise being entered.
//        /// </summary>
//        [MessageBlockString(18, 49, "C")]
//        public ZString ScientificSubSpeciesName;

//        /// <summary>
//        /// This includes Fish and Wildlife Service (FWS) Wildlife Category codes.
//        /// </summary>
//        [MessageBlockString(7, 67, "C")]
//        public ZString ScientificSpeciesCode;

//        /// <summary>
//        /// FWS Description Codes assigned by FWS.
//        /// </summary>
//        [MessageBlockString(7, 74, "C")]
//        public ZString FWSDescriptionCode;
//    }
//}