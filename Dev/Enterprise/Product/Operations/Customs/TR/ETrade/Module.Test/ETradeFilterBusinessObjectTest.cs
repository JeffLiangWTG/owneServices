using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using AsycudaManifestHeader = Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader;

namespace Enterprise.Customs.TR.ETrade.Module.Testing
{
	[TestedType(typeof(ETradeFilterBusinessObject))]
	public class ETradeFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestToMatchTheFilters()
		{
			TestJobNumberFilter();
			TestTemporaryRegistrationNumberFilter();
			TestTemporaryRegistrationNumberDateFilter();
			TestRegistrationNumberFilter();
		}

		public void MaxLengthsOfTheFilters()
		{
			TestJobNumberFilterMaxLength();
			TestTemporaryRegistrationNumberFilterMaxLength();
			TestRegistrationNumberFilterMaxLength();
		}

		#region TestToMatchTheFilters

		public void TestJobNumberFilter()
		{
			var filterObj = new ETradeFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[ETradeFilterBusinessObject.FilterConstants.JobNumber];
			filter.IsActive = true;
			filter.Property = "ULU";

			CombineAssertions("Asserted to Match the Filters for JobNumber", () =>
			{
				Assert(eTradeHeader1.MatchesFilter(filterObj.Filter));
				Assert(!eTradeHeader2.MatchesFilter(filterObj.Filter));
			});
		}

		public void TestTemporaryRegistrationNumberFilter()
		{
			var filterObj = new ETradeFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[ETradeFilterBusinessObject.FilterConstants.TemporaryRegistrationNumber];

			filter.IsActive = true;
			filter.Property = "22000000123";

			CombineAssertions("Asserted to Match the Filters for Temporary Registration Number", () =>
			{
				Assert(eTradeHeader1.MatchesFilter(filterObj.Filter));
				Assert(!eTradeHeader2.MatchesFilter(filterObj.Filter));
			});
		}

		public void TestTemporaryRegistrationNumberDateFilter()
		{
			var filterObj = new ETradeFilterBusinessObject();
			var filter = (ModuleDateFilter)filterObj[ETradeFilterBusinessObject.FilterConstants.TemporaryRegistrationNumberDate];

			filter.IsActive = true;
			filter.Property1 = new CargoWise.Types.ZDateTime(2022, 03, 5);
			filter.Property2 = new CargoWise.Types.ZDateTime(2022, 03, 20);
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			CombineAssertions("Asserted to Match the Filters for Temporary Registration Number Date", () =>
			{
				Assert(eTradeHeader1.MatchesFilter(filterObj.Filter));
				Assert(!eTradeHeader2.MatchesFilter(filterObj.Filter));
			});
		}

		public void TestRegistrationNumberFilter()
		{
			var filterObj = new ETradeFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[ETradeFilterBusinessObject.FilterConstants.RegistrationNumber];

			filter.IsActive = true;
			filter.Property = "20001234";

			CombineAssertions("Asserted to Match the Filters for Registration Number", () =>
			{
				Assert(asycudaHeaderForTest1.MatchesFilter(filterObj.Filter));
				Assert(!asycudaHeaderForTest2.MatchesFilter(filterObj.Filter));
			});
		}

		#endregion

		#region MaxLengthsOfTheFilters

		public void TestJobNumberFilterMaxLength()
		{
			var filterObj = new ETradeFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[ETradeFilterBusinessObject.FilterConstants.JobNumber];

			AssertEquals("ETrade Filter Should Contain MaxLength for AMA_JobReference", AsycudaManifestHeaderSchema.AMA_JobReference.MaxLength, filter.MaxLength);
		}

		public void TestTemporaryRegistrationNumberFilterMaxLength()
		{
			var filterObj = new ETradeFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[ETradeFilterBusinessObject.FilterConstants.TemporaryRegistrationNumber];

			AssertEquals("ETrade Filter Should Contain MaxLength for CY_Data", Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader.Schema.TempRegNoMaxLength, filter.MaxLength);
		}

		public void TestRegistrationNumberFilterMaxLength()
		{
			var filterObj = new ETradeFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[ETradeFilterBusinessObject.FilterConstants.RegistrationNumber];

			AssertEquals("ETrade Filter Should Contain MaxLength for CE_EntryNum", Enterprise.Customs.Common.CusEntryNumber.Schema.CE_EntryNumMaxLength, filter.MaxLength);
		}

		#endregion

		public void TestExistanceOfTheFilters()
		{
			var filter = new ETradeFilterBusinessObject();

			CombineAssertions("Asserted to Check the Constants Existance", () =>
			{
				AssertNotNull(filter[ETradeFilterBusinessObject.FilterConstants.JobNumber]);
				AssertNotNull(filter[ETradeFilterBusinessObject.FilterConstants.TemporaryRegistrationNumber]);
				AssertNotNull(filter[ETradeFilterBusinessObject.FilterConstants.TemporaryRegistrationNumberDate]);
				AssertNotNull(filter[ETradeFilterBusinessObject.FilterConstants.RegistrationNumber]);
			});
		}

		AsycudaManifestHeader eTradeHeader1;
		AsycudaManifestHeader eTradeHeader2;
		ASYCUDA.Business.AsycudaManifestHeader asycudaHeaderForTest1;
		ASYCUDA.Business.AsycudaManifestHeader asycudaHeaderForTest2;

		void SetData()
		{
			eTradeHeader1 = Factory.New<AsycudaManifestHeader>();

			eTradeHeader1.AMA_ApplicationCode = Messaging.Integration.ApplicationCodeList.Codes.TRETrade;
			eTradeHeader1.AMA_JobReference = "ULU";
			eTradeHeader1.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;
			eTradeHeader1.TempRegNo = "22000000123";
			eTradeHeader1.TempRegNoDate = new CargoWise.Types.ZDateTime(2022, 03, 15);

			asycudaHeaderForTest1 = Factory.New<ASYCUDA.Business.AsycudaManifestHeader>();

			asycudaHeaderForTest1.AMA_ApplicationCode = Messaging.Integration.ApplicationCodeList.Codes.TRETrade;
			asycudaHeaderForTest1.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;
			asycudaHeaderForTest1.RegistrationNumber = "20001234";

			eTradeHeader2 = Factory.New<AsycudaManifestHeader>();

			eTradeHeader2.AMA_ApplicationCode = Messaging.Integration.ApplicationCodeList.Codes.TRETrade;
			eTradeHeader2.AMA_JobReference = "ABC";
			eTradeHeader2.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;
			eTradeHeader2.TempRegNo = "123123123";
			eTradeHeader2.TempRegNoDate = new CargoWise.Types.ZDateTime(2022, 2, 15);

			asycudaHeaderForTest2 = Factory.New<ASYCUDA.Business.AsycudaManifestHeader>();

			asycudaHeaderForTest2.AMA_ApplicationCode = Messaging.Integration.ApplicationCodeList.Codes.TRETrade;
			asycudaHeaderForTest2.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;
			asycudaHeaderForTest2.RegistrationNumber = "123123";

			Factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();
			SetData();
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ETradeFilterBusinessObject();
		}
	}
}
