using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.Testing
{
	sealed class LineLevelInformationForTest : ILineLevelInformation
	{
		public LineLevelInformationForTest()
		{
			DateOfAssessment = ZDateTime.Today;
		}

		public BusinessObjectFactory Factory { get; set; }

		public ZDateTime DateOfAssessment { get; set; }

		public ZDecimal ActualPrice { get; set; }

		public IEnumerable<IAdditionalInformation> AdditionalInformations { get; set; }

		public ZDecimal AdditionalQuantity { get; set; }

		public ZString AdditionalUnitQty { get; set; }

		public ZDecimal ClassificationQuantity { get; set; }

		public ZString ClassificationUnitQty { get; set; }

		public ZString CountryOfOrigin { get; set; }

		public ZString CustomsProcedureCode { get; set; }

		public ZDecimal CustomsQuantity { get; set; }

		public ZString CustomsUnitQty { get; set; }

		public ZDecimal CustomsValue { get; set; }

		public IEnumerable<IDutyFeeInformation> DutiesAndFees { get; set; }

		public IEnumerable<IDutyFeeInformation> ProvisionalPayments { get; set; }

		public ZString GoodsDescription { get; set; }

		public ZString LineNumber { get; set; }

		public ZString PreferenceCode { get; set; }

		public ZString PreviousProcedureCode { get; set; }

		public ZString PreviousProcedureMRN { get; set; }

		public ZString PrimaryPreference { get; set; }

		public ZString ProcedureMeasure { get; set; }

		public ZString TariffCode { get; set; }

		public ZString TradeStatisticsIndicator { get; set; }

		public ZDecimal WarehouseCountableQuantity { get; set; }

		public ZString WarehouseCountableUnitQty { get; set; }

		public ZString RebateUserCode { get; set; }

		public ZString WarehousingMRNLineNumber { get; set; }
	}
}
