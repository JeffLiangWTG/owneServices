using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.MultiLineAddInfos
{
	public class UnknownCusAddInfo : CusAddInfo
	{
		public UnknownCusAddInfo(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
