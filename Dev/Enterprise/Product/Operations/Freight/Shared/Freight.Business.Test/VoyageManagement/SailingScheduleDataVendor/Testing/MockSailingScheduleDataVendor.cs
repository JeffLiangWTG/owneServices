using System;
using CargoWise.Types;

namespace Enterprise.Freight.Business.Testing
{
	public sealed class MockSailingScheduleDataVendor : SailingScheduleDataVendor
	{
		#region Instance

		public new static MockSailingScheduleDataVendor Instance
		{
			get
			{
				if (fInstance == null)
				{
					fInstance = new MockSailingScheduleDataVendor();
				}
				return fInstance;
			}
		}
		[ThreadStatic]
		static MockSailingScheduleDataVendor fInstance;

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = delegate
			{ return Instance; };
		}

		public static void UnregisterThisSubTypeOverride()
		{
			OverridableNewDelegate.ResetValue();
			fInstance = null;
		}

		#endregion

		public new bool IsEnabled = true;

		protected override bool IsEnabledCore
		{
			get { return IsEnabled; }
		}

		public new bool IsVendorDataCurrent = true;

		protected override bool IsVendorDataCurrentCore
		{
			get { return IsVendorDataCurrent; }
		}

		public override string Status
		{
			get { return IsVendorDataCurrent ? "Vendor data current" : "Vendor data out of date"; }
		}

		public bool UpdateAllVoyageSailingsCalled;
		public override void UpdateAllVoyageSailings(JobVoyage voyage)
		{
			base.UpdateAllVoyageSailings(voyage);
			UpdateAllVoyageSailingsCalled = true;
		}

		public bool UpdateVoyageOriginCalled;
		protected override void UpdateVoyageOriginCore(VoyageOrigin origin)
		{
			origin.JA_E_DEP = new ZDateTime(2000, 1, 1);
			origin.JA_A_DEP = new ZDateTime(2000, 1, 2);

			if (origin.Voyage != null && origin.Voyage.Destinations.Count == 1)
			{
				origin.Voyage.Destinations[0].JB_AvailabilityDate = new ZDateTime(2000, 1, 3);
				origin.Voyage.Destinations[0].JB_StorageDate = new ZDateTime(2000, 1, 4);
			}
			UpdateVoyageOriginCalled = true;
		}

		public bool UpdateVoyageDestinationCalled;
		protected override void UpdateVoyageDestinationCore(VoyageDestination destination)
		{
			destination.JB_E_ARV = new ZDateTime(2000, 1, 5);
			destination.JB_A_ARV = new ZDateTime(2000, 1, 6);

			if (destination.Voyage != null && destination.Voyage.Origins.Count == 1)
			{
				destination.Voyage.Origins[0].JA_CutOff = new ZDateTime(2000, 1, 7);
				destination.Voyage.Origins[0].JA_ReceivalCommences = new ZDateTime(2000, 1, 8);
			}
			UpdateVoyageDestinationCalled = true;
		}
	}
}
