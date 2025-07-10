using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Test
{
	public class DynamicLayoutPanelWithZDocAddressControlTest : TestCaseWithFactory
	{
		public void TestZDocAddressControl_SetDataBinding()
		{
			var bo = Factory.New<DummyWithZAddress>();
			var childBo = bo.Dummies.AddNew();
			childBo.Z0_NVarChar = "A";

			using (var form = new FormForTest(bo))
			{
				var panel = form.PanelForChildBusinessObject;
				panel.SetDataBinding(bo, nameof(DummyWithZAddress.Dummies));
				panel.UpdateLayout(new ZDocAddressControlLayoutProvider());
				form.Show();

				AssertEquals("Dummies.Lookups.DummyWithZAddressList", panel.FindSingle<ZDocAddressControl>(nameof(BagWithZDocAddressControl.DocAddressControl)).BindToOrganisations);
			}
		}

		class ZDocAddressControlLayoutProvider : IPanelLayoutProvider
		{
			public PanelLayout Layout => CreateLayout();

			PanelLayout CreateLayout()
			{
				var common = BagWithZDocAddressControl.Instance;
				var layout = new PanelLayout();
				layout.RegisterControlBag(common);

				layout.Include(common.DocAddressControl);
				layout.SetVisibility<DummyWithZAddress>(common.DocAddressControl, d => d.Z0_NVarChar == "A", d => d.Z0_NVarCharInfo);
				return layout;
			}
		}

		sealed class BagWithZDocAddressControl : ControlBag
		{
			public static BagWithZDocAddressControl Instance { get; } = new BagWithZDocAddressControl();

			BagWithZDocAddressControl()
			{
				DocAddressControl = RegisterControl(nameof(DocAddressControl));
			}

			public ControlReference DocAddressControl { get; }

			protected override Control CreateTemplate()
			{
				var template = new ZUserControl();
				template.BindingSource.DataSourceType = typeof(DummyWithZAddress);
				var docAddressControl = new ZDocAddressControl() { Name = nameof(DocAddressControl), BindToOrganisations = "Lookups.DummyWithZAddressList" };
				template.Controls.Add(docAddressControl);
				return template;
			}
		}

		sealed class FormForTest : ZForm
		{
			public FormForTest(DummyWithZAddress bo) : base(bo)
			{
				Controls.Add(PanelForChildBusinessObject);
				BindingSource.SetBindingMember(PanelForChildBusinessObject, nameof(DummyWithZAddress.Dummies));
				SetDataBinding(bo, "");
			}

			public readonly DynamicLayoutPanel PanelForChildBusinessObject = new DynamicLayoutPanel();
		}
	}
}
