using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting
{
	public class CusEntryHeaderCollection : BusinessObjectCollection<CusEntryHeader>
	{
		public CusEntryHeaderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			return new ZQuery(CusEntryHeaderSchema.CH_MessageType, CusEntryHeader.EntryHeaderTypes.NZ.ECIWriteOffManifest);
		}
	}
}
