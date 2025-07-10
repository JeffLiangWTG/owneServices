using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.LandedCosting.Business;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.LandedCosting.GUI
{
	public class LandedCostingPlugIn : ZMutexedPlugIn
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Plugin Tab Name")]
		public const string LCPlugInName = "Landed Costing";

		public LandedCostingPlugIn(ILandedCostHeader hostEntity)
			: base(hostEntity as IBusiness)
		{
			this.HostEntity = hostEntity;

			this.HostEntity.OnLCSupportedChanged += HostEntity_OnLCSupportedChanged;

			Enabled = hostEntity.IsLCSupported;
		}

		void HostEntity_OnLCSupportedChanged(object sender, EventArgs e)
		{
			Enabled = HostEntity.IsLCSupported;
		}

		#region ZPlugIn Members

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.Core; }
		}

		public override string Name
		{
			get { return LCPlugInName; }
		}

		protected override Control GetNewUserControl()
		{
			return new LandCostInputUserControl();
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return LCHeader;
		}

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override MenuItem GetNewTopLevelMenu()
		{
			return LCMenu;
		}

		protected LandedCostingMenu LCMenu
		{
			get
			{
				if (fLCMenu == null)
				{
					fLCMenu = CreateLandedCostingMenu();
					fLCMenu.LCHeaderDelegate = () => LCHeader;
				}
				return fLCMenu;
			}
		}
		LandedCostingMenu fLCMenu;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (fLCMenu != null)
				{
					fLCMenu.Dispose();
				}

				this.HostEntity.OnLCSupportedChanged -= HostEntity_OnLCSupportedChanged;
			}

			base.Dispose(disposing);
		}

		protected virtual LandedCostingMenu CreateLandedCostingMenu()
		{
			return new LandedCostingMenu(delegate
				{
					var result = Env.Licence.LandedCosting.Login(this);
					if (result == Integration.Licensing.LicenceLoginResponse.Denied)
					{
						return Env.Licence.LandedCosting.LastReasonForNotAllowing;
					}
					else
					{
						return "";
					}
				});
		}

		#endregion

		#region Create/Synchronise

		protected override bool ShouldPlugInGUIAndBusinessEntityBeCreatedCore()
		{
			return LCHeader != null;
		}

		protected override bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreated()
		{
			CreateLCJobIfRequired();
			return LCHeader != null;
		}

		public override ZString PlugInNotDisplayedMessage
		{
			get { return CoveringLabel; }
		}

		ZString CoveringLabel;

		protected void CreateLCJobIfRequired()
		{
			CoveringLabel = "";
			bool createLCHeader = false;
			bool newLCJobCreated = false;

			if (!HostEntity.IsLCSupported)
			{
				CoveringLabel = HostEntity.MessageShownWhenLCIsNotSupported;
			}
			else if (LCHeader == null)
			{
				if (!Mutex.IsLocked)
				{
					if (!HostEntity.IsJobInLCRunnableState)
					{
						if (GetConfirmationFromUsersToRunLC(Res.GetString("d1fd9695-5c05-4c70-a33a-2ee60c9c623d", "This job is not cleared yet. Are you sure you want to run Landed Costing now?")) == DialogResult.Yes)
						{
							createLCHeader = true;
						}
						else
						{
							CoveringLabel = Res.GetString("def275ce-8ab4-4962-81af-018f128c10b5", "This job is not cleared and you have agreed not to run Landed Costing now.");
						}
					}
					else if (GetConfirmationFromUsersToRunLC(Res.GetString("47972447-9844-49f3-8c00-89bb68dca1f0", "Do you want to create and run Landed Costing now?")) == DialogResult.No)
					{
						CoveringLabel = Res.GetString("7007dd13-0a5c-492a-b9bd-efe18163ab85", "You have chosen not to create and run Landed Costing now.");
					}
					else
					{
						createLCHeader = true;
					}
				}

				// Mutex.IsLocked should be checked again. While a modal dialog box is displayed above, another user could have created a LC job.
				if (Mutex.IsLocked)
				{
					LockInfo lockInfo = Mutex.GetLockInfo();
					CoveringLabel = Res.GetString("37be08e9-c7a4-4b46-bab3-d21bf0e02dbb", "{0} is already in the process of creating and running Landed Costing for this job from {1}. You can access it when they save.", lockInfo.UserWithLock.GS_FullName, lockInfo.LockStartTime.ToLongTimeString());
				}
				else if (createLCHeader && Mutex.Lock())
				{
					if (Env.Licence.LandedCosting.Login(this) != Integration.Licensing.LicenceLoginResponse.Denied)
					{
						CreateAndSynchroniseLandedCostObjects();
						newLCJobCreated = true;
					}
					else
					{
						CoveringLabel = Env.Licence.LandedCosting.LastReasonForNotAllowing;
					}
				}
			}

			if (LCHeader != null && !newLCJobCreated)
			{
				if (Env.Licence.LandedCosting.Login(this) != Integration.Licensing.LicenceLoginResponse.Denied)
				{
					LCHeader.ExchangeRates.LoadFromLCHeaderHost();
				}
				else
				{
					CoveringLabel = Env.Licence.LandedCosting.LastReasonForNotAllowing;
				}
			}
		}

		public static string MoreThanOneLCHeaderMessage
		{
			get { return Res.GetString("6cfdb067-81c8-45d4-8ebc-32de58faadd7", "More than one LC Header exist for this Business Object"); }
		}

		public LandedCostHeader LCHeader
		{
			get
			{
				if (fLCHeader == null)
				{
					LandedCostHeader[] result = (LandedCostHeader[])HostBusinessEntity.Factory.Load(typeof(LandedCostHeader), LandedCostingFilter);
					if (result.Length > 1)
					{
						fLCHeader = result[0];
						ErrorReporter.ReportOnce(MoreThanOneLCHeaderMessage, "More than one LC Header exist for this Business Object" + HostEntity.PK);
					}
					else if (result.Length == 1)
					{
						fLCHeader = result[0];
					}
					else if (result.Length == 0)
					{
						bool oldReload = LandedCostingFilter.ReLoadExistingRows;
						try
						{
							LandedCostingFilter.ReLoadExistingRows = true; //If Mutex was released by others and other user saves a created LCHeader, need to bypass the cached query
							fLCHeader = HostBusinessEntity.Factory.LoadTop1<LandedCostHeader>(LandedCostingFilter);
						}
						finally
						{
							LandedCostingFilter.ReLoadExistingRows = oldReload;
						}
					}
				}
				return fLCHeader;
			}
		}
		LandedCostHeader fLCHeader;

		protected ZQuery LandedCostingFilter
		{
			get
			{
				if (fLandedCostingFilter == null)
				{
					fLandedCostingFilter = new LandedCostHeaderFilter(HostEntity);
					fLandedCostingFilter.AddToFilter(JoinCondition.And, LandedCostHeaderSchema.LT_ParentTableCode, SQLComparisonOperator.Equal, HostEntity.TableCode);
					fLandedCostingFilter.IgnoreActiveFilter = true;
					fLandedCostingFilter.FetchOnlyFromLocalCache = false;
				}
				return fLandedCostingFilter;
			}
		}
		ZQuery fLandedCostingFilter;

		#endregion

		#region Implementation

		protected readonly ILandedCostHeader HostEntity;

		protected DialogResult GetConfirmationFromUsersToRunLC(string question)
		{
			return Globals.Message.Show(
						question,
						Res.GetString("b9cd0b08-fa5f-4108-830c-4e4ab3c73d87", "Landed Costing"),
						MessageBoxButtons.YesNo,
						MessageBoxIcon.Question);
		}

		protected virtual void CreateAndSynchroniseLandedCostObjects()
		{
			fLCHeader = HostBusinessEntity.Factory.New<LandedCostHeader>();
			fLCHeader.SynchroniseAll(HostEntity);
			LCMenu.LCHeaderDelegate = () => fLCHeader;
		}

		#endregion

		#region Mutex

		public override ZGlobalMutex Mutex
		{
			get
			{
				if (fMutex == null)
				{
					fMutex = HostEntity.GetLandedCostMutex();
				}
				return fMutex;
			}
		}
		ZGlobalMutex fMutex;

		#endregion
	}
}
