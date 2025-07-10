using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.NZ.Module.Declaration
{
	public class ECIWriteOffManifestingModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.NZ.ECIWriteOffManifesting; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Broker; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.NZCustomsECIManifesting; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.NZ.ECIWriteOffManifesting);
		}

		protected override ZArchitecture.Business.FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ECIWriteOffManifestingFilterBusinessObject();
		}

		protected override ZArchitecture.GUI.IFilterControl GetNewFilterControl()
		{
			return new ECIWriteOffManifestingFilterControl(GridCollection, (ECIWriteOffManifestingFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CusEntryHeaderCollection(Factory);
		}

		public override bool AllowDelete
		{
			get { return false; }
		}

		public override bool AllowEdit
		{
			get { return true; }
		}

		public override bool AllowNew
		{
			get { return true; }
		}

		protected override bool CanBeCopied()
		{
			return false;
		}

		protected override bool ShowRecentItemsCore()
		{
			return false;
		}
	}
}

