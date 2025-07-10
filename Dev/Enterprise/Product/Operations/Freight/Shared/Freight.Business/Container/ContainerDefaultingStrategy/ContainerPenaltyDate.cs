using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public record ContainerPenaltyDate(ZDateTime Date, ZString Name)
	{
		public static ContainerPenaltyDate Empty => new ContainerPenaltyDate(ZDateTime.Empty, ZString.Empty);

		public bool IsEmpty() => this == ContainerPenaltyDate.Empty;
	}
}
