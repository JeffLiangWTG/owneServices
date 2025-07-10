using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSupBuyLinkTrnModeDependentCollection))]
	sealed class OrgSupBuyLinkTrnModeDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new OrgSupBuyLinkTrnModeDependentCollection(OrgSupplierBuyerLink, Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<OrgSupBuyLinkTrnMode>();
		}

		#region TestFind

		public void TestFind()
		{
			OrgSupBuyLinkTrnMode linkMode1 = OrgSupplierBuyerLink.OrgSupBuyLinkTrnModes[0];
			linkMode1.PF_TransportMode = "";
			AssertNull("Should not find", OrgSupplierBuyerLink.OrgSupBuyLinkTrnModes.Find("SEA", ""));

			linkMode1.PF_TransportMode = "SEA";
			linkMode1.PF_ContainerMode = "";
			AssertEquals("Should find SEA linkMode", linkMode1.PK, OrgSupplierBuyerLink.OrgSupBuyLinkTrnModes.Find("SEA", "ABC").PK);

			OrgSupBuyLinkTrnMode linkModeSeaLCL = OrgSupplierBuyerLink.OrgSupBuyLinkTrnModes.AddNew();
			linkModeSeaLCL.PF_TransportMode = "SEA";
			linkModeSeaLCL.PF_ContainerMode = "LCL";

			OrgSupBuyLinkTrnMode linkModeRoadFCL = OrgSupplierBuyerLink.OrgSupBuyLinkTrnModes.AddNew();
			linkModeRoadFCL.PF_TransportMode = "ROA";
			linkModeRoadFCL.PF_ContainerMode = "FCL";

			AssertEquals("should find SEA linkMode", linkMode1.PK, OrgSupplierBuyerLink.OrgSupBuyLinkTrnModes.Find("SEA", "FCL").PK);
			AssertEquals("should find SEA+LCL linkMode", linkModeSeaLCL.PK, OrgSupplierBuyerLink.OrgSupBuyLinkTrnModes.Find("SEA", "LCL").PK);
			AssertEquals("should find ROA+FCL linkMode", linkModeRoadFCL.PK, OrgSupplierBuyerLink.OrgSupBuyLinkTrnModes.Find("ROA", "").PK);

			OrgSupBuyLinkTrnMode linkModeSeaFCL = OrgSupplierBuyerLink.OrgSupBuyLinkTrnModes.AddNew();
			linkModeSeaFCL.PF_TransportMode = "SEA";
			linkModeSeaFCL.PF_ContainerMode = "FCL";
			AssertEquals("should find SEA+FCL linkMode", linkModeSeaFCL.PK, OrgSupplierBuyerLink.OrgSupBuyLinkTrnModes.Find("SEA", "FCL").PK);
			AssertEquals("should find SEA+LCL linkMode", linkModeSeaLCL.PK, OrgSupplierBuyerLink.OrgSupBuyLinkTrnModes.Find("SEA", "LCL").PK);
			AssertEquals("should find ROA+FCL linkMode", linkModeRoadFCL.PK, OrgSupplierBuyerLink.OrgSupBuyLinkTrnModes.Find("ROA", "").PK);

			OrgSupBuyLinkTrnMode linkModeAIR = OrgSupplierBuyerLink.OrgSupBuyLinkTrnModes.AddNew();
			linkModeAIR.PF_TransportMode = "AIR";
			AssertEquals("Should find AIR linkMode", linkModeAIR.PK, OrgSupplierBuyerLink.OrgSupBuyLinkTrnModes.Find("AIR", "").PK);

			linkMode1.PF_TransportMode = "AIR";
			linkMode1.PF_ContainerMode = Core.Constants.ContainerModes.Loose;
			AssertEquals("Should find AIR+LSE linkMode", linkMode1.PK, OrgSupplierBuyerLink.OrgSupBuyLinkTrnModes.Find("AIR", Core.Constants.ContainerModes.Loose).PK);

			linkMode1.PF_TransportMode = "ALL";
			AssertEquals("Should find ALL linkMode(closest match), no RAI exists", linkMode1.PK, OrgSupplierBuyerLink.OrgSupBuyLinkTrnModes.Find("RAI", "").PK);
		}

		#endregion

		#region Implementation

		OrgSupplierBuyerLink OrgSupplierBuyerLink
		{
			get { return orgSupplierBuyerLink ?? (orgSupplierBuyerLink = Factory.New<OrgSupplierBuyerLink>()); }
		}
		OrgSupplierBuyerLink orgSupplierBuyerLink;

		#endregion
	}
}
