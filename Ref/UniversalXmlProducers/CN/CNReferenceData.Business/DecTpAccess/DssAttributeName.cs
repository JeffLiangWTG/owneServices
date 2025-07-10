namespace CargoWise.RefDbRepo.CNReferenceData.Business
{
	public class DssAttributeName
	{
		public string Name { get; set; }
		public string EnglishDescription { get; set; }
		public string ChineseDescription { get; set; }
		public string Caption { get; set; }
		public string ChineseCaption { get; set; }

		public bool IsCodeSuffix => Name == Constants.DecTpAccess.AttributeNames.CodeSuffix;
		public bool IsCustomsOffice => Name == Constants.DecTpAccess.AttributeNames.CustomsOffice;
	}
}
