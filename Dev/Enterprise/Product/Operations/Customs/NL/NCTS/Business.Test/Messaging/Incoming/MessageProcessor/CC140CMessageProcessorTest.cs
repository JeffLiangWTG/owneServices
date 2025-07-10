using System;
using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Business;
using Moq;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class CC140CMessageProcessorTest : NCTSResponseMessageProcessorAbstractTest<CC140CMessageProcessor, ICC140CDataProvider>
{
	public void TestCIPEvent()
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader);

		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;

		AssertNotNull("CusInBondHeader should have a CIP event logged", nctsHeader.Logs.Find(x => x.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code).SingleOrDefault());
	}

	public void TestCustomsOfficeOfEnquiry()
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader);

		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;

		CombineAssertions(() =>
		{
			AssertEquals("There should be a customs office created", 1, movementHeader.CustomsOffices.Where(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfEnquiry).Count());
			AssertEquals("The customs office should be the same as CustomsOfficeOfEnquiryReferenceNumber", "NL000432", movementHeader.CustomsOffices.Where(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfEnquiry).FirstOrDefault().CY_Data);
		});
	}

	protected override ICC140CDataProvider GetMessageDataProviderMock()
	{
		return Mock.Of<ICC140CDataProvider>(p =>
			p.MRN == "22NL000000000012J1" &&
			p.RequestOnNonArrivedMovementDate == new DateTime(2024, 2, 29) &&
			p.LimitForResponseDate == new DateTime(2024, 3, 28) &&
			p.CustomsOfficeOfEnquiryReferenceNumber == "NL000432"
		);
	}

	protected override string InitialCustomsStatus => NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit;

	protected override Action<NctsHeader> SetupNctsHeader => (nctsHeader) =>
	{
		var mrnEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Netherlands);
		mrnEntryNumber.CE_EntryNum = "22NL000000000012J1";
	};

	protected override string ExpectedCustomsStatus => NCTS5DepartureCustomsStatusList.Codes.UnderEnquiry;

	protected override string ExpectedPhase => NCTS5DeparturePhaseList.Codes.Declaration;

	protected override string ExpectedMessageStatus => LogicalStatusList.Codes.Accepted;
}
