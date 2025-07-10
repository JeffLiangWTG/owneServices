using Enterprise.MasterFiles.Business;
using ResString = Enterprise.Customs.DataTransfer.ResString;

namespace Enterprise.Customs.Business
{
	public class JobDeclarationFormCustomisationSettingsProvider : FormCustomisationSettingsProvider
	{
		protected override string[] GetPropertiesThatAffectWorkflow()
		{
			return new[]
			{
				BaseJobDeclaration.Schema.JE_TransportMode,
				BaseJobDeclaration.Schema.JE_MessageType,
				BaseJobDeclaration.Schema.JE_RL_NKPortOfLoading,
				BaseJobDeclaration.Schema.JE_RL_NKPortOfArrival,
				BaseJobDeclaration.Schema.JE_OH_Importer,
				BaseJobDeclaration.Schema.JE_OH_Supplier,
				BaseJobDeclaration.Schema.JE_GB
			};
		}

		protected override FormCustomisableElementCollection GetDisplayTabs()
		{
			#region SuppressResourceStringsCheckRegion

			var result = new FormCustomisableElementCollection();
			result.SuspendValidation();
			result.Add(ResString.GetMultilingualString("d8659763-254b-48c7-8c6e-7e7a5e143b2b", "Routing"), "RoutingTabPage");
			result.Add(ResString.GetMultilingualString("c3bef575-e12e-4fff-bcf7-5c3e49cf7881", "Containers"), "ContainerTabPage");
			result.Add(ResString.GetMultilingualString("717a4bdb-0025-4b8d-9f52-33674da2412b", "Inv. Grouping"), "InvoiceGroupingTabPage");
			result.Add(ResString.GetMultilingualString("3D8FDE4D-01D7-420A-83C8-71EED7A7D5D0", "Pickup"), "PickupTabPage");
			result.Add(ResString.GetMultilingualString("9E66F9CD-06B9-41FF-B469-EA6F8C1D6005", "Delivery"), "DeliveryTabPage");
			result.Add(ResString.GetMultilingualString("1bf3ac9b-1e2b-4cf7-93e5-bcad492e404d", "Workflow & Tracking"), "WorkflowTabPage");
			result.Add(ResString.GetMultilingualString("344158be-d8c7-40db-89fe-ce72969e325a", "Landed Costing"), "LandedCostingTabPage");
			result.Add(ResString.GetMultilingualString("0d1ea493-6e1d-4642-b8e0-f5ff13e59179", "Billing"), "BillingTabPage");
			result.Add(ResString.GetMultilingualString("4038fb67-fe7c-432d-806a-2ca3e90f3e47", "Addresses"), "AddressesTabPage");
			result.Add(ResString.GetMultilingualString("83f59416-853d-43a0-8cc8-08b4cce50a9f", "Doc Data"), "DocDataTabPage");
			result.Add(ResString.GetMultilingualString("fc4abca1-5215-4733-ba77-8186677e2eb9", "eDocs"), "eDocsTabPage");
			result.Add(ResString.GetMultilingualString("9f17c43b-ea25-4b9b-8cdd-9ac8eb4e9958", "Notes"), "BrokerageStmNoteTabPage");
			result.Add(ResString.GetMultilingualString("e481726b-6f15-46af-9d76-dbb62e6f4881", "Logs"), "EventTabPage");

			var complianceRisk = result.Add(ResString.GetMultilingualString("5a92100a-67ed-43e4-aaa6-2ea2b84d1d30", "Compliance Risk"), ComplianceRisk.Integration.ComplianceWiseConstants.ComplianceRiskTabPageName);
			complianceRisk.IsAvailableFunction = () => ComplianceRiskHelper.IsCustomsEnabledComplianceWise;

			result.ResumeValidation();
			return result;

			#endregion

		}
	}
}
