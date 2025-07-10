using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(MergeOrgAddressCollection))]
	sealed class MergeOrgAddressCollectionTest : MergeOrgElementsCollectionTest<MergeOrgAddressCollection, MergeOrgAddress>
	{
		#region Implementation

		protected override MergeOrgAddressCollection GetCollectionToTest()
		{
			return new MergeOrgAddressCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new MergeOrgAddress(Factory, Organisation.MainAddress, Organisation);
		}

		#endregion

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

			new MergeOrgAddressCollection(factory, org1OtherFactory, factory.LoadTop1<OrgHeader>(new ZQuery()), null);
		}

		protected override void AssertSetInnerMerges(int count, List<IMergeOrgElement> list)
		{
			AssertEquals(1, count);
			AssertEquals("dup address", (list[0] as MergeOrgAddress).OldAddressAddress1);
		}

		protected override BusinessObjectCollection GetOldObjectCollectionFromMergeOrgHeader(MergeOrgHeader merge)
		{
			return merge.OldOrgAddressCollectionForSimilarOrgsByName;
		}
	}
}
