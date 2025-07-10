using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Rating.DataTransfer.Testing
{
	[TestedType(typeof(ClientRateDataContextManager))]
	public abstract class RatingHeaderDataContextManagerTestCase<T, U> : DataContextManagerTestCase<T, U>
		where T : DataContextManager<U>, new()
		where U : RatingHeader
	{
		public void TestDataContextKey_LocalRate()
		{
			var testData = GetDataForTest(isGlobalRate: false);
			TestDataContextKey(testData);
		}

		public virtual void TestDataContextKey_GlobalRate()
		{
			var testData = GetDataForTest(isGlobalRate: true);
			TestDataContextKey(testData);
		}

		void TestDataContextKey(DataForTest testData)
		{
			var clientRateDataContextManager = GetNewContextManagerForTesting();
			((IDataContextManager)clientRateDataContextManager).Init(testData.BusinessObject);

			AssertEquals(testData.ExpectedDataContextKey, clientRateDataContextManager.DataContextKey);
		}

		public void TestLoadBusinessObjectFromDataSource_LocalRate()
		{
			var testData = GetDataForTest(isGlobalRate: false);
			TestLoadBusinessObjectFromDataSource(testData);
		}

		public virtual void TestLoadBusinessObjectFromDataSource_GlobalRate()
		{
			var testData = GetDataForTest(isGlobalRate: true);
			TestLoadBusinessObjectFromDataSource(testData);
		}

		void TestLoadBusinessObjectFromDataSource(DataForTest testData)
		{
			var topLevelDO = new Mock<ITopLevelDataObject>();
			topLevelDO.Setup(t => t.DataContext).Returns(Mock.Of<IDataContextDataObject>());

			var dataSource = new Mock<IDataSourceDataObject>();
			dataSource.Setup(d => d.Key).Returns(testData.ExpectedDataContextKey);

			var bizOsFromDataSource = GetNewContextManagerForTesting().LoadBusinessObjectFromDataSource(topLevelDO.Object, dataSource.Object, Factory.BOFactory, Mock.Of<IXmlImportLogger>());
			AssertContainsExactElementsInAnyOrder(new[] { testData.BusinessObject }, bizOsFromDataSource);
		}

		protected abstract RatingHeaderDataContextManager<U> GetNewContextManagerForTesting();

		protected abstract U GetBusinessObjectForTesting();

		DataForTest GetDataForTest(bool isGlobalRate)
		{
			var bizO = GetBusinessObjectForTesting();

			var orgHeaderPk = CreateAndSetOrgHeaderForTestData(bizO);
			var quoteNumber = CreateAndSetQuoteNumberForTestData(bizO);
			var companyPk = CreateAndSetCompanyForTestData(bizO, isGlobalRate);

			var keyParts = new[]
			{
				orgHeaderPk.IsEmpty ? string.Empty : orgHeaderPk.ToString(),
				companyPk,
				bizO.RateTypeSafe(),
				quoteNumber,
				bizO.TH_GlobalRateLevel.ToString()
			};

			Factory.SaveForTesting();

			return new DataForTest
			{
				BusinessObject = bizO,
				ExpectedDataContextKey = string.Join("~", keyParts)
			};
		}

		protected virtual ZGuid CreateAndSetOrgHeaderForTestData(RatingHeader bizO)
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "PROVIDER";
			bizO.TH_OH = orgHeader.PK;

			return orgHeader.PK;
		}

		/// <summary>
		/// Only Quotes should have a QuoteNumber.
		/// </summary>
		protected virtual string CreateAndSetQuoteNumberForTestData(RatingHeader bizO)
		{
			bizO.TH_QuoteNumber = string.Empty;
			return string.Empty;
		}

		static string CreateAndSetCompanyForTestData(RatingHeader bizO, bool isGlobalRate)
		{
			var companyPk = string.Empty;
			if (isGlobalRate)
			{
				bizO.TH_GC = ZGuid.Empty;
			}
			else
			{
				companyPk = GlbCompany.CurrentCompany.PK.ToString();
				AssertEquals("Precondition: Current Company PK should be set to TH_GC by NewWithValidTestData", companyPk, bizO.TH_GC.ToString());
			}

			return companyPk;
		}

		struct DataForTest
		{
			public U BusinessObject;
			public ZString ExpectedDataContextKey;
		}
	}
}
