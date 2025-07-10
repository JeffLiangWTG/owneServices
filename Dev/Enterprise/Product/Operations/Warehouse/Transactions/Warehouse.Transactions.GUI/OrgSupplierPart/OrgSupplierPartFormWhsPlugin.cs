using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class OrgSupplierPartFormWhsPlugin : ZPlugIn
	{
		public OrgSupplierPartFormWhsPlugin(OrgSupplierPart part)
			: base(part)
		{
			Product = WhsProduct.GetWhsProduct(part);
		}

		public override string Name => (NoResString)"Warehouse"; // Hard-coded name

		protected override LicenceCheckpoint LicenceCheckPoint => Env.Licence.Core;

		protected override Control GetNewUserControl()
		{
			return new ProductEntryUserControl();
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return Product;
		}
		readonly WhsProduct Product;

		protected override ZBool HasUserControl => true;
	}
}

#region Test
#if DEBUG

namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class OrgSupplierPartFormWhsPlugin
	{
		public WhsProduct WhsProductForTest => Product;
		public LicenceCheckpoint LicenceCheckPointForTest => LicenceCheckPoint;
		public ZBool HasUserControlForTest => HasUserControl;
		public IBusiness GetBusinessEntityForPlugInForTest() => GetBusinessEntityForPlugIn();
		public Control GetNewUserControlForTest() => GetNewUserControl();
	}
}

#endif
#endregion
