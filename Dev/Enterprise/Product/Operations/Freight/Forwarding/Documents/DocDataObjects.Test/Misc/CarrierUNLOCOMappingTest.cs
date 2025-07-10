using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class CarrierUNLOCOMappingTest : TestCaseWithFactory
	{
		public void TestGetLocalCode()
		{
			var carrier = CreateOrgHeaderWithUNLOCOMappings();
			var carrierUNLOCOMapping = new CarrierUNLOCOMapping(Factory, carrier.PK);

			AssertEquals("Local code CNSHA for CNXXX", "CNSHA", carrierUNLOCOMapping.GetLocalCode("CNXXX"));
			AssertEquals("Local code AUSYD for AUYYY", "AUSYD", carrierUNLOCOMapping.GetLocalCode("AUYYY"));
			AssertEquals("Local code DEHAM for DEZZZ", "DEHAM", carrierUNLOCOMapping.GetLocalCode("DEZZZ"));

			AssertEquals("Not autocomplete by default for CNXX", "CNXX", carrierUNLOCOMapping.GetLocalCode("CNXX"));
			AssertEquals("Not autocomplete by default for CNXX", "CNSHA", carrierUNLOCOMapping.GetLocalCode("CNXX", true));

			AssertEquals("No mapping mapped for foreign code HKHKG, use original UNLOCO", "HKHKG", carrierUNLOCOMapping.GetLocalCode("HKHKG"));
		}

		public void TestGetForeignCode()
		{
			var carrier = CreateOrgHeaderWithUNLOCOMappings();
			var carrierUNLOCOMapping = new CarrierUNLOCOMapping(Factory, carrier.PK);

			AssertEquals("Foreign code CNXXX for CNSHA", "CNXXX", carrierUNLOCOMapping.GetForeignCode("CNSHA"));
			AssertEquals("Foreign code AUYYY for AUSYD", "AUYYY", carrierUNLOCOMapping.GetForeignCode("AUSYD"));
			AssertEquals("Foreign code DEZZZ for DEHAM", "DEZZZ", carrierUNLOCOMapping.GetForeignCode("DEHAM"));

			AssertEquals("Not autocomplete by default for CNXX", "CNSH", carrierUNLOCOMapping.GetForeignCode("CNSH"));
			AssertEquals("Not autocomplete by default for CNXX", "CNXXX", carrierUNLOCOMapping.GetForeignCode("CNSH", true));

			AssertEquals("No mapping mapped, use original UNLOCO", "HKHKG", carrierUNLOCOMapping.GetLocalCode("HKHKG"));
		}

		OrgHeader CreateOrgHeaderWithUNLOCOMappings()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var mapping1 = orgHeader.CreatePatternMatchOverrideForTest();
			SetupOrgPatternMatchOverride(mapping1, "CNXXX", "CNSHA");

			var mapping2 = orgHeader.CreatePatternMatchOverrideForTest();
			SetupOrgPatternMatchOverride(mapping2, "AUYYY", "AUSYD");

			var mapping3 = orgHeader.CreatePatternMatchOverrideForTest();
			SetupOrgPatternMatchOverride(mapping3, "DEZZZ", "DEHAM");

			return orgHeader;
		}

		void SetupOrgPatternMatchOverride(OrgPatternMatchOverride mapping, string foreignUNLOCO, string localUNLOCO)
		{
			mapping.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Port;
			mapping.OO_Context = Core.Constants.OrgPatternMatchOverrideContexts.Codes.OceanCarrierMessage;
			mapping.OO_ForeignCode = foreignUNLOCO;
			mapping.OO_LocalCode = localUNLOCO;
		}
	}
}
