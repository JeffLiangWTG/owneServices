using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class MergeOrgElementsCollectionTest<T, U> : NonPersistentBusinessObjectCollectionTestCase<T> where T : MergeOrgElementCollection<U> where U : NonPersistentBusinessObject, IMergeOrgElement
	{
		public override void TestAddNew()
		{
			Assert("The collection should not be added to", true);
		}

		public void TestSetInnerMerges()
		{
			BusinessObjectCollection col = GetOldObjectCollectionFromMergeOrgHeader(SetupMergeOrgHeaderWithCollection());
			int count = 0;
			List<IMergeOrgElement> list = new List<IMergeOrgElement>();
			foreach (IMergeOrgElement obj in col)
			{
				if (obj.Action == MergeOrgAddress.ActionMerge)
				{
					count++;
					list.Add(obj);
				}
			}
			AssertSetInnerMerges(count, list);
		}

		public void TestSetParentCollection_OnDeletedMergeOrgElement()
		{
			BusinessObjectCollection col = GetOldObjectCollectionFromMergeOrgHeader(SetupMergeOrgHeaderWithCollection());

			var address = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.OA_Address1, "dup address"));
			address.Delete();

			((IParentCollectionSetter)col).SetParentCollection();
			AssertEquals("Accessing deleted element must return 0 error", 0, ErrorReporter.TotalErrorCount);
			AssertEquals("Accessing deleted element must not return any error", "", ErrorReporter.LastMessageReported);

			int count = 0;
			List<IMergeOrgElement> list = new List<IMergeOrgElement>();
			foreach (IMergeOrgElement obj in col)
			{
				if (obj.Action == MergeOrgAddress.ActionMerge)
				{
					count++;
					list.Add(obj);
				}
			}
			AssertSetInnerMerges(count, list);
			ErrorReporter.Clear();
		}

		protected abstract BusinessObjectCollection GetOldObjectCollectionFromMergeOrgHeader(MergeOrgHeader merge);

		protected abstract void AssertSetInnerMerges(int count, List<IMergeOrgElement> list);

		protected virtual MergeOrgHeader SetupMergeOrgHeaderWithCollection()
		{
			OrgHeader newOrg = Factory.NewWithValidTestData<OrgHeader>();
			newOrg.MainAddress.OA_Address1 = "old main";
			newOrg.OH_FullName = "~test~find~me";

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_Address1 = "address1";
			org.MainAddress.OA_Address2 = "address1 line 2";
			org.MainAddress.OA_Code = "address1 code";
			OrgAddress adr2 = org.Addresses.AddNew();
			adr2.OA_Address1 = "second address";
			adr2.OA_Address2 = "second address line 2";
			OrgAddress adr3 = org.Addresses.AddNew();
			adr3.OA_Address1 = "dup address";
			adr3.OA_Address2 = "dup address line 2";
			adr3.OA_Code = "dup address code";
			org.OH_FullName = "~test~find~me";
			OrgContact cnt1 = org.Contacts.AddNew();
			cnt1.OC_Email = "1@1.1";
			cnt1.OC_ContactName = "robin good";
			OrgContact cnt2 = org.Contacts.AddNew();
			cnt2.OC_Email = "2@2.2";
			cnt2.OC_ContactName = "pupsik";
			OrgContact cnt3 = org.Contacts.AddNew();
			cnt3.OC_Email = "3@3.3";
			cnt3.OC_ContactName = "obama";

			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.MainAddress.OA_Address1 = "address1";
			org2.MainAddress.OA_Address2 = "address1 line 2";
			org2.MainAddress.OA_Code = "address1 diff code";
			OrgAddress adr22 = org2.Addresses.AddNew();
			adr22.OA_Address1 = "third address";
			adr22.OA_Address2 = "third address line 2";
			OrgAddress adr33 = org2.Addresses.AddNew();
			adr33.OA_Address1 = "dup address";
			adr33.OA_Address2 = "dup address line 2";
			adr33.OA_Code = "dup address code";
			org2.OH_FullName = "~test~find~me";
			OrgContact cnt4 = org2.Contacts.AddNew();
			cnt4.OC_Email = "1x@1x.1x";
			cnt4.OC_ContactName = "robin bad";
			OrgContact cnt5 = org2.Contacts.AddNew();
			cnt5.OC_Email = "2@2.2";
			cnt5.OC_ContactName = "alien";
			OrgContact cnt6 = org2.Contacts.AddNew();
			cnt6.OC_Email = "q@q.q";
			cnt6.OC_ContactName = "obama";

			Factory.Save();

			MergeOrgHeader merge = new MergeOrgHeader(Factory, null);
			merge.HasToLoadSimilarOrgs = true;
			merge.NewOrganisationPk = newOrg.PK;

			return merge;
		}

		#region Implementation

		protected OrgHeader Organisation
		{
			get { return organisation ?? (organisation = Factory.New<OrgHeader>()); }
		}
		OrgHeader organisation;

		#endregion
	}
}
