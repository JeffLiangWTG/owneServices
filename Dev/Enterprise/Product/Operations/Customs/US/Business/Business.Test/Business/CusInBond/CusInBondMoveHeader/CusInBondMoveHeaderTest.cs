using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestsSubclassesOf(typeof(CusInBondMoveHeader))]
	public abstract class CusInBondMoveHeaderTest<T> : EnterpriseBusinessObjectTestCase
		where T : CusInBondMoveHeader
	{
		public virtual void TestLookups()
		{
			AssertEquals(true, typeof(CusInBondMoveHeaderLookups).IsAssignableFrom(moveHeader.Lookups.GetType()));
		}

		public virtual void TestValidation()
		{
			AssertEquals(true, typeof(CusInBondMoveHeaderValidation).IsAssignableFrom(moveHeader.Validation.GetType()));
		}

		public virtual void TestHumanReadableName()
		{
			AssertEquals("", moveHeader.HumanReadableName);
			moveHeader.InBondNumber = "234322123";
			AssertEquals("234322123", moveHeader.HumanReadableName);
		}

		public virtual void TestMovementDescription()
		{
			AssertEquals(ZString.Empty, moveHeader.MovementDescription);
			moveHeader.InBondNumber = "INB123456";
			AssertEquals("INB123456", moveHeader.MovementDescription);
			ZStringBuilder builder = new ZStringBuilder();
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			moveHeader.BM_BTAIndicator = ZString.Empty;
			builder.Append("Type:" + InbondCommonTypeList.Codes._2TransportandExport);
			AssertEquals(builder.ToStringWithDelimiterBetweenAppends(" "), moveHeader.MovementDescription);
			moveHeader.BM_InBondCarrierSCAC = "AAPT";
			builder.Append("SCAC:AAPT");
			AssertEquals(builder.ToStringWithDelimiterBetweenAppends(" "), moveHeader.MovementDescription);
			moveHeader.BM_DestinationPortCode = "2705";
			builder.Append("US Dest.:2705");
			AssertEquals(builder.ToStringWithDelimiterBetweenAppends(" "), moveHeader.MovementDescription);
			moveHeader.BM_ForeignDestPortKCode = "8205";
			builder.Append("Foreign Dest.:8205");
			AssertEquals(builder.ToStringWithDelimiterBetweenAppends(" "), moveHeader.MovementDescription);
			moveHeader.BM_InBondCarrierID = "6369557452";
			builder.Append("Carrier ID:6369557452");
			AssertEquals(builder.ToStringWithDelimiterBetweenAppends(" "), moveHeader.MovementDescription);
			moveHeader.BM_BTAIndicator = Enterprise.Customs.US.Business.YesNoDefaultList.Codes.Yes;
			builder.Append("BTA:Y");
			AssertEquals(builder.ToStringWithDelimiterBetweenAppends(" "), moveHeader.MovementDescription);
			moveHeader.BM_MonetaryValue = 10.50m;
			builder.Append("Value:10.50");
			AssertEquals(builder.ToStringWithDelimiterBetweenAppends(" "), moveHeader.MovementDescription);
		}

		public void TestForeignDestPortKCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
			var foreignPort1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "12345", "port1", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(foreignPort1.PK, RefCusCodeListAttributeTypes.Codes.PortValidType, ForeignPortTypeList.Codes.Common);
			Factory.Save();

			moveHeader.BM_ForeignDestPortKCode = "12345";

			var foreignDestPortKCode = moveHeader.ForeignDestPortKCode;
			CombineAssertions(() =>
			{
				AssertNotNull(foreignDestPortKCode);
				AssertEquals("port1", foreignDestPortKCode.ZZD_Description);
				AssertSame("Cached", Factory.GetCachedValue<ZZRefCusCodeListCombined>("CusInBondMoveHeader|12345", () => null), foreignDestPortKCode);
			});
		}

		public void TestBM_DestinationPortcode()
		{
			var header = CreateNewCusInBondHeader();
			CusInBondMoveHeader moveHeader1 = CreateNewCusInBondMoveHeader(header);
			AssertEquals("MaxLength 4 for BM_DestinationPortCode", 4, moveHeader1.BM_DestinationPortCodeInfo.MaxLength);
		}

		public virtual void TestMovementUniqueCode()
		{
			var header = CreateNewCusInBondHeader();
			CusInBondMoveHeader moveHeader1 = CreateNewCusInBondMoveHeader(header);
			moveHeader1.BM_InBondEntryType = "2";
			CusInBondMoveHeader moveHeader2 = CreateNewCusInBondMoveHeader(header);
			moveHeader2.InBondNumber = "INB12365";
			moveHeader2.BM_InBondEntryType = "1";
			CusInBondMoveHeader moveHeader3 = CreateNewCusInBondMoveHeader(header);
			moveHeader3.BM_InBondEntryType = "4";
			CusInBondMoveHeader moveHeader4 = CreateNewCusInBondMoveHeader(header);
			moveHeader4.BM_InBondEntryType = "3";
			AssertEquals("NOT YET SPECIFIED 1", moveHeader1.MovementUniqueCode);
			AssertEquals("INB12365", moveHeader2.MovementUniqueCode);
			AssertEquals("NOT YET SPECIFIED 2", moveHeader3.MovementUniqueCode);
			AssertEquals("NOT YET SPECIFIED 3", moveHeader4.MovementUniqueCode);
		}

		public void TestDisallowAllocateInBondNumber()
		{
			AssertEquals("", moveHeader.DisallowAllocateInBondNumber);
			moveHeader.InBondNumber = "INB2423";
			AssertEquals(CusInBondMoveHeader.Constants.InBondNumberAlreadyAllocated("INB2423"), moveHeader.DisallowAllocateInBondNumber);
		}

		public void TestInBondNumberAllocationMutex()
		{
			AssertEquals(true, moveHeader.LockInBondNumberAllocationMutex());
			AssertEquals(true, moveHeader.InBondNumberAllocationMutexHasLock());
			AssertEquals("Is still locked", true, moveHeader.LockInBondNumberAllocationMutex());
			AssertEquals(true, moveHeader.InBondNumberAllocationMutexHasLock());
			moveHeader.UnLockInBondNumberAllocationMutex();
			AssertEquals(false, moveHeader.InBondNumberAllocationMutexHasLock());
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var moveHeaderInDiffFactory = newFactory.Load<T>(moveHeader.PK);
			AssertEquals(true, moveHeaderInDiffFactory.LockInBondNumberAllocationMutex());
			AssertEquals(false, moveHeader.LockInBondNumberAllocationMutex());
			moveHeaderInDiffFactory.UnLockInBondNumberAllocationMutex();
		}

		public void TestInBondNumberAllocationMutexLockInfo_NullUser()
		{
			using (var mutex = new ZGlobalMutex(MutexIDs.CustomsTransactionIDAllocation, "INB" + moveHeader.PK.ToString()))
			{
				Assert(mutex.Lock());
				var lockInfo = "Mutex:" + MutexIDs.CustomsTransactionIDAllocation.Name + ":INB" + moveHeader.PK.ToString();
				var emptyGuid = Guid.Empty;
				var sql = $@"UPDATE TOP(1) StmServiceHeartBeat
									SET SV_ParentId = '{emptyGuid}',
										SV_SystemLastEditTimeUtc = GetUtcDate(),
										SV_SystemLastEditUser = 'USR'
									FROM dbo.StmServiceSemaphore
									INNER JOIN dbo.StmServiceHeartBeat ON SS_SV = SV_PK
									WHERE SS_LockInfo LIKE '%{lockInfo}%';";
				TestConnection.ExecuteNonQuery(sql);
				AssertEquals(false, moveHeader.LockInBondNumberAllocationMutex());
				AssertNoExceptionThrown(() => moveHeader.GetInBondNumberAllocationMutexLockInfo());
			}
		}

		public void TestCanDelete()
		{
			Factory.Save();
			AssertEquals(true, moveHeader.CanDelete);
			var result = moveHeader.LockInBondNumberAllocationMutex();
			AssertEquals(true, result);
			AssertEquals(false, moveHeader.CanDelete);
			moveHeader.UnLockInBondNumberAllocationMutex();
			AssertEquals(true, moveHeader.CanDelete);
			moveHeader.Delete();
		}

		public void TestUnableToDeleteWhenInBondNumberAllocationMutexIsLocked()
		{
			AssertEquals(ZString.Empty, moveHeader.ReasonForNotAbleToDelete);
			var result = moveHeader.LockInBondNumberAllocationMutex();
			AssertEquals(true, result);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var loadedMove = newFactory.Load<T>(moveHeader.PK);
			AssertNoExceptionThrown(() =>
			{
				loadedMove.Delete();
			});
			AssertEquals(false, loadedMove.IsDeleted);
			AssertContains("is in the process of allocating a new InBond Number for this movement; this movement cannot be deleted.", loadedMove.ReasonForNotAbleToDelete);
			moveHeader.UnLockInBondNumberAllocationMutex();
			AssertEquals(ZString.Empty, loadedMove.ReasonForNotAbleToDelete);
			AssertNoExceptionThrown(() =>
			{
				loadedMove.Delete();
			});
			AssertEquals(true, loadedMove.IsDeleted);
		}

		public void TestReloadInBondNumberObj()
		{
			moveHeader.AllocateInBondNumber("TST0001");
			var number01 = moveHeader.InBondNumberObj.CE_EntryNum;
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var loadedMove = newFactory.Load<T>(moveHeader.PK);
			var number02 = loadedMove.InBondNumberObj.CE_EntryNum;
			AssertEquals(number01, number02);
		}

		public void TestAllocateInBondNumber()
		{
			DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			moveHeader.AllocateInBondNumber("9123212223");
			AssertEquals("9123212223", moveHeader.InBondNumber);
			AssertEquals(false, moveHeader.InBondNumberObj.CE_EntryIsSystemGenerated);
			Factory.Save();
			AssertEquals("9123212223", moveHeader.InBondNumber);
			moveHeader.AllocateInBondNumber("");
			AssertEquals("9123212223", moveHeader.InBondNumber);
			AssertEquals(false, moveHeader.InBondNumberObj.CE_EntryIsSystemGenerated);
			Factory.Save();
			AssertNotEquals("9123212223", moveHeader.InBondNumber);
			AssertEquals(true, moveHeader.InBondNumberObj.CE_EntryIsSystemGenerated);
			ZString number = moveHeader.InBondNumber;
			moveHeader.AllocateInBondNumber("");
			var moveDetail = (Customs.Business.CusInBondMoveDetail)moveHeader.MovementDetails.AddNew();
			AssertExceptionThrown<ZSaveException>(() => Factory.Save());
			AssertEquals(false, moveDetail.IsInDatabase);
			AssertEquals(number, moveHeader.InBondNumber);
			AssertEquals(true, moveHeader.InBondNumberObj.CE_EntryIsSystemGenerated);
			var bill = (Customs.Business.CusInBondBill)header.Bills.AddNew();
			bill.B0_MasterBillNumber = "MB1";
			moveDetail.B9_B0 = bill.PK;
			Factory.Save();
			AssertEquals(true, moveDetail.IsInDatabase);
			AssertEquals(number, moveHeader.InBondNumber);
			AssertNotEquals(ZString.Empty, moveHeader.InBondNumber);
			AssertEquals(true, moveHeader.InBondNumberObj.CE_EntryIsSystemGenerated);
			ZString number2 = moveHeader.InBondNumber;
			moveHeader.AllocateInBondNumber("");
			Factory.Save();
			AssertNotEquals(number, moveHeader.InBondNumber);
			AssertNotEquals(number2, moveHeader.InBondNumber);
			AssertNotEquals(ZString.Empty, moveHeader.InBondNumber);
			AssertEquals(true, moveHeader.InBondNumberObj.CE_EntryIsSystemGenerated);
			header.BH_PostDepartureOnly = true;
			moveHeader.InBondNumber = "";
			moveHeader.AllocateInBondNumber("");
			Factory.Save();
			AssertEquals(ZString.Empty, moveHeader.InBondNumber);
		}

		public void TestInBondNumberObj()
		{
			moveHeader = CreateNewCusInBondMoveHeader(header);
			var num = Factory.New<CusEntryNumber>();
			num.CE_ParentTable = CusInBondMoveHeader.Schema.TableName;
			num.CE_ParentID = moveHeader.PK;
			num.CE_EntryType = CusEntryNumber.EntryType.InBond;
			num.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			num.CE_EntryNum = "N0";
			num.CE_SystemCreateTimeUtc = ZDateTime.UtcNow;
			Factory.Save();
			AssertEquals(num.PK, moveHeader.InBondNumberObj.PK);
		}

		[TestDate(2012, 4, 1)]
		public void TestReportLimitHasReachedIfNeededForBranch()
		{
			var staff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			staff.GS_EmailAddress = "bob@where.com";
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "!2s";
			var staff2 = group.Staff.AddNew();
			staff2.GS_Code = "3$3";
			staff2.GS_LoginName = "32!";
			staff2.GS_EmailAddress = "joe@who.com";
			var staff3 = Factory.New<GlbStaff>();
			staff3.GS_Code = "4$4";
			staff3.GS_LoginName = "95!";
			staff3.GS_EmailAddress = "jay@how.com";
			Factory.Save();
			USCustomsDataRegistry.Instance.NumberRangeLimitWarningGroup.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new GroupNotification(Core.Constants.EmailTo.StaffMemberAndNominatedGroup, group.PK));
			var connection = ((IDbConnected)Factory).Connection;
			connection.BeginTransaction();
			try
			{
				DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
				var numberRange = USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange.GetValueWithoutFallback(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
				var branchNumberFountain = Env.NumberFountains.USInBondNumberFountain(GlbBranch.CurrentBranch.PK.ToGuid());
				branchNumberFountain.SetNext(Factory, (long)(numberRange.LastNumber - 101));
				var dateTime = ZDateTime.Now.AddHours(-1).ToDateTime();
				DataRegistry.Business.USCustomsDataRegistry.Instance.NextInBondNumberLimitWarningReportRun.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, dateTime);
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				moveHeader.AllocateInBondNumber("");
				Factory.Save();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("Subject", "INBOND NUMBER RANGE LIMIT WARNING", email.Subject);
				AssertEquals("Body", string.Format("The In-Bond Number Range set up for branch ('{0} - {1}') is running out.\r\nThere are only 101 numbers remaining.\r\nYou will need to prepare to allocate a new number range.", GlbBranch.CurrentBranch.GB_Code, GlbBranch.CurrentBranch.GB_BranchName), email.Body);
				AssertEquals(ZDateTime.Now.AddDays(1), DataRegistry.Business.USCustomsDataRegistry.Instance.NextInBondNumberLimitWarningReportRun.GetValueWithoutFallback(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty));
				AssertEquals(2, email.Recipients.Count);
				AssertEquals("Email contains " + staff.GS_EmailAddress, true, email.Recipients.Contains(staff.GS_EmailAddress));
				AssertEquals("Email contains " + staff2.GS_EmailAddress, true, email.Recipients.Contains(staff2.GS_EmailAddress));
				while (branchNumberFountain.GetNext(Factory) != numberRange.LastNumber)
				{
				}

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				moveHeader.AllocateInBondNumber("");
				Factory.Save();
				AssertEquals("Should not run as it's not the right time", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				DataRegistry.Business.USCustomsDataRegistry.Instance.NextInBondNumberLimitWarningReportRun.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, dateTime);
				staff.GS_EmailAddress = ZString.Empty;
				GlbStaff.CurrentUser.GS_EmailAddress = ZString.Empty;
				Factory.Save();
				moveHeader.AllocateInBondNumber("");
				Factory.Save();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("Subject", "INBOND NUMBER RANGE LIMIT WARNING", email.Subject);
				AssertEquals("Body", string.Format("The In-Bond Number Range set up for branch ('{0} - {1}') has run out.\r\nPlease allocate a new number range.", GlbBranch.CurrentBranch.GB_Code, GlbBranch.CurrentBranch.GB_BranchName), email.Body);
				AssertEquals(ZDateTime.Now.AddDays(1), DataRegistry.Business.USCustomsDataRegistry.Instance.NextInBondNumberLimitWarningReportRun.GetValueWithoutFallback(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty));
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("Email", staff2.GS_EmailAddress, email.Recipients[0].Email);
				staff2.GS_EmailAddress = ZString.Empty;
				Factory.Save();
				DataRegistry.Business.USCustomsDataRegistry.Instance.NextInBondNumberLimitWarningReportRun.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, dateTime);
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				moveHeader.AllocateInBondNumber("");
				Factory.Save();
				AssertEquals(0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			}
			finally
			{
				connection.RollbackTransaction();
			}
		}

		public void TestTOLCarrierCityAndStateDefault()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.PuertoRico;
			var tOLCarrier = Factory.NewWithValidTestData<OrgHeader>();
			tOLCarrier.OH_RL_NKClosestPort = "USLAX";
			tOLCarrier.Addresses[0].OA_City = "Los Angeles";
			tOLCarrier.Addresses[0].OA_Address1 = "Main St.";
			var address2 = tOLCarrier.Addresses.AddNew();
			address2.OA_Address1 = "Park Lane";
			address2.OA_RL_NKRelatedPortCode = "USSFO";
			address2.OA_City = "San Francisco";
			address2.OA_State = "CA";
			tOLCarrier.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "1234", Core.Constants.CountryCodes.UnitedStates);
			tOLCarrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "ABCD", Core.Constants.CountryCodes.UnitedStates);
			moveHeader.BM_OA_TOLCarrier = address2.PK;
			moveHeader.TOLCarrierOrgPK = tOLCarrier.PK;
			AssertEquals("TOL City", "San Francisco", moveHeader.BM_TOLCityName);
			AssertEquals("TOL State", "CA", moveHeader.BM_TOLStateCode);
			moveHeader.TOLCarrierOrgPK = ZGuid.Empty;
			moveHeader.TOLCarrierOrgPK = tOLCarrier.PK;
			AssertEquals("Carrier ID", "1234", moveHeader.BM_TOLCarrierID);
			AssertEquals("Carrier Code", "ABCD", moveHeader.BM_TOLCarrierCode);
		}

		public void TestInBondCarrierDefault()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.PuertoRico))
			{
				var carrier = Factory.NewWithValidTestData<OrgHeader>();
				carrier.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "13-1502798000", Core.Constants.CountryCodes.UnitedStates);
				carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "ABCD", Core.Constants.CountryCodes.UnitedStates);
				var address2 = carrier.Addresses.AddNew();
				address2.OA_Address1 = "Park Lane";
				address2.OA_RL_NKRelatedPortCode = "USSFO";
				address2.OA_City = "San Francisco";
				address2.OA_State = "CA";
				moveHeader.BM_OA_InBondCarrier = address2.PK;
				moveHeader.InBondCarrierOrgPK = ZGuid.Empty;
				moveHeader.InBondCarrierOrgPK = carrier.PK;
				AssertEquals("Should not be defaulted because EIN format is wrong", ZString.Empty, moveHeader.BM_InBondCarrierID);
				AssertEquals("Carrier code", "ABCD", moveHeader.BM_InBondCarrierSCAC);
				carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.TruckCarrierCode, "OKD2", Core.Constants.CountryCodes.UnitedStates);
				var code = carrier.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, Core.Constants.CountryCodes.UnitedStates);
				code.OK_CustomsRegNo = "13-150279800";
				var moveHeader2 = CreateNewCusInBondMoveHeader(header);
				moveHeader2.BM_OA_InBondCarrier = address2.PK;
				moveHeader2.InBondCarrierOrgPK = ZGuid.Empty;
				moveHeader2.InBondCarrierOrgPK = carrier.PK;
				AssertEquals("Should be defaulted, because EIN is correct", "13-150279800", moveHeader2.BM_InBondCarrierID);
				AssertEquals("Carrier code", "OKD2", moveHeader2.BM_InBondCarrierSCAC);
			}
		}

		public void TestBM_BTAIndicatorDefaulting()
		{
			AssertEquals(ZString.Empty, moveHeader.BM_BTAIndicator);
			AssertEquals(false, moveHeader.BM_BTAIndicatorInfo.ReadOnly);
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._3ImmediateExport;
			AssertEquals(YesNoDefaultList.Codes.No, moveHeader.BM_BTAIndicator);
			AssertEquals(true, moveHeader.BM_BTAIndicatorInfo.ReadOnly);
			moveHeader.BM_BTAIndicator = YesNoDefaultList.Codes.Yes;
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			AssertEquals(YesNoDefaultList.Codes.Yes, moveHeader.BM_BTAIndicator);
			AssertEquals(false, moveHeader.BM_BTAIndicatorInfo.ReadOnly);
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._1ImmediateTransport;
			AssertEquals(YesNoDefaultList.Codes.Yes, moveHeader.BM_BTAIndicator);
			AssertEquals(false, moveHeader.BM_BTAIndicatorInfo.ReadOnly);
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._3ImmediateExport;
			AssertEquals(YesNoDefaultList.Codes.No, moveHeader.BM_BTAIndicator);
			AssertEquals(true, moveHeader.BM_BTAIndicatorInfo.ReadOnly);
		}

		public virtual void TestThrowAwayInBondNumber()
		{
			var inbond = Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			inbond.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.InBond;
			inbond.BH_JobReference = "INB0000002";
			inbond.BH_CarrierSCAC = "EGLV";
			inbond.BH_PortUnladingDCode = "2704";
			var inbondMoveHeader = Factory.New<Integration.Customs.US.InBond.ICusInBondMoveHeader>();
			inbondMoveHeader.InBondNumber = "693000140";
			inbondMoveHeader.BM_BH = inbond.PK;
			var moveHeaderPK = inbondMoveHeader.PK;
			var query = new ZQuery(CusEntryNumSchema.CE_EntryNum, "693000140");
			var inbondNum = Factory.Load<CusEntryNumber>(query);
			AssertEquals("CusEntryNum for moveHeader exists", 1, inbondNum.Length);
			AssertEquals("CusEntryNum for moveHeader exists", inbondNum[0].CE_ParentID, inbondMoveHeader.PK);
			((Customs.Business.CusInBondHeader)inbond).MovementHeader.Delete();
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			inbondNum = newFactory.Load<CusEntryNumber>(query);
			AssertEquals("Movement Header was deleted with his InBond number", 0, inbondNum.Length);
		}

		protected Customs.Business.CusInBondHeader header;
		protected T moveHeader;
		protected override void SetUp()
		{
			base.SetUp();
			header = CreateNewCusInBondHeader();
			moveHeader = CreateNewCusInBondMoveHeader(header);
		}

		protected abstract T CreateNewCusInBondMoveHeader(Customs.Business.CusInBondHeader header);

		protected abstract Customs.Business.CusInBondHeader CreateNewCusInBondHeader();

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = CreateNewCusInBondHeader();
			return CreateNewCusInBondMoveHeader(header);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			header = CreateNewCusInBondHeader();
			return CreateNewCusInBondMoveHeader(header);
		}
	}
}
