using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(RefEquipmentForm))]
	sealed class RefEquipmentFormTest : ZFormBasherTest
	{
		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			if (control.Name == "MessageTextBox")
			{
				return true;
			}
			return base.ShouldIgnoreMissingBindingMember(control);
		}

		protected override Form GetFormToBashCore()
		{
			return new RefEquipmentForm(Factory.New<RefEquipment>());
		}

		public void TestSetAUFieldsVisibility2()
		{
			var otherBranch = Factory.NewWithValidTestData<GlbBranch>();
			otherBranch.GB_RL_NKHomePort = "AUSYD";

			var otherBranch2 = Factory.NewWithValidTestData<GlbBranch>();
			otherBranch2.GB_RL_NKHomePort = "DEBER";

			var equipment = Factory.NewWithValidTestData<RefEquipment>();

			using (var form = new TestRefEquipmentForm(equipment))
			{
				form.Show();
				form.MainTabControl.SelectedIndex = 0;
				equipment.RQ_RN_NKRegistrationCountry = otherBranch.GB_RL_NKHomePort.SubstringSafe(0, 2);
				Factory.Save();
				Assert("NHVAStextbox should be visible", form.NHVAStextbox.Visible);

				equipment.RQ_RN_NKRegistrationCountry = otherBranch2.GB_RL_NKHomePort.SubstringSafe(0, 2);
				Factory.Save();
				Assert("NHVAStextbox should not be visible", !form.NHVAStextbox.Visible);
			}
		}

		public void TestEquipmentOwnerFieldDoesNotDisappearWhenSliderIsMoved()
		{
			var equipment = Factory.NewWithValidTestData<RefEquipment>();

			using (var form = new TestRefEquipmentForm(equipment))
			{
				form.Show();
				form.MainTabControl.SelectedIndex = 0;
				var headerGuidFindBox = form.HeaderGuidFindBox;
				form.SplitContainer.ParentForm.Width = 1000;
				form.SplitContainer.Panel1MinSize = 100;
				form.SplitContainer.SplitterDistance = 1000;
				Assert("Precondition: Owner box has width", headerGuidFindBox.Width > 0);
				form.SplitContainer.SplitterDistance = 100;
				Assert("Owner box still has width", headerGuidFindBox.Width > 0);
			}
		}

		class TestRefEquipmentForm : RefEquipmentForm
		{
			public TestRefEquipmentForm(RefEquipment bo)
				: base(bo)
			{
			}

			public new ZTemplateTabControl MainTabControl
			{
				get { return base.MainTabControl; }
			}

			public ZTextBox NHVAStextbox
			{
				get { return VehicleDetailsUserControl.NHVAStextbox; }
			}

			public ZGuidFindBox HeaderGuidFindBox
			{
				get { return EquipmentDetailsUserControl.HeaderGuidFindBox; }
			}

			new public CargoWise.Windows.UI.KSplitContainer SplitContainer
			{
				get { return base.SplitContainer; }
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			if (!ObjectFactory.HasBeenSubstituted<IWebNavigate>())
			{
				ObjectFactory.Substitute(Mock.Of<IWebNavigate>());
			}
		}
	}
}
