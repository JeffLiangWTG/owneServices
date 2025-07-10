// See HTSV56789ABCEFGHIJK

//namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
//{
//    using CargoWise.Types;

//    [OutputBlock("VC")]
//    public partial class HTSVC : MessageBlock
//    {
//        public HTSVC()
//            : base("VC")
//        {
//        }

//        /// <summary>
//        /// A code located in the Harmonized Tariff Schedule of the United States Annotated (HTS) representing the tariff number. If this number contains less than 10 positions, it is left justified. This number is the same as the record reported in Record Identifier V1.
//        /// </summary>
//        [MessageBlockString(10, 3, "M")]
//        public ZString TariffNumber;

//        /// <summary>
//        /// A code representing the country. Valid ISO country codes are listed in Appendix B of this publication. E followed by a space (Caribbean Basin Initiative), and J followed by a space (Andian Trade Preference Act), and R followed by a space (Caribbean Trade Partnership Act), are also valid codes for special rates. Countries eligible for E and J are indicated in the ACS country code file and the Harmonized Tariff Schedule of the United States - Annotated (HTS).
//        /// </summary>
//        [MessageBlockString(2, 13, "C")]
//        public ZString InternationalOrganizationForStandardizationISOCountryCode8;

//        /// <summary>
//        /// The specific rate of duty listed in the Special column of the HTS. Eight decimal places are implied.
//        /// </summary>
//        [MessageBlockDecimal(12, 15, "C", 8)]
//        public ZDecimal SpecificSpecialRate8;

//        /// <summary>
//        /// The ad valorem rate of duty listed in the Special column of the HTS. Eight decimal places are implied.
//        /// </summary>
//        [MessageBlockDecimal(12, 27, "C", 8)]
//        public ZDecimal AdValoremSpecialRate8;

//        /// <summary>
//        /// The rate of duty listed in the Special column of the HTS that is not a specific or ad valorem rate. Eight decimal places are implied.
//        /// </summary>
//        [MessageBlockDecimal(12, 39, "C", 8)]
//        public ZDecimal OtherSpecialRate8;

//        /// <summary>
//        /// A code that indicates the tax/fee class. Valid tax/fee class codes are listed in Appendix B of this publication.
//        /// </summary>
//        [MessageBlockString(3, 51, "C")]
//        public ZString TaxFeeClassCode8;

//        /// <summary>
//        /// A code that indicates the tax/fee computation formula. Computation formulas are presented in Appendix F of this publication.
//        /// </summary>
//        [MessageBlockString(1, 54, "C")]
//        public ZString TaxFeeComputationCode8;

//        /// <summary>
//        /// A code that indicates if a tax/fee is required. Valid Tax/Fee Flag Codes are:
//        /// 
//        /// 1 = Tax/fee required
//        /// 2 = Tax/fee may be required
//        /// </summary>
//        [MessageBlockString(1, 55, "C")]
//        public ZString TaxFeeFlag8;

//        /// <summary>
//        /// The specific rate required to compute taxes and/or fees. Eight decimal places are implied.
//        /// </summary>
//        [MessageBlockDecimal(12, 56, "C", 8)]
//        public ZDecimal TaxFeeSpecificRate8;

//        /// <summary>
//        /// The ad valorem rate required to compute taxes and/or fees. Eight decimal places are implied.
//        /// </summary>
//        [MessageBlockDecimal(12, 68, "C", 8)]
//        public ZDecimal TaxFeeAdValorem8;
//    }
//}
