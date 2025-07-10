using System;
using System.Threading.Tasks;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.Business
{
	public class LinkedModuleComplianceCommodityDetail : NonPersistentBusinessObject, ILinkedModuleComplianceCommodityDetail
	{
		public LinkedModuleComplianceCommodityDetail(BaseJobComInvoiceLine parent, BaseJobDeclaration jobDeclaration) : base(parent.Factory)
		{
			Parent = parent;
			Declaration = jobDeclaration;
			CommodityRiskStatusProvider = jobDeclaration;
		}

		public void InitializeIfNeeded()
		{
			InitializeIfNeededCore(null, false);
		}

		public void BatchInitialize(ComplianceResultFromCpw? commodityInfo)
		{
			InitializeIfNeededCore(commodityInfo, true);
		}

		void InitializeIfNeededCore(ComplianceResultFromCpw? commodityInfo, bool batchInitialise)
		{
			if (!initialized
				&& CommodityRiskStatusProvider.IsEnabledComplianceWise
				&& CommodityRiskStatusProvider is ISupportInteractionWithComplianceWiseCommodities supportInteractionWithCommodities
				&& supportInteractionWithCommodities.Helper != null)
			{
				SupportInteractionWithCommodities = supportInteractionWithCommodities;

				if (batchInitialise)
				{
					SetCommodityInfoFromCPW(commodityInfo);
				}
				else
				{
					SetCommodityInfoFromCPW();
				}

				Parent.JI_TariffInfo.ValueChanged -= Commodity_ValueChanged;
				Parent.JI_CountryOfOriginInfo.ValueChanged -= Commodity_ValueChanged;
				Parent.JI_DescriptionInfo.ValueChanged -= Commodity_ValueChanged;
				Parent.JI_NDescriptionInfo.ValueChanged -= Commodity_ValueChanged;

				RiskStatusDescriptionInfo.ValueChanged -= CommodityRisk_AssessmentChanged;
				AssessmentNotesInfo.ValueChanged -= CommodityRisk_AssessmentChanged;

				Parent.JI_TariffInfo.ValueChanged += Commodity_ValueChanged;
				Parent.JI_CountryOfOriginInfo.ValueChanged += Commodity_ValueChanged;
				Parent.JI_DescriptionInfo.ValueChanged += Commodity_ValueChanged;
				Parent.JI_NDescriptionInfo.ValueChanged += Commodity_ValueChanged;

				RiskStatusDescriptionInfo.ValueChanged += CommodityRisk_AssessmentChanged;
				AssessmentNotesInfo.ValueChanged += CommodityRisk_AssessmentChanged;

				initialized = true;
			}
		}

		bool initialized;

		void SetCommodityInfoFromCPW()
		{
			var commodityInfo = SupportInteractionWithCommodities.Helper.SourceSideCommodities?.GetCommodityStatusFromCpw?.Invoke(new ComplianceCommodityFromSource
			{
				HarmonizedCode = Parent.JI_TariffForComplianceWise,
				GroupingOrCountry = WorldCustomsOrganisationWCO,
				GoodsDescription = Parent.JI_Description.IsEmpty ? Parent.JI_NDescription : Parent.JI_Description,
				OriginOfGoods = Parent.JI_CountryOfOrigin
			});

			SetCommodityInfoFromCPW(commodityInfo);

			AssessmentNotesInfo.RefreshBinding();
			RiskStatusDescriptionInfo.RefreshBinding();
		}

		[ThreadStatic]
		static bool suspendChanges;

		class SuspendChanges : IDisposable
		{
			public SuspendChanges()
			{
				suspendChanges = true;
			}

			public void Dispose()
			{
				suspendChanges = false;
			}
		}

		void SetCommodityInfoFromCPW(ComplianceResultFromCpw? commodityInfo)
		{
			using (new SuspendChanges())
			{
				if (commodityInfo.HasValue)
				{
					AssessmentInitialized = commodityInfo.Value.AssessmentInitialized;
					CommodityExists = true;

					assessmentNotes = commodityInfo.Value.RiskNotes;
					riskStatus = commodityInfo.Value.RiskStatus;
					HarmonizedBorderWiseTextual = commodityInfo.Value.HarmonizedBorderWiseTextual;

					ImportAlertStatus = commodityInfo.Value.ImportAlertStatus;
				}
				else
				{
					CommodityExists = false;

					assessmentNotes = ZString.Empty;
					riskStatus = ZString.Empty;
					HarmonizedBorderWiseTextual = ZString.Empty;

					ImportAlertStatus = ZString.Empty;
				}
			}
		}

		public void SetCommodityInfo(ComplianceResultFromCpw commodityInfo)
		{
			SetCommodityInfoFromCPW(commodityInfo);
			AssessmentNotesInfo.RefreshBinding();
			RiskStatusDescriptionInfo.RefreshBinding();
		}

		void Commodity_ValueChanged(object sender, EventArgs e)
		{
			if (suspendChanges)
			{
				return;
			}

			ComplianceCommodity[] changedCommodities;

			if (Parent.JI_TariffForComplianceWise.IsEmpty)
			{
				changedCommodities = Array.Empty<ComplianceCommodity>();
			}
			else
			{
				changedCommodities = new[] { new ComplianceCommodity(Parent.JI_TariffForComplianceWise, WorldCustomsOrganisationWCO, Declaration.JE_DeclarationReference, CommodityRiskStatusProvider.ParentID, Parent.JI_CountryOfOrigin, Res.GetString("3612C1EC-ADD0-4DAD-B9DD-75E8F5AB06D1", "Tariff"), Parent.JI_Description.IsEmpty ? Parent.JI_NDescription : Parent.JI_Description) };
			}

			SupportInteractionWithCommodities.Helper.SourceSideCommodities.CommoditiesChanged?.Invoke(changedCommodities);

			SetCommodityInfoFromCPW();
		}

		void CommodityRisk_AssessmentChanged(object sender, EventArgs e)
		{
			if (suspendChanges)
			{
				return;
			}

			SupportInteractionWithCommodities.Helper.SourceSideCommodities.CommoditiesAssessmentChanged?.Invoke(new[]
			{
				new ComplianceCommodityFromSource
				{
					HarmonizedCode = Parent.JI_TariffForComplianceWise,
					GoodsDescription = Parent.JI_Description.IsEmpty ? Parent.JI_NDescription : Parent.JI_Description,
					OriginOfGoods = Parent.JI_CountryOfOrigin,
					GroupingOrCountry = WorldCustomsOrganisationWCO,
					RiskNotes = AssessmentNotes,
					RiskStatus = RiskStatus,
				}
			});
		}

		public virtual bool RiskStatusDescription_ReadOnly => FieldReadOnly;

		public virtual bool AssessmentNotes_ReadOnly => FieldReadOnly;

		public bool CommodityExists { get; set; }

		bool FieldReadOnly => Parent.ReadOnly || !(CommodityRiskStatusProvider?.EditComplianceAssessmentSecurity.IsAllowed ?? false) || !CommodityExists || Parent.JI_TariffForComplianceWise.IsEmpty || !AssessmentInitialized;

		[BusinessObjectTestExclude]
		[List(nameof(CommodityRiskStatusCodeList))]
		public ZString RiskStatusDescription
		{
			get
			{
				return CommodityExists ? (AssessmentInitialized ? CommodityRiskStatusCodeList.GetDescriptionFromCode(RiskStatus) : ComplianceRiskStatusCodeList.Descriptions.AssessmentNotInitialized.ToString()) : ZString.Empty;
			}
			set
			{
				var riskStatus = CommodityRiskStatusCodeList.GetCodeFromDescription(value);
				if (riskStatus != null)
				{
					RiskStatus = riskStatus;
				}

				RiskStatusDescriptionInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo RiskStatusDescriptionInfo => GetZPropertyInfo(nameof(RiskStatusDescription));

		ZString riskStatus;
		public ZString RiskStatus
		{
			get => riskStatus;
			set
			{
				if (riskStatus != value)
				{
					riskStatus = value;
					RiskStatusDescriptionInfo.RefreshBinding();
				}
			}
		}

		ZString importAlertStatus;

		[BusinessObjectTestExclude]
		public ZString ImportAlertStatus
		{
			get
			{
				return importAlertStatus;
			}
			set
			{
				importAlertStatus = value;
				ImportAlertStatusInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ImportAlertStatusInfo => GetZPropertyInfo(nameof(ImportAlertStatus));

		bool AssessmentInitialized { get; set; }

		ZString assessmentNotes;
		public ZString AssessmentNotes
		{
			get => assessmentNotes;
			set
			{
				if (value != assessmentNotes)
				{
					assessmentNotes = value;
					AssessmentNotesInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo AssessmentNotesInfo => GetZPropertyInfo(nameof(AssessmentNotes));

		ZString harmonizedBorderWiseTextual;
		public ZString HarmonizedBorderWiseTextual
		{
			get => harmonizedBorderWiseTextual;
			set
			{
				harmonizedBorderWiseTextual = value;
				HarmonizedBorderWiseTextualInfo.RefreshBinding();
				LegalBookLinkInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo HarmonizedBorderWiseTextualInfo => GetZPropertyInfo(nameof(HarmonizedBorderWiseTextual));

		public ZString LegalBookLink
		{
			get
			{
				return HarmonizedBorderWiseTextual.IsEmpty ? ZString.Empty : Res.GetString("CBCD11B3-390C-49ED-A87F-05DEE0193483", "View");
			}
		}

		public ZPropertyInfo LegalBookLinkInfo => GetZPropertyInfo(nameof(LegalBookLink));

		public BaseJobComInvoiceLine Parent { get; }

		public IComplianceCommodityRiskStatusProvider CommodityRiskStatusProvider { get; }
		BaseJobDeclaration Declaration { get; }
		public ISupportInteractionWithComplianceWiseCommodities SupportInteractionWithCommodities { get; private set; }

		public async Task ViewBorderWisePortalIfAvailable()
		{
			if (CommodityExists && !HarmonizedBorderWiseTextual.IsEmpty
				&& SupportInteractionWithCommodities.Helper?.SourceSideCommodities?.ViewBorderWisePortalIfAvailable != null)
			{
				await SupportInteractionWithCommodities.Helper.SourceSideCommodities.ViewBorderWisePortalIfAvailable.Invoke(new ComplianceCommodityFromSource
				{
					HarmonizedCode = Parent.JI_TariffForComplianceWise,
					GroupingOrCountry = WorldCustomsOrganisationWCO,
					GoodsDescription = Parent.JI_Description.IsEmpty ? Parent.JI_NDescription : Parent.JI_Description,
					OriginOfGoods = Parent.JI_CountryOfOrigin
				});
			}
		}

		public CodeDescriptionPairList CommodityRiskStatusCodeList
		{
			get
			{
				return (string)RiskStatus switch
				{
					ComplianceRiskStatusCodeList.Codes.Clear => Factory.GetCachedValue("CommodityRiskStatusCodeList|Clear", () => ComplianceRiskStatusCodeList.GetCommodityClearRiskStatusList()),
					ComplianceRiskStatusCodeList.Codes.PotentialRisk => Factory.GetCachedValue("CommodityRiskStatusCodeList|PotentialRisk", () => ComplianceRiskStatusCodeList.GetCommodityPotentialRiskStatusList()),
					ComplianceRiskStatusCodeList.Codes.NotChecked => Factory.GetCachedValue((NoResString)"CommodityRiskStatusCodeList|Not Checked", () => ComplianceRiskStatusCodeList.GetCommodityNotCheckedStatusList()),
					ComplianceRiskStatusCodeList.Codes.PossibleRisk => Factory.GetCachedValue("CommodityRiskStatusCodeList|PossibleRisk", () => ComplianceRiskStatusCodeList.GetCommodityPossibleRiskStatusList()),
					ComplianceRiskStatusCodeList.Codes.HighRisk => Factory.GetCachedValue("CommodityRiskStatusCodeList|HighRisk", () => ComplianceRiskStatusCodeList.GetCommodityHighRiskStatusList()),
					_ => Factory.GetCachedValue("CommodityRiskStatusCodeList|Other", () => ComplianceRiskStatusCodeList.GetCommodityBlockedReleasedStatusList()),
				};
			}
		}

		public CodeDescriptionPairList CommodityImportAlertForExportCodeList
		{
			get
			{
				return Factory.GetCachedValue("CommodityImportAlertForExportCodeList", () => ComplianceRiskStatusCodeList.GetImportAlertForExportJobStatusList());
			}
		}
	}
}
