using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.AMS.Business
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
			var result = new ZQuery(CusInBondHeaderSchema.BH_ApplicationCode, CusInBondApplicationCodeList.Codes.AMS);
			if (filter != null)
			{
				result.AddToFilter(filter);
			}
			return result;
		}
	}
}
