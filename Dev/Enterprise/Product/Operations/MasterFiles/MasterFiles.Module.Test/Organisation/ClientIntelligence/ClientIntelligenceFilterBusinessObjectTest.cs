using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(ClientIntelligenceFilterBusinessObject))]
	sealed class ClientIntelligenceFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			ClientIntelligenceFilterBusinessObject filterStripBizO = (ClientIntelligenceFilterBusinessObject)GetNewFilterStripBusinessObject();

			OrgTypeModuleFilter filter = (OrgTypeModuleFilter)filterStripBizO["Organisation Types"];
			Assert("Sales Lead", filter.Property11);

			filterStripBizO.ResetToDefaultValues();
			Assert("Remains as Sales Lead", filter.Property11);
		}

		public void TestCorrectOrgTypesToFilterByShown()
		{
			Env.Registry.SetOrgShowARTab(true);
			Env.Registry.SetOrgShowConsigneeConsignorTab(true);
			ClientIntelligenceFilterBusinessObject filterStripBizO = (ClientIntelligenceFilterBusinessObject)GetNewFilterStripBusinessObject();

			OrgTypeModuleFilter filter = (OrgTypeModuleFilter)filterStripBizO["Organisation Types"];

			Assert("Consignee option should not be read only when registry setting on", !filter.Property2Info.ReadOnly);
			Assert("Consignor option should not be read only when registry setting on", !filter.Property3Info.ReadOnly);
			Assert("AR option should not be read only when registry setting on", !filter.Property0Info.ReadOnly);
			Assert("OR option should not be readonly when registry setting on", !filter.OrJoinConditionInfo.ReadOnly);
			Assert("And option should not be readonly when registry setting on", !filter.AndJoinConditionInfo.ReadOnly);

			Env.Registry.SetOrgShowARTab(false);
			Env.Registry.SetOrgShowConsigneeConsignorTab(true);

			filterStripBizO = (ClientIntelligenceFilterBusinessObject)GetNewFilterStripBusinessObject();
			filter = (OrgTypeModuleFilter)filterStripBizO["Organisation Types"];

			Assert("Consignee option should not be read only when registry setting on", !filter.Property2Info.ReadOnly);
			Assert("Consignor option should not be read only when registry setting on", !filter.Property3Info.ReadOnly);
			Assert("AR option should be read only when registry setting off", filter.Property0Info.ReadOnly);
			Assert("AR option should not be selected when registry setting off", !filter.Property0);
			Assert("OR option should not be readonly when either registry setting on", !filter.OrJoinConditionInfo.ReadOnly);
			Assert("And option should not be readonly when either registry setting on", !filter.AndJoinConditionInfo.ReadOnly);

			Env.Registry.SetOrgShowARTab(true);
			Env.Registry.SetOrgShowConsigneeConsignorTab(false);

			filterStripBizO = (ClientIntelligenceFilterBusinessObject)GetNewFilterStripBusinessObject();
			filter = (OrgTypeModuleFilter)filterStripBizO["Organisation Types"];

			Assert("Consignee option should be read only when registry setting off", filter.Property2Info.ReadOnly);
			Assert("Consignor option should be read only when registry setting off", filter.Property3Info.ReadOnly);
			Assert("AR option should not be read only when registry setting on", !filter.Property0Info.ReadOnly);
			Assert("Consignor option should not be selected when registry setting off", !filter.Property3);
			Assert("Consignee option should not be selected when registry setting off", !filter.Property2);
			Assert("OR option should not be readonly when either registry setting on", !filter.OrJoinConditionInfo.ReadOnly);
			Assert("And option should not be readonly when either registry setting on", !filter.AndJoinConditionInfo.ReadOnly);

			Env.Registry.SetOrgShowARTab(false);
			Env.Registry.SetOrgShowConsigneeConsignorTab(false);

			filterStripBizO = (ClientIntelligenceFilterBusinessObject)GetNewFilterStripBusinessObject();
			filter = (OrgTypeModuleFilter)filterStripBizO["Organisation Types"];

			Assert("Consignee option should be read only when registry setting off", filter.Property2Info.ReadOnly);
			Assert("Consignor option should be read only when registry setting off", filter.Property3Info.ReadOnly);
			Assert("AR option should be read only when registry setting off", filter.Property0Info.ReadOnly);
			Assert("Consignor option should not be selected when registry setting off", !filter.Property3);
			Assert("Consignee option should not be selected when registry setting off", !filter.Property2);
			Assert("AR option should not be selected when registry setting off", !filter.Property0);
			Assert("OR option should be readonly when both registry settings off", filter.OrJoinConditionInfo.ReadOnly);
			Assert("And option should be readonly when bothregistry settings off", filter.AndJoinConditionInfo.ReadOnly);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ClientIntelligenceFilterBusinessObject();
		}
	}
}
