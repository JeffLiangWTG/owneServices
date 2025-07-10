using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.NZ.Business.MasterFiles;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.NZMAFData)]
	public class NZAddInfo : AutoNZAddInfo
	{
		public new class Schema : AutoNZAddInfo.Schema
		{
			public const string Prefix = "ZN";
		}

		#region Constructor

		public NZAddInfo(BusinessObject parent)
			: base(parent.Factory)
		{
			isInitialised = false;
			Parent = parent;
			isInitialised = true;
		}

		public NZAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public NZAddInfo(BusinessObject parent, ZPropertyInfo addInfoProperty)
			: this(parent)
		{
			using (SuspendSettingHasChanges())
			{
				AddInfoProperty = addInfoProperty;
				LoadPropertiesFromString((ZString)addInfoProperty.Value);
			}
		}

		public BusinessObject ParentObject
		{
			get { return Parent; }
		}

		#endregion

		public bool IsLineLevel
		{
			get
			{
				return Parent is CusClassification
					|| Parent is CusClassPartPivot;
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			Validation.ValidateZN_ConcessionCode();
			OtherInfos.RunPreSaveValidation();
			PermitCodes.RunPreSaveValidation();
			ProhibitedCodes.RunPreSaveValidation();
		}

		public override bool IsInDatabaseIncludingChildren
		{
			get { return Parent.IsInDatabase; }
		}

		#region OtherInfos
		public OtherInfoCollection OtherInfos
		{
			get
			{
				if (IsLineLevel)
				{
					return LineOtherInfos;
				}
				else
				{
					return HeaderOtherInfos;
				}
			}
		}
		#endregion

		#region HeaderOtherInfos
		public HeaderOtherInfoCollection HeaderOtherInfos
		{
			get
			{
				if (fHeaderOtherInfos == null)
				{
					fHeaderOtherInfos = new HeaderOtherInfoCollection(Parent.Factory, ZN_OtherInfosInfo);
					fHeaderOtherInfos.LoadFromString(ZN_OtherInfos);
					RegisterEditableChildObject(fHeaderOtherInfos);
				}
				return fHeaderOtherInfos;
			}
		}
		HeaderOtherInfoCollection fHeaderOtherInfos;
		#endregion

		#region LineOtherInfos
		public LineOtherInfoCollection LineOtherInfos
		{
			get
			{
				if (fLineOtherInfos == null)
				{
					fLineOtherInfos = new LineOtherInfoCollection(Parent.Factory, ZN_OtherInfosInfo);
					fLineOtherInfos.LoadFromString(ZN_OtherInfos);
					RegisterEditableChildObject(fLineOtherInfos);
				}
				return fLineOtherInfos;
			}
		}
		LineOtherInfoCollection fLineOtherInfos;
		#endregion

		#region PermitCodes
		public PermitCodeCollection PermitCodes
		{
			get
			{
				if (fPermitCodes == null)
				{
					fPermitCodes = new PermitCodeCollection(Parent.Factory, ZN_PermitCodesInfo);
					fPermitCodes.LoadFromString(ZN_PermitCodes);
					RegisterEditableChildObject(fPermitCodes);
				}
				return fPermitCodes;
			}
		}
		PermitCodeCollection fPermitCodes;
		#endregion

		#region ProhibitedCodes
		public ProhibitedCodeCollection ProhibitedCodes
		{
			get
			{
				if (fProhibitedCodes == null)
				{
					fProhibitedCodes = new ProhibitedCodeCollection(Parent.Factory, ZN_ProhibitedCodesInfo);
					fProhibitedCodes.LoadFromString(ZN_ProhibitedCodes);
					RegisterEditableChildObject(fProhibitedCodes);
				}
				return fProhibitedCodes;
			}
		}
		ProhibitedCodeCollection fProhibitedCodes;
		#endregion

		protected override IValueSetStrategy GetValueSetStrategy()
		{
			return new ValueSetStrategy(Parent);
		}

		class ValueSetStrategy : IValueSetStrategy
		{
			public ValueSetStrategy(BusinessObject parent)
			{
				this.parent = parent;
			}
			readonly BusinessObject parent;

			public void ValueSet(ZPropertyInfo valueThatHasChanged, IZType oldValue)
			{
				parent.MarkAsNeedingValidation();
			}
		}

		#region Stop Light Validation Testing giving false positives. ValueSetStrategy works in practice.

#if DEBUG
		[LightValidationTestExempt]
		public override ZString ZN_TSWCombinedStatus { get => base.ZN_TSWCombinedStatus; set => base.ZN_TSWCombinedStatus = value; }
		[LightValidationTestExempt]
		public override ZDecimal ZN_AntiDumpingDuty { get => base.ZN_AntiDumpingDuty; set => base.ZN_AntiDumpingDuty = value; }
		[LightValidationTestExempt]
		public override ZString ZN_ConcessionCode { get => base.ZN_ConcessionCode; set => base.ZN_ConcessionCode = value; }
		[LightValidationTestExempt]
		public override ZString ZN_MAF_AccountHolder { get => base.ZN_MAF_AccountHolder; set => base.ZN_MAF_AccountHolder = value; }
		[LightValidationTestExempt]
		public override ZString ZN_GoodsLocatedAt { get => base.ZN_GoodsLocatedAt; set => base.ZN_GoodsLocatedAt = value; }
		[LightValidationTestExempt]
		public override ZString ZN_MPIAccountHolder { get => base.ZN_MPIAccountHolder; set => base.ZN_MPIAccountHolder = value; }
		[LightValidationTestExempt]
		public override ZString ZN_ImporterName { get => base.ZN_ImporterName; set => base.ZN_ImporterName = value; }
		[LightValidationTestExempt]
		public override ZBool ZN_IsFormalChangedToIPI { get => base.ZN_IsFormalChangedToIPI; set => base.ZN_IsFormalChangedToIPI = value; }
		[LightValidationTestExempt]
		public override ZString ZN_IsZeroRatedAll { get => base.ZN_IsZeroRatedAll; set => base.ZN_IsZeroRatedAll = value; }
		[LightValidationTestExempt]
		public override ZString ZN_MAF_AccountNumber { get => base.ZN_MAF_AccountNumber; set => base.ZN_MAF_AccountNumber = value; }
		[LightValidationTestExempt]
		public override ZString ZN_MAF_CargoType { get => base.ZN_MAF_CargoType; set => base.ZN_MAF_CargoType = value; }
		[LightValidationTestExempt]
		public override ZString ZN_MAF_ConsignmentNumber { get => base.ZN_MAF_ConsignmentNumber; set => base.ZN_MAF_ConsignmentNumber = value; }
		[LightValidationTestExempt]
		public override ZString ZN_MAF_ConsignmentType { get => base.ZN_MAF_ConsignmentType; set => base.ZN_MAF_ConsignmentType = value; }
		[LightValidationTestExempt]
		public override ZString ZN_MAF_ExpContactEmail { get => base.ZN_MAF_ExpContactEmail; set => base.ZN_MAF_ExpContactEmail = value; }
		[LightValidationTestExempt]
		public override ZString ZN_MAF_ExpContactFax { get => base.ZN_MAF_ExpContactFax; set => base.ZN_MAF_ExpContactFax = value; }
		[LightValidationTestExempt]
		public override ZString ZN_MAF_ExpContactName { get => base.ZN_MAF_ExpContactName; set => base.ZN_MAF_ExpContactName = value; }
		[LightValidationTestExempt]
		public override ZString ZN_MAF_ExpContactPhone { get => base.ZN_MAF_ExpContactPhone; set => base.ZN_MAF_ExpContactPhone = value; }
		[LightValidationTestExempt]
		public override ZString ZN_MAF_ImpContactEmail { get => base.ZN_MAF_ImpContactEmail; set => base.ZN_MAF_ImpContactEmail = value; }
		[LightValidationTestExempt]
		public override ZString ZN_MAF_ImpContactFax { get => base.ZN_MAF_ImpContactFax; set => base.ZN_MAF_ImpContactFax = value; }
		[LightValidationTestExempt]
		public override ZString ZN_MAF_ImpContactName { get => base.ZN_MAF_ImpContactName; set => base.ZN_MAF_ImpContactName = value; }
		[LightValidationTestExempt]
		public override ZString ZN_MAF_ImpContactPhone { get => base.ZN_MAF_ImpContactPhone; set => base.ZN_MAF_ImpContactPhone = value; }
		[LightValidationTestExempt]
		public override ZString ZN_MAF_IsCustomsCashClient { get => base.ZN_MAF_IsCustomsCashClient; set => base.ZN_MAF_IsCustomsCashClient = value; }
		[LightValidationTestExempt]
		public override ZString ZN_MAF_IsCustomsXRayRequired { get => base.ZN_MAF_IsCustomsXRayRequired; set => base.ZN_MAF_IsCustomsXRayRequired = value; }
		[LightValidationTestExempt]
		public override ZString ZN_MAF_IsMAFAuditRequiredByCustoms { get => base.ZN_MAF_IsMAFAuditRequiredByCustoms; set => base.ZN_MAF_IsMAFAuditRequiredByCustoms = value; }
		[LightValidationTestExempt]
		public override ZString ZN_MAF_MeasurementUQ { get => base.ZN_MAF_MeasurementUQ; set => base.ZN_MAF_MeasurementUQ = value; }
		[LightValidationTestExempt]
		public override ZInt ZN_MAF_MeasurementValue { get => base.ZN_MAF_MeasurementValue; set => base.ZN_MAF_MeasurementValue = value; }
		[LightValidationTestExempt]
		public override ZString ZN_MAF_MessagingStatus { get => base.ZN_MAF_MessagingStatus; set => base.ZN_MAF_MessagingStatus = value; }
		[LightValidationTestExempt]
		public override ZString ZN_MAF_PaymentMethod { get => base.ZN_MAF_PaymentMethod; set => base.ZN_MAF_PaymentMethod = value; }
		[LightValidationTestExempt]
		public override ZString ZN_MAF_ProcessingOffice { get => base.ZN_MAF_ProcessingOffice; set => base.ZN_MAF_ProcessingOffice = value; }
		[LightValidationTestExempt]
		public override ZString ZN_MAF_ReceiptNumber { get => base.ZN_MAF_ReceiptNumber; set => base.ZN_MAF_ReceiptNumber = value; }
		[LightValidationTestExempt]
		public override ZString ZN_ManifestBioStatus { get => base.ZN_ManifestBioStatus; set => base.ZN_ManifestBioStatus = value; }
		[LightValidationTestExempt]
		public override ZString ZN_ManifestNZCSStatus { get => base.ZN_ManifestNZCSStatus; set => base.ZN_ManifestNZCSStatus = value; }
		[LightValidationTestExempt]
		public override ZString ZN_MPIAccountNumber { get => base.ZN_MPIAccountNumber; set => base.ZN_MPIAccountNumber = value; }
		[LightValidationTestExempt]
		public override ZString ZN_MPIBioStatus { get => base.ZN_MPIBioStatus; set => base.ZN_MPIBioStatus = value; }
		[LightValidationTestExempt]
		public override ZString ZN_MPIFoodStatus { get => base.ZN_MPIFoodStatus; set => base.ZN_MPIFoodStatus = value; }
		[LightValidationTestExempt]
		public override ZString ZN_MPIPaymentMethod { get => base.ZN_MPIPaymentMethod; set => base.ZN_MPIPaymentMethod = value; }
		[LightValidationTestExempt]
		public override ZString ZN_MsgTransMode { get => base.ZN_MsgTransMode; set => base.ZN_MsgTransMode = value; }
		[LightValidationTestExempt]
		public override ZString ZN_NZCSStatus { get => base.ZN_NZCSStatus; set => base.ZN_NZCSStatus = value; }
		[LightValidationTestExempt]
		public override ZString ZN_OriginalEntryNumber { get => base.ZN_OriginalEntryNumber; set => base.ZN_OriginalEntryNumber = value; }
		[LightValidationTestExempt]
		public override ZString ZN_OtherInfos { get => base.ZN_OtherInfos; set => base.ZN_OtherInfos = value; }
		[LightValidationTestExempt]
		public override ZString ZN_PartsOfClassification { get => base.ZN_PartsOfClassification; set => base.ZN_PartsOfClassification = value; }
		[LightValidationTestExempt]
		public override ZString ZN_PermitCodes { get => base.ZN_PermitCodes; set => base.ZN_PermitCodes = value; }
		[LightValidationTestExempt]
		public override ZString ZN_ProhibitedCodes { get => base.ZN_ProhibitedCodes; set => base.ZN_ProhibitedCodes = value; }
		[LightValidationTestExempt]
		public override ZString ZN_RL_NKProcessingPort { get => base.ZN_RL_NKProcessingPort; set => base.ZN_RL_NKProcessingPort = value; }
		[LightValidationTestExempt]
		public override ZString ZN_RN_NKCountryOfOrigin { get => base.ZN_RN_NKCountryOfOrigin; set => base.ZN_RN_NKCountryOfOrigin = value; }
		[LightValidationTestExempt]
		public override ZString ZN_SoldOrConsigned { get => base.ZN_SoldOrConsigned; set => base.ZN_SoldOrConsigned = value; }
		[LightValidationTestExempt]
		public override ZString ZN_SupplierName { get => base.ZN_SupplierName; set => base.ZN_SupplierName = value; }
		[LightValidationTestExempt]
		public override ZString ZN_TransactionNature { get => base.ZN_TransactionNature; set => base.ZN_TransactionNature = value; }
#endif

		#endregion

		public ITariff PartsOfClassification
		{
			get
			{
				var dateForDutyRate = ZDateTime.Today;
				var partsOfClassification = UniversalTariffHelper.UseRefDatabaseData ? ZN_PartsOfClassification.Replace(".", ZString.Empty) : ZN_PartsOfClassification;
				return UniversalTariffHelper.GetTariff(Factory, partsOfClassification, dateForDutyRate);
			}
		}

		public override bool HasChanges
		{
			get { return base.HasChanges; }
			set
			{
				base.HasChanges = value;
				if (base.HasChanges && isInitialised)
				{
					UpdateRelatedPropertyInfo();
				}
			}
		}

		#region Serialise/Deserialise

		protected override bool LoadCodeInfoToCollection(string addInfoString, string propertyName)
		{
			CodeDataPairCollection collection;
			switch (propertyName)
			{
				case NZAddInfoSchema.Constants.ZN_PermitCodes:
					collection = PermitCodes;
					break;
				case NZAddInfoSchema.Constants.ZN_OtherInfos:
					collection = OtherInfos;
					break;
				case NZAddInfoSchema.Constants.ZN_ProhibitedCodes:
					collection = ProhibitedCodes;
					break;
				default:
					collection = null;
					break;
			}
			if (collection != null)
			{
				this[propertyName] = addInfoString;
				collection.LoadFromString(addInfoString);
			}
			return collection != null;
		}

		#endregion
	}

	public interface IHaveNZAddInfo
	{
		NZAddInfo AddInfo { get; }
	}
}
