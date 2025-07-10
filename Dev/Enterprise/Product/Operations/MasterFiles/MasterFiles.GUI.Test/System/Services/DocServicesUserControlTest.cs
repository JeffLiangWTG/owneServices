using System.Windows.Forms;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI
{
	[TestedType(typeof(BasherForm))]
	sealed class DocServicesUserControlTest : ZFormBasherTest
	{
		[RequiresSTA]
		public void TestContextColumnVisibleInGrid()
		{
			var serviceObj = Factory.NewWithValidTestData<DummyWithServices>();

			using (var form = new ZForm(serviceObj))
			using (var serviceControl = new DocServicesUserControl())
			{
				form.Controls.Add(serviceControl);
				form.Show();

				AssertEquals(false, serviceControl.ContextColumnVisibleInGrid);
				var column = serviceControl.ServicesGrid.Columns["ParentContextID"];
				AssertNull(column);

				serviceControl.Visible = false;
				serviceControl.ContextColumnVisibleInGrid = true;
				serviceControl.Visible = true;
				column = serviceControl.ServicesGrid.Columns["ParentContextID"];
				AssertNotNull(column);
				AssertEquals(false, column.IsUnavailable);
				AssertEquals(true, column.IsVisible);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var serviceObj = Factory.NewWithValidTestData<DummyWithServices>();
			Factory.Save();

			return new BasherForm(serviceObj);
		}

		internal sealed class BasherForm : ZForm
		{
			internal BasherForm(DummyWithServices serviceParentObj)
				: base(serviceParentObj)
			{
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();

				Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 768);

				var control = new DocServicesUserControl();
				Controls.Add(control);
				CaptionRenderingEnabled = true;
			}
		}

		#endregion
	}
}
