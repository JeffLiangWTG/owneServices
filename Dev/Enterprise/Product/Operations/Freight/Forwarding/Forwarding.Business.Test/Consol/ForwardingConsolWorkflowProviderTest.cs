using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingConsol))]
	sealed class ForwardingConsolWorkflowProviderTest : WorkflowProviderTest<ForwardingConsol, ForwardingConsolProcessTaskCollection>
	{
		public void TestTemplateTasksAreNotCreatedWhenConsolForwardingFlagIsFalse()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = ExpectedWorkflowType;

			var milestone = template.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = "XXX";

			Factory.Save();

			var consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_IsForwarding = true;
			consol1.HasChanges = true;

			var consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_IsForwarding = false;
			consol2.HasChanges = true;

			Factory.Save();

			AssertEquals("Template tasks created", 1, consol1.WorkflowItems.Milestones.Count);
			AssertEquals("Template tasks NOT created when consol is not forward registered", 0, consol2.WorkflowItems.Milestones.Count);
		}

		[GuiTest]
		public void TestOnShipmentCountChangeLinkUpdate()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;

			var testHelper = ObjectFactory.Get<IBMTestHelper>();
			testHelper.CreateSystem(Factory, "SHP", "CON");
			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "CON";
			var templateJobHeader1 = template1.ProcessHeaders[0];

			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_ProcessType = "SHP";
			var templateJobHeader2 = template2.ProcessHeaders[0];

			var templateLink1 = (IProcessHeaderLink)template1.ProcessHeaderLinks.AddNew();
			templateLink1.FP_FH_HeaderFrom = templateJobHeader1.PK;
			templateLink1.FP_FH_HeaderTo = templateJobHeader2.PK;
			templateLink1.FP_LinkType = "DEP";
			templateLink1.FromWorkflowExternalTemplatePK = template2.PK;
			Factory.Save();

			var shipment = Factory.New<ForwardingShipment>();
			var consol = Factory.New<ForwardingConsol>();
			consol.Shipments.Add(shipment);

			Factory.Save();

			var shipmentJobHeader = ProcessJobHeaderProvider.GetForParent(shipment, Factory);
			var consolJobHeader = ProcessJobHeaderProvider.GetForParent(consol, Factory);

			testHelper.AssertIsPrerequisite(consolJobHeader, shipmentJobHeader);
			AssertEquals(false, consolJobHeader.Links.IsCountEqualTo(0));

			AssertEquals("The shipment should be consolidated, so it could not be directly deleted as a business object", true, shipment.JS_IsForwardRegistered);

			consol.Shipments.Remove(shipment);

			Factory.Save();

			AssertEquals(true, consolJobHeader.Links.IsCountEqualTo(0));
		}

		#region GetColumnValueRanker

		public void TestGetTemplateFilterCriteria_ForTransportMode()
		{
			Consol.Factory.Save();
			AssertGetTemplateFilterCriteria<ZString>(Consol.JK_TransportModeInfo, ProcessTaskTemplate.P0_SubType1Info, Core.Constants.TransportModes.Air, Core.Constants.TransportModes.Sea, ZString.Empty);
		}

		public void TestGetTemplateFilterCriteria_ForLoadPort()
		{
			Consol.Factory.Save();
			AssertGetTemplateFilterCriteria<ZString>(Consol.JK_RL_NKLoadPortInfo, ProcessTaskTemplate.P0_LoadPortCountryInfo, "AUSYD", "MYPKG", ZString.Empty);
		}

		public void TestGetTemplateFilterCriteria_ForDischargePort()
		{
			Consol.Factory.Save();
			AssertGetTemplateFilterCriteria<ZString>(Consol.JK_RL_NKDischargePortInfo, ProcessTaskTemplate.P0_DischargePortCountryInfo, "AUSYD", "MYPKG", ZString.Empty);
		}

		#endregion

		#region IWorkflowTriggerFieldChangeSource

		public void TestParentWorkflowProviders()
		{
			ForwardingShipment shipment1 = Consol.Shipments.AddNew();
			ForwardingShipment shipment2 = Consol.Shipments.AddNew();
			var relatedWorkflowProviders = ((IWorkflowTriggerFieldChangeSource)Consol).ParentWorkflowProviders;
			AssertEquals("2 related shipment IWorkflowProviders", 2, relatedWorkflowProviders.Count);
		}

		#endregion

		#region Implementation

		ForwardingConsol Consol
		{
			get { return BusinessObject; }
		}

		protected override ZString ExpectedWorkflowType
		{
			get { return JobInvoicingConsumerTypes.Consol.Code; }
		}

		#endregion
	}
}
