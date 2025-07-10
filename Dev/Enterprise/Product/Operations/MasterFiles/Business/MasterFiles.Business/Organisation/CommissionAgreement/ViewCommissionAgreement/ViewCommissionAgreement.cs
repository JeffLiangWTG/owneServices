using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class ViewCommissionAgreement : AutoViewCommissionAgreement
	{
		public ViewCommissionAgreement(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
