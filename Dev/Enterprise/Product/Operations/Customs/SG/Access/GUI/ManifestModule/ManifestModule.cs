using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.SG.Access.GUI
{
	public class ManifestModule : ASYCUDA.Module.AsycudaModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.ASYCUDA.SGAccess.Manifest;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.ASYCUDA.SGAccess.Manifest);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ManifestModuleCollection(Factory);
		}

		protected override MenuItem GetNVCMenu()
		{
			return new ZMenuItem(Res.GetData("SGAccess.GUI.ManifestModule.NewNVC", "&New Manifest"), (sender, e) =>
			{
				ShowNewNVCManifestForm(
					ApplicationBusinessProvider.GetApplicationBusinessProviders(Factory, Core.Constants.CountryCodes.Singapore).First(),
					Core.Constants.CountryCodes.Singapore);
			}, IconTypes.NewButtonActive, IconTypes.NewButtonRest);
		}

		protected override MenuItem GetVOCMenu()
		{
			return null;
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ManifestFilterStrip();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new ManifestFilterStripControl(GridCollection, FilterBusinessObject);
		}
	}
}
