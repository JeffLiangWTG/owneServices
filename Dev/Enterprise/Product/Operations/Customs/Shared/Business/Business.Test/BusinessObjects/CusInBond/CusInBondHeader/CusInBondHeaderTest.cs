using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusInBondHeaderTest : TestCaseWithFactory
	{
		[TestDate(2015, 7, 1)]
		public void TestEarliesMovementHeaderIsLoaded()
		{
			var amsHeader = (CusInBondHeader)Factory.New<Integration.Customs.US.USAMS.ICusInBondHeader>();
			amsHeader.BH_CarrierSCAC = "A";
			var amsMoveHeader1 = amsHeader.MovementHeader;
			amsMoveHeader1.BM_CustomsStatus = "A";
			amsMoveHeader1.BM_SystemCreateTimeUtc = new ZDateTime(2015, 7, 2);
			var amsMoveHeader2 = amsHeader.MovementHeaders.AddNew();
			amsMoveHeader2.BM_CustomsStatus = "A";
			amsMoveHeader2.BM_SystemCreateTimeUtc = new ZDateTime(2015, 7, 1);
			var inbondHeader = (CusInBondHeader)Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			inbondHeader.BH_CarrierSCAC = "A";
			var inbondMoveHeader1 = inbondHeader.MovementHeader;
			inbondMoveHeader1.BM_CustomsStatus = "A";
			inbondMoveHeader1.BM_SystemCreateTimeUtc = new ZDateTime(2015, 7, 2);
			var inbondMoveHeader2 = inbondHeader.MovementHeaders.AddNew();
			inbondMoveHeader2.BM_CustomsStatus = "A";
			inbondMoveHeader2.BM_SystemCreateTimeUtc = new ZDateTime(2015, 7, 1);
			Factory.Save();

			for (int i = 0; i < 3; i++)
			{
				var newFactory = new BusinessObjectFactory();
				amsHeader = newFactory.Load<CusInBondHeader>(amsHeader.PK);
				AssertEquals("amsHeader.MovementHeaders.Count", 2, amsHeader.MovementHeaders.Count);
				AssertEquals("amsHeader.MovementHeader.PK", amsMoveHeader2.PK, amsHeader.MovementHeader.PK);
				AssertNotNull(amsHeader.MovementHeaders.FindByPK(amsMoveHeader1.PK));
				inbondHeader = newFactory.Load<CusInBondHeader>(inbondHeader.PK);
				AssertEquals("inbondHeader.MovementHeaders.Count", 2, inbondHeader.MovementHeaders.Count);
				AssertEquals("inbondHeader.MovementHeader.PK", inbondMoveHeader2.PK, inbondHeader.MovementHeader.PK);
				AssertNotNull(inbondHeader.MovementHeaders.FindByPK(inbondMoveHeader1.PK));
			}
		}

		[TestDate(2021, 08, 15)]
		public void TestSendCustomsMessageMutex()
		{
			var header = (CusInBondHeader)Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			header.BH_JobReference = "INB1234567";
			Factory.Save();
			header.LockSendCustomsMessageMutex();
			Assert("Mutex is locked", header.IsSendCustomsMessageMutexLocked);
			AssertContains("is in the process of sending messages for INB1234567, please try again later.", header.CannotSendCustomsMessageWhenMutexIsLocked());
			header.UnlockSendCustomsMessageMutex();
			Assert("Mutex is unlocked", !header.IsSendCustomsMessageMutexLocked);

			var newFactory = new BusinessObjectFactory();
			var loadedHeader = newFactory.Load<CusInBondHeader>(header.PK);
			Assert("Mutex is unlocked", !loadedHeader.IsSendCustomsMessageMutexLocked);
		}
	}
}
