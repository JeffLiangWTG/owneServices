using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CusBondDetailCollection))]
	sealed class CusBondDetailCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestDoesNotShowOtherRows()
		{
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var baseDetail1 = Factory.New<MasterFiles.Business.CusBondDetail>();
			var baseDetail2 = Factory.New<MasterFiles.Business.CusBondDetail>();
			var baseDetail3 = Factory.New<MasterFiles.Business.CusBondDetail>();
			baseDetail1.Parent = org;
			baseDetail2.Parent = org;
			baseDetail3.Parent = org;
			var usCollection = new CusBondDetailCollection(org);
			usCollection.Load();
			Assert(!usCollection.Contains(baseDetail1));
			Assert(!usCollection.Contains(baseDetail2));
			Assert(!usCollection.Contains(baseDetail3));
			baseDetail2.PW_ApplicationCode = ApplicationCodeList.Codes.EuNcts;
			baseDetail1.PW_ApplicationCode = ApplicationCodeList.Codes.UsaInBond;
			usCollection = new CusBondDetailCollection(org);
			usCollection.Load();
			Assert(usCollection.Contains(baseDetail1));
			Assert(!usCollection.Contains(baseDetail2));
			Assert(!usCollection.Contains(baseDetail3));
			baseDetail1.PW_ApplicationCode = ApplicationCodeList.Codes.EuNcts;
			baseDetail3.PW_ApplicationCode = ApplicationCodeList.Codes.INConsolManifest;
			usCollection = new CusBondDetailCollection(org);
			usCollection.Load();
			Assert(!usCollection.Contains(baseDetail1));
			Assert(!usCollection.Contains(baseDetail2));
			Assert(!usCollection.Contains(baseDetail3));
			var usDetail = usCollection.AddNew();
			AssertEquals(ApplicationCodeList.Codes.UsaInBond, usDetail.PW_ApplicationCode);
			Assert(usCollection.Contains(usDetail));
		}

		public void TestGetActiveBondDetailDataFor_ActivityCode()
		{
			CusBondDetailCollection coll = new CusBondDetailCollection(Organisation);
			CusBondDetail bondData = coll.AddNew();
			bondData.PW_ActivityCode = ActivityCodeList.Codes._1;
			bondData.PW_BondEffectiveDate = new ZDateTime(2007, 1, 1);
			bondData.PW_BondType = ImporterBondTypeList.Codes.ContinuousBond;
			CusBondDetail bondData2 = coll.AddNew();
			bondData2.PW_ActivityCode = ActivityCodeList.Codes._1;
			bondData2.PW_BondEffectiveDate = new ZDateTime(2007, 2, 1);
			bondData2.PW_BondType = ImporterBondTypeList.Codes.ContinuousBond;
			CusBondDetail bondData3 = coll.AddNew();
			bondData3.PW_ActivityCode = ActivityCodeList.Codes._2;
			bondData3.PW_BondEffectiveDate = new ZDateTime(2007, 3, 1);
			bondData2.PW_BondExpiryDate = ZDateTime.Today.AddDays(2);
			bondData3.PW_BondExpiryDate = ZDateTime.Today;
			bondData3.PW_BondType = ImporterBondTypeList.Codes.ContinuousBond;
			AssertEquals(bondData2, coll.GetActiveBondDetailDataFor(new List<ZString>(new ZString[] { ActivityCodeList.Codes._1 }), "", ZDateTime.Today));
			AssertEquals(bondData3, coll.GetActiveBondDetailDataFor(new List<ZString>(new ZString[] { ActivityCodeList.Codes._1, ActivityCodeList.Codes._2 }), "", ZDateTime.Today));
			AssertEquals(bondData3, coll.GetActiveBondDetailDataFor(new List<ZString>(new ZString[] { ActivityCodeList.Codes._2 }), "", ZDateTime.Today));
			bondData3.PW_BondExpiryDate = ZDateTime.Today.AddDays(2);
			AssertEquals(bondData3, coll.GetActiveBondDetailDataFor(new List<ZString>(new ZString[] { ActivityCodeList.Codes._2 }), "", ZDateTime.Today));
			bondData3.PW_BondExpiryDate = ZDateTime.Today;
			AssertEquals(bondData3, coll.GetActiveBondDetailDataFor(new List<ZString>(new ZString[] { ActivityCodeList.Codes._2 }), "", ZDateTime.Today));
			bondData2.PW_BondExpiryDate = ZDateTime.Empty;
			AssertEquals(bondData2, coll.GetActiveBondDetailDataFor(new List<ZString>(new ZString[] { ActivityCodeList.Codes._1 }), "", ZDateTime.Today));
		}

		public void TestGetActiveBondDetailDataFor_ActivityCode_BondType()
		{
			CusBondDetailCollection coll = new CusBondDetailCollection(Organisation);
			CusBondDetail bondData = coll.AddNew();
			bondData.PW_ActivityCode = ActivityCodeList.Codes._1;
			bondData.PW_BondType = ImporterBondTypeList.Codes.ContinuousBond;
			bondData.PW_BondEffectiveDate = new ZDateTime(2007, 1, 1);
			CusBondDetail bondData2 = coll.AddNew();
			bondData2.PW_ActivityCode = ActivityCodeList.Codes._1;
			bondData2.PW_BondType = ImporterBondTypeList.Codes.ContinuousBond;
			bondData2.PW_BondEffectiveDate = new ZDateTime(2007, 2, 1);
			CusBondDetail bondData3 = coll.AddNew();
			bondData3.PW_ActivityCode = ActivityCodeList.Codes._1;
			bondData3.PW_BondType = ImporterBondTypeList.Codes.SingleTransactionBond;
			bondData3.PW_BondEffectiveDate = ZDateTime.Empty;
			bondData2.PW_BondExpiryDate = ZDateTime.Today.AddDays(2);
			bondData3.PW_BondExpiryDate = ZDateTime.Today;
			AssertEquals(bondData2, coll.GetActiveBondDetailDataFor(new List<ZString>(new ZString[] { ActivityCodeList.Codes._1 }), BondTypeList.Codes.ContinuousBond, ZDateTime.Today));
			AssertEquals(bondData3, coll.GetActiveBondDetailDataFor(new List<ZString>(new ZString[] { ActivityCodeList.Codes._1 }), BondTypeList.Codes.SingleTransactionBond, ZDateTime.Today));
			CusBondDetail bondData4 = coll.AddNew();
			bondData4.PW_ActivityCode = ActivityCodeList.Codes._1;
			bondData4.PW_BondType = ImporterBondTypeList.Codes.SingleTransactionBond;
			bondData4.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-1);
			bondData4.PW_BondExpiryDate = ZDateTime.Today.AddDays(2);
			bondData4.PW_BondExpiryDate = ZDateTime.Today;
			CusBondDetail bondData5 = coll.AddNew();
			bondData5.PW_ActivityCode = ActivityCodeList.Codes._1;
			bondData5.PW_BondType = ImporterBondTypeList.Codes.SingleTransactionBond;
			bondData5.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-2);
			bondData5.PW_BondExpiryDate = ZDateTime.Today.AddDays(2);
			bondData5.PW_BondExpiryDate = ZDateTime.Today;
			AssertEquals(bondData4, coll.GetActiveBondDetailDataFor(new List<ZString>(new ZString[] { ActivityCodeList.Codes._1 }), BondTypeList.Codes.SingleTransactionBond, ZDateTime.Today));
		}

		public void TestGetBondDetailForAccountNo()
		{
			CusBondDetailCollection collection = new CusBondDetailCollection(Organisation);
			CusBondDetail bondData = collection.AddNew();
			bondData.PW_ActivityCode = ActivityCodeList.Codes._1;
			bondData.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-1);
			bondData.PW_BondNumber = "555";
			AssertNull(collection.GetBondDetailForAccountNo("1236454"));
			AssertEquals(bondData, collection.GetBondDetailForAccountNo("555"));
		}

		public void TestGetBondDetailForSuretyCode()
		{
			CusBondDetailCollection collection = new CusBondDetailCollection(Organisation);
			CusBondDetail bondData = collection.AddNew();
			bondData.PW_ActivityCode = ActivityCodeList.Codes._1;
			bondData.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-1);
			bondData.PW_BondExpiryDate = ZDateTime.Today.AddDays(2);
			bondData.PW_SuretyCode = "555";
			CusBondDetail bondData1 = collection.AddNew();
			bondData1.PW_ActivityCode = ActivityCodeList.Codes._1;
			bondData1.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-1);
			bondData1.PW_BondExpiryDate = ZDateTime.Today.AddDays(2);
			bondData1.PW_SuretyCode = "777";
			CusBondDetail bondData2 = collection.AddNew();
			bondData2.PW_ActivityCode = ActivityCodeList.Codes._1;
			bondData2.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-1);
			bondData2.PW_BondExpiryDate = ZDateTime.Today.AddDays(2);
			bondData2.PW_SuretyCode = "999";
			AssertEquals(bondData1, collection.GetBondDetailForSuretyCode("777"));
		}

		public void TestHasContinuousBond()
		{
			CusBondDetailCollection collection = new CusBondDetailCollection(Organisation);
			AssertEquals(false, collection.HasContinuousBond);
			CusBondDetail bondData = collection.AddNew();
			bondData.PW_BondType = ImporterBondTypeList.Codes.ContinuousBond;
			bondData.PW_ActivityCode = ActivityCodeList.Codes._1;
			bondData.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-1);
			bondData.PW_BondExpiryDate = ZDateTime.Today.AddDays(2);
			bondData.PW_SuretyCode = "555";
			AssertEquals(true, collection.HasContinuousBond);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new CusBondDetailCollection(Organisation);

		OrgHeader organisation;
		OrgHeader Organisation => organisation ?? (organisation = Factory.NewWithValidTestData<OrgHeader>());
	}
}
