using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public partial class JobDeclaration : AutoJobDeclaration
	{
		[MaxLength(nameof(GoodsDescriptionMaxLength))]
		public override ZString JE_GoodsDescription
		{
			get { return base.JE_GoodsDescription; }
			set { base.JE_GoodsDescription = value; }
		}

		int GoodsDescriptionMaxLength
		{
			get { return IsDrawback ? 35 : Schema.JE_GoodsDescriptionMaxLength; }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobDeclarationLookups.US_NAFTACountryCodeList))]
		public override ZString US_NAFTADrawbackCountry
		{
			get { return base.US_NAFTADrawbackCountry; }
			set { base.US_NAFTADrawbackCountry = value; }
		}

		public override ZString US_DRWPurpose
		{
			get { return base.US_DRWPurpose; }
			set
			{
				var oldValue = US_DRWPurpose;
				base.US_DRWPurpose = value;
				if (!IsCopying && oldValue != US_DRWPurpose && IsDrawback)
				{
					foreach (var line in InvoiceLines.Cast<JobComInvoiceLine>().Where(x => x.ShouldReCalculateDrawbackData))
					{
						var claims = line.Claims;
						claims.HMFClaim.DefaultDeclaredAmount();
						claims.MPFClaim.DefaultDeclaredAmount();
					}
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobDeclarationLookups.US_ClaimPortCodeList))]
		public override ZString US_ClaimPort
		{
			get { return base.US_ClaimPort; }
			set
			{
				base.US_ClaimPort = value;
				if (!value.IsEmpty)
				{
					if (US_TeamNo.IsEmpty)
					{
						ZString teamNo;
						if (AddInfoLookups.ValidClaimPortTeamNos.TryGetValue(value, out teamNo))
						{
							US_TeamNo = teamNo;
						}
					}

					if (IsACEDrawback && US_PreparerDistrictPort.IsEmpty && AddInfoLookups.ACEDrawbackProcessingPortCodeList.ContainsCode(value))
					{
						US_PreparerDistrictPort = value;
					}
				}
			}
		}

		[ReadOnlyMember(nameof(IsACEDrawback))]
		public override ZDecimal US_DRWTotalPRDC
		{
			get { return IsACEDrawback ? DrawbackSupporter.TotalPuertoRicoDrawback : base.US_DRWTotalPRDC; }
			set { base.US_DRWTotalPRDC = value; }
		}

		public ZString JE_TransfereeMiscFields
		{
			get { return ZString.Empty; }
		}

		public ZBool Is7552
		{
			get { return IsDrawback && (US_DRWPurpose == DrawbackDeclarationPurposeList.Codes.CD || US_DRWPurpose == DrawbackDeclarationPurposeList.Codes.CM); }
		}

		public ZBool IsManufacturingDrawbackSupported
		{
			get { return IsDrawback && (US_DRWPurpose == DrawbackDeclarationPurposeList.Codes.DRW || US_DRWPurpose == DrawbackDeclarationPurposeList.Codes.CM); }
		}

		public ZString US_ClaimPortName
		{
			get { return AddInfoLookups.US_ClaimPortCodeList.GetDescriptionFromCode(US_ClaimPort); }
		}

		public ZString US_PreparerDistrictPortName
		{
			get
			{
				var port = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, US_PreparerDistrictPort, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);
				return (port == null) ? ZString.Empty : port.ZZD_Description;
			}
		}

		public ZString US_ConcatenatedContracts
		{
			get
			{
				var concatenatedContracts = new ZStringBuilder();
				var contracts = Factory.Load<ContractNumber>(new ZQuery(CusCodeDataSchema.CY_ParentID, this.PK));

				foreach (var contract in contracts)
				{
					concatenatedContracts.Append(contract.CY_Data);
				}

				return concatenatedContracts.ToStringWithDelimiterBetweenAppends(", ");
			}
		}

		public void SetDefaultValuesForDrawback()
		{
			using (SuspendSettingHasChanges())
			{
				JE_RS_NKServiceLevel = ZString.Empty;
				JE_MessageType = JobMessageTypeList.Codes.Drawback;
				JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
				JE_TransportMode = String.Empty;
				US_DRWFilingMethod = "A";
			}
		}

		void FillInDrawbackDeclarationNumberIfRequired()
		{
			if (DeclarationNumber.IsEmpty && string.IsNullOrEmpty(DisallowAllocateImportEntryNumber))
			{
				AllocateNextImportEntryNumber();
			}
		}

		public JobDeclarationDrawbackSupporter DrawbackSupporter
		{
			get
			{
				if (drawbackSupporter == null)
				{
					drawbackSupporter = new CachedProperty<JobDeclarationDrawbackSupporter>(Factory, delegate
					{
						return new JobDeclarationDrawbackSupporter(this, UpdateActionCode.Add);
					});
				}
				return drawbackSupporter.Value;
			}
		}
		CachedProperty<JobDeclarationDrawbackSupporter> drawbackSupporter;

		public ZDateTime CustomsLastEntryStatusDate
		{
			get
			{
				if (customsLastEntryStatusDate == null)
				{
					customsLastEntryStatusDate = new CachedProperty<ZDateTime>(Factory, () =>
					{
						return Logs.Find(l => l.SL_SE_NKEvent == Events.CustomsEntryStatus.Code).MaxOrDefault(x => x.SL_EventTime);
					});
				}
				return customsLastEntryStatusDate.Value;
			}
		}
		CachedProperty<ZDateTime> customsLastEntryStatusDate;

		public ZDecimal TotalDutyClaimAmount
		{
			get { return DrawbackSupporter.TotalClaimDuty; }
		}

		public ZDecimal TotalTaxClaimAmount
		{
			get { return DrawbackSupporter.TotalClaimTax; }
		}

		public ZDecimal TotalMPFClaimAmount
		{
			get { return DrawbackSupporter.TotalMPF; }
		}

		public ZDecimal TotalHMFClaimAmount
		{
			get { return DrawbackSupporter.TotalHMF; }
		}

		public ZDecimal TotalOtherClaimAmount
		{
			get { return DrawbackSupporter.TotalOtherFees; }
		}

		public ZDecimal TotalClaimAmount
		{
			get { return DrawbackSupporter.TotalDrawbackClaimed; }
		}

		public ZDecimal TotalAdjDutyClaimAmount => DrawbackSupporter.TotalAdjDutyClaimAmount;
		public ZDecimal TotalAdjTaxClaimAmount => DrawbackSupporter.TotalAdjTaxClaimAmount;
		public ZDecimal TotalAdjHMFClaimAmount => DrawbackSupporter.TotalAdjHMFClaimAmount;
		public ZDecimal TotalAdjMPFClaimAmount => DrawbackSupporter.TotalAdjMPFClaimAmount;

		public ZBool IsHMF_MPFClaimable
		{
			get
			{
				var result = false;

				if (US_DRWPurpose == DrawbackDeclarationPurposeList.Codes.DRW)
				{
					if (IsACEDrawback)
					{
						result = ACEDrawbackProvisionsList.IsHMF_MPFClaimable(US_EntryType);
					}
					else
					{
						result = EntryTypeList.IsDrawbackHMF_MPFClaimable(US_EntryType);
					}
				}

				return result;
			}
		}

		public bool IsNotRejectedMerchandiseDrawback
		{
			get
			{
				return (!IsACEDrawback && US_EntryType != EntryTypeList.Codes.RejectedMerchandiseDrawback)
					|| (IsACEDrawback && !ACEDrawbackProvisionsList.IsRejectedMerchandise(US_EntryType));
			}
		}

		public bool IsACEDrawback
		{
			get { return IsDrawback && JE_ApplicationCode == JobApplicationCodeList.Codes.ACE; }
		}

		public bool IsDrawbackTFTEA
		{
			get { return ACEDrawbackProvisionsList.IsTFTEA(US_EntryType); }
		}

		public override bool IsPropertySupported(ZPropertyInfo propertyInfo)
		{
			return base.IsPropertySupported(propertyInfo) && (propertyInfo != JE_RS_NKServiceLevelInfo || !IsDrawback);
		}

		#region Line To Print

		protected override void Lines_CountChanged(object sender, EventArgs e)
		{
			base.Lines_CountChanged(sender, e);
			InvoiceLines.CountChanged -= Lines_CountChanged;
		}

		protected override Customs.Business.LineToPrintCollection GetNewLineToPrintCollection()
		{
			InvoiceLines.CountChanged += Lines_CountChanged;
			return new JobDeclarationDrawbackSupporter.DrawbackNoticeOfIntentLineToPrintCollection(this);
		}

		public override bool SupportSelectingLinesToPrint
		{
			get { return true; }
		}

		#endregion

		#region Overriden Properties

		public override ZString US_DRWRejectedMerchandiseReason
		{
			get { return IsNotRejectedMerchandiseDrawback ? ZString.Empty : base.US_DRWRejectedMerchandiseReason; }
			set { base.US_DRWRejectedMerchandiseReason = value; }
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			if (!SuspendAddingWorkflow && IsDrawback)
			{
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			}
			base.OnFactorySavingBeforeTransactionCore();
		}

		public override ZBool US_AcceleratedClaimInd
		{
			get { return base.US_AcceleratedClaimInd; }
			set
			{
				var hasChanges = base.US_AcceleratedClaimInd != value;
				base.US_AcceleratedClaimInd = value;

				if (hasChanges && !IsCopying)
				{
					if (IsACEDrawback && US_AcceleratedClaimInd)
					{
						new BondDetailsDefaulter().Default(this, US_BondType);
					}

					if (!US_AcceleratedClaimInd)
					{
						US_BondType = ZString.Empty;
						US_BondType2 = ZString.Empty;
					}

					InvoiceLines.OfType<JobComInvoiceLine>().ForEach(invoiceLine =>
					{
						invoiceLine.Claims.OtherFeesClaim.DefaultOverrideData();
						invoiceLine.DrawbackOtherFees.Cast<DrawbackOtherFee>().ForEach(x => x.Claims.DefaultOverrideData());
					});
				}
			}
		}

		public override ZString US_DRWSection
		{
			get => base.US_DRWSection;
			set
			{
				var oldValue = US_DRWSection;
				base.US_DRWSection = value;
				if (!IsCopying && oldValue != US_DRWSection && IsDrawback && JE_ApplicationCode != JobApplicationCodeList.Codes.ACE)
				{
					InvoiceLines.OfType<JobComInvoiceLine>().ForEach(line =>
					{
						if (line.ShouldReCalculateDrawbackData)
						{
							line.Claims.IRTaxClaim.Default_99ClaimedDutyAndCalculatedAmount();
						}
					});
				}
			}
		}

		#endregion
	}
}
