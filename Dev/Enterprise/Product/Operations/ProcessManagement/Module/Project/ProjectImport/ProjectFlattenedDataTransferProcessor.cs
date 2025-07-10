using CargoWise.Types;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.DataMapping;

namespace Enterprise.ProcessManagement.Module
{
	public class ProjectFlattenedDataTransferProcessor : ValidationBasedDataTransferProcessor<ProjectFlattened, ProjectFlattenedCollectionInfo, Project>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Validation message")]
		public ProjectFlattenedDataTransferProcessor(ProjectFlattenedCollectionInfo flattenedCollectionInfo)
			: base(flattenedCollectionInfo, new ValidationErrorToResultingErrorMapping[] {
				new ValidationErrorToResultingErrorMapping("WKP_Type: Enter a valid selection.", (flattenedRecord) => Res.GetString("324C9C8C-CF8F-4419-BBCE-CC0BFDD5593F", "Invalid project type code '{0}'", flattenedRecord.WKP_Type)),
				new ValidationErrorToResultingErrorMapping("WKP_SubType: Enter a valid selection.", (flattenedRecord) => Res.GetString("3ACD387C-E391-4BD2-A8BE-7D7D8965AA05", "Invalid project subtype code '{0}'", flattenedRecord.WKP_SubType)),
				new ValidationErrorToResultingErrorMapping("WKP_Module: Enter a valid selection.", (flattenedRecord) => Res.GetString("FE1830C6-5D39-492D-AC3A-A4C753C8ADD5", "Invalid project module code '{0}'", flattenedRecord.WKP_Module)),
				new ValidationErrorToResultingErrorMapping("WKP_Priority: Enter a valid Priority.", (flattenedRecord) => Res.GetString("864C8AB4-C089-4954-B1D0-3A03C49E0D37", "Invalid project priority '{0}'", flattenedRecord.WKP_Priority)),
				new ValidationErrorToResultingErrorMapping("WKP_Summary: Please enter a Summary.", (flattenedRecord) => Res.GetString("16A5A2CE-3B58-4247-AE62-70E415197A61", "Missing project summary"))
			})
		{
		}

		protected override string ProgressMessage => Res.GetString("C30F5700-1C3B-4F86-BADA-94A455B0B617", "Importing projects");

		protected override Project GetImportedItem(ProjectFlattened flattenedRecord)
		{
			var project = Factory.New<Project>();
			project.WKP_Type = flattenedRecord.WKP_Type;
			project.WKP_SubType = flattenedRecord.WKP_SubType;
			project.WKP_Module = flattenedRecord.WKP_Module;
			project.WKP_Priority = flattenedRecord.WKP_Priority;
			project.WKP_Summary = flattenedRecord.WKP_Summary.Trim();
			project.WKP_Details = ZBlob.FromUTF8(flattenedRecord.WKP_Details.Trim());
			return project;
		}

		protected override void ValidateImportedItem(Project importedItem)
		{
			importedItem.Validation.ValidateAll();
		}
	}
}
