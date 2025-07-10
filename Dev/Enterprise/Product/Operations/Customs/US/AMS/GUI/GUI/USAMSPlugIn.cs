using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.GUI.PlugIn;
using Enterprise.Customs.US.AMS.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using CusInBondHeader = Enterprise.Customs.US.AMS.Business.CusInBondHeader;

namespace Enterprise.Customs.US.AMS.GUI
{
	public class USAMSPlugIn : CustomsCargoManifestPlugin
	{
		public USAMSPlugIn(ForwardingConsol hostBusinessEntity)
			: base(hostBusinessEntity)
		{
			if (ManifestProvider != null)
			{
				CustomsManifestVisibilityChanged -= OnChangeTheVisibilityRequired;
				CustomsManifestVisibilityChanged += OnChangeTheVisibilityRequired;
				hostBusinessEntity.Transports.CountChanged -= Transports_CountChanged;
				hostBusinessEntity.Transports.CountChanged += Transports_CountChanged;
				ChangeTheVisibility();
			}

			hostBusinessEntity.Factory.Saved += Factory_Saved;
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing)
				{
					UnlockMutexIfLockedByThisInstance();
					UnHookFactorySaveEvent();
				}
			}
			finally
			{
				if (disposing)
				{
					if (ManifestProvider != null)
					{
						CustomsManifestVisibilityChanged -= OnChangeTheVisibilityRequired;
						ManifestProvider.Transports.CountChanged -= Transports_CountChanged;
					}
				}

				base.Dispose(disposing);
			}
		}

		void Transports_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			OnChangeTheVisibilityRequired(sender, e);
		}

		protected override string NameCore
		{
			get { return "AMS Direct"; }
		}

		protected new ForwardingConsol ManifestProvider
		{
			get { return (ForwardingConsol)base.ManifestProvider; }
		}

		public CusInBondHeader Header
		{
			get { return BusinessEntity as CusInBondHeader; }
		}

		#region Implementation

		protected new ForwardingConsol HostBusinessEntity
		{
			get { return (ForwardingConsol)base.HostBusinessEntity; }
		}

		protected ZGlobalMutex Mutex
		{
			get { return mutex ?? (mutex = CusInBondHeader.CreateMutex(ManifestProvider.PK)); }
		}

		ZGlobalMutex mutex;

		public override ZString PlugInNotDisplayedMessage
		{
			get { return coveringLabelText; }
		}

		ZString coveringLabelText;

		protected override bool ShouldPlugInGUIAndBusinessEntityBeCreatedCore()
		{
			return Header != null;
		}

		protected override bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreated()
		{
			QueryAddAMSHeader();
			return Header != null;
		}

		void QueryAddAMSHeader()
		{
			coveringLabelText = string.Empty;

			if (Header == null)
			{
				if (!Mutex.IsLocked)
				{
					if (QueryUserToCreateAMS())
					{
						if (!CreateAMS())
						{
							coveringLabelText = Constants.Message.MutexLockText;
						}
					}
					else
					{
						coveringLabelText = Constants.Message.NotToCreateJobText;
					}
				}
				else
				{
					coveringLabelText = Constants.Message.MutexLockText;
				}
			}
		}

		bool QueryUserToCreateAMS()
		{
			return QueryUser(Constants.Message.CreateAMSQuery, Constants.Caption.CreateAMSQueryTitle);
		}

		bool QueryUser(string question, string caption)
		{
			var result = DialogResult.Yes;
			if (!Globals.IsTest)
			{
				result = Globals.Message.Show(
					question,
					caption,
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Information);
			}

			return (result == DialogResult.Yes);
		}

		protected ZBool CreateAMS()
		{
			var result = Mutex.Lock();
			if (result)
			{
				CreateNewAMSHeader();
				StartPlugInSynchroniser();
				if (!Globals.IsTest)
				{
					Header.HasChanges = true;
				}
			}

			return result;
		}

		void StartPlugInSynchroniser()
		{
			if (Enabled)
			{
				if (InternalHeader != null)
				{
					((Integration.Customs.US.USAMS.ICusInBondHeaderWithConsolSynchonisation)InternalHeader)
						.SynchroniseWithConsolIfNeeded();
				}
			}
		}

		void CreateNewAMSHeader()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			InternalHeader = header;
		}

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				UnlockMutexIfLockedByThisInstance();
			}
		}

		void UnlockMutexIfLockedByThisInstance()
		{
			if (Mutex.HasLock)
			{
				Mutex.Unlock();
			}
		}

		void UnHookFactorySaveEvent()
		{
			HostBusinessEntity.Factory.Saved -= new BusinessObjectFactory.SavedEventHandler(Factory_Saved);
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return InternalHeader;
		}

		CusInBondHeader GetAMSFromConsol()
		{
			var query = new ZQuery(CusInBondHeaderSchema.BH_ParentID, ManifestProvider.PK);
			query.AddToFilter(CusInBondHeaderSchema.BH_ApplicationCode, CusInBondApplicationCodeList.Codes.AMS);
			query.FetchOnlyFromLocalCache = !ManifestProvider.IsInDatabase;
			query.ReLoadExistingRows = true;
			return Factory.LoadTop1<CusInBondHeader>(query);
		}

		protected CusInBondHeader InternalHeader
		{
			get
			{
				if (fInternalHeader == null)
				{
					InternalHeader = GetAMSFromConsol();
					if (fInternalHeader != null)
					{
						StartPlugInSynchroniser();
					}
				}

				return fInternalHeader;
			}
			set
			{
				fInternalHeader = value;
				if (fInternalHeader != null)
				{
					fInternalHeader.BH_ParentID = ManifestProvider.PK;
					fInternalHeader.BH_ParentTableCode = ManifestProvider.TablePrefix;
					if (amsMainMenuItem != null)
					{
						amsMainMenuItem.Header = fInternalHeader;
					}
				}
			}
		}

		CusInBondHeader fInternalHeader;

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.Core; }
		}

		protected override CustomsManifestStatus GetCustomsManifestStatus(IManifestProvider manifestProvider)
		{
			return null;
		}

		protected override Control GetNewUserControl()
		{
			if (fMessageUserControl == null)
			{
				var header = Header;
				if (header != null)
				{
					fMessageUserControl = new USAMSConsolManifestUserControl(header);
				}
			}

			return fMessageUserControl;
		}

		USAMSConsolManifestUserControl fMessageUserControl;

		protected override MultilingualString MainMenuText
		{
			get { return ResString.GetMultilingualString("E980B921-4BBB-4750-A647-97BAAF5F9B36", "AMS"); }
		}

		protected override MenuItem GetNewTopLevelMenu()
		{
			var header = Header;
			if (amsMainMenuItem == null)
			{
				amsMainMenuItem = new AMSMainMenuItem(header);
			}

			return amsMainMenuItem;
		}

		AMSMainMenuItem amsMainMenuItem;

		protected override void ChangeTheVisibilityCore()
		{
			if (ManifestProvider != null)
			{
				Enabled = !ManifestProvider.IsDeleted
					&& (ManifestProvider.IsSea || ManifestProvider.IsRail)
					&& (ManifestProvider.IsGoingViaIgnoringDomesticRoute(Core.Constants.CountryCodes.UnitedStates) || ManifestProvider.IsGoingViaIgnoringDomesticRoute(Core.Constants.CountryCodes.PuertoRico));
			}
		}

		protected override void SetupTopLevelMenu()
		{
			MenuItem menuItemDeclareManifest = new ZMenuItem(ResString.GetMultilingualString("96878BE1-FE4D-4F36-841C-817EAE1BD587", "Send &Manifest"));
			menuItemDeclareManifest.Click += delegate
				{ DeclareManifest(); };
			mainMenuItem.MenuItems.Add(menuItemDeclareManifest);
		}

		public static class Constants
		{
			public static class Caption
			{
				public static string CreateAMSQueryTitle
				{
					get
					{
						return Res.GetString("USAMSPlugIn|D458FCFA-135D-4C45-9218-9881B527966C",
							"Create AMS for Consol");
					}
				}
			}

			public static class Message
			{
				public static string CreateAMSQuery
				{
					get { return Res.GetString("USAMSPlugIn|CreateAMSQuery", "Do you want to create AMS data?"); }
				}

				public static string NotToCreateJobText
				{
					get
					{
						return Res.GetString("USAMSPlugIn|NotToCreateJobText",
							"You have chosen not to create an AMS now.\r\nPlease change to another tab, then click back to this tab to create an AMS for this Consol.");
					}
				}

				public static string MutexLockText
				{
					get
					{
						return Res.GetString("USAMSPlugIn|MutexLockText",
							"Someone else is already in the process of creating an AMS for this Consol.\r\nYou should be able to access the AMS when the person has saved the record. Please try later.");
					}
				}
			}
		}

		#endregion

		protected event EventHandler CustomsManifestVisibilityChanged
		{
			add
			{
				ManifestProvider.JK_RL_NKFirstForeignPortInfo.ValueChanged += value;
				ManifestProvider.JK_RL_NKLastForeignPortInfo.ValueChanged += value;
				ManifestProvider.JK_RL_NKPortOfFirstArrivalInfo.ValueChanged += value;
				if (transportsHasChangedChangedWrapper == null)
				{
					ManifestProvider.Transports.HasChangesChanged += new EventHandler<HasChangesChangedEventArgs>(Transports_HasChangesChanged);
				}

				transportsHasChangedChangedWrapper += value;
			}
			remove
			{
				ManifestProvider.JK_RL_NKFirstForeignPortInfo.ValueChanged -= value;
				ManifestProvider.JK_RL_NKLastForeignPortInfo.ValueChanged -= value;
				ManifestProvider.JK_RL_NKPortOfFirstArrivalInfo.ValueChanged -= value;
				transportsHasChangedChangedWrapper -= value;
				if (transportsHasChangedChangedWrapper == null)
				{
					ManifestProvider.Transports.HasChangesChanged -= new EventHandler<HasChangesChangedEventArgs>(Transports_HasChangesChanged);
				}
			}
		}

		void Transports_HasChangesChanged(object sender, HasChangesChangedEventArgs e)
		{
			if (transportsHasChangedChangedWrapper != null)
			{
				transportsHasChangedChangedWrapper(sender, e);
			}
		}

		event EventHandler transportsHasChangedChangedWrapper;
	}
}
