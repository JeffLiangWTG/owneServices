using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework.TestHelper;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class OrgQueryClaimDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected Type GetExpectedBusinessObjectType() =>
			TestedTypeHelper.GetTestedType(GetType());

		public void TestIndexer()
		{
			BusinessObject obj1 = OrgCollection.AddNew();
			AssertEquals("Index 0", obj1, OrgCollection[0]);

			BusinessObject obj2 = OrgCollection.AddNew();
			AssertEquals("Index 1", obj2, OrgCollection[1]);
		}

		public void TestIfOnAddNewItIsReadOnly()
		{
			var claim = OrgCollection.AddNew();
			Assert("Debtor for New Object is Read Only", claim.AY_OH_DebtorInfo.ReadOnly);
		}

		public void TestIfOnAddItIsReadOnly()
		{
			AccQueryClaim claim = (AccQueryClaim)(Org.Factory).New<Enterprise.Integration.Accounting.IARAccQueryClaim>();
			OrgCollection.Add(claim);
			Assert("Debtor for Object added is Read Only", claim.AY_OH_DebtorInfo.ReadOnly);
		}

		#region Implementation

		OrgHeader fOrg;
		protected OrgHeader Org
		{
			get
			{
				if (fOrg == null)
				{
					fOrg = Factory.LoadTop1<OrgHeader>(new ZQuery());
				}
				return fOrg;
			}
		}

		OrgQueryClaimDependentCollection fOrgCollection;
		protected OrgQueryClaimDependentCollection OrgCollection
		{
			get { return fOrgCollection ?? (fOrgCollection = (OrgQueryClaimDependentCollection)GetCollectionToTest()); }
		}

		#endregion
	}
}
