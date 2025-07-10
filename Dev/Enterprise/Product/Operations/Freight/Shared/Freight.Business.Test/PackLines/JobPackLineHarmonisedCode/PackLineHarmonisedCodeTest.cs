using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(JobPackLineHarmonisedCode))]
	sealed class PackLineHarmonisedCodeTest : EnterpriseBusinessObjectTestCase
	{
		#region IsEmptyItem

		public void TestIsEmptyItem()
		{
			var item = Factory.New<JobPackLineHarmonisedCode>();
			AssertEquals("Precondition", true, item.IsEmptyItem);

			item.JLH_RN_NKCountry = "FR";
			AssertEquals("JLH_RN_NKCountry", false, item.IsEmptyItem);
			item.JLH_RN_NKCountry = ZString.Empty;

			item.JLH_Code = "123";
			AssertEquals("JLH_Code", false, item.IsEmptyItem);
			item.JLH_Code = ZString.Empty;

			AssertEquals("All empty", true, item.IsEmptyItem);
		}

		#endregion

		#region Saving

		public void TestIsSavedByFactory()
		{
			var item = Factory.New<JobPackLineHarmonisedCode>();
			item.IsAutoAddedItem = true;
			AssertEquals(false, item.IsSavedByFactory);

			item.JLH_RN_NKCountry = "ZA";
			item.JLH_Code = "12345";
			AssertEquals(true, item.IsSavedByFactory);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return CreateSaveablePackLineHarmonisedCode(factory);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return CreateSaveablePackLineHarmonisedCode(Factory);
		}

		JobPackLineHarmonisedCode CreateSaveablePackLineHarmonisedCode(BusinessObjectFactory factory)
		{
			var packline = factory.NewWithValidTestData<PackLine>();

			var result = factory.NewWithValidTestData<JobPackLineHarmonisedCode>();
			result.JLH_JL = packline.PK;
			result.JLH_Code = "Hello!";
			result.JLH_RN_NKCountry = "MD";

			return result;
		}

		#endregion
	}
}
