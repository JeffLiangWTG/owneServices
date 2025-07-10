using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(SGCustomsNumberViewStmNumsWrapperCollection))]
	public class SGCustomsNumberViewStmNumsWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<SGCustomsNumberViewStmNumsWrapperCollection>
	{
		CustomsNumberViewStmNumsBusinessProvider Provider => provider ?? (provider = Company.CustomsNumberProvider);
		CustomsNumberViewStmNumsBusinessProvider provider;
		GlbCompany Company => company ?? (company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_RN_NKCountryCode, Core.Constants.CountryCodes.Singapore)));
		GlbCompany company;
		protected override SGCustomsNumberViewStmNumsWrapperCollection GetCollectionToTest()
		{
			return (SGCustomsNumberViewStmNumsWrapperCollection)Provider.CustomsNumberWrappers;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var stmNum = Provider.CustomsNumbers.AddNew();
			return Provider.GetOrCreateWrapper(stmNum);
		}
	}
}
