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
	[TestedType(typeof(OrgCusCodeController))]
	sealed class OrgCusCodeControllerTest : ZControllerBasherTest
	{
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			OrgHeader org = TestHeaders.AddNew();
			org.OH_FullName = "Name";
			org.MainAddress.OA_Address1 = "Address";
			org.OH_RL_NKClosestPort = "USLAX";

			OrgCusCode cusCode = org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "ABC1");

			Factory.Save();

			return cusCode;
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

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.OrgCusCode;
		}

		public override void TestNewForm()
		{
			Assert(true); // The ZControllerBasherTest creates a new ZController without setting collection and relationship defaults, which ends in a bad way if the developer creates a new orgcontact
						  // without going through an organisation. This is not possible in the application from a user point of view, so this test is invalid for this controller.
						  //
						  // see ZFilterGridModule.GetNewControllerInternal() to see how it is done in the application for users (and works)
		}

		[ExpectNoExceptions()]
		public void TestShowNewForm()
		{
			ZController controller = ZControllerFactory.Create(ControllerIDs.OrgCusCode);
			using (IZForm form = controller.ShowNewForm())
			{
				AssertNull("Should return null as Customs Registration Number does not have a Header", form);
			}
		}

		public void TestOrgFormIsShown()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_FullName = "Name";
			org.MainAddress.OA_Address1 = "Address";
			org.OH_RL_NKClosestPort = "USLAX";

			OrgCusCode cusCode = org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "ABC1");
			Factory.Save();

			ZController controller = ZControllerFactory.Create(ControllerIDs.OrgCusCode);
			using (IZForm shownForm = controller.ShowEditForm(cusCode))
			{
				ZOrganisationsForm orgForm = shownForm as ZOrganisationsForm;
				AssertNotNull("ShownForm is OrgForm", orgForm);

				ZTemplateTabControl foundControl = FindControlByName(orgForm, "OrganisationsTabControl") as ZTemplateTabControl;
				AssertNotNull("Found the Tab Control", foundControl);
				AssertEquals("The selected Index is the Detailss Tabpage", OrganisationTabPages.Details.Name, foundControl.SelectedTab.Name);
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
	}
}
