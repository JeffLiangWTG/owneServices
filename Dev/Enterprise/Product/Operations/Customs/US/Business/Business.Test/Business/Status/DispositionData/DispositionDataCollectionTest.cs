using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DispositionDataCollection))]
	sealed class DispositionDataCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestGetLatestDisposition()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("BB", "BB");
			list.AddPair("AA", "AA");
			var declaration = Factory.New<JobDeclaration>();
			var bill = declaration.Bills.AddNew();
			var data1 = bill.DispositionCodes.AddNewIfNotExist("BB", ZDateTime.BrettsBirthday.AddDays(-1), BillDispositionSourceList.Codes.SO);
			var data2 = bill.DispositionCodes.AddNewIfNotExist("AA", ZDateTime.BrettsBirthday, BillDispositionSourceList.Codes.SO);
			var data3 = bill.DispositionCodes.AddNewIfNotExist("CC", ZDateTime.BrettsBirthday.AddDays(1), BillDispositionSourceList.Codes.SO);
			Assert(bill.DispositionCodes.GetLatestDispositions(BillDispositionSourceList.Codes.SO).Any(x => x.PK == data3.PK));

			var data4 = bill.DispositionCodes.AddNewIfNotExist("AA", ZDateTime.BrettsBirthday.AddDays(1), BillDispositionSourceList.Codes.SO);
			AssertEquals(2, bill.DispositionCodes.GetLatestDispositions().Count(x => x.PK == data3.PK || x.PK == data4.PK));
			AssertEquals(data3.PK, bill.DispositionCodes.GetLatestDisposition(null, (x) => x == "AA").PK);
		}

		public void TestHasCode()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			Bill bill = declaration.Bills.AddNew();
			DispositionData data = bill.DispositionCodes.AddNewIfNotExist("AA", ZDateTime.BrettsBirthday);
			data = bill.DispositionCodes.AddNewIfNotExist("BB", ZDateTime.BrettsBirthday);
			data = bill.DispositionCodes.AddNewIfNotExist("CC", ZDateTime.BrettsBirthday);
			data = bill.DispositionCodes.AddNewIfNotExist("DD", ZDateTime.BrettsBirthday);
			AssertEquals(true, bill.DispositionCodes.HasCode("AA"));
			AssertEquals(true, bill.DispositionCodes.HasCode("BB"));
			AssertEquals(true, bill.DispositionCodes.HasCode("CC"));
			AssertEquals(true, bill.DispositionCodes.HasCode("DD"));
			AssertEquals(false, bill.DispositionCodes.HasCode("EE"));
		}

		public void TestAddNewIfNotExist()
		{
			var dispositionDate = ZDateTime.Now;
			var declaration = Factory.New<JobDeclaration>();
			var bill = declaration.Bills.AddNew();
			bill.DispositionCodes.AddNewIfNotExist("AA", dispositionDate);
			AssertEquals("New element is added", 1, bill.DispositionCodes.Count);

			bill.DispositionCodes.AddNewIfNotExist("AA", dispositionDate);
			AssertEquals("No new element is added as values already exist", 1, bill.DispositionCodes.Count);

			bill.DispositionCodes.AddNewIfNotExist("AA", dispositionDate.AddHours(1));
			AssertEquals("New element is added as values do not exist", 2, bill.DispositionCodes.Count);

			bill.DispositionCodes.AddNewIfNotExist("BB", dispositionDate.AddHours(1), BillDispositionSourceList.Codes.SO);
			AssertEquals("New element is added as values do not exist", 3, bill.DispositionCodes.Count);
		}

		public void TestAddNewWithMoreParameters()
		{
			var declaration = Factory.New<JobDeclaration>();
			var bill = declaration.Bills.AddNew();
			var newOne = bill.DispositionCodes.AddNewIfNotExist("AA", ZDateTime.BrettsBirthday);
			AssertEquals("AA", newOne.US_Code);
			AssertEquals((ZShort)1, newOne.US_Order);
			AssertEquals(ZDateTime.BrettsBirthday, newOne.US_DispositionDate);

			var newOne2 = bill.DispositionCodes.AddNewIfNotExist("AA", ZDateTime.BrettsBirthday);
			AssertEquals("No new element is not added", 1, bill.DispositionCodes.Count);
			AssertEquals("NewOne == newOne2", newOne, newOne2);

			var newOne3 = bill.DispositionCodes.AddNewIfNotExist("BB", ZDateTime.BrettsBirthday, BillDispositionSourceList.Codes.IS);
			AssertEquals(BillDispositionSourceList.Codes.IS, newOne3.US_Source);
		}

		public void TestGetLatestDispositions()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("71", "71");
			list.AddPair("73", "73");
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			DispositionData data1 = declaration.DispositionCodes.AddNewIfNotExist("06", ZDateTime.Today.AddDays(-9));
			DispositionData data2 = declaration.DispositionCodes.AddNewIfNotExist("22", ZDateTime.Today.AddDays(-9));
			DispositionData data3 = declaration.DispositionCodes.AddNewIfNotExist("22", ZDateTime.Today.AddDays(-3));
			DispositionData data4 = declaration.DispositionCodes.AddNewIfNotExist("22", ZDateTime.Today.AddDays(-1));
			DispositionData data5 = declaration.DispositionCodes.AddNewIfNotExist("51", ZDateTime.Today.AddDays(-1));
			DispositionData data6 = declaration.DispositionCodes.AddNewIfNotExist("52", ZDateTime.Today.AddDays(-1));
			DispositionData data7 = declaration.DispositionCodes.AddNewIfNotExist("52", ZDateTime.Today);
			DispositionData data8 = declaration.DispositionCodes.AddNewIfNotExist("06", ZDateTime.Today);
			DispositionData data9 = declaration.DispositionCodes.AddNewIfNotExist("73", ZDateTime.Today.AddDays(1));
			DispositionData data10 = declaration.DispositionCodes.AddNewIfNotExist("22", ZDateTime.Today);
			DispositionData data11 = declaration.DispositionCodes.AddNewIfNotExist("22", ZDateTime.Today.AddDays(-5));
			DispositionData data12 = declaration.DispositionCodes.AddNewIfNotExist("72", ZDateTime.Today.AddDays(1));
			DispositionData data13 = declaration.DispositionCodes.AddNewIfNotExist("71", ZDateTime.Today.AddDays(1));
			DispositionData data14 = declaration.DispositionCodes.AddNewIfNotExist("52", ZDateTime.Today.AddDays(-3));

			IEnumerable<DispositionData> result = declaration.DispositionCodes.GetLatestDispositions();
			AssertEquals(3, result.Count());
			Assert(result.Contains(data9));
			Assert(result.Contains(data12));
			Assert(result.Contains(data13));

			result = declaration.DispositionCodes.GetLatestDispositions(list);
			AssertEquals(2, result.Count());
			Assert(result.Contains(data9));
			Assert(result.Contains(data13));
		}

		public void TestGetDispositionsFor1302Document()
		{
			var declaration = Factory.New<JobDeclaration>();
			var bill = declaration.Bills.AddNew();
			var disp1 = bill.DispositionCodes.AddNew();
			disp1.US_Code = "A1";
			disp1.US_DispositionDate = new ZDateTime(2014, 3, 26, 9, 34, 0);
			var disp2 = bill.DispositionCodes.AddNew();
			disp2.US_Code = "69";
			disp2.US_DispositionDate = new ZDateTime(2014, 3, 26, 9, 33, 0);
			var disp3 = bill.DispositionCodes.AddNew();
			disp3.US_Code = "A2";
			disp3.US_DispositionDate = new ZDateTime(2014, 3, 26, 17, 34, 0);

			ZString dispositionOutput = @"69 Bill on File 03/26/2014 9:33:00 AM
A1 FDA PN Advisory 03/26/2014 9:34:00 AM
A2 FDA PN Warning 03/26/2014 5:34:00 PM
";
			AssertEquals(dispositionOutput, bill.DispositionCodes.GetDispositionsFor1302Document(System.Environment.NewLine));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return Bill.DispositionCodes;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<DispositionData>();
		}

		Bill Bill
		{
			get
			{
				if (fBill == null)
				{
					JobDeclaration declaration = Factory.New<JobDeclaration>();
					fBill = declaration.Bills.AddNew();
				}
				return fBill;
			}
		}
		Bill fBill;
	}
}
