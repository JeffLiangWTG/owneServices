using CargoWise.Types;
using Enterprise.Customs.Common.US;

namespace Enterprise.Customs.US.Business
{
	partial class CusEntryHeaderMessageTypeList
	{
		public static ImportMessageStatusList.MessageType GetMessagesTypesRightFor(ZString cH_MessageType)
		{
			switch (cH_MessageType)
			{
				case Codes.BorderCargoRelease:
					return ImportMessageStatusList.MessageType.BorderCargoRelease;
				case Codes.Export:
					return ImportMessageStatusList.MessageType.Export;
				case Codes.CargoRelease:
					return ImportMessageStatusList.MessageType.CargoRelease;
				case Codes.EntrySummary:
					return ImportMessageStatusList.MessageType.EntrySummary;
				case Codes.InBond:
					return ImportMessageStatusList.MessageType.InBondDeparture;
				case Codes.ReconEntry:
					return ImportMessageStatusList.MessageType.ReconEntry;
				case Codes.ACECargoRelease:
					return ImportMessageStatusList.MessageType.ACECargoRelease;
				default:
					return ImportMessageStatusList.MessageType.Undefined;
			}
		}
	}
}
