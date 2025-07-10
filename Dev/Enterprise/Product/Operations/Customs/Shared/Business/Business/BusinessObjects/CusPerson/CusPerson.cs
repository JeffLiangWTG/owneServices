using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class CusPerson : AutoCusPerson
	{
		public CusPerson(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ChildEditable(true)]
		public ICusPersonCountryCollection<CusPersonCountry> Countries
		{
			get
			{
				if (countries == null)
				{
					countries = CreateNewCusPersonCountryCollection();
					countries.Load();
					RegisterEditableChildObject(countries);
				}
				return countries;
			}
		}
		ICusPersonCountryCollection<CusPersonCountry> countries;

		protected virtual ICusPersonCountryCollection<CusPersonCountry> CreateNewCusPersonCountryCollection() => new CusPersonCountryCollection<CusPersonCountry>(this);

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			base.CPN_ParentTableCode = "JE";
		}
#endif
	}
}
