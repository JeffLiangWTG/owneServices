using CargoWise.Types;

namespace Enterprise.Customs.US.AMS.Messaging.Interface
{
	public interface ICargoDescription
	{
		ZString HarmonizedNumber { get; }
		ZInt Value { get; }
		ZInt Weight { get; }
		ZString WeightUnit { get; }
		ZDecimal PieceCount { get; }
		ZString Description { get; }
		ZString C4Number { get; }
		ZString ManifestUnitCode { get; }
		ZString CountryCode { get; }
		ZString MarksAndNumbers { get; }
	}
}
