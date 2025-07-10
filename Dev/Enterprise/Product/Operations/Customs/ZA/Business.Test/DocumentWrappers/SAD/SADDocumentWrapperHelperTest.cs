using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.DocumentWrappers;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers.Testing
{
	sealed class SADDocumentWrapperHelperTest : TestCaseWithFactory
	{
		public void TestConsolidateDutiesIfNeeded()
		{
			var wrapper1P1 = new DutyFeeInformationDocWrapper("1P1", 1m);
			var wrapper12A = new DutyFeeInformationDocWrapper("12A", 2m);
			var wrapper13A = new DutyFeeInformationDocWrapper("13A", 3m);
			var wrapperVAT = new DutyFeeInformationDocWrapper("VAT", 4m);
			var wrapper12B = new DutyFeeInformationDocWrapper("12B", 5m);
			var wrapperPPS = new DutyFeeInformationDocWrapper("PP's", 6m);
			var wrapperDTY = new DutyFeeInformationDocWrapper("DTY", 7m);

			var collection = new BusinessObjectCollectionWrapper<DutyFeeInformationDocWrapper>
			{
				wrapper1P1,
				wrapper12A,
				wrapper13A,
				wrapperVAT,
				wrapper12B
			};

			var newCollection = SADDocumentWrapperHelper.ConsolidateDutiesIfNeeded(collection);
			AssertEquals(5, newCollection.Count);
			AssertSame(wrapper1P1, newCollection[0]);
			AssertSame(wrapper12A, newCollection[1]);
			AssertSame(wrapper13A, newCollection[2]);
			AssertSame(wrapperVAT, newCollection[3]);
			AssertSame(wrapper12B, newCollection[4]);

			collection = new BusinessObjectCollectionWrapper<DutyFeeInformationDocWrapper>
			{
				wrapper1P1,
				wrapper12A,
				wrapper13A,
				wrapperVAT,
				wrapper12B,
				wrapperPPS
			};
			newCollection = SADDocumentWrapperHelper.ConsolidateDutiesIfNeeded(collection);
			AssertEquals(4, newCollection.Count);
			AssertDutyFeeInformationDocWrapper(newCollection[0], "DTY", 6m);
			AssertSame(wrapperVAT, newCollection[1]);
			AssertSame(wrapper12B, newCollection[2]);
			AssertSame(wrapperPPS, newCollection[3]);

			collection = new BusinessObjectCollectionWrapper<DutyFeeInformationDocWrapper>
			{
				wrapper1P1,
				wrapper12A,
				wrapper13A,
				wrapperVAT,
				wrapper12B,
				wrapperPPS,
				wrapperDTY
			};
			newCollection = SADDocumentWrapperHelper.ConsolidateDutiesIfNeeded(collection);
			AssertEquals(4, newCollection.Count);
			AssertDutyFeeInformationDocWrapper(newCollection[0], "DTY", 13m);
			AssertSame(wrapperVAT, newCollection[1]);
			AssertSame(wrapper12B, newCollection[2]);
			AssertSame(wrapperPPS, newCollection[3]);

			collection = new BusinessObjectCollectionWrapper<DutyFeeInformationDocWrapper>
			{
				wrapper1P1,
				wrapper12A,
				wrapper13A,
				wrapperVAT,
				wrapperDTY
			};
			newCollection = SADDocumentWrapperHelper.ConsolidateDutiesIfNeeded(collection);
			AssertEquals(2, newCollection.Count);
			AssertDutyFeeInformationDocWrapper(newCollection[0], "DTY", 13m);
			AssertSame(wrapperVAT, newCollection[1]);

			collection = new BusinessObjectCollectionWrapper<DutyFeeInformationDocWrapper>
			{
				wrapper1P1,
				wrapper12A,
				wrapper13A,
				wrapperVAT
			};
			newCollection = SADDocumentWrapperHelper.ConsolidateDutiesIfNeeded(collection);
			AssertEquals(4, newCollection.Count);
			AssertSame(wrapper1P1, newCollection[0]);
			AssertSame(wrapper12A, newCollection[1]);
			AssertSame(wrapper13A, newCollection[2]);
			AssertSame(wrapperVAT, newCollection[3]);
		}

		static void AssertDutyFeeInformationDocWrapper(DutyFeeInformationDocWrapper wrapper, ZString code, ZDecimal value)
		{
			AssertEquals(code, wrapper.Code);
			AssertEquals(value, wrapper.Value);
		}
	}
}
