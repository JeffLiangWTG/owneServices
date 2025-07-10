using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class RefVesselLookups : AutoRefVesselLookups
	{
		public RefVesselLookups(AutoRefVessel parent) : base(parent)
		{
		}

		public CodeDescriptionPairList RV_VesselType_List
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.VesselType); }
		}

		TransportShippingProviderCollection fRV_ShippingLine_List;
		public TransportShippingProviderCollection RV_ShippingLine_List
		{
			get
			{
				if (fRV_ShippingLine_List == null)
				{
					fRV_ShippingLine_List = new TransportShippingProviderCollection(Factory);
				}

				return fRV_ShippingLine_List;
			}
		}

		RefCountryCollection fRV_RN_List;
		public RefCountryCollection RV_RN_List
		{
			get
			{
				if (fRV_RN_List == null)
				{
					fRV_RN_List = new RefCountryCollection(Factory);
				}
				return fRV_RN_List;
			}
		}

		RefCarrierConsortiumCollection fRV_RG_List;
		public RefCarrierConsortiumCollection RV_RG_List
		{
			get
			{
				if (fRV_RG_List == null)
				{
					fRV_RG_List = new RefCarrierConsortiumCollection(Factory);
				}
				return fRV_RG_List;
			}
		}

		CodeDescriptionPairList fScreeningStatusesList;
		public CodeDescriptionPairList ScreeningStatusesList
		{
			get { return fScreeningStatusesList ?? (fScreeningStatusesList = new ScreeningStatusesList()); }
		}
	}
}
