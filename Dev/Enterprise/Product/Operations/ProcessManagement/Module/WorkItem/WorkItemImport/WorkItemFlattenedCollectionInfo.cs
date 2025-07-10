using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProcessManagement.Module
{
	public class WorkItemFlattenedCollectionInfo : ImportCollectionInfoImpl
	{
		public WorkItemFlattenedCollectionInfo(IBusinessObjectCollection collection)
			: base(collection)
		{
			AddProperty(WorkItemSchema.Constants.WKI_WorkItemType, Res.GetString("F5E1891D-8702-4063-9C58-6D7EDA1984B9", "Type"), ZCharacterCasing.Upper);
			AddProperty(WorkItemSchema.Constants.WKI_WorkItemArea, Res.GetString("127FE35E-61FE-453F-B9C3-1C3DD8E0ABE4", "Area"), ZCharacterCasing.Upper);
			AddProperty(WorkItemSchema.Constants.WKI_ActivityType, Res.GetString("9B31F40C-E4D2-4534-BA13-0CADEEFD24DA", "Activity Type"), ZCharacterCasing.Upper);
			AddProperty(WorkItemSchema.Constants.WKI_ActivitySubtype, Res.GetString("5366D13C-CB37-4113-B207-C173F495EDEA", "Activity Sub Type"), ZCharacterCasing.Upper);
			AddProperty(WorkItemSchema.Constants.WKI_Priority, Res.GetString("793FA435-738A-480B-9E04-126C38042527", "Priority"), ZCharacterCasing.Upper);
			AddProperty(WorkItemSchema.Constants.WKI_PortOrCountry, Res.GetString("AE03C771-705B-4A8D-A799-2652059881FF", "Port or Country/Region"), ZCharacterCasing.Upper);
			AddProperty(WorkItemSchema.Constants.WKI_Summary, Res.GetString("B63BDE28-622B-49B0-BD0A-7CB262BFE033", "Summary"), isMandatory: true);
			AddProperty(WorkItemSchema.Constants.WKI_Details, Res.GetString("91CB88BA-094A-40F6-94DE-B2D8787FDD68", "Details"));
		}
		void AddProperty(string flattenedPropertyName, string headerText, ZCharacterCasing characterCasing = ZCharacterCasing.Normal, bool isMandatory = false)
		{
			var property = new ImportPropertyInfoImpl<WorkItemFlattened>(flattenedPropertyName, isMandatory)
			{
				HeaderText = headerText,
				CharacterCasing = characterCasing,
			};
			Add(property);
		}
	}
}
