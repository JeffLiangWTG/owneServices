using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs._CustomsTemplate_.Business
{
	public partial class Bill : Customs.Business.Bill, Integration.Customs._CustomsTemplate_.IBill
	{
		public Bill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
