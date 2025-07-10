using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	public class ExportAWBHeaderFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public ExportAWBHeaderFetchStrategy(ExportAWBHeader exportAWBHeader)
			: base(exportAWBHeader)
		{
		}

		ExportAWBHeader AWBHeader => BusinessObject as ExportAWBHeader;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override void FetchForViewCore(TableColumn[] columns)
		{
			var refAirLineRequired = false;
			var exportAWBRateLineRequired = false;
			var exportAWBSecurityStatusLineRequired = false;
			var exportAWBSpecialHandlingRequired = false;
			var exportAWBOtherChargesRequired = false;

			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case ExportAWBHeader.Schema.EH_By1stAirlineName:
						refAirLineRequired = true;
						break;

					case ExportAWBHeader.Schema.EH_TotalGrossWeight:
					case ExportAWBHeader.Schema.EH_TotalLineTotals:
					case ExportAWBHeader.Schema.EH_TotalNoOfPieces:
						exportAWBRateLineRequired = true;
						break;

					case ExportAWBHeader.Schema.EH_SecurityStatus:
					case ExportAWBHeader.Schema.EH_SecurityStatusForNonBorrowedMAWBs:
						exportAWBSecurityStatusLineRequired = true;
						exportAWBSpecialHandlingRequired = true;
						break;

					case ExportAWBHeader.Schema.EH_TotalCOL:
					case ExportAWBHeader.Schema.EH_TotalPPD:
					case ExportAWBHeader.Schema.EH_OtherChargesDueAgentCOL:
					case ExportAWBHeader.Schema.EH_OtherChargesDueAgentPPD:
					case ExportAWBHeader.Schema.EH_OtherChargesDueCarrierCOL:
					case ExportAWBHeader.Schema.EH_OtherChargesDueCarrierPPD:
						exportAWBOtherChargesRequired = true;
						break;
				}
			}

			if (refAirLineRequired)
			{
				var query = new ZQuery(RefAirlineSchema.RM_TwoCharacterCode, AWBHeader.EH_By1st);
				Factory.AddFetchHint(typeof(RefAirline), query);
			}

			if (exportAWBRateLineRequired)
			{
				Factory.AddFetchHint(typeof(ExportAWBRateLine), ExportAWBRateLineSchema.ER_EH, BusinessObject.PK);
			}

			if (exportAWBSecurityStatusLineRequired)
			{
				Factory.AddFetchHint(typeof(ExportAWBSecurityStatusLine), ExportAWBSecurityStatusLineSchema.EAS_EH, BusinessObject.PK);
			}

			if (exportAWBSpecialHandlingRequired)
			{
				Factory.AddFetchHint(typeof(ExportAWBSpecialHandling), ExportAWBSpecialHandlingSchema.EP_EH, BusinessObject.PK);
			}

			if (exportAWBOtherChargesRequired)
			{
				Factory.AddFetchHint(typeof(ExportAWBOtherCharges), ExportAWBOtherChargesSchema.EO_EH, BusinessObject.PK);
			}

			base.FetchForViewCore(columns);
		}
	}
}
