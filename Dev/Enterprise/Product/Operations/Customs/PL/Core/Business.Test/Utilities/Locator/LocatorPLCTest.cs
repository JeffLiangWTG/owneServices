using CargoWise.EntityFramework;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.PL.Business.Testing;

class LocatorPLCTest : LocatorTestBase<LocatorPLC, EDIMessage>
{
	protected override string ExpectedApplicationCodes => ApplicationCodeList.Codes.PLCustoms;

	protected override string MessageApplicationCode => ApplicationCodeList.Codes.PLCustoms;

	protected override BusinessObject CreateLinkedObject()
	{
		var cusEntryHeader = Factory.New<CusEntryHeader>();
		cusEntryHeader.CH_DataModel = Core.Constants.CountryCodes.Poland;

		return cusEntryHeader;
	}

	protected override void AllocateLRN(BusinessObject linkedObject, string lrn) =>
		((CusEntryHeader)linkedObject).CH_BGMReference = lrn;
}
