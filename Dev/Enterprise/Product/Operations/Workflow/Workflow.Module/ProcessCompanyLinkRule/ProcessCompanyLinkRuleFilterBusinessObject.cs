using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Workflow.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Workflow.Module
{
	public class ProcessCompanyLinkRuleFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			result.AddFlagsFilter(ProcessCompanyLinkRuleSchema.Constants.PCR_IsActive,
				new[] {
					Res.GetString("fa07a509-5bc1-4879-85b8-49e2a674effe", "Active"),
					Res.GetString("fa07a509-5bc1-4879-85b8-49e2a674efff", "Inactive")
				}, new[] {
					new GetFlagsQuery(t => t ? new ZQuery(ProcessCompanyLinkRuleSchema.PCR_IsActive, true) : new ZQuery()),
					new GetFlagsQuery(t => t ? new ZQuery(ProcessCompanyLinkRuleSchema.PCR_IsActive, false) : new ZQuery())
				}).MultilingualDescription = ResString.GetMultilingualString("PCLRFilter|PCR_IsActive", "Is Active");
			result.AddGuidFilter(ProcessCompanyLinkRuleSchema.Constants.PCR_GC_Company, ModuleIDs.GlbCompany, ProcessCompanyLinkRuleSchema.PCR_GC_Company, () => new GlbCompanyCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("PCLRFilter|PCR_GC_Company", "Company");
			result.AddTextFilter(ProcessCompanyLinkRuleSchema.Constants.PCR_Type, ProcessCompanyLinkRuleSchema.PCR_Type, () => new WorkflowDescriptorList()).MultilingualDescription = ResString.GetMultilingualString("PCLRFilter|PCR_Type", "Workflow Type");
			result.AddDateFilter(ProcessCompanyLinkRuleSchema.Constants.PCR_SystemLastEditTimeUtc, ProcessCompanyLinkRuleSchema.PCR_SystemLastEditTimeUtc, true).MultilingualDescription = ResString.GetMultilingualString("PCLRFilter|PCR_SystemLastEditTime", "System Last Edit Time");
			result.AddDateFilter(ProcessCompanyLinkRuleSchema.Constants.PCR_SystemCreateTimeUtc, ProcessCompanyLinkRuleSchema.PCR_SystemCreateTimeUtc, true).MultilingualDescription = ResString.GetMultilingualString("PCLRFilter|PCR_SystemCreateTime", "System Create Time");

			return result;
		}
	}
}
