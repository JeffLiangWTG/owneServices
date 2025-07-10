using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.GUI;
using Enterprise.Freight.Business;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Module
{
	public partial class AgencyAllocationPlugin : ZPlugIn
	{
		public AgencyAllocationPlugin(JobVoyage hostBusinessEntity)
			: base(hostBusinessEntity)
		{
			countryCollection = new AgencyCountryCollection(hostBusinessEntity);
		}

		#region Implementation

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return countryCollection;
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.ShippingManager; }
		}

		public override string Name
		{
			get { return Res.GetString("786655d1-113a-4750-9c72-25ea15806439", "Allocations"); }
		}

		protected override Control GetNewUserControl()
		{
			JobVoyage voyage = countryCollection.Voyage;

			foreach (VoyageCountry country in voyage.Countries)
			{
				if (!country.IsInDatabase)
				{
					continue;
				}

				Factory.AddFetchHint(JobSlotAllocationSchema.E0_ParentID, country.PK);
			}

			foreach (VoyageOrigin origin in voyage.Origins)
			{
				if (!origin.IsInDatabase)
				{
					continue;
				}

				Factory.AddFetchHint(JobSlotAllocationSchema.E0_ParentID, origin.PK);
			}

			foreach (JobSailing sailing in voyage.Sailings)
			{
				if (!sailing.IsInDatabase)
				{
					continue;
				}

				Factory.AddFetchHint(JobSlotAllocationSchema.E0_ParentID, sailing.PK);
			}

			if (countryCollection.UseGlobalAllocations)
			{
				return new TopLevelVoyageAllocationControl();
			}
			else
			{
				return new OuterVoyageAllocationControl();
			}
		}

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (countryCollection.Mutex.HasLock)
				{
					countryCollection.Mutex.Unlock();
				}
			}

			base.Dispose(disposing);
		}

		public override void OnSaving()
		{
			base.OnSaving();
			countryCollection.PerformPreSave();
		}

		public override void OnSaveCompletedOrAborted(bool saved)
		{
			base.OnSaveCompletedOrAborted(saved);
			countryCollection.PerformPostSave();
		}

		public override void OnUserControlShown()
		{
			base.OnUserControlShown();
			countryCollection.Refresh();
		}

		public override void RefreshData()
		{
			countryCollection.Refresh();
		}

		public override ContinueWithSave ShowPreSaveDialogsCore()
		{
			var result = base.ShowPreSaveDialogsCore();
			var mutex = countryCollection.Mutex;

			if (result == ContinueWithSave.Yes)
			{
				if (!mutex.HasLock)
				{
					result = ContinueWithSave.No;

					var lockInfo = mutex.GetLockInfo();
					var (caption, message, allowRelease) = mutex.GetFriendlyMessage(lockInfo);
					var shouldForceUnlock = Globals.Message.Show(message, caption, allowRelease ? MessageBoxButtons.YesNo : MessageBoxButtons.OK, MessageBoxIcon.Error);

					if (shouldForceUnlock == DialogResult.Yes)
					{
						mutex.ReleaseLocks(lockInfo);
						mutex.Lock();
						result = ContinueWithSave.Yes;
					}
				}
			}
			else
			{
				if (mutex.HasLock)
				{
					mutex.Unlock();
				}
			}

			return result;
		}

		readonly AgencyCountryCollection countryCollection;

		#endregion
	}
}
