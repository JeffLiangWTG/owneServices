using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.LandedCosting.Business
{
	[SingleObjectAroundARow]
	[CodeProperty(LandedCostHeader.Schema.ReferenceNumber), DescriptionProperty(LandedCostHeader.Schema.ReferenceNumber)]
	public class LandedCostHeader : AutoLandedCostHeader,
		Integration.LandedCosting.ILandedCostHeader,
		ILandedCostHistoryMaster,
		ICustomLabelsConfigOrgProvider,
		IDocumentSupportable,
		IDocManagerSupport,
		IDocsAndCartageParent,
		IClusterKeyMaster,
		IClusterKeyWorker
	{
		public new class Schema : AutoLandedCostHeader.Schema
		{
			public const string ReferenceNumber = "ReferenceNumber";
		}

		public LandedCostHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static LandedCostHeader New(ILandedCostHeader lCHost)
		{
			LandedCostHeader lCHeader = (LandedCostHeader)lCHost.Factory.New(typeof(LandedCostHeader));
			lCHeader.DefaultFromHost(lCHost);

			return lCHeader;
		}

		public ZString ReferenceNumber
		{
			get { return Parent != null ? Parent.JobNumber : ZString.Empty; }
		}

		#region Methods for defaulting & synchronisation

		public void DefaultFromHost(ILandedCostHeader hostEntity)
		{
			using (GetValidationSuspender())
			using (SuspendSettingHasChanges())
			{
				LT_ParentID = hostEntity.PK;
				LT_ParentTableCode = hostEntity.TableCode;
				LT_GC = hostEntity.CompanyPK;
				DefaultFromHost();
			}
		}

		public void DefaultFromHost()
		{
			using (GetValidationSuspender())
			using (SuspendSettingHasChanges())
			{
				if (Parent != null)
				{
					LT_DateOfEntry = Parent.DateOfEntry;
					LT_LandedCostType = Parent.LandedCostType;
					LT_DefaultEstimatedDutyRate = Parent.DefaultEstimatedDutyPercent;
				}
			}
		}

		public void SynchroniseAll()
		{
			using (GetValidationSuspender())
			using (SuspendSettingHasChanges())
			{
				if (Parent != null)
				{
					DefaultFromHost();
					ExchangeRates.LoadFromLCHeaderHost();

					if (LT_LandedCostType == LandedCostType.Estimated)
					{
						Autorate(Parent);
					}

					foreach (ILandedCostChargeHolder chargeHolder in Parent.ChargeHolders)
					{
						foreach (IDefaultLandedCostInput chargesToImport in chargeHolder.ChargesToImportForLandedCosting)
						{
							LandCostInput costInput = CostInputs.AddNew();
							costInput.DefaultFromHost(chargeHolder, chargesToImport);
						}
					}
				}
			}
		}

		public void SynchroniseAll(ILandedCostHeader hostEntity)
		{
			using (GetValidationSuspender())
			using (SuspendSettingHasChanges())
			{
				DefaultFromHost(hostEntity);
				SynchroniseAll();
			}
		}

		public void ResetToOriginal()
		{
			LT_DateOfProcessing = ZDateTime.Empty;
			Histories.DeleteAll();
		}

		void Autorate(ILandedCostHeader target)
		{
			var objectToAutorate = target as IBusiness;
			if (target == null)
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The argument should be of type {0}", nameof(IBusiness)), nameof(target));
			}

			var autoratingStarter = new AutoRatingStarter(objectToAutorate, null, autoratingStrategy: new LandedCostAutoRatingStrategy(this, target));
			autoratingStarter.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
		}

		#endregion

		#region Calculated

		public bool IsEstimatedLC
		{
			get { return LT_LandedCostType == Enterprise.MasterFiles.Business.LandedCostType.Estimated; }
		}

		public bool IsReciprocalExRate
		{
			get
			{
				var declaration = this.Parent as Integration.Customs.IBaseJobDeclaration;
				return declaration?.IsReciprocalRates ?? this.Company.GC_IsReciprocal;
			}
		}

		public ZString LocalCurrencyCode
		{
			get
			{
				var declaration = this.Parent as Integration.Customs.IBaseJobDeclaration;
				return declaration?.LocalCurrencyCode ?? this.Company.GC_RX_NKLocalCurrency;
			}
		}

		public ZString UniqueReferenceNumber
		{
			get { return Parent == null ? ZString.Empty : Parent.UniqueReferenceNumber; }
		}

		public ZDecimal TotalCost
		{
			get { return TotalInvoiceCost + TotalCustomsDisbursementCharges + TotalLandingCost; }
		}

		public ZDecimal TotalCustomsDisbursementCharges
		{
			get
			{
				return CustomsChargeLCItemSettings.Sum(x => GetTotalCustomsDisbursementChargeAmount(x.CostType));
			}
		}

		public ZDecimal GetTotalCustomsDisbursementChargeAmount(ZString costType)
		{
			return Parent?.TotalDutyTaxEntryFeeItems[costType] ?? ZDecimal.Zero;
		}

		public ZDecimal TotalInvoiceCost
		{
			get { return Histories.TotalInvoiceCost; }
		}

		public ZDecimal TotalLinePrice
		{
			get { return Histories.TotalLinePrice; }
		}

		public ZDecimal TotalCostWithMarkup1Applied
		{
			get { return Histories.TotalCostWithMarkup1Applied; }
		}

		public ZString MultipleInvoices
		{
			get { return Parent.HasMultiInvoices ? "Y" : "N"; }
		}

		public ZDecimal TotalLandingCost
		{
			get { return CostInputs.TotalLandingCost; }
		}

		public ZDecimal TotalGroup1
		{
			get { return CostInputs.TotalGroup1; }
		}

		public ZDecimal TotalGroup2
		{
			get { return CostInputs.TotalGroup2; }
		}

		public ZDecimal TotalGroup3
		{
			get { return CostInputs.TotalGroup3; }
		}

		public ZDecimal TotalGroup4
		{
			get { return CostInputs.TotalGroup4; }
		}

		public ZDecimal TotalGroup5
		{
			get { return CostInputs.TotalGroup5; }
		}

		public ZDecimal TotalGroup6
		{
			get { return CostInputs.TotalGroup6; }
		}

		public ZDecimal TotalGroupMisc
		{
			get { return CostInputs.TotalGroupMisc; }
		}

		public OrgHeader Consignee
		{
			get { return Parent.Consignee; }
		}

		#endregion

		#region Labels for Document Wrappers

		public int LocalCurrencyDecimals => Company.LocalCurrency.Decimals;

		public
#if DEBUG
			virtual
#endif
			ZString LandedCostGroup1Label => GetGroupNameUsingGroupID(1);

		public
#if DEBUG
			virtual
#endif
			ZString LandedCostGroup2Label => GetGroupNameUsingGroupID(2);

		public
#if DEBUG
			virtual
#endif
			ZString LandedCostGroup3Label => GetGroupNameUsingGroupID(3);

		public
#if DEBUG
			virtual
#endif
			ZString LandedCostGroup4Label => GetGroupNameUsingGroupID(4);

		public
#if DEBUG
			virtual
#endif
			ZString LandedCostGroup5Label => GetGroupNameUsingGroupID(5);

		public
#if DEBUG
			virtual
#endif
			ZString LandedCostGroup6Label => GetGroupNameUsingGroupID(6);

		public ZString LandedCostGroupMiscLabel => Res.GetString("c412f8ca-2b43-4754-96b5-4bdd8871e86b", "Misc Charges");

		public
#if DEBUG
			virtual
#endif
			ZString LandedCostPercentageLabel => LandedCostPercentageCoreLabel;

		public ZString LandedTotalCostPercentageLabel => Res.GetString("BB184FD5-0750-46C8-AD79-9B516B5DFB37", "Total Cost %");

		public ZString Disclaimer => DocumentEngineCore.Registry.DocumentsDataRegistry.Instance.LandedCostingClosingText.GetFallBackValueAtAllLevels(LT_GC.ToGuid(), Guid.Empty, GlbDepartment.CurrentDepartment.PK.ToGuid());

		#endregion

		#region For xml Export

		public ZString LandedCostGroup1CostDistributionCode
		{
			get { return GetGroupCostDistributionCodeFromGroupID(1); }
		}

		public ZString LandedCostGroup2CostDistributionCode
		{
			get { return GetGroupCostDistributionCodeFromGroupID(2); }
		}

		public ZString LandedCostGroup3CostDistributionCode
		{
			get { return GetGroupCostDistributionCodeFromGroupID(3); }
		}

		public ZString LandedCostGroup4CostDistributionCode
		{
			get { return GetGroupCostDistributionCodeFromGroupID(4); }
		}

		public ZString LandedCostGroup5CostDistributionCode
		{
			get { return GetGroupCostDistributionCodeFromGroupID(5); }
		}

		public ZString LandedCostGroup6CostDistributionCode
		{
			get { return GetGroupCostDistributionCodeFromGroupID(6); }
		}

		#endregion

		#region Properties Overrides

		[ReadOnly(true)]
		public override ZDate LT_DateOfEntry
		{
			get { return base.LT_DateOfEntry; }
			set { base.LT_DateOfEntry = value; }
		}

		[ReadOnly(true)]
		public override ZDateTime LT_DateOfProcessing
		{
			get { return base.LT_DateOfProcessing; }
			set { base.LT_DateOfProcessing = value; }
		}

		public override ZGuid LT_ParentID
		{
			get { return base.LT_ParentID; }
			set
			{
				bool hasChanged = base.LT_ParentID != value;
				base.LT_ParentID = value;
				if (hasChanged)
				{
					NeedToRefreshParent = true;
					CostInputs.MarkAsNeedingValidation();
					Histories.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString LT_ParentTableCode
		{
			get { return base.LT_ParentTableCode; }
			set
			{
				bool hasChanged = base.LT_ParentTableCode != value;
				base.LT_ParentTableCode = value;
				if (hasChanged)
				{
					NeedToRefreshParent = true;
					CostInputs.MarkAsNeedingValidation();
					Histories.MarkAsNeedingValidation();
				}
			}
		}

		public override ZGuid LT_GC
		{
			get { return base.LT_GC; }
			set
			{
				var oldCountryCode = CountryCode;
				base.LT_GC = value;
				if (oldCountryCode != CountryCode)
				{
					customsChargeLCItemSettings = null;
				}
			}
		}

		#endregion

		#region Methods overrides

		public override void Delete()
		{
			Histories.DeleteAll();
			CostInputs.RemoveAndDeleteAll();
			base.Delete();
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			LT_GC = GlbCompany.CurrentCompany.PK;
		}

		#endregion

		#region Related Objects

		public TypeLoaderCollection ParentLoaderForLandedCostInputDistributeTo => parentLoaderForLandedCostInputDistributeTo ??=
			GetParentLoaderForLandedCostInputDistributeTo();
		TypeLoaderCollection parentLoaderForLandedCostInputDistributeTo;

		protected virtual TypeLoaderCollection GetParentLoaderForLandedCostInputDistributeTo()
		{
			var result = new TypeLoaderCollection();

			if (LT_ParentTableCode == JobDeclarationSchema.Constants.Prefix)
			{
				result.Add(ObjectFactory.GetType<Integration.Customs.IBaseJobComInvoiceLine>());
				result.Add(ObjectFactory.GetType<Integration.Customs.Shared.IBaseCusContainer>());
				result.Add(ObjectFactory.GetType<Integration.Customs.Shared.ICommonJobComInvoiceHeader>());
			}
			else if (LT_ParentTableCode == JobOrderHeaderSchema.Constants.Prefix)
			{
				result.Add(ObjectFactory.GetType<Integration.Forwarding.IOrder>());
				result.Add(ObjectFactory.GetType<Freight.Integration.Forwarding.IOrderLine>());
			}

#if DEBUG
			result.Add(typeof(Testing.DummyLandedCostDistributeTo));
#endif
			return result;
		}

		public ILandedCostHeader Parent
		{
			get
			{
				if (fParent == null || NeedToRefreshParent)
				{
					fParent = (ILandedCostHeader)ParentLoaders.LoadBusinessObject(Factory, LT_ParentTableCode, LT_ParentID);
					NeedToRefreshParent = false;
				}
				return fParent;
			}
		}

		public TypeLoaderCollection ParentLoaders => fParentLoaders ??= GetParentLoaders();
		TypeLoaderCollection fParentLoaders;

		protected virtual TypeLoaderCollection GetParentLoaders()
		{
			var loaders = new TypeLoaderCollection(
				ObjectFactory.GetType<Integration.Customs.IBaseJobDeclaration>(),
				ObjectFactory.GetType<Integration.Forwarding.IOrder>()
			);
#if DEBUG
			loaders.Add(new TypeLoader(typeof(Testing.DummyLandedCostHeader)));
#endif
			return loaders;
		}

		public IEnumerable<ICustomsChargeLCItemSetting> CustomsChargeLCItemSettings
		{
			get
			{
				var isDeclarationIntegrated = (Parent as Integration.Customs.IBaseJobDeclaration)?.IsDeclarationIntegrated;
				if (customsChargeLCItemSettings == null || isDeclarationIntegrated != previousIsDeclarationIntegrated)
				{
					previousIsDeclarationIntegrated = isDeclarationIntegrated;
					var company = Company ?? GlbCompany.CurrentCompany;
					customsChargeLCItemSettings = ObjectFactory.Get<ICustomsChargeLCItemSettingsProvider>("ICustomsChargeLCItemSettingsProvider").GetCustomsChargeLCItemSettings(company.GC_RN_NKCountryCode, isDeclarationIntegrated, company.PK);
					if (customsChargeLCItemSettings == null)
					{
						customsChargeLCItemSettings = Array.Empty<ICustomsChargeLCItemSetting>();
					}
				}
				return customsChargeLCItemSettings;
			}
		}
		IEnumerable<ICustomsChargeLCItemSetting> customsChargeLCItemSettings;
		bool? previousIsDeclarationIntegrated;

		public ZString CountryCode
		{
			get { return Company != null ? Company.GC_RN_NKCountryCode : GlbCompany.CurrentCompany.GC_RN_NKCountryCode; }
		}

		[ChildEditable(true)]
		public LandedCostHistoryCollection Histories
		{
			get
			{
				if (fHistories == null)
				{
					fHistories = GetLCHistoryCollection();
					RegisterEditableChildObject(fHistories);
				}
				return fHistories;
			}
		}
		LandedCostHistoryCollection fHistories;

		protected virtual LandedCostHistoryCollection GetLCHistoryCollection()
		{
			return new LandedCostHistoryCollection(this);
		}

		[ChildEditable(true)]
		public LandCostInputCollection CostInputs
		{
			get
			{
				if (fCostInputs == null)
				{
					fCostInputs = new LandCostInputCollection(this);
					fCostInputs.Load();
					RegisterEditableChildObject(fCostInputs);
				}
				return fCostInputs;
			}
		}
		LandCostInputCollection fCostInputs;

		public LCPreferenceFallBackCalculator PreferenceCalculator
		{
			get
			{
				if (fPreferenceCalculator == null && Parent != null)
				{
					fPreferenceCalculator = new LCPreferenceFallBackCalculator(Parent.Consignee);
				}
				return fPreferenceCalculator;
			}
		}
		LCPreferenceFallBackCalculator fPreferenceCalculator;

		[ChildEditable(true)]
		public LandedCostingExRateCollection ExchangeRates
		{
			get
			{
				if (fExRates == null)
				{
					fExRates = new LandedCostingExRateCollection(Parent);
					fExRates.LoadFromLCHeaderHost();
					RegisterEditableChildObject(fExRates);

					Parent.OnExchangeRateHolderDeleted += HostEntity_OnExchangeRateHolderDeleted;
				}
				return fExRates;
			}
		}
		LandedCostingExRateCollection fExRates;

		void HostEntity_OnExchangeRateHolderDeleted(object sender, EventArgs e)
		{
			ExchangeRates.LoadFromLCHeaderHost();
		}

		public RoundingHelper RoundingHelper
		{
			get
			{
				if (fRoundingHelper == null)
				{
					fRoundingHelper = CreateRoundingHelper();
				}
				return fRoundingHelper;
			}
		}
		RoundingHelper fRoundingHelper;

		protected virtual RoundingHelper CreateRoundingHelper()
		{
			return new RoundingHelper();
		}

		public LandedCostingCustomsFeeCollection CustomsFees
		{
			get
			{
				if (customsFees == null)
				{
					customsFees = new LandedCostingCustomsFeeCollection(Factory);
					customsFees.LoadCollection(Histories);
				}
				return customsFees;
			}
		}
		LandedCostingCustomsFeeCollection customsFees;

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region ILandedCostHistoryMaster Members

		ZGuid ILandedCostHistoryMaster.PK
		{
			get { return PK; }
		}

		SchemaGuidColumn ILandedCostHistoryMaster.FKSchemaColumnInLandedCostHistory
		{
			get { return LandedCostHistorySchema.LH_LT; }
		}

		ZBool ILandedCostHistoryMaster.ShouldMarginPercentagesReadOnly
		{
			get { return false; }
		}

		ZString ILandedCostHistoryMaster.CountryCode
		{
			get { return CountryCode; }
		}

		IEnumerable<ICustomsChargeLCItemSetting> ILandedCostHistoryMaster.CustomsChargeLCItemSettings
		{
			get { return CustomsChargeLCItemSettings; }
		}

		#endregion

		#region ICustomLabelsConfigOrgProvider Members

		event EventHandler ICustomLabelsConfigOrgProvider.ConfigOrgChanged
		{
			add { }
			remove { }
		}

		OrgHeader ICustomLabelsConfigOrgProvider.ConfigOrg
		{
			get { return Parent.Consignee; }
		}

		#endregion

		#region IDocumentSupportable

		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get { return new LandedCostingDocumentDeclarationSupporter(this); }
		}

		#endregion

		#region Implementation

		ILandedCostHeader fParent;
		bool NeedToRefreshParent = true;

		protected virtual ZString LandedCostPercentageCoreLabel
		{
			get { return Res.GetString("D0F64AC2-7996-401B-A704-3A60CCED0894", "Total Import Cost %"); }
		}

		protected ZString GetGroupNameUsingGroupID(ZByte groupID)
		{
			string result = Res.GetString("749f069f-efc1-4680-b3e8-c70bf566eefe", "Misc");
			if (groupID < 7 && PreferenceCalculator != null)
			{
				result = PreferenceCalculator.GetLandedCostingGroupNameFromID(groupID);
			}
			return result;
		}

		ZString GetGroupCostDistributionCodeFromGroupID(ZByte groupID)
		{
			string result = "";
			if (groupID < 7 && PreferenceCalculator != null)
			{
				result = PreferenceCalculator.GetLandedCostingGroupDistributionCodeFromID(groupID);
			}
			return result;
		}

		#endregion

		#region IClusterKeyMaster & IClusterKeyWorker & IOptionalClusterKeyEntity

		public sealed override ZInt LT_ClusterKey
		{
			get => base.LT_ClusterKey;
			set
			{
				if (base.LT_ClusterKey != value)
				{
					this.CheckCanSetMasterClusterKey();
					base.LT_ClusterKey = value;
				}
			}
		}

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)LT_ClusterKeyInfo;

		Type IClusterKeyWorker.ParentBizObjType
		{
			get
			{
				return (string)LT_ParentTableCode switch
				{
					JobDeclarationSchema.Constants.Prefix => ObjectFactory.GetType<Integration.Customs.IBaseJobDeclaration>(),
					_ => null,
				};
			}
		}

		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty
		{
			get
			{
				return (string)LT_ParentTableCode switch
				{
					JobDeclarationSchema.Constants.Prefix => (ZPropertyInfoGuid)LT_ParentIDInfo,
					_ => null,
				};
			}
		}

		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList
		{
			get
			{
				yield return new ClusterKeyChildInfo(typeof(LandCostInput), LandCostInputSchema.LI_LT);
				yield return new ClusterKeyChildInfo(typeof(LandedCostHistory), LandedCostHistorySchema.LH_LT);
			}
		}

		#endregion

		IHaveRequiredDocuments IDocsAndCartageParent.RequiredDocumentsProvider
		{
			get { return Parent.RequiredDocumentsProvider; }
		}

		Type IDocsAndCartageParent.DocsAndCartageParentType
		{
			get { return Parent.DocsAndCartageParentType; }
		}

		Type IDocsAndCartageParent.DocsAndCartageType
		{
			get { return Parent.DocsAndCartageType; }
		}

		public DocManagerInfo DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.LandedCostHeader);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			var jobDeclaration = (BusinessObject)Factory.New<Integration.Customs.IBaseJobDeclaration>();
			LT_ParentID = jobDeclaration.PK;
			LT_ParentTableCode = jobDeclaration.TablePrefix;
		}

#endif
	}
}
