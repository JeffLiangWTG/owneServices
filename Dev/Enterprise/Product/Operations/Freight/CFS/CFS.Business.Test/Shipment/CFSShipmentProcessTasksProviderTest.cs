using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(CFSShipment))]
	class CFSShipmentProcessTasksProviderTest : WorkflowProviderTest<CFSShipment, ProcessTaskCollection>
	{
		public void TestGetTemplateFilterCriteria()
		{
			AssertHasJobRelatedTemplateFilterCriteria((CFSShipment shipment, OrgHeader consignee, OrgHeader consignor, string origin, string dest) =>
			{
				shipment.ConsigneePK = consignee.PK;
				shipment.ConsignorPK = consignor.PK;
				shipment.JS_RL_NKOrigin = origin;
				shipment.JS_RL_NKDestination = dest;
			});

			var consigneePK = Shipment.ConsigneePK;
			var consignorPK = Shipment.ConsignorPK;
			var localChargePK = Shipment.Job.LocalChargesPK;

			var clientPK = Factory.New<OrgHeader>().PK;
			Shipment.JS_OH_HandledOnBehalfOfForwarder = clientPK;

			Shipment.JS_TransportMode = Constants.TransportModes.Sea;
			Shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			var ranker = (ColumnValueRanker)((IWorkflowProvider)Shipment).GetTemplateSelectionCriteria();
			AssertArrayEqualsByElements(new object[] { Constants.TransportModes.Sea, "" }, ranker.GetValues(ProcessTaskTemplateSchema.P0_SubType1));
			AssertArrayEqualsByElements(new object[] { Constants.ContainerModes.FCL, "" }, ranker.GetValues(ProcessTaskTemplateSchema.P0_SubType2));
			AssertArrayEqualsByElements(new object[] { clientPK, consigneePK, consignorPK, localChargePK, ZGuid.Empty }, ranker.GetValues(ProcessTaskTemplateSchema.P0_OH_Client));

			Shipment.JS_OH_HandledOnBehalfOfForwarder = ZGuid.Empty;
			ranker = (ColumnValueRanker)((IWorkflowProvider)Shipment).GetTemplateSelectionCriteria();
			AssertArrayEqualsByElements(new object[] { consigneePK, consignorPK, localChargePK, ZGuid.Empty }, ranker.GetValues(ProcessTaskTemplateSchema.P0_OH_Client));
		}

		public void TestShipmentIsForwardRegistered_ProcessTasksMustNotBeCreatedOnSave()
		{
			var workflowTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			workflowTemplate.P0_ProcessType = WorkflowType;

			workflowTemplate.WorkflowItems.AddNew();

			Factory.Save();

			var workflowItems = (Shipment as IWorkflowProvider).WorkflowItems;

			AssertEquals("No tasks initially", 0, workflowItems.Count);

			Shipment.JS_IsForwardRegistered = true;
			AssertEquals("Prerequisite", true, Shipment.HasChanges);

			Factory.Save();

			AssertEquals("Tasks should NOT be created from the template on save", 0, workflowItems.Count);
		}

		#region Implementation

		CFSShipment Shipment
		{
			get { return BusinessObject; }
		}

		protected override ZString ExpectedWorkflowType
		{
			get { return JobInvoicingConsumerTypes.CFSShipment.Code; }
		}

		protected override CFSShipment GetNewBusinessObject(BusinessObjectFactory factory)
		{
			CFSShipment result = base.GetNewBusinessObject(factory);

			// so that a department can be applied to the JobHeader
			result.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			result.JS_RL_NKDestination = "MYPKG";
			return result;
		}

		#endregion
	}
}
