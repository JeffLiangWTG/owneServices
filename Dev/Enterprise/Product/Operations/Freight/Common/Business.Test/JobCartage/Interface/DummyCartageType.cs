using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Common.Business.Testing
{
	public class DummyCartageType : CartageType
	{
		public DummyCartageType(DummyCartageParent parent, ZString[] matchingDirectionCodes = null)
			: this(parent, Core.Constants.CartageJobType.NEW_AirExport, null, matchingDirectionCodes)
		{
		}

		public DummyCartageType(DummyCartageParent parent, ZString cartageJobType, Dictionary<ZString, JobDocAddress> addressesByOrgType, ZString[] matchingDirectionCodes = null)
			: base(parent)
		{
			this.parent = parent;
			this.cartageJobType = cartageJobType;
			this.addressesByOrgType = addressesByOrgType;
			this.containers = Array.Empty<ICartageContainer>();
			this.looseGoods = new ICartageLooseCargo[] { new DummyCartageLooseCargo(new BusinessObjectFactory(), 3) };
			this.matchingDirectionCodes = matchingDirectionCodes ?? Array.Empty<ZString>();
		}

		readonly DummyCartageParent parent;
		ZString cartageJobType;
		readonly Dictionary<ZString, JobDocAddress> addressesByOrgType;

		#region Public Setters

		public void SetContainers(ICartageContainer[] containers)
		{
			this.containers = containers;
		}
		ICartageContainer[] containers;

		public void SetLoose(ICartageLooseCargo[] looseGoods)
		{
			this.looseGoods = looseGoods;
		}
		ICartageLooseCargo[] looseGoods;

		#endregion

		public override void CartageAdvised(BusinessObjectFactory factory)
		{
			CartageAdvisedCount++;
			LastCartageAdvisedFactory = factory;
		}
		public ZInt CartageAdvisedCount;
		public BusinessObjectFactory LastCartageAdvisedFactory;

		public override void PickupCompleted(AutoJobContainer container, DocAddressType addressType, ZDateTime timeOut)
		{
		}

		public override void DeliveryCompleted(AutoJobContainer container, DocAddressType addressType, ZDateTime timeOut)
		{
		}

		public override void PickupCompleted(DocAddressType addressType, ZDateTime timeOut)
		{
		}

		public override void DeliveryCompleted(DocAddressType addressType, ZDateTime timeOut)
		{
		}

		public override void SetTotalDemurrage(TimeSpan demurrage)
		{
			parent.Demurrage = demurrage;
		}

		public override IReadOnlyCollection<ICartageContainer> CartageContainers
		{
			get { return containers; }
		}

		public override ZString CartageJobType
		{
			get { return cartageJobType; }
		}

		public void SetCartageJobType(ZString cartageJobType)
		{
			this.cartageJobType = cartageJobType;
		}

		public override IReadOnlyCollection<ICartageLooseCargo> CartageLooseCargo
		{
			get { return looseGoods; }
		}

		public override OrgAddress LocalTransportProviderAddress
		{
			get
			{
				if (cartageOrganisation == null && allowCartageOrganisation)
				{
					cartageOrganisation = parent.Factory.NewWithValidTestData<OrgHeader>();
				}
				return cartageOrganisation != null ? cartageOrganisation.MainAddress : null;
			}
		}
		OrgHeader cartageOrganisation;

		public override ZPropertyInfo CartageAddressInfo
		{
			get { return parent.CartageAddress.E2_OA_AddressInfo; }
		}

		public void RemoveCartageOrganisation()
		{
			allowCartageOrganisation = false;
			cartageOrganisation = null;
		}
		bool allowCartageOrganisation = true;

		public void SetCartageOrganisation(OrgHeader orgHeader)
		{
			allowCartageOrganisation = true;
			cartageOrganisation = orgHeader;
		}

		public override MultilingualString Description
		{
			get { return (NoResString)"Dummy"; }
		}

		public override ZDateTime EstimatedCartagePickup
		{
			get { return new ZDateTime(2008, 4, 18); }
		}

		public override ZDateTime EstimatedCartageDelivery
		{
			get { return new ZDateTime(2008, 4, 20); }
		}

		public override ZDateTime E_ARV
		{
			get { return new ZDateTime(2008, 4, 17); }
		}

		public override ZDateTime E_DEP
		{
			get { return new ZDateTime(2008, 4, 15); }
		}

		public override ZDateTime A_ARV
		{
			get { return new ZDateTime(2008, 4, 22); }
		}

		public override ZDateTime A_DEP
		{
			get { return new ZDateTime(2008, 4, 30); }
		}
		public override ZDateTime LCLReceivalCommences
		{
			get { return new ZDateTime(2008, 4, 14); }
		}

		public override ZDateTime LCLCutOff
		{
			get { return new ZDateTime(2008, 4, 15); }
		}

		public override ZDateTime FCLReceivalCommences
		{
			get { return new ZDateTime(2008, 4, 16); }
		}

		public override ZDateTime FCLCutOff
		{
			get { return new ZDateTime(2008, 4, 17); }
		}

		public override ZDateTime FCLAvailabilityDate
		{
			get { return new ZDateTime(2008, 4, 18); }
		}

		public override ZDateTime FCLStorageDate
		{
			get { return new ZDateTime(2008, 4, 19); }
		}

		public override JobDocAddress GetCartageAddress(ZString orgType)
		{
			if (addressesByOrgType == null)
			{
				if (address == null)
				{
					address = CartageParent.Factory.New<JobDocAddress>();
					address.E2_OA_Address = CartageParent.Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
				}
				return address;
			}
			else
			{
				JobDocAddress result = null;
				addressesByOrgType.TryGetValue(orgType, out result);
				return result ?? CartageParent.Factory.New<JobDocAddress>();
			}
		}
		JobDocAddress address;

		protected override ZPropertyInfo[] GetCartageAddressInfosToMonitor()
		{
			return Array.Empty<ZPropertyInfo>();
		}

		public override ZDateTime LCLAvailabilityDate
		{
			get { return new ZDateTime(2008, 4, 20); }
		}

		public override ZDateTime LCLStorageDate
		{
			get { return new ZDateTime(2008, 4, 21); }
		}

		public override ZString PortOfDischarge
		{
			get { return "AUSYD"; }
		}

		public override ZString PortOfLoading
		{
			get { return "NZAKL"; }
		}

		public override ZString Vessel
		{
			get { return ""; }
		}

		public override ZString VoyageFlight
		{
			get { return ""; }
		}

		public override ZString DropMode
		{
			get { return dropMode; }
		}

		public void SetDropMode(ZString dropMode)
		{
			this.dropMode = dropMode;
		}
		ZString dropMode;

		public override IEnumerable<ZString> GetMatchingDirectionCodes()
		{
			return matchingDirectionCodes;
		}

		readonly IEnumerable<ZString> matchingDirectionCodes = Array.Empty<ZString>();
	}
}
