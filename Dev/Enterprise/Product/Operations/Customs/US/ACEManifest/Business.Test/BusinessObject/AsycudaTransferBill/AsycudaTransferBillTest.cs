using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.AIM.Messaging;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	[TestedType(typeof(AsycudaTransferBill))]
	class AsycudaTransferBillTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_MasterBill = "MAN002";
			var arrivalHeader = manifestHeader.ArrivalHeaders.AddNew();
			var transferHeader = arrivalHeader.TransferHeaders.AddNew();
			transferBill = transferHeader.TransferBills.AddNew();
			AssertEquals("STD", transferBill.ATB_BillOfLadingType);
		}

		public void TestATB_BillNumber()
		{
			var houseBill = manifestHeader.Bills.AddNew();
			houseBill.ABL_BillNumber = "HB001";
			AssertEquals("STD", houseBill.ABL_BolType);
			AssertEquals("", transferBill.ATB_BillNumber);
			AssertEquals("STD", transferBill.ATB_BillOfLadingType);
			transferBill.ATB_BillNumber = "HB001";
			AssertEquals("ATB_BillOfLadingType", "STD", transferBill.ATB_BillOfLadingType);
			AssertEquals("ATB_ABL_Bill", houseBill.PK, transferBill.ATB_ABL_Bill);
			transferBill.ATB_BillNumber = "HB999";
			AssertEquals("ATB_BillOfLadingType", "STD", transferBill.ATB_BillOfLadingType);
			AssertEquals("ATB_ABL_Bill", ZGuid.Empty, transferBill.ATB_ABL_Bill);
			transferBill.ATB_BillNumber = "MAN001";
			AssertEquals("ATB_BillOfLadingType", "BOL", transferBill.ATB_BillOfLadingType);
			AssertEquals("ATB_ABL_Bill", manifestHeader.MasterBill.PK, transferBill.ATB_ABL_Bill);
			transferBill.ATB_BillNumber = "";
			AssertEquals("ATB_BillOfLadingType", "STD", transferBill.ATB_BillOfLadingType);
			AssertEquals("ATB_ABL_Bill", ZGuid.Empty, transferBill.ATB_ABL_Bill);
			houseBill.ABL_BolType = "";
			transferBill.ATB_BillNumber = "HB001";
			AssertEquals("ATB_BillOfLadingType", "STD", transferBill.ATB_BillOfLadingType);
			AssertEquals("ATB_ABL_Bill", houseBill.PK, transferBill.ATB_ABL_Bill);
		}

		public void TestATB_MessageStatusDescription()
		{
			AssertEquals("", transferBill.ATB_MessageStatusDescription);
			transferBill.ATB_MessageStatus = US.AIM.Messaging.AIMTransferStatusCodes.Codes.TransferAccepted;
			AssertEquals(US.AIM.Messaging.AIMTransferStatusCodes.Descriptions.TransferAccepted, transferBill.ATB_MessageStatusDescription);
		}

		public void TestIsActive()
		{
			transferBill.ATB_MessageStatus = "";
			Assert(transferBill.IsActive);
			transferBill.ATB_MessageStatus = AIMTransferStatusCodes.Codes.TransferError;
			Assert(transferBill.IsActive);
			transferBill.ATB_MessageStatus = AIMTransferStatusCodes.Codes.Arrived;
			Assert(!transferBill.IsActive);
			transferBill.ATB_MessageStatus = AIMTransferStatusCodes.Codes.TransferCancelled;
			Assert(!transferBill.IsActive);
		}

		public void TestIsSubmitted()
		{
			transferBill.ATB_MessageStatus = "";
			Assert(!transferBill.IsSubmitted);
			transferBill.ATB_MessageStatus = AIMTransferStatusCodes.Codes.ArrivalSent;
			Assert(transferBill.IsSubmitted);
			transferBill.ATB_MessageStatus = AIMTransferStatusCodes.Codes.TransferSent;
			Assert(transferBill.IsSubmitted);
			transferBill.ATB_MessageStatus = AIMTransferStatusCodes.Codes.ArrivalError;
			Assert(!transferBill.IsSubmitted);
			transferBill.ATB_MessageStatus = AIMTransferStatusCodes.Codes.TransferError;
			Assert(!transferBill.IsSubmitted);
		}

		public void TestIsSendable()
		{
			transferBill.ATB_MessageStatus = "";
			Assert(transferBill.IsSendable);
			transferBill.ATB_MessageStatus = AIMTransferStatusCodes.Codes.ArrivalSent;
			Assert(!transferBill.IsSendable);
			transferBill.ATB_MessageStatus = AIMTransferStatusCodes.Codes.TransferSent;
			Assert(!transferBill.IsSendable);
			transferBill.ATB_MessageStatus = AIMTransferStatusCodes.Codes.ArrivalError;
			Assert(!transferBill.IsSendable);
			transferBill.ATB_MessageStatus = AIMTransferStatusCodes.Codes.TransferError;
			Assert(transferBill.IsSendable);
		}

		public void TestIsCancellable()
		{
			transferBill.ATB_MessageStatus = "";
			Assert(!transferBill.IsCancellable);
			transferBill.ATB_MessageStatus = AIMTransferStatusCodes.Codes.ArrivalSent;
			Assert(!transferBill.IsCancellable);
			transferBill.ATB_MessageStatus = AIMTransferStatusCodes.Codes.TransferSent;
			Assert(transferBill.IsCancellable);
			transferBill.ATB_MessageStatus = AIMTransferStatusCodes.Codes.ArrivalError;
			Assert(!transferBill.IsCancellable);
			transferBill.ATB_MessageStatus = AIMTransferStatusCodes.Codes.TransferError;
			Assert(!transferBill.IsCancellable);
			transferBill.ATB_MessageStatus = AIMTransferStatusCodes.Codes.TransferAccepted;
			Assert(transferBill.IsCancellable);
		}

		public void TestReadOnly()
		{
			transferBill.ATB_MessageStatus = "";
			Assert(!transferBill.ReadOnly);
			transferBill.ATB_MessageStatus = AIMTransferStatusCodes.Codes.TransferSent;
			Assert(transferBill.ReadOnly);
			transferBill.ATB_MessageStatus = AIMTransferStatusCodes.Codes.TransferError;
			Assert(!transferBill.ReadOnly);
		}

		public void TestInBondNumber()
		{
			var houseBill = manifestHeader.Bills.AddNew();
			houseBill.ABL_BillNumber = "HB001";
			transferBill.ATB_BillNumber = "HB001";
			Factory.Save();
			AssertEquals("", transferBill.InBondNumber);
			transferBill.InBondNumber = "01234";
			AssertEquals("01234", transferBill.InBondNumber);
			var inBondNumberObj = transferBill.InBondNumberObj;
			AssertEquals("InBond CusEntryNum CE_Category", "CUS", inBondNumberObj.CE_Category);
			AssertEquals("InBond CusEntryNum CE_EntryType", "ICN", inBondNumberObj.CE_EntryType);
			AssertEquals("InBond CusEntryNum CE_RN_NKCountryCode", "US", inBondNumberObj.CE_RN_NKCountryCode);
			AssertEquals("InBond CusEntryNum CE_ParentTable", "AsycudaTransferBill", inBondNumberObj.CE_ParentTable);
			AssertEquals("InBond CusEntryNum CE_ParentID", transferBill.PK, inBondNumberObj.CE_ParentID);
		}

		public void TestAllocateInBondNumber()
		{
			US.Business.Testing.DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			var houseBill = manifestHeader.Bills.AddNew();
			houseBill.ABL_BillNumber = "HB001";
			transferBill.ATB_BillNumber = "HB001";
			Factory.Save();
			transferBill.AllocateInBondNumber("9123212223");
			AssertEquals("9123212223", transferBill.InBondNumber);
			AssertEquals(false, transferBill.InBondNumberObj.CE_EntryIsSystemGenerated);
			Factory.Save();
			AssertEquals("9123212223", transferBill.InBondNumber);
			transferBill.AllocateInBondNumber("");
			AssertEquals("9123212223", transferBill.InBondNumber);
			AssertEquals(false, transferBill.InBondNumberObj.CE_EntryIsSystemGenerated);
			Factory.Save();
			AssertNotEquals("9123212223", transferBill.InBondNumber);
			AssertNotEquals(ZString.Empty, transferBill.InBondNumber);
			AssertEquals(true, transferBill.InBondNumberObj.CE_EntryIsSystemGenerated);
			var number = transferBill.InBondNumber;
			transferBill.AllocateInBondNumber("");
			Factory.Save();
			AssertNotEquals(number, transferBill.InBondNumber);
			AssertNotEquals(ZString.Empty, transferBill.InBondNumber);
			AssertEquals(true, transferBill.InBondNumberObj.CE_EntryIsSystemGenerated);
		}

		public void TestInBondNumberAllocationMutex()
		{
			var houseBill = manifestHeader.Bills.AddNew();
			houseBill.ABL_BillNumber = "HB001";
			transferBill.ATB_BillNumber = "HB001";
			AssertEquals(true, transferBill.LockInBondNumberAllocationMutex());
			AssertEquals(true, transferBill.InBondNumberAllocationMutexHasLock());
			AssertEquals("Is still locked", true, transferBill.LockInBondNumberAllocationMutex());
			AssertEquals(true, transferBill.InBondNumberAllocationMutexHasLock());
			transferBill.UnLockInBondNumberAllocationMutex();
			AssertEquals(false, transferBill.InBondNumberAllocationMutexHasLock());
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var moveHeaderInDiffFactory = newFactory.Load<AsycudaTransferBill>(transferBill.PK);
			AssertEquals(true, moveHeaderInDiffFactory.LockInBondNumberAllocationMutex());
			AssertEquals(false, transferBill.LockInBondNumberAllocationMutex());
			moveHeaderInDiffFactory.UnLockInBondNumberAllocationMutex();
		}

		public void TestInBondNumberAllocationMutexLockInfo_NullUser()
		{
			var houseBill = manifestHeader.Bills.AddNew();
			houseBill.ABL_BillNumber = "HB001";
			transferBill.ATB_BillNumber = "HB001";
			using (var mutex = new ZGlobalMutex(MutexIDs.CustomsTransactionIDAllocation, "INB" + transferBill.PK.ToString()))
			{
				Assert(mutex.Lock());
				var lockInfo = "Mutex:" + MutexIDs.CustomsTransactionIDAllocation.Name + ":INB" + transferBill.PK.ToString();
				var emptyGuid = Guid.Empty;
				var sql = $@"UPDATE TOP(1) StmServiceHeartBeat
									SET SV_ParentId = '{emptyGuid}',
										SV_SystemLastEditTimeUtc = GetUtcDate(),
										SV_SystemLastEditUser = 'USR'
									FROM dbo.StmServiceSemaphore
									INNER JOIN dbo.StmServiceHeartBeat ON SS_SV = SV_PK
									WHERE SS_LockInfo LIKE '%{lockInfo}%';";
				TestConnection.ExecuteNonQuery(sql);
				AssertEquals(false, transferBill.LockInBondNumberAllocationMutex());
				AssertNoExceptionThrown(() => transferBill.GetInBondNumberAllocationMutexLockInfo());
			}
		}

		public void TestResetInBondNumber()
		{
			transferBill.ATB_CustomsStatus = ZString.Empty;
			Assert(transferBill.IsInBondNumberResetable);
			transferBill.ATB_CustomsStatus = ImportMessageStatusList.Codes.AwaitingDepartureOriginal;
			Assert(!transferBill.IsInBondNumberResetable);
			transferBill.ATB_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			Assert(!transferBill.IsInBondNumberResetable);
			transferBill.ATB_CustomsStatus = ImportMessageStatusList.Codes.AwaitingDepartureWithdraw;
			transferBill.ATB_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureWithdraw;
			Assert(transferBill.IsInBondNumberResetable);
			transferBill.AllocateInBondNumber("1234");
			Factory.Save();
			var inBondNumber = transferBill.InBondNumber;
			Assert(!inBondNumber.IsEmpty);
			transferBill.ResetInBondNumber("TEST RESET");
			Factory.Save();
			Assert(transferBill.InBondNumber.IsEmpty);
			var mostRecentLog = transferBill.Logs.MostRecentLog;
			AssertEquals(Events.ResetEntryMessageItemFunction.Code, mostRecentLog.Event.SE_Code);
			AssertContains($"Previous In-Bond #: {inBondNumber}. Reset reason: TEST RESET", mostRecentLog.SL_Reference);
		}

		public void TestCanDelete()
		{
			var houseBill = manifestHeader.Bills.AddNew();
			houseBill.ABL_BillNumber = "HB001";
			transferBill.ATB_BillNumber = "HB001";
			Factory.Save();
			AssertEquals(true, transferBill.CanDelete);
			var result = transferBill.LockInBondNumberAllocationMutex();
			AssertEquals(true, result);
			AssertEquals(false, transferBill.CanDelete);
			transferBill.UnLockInBondNumberAllocationMutex();
			AssertEquals(true, transferBill.CanDelete);
			transferBill.Delete();
		}

		public void TestUnableToDeleteWhenInBondNumberAllocationMutexIsLocked()
		{
			var houseBill = manifestHeader.Bills.AddNew();
			houseBill.ABL_BillNumber = "HB001";
			transferBill.ATB_BillNumber = "HB001";
			AssertEquals(ZString.Empty, transferBill.ReasonForNotAbleToDelete);
			var result = transferBill.LockInBondNumberAllocationMutex();
			AssertEquals(true, result);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var loadedMove = newFactory.Load<AsycudaTransferBill>(transferBill.PK);
			AssertNoExceptionThrown(() =>
			{
				loadedMove.Delete();
			});
			AssertEquals(false, loadedMove.IsDeleted);
			AssertContains("is in the process of allocating a new InBond Number for this movement; this movement cannot be deleted.", loadedMove.ReasonForNotAbleToDelete);
			transferBill.UnLockInBondNumberAllocationMutex();
			AssertEquals(ZString.Empty, loadedMove.ReasonForNotAbleToDelete);
			AssertNoExceptionThrown(() =>
			{
				loadedMove.Delete();
			});
			AssertEquals(true, loadedMove.IsDeleted);
		}

		public void TestHasBeenDeleted()
		{
			Factory.RefreshEnabled = false;
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			var transferBillIOF = newFactory.Load<AsycudaTransferBill>(transferBill.PK);
			transferBill.Delete();
			Factory.Save();
			Assert("HasBeenDeleted", transferBillIOF.HasBeenDeleted);
		}

		public void TestReloadInBondNumberObj()
		{
			var houseBill = manifestHeader.Bills.AddNew();
			houseBill.ABL_BillNumber = "HB001";
			transferBill.ATB_BillNumber = "HB001";
			transferBill.AllocateInBondNumber("TST0001");
			var number01 = transferBill.InBondNumberObj.CE_EntryNum;
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var loadedMove = newFactory.Load<AsycudaTransferBill>(transferBill.PK);
			var number02 = loadedMove.InBondNumberObj.CE_EntryNum;
			AssertEquals(number01, number02);
		}

		public void TestTransferHeader()
		{
			var transferHeader = Factory.New<AsycudaTransferHeader>();
			var transferBill = transferHeader.TransferBills.AddNew();
			AssertEquals(typeof(AsycudaTransferHeader), transferBill.TransferHeader.GetType());
			AssertEquals(typeof(AsycudaTransferHeader), ((ASYCUDA.Business.AsycudaTransferBill)transferBill).TransferHeader.GetType());
		}

		public void TestGetNewLookups()
		{
			var transferBill = Factory.New<AsycudaTransferBill>();
			AssertEquals(typeof(AsycudaTransferBillLookups), transferBill.Lookups.GetType());
			AssertEquals(typeof(AsycudaTransferBillLookups), ((ASYCUDA.Business.AsycudaTransferBill)transferBill).Lookups.GetType());
		}

		public void TestGetNewValidation()
		{
			var transferBill = Factory.New<AsycudaTransferBill>();
			AssertEquals(typeof(AsycudaTransferBillValidation), transferBill.Validation.GetType());
			AssertEquals(typeof(AsycudaTransferBillValidation), ((ASYCUDA.Business.AsycudaTransferBill)transferBill).Validation.GetType());
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return transferBill;
		}

		protected override void SetUp()
		{
			base.SetUp();
			manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_MasterBill = "MAN001";
			manifestHeader.AMA_JobReference = "MAN001";
			var arrivalHeader = manifestHeader.ArrivalHeaders.AddNew();
			var transferHeader = arrivalHeader.TransferHeaders.AddNew();
			transferHeader.ATF_TransferType = ManifestBase.TransferTypeList.Codes.Domestic;
			transferBill = transferHeader.TransferBills.AddNew();
		}

		AsycudaManifestHeader manifestHeader;
		AsycudaTransferBill transferBill;
	}
}
