using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.BufferManagement.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Module;
using Enterprise.Customs.Common.Shared;
using Enterprise.Environment;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.TransportBookings.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Module
{
	public class JobDeclarationFilterBusinessObject : FilterStripBusinessObject,
		IAccountingFilterStripHolder,
		IGridColourAdditionalModuleIdFilterSupporter
	{
		public JobDeclarationFilterBusinessObject()
		{
			CountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}

		public JobDeclarationFilterLookups Lookups
		{
			get
			{
				if (lookups == null)
				{
					lookups = GetNewLookups();
				}
				return lookups;
			}
		}
		JobDeclarationFilterLookups lookups;

		public ZString CountryCode
		{
			get { return fCountryCode; }
			set { fCountryCode = value; }
		}
		ZString fCountryCode;

		#region Implementation

		protected virtual JobDeclarationFilterLookups GetNewLookups()
		{
			return new JobDeclarationFilterLookups(this);
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection result = new ModuleFilterCollection();

			AddNumberFilters(result);
			AddDateFilters(result);
			AddOrganisationFilters(result);
			AddLocationFilters(result);
			AddModeFilters(result);
			AddStatusAndFlagsFilters(result);

			if (ShowManifestNumberFilter())
			{
				var manifestNumberFilter = result.AddTextFilter(DeclarationFilterConstants.NumberFilterTypes.ManifestNumber, JobDeclarationSchema.JE_ManifestNumber);
				manifestNumberFilter.Category = FilterCategories.NumbersAndReferences;
				manifestNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|ManifestNumber", DeclarationFilterConstants.NumberFilterTypes.ManifestNumber);
			}

			ModuleNkFilter filter = result.AddNkFilter("Commodity Code", GeCommodityCodeQuery, ModuleIDs.RefCommodityCode, RefCommodity_List);
			filter.Category = FilterCategories.NumbersAndReferences;
			filter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|CommodityCode", "Commodity Code");

			if (ObjectFactory.Get<ITagRulePolicy>().ShouldAddCompanyRelatedFilters)
			{
				AddCurrentCompanyFilter(result);
			}

			FilterGenerator.AddAdditionalReferenceNumberFilter(result, Factory);

			CusEntryNumberFilterStripHelper.AddReferenceNumberDateFilter(result, typeof(BaseJobDeclaration));

			AddCommercialInvoiceAttributeFilter(result);
			AddAuditFilters(result);
			AddBillingFilters(result);
			AddJobManagementFilters(result, Env.Security.CustomsDeclarationJobInvoicing);
			securityProvider.AddCRMSecurityFilterStrips(Factory, result);

			AddJobDeclarationRelatedShipmentSecurityFilter(result);
			AddRelatedContainersFilter(result);
			AddRelatedTransportBookingsFilter(result);

			AddApplicationCodeFilter(result);

			AddHVLVFilters(result);

			return result;
		}

		void AddJobDeclarationRelatedShipmentSecurityFilter(ModuleFilterCollection filters)
		{
			if (new JobShipmentCRMSecurityProvider().HasRestrictions)
			{
				var filter = new JobDeclarationRelatedShipmentSecurityFilter(Factory);
				filter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|RelatedShipmentSecurityFilter", "Related Shipment Security");
				filter.IsPublishedOnWeb = false;
				filter.Category = FilterCategories.CRMSecurity;
				filter.GroupOrCategory = FilterOrCategory.None;
				filter.IsGroupOrCategoryReadOnly = true;
				filter.Visibility = FilterVisibility.AlwaysApplied | FilterVisibility.AlwaysVisible;
				filter.ReadOnly = true;
				filter.OrCategory = FilterOrCategory.MandatoryFilterOrCategory;
				filter.IsOrCategoryReadOnly = true;
				filter.IsMandatorySecurityFilter = true;
				filters.AddFilter(filter);
			}
		}

		protected override void AddInitialAuditFilters(ModuleFilterCollection filters)
		{
			base.AddInitialAuditFilters(filters);

			var createdTimeFilter = filters[FilterDescriptions.CreatedTime] as ModuleDateFilter;
			if (createdTimeFilter != null)
			{
				createdTimeFilter.Visibility = FilterVisibility.AlwaysVisible;
				createdTimeFilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Last3Mths;
			}
		}

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();

			var workflowHelper = new WorkflowFilterStripsHelperCustoms(typeof(BaseJobDeclaration), WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorBrokerageAttachedCode, Factory);
			workflowHelper.SetShouldAddWorkflowCustomFieldsFilters(ShouldAddWorkflowCustomFieldsFilters);
			helpers.Add(workflowHelper);

			var documentTrackingHelper = new JobDeclarationDocumentTrackingFilterStripsHelper();
			helpers.Add(documentTrackingHelper);

			return helpers;
		}

		protected virtual bool ShouldAddWorkflowCustomFieldsFilters => true;

		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			ModuleNumberFilter declarationFilter = new ModuleNumberFilter(DeclarationFilterConstants.NumberFilterTypes.DeclarationReference, GetDeclarationReferenceQuery);
			declarationFilter.MaxLength = JobDeclarationSchema.JE_DeclarationReference.MaxLength;
			declarationFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|DeclarationReference", DeclarationFilterConstants.NumberFilterTypes.DeclarationReference);
			return declarationFilter;
		}

		protected void AddCurrentCompanyFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddNkFilter(DeclarationFilterConstants.Country, GetCountryQuery, ModuleIDs.RefCountry, new RefCountryCollection(Factory));
			filter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|Country", DeclarationFilterConstants.Country);
			if (!Globals.IsWeb)
			{
				filter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
			}
		}

		protected ZQuery GetCountryQuery(ZString userEnteredCountryCode)
		{
			var result = new ZQuery();
			if (!Globals.IsWeb)
			{
				var code = CountryCode.IsEmpty ? userEnteredCountryCode : CountryCode;
				if (code.IsEmpty)
				{
					result.IsNoResultQuery = true;
				}
				else if (code == GlbCompany.CurrentCompany.Country.Code)
				{
					var companyQuery = GetCompanyQuery();
					companyQuery.IgnoreActiveFilter = true;
					result.AddToFilter(companyQuery);
				}
				else
				{
					result.AddToFilter(JobDeclarationFilter.ForCountry(ignoreActiveFilter: true, countryCode: code, factory: Factory));
				}
			}
			else
			{
				result.AddToFilter(JobDeclarationFilter.ForCountry(ignoreActiveFilter: true, countryCode: userEnteredCountryCode, factory: Factory));
			}
			return result;
		}

		protected ModuleNumberFilter AddEntryNumberFilter(ModuleFilterCollection filters)
		{
			var entryNumberFilter = filters.AddNumberFilter(DeclarationFilterConstants.NumberFilterTypes.EntryNumber, GetEntryNumberQuery)
				.WithMaxLengthOf<ModuleNumberFilter>(CusEntryNumSchema.CE_EntryNum);

			entryNumberFilter.IsCommon = IsCommonForEntryNumberFilter;
			entryNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|EntryNumber", DeclarationFilterConstants.NumberFilterTypes.EntryNumber);

			return entryNumberFilter;
		}

		protected virtual bool IsCommonForEntryNumberFilter => false;

		#endregion

		#region Number Filters

		protected virtual bool ShowManifestNumberFilter()
		{
			return false;
		}

		protected virtual void AddNumberFilters(ModuleFilterCollection filters)
		{
			var agentsReferenceFilter = FilterGenerator.AddAgentsReferenceFilter(filters, DeclarationFilterConstants.NumberFilterTypes.AgentsReference);
			agentsReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|AgentsReference", DeclarationFilterConstants.NumberFilterTypes.AgentsReference);

			var containerFilter = filters.AddNumberFilter(DeclarationFilterConstants.NumberFilterTypes.ContainerNumber, CusContainerSchema.CO_ContainerNumber);
			containerFilter.SubGroup = new ContainerNumberSubGroup();
			containerFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|ContainerNumber", DeclarationFilterConstants.NumberFilterTypes.ContainerNumber);

			AddEntryNumberFilter(filters);

			ModuleNumberFilter commonFilter = filters.AddNumberFilter(DeclarationFilterConstants.NumberFilterTypes.Common, GetCustomCommonModuleFilterQuery);
			commonFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			commonFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			commonFilter.MaxLength = CommonNumbersAndReferencesMaxLength;
			commonFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|Common", DeclarationFilterConstants.NumberFilterTypes.Common);

			filters.AddNumberFilter(DeclarationFilterConstants.NumberFilterTypes.InvoiceLineProductCode, GetInvoiceLineProductCodeQuery)
				.WithMaxLengthOf<ModuleNumberFilter>(JobComInvoiceLineSchema.JI_PartNo).MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|InvoiceLineProductCode", DeclarationFilterConstants.NumberFilterTypes.InvoiceLineProductCode);

			AddInvoiceNumberFilter(filters);

			var invoiceAmount = filters.AddNumberRangeFilter(DeclarationFilterConstants.NumberFilterTypes.InvoiceAmount, JobComInvoiceHeaderSchema.JZ_InvoiceAmount);
			invoiceAmount.Decimals = 2;
			invoiceAmount.SubGroup = InvoiceHeaderSubgroup;
			invoiceAmount.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|InvoiceAmount", DeclarationFilterConstants.NumberFilterTypes.InvoiceAmount);

			AddMasterBillAndHouseBillFilters(filters);

			var orderNumberOwnersFilter = filters.AddNumberFilter(DeclarationFilterConstants.NumberFilterTypes.OrderNumberOwnersRef, JobDeclarationSchema.JE_OwnerRef);
			orderNumberOwnersFilter.SubGroup = new OrderNumberOwnersRefSpecificFieldSubGroup();
			orderNumberOwnersFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			orderNumberOwnersFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			orderNumberOwnersFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|OrderNumberOwnersRef", DeclarationFilterConstants.NumberFilterTypes.OrderNumberOwnersRef);

			var invLineSubGroup = new InvoiceLineSpecificFieldSubGroup();

			var attr1Filter1 = filters.AddNumberFilter(DeclarationFilterConstants.NumberFilterTypes.PartAttribute1, JobComInvoiceLineSchema.JI_PartAttrib1);
			attr1Filter1.SubGroup = invLineSubGroup;
			attr1Filter1.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|PartAttribute1", DeclarationFilterConstants.NumberFilterTypes.PartAttribute1);
			var attr1Filter2 = filters.AddNumberFilter(DeclarationFilterConstants.NumberFilterTypes.PartAttribute2, JobComInvoiceLineSchema.JI_PartAttrib2);
			attr1Filter2.SubGroup = invLineSubGroup;
			attr1Filter2.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|PartAttribute2", DeclarationFilterConstants.NumberFilterTypes.PartAttribute2);
			var attr3Filter = filters.AddNumberFilter(DeclarationFilterConstants.NumberFilterTypes.PartAttribute3, JobComInvoiceLineSchema.JI_PartAttrib3);
			attr3Filter.SubGroup = invLineSubGroup;
			attr3Filter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|PartAttribute3", DeclarationFilterConstants.NumberFilterTypes.PartAttribute3);

			var paymentAmountFilter = filters.AddNumberRangeFilter(DeclarationFilterConstants.NumberFilterTypes.PaymentAmount, JobComInvoiceHeaderSchema.JZ_PaymentAmount);
			paymentAmountFilter.Decimals = 2;
			paymentAmountFilter.SubGroup = InvoiceHeaderSubgroup;
			paymentAmountFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|PaymentAmount", DeclarationFilterConstants.NumberFilterTypes.PaymentAmount);

			var paymentNoFilter = filters.AddNumberFilter(DeclarationFilterConstants.NumberFilterTypes.PaymentNumber, JobComInvoiceHeaderSchema.JZ_PaymentNo);
			paymentNoFilter.SubGroup = new PaymentNumberSubGroup();
			paymentNoFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|PaymentNumber", DeclarationFilterConstants.NumberFilterTypes.PaymentNumber);

			var tariffInvoiceLineFilter = filters.AddNumberFilter(DeclarationFilterConstants.NumberFilterTypes.TariffInvLine, JobComInvoiceLineSchema.JI_Tariff);
			tariffInvoiceLineFilter.SubGroup = invLineSubGroup;
			tariffInvoiceLineFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|TariffInvLine", DeclarationFilterConstants.NumberFilterTypes.TariffInvLine);

			var descriptionFilter = filters.AddNumberFilter(DeclarationFilterConstants.NumberFilterTypes.DescriptionInvLine, JobComInvoiceLineSchema.JI_Description);
			descriptionFilter.SubGroup = invLineSubGroup;
			descriptionFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|DescriptionInvLine", DeclarationFilterConstants.NumberFilterTypes.DescriptionInvLine);
		}

		const int CommonNumbersAndReferencesMaxLength = 35;

		protected virtual void AddMasterBillAndHouseBillFilters(ModuleFilterCollection filters)
		{
			var billFilter = filters.AddNumberFilter(DeclarationFilterConstants.NumberFilterTypes.HouseBill, CusDecHouseBillSchema.CU_BillNum);
			billFilter.SubGroup = new HouseBillSubGroup();
			billFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|HouseBill", DeclarationFilterConstants.NumberFilterTypes.HouseBill);

			var masterBillFilter = filters.AddNumberFilter(DeclarationFilterConstants.NumberFilterTypes.MasterBill, CusDecHouseBillSchema.CU_BillNum);
			masterBillFilter.SubGroup = new MasterBillSubGroup();
			masterBillFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|MasterBill", DeclarationFilterConstants.NumberFilterTypes.MasterBill);
		}

		protected void AddInvoiceNumberFilter(ModuleFilterCollection filters)
		{
			var invNumFilter = filters.AddNumberFilter(DeclarationFilterConstants.NumberFilterTypes.InvoiceNumber, JobComInvoiceHeaderSchema.JZ_InvoiceNumber);
			invNumFilter.SubGroup = InvoiceHeaderSubgroup;
			invNumFilter.MaxLength = JobComInvoiceHeaderSchema.JZ_InvoiceNumber.MaxLength;
			invNumFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|InvoiceNumber", DeclarationFilterConstants.NumberFilterTypes.InvoiceNumber);
		}

		InvoiceHeaderSubGroup InvoiceHeaderSubgroup
		{
			get { return invoiceHeaderSubgroup ?? (invoiceHeaderSubgroup = new InvoiceHeaderSubGroup()); }
		}
		InvoiceHeaderSubGroup invoiceHeaderSubgroup;

		public RefCommodityCodeCollection RefCommodity_List
		{
			get
			{
				if (fRefCommodity_List == null)
				{
					fRefCommodity_List = new RefCommodityCodeCollection(Factory);
				}

				return fRefCommodity_List;
			}
		}
		RefCommodityCodeCollection fRefCommodity_List;

		#region Queries

		ZQuery GetCustomCommonModuleFilterQuery(SQLComparisonOperator @operator, ZString value)
		{
			return GetCustomCommonModuleFilterQueryCore(@operator, value);
		}

		protected virtual ZQuery GetCustomCommonModuleFilterQueryCore(SQLComparisonOperator @operator, ZString value)
		{
			var notOrBlank = GetNotForInSubqueryIfNegative(@operator);
			@operator = @operator.GetNegatingSQLOperatorIfNotInSubquery();
			var entryNumberQuery = EntryNumberQueryGenerator.GetEntryNumberQuery(@operator, value, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(CountryCode));

			var entryNumberString = entryNumberQuery.GetAsWhereClause(true).TrimEnd(')').Replace("WHERE " + JobDeclarationSchema.PK.Name + " IN", JobDeclarationSchema.PK.Name + " IN");
			entryNumberString = Regex.Replace(entryNumberString, "WHERE.+?" + JobDeclarationSchema.PK.Name + ".+?IN", JobDeclarationSchema.PK.Name + " " + notOrBlank + " IN");

			var sqlParameters = new ZSqlParameterCollection();

			ZString queryString = string.Format(@"{0}" +
@"
UNION ALL

SELECT CU_JE FROM dbo.CusDecHouseBill
WHERE {1}
AND ( CU_BillType = 'SH'
OR CU_BillType = 'HB' )

UNION ALL

SELECT JE_PK FROM dbo.JobDeclaration
WHERE {2})", entryNumberString,
SQLAndParametersForOneCondition(@operator, value, CusDecHouseBillSchema.CU_BillNum, sqlParameters),
SQLAndParametersForOneCondition(@operator, value, JobDeclarationSchema.JE_DeclarationReference, sqlParameters));

			var result = new ZDBOnlyQuery(typeof(AutoJobDeclaration));
			result.AddFilterAndZSQLParameterCollection(queryString, sqlParameters);
			return result;
		}

		protected internal static string SQLAndParametersForOneCondition(SQLComparisonOperator @operator, ZString value, SchemaStringColumn column, ZSqlParameterCollection sqlParameters)
		{
			var query = new ZQuery();
			query.AddToFilter_PossiblyCommaSeparated(column, @operator, value);
			var result = "(" + query.FilterString + ")";
			var counter = 0;
			foreach (var param in query.Params)
			{
				var oldName = param.ParameterName;
				param.Rename("@" + column.Name.Replace("_", "q") + "_" + (counter++));
				result = result.Replace(oldName, param.ParameterName);
				sqlParameters.Add(param);
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI003", Justification = "Don't have access to the alternatives it wants")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		protected ZQuery GetDeclarationReferenceQuery(SQLComparisonOperator @operator, ZString oldValue)
		{
			var result = new ZQuery();
			if (@operator == SQLComparisonOperator.Equal)
			{
				var multiSearchSeparator = EnvProxy.Instance.Registry.MultiSearchSeparator;
				ZString[] values = null;
				if (oldValue.Contains(multiSearchSeparator))
				{
					values = oldValue.Split(multiSearchSeparator).Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToArray();
				}
				else
				{
					values = new ZString[] { oldValue };
				}

				var innerQuery = new ZQuery();

				foreach (var value in values)
				{
					if (value.Length <= 8)
					{
						if (value.StartsWith("S") || value.StartsWith("B") || value.StartsWith("G"))
						{
							var jobPrefix = value.Left(1);
							var jobNumber = value.SubstringSafe(1);
							if (jobNumber.IsEmpty)
							{
								innerQuery.AddToFilter(JoinCondition.Or, JobDeclarationSchema.JE_DeclarationReference, SQLComparisonOperator.StartsWith, jobPrefix);
							}
							else if (jobNumber.IsNumbersOnlyOrEmpty)
							{
								var jobNumberZeroPadded = jobNumber.PadLeft(8, '0');
								var subQuery = new ZQuery(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, SQLComparisonOperator.Equal, jobPrefix + jobNumber));
								subQuery.AddToFilter(JoinCondition.Or, JobDeclarationSchema.JE_DeclarationReference, SQLComparisonOperator.Equal, jobPrefix + jobNumberZeroPadded);
								innerQuery.AddToFilter(subQuery, JoinCondition.Or);
							}
							else
							{
								innerQuery.AddToFilter(JoinCondition.Or, JobDeclarationSchema.JE_DeclarationReference, SQLComparisonOperator.Equal, value);
							}
						}
						else if (value.IsNumbersOnlyOrEmpty)
						{
							var jobNumberZeroPadded = value.PadLeft(8, '0');
							var subQuery = new ZQuery(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, SQLComparisonOperator.Equal, value));
							subQuery.AddToFilter(JoinCondition.Or, JobDeclarationSchema.JE_DeclarationReference, SQLComparisonOperator.Equal, 'S' + jobNumberZeroPadded);
							subQuery.AddToFilter(JoinCondition.Or, JobDeclarationSchema.JE_DeclarationReference, SQLComparisonOperator.Equal, 'B' + jobNumberZeroPadded);
							subQuery.AddToFilter(JoinCondition.Or, JobDeclarationSchema.JE_DeclarationReference, SQLComparisonOperator.Equal, 'G' + jobNumberZeroPadded);
							innerQuery.AddToFilter(subQuery, JoinCondition.Or);
						}
						else
						{
							innerQuery.AddToFilter(JoinCondition.Or, JobDeclarationSchema.JE_DeclarationReference, SQLComparisonOperator.Equal, value);
						}
					}
					else
					{
						innerQuery.AddToFilter(JoinCondition.Or, JobDeclarationSchema.JE_DeclarationReference, SQLComparisonOperator.Equal, value);
					}
				}

				result.AddToFilter(innerQuery);
			}
			else
			{
				result.AddToFilter_PossiblyCommaSeparated(JobDeclarationSchema.JE_DeclarationReference, @operator, oldValue);
			}
			return result;
		}

		public class MasterBillSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlySubQuery masterBillSubQuery = new ZDBOnlySubQuery(typeof(Bill), CusDecHouseBillSchema.CU_JE);
				masterBillSubQuery.AddToFilter(CusDecHouseBillSchema.CU_BillType, BillTypeList.Codes.MasterBill);

				// DJC note to reviewer.  It's OK not to try two options, one verbatim (111-22222222) and one with the hyphen and space stripped.
				// JE_MasterBill doesn't allow such chars to be entered, and CU_BillNum has blue validation to suppress them,
				// so it is reasonable to assume that CU_BillNum will have no hyphen.
				masterBillSubQuery.AddToFilter(filter);

				ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
				result.AddSubQuery(masterBillSubQuery, JoinCondition.Or);

				if (filter.Params.Any(x => x.SchemaColumn == CusDecHouseBillSchema.CU_BillNum && x.ComparisonOperator == SpecialComparisonOperator.IsBlank))
				{
					var houseBillNotExistSql = string.Format(CultureInfo.InvariantCulture, @"
NOT EXISTS
(
	SELECT 1 FROM {0} WHERE {1} = {2}
	AND
	(
		{3} = '{4}'
	)
)
", CusDecHouseBillSchema.Constants.TableName, CusDecHouseBillSchema.Constants.CU_JE, JobDeclarationSchema.Constants.PK, CusDecHouseBillSchema.Constants.CU_BillType, BillTypeList.Codes.MasterBill);
					result.AddFilterAndZSQLParameterCollection(houseBillNotExistSql, new ZSqlParameterCollection(), JoinCondition.Or);
				}

				return result;
			}
		}

		public class HouseBillSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlySubQuery hblFilter = new ZDBOnlySubQuery(typeof(Bill), CusDecHouseBillSchema.CU_JE);
				hblFilter.AddToFilter(filter);

				ZQuery billTypeFilter = new ZQuery(CusDecHouseBillSchema.CU_BillType, BillTypeList.Codes.SubHouseBill);
				billTypeFilter.AddToFilter(JoinCondition.Or, CusDecHouseBillSchema.CU_BillType, SQLComparisonOperator.Equal, BillTypeList.Codes.HouseBill);
				hblFilter.AddToFilter(billTypeFilter);

				ZDBOnlyQuery dbOnlyQuery = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
				dbOnlyQuery.AddSubQuery(hblFilter, JoinCondition.Or);

				if (filter.Params.Any(x => x.SchemaColumn == CusDecHouseBillSchema.CU_BillNum && x.ComparisonOperator == SpecialComparisonOperator.IsBlank))
				{
					var houseBillNotExistSql = string.Format(CultureInfo.InvariantCulture, @"
NOT EXISTS
(
	SELECT 1 FROM {0} WHERE {1} = {2}
	AND
	(
		{3} = '{4}'
		OR
		{3} = '{5}'
	)
)
", CusDecHouseBillSchema.Constants.TableName, CusDecHouseBillSchema.Constants.CU_JE, JobDeclarationSchema.Constants.PK, CusDecHouseBillSchema.Constants.CU_BillType, BillTypeList.Codes.SubHouseBill, BillTypeList.Codes.HouseBill);
					dbOnlyQuery.AddFilterAndZSQLParameterCollection(houseBillNotExistSql, new ZSqlParameterCollection(), JoinCondition.Or);
				}

				return dbOnlyQuery;
			}
		}

		public class ContainerNumberSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(BaseCusContainer), CusContainerSchema.CO_JE);
				subQuery.AddToFilter(filter);

				ZDBOnlyQuery dbOnlyResult = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
				dbOnlyResult.AddSubQuery(subQuery, JoinCondition.And);

				return dbOnlyResult;
			}
		}

		public sealed class EntryInstructionSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var subQuery = new ZDBOnlySubQuery(typeof(CusEntryInstruction), CusEntryInstructionSchema.CEI_ClusterKey, JobDeclarationSchema.JE_ClusterKey);
				subQuery.AddToFilter(filter);

				var dbOnlyResult = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
				dbOnlyResult.AddSubQuery(subQuery, JoinCondition.And);

				return dbOnlyResult;
			}
		}

		public ZQuery GetEntryNumberQuery(SQLComparisonOperator operartor, ZString value)
		{
			return EntryNumberQueryGenerator.GetEntryNumberQuery(operartor, value, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(CountryCode));
		}

		#region GeCommodityCodeQuery

		protected ZQuery GeCommodityCodeQuery(ZString commodityCode)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(BaseJobDeclaration));

			RefCommodityCode commodity = Factory.LoadFromNaturalKey<RefCommodityCode>(RefCommodityCodeSchema.RH_Code, commodityCode);
			if (commodity != null)
			{
				ZDBOnlySubQuery invoiceSubQuery = new ZDBOnlySubQuery(typeof(BaseJobComInvoiceHeader), JobComInvoiceHeaderSchema.JZ_JE);
				ZDBOnlySubQuery invoiceLineSubQuery = new ZDBOnlySubQuery(typeof(BaseJobComInvoiceLine), JobComInvoiceLineSchema.JI_JZ);
				invoiceLineSubQuery.AddToFilter(JobComInvoiceLineSchema.JI_RH_NKCommodity_Code, commodity.RH_Code);

				invoiceSubQuery.AddSubQuery(invoiceLineSubQuery, JoinCondition.And);
				result.AddSubQuery(invoiceSubQuery, JoinCondition.And);
			}

			return result;
		}

		#endregion

		class PaymentNumberSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(BaseJobComInvoiceHeader), JobComInvoiceHeaderSchema.JZ_JE);
				subQuery.AddToFilter(filter);

				ZDBOnlyQuery dbOnlyResult = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
				dbOnlyResult.AddSubQuery(subQuery, JoinCondition.And);

				return dbOnlyResult;
			}
		}

		protected ZQuery GetInvoiceLineProductCodeQuery(SQLComparisonOperator @operator, ZString value)
		{
			ZDBOnlyQuery partSubQuery = ObjectFactory.Get<ICustomsFilterProvider>().GetInvoiceLineProductCodeQuery(@operator, value);
			return GetCommercialInvoiceQuery(partSubQuery);
		}

		public class InvoiceHeaderSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery dbOnlyResult = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(BaseJobComInvoiceHeader), JobComInvoiceHeaderSchema.JZ_JE);
				subQuery.AddToFilter(filter);
				subQuery.AddToFilter(JobComInvoiceHeaderSchema.JZ_GroupInvoice, false);
				dbOnlyResult.AddSubQuery(subQuery, JoinCondition.And);
				return dbOnlyResult;
			}
		}

		public class InvoiceLineSpecificFieldSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var invoiceLineSubQuery = new ZDBOnlySubQuery(typeof(BaseJobComInvoiceLine), JobComInvoiceLineSchema.JI_ClusterKey);

				invoiceLineSubQuery.AddToFilter(filter);

				var result = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
				result.AddSubQuery(JobDeclarationSchema.JE_ClusterKey, invoiceLineSubQuery, JoinCondition.And);
				return result;
			}
		}

		protected ZQuery GetSpecificFieldQueryFromInvoiceLine(SchemaStringColumn fieldToSearchColumn, SQLComparisonOperator comparisonOperator, ZString value, bool searchFromAddInfo = false)
		{
			ZDBOnlySubQuery invoiceLineSubQuery = new ZDBOnlySubQuery(typeof(BaseJobComInvoiceLine), JobComInvoiceLineSchema.JI_JZ);

			if (searchFromAddInfo)
			{
				var genAddOnSubQuery = new GenAddOnColumnQueryHelper(typeof(BaseJobComInvoiceLine)).GetContainsValueForAnyQuery(false, fieldToSearchColumn.Name);
				genAddOnSubQuery.AddToFilter_PossiblyCommaSeparated(GenAddOnColumnSchema.XA_Data, comparisonOperator, value);
				invoiceLineSubQuery.AddSubQuery(genAddOnSubQuery, JoinCondition.And);
			}
			else
			{
				invoiceLineSubQuery.AddToFilter_PossiblyCommaSeparated(fieldToSearchColumn, comparisonOperator, value);
			}

			var invoiceHeaderSubQuery = new ZDBOnlySubQuery(typeof(BaseJobComInvoiceHeader), JobComInvoiceHeaderSchema.JZ_JE);
			invoiceHeaderSubQuery.AddSubQuery(invoiceLineSubQuery, JoinCondition.And);

			var result = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
			result.AddSubQuery(invoiceHeaderSubQuery, JoinCondition.And);
			return result;
		}

		#endregion

		#endregion

		#region Date Filters

		void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter(DeclarationFilterConstants.DateFilterTypes.DateOfExport, JobDeclarationSchema.JE_ExportDate).MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|DateOfExport", DeclarationFilterConstants.DateFilterTypes.DateOfExport);
			filters.AddDateFilter(DeclarationFilterConstants.DateFilterTypes.DateOfArrival, JobDeclarationSchema.JE_DateOfArrival).MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|DateOfArrival", DeclarationFilterConstants.DateFilterTypes.DateOfArrival);
			filters.AddDateFilter(DeclarationFilterConstants.DateFilterTypes.FirstArrival, JobDeclarationSchema.JE_DateOfFirstArrival).MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|FirstArrival", DeclarationFilterConstants.DateFilterTypes.FirstArrival);
			var commercialInvoiceDateFilter = filters.AddDateFilter(DeclarationFilterConstants.DateFilterTypes.CommercialInvoiceDate, GetInvoiceDateQuery);
			commercialInvoiceDateFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|CommercialInvoiceDate", DeclarationFilterConstants.DateFilterTypes.CommercialInvoiceDate);
			commercialInvoiceDateFilter.SubGroup = InvoiceHeaderSubgroup;
			AddSubmittedDate(filters);
			var commercialInvoicePaymentDateFilter = filters.AddDateFilter(DeclarationFilterConstants.DateFilterTypes.CommercialInvoicePaymentDate, GetCommercialInvoicePaymentDateQuery);
			commercialInvoicePaymentDateFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|CommercialInvoicePaymentDate", DeclarationFilterConstants.DateFilterTypes.CommercialInvoicePaymentDate);
			commercialInvoicePaymentDateFilter.SubGroup = InvoiceHeaderSubgroup;
			filters.AddDateFilter(DeclarationFilterConstants.DateFilterTypes.GoodsDelivered, GetGoodsDeliveredDateQuery).MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|GoodsDelivered", DeclarationFilterConstants.DateFilterTypes.GoodsDelivered);
			filters.AddDateFilter(DeclarationFilterConstants.DateFilterTypes.CartageAdvised, GetCartageAdvisedDateQuery).MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|CartageAdvised", DeclarationFilterConstants.DateFilterTypes.CartageAdvised);
			filters.AddDateFilter(DeclarationFilterConstants.DateFilterTypes.EstimatedTimeOfArrival, JobDeclarationSchema.JE_DateAtFinalDestination).MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|EstimatedTimeOfArrival", DeclarationFilterConstants.DateFilterTypes.EstimatedTimeOfArrival);
			AddAdditionalDateFilters(filters);

			filters.AddDateFilter(DeclarationFilterConstants.DateFilterTypes.ETDOfLoading, GetETDOfLoadingFilter, false).MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|ETDOfLoading", DeclarationFilterConstants.DateFilterTypes.ETDOfLoading);
			filters.AddDateFilter(DeclarationFilterConstants.DateFilterTypes.ETAOfDischarge, GetETAOfDischargeFilter, false).MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|ETAOfDischarge", DeclarationFilterConstants.DateFilterTypes.ETAOfDischarge);

			filters.AddDateFilter(DeclarationFilterConstants.DateFilterTypes.ServiceDateBooked, GetServiceDateBookedFilter, true).MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|ServiceDateBooked", DeclarationFilterConstants.DateFilterTypes.ServiceDateBooked);
			filters.AddDateFilter(DeclarationFilterConstants.DateFilterTypes.ServiceCompleted, GetServicesCompletedFilter, true).MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|ServicesCompleted", DeclarationFilterConstants.DateFilterTypes.ServiceCompleted);

			if (Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(CountryCode) != Core.Constants.CountryCodes.UnitedStates)
			{
				var dateFilter = filters.AddDateFilter(DeclarationFilterConstants.DateFilterTypes.EntryReleaseDate, GetReleaseDateQuery);
				dateFilter.SubGroup = EntryHeaderSubGroupForReleaseDateFilter;
				dateFilter.MultilingualDescription = ResString.GetMultilingualString("C1B7823B-964C-4FCF-B7B4-4E732C34F45F", DeclarationFilterConstants.DateFilterTypes.EntryReleaseDate);
			}
		}

		protected virtual void AddAdditionalDateFilters(ModuleFilterCollection filters)
		{
		}

		protected virtual void AddSubmittedDate(ModuleFilterCollection filters)
		{
			filters.AddDateFilter(DeclarationFilterConstants.DateFilterTypes.Submitted, JobDeclarationSchema.JE_EntrySubmittedDate).MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|Submitted", DeclarationFilterConstants.DateFilterTypes.Submitted);
		}

		ZQuery GetInvoiceDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var result = new ZQuery();
			AddDateRange(result, comparisonOperator, JoinCondition.And, JobComInvoiceHeaderSchema.JZ_InvoiceDate, value1.Date, value2.Date);
			return result;
		}

		ZQuery GetCommercialInvoicePaymentDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var result = new ZQuery();
			AddDateRange(result, comparisonOperator, JoinCondition.And, JobComInvoiceHeaderSchema.JZ_PaymentDate, value1.Date, value2.Date);
			return result;
		}

		ZQuery GetGoodsDeliveredDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			return GetCartageDateQuery(comparisonOperator, value1, value2, JobDocsAndCartageSchema.JP_DeliveryCartageCompleted);
		}

		ZQuery GetCartageAdvisedDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			return GetCartageDateQuery(comparisonOperator, value1, value2, JobDocsAndCartageSchema.JP_DeliveryCartageAdvised);
		}

		ZQuery GetCartageDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2, SchemaDateTimeColumn dateColumn)
		{
			var dbOnlyResult = new ZDBOnlyQuery(typeof(BaseJobDeclaration));

			var cartageSubQuery = new ZDBOnlySubQuery(typeof(JobDocsAndCartage), JobDocsAndCartageSchema.JP_ParentID);
			AddDateRange(cartageSubQuery, comparisonOperator, JoinCondition.And, dateColumn, value1.Date, value2.Date);

			dbOnlyResult.AddSubQuery(cartageSubQuery, JoinCondition.And);
			dbOnlyResult.AddSubQuery(JobDeclarationSchema.JE_JS, cartageSubQuery, JoinCondition.Or);
			return dbOnlyResult;
		}

		ZQuery GetReleaseDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var result = new ZQuery();
			AddDateRange(result, comparisonOperator, JoinCondition.And, CusEntryHeaderSchema.CH_EntryReleaseDate, value1.Date, value2.Date);
			return result;
		}

		CusEntryHeaderSubGroupForReleaseDateFilter EntryHeaderSubGroupForReleaseDateFilter
		{
			get { return cusEntryHeaderSubGroupForReleaseDateFilter ??= new CusEntryHeaderSubGroupForReleaseDateFilter(); }
		}
		CusEntryHeaderSubGroupForReleaseDateFilter cusEntryHeaderSubGroupForReleaseDateFilter;

		class CusEntryHeaderSubGroupForReleaseDateFilter : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(BaseJobDeclaration));

				var subQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_ClusterKey);
				subQuery.AddToFilter(filter);
				result.AddSubQuery(JobDeclarationSchema.JE_ClusterKey, subQuery, JoinCondition.And);

				return result;
			}
		}

		#region Get Esitmate Date Filter

		ZQuery GetETDOfLoadingFilter(DateComparisonOperator comaprisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var transportQuery = new ZQuery();
			AddDateRange(transportQuery, comaprisonOperator, JoinCondition.And, JobConsolTransportSchema.JW_ETD, value1.Date, value2.Date);

			return GetEsitmateDateQuery(transportQuery, declarationLoadingPortETD, AutoJobDeclaration.Schema.JE_RL_NKPortOfLoading);
		}

		ZQuery GetETAOfDischargeFilter(DateComparisonOperator comaprisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var transportQuery = new ZQuery();
			AddDateRange(transportQuery, comaprisonOperator, JoinCondition.And, JobConsolTransportSchema.JW_ETA, value1.Date, value2.Date);

			return GetEsitmateDateQuery(transportQuery, declarationDischargePortETA, AutoJobDeclaration.Schema.JE_RL_NKPortOfArrival);
		}

		ZQuery GetEsitmateDateQuery(ZQuery transportQuery, string functionName, string portColumnName)
		{
			var sql = string.Format(CultureInfo.InvariantCulture, @"
EXISTS
(
	SELECT
		1
	FROM
		dbo.{0}(JE_PK, JE_JS, {1}) AS FirstEstimateTime
	WHERE
		{2}
)", functionName, portColumnName, transportQuery.LiteralTextSqlFormatted);

			var result = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
			result.AddFilterAndZSQLParameterCollection(sql, new ZSqlParameterCollection());

			return result;
		}

		const string declarationLoadingPortETD = "DeclarationLoadingPortETD";
		const string declarationDischargePortETA = "DeclarationDischargePortETA";

		#endregion

		#region Get Services Date Filter

		ZQuery GetServiceDateBookedFilter(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			return GetServiceDateFilter(comparisonOperator, value1, value2, JobServiceSchema.ES_Booked);
		}

		ZQuery GetServicesCompletedFilter(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			return GetServiceDateFilter(comparisonOperator, value1, value2, JobServiceSchema.ES_Completed);
		}

		ZQuery GetServiceDateFilter(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2, SchemaDateTimeColumn dateColumn)
		{
			var serviceSubQuery = new ZDBOnlySubQuery(typeof(JobService), JobServiceSchema.ES_ParentID);
			AddDateRange(serviceSubQuery, comparisonOperator, JoinCondition.And, dateColumn, value1.Date, value2.Date);

			var cartageSubQuery = new ZDBOnlySubQuery(typeof(JobDocsAndCartage), JobDocsAndCartageSchema.JP_ParentID);
			cartageSubQuery.AddSubQuery(serviceSubQuery, JoinCondition.And);

			var dbOnlyResult = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
			dbOnlyResult.AddSubQuery(cartageSubQuery, JoinCondition.And);
			return dbOnlyResult;
		}

		#endregion

		#endregion

		#region Organisation Filters

		protected virtual void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			AddBranchAndBrokerFilters(filters);
			AddImporterNameFilter(filters);
			AddSupplierNameFilter(filters);
			AddImporterSupplierFilter(filters);
			AddControllingAgentFilter(filters);
			AddControllingCustomerFilter(filters);
			AddExternalBrokerFilter(filters);
			AddServiceProviderFilter(filters);
			AddBillingOrganisationFilter(filters);
			AddDeclarantFilter(filters);
			AddPickupFromNameSearch(filters);
			AddDeliveryToNameSearch(filters);
			AddPickupFromOrganizationFilters(filters);
			AddDeliveryToOrganizationFilters(filters);

			ModuleFilter cartageCofilter = filters.AddGuidFilter(DeclarationFilterConstants.OrgFilterTypes.CartageCoordinator, ModuleIDs.GlbStaff, GetCartageCoordinatorQuery, Lookups.StaffList);
			cartageCofilter.Category = FilterCategories.Organisations;
			cartageCofilter.IsPublishedOnWeb = false;
			cartageCofilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|CartageCoordinator", DeclarationFilterConstants.OrgFilterTypes.CartageCoordinator);

			JobDeclarationClientAssignedStaffModuleFilter clientAssignedStaffFilter = new JobDeclarationClientAssignedStaffModuleFilter(DeclarationFilterConstants.OrgFilterTypes.ClientAssignedStaff);
			clientAssignedStaffFilter.Category = FilterCategories.Organisations;
			clientAssignedStaffFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|ClientAssignedStaff", DeclarationFilterConstants.OrgFilterTypes.ClientAssignedStaff);
			filters.AddCustomFilter(clientAssignedStaffFilter);
		}

		protected void AddBranchAndBrokerFilters(ModuleFilterCollection filters)
		{
			var branchFilter = filters.AddGuidFilter("Declaration Branch", ModuleIDs.GlbBranch, JobDeclarationSchema.JE_GB, Lookups.BranchList);
			branchFilter.Category = FilterCategories.Organisations;
			branchFilter.IsPublishedOnWeb = false;
			branchFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|DeclarationBranch", "Declaration Branch");

			var brokerFilter = filters.AddNkFilter("Customs Broker", JobDeclarationSchema.JE_GS_NKCusAgent, ModuleIDs.GlbStaff, Lookups.StaffList);
			brokerFilter.Category = FilterCategories.Organisations;
			brokerFilter.IsPublishedOnWeb = false;
			brokerFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|CustomsBroker", "Customs Broker");
		}

		protected void AddBillingOrganisationFilter(ModuleFilterCollection filters)
		{
			var branchFilter = filters.AddGuidFilter("Billing Branch", ModuleIDs.GlbBranch, new GetGuidQueryWithOperator((comparisonOperator, value) => GetBillingOrganisationQuery(JobHeaderSchema.JH_GB, comparisonOperator, value)), Lookups.BranchList);
			branchFilter.Category = FilterCategories.Organisations;
			branchFilter.IsPublishedOnWeb = false;
			branchFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|BillingBranch", "Billing Branch");
			branchFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			branchFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);

			var branchCodeFilter = filters.AddTextFilter("Billing Branch Code", new GetTextQueryWithOperator((comparisonOperator, value) => GetBillingOrganisationCodeQuery(typeof(GlbBranch), JobHeaderSchema.JH_GB, GlbBranchSchema.GB_Code, comparisonOperator, value)))
				.WithMaxLengthOf<ModuleTextFilter>(GlbBranchSchema.GB_Code);

			branchCodeFilter.Category = FilterCategories.Organisations;
			branchCodeFilter.IsPublishedOnWeb = false;
			branchCodeFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|BillingBranchCode", "Billing Branch Code");

			var departmentFilter = filters.AddGuidFilter("Billing Department", ModuleIDs.GlbDepartment, new GetGuidQueryWithOperator((comparisonOperator, value) => GetBillingOrganisationQuery(JobHeaderSchema.JH_GE, comparisonOperator, value)), Lookups.DepartmentList);
			departmentFilter.Category = FilterCategories.Organisations;
			departmentFilter.IsPublishedOnWeb = false;
			departmentFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|BillingDepartment", "Billing Department");
			departmentFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			departmentFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);

			var departmentCodeFilter = filters.AddTextFilter("Billing Department Code", new GetTextQueryWithOperator((comparisonOperator, value) => GetBillingOrganisationCodeQuery(typeof(GlbDepartment), JobHeaderSchema.JH_GE, GlbDepartmentSchema.GE_Code, comparisonOperator, value)))
				.WithMaxLengthOf<ModuleTextFilter>(GlbDepartmentSchema.GE_Code);
			departmentCodeFilter.Category = FilterCategories.Organisations;
			departmentCodeFilter.IsPublishedOnWeb = false;
			departmentCodeFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|BillingDepartmentCode", "Billing Department Code");

			var operatorFilter = filters.AddNkFilter("Billing Operator", (value) => GetBillingOrganisationQuery(JobHeaderSchema.JH_GS_NKRepOps, SQLComparisonOperator.Equal, value), ModuleIDs.GlbStaff, Lookups.StaffList);
			operatorFilter.Category = FilterCategories.Organisations;
			operatorFilter.IsPublishedOnWeb = false;
			operatorFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|BillingOperator", "Billing Operator");

			if (AccountingMasterFilesUtils.IsTaxBranchApplicable)
			{
				var taxBranchFilter = filters.AddGuidFilter("Billing Tax Branch", ModuleIDs.GlbBranch, new GetGuidQueryWithOperator((comparisonOperator, value) => GetBillingOrganisationQuery(JobHeaderSchema.JH_GB_TaxBranch, comparisonOperator, value)), Lookups.BranchList);
				taxBranchFilter.Category = FilterCategories.Organisations;
				taxBranchFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|BillingTaxBranch", "Billing Tax Branch");
				taxBranchFilter.IsPublishedOnWeb = false;
			}
		}

		ZQuery GetBillingOrganisationQuery(SchemaColumn schemaColumn, SQLComparisonOperator comparisonOperator, object value)
		{
			var result = new ZDBOnlyQuery(typeof(BaseJobDeclaration));

			var subQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);
			subQuery.AddToFilter(schemaColumn, comparisonOperator, value);
			subQuery.AddToFilter(JoinCondition.And, JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
			subQuery.AddToFilter(JoinCondition.And, JobHeaderSchema.JH_IsActive, true);

			result.AddSubQuery(subQuery, JoinCondition.And);
			result.AddSubQuery(JobDeclarationSchema.JE_JS, subQuery, JoinCondition.Or);

			return result;
		}

		ZQuery GetBillingOrganisationCodeQuery(Type type, SchemaColumn schemaColumn1, SchemaColumn schemaColumn2, SQLComparisonOperator comparisonOperator, object value)
		{
			var subCodeQuery = new ZDBOnlySubQuery(type, schemaColumn1);
			subCodeQuery.AddToFilter(schemaColumn2, comparisonOperator, value);

			var subQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);
			subQuery.AddSubQuery(subCodeQuery, JoinCondition.And);
			subQuery.AddToFilter(JoinCondition.And, JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
			subQuery.AddToFilter(JoinCondition.And, JobHeaderSchema.JH_IsActive, true);

			var result = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
			result.AddSubQuery(subQuery, JoinCondition.And);
			result.AddSubQuery(JobDeclarationSchema.JE_JS, subQuery, JoinCondition.Or);

			return result;
		}

		protected virtual void AddImporterNameFilter(ModuleFilterCollection filters)
		{
			var importerNameFilter = filters.AddTextFilter(DeclarationFilterConstants.OrgFilterTypes.ImporterName, GetImporterSupplierNameQuery(JobDeclarationSchema.JE_OH_Importer));
			importerNameFilter.Category = FilterCategories.Organisations;
			importerNameFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|ImporterName", "Importer Name");
		}

		protected virtual void AddSupplierNameFilter(ModuleFilterCollection filters)
		{
			var supplierNameFilter = filters.AddTextFilter(DeclarationFilterConstants.OrgFilterTypes.SupplierName, GetImporterSupplierNameQuery(JobDeclarationSchema.JE_OH_Supplier));
			supplierNameFilter.Category = FilterCategories.Organisations;
			supplierNameFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|SupplierName", "Supplier Name");
		}

		GetTextQueryWithOperator GetImporterSupplierNameQuery(SchemaColumn column)
		{
			return (SQLComparisonOperator comparisonOperator, ZString value) =>
			{
				var result = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
				var subQuery = new ZDBOnlySubQuery(typeof(OrgHeader), column);
				subQuery.AddToFilter_PossiblyCommaSeparated(OrgHeaderSchema.OH_FullName, comparisonOperator, value);
				result.AddSubQuery(subQuery, JoinCondition.And);
				return result;
			};
		}

		protected virtual void AddImporterSupplierFilter(ModuleFilterCollection filters)
		{
			ModuleGuidsFilter importerSupplierFilter = new ModuleGuidsFilterForOrg(DeclarationFilterConstants.OrgFilterTypes.ImporterSupplier, ModuleIDs.Organisation, GetImporterSupplierQuery, Lookups.FilterConsignees, Lookups.FilterConsignors);
			importerSupplierFilter.SetItemDescriptions(Res.GetData("Customs|DeclarationFilter|Importer", "Importer"), Res.GetData("Customs|DeclarationFilter|Supplier", "Supplier"));
			importerSupplierFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|ImporterSupplier", DeclarationFilterConstants.OrgFilterTypes.ImporterSupplier);
			if (IsForConsolidation)
			{
				importerSupplierFilter.Visibility = FilterVisibility.AlwaysVisible;
			}
			filters.AddFilter(importerSupplierFilter);
		}

		protected virtual void AddServiceProviderFilter(ModuleFilterCollection filters)
		{
			var shipplineLineForwarderFilter = new ModuleGuidsFilterForOrg(DeclarationFilterConstants.OrgFilterTypes.ShippingLineForwarder, ModuleIDs.Organisation, JobDeclarationSchema.JE_OH_ShippingLine, Lookups.FilterShippingLines, JobDeclarationSchema.JE_OH_Forwarder, Lookups.FilterForwarders);
			shipplineLineForwarderFilter.SetItemDescriptions(Res.GetData("Customs|DeclarationFilter|ShippingLine", "Shipping Line"), Res.GetData("Customs|DeclarationFilter|Forwarder", "Forwarder"));
			shipplineLineForwarderFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|ShippingLineForwarder", DeclarationFilterConstants.OrgFilterTypes.ShippingLineForwarder);
			filters.AddFilter(shipplineLineForwarderFilter);
		}

		protected void AddControllingAgentFilter(ModuleFilterCollection filters)
		{
			var controllingAgentFilter = new ModuleGuidFilterForOrg(DeclarationFilterConstants.OrgFilterTypes.ControllingAgent, ModuleIDs.Organisation, JobDeclarationSchema.JE_OH_ControllingAgent, Lookups.FilterControllingAgents);
			controllingAgentFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|ControllingAgent", DeclarationFilterConstants.OrgFilterTypes.ControllingAgent);
			controllingAgentFilter.Category = FilterCategories.Organisations;
			filters.AddFilter(controllingAgentFilter);
		}

		protected void AddControllingCustomerFilter(ModuleFilterCollection filters)
		{
			var controllingCustomerFilter = new ModuleGuidFilterForOrg(DeclarationFilterConstants.OrgFilterTypes.ControllingCustomer, ModuleIDs.Organisation, JobDeclarationSchema.JE_OH_ControllingCustomer, Lookups.FilterControllingCustomers);
			controllingCustomerFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|ControllingCustomer", DeclarationFilterConstants.OrgFilterTypes.ControllingCustomer);
			controllingCustomerFilter.Category = FilterCategories.Organisations;
			filters.AddFilter(controllingCustomerFilter);
		}

		protected void AddExternalBrokerFilter(ModuleFilterCollection filters)
		{
			var externalBrokerFilter = new ModuleGuidFilterForOrg(DeclarationFilterConstants.OrgFilterTypes.ExternalBroker, ModuleIDs.Organisation, JobDeclarationSchema.JE_OH_ExternalBroker, Lookups.FilterExternalBrokers);
			externalBrokerFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|ExternalBroker", DeclarationFilterConstants.OrgFilterTypes.ExternalBroker);
			externalBrokerFilter.Category = FilterCategories.Organisations;
			filters.AddFilter(externalBrokerFilter);
		}

		#region GetCartageCoordinatorQuery

		protected ZQuery GetCartageCoordinatorQuery(ZGuid cartageCoordinatorPK)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(BaseJobDeclaration));

			ZDBOnlySubQuery consigneeQuery = new ZDBOnlySubQuery(typeof(OrgHeader), JobDeclarationSchema.JE_OH_Importer);
			ZDBOnlySubQuery consigneeStaffQuery = new ZDBOnlySubQuery(typeof(OrgStaffAssignments), OrgStaffAssignmentsSchema.O8_OH);
			consigneeStaffQuery.AddToFilter(OrgStaffAssignmentsSchema.O8_GC, GlbCompany.CurrentCompany.PK);
			consigneeStaffQuery.AddToFilter(OrgStaffAssignmentsSchema.O8_Role, StaffAssignmentRoles.Codes.CartageCoordinator);

			ZDBOnlySubQuery staffSubQuery = new ZDBOnlySubQuery(typeof(GlbStaff), GlbStaffSchema.GS_Code);
			staffSubQuery.AddToFilter(GlbStaffSchema.PK, cartageCoordinatorPK);

			consigneeStaffQuery.AddSubQuery(OrgStaffAssignmentsSchema.O8_GS_NKPersonResponsible, staffSubQuery, JoinCondition.And);
			consigneeQuery.AddSubQuery(consigneeStaffQuery, JoinCondition.And);

			ZDBOnlySubQuery consignorQuery = new ZDBOnlySubQuery(typeof(OrgHeader), JobDeclarationSchema.JE_OH_Supplier);
			ZDBOnlySubQuery consignorStaffQuery = new ZDBOnlySubQuery(typeof(OrgStaffAssignments), OrgStaffAssignmentsSchema.O8_OH);
			consignorStaffQuery.AddToFilter(OrgStaffAssignmentsSchema.O8_GC, GlbCompany.CurrentCompany.PK);
			consignorStaffQuery.AddToFilter(OrgStaffAssignmentsSchema.O8_Role, StaffAssignmentRoles.Codes.CartageCoordinator);

			consignorStaffQuery.AddSubQuery(OrgStaffAssignmentsSchema.O8_GS_NKPersonResponsible, staffSubQuery, JoinCondition.And);
			consignorQuery.AddSubQuery(consignorStaffQuery, JoinCondition.And);

			result.AddSubQuery(consigneeQuery, JoinCondition.Or);
			result.AddSubQuery(consignorQuery, JoinCondition.Or);

			return result;
		}

		#endregion

		#region GetImporterSupplierQuery

		protected virtual ZQuery GetImporterSupplierQuery(ZGuid importer, ZGuid supplier)
		{
			ZQuery result = new ZQuery();
			if (!importer.IsEmpty)
			{
				result.AddToFilter(JobDeclarationSchema.JE_OH_Importer, importer);
			}

			if (!supplier.IsEmpty)
			{
				ZDBOnlySubQuery subQueries = new ZDBOnlySubQuery(typeof(BaseJobComInvoiceHeader), JobComInvoiceHeaderSchema.JZ_JE);
				ZDBOnlySubQuery subSubQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), JobDeclarationSchema.PK);
				subSubQuery.AddToFilter(new ZQuery(JobDeclarationSchema.JE_OH_Supplier, supplier));
				subQueries.AddToFilter(JobComInvoiceHeaderSchema.JZ_OH_Supplier, supplier);
				subQueries.AddAsUnionQuery(subSubQuery, true);

				ZDBOnlyQuery dbOnlyResult = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
				dbOnlyResult.AddSubQuery(subQueries, JoinCondition.And);
				result.AddToFilter(dbOnlyResult);
			}

			return result;
		}

		#endregion

		protected virtual bool ShowDeclarantFilter => false;

		void AddDeclarantFilter(ModuleFilterCollection filters)
		{
			if (ShowDeclarantFilter)
			{
				var declarantFilter = new ModuleGuidFilterForOrg(DeclarationFilterConstants.OrgFilterTypes.Declarant, ModuleIDs.Organisation, GetDeclarantQuery, new OrganisationsFindBoxCollection(Factory));
				declarantFilter.MultilingualDescription = ResString.GetMultilingualString("80A91DCC-B577-402A-9329-8DFD63D518DA", DeclarationFilterConstants.OrgFilterTypes.Declarant);
				declarantFilter.Category = FilterCategories.Organisations;
				filters.AddFilter(declarantFilter);
			}
		}

		static ZQuery GetDeclarantQuery(ZGuid value)
		{
			var orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, value);

			var result = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
			result.AddSubQuery(JobDeclarationSchema.JE_OA_DeclarantAddress, orgAddressSubQuery, JoinCondition.And);
			return result;
		}

		#endregion

		#region Location Filters

		void AddLocationFilters(ModuleFilterCollection filters)
		{
			var originDestinationFilter = FilterGenerator.AddOriginDestinationFilter(filters, DeclarationFilterConstants.PortFilterTypes.OriginDestination);
			originDestinationFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|OriginDestination", DeclarationFilterConstants.PortFilterTypes.OriginDestination);

			var loadDischargeFilter = FilterGenerator.AddLoadDischargeFilter(filters, DeclarationFilterConstants.PortFilterTypes.LoadDischarge);
			loadDischargeFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|LoadDischarge", DeclarationFilterConstants.PortFilterTypes.LoadDischarge);
			loadDischargeFilter.SetItemDescriptions(Res.GetData("Customs|DeclarationFilter|Load", DeclarationFilterConstants.PortFilterTypes.Load), Res.GetData("Customs|DeclarationFilter|Discharge", DeclarationFilterConstants.PortFilterTypes.Discharge));

			AddAdditionalLocationFilters(filters);

			var firstArrivalPortFilter = filters.AddNkFilter(DeclarationFilterConstants.PortFilterTypes.PortOfFirstArrival, JobDeclarationSchema.JE_RL_NKPortOfFirstArrival, ModuleIDs.RefUNLOCO, Lookups.PortOfFirstArrivalList);
			firstArrivalPortFilter.Category = FilterCategories.Locations;
			firstArrivalPortFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|PortOfFirstArrival", DeclarationFilterConstants.PortFilterTypes.PortOfFirstArrival);
		}

		protected virtual void AddAdditionalLocationFilters(ModuleFilterCollection filters)
		{
		}

		#endregion

		#region Modes Filters

		protected virtual void AddApplicationCodeFilter(ModuleFilterCollection filters)
		{
			var applicationCodeFilter = new ModuleTextFilter(DeclarationFilterConstants.SubmitType, JobDeclarationSchema.JE_ApplicationCode, Lookups.ApplicationCodeList);
			RemoveCodesExcept(applicationCodeFilter.ComparisonOperator_List, new[] { ModuleTextFilter.ComparisonConstants.Exact });

			applicationCodeFilter.Category = FilterCategories.ModesAndTypes;
			applicationCodeFilter.MultilingualDescription = ApplicationCodeFilterCaption;
			filters.AddFilter(applicationCodeFilter);
		}

		void RemoveCodesExcept(CodeDescriptionPairList list, IEnumerable<string> codes)
		{
			foreach (var code in list.GetAllCodes().Except(codes))
			{
				list.RemoveCode(code);
			}
		}

		protected virtual void AddModeFilters(ModuleFilterCollection filters)
		{
			ModuleFilter containerModeFilter = filters.AddTextFilter(DeclarationFilterConstants.NumberFilterTypes.ContainerModeCustoms, JobDeclarationSchema.JE_ContainerMode, Lookups.ContainerModeList);
			containerModeFilter.Category = FilterCategories.ModesAndTypes;
			containerModeFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|ContainerModeCustoms", DeclarationFilterConstants.NumberFilterTypes.ContainerModeCustoms);

			ModuleFilter flightVoyageVesselFilter = filters.AddTextAndNkFilter(DeclarationFilterConstants.FlightVoyageVessel, GetDeclarationFlightVoyageAndVesselQuery, ModuleIDs.RefVessel, Lookups.VesselList).WithMaxLengthOf(JobDeclarationSchema.JE_VoyageFlightNo, JobDeclarationSchema.JE_VesselName);
			flightVoyageVesselFilter.Category = FilterCategories.ModesAndTypes;
			flightVoyageVesselFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|FlightVoyageVessel", DeclarationFilterConstants.FlightVoyageVessel);

			AddShipmentTypeAndShipmentSubTypeFilters(filters);

			ModuleFilter transportModeFilter = filters.AddTextFilter(DeclarationFilterConstants.TransportMode, JobDeclarationSchema.JE_TransportMode, Lookups.TransportTypeList);
			transportModeFilter.Category = FilterCategories.ModesAndTypes;
			transportModeFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|TransportMode", DeclarationFilterConstants.TransportMode);

			AddServiceLevelFilter(filters);
			AddServiceTypeFilter(filters);

			AddPickupDeliveryDropModeFilters(filters);
			AddPickupDeliveryTransportCompanyFilters(filters);
		}

		protected void AddServiceTypeFilter(ModuleFilterCollection filters)
		{
			var serviceTypeFilter = filters.AddTextFilter(DeclarationFilterConstants.ServiceType, JobServiceSchema.ES_ServiceCode, Lookups.ServiceTypeList);
			serviceTypeFilter.SubGroup = new ServiceTypeSubGroup();
			serviceTypeFilter.Category = FilterCategories.ModesAndTypes;
			serviceTypeFilter.IsPublishedOnWeb = false;
			serviceTypeFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|ServicesType", DeclarationFilterConstants.ServiceType);
		}

		protected JobDeclarationFilterGenerator FilterGenerator => filterGenerator ?? (filterGenerator = new JobDeclarationFilterGenerator(this));
		protected JobDeclarationFilterGenerator filterGenerator;

		class ServiceTypeSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var serviceSubQuery = new ZDBOnlySubQuery(typeof(JobService), JobServiceSchema.ES_ParentID);
				serviceSubQuery.AddToFilter(filter);

				var cartageSubQuery = new ZDBOnlySubQuery(typeof(JobDocsAndCartage), JobDocsAndCartageSchema.JP_ParentID);
				cartageSubQuery.AddSubQuery(serviceSubQuery, JoinCondition.And);

				var dbOnlyResult = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
				dbOnlyResult.AddSubQuery(cartageSubQuery, JoinCondition.And);
				return dbOnlyResult;
			}
		}

		protected ZQuery GetDeclarationFlightVoyageAndVesselQuery(SQLComparisonOperator flightOrVoyageNoComparisonOperator, ZString flightOrVoyageNo, ZString nKVessel)
		{
			var query = new ZQuery();

			if (!flightOrVoyageNo.IsEmpty || flightOrVoyageNoComparisonOperator == SpecialComparisonOperator.IsBlank || flightOrVoyageNoComparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				query.AddToFilter(JobDeclarationSchema.JE_VoyageFlightNo, flightOrVoyageNoComparisonOperator, flightOrVoyageNo);
			}

			if (!nKVessel.IsEmpty || flightOrVoyageNoComparisonOperator == SpecialComparisonOperator.IsBlank || flightOrVoyageNoComparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				query.AddToFilter(JobDeclarationSchema.JE_VesselName, flightOrVoyageNoComparisonOperator, nKVessel);
			}

			return query;
		}

		protected virtual void AddServiceLevelFilter(ModuleFilterCollection filters)
		{
			var serviceLevelModeFilter = filters.AddNkFilter(DeclarationFilterConstants.ServiceLevel, JobDeclarationSchema.JE_RS_NKServiceLevel, ModuleIDs.ServiceLevel, Lookups.ServiceLevelList);
			serviceLevelModeFilter.Category = FilterCategories.ModesAndTypes;
			serviceLevelModeFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|ServiceLevel", DeclarationFilterConstants.ServiceLevel);
		}

		protected virtual void AddShipmentSubTypeFilter(ModuleFilterCollection filters)
		{
			ModuleFilter shipSubTypeFilter = filters.AddTextFilter(DeclarationFilterConstants.ShipmentSubType, JobDeclarationSchema.JE_MessageSubType, Lookups.MessageSubTypeList);
			shipSubTypeFilter.Category = FilterCategories.ModesAndTypes;
			shipSubTypeFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|ShipmentSubType", DeclarationFilterConstants.ShipmentSubType);
		}

		protected virtual void AddShipmentTypeAndShipmentSubTypeFilters(ModuleFilterCollection filters)
		{
			ModuleFilter shipTypeFilter = filters.AddTextFilter(DeclarationFilterConstants.ShipmentType, JobDeclarationSchema.JE_MessageType, Lookups.MessageTypeList);
			shipTypeFilter.Category = FilterCategories.ModesAndTypes;
			shipTypeFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|ShipmentType", DeclarationFilterConstants.ShipmentType);

			AddShipmentSubTypeFilter(filters);
		}

		#endregion

		#region Status / Flags Filters

		void AddStatusAndFlagsFilters(ModuleFilterCollection filters)
		{
			AddEntryStatusFilter(filters);
			var currentCompanyCountry = GlbCompany.CurrentCompany.Country.RN_Code;

			if (!DeclarationStatusHelper.ShouldExcludeMessageStatusFromModuleGrid(currentCompanyCountry, GlbCompany.CurrentCompany.PK))
			{
				AddMessageStatusFilter(filters);
				if (ShouldExcludeComparisonOperatorsFromMessageStatusFilter)
				{
					var messageStatusFilter = filters
						.OfType<ModuleTextFilter>()
						.FirstOrDefault(f => f.Description == DeclarationFilterConstants.MessageStatusText);

					messageStatusFilter?.RemoveComparisonOperatorsLeavingOne(ModuleTextFilter.ComparisonConstants.Exact);
				}
			}

			if (new StmData.Loader(Factory).LoadTop1("JobsWithFCAAndFIFTApportionmentErrorsExist", GlbCompany.CurrentCompany.PK, ZGuid.Empty) != null)
			{
				ModuleFilter showFCAAndFIFTApportionmentFilter = filters.AddFlagsFilter(DeclarationFilterConstants.ShowJobsFCA_FIFTApportionment, new string[] { DeclarationFilterConstants.Show }, new GetFlagsQuery[] { GetFCA_FIFTApportionmentQuery });
				showFCAAndFIFTApportionmentFilter.Category = FilterCategories.StatusAndFlags;
				showFCAAndFIFTApportionmentFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|ShowJobsFCA_FIFTApportionment", DeclarationFilterConstants.ShowJobsFCA_FIFTApportionment);
			}

			if (SupportWHSStatus)
			{
				var whsStatusFilter = filters.AddTextFilter(DeclarationFilterConstants.WHSStatus, GetWarehouseTransactionStatusQuery, Factory.GetCachedValue<WarehouseTransactionStatusList>());
				whsStatusFilter.MaxLength = CusEntryHeaderSchema.CH_WarehouseTransactionStatus.MaxLength;
				whsStatusFilter.Category = FilterCategories.StatusAndFlags;
				whsStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|WHSStatus", DeclarationFilterConstants.WHSStatus);
			}
		}

		protected virtual bool SupportWHSStatus
		{
			get { return false; }
		}

		protected virtual ZQuery GetWarehouseTransactionStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
			var isBlank = comparisonOperator == SpecialComparisonOperator.IsBlank;
			var subQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE, isBlank);
			if (isBlank || comparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				subQuery.AddToFilter(CusEntryHeaderSchema.CH_WarehouseTransactionStatus, SQLComparisonOperator.NotEqual, ZString.Empty);
			}
			else
			{
				subQuery.AddToFilter(CusEntryHeaderSchema.CH_WarehouseTransactionStatus, comparisonOperator, value);
			}
			result.AddSubQuery(subQuery, JoinCondition.And);
			return result;
		}

		protected virtual void AddMessageStatusFilter(ModuleFilterCollection filters)
		{
			var messageStatusfilter = filters.AddTextFilter(DeclarationFilterConstants.MessageStatusText, GetMessageStatusQuery, Lookups.MessageStatusList);
			messageStatusfilter.Category = FilterCategories.StatusAndFlags;
			messageStatusfilter.MultilingualDescription = MessageStatusText;
		}

		protected virtual bool ShouldExcludeComparisonOperatorsFromMessageStatusFilter => true;

		protected virtual ZQuery GetMessageStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			if (value == DeclarationFilterConstants.EntryStatus.NotSentForFilter)
			{
				value = ZString.Empty;
			}

			if (DeclarationStatusHelper.ShouldCombineMessageStatusFromHeaders(GlbCompany.CurrentCompany.Country.RN_Code))
			{
				var result = new ZQuery();

				var declarationResult = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
				declarationResult.AddToFilter(JobDeclarationSchema.JE_MessageStatus, comparisonOperator, value);
				var messageStatusFilter = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE, true);
				messageStatusFilter.AddToFilter(CusEntryHeaderSchema.CH_Status, SQLComparisonOperator.NotEqual, ZString.Empty);
				declarationResult.AddSubQuery(messageStatusFilter, JoinCondition.And);
				result.AddToFilter(declarationResult, JoinCondition.And);

				var entryHeaderResult = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
				var cusEntryFilter = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE);
				cusEntryFilter.AddToFilter(CusEntryHeaderSchema.CH_Status, comparisonOperator, value);
				entryHeaderResult.AddSubQuery(cusEntryFilter, JoinCondition.And);
				result.AddToFilter(entryHeaderResult, JoinCondition.Or);

				return result;
			}
			else
			{
				return new ZQuery(JobDeclarationSchema.JE_MessageStatus, comparisonOperator, value);
			}
		}

		public virtual MultilingualString MessageStatusText => ResString.GetMultilingualString("Customs|DeclarationFilter|MessageStatusText", "Message Status");

		protected virtual bool ShouldCombineEntryStatusFromHeaders => DeclarationStatusHelper.ShouldCombineEntryStatusFromHeaders(GlbCompany.CurrentCompany.Country.RN_Code, GlbCompany.CurrentCompany.PK);

		protected virtual void AddEntryStatusFilter(ModuleFilterCollection filters)
		{
			var showFilterType = ShouldCombineEntryStatusFromHeaders;

			var entryStatusFilter = new EntryStatusFilter(DeclarationFilterConstants.EntryStatusText,
				new JobDeclarationEntryStatusFilterHelper(GetEntryStatusQuery, JobDeclarationFilter.GetEntryStatusQueryAllEntries).GetEntryStatusFilter, lookups.EntryStatusList, useFilterType: true).WithMaxLengthOf<EntryStatusFilter>(CusEntryHeaderSchema.CH_EntryStatus);
			entryStatusFilter.ShowFilterType = showFilterType;

			entryStatusFilter.MultilingualDescription = ResString.GetMultilingualString("048044B8-E55D-419D-AAB2-85BE08F96884", DeclarationFilterConstants.EntryStatusText);
			entryStatusFilter.ComparisonOperatorChanged += CustomsEntryStatusFilter_ComparisonOperatorChanged;

			filters.AddCustomFilter(entryStatusFilter);

			if (IsForConsolidation)
			{
				using (entryStatusFilter.GetValidationSuspender())
				{
					entryStatusFilter.Property = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;
					entryStatusFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
				}
			}
		}

		protected void CustomsEntryStatusFilter_ComparisonOperatorChanged(object sender, EventArgs e)
		{
			if (sender is ModuleTextBaseFilter entryStatusFilter && !entryStatusFilter.ComparisonOperator_List.ContainsCode(entryStatusFilter.ComparisonOperator))
			{
				entryStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			}
		}

		protected virtual ZQuery GetEntryStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			if (value == DeclarationFilterConstants.EntryStatus.NotSentForFilter)
			{
				value = ZString.Empty;
			}

			if (ShouldCombineEntryStatusFromHeaders)
			{
				var result = new ZQuery();

				var declarationResult = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
				declarationResult.AddToFilter(JobDeclarationSchema.JE_EntryStatus, comparisonOperator, value);
				var entryStatusFilter = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE, true);
				entryStatusFilter.AddToFilter(CusEntryHeaderSchema.CH_EntryStatus, SQLComparisonOperator.NotEqual, ZString.Empty);
				declarationResult.AddSubQuery(entryStatusFilter, JoinCondition.And);
				result.AddToFilter(declarationResult, JoinCondition.And);

				var entryHeaderResult = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
				var cusEntryFilter = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE);
				cusEntryFilter.AddToFilter(CusEntryHeaderSchema.CH_EntryStatus, comparisonOperator, value);
				entryHeaderResult.AddSubQuery(cusEntryFilter, JoinCondition.And);
				result.AddToFilter(entryHeaderResult, JoinCondition.Or);

				return result;
			}
			else
			{
				return new ZQuery(JobDeclarationSchema.JE_EntryStatus, comparisonOperator, value);
			}
		}

		public virtual MultilingualString EntryStatusText => ResString.GetMultilingualString("Customs|DeclarationFilter|EntryStatusText", "Entry Status");

		ZQuery GetFCA_FIFTApportionmentQuery(ZBool value)
		{
			ZDBOnlyQuery decQuery = new ZDBOnlyQuery(typeof(BaseJobDeclaration));

			if (value)
			{
				ZDBOnlySubQuery invoiceQuery = new ZDBOnlySubQuery(typeof(BaseJobComInvoiceHeader), JobComInvoiceHeaderSchema.JZ_JE);
				invoiceQuery.AddToFilter(JobComInvoiceHeaderSchema.JZ_IncoTerm, Core.Constants.IncoTerms.FreeCarrier);

				ZDBOnlySubQuery invoiceChargeQuery = new ZDBOnlySubQuery(typeof(BaseJobComInvHeaderCharge), JobComInvHeaderChargeSchema.J7_ParentID);
				invoiceChargeQuery.AddToFilter(JobComInvHeaderChargeSchema.J7_ChargeType, Enterprise.Customs.Common.CustomsChargeTypeList.Codes.ForeignInlandFreight);
				invoiceChargeQuery.AddToFilter(JobComInvHeaderChargeSchema.J7_IsIncludedInITOT, "Y");
				invoiceChargeQuery.AddToFilter(JobComInvHeaderChargeSchema.J7_IsApportionedCharge, "Y");

				invoiceQuery.AddSubQuery(invoiceChargeQuery, JoinCondition.And);

				decQuery.AddSubQuery(invoiceQuery, JoinCondition.And);
			}

			return decQuery;
		}

		#endregion

		#region Last Audit Filters

		protected void AddAuditFilters(ModuleFilterCollection filters)
		{
			ModuleDateFilter auditDateFilter = filters.AddDateFilter(DeclarationFilterConstants.LastAuditFilterTypes.LastAuditDate, JobDeclarationSchema.JE_AuditDateUtc, true);
			auditDateFilter.Category = FilterCategories.AuditInformation;
			auditDateFilter.IsPublishedOnWeb = false;
			auditDateFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|LastAuditDate", DeclarationFilterConstants.LastAuditFilterTypes.LastAuditDate);

			ModuleNumberFilter auditReferenceFilter = filters.AddNumberFilter(DeclarationFilterConstants.LastAuditFilterTypes.LastAuditReference, JobDeclarationSchema.JE_AuditReference);
			auditReferenceFilter.Category = FilterCategories.AuditInformation;
			auditReferenceFilter.IsPublishedOnWeb = false;
			auditReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|LastAuditReference", DeclarationFilterConstants.LastAuditFilterTypes.LastAuditReference);

			ModuleFilter auditUserFilter = filters.AddNkFilter(DeclarationFilterConstants.LastAuditFilterTypes.LastAuditLogUser, JobDeclarationSchema.JE_GS_NKAuditUser, ModuleIDs.GlbStaff, Lookups.StaffList);
			auditUserFilter.Category = FilterCategories.AuditInformation;
			auditUserFilter.IsPublishedOnWeb = false;
			auditUserFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|LastAuditLogUser", DeclarationFilterConstants.LastAuditFilterTypes.LastAuditLogUser);

			ModuleNumberFilter auditUserNameFilter = filters.AddNumberFilter(DeclarationFilterConstants.LastAuditFilterTypes.LastAuditLogUserName, GetAuditUserNameQuery);
			auditUserNameFilter.Category = FilterCategories.AuditInformation;
			auditUserNameFilter.IsPublishedOnWeb = false;
			auditUserNameFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|LastAuditLogUserName", DeclarationFilterConstants.LastAuditFilterTypes.LastAuditLogUserName);
		}

		protected ZQuery GetAuditUserNameQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
			if (comparisonOperator != SpecialComparisonOperator.IsBlank)
			{
				var staffSubQuery = new ZDBOnlySubQuery(typeof(GlbStaff), GlbStaffSchema.GS_Code);
				staffSubQuery.AddToFilter_PossiblyCommaSeparated(GlbStaffSchema.GS_FullName, comparisonOperator, value);
				result.AddSubQuery(JobDeclarationSchema.JE_GS_NKAuditUser, staffSubQuery, JoinCondition.And);
			}
			else
			{
				result.AddToFilter(JobDeclarationSchema.JE_GS_NKAuditUser, ZString.Empty);
			}

			return result;
		}

		#endregion

		#region Billing Filters

		protected void AddBillingFilters(ModuleFilterCollection filters)
		{
			AccountingFilterStrip.AddBillingFilters(filters);
		}

		#endregion

		protected void AddJobManagementFilters(ModuleFilterCollection filters, SecurityCheckpoint jobManagementSecurity)
		{
			AccountingFilterStrip.AddJobManagementFilters(filters, jobManagementSecurity);
		}

		#region Related Containers Filter

		void AddRelatedContainersFilter(ModuleFilterCollection filters)
		{
			var filter = new RelatedContainersOfJobDeclarationFilter(DeclarationFilterConstants.RelatedContainers, () => new CommonContainerCollection(Factory));
			filter.IsPublishedOnWeb = false;
			filter.MultilingualDescription = ResString.GetMultilingualString("Customs|JobDeclarationFilter|RelatedContainers", "Related Containers");

			var description = ResString.GetMultilingualString("Customs|JobDeclarationFilter|ContainerCategory", "Container");
			filter.Category = FilterCategories.GetOrCreateFilterCategory(description);

			filters.AddFilter(filter);
		}

		#endregion

		#region Related Transport Bookings Filter

		void AddRelatedTransportBookingsFilter(ModuleFilterCollection filters)
		{
			var filter = new RelatedTransportBookingsOfJobDeclarationFilter(DeclarationFilterConstants.RelatedTransportBookings, () => new DtbBookingCollection(Factory));
			filter.IsPublishedOnWeb = false;
			filter.MultilingualDescription = ResString.GetMultilingualString("Customs|JobDeclarationFilter|RelatedTransportBookings", DeclarationFilterConstants.RelatedTransportBookings);
			filter.Category = FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("Customs|JobDeclarationFilter|TransportBookings", "Transport Bookings"));
			filters.AddFilter(filter);
		}

		#endregion

		#region Related HVLV Filters

		void AddHVLVFilters(ModuleFilterCollection filters)
		{
			ObjectFactory.Get<IHVLVFilterProviderForDeclaration>().AddHVLVFilters(filters, Factory);
		}

		#endregion

		#region Pickup/Delivery Filters

		protected virtual DocAddressType PickupOrganizationJobDocAddressType => DocAddressType.SupplierPickupDeliveryAddress;

		protected virtual DocAddressType DeliveryToOrganizationJobDocAddressType => DocAddressType.ImporterPickupDeliveryAddress;

		#region Drop Mode filters

		protected void AddPickupDeliveryDropModeFilters(ModuleFilterCollection filters)
		{
			var pickupDropModeFilter = filters.AddTextFilter(DeclarationFilterConstants.ModeFilterTypes.PickupDropMode, JobDocsAndCartageSchema.JP_FCLPickupEquipmentNeeded, Lookups.DropModeList);
			pickupDropModeFilter.Category = FilterCategories.ModesAndTypes;
			pickupDropModeFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|PickupDropMode", DeclarationFilterConstants.ModeFilterTypes.PickupDropMode);
			pickupDropModeFilter.SubGroup = JobDocsAndCartageSubGroup;

			var deliveryDropModeFilter = filters.AddTextFilter(DeclarationFilterConstants.ModeFilterTypes.DeliveryDropMode, JobDocsAndCartageSchema.JP_FCLDeliveryEquipmentNeeded, Lookups.DropModeList);
			deliveryDropModeFilter.Category = FilterCategories.ModesAndTypes;
			deliveryDropModeFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|DeliveryDropMode", DeclarationFilterConstants.ModeFilterTypes.DeliveryDropMode);
			deliveryDropModeFilter.SubGroup = JobDocsAndCartageSubGroup;
		}

		#endregion

		#region Transport Company Filters

		protected void AddPickupDeliveryTransportCompanyFilters(ModuleFilterCollection filters)
		{
			var pickupTransportCompany = new ModuleGuidFilterForOrg(DeclarationFilterConstants.OrgFilterTypes.PickupTransportCompany, ModuleIDs.Organisation, GetCartageCompanyQueryComparisonDelegate(JobDocsAndCartageSchema.JP_OA_PickupCartageCoAddr), Lookups.CartageList);
			pickupTransportCompany.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|PickupTransportCompany", DeclarationFilterConstants.OrgFilterTypes.PickupTransportCompany);
			pickupTransportCompany.SupportsBlankComparisonOperators = true;
			pickupTransportCompany.SubGroup = JobDocsAndCartageSubGroup;
			filters.AddFilter(pickupTransportCompany);

			var deliveryTransportCompany = new ModuleGuidFilterForOrg(DeclarationFilterConstants.OrgFilterTypes.DeliveryTransportCompany, ModuleIDs.Organisation, GetCartageCompanyQueryComparisonDelegate(JobDocsAndCartageSchema.JP_OA_DeliveryCartageCoAddr), Lookups.CartageList);
			deliveryTransportCompany.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|DeliveryTransportCompany", DeclarationFilterConstants.OrgFilterTypes.DeliveryTransportCompany);
			deliveryTransportCompany.SupportsBlankComparisonOperators = true;
			deliveryTransportCompany.SubGroup = JobDocsAndCartageSubGroup;
			filters.AddFilter(deliveryTransportCompany);
		}

		#endregion

		#region Organization Filters

		void AddPickupFromOrganizationFilters(ModuleFilterCollection filters)
		{
			var pickupFilter = filters.AddGuidFilter(
				DeclarationFilterConstants.OrgFilterTypes.PickupFrom,
				ModuleIDs.Organisation,
				GetPickupFrom_DeliveryTo_OrganizationFromJobDocAddressDelegate(PickupOrganizationJobDocAddressType, DocAddressType.ConsignorPickupDeliveryAddress),
				Lookups.Consignors);
			pickupFilter.Category = FilterCategories.Organisations;
			pickupFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|PickupFrom", DeclarationFilterConstants.OrgFilterTypes.PickupFrom);
			pickupFilter.SupportsBlankComparisonOperators = true;
		}

		void AddDeliveryToOrganizationFilters(ModuleFilterCollection filters)
		{
			var pickupFilter = filters.AddGuidFilter(
				DeclarationFilterConstants.OrgFilterTypes.DeliveryTo,
				ModuleIDs.Organisation,
				GetPickupFrom_DeliveryTo_OrganizationFromJobDocAddressDelegate(DeliveryToOrganizationJobDocAddressType, DocAddressType.ConsigneePickupDeliveryAddress),
				Lookups.Consignees);
			pickupFilter.Category = FilterCategories.Organisations;
			pickupFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|DeliveryTo", DeclarationFilterConstants.OrgFilterTypes.DeliveryTo);
			pickupFilter.SupportsBlankComparisonOperators = true;
		}

		GetGuidQueryWithOperator GetPickupFrom_DeliveryTo_OrganizationFromJobDocAddressDelegate(DocAddressType declarationDocAddressType, DocAddressType shipmentDocAddressType)
		{
			return (SQLComparisonOperator comparisonOperator, object value) =>
			{
				var notIn = comparisonOperator == SQLComparisonOperator.NotEqual || comparisonOperator == SQLComparisonOperator.IsBlank;
				var result = new ZDBOnlyQuery(typeof(BaseJobDeclaration));

				var declarationJobDocAddressQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID, notIn);
				declarationJobDocAddressQuery.AddToFilter(JoinCondition.And, JobDocAddressSchema.E2_ParentTableCode, JobDeclarationSchema.Constants.Prefix);
				declarationJobDocAddressQuery.AddToFilter(JoinCondition.And, JobDocAddressSchema.E2_AddressOverride, false);
				declarationJobDocAddressQuery.AddToFilter(JoinCondition.And, JobDocAddressSchema.E2_AddressType, DocAddressTypes.GetCode(Factory, declarationDocAddressType));

				var shipmentJobDocAddressQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID, notIn);
				shipmentJobDocAddressQuery.AddToFilter(JoinCondition.And, JobDocAddressSchema.E2_ParentTableCode, JobShipmentSchema.Constants.Prefix);
				shipmentJobDocAddressQuery.AddToFilter(JoinCondition.And, JobDocAddressSchema.E2_AddressOverride, false);
				shipmentJobDocAddressQuery.AddToFilter(JoinCondition.And, JobDocAddressSchema.E2_AddressType, DocAddressTypes.GetCode(Factory, shipmentDocAddressType));

				if (comparisonOperator == SQLComparisonOperator.Equal || comparisonOperator == SQLComparisonOperator.NotEqual)
				{
					var oaQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);
					oaQuery.AddToFilter(OrgAddressSchema.OA_OH, value);

					declarationJobDocAddressQuery.AddSubQuery(oaQuery, JoinCondition.And);
					shipmentJobDocAddressQuery.AddSubQuery(oaQuery, JoinCondition.And);
				}
				else
				{
					declarationJobDocAddressQuery.AddToFilter(JoinCondition.And, JobDocAddressSchema.E2_OA_Address, SQLComparisonOperator.NotEqual, DBNull.Value);
					shipmentJobDocAddressQuery.AddToFilter(JoinCondition.And, JobDocAddressSchema.E2_OA_Address, SQLComparisonOperator.NotEqual, DBNull.Value);
				}

				result.AddSubQuery(declarationJobDocAddressQuery, JoinCondition.And);

				if (notIn)
				{
					var jejsNotNullQuery = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
					jejsNotNullQuery.AddSubQuery(JobDeclarationSchema.JE_JS, shipmentJobDocAddressQuery, JoinCondition.And);
					var jejsNullQuery = new ZQuery(JobDeclarationSchema.JE_JS, null);
					var combineQuery = new ZQuery(jejsNotNullQuery, JoinCondition.Or, jejsNullQuery);

					result.AddToFilter(combineQuery, JoinCondition.And);
				}
				else
				{
					result.AddSubQuery(JobDeclarationSchema.JE_JS, shipmentJobDocAddressQuery, JoinCondition.Or);
				}

				return result;
			};
		}

		#endregion

		#region Name Filters

		void AddPickupFromNameSearch(ModuleFilterCollection filters)
		{
			var pickupFromNameFilter = filters.AddTextFilter(DeclarationFilterConstants.OrgFilterTypes.PickupFromName, GetPickupFromName_DeliveryToNameQuery(PickupOrganizationJobDocAddressType, DocAddressType.ConsignorPickupDeliveryAddress));
			pickupFromNameFilter.Category = FilterCategories.Organisations;
			pickupFromNameFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|PickupFromName", DeclarationFilterConstants.OrgFilterTypes.PickupFromName);
		}

		void AddDeliveryToNameSearch(ModuleFilterCollection filters)
		{
			var pickupFromNameFilter = filters.AddTextFilter(DeclarationFilterConstants.OrgFilterTypes.DeliveryToName, GetPickupFromName_DeliveryToNameQuery(DeliveryToOrganizationJobDocAddressType, DocAddressType.ConsigneePickupDeliveryAddress));
			pickupFromNameFilter.Category = FilterCategories.Organisations;
			pickupFromNameFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|DeliveryToName", DeclarationFilterConstants.OrgFilterTypes.DeliveryToName);
		}

		GetTextQueryWithOperator GetPickupFromName_DeliveryToNameQuery(DocAddressType declarationDocAddressType, DocAddressType shipmentDocAddressType)
		{
			return (SQLComparisonOperator comparisonOperator, ZString value) =>
			{
				var notIn = comparisonOperator == SQLComparisonOperator.DoesNotStartWith || comparisonOperator == SQLComparisonOperator.NotContains || comparisonOperator == SQLComparisonOperator.NotEqual || comparisonOperator == SQLComparisonOperator.IsBlank;
				var queryOperator = comparisonOperator;
				if (comparisonOperator == SQLComparisonOperator.DoesNotStartWith)
				{
					queryOperator = SQLComparisonOperator.StartsWith;
				}
				else if (comparisonOperator == SQLComparisonOperator.NotContains)
				{
					queryOperator = SQLComparisonOperator.Contains;
				}
				else if (comparisonOperator == SQLComparisonOperator.NotEqual)
				{
					queryOperator = SQLComparisonOperator.Equal;
				}
				else if (comparisonOperator == SQLComparisonOperator.IsBlank)
				{
					queryOperator = SQLComparisonOperator.IsNotBlank;
				}

				var result = new ZDBOnlyQuery(typeof(BaseJobDeclaration));

				var ohQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgAddressSchema.OA_OH);
				ohQuery.AddToFilter(OrgHeaderSchema.OH_FullName, queryOperator, value);

				var oaQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);
				oaQuery.AddSubQuery(ohQuery, JoinCondition.And);

				var jobDocAddressQueryNotOverrided = new ZDBOnlyQuery(typeof(JobDocAddress));
				jobDocAddressQueryNotOverrided.AddToFilter(JoinCondition.And, JobDocAddressSchema.E2_AddressOverride, false);
				jobDocAddressQueryNotOverrided.AddSubQuery(oaQuery, JoinCondition.And);

				var jobDocAddressQueryOverrided = new ZDBOnlyQuery(typeof(JobDocAddress));
				jobDocAddressQueryOverrided.AddToFilter(JoinCondition.And, JobDocAddressSchema.E2_AddressOverride, true);
				jobDocAddressQueryOverrided.AddToFilter(JoinCondition.And, JobDocAddressSchema.E2_CompanyName, queryOperator, value);

				var jobDocAddressQuery = new ZQuery(jobDocAddressQueryNotOverrided, JoinCondition.Or, jobDocAddressQueryOverrided);

				var declarationJobDocAddressQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID, notIn);
				declarationJobDocAddressQuery.AddToFilter(JoinCondition.And, JobDocAddressSchema.E2_ParentTableCode, JobDeclarationSchema.Constants.Prefix);
				declarationJobDocAddressQuery.AddToFilter(JoinCondition.And, JobDocAddressSchema.E2_AddressType, DocAddressTypes.GetCode(Factory, declarationDocAddressType));
				declarationJobDocAddressQuery.AddToFilter(jobDocAddressQuery);

				result.AddSubQuery(declarationJobDocAddressQuery, JoinCondition.And);

				var shipmentJobDocAddressQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID, notIn);
				shipmentJobDocAddressQuery.AddToFilter(JoinCondition.And, JobDocAddressSchema.E2_ParentTableCode, JobShipmentSchema.Constants.Prefix);
				shipmentJobDocAddressQuery.AddToFilter(JoinCondition.And, JobDocAddressSchema.E2_AddressType, DocAddressTypes.GetCode(Factory, shipmentDocAddressType));
				shipmentJobDocAddressQuery.AddToFilter(jobDocAddressQuery);

				if (notIn)
				{
					var jejsNotNullQuery = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
					jejsNotNullQuery.AddSubQuery(JobDeclarationSchema.JE_JS, shipmentJobDocAddressQuery, JoinCondition.And);
					var jejsNullQuery = new ZQuery(JobDeclarationSchema.JE_JS, null);
					var combineQuery = new ZQuery(jejsNotNullQuery, JoinCondition.Or, jejsNullQuery);

					result.AddToFilter(combineQuery, JoinCondition.And);
				}
				else
				{
					result.AddSubQuery(JobDeclarationSchema.JE_JS, shipmentJobDocAddressQuery, JoinCondition.Or);
				}

				return result;
			};
		}

		#endregion

		#region Queries

		#region GetCartageCompanyQueryComparisonDelegate

		GetGuidQueryWithOperator GetCartageCompanyQueryComparisonDelegate(SchemaColumn addressColumn)
		{
			return (SQLComparisonOperator comparisonOperator, object pK) =>
			{
				var jobDocsAndCartageQuery = new ZDBOnlyQuery(typeof(JobDocsAndCartage));

				if (comparisonOperator != SpecialComparisonOperator.IsBlank && comparisonOperator != SpecialComparisonOperator.IsNotBlank)
				{
					var addressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), addressColumn);
					addressQuery.AddToFilter(OrgAddressSchema.OA_OH, comparisonOperator, (ZGuid)pK);
					jobDocsAndCartageQuery.AddSubQuery(addressQuery, JoinCondition.And);
				}
				else
				{
					jobDocsAndCartageQuery.AddToFilter(addressColumn, comparisonOperator == SQLComparisonOperator.IsBlank ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual, null);
				}
				return jobDocsAndCartageQuery;
			};
		}

		#endregion

		#endregion

		#region Subgroup

		ModuleFilterSubGroup JobDocsAndCartageSubGroup
		{
			get { return jobDocsAndCartageSubGroup ?? (jobDocsAndCartageSubGroup = new JobDocsAndCartageFilterSubGroup()); }
		}
		JobDocsAndCartageFilterSubGroup jobDocsAndCartageSubGroup;

		class JobDocsAndCartageFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var jobDocsAndCartageQuery = new ZDBOnlySubQuery(typeof(JobDocsAndCartage), JobDocsAndCartageSchema.JP_ParentID);
				jobDocsAndCartageQuery.AddToFilter(filter);

				var result = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
				result.AddSubQuery(jobDocsAndCartageQuery, JoinCondition.And);
				result.AddSubQuery(JobDeclarationSchema.JE_JS, jobDocsAndCartageQuery, JoinCondition.Or);

				return result;
			}
		}

		#endregion

		#endregion

		#region Comercial Invoice Line Custom Fields

		protected void AddCommercialInvoiceAttributeFilter(ModuleFilterCollection filters)
		{
			filters.AddAttributeFilters(new AttributeManager().GetAllAttributes(AttributeManager.AttributeModules.CommercialInvoice, null, LoggedInWebUsersOrg), FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("6274C333-55F4-4381-812A-41A540248E6B", "Commercial Invoice Attribute Search")), new CommericalInvoiceAttributeFilterGroup());
		}

		public OrgHeader LoggedInWebUsersOrg
		{
			get { return fLoggedInWebUsersOrg; }
			set { fLoggedInWebUsersOrg = value; }
		}
		OrgHeader fLoggedInWebUsersOrg;

		class CommericalInvoiceAttributeFilterGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlySubQuery jobDeclarationSubQuery = ObjectFactory.Get<ICustomsFilterProvider>().GetJobComInvoiceHeaderFromCommercialInvoiceQueryWithLineFilterAttachedToJobDeclaration(filter);
				ZDBOnlyQuery jobDeclarationQuery = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
				jobDeclarationQuery.AddSubQuery(jobDeclarationSubQuery, JoinCondition.And);
				return jobDeclarationQuery;
			}
		}

		protected ZQuery GetCommercialInvoiceQuery(ZQuery invoiceLineFilter)
		{
			ZDBOnlySubQuery jobDeclarationSubQuery = ObjectFactory.Get<ICustomsFilterProvider>().GetJobComInvoiceHeaderFromCommercialInvoiceQueryWithLineFilterAttachedToJobDeclaration(invoiceLineFilter);
			ZDBOnlyQuery jobDeclarationQuery = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
			jobDeclarationQuery.AddSubQuery(jobDeclarationSubQuery, JoinCondition.And);
			return jobDeclarationQuery;
		}

		#endregion

		#region GetCompanyQuery

		protected ZQuery GetCompanyQuery()
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
			ZDBOnlySubQuery companyQuery = new ZDBOnlySubQuery(typeof(GlbBranch), JobDeclarationSchema.JE_GB);
			companyQuery.AddToFilter(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);

			result.AddSubQuery(companyQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#region IAccountingFilterStripHolder

		IAccountingFilterStrip AccountingFilterStrip
		{
			get
			{
				return accountingFilterStrip_innerValue
					?? (accountingFilterStrip_innerValue = CreateAccountingFilterStrip());
			}
		}
		IAccountingFilterStrip accountingFilterStrip_innerValue;

		protected virtual IAccountingFilterStrip CreateAccountingFilterStrip()
		{
			var accountingFilterStrip = (IAccountingFilterStrip)Activator.CreateInstance(ObjectFactory.GetType<IAccountingFilterStrip>(), this);
			accountingFilterStrip.Initialize(addOrganisationFilters: false, addDateFilters: false, addAmountFilters: true, addNumbersAndReferencesFilters: false, addProfitLossReasonFilters: true);
			return accountingFilterStrip;
		}

		ZQuery IAccountingFilterStripHolder.TopLevelBusinessObjectQuery(ZDBOnlySubQuery billingPKSubQuery)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
			result.AddSubQuery(billingPKSubQuery, JoinCondition.And);

			return result;
		}

		ZBool IAccountingFilterStripHolder.IsFilterStripForParentTable => true;

		Dictionary<string, object> IAccountingFilterStripHolder.AccountingFilterStripConfiguration => new Dictionary<string, object>()
		{
			{ AccountingFilterStripConfigurationKeys.BusinessObjectType, typeof(BaseJobDeclaration) },
			{ AccountingFilterStripConfigurationKeys.JobHeaderBusinessObjectType, typeof(BaseJobDeclaration) },
			{ AccountingFilterStripConfigurationKeys.JobHeaderAdditionalKeyColumn, JobDeclarationSchema.JE_JS },
			{ AccountingFilterStripConfigurationKeys.InvoicingJobStatusFilterNameOverride, ResString.GetMultilingualString("Customs|JobDeclarationFilter|JobStatus", "Job Status") }
		};

		MultilingualString IAccountingFilterStripHolder.AmountFiltersCategoryNameOveride
		{
			get { return null; }
		}

		MultilingualString IAccountingFilterStripHolder.BillingFiltersCategoryNameOveride
		{
			get { return null; }
		}

		MultilingualString IAccountingFilterStripHolder.FilterNameSuffixInOtherCategories
		{
			get { return null; }
		}

		#endregion

		#region Query Helpers

		protected ModuleTextFilter GetAddInfoTextFilter(ZString description, string addInfoPropertyName)
		{
			return new AddInfoModuleTextFilter(description, JobDeclarationSchema.JE_AddInfo, addInfoPropertyName);
		}

		protected ModuleTextFilter GetAddInfoTextFilter(ZString description, string addInfoPropertyName, IList list)
		{
			return new AddInfoModuleTextFilter(description, JobDeclarationSchema.JE_AddInfo, addInfoPropertyName, list);
		}

		#endregion

		#region IGridColourAdditionalModuleIdFilterSupporter Members

		void IGridColourAdditionalModuleIdFilterSupporter.AddAdditionalFilter(ZQuery moduleIdQuery, SchemaColumn schemaColumn)
		{
			if (schemaColumn == StmModuleFilterSchema.S9_ModuleID)
			{
				moduleIdQuery.AddToFilter(JoinCondition.Or, schemaColumn, SQLComparisonOperator.Equal, typeof(BaseJobDeclaration).FullName + "|" + StmModuleFilter.ModuleIdSuffix.GridColorScheme);
			}
		}

		#endregion

		#region CRM Security

		readonly JobDeclarationCRMSecurityProvider securityProvider = new JobDeclarationCRMSecurityProvider();

		#endregion

		public virtual MultilingualString ApplicationCodeFilterCaption => ResString.GetMultilingualString("Customs|DeclarationFilter|SubmitType", "Submit Type");

		protected bool IsForConsolidation => ParentModuleID == ModuleIDs.Customs.ConsolidatedDeclaration;

		#region Overrides of FilterStripBusinessObject

		protected override ModuleFilterCollection GetModuleFiltersFromGlowCore()
		{
			var moduleFilterFromGlow = base.GetModuleFiltersFromGlowCore();

			foreach (var filterHandler in FilterHandlers)
			{
				filterHandler.Add(moduleFilterFromGlow);
			}

			return moduleFilterFromGlow;
		}

		public IReadOnlyCollection<IIndexFilterHandler> FilterHandlers => filterHandlers ??= GetFilterHandlers();

		IReadOnlyCollection<IIndexFilterHandler> filterHandlers;

		IReadOnlyCollection<IIndexFilterHandler> GetFilterHandlers()
		{
			return
			[
				new CurrentCompanyFilterHandler(this),
				new AgentsReferenceFilterHandler(this),
				new ContainerNumberFilterHandler(this),
				new InvoiceNumberFilterHandler(this)
			];
		}

		#endregion
	}
}
