using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class MessageSendingActionLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestMessageTypeList_Cached()
	{
		var header = Factory.New<NctsHeader>();
		header.SetMovementType("D");
		var movementHeader = header.MovementHeader;
		movementHeader.BM_SubApplicationCode = "D";
		movementHeader.BM_AdditionalDeclarationType = "A";
		movementHeader.BM_CustomsStatus = string.Empty;
		movementHeader.BM_Phase = string.Empty;
		var messageSendingAction = new MessageSendingAction(movementHeader);
		var lookups = messageSendingAction.Lookups;
		var messageTypeList = lookups.MessageTypeList;

		AssertSame(messageTypeList, lookups.MessageTypeList);
	}

	public void TestMessageTypeList()
	{
		var subApplicationCodes = new NctsMoveHeaderType().GetAllCodes();

		var additionalDeclarationTypes = new NctsTypeOfAdditionalDeclarationList().GetAllCodes().ToList();
		additionalDeclarationTypes.Add(string.Empty);
		var customsStatuses = new NctsTransitStatusList().GetAllCodes().ToList();
		customsStatuses.Add(string.Empty);
		customsStatuses.AddRange(new NCTS5DepartureCustomsStatusList().GetAllCodes());
		customsStatuses.AddRange(new NCTS5ArrivalCustomsStatusList().GetAllCodes());

		var phases = new NctsMovementHeaderTransactionStatusList().GetAllCodes().ToList();
		phases.Add(string.Empty);

		var messageStatusses = new LogicalStatusList().GetAllCodes().ToList();
		messageStatusses.Add(string.Empty);
		var header = Factory.New<NctsHeader>();
		header.SetMovementType("D");
		var movementHeader = header.MovementHeader;
		var messageSendingAction = new MessageSendingAction(movementHeader);
		var lookups = messageSendingAction.Lookups;

		CombineAssertions(() =>
		{
			foreach (var subApplicationCode in subApplicationCodes)
			{
				movementHeader.BM_SubApplicationCode = subApplicationCode;
				foreach (var additionalDeclarationType in additionalDeclarationTypes.Distinct().ToList())
				{
					movementHeader.BM_AdditionalDeclarationType = additionalDeclarationType;
					foreach (var customsStatus in customsStatuses.Distinct().ToList())
					{
						movementHeader.BM_CustomsStatus = customsStatus;
						foreach (var phase in phases.Distinct().ToList())
						{
							movementHeader.BM_Phase = phase;
							foreach (var messageStatus in messageStatusses.Distinct().ToList())
							{
								movementHeader.BM_MessageStatus = messageStatus;
								header.BH_MessageStatus = messageStatus;
								movementHeader.Header.EffectiveMessageStatus = messageStatus;
								var expected = GetMessageTypesExpected(subApplicationCode, additionalDeclarationType, customsStatus, phase, messageStatus);
								var actual = lookups.MessageTypeList.CodesAsString;
								AssertEquals($"subApplicationCode '{subApplicationCode}', additionalDeclarationType '{additionalDeclarationType}', customsStatus '{customsStatus}, phase '{phase}', messageStatus '{messageStatus}'", expected, actual);
							}
						}
					}
				}
			}
		});
	}

	string GetMessageTypesExpected(ZString subApplicationCode, ZString additionalDeclarationType, ZString customsStatus, ZString phase, ZString messageStatus)
	{
		return messageTypesExpected.TryGetValue((subApplicationCode, additionalDeclarationType, customsStatus, phase, messageStatus), out var messageTypes) ? messageTypes : string.Empty;
	}

	readonly Dictionary<(string subApplicationCode, string additionalDeclarationType, string customsStatus, string phase, string messageStatus), string> messageTypesExpected =
		new Dictionary<(string subApplicationCode, string additionalDeclarationType, string customsStatus, string phase, string messageStatus), string>()
		{
			{ ("D", "A", "", "", ""), "DEC" },
			{ ("D", "A", "", "", "ACC"), "DEC" },
			{ ("D", "A", "", "", "ACK"), "DEC" },
			{ ("D", "A", "", "", "ERR"), "DEC" },
			{ ("D", "A", "", "", "FAL"), "DEC" },
			{ ("D", "A", "", "", "INV"), "DEC" },
			{ ("D", "A", "", "", "SNT"), "DEC" },
			{ ("D", "A", "", "015", ""), "DEC" },
			{ ("D", "A", "", "015", "ACC"), "DEC" },
			{ ("D", "A", "", "015", "ACK"), "DEC" },
			{ ("D", "A", "", "015", "ERR"), "DEC" },
			{ ("D", "A", "", "015", "FAL"), "DEC" },
			{ ("D", "A", "", "015", "INV"), "DEC" },
			{ ("D", "A", "", "015", "SNT"), "DEC" },
			{ ("D", "D", "", "", ""), "DEC" },
			{ ("D", "D", "", "", "ACC"), "DEC" },
			{ ("D", "D", "", "", "ACK"), "DEC" },
			{ ("D", "D", "", "", "ERR"), "DEC" },
			{ ("D", "D", "", "", "FAL"), "DEC" },
			{ ("D", "D", "", "", "INV"), "DEC" },
			{ ("D", "D", "", "", "SNT"), "DEC" },
			{ ("D", "D", "", "015", ""), "DEC" },
			{ ("D", "D", "", "015", "ACC"), "DEC" },
			{ ("D", "D", "", "015", "ACK"), "DEC" },
			{ ("D", "D", "", "015", "ERR"), "DEC" },
			{ ("D", "D", "", "015", "FAL"), "DEC" },
			{ ("D", "D", "", "015", "INV"), "DEC" },
			{ ("D", "D", "", "015", "SNT"), "DEC" },
			{ ("D", "D", "PRE", "013", ""), "AMD, INV, PRN" },
			{ ("D", "D", "PRE", "013", "ACC"), "AMD, INV, PRN" },
			{ ("D", "D", "PRE", "013", "ACK"), "AMD, INV, PRN" },
			{ ("D", "D", "PRE", "013", "ERR"), "AMD, INV, PRN" },
			{ ("D", "D", "PRE", "013", "FAL"), "AMD, INV, PRN" },
			{ ("D", "D", "PRE", "013", "INV"), "AMD, INV, PRN" },
			{ ("D", "D", "PRE", "013", "SNT"), "AMD, INV, PRN" },
			{ ("D", "D", "PRE", "014", ""), "AMD, INV, PRN" },
			{ ("D", "D", "PRE", "014", "ACC"), "AMD, INV, PRN" },
			{ ("D", "D", "PRE", "014", "ACK"), "AMD, INV, PRN" },
			{ ("D", "D", "PRE", "014", "ERR"), "AMD, INV, PRN" },
			{ ("D", "D", "PRE", "014", "FAL"), "AMD, INV, PRN" },
			{ ("D", "D", "PRE", "014", "INV"), "AMD, INV, PRN" },
			{ ("D", "D", "PRE", "014", "SNT"), "AMD, INV, PRN" },
			{ ("D", "D", "PRE", "015", ""), "AMD, INV, PRN" },
			{ ("D", "D", "PRE", "015", "ACC"), "AMD, INV, PRN" },
			{ ("D", "D", "PRE", "015", "ACK"), "AMD, INV, PRN" },
			{ ("D", "D", "PRE", "015", "ERR"), "AMD, INV, PRN" },
			{ ("D", "D", "PRE", "015", "FAL"), "AMD, INV, PRN" },
			{ ("D", "D", "PRE", "015", "INV"), "AMD, INV, PRN" },
			{ ("D", "D", "PRE", "015", "SNT"), "AMD, INV, PRN" },
			{ ("D", "D", "PRE", "170", "ERR"), "AMD, INV, PRN" },
			{ ("D", "D", "PRE", "170", "FAL"), "AMD, INV, PRN" },
			{ ("D", "D", "PRE", "170", "INV"), "AMD, INV, PRN" },
			{ ("D", "A", "ACK", "013", ""), "AMD, INV" },
			{ ("D", "A", "ACK", "013", "ACC"), "AMD, INV" },
			{ ("D", "A", "ACK", "013", "ACK"), "AMD, INV" },
			{ ("D", "A", "ACK", "013", "ERR"), "AMD, INV" },
			{ ("D", "A", "ACK", "013", "FAL"), "AMD, INV" },
			{ ("D", "A", "ACK", "013", "INV"), "AMD, INV" },
			{ ("D", "A", "ACK", "013", "SNT"), "AMD, INV" },
			{ ("D", "A", "ACK", "014", ""), "AMD, INV" },
			{ ("D", "A", "ACK", "014", "ACC"), "AMD, INV" },
			{ ("D", "A", "ACK", "014", "ACK"), "AMD, INV" },
			{ ("D", "A", "ACK", "014", "ERR"), "AMD, INV" },
			{ ("D", "A", "ACK", "014", "FAL"), "AMD, INV" },
			{ ("D", "A", "ACK", "014", "INV"), "AMD, INV" },
			{ ("D", "A", "ACK", "014", "SNT"), "AMD, INV" },
			{ ("D", "A", "ACK", "015", ""), "AMD, INV" },
			{ ("D", "A", "ACK", "015", "ACC"), "AMD, INV" },
			{ ("D", "A", "ACK", "015", "ACK"), "AMD, INV" },
			{ ("D", "A", "ACK", "015", "ERR"), "AMD, INV" },
			{ ("D", "A", "ACK", "015", "FAL"), "AMD, INV" },
			{ ("D", "A", "ACK", "015", "INV"), "AMD, INV" },
			{ ("D", "A", "ACK", "015", "SNT"), "AMD, INV" },
			{ ("D", "A", "MRN", "013", ""), "AMD, INV" },
			{ ("D", "A", "MRN", "013", "ACC"), "AMD, INV" },
			{ ("D", "A", "MRN", "013", "ACK"), "AMD, INV" },
			{ ("D", "A", "MRN", "013", "ERR"), "AMD, INV" },
			{ ("D", "A", "MRN", "013", "FAL"), "AMD, INV" },
			{ ("D", "A", "MRN", "013", "INV"), "AMD, INV" },
			{ ("D", "A", "MRN", "013", "SNT"), "AMD, INV" },
			{ ("D", "A", "MRN", "014", ""), "AMD, INV" },
			{ ("D", "A", "MRN", "014", "ACC"), "AMD, INV" },
			{ ("D", "A", "MRN", "014", "ACK"), "AMD, INV" },
			{ ("D", "A", "MRN", "014", "ERR"), "AMD, INV" },
			{ ("D", "A", "MRN", "014", "FAL"), "AMD, INV" },
			{ ("D", "A", "MRN", "014", "INV"), "AMD, INV" },
			{ ("D", "A", "MRN", "014", "SNT"), "AMD, INV" },
			{ ("D", "A", "MRN", "015", ""), "AMD, INV" },
			{ ("D", "A", "MRN", "015", "ACC"), "AMD, INV" },
			{ ("D", "A", "MRN", "015", "ACK"), "AMD, INV" },
			{ ("D", "A", "MRN", "015", "ERR"), "AMD, INV" },
			{ ("D", "A", "MRN", "015", "FAL"), "AMD, INV" },
			{ ("D", "A", "MRN", "015", "INV"), "AMD, INV" },
			{ ("D", "A", "MRN", "015", "SNT"), "AMD, INV" },
			{ ("D", "A", "GIV", "013", ""), "AMD" },
			{ ("D", "A", "GIV", "013", "ACC"), "AMD" },
			{ ("D", "A", "GIV", "013", "ACK"), "AMD" },
			{ ("D", "A", "GIV", "013", "ERR"), "AMD" },
			{ ("D", "A", "GIV", "013", "FAL"), "AMD" },
			{ ("D", "A", "GIV", "013", "INV"), "AMD" },
			{ ("D", "A", "GIV", "013", "SNT"), "AMD" },
			{ ("D", "A", "GIV", "015", ""), "AMD" },
			{ ("D", "A", "GIV", "015", "ACC"), "AMD" },
			{ ("D", "A", "GIV", "015", "ACK"), "AMD" },
			{ ("D", "A", "GIV", "015", "ERR"), "AMD" },
			{ ("D", "A", "GIV", "015", "FAL"), "AMD" },
			{ ("D", "A", "GIV", "015", "INV"), "AMD" },
			{ ("D", "A", "GIV", "015", "SNT"), "AMD" },
			{ ("D", "A", "AMR", "013", ""), "AMD" },
			{ ("D", "A", "AMR", "013", "ACC"), "AMD" },
			{ ("D", "A", "AMR", "013", "ACK"), "AMD" },
			{ ("D", "A", "AMR", "013", "ERR"), "AMD" },
			{ ("D", "A", "AMR", "013", "FAL"), "AMD" },
			{ ("D", "A", "AMR", "013", "INV"), "AMD" },
			{ ("D", "A", "AMR", "013", "SNT"), "AMD" },
			{ ("D", "A", "AMR", "015", ""), "AMD" },
			{ ("D", "A", "AMR", "015", "ACC"), "AMD" },
			{ ("D", "A", "AMR", "015", "ACK"), "AMD" },
			{ ("D", "A", "AMR", "015", "ERR"), "AMD" },
			{ ("D", "A", "AMR", "015", "FAL"), "AMD" },
			{ ("D", "A", "AMR", "015", "INV"), "AMD" },
			{ ("D", "A", "AMR", "015", "SNT"), "AMD" },
			{ ("D", "A", "NRL", "015", ""), "INV" },
			{ ("D", "A", "NRL", "015", "ACC"), "INV" },
			{ ("D", "A", "NRL", "015", "ACK"), "INV" },
			{ ("D", "A", "NRL", "015", "ERR"), "INV" },
			{ ("D", "A", "NRL", "015", "FAL"), "INV" },
			{ ("D", "A", "NRL", "015", "INV"), "INV" },
			{ ("D", "A", "NRL", "015", "SNT"), "INV" },
			{ ("D", "A", "REL", "015", ""), "INV" },
			{ ("D", "A", "REL", "015", "ACC"), "INV" },
			{ ("D", "A", "REL", "015", "ACK"), "INV" },
			{ ("D", "A", "REL", "015", "ERR"), "INV" },
			{ ("D", "A", "REL", "015", "FAL"), "INV" },
			{ ("D", "A", "REL", "015", "INV"), "INV" },
			{ ("D", "A", "REL", "015", "SNT"), "INV" },
			{ ("D", "A", "ENQ", "015", ""), "RNM" },
			{ ("D", "A", "ENQ", "015", "ACC"), "RNM" },
			{ ("D", "A", "ENQ", "015", "ACK"), "RNM" },
			{ ("D", "A", "ENQ", "015", "ERR"), "RNM" },
			{ ("D", "A", "ENQ", "015", "FAL"), "RNM" },
			{ ("D", "A", "ENQ", "015", "INV"), "RNM" },
			{ ("D", "A", "ENQ", "015", "SNT"), "RNM" },
			{ ("D", "A", "ENQ", "141", ""), "RNM" },
			{ ("D", "A", "ENQ", "141", "ACC"), "RNM" },
			{ ("D", "A", "ENQ", "141", "ACK"), "RNM" },
			{ ("D", "A", "ENQ", "141", "ERR"), "RNM" },
			{ ("D", "A", "ENQ", "141", "FAL"), "RNM" },
			{ ("D", "A", "ENQ", "141", "INV"), "RNM" },
			{ ("D", "A", "ENQ", "141", "SNT"), "RNM" },
			{ ("D", "A", "DIS", "015", ""), "RRL" },
			{ ("D", "A", "DIS", "015", "ACC"), "RRL" },
			{ ("D", "A", "DIS", "015", "ACK"), "RRL" },
			{ ("D", "A", "DIS", "015", "ERR"), "RRL" },
			{ ("D", "A", "DIS", "015", "FAL"), "RRL" },
			{ ("D", "A", "DIS", "015", "INV"), "RRL" },
			{ ("D", "A", "DIS", "015", "SNT"), "RRL" },
			{ ("D", "A", "CO0", "015", ""), "RRL" },
			{ ("D", "A", "CO0", "015", "ACC"), "RRL" },
			{ ("D", "A", "CO0", "015", "ACK"), "RRL" },
			{ ("D", "A", "CO0", "015", "ERR"), "RRL" },
			{ ("D", "A", "CO0", "015", "FAL"), "RRL" },
			{ ("D", "A", "CO0", "015", "INV"), "RRL" },
			{ ("D", "A", "CO0", "015", "SNT"), "RRL" },
			{ ("D", "A", "CO1", "015", ""), "RRL" },
			{ ("D", "A", "CO1", "015", "ACC"), "RRL" },
			{ ("D", "A", "CO1", "015", "ACK"), "RRL" },
			{ ("D", "A", "CO1", "015", "ERR"), "RRL" },
			{ ("D", "A", "CO1", "015", "FAL"), "RRL" },
			{ ("D", "A", "CO1", "015", "INV"), "RRL" },
			{ ("D", "A", "CO1", "015", "SNT"), "RRL" },
			{ ("D", "A", "CO2", "015", ""), "RRL" },
			{ ("D", "A", "CO2", "015", "ACC"), "RRL" },
			{ ("D", "A", "CO2", "015", "ACK"), "RRL" },
			{ ("D", "A", "CO2", "015", "ERR"), "RRL" },
			{ ("D", "A", "CO2", "015", "FAL"), "RRL" },
			{ ("D", "A", "CO2", "015", "INV"), "RRL" },
			{ ("D", "A", "CO2", "015", "SNT"), "RRL" },
			{ ("A", "", "", "", ""), "ARN" },
			{ ("A", "", "", "", "ACC"), "ARN" },
			{ ("A", "", "", "", "ACK"), "ARN" },
			{ ("A", "", "", "", "ERR"), "ARN" },
			{ ("A", "", "", "", "FAL"), "ARN" },
			{ ("A", "", "", "", "INV"), "ARN" },
			{ ("A", "", "", "", "SNT"), "ARN" },
			{ ("A", "", "", "007", ""), "ARN" },
			{ ("A", "", "", "007", "ACC"), "ARN" },
			{ ("A", "", "", "007", "ACK"), "ARN" },
			{ ("A", "", "", "007", "ERR"), "ARN" },
			{ ("A", "", "", "007", "FAL"), "ARN" },
			{ ("A", "", "", "007", "INV"), "ARN" },
			{ ("A", "", "", "007", "SNT"), "ARN" },
			{ ("A", "", "UAP", "007", ""), "URM" },
			{ ("A", "", "UAP", "007", "ACC"), "URM" },
			{ ("A", "", "UAP", "007", "ACK"), "URM" },
			{ ("A", "", "UAP", "007", "ERR"), "URM" },
			{ ("A", "", "UAP", "007", "FAL"), "URM" },
			{ ("A", "", "UAP", "007", "INV"), "URM" },
			{ ("A", "", "UAP", "007", "SNT"), "URM" },
			{ ("A", "", "UAP", "043", ""), "URM" },
			{ ("A", "", "UAP", "043", "ACC"), "URM" },
			{ ("A", "", "UAP", "043", "ACK"), "URM" },
			{ ("A", "", "UAP", "043", "ERR"), "URM" },
			{ ("A", "", "UAP", "043", "FAL"), "URM" },
			{ ("A", "", "UAP", "043", "INV"), "URM" },
			{ ("A", "", "UAP", "043", "SNT"), "URM" },
			{ ("A", "", "UAP", "044", ""), "URM" },
			{ ("A", "", "UAP", "044", "ACC"), "URM" },
			{ ("A", "", "UAP", "044", "ACK"), "URM" },
			{ ("A", "", "UAP", "044", "ERR"), "URM" },
			{ ("A", "", "UAP", "044", "FAL"), "URM" },
			{ ("A", "", "UAP", "044", "INV"), "URM" },
			{ ("A", "", "UAP", "044", "SNT"), "URM" },
			{ ("A", "", "ULR", "043", ""), "URM" },
			{ ("A", "", "ULR", "043", "ACC"), "URM" },
			{ ("A", "", "ULR", "043", "ACK"), "URM" },
			{ ("A", "", "ULR", "043", "ERR"), "URM" },
			{ ("A", "", "ULR", "043", "FAL"), "URM" },
			{ ("A", "", "ULR", "043", "INV"), "URM" },
			{ ("A", "", "ULR", "043", "SNT"), "URM" },
			{ ("A", "", "ULR", "044", ""), "URM" },
			{ ("A", "", "ULR", "044", "ACC"), "URM" },
			{ ("A", "", "ULR", "044", "ACK"), "URM" },
			{ ("A", "", "ULR", "044", "ERR"), "URM" },
			{ ("A", "", "ULR", "044", "FAL"), "URM" },
			{ ("A", "", "ULR", "044", "INV"), "URM" },
			{ ("A", "", "ULR", "044", "SNT"), "URM" },
		};
}
