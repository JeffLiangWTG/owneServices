using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Rating;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RatingDocumentsChargeGroupingOrRollupCollection))]
	public class RatingDocumentsChargeGroupingOrRollupCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var newElement = Factory.New<RatingDocumentsChargeGroupingOrRollup>();
			newElement.RCG_OB_CompanyData = CompanyData.PK;
			newElement.RCG_Module = DocRollupOrSortModuleList.Codes.All;
			newElement.RCG_JobType = DocRollupOrSortJobTypeList.Codes.All;
			newElement.RCG_TransportMode = DocRollupOrSortTransportModeList.Codes.All;
			newElement.RCG_Display = DocRollupOrSortDisplayList.Codes.Default;
			newElement.RCG_Style = DocRollupOrSortStyleList.Codes.Default;

			return newElement;
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new RatingDocumentsChargeGroupingOrRollupCollection(CompanyData);

		public void TestDefaultsForChildren()
		{
			var newElement = (RatingDocumentsChargeGroupingOrRollup)(GetCollectionToTest().AddNew());
			AssertEquals("RCG_OB_CompanyData: ", CompanyData.PK, newElement.RCG_OB_CompanyData);
			AssertEquals("RCG_Module: ", ZString.Empty, newElement.RCG_Module);
			AssertEquals("RCG_JobType: ", ZString.Empty, newElement.RCG_JobType);
			AssertEquals("RCG_TransportMode: ", ZString.Empty, newElement.RCG_TransportMode);
			AssertEquals("RCG_Display: ", ZString.Empty, newElement.RCG_Display);
			AssertEquals("RCG_Style: ", ZString.Empty, newElement.RCG_Style);
		}

		#region Implementation

		protected OrgCompanyData CompanyData => companyData ??= Factory.New<OrgCompanyData>();
		OrgCompanyData companyData;

		#endregion
	}
}
