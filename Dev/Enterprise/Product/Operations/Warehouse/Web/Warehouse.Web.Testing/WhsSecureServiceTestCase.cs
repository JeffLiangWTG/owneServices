using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	abstract class WhsSecureServiceTestCase : SecureServiceBaseTestCase<WhsSecureService>
	{
		#region IsFinalised Assertions

		public static void AssertIsFinalisedPrecondition(WhsDocket docket) => WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(docket);

		public static void AssertIsFinalisedPrecondition(WhsDocketLine docketLine) => WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(docketLine);

		public static void AssertIsFinalisedPrecondition(WhsPick pick) => WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(pick);

		public static void AssertIsFinalisedPrecondition(WhsVASOrder vasOrder) => WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(vasOrder);

		#endregion

		#region PopluateASNLines

		internal void PopluateASNLines(WhsReceive receive)
		{
			var expectAsnCount = receive.Lines.Count;
			// create ASN Lines for receive, and remove all inventory
			receive.PopulateASNLines();
			receive.Inventory.Cast<WhsInventoryView>().ForEach(inv => inv.WI_InDocketLineUnits = 0);
			receive.Factory.Save();

			AssertEquals("Precondition: ASN Line count should equal to or less that receive line count. (Note: Identical ASN Lines, except Quantity, can be merged)", true, expectAsnCount >= receive.AsnLines.Count);
		}

		#endregion

		#region CreateWhsDocketLineInfo

		protected static WhsLightDocketLineInfo CreateWhsDocketLineInfo(Guid receivePK, string productCode, decimal packs, string packUQ, string attr1, string attr2, string attr3, string serialNumber, ZDate expiryDate, ZDate packingDate,
			string heldCode, string palletID, string locationString)
		{
			return CreateWhsDocketLineInfo(receivePK, productCode, packs, packUQ, attr1, attr2, attr3, serialNumber, expiryDate, packingDate, heldCode, palletID, locationString, Guid.Empty);
		}

		protected static WhsLightDocketLineInfo CreateWhsDocketLineInfo(Guid receivePK, string productCode, decimal packs, string packUQ, string attr1, string attr2, string attr3, string serialNumber, ZDate expiryDate, ZDate packingDate,
			string heldCode, string palletID, string locationString, Guid dockDoorLocationPK)
		{
			var docketLineInfo = new WhsLightDocketLineInfo();
			docketLineInfo.DocketPK = receivePK;
			docketLineInfo.ProductCode = productCode;
			docketLineInfo.Location = locationString;
			docketLineInfo.PalletID = palletID;
			docketLineInfo.DockDoorLocationPK = dockDoorLocationPK;
			docketLineInfo.Packs = packs;
			docketLineInfo.PackUQ = packUQ;
			docketLineInfo.ExpiryDate = expiryDate.IsValid ? expiryDate.ToDateTime() : DateTime.MinValue;
			docketLineInfo.PackingDate = packingDate.IsValid ? packingDate.ToDateTime() : DateTime.MinValue;
			docketLineInfo.InventoryHeldCode = heldCode;
			docketLineInfo.Attribute1 = attr1;
			docketLineInfo.Attribute2 = attr2;
			docketLineInfo.Attribute3 = attr3;
			docketLineInfo.SerialNumber = serialNumber;

			return docketLineInfo;
		}

		#endregion

		#region AssertUnloadWhsReceiveLine

		protected static void AssertUnloadWhsReceiveLine(BusinessObjectFactory factory, WhsTestHelperFunctions helper, Guid inventoryLinePK, ZString expectedProduct, ZDecimal expectedPacks, ZString expectedPacksUQ,
			ZString expectedAttribute1, ZString expectedAttribute2, ZString expectedAttribute3, ZString expectedSerial,
			ZDate expectedExpiryDate, ZDate expectedPackingDate, ZString expectedHeldCode,
			ZString expectedPalletID, ZString expectedLocation, ZGuid expectedReceivePK, ZInt expectedInventoryCount,
			ZDecimal expecteUnits, ZString expectedUnitsUQ, WhsInventoryView expectedLine)
		{
			var line = factory.Load<WhsInventoryView>(new ZGuid(inventoryLinePK));
			AssertNotNull(line);
			AssertEquals(expectedReceivePK, line.InDocketLine.Docket.PK);

			// we can't check inventory count by accessing receive.Inventory collection as it may not be updated during web service call (for perfomance reasons)
			AssertEquals(expectedInventoryCount, factory.GetDatabaseCount(typeof(WhsInventoryView), new ZQuery(WhsInventoryViewSchema.WI_WD, expectedReceivePK)));
			AssertNotNull(line.SupplierPart);
			AssertEquals(expectedProduct, line.SupplierPart.OP_PartNum);
			AssertEquals(expectedPacks, line.InDocketLine.WE_PackQuantity);
			AssertEquals(expectedPacksUQ, line.WI_F3_NKPackType);
			AssertEquals(expecteUnits, line.WI_InDocketLineUnits);
			AssertEquals(0m, line.WI_ExpectedReceiptQuantity); // WI_ExpectedReceiptQuantity not populated after ASNs are created
			AssertEquals(expectedUnitsUQ, line.WI_UnitsUQ);
			AssertEquals(expectedAttribute1, line.WI_PartAttrib1);
			AssertEquals(expectedAttribute2, line.WI_PartAttrib2);
			AssertEquals(expectedAttribute3, line.WI_PartAttrib3);
			AssertEquals(expectedSerial, line.WI_SerialNumber);
			AssertEquals(expectedExpiryDate, line.WI_ExpiryDate);
			AssertEquals(expectedPackingDate, line.WI_PackingDate);
			AssertEquals(expectedHeldCode, line.OriginalInventoryHeldCode);
			AssertEquals(expectedPalletID, line.WI_PalletID);
			AssertEquals(expectedLocation, line.LocationString);
			AssertEquals(1, helper.FindLogs(line.InDocketLine.Logs, Events.AddedARecordToTheSystem, "RF").Length);
			if (expectedLine != null)
			{
				AssertEquals(expectedLine, line);
			}
		}

		#endregion

		#region Notify

		protected TestNotificationBuffer Notify => notify ?? (notify = new TestNotificationBuffer());

		TestNotificationBuffer notify;

		#endregion

		#region Implementation

		protected void SetupSecurityHeader(WhsSecureService service, WhsWarehouse warehouse, GlbStaff staff = null, string staffPassword = null)
		{
			if (service != null)
			{
				if (staff != null)
				{
					service.SecurityHeader.UserName = staff.GS_LoginName;

					var password = ZArchitecture.Environment.User.MasterPassword;
					if (!string.IsNullOrEmpty(staffPassword))
					{
						password = staffPassword;
					}
					else if (!staff.StaffPlainTextPassword.IsEmpty)
					{
						password = staff.StaffPlainTextPassword.ToString();
					}
					service.SecurityHeader.Password = GetEncryptedText(password);
				}

				if (warehouse != null)
				{
					service.SecurityHeader.WarehouseCode = warehouse.WW_WarehouseCode;
				}

				service.AllowedToRunServiceHasBeenCalled = false;
			}
		}

		#region Helper

		protected WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(new BusinessObjectFactory()));

		WhsTestHelperFunctions helper;

		#endregion

		#region GetNewWebService

		protected WhsSecureService GetNewWebService(WhsWarehouse warehouse, GlbStaff staff = null, string staffPassword = null)
		{
			var webService = base.GetNewWebService();
			string password = null;
			if (string.IsNullOrEmpty(staffPassword) && staff != null && !staff.StaffPlainTextPassword.IsEmpty)
			{
				password = staff.StaffPlainTextPassword.ToString();
			}
			SetupSecurityHeader(webService, warehouse, staff, password);
			return webService;
		}

		#endregion

		#region Implementation

		[NonSerializedClass]
		public class DummyDataException : ZDataException
		{
			public DummyDataException(DataRow row, DbConnection connection)
				: base(new Exception("Blah"), "Blah", "", row, connection)
			{
			}
		}

		#endregion

		#region AssertNoResponseError

		protected void AssertNoResponseError(WebServiceResponse response)
		{
			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response.Error);
				AssertEquals(string.Empty, response.ErrorMessage ?? string.Empty);
			});
		}

		#endregion

		#endregion
	}
}
