using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class ViewCommissionAgreementOverallItem : AutoViewCommissionAgreementOverallItem
	{
		public ViewCommissionAgreementOverallItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		#region VCI_CA0

		public virtual OrgCommissionAgreement CommissionAgreement
		{
			get { return Factory.Load<OrgCommissionAgreement>(VCI_CA0); }
		}

		#endregion

		#endregion
	}
}
