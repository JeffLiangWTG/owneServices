using System.Linq;
using Enterprise.Customs.NZ.Business.Declaration.CustomsChargeCalculators;
using Enterprise.Customs.Universal;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.NZ.Business.Declaration.FormalEntry
{
	public class LineMerger : Customs.Business.LineMerger
	{
		public LineMerger(JobDeclaration declaration)
			: base(declaration)
		{
		}

		public new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
		}

		protected override Customs.Business.EntryCreationStrategy[] GetEntryCreationStrategies()
		{
			return new EntryCreationStrategy[] { new EntryCreationStrategy(this) };
		}

		#region CalculateDuties

		void CreateOtherInfosIfNecessary()
		{
			var declaration = Declaration;
			var header = declaration.CusEntryHeader;

			if (declaration.IsTSWImportDeclaration)
			{
				var taxOrFee = new RefCusTaxOrFee.Loader(declaration.Factory).LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.NewZealand, RateTypes.Deminimus, declaration.JE_DateOfArrival);
				if (taxOrFee != null && header.VFDWholeNZD <= taxOrFee.ZZF_Value)
				{
					foreach (JobComInvoiceLine invoiceLine in declaration.InvoiceLines)
					{
						if (invoiceLine.IsTabaccoOrAlcoholic && !invoiceLine.OtherInfos.Cast<LineOtherInfo>().Any(x => x.IsLVX))
						{
							invoiceLine.OtherInfos.AddNew(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NZLowValueGoodsExclusion, "");
						}
					}
				}
			}
		}

		protected override void CalculateDuties()
		{
			CreateOtherInfosIfNecessary();

			CusEntryHeader header = (CusEntryHeader)Declaration.CusEntryHeader;
			foreach (CusEntryLine entryLine in header.MergedLines)
			{
				entryLine.CopyInUserEnteredCustomsChargesAndCredits();
				entryLine.CalculateDutyAndGST();
			}

			if (!header.CH_EntryChargeWaived && NeedsToSetEntryFeeOnDeclaration)
			{
				new EntryFeeCalculator(Declaration).SetEntryFeesAndLeviesOn(header);
			}

			new LineDutyApportionManager().Apportion(Declaration);
		}

		protected override void PerformCountrySpecificOperationAfterMergeBeforeCalculateDuty()
		{
			base.PerformCountrySpecificOperationAfterMergeBeforeCalculateDuty();

			var declaration = Declaration;
			foreach (CusEntryHeader header in declaration.ActiveEntryHeaders)
			{
				foreach (CusEntryLine line in header.MergedLines)
				{
					line.CL_CustomsValue = line.CL_CustomsValue.Round(declaration.IsImport ? 0 : line.CurrencyConverter.LocalCurrency.Decimals);
				}
			}
		}

		#endregion
	}
}
