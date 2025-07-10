using CargoWise.EntityFramework;

namespace Enterprise.Customs.NO.Business
{
	public class CusEntryHeaderCollection<TCusEntryHeader> : Customs.Business.CusEntryHeaderCollection<TCusEntryHeader>
		where TCusEntryHeader : CusEntryHeader
	{
		public CusEntryHeaderCollection(JobDeclaration parentBO, BusinessObjectFactory factory)
			: base(parentBO, factory)
		{
		}

		protected override bool AllowNewCore => true;
	}
}
