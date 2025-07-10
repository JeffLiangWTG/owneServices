using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public static class ConsolAdditionalReferenceNumberHelper
	{
		public static void PopulateCarrierMessageReferenceNumber(CommonConsol consol, ZString referenceNumber)
		{
			var cmrReferenceNumber = FindCMRAdditionalReferenceNumber(consol) ?? CreateCMRAdditionalReferenceNumber(consol);
			cmrReferenceNumber.CE_EntryNum = referenceNumber;
		}

		static CusEntryNumber FindCMRAdditionalReferenceNumber(CommonConsol consol)
		{
			return consol.Numbers.Find(num => num.CE_EntryType == ConsolNonCustomsAdditionalReferenceCodesCodeList.Codes.CarrierMessageReference
				&& num.CE_EntryIsSystemGenerated).FirstOrDefault();
		}

		static CusEntryNumber CreateCMRAdditionalReferenceNumber(CommonConsol consol)
		{
			var result = consol.Numbers.AddNew();
			result.CE_EntryType = ConsolNonCustomsAdditionalReferenceCodesCodeList.Codes.CarrierMessageReference;
			result.CE_RN_NKCountryCode = GlbCompany.CurrentCompany?.GC_RN_NKCountryCode ?? ZString.Empty;
			result.CE_EntryIsSystemGenerated = true;

			result.SetReadOnlyIncludingChildren(true);

			return result;
		}
	}
}
