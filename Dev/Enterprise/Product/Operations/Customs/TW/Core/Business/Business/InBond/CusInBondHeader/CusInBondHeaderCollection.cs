using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	public class CusInBondHeaderCollection : ActiveBusinessObjectCollection<CusInBondHeader>
	{
		public CusInBondHeaderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();
			result.AddToFilter(CusInBondHeaderSchema.BH_ApplicationCode, Common.CusInBondApplicationCodeList.Codes.TWTranshipment);
			return result;
		}
	}
}
