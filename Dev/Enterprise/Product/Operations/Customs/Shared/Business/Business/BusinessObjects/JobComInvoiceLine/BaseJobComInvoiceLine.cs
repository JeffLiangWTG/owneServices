using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Integration.BondedWarehouse;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;
using DefaultOptions = Enterprise.Core.Constants.Customs.ASNRefreshDefaultsOptions;
using JobComInvCharge = Enterprise.Customs.Common.JobComInvCharge;
using MetaData = CargoWise.ComponentModel.MetaData;

namespace Enterprise.Customs.Business
{
	[UserDefinedValues]
	[TestExcludeWorkflowProviderHasTestCase]
	[SingleObjectAroundARow]
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	[UniversalCopyIgnoreElement("CusEntryLine", JobComInvoiceLineSchema.Constants.JI_CL, JobComInvoiceLineSchema.Constants.JI_CEI)]
	[UniversalCopyWithExtendedEntities(FinishCopyMethod = "FinishUniversalCopy")]
	[UniversalCopyExtraCollection("CustomFields", "IAddOnValue", GenCustomAddOnValueSchema.Constants.TableName, GenCustomAddOnValueSchema.Constants.XV_ParentID, GenCustomAddOnValueSchema.Constants.XV_ParentTableCode)]
	[UniversalCopyExtraCollection("Charges", "IJobComInvHeaderCharge", JobComInvHeaderChargeSchema.Constants.TableName, JobComInvHeaderChargeSchema.Constants.J7_ParentID, JobComInvHeaderChargeSchema.Constants.J7_ParentTableCode)]
	[UniversalCopyAddInfo]
	public class BaseJobComInvoiceLine
		: AutoJobComInvoiceLine,
		Integration.Customs.IBaseJobComInvoiceLine,
		IExternalFactoryRefreshable,
		ISupportDataImporting,
		ITariffProvider,
		ICommonInvoice,
		ILandedCostDistributeTo,
		IUltimateDistributee,
		IAllInvoiceLines,
		IDeclarationProvider,
		IBondedWarehouseTransactionLineProvider,
		ICurrencyConverterProvider,
		IChargeApportionee,
		IWeightApportionee,
		ICanDelete,
		IUNDGDataItemProvider,
		IRegistryAccessingSupporter,
		ILinePriceCalculationFieldSettingSupporter,
		IShortSequenceNumberLine,
		ISynchroniserReadOnlyMembersProvider,
		ITopLevelBizOProviderForJobDocAddress,
		ICommonNonApportionedChargeProvider<BaseInvoiceLineCharge>,
		IUnitConverterDataProvider,
		IInvoiceLinePartDetails,
		ICusLinkPackageSupporter,
		ICusLineTariffDetailParent,
		WarehouseExtensions.IWarehouseProductLine,
		ITariffFormatProvider,
		IWorkflowProvider,
		ICustomFieldProvider,
		IWorkflowAffectedPropertyProvider,
		ILandedCostChargeHolder,
		IInvoiceLinePartClassificationTariffDescriptionSyncroniser,
		ISetterSuspenderSupporter,
		IBaseInvoiceLine,
		IClusterKeyWorker,
		ITariffViewFilterDataProvider,
		ITypeDeciderContext,
		IAddInfoChildSupporter,
		IDataModelSupporter
	{
		#region Constants

		#region Schema
		public new class Schema : AutoJobComInvoiceLine.Schema
		{
			public const string JI_FOB = "JI_FOB";
			public const string JI_FormattedTariff = "JI_FormattedTariff";
			public const string JI_Calc_Invoice = "JI_Calc_Invoice";
			public const string MergedLineNumber = "MergedLineNumber";
			public const string JI_Calc_MergedLineNumber = "JI_Calc_MergedLineNumber";
			public const string JI_RX_NKLinePriceCurr = "JI_RX_NKLinePriceCurr";
			public const string JI_Calc_FreightInInvoiceCurr = "JI_Calc_FreightInInvoiceCurr";
			public const string JI_Calc_InsuranceInInvoiceCurr = "JI_Calc_InsuranceInInvoiceCurr";
			public const string JI_Calc_FOB = "JI_Calc_FOB";
			public const string JI_Calc_CIF = "JI_Calc_CIF";
			public const string JI_Calc_GSTVATAmount = "JI_Calc_GSTVATAmount";
			public const string JI_Calc_GSTVATAmountIncludingWHEstimate = "JI_Calc_GSTVATAmountIncludingWHEstimate";
			public const string JI_Calc_GSTVATDeferred = "JI_Calc_GSTVATDeferred";
			public const string JI_Calc_DutyAmount = "JI_Calc_DutyAmount";
			public const string JI_Calc_DutyAmountIncludingWHEstimate = "JI_Calc_DutyAmountIncludingWHEstimate";
			public const string JI_Calc_OtherTaxesAmount = "JI_Calc_OtherTaxesAmount";
			public const string JI_Calc_Balance = "JI_Calc_Balance";
			public const string JI_Calc_LinesEntered = "JI_Calc_LinesEntered";
			public const string JI_Calc_LinesTotal = "JI_Calc_LinesTotal";
			public const string JI_LineTotal = "JI_LineTotal";
			public const string JI_Calc_OrderLineNumberAndSubLine = "JI_Calc_OrderLineNumberAndSubLine";
			public const string JI_Calc_OwnerPartNo = "JI_Calc_OwnerPartNo";
			public const string JI_Calc_SupplierPartNo = "JI_Calc_SupplierPartNo";
			public const string JI_Calc_CIF_InLocalCurrency = "JI_Calc_CIF_InLocalCurrency";
			public const string JI_Calc_PreviousProcedure = "JI_Calc_PreviousProcedure";
			public const string UnitPrice = "UnitPrice";
			public const string ShipToPartyOrgPK = "ShipToPartyOrgPK";
			public const string SellerOrgPK = "SellerOrgPK";
			public const string SoldToPartyOrgPK = "SoldToPartyOrgPK";
			public const string ConsigneeAddressOrgPK = "ConsigneeAddressOrgPK";
			public const string ManufacturerOrgPK = "ManufacturerOrgPK";
			public const string JI_IsClassUsageCommentRead = "JI_IsClassUsageCommentRead";
			public const string JI_CustomsValue = "JI_CustomsValue";
			public const string JI_RX_LocalCurrency = "JI_RX_LocalCurrency";
		}
		#endregion

		#endregion

		#region ICustomLabelsProvider

		public class CustomLabelsProvider : ICustomLabelsProvider
		{
			public CustomLabelsProvider(ICustomsCustomLabelsConfigOrgProvider configOrgProvider)
			{
				fConfigOrgProvider = configOrgProvider;
			}

			public ICustomsCustomLabelsConfigOrgProvider ConfigOrgProvider
			{
				get { return fConfigOrgProvider; }
			}

			public CustomLabelInfoList GetCustomFields(OrgHeader configOrg, BusinessObjectFactory factory)
			{
				if (configOrg != null)
				{
					var dictionary = configOrg.Factory.GetCachedValue("JobComInvoiceLine_CustomLabelsProvider", () => new Dictionary<ZGuid, CustomLabelInfoList>());
					if (!dictionary.TryGetValue(configOrg.PK, out var result))
					{
						result = GetCustomFieldsCore(configOrg, factory);
						dictionary.Add(configOrg.PK, result);
					}
					return result;
				}
				return customFieldsCached ?? (customFieldsCached = GetCustomFieldsCore(configOrg, factory));
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "used in XML keys, no need to be translated")]
			CustomLabelInfoList GetCustomFieldsCore(OrgHeader configOrg, BusinessObjectFactory factory)
			{
				var customFields = new CustomLabelInfoList(typeof(BaseJobComInvoiceLine), configOrg, ResString.GetMultilingualString("b88bf833-54e0-42a8-87a6-4a23289dfee1", "buyer or supplier on the declaration"), factory)
				{
					{ Constants.CustomLabels.ComInvoiceLine.CustomAttribute1, Schema.JI_CustomAttrib1, Constants.CustomLabels.Descriptions.CustomAttribute(1) },
					{ Constants.CustomLabels.ComInvoiceLine.CustomAttribute2, Schema.JI_CustomAttrib2, Constants.CustomLabels.Descriptions.CustomAttribute(2) },
					{ Constants.CustomLabels.ComInvoiceLine.CustomAttribute3, Schema.JI_CustomAttrib3, Constants.CustomLabels.Descriptions.CustomAttribute(3) },
					{ Constants.CustomLabels.ComInvoiceLine.CustomAttribute4, Schema.JI_CustomAttrib4, Constants.CustomLabels.Descriptions.CustomAttribute(4) },
					{ Constants.CustomLabels.ComInvoiceLine.CustomAttribute5, Schema.JI_CustomAttrib5, Constants.CustomLabels.Descriptions.CustomAttribute(5) },
					{ Constants.CustomLabels.ComInvoiceLine.CustomAttribute6, Schema.JI_CustomAttrib6, Constants.CustomLabels.Descriptions.CustomAttribute(6) },
					{ Constants.CustomLabels.ComInvoiceLine.CustomText1, Schema.JI_CustomTextBlob1, Constants.CustomLabels.Descriptions.CustomText(1), CustomLabelStyles.MultiLineTextBox },
					{ Constants.CustomLabels.ComInvoiceLine.CustomFlag1, Schema.JI_CustomFlag1, Constants.CustomLabels.Descriptions.CustomFlag(1) },
					{ Constants.CustomLabels.ComInvoiceLine.CustomFlag2, Schema.JI_CustomFlag2, Constants.CustomLabels.Descriptions.CustomFlag(2) },
					{ Constants.CustomLabels.ComInvoiceLine.CustomFlag3, Schema.JI_CustomFlag3, Constants.CustomLabels.Descriptions.CustomFlag(3) },
					{ Constants.CustomLabels.ComInvoiceLine.CustomDate1, Schema.JI_CustomDate1, Constants.CustomLabels.Descriptions.CustomDate(1) },
					{ Constants.CustomLabels.ComInvoiceLine.CustomDate2, Schema.JI_CustomDate2, Constants.CustomLabels.Descriptions.CustomDate(2) },
					{ Constants.CustomLabels.ComInvoiceLine.CustomDate3, Schema.JI_CustomDate3, Constants.CustomLabels.Descriptions.CustomDate(3) },
					{ Constants.CustomLabels.ComInvoiceLine.CustomDecimal1, Schema.JI_CustomDecimal1, Constants.CustomLabels.Descriptions.CustomNumber(1) },
					{ Constants.CustomLabels.ComInvoiceLine.CustomDecimal2, Schema.JI_CustomDecimal2, Constants.CustomLabels.Descriptions.CustomNumber(2) },
					{ Constants.CustomLabels.ComInvoiceLine.CustomDecimal3, Schema.JI_CustomDecimal3, Constants.CustomLabels.Descriptions.CustomNumber(3) }
				};

				var configOrgProviderConfigOrg = ConfigOrgProvider?.ConfigOrg;
				var configOrgProviderConfigOrgMiscServ = configOrgProviderConfigOrg?.MiscServ;
				if (configOrgProviderConfigOrgMiscServ != null)
				{
					customFields.Add(new PartCustomLabelInfo(configOrgProviderConfigOrgMiscServ.OM_IMPartAttrib1Name, configOrgProviderConfigOrgMiscServ.OM_IMPartAttrib1Type, ConfigOrgProvider.PartAttribute1, typeof(ZString), (NoResString)"Part Attribute 1", configOrgProviderConfigOrg, factory));
					customFields.Add(new PartCustomLabelInfo(configOrgProviderConfigOrgMiscServ.OM_IMPartAttrib2Name, configOrgProviderConfigOrgMiscServ.OM_IMPartAttrib2Type, ConfigOrgProvider.PartAttribute2, typeof(ZString), (NoResString)"Part Attribute 2", configOrgProviderConfigOrg, factory));
					customFields.Add(new PartCustomLabelInfo(configOrgProviderConfigOrgMiscServ.OM_IMPartAttrib3Name, configOrgProviderConfigOrgMiscServ.OM_IMPartAttrib3Type, ConfigOrgProvider.PartAttribute3, typeof(ZString), (NoResString)"Part Attribute 3", configOrgProviderConfigOrg, factory));
					customFields.Add(new PartCustomLabelInfo(configOrgProviderConfigOrg.PartAttributeManager.SerialNumberName.GetUnresolvedString(), "", ConfigOrgProvider.SerialNumber, typeof(ZString), (NoResString)"Serial Number", configOrgProviderConfigOrg, factory));
				}
				return customFields;
			}

			protected ICustomsCustomLabelsConfigOrgProvider fConfigOrgProvider;
			CustomLabelInfoList customFieldsCached;
			ICustomLabelsConfigOrgProvider ICustomLabelsProvider.ConfigOrgProvider => ConfigOrgProvider;
		}

		#endregion

		#region Comparer

		public class LineComparer : IComparer, IComparer<IBaseInvoiceLine>, IComparer<BaseJobComInvoiceLine>, IComparer<IChargeApportionee>
		{
			public int Compare(object x, object y)
			{
				var lineX = (IBaseInvoiceLine)x;
				var lineY = (IBaseInvoiceLine)y;

				if (lineX == null && lineY != null)
				{
					return -1;
				}
				else if (lineX != null && lineY == null)
				{
					return 1;
				}
				else if (lineX == null && lineY == null)
				{
					return 0;
				}

				return CompareCore(lineX, lineY);
			}

			protected virtual int CompareCore(IBaseInvoiceLine lineX, IBaseInvoiceLine lineY)
			{
				var lineXHeader = lineX.InvoiceHeader;
				var lineYHeader = lineY.InvoiceHeader;
				if (lineXHeader == null && lineYHeader != null)
				{
					return -1;
				}
				else if (lineXHeader != null && lineYHeader == null)
				{
					return 1;
				}
				else if (lineXHeader == null && lineYHeader == null)
				{
					return 0;
				}

				int result = lineXHeader.OrderByColumn.CompareTo(lineYHeader.OrderByColumn);
				if (result == 0)
				{
					result = lineX.JI_LineNo.CompareTo(lineY.JI_LineNo);
				}
				if (result == 0)
				{
					result = lineX.PK.CompareTo(lineY.PK);
				}

				return result;
			}

			public override bool Equals(object obj)
			{
				return obj.GetType() == typeof(LineComparer);
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Required because Equals is overridden")]
			public override int GetHashCode()
			{
				return base.GetHashCode();
			}

			int IComparer<IChargeApportionee>.Compare(IChargeApportionee x, IChargeApportionee y)
			{
				return Compare(x, y);
			}

			int IComparer<BaseJobComInvoiceLine>.Compare(BaseJobComInvoiceLine x, BaseJobComInvoiceLine y)
			{
				return Compare(x, y);
			}

			int IComparer<IBaseInvoiceLine>.Compare(IBaseInvoiceLine x, IBaseInvoiceLine y)
			{
				return Compare(x, y);
			}
		}

		#endregion

		public BaseJobComInvoiceLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly BaseJobComInvoiceLineTypeDecider TypeDecider = new BaseJobComInvoiceLineTypeDecider();

		public void SetTariffEtcDataFromProductsPivot(BaseCusClassPartPivot pivot)
		{
			SetTariffEtcDataFromProductsPivotCore(pivot);
		}

		protected virtual void SetTariffEtcDataFromProductsPivotCore(BaseCusClassPartPivot pivot)
		{
			if (!pivot.CI_TariffNum.IsEmpty)
			{
				JI_Tariff = pivot.CI_TariffNum;
			}
		}

		public ZString GetPartPivotType()
		{
			return GetPartPivotTypeCore();
		}

		protected virtual ZString GetPartPivotTypeCore()
		{
			var result = string.Empty;
			if (IsImport)
			{
				result = GetClassificationTypeProvider().HTICode;
			}
			else if (IsExport)
			{
				result = GetClassificationTypeProvider().HTECode;
			}
			return result;
		}
		public static ZString ConsumptionTaxDescription => GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription;

		public virtual VehicleRelationshipType VehicleRelationship => VehicleRelationshipType.None;

		#region LinkedModuleCommodity

		[List(nameof(LinkedModuleCommodity) + "." + nameof(LinkedModuleCommodity.CommodityRiskStatusCodeList))]
		public ZString LinkedModuleCommodityRiskStatusDescription
		{
			get
			{
				return LinkedModuleCommodity.RiskStatusDescription;
			}
			set
			{
				LinkedModuleCommodity.RiskStatusDescription = value;
			}
		}

		public ZPropertyInfo LinkedModuleCommodityRiskStatusDescriptionInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(LinkedModuleCommodityRiskStatusDescription), x => LinkedModuleCommodity.RiskStatusDescriptionInfo); }
		}

		public ZString LinkedModuleCommodityImportAlertDescription => LinkedModuleCommodity.ImportAlertStatus;

		public ZPropertyInfo LinkedModuleCommodityImportAlertDescriptionInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(LinkedModuleCommodityImportAlertDescription), x => LinkedModuleCommodity.ImportAlertStatusInfo); }
		}

		public ZString LegalBookLink => LinkedModuleCommodity.LegalBookLink;

		public ZPropertyInfo LegalBookLinkInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(LegalBookLink), x => LinkedModuleCommodity.LegalBookLinkInfo); }
		}

		LinkedModuleComplianceCommodityDetail _linkedModuleCommodity;
		public LinkedModuleComplianceCommodityDetail LinkedModuleCommodity
		{
			get
			{
				if (_linkedModuleCommodity == null)
				{
					_linkedModuleCommodity = new LinkedModuleComplianceCommodityDetail(this, this.Declaration);
				}

				return _linkedModuleCommodity;
			}
		}

		#endregion

		#region Business Object Overrides

		public override void OnSaving()
		{
			PopulateJI_MatchingKeyIfNeeded();
			base.OnSaving();
			if (!hasSetConcurrencyPolicy && IsInDatabase)
			{
				hasSetConcurrencyPolicy = true;
				SetConcurrencyPolicyOnProperties(ConcurrencyPolicy.Observe);
				ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(JI_CL), ConcurrencyPolicy.Strict);
			}
			PopulateDataModelIfNeeded();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (!saveSucceeded && !IsInDatabase && isMatchingKeyPopulatedByNumberFountain)
			{
				JI_MatchingKey = string.Empty;
			}
		}

		bool isMatchingKeyPopulatedByNumberFountain;
		public void PopulateJI_MatchingKeyIfNeeded()
		{
			if (string.IsNullOrEmpty(JI_MatchingKey) && !IsInDatabase)
			{
				var lines = Declaration != null
					? Declaration.InvoiceLines.Cast<BaseJobComInvoiceLine>()
					: InvoiceHeader != null
						? InvoiceHeader.InvoiceLines.Cast<BaseJobComInvoiceLine>()
						: new[] { this };

				var lineNoMatchingKey = lines.Where(l => string.IsNullOrEmpty(l.JI_MatchingKey) && !l.IsInDatabase).ToArray();

				PopulateFormattedNumberPropertyIfRequired(lineNoMatchingKey.Select(l => l.JI_MatchingKeyInfo).ToArray(),
					MatchingKeyNumberFountainProxy(), DuplicateCheckingForMatchingKeys);

				foreach (var line in lineNoMatchingKey)
				{
					line.isMatchingKeyPopulatedByNumberFountain = true;
				}
			}
		}

		INumberFountainProxy MatchingKeyNumberFountainProxy()
		{
			var prefix = GlbCompany.CurrentCompany.LicenceEnterpriseCode + GlbCompany.CurrentCompany.LicenceServerID;
			return Env.NumberFountains.JobComInvoiceLineMatchingKey(prefix, JobComInvoiceLineSchema.JI_MatchingKey.MaxLength);
		}

		IList<string> DuplicateCheckingForMatchingKeys(IList<string> sourceKeys, BusinessObjectFactory factory)
		{
			var minKey = sourceKeys.Min();
			var maxKey = sourceKeys.Max();

			var sqlQuery = @"SELECT JI_MatchingKey FROM dbo.JobComInvoiceLine WHERE JI_MatchingKey >= @MinKey AND JI_MatchingKey <= @MaxKey AND JI_MatchingKey <> ''";

			var paramCollection = new ZSqlParameterCollection();
			paramCollection.Add("@MinKey", minKey, JobComInvoiceLineSchema.JI_MatchingKey);
			paramCollection.Add("@MaxKey", maxKey, JobComInvoiceLineSchema.JI_MatchingKey);

			var duplicatedData = new DynamicBusinessObjectCollection(factory);
			duplicatedData.Load(sqlQuery, paramCollection);
			var duplicatesInDb = duplicatedData.Select(result => result[JobComInvoiceLineSchema.Constants.JI_MatchingKey].ToString());

			var duplicatesInFactoryQuery = new ZQuery();
			duplicatesInFactoryQuery.AddToFilter(JobComInvoiceLineSchema.JI_MatchingKey, SQLComparisonOperator.GreaterThanOrEqualTo, minKey);
			duplicatesInFactoryQuery.AddToFilter(JobComInvoiceLineSchema.JI_MatchingKey, SQLComparisonOperator.LessThanOrEqualTo, maxKey);
			duplicatesInFactoryQuery.FetchOnlyFromLocalCache = true;
			var duplicatesInFactory = Factory.Load<BaseJobComInvoiceLine>(duplicatesInFactoryQuery).Select(invoiceLine => invoiceLine.JI_MatchingKey.ToString());

			return duplicatesInDb.Union(duplicatesInFactory).ToList();
		}

		protected bool hasSetConcurrencyPolicy;

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			FallBackToZeroIfMoreThanMaxValue((ZPropertyInfoDecimal)JI_VolumeInfo, JobComInvoiceLineSchema.JI_Volume);
			FallBackToZeroIfMoreThanMaxValue((ZPropertyInfoDecimal)JI_WeightInfo, JobComInvoiceLineSchema.JI_Weight);
			FallBackToZeroIfMoreThanMaxValue((ZPropertyInfoDecimal)JI_NetWeightInfo, JobComInvoiceLineSchema.JI_NetWeight);
		}

		void FallBackToZeroIfMoreThanMaxValue(ZPropertyInfoDecimal propertyInfo, SchemaDecimalColumn schemaDecimalColumn)
		{
			var value = propertyInfo.Value;
			if (!value.IsWithinSqlPrecisionAndScale(schemaDecimalColumn.Precision, schemaDecimalColumn.Scale))
			{
				propertyInfo.Value = ZDecimal.Zero;
			}
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new FetchStrategies.JobComInvoiceLineFetchStrategy(this);
		}

		internal bool HasAddFetchForDeleteChildren { get; set; }

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			try
			{
				base.FillWithValidTestDataCore(kind, propertyPath);
				JI_RelatedIndicator = "Y";
			}
			finally
			{
			}
		}
#endif

		#endregion

		#region Custom Row Highlighting

		bool hasFetchedLineErrorState;
		bool highlightRow;

		public bool HasNoChangesAndHasErrorResponse
		{
			get
			{
				if (HasChanges)
				{
					highlightRow = false;
				}
				else if (!hasFetchedLineErrorState)
				{
					hasFetchedLineErrorState = true;
					highlightRow = LineHasErrorResponse;
				}
				return highlightRow;
			}
		}

		protected virtual bool LineHasErrorResponse
		{
			get
			{
				CusEntryLine line = CusEntryLine;
				return line != null && line.HasMessageResponseError;
			}
		}

		#endregion

		#region Related Business Objects

		#region Master (InvoiceHeader)
		public BaseJobComInvoiceHeader Master
		{
			get { return InvoiceHeader; }
		}
		#endregion

		#region OrderLine
		public OrderLine OrderLine
		{
			get
			{
				if (fOrderLine == null && JI_JO.IsValid)
				{
					fOrderLine = Factory.LoadTop1<OrderLine>(new ZQuery(JobOrderLineSchema.PK, JI_JO));
				}

				return fOrderLine;
			}
		}
		OrderLine fOrderLine;
		#endregion

		#region RefCusTaxOrFee

		public RefCusTaxOrFee AppliedTaxAndFee
		{
			get
			{
				var taxOrFeeCode = JI_ZZF_NKTaxType;
				if (taxOrFeeCode.IsEmpty)
				{
					appliedTaxAndFee = null;
				}
				else if (appliedTaxAndFee == null || appliedTaxAndFee.ZZF_Code != taxOrFeeCode)
				{
					var loader = new RefCusTaxOrFee.Loader(Factory);
					appliedTaxAndFee = loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(CustomsCountryCode, JI_ZZF_NKTaxType, EffectiveAssessmentDate);
				}
				return appliedTaxAndFee;
			}
		}
		RefCusTaxOrFee appliedTaxAndFee;

		#endregion

		#region InvoiceHeader
		public
#if DEBUG
 virtual
#endif
 BaseJobComInvoiceHeader InvoiceHeader
		{
			get
			{
				if (!IsDeleted && (fInvoiceHeader == null || fInvoiceHeader.PK != JI_JZ))
				{
					ZGuid reference = JI_JZ.IsEmpty ? jI_JZCachedOnRelationshipResetByCore : JI_JZ;
					fInvoiceHeader = (BaseJobComInvoiceHeader)Factory.Load(InvoiceHeaderType, reference);
				}
				return fInvoiceHeader != null && !fInvoiceHeader.IsDeleted ? fInvoiceHeader : null;
			}
		}
		BaseJobComInvoiceHeader fInvoiceHeader;

		#endregion

		#region CusEntryInstruction

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CustomsEntryInstructions))]
		[RelatedBusinessObject(nameof(EntryInstruction))]
		[ResourceStringData("09C27E76-62DB-4E04-9CA3-4C533A979539", ShortCaption = "Entry Ins.", Caption = "Entry Instruction", FullDescription = "Entry Instruction Customs Procedure Code")]
		public override ZGuid JI_CEI
		{
			get { return base.JI_CEI; }
			set
			{
				base.JI_CEI = value;
				if (!(EntryInstruction?.IsPersistent ?? true))
				{
					MakeNonPersistent();
				}
			}
		}

		public override bool IsSavedByFactory
		{
			get { return isPersistent && base.IsSavedByFactory; }
		}

		bool isPersistent = true;

		public void MakeNonPersistent()
		{
			isPersistent = false;
		}

		public CusEntryInstruction EntryInstruction
		{
			get
			{
				if (!IsDeleted && (fEntryInstruction == null || fEntryInstruction.PK != JI_CEI))
				{
					fEntryInstruction = JI_CEI.IsEmpty ? null : Factory.Load<CusEntryInstruction>(JI_CEI);
				}
				return fEntryInstruction != null && !fEntryInstruction.IsDeleted ? fEntryInstruction : null;
			}
		}
		CusEntryInstruction fEntryInstruction;

		#endregion

		public RefCountryStates OriginState
		{
			get
			{
				var originCountryCode = CountryOfOrigin?.RN_Code ?? ZString.Empty;
				var newStateCode = JI_StateOrRegionOfOrigin;
				if (string.IsNullOrEmpty(originCountryCode) || newStateCode.IsEmpty)
				{
					originState = null;
				}
				else if (originState == null || originState.RW_Code != newStateCode || originState.Country != CountryOfOrigin)
				{
					var loadingQuery = new ZQuery(RefCountryStatesSchema.RW_Code, newStateCode);
					loadingQuery.AddToFilter(RefCountryStatesSchema.RW_RN_NKCountryCode, originCountryCode);
					originState = Factory.Load<RefCountryStates>(loadingQuery).FirstOrDefault();
				}
				return originState;
			}
		}
		RefCountryStates originState;

		#region SetDefaultValues
		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			InitialisePartSyncManager();
			InitialiseCustomsUnitDefaultingStrategy();
			hasInitialisedData = true;
		}
		#endregion

		public override void OnLoaded()
		{
			base.OnLoaded();
			InitialiseData(true);
		}

		internal void InitialiseData(bool refreshPartDetails)
		{
			if (!hasInitialisedData && (Declaration == null || Declaration.ShouldInitialiseInvoiceLineData))
			{
				hasInitialisedData = true;
				if (!IsDeleted)
				{
					callPartDetailsRefresh = true;
					InitialisePartSyncManager();
					InitialiseCustomsUnitDefaultingStrategy();
					if (refreshPartDetails)
					{
						RefreshPartDetailsIfNeeded();
					}
				}
			}
		}
		bool hasInitialisedData;
		bool callPartDetailsRefresh;

		internal void RefreshPartDetailsIfNeeded()
		{
			if (callPartDetailsRefresh)
			{
				callPartDetailsRefresh = false;
				if (PartSyncManager.Enabled)
				{
					RefreshDetailsIfPartHasBeenCreatedSinceCodeWasEntered();
					PreLoadLastPartDescriptionIfPartHasBeenUsed();
				}
				RefreshDetailsAfterPartInitialisation();
			}
		}

		protected virtual void RefreshDetailsAfterPartInitialisation() { }

		internal void AddAddFetchHintForInitialiseData()
		{
			if (JI_ParentID.IsEmpty && !JI_PartNo.IsEmpty && JI_OP.IsEmpty)
			{
				Factory.AddFetchHint(typeof(OrgSupplierPart), OrgSupplierPartSchema.OP_PartNum, JI_PartNo);
			}
		}

		#region CustomsUnitDefaultingStrategy

		public void ExecuteCustomsUnitDefaultingStrategy()
		{
			CustomsUnitDefaultingStrategy?.DefaultUOMs(this);
		}

		void InitialiseCustomsUnitDefaultingStrategy()
		{
			CustomsUnitDefaultingStrategy?.Initialise(this);
		}

		protected virtual ICustomsUnitDefaultingStrategy GetCustomsUnitDefaultingStrategy() => null;

		protected void ResetCustomsUnitDefaultingStrategy()
		{
			if (customsUnitDefaultingStrategy != null)
			{
				customsUnitDefaultingStrategy.Value?.Deinitialise(this);
				customsUnitDefaultingStrategy.InvalidateCache();
				InitialiseCustomsUnitDefaultingStrategy();
			}
		}

		ICustomsUnitDefaultingStrategy CustomsUnitDefaultingStrategy => (customsUnitDefaultingStrategy ?? (customsUnitDefaultingStrategy = new RecalculableCachedValue<ICustomsUnitDefaultingStrategy>(GetCustomsUnitDefaultingStrategy))).Value;
		RecalculableCachedValue<ICustomsUnitDefaultingStrategy> customsUnitDefaultingStrategy;

		#endregion

		#region Declaration
		public BaseJobDeclaration Declaration
		{
			get
			{
				if (OverrideParent != null)
				{
					return OverrideParent;
				}
				// Leave for performance
				if (fDeclaration == null || fDeclaration.IsDeleted)
				{
					var invoiceHeaderCached = InvoiceHeader;
					fDeclaration = invoiceHeaderCached?.JobDeclaration;
				}
				return fDeclaration;
			}
		}
		BaseJobDeclaration fDeclaration;

		internal void RefreshDeclaration()
		{
			fDeclaration = null;
		}

		internal BaseJobDeclaration OverrideParent
		{
			get { return fOverrideParent; }
			set
			{
				if (fOverrideParent != null && value != null && fOverrideParent != value)
				{
					throw new DeveloperNotificationException("Parent has already been overriden");
				}
				else
				{
					fOverrideParent = value;
				}
			}
		}
		BaseJobDeclaration fOverrideParent;

		#endregion

		#region SetDeclarationForTesting
#if DEBUG
		public void SetDeclarationForTesting(BaseJobDeclaration declaration)
		{
			if (!Enterprise.ZArchitecture.Environment.Globals.IsTest)
			{
				throw new NotSupportedException("test only!");
			}
			fDeclaration = declaration;
		}
#endif
		#endregion

		#region Effective Supplier and Importer
		#region Importer_Effective
		public OrgHeader Importer_Effective
		{
			get { return Factory.Load<OrgHeader>(ImporterPK_Effective); }
		}
		#endregion

		#region Supplier_Effective
		public OrgHeader Supplier_Effective
		{
			get { return Factory.Load<OrgHeader>(SupplierPK_Effective); }
		}
		#endregion

		#region ImporterPK_Effective
		public ZGuid ImporterPK_Effective
		{
			get { return InvoiceHeader == null ? ZGuid.Empty : InvoiceHeader.JZ_OH_Buyer_Effective; }
		}
		#endregion

		#region SupplierPK_Effective
		public ZGuid SupplierPK_Effective
		{
			get { return InvoiceHeader == null ? ZGuid.Empty : InvoiceHeader.JZ_OH_Supplier_Effective; }
		}
		#endregion
		#endregion

		#region Part

		public override ZGuid JI_OP
		{
			get { return base.JI_OP; }
			set
			{
				var oldValue = JI_OP;
				base.JI_OP = value;
				if (!IsCopying && oldValue != JI_OP)
				{
					if (PartSyncManager != null)
					{
						PartSyncManager.ReloadPart = true;
					}
				}
			}
		}

		public OrgSupplierPart Part
		{
			get
			{
#if DEBUG
				if (partOverrideForTesting != null)
				{
					return partOverrideForTesting;
				}
#endif

				OrgSupplierPart result = null;
				if (fPartSyncManager == null)
				{
					if (Declaration.ShouldInitialiseInvoiceLineData)
					{
						ErrorReporter.ReportOnce("SyncManagerNullAccess", "PartSyncManager has not been initialised");
					}
					else
					{
						result = (OrgSupplierPart)Factory.Load(TypeOfPartUsed, JI_OP);
					}
				}
				else
				{
					if (PartSyncManager.Part is OrgSupplierPart part && !part.IsDeleted)
					{
						result = part;
					}
				}

				return result;
			}
		}

		public BaseCusClassPartPivot Pivot
		{
			get
			{
				var cachedPivotValue = cachedPivot?.Value;
				if (cachedPivot == null || (cachedPivotValue?.IsDeleted ?? false))
				{
					cachedPivot = new CachedProperty<BaseCusClassPartPivot>(Factory, GetPivot);
					cachedPivotValue = cachedPivot.Value;
				}
				return cachedPivotValue;
			}
		}
		CachedProperty<BaseCusClassPartPivot> cachedPivot;

		public BaseCusClassPartPivot GetPivot() => GetPivotCore();

		protected virtual BaseCusClassPartPivot GetPivotCore()
		{
			BaseCusClassPartPivot result = null;
			var pivots = GetPivots();
			if (pivots != null && pivots.Length == 1)
			{
				result = pivots.First();
			}

			return result;
		}

		public IClassificationTypeProvider GetClassificationTypeProvider() => ClassificationTypeProvider.GetProviderFor(CustomsCountryCode);

		protected internal BaseCusClassPartPivot[] GetPivots()
		{
			return Factory.GetCached(ref cachedPivots, () =>
			{
				BaseCusClassPartPivot[] result = null;
				var part = Part;
				if (part != null && InvoiceHeader != null)
				{
					var effectiveDate = EffectiveDateForDutyRate;
					var query = new ZQuery(CusClassPartPivotSchema.CI_OP, part.PK);
					query.AddToFilter(CusClassPartPivotSchema.CI_RN_NKCountry, CustomsCountryCode);
					query.AddToFilter(CusClassPartPivotSchema.CI_CI_Parent, null);

					var startDateQuery = new ZQuery(CusClassPartPivotSchema.CI_DateStart, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, effectiveDate);
					startDateQuery.AddToFilter(JoinCondition.Or, CusClassPartPivotSchema.CI_DateStart, null);

					var endDateQuery = new ZQuery(CusClassPartPivotSchema.CI_DateEnd, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, effectiveDate);
					endDateQuery.AddToFilter(JoinCondition.Or, CusClassPartPivotSchema.CI_DateEnd, null);

					var dateQuery = new ZQuery();
					dateQuery.AddToFilter(startDateQuery);
					dateQuery.AddToFilter(endDateQuery);

					query.AddToFilter(dateQuery);
					query.FetchOnlyFromLocalCache = !part.IsInDatabase;
					var pivots = Factory.Load<BaseCusClassPartPivot>(query);
					result = GetPivotsByTypeAndOwnerSupplier(pivots);
				}
				return result ?? Array.Empty<BaseCusClassPartPivot>();
			});
		}
		CachedProperty<BaseCusClassPartPivot[]> cachedPivots;

		internal bool HasMultiplePivotsMatchingProduct => GetPivots().Length > 0;

		protected virtual BaseCusClassPartPivot[] GetPivotsByTypeAndOwnerSupplier(BaseCusClassPartPivot[] pivots)
		{
			BaseCusClassPartPivot[] result = null;
			if (pivots.Length > 0)
			{
				var provider = GetClassificationTypeProvider();
				if (IsImport)
				{
					result = GetMatches(pivots, provider, provider.HTICode, GetPartAttribs());
				}
				else if (IsExport)
				{
					result = GetMatches(pivots, provider, provider.HTECode);
				}
			}
			return result ?? Array.Empty<BaseCusClassPartPivot>();
		}

		protected BaseCusClassPartPivot[] GetMatches(BaseCusClassPartPivot[] pivots, IClassificationTypeProvider provider, ZString typeOfPivot, CusAttributeFilter.AttributeValue[] attribs = null)
		{
			var pivotMatcher = new ClassPartPivotMatcher(pivots, provider.HTBCode, false);
			return pivotMatcher.GetMatchListSkipTypeFallback(typeOfPivot, InvoiceHeader.JZ_OH_Buyer_Effective, InvoiceHeader.JZ_OH_Supplier_Effective, attribs.IsNullOrEmpty(), attribs).ToArray();
		}

		protected CusAttributeFilter.AttributeValue[] GetPartAttribs()
		{
			var result = new List<CusAttributeFilter.AttributeValue>
			{
				new CusAttributeFilter.AttributeValue() { Name = CusAttributeFilter.AttributeFilterName.AT1, Value = JI_PartAttrib1 },
				new CusAttributeFilter.AttributeValue() { Name = CusAttributeFilter.AttributeFilterName.AT2, Value = JI_PartAttrib2 },
				new CusAttributeFilter.AttributeValue() { Name = CusAttributeFilter.AttributeFilterName.AT3, Value = JI_PartAttrib3 }
			};

			return result.ToArray();
		}

		public Type TypeOfPartUsed => TypeOfPartUsedCore;
		protected virtual Type TypeOfPartUsedCore => OrgSupplierPartTypeDecider.GetOrgSupplierPartType(CustomsCountryCode);

#if DEBUG
		public void SetPartForTesting(OrgSupplierPart part)
		{
			partOverrideForTesting = part;
		}
		OrgSupplierPart partOverrideForTesting;
#endif

		void RefreshDetailsIfPartHasBeenCreatedSinceCodeWasEntered()
		{
			if (!JI_PartNo.IsEmpty && JI_OP.IsEmpty)
			{
				using (GetValidationSuspender())
				{
					if (JI_Tariff.IsEmpty && JI_CC.IsEmpty && JI_CustomsQuantity == 0)
					{
						PartSyncManager.Refresh();
					}
					else
					{
						PartSyncManager.OnlySetPartPK();
					}
				}
			}
		}

		public JobComInvoiceLinePartSynchronisationManager PartSyncManager
		{
			get { return fPartSyncManager; }
		}

		public override OrgHeader Supplier
		{
			get
			{
				var result = base.Supplier;

				if (result == null)
				{
					var invoiceHeader = InvoiceHeader;
					if (invoiceHeader != null && !invoiceHeader.IsDeleted)
					{
						result = invoiceHeader.Supplier;
					}
				}

				return result;
			}
		}

		public OrgHeader Importer
		{
			get { return ImporterCore; }
		}

		protected virtual OrgHeader ImporterCore
		{
			get
			{
				OrgHeader result = null;
				if (InvoiceHeader != null && !InvoiceHeader.IsAttachedToPersistentDeclaration)
				{
					result = InvoiceHeader.Buyer;
				}

				if (result == null)
				{
					var declaration = Declaration;
					result = (IsDeleted || declaration == null || declaration.IsDeleted) ? null : declaration.Importer;
				}

				return result;
			}
		}

		public new BaseJobComInvoiceLine Clone()
		{
			return (BaseJobComInvoiceLine)base.Clone();
		}

		#region Implementation

		protected virtual void InitialisePartSyncManager()
		{
			if (fPartSyncManager == null)
			{
				fPartSyncManager = new JobComInvoiceLinePartSynchronisationManager(this);
			}
		}

		protected virtual Type InvoiceHeaderType
		{
			get { return typeof(BaseJobComInvoiceHeader); }
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			BaseJobComInvoiceLine result = (BaseJobComInvoiceLine)base.CloneInternal(args);

			if (result.InvoiceHeader != null)
			{
				result.PartSyncManager.Refresh();
			}
			if (this is INAddInfoSupporter)
			{
				result.JI_AddInfo = JI_AddInfo;
			}
			return result;
		}

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			var result = new List<string>(base.GetPropertiesToExcludeFromCloning())
			{
				JobComInvoiceLineSchema.Constants.JI_CL,
				JobComInvoiceLineSchema.Constants.JI_CEI,
				JobComInvoiceLineSchema.Constants.JI_GS_NKClassUsageCommentReviewer
			};

			if (this is INAddInfoSupporter)
			{
				result.Add(JobComInvoiceLineSchema.Constants.JI_NAddInfo);
			}

			return result;
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override JobComInvoiceLineValidation GetNewValidation() => new BaseJobComInvoiceLineValidation(this);

		public void ResetValuesAfterClone()
		{
			ResetValuesAfterCloneCore();
		}

		protected virtual void ResetValuesAfterCloneCore()
		{
		}

		protected JobComInvoiceLinePartSynchronisationManager fPartSyncManager;

		#endregion

		#endregion

		#region Class

		public BaseCusClassification Classification
		{
			get
			{
				BaseCusClassification result = Factory.Load<BaseCusClassification>(JI_CC);
				if (result != null && InvoiceHeader != null && (InvoiceHeader.Branch == null || CustomsCountryCode != result.CC_RN_NKCountryCode))
				{
					result = null;
				}
				return result;
			}
		}

		protected virtual Type GetClassificationType()
		{
			return typeof(BaseCusClassification);
		}
		#endregion

		#region CusEntryLine
		public CusEntryLine CusEntryLine
		{
			get
			{
				if (fCusEntryLine == null || fCusEntryLine.IsDeleted || fCusEntryLine.PK != JI_CL)
				{
					fCusEntryLine = (CusEntryLine)Factory.Load(CusEntryLineType, JI_CL);
				}
				return fCusEntryLine;
			}
		}
		CusEntryLine fCusEntryLine;

		#endregion

		protected virtual Type CusEntryLineType
		{
			get { return typeof(CusEntryLine); }
		}

		#region UnitConverter
		public UnitConverter UnitConverter
		{
			get { return fUnitConverter ?? (fUnitConverter = new UnitConverter(this)); }
		}
		UnitConverter fUnitConverter;
		#endregion

		#region UNDGs

		[ChildEditable(true)]
		public UNDGDataItemCollection UNDGs
		{
			get
			{
				if (fUNDGs == null)
				{
					if (Declaration != null)
					{
						fUNDGs = new UNDGDataItemCollection(this, Declaration.Consignee, Declaration.Consignor);
					}
					else
					{
						fUNDGs = new UNDGDataItemCollection(this);
					}
					InitialiseUNDGs();
					RegisterEditableChildObject(fUNDGs);
				}
				return fUNDGs;
			}
		}
		UNDGDataItemCollection fUNDGs;

		bool IUNDGDataItemProvider.NeedFetchHintForLoad => true;

		protected virtual void InitialiseUNDGs()
		{
		}

		#endregion

		#endregion

		#region ReadOnly
		public List<string> SynchroniserReadOnlyMembers { get { return synchroniserReadOnlyMembers ?? (synchroniserReadOnlyMembers = new List<string>()); } }
		List<string> synchroniserReadOnlyMembers;

		protected virtual bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return MetaData.GetReadOnlyExcludingMethodProvider(this, property) || SynchroniserReadOnlyMembers.Contains(property.Name);
		}
		#endregion

		#region SellerOrgPK
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.SellerConsignors))]
		public ZGuid SellerOrgPK
		{
			get { return JI_OA_Seller_ZAddress.OrgPK; }
			set { JI_OA_Seller_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo SellerOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.SellerOrgPK, x => JI_OA_Seller_ZAddress.OrgPKInfo); }
		}
		#endregion

		#region ShipToPartyOrgPK
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.ShipToParties))]
		public ZGuid ShipToPartyOrgPK
		{
			get { return JI_OA_ShipToPartyAddress_ZAddress.OrgPK; }
			set { JI_OA_ShipToPartyAddress_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo ShipToPartyOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ShipToPartyOrgPK, x => JI_OA_ShipToPartyAddress_ZAddress.OrgPKInfo); }
		}

		public OrgHeader ShipToParty
		{
			get { return Factory.Load<OrgHeader>(ShipToPartyOrgPK); }
		}

		#endregion

		#region New Properties

		public bool IsImport => InvoiceHeader?.IsImport ?? false;

		public bool IsExport => InvoiceHeader?.IsExport ?? false;

		public virtual ZPropertyInfo CountryOfOriginFieldInfo
		{
			get { return JI_CountryOfOriginInfo; }
		}

		public ZString InvoiceAndLineReference
		{
			get { return Res.GetString("308ea3cb-2d27-4001-bbf4-2e7772a6940e", "invoice {0} line {1}", InvoiceNumber, JI_LineNo); }
		}

		public ZString InvoiceNumber => InvoiceHeader?.JZ_InvoiceNumber ?? ZString.Empty;

		public ZBool IsInvoiceCurrExRateUserEnterable
		{
			get { return InvoiceHeader == null ? ZBool.False : InvoiceHeader.IsJZ_InvoiceCurrExRateUserEnterable; }
		}

		/// <summary>
		/// You should set the Duty Percent when Duty is calculated to display this to users. See AU LineMerger.
		/// </summary>
		public ZDecimal AdValoremDutyPercent
		{
			get
			{
				var cusEntryLine = CusEntryLine;
				return cusEntryLine == null ? ZDecimal.Zero : cusEntryLine.CL_DutyPercent;
			}
		}

		public ZString ProvProgTariff => SecondTariff;
		public ZString ProvProgDutyRate => SecondTariffDutyRate;
		public ZString ProvProgCustomsQty => SecondTariffCustomsQty;
		public ZString ProvProgTariff1 => ThirdTariff;
		public ZString ProvProgDutyRate1 => ThirdTariffDutyRate;
		public ZString ProvProgCustomsQty1 => ThirdTariffCustomsQty;
		public ZString ProvProgTariff2 => FourthTariff;
		public ZString ProvProgDutyRate2 => FourthTariffDutyRate;
		public ZString ProvProgCustomsQty2 => FourthTariffCustomsQty;
		public ZString ProvProgTariff3 => FifthTariff;
		public ZString ProvProgDutyRate3 => FifthTariffDutyRate;
		public ZString ProvProgCustomsQty3 => FifthTariffCustomsQty;
		public ZString ProvProgTariff4 => SixthTariff;
		public ZString ProvProgDutyRate4 => SixthTariffDutyRate;
		public ZString ProvProgCustomsQty4 => SixthTariffCustomsQty;
		public ZString ProvProgTariff5 => SeventhTariff;
		public ZString ProvProgDutyRate5 => SeventhTariffDutyRate;
		public ZString ProvProgCustomsQty5 => SeventhTariffCustomsQty;
		protected virtual ZString SecondTariff => ZString.Empty;
		protected virtual ZString SecondTariffDutyRate => ZString.Empty;
		protected virtual ZString SecondTariffCustomsQty => ZString.Empty;
		protected virtual ZString ThirdTariff => ZString.Empty;
		protected virtual ZString ThirdTariffDutyRate => ZString.Empty;
		protected virtual ZString ThirdTariffCustomsQty => ZString.Empty;
		protected virtual ZString FourthTariff => ZString.Empty;
		protected virtual ZString FourthTariffDutyRate => ZString.Empty;
		protected virtual ZString FourthTariffCustomsQty => ZString.Empty;
		protected virtual ZString FifthTariff => ZString.Empty;
		protected virtual ZString FifthTariffDutyRate => ZString.Empty;
		protected virtual ZString FifthTariffCustomsQty => ZString.Empty;
		protected virtual ZString SixthTariff => ZString.Empty;
		protected virtual ZString SixthTariffDutyRate => ZString.Empty;
		protected virtual ZString SixthTariffCustomsQty => ZString.Empty;
		protected virtual ZString SeventhTariff => ZString.Empty;
		protected virtual ZString SeventhTariffDutyRate => ZString.Empty;
		protected virtual ZString SeventhTariffCustomsQty => ZString.Empty;

		public ZPropertyInfo AdValoremDutyPercentInfo
		{
			get { return GetZPropertyInfo(nameof(AdValoremDutyPercent)); }
		}

		public bool IsBondedWarehousingDisabled
		{
			get { return IsBondedWarehousingDisabledCore; }
		}

		protected virtual bool IsBondedWarehousingDisabledCore
		{
			get { return CusEntryLine?.Header?.IsBondedWarehousingDisabled ?? false; }
		}

		public bool SupportsBondedWarehousing
		{
			get { return SupportsBondedWarehousingCore; }
		}

		protected virtual bool SupportsBondedWarehousingCore
		{
			get
			{
#if DEBUG
				if (BaseJobDeclaration.IsWHSUniversalXMLActiveValueForTestingSetup)
				{
					return Declaration?.SupportsBondedWarehousing ?? false;
				}
#endif

				return CusEntryLine?.Header?.SupportsBondedWarehousing ?? false;
			}
		}

		public bool UseBondedWarehouseAutomation
		{
			get
			{
#if DEBUG
				if (overrideUseBondedWarehouseAutomation)
				{
					return overridenUseBondedWarehouseAutomation;
				}
#endif
				return UseBondedWarehouseAutomationCore;
			}
			set
			{
#if DEBUG
				if (overrideUseBondedWarehouseAutomation)
				{
					overridenUseBondedWarehouseAutomation = value;
				}
				else
#endif
				{
					UseBondedWarehouseAutomationCore = value;
				}
			}
		}

		protected virtual bool UseBondedWarehouseAutomationCore
		{
			get
			{
				if (useBondedWarehouseAutomationCached == null)
				{
					useBondedWarehouseAutomationCached = new CachedProperty<bool>(Factory, () =>
					{
						return (Declaration?.SupportMultipleWarehouseEntry ?? false) && UseBondedWarehouseAutomationForMultipleWarehouseEntry;
					});
				}
				return useBondedWarehouseAutomationCached.Value;
			}
			set { throw new NotSupportedException("Please override UseBondedWarehouseAutomationCore"); }
		}
		CachedProperty<bool> useBondedWarehouseAutomationCached;

		bool UseBondedWarehouseAutomationForMultipleWarehouseEntry
		{
			get
			{
				var result = false;
				var entryInstruction = EntryInstruction;
				if (entryInstruction != null)
				{
					if (HasBothOutOfAndIntoRegimeProcedure)
					{
						result = entryInstruction.Warehouse2IsInventoryManagementOn || entryInstruction.WarehouseIsInventoryManagementOn || entryInstruction.ClientIsInventoryManagementOn || WarehouseTransactionStatusList.IsOutwardCode(entryInstruction.EntryHeader?.CH_WarehouseTransactionStatus ?? ZString.Empty);
					}
					else
					{
						result = HasOutOfRegimeProcedure;
					}
				}
				return result;
			}
		}

#if DEBUG
		public void SetUseBondedWarehouseAutomationForTesting(bool enabled)
		{
			overrideUseBondedWarehouseAutomation = true;
			overridenUseBondedWarehouseAutomation = enabled;
			Factory.InvalidateCachedProperties();
		}
		bool overrideUseBondedWarehouseAutomation;
		bool overridenUseBondedWarehouseAutomation;
#endif

		public bool IsWHSUniversalXMLActive
		{
			get
			{
				var declaration = Declaration;
				return declaration != null && declaration.IsWHSUniversalXMLActive;
			}
		}

		public bool IsGoingIntoBondedWarehouse
		{
			get
			{
#if DEBUG
				if (overridingIsGoingIntoBondedWarehouseCore)
				{
					var declaration = Declaration;
					return declaration != null && (declaration.IsWarehousedByExternalAgent || overrideIsGoingIntoBondedWarehouseCore);
				}
#endif
				if (isGoingIntoBondedWarehouseCached == null)
				{
					isGoingIntoBondedWarehouseCached = new CachedProperty<bool>(Factory, () =>
					{
						var declaration = Declaration;
						return declaration != null && (declaration.SupportMultipleWarehouseEntry ? IsGoingIntoBondedWarehouseForMultipleWarehouseEntry : (declaration.IsWarehousedByExternalAgent || IsGoingIntoBondedWarehouseCore));
					});
				}
				return isGoingIntoBondedWarehouseCached.Value;
			}
		}
		CachedProperty<bool> isGoingIntoBondedWarehouseCached;

		protected virtual bool IsGoingIntoBondedWarehouseCore => HasIntoRegimeProcedure;

		bool IsGoingIntoBondedWarehouseForMultipleWarehouseEntry
		{
			get
			{
				var result = false;
				var entryInstruction = EntryInstruction;
				if (entryInstruction != null)
				{
					if (HasBothOutOfAndIntoRegimeProcedure)
					{
						result = entryInstruction.Warehouse2IsInventoryManagementOn || entryInstruction.WarehouseIsInventoryManagementOn || entryInstruction.OwnerIsInventoryManagementOn || WarehouseTransactionStatusList.IsInwardCode(entryInstruction.EntryHeader?.CH_WarehouseTransactionStatus ?? ZString.Empty);
					}
					else
					{
						result = HasIntoRegimeProcedure;
					}
				}
				return result;
			}
		}

		/// <summary>
		/// Override and call through to add info in relevant countries
		/// </summary>
		public virtual ZGuid JI_BondedWarehouseLineKey
		{
			get { return ZGuid.Empty; }
			set { }
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.Procedures))]
		[ResourceStringData("16a4d865-fc4b-4d68-816b-788d6215aba5", Caption = "Procedure Code")]
		public override ZString JI_Procedure
		{
			get => base.JI_Procedure;
			set
			{
				var oldValue = JI_Procedure;
				base.JI_Procedure = value;
				WipeNKTaxType();
			}
		}

		public virtual bool ShouldWipeNKTaxType => !CusProcedure?.ZZ6_CalculateVAT ?? false;

		public virtual ZString ProcedureIndicatesVATNotApply => JI_Procedure;

		public void WipeNKTaxType() => WipeNKTaxTypeCore();

		protected virtual void WipeNKTaxTypeCore()
		{
			if (ShouldWipeNKTaxType)
			{
				JI_ZZF_NKTaxType = ZString.Empty;
			}
		}

		/// <summary>
		/// If set to false this will give a validation warning on JI_Procedure if one invoice line has a procedure of ZZ6_Category=X while its brother invoice line (on same CEI) has one of category Y.
		/// </summary>
		protected virtual bool AllowMyBrotherInvoiceLinesToBeOfMixedCategoriesCore
		{
			get { return true; }
		}

		public bool AllowMyBrotherInvoiceLinesToBeOfMixedCategories
		{
			get { return AllowMyBrotherInvoiceLinesToBeOfMixedCategoriesCore; }
		}

		#region Change Of Ownership

		public bool IsChangeOfOwnershipWarehousing
		{
			get
			{
				if (isChangeOfOwnershipWarehousingCached == null)
				{
					isChangeOfOwnershipWarehousingCached = new CachedProperty<bool>(Factory, () => HasBothOutOfAndIntoRegimeProcedure
						&& ((CusEntryLine?.Header?.HasWHSChangeOfOwnershipTransaction ?? false)
							|| (EntryInstruction is CusEntryInstruction entryInstruction
								&& (entryInstruction.WarehouseIsInventoryManagementOn || entryInstruction.Warehouse2IsInventoryManagementOn
									|| (entryInstruction.ClientIsInventoryManagementOn && entryInstruction.OwnerIsInventoryManagementOn)))));
				}
				return isChangeOfOwnershipWarehousingCached.Value;
			}
		}
		CachedProperty<bool> isChangeOfOwnershipWarehousingCached;

		public bool HasBothOutOfAndIntoRegimeProcedure
		{
			get
			{
				if (hasChangeOfOwnershipProcedureCached == null)
				{
					hasChangeOfOwnershipProcedureCached = new CachedProperty<bool>(Factory, () =>
					{
						var procedure = CusProcedure;
						return procedure != null && procedure.IsOutOfRegime() && procedure.IsIntoRegime();
					});
				}
				return hasChangeOfOwnershipProcedureCached.Value;
			}
		}
		CachedProperty<bool> hasChangeOfOwnershipProcedureCached;

		#endregion

		#region Change Of Regime

		public bool IsChangeOfRegimeWarehousing
		{
			get
			{
				if (isChangeOfRegimeWarehousingCached == null)
				{
					isChangeOfRegimeWarehousingCached = new CachedProperty<bool>(Factory, () => HasBothOutOfAndIntoRegimeProcedure
						&& ((CusEntryLine?.Header?.HasWHSChangeOfRegimeTransaction ?? false) || (EntryInstruction?.HasChangeOfRegimeWarehousing ?? false)));
				}
				return isChangeOfRegimeWarehousingCached.Value;
			}
		}
		CachedProperty<bool> isChangeOfRegimeWarehousingCached;

		#endregion

		#region Into & Out Of Warehouse

		public bool HasOutOfRegimeProcedure => Factory.GetValue(ref hasOutOfRegimeProcedureCached, () => CusProcedure is RefCusProcedure procedure && IsOutOfRegime(procedure));
		CachedProperty<bool> hasOutOfRegimeProcedureCached;

		public bool HasIntoRegimeProcedure => Factory.GetValue(ref hasIntoRegimeProcedureCached, () => CusProcedure is RefCusProcedure procedure && IsIntoRegime(procedure));
		CachedProperty<bool> hasIntoRegimeProcedureCached;

		public bool IsOutOfRegime(RefCusProcedure procedure) => GetNewProcedureRegimeDecider().IsOutOfRegime(procedure);
		public bool IsIntoRegime(RefCusProcedure procedure) => GetNewProcedureRegimeDecider().IsIntoRegime(procedure);
		protected virtual ProcedureRegimeDecider GetNewProcedureRegimeDecider() => new ProcedureRegimeDecider();

		public bool IsOutOfWarehouseWarehousing
		{
			get
			{
				if (isOutOfWarehouseWarehousingCached == null)
				{
					isOutOfWarehouseWarehousingCached = new CachedProperty<bool>(Factory, () =>
					{
						var entryInstruction = EntryInstruction;
						var procedure = CusProcedure;
						return procedure != null && procedure.IsOutOfWarehouse() && (!procedure.IsIntoWarehouse() || (CusEntryLine?.Header?.HasWHSOutwardTransaction ?? false) || (entryInstruction != null && (entryInstruction.ClientIsBondedWarehousing && !entryInstruction.WarehouseIsBondedWarehousing && !entryInstruction.Warehouse2IsBondedWarehousing && !entryInstruction.OwnerIsBondedWarehousing)));
					});
				}
				return isOutOfWarehouseWarehousingCached.Value;
			}
		}
		CachedProperty<bool> isOutOfWarehouseWarehousingCached;

		public bool HasOutOfWarehouseProcedure
		{
			get
			{
				if (hasOutOfWarehouseProcedureCached == null)
				{
					hasOutOfWarehouseProcedureCached = new CachedProperty<bool>(Factory, () =>
					{
						return CusProcedure?.IsOutOfWarehouse() ?? ZBool.False;
					});
				}
				return hasOutOfWarehouseProcedureCached.Value;
			}
		}
		CachedProperty<bool> hasOutOfWarehouseProcedureCached;

		public bool IsIntoWarehouseWarehousing
		{
			get
			{
				if (isIntoWarehouseWarehousingCached == null)
				{
					isIntoWarehouseWarehousingCached = new CachedProperty<bool>(Factory, () =>
					{
						var entryInstruction = EntryInstruction;
						var procedure = GetCusProcedure(ProcedureCode);
						return procedure != null && procedure.IsIntoWarehouse() && (!procedure.IsOutOfWarehouse() || (CusEntryLine?.Header?.HasWHSInwardTransaction ?? false) || (entryInstruction != null && (entryInstruction.OwnerIsBondedWarehousing && !entryInstruction.WarehouseIsBondedWarehousing && !entryInstruction.Warehouse2IsBondedWarehousing && !entryInstruction.ClientIsBondedWarehousing)));
					});
				}
				return isIntoWarehouseWarehousingCached.Value;
			}
		}
		CachedProperty<bool> isIntoWarehouseWarehousingCached;

		public bool HasIntoWarehouseProcedure
		{
			get
			{
				if (hasIntoWarehouseProcedureCached == null)
				{
					hasIntoWarehouseProcedureCached = new CachedProperty<bool>(Factory, () =>
					{
						return CusProcedure?.IsIntoWarehouse() ?? ZBool.False;
					});
				}
				return hasIntoWarehouseProcedureCached.Value;
			}
		}
		CachedProperty<bool> hasIntoWarehouseProcedureCached;

		public bool HasIntoVATWarehouseProcedure
		{
			get
			{
				if (hasIntoVATWarehouseProcedureCached == null)
				{
					hasIntoVATWarehouseProcedureCached = new CachedProperty<bool>(Factory, () =>
					{
						return CusProcedure?.IsIntoVATWarehouse() ?? ZBool.False;
					});
				}
				return hasIntoVATWarehouseProcedureCached.Value;
			}
		}
		CachedProperty<bool> hasIntoVATWarehouseProcedureCached;

		public bool IsIntoOrOutOfRegimeProcedure => HasIntoRegimeProcedure || HasOutOfRegimeProcedure;

		#endregion

		#region Inward & Outward Processing

		#region Inward
		public virtual bool HasIntoInwardProcessingProcedure
		{
			get
			{
				if (hasIntoInwardProcessingProcedureCached == null)
				{
					hasIntoInwardProcessingProcedureCached = new CachedProperty<bool>(Factory, () => CusProcedure != null && CusProcedure.IsIntoInwardProcessing());
				}
				return hasIntoInwardProcessingProcedureCached.Value;
			}
		}
		CachedProperty<bool> hasIntoInwardProcessingProcedureCached;

		public bool IsIntoInwardProcessing
		{
			get
			{
				if (isIntoInwardProcessingCached == null)
				{
					isIntoInwardProcessingCached = new CachedProperty<bool>(Factory, () =>
					{
						var entryInstruction = EntryInstruction;
						var procedure = GetCusProcedure(ProcedureCode);
						return procedure != null && procedure.IsIntoInwardProcessing() && (!procedure.IsOutOfInwardProcessing() || (CusEntryLine?.Header?.HasWHSOutwardTransaction ?? false) || (entryInstruction != null && (entryInstruction.OwnerIsBondedWarehousing && !entryInstruction.WarehouseIsInwardProcessing && !entryInstruction.Warehouse2IsInwardProcessing && !entryInstruction.ClientIsBondedWarehousing)));
					});
				}
				return isIntoInwardProcessingCached.Value;
			}
		}
		CachedProperty<bool> isIntoInwardProcessingCached;

		public virtual bool HasOutOfInwardProcessingProcedure
		{
			get
			{
				if (hasOutOfInwardProcessingProcedureCached == null)
				{
					hasOutOfInwardProcessingProcedureCached = new CachedProperty<bool>(Factory, () => CusProcedure != null && CusProcedure.IsOutOfInwardProcessing());
				}
				return hasOutOfInwardProcessingProcedureCached.Value;
			}
		}
		CachedProperty<bool> hasOutOfInwardProcessingProcedureCached;

		public bool IsOutOfInwardProcessing
		{
			get
			{
				if (isOutOfInwardProcessingCached == null)
				{
					isOutOfInwardProcessingCached = new CachedProperty<bool>(Factory, () =>
					{
						var entryInstruction = EntryInstruction;
						var procedure = GetCusProcedure(ProcedureCode);
						return procedure != null && procedure.IsOutOfInwardProcessing() && (!procedure.IsIntoInwardProcessing() || (CusEntryLine?.Header?.HasWHSOutwardTransaction ?? false) || (entryInstruction != null && (entryInstruction.ClientIsInwardProcessing && !entryInstruction.WarehouseIsInwardProcessing && !entryInstruction.Warehouse2IsInwardProcessing && !entryInstruction.OwnerIsInwardProcessing)));
					});
				}
				return isOutOfInwardProcessingCached.Value;
			}
		}
		CachedProperty<bool> isOutOfInwardProcessingCached;

		public bool SupportOutOfInwardProcessing => Declaration != null && Declaration.SupportInwardProcessing && HasOutOfInwardProcessingProcedure;

		#endregion

		#region Outward
		public virtual bool HasIntoOutwardProcessingProcedure
		{
			get
			{
				if (hasIntoOutwardProcessingProcedureCached == null)
				{
					hasIntoOutwardProcessingProcedureCached = new CachedProperty<bool>(Factory, () => CusProcedure != null && CusProcedure.IsIntoOutwardProcessing());
				}
				return hasIntoOutwardProcessingProcedureCached.Value;
			}
		}
		CachedProperty<bool> hasIntoOutwardProcessingProcedureCached;

		public bool IsIntoOutwardProcessing
		{
			get
			{
				if (isIntoOutwardProcessingCached == null)
				{
					isIntoOutwardProcessingCached = new CachedProperty<bool>(Factory, () =>
					{
						var entryInstruction = EntryInstruction;
						var procedure = GetCusProcedure(ProcedureCode);
						return procedure != null && procedure.IsIntoOutwardProcessing() && (!procedure.IsOutOfOutwardProcessing() || (CusEntryLine?.Header?.HasWHSOutwardTransaction ?? false) || (entryInstruction != null && (entryInstruction.OwnerIsOutwardProcessing && !entryInstruction.WarehouseIsOutwardProcessing && !entryInstruction.Warehouse2IsOutwardProcessing && !entryInstruction.ClientIsOutwardProcessing)));
					});
				}
				return isIntoOutwardProcessingCached.Value;
			}
		}
		CachedProperty<bool> isIntoOutwardProcessingCached;

		public virtual bool HasOutOfOutwardProcessingProcedure
		{
			get
			{
				if (hasOutOfOutwardProcessingProcedureCached == null)
				{
					hasOutOfOutwardProcessingProcedureCached = new CachedProperty<bool>(Factory, () => CusProcedure != null && CusProcedure.IsOutOfOutwardProcessing());
				}
				return hasOutOfOutwardProcessingProcedureCached.Value;
			}
		}
		CachedProperty<bool> hasOutOfOutwardProcessingProcedureCached;

		public bool IsOutOfOutwardProcessing
		{
			get
			{
				if (isOutOfOutwardProcessingCached == null)
				{
					isOutOfOutwardProcessingCached = new CachedProperty<bool>(Factory, () =>
					{
						var entryInstruction = EntryInstruction;
						var procedure = GetCusProcedure(ProcedureCode);
						return procedure != null && procedure.IsOutOfOutwardProcessing() && (!procedure.IsIntoOutwardProcessing() || (CusEntryLine?.Header?.HasWHSOutwardTransaction ?? false) || (entryInstruction != null && (entryInstruction.ClientIsOutwardProcessing && !entryInstruction.WarehouseIsOutwardProcessing && !entryInstruction.Warehouse2IsOutwardProcessing && !entryInstruction.OwnerIsOutwardProcessing)));
					});
				}
				return isOutOfOutwardProcessingCached.Value;
			}
		}
		CachedProperty<bool> isOutOfOutwardProcessingCached;

		public bool SupportOutOfOutwardProcessing => Declaration != null && Declaration.SupportOutwardProcessing && HasOutOfOutwardProcessingProcedure;

		#endregion

		public bool SupportInvoiceLineComponents => SupportOutOfInwardProcessing || SupportOutOfOutwardProcessing;

		public bool IsInvoiceLineComponentsVisible => SupportInvoiceLineComponents || ComponentInventoryCollection.Count > 0;

		#endregion

		#region guarantee released and consumed

		public bool HasGuaranteeConsumingProcedure
		{
			get
			{
				if (hasGuaranteeConsumingProcedureCached == null)
				{
					hasGuaranteeConsumingProcedureCached = new CachedProperty<bool>(Factory, () =>
					{
						return CusProcedure?.IsGuaranteeConsumed() ?? ZBool.False;
					});
				}
				return hasGuaranteeConsumingProcedureCached.Value;
			}
		}
		CachedProperty<bool> hasGuaranteeConsumingProcedureCached;

		public bool HasGuaranteeReleasedProcedure
		{
			get
			{
				if (hasGuaranteeReleasedProcedureCached == null)
				{
					hasGuaranteeReleasedProcedureCached = new CachedProperty<bool>(Factory, () =>
					{
						return CusProcedure?.IsGuaranteeReleased() ?? ZBool.False;
					});
				}
				return hasGuaranteeReleasedProcedureCached.Value;
			}
		}
		CachedProperty<bool> hasGuaranteeReleasedProcedureCached;

		#endregion

		public ZBool HasAnyProcedureWithSuspendedVat
		{
			get
			{
				if (hasAnyProcedureWithSuspendedVatCached == null)
				{
					hasAnyProcedureWithSuspendedVatCached = new CachedProperty<ZBool>(Factory, () =>
					{
						return !ProcedureIndicatesVATNotApply.IsEmpty;
					});
				}
				return hasAnyProcedureWithSuspendedVatCached.Value;
			}
		}
		CachedProperty<ZBool> hasAnyProcedureWithSuspendedVatCached;

		#region Procedure

		/// <summary>
		/// Base: First two chars of JI_Procedure. ZA: CEI_Style.
		/// </summary>
		public ZString ProcedureCode
		{
			get { return GetProcedureCodeCore(); }
		}

		protected virtual ZString GetProcedureCodeCore()
		{
			// Base assumes first two chars of JI_Procedure are the CPC.
			// ZA will instead use CEI_Style
			return JI_Procedure.Left(2);
		}

		public RefCusProcedure CusProcedure => GetCusProcedure(ProcedureCode);

		protected virtual RefCusProcedure GetCusProcedure(ZString procedureCode)
		{
			if (procedureCode.IsEmpty)
			{
				return null;
			}
			var core = GetCusProcedureCore(procedureCode);
			return Factory.GetCachedValue(core.GetKeyFunc(), core.GetCusProcedureFunc);
		}

		protected virtual (GetValueDelegate<RefCusProcedure> GetCusProcedureFunc, Func<string> GetKeyFunc) GetCusProcedureCore(ZString procedureCode)
		{
			string KeyFunc() => $"BaseCusProcedure_{CustomsCountryCode}_{procedureCode}_{JI_Calc_PreviousProcedure}_{JI_Calc_Concession}";

			RefCusProcedure ProcedureFunc()
			{
				var prevProcedure = JI_Calc_PreviousProcedure;
				var concession = JI_Calc_Concession;
				var today = ZDateTime.Today;
				return !prevProcedure.IsEmpty && !concession.IsEmpty
					? new RefCusProcedure.Loader(Factory).LoadTop1FromFullCodeCurrentPlusPreviousPlusConcession(procedureCode + prevProcedure + concession, CustomsCountryCode, today)
					: new RefCusProcedure.Loader(Factory).LoadTop1FromCodeAndCountry(procedureCode, prevProcedure, CustomsCountryCode, today);
			}

			return (ProcedureFunc, KeyFunc);
		}

		public virtual ZString JI_Calc_PreviousProcedure
		{
			get
			{
				return (JI_Procedure.PadRight(4).SubstringSafe(2, 2)).TrimEnd();
			}
		}
		public virtual ZString JI_Calc_Concession
		{
			get
			{
				return (JI_Procedure.PadRight(4).SubstringSafe(4, 3)).TrimEnd();
			}
		}

		public RefCusProcedure PreviousProcedure
		{
			get
			{
				var procedureCode = JI_Calc_PreviousProcedure;
				if (procedureCode.IsEmpty)
				{
					previousProcedure = null;
				}
				else if (previousProcedure == null || previousProcedure.ZZ6_ProcedureCode != procedureCode)
				{
					previousProcedure = new RefCusProcedure.Loader(Factory).LoadTop1FromCodeAndCountry(procedureCode, ZString.Empty, CustomsCountryCode, ZDateTime.Today);
				}
				return previousProcedure;
			}
		}
		RefCusProcedure previousProcedure;

		#endregion

#if DEBUG
		public void SetIsGoingIntoBondedWarehouseCoreForTesting(bool value)
		{
			overridingIsGoingIntoBondedWarehouseCore = true;
			overrideIsGoingIntoBondedWarehouseCore = value;
			Factory.InvalidateCachedProperties();
		}
		bool overridingIsGoingIntoBondedWarehouseCore;
		bool overrideIsGoingIntoBondedWarehouseCore;
#endif

		public ZString MessageType
		{
			get { return Declaration != null ? Declaration.JE_MessageType : ZString.Empty; }
		}

		#region JI_Calc_Invoice

		[BusinessObjectTestExclude]
		public virtual ZString JI_Calc_Invoice
		{
			get
			{
				ZString result = "";
				if (InvoiceHeader != null)
				{
					result = InvoiceHeader.JZ_InvoiceNumber;
				}
				return result;
			}
			set
			{
				BaseJobComInvoiceHeader oldInvoice = InvoiceHeader;
				var declaration = Declaration;
				if (declaration != null && declaration.SortedInvoiceList != null)
				{
					foreach (BaseJobComInvoiceHeader invoice in declaration.SortedInvoiceList)
					{
						if (invoice.JZ_InvoiceNumber == value)
						{
							JI_JZ = invoice.PK;
							MarkApportionmentDirty(invoice != oldInvoice);
							break;
						}
					}
				}

				if (oldInvoice != InvoiceHeader)
				{
					if (oldInvoice != null)
					{
						oldInvoice.MarkAsNeedingValidation();
					}
					if (InvoiceHeader != null)
					{
						InvoiceHeader.MarkAsNeedingValidation();
					}
				}

				JI_Calc_InvoiceInfo.RefreshBinding();
				(Validation as BaseJobComInvoiceLineValidation)?.ValidateJI_Calc_Invoice();
			}
		}

		[MaxLength(35)]
		public ZPropertyInfo JI_Calc_InvoiceInfo
		{
			get { return GetZPropertyInfo(Schema.JI_Calc_Invoice); }
		}
		#endregion

		#region JI_Calc_OrderLineNumberAndSubLine

		public ZString JI_Calc_OrderLineNumberAndSubLine
		{
			get
			{
				if (OrderLine != null)
				{
					return OrderLine.JO_Calc_OrderLineNoAndSubLineNo;
				}
				return ZString.Empty;
			}
		}

		public ZPropertyInfo JI_Calc_OrderLineNumberAndSubLineInfo
		{
			get
			{
				if (OrderLine != null)
				{
					return GetWrappedZPropertyInfo(Schema.JI_Calc_OrderLineNumberAndSubLine, x => OrderLine.JO_Calc_OrderLineNoAndSubLineNoInfo);
				}
				else
				{
					return GetZPropertyInfo(Schema.JI_Calc_OrderLineNumberAndSubLine);
				}
			}
		}

		#endregion

		#region JI_Calc_FOB

		protected ICustomsValuationCalculator ValuationCalculator => fValuationCalculator ?? (fValuationCalculator = GetValuationCalculatorCore());
		ICustomsValuationCalculator fValuationCalculator;

		protected virtual ICustomsValuationCalculator GetValuationCalculatorCore() => new CustomsValuationCalculator(this);

		CachedProperty<ZDecimal> cachedJI_Calc_FOB;
		public ZDecimal JI_Calc_FOB
		{
			get
			{
				if (cachedJI_Calc_FOB == null)
				{
					cachedJI_Calc_FOB = new CachedProperty<ZDecimal>(Factory, GetJI_Calc_FOB);
				}
				return cachedJI_Calc_FOB.Value;
			}
		}

		protected virtual ZDecimal GetJI_Calc_FOB()
		{
			decimal result = 0m;
			var invoiceHeader = InvoiceHeader; //Caching for performance
			if (invoiceHeader != null && invoiceHeader.Invoice_Currency != null)
			{
				result = ComponentPrice + ValuationCalculator.GetAmountToAddToITOTForDutiable(invoiceHeader.Invoice_Currency);
			}
			return result;
		}

		public ZPropertyInfo JI_Calc_FOBInfo
		{
			get { return GetZPropertyInfo(Schema.JI_Calc_FOB); }
		}

		/// <summary>
		/// Returns the base price actually declared to Customs (equivalent to JI_LinePrice but overridden in USA)
		/// </summary>
		protected virtual ZDecimal ComponentPrice
		{
			get { return JI_LinePrice; }
		}

		#endregion

		#region JI_Calc_FOB_InLocalCurrency

		/// <summary>
		/// In a local currency
		/// </summary>
		public virtual ZDecimal JI_CustomsValue
		{
			get { return JI_Calc_FOB_InLocalCurrency; }
		}

		public virtual ZPropertyInfo JI_CustomsValueInfo
		{
			get { return GetZPropertyInfo(Schema.JI_CustomsValue, "Customs Value"); }
		}

		public ZDecimal JI_Calc_FOB_InLocalCurrency
		{
			get { return InvoiceHeader != null ? ConvertToLocalAmountExact(JI_FOB).Amount : ZDecimal.Zero; }
		}

		#endregion

		#region JI_Calc_CIF

		CachedProperty<ZDecimal> cachedJI_Calc_CIF;
		public ZDecimal JI_Calc_CIF
		{
			get
			{
				if (cachedJI_Calc_CIF == null)
				{
					cachedJI_Calc_CIF = new CachedProperty<ZDecimal>(Factory, GetJI_Calc_CIF);
				}
				return cachedJI_Calc_CIF.Value;
			}
		}

		protected virtual ZDecimal GetJI_Calc_CIF()
		{
			decimal result = 0m;
			if (InvoiceHeader != null && InvoiceHeader.Invoice_Currency != null)
			{
				result = ComponentPrice + ValuationCalculator.GetAmountToAddToITOTForVatableGstable(InvoiceHeader.Invoice_Currency);
			}
			return result;
		}

		public ZPropertyInfo JI_Calc_CIFInfo
		{
			get { return GetZPropertyInfo(Schema.JI_Calc_CIF); }
		}

		public ZDecimal JI_Calc_CIF_InLocalCurrency
		{
			get { return InvoiceHeader != null ? ConvertToLocalAmountExact(JI_CIF).Amount : ZDecimal.Zero; }
		}

		public ZPropertyInfo JI_Calc_CIF_InLocalCurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.JI_Calc_CIF_InLocalCurrency); }
		}

		public ZDecimal JI_Calc_InsuranceInInvoiceCurr
		{
			get
			{
				ZDecimal result = 0;
				if (InvoiceHeader != null && InvoiceHeader.Invoice_Currency != null)
				{
					result = CurrencyConverter.ConvertExact(JI_OverseasInsurance, InvoiceHeader.Invoice_Currency).Amount;
				}
				return result;
			}
		}

		public ZPropertyInfo JI_Calc_InsuranceInInvoiceCurrInfo
		{
			get { return GetZPropertyInfo(nameof(JI_Calc_InsuranceInInvoiceCurr)); }
		}

		public ZDecimal JI_Calc_FreightInInvoiceCurr
		{
			get
			{
				ZDecimal result = 0;
				if (InvoiceHeader != null && InvoiceHeader.Invoice_Currency != null)
				{
					result = CurrencyConverter.ConvertExact(JI_OverseasFreight, InvoiceHeader.Invoice_Currency).Amount;
				}
				return result;
			}
		}

		public ZPropertyInfo JI_Calc_FreightInInvoiceCurrInfo
		{
			get { return GetZPropertyInfo(nameof(JI_Calc_FreightInInvoiceCurr)); }
		}

		#endregion

		#region JI_Calc_LinesTotal/JI_Calc_LinesEntered

		public
#if DEBUG
 virtual
#endif
 ZDecimal JI_Calc_LinesTotal
		{
			get
			{
				if (InvoiceHeader != null)
				{
					return InvoiceHeader.InvoiceLineTotal;
				}
				return 0;
			}
		}
		public ZPropertyInfo JI_Calc_LinesTotalInfo
		{
			get { return GetZPropertyInfo(Schema.JI_Calc_LinesTotal); }
		}

		public ZDecimal JI_Calc_LinesEntered
		{
			get
			{
				if (InvoiceHeader != null)
				{
					return InvoiceHeader.JZ_Calc_LinesEntered;
				}
				return 0;
			}
		}
		public ZPropertyInfo JI_Calc_LinesEnteredInfo
		{
			get { return GetZPropertyInfo(Schema.JI_Calc_LinesEntered); }
		}

		public ZDecimal JI_Calc_Balance
		{
			get
			{
				if (InvoiceHeader != null)
				{
					return InvoiceHeader.JZ_Calc_Balance;
				}
				return 0;
			}
		}
		public ZPropertyInfo JI_Calc_BalanceInfo
		{
			get { return GetZPropertyInfo(Schema.JI_Calc_Balance); }
		}

		#endregion

		#region JI_Calc_OwnerPartNo
		public ZString JI_Calc_OwnerPartNo
		{
			get
			{
				if (cachedJI_Calc_OwnerPartNo == null)
				{
					cachedJI_Calc_OwnerPartNo = new CachedProperty<ZString>(Factory, GetJI_Calc_OwnerPartNo);
				}
				return cachedJI_Calc_OwnerPartNo.Value;
			}
		}
		CachedProperty<ZString> cachedJI_Calc_OwnerPartNo;

		ZString GetJI_Calc_OwnerPartNo()
		{
			return GetLocalPartNumber(ImporterPK_Effective, OrgPartRelation.RelationshipTypes.Owner);
		}

		public ZPropertyInfo JI_Calc_OwnerPartNoInfo
		{
			get { return GetZPropertyInfo(Schema.JI_Calc_OwnerPartNo); }
		}
		#endregion

		#region JI_Calc_SupplierPartNo
		public ZString JI_Calc_SupplierPartNo
		{
			get
			{
				if (cachedJI_Calc_SupplierPartNo == null)
				{
					cachedJI_Calc_SupplierPartNo = new CachedProperty<ZString>(Factory, GetJI_Calc_SupplierPartNo);
				}
				return cachedJI_Calc_SupplierPartNo.Value;
			}
		}
		CachedProperty<ZString> cachedJI_Calc_SupplierPartNo;

		ZString GetJI_Calc_SupplierPartNo()
		{
			return GetLocalPartNumber(SupplierPK_Effective, OrgPartRelation.RelationshipTypes.Supplier);
		}

		public ZPropertyInfo JI_Calc_SupplierPartNoInfo
		{
			get { return GetZPropertyInfo(Schema.JI_Calc_SupplierPartNo); }
		}
		#endregion

		#region JI_RX_NKLinePriceCurr

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CurrencyList))]
		public ZString JI_RX_NKLinePriceCurr
		{
			get
			{
				return JI_RX_NKLinePriceCurrCore;
			}
		}

		protected virtual ZString JI_RX_NKLinePriceCurrCore
		{
			get
			{
				if (InvoiceHeader == null)
				{
					return ZString.Empty;
				}
				else
				{
					RefCurrency currency = RefCurrency.LoadFromCurrencyCode(Factory, InvoiceHeader.JZ_RX_NKInvoice_Currency);
					if (currency != null)
					{
						return currency.RX_Code;
					}
					else
					{
						return ZString.Empty;
					}
				}
			}
		}

		public ZPropertyInfo JI_RX_NKLinePriceCurrInfo
		{
			get { return GetZPropertyInfo(Schema.JI_RX_NKLinePriceCurr); }
		}

		public RefCurrency LinePriceRefCurrency
		{
			get { return RefCurrency.LoadFromCurrencyCode(Factory, JI_RX_NKLinePriceCurr); }
		}

		#endregion

		#region From dbo.CusEntryLine

		#region JI_Calc_DutyAmount

		public virtual ZDecimal JI_Calc_DutyAmount
		{
			get
			{
				var entryLine = CusEntryLine;
				return (entryLine == null) ? new ZDecimal(0m) : GetAmountApportionedFromCusEntryLine(entryLine.DutyAmount).Amount;
			}
		}
		public ZPropertyInfo JI_Calc_DutyAmountInfo
		{
			get { return GetZPropertyInfo(Schema.JI_Calc_DutyAmount); }
		}

		public ZGuid JI_RX_LocalCurrency
		{
			get { return LocalCurrency != null ? LocalCurrency.PK : ZGuid.Empty; }
		}

		public virtual RefCurrency LocalCurrency
		{
			get { return BaseJobComInvoiceHeader.GetLocalCurrencyFor(InvoiceHeader); }
		}

		public ZPropertyInfo JI_RX_LocalCurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.JI_RX_LocalCurrency); }
		}

		public virtual ZDecimal JI_Calc_DutyAmountIncludingWHEstimate
		{
			get { return JI_Calc_DutyAmount; }
		}
		public ZPropertyInfo JI_Calc_DutyAmountIncludingWHEstimateInfo
		{
			get { return GetZPropertyInfo(Schema.JI_Calc_DutyAmountIncludingWHEstimate); }
		}

		#endregion

		#region JI_Calc_GSTVATAmount

		public virtual bool HasGSTVAT
		{
			get { return true; }
		}

		public ZDecimal JI_Calc_GSTVATAmount
		{
			get
			{
				return GetGSTVATAmountCore();
			}
		}

		protected virtual ZDecimal GetGSTVATAmountCore()
		{
			var entryLine = CusEntryLine;
			return (entryLine == null) ? new ZDecimal(0m) : GetAmountApportionedFromCusEntryLine(entryLine.GSTVATAmount).Amount;
		}

		public ZPropertyInfo JI_Calc_GSTVATAmountInfo
		{
			get { return GetZPropertyInfo(Schema.JI_Calc_GSTVATAmount); }
		}

		public ZDecimal GSTVATAmountForLandedCost
		{
			get
			{
				var entryLine = CusEntryLine;
				return (entryLine == null) ? new ZDecimal(0m) : GetAmountApportionedFromCusEntryLine(entryLine.Fees.GetAmountIncludingLCOnly(Declaration?.GSTOrVATCode ?? ZString.Empty)).Amount;
			}
		}

		public virtual bool IsDutyAndTaxEstimatedForWH
		{
			get { return false; }
		}

		public virtual ZDecimal JI_Calc_GSTVATAmountIncludingWHEstimate
		{
			get { return JI_Calc_GSTVATAmount; }
		}
		public ZPropertyInfo JI_Calc_GSTVATAmountIncludingWHEstimateInfo
		{
			get { return GetZPropertyInfo(Schema.JI_Calc_GSTVATAmountIncludingWHEstimate); }
		}
		#endregion

		#region JI_Calc_GSTVATDeferred

		public ZDecimal JI_Calc_GSTVATDeferred
		{
			get { return (CusEntryLine == null) ? new ZDecimal(0m) : GetAmountApportionedFromCusEntryLine(CusEntryLine.GSTVATDeferred).Amount; }
		}

		public ZPropertyInfo JI_Calc_GSTVATDeferredInfo
		{
			get { return GetZPropertyInfo(Schema.JI_Calc_GSTVATDeferred); }
		}

		#endregion

		#region JI_Calc_OtherTaxesAmount
		public ZDecimal JI_Calc_OtherTaxesAmount => Factory.GetValue(ref ji_Calc_OtherTaxAmountCached, GetOtherTaxAmountCore);
		CachedProperty<ZDecimal> ji_Calc_OtherTaxAmountCached;

		protected virtual ZDecimal GetOtherTaxAmountCore()
		{
			var entryLine = CusEntryLine;
			return (entryLine == null) ? new ZDecimal(0m) : GetAmountApportionedFromCusEntryLine(entryLine.TotalDutyAndTaxesAmount - entryLine.DutyAmount - entryLine.GSTVATAmount).Amount;
		}

		public ZPropertyInfo JI_Calc_OtherTaxesAmountInfo => GetZPropertyInfo(Schema.JI_Calc_OtherTaxesAmount);
		#endregion

		protected Money GetAmountApportionedFromCusEntryLine(ZDecimal cusEntryLineAmount)
		{
			ZDecimal amount = 0m;
			ZBool amountIsValid = false;

			if (Declaration != null && !Declaration.IsMergeInProgress && CusEntryLine != null && cusEntryLineAmount.IsValid)
			{
				amount = cusEntryLineAmount * proportionOfCusEntryLine;
				amountIsValid = true;
			}

			return new Money(amount, LocalCurrency, amountIsValid).Round(8);
		}

		ZDecimal proportionOfCusEntryLine
		{
			get
			{
				if (proportionOfCusEntryLineCache == null)
				{
					proportionOfCusEntryLineCache = new CachedProperty<ZDecimal>(Factory, ProportionOfCusEntryLine);
				}
				return proportionOfCusEntryLineCache.Value;
			}
		}

		CachedProperty<ZDecimal> proportionOfCusEntryLineCache;

		protected virtual ZDecimal ProportionOfCusEntryLine()
		{
			ZDecimal result = 1m;

			if (Declaration != null && !Declaration.IsMergeInProgress && CusEntryLine != null && CusEntryLine.InvoiceLines.Count > 1)
			{
				Money cusEntryLineFOBInLocalCurrency = CusEntryLine.FOBInLocalCurrency;

				if (cusEntryLineFOBInLocalCurrency.Amount != 0m)
				{
					result = JI_Calc_FOB_InLocalCurrency / cusEntryLineFOBInLocalCurrency.Amount;
				}
			}

			return result;
		}

		#endregion From dbo.CusEntryLine
		public ZDecimal GSTRate
		{
			get
			{
				ZDecimal result = 0m;
				if (CusEntryLine != null)
				{
					result = CusEntryLine.GSTRate;
				}
				return result;
			}
		}

		#region Header and EffectiveCountryOfOrigin
		public virtual ZString HeaderOrigin
		{
			get { return (InvoiceHeader != null && InvoiceHeader.DefaultOrigin != null) ? InvoiceHeader.JZ_RN_NKDefaultOrigin : ZString.Empty; }
		}

		public ZPropertyInfo HeaderOriginInfo
		{
			get { return GetZPropertyInfo(nameof(HeaderOrigin)); }
		}

		public ZString EffectiveCountryOfOrigin
		{
			get { return EffectiveCountryOfOriginCore; }
		}

		protected virtual ZString EffectiveCountryOfOriginCore
		{
			get { return JI_CountryOfOrigin == ZString.Empty ? HeaderOrigin : JI_CountryOfOrigin; }
		}

		public ZPropertyInfo EffectiveCountryOfOriginInfo
		{
			get { return GetZPropertyInfo(nameof(EffectiveCountryOfOrigin)); }
		}

		public ZString ClassificationDetailsForGenericWrapper
		{
			get { return ClassificationDetailsForGenericWrapperCore; }
		}

		protected virtual ZString ClassificationDetailsForGenericWrapperCore
		{
			get { return TariffFormatter.DisplayFormat(JI_Tariff); }
		}

		#endregion

		#region Money Values

		[Obsolete("Use JI_LinePriceMoney instead", true)]
		public Money JI_LineTotal
		{
			get { return JI_LinePriceMoney; }
		}

		public Money JI_LinePriceMoney
		{
			get
			{
				Money result = Money.Empty;
				if (InvoiceHeader != null)
				{
					result = GetEffectiveMoney(new Money(JI_LinePrice, InvoiceHeader.Invoice_Currency));
				}
				return result;
			}
		}

		public Money JI_LinePriceInLocalCurrencyMoney
		{
			get { return ConvertToLocalAmountExact(JI_LinePriceMoney); }
		}

		public Money JI_FOB
		{
			get
			{
				if (ShouldUseBackRoundedInvoiceLineCV)
				{
					var customsValue = CanPerformApportionmentOfCusEntryLineValues ? Declaration.LCGSTBackRoundingCalculator.GetBackRoundedCustomsValue(PK) : ZDecimal.Zero;
					return new Money(customsValue, LocalCurrency);
				}
				else
				{
					return GetEffectiveMoney(new Money(JI_Calc_FOB, InvoiceHeader.Invoice_Currency));
				}
			}
		}

		bool ShouldUseBackRoundedInvoiceLineCV
		{
			get
			{
				return Declaration != null && Declaration.IsDeclarationIntegrated
					&& InvoiceHeader.JZ_InvoiceAmount > 0 && InvoiceHeader.JZ_Calc_FOBAmountInLocalCurrency == 0;
			}
		}

		public bool CanPerformApportionmentOfCusEntryLineValues
		{
			get
			{
				bool result = false;
				if (CusEntryLine != null)
				{
					var invoiceLines = CusEntryLine.InvoiceLines.Cast<BaseJobComInvoiceLine>();
					result = invoiceLines.Take(2).Count() == 1 || invoiceLines.GroupBy(l => new { l.InvoiceHeader.JZ_RX_NKInvoice_Currency, l.InvoiceHeader.JZ_IncoTerm }).Take(2).Count() == 1;
				}
				return result;
			}
		}

		public Money JI_CIF
		{
			get { return GetEffectiveMoney(new Money(JI_Calc_CIF, InvoiceHeader.Invoice_Currency)); }
		}

		public virtual Money TransportAndInsurance
		{
			get { return CurrencyConverter == null ? Money.Empty : CurrencyConverter.Add(JI_OverseasFreight, JI_OverseasInsurance); }
		}

		public virtual Money JI_OverseasInsurance
		{
			get
			{
				Money result = Money.Empty;

				if (InvoiceHeader != null)
				{
					result = GetCharge(InvoiceHeader.IncoTermAndChargeFactory.GetCharge(ChargeCodeForOverseasInsurance));
				}

				return result;
			}
		}

		public virtual Money JI_OverseasFreight
		{
			get
			{
				Money result = Money.Empty;

				if (InvoiceHeader != null)
				{
					result = GetCharge(InvoiceHeader.IncoTermAndChargeFactory.GetCharge(ChargeCodeForOverseasFreight));
				}

				return result;
			}
		}

		public virtual Money JI_DutiableAdditions
		{
			get
			{
				var result = Money.Empty;
				if (InvoiceHeader != null)
				{
					result = GetCharge(true, false);
				}
				return result;
			}
		}

		public virtual Money JI_NonDutiableDeductions
		{
			get
			{
				var result = Money.Empty;
				if (InvoiceHeader != null)
				{
					result = GetCharge(false, true);
				}
				return result;
			}
		}

		protected virtual string ChargeCodeForOverseasFreight => CustomsChargeTypeList.Codes.OverseasFreight;

		protected virtual string ChargeCodeForOverseasInsurance => CustomsChargeTypeList.Codes.OverseasInsurance;

		public CurrencyConverter CurrencyConverter
		{
			//Issue 00853506 JI_JZ nullified during JobComInvoiceLine.Delete. Trasient problem DO NOT cache converter constructed when Invoice Header is null
			get { return GetCurrencyConverter(); }
		}

		protected virtual CurrencyConverter GetCurrencyConverter()
		{
			return InvoiceHeader == null ? new RefCurrencyCurrencyConverter(Factory) { DateForRate = ZDateTime.Today, RateType = ExchangeRateType.Customs } : InvoiceHeader.CurrencyConverter;
		}

		#endregion

		#region UnitPrice

		#region Is Setting Unit Price?

		public bool IsSettingUnitPrice
		{
			get { return IsFieldSettingInProgress(LinePriceCalculationFieldSettingType.UnitPrice); }
		}

		#endregion

		[DecimalPlaces("UnitPriceDecimalPlaces")]
		[ResourceStringData("2242758F-73F4-4C1A-860E-F31772F25CB3", Caption = "Unit Price")]
		public virtual ZDecimal UnitPrice
		{
			get
			{
				if (unitPrice == null)
				{
					unitPrice = CalculateUnitPrice();
				}
				return unitPrice.Value;
			}
			set
			{
				using (GetNewLinePriceCalculationFieldSettingSupporter(LinePriceCalculationFieldSettingType.UnitPrice))
				{
					ZDecimal oldValue = UnitPrice;
					unitPrice = value;

					if (!IsValidationSuspended)
					{
						(Validation as BaseJobComInvoiceLineValidation)?.ValidateUnitPrice();
					}

					if (!IsCopying && oldValue != UnitPrice)
					{
						UpdateLinePriceIfChangedFromUnitPriceChanges();
					}
					UnitPriceInfo.RefreshBinding(oldValue);
				}
			}
		}
		ZDecimal? unitPrice;

		protected virtual int UnitPriceDecimalPlaces => 4;

		public ZPropertyInfo UnitPriceInfo
		{
			get { return GetZPropertyInfo(Schema.UnitPrice); }
		}

		public ZDecimal UnitPriceInLocalCurrency
		{
			get { return ConvertToLocalAmountExact(new Money(UnitPrice, LinePriceRefCurrency)).Amount; }
		}

		#endregion

		protected internal virtual bool IsValidForLineTotalCalculation
		{
			get { return true; }
		}

		public ZBool IsContainerisedMode
		{
			get
			{
				var containerMode = JI_ContainerMode;

				if (containerMode.IsEmpty && Declaration != null)
				{
					containerMode = Declaration.JE_ContainerMode;
				}

				return IsContainerisedModeCore(containerMode);
			}
		}

		protected virtual bool IsContainerisedModeCore(string containerMode)
		{
			return containerMode == Core.Constants.ContainerModes.FCL
					|| containerMode == Core.Constants.ContainerModes.LCL
					|| containerMode == Core.Constants.ContainerModes.FCLMixedShipper
					|| containerMode == Core.Constants.ContainerModes.Containerised;
		}

		public virtual bool IsContainerLinkMandatory
		{
			get { return true; }
		}

		public virtual ZString PermitNumber1
		{
			get { return ZString.Empty; }
		}

		public virtual ZString PermitNumber2
		{
			get { return ZString.Empty; }
		}

		public virtual ZString PermitNumber3
		{
			get { return ZString.Empty; }
		}

		public virtual ZString TempImportNum
		{
			get { return ZString.Empty; }
		}

		public ZBool IsAdvanceShippingNoticeInvoice
		{
			get
			{
				var invoiceHeader = InvoiceHeader;
				return invoiceHeader != null && invoiceHeader.IsAdvanceShippingNotice;
			}
		}

		[ReadOnlyMember(nameof(IsClassUsageCommentRead_ReadOnly))]
		public ZBool JI_IsClassUsageCommentRead
		{
			get => !JI_GS_NKClassUsageCommentReviewer.IsEmpty;
			set
			{
				JI_GS_NKClassUsageCommentReviewer = value ? GlbStaff.CurrentUser.GS_Code : ZString.Empty;

				if (!IsCopying)
				{
					(Validation as BaseJobComInvoiceLineValidation)?.ValidateJI_IsClassUsageCommentRead();
					JI_IsClassUsageCommentReadInfo.RefreshBinding();
				}
			}
		}

		public bool IsClassUsageCommentRead_ReadOnly
		{
			get { return JI_ClassUsageComment.IsEmpty && JI_GS_NKClassUsageCommentReviewer.IsEmpty; }
		}

		public ZPropertyInfo JI_IsClassUsageCommentReadInfo => GetZPropertyInfo(nameof(JI_IsClassUsageCommentRead));

		public ZBool IsPreviousEntryNumberVisible => IsPreviousEntryNumberVisibleCore || AtLeastOneBWHPropertyHasValue;

		protected virtual ZBool IsPreviousEntryNumberVisibleCore => false;

		protected virtual bool AtLeastOneBWHPropertyHasValue => !JI_PreviousEntryNumber.IsEmpty || !JI_PreviousEntryLineNumber.IsEmpty || !JI_BondedWhsQuantity.IsEmpty;

		#endregion

		#region Methods overrides

		public override void Delete()
		{
			var linesNeedToResetInvoiceHeadersAndLines = new List<CusEntryLine> { CusEntryLine };
			linesNeedToResetInvoiceHeadersAndLines.AddRange(AdditionalEntryLineLinks.AdditionalEntryLines);
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
			}

			FetchForLoadChildEditableObjectsIfNeeded();
			this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();

			customsUnitDefaultingStrategy?.Value?.Deinitialise(this);

			var invoiceHeader = InvoiceHeader;

			if (invoiceHeader != null)
			{
				invoiceHeader.InvoiceLineLineNumberGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
				invoiceHeader.ResetHasMultipleInvoiceUQs();
			}

			var linePriceForWeightApportionCalculation = LinePriceForWeightApportionCalculation;
			if (invoiceHeader != null)
			{
				Charges.RemoveAndDeleteAll();
				ApportionedCharges.RemoveAndDeleteAll();
			}

			AdditionalEntryLineLinks.DeleteAll();
			ContainersPivot.DeleteAll();
			if (SupportsChcPivotBetweenInvoiceLineAndPacking)
			{
				PackagesPivot.DeleteAll();
			}
			if (SupportInvoiceLineRefs)
			{
				InvoiceLineRefs.DeleteAll();
			}
			if (SupportRulingConfigurations)
			{
				RulingConfigurations.DeleteAll();
			}
			if (Declaration?.SupportInwardProcessing ?? false)
			{
				ComponentInventoryCollection.RemoveAndDeleteAll();
			}
			if (SupportsAdditionalTariffs)
			{
				CusLineTariffDetails.DeleteAll();
			}

			this.DeleteChildren<CusPackableItem>(CusPackableItemSchema.CUI_JI);
			if (VehicleRelationship != VehicleRelationshipType.None)
			{
				Vehicles.RemoveAndDeleteAll();
			}

			WorkflowItems.RemoveAndDeleteAll();

			// Caution - two base.Delete() in here...
			if (PartSyncManager != null)
			{
				bool originalEnabledState = PartSyncManager.Enabled;
				PartSyncManager.Enabled = false;
				base.Delete();
				if (!IsDeleted)
				{
					PartSyncManager.Enabled = originalEnabledState;
				}
			}
			else
			{
				base.Delete();
			}

			if (invoiceHeader != null)
			{
				invoiceHeader.ReApportionLineWeightIfNeeded(!linePriceForWeightApportionCalculation.IsEmpty);
			}

			linesNeedToResetInvoiceHeadersAndLines.ForEach(x => x?.Header?.ResetInvoiceHeadersAndLines());

			if (IsDebugLogEnabled)
			{
				DebugLog.AppendLine(FormattableString.Invariant($"{ZDateTime.Now.ToISO8601String()}:\tJobComInvoiceLine delete. \r\n{System.Environment.StackTrace}"));
			}
		}

		/// <summary>
		/// We need these members on the invoice line because the invoice line needs to be able to say yes or no to supporting CHC even when it has no declaration attached.  This occurs when the Z framework transiently adds and then removes an uncommitted row to the invoice lines grid after you've saved the facotry.
		/// Before, we relied on the declaration to determine support for CHC. Having seen no declaration, we answered no, and so during JobComInvoiceLine.Delete() we didn't delete the CHCs.
		/// This way, we can still delete the CHCs during JobComInvoiceLine.Delete() even when there's no declaration.
		/// </summary>
		public bool SupportsChcPivotBetweenInvoiceLineAndPacking
		{
			get { return Declaration == null || Declaration.SupportsChcPivotBetweenInvoiceLineAndPacking; }
		}

		public override string ToString()
		{
			return Res.GetString("d35e77ae-c338-4eb7-9cfc-4c40e8a62159", "Line: {0} - {1}", JI_LineNo.ToString(), JI_Description);
		}

		#endregion

		#region New Methods

		public virtual void UpdateProductDetailsOnSupplierBuyerChange()
		{
			if (!IsCopying)
			{
				PartSyncManager.Refresh();
			}
		}

		public bool IsNonContainerised
		{
			get
			{
				var query = new ZQuery(CusContainerInvoiceLinePivotSchema.C2_JI, PK) { FetchOnlyFromLocalCache = !IsInDatabase };
				return Factory.LoadTop1<CusContainerInvoiceLinePivot>(query) == null;
			}
		}

		public bool IsContainedIn(BaseCusContainer container)
		{
			return container != null && ContainersPivot.Contains(container);
		}

		public bool IsSplitToMultiContainers
		{
			get { return ContainersPivot.Count > 1; }
		}

		public bool IsSingleAssociationWithContainer
		{
			get { return ContainersPivot.Count == 1; }
		}

		/// <summary>
		/// This is a parent for secondary tariff lines and also, for a set component line
		/// </summary>
		public BaseJobComInvoiceLine ParentTariffLine
		{
			get
			{
				if (IsDebugLogEnabled && !debugLogSent && IsDeleted && parentTariffLine != null && !parentTariffLine.IsDeleted)
				{
					debugLogSent = true;
					ExceptionReporter.Instance.ReportDeveloperException(
						FormattableString.Invariant($"{nameof(BaseJobComInvoiceLine)}.{nameof(ParentTariffLine)}.{nameof(IsDeleted)}"),
						FormattableString.Invariant($"Line was deleted at:\r\n{DebugLog}"),
						new InvalidOperationException("Deleted row information cannot be accessed through the row.")
					);
				}

				if ((parentTariffLine == null || parentTariffLine.IsDeleted || parentTariffLine.PK != JI_ParentID) && !IsDeleted)
				{
					parentTariffLine = JI_ParentID == PK ? null : Factory.Load<BaseJobComInvoiceLine>(JI_ParentID);
				}

				return parentTariffLine;
			}
		}
		BaseJobComInvoiceLine parentTariffLine;

		protected internal ZStringBuilder DebugLog => debugLog ?? (debugLog = new ZStringBuilder());
		ZStringBuilder debugLog;

		[WTG.StaticAnalysis.Annotation.ThreadSafe(WTG.StaticAnalysis.Annotation.ThreadSafeAttribute.Mechanism.Interlocked)]
		static volatile int debugLogEnabled;

		[WTG.StaticAnalysis.Annotation.ThreadSafe(WTG.StaticAnalysis.Annotation.ThreadSafeAttribute.Mechanism.Interlocked)]
#if DEBUG
		internal
#endif
		static volatile bool debugLogSent;

		protected internal IDisposable EnableDebugLog() => new DisposableAction(StartLogging, EndLogging);

		bool IsDebugLogEnabled => debugLogEnabled > 0;

		void StartLogging() => System.Threading.Interlocked.Increment(ref debugLogEnabled);

		void EndLogging() => System.Threading.Interlocked.Decrement(ref debugLogEnabled);

		public bool IsChildLine
		{
			get { return ParentTariffLine != null; }
		}

		#endregion

		#region Property Overrides

		public override ZString JI_DataModel
		{
			get { return base.JI_DataModel; }
			set
			{
				this.ReportDataModelErrorIfNeeded(JI_DataModelInfo, value);
				base.JI_DataModel = value;
			}
		}

		[ReadOnly(true)]
		public override ZString JI_ClassUsageComment
		{
			get => base.JI_ClassUsageComment;
			set
			{
				if (JI_ClassUsageComment != value)
				{
					base.JI_ClassUsageComment = value;

					if (!IsCopying)
					{
						JI_GS_NKClassUsageCommentReviewer = ZString.Empty;
						JI_IsClassUsageCommentReadInfo.RefreshBinding();
					}
				}
			}
		}

		[ReadOnly(true)]
		public override ZString JI_GS_NKClassUsageCommentReviewer
		{
			get => base.JI_GS_NKClassUsageCommentReviewer;
			set
			{
				if (JI_GS_NKClassUsageCommentReviewer != value)
				{
					base.JI_GS_NKClassUsageCommentReviewer = value;

					if (!IsCopying)
					{
						JI_IsClassUsageCommentReadInfo.RefreshBinding();
					}
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.RelatedIndicatorList))]
		[ResourceStringData("D5B00A29-1D0D-4F12-8C37-4EE2353FA1FE", Caption = "Related Indicator")]
		public override ZString JI_RelatedIndicator { get => base.JI_RelatedIndicator; set => base.JI_RelatedIndicator = value; }

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.ValuationCodeList))]
		[ResourceStringData("1C4D026F-EF02-4973-8837-68B7326E5FAD", Caption = "Valuation Code")]
		public override ZString JI_ValuationCode { get => base.JI_ValuationCode; set => base.JI_ValuationCode = value; }

		[List(nameof(JI_OA_ExporterAddress_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		[ResourceStringData("C1B02696-3D8A-4F45-B508-4DF98E8649EA", Caption = "Exporter")]
		public override ZGuid JI_OA_ExporterAddress
		{
			get { return base.JI_OA_ExporterAddress; }
			set { base.JI_OA_ExporterAddress = value; }
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.BondedWhsUnitQtyList))]
		public override ZString JI_BondedWhsUnitQty
		{
			get { return base.JI_BondedWhsUnitQty; }
			set
			{
				var oldValue = JI_BondedWhsUnitQty;
				base.JI_BondedWhsUnitQty = value;
				if (!IsCopying && oldValue != JI_BondedWhsUnitQty)
				{
					CalculateBondedWhsQuantityFromInvoiceQuantity();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.HazardousMaterialCodeQualifierList))]
		[ResourceStringData("7BA0F5ED-9C69-466E-9650-9EFBEBABC65D", ShortCaption = "Hazmat Code Qua.", Caption = "Hazmat Code Qualifier")]
		public override ZString JI_HazMatCodeQualifier
		{
			get { return base.JI_HazMatCodeQualifier; }
			set { base.JI_HazMatCodeQualifier = value; }
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.RefCountryStates))]
		[ResourceStringData("35C76F10-1E15-47D1-B13A-CDDBD6BE9A78", Caption = "Origin State")]
		public override ZString JI_StateOrRegionOfOrigin
		{
			get => base.JI_StateOrRegionOfOrigin;
			set => base.JI_StateOrRegionOfOrigin = value;
		}

		[ResourceStringData("7B4161CF-0FCF-4DD3-B411-B9E10401BED0", Caption = "Whs. Order No.")]
		public override ZString JI_BondedWHSOrderNumber
		{
			get => base.JI_BondedWHSOrderNumber;
			set => base.JI_BondedWHSOrderNumber = value;
		}

		[ResourceStringData("D8EA8851-B777-4DF3-9E10-7F274C72411A", Caption = "Whs. Order Line No.")]
		public override ZShort JI_BondedWHSOrderLineNumber
		{
			get => base.JI_BondedWHSOrderLineNumber;
			set => base.JI_BondedWHSOrderLineNumber = value;
		}

		public ZBool IsBondedWHSOrderNumberVisible => IsWarehouseOrderEnabled || BondedWHSOrderNumberHasValue;

		public bool IsWarehouseOrderEnabled => (Declaration?.IsWarehouseOrderFunctionActivated ?? false) && IsWarehouseOrderEnabledCore;

		protected virtual bool IsWarehouseOrderEnabledCore => (EntryInstruction?.WarehouseIsBondedWarehousing ?? false) && HasOutOfWarehouseProcedure;

		bool BondedWHSOrderNumberHasValue => !JI_BondedWHSOrderNumber.IsEmpty || !JI_BondedWHSOrderLineNumber.IsEmpty;

		#region override Caption

		[ResourceStringData("BAFEAEF4-70A1-416F-A902-61B3E7DC1DAA", Caption = "Country/Region Of Export", ShortCaption = "Ctry./Rgn. of Exp.")]
		public override ZString JI_RN_NKCountryOfExport { get => base.JI_RN_NKCountryOfExport; set => base.JI_RN_NKCountryOfExport = value; }

		public IRefCountry CountryOfExportFallback => CountryOfExportFallbackCore;
		protected virtual IRefCountry CountryOfExportFallbackCore => CountryOfExport != null ? new DataTransferCountryInfo(CountryOfExport.RN_Code, CountryOfExport.RN_Desc) : null;

		[ResourceStringData("3E4303FA-F774-43BF-8083-66A59B810343", Caption = "Sold To Party")]
		public override ZGuid JI_OA_SoldToPartyAddress { get => base.JI_OA_SoldToPartyAddress; set => base.JI_OA_SoldToPartyAddress = value; }

		[ResourceStringData("49EFFE25-BCAB-4F4C-BEB7-92483B891438", Caption = "Ship To Party")]
		public override ZGuid JI_OA_ShipToPartyAddress { get => base.JI_OA_ShipToPartyAddress; set => base.JI_OA_ShipToPartyAddress = value; }

		[ResourceStringData("03C461CF-F345-45AB-9BDC-4A77351CA44B", Caption = "Seller")]
		public override ZGuid JI_OA_Seller { get => base.JI_OA_Seller; set => base.JI_OA_Seller = value; }

		[ResourceStringData("EBC9A8F6-58EB-4C94-BA9E-20EE94FAD643", Caption = "Grower")]
		public override ZGuid JI_OA_GrowerAddress { get => base.JI_OA_GrowerAddress; set => base.JI_OA_GrowerAddress = value; }

		[ResourceStringData("EF6C2E2D-BBD6-4EF7-BE14-0698947BEAF3", Caption = "Producer")]
		public override ZGuid JI_OA_ProducerAddress { get => base.JI_OA_ProducerAddress; set => base.JI_OA_ProducerAddress = value; }

		[ResourceStringData("249A246F-2322-497E-A5AD-1C5CCFCEC0D1", Caption = "Treatment Provider")]
		public override ZGuid JI_OH_TreatmentProvider { get => base.JI_OH_TreatmentProvider; set => base.JI_OH_TreatmentProvider = value; }

		[ResourceStringData("E2AB3457-8C53-4C02-9480-79BE3FFCFDD2", MediumCaption = "Bonded Warehouse Qty", Caption = "Bonded Warehouse Quantity", ShortCaption = "Bonded Whs. Qty")]
		public override ZDecimal JI_BondedWhsQuantity
		{
			get => base.JI_BondedWhsQuantity;
			set => base.JI_BondedWhsQuantity = value;
		}

		public ZBool IsBondedWhsQuantityVisible => IsBondedWhsQuantityVisibleCore || AtLeastOneBWHPropertyHasValue;

		protected virtual ZBool IsBondedWhsQuantityVisibleCore => false;

		[ResourceStringData("014444E1-0191-460D-9314-F612CA0CFDDD", Caption = "Valuation Markup")]
		public override ZDecimal JI_ValuationMarkup { get => base.JI_ValuationMarkup; set => base.JI_ValuationMarkup = value; }

		[ResourceStringData("96EE4D12-27E5-419D-8A8B-1D11D5D941A1", Caption = "Hazmat Code")]
		public override ZString JI_HazMatCode { get => base.JI_HazMatCode; set => base.JI_HazMatCode = value; }

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.ContainerModeList))]
		[ResourceStringData("02645403-A364-4079-851D-3D3ABCF794D0", Caption = "Container Mode")]
		public override ZString JI_ContainerMode { get => base.JI_ContainerMode; set => base.JI_ContainerMode = value; }

		[ResourceStringData("4CB93B5E-22F9-4AE7-9BD8-88DAB4724C00", ShortCaption = "Secondary Pref.", Caption = "Secondary Preference")]
		public override ZString JI_SecondaryPreference { get => base.JI_SecondaryPreference; set => base.JI_SecondaryPreference = value; }

		[ResourceStringData("A610CFA6-7EB5-4471-999D-D26EAB407126", Caption = "Model")]
		public override ZString JI_Model { get => base.JI_Model; set => base.JI_Model = value; }

		[ResourceStringData("7DD0D85C-0189-45A8-AE29-62666110CDC8", Caption = "Brand Name")]
		public override ZString JI_BrandName { get => base.JI_BrandName; set => base.JI_BrandName = value; }

		[ResourceStringData("5AC65696-888F-4E02-91B4-1871E7B85D94", Caption = "Description")]
		public override ZString JI_NDescription { get => base.JI_NDescription; set => base.JI_NDescription = value; }

		#endregion

		#region JI_RH_NKCommodity_Code
		public override ZString JI_RH_NKCommodity_Code
		{
			get { return base.JI_RH_NKCommodity_Code; }
			set
			{
				if (Declaration != null && Declaration.CustomsEntryHeaders.Count > 0)
				{
					using (new MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(Declaration))
					{
						base.JI_RH_NKCommodity_Code = value;
					}
				}
				else
				{
					base.JI_RH_NKCommodity_Code = value;
				}
			}
		}
		#endregion

		[RelatedBusinessObject("InvoiceHeader")]
		public override ZGuid JI_JZ
		{
			get { return base.JI_JZ; }
			set
			{
				var oldValue = JI_JZ;
				var oldInvoiceHeader = InvoiceHeader;
				if (value.IsEmpty && !IsCopying)
				{
					jI_JZCachedOnRelationshipResetByCore = base.JI_JZ;
				}
				base.JI_JZ = value;
				if (oldValue != JI_JZ && !IsCopying)
				{
					fDeclaration = null;
					ClearJI_ParentIDIfNeeded();
					RefreshPartSyncManagerActiveDeciderPK(oldInvoiceHeader?.JZ_JE ?? oldValue);
					var declaration = Declaration;
					if (declaration == null || !declaration.IsCloning)
					{
						UpdatePartSyncManagerAndRefresh(!IsAdvanceShippingNoticeInvoice);
					}
					SetLineNoOnSettingJI_JZ(oldInvoiceHeader);

					if (oldInvoiceHeader != null)
					{
						oldInvoiceHeader.ReApportionLineWeightIfNeeded(!LinePriceForWeightApportionCalculation.IsEmpty);
						oldInvoiceHeader.ResetHasMultipleInvoiceUQs();
					}
					if (InvoiceHeader != null)
					{
						InvoiceHeader.ReApportionLineWeightIfNeeded(!LinePriceForWeightApportionCalculation.IsEmpty);
						InvoiceHeader.ResetHasMultipleInvoiceUQs();
					}

					if (((ISupportDataImporting)this).IsImportingData)
					{
						if (oldInvoiceHeader != null && InvoiceHeader != null)
						{
							oldInvoiceHeader.JobComInvoiceLines.Rebuild();
							InvoiceHeader.JobComInvoiceLines.Rebuild();
						}
					}

					oldInvoiceHeader?.InvalidateJZ_Calc_LinesEnteredCache();
					InvoiceHeader?.InvalidateJZ_Calc_LinesEnteredCache();
					ContainersPivot.MarkAsNeedingValidation();
				}
			}
		}
		ZGuid jI_JZCachedOnRelationshipResetByCore;

#if DEBUG
		public void WipeJzForTesting()
		{
			JI_JZ = ZGuid.Empty;
			jI_JZCachedOnRelationshipResetByCore = ZGuid.Empty;
			RefreshDeclaration();
		}
#endif

		void ClearJI_ParentIDIfNeeded()
		{
			if (!JI_ParentID.IsEmpty)
			{
				BaseJobComInvoiceLine parentTariffLine = ParentTariffLine;
				if (parentTariffLine == null || (parentTariffLine.JI_JZ != JI_JZ && !AllowParentTariffLineAndThisLineHavingDifferentHeader))
				{
					JI_ParentID = ZGuid.Empty;
				}
			}
		}

		public virtual void UpdatePartSyncManagerAndRefresh(bool enabledStatus)
		{
			if (PartSyncManager != null)
			{
				PartSyncManager.Enabled = enabledStatus;
				if (!JI_PartNo.IsEmpty)
				{
					PartSyncManager.Refresh();
				}
				if (!enabledStatus)
				{
					JI_OP = ZGuid.Empty;
					PartSyncManager.ClearPart();
				}
			}
		}

		protected virtual bool AllowParentTariffLineAndThisLineHavingDifferentHeader
		{
			get { return false; }
		}

		public override ZString JI_Description
		{
			get { return base.JI_Description; }
			set
			{
				ZString oldValue = JI_Description;
				base.JI_Description = value;
				if (!IsCopying && oldValue != JI_Description && value.IsEmpty)
				{
					JI_ExtraInfoForClassification = ZString.Empty;
				}
			}
		}

		[ReadOnlyMember(nameof(JI_ExtraInfoForClassification_ReadOnly))]
		[ResourceStringData("30883F40-A518-48A8-8346-4C12BAEDF155", Caption = "Extra Info For Classification")]
		public override ZString JI_ExtraInfoForClassification
		{
			get { return base.JI_ExtraInfoForClassification; }
			set { base.JI_ExtraInfoForClassification = value; }
		}

		protected virtual bool JI_ExtraInfoForClassification_ReadOnly
		{
			get { return JI_Description.IsEmpty; }
		}

		[List(nameof(JI_OA_ManufacturerAddress_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		[ResourceStringData("743E1E9F-F53F-434C-A1C0-111530AE5525", Caption = "Manufacturer")]
		public override ZGuid JI_OA_ManufacturerAddress
		{
			get { return base.JI_OA_ManufacturerAddress; }
			set { base.JI_OA_ManufacturerAddress = value; }
		}

		[List(nameof(JI_OA_ConsigneeAddress_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		[ResourceStringData("16780AFE-4B5F-44D2-84DE-3E953929D2C2", Caption = "Consignee")]
		public override ZGuid JI_OA_ConsigneeAddress
		{
			get { return base.JI_OA_ConsigneeAddress; }
			set { base.JI_OA_ConsigneeAddress = value; }
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.TaxOrFeeCodeList))]
		[ResourceStringData("949651b3-ad98-447a-8c19-adf5431957fb", Caption = "Tax Type Code")]
		public override ZString JI_ZZF_NKTaxType
		{
			get { return base.JI_ZZF_NKTaxType; }
			set { base.JI_ZZF_NKTaxType = value; }
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.PrimaryPreferenceList))]
		[ResourceStringData("db90f1ba-ce77-4233-ae11-79617f811ba9", Caption = "Preference")]
		public override ZString JI_PrimaryPreference { get => base.JI_PrimaryPreference; set => base.JI_PrimaryPreference = value; }

		public ZString EffectivePrimaryPreference => EffectivePrimaryPreferenceCore;

		protected virtual ZString EffectivePrimaryPreferenceCore => JI_PrimaryPreference;

		#endregion

		#region Universal Copy
		protected void FinishUniversalCopy()
		{
			FinishUniversalCopyCore(Factory.ServiceContainer.GetService<BusinessObjectUniversalCopyFactoryService>());
		}

		protected virtual void FinishUniversalCopyCore(BusinessObjectUniversalCopyFactoryService factoryCopyService)
		{
			if (!JI_JZ.IsEmpty && factoryCopyService != null)
			{
				factoryCopyService.AddOnCopyFinishedAction(() => SetLineNoOnSettingJI_JZ(null));
			}
		}
		#endregion

		#region SetLineNoOnSettingJI_JZ
		void SetLineNoOnSettingJI_JZ(BaseJobComInvoiceHeader oldInvoiceHeader)
		{
			if (!IsCopying)
			{
				if (oldInvoiceHeader != null)
				{
					oldInvoiceHeader.InvoiceLineLineNumberGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
				}

				if (InvoiceHeader is BaseJobComInvoiceHeader invoiceHeader && !invoiceHeader.IsUniversalCopying)
				{
					invoiceHeader.InvoiceLineLineNumberGenerator.RecalculateWhenAdded(this);
				}
			}
		}

		[BusinessObjectTestExclude]
		public override ZShort JI_LineNo
		{
			get { return base.JI_LineNo; }
			set
			{
				if (value > 0)
				{
					ZShort oldValue = JI_LineNo;

					base.JI_LineNo = value;

					if (!IsCopying && InvoiceHeader != null)
					{
						InvoiceHeader.InvoiceLineLineNumberGenerator.RecalculateWhenRenumbered(this, oldValue);
					}
				}
			}
		}

		#region JI_AddInfo

		[BusinessObjectTestExclude]
		public override ZString JI_AddInfo
		{
			get
			{
				return this is INAddInfoSupporter ? AddInfoParser.ConcatAddInfoStrings(base.JI_AddInfo, base.JI_NAddInfo) : base.JI_AddInfo;
			}
			set
			{
				if (this.GetBaseAddInfoWithNAddInfoSupport() is BaseAddInfo addInfo)
				{
					var addInfoStrings = addInfo.SplitAddInfoString(value);

					base.JI_AddInfo = addInfoStrings.Item1;
					base.JI_NAddInfo = addInfoStrings.Item2;
				}
				else
				{
					base.JI_AddInfo = value;
				}
			}
		}

		#endregion

		#region JI_LinePrice

		#region Is Setting Line Price?

		public bool IsSettingLinePrice
		{
			get { return IsFieldSettingInProgress(LinePriceCalculationFieldSettingType.LinePrice); }
		}

		#endregion

		public virtual ZDecimal LinePriceForBalanceCalc
		{
			get { return JI_LinePrice; }
		}

		[DecimalPlaces(2)]
		public override ZDecimal JI_LinePrice
		{
			get { return base.JI_LinePrice; }
			set
			{
				if (IsSettingLinePrice)
				{
					ErrorReporter.ReportOnce("BaseJobComInvoiceLine.JI_LinePrice_Set",
						"Called JI_LinePrice from within itself" + System.Environment.NewLine +
						"Original Value = " + JI_LinePrice.ToString(2) + " with PK = " + PK + System.Environment.NewLine +
						"New Value = " + value.ToString(2));
				}
				else
				{
					using (GetNewLinePriceCalculationFieldSettingSupporter(LinePriceCalculationFieldSettingType.LinePrice))
					{
						var newValue = ZArchitecture.Core.Utilities.Round(value, 2);
						var oldValue = JI_LinePrice;
						base.JI_LinePrice = newValue;
						if (!IsCopying && oldValue != JI_LinePrice)
						{
							UpdateUnitPriceIfChangedFromLinePriceChanges();
							if (ShouldRecalculatePercentageChargeAmountBasedOnLinePrice)
							{
								foreach (BaseInvoiceLineCharge lineCharge in Charges)
								{
									lineCharge.CalculateAmountBasedOnPercentageIfNecessary();
								}
							}
							ContainersPivot.MarkAsNeedingValidation();
							MarkApportionmentDirty(true);

							if (InvoiceHeader is BaseJobComInvoiceHeader invoiceHeader)
							{
								invoiceHeader.InvalidateJZ_Calc_LinesEnteredCache();
								invoiceHeader.MarkAsNeedingValidation();
								if (ShouldReApportionWeightOnLinePriceChange)
								{
									invoiceHeader.ApportionLineWeight(this);
								}
							}
						}
					}
				}
			}
		}

		protected virtual bool ShouldReApportionWeightOnLinePriceChange
		{
			get { return true; }
		}

		//SG means percentage of CIF.
		protected virtual internal bool ShouldRecalculatePercentageChargeAmountBasedOnLinePrice
		{
			get { return true; }
		}
		#endregion

		#region Apportionment Dirty

		[DecimalPlaces(3)]
		[MeasureUnit(Schema.JI_WeightUQ, MeasureUnitType.Weight)]
		public override ZDecimal JI_Weight
		{
			get { return base.JI_Weight; }
			set
			{
				var oldValue = JI_Weight;
				base.JI_Weight = value;
				if (!IsCopying && oldValue != JI_Weight)
				{
					MarkApportionmentDirty(HasParentChargeDistributedByThisToMarkApportionmentDirty(ChargeDistributeByList.Codes.Weight));
				}
			}
		}

		[DecimalPlaces(3)]
		[MeasureUnit(Schema.JI_NetWeightUQ, MeasureUnitType.Weight)]
		public override ZDecimal JI_NetWeight
		{
			get { return base.JI_NetWeight; }
			set
			{
				var oldValue = JI_NetWeight;
				base.JI_NetWeight = value;
				if (!IsCopying && oldValue != JI_NetWeight)
				{
					CalculateFromNetWeightToCustomsQty();
					ContainersPivot.MarkAsNeedingValidation();
				}
			}
		}

		public virtual void CalculateFromNetWeightToCustomsQty()
		{
			CustomsQuantityConverter.CalculateFromNetWeightToCustomsQty();
			if (CustomsQuantity2Converter != null)
			{
				CustomsQuantity2Converter.CalculateFromNetWeightToCustomsQty();
			}
		}

		public virtual bool CanConvertFromNetWeightToCustomsUnit(ZString customsUnit)
		{
			return JI_NetWeight > 0m && Core.Constants.Weight.ContainsCode(JI_NetWeightUQ) && Core.Constants.Weight.ContainsCode(customsUnit);
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.WeightUQList))]
		public override ZString JI_WeightUQ
		{
			get { return base.JI_WeightUQ; }
			set
			{
				var oldValue = JI_WeightUQ;
				base.JI_WeightUQ = value;
				if (!IsCopying && oldValue != JI_WeightUQ)
				{
					MarkApportionmentDirty(HasParentChargeDistributedByThisToMarkApportionmentDirty(ChargeDistributeByList.Codes.Weight));
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.WeightUQList))]
		public override ZString JI_NetWeightUQ
		{
			get { return base.JI_NetWeightUQ; }
			set
			{
				var oldValue = JI_NetWeightUQ;
				base.JI_NetWeightUQ = value;
				if (oldValue != JI_NetWeightUQ && !IsCopying)
				{
					CalculateFromNetWeightToCustomsQty();
					ContainersPivot.MarkAsNeedingValidation();
				}
			}
		}

		[DecimalPlaces(3)]
		[MeasureUnit(Schema.JI_VolumeUQ, MeasureUnitType.Volume)]
		public override ZDecimal JI_Volume
		{
			get { return base.JI_Volume; }
			set
			{
				var oldValue = JI_Volume;
				base.JI_Volume = value;
				if (!IsCopying && oldValue != JI_Volume)
				{
					MarkApportionmentDirty(HasParentChargeDistributedByThisToMarkApportionmentDirty(ChargeDistributeByList.Codes.Volume));
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.VolumeUQList))]
		public override ZString JI_VolumeUQ
		{
			get { return base.JI_VolumeUQ; }
			set
			{
				var oldValue = JI_VolumeUQ;
				base.JI_VolumeUQ = value;
				if (!IsCopying && oldValue != JI_VolumeUQ)
				{
					MarkApportionmentDirty(HasParentChargeDistributedByThisToMarkApportionmentDirty(ChargeDistributeByList.Codes.Volume));
				}
			}
		}

		protected bool HasParentChargeDistributedByThisToMarkApportionmentDirty(string distributeBy)
		{
			return !IsCopying &&
				Declaration != null &&
				!Declaration.ApportionmentDirty &&
				this.HasParentChargeDistributedByThisField(distributeBy);
		}

		protected void MarkApportionmentDirty(bool markIt)
		{
			if (!IsCopying && Declaration != null && markIt)
			{
				Declaration.MarkApportionmentDirty();
			}
		}

		public ZDecimal MinimumReapportionedLineWeight { get => Master.MinimumReapportionedLineWeight; }

		#endregion

		public ZDecimal JI_LinePriceInLocalCurrency
		{
			get { return JI_LinePriceInLocalCurrencyMoney.Amount; }
		}

		internal bool AutoAddOrderNumberOnSet = true;

		#region JI_OrderNumber

		[ReadOnlyMember(nameof(HasValidOrder))]
		public override ZString JI_OrderNumber
		{
			get
			{
				if (HasValidOrder)
				{
					return OrderLine.Order.JD_OrderNumberAndSplit;
				}
				else
				{
					return base.JI_OrderNumber;
				}
			}
			set
			{
				if (!IsCopying && AutoAddOrderNumberOnSet && !value.IsEmpty && Declaration != null)
				{
					AddNewOrderItemsToDocsAndCartage(value);
				}
				base.JI_OrderNumber = value;
			}
		}

		internal void AddNewOrderItemsToDocsAndCartage(string orderNumber)
		{
			var lowerCaseOrderNumber = orderNumber.ToLower();
			if (Declaration is BaseJobDeclaration declaration && !string.IsNullOrEmpty(lowerCaseOrderNumber))
			{
				var found = declaration.DocsAndCartage.OrderItems.Cast<OrderItem>().Any(item => lowerCaseOrderNumber == item.JT_OrderReference.ToLower())
							|| declaration.AttachedOrders.Cast<IAttachedOrder>().Any(item => lowerCaseOrderNumber == item.JobNo.ToLower());

				if (!found && ShouldCopyOrderNumber)
				{
					var newItem = declaration.DocsAndCartage.OrderItems.AddNew();
					newItem.JT_OrderReference = orderNumber;
				}
			}
		}

		protected bool ShouldCopyOrderNumber
		{
			get
			{
				var declaration = Declaration;
				return declaration != null && declaration.IsPersistent;
			}
		}

		#endregion

		#endregion

		#region HasValidOrder
		public bool HasValidOrder
		{
			get { return JI_JO.IsValid && OrderLine != null && OrderLine.Order != null; }
		}
		#endregion

		public bool JI_CustomsUnitQty_ReadOnly
		{
			get { return !IsCopying && GetJI_CustomsUnitQtyInfoReadOnly(); }
		}

		protected virtual bool GetJI_CustomsUnitQtyInfoReadOnly()
		{
			return false;
		}

		public bool JI_CustomsQuantity_ReadOnly
		{
			get { return GetJI_CustomsQuantityReadOnly(); }
		}

		protected virtual bool GetJI_CustomsQuantityReadOnly()
		{
			return false;
		}

		[ResourceStringData("a82ce3da-6f9d-466d-a7ba-75562a49afe3", Caption = "Additional Qty 1")]
		public override ZDecimal JI_CustomsSecondQuantity
		{
			get { return base.JI_CustomsSecondQuantity; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.JI_CustomsSecondQuantity))
				{
					base.JI_CustomsSecondQuantity = value;
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CustomsUQList))]
		[MaxLength(4)]
		public override ZString JI_CustomsSecondUnitQty
		{
			get { return base.JI_CustomsSecondUnitQty; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.JI_CustomsSecondUnitQty))
				{
					ZString oldValue = JI_CustomsSecondUnitQty;
					base.JI_CustomsSecondUnitQty = value;
					if (!IsCopying && oldValue != JI_CustomsSecondUnitQty && ShouldConverCustomsQuantity2)
					{
						if (CanConvertFromNetWeightToCustomsUnit(JI_CustomsSecondUnitQty))
						{
							CustomsQuantity2Converter.CalculateFromNetWeightToCustomsQty();
						}
						else
						{
							CustomsQuantity2Converter.CalculateCustomsFactorAndQty();
						}
					}
				}
			}
		}

		[ResourceStringData("092575ed-01a3-4bfd-b1fa-effdd3bea8ad", Caption = "Additional Qty 2")]
		public override ZDecimal JI_CustomsThirdQuantity
		{
			get { return base.JI_CustomsThirdQuantity; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.JI_CustomsThirdQuantity))
				{
					base.JI_CustomsThirdQuantity = value;
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CustomsUQList))]
		[MaxLength(4)]
		public override ZString JI_CustomsThirdUnitQty
		{
			get { return base.JI_CustomsThirdUnitQty; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.JI_CustomsThirdUnitQty))
				{
					base.JI_CustomsThirdUnitQty = value;
				}
			}
		}

		[ResourceStringData("10F38E43-B30D-4D22-AA2D-3DB71D2113AF", Caption = "Additional Qty 3")]
		public override ZDecimal JI_CustomsFourthQuantity
		{
			get { return base.JI_CustomsFourthQuantity; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.JI_CustomsFourthQuantity))
				{
					base.JI_CustomsFourthQuantity = value;
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CustomsUQList))]
		[MaxLength(4)]
		public override ZString JI_CustomsFourthUnitQty
		{
			get { return base.JI_CustomsFourthUnitQty; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.JI_CustomsFourthUnitQty))
				{
					base.JI_CustomsFourthUnitQty = value;
				}
			}
		}

		[ResourceStringData("12ADD32A-D317-458C-B97A-E3074D15F3DE", Caption = "Additional Qty 4", FullDescription = "Additional Quantity 4")]
		public override ZDecimal JI_CustomsFifthQuantity
		{
			get { return base.JI_CustomsFifthQuantity; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.JI_CustomsFifthQuantity))
				{
					base.JI_CustomsFifthQuantity = value;
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CustomsUQList))]
		[MaxLength(4)]
		public override ZString JI_CustomsFifthUnitQty
		{
			get { return base.JI_CustomsFifthUnitQty; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.JI_CustomsFifthUnitQty))
				{
					base.JI_CustomsFifthUnitQty = value;
				}
			}
		}

		public virtual bool ShouldConverCustomsQuantity2
		{
			get { return true; }
		}

		#region JI_PartNo / JI_CC / JI_Tariff

		public bool AreClassificationDetailsBeingUpdated
		{
			get { return ClassificationDetailsUpdater.AreClassificationDetailsBeingUpdated; }
		}

		ClassificationDetailsUpdater ClassificationDetailsUpdater
		{
			get { return fClassificationDetailsUpdater ?? (fClassificationDetailsUpdater = GetNewClassificationDetailsUpdater()); }
		}
		ClassificationDetailsUpdater fClassificationDetailsUpdater;

		protected virtual ClassificationDetailsUpdater GetNewClassificationDetailsUpdater()
		{
			return new ClassificationDetailsUpdater(this);
		}

		protected virtual void OnExternalFactoryRefreshEnabledChanged()
		{
		}

		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.PartsList))]
		[ReadOnlyMember(nameof(JI_PartNo_ReadOnly))]
		public override ZString JI_PartNo
		{
			get { return base.JI_PartNo; }
			set
			{
				using (GetValidationSuspender())
				{
					bool reloadNeeded = (JI_PartNo != value);
					base.JI_PartNo = value;
					if (reloadNeeded && !IsCopying)
					{
						PartSyncManager.Refresh();
						if (JI_PartNo.IsEmpty)
						{
							JI_PartAttrib1 = ZString.Empty;
							JI_PartAttrib2 = ZString.Empty;
							JI_PartAttrib3 = ZString.Empty;
							JI_SerialNumber = ZString.Empty;
						}
					}
				}
				if (!IsCopying)
				{
					Validation.ValidateJI_PartNo();
				}
			}
		}

		public virtual bool ShouldSetDescriptionWhenPartNoChanges => PartClassificationTariffDescriptionSyncroniser.IsDefaultDescription;

		public virtual void SetInvoiceUQWhenPartNoChanged(ZString newUQ)
		{
			JI_InvoiceUQ = newUQ;
		}

		protected bool JI_PartNo_ReadOnly
		{
			get { return !JI_PartNo_CanBeSetByCustomer; }
		}

		public bool JI_PartNo_CanBeSetByCustomer => JI_PartNo_CanBeSetByCustomerCore;
		protected virtual bool JI_PartNo_CanBeSetByCustomerCore => JI_ParentID.IsEmpty;

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.PartAttrib1List))]
		[ReadOnlyMember(nameof(JI_PartAttrib1_ReadOnly))]
		public override ZString JI_PartAttrib1
		{
			get { return base.JI_PartAttrib1; }
			set
			{
				bool hasChanges = base.JI_PartAttrib1 != value;

				base.JI_PartAttrib1 = value;
				if (JI_PartNo_CanBeSetByCustomer)
				{
					if (hasChanges && !IsCopying)
					{
						PartSyncManager.Refresh();
					}
				}
			}
		}

		protected bool JI_PartAttrib1_ReadOnly
		{
			get { return !JI_PartNo_CanBeSetByCustomer; }
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.PartAttrib2List))]
		[ReadOnlyMember(nameof(JI_PartAttrib2_ReadOnly))]
		public override ZString JI_PartAttrib2
		{
			get { return base.JI_PartAttrib2; }
			set
			{
				bool hasChanges = base.JI_PartAttrib2 != value;

				base.JI_PartAttrib2 = value;
				if (JI_PartNo_CanBeSetByCustomer)
				{
					if (hasChanges && !IsCopying)
					{
						PartSyncManager.Refresh();
					}
				}
			}
		}

		protected bool JI_PartAttrib2_ReadOnly
		{
			get { return !JI_PartNo_CanBeSetByCustomer; }
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.PartAttrib3List))]
		[ReadOnlyMember(nameof(JI_PartAttrib3_ReadOnly))]
		public override ZString JI_PartAttrib3
		{
			get { return base.JI_PartAttrib3; }
			set
			{
				bool hasChanges = base.JI_PartAttrib3 != value;

				base.JI_PartAttrib3 = value;
				if (JI_PartNo_CanBeSetByCustomer)
				{
					if (hasChanges && !IsCopying)
					{
						PartSyncManager.Refresh();
					}
				}
			}
		}

		protected bool JI_PartAttrib3_ReadOnly
		{
			get { return !JI_PartNo_CanBeSetByCustomer; }
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.SerialList))]
		[ReadOnlyMember(nameof(JI_SerialNumber_ReadOnly))]
		public override ZString JI_SerialNumber
		{
			get => base.JI_SerialNumber;
			set
			{
				var hasChanges = base.JI_SerialNumber != value;

				base.JI_SerialNumber = value;
				if (JI_PartNo_CanBeSetByCustomer)
				{
					if (hasChanges && !IsCopying)
					{
						PartSyncManager.Refresh();
					}
				}
			}
		}

		protected bool JI_SerialNumber_ReadOnly => !JI_PartNo_CanBeSetByCustomer;

		protected virtual bool ShouldSetDescriptionWhenClassificationChanges => PartClassificationTariffDescriptionSyncroniser.IsDefaultDescription;

		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(JI_CC_ReadOnly))]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.ClassificationList))]
		public override ZGuid JI_CC
		{
			get { return base.JI_CC; }
			set
			{
				var hasChanges = base.JI_CC != value;
				var defaultDescriptionIfNeeded = ShouldSetDescriptionWhenClassificationChanges;
				base.JI_CC = value;
				if (hasChanges && !IsCopying)
				{
					ClassificationDetailsUpdater.UpdateWhenJI_CCIsSet();
					if (defaultDescriptionIfNeeded)
					{
						PartClassificationTariffDescriptionSyncroniser.SetDescription();
					}
				}
			}
		}

		protected bool JI_CC_ReadOnly
		{
			get { return !JI_PartNo_CanBeSetByCustomer; }
		}

		[BusinessObjectTestExclude]
		public override ZString JI_Tariff
		{
			get { return base.JI_Tariff; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.JI_Tariff))
				{
					if (IsCopying)
					{
						base.JI_Tariff = value;
					}
					else
					{
						ZString oldTariff = JI_Tariff;
						ZBool shouldSetDescription = ShouldSetDescriptionWhenTariffChanges;
						ZString newTariff = FormatTariffForSaving(value).Left(JI_TariffInfo.MaxLength);
						base.JI_Tariff = newTariff;

						if (!IsCopying && newTariff != oldTariff)
						{
							if (shouldSetDescription)
							{
								PartClassificationTariffDescriptionSyncroniser.SetDescription();
							}

							ClassificationDetailsUpdater.UpdateWhenJI_TariffIsSet();

							SetDefaultTaxOrFeeCode();
						}
					}
				}
			}
		}

		public virtual ZString FormatTariffForSaving(ZString unformattedTariff) => TariffFormatter.Format(unformattedTariff);

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.OrderNumbersList))]
		[ResourceStringData("42963048-C2D4-430B-9E67-251BCFB6E0B7", Caption = "Concession Order")]
		public override ZString JI_ConcessionOrder
		{
			get { return base.JI_ConcessionOrder; }
			set { base.JI_ConcessionOrder = value; }
		}

		protected virtual void SetDefaultTaxOrFeeCode()
		{
			if (UseUniversalTariff)
			{
				var defaultTaxOrFeeCode = DefaultTaxOrFeeCode;
				if (!defaultTaxOrFeeCode.IsEmpty)
				{
					JI_ZZF_NKTaxType = defaultTaxOrFeeCode;
				}
			}
		}

		public virtual TariffView UniversalTariff
		{
			get
			{
				TariffView universalTariff = null;
				if (UseUniversalTariff && !JI_Tariff.IsEmpty)
				{
					universalTariff = new TariffView.Loader(Factory).LoadMostRecentCachedTariff(GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff), UniversalTariffType, JI_Tariff, UniversalTariffValuationDate);
				}
				return universalTariff;
			}
		}

		#region Data Grouping
		public ZString GetDefaultDataGroupingCode(DefaultDataGroupingType dataGroupingType = DefaultDataGroupingType.None)
		{
			switch (dataGroupingType)
			{
				case DefaultDataGroupingType.Tariff:
					return DefaultDataGroupingForTariffsCore;
				case DefaultDataGroupingType.DutyRateCodes:
					return DefaultDataGroupingForDutyRateCodesCore;
				case DefaultDataGroupingType.CusProcedure:
					return DefaultDataGroupingForCusProcedure;
				case DefaultDataGroupingType.AdditionalDocumentCodes:
					return DefaultDataGroupingForAdditionalDocumentCodesCore;
				default:
					return DefaultDataGroupingCore;
			}
		}

		protected virtual ZString DefaultDataGroupingCore => InvoiceHeader?.GetDefaultDataGroupingCode() ?? CountryCode;
		protected virtual ZString DefaultDataGroupingForTariffsCore => InvoiceHeader?.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff) ?? CountryCode;
		protected virtual ZString DefaultDataGroupingForDutyRateCodesCore => InvoiceHeader?.GetDefaultDataGroupingCode(DefaultDataGroupingType.DutyRateCodes) ?? CountryCode;
		protected virtual ZString DefaultDataGroupingForCusProcedure => InvoiceHeader?.GetDefaultDataGroupingCode(DefaultDataGroupingType.CusProcedure) ?? CountryCode;
		protected virtual ZString DefaultDataGroupingForAdditionalDocumentCodesCore => InvoiceHeader?.GetDefaultDataGroupingCode(DefaultDataGroupingType.AdditionalDocumentCodes) ?? CountryCode;
		#endregion

		public bool UseUniversalTariff => UseUniversalTariffCore;
		protected internal virtual bool UseUniversalTariffCore => true;

		public virtual ZString UniversalTariffType => Universal.Constants.TariffTypes.HarmonizedSystem;

		protected virtual ZDateTime UniversalTariffValuationDate => EffectiveAssessmentDate;

		protected virtual ZString AdditionalCode => ZString.Empty;

		protected virtual ZString UniversalTariffDutyRateCode => ZString.Empty;

		public virtual RateView UniversalDutyRate => UniversalTariff?.GetApplicableRates(DutyRateSelectionCriteria).FirstOrDefault(x => UniversalTariffDutyRateCode.IsEmpty || x.RateCode == UniversalTariffDutyRateCode);

		public virtual IEnumerable<RateView> AllApplicableRates => UniversalTariff?.GetApplicableRates(AllApplicableRatesSelectionCriteria);

		public virtual ZDateTime EffectiveAssessmentDate => InvoiceHeader?.EffectiveValuationDate ?? ZDateTime.Today;

		ZString DefaultTaxOrFeeCode => UniversalTariff?.GetDefaultTaxOrFeeCode(EffectiveAssessmentDate, DefaultDataGroupingForTaxOrFee) ?? ZString.Empty;

		protected virtual ZString DefaultDataGroupingForTaxOrFee => GetDefaultDataGroupingCode();

		protected virtual bool ShouldSetDescriptionWhenTariffChanges
		{
			get
			{
				var registryValue = DataRegistry.Business.CustomsDataRegistry.Instance.EnableAutoTariffDescriptionPopulation.GetFallBackValueAtAllLevels(RegistryCompanyPK, RegistryBranchPK, Guid.Empty);
				return ((this.InvoiceHeader?.IsAttachedToPersistentDeclaration ?? false) ? registryValue.EnableCustomsDeclaration : registryValue.EnableCommercialInvoice) && PartClassificationTariffDescriptionSyncroniser.IsDefaultDescription;
			}
		}

		public virtual ZString JI_TariffForComplianceWise => JI_Tariff;

		[BusinessObjectTestExclude]
		[ResourceStringData("6BDA8D4D-A4CF-43AC-A6AB-774233BD7621", Caption = "Tariff")]
		public virtual ZString JI_FormattedTariff
		{
			get { return TariffFormatter.DisplayFormat(JI_Tariff); }
			set { JI_Tariff = value; }
		}

		public ZPropertyInfo JI_FormattedTariffInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JI_FormattedTariff, x => JI_TariffInfo); }
		}

		protected virtual TariffFormatter TariffFormatter
		{
			get { return new TariffFormatter(); }
		}

		ITariffFormatter ITariffFormatProvider.TariffFormatter => TariffFormatter;

		#region UpdateDetailsOnPartChange

		/// <summary>
		/// Called by the PartSyncManager when the Part is changed in another factory
		/// </summary>
		public void UpdateDetailsOnPartChange()
		{
			if (!IsCopying && !IsDeleted && !IsNull && CanCopyFromProduct)
			{
				var invoice = InvoiceHeader;
				var refreshOptions = invoice.Importer_Effective?.GetASNRefreshOptions(invoice.RegistryCompanyPK);
				if (invoice.IsASN && refreshOptions != null && refreshOptions.Any())
				{
					ASNRefereshData(refreshOptions);
				}
				else
				{
					using (OnUpdatingDetailsFromPart())
					{
						UpdateDetailsFromProductOnPartChangeCore();
						UpdateDetailsFromPivotOnPartChangeCore();
					}
				}
			}
		}

		protected virtual IDisposable OnUpdatingDetailsFromPart()
		{
			return null;
		}

		protected virtual bool CanCopyFromProduct => true;

		void ASNRefereshData(IEnumerable<ZString> refreshOptions)
		{
			var pivot = Pivot;
			if (pivot != null)
			{
				if (refreshOptions.Contains(DefaultOptions.Codes.Tariff))
				{
					JI_Tariff = pivot.CI_TariffNum;
				}
				if (refreshOptions.Contains(DefaultOptions.Codes.Classification))
				{
					JI_CC = pivot.CI_CC;
				}
				ASNRefereshDataCountrySpecific(refreshOptions, pivot);
			}
		}

		protected virtual void ASNRefereshDataCountrySpecific(IEnumerable<ZString> refreshOptions, BaseCusClassPartPivot pivot)
		{ }

		public virtual void UpdateDetailsFromProductOnPartChangeCore()
		{
			ClassificationDetailsUpdater.UpdateWhenJI_PartNoIsSet();
		}

		public virtual void UpdateDetailsFromPivotOnPartChangeCore()
		{
			JI_ClassUsageComment = Pivot?.CI_UsageComment ?? ZString.Empty;
		}

		#endregion

		void PreLoadLastPartDescriptionIfPartHasBeenUsed()
		{
			if (!JI_OP.IsEmpty)
			{
				var part = (OrgSupplierPart)Factory.Load(TypeOfPartUsed, JI_OP);
				PartClassificationTariffDescriptionSyncroniser.RefreshPartGeneratedLineDescription(GetPartDescription(part, Pivot), GetPartExtendedCommercialDescription(part));
			}
		}

		public ZString TariffDescription
		{
			get { return GetTariffDescription(JI_Tariff); }
		}

		protected virtual ZString GetTariffDescription(ZString tariffCode)
		{
			return ZString.Empty;
		}

		/// <summary>
		/// Used by the "invoice batch report", allows countries to pull duty or all duties on this invoiceLine and show them however they wish.  Default: returns just DutyAmount.
		/// </summary>
		public ZString DutyAmountsAsString
		{
			get { return DutyAmountsAsStringCore; }
		}

		protected virtual ZString DutyAmountsAsStringCore
		{
			get { return JI_Calc_DutyAmount.ToStringTrimZeros(2); }
		}

		public virtual ZDate EffectiveDateForDutyRate
		{
			get { return InvoiceHeader?.EffectiveDateForDutyRate ?? ZDate.Today; }
		}

		#region Volume

		public
#if DEBUG
 virtual    // Mocked in CusEntryLine
#endif
		ZVolume EffectiveVolume
		{
			get
			{
				ZVolume result;
				if (JI_Volume != 0 || InvoiceHeader == null)
				{
					result = new ZVolume(JI_Volume, JI_VolumeUQ);
				}
				else
				{
					result = InvoiceHeader.LineVolumeCalculator.GetVolume(this);
				}
				return result;
			}
		}

		#endregion

		#region Weight
		public virtual ZWeight EffectiveGrossWeight
		{
			get
			{
				ZWeight result;
				if (JI_Weight != 0 || InvoiceHeader == null)
				{
					result = new ZWeight(JI_Weight, JI_WeightUQ);
				}
				else
				{
					result = InvoiceHeader.LineWeightCalculator.GetWeight(this);
				}
				return result;
			}
		}

		public ZDecimal NetWeightInKG
		{
			get { return new ZWeight(JI_NetWeight, JI_NetWeightUQ).InKilogramsSafe; }
		}

		public ZDecimal GrossWeightInKG
		{
			get { return new ZWeight(JI_Weight, JI_WeightUQ).InKilogramsSafe; }
		}

		public ZDecimal CustomsFirstQuantityInKG => CustomsWeight.InKilogramsSafe;

		public ZDecimal CustomsSecondQuantityInKG => CustomsSecondQuantity.InKilogramsSafe;

		#endregion

		#region Calculate Weight/Volume

		public delegate ZString GetConvertedStockUnitDelegate(ZString unit);

		public GetConvertedStockUnitDelegate GetConvertedStockUnit;

		public delegate ZString GetConvertedInvoiceUnitDelegate(ZString unit);

		public GetConvertedInvoiceUnitDelegate GetConvertedInvoiceUnit;

		public ZString PartStockTakeUnit
		{
			get
			{
				if (partStockTakeUnit == null)
				{
					partStockTakeUnit = new CachedProperty<ZString>(Factory, delegate
					{
						return Part == null ? ZString.Empty : (GetConvertedStockUnit == null ? Part.OP_StockKeepingUnit : GetConvertedStockUnit(Part.OP_StockKeepingUnit));
					});
				}
				return partStockTakeUnit.Value;
			}
		}
		CachedProperty<ZString> partStockTakeUnit;

		public ZString ConvertedInvoiceUnit
		{
			get
			{
				if (convertedInvoiceUnit == null)
				{
					convertedInvoiceUnit = new CachedProperty<ZString>(Factory, delegate
					{
						return GetConvertedInvoiceUnit == null ? JI_InvoiceUQ : GetConvertedInvoiceUnit(JI_InvoiceUQ);
					});
				}
				return convertedInvoiceUnit.Value;
			}
		}
		CachedProperty<ZString> convertedInvoiceUnit;

		internal bool IsVolumeCalculableFromInvoiceQty
		{
			get { return !JI_InvoiceUQ.IsEmpty && Core.Constants.Volume.ContainsCode(JI_InvoiceUQ); }
		}

		internal bool IsVolumeCalculableFromProduct
		{
			get { return InvoiceAndPartUnitsConvertibleSensibly(Part.OP_Cubic, Core.Constants.Volume.Codes); }
		}

		void CalculateVolume()
		{
			if (!IsCopying)
			{
				if (Part != null && IsVolumeCalculableFromProduct)
				{
					ZDecimal invoiceQtyInStockKeepingUnit = UnitConverter.Convert(JI_InvoiceQuantity, ConvertedInvoiceUnit, Part.OP_StockKeepingUnit);

					if (invoiceQtyInStockKeepingUnit != 0m)
					{
						JI_Volume = invoiceQtyInStockKeepingUnit * Part.OP_Cubic;
						JI_VolumeUQ = Part.OP_CubicUQ;
					}
				}
				else if (IsVolumeCalculableFromInvoiceQty)
				{
					JI_Volume = JI_InvoiceQuantity;
					JI_VolumeUQ = JI_InvoiceUQ.Left(BaseJobComInvoiceLine.Schema.JI_VolumeUQMaxLength);//part of the if condition
				}
			}
		}

		internal bool IsGrossWeightCalculableFromInvoiceQty
		{
			get { return !JI_InvoiceUQ.IsEmpty && Core.Constants.Weight.ContainsCode(JI_InvoiceUQ); }
		}

		internal bool IsGrossWeightCalculableFromProduct
		{
			get { return InvoiceAndPartUnitsConvertibleSensibly(Part.OP_Weight, Core.Constants.Weight.Codes); }
		}

		bool InvoiceAndPartUnitsConvertibleSensibly(ZDecimal quantityInPart, IEnumerable<string> listOfUnits)
		{
			return Part != null &&
				quantityInPart > 0m &&
				!listOfUnits.Contains(Part.OP_StockKeepingUnit.ToString()) && // KG times KG etc is meaningless
				UnitConverter.Convertible(ConvertedInvoiceUnit, Part.OP_StockKeepingUnit);
		}

		internal bool IsNetWeightCalculableFromProduct
		{
			get { return InvoiceAndPartUnitsConvertibleSensibly(Part.OP_NetWeight, Core.Constants.Weight.Codes); }
		}

		void CalculateWeight()
		{
			if (!IsCopying)
			{
				bool hasCalculatedGrossWeight = false;
				var part = Part;

				if (part != null)
				{
					ZDecimal? invoiceQtyInStockKeepingUnit = null;
					if (Constants.Weight.ContainsCode(part.OP_WeightUQ) && (part.OP_Weight > 0 || part.OP_NetWeight > 0))
					{
						invoiceQtyInStockKeepingUnit = UnitConverter.Convert(JI_InvoiceQuantity, ConvertedInvoiceUnit, part.OP_StockKeepingUnit);
					}
					if (invoiceQtyInStockKeepingUnit.HasValue && invoiceQtyInStockKeepingUnit.Value > 0 && IsGrossWeightCalculableFromProduct)
					{
						JI_Weight = invoiceQtyInStockKeepingUnit.Value * part.OP_Weight;
						JI_WeightUQ = part.OP_WeightUQ;
						hasCalculatedGrossWeight = true;
					}
					if (invoiceQtyInStockKeepingUnit.HasValue && invoiceQtyInStockKeepingUnit.Value > 0 && IsNetWeightCalculableFromProduct)
					{
						var netWeight = new ZWeight(invoiceQtyInStockKeepingUnit.Value * part.OP_NetWeight, part.OP_WeightUQ);
						if (netWeight.IsValid && Lookups.WeightUQList.ContainsCode(JI_WeightUQ))
						{
							var weight = netWeight.ConvertTo(JI_WeightUQ);
							if (!weight.IsEmpty)
							{
								JI_NetWeight = weight;
								JI_NetWeightUQ = JI_WeightUQ;
							}
						}
					}
				}
				if (!hasCalculatedGrossWeight && IsGrossWeightCalculableFromInvoiceQty)
				{
					if (CustomsDataRegistry.Instance.DefaultWeightFromInvoiceQty.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty))
					{
						JI_Weight = JI_InvoiceQuantity;
						JI_WeightUQ = JI_InvoiceUQ.Left(BaseJobComInvoiceLine.Schema.JI_WeightUQMaxLength); // Part of if statement
					}
				}
			}
		}

		internal void CalculateWeightAndVolume()
		{
			CalculateWeight();
			CalculateVolume();
		}

		#endregion

		#region NeedsCustomsQuantity
		public virtual bool NeedsCustomsQuantity
		{
			get { return CustomsUQ != ZString.Empty; }
		}
		#endregion

		#region CustomsUQ
		public virtual ZString CustomsUQ
		{
			get { return ZString.Empty; }
		}
		#endregion

		#region Calculate Customs Quantity

		internal protected void CalculateCustomsFactorAndQty()
		{
			if (ShouldReCalculateCustomsQtyOnLineQuantityChange)
			{
				CalculateCustomsFactorAndQtyCore();
			}
		}

		protected virtual void CalculateCustomsFactorAndQtyCore()
		{
			CustomsQuantityConverter.CalculateCustomsFactorAndQty();

			if (CustomsQuantity2Converter != null)
			{
				CustomsQuantity2Converter.CalculateCustomsFactorAndQty();
			}
		}

		#endregion

		#endregion

		#region InvoiceQuantity / Customs Quantity

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CustomsUQList))]
		[MaxLength(3)]
		public override ZString JI_CustomsUnitQty
		{
			get { return base.JI_CustomsUnitQty; }
			set
			{
				if (JI_CustomsUnitQty != value && !SetterSuspender.IsSetterSuspended(Schema.JI_CustomsUnitQty))
				{
					base.JI_CustomsUnitQty = value;
					if (!IsCopying)
					{
						if (ClearCustomsQuantityWhenUQSet)
						{
							JI_CustomsQuantity = 0m;
						}

						if (!JI_CustomsQuantity_ReadOnly)
						{
							if (CanConvertFromNetWeightToCustomsUnit(JI_CustomsUnitQty))
							{
								CustomsQuantityConverter.CalculateFromNetWeightToCustomsQty();
							}
							else
							{
								CustomsQuantityConverter.CalculateCustomsFactorAndQty();
							}
						}

						if (Declaration != null)
						{
							Declaration.MarkAsNeedingValidation();
						}
					}
				}
			}
		}

		protected virtual bool ClearCustomsQuantityWhenUQSet
		{
			get { return JI_CustomsQuantity_ReadOnly; }
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.InvoiceUQList))]
		public override ZString JI_InvoiceUQ
		{
			get { return base.JI_InvoiceUQ; }
			set
			{
				value = value.Trim();

				if (JI_InvoiceUQ != value && !SetterSuspender.IsSetterSuspended(nameof(JI_InvoiceUQ)))
				{
					base.JI_InvoiceUQ = value;
					if (!IsCopying)
					{
						CalculateCustomsFactorAndQty();
						CalculateWeightAndVolume();
						CalculateLinePriceFromInvoiceQuantityChanges();

						var invoice = InvoiceHeader;
						if (invoice != null)
						{
							invoice.ResetHasMultipleInvoiceUQs();
						}
					}
				}
			}
		}

		[DecimalPlaces(6)]
		public override ZDecimal JI_CustomsQuantity
		{
			get { return base.JI_CustomsQuantity; }
			set
			{
				if (!isSettingCustomsQuantity && !SetterSuspender.IsSetterSuspended(Schema.JI_CustomsQuantity))
				{
					isSettingCustomsQuantity = true;
					try
					{
						base.JI_CustomsQuantity = value;
					}
					finally
					{
						isSettingCustomsQuantity = false;
					}
					if (!IsCopying && Declaration != null)
					{
						Declaration.MarkAsNeedingValidation();
					}
				}
			}
		}
		protected bool isSettingCustomsQuantity;

		#region ILinePriceCalculationFieldSettingSupporter

		void ILinePriceCalculationFieldSettingSupporter.Start(object type)
		{
			if (type != null)
			{
				if (!FieldSettingTypes.ContainsKey(type))
				{
					FieldSettingTypes.Add(type, 0);
				}
				FieldSettingTypes[type]++;
			}
		}

		void ILinePriceCalculationFieldSettingSupporter.Stop(object type)
		{
			if (type != null)
			{
				if (FieldSettingTypes.ContainsKey(type))
				{
					FieldSettingTypes[type]--;
				}
			}
		}

		protected bool IsFieldSettingInProgress(object type)
		{
			return FieldSettingTypes.TryGetValue(type, out var index) && index > 0;
		}

		Dictionary<object, int> FieldSettingTypes
		{
			get { return fieldSettingTypes ?? (fieldSettingTypes = new Dictionary<object, int>()); }
		}
		Dictionary<object, int> fieldSettingTypes;

		public IDisposable GetNewLinePriceCalculationFieldSettingSupporter(object type)
		{
			return new LinePriceCalculationFieldSettingSupporter(this, type);
		}

		#endregion

		#region Is Setting Invoice Quantity?

		public bool IsSettingInvoiceQuantity
		{
			get { return IsFieldSettingInProgress(LinePriceCalculationFieldSettingType.Quantity); }
		}

		#endregion

		[DecimalPlaces(6)]
		public override ZDecimal JI_InvoiceQuantity
		{
			get { return base.JI_InvoiceQuantity; }
			set
			{
				value = ZArchitecture.Core.Utilities.Round(value, 6);

				if (forceUpdateInvoiceQuantityWithoutDefaulting)
				{
					base.JI_InvoiceQuantity = value;
				}
				else if (IsSettingInvoiceQuantity)
				{
					ErrorReporter.ReportOnce("BaseJobComInvoiceLine.JI_InvoiceQuantity_Set",
						"Called JI_InvoiceQuantity from within itself" + System.Environment.NewLine +
						"Original Value = " + JI_InvoiceQuantity.ToString(5) + " with PK = " + PK + System.Environment.NewLine +
						"New Value = " + value.ToString(5));
				}
				else
				{
					using (GetNewLinePriceCalculationFieldSettingSupporter(LinePriceCalculationFieldSettingType.Quantity))
					{
						var oldValue = JI_InvoiceQuantity;
						base.JI_InvoiceQuantity = value;
						if (!IsCopying && oldValue != JI_InvoiceQuantity)
						{
							CalculateWeightAndVolume();
							CalculateCustomsFactorAndQty();
							CalculateLinePriceFromInvoiceQuantityChanges();
							CalculateBondedWhsQuantityFromInvoiceQuantity();
							CalculateAllocatedQuantityFromInvoiceQuantity();
							MarkApportionmentDirty(HasParentChargeDistributedByThisToMarkApportionmentDirty(ChargeDistributeByList.Codes.Quantity));
						}
					}
				}
			}
		}

		bool forceUpdateInvoiceQuantityWithoutDefaulting;

		public IDisposable ForceUpdateInvoiceQuantityWithoutDefaulting()
		{
			return new DisposableAction(() => forceUpdateInvoiceQuantityWithoutDefaulting = true, () => forceUpdateInvoiceQuantityWithoutDefaulting = false);
		}

		void CalculateBondedWhsQuantityFromInvoiceQuantity()
		{
			if (SupportInvoiceLineComponents && (Declaration?.IsBondedWhsQuantityRequiredForBondedWarehouse ?? false))
			{
				if (!JI_InvoiceUQ.IsEmpty && !JI_BondedWhsUnitQty.IsEmpty && JI_InvoiceQuantity > 0)
				{
					var invoiceQtyInBondedWhsUnit = UnitConverter.Convert(JI_InvoiceQuantity, JI_InvoiceUQ, JI_BondedWhsUnitQty);
					if (invoiceQtyInBondedWhsUnit > 0)
					{
						JI_BondedWhsQuantity = invoiceQtyInBondedWhsUnit;
					}
				}
			}
		}

		void CalculateAllocatedQuantityFromInvoiceQuantity()
		{
			if (SupportInvoiceLineComponents && (Declaration?.IsAllocatedQuantityRequiredForBondedWarehouse ?? false))
			{
				if (ComponentInventoryCollection.Count == 1 && ComponentInventoryCollection[0] is JobComInvLineComponentInventory firstComponentInventory && firstComponentInventory.Inventory != null)
				{
					if (!JI_InvoiceUQ.IsEmpty && !firstComponentInventory.PackType.IsEmpty && JI_InvoiceQuantity > 0)
					{
						var invoiceQtyInAllocatedPackType = UnitConverter.Convert(JI_InvoiceQuantity, JI_InvoiceUQ, firstComponentInventory.PackType);
						if (invoiceQtyInAllocatedPackType > 0)
						{
							firstComponentInventory.JIV_QuantityToDraw = invoiceQtyInAllocatedPackType;
						}
					}
				}
			}
		}

		protected virtual ZBool ShouldReCalculateCustomsQtyOnLineQuantityChange => ZBool.True;

		protected virtual void CalculateLinePriceFromInvoiceQuantityChanges()
		{
			if (!IsSettingLinePrice)
			{
				var declaration = Declaration;
				if (declaration != null && Part != null && JI_LinePrice.IsEmpty && (ConsignorDefaultsPriceFromPartLastCost || declaration.IsDrawback))
				{
					JI_LinePrice = Part.OP_LastCost * UnitConverter.Convert(JI_InvoiceQuantity, ConvertedInvoiceUnit, Part.OP_StockKeepingUnit);
				}
				else
				{
					UpdateUnitPriceIfChangedFromLinePriceChanges();
				}
			}
		}

		bool ConsignorDefaultsPriceFromPartLastCost
		{
			get
			{
				bool defaultUnitPrice = false;

				foreach (OrgPartRelation relation in Part.RelatedOrganisations)
				{
					if (relation.Organisation.OH_IsConsignor && relation.Organisation.MiscServ.OM_EXDefaultInvoicePriceFromProductLastCost == "YES")
					{
						defaultUnitPrice = true;
						break;
					}
				}

				return defaultUnitPrice;
			}
		}

		#endregion

		#region Collection

		AllChargesCollection Common.ICommonInvoice.AllCharges
		{
			get
			{
				if (fAllCharges == null)
				{
					fAllCharges = new AllChargesCollection(this);
					fAllCharges.Load();
				}
				return fAllCharges;
			}
		}
		AllChargesCollection fAllCharges;

		IApportionInvoiceHolder Common.ICommonInvoice.InvoicesHolder
		{
			get { return Declaration; }
		}

		[ChildEditable(true)]
		public IJobComInvChargeCollection<BaseInvoiceLineCharge> Charges
		{
			get
			{
				if (fCharges == null)
				{
					fCharges = CreateNewInvoiceLineChargeCollection();
					RegisterEditableChildObject(fCharges);
				}
				return fCharges;
			}
		}

		[ChildEditable(true)]
		public IJobComInvApportionedChargeCollection<BaseInvoiceLineApportionedCharge> ApportionedCharges
		{
			get
			{
				if (fApportionedCharges == null)
				{
					fApportionedCharges = CreateNewInvoiceLineApportionedChargeCollection();
					fApportionedCharges.SetReadOnlyIncludingChildren(true);
					RegisterEditableChildObject(fApportionedCharges);
				}
				return fApportionedCharges;
			}
		}

		[ChildEditable(true)]
		public ICusVehicleCollection<CusVehicle, BaseJobComInvoiceLine> Vehicles
		{
			get
			{
				if (vehicles == null)
				{
					vehicles = GetNewCusVehicleCollection();
					vehicles.Load();
					RegisterEditableChildObject(vehicles);
				}
				return vehicles;
			}
		}
		ICusVehicleCollection<CusVehicle, BaseJobComInvoiceLine> vehicles;

		protected virtual ICusVehicleCollection<CusVehicle, BaseJobComInvoiceLine> GetNewCusVehicleCollection() => new CusVehicleCollection<CusVehicle, BaseJobComInvoiceLine>(this);

		public CusVehicle FirstVehicle => VehicleRelationship == VehicleRelationshipType.None ? null :
			Vehicles.Count > 0 ? Vehicles.Cast<CusVehicle>().OrderBy(vehicle => vehicle.CVH_SystemCreateTimeUtc).First() : Vehicles.AddNew();

		#region Components & Inventories

		[ChildEditable(true)]
		public JobComInvLineComponentInventoryCollection ComponentInventoryCollection
		{
			get
			{
				if (fComponentInventoryCollection == null)
				{
					fComponentInventoryCollection = GetNewComponentInventoryCollectionCore();
					fComponentInventoryCollection.Load();
					RegisterEditableChildObject(fComponentInventoryCollection);
				}
				return fComponentInventoryCollection;
			}
		}
		JobComInvLineComponentInventoryCollection fComponentInventoryCollection;

		protected virtual JobComInvLineComponentInventoryCollection GetNewComponentInventoryCollectionCore()
		{
			return new JobComInvLineComponentInventoryCollection(this);
		}

		public virtual OrgAddress WarehouseForComponentInventories => EntryInstruction?.Warehouse;

		#endregion

		[ChildEditable(true)]
		public CusContainersInvoiceLinesCollection ContainersPivot
		{
			get
			{
				if (fContainersPivot == null)
				{
					fContainersPivot = GetNewContainersPivotCore();
					fContainersPivot.Load();
					fContainersPivot.IsManagedForDataRefresh = true;
					RegisterEditableChildObject(fContainersPivot);
				}
				return fContainersPivot;
			}
		}
		CusContainersInvoiceLinesCollection fContainersPivot;

		protected virtual CusContainersInvoiceLinesCollection GetNewContainersPivotCore()
		{
			return new CusContainersInvoiceLinesCollection(this);
		}

		internal bool ContainersPivotIsLoaded
		{
			get { return fContainersPivot != null; }
		}

		//DO NOT CACHE - Memory Held up by local cache
		//For binding Only - Either use ContainersPivot or declaration.CusContainers. This is refactored in Base & AU. Please refactor in other countries as well.
		public NonPersistentCusContainerCollection ContainersForInvoiceLinesForBindingOnly
		{
			get { return ContainersForInvoiceLinesCore(); }
		}

		protected virtual NonPersistentCusContainerCollection ContainersForInvoiceLinesCore()
		{
			return new NonPersistentCusContainerCollection(this);
		}

		internal CusContainerInvoiceLinePivot ToggleLinkageWithContainer(BaseCusContainer container, bool value)
		{
			CusContainerInvoiceLinePivot pivot = null;
			if (container != null)
			{
				if (value)
				{
					pivot = ContainersPivot.AddPivotFor(container);
					if (container.IsInvoiceLinePivotCollectionLoaded && !container.InvoiceLinePivotCollection.Contains(pivot))
					{
						container.InvoiceLinePivotCollection.Add(pivot);
					}
				}
				else
				{
					ContainersPivot.DeletePivotFor(container);
				}
			}
			return pivot;
		}

		#region ICusLinkPackageSupporter

		PivotLevel ICusLinkPackageSupporter.PivotLevel => PivotLevel.InvoiceLine;

		ZBool ICusLinkPackageSupporter.IsSupportPivot => SupportsChcPivotBetweenInvoiceLineAndPacking;

		BaseJobDeclaration ICusLinkPackageSupporter.Declaration => Declaration;

		IBasePackagePivotCollection ICusLinkPackageSupporter.CusPackPivots => PackagesPivot;

		BaseCusLinkPackageValidation ICusLinkPackageSupporter.GetNewLinkPackValidation(BaseCusLinkPackage linkPackage)
		{
			return GetNewLinkPackValidationCore(linkPackage);
		}

		protected virtual InvoiceLinePackageValidation GetNewLinkPackValidationCore(BaseCusLinkPackage linkPackage)
		{
			return new InvoiceLinePackageValidation(linkPackage, this);
		}

		public EventHandler PackagePivotsChanged;

		public ICusPackagePivot ToggleLinkageWithPackage(BasePackage package, bool value)
		{
			var result = this.ToggleLinkageWithPackageCore(package, value);

			if (package != null)
			{
				SynchronizeContainersPivotWithPackagesPivot(package, string.Empty);
			}

			PackagePivotsChanged?.Invoke(this, EventArgs.Empty);
			return result;
		}

		ZBool ICusLinkPackageSupporter.IsSupportEmptyPackType(BasePackage package) => IsSupportEmptyPackType(package);

		protected virtual ZBool IsSupportEmptyPackType(BasePackage package) => false;

		HashSet<ZString> ICusLinkPackageSupporter.DistinctPackageTypes
		{
			get
			{
				if (distinctPackageTypes == null)
				{
					distinctPackageTypes = new CachedProperty<HashSet<ZString>>(Factory, GetDistinctPackageTypesCore);
				}
				return distinctPackageTypes.Value;
			}
		}
		CachedProperty<HashSet<ZString>> distinctPackageTypes;

		protected virtual HashSet<ZString> GetDistinctPackageTypesCore()
		{
			var result = new HashSet<ZString>();
			foreach (InvoiceLinePackagePivot pivot in PackagesPivot)
			{
				if (pivot.Package is BasePackage package)
				{
					if (package.CW_MarksAndNos.IsEmpty)
					{
						result.Add(package.CW_PackType);
					}
					else
					{
						result.Add(package.PK.ToString());
					}
				}
				else
				{
					result.Add(ZString.Empty);
				}
			}
			return result;
		}

		#endregion

		#region PACKAGES

		[ChildEditable(true)]
		[ChildEditableTestExclude]
		public InvoiceLinePackagePivotCollection PackagesPivot
		{
			get
			{
				if (fPackagesPivot == null)
				{
					fPackagesPivot = GetNewPackagesPivotCore();
					if (SupportsChcPivotBetweenInvoiceLineAndPacking)
					{
						fPackagesPivot.Load();
						fPackagesPivot.IsManagedForDataRefresh = true;
						RegisterEditableChildObject(fPackagesPivot);
					}
					else
					{
						((ILegacyBusinessObjectCollectionInternals)fPackagesPivot).SetOverriddenAdditionalFilter(ZQuery.NoResultQuery);
						fPackagesPivot.SetCountedReadOnlyIncludingChildren(true);
					}
				}
				return fPackagesPivot;
			}
		}
		InvoiceLinePackagePivotCollection fPackagesPivot;

		protected virtual InvoiceLinePackagePivotCollection GetNewPackagesPivotCore()
		{
			return new InvoiceLinePackagePivotCollection(this);
		}

		internal void RefreshPackagesPivotMergeKeyIfNeeded()
		{
			fPackagesPivot?.RefreshMergeKey();
		}

		internal bool PackagesPivotIsLoaded
		{
			get { return fPackagesPivot != null; }
		}

		//DO NOT CACHE - Memory Held up by local cache
		//For binding Only - Either use PackagesPivot or declaration.CusPackages. This is refactored in Base & AU. Please refactor in other countries as well.
		public BaseCusLinkPackageCollection PackagesForInvoiceLinesForBindingOnly
		{
			get { return PackagesForInvoiceLinesCore(); }
		}

		protected virtual BaseCusLinkPackageCollection PackagesForInvoiceLinesCore()
		{
			return new BaseCusLinkPackageCollection(this);
		}

		#endregion

		public void SynchronizeContainersPivotWithPackagesPivot(BasePackage package, string oldContainerNo)
		{
			var container = package?.PackingGroup?.Container;

			if (container != null && Declaration != null)
			{
				var containerPivot = ContainersPivot.GetRelatedPivot(container);
				var packPivot = PackagesPivot.GetRelatedPivot(package) ?? GetInvoicePackPivotsFromContainer(container).FirstOrDefault();

				if (packPivot != null && containerPivot == null)
				{
					ToggleLinkageWithContainer(container, true);
				}
				else if (packPivot == null && containerPivot != null)
				{
					UnLinkedWithContainer(container);
				}

				if (!string.IsNullOrEmpty(oldContainerNo))
				{
					var oldContainer = Declaration.CusContainers.Find(oldContainerNo);
					if (oldContainer != null && oldContainer.PK != container.PK)
					{
						var oldContainerPivot = ContainersPivot.GetRelatedPivot(oldContainer);
						if (oldContainerPivot != null)
						{
							UnLinkedWithContainer(oldContainer);
						}
					}
				}
			}
		}

		public IEnumerable<ICusPackagePivot> GetInvoicePackPivotsFromContainer(BaseCusContainer container)
		{
			var invoicePackagesPivot = InvoiceHeader?.PackagesPivot;

			if (invoicePackagesPivot != null)
			{
				foreach (var cusPackage in container.Packages)
				{
					var pivot = invoicePackagesPivot.GetRelatedPivot(cusPackage);

					if (pivot != null)
					{
						yield return pivot;
					}
				}
			}
		}

		void UnLinkedWithContainer(BaseCusContainer container)
		{
			var hasOtherPackageLinked = PackagesPivot.Cast<InvoiceLinePackagePivot>().Any(p => p.Package?.PackingGroup?.CR_CO_Container == container.PK);
			if (!hasOtherPackageLinked)
			{
				ToggleLinkageWithContainer(container, false);
			}
		}

		#endregion

		[ChildEditable(true)]
		public AdditionalLineLinkInvoiceLineCollection AdditionalEntryLineLinks
		{
			get
			{
				if (fAdditionalEntryLineLinks == null)
				{
					fAdditionalEntryLineLinks = new AdditionalLineLinkInvoiceLineCollection(this);
					fAdditionalEntryLineLinks.Load();
					RegisterEditableChildObject(fAdditionalEntryLineLinks);
				}
				return fAdditionalEntryLineLinks;
			}
		}
		AdditionalLineLinkInvoiceLineCollection fAdditionalEntryLineLinks;

		#region InvoiceLineRefs

		[ChildEditable(true)]
		[ChildEditableTestExclude]
		public JobComInvLineRefsCollection InvoiceLineRefs
		{
			get
			{
				if (invoiceLineRefs == null)
				{
					invoiceLineRefs = GetNewInvoiceLineRefs();
					if (SupportInvoiceLineRefs)
					{
						invoiceLineRefs.Load();
						RegisterEditableChildObject(invoiceLineRefs);
					}
					else
					{
						((ILegacyBusinessObjectCollectionInternals)invoiceLineRefs).SetOverriddenAdditionalFilter(ZQuery.NoResultQuery);
						invoiceLineRefs.SetCountedReadOnlyIncludingChildren(true);
					}
				}
				return invoiceLineRefs;
			}
		}
		JobComInvLineRefsCollection invoiceLineRefs;

		protected virtual JobComInvLineRefsCollection GetNewInvoiceLineRefs()
		{
			return new JobComInvLineRefsCollection(this);
		}

		protected internal virtual bool SupportInvoiceLineRefs => Declaration?.SupportInvoiceLineRefs ?? false;

		#endregion

		#region Other Objects

		BaseCustomsQuantityConverter fCustomsQuantityConverter;
		protected internal BaseCustomsQuantityConverter CustomsQuantityConverter
		{
			get
			{
				if (fCustomsQuantityConverter == null)
				{
					fCustomsQuantityConverter = GetCustomsQuantityConverter();
				}
				return fCustomsQuantityConverter;
			}
#if DEBUG
			set
			{
				fCustomsQuantityConverter = value;
			}
#endif
		}

		protected virtual BaseCustomsQuantityConverter GetCustomsQuantityConverter()
		{
			return new BaseCustomsQuantityConverter(this, (ZPropertyInfoDecimal)JI_CustomsQuantityInfo, (ZPropertyInfoString)JI_CustomsUnitQtyInfo);
		}

		BaseCustomsQuantityConverter fCustomsQuantity2Converter;
		protected BaseCustomsQuantityConverter CustomsQuantity2Converter
		{
			get
			{
				if (fCustomsQuantity2Converter == null)
				{
					fCustomsQuantity2Converter = GetCustomsQuantity2Converter();
				}
				return fCustomsQuantity2Converter;
			}
		}

		/// <summary>
		/// If you implement ISecondCustomsQuantity, you dont have to override this method to calculate 2nd customs quantity.
		/// If your conversion needs a special consideration, the do override this method to return your converter.
		/// </summary>
		/// <returns></returns>
		protected virtual BaseCustomsQuantityConverter GetCustomsQuantity2Converter()
		{
			BaseCustomsQuantityConverter result;

			if (this is ISecondCustomsQuantity secondCustomsQtySupporter)
			{
				result = new BaseCustomsQuantityConverter(this, secondCustomsQtySupporter.SecondCustomsQtyInfo, secondCustomsQtySupporter.SecondCustomsUQInfo);
			}
			else
			{
				result = new BaseCustomsQuantityConverter(this, JI_CustomsSecondQuantityInfo, JI_CustomsSecondUnitQtyInfo);
			}
			return result;
		}

		#endregion

		#region Merged Line Number

		public ZPropertyInfo MergedLineNumberInfo
		{
			get { return GetZPropertyInfo(Schema.MergedLineNumber); }
		}

		public ZString MergedLineNumber
		{
			get { return MergedLineNumberInternal; }
		}

		protected virtual ZString MergedLineNumberInternal
		{
			get
			{
				var entryLine = CusEntryLine;
				ZString result;
				if (entryLine != null)
				{
					result = entryLine.CL_LineNumber.ToString();
					var entryNumber = CompleteEntryNumber;
					if (!entryNumber.IsEmpty)
					{
						result += "/" + entryNumber;
					}
				}
				else
				{
					result = Res.GetString("3757116a-71dd-45d6-9030-3968874dec2f", "Not Merged");
				}

				return result;
			}
		}

		/// <summary>
		/// US has an entry filer code prepended to make the entry number complete
		/// </summary>
		public ZString CompleteEntryNumber
		{
			get { return CompleteEntryNumberCore; }
		}

		protected virtual ZString CompleteEntryNumberCore
		{
			get { return CusEntryLine != null && CusEntryLine.Header != null ? CusEntryLine.Header.EntryNumber : ZString.Empty; }
		}

		public virtual ZString JI_Calc_MergedLineNumber
		{
			get { return CusEntryLine == null ? notMerged : (ZString)CusEntryLine.CL_LineNumber.ToString(); }
		}
		protected ZString notMerged = Res.GetString("026e5c49-7dac-495d-b082-74ee292ff2fd", "Not Merged");

		public ZPropertyInfo JI_Calc_MergedLineNumberInfo
		{
			get { return GetZPropertyInfo(Schema.JI_Calc_MergedLineNumber); }
		}

		public virtual ZPropertyInfo ExposedCusEntryLineErrorPropertyInfo
		{
			get { return MergedLineNumberInfo; }
		}

		#endregion

		#region Implementation

		internal void CopyCustomFieldsFromProduct(OrgSupplierPart product)
		{
			var productCustomBizo = product.GetCustomBusinessObject();

			if (productCustomBizo != null)
			{
				var lineCustomBizo = this.GetCustomBusinessObject(true);
				var matchedCustomFields = lineCustomBizo.GetOrderedCustomProperties().Intersect(productCustomBizo.GetOrderedCustomProperties()).Where(x => !x.EndsWith((NoResString)"Info"));

				foreach (var field in matchedCustomFields)
				{
					if (lineCustomBizo[field] is IZType fieldValue && (fieldValue is ZBool || fieldValue.IsEmpty))
					{
						lineCustomBizo[field] = productCustomBizo[field];
					}
				}
			}
		}

		internal void CopyCommodityFromProduct(OrgSupplierPart product)
		{
			if (product != null)
			{
				JI_RH_NKCommodity_Code = product.OP_RH_NKCommodityCode;
			}
		}

		internal void CopyUNDGsIfSupported(UNDGDataItemCollection undgsToCopy)
		{
			if (IsUNDGSupported && undgsToCopy.Count == 1)
			{
				var undg = UNDGs.Count > 0 ? UNDGs[0] : null;
				if (undg == null)
				{
					undg = UNDGs.AddNew();
				}
				else
				{
					foreach (var undgToDelete in UNDGs.ToArray())
					{
						if (undgToDelete != undg)
						{
							undgToDelete.Delete();
						}
					}
					if (undg.IsDeleted)
					{
						undg = UNDGs.AddNew();
					}
				}
				undg.CopyPersistentValuesFrom(undgsToCopy[0], new BusinessObjectCloneArgs(ColumnsToExcludeFromUNDGCopy));
			}
		}

		IEnumerable<string> ColumnsToExcludeFromUNDGCopy
		{
			get
			{
				return new string[]
					{
						UNDGDataItemSchema.DI_ParentID.Name,
						UNDGDataItemSchema.DI_ParentTableCode.Name
					};
			}
		}

		internal bool IsUNDGSupported
		{
			get
			{
				var dec = Declaration;
				return dec != null && dec.IsUNDGSupportedOnInvoiceLines;
			}
		}

		ZString GetLocalPartNumber(ZGuid orgPk, ZString relationshipType)
		{
			ZString result = ZString.Empty;
			OrgSupplierPart part = Part;
			if (part != null && !orgPk.IsEmpty && !relationshipType.IsEmpty)
			{
				OrgPartRelation relation = part.RelatedOrganisations.FindByOrganisationPKAndRelationship(orgPk, relationshipType);
				result = (relation == null || relation.OU_LocalPartNumber.IsEmpty) ? part.OP_PartNum : relation.OU_LocalPartNumber;
			}
			return result;
		}

		protected Money GetEffectiveMoney(Money money)
		{
			return IsInvoiceCurrExRateUserEnterable ? ConvertToLocalAmountExact(money) : money;
		}

		protected IJobComInvChargeCollection<BaseInvoiceLineCharge> fCharges;
		protected virtual IJobComInvChargeCollection<BaseInvoiceLineCharge> CreateNewInvoiceLineChargeCollection()
		{
			return new JobComInvChargeCollection<BaseInvoiceLineCharge>(this);
		}

		IJobComInvApportionedChargeCollection<BaseInvoiceLineApportionedCharge> fApportionedCharges;
		protected virtual IJobComInvApportionedChargeCollection<BaseInvoiceLineApportionedCharge> CreateNewInvoiceLineApportionedChargeCollection()
		{
			return new JobComInvApportionedChargeCollection<BaseInvoiceLineApportionedCharge>(this);
		}

		protected virtual void UpdateUnitPriceIfChangedFromLinePriceChanges()
		{
			if (!IsSettingUnitPrice)
			{
				ZDecimal newUnitPrice = CalculateUnitPrice();
				if (UnitPrice != newUnitPrice)
				{
					UnitPrice = newUnitPrice;
				}
			}
		}

		protected virtual ZDecimal CalculateUnitPrice()
		{
			return JI_LinePrice == 0m || JI_InvoiceQuantity == 0 ? 0m : decimal.Round(JI_LinePrice / JI_InvoiceQuantity, UnitPriceDecimalPlaces);
		}

		protected virtual void UpdateLinePriceIfChangedFromUnitPriceChanges()
		{
			if (!IsSettingLinePrice && !(IsSettingUnitPrice && IsSettingInvoiceQuantity))
			{
				var newLinePrice = CalculateLinePriceFromUnitPrice(JI_InvoiceQuantity, UnitPrice);
				if (JI_LinePrice != newLinePrice)
				{
					JI_LinePrice = newLinePrice;
				}
			}
		}

		public virtual ZDecimal CalculateLinePriceFromUnitPrice(ZDecimal invoiceQuantity, ZDecimal invoiceUnitPrice)
		{
			try
			{
				return decimal.Round(invoiceQuantity * invoiceUnitPrice, 2);
			}
			catch (OverflowException)
			{
				return ZDecimal.Zero;
			}
		}

		#region Apportioning

		protected Money GetCharge(ICustomsChargeCode chargeCode)
		{
			return chargeCode == null ? GetEmptyMoneyInInvoiceCurrencyIfEmptyAmount(Money.Empty) : GetCharge(chargeCode.ChargeCodeChargeKey);
		}

		protected Money GetCharge(ChargeCodeChargeKey chargeKey)
		{
			var result = Charges.GetCharge(chargeKey);

			var currencyConverter = CurrencyConverter;

			if (currencyConverter != null)
			{
				result = currencyConverter.Add(result, ApportionedCharges.GetCharge(chargeKey));
			}

			return GetEmptyMoneyInInvoiceCurrencyIfEmptyAmount(result);
		}

		protected Money GetCharge(MessageChargeKey messageKey)
		{
			var result = Charges.GetCharge(messageKey);
			var currencyConverter = CurrencyConverter;

			if (currencyConverter != null)
			{
				result = currencyConverter.Add(result, ApportionedCharges.GetCharge(messageKey));
			}
			return GetEmptyMoneyInInvoiceCurrencyIfEmptyAmount(result);
		}

		protected Money GetCharge(bool isDutiable, bool isIncludedInLines)
		{
			var result = Charges.GetCharge(isDutiable, isIncludedInLines);
			var currencyConverter = CurrencyConverter;
			if (currencyConverter != null)
			{
				result = currencyConverter.Add(result, ApportionedCharges.GetCharge(isDutiable, isIncludedInLines));
			}
			return GetEmptyMoneyInInvoiceCurrencyIfEmptyAmount(result);
		}

		protected Money GetEmptyMoneyInInvoiceCurrencyIfEmptyAmount(Money initialMoney)
		{
			if (initialMoney.IsEmpty && InvoiceHeader != null)
			{
				return new Money(0m, InvoiceHeader.Invoice_Currency);
			}
			return initialMoney;
		}

		#endregion

		protected Money ConvertToLocalAmountExact(Money moneyAmount)
		{
			return CurrencyConverter == null ? Money.Empty : CurrencyConverter.ConvertExact(moneyAmount, LocalCurrency);
		}

		public CusEntryLine.Loader CusEntryLineLoader
		{
			get
			{
				if (cusEntryLineLoader == null)
				{
					cusEntryLineLoader = new CusEntryLine.Loader(Factory);
				}

				return cusEntryLineLoader;
			}
		}
		CusEntryLine.Loader cusEntryLineLoader;

		public virtual void CopyHazMatCodeFromUNDGs(OrgSupplierPart product)
		{
		}

		#endregion

		#region IExternalFactoryRefreshable Members

		bool fExternalFactoryRefreshEnabled;
		public bool ExternalFactoryRefreshEnabled
		{
			get { return fExternalFactoryRefreshEnabled; }
			set
			{
				if (fExternalFactoryRefreshEnabled != value)
				{
					fExternalFactoryRefreshEnabled = value;
					OnExternalFactoryRefreshEnabledChanged();
				}
			}
		}

		#endregion

		#region ISupportDataImporting Members

		bool fIsImportingData;
		bool ISupportDataImporting.IsImportingData
		{
			get { return fIsImportingData; }
			set { fIsImportingData = value; }
		}

		#endregion

		public bool IsDataImportInProgress
		{
			get
			{
				if (dataImportIndicatorService == null)
				{
					dataImportIndicatorService = DataImportIndicatorService.GetInstance(Factory);
				}

				return dataImportIndicatorService.IsDataImportInProgress;
			}
		}

		DataImportIndicatorService dataImportIndicatorService;

		#region ITariffProvider Members

		ZString ITariffProvider.Tariff
		{
			get { return JI_Tariff; }
		}

		ZPropertyInfo ITariffProvider.TariffInfo
		{
			get { return JI_TariffInfo; }
		}

		#endregion

		#region ICommonInvoice Members

		public IncoTermAndCustomsChargeFactory IncoTermAndChargeFactory => InvoiceHeader?.IncoTermAndChargeFactory;

		public bool IsGroup
		{
			get { return false; }
		}

		BaseJobDeclaration ICommonInvoice.JobDeclaration
		{
			get { return Declaration; }
		}

		public ZString IncoTerm
		{
			get { return InvoiceHeader?.IncoTerm ?? ZString.Empty; }
		}

		IAllInvoiceLines ICommonInvoice.InvoiceLines
		{
			get { return this; }
		}

		Common.ICommonInvoice Common.ICommonInvoice.ImmediateCommonInvoiceParent
		{
			get { return InvoiceHeader; }
		}

		ZString ICommonInvoice.UserFriendlyCode
		{
			get
			{
				ZStringBuilder result = new ZStringBuilder();
				result.Append(JI_LineNo.ToString());
				if (InvoiceHeader != null)
				{
					result.Append("/");
					result.Append(InvoiceHeader.JZ_InvoiceNumber);
				}
				return result.ToString();
			}
		}

		RefCurrencyCurrencyConverter Common.ICommonInvoice.CurrencyConverter
		{
			get { return (RefCurrencyCurrencyConverter)CurrencyConverter; }
		}

		ZString Common.ICommonInvoice.LocalCurrencyCode
		{
			get { return BaseJobComInvoiceHeader.GetLocalCurrencyCodeFor(InvoiceHeader); }
		}

		CodeDescriptionPairList ICommonInvoice.ChargeTypeList => InvoiceHeader == null ? new CodeDescriptionPairList() : Factory.GetCachedValue(InvoiceHeader.GetCustomsChargeTypeListCacheKey(ChargeParentTypes.InvoiceLine), () => InvoiceHeader.IncoTermAndChargeFactory.GetChargeTypeList(ChargeParentTypes.InvoiceLine));

		CodeDescriptionPairList ICommonInvoice.AllChargeTypeList => InvoiceHeader == null ? new CodeDescriptionPairList() : Factory.GetCachedValue(InvoiceHeader.GetCustomsChargeTypeListCacheKey(ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine), () => InvoiceHeader.IncoTermAndChargeFactory.GetChargeTypeList(ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine));

		bool Common.ICommonInvoice.HasMultipleInvoiceUQs
		{
			get { return false; }
		}

		#endregion

		#region IAllInvoiceLines members

		bool IAllInvoiceLines.Contains(BaseJobComInvoiceLine invoiceLine)
		{
			return PK == invoiceLine.PK;
		}

		int IAllInvoiceLines.Count
		{
			get { return 1; }
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IList)EnumerableList).GetEnumerator();
		}

		ArrayList EnumerableList
		{
			get
			{
				if (fEnumerableList == null)
				{
					fEnumerableList = new ArrayList
					{
						this
					};
				}
				return fEnumerableList;
			}
		}
		ArrayList fEnumerableList;

		#endregion

		#region ILandedCostDistributeTo Members

		ZGuid ILandedCostDistributeTo.PK
		{
			get { return PK; }
		}

		ZString ILandedCostDistributeTo.TableCode
		{
			get { return JobComInvoiceLineSchema.Constants.Prefix; }
		}

		public const string InvoiceLineConstString = "LINE ";
		ZString ILandedCostDistributeTo.UniqueCode
		{
			get
			{
				return InvoiceLineConstString + JI_LineNo + (NoResString)": INVOICE " +
				(InvoiceHeader != null ? InvoiceHeader.JZ_InvoiceNumber : (ZString)(NoResString)"<NO HEADER>");
			}
		}

		ZString ILandedCostDistributeTo.Description
		{
			get { return ((ILandedCostDistributeTo)this).UniqueCode; }
		}

		IEnumerable<IUltimateDistributee> ILandedCostDistributeTo.UltimateDistributees
		{
			get { yield return this; }
		}

		#endregion

		#region IUltimateDistributee Members

		ZGuid IUltimateDistributee.PK
		{
			get { return PK; }
		}

		string IUltimateDistributee.TableCode
		{
			get { return JobComInvoiceLineSchema.Constants.Prefix; }
		}

		ZGuid IUltimateDistributee.FKToProduct
		{
			get { return JI_OP; }
		}

		LCMarginPercentages IUltimateDistributee.GetProductSpecificLCMarginPercentagesForFallBack(OrgHeader consignee)
		{
			var part = Part;

			LCMarginPercentages result;
			if (part != null)
			{
				result = part.RelatedOrganisations.GetLCMarginPercentagesForFallBack(consignee);
			}
			else
			{
				result = new LCMarginPercentages();
			}
			return result;
		}

		ZString IUltimateDistributee.ProductCode
		{
			get { return Part != null ? Part.OP_PartNum : ZString.Empty; }
		}

		ZString IUltimateDistributee.ProductDepartment
		{
			get { return Part != null ? Part.OP_Department : ZString.Empty; }
		}

		ZString IUltimateDistributee.ProductDivision
		{
			get { return Part != null ? Part.OP_Division : ZString.Empty; }
		}

		ZString IUltimateDistributee.HumanReadableCode
		{
			get { return ((ILandedCostDistributeTo)this).UniqueCode; }
		}

		ZPropertyInfo[] IUltimateDistributee.HumanReadableCodeInfos
		{
			get
			{
				ArrayList result = new ArrayList();
				if (InvoiceHeader != null)
				{
					result.Add(InvoiceHeader.JZ_InvoiceNumberInfo);
				}
				result.Add(JI_LineNoInfo);
				return (ZPropertyInfo[])result.ToArray(typeof(ZPropertyInfo));
			}
		}

		ZDecimal IUltimateDistributee.Actual
		{
			get
			{
				ZDecimal result = 0m;
				if (Declaration != null)
				{
					if (Declaration.IsAir)
					{
						result = ((IUltimateDistributee)this).ActualWeightInKG;
					}
					else if (Declaration.IsSea)
					{
						result = ((IUltimateDistributee)this).ActualVolumeInM3;
					}
				}
				return result;
			}
		}

		ZDecimal IUltimateDistributee.ActualWeightInKG
		{
			get { return Constants.Weight.ConvertSafe(JI_Weight, JI_WeightUQ.ToUpper(), Constants.Weight.Kilograms); }
		}

		ZDecimal IUltimateDistributee.ActualVolumeInM3
		{
			get { return Constants.Volume.ConvertSafe(JI_Volume, JI_VolumeUQ.ToUpper(), Constants.Volume.CubicMetres); }
		}

		ZDecimal IUltimateDistributee.ItemCount
		{
			get { return JI_InvoiceQuantity == 0m ? JI_CustomsQuantity : JI_InvoiceQuantity; }
		}

		ZDecimal IUltimateDistributee.CostInLocalCurrency
		{
			get
			{
				return CostInLocalCurrencyForLC;
			}
		}

		protected ZDecimal CostInLocalCurrencyForLC
		{
			get { return LCCurrencyConverter.ConvertExact(JI_LinePriceMoney, LocalCurrency).Amount; }
		}

		protected CurrencyConverter LCCurrencyConverter
		{
			get
			{
				return fLCCurrencyConverter ?? (fLCCurrencyConverter = new CurrencyConverterWithFixedExchangeRatesDataProvider(Factory, new LandedCostCurrencyConverterDataProvider(InvoiceHeader)));
			}
		}
		CurrencyConverter fLCCurrencyConverter;

		ZDecimal IUltimateDistributee.UnitPriceInInvoiceCurrency
		{
			get { return UnitPrice; }
		}

		DutyTaxEntryFee IUltimateDistributee.LineDutyTaxEntryFeeItems
		{
			get
			{
				if (Declaration is ILandedCostHeader lcHeader && lcHeader.LandedCostType == LandedCostType.Actual)
				{
					if (cachedLineDutyTaxEntryFeeItems == null)
					{
						var landedCostingHelper = Declaration.GetLandedCostingHelper();
						if (landedCostingHelper != null)
						{
							cachedLineDutyTaxEntryFeeItems = new CachedProperty<DutyTaxEntryFee>(Factory, () => landedCostingHelper.GetLineDutyTaxEntryFeeItems(this));
						}
						else
						{
							ErrorReporter.ReportOnce("IUltimateDistributee members (Excise, OtherOrFlatDutyAmount, AllCustomsFee, SpecialTax1, SpecialTax2, SpecialTax3) should be implemented in Country-specific JobComInvoiceLine", "IUltimateDistributee members (Excise, OtherOrFlatDutyAmount, AllCustomsFee, SpecialTax1, SpecialTax2, SpecialTax3) should be implemented in Country-specific JobComInvoiceLine");
						}
					}
					if (cachedLineDutyTaxEntryFeeItems != null)
					{
						return cachedLineDutyTaxEntryFeeItems.Value;
					}
				}
				return new DutyTaxEntryFee();
			}
		}
		CachedProperty<DutyTaxEntryFee> cachedLineDutyTaxEntryFeeItems;

		ZDecimal IUltimateDistributee.DutyPercent
		{
			get { return CusEntryLine != null ? CusEntryLine.CL_DutyPercent : ZDecimal.Zero; }
		}

		ZDecimal IUltimateDistributee.CustomsValue
		{
			get { return JI_Calc_FOB_InLocalCurrency; }
		}

		ZString IUltimateDistributee.InvoiceNumber
		{
			get { return InvoiceHeader != null ? InvoiceHeader.JZ_InvoiceNumber : ZString.Empty; }
		}

		ZString IUltimateDistributee.SupplierName
		{
			get { return InvoiceHeader != null && InvoiceHeader.Supplier != null ? InvoiceHeader.Supplier.OH_FullNameTruncated : ZString.Empty; }
		}

		ZString IUltimateDistributee.InvoiceCurrencyCode
		{
			get { return InvoiceHeader != null && InvoiceHeader.Invoice_Currency != null ? InvoiceHeader.Invoice_Currency.RX_Code : ZString.Empty; }
		}

		ZString IUltimateDistributee.OrderNumber
		{
			get { return JI_OrderNumber; }
		}

		ZInt IUltimateDistributee.OrderLineNumber
		{
			get { return OrderLine == null ? ZInt.Zero : OrderLine.JO_LineNo; }
		}

		ZString IUltimateDistributee.LineDescription
		{
			get { return JI_Description; }
		}

		ZDecimal IUltimateDistributee.LandedCostingExRateFallingBackToJobInvoicingForInvoiceCurrency
		{
			get { return InvoiceHeader != null ? InvoiceHeader.LandedCostingExRateFallBackToJobExRate : ZDecimal.Zero; }
		}

		ZString IUltimateDistributee.CountryOfOriginCode
		{
			get { return JI_CountryOfOrigin; }
		}

		ZDecimal IUltimateDistributee.CustomsQuantity
		{
			get { return JI_CustomsQuantity; }
		}

		ZString IUltimateDistributee.CustomsUQ
		{
			get { return JI_CustomsUnitQty; }
		}

		ZString IUltimateDistributee.DutyRateDescription
		{
			get { return CusEntryLine != null ? CusEntryLine.DutyRateDescription : ZString.Empty; }
		}

		ZString IUltimateDistributee.InvoiceUQ
		{
			get { return JI_InvoiceUQ; }
		}

		ZDecimal IUltimateDistributee.LinePriceInInvoiceCurrency
		{
			get { return JI_LinePrice; }
		}

		ZString IUltimateDistributee.TariffNumber
		{
			get { return JI_Tariff; }
		}

		ZDecimal IUltimateDistributee.Volume
		{
			get { return JI_Volume; }
		}

		ZString IUltimateDistributee.VolumeUQ
		{
			get { return JI_VolumeUQ; }
		}

		ZDecimal IUltimateDistributee.Weight
		{
			get { return JI_Weight; }
		}

		ZString IUltimateDistributee.WeightUQ
		{
			get { return JI_WeightUQ; }
		}

		ZDecimal IUltimateDistributee.GSTVATAmount
		{
			get
			{
				var declaration = Declaration;

				return declaration != null ? declaration.LCGSTBackRoundingCalculator.GetBackRoundedGSTAmount(PK) : new ZDecimal(GSTVATAmountForLandedCost + JI_Calc_GSTVATDeferred);
			}
		}

		IEnumerable<ICustomsFee> IUltimateDistributee.Fees
		{
			get { return Array.Empty<ICustomsFee>(); }
		}

		ZBool IUltimateDistributee.IsCapableOfCalculatingOwnGstVatRate { get { return false; } }
		ZDecimal IUltimateDistributee.CalculateOwnGstVatRate() { return ZDecimal.Zero; }

		#endregion

		#region PartClassificationTariffDescriptionSyncroniser

		internal InvoiceLinePartClassificationTariffDescriptionSyncroniser PartClassificationTariffDescriptionSyncroniser
		{
			get { return fPartClassificationTariffDescriptionSyncroniser ?? (fPartClassificationTariffDescriptionSyncroniser = GetInvoiceLinePartClassificationTariffDescriptionSyncroniserCore()); }
		}
		InvoiceLinePartClassificationTariffDescriptionSyncroniser fPartClassificationTariffDescriptionSyncroniser;

		protected virtual InvoiceLinePartClassificationTariffDescriptionSyncroniser GetInvoiceLinePartClassificationTariffDescriptionSyncroniserCore() => new InvoiceLinePartClassificationTariffDescriptionSyncroniser(this);

		public void ReCalculateTariffDescriptionWhenAttachedToDec()
		{
			if (!JI_Description.IsEmpty && !ShouldSetDescriptionWhenTariffChanges && PartClassificationTariffDescriptionSyncroniser.IsDefaultDescription)
			{
				JI_Description = ZString.Empty;
			}
			else if (JI_Description.IsEmpty && ShouldSetDescriptionWhenTariffChanges)
			{
				PartClassificationTariffDescriptionSyncroniser.SetDescription();
			}
		}

		#endregion

		#region ClassificationDescription
		public ZString ClassificationDescription
		{
			get { return Classification != null ? Classification.CC_Description.Trim().ToUpper() : ZString.Empty; }
		}

		#endregion

		#region PartDescription
		public ZString PartDescription
		{
			get { return GetPartDescription(Part, Pivot); }
		}

		protected virtual ZString GetPartDescription(OrgSupplierPart part, BaseCusClassPartPivot pivot)
		{
			var result = ZString.Empty;

			if (part != null)
			{
				result = pivot?.CI_Description ?? ZString.Empty;
				if (result.IsEmpty)
				{
					result = part.OP_Desc;
				}

				if (Declaration?.PrefixPartDescriptionWithPartNumber ?? false)
				{
					result = part.OP_PartNum + " - " + result;
				}
			}

			return result;
		}
		#endregion

		#region PartExtendedCommercialDescription
		public ZString PartExtendedCommercialDescription
		{
			get { return GetPartExtendedCommercialDescription(Part); }
		}

		protected ZString GetPartExtendedCommercialDescription(OrgSupplierPart part)
		{
			return part == null || !IsExtendedCommercialDescriptionEnabled ? ZString.Empty : part.OP_ExtendedCommercialDescription;
		}
		#endregion

		#region Ruling Configurations

		[ChildEditable(true)]
		[ChildEditableTestExclude]
		public CusRulingConfigCombinedCollection RulingConfigurations
		{
			get
			{
				if (configurations == null)
				{
					configurations = GetRulingConfigurations();
					if (SupportRulingConfigurations)
					{
						configurations.Load();
						configurations.SetReadOnlyIncludingChildren(ConfigurationsReadOnly);
						RegisterEditableChildObject(configurations);
					}
					else
					{
						((ILegacyBusinessObjectCollectionInternals)configurations).SetOverriddenAdditionalFilter(ZQuery.NoResultQuery);
						configurations.SetCountedReadOnlyIncludingChildren(true);
					}
				}
				return configurations;
			}
		}
		CusRulingConfigCombinedCollection configurations;

		protected virtual CusRulingConfigCombinedCollection GetRulingConfigurations()
		{
			return new CusRulingConfigCombinedCollection(this);
		}

		internal protected virtual bool SupportRulingConfigurations
		{
			get { return false; }
		}

		#endregion

		#region CusLineTariffDetails

		[ChildEditable(true)]
		[BusinessObjectTestExclude]
		[UniversalCopyCollectionEntity(CusLineTariffDetailSchema.Constants.TableName, CusLineTariffDetailSchema.Constants.BZ_ParentID)]
		public ICusLineTariffDetailCollection<CusLineTariffDetail> CusLineTariffDetails
		{
			get
			{
				if (cusLineTariffDetails == null)
				{
					cusLineTariffDetails = GetCusLineTariffDetails();
					if (SupportsAdditionalTariffs)
					{
						cusLineTariffDetails.Load();
						cusLineTariffDetails.CountChanged += CusLineTariffDetailsCountChanged;
						RegisterEditableChildObject(cusLineTariffDetails);
					}
					else
					{
						((ILegacyBusinessObjectCollectionInternals)cusLineTariffDetails).SetOverriddenAdditionalFilter(ZQuery.NoResultQuery);
						cusLineTariffDetails.SetCountedReadOnlyIncludingChildren(true);
					}
				}
				return cusLineTariffDetails;
			}
		}
		ICusLineTariffDetailCollection<CusLineTariffDetail> cusLineTariffDetails;

		protected virtual ICusLineTariffDetailCollection<CusLineTariffDetail> GetCusLineTariffDetails() => new CusLineTariffDetailCollection<CusLineTariffDetail>(this);

		internal protected virtual bool SupportsAdditionalTariffs => false;

		protected virtual void CusLineTariffDetailsCountChanged(object sender, CollectionCountChangedEventArgs e)
		{
		}

		protected virtual ZBool ConfigurationsReadOnly => ZBool.False;

		ZString ICusLineTariffDetailParent.CustomsCountryCode => CustomsCountryCode;

		#endregion

		#region IBondedWarehouseTransactionLineProvider Members

		IWhsBondedWarehouseTransactionLine IBondedWarehouseTransactionLineProvider.TransactionLine
		{
			get
			{
				if (fBondedWarehouseTransactionLine == null)
				{
					fBondedWarehouseTransactionLine = GetNewBondedWarehouseTransactionLine();
				}
				return fBondedWarehouseTransactionLine;
			}
		}

		protected virtual BondedWarehouseTransactionLine GetNewBondedWarehouseTransactionLine()
		{
			return new BondedWarehouseTransactionLine(this);
		}

		BondedWarehouseTransactionLine fBondedWarehouseTransactionLine;

		#endregion

		#region IChargeApportionee Members

		bool IChargeApportionee.IsValidToApportionTo
		{
			get { return IsValidToApportionToCore; }
		}

		protected virtual bool IsValidToApportionToCore
		{
			get { return true; }
		}

		IJobComInvApportionedChargeCollection<JobComInvCharge> IChargeApportionee.ApportionedCharges
		{
			get { return ApportionedCharges; }
		}

		ZDecimal IChargeApportionee.GetBaseValueToApportionOn(CurrencyConverter currencyConverter, string distributeBy)
		{
			return GetBaseValueToApportionOnCore(currencyConverter, distributeBy);
		}

		void IChargeApportionee.CalculateAmountBasedOnPercentage(JobComInvCharge charge)
		{
			CalculateAmountBasedOnPercentageIfNecessary(charge);
		}

		public void CalculateAmountBasedOnPercentageIfNecessary(JobComInvCharge charge)
		{
			if (charge.J7_Percentage > 0 &&
				ShouldRecalculatePercentageChargeAmountBasedOnLinePrice &&
				LinePriceRefCurrency != null)
			{
				charge.J7_Amount = JI_LinePrice * charge.J7_Percentage / 100;
				charge.J7_RX_NKCurrency = LinePriceRefCurrency.RX_Code;
			}
		}

		public void SetChargeCurrencyToLinePriceRefCurrencyIfNeeded()
		{
			if (ShouldRecalculatePercentageChargeAmountBasedOnLinePrice && LinePriceRefCurrency != null)
			{
				foreach (JobComInvCharge charge in Charges)
				{
					if (charge.ShouldSetCurrencyFromParent)
					{
						charge.J7_RX_NKCurrency = LinePriceRefCurrency.RX_Code;
					}
				}
			}
		}

		protected virtual ZDecimal GetBaseValueToApportionOnCore(CurrencyConverter currencyConverter, string distributeBy)
		{
			switch (distributeBy)
			{
				case ChargeDistributeByList.Codes.Value:
					return currencyConverter.ConvertExact(JI_LinePriceMoney, LocalCurrency).Amount;
				case ChargeDistributeByList.Codes.Weight:
					return Core.Constants.Weight.ContainsCode(JI_WeightUQ.ToUpper()) ? Core.Constants.Weight.Convert(JI_Weight, JI_WeightUQ.ToUpper(), Core.Constants.Weight.Kilograms) : 0m;
				case ChargeDistributeByList.Codes.Volume:
					return Core.Constants.Volume.ContainsCode(JI_VolumeUQ.ToUpper()) ? Core.Constants.Volume.Convert(JI_Volume, JI_VolumeUQ.ToUpper(), Core.Constants.Volume.CubicMetres) : 0m;
				case ChargeDistributeByList.Codes.Quantity:
					return JI_InvoiceQuantity;
				default:
					return 0m;
			}
		}

		bool IChargeApportionee.CanThisChargeBeApportionedBasedOnIncoterm(ApportionChargeKey apportionedChargeKey)
		{
			return InvoiceHeader != null && ((IChargeApportionee)InvoiceHeader).CanThisChargeBeApportionedBasedOnIncoterm(apportionedChargeKey);
		}

		bool IChargeHolder.IsGroupInvoice
		{
			get { return false; }
		}

		#endregion

		#region IChargeHolder Members

		IJobComInvChargeCollection<JobComInvCharge> IChargeHolder.Charges
		{
			get { return Charges; }
		}

		IChargeApportionee[] IChargeHolder.AllApportionees => Array.Empty<IChargeApportionee>();

		IChargeHolder[] IChargeHolder.ImmediateChargeHolderChildren
		{
			get { return Array.Empty<IChargeHolder>(); }
		}

		IChargeHolder IChargeHolder.ImmediateChargeHolderParent
		{
			get { return InvoiceHeader; }
		}

		ZString IChargeHolder.GetDefaultCurrencyCode(ICustomsChargeCode chargeType, JobComInvCharge charge)
		{
			return InvoiceHeader != null ? ((IChargeHolder)InvoiceHeader).GetDefaultCurrencyCode(chargeType, charge) : ZString.Empty;
		}

		ZString IChargeHolder.GetDefaultDistributeBy() => ZString.Empty;

		#endregion

		#region CustomsWeight
		public ZWeight CustomsWeight
		{
			get { return GetCustomsWeight(); }
		}

		protected virtual ZWeight GetCustomsWeight()
		{
			return new ZWeight(JI_CustomsQuantity, JI_CustomsUnitQty);
		}
		#endregion

		#region CustomsSecondQuantity
		public ZWeight CustomsSecondQuantity
		{
			get { return GetCustomsSecondQuantity(); }
		}

		protected ZWeight GetCustomsSecondQuantity()
		{
			return new ZWeight(JI_CustomsSecondQuantity, JI_CustomsSecondUnitQty);
		}
		#endregion

		#region SoldToPartyOrgPK
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.SoldToParties))]
		public ZGuid SoldToPartyOrgPK
		{
			get { return JI_OA_SoldToPartyAddress_ZAddress.OrgPK; }
			set { JI_OA_SoldToPartyAddress_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo SoldToPartyOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.SoldToPartyOrgPK, x => JI_OA_SoldToPartyAddress_ZAddress.OrgPKInfo); }
		}

		public OrgHeader SoldToParty
		{
			get { return Factory.Load<OrgHeader>(SoldToPartyOrgPK); }
		}

		#endregion

		#region ManufacturerOrgPK

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.SupplierList))]
		public virtual ZGuid ManufacturerOrgPK
		{
			get { return JI_OA_ManufacturerAddress_ZAddress.OrgPK; }
			set { JI_OA_ManufacturerAddress_ZAddress.OrgPK = value; }
		}

		public virtual ZPropertyInfo ManufacturerOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ManufacturerOrgPK, x => JI_OA_ManufacturerAddress_ZAddress.OrgPKInfo); }
		}

		#endregion

		#region ConsigneeAddressOrgPK

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.Consignees))]
		public ZGuid ConsigneeAddressOrgPK
		{
			get { return JI_OA_ConsigneeAddress_ZAddress.OrgPK; }
			set { JI_OA_ConsigneeAddress_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo ConsigneeAddressOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ConsigneeAddressOrgPK, x => JI_OA_ConsigneeAddress_ZAddress.OrgPKInfo); }
		}

		public OrgHeader ConsigneeOrgAddress
		{
			get { return Factory.Load<OrgHeader>(ConsigneeAddressOrgPK); }
		}

		#endregion

		public virtual bool IsExtendedCommercialDescriptionEnabled
		{
			get { return false; }
		}

		public virtual bool UseImportClassification
		{
			get { return IsImport || IsDrawback; }
		}

		public virtual bool UseExportClassification
		{
			get { return IsExport; }
		}

		#region Product Audit

		public bool IsProductAuditedOrInvalid() => IsProductAuditedOrInvalidCore();

		protected virtual bool IsProductAuditedOrInvalidCore() => Pivot?.CI_LastAuditedDate.IsValid ?? true;

		public bool IsLookupAudited() => IsProductAuditedOrInvalid() && (Classification?.CC_LastAuditedDate.IsValid ?? true);

		#endregion

		#region Drawbacks

		public bool IsDrawback => Declaration?.IsDrawback ?? false;

		public virtual bool IsForImportSectionOfDrawback
		{
			get { return false; }
		}

		public virtual bool IsForExportSectionOfDrawback
		{
			get { return false; }
		}

		public virtual ZString DrawbackImportDeclarationNumber
		{
			get { return ZString.Empty; }
		}

		public virtual ZInt DrawbackImportDeclarationLine
		{
			get { return 0; }
		}

		public IBaseDrawbackEntryLine DrawbackImportEntryLine
		{
			get
			{
				CalculateDrawbackImportEntryLine();
				return drawbackImportEntryLine;
			}
		}
		IBaseDrawbackEntryLine drawbackImportEntryLine;

		void CalculateDrawbackImportEntryLine()
		{
			if (!hasCalculateDrawbackImportEntryLine)
			{
				hasCalculateDrawbackImportEntryLine = true;
				drawbackImportEntryLine = !DrawbackImportDeclarationNumber.IsEmpty && DrawbackImportDeclarationLine > 0 ? DrawbackImportEntryLineCore : null;
			}
		}
		bool hasCalculateDrawbackImportEntryLine;

		public void RefreshDrawbackImportEntryLine()
		{
			hasCalculateDrawbackImportEntryLine = false;
		}

		protected virtual IBaseDrawbackEntryLine DrawbackImportEntryLineCore
		{
			get { return CusEntryLineLoader.FindByDeclarationAndLineNumbers(DrawbackImportDeclarationNumber, DrawbackImportDeclarationLine); }
		}

		public ZDecimal DrawbackClaimQuantity
		{
			get
			{
				return JI_CustomsQuantity == 0 && JI_CustomsUnitQty.IsEmpty ? JI_InvoiceQuantity : JI_CustomsQuantity;
			}
		}

		#endregion

		#region BOM Proceesing

		#region BOMExpander Members

		public virtual ZGuid BOMParentLinePK { get; set; }

		public virtual ZBool IsBOMLineExpanded { get; set; }
		#endregion

		public ZBool IsBOMParentLine
		{
			get { return Part != null && Part.BillOfMaterials.Count > 0; }
		}

		public ZPropertyInfo IsBOMParentLineInfo
		{
			get { return GetZPropertyInfo(nameof(IsBOMParentLine)); }
		}

		public BaseJobComInvoiceLine BOMParentLine
		{
			get
			{
				if (bOMParentLine == null || bOMParentLine.PK != BOMParentLinePK)
				{
					bOMParentLine = Factory.Load<BaseJobComInvoiceLine>(BOMParentLinePK);
				}
				return bOMParentLine;
			}
			set
			{
				BOMParentLinePK = value.PK;
			}
		}
		BaseJobComInvoiceLine bOMParentLine;

		public ZString BOMParentLineNumber
		{
			get { return BOMParentLine != null ? BOMParentLine.JI_LineNo.ToString() : ""; }
		}

		public ZPropertyInfo BOMParentLineNumberInfo
		{
			get { return GetZPropertyInfo(nameof(BOMParentLineNumber)); }
		}

		#endregion

		#region ICanDelete

		bool ICanDelete.CanDelete
		{
			get { return CanDeleteCore; }
		}

		protected virtual bool CanDeleteCore => !IsBOMLineExpanded;

		MultilingualString ICanDelete.ReasonForNotAbleToDelete
		{
			get { return ReasonForNotAbleToDeleteCore; }
		}

		protected virtual MultilingualString ReasonForNotAbleToDeleteCore
		{
			get { return ResString.GetMultilingualString("c7f55f96-9740-4a59-a19b-81019310981a", "This Line may not be deleted because there are 'Expanded' Bill Of Materials Lines linked to this line. Collapse this line first and then delete it (Right Click on Line and select 'Collapse Bill Of Materials Line')."); }
		}

		#endregion

		protected ZDecimal ItemPriceInLocalCurrency => CurrencyConverter.ConvertExact(new Money(ComponentPrice, InvoiceHeader.Invoice_Currency), LocalCurrency).Amount;

		// BP: VAT Value = Line Price + Sum of VATable Additions – Sum of non-VATable deductions
		public virtual ZDecimal JI_Calc_ValueForVat
		{
			get
			{
				decimal result = 0m;
				if (InvoiceHeader != null)
				{
					result = ItemPriceInLocalCurrency + ValuationCalculator.GetAmountToAddToITOTForVatableGstable(LocalCurrency);
				}
				return result;
			}
		}

		public ZDecimal LinePriceForWeightApportionCalculation
		{
			get { return LinePriceForWeightApportionCalculationCore; }
		}

		protected virtual ZDecimal LinePriceForWeightApportionCalculationCore
		{
			get { return JI_LinePrice; }
		}

		void Integration.Customs.IBaseJobComInvoiceLine.SynchroniseFromForwardingOrderLine(Integration.Forwarding.IOrderLine orderLine, bool autoAddOrderNumberOnSet)
		{
			var specificOrderLine = (OrderLine)orderLine;
			if (specificOrderLine != null)
			{
				AutoAddOrderNumberOnSet = autoAddOrderNumberOnSet;
				SynchroniseFromForwardingOrderLineCore(specificOrderLine);
			}
		}

		protected virtual void SynchroniseFromForwardingOrderLineCore(OrderLine specificOrderLine)
		{
			((ZPropertyInfo<ZString>)JI_DescriptionInfo).SetValueSafe(specificOrderLine.JO_Description);
			((ZPropertyInfo<ZString>)JI_PartNoInfo).SetValueSafe(specificOrderLine.JO_Partno);
			((ZPropertyInfo<ZString>)JI_InvoiceUQInfo).SetValueSafe(specificOrderLine.JO_F3_NKPackType);
			((ZPropertyInfo<ZString>)JI_CustomAttrib1Info).SetValueSafe(specificOrderLine.JO_CustomAttrib1);
			((ZPropertyInfo<ZString>)JI_CustomAttrib2Info).SetValueSafe(specificOrderLine.JO_CustomAttrib2);
			((ZPropertyInfo<ZString>)JI_CustomAttrib3Info).SetValueSafe(specificOrderLine.JO_CustomAttrib3);
			((ZPropertyInfo<ZString>)JI_CustomAttrib4Info).SetValueSafe(specificOrderLine.JO_CustomAttrib4);
			((ZPropertyInfo<ZString>)JI_CustomAttrib5Info).SetValueSafe(specificOrderLine.JO_CustomAttrib5);
			((ZPropertyInfo<ZString>)JI_CustomAttrib6Info).SetValueSafe(specificOrderLine.JO_CustomAttrib6);
			((ZPropertyInfo<ZString>)JI_CustomTextBlob1Info).SetValueSafe(specificOrderLine.JO_CustomTextBlob1);
			((ZPropertyInfo<ZString>)JI_PartAttrib1Info).SetValueSafe(specificOrderLine.JO_PartAttrib1);
			((ZPropertyInfo<ZString>)JI_PartAttrib2Info).SetValueSafe(specificOrderLine.JO_PartAttrib2);
			((ZPropertyInfo<ZString>)JI_PartAttrib3Info).SetValueSafe(specificOrderLine.JO_PartAttrib3);
			((ZPropertyInfo<ZString>)JI_SerialNumberInfo).SetValueSafe(specificOrderLine.JO_SerialNumber);
			JI_CustomDate1 = specificOrderLine.JO_CustomDate1;
			JI_CustomDate2 = specificOrderLine.JO_CustomDate2;
			JI_CustomDate3 = specificOrderLine.JO_CustomDate3;
			JI_CustomDate4 = specificOrderLine.JO_CustomDate4;
			JI_CustomDate5 = specificOrderLine.JO_CustomDate5;
			JI_CustomDecimal1 = specificOrderLine.JO_CustomDecimal1;
			JI_CustomDecimal2 = specificOrderLine.JO_CustomDecimal2;
			JI_CustomDecimal3 = specificOrderLine.JO_CustomDecimal3;
			JI_CustomDecimal4 = specificOrderLine.JO_CustomDecimal4;
			JI_CustomDecimal5 = specificOrderLine.JO_CustomDecimal5;
			JI_CustomFlag1 = specificOrderLine.JO_CustomFlag1;
			JI_CustomFlag2 = specificOrderLine.JO_CustomFlag2;
			JI_CustomFlag3 = specificOrderLine.JO_CustomFlag3;
			JI_CustomFlag4 = specificOrderLine.JO_CustomFlag4;
			JI_CustomFlag5 = specificOrderLine.JO_CustomFlag5;
			JI_JO = specificOrderLine.PK;

			((ZPropertyInfo<ZString>)JI_OrderNumberInfo).SetValueSafe(specificOrderLine.Order.JD_OrderNumber);
			JI_InvoiceQuantity = specificOrderLine.JO_QtyInvoiced;
			JI_LinePrice = specificOrderLine.JO_QtyInvoiced > 0 ? new ZDecimal(specificOrderLine.JO_ItemPrice * specificOrderLine.JO_QtyInvoiced) : specificOrderLine.JO_LinePrice;  // JO_LinePrice is the price of all the items ordered, not the price of those being invoiced/fulfilled now. e.g. 2000 items @ £1 each ordered (JO_ItemPrice = 1,  JO_Quantity = 2000, JI_LinePrice = 2000), of which 1500 are invoiced and declared (JO_QtyInvoiced = 1500), we should not set  JI_Price to 2000 but to 1500.

			var countryOfOriginFromOrderLine = new ZString(specificOrderLine.JO_RN_NKCountryOfOrigin.IsEmpty ? specificOrderLine.Order.JD_RN_NKCountryOfSupply : specificOrderLine.JO_RN_NKCountryOfOrigin);
			if (!countryOfOriginFromOrderLine.IsEmpty)
			{
				CountryOfOriginFieldInfo.Value = countryOfOriginFromOrderLine;
			}
		}

		#region IWeightApportionee Members

		ZDecimal IWeightApportionee.Amount
		{
			get { return LinePriceForWeightApportionCalculation; }
		}

		ZDecimal IWeightApportionee.Weight
		{
			get { return JI_Weight; }
			set { JI_Weight = value; }
		}

		ZString IWeightApportionee.WeightUQ
		{
			get { return JI_WeightUQ; }
			set { JI_WeightUQ = value; }
		}

		ZDecimal IWeightApportionee.NetWeight
		{
			get { return JI_NetWeight; }
			set { JI_NetWeight = value; }
		}

		ZString IWeightApportionee.NetWeightUQ
		{
			get { return JI_NetWeightUQ; }
			set { JI_NetWeightUQ = value; }
		}

		public virtual bool NeedToApportionNetWeight
		{
			get { return true; }
		}

		#endregion

		#region IShortSequenceNumberLine Members

		ZGuid ISequenceNumberLine.FKToHeader => JI_JZ;

		ZShort ISequenceNumberLine<ZShort>.SequenceNumber
		{
			get => JI_LineNo;
			set => JI_LineNo = value;
		}

		#endregion

		#region IRegistryAccessingSupporter Members

		public virtual Guid RegistryCompanyPK
		{
			get
			{
				var invoice = InvoiceHeader;
				return invoice == null ? GlbCompany.CurrentCompany.PK.ToGuid() : invoice.RegistryCompanyPK;
			}
		}

		public virtual Guid RegistryBranchPK
		{
			get
			{
				var invoice = InvoiceHeader;
				return invoice == null ? GlbBranch.CurrentBranch.PK.ToGuid() : invoice.RegistryBranchPK;
			}
		}

		#endregion

		#region ICommonNonApportionedChargeProvider<BaseInvoiceLineCharge> Members

		BaseInvoiceLineCharge ICommonNonApportionedChargeProvider<BaseInvoiceLineCharge>.CreateNew()
		{
			return Charges.AddNew();
		}

		BaseInvoiceLineCharge ICommonNonApportionedChargeProvider<BaseInvoiceLineCharge>.GetChargeWithZeroAmount(ZString chargeCode)
		{
			return Charges.GetCharge(chargeCode).FirstOrDefault(x => x.J7_Amount.IsEmpty);
		}

		#endregion

		#region IUnitConverterDataProvider

		public ZString CustomsCountryCode => CustomsCountryCodeCore;
		protected virtual ZString CustomsCountryCodeCore => Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(InvoiceHeader?.CountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

		ZString IUnitConverterDataProvider.CountryCode
		{
			get { return CustomsCountryCode; }
		}

		BusinessObjectFactory IUnitConverterDataProvider.Factory
		{
			get { return Factory; }
		}

		IEnumerable<IUnitConverter> IUnitConverterDataProvider.GetUnitConversionFactorsFromProductUnits()
		{
			return Part?.GetUnitConversionFactorsFromProductUnits() ?? Array.Empty<IUnitConverter>();
		}

		MasterFiles.Business.OrgSupplierPart IUnitConverterDataProvider.Product
		{
			get { return Part; }
		}

		ZGuid IUnitConverterDataProvider.SupplierFK
		{
			get { return InvoiceHeader?.JZ_OH_Supplier ?? ZGuid.Empty; }
		}

		bool IUnitConverterDataProvider.ProductHasSpecificUnitConversions
		{
			get { return Part?.HasSpecificUnitConversions() ?? false; }
		}

		ZString IUnitConverterDataProvider.Type
		{
			get { return RPTypeList.Codes.CommercialInvoice; }
		}

		#endregion

		#region IWorkflowProvider Members

		public ZString WorkflowType => WorkflowDescriptors.CommericalInvoiceLineWorkflowDescriptorCode;

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		[ChildEditable(true)]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new ProcessTaskCollection(this)); // Generic process task collection as we only want custom fields
					RegisterEditableChildObject(workflowItems);
				}

				return workflowItems;
			}
		}
		ProcessTaskCollection workflowItems;

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			var result = new ColumnValueRanker();
			var invoice = InvoiceHeader;

			if (invoice != null)
			{
				result.Add(ProcessTaskTemplateSchema.P0_OH_Client, invoice.GetClients());
				result.Add(ProcessTaskTemplateSchema.P0_SubType1, invoice.JZ_MessageType, ZString.Empty);
				result.Add(ProcessTaskTemplateSchema.P0_GB, invoice.IsAttachedToPersistentDeclaration ? invoice.JobDeclaration.JE_GB : invoice.JZ_GB, ZGuid.Empty);
			}

			return result;
		}

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		#endregion

		#region ICustomFieldProvider Members

		CustomBusinessObject ICustomFieldProvider.GetCustomBusinessObject(bool shouldRefresh)
		{
			if (customBusinessObject == null || shouldRefresh)
			{
				var properties = new UserDefinedPropertyCollection(this, () => { this.RefreshBinding(); }).WithWorkflowTemplateCustomFields(this);
				customBusinessObject = new CustomBusinessObject(Factory, this, properties);
			}

			return customBusinessObject;
		}

		CustomBusinessObject customBusinessObject;

		#endregion

		#region IWorkflowAffectedPropertyProvider Members

		public ZPropertyInfo[] PropertyThatAffectWorkflowChanged
		{
			get
			{
				var result = new List<ZPropertyInfo>();
				if (InvoiceHeader is BaseJobComInvoiceHeader invoice)
				{
					result.AddRange(PropertyThatAffectWorkflowChangedFromInvoiceHeader(invoice));
					if (invoice.IsAttachedToPersistentDeclaration)
					{
						result.AddRange(PropertyThatAffectWorkflowChangedFromDeclaration(invoice.JobDeclaration));
					}
				}
				return result.ToArray();
			}
		}

		public static ZPropertyInfo[] PropertyThatAffectWorkflowChangedFromInvoiceHeader(BaseJobComInvoiceHeader invoiceHeader)
		{
			var result = new List<ZPropertyInfo>();
			if (invoiceHeader != null)
			{
				result.Add(invoiceHeader.JZ_OH_BuyerInfo);
				result.Add(invoiceHeader.JZ_OH_SupplierInfo);
				result.Add(invoiceHeader.JZ_MessageTypeInfo);
			}
			return result.ToArray();
		}

		public static ZPropertyInfo[] PropertyThatAffectWorkflowChangedFromDeclaration(BaseJobDeclaration declaration)
		{
			var result = new List<ZPropertyInfo>();
			if (declaration != null)
			{
				result.Add(declaration.JE_MessageTypeInfo);
				result.Add(declaration.JE_OH_ImporterInfo);
				result.Add(declaration.JE_OH_SupplierInfo);
			}
			return result.ToArray();
		}

		#endregion

		public virtual ZBool ShouldClone { get { return true; } }

		#region IInvoiceLinePartDetails
		internal void RefreshPartSyncManagerActiveDeciderPK(ZGuid oldPartSyncManagerActiveDeciderPK)
		{
			partSyncManagerActiveDeciderPK = null;
			PartSyncManager?.RefreshPartSyncManagerActiveDeciderPKDictionary(oldPartSyncManagerActiveDeciderPK);
		}

		ZString IInvoiceLinePartDetails.CustomsCountryCode => CustomsCountryCode;
		ZGuid IInvoiceLinePartDetails.PartSyncManagerActiveDeciderPK => PartSyncManagerActiveDeciderPK;
		void IInvoiceLinePartDetails.UpdateDetailsOnPartChange() => UpdateDetailsOnPartChange();
		OrgHeader IInvoiceLinePartDetails.Importer => Importer;
		OrgHeader IInvoiceLinePartDetails.Supplier => Supplier;
		BaseJobComInvoiceHeader IInvoiceLinePartDetails.Header => InvoiceHeader;
		bool IInvoiceLinePartDetails.IsForImportSectionOfDrawback => IsForImportSectionOfDrawback;

		bool IInvoiceLinePartDetails.IsForExportSectionOfDrawback => IsForExportSectionOfDrawback;
		bool IInvoiceLinePartDetails.IsDrawback => IsDrawback;
		bool IInvoiceLinePartDetails.IsDeleted => IsDeleted;
		bool IInvoiceLinePartDetails.Enabled => !IsAdvanceShippingNoticeInvoice;
		ZGuid IInvoiceLinePartDetails.PartPK
		{
			get { return JI_OP; }
			set { JI_OP = value; }
		}
		ZString IInvoiceLinePartDetails.PartNo => JI_PartNo;
		RefCountry IInvoiceLinePartDetails.InvoiceCountry => InvoiceHeader?.InvoiceCountry;
		bool IInvoiceLinePartDetails.JustUpdatedByDataRefresh
		{
			get
			{
				var invoiceHeader = InvoiceHeader;
				return invoiceHeader != null && (invoiceHeader.IsAdvanceShippingNotice || invoiceHeader.JZ_StandAloneInvoiceDirection == JobMessageTypeList.MoreCodes.AdvanceShippingNotice);
			}
		}
		Type IInvoiceLinePartDetails.TypeOfPartUsed => TypeOfPartUsed;
		BusinessObjectFactory IInvoiceLinePartDetails.Factory => Factory;

		public ZGuid PartSyncManagerActiveDeciderPK
		{
			get
			{
				if (!partSyncManagerActiveDeciderPK.HasValue)
				{
					var result = ZGuid.Empty;
					if (InvoiceHeader is BaseJobComInvoiceHeader invoice)
					{
						result = invoice.JZ_JE;
						if (!result.IsValid)
						{
							result = invoice.PK;
						}
					}
					partSyncManagerActiveDeciderPK = result;
				}
				return partSyncManagerActiveDeciderPK.Value;
			}
		}
		ZGuid? partSyncManagerActiveDeciderPK;

		#endregion

		#region SetterSuspender

		public SetterSuspender SetterSuspender => setterSuspender ?? (setterSuspender = new SetterSuspender());
		SetterSuspender setterSuspender;

		IEnumerable<string> ISetterSuspenderSupporter.SupportedFields => GetSupportedFields();

		protected virtual IEnumerable<string> GetSupportedFields()
		{
			yield return Schema.JI_Tariff;
			yield return Schema.JI_InvoiceUQ;
			yield return Schema.JI_CustomsQuantity;
			yield return Schema.JI_CustomsUnitQty;
			yield return Schema.JI_CustomsSecondQuantity;
			yield return Schema.JI_CustomsSecondUnitQty;
			yield return Schema.JI_CustomsThirdQuantity;
			yield return Schema.JI_CustomsThirdUnitQty;
			yield return Schema.JI_CustomsFourthQuantity;
			yield return Schema.JI_CustomsFourthUnitQty;
		}

		#endregion

		#region Suspend UOM Defaulting

		protected bool IsUOMDefaultingSuspended
		{
			get { return uomDefaultingSuspenderIndex > 0; }
		}
		int uomDefaultingSuspenderIndex;

		public IDisposable SuspendUOMDefaulting()
		{
			return new UOMDefaultingSuspender(this);
		}

		class UOMDefaultingSuspender : IDisposable
		{
			public UOMDefaultingSuspender(BaseJobComInvoiceLine invoiceLine)
			{
				this.invoiceLine = invoiceLine;
				this.invoiceLine.uomDefaultingSuspenderIndex++;
			}

			readonly BaseJobComInvoiceLine invoiceLine;

			#region IDisposable Members

			public void Dispose()
			{
				invoiceLine.uomDefaultingSuspenderIndex--;
			}

			#endregion
		}

		#endregion

		#region ILandedCostChargeHolder Members

		IEnumerable<IDefaultLandedCostInput> ILandedCostChargeHolder.ChargesToImportForLandedCosting
		{
			get
			{
				if (!(Declaration?.ApportionmentDirty ?? true))
				{
					foreach (var invoiceLineCharge in Charges.Cast<IDefaultLandedCostInput>().Where(x => x.IsValidToImport))
					{
						yield return invoiceLineCharge;
					}
				}
			}
		}

		#endregion

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.GoodsCatalogList))]
		[RelatedBusinessObject(nameof(GoodsCatalog))]
		public override ZGuid JI_CGC_Catalog { get => base.JI_CGC_Catalog; set => base.JI_CGC_Catalog = value; }

		public BaseCusGoodsCatalog GoodsCatalog
		{
			get
			{
				if (!IsDeleted && (goodsCatalog == null || goodsCatalog.PK != JI_CGC_Catalog))
				{
					goodsCatalog = Factory.Load<BaseCusGoodsCatalog>(JI_CGC_Catalog);
				}
				return goodsCatalog;
			}
		}
		BaseCusGoodsCatalog goodsCatalog;

		public BusinessObject GetTopBusinessObject()
		{
			var parentInvoiceHeader = Factory.Load<CommonJobComInvoiceHeader>(JI_JZ);
			return parentInvoiceHeader.JobDeclaration;
		}

		public bool SupportsJobComInvoiceLineTax => Declaration?.SupportsJobComInvoiceLineTax ?? false;

		public virtual OrgAddress ConsigneeAddressForDocument => base.ConsigneeAddress;

		public IZZRateSelectionCriteria AllApplicableRatesSelectionCriteria => (allApplicableRatesSelectionCriteria ?? (allApplicableRatesSelectionCriteria = new CachedProperty<IZZRateSelectionCriteria>(Factory, GetAllApplicableRatesSelectionCriteriaCore))).Value;
		CachedProperty<IZZRateSelectionCriteria> allApplicableRatesSelectionCriteria;

		protected virtual IZZRateSelectionCriteria GetAllApplicableRatesSelectionCriteriaCore() => new RateSelectionCriteria<BaseJobComInvoiceLine>(this, ZString.Empty, ZString.Empty);

		public IZZRateSelectionCriteria DutyRateSelectionCriteria => (dutyRateSelectionCriteria ?? (dutyRateSelectionCriteria = new CachedProperty<IZZRateSelectionCriteria>(Factory, GetDutyRateSelectionCriteriaCore))).Value;
		CachedProperty<IZZRateSelectionCriteria> dutyRateSelectionCriteria;

		protected virtual IZZRateSelectionCriteria GetDutyRateSelectionCriteriaCore() => new RateSelectionCriteria<BaseJobComInvoiceLine>(this, Universal.Constants.RateTypes.Duty, ZString.Empty);

		protected virtual RateDirection RateSelectionCriteriaDirection => RateDirection.Both;

		public class RateSelectionCriteria<T> : IZZRateSelectionCriteria where T : BaseJobComInvoiceLine
		{
			public RateSelectionCriteria(T invoiceLine, ZString rateType, ZString rateCode)
			{
				EffectiveDate = GetEffectiveDate(invoiceLine);
				TradeGroupCountry = GetTradeGroupCountry(invoiceLine);
				SecondTradeGroups = GetSecondTradeGroups(invoiceLine);
				DataGrouping = invoiceLine.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff);
				PrimaryPreference = GetPrimaryPreference(invoiceLine);
				AdditionalCodes = GetAdditionalCodes(invoiceLine);
				ConcessionOrder = GetConcessionOrder(invoiceLine);
				RateType = rateType;
				RateCode = rateCode;
				Direction = invoiceLine.RateSelectionCriteriaDirection;
			}

			public ZDateTime EffectiveDate { get; protected set; }
			public ZString TradeGroupCountry { get; protected set; }
			public ISet<ZString> SecondTradeGroups { get; protected set; }
			public ZString DataGrouping { get; protected set; }
			public ZString PrimaryPreference { get; protected set; }
			public ISet<ZString> AdditionalCodes { get; protected set; }
			public ZString ConcessionOrder { get; protected set; }
			public ZString RateType { get; protected set; }
			public ZString RateCode { get; protected set; }
			public RateDirection Direction { get; protected set; }

			protected virtual ZDateTime GetEffectiveDate(T invoiceLine) => invoiceLine.EffectiveAssessmentDate;
			protected virtual ZString GetTradeGroupCountry(T invoiceLine) => invoiceLine.EffectiveCountryOfOrigin;
			protected virtual ISet<ZString> GetSecondTradeGroups(T invoiceLine) => new HashSet<ZString>();
			protected virtual ZString GetPrimaryPreference(T invoiceLine) => invoiceLine.EffectivePrimaryPreference;
			protected virtual ISet<ZString> GetAdditionalCodes(T invoiceLine) => new HashSet<ZString> { invoiceLine.AdditionalCode };
			protected virtual ZString GetConcessionOrder(T invoiceLine) => invoiceLine.JI_ConcessionOrder;
		}

		public virtual ConditionChecker.EvaluateConditionValue EvaluateConditionValue => null;
		public virtual ConditionChecker.GetFriendlyConditionValue GetFriendlyConditionValue => null;
		public virtual IUniversalRateCalcData CalcDataForConditionFormula => new ConditionCalcDataForInvoiceLine(this);

		public IZZConditionSelectionCriteria[] ConditionSelectionCriterias => (conditionSelectionCriterias ?? (conditionSelectionCriterias = new CachedProperty<IZZConditionSelectionCriteria[]>(Factory, GetConditionSelectionCriterias))).Value;
		CachedProperty<IZZConditionSelectionCriteria[]> conditionSelectionCriterias;

		protected virtual IZZConditionSelectionCriteria[] GetConditionSelectionCriterias() => new[] { new ZZConditionSelectionCriteria<BaseJobComInvoiceLine>(this) };

		protected internal virtual bool UseUniversalConditionCheck => false;

		public class ZZConditionSelectionCriteria<T> : IZZConditionSelectionCriteria where T : BaseJobComInvoiceLine
		{
			public ZZConditionSelectionCriteria(T invoiceLine)
			{
				EffectiveDate = invoiceLine.EffectiveAssessmentDate;
				TradeGroupCountry = GetTradeGroupCountry(invoiceLine);
				PrimaryPreference = invoiceLine.JI_PrimaryPreference;
				ConcessionOrder = invoiceLine.JI_ConcessionOrder;
				DataGrouping = invoiceLine.Declaration?.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff) ?? ZString.Empty;
				Direction = GetDirection(invoiceLine);
				AdditionalCodes = GetAdditionalCodes(invoiceLine);
				SecondTradeGroups = GetSecondTradeGroups(invoiceLine);
			}

			public ZZConditionSelectionCriteria(T invoiceLine, ZString conditionClass, ZString conditionType)
				: this(invoiceLine)
			{
				EffectiveDate = invoiceLine.EffectiveAssessmentDate;
				TradeGroupCountry = GetTradeGroupCountry(invoiceLine);
				PrimaryPreference = invoiceLine.JI_PrimaryPreference;
				ConcessionOrder = invoiceLine.JI_ConcessionOrder;
				DataGrouping = invoiceLine.Declaration?.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff) ?? ZString.Empty;
				Direction = GetDirection(invoiceLine);
				AdditionalCodes = GetAdditionalCodes(invoiceLine);
				SecondTradeGroups = GetSecondTradeGroups(invoiceLine);
				ConditionClass = conditionClass;
				ConditionType = conditionType;
			}

			public ZDateTime EffectiveDate { get; protected set; }
			public ZString TradeGroupCountry { get; protected set; }
			public ZString PrimaryPreference { get; protected set; }
			public ISet<ZString> AdditionalCodes { get; protected set; }
			public ZString ConcessionOrder { get; protected set; }
			public ZString DataGrouping { get; protected set; }
			public ConditionChecker.ConditionDirection Direction { get; protected set; }
			public ZString ConditionClass { get; protected set; }
			public ZString ConditionType { get; protected set; }
			public ISet<ZString> SecondTradeGroups { get; protected set; }

			protected virtual ZString GetTradeGroupCountry(T invoiceLine) => invoiceLine.EffectiveCountryOfOrigin;
			protected virtual ISet<ZString> GetAdditionalCodes(T invoiceLine) => new HashSet<ZString> { invoiceLine.AdditionalCode };
			protected virtual ISet<ZString> GetSecondTradeGroups(T invoiceLine) => new HashSet<ZString>();
			protected virtual ConditionChecker.ConditionDirection GetDirection(T invoiceLine) => invoiceLine.IsImport ? ConditionChecker.ConditionDirection.Import : invoiceLine.IsExport ? ConditionChecker.ConditionDirection.Export : ConditionChecker.ConditionDirection.Either;
		}

		public IVATSelectionCriteria VATSelectionCriteria => (vatSelectionCriteria ?? (vatSelectionCriteria = new CachedProperty<IVATSelectionCriteria>(Factory, GetVATSelectionCriteriaCore, this, null))).Value;
		CachedProperty<IVATSelectionCriteria> vatSelectionCriteria;

		protected virtual IVATSelectionCriteria GetVATSelectionCriteriaCore() => new ZZVATSelectionCriteria<BaseJobComInvoiceLine>(this);

		public class ZZVATSelectionCriteria<T> : IVATSelectionCriteria where T : BaseJobComInvoiceLine
		{
			public ZZVATSelectionCriteria(T invoiceLine)
			{
				EffectiveDate = invoiceLine.EffectiveAssessmentDate;
				DataGrouping = invoiceLine.Declaration?.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff) ?? ZString.Empty;
				TaxOrFeeCode = invoiceLine.JI_ZZF_NKTaxType;
				AdditionalCodes = GetAdditionalCodes(invoiceLine);
				TradeGroups = GetTradeGroups(invoiceLine);
			}

			public ZDateTime EffectiveDate { get; protected set; }
			public ZString DataGrouping { get; protected set; }
			public ZString TaxOrFeeCode { get; protected set; }
			public ISet<ZString> AdditionalCodes { get; protected set; }
			public ISet<ZString> TradeGroups { get; protected set; }

			protected virtual ISet<ZString> GetAdditionalCodes(T invoiceLine) => new HashSet<ZString> { invoiceLine.AdditionalCode };
			protected virtual ISet<ZString> GetTradeGroups(T invoiceLine) => new HashSet<ZString>();
		}

		public int GetChildPackageUsage(BasePackage parentPackage)
		{
			if (parentPackage == null)
			{
				return default;
			}
			var subPackagePKs = parentPackage.Children?.Select(x => x.PK);
			var subPackageUsage = PackagesPivot?.Where(x => subPackagePKs != null && subPackagePKs.Contains(x.Package.PK)).Sum(x => x.CHC_NumberOfPacks) ?? 0;
			return subPackageUsage;
		}

		public void SyncParentPivotPackNum(BasePackage parentPackage)
		{
			var linePivot = GetPivotByPackage(parentPackage);
			if (null != linePivot)
			{
				linePivot.CHC_NumberOfPacks = new ZInt(GetChildPackageUsage(parentPackage));
			}
			else
			{
				InvoiceHeader?.SyncParentPivotPackNum(parentPackage);
			}
		}

		public AutoCusHouseContPackInvoiceLinePivot GetPivotByPackage(BasePackage package)
		{
			return PackagesPivot?.Cast<AutoCusHouseContPackInvoiceLinePivot>().FirstOrDefault(x => package != null && x.CHC_CW == package.PK);
		}

		public ITariffAdditionalCodeSelectionCriteria TariffAdditionalCodeSelectionCriteria => (tariffAdditionalCodeSelectionCriteria ?? (tariffAdditionalCodeSelectionCriteria = new CachedProperty<ITariffAdditionalCodeSelectionCriteria>(Factory, GetTariffAdditionalCodeSelectionCriteriaCore, this, null))).Value;
		CachedProperty<ITariffAdditionalCodeSelectionCriteria> tariffAdditionalCodeSelectionCriteria;

		protected virtual ITariffAdditionalCodeSelectionCriteria GetTariffAdditionalCodeSelectionCriteriaCore() => null;

		#region IInvoiceLinePartClassificationTariffDescriptionSyncroniser

		ZPropertyInfo IInvoiceLinePartClassificationTariffDescriptionSyncroniser.DescriptionInfo => base.JI_DescriptionInfo;

		ZString IInvoiceLinePartClassificationTariffDescriptionSyncroniser.Description
		{
			get { return JI_Description; }
			set { JI_Description = value; }
		}

		ZString IInvoiceLinePartClassificationTariffDescriptionSyncroniser.ExtraInfoForClassification
		{
			get { return JI_ExtraInfoForClassification; }
			set { JI_ExtraInfoForClassification = value; }
		}
		#endregion

		#region IBaseInvoiceLine Members
		IBaseInvoiceHeader IBaseInvoiceLine.InvoiceHeader => InvoiceHeader;
		ZString IBaseInvoiceLine.InvoiceNumber => InvoiceHeader?.JZ_InvoiceNumber ?? ZString.Empty;

		#region JI_CountryOfOrigin

		[RelatedBusinessObject("CountryOfOrigin")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CountryOfOrigins))]
		public override ZString JI_CountryOfOrigin
		{
			get { return base.JI_CountryOfOrigin; }
			set
			{
				value = value.ToUpperInvariant();
				base.JI_CountryOfOrigin = value;
			}
		}

		public RefCountry CountryOfOrigin
		{
			get { return Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, JI_CountryOfOrigin); }
		}

		public IRefCountry CountryOfOriginFallback => CountryOfOriginFallbackCore;
		protected virtual IRefCountry CountryOfOriginFallbackCore => CountryOfOrigin != null ? new DataTransferCountryInfo(CountryOfOrigin.RN_Code, CountryOfOrigin.RN_Desc) : null;

		#endregion

		#endregion

		#region IClusterKeyWorker

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)JI_ClusterKeyInfo;
		Type IClusterKeyWorker.ParentBizObjType => typeof(BaseJobComInvoiceHeader);
		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)JI_JZInfo;

		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList
		{
			get
			{
				yield return new ClusterKeyChildInfo(typeof(JobComInvLineRefs), JobComInvLineRefsSchema.JG_JI);
				yield return new ClusterKeyChildInfo(typeof(JobComInvoiceLineTax), JobComInvoiceLineTaxSchema.JLT_JI);
				yield return new ClusterKeyChildInfo(ObjectFactory.GetType<Integration.Customs.AU.IQuarantineExdocLine>(), QuarantineExDocLineSchema.QL_JI);
				yield return new ClusterKeyChildInfo(typeof(JobComInvLineComponentInventory), JobComInvLineComponentInventorySchema.JIV_JI);
				yield return new ClusterKeyChildInfo(ObjectFactory.GetType<Integration.Customs.US.IJobUSComInvoiceLine>(), JobUSComInvoiceLineSchema.USI_JI);
				yield return new ClusterKeyChildInfo(ObjectFactory.GetType<Integration.Customs.EU.ICusAuthorizationUsage>(), CusAuthorizationUsageSchema.AGC_ParentID);
				yield return new ClusterKeyChildInfo(ObjectFactory.GetType<Integration.Customs.TW.IJobTWComInvoiceLine>(), JobTWComInvoiceLineSchema.TWL_JI);
				yield return new ClusterKeyChildInfo(typeof(CusVehicle), CusVehicleSchema.CVH_ParentID);
				yield return new ClusterKeyChildInfo(typeof(CusEngine), CusEngineSchema.CEG_ParentID);
			}
		}

		#region ITariffViewFilterData

		ZBool IsImportTariffViewFilterData => IsImport;

		protected virtual ZBool IsExportTariffViewFilterData => IsExport || (Declaration?.IsMiscellaneous ?? false);

		public virtual ITariffViewFilterData TariffViewFilterData
		{
			get
			{
				var ratesApplyToCountry = GetRatesApplyToCountry();
				return new TariffViewFilterData(ratesApplyToCountry, EffectiveDateForDutyRate);
			}
		}

		protected ZString GetRatesApplyToCountry()
		{
			return this switch
			{
				_ when IsImportTariffViewFilterData => JI_CountryOfOrigin,
				_ when IsExportTariffViewFilterData => Declaration?.FinalDestination?.RL_RN_NKCountryCode ?? ZString.Empty,
				_ => ZString.Empty
			};
		}

		#endregion

		#endregion

		#region IAddInfoChildSupporter Members

		BusinessObject IAddInfoChildSupporter.AddInfoChild => GetAddInfoChild();
		protected virtual BusinessObject GetAddInfoChild() => null;

		SchemaGuidColumn IAddInfoChildSupporter.ChildForeignKeyColumn => GetChildForeignKeyColumn();
		protected virtual SchemaGuidColumn GetChildForeignKeyColumn() => null;

		void IAddInfoChildSupporter.RegisterListChangedCalledRefreshBinding(IBindingList element) => RegisterListChangedCalledRefreshBinding(element);
		void IAddInfoChildSupporter.UnRegisterListChangedCalledRefreshBinding(IBindingList element) => UnRegisterListChangedCalledRefreshBinding(element);
		#endregion

		#region PackingList

		public CusPackableItem CreateNewCusPackableItem()
		{
			var item = Factory.New<CusPackableItem>();
			using (item.GetValidationSuspender())
			{
				item.CUI_JI = PK;
				SetDefaultValuesForNewPackableItem(item);
			}

			return item;
		}

		public virtual void SetDefaultValuesForNewPackableItem(CusPackableItem newItem)
		{
			newItem.CUI_GoodsDescription = GetPackableItemGoodsDescription();
			newItem.CUI_PackableQty = JI_InvoiceQuantity;
			newItem.CUI_PackableUQ = JI_InvoiceUQ;
			newItem.CUI_NetWeight = JI_NetWeight;
			newItem.CUI_NetWeightUQ = JI_NetWeightUQ;
		}

		public virtual ZString GetPackableItemGoodsDescription()
		{
			return JI_Description;
		}

		public virtual CodeDescriptionPairList GetPackableItemUQList()
		{
			return Lookups.CustomsUQList;
		}

		#endregion

		protected virtual bool IncludedInUniversalXMLCore => true;

		public bool IncludedInUniversalXML => IncludedInUniversalXMLCore;

		protected virtual bool IncludeEntryDetailsInUniversalXMLCore => true;

		public bool IncludeEntryDetailsInUniversalXML => IncludeEntryDetailsInUniversalXMLCore;

		#region ITypeDeciderContext Members

		string ITypeDeciderContext.Country => CountryCode;
		public ZString CountryCode => (InvoiceHeader as ITypeDeciderContext)?.Country ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		#endregion

		#region IDataModelSupporter

		public void PopulateDataModelIfNeeded() => this.PopulateDataModelFromParentIfNeeded(InvoiceHeader);

		ZString IDataModelSupporter.DataModel { get => JI_DataModel; set => JI_DataModel = value; }

		#endregion
	}
}
