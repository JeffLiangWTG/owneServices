using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class EntryInstructionBasicDetailsControlTest : TestCase
	{
		public void TestStyleDropEdit()
		{
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>(control.StyleDropEdit);
				AssertEquals("BindingMember", "CEI_Style", control.StyleDropEdit.BindTo);
			});
		}

		public void TestSubStyleDropEdit()
		{
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>(control.SubStyleDropEdit);
				AssertEquals("BindingMember", "CEI_SubStyle", control.SubStyleDropEdit.BindTo);
			});
		}

		public void TestCPCDropEdit()
		{
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>(control.CPCDropEdit);
				AssertEquals("BindingMember", "CEI_Procedure", control.CPCDropEdit.BindTo);
			});
		}

		public void TestDescriptionTextBox()
		{
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>(control.DescriptionTextBox);
				AssertEquals("BindingMember", "CEI_Description", control.DescriptionTextBox.BindTo);
			});
		}

		public void TestAssessmentDateEdit()
		{
			CombineAssertions(() =>
			{
				AssertType<ZDateEdit>(control.AssessmentDateEdit);
				AssertEquals("BindingMember", "CEI_DateForDuty", control.AssessmentDateEdit.BindTo);
			});
		}

		public void TestDetailsLabel()
		{
			CombineAssertions(() =>
			{
				AssertType<ZLabel>(control.DetailsLabel);
				AssertEquals("Caption", "Details", control.DetailsLabel.CaptionResourceString.Caption);
			});
		}

		public void TestOtherPartiesSeperatorUserControl()
		{
			CombineAssertions(() =>
			{
				AssertType<SeparatorUserControl>(control.OtherPartiesSeparatorUserControl);
				AssertEquals("Caption", "Other Parties", control.OtherPartiesSeparatorUserControl.CaptionResourceString.Caption);
			});
		}

		public void TestBondHolderOrganisationControl()
		{
			CombineAssertions(() =>
			{
				AssertType<ZOrganisationControl>(control.BondHolderOrganisationControl);
				AssertEquals("BindingMember", "CEI_OH_BondHolder", control.BondHolderOrganisationControl.BindTo);
			});
		}

		public void TestRemoverOrganisationControl()
		{
			CombineAssertions(() =>
			{
				AssertType<ZOrganisationControl>(control.RemoverOrganisationControl);
				AssertEquals("BindingMember", "CEI_OH_Carrier", control.RemoverOrganisationControl.BindTo);
			});
		}

		public void TestNewOwnerOrganisationControl()
		{
			CombineAssertions(() =>
			{
				AssertType<ZOrganisationControl>(control.NewOwnerOrganisationControl);
				AssertEquals("BindingMember", "CEI_OH_Owner", control.NewOwnerOrganisationControl.BindTo);
			});
		}

		public void TestNewOwnerOrganisationControlCaption()
		{
			AssertEquals("NewOwnerOrganisationControlCaption", "New Owner", control.NewOwnerOrganisationControl.CaptionResourceString.Caption);
		}

		public void TestBondHolderOrganisationControlCaption()
		{
			AssertEquals("BondHolderOrganisationControllCaption", "Bond Holder", control.BondHolderOrganisationControl.CaptionResourceString.Caption);
		}

		public void TestRemoverOrganisationControlCaption()
		{
			AssertEquals("RemoverOrganisationControl", "Remover", control.RemoverOrganisationControl.CaptionResourceString.Caption);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new EntryInstructionBasicDetailsControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		EntryInstructionBasicDetailsControl control;
	}
}
