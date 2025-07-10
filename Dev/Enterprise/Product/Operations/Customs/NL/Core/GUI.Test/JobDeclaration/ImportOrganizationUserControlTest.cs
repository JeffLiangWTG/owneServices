using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI.Testing;

public class ImportOrganizationUserControlTest : TestCaseWithFactory
{
	public void TestControlVisibility()
	{
		using (var control = new ImportOrganizationUserControl())
		{
			control.SetDataBinding(declaration, "");
			CombineAssertions(() =>
			{
				AssertEquals("IntracomReceiverControl", true, control.FindSingle<Control>("IntracomReceiverControl").Visible);
				AssertEquals("BuyerAddressControl", true, control.FindSingle<Control>("BuyerAddressControl").Visible);
				AssertEquals("RepresentativeAddressControl", false, control.FindSingle<Control>("RepresentativeAddressControl").Visible);
			});
		}
	}
	public void TestControlNames()
	{
		using (var control = new ImportOrganizationUserControl())
		{
			control.SetDataBinding(declaration, "");
			CombineAssertions(() =>
			{
				AssertEquals("IntracomReceiverControl", "Fiscal rep", control.FindSingle<ZAddressControl>("IntracomReceiverControl").CaptionResourceString.Caption);
				AssertEquals("BuyerAddressControl", "Buyer", control.FindSingle<ZAddressControl>("BuyerAddressControl").CaptionResourceString.Caption);
			});
		}
	}

	public void TestCompanyDisplayOnDefermentPartyDocAddressControl()
	{
		using (var control = new ImportOrganizationUserControl())
		{
			AssertEquals("DefermentPartyDocAddressControl show company", true, control.FindSingle<ZDocAddressControl>("DefermentPartyDocAddressControl").ShowCompanyName);
			AssertEquals("DefermentPartyDocAddressControl width", 320, control.FindSingle<ZDocAddressControl>("DefermentPartyDocAddressControl").Width);
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
	}
	JobDeclaration declaration;
}
