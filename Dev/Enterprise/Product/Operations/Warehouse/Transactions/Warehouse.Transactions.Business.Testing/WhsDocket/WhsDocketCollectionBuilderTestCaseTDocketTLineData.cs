using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	abstract class WhsDocketCollectionBuilderTestCase<TDocket, TLineData> : WhsTestCaseWithFactory
			where TDocket : WhsDocket
			where TLineData : LineData, new()
	{
		#region Constructors

		public virtual void TestConstructor()
		{
			AssertEquals(Factory._Instance, Builder.Factory._Instance);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestConstructorThrowsArgumentExceptionIfFactoryNull()
		{
			GetNewDocketBuilder(null);
		}

		#endregion

		#region Add Line

		#region TestAddLine

		protected abstract string SetTestAddLineDocketSubType { get; }

		public void TestAddLine()
		{
			var whs1 = Helper.CreateWarehouse("1", "A");
			var whs2 = Helper.CreateWarehouse("2", "A");
			var org1 = Helper.CreateClient("1");
			var org2 = Helper.CreateClient("2");
			var prod1 = Helper.CreateProduct(org1, "P1");
			var prod2 = Helper.CreateProduct(org2, "P2");
			var expiry = ZDate.Today.AddDays(2);
			var packing = ZDate.Today.AddDays(-2);

			Builder.AddLine(whs1, org1, "A", "", prod1, 10m, "A", "C1", "CLI");
			Builder.AddLine(whs1, org2, "B", "", prod2, 20m, "A", "C2", "CLI");
			Builder.AddLine(whs2, org1, "C", "", prod1, -20m, "A", "C3", "CLI");
			Builder.AddLine(whs2, org1, "C", SetTestAddLineDocketSubType, prod1, 25m, "B", "C33", "CLI");
			Builder.AddLine(whs2, org1, "C2", "", prod1, 25m, "B", "C22", "CLI");
			Builder.AddLine(whs2, org2, "D", "", prod2, -10m, "B", "C4", "CLI", "E1", expiry, packing, "PA1", "PA2", "PA3", "");
			Builder.AddLine(whs2, org2, "D", SetTestAddLineDocketSubType, prod2, 1m, "B", "C4", "CLI", "E1-5", expiry, packing, "", "", "", "SN1");

			AssertEquals(5, Builder.Dockets.Count);

			foreach (var docketLine in Builder.Dockets.SelectMany(d => d.Lines).Where(l => l.WE_TransactionQuantity > 0))
			{
				AssertEquals(docketLine.Docket.WD_ArrivalDate, docketLine.WE_AdjustmentArrivalDate);
			}

			var docket = Builder.Dockets.FindDocket(whs1, org1);
			AssertEquals(false, docket.IsFinalised);
			AssertEquals("A", docket.WD_ExternalReference);
			AssertEquals(1, docket.Lines.Count);
			AssertDocketLine(docket.Lines[0], prod1, 10m, "A", "C1", "CLI", "", ZDate.Empty, ZDate.Empty, "", "", "", "");

			docket = Builder.Dockets.FindDocket(whs1, org2);
			AssertEquals(false, docket.IsFinalised);
			AssertEquals("B", docket.WD_ExternalReference);
			AssertEquals(1, docket.Lines.Count);
			AssertDocketLine(docket.Lines[0], prod2, 20m, "A", "C2", "CLI", "", ZDate.Empty, ZDate.Empty, "", "", "", "");

			docket = Builder.Dockets.FindDocket(whs2.PK, org1.PK, "C");
			AssertEquals(false, docket.IsFinalised);
			AssertEquals("C", docket.WD_ExternalReference);
			AssertEquals(2, docket.Lines.Count);
			AssertDocketLine(docket.Lines[0], prod1, -20m, "A", "C3", "CLI", "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			AssertDocketLine(docket.Lines[1], prod1, 25m, "B", "C33", "CLI", "", ZDate.Empty, ZDate.Empty, "", "", "", "");

			docket = Builder.Dockets.FindDocket(whs2.PK, org1.PK, "C2");
			AssertEquals(false, docket.IsFinalised);
			AssertEquals("C2", docket.WD_ExternalReference);
			AssertEquals(1, docket.Lines.Count);
			AssertDocketLine(docket.Lines[0], prod1, 25m, "B", "C22", "CLI", "", ZDate.Empty, ZDate.Empty, "", "", "", "");

			docket = Builder.Dockets.FindDocket(whs2, org2);
			AssertEquals(false, docket.IsFinalised);
			AssertEquals("D", docket.WD_ExternalReference);
			AssertEquals(2, docket.Lines.Count);
			AssertDocketLine(docket.Lines[0], prod2, -10m, "B", "C4", "CLI", "E1", expiry, packing, "PA1", "PA2", "PA3", "");
			AssertDocketLine(docket.Lines[1], prod2, 1m, "B", "C4", "CLI", "E1-5", expiry, packing, "", "", "", "SN1");
		}

		#endregion

		#region TestAddLine_UsingDocketLine

		public virtual void TestAddLine_UsingDocketLine()
		{
			var expiryDate = ZDate.Today.AddDays(2);
			var packingDate = ZDate.Today.AddDays(-2);
			var part = Factory.New<OrgSupplierPart>();

			var docket = GetNewDocket();
			docket.WD_WW_Whs = Helper.CreateWarehouse("1").PK;
			docket.WD_OH_Client = Helper.CreateClient().PK;
			docket.WD_ExternalReference = "REF";
			docket.WD_DocketSubType = "CUS";

			var line = docket.Lines.AddNew();
			line.WE_OP = part.PK;
			line.WE_TransactionQuantity = 1m;
			line.WE_BondedEntryKey = "BEK-1";
			line.WE_ExpiryDate = expiryDate;
			line.WE_PackingDate = packingDate;
			line.WE_LineComment = "COMMENT";
			line.WE_ReasonCode = "CLI";
			line.WE_PartAttrib1 = "PA1";
			line.WE_PartAttrib2 = "PA2";
			line.WE_PartAttrib3 = "PA3";
			line.WE_SerialNumber = "SN1";
			line.LocationString = "A2";

			var line2 = Builder.AddLine(line, "", "CUS");
			AssertDocketLine(line2, part, 1m, "A2", "COMMENT", "CLI", "BEK-1", expiryDate, packingDate, "PA1", "PA2", "PA3", "SN1");

			var docket2 = line2.Docket;
			AssertNotNull(docket2);
			AssertEquals(docket.Warehouse.PK, docket2.Warehouse.PK);
			AssertEquals(docket.Client.PK, docket2.Client.PK);
			AssertEquals("REF", docket.WD_ExternalReference);
		}

		#endregion

		#region TestAddLine_WithOptions

		public void TestAddLine_WithOptions()
		{
			var whs = Helper.CreateWarehouse("1", "A");
			var org = Helper.CreateClient("1");
			var prod = Helper.CreateProduct(org, "P1");

			var data = WhsDocketCollectionBuilder<TDocket, TLineData>.GetNewLineData();
			var options = WhsDocketCollectionBuilderLineOptions.MergeLine;

			data.WhsPK = whs.PK;
			data.OrgPK = org.PK;
			data.DocketSubType = CodeLists.OrderType.Codes.Order;
			data.PartPK = prod.PK;
			data.EntryKey = "E1";
			data.ExpiryDate = ZDate.Today;
			data.PackingDate = ZDate.Today.AddDays(-1);
			data.PartAttrib1 = "PA1";
			data.PartAttrib2 = "PA2";
			data.PartAttrib3 = "PA3";
			data.Quantity = 15;

			Builder.AddLine(data, options);
			Builder.AddLine(data, options);

			data.PartAttrib3 = "";
			Builder.AddLine(data, options);

			data.PartAttrib2 = "";
			data.Quantity = 0;
			Builder.AddLine(data, options); // this zero quantity line should be ignored

			data.PartAttrib1 = "";
			data.Quantity = 0m;
			options |= WhsDocketCollectionBuilderLineOptions.AddZeroQuantityLines;
			Builder.AddLine(data, options); // this zero quantity line should be ignored

			AssertEquals(1, Builder.Dockets.Count);
			var docket = Builder.Dockets.Single();
			AssertEquals(3, docket.Lines.Count);
			AssertEquals(30m, docket.Lines[0].WE_TransactionQuantity);
			AssertEquals(15m, docket.Lines[1].WE_TransactionQuantity);
			AssertEquals(0m, docket.Lines[2].WE_TransactionQuantity);
			AssertEquals("", docket.Lines[2].WE_PartAttrib1); // make sure we added the correct line
		}

		public void TestAddLine_WithOptions_MergeLineWithSerialNumber()
		{
			var whs = Helper.CreateWarehouse("1", "A");
			var org = Helper.CreateClient("1");
			var prod = Helper.CreateProduct(org, "P1");

			var data = WhsDocketCollectionBuilder<TDocket, TLineData>.GetNewLineData();
			var options = WhsDocketCollectionBuilderLineOptions.MergeLine;

			data.WhsPK = whs.PK;
			data.OrgPK = org.PK;
			data.DocketSubType = CodeLists.OrderType.Codes.Order;
			data.PartPK = prod.PK;
			data.SerialNumber = "SN1";
			data.Quantity = 1;

			Builder.AddLine(data, options);
			Builder.AddLine(data, options);

			AssertEquals(1, Builder.Dockets.Count);
			var docket = Builder.Dockets.Single();
			AssertEquals(1, docket.Lines.Count);
			var line = docket.Lines.Single();
			AssertEquals("SN1", line.WE_SerialNumber);
			AssertEquals(2m, line.WE_TransactionQuantity);
		}

		#endregion

		#endregion

		#region Finalise All Dockets

		public virtual void TestFinaliseAllDockets()
		{
			var whs1 = Helper.CreateWarehouse("1", "A", 2, 1);
			var whs2 = Helper.CreateWarehouse("2", "A", 2, 1);
			var org1 = Helper.CreateClient("1");
			var prod1 = Helper.CreateProduct(org1, "P1");
			Factory.Save();

			Builder.AddLine(whs1, org1, "A", "", prod1, 10m, "A-1", "C1", "CLI");
			Builder.AddLine(whs2, org1, "A", "", prod1, 20m, "A-1", "C3", "CLI");

			foreach (var docket in Builder.Dockets)
			{
				ProcessDocketBeforeTestFinaliseAllDockets(docket);
			}

			Factory.Save(); // need to save as finalise uses a 2nd factory
			Builder.FinaliseAllDockets();

			AssertEquals(2, Builder.Dockets.Count);
			AssertEquals(2, Builder.Dockets.Count(d => d.IsFinalised));
		}

		protected virtual void ProcessDocketBeforeTestFinaliseAllDockets(TDocket docket)
		{
			// to be overridden if concrete docket type needs processing before it is finalised (eg, putaway, picked).
			docket.NotificationManager.Push(Notify);
		}

		#endregion

		#region Properties

		public void TestDockets()
		{
			AssertEquals(0, Builder.Dockets.Count);
			Builder.AddLine(Factory.New<WhsWarehouse>(), Factory.NewWithValidTestData<OrgHeader>(), "A", "", Factory.New<OrgSupplierPart>(), 10m, "", "", "");
			AssertEquals(1, Builder.Dockets.Count);
		}

		public void TestFactory()
		{
			AssertEquals(Factory._Instance, Builder.Factory._Instance);
		}

		#endregion

		#region Implementation

		protected void AssertDocketLine(WhsDocketLine line, OrgSupplierPart part, ZDecimal units, ZString locationString2, ZString lineComment, ZString reasonCode, ZString entryKey, ZDate expiryDate, ZDate packingDate, ZString partAttrib1, ZString partAttrib2, ZString partAttrib3, ZString serialNumber)
		{
			AssertEquals("Product incorrect", part.PK, line.WE_OP);
			AssertEquals("Units incorrect", units, line.WE_TransactionQuantity);
			AssertEquals("Location2 incorrect", locationString2, line.LocationString);
			AssertEquals("LineComment incorrect", lineComment, line.WE_LineComment);
			AssertEquals("ReasonCode incorrect", reasonCode, line.WE_ReasonCode);
			AssertEquals("EntryKey incorrect", entryKey, line.WE_BondedEntryKey);
			AssertEquals("Expiry Date incorrect", expiryDate, line.WE_ExpiryDate);
			AssertEquals("Packing Date incorrect", packingDate, line.WE_PackingDate);
			AssertEquals("Part Attrib1 incorrect", partAttrib1, line.WE_PartAttrib1);
			AssertEquals("Part Attrib2 incorrect", partAttrib2, line.WE_PartAttrib2);
			AssertEquals("Part Attrib3 incorrect", partAttrib3, line.WE_PartAttrib3);
			AssertEquals("Serial Number incorrect", serialNumber, line.WE_SerialNumber);
		}

		protected WhsDocketCollectionBuilder<TDocket, TLineData> GetNewDocketBuilder()
		{
			return GetNewDocketBuilder(Factory);
		}

		protected abstract WhsDocketCollectionBuilder<TDocket, TLineData> GetNewDocketBuilder(BusinessObjectFactory factory);
		protected abstract TDocket GetNewDocket();

		protected WhsDocketCollectionBuilder<TDocket, TLineData> Builder
		{
			get { return builder ?? (builder = GetNewDocketBuilder()); }
			set { builder = value; }
		}

		WhsDocketCollectionBuilder<TDocket, TLineData> builder;

		#endregion
	}
}
