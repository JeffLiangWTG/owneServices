using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business
{
	public class PackingHouseLookups : CusCodeDataLookups
	{
		public PackingHouseLookups(AutoCusCodeData parent) : base(parent)
		{
		}

		new PackingHouse Parent => base.Parent as PackingHouse;

		public new IBusinessObjectCollection CY_CodeList
		{
			get
			{
				var invoiceline = Parent.Parent;
				return TWRefCusCodeListTypes.GetPackingHouseList(Factory, new ZString(UniversalReferenceConstants.RefCusCodeListAttributes.Country),
					invoiceline.JI_CountryOfOrigin, new ZString(UniversalReferenceConstants.RefCusCodeListAttributes.Tariff), invoiceline.JI_Tariff);
			}
		}
	}
}
