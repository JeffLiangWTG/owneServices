using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class JobRequiredDocumentAddInfoModule : ZFilterGridModule
	{
		public JobRequiredDocumentAddInfoModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.JobRequiredDocumentAddInfo; }
		}

		public override ZBool HasActions
		{
			get { return false; }
		}

		public override bool AllowNew
		{
			get { return false; }
		}
		public override bool AllowEdit
		{
			get { return false; }
		}

		public override bool AllowDelete
		{
			get { return false; }
		}

		protected override IZForm ShowViewForm(BusinessObject selectedBusinessObject)
		{
			return null;
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return null;
		}

		public override Security.SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		protected override Licensing.LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new JobRequiredDocumentAddInfoFilterStripControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new JobRequiredDocumentAddInfoCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new JobRequiredDocumentAddInfoFilterBusinessObject();
		}

		protected override SortInfo DefaultSortOrder
		{
			get { return null; }
		}

		protected override ZQuery GetDisplayResultsQuery()
		{
			var result = base.GetDisplayResultsQuery();
			result.AddToFilter(ExportQuery);
			return result;
		}
	}
}
