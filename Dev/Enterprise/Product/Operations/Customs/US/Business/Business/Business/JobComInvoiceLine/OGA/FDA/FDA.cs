using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.US.Business
{
	public class FDA : AutoFDA, IPriorNoticeLine, ICusCodeDataTypeSupporter, IFDARelatedContainer
	{
		public FDA(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoFDA.Schema
		{
			public const string FDAFEIOrgPK = "FDAFEIOrgPK";
			public const string ManufacturerOrgPK = "ManufacturerOrgPK";
			public const string ShipperOrgPK = "ShipperOrgPK";
		}

		#region Related

		public JobComInvoiceLine InvoiceLine
		{
			get { return Factory.Load<JobComInvoiceLine>(B7_ParentID); }
		}

		JobComInvoiceHeader InvoiceHeader
		{
			get
			{
				var invoiceLine = InvoiceLine;
				return invoiceLine != null ? invoiceLine.InvoiceHeader : null;
			}
		}

		internal ZString InvoiceNumber
		{
			get
			{
				var invoiceHeader = InvoiceHeader;
				return invoiceHeader != null ? invoiceHeader.JZ_InvoiceNumber : ZString.Empty;
			}
		}

		public JobDeclaration Declaration
		{
			get
			{
				var invoiceLine = InvoiceLine;
				return invoiceLine != null ? invoiceLine.Declaration : null;
			}
		}

		public OrgAddress ManufacturerAddress
		{
			get { return Factory.Load<OrgAddress>(US_FDAManufacturerAddress); }
		}

		public OrgAddress ShipperAddress
		{
			get { return Factory.Load<OrgAddress>(US_FDAShipperAddress); }
		}

		public OrgAddress FDAFEIAddress
		{
			get { return Factory.Load<OrgAddress>(US_OA_FDAFEI); }
		}

		[ChildEditable(true)]
		public AffirmationCodeCollection AffirmationCodes
		{
			get
			{
				if (affirmationCodes == null)
				{
					affirmationCodes = new AffirmationCodeCollection(this);
					affirmationCodes.Load();
					RegisterEditableChildObject(affirmationCodes);
				}

				return affirmationCodes;
			}
		}
		AffirmationCodeCollection affirmationCodes;

		#region Containers

		//DO NOT CACHE - Memory Held up by local cache
		public FDARelatedContainersCollection ContainersForInvoiceLine
		{
			get { return new FDARelatedContainersCollection(this); }
		}

		[ChildEditable(true)]
		public FDARelatedContainersGenPivotCollection ContainersForFDALine
		{
			get
			{
				if (containersForFDALine == null)
				{
					containersForFDALine = new FDARelatedContainersGenPivotCollection(this);
					containersForFDALine.Load();
					RegisterEditableChildObject(containersForFDALine);
				}

				return containersForFDALine;
			}
		}
		FDARelatedContainersGenPivotCollection containersForFDALine;

		#endregion

		#region Bills

		//DO NOT CACHE - Memory Held up by local cache
		public FDARelatedBillsCollection BillsAvailable
		{
			get { return new FDARelatedBillsCollection(this); }
		}

		[ChildEditable(true)]
		public FDARelatedBillsGenPivotCollection BillsForFDALine
		{
			get
			{
				if (billsForFDALine == null)
				{
					billsForFDALine = new FDARelatedBillsGenPivotCollection(this);
					billsForFDALine.Load();
					RegisterEditableChildObject(billsForFDALine);
				}

				return billsForFDALine;
			}
		}
		FDARelatedBillsGenPivotCollection billsForFDALine;

		#endregion

		#endregion

		#region True/False

		public bool ValueFieldsVisible
		{
			get { return InvoiceLine != null; }
		}

		public bool IsValidFDAProductCodeLength
		{
			get { return US_FDAProductCode.Length == 7; }
		}

		public bool FDAAdmissibilityReviewDONOTSUBMIT
		{
			get
			{
				return InvoiceLine != null &&
					(
						InvoiceLine.ImportTariff != null && InvoiceLine.ImportTariff.FDAAdmissibilityReviewDONOTSUBMIT ||
						InvoiceLine.ImportSupTariff != null && InvoiceLine.ImportSupTariff.FDAAdmissibilityReviewDONOTSUBMIT
					);
			}
		}

		public bool FDAAdmissibilityReviewMayBeRequired
		{
			get
			{
				return InvoiceLine != null &&
					(
						InvoiceLine.ImportTariff != null && InvoiceLine.ImportTariff.FDAAdmissibilityReviewMayBeRequired ||
						InvoiceLine.ImportSupTariff != null && InvoiceLine.ImportSupTariff.FDAAdmissibilityReviewMayBeRequired
					);
			}
		}

		public bool FDAAdmissibilityReviewRequired
		{
			get
			{
				return InvoiceLine != null &&
					(
						InvoiceLine.ImportTariff != null && InvoiceLine.ImportTariff.FDAAdmissibilityReviewRequired ||
						InvoiceLine.ImportSupTariff != null && InvoiceLine.ImportSupTariff.FDAAdmissibilityReviewRequired
					);
			}
		}

		public bool FDAPriorNoticeAndAdmissibilityReviewMayBeRequired
		{
			get
			{
				return InvoiceLine != null &&
					(
						InvoiceLine.ImportTariff != null && InvoiceLine.ImportTariff.FDAPriorNoticeAndAdmissibilityReviewMayBeRequired ||
						InvoiceLine.ImportSupTariff != null && InvoiceLine.ImportSupTariff.FDAPriorNoticeAndAdmissibilityReviewMayBeRequired
					);
			}
		}

		public bool FDAPriorNoticeAndAdmissibilityReviewRequired
		{
			get
			{
				return InvoiceLine != null &&
					(
						InvoiceLine.ImportTariff != null && InvoiceLine.ImportTariff.FDAPriorNoticeAndAdmissibilityReviewRequired ||
						InvoiceLine.ImportSupTariff != null && InvoiceLine.ImportSupTariff.FDAPriorNoticeAndAdmissibilityReviewRequired
					);
			}
		}

		#endregion

		#region New Properties

		public bool HasPNCorPND
		{
			get { return !US_PNC.IsEmpty || US_PND; }
		}

		#region US_FDAManufacturerAddress_ZAddress

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress US_FDAManufacturerAddress_ZAddress
		{
			get
			{
				if (fUS_FDAManufacturerAddress_ZAddress == null)
				{
					fUS_FDAManufacturerAddress_ZAddress = GetNewUS_FDAManufacturerAddress_ZAddress();
					fUS_FDAManufacturerAddress_ZAddress.IsOrgVisible = true;
					fUS_FDAManufacturerAddress_ZAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetMIDAddress);
					fUS_FDAManufacturerAddress_ZAddress.AddresssListOverride = AddressListOverrider.ShowManufacturerIDInAddressList;
				}
				return fUS_FDAManufacturerAddress_ZAddress;
			}
		}
		ZAddress fUS_FDAManufacturerAddress_ZAddress;

		public void RefreshUS_FDAManufacturerAddress_ZAddress()
		{
			fUS_FDAManufacturerAddress_ZAddress = null;
		}

		protected virtual ZAddress GetNewUS_FDAManufacturerAddress_ZAddress()
		{
			return new ZAddress(US_FDAManufacturerAddressInfo);
		}

		[List(nameof(US_FDAManufacturerAddress_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public override ZGuid US_FDAManufacturerAddress
		{
			get
			{
				ZGuid result = base.US_FDAManufacturerAddress;
				if (result.IsEmpty && InvoiceLine != null)
				{
					result = InvoiceLine.JI_OA_ManufacturerAddress;
				}

				return result;
			}
			set
			{
				base.US_FDAManufacturerAddress = value;
				SetManufacturerDefaults();
			}
		}

		OrgHeaderWrapper ManufacturerWrapper
		{
			get
			{
				OrgHeaderWrapper result = null;

				if (ManufacturerAddress != null && ManufacturerAddress.Header != null)
				{
					result = OrgHeaderWrapper.New(ManufacturerAddress.Header);
				}
				else if (InvoiceLine != null && InvoiceLine.ManufacturerAddress != null && InvoiceLine.ManufacturerAddress.Header != null)
				{
					result = OrgHeaderWrapper.New(InvoiceLine.ManufacturerAddress.Header);
				}

				return result;
			}
		}

		#region ManufacturerOrgPK
		[List(nameof(AddInfoLookups) + "." + nameof(USFDAAddInfoLookups.Consignors))]
		public ZGuid ManufacturerOrgPK
		{
			get { return US_FDAManufacturerAddress_ZAddress.OrgPK; }
			set { US_FDAManufacturerAddress_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo ManufacturerOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ManufacturerOrgPK, x => US_FDAManufacturerAddress_ZAddress.OrgPKInfo); }
		}
		#endregion

		#endregion

		#region US_OA_FDAFEI_ZAddress

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress US_OA_FDAFEI_ZAddress
		{
			get
			{
				if (fUS_OA_FDAFEI_ZAddress == null)
				{
					fUS_OA_FDAFEI_ZAddress = GetNewUS_OA_FDAFEI_ZAddress();
					fUS_OA_FDAFEI_ZAddress.IsOrgVisible = true;
					fUS_OA_FDAFEI_ZAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetFEIAddress);
					fUS_OA_FDAFEI_ZAddress.AddresssListOverride = AddressListOverrider.ShowFDAEstablishmentIdentifierInAddressList;
				}
				return fUS_OA_FDAFEI_ZAddress;
			}
		}
		ZAddress fUS_OA_FDAFEI_ZAddress;

		protected virtual ZAddress GetNewUS_OA_FDAFEI_ZAddress()
		{
			return new ZAddress(US_OA_FDAFEIInfo);
		}

		[List(nameof(US_OA_FDAFEI_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public override ZGuid US_OA_FDAFEI
		{
			get { return base.US_OA_FDAFEI; }
			set
			{
				base.US_OA_FDAFEI = value;
				fUS_OA_FDAFEI_ZAddress = null;
			}
		}

		#region FDAFEIOrgPK
		[List(nameof(AddInfoLookups) + "." + nameof(USFDAAddInfoLookups.Consignees))]
		public ZGuid FDAFEIOrgPK
		{
			get { return US_OA_FDAFEI_ZAddress.OrgPK; }
			set { US_OA_FDAFEI_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo FDAFEIOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.FDAFEIOrgPK, x => US_OA_FDAFEI_ZAddress.OrgPKInfo); }
		}
		#endregion

		#endregion

		#region US_FDAShipperAddress_ZAddress

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress US_FDAShipperAddress_ZAddress
		{
			get
			{
				if (fUS_FDAShipperAddress_ZAddress == null)
				{
					fUS_FDAShipperAddress_ZAddress = GetNewUS_FDAShipperAddress_ZAddress();
					fUS_FDAShipperAddress_ZAddress.IsOrgVisible = true;
					fUS_FDAShipperAddress_ZAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetMIDAddress);
					fUS_FDAShipperAddress_ZAddress.AddresssListOverride = AddressListOverrider.ShowManufacturerIDInAddressList;
				}
				return fUS_FDAShipperAddress_ZAddress;
			}
		}
		ZAddress fUS_FDAShipperAddress_ZAddress;

		protected virtual ZAddress GetNewUS_FDAShipperAddress_ZAddress()
		{
			return new ZAddress(US_FDAShipperAddressInfo);
		}

		[List(nameof(US_FDAShipperAddress_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public override ZGuid US_FDAShipperAddress
		{
			get
			{
				var result = ZGuid.Empty;

				if (!base.US_FDAShipperAddress.IsEmpty)
				{
					result = base.US_FDAShipperAddress;
				}
				else if (InvoiceLine is JobComInvoiceLine invoiceLine && InvoiceHeader is JobComInvoiceHeader invoiceHeader)
				{
					result = invoiceLine.JI_OA_FDAShipperAddress;

					var invoiceFDAShipperAddressOrgPk = invoiceHeader.JZ_OA_FDAShipperAddress_ZAddress.OrgPK.IsValid ? invoiceHeader.JZ_OA_FDAShipperAddress_ZAddress.OrgPK : ZGuid.Empty;
					US_FDAShipperAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(invoiceFDAShipperAddressOrgPk);
				}

				return result;
			}
			set
			{
				var valueToMatchAgainst = InvoiceHeader?.JZ_OA_FDAShipperAddress ?? ZGuid.Empty;

				if (value == valueToMatchAgainst)
				{
					base.US_FDAShipperAddress = ZGuid.Empty;
				}
				else
				{
					base.US_FDAShipperAddress = value;
				}

				SetShipperDefaults();
			}
		}

		#region ShipperOrgPK
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.SupplierList))]
		public ZGuid ShipperOrgPK
		{
			get { return US_FDAShipperAddress_ZAddress.OrgPK; }
			set { US_FDAShipperAddress_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo ShipperOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ShipperOrgPK, x => US_FDAShipperAddress_ZAddress.OrgPKInfo); }
		}
		#endregion

		#endregion

		#region Qty Running Total

		public ZString FDAQtyRunningTotal
		{
			get
			{
				if (!fdaQtyRunningTotal.HasValue)
				{
					ZDecimal calculatedQty = GetCalculatedRunningBaseQty();
					fdaQtyRunningTotal = ZString.Empty;

					if (!calculatedQty.IsEmpty)
					{
						fdaQtyRunningTotal = calculatedQty > maxDisplaySize ? "Large Qty - please check" : "Total " + calculatedQty.ToString(2) + " " + US_FDAMeasure1;
					}
				}

				return fdaQtyRunningTotal.Value;
			}
		}
		ZString? fdaQtyRunningTotal;
		readonly ZDecimal maxDisplaySize = 99999999999999.99m;

		void ReCalculateRunningQty()
		{
			fdaQtyRunningTotal = null;
		}

		ZDecimal GetCalculatedRunningBaseQty()
		{
			ZDecimal result = 0m;

			if (US_FDAQty1 > 0)
			{
				try
				{
					result = GetQtyMultiplier(US_FDAQty1) * GetQtyMultiplier(US_FDAQty2) * GetQtyMultiplier(US_FDAQty3) * GetQtyMultiplier(US_FDAQty4) * GetQtyMultiplier(US_FDAQty5) * GetQtyMultiplier(US_FDAQty6);
				}
				catch (OverflowException)
				{
					ZDecimal forceQtyTooBigWarning = maxDisplaySize + 1;
					result = forceQtyTooBigWarning;
				}
			}

			return result;
		}

		ZDecimal GetQtyMultiplier(ZDecimal value)
		{
			return value == 0 ? 1 : value;
		}

		#endregion

		public ZString InvCurrencyCode
		{
			get
			{
				var invoiceHeader = this.InvoiceHeader;
				return invoiceHeader != null ? invoiceHeader.JZ_RX_NKInvoice_Currency : ZString.Empty;
			}
		}

		public ZString FDAValue
		{
			get
			{
				var result = ZString.Empty;
				var invoiceLine = this.InvoiceLine;

				if (invoiceLine == null || invoiceLine.IsOGAValueUpToDate)
				{
					result = US_FDAValue.ToString(0);
				}
				else
				{
					result = "...";
				}

				return result;
			}
		}

		public ZPropertyInfo FDAValueInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(FDAValue), (x) => US_FDAValueInfo); }
		}

		#endregion

		#region Overrides

		internal void SetDefaultFDARelatedBill()
		{
			if (!IsDeleted)
			{
				if (BillsForFDALine.Count > 0)
				{
					var declaration = Declaration;
					var declarationContainsBillsForFDALine = declaration.Bills.OfType<Bill>().Select(x => x.PK).Intersect(BillsForFDALine.Select(y => y.XX_Relation2ID)).Any();

					if (!declarationContainsBillsForFDALine)
					{
						BillsForFDALine.RemoveAndDeleteAll();
					}
				}
				BillsForFDALine.AddMissingPivotIfOnlyOneBill();
			}
		}

		internal void SetFDARelatedContainers()
		{
			if (!IsDeleted)
			{
				ContainersForFDALine.AddMissingPivotIfOnlyOneContainer();
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			US_OFT = OwnerFirmTypeList.Codes.I;
		}

		internal void SetManufacturerDefaults()
		{
			if (ManufacturerAddress != null)
			{
				if (US_UC_NKFDAProduction.IsEmpty && ManufacturerAddress.Header != null && ManufacturerAddress.Header.UNLOCO != null)
				{
					US_UC_NKFDAProduction = ManufacturerAddress.Header.OH_RL_NKClosestPort.Left(2);
				}

				if (US_PFR.IsEmpty && InvoiceLine != null)
				{
					US_PFR = ManufacturerAddress.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.FoodFacilityRegistrationNumber, Core.Constants.CountryCodes.UnitedStates).Left(US_PFRInfo.MaxLength);
				}
			}

			if (ManufacturerWrapper != null)
			{
				if (US_PFT.IsEmpty)
				{
					US_PFT = ManufacturerWrapper.ZO_ProducerFirmType;
				}

				if (US_FME.IsEmpty)
				{
					US_FME = ManufacturerWrapper.ZO_MFRRegExempt;
				}
			}
		}

		internal void SetShipperDefaults()
		{
			if (ShipperAddress != null)
			{
				OrgHeader shipperOrg = (OrgHeader)US_FDAShipperAddress_ZAddress.OrgHeader;
				if (shipperOrg != null)
				{
					if (US_SFR.IsEmpty)
					{
						US_SFR = shipperOrg.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.ShipperRegistrationNumber, Core.Constants.CountryCodes.UnitedStates);
					}
				}
			}
		}

		internal void SetFDAQuantityDefaultsByInvoice()
		{
			var fdaQtyHelper = GetLastFDAQty(false);
			if (fdaQtyHelper.LastFDAQtyInfo != null)
			{
				var unitConverter = InvoiceLine.UnitConverter;
				var defaultValue = unitConverter.Convert(InvoiceLine.JI_InvoiceQuantity, InvoiceLine.JI_InvoiceUQ, fdaQtyHelper.LastFDAUQ);

				if (!fdaQtyHelper.IsBaseFDAQty)
				{
					var baseQuantity = unitConverter.Convert(InvoiceLine.JI_InvoiceQuantity, InvoiceLine.JI_InvoiceUQ, US_FDAMeasure1);
					if (baseQuantity != 0)
					{
						var calculatedRunningBaseQty = GetCalculatedRunningBaseQty();
						if (calculatedRunningBaseQty != ZDecimal.Zero)
						{
							defaultValue = baseQuantity / calculatedRunningBaseQty;
						}
					}
				}

				if (defaultValue != 0 && (fdaQtyHelper.IsBaseFDAQty || fdaQtyHelper.LastFDAQtyInfo.Value.IsEmpty))
				{
					fdaQtyHelper.LastFDAQtyInfo.Value = defaultValue;
				}
			}
		}

		void SetAgainWhenFDAMeasureChanged()
		{
			if (InvoiceLine != null && InvoiceLine.FDAs.Count == 1 && InvoiceLine.Part != null && InvoiceLine.JI_InvoiceQuantity != 0)
			{
				SetFDAQuantityDefaultsByInvoice();
			}
		}

		internal FDAQtyHelper GetLastFDAQty(bool hasQty)
		{
			var result = new FDAQtyHelper();

			if (!US_FDAMeasure6.IsEmpty)
			{
				if ((US_FDAQty6 == 0 && !hasQty) || (US_FDAQty6 != 0 && hasQty))
				{
					result.LastFDAQtyInfo = US_FDAQty6Info;
					result.LastFDAUQ = US_FDAMeasure6;
				}
				return result;
			}

			if (!US_FDAMeasure5.IsEmpty)
			{
				if ((US_FDAQty5 == 0 && !hasQty) || (US_FDAQty5 != 0 && hasQty))
				{
					result.LastFDAQtyInfo = US_FDAQty5Info;
					result.LastFDAUQ = US_FDAMeasure5;
				}
				return result;
			}

			if (!US_FDAMeasure4.IsEmpty)
			{
				if ((US_FDAQty4 == 0 && !hasQty) || (US_FDAQty4 != 0 && hasQty))
				{
					result.LastFDAQtyInfo = US_FDAQty4Info;
					result.LastFDAUQ = US_FDAMeasure4;
				}
				return result;
			}

			if (!US_FDAMeasure3.IsEmpty)
			{
				if ((US_FDAQty3 == 0 && !hasQty) || (US_FDAQty3 != 0 && hasQty))
				{
					result.LastFDAQtyInfo = US_FDAQty3Info;
					result.LastFDAUQ = US_FDAMeasure3;
				}
				return result;
			}

			if (!US_FDAMeasure2.IsEmpty)
			{
				if ((US_FDAQty2 == 0 && !hasQty) || (US_FDAQty2 != 0 && hasQty))
				{
					result.LastFDAQtyInfo = US_FDAQty2Info;
					result.LastFDAUQ = US_FDAMeasure2;
				}
				return result;
			}

			if (!US_FDAMeasure1.IsEmpty)
			{
				result.LastFDAQtyInfo = US_FDAQty1Info;
				result.LastFDAUQ = US_FDAMeasure1;
				result.IsBaseFDAQty = true;
				return result;
			}
			return result;
		}

		public void SetFDADefaultValueForBaseQty()
		{
			if (InvoiceLine != null && InvoiceLine.Part == null && InvoiceLine.RequiresPriorNoticeReporting() && !HasPNCorPND && AddInfoLookups.FDABaseUQs.ContainsCode(InvoiceLine.JI_InvoiceUQ) && US_FDAQty1.IsEmpty)
			{
				US_FDAMeasure1 = InvoiceLine.JI_InvoiceUQ;
				US_FDAQty1 = InvoiceLine.JI_InvoiceQuantity;
			}
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(FDA fda)
				: base(fda)
			{
			}

			protected new FDA BusinessObject
			{
				get { return (FDA)base.BusinessObject; }
			}

			protected override void FetchForLoadChildEditableObjectsCore()
			{
				base.FetchForLoadChildEditableObjectsCore();
				Factory.AddFetchHint(GenPivotSchema.XX_Relation1ID, BusinessObject.PK);
				var genPivotQuery = new ZQuery(GenPivotSchema.XX_Relation1ID, BusinessObject.PK);
				Factory.AddFetchHint(typeof(FDARelatedContainersGenPivot), new ZQuery(GenPivotSchema.XX_RelationType, FDARelatedContainersGenPivot.RelationType), genPivotQuery);
				Factory.AddFetchHint(typeof(FDARelatedBillsGenPivot), new ZQuery(GenPivotSchema.XX_RelationType, FDARelatedBillsGenPivot.RelationType), genPivotQuery);
			}

			protected override void FetchForValidateCore()
			{
				base.FetchForValidateCore();
				Factory.AddFetchHint(CusCodeDataSchema.CY_ParentID, BusinessObject.PK);
				Factory.AddFetchHint(GenPivotSchema.XX_Relation1ID, BusinessObject.PK);
				var genPivotQuery = new ZQuery(GenPivotSchema.XX_Relation1ID, BusinessObject.PK);
				Factory.AddFetchHint(typeof(FDARelatedContainersGenPivot), new ZQuery(GenPivotSchema.XX_RelationType, FDARelatedContainersGenPivot.RelationType), genPivotQuery);
				Factory.AddFetchHint(typeof(FDARelatedBillsGenPivot), new ZQuery(GenPivotSchema.XX_RelationType, FDARelatedBillsGenPivot.RelationType), genPivotQuery);
				Factory.AddFetchHint(OrgCusCodeSchema.OK_OA_PremisesAddress, BusinessObject.US_OA_FDAFEI);
				Factory.AddFetchHint(OrgCusCodeSchema.OK_OH, BusinessObject.US_FDAShipperAddress_ZAddress.OrgPK);
			}
		}

		public new FDAValidation Validation
		{
			get { return (FDAValidation)base.Validation; }
		}

		protected override Customs.Business.MultiLineAddInfos.CusAddInfoValidation GetNewValidation()
		{
			return new FDAValidation(this);
		}

		protected override ZString HumanReadableNameCore
		{
			get { return "FDA"; }
		}

		public override void Delete()
		{
			AffirmationCodes.RemoveAndDeleteAll();
			BillsForFDALine.RemoveAndDeleteAll();
			ContainersForFDALine.RemoveAndDeleteAll();
			US_FDAValue = 0;

			base.Delete();
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			updateAddInfoProperties();

			FDA result = (FDA)base.CloneInternal(args);
			foreach (AffirmationCode affirmationCode in AffirmationCodes)
			{
				result.AffirmationCodes.Add((AffirmationCode)affirmationCode.Clone(new BusinessObjectCloneArgs(args?.AlternativeFactoryToInstantiateCloneIn ?? Factory, Array.Empty<string>(), typeof(AffirmationCode), false)));
			}

			var declaration = Declaration;
			if (declaration != null && declaration.FilteredInvoiceLines.CopyLastLineDetailsToNewLines)
			{
				foreach (FDARelatedBillsGenPivot pivot in BillsForFDALine)
				{
					var bill = pivot.Relation2Object;
					if (bill != null)
					{
						result.BillsForFDALine.AddPivotFor(bill);
					}
				}
			}

			result.US_FDAConfirmDate = ZDateTime.Empty;
			result.US_PNC = ZString.Empty;
			result.US_PND = false;

			return result;
		}

		public void UpdateAddInfoProperties()
		{
			updateAddInfoProperties();
		}

		[DecimalPlaces(2)]
		public override ZDecimal US_InvCurrFDAValue
		{
			get { return base.US_InvCurrFDAValue; }
			set
			{
				base.US_InvCurrFDAValue = value;

				var invoiceLine = this.InvoiceLine;

				if (invoiceLine != null)
				{
					FDAValueInvCurrRunningTotalInfo.RefreshBinding();

					if (invoiceLine.Declaration != null)
					{
						invoiceLine.Declaration.MarkApportionmentDirty();
					}
				}
			}
		}

		public ZDecimal FDAValueInvCurrRunningTotal
		{
			get { return InvoiceLine != null ? InvoiceLine.FDAValueInvCurrRunningTotal : ZDecimal.Zero; }
		}

		public ZPropertyInfo FDAValueInvCurrRunningTotalInfo
		{
			get { return GetZPropertyInfo(nameof(FDAValueInvCurrRunningTotal)); }
		}

		[ReadOnlyMember(nameof(US_FDAValue_ReadOnly))]
		public override ZDecimal US_FDAValue
		{
			get { return base.US_FDAValue; }
			set { base.US_FDAValue = value; }
		}

		bool US_FDAValue_ReadOnly
		{
			get { return true; }
		}

		public override ZDecimal US_FDAQty1
		{
			get { return base.US_FDAQty1; }
			set
			{
				bool hasChanges = base.US_FDAQty1 != value;
				base.US_FDAQty1 = value.Round(USConstants.FDA.RoundToDecimalPlaces);
				if (hasChanges & !IsCopying)
				{
					ReCalculateRunningQty();
				}
			}
		}

		public override ZDecimal US_FDAQty2
		{
			get { return base.US_FDAQty2; }
			set
			{
				bool hasChanges = base.US_FDAQty2 != value;
				base.US_FDAQty2 = value.Round(USConstants.FDA.RoundToDecimalPlaces);
				if (hasChanges & !IsCopying)
				{
					ReCalculateRunningQty();
				}
			}
		}

		public override ZDecimal US_FDAQty3
		{
			get { return base.US_FDAQty3; }
			set
			{
				bool hasChanges = base.US_FDAQty3 != value;
				base.US_FDAQty3 = value.Round(USConstants.FDA.RoundToDecimalPlaces);
				if (hasChanges & !IsCopying)
				{
					ReCalculateRunningQty();
				}
			}
		}

		public override ZDecimal US_FDAQty4
		{
			get { return base.US_FDAQty4; }
			set
			{
				bool hasChanges = base.US_FDAQty4 != value;
				base.US_FDAQty4 = value.Round(USConstants.FDA.RoundToDecimalPlaces);
				if (hasChanges & !IsCopying)
				{
					ReCalculateRunningQty();
				}
			}
		}

		public override ZDecimal US_FDAQty5
		{
			get { return base.US_FDAQty5; }
			set
			{
				bool hasChanges = base.US_FDAQty5 != value;
				base.US_FDAQty5 = value.Round(USConstants.FDA.RoundToDecimalPlaces);
				if (hasChanges & !IsCopying)
				{
					ReCalculateRunningQty();
				}
			}
		}

		public override ZDecimal US_FDAQty6
		{
			get { return base.US_FDAQty6; }
			set
			{
				bool hasChanges = base.US_FDAQty6 != value;
				base.US_FDAQty6 = value.Round(USConstants.FDA.RoundToDecimalPlaces);
				if (hasChanges & !IsCopying)
				{
					ReCalculateRunningQty();
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USFDAAddInfoLookups.FDABaseUQs))]
		public override ZString US_FDAMeasure1
		{
			get { return base.US_FDAMeasure1; }
			set
			{
				bool hasChanges = base.US_FDAMeasure1 != value;
				base.US_FDAMeasure1 = value;
				if (hasChanges & !IsCopying)
				{
					ReCalculateRunningQty();
					SetAgainWhenFDAMeasureChanged();
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USFDAAddInfoLookups.FDAUQs))]
		public override ZString US_FDAMeasure2
		{
			get { return base.US_FDAMeasure2; }
			set
			{
				bool hasChanges = base.US_FDAMeasure2 != value;
				base.US_FDAMeasure2 = value;
				if (hasChanges & !IsCopying)
				{
					ReCalculateRunningQty();
					SetAgainWhenFDAMeasureChanged();
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USFDAAddInfoLookups.FDAUQs))]
		public override ZString US_FDAMeasure3
		{
			get { return base.US_FDAMeasure3; }
			set
			{
				bool hasChanges = base.US_FDAMeasure3 != value;
				base.US_FDAMeasure3 = value;
				if (hasChanges & !IsCopying)
				{
					ReCalculateRunningQty();
					SetAgainWhenFDAMeasureChanged();
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USFDAAddInfoLookups.FDAUQs))]
		public override ZString US_FDAMeasure4
		{
			get { return base.US_FDAMeasure4; }
			set
			{
				bool hasChanges = base.US_FDAMeasure4 != value;
				base.US_FDAMeasure4 = value;
				if (hasChanges & !IsCopying)
				{
					ReCalculateRunningQty();
					SetAgainWhenFDAMeasureChanged();
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USFDAAddInfoLookups.FDAUQs))]
		public override ZString US_FDAMeasure5
		{
			get { return base.US_FDAMeasure5; }
			set
			{
				bool hasChanges = base.US_FDAMeasure5 != value;
				base.US_FDAMeasure5 = value;
				if (hasChanges & !IsCopying)
				{
					ReCalculateRunningQty();
					SetAgainWhenFDAMeasureChanged();
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USFDAAddInfoLookups.FDAUQs))]
		public override ZString US_FDAMeasure6
		{
			get { return base.US_FDAMeasure6; }
			set
			{
				bool hasChanges = base.US_FDAMeasure6 != value;
				base.US_FDAMeasure6 = value;
				if (hasChanges & !IsCopying)
				{
					ReCalculateRunningQty();
					SetAgainWhenFDAMeasureChanged();
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USFDAAddInfoLookups.ProducerFirmTypes))]
		public override ZString US_PFT
		{
			get { return base.US_PFT; }
			set { base.US_PFT = value; }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USFDAAddInfoLookups.CountryList))]
		public override ZString US_CSH
		{
			get { return GetEffectiveValueToReturn(base.US_CSH, JobComInvoiceLine.Schema.US_UC_NKCountryOfExport); }
			set { base.US_CSH = GetEffectiveValueToSet(value, JobComInvoiceLine.Schema.US_UC_NKCountryOfExport); }
		}

		T GetEffectiveValueToReturn<T>(T baseValue, string fieldNameInJobComInvoiceLine) where T : IZType
		{
			T result = baseValue;

			if (result.IsEmpty)
			{
				if (InvoiceLine != null)
				{
					result = (T)InvoiceLine[fieldNameInJobComInvoiceLine];
				}
			}

			return result;
		}

		T GetEffectiveValueToSet<T>(T valuePassed, string fieldNameInJobComInvoiceLine) where T : IZType
		{
			T result = valuePassed;

			if (InvoiceLine != null && InvoiceLine[fieldNameInJobComInvoiceLine].Equals(valuePassed))
			{
				result = (T)valuePassed.Default;
			}

			return result;
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USFDAAddInfoLookups.OwnerFirmTypes))]
		public override ZString US_OFT
		{
			get { return base.US_OFT; }
			set { base.US_OFT = value; }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USFDAAddInfoLookups.FoodFacilityRegistrationExemptionCodes))]
		public override ZString US_FME
		{
			get { return base.US_FME; }
			set { base.US_FME = value; }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USFDAAddInfoLookups.DimensionUQs))]
		public override ZString US_DimUQ
		{
			get { return base.US_DimUQ; }
			set { base.US_DimUQ = value; }
		}

		#endregion

		#region IPriorNoticeLine Members

		IPriorNoticeHeader IPriorNoticeLine.PriorNoticeHeader
		{
			get { return InvoiceHeader != null ? InvoiceHeader.JobDeclaration : null; }
		}

		ZBool IPriorNoticeLine.RequiresPriorNotice
		{
			get
			{
				return US_FDAForcePN
					|| FDAPriorNoticeAndAdmissibilityReviewMayBeRequired
					|| FDAPriorNoticeAndAdmissibilityReviewRequired;
			}
		}

		ZString IPriorNoticeLine.CountryOfShipping
		{
			get { return US_CSH; }
		}

		ZString IPriorNoticeLine.ShipperRegistrationNumber
		{
			get { return US_SFR; }
		}

		ZString IPriorNoticeLine.ConfirmationNumber
		{
			get { return US_PNC; }
		}

		ZBool IPriorNoticeLine.IsDisclaimed
		{
			get { return US_PND; }
		}

		ZString IPriorNoticeLine.FoodFacilityRegistrationExemption
		{
			get { return US_FME; }
		}

		ZString IPriorNoticeLine.FoodFacilityRegistrationNumber
		{
			get { return US_PFR; }
		}

		ZString IPriorNoticeLine.OwnerFirmType
		{
			get { return US_OFT; }
		}

		ZString IPriorNoticeLine.ProducerFirmType
		{
			get { return US_PFT; }
		}

		IEnumerable<ZString> IPriorNoticeLine.RailCarNumbers
		{
			get
			{
				List<ZString> result = new List<ZString>();
				foreach (IUSContainer container in ContainersForFDALine)
				{
					if (container.IsRailCar)
					{
						result.Add(container.ContainerEquipmentID);
					}
				}

				return result;
			}
		}

		IEnumerable<ZString> IPriorNoticeLine.ContainerNumbers
		{
			get
			{
				List<ZString> result = new List<ZString>();

				foreach (IUSContainer containerNumber in ContainersForFDALine)
				{
					if (!containerNumber.IsRailCar)
					{
						result.Add(containerNumber.ContainerEquipmentID);
					}
				}

				return result;
			}
		}

		IEnumerable<IMasterHouse> IPriorNoticeLine.Bills
		{
			get
			{
				List<IMasterHouse> result = new List<IMasterHouse>();

				foreach (IMasterHouse masterHouse in BillsForFDALine)
				{
					result.Add(masterHouse);
				}

				return result;
			}
		}

		ZString IPriorNoticeLine.HarmonizedTariffNumber
		{
			get { return InvoiceLine != null ? InvoiceLine.JI_Tariff : ZString.Empty; }
		}

		OrgHeader IPriorNoticeLine.Importer
		{
			get
			{
				if (importerCached == null)
				{
					importerCached = new CachedProperty<OrgHeader>(Factory, delegate
					{
						var invoice = InvoiceHeader;
						return invoice != null ? invoice.Importer : null;
					});
				}
				return importerCached.Value;
			}
		}
		CachedProperty<OrgHeader> importerCached;

		OrgHeader IPriorNoticeLine.Consignee
		{
			get
			{
				if (consigneeCached == null)
				{
					consigneeCached = new CachedProperty<OrgHeader>(Factory, delegate
					{
						var invoiceLine = InvoiceLine;
						return invoiceLine != null ? invoiceLine.ConsigneeOrgAddress : null;
					});
				}
				return consigneeCached.Value;
			}
		}
		CachedProperty<OrgHeader> consigneeCached;

		#endregion

		#region IFDALine Members

		ZString IFDALine.CommercialDescription
		{
			get { return US_FDACommercialDesc; }
		}

		ZInt IFDALine.FDALineNumber
		{
			get { return US_FDALineNo; }
			set { US_FDALineNo = value; }
		}

		ZString IFDALine.FDAProductCode
		{
			get { return US_FDAProductCode; }
		}

		ZString IFDALine.CargoStorageStatus
		{
			get { return US_FDACargoStorageCode; }
		}

		ZString IFDALine.CountryOfProduction
		{
			get { return US_UC_NKFDAProduction; }
		}

		IReadOnlyList<AffirmationCode> IFDALine.AffirmationCodes
		{
			get { return AffirmationCodes.Cast<AffirmationCode>().ToArray(); }
		}

		ZString IFDALine.ManufacturerNumber
		{
			get { return ManufacturerAddress != null ? ManufacturerAddress.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.ManufacturerID, Core.Constants.CountryCodes.UnitedStates) : ZString.Empty; }
		}

		ZString IFDALine.SupplierOrShipperNumber
		{
			get
			{
				ZString result = ShipperAddress != null ? ShipperAddress.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.ManufacturerID, Core.Constants.CountryCodes.UnitedStates) : ZString.Empty;

				if (result.IsEmpty && InvoiceLine != null)
				{
					if (InvoiceLine.InvoiceHeader.FDAShipperAddress != null)
					{
						result = InvoiceLine.InvoiceHeader.FDAShipperAddress.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.ManufacturerID, Core.Constants.CountryCodes.UnitedStates);
					}
					else if (InvoiceLine.InvoiceHeader.SupplierAddress != null)
					{
						result = InvoiceLine.InvoiceHeader.SupplierAddress.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.ManufacturerID, Core.Constants.CountryCodes.UnitedStates);
					}
				}

				return result;
			}
		}

		List<FDAQtyUQPair> IFDALine.OrderedQtyUQs
		{
			get
			{
				List<FDAQtyUQPair> result = new List<FDAQtyUQPair>();

				if (!US_FDAQty6.IsEmpty)
				{
					result.Add(new FDAQtyUQPair(US_FDAQty6, US_FDAMeasure6));
				}

				if (!US_FDAQty5.IsEmpty)
				{
					result.Add(new FDAQtyUQPair(US_FDAQty5, US_FDAMeasure5));
				}

				if (!US_FDAQty4.IsEmpty)
				{
					result.Add(new FDAQtyUQPair(US_FDAQty4, US_FDAMeasure4));
				}

				if (!US_FDAQty3.IsEmpty)
				{
					result.Add(new FDAQtyUQPair(US_FDAQty3, US_FDAMeasure3));
				}

				if (!US_FDAQty2.IsEmpty)
				{
					result.Add(new FDAQtyUQPair(US_FDAQty2, US_FDAMeasure2));
				}

				if (!US_FDAQty1.IsEmpty)//base unit is the last unit that is added.
				{
					result.Add(new FDAQtyUQPair(US_FDAQty1, US_FDAMeasure1));
				}

				return result;
			}
		}

		ZDecimal IFDALine.ValueInWholeDollars
		{
			get { return US_FDAValue.Round(0); }
			set { US_FDAValue = value; }
		}

		ZString IFDALine.ConsigneeFEI
		{
			get
			{
				var fdaFEIAddress = FDAFEIAddress;
				ZString result = fdaFEIAddress != null ? fdaFEIAddress.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, Core.Constants.CountryCodes.UnitedStates) : ZString.Empty;

				if (result.Length == 10)
				{
					result = result.PadLeft(12, '0');
				}

				return result;
			}
		}

		ZString IFDALine.TradeOrBrandName
		{
			get { return US_TradeBrandName; }
		}

		ZDecimal IFDALine.FirstDimension
		{
			get { return US_ContainerDim1; }
		}

		ZDecimal IFDALine.SecondDimension
		{
			get { return US_ContainerDim2; }
		}

		ZDecimal IFDALine.ThirdDimension
		{
			get { return US_ContainerDim3; }
		}

		ZString IFDALine.DimensionUQ
		{
			get { return US_DimUQ; }
		}

		ZString IFDALine.ContactName
		{
			get { return InvoiceLine != null ? InvoiceLine.InvoiceHeader.US_FDAContactName : ZString.Empty; }
		}

		ZString IFDALine.ContactPhone
		{
			get { return InvoiceLine != null ? InvoiceLine.InvoiceHeader.US_FDAContactPhoneNo : ZString.Empty; }
		}

		ZString IFDALine.ContactEmail
		{
			get { return InvoiceLine != null ? InvoiceLine.InvoiceHeader.US_FDAContactEmail : ZString.Empty; }
		}

		ZString IOGALine.CommercialDesc
		{
			get { return US_FDACommercialDesc; }
			set { US_FDACommercialDesc = value; }
		}

		#endregion

		#region ICusCodeDataTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusCodeDataTypeList.Codes.AffirmationCode, typeof(AffirmationCode));
			return result;
		}

		#endregion

		public class FDAQtyHelper
		{
			internal ZPropertyInfo LastFDAQtyInfo
			{
				get;
				set;
			}

			internal string LastFDAUQ
			{
				get;
				set;
			}

			internal bool IsBaseFDAQty
			{
				get;
				set;
			}
		}
	}

	public interface IUSContainer : IContainerNumber
	{
		ZString USContainerCode { get; }
		ZBool IsRailCar { get; }
	}
}
