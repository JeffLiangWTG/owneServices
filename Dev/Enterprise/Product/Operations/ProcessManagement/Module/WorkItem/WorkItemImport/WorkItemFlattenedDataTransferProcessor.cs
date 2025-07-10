using CargoWise.Types;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.DataMapping;

namespace Enterprise.ProcessManagement.Module
{
	public class WorkItemFlattenedDataTransferProcessor : ValidationBasedDataTransferProcessor<WorkItemFlattened, WorkItemFlattenedCollectionInfo, WorkItem>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Validation message")]
		public WorkItemFlattenedDataTransferProcessor(WorkItemFlattenedCollectionInfo flattenedCollectionInfo)
			: base(flattenedCollectionInfo, new ValidationErrorToResultingErrorMapping[] {
				new ValidationErrorToResultingErrorMapping("WKI_WorkItemType: Enter a valid Work Item Type.", (flattenedRecord) => Res.GetString("29E7FDE0-7015-4B0F-B277-70A1AB9F2DD5", "Invalid work item type code '{0}'", flattenedRecord.WKI_WorkItemType)),
				new ValidationErrorToResultingErrorMapping("WKI_WorkItemArea: Enter a valid Work Item Area.", (flattenedRecord) => Res.GetString("AAEBC955-9506-433A-B997-AE03C49246AF", "Invalid work item area code '{0}'", flattenedRecord.WKI_WorkItemArea)),
				new ValidationErrorToResultingErrorMapping("WKI_ActivityType: Enter a valid Activity Type.", (flattenedRecord) => Res.GetString("C0318DC7-F213-429D-B041-43CA435E4448", "Invalid work item activity type code '{0}'", flattenedRecord.WKI_ActivityType)),
				new ValidationErrorToResultingErrorMapping("WKI_ActivitySubtype: Enter a valid Activity Sub Type.", (flattenedRecord) => Res.GetString("116C546F-E41C-49FD-ACF0-5CFB0F8F3A59", "Invalid work item activity subtype code '{0}'", flattenedRecord.WKI_ActivitySubtype)),
				new ValidationErrorToResultingErrorMapping("WKI_Priority: Enter a valid Priority.", (flattenedRecord) => Res.GetString("9594EC54-59EE-4759-BB31-77C2C9E733AA", "Invalid work item priority '{0}'", flattenedRecord.WKI_Priority)),
				new ValidationErrorToResultingErrorMapping("WKI_PortOrCountry: Enter a valid Country/Region/Port.", (flattenedRecord) => Res.GetString("16BD95E7-4669-4708-8F81-E652444855E6", "Invalid work item port or country/region '{0}'", flattenedRecord.WKI_PortOrCountry)),
				new ValidationErrorToResultingErrorMapping("WKI_Summary: Please enter a Summary.", (flattenedRecord) => Res.GetString("8AE40460-8C2E-4426-9E8F-9D65F08321F8", "Missing work item summary"))
			})
		{
		}

		protected override string ProgressMessage => Res.GetString("3CB6086A-0D77-43C3-AD54-E398ECEA4D00", "Importing work items");

		protected override WorkItem GetImportedItem(WorkItemFlattened flattenedRecord)
		{
			var workItem = Factory.New<WorkItem>();
			workItem.WKI_WorkItemType = flattenedRecord.WKI_WorkItemType;
			workItem.WKI_WorkItemArea = flattenedRecord.WKI_WorkItemArea;
			workItem.WKI_ActivityType = flattenedRecord.WKI_ActivityType;
			workItem.WKI_ActivitySubtype = flattenedRecord.WKI_ActivitySubtype;
			workItem.WKI_Priority = flattenedRecord.WKI_Priority;
			workItem.WKI_PortOrCountry = flattenedRecord.WKI_PortOrCountry;
			workItem.WKI_Summary = flattenedRecord.WKI_Summary.Trim();
			workItem.WKI_Details = ZBlob.FromUTF8(flattenedRecord.WKI_Details.Trim());
			return workItem;
		}

		protected override void ValidateImportedItem(WorkItem importedItem)
		{
			importedItem.Validation.ValidateAll();
		}
	}
}
