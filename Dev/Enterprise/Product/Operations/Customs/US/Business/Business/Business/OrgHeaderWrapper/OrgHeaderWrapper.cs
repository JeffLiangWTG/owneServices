using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.US;
using static Enterprise.MasterFiles.Business.OrgConstants;

namespace Enterprise.Customs.US.Business
{
	[SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	public class OrgHeaderWrapper : NonPersistentBusinessObject
		, IObsoleteValidation
		, IMessageAttachee
		, IPGAContactDetails
		, IUSIORWrapper
	{
		protected OrgHeaderWrapper(OrgHeader organisation)
			: base(organisation.Factory)
		{
			this.organisation = organisation;
			this.orgAddress = GetAddressForOrganisation(organisation);
		}

		protected OrgHeaderWrapper(OrgHeader organisation, Func<OrgHeader, OrgAddress> getAddress)
			: base(organisation.Factory)
		{
			this.organisation = organisation;
			this.orgAddress = getAddress(organisation);
		}

		OrgHeaderWrapper(OrgAddress orgAddress)
			: base(orgAddress.Factory)
		{
			this.orgAddress = orgAddress;
			this.organisation = orgAddress.Header;
		}

		#region static New

		public static OrgHeaderWrapper New(OrgHeader organisation)
		{
			return New(organisation, x => GetAddressForOrganisation(x));
		}

		public static OrgHeaderWrapper New(OrgHeader organisation, Func<OrgHeader, OrgAddress> getAddress)
		{
			OrgHeaderWrapper result = null;

			if (organisation != null)
			{
				result = organisation.Factory.GetCachedValue(organisation.PK.ToStringKey(), delegate
				{
					return new OrgHeaderWrapper(organisation, getAddress);
				});
			}
			if (result != null)
			{
				result.SetDefaultAllocationTypes();
			}

			return result;
		}

		public static OrgHeaderWrapper New(OrgAddress orgAddress)
		{
			OrgHeaderWrapper result = null;

			if (orgAddress != null)
			{
				result = orgAddress.Factory.GetCachedValue(orgAddress.PK.ToStringKey(), delegate
				{
					return new OrgHeaderWrapper(orgAddress);
				});
			}
			if (result != null)
			{
				result.SetDefaultAllocationTypes();
			}

			return result;
		}

		public static OrgHeaderWrapper New(OrgAddress address, IEnumerable<string> allocationTypes)
		{
			var orgWrapper = New(address);
			orgWrapper.allocationTypes = allocationTypes;
			return orgWrapper;
		}
		IEnumerable<string> allocationTypes = new string[] { ContactAllocationType.USPGA };

		void SetDefaultAllocationTypes()
		{
			allocationTypes = new string[] { ContactAllocationType.USPGA };
		}

		#endregion

		readonly OrgAddress orgAddress;
		public readonly OrgHeader organisation;

		#region Bindable Properties

		#region ZO_NPID

		public ZString ZO_NPID
		{
			get { return ImportAddInfo.ZO_NPID; }
			set { ImportAddInfo.ZO_NPID = value; }
		}

		public bool ZO_NPID_ReadOnly
		{
			get { return ZO_OH_NP.IsValid; }
		}

		public ZPropertyInfo ZO_NPIDInfo
		{
			get { return ImportAddInfo == null ? null : GetWrappedZPropertyInfo(USOrgImpAddInfoSchema.Constants.ZO_NPID, x => ImportAddInfo.ZO_NPIDInfo); }
		}

		#endregion

		#region ZO_OH_NP

		[List(nameof(ImportAddInfoLookups) + "." + nameof(USOrgImpAddInfoLookups.NotifyParties))]
		public ZGuid ZO_OH_NP
		{
			get { return ImportAddInfo.ZO_OH_NP; }
			set { ImportAddInfo.ZO_OH_NP = value; }
		}

		public ZPropertyInfo ZO_OH_NPInfo
		{
			get { return ImportAddInfo == null ? null : GetWrappedZPropertyInfo(USOrgImpAddInfoSchema.Constants.ZO_OH_NP, x => ImportAddInfo.ZO_OH_NPInfo); }
		}

		public OrgHeader NotifyParty
		{
			get { return Factory.Load<OrgHeader>(ZO_OH_NP); }
		}

		#endregion

		#region ZO_OA_ConsigneeAddress

		[RelatedBusinessObject("ConsigneeAddress")]
		[List(nameof(ZO_OA_ConsigneeAddress_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public ZGuid ZO_OA_ConsigneeAddress
		{
			get { return ImportAddInfo.ZO_OA_ConsigneeAddress; }
			set { ImportAddInfo.ZO_OA_ConsigneeAddress = value; }
		}

		public ZPropertyInfo ZO_OA_ConsigneeAddressInfo
		{
			get { return ImportAddInfo == null ? null : GetWrappedZPropertyInfo(USOrgImpAddInfoSchema.Constants.ZO_OA_ConsigneeAddress, x => ImportAddInfo.ZO_OA_ConsigneeAddressInfo); }
		}

		public OrgAddress ConsigneeAddress
		{
			get { return Factory.Load<OrgAddress>(ZO_OA_ConsigneeAddress); }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress ZO_OA_ConsigneeAddress_ZAddress
		{
			get
			{
				if (fZO_OA_ConsigneeAddress_ZAddress == null)
				{
					fZO_OA_ConsigneeAddress_ZAddress = new ZAddress(ZO_OA_ConsigneeAddressInfo);
					fZO_OA_ConsigneeAddress_ZAddress.IsOrgVisible = true;
					fZO_OA_ConsigneeAddress_ZAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetDeliveryAddress);
				}
				return fZO_OA_ConsigneeAddress_ZAddress;
			}
		}
		ZAddress fZO_OA_ConsigneeAddress_ZAddress;

		#endregion

		#region ZO_OA_ShipToAddress

		[RelatedBusinessObject("ShipToAddress")]
		[List(nameof(ZO_OA_ShipToAddress_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public ZGuid ZO_OA_ShipToAddress
		{
			get { return ImportAddInfo.ZO_OA_ShipToAddress; }
			set { ImportAddInfo.ZO_OA_ShipToAddress = value; }
		}

		public ZPropertyInfo ZO_OA_ShipToAddressInfo
		{
			get { return ImportAddInfo == null ? null : GetWrappedZPropertyInfo(USOrgImpAddInfoSchema.Constants.ZO_OA_ShipToAddress, x => ImportAddInfo.ZO_OA_ShipToAddressInfo); }
		}

		public OrgAddress ShipToAddress
		{
			get { return Factory.Load<OrgAddress>(ZO_OA_ShipToAddress); }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress ZO_OA_ShipToAddress_ZAddress
		{
			get
			{
				if (fZO_OA_ShipToAddress == null)
				{
					fZO_OA_ShipToAddress = new ZAddress(ZO_OA_ShipToAddressInfo);
					fZO_OA_ShipToAddress.IsOrgVisible = true;
					fZO_OA_ShipToAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetDeliveryAddress);
				}
				return fZO_OA_ShipToAddress;
			}
		}
		ZAddress fZO_OA_ShipToAddress;

		#endregion

		#region Importer Type

		public ZString ZO_ImporterType
		{
			get { return ImportAddInfo.ZO_ImporterType; }
			set { ImportAddInfo.ZO_ImporterType = value; }
		}

		public ZPropertyInfo ZO_ImporterTypeInfo
		{
			get { return ImportAddInfo == null ? null : GetWrappedZPropertyInfo(USOrgImpAddInfoSchema.Constants.ZO_ImporterType, x => ImportAddInfo.ZO_ImporterTypeInfo); }
		}

		#endregion

		#region Producer Firm Type

		[List(nameof(ImportAddInfoLookups) + "." + nameof(USOrgImpAddInfoLookups.ProducerFirmTypes))]
		public ZString ZO_ProducerFirmType
		{
			get { return ImportAddInfo.ZO_ProducerFirmType; }
			set { ImportAddInfo.ZO_ProducerFirmType = value; }
		}

		public ZPropertyInfo ZO_ProducerFirmTypeInfo
		{
			get { return ImportAddInfo == null ? null : GetWrappedZPropertyInfo(USOrgImpAddInfoSchema.Constants.ZO_ProducerFirmType, x => ImportAddInfo.ZO_ProducerFirmTypeInfo); }
		}

		#endregion

		#region Submitter Firm Type

		[List(nameof(ImportAddInfoLookups) + "." + nameof(USOrgImpAddInfoLookups.SubmitterFirmTypes))]
		public ZString ZO_SubmitterFirmType
		{
			get { return ImportAddInfo.ZO_SubmitterFirmType; }
			set { ImportAddInfo.ZO_SubmitterFirmType = value; }
		}

		public ZPropertyInfo ZO_SubmitterFirmTypeInfo
		{
			get { return ImportAddInfo == null ? null : GetWrappedZPropertyInfo(USOrgImpAddInfoSchema.Constants.ZO_SubmitterFirmType, x => ImportAddInfo.ZO_SubmitterFirmTypeInfo); }
		}

		#endregion

		#region File Their Own Recon

		public ZBool ZO_FileTheirOwnRecon
		{
			get { return ImportAddInfo.ZO_FileTheirOwnRecon; }
			set { ImportAddInfo.ZO_FileTheirOwnRecon = value; }
		}

		public ZPropertyInfo ZO_FileTheirOwnReconInfo
		{
			get { return ImportAddInfo == null ? null : GetWrappedZPropertyInfo(USOrgImpAddInfoSchema.Constants.ZO_FileTheirOwnRecon, x => ImportAddInfo.ZO_FileTheirOwnReconInfo); }
		}

		#endregion

		#region NAFTA Recon Indicator

		public ZBool ZO_NAFTAReconIndicator
		{
			get { return ImportAddInfo.ZO_NAFTAReconIndicator; }
			set { ImportAddInfo.ZO_NAFTAReconIndicator = value; }
		}

		public ZPropertyInfo ZO_NAFTAReconIndicatorInfo
		{
			get { return ImportAddInfo == null ? null : GetWrappedZPropertyInfo(USOrgImpAddInfoSchema.Constants.ZO_NAFTAReconIndicator, x => ImportAddInfo.ZO_NAFTAReconIndicatorInfo); }
		}

		#endregion

		#region Known importer Indicator

		public ZString ZO_KnwImpInd
		{
			get { return ImportAddInfo.ZO_KnwImpInd; }
			set { ImportAddInfo.ZO_KnwImpInd = value; }
		}

		public ZPropertyInfo ZO_KnwImpIndInfo
		{
			get { return ImportAddInfo == null ? null : GetWrappedZPropertyInfo(USOrgImpAddInfoSchema.Constants.ZO_KnwImpInd, x => ImportAddInfo.ZO_KnwImpIndInfo); }
		}

		#endregion

		#region Other Recon Indicator

		[List(nameof(ImportAddInfoLookups) + "." + nameof(USOrgImpAddInfoLookups.OtherReconIssueList))]
		public ZString ZO_OtherReconIndicator
		{
			get { return ImportAddInfo.ZO_OtherReconIndicator; }
			set { ImportAddInfo.ZO_OtherReconIndicator = value; }
		}

		public ZPropertyInfo ZO_OtherReconIndicatorInfo
		{
			get { return ImportAddInfo == null ? null : GetWrappedZPropertyInfo(USOrgImpAddInfoSchema.Constants.ZO_OtherReconIndicator, x => ImportAddInfo.ZO_OtherReconIndicatorInfo); }
		}

		#endregion

		#region ZO_IsEINNumberVerifiedIndicator

		public ZString ZO_IsEINNumberVerifiedIndicator
		{
			get { return ImportAddInfo.ZO_IsEINNumberVerifiedIndicator; }
			set { ImportAddInfo.ZO_IsEINNumberVerifiedIndicator = value; }
		}

		public ZPropertyInfo ZO_IsEINNumberVerifiedIndicatorInfo
		{
			get { return ImportAddInfo == null ? null : GetWrappedZPropertyInfo(USOrgImpAddInfoSchema.Constants.ZO_IsEINNumberVerifiedIndicator, x => ImportAddInfo.ZO_IsEINNumberVerifiedIndicatorInfo); }
		}

		#endregion

		#region Account No

		public ZString ZO_AccountNo
		{
			get { return ImportAddInfo.ZO_AccountNo; }
			set
			{
				ImportAddInfo.ZO_AccountNo = value;

				DefaultBrokerToPayIndicator();
			}
		}

		void DefaultBrokerToPayIndicator()
		{
			ZO_BrokerToPay = new PaymentDetailsDefaulter().GetDefaultBrokerToPayIndicator(ZO_AccountNo, ZO_PaymentType, GlbCompany.CurrentCompany.PK.ToGuid());
		}

		public ZPropertyInfo ZO_AccountNoInfo
		{
			get { return ImportAddInfo == null ? null : GetWrappedZPropertyInfo(USOrgImpAddInfoSchema.Constants.ZO_AccountNo, x => ImportAddInfo.ZO_AccountNoInfo); }
		}

		#endregion

		#region ZO_Purchased

		[List(nameof(ImportAddInfoLookups) + "." + nameof(USOrgImpAddInfoLookups.ZO_YesNoList))]
		public ZString ZO_Purchased
		{
			get { return ImportAddInfo.ZO_Purchased; }
			set { ImportAddInfo.ZO_Purchased = value; }
		}

		public ZPropertyInfo ZO_PurchasedInfo
		{
			get { return ImportAddInfo == null ? null : GetWrappedZPropertyInfo(USOrgImpAddInfoSchema.Constants.ZO_Purchased, x => ImportAddInfo.ZO_PurchasedInfo); }
		}

		#endregion

		#region Contact Phone No

		public ZString ContactPhoneNoForFDA
		{
			get
			{
				ZString phoneForFDA = ((IPGAContactDetails)this).PhoneNumber.KeepNumericCharacters();
				if (phoneForFDA.IsEmpty)
				{
					phoneForFDA = ((IAddressDetails)orgAddress).Phone.KeepNumericCharacters();
					if (((IAddressDetails)orgAddress).Country == Core.Constants.CountryCodes.UnitedStates)
					{
						if (phoneForFDA.Length == 11 && phoneForFDA[0] == '1')
						{
							phoneForFDA = phoneForFDA.SubstringSafe(1);
						}
					}
				}

				return phoneForFDA;
			}
		}

		#endregion

		#region Contact Fax

		public ZString ContactFaxForFDA
		{
			get
			{
				ZString faxForFDA = ((IPGAContactDetails)this).Fax.KeepNumericCharacters();
				if (faxForFDA.IsEmpty)
				{
					faxForFDA = ((IAddressDetails)orgAddress).Fax.KeepNumericCharacters();
					if (((IAddressDetails)orgAddress).Country == Core.Constants.CountryCodes.UnitedStates)
					{
						if (faxForFDA.Length == 11 && faxForFDA[0] == '1')
						{
							faxForFDA = faxForFDA.SubstringSafe(1);
						}
					}
				}

				if (faxForFDA.IsEmpty)
				{
					faxForFDA = new ZString('0', 10);
				}

				return faxForFDA;
			}
		}

		#endregion

		#region Contact email

		public ZString ContactEmailForFDA
		{
			get
			{
				ZString emailForFDA = ((IPGAContactDetails)this).EmailAddress;
				if (emailForFDA.IsEmpty)
				{
					emailForFDA = ((IAddressDetails)orgAddress).Email;
				}

				if (emailForFDA.IsEmpty)
				{
					emailForFDA = "NONE";
				}

				return emailForFDA;
			}
		}

		#endregion

		#region MFR Reg Exemption

		[List(nameof(ImportAddInfoLookups) + "." + nameof(USOrgImpAddInfoLookups.FDAPriorNoticeExemptCodeList))]
		public ZString ZO_MFRRegExempt
		{
			get { return ImportAddInfo.ZO_MFRRegExempt; }
			set { ImportAddInfo.ZO_MFRRegExempt = value; }
		}

		public ZPropertyInfo ZO_MFRRegExemptInfo
		{
			get { return ImportAddInfo == null ? null : GetWrappedZPropertyInfo(USOrgImpAddInfoSchema.Constants.ZO_MFRRegExempt, x => ImportAddInfo.ZO_MFRRegExemptInfo); }
		}

		#endregion

		#region Payment Type

		[List(nameof(ImportAddInfoLookups) + "." + nameof(USOrgImpAddInfoLookups.PaymentTypes))]
		public ZString ZO_PaymentType
		{
			get { return ImportAddInfo.ZO_PaymentType; }
			set
			{
				ImportAddInfo.ZO_PaymentType = value;
				DefaultBrokerToPayIndicator();
			}
		}

		public ZPropertyInfo ZO_PaymentTypeInfo
		{
			get { return ImportAddInfo == null ? null : GetWrappedZPropertyInfo(USOrgImpAddInfoSchema.Constants.ZO_PaymentType, x => ImportAddInfo.ZO_PaymentTypeInfo); }
		}

		#endregion

		#region Broker To Pay

		[List(nameof(ImportAddInfoLookups) + "." + nameof(USOrgImpAddInfoLookups.ZO_YesNoList))]
		public ZString ZO_BrokerToPay
		{
			get { return ImportAddInfo.ZO_BrokerToPay; }
			set { ImportAddInfo.ZO_BrokerToPay = value; }
		}

		public ZPropertyInfo ZO_BrokerToPayInfo
		{
			get { return ImportAddInfo == null ? null : GetWrappedZPropertyInfo(USOrgImpAddInfoSchema.Constants.ZO_BrokerToPay, x => ImportAddInfo.ZO_BrokerToPayInfo); }
		}

		#endregion

		#region Tax Deferred Ind

		[List(nameof(ImportAddInfoLookups) + "." + nameof(USOrgImpAddInfoLookups.TaxDeferredIndicators))]
		public ZString ZO_TaxDeferredInd
		{
			get { return ImportAddInfo.ZO_TaxDeferredInd; }
			set { ImportAddInfo.ZO_TaxDeferredInd = value; }
		}

		public ZPropertyInfo ZO_TaxDeferredIndInfo
		{
			get { return ImportAddInfo == null ? null : GetWrappedZPropertyInfo(USOrgImpAddInfoSchema.Constants.ZO_TaxDeferredInd, x => ImportAddInfo.ZO_TaxDeferredIndInfo); }
		}

		#endregion

		#region Deferred Tax Due Date Calculation

		[List(nameof(ImportAddInfoLookups) + "." + nameof(USOrgImpAddInfoLookups.DefTaxDueDateCalculationOptionList))]
		public ZString ZO_DefTaxDateCalcOption
		{
			get { return ImportAddInfo.ZO_DefTaxDateCalcOption; }
			set { ImportAddInfo.ZO_DefTaxDateCalcOption = value; }
		}

		public ZPropertyInfo ZO_DefTaxDateCalcOptionInfo
		{
			get { return ImportAddInfo == null ? null : GetWrappedZPropertyInfo(USOrgImpAddInfoSchema.Constants.ZO_DefTaxDateCalcOption, x => ImportAddInfo.ZO_DefTaxDateCalcOptionInfo); }
		}

		#endregion

		#region Do NOT auto generate SDCR

		public ZBool ZO_DoNotAutoGenerateSDCR
		{
			get { return ImportAddInfo.ZO_DoNotAutoGenerateSDCR; }
			set { ImportAddInfo.ZO_DoNotAutoGenerateSDCR = value; }
		}

		public ZPropertyInfo ZO_DoNotAutoGenerateSDCRInfo
		{
			get { return ImportAddInfo == null ? null : GetWrappedZPropertyInfo(USOrgImpAddInfoSchema.Constants.ZO_DoNotAutoGenerateSDCR, x => ImportAddInfo.ZO_DoNotAutoGenerateSDCRInfo); }
		}

		#endregion

		#region Do NOT convert SKU

		public ZBool ZO_DoNotConvertSKU
		{
			get { return ImportAddInfo.ZO_DoNotConvertSKU; }
			set { ImportAddInfo.ZO_DoNotConvertSKU = value; }
		}

		public ZPropertyInfo ZO_DoNotConvertSKUInfo
		{
			get { return ImportAddInfo == null ? null : GetWrappedZPropertyInfo(USOrgImpAddInfoSchema.Constants.ZO_DoNotConvertSKU, x => ImportAddInfo.ZO_DoNotConvertSKUInfo); }
		}

		#endregion

		#region Statement Print Date Number of Days

		public ZInt ZO_SPDNumberOfDays
		{
			get { return ImportAddInfo.ZO_SPDNumberOfDays; }
			set { ImportAddInfo.ZO_SPDNumberOfDays = value; }
		}

		public ZPropertyInfo ZO_SPDNumberOfDaysInfo
		{
			get { return ImportAddInfo == null ? null : GetWrappedZPropertyInfo(USOrgImpAddInfoSchema.Constants.ZO_SPDNumberOfDays, x => ImportAddInfo.ZO_SPDNumberOfDaysInfo); }
		}

		#endregion

		#region ZO_ENSPrintProduct

		public ZBool ZO_ENSPrintProduct
		{
			get { return ImportAddInfo.ZO_ENSPrintProduct; }
			set { ImportAddInfo.ZO_ENSPrintProduct = value; }
		}

		public ZPropertyInfo ZO_ENSPrintProductInfo
		{
			get { return ImportAddInfo == null ? null : GetWrappedZPropertyInfo(USOrgImpAddInfoSchema.Constants.ZO_ENSPrintProduct, x => ImportAddInfo.ZO_ENSPrintProductInfo); }
		}

		#endregion

		#region ZO_ENSPrintCustomAttrib1

		public ZBool ZO_ENSPrintCustomAttrib1
		{
			get { return ImportAddInfo.ZO_ENSPrintCustomAttrib1; }
			set { ImportAddInfo.ZO_ENSPrintCustomAttrib1 = value; }
		}

		public ZPropertyInfo ZO_ENSPrintCustomAttrib1Info
		{
			get { return ImportAddInfo == null ? null : GetWrappedZPropertyInfo(USOrgImpAddInfoSchema.Constants.ZO_ENSPrintCustomAttrib1, x => ImportAddInfo.ZO_ENSPrintCustomAttrib1Info); }
		}

		#endregion

		#region ZO_ENSPrintCustomAttrib2

		public ZBool ZO_ENSPrintCustomAttrib2
		{
			get { return ImportAddInfo.ZO_ENSPrintCustomAttrib2; }
			set { ImportAddInfo.ZO_ENSPrintCustomAttrib2 = value; }
		}

		public ZPropertyInfo ZO_ENSPrintCustomAttrib2Info
		{
			get { return ImportAddInfo == null ? null : GetWrappedZPropertyInfo(USOrgImpAddInfoSchema.Constants.ZO_ENSPrintCustomAttrib2, x => ImportAddInfo.ZO_ENSPrintCustomAttrib2Info); }
		}

		#endregion

		#region ZO_ENSPrintCustomAttrib3

		public ZBool ZO_ENSPrintCustomAttrib3
		{
			get { return ImportAddInfo.ZO_ENSPrintCustomAttrib3; }
			set { ImportAddInfo.ZO_ENSPrintCustomAttrib3 = value; }
		}

		public ZPropertyInfo ZO_ENSPrintCustomAttrib3Info
		{
			get { return ImportAddInfo == null ? null : GetWrappedZPropertyInfo(USOrgImpAddInfoSchema.Constants.ZO_ENSPrintCustomAttrib3, x => ImportAddInfo.ZO_ENSPrintCustomAttrib3Info); }
		}

		#endregion

		#region ZO_GB

		[List(nameof(ImportAddInfoLookups) + "." + nameof(USOrgImpAddInfoLookups.Branches))]
		public ZGuid ZO_GB
		{
			get { return ImportAddInfo.ZO_GB; }
			set { ImportAddInfo.ZO_GB = value; }
		}

		public ZPropertyInfo ZO_GBInfo
		{
			get { return ImportAddInfo == null ? null : GetWrappedZPropertyInfo(USOrgImpAddInfoSchema.Constants.ZO_GB, x => ImportAddInfo.ZO_GBInfo); }
		}

		public GlbBranch BIRDDefaultBranch
		{
			get { return Factory.Load<GlbBranch>(ZO_GB); }
		}

		#endregion

		#region Import Source

		[List(nameof(ImportAddInfoLookups) + "." + nameof(USOrgImpAddInfoLookups.ZO_ImportSourceList))]
		public ZString ZO_ImportSource
		{
			get { return ImportAddInfo.ZO_ImportSource; }
			set { ImportAddInfo.ZO_ImportSource = value; }
		}

		public ZPropertyInfo ZO_ImportSourceInfo
		{
			get { return ImportAddInfo == null ? null : GetWrappedZPropertyInfo(USOrgImpAddInfoSchema.Constants.ZO_ImportSource, x => ImportAddInfo.ZO_ImportSourceInfo); }
		}

		#endregion

		#region Recon Filing Port

		[List(nameof(ImportAddInfoLookups) + "." + nameof(USOrgImpAddInfoLookups.ReconPorts))]
		public ZString ZO_ReconFilingPort
		{
			get { return ImportAddInfo.ZO_ReconFilingPort; }
			set
			{
				ImportAddInfo.ZO_ReconFilingPort = value;
				ZO_ReconFilingPortInfo?.RefreshBinding();
			}
		}

		public ZPropertyInfo ZO_ReconFilingPortInfo
		{
			get { return ImportAddInfo == null ? null : GetWrappedZPropertyInfo(USOrgImpAddInfoSchema.Constants.ZO_ReconFilingPort, x => ImportAddInfo.ZO_ReconFilingPortInfo); }
		}

		#endregion

		#region Recon Payment Type

		[List(nameof(ImportAddInfoLookups) + "." + nameof(USOrgImpAddInfoLookups.ReconPaymentTypes))]
		public ZString ZO_ReconPaymentType
		{
			get { return ImportAddInfo.ZO_ReconPaymentType; }
			set
			{
				ImportAddInfo.ZO_ReconPaymentType = value;
				ZO_ReconBrokerToPay = new PaymentDetailsDefaulter().GetDefaultBrokerToPayIndicatorBasedOnPaymentType(ZO_ReconPaymentType);
			}
		}

		public ZPropertyInfo ZO_ReconPaymentTypeInfo
		{
			get { return ImportAddInfo == null ? null : GetWrappedZPropertyInfo(USOrgImpAddInfoSchema.Constants.ZO_ReconPaymentType, x => ImportAddInfo.ZO_ReconPaymentTypeInfo); }
		}

		#endregion

		#region Recon Broker To Pay

		[List(nameof(ImportAddInfoLookups) + "." + nameof(USOrgImpAddInfoLookups.ZO_YesNoList))]
		public ZString ZO_ReconBrokerToPay
		{
			get { return ImportAddInfo.ZO_ReconBrokerToPay; }
			set { ImportAddInfo.ZO_ReconBrokerToPay = value; }
		}

		public ZPropertyInfo ZO_ReconBrokerToPayInfo
		{
			get { return ImportAddInfo == null ? null : GetWrappedZPropertyInfo(USOrgImpAddInfoSchema.Constants.ZO_ReconBrokerToPay, x => ImportAddInfo.ZO_ReconBrokerToPayInfo); }
		}

		#endregion

		#region ZO_PayMethod

		[List(nameof(ImportAddInfoLookups) + "." + nameof(USOrgImpAddInfoLookups.PayMethodList))]
		public ZString ZO_PayMethod
		{
			get { return ImportAddInfo.ZO_PayMethod; }
			set { ImportAddInfo.ZO_PayMethod = value; }
		}

		public ZPropertyInfo ZO_PayMethodInfo
		{
			get { return ImportAddInfo == null ? null : GetWrappedZPropertyInfo(USOrgImpAddInfoSchema.Constants.ZO_PayMethod, x => ImportAddInfo.ZO_PayMethodInfo); }
		}

		#endregion

		#region ZO_FIRMS

		[RelatedBusinessObject(nameof(LocationOfGoods))]
		[List(nameof(ImportAddInfoLookups) + "." + nameof(USOrgImpAddInfoLookups.FIRMSList))]
		public ZString ZO_FIRMS
		{
			get { return ImportAddInfo.ZO_FIRMS; }
			set { ImportAddInfo.ZO_FIRMS = value; }
		}

		public ZPropertyInfo ZO_FIRMSInfo
		{
			get { return ImportAddInfo == null ? null : GetWrappedZPropertyInfo(USOrgImpAddInfoSchema.Constants.ZO_FIRMS, x => ImportAddInfo.ZO_FIRMSInfo); }
		}

		public ZZRefCusCodeListCombined LocationOfGoods
		{
			get { return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, ZO_FIRMS, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, ZDateTime.Today); }
		}

		#endregion

		#region ZO_ZoneID

		public ZString ZO_ZoneID
		{
			get { return ImportAddInfo.ZO_ZoneID; }
			set { ImportAddInfo.ZO_ZoneID = value; }
		}

		public ZPropertyInfo ZO_ZoneIDInfo
		{
			get { return ImportAddInfo == null ? null : GetWrappedZPropertyInfo(USOrgImpAddInfoSchema.Constants.ZO_ZoneID, x => ImportAddInfo.ZO_ZoneIDInfo); }
		}

		#endregion

		#region ZO_SubZone

		public ZString ZO_SubZone
		{
			get { return ImportAddInfo.ZO_SubZone; }
			set { ImportAddInfo.ZO_SubZone = value; }
		}

		public ZPropertyInfo ZO_SubZoneInfo
		{
			get { return ImportAddInfo == null ? null : GetWrappedZPropertyInfo(USOrgImpAddInfoSchema.Constants.ZO_SubZone, x => ImportAddInfo.ZO_SubZoneInfo); }
		}

		#endregion

		#region ZO_Site

		public ZString ZO_Site
		{
			get { return ImportAddInfo.ZO_Site; }
			set { ImportAddInfo.ZO_Site = value; }
		}

		public ZPropertyInfo ZO_SiteInfo
		{
			get { return ImportAddInfo == null ? null : GetWrappedZPropertyInfo(USOrgImpAddInfoSchema.Constants.ZO_Site, x => ImportAddInfo.ZO_SiteInfo); }
		}

		#endregion

		#region OV_OA_WarehouseAddress

		[RelatedBusinessObject("WarehouseAddress")]
		[List(nameof(ImportLookups) + "." + nameof(OrgCountryDataLookups.BondedWarehouseList))]
		public ZGuid OV_OA_WarehouseAddress
		{
			get { return CountryData.OV_OA_WarehouseAddress; }
			set { CountryData.OV_OA_WarehouseAddress = value; }
		}

		public OrgAddress WarehouseAddress
		{
			get { return CountryData.WarehouseAddress; }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress OV_OA_WarehouseAddress_ZAddress
		{
			get
			{
				if (fOV_OA_WarehouseAddress_ZAddress == null)
				{
					fOV_OA_WarehouseAddress_ZAddress = GetNewOV_OA_WarehouseAddress_ZAddress();
					fOV_OA_WarehouseAddress_ZAddress.IsOrgVisible = true;
				}
				return fOV_OA_WarehouseAddress_ZAddress;
			}
		}
		ZAddress fOV_OA_WarehouseAddress_ZAddress;

		protected ZAddress GetNewOV_OA_WarehouseAddress_ZAddress()
		{
			return new ZAddress(OV_OA_WarehouseAddressInfo);
		}

		public ZPropertyInfo OV_OA_WarehouseAddressInfo
		{
			get { return CountryData.OV_OA_WarehouseAddressInfo; }
		}

		#endregion

		#endregion

		#region New Properties

		public bool IsGovernmentImporter
		{
			get { return ImporterTypeList.IsGovernmentImporter(ZO_ImporterType); }
		}

		public ZString StandardCarrierAlphaCode
		{
			get { return GetCustomsCode(OrgCusCode.CodeTypes.CarrierCode); }
		}

		public ZString CBPAssignedNumber
		{
			get { return GetCustomsCode(OrgCusCode.USACodeTypes.CBPAssignedNumber); }
		}

		public ZString CTPAT
		{
			get { return GetCustomsCode(OrgCusCode.USACodeTypes.CTPAT); }
		}

		public ZString EmployerIdentificationNumber
		{
			get { return GetCustomsCode(OrgCusCode.USACodeTypes.EmployerIdentificationNumber); }
		}

		public ZString FAAIndirectCarrierNumber
		{
			get { return GetCustomsCode(OrgCusCode.USACodeTypes.FAAIndirectCarrierNumber); }
		}

		public ZString SocialSecurityNumber
		{
			get { return GetCustomsCode(OrgCusCode.USACodeTypes.SocialSecurityNumber); }
		}

		public ReconIssues GetReconIssueCalculated()
		{
			return ReconIssueCodeList.GetReconIssuesValue(ZO_OtherReconIndicator);
		}

		#endregion

		#region New Methods

		public static OrgAddress GetAddressForOrganisation(OrgHeader organisation)
		{
			OrgAddress result = null;
			if (organisation != null)
			{
				var customsAddress = organisation.AddressesActive.FirstOrDefault(x => x.IsCustomsAddress);
				result = customsAddress ?? organisation.MainAddress;
			}
			return result;
		}

		public static OrgAddress GetAddressForPSTCarrier(OrgHeader organisation)
		{
			OrgAddress result = null;
			if (organisation != null)
			{
				var usAddress = GetActiveAddressForSpecificCountry(organisation.AddressesActive, Core.Constants.CountryCodes.UnitedStates);
				var caAddress = GetActiveAddressForSpecificCountry(organisation.AddressesActive, Core.Constants.CountryCodes.Canada);
				var mxAddress = GetActiveAddressForSpecificCountry(organisation.AddressesActive, Core.Constants.CountryCodes.Mexico);
				result = (usAddress ?? (caAddress ?? mxAddress)) ?? organisation.MainAddress;
			}
			return result;
		}

		static OrgAddress GetActiveAddressForSpecificCountry(ActiveBusinessObjectCollection<OrgAddress> activeAddresses, ZString countryCode)
		{
			return activeAddresses.FirstOrDefault(x => x.OA_RL_NKRelatedPortCode.StartsWith(countryCode, StringComparison.CurrentCultureIgnoreCase) || (x.OA_RL_NKRelatedPortCode.IsEmpty && x.OA_RN_NKCountryCode.EqualsIgnoringCase(countryCode)));
		}

		public static OrgCusCode GetCustomsRelatedOrgCusCode(OrgHeader organisation, OrgMatchedCustomsRegNoType customsRegNoType)
		{
			OrgCusCode result = null;

			if (organisation != null)
			{
				if (customsRegNoType == OrgMatchedCustomsRegNoType.EIN)
				{
					result = GetOrgCusCodeObjectMatching(organisation, new ZString[] { OrgCusCode.USACodeTypes.EmployerIdentificationNumber, OrgCusCode.USACodeTypes.SocialSecurityNumber, OrgCusCode.USACodeTypes.CBPAssignedNumber });
				}
				else if (customsRegNoType == OrgMatchedCustomsRegNoType.ECN)
				{
					result = GetOrgCusCodeObjectMatching(organisation, new ZString[] { OrgCusCode.USACodeTypes.EmployerIdentificationNumber, OrgCusCode.USACodeTypes.SocialSecurityNumber, OrgCusCode.USACodeTypes.CBPAssignedNumber, OrgCusCode.USACodeTypes.EncryptedConsigneeNumber });
				}
				else if (customsRegNoType == OrgMatchedCustomsRegNoType.MID)
				{
					ErrorReporter.ReportOnce("MID is requested against organisation", "You should not use this method. Use a method that gets passed with OrgAddress.");
				}
			}

			return result;
		}

		public static ZString GetCustomsRelatedCode(OrgHeader organisation, OrgMatchedCustomsRegNoType customsRegNoType)
		{
			ZString result = ZString.Empty;

			if (organisation != null)
			{
				if (customsRegNoType == OrgMatchedCustomsRegNoType.EIN)
				{
					result = GetCustomsCode(organisation, new ZString[] { OrgCusCode.USACodeTypes.EmployerIdentificationNumber, OrgCusCode.USACodeTypes.SocialSecurityNumber, OrgCusCode.USACodeTypes.CBPAssignedNumber });
				}
				else if (customsRegNoType == OrgMatchedCustomsRegNoType.ECN)
				{
					result = GetCustomsCode(organisation, new ZString[] { OrgCusCode.USACodeTypes.EmployerIdentificationNumber, OrgCusCode.USACodeTypes.SocialSecurityNumber, OrgCusCode.USACodeTypes.CBPAssignedNumber, OrgCusCode.USACodeTypes.EncryptedConsigneeNumber });
				}
				else if (customsRegNoType == OrgMatchedCustomsRegNoType.MID)
				{
					ErrorReporter.ReportOnce("MID is requested against organisation", "You should not use this method. Use a method that gets passed with OrgAddress.");
				}
			}

			return result;
		}

		public static ZString GetCustomsCode(OrgHeader organisation, params ZString[] codeTypes)
		{
			return organisation == null ? ZString.Empty : organisation.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(Core.Constants.CountryCodes.UnitedStates, codeTypes);
		}

		public static OrgCusCode GetOrgCusCodeObjectMatching(OrgHeader organisation, params ZString[] codeTypes)
		{
			return organisation == null ? null : organisation.CustomsCodes.GetOrgCusCodeObjectMatchingCountryAndCodes(Core.Constants.CountryCodes.UnitedStates, codeTypes);
		}

		public ZString GetCustomsCode(params ZString[] codeTypes)
		{
			return GetCustomsCode(organisation, codeTypes);
		}

		public static ZString GetAddressCustomsRelatedCode(OrgAddress address, OrgMatchedCustomsRegNoType customsRegNoType)
		{
			var result = ZString.Empty;

			if (address != null)
			{
				if (customsRegNoType == OrgMatchedCustomsRegNoType.MID)
				{
					result = GetCustomsCodeFromAddress(address, OrgCusCode.USACodeTypes.ManufacturerID);
				}
				else
				{
					ErrorReporter.ReportOnce("Customs Codes other then MID should be requested against organisation", "You should not use this method. Use a method that gets passed with OrgHeader.");
				}
			}

			return result;
		}

		public static ZString GetCustomsCodeFromAddress(OrgAddress address, ZString codeType)
		{
			var result = ZString.Empty;
			var customsCodes = address?.CustomsCodes;
			if (customsCodes != null)
			{
				result = customsCodes.GetCustomsRegNo(codeType, Core.Constants.CountryCodes.UnitedStates);
			}
			return result;
		}

		#endregion

		#region Proxied Properties

		public ZString FullName
		{
			get { return organisation.OH_FullNameTruncated; }
		}

		public OrgAddress MainAddress
		{
			get { return organisation.MainAddress; }
		}

		public OrgAddress MailingAddress
		{
			get
			{
				if (mailingAddressCached == null)
				{
					mailingAddressCached = new CachedProperty<OrgAddress>(organisation.Factory, delegate
					{
						OrgAddress result = null;
						OrgAddressList addresses = organisation.Addresses.AddressesOfType(OrgAddressType.Postal);
						result = addresses.Count > 0 ? addresses[0] : null;
						if (result == null)
						{
							result = organisation.MainAddress;
						}
						return result;
					});
				}
				return mailingAddressCached.Value;
			}
		}
		CachedProperty<OrgAddress> mailingAddressCached;

		#endregion

		#region Collection

		public OrgHeaderWrapperMessageCollection Messages
		{
			get
			{
				if (fMessages == null)
				{
					fMessages = new OrgHeaderWrapperMessageCollection(organisation);
					fMessages.Load();
					RegisterEditableChildObject(fMessages);
				}
				return fMessages;
			}
		}
		OrgHeaderWrapperMessageCollection fMessages;

		public CusBondDetailCollection BondDetails
		{
			get
			{
				if (fBondDetails == null)
				{
					fBondDetails = new CusBondDetailCollection(organisation);
					fBondDetails.Load();
					organisation.RegisterEditableChildObject(fBondDetails);
				}
				return fBondDetails;
			}
		}
		CusBondDetailCollection fBondDetails;

		#endregion

		#region AddInfo

		public USOrgImpAddInfoLookups ImportAddInfoLookups
		{
			get { return ImportAddInfo.Lookups; }
		}

		internal OrgImpAddInfo ImportAddInfo
		{
			get
			{
				if (fImportAddInfo == null && CountryData != null)
				{
					fImportAddInfo = (OrgImpAddInfo)CountryData.ImpAddInfo;
					RegisterEditableChildObject(fImportAddInfo);
				}
				return fImportAddInfo;
			}
		}
		OrgImpAddInfo fImportAddInfo;

		#endregion

		#region CountryData

		public OrgCountryDataLookups ImportLookups
		{
			get { return CountryData.Lookups; }
		}

		internal OrgCountryData CountryData
		{
			get
			{
				if (fCountryData == null && organisation != null)
				{
					fCountryData = organisation.CountryData;
					if (fCountryData.OV_RN_NKClientCountryRelation != Core.Constants.CountryCodes.UnitedStates)
					{
						fCountryData = organisation.GetCountryData(Core.Constants.CountryCodes.UnitedStates);
					}
				}
				return fCountryData;
			}
		}
		OrgCountryData fCountryData;

		#endregion

		#region IControllerIDProvider Members

		ControllerID IControllerIDProvider.ControllerID
		{
			get { return ControllerIDs.Organisation; }
		}

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get { return organisation.PK.ToGuid(); }
		}

		#endregion

		#region IMessageAttachee Members

		ZString IMessageAttachee.MessageStatus
		{
			get { return ""; }
			set { }
		}

		CBPEDIMessageCollection IMessageAttachee.Messages
		{
			get { return Messages; }
		}

		BusinessObjectFactory IMessageAttachee.Factory
		{
			get { return Factory; }
		}

		GlbBranch IMessageAttachee.Branch
		{
			get { return null; }
		}

		BusinessObject IMessageAttachee.TopLevelBusinessObject
		{
			get { return organisation; }
		}

		string IMessageAttachee.TopLevelBizObjReferenceNumber
		{
			get { return organisation.OH_Code; }
		}

		Logs IMessageAttachee.TopLevelBusinessObjectLogs
		{
			get { return organisation.Logs; }
		}

		#endregion

		#region IOR Business Rules

		[ChildEditable(true)]
		public RestrictedCodeCollection RestrictedEntryTypes
		{
			get
			{
				if (fRestrictedEntryTypes == null)
				{
					fRestrictedEntryTypes = new RestrictedCodeCollection(CountryData, RestrictedCodeTypeList.Codes.RestrictedEntryType);
					fRestrictedEntryTypes.Load();
					RegisterEditableChildObject(fRestrictedEntryTypes);
				}
				return fRestrictedEntryTypes;
			}
		}
		RestrictedCodeCollection fRestrictedEntryTypes;

		[ChildEditable(true)]
		public RestrictedCodeCollection RestrictedSPIs
		{
			get
			{
				if (fRestrictedSPIs == null)
				{
					fRestrictedSPIs = new RestrictedCodeCollection(CountryData, RestrictedCodeTypeList.Codes.RestrictedSPI);
					fRestrictedSPIs.Load();
					RegisterEditableChildObject(fRestrictedSPIs);
				}
				return fRestrictedSPIs;
			}
		}
		RestrictedCodeCollection fRestrictedSPIs;

		[ChildEditable(true)]
		public RestrictedCodeCollection RestrictedTariffs
		{
			get
			{
				if (fRestrictedTariffs == null)
				{
					fRestrictedTariffs = new RestrictedCodeCollection(CountryData, RestrictedCodeTypeList.Codes.RestrictedTariff);
					fRestrictedTariffs.Load();
					RegisterEditableChildObject(fRestrictedTariffs);
				}
				return fRestrictedTariffs;
			}
		}
		RestrictedCodeCollection fRestrictedTariffs;

		[ChildEditable(true)]
		public RestrictedCodeCollection FTZAllowsFDAs
		{
			get
			{
				if (fFTZAllowsFDAs == null)
				{
					fFTZAllowsFDAs = new RestrictedCodeCollection(CountryData, RestrictedCodeTypeList.Codes.FTZAllowsFDA);
					fFTZAllowsFDAs.Load();
					RegisterEditableChildObject(fFTZAllowsFDAs);
				}
				return fFTZAllowsFDAs;
			}
		}
		RestrictedCodeCollection fFTZAllowsFDAs;

		public bool FTZAllowsFDAsContainZoneID(ZString zoneID)
		{
			if (FTZAllowsFDAs != null)
			{
				foreach (RestrictedCode fTZAllowsFDAData in FTZAllowsFDAs)
				{
					if (fTZAllowsFDAData.CY_Data == zoneID)
					{
						return true;
					}
				}
			}
			return false;
		}

		#endregion

		#region IPGAContactDetails

		ZString IPGAContactDetails.Name
		{
			get
			{
				var contactDetails = this.organisation.GetContactDetails(allocationTypes);
				return contactDetails != null ? contactDetails.Name : ZString.Empty;
			}
		}

		ZString IPGAContactDetails.PhoneNumber
		{
			get
			{
				var contactDetails = this.organisation.GetContactDetails(allocationTypes);
				return contactDetails != null ? contactDetails.PhoneNumber : ZString.Empty;
			}
		}

		ZString IPGAContactDetails.EmailAddress
		{
			get
			{
				var contactDetails = this.organisation.GetContactDetails(allocationTypes);
				return contactDetails != null ? contactDetails.EmailAddress : ZString.Empty;
			}
		}

		ZString IPGAContactDetails.Fax
		{
			get
			{
				var contactDetails = this.organisation.GetContactDetails(allocationTypes);
				return contactDetails != null ? contactDetails.Fax : ZString.Empty;
			}
		}

		IAddressDetails IPGAContactDetails.CompanyAddress
		{
			get { return new AddressWrapper(orgAddress); }
		}

		#endregion

		#region AddressWrapper
		class AddressWrapper : IAddressDetails
		{
			public AddressWrapper(OrgAddress address)
			{
				this.addressDetails = address;
				this.address = address;
			}

			readonly IAddressDetails addressDetails;
			readonly OrgAddress address;

			public ZString AddressLine1
			{
				get { return addressDetails.AddressLine1; }
			}

			public ZString AddressLine2
			{
				get { return addressDetails.AddressLine2; }
			}

			public ZString City
			{
				get { return addressDetails.City; }
			}

			public ZString CompanyName
			{
				get { return address.EffectiveCompanyName; }
			}

			public ZString ContactName
			{
				get { return addressDetails.ContactName; }
			}

			public ZString Country
			{
				get
				{
					var result = addressDetails.Country;
					return result == Core.Constants.CountryCodes.PuertoRico ? (ZString)Core.Constants.CountryCodes.UnitedStates : result;
				}
			}

			public ZString Email
			{
				get { return addressDetails.Email; }
			}

			public ZString Fax
			{
				get { return addressDetails.Fax; }
			}

			public ZString Phone
			{
				get { return addressDetails.Phone; }
			}

			public ZString PostCode
			{
				get { return addressDetails.PostCode; }
			}

			public ZString State
			{
				get
				{
					var result = ZString.Empty;
					var country = addressDetails.Country;
					var isPR = country == Core.Constants.CountryCodes.PuertoRico;
					if (isPR || OrganisationValidation.ShouldCheckStateCodeForPGA(country))
					{
						if (isPR)
						{
							result = Core.Constants.CountryCodes.PuertoRico;
						}
						else
						{
							result = addressDetails.State.ToString();
							if (!result.IsEmpty)
							{
								var relatedState = new RefCountryStates.Loader(address.Factory).LoadRefCountryStatesFromCode(result, Country);
								if (relatedState != null)
								{
									result = relatedState.GetCustomsCodeFor(RefCusMapTypeList.Codes.IMPSTA, Core.Constants.CountryCodes.UnitedStates);
								}
							}
						}
					}
					return result;
				}
			}

			public override bool Equals(object obj)
			{
				var otherAddressWrapper = obj as AddressWrapper;
				if (otherAddressWrapper == null)
				{
					return false;
				}
				return this.addressDetails == otherAddressWrapper.addressDetails;
			}

			public override int GetHashCode()
			{
				return this.addressDetails.GetHashCode();
			}
		}

		#endregion

		#region CBMA Collections

		[ChildEditable(true)]
		public ImportersControlledGroupNameCollection ImportersControlledGroupNames
		{
			get
			{
				if (fImportersControlledGroupNames == null)
				{
					fImportersControlledGroupNames = new ImportersControlledGroupNameCollection(CountryData);
					fImportersControlledGroupNames.Load();
					RegisterEditableChildObject(fImportersControlledGroupNames);
				}

				return fImportersControlledGroupNames;
			}
		}
		ImportersControlledGroupNameCollection fImportersControlledGroupNames;

		[ChildEditable(true)]
		public AllocationQuantityPerFPICollection AllocationQuantityPerFPIs
		{
			get
			{
				if (fAllocationQuantityPerFPIs == null)
				{
					fAllocationQuantityPerFPIs = new AllocationQuantityPerFPICollection(CountryData);
					fAllocationQuantityPerFPIs.Load();
					RegisterEditableChildObject(fAllocationQuantityPerFPIs);
				}

				return fAllocationQuantityPerFPIs;
			}
		}
		AllocationQuantityPerFPICollection fAllocationQuantityPerFPIs;

		#endregion

		IUSOrgAddress IUSIORWrapper.MainAddress
		{
			get
			{
				return MainAddress;
			}
		}
	}
}
