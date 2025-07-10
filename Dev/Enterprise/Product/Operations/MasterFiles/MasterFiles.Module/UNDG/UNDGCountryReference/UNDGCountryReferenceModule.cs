using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class UNDGCountryReferenceModule : ZFilterGridModule
	{
		#region Standard Module Overrides

		public override ModuleIdentifier ID => ModuleIDs.UNDGCountryReference;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => null;

		protected override IFilterControl GetNewFilterControl() => new UNDGCountryReferenceFilterControl(GridCollection, (UNDGCountryReferenceFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new UNDGCountryReferenceCollection(Factory);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new UNDGCountryReferenceFilterBusinessObject();

		protected override bool ShowRecentItemsCore() => false;

		public override bool AllowNew => false;

		public override bool AllowEdit => false;

		public override bool AllowDelete => false;

		public override bool AllowUniversalCopy => false;

		public override bool AllowView => false;

		#endregion

		#region Licence/Security

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.None;

		#endregion
	}
}
