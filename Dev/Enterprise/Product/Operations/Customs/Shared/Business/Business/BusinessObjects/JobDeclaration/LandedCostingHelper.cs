using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public abstract class LandedCostingHelper
	{
		public DutyTaxEntryFee GetTotalDutyTaxEntryFeeItems(BaseJobDeclaration declaration) { return GetTotalDutyTaxEntryFeeItemsCore(declaration); }
		public DutyTaxEntryFee GetLineDutyTaxEntryFeeItems(BaseJobComInvoiceLine invoiceLine) { return GetLineDutyTaxEntryFeeItemsCore(invoiceLine); }

		protected abstract DutyTaxEntryFee GetTotalDutyTaxEntryFeeItemsCore(BaseJobDeclaration declaration);
		protected abstract DutyTaxEntryFee GetLineDutyTaxEntryFeeItemsCore(BaseJobComInvoiceLine invoiceLine);
	}

	public class IntegratedCountryLandedCostingHelper : LandedCostingHelper
	{
		public static ZDecimal GetDutyAmount(CusEntryLine entryLine)
		{
			Argument.NotNull(entryLine, "entryLine");
			var entryHeader = entryLine.Header;
			var declaration = entryLine.Declaration;

			if (declaration != null && entryHeader != null && declaration.IsDeclarationIntegrated)
			{
				var dutyCode = IntegratedCountryHelper.CustomsWareInstallations(declaration.Country.RN_Code) ? FeeTypeList.Codes.A00 : entryHeader.DutyCode;
				return entryLine.Fees.GetAmount(dutyCode);
			}

			return new ZDecimal(0);
		}

		protected override DutyTaxEntryFee GetTotalDutyTaxEntryFeeItemsCore(BaseJobDeclaration declaration)
		{
			var result = new DutyTaxEntryFee();

			foreach (CusEntryHeader entryHeader in declaration.ActiveEntryHeaders)
			{
				foreach (var entryLine in entryHeader.MergedLines)
				{
					result[DutyFeeType] += GetDutyAmount(entryLine);
				}
			}
			return result;
		}

		protected override DutyTaxEntryFee GetLineDutyTaxEntryFeeItemsCore(BaseJobComInvoiceLine invoiceLine)
		{
			var fee = new DutyTaxEntryFee();
			fee[DutyFeeType] = invoiceLine.CanPerformApportionmentOfCusEntryLineValues ? invoiceLine.JI_Calc_DutyAmount : ZDecimal.Zero;

			return fee;
		}

		public const string DutyFeeType = "TDT";

		public static IEnumerable<ICustomsChargeLCItemSetting> GetCustomsChargeLCItemSettings()
		{
			yield return new LandedLineCostItemSetting() { CostType = DutyFeeType, Description = Res.GetString("ca9a1a66-e7a2-4f30-8b57-4e53ffe09295", "Total Duty"), NumberOfDecimals = 2, IsDuty = true };
		}
	}
}
