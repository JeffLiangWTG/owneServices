using Enterprise.MasterFiles.Business;
using Enterprise.Workflow.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Workflow.Module
{
	public class EDIMessagePurposeFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			result.AddTextFilter(EDIMessagePurposeSchema.Constants.EMP_Code, EDIMessagePurposeSchema.EMP_Code).MultilingualDescription = ResString.GetMultilingualString("EDIMPFilter|EMP_Code", "Code");
			result.AddGuidFilter(EDIMessagePurposeSchema.Constants.EMP_ECF_Filter, ModuleIDs.Messaging.EDIMessageContentFilter, EDIMessagePurposeSchema.EMP_ECF_Filter, () => new EDIMessageContentFilterCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("EDIMPFilter|EMP_ECF", "EDI Message Profile");
			result.AddFiltersForTranslatableText(EDIMessagePurposeSchema.Constants.EMP_Description, EDIMessagePurposeSchema.EMP_Description, typeof(EDIMessagePurpose), ResString.GetMultilingualString("EDIMPFilter|EMP_Description", "Description"));
			result.AddNkFilter(EDIMessagePurposeSchema.Constants.EMP_SystemCreateUser, EDIMessagePurposeSchema.EMP_SystemCreateUser, ModuleIDs.GlbStaff, () => new GlbStaffCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("EDIMPFilter|EMP_SystemCreateUser", "Create User");
			result.AddNkFilter(EDIMessagePurposeSchema.Constants.EMP_SystemLastEditUser, EDIMessagePurposeSchema.EMP_SystemLastEditUser, ModuleIDs.GlbStaff, () => new GlbStaffCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("EDIMPFilter|EMP_SystemLastEditUser", "Last Edit User");
			result.AddDateFilter(EDIMessagePurposeSchema.Constants.EMP_SystemLastEditTimeUtc, EDIMessagePurposeSchema.EMP_SystemLastEditTimeUtc, true).MultilingualDescription = ResString.GetMultilingualString("EDIMPFilter|EMP_SystemLastEditTime", "System Last Edit Time");
			result.AddDateFilter(EDIMessagePurposeSchema.Constants.EMP_SystemCreateTimeUtc, EDIMessagePurposeSchema.EMP_SystemCreateTimeUtc, true).MultilingualDescription = ResString.GetMultilingualString("EDIMPFilter|EMP_SystemCreateTime", "System Create Time");
			return result;
		}
	}
}
