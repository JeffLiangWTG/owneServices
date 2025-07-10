using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI.Testing;

sealed class SupportingInformationUserControlTest : TestCaseWithFactory
{
	public void TestCaptionForFiscalReferencesTab()
	{
		using (var form = new ZForm(declaration))
		using (var control = new MiscOptionsLayoutUserControl())
		{
			form.Controls.Add(control);
			form.Show();
			AssertEquals("Fiscal References", control.FindSingle<ZTabPage>(x => x.Name == "FiscalReferencesTabPage").Text);
		}
	}

	public void TestGuaranteesUserControlType()
	{
		using (var control = new SupportingInformationControlForTest())
		{
			AssertEquals("Enterprise.Customs.EU.GUI.GuaranteesUserControl", control.GetGuaranteesUserControlType_Exposed().FullName);
		}
	}

	public void TestPreviousDocumentsUserControl()
	{
		using (var control = new SupportingInformationControlForTest())
		{
			var previousDocumentsUserControl = control.Controls.Find("previousDocumentsUserControl", true).FirstOrDefault() as ZDynamicControlCreationUserControl;
			AssertEquals(typeof(NLPreviousDocumentsUserControl), previousDocumentsUserControl.UserControlType);
		}
	}

	public void TestSupportingDocumentTabPageVisibility()
	{
		using (var form = new ZForm(declaration))
		using (var control = new SupportingInformationControlForTest())
		{
			form.Controls.Add(control);
			form.Show();

			AssertEquals(false, control.SupportingDocumentTabPage_Exposed.TabVisible);
		}
	}

	public void TestAdditionalInfoTabPageVisibility()
	{
		using (var form = new ZForm(declaration))
		using (var control = new SupportingInformationControlForTest())
		{
			form.Controls.Add(control);
			form.Show();

			AssertEquals(false, control.AdditionalInfoTabPage_Exposed.TabVisible);
		}
	}

	public void TestPreviousDocumentTabPageVisibility()
	{
		using (var form = new ZForm(declaration))
		using (var control = new SupportingInformationControlForTest())
		{
			form.Controls.Add(control);
			form.Show();

			AssertEquals(false, control.PreviousDocumentTabPage_Exposed.TabVisible);
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.CustomsEntryInstructions.AddNew();
	}
	JobDeclaration declaration;

	class SupportingInformationControlForTest : SupportingInformationControl
	{
		public SupportingInformationControlForTest() : base() { }

		public Type GetGuaranteesUserControlType_Exposed() => base.GetGuaranteesUserControlType();

		public ZTabPage GuaranteesTabPage_Exposed => base.GuaranteesTabPage;

		public ZTabPage SupportingDocumentTabPage_Exposed => base.SupportingDocumentTabPage;

		public ZTabPage AdditionalInfoTabPage_Exposed => base.AdditionalInfoTabPage;

		public ZTabPage PreviousDocumentTabPage_Exposed => base.PreviousDocumentTabPage;
	}
}
