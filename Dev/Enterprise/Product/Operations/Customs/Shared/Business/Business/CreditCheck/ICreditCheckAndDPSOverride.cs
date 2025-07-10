using CargoWise.Types;
using Enterprise.MasterFiles.CreditControl.Business;

namespace Enterprise.Customs.Business
{
	public interface ICreditCheckAndDPSOverride
	{
		void ProcessOverride(SecurityLoginEventArgsForDocumentApproval args, ICreditControlledDocumentDelivery bizObj, ZString caption);
	}
}
