using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgCollectionCallCollection))]
	sealed class OrgCollectionCallCollectionTest : BusinessObjectCollectionTestCase
	{
		[ExpectException(typeof(NotSupportedException))]
		public override void TestAddNew()
		{
			Collection.AddNew();
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new OrgCollectionCallCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			OrgCollectionCall result = null;

			numberOfObjectsGot++;
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			OrgHeader header = newFactory.NewWithValidTestData<OrgHeader>();
			header.OH_Code = "OrgHeader" + numberOfObjectsGot;
			header.CompanyData.OB_IsDebtor = true;
			ZQuery query = new ZQuery(vw_OrgCollectionCallSchema.CC_OH, header.PK);
			query.AddToFilter(vw_OrgCollectionCallSchema.CC_GC, GlbCompany.CurrentCompany.PK);
			newFactory.Save();
			result = Factory.Load<OrgCollectionCall>(query)[0];

			return result;
		}

		int numberOfObjectsGot;
	}
}
