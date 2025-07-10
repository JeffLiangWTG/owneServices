using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.TW.Business
{
	[SystemDefinedValues]
	public class TWJobDocAddress : JobDocAddress, ISupportMultipleResourceStringData
	{
		public TWJobDocAddress(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		const string ZZZDefaultCode = "123";

		#region Schema

		public new class Schema : JobDocAddress.Schema
		{
			public const string AddressDetail = nameof(TWJobDocAddress.AddressDetail);
			public const string LocalAddressDetail = nameof(TWJobDocAddress.LocalAddressDetail);
			public const string ContactDetail = nameof(TWJobDocAddress.ContactDetail);
			public const string IDCode = nameof(TWJobDocAddress.IDCode);
			public const string AEOCode = nameof(TWJobDocAddress.AEOCode);
			public const string TPCCode = nameof(TWJobDocAddress.TPCCode);
			public const string CBPCode = nameof(TWJobDocAddress.CBPCode);
			public const string FRICode = nameof(TWJobDocAddress.FRICode);
			public const string IDCodeType = nameof(TWJobDocAddress.IDCodeType);
			public const string AEOCodeType = nameof(TWJobDocAddress.AEOCodeType);
			public const string TPCCodeType = nameof(TWJobDocAddress.TPCCodeType);
			public const string CBPCodeType = nameof(TWJobDocAddress.CBPCodeType);
			public const string FRICodeType = nameof(TWJobDocAddress.FRICodeType);
		}

		#endregion

		#region Properties
		public ZString AddressDetail => Factory.GetValue(ref addressDetail, delegate
					{
						OrgAddress address;
						var stringBuilder = new ZStringBuilder();
						if (!E2_AddressOverride && (address = Address) != null)
						{
							stringBuilder.AppendIfNotEmpty(address.CompanyName);
							stringBuilder.AppendIfNotEmpty(address.Address1);
							stringBuilder.AppendIfNotEmpty(address.Address2);
							if (!this.IsSupplierOrImporterDocumentaryAddress())
							{
								stringBuilder.AppendIfNotEmpty(address.UnrestrictedAdditionalAddressInformation);
							}

							var cityBuilder = new ZStringBuilder();
							cityBuilder.AppendIfNotEmpty(address.City);
							cityBuilder.AppendIfNotEmpty(address.State);
							cityBuilder.AppendIfNotEmpty(address.CountryName);
							cityBuilder.AppendIfNotEmpty(address.Postcode);

							stringBuilder.AppendIfNotEmpty(cityBuilder.ToStringWithDelimiterBetweenAppends("  "));
						}
						return stringBuilder.ToStringWithNewLineBetweenAppends().ToUpperInvariant();
					});

		CachedProperty<ZString> addressDetail;

		public ZPropertyInfo AddressDetailInfo => GetZPropertyInfo(Schema.AddressDetail);

		public ZString LocalAddressDetail => Factory.GetValue(ref localAddressDetail, delegate
					{
						var result = ZString.Empty;
						if (!E2_AddressOverride)
						{
							var translatedAddress = ChineseTranslatedAddress;
							if (translatedAddress != null)
							{
								IEffectiveAddress wrapper = new OrgTranslatedAddressEffectiveAddress(translatedAddress);
								var stringBuilder = new ZStringBuilder();
								var cityBuilder = new ZStringBuilder();
								cityBuilder.AppendIfNotEmpty(translatedAddress.City);
								cityBuilder.AppendIfNotEmpty(wrapper.GetStateDescription(Core.SharedConstants.Languages.ChineseTraditional));
								cityBuilder.AppendIfNotEmpty(wrapper.GetCountryName(Core.SharedConstants.Languages.ChineseTraditional));
								cityBuilder.AppendIfNotEmpty(translatedAddress.Postcode);

								stringBuilder.AppendIfNotEmpty(translatedAddress.CompanyName);
								stringBuilder.AppendIfNotEmpty(translatedAddress.Address1);
								stringBuilder.AppendIfNotEmpty(translatedAddress.Address2);
								if (!this.IsSupplierOrImporterDocumentaryAddress())
								{
									stringBuilder.AppendIfNotEmpty(translatedAddress.UnrestrictedAdditionalAddressInformation);
								}

								stringBuilder.AppendIfNotEmpty(cityBuilder.ToStringWithDelimiterBetweenAppends("  "));
								result = stringBuilder.ToStringWithNewLineBetweenAppends();
							}
						}
						return result;
					});

		CachedProperty<ZString> localAddressDetail;

		public ZPropertyInfo LocalAddressDetailInfo => GetZPropertyInfo(Schema.LocalAddressDetail);

		public ZString ContactDetail => Factory.GetValue(ref contactDetail, delegate
					{
						OrgAddress address;
						var stringBuilder = new ZStringBuilder();
						if (!E2_AddressOverride && (address = Address) != null)
						{
							stringBuilder.AppendIfNotEmpty(string.Format("{0} ", address.OA_PhoneInfo.HumanReadableName), address.OA_Phone);
							stringBuilder.AppendIfNotEmpty(string.Format("{0} ", address.OA_MobileInfo.HumanReadableName), address.OA_Mobile);
							stringBuilder.AppendIfNotEmpty(string.Format("{0} ", address.OA_FaxInfo.HumanReadableName), address.OA_Fax);
							stringBuilder.AppendIfNotEmpty(address.OA_Email);
						}
						return stringBuilder.ToStringWithNewLineBetweenAppends();
					});

		CachedProperty<ZString> contactDetail;

		public ZPropertyInfo ContactDetailInfo => GetZPropertyInfo(Schema.ContactDetail);

		#region LocalAddress
		public JobDocAddress LocalAddress
		{
			get
			{
				SetupLocalAddress();
				return fLocalAddress;
			}
		}
		JobDocAddress fLocalAddress;

		internal void SetupLocalAddress()
		{
			if (E2_AddressOverride && LocalAddressType != MasterFiles.Integration.DocAddressType.None && (fLocalAddress == null || fLocalAddress.IsDeleted))
			{
				fLocalAddress = Parent?.DocAddresses?.FindOrCreateWithRequirement(GetLocalDocAddressRequirement(LocalAddressType));

				if (fLocalAddress != null && !fLocalAddress.IsInDatabase)
				{
					using (fLocalAddress.SuspendSettingHasChanges())
					{
						fLocalAddress.E2_AddressOverride = true;
					}
				}
			}
		}

		JobDocAddressRequirement GetLocalDocAddressRequirement(DocAddressType addressType)
		{
			switch (addressType)
			{
				case DocAddressType.SupplierTranslatedDocumentaryAddress:
					return new SupplierLocalAddressRequirement(addressType);
				case DocAddressType.ImporterTranslatedDocumentaryAddress:
					return Parent is CusTWControllingMessageHeader header ? new ControllingMessageHeaderImporterLocalAddressRequirement(header, addressType) : new ImporterLocalAddressRequirement(addressType);
				case DocAddressType.LocalProcessorTranslatedDocAddress:
					return new LocalProcessorLocalAddressRequirement(addressType);
				case DocAddressType.BuyerTranslatedDocumentaryAddress:
					return new BuyerLocalAddressRequirement(addressType);
				case DocAddressType.ApplicantTranslatedDocumentaryAddress:
					return new ApplicantLocalAddressRequirement(addressType);
				case DocAddressType.ManufacturerTranslatedDocumentaryAddress:
					return new ManufacturerLocalAddressRequirement(addressType);
				default:
					return new JobDocAddressRequirement(addressType);
			}
		}

		public bool IsLocalProcessor => DocAddressType == DocAddressType.LocalProcessorAddress;

		public bool IsLocalProcessorTranslatedDocAddress => DocAddressType == DocAddressType.LocalProcessorTranslatedDocAddress;

		public bool IsManufacturerTranslatedDocumentaryAddress => DocAddressType == DocAddressType.ManufacturerTranslatedDocumentaryAddress;

		public bool IsSupplierTranslatedDocumentaryAddress => DocAddressType == DocAddressType.SupplierTranslatedDocumentaryAddress;

		public bool IsImporterTranslatedDocumentaryAddress => DocAddressType == DocAddressType.ImporterTranslatedDocumentaryAddress;

		public bool IsImporterDocumentaryAddress => DocAddressType == DocAddressType.ImporterDocumentaryAddress;

		public bool IsSupplierDocumentaryAddress => DocAddressType == DocAddressType.SupplierDocumentaryAddress;

		internal DocAddressType LocalAddressType
		{
			get
			{
				var result = DocAddressType.None;
				if (fLocalAddressType.HasValue)
				{
					result = fLocalAddressType.Value;
				}
				else
				{
					switch (DocAddressType)
					{
						case DocAddressType.SupplierDocumentaryAddress:
							result = DocAddressType.SupplierTranslatedDocumentaryAddress;
							break;
						case DocAddressType.ImporterDocumentaryAddress:
							result = DocAddressType.ImporterTranslatedDocumentaryAddress;
							break;
						case DocAddressType.LocalProcessorAddress:
							result = DocAddressType.LocalProcessorTranslatedDocAddress;
							break;
						case DocAddressType.BuyerDocumentaryAddress:
							result = DocAddressType.BuyerTranslatedDocumentaryAddress;
							break;
						case DocAddressType.Applicant:
							result = DocAddressType.ApplicantTranslatedDocumentaryAddress;
							break;
						case DocAddressType.Manufacturer:
							result = DocAddressType.ManufacturerTranslatedDocumentaryAddress;
							break;
					}
					fLocalAddressType = result;
				}
				return result;
			}
			set
			{
				fLocalAddressType = value;
			}
		}
		DocAddressType? fLocalAddressType;
		#endregion

		[ReadOnlyMember(nameof(IDCodeReadonly))]
		[MaxLength(AutoJobDocAddressNumber.Schema.E2N_NumberMaxLength)]
		public ZString IDCode
		{
			get
			{
				return E2_AddressOverride ? IDAddressNumber?.E2N_Number ?? ZString.Empty : GetIDCode();
			}
			set
			{
				if (E2_AddressOverride)
				{
					var oldValue = IDCode;
					if (oldValue != value && fIDAddressNumber != null)
					{
						fIDAddressNumber.E2N_Number = value;
					}
					if (!IsValidationSuspended)
					{
						Validation.ValidateIDCode();
					}
					IDCodeInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo IDCodeInfo => GetZPropertyInfo(Schema.IDCode);

		internal bool IDCodeReadonly => CodeReadonly || !AddressCodeTypes.IDCodeTypes.Any(x => x.Equals(IDCodeType));

		[MaxLength(AutoJobDocAddressNumber.Schema.E2N_NumberTypeMaxLength)]
		[ReadOnlyMember(nameof(CodeReadonly))]
		[List(nameof(Lookups) + "." + nameof(TWJobDocAddressLookups.IDCodeTypeList))]
		[ResourceStringData("Enterprise.Customs.TW.Business.TWJobDocAddress|IDCodeType", Caption = "ID")]
		[ResourceStringData("Enterprise.Customs.TW.Business.TWJobDocAddress|IDCodeType|SupplierDocumentaryAddress", Caption = "ID", FullDescription = "The supplier's VAT code or passport number or ID card number.", MultipleKey = SupplierDocumentaryAddressCaptionKey)]
		[ResourceStringData("Enterprise.Customs.TW.Business.TWJobDocAddress|IDCodeType|ImporterDocumentaryAddress", Caption = "ID", FullDescription = "The importer's VAT code or passport number or ID card number.", MultipleKey = ImporterDocumentaryAddressCaptionKey)]
		public ZString IDCodeType
		{
			get
			{
				return E2_AddressOverride ? IDAddressNumber?.E2N_NumberType ?? ZString.Empty : GetIDCodeType();
			}
			set
			{
				if (E2_AddressOverride)
				{
					var oldValue = IDCodeType;
					if (oldValue != value)
					{
						if (value.IsEmpty)
						{
							TryDeleteAddressNumber(fIDAddressNumber);
						}
						else
						{
							fIDAddressNumber = DocAddressNumbers.FindOrCreateWithNumberType(value, Core.Constants.CountryCodes.Taiwan, AddressCodeTypes.IDCodeTypes);
						}
					}
					if (!IsValidationSuspended)
					{
						Validation.ValidateIDCodeType();
						Validation.ValidateIDCode();
					}
				}
				IDCodeTypeInfo.RefreshBinding();
				IDCodeInfo.RefreshBinding();
			}
		}

		void TryDeleteAddressNumber(JobDocAddressNumber addressNumber)
		{
			if (addressNumber != null && !addressNumber.IsDeleted)
			{
				addressNumber.Delete();
			}
		}

		public ZPropertyInfo IDCodeTypeInfo => GetZPropertyInfo(Schema.IDCodeType);

		JobDocAddressNumber IDAddressNumber
		{
			get
			{
				if (fIDAddressNumber == null || fIDAddressNumber.IsDeleted)
				{
					fIDAddressNumber = DocAddressNumbers.FindOrCreateWithNumberType(ZString.Empty, Core.Constants.CountryCodes.Taiwan, AddressCodeTypes.IDCodeTypes);
				}
				return fIDAddressNumber;
			}
		}
		JobDocAddressNumber fIDAddressNumber;

		[MaxLength(AutoJobDocAddressNumber.Schema.E2N_NumberMaxLength)]
		[ReadOnlyMember(nameof(CodeReadonly))]
		public ZString AEOCode
		{
			get
			{
				var code = ZString.Empty;
				if (E2_AddressOverride)
				{
					code = AEOAddressNumber?.E2N_Number ?? ZString.Empty;
				}
				else if (Organisation != null && Address != null && Address.PK.IsValid)
				{
					code = GetAEOCode(Organisation, Address.OA_RN_NKCountryCode);
				}
				return code;
			}
			set
			{
				if (E2_AddressOverride)
				{
					var oldValue = AEOCode;
					if (oldValue != value)
					{
						if (fAEOAddressNumber == null || fAEOAddressNumber.IsDeleted)
						{
							fAEOAddressNumber = DocAddressNumbers.FindOrCreateWithNumberType(AEOCodeType, E2_RN_NKCountryCode);
						}
						fAEOAddressNumber.E2N_Number = value;
					}
					if (!IsValidationSuspended)
					{
						Validation.ValidateAEOCode();
					}
				}
				AEOCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AEOCodeInfo => GetZPropertyInfo(Schema.AEOCode);

		[List(nameof(Lookups) + "." + nameof(TWJobDocAddressLookups.AEOCodeTypeList))]
		[ResourceStringData("Enterprise.Customs.TW.Business.TWJobDocAddress|AEOCodeType", Caption = "AEO")]
		[ResourceStringData("Enterprise.Customs.TW.Business.TWJobDocAddress|AEOCodeType|SupplierDocumentaryAddress", Caption = "AEO", FullDescription = "The supplier's Authorized Economic Operator (AEO) number.", MultipleKey = SupplierDocumentaryAddressCaptionKey)]
		[ResourceStringData("Enterprise.Customs.TW.Business.TWJobDocAddress|AEOCodeType|ImporterDocumentaryAddress", Caption = "AEO", FullDescription = "The importer's Authorized Economic Operator (AEO) number.", MultipleKey = ImporterDocumentaryAddressCaptionKey)]
		public ZString AEOCodeType => OrgCusCode.TaiwanCodeTypes.AEO;

		public ZPropertyInfo AEOCodeTypeInfo => GetZPropertyInfo(Schema.AEOCodeType);

		JobDocAddressNumber AEOAddressNumber
		{
			get
			{
				var countryCode = E2_RN_NKCountryCode;
				if (fAEOAddressNumber == null || fAEOAddressNumber.IsDeleted || fAEOAddressNumber.E2N_RN_NKCountryCode != countryCode)
				{
					fAEOAddressNumber = DocAddressNumbers.FindOrCreateWithNumberType(ZString.Empty, countryCode, AddressCodeTypes.AEOCodeTypes);
				}
				return fAEOAddressNumber;
			}
		}
		JobDocAddressNumber fAEOAddressNumber;

		[MaxLength(AutoJobDocAddressNumber.Schema.E2N_NumberMaxLength)]
		[ReadOnlyMember(nameof(CodeReadonly))]
		public ZString TPCCode
		{
			get
			{
				return E2_AddressOverride ? TPCAddressNumber?.E2N_Number ?? ZString.Empty : GetTPCCode(Organisation);
			}
			set
			{
				if (E2_AddressOverride)
				{
					var oldValue = TPCCode;
					if (oldValue != value)
					{
						if (fTPCAddressNumber == null || fTPCAddressNumber.IsDeleted)
						{
							fTPCAddressNumber = DocAddressNumbers.FindOrCreateWithNumberType(TPCCodeType, Core.Constants.CountryCodes.Taiwan);
						}
						fTPCAddressNumber.E2N_Number = value;
					}
					if (!IsValidationSuspended)
					{
						Validation.ValidateTPCCode();
					}
				}
				TPCCodeInfo.RefreshBinding();
			}
		}

		public override ZString E2_RN_NKCountryCode
		{
			get { return base.E2_RN_NKCountryCode; }
			set
			{
				if (fAEOAddressNumber != null && !fAEOAddressNumber.IsDeleted && fAEOAddressNumber.E2N_RN_NKCountryCode != value)
				{
					fAEOAddressNumber.E2N_RN_NKCountryCode = value;
				}
				base.E2_RN_NKCountryCode = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateIDCode();
					IDCodeInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo TPCCodeInfo => GetZPropertyInfo(Schema.TPCCode);

		[List(nameof(Lookups) + "." + nameof(TWJobDocAddressLookups.TPCCodeTypeList))]
		[ResourceStringData("Enterprise.Customs.TW.Business.TWJobDocAddress|TPCCodeType", Caption = "TPC")]
		[ResourceStringData("Enterprise.Customs.TW.Business.TWJobDocAddress|TPCCodeType|SupplierDocumentaryAddress", Caption = "TPC", FullDescription = "The \"tax payment on account\" business identifier for the supplier.", MultipleKey = SupplierDocumentaryAddressCaptionKey)]
		[ResourceStringData("Enterprise.Customs.TW.Business.TWJobDocAddress|TPCCodeType|ImporterDocumentaryAddress", Caption = "TPC", FullDescription = "The \"tax payment on account\" business identifier for the importer.", MultipleKey = ImporterDocumentaryAddressCaptionKey)]
		public ZString TPCCodeType => OrgCusCode.TaiwanCodeTypes.TPC;

		public ZPropertyInfo TPCCodeTypeInfo => GetZPropertyInfo(Schema.TPCCodeType);

		JobDocAddressNumber TPCAddressNumber
		{
			get
			{
				if (fTPCAddressNumber == null || fTPCAddressNumber.IsDeleted)
				{
					fTPCAddressNumber = DocAddressNumbers.FindOrCreateWithNumberType(ZString.Empty, Core.Constants.CountryCodes.Taiwan, AddressCodeTypes.TPCCodeTypes);
				}
				return fTPCAddressNumber;
			}
		}
		JobDocAddressNumber fTPCAddressNumber;

		[MaxLength(AutoJobDocAddressNumber.Schema.E2N_NumberMaxLength)]
		[ReadOnlyMember(nameof(CBPCodeReadonly))]
		public ZString CBPCode
		{
			get
			{
				return E2_AddressOverride ? CBPAddressNumber?.E2N_Number ?? ZString.Empty : GetCBPCode(Address);
			}
			set
			{
				if (E2_AddressOverride)
				{
					var oldValue = CBPCode;
					if (oldValue != value && fCBPAddressNumber != null)
					{
						fCBPAddressNumber.E2N_Number = value;
					}
					if (!IsValidationSuspended)
					{
						Validation.ValidateCBPCode();
					}
				}
				CBPCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CBPCodeInfo => GetZPropertyInfo(Schema.CBPCode);

		bool CBPCodeReadonly => CodeReadonly || !Lookups.CBPCodeTypeList.ContainsCode(CBPCodeType);

		internal bool IsImporterDocAddressAndDeclarationTypeL1 => this.IsImporterDocumentaryAddress() && Parent is JobDeclaration jobDeclaration && jobDeclaration.CusEntryInstruction.CEI_Style == Enterprise.Customs.TW.Business.Constants.DeclarationTypes.Import.L1;

		[MaxLength(AutoJobDocAddressNumber.Schema.E2N_NumberTypeMaxLength)]
		[ReadOnlyMember(nameof(CodeReadonly))]
		[List(nameof(Lookups) + "." + nameof(TWJobDocAddressLookups.CBPCodeTypeList))]
		public ZString CBPCodeType
		{
			get
			{
				return E2_AddressOverride ? CBPAddressNumber?.E2N_NumberType ?? ZString.Empty : GetCBPCodeType(Address);
			}
			set
			{
				if (E2_AddressOverride)
				{
					var oldValue = CBPCodeType;
					if (oldValue != value)
					{
						if (value.IsEmpty)
						{
							TryDeleteAddressNumber(fCBPAddressNumber);
						}
						else
						{
							fCBPAddressNumber = DocAddressNumbers.FindOrCreateWithNumberType(value, Core.Constants.CountryCodes.Taiwan, AddressCodeTypes.CBPCodeTypes);
						}
					}
					if (!IsValidationSuspended)
					{
						Validation.ValidateCBPCodeType();
						Validation.ValidateCBPCode();
					}
				}
				CBPCodeTypeInfo.RefreshBinding();
				CBPCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CBPCodeTypeInfo => GetZPropertyInfo(Schema.CBPCodeType);

		JobDocAddressNumber CBPAddressNumber
		{
			get
			{
				if (fCBPAddressNumber == null || fCBPAddressNumber.IsDeleted)
				{
					fCBPAddressNumber = DocAddressNumbers.FindOrCreateWithNumberType(ZString.Empty, Core.Constants.CountryCodes.Taiwan, AddressCodeTypes.CBPCodeTypes);
				}
				return fCBPAddressNumber;
			}
		}
		JobDocAddressNumber fCBPAddressNumber;

		bool CodeReadonly => !E2_AddressOverride;

		[MaxLength(14)]
		[ReadOnlyMember(nameof(FRICodeReadonly))]
		public ZString FRICode
		{
			get
			{
				return E2_AddressOverride ? FRIAddressNumber?.E2N_Number ?? ZString.Empty : GetFRICode(Address);
			}
			set
			{
				if (E2_AddressOverride)
				{
					var oldValue = FRICode;
					if (oldValue != value && fFRIAddressNumber != null)
					{
						CheckMaximumLength(FRICodeInfo, value);
						fFRIAddressNumber.E2N_Number = value;
					}
					if (!IsValidationSuspended)
					{
						Validation.ValidateFRICode();
					}
					FRICodeInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo FRICodeInfo => GetZPropertyInfo(Schema.FRICode);

		bool FRICodeReadonly => CodeReadonly || !AddressCodeTypes.FRICodeTypes.Any(x => x.Equals(FRICodeType));

		JobDocAddressNumber FRIAddressNumber
		{
			get
			{
				if (fFRIAddressNumber == null || fFRIAddressNumber.IsDeleted)
				{
					fFRIAddressNumber = DocAddressNumbers.FindOrCreateWithNumberType(ZString.Empty, Core.Constants.CountryCodes.Taiwan, AddressCodeTypes.FRICodeTypes);
				}
				return fFRIAddressNumber;
			}
		}
		JobDocAddressNumber fFRIAddressNumber;

		[MaxLength(AutoJobDocAddressNumber.Schema.E2N_NumberTypeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(TWJobDocAddressLookups.FRICodeTypeList))]
		[ReadOnlyMember(nameof(CodeReadonly))]
		public ZString FRICodeType
		{
			get
			{
				return E2_AddressOverride ? FRIAddressNumber?.E2N_NumberType ?? ZString.Empty : GetFRICodeType(Address);
			}
			set
			{
				if (E2_AddressOverride)
				{
					var oldValue = FRICodeType;
					if (oldValue != value)
					{
						if (value.IsEmpty)
						{
							TryDeleteAddressNumber(fFRIAddressNumber);
						}
						else
						{
							fFRIAddressNumber = DocAddressNumbers.FindOrCreateWithNumberType(value, Core.Constants.CountryCodes.Taiwan, AddressCodeTypes.FRICodeTypes);
						}
					}
					if (!IsValidationSuspended)
					{
						Validation.ValidateFRICodeType();
						Validation.ValidateFRICode();
					}
				}
				FRICodeTypeInfo.RefreshBinding();
				FRICodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo FRICodeTypeInfo => GetZPropertyInfo(Schema.FRICodeType);

		#endregion

		#region Lookups

		protected override JobDocAddressLookups GetNewLookups()
		{
			return new TWJobDocAddressLookups(this);
		}

		public new TWJobDocAddressLookups Lookups
		{
			get { return (TWJobDocAddressLookups)base.Lookups; }
		}

		#endregion

		public override ZBool E2_AddressOverride
		{
			get => base.E2_AddressOverride;
			set
			{
				var isChanged = E2_AddressOverride != value;
				if (isChanged)
				{
					if (!value)
					{
						DocAddressNumbers.RemoveAndDeleteAll();
					}

					var localAddress = LocalAddress;
					if (localAddress != null)
					{
						localAddress.E2_AddressOverride = value;
					}
				}

				var translatedAddress = ChineseTranslatedAddress;
				var organisation = Organisation;
				var address = Address;
				base.E2_AddressOverride = value;
				if (isChanged)
				{
					if (!this.IsLocalAddress())
					{
						if (E2_AddressOverride)
						{
							SetAddressOverrideDefaultValues(translatedAddress, organisation, address);
						}
						else
						{
							LocalAddress?.Delete();
							fLocalAddress = null;
						}
					}

					Validation.ValidateCodes();
					if (Parent is BusinessObject parent)
					{
						parent.MarkAsNeedingValidation();
					}
				}
			}
		}

		public override ZGuid E2_OA_Address
		{
			get => base.E2_OA_Address;
			set
			{
				var hasChanged = E2_OA_Address != value;
				base.E2_OA_Address = value;
				if (!IsCopying && hasChanged)
				{
					SetWareHouseDefaultValues();
					if (Parent is CusTWControllingMessageHeader header)
					{
						header.MarkAsNeedingValidation();
					}
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.TWJobDocAddress|E2_Contact", Caption = "Ct. Name")]
		public override ZString E2_Contact { get => base.E2_Contact; set => base.E2_Contact = value; }

		public override bool SupportsDocAddressNumbers => true;

		#region Set Default Values
		void SetAddressOverrideDefaultValues(OrgTranslatedAddress translatedAddress, OrgHeader organisation, OrgAddress address)
		{
			if (!HasRealAddress && address != null)
			{
				E2_Phone = address.OA_Phone;
				E2_Email = address.OA_Email;
				E2_Fax = address.OA_Fax;
			}
			E2_Mobile = ZString.Empty;
			SetLocalAddressDefaultValues(translatedAddress);
			SetCodeDefaultValues(organisation, address);
		}

		void SetLocalAddressDefaultValues(OrgTranslatedAddress translatedAddress)
		{
			if (!HasRealAddress && translatedAddress != null && !ShouldClearAddressFieldsWhenOverride)
			{
				var localAddress = LocalAddress;
				if (localAddress != null)
				{
					localAddress.E2_RN_NKCountryCode = translatedAddress.CountryCodeISO2;
					localAddress.City = translatedAddress.City;
					localAddress.State = translatedAddress.StateCode;
					localAddress.Postcode = translatedAddress.Postcode.Left(localAddress.Postcode_MaxLength);

					localAddress.CompanyName = translatedAddress.CompanyName;
					localAddress.Address1 = translatedAddress.Address1;
					localAddress.Address2 = translatedAddress.Address2;
					localAddress.UnrestrictedAdditionalAddressInformation = translatedAddress.UnrestrictedAdditionalAddressInformation;
				}
			}
		}

		void SetCodeDefaultValues(OrgHeader organisation, OrgAddress address)
		{
			var isLocalProcessorAddress = this.IsLocalProcessorAddress();
			var isManufacturerAddress = this.IsManufacturerAddress();
			var idType = isLocalProcessorAddress ? GetLocalProcessorIDCodeType(organisation, address) : GetIDCodeType(organisation);
			if (!HasRealOrganisation)
			{
				if (!idType.IsEmpty)
				{
					IDCodeType = idType;
					IDCode = isLocalProcessorAddress ? GetLocalProcessorIDCode(organisation, address) : GetIDCode(organisation);
				}
				if (!isLocalProcessorAddress)
				{
					var countryCode = address?.OA_RN_NKCountryCode ?? ZString.Empty;
					AEOCode = countryCode.IsEmpty ? ZString.Empty : GetAEOCode(organisation, countryCode);
					if ((IsExport && this.IsSupplierDocumentaryAddress()) || (IsImport && this.IsImporterDocumentaryAddress()))
					{
						TPCCode = GetTPCCode(organisation);
					}
				}
			}
			if (idType.IsEmpty)
			{
				IDCodeType = AddressCodeTypes.IDCodeTypes.FirstOrDefault();
			}
			if (!isLocalProcessorAddress && !isManufacturerAddress)
			{
				if (!HasRealAddress)
				{
					var cbpType = GetCBPCodeType(address);
					if (!cbpType.IsEmpty)
					{
						CBPCodeType = cbpType;
						CBPCode = GetCBPCode(address);
					}
				}

				if (CBPCodeType.IsEmpty)
				{
					CBPCodeType = IsImporterDocAddressAndDeclarationTypeL1 ? OrgCusCode.CodeTypes.ControlledPremisesID : OrgCusCode.TaiwanCodeTypes.EPZ;
				}
			}

			if (isManufacturerAddress && !HasRealAddress)
			{
				FRICodeType = Parent is JobComInvoiceLine line && line.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().Any(c => c.IsNX101ContainZZZCertificateTypes) ? Constants.OrgCusCodeType.CustomCode : OrgCusCode.TaiwanCodeTypes.FactoryRegistrationNumber;
				FRICode = GetFRICode(address);
			}
		}

		ZString GetIDCode() => this.IsLocalProcessorAddress() ? GetLocalProcessorIDCode(Organisation, Address) : GetIDCode(Organisation);

		void SetWareHouseDefaultValues()
		{
			if (this.IsSupplierDocumentaryAddress())
			{
				SetFromWareHouseDefaultAddress();
			}
			else if (this.IsImporterDocumentaryAddress())
			{
				SetToWareHouseDefaultAddress();
			}
		}

		void SetFromWareHouseDefaultAddress()
		{
			if (Address != null && Parent is JobDeclaration jobDeclaration && TWJobDocAddressHelper.ShouldDefaultWareHouseFromSupplierAddress(jobDeclaration.CusEntryInstruction.CEI_Style))
			{
				string orgCodeType = GetWareHouseCodeType(Address);
				if (OrgHeaderHelper.WareHoseCodeTypes.Contains(orgCodeType))
				{
					jobDeclaration.CusEntryInstruction.CEI_OA_Warehouse = Address.PK;
				}
			}
		}

		void SetToWareHouseDefaultAddress()
		{
			if (Address != null && Parent is JobDeclaration jobDeclaration && TWJobDocAddressHelper.ShouldDefaultWareHouse2FromImporterAddress(jobDeclaration.CusEntryInstruction.CEI_Style))
			{
				string orgCodeType = GetWareHouseCodeType(Address);
				if (OrgHeaderHelper.WareHoseCodeTypes.Contains(orgCodeType))
				{
					jobDeclaration.CusEntryInstruction.CEI_OA_Warehouse2 = Address.PK;
				}
			}
		}

		ZString GetIDCode(OrgHeader organisation) => organisation.GetOrgCusCode(AddressCodeTypes.IDCodeTypes)?.OK_CustomsRegNo ?? ZString.Empty;

		ZString GetLocalProcessorIDCode(OrgHeader organisation, OrgAddress address)
		{
			ZString result;
			if ((organisation != null || address != null) && this.Parent is CusTWControllingMessageHeader header && header.IsNX101ContainZZZCertificateTypes)
			{
				result = ZZZDefaultCode;
			}
			else
			{
				result = organisation.GetLocalProcessorIDOrgCusCode(address)?.OK_CustomsRegNo ?? ZString.Empty;
			}
			return result;
		}

		ZString GetIDCodeType() => this.IsLocalProcessorAddress() ? GetLocalProcessorIDCodeType(Organisation, Address) : GetIDCodeType(Organisation);

		ZString GetIDCodeType(OrgHeader organisation) => organisation.GetOrgCusCode(AddressCodeTypes.IDCodeTypes)?.OK_CodeType ?? ZString.Empty;

		ZString GetLocalProcessorIDCodeType(OrgHeader organisation, OrgAddress address)
		{
			var result = ZString.Empty;
			if ((organisation != null || address != null) && this.Parent is CusTWControllingMessageHeader header && header.IsNX101ContainZZZCertificateTypes)
			{
				result = Constants.OrgCusCodeType.CustomCode;
			}
			else
			{
				result = organisation.GetLocalProcessorIDOrgCusCode(address)?.OK_CodeType ?? ZString.Empty;
			}
			return result;
		}

		ZString GetCBPCodeType(OrgAddress address) => address.GetOrgCusCode(AddressCodeTypes.CBPCodeTypes)?.OK_CodeType ?? ZString.Empty;

		ZString GetCBPCode(OrgAddress address) => address.GetOrgCusCode(AddressCodeTypes.CBPCodeTypes)?.OK_CustomsRegNo ?? ZString.Empty;

		ZString GetWareHouseCodeType(OrgAddress address) => address.GetWareHouseOrgCusCode()?.OK_CodeType ?? ZString.Empty;

		ZString GetAEOCode(OrgHeader organisation, string countryCode) => organisation.GetCustomsRegNo(AEOCodeType, countryCode);

		ZString GetTPCCode(OrgHeader organisation) => organisation.GetCustomsRegNo(TPCCodeType);

		ZString GetFRICodeType(OrgAddress address)
		{
			var result = ZString.Empty;
			if (Parent is JobComInvoiceLine line && address != null)
			{
				var invoiceLineLinkControllingMsgHeaders = line.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>();
				if (invoiceLineLinkControllingMsgHeaders.Any(c => c.IsNX101ContainZZZCertificateTypes))
				{
					result = Constants.OrgCusCodeType.CustomCode;
				}
				else if (invoiceLineLinkControllingMsgHeaders.Any(c => c.IsNX101NotContainZZZCertificateTypes))
				{
					result = address.GetOrgCusCode(new string[] { OrgCusCode.TaiwanCodeTypes.FactoryRegistrationNumber })?.OK_CodeType ?? ZString.Empty;
				}
			}
			return result;
		}

		ZString GetFRICode(OrgAddress address)
		{
			var result = ZString.Empty;
			if (Parent is JobComInvoiceLine line && address != null)
			{
				var invoiceLineLinkControllingMsgHeaders = line.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>();
				if (invoiceLineLinkControllingMsgHeaders.Any(c => c.IsNX101ContainZZZCertificateTypes))
				{
					result = ZZZDefaultCode;
				}
				else if (invoiceLineLinkControllingMsgHeaders.Any(c => c.IsNX101NotContainZZZCertificateTypes))
				{
					result = address.GetOrgCusCode(new string[] { OrgCusCode.TaiwanCodeTypes.FactoryRegistrationNumber })?.OK_CustomsRegNo ?? ZString.Empty;
				}
			}
			return result;
		}

		public OrgTranslatedAddress ChineseTranslatedAddress => Address?.GetTranslatedAddressInSpecificLanguage(Core.SharedConstants.Languages.ChineseTraditional);
		#endregion

		public override void Delete()
		{
			base.Delete();
			var localAddress = LocalAddress;
			if (localAddress != null && !localAddress.IsDeleted)
			{
				localAddress.Delete();
			}
		}

		public ImmutableHashSet<string> ManufactureIDTypes => fManufactureIDTypes ?? (fManufactureIDTypes = new[]
		{
			OrgCusCode.CodeTypes.VATCode,
			OrgCusCode.CodeTypes.PassportID,
			OrgCusCode.TaiwanCodeTypes.PID,
			OrgCusCode.TaiwanCodeTypes.FactoryRegistrationNumber,
		}.ToImmutableHashSet());

		ImmutableHashSet<string> fManufactureIDTypes;

		protected override JobDocAddressValidation GetNewValidation() => new TWJobDocAddressValidation(this);

		public new TWJobDocAddressValidation Validation => (TWJobDocAddressValidation)base.Validation;

		public ZString TypeCode
		{
			get
			{
				var result = ZString.Empty;
				switch (IDCodeType)
				{
					case OrgCusCode.CodeTypes.VATCode:
						result = PartyIdentifierCodeList.Codes._58;
						break;
					case OrgCusCode.CodeTypes.PassportID:
						result = PartyIdentifierCodeList.Codes._53;
						break;
					case OrgCusCode.TaiwanCodeTypes.PID:
						result = PartyIdentifierCodeList.Codes._174;
						break;
					case OrgCusCode.TaiwanCodeTypes.FactoryRegistrationNumber:
						result = PartyIdentifierCodeList.Codes._160;
						break;
					case Constants.OrgCusCodeType.CustomCode:
						result = Constants.OrgCusCodeType.CustomCode;
						break;
				}
				return result;
			}
		}

		public ZBool IsExport => Factory.GetValue(ref isExportCached, () =>
					{
						var result = false;
						var parent = Parent;
						if (parent is JobDeclaration declaration)
						{
							result = declaration.IsExport;
						}
						else if (parent is CusTWControllingMessageHeader controllingMessageHeader)
						{
							result = controllingMessageHeader.EntryInstruction?.JobDeclaration?.IsExport ?? false;
						}
						else if (parent is JobComInvoiceHeader invoice)
						{
							result = invoice.IsExport;
						}
						else if (parent is JobComInvoiceLine line)
						{
							result = line.IsExport;
						}
						return result;
					});

		CachedProperty<ZBool> isExportCached;

		public ZBool IsImport => Factory.GetValue(ref isImportCached, () =>
					{
						var result = false;
						var parent = Parent;
						if (parent is JobDeclaration declaration)
						{
							result = declaration.IsImport;
						}
						else if (parent is CusTWControllingMessageHeader controllingMessageHeader)
						{
							result = controllingMessageHeader.EntryInstruction?.JobDeclaration?.IsImport ?? false;
						}
						else if (parent is JobComInvoiceHeader invoice)
						{
							result = invoice.IsImport;
						}
						else if (parent is JobComInvoiceLine line)
						{
							result = line.IsImport;
						}
						return result;
					});

		CachedProperty<ZBool> isImportCached;

		public bool IsFreeTradeZone
		{
			get
			{
				if (E2_AddressOverride)
				{
					return CBPCodeType == OrgCusCode.TaiwanCodeTypes.FTZ;
				}
				else
				{
					var ftzNumber = Address?.GetCustomsRegNo(new string[] { OrgCusCode.TaiwanCodeTypes.FTZ }) ?? ZString.Empty;
					return !ftzNumber.IsEmpty;
				}
			}
		}

		public ZBool IsIdentificationSameAsLocalProcessAddressForExport(CusTWControllingMessageHeader cMHeader)
		{
			var result = false;
			if (IsExport && cMHeader.LocalProcessorAddress is TWJobDocAddress localProcessorAddress && ManufactureIDTypes.Contains(localProcessorAddress.IDCodeType))
			{
				var localProcessorAddressID = $"{localProcessorAddress.IDCodeType}{localProcessorAddress.IDCode}";
				result = localProcessorAddressID == $"{IDCodeType}{IDCode}" || localProcessorAddressID == $"{FRICodeType}{FRICode}";
			}
			return result;
		}

		public ZString CompanyChineseName => E2_AddressOverride ? LocalAddress.CompanyName : ChineseTranslatedAddress?.CompanyName ?? ZString.Empty;

		public ZString CompanyEnglishName
		{
			get
			{
				if (E2_AddressOverride)
				{
					return E2_CompanyName;
				}
				else
				{
					var result = ZString.Empty;

					if (Address is OrgAddress address)
					{
						result = address.IsEnglish ? address.CompanyName : (address.TranslatedAddresses.FirstOrDefault(x => x.IsEnglish)?.CompanyName ?? ZString.Empty);
					}

					return result;
				}
			}
		}

		#region Lookups

		public IAddressCodeTypes AddressCodeTypes
		{
			get
			{
				if (fAddressCodeTypes == null || !IsLookupsCachedInBase)
				{
					fAddressCodeTypes = GetNewAddressCodeTypes();
				}

				return fAddressCodeTypes;
			}
		}

		IReadOnlyList<string> ISupportMultipleResourceStringData.MultipleKeysToUse
		{
			get
			{
				if (this.IsSupplierDocumentaryAddress())
				{
					return new[] { SupplierDocumentaryAddressCaptionKey };
				}
				else if (this.IsImporterDocumentaryAddress())
				{
					return new[] { ImporterDocumentaryAddressCaptionKey };
				}
				else
				{
					return Array.Empty<string>();
				}
			}
		}

		public const string SupplierDocumentaryAddressCaptionKey = "3454FBED-A498-469A-8080-AB1855A0D88E";

		public const string ImporterDocumentaryAddressCaptionKey = "29BDB5F5-89A5-43B2-B38B-38EFD1161905";

		protected IAddressCodeTypes GetNewAddressCodeTypes()
		{
			if (this.IsControllingMessageDocAddress())
			{
				if (this.IsApplicantAddress())
				{
					return new CMApplicantAddressCodeTypes(this);
				}
				else if (this.IsSupplierDocumentaryAddress())
				{
					return new CMSupplierAddressCodeTypes(this);
				}
				else if (this.IsImporterDocumentaryAddress())
				{
					return new CMImporterAddressCodeTypes(this);
				}
				else if (this.IsLocalProcessorAddress())
				{
					return new LocalProcessorAddressCodeTypes(this);
				}
				else
				{
					return new BaseAddressCodeTypes(this);
				}
			}
			else if (this.IsImporterDocumentaryAddress())
			{
				return new ImporterAddressCodeTypes(this);
			}
			else if (this.IsSupplierDocumentaryAddress())
			{
				return new SupplierAddressCodeTypes(this);
			}
			else if (this.IsManufacturerAddress())
			{
				return new ManufacturerAddressCodeTypes(this);
			}
			else
			{
				return new BaseAddressCodeTypes(this);
			}
		}

		IAddressCodeTypes fAddressCodeTypes;

		#endregion
	}
}
