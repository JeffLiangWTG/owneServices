using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Module
{
	sealed class ReconModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.US.Reconciliation;

		public override bool AllowNew => true;

		public override bool AllowEdit => true;

		public override bool AllowDelete => true;

		protected override ZQuery GetDisplayResultsQuery()
		{
			var result = base.GetDisplayResultsQuery();
			result.AddToFilter(ExportQuery);
			return result;
		}

		protected override PerformSearchResult LoadCollection(BusinessObjectFactory factory, Type type, ZQuery query)
		{
			var declarations = new JobDeclarationCollection(factory, GlbCompany.CurrentCompany.PK);
			declarations.Load(query);

			var result = declarations.Cast<JobDeclaration>().Select(s => new ReconDeclaration(s)).ToArray();
			return PerformSearchResult.Success(factory, query, result, permitActiveCollectionUpdates: false);
		}

		protected override SortInfo DefaultSortOrder => null;

		protected override IFilterControl GetNewFilterControl() => new ReconFilterStripControl(GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new ReconCollection(Factory);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new ReconFilterStripBusinessObject();

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => JobDeclarationZControllerDecider.GetZController(selectedBusinessObject as JobDeclaration) ?? ZControllerFactory.Create(ControllerIDs.Customs.US.Recon);

		public override ToolBarButton[] ToolBarButtons => Array.FindAll(base.ToolBarButtons, (button) => Array.IndexOf(new string[] { "Data Transfer" }, button.Text.Replace("&", "")) < 0);

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.ImportBroker;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.USRecon;

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.JobDeclarationWorkflowDescriptorCode;

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems());
			if (Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) == Core.Constants.CountryCodes.UnitedStates)
			{
				result.Add(new ZMenuItem("-"));
				result.Add(new ZMenuItem(CreateNewDeclarationMenuName, OnCreateNewDeclaration_Click));
			}
			return result.ToArray();
		}

		void OnCreateNewDeclaration_Click(object sender, EventArgs e)
		{
			if (Grid.SelectedElements.Length == 0)
			{
				Globals.Message.ShowError(SelectAtLeastOneeReconDec);
			}
			else if (Grid.SelectedElements.Length > 1)
			{
				Globals.Message.ShowInformation(SelectedMultiReconDec);
			}
			else
			{
				var selectedRecon = Grid.SelectedElements[0] as ReconDeclaration;
				var resultRecon = selectedRecon.TemplateReconDeclarationCopyCore(Customs.Business.CloneType.TemplateCopy);
				ZControllerFactory.Create(ControllerIDs.Customs.JobDeclaration).ShowFormForNewEntity(resultRecon);
			}
		}

		internal const string CreateNewDeclarationMenuName = "Create Recon from Existing Recon";
		internal string SelectAtLeastOneeReconDec = Res.GetString("BEFD7326-14B2-4195-AD8A-CC44AE961959", "Please select at least one Reconciliation.");
		internal string SelectedMultiReconDec = Res.GetString("76E38FE3-EAEC-40E7-99C5-4DBAF8AC71AC", "Please select one Recon. Declaration to copy.");
	}
}
