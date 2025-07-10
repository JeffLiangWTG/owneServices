using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public class CusPackableItem : Customs.Business.CusPackableItem, Integration.Customs.TW.ICusPackableItem
	{
		public CusPackableItem(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
