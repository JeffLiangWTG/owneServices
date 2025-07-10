using System;

namespace Enterprise.MasterFiles.Business.Accounting.GlobalEInvoicingRegistration.SaudiArabia
{
	public class SARegistrationEventArgs : EventArgs
	{
		public string BinarySecurityToken { get; set; }
		public string Secret { get; set; }
		public string RequestId { get; set; }
	}
}
