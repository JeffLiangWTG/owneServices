using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Business
{
	public class CargoReportAcceptedExceptionValidation : ExceptionValidation
	{
		public CargoReportAcceptedExceptionValidation(ForwardingShipmentProcessTask parent)
			: base(parent)
		{
			this.parent = parent;
		}
		readonly ForwardingShipmentProcessTask parent;

		public void ValidateLateCargoReportReason()
		{
			ValidateCalculatedProperty(parent.LateCargoReportReasonInfo);
		}

		protected void CheckLateCargoReportReason()
		{
			MandatoryValidation.CheckEntered(parent.LateCargoReportReasonInfo);
			ListValidation.ErrorIfInvalidCode(parent.LateCargoReportReasonInfo, parent.LateCargoReportingReasonCodes);
		}

		public void ValidateLateCargoReportText()
		{
			ValidateCalculatedProperty(parent.LateCargoReportTextInfo);
		}

		protected void CheckLateCargoReportText()
		{
			if (parent.LateCargoReportingReasonCodes.ContainsCode(parent.LateCargoReportReason) && (parent.LateCargoReportingReasonCodes.GetDescriptionFromCode(parent.LateCargoReportReason)).Contains((NoResString)"additional reason required")) // Hard-coded constant
			{
				MandatoryValidation.CheckEntered(parent.LateCargoReportTextInfo);
			}
		}

		public void ValidateLateCargoReport()
		{
			ValidateLateCargoReportReason();
			ValidateLateCargoReportText();
		}
	}
}
