using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class CusEUEntryHeader : EU.Business.Declaration.CusEUEntryHeader, Integration.Customs.TR.ICusEUEntryHeader
	{
		public CusEUEntryHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
