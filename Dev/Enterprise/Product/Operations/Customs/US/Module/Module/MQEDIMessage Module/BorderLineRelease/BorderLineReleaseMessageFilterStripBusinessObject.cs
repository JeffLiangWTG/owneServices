using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Module
{
	public class BorderLineReleaseMessageFilterStripBusinessObject : MQEDIMessageCommonFilterStripBusinessObject
	{
		public OrgHeaderCollection Consignors => new ConsignorCollection(Factory);

		public OrgHeaderCollection Consignees => new ConsigneeCollection(Factory);

		protected override ZBool ShouldAddApplicationCodeFilter => false;

		protected override ZBool ShouldAddDirectionFilter => false;

		protected override ZBool ShouldAddMessageTypeSubTypeFilter => false;

		protected override ZBool ShouldAddInterchageDetailFilters => false;

		protected override ZBool ShouldAddMessageNumFilter => false;

		protected override bool ShouldAddStatusFilter => false;

		protected override ZBool ShouldAddMessageTimeFilter => true;

		protected override ZBool ShouldAddEHubIDFilters => false;

		protected override ZBool ShouldAddSenderFilters => false;

		protected override ZBool ShouldAddReceiverFilters => false;

		protected override string GetApplicationReferenceFilterName() => "Entry Number";

		GenAddOnColumnQueryHelper simpleQueryHelper;
		protected GenAddOnColumnQueryHelper SimpleQueryHelper => simpleQueryHelper ?? (simpleQueryHelper = new GenAddOnColumnQueryHelper(typeof(LineReleaseMQEDIMessage), typeof(LineReleaseMQEDIMessage), EDIMessageSchema.PK));

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = base.GetModuleFiltersCore();
			result.AddTextFilter(DeclarationFilterConstants.ActionStatus, GetActionStatusQuery, new EM_ActionStatusList());
			result.AddNumberFilter(DeclarationFilterConstants.ImporterEIN, GetImporterNumberQuery).MaxLength = LineReleaseMQEDIMessage.Schema.US_ImporterNumberMaxLength;
			result.AddDateFilter(DeclarationFilterConstants.ColumnnCaptions.ReleaseDate, GetReleaseDateQuery);
			result.AddNumberFilter(DeclarationFilterConstants.IssuerScacBol, GetSCACBillOfLadingNumberQuery).MaxLength = LineReleaseMQEDIMessage.Schema.US_BillOfLadingMaxLength;

			var importerSupplierFilter = result.AddGuidFilter(DeclarationFilterConstants.OrgFilterTypes.ImporterSupplier, ModuleIDs.Organisation, GetImporterSupplierQuery, Consignees, Consignors);
			importerSupplierFilter.SetItemDescriptions(new ResourceStringData("", DeclarationFilterConstants.OrgFilterTypes.Importer), new ResourceStringData("", DeclarationFilterConstants.OrgFilterTypes.Supplier));

			return result;
		}

		ZQuery GetImporterSupplierQuery(ZGuid importer, ZGuid supplier)
		{
			var zQuery = new ZDBOnlyQuery(typeof(LineReleaseMQEDIMessage));

			if (importer.IsValid)
			{
				AddImporterSubQuery(importer, zQuery);
			}

			if (supplier.IsValid)
			{
				AddSupplierSubQuery(supplier, zQuery);
			}

			return zQuery;
		}

		void AddSupplierSubQuery(ZGuid supplier, ZDBOnlyQuery zQuery)
		{
			var cusCodeQuery = new ZDBOnlySubQuery(typeof(OrgCusCode), OrgCusCodeSchema.OK_CustomsRegNo);
			cusCodeQuery.AddToFilter(JoinCondition.Or, OrgCusCodeSchema.OK_CodeType, OrgCusCode.USACodeTypes.ManufacturerID);
			cusCodeQuery.AddToFilter(JoinCondition.And, OrgCusCodeSchema.OK_OH, supplier);

			var subQuery = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID);
			subQuery.AddToFilter(JoinCondition.And, GenAddOnColumnSchema.XA_Name, SQLComparisonOperator.Equal, LineReleaseMQEDIMessage.Schema.US_SupplierCode);
			subQuery.AddSubQuery(GenAddOnColumnSchema.XA_Data, cusCodeQuery, JoinCondition.And);
			zQuery.AddSubQuery(subQuery, JoinCondition.And);
		}

		void AddImporterSubQuery(ZGuid importer, ZDBOnlyQuery zQuery)
		{
			var cusCodeQuery = new ZDBOnlySubQuery(typeof(OrgCusCode), OrgCusCodeSchema.OK_CustomsRegNo);
			cusCodeQuery.AddToFilter(JoinCondition.Or, OrgCusCodeSchema.OK_CodeType, OrgCusCode.USACodeTypes.SocialSecurityNumber);
			cusCodeQuery.AddToFilter(JoinCondition.Or, OrgCusCodeSchema.OK_CodeType, OrgCusCode.USACodeTypes.EmployerIdentificationNumber);
			cusCodeQuery.AddToFilter(JoinCondition.Or, OrgCusCodeSchema.OK_CodeType, OrgCusCode.USACodeTypes.CBPAssignedNumber);
			cusCodeQuery.AddToFilter(JoinCondition.And, OrgCusCodeSchema.OK_OH, importer);

			var subQuery = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID);
			subQuery.AddToFilter(JoinCondition.And, GenAddOnColumnSchema.XA_Name, SQLComparisonOperator.Equal, LineReleaseMQEDIMessage.Schema.US_ImporterNumber);
			subQuery.AddSubQuery(GenAddOnColumnSchema.XA_Data, cusCodeQuery, JoinCondition.And);
			zQuery.AddSubQuery(subQuery, JoinCondition.And);
		}

		ZQuery GetSCACBillOfLadingNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(LineReleaseMQEDIMessage));

			var subQuery = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID);
			subQuery.AddToFilter(JoinCondition.And, GenAddOnColumnSchema.XA_Name, SQLComparisonOperator.Equal, LineReleaseMQEDIMessage.Schema.US_BillOfLading);
			subQuery.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, GenAddOnColumnSchema.XA_Data, comparisonOperator, value);
			result.AddSubQuery(subQuery, JoinCondition.And);

			return result;
		}

		ZQuery GetReleaseDateQuery(DateComparisonOperator comparisonOperator, ZDateTime startDate, ZDateTime endDate)
		{
			return SimpleQueryHelper.GetQueryWithSubQueryOnGenAddOnColumn(LineReleaseMQEDIMessage.Schema.US_ReleaseDateTime, comparisonOperator, startDate, endDate);
		}

		ZQuery GetImporterNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(LineReleaseMQEDIMessage));

			var subQuery = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID);
			subQuery.AddToFilter(JoinCondition.And, GenAddOnColumnSchema.XA_Name, SQLComparisonOperator.Equal, LineReleaseMQEDIMessage.Schema.US_ImporterNumber);
			subQuery.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, GenAddOnColumnSchema.XA_Data, comparisonOperator, value);
			result.AddSubQuery(subQuery, JoinCondition.And);

			return result;
		}

		ZQuery GetActionStatusQuery(ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(LineReleaseMQEDIMessage));

			var subQuery = new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.SL_Parent, value == EM_ActionStatusList.Codes.Incomplete);
			subQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.Authorised.Code);
			subQuery.AddToFilter(StmALogSchema.SL_IsCancelled, false);

			if (value == EM_ActionStatusList.Codes.Incomplete)
			{
				subQuery.AddToFilter(StmALogSchema.SL_Reference, EM_ActionStatusList.Codes.Complete);
			}
			else
			{
				subQuery.AddToFilter(StmALogSchema.SL_Reference, value);
			}
			result.AddSubQuery(subQuery, JoinCondition.Or);

			return result;
		}
	}
}
