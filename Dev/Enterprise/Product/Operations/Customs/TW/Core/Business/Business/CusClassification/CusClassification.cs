using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public partial class CusClassification : Customs.Business.BaseCusClassification
	{
		public CusClassification(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
