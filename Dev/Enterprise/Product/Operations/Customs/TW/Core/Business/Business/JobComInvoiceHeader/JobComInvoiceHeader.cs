using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	public partial class JobComInvoiceHeader : BaseJobComInvoiceHeader, Integration.Customs.TW.IJobComInvoiceHeader
	{
		public JobComInvoiceHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			JZ_IncoTermInfo.ValueChanged += JZ_IncoTermInfo_ValueChanged;
		}

		public new class Schema : BaseJobComInvoiceHeader.Schema
		{
			public const int TW_MarksAndNumbersMaxLength = StmNote.Schema.ST_NoteTextMaxLength;
			public const string TW_MarksAndNumbers = "TW_MarksAndNumbers";
		}

		internal void LoadBeforeValationAll()
		{
			SupplierDocumentaryAddress?.SetupLocalAddress();
			BuyerDocumentaryAddress?.SetupLocalAddress();
		}

		void JZ_IncoTermInfo_ValueChanged(object sender, EventArgs e)
		{
			if (JobDeclaration == null)
			{
				((JobComInvoiceHeader)sender).NeedToGetNewIncoTermAndChargeFactory = true;
			}
			else
			{
				JobDeclaration.RefreshIncotermAndChargeFactory();
			}
		}

		protected override ZString LocalCurrencyCodeCore => Core.Constants.CurrencyCodes.Taiwan;

		protected override CurrencyConverter GetNewCurrencyConverter()
		{
			return new CurrencyConverterWithFixedExchangeRatesDataProvider(Factory, this, false);
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceHeader|JZ_RelatedIndicator", Caption = "Related Indicator", FullDescription = "Indicates the special relationship between buyer and provider, which affects the trading price.")]
		public override ZString JZ_RelatedIndicator { get => base.JZ_RelatedIndicator; set => base.JZ_RelatedIndicator = value; }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			JZ_RelatedIndicator = RelationshipIndicatorList.Codes.NoRelationship;
		}

		protected override string GetStandaloneIncoTermAndChargeFactoryCountryContext()
		{
			return base.GetStandaloneIncoTermAndChargeFactoryCountryContext() + this.GetCustomsChargeTypeListCacheKey();
		}

		#region override Property
		public override ZString JZ_InvoiceCurrExRateType
		{
			get { return base.JZ_InvoiceCurrExRateType; }
			set
			{
				bool hasChanges = base.JZ_InvoiceCurrExRateType != value;
				base.JZ_InvoiceCurrExRateType = value;
				if (hasChanges && !IsCopying)
				{
					JobComInvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		protected override ExchangeRateType RateTypeCore => IsImport ? ExchangeRateType.Customs : IsExport ? ExchangeRateType.CustomsSecondary : base.RateTypeCore;

		protected override bool AllowNonWesternEuropeanCharacterForMarksAndNumbersCore => true;

		[ResourceStringData("A370D557-2BB2-4021-BB1F-0C91A2245AAD", Caption = "L/C Number")]
		public override ZString JZ_LetterOfCreditNumber { get => base.JZ_LetterOfCreditNumber; set => base.JZ_LetterOfCreditNumber = value; }

		[ResourceStringData("07A5D8F4-E3D8-40E5-BC33-E8A6285270EC", Caption = "L/C Date")]
		public override ZDate JZ_LetterOfCreditDate { get => base.JZ_LetterOfCreditDate; set => base.JZ_LetterOfCreditDate = value; }
		#endregion

		HiddenTextNote TW_MarksAndNumbersNote => fTW_MarksAndNumbersNote ?? (fTW_MarksAndNumbersNote = new HiddenTextNote(this, PredefinedNoteTypes.Instance.MarksAndNumbersOverflow.Description));
		HiddenTextNote fTW_MarksAndNumbersNote;

		public ZString TW_MarksAndNumbersOverFlow
		{
			get => TW_MarksAndNumbersNote.Text;
			set => TW_MarksAndNumbersNote.SetNoteText(this, TW_MarksAndNumbersOverFlowInfo, value);
		}

		public ZPropertyInfo TW_MarksAndNumbersOverFlowInfo => GetZPropertyInfo(nameof(TW_MarksAndNumbersOverFlow));

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceHeader|TW_MarksAndNumbers", Caption = "Marks & Numbers", FullDescription = "The marks and numbers printed on the outer package of the cargo.")]
		[MaxLength(Schema.TW_MarksAndNumbersMaxLength)]
		public ZString TW_MarksAndNumbers
		{
			get => JZ_MarksAndNumbers + TW_MarksAndNumbersOverFlow;
			set
			{
				TW_MarksAndNumbersOverFlow = value.SubstringSafe(Schema.JZ_MarksAndNumbersMaxLength);
				JZ_MarksAndNumbers = value.Left(Schema.JZ_MarksAndNumbersMaxLength);
				if (!IsValidationSuspended)
				{
					Validation.ValidateTW_MarksAndNumbers();
				}
				TW_MarksAndNumbersInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo TW_MarksAndNumbersInfo => GetZPropertyInfo(nameof(TW_MarksAndNumbers));

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceHeader|JZ_Remarks", Caption = "Remarks", FullDescription = "The remarks printed on the commercial invoice.")]
		public override ZString JZ_Remarks { get => base.JZ_Remarks; set => base.JZ_Remarks = value; }

		public override ZGuid JZ_OA_ManufacturerAddress
		{
			get => base.JZ_OA_ManufacturerAddress;
			set
			{
				var oldValue = JZ_OA_ManufacturerAddress;
				base.JZ_OA_ManufacturerAddress = value;
				if (!IsCopying && oldValue != JZ_OA_ManufacturerAddress)
				{
					InvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		public override ZGuid JZ_JE
		{
			get { return base.JZ_JE; }
			set
			{
				var oldValue = JZ_JE;
				base.JZ_JE = value;
				if (!IsCopying && JZ_JE != oldValue)
				{
					InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x => x.SetDefaultEntryInstruction());
					SetDefaultValuesFromDeclaration();
				}
			}
		}

		void SetDefaultValuesFromDeclaration()
		{
			if (JZ_MarksAndNumbers.IsEmpty && JobDeclaration != null)
			{
				TW_MarksAndNumbers = JobDeclaration.JE_MarksAndNumbers;
			}
		}

		public override ZDateTime JZ_ValuationDateOverride
		{
			get => base.JZ_ValuationDateOverride;
			set
			{
				var oldValue = JZ_ValuationDateOverride;
				base.JZ_ValuationDateOverride = value;
				if (!IsCopying && oldValue != JZ_ValuationDateOverride)
				{
					JobComInvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		[DecimalPlaces(0)]
		public override ZDecimal JZ_NoOfPacks { get => base.JZ_NoOfPacks; set => base.JZ_NoOfPacks = value; }

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new JobComInvoiceHeaderFetchStrategy(this);

		public ZString EffectiveCurrencyCode
		{
			get
			{
				ZString result;
				if (JobDeclaration != null && FirstEntryHeader is CusEntryHeader entryHeader)
				{
					result = entryHeader.FirstInvoiceCurrencyCode;
				}
				else
				{
					result = JZ_RX_NKInvoice_Currency;
				}
				return result;
			}
		}

		public override ZString JZ_IncoTerm
		{
			get => base.JZ_IncoTerm;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.JZ_IncoTerm))
				{
					var oldValue = JZ_IncoTerm;
					base.JZ_IncoTerm = value;
					if (!IsCopying && oldValue != JZ_IncoTerm)
					{
						SetIncoTermPlaceIfRequired();
					}
				}
			}
		}

		public override ZGuid JZ_OH_Buyer
		{
			get => base.JZ_OH_Buyer;
			set
			{
				var oldValue = JZ_OH_Buyer;
				base.JZ_OH_Buyer = value;
				if (!IsCopying && oldValue != JZ_OH_Buyer)
				{
					InvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		protected override JobComInvoiceHeaderDeepCopyStrategy GetTemplateCopyStrategy(CloneType cloneType)
		{
			return new JobComInvoiceHeaderDeepCloneStrategy(this, cloneType);
		}

		public override ZString JZ_RX_NKInvoice_Currency
		{
			get => base.JZ_RX_NKInvoice_Currency;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.JZ_RX_NKInvoice_Currency))
				{
					var oldValue = JZ_RX_NKInvoice_Currency;
					base.JZ_RX_NKInvoice_Currency = value;
					if (!IsCopying && JZ_RX_NKInvoice_Currency != oldValue)
					{
						JobComInvoiceLines.MarkAsNeedingValidation();
					}
				}
			}
		}

		public void SetIncoTermPlaceIfRequired(bool appendZ99 = false)
		{
			var declaration = JobDeclaration;
			if (declaration != null && (JZ_IncoTermPlace.IsEmpty || appendZ99))
			{
				var result = ZString.Empty;
				var incoTerm = JZ_IncoTerm;
				if (incoTerm == Core.Constants.IncoTerms.FreeOnBoard || incoTerm == Core.Constants.IncoTerms.ExWorks || incoTerm == Core.Constants.IncoTerms.FreeAlongsideShip)
				{
					result = declaration.PortOfOriginCountry?.RN_Desc ?? ZString.Empty;
				}
				else if (incoTerm == Core.Constants.IncoTerms.CostInsuranceAndFreight || incoTerm == Core.Constants.IncoTerms.CostAndFreight || incoTerm == Core.Constants.IncoTerms.CostAndInsurance)
				{
					var finalDestinationZStringBuilder = new ZStringBuilder();
					finalDestinationZStringBuilder.AppendIfNotEmpty(declaration.FinalDestinationCountry?.RN_Desc ?? ZString.Empty);
					finalDestinationZStringBuilder.AppendIfNotEmpty(declaration.FinalDestinationName);
					result = finalDestinationZStringBuilder.ToStringWithDelimiterBetweenAppends(" ");
				}

				if (!result.IsEmpty)
				{
					JZ_IncoTermPlace = result.Left(JobComInvoiceHeader.Schema.JZ_IncoTermPlaceMaxLength);
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceHeader|JZ_IncoTermPlace", Caption = "Agreed Place")]
		public override ZString JZ_IncoTermPlace { get => base.JZ_IncoTermPlace; set => base.JZ_IncoTermPlace = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceHeader|JZ_Description", Caption = "Goods Description")]
		public override ZString JZ_Description { get => base.JZ_Description; set => base.JZ_Description = value; }

		public bool JustAddedByDataObjectReader { get; set; }

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (saveSucceeded)
			{
				JustAddedByDataObjectReader = false;
			}
		}

		protected override IComparable OrderByColumnCore => JZ_InvoiceDisplaySequence;

		internal void RebuildInvoiceQuantityAndUnitQtyResultIfNeeded()
		{
			InvoiceLines.Cast<JobComInvoiceLine>().Where(x => x.IsInvoiceQuantityAndUnitQtyResultCollectionLoaded).ForEach(x => x.InvoiceQuantityAndUnitQtyResultCollection.ShouldRebuildElements());
		}

		protected override JobDocAddressDependentCollection GetDocAddressesCore()
		{
			var docAddresses = new TWJobDocAddressDependentCollection(this);
			docAddresses.Load();
			RegisterEditableChildObject(docAddresses);
			return docAddresses;
		}

		#region SupplierDocumentaryAddress

		public TWJobDocAddress SupplierDocumentaryAddress
		{
			get
			{
				SetupSupplierJobDocAddresses();
				return fSupplierDocumentaryAddress;
			}
		}
		TWJobDocAddress fSupplierDocumentaryAddress;

		void SetupSupplierJobDocAddresses()
		{
			if (fSupplierDocumentaryAddress == null || fSupplierDocumentaryAddress.IsDeleted)
			{
				SupplierDocAddressRequirement = AddSupplierDocAddressRequirement();
				fSupplierDocumentaryAddress = (TWJobDocAddress)DocAddresses.FindOrCreateWithRequirement(SupplierDocAddressRequirement);
				if (fSupplierDocumentaryAddress != null)
				{
					fSupplierDocumentaryAddress.DocAddressChanged += new EventHandler(SupplierDocumentaryAddressChanged);
				}
			}
		}

		void SupplierDocumentaryAddressChanged(object sender, EventArgs e)
		{
			var supplier = ZGuid.Empty;
			if (SupplierDocumentaryAddress is TWJobDocAddress supplierDocumentaryAddress && !supplierDocumentaryAddress.E2_AddressOverride)
			{
				supplier = supplierDocumentaryAddress.OrganisationPK;
			}
			if (JZ_OH_Supplier != supplier)
			{
				JZ_OH_Supplier = supplier;
			}
		}

		#endregion

		#region SupplierDocAddressRequirement

		protected JobDocAddressRequirement SupplierDocAddressRequirement
		{
			get { return fSupplierDocAddressRequirement ?? (fSupplierDocAddressRequirement = AddSupplierDocAddressRequirement()); }
			set { fSupplierDocAddressRequirement = value; }
		}
		JobDocAddressRequirement fSupplierDocAddressRequirement;

		protected JobDocAddressRequirement AddSupplierDocAddressRequirement()
		{
			var requirement = GetSupplierDocAddressRequirement();
			DocAddressManager.AddRequirement(requirement);
			return requirement;
		}

		protected JobDocAddressRequirement GetSupplierDocAddressRequirement()
		{
			return new SupplierAddressRequirement(DocAddressType.SupplierDocumentaryAddress, ContactType.Consignor);
		}

		#endregion

		#region BuyerDocumentaryAddress

		public TWJobDocAddress BuyerDocumentaryAddress
		{
			get
			{
				SetupBuyerJobDocAddresses();
				return fBuyerDocumentaryAddress;
			}
		}
		TWJobDocAddress fBuyerDocumentaryAddress;

		void SetupBuyerJobDocAddresses()
		{
			if (fBuyerDocumentaryAddress == null || fBuyerDocumentaryAddress.IsDeleted)
			{
				BuyerDocAddressRequirement = AddBuyerDocAddressRequirement();
				fBuyerDocumentaryAddress = (TWJobDocAddress)DocAddresses.FindOrCreateWithRequirement(BuyerDocAddressRequirement);
				if (fBuyerDocumentaryAddress != null)
				{
					fBuyerDocumentaryAddress.DocAddressChanged += new EventHandler(BuyerDocumentaryAddressChanged);
				}
			}
		}

		void BuyerDocumentaryAddressChanged(object sender, EventArgs e)
		{
			var buyer = ZGuid.Empty;
			if (BuyerDocumentaryAddress is TWJobDocAddress buyerDocumentaryAddress && !buyerDocumentaryAddress.E2_AddressOverride)
			{
				buyer = buyerDocumentaryAddress.OrganisationPK;
			}
			if (JZ_OH_Buyer != buyer)
			{
				JZ_OH_Buyer = buyer;
			}
		}

		#endregion

		#region BuyerDocAddressRequirement

		protected JobDocAddressRequirement BuyerDocAddressRequirement
		{
			get { return fBuyerDocAddressRequirement ?? (fBuyerDocAddressRequirement = AddBuyerDocAddressRequirement()); }
			set { fBuyerDocAddressRequirement = value; }
		}
		JobDocAddressRequirement fBuyerDocAddressRequirement;

		protected JobDocAddressRequirement AddBuyerDocAddressRequirement()
		{
			var requirement = GetBuyerDocAddressRequirement();
			DocAddressManager.AddRequirement(requirement);
			return requirement;
		}

		protected JobDocAddressRequirement GetBuyerDocAddressRequirement()
		{
			return new BuyerAddressRequirement(DocAddressType.BuyerDocumentaryAddress, ContactType.Consignee);
		}

		#endregion

		#region DocAddress Requirements

		protected override DocAddressType[] SupportedAddressTypesCore()
		{
			return new DocAddressType[] { DocAddressType.SupplierDocumentaryAddress, DocAddressType.BuyerDocumentaryAddress };
		}

		#endregion

		internal CusPackingList LoadCusPackingList(BusinessObjectFactory factory)
		{
			var query = new ZQuery(CusPackingListSchema.CUL_JZ, PK);
			query.OrderBy = CusPackingListSchema.Constants.CUL_SystemCreateTimeUtc;
			return factory.LoadTop1<CusPackingList>(query);
		}

		internal CusPackingList CreateCusPackingList(BusinessObjectFactory factory)
		{
			var packingList = factory.New<CusPackingList>();
			packingList.CUL_JZ = PK;
			packingList.CUL_PackingListDate = JZ_InvoiceDate.Date;
			packingList.CUL_Description = JZ_Description;
			return packingList;
		}

		public ZBool HasCusPackingList => IsInDatabase && LoadCusPackingList(new BusinessObjectFactory()) != null;

		public int GetTWInvoiceLineMaxDecimalPlaces(string fieldName) => DecimalPlacesCalculator.GetTWInvoiceLineMaxDecimalPlaces(fieldName);

		TWInvoiceLineDecimalPlacesCalculator DecimalPlacesCalculator => decimalPlacesCalculator ??= new TWInvoiceLineDecimalPlacesCalculator(this);
		TWInvoiceLineDecimalPlacesCalculator decimalPlacesCalculator;

		public ZBool HasInternationFreightAmount => Factory.GetCached(ref hasInternationFreightAmountCached, () =>
		{
			var allCharges = Charges.Cast<Common.JobComInvCharge>().ToList();
			if (GroupHeader != null)
			{
				allCharges = [.. allCharges, .. GroupHeader.Charges.Cast<Common.JobComInvCharge>()];
			}
			return allCharges.Any(c => c.J7_ChargeType == Common.CustomsChargeTypeList.Codes.OverseasFreight && c.J7_Amount > 0);
		});
		public CachedProperty<ZBool> hasInternationFreightAmountCached;

		public override ZShort JZ_InvoiceDisplaySequence
		{
			get => base.JZ_InvoiceDisplaySequence;
			set
			{
				var oldValue = base.JZ_InvoiceDisplaySequence;
				base.JZ_InvoiceDisplaySequence = value;
				if (!IsCopying && value != oldValue)
				{
					InvoiceLines.MarkAsNeedingValidation();
					InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x => x.Validation.ValidateJI_Procedure());
				}
			}
		}
	}
}
