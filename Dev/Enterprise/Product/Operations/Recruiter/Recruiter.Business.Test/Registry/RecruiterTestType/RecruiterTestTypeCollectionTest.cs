using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(RecruiterTestTypeCollection))]
	sealed class RecruiterTestTypeCollectionTest : RegistryBusinessObjectCollectionTestCase<RecruiterTestTypeCollection>
	{
		public void TestGetCodesFromCategory()
		{
			Collection.Add("ABC", "ABC Description", RecruiterTestCategory.Codes.Accreditation, "", true);
			Collection.Add("xyz", "xyz Description", RecruiterTestCategory.Codes.Assessment, "", true);
			Collection.Add("DDD", "DDD Description", RecruiterTestCategory.Codes.Accreditation, "", false);
			Collection.Add(">.<", ">.< Description", RecruiterTestCategory.Codes.SkillTest, "", true);

			AssertEquals("Get codes from category: ACC", 2, Collection.GetCodesFromCategory(RecruiterTestCategory.Codes.Accreditation).Length);
			AssertEquals("Code 1 from category: ACC", "ABC", Collection.GetCodesFromCategory(RecruiterTestCategory.Codes.Accreditation)[0]);
			AssertEquals("Code 2 from category: ACC", "DDD", Collection.GetCodesFromCategory(RecruiterTestCategory.Codes.Accreditation)[1]);

			AssertEquals("Get codes from category: ASS", 1, Collection.GetCodesFromCategory(RecruiterTestCategory.Codes.Assessment).Length);
			AssertEquals("Code 1 from category: ASS", "xyz", Collection.GetCodesFromCategory(RecruiterTestCategory.Codes.Assessment)[0]);

			AssertEquals("Get codes from category: SKL", 1, Collection.GetCodesFromCategory(RecruiterTestCategory.Codes.SkillTest).Length);
			AssertEquals("Code 1 from category: SKL", ">.<", Collection.GetCodesFromCategory(RecruiterTestCategory.Codes.SkillTest)[0]);

			AssertEquals("Get web visible codes from category: ACC", 1, Collection.GetWebVisibleCodesFromCategory(RecruiterTestCategory.Codes.Accreditation).Length);
			AssertEquals("Code 1 from category: ACC", "ABC", Collection.GetCodesFromCategory(RecruiterTestCategory.Codes.Accreditation)[0]);
		}

		public void TestGetCodes()
		{
			Collection.Add("FGH", "FGH Description", RecruiterTestCategory.Codes.Accreditation, "", true);
			Collection.Add("KLJ", "KLJ Description", RecruiterTestCategory.Codes.Assessment, "", false);

			AssertEquals("Get codes", 2, Collection.GetCodes().Length);
			AssertEquals("Code 1", "FGH", Collection.GetCodes()[0]);
			AssertEquals("Code 2", "KLJ", Collection.GetCodes()[1]);

			AssertEquals("Get codes", 1, Collection.GetWebVisibleCodes().Length);
			AssertEquals("Code 1", "FGH", Collection.GetCodes()[0]);
		}

		public void TestGetDescriptionFromCode()
		{
			Collection.Add("OOO", "OOO Description", RecruiterTestCategory.Codes.Accreditation);
			Collection.Add("TTT", "TTT Description", RecruiterTestCategory.Codes.Assessment);

			AssertEquals("Description 1 from code: OOO", "OOO Description", Collection.GetDescriptionFromCode("OOO"));
			AssertEquals("Description 2 from code: OOO", "TTT Description", Collection.GetDescriptionFromCode("TTT"));
		}

		public void TestGetCategoryFromCode()
		{
			Collection.Add("OOO", "OOO Description", RecruiterTestCategory.Codes.Accreditation);
			Collection.Add("TTT", "TTT Description", RecruiterTestCategory.Codes.Assessment);

			AssertEquals("Category 1 from code: OOO", RecruiterTestCategory.Codes.Accreditation, Collection.GetCategoryFromCode("OOO"));
			AssertEquals("Category 2 from code: OOO", RecruiterTestCategory.Codes.Assessment, Collection.GetCategoryFromCode("TTT"));
		}

		public void TestGetNewCollection()
		{
			var testFallBack = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			var collection = new RecruiterTestTypeCollection(testFallBack);
			var collection2 = collection.GetNewCollection();
			AssertEquals(testFallBack, collection2.CurrentFallbackLevel);
		}

		public void TestGetClone()
		{
			var testFallBack = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			var collection = new RecruiterTestTypeCollectionForTest(testFallBack);
			collection.CodeMaxLengthForTest = 30;
			var collection2 = (RecruiterTestTypeCollection)collection.GetCloneForTest(testFallBack, Factory);
			var recuriterTypeInClonedCollection = collection2.Add("ABC", "ABC description", RecruiterTestCategory.Codes.Accreditation);
			AssertEquals(testFallBack, collection2.CurrentFallbackLevel);
			AssertEquals(30, recuriterTypeInClonedCollection.CodeMaxLength);
		}

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override RecruiterTestTypeCollection GetCollectionToTest() => new RecruiterTestTypeCollection();

		protected override BusinessObject GetNewElementToAddToTheCollection() => new RecruiterTestType();

		sealed class RecruiterTestTypeCollectionForTest : RecruiterTestTypeCollection
		{
			public RecruiterTestTypeCollectionForTest(FallbackLevel fallbackLevel)
				: base(fallbackLevel)
			{
			}

			internal RegistryBusinessObjectCollectionTemplate GetCloneForTest(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => GetClone(fallbackLevel, factory);

			internal int CodeMaxLengthForTest
			{
				get { return CodeMaxLength; }
				set { CodeMaxLength = value; }
			}
		}
	}
}
