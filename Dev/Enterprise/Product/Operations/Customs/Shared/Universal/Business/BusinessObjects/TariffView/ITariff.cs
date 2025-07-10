using CargoWise.Types;

namespace Enterprise.Customs.Universal
{
	public interface ITariff
	{
		ZString Code { get; }
		ZString Description { get; }
		ZString UQ1 { get; }
		ZString UQ2 { get; }
		ZString UQ3 { get; }
		ZString UQ4 { get; }
		ZString UQ5 { get; }
	}
}
