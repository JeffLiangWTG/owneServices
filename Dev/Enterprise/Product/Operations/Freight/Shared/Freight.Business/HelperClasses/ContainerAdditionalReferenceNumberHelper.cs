using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public static class ContainerAdditionalReferenceNumberHelper
	{
		public static void PopulateCarrierMessageReferenceNumber(CommonContainer container, ZString referenceNumber)
		{
			var cmrReferenceNumber = FindCMRAdditionalReferenceNumber(container) ?? CreateCMRAdditionalReferenceNumber(container);
			cmrReferenceNumber.CE_EntryNum = referenceNumber;
		}

		static CusEntryNumber FindCMRAdditionalReferenceNumber(CommonContainer container)
		{
			return container.AdditionalReferenceNumbers.Find(num => num.CE_EntryType == ContainerNonCustomsAdditionalReferenceCodesCodeList.Codes.CarrierMessageReference
				&& num.CE_EntryIsSystemGenerated).FirstOrDefault();
		}

		static CusEntryNumber CreateCMRAdditionalReferenceNumber(CommonContainer container)
		{
			var result = container.AdditionalReferenceNumbers.AddNew();
			result.CE_EntryType = ContainerNonCustomsAdditionalReferenceCodesCodeList.Codes.CarrierMessageReference;
			result.CE_RN_NKCountryCode = GlbCompany.CurrentCompany?.GC_RN_NKCountryCode ?? ZString.Empty;
			result.CE_EntryIsSystemGenerated = true;

			result.SetReadOnlyIncludingChildren(true);

			return result;
		}
	}
}
