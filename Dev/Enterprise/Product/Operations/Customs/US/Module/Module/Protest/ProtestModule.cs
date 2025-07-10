using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Protest;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Module
{
	class ProtestModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.US.Protest;

		public override bool AllowNew => true;

		public override bool AllowEdit => true;

		public override bool AllowDelete => false;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.USProtest;

		public override ToolBarButton[] ToolBarButtons => Array.FindAll(base.ToolBarButtons, (button) => Array.IndexOf(new string[] { "Data Transfer" }, button.Text.Replace("&", "")) < 0);

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.ProtestWorkflowDescriptorCode;

		protected override ZQuery GetDisplayResultsQuery()
		{
			var result = base.GetDisplayResultsQuery();
			result.AddToFilter(ExportQuery);
			return result;
		}

		protected override PerformSearchResult LoadCollection(BusinessObjectFactory factory, Type type, ZQuery query)
		{
			var protestDeclarations = new JobDeclarationCollection(factory, GlbCompany.CurrentCompany.PK);
			protestDeclarations.Load(query);

			var result = protestDeclarations.Cast<JobDeclaration>().Select(s => new Business.Protest.Protest(s)).ToArray();
			return PerformSearchResult.Success(factory, query, result, permitActiveCollectionUpdates: false);
		}

		protected override SortInfo DefaultSortOrder => null;

		protected override IFilterControl GetNewFilterControl() => new ProtestFilterStripControl(GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new ProtestCollection(Factory);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new ProtestFilterStripBusinessObject();

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => JobDeclarationZControllerDecider.GetZController(selectedBusinessObject as JobDeclaration) ?? ZControllerFactory.Create(ControllerIDs.Customs.US.Protest);

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.ImportBroker;
	}
}
