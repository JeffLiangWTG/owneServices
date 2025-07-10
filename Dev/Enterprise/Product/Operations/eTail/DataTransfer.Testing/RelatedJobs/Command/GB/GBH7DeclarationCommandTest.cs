using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs.GB;

namespace Enterprise.eTail.DataTransfer.Testing
{
	public class GBH7DeclarationCommandTest : BaseHVLVRelatedJobCommandTest
	{
		public void TestRelatedCustomsJobs()
		{
			var command = GetCommandForTest();
			var consignmentHeader = command.Header;
			var manifestHeader1 = CreateManifestHeader(CountryCodes.Ireland);
			var manifestHeader2 = CreateManifestHeader(CountryCodes.UnitedKingdom);

			var pivot1 = consignmentHeader.GenPivotCollection.AddNew();
			pivot1.Relation2Object = (BusinessObject)manifestHeader1;
			var pivot2 = consignmentHeader.GenPivotCollection.AddNew();
			pivot2.Relation2Object = (BusinessObject)manifestHeader2;

			var relatedCustomsJobs = command.ActiveRelatedCustomsJobs;
			CombineAssertions("Should only contain one GB H7 manifest header in related customs jobs", () =>
			{
				AssertEquals(1, relatedCustomsJobs.Count());
				AssertEquals(manifestHeader2.PK, relatedCustomsJobs.Single().PK);
			});
		}

		protected override string ExpectedRelatedJobName => "Low Value (H7/BIRDS)";

		protected override Type ExpectedRelatedJobConverterType => typeof(GBH7DeclarationConverter);

		protected override string[] ExpectedApplicableLoginCountries => new[] { CountryCodes.UnitedKingdom };

		protected override Directions[] ExpectedShipmentDirections => new[] { Directions.Import };

		protected override string[] ExpectedShipmentTransportModes => new[] { TransportModes.Air, TransportModes.Sea, TransportModes.Rail, TransportModes.Road };

		protected override bool ExpectedShouldValidateWaybill => false;

		protected override bool ExpectedShouldTrackPrimaryFieldChanges => false;

		protected override bool ExpectedNeedPreScreening => true;

		protected override string ExpectedUsageCode => "H7D";

		protected override string ExpectedUsageCategory => "LVD";

		protected override string ExpectedRequiredFeatureControlCode => LicenceFeatureCodeList.Codes.EcommerceH7Feature;

		protected override Type ExpectedCustomsJobsType => ObjectFactory.GetType<GBH7.IAsycudaManifestHeader>();

		protected override BaseHVLVRelatedJobCommand GetCommandForTest()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			HVLVConsignmentHeader.GetOrCreate(shipment);
			return new GBH7DeclarationCommand(shipment);
		}

		Enterprise.Integration.Customs.ASYCUDA.EUH7.IAsycudaManifestHeader CreateManifestHeader(string countryCode)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				return Factory.New<Enterprise.Integration.Customs.ASYCUDA.EUH7.IAsycudaManifestHeader>();
			}
		}
	}
}
