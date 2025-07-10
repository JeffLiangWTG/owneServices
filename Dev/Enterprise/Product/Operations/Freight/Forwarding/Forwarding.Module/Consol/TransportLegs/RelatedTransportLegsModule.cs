using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Module
{
	class RelatedTransportLegsModule : ZFilterGridModule
	{
		#region Security

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		#endregion

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.RelatedTransportLegs; }
		}

		public override bool SupportsWorkflow
		{
			get { return true; }
		}

		public override string WorkflowType
		{
			get { return string.Empty; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return null;
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new TransportLegFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new TransportNonDependentCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new RelatedTransportLegsFilterBusinessObject();
		}

		protected override bool ShowRecentItemsCore()
		{
			return false;
		}

		public override bool AllowNew
		{
			get { return false; }
		}

		public override bool AllowView
		{
			get { return false; }
		}

		public override bool AllowDelete
		{
			get { return false; }
		}

		public override bool AllowEdit
		{
			get { return false; }
		}

		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;
	}
}
