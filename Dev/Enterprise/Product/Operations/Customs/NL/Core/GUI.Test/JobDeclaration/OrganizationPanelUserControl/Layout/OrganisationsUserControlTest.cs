using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI.Testing;

class OrganisationsUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceDataSourceType()
	{
		AssertEquals(typeof(JobDeclaration), control.BindingSource.DataSourceType);
	}

	public void TestOrganizationUserControl()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Organizations User Control Width", 528, control.Width);
			AssertEquals("Organizations User Control Height", 385, control.Height);
			AssertType<OrganisationsUserControl>("Organisations User Control Type", control);
		});
	}

	public void TestIntracomReceiverControl()
	{
		CombineAssertions(() =>
		{
			AssertType<ZAddressControl>(control.IntracomReceiverAddressControl);
		});
	}

	public void TestDefermentPartyDocAddressControl()
	{
		var defermentPartyDocAddressControl = control.DefermentPartyDocAddressControl;

		CombineAssertions(() =>
		{
			AssertType<ZDocAddressControl>(defermentPartyDocAddressControl);
			AssertEquals("DefermentPartyDocAddressControl Caption", "Deferment Party", defermentPartyDocAddressControl.CaptionResourceString.Caption);
			AssertEquals("DefermentPartyDocAddressControl Width", 350, defermentPartyDocAddressControl.Width);
			AssertEquals("DefermentPartyDocAddressControl ShowCompanyName", true, defermentPartyDocAddressControl.ShowCompanyName);
		});
	}

	OrganisationsUserControl control;
	protected override void SetUp()
	{
		base.SetUp();
		control = new OrganisationsUserControl();
	}
	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}
}
