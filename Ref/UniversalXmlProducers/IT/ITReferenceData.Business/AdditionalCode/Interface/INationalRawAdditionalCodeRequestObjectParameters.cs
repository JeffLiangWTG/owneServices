namespace CargoWise.RefDbRepo.ITReferenceData.Business.AdditionalCode
{
	public interface INationalRawAdditionalCodeRequestObjectParameters
	{
		string UC { get; }
		string SC { get; }
		string ST { get; }
		string Label { get; }
		string AdditionalCodeSequentialNumber { get; }
		string AdditionalCodeType { get; }
		string ValidityStartDate { get; }
		string SidCad { get; }
	}
}
