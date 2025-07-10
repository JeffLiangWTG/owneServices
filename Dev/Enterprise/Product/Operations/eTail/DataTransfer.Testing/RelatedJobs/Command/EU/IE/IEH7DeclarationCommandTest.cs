using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs.ASYCUDA;

namespace Enterprise.eTail.DataTransfer.Testing
{
	public class IEH7DeclarationCommandTest : BaseHVLVRelatedJobCommandTest
	{
		public void TestRelatedCustomsJobs_TheAsycudaManifestHeaderApplicationCodeShouldBeLVC()
		{
			var header = Factory.New<EUH7.IAsycudaManifestHeader>();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Ireland))
			{
				var command = GetCommandForTest();
				var consignmentHeader = command.Header;

				var typeDecider = ObjectFactory.Get<EUH7.IAsycudaManifestHeaderTypeDecider>();
				var row = ((INeedRow)header).Row;
				var typeForLoad = typeDecider.GetTypeForLoad(row, Factory);
				var manifestHeaderLVC = (EUH7.IAsycudaManifestHeader)Factory.New(typeForLoad);

				var manifestHeaderICS = Factory.New<EUICS2.IAsycudaManifestHeader>();

				var piovt1 = consignmentHeader.GenPivotCollection.AddNew();
				piovt1.Relation2Object = (BusinessObject)manifestHeaderLVC;
				var piovt2 = consignmentHeader.GenPivotCollection.AddNew();
				piovt2.Relation2Object = (BusinessObject)manifestHeaderICS;

				AssertEquals(1, command.ActiveRelatedCustomsJobs.Count());
			}
		}

		protected override string ExpectedRelatedJobName => "Low Value (H7)";

		protected override Type ExpectedRelatedJobConverterType => typeof(IEH7DeclarationConverter);

		protected override string[] ExpectedApplicableLoginCountries => new[] { CountryCodes.Ireland };

		protected override Directions[] ExpectedShipmentDirections => new[] { Directions.Import };

		protected override string[] ExpectedShipmentTransportModes => new[] { TransportModes.Air, TransportModes.Sea, TransportModes.Rail, TransportModes.Road };

		protected override bool ExpectedShouldValidateWaybill => false;

		protected override bool ExpectedShouldTrackPrimaryFieldChanges => false;

		protected override bool ExpectedNeedPreScreening => true;

		protected override string ExpectedUsageCode => "H7D";

		protected override string ExpectedUsageCategory => "LVD";

		protected override string ExpectedRequiredFeatureControlCode => LicenceFeatureCodeList.Codes.EcommerceH7Feature;

		protected override Type ExpectedCustomsJobsType => GetExpectedCustomsJobsType();

		protected override BaseHVLVRelatedJobCommand GetCommandForTest()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			HVLVConsignmentHeader.GetOrCreate(shipment);
			return new IEH7DeclarationCommand(shipment);
		}

		Type GetExpectedCustomsJobsType()
		{
			var header = Factory.New<EUH7.IAsycudaManifestHeader>();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Ireland))
			{
				var typeDecider = ObjectFactory.Get<EUH7.IAsycudaManifestHeaderTypeDecider>();
				var row = ((INeedRow)header).Row;
				return typeDecider.GetTypeForLoad(row, Factory);
			}
		}
	}
}
