using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	class USExportAsycudaBillValidationHelper
	{
		public static void CheckITNAndExemptionCodeAndInBondNumber(USExportAsycudaBill bill, ZPropertyInfo propertyInfo)
		{
			if (bill.Header is USExportAsycudaManifestHeader header && header.IsConsolidator && bill.PK != header.MasterBill.PK
				&& bill.AESITNNumbers.IsEmpty && bill.InBondNumbers.IsEmpty && bill.ABL_UCRNumber.IsEmpty)
			{
				propertyInfo.AddMessageError(ITNOrExemptionCodeOrInBondNumberMustBeProvided);
			}
		}

		public static void AddNotificationFromCusEntryNumber(CusEntryNumCollection entryNumbers, ZPropertyInfo propertyInfo)
		{
			foreach (CusEntryNumber entryNumber in entryNumbers)
			{
				entryNumber.Validation.ValidateCE_EntryNum();
				if (entryNumber.CE_EntryNumInfo.HasNotifications())
				{
					propertyInfo.AddAllNotificationsFrom(entryNumber.CE_EntryNumInfo);
					break;
				}
			}
		}

		static ZString ITNOrExemptionCodeOrInBondNumberMustBeProvided
		{
			get { return Res.GetString("6DA4AE56-7460-42DA-A22D-F092E9A1C58E", "An AES ITN or an AES Exemption Code or an In-Bond Number must be provided"); }
		}
	}
}
