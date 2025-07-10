using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.InBond.Business
{
	public class CusInBondHeaderCollection : ActiveBusinessObjectCollection<CusInBondHeader>
	{
		public CusInBondHeaderCollection(BusinessObjectFactory factory)
			: this(factory, null)
		{
		}

		public CusInBondHeaderCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, GetApplicationFilter(filter))
		{
		}

		static ZQuery GetApplicationFilter(ZQuery filter)
		{
			var result = new ZQuery(CusInBondHeaderSchema.BH_ApplicationCode, CusInBondApplicationCodeList.Codes.InBond);
			if (filter != null)
			{
				result.AddToFilter(filter);
			}
			return result;
		}
	}
}
