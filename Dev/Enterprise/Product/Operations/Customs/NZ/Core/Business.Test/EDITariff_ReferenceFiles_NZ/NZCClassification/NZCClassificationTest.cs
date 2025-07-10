using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ.Testing
{
	using Enterprise.ZArchitecture.Business.Testing;
	using NUnit.Framework;

	[TestedType(typeof(NZCClassification))]
	class NZCClassificationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDutyRates()
		{
			var classification = Factory.New<NZCClassification>();
			var rate = Factory.New<NZCClassificationDutyRate>();
			rate.U1_U0_Classification = classification.PK;
			AssertEquals(1, classification.DutyRates.Count);
		}

		public void TestCodeAndDescriptionAttributes()
		{
			NZCClassification classification = Factory.New<NZCClassification>();
			classification.U0_Tariff = "1111";
			classification.U0_Description = "Joo Is A Legend!!";
			AssertEquals("CodePropertyAttribute.CodeFromBusinessObject(classification)", "1111", CodePropertyAttribute.CodeFromBusinessObject(classification));
			AssertEquals("DescriptionPropertyAttribute.DescriptionFromBusinessObject(classification)", "Joo Is A Legend!!", DescriptionPropertyAttribute.DescriptionFromBusinessObject(classification));
		}
		public void TestStaticNewConstructor()
		{
			AssertEquals("Type check", GetExpectedBusinessObjectType(), NZCClassification.New(Factory).GetType());
		}

		public void TestSupportsNotes()
		{
			Assert(!ClassNew.SupportsNotes);
		}

		public void TestLevyCode()
		{
			ClassNew.U0_LevyCode = "TS";
			AssertEquals("TS", ClassNew.U0_LevyCode);
		}

		public void TestLevyRate()
		{
			ClassNew.U0_LevyRate = new ZDecimal(1.5);
			AssertEquals(new ZDecimal(1.5), ClassNew.U0_LevyRate);
		}

		public void TestLevyUnit()
		{
			ClassNew.U0_LevyUnit = "TST";
			AssertEquals("TST", ClassNew.U0_LevyUnit);
		}

		public void TestStatUnit()
		{
			ClassNew.U0_StatisticalUnit = "TST";
			AssertEquals(ClassNew.U0_StatisticalUnit, ClassNew.StatUnit);
		}

		public void TestSuppUnit()
		{
			ClassNew.U0_SupplementaryUnit = "TST";
			AssertEquals(ClassNew.U0_SupplementaryUnit, ClassNew.SuppUnit);
		}

		public void TestWrappedLongDescription()
		{
			ClassNew.U0_Description = "Long Description";
			AssertEquals(ClassNew.U0_Description, ClassNew.WrappedLongDescription);
		}

		public void TestShortDescription()
		{
			ClassNew.U0_Description = "Short Description";
			string expDesc = String.Format("{0} {1}", ClassNew.U0_Tariff, ClassNew.U0_Description);
			AssertEquals(expDesc, ClassNew.ShortDescription);
		}

		public void TestLongDescription()
		{
			ClassNew.U0_Description = "Long Description";
			AssertEquals(ClassNew.U0_Description, ClassNew.LongDescription);
		}

		public void TestHasChildren()
		{
			Assert(!ClassNew.HasChildren);
		}

		public void TestChildren()
		{
			AssertEquals("Expected children to have zero count", 0, ClassNew.Children.Length);
		}

		public void TestITariff_CodeAndDescription()
		{
			var classification = Factory.New<NZCClassification>();
			classification.U0_Tariff = "tariff";
			classification.U0_Description = "description";
			var tariff = (ITariff)classification;

			AssertEquals("tariff should be correct", "tariff", tariff.Code);
			AssertEquals("description should be correct", "description", tariff.Description);
		}

		public void TestITariff_UQ1()
		{
			var classification = Factory.New<NZCClassification>();
			classification.U0_StatisticalUnit = "abc";
			var tariff = (ITariff)classification;

			AssertEquals("UQ1 should be correct", "abc", tariff.UQ1);
			AssertEquals("UQ2 should be empty", "", tariff.UQ2);
			AssertEquals("UQ3 should be empty", "", tariff.UQ3);
			AssertEquals("UQ4 should be empty", "", tariff.UQ4);
			AssertEquals("UQ5 should be empty", "", tariff.UQ5);
		}

		public void TestGetHierarchy()
		{
			NZCClassification @class = NZCClassification.New(Factory);
			NZCClassificationChapter chapter = Factory.New<NZCClassificationChapter>();
			NZCClassificationSection section = Factory.New<NZCClassificationSection>();

			@class.U0_Tariff = "0101.10.00.11A";
			@class.U0_Description = "Test Tariff";
			@class.U0_DateActiveFrom = new ZDateTime(2000, 1, 1);
			@class.U0_DateActiveTo = ZDateTime.Empty;

			chapter.Q2_Chapter = "1";
			chapter.Q2_Description = "Test Chapter";
			chapter.Tariffs.Add(@class);

			section.Q1_Section = "01";
			section.Q1_Description = "Test Section";
			section.Chapters.Add(chapter);

			CargoWise.EntityFramework.IFamilyMember[] hierarchy = @class.GetHierarchy();
			AssertEquals("Expected three elements in the hierarchy", 3, hierarchy.Length);
			AssertEquals("Expected first element to be the class", @class, hierarchy[0]);
			AssertEquals("Expected second element to be the chapter", chapter, hierarchy[1]);
			AssertEquals("Expected third element to be the section", section, hierarchy[2]);
		}

		public void TestIsPetrolClassification()
		{
			NZCClassification classification = NZCClassification.New(Factory);
			classification.U0_Tariff = "2710.19.11.11F";
			AssertEquals("Classification.IsPetrolClassification", true, classification.IsPetrolClassification);
			classification.U0_Tariff = "2710.19.11.19A";
			AssertEquals("Classification.IsPetrolClassification", true, classification.IsPetrolClassification);
			classification.U0_Tariff = "2710.19.29.11B";
			AssertEquals("Classification.IsPetrolClassification", true, classification.IsPetrolClassification);
			classification.U0_Tariff = "2710.19.29.19H";
			AssertEquals("Classification.IsPetrolClassification", true, classification.IsPetrolClassification);
			classification.U0_Tariff = "2203.00.39.02K";
			AssertEquals("Classification.IsPetrolClassification", false, classification.IsPetrolClassification);
			classification.U0_Tariff = "2106.90.81.01J";
			AssertEquals("Classification.IsPetrolClassification", false, classification.IsPetrolClassification);
			classification.U0_Tariff = "8529.90.11.01H";
			AssertEquals("Classification.IsPetrolClassification", false, classification.IsPetrolClassification);
			classification.U0_Tariff = "";
			AssertEquals("Classification.IsPetrolClassification", false, classification.IsPetrolClassification);
		}

		public void TestU0_SupplementaryUnitForInvoiceLine()
		{
			NZCClassification classification = NZCClassification.New(Factory);
			AssertEquals("U0_SupplementaryUnitForInvoiceLine", "", classification.U0_SupplementaryUnitForInvoiceLine);

			classification.U0_Tariff = "2710.19.11.11F";
			AssertEquals("U0_SupplementaryUnitForInvoiceLine", "", classification.U0_SupplementaryUnitForInvoiceLine);

			classification.U0_Tariff = "";
			AssertEquals("U0_SupplementaryUnitForInvoiceLine", "", classification.U0_SupplementaryUnitForInvoiceLine);
		}

		public void TestGetPartialCode()
		{
			NZCClassification classOld = NZCClassification.New(Factory);
			classOld.U0_Tariff = "1234567890123Z";
			classOld.U0_DateActiveFrom = new ZDateTime(2000, 1, 1);
			classOld.U0_DateActiveTo = new ZDateTime(2003, 12, 31);

			NZCClassification classPrevious = NZCClassification.New(Factory);
			classPrevious.U0_Tariff = "1234567890123Z";
			classPrevious.U0_DateActiveFrom = new ZDateTime(2004, 1, 1);
			classPrevious.U0_DateActiveTo = new ZDateTime(2004, 12, 31);

			NZCClassification classNow = NZCClassification.New(Factory);
			classNow.U0_Tariff = "1234567890123Z";
			classNow.U0_DateActiveFrom = new ZDateTime(2005, 1, 1);
			classNow.U0_DateActiveTo = ZDateTime.Empty;

			NZCClassification @class = NZCClassification.GetClassForPartialCode(Factory, "12345", new ZDateTime(1974, 10, 25));
			AssertNull("Expected to return null for old barrier date", @class);

			@class = NZCClassification.GetClassForPartialCode(Factory, "12345", new ZDateTime(2002, 10, 25));
			AssertEquals("Expected old classification to be returned", classOld, @class);

			@class = NZCClassification.GetClassForPartialCode(Factory, "12345", new ZDateTime(2004, 10, 25));
			AssertEquals("Expected Previous classification to be returned", classPrevious, @class);

			@class = NZCClassification.GetClassForPartialCode(Factory, "12345", ZDateTime.Now);
			AssertEquals("Expected Current classification to be returned", classNow, @class);

			ZDateTime futureDate = ZDateTime.Today.AddYears(2);
			@class = NZCClassification.GetClassForPartialCode(Factory, "12345", futureDate);
			AssertEquals("Expected Current classification to be returned for future date", classNow, @class);

			@class = NZCClassification.GetClassForPartialCode(Factory, "12345", ZDateTime.Empty);
			AssertEquals("Expected Current classification to be returned for empty date", classNow, @class);

			@class = NZCClassification.GetClassForPartialCode(Factory, "12345", ZDateTime.Invalid);
			AssertNull("Expected null to be returned for invalid date", @class);

			classNow.U0_DateActiveTo = ZDateTime.Today;
			@class = NZCClassification.GetClassForPartialCode(Factory, "12345", futureDate);
			AssertNull("Expected null to be returned for future date with no matching classification date", @class);
		}

		public void TestGetCompleteCode()
		{
			NZCClassification classOld = NZCClassification.New(Factory);
			classOld.U0_Tariff = "1234567890123Z";
			classOld.U0_DateActiveFrom = new ZDateTime(2000, 1, 1);
			classOld.U0_DateActiveTo = new ZDateTime(2003, 12, 31);

			NZCClassification classPrevious = NZCClassification.New(Factory);
			classPrevious.U0_Tariff = "1234567890123Z";
			classPrevious.U0_DateActiveFrom = new ZDateTime(2004, 1, 1);
			classPrevious.U0_DateActiveTo = new ZDateTime(2004, 12, 31);

			NZCClassification classNow = NZCClassification.New(Factory);
			classNow.U0_Tariff = "1234567890123Z";
			classNow.U0_DateActiveFrom = new ZDateTime(2005, 1, 1);
			classNow.U0_DateActiveTo = ZDateTime.Empty;

			NZCClassification @class = NZCClassification.GetClassForCompleteCode(Factory, "1234567890123Z", new ZDateTime(1974, 10, 25));
			AssertNull("Expected to return null for old barrier date", @class);

			@class = NZCClassification.GetClassForCompleteCode(Factory, "1234567890123Z", new ZDateTime(2002, 10, 25));
			AssertEquals("Expected old classification to be returned", classOld, @class);

			@class = NZCClassification.GetClassForCompleteCode(Factory, "1234567890123Z", new ZDateTime(2004, 10, 25));
			AssertEquals("Expected previous classification to be returned", classPrevious, @class);

			@class = NZCClassification.GetClassForCompleteCode(Factory, "1234567890123Z", new ZDateTime(2005, 10, 25));
			AssertEquals("Expected current classification to be returned", classNow, @class);

			ZDateTime futureDate = ZDateTime.Today.AddYears(2);
			@class = NZCClassification.GetClassForCompleteCode(Factory, "1234567890123Z", futureDate);
			AssertEquals("Expected Current classification to be returned for future date", classNow, @class);

			@class = NZCClassification.GetClassForCompleteCode(Factory, "1234567890123Z", ZDateTime.Empty);
			AssertEquals("Expected current classification to be returned for empty barrier date", classNow, @class);

			classNow.U0_DateActiveTo = ZDateTime.Today;
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();   // second factory required as first factory has cache of details
			@class = NZCClassification.GetClassForCompleteCode(factory2, "1234567890123Z", futureDate);
			AssertNull("Expected null to be returned for future date with no matching classification date", @class);
		}

		public void TestGetClassThatHasDutyRatesAgainstIt()
		{
			NZCClassification classOld = NZCClassification.New(Factory);
			classOld.U0_Tariff = "1234567890123Z";
			classOld.U0_DateActiveFrom = new ZDateTime(2000, 1, 1);
			classOld.U0_DateActiveTo = new ZDateTime(2003, 12, 31);

			NZCClassification classPrevious = NZCClassification.New(Factory);
			classPrevious.U0_Tariff = "1234567890123Z";
			classPrevious.U0_DateActiveFrom = new ZDateTime(2004, 1, 1);
			classPrevious.U0_DateActiveTo = new ZDateTime(2004, 12, 31);

			NZCClassification classNow = NZCClassification.New(Factory);
			classNow.U0_Tariff = "1234567890123Z";
			classNow.U0_DateActiveFrom = new ZDateTime(2005, 1, 1);
			classNow.U0_DateActiveTo = ZDateTime.Empty;

			NZCClassification returnedClass = NZCClassification.GetClassThatHasDutyRatesAgainstIt(Factory, "1234567890123Z");
			AssertEquals("Returned Classification should be most recent by U0_DateActiveFrom", classNow.PK, returnedClass.PK);
		}

		public void TestGetDescription()
		{
			ClassNew.U0_Description = "LALALA";
			ClassNew.U0_DateActiveTo = ZDateTime.Empty;
			AssertEquals("Descriptions should match", ClassNew.U0_Description, NZCClassification.GetDescriptionForCompleteCode(Factory, ClassNew.U0_Tariff));
		}

		public void TestGetDescriptionReturnsComputerDumpDescription()
		{
			ClassNew.U0_ComputerDumpDescr = "BLAHBLAHLBAH";
			ClassNew.U0_Description = "LALALA";
			ClassNew.U0_DateActiveTo = ZDateTime.Empty;
			AssertEquals("Descriptions should match", ClassNew.U0_ComputerDumpDescr, NZCClassification.GetDescriptionForCompleteCode(Factory, ClassNew.U0_Tariff));
		}

		public void TestGetDescriptionReturnsEmptyStringForNullCode()
		{
			AssertEquals("Description should be empty string", "", NZCClassification.GetDescriptionForCompleteCode(Factory, ""));
		}

		public static NZCClassification CreateSGGTariff(BusinessObjectFactory factory)
		{
			var tariff = factory.New<NZCClassification>();
			tariff.U0_Tariff = "0000.00.00.00E";
			tariff.U0_DateActiveFrom = new ZDateTime(2011, 12, 12);
			tariff.U0_Description = "Greenhouse Gas, aka Ben ate beans";
			tariff.U0_StatisticalUnit = "NMB";

			var levy = factory.New<NZCClassificationLevyRate>();
			levy.L0_DateActiveFrom = tariff.U0_DateActiveFrom;
			levy.L0_LevyUnit = tariff.U0_StatisticalUnit;
			levy.L0_LevyRate = 8.55m;
			levy.L0_U0_Classification = tariff.PK;
			levy.L0_LevyCode = "GG";

			return tariff;
		}

		#region Implementation
		protected NZCClassification ClassNew
		{
			get
			{
				if (fClassNew == null)
				{
					fClassNew = Factory.New<NZCClassification>();
					fClassNew.U0_Tariff = "1234567890123Z";
					fClassNew.U0_DateActiveFrom = new ZDateTime(2004, 12, 1);
					fClassNew.U0_DateActiveTo = new ZDateTime(2004, 12, 31);
				}
				return fClassNew;
			}
		}
		NZCClassification fClassNew;

		#endregion Implementation
	}
}
