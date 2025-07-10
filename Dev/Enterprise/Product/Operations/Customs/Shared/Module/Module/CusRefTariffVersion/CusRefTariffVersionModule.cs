using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Module
{
	public class CusRefTariffVersionModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.CusRefTariffVersion;

		public override Security.SecurityCheckpoint SecurityCheckpoint => Env.Security.GlobalTariffs;

		protected override Licensing.LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.CusRefTariffVersion);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new CusRefTariffVersionFilterStripBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new RefCusTariffVersionFilterUserControl(GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CusRefTariffVersionCollection(Factory);
		}

		public override bool AllowUniversalCopy => false;

		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;

		protected override IZForm ShowDeleteForm(BusinessObject selectedBusinessObject)
		{
			IZForm form = null;
			var version = selectedBusinessObject as CusRefTariffVersion;
			if (version != null && HasLinkedTariff(version.CRT_Version))
			{
				Globals.Message.ShowError(Res.GetString("569025B8-D2BB-4B27-8AF5-B910108DBEC6", "There are Tariffs linked to the version and it can not be deleted."));
			}
			else
			{
				form = base.ShowDeleteForm(selectedBusinessObject);
			}

			return form;
		}

		bool HasLinkedTariff(ZString versionCode)
		{
			var query = new ZQuery(TariffViewSchema.ZZ1_CRT_NKTariffVersion, versionCode);
			query.AddToFilter(TariffViewSchema.ZZ1_IsSystem, false);
			return Factory.LoadTop1<TariffView>(query) != null;
		}
	}
}
