using System;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubTransactions
{
    public class eHubClientRegistration
    {
		public eHubClientRegistration()
        {
			CX_PK = Guid.NewGuid();
        }

		public Guid CX_PK { get; set; }
		public Guid CX_CC { get; set; }
		public Guid CX_RT { get; set; }
		public string CX_Code { get; set; }
		public string CX_Qualifier { get; set; }
		public string CX_Attr1 { get; set; }
		public string CX_Password1 { get; set; }
		public byte? CX_Flag1 { get; set; }
		public byte? CX_Flag2 { get; set; }
		public byte[] CX_RV { get; set; }
		public virtual string CX_ConfigXml { get; set; }
		public DateTime? CX_IssuedUTC { get; set; }
		public DateTime? CX_ExpiryUTC { get; set; }
	}
}
