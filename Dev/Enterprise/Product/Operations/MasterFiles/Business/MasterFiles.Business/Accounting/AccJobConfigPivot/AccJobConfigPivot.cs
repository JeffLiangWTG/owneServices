using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public abstract class AccJobConfigPivot : AutoAccJobConfigPivot
	{
		protected AccJobConfigPivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
