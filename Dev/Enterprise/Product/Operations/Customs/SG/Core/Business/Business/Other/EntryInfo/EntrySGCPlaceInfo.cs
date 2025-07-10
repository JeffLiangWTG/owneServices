
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.SG.V4.Business
{
	public class EntrySGCPlaceInfo : ISGCPlace
	{
		public EntrySGCPlaceInfo(ZString placeCode)
		{
			this.placeCode = placeCode;
		}
		readonly ZString placeCode;

		#region Place

		ZZRefCusCodeListCombined Place
		{
			get
			{
				if (place == null && !placeCode.IsEmpty)
				{
					place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, placeCode);
				}
				return place;
			}
		}
		ZZRefCusCodeListCombined place;

		#endregion

		#region Factory

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;

		#endregion

		#region ISGCPlace Members

		public ZString Code
		{
			get { return Place != null ? Place.ZZD_Code : placeCode; }
		}

		public ZString Type
		{
			get { return Place.GetSGCType(); }
		}

		public ZString NameAndAddress
		{
			get { return Place != null ? Place.ZZD_Description : ZString.Empty; }
		}

		public ZBool AddressRequired
		{
			get { return Place != null && Place.ZZD_Code != Place.GetSGCType(); }
		}

		public ZBool IsNonSystemNonLicenced
		{
			get { return Place.IsNonSystemNonLicenced(); }
		}

		#endregion
	}
}
