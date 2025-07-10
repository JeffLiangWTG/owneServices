using CargoWise.Types;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.DataTransfer.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public sealed class ConsolCO2eResponseCodeMappingTest : CO2eResponseCodeMappingTest
	{
		public void TestCO2eResponseIsNotRejected()
		{
			var orgProxy = Factory.BOFactory.NewWithValidTestData<OrgHeader>();
			Factory.SaveForTesting();

			using (SetupCurrentBranchWithOrgProxy(orgProxy))
			{
				var consol = (ForwardingConsol)CO2eTestHelper.CreateForwardingConsolWithLegs(Factory.BOFactory);
				consol.JK_UniqueConsignRef = "C0001";
				var carrier1 = Factory.BOFactory.NewWithValidTestData<OrgHeader>();
				consol.Transports[0].JW_OA_CarrierAddress = carrier1.MainAddress.PK;
				Factory.SaveForTesting();

				var dataObject = CO2eTestHelper.GetSampleCO2eResponseDataObject();
				dataObject.TransportLegCollection[0].Carrier = new OrganizationAddress
				{
					AddressType = "Carrier",
					OrganizationCode = carrier1.OH_Code
				};
				dataObject.DataContext.AddDataTarget(DataContextType.ForwardingConsol, "C0001");
				Assert("Pre-condition", consol.IsCO2eResponseApplicable(dataObject));

				var carrier2 = Factory.BOFactory.NewWithValidTestData<OrgHeader>();
				var unlocoLoader = new RefUNLOCO.Loader(Factory.BOFactory);
				CreateMappings(orgProxy, new (string Relationship, string ForeignCode, ZGuid LocalGuid)[]
				{
					(Core.Constants.OrgPatternMatchOverrideRelationships.Organisation, carrier1.OH_Code, carrier2.PK),
					(Core.Constants.OrgPatternMatchOverrideRelationships.Port, "AUSYD", unlocoLoader.Load("AUMEL").PK)
				});

				Factory.SaveForTesting();
				ProcessUniversalShipmentAndAssert(dataObject, "ForwardingConsol");
			}
		}
	}
}
