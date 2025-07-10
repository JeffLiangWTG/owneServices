namespace CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.Config
{
	public interface ICDSPortSource
	{
		string Name { get; }
		string Code { get; }
		string PageURL { get; }
		int CodeColumn { get; }
		int[] DescriptionColumns { get; }
		string AttributeName { get; }
		bool CheckCCSUKLocation { get; }
		int AdditionalInfoColumn { get; }
		bool UseODS { get; }
		string AnchorText { get; }
		string ODSDataTag { get; }
	}
}
