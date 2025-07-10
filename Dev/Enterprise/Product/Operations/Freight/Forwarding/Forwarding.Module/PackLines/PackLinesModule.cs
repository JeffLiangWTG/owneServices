using System.Windows.Forms;
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
	public class PackLinesModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.PackLines;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.None;

		public override bool AllowEdit => false;

		public override bool AllowView => false;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.AlwaysAllow;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			throw new ModuleGuiNotSupportedException("Findbox only");
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			return System.Array.Empty<MenuItem>();
		}

		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new PackLinesFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new PackLinesFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ParentShipmentOuterPackLineCollection(Factory);
		}
	}
}
