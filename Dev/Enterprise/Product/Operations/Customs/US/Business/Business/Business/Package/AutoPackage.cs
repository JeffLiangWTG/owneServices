using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public abstract class AutoPackage : Customs.Business.BasePackage
	{
		protected AutoPackage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
