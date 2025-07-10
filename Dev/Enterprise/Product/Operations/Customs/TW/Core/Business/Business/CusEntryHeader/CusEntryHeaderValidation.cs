using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public class CusEntryHeaderValidation : AutoTWCusEntryHeaderValidation
	{
		public CusEntryHeaderValidation(CusEntryHeader parent)
			: base(parent)
		{
		}

		public new CusEntryHeader Parent => (CusEntryHeader)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateCH_TotalNetWeightInKilograms();
		}

		public void ValidateCH_TotalNetWeightInKilograms()
		{
			ValidateCalculatedProperty(Parent.CH_TotalNetWeightInKilogramsInfo);
		}

		protected void CheckCH_TotalNetWeightInKilograms()
		{
			if (Parent.IsTotalNetWeightGreaterThanTotalGrossWeight)
			{
				Parent.CH_TotalNetWeightInKilogramsInfo.AddMessageError(ValidationConstants.CusEntryHeader.NetWeightNotBeGreaterThanGrossWeight);
			}
		}

		protected override void CheckCH_DeclarationIncoterm()
		{
			base.CheckCH_DeclarationIncoterm();
			MandatoryValidation.CheckEntered(Parent.CH_DeclarationIncotermInfo);
			var declarationIncoterm = Parent.CH_DeclarationIncoterm;
			if (!declarationIncoterm.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.CH_DeclarationIncotermInfo, Parent.Lookups.IncoTermList);
				CheckIncoTermByCharges(declarationIncoterm);
			}
		}

		void CheckIncoTermByCharges(ZString declarationIncoterm)
		{
			var entryHeader = Parent;
			var isExport = entryHeader.IsExport;
			if (isExport)
			{
				switch (declarationIncoterm)
				{
					case Core.Constants.IncoTerms.ExWorks:
						if (!entryHeader.CH_TotalInternationalFreightAmountInInvoiceCurrency.IsEmpty || !entryHeader.CH_TotalInternationalInsuranceAmountInInvoiceCurrency.IsEmpty)
						{
							Parent.CH_DeclarationIncotermInfo.AddWarning(Res.GetString("7d89135a-e0d6-47e9-a9d0-60d4dee6fd1a", "依據預報貨物通關報關手冊之規定，實際交易條件為EXW，僅為貨物出廠價格者，運費(17)、保險費(18)欄不得填列。"));
						}
						break;
					case Core.Constants.IncoTerms.CostInsuranceAndFreight:
						if (entryHeader.CH_TotalInternationalFreightAmountInInvoiceCurrency.IsEmpty || entryHeader.CH_TotalInternationalInsuranceAmountInInvoiceCurrency.IsEmpty)
						{
							Parent.CH_DeclarationIncotermInfo.AddWarning(Res.GetString("0eebaea5-357a-4165-849b-064e2b23ccd6", "依據預報貨物通關報關手冊之規定，實際交易條件為CIF，除貨物本身之離岸價格外，包含該筆交易中離岸後之保險費用及運輸費用者，運費(17)欄及保險費(18)欄應填報。"));
						}
						break;
					default:
						break;
				}
			}
		}
	}
}
