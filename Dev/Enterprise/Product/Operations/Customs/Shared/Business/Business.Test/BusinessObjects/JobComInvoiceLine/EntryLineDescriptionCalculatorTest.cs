using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class EntryLineDescriptionCalculatorTest : TestCaseWithFactory
	{
		public void TestDescriptionFromOverride()
		{
			Calculator.DescriptionOverrideDelegate = GetTuple(() => "12345");
			Calculator.CalculateDescription();
			AssertEquals("12345", Calculator.Description);
		}

		public void TestDescriptionFromPart()
		{
			Calculator.Part = Part;
			Calculator.Class = Class;
			Calculator.CalculateDescription();
			AssertEquals("PART", Calculator.Description);
		}
		public void TestDescriptionFromPartWithPrefix()
		{
			Calculator.Declaration = Factory.New<BaseJobDeclaration>();
			Calculator.Part = Part;
			Calculator.Class = Class;
			Calculator.Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.PartNumberUsingProductNumberInDescription;
			Calculator.CalculateDescription();
			AssertEquals("NUM - PART", Calculator.Description);
			Calculator.Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.PartNumber;
			Calculator.CalculateDescription();
			AssertEquals("PART", Calculator.Description);
			Calculator.Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMergeUsingProductNumberInDescription;
			Calculator.CalculateDescription();
			AssertEquals("NUM - PART", Calculator.Description);
			Calculator.Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			Calculator.CalculateDescription();
			AssertEquals("PART", Calculator.Description);
			Calculator.Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			Calculator.CalculateDescription();
			AssertEquals("PART", Calculator.Description);

			Calculator.DescriptionOverrideDelegate = Tuple.Create<ZStringReturner, ZString>(() => "Fallback Description", "Description");
			Calculator.Part = null;
			Calculator.Class = null;
			Calculator.Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMergeUsingProductNumberInDescription;
			Calculator.CalculateDescription();
			AssertEquals("Fallback Description", Calculator.Description);
			Calculator.Part = Part;
			Calculator.CalculateDescription();
			AssertEquals("NUM - Fallback Description", Calculator.Description);
		}

		public void TestDescriptionFromPartButUsingClassDesc()
		{
			Calculator.Declaration = Factory.New<BaseJobDeclaration>();
			Calculator.Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.ClassificationUsingClassificationDescriptionAlways;
			Calculator.Part = Part;
			Calculator.Class = Class;
			Calculator.CalculateDescription();
			AssertEquals("CLASS", Calculator.Description);
		}

		public void TestDescriptionFromClass()
		{
			Calculator.Class = Class;
			Calculator.TariffDescriptionDelegate = GetTuple(() => "TARIFF");
			Calculator.CalculateDescription();
			AssertEquals("CLASS", Calculator.Description);
		}

		public void TestDescriptionFromTariff()
		{
			var declarationMoq = Factory.NewMoq<BaseJobDeclaration>();
			var declaration = declarationMoq.Object;
			Calculator.Declaration = declaration;

			Calculator.TariffDescriptionDelegate = GetTuple(() => "TARIFF");
			Calculator.CalculateDescription();
			AssertEquals("TARIFF", Calculator.Description);

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.ClassificationUsingClassificationDescriptionAlways;
			Calculator.DescriptionOverrideDelegate = Tuple.Create<ZStringReturner, ZString>(() => "Fallback Description", "Description");
			Calculator.CalculateDescription();
			AssertEquals("TARIFF", Calculator.Description);

			declarationMoq.Setup(x => x.UseTariffDescriptionForEntryLine).Returns(false);
			Calculator.CalculateDescription();
			AssertEquals("Fallback Description", Calculator.Description);
		}

		Tuple<ZStringReturner, ZString> GetTuple(ZStringReturner getDescription)
		{
			return Tuple.Create(getDescription, ZString.Empty);
		}

		EntryLineDescriptionCalculator fCalculator;
		EntryLineDescriptionCalculator Calculator
		{
			get
			{
				if (fCalculator == null)
				{
					fCalculator = new EntryLineDescriptionCalculator();
				}
				return fCalculator;
			}
		}

		OrgSupplierPart fPart;
		OrgSupplierPart Part
		{
			get
			{
				if (fPart == null)
				{
					fPart = Factory.New<OrgSupplierPart>();
					fPart.OP_Desc = "PART";
					fPart.OP_PartNum = "NUM";
				}
				return fPart;
			}
		}

		BaseCusClassification fClass;
		BaseCusClassification Class
		{
			get
			{
				if (fClass == null)
				{
					fClass = Factory.New<BaseCusClassification>();
					fClass.CC_Description = "CLASS";
				}
				return fClass;
			}
		}
	}
}
