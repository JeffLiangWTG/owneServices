using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class CusEntryLineLookups : Customs.Business.CusEntryLineLookups
	{
		public CusEntryLineLookups(CusEntryLine parent)
			: base(parent)
		{
		}

		public CusEntryLine EntryLine => Parent;

		protected new CusEntryLine Parent => (CusEntryLine)base.Parent;

		IRefCusPackListProvider CachedCusPackListProvider => RefCusPackListProvider.GetCachedCusPackListProvider(Factory);

		public CodeDescriptionPairList InvoiceUQList => CachedCusPackListProvider.GetCommercialPackList(Factory, ZString.Empty);
	}
}
