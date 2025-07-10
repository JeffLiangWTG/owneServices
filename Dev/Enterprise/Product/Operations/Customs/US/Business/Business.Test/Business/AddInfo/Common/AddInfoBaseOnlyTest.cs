using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(AddInfoForTest))]
	sealed class AddInfoBaseOnlyTest : NonPersistentBusinessObjectTestCase
	{
		public void TestResetToOriginalValue()
		{
			addInfoDummy.US_CRLCertStatus = "A";
			Factory.Save();
			addInfoDummy.US_CRLCertStatus = "B";
			addInfoDummy.ResetToOriginalValue(USAddInfoSchema.US_CRLCertStatus);
			AssertEquals("US_CRLCertStatus reset to the value before save", "A", addInfoDummy.US_CRLCertStatus);
		}

		public void TestHasChangesSinceLastSaving()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			AssertEquals(false, declaration.HasAddInfoChangesSinceLastSaving(USAddInfoSchema.US_EnableCRL));
			declaration.US_EnableCRL = true;
			AssertEquals("not saved yet", false, declaration.HasAddInfoChangesSinceLastSaving(USAddInfoSchema.US_EnableCRL));
			Factory.Save();
			AssertEquals("not changed yet", false, declaration.HasAddInfoChangesSinceLastSaving(USAddInfoSchema.US_EnableCRL));
			declaration.US_EnableCRL = false;
			AssertEquals("changed yet", true, declaration.HasAddInfoChangesSinceLastSaving(USAddInfoSchema.US_EnableCRL));
		}

		public void TestUS_TransactionsRelated()
		{
			addInfoDummy.US_TransactionsRelated = "n";
			AssertEquals("US_TransactionsRelated", "N", addInfoDummy.US_TransactionsRelated);
			addInfoDummy.US_TransactionsRelated = "y";
			AssertEquals("US_TransactionsRelated", "Y", addInfoDummy.US_TransactionsRelated);
		}

		public void TestUS_RoutedTransaction()
		{
			addInfoDummy.US_RoutedTransaction = "n";
			AssertEquals("US_RoutedTransaction", "N", addInfoDummy.US_RoutedTransaction);
			addInfoDummy.US_RoutedTransaction = "y";
			AssertEquals("US_RoutedTransaction", "Y", addInfoDummy.US_RoutedTransaction);
		}

		public void TestUS_HazardousCargo()
		{
			addInfoDummy.US_HazardousCargo = "n";
			AssertEquals("US_HazardousCargo", "N", addInfoDummy.US_HazardousCargo);
			addInfoDummy.US_HazardousCargo = "y";
			AssertEquals("US_HazardousCargo", "Y", addInfoDummy.US_HazardousCargo);
		}

		public void TestUS_LicenseValue_Decimals()
		{
			addInfoDummy.US_LicenseValue = 99.99m;
			AssertEquals("US_LicenseValue is not rounded", 99.99m, addInfoDummy.US_LicenseValue);
		}

		protected override List<string> ColumnsToClearValueAfterTested
		{
			get
			{
				List<string> result = new List<string>();
				foreach (SchemaColumn column in USAddInfoSchema.All)
				{
					result.Add(column.Name);
				}

				return result;
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AddInfoForTest(classification.CC_AddInfoInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "TestLookup";
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			addInfoDummy = new AddInfoForTest(classification.CC_AddInfoInfo);
		}

		CusClassification classification;
		AddInfoForTest addInfoDummy;
	}
}
