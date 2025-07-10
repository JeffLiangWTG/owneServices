using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.Customs.TR.Messaging;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	public class CusEntryHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPaymentTypesList()
		{
			var cusEntryHeader = Factory.New<CusEntryHeader>();
			var lookups = cusEntryHeader.Lookups;

			AssertContainsExactElementsInAnyOrder(new ICodeDescription[]
			{
				new CodeDescriptionPair(TRMessageStatusCodeList.Codes.Accepted, TRMessageStatusCodeList.Descriptions.Accepted),
				new CodeDescriptionPair(TRMessageStatusCodeList.Codes.Awaiting, TRMessageStatusCodeList.Descriptions.Awaiting),
				new CodeDescriptionPair(TRMessageStatusCodeList.Codes.Cancel, TRMessageStatusCodeList.Descriptions.Cancel),
				new CodeDescriptionPair(TRMessageStatusCodeList.Codes.CLR, TRMessageStatusCodeList.Descriptions.CLR),
				new CodeDescriptionPair(TRMessageStatusCodeList.Codes.Error, TRMessageStatusCodeList.Descriptions.Error),
				new CodeDescriptionPair(TRMessageStatusCodeList.Codes.NotSent, TRMessageStatusCodeList.Descriptions.NotSent),
				new CodeDescriptionPair(TRMessageStatusCodeList.Codes.REG, TRMessageStatusCodeList.Descriptions.REG),
				new CodeDescriptionPair(TRMessageStatusCodeList.Codes.REM, TRMessageStatusCodeList.Descriptions.REM),
				new CodeDescriptionPair(TRMessageStatusCodeList.Codes.Sent, TRMessageStatusCodeList.Descriptions.Sent),
				new CodeDescriptionPair(TRMessageStatusCodeList.Codes.TRG, TRMessageStatusCodeList.Descriptions.TRG),
				new CodeDescriptionPair(TRMessageStatusCodeList.Codes.Unknown, TRMessageStatusCodeList.Descriptions.Unknown),
				new CodeDescriptionPair(TRMessageStatusCodeList.Codes.Updated, TRMessageStatusCodeList.Descriptions.Updated),
			}, lookups.MessageStatusList);
		}
	}
}
