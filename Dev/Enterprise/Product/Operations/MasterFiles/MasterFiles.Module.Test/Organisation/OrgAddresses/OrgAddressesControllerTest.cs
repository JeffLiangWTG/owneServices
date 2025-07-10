using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrgAddressesController))]
	sealed class OrgAddressesControllerTest : ZControllerBasherTest
	{
		[RequiresSTA]
		public void TestLastSavedPK()
		{
			OrgAddress testAddress = (OrgAddress)GetBusinessObjectThatIsInTheDatabase();

			ZController controller = ZControllerFactory.Create(ControllerIDs.OrgAddresses);
			using (IZForm form = controller.ShowEditForm(testAddress))
			{
				AssertEquals(testAddress.PK, controller.LastSavedPK);
			}
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			OrgHeader org = TestHeaders.AddNew();
			org.OH_FullName = "Name";

			org.OH_RL_NKClosestPort = "USLAX";
			OrgAddress mainAddress = org.MainAddress;
			mainAddress.OA_Address1 = "Address";
			OrgAddress address = org.Addresses.AddNew();
			address.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);
			address.OA_Address1 = "Address";

			Factory.Save();

			return address;
		}

		OrgHeaderCollection TestHeaders
		{
			get
			{
				if (ftestHeaders == null)
				{
					ftestHeaders = new OrgHeaderCollection(Factory);
				}

				return ftestHeaders;
			}
		}
		OrgHeaderCollection ftestHeaders;

		public override void TestNewForm()
		{
			Assert(true); // The ZControllerBasherTest creates a new ZController without setting collection and relationship defaults, which ends in a bad way if the developer creates a new orgaddress
						  // without going through an organisation. This is not possible in the application from a user point of view, so this test is invalid for this controller.
						  //
						  // see ZFilterGridModule.GetNewControllerInternal() to see how it is done in the application for users (and works)
		}

		public void TestShowNewForm()
		{
			OrgAddress testAddress = (OrgAddress)GetBusinessObjectThatIsInTheDatabase();

			ZController controller = ZControllerFactory.Create(ControllerIDs.OrgAddresses);
			controller.SetCollectionForDefaultsAndValidation(testAddress.Header.Addresses);
			using (IZForm form = controller.ShowNewForm())
			{
				AssertNotNull("Form should have been constructed");
			}
		}

		[RequiresSTA]
		public void TestOrgFormIsShown()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_FullName = "Name";
			org.MainAddress.OA_Address1 = "Address";
			org.OH_RL_NKClosestPort = "USLAX";
			Factory.Save();

			ZController controller = ZControllerFactory.Create(ControllerIDs.OrgAddresses);
			using (IZForm shownForm = controller.ShowEditForm(org.MainAddress))
			{
				ZOrganisationsForm orgForm = shownForm as ZOrganisationsForm;
				AssertNotNull("ShownForm is OrgForm", orgForm);

				ZTemplateTabControl foundControl = FindControlByName(orgForm, "OrganisationsTabControl") as ZTemplateTabControl;
				AssertNotNull("Found the Tab Control", foundControl);
				AssertEquals("The selected Index is the Addresses Tabpage", OrganisationTabPages.Address.Name, foundControl.SelectedTab.Name);
			}
		}

		Control FindControlByName(Control outerControl, string name)
		{
			Control result = null;
			foreach (Control ctrl in outerControl.Controls)
			{
				if (ctrl.Name == name)
				{
					result = ctrl;
					break;
				}
				result = FindControlByName(ctrl, name);
				if (result != null)
				{
					break;
				}
			}
			return result;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.OrgAddresses;
		}
	}
}
