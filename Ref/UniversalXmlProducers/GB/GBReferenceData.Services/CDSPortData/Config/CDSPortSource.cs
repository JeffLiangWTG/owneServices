namespace CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.Config
{
	public class CDSPortSource : ICDSPortSource
	{
		public string Name { get; set; }
		public string Code { get; set; }

		public string PageURL { get; set; }

		public int CodeColumn { get; set; }

		public int[] DescriptionColumns { get; set; }
		public string AttributeName { get; set; }
		public bool CheckCCSUKLocation { get; set; }
		public int AdditionalInfoColumn { get; set; } = -1;

		public bool UseODS { get; set; }
		public string AnchorText { get; set; } = string.Empty;
		public string ODSDataTag { get; set; } = string.Empty;
	}
}
