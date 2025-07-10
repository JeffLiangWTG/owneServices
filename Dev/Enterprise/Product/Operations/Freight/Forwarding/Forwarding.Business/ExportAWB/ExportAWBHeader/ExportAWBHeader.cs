using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.DocumentEngine.ValueProviders;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineIntegration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Integration.AWB;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants.AWB;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public abstract partial class ExportAWBHeader : Forwarding.AWB.Business.ExportAWBHeader, IDocAddresses, ISourceIdentifierProvider
	{
		protected ExportAWBHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public abstract IAWBParent Parent { get; }

		public bool IsExportingData { get; private set; }

		public IDisposable SetIsExportingData()
		{
			var old = IsExportingData;
			return new DisposableAction(() => IsExportingData = true, () => IsExportingData = old);
		}

		public virtual ICollection<IAWBHeaderValidator> Validators
		{
			get
			{
				return new List<IAWBHeaderValidator>
				{
					new BangladeshDestAWBHeaderValidator(this),
					new BrazilImportAWBHeaderValidator(this),
					new ChinaImportAWBHeaderValidator(this),
					new ChinaTransitingAWBHeaderValidator(this),
					new EgyptDestAWBHeaderValidator(this),
					new ICS2ZoneImportAWBHeaderValidator(this),
					new MoroccoDestAWBHeaderValidator(this),
					new BoliviaImportAWBHeaderValidator(this),
					new BoliviaExportAWBHeaderValidator(this),
					new HondurasImportAWBHeaderValidator(this),
					new MauritiusExportAWBHeaderValidator(this),
					new MauritiusImportAWBHeaderValidator(this),
					new SwitzerlandGDRNumberAWBHeaderValidator(this),
					new TraderTypeNoAWBHeaderValidator(this)
				};
			}
		}

		public new static readonly ExportAWBHeaderTypeDecider TypeDecider = new ExportAWBHeaderTypeDecider();

		public new class Schema : Forwarding.AWB.Business.ExportAWBHeader.Schema
		{
			public const string EH_AirlineName = "EH_AirlineName";

			public const string EH_BillNumber = "EH_BillNumber";
			public const string EH_RateClassLabelText = "EH_RateClassLabelText";
			public const string EH_ConsigneeDefaultAddressPicker = "EH_ConsigneeDefaultAddressPicker";
			public const string EH_ShipperDefaultAddressPicker = "EH_ShipperDefaultAddressPicker";
			public const string EH_AlsoNotifyDefaultAddressPicker = "EH_AlsoNotifyDefaultAddressPicker";
			public const string DeclarationSecurityStatus = "DeclarationSecurityStatus";
		}

		#region Loader

		protected class AWBHeaderLoader<THeader, TParent>
			where THeader : ExportAWBHeader
			where TParent : BusinessObject, IAWBParent
		{
			public THeader LoadOrCreate(TParent parent)
			{
				Argument.NotNull(parent, "parent");

				var result = Load(parent) ?? Create(parent);

				return result;
			}

			THeader Load(TParent parent)
			{
				ZQuery query = new ZQuery(ExportAWBHeaderSchema.EH_ParentID, parent.PK);
				query.AddToFilter(ExportAWBHeaderSchema.EH_Table, parent.TableName);
				query.FetchOnlyFromLocalCache = !parent.IsInDatabase;

				return parent.Factory.LoadTop1<THeader>(query);
			}

			THeader Create(TParent parent)
			{
				THeader header = parent.Factory.New<THeader>();
				header.EH_Table = parent.TableName;
				header.EH_ParentID = parent.PK;

				return header;
			}
		}

		#endregion

		public override ZGuid EH_ParentID
		{
			get { return base.EH_ParentID; }
			set
			{
				if (base.EH_ParentID != value)
				{
					base.EH_ParentID = value;
					if (!IsDeleted && !EH_ParentID.IsEmpty && ShouldPopulate)
					{
						using (GetValidationSuspender())
						{
							Populate();
						}
					}
				}
			}
		}

		#region Populate

		public void Populate()
		{
			PopulateCore(populateAll: true);
		}

		public void PopulateIfNotOverridden()
		{
			PopulateCore(Parent != null && !IsAWBOverridden);
		}

		public void PopulateSecurityDeclarationIfNotOverridden()
		{
			if (OverrideSecurityDeclarationDefaults)
			{
				return;
			}

			using (SuspendSettingHasChanges())
			using (GetValidationSuspender())
			{
				PopulateSecurityDeclaration();
			}

			RefreshBinding();

			if (!IsValidationSuspended)
			{
				RunPreSaveValidationWithFetchHints();
			}
		}

		protected virtual void PopulateFollowOnAndDimensions()
		{
		}

		protected virtual void PopulateCore(bool populateAll)
		{
			using (SuspendSettingHasChanges())
			using (GetValidationSuspender())
			{
				if (populateAll)
				{
					issuedBy = null;
					borrowedMaster = null;

					EH_AWBOriginCode = OriginCode;
					EH_AirportOfDepartureAndRequestRouteText = AirportOfDeparture;
					EH_AirportOfDestinationText = AWBDestinationText;
					EH_AirportOfDestinationCode = AWBDestinationCode;
					EH_Booking1stCarrier = Booking1stCarrier;
					EH_Booking1stFlight = Booking1stFlight;
					EH_Booking1stFlightDate = Booking1stFlightDay;
					EH_Booking2ndCarrier = Booking2ndCarrier;
					EH_Booking2ndFlight = Booking2stFlight;
					EH_Booking2ndFlightDate = Booking2ndFlightDay;
					EH_To1st = To1st;
					EH_By1st = By1st;
					EH_To2nd = To2nd;
					EH_By2nd = By2nd;
					EH_To3rd = To3rd;
					EH_By3rd = By3rd;

					DocAddresses.RemoveAndDeleteAll();
					ResetAddressOverrides();
					PopulateShipperAddressDefaults();
					PopulateConsigneeAddressDefaults();
					PopulateAlsoNotifyAddressDefaults(NotifyPartyDocumentaryAddress);

					EH_OptionalShippingInformation = OptionalShippingInformation1.SubstringSafe(0,
						ExportAWBHeaderSchema.EH_OptionalShippingInformation.MaxLength);
					EH_OptionalShippingInformation2 = OptionalShippingInformation2.SubstringSafe(0,
						ExportAWBHeaderSchema.EH_OptionalShippingInformation2.MaxLength);

					EH_Currency = AWBCurrency;
					EH_ChargesCode = ChargesCode;
					EH_WeightVPPDCOL = WeightVPPDCOL;
					EH_OtherPPDCOL = OtherPPDCOL;
					EH_ExtraCarrierInfoLine2 = ExtraCarrierInfoLine2;
					EH_ShippersSignature = GetShippersSignature();
					EH_ExtraShipperInfoLine1 = ExtraShipperInfoLine1;

					PopulateIssueDate();
					EH_AWBIssuePlace = AWBIssuePlace;
					EH_AWBAgentsSignature = AWBAgentsSignature;
					EH_HandlingInformation = HandlingInformation.SubstringSafe(0, EH_HandlingInformationInfo.MaxLength);
					PopulateRateLinesRelatedProperties();
					EH_NetRateCode = "";
					EH_SpecialHandlingCode = SpecialHandlingCode;

					EH_CustomsValue = CustomsValue;
					EH_HouseCustomsValueCurrency = CustomsValueCurrency;
					EH_InsuranceValue = InsuranceValue;
					EH_HouseInsuranceValueCurrency = InsuranceValueCurrency;
					EH_DeclaredValue = DeclaredValue;
					EH_HouseDeclaredValueCurrency = DeclaredValueCurrency;

					EH_IsConsigneeDeclarantForAdvanceCargoReporting = HasInboundToICS2Zone ? IsConsigneeAdvanceCargoReportingSelfFilerSet : false;

					if (!AgentDetailsAreNotModifiable && GlbStaff.CurrentUser.GS_Code != User.ServiceUserCode)
					{
						SetupDefaultAgentDetails();
					}

					PopulateNatureAndQtyOfGoods();
					PopulateShortGoodsDescriptionforFHL();
					PopulateIssuedBy();
					PopulateRateLines();

					PopulateOtherCharges();
					PopulateAccountingInfo();
					PopulateSpecialHandlingItems();
					PopulateSLAC();
					PopulateTaxAmounts();
				}
				else if (!EH_AreRateLinesOverridden)
				{
					PopulateAllRateLinesSectionInfo();
				}

				PopulateFollowOnAndDimensions();

				PopulateSecurityDeclaration();

				if (populateAll)
				{
					PopulateExtraShipperInfoLine2();
				}
			}

			RefreshBinding();
#if DEBUG
			NotifyPopulated();
#endif

			if (!IsValidationSuspended)
			{
				RunPreSaveValidationWithFetchHints();
			}
		}

		#region Populate Security Declaration

		protected virtual void PopulateSecurityDeclaration()
		{
		}

		#endregion

		#region OnPopulated Event
#if DEBUG
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "Only run in testing")]
		[SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible", Justification = "Only run in testing")]
		public static EventHandler OnPopulated;

		void NotifyPopulated()
		{
			OnPopulated?.Invoke(this, EventArgs.Empty);
		}
#endif
		#endregion

		public virtual bool ShouldPopulateSlacLine(CommonContainer uldContainer) => false;

		public virtual bool ShouldSuppressSlacLines() => false;

		protected abstract bool ShouldPopulate { get; }

		#endregion

		#region ULDContainers

		public virtual IEnumerable<CommonContainer> ULDContainers
		{
			get { return Enumerable.Empty<CommonContainer>(); }
		}

		#endregion

		#region Job & Freight Charges

		protected virtual JobCharge[] GetJobCharges(JobHeader invoiceJobHeader)
		{
			var result = Array.Empty<JobCharge>();

			if (invoiceJobHeader != null)
			{
				var chargeFilter = new ZQuery(JobChargeSchema.JR_JH, invoiceJobHeader.PK);
				if (!ObjectFactory.Get<IAccounting>().ProfitShareChargeCode.IsEmpty)
				{
					chargeFilter.AddToFilter(JobChargeSchema.JR_AC, SQLComparisonOperator.NotEqual, ObjectFactory.Get<IAccounting>().ProfitShareChargeCode);
				}

				result = Factory.Load<JobCharge>(chargeFilter)
					.Where(x => !(x.IsRevenuePostedWithAutoJobRevenueJournal || x.IsRevenuePostedWithManualJobRevenueJournal))
					.ToArray();
			}

			return result;
		}

		public JobCharge[] GetFreightCharges(JobHeader invoiceJobHeader)
		{
			var jobCharges = GetJobCharges(invoiceJobHeader);

			return GetFreightCharges(jobCharges);
		}

		protected virtual JobCharge[] GetFreightCharges(JobCharge[] jobCharges)
		{
			return jobCharges
				.Where((c) =>
					c.ChargeCode != null
					&& c.ChargeCode.IsFreightChargeCode()
				).ToArray();
		}

		protected virtual ZDecimal GetAmountBasedOnCurrencyObject(
			JobHeader jobHeader,
			RefCurrency currencyObject,
			Money initialMonetaryAmount,
			ZGuid orgPK,
			CostSell costOrSell
		)
		{
			if (jobHeader != null)
			{
				Money convertedAmount;
				var converter = jobHeader.CurrencyConverter;

				if (converter is IJobExRateCurrencyConverter exRateCurrencyConverter)
				{
					using (new DisposableAction(() => jobHeader.Factory.SetContext(BusinessContext.ConvertingAmountsForExportAWBHeader), () => jobHeader.Factory.RemoveContext(BusinessContext.ConvertingAmountsForExportAWBHeader)))
					{
						convertedAmount = exRateCurrencyConverter.ConvertExact(initialMonetaryAmount, currencyObject, orgPK, costOrSell);
					}
				}
				else
				{
					convertedAmount = converter.ConvertExact(initialMonetaryAmount, currencyObject);
				}

				return convertedAmount.Amount;
			}

			return initialMonetaryAmount.Amount;
		}

		#endregion

		#region Nature and Quantity of Goods

		void PopulateNatureAndQtyOfGoods()
		{
			if (EH_AreRateLinesOverridden)
			{
				return;
			}

			ZStringBuilder text = new ZStringBuilder(GoodsDescription.Trim());

			RefCountry originCountry = OriginCountry;
			if (originCountry != null)
			{
				foreach (ExportStatementSetting mandatorySetting in FreightDataRegistry.Instance.ExportStatementSettings.Value.GetMandatoryStatements(originCountry.Code))
				{
					if (ShowExportStatementSetting(mandatorySetting))
					{
						text.AppendIfNotEmpty(mandatorySetting.Statement.Trim());
					}
				}
			}
			PopulateExportStatementIfRequired(text);

			ZString extraNatureAndQtyOfGoodsResult = ZString.Empty;
			AddExtraText(ExtraNatureAndQtyOfGoods.Value, ref extraNatureAndQtyOfGoodsResult, ExtraNatureAndQtyOfGoods);
			text.AppendIfNotEmpty(extraNatureAndQtyOfGoodsResult.Trim());

			NatureAndQtyOfGoods = TextToNatureAndQtyOfGoodsLines(text.ToStringWithDelimiterBetweenAppends(System.Environment.NewLine));
		}

		protected abstract StringRegistryItem ExtraNatureAndQtyOfGoods { get; }
		protected abstract bool ShowExportStatementSetting(ExportStatementSetting mandatorySetting);

		#endregion

		#region Short Goods Description for FHL

		protected virtual void PopulateShortGoodsDescriptionforFHL()
		{
		}

		#endregion

		#region SLAC

		protected virtual void PopulateSLAC()
		{
			if (EH_AreRateLinesOverridden)
			{
				return;
			}
			using (((ExportAWBRateLine)SLACLine).SuspendSettingHasChanges())
			{
				if (EH_ShippingLoadAndCount > 0)
				{
					SLACLine.NatureAndQtyOfGoodsDescription = EH_ShippingLoadAndCount + " " + Constants.SLAC;
				}
			}
		}

		#endregion

		#region Other Charges

		protected void PopulateOtherCharges()
		{
			suppressOtherChargesRefreshings = true;

			try
			{
				var chargeTemplates = GetOtherChargeTemplates();

				NonPersistentExportAWBOtherCharge[] charges = chargeTemplates
					.SelectMany(template => template.CreateCharges())
					.Where(charge => charge.Amount != 0)
					.ToArray();

				if (GroupChargesByIATACode)
				{
					charges = GroupChargesByIATAChargeCode(charges);
				}

				MergeWithExistingOtherCharges(GroupExcessiveChargesOnLastLineIfNecessary(charges));
			}
			finally
			{
				suppressOtherChargesRefreshings = false;
				RefreshOtherChargesData();
			}
		}

		void MergeWithExistingOtherCharges(NonPersistentExportAWBOtherCharge[] newCharges)
		{
			List<ExportAWBOtherCharges> unmatchedExistingCharges = new List<ExportAWBOtherCharges>();
			List<NonPersistentExportAWBOtherCharge> unmatchedNewCharges = new List<NonPersistentExportAWBOtherCharge>(newCharges);

			foreach (ExportAWBOtherCharges existingCharge in AWBOtherCharges)
			{
				NonPersistentExportAWBOtherCharge matchedNewCharge = unmatchedNewCharges
					.FirstOrDefault(newCharge => newCharge.Equals(existingCharge));

				if (matchedNewCharge != null)
				{
					unmatchedNewCharges.Remove(matchedNewCharge);
				}
				else
				{
					unmatchedExistingCharges.Add(existingCharge);
				}
			}

			using (AWBOtherCharges.SuspendSettingHasChanges())
			{
				foreach (ExportAWBOtherCharges unmatchedExistingCharge in unmatchedExistingCharges)
				{
					AWBOtherCharges.RemoveAndDelete(unmatchedExistingCharge);
				}

				foreach (NonPersistentExportAWBOtherCharge unmatchedNewCharge in unmatchedNewCharges)
				{
					ExportAWBOtherCharges charge = unmatchedNewCharge.ToPersistentCharge();
					using (charge.SuspendSettingHasChanges())
					{
						AWBOtherCharges.Add(charge);
					}
				}
			}
		}

		NonPersistentExportAWBOtherCharge[] GroupChargesByIATAChargeCode(NonPersistentExportAWBOtherCharge[] charges)
		{
			List<NonPersistentExportAWBOtherCharge> groupedChargesWithChargeCode = charges
				.Where(charge => charge.ChargeCode.Trim() != ZString.Empty)
				.GroupBy(charge => new
				{
					EO_ChargeCode = charge.ChargeCode,
					EO_EntitlementCode = charge.EntitlementCode,
					EO_PPDCLT = charge.PPDCLT
				})
				.Select(group => new NonPersistentExportAWBOtherCharge(this)
				{
					ChargeCode = group.Key.EO_ChargeCode,
					EntitlementCode = group.Key.EO_EntitlementCode,
					PPDCLT = group.Key.EO_PPDCLT,
					Amount = group.Sum(t => t.Amount)
				})
				.ToList();

			IEnumerable<NonPersistentExportAWBOtherCharge> chargesToPrefixWithChargeType = groupedChargesWithChargeCode
				.GroupBy(charge => new
				{
					EO_ChargeCode = charge.ChargeCode,
					EO_EntitlementCode = charge.EntitlementCode
				})
				.Where(group => group.Count() > 1)
				.SelectMany(group => group);

			foreach (var charge in chargesToPrefixWithChargeType)
			{
				if (!charge.ChargeDescription.StartsWith(charge.PPDCLT, StringComparison.Ordinal))
				{
					charge.ChargeDescription = ZString.Format("{0} / {1}", charge.PPDCLT, charge.ChargeDescription);
				}
			}

			groupedChargesWithChargeCode.AddRange(charges.Where(charge => charge.ChargeCode.Trim() == ZString.Empty));

			return groupedChargesWithChargeCode.ToArray();
		}

		NonPersistentExportAWBOtherCharge[] GroupExcessiveChargesOnLastLineIfNecessary(NonPersistentExportAWBOtherCharge[] charges)
		{
			var maxOtherCharges = AWBOtherCharges.MaxOtherChargesThatCouldFitOnPrintedAWB;

			if (charges.Any(otherCharge => otherCharge.ChargeCode.IsEmpty)
				|| (AWBType == TypeOfAWB.House
				&& !Env.Registry.Freight.AirWaybill.ShowChargeCodeForOtherChargesInHAWBScreen))
			{
				maxOtherCharges = AWBOtherCharges.MaxOtherChargesWithMissingChargeCodeThatCouldFitOnPrintedAWB;
			}

			if (charges.Length > maxOtherCharges)
			{
				var groupedCharges = charges
					.GroupBy(charge => new
					{
						EO_EntitlementCode = charge.EntitlementCode,
						EO_PPDCLT = charge.PPDCLT
					})
					.Select(group => new
					{
						EntitlementCode = group.Key.EO_EntitlementCode,
						PrepaidCollect = group.Key.EO_PPDCLT,
						Charges = group.Select(charge => charge).ToArray()
					})
					.OrderBy(group => group.Charges.Length)
					.ThenByDescending(group => group.EntitlementCode)
					.ToList();

				var result = new List<NonPersistentExportAWBOtherCharge>();

				for (int i = groupedCharges.Count - 1; i >= 0; i--)
				{
					var currentGroup = groupedCharges[i];
					int chargesLeft = groupedCharges.Sum(group => group.Charges.Length);
					int excessCharges = chargesLeft + result.Count - maxOtherCharges;

					if (excessCharges <= 0)
					{
						result.AddRange(currentGroup.Charges);
					}
					else if (excessCharges >= currentGroup.Charges.Length)
					{
						result.Add(MergeCharges(currentGroup.Charges, currentGroup.EntitlementCode, currentGroup.PrepaidCollect));
					}
					else
					{
						int chargesNeedToGroup = excessCharges + 1;
						result.Add(MergeCharges(currentGroup.Charges.Take(chargesNeedToGroup), currentGroup.EntitlementCode, currentGroup.PrepaidCollect));
						result.AddRange(currentGroup.Charges.Skip(chargesNeedToGroup));
					}

					groupedCharges.Remove(currentGroup);
				}

				result.Reverse();

				return result.ToArray();
			}

			return charges;
		}

		NonPersistentExportAWBOtherCharge MergeCharges(IEnumerable<NonPersistentExportAWBOtherCharge> charges, ZString entitlementCode, ZString prepaidCollect)
		{
			NonPersistentExportAWBOtherCharge mergedCharge = new NonPersistentExportAWBOtherCharge(this);
			mergedCharge.ChargeCode = Core.Constants.AWB.ChargeCodes.MB;
			mergedCharge.ChargeDescription = (NoResString)"Other Misc Charges";
			mergedCharge.EntitlementCode = entitlementCode;
			mergedCharge.PPDCLT = prepaidCollect;
			mergedCharge.Amount = charges.Sum(charge => charge.Amount);

			return mergedCharge;
		}

		protected abstract IEnumerable<OtherChargeTemplate> GetOtherChargeTemplates();
		protected abstract bool GroupChargesByIATACode { get; }

		#endregion

		#region Export Statement

		void PopulateExportStatementIfRequired(ZStringBuilder text)
		{
			var exportStatements = ExportStatements;
			if (exportStatements.Length > 0)
			{
				foreach (ZString exportStatement in exportStatements)
				{
					text.AppendIfNotEmpty(exportStatement.Trim());
				}
			}
		}

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		protected virtual ZString[] ExportStatements
		{
			get { return Array.Empty<ZString>(); }
		}

		#endregion

		#region Rate Lines

		protected bool WillFitInFreeSpace(int dimensionsLinesCount)
		{
			return LineNumberOfFirstEmptyNatureAndQtyOfGoods + dimensionsLinesCount <= Constants.NumberNatureAndDescriptionLines;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "IATA Text")]
		protected string GetDimensionText(PackLine packLine)
		{
			var result = string.Empty;

			if (packLine.JL_UnitOfDimension_List.ContainsCode(packLine.JL_UnitOfDimension))
			{
				const string times = "x";

				string dimensionUnit = Core.Constants.Length.Centimetres;

				if (packLine.JL_UnitOfDimension == Core.Constants.Length.Inches || packLine.JL_UnitOfDimension == Core.Constants.Length.Feet || packLine.JL_UnitOfDimension == Core.Constants.Length.Yards)
				{
					dimensionUnit = Core.Constants.Length.Inches;
				}

				ZDecimal length = Core.Constants.Length.Convert(packLine.JL_Length, packLine.JL_UnitOfDimension, dimensionUnit);
				ZDecimal width = Core.Constants.Length.Convert(packLine.JL_Width, packLine.JL_UnitOfDimension, dimensionUnit);
				ZDecimal height = Core.Constants.Length.Convert(packLine.JL_Height, packLine.JL_UnitOfDimension, dimensionUnit);

				result = "DIMS " +
						 length.ToString(0) +
						 times +
						 width.ToString(0) +
						 times +
						 height.ToString(0) +
						 " " +
						 dimensionUnit +
						 " " + times + " " +
						 packLine.JL_PackageCount;
			}

			return result;
		}

		#endregion

		#region Nature and Description of Goods

		public int LineNumberOfFirstEmptyNatureAndQtyOfGoods
		{
			get
			{
				int result = -1;
				for (int i = 0; i < AWBRateLines.Count; i++)
				{
					ExportAWBRateLine rateLine = AWBRateLines[i];
					if (result == -1 && rateLine.IsNatureAndQtyOfGoodsEmpty)
					{
						result = i;
					}
					else if (!rateLine.IsNatureAndQtyOfGoodsEmpty)
					{
						result = -1;

						if (rateLine.NatureAndQtyOfGoodsType == Core.Constants.AWB.NatureAndQtyOfGoodsTypes.LithiumBattery)
						{
							i += Math.Max(0, rateLine.NatureAndQtyOfGoodsLithiumBattery.WrappedDescriptions.Count - 1);
						}
					}
				}

				return result + 1;
			}
		}

		#endregion

		#region Volume and Dimensions

		protected abstract void SetVOLNatureAndQtyOfGoods();
		protected abstract void SetPKSNatureAndQtyOfGoods();
		protected abstract void SetDEFNatureAndQtyOfGoods();
		protected abstract void SetALLNatureAndQtyOfGoods();
		protected abstract void SetNDANatureAndQtyOfGoods();

		#endregion

		#region Security Status

		internal ZString GetULDAviationSecurityStatus(CommonContainer uldContainer)
		{
			var parentShipments = uldContainer.GetParentShipments();
			if (parentShipments.Any(x => x.AviationSecurity.HasUnknownInspectionTypeCode))
			{
				return FreightDataRegistry.AviationSecurity_Unknown_Code;
			}

			if (parentShipments.Any(x => !x.AviationSecurity.IsAllowedOnPassengerFlights()))
			{
				return AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;
			}

			return AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
		}

		#endregion

		#region EH_AgentApprovalNumber

		protected virtual bool EH_AgentApprovalNumber_ReadOnly
		{
			get { return IsSecurityDeclarationReadOnly; }
		}

		#endregion

		#region EH_RN_NKAgentApprovalCountryCode

		[List("Lookups.IssuingCountryList")]
		public override ZString EH_RN_NKAgentApprovalCountryCode
		{
			get { return base.EH_RN_NKAgentApprovalCountryCode; }
			set
			{
				if (value != EH_RN_NKAgentApprovalCountryCode)
				{
					base.EH_RN_NKAgentApprovalCountryCode = value;

					Validation.ValidateEH_AgentApprovalNumber();

					ExportAWBSecurityStatusLines.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region EH_AgentApprovalCategory

		[List("Lookups.AgentApprovalCategoryList")]
		public override ZString EH_AgentApprovalCategory
		{
			get { return base.EH_AgentApprovalCategory; }
			set { base.EH_AgentApprovalCategory = value; }
		}

		#endregion

		#region Declaration Security Status

		[ReadOnly(true)]
		public ZString DeclarationSecurityStatus
		{
			get { return declarationSecurityStatus; }
			set { SetNonPersistentPropertyValue(DeclarationSecurityStatusInfo, ref declarationSecurityStatus, value); }
		}

		ZString declarationSecurityStatus;

		public ZPropertyInfo DeclarationSecurityStatusInfo
		{
			get { return GetZPropertyInfo(Schema.DeclarationSecurityStatus); }
		}

		#endregion

		#region USA Security Statement

		bool IsOriginatedFromTSAListedCountry
		{
			get
			{
				return
					Consol != null && (Consol.Transports.Cast<Transport>().Any(transport => IsPortCodeInTSACounrtyList(transport.JW_RL_NKLoadPort))
					|| Consol.Shipments.Cast<ForwardingShipment>()
						.Any(
							shipment =>
								IsPortCodeInTSACounrtyList(shipment.JS_RL_NKOrigin)
								|| shipment.Transports.Cast<Transport>().Any(transport => IsPortCodeInTSACounrtyList(transport.JW_RL_NKLoadPort))));
			}
		}

		bool HasUSOrUSTerritoryDestination
		{
			get
			{
				return Consol != null
					&& ((DestinationLOCO != null && Core.Constants.CountryCodes.IsUsaOrTerritory(DestinationLOCO.RL_RN_NKCountryCode)
						|| Consol.Transports.Cast<Transport>().Any(transport =>
							transport.JW_TransportMode == Core.Constants.TransportModes.Air
							&& transport.DiscPort != null
							&& Core.Constants.CountryCodes.IsUsaOrTerritory(transport.DiscPort.RL_RN_NKCountryCode))));
			}
		}

		bool IsPortCodeInTSACounrtyList(string portCode)
		{
			var registryCountries = FreightDataRegistry.Instance.AWBCountriesOfOriginOrTranshipmentInUSAAndUSTerritoriesSecurityStatement.Value;

			if (portCode.Length < 2 || registryCountries.IsNullOrEmpty())
			{
				return false;
			}

			var portCountry = Factory.Load<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, portCode.Substring(0, 2))).FirstOrDefault();

			return portCountry != null && registryCountries.Contains(portCountry.PK.ToGuid());
		}

		public override ZString TSASecurityStatement
		{
			get
			{
				var result = ZString.Empty;

				if (Consol != null && HasUSOrUSTerritoryDestination && !IsOriginatedFromTSAListedCountry)
				{
					var macro = FreightDataRegistry.Instance.AWBSecurityStatementTextToUSAAndUSTerritories.Value;
					result = new MacroStringReplacer(new DataProviderList(GetNewExtraTextMacroDataProvider())).ReplaceMacros(macro);
				}

				return result;
			}
		}

		public ZString AdditionalSecurityInformation
		{
			get
			{
				var result = ZString.Empty;

				if (Consol != null)
				{
					result = TSASecurityStatement;

					var note = Consol.Notes.FindByDescription(PredefinedNoteTypes.Instance.AdditionalSecurityInformation.Description).FirstOrDefault();

					if (note != null && !note.ST_NoteText.IsEmpty)
					{
						if (!result.IsEmpty)
						{
							result += System.Environment.NewLine + System.Environment.NewLine;
						}

						result += note.ST_NoteText;
					}
				}

				return result;
			}
		}

		#endregion

		#region Related Business Objects

		#region Accounting Information

		public new ExportAWBAccountingInformationCollection AWBAccountingInformations
		{
			get { return (ExportAWBAccountingInformationCollection)base.AWBAccountingInformations; }
		}

		protected override Forwarding.AWB.Business.ExportAWBAccountingInformationCollection GetNewAWBAccountingInformations()
		{
			return new ExportAWBAccountingInformationCollection(this, Factory);
		}

		void PopulateAccountingInfo()
		{
			List<ExportAWBAccountingInformation> populatedInfos = new List<ExportAWBAccountingInformation>();
			ZByte sequence = 0;

			foreach (ICodeDescription pair in ExtraAccountingInfo.Value)
			{
				ZString code = ZString.Empty;
				ZString extraTextDescription = ZString.Empty;

				AddExtraText(pair.Code, ref code, ExtraAccountingInfo);
				AddExtraText(pair.Description, ref extraTextDescription, ExtraAccountingInfo);

				string[] splitDescriptions = ((string)extraTextDescription).Replace("\r\n", "\n").Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
				splitDescriptions = splitDescriptions.Length > 0 ? splitDescriptions : new[] { string.Empty };

				foreach (ZString splitDescription in splitDescriptions)
				{
					sequence++;

					if (!code.IsEmpty || !splitDescription.IsEmpty)
					{
						code = code.SubstringSafe(0, ExportAWBAccountingInformationSchema.EA_InformationID.MaxLength);
						ZString description = splitDescription.SubstringSafe(0, ExportAWBAccountingInformationSchema.EA_Information.MaxLength);

						ExportAWBAccountingInformation populatedInfo = AWBAccountingInformations.Cast<ExportAWBAccountingInformation>().FirstOrDefault(x => x.EA_Sequence == sequence && x.EA_InformationID == code && x.EA_Information.Trim() == description.Trim());

						if (populatedInfo == null)
						{
							populatedInfo = AWBAccountingInformations.AddNew();
							using (populatedInfo.SuspendSettingHasChanges())
							{
								populatedInfo.EA_Sequence = sequence;
								populatedInfo.EA_InformationID = code;
								populatedInfo.EA_Information = description;
							}
						}

						populatedInfos.Add(populatedInfo);
					}
				}
			}

			using (AWBAccountingInformations.SuspendSettingHasChanges())
			{
				for (int i = AWBAccountingInformations.Count - 1; i >= 0; i--)
				{
					if (!AWBAccountingInformations[i].IsItalianRegistrationCode && !populatedInfos.Contains(AWBAccountingInformations[i]))
					{
						AWBAccountingInformations.RemoveAndDelete(AWBAccountingInformations[i]);
					}
				}
			}

			AWBAccountingInformations.Sort(ExportAWBAccountingInformationSchema.EA_Sequence.Name);
			PopulateItalyCodes();
		}

		void PopulateItalyCodes()
		{
			if (GlbCompany.CurrentCompany.Country != null && GlbCompany.CurrentCompany.Country.RN_Code == Enterprise.Core.Constants.CountryCodes.Italy)
			{
				var informations = AWBAccountingInformations.Cast<ExportAWBAccountingInformation>();

				ZString iva = IssuedBy.ItalianIVA;
				var iVArow = informations.FirstOrDefault(info => info.EA_InformationID == ExportAWBAccountingInformationLookups.IssuedByIVA);

				if (!iva.IsEmpty && (iVArow == null || iVArow.EA_Information != iva))
				{
					iVArow = iVArow ?? AWBAccountingInformations.AddNew();
					using (iVArow.SuspendSettingHasChanges())
					{
						iVArow.EA_InformationID = ExportAWBAccountingInformationLookups.IssuedByIVA;
						iVArow.EA_Information = iva;
					}
				}
				else if (iva.IsEmpty && iVArow != null)
				{
					iVArow.Delete();
				}

				ZString shipperCodiceFiscaleOrIVA = ZString.Empty;

				if (Shipper != null)
				{
					shipperCodiceFiscaleOrIVA = Shipper.CustomsCodes.GetCustomsRegNo(ItalyOrgCusCodeInfo.OrgCusCodes.CodiceFiscale, Core.Constants.CountryCodes.Italy);
					if (shipperCodiceFiscaleOrIVA.IsEmpty)
					{
						shipperCodiceFiscaleOrIVA = Shipper.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.IVA, Core.Constants.CountryCodes.Italy);
					}
				}

				var sIVrow = informations.FirstOrDefault(info => info.EA_InformationID == ExportAWBAccountingInformationLookups.ShipperCodiceFiscaleOrIVA);

				if (!shipperCodiceFiscaleOrIVA.IsEmpty && (sIVrow == null || sIVrow.EA_Information != shipperCodiceFiscaleOrIVA))
				{
					sIVrow = sIVrow ?? AWBAccountingInformations.AddNew();
					using (sIVrow.SuspendSettingHasChanges())
					{
						sIVrow.EA_InformationID = ExportAWBAccountingInformationLookups.ShipperCodiceFiscaleOrIVA;
						sIVrow.EA_Information = shipperCodiceFiscaleOrIVA;
					}
				}
				else if (shipperCodiceFiscaleOrIVA.IsEmpty && sIVrow != null)
				{
					sIVrow.Delete();
				}
			}
		}

		protected abstract CodeDescriptionPairListRegistryItem ExtraAccountingInfo { get; }

		#endregion

		#region Other Charges

		public new ExportAWBOtherChargesCollection AWBOtherCharges
		{
			get { return (ExportAWBOtherChargesCollection)base.AWBOtherCharges; }
		}

		#endregion

		#region Rate Lines

		public new ExportAWBRateLineCollection AWBRateLines
		{
			get { return (ExportAWBRateLineCollection)base.AWBRateLines; }
		}

		protected override Forwarding.AWB.Business.ExportAWBRateLineCollection GetNewAWBRateLines()
		{
			return new ExportAWBRateLineCollection(this);
		}

		public int LineNumberOfFirstEmptyRateLine
		{
			get
			{
				int result = -1;
				for (int i = 0; i < AWBRateLines.Count; i++)
				{
					ExportAWBRateLine rateLine = AWBRateLines[i];
					if (result == -1 && rateLine.IsRateDescriptionEmpty)
					{
						result = i;
					}
					else if (!rateLine.IsRateDescriptionEmpty)
					{
						result = -1;
					}
				}

				return result + 1;
			}
		}

		void PopulateRateLinesRelatedProperties()
		{
			if (EH_AreRateLinesOverridden)
			{
				return;
			}

			EH_AsAgreed1st = AsAgreed1st;
			EH_AsAgreed2nd = AsAgreed2nd;
			EH_ShippingLoadAndCount = ShippingLoadAndCount;
		}

		void PopulateRateLines()
		{
			if (EH_AreRateLinesOverridden)
			{
				return;
			}

			ResetCalculationLogsAnalyzer();
			ClearAllRateLines();

			int numberOfRateLinesPopulated = CalculationLogsAnalyzer.PopulateRateLines();

			bool[] rateLinesPopulated = new bool[Constants.NumberOfRateLines];
			for (int i = 0; i < numberOfRateLinesPopulated; i++)
			{
				rateLinesPopulated[i] = true;
			}

			if (numberOfRateLinesPopulated == 0)
			{
				PopulateInitialRateLines(rateLinesPopulated);
				PopulateMandatoryRateLines(rateLinesPopulated);
				PopulateAdditionalRateLines(rateLinesPopulated);
			}
			else
			{
				PopulateMandatoryRateLines(rateLinesPopulated);
			}

			RateLinesFinalRefinement();
		}

		protected virtual void RateLinesFinalRefinement()
		{
			SetHarmonizedCode();
			SetVolumeAndDimensions();
		}

		protected virtual void PopulateInitialRateLines(bool[] linesPopulated)
		{
			var rateLine = AWBRateLines[ExportAWBRateLine.Schema.ER_LineCount, (ZByte)1];
			using (rateLine.SuspendSettingHasChanges())
			{
				rateLine.Clear();
				rateLine.ER_NoOfPiecesOrRCP = RateLineNoPieces;
				rateLine.ER_RateClass = RateClass;
				rateLine.ER_CommodityItemNumber = CommodityItemNumber;
				rateLine.ER_WeightInLBsOrKGs = RateLineWeightUnit;
				rateLine.ER_GrossWeight = RateLineGrossWeight;
				rateLine.ER_ChargeableWeight = RateLineChargeableWeight;
				rateLine.ER_RateChargeOrDiscount = RateLineRateChargeOrDiscount;
				if (rateLine.ER_Total == 0M || rateLine.ER_RateChargeOrDiscount == 0M)
				{
					rateLine.ER_Total = RateLineTotal;
				}
			}

			linesPopulated[0] = true;
		}

		protected virtual void PopulateMandatoryRateLines(bool[] linesPopulated)
		{
		}

		protected virtual void PopulateAdditionalRateLines(bool[] linesPopulated)
		{
		}

		void ClearAllRateLines()
		{
			for (int i = 0; i < Constants.NumberOfRateLines; i++)
			{
				var rateLine = (ExportAWBRateLine)GetRateLineAt(i + 1);
				if (rateLine != null)
				{
					using (rateLine.SuspendSettingHasChanges())
					{
						rateLine.Clear();
					}
				}
			}
		}

		protected abstract string VolumeAndDimensionsPrintOption { get; }

		protected virtual void SetVolumeAndDimensions()
		{
			switch (VolumeAndDimensionsPrintOption)
			{
				case Core.Constants.AWB.Dimensions.M3:
					SetVOLNatureAndQtyOfGoods();
					break;

				case Core.Constants.AWB.Dimensions.PKS:
					SetPKSNatureAndQtyOfGoods();
					break;

				case Core.Constants.AWB.Dimensions.DEF:
					SetDEFNatureAndQtyOfGoods();
					break;
				case Core.Constants.AWB.Dimensions.NDA:
					SetNDANatureAndQtyOfGoods();
					break;

				default:
					SetALLNatureAndQtyOfGoods();
					break;
			}
		}

		public CalculationLogsAnalyzer CalculationLogsAnalyzer
		{
			get { return calculationLogAnalyzer ?? (calculationLogAnalyzer = GetCalculationLogsAnalyzer()); }
		}
		CalculationLogsAnalyzer calculationLogAnalyzer;

		protected virtual CalculationLogsAnalyzer GetCalculationLogsAnalyzer()
		{
			return new CalculationLogsAnalyzer(this);
		}

		void ResetCalculationLogsAnalyzer()
		{
			calculationLogAnalyzer = null;
		}

		#endregion

		#region Special Handling Codes

		public new ExportAWBSpecialHandlingCollection AWBSpecialHandlingItems
		{
			get { return (ExportAWBSpecialHandlingCollection)base.AWBSpecialHandlingItems; }
		}

		protected override Forwarding.AWB.Business.ExportAWBSpecialHandlingCollection GetNewAWBSpecialHandlingItems()
		{
			return new ExportAWBSpecialHandlingCollection(this);
		}

		#endregion

		protected abstract void PopulateSpecialHandlingItems();

		#endregion

		#region Properties

		[BusinessObjectTestExclude()]
		[ReadOnly(true)]
		public override ZString EH_ReferenceNumber
		{
			get { return ReferenceNumber; }
		}

		#region EH_BillNumber

		public ZString EH_BillNumber
		{
			get { return BillNumber; }
		}

		public ZPropertyInfo EH_BillNumberInfo
		{
			get { return GetZPropertyInfo(Schema.EH_BillNumber); }
		}

		#endregion

		#region EH_GS_NKSecurityStatusIssuedByCode

		public override ZString EH_GS_NKSecurityStatusIssuedByCode
		{
			get { return base.EH_GS_NKSecurityStatusIssuedByCode; }
			set
			{
				if (base.EH_GS_NKSecurityStatusIssuedByCode != value)
				{
					using (((ISingleElementListInternal)this).SuspendListChanged())
					{
						base.EH_GS_NKSecurityStatusIssuedByCode = value;
						if (SecurityStatusIssuedBy != null)
						{
							EH_SecurityStatusIssuedBy = SecurityStatusIssuedBy.GS_FullName.Left(EH_SecurityStatusIssuedByInfo.MaxLength);
						}
						else
						{
							EH_SecurityStatusIssuedBy = ZString.Empty;
						}
					}

					EH_GS_NKSecurityStatusIssuedByCodeInfo.RefreshBinding();
					EH_SecurityStatusIssuedByInfo.RefreshBinding();
				}
			}
		}

		public GlbStaff SecurityStatusIssuedBy => SecurityStatusIssuedByCode;

		protected virtual bool EH_SecurityStatusIssuedBy_ReadOnly
		{
			get
			{
				return !EH_GS_NKSecurityStatusIssuedByCode.IsEmpty;
			}
		}

		#endregion

		#region EH_AdditionalSecurityInformation

		public override ZString EH_AdditionalSecurityInformation
		{
			get { return base.EH_AdditionalSecurityInformation; }
			set
			{
				if (base.EH_AdditionalSecurityInformation != value)
				{
					base.EH_AdditionalSecurityInformation = value;
				}
			}
		}

		#endregion

		#region Additional Security Information

		public override List<ZString> AdditionalSecurityInformations => SupplyChainSecurityConfiguration.GetAdditionalSecurityInformations(Consol).ToList();

		#endregion

		#region Scheduled Arrival Date

		public override bool ShouldSetScheduledArrivalDate => SupplyChainSecurityConfiguration.ShouldSetScheduledArrivalDate(Consol);

		public override ZDateTimeOffset EH_ScheduledArrivalDate
		{
			get { return base.EH_ScheduledArrivalDate; }
			set
			{
				if (base.EH_ScheduledArrivalDate != value)
				{
					base.EH_ScheduledArrivalDate = value;
				}
			}
		}

		protected bool EH_ScheduledArrivalDate_ReadOnly
		{
			get
			{
				return IsSecurityDeclarationReadOnly ||
					EH_SecurityStatus != AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft ||
					!(GlbCompany.CurrentCompany.OrgProxy != null
					&& GlbCompany.CurrentCompany.OrgProxy.Addresses != null
					&& GlbCompany.CurrentCompany.OrgProxy.Addresses.Cast<OrgAddress>()
					.Any(x => x != null
					&& x.KnownShipper != null
					&& x.KnownShipper.OV_EXApprovedOrMajorExporter == AviationSecuritySchemeMembership.Codes.RegulatedAgent));
			}
		}

		#endregion

		#region Security Statement

		[List("Lookups.AdditionalSecurityStatementList")]
		public override ZString EH_AdditionalSecurityInformationStatement
		{
			get { return base.EH_AdditionalSecurityInformationStatement; }
			set
			{
				if (base.EH_AdditionalSecurityInformationStatement != value)
				{
					base.EH_AdditionalSecurityInformationStatement = value;
				}
			}
		}

		public override bool ShouldSetSecurityStatement => SupplyChainSecurityConfiguration.ShouldSetSecurityStatement;

		#endregion

		#region Movement Reference Numbers

		protected ZString GetMovementCode(ForwardingShipment shipment)
		{
			if (shipment.Origin != null
				&& shipment.Origin.Country != null
				&& shipment.Origin.Country.IsPartOfEuropeanUnion)
			{
				return MovementReferenceCode.Codes.CustomsExport;
			}

			if (shipment.Destination != null
				&& shipment.Destination.Country != null
				&& shipment.Destination.Country.IsPartOfEuropeanUnion)
			{
				return MovementReferenceCode.Codes.CustomsImport;
			}

			return MovementReferenceCode.Codes.CustomsTransit;
		}

		protected ZString GetNonEUMovementCode(ForwardingShipment shipment, string countryCode)
		{
			if (shipment.Origin != null
				&& shipment.Origin.Country != null
				&& shipment.Origin.Country.Code == countryCode)
			{
				return MovementReferenceCode.Codes.CustomsExport;
			}

			if (shipment.Destination != null
				&& shipment.Destination.Country != null
				&& shipment.Destination.Country.Code == countryCode)
			{
				return MovementReferenceCode.Codes.CustomsImport;
			}

			return MovementReferenceCode.Codes.CustomsTransit;
		}

		protected List<ZString> GetAllMovementReferenceNumbersWithoutLeadingTypeCode(ForwardingShipment shipment)
		{
			var cusEntryNumbers = shipment.CusEntryNumbers
				.Where(x => x.CE_EntryType == CusEntryNumberTypes.Standard.MovementReferenceNumber)
				.ToList();

			var numbers = new List<ZString>();

			foreach (var entryNumber in cusEntryNumbers)
			{
				if (!entryNumber.CE_EntryNum.IsEmpty)
				{
					numbers.Add(entryNumber.CE_EntryNum.StartsWith(CusEntryNumberTypes.Standard.MovementReferenceNumber, StringComparison.Ordinal)
						? entryNumber.CE_EntryNum.SubstringSafe(CusEntryNumberTypes.Standard.MovementReferenceNumber.Length)
						: entryNumber.CE_EntryNum
						);
				}
			}

			return numbers;
		}

		#endregion

		public bool HasOtherScreeningMethod
		{
			get
			{
				return CargoSecurityScreeningMethods
					.Cast<ExportAWBSecurityStatusLine>()
					.Any(line => line.EAS_ScreeningMethod == ScreeningMethods.Codes.SubjectedToAnyOtherMeans);
			}
		}

		public override ZString EH_WeightPrepaidCollect
		{
			get { return OverrideWaybillDefaults ? base.EH_WeightPrepaidCollect : WeightVPPDCOL.Left(1); }
			set { base.EH_WeightPrepaidCollect = value; }
		}

		public override ZString EH_OtherPrepaidCollect
		{
			get { return OverrideWaybillDefaults ? base.EH_OtherPrepaidCollect : OtherPPDCOL.Left(1); }
			set { base.EH_OtherPrepaidCollect = value; }
		}

		protected override ZString CustomsEntryNumber
		{
			get
			{
				var formattedNumbers = CustomsEntryNumbers
					.Select(num => ZString.Format("{0}: {1}", num.Type, num.Number));

				return formattedNumbers.Count() > 4 ? string.Empty : string.Join(",\r\n", formattedNumbers);
			}
		}

		protected IEnumerable<EntryNumber> CreateCustomsEntryNumbers(IEnumerable<CusEntryNumber> collection)
		{
			return collection
				.Select(CreateCustomsEntryNumber)
				.Where(num => num != null);
		}

		EntryNumber CreateCustomsEntryNumber(CusEntryNumber number)
		{
			if (number == null)
			{
				return null;
			}

			EntryNumber result = null;

			var entryType = CusEntryNumberTypes.UserFriendlyEntryType(number.CE_EntryType);

			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Australia)
			{
				if (entryType == CusEntryNumberTypes.Australia.CRN)
				{
					entryType = CusEntryNumberTypes.Australia.CAN;
				}
				else
				{
					string cmrCode = CMRExportExemptionCodes.Get4CharCode(entryType);

					if (new CMRExportExemptionCodesList().ContainsCode(cmrCode))
					{
						result = new EntryNumber
						{
							Type = CusEntryNumberTypes.Australia.CAN,
							Number = cmrCode
						};
					}
				}
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.UnitedStates && entryType == CusEntryNumberTypes.UnitedStates.ITN)
			{
				entryType = CusEntryNumberTypes.UnitedStates.AES;
			}

			if (result == null && !number.CE_EntryNum.IsEmpty)
			{
				result = new EntryNumber
				{
					Type = entryType,
					Number = number.CE_EntryNum
				};
			}

			return result;
		}

		#region EH_AgentApprovedExporterNumber

		public override ZString EH_SecurityStatusForNonBorrowedMAWBs
		{
			get
			{
				var securityStatus = IsBorrowedMaster
					? ZString.Empty
					: EH_SecurityStatus;

				return securityStatus == AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft
					&& SecurityStatusAWBVisibility
						? ZString.Empty
						: securityStatus;
			}
		}

		public virtual bool SecurityStatusAWBVisibility
		{
			get { return false; }
		}

		#endregion

		#region Airline

		RefAirline Airline
		{
			get
			{
				if (airline == null)
				{
					if (!EH_By1st.IsEmpty)
					{
						airline = RefAirline.LoadFromAirline2LetterCode(Factory, EH_By1st);
					}

					if (airline == null && !EH_AirlinePrefix.IsEmpty)
					{
						airline = RefAirline.LoadFromAirlinePrefix(Factory, EH_AirlinePrefix);
					}
				}

				return airline;
			}
		}
		RefAirline airline;

		#region Airline Name

		public ZString EH_AirlineName
		{
			get
			{
				RefAirline airline = Airline;
				return airline != null ? airline.RM_AirlineName1 : ZString.Empty;
			}
		}

		#endregion

		#region Airline Short Name

		public ZString EH_AirlineShortName
		{
			get
			{
				RefAirline airline = Airline;
				return airline != null ? airline.RM_LabelShortName : ZString.Empty;
			}
		}

		#endregion

		#region Airline Default Identifier for Consignee/Notify Party Contact Name

		public ZString EH_AirlineDefaultIdentifierForCneNfyName
		{
			get
			{
				return Airline?.RM_ContactNameOCIIdentifier ?? ZString.Empty;
			}
		}

		#endregion

		#region Airline Default Identifier for Consignee/Notify Party Contact Phone

		public ZString EH_AirlineDefaultIdentifierForCneNfyPhone
		{
			get
			{
				return Airline?.RM_ContactPhoneOCIIdentifier ?? ZString.Empty;
			}
		}

		#endregion

		#endregion

		#region Tax Calculation

		void PopulateTaxAmounts()
		{
			CalculateTotalsForTax();
			EH_TaxesPPD = autoCalculatedTotalPPDTax;
			EH_TaxesCOL = autoCalculatedTotalCOLTax;
		}

		decimal autoCalculatedTotalPPDTax;
		decimal autoCalculatedTotalCOLTax;

		void CalculateTotalsForTax()
		{
			autoCalculatedTotalPPDTax = 0;
			autoCalculatedTotalCOLTax = 0;

			if (IsTaxAutoCalculated)
			{
				var chargeTemplates = GetOtherChargeTemplates();
				foreach (OtherChargeTemplate charge in chargeTemplates)
				{
					if (charge.IsValid)
					{
						if (charge.PrepaidCollect == Constants.PrepaidCollect3CharCodes.Prepaid)
						{
							autoCalculatedTotalPPDTax += charge.TaxAmount;
						}
						else if (charge.PrepaidCollect == Constants.PrepaidCollect3CharCodes.Collect)
						{
							autoCalculatedTotalCOLTax += charge.TaxAmount;
						}
					}
				}

				var freightTaxes = CalculateTotalsForTaxFromFreightCharges();

				autoCalculatedTotalPPDTax += freightTaxes.PrepaidTax;
				autoCalculatedTotalCOLTax += freightTaxes.CollectTax;
			}
		}

		protected abstract FreightTaxes CalculateTotalsForTaxFromFreightCharges();

		[DecimalPlaces(2)]
		public override ZDecimal EH_TaxesPPD
		{
			get { return base.EH_TaxesPPD; }
		}

		[DecimalPlaces(2)]
		public override ZDecimal EH_TaxesCOL
		{
			get { return base.EH_TaxesCOL; }
		}

		protected abstract ZBool IsDomestic { get; }

		protected bool IsTaxAutoCalculated
		{
			get
			{
				if (OverrideWaybillDefaults)
				{
					return false;
				}
				else
				{
					return (IsDomestic) ? Env.Registry.Freight.AirWaybill.AllowAutoCalculationOfTax :
							FreightDataRegistry.Instance.AllowAutoCalculationOfInternationalTax.Value;
				}
			}
		}

		#endregion

		#region EH_RateClassLabelText

		public virtual ZString EH_RateClassLabelText
		{
			get { return Res.GetString("ExportAWBHeader|RateClassLabelText", "Rate Class"); }
		}

		public ZPropertyInfo EH_RateClassLabelTextInfo
		{
			get { return GetZPropertyInfo(Schema.EH_RateClassLabelText); }
		}

		#endregion

		public override ZInt EH_ShippingLoadAndCount
		{
			get { return base.EH_ShippingLoadAndCount; }
			set
			{
				base.EH_ShippingLoadAndCount = value;
				PopulateSLAC();
			}
		}

		#region ExportAWBSecurityStatusLines

		public new ExportAWBSecurityStatusLineCollection ExportAWBSecurityStatusLines
		{
			get { return (ExportAWBSecurityStatusLineCollection)base.ExportAWBSecurityStatusLines; }
		}

		protected override Forwarding.AWB.Business.ExportAWBSecurityStatusLineCollection GetNewExportAWBSecurityStatusLines()
		{
			return new ExportAWBSecurityStatusLineCollection(this);
		}

		#endregion

		#region Security Declaration Read Only

		protected bool IsSecurityDeclarationReadOnly => !OverrideSecurityDeclarationDefaults;

		#endregion

		#region Defaulting Data

		protected virtual ZString RateClass
		{
			get
			{
				ZDecimal weightInKilograms = RateLineChargeableWeight;
				if (RateLineWeightUnit == Core.Constants.AWB.RateLineUQ.Pounds)
				{
					weightInKilograms = Core.Constants.Weight.Convert(RateLineChargeableWeight, Core.Constants.Weight.Pounds, Core.Constants.Weight.Kilograms);
				}
				return weightInKilograms >= 45m ? Core.Constants.AWB.RateClass.QuantityRate : Core.Constants.AWB.RateClass.NormalCharge;
			}
		}

		protected virtual ZString CommodityItemNumber => ZString.Empty;

		protected virtual ZString AWBIssuePlace
		{
			get
			{
				return GlbBranch.CurrentBranch.GB_City.Left(17);
			}
		}

		protected virtual ZString AWBAgentsSignature => GetIssuingAgentName(AgentsSignatureMaxLength);

		public virtual int AgentsSignatureMaxLength
		{
			get { return ExportAWBHeaderSchema.EH_AWBAgentsSignature.MaxLength; }
		}

		string GetProperUserName(GlbStaff user, int length)
		{
			var userName = user.GS_FullName.ToString();

			if (length <= 0 || string.IsNullOrWhiteSpace(userName))
			{
				return string.Empty;
			}

			var array = userName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			var firstWord = array.First();
			var lastWord = array.Last();

			if (array.Length == 1)
			{
				return firstWord.Length > length ? firstWord.Substring(0, 1) : firstWord;
			}

			var matchWords = new[]
			{
				string.Join(" ", array),
				firstWord + " " + lastWord,
				firstWord.Substring(0, 1) + " " + lastWord,
				firstWord,
				firstWord.Substring(0, 1)
			};

			return matchWords.FirstOrDefault(c => c.Length <= length);
		}

		public virtual ZString GetShippersSignature(GlbStaff user = null)
		{
			if (user == null)
			{
				user = GlbStaff.CurrentUser;
			}

			if (user.GS_IsSystemAccount && !user.GS_IsDeveloper)
			{
				return EH_ShippersSignature;
			}

			var dgnNumber = user.DangerousGoodsCertificateNumber.Trim();
			var maxLength = ShippersSignatureMaxLength;

			if (IsBorrowedMaster && GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.HongKong && IsHongKongExport)
			{
				return GetIssuingAgentName(maxLength);
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Australia)
			{
				var maxLengthForUserName = dgnNumber.IsEmpty ? maxLength - dgnNumber.Length : maxLength - dgnNumber.Length - 1;
				var userName = GetProperUserName(user, maxLengthForUserName);
				return ZString.Format("{0} {1}", dgnNumber, userName).Trim().SubstringSafe(0, maxLength).Trim();
			}

			return GetUsernamePlusDgn(user, maxLength);
		}

		protected virtual ZString GetIssuingAgentName(int maxLength)
		{
			return ((ZString)Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName).Left(maxLength);
		}

		protected ZString GetUsernamePlusDgn(GlbStaff user, int maxLength)
		{
			var dgnNumber = user.DangerousGoodsCertificateNumber.Trim();
			return ZString.Format("{0} {1}", user.GS_FullName, dgnNumber).SubstringSafe(0, maxLength).Trim();
		}

		public virtual int ShippersSignatureMaxLength
		{
			get { return ExportAWBHeaderSchema.EH_ShippersSignature.MaxLength; }
		}

		protected abstract ZString ExtraCarrierInfoLine2 { get; }

		protected abstract ZString ExtraShipperInfoLine1 { get; }

		protected abstract ZString ExtraShipperInfoLine2 { get; }

		#region Address Defaulting

		#region Set Default Address

		void ResetAddressOverrides()
		{
			EH_IsShipperOverriden = false;
			EH_IsConsigneeOverriden = false;
			EH_IsNotifyOverriden = false;
		}

		#endregion

		#region Shipper Address Defaulting

		#region EH_ShipperDefaultAddressPicker

		[BusinessObjectTestExclude]
		[List("ShipperAddressPickList")]
		[MaxLength(30)]
		public ZString EH_ShipperDefaultAddressPicker
		{
			get { return Res.GetString("0b8fa667-14f7-4c96-93d8-ef207a786982", "PICK SHIPPER ADDRESS"); }
			set
			{
				if (!value.IsEmpty)
				{
					string description = ShipperAddressPickList.GetDescriptionFromCode(value);

					if (!string.IsNullOrEmpty(description))
					{
						int index = ZInt.Parse(description);
						if (index == -1)
						{
							PopulateShipperContactDetails(ShipperDocumentaryAddress);
							PopulateShipperAddressDetails(ShipperDocumentaryAddress);
						}
						else
						{
							PopulateShipperContactDetails(ShipperAddresses[index]);
							PopulateShipperAddressDetails(ShipperAddresses[index]);
						}
					}
					EH_ShipperDefaultAddressPickerInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo EH_ShipperDefaultAddressPickerInfo
		{
			get { return GetZPropertyInfo(Schema.EH_ShipperDefaultAddressPicker); }
		}

		#endregion

		#region Shipper Address List

		public CodeDescriptionPairList ShipperAddressPickList
		{
			get
			{
				if (fShipperAddressPickList == null)
				{
					fShipperAddressPickList = GetAddressPickList(ShipperDocumentaryAddress, ShipperAddresses);
				}
				return fShipperAddressPickList;
			}
		}
		CodeDescriptionPairList fShipperAddressPickList;

		List<OrgAddress> ShipperAddresses
		{
			get
			{
				if (fShipperAddresses == null)
				{
					fShipperAddresses = GetShipperAddresses();
				}
				return fShipperAddresses;
			}
		}
		List<OrgAddress> fShipperAddresses;
		protected abstract List<OrgAddress> GetShipperAddresses();

		#endregion

		#region Populate Shipper Address

		protected virtual DefaultAddressTypes DefaultShipperAddressType
		{
			get { return TranslateDefaultAddressType(Env.Registry.Freight.AirWaybill.ShipperAddressDefaultsTo); }
		}

		void PopulateShipperAddressDefaults()
		{
			EH_ShipperAccount = ShipperAccount;

			var addressToUse = (DefaultShipperAddressType == DefaultAddressTypes.Office && ShipperOfficeAddress != null)
						? ShipperOfficeAddress
						: (DefaultShipperAddressType == DefaultAddressTypes.Pickup && ShipperPickupAddress != null)
							? ShipperPickupAddress as IDocAddress
							: ShipperDocumentaryAddress;

			if (addressToUse != null)
			{
				PopulateShipperContactDetails(addressToUse);
				PopulateShipperAddressDetails(addressToUse);
			}
		}

		protected virtual void PopulateShipperContactDetails(IDocAddress address)
		{
			(EH_ShipperContactName, EH_ShipperContactCode, EH_ShipperContactDetail) =
				SetContactCodeAndDetails(address, EH_ShipperContactNameInfo.MaxLength);
		}

		void PopulateShipperAddressDetails(IDocAddress address)
		{
			var awbAddress = GetAWBAddressWithFallback(address);
			var addressDetails = AddressDetails.Get(awbAddress, true);

			var shipperName = addressDetails.CompanyName.IsEmpty ? DefaultShipperCompanyName : addressDetails.CompanyNameTruncated;
			EH_ShipperName = shipperName.Left(EH_ShipperNameInfo.MaxLength);

			EH_ShipperAddress = addressDetails.Address1.Left(EH_ShipperAddressInfo.MaxLength);
			EH_ShipperAddress2 = addressDetails.Address2.Left(EH_ShipperAddress2Info.MaxLength);
			EH_ShipperPlace = addressDetails.City.Left(EH_ShipperPlaceInfo.MaxLength);
			EH_ShipperPostCode = addressDetails.Postcode.Left(EH_ShipperPostCodeInfo.MaxLength);
			if (awbAddress is OrgAddress orgAddress)
			{
				EH_ShipperCountryCode = orgAddress.OA_RL_NKRelatedPortCode.IsEmpty ? orgAddress.OA_RN_NKCountryCode : orgAddress.OA_RL_NKRelatedPortCode.Left(2);
			}
			else
			{
				EH_ShipperCountryCode = awbAddress.CountryCode;
			}

			EH_ShipperState = addressDetails.State.TransformState(Factory, EH_ShipperCountryCode).Left(EH_ShipperStateInfo.MaxLength);

			var regNoProvider = new OrgHeaderRegistrationNumberProvider(Shipper);
			var taxInfo = GetTaxCodeInformation(OriginLOCO, regNoProvider, false);
			IsShipperTraderNoExceedingMaxLength = taxInfo.TaxNumber.Length > EH_ShipperTraderNoInfo.MaxLength;

			var taxTraderNo = PopulateTaxTraderNo(taxInfo);

			ShipperCategory = Shipper != null ? Shipper.OH_Category : ZString.Empty;
			EH_ShipperTraderNo = IsShipperTraderNoExceedingMaxLength ? ZString.Empty : taxTraderNo;
			EH_ShipperTraderNoType = string.IsNullOrEmpty(taxInfo.TaxNumber) ? ZString.Empty : taxInfo.TaxPrefix;
			EH_ShipperTraderNoCountryCode = taxInfo.TaxCountryCode;
			EH_OA_ShipperAddress = awbAddress.E2_OA_Address;

			PopulateShipperOverrideAddress(addressDetails);
		}

		internal ZString DestinationCountryCode => DestinationCountry?.Code ?? ZString.Empty;

		bool IsHongKongExport => OriginCountryCode == Core.Constants.CountryCodes.HongKong && DestinationCountryCode != Core.Constants.CountryCodes.HongKong;

		void PopulateShipperOverrideAddress(AddressDetails addressDetails)
		{
			if (addressDetails != null && !EH_IsShipperOverriden)
			{
				EH_ShipperOverride1 = EH_ShipperName.Left(EH_ShipperOverride1Info.MaxLength);
				EH_ShipperOverride2 = addressDetails.Address1.Left(EH_ShipperOverride2Info.MaxLength);
				EH_ShipperOverride3 = addressDetails.Address2.Left(EH_ShipperOverride3Info.MaxLength);
				ZString shipperOverride4 = (EH_ShipperPlace + " " + EH_ShipperState + " " + EH_ShipperPostCode + " " + EH_ShipperCountryCode).Trim();
				EH_ShipperOverride4 = shipperOverride4.Left(EH_ShipperOverride4Info.MaxLength);

				var stringsForOverride5 = new[] { EH_ShipperContactCode, EH_ShipperContactDetail, EH_ShipperContactName, ExtraShipperData };
				var shipperOverride5 = ZString.Join(" ", stringsForOverride5.Where(x => !x.IsEmpty).ToArray()).Trim();
				EH_ShipperOverride5 = shipperOverride5.Left(EH_ShipperOverride5Info.MaxLength);
			}
		}

		protected virtual void PopulateShipperContactDetails()
		{
		}

		protected virtual ZString ShipperAccount
		{
			get
			{
				var shipper = ShipperDocumentaryAddress?.Organisation;
				return shipper != null ? shipper.OH_Code : ZString.Empty;
			}
		}

		public ZGuid EH_OA_ShipperAddress
		{
			get;
			private set;
		}

		public OrgHeader GetShipperOrgHeaderForSavingAWBAddress(BusinessObjectFactory newFactory)
		{
			var shipperAddress = Factory.Load<OrgAddress>(EH_OA_ShipperAddress);

			if (shipperAddress != null)
			{
				var orgHeader = newFactory.Load<OrgHeader>(shipperAddress.OA_OH);

				if (IsDeleted)
				{
					ErrorReporter.ReportOnce("AccessingRemovedAWBHeader", DeleteStackTrace);
					return orgHeader;
				}

				if (orgHeader != null)
				{
					var orgAddress = orgHeader.Addresses.AddNew();
					orgAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.AWB);
					orgAddress.OA_CompanyNameOverride = EH_ShipperName;
					orgAddress.OA_Address1 = EH_ShipperAddress;
					orgAddress.OA_Address2 = EH_ShipperAddress2;
					orgAddress.OA_City = EH_ShipperPlace;
					orgAddress.OA_State = EH_ShipperState;
					orgAddress.OA_PostCode = EH_ShipperPostCode;
					orgAddress.OA_RN_NKCountryCode = EH_ShipperCountryCode;
					orgAddress.OA_RL_NKRelatedPortCode = shipperAddress.OA_RL_NKRelatedPortCode;
					orgAddress.OA_Phone = EH_ShipperContactDetail.Left(orgAddress.OA_PhoneInfo.MaxLength);
				}

				return orgHeader;
			}

			return null;
		}

		#endregion

		#region Abstracts

		protected abstract OrgHeader Shipper { get; }
		protected abstract OrgAddress ShipperOfficeAddress { get; }
		protected abstract OrgAddress ShipperPickupAddress { get; }
		protected abstract JobDocAddress ShipperDocumentaryAddress { get; }
		protected abstract ZString DefaultShipperCompanyName { get; }

		protected abstract bool IsImportToCountry(ZString countryCode);
		protected abstract bool IsExportFromCountry(ZString countryCode);
		protected abstract bool IsTransitingThrough(ZString countryCode);

		#endregion

		#endregion

		#region Consignee Address Defaulting

		#region EH_ConsigneeDefaultAddressPicker

		[BusinessObjectTestExclude]
		[List("ConsigneeAddressPickList")]
		[MaxLength(30)]
		public ZString EH_ConsigneeDefaultAddressPicker
		{
			get { return Res.GetString("e1d3deeb-43a5-4497-acbc-300f86c01673", "PICK CONSIGNEE ADDRESS"); }
			set
			{
				if (!value.IsEmpty)
				{
					string description = ConsigneeAddressPickList.GetDescriptionFromCode(value);

					if (!string.IsNullOrEmpty(description))
					{
						int index = ZInt.Parse(description);
						if (index == -1)
						{
							PopulateConsigneeContactDetails(ConsigneeDocumentaryAddress);
							PopulateConsigneeAddressDetails(ConsigneeDocumentaryAddress);
						}
						else
						{
							PopulateConsigneeContactDetails(ConsigneeAddresses[index]);
							PopulateConsigneeAddressDetails(ConsigneeAddresses[index]);
						}
					}
					EH_ConsigneeDefaultAddressPickerInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo EH_ConsigneeDefaultAddressPickerInfo
		{
			get { return GetZPropertyInfo(Schema.EH_ConsigneeDefaultAddressPicker); }
		}

		#endregion

		#region Consignee Address List

		public CodeDescriptionPairList ConsigneeAddressPickList
		{
			get
			{
				if (fConsigneeAddressPickList == null)
				{
					fConsigneeAddressPickList = GetAddressPickList(ConsigneeDocumentaryAddress, ConsigneeAddresses);
				}
				return fConsigneeAddressPickList;
			}
		}
		CodeDescriptionPairList fConsigneeAddressPickList;

		List<OrgAddress> ConsigneeAddresses
		{
			get
			{
				if (fConsigneeAddresses == null)
				{
					fConsigneeAddresses = GetConsigneeAddresses();
				}
				return fConsigneeAddresses;
			}
		}
		List<OrgAddress> fConsigneeAddresses;
		protected abstract List<OrgAddress> GetConsigneeAddresses();

		#endregion

		#region Populate Consignee Addresses

		protected virtual ZString ConsigneeAccount
		{
			get
			{
				var consignee = ConsigneeDocumentaryAddress?.Organisation;
				return consignee != null ? consignee.OH_Code : ZString.Empty;
			}
		}

		protected virtual DefaultAddressTypes DefaultConsigneeAddressType
		{
			get { return TranslateDefaultAddressType(Env.Registry.Freight.AirWaybill.ConsigneeAddressDefaultsTo); }
		}

		void PopulateConsigneeAddressDefaults()
		{
			EH_ConsigneeAccount = ConsigneeAccount;

			var addressToUse = (DefaultConsigneeAddressType == DefaultAddressTypes.Office && ConsigneeOfficeAddress != null)
				? ConsigneeOfficeAddress
				: (DefaultConsigneeAddressType == DefaultAddressTypes.Delivery && ConsigneeDeliveryAddress != null)
					? ConsigneeDeliveryAddress as IDocAddress
					: ConsigneeDocumentaryAddress;

			if (addressToUse != null)
			{
				PopulateConsigneeContactDetails(addressToUse);
				PopulateConsigneeAddressDetails(addressToUse);
			}
		}

		protected virtual void PopulateConsigneeContactDetails(IDocAddress address)
		{
			(EH_ConsigneeContactName, EH_ConsigneeContactCode, EH_ConsigneeContactDetail) =
				SetContactCodeAndDetails(address, EH_ConsigneeContactNameInfo.MaxLength);
		}

		void PopulateConsigneeAddressDetails(IDocAddress address)
		{
			var awbAddress = GetAWBAddressWithFallback(address);
			var addressDetails = AddressDetails.Get(awbAddress, true);

			var consigneeName = addressDetails.CompanyName.IsEmpty ? DefaultConsigneeCompanyName : addressDetails.CompanyNameTruncated;
			EH_ConsigneeName = consigneeName.Left(EH_ConsigneeNameInfo.MaxLength);

			EH_ConsigneeAddress = addressDetails.Address1.Left(EH_ConsigneeAddressInfo.MaxLength);
			EH_ConsigneeAddress2 = addressDetails.Address2.Left(EH_ConsigneeAddress2Info.MaxLength);
			EH_ConsigneePlace = addressDetails.City.Left(EH_ConsigneePlaceInfo.MaxLength);
			EH_ConsigneePostCode = addressDetails.Postcode.Left(EH_ConsigneePostCodeInfo.MaxLength);

			if (awbAddress is OrgAddress orgAddress)
			{
				EH_ConsigneeCountryCode = orgAddress.OA_RL_NKRelatedPortCode.IsEmpty ? orgAddress.OA_RN_NKCountryCode : orgAddress.OA_RL_NKRelatedPortCode.Left(2);
			}
			else
			{
				EH_ConsigneeCountryCode = awbAddress.CountryCode;
			}

			EH_ConsigneeState = addressDetails.State.TransformState(Factory, EH_ConsigneeCountryCode).Left(EH_ConsigneeStateInfo.MaxLength);

			var regNoProvider = new OrgHeaderRegistrationNumberProvider(Consignee);
			var taxInfo = GetTaxCodeInformation(DestinationLOCO, regNoProvider, false);
			IsConsigneeTraderNoExceedingMaxLength = taxInfo.TaxNumber.Length > EH_ConsigneeTraderNoInfo.MaxLength;

			var taxTraderNo = PopulateTaxTraderNo(taxInfo);

			ConsigneeCategory = Consignee != null ? Consignee.OH_Category : ZString.Empty;
			EH_ConsigneeTraderNo = IsConsigneeTraderNoExceedingMaxLength ? ZString.Empty : taxTraderNo;
			EH_ConsigneeTraderNoType = string.IsNullOrEmpty(taxInfo.TaxNumber) ? ZString.Empty : taxInfo.TaxPrefix;
			EH_ConsigneeTraderNoCountryCode = taxInfo.TaxCountryCode;

			EH_OA_ConsigneeAddress = awbAddress.E2_OA_Address;

			PopulateConsigneeOverrideAddress(addressDetails);
		}

		IDocAddress GetAWBAddressWithFallback(IDocAddress address)
		{
			if (Parent == null || !IsAWBOverridden)
			{
				var realAddress = address is OrgAddress orgAddress ? orgAddress
					: address is JobDocAddress jobDocAddress && !jobDocAddress.E2_AddressOverride && jobDocAddress.HasRealAddress ? jobDocAddress.Address
					: null;

				if (realAddress != null
					&& !realAddress.AddressCapability.GetCapabilityEnabled(OrgAddressType.AWB)
					&& realAddress.Header != null)
				{
					var awbAddress = realAddress.Header.Addresses.GetAddressWithMainAddressFallback(realAddress.OA_RL_NKRelatedPortCode, OrgAddressType.AWB);
					if (awbAddress != null)
					{
						return awbAddress;
					}
				}
			}

			return address;
		}

		(ZString name, ZString code, ZString detail) SetContactCodeAndDetails(IDocAddress docAddress, int nameMaxLength)
		{
			var jobDocAddress = docAddress as JobDocAddress;

			var name = jobDocAddress != null ? jobDocAddress.E2_Contact.Left(nameMaxLength) : ZString.Empty;

			if (jobDocAddress?.Contact == null)
			{
				docAddress = GetAWBAddressWithFallback(docAddress);
			}

			var (code, detail) = SetContactCodeAndDetails(docAddress.E2_Phone, docAddress.E2_Fax);
			return (name, code, detail);
		}

		void PopulateConsigneeOverrideAddress(AddressDetails addressDetails)
		{
			if (addressDetails != null && !EH_IsConsigneeOverriden)
			{
				EH_ConsigneeOverride1 = EH_ConsigneeName.Left(EH_ConsigneeOverride1Info.MaxLength);
				ZString consigneeOverride4 = (EH_ConsigneePlace + " " + EH_ConsigneeState + " " + EH_ConsigneePostCode + " " + EH_ConsigneeCountryCode).Trim();
				EH_ConsigneeOverride4 = consigneeOverride4.Left(EH_ConsigneeOverride4Info.MaxLength);
				EH_ConsigneeOverride2 = addressDetails.Address1.Left(EH_ConsigneeOverride2Info.MaxLength);
				EH_ConsigneeOverride3 = addressDetails.Address2.Left(EH_ConsigneeOverride3Info.MaxLength);

				var stringsForOverride5 = new[] { EH_ConsigneeContactCode, EH_ConsigneeContactDetail, EH_ConsigneeContactName, RegistrationNumber };
				var consigneeOverride5 = ZString.Join(" ", stringsForOverride5.Where(x => !x.IsEmpty).ToArray()).Trim();
				EH_ConsigneeOverride5 = consigneeOverride5.Left(EH_ConsigneeOverride5Info.MaxLength);
			}
		}

		public ZGuid EH_OA_ConsigneeAddress
		{
			get;
			private set;
		}

		public OrgHeader GetConsigneeOrgHeaderForSavingAWBAddress(BusinessObjectFactory newFactory)
		{
			var consigneeAddress = Factory.Load<OrgAddress>(EH_OA_ConsigneeAddress);

			if (consigneeAddress != null)
			{
				var orgHeader = newFactory.Load<OrgHeader>(consigneeAddress.OA_OH);

				if (IsDeleted)
				{
					ErrorReporter.ReportOnce("AccessingRemovedAWBHeader", DeleteStackTrace);
					return orgHeader;
				}

				if (orgHeader != null)
				{
					var orgAddress = orgHeader.Addresses.AddNew();
					orgAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.AWB);
					orgAddress.OA_CompanyNameOverride = EH_ConsigneeName;
					orgAddress.OA_Address1 = EH_ConsigneeAddress;
					orgAddress.OA_Address2 = EH_ConsigneeAddress2;
					orgAddress.OA_City = EH_ConsigneePlace;
					orgAddress.OA_State = EH_ConsigneeState;
					orgAddress.OA_PostCode = EH_ConsigneePostCode;
					orgAddress.OA_RN_NKCountryCode = EH_ConsigneeCountryCode;
					orgAddress.OA_RL_NKRelatedPortCode = consigneeAddress.OA_RL_NKRelatedPortCode;
					orgAddress.OA_Phone = EH_ConsigneeContactDetail.Left(orgAddress.OA_PhoneInfo.MaxLength);
				}

				return orgHeader;
			}

			return null;
		}

		#endregion

		#region Abstracts

		protected abstract OrgHeader Consignee { get; }
		protected abstract OrgAddress ConsigneeOfficeAddress { get; }
		protected abstract OrgAddress ConsigneeDeliveryAddress { get; }
		protected abstract JobDocAddress ConsigneeDocumentaryAddress { get; }
		protected abstract ZString DefaultConsigneeCompanyName { get; }

		#endregion

		#endregion

		#region Notify Address Defaulting

		#region EH_AlsoNotifyDefaultAddressPicker

		[BusinessObjectTestExclude]
		[List("AlsoNotifyAddressPickList")]
		[MaxLength(30)]
		public ZString EH_AlsoNotifyDefaultAddressPicker
		{
			get { return Res.GetString("7cd5d1dc-eb31-47c6-8046-942299791d87", "PICK ALSO NOTIFY ADDRESS"); }
			set
			{
				if (!value.IsEmpty)
				{
					string description = AlsoNotifyAddressPickList.GetDescriptionFromCode(value);
					if (!string.IsNullOrEmpty(description))
					{
						int index = ZInt.Parse(description);
						if (index == -1)
						{
							PopulateAlsoNotifyAddressDefaults(NotifyPartyDocumentaryAddress);
						}
						else
						{
							PopulateAlsoNotifyAddressDefaults(AlsoNotifyAddresses[index]);
						}
					}
					EH_AlsoNotifyDefaultAddressPickerInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo EH_AlsoNotifyDefaultAddressPickerInfo
		{
			get { return GetZPropertyInfo(Schema.EH_AlsoNotifyDefaultAddressPicker); }
		}

		#endregion

		#region Also Notify Address List

		public CodeDescriptionPairList AlsoNotifyAddressPickList
		{
			get
			{
				if (fAlsoNotifyAddressPickList == null)
				{
					fAlsoNotifyAddressPickList = GetAddressPickList(NotifyPartyDocumentaryAddress, AlsoNotifyAddresses);
				}

				return fAlsoNotifyAddressPickList;
			}
		}
		CodeDescriptionPairList fAlsoNotifyAddressPickList;

		List<OrgAddress> AlsoNotifyAddresses
		{
			get
			{
				if (fAlsoNotifyAddresses == null)
				{
					fAlsoNotifyAddresses = GetAlsoNotifyAddresses();
				}
				return fAlsoNotifyAddresses;
			}
		}
		List<OrgAddress> fAlsoNotifyAddresses;

		#endregion

		#region Populate Also Notify Address

		void PopulateAlsoNotifyAddressDefaults(JobDocAddress jobDocAddress)
		{
			if (jobDocAddress != null)
			{
				(EH_AlsoNotifyContactName, EH_AlsoNotifyContactCode, EH_AlsoNotifyContactDetail)
					= SetContactCodeAndDetails(jobDocAddress, EH_AlsoNotifyContactNameInfo.MaxLength);

				if ((Parent == null || !IsAWBOverridden)
					&& !jobDocAddress.E2_AddressOverride
					&& jobDocAddress.HasRealAddress
					&& !jobDocAddress.Address.AddressCapability.GetCapabilityEnabled(OrgAddressType.AWB)
					&& jobDocAddress.Address.Header != null)
				{
					var awbAddress = jobDocAddress.Address.Header.Addresses.GetAddressWithMainAddressFallback(jobDocAddress.Address.OA_RL_NKRelatedPortCode, OrgAddressType.AWB);

					if (awbAddress != null)
					{
						PopulateAlsoNotifyAddressDefaults(awbAddress, true);

						return;
					}
				}

				var addressDetails = AddressDetails.Get(jobDocAddress, true);
				EH_AlsoNotifyName = addressDetails.CompanyNameTruncated.Left(EH_AlsoNotifyNameInfo.MaxLength);

				ZString alsoNotifyAddress = (addressDetails.Address1 + " " + addressDetails.Address2).Trim();
				EH_AlsoNotifyAddress = alsoNotifyAddress.Left(EH_AlsoNotifyAddressInfo.MaxLength);
				EH_AlsoNotifyPlace = addressDetails.City.Left(EH_AlsoNotifyPlaceInfo.MaxLength);
				EH_AlsoNotifyPostCode = addressDetails.Postcode.Left(EH_AlsoNotifyPostCodeInfo.MaxLength);
				EH_AlsoNotifyCountryCode = jobDocAddress.E2_RN_NKCountryCode.Left(2);
				EH_AlsoNotifyState = addressDetails.State.TransformState(Factory, EH_AlsoNotifyCountryCode).Left(EH_AlsoNotifyStateInfo.MaxLength);

				var regNoProvider = new OrgHeaderRegistrationNumberProvider(jobDocAddress.Organisation);
				var taxInfo = GetTaxCodeInformation(DestinationLOCO, regNoProvider, true);
				IsAlsoNotifyTraderNoExceedingMaxLength = taxInfo.TaxNumber.Length > EH_AlsoNotifyTraderNoInfo.MaxLength;

				var taxTraderNo = PopulateTaxTraderNo(taxInfo);

				NotifyPartyCategory = jobDocAddress.Organisation != null ? jobDocAddress.Organisation.OH_Category : ZString.Empty;
				EH_AlsoNotifyTraderNo = IsAlsoNotifyTraderNoExceedingMaxLength ? ZString.Empty : taxTraderNo;
				EH_AlsoNotifyTraderNoType = string.IsNullOrEmpty(taxInfo.TaxNumber) ? ZString.Empty : taxInfo.TaxPrefix;
				EH_AlsoNotifyTraderNoCountryCode = taxInfo.TaxCountryCode;

				EH_OA_AlsoNotifyAddress = jobDocAddress.E2_OA_Address;

				PopulateNotifyOverrideAddressDefaults(addressDetails);
			}
		}

		void PopulateAlsoNotifyAddressDefaults(OrgAddress orgAddress, bool isAWBAddress = false)
		{
			if (orgAddress != null)
			{
				if ((Parent == null || !IsAWBOverridden)
					&& !isAWBAddress
					&& !orgAddress.AddressCapability.GetCapabilityEnabled(OrgAddressType.AWB)
					&& orgAddress.Header != null)
				{
					var awbAddress = orgAddress.Header.Addresses.GetAddressWithMainAddressFallback(orgAddress.OA_RL_NKRelatedPortCode, OrgAddressType.AWB);

					if (awbAddress != null)
					{
						orgAddress = awbAddress;
					}
				}

				var addressDetails = AddressDetails.Get(orgAddress, true);
				EH_AlsoNotifyName = addressDetails.CompanyName.Left(EH_AlsoNotifyNameInfo.MaxLength);

				ZString alsoNotifyAddress = (addressDetails.Address1 + " " + addressDetails.Address2).Trim();
				EH_AlsoNotifyAddress = alsoNotifyAddress.Left(EH_AlsoNotifyAddressInfo.MaxLength);
				EH_AlsoNotifyPlace = addressDetails.City.Left(EH_AlsoNotifyPlaceInfo.MaxLength);
				EH_AlsoNotifyPostCode = addressDetails.Postcode.Left(EH_AlsoNotifyPostCodeInfo.MaxLength);
				EH_AlsoNotifyCountryCode = orgAddress.OA_RL_NKRelatedPortCode.Left(2);
				EH_AlsoNotifyState = addressDetails.State.TransformState(Factory, EH_AlsoNotifyCountryCode).Left(EH_AlsoNotifyStateInfo.MaxLength);
				EH_AlsoNotifyContactCode = "";
				EH_AlsoNotifyContactDetail = orgAddress.OA_Phone;

				var regNoProvider = new OrgHeaderRegistrationNumberProvider(orgAddress.Header);
				var taxInfo = GetTaxCodeInformation(DestinationLOCO, regNoProvider, true);
				IsAlsoNotifyTraderNoExceedingMaxLength = taxInfo.TaxNumber.Length > EH_AlsoNotifyTraderNoInfo.MaxLength;

				var taxTraderNo = PopulateTaxTraderNo(taxInfo);

				NotifyPartyCategory = orgAddress.Header != null ? orgAddress.Header.OH_Category : ZString.Empty;
				EH_AlsoNotifyTraderNo = IsAlsoNotifyTraderNoExceedingMaxLength ? ZString.Empty : taxTraderNo;
				EH_AlsoNotifyTraderNoType = string.IsNullOrEmpty(taxInfo.TaxNumber) ? ZString.Empty : taxInfo.TaxPrefix;
				EH_AlsoNotifyTraderNoCountryCode = taxInfo.TaxCountryCode;

				EH_OA_AlsoNotifyAddress = orgAddress.PK;

				if (!EH_AlsoNotifyContactDetail.IsEmpty)
				{
					EH_AlsoNotifyContactCode = Core.Constants.AWB.ContactCodes.TELEPHONE;
				}

				if (!isAWBAddress)
				{
					EH_AlsoNotifyContactName = ZString.Empty;
				}

				PopulateNotifyOverrideAddressDefaults(addressDetails);
			}
		}

		void PopulateNotifyOverrideAddressDefaults(AddressDetails addressDetails)
		{
			if (addressDetails != null && !EH_IsNotifyOverriden)
			{
				EH_NotifyOverride1 = EH_AlsoNotifyName.Left(EH_NotifyOverride1Info.MaxLength);
				EH_NotifyOverride2 = addressDetails.Address1.Left(EH_NotifyOverride2Info.MaxLength);
				EH_NotifyOverride3 = addressDetails.Address2.Left(EH_NotifyOverride3Info.MaxLength);
				ZString alsoNotifyOverride4 = (EH_AlsoNotifyPlace + " " + EH_AlsoNotifyState + " " + EH_AlsoNotifyCountryCode).Trim();
				EH_NotifyOverride4 = alsoNotifyOverride4.Left(EH_NotifyOverride4Info.MaxLength);

				var stringsForOverride5 = new[] { EH_AlsoNotifyContactCode, EH_AlsoNotifyContactDetail, EH_AlsoNotifyContactName, AlsoNotifyTraderTypeWithNo };
				var alsoNotifyOverride5 = ZString.Join(" ", stringsForOverride5.Where(x => !x.IsEmpty).ToArray()).Trim();
				EH_NotifyOverride5 = alsoNotifyOverride5.Left(EH_NotifyOverride5Info.MaxLength);
			}
		}

		public ZGuid EH_OA_AlsoNotifyAddress
		{
			get;
			private set;
		}

		public OrgHeader GetAlsoNotifyOrgHeaderForSavingAWBAddress(BusinessObjectFactory newFactory)
		{
			var alsoNotifyAddress = Factory.Load<OrgAddress>(EH_OA_AlsoNotifyAddress);

			if (alsoNotifyAddress != null)
			{
				var orgHeader = newFactory.Load<OrgHeader>(alsoNotifyAddress.OA_OH);

				if (IsDeleted)
				{
					ErrorReporter.ReportOnce("AccessingRemovedAWBHeader", DeleteStackTrace);
					return orgHeader;
				}

				if (orgHeader != null)
				{
					var orgAddress = orgHeader.Addresses.AddNew();
					orgAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.AWB);
					orgAddress.OA_CompanyNameOverride = EH_AlsoNotifyName;
					orgAddress.OA_Address1 = EH_AlsoNotifyAddress.Left(orgAddress.OA_Address1Info.MaxLength);
					orgAddress.OA_Address2 = EH_AlsoNotifyAddress2;
					orgAddress.OA_City = EH_AlsoNotifyPlace;
					orgAddress.OA_State = EH_AlsoNotifyState;
					orgAddress.OA_PostCode = EH_AlsoNotifyPostCode;
					orgAddress.OA_RN_NKCountryCode = EH_AlsoNotifyCountryCode;
					orgAddress.OA_RL_NKRelatedPortCode = alsoNotifyAddress.OA_RL_NKRelatedPortCode;
					orgAddress.OA_Phone = EH_AlsoNotifyContactDetail.Left(orgAddress.OA_PhoneInfo.MaxLength);
				}

				return orgHeader;
			}

			return null;
		}

		#endregion

		#region Abstracts

		protected abstract List<OrgAddress> GetAlsoNotifyAddresses();
		internal protected abstract JobDocAddress NotifyPartyDocumentaryAddress { get; }

		#endregion

		#endregion

		#region Common Address Defaulting

		public enum DefaultAddressTypes { None, Documentary, Office, Delivery, Pickup }

		protected DefaultAddressTypes TranslateDefaultAddressType(string defaultAddressTypeString)
		{
			DefaultAddressTypes result = DefaultAddressTypes.None;
			if (defaultAddressTypeString == OrgConstants.AddressType.Office)
			{
				result = DefaultAddressTypes.Office;
			}
			else if (defaultAddressTypeString == OrgConstants.AddressType.Pickup)
			{
				result = DefaultAddressTypes.Pickup;
			}
			else if (defaultAddressTypeString == OrgConstants.AddressType.Delivery)
			{
				result = DefaultAddressTypes.Delivery;
			}
			else if (defaultAddressTypeString == OrgConstants.AddressType.Documentary)
			{
				result = DefaultAddressTypes.Documentary;
			}
			return result;
		}

		CodeDescriptionPairList GetAddressPickList(JobDocAddress jobDocAddress, List<OrgAddress> orgAddressses)
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			CodeDescriptionPair codeDescriptionPair;

			if (jobDocAddress != null && !jobDocAddress.E2_Address1.IsEmpty)
			{
				codeDescriptionPair = new CodeDescriptionPair(OrgConstants.AddressType.Documentary + ": " + jobDocAddress.E2_Address1, "-1");
				result.Add(codeDescriptionPair);
			}

			for (int i = 0; i < orgAddressses.Count; i++)
			{
				OrgAddress orgAddress = orgAddressses[i];
				if (!orgAddress.OA_Address1.IsEmpty)
				{
					codeDescriptionPair = new CodeDescriptionPair(orgAddress.AddressCapability.GetListOfCodes() + ": " + orgAddress.OA_Address1, i.ToString(CultureInfo.InvariantCulture));
					result.Add(codeDescriptionPair);
				}
			}

			return result;
		}

		#endregion

		protected static (ZString code, ZString detail) SetContactCodeAndDetails(ZString? phone, ZString? fax)
		{
			if (phone.HasValue && !phone.Value.IsEmpty)
			{
				return ((ZString)Core.Constants.AWB.ContactCodes.TELEPHONE, phone.Value);
			}

			if (fax.HasValue && !fax.Value.IsEmpty)
			{
				return ((ZString)Core.Constants.AWB.ContactCodes.FAX, fax.Value);
			}

			return (ZString.Empty, ZString.Empty);
		}

		#endregion

		protected abstract ZString OriginCode { get; }
		protected abstract ZString AirportOfDeparture { get; }

		protected virtual ZString Booking1stCarrier
		{
			get { return (DepartureFlight1 != null && !DepartureFlight1.JW_VoyageFlight.IsEmpty) ? DepartureFlight1.JW_VoyageFlight.Left(2) : ZString.Empty; }
		}

		protected virtual ZString Booking1stFlight
		{
			get { return (DepartureFlight1 != null && !DepartureFlight1.JW_VoyageFlight.IsEmpty) ? DepartureFlight1.JW_VoyageFlight.SubstringSafe(2, 5) : ZString.Empty; }
		}

		public virtual ZDateTime Booking1stFlightDate
		{
			get
			{
				return (OverrideWaybillDefaults)
					? GetFlightDateFromDayString(EH_Booking1stFlightDate)
					: (DepartureFlight1 != null && !DepartureFlight1.JW_ETD.IsEmpty) ? DepartureFlight1.JW_ETD : ZDateTime.Empty;
			}
		}

		protected virtual ZString Booking1stFlightDay
		{
			get { return (DepartureFlight1 != null && DepartureFlight1.JW_ETD.IsValid) ? DepartureFlight1.JW_ETD.ToString("dd", CultureInfo.InvariantCulture) : ""; }
		}

		public virtual ZDateTime Booking2ndFlightDate
		{
			get
			{
				return (OverrideWaybillDefaults)
					? GetFlightDateFromDayString(EH_Booking2ndFlightDate)
					: (DepartureFlight2 != null && !DepartureFlight2.JW_ETD.IsEmpty) ? DepartureFlight2.JW_ETD : ZDateTime.Empty;
			}
		}

		protected virtual ZString Booking2ndFlightDay
		{
			get { return (DepartureFlight2 != null && DepartureFlight2.JW_ETD.IsValid) ? DepartureFlight2.JW_ETD.ToString("dd", CultureInfo.InvariantCulture) : ""; }
		}

		protected virtual ZString Booking2ndCarrier
		{
			get { return (DepartureFlight2 != null && !DepartureFlight2.JW_VoyageFlight.IsEmpty) ? DepartureFlight2.JW_VoyageFlight.Left(2) : ZString.Empty; }
		}

		protected virtual ZString Booking2stFlight
		{
			get { return (DepartureFlight2 != null && !DepartureFlight2.JW_VoyageFlight.IsEmpty) ? DepartureFlight2.JW_VoyageFlight.SubstringSafe(2, 5) : ZString.Empty; }
		}

		protected virtual Transport DepartureFlight1
		{
			get { return Consol != null && !Consol.IsDeleted ? Consol.GetTransportByPlanningType(Core.Constants.TransportPlanningType.Flight1) : null; }
		}

		protected virtual Transport DepartureFlight2
		{
			get { return Consol != null && !Consol.IsDeleted ? Consol.GetTransportByPlanningType(Core.Constants.TransportPlanningType.Flight2) : null; }
		}

		protected virtual Transport DepartureFlight3
		{
			get { return Consol != null && !Consol.IsDeleted ? Consol.GetTransportByPlanningType(Core.Constants.TransportPlanningType.Flight3) : null; }
		}

		protected virtual ZString To1st
		{
			get { return DepartureFlight1 != null ? IATAPortCodeFromUNLOCOCode(DepartureFlight1.DiscPort) : ZString.Empty; }
		}

		internal RefAirline By1stCarrier
		{
			get { return Factory.LoadTop1<RefAirline>(new ZQuery(RefAirlineSchema.RM_TwoCharacterCode, By1st)); }
		}

		protected virtual ZString By1st
		{
			get { return DepartureFlight1 != null && !DepartureFlight1.JW_VoyageFlight.IsEmpty ? DepartureFlight1.JW_VoyageFlight.Left(2) : ZString.Empty; }
		}

		protected virtual ZString To2nd
		{
			get { return DepartureFlight2 != null ? IATAPortCodeFromUNLOCOCode(DepartureFlight2.DiscPort) : ZString.Empty; }
		}

		protected virtual ZString By2nd
		{
			get { return DepartureFlight2 != null && !DepartureFlight2.JW_VoyageFlight.IsEmpty ? DepartureFlight2.JW_VoyageFlight.Left(2) : ZString.Empty; }
		}

		protected virtual ZString To3rd
		{
			get { return DepartureFlight3 != null ? IATAPortCodeFromUNLOCOCode(DepartureFlight3.DiscPort) : ZString.Empty; }
		}

		protected virtual ZString By3rd
		{
			get { return DepartureFlight3 != null && !DepartureFlight3.JW_VoyageFlight.IsEmpty ? DepartureFlight3.JW_VoyageFlight.Left(2) : ZString.Empty; }
		}

		protected virtual ZString AWBCurrency
		{
			get { return GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency; }
		}

		protected virtual ZString AWBDestinationText
		{
			get { return DestinationLOCO != null ? DestinationLOCO.RL_PortName.Left(35) : ZString.Empty; }
		}

		protected virtual ZString AWBDestinationCode
		{
			get { return IATAPortCodeFromUNLOCOCode(DestinationLOCO); }
		}

		protected virtual ZDecimal RateLineTotal
		{
			get { return 0; }
		}

		protected string Get3rdFlightInformationString(TransportCollection transports)
		{
			ZString result = ZString.Empty;

			if (transports != null && transports.Count > 2)
			{
				Transport[] flight3 = (Transport[])transports.Find(new ZQuery(JobConsolTransportSchema.JW_TransportType, Core.Constants.TransportPlanningType.Flight3));

				if (flight3.Length > 0)
				{
					if (!flight3[0].JW_VoyageFlight.IsEmpty)
					{
						result = flight3[0].JW_VoyageFlight;
					}

					if (!flight3[0].JW_ETD.Date.IsEmpty)
					{
						result += "/" + flight3[0].JW_ETD.Date.Day.ToString(CultureInfo.InvariantCulture);
					}
				}
			}

			return result;
		}

		#endregion

		#region Abstract Properties for Defaulting Data

		public virtual bool IsMAWB => false;
		public virtual bool IsHAWB => false;
		public virtual bool IsDirectMAWB => false;
		public virtual bool IsIndirectHAWB => false;

		protected abstract ZBool IsConsigneeAdvanceCargoReportingSelfFilerSet { get; }

		protected abstract ZBool OverrideWaybillDefaults { get; }
		protected virtual ZBool OverrideSecurityDeclarationDefaults { get; }

		public abstract ZString GoodsDescription { get; }
		protected abstract ZString ChargesCode { get; }
		protected abstract ZDecimal CustomsValue { get; }
		protected abstract ZString CustomsValueCurrency { get; }

		protected abstract ZDecimal InsuranceValue { get; }
		protected abstract ZString InsuranceValueCurrency { get; }

		protected virtual ZDecimal DeclaredValue { get { return 0m; } }
		protected virtual ZString DeclaredValueCurrency { get { return ""; } }

		protected abstract ZString ReferenceNumber { get; }

		public abstract ForwardingConsol Consol { get; }

		#region AWB Locations

		protected abstract RefUNLOCO OriginLOCO { get; }
		protected abstract RefUNLOCO DestinationLOCO { get; }

		public ZString DestinationLOCOCode => DestinationLOCO == null ? ZString.Empty : DestinationLOCO.RL_Code;

		public ZString OriginLOCOCode => OriginLOCO == null ? ZString.Empty : OriginLOCO.RL_Code;

		public abstract GlbBranch DeparturePortRelatedBranch { get; }

		public override RefCountry OriginCountry
		{
			get { return OriginLOCO != null ? OriginLOCO.Country : null; }
		}

		internal ZString OriginCountryCode
		{
			get { return OriginCountry != null ? OriginCountry.Code : ZString.Empty; }
		}

		public override RefCountry DestinationCountry
		{
			get { return DestinationLOCO != null ? DestinationLOCO.Country : null; }
		}

		protected RefUNLOCO ConsolOriginLOCO
		{
			get { return Consol != null ? Consol.DepartureFlightOriginLoco : null; }
		}

		protected RefUNLOCO ConsolDestinationLOCO
		{
			get { return Consol != null ? Consol.InboundFlightDestinationLoco : null; }
		}

		#endregion

		protected abstract ZString RateLineNoPieces { get; }
		protected abstract ZString RateLineWeightUnit { get; }
		protected abstract ZDecimal RateLineGrossWeight { get; }
		protected abstract ZString RateLineGrossWeightUnit { get; }
		protected abstract ZDecimal RateLineChargeableWeight { get; }
		protected abstract ZDecimal RateLineRateChargeOrDiscount { get; }
		public abstract ZString WeightVPPDCOL { get; }
		protected abstract ZString OtherPPDCOL { get; }

		protected void PopulateIssuedBy()
		{
			SetIssuedByAddress(IssuedBy);
		}
		protected BillIssuedBy IssuedBy
		{
			get { return issuedBy ?? (issuedBy = GetNewIssuedBy()); }
		}
		BillIssuedBy issuedBy;
		protected abstract BillIssuedBy GetNewIssuedBy();

		protected abstract ZString AsAgreed1st { get; }
		protected abstract ZString AsAgreed2nd { get; }
		public abstract ZString AWBRatelineOvertypedNotes { get; }
		protected abstract ZString HandlingInformation { get; }
		protected abstract ZString OptionalShippingInformation1 { get; }
		protected abstract ZString OptionalShippingInformation2 { get; }
		protected abstract ZString BillNumber { get; }
		protected abstract ZInt ShippingLoadAndCount { get; }
		public abstract ZString RegistrationNumber { get; }
		public abstract ZString ExtraShipperData { get; }
		public abstract ZString SpecialHandlingCode { get; }

		public ZString ExtraAlsoNotifyData
		{
			get { return AlsoNotifyTraderTypeWithNo; }
		}

		#endregion

		[DecimalPlaces(2)]
		public override ZDecimal EH_DeclaredValue
		{
			get
			{
				return base.EH_DeclaredValue;
			}
			set
			{
				base.EH_DeclaredValue = value;
			}
		}

		[DecimalPlaces(2)]
		public override ZDecimal EH_CustomsValue
		{
			get
			{
				return base.EH_CustomsValue;
			}
			set
			{
				base.EH_CustomsValue = value;
			}
		}

		[DecimalPlaces(2)]
		public override ZDecimal EH_InsuranceValue
		{
			get
			{
				return base.EH_InsuranceValue;
			}
			set
			{
				base.EH_InsuranceValue = value;
			}
		}

		[DecimalPlaces(2)]
		public override ZDecimal EH_ValuationPPD
		{
			get
			{
				return base.EH_ValuationPPD;
			}
			set
			{
				base.EH_ValuationPPD = value;
			}
		}

		[DecimalPlaces(2)]
		public override ZDecimal EH_ValuationCOL
		{
			get
			{
				return base.EH_ValuationCOL;
			}
			set
			{
				base.EH_ValuationCOL = value;
			}
		}

		#endregion

		#region Macro Resolution

		[SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
		protected void AddExtraText(string macro, ref ZString input, IRegistryItemInternals registryItem, string delimiter = " ")
		{
			if (inReplaceMacros)
			{
				return; // to avoid StackOverflowException
			}

			inReplaceMacros = true;
			try
			{
				string extraText;

				using (Factory.GetDocWrapperContextManager().SetContextValuesTemporarily(DocumentDirection.DEP, ContactType.All))
				{
					var macroRelacer = new MacroStringReplacer(new DataProviderList(GetNewExtraTextMacroDataProvider()));

					try
					{
						extraText = macroRelacer.ReplaceMacros(macro);
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						throw new ExportAWBHeaderReplaceMacrosException(MessageForReplaceMacrosFailed + System.Environment.NewLine + Res.GetString("4EBE62CF-82E3-4544-9815-98AB35788225", "Please correct macro format in Registry -> {0}.", registryItem?.HumanReadableRegistryPath()), ex);
					}
				}

				if (extraText.Length > 0)
				{
					if (input.Length > 0)
					{
						input += delimiter + extraText;
					}
					else
					{
						input += extraText;
					}
				}
			}
			finally
			{
				inReplaceMacros = false;
			}
		}
		bool inReplaceMacros;

		protected virtual IBODocDataProvider GetNewExtraTextMacroDataProvider()
		{
			return DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.AWB, this);
		}

		protected abstract ZString MessageForReplaceMacrosFailed { get; }

		[BusinessObjectTestExclude()]
		public override ZBool EH_AreRateLinesOverridden
		{
			get => base.EH_AreRateLinesOverridden;
			set
			{
				if (base.EH_AreRateLinesOverridden != value)
				{
					base.EH_AreRateLinesOverridden = value;

					if (!value && Parent != null)
					{
						PopulateAllRateLinesSectionInfo();
					}

					AWBRateLines.MarkAsNeedingValidation();
					AWBRateLines.Cast<ExportAWBRateLine>().ForEach(x =>
					{
						x.NatureAndQtyOfGoods.Validation.ValidateText();
						x.RefreshBindingForOverride();
					});
				}
			}
		}

		void PopulateAllRateLinesSectionInfo()
		{
			PopulateRateLinesRelatedProperties();
			PopulateNatureAndQtyOfGoods();
			PopulateRateLines();

			AWBRateLines.Cast<ExportAWBRateLine>().ForEach(x => x.CalculateTotal());
			PopulateSLAC();
		}

		protected bool ShouldSavedHeaderForRateLinesOverridden => !IsDeleted
					&& (EH_AreRateLinesOverriddenInfo.HasChanges
						|| (EH_AreRateLinesOverridden && (!IsInDatabase || EH_AsAgreed1stInfo.HasChanges || EH_AsAgreed2ndInfo.HasChanges || EH_ShippingLoadAndCountInfo.HasChanges)));

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			if (property.Name == ExportAWBHeaderSchema.Constants.EH_AreRateLinesOverridden)
			{
				return false;
			}
			else if (property.Name == ExportAWBHeaderSchema.Constants.EH_AsAgreed1st
					|| property.Name == ExportAWBHeaderSchema.Constants.EH_AsAgreed2nd
					|| property.Name == ExportAWBHeaderSchema.Constants.EH_ShippingLoadAndCount)
			{
				return !EH_AreRateLinesOverridden;
			}
			else if (property.Name == ExportAWBHeaderSchema.Constants.EH_SecurityStatusIssueDate
					|| property.Name == ExportAWBHeaderSchema.Constants.EH_AgentApprovalExpiryDate
					|| property.Name == ExportAWBHeaderSchema.Constants.EH_RN_NKAgentApprovalCountryCode
					|| property.Name == ExportAWBHeaderSchema.Constants.EH_GS_NKSecurityStatusIssuedByCode
					|| property.Name == ExportAWBHeaderSchema.Constants.EH_AdditionalSecurityInformation
					|| property.Name == ExportAWBHeaderSchema.Constants.EH_AdditionalSecurityInformationStatement
					|| property.Name == ExportAWBHeaderSchema.Constants.EH_AdditionalScreeningMethods)
			{
				return IsSecurityDeclarationReadOnly;
			}
			else if (property.Name == ExportAWBHeaderSchema.Constants.EH_AgentApprovalNumber)
			{
				return EH_AgentApprovalNumber_ReadOnly;
			}
			else if (property.Name == ExportAWBHeaderSchema.Constants.EH_ScheduledArrivalDate)
			{
				return EH_ScheduledArrivalDate_ReadOnly;
			}

			return CargoWise.ComponentModel.MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion

		#region Addresses

		public void ResetAddressPickerDropLists()
		{
			fShipperAddressPickList = null;
			fShipperAddresses = null;
			fConsigneeAddressPickList = null;
			fConsigneeAddresses = null;
			fAlsoNotifyAddressPickList = null;
			fAlsoNotifyAddresses = null;
		}

		public JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (fDocAddresses == null)
				{
					fDocAddresses = new JobDocAddressDependentCollection(this);
					fDocAddresses.IsManagedForDataRefresh = true;
				}

				return fDocAddresses;
			}
		}

		JobDocAddressDependentCollection fDocAddresses;

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return null;
		}

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
		{
			get { return Array.Empty<DocAddressType>(); }
		}

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			return null;
		}

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return Env.Security.None;
		}

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress)
		{
			return false;
		}

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			return null;
		}

		#endregion

		#region Implementation

		protected virtual void PopulateIssueDate()
		{
			if (Consol != null)
			{
				if (!OverrideWaybillDefaults || EH_AWBIssueDate.IsEmpty)
				{
					EH_AWBIssueDate = Consol.JK_MasterBillIssueDate.IsEmpty ? Consol.GetAWBIssueDate() : Consol.JK_MasterBillIssueDate;
				}
			}
			else
			{
				EH_AWBIssueDate = ZDateTime.Now;
			}
		}

		public void PopulateExtraShipperInfoLine2()
		{
			EH_ExtraShipperInfoLine2 = ExtraShipperInfoLine2;
		}

		protected ZString IATAPortCodeFromUNLOCOCode(RefUNLOCO uNLOCO)
		{
			ZString result = "";
			if (uNLOCO != null)
			{
				if (uNLOCO.RL_IATA.Length > 0)
				{
					result = uNLOCO.RL_IATA;
				}
				else if (uNLOCO.Code.Length > 3)
				{
					result = uNLOCO.Code.SubstringSafe(2, 3);
				}
			}

			return result;
		}

		protected void SetIssuedByAddress(BillIssuedBy billIssuedBy)
		{
			var addressFormatter = new AWBAddressFormatter(Factory);

			addressFormatter.Format(billIssuedBy, transformState: true);

			EH_IssuingAgentName = addressFormatter.IssuingAgentName;
			EH_IssuingAgentAddress1 = addressFormatter.IssuingAgentAddress1;
			EH_IssuingAgentAddress2 = addressFormatter.IssuingAgentAddress2;
		}

		protected ZDateTime GetFlightDateFromDayString(ZString day)
		{
			ZDateTime flightDate;

			var referenceDate = (EH_AWBIssueDate.IsEmpty) ? ZDateTime.Now : EH_AWBIssueDate;

			if (int.TryParse(day, out var iDay)
				&& iDay > 0)
			{
				if (referenceDate.Day > iDay)
				{
					referenceDate = referenceDate.AddMonths(1);
				}

				var daysInMonth = DateTime.DaysInMonth(referenceDate.Year, referenceDate.Month);
				flightDate = new ZDateTime(referenceDate.Year, referenceDate.Month, (iDay > daysInMonth) ? daysInMonth : iDay);
			}
			else
			{
				flightDate = ZDateTime.Empty;
			}

			return flightDate;
		}

		#endregion

		#region Borrowed Masters

		JobMawb GetBorrowedMaster()
		{
			JobMawb result = null;

			if (Consol != null)
			{
				ZQuery filter = new ZQuery(JobMawbSchema.JM_ParentTableCode, JobConsolSchema.Constants.Prefix);
				filter.AddToFilter(JoinCondition.And, JobMawbSchema.JM_ParentID, SQLComparisonOperator.Equal, Consol.PK);
				result = Factory.LoadTop1<JobMawb>(filter);

				if (result == null && !Consol.JK_IsNeutralMaster)
				{
					filter = new ZQuery(JobMawbSchema.JM_Airline3DigitPrefix, Consol.MasterBillAirlinePrefix);
					filter.AddToFilter(JobMawbSchema.JM_MAWB, Consol.MasterBillMAWB);
					filter.AddToFilter(JobMawbSchema.JM_IsPaper, ZBool.True);
					result = Factory.LoadTop1<JobMawb>(filter);
				}
			}

			return result;
		}

		protected JobMawb BorrowedMaster
		{
			get
			{
				if (borrowedMaster == null || borrowedMaster.IsDeleted)
				{
					borrowedMaster = GetBorrowedMaster();
				}

				return borrowedMaster;
			}
		}
		JobMawb borrowedMaster;

		protected ZBool IsBorrowedMaster
		{
			get { return !BorrowedMaster?.JM_OA_From.IsEmpty ?? false; }
		}

		protected OrgHeader BorrowedFrom
		{
			get { return BorrowedMaster?.From.Header; }
		}

		#endregion

		#region Doc Wrapper Usage

		public ZInt LabelStartRange
		{
			get
			{
				EnsureDocumentSettingsPopulated();
				return labelStartRange;
			}
			set => labelStartRange = value;
		}
		ZInt labelStartRange;

		public ZInt LabelEndRange
		{
			get
			{
				EnsureDocumentSettingsPopulated();
				return labelEndRange;
			}
			set => labelEndRange = value;
		}
		ZInt labelEndRange;

		public ZInt LabelTotalPacks
		{
			get
			{
				EnsureDocumentSettingsPopulated();
				return labelTotalPacks;
			}
			set => labelTotalPacks = value;
		}
		ZInt labelTotalPacks;

		public ZInt MAWBLabelStartRange
		{
			get
			{
				EnsureDocumentSettingsPopulated();
				return mAWBLabelStartRange;
			}
			set => mAWBLabelStartRange = value;
		}
		ZInt mAWBLabelStartRange;

		public ZInt MAWBLabelTotalPacks
		{
			get
			{
				EnsureDocumentSettingsPopulated();
				return mAWBLabelTotalPacks;
			}
			set => mAWBLabelTotalPacks = value;
		}
		ZInt mAWBLabelTotalPacks;

		public ZString DocumentSize
		{
			get
			{
				EnsureDocumentSettingsPopulated();
				return documentSize;
			}
			set => documentSize = value;
		}
		ZString documentSize;

		public virtual ZBool IsPrintingFinalNeutralMAWB
		{
			get { return ZBool.False; }
			set { }
		}

		public virtual ZBool IsPrintingDraftNeutralMAWB
		{
			get { return ZBool.False; }
		}

		public virtual ZBool IsReprintingNeutralMAWB
		{
			get { return ZBool.False; }
		}

		public ZBool PrintOptionalInformation
		{
			get
			{
				EnsureDocumentSettingsPopulated();
				return printOptionalInformation;
			}
			set => printOptionalInformation = value;
		}
		ZBool printOptionalInformation = true;

		void EnsureDocumentSettingsPopulated()
		{
			if (!DocumentSettingsPopulated)
			{
				GetAWBActions()?.SetDocumentSettings();
			}
		}
		public bool DocumentSettingsPopulated { get; set; }
		protected abstract AWBActions GetAWBActions();

		#endregion

		#region VAT Numbers

		protected ZString ShipperTraderTypeWithNo
		{
			get
			{
				return RequiredTaxNumbers.CombineTaxTypeAndNumber(EH_ShipperTraderNoType, EH_ShipperTraderNo);
			}
		}

		protected ZString ConsigneeTraderTypeWithNo
		{
			get
			{
				return RequiredTaxNumbers.CombineTaxTypeAndNumber(EH_ConsigneeTraderNoType, EH_ConsigneeTraderNo);
			}
		}

		protected ZString AlsoNotifyTraderTypeWithNo
		{
			get
			{
				return RequiredTaxNumbers.CombineTaxTypeAndNumber(EH_AlsoNotifyTraderNoType, EH_AlsoNotifyTraderNo);
			}
		}

		#endregion

		#region Country Related Properties

		public virtual bool IsImportToBangladesh => false;

		public virtual bool IsImportToBrazil => false;

		public virtual bool IsImportToChina => false;

		public virtual bool IsImportToCanada => false;

		public virtual bool IsExportFromCanada => false;

		public virtual bool IsTransitingThroughChina => false;

		public bool IsImportToHonduras => IsImportToCountry(Core.Constants.CountryCodes.Honduras);

		public bool IsExportFromHonduras => IsExportFromCountry(Core.Constants.CountryCodes.Honduras);

		public bool IsImportToBolivia => IsImportToCountry(Core.Constants.CountryCodes.Bolivia);

		public bool IsExportFromBolivia => IsExportFromCountry(Core.Constants.CountryCodes.Bolivia);

		public virtual bool IsBolivianNITRequired => false;

		public virtual bool IsHondurasRTNRequired => false;

		public bool IsImportToIndia => IsImportToCountry(Core.Constants.CountryCodes.India);

		public bool IsExportFromIndia => IsExportFromCountry(Core.Constants.CountryCodes.India);

		public virtual bool IsIndianCARNRequired => false;

		public virtual string SwitzerlandDepartureFlightCode => string.Empty;

		#endregion

		#region VAT & Contact Number for FWB/FHL

		public override List<IVATCountryHandler> GetVATCountryHandlers(IFBaseMessageDetailsProvider provider)
		{
			return new List<IVATCountryHandler>() {
				new EgyptVATCountryHandler(provider),
				new KenyaVATCountryHandler(provider),
				new ArgentinaVATCountryHandler(provider),
				new BrazilVATCountryHandler(provider),
				new MoroccoVATCountryHandler(provider),
				new BangladeshVATCountryHandler(provider)
			}.Concat(base.GetVATCountryHandlers(provider)).ToList();
		}

		public override List<IContactNumberCountryHandler> GetContactNumberCountryHandlers(IFBaseMessageDetailsProvider provider)
		{
			return new List<IContactNumberCountryHandler>() {
				new ChinaContactNumberCountryHandler(provider, this)
			}.Concat(base.GetContactNumberCountryHandlers(provider)).ToList();
		}

		IACASCountryHandler usaACASCountryHandler;
		public override IACASCountryHandler GetACASCountryHandler()
		{
			usaACASCountryHandler ??= new UsaACASCountryHandler(this);
			return usaACASCountryHandler;
		}

		#endregion

		#region Tax Numbers

		(ZString TaxCode, ZString TaxPrefix, ZString TaxNumber, ZString TaxCountryCode) GetTaxCodeInformation(RefUNLOCO refUNLOCO, OrgHeaderRegistrationNumberProvider regNoProvider, bool isAlsoNitify)
		{
			var countryCode = refUNLOCO?.Country?.Code ?? ZString.Empty;
			var taxInfos = GetTaxInfoFromRefTable(countryCode, regNoProvider);

			if (taxInfos.Any())
			{
				var taxInfo = GetTaxCodeInformationBasedOnCountryPolicy(taxInfos, isAlsoNitify);
				var taxNumber = taxInfo?.Number ?? string.Empty;
				var taxPrefix = taxInfo?.ShortLabel ?? string.Empty;
				var taxCode = taxInfo?.Code ?? string.Empty;

				if (!taxNumber.IsEmpty
					&& IsPartOfEUOrNorthernIreland(refUNLOCO)
					&& CountryVatCodeType.Value.TryGetValue(countryCode, out var vatCode)
					&& taxInfo.Code.Equals(vatCode))
				{
					EnsureTaxNumberIncludesCountryCodePrefixForEUCountries(refUNLOCO, ref taxNumber);
				}

				return (TaxCode: taxCode,
					TaxPrefix: taxPrefix,
					TaxNumber: taxNumber,
					TaxCountryCode: taxInfo?.CountryCode ?? string.Empty);
			}

			return (TaxCode: string.Empty,
				TaxPrefix: string.Empty,
				TaxNumber: string.Empty,
				TaxCountryCode: string.Empty);
		}

		TaxCodeInformation GetTaxCodeInformationBasedOnCountryPolicy(List<TaxCodeInformation> taxInfos, bool isAlsoNotify)
		{
			TaxCodeInformation taxCodeInformation = null;
			if (IsImportToBangladesh)
			{
				taxCodeInformation = GetTaxCodeInformationForBangladesh(taxInfos);
			}
			else if (IsExportFromCanada && taxInfos.Any(t => t.Code == OrgCusCode.CodeTypes.CarrierCode))
			{
				taxCodeInformation = null;
			}
			else
			{
				taxCodeInformation = taxInfos.FirstOrDefault(t => !t.Number.IsEmpty) ?? taxInfos.First();
			}

			if ((IsExportFromBolivia || IsImportToBolivia) && taxCodeInformation != null && taxCodeInformation.Code == OrgCusCode.BoliviaCodeTypes.NIT && !IsBolivianNITRequired)
			{
				taxCodeInformation = null;
			}

			if ((IsExportFromIndia || IsImportToIndia) && taxCodeInformation != null && taxCodeInformation.Code == IndiaOrgCusCodeInfo.OrgCusCodes.CAN && (!IsIndianCARNRequired || isAlsoNotify))
			{
				taxCodeInformation = null;
			}

			if ((IsExportFromHonduras || IsImportToHonduras) && taxCodeInformation != null && taxCodeInformation.Code == OrgCusCode.HondurasCodeTypes.RTN && !IsHondurasRTNRequired)
			{
				taxCodeInformation = null;
			}

			return taxCodeInformation;
		}

		protected abstract TaxCodeInformation GetTaxCodeInformationForBangladesh(List<TaxCodeInformation> taxInfos);

		#region GetEUConsigneeTaxCodeInformation

		List<TaxCodeInformation> GetEUConsigneeTaxCodeInformation(ZString countryCode, OrgHeaderRegistrationNumberProvider regNoProvider)
		{
			var result = new List<TaxCodeInformation>();

			if (regNoProvider is IRegistrationNumberProvider provider)
			{
				if (countryCode == Core.Constants.CountryCodes.Norway)
				{
					result.AddRange(GetTaxCodeInformations(provider, Core.Constants.CountryCodes.Norway, OrgCusCode.NorwayCodeTypes.MVA));
				}
				else if (countryCode == Core.Constants.CountryCodes.Switzerland)
				{
					result.AddRange(GetTaxCodeInformations(provider, Core.Constants.CountryCodes.Switzerland, OrgCusCode.SwissCodeTypes.UID));
				}
				else if (countryCode == Core.Constants.CountryCodes.Liechtenstein)
				{
					result.AddRange(GetTaxCodeInformations(provider, Core.Constants.CountryCodes.Liechtenstein, OrgCusCode.SwissCodeTypes.UID));
					result.AddRange(GetTaxCodeInformations(provider, Core.Constants.CountryCodes.Switzerland, OrgCusCode.SwissCodeTypes.UID));
				}

				if (!result.Any())
				{
					result.AddRange(GetTaxCodeInformations(provider, null, OrgCusCode.EuropeanUnionSharedCodeTypes.Eori));
				}
			}

			return result;
		}

		List<TaxCodeInformation> GetTaxCodeInformations(IRegistrationNumberProvider provider, string countryCode, ZString typeCode)
		{
			var result = new List<TaxCodeInformation>();

			foreach (var (taxNumber, countryOfIssue) in provider.GetTaxNumbers(typeCode))
			{
				if (!taxNumber.IsEmpty && (countryCode == null || countryCode == countryOfIssue))
				{
					var refDocInfo = RequiredTaxNumbers.GetTaxInfoFromRefTable(countryOfIssue, provider, Factory).FirstOrDefault(c => c.Code == typeCode && c.DocumentType == SupportedTaxDocumentType);

					if (refDocInfo != null)
					{
						result.Add(refDocInfo);
					}
					else
					{
						result.Add(new TaxCodeInformation(typeCode,
							typeCode,
							typeCode,
							ZString.Empty,
							taxNumber,
							countryOfIssue,
							1,
							SupportedTaxDocumentType,
							ZString.Empty,
							ZString.Empty));
					}
				}
			}

			return result;
		}

		bool IsEUTaxCode(TaxCodeInformation taxCodeInfo) => taxCodeInfo != null && IsEUTaxCode(taxCodeInfo.CountryCode, taxCodeInfo.Code);

		bool IsEUTaxCode(ZString countryCode, ZString typeCode) =>
			typeCode == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori
			|| (countryCode == Core.Constants.CountryCodes.Norway && typeCode == OrgCusCode.NorwayCodeTypes.MVA)
			|| (countryCode == Core.Constants.CountryCodes.Switzerland && typeCode == OrgCusCode.SwissCodeTypes.UID)
			|| (countryCode == Core.Constants.CountryCodes.Liechtenstein && typeCode == OrgCusCode.SwissCodeTypes.UID);

		bool NeedToAddEUCountryCodePrefix(ZString typeCode, ZString taxNumber) =>
			(typeCode == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori
			|| typeCode == OrgCusCode.NorwayCodeTypes.MVA
			|| typeCode == OrgCusCode.SwissCodeTypes.UID) && (taxNumber.Length < 2 || !taxNumber.SubstringSafe(0, 2).IsLettersOnlyOrEmpty);

		ZString PopulateTaxTraderNo((ZString TaxCode, ZString TaxPrefix, ZString TaxNumber, ZString TaxCountryCode) taxInfo)
		{
			var taxTraderNo = taxInfo.TaxNumber;
			if (!taxTraderNo.IsEmpty && NeedToAddEUCountryCodePrefix(taxInfo.TaxCode, taxInfo.TaxNumber))
			{
				if (taxInfo.TaxCode == OrgCusCode.SwissCodeTypes.UID && taxInfo.TaxCountryCode == Core.Constants.CountryCodes.Liechtenstein)
				{
					taxTraderNo = new ZString(Core.Constants.CountryCodes.Switzerland + taxInfo.TaxNumber);
				}
				else
				{
					taxTraderNo = new ZString(taxInfo.TaxCountryCode + taxInfo.TaxNumber);
				}
			}
			return taxTraderNo;
		}
		#endregion

		bool IsPartOfEUOrNorthernIreland(RefUNLOCO refUNLOCO) => refUNLOCO != null && (refUNLOCO.IsInNorthernIreland || (refUNLOCO.Country.IsPartOfEuropeanUnion && refUNLOCO.Country.Code != Core.Constants.CountryCodes.IsleOfMan));

		public static Lazy<ImmutableDictionary<string, string>> CountryVatCodeType => new Lazy<ImmutableDictionary<string, string>>(() =>
		{
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, (NoResString)"SELECT CountryCode, TaxBusinessRegistrationCode FROM EUTaxBusinessRegistrationCodes()"); // SQL query
			return dataTable.AsEnumerable().ToImmutableDictionary(row => row[0].ToString(), row => row[1].ToString());
		}, true);

		void EnsureTaxNumberIncludesCountryCodePrefixForEUCountries(RefUNLOCO refUNLOCO, ref ZString taxNumber)
		{
			if (refUNLOCO?.Country?.Code is ZString countryCode)
			{
				var prefix = GetEUVatNumberPrefix(countryCode);

				bool isMissingPrefix;
				switch (countryCode)
				{
					case Core.Constants.CountryCodes.Austria:
					case Core.Constants.CountryCodes.Cyprus:
					case Core.Constants.CountryCodes.Spain:
						isMissingPrefix = !taxNumber.StartsWith(prefix) || taxNumber.Length == 9;
						break;
					case Core.Constants.CountryCodes.France:
					case Core.Constants.CountryCodes.Monaco:
						isMissingPrefix = !taxNumber.StartsWith(prefix) || taxNumber.Length == 11;
						break;
					case Core.Constants.CountryCodes.Ireland:
						isMissingPrefix = !taxNumber.StartsWith(prefix) || taxNumber.Length == 8 || taxNumber.Length == 9;
						break;
					case Core.Constants.CountryCodes.Netherlands:
						isMissingPrefix = !taxNumber.StartsWith(prefix) || taxNumber.Length == 12;
						break;
					default:
						isMissingPrefix = !taxNumber.StartsWith(prefix);
						break;
				}

				if (isMissingPrefix)
				{
					taxNumber = prefix + taxNumber;
				}
			}
		}

		ZString GetEUVatNumberPrefix(ZString countryCode)
		{
			switch (countryCode)
			{
				case Core.Constants.CountryCodes.Greece:
					return "EL";
				case Core.Constants.CountryCodes.UnitedKingdom:
					return "XI";
				case Core.Constants.CountryCodes.Monaco:
					return Core.Constants.CountryCodes.France;
				default:
					return countryCode;
			}
		}

		List<TaxCodeInformation> GetTaxInfoFromRefTable(ZString countryCode, OrgHeaderRegistrationNumberProvider regNoProvider = null)
		{
			var regulatingCountry = DestinationCountryCode == Core.Constants.CountryCodes.Egypt ? DestinationCountryCode : countryCode;

			var taxInfos = new List<TaxCodeInformation>();
			taxInfos.AddRange(RequiredTaxNumbers.GetTaxInfoFromRefTable(countryCode, regNoProvider, Factory, regulatingCountry).Where(c => !IsEUTaxCode(c)));

			if (IsImportToICS2Zone)
			{
				taxInfos.AddRange(GetEUConsigneeTaxCodeInformation(countryCode, regNoProvider));
			}

			return taxInfos.Where(t => SupportedTaxDocumentType == t.DocumentType).ToList();
		}

		internal ZString GetRequiredFormattedTraderTypes(ZString countryCode)
		{
			var taxInfos = GetTaxInfoFromRefTable(countryCode);
			return string.Join((NoResString)" or ", taxInfos.Select(x => GetFormattedTaxCodeInformation(x))); // formatted validation message
		}

		ZString GetFormattedTaxCodeInformation(TaxCodeInformation taxCodeInformation)
		{
			var label = taxCodeInformation.ShortLabel;
			return taxCodeInformation.Code == label || label.IsEmpty ? taxCodeInformation.Code : ZString.Format("{0} ({1})", taxCodeInformation.Code, label);
		}

		protected const string mawbDocumentType = "AWB";
		protected const string hawbDocumentType = "HAW";

		protected abstract string SupportedTaxDocumentType { get; }

		public override bool HasDestinationInIcs2Zone => DestinationLOCO?.IsInIcs2Zone ?? false;

#endregion

		#region Supply Chain Security Configuration

		internal SupplyChainSecurityConfiguration SupplyChainSecurityConfiguration
		{
			get { return supplyChainSecurityConfiguration ?? (supplyChainSecurityConfiguration = SupplyChainSecurityConfiguration.New()); }
		}
		SupplyChainSecurityConfiguration supplyChainSecurityConfiguration;

		internal SupplyChainSecurityConfiguration DestinationSupplyChainSecurityConfiguration
		{
			get
			{
				var countryCode = Consol?.JK_RL_NKDischargePort.SubstringSafe(0, 2) ?? ZString.Empty;
				var cacheKey = "ExportAWBHeader.DestinationSupplyChainSecurityConfiguration." + countryCode;
				return Factory.GetCachedValue(cacheKey, delegate
				{
					return SupplyChainSecurityConfiguration.New(countryCode);
				});
			}
		}

		#endregion

		protected override ZString MasterBill
		{
			get
			{
				return Consol != null && !Consol.IsDeleted ? Consol.JK_MasterBillNum : ZString.Empty;
			}
		}

		protected override IEnumerable<ZString> GetDGCodes()
		{
			return GetDGCodesFromParentBO();
		}

		protected abstract IEnumerable<ZString> GetDGCodesFromParentBO();

		protected override void SetDefaultValuesForStandaloneAWB()
		{
			// Do nothing - Subclasses of this object are not standalone.
		}

		public bool ForceSavingByFactory { get; set; }

		public enum SaveMode
		{
			Normal,
			Never,
			Forced
		}

		public abstract SaveMode FactorySaveMode { get; }

		public override bool IsSavedByFactory
		{
			get
			{
				switch (FactorySaveMode)
				{
					case SaveMode.Forced:
						return true;
					case SaveMode.Never:
						return false;
					default:
						return base.IsSavedByFactory;
				}
			}
		}

		public override bool IsAWBOverridden => Parent?.IsAWBValuesOverriddenProperty ?? false;

		public override bool IsCSDOverridden => Consol?.IsCSDValuesOverriddenProperty ?? false;

		#region ACAS Calculated Properties

		public bool EH_Calculated_ACASInfoOverridden
		{
			get
			{
				return !AWBAccountingInformations.Find(information =>
				{
					var allAccountingCodes = new CodeDescriptionPairList(OLookUpEditType.AWBAccountingCodes);
					return allAccountingCodes.ContainsCode(information.EA_InformationID) && !information.Lookups.AccountingCodes.ContainsCode(information.EA_InformationID);
				}).IsNullOrEmpty();
			}
		}

		public ZString EH_Calculated_ACASShipperEmail
		{
			get
			{
				var accountingValue = AWBAccountingInformations.Find(information => information.EA_InformationID == AccountingCodes.SPE).FirstOrDefault();
				if (accountingValue != null)
				{
					return accountingValue.EA_Information;
				}
				return EH_ShipperContactEmail;
			}
			set
			{
				CreateOrUpdateAccountingInfo(AccountingCodes.SPE, value);
			}
		}

		public ZString EH_Calculated_ACASConsigneeEmail
		{
			get
			{
				var accountingValue = AWBAccountingInformations.Find(information => information.EA_InformationID == AccountingCodes.CNE).FirstOrDefault();
				if (accountingValue != null)
				{
					return accountingValue.EA_Information;
				}
				return EH_ConsigneeContactEmail;
			}
			set
			{
				CreateOrUpdateAccountingInfo(AccountingCodes.CNE, value);
			}
		}

		public ZString EH_Calculated_ACASCustomerAccountName
		{
			get
			{
				var accountingValue = AWBAccountingInformations.Find(information => information.EA_InformationID == AccountingCodes.ANM).FirstOrDefault();
				if (accountingValue != null)
				{
					return accountingValue.EA_Information;
				}
				GetACASCountryHandler().GetCustomerAccountHolderAndName(out _, out var accountName);
				return accountName;
			}
			set
			{
				CreateOrUpdateAccountingInfo(AccountingCodes.ANM, value);
			}
		}

		public ZString EH_Calculated_ACASCustomerAccountIssuer
		{
			get
			{
				var accountingValue = AWBAccountingInformations.Find(information => information.EA_InformationID == AccountingCodes.AIS).FirstOrDefault();
				if (accountingValue != null)
				{
					return accountingValue.EA_Information;
				}
				GetACASCountryHandler().GetCustomerAccountIssuerAndNumber(out var accountIssuer, out _);
				return accountIssuer;
			}
			set
			{
				CreateOrUpdateAccountingInfo(AccountingCodes.AIS, value);
			}
		}

		public ZString EH_Calculated_ACASCustomerAccountNumber
		{
			get
			{
				var accountingValue = AWBAccountingInformations.Find(information => information.EA_InformationID == AccountingCodes.ANB).FirstOrDefault();
				if (accountingValue != null)
				{
					return accountingValue.EA_Information;
				}
				GetACASCountryHandler().GetCustomerAccountIssuerAndNumber(out _, out var accountNumber);
				return accountNumber;
			}
			set
			{
				CreateOrUpdateAccountingInfo(AccountingCodes.ANB, value);
			}
		}

		public ZString EH_Calculated_ACASCustomerAccountHolder
		{
			get
			{
				var accountingValue = AWBAccountingInformations.Find(information => information.EA_InformationID == AccountingCodes.AHD).FirstOrDefault();
				if (accountingValue != null)
				{
					return accountingValue.EA_Information;
				}
				GetACASCountryHandler().GetCustomerAccountHolderAndName(out var accountHolder, out _);
				return accountHolder;
			}
			set
			{
				CreateOrUpdateAccountingInfo(AccountingCodes.AHD, value);
			}
		}

		public ZString EH_Calculated_ACASCustomerAccountShippingFrequency
		{
			get
			{
				var accountingValue = AWBAccountingInformations.Find(information => information.EA_InformationID == AccountingCodes.ASF).FirstOrDefault();
				if (accountingValue != null)
				{
					return accountingValue.EA_Information;
				}
				return GetACASCountryHandler().GetCustomerAccountShippingFrequency();
			}
			set
			{
				CreateOrUpdateAccountingInfo(AccountingCodes.ASF, value);
			}
		}

		public ZBool EH_Calculated_ACASVerifiedKnownConsignor
		{
			get
			{
				var accountingValue = AWBAccountingInformations.Find(information => information.EA_InformationID == AccountingCodes.VKC).FirstOrDefault();
				if (accountingValue != null)
				{
					return accountingValue.EA_Information == bool.TrueString;
				}
				return GetACASCountryHandler().IsVerifiedKnownConsignor();
			}
			set
			{
				CreateOrUpdateAccountingInfo(AccountingCodes.VKC, value.ToString());
			}
		}

		public ZDate EH_Calculated_ACASCustomerAccountEstablishmentDate
		{
			get
			{
				var accountingValue = AWBAccountingInformations.Find(information => information.EA_InformationID == AccountingCodes.AED).FirstOrDefault();
				if (accountingValue != null && DateTime.TryParseExact(accountingValue.EA_Information, "ddMMMyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
				{
					return new ZDate(result);
				}
				GetACASCountryHandler().GetCustomerAccountEstablishmentDate(out var establishmentDateString);
				var establishmentDate = DateTime.TryParseExact(establishmentDateString, "ddMMMyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var establishmentDateResult) ? establishmentDateResult : ZDateTime.Empty;
				return new ZDate(establishmentDate);
			}
			set
			{
				CreateOrUpdateAccountingInfo(AccountingCodes.AED, value.ToString("ddMMMyy", CultureInfo.InvariantCulture));
			}
		}

		public ZString EH_Calculated_ACASCustomerAccountBillingType
		{
			get
			{
				var accountingValue = AWBAccountingInformations.Find(information => information.EA_InformationID == AccountingCodes.ABT).FirstOrDefault();
				if (accountingValue != null)
				{
					return accountingValue.EA_Information;
				}
				GetACASCountryHandler().GetCustomerAccountBillingType(out var billingType);
				return billingType;
			}
			set
			{
				CreateOrUpdateAccountingInfo(AccountingCodes.ABT, value);
			}
		}

		public ZString EH_Calculated_ACASBiographicDataType
		{
			get
			{
				var accountingValue = AWBAccountingInformations.Find(information => information.EA_InformationID == AccountingCodes.BDT).FirstOrDefault();
				if (accountingValue != null)
				{
					return accountingValue.EA_Information;
				}
				var (_, idType, _, _) = GetACASCountryHandler().GetBiographicData();
				return idType;
			}
			set
			{
				CreateOrUpdateAccountingInfo(AccountingCodes.BDT, value);
			}
		}

		public ZString EH_Calculated_ACASBiographicDataCountry
		{
			get
			{
				var accountingValue = AWBAccountingInformations.Find(information => information.EA_InformationID == AccountingCodes.BDC).FirstOrDefault();
				if (accountingValue != null)
				{
					return accountingValue.EA_Information;
				}
				var (_, _, idCountry, _) = GetACASCountryHandler().GetBiographicData();
				return idCountry;
			}
			set
			{
				CreateOrUpdateAccountingInfo(AccountingCodes.BDC, value);
			}
		}

		public ZString EH_Calculated_ACASBiographicDataNumber
		{
			get
			{
				var accountingValue = AWBAccountingInformations.Find(information => information.EA_InformationID == AccountingCodes.BDN).FirstOrDefault();
				if (accountingValue != null)
				{
					return accountingValue.EA_Information;
				}
				var (_, _, _, idNumber) = GetACASCountryHandler().GetBiographicData();
				return idNumber;
			}
			set
			{
				CreateOrUpdateAccountingInfo(AccountingCodes.BDN, value);
			}
		}

		void CreateOrUpdateAccountingInfo(ZString code, ZString value)
		{
			var accountingValue = AWBAccountingInformations.Find(information => information.EA_InformationID == code).FirstOrDefault();
			if (accountingValue != null)
			{
				accountingValue.EA_Information = value;
				return;
			}
			var newAccountingValue = AWBAccountingInformations.AddNew();
			newAccountingValue.EA_EH = PK;
			newAccountingValue.EA_InformationID = code;
			newAccountingValue.EA_Information = value;
		}

		#endregion

		#region UniqueIndexFailureHandler

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return new ExportAWBHeaderUniqueIndexFailureHandler(this); }
		}

		#endregion

		#region Harmonized Code

		void SetHarmonizedCode()
		{
			foreach (var hsCode in GetFormattedHarmonisedCodes())
			{
				if (LineNumberOfFirstEmptyRateLine <= 0 || LineNumberOfFirstEmptyNatureAndQtyOfGoods <= 0)
				{
					return;
				}
				var rateLineNumber = Math.Max(LineNumberOfFirstEmptyRateLine, LineNumberOfFirstEmptyNatureAndQtyOfGoods);
				if (rateLineNumber < Constants.NumberOfRateLines)
				{
					var rateLine = AWBRateLines[ExportAWBRateLine.Schema.ER_LineCount, (ZByte)rateLineNumber];
					if (rateLine != null)
					{
						using (rateLine.SuspendSettingHasChanges())
						{
							PopulateNatureAndQtyOfGoodsLine(rateLineNumber, hsCode);
							PopulateHarmonizedCodeType(rateLineNumber);
						}
					}
				}
				else
				{
					return;
				}
			}
		}

		public override ZByte LineCountOfLastHSCode
		{
			get
			{
				var rateLine = AWBRateLines.Cast<ExportAWBRateLine>().LastOrDefault(e => e.IsHSCodeLine);
				return rateLine?.ER_LineCount ?? 0;
			}
		}

		protected virtual StringCollectionX GetFormattedHarmonisedCodes()
		{
			return GetAvailableHarmonisedCodes();
		}

		protected virtual void PopulateHarmonizedCodeType(int rateLineNumber)
		{
		}

		public virtual ZString ShipmentNumberWithEmptyHSCode => ZString.Empty;

		#endregion

		#region HSCodeValidation

		public virtual bool HasInboundToICS2Zone => false;

		public bool IsImportToExportFromTransitingThroughUnitedArabEmirates
		{
			get
			{
				return (IsExportFromCountry(Core.Constants.CountryCodes.UnitedArabEmirates)
					|| IsImportToCountry(Core.Constants.CountryCodes.UnitedArabEmirates)
					|| IsTransitingThrough(Core.Constants.CountryCodes.UnitedArabEmirates))
					&& ConsigneeCategory != OrgConstants.Category.NaturalPersonIndividual
					&& ShipperCategory != OrgConstants.Category.NaturalPersonIndividual;
			}
		}

		public bool IsAnyDischargePortInMorocco
		{
			get
			{
				return (IsImportToCountry(Core.Constants.CountryCodes.Morocco) || IsTransitingThrough(Core.Constants.CountryCodes.Morocco))
					   && ConsigneeCategory != OrgConstants.Category.NaturalPersonIndividual
					   && ShipperCategory != OrgConstants.Category.NaturalPersonIndividual;
			}
		}

		#endregion

		#region Saving

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (saveSucceeded)
			{
				DocumentSettingsPopulated = false;
			}
		}

		#endregion

		#region ISourceIdentifierProvider

		ZGuid ISourceIdentifierProvider.SourceIdentifier => IsInDatabase ? PK : EH_ParentID;

		#endregion
	}
}

#region Test
#if DEBUG

#region ExportAWBHeader Test Methods

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	partial class ExportAWBHeader
	{
		internal void ResetSupplyChainSecurityConfigurationForTesting()
		{
			supplyChainSecurityConfiguration = null;
		}
	}
}

#endregion

#endif
#endregion
