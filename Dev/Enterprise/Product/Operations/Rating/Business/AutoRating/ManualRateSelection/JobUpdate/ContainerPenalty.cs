using CargoWise.Types;
using Enterprise.Integration.Freight;

namespace Enterprise.Rating.Business
{
	public class ContainerPenalty : IContainerPenalty
	{
		public ContainerPenalty(string containerType)
		{
			RefContainerCode = containerType;
		}

		public ZString RefContainerCode { get; }
		public ZString CPY_ProcessType { get; set; }
		public ZString CPY_PenaltyType { get; set; }
		public ZString CPY_CreditorType { get; set; }
		public ZDateTime CPY_FreeTime { get; set; }
		public ZString CPY_TimeUnit { get; set; }
		public ZDecimal CPY_PerUnitCost { get; set; }
		public ZString CPY_RX_NKCurrency { get; set; }
	}
}
