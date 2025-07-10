using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business.MessageBuilders
{
	public static class MessageBuilderHelper
	{
		public static string UnlocoToIata(BusinessObjectFactory factory, ZString transportMode, ZString unloco)
		{
			return UnlocoToIata(factory, transportMode == Core.Constants.TransportModes.Air, unloco);
		}

		public static string UnlocoToIata(BusinessObjectFactory factory, ZBool isAir, ZString unloco)
		{
			var result = unloco;
			if (isAir)
			{
				var refUnloco = new RefUNLOCO.Loader(factory).Load(unloco);
				result = refUnloco?.RL_IATA ?? unloco.Right(3);
			}
			return result;
		}

		public static string GetMessageFunction_D16A(this MessageSubTypes subType)
		{
			Edifact.D16A.Elements.MessageFunctionCodeList result = null;
			switch (subType)
			{
				case MessageSubTypes.Amend:
				case MessageSubTypes.Change:
				case MessageSubTypes.Replace:
				case MessageSubTypes.ReplaceHeader:
				case MessageSubTypes.ReplaceLines:
				case MessageSubTypes.AddLines:
					result = Edifact.D16A.Elements.MessageFunctionCodeList.Change;
					break; //4
				case MessageSubTypes.Create:
					result = Edifact.D16A.Elements.MessageFunctionCodeList.Original;
					break; //9
				case MessageSubTypes.Withdraw:
					result = Edifact.D16A.Elements.MessageFunctionCodeList.Cancellation;
					break; // 1
			}
			return result?.ToString() ?? "";
		}

		public static string GetMessageFunction_D95B(this MessageSubTypes subType)
		{
			Edifact.D95B.Elements.MessageFunctionCodedList result = null;
			switch (subType)
			{
				case MessageSubTypes.Amend:
				case MessageSubTypes.Change:
				case MessageSubTypes.Replace:
				case MessageSubTypes.ReplaceHeader:
				case MessageSubTypes.ReplaceLines:
				case MessageSubTypes.AddLines:
					result = Enterprise.Edifact.D95B.Elements.MessageFunctionCodedList.Change;
					break; //4
				case MessageSubTypes.Create:
					result = Enterprise.Edifact.D95B.Elements.MessageFunctionCodedList.Original;
					break; //9
				case MessageSubTypes.Withdraw:
					result = Enterprise.Edifact.D95B.Elements.MessageFunctionCodedList.Cancellation;
					break; // 1
			}
			return result?.ToString() ?? "";
		}

		public static ZBool IsAmendment(this MessageSubTypes subType) => subType == MessageSubTypes.Amend
			|| subType == MessageSubTypes.Change
			|| subType == MessageSubTypes.Replace
			|| subType == MessageSubTypes.ReplaceHeader
			|| subType == MessageSubTypes.ReplaceLines;

		public static ZBool IsCancellation(this MessageSubTypes subType) => subType == MessageSubTypes.Withdraw;

		public static ZBool IsAmendmentOrCancellation(this MessageSubTypes subType) => IsAmendment(subType) || IsCancellation(subType);
	}
}
