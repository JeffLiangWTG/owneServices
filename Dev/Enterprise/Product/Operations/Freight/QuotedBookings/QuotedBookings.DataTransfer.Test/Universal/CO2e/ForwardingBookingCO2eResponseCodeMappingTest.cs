using CargoWise.Types;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Universal.Test
{
	public sealed class ForwardingBookingCO2eResponseCodeMappingTest : CO2eResponseCodeMappingTest
	{
		public void TestCO2eResponseIsNotRejected()
		{
			var orgProxy = Factory.BOFactory.NewWithValidTestData<OrgHeader>();
			Factory.SaveForTesting();

			using (SetupCurrentBranchWithOrgProxy(orgProxy))
			{
				var quotedBooking = CO2eTestHelper.CreateQuotedBooking(Factory.BOFactory, sailingLoad: "AUSYD", sailingDischarge: "VNVNH");
				quotedBooking.Booking.JS_UniqueConsignRef = "S0001";
				var carrier1 = Factory.BOFactory.NewWithValidTestData<OrgHeader>();
				quotedBooking.Booking.Sailing.Voyage.JV_OH_Line = carrier1.PK;
				Factory.SaveForTesting();

				var dataObject = CO2eTestHelper.GetSampleCO2eResponseDataObjectForQuotedBooking();
				dataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>()
				{
					new TransportLeg
					{
						PortOfLoading = new UNLOCO { Code = "AUSYD" },
						PortOfDischarge = new UNLOCO { Code = "VNVNH" },
						TransportMode = TransportMode.Sea,
						GreenhouseGasEmission = new GreenhouseGasEmission
						{
							CO2ePerTonne = 10m, CO2ePerTonneUnit = new UnitOfWeight { Code = "T" }
						},
						LegOrder = 0,
						Carrier = new OrganizationAddress
						{
							AddressType = "Carrier",
							OrganizationCode = carrier1.OH_Code
						}
					}
				});
				dataObject.DataContext.AddDataTarget(DataContextType.ForwardingBooking, "S0001");
				Assert("Pre-condition", quotedBooking.IsCO2eResponseApplicable(dataObject));
				var carrier2 = Factory.BOFactory.NewWithValidTestData<OrgHeader>();
				var unlocoLoader = new RefUNLOCO.Loader(Factory.BOFactory);
				CreateMappings(orgProxy, new (string Relationship, string ForeignCode, ZGuid LocalGuid)[]
				{
					(Core.Constants.OrgPatternMatchOverrideRelationships.Organisation, carrier1.OH_Code, carrier2.PK),
					(Core.Constants.OrgPatternMatchOverrideRelationships.Port, "AUSYD", unlocoLoader.Load("AUMEL").PK)
				});

				Factory.SaveForTesting();
				ProcessUniversalShipmentAndAssert(dataObject, "QuotedBooking");
			}
		}
	}
}
