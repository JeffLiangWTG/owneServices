using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Common.Business
{
	public abstract class CartageType
	{
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		protected CartageType(ICartageParent cartageParent)
		{
			CartageParent = cartageParent;
			ResetMonitoredInfos(false);
		}

		public ICartageParent CartageParent { get; }

		protected ICartageParent GetCartageParentInFactory(BusinessObjectFactory factory)
		{
			return (ICartageParent)factory.Load(CartageParent.GetType(), ((BusinessObject)CartageParent).PK);
		}

		// Tested via CommonCartage.
		public static CartageType GetCartageType(ICartageParent cartageParent, AutoJobCartage cartage)
		{
			if (cartageParent != null)
			{
				foreach (var cartageType in cartageParent.CartageTypes)
				{
					if (cartageType.CartageJobType == cartage.JJ_E3_NKJobType)
					{
						return cartageType;
					}
				}

				foreach (var cartageType in cartageParent.CartageTypes)
				{
					if (cartageType.GetMatchingDirectionCodes().Contains(cartage.JJ_Direction))
					{
						return cartageType;
					}
				}

				if (cartageParent.CartageTypes.Count == 1)
				{
					return cartageParent.CartageTypes.First();
				}
			}

			return null;
		}

		public abstract MultilingualString Description { get; }
		public abstract OrgAddress LocalTransportProviderAddress { get; }
		public abstract ZString CartageJobType { get; }
		public abstract JobDocAddress GetCartageAddress(ZString orgType);
		public abstract IReadOnlyCollection<ICartageContainer> CartageContainers { get; }
		public abstract IReadOnlyCollection<ICartageLooseCargo> CartageLooseCargo { get; }
		public abstract void CartageAdvised(BusinessObjectFactory factoryToSaveIn);
		public abstract void PickupCompleted(AutoJobContainer container, DocAddressType addressType, ZDateTime timeOut);
		public abstract void DeliveryCompleted(AutoJobContainer container, DocAddressType addressType, ZDateTime timeOut);
		public abstract void PickupCompleted(DocAddressType addressType, ZDateTime timeOut);
		public abstract void DeliveryCompleted(DocAddressType addressType, ZDateTime timeOut);
		public abstract void SetTotalDemurrage(TimeSpan demurrage);
		public abstract ZString DropMode { get; }

		public abstract ZDateTime EstimatedCartagePickup { get; }
		public abstract ZDateTime EstimatedCartageDelivery { get; }

		public abstract ZString PortOfLoading { get; }
		public abstract ZString PortOfDischarge { get; }
		public abstract ZString Vessel { get; }
		public abstract ZString VoyageFlight { get; }
		public abstract ZDateTime E_DEP { get; }
		public abstract ZDateTime E_ARV { get; }
		public abstract ZDateTime A_ARV { get; }
		public abstract ZDateTime A_DEP { get; }

		public abstract ZDateTime FCLReceivalCommences { get; }
		public abstract ZDateTime LCLReceivalCommences { get; }
		public abstract ZDateTime FCLCutOff { get; }
		public abstract ZDateTime LCLCutOff { get; }

		public abstract ZDateTime FCLAvailabilityDate { get; }
		public abstract ZDateTime LCLAvailabilityDate { get; }
		public abstract ZDateTime FCLStorageDate { get; }
		public abstract ZDateTime LCLStorageDate { get; }

		public abstract ZPropertyInfo CartageAddressInfo { get; }

		public MultilingualString DescriptionForMenu
		{
			get { return GetDescriptionForMenu(); }
		}

		protected virtual MultilingualString GetDescriptionForMenu()
		{
			return Description;
		}

		public abstract IEnumerable<ZString> GetMatchingDirectionCodes();

		#region Address Synching

		protected abstract ZPropertyInfo[] GetCartageAddressInfosToMonitor();

		public event EventHandler AddressesChanged
		{
			add
			{
				addressesChanged += value;

				if (monitoredInfos != null)
				{
					foreach (ZPropertyInfo info in monitoredInfos)
					{
						info.ValueChanged += value;
					}
				}
			}
			remove
			{
				addressesChanged -= value;

				if (monitoredInfos != null)
				{
					foreach (ZPropertyInfo info in monitoredInfos)
					{
						info.ValueChanged -= value;
					}
				}
			}
		}
		EventHandler addressesChanged;

		protected void ResetMonitoredInfos(bool forceChange)
		{
			if (monitoredInfos != null && addressesChanged != null)
			{
				foreach (ZPropertyInfo info in monitoredInfos)
				{
					foreach (EventHandler handler in addressesChanged.GetInvocationList())
					{
						info.ValueChanged -= handler;
					}
				}
			}

			monitoredInfos = GetCartageAddressInfosToMonitor();

			if (monitoredInfos != null && addressesChanged != null)
			{
				foreach (ZPropertyInfo info in monitoredInfos)
				{
					foreach (EventHandler handler in addressesChanged.GetInvocationList())
					{
						info.ValueChanged += handler;
					}
				}
			}

			if (forceChange && addressesChanged != null)
			{
				addressesChanged(this, EventArgs.Empty);
			}
		}
		ZPropertyInfo[] monitoredInfos;

		#endregion
	}
}
