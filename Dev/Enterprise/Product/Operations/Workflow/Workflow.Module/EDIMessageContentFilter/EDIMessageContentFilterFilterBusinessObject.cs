using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Workflow.Module
{
	public class EDIMessageContentFilterFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			result.AddTextFilter(EDIMessageContentFilterSchema.Constants.ECF_Name, EDIMessageContentFilterSchema.ECF_Name).MultilingualDescription = ResString.GetMultilingualString("EDIMcfFilter|ECF_Name", "Name");
			result.AddNkFilter(EDIMessageContentFilterSchema.Constants.ECF_SystemCreateUser, EDIMessageContentFilterSchema.ECF_SystemCreateUser, ModuleIDs.GlbStaff, () => new GlbStaffCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("EDIMcfFilter|ECF_SystemCreateUser", "Create User");
			result.AddNkFilter(EDIMessageContentFilterSchema.Constants.ECF_SystemLastEditUser, EDIMessageContentFilterSchema.ECF_SystemLastEditUser, ModuleIDs.GlbStaff, () => new GlbStaffCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("EDIMcfFilter|ECF_SystemLastEditUser", "Last Edit User");
			result.AddDateFilter(EDIMessageContentFilterSchema.Constants.ECF_SystemLastEditTimeUtc, EDIMessageContentFilterSchema.ECF_SystemLastEditTimeUtc, true).MultilingualDescription = ResString.GetMultilingualString("EDIMcfFilter|ECF_SystemLastEditTime", "System Last Edit Time");
			result.AddDateFilter(EDIMessageContentFilterSchema.Constants.ECF_SystemCreateTimeUtc, EDIMessageContentFilterSchema.ECF_SystemCreateTimeUtc, true).MultilingualDescription = ResString.GetMultilingualString("EDIMcfFilter|ECF_SystemCreateTime", "System Create Time");
			return result;
		}
	}
}
