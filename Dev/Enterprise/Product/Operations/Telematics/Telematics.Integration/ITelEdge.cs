using CargoWise.Types;

namespace Enterprise.Telematics.Integration
{
	public interface ITelEdge
	{
		ZString TE_EntityTableCodeFrom { get; set; }
		ZGuid TE_EntityIdFrom { get; set; }
		ZString TE_EntityTableCodeTo { get; set; }
		ZGuid TE_EntityIdTo { get; set; }
		ZByte TE_TemplateMapping { get; set; }
		ZDateTimeOffset TE_StartTime { get; set; }
		ZDateTimeOffset TE_EndTime { get; set; }
	}
}
