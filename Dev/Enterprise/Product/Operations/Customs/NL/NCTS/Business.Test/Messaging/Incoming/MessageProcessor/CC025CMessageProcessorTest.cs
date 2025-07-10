using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(CC025CMessageProcessor))]
sealed class CC025CMessageProcessorTest : NCTSResponseMessageProcessorAbstractTest<CC025CMessageProcessor, ICC025CDataProvider>
{
	public void TestCESEvent()
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader);
		Factory.Save();

		var nctsHeader = incomingMessage.EM_LinkedObject as NctsHeader;
		var movementHeader = nctsHeader.ArrivalMovementHeader;

		var eventLog = movementHeader.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => l.SL_SE_NKEvent == AutoEvents.CustomsEntryStatusCode && (l.SL_Reference == NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease));
		AssertNotNull("Event should be created", eventLog);
		AssertEquals("Date is correct", $"{ZDateTime.Now.Year}.{ZDateTime.Now.Month}.{ZDateTime.Now.Day}", $"{eventLog.SL_EventTime.Year}.{eventLog.SL_EventTime.Month}.{eventLog.SL_EventTime.Day}");
		AssertEquals("Branch", "BNE", eventLog.SL_GB_NKBranch);
		AssertEquals("Department", "BRN", eventLog.SL_GE_NKDepartment);
		AssertEquals("Reference", NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease, eventLog.SL_Reference);
		AssertEquals("Only 1 CES-event should have been created", 1, movementHeader.Logs.Find(x => x.SL_SE_NKEvent == Events.CustomsEntryStatus.Code && x.SL_Reference == movementHeader.BM_CustomsStatus).Count());
	}

	protected override string MovementType => NctsMovementType.Codes.Arrival;

	protected override string InitialCustomsStatus => NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks;

	protected override string ExpectedCustomsStatus => NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;

	protected override string ExpectedPhase => NctsMovementHeaderTransactionStatusList.Codes.Declaration;

	protected override Action<NctsHeader> SetupNctsHeader => (NctsHeader header) =>
		{
			var mrnEntryNumber = CusEntryNumber.LoadOrCreate(header, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Netherlands);
			mrnEntryNumber.CE_EntryNum = "TestMRN";
		};

	protected override ICC025CDataProvider GetMessageDataProviderMock()
	{
		return Mock.Of<ICC025CDataProvider>(m =>
			m.MRN == "TestMRN" &&
			m.ReleaseDate == ZDateTime.Now.ToDateTime() &&
			m.ReleaseIndicator == NLNctsConstants.ReleaseIndicator.FullRelease &&
			m.HouseConsignments == new ReadOnlyCollection<INCTSHouseConsignmentProvider>(new List<INCTSHouseConsignmentProvider>() {
						Mock.Of<INCTSHouseConsignmentProvider>(h =>
							h.SequenceNumeric == 1 &&
							h.ReleaseType == 2 &&
							h.ConsignmentItems == new ReadOnlyCollection<INCTSConsignmentItemProvider>(new List<INCTSConsignmentItemProvider>() {
								Mock.Of<INCTSConsignmentItemProvider>(c =>
									c.DeclarationGoodsItemNumber == 1 &&
									c.ReleaseType == 2 &&
									c.Packagings == new ReadOnlyCollection<INCTSPackagingProvider>(new List<INCTSPackagingProvider>() {
										Mock.Of<INCTSPackagingProvider>(p =>
											p.NumberOfPackages == 5 &&
											p.TypeOfPackages == "PKG" &&
											p.ShippingMarks == "marksandnumbers"
										)
									})
								)
							})
						)
			})
		);
	}
}
