using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusEntryNumCollection : BusinessObjectCollection<CusEntryNumber>
	{
		public CusEntryNumCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public override void Load(ZQuery alternativeAdditionalFilter)
		{
			if (alternativeAdditionalFilter.IsEmpty)
			{
				alternativeAdditionalFilter = new ZQuery(CusEntryNumSchema.PK, ZGuid.Missing);
			}
			base.Load(alternativeAdditionalFilter);
		}
	}
}
