using CargoWise.Types;
namespace Enterprise.Packing.Business
{
	public interface IOutturnProvider
	{
		int OutturnQty { get; }
		int OutturnDamagedQty { get; }
		int OutturnPillagedQty { get; }
		decimal OutturnedHeight { get; }
		decimal OutturnedLength { get; }
		decimal OutturnedVolume { get; }
		decimal OutturnedWeight { get; }
		decimal OutturnedWidth { get; }
		string ActualTransportJobID { get; }
		string ActualTransportJobTypeCode { get; }
		string ActualTransportJobTypeDescription { get; }
		string ExpectedTransportJobID { get; }
		string ExpectedTransportJobTypeCode { get; }
		string ExpectedTransportJobTypeDescription { get; }
		ZDateTime? UnloadDate { get; }
		ZDateTime? LoadDate { get; }
		bool IsHighRisk { get; }
		ZString OverriddenAviationSecurityInspectionType { get; }
	}
}
