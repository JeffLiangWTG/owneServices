using System;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	public abstract class RatingHeaderCollectionBaseTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new RatingHeaderCollection(Factory);
		}

		public void TestFindBoxListProvider()
		{
			var collection = (RatingHeaderCollection)GetCollectionToTest();
			AssertEquals(collection.GetType().Name + " has a different findbox list provider than expected.", GetExpectedFindBoxListProviderType(), collection.FindBoxListProvider_ForTest.GetType());
		}

		protected virtual Type GetExpectedFindBoxListProviderType()
		{
			return typeof(FindBoxListProvider);
		}

		public void TestRateType()
		{
			var collection = (RatingHeaderCollection)GetCollectionToTest();
			if (collection.GetType() == typeof(RatingHeaderCollection))
			{
				AssertEquals("", collection.RateType);
			}
			else
			{
				Assert(!collection.RateType.IsEmpty);
				var addNewMethod = collection.GetType().GetMethod("AddNew", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
				var header = (RatingHeader)addNewMethod.Invoke(collection, null);
				AssertNotNull(header);
				AssertEquals(collection.RateType, header.TH_RateType);

				var collection2 = (RatingHeaderCollection)GetCollectionToTest();
				collection2.Load();
				AssertEquals(1, collection2.Count);
				AssertEquals(header.PK, collection2[0].PK);
			}
		}

		public void TestHasFactoryOnlyConstractor()
		{
			var collectionType = GetCollectionToTest().GetType();

			AssertNotNull(collectionType.GetConstructor(new Type[] { typeof(BusinessObjectFactory) }));
		}

		#region Implementation

		protected TestHelper Helper
		{
			get
			{
				if (fHelper == null)
				{
					fHelper = new TestHelper(Factory);
				}

				return fHelper;
			}
		}

		TestHelper fHelper;

		#endregion
	}

	[TestedType(typeof(RatingHeaderCollection))]
	public class RatingHeaderCollectionTest : RatingHeaderCollectionBaseTest
	{
		public void TestCompanyFilter()
		{
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Code = "NEW";
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			company2.Branches.Add(branch1);
			ClientRate rate1;
			ClientRate rate2;
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				rate1 = Helper.NewClientRate(Helper.NewOrgHeader());
				rate2 = Helper.NewClientRate(Helper.NewOrgHeader());
			}

			var rate3 = Helper.NewClientRate(Helper.NewOrgHeader());
			Factory.Save();

			var testCollection = new RatingHeaderCollection(Factory);
			testCollection.Load();
			AssertEquals(1, testCollection.Count);

			testCollection = new RatingHeaderCollection(Factory, company2);
			testCollection.Load();
			AssertEquals(2, testCollection.Count);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new RatingHeaderCollection(Factory);
		}
	}
}
