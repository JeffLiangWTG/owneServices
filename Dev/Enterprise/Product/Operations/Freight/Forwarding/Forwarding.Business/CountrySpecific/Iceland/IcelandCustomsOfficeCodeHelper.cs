using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

#if DEBUG
using CargoWise.Common.Testing;
#endif

namespace Enterprise.Freight.Forwarding.Business
{
#if DEBUG
	[SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
#endif
	internal static class IcelandCustomsOfficeCodeHelper
	{
		public static void ProcessIcelandCustomsOfficeCode(CusEntryNumAdditionalReferenceCollection numbers, ZString coc, bool allowToOverride)
		{
			if (GlbBranch.CurrentBranch.Country.Code == Core.Constants.CountryCodes.Iceland)
			{
				CusEntryNumber entry = FindIcelandCustomsHouseEntry(numbers);
				if (entry == null)
				{
					SetIcelandCustomsHouseCode(numbers, coc, ZString.Empty, allowToOverride);
				}
				else if (entry != null)
				{
					SetIcelandCustomsHouseEntryNumChangeability(entry, allowToOverride);
				}
			}
		}

		public static void SetIcelandCustomsHouseCode(CusEntryNumAdditionalReferenceCollection numbers, ZString coc,
				ZString cause, bool allowToOverride)
		{
			if (GlbBranch.CurrentBranch.Country.Code == Core.Constants.CountryCodes.Iceland)
			{
				CusEntryNumber result = FindIcelandCustomsHouseEntry(numbers);
				bool isNew = false;
				if (result == null)
				{
					result = numbers.AddNew();
					result.CE_EntryType = IcelandForwardingShipmentSupport.CustomsOfficeCode;
					result.CE_ParentTable = "JobShipment";
					isNew = true;
				}

				if (coc != result.CE_EntryNum)
				{
					result.CE_EntryNumInfo.ValueChanged -= CusEntryNumber_CE_EntryNum_ValueChanged;
					result.CE_EntryNum = coc;
					if (!isNew)
					{
						result.AdditionalCustomLogReferenceSuffix = cause;
					}
				}

				SetIcelandCustomsHouseEntryNumChangeability(result, allowToOverride);
			}
		}

		internal static CusEntryNumber FindIcelandCustomsHouseEntry(CusEntryNumAdditionalReferenceCollection numbers)
		{
			ZQuery query = new ZQuery(CusEntryNumSchema.CE_EntryType, IcelandForwardingShipmentSupport.CustomsOfficeCode);
			query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Iceland);
			CusEntryNumber[] results = (CusEntryNumber[])numbers.Find(query);
			return results.Length > 0 ? results[0] : null;
		}

		static void SetIcelandCustomsHouseEntryNumChangeability(CusEntryNumber entryNumber, bool allowToOverride)
		{
			if (entryNumber != null)
			{
				entryNumber.CE_EntryType_ReadOnly = true;
				entryNumber.CE_EntryNum_ReadOnly = !allowToOverride;
				entryNumber.CanDeleteHandler -= CusEntryNumber_CanDelete;
				entryNumber.CanDeleteHandler += CusEntryNumber_CanDelete;
				entryNumber.CE_EntryNumInfo.ValueChanged -= CusEntryNumber_CE_EntryNum_ValueChanged;
				entryNumber.CE_EntryNumInfo.ValueChanged += CusEntryNumber_CE_EntryNum_ValueChanged;
			}
		}

		static void CusEntryNumber_CE_EntryNum_ValueChanged(object sender, EventArgs e)
		{
			((CusEntryNumber)sender).AdditionalCustomLogReferenceSuffix = Res.GetString("d57e7e01-4a94-434a-be34-38cf911bad41", "due to manual override by user");
		}

		static void CusEntryNumber_CanDelete(object sender, CanDeleteCusEntryNumberEventArgs e)
		{
			e.CanDelete = false;
			e.ReasonForNotAbleToDelete = ResString.GetMultilingualString("f81cb178-2e36-4526-bfc7-2ac29f071349", "COC Number is created automatically and can't be deleted");
		}
	}
}
