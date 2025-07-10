using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.SG;
using Enterprise.Customs.SG.Business.CustomsMessaging;
using Enterprise.Customs.SG.Registry;
using Enterprise.Customs.SG.V4.Business.TypeSafe;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Edifact.Utilities;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.SG.V4.Business
{
	public partial class JobDeclaration : TypeSafeJobDeclaration,
		Integration.Customs.SG.IJobDeclaration,
		IApportionInvoiceHolder,
		ICusAddInfoTypeSupporter,
		ICusCodeDataTypeSupporter,
		ICusSupportingInfoTypeSupporter,
		ICommonInvoiceDataProvider,
		IImportExport,
		IDocAddresses
	{
		#region Schema

		public new class Schema : AutoJobDeclaration.Schema
		{
			public const string JE_Calc_InvoicesCount = "JE_Calc_InvoicesCount";
			public const string CertificateNumber = "CertificateNumber";
			public const string ManifestStatus = "ManifestStatus";
			public const string ManifestStatusDescription = "ManifestStatusDescription";
			public const string OutwardShippingLineForwarderPK = "OutwardShippingLineForwarderPK";
			public const string JE_OH_Claimant = "JE_OH_Claimant";
			public const string JE_OH_InwardCarrierAgent = "JE_OH_InwardCarrierAgent";
			public const string JE_OH_HandlingAgent = "JE_OH_HandlingAgent";
		}

		#endregion

		public JobDeclaration(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			InvoiceDeleteEvent.AddInvoiceDeletedEventHandler(factory, OnInvoiceDeleted);
		}

		protected override BusinessObject GetAddInfoChild() => AddInfoChild;
		protected override SchemaGuidColumn GetChildForeignKeyColumn() => JobSGDeclarationSchema.SGE_JE;

		[List(nameof(Lookups) + "." + nameof(SGAddInfoLookups.UnitOfQuantityList))]
		public override ZDecimal JE_TotalNoOfPacksDecimal
		{
			get { return base.JE_TotalNoOfPacksDecimal != 0m ? base.JE_TotalNoOfPacksDecimal : ZDecimal.Parse(JE_TotalNoOfPacks.ToString()); }
			set
			{
				base.JE_TotalNoOfPacksDecimal = value;
				JE_TotalNoOfPacks = ZInt.ParseSafe(value.ToString(0), 0);
			}
		}

		protected override string SubmissionTypeBuiltinCode => SGConstants.TradeNetVersion.FourPointOne;

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.ApplicationCodeList))]
		[ResourceStringData("1486BEC0-21AF-45BF-BB23-1E03375DEB88", Caption = "Message")]
		public override ZString JE_ApplicationCode
		{
			get => base.JE_ApplicationCode;
			set
			{
				var oldValue = base.JE_ApplicationCode;
				if (oldValue != value)
				{
					base.JE_ApplicationCode = value;
				}
			}
		}

		protected override bool SupportTransferFromCustomsToManifestEvent => true;

		protected override bool IsClearedEntryStatus(ZString status)
		{
			return
				status == Core.SGConstants.DeclarationStatus.DeclarationPermitReceived ||
				status == Core.SGConstants.DeclarationStatus.AmendmentPermitReceived ||
				status == Core.SGConstants.DeclarationStatus.RefundPermitReceived ||
				status == Core.SGConstants.DeclarationStatus.CancellationAccepted;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			using (SetSettingDefaultValuesInProgress())
			using (SuspendMarkingAsNeedingValidation())
			{
				JE_ContainerMode = CargoPackingCodeList.Codes.PackingType5;
				JE_PaymentMethod = ZString.Empty;
				JE_MergeBy = Env.Registry.CommercialInvoiceLineMergeMethod;
				if (GlbBranch.CurrentBranch.OrgProxy != null)
				{
					JE_OH_Forwarder = GlbBranch.CurrentBranch.OrgProxy.PK;
				}

				if (IsOUTDEC && !JE_TransportMode.IsEmpty)
				{
					SG_OutwardTransportMode = JE_TransportMode;
					JE_TransportMode = ZString.Empty;
				}
			}
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			if (!ReadOnly)
			{
				SetReadonlyState();
			}
		}

		public override void Delete()
		{
			base.Delete();
			InvoiceDeleteEvent.RemoveInvoiceDeletedEventHandler(Factory, OnInvoiceDeleted);
		}

		void SetReadonlyState()
		{
			SetReadOnlyIncludingChildren(
				   JE_EntryStatus == Core.SGConstants.DeclarationStatus.DeclarationPending
				   || JE_EntryStatus == Core.SGConstants.DeclarationStatus.DeclarationSent
				   || JE_EntryStatus == Core.SGConstants.DeclarationStatus.AmendmentPending
				   || JE_EntryStatus == Core.SGConstants.DeclarationStatus.AmendmentSent
				   || JE_EntryStatus == Core.SGConstants.DeclarationStatus.RefundPending
				   || JE_EntryStatus == Core.SGConstants.DeclarationStatus.RefundSent
				   || JE_EntryStatus == Core.SGConstants.DeclarationStatus.CancellationPending
				   || JE_EntryStatus == Core.SGConstants.DeclarationStatus.CancellationSent);
		}

		public void ResetToWorking()
		{
			string newStatus = DeclarationNumber.IsEmpty && CertificateNumber.IsEmpty ? "" : Core.SGConstants.DeclarationStatus.DeclarationPermitReceived;

			foreach (CusEntryHeader cusEntryHeader in ActiveEntryHeaders)
			{
				cusEntryHeader.CH_Status = newStatus;
			}
		}

		public override ZBool AreMultipleEntryInstructionsAllowed => false;

		#region Properties

		public override bool IsAir
		{
			get
			{
				return IsOutwardTransportOnly ? SG_OutwardTransportMode == Enterprise.Core.Constants.TransportModes.Air : base.IsAir;
			}
		}

		public override ZBool IsPost
		{
			get
			{
				return IsOutwardTransportOnly ? (ZBool)(SG_OutwardTransportMode == Enterprise.Core.Constants.TransportModes.Mail) : base.IsPost;
			}
		}

		public override ZBool IsRail
		{
			get
			{
				return IsOutwardTransportOnly ? (ZBool)(SG_OutwardTransportMode == Enterprise.Core.Constants.TransportModes.Rail) : base.IsRail;
			}
		}

		public override ZBool IsRoad
		{
			get
			{
				return IsOutwardTransportOnly ? (ZBool)(SG_OutwardTransportMode == Enterprise.Core.Constants.TransportModes.Road) : base.IsRoad;
			}
		}

		public override ZBool IsSea
		{
			get
			{
				return IsOutwardTransportOnly ? (ZBool)(SG_OutwardTransportMode == Enterprise.Core.Constants.TransportModes.Sea) : base.IsSea;
			}
		}

		public bool IsOutwardTransportOnly
		{
			get { return JE_TransportMode.IsEmpty & IsOUTDEC; }
		}

		public ZInt JE_Calc_InvoicesCount
		{
			get { return Invoices.Count; }
		}

		public ZPropertyInfo JE_Calc_InvoicesCountInfo
		{
			get { return GetZPropertyInfo(Schema.JE_Calc_InvoicesCount); }
		}

		void OnInvoiceDeleted(object sender, EventArgs e)
		{
			if (!IsValidationSuspended)
			{
				Validation.ValidateJE_Calc_InvoicesCount();
			}

			JE_Calc_InvoicesCountInfo.RefreshBinding();
		}

		public bool IsOutwardTransportModeSea => SG_OutwardTransportMode == TransportModeCodeList.Codes.TransportMode_1_SEA;

		public bool IsOutwardTransportModeRail => SG_OutwardTransportMode == TransportModeCodeList.Codes.TransportMode_2_Rail;

		public bool IsOutwardTransportModeRoad => SG_OutwardTransportMode == TransportModeCodeList.Codes.TransportMode_3_Road;

		public bool IsOutwardTransportModeAir => SG_OutwardTransportMode == TransportModeCodeList.Codes.TransportMode_4_Air;

		public bool IsOutwardTransportModeMail => SG_OutwardTransportMode == TransportModeCodeList.Codes.TransportMode_5_Mail;

		public bool IsOUTDEC => JE_MessageType == MessageTypeCodeList.Codes.OUT;

		public bool IsSeaStore
		{
			get
			{
				var result = false;
				if (IsTradeNet4Point1)
				{
					foreach (SGCPC cpcValue in CPCs)
					{
						var procedureCode = cpcValue.SG_CPCCode.SubstringSafe(0, 3);
						var concession = cpcValue.SG_CPCCode.SubstringSafe(3, 4);
						var cpcCode = new RefCusProcedure.Loader(Factory).LoadFromProcedureAndPreviousProcedureAndConcession(procedureCode, ZString.Empty, concession, ZString.Empty, Core.Constants.CountryCodes.Singapore, ZDateTime.Today);
						if (cpcCode != null && cpcCode.HasAttribute(AttributeNames.Codes.ISSEASTORE))
						{
							result = true;
						}
					}
				}
				else
				{
					result = SG_IsSeaStore;
				}

				return result;
			}
		}

		public bool HasCofO
		{
			get { return IsStandAloneCertificateOfOrigin || (IsOUTDEC && JE_MessageSubType != DeclarationTypeCodeList.Codes.BKO && !SG_ApplicationProductType.IsEmpty); }
		}

		public bool IsStandAloneCertificateOfOrigin
		{
			get { return JE_MessageType == MessageTypeCodeList.Codes.COO; }
		}

		public IAdditionalMessageInformation AdditionalMessageInformation
		{
			get { return additionalMessageInformation; }
			set { additionalMessageInformation = value; }
		}
		IAdditionalMessageInformation additionalMessageInformation;

		public bool HasLiquorOrTobacco
		{
			get
			{
				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					if (invoiceLine.SG_TariffCommodityType == CommodityTypeList.Codes.Alcohol || invoiceLine.SG_TariffCommodityType == CommodityTypeList.Codes.Tobacco)
					{
						return true;
					}
				}

				return false;
			}
		}

		public bool IsTemporaryConsignment
		{
			get
			{
				return JE_MessageSubType == DeclarationTypeCodeList.Codes.TCS
					|| JE_MessageSubType == DeclarationTypeCodeList.Codes.TCR
					|| JE_MessageSubType == DeclarationTypeCodeList.Codes.TCE
					|| JE_MessageSubType == DeclarationTypeCodeList.Codes.TCO;
			}
		}

		public bool IsTradenet4
		{
			get
			{
				return JE_ApplicationCode == SGConstants.TradeNetVersion.Four ||
					   JE_ApplicationCode == SGConstants.TradeNetVersion.FourPointOne;
			}
		}

		public bool IsTradenet4OrITF => IsTradenet4 || JE_ApplicationCode == DeclarationApplicationCodeList.Codes.Interfaced;

		public bool IsTradeNet4Point1
		{
			get { return JE_ApplicationCode == SGConstants.TradeNetVersion.FourPointOne; }
		}

		public bool HasBeenCleared
		{
			get { return !JE_EntryAuthorisationDate.IsEmpty; }
		}

		#region CertificateNumber

		[BusinessObjectTestExclude]
		[MaxLength(CusEntryNumber.Schema.CE_EntryNumMaxLength)]
		[ReadOnly(true)]
		public ZString CertificateNumber
		{
			get
			{
				ZStringBuilder result = new ZStringBuilder();

				foreach (CusEntryHeader cusEntryHeader in ActiveEntryHeaders)
				{
					if (!cusEntryHeader.CertificateNumber.IsEmpty)
					{
						result.Append(cusEntryHeader.CertificateNumber);
					}
				}

				return result.ToStringWithDelimiterBetweenAppends(",");
			}
		}

		public ZPropertyInfo CertificateNumberInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.CertificateNumber);
			}
		}

		#endregion

		public bool ShouldAEOBeApplied
		{
			get
			{
				var result = false;
				foreach (RefCusProcedure cpcCode in CPCCollection)
				{
					if (cpcCode.HasAttribute(Universal.AttributeNames.Codes.ISAEO))
					{
						result = true;
						break;
					}
				}

				return result;
			}
		}

		public IEnumerable<ZString> AEOAppliedCountryList
		{
			get
			{
				var today = ZDateTime.Today;
				var query = new ZQuery(RefCusCodeListSchema.ZZD_ZZK_NKCodeType, OrgCusCode.SingaporeCodeTypes.AEO);
				query.AddToFilter(RefCusCodeListSchema.ZZD_ZZZ_NKDataGrouping, Core.Constants.CountryCodes.Singapore);
				query.AddToFilter(RefCusCodeListSchema.ZZD_StartDate, SQLComparisonOperator.LessThanOrEqualTo, today);
				query.AddToFilter(RefCusCodeListSchema.ZZD_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, today);
				var cusCodeList = Factory.Load<RefCusCodeList>(query);
				return cusCodeList.Any() ? cusCodeList.Cast<RefCusCodeList>().Select(x => x.ZZD_Code) : Array.Empty<ZString>();
			}
		}

		#region Outward Vessel BOs

		public virtual RefVessel OutwardVessel
		{
			get { return Factory.LoadFromNaturalKey<RefVessel>(RefVesselSchema.RV_Code, SG_OutwardVesselName); }
		}

		public virtual RefVessel TowingVessel
		{
			get { return Factory.LoadFromNaturalKey<RefVessel>(RefVesselSchema.RV_Code, SG_TowingVesselName); }
		}

		#endregion

		public ZZRefCusCodeListCombined PlaceOfStorage => SG_US_NKPlaceOfStorage.IsEmpty ? null : SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SG_US_NKPlaceOfStorage);

		public ZZRefCusCodeListCombined PlaceOfRelease => SG_US_NKPlaceOfCargoRelease.IsEmpty ? null : SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SG_US_NKPlaceOfCargoRelease);

		public ZZRefCusCodeListCombined PlaceOfReceipt => SG_US_NKPlaceOfReceipt.IsEmpty ? null : SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SG_US_NKPlaceOfReceipt);

		public ZZRefCusCodeListCombined InwardVesselBerth => SG_US_NKInwardVesselBerth.IsEmpty ? null : SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SG_US_NKInwardVesselBerth);

		public ZZRefCusCodeListCombined OutwardVesselBerth => SG_US_NKOutwardVesselBerth.IsEmpty ? null : SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SG_US_NKOutwardVesselBerth);

		#endregion

		#region Calculated Properties For binding

		public ZDecimal TotalDutyPayable
		{
			get
			{
				ZDecimal result = 0;

				if (TotalCustomsValue > 0m && ActiveEntryHeaders.Count >= 1)
				{
					result = ((ISGCUSDEC)ActiveEntryHeaders[0]).TotalDutyPayable;
				}

				return result;
			}
		}

		public ZPropertyInfo TotalDutyPayableInfo
		{
			get { return GetZPropertyInfo(nameof(TotalDutyPayable)); }
		}

		public ZDecimal TotalExcisePayable
		{
			get
			{
				ZDecimal result = 0;

				if (TotalCustomsValue > 0m && ActiveEntryHeaders.Count >= 1)
				{
					result = ((ISGCUSDEC)ActiveEntryHeaders[0]).TotalExcisePayable;
				}

				return result;
			}
		}

		public ZPropertyInfo TotalExcisePayableInfo
		{
			get { return GetZPropertyInfo(nameof(TotalExcisePayable)); }
		}

		public ZDecimal TotalOtherTaxPayable
		{
			get
			{
				ZDecimal result = 0;

				if (TotalCustomsValue > 0m && ActiveEntryHeaders.Count >= 1)
				{
					result = ((ISGCUSDEC)ActiveEntryHeaders[0]).TotalOtherTaxPayable;
				}

				return result;
			}
		}

		public ZPropertyInfo TotalOtherTaxPayableInfo
		{
			get { return GetZPropertyInfo(nameof(TotalOtherTaxPayable)); }
		}

		public ZDecimal TotalGSTPayable
		{
			get
			{
				ZDecimal result = 0;

				if (TotalCustomsValue > 0m && ActiveEntryHeaders.Count >= 1)
				{
					result = ((ISGCUSDEC)ActiveEntryHeaders[0]).TotalGSTPayable;
				}

				return result;
			}
		}

		public ZPropertyInfo TotalGSTPayableInfo
		{
			get { return GetZPropertyInfo(nameof(TotalGSTPayable)); }
		}

		public ZDecimal TotalPayable
		{
			get
			{
				ZDecimal result = 0;

				if (TotalCustomsValue > 0m && ActiveEntryHeaders.Count >= 1)
				{
					result = ((ISGCUSDEC)ActiveEntryHeaders[0]).TotalPayable;
				}

				return result;
			}
		}

		public ZPropertyInfo TotalPayableInfo
		{
			get { return GetZPropertyInfo(nameof(TotalPayable)); }
		}

		public ZDecimal TotalCustomsValue
		{
			get
			{
				ZDecimal result = 0;

				if (ActiveEntryHeaders.Count >= 1)
				{
					result = ((ISGCUSDEC)ActiveEntryHeaders[0]).TotalCustomsValue;
				}

				return result;
			}
		}

		public ZPropertyInfo TotalCustomsValueInfo
		{
			get { return GetZPropertyInfo(nameof(TotalCustomsValue)); }
		}

		#endregion

		#region Linked Global Manifest Details

		public ZString ManifestStatus
		{
			get
			{
				var result = ZString.Empty;
				var linkedManifests = LinkedGlobalManifestBillCountry;
				var linkedManifestsCount = linkedManifests.Count;
				if (linkedManifestsCount == 1)
				{
					result = (ZString)linkedManifests[0][Schema.ManifestStatus];
				}
				else if (linkedManifestsCount > 1)
				{
					result = MultipleStatusesCode;
				}
				return result;
			}
		}

		public ZPropertyInfo ManifestStatusInfo => GetZPropertyInfo(Schema.ManifestStatus);

		public ZString ManifestStatusDescription
		{
			get
			{
				var result = Common.SG.GlobalManifestStatusList.Descriptions.NoStatus;
				var manifestStatus = ManifestStatus;
				if (!manifestStatus.IsEmpty)
				{
					result = manifestStatus == MultipleStatusesCode ? "Multiple Statuses" : Lookups.GlobalManifestStatusList.GetDescriptionFromCode(manifestStatus);
				}
				return result;
			}
		}

		public ZPropertyInfo ManifestStatusDescriptionInfo => GetZPropertyInfo(Schema.ManifestStatusDescription);

		DynamicBusinessObjectCollection LinkedGlobalManifestBillCountry
		{
			get
			{
				if (linkedGlobalManifestBillCountry == null)
				{
					var sqlParameters = new ZSqlParameterCollection
					{
						{ "@ABL_HouseNumber", JE_HouseBill, AsycudaBillSchema.ABL_BillNumber },
						{ "@ABL_MasterNumber", JE_MasterBill, AsycudaBillSchema.ABL_BillNumber }
					};

					var queryString = FormattableString.Invariant(
						$@"SELECT hbl.{AsycudaBillSchema.Constants.ABL_BillStatus} [{Schema.ManifestStatus}] FROM {AsycudaBillSchema.Constants.SqlSchemaName}.{AsycudaBillSchema.Constants.TableName} AS mbl
						INNER JOIN {AsycudaManifestHeaderSchema.Constants.SqlSchemaName}.{AsycudaManifestHeaderSchema.Constants.TableName} ON {AsycudaManifestHeaderSchema.Constants.PK} = mbl.{AsycudaBillSchema.Constants.ABL_AMA}
							AND {AsycudaManifestHeaderSchema.Constants.AMA_RN_NKCountry} = '{Core.Constants.CountryCodes.Singapore}'
						INNER JOIN {AsycudaBillSchema.Constants.SqlSchemaName}.{AsycudaBillSchema.Constants.TableName} AS hbl ON hbl.{AsycudaBillSchema.Constants.ABL_AMA} = {AsycudaManifestHeaderSchema.Constants.PK}
							AND hbl.{AsycudaBillSchema.Constants.ABL_BillNumber} = @ABL_HouseNumber
							AND hbl.{AsycudaBillSchema.Constants.ABL_BolType} <> '{ChildBolCode}'
						WHERE mbl.{AsycudaBillSchema.Constants.ABL_BillNumber} = @ABL_MasterNumber
							AND mbl.{AsycudaBillSchema.Constants.ABL_BolType} = '{ChildBolCode}'");

					var mawbRecyclePeriod = FreightDataRegistry.Instance.MAWBRecyclePeriod.Value;
					if (mawbRecyclePeriod > 0)
					{
						sqlParameters.Add("@ABL_SystemCreateTimeUtc", ZDateTime.UtcNow.AddMonths(-mawbRecyclePeriod), AsycudaBillSchema.ABL_SystemCreateTimeUtc);
						queryString = string.Concat(queryString, FormattableString.Invariant($" AND mbl.{AsycudaBillSchema.Constants.ABL_SystemCreateTimeUtc} >= @ABL_SystemCreateTimeUtc"));
					}
					linkedGlobalManifestBillCountry = new DynamicBusinessObjectCollection(Factory);
					linkedGlobalManifestBillCountry.Load(queryString, sqlParameters);
				}
				return linkedGlobalManifestBillCountry;
			}
		}
		DynamicBusinessObjectCollection linkedGlobalManifestBillCountry;

		public const string ChildBolCode = "BOL";
		const string MultipleStatusesCode = "ML";

		#endregion

		#region CPCS

		[ChildEditable(true)]
		public SGCPCCollection CPCs
		{
			get
			{
				if (cpcs == null)
				{
					cpcs = new SGCPCCollection(this);
					RegisterEditableChildObject(cpcs);
				}

				return cpcs;
			}
		}
		SGCPCCollection cpcs;

		public RefCusProcedureCollection CPCCollection
		{
			get
			{
				var dataGrouping = GetDefaultDataGroupingCode();
				var cacheKey = string.Format(CultureInfo.InvariantCulture, "{0}_{1}_{2}_CPCCollection", "SG", JE_MessageSubType, JE_MessageType);
				return Factory.GetCachedValue(cacheKey, delegate
				{
					return new RefCusProcedureCollection(Factory, dataGrouping, DateOfValuation, JE_MessageSubType, JE_MessageType);
				}
				);
			}
		}

		#endregion

		#region Licences

		[ChildEditable(true)]
		public CALicenceNumberCollection CALicences
		{
			get
			{
				if (fCALicences == null)
				{
					fCALicences = new CALicenceNumberCollection(this);
					RegisterEditableChildObject(fCALicences);
					fCALicences.Load();
				}

				return fCALicences;
			}
		}
		CALicenceNumberCollection fCALicences;

		#endregion

		#region Certificate of Origin Types

		public ZZRefCusCodeListCombined Certificate1Type => SG_Cert1Type.IsEmpty ? null : SGCertificatesCodeList.GetCurrentOrMatchingCertificate(Factory, SG_Cert1Type);

		public ZZRefCusCodeListCombined Certificate2Type => SG_Cert2Type.IsEmpty ? null : SGCertificatesCodeList.GetCurrentOrMatchingCertificate(Factory, SG_Cert2Type);

		#endregion

		#region TradersRemarks

		[ChildEditable(true)]
		public TradersRemarkCollection TradersRemarks
		{
			get
			{
				if (fTradersRemarks == null)
				{
					fTradersRemarks = new TradersRemarkCollection(this);
					fTradersRemarks.Load();
					LoadTradersRemarksFromNote(fTradersRemarks);
					RegisterEditableChildObject(fTradersRemarks);
				}
				return fTradersRemarks;
			}
		}
		TradersRemarkCollection fTradersRemarks;

		void LoadTradersRemarksFromNote(TradersRemarkCollection tradersRemarks)
		{
			if (!tradersRemarks.Any())
			{
				var remarksTextSplit = new TextSplitElegantly(512, 5);
				var tradersRemarksNote = new HiddenTextNote(this, PredefinedNoteTypes.Instance.SGTradersRemarks.Description);
				remarksTextSplit.Text = tradersRemarksNote.Text;
				for (var i = 0; i < 5; i++)
				{
					if (!remarksTextSplit[i].IsNullOrEmpty())
					{
						var remark = tradersRemarks.AddNew();
						remark.CSI_Description = remarksTextSplit[i];
					}
				}
			}
		}

		#endregion

		#region Overrides

		[ResourceStringData("116f5dd7-1096-40d6-bca7-09307f2f8ef4", Caption = "BG Indicator")]
		public override ZString JE_PaymentMethod { get => base.JE_PaymentMethod; set => base.JE_PaymentMethod = value; }

		[ResourceStringData("cd666141-4fde-44fe-acf0-1eff8b2b2195", Caption = "Berth")]
		public override ZString SG_US_NKInwardVesselBerth { get => base.SG_US_NKInwardVesselBerth; set => base.SG_US_NKInwardVesselBerth = value; }

		[ResourceStringData("cd666141-4fde-44fe-acf0-1eff8b2b2195", Caption = "Berth")]
		public override ZString SG_US_NKOutwardVesselBerth { get => base.SG_US_NKOutwardVesselBerth; set => base.SG_US_NKOutwardVesselBerth = value; }

		protected override ZBool IsReciprocalRatesCore
		{
			get { return IsReciprocalRatesConstant; }
		}

		internal static bool IsReciprocalRatesConstant
		{
			get { return true; }
		}

		protected override ZString LocalCurrencyCodeCore
		{
			get { return LocalCurrencyConstantCode; }
		}

		internal static ZString LocalCurrencyConstantCode
		{
			get { return Enterprise.Core.Constants.CurrencyCodes.Singapore; }
		}
		protected override ZDate GetDateForDutyRateCore()
		{
			return ZDate.Today.Year < 2012 && IsTradeNet4Point1
				? new ZDate(2012, 01, 01)
				: ZDate.Today;
		}

		public override ZString JE_MessageType
		{
			get { return base.JE_MessageType; }
			set
			{
				if (base.JE_MessageType != value)
				{
					base.JE_MessageType = value;

					if (!IsCopying)
					{
						if (value == MessageTypeCodeList.Codes.INP)
						{
							JE_MessageSubType = DeclarationTypeCodeList.Codes.SFZ;
						}
						else if (value == MessageTypeCodeList.Codes.IPT)
						{
							JE_MessageSubType = DeclarationTypeCodeList.Codes.GST;
						}
						else if (value == MessageTypeCodeList.Codes.OUT)
						{
							JE_MessageSubType = DeclarationTypeCodeList.Codes.DRT;
						}
						else if (value == MessageTypeCodeList.Codes.TNP)
						{
							JE_MessageSubType = DeclarationTypeCodeList.Codes.TTF;
						}
						else
						{
							JE_MessageSubType = "";
						}

						DefaultCountryOfFinalDestination();

						JE_MessageSubTypeInfo.RefreshBinding();
					}
				}
			}
		}

		public bool SupportExtendingAmendmentReason => JE_MessageType == MessageTypeCodeList.Codes.INP;

		#region DocAddressRequirement

		protected override JobDocAddressRequirement GetDocAddressRequirement(DocAddressType addressType)
		{
			switch (addressType)
			{
				case DocAddressType.CarrierHandlingAgent:
					return CarrierHandlingAgentRequirement;
				case DocAddressType.ClaimantAddress:
					return ClaimantAddressRequirement;
				case DocAddressType.InwardCarrierAgent:
					return InwardCarrierAgentRequirement;
				case DocAddressType.OutwardCarrierAgent:
					return OutwardShippingLineForwarderRequirement;
				default:
					return base.GetDocAddressRequirement(addressType);
			}
		}

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			switch (addressType)
			{
				case DocAddressType.CarrierHandlingAgent:
					return Lookups.ForwarderList;
				case DocAddressType.ClaimantAddress:
					return Lookups.Organisations;
				case DocAddressType.InwardCarrierAgent:
					return Lookups.Organisations;
				case DocAddressType.OutwardCarrierAgent:
					return Lookups.Organisations;
			}

			return null;
		}

		public JobDocAddressRequirement CarrierHandlingAgentRequirement => fCarrierHandlingAgentRequirement ?? (fCarrierHandlingAgentRequirement = AddRequirement(DocAddressType.CarrierHandlingAgent, DoCarrierHandlingAgentValidation));
		JobDocAddressRequirement fCarrierHandlingAgentRequirement;

		void DoCarrierHandlingAgentValidation(JobDocAddressValidation validation)
		{
			Validation.ValidateCarrierHandlingAgent(validation);
		}

		public JobDocAddressRequirement ClaimantAddressRequirement => fClaimantAddressRequirement ?? (fClaimantAddressRequirement = AddRequirement(DocAddressType.ClaimantAddress, DoClaimantAddressValidation));
		JobDocAddressRequirement fClaimantAddressRequirement;

		void DoClaimantAddressValidation(JobDocAddressValidation validation)
		{
			Validation.ValidateClaimantAddress(validation);
		}

		public JobDocAddressRequirement InwardCarrierAgentRequirement => fInwardCarrierAgentRequirement ?? (fInwardCarrierAgentRequirement = AddRequirement(DocAddressType.InwardCarrierAgent, DoInwardCarrierAgentValidation));
		JobDocAddressRequirement fInwardCarrierAgentRequirement;

		void DoInwardCarrierAgentValidation(JobDocAddressValidation validation)
		{
			Validation.ValidateInwardCarrierAgent(validation);
		}

		public JobDocAddressRequirement OutwardShippingLineForwarderRequirement => fOutwardShippingLineForwarderRequirement ?? (fOutwardShippingLineForwarderRequirement = AddRequirement(DocAddressType.OutwardCarrierAgent, DoOutwardShippingLineForwarderValidation));
		JobDocAddressRequirement fOutwardShippingLineForwarderRequirement;

		void DoOutwardShippingLineForwarderValidation(JobDocAddressValidation validation)
		{
			Validation.ValidateOutwardShippingLineForwarder(validation);
		}

		JobDocAddressRequirement AddRequirement(DocAddressType addressType, JobDocAddressRequirement.ValidationDelegate validationFunc)
		{
			var requirement = new JobDocAddressRequirement(addressType)
			{
				CanOverride = false,
				ValidateOrganisationPK = validationFunc
			};
			DocAddressManager.AddRequirement(requirement);
			return requirement;
		}

		#endregion

		#region Claimant

		[RelatedBusinessObject("Claimant")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.Organisations))]
		public ZGuid JE_OH_Claimant
		{
			get { return ClaimantAddress.OrganisationPK; }
			set { ClaimantAddress.OrganisationPK = value; }
		}

		public ZPropertyInfo JE_OH_ClaimantInfo => GetWrappedZPropertyInfo(Schema.JE_OH_Claimant, x => ClaimantAddress.OrganisationPKInfo);

		public OrgHeader Claimant => ClaimantAddress?.Organisation;

		public JobDocAddress ClaimantAddress
		{
			get
			{
				if (fClaimant == null || fClaimant.IsDeleted)
				{
					if (fClaimant != null)
					{
						fClaimant.OrgHeaderAfterChange -= ClaimantAddress_OrgHeaderAfterChange;
					}

					fClaimant = DocAddresses.FindOrCreateWithRequirement(ClaimantAddressRequirement);
					fClaimant.OrgHeaderAfterChange += ClaimantAddress_OrgHeaderAfterChange;
				}

				return fClaimant;
			}
		}
		JobDocAddress fClaimant;

		void ClaimantAddress_OrgHeaderAfterChange(object sender, EventArgs e) => MarkAsNeedingValidation();

		#endregion

		#region JE_OH_HandlingAgent

		[RelatedBusinessObject("HandlingAgent")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.ForwarderList))]
		public ZGuid JE_OH_HandlingAgent
		{
			get { return HandlingAgentAddress.OrganisationPK; }
			set { HandlingAgentAddress.OrganisationPK = value; }
		}

		public ZPropertyInfo JE_OH_HandlingAgentInfo => GetWrappedZPropertyInfo(Schema.JE_OH_HandlingAgent, x => HandlingAgentAddress.OrganisationPKInfo);

		public OrgHeader HandlingAgent => HandlingAgentAddress.Organisation;

		public JobDocAddress HandlingAgentAddress
		{
			get
			{
				if (fHandlingAgentAddress == null || fHandlingAgentAddress.IsDeleted)
				{
					if (fHandlingAgentAddress != null)
					{
						fHandlingAgentAddress.OrgHeaderAfterChange -= HandlingAgentAddress_OrgHeaderAfterChange;
					}

					fHandlingAgentAddress = DocAddresses.FindOrCreateWithRequirement(CarrierHandlingAgentRequirement);
					fHandlingAgentAddress.OrgHeaderAfterChange += HandlingAgentAddress_OrgHeaderAfterChange;
				}

				return fHandlingAgentAddress;
			}
		}
		JobDocAddress fHandlingAgentAddress;

		void HandlingAgentAddress_OrgHeaderAfterChange(object sender, EventArgs e) => MarkAsNeedingValidation();

		#endregion

		#region JE_OH_InwardCarrierAgent

		[RelatedBusinessObject("InwardCarrierAgent")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.Organisations))]
		public ZGuid JE_OH_InwardCarrierAgent
		{
			get { return InwardCarrierAgentAddress.OrganisationPK; }
			set
			{
				var oldValue = JE_OH_InwardCarrierAgent;
				InwardCarrierAgentAddress.OrganisationPK = value;
				var newValue = JE_OH_InwardCarrierAgent;
				if (newValue != oldValue && newValue.IsValid && !IsCopying && JE_OH_ShippingLine.IsEmpty && IsSea)
				{
					if (InwardCarrierAgent?.OH_IsShippingLine ?? false)
					{
						JE_OH_ShippingLine = newValue;
					}
				}
			}
		}

		public ZPropertyInfo JE_OH_InwardCarrierAgentInfo => GetWrappedZPropertyInfo(Schema.JE_OH_InwardCarrierAgent, x => InwardCarrierAgentAddress.OrganisationPKInfo);

		public OrgHeader InwardCarrierAgent => InwardCarrierAgentAddress.Organisation;

		public JobDocAddress InwardCarrierAgentAddress
		{
			get
			{
				if (fInwardCarrierAgentAddress == null || fInwardCarrierAgentAddress.IsDeleted)
				{
					if (fInwardCarrierAgentAddress != null)
					{
						fInwardCarrierAgentAddress.OrgHeaderAfterChange -= InwardCarrierAgentAddress_OrgHeaderAfterChange;
					}

					fInwardCarrierAgentAddress = DocAddresses.FindOrCreateWithRequirement(InwardCarrierAgentRequirement);
					fInwardCarrierAgentAddress.OrgHeaderAfterChange += InwardCarrierAgentAddress_OrgHeaderAfterChange;
				}

				return fInwardCarrierAgentAddress;
			}
		}
		JobDocAddress fInwardCarrierAgentAddress;

		void InwardCarrierAgentAddress_OrgHeaderAfterChange(object sender, EventArgs e) => MarkAsNeedingValidation();

		#endregion

		#region OutwardShippingLineForwarder

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.Organisations))]
		public ZGuid OutwardShippingLineForwarderPK
		{
			get => OutwardShippingLineForwarderDocAddress.OrganisationPK;
			set { OutwardShippingLineForwarderDocAddress.OrganisationPK = value; }
		}

		public ZPropertyInfo OutwardShippingLineForwarderPKInfo => GetWrappedZPropertyInfo(Schema.OutwardShippingLineForwarderPK, x => OutwardShippingLineForwarderDocAddress.OrganisationPKInfo);

		public JobDocAddress OutwardShippingLineForwarderDocAddress
		{
			get
			{
				if (fOutwardShippingLineForwarderDocAddress == null || fOutwardShippingLineForwarderDocAddress.IsDeleted)
				{
					if (fOutwardShippingLineForwarderDocAddress != null)
					{
						fOutwardShippingLineForwarderDocAddress.OrgHeaderAfterChange -= OutwardShippingLineForwarderAddress_OrgHeaderAfterChange;
					}

					fOutwardShippingLineForwarderDocAddress = DocAddresses.FindOrCreateWithRequirement(OutwardShippingLineForwarderRequirement);
					fOutwardShippingLineForwarderDocAddress.OrgHeaderAfterChange += OutwardShippingLineForwarderAddress_OrgHeaderAfterChange;
				}

				return fOutwardShippingLineForwarderDocAddress;
			}
		}
		JobDocAddress fOutwardShippingLineForwarderDocAddress;

		void OutwardShippingLineForwarderAddress_OrgHeaderAfterChange(object sender, EventArgs e) => MarkAsNeedingValidation();

		#endregion

		protected override ZString GetMessageTypeForDocumentFilter()
		{
			var result = ZString.Empty;

			if (IsImportOnly)
			{
				result = JobMessageTypeList.Codes.Import;
			}
			else if (IsExportOnly)
			{
				result = JobMessageTypeList.Codes.Export;
			}
			else if (IsTranshipment)
			{
				result = MessageTypeCodeList.Codes.TNP;
			}

			return result;
		}

		protected override string GetDefaultMessageType(bool import)
		{
			return import ? MessageTypeCodeList.Codes.INP : MessageTypeCodeList.Codes.OUT;
		}

		public ZBool IsTranshipment
		{
			get { return JE_MessageType == MessageTypeCodeList.Codes.TNP; }
		}

		public ZBool IsExportOnly
		{
			get { return IsOUTDEC; }
		}

		public ZBool IsImportOnly
		{
			get { return JE_MessageType == MessageTypeCodeList.Codes.IPT || JE_MessageType == MessageTypeCodeList.Codes.INP; }
		}

		public override ZBool IsExport
		{
			get { return (ZBool)(IsOUTDEC || JE_MessageType == MessageTypeCodeList.Codes.TNP || JE_MessageType == MessageTypeCodeList.Codes.COO); }
		}

		public override ZBool IsImport
		{
			get { return (ZBool)(JE_MessageType == MessageTypeCodeList.Codes.IPT || JE_MessageType == MessageTypeCodeList.Codes.INP || JE_MessageType == MessageTypeCodeList.Codes.TNP); }
		}

		public override ZBool IsContainerised
		{
			get
			{
				return IsTradeNet4Point1 ? JE_ContainerMode == CargoPackingCodeList.Codes.PackingType9 : JE_ContainerMode == CargoPackingTypeCodeList.Codes.PackingType3;
			}
		}

		public override ZBool ContainersRequired
		{
			get { return true; }
		}

		public override ZBool ContainersAlwaysRequired
		{
			get { return true; }
		}

		public override ZString GetDefaultContainerisedContainerMode()
		{
			return IsTradeNet4Point1 ? CargoPackingCodeList.Codes.PackingType9 : CargoPackingTypeCodeList.Codes.PackingType3;
		}

		protected override bool IsCustomsHeaderAmendmentATotalReplacement
		{
			get { return true; }
		}

		protected override bool IsCustomsLineAmendmentATotalReplacement
		{
			get { return true; }
		}

		protected override bool SupportJE_PaymentMethodUsageCore
		{
			get { return true; }
		}

		public override ZString JE_TransportMode
		{
			get { return base.JE_TransportMode; }
			set
			{
				if (base.JE_TransportMode != value)
				{
					base.JE_TransportMode = value;

					if (JE_TransportMode.IsEmpty)
					{
						JE_MasterBill = "";
						JE_HouseBill = "";
						JE_VoyageFlightNo = "";
						ClearInwardVesselDetails();
					}
					else if (IsAir)
					{
						ClearInwardVesselDetails();
					}
					else if (!IsSea && !IsAir)
					{
						JE_MasterBill = "";
						JE_VoyageFlightNo = "";
						ClearInwardVesselDetails();
					}

					JE_TransportModeInfo.HumanReadableName = JE_VoyageFlightNoHumanReadableName;
				}
			}
		}

		void ClearInwardVesselDetails()
		{
			SG_US_NKInwardVesselBerth = "";
			JE_VesselName = "";
			JE_LloydsIMO = "";
		}

		[ResourceStringData("41905e6a-ebd4-4db3-852a-36712a4a997a", Caption = "Outward Transport")]
		public override ZString SG_OutwardTransportMode
		{
			get { return base.SG_OutwardTransportMode; }
			set
			{
				if (base.SG_OutwardTransportMode != value)
				{
					base.SG_OutwardTransportMode = value;

					if (SG_OutwardTransportMode.IsEmpty)
					{
						SG_OutwardMAWB = "";
						SG_OutwardHAWB = "";
						SG_OutwardVoyageFlightNo = "";
						ClearOutwardVesselDetails();
					}
					else if (IsOutwardTransportModeAir)
					{
						ClearOutwardVesselDetails();
					}
					else if (!IsOutwardTransportModeSea && !IsOutwardTransportModeAir)
					{
						SG_OutwardMAWB = "";
						SG_OutwardVoyageFlightNo = "";
						ClearOutwardVesselDetails();
					}
				}
			}
		}

		void ClearOutwardVesselDetails()
		{
			SG_US_NKOutwardVesselBerth = "";
			SG_OutwardVesselName = "";
			SGE_RN_NKOutwardVesselNationality = "";
			SGE_OutwardVesselNRT = 0;
			SGE_OutwardVesselType = "";
		}

		public void DefaultOutwardVesselDetails()
		{
			DefaultOutwardVesselType();
			SGE_OutwardVesselNRT = OutwardVessel?.RV_NetRegisterTon ?? ZInt.Zero;
			SGE_RN_NKOutwardVesselNationality = OutwardVessel?.RV_RN_NKCountryOfReg ?? ZString.Empty;
		}

		void DefaultOutwardVesselType()
		{
			if (OutwardVessel != null)
			{
				SGE_OutwardVesselType = OutwardVessel.RV_VesselType.Length > 2 ? (ZString)Core.Constants.VesselType.CargoVessel : OutwardVessel.RV_VesselType;
			}
			else
			{
				SGE_OutwardVesselType = ZString.Empty;
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CountryCodeList))]
		public override ZString SGE_RN_NKOutwardVesselNationality { get => base.SGE_RN_NKOutwardVesselNationality; set => base.SGE_RN_NKOutwardVesselNationality = value; }

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.VesselTypeList))]
		public override ZString SGE_OutwardVesselType { get => base.SGE_OutwardVesselType; set => base.SGE_OutwardVesselType = value; }

		[ResourceStringData("f484325c-a407-408b-b3fd-a398c1085012", Caption = "Vessel")]
		public override ZString SG_OutwardVesselName { get => base.SG_OutwardVesselName; set => base.SG_OutwardVesselName = value; }

		#endregion

		public override ZString SG_OutwardMAWB
		{
			get { return base.SG_OutwardMAWB; }
			set
			{
				base.SG_OutwardMAWB = value.KeepAlphanumericCharacters();
			}
		}

		string JE_VoyageFlightNoHumanReadableName
		{
			get
			{
				var result = ResString.GetMultilingualString("0E1C17BE-7A36-4BAA-A1DF-56732FBAF50D", "Voyage / Flight No");
				if (IsTradeNet4Point1)
				{
					if (IsAir)
					{
						result = ResString.GetMultilingualString("9944332B-2156-4AE6-A8F8-0381035DEBA0", "Flight No. / Charter Flight Registration");
					}
					else if (IsSea)
					{
						result = ResString.GetMultilingualString("002B2FF5-1DFD-4D78-9AE6-0AEBDD02C878", "Voyage");
					}
					else if (IsRoad)
					{
						result = ResString.GetMultilingualString("73E8538F-E695-42CF-ACAA-2D8A81EF820D", "Vehicle Registration");
					}
				}

				return result;
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.SGLocoList))]
		public override ZString JE_RL_NKPortOfArrival
		{
			get { return base.JE_RL_NKPortOfArrival; }
			set
			{
				if (base.JE_RL_NKPortOfArrival != value)
				{
					base.JE_RL_NKPortOfArrival = value;
					DefaultCountryOfFinalDestination();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.SGLocoList))]
		public override ZString JE_RL_NKPortOfLoading
		{
			get { return base.JE_RL_NKPortOfLoading; }
			set { base.JE_RL_NKPortOfLoading = value; }
		}

		public override ZString JE_EntryStatusDescription
		{
			get
			{
				if (ActiveEntryHeaders.Count > 0)
				{
					if (JE_MessageType == MessageTypeCodeList.Codes.COO)
					{
						switch (ActiveEntryHeaders[0].CH_Status)
						{
							case Core.SGConstants.DeclarationStatus.DeclarationPending:
								return "Certificate of Origin Queued for Sending.";
							case Core.SGConstants.DeclarationStatus.DeclarationSent:
								return "Certificate of Origin Sent Waiting Response.";
							case Core.SGConstants.DeclarationStatus.DeclarationRejectedByCustoms:
								return "Certificate of Origin Rejected.";
							case Core.SGConstants.DeclarationStatus.DeclarationHadSyntaxErrors:
								return "Certificate of Origin Syntax Error.";
							case Core.SGConstants.DeclarationStatus.DeclarationPermitReceived:
								return "Certificate of Origin Approved.";
							case Core.SGConstants.DeclarationStatus.DeclarationQuery:
								return "Certificate of Origin has been queried. Please take requested action.";
						}
					}
					else
					{
						switch (ActiveEntryHeaders[0].CH_Status)
						{
							case Core.SGConstants.DeclarationStatus.DeclarationPending:
								return "Declaration Queued for Sending.";
							case Core.SGConstants.DeclarationStatus.DeclarationSent:
								return "Declaration Sent Waiting Response.";
							case Core.SGConstants.DeclarationStatus.DeclarationRejectedByCustoms:
								return "Declaration Rejected.";
							case Core.SGConstants.DeclarationStatus.DeclarationHadSyntaxErrors:
								return "Declaration Syntax Error.";
							case Core.SGConstants.DeclarationStatus.DeclarationPermitReceived:
								return "Permit Approved.";
							case Core.SGConstants.DeclarationStatus.DeclarationQuery:
								return "Declaration has been queried. Please take requested action then await Custom's subsequent response.";

							case Core.SGConstants.DeclarationStatus.AmendmentPending:
								return "Amendment Queued for Sending.";
							case Core.SGConstants.DeclarationStatus.AmendmentSent:
								return "Amendment Sent Waiting Response.";
							case Core.SGConstants.DeclarationStatus.AmendmentRejectedByCustoms:
								return "Amendment Rejected.";
							case Core.SGConstants.DeclarationStatus.AmendmentHadSyntaxErrors:
								return "Amendment Syntax Error.";
							case Core.SGConstants.DeclarationStatus.AmendmentPermitReceived:
								return "Permit Approved.";
							case Core.SGConstants.DeclarationStatus.AmendmentQuery:
								return "Amendment has been queried. Please take requested action.";

							case Core.SGConstants.DeclarationStatus.RefundPending:
								return "Refund Queued for Sending.";
							case Core.SGConstants.DeclarationStatus.RefundSent:
								return "Refund Sent Waiting Response.";
							case Core.SGConstants.DeclarationStatus.RefundRejectedByCustoms:
								return "Refund Rejected.";
							case Core.SGConstants.DeclarationStatus.RefundHadSyntaxErrors:
								return "Refund Syntax Error.";
							case Core.SGConstants.DeclarationStatus.RefundPermitReceived:
								return "Refund Approved.";
							case Core.SGConstants.DeclarationStatus.RefundQuery:
								return "Refund has been queried. Please take requested action.";

							case Core.SGConstants.DeclarationStatus.CancellationPending:
								return "Cancellation Queued for Sending.";
							case Core.SGConstants.DeclarationStatus.CancellationSent:
								return "Cancellation Sent Waiting Response.";
							case Core.SGConstants.DeclarationStatus.CancellationRejectedByCustoms:
								return "Cancellation Rejected.";
							case Core.SGConstants.DeclarationStatus.CancellationHadSyntaxErrors:
								return "Cancellation Syntax Error.";
							case Core.SGConstants.DeclarationStatus.CancellationAccepted:
								return "Cancellation Approved.";
							case Core.SGConstants.DeclarationStatus.CancellationQuery:
								return "Cancellation has been queried. Please take requested action.";
						}
					}
				}

				return "Working. Declaration has not been sent.";
			}
		}

		public override ZString JE_EntryStatus
		{
			get { return base.JE_EntryStatus; }
			set
			{
				if (base.JE_EntryStatus != value)
				{
					base.JE_EntryStatus = value;
					SetReadonlyState();
				}
			}
		}

		public override ZDateTime JE_EntryAuthorisationDate
		{
			get { return base.JE_EntryAuthorisationDate; }
			set
			{
				if (base.JE_EntryAuthorisationDate != value)
				{
					base.JE_EntryAuthorisationDate = value;
					if (!value.IsEmpty)
					{
						LogCustomsClearedIfNeeded();
					}

					foreach (var invoiceHeader in Invoices)
					{
						invoiceHeader.MarkAsNeedingValidation();
						foreach (var invoiceLine in invoiceHeader.JobComInvoiceLines)
						{
							invoiceLine.MarkAsNeedingValidation();
						}
					}
				}
			}
		}

		public override ZDateTime DateOfValuation
		{
			get { return ZDateTime.Today; }
		}

		[BusinessObjectTestExclude]
		[ReadOnly(true)]
		public override ZShort JE_ContainerCount
		{
			get { return (ZShort)CusContainers.Count; }
		}

		protected override bool IsPackingInformationRelevantCore
		{
			get { return false; }
		}

		[ResourceStringData("16040B87-E58F-4849-A3BE-BEE92448F0EB", Caption = "Flight/Rego.", IsApplicableMember = nameof(IsAir))]
		[ResourceStringData("16040B87-E58F-4849-A3BE-BEE92448F0EB|4.1|AIR", Caption = "Flight No. / Aircraft Registration", ShortCaption = "Flight/Rego.", FullDescription = "For Air Transport, specify the Flight No. or Aircraft Registration Number for chartered flights.", MultipleKey = CaptionKeyTradeNet4Point1, IsApplicableMember = nameof(IsAir))]
		[ResourceStringData("16040B87-E58F-4849-A3BE-BEE92448F0EB|4.1|SEA", Caption = "Voyage Number", ShortCaption = "Voyage", FullDescription = "For Sea Transport, specify the Voyage Number.", MultipleKey = CaptionKeyTradeNet4Point1, IsApplicableMember = nameof(IsSea))]
		[ResourceStringData("16040B87-E58F-4849-A3BE-BEE92448F0EB|4.1|ROAD", Caption = "Registration", FullDescription = "For Road Transport, specify the vehicle License / Registration number.", MultipleKey = CaptionKeyTradeNet4Point1, IsApplicableMember = nameof(IsRoad))]
		public override ZString JE_VoyageFlightNo { get => base.JE_VoyageFlightNo; set => base.JE_VoyageFlightNo = value; }

		public override void DefaultArrivalAndLoadingDatesFromDestinationAndOrigin()
		{
		}

		public const string CaptionKeyTradeNet4Point1 = "9CB471F2-21BB-440E-8032-C76993CC8540";

		protected override IReadOnlyList<string> MultipleKeysToUseCore => IsTradenet4 ? new[] { CaptionKeyTradeNet4Point1 } : base.MultipleKeysToUseCore;

		protected override void SetPortOfLoading(ZString origin)
		{
		}

		protected override bool ShouldCopyPortOfArrivalToFinalDestination
		{
			get { return false; }
		}

		protected override void SetPortOfArrival(ZString arrival)
		{
		}

		public override ZString JE_AddInfo
		{
			get { return base.JE_AddInfo; }
			set
			{
				if (JE_AddInfo != value)
				{
					base.JE_AddInfo = value;

					foreach (JobComInvoiceHeader invoiceHeader in Invoices)
					{
						invoiceHeader.MarkAsNeedingValidation();
					}

					foreach (JobComInvoiceLine invoiceLine in FilteredInvoiceLines)
					{
						invoiceLine.MarkAsNeedingValidation();
					}
				}
			}
		}

		public override ZBool JE_IsCancelled
		{
			get { return base.JE_IsCancelled; }
			set
			{
				if (JE_IsCancelled != value)
				{
					base.JE_IsCancelled = value;

					foreach (JobComInvoiceHeader invoiceHeader in Invoices)
					{
						invoiceHeader.MarkAsNeedingValidation();
					}

					foreach (JobComInvoiceLine invoiceLine in FilteredInvoiceLines)
					{
						invoiceLine.MarkAsNeedingValidation();
					}
				}
			}
		}

		protected override BaseJobDeclaration GetNewRelatedDeclarationCore(BusinessObjectFactory factory, string relationshipType = null)
		{
			JobDeclaration result = (JobDeclaration)base.GetNewRelatedDeclarationCore(factory, relationshipType);
			result.SG_OutwardMAWB = this.SG_OutwardMAWB;
			result.SG_OutwardHAWB = this.SG_OutwardHAWB;
			return result;
		}

		void DefaultCountryOfFinalDestination()
		{
			if (IsExport)
			{
				SG_RN_NKFinalDestination = JE_RL_NKPortOfArrival.Left(2);
			}
		}

		protected override BaseJobDeclarationInvoicingSupporter GetNewInvoicingSupporter()
		{
			return new JobDeclarationInvoicingSupporter(this);
		}

		protected override JobDeclarationDeepCloneStrategy GetTemplateCopyStrategy(BusinessObjectFactory factory, CloneType cloneType)
		{
			return new JobDeclarationCloneStrategy(this, cloneType, factory);
		}

		protected override RelatedDeclarationCollection GetRelatedDeclarations()
		{
			var result = new ZQuery();
			var queryRequired = false;

			if (!JE_SystemCreateTimeUtc.IsEmpty)
			{
				if (!JE_HouseBill.IsEmpty || !SG_OutwardHAWB.IsEmpty)
				{
					if (JE_MessageType == MessageTypeCodeList.Codes.TNP || IsOUTDEC || JE_MessageType == MessageTypeCodeList.Codes.COO)
					{
						if (!SG_OutwardHAWB.IsEmpty)
						{
							result.AddToFilter(JobDeclarationSchema.JE_AddInfo, SQLComparisonOperator.Contains, "SG62=" + SG_OutwardHAWB);
							queryRequired = true;
						}
					}

					if (JE_MessageType == MessageTypeCodeList.Codes.TNP || JE_MessageType == MessageTypeCodeList.Codes.INP || JE_MessageType == MessageTypeCodeList.Codes.IPT)
					{
						if (!JE_HouseBill.IsEmpty)
						{
							result.AddToFilter(JobDeclarationSchema.JE_HouseBill, JE_HouseBill);
							queryRequired = true;
						}
					}
				}
				else if (!JE_MasterBill.IsEmpty || !SG_OutwardMAWB.IsEmpty)
				{
					if (JE_MessageType == MessageTypeCodeList.Codes.TNP || IsOUTDEC || JE_MessageType == MessageTypeCodeList.Codes.COO)
					{
						if (!SG_OutwardMAWB.IsEmpty)
						{
							result.AddToFilter(JobDeclarationSchema.JE_AddInfo, SQLComparisonOperator.Contains, "SG63=" + SG_OutwardMAWB);
							queryRequired = true;
						}
					}

					if (JE_MessageType == MessageTypeCodeList.Codes.TNP || JE_MessageType == MessageTypeCodeList.Codes.INP || JE_MessageType == MessageTypeCodeList.Codes.IPT)
					{
						if (!JE_MasterBill.IsEmpty)
						{
							result.AddToFilter(JobDeclarationSchema.JE_MasterBill, JE_MasterBill);
							queryRequired = true;
						}
					}
				}

				if (queryRequired)
				{
					result.AddToFilter(JobDeclarationSchema.JE_ClusterKey, SQLComparisonOperator.NotEqual, JE_ClusterKey);
					result.AddToFilter(JobDeclarationSchema.JE_SystemCreateTimeUtc, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, JE_SystemCreateTimeUtc.AddDays(14));
					result.AddToFilter(JobDeclarationSchema.JE_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, JE_SystemCreateTimeUtc.AddDays(-14));
					return RelatedDeclarationCollection.GetLooselyRelatedCollection(this, result);
				}
			}

			return RelatedDeclarationCollection.GetLooselyRelatedCollection(this, ZQuery.NoResultQuery);
		}

		public override bool UseGenPivotForRelatedDeclarations
		{
			get { return false; }
		}

		public override ZString DeclarationNumber
		{
			get
			{
				ZString result = "";

				if (IsTradenet4OrITF)
				{
					result = base.DeclarationNumber;
				}
				else
				{
					ZQuery query = new ZQuery(CusEntryNumSchema.CE_EntryType, "PMT");
					query.AddToFilter(CusEntryNumSchema.CE_ParentID, PK);

					CusEntryNumber[] entryNumbers = Factory.Load<CusEntryNumber>(query);

					if (entryNumbers.Length == 1)
					{
						result = entryNumbers[0].CE_EntryNum;
					}
				}

				return result;
			}
		}

		public override ZString SG_ClaimantCode
		{
			get { return base.SG_ClaimantCode; }
			set
			{
				if (base.SG_ClaimantCode != value)
				{
					base.SG_ClaimantCode = value;
					if (value.IsEmpty)
					{
						base.SG_DutyExempt = false;
					}
					else if (JE_MessageSubType == DeclarationTypeCodeList.Codes.GST)
					{
						base.SG_DutyExempt = true;
					}
				}
			}
		}

		[ReadOnlyMember(nameof(Seastore_ReadOnly))]
		public override ZInt SG_NoOfCrew
		{
			get { return base.SG_NoOfCrew; }
			set { base.SG_NoOfCrew = value; }
		}

		[ReadOnlyMember(nameof(Seastore_ReadOnly))]
		public override ZInt SG_VoyageDuration
		{
			get { return base.SG_VoyageDuration; }
			set { base.SG_VoyageDuration = value; }
		}

		protected virtual bool Seastore_ReadOnly => !SG_IsSeaStore;

		protected override ZString GetContainerModeForDeclarationCore(ZString transportMode, ZString shipmentPackingMode)
		{
			var result = ZString.Empty;

			if (IsTradeNet4Point1)
			{
				if (Core.Constants.ContainerModes.IsContainerised(shipmentPackingMode))
				{
					result = CargoPackingCodeList.Codes.PackingType9;
				}
				else
				{
					result = CargoPackingCodeList.Codes.PackingType5;
				}
			}
			else
			{
				if (shipmentPackingMode == Enterprise.Core.Constants.ContainerModes.FCL)
				{
					result = CargoPackingTypeCodeList.Codes.PackingType3;
				}
				else if (shipmentPackingMode == Enterprise.Core.Constants.ContainerModes.Bulk)
				{
					result = CargoPackingTypeCodeList.Codes.PackingType1;
				}
				else if (shipmentPackingMode == Enterprise.Core.Constants.ContainerModes.ShippersConsol)
				{
					result = CargoPackingCodeList.Codes.PackingType9;
				}
				else
				{
					result = CargoPackingTypeCodeList.Codes.PackingType2;
				}
			}

			return result;
		}

		public new OrgHeader Consignee => JE_OH_Consignee.IsEmpty ? null : Factory.Load<OrgHeader>(JE_OH_Consignee);

		public override ZGuid JE_OH_Importer
		{
			get => base.JE_OH_Importer;
			set
			{
				var oldValue = base.JE_OH_Importer;
				base.JE_OH_Importer = value;
				if (!IsCopying && oldValue != JE_OH_Importer)
				{
					if (!JE_OH_Importer.IsEmpty && IsAEOToApplyForImport)
					{
						UpdateAEOCpcs(JE_OH_Importer, JE_RL_NKOrigin);
					}
				}
			}
		}

		public override ZGuid JE_OH_Supplier
		{
			get => base.JE_OH_Supplier;
			set
			{
				var oldValue = base.JE_OH_Supplier;
				base.JE_OH_Supplier = value;
				if (!IsCopying && oldValue != JE_OH_Supplier)
				{
					if (!JE_OH_Supplier.IsEmpty && IsAEOToApplyForOutward)
					{
						UpdateAEOCpcs(JE_OH_Supplier, JE_RL_NKFinalDestination);
					}
				}
			}
		}

		public override ZString JE_RL_NKOrigin
		{
			get => base.JE_RL_NKOrigin;
			set
			{
				var oldValue = base.JE_RL_NKOrigin;
				base.JE_RL_NKOrigin = value;
				if (!IsCopying && oldValue != JE_RL_NKOrigin)
				{
					if (!JE_OH_Importer.IsEmpty && IsAEOToApplyForImport)
					{
						UpdateAEOCpcs(JE_OH_Importer, JE_RL_NKOrigin);
					}
				}
			}
		}

		public override ZString JE_RL_NKFinalDestination
		{
			get => base.JE_RL_NKFinalDestination;
			set
			{
				var oldValue = base.JE_RL_NKFinalDestination;
				base.JE_RL_NKFinalDestination = value;
				if (!IsCopying && oldValue != JE_RL_NKFinalDestination)
				{
					if (!JE_OH_Supplier.IsEmpty && IsAEOToApplyForOutward)
					{
						UpdateAEOCpcs(JE_OH_Supplier, JE_RL_NKFinalDestination);
					}

					UpdateSG_RN_NKFinalDestinationIfNecessary(oldValue);
				}
			}
		}

		void UpdateSG_RN_NKFinalDestinationIfNecessary(ZString oldDestination)
		{
			if (!JE_RL_NKFinalDestination.IsEmpty && (SG_RN_NKFinalDestination.IsEmpty || oldDestination.StartsWith(SG_RN_NKFinalDestination)))
			{
				var countryCode = JE_RL_NKFinalDestination.Left(2);
				var refCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, countryCode);
				if (refCountry != null)
				{
					SG_RN_NKFinalDestination = countryCode;
				}
			}
		}

		public bool AuthorisedEconomyOperatorIsToApply => ShouldAEOBeApplied && DeclarationNumber.IsEmpty;

		public bool IsAEOToApplyForImport => IsImport && AuthorisedEconomyOperatorIsToApply;

		public bool IsAEOToApplyForOutward => (IsOUTDEC || IsTranshipment) && AuthorisedEconomyOperatorIsToApply;

		void UpdateAEOCpcs(ZGuid orgGuid, ZString orgOrDes)
		{
			var orgHeader = Factory.Load<OrgHeader>(orgGuid);
			if (orgHeader != null)
			{
				var aeoCPCs = CPCs.Where(x => x.SG_APCCodeDescription.StartsWith(OrgCusCode.SingaporeCodeTypes.AEO, StringComparison.OrdinalIgnoreCase));
				var cusCodes = orgHeader.CustomsCodes.Cast<OrgCusCode>().Where(x => x.OK_CodeType == OrgCusCode.SingaporeCodeTypes.AEO);
				if (cusCodes.Any())
				{
					var orgCusCode = cusCodes.Cast<OrgCusCode>().FirstOrDefault(x => x.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.Singapore);
					if (orgCusCode == null)
					{
						var countryCode = orgOrDes.Left(2);
						if (AEOAppliedCountryList.Contains(countryCode))
						{
							orgCusCode = cusCodes.Cast<OrgCusCode>().FirstOrDefault(x => x.OK_RN_NKCodeCountry == countryCode);
						}
					}

					if (orgCusCode != null)
					{
						if (aeoCPCs.Any())
						{
							aeoCPCs.ForEach(x => x.SG_PC1 = orgCusCode.OK_RN_NKCodeCountry);
							aeoCPCs.ForEach(x => x.SG_PC2 = orgCusCode.OK_CustomsRegNo.Left(SGCPCAddInfo.Schema.SG_PC2MaxLength));
						}
						else
						{
							var cpc = CPCs.AddNew();
							cpc.SG_CPCCode = GetCPCCodeForDeclarationType(OrgCusCode.SingaporeCodeTypes.AEO);
							cpc.SG_PC1 = orgCusCode.OK_RN_NKCodeCountry;
							cpc.SG_PC2 = orgCusCode.OK_CustomsRegNo.Left(SGCPCAddInfo.Schema.SG_PC2MaxLength);
						}
					}
					else
					{
						aeoCPCs.DeleteAll();
					}
				}
				else
				{
					aeoCPCs.DeleteAll();
				}
			}
		}

		#region CPC values for Declaration type

		public string GetCPCCodeForDeclarationType(string cpc)
		{
			var result = cpc.Length > 35 ? cpc.Substring(0, 35) : cpc;
			foreach (RefCusProcedure cpcCode in CPCCollection)
			{
				if (cpcCode.ZZ6_Description == cpc || cpcCode.ZZ6_Description.StartsWith(cpc + " (", StringComparison.OrdinalIgnoreCase))
				{
					if (HasCofO)
					{
						if (cpcCode.HasAttribute(AttributeNames.Codes.ISCOO))
						{
							result = cpcCode.ZZ6_ProcedureCode + cpcCode.ZZ6_Concession;
							break;
						}
					}
					else
					{
						if (!cpcCode.HasAttribute(AttributeNames.Codes.ISCOO))
						{
							result = cpcCode.ZZ6_ProcedureCode + cpcCode.ZZ6_Concession;
							break;
						}
					}
				}
			}

			return result;
		}

		public string GetCPCDescriptionForDeclarationType(string concession)
		{
			var result = string.Empty;
			foreach (RefCusProcedure cpcCode in CPCCollection)
			{
				if (cpcCode.ZZ6_Concession == concession)
				{
					if (HasCofO)
					{
						if (cpcCode.HasAttribute(AttributeNames.Codes.ISCOO))
						{
							result = cpcCode.ZZ6_Description;
							break;
						}
					}
					else
					{
						if (!cpcCode.HasAttribute(AttributeNames.Codes.ISCOO))
						{
							result = cpcCode.ZZ6_Description;
							break;
						}
					}
				}
			}

			return result.Split('(').First().Trim();
		}

		#endregion

		#region Message Manager

		public IMessageManager SG4MessageManager
		{
			get { return sg4MessageManager ?? (sg4MessageManager = new SG4MessageManager(this)); }
		}
		IMessageManager sg4MessageManager;

		public bool IsXMLTradeNetMessageEnabled => SGCustomsDataRegistry.Instance.EnableXMLTradeNetMessaging.Value;

		#endregion

		#region Get Last Sent Messages

		public CUSDECEDIMessage GetLastSentMessage_PermitOnly()
		{
			CUSDECEDIMessage result = null;

			if (ActiveEntryHeaders.Count == 1 && ActiveEntryHeaders[0].Messages.LastOutgoingMessage != null && ActiveEntryHeaders[0].Messages.LastOutgoingMessage.EM_MessageText.Contains(CusdecConstants.UnhMessageTypeIdentifier))
			{
				CUSDECEDIMessage lastSentMessage = (CUSDECEDIMessage)ActiveEntryHeaders[0].Messages.LastOutgoingMessage;
				if (lastSentMessage != null)
				{
					if (lastSentMessage.EM_MessageSubType == CUSDECEDIMessage.Declaration || lastSentMessage.EM_MessageSubType == CUSDECEDIMessage.Amendment)
					{
						result = lastSentMessage;
					}
				}
			}

			return result;
		}

		public CUSDECEDIMessage GetLastSentMessage_RefundOnly()
		{
			CUSDECEDIMessage result = null;

			if (ActiveEntryHeaders.Count == 1 && ActiveEntryHeaders[0].Messages.LastOutgoingMessage != null && ActiveEntryHeaders[0].Messages.LastOutgoingMessage.EM_MessageText.Contains("CUSDEC"))
			{
				CUSDECEDIMessage lastSentMessage = (CUSDECEDIMessage)ActiveEntryHeaders[0].Messages.LastOutgoingMessage;
				if (lastSentMessage != null)
				{
					if (lastSentMessage.EM_MessageSubType == CUSDECEDIMessage.Refund)
					{
						result = lastSentMessage;
					}
				}
			}

			return result;
		}

		#endregion

		#region Shipment Synchroniser

		protected override Customs.Business.JobDeclarationSynchroniser GetNewShipmentSynchroniser()
		{
			return new JobDeclarationSynchroniser(this);
		}

		#endregion

		#region IDocumentSupportable Override

		protected override DocumentSupporter CreateNewDocumentSupporter()
		{
			return new JobDeclarationDocumentSupporter(this);
		}

		#endregion

		#region Merge

		protected override Customs.Business.MergeManager GetMergeManager()
		{
			return new MergeManager(this);
		}

		protected override bool DoMergeCore(Customs.Business.ISendsMessagesToCustoms notifier)
		{
			bool result = base.DoMergeCore(notifier);

			TotalPayableInfo.RefreshBinding();
			TotalCustomsValueInfo.RefreshBinding();
			TotalGSTPayableInfo.RefreshBinding();
			TotalDutyPayableInfo.RefreshBinding();
			TotalExcisePayableInfo.RefreshBinding();
			TotalOtherTaxPayableInfo.RefreshBinding();

			foreach (JobComInvoiceLine invoiceLine in FilteredInvoiceLines)
			{
				invoiceLine.JI_Calc_DutyAmountInfo.RefreshBinding();
				invoiceLine.JI_Calc_GSTVATAmountInfo.RefreshBinding();
				invoiceLine.JI_Calc_ExciseAmountInfo.RefreshBinding();
				invoiceLine.JI_Calc_OtherTaxAmountInfo.RefreshBinding();
				invoiceLine.JI_Calc_CIFInfo.RefreshBinding();
			}

			return result;
		}

		public ZInt MergedLinesCount
		{
			get { return CusEntryHeader != null ? CusEntryHeader.MergedLines.Count : 0; }
		}

		#endregion

		#region CusEntryHeader
		/// <summary>
		/// Contains the Current Active CusEntryHeader for this Declaration.
		/// </summary>
		public CusEntryHeader CusEntryHeader => Factory.GetValue(ref cusEntryHeaderProperty, () => ActiveEntryHeaders.OfType<CusEntryHeader>().FirstOrDefault());

		CachedProperty<CusEntryHeader> cusEntryHeaderProperty;

		#endregion

		#region Copy/Clone

		protected override void ResetValuesOnTemplateCopyAfterClone(BaseJobDeclaration declaration, CloneType cloneType)
		{
			base.ResetValuesOnTemplateCopyAfterClone(declaration, cloneType);
			JobDeclaration sgDeclaration = declaration as JobDeclaration;
			if (sgDeclaration != null)
			{
				sgDeclaration.SG_OutwardHAWB = "";
				sgDeclaration.SG_OutwardMAWB = "";
				sgDeclaration.SG_RemovalStartDate = ZDateTime.Empty;
				sgDeclaration.SG_EndDateTempImport = ZDateTime.Empty;
				DefaultJE_ApplicationCode();
				sgDeclaration.JE_GS_NKCusAgent = CurrentUserIsABroker ? GlbStaff.CurrentUser.GS_Code : ZString.Empty;
			}
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			JobDeclaration result = (JobDeclaration)base.CloneInternal(args);
			using (GetValidationSuspender())
			{
				using (result.SuspendSettingHasChanges())
				{
					DefaultJE_ApplicationCode();
					result.SG_ApplicationProductType = SG_ApplicationProductType;

					foreach (CALicenceNumber licenceNumber in CALicences)
					{
						result.CALicences.Add(licenceNumber.Clone());
					}

					foreach (TradersRemark remark in TradersRemarks)
					{
						result.TradersRemarks.Add(remark.Clone());
					}
				}
			}

			return result;
		}

		protected override ZBool CloneBills
		{
			get { return false; }
		}

		#endregion

		#region Validation

		protected override Customs.Business.JobDeclarationValidation GetNewValidation()
		{
			Customs.Business.JobDeclarationValidation result = null;

			if (JE_MessageType == MessageTypeCodeList.Codes.IPT)
			{
				result = new JobDeclarationValidation_IPT(this);
			}
			else if (IsOUTDEC)
			{
				result = new JobDeclarationValidation_OUT(this);
			}
			else if (JE_MessageType == MessageTypeCodeList.Codes.INP)
			{
				result = new JobDeclarationValidation_INP(this);
			}
			else if (JE_MessageType == MessageTypeCodeList.Codes.TNP)
			{
				result = new JobDeclarationValidation_TNP(this);
			}
			else if (JE_MessageType == MessageTypeCodeList.Codes.COO)
			{
				result = new COValidation(this);
			}
			else
			{
				result = new JobDeclarationValidation(this);
			}

			return result;
		}

		#endregion

		#region EDocs

		public IStorageDocsBaseCollection AllEDocs
		{
			get { return JE_JS != ZGuid.Empty ? Shipment.DocManagerInfo.AllEDocs : DocManagerInfo.AllEDocs; }
		}

		#endregion

		#region IApportionInvoiceHolder Members

		System.Collections.IComparer IApportionInvoiceHolder.ChargeComparer
		{
			get { return new ChargesComparer(); }
		}

		#endregion

		#region Fetch Hints

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new JobDeclarationFetchStrategy(this);
		}

		class JobDeclarationFetchStrategy : Customs.Business.FetchStrategies.BaseJobDeclarationFetchStrategy
		{
			public JobDeclarationFetchStrategy(JobDeclaration declaration)
				: base(declaration)
			{
			}

			protected override void FetchForValidateCore()
			{
				base.FetchForValidateCore();
				Factory.AddFetchHint(StmNoteSchema.ST_ParentID, BusinessObject.PK);
			}

			protected override void AddMergeFetchHintsAfterInvoiceLines()
			{
				base.AddMergeFetchHintsAfterInvoiceLines();
				foreach (JobComInvoiceLine invoiceLine in BusinessObject.InvoiceLines)
				{
					if (!invoiceLine.JI_Tariff.IsEmpty)
					{
						Factory.AddFetchHint(typeof(TariffView), TariffView.Loader.GetEffectiveTariffFilter(Factory, Core.Constants.CountryCodes.Singapore, Constants.TariffTypes.HarmonizedSystem, invoiceLine.JI_Tariff, invoiceLine.EffectiveDateForDutyRate));
					}
				}
			}

			protected override void AddMergeFetchHintsFor(BaseJobComInvoiceLine invoiceLine)
			{
				base.AddMergeFetchHintsFor(invoiceLine);
				Factory.AddFetchHint(CusCodeDataSchema.CY_ParentID, invoiceLine.PK);
				Factory.AddFetchHint(CusLineTariffDetailSchema.BZ_ParentID, invoiceLine.PK);
				Factory.AddFetchHint(StmDataSchema.SD_Owner, invoiceLine.PK);
				Factory.AddFetchHint(StmNoteSchema.ST_ParentID, invoiceLine.PK);
				if (!invoiceLine.JI_Tariff.IsEmpty)
				{
					Factory.AddFetchHint(typeof(TariffView), TariffView.Loader.GetEffectiveTariffFilter(Factory, Core.Constants.CountryCodes.Singapore, Constants.TariffTypes.HarmonizedSystem, invoiceLine.JI_Tariff, invoiceLine.EffectiveDateForDutyRate));
				}
			}
		}

		#endregion

		public class JobDeclarationInvoicingSupporter : BaseJobDeclarationInvoicingSupporter
		{
			public JobDeclarationInvoicingSupporter(JobDeclaration parent)
				: base(parent)
			{
			}

			protected new JobDeclaration Parent
			{
				get { return (JobDeclaration)base.Parent; }
			}

			protected override ZString JobInvoicingTransportMode
			{
				get
				{
					string transportMode = Parent.JE_TransportMode;
					if (Parent.JE_TransportMode.IsEmpty)
					{
						transportMode = Parent.SG_OutwardTransportMode;
					}

					return transportMode;
				}
			}

			protected override ZGuid OverridenDepartment
			{
				get
				{
					ZGuid departmentPK = ObjectFactory.Get<IAccounting>().CustomsOther;

					if (IsImport && !IsExport)
					{
						if (JobInvoicingTransportMode == TransportModeCodeList.Codes.TransportMode_4_Air)
						{
							departmentPK = ObjectFactory.Get<IAccounting>().CustomsImportAirUld;
						}
						else if (JobInvoicingTransportMode == TransportModeCodeList.Codes.TransportMode_1_SEA)
						{
							departmentPK = Parent.JE_ContainerCount > 0 ? ObjectFactory.Get<IAccounting>().CustomsImportSeaFcl : ObjectFactory.Get<IAccounting>().CustomsImportSeaLcl;
						}
						else if (JobInvoicingTransportMode == TransportModeCodeList.Codes.TransportMode_2_Rail)
						{
							departmentPK = ObjectFactory.Get<IAccounting>().CustomsImportRail;
						}
						else if (JobInvoicingTransportMode == TransportModeCodeList.Codes.TransportMode_3_Road)
						{
							departmentPK = ObjectFactory.Get<IAccounting>().CustomsImportRoad;
						}
						else if (JobInvoicingTransportMode == TransportModeCodeList.Codes.TransportMode_5_Mail)
						{
							departmentPK = ObjectFactory.Get<IAccounting>().CustomsImportPost;
						}
						else
						{
							departmentPK = ObjectFactory.Get<IAccounting>().CustomsImportOther;
						}
					}
					else if (IsExport && !IsImport)
					{
						if (JobInvoicingTransportMode == TransportModeCodeList.Codes.TransportMode_4_Air)
						{
							departmentPK = ObjectFactory.Get<IAccounting>().CustomsExportAirUld;
						}
						else if (JobInvoicingTransportMode == TransportModeCodeList.Codes.TransportMode_1_SEA)
						{
							departmentPK = Parent.JE_ContainerCount > 0 ? ObjectFactory.Get<IAccounting>().CustomsExportSeaFcl : ObjectFactory.Get<IAccounting>().CustomsExportSeaLcl;
						}
						else if (JobInvoicingTransportMode == TransportModeCodeList.Codes.TransportMode_2_Rail)
						{
							departmentPK = ObjectFactory.Get<IAccounting>().CustomsExportRail;
						}
						else if (JobInvoicingTransportMode == TransportModeCodeList.Codes.TransportMode_3_Road)
						{
							departmentPK = ObjectFactory.Get<IAccounting>().CustomsExportRoad;
						}
						else if (JobInvoicingTransportMode == TransportModeCodeList.Codes.TransportMode_5_Mail)
						{
							departmentPK = ObjectFactory.Get<IAccounting>().CustomsExportPost;
						}
					}
					return departmentPK;
				}
			}
		}

		public class JobMessageTypeList : Customs.Business.JobMessageTypeList
		{
			public new class Codes : Common.Shared.SharedJobMessageTypeList.Codes
			{
				public const string TradeNet4Point1 = "TN41";
			}
		}

		#region ICusAddInfoTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusAddInfoTypeSupporterFetchStrategy(this);
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
			yield return new Customs.Business.FetchStrategies.CusSupportingInfoTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(Enterprise.Customs.Business.MultiLineAddInfos.CusAddInfoTypeAttribute.Codes.SGCustomsProcedureCode, typeof(SGCPC));
			return result;
		}

		#endregion

		#region ICusCodeDataTypeSupporter Members

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusCodeDataTypeList.Codes.CALicenceNumber, typeof(CALicenceNumber));
			return result;
		}

		#endregion

		#region ICusSupportingInfoTypeSupporter Members

		IDictionary<ZString, Type> ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes()
		{
			var result = new Dictionary<ZString, Type>
			{
				{ CusSupportingInfoTypeList.Codes.TradersRemarks, typeof(TradersRemark) },
			};
			return result;
		}

		#endregion

		protected override DocAddressType[] SupportedAddressTypesCore
		{
			get
			{
				var sgDocAddressTypes = new DocAddressType[]
				{
					DocAddressType.CarrierHandlingAgent,
					DocAddressType.ClaimantAddress,
					DocAddressType.InwardCarrierAgent,
					DocAddressType.OutwardCarrierAgent
				};

				return base.SupportedAddressTypesCore.Concat(sgDocAddressTypes).ToArray();
			}
		}

		protected override Directions GetJobDirection()
		{
			if (IsTranshipment)
			{
				return Directions.CrossTrade;
			}

			return base.GetJobDirection();
		}

		public override void DefaultIncoTerm()
		{
			if (IsStandAlone)
			{
				base.DefaultIncoTerm();
			}
		}
	}
}
