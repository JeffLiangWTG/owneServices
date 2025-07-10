using System;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Integration;

namespace Enterprise.Customs.NZ.Business.Declaration.InterfaceImplementations
{
	class CusEntryHeaderCustomsCharges : Customs.Business.InterfaceImplementations.CusEntryHeaderCustomsCharges
	{
		public CusEntryHeaderCustomsCharges(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
		}

		CusEntryHeader EntryHeader
		{
			get { return (CusEntryHeader)entryHeader; }
		}

		protected override Accounting.Integration.CustomsCharge[] GetCustomsCharges(ILogger logger)
		{
			if (!EntryHeader.CH_IsActive)
			{
				return Array.Empty<Accounting.Integration.CustomsCharge>();
			}
			return base.GetCustomsCharges(logger);
		}

		protected override CargoWise.Types.ZGuid GetAccChargeCode(Enterprise.Registry.Business.Customs.EntryChargeType chargeType)
		{
			if (EntryHeader.Declaration.IsExport && (chargeType.Code == EntryChargeTypeList.Codes.EntryFee || chargeType.Code == EntryChargeTypeList.Codes.EntryFeeGST))
			{
				return NZCustomsDataRegistry.Instance.ExportEntryFeeChargeCode.GetValueWithoutFallback(EntryHeader.RegistryCompanyPK, Guid.Empty, Guid.Empty);
			}
			return base.GetAccChargeCode(chargeType);
		}
	}
}
