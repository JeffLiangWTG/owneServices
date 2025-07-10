using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(FoodData))]
	sealed class FoodDataTest : Customs.Business.Testing.CusCodeDataTest<FoodData>
	{
		[ExpectNoExceptions]
		public void TestValidation()
		{
			NUnit.Framework.Assert.That(foodData.Validation.GetType(), NUnit.Framework.Is.EqualTo(typeof(FoodDataValidation)), "Validation");
		}

		[ExpectNoExceptions]
		public void TestSetDefaultValues()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(foodData.CY_Type, NUnit.Framework.Is.EqualTo(CusCodeDataTypeList.Codes.Food).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(foodData.CY_ParentTableCode, NUnit.Framework.Is.EqualTo(JobComInvoiceLineSchema.Constants.Prefix).Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestParent()
		{
			NUnit.Framework.Assert.That(foodData.Parent, NUnit.Framework.Is.EqualTo(jobComInvoiceLine));
		}

		[ExpectNoExceptions]
		public void TestContent()
		{
			foodData.Content = 999.3443m;
			NUnit.Framework.Assert.That(foodData.Content, NUnit.Framework.Is.EqualTo(999.3443m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(typeof(FoodData), CustomConstraints.HasCustomAttribute<DecimalPlacesAttribute>("Content", true, attrib => attrib.DecimalPlaces == 4));
			NUnit.Framework.Assert.That(typeof(FoodData), CustomConstraints.HasCustomAttribute<DecimalPrecisionAttribute>("Content", true, attrib => attrib.DecimalPrecision == 14));
		}

		public void TestCY_Data()
		{
			AssertNoExceptionThrown(() => foodData.CY_Data = ZString.Replicate('X', 500));
			AssertExceptionThrown<MaxLengthExceededException>(() => foodData.CY_Data += 'X');
			ExceptionReporterTestListener.Instance.Clear();
		}

		[ExpectNoExceptions]
		public void TestBizoCaptionsAndDescriptions()
		{
			CombineAssertions(() =>
			{
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(foodData.CY_DataInfo, "Ingredient", "The name of the ingredient or food additive of the product.");
				BusinessObjectCaptionTestHelper.AssertCaptions(foodData.CY_CodeInfo, "Content");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(foodData.ContentInfo, "Content", "The content of the ingredient or food additive of the product.");
			});
		}

		protected override IEnumerable<FoodData> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return factory.New<JobDeclaration>().JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew().FoodDataCollection.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return jobComInvoiceLine.FoodDataCollection.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			jobComInvoiceLine = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			foodData = jobComInvoiceLine.FoodDataCollection.AddNew();
		}

		JobComInvoiceLine jobComInvoiceLine;
		FoodData foodData;
	}
}
