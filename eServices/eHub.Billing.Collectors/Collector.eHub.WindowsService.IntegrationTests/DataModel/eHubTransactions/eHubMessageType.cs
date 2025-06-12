using System;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubTransactions
{
	class eHubMessageType
	{
		public eHubMessageType(string code)
		{
			DT_PK = Guid.NewGuid();
			DT_Code = code;
		}

		public Guid DT_PK { get; set; }
		public string DT_Code { get; set; }
		public bool DT_IsFlatFile { get; set; }
		public bool DT_IsEDI { get; set; }
		public string DT_Charset { get; set; }
		public string DT_EnvelopeXpath { get; set; }
		public Guid DT_DT_InnerType { get; set; }
		public bool DT_ReprocessSubMessage { get; set; }
		public bool DT_PostAssembleMapping { get; set; }
		public Guid DT_DT_PostAssembleWrapper { get; set; }
		public bool DT_IsJson { get; set; }
	}
}
