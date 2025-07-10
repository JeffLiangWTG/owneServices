using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class OrgContactsModule : ZFilterGridModule, IOperationalActionSupportable
	{
		public OrgContactsModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		#region Standard Module Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.OrgContacts; }
		}

		public override bool SupportsWorkflow => true;

		public override string WorkflowType
		{
			get { return string.Empty; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.OrgContacts);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new OrgContactsFilterControl(GridCollection, (OrgContactsFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new OrgContactCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new OrgContactsFilterBusinessObject();
		}

		public override bool AllowNew
		{
			get { return true; }
		}

		public override bool AllowDelete
		{
			get { return false; }
		}

		public override bool AllowDefaultActivateDeactivate
		{
			get { return true; }
		}

		protected override bool AllowAdvancedDataAutomationWizard => true;

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.OrgContact; }
		}

		#endregion

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		#endregion

		#endregion

		#region IOperationalActionSupportable Members

		OperationalActionSupporter IOperationalActionSupportable.OperationalActionSupporter
		{
			get { return new OrgContactsActionSupporter(); }
		}

		#endregion

		#region WebAccess

		protected MenuItem EnableWebAccessMenuItem;
		protected MenuItem DisableWebAccessMenuItem;

		public static class WebAccessCaptions
		{
			public static string DisableWebAccess
			{
				get { return Res.GetString("b9af735c-8208-4dc4-a7a9-15bebb8e76cb", "&Disable Web Access"); }
			}

			public static string EnableWebAccess
			{
				get { return Res.GetString("a31ed87a-00b0-4a67-92ea-1a186b118e75", "&Enable Web Access"); }
			}

			public static string DeactivateWithRedirection
			{
				get { return Res.GetString("5db8d4c6-d6ad-4672-ba3e-04ddbd24f304", "&Deactivate and Supersede Web Access"); }
			}
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = base.GetNewActionMenuItems().ToList();

			if (AllowEdit)
			{
				result.Add(new ZMenuItem(ZMenuItem.Separator));
				result.Add(EnableWebAccessMenuItem = new ZMenuItem((NoResString)WebAccessCaptions.EnableWebAccess, EnableWebAccess_OnClick));
				result.Add(DisableWebAccessMenuItem = new ZMenuItem((NoResString)WebAccessCaptions.DisableWebAccess, DisableWebAccess_OnClick));
				result.Add(DisableWebAccessMenuItem = new ZMenuItem((NoResString)WebAccessCaptions.DeactivateWithRedirection, DeactivateWithRedirection_OnClick));
				result.Add(SendPasswordInstructionsMenuItem = new ZMenuItem((NoResString)SendPasswordInstructions, SendPasswordInstructions_OnClick));
			}

			return result.ToArray();
		}

		protected void EnableWebAccess_OnClick(object sender, EventArgs e)
		{
			EnableDisableWebAccess(true);
		}

		protected void DisableWebAccess_OnClick(object sender, EventArgs e)
		{
			EnableDisableWebAccess(false);
		}

		protected void DeactivateWithRedirection_OnClick(object sender, EventArgs e)
		{
			var selectedObjects = GetSelectedBusinessObjects();
			OrgContactSupersedeHelper.SupersedeContacts(selectedObjects.Cast<OrgContact>().ToArray());
		}

		void EnableDisableWebAccess(bool enable)
		{
			var selectedObjects = GetSelectedBusinessObjects();

			OrgContactEnableDisableWebAccess.EnableDisableWebAccess(enable, selectedObjects, Env.Security.OrgContactModify);
		}

		OrgContactEnableDisableWebAccess orgContactEnableDisableWebAccess;
		OrgContactEnableDisableWebAccess OrgContactEnableDisableWebAccess => orgContactEnableDisableWebAccess ?? (orgContactEnableDisableWebAccess = GetNewOrgContactEnableDisableWebAccess());
		protected virtual OrgContactEnableDisableWebAccess GetNewOrgContactEnableDisableWebAccess()
		{
			return new OrgContactEnableDisableWebAccess();
		}

		#endregion

		#region Send Password Instructions

		protected MenuItem SendPasswordInstructionsMenuItem;

		public static string SendPasswordInstructions
		{
			get { return Res.GetString("d8573ee8-7323-4552-a022-f43a68040772", "&Send Password Instructions"); }
		}

		internal protected static string InactiveContactsMessage => Res.GetString("991a692f-b926-4cdd-b259-6c48edb794e0", "Selected contacts are marked inactive below.");
		internal protected static string WebAccessDisabledContactsMessage => Res.GetString("2233d3cf-d07f-4dfc-8fe6-455b96b25027", "Selected contacts with disabled Web Access below.");
		internal protected static string HasInvalidEmailContactsMessage => Res.GetString("f4ee8878-6e13-477e-ac49-e4bd8b1ca9f9", "Selected contacts with blank email address below.");

		protected void SendPasswordInstructions_OnClick(object sender, EventArgs e)
		{
			var selectedObjects = GetSelectedBusinessObjects();

			var errorMessage = ValidateContactsAndGetErrorMessage(selectedObjects);

			if (errorMessage.Length > 0)
			{
				Globals.Message.ShowError(errorMessage.ToString());
				return;
			}

			if (new ContactSendEmailSetResetPassword().SendEmailToContacts(selectedObjects))
			{
				Globals.Message.Show(ResString.GetMultilingualString("935d585b-c983-4487-a7e0-fd3d065de3a8", "Send Password Instructions successful. An email was sent to the selected contacts containing password Instruction and URL."));
			}
			else
			{
				Globals.Message.ShowError(ResString.GetMultilingualString("d3d725f0-c355-45e1-b665-f1fee27c0d59", "An error was encountered while sending the password instruction email."));
			}
		}

		static ZStringBuilder ValidateContactsAndGetErrorMessage(BusinessObject[] selectedObjects)
		{
			var inactiveContacts = new List<OrgContact>();
			var webAccessDisabledContacts = new List<OrgContact>();
			var hasInvalidEmailContacts = new List<OrgContact>();
			var errorMessage = new ZStringBuilder();

			foreach (var contact in selectedObjects.Cast<OrgContact>())
			{
				if (!contact.OC_IsActive)
				{
					inactiveContacts.Add(contact);
				}
				if (!contact.OC_WebAccessEnabled)
				{
					webAccessDisabledContacts.Add(contact);
				}
				if (!contact.CheckContactHasValidEmail())
				{
					hasInvalidEmailContacts.Add(contact);
				}
			}

			if (inactiveContacts.Count > 0 || webAccessDisabledContacts.Count > 0 || hasInvalidEmailContacts.Count > 0)
			{
				errorMessage.Append(Res.GetString("F8F7A926-126E-40E8-AFB6-EF0A2F037291", "Send Password Instructions failed. "));
				if (inactiveContacts.Count > 0)
				{
					errorMessage.Append(InactiveContactsMessage).AppendLine();
					errorMessage.Append(string.Join(", ", inactiveContacts.Select(x => x.OC_ContactName))).AppendLine();
				}
				if (webAccessDisabledContacts.Count > 0)
				{
					errorMessage.Append(WebAccessDisabledContactsMessage).AppendLine();
					errorMessage.Append(string.Join(", ", webAccessDisabledContacts.Select(x => x.OC_ContactName))).AppendLine();
				}
				if (hasInvalidEmailContacts.Count > 0)
				{
					errorMessage.Append(HasInvalidEmailContactsMessage).AppendLine();
					errorMessage.Append(string.Join(", ", hasInvalidEmailContacts.Select(x => x.OC_ContactName))).AppendLine();
				}
			}

			return errorMessage;
		}

		#endregion

		protected override SecurityCheckpoint GetCheckpointForActivateDeactivate(BusinessObject[] selectedObjects)
		{
			return Env.Security.OrgContactModify;
		}

		protected override BusinessObjectActivator GetNewBusinessObjectActivator()
		{
			return new OrgContactActivator();
		}
	}
}
