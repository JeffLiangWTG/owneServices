using Enterprise.Customs.Common.US;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class CusEntryHeaderLookups : Customs.Business.CusEntryHeaderLookups
	{
		public CusEntryHeaderLookups(CusEntryHeader parent)
			: base(parent)
		{
		}

		public CusEntryHeader EntryHeader
		{
			get { return Parent; }
		}

		protected new CusEntryHeader Parent
		{
			get { return (CusEntryHeader)base.Parent; }
		}

		public override CodeDescriptionPairList MessageStatusList
		{
			get
			{
				return Factory.GetCachedValue<CodeDescriptionPairList>("MessageStatusList" + EntryHeader.CH_MessageType, delegate
				{
					if (EntryHeader.IsExport)
					{
						return new AESDirectCustomsEntryStatus();
					}
					else if (EntryHeader.IsReconEntry)
					{
						return new ReconMessageStatusList();
					}
					else
					{
						return new ImportMessageStatusList();
					}
				});
			}
		}

		public override CodeDescriptionPairList CH_MessageTypeList
		{
			get { return Factory.GetCachedValue<CusEntryHeaderMessageTypeList>(); }
		}

		public EntryTypeList US_EntryTypeList
		{
			get { return Factory.GetCachedValue<EntryTypeList>(); }
		}

		public MonthList US_MonthList
		{
			get { return Factory.GetCachedValue<MonthList>(); }
		}
	}
}
