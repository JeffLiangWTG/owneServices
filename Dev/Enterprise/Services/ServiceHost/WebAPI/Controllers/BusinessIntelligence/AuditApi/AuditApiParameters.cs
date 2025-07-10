namespace Enterprise.Services.ServiceHost.WebAPI.Controllers.BusinessIntelligence.Audit
{
	public abstract class AuditApiParameters
	{
		public FormatType Format { get; set; } = FormatType.JSON;
	}
}
