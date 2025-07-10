using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.ZArchitecture.Schema;
using USIntegration = Enterprise.Integration.Customs.US;

namespace Enterprise.Customs.US.Business
{
	public class ForwardingShipmentCustomsQueryProvider : USIntegration.IForwardingShipmentCustomsQueryProvider
	{
		ZQuery USIntegration.IForwardingShipmentCustomsQueryProvider.GetSimplifiedEntryBillStatusQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));

			if (value == SEBillProcessingResultList.BillStatusHoldOrExam)
			{
				var billSubQuery = new ZDBOnlySubQuery(typeof(Bill), CusDecHouseBillSchema.CU_JE);
				billSubQuery.AddToFilter(CusDecHouseBillSchema.CU_MessageStatus, SQLComparisonOperator.NotEqual, ZString.Empty);
				result.AddSubQuery(billSubQuery, JoinCondition.And);
			}
			else if (!value.IsEmpty)
			{
				result.AddFilterAndZSQLParameterCollection(BillStatusQueryText, new ZSqlParameterCollection(ZSqlParameter.New("@Status", value, CusDecHouseBillSchema.CU_MessageStatus, filterOperator)), JoinCondition.And);
			}
			return result;
		}

		const string BillStatusQueryText = @"
EXISTS
(
	SELECT NULL
	FROM dbo.CusDecHouseBill
	CROSS APPLY 
	(
		SELECT TOP (1) B7_ParentID AS BillPK, DispositionDateInfo.ValueAsSmallDateTime AS MaxDispositionDate
		FROM dbo.CusAddInfo
		CROSS APPLY dbo.csfn_GetAddInfoValueFromCodeInlineAsSmallDateTime(B7_AddInfoData, 'DispositionDate') DispositionDateInfo
		CROSS APPLY dbo.csfn_GetAddInfoValueFromCodeInline(B7_AddInfoData, 'Source') SourceInfo
		WHERE 1 = 1
			AND B7_ParentID = CU_PK
			AND B7_ParentTableCode = 'CU'
			AND B7_Type = 'UDP'
			AND SourceInfo.Value = 'SO'
			AND DispositionDateInfo.ValueAsSmallDateTime IS NOT NULL
		ORDER BY DispositionDateInfo.ValueAsSmallDateTime DESC
	) MaxDispositionDateInfo
	WHERE CU_ClusterKey = JE_ClusterKey
	AND EXISTS(SELECT NULL
				FROM dbo.CusAddInfo
				CROSS APPLY dbo.csfn_GetAddInfoValueFromCodeInline(B7_AddInfoData, 'Code') CodeInfo
				CROSS APPLY dbo.csfn_GetAddInfoValueFromCodeInline(B7_AddInfoData, 'Source') SourceInfo
				CROSS APPLY dbo.csfn_GetAddInfoValueFromCodeInlineAsSmallDateTime(B7_AddInfoData, 'DispositionDate') DispositionDateInfo
				WHERE 1 = 1
					AND B7_ParentID = BillPK
					AND B7_ParentTableCode = 'CU'
					AND B7_Type = 'UDP'
					AND SourceInfo.Value = 'SO'
					AND CodeInfo.Value = @Status
					AND DispositionDateInfo.ValueAsSmallDateTime = MaxDispositionDateInfo.MaxDispositionDate)
)
";

		ZQuery USIntegration.IForwardingShipmentCustomsQueryProvider.GetHoldExamBillStatusQuery(ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));

			var billStatusSubQuery = new ZDBOnlySubQuery(typeof(Bill), CusDecHouseBillSchema.CU_ClusterKey);
			if (value == HLDOrEXMStatusList.Codes.Yes || value == HLDOrEXMStatusList.Codes.No)
			{
				var notIn = value == HLDOrEXMStatusList.Codes.No;
				var billWithNoneEmptyStatusSubQuery = new ZDBOnlySubQuery(typeof(Bill), CusDecHouseBillSchema.CU_ClusterKey, notIn);
				billWithNoneEmptyStatusSubQuery.AddToFilter(CusDecHouseBillSchema.CU_MessageStatus, SQLComparisonOperator.NotEqual, ZString.Empty);
				result.AddSubQuery(JobDeclarationSchema.JE_ClusterKey, billWithNoneEmptyStatusSubQuery, JoinCondition.And);
			}
			result.AddSubQuery(JobDeclarationSchema.JE_ClusterKey, billStatusSubQuery, JoinCondition.And);
			return result;
		}
	}
}
