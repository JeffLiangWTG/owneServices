using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.DE
{
	public class AdvancedLogisticsPortOrderLookups
	{
		public CodeDescriptionPairList GetEntryTypesList(EntryTypeStatus status = EntryTypeStatus.ALL)
		{
			switch (status)
			{
				case EntryTypeStatus.DefaultToAES:
					return new CodeDescriptionPairList
					{
						new CodeDescriptionPair(EntryTypes.Codes.EntryTypes_AES, EntryTypes.Descriptions.EntryTypes_AES),
						new CodeDescriptionPair(EntryTypes.Codes.EntryTypes_AE1, EntryTypes.Descriptions.EntryTypes_AE1)
					};
				case EntryTypeStatus.ReadOnly_DefaultToAES:
					return new CodeDescriptionPairList
					{
						new CodeDescriptionPair(EntryTypes.Codes.EntryTypes_AES, EntryTypes.Descriptions.EntryTypes_AES)
					};
				case EntryTypeStatus.ReadOnly_DefaultToAE1:
					return new CodeDescriptionPairList
					{
						new CodeDescriptionPair(EntryTypes.Codes.EntryTypes_AE1, EntryTypes.Descriptions.EntryTypes_AE1)
					};
				case EntryTypeStatus.ReadOnly_DefaultToNA:
					return new CodeDescriptionPairList
					{
						new CodeDescriptionPair(EntryTypes.Codes.EntryTypes_NA, EntryTypes.Descriptions.EntryTypes_NA)
					};
				default:
					return new EntryTypes();
			}
		}
	}
}
