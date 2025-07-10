using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class TWWhsDataTestHelper : Customs.Business.Testing.WhsDataTestHelper<JobDeclaration, OrgSupplierPart, CusClassification, Customs.Business.BaseCusClassPartPivot>
	{
		public TWWhsDataTestHelper()
		{
		}

		public TWWhsDataTestHelper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override string CountryCode
		{
			get { return Core.Constants.CountryCodes.Taiwan; }
		}
	}
}
