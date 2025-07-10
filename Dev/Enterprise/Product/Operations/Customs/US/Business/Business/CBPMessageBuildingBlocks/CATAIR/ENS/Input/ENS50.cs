using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Business.BIRD.Common;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input
{
	public partial class ENS50 : Messaging.Business.MessageBuildingBlocks.Input.Abstract.ENS50, IBIRDLineRecord
	{
		#region IBIRDLineRecord Members

		void IBIRDLineRecord.Update(JobComInvoiceLine invoiceLine, INotifications notifications)
		{
			IBIRDLineRecord record = this;

			record.SetTariffRelatedDetails(invoiceLine, Quantity1, UnitOfMeasure1, Quantity2, UnitOfMeasure2, Quantity3, UnitOfMeasure3);

			if (record.IsSupplementaryTariff())
			{
				if (CustomsValueDeciderForInvoiceLine.IsTariffValidFor98GoodsValue(TariffNumber1))
				{
					invoiceLine.US_98GoodsValue = invoiceLine.JI_LinePrice;
					invoiceLine.JI_LinePrice = 0m;
				}
			}

			invoiceLine.US_UC_NKCountryOfExport = CountryOfExport;

			invoiceLine.US_TransactionsRelated = RelatedPartyIndicator;

			var declaration = invoiceLine.Declaration;
			new BIRDUpdateHeaderHelperTool().UpdateAtFallbacksIfPossible(new ZPropertyInfo[] { declaration.JE_ExportDateInfo, declaration.US_DateOfExportInfo, invoiceLine.InvoiceHeader.US_DateOfExportInfo }, DateOfExportation.ToZDateTime(), invoiceLine.US_DateOfExportInfo);

			if (!SpecialProgramsIndicatorCountry.IsEmpty && !SpecialProgramsIndicatorPrimary.IsEmpty)
			{
				notifications.AddWarning(BothSPIPrimaryAndCountryExist);
			}

			invoiceLine.US_SPI = !SpecialProgramsIndicatorCountry.IsEmpty ? SpecialProgramsIndicatorCountry : SpecialProgramsIndicatorPrimary;
			if (invoiceLine.US_SPI.IsEmpty && invoiceLine.AddInfoLookups.SPIList.ContainsCode(SPICompleteList.MoreCodes.NotApplicable))
			{
				invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			}

			invoiceLine.US_SecondarySPI = SpecialProgramsIndicatorSecondary;

			invoiceLine.CusEntryLine.Fees.UpdateOrAddCharge(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, Duty);
		}

		public const string BothSPIPrimaryAndCountryExist = "Both SPI primary and SPI country exist in this 7501. SPI country is set at this line.";

		ZString IBIRDTariffRecord.Tariff
		{
			get { return TariffNumber1; }
		}

		#endregion

	}
}
