using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.InBond.GUI
{
	public class InBondPlugIn : Customs.GUI.CusInBondPlugIn<CusInBondHeader, EDIMenu>
	{
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public InBondPlugIn(ICusInBondParent parent)
			: base(parent)
		{
			ChangeTheVisibility();
		}

		public override string Name => "In-Bond"; // Plugin Tab Name

		public override ContinueWithSave ShowPreSaveDialogsCore()
		{
			var result = base.ShowPreSaveDialogsCore();
			if (result == ContinueWithSave.Yes && checkRights)
			{
				if (BusinessEntity.HasChanges || !BusinessEntity.IsInDatabase)
				{
					result = new USInBondSecurityAlertHelper(BusinessEntity).GetSecurityAlertMessage();
				}
			}
			return result;
		}

		protected override void ChangeTheVisibility()
		{
			Enabled = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) == Core.Constants.CountryCodes.UnitedStates;
			if (HostBusinessEntity is ForwardingConsol consol)
			{
				Enabled = Enabled && consol.JK_TransportMode == USCustomsDataRegistry.Instance.TransportModeForInBondCreationFromConsol.Value;
			}
		}

		protected override Control GetNewUserControl() => CreateInBondUserControl();

		protected USInBondUserControl CreateInBondUserControl() => new USInBondUserControl(InternalInBond);

		protected override MenuItem GetNewTopLevelMenu() => SecurityCheckpointForMessagingMenu.IsAllowed ? base.GetNewTopLevelMenu() : InBondMenuStubWhenSecurityRightsDenied;

		protected override void ExcludeInBondPlugInFromHostMessageNotifications()
		{
			if (HostBusinessEntity is IMessageNotificationsProvider messageNotificationsProvider)
			{
				messageNotificationsProvider.ExcludeChildAndItsDescendentsFromMessageNotifications(BusinessEntity);
			}
		}

		protected override bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreated()
		{
			if (IsConsolParent)
			{
				AddInBondForConsol();
			}
			else
			{
				QueryAddInBond();
			}
			return InternalInBond != null;
		}

		protected override bool ShouldPluginDropdownMenuBeCreated()
		{
			var result = false;

			if (IsConsolParent)
			{
				result = InternalInBond != null;
				if (!result)
				{
					Globals.Message.Show(Res.GetString("C647F1AE-2189-4F63-954F-3B173AD516D6", "Please click In-Bond tab and create In-Bond firstly."));
				}
			}
			else
			{
				result = base.ShouldPluginDropdownMenuBeCreated();
			}
			
			return result;
		}

		void AddInBondForConsol()
		{
			coveringLabelText = string.Empty;
			if (InternalInBond == null)
			{
				if (!Mutex.IsLocked)
				{
					Mutex.Lock();
					inBondForConsolWraperCache = IsConsolParent ? new CreateInBondForConsolWraper(HostBusinessEntity) : null;
					var createInBondForm = new CreateInBondForConsolForm(inBondForConsolWraperCache);
					ZFormModaliser.ShowDialogAndDispose(createInBondForm);
					if (createInBondForm.DialogResult == DialogResult.OK)
					{
						if (!CreateInBond())
						{
							coveringLabelText = Constants.MutexLockText;
						}
					}
					else
					{
						if (Mutex.HasLock)
						{
							Mutex.Unlock();
						}

						if (!GetIsAllowed())
						{
							coveringLabelText = GetErrorMessageForNotAllowed();
						}
						else
						{
							coveringLabelText = Constants.NotToCreateJobText;
						}
					}
				}
				else
				{
					coveringLabelText = Constants.MutexLockText;
				}
			}
		}

		CreateInBondForConsolWraper inBondForConsolWraperCache;

		bool IsConsolParent => HostBusinessEntity is ForwardingConsol;

		protected override string ApplicationCode => CusInBondApplicationCodeList.Codes.InBond;

		protected override void SetHeaderOnMessagingMenu(CusInBondHeader value)
		{
			fInBondMenu.Header = value;
		}

		protected override void UnWireEventHandlerForLicenceLogin(CusInBondHeader fInBond)
		{
			fInBond.BondedWarehouseLicenceLogin -= fInBond_BondedWarehouseLicenceLogin;
		}

		protected override void WireEventHandlerForLicenceLogin(CusInBondHeader fInBond)
		{
			fInBond.BondedWarehouseLicenceLogin += fInBond_BondedWarehouseLicenceLogin;
		}

		protected override Security.SecurityCheckpoint SecurityCheckpointForMessagingMenu => Env.Security.USInBondMessaging;

		protected override EDIMenu CreateNewInBondMenu()
		{
			fInBondMenu = new EDIMenu();
			fInBondMenu.Header = InBond;
			return fInBondMenu;
		}

		protected override void InitiliseNewInbondAfterCreateNew(CusInBondHeader inBond)
		{
			if (HostBusinessEntity is ForwardingConsol && inBondForConsolWraperCache != null)
			{
				inBondForConsolWraperCache.Initialize(inBond);
			}
		}

		public static class Constants
		{
			public static string NotToCreateJobText
			{
				get { return Res.GetString("FA876758-D2D9-4A42-BB68-5C979C259D07", "You have chosen not to create In-Bond Movements now.\r\nPlease change to another tab. You can click back to this tab when/if you need to create a Movement for this Job."); }
			}

			public static string MutexLockText
			{
				get { return Res.GetString("5537AD86-6E0C-4AFE-9799-0A023FAA502E", "Someone else is already in the process of creating In-Bond Movements for job.\r\nYou should be able to access the Movement when the person has saved the record. Please try later."); }
			}
		}
	}
}
