using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class BondedWarehouseOperationDeterminerTest : TestCaseWithFactory
	{
		public void TestCanCancelBondedWarehouseChangeOfOwnershipCheck()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var entryMock = Factory.NewMoq<CusEntryHeader>();
				entryMock.Setup(m => m.IsWaitingForResponse).Returns(true);
				var entry = entryMock.Object;
				var determiner = new EntryBondedWarehouseOperationDeterminer(entry);
				AssertEquals("CanCancelBondedWarehouseChangeOfOwnershipCheck", false, determiner.CanCancelBondedWarehouseChangeOfOwnershipCheck());
				AssertEquals("Cannot cancel Bonded Warehouse Change of Ownership while waiting for a response.", UnitTestUserNotification.Instance.LastMessage.Text);
				entryMock.Setup(m => m.IsWaitingForResponse).Returns(false);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertEquals("CanCancelBondedWarehouseChangeOfOwnershipCheck", true, determiner.CanCancelBondedWarehouseChangeOfOwnershipCheck());
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCanUpdateBondedWarehouseChangeOfOwnershipCheck()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var entryMock = Factory.NewMoq<CusEntryHeader>();
				entryMock.Setup(m => m.IsWaitingForResponse).Returns(true);
				var entry = entryMock.Object;
				var determiner = new EntryBondedWarehouseOperationDeterminer(entry);
				AssertEquals("CanUpdateBondedWarehouseChangeOfOwnershipCheck", false, determiner.CanUpdateBondedWarehouseChangeOfOwnershipCheck());
				AssertEquals("Cannot update Bonded Warehouse Change of Ownership while waiting for a response.", UnitTestUserNotification.Instance.LastMessage.Text);
				entryMock.Setup(m => m.IsWaitingForResponse).Returns(false);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertEquals("CanUpdateBondedWarehouseChangeOfOwnershipCheck", true, determiner.CanUpdateBondedWarehouseChangeOfOwnershipCheck());
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCanCancelBondedWarehouseOutwardCheck()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var entryMock = Factory.NewMoq<CusEntryHeader>();
				entryMock.Setup(m => m.IsWaitingForResponse).Returns(true);
				var entry = entryMock.Object;
				var determiner = new EntryBondedWarehouseOperationDeterminer(entry);
				AssertEquals("CanCancelBondedWarehouseOutwardCheck", false, determiner.CanCancelBondedWarehouseOutwardCheck());
				AssertEquals("Cannot cancel Inventory stock release while waiting for a response.", UnitTestUserNotification.Instance.LastMessage.Text);
				entryMock.Setup(m => m.IsWaitingForResponse).Returns(false);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertEquals("CanCancelBondedWarehouseOutwardCheck", true, determiner.CanCancelBondedWarehouseOutwardCheck());
			}

			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestCanCancelUpdateBondedWarehouseInwardCheck()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var entryMock = Factory.NewMoq<CusEntryHeader>();
				entryMock.Setup(m => m.IsWaitingForResponse).Returns(true);
				var entry = entryMock.Object;
				var determiner = new EntryBondedWarehouseOperationDeterminer(entry);
				AssertEquals("CanCancelBondedWarehouseOutwardCheck", false, determiner.CanCancelUpdateBondedWarehouseInwardCheck());
				AssertEquals("Cannot cancel Inventory stock levels update while waiting for a response.", UnitTestUserNotification.Instance.LastMessage.Text);
				entryMock.Setup(m => m.IsWaitingForResponse).Returns(false);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertEquals("CanCancelBondedWarehouseOutwardCheck", true, determiner.CanCancelUpdateBondedWarehouseInwardCheck());
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCanUpdateBondedWarehouseInwardCheck()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var entryMock = Factory.NewMoq<CusEntryHeader>();
				entryMock.Setup(m => m.IsWaitingForResponse).Returns(true);
				var entry = entryMock.Object;
				var determiner = new EntryBondedWarehouseOperationDeterminer(entry);
				AssertEquals("CanCancelBondedWarehouseOutwardCheck", false, determiner.CanUpdateBondedWarehouseInwardCheck());
				AssertEquals("Cannot update Inventory stock levels while waiting for a response.", UnitTestUserNotification.Instance.LastMessage.Text);
				entryMock.Setup(m => m.IsWaitingForResponse).Returns(false);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertEquals("CanCancelBondedWarehouseOutwardCheck", true, determiner.CanUpdateBondedWarehouseInwardCheck());
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCanUpdateBondedWarehouseOutwardCheck()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var entryMock = Factory.NewMoq<CusEntryHeader>();
				entryMock.Setup(m => m.IsWaitingForResponse).Returns(true);
				var entry = entryMock.Object;
				var determiner = new EntryBondedWarehouseOperationDeterminer(entry);
				AssertEquals("CanCancelBondedWarehouseOutwardCheck", false, determiner.CanUpdateBondedWarehouseOutwardCheck());
				AssertEquals("Cannot update Inventory stock release while waiting for a response.", UnitTestUserNotification.Instance.LastMessage.Text);
				entryMock.Setup(m => m.IsWaitingForResponse).Returns(false);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertEquals("CanCancelBondedWarehouseOutwardCheck", true, determiner.CanUpdateBondedWarehouseOutwardCheck());
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
