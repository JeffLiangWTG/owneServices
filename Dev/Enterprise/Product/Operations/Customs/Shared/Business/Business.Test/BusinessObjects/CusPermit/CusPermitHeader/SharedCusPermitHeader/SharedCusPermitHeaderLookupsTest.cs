using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class SharedCusPermitHeaderLookupsTest<TSharedCusPermitHeaderLookups, TSharedCusPermitHeader> : BusinessObjectLookupsTestCase
			where TSharedCusPermitHeaderLookups : SharedCusPermitHeaderLookups
			where TSharedCusPermitHeader : SharedCusPermitHeader
	{
		public void TestPermitQtyValIndicators()
		{
			AssertType<PermitQtyValIndicatorList>(PermitHeader.Lookups.PermitQtyValIndicators);
		}

		public void TestPermitTransactionCategories()
		{
			AssertType<PermitTransactionCategoryList>(PermitHeader.Lookups.PermitTransactionCategories);
		}

		public virtual void TestPermitTypes()
		{
			AssertType<PermitTypeList>(PermitHeader.Lookups.PermitTypes);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.SouthAfrica))
			{
				AssertEquals("Enterprise.Customs.ZA.Business.PermitTypeList", Factory.New<BaseCusPermitHeader>().Lookups.PermitTypes.GetType().ToString());
			}
		}

		public void TestAppliesToList()
		{
			AssertType<OrgHeaderCollection>(PermitHeader.Lookups.AppliesToList);
		}

		public void TestPermitSubTypes()
		{
			AssertType<PermitSubTypeList>(PermitHeader.Lookups.PermitSubTypes);
		}

		public void TestPermitNumberCollection()
		{
			AssertNull(PermitHeader.Lookups.PermitNumberCollection);
		}

		#region Implementation

		protected abstract TSharedCusPermitHeader GetNewPermitHeader(BusinessObjectFactory factory);

		protected override void SetUp()
		{
			base.SetUp();
			PermitHeader = GetNewPermitHeader(Factory);
		}

		protected TSharedCusPermitHeader PermitHeader { get; set; }

		#endregion
	}
}
