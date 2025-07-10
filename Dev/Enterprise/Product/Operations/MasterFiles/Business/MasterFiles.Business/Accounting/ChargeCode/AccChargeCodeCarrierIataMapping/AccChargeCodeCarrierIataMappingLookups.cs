using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class AccChargeCodeCarrierIataMappingLookups : AutoAccChargeCodeCarrierIataMappingLookups
	{
		public AccChargeCodeCarrierIataMappingLookups(AutoAccChargeCodeCarrierIataMapping parent) : base(parent)
		{
		}

		#region ACI_IATAChargeCodeMap_List

		public CodeDescriptionPairList ACI_IATAChargeCodeMap_List => new UntranslatableCodeDescriptionPairList((NoResString)"IATA AWB values cannot be translated", OLookUpEditType.AWBChargeCodes);

		#endregion
	}
}
