using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	class ProductCodeQualifiersListTest : TestCaseWithFactory
	{
		public void TestGetAPHISProductCodeQualifiers()
		{
			var list1 = ProductCodeQualifiersList.GetAPHISProductCodeQualifiers(Factory);
			var list2 = ProductCodeQualifiersList.GetAPHISProductCodeQualifiers(Factory);
			AssertEquals("Is cached", list1, list2);
			var fullList = new CodeDescriptionPairList();
			fullList.AddPair(ProductCodeQualifiersList.Codes.APHISVeterinaryBiologics, ProductCodeQualifiersList.Descriptions.APHISVeterinaryBiologics);
			fullList.AddPair(ProductCodeQualifiersList.Codes.GlobalProductClasBrickCode, ProductCodeQualifiersList.Descriptions.GlobalProductClasBrickCode);
			fullList.AddPair(ProductCodeQualifiersList.Codes.UNStandardProductsServicesCode, ProductCodeQualifiersList.Descriptions.UNStandardProductsServicesCode);
			fullList.AddPair(ProductCodeQualifiersList.Codes.TaxonomicSerialNumber, ProductCodeQualifiersList.Descriptions.TaxonomicSerialNumber);
			AssertEquals("list1.Count", fullList.Count, list1.Count);
			foreach (ICodeDescription pair in fullList)
			{
				AssertEquals(pair.Code, pair.Description, list1.GetDescriptionFromCode(pair.Code));
			}
		}
	}
}
