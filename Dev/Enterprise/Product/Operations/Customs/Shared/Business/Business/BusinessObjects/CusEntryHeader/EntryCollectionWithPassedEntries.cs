using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class EntryCollectionWithPassedEntries : BusinessObjectCollection<CusEntryHeader>
	{
		public EntryCollectionWithPassedEntries(CusEntryHeader[] entries, BusinessObjectFactory factory) : base(factory)
		{
			AddRange(entries);
		}
		public override void Load(ZQuery alternativeAdditionalFilter)
		{
			throw new NotSupportedException();
		}
	}
}
