using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Module
{
	public class JobSupplierBookingLineModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.SupplierBookingLine;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.None;

		public override bool AllowDelete => false;

		public override bool AllowEdit => false;

		public override bool AllowNew => false;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.AlwaysAllow;

		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			throw new ModuleGuiNotSupportedException("Findbox only");
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			return Array.Empty<MenuItem>();
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new JobSupplierBookingLineFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new JobSupplierBookingLineFilterControl(GridCollection, (JobSupplierBookingLineFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new JobSupplierBookingLineCollection(Factory);
	}
}
