using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NL.GUI.Testing;

[TestedType(typeof(JobDeclarationUserControl))]
sealed class JobDeclarationUserControlTest : Customs.GUI.Testing.BaseCustomsDeclarationUserControlAbstractTest<JobDeclarationUserControl, JobDeclaration>
{
	public void TestControlVisibilityForExport()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

		using (var form = new ZForm(declaration))
		using (var userControl = new JobDeclarationUserControl())
		{
			form.Controls.Add(userControl);
			form.Show();

			CombineAssertions(() =>
			{
				AssertControlVisibility(userControl, "PresentationGroupBox", true);
				AssertControlVisibility(userControl, "PresentationStartDate", true);
				AssertControlVisibility(userControl, "PresentationEndDate", true);
			});
		}
	}

	public void TestControlVisibilityForImport()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		using (var form = new ZForm(declaration))
		using (var userControl = new JobDeclarationUserControl())
		{
			form.Controls.Add(userControl);
			form.Show();

			CombineAssertions(() =>
			{
				AssertControlVisibility(userControl, "PresentationGroupBox", false);
				AssertControlVisibility(userControl, "PresentationStartDate", false);
				AssertControlVisibility(userControl, "PresentationEndDate", false);
			});
		}
	}

	public void TestGroupBoxLocationsAndSize()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		using (var userControl = new JobDeclarationUserControl())
		{
			userControl.JobDeclaration = declaration;
			CombineAssertions(() =>
			{
				AssertEquals("TransportDetailsGroupBox Size", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 253, true), userControl.TransportDetailsGroupBox.Size);
				AssertEquals("ShipmentDetailsGroupBox Location", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(255, 310, true), userControl.ShipmentDetailsGroupBox.Location);
				AssertEquals("ShipmentDetailsGroupBox Size", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 344, true), userControl.ShipmentDetailsGroupBox.Size);
				AssertEquals("DeclarationDetailsGroupBox Location", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(731, 8, true), userControl.DeclarationDetailsGroupBox.Location);
				AssertEquals("DeclarationDetailsGroupBox Size", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(518, 203, true), userControl.DeclarationDetailsGroupBox.Size);
			});
		}
	}

	public void TestSetRightTabControlSelectTab()
	{
		var declaration = Factory.New<JobDeclaration>();
		using (var form = new ZForm(declaration))
		using (var control = new JobDeclarationUserControl())
		{
			control.JobDeclaration = declaration;
			form.Controls.Add(control);
			form.Show();

			AssertEquals("When the declaration form opens, show Organizations as default.", control.OrganisationsTabPage, control.RightTabControl.SelectedTab);
		}
	}

	public void TestImportDocAddressCaption()
	{
		using (var control = new JobDeclarationUserControl())
		{
			AssertEquals("[UCC 3/15] Importer", control.ImporterDocAddress.CaptionResourceString.Caption);
		}
	}

	public void TestSupplierDocAddressCaption()
	{
		using (var control = new JobDeclarationUserControl())
		{
			AssertEquals("[UCC 3/7] Supplier", control.SupplierDocAddress.CaptionResourceString.Caption);
		}
	}

	public void TestDeclarationDetailsGroupBox_ChildControls()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		using (var userControl = new JobDeclarationUserControl())
		{
			userControl.JobDeclaration = declaration;

			AssertEquals(nameof(JobDeclarationUserControl.DeclarationDetailsUserControl), userControl.DeclarationDetailsGroupBox.Controls.Cast<Control>().Single(x => x.Visible).Name);
		}
	}

	public void TestDeclarationDetailsCaption()
	{
		using (var control = new JobDeclarationUserControl())
		{
			AssertEquals("Entry Details", control.DeclarationDetailsGroupBox.CaptionResourceString.Caption);
		}
	}

	JobDeclaration declaration;

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
	}

	void AssertControlVisibility(Control userControl, string controlName, bool isVisible)
	{
		AssertEquals(controlName, isVisible, userControl.FindSingle<Control>(controlName).Visible);
	}
}
