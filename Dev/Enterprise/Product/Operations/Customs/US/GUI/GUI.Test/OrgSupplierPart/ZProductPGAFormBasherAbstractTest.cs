using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.US.GUI.Testing
{
	abstract class ZProductPGAFormBasherAbstractTest<TProductPGAUserControl> : ZFormBasherTest
		where TProductPGAUserControl : UserControl, new()
	{
		protected override Form GetFormToBashCore()
		{
			var product = GetProduct();
			GetPGABusinessObject(product.PivotsForBinding[0]);
			Factory.Save();

			return new ProductPGATestForm<TProductPGAUserControl>(product, BindMember);
		}

		protected abstract string BindMember { get; }

		protected abstract BusinessObject GetPGABusinessObject(CusClassPartPivot pivot);

		protected OrgSupplierPart GetProduct()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "PROPGA";
			product.OP_Desc = "PRODUCT PGA";
			product.PivotsForBinding.AddNew();
			return product;
		}
	}

	sealed class ProductPGATestForm<TProductPGAUserControl> : ZForm
		where TProductPGAUserControl : UserControl, new()
	{
		public ProductPGATestForm(OrgSupplierPart product, string bindMember)
			: base(product)
		{
			this.bindMember = bindMember;
			InitializeComponent();
		}
		readonly string bindMember;
		UserControl userControl;

		new void InitializeComponent()
		{
			base.InitializeComponent();

			userControl = new TProductPGAUserControl();
			BindingSource.SetBindingMember(userControl, bindMember);
			userControl.Dock = DockStyle.Fill;
			Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 680, true);

			Controls.Add(userControl);
			DataSourceAssemblyName = "Enterprise.Customs.US.OrgSupplierPart";
			DataSourceTypeName = "Enterprise.Customs.US.Business.OrgSupplierPart";
			Name = "PGAUserControl";
		}
	}
}
