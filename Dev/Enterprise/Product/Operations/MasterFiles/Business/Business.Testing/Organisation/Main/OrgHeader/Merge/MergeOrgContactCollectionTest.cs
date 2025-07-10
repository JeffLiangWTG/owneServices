using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(MergeOrgContactCollection))]
	sealed class MergeOrgContactCollectionTest : MergeOrgElementsCollectionTest<MergeOrgContactCollection, MergeOrgContact>
	{
		[ExpectNoExceptions]
		public void TestDeletedFromFactory()
		{
			var factory = new BusinessObjectFactory();
			OrgHeader org1 = factory.New<OrgHeader>();
			ZGuid org1PK = org1.PK;
			org1.OH_Code = "~test~";
			org1.MainAddress.OA_Address1 = "~test~";
			factory.Save();
			var factory2 = new BusinessObjectFactory();

			OrgHeader org1OtherFactory = factory2.Load<OrgHeader>(org1PK);
			org1.Delete();

			new MergeOrgContactCollection(factory, org1OtherFactory, factory.LoadTop1<OrgHeader>(new ZQuery()), null);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "gimli";
			contact.OC_OH = Organisation.PK;
			return new MergeOrgContact(Factory, contact, Organisation);
		}

		protected override MergeOrgContactCollection GetCollectionToTest()
		{
			return new MergeOrgContactCollection(Factory);
		}

		protected override void AssertSetInnerMerges(int count, List<IMergeOrgElement> list)
		{
			AssertEquals(2, count);
			if ((list[0] as MergeOrgContact).OldContactName == "alien")
			{
				AssertEquals("alien", (list[0] as MergeOrgContact).OldContactName);
				AssertEquals("2@2.2", (list[0] as MergeOrgContact).OldContactEmail);

				AssertEquals("obama", (list[1] as MergeOrgContact).OldContactName);
				AssertEquals("q@q.q", (list[1] as MergeOrgContact).OldContactEmail);
			}
			else if ((list[1] as MergeOrgContact).OldContactName == "alien")
			{
				AssertEquals("alien", (list[1] as MergeOrgContact).OldContactName);
				AssertEquals("2@2.2", (list[1] as MergeOrgContact).OldContactEmail);

				AssertEquals("obama", (list[0] as MergeOrgContact).OldContactName);
				AssertEquals("q@q.q", (list[0] as MergeOrgContact).OldContactEmail);
			}
			else if ((list[0] as MergeOrgContact).OldContactName == "pupsik")
			{
				AssertEquals("pupsik", (list[0] as MergeOrgContact).OldContactName);
				AssertEquals("2@2.2", (list[0] as MergeOrgContact).OldContactEmail);

				AssertEquals("obama", (list[1] as MergeOrgContact).OldContactName);
				AssertEquals("3@3.3", (list[1] as MergeOrgContact).OldContactEmail);
			}
			else
			{
				AssertEquals("pupsik", (list[1] as MergeOrgContact).OldContactName);
				AssertEquals("2@2.2", (list[1] as MergeOrgContact).OldContactEmail);

				AssertEquals("obama", (list[0] as MergeOrgContact).OldContactName);
				AssertEquals("3@3.3", (list[0] as MergeOrgContact).OldContactEmail);
			}
		}

		protected override BusinessObjectCollection GetOldObjectCollectionFromMergeOrgHeader(MergeOrgHeader merge)
		{
			return merge.OldOrgContactCollectionForSimilarOrgsByName;
		}
	}
}
