using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public abstract class SailingScheduleDataVendor : Integration.ISailingScheduleDataVendor
	{
		#region Factory Method

		public static SailingScheduleDataVendor Instance
		{
			get
			{
				SailingScheduleDataVendor result;
				var overridden = OverridableNewDelegate.Value;
				if (overridden != null)
				{
					result = overridden();
				}
				else
				{
					result = SailingDataFeedVendor;
				}
				return result;
			}
		}

		static SailingScheduleDataVendor SailingDataFeedVendor
		{
			get
			{
				if (sailingDataFeedVendor == null
					|| sailingDataFeedVendorCountryCode != GlbCompany.CurrentCompany.GC_RN_NKCountryCode
					|| sailingDataFeedVendorIsImportingUXML != IsImportingUXML)
				{
					sailingDataFeedVendorCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
					sailingDataFeedVendorIsImportingUXML = IsImportingUXML;

					if (IsImportingUXML)
					{
						sailingDataFeedVendor = new NoActionSailingScheduleDataVendor();
					}
					else
					{
						var onlineSailingSchedules = ObjectFactory.Get<Integration.SailingDataVendor.IOnlineSailingSchedulesDataVendor>() as SailingScheduleDataVendor;

						if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Australia
							|| GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.NewZealand)
						{
							sailingDataFeedVendor = (SailingScheduleDataVendor)ObjectFactory.Get<Integration.SailingDataVendor.IOneStopSailingScheduleDataVendor>();
						}
						else if (ObjectFactory.Get<Enterprise.Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnion(GlbCompany.CurrentCompany.GC_RN_NKCountryCode))
						{
							sailingDataFeedVendor = (SailingScheduleDataVendor)ObjectFactory.Get<Integration.SailingDataVendor.IDakosySailingScheduleDataVendor>();
						}
						else if (onlineSailingSchedules != null && onlineSailingSchedules.IsEnabled)
						{
							sailingDataFeedVendor = onlineSailingSchedules;
						}
						else
						{
							sailingDataFeedVendor = new NoActionSailingScheduleDataVendor();
						}
					}
				}

				return sailingDataFeedVendor;
			}
		}

		static bool IsImportingUXML
		{
			get
			{
				var serviceTaskCode = Env.Instance.ServiceTaskCode;
				return serviceTaskCode == "UMI" || serviceTaskCode == "UMQ";
			}
		}

		[ThreadStatic]
		static SailingScheduleDataVendor sailingDataFeedVendor;

		[ThreadStatic]
		static ZString sailingDataFeedVendorCountryCode;

		[ThreadStatic]
		static ZBool sailingDataFeedVendorIsImportingUXML;

		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();
		protected delegate SailingScheduleDataVendor NewDelegate();

		#endregion

		public bool IsEnabled
		{
			get { return IsEnabledCore; }
		}
		protected abstract bool IsEnabledCore { get; }

		public bool IsVendorDataCurrent
		{
			get { return IsVendorDataCurrentCore; }
		}
		protected abstract bool IsVendorDataCurrentCore { get; }

		public abstract string Status { get; }

		#region UpdateAllVoyageSailings

#if DEBUG
		virtual
#endif
		public void UpdateAllVoyageSailings(JobVoyage voyage)
		{
			if (IsVendorDataCurrent && !voyage.JV_VoyageFlight.IsEmpty && !voyage.JV_RV_NKVessel.IsEmpty)
			{
				foreach (JobSailing sailing in voyage.Sailings)
				{
					UpdateVoyageOrigin(sailing.Origin);
					UpdateVoyageDestination(sailing.Destination);
				}
			}
		}

		public SailingInformation TryFindSailingIncludingRelatedPorts(VoyageOrigin origin, VoyageDestination destination)
		{
			if (IsVendorDataCurrent)
			{
				var busyIndicatorProvider = origin.Factory.GetValue<IBusyIndicatorProvider>();
				using (busyIndicatorProvider?.NewBusyIndicator())
				{
					return TryFindSailingIncludingRelatedPortsCore(origin, destination);
				}
			}

			return null;
		}

		protected virtual SailingInformation TryFindSailingIncludingRelatedPortsCore(VoyageOrigin origin, VoyageDestination destination)
		{
			return null;
		}

		#endregion

		#region UpdateVoyageOrigin / UpdateVoyageDestination

		public void UpdateVoyageOrigin(VoyageOrigin origin)
		{
			if (IsVendorDataCurrent && !IsOriginUpdateSuppressed(origin.Factory))
			{
				var busyIndicatorProvider = origin.Factory.GetValue<IBusyIndicatorProvider>();
				using (busyIndicatorProvider?.NewBusyIndicator())
				{
					UpdateVoyageOriginCore(origin);
				}
			}
		}
		protected abstract void UpdateVoyageOriginCore(VoyageOrigin origin);

		public void UpdateVoyageDestination(VoyageDestination destination)
		{
			if (IsVendorDataCurrent && !IsDestinationUpdateSuppressed(destination.Factory))
			{
				var busyIndicatorProvider = destination.Factory.GetValue<IBusyIndicatorProvider>();
				using (busyIndicatorProvider?.NewBusyIndicator())
				{
					UpdateVoyageDestinationCore(destination);
				}
			}
		}
		protected abstract void UpdateVoyageDestinationCore(VoyageDestination destination);

		#endregion

		#region Update Suppression

		internal static bool IsOriginUpdateSuppressed(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue<VoyageUpdateSuppressor>().SuppressOriginUpdate > 0;
		}

		internal static bool IsDestinationUpdateSuppressed(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue<VoyageUpdateSuppressor>().SuppressDestinationUpdate > 0;
		}

		public static IDisposable SuppressVoyageOriginUpdate(BusinessObjectFactory factory)
		{
			VoyageUpdateSuppressor updateSuppressor = factory.GetCachedValue<VoyageUpdateSuppressor>();
			updateSuppressor.SuppressOriginUpdate++;

			return new DisposableAction(() => updateSuppressor.SuppressOriginUpdate--);
		}

		public static IDisposable SuppressVoyageDestinationUpdate(BusinessObjectFactory factory)
		{
			VoyageUpdateSuppressor updateSuppressor = factory.GetCachedValue<VoyageUpdateSuppressor>();
			updateSuppressor.SuppressDestinationUpdate++;

			return new DisposableAction(() => updateSuppressor.SuppressDestinationUpdate--);
		}

		sealed class VoyageUpdateSuppressor
		{
			public int SuppressOriginUpdate { get; set; }
			public int SuppressDestinationUpdate { get; set; }
		}

		#endregion
	}
}
