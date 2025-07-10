using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Security;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Module.Testing
{
	[TestedType(typeof(WhsTransitDispatchConsignmentModule))]
	public class WhsTransitDispatchConsignmentModuleTest : WhsTransitModuleTest<WhsTransitDispatchConsignmentModule>
	{
		protected override bool ExpectedAllowEdit => true;
		protected override bool ExpectedAllowView => true;

		protected override Type ExpectedFilterBusinessObjectType => typeof(WhsTransitDispatchConsignmentFilterBusinessObject);

		protected override Type ExpectedFilterControlType => typeof(WhsTransitDispatchConsignmentFilterControl);

		protected override Type ExpectedCollectionType => typeof(WhsItemDispatchConsignmentCollection);

		protected override string ExpectedWarehouseSchema => WhsTransitDispatchConsignmentFilterBusinessObject.Schema.Warehouse;

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.WhsTransitDispatchConsignment;

		protected override SecurityCheckpoint ExpectedSecurityCheckPoint => Env.Security.WhsItemDispatchConsignment;

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			var queryTemplates = new ZDBOnlySubQuery(typeof(ProcessTaskTemplate), ProcessTaskTemplateSchema.PK);
			queryTemplates.AddToFilter(ProcessTaskTemplateSchema.P0_ProcessType, WorkflowDescriptors.TransitDispatchConsignment);
			queryTemplates.AddToFilter(ProcessTaskTemplateSchema.P0_IsActive, true);

			var queryDefinitions = new ZDBOnlyQuery(typeof(GenCustomColumnDefinition));
			queryDefinitions.AddSubQuery(GenCustomColumnDefinitionSchema.XC_ParentID, queryTemplates, JoinCondition.And);

			var customColumnDefinition = Factory.Load<GenCustomColumnDefinition>(queryDefinitions);
			customColumnDefinition.DeleteAll();
			Factory.Save();
		}

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var dcn = (WhsItemDispatchConsignment)factory.NewWithValidTestData(businessObjectType);
			dcn.WDC_WW_Warehouse = TWInCurrentBranch.PK;

			return dcn;
		}

		#endregion
	}
}
