using Enterprise.Customs.Common.NZ;
using Enterprise.Customs.NZ.Business.TradeSingleWindow;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Module.Testing
{
	[TestedType(typeof(ICRFilterBusinessObject))]
	sealed class ICRFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestConsolIDQuery()
		{
			Factory.Save();
			((ModuleNumberFilter)filterBO["Consol ID"]).IsActive = true;
			((ModuleNumberFilter)filterBO["Consol ID"]).Property = consol1.JK_UniqueConsignRef;
			filterCollection.Load(filterBO.Filter);
			AssertEquals("One record should be retrieved", 1, filterCollection.Count);
		}

		public void TestWithPartialConsolIDQuery()
		{
			Factory.Save();
			((ModuleNumberFilter)filterBO["Consol ID"]).IsActive = true;
			((ModuleNumberFilter)filterBO["Consol ID"]).Property = consol1.JK_UniqueConsignRef.SubstringSafe(1, consol1.JK_UniqueConsignRef.Length - 1);
			filterCollection.Load(filterBO.Filter);
			AssertEquals("One record should be retrieved", 1, filterCollection.Count);
		}

		public void TestMasterBillNumQuery()
		{
			consol1.JK_MasterBillNum = "08155555555";
			consol2.JK_MasterBillNum = "08178787878";
			Factory.Save();
			((ModuleNumberFilter)filterBO["MAWB/Bill of lading"]).IsActive = true;
			((ModuleNumberFilter)filterBO["MAWB/Bill of lading"]).Property = consol1.JK_MasterBillNum;
			filterCollection.Load(filterBO.Filter);
			AssertEquals("One record should be retrieved", 1, filterCollection.Count);
			((ModuleNumberFilter)filterBO["MAWB/Bill of lading"]).Property = "0813674125";
			filterCollection.Load(filterBO.Filter);
			AssertEquals("No record should be retrieved", 0, filterCollection.Count);
		}

		ICRFilterBusinessObject filterBO;
		CusEntryNumberCollection filterCollection;
		ForwardingConsol consol1;
		ForwardingConsol consol2;
		protected override void SetUp()
		{
			base.SetUp();
			filterBO = new ICRFilterBusinessObject();
			filterCollection = new CusEntryNumberCollection(Factory);
			consol1 = Factory.New<ForwardingConsol>();
			CusEntryNumber entryNumberForConsol1 = Factory.New<CusEntryNumber>();
			entryNumberForConsol1.CE_ParentID = consol1.PK;
			entryNumberForConsol1.CE_ParentTable = consol1.TableName;
			entryNumberForConsol1.CE_EntryType = CusEntryNumberTypeList.Codes.ICRNumber;
			consol2 = Factory.New<ForwardingConsol>();
			CusEntryNumber entryNumberForConsol2 = Factory.New<CusEntryNumber>();
			entryNumberForConsol2.CE_ParentID = consol2.PK;
			entryNumberForConsol2.CE_ParentTable = consol2.TableName;
			entryNumberForConsol2.CE_EntryType = CusEntryNumberTypeList.Codes.ICRNumber;
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ICRFilterBusinessObject();
		}
	}
}
