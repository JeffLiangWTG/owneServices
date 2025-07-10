using System.Collections.Generic;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public class AR44TCodeListGenerator : ExcelDrivenCodeListDataFileGenerator
	{
		protected override IEnumerable<string> InputFileNames
		{
			get
			{
				yield return ApplicationConfig.Instance.PntsCodeListsFileName;
			}
		}

		protected override string DataGrouping => UniversalDataHelper.Constants.France;
		protected override string CodeType => "AR44T";
		protected override string SheetName => "CL380";
	}
}
