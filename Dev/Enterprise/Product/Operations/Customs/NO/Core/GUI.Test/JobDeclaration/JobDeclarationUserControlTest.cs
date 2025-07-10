using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing;

[TestedType(typeof(JobDeclarationUserControl))]
sealed class JobDeclarationUserControlTest : BaseCustomsDeclarationUserControlAbstractTest<JobDeclarationUserControl, JobDeclaration>
{
	public void TestControlsVisibility()
	{
		CombineAssertions(() =>
		{
			userControl.AssertContainsControl("CustomsOfficesUserControl");
			userControl.AssertContainsControl("ShipmentDetailsFinalDestinationUserControl");
			userControl.AssertContainsControl("ShipmentDetailsOriginUserControl");
			userControl.AssertContainsControl("ShipmentDetailsGoodsLocationUserControl");
			userControl.AssertContainsControl("ShipmentDetailsWeightAndVolumeUserControl");
		});
	}

	public void TestOrganizationTabShouldBeSelectedByDefault()
	{
		var tabControl = form.FindSingle<ZTabControl>("RightTabControl");
		var selectedTab = tabControl.SelectedTab;

		AssertEquals("The Organizations tab should be selected by default, even though it's not the first tab.", userControl.OrganisationsTabPage, selectedTab);
	}

	public void TestCustomsOffices()
	{
		CombineAssertions(() =>
		{
			var dynamicControl = userControl.AssertContainsControl<ZDynamicControlCreationUserControl>("CustomsOfficesUserControl");

			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			AssertEquals("JE_MessageType 'IMP'", typeof(ImportCustomsOfficesUserControl), dynamicControl.UserControlType);

			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			AssertEquals("JE_MessageType 'EXP'", typeof(ExportCustomsOfficesUserControl), dynamicControl.UserControlType);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
		form = new ZForm(declaration);
		userControl = new JobDeclarationUserControl();
		form.Controls.Add(userControl);
		form.Show();
	}

	ZForm form;
	JobDeclarationUserControl userControl;
	JobDeclaration declaration;

	protected override void TearDown()
	{
		userControl?.Dispose();
		form?.Dispose();
		base.TearDown();
	}
}
