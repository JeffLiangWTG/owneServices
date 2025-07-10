using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.Business.Business.JobComInvoiceHeader
{
	class JobComInvoiceHeaderExternalMessageValidation : ExternalMessageValidation
	{
		public JobComInvoiceHeaderExternalMessageValidation(BusinessObject businessObject) : base(businessObject)
		{
		}

		protected override void AddExchangeRateOutOfRangeError(ZPropertyInfo currInfo, ZString message)
		{
			currInfo.AddMessageError(message);
		}

		protected override INotificationType DateOfExportNotificationSeverity => CargoWise.EntityFramework.NotificationType.Warning;
	}
}
