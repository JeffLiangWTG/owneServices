using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgHeaderDocumentSupporter))]
	sealed class OrgHeaderDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestSupportedDataContext()
		{
			AssertEquals("DataContext.Organisation is supported", true, DocumentSupporter.IsDataContextSupported(new DataContextValue(nameof(DataContext.Organisation))));
			AssertEquals("DataContext.OrgSupplierLink is supported", true, DocumentSupporter.IsDataContextSupported(new DataContextValue(nameof(DataContext.OrgSupplierLink))));
			AssertEquals("DataContext.OrgBuyerLink is supported", true, DocumentSupporter.IsDataContextSupported(new DataContextValue(nameof(DataContext.OrgBuyerLink))));
			AssertEquals("DataContext.Notes is Supported", true, DocumentSupporter.IsDataContextSupported(new DataContextValue(nameof(DataContext.Notes))));
			AssertEquals("DataContext.OrganisationIRS1099 is supported", true, DocumentSupporter.IsDataContextSupported(new DataContextValue(nameof(DataContext.OrganisationIRS1099))));
			AssertEquals("DataContext.GenericFreightJob is supported", true, DocumentSupporter.IsDataContextSupported(new DataContextValue(nameof(DataContext.GenericFreightJob))));
		}

		public void TestGetDocBusinessObject()
		{
			DocumentWrapper[] wrappers = DocumentSupporter.GetDocumentWrappers(DataContext.Notes, null);
			AssertEquals(1, wrappers.Length);

			wrappers = DocumentSupporter.GetDocumentWrappers(DataContext.Organisation, null);
			AssertEquals(1, wrappers.Length);
		}

		public void TestGetDocWrapperForDocBuilderTemplates()
		{
			var menuItem = Factory.NewWithValidTestData<StmMenuItem>();
			menuItem.SU_MenuPath = "";

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var buyerHeader = Factory.NewWithValidTestData<OrgHeader>();
			buyerHeader.OH_IsForwarder = true;
			var unloco = Factory.NewWithValidTestData<RefUNLOCO>();
			buyerHeader.OH_RL_NKClosestPort = unloco.RL_Code;

			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			var orgAppointedAgent = Factory.New<OrgAppointedAgentPorts>();
			orgAppointedAgent.O5_PortOrCountry = unloco.RL_Code;
			orgAppointedAgent.O5_AgentDirection = "BTH";
			orgAppointedAgent.O5_SeaAgentStatus = "PUB";
			orgAppointedAgent.O5_OH = buyerHeader.PK;
			orgAppointedAgent.O5_OA_AgentOfficeAddress = orgAddress.PK;

			var supplierBuyer = orgHeader.BuyerLinks.AddNew(buyerHeader);
			var supBuyLinkMode = supplierBuyer.OrgSupBuyLinkTrnModes.AddNew();
			supBuyLinkMode.PF_TransportMode = AgentDirectionList.Codes.Export;
			supplierBuyer.SelectedForPrinting = true;
			Factory.Save();

			var supporter = new OrgHeaderDocumentSupporter(orgHeader);

			menuItem.SU_MenuName = "";
			supporter.GetDataStateBeforeRun(menuItem);
			var wrappers = supporter.GetDocumentWrappers(DataContext.GenericFreightJob, null);
			AssertEquals(1, wrappers.Length);

			menuItem.SU_MenuName = "Routing Recommendation To Consignee";
			supporter.GetDataStateBeforeRun(menuItem);
			wrappers = supporter.GetDocumentWrappers(DataContext.GenericFreightJob, null);
			AssertEquals(1, wrappers.Length);
		}

		public void TestGetDataStateBeforeRun_DoesntReturnNullForUnhandledMenuPath()
		{
			StmMenuItem menuItem = Factory.NewWithValidTestData<StmMenuItem>();
			menuItem.SU_MenuPath = "";
			AssertNotNull(DocumentSupporter.GetDataStateBeforeRun(menuItem));
			menuItem.SU_MenuPath = "This is a fake, unhandled menu item path";
			AssertNotNull(DocumentSupporter.GetDataStateBeforeRun(menuItem));
		}

		public void TestGetDataStateBeforeRun_SetPrintMenuItemCorrectly()
		{
			var menuItem = Factory.NewWithValidTestData<StmMenuItem>();
			menuItem.SU_MenuName = "";
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var supporter = new OrgHeaderDocumentSupporter(orgHeader);

			AssertNotNull(supporter.GetDataStateBeforeRun(menuItem));
			AssertEquals(OrgHeaderDocumentSupporter.PrintMenuItem.Default, supporter.menuItemPrinted);

			menuItem.SU_MenuName = "Routing Order";
			AssertNotNull(supporter.GetDataStateBeforeRun(menuItem));
			AssertEquals(OrgHeaderDocumentSupporter.PrintMenuItem.RoutingOrder, supporter.menuItemPrinted);

			menuItem.SU_MenuName = "Routing Recommendation To Consignee";
			AssertNotNull(supporter.GetDataStateBeforeRun(menuItem));
			AssertEquals(OrgHeaderDocumentSupporter.PrintMenuItem.RoutingRecommendationToConsignee, supporter.menuItemPrinted);

			menuItem.SU_MenuName = "Routing Recommendation To Consignor";
			AssertNotNull(supporter.GetDataStateBeforeRun(menuItem));
			AssertEquals(OrgHeaderDocumentSupporter.PrintMenuItem.RoutingRecommendationToConsignor, supporter.menuItemPrinted);

			menuItem.SU_MenuName = "Replacement Routing Order";
			AssertNotNull(supporter.GetDataStateBeforeRun(menuItem));
			AssertEquals(OrgHeaderDocumentSupporter.PrintMenuItem.ReplacementRoutingOrder, supporter.menuItemPrinted);

			supporter.BaseResultForTest = new DocumentSupporterDataState(false, "ERROR!!");
			var dataState = supporter.GetDataStateBeforeRun(menuItem);
			AssertNotNull(dataState);
			Assert("DataState should be not valid", !dataState.IsValid);
			AssertEquals("Error Message should be base error", "ERROR!!", dataState.ErrorMessage);
		}

		public void TestAgentReplacementRoutingOrder()
		{
			OrgHeader oldAgent = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeaderDocumentSupporter agentDocSupporter = (OrgHeaderDocumentSupporter)oldAgent.DocumentSupporter;

			oldAgent.OH_RL_NKClosestPort = "INBOM";
			oldAgent.OH_IsForwarder = true;
			OrgAppointedAgentPorts newAppAgPorts = oldAgent.AppointedAgentPorts.AddNew();
			newAppAgPorts.O5_PortOrCountry = "INBOM";
			newAppAgPorts.O5_AirAgentStatus = "PUB";
			newAppAgPorts.O5_OA_AgentOfficeAddress = oldAgent.MainAddress.PK;

			OrgHeader newAgent = Factory.NewWithValidTestData<OrgHeader>();
			newAgent.OH_RL_NKClosestPort = "INBOM";
			newAgent.OH_IsForwarder = true;

			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			consignor.OH_RL_NKClosestPort = "INBOM";

			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;
			OrgSupplierBuyerLink link = consignee.SupplierLinks.AddNew();
			link.OL_OH_Supplier = consignor.PK;
			link.OrgSupBuyLinkTrnModes[0].PF_TransportMode = "ALL";

			Factory.Save();

			AssertNull("Supplier link has no replacement agent", link.ReplacementAgent);

			agentDocSupporter.SetupReplacementAgentsForRoutingOrder();
			AssertNull("No replacement agent specified as event was not hooked up", link.ReplacementAgent);

			((OrgHeaderDocumentSupporter)oldAgent.DocumentSupporter).OnRoutingOrderReplacementPrinted += new OrgHeaderDocumentSupporter.RoutingOrderReplacementPrintedEventHandler((sender, e) => e.ReplacementAgent = newAgent);
			agentDocSupporter.SetupReplacementAgentsForRoutingOrder();
			AssertEquals("Replacement agent is set on the correct supplier link", newAgent, link.ReplacementAgent);

			oldAgent.AppointedAgentPorts[0].O5_PortOrCountry = "USLAX";
			Factory.Save();
			agentDocSupporter.SetupReplacementAgentsForRoutingOrder();
			AssertNull("No replacement agent specified as agent was not for the suppler's port", link.ReplacementAgent);

			oldAgent.AppointedAgentPorts[0].O5_PortOrCountry = string.Empty;
			Factory.Save();
			agentDocSupporter.SetupReplacementAgentsForRoutingOrder();
			AssertEquals("Replacement agent is set on the correct supplier link as the agent's port was same as supplier's port", newAgent, link.ReplacementAgent);

			oldAgent.OH_RL_NKClosestPort = "USLAX";
			Factory.Save();
			agentDocSupporter.SetupReplacementAgentsForRoutingOrder();
			AssertNull("No replacement agent specified as agent was not for the suppler's port", link.ReplacementAgent);

			oldAgent.AppointedAgentPorts[0].O5_PortOrCountry = "IN";
			Factory.Save();
			agentDocSupporter.SetupReplacementAgentsForRoutingOrder();
			AssertEquals("Replacement agent is set on the correct supplier link as the agent's published air COUNTRY contained the supplier's port", newAgent, link.ReplacementAgent);
		}

		public void TestJobRequiredDocumentDocumentSupporter()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				var documentSupporter = new OrgHeaderDocumentSupporter(Factory.New<OrgHeader>());
				AssertEquals("DataContext.TWLetterOfAuthorization is supported", true, documentSupporter.IsDataContextSupported(new DataContextValue(".TWLetterOfAuthorization")));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var documentSupporter = new OrgHeaderDocumentSupporter(Factory.New<OrgHeader>());
				AssertEquals("DataContext.TWLetterOfAuthorization is supported", true, documentSupporter.IsDataContextSupported(new DataContextValue(".TWLetterOfAuthorization")));
			}
		}

		#region Implementation

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "~CODE1";
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			orgHeader.OH_IsForwarder = true;

			var newAppAgPorts = orgHeader.AppointedAgentPorts.AddNew();
			newAppAgPorts.O5_PortOrCountry = "AUSYD";
			newAppAgPorts.O5_AirAgentStatus = AgentStatusList.Codes.Published;
			newAppAgPorts.O5_OA_AgentOfficeAddress = orgHeader.MainAddress.PK;

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "~CODE2";
			consignor.OH_IsConsignor = true;
			consignor.OH_RL_NKClosestPort = "AUSYD";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "~CODE3";
			consignee.OH_IsConsignee = true;
			consignee.OH_RL_NKClosestPort = "AUSYD";

			var supplierLink = orgHeader.SupplierLinks.AddNew();
			supplierLink.OL_OH_Supplier = consignor.PK;
			supplierLink.SelectedForPrinting = true;
			supplierLink.OrgSupBuyLinkTrnModes[0].PF_TransportMode = "ALL";

			var buyerLink = orgHeader.BuyerLinks.AddNew();
			buyerLink.OL_OH_Buyer = consignee.PK;
			buyerLink.SelectedForPrinting = true;
			buyerLink.OrgSupBuyLinkTrnModes[0].PF_TransportMode = "ALL";

			Factory.Save();

			return orgHeader;
		}

		OrgHeaderDocumentSupporter DocumentSupporter
		{
			get { return new OrgHeaderDocumentSupporter(Factory.New<OrgHeader>()); }
		}

		#endregion
	}
}
