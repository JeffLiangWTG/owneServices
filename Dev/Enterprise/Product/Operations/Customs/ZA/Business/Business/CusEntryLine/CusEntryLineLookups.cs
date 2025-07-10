using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business
{
	public class CusEntryLineLookups : Customs.Business.CusEntryLineLookups
	{
		public CusEntryLineLookups(AutoZACusEntryLine entryLine)
			: base(entryLine)
		{
		}

		public QuantityCodeList QuantityUnitCodeList
		{
			get { return Factory.GetCachedValue<QuantityCodeList>(); }
		}

		public CountableQuantityCodeList CountableUnitCodeList
		{
			get { return Factory.GetCachedValue<CountableQuantityCodeList>(); }
		}

		public RefCountryCollection CountryOfOrigins
		{
			get { return new RefCountryCollection(Factory); }
		}
	}
}
