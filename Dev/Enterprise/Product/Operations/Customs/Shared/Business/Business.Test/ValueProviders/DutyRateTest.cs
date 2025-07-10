using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.MacroValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.ValueProviders.Testing
{
	[TestedType(typeof(DutyRate))]
	sealed class DutyRateTest : ValueProviderWithLoadControlFactoryTest<DutyRate>
	{
		[TestDate(2008, 06, 06)]
		public void TestRateFromCusClassificationAU()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);

			DutyRate valueProvider = new DutyRate();
			BusinessObjectFactory factory = valueProvider.FactoryForTesting;
			BusinessObject classification = (BusinessObject)factory.New<Integration.Customs.AU.IClassification>();
			classification[CusClassificationSchema.CC_ClassificationType] = "IMP";
			classification[CusClassificationSchema.CC_TariffNum] = "4201.00.00 01";

			AssertEquals("Duty Rate for 4201.00.00 01 on Classification", "5.00000", valueProvider.GetReplacement("<DutyRate(CusClassification, " + classification.PK + ")>", Report));
		}

		[TestDate(2008, 06, 06)]
		public void TestRateFromOrgSupplierPartAU()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);

			DutyRate valueProvider = new DutyRate();
			BusinessObjectFactory factory = valueProvider.FactoryForTesting;
			BusinessObject part = (BusinessObject)factory.New<Integration.Customs.AU.IOrgSupplierPart>();
			BusinessObject classification = (BusinessObject)factory.New<Integration.Customs.AU.IClassification>();
			BusinessObject pivot = (BusinessObject)factory.New<Integration.Customs.AU.ICusClassPartPivot>();
			pivot[CusClassPartPivotSchema.CI_CC] = classification.PK;
			pivot[CusClassPartPivotSchema.CI_OP] = part.PK;
			classification[CusClassificationSchema.CC_ClassificationType] = "IMP";
			classification[CusClassificationSchema.CC_TariffNum] = "4201.00.00 01";
			AssertEquals("Duty Rate for 4201.00.00 01 on Part", "", valueProvider.GetReplacement("<DutyRate(OrgSupplierPart, " + pivot.PK + ")>", Report));
		}

		[TestDate(2008, 06, 06)]
		public void TestRateFromCusClassificationNZ()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.NewZealand);

			DutyRate valueProvider = new DutyRate();
			BusinessObjectFactory factory = valueProvider.FactoryForTesting;
			BusinessObject classification = (BusinessObject)factory.New<Integration.Customs.NZ.ICusClassification>();
			classification[CusClassificationSchema.CC_TariffNum] = "4201.00.00.01B";

			AssertEquals("Duty Rate for 4201.00.00.01B on Classification", "7.00%", valueProvider.GetReplacement("<DutyRate(CusClassification, " + classification.PK + ")>", Report));
		}

		[TestDate(2008, 6, 6)]
		public void TestRateFromOrgSupplierPartNZ()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.NewZealand);
			var valueProvider = new DutyRate();
			var factory = valueProvider.FactoryForTesting;
			var part = (BusinessObject)factory.New<Integration.Customs.NZ.IOrgSupplierPart>();
			var classification = (BusinessObject)factory.New<Integration.Customs.NZ.ICusClassification>();
			var pivot = (BusinessObject)factory.New<Integration.Customs.NZ.ICusClassPartPivot>();
			pivot[CusClassPartPivotSchema.CI_CC] = classification.PK;
			pivot[CusClassPartPivotSchema.CI_OP] = part.PK;
			classification[CusClassificationSchema.CC_TariffNum] = "4201.00.00.01B";
			AssertEquals("Duty Rate for 4201.00.00.01B on Part", "", valueProvider.GetReplacement("<DutyRate(OrgSupplierPart, " + part.PK + ")>", Report));
		}

		public override void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<DutyRate(AField)>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<dutrate>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<   duty rate   >", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<   dutyrate somefield   >", Passes.FirstPass));
			Assert("should match ", ValueProviderToTest.IsResponsibleForReplacing("<   DutyRate  \t  (     fld , dd )   >", Passes.SecondPass));
			Assert("should match ", ValueProviderToTest.IsResponsibleForReplacing("<   DutyRate(AField, other)   >", Passes.SecondPass));
		}

		public override void TestReplacement()
		{
			ValueProvider = (DutyRate)ValueProviderToTest;
			PrepareRenderer();
			ValueProviderFactory.New<Integration.Customs.AU.IOrgSupplierPart>();
			var supplier = (BusinessObject)ValueProviderFactory.Load<Integration.Customs.AU.IOrgSupplierPart>(new ZQuery())[0];

			AssertEquals("", ValueProvider.GetReplacement("<DutyRate( ORGSUPPLIERPART , " + supplier.PK + ")>", Report));
		}

		[TestDate(2008, 06, 06)]
		public override void TestDocumentation()
		{
			base.TestDocumentation();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			var factory = ((DutyRate)ValueProviderToTest).FactoryForTesting;
			var classification = (BusinessObject)factory.New<Integration.Customs.AU.IClassification>();
			classification[CusClassificationSchema.CC_ClassificationType] = "IMP";
			classification[CusClassificationSchema.CC_TariffNum] = "4201.00.00 01";
			classificationPK = classification.PK.ToGuid();
		}

		Guid classificationPK;

		protected override void AssertExamplesAreReplacedAsExpected(string example, object expectedResult)
		{
			var actualExample = example.Replace("<InvoiceLine.JI_OP>", classificationPK.ToString());
			AssertIsReplacedWith(expectedResult, actualExample);
		}
	}
}
