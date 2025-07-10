using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
namespace Enterprise.Customs.GUI
{
	public abstract class CusInBondPlugIn<T, U> : ZMutexedPlugIn
		where T : CusInBondHeader
		where U : KMenuItem
	{
		public CusInBondPlugIn(ICusInBondParent parent)
			: base(parent)
		{
			HostBusinessEntity.VisibilityChanged += OnChangeTheVisibilityRequired;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (HostBusinessEntity != null)
				{
					HostBusinessEntity.VisibilityChanged -= OnChangeTheVisibilityRequired;
				}
			}

			base.Dispose(disposing);
		}

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected void OnChangeTheVisibilityRequired(object sender, EventArgs e)
		{
			ChangeTheVisibility();
		}

		protected virtual void ChangeTheVisibility()
		{
			if (HostBusinessEntity != null)
			{
				Enabled = HostBusinessEntity.IsVisible;
				var header = BusinessEntity;
				if (header != null)
				{
					header.Synchroniser.SetEnabled(Enabled, header.Synchroniser.DetectEnabled);
				}
			}
		}

		protected new ICusInBondParent HostBusinessEntity
		{
			get { return (ICusInBondParent)base.HostBusinessEntity; }
		}

		protected new T BusinessEntity
		{
			get { return (T)base.BusinessEntity; }
		}

		protected override Licensing.LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.ImportBroker; }
		}

		#region Plugin Overrides

		protected bool gUIHasBeenShown;

		public override void OnGUIShown()
		{
			base.OnGUIShown();
			if (!gUIHasBeenShown)
			{
				foreach (MenuItem menu in TopLevelMenu.MenuItems)
				{
					menu.Select += new EventHandler(Menu_Select);
				}
			}
			gUIHasBeenShown = true;
		}

		void Menu_Select(object sender, EventArgs e)
		{
			checkRights = true;
		}

		public override void OnUserControlShown()
		{
			base.OnUserControlShown();
			checkRights = true;
		}

		protected bool checkRights;
		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return InBond;
		}

		protected override MenuItem GetNewTopLevelMenu()
		{
			if (SecurityCheckpointForMessagingMenu.IsAllowed)
			{
				InBondMenu.Text = Name;
				return InBondMenu;
			}
			else
			{
				return base.GetNewTopLevelMenu();
			}
		}

		protected U InBondMenu
		{
			get
			{
				if (fInBondMenu == null)
				{
					fInBondMenu = CreateNewInBondMenu();
				}

				return fInBondMenu;
			}
		}
		protected U fInBondMenu;

		protected abstract U CreateNewInBondMenu();

		protected ZMenuItem InBondMenuStubWhenSecurityRightsDenied
		{
			get
			{
				if (fInBondMenuStubWhenSecurityRightsDenied == null)
				{
					fInBondMenuStubWhenSecurityRightsDenied = new InBondMenuStub(Name, SecurityCheckpointForMessagingMenu);
				}

				return fInBondMenuStubWhenSecurityRightsDenied;
			}
		}
		ZMenuItem fInBondMenuStubWhenSecurityRightsDenied;

		#endregion

		protected override bool ShouldPluginDropdownMenuBeCreated()
		{
			return QueryUserShouldPlugInGUIAndBusinessEntityBeCreated();
		}

		protected virtual bool QueryUserToCreateInBond()
		{
			bool result = false;
			if (GetIsAllowed())
			{
				string queryText = null;
				if (HostBusinessEntity.IsInternalBrokerage)
				{
					queryText = CreateInBondQuery;
				}
				else
				{
					queryText = CreateInBondWarningQuery;
				}

				if (queryText != null)
				{
					result = QueryUser(queryText, GetCreateNewMovementText());
				}
			}

			return result;
		}

		protected virtual bool GetIsAllowed() => Env.Security.USInBondNew.IsAllowed;

		protected virtual string GetCreateNewMovementText() => Res.GetString("B3C4905A-2883-4040-A73A-9E9A856694E9", "Create In-Bond Movement for {0}", HostBusinessEntity.ParentType);

		protected virtual bool QueryUser(string question, string caption)
		{
			var result = Globals.Message.Show(question, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Information);
			return (result == DialogResult.Yes);
		}

		#region Validity

		public override ZString PlugInNotDisplayedMessage
		{
			get { return coveringLabelText; }
		}

		protected ZString coveringLabelText;

		protected override bool ShouldPlugInGUIAndBusinessEntityBeCreatedCore()
		{
			return InternalInBond != null;
		}

		protected override bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreated()
		{
			QueryAddInBond();
			return InternalInBond != null;
		}

		protected void QueryAddInBond()
		{
			coveringLabelText = string.Empty;

			if (InternalInBond == null)
			{
				if (!Mutex.IsLocked)
				{
					if (QueryUserToCreateInBond())
					{
						CreateInBond();
					}
					else
					{
						if (!GetIsAllowed())
						{
							coveringLabelText = GetErrorMessageForNotAllowed();
						}
						else
						{
							coveringLabelText = NotToCreateJobText;
						}
					}
				}
				else
				{
					coveringLabelText = MutexLockText;
				}
			}
		}

		protected virtual MultilingualString GetErrorMessageForNotAllowed() => Env.Security.USInBondNew.ErrorMessageForNotAllowed;

		#endregion

		#region Implementation

		protected virtual void ExcludeInBondPlugInFromHostMessageNotifications()
		{
			// Override in US
		}

		void StartPlugInSynchroniser()
		{
			if (Enabled)
			{
				var header = BusinessEntity;
				if (header != null)
				{
					header.SynchroniseWithParentIfNeeded();
				}
			}
		}

		public T InBond
		{
			get { return InternalInBond; }
		}

		protected T InternalInBond
		{
			get
			{
				if (fInBond == null)
				{
					fInBond = GetInbondFromParent();
					if (fInBond != null)
					{
						ExcludeInBondPlugInFromHostMessageNotifications();
						StartPlugInSynchroniser();
					}
				}

				return fInBond;
			}
			set
			{
				if (fInBond != value && fInBondMenu != null && value != null)
				{
					if (fInBond != null)
					{
						UnWireEventHandlerForLicenceLogin(fInBond);
					}
					SetHeaderOnMessagingMenu(value);
					DiscardCurrentUserControl();
				}

				fInBond = value;
				if (fInBond != null)
				{
					fInBond.BH_ParentID = HostBusinessEntity.PK;
					fInBond.BH_ParentTableCode = HostBusinessEntity.TablePrefix;
					WireEventHandlerForLicenceLogin(fInBond);
				}
			}
		}

		T fInBond;

		protected void fInBond_BondedWarehouseLicenceLogin(object sender, LicenceLoginEventArgs e)
		{
			if (gUIHasBeenShown)
			{
				e.LoginHasBeenAttempted = true;
				e.LicenceCheckPoint.Login(this);
			}
		}

		protected virtual T GetInbondFromParent()
		{
			return GetInBondHeader(ApplicationCode);
		}

		protected T GetInBondHeader(ZString applicationCode) => HostBusinessEntity.GetInBondHeader(applicationCode) as T;

		public bool CreateInBond()
		{
			var result = Mutex.Lock();
			if (!result)
			{
				coveringLabelText = MutexLockText;
			}
			else if (GetInbondFromParent() is not null)
			{
				coveringLabelText = GetMovementAlreadyCreatedText();
				Globals.Message.ShowWarning(coveringLabelText);
				result = false;
			}
			else
			{
				CreateNewInBond();
				ExcludeInBondPlugInFromHostMessageNotifications();
				StartPlugInSynchroniser();
				if (!Globals.IsTest)
				{
					InBond.HasChanges = true;
				}
			}
			return result;
		}

		protected void CreateNewInBond()
		{
			var inbond = HostBusinessEntity.Factory.New<T>();
			inbond.BH_ApplicationCode = ApplicationCode;
			InitiliseNewInbondAfterCreateNew(inbond);
			InternalInBond = inbond;
		}

		protected virtual void InitiliseNewInbondAfterCreateNew(T inbond)
		{
		}

		protected ZGlobalMutex fMutex;
		public override ZGlobalMutex Mutex
		{
			get
			{
				if (fMutex == null)
				{
					fMutex = new ZGlobalMutex(GetMutexID(), HostBusinessEntity.PK.ToString() + GlbCompany.CurrentCompany.GC_RN_NKCountryCode.ToString() + Name);
					// Name is included in key to allow users to create USInbond and NCTS records for a shipment simultaneously.
				}
				return fMutex;
			}
		}

		protected virtual MutexID GetMutexID()
		{
			return MutexIDs.InBondBeingCreatedForJob;
		}

		protected static string NotToCreateJobText
		{
			get { return Res.GetString("0CE561C4-1929-4CC8-9FF8-9CC9606541A3", "You have chosen not to create a Movement now.\r\nPlease change to another tab. You can click back to this tab when/if you need to create a Movement for this Job."); }
		}

		protected static string MutexLockText
		{
			get { return Res.GetString("EC3CBF53-B0F5-4869-9358-369F329E5B1F", "Someone else is already in the process of creating a Movement for job.\r\nYou should be able to access the Movement when the person has saved the record. Please try later."); }
		}

		protected virtual string GetMovementAlreadyCreatedText() => Res.GetString("b1051a3d-026f-406f-a55a-5d2e78164319", "Movement has already been created and linked to this Job.");

		protected static string CreateInBondQuery
		{
			get { return Res.GetString("InBoundPlugIn|CreateInBondQuery", "Are you sure you want to create a Movement now?"); }
		}

		protected static string CreateInBondWarningQuery
		{
			get { return Res.GetString("{80081CAF-8B9E-4BEB-8BEF-B6F98C9B8735}", "Your company is not the nominated customs broker. Are you sure you want to create the Movement?"); }
		}

		#endregion

		protected virtual void SetHeaderOnMessagingMenu(T value)
		{
			// For US:
			//fInBondMenu.Header = value;
		}

		protected virtual void UnWireEventHandlerForLicenceLogin(T fInBond)
		{
			// For US:  fInBond.BondedWarehouseLicenceLogin -= fInBond_BondedWarehouseLicenceLogin;
		}

		protected virtual void WireEventHandlerForLicenceLogin(T fInBond)
		{
			// For US:  fInBond.BondedWarehouseLicenceLogin += fInBond_BondedWarehouseLicenceLogin;
		}

		protected abstract SecurityCheckpoint SecurityCheckpointForMessagingMenu { get; }

		protected abstract string ApplicationCode { get; }
	}
}
