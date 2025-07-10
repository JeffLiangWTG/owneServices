using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Universal
{
	public class RefSysConfigType : AutoRefSysConfigType, Integration.Customs.Shared.IRefSysConfigType
	{
		public RefSysConfigType(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
