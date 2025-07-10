using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(USMovementHeaderResetCollection))]
	sealed class USMovementHeaderResetCollectionTest : NonPersistentBusinessObjectCollectionTestCase<USMovementHeaderResetCollection>
	{
		public void TestUSMovementHeaderCollectionReset()
		{
			var moveHeader1 = Header.MovementHeaders.AddNew();
			moveHeader1.AllocateInBondNumber("1111");
			var sendingObj = new InBondMessageSendingObject(moveHeader1, InBondMessageType.DepartureDelete);
			sendingObj.Send();
			var moveHeader2 = Header.MovementHeaders.AddNew();
			moveHeader2.AllocateInBondNumber("2111");
			moveHeader2.BM_CustomsStatus = Enterprise.Customs.Common.US.ImportMessageStatusList.Codes.ClearDepartureOriginal;
			var moveHeader3 = Header.MovementHeaders.AddNew();
			moveHeader3.AllocateInBondNumber("3111");
			var moveHeader4 = Header.MovementHeaders.AddNew();
			var bill1 = Header.Bills.AddNew();
			bill1.B0_MessageStatus = Enterprise.Customs.Common.US.ImportMessageStatusList.Codes.AwaitingArrival;
			var moveDetail = Header.MovementDetails.AddNew();
			moveDetail.B9_B0 = bill1.PK;
			moveDetail.B9_BM = moveHeader1.PK;
			var container = moveDetail.Containers.AddNew();
			container.BC_MessageStatus = Enterprise.Customs.Common.US.ImportMessageStatusList.Codes.AwaitingArrival;
			AssertEquals(4, Coll.Count);
			Coll[0].RO_ResetToOriginal = true;
			Coll[1].RO_ResetToOriginal = true;
			Coll[2].RO_ResetToOriginal = false;
			Coll[3].RO_ResetToOriginal = true;
			Coll[0].RO_ResetReason = "reason1";
			Coll[1].RO_ResetReason = "reason1";
			Coll[3].RO_ResetReason = "reason1";
			Coll.ResetMoveHeadersCollectionToOriginal();
			AssertEquals("", Header.MovementHeaders[0].BM_CustomsStatus);
			AssertNotEquals("", Header.MovementHeaders[1].BM_CustomsStatus);
			AssertEquals("", bill1.B0_MessageStatus);
			AssertEquals("", container.BC_MessageStatus);
		}

		protected override USMovementHeaderResetCollection GetCollectionToTest() => Coll;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var movement = Header.MovementHeaders.AddNew();
			return new USMovementHeaderReset(movement, Coll);
		}

		CusInBondHeader header;
		CusInBondHeader Header => header ?? (header = Factory.New<CusInBondHeader>());

		USMovementHeaderResetCollection collection;
		USMovementHeaderResetCollection Coll => collection ?? (collection = new USMovementHeaderResetCollection(Header));
	}
}
