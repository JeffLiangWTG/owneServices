using System.Linq;
using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.ACE;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common
{
	public partial class AENS52 : Abstract.AENS52, IACEBIRDLineRecord
	{
		#region IACEBIRDLineRecord Members

		void IACEBIRDLineRecord.Update(JobComInvoiceLine invoiceLine, INotifications notifications)
		{
			if (!invoiceLine.LicenceAndPermits.OfType<LicenceAndPermit>().Any(x => x.CY_Code == LicenseCertificatePermitTypeCode && x.CY_Data == LicenseNumberCertificateNumberPermitNumber))
			{
				invoiceLine.LicenceAndPermits.AddNew(LicenseCertificatePermitTypeCode, LicenseNumberCertificateNumberPermitNumber);
			}
		}

		#endregion
	}
}
