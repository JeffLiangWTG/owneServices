using System;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;
using static Enterprise.eTail.Integration.HVLVConstants;

namespace Enterprise.eTail.DataTransfer.Testing
{
	public class CustomsDeclarationCommandTest : BaseHVLVRelatedJobCommandTest
	{
		protected override string ExpectedRelatedJobName => "Stand Alone Declaration";

		protected override string[] ExpectedApplicableLoginCountries => new[] { CountryCodes.Canada };

		protected override string ExpectedShipmentDestinationCountry => CountryCodes.Canada;

		protected override Directions[] ExpectedShipmentDirections => new[] { Directions.Import };

		protected override Type ExpectedRelatedJobConverterType => typeof(CustomsDeclarationConverter);

		protected override bool ExpectedShouldValidateWaybill => false;

		protected override bool ExpectedShouldTrackPrimaryFieldChanges => false;

		protected override bool ExpectedNeedPreScreening => false;

		protected override string ExpectedUsageCode => UsageCodes.ImportStandAloneDeclaration;

		protected override string ExpectedUsageCategory => UsageCategories.HighValueDeclaration;

		protected override Type ExpectedCustomsJobsType => typeof(JobDeclaration);

		protected override BaseHVLVRelatedJobCommand GetCommandForTest()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			HVLVConsignmentHeader.GetOrCreate(shipment);
			return new CustomsDeclarationCommand(shipment);
		}
	}
}
