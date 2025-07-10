using System;
using System.Data;
using System.Linq;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class PickLineUpdaterTransactionTest : TestCase
	{
		#region TestConfirmPickLinePickedQty_WhenExceptionThrownDuringFactorySave

		[UseSnapshotProtection]
		public void TestConfirmPickLinePickedQty_WhenExceptionThrownDuringFactorySave()
		{
			var factory = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory);

			var data = new TestDataSimpleEnvironment(factory);
			var staff = factory.NewWithValidTestData<GlbStaff>();

			helper.SetClientAttributeType(data.Org1, AttributeNumber.One, "SER", "Serial No.");
			helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, true);
			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 51m);

			factory.Save();

			var order1 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 51m);
			var pick = helper.CreatePickNew(order1);

			factory.Save();

			var pickLines = pick.GetAllPickLines().ToArray();
			var transactionLinePks = pickLines.Select(pickLine => pickLine.WZ_WE_TransactionLine).ToArray();
			var persistedPickLines = factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_TransactionLine, transactionLinePks));

			var pickLinePks = pickLines.Select(pickLine => pickLine.PK).ToArray();
			AssertEquals("Precondition: Expecting 1 pick lines", 1, persistedPickLines.Length);

			var releaseCapturedInfo = Enumerable
				.Range(1, 51)
				.Select(serial => new WhsReleaseCapturedInfo { Attribute1 = serial.ToString(), Quantity = 1 })
				.ToArray();

			factory.Saving += bof =>
			{
				throw new Exception("Mocking saving failure");
			};

			// Act
			var pickLinePKs = pickLines.Select(p => p.PK.ToGuid()).ToArray();
			AssertExceptionThrown<Exception>(() =>
			{
				PickLineUpdater.ConfirmPickLinesPickedQty(
					pickLines,
					new PickingInfo(51m, false),
					staff,
					new[] { new PickLinesToPickedPackTypeInfo(pickLinePKs, new[] { new PickedPackTypeInfo("BOX", 3m, "", releaseCapturedInfo) }) },
					null,
					true);
				staff.Factory.Save();
			});
			// Assert

			var assertionFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			var resultPickLines = assertionFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_TransactionLine, transactionLinePks));

			AssertEquals("Expecting 1 pick lines", 1, resultPickLines.Length);
			AssertEquals("Expecting all pick lines to have empty 'Assigned to Time' value", true, resultPickLines.All(pickLine => pickLine.WZ_PickedDateTime.IsEmpty));
			AssertEquals("Expecting all pick lines to have empty 'Assigned to' value", true, resultPickLines.All(pickLine => pickLine.WZ_GS_NKAssignedTo.IsEmpty));
		}

		#endregion
	}
}
