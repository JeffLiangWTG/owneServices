using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.ServiceTasks.TelematicsEquipment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Telematics.ServiceTasks.Test.TelematicsEquipment
{
	[TestedType(typeof(TelEdgeEquipmentTreeNode))]
	public class TelEdgeEquipmentTreeNodeTests : BusinessObjectBaseTestCase
	{
		public void TestProperties()
		{
			var expectedGuidTE_PK = ZGuid.NewZGuid();
			var expectedGuidTE_EntityIdFrom = ZGuid.NewZGuid();
			var expectedGuidTE_EntityIdTo = ZGuid.NewZGuid();
			var expectedCodeEntityTableCodeFrom = "ASD";
			var expectedCodeEntityTableCodeTo = "DSA";
			var expectedStringParentConfig = "{'Things'}";
			var expectedStringChildConfig = "{'MoreThings'}";
			var expectedStringChildId = "01020304";
			var expectedStringParentId = "04030201";
			var expectedStringChildType = TelEdgeEntityTableCodes.Codes.TSE;
			var expectedStringParentType = TelEdgeEntityTableCodes.Codes.TSE;
			var expectedDateTimeOffsetStartTime = DateTimeOffset.Now.AddDays(-1);
			var expectedDateTimeOffsetEndTime = DateTimeOffset.Now;
			DynamicBusinessObjectCollection<TelEdgeEquipmentTreeNode> collection = new DynamicBusinessObjectCollection<TelEdgeEquipmentTreeNode>(Factory);
			ZSqlParameterCollection @params = new ZSqlParameterCollection();
			@params.Add("@TET_TelEdgePK", expectedGuidTE_PK, DummyBizoSchema.Z0_Guid);
			@params.Add("@TET_ParentPK", expectedGuidTE_EntityIdFrom, DummyBizoSchema.Z0_Guid);
			@params.Add("@TET_ChildPK", expectedGuidTE_EntityIdTo, DummyBizoSchema.Z0_Guid);
			@params.Add("@TET_ParentTableCode", expectedCodeEntityTableCodeFrom, DummyBizoSchema.Z0_Code);
			@params.Add("@TET_ChildTableCode", expectedCodeEntityTableCodeTo, DummyBizoSchema.Z0_Code);
			@params.Add("@TET_ParentConfig", expectedStringParentConfig, DummyBizoSchema.Z0_Description);
			@params.Add("@TET_ChildConfig", expectedStringChildConfig, DummyBizoSchema.Z0_Description);
			@params.Add("@TET_ChildId", expectedStringChildId, DummyBizoSchema.Z0_Code);
			@params.Add("@TET_ParentId", expectedStringParentId, DummyBizoSchema.Z0_Code);
			@params.Add("@TET_ChildType", expectedStringChildType, DummyBizoSchema.Z0_Code);
			@params.Add("@TET_ParentType", expectedStringParentType, DummyBizoSchema.Z0_Code);
			@params.Add("@TET_StartTime", expectedDateTimeOffsetStartTime, DummyBizoSchema.Z0_DateTimeOffset);
			@params.Add("@TET_EndTime", expectedDateTimeOffsetEndTime, DummyBizoSchema.Z0_DateTimeOffset);
			collection.Load(@"
SELECT
    @TET_TelEdgePK AS TET_TelEdgePK,
    @TET_ParentPK AS TET_ParentPK,
    @TET_ChildPK AS TET_ChildPK,
    @TET_ParentTableCode AS TET_ParentTableCode,
    @TET_ChildTableCode AS TET_ChildTableCode,
    @TET_ParentConfig AS TET_ParentConfig,
    @TET_ChildConfig AS TET_ChildConfig,
    @TET_ChildId AS TET_ChildId,
    @TET_ParentId AS TET_ParentId,
    @TET_ChildType AS TET_ChildType,
    @TET_ParentType AS TET_ParentType,
    @TET_EndTime AS TET_EndTime,
    @TET_StartTime AS TET_StartTime", @params);

			AssertEquals(expectedGuidTE_PK, collection[0].TET_TelEdgePK);
			AssertEquals(expectedGuidTE_EntityIdFrom, collection[0].TET_ParentPK);
			AssertEquals(expectedGuidTE_EntityIdTo, collection[0].TET_ChildPK);
			AssertEquals(expectedCodeEntityTableCodeFrom, collection[0].TET_ParentTableCode);
			AssertEquals(expectedCodeEntityTableCodeTo, collection[0].TET_ChildTableCode);
			AssertEquals(expectedStringParentConfig, collection[0].TET_ParentConfig);
			AssertEquals(expectedStringChildConfig, collection[0].TET_ChildConfig);
			AssertEquals(expectedStringChildId, collection[0].TET_ChildId);
			AssertEquals(expectedStringParentId, collection[0].TET_ParentId);
			AssertEquals(expectedStringChildType, collection[0].TET_ChildType);
			AssertEquals(expectedStringParentType, collection[0].TET_ParentType);
			AssertEquals(expectedDateTimeOffsetEndTime, collection[0].TET_EndTime);
			AssertEquals(expectedDateTimeOffsetStartTime, collection[0].TET_StartTime);
		}

		public void TestNullableProperties()
		{
			var expectedGuidTE_PK = ZGuid.NewZGuid();
			var expectedGuidTE_EntityIdFrom = ZGuid.NewZGuid();
			var expectedGuidTE_EntityIdTo = ZGuid.NewZGuid();
			var expectedCodeEntityTableCodeFrom = "ASD";
			var expectedCodeEntityTableCodeTo = "DSA";
			var expectedStringParentConfig = "{'Things'}";
			var expectedStringChildConfig = "{'MoreThings'}";
			var expectedStringChildId = "01020304";
			var expectedStringParentId = "04030201";
			var expectedStringChildType = TelEdgeEntityTableCodes.Codes.TSE;
			var expectedStringParentType = TelEdgeEntityTableCodes.Codes.TSE;
			var expectedDateTimeOffsetStartTime = DateTimeOffset.Now.AddDays(-1);
			var dateTimeOffsetEndTime = ZDateTimeOffset.Empty;
			DynamicBusinessObjectCollection<TelEdgeEquipmentTreeNode> collection = new DynamicBusinessObjectCollection<TelEdgeEquipmentTreeNode>(Factory);
			ZSqlParameterCollection @params = new ZSqlParameterCollection();
			@params.Add("@TET_TelEdgePK", expectedGuidTE_PK, DummyBizoSchema.Z0_Guid);
			@params.Add("@TET_ParentPK", expectedGuidTE_EntityIdFrom, DummyBizoSchema.Z0_Guid);
			@params.Add("@TET_ChildPK", expectedGuidTE_EntityIdTo, DummyBizoSchema.Z0_Guid);
			@params.Add("@TET_ParentTableCode", expectedCodeEntityTableCodeFrom, DummyBizoSchema.Z0_Code);
			@params.Add("@TET_ChildTableCode", expectedCodeEntityTableCodeTo, DummyBizoSchema.Z0_Code);
			@params.Add("@TET_ParentConfig", expectedStringParentConfig, DummyBizoSchema.Z0_Description);
			@params.Add("@TET_ChildConfig", expectedStringChildConfig, DummyBizoSchema.Z0_Description);
			@params.Add("@TET_ChildId", expectedStringChildId, DummyBizoSchema.Z0_Code);
			@params.Add("@TET_ParentId", expectedStringParentId, DummyBizoSchema.Z0_Code);
			@params.Add("@TET_ChildType", expectedStringChildType, DummyBizoSchema.Z0_Code);
			@params.Add("@TET_ParentType", expectedStringParentType, DummyBizoSchema.Z0_Code);
			@params.Add("@TET_StartTime", expectedDateTimeOffsetStartTime, DummyBizoSchema.Z0_DateTimeOffset);
			@params.Add("@TET_EndTime", dateTimeOffsetEndTime, DummyBizoSchema.Z0_DateTimeOffset);
			collection.Load(@"
SELECT
    @TET_TelEdgePK AS TET_TelEdgePK,
    @TET_ParentPK AS TET_ParentPK,
    @TET_ChildPK AS TET_ChildPK,
    @TET_ParentTableCode AS TET_ParentTableCode,
    @TET_ChildTableCode AS TET_ChildTableCode,
    @TET_ParentConfig AS TET_ParentConfig,
    @TET_ChildConfig AS TET_ChildConfig,
    @TET_ChildId AS TET_ChildId,
    @TET_ParentId AS TET_ParentId,
    @TET_ChildType AS TET_ChildType,
    @TET_ParentType AS TET_ParentType,
    @TET_EndTime AS TET_EndTime,
    @TET_StartTime AS TET_StartTime", @params);

			AssertEquals(expectedGuidTE_PK, collection[0].TET_TelEdgePK);
			AssertEquals(expectedGuidTE_EntityIdFrom, collection[0].TET_ParentPK);
			AssertEquals(expectedGuidTE_EntityIdTo, collection[0].TET_ChildPK);
			AssertEquals(expectedCodeEntityTableCodeFrom, collection[0].TET_ParentTableCode);
			AssertEquals(expectedCodeEntityTableCodeTo, collection[0].TET_ChildTableCode);
			AssertEquals(expectedStringParentConfig, collection[0].TET_ParentConfig);
			AssertEquals(expectedStringChildConfig, collection[0].TET_ChildConfig);
			AssertEquals(expectedStringChildId, collection[0].TET_ChildId);
			AssertEquals(expectedStringParentId, collection[0].TET_ParentId);
			AssertEquals(expectedStringChildType, collection[0].TET_ChildType);
			AssertEquals(expectedStringParentType, collection[0].TET_ParentType);
			AssertEquals(expectedDateTimeOffsetStartTime, collection[0].TET_StartTime);
			AssertNull(collection[0].TET_EndTime);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var row = new DataTable().Rows.Add(Array.Empty<object>());
			return new TelEdgeEquipmentTreeNode(Factory, row);
		}
	}
}
