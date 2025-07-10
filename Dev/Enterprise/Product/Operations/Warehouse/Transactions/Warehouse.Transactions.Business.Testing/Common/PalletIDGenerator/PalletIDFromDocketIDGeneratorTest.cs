using System;
using System.Linq;
using CargoWise.Application;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class PalletIDFromDocketIDGeneratorTest : WhsTestCaseWithFactory
	{
		#region ObjectFactoryRegistration

		public void TestObjectFactoryRegistration()
		{
			AssertType<PalletIDFromDocketIDGenerator>(ObjectFactory.Get<IPalletIDFromDocketIDGenerator>());
		}

		#endregion

		#region GenerateIDs

		public void TestGenerateIDs_NullDocket_ShouldError()
		{
			var docketIDGenerator = new PalletIDFromDocketIDGenerator();

			AssertExceptionThrown<ArgumentNullException>(() => docketIDGenerator.GenerateIDs(null, 1));
		}

		public void TestGenerateIDs_NumberOfIDsLessThanOne_ShouldError_WhsReceive()
		{
			TestGenerateIDs_NumberOfIDsLessThanOne_ShouldErrorCore<WhsReceive>();
		}

		public void TestGenerateIDs_NumberOfIDsLessThanOne_ShouldError_WhsTransfer()
		{
			TestGenerateIDs_NumberOfIDsLessThanOne_ShouldErrorCore<WhsTransfer>();
		}

		void TestGenerateIDs_NumberOfIDsLessThanOne_ShouldErrorCore<T>() where T : WhsDocket
		{
			var docketIDGenerator = new PalletIDFromDocketIDGenerator();

			var docket = Factory.New<T>();

			AssertExceptionThrown<ArgumentOutOfRangeException>(() => docketIDGenerator.GenerateIDs(docket, 0));
		}

		public void TestGenerateIDs_StartIDLessThanOne_ShouldError_WhsReceive()
		{
			TestGenerateIDs_StartIDLessThanOne_ShouldErrorCore<WhsReceive>();
		}

		public void TestGenerateIDs_StartIDLessThanOne_ShouldError_WhsTransfer()
		{
			TestGenerateIDs_StartIDLessThanOne_ShouldErrorCore<WhsTransfer>();
		}

		void TestGenerateIDs_StartIDLessThanOne_ShouldErrorCore<T>() where T : WhsDocket
		{
			var docketIDGenerator = new PalletIDFromDocketIDGenerator();

			var docket = Factory.New<T>();

			AssertExceptionThrown<ArgumentOutOfRangeException>(() => docketIDGenerator.GenerateIDs(docket, 1, 0));
		}

		public void TestGenerateIDs_NoMoreIDs_WhsReceive()
		{
			TestGenerateIDs_NoMoreIDsCore<WhsReceive>();
		}

		public void TestGenerateIDs_NoMoreIDs_WhsTransfer()
		{
			TestGenerateIDs_NoMoreIDsCore<WhsTransfer>();
		}

		void TestGenerateIDs_NoMoreIDsCore<T>() where T : WhsDocket
		{
			var docketIDGenerator = new PalletIDFromDocketIDGenerator();
			var docket = Factory.New<T>();
			docket.WD_DocketID = DocketID;

			var ids = docketIDGenerator.GenerateIDs(docket, 10, 9999).ToArray();

			AssertEquals(1, ids.Length);
			AssertEquals(new GeneratedID(DocketID + Seperator + "9999", 9999), ids[0]);
		}

		public void TestGenerateIDs_FirstIDInDocket_WhsReceive()
		{
			TestGenerateIDs_FirstIDInDocketCore<WhsReceive>();
		}

		public void TestGenerateIDs_FirstIDInDocket_WhsTransfer()
		{
			TestGenerateIDs_FirstIDInDocketCore<WhsTransfer>();
		}

		void TestGenerateIDs_FirstIDInDocketCore<T>() where T : WhsDocket
		{
			var docketIDGenerator = new PalletIDFromDocketIDGenerator();
			var docket = Factory.New<T>();
			docket.WD_DocketID = DocketID;

			docket.Lines.AddNew();
			docket.Lines.AddNew();
			docket.Lines.AddNew();

			var ids = docketIDGenerator.GenerateIDs(docket, 3).ToArray();

			AssertEquals(3, ids.Length);
			AssertEquals(new GeneratedID(DocketID + Seperator + "0001", 1), ids[0]);
			AssertEquals(new GeneratedID(DocketID + Seperator + "0002", 2), ids[1]);
			AssertEquals(new GeneratedID(DocketID + Seperator + "0003", 3), ids[2]);
		}

		public void TestGenerateIDs_NoLinesInDocket_WhsReceive()
		{
			TestGenerateIDs_NoLinesInDocketCore<WhsReceive>();
		}

		public void TestGenerateIDs_NoLinesInDocket_WhsTransfer()
		{
			TestGenerateIDs_NoLinesInDocketCore<WhsTransfer>();
		}

		void TestGenerateIDs_NoLinesInDocketCore<T>() where T : WhsDocket
		{
			var docketIDGenerator = new PalletIDFromDocketIDGenerator();
			var docket = Factory.New<T>();
			docket.WD_DocketID = DocketID;

			AssertEquals(0, docket.Lines.Count);

			var ids = docketIDGenerator.GenerateIDs(docket, 3).ToArray();

			AssertEquals(3, ids.Length);
			AssertEquals(new GeneratedID(DocketID + Seperator + "0001", 1), ids[0]);
			AssertEquals(new GeneratedID(DocketID + Seperator + "0002", 2), ids[1]);
			AssertEquals(new GeneratedID(DocketID + Seperator + "0003", 3), ids[2]);
		}

		public void TestGenerateIDs_FindsGapsInIDsInDocket_WhsReceive()
		{
			TestGenerateIDs_FindsGapsInIDsInDocketCore<WhsReceive>();
		}

		public void TestGenerateIDs_FindsGapsInIDsInDocket_WhsTransfer()
		{
			TestGenerateIDs_FindsGapsInIDsInDocketCore<WhsTransfer>();
		}

		void TestGenerateIDs_FindsGapsInIDsInDocketCore<T>() where T : WhsDocket
		{
			var docketIDGenerator = new PalletIDFromDocketIDGenerator();
			var docket = Factory.New<T>();
			docket.WD_DocketID = DocketID;

			foreach (var i in new int[] { 1, 2, 4, 5, 6, 7, 8, 9, 13, 14 })
			{
				var line = docket.Lines.AddNew();
				line.WE_PalletID = DocketID + Seperator + i.ToString().PadLeft(4, '0');
			}

			var ids = docketIDGenerator.GenerateIDs(docket, 6).ToArray();

			AssertEquals(6, ids.Length);
			AssertEquals(new GeneratedID(DocketID + Seperator + "0003", 3), ids[0]);
			AssertEquals(new GeneratedID(DocketID + Seperator + "0010", 10), ids[1]);
			AssertEquals(new GeneratedID(DocketID + Seperator + "0011", 11), ids[2]);
			AssertEquals(new GeneratedID(DocketID + Seperator + "0012", 12), ids[3]);
			AssertEquals(new GeneratedID(DocketID + Seperator + "0015", 15), ids[4]);
			AssertEquals(new GeneratedID(DocketID + Seperator + "0016", 16), ids[5]);
		}

		public void TestGenerateIDs_IgnoresOtherIDs_WhsReceive()
		{
			TestGenerateIDs_IgnoresOtherIDsCore<WhsReceive>();
		}

		public void TestGenerateIDs_IgnoresOtherIDs_WhsTransfer()
		{
			TestGenerateIDs_IgnoresOtherIDsCore<WhsTransfer>();
		}

		void TestGenerateIDs_IgnoresOtherIDsCore<T>() where T : WhsDocket
		{
			var docketIDGenerator = new PalletIDFromDocketIDGenerator();
			var docket = Factory.New<T>();
			docket.WD_DocketID = DocketID;

			var otherLine = docket.Lines.AddNew();
			otherLine.WE_PalletID = string.Concat(Enumerable.Repeat("1", DocketID.Length + Seperator.Length)) + "0001";

			var ids = docketIDGenerator.GenerateIDs(docket, 1).ToArray();

			AssertEquals(1, ids.Length);
			AssertEquals(new GeneratedID(DocketID + Seperator + "0001", 1), ids[0]);
		}

		#endregion

		const string DocketID = "TestDocketID";
		const string Seperator = "-";
	}
}
