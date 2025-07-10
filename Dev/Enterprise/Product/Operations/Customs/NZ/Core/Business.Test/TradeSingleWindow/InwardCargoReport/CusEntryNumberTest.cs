using CargoWise.EntityFramework;
using Enterprise.Customs.Common.NZ;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow.Testing
{
	using Enterprise.ZArchitecture.Business.Testing;
	using NUnit.Framework;

	[TestedType(typeof(CusEntryNumber))]
	public class CusEntryNumberTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var cusEntryNum = (CusEntryNumber)base.GetNewBusinessObjectForDeleteTest(factory);
			cusEntryNum.CE_ParentTable = "CusEntryHeader";

			return cusEntryNum;
		}

		public void TestGetConsolID()
		{
			consol.JK_UniqueConsignRef = "CZ000001";
			AssertEquals("Consol No", consol.JK_UniqueConsignRef, entryNumber.CE_ConsolID);
		}

		public void TestGetMasterBillNumber()
		{
			consol.JK_MasterBillNum = "0819090934";
			AssertEquals("Consol Master Bill Num", consol.JK_MasterBillNum, entryNumber.CE_MasterBillNumber);
		}

		public void TestGetEntryNoFromConsol()
		{
			consol.JK_UniqueConsignRef = "CZ000001";
			entryNumber.CE_EntryType = CusEntryNumberTypeList.Codes.ICRNumber;
			entryNumber.CE_EntryNum = "97523021";

			var cusEntryNumber = consol.Factory.LoadTop1<CusEntryNumber>(CusEntryNumber.GetEntryNumberFilter(consol, CusEntryNumberTypeList.Codes.ICRNumber));
			AssertEquals("Entry Number should be found from Consol", entryNumber.CE_EntryNum, cusEntryNumber.CE_EntryNum);
		}

		public void TestGetEntryNoForGuid()
		{
			consol.JK_UniqueConsignRef = "CZ000001";
			entryNumber.CE_EntryType = CusEntryNumberTypeList.Codes.ICRNumber;
			entryNumber.CE_EntryNum = "97523021";
			entryNumber.CE_ParentTable = "CusEntryHeader";
			Factory.Save();

			var query = CusEntryNumber.GetEntryNumberFilter(consol.PK, CusEntryNumberTypeList.Codes.ICRNumber);
			var loadedNumber = new BusinessObjectFactory().LoadTop1<CusEntryNumber>(query);
			AssertEquals("Should have loaded the correct CusEntryNumber.", entryNumber.PK, loadedNumber.PK);
		}

		#region Implementation

		ForwardingConsol consol;
		CusEntryNumber entryNumber;

		protected override void SetUp()
		{
			base.SetUp();
			consol = Factory.New<ForwardingConsol>();
			entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_ParentID = consol.PK;
			entryNumber.CE_EntryType = CusEntryNumberTypeList.Codes.ICRNumber;
		}

		#endregion
	}
}
