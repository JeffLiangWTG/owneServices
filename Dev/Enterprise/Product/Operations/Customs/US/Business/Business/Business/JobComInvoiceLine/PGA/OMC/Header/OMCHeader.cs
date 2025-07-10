using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	[GlowInterfaceReference("")]
	public class OMCHeader : CusAddInfo<OMCHeaderAddInfo>, ICusAddInfoTypeSupporter, IPGADataCorrection, ICanDelete, IOMCHeader, ICusDispositionParent
	{
		public OMCHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : CusAddInfo<OMCHeaderAddInfo>.Schema
		{
			public const string US_LineNo = USOMCAddInfoSchema.Constants.US_LineNo;
			public const string US_ElectronicImageSubmitted = USOMCAddInfoSchema.Constants.US_ElectronicImageSubmitted;
			public const string US_NetWeight = USOMCAddInfoSchema.Constants.US_NetWeight;
			public const string US_NetWeightUQ = USOMCAddInfoSchema.Constants.US_NetWeightUQ;
			public const string US_SourceCountry = USOMCAddInfoSchema.Constants.US_SourceCountry;
			public const string US_DepartureDate = USOMCAddInfoSchema.Constants.US_DepartureDate;
			public const string US_OA_Exporter = USOMCAddInfoSchema.Constants.US_OA_Exporter;
			public const string US_OA_ResponsibleGovernmentOfficial = USOMCAddInfoSchema.Constants.US_OA_ResponsibleGovernmentOfficial;
			public const string US_DeclarationCode = USOMCAddInfoSchema.Constants.US_DeclarationCode;
			public const string US_OA_AquacultureFacility = USOMCAddInfoSchema.Constants.US_OA_AquacultureFacility;
			public const string US_TrackingStatus = USOMCAddInfoSchema.Constants.US_TrackingStatus;
			public const string US_ExporterPGAContactEmail = USOMCAddInfoSchema.Constants.US_ExporterPGAContactEmail;
			public const string US_ExporterPGAContactName = USOMCAddInfoSchema.Constants.US_ExporterPGAContactName;
			public const string US_ExporterPGAContactPhoneNo = USOMCAddInfoSchema.Constants.US_ExporterPGAContactPhoneNo;
			public const string US_OfficialPGAContactEmail = USOMCAddInfoSchema.Constants.US_OfficialPGAContactEmail;
			public const string US_OfficialPGAContactName = USOMCAddInfoSchema.Constants.US_OfficialPGAContactName;
			public const string US_OfficialPGAContactPhoneNo = USOMCAddInfoSchema.Constants.US_OfficialPGAContactPhoneNo;
			public const string US_ExporterCertificationDate = USOMCAddInfoSchema.Constants.US_ExporterCertificationDate;
			public const string US_OfficialCertificationDate = USOMCAddInfoSchema.Constants.US_OfficialCertificationDate;
			public const string US_TrackingStatusDesc = "US_TrackingStatusDesc";
			public const string ExporterOrgPK = "ExporterOrgPK";
			public const string ResponsibleGovernmentOfficialPK = "ResponsibleGovernmentOfficialPK";
			public const string AquacultureFacilityPK = "AquacultureFacilityPK";
		}

		#endregion

		#region AddInfo Properties

		[ReadOnly(true)]
		public ZInt US_LineNo
		{
			get { return AddInfo.US_LineNo; }
			set
			{
				var oldValue = US_LineNo;
				if (oldValue != value)
				{
					try
					{
						suspendTrackingStatusChange = true;
						AddInfo.US_LineNo = value;
					}
					finally
					{
						suspendTrackingStatusChange = false;
					}
				}
			}
		}
		bool suspendTrackingStatusChange;

		public ZPropertyInfo US_LineNoInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_LineNo, x => AddInfo.US_LineNoInfo); }
		}

		[ReadOnly(true)]
		public ZString US_TrackingStatus
		{
			get { return AddInfo.US_TrackingStatus; }
			set { AddInfo.US_TrackingStatus = value; }
		}

		public ZPropertyInfo US_TrackingStatusInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_TrackingStatus, x => AddInfo.US_TrackingStatusInfo); }
		}

		public ZString US_TrackingStatusDesc
		{
			get { return Factory.GetCachedValue<PGATrackingStatusList>().GetDescriptionFromCode(US_TrackingStatus); }
		}

		public ZPropertyInfo US_TrackingStatusDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_TrackingStatusDesc); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USOMCAddInfoLookups.DeclarationCodes))]
		public ZString US_DeclarationCode
		{
			get { return AddInfo.US_DeclarationCode; }
			set
			{
				var oldValue = US_DeclarationCode;
				AddInfo.US_DeclarationCode = value;
				if (!IsCopying && oldValue != US_DeclarationCode)
				{
					US_ElectronicImageSubmitted = ConformanceDeclarationCodeList.IsDefaultElectronicImageSubmitted(US_DeclarationCode);

					if (!IsAquacultureFacilityRequired)
					{
						AquacultureFacilityPK = ZGuid.Empty;
						AquacultureFacilities.RemoveAndDeleteAll();
					}

					AquacultureFacilities.RefreshBinding();
					MarkAsNeedingValidationIncludingChildren();
				}
			}
		}

		public ZPropertyInfo US_DeclarationCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_DeclarationCode, x => AddInfo.US_DeclarationCodeInfo); }
		}

		public ZString US_ExporterPGAContactEmail
		{
			get { return AddInfo.US_ExporterPGAContactEmail; }
			set { AddInfo.US_ExporterPGAContactEmail = value; }
		}

		public ZPropertyInfo US_ExporterPGAContactEmailInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_ExporterPGAContactEmail, x => AddInfo.US_ExporterPGAContactEmailInfo); }
		}

		public ZString US_ExporterPGAContactName
		{
			get { return AddInfo.US_ExporterPGAContactName; }
			set { AddInfo.US_ExporterPGAContactName = value; }
		}

		public ZPropertyInfo US_ExporterPGAContactNameInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_ExporterPGAContactName, x => AddInfo.US_ExporterPGAContactNameInfo); }
		}

		public ZString US_ExporterPGAContactPhoneNo
		{
			get { return AddInfo.US_ExporterPGAContactPhoneNo; }
			set { AddInfo.US_ExporterPGAContactPhoneNo = value; }
		}

		public ZPropertyInfo US_ExporterPGAContactPhoneNoInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_ExporterPGAContactPhoneNo, x => AddInfo.US_ExporterPGAContactPhoneNoInfo); }
		}

		public ZString US_OfficialPGAContactEmail
		{
			get { return AddInfo.US_OfficialPGAContactEmail; }
			set { AddInfo.US_OfficialPGAContactEmail = value; }
		}

		public ZPropertyInfo US_OfficialPGAContactEmailInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_OfficialPGAContactEmail, x => AddInfo.US_OfficialPGAContactEmailInfo); }
		}

		public ZString US_OfficialPGAContactName
		{
			get { return AddInfo.US_OfficialPGAContactName; }
			set { AddInfo.US_OfficialPGAContactName = value; }
		}

		public ZPropertyInfo US_OfficialPGAContactNameInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_OfficialPGAContactName, x => AddInfo.US_OfficialPGAContactNameInfo); }
		}

		public ZString US_OfficialPGAContactPhoneNo
		{
			get { return AddInfo.US_OfficialPGAContactPhoneNo; }
			set { AddInfo.US_OfficialPGAContactPhoneNo = value; }
		}

		public ZPropertyInfo US_OfficialPGAContactPhoneNoInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_OfficialPGAContactPhoneNo, x => AddInfo.US_OfficialPGAContactPhoneNoInfo); }
		}

		public ZDateTime US_DepartureDate
		{
			get { return AddInfo.US_DepartureDate; }
			set { AddInfo.US_DepartureDate = value; }
		}

		public ZPropertyInfo US_DepartureDateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_DepartureDate, x => AddInfo.US_DepartureDateInfo); }
		}

		public ZDateTime US_ExporterCertificationDate
		{
			get { return AddInfo.US_ExporterCertificationDate; }
			set { AddInfo.US_ExporterCertificationDate = value; }
		}

		public ZPropertyInfo US_ExporterCertificationDateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_ExporterCertificationDate, x => AddInfo.US_ExporterCertificationDateInfo); }
		}

		public ZDateTime US_OfficialCertificationDate
		{
			get { return AddInfo.US_OfficialCertificationDate; }
			set { AddInfo.US_OfficialCertificationDate = value; }
		}

		public ZPropertyInfo US_OfficialCertificationDateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_OfficialCertificationDate, x => AddInfo.US_OfficialCertificationDateInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USOMCAddInfoLookups.USCountries))]
		public ZString US_SourceCountry
		{
			get { return AddInfo.US_SourceCountry; }
			set { AddInfo.US_SourceCountry = value; }
		}

		public ZPropertyInfo US_SourceCountryInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_SourceCountry, x => AddInfo.US_SourceCountryInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USOMCAddInfoLookups.UnitOfMeasureList))]
		public ZString US_NetWeightUQ
		{
			get { return AddInfo.US_NetWeightUQ; }
			set { AddInfo.US_NetWeightUQ = value; }
		}

		public ZPropertyInfo US_NetWeightUQInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NetWeightUQ, x => AddInfo.US_NetWeightUQInfo); }
		}

		[MeasureUnit(Schema.US_NetWeightUQ, MeasureUnitType.Weight)]
		public ZDecimal US_NetWeight
		{
			get { return AddInfo.US_NetWeight; }
			set { AddInfo.US_NetWeight = value; }
		}

		public ZPropertyInfo US_NetWeightInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NetWeight, x => AddInfo.US_NetWeightInfo); }
		}

		public ZBool US_ElectronicImageSubmitted
		{
			get { return AddInfo.US_ElectronicImageSubmitted; }
			set { AddInfo.US_ElectronicImageSubmitted = value; }
		}

		public ZPropertyInfo US_ElectronicImageSubmittedInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_ElectronicImageSubmitted, x => AddInfo.US_ElectronicImageSubmittedInfo); }
		}

		#endregion

		#region US_OA_Exporter

		[List(nameof(US_OA_Exporter_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public ZGuid US_OA_Exporter
		{
			get { return AddInfo.US_OA_Exporter; }
			set
			{
				var oldvalue = US_OA_Exporter;
				AddInfo.US_OA_Exporter = value;
				if (oldvalue != US_OA_Exporter && !IsCopying)
				{
					var iexporterWrapper = ExporterWrapper as IPGAContactDetails;
					if (iexporterWrapper != null)
					{
						US_ExporterPGAContactEmail = iexporterWrapper.EmailAddress;
						US_ExporterPGAContactName = iexporterWrapper.Name;
						US_ExporterPGAContactPhoneNo = iexporterWrapper.PhoneNumber;
					}
				}
			}
		}

		public ZPropertyInfo US_OA_ExporterInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_OA_Exporter, x => AddInfo.US_OA_ExporterInfo); }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress US_OA_Exporter_ZAddress
		{
			get
			{
				if (exporterAddress_ZAddress == null)
				{
					exporterAddress_ZAddress = GetNewUS_ExporterAddress_ZAddress();
					exporterAddress_ZAddress.IsOrgVisible = true;
					exporterAddress_ZAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetMainAddress);
				}
				return exporterAddress_ZAddress;
			}
		}
		ZAddress exporterAddress_ZAddress;

		protected ZAddress GetNewUS_ExporterAddress_ZAddress()
		{
			return new ZAddress(US_OA_ExporterInfo);
		}

		public OrgAddress ExporterAddress
		{
			get { return Factory.Load<OrgAddress>(US_OA_Exporter); }
		}

		internal OrgHeaderWrapper ExporterWrapper
		{
			get
			{
				OrgHeaderWrapper result = null;

				if (ExporterAddress != null)
				{
					result = OrgHeaderWrapper.New(ExporterAddress);
				}
				return result;
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USOMCAddInfoLookups.Organizations))]
		public ZGuid ExporterOrgPK
		{
			get { return US_OA_Exporter_ZAddress.OrgPK; }
			set { US_OA_Exporter_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo ExporterOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ExporterOrgPK, x => US_OA_Exporter_ZAddress.OrgPKInfo); }
		}

		#endregion

		#region US_OA_ResponsibleGovernmentOfficial

		[List(nameof(US_OA_ResponsibleGovernmentOfficial_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public ZGuid US_OA_ResponsibleGovernmentOfficial
		{
			get { return AddInfo.US_OA_ResponsibleGovernmentOfficial; }
			set
			{
				var oldvalue = US_OA_ResponsibleGovernmentOfficial;
				AddInfo.US_OA_ResponsibleGovernmentOfficial = value;
				if (oldvalue != US_OA_ResponsibleGovernmentOfficial && !IsCopying)
				{
					var iresponsibleGovernmentOfficialWrapper = ResponsibleGovernmentOfficialWrapper as IPGAContactDetails;
					if (iresponsibleGovernmentOfficialWrapper != null)
					{
						US_OfficialPGAContactEmail = iresponsibleGovernmentOfficialWrapper.EmailAddress;
						US_OfficialPGAContactName = iresponsibleGovernmentOfficialWrapper.Name;
						US_OfficialPGAContactPhoneNo = iresponsibleGovernmentOfficialWrapper.PhoneNumber;
					}
				}
			}
		}

		public ZPropertyInfo US_OA_ResponsibleGovernmentOfficialInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_OA_ResponsibleGovernmentOfficial, x => AddInfo.US_OA_ResponsibleGovernmentOfficialInfo); }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress US_OA_ResponsibleGovernmentOfficial_ZAddress
		{
			get
			{
				if (responsibleGovernmentOfficialAddress_ZAddress == null)
				{
					responsibleGovernmentOfficialAddress_ZAddress = GetNewUS_ResponsibleGovernmentOfficialAddress_ZAddress();
					responsibleGovernmentOfficialAddress_ZAddress.IsOrgVisible = true;
					responsibleGovernmentOfficialAddress_ZAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetMainAddress);
				}
				return responsibleGovernmentOfficialAddress_ZAddress;
			}
		}
		ZAddress responsibleGovernmentOfficialAddress_ZAddress;

		protected ZAddress GetNewUS_ResponsibleGovernmentOfficialAddress_ZAddress()
		{
			return new ZAddress(US_OA_ResponsibleGovernmentOfficialInfo);
		}

		public OrgAddress ResponsibleGovernmentOfficialAddress
		{
			get { return Factory.Load<OrgAddress>(US_OA_ResponsibleGovernmentOfficial); }
		}

		internal OrgHeaderWrapper ResponsibleGovernmentOfficialWrapper
		{
			get
			{
				OrgHeaderWrapper result = null;

				if (ResponsibleGovernmentOfficialAddress != null)
				{
					result = OrgHeaderWrapper.New(ResponsibleGovernmentOfficialAddress);
				}
				return result;
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USOMCAddInfoLookups.Organizations))]
		public ZGuid ResponsibleGovernmentOfficialPK
		{
			get { return US_OA_ResponsibleGovernmentOfficial_ZAddress.OrgPK; }
			set { US_OA_ResponsibleGovernmentOfficial_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo ResponsibleGovernmentOfficialPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ResponsibleGovernmentOfficialPK, x => US_OA_ResponsibleGovernmentOfficial_ZAddress.OrgPKInfo); }
		}

		#endregion

		#region US_OA_AquacultureFacility

		[List(nameof(US_OA_AquacultureFacility_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		[ReadOnlyMember(nameof(AquacultureFacilityPK_ReadOnly))]
		public ZGuid US_OA_AquacultureFacility
		{
			get { return AddInfo.US_OA_AquacultureFacility; }
			set { AddInfo.US_OA_AquacultureFacility = value; }
		}

		public ZPropertyInfo US_OA_AquacultureFacilityInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_OA_AquacultureFacility, x => AddInfo.US_OA_AquacultureFacilityInfo); }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress US_OA_AquacultureFacility_ZAddress
		{
			get
			{
				if (aquacultureFacility_ZAddress == null)
				{
					aquacultureFacility_ZAddress = GetNewUS_OA_AquacultureFacility_ZAddress();
					aquacultureFacility_ZAddress.IsOrgVisible = true;
					aquacultureFacility_ZAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetMainAddress);
				}
				return aquacultureFacility_ZAddress;
			}
		}
		ZAddress aquacultureFacility_ZAddress;

		protected ZAddress GetNewUS_OA_AquacultureFacility_ZAddress()
		{
			return new ZAddress(US_OA_AquacultureFacilityInfo);
		}

		public OrgAddress AquacultureFacilityAddress
		{
			get { return Factory.Load<OrgAddress>(US_OA_AquacultureFacility); }
		}

		internal OrgHeaderWrapper AquacultureFacilityWrapper
		{
			get
			{
				OrgHeaderWrapper result = null;

				if (AquacultureFacilityAddress != null)
				{
					result = OrgHeaderWrapper.New(AquacultureFacilityAddress);
				}
				return result;
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USOMCAddInfoLookups.Organizations))]
		[ReadOnlyMember(nameof(AquacultureFacilityPK_ReadOnly))]
		public ZGuid AquacultureFacilityPK
		{
			get { return US_OA_AquacultureFacility_ZAddress.OrgPK; }
			set { US_OA_AquacultureFacility_ZAddress.OrgPK = value; }
		}

		public bool AquacultureFacilityPK_ReadOnly
		{
			get { return !IsAquacultureFacilityRequired; }
		}

		public ZPropertyInfo AquacultureFacilityPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.AquacultureFacilityPK, x => US_OA_AquacultureFacility_ZAddress.OrgPKInfo); }
		}

		#endregion

		#region Related

		public JobComInvoiceLine InvoiceLine
		{
			get { return Factory.Load<JobComInvoiceLine>(B7_ParentID); }
		}

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(AutoCusAddInfo.Schema.TableName, AutoCusAddInfo.Schema.B7_ParentID)]
		public USOMCAquacultureFacilityCollection AquacultureFacilities
		{
			get
			{
				if (fAquacultureFacilities == null)
				{
					fAquacultureFacilities = new USOMCAquacultureFacilityCollection(this);
					fAquacultureFacilities.Load();
					RegisterEditableChildObject(fAquacultureFacilities);
				}
				return fAquacultureFacilities;
			}
		}
		USOMCAquacultureFacilityCollection fAquacultureFacilities;

		#endregion

		#region AddInfo object/Validation and Lookups objects

		public USOMCAddInfoLookups AddInfoLookups
		{
			get { return AddInfo.Lookups; }
		}

		public USOMCAddInfoValidation AddInfoValidation
		{
			get { return AddInfo.Validation; }
		}

		OMCHeaderAddInfo AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new OMCHeaderAddInfo(B7_AddInfoDataInfo);
					RegisterEditableChildObject(fAddInfo);
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}
		OMCHeaderAddInfo fAddInfo;

		public bool IsAquacultureFacilityRequired
		{
			get { return ConformanceDeclarationCodeList.IsAquacultureFacilityRequired(US_DeclarationCode); }
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			PGADataCorrection.RegisterTrackerIfNeeded();
			SetReadOnlyIncludingChildren(PGADataCorrection.IsPGALineReadOnly());
		}

		public void UpdateAddInfoProperties()
		{
			updateAddInfoProperties();
		}

		protected void updateAddInfoProperties()
		{
			if (AddInfo != null && HasChanges)
			{
				AddInfo.UpdateRelatedPropertyInfo();
			}
		}

		#endregion

		#region Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			US_NetWeightUQ = ABIUnitOfMeasureList.Codes.Kilograms;
		}

		protected override ZString HumanReadableNameCore
		{
			get { return "OMC"; }
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			updateAddInfoProperties();

			var result = (OMCHeader)base.CloneInternal(args);
			result.CloneChildren(this, args);

			return result;
		}

		internal void CloneChildren(OMCHeader previousLine, BusinessObjectCloneArgs args = null)
		{
			foreach (USOMCAquacultureFacility aquacultureFacility in previousLine.AquacultureFacilities)
			{
				AquacultureFacilities.Add((USOMCAquacultureFacility)aquacultureFacility.Clone(new BusinessObjectCloneArgs(args?.AlternativeFactoryToInstantiateCloneIn ?? Factory, Array.Empty<string>(), typeof(USOMCAquacultureFacility), false)));
			}
		}

		public override void Delete()
		{
			PGADataCorrection.UnRegisterTrackerIfNeeded();
			base.Delete();
		}

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusAddInfoTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.USOMCDetails, typeof(USOMCAquacultureFacility));
			return result;
		}

		#endregion

		#region IPGADataCorrection

		IPGADataCorrection PGADataCorrection
		{
			get { return this; }
		}

		bool IPGADataCorrection.SettingStatusInProgress
		{
			get { return settingPGATrackingStatusInProgress; }
			set { settingPGATrackingStatusInProgress = value; }
		}
		bool settingPGATrackingStatusInProgress;

		bool IPGADataCorrection.SuspendTrackingStatusChange
		{
			get { return suspendTrackingStatusChange || AddInfo.IsSettingAddInfoPropertyInProgress; }
		}

		JobComInvoiceLine IPGADataCorrection.InvoiceLine
		{
			get { return InvoiceLine; }
		}

		string[] IPGADataCorrection.GetIndicatorFields()
		{
			return new[] { JobComInvoiceLine.Schema.US_OMCInd };
		}

		string[] IPGADataCorrection.GetDislaimReasonFields()
		{
			return new[] { JobComInvoiceLine.Schema.US_OMCDisclaimReason };
		}

		string[] IPGADataCorrection.GetRelatedInvoiceLineFields()
		{
			return Array.Empty<string>();
		}

		string[] IPGADataCorrection.GetRelatedInvoiceFields()
		{
			return Array.Empty<string>();
		}

		string[] IPGADataCorrection.GetRelatedContainerFields()
		{
			return Array.Empty<string>();
		}

		string[] IPGADataCorrection.GetRelatedDeclarationFields()
		{
			return Array.Empty<string>();
		}

		ZPropertyInfo IPGADataCorrection.TrackingStatusInfo
		{
			get { return US_TrackingStatusInfo; }
		}

		#endregion

		#region ICanDelete

		bool ICanDelete.CanDelete
		{
			get { return PGADataCorrection.PGALinesCanBeDeleted(); }
		}

		MultilingualString ICanDelete.ReasonForNotAbleToDelete
		{
			get { return PGADataChangeTracker.ReasonForNotAbleToDelete; }
		}

		#endregion

		#region IOMCHeader

		ZInt IOMCHeader.LineNo
		{
			get { return US_LineNo; }
			set { US_LineNo = value; }
		}

		ZBool IOMCHeader.ElectronicImageSubmitted
		{
			get { return US_ElectronicImageSubmitted; }
		}

		ZString IOMCHeader.SourceCountry
		{
			get { return US_SourceCountry; }
		}

		ZDateTime IOMCHeader.DepartureDate
		{
			get { return US_DepartureDate; }
		}

		IPGAContactDetails IOMCHeader.Exporter
		{
			get { return ExporterWrapper; }
		}

		IPGAContactDetails IOMCHeader.ResponsibleGovernmentOfficial
		{
			get { return ResponsibleGovernmentOfficialWrapper; }
		}

		IEnumerable<IPGAContactDetails> IOMCHeader.AquacultureFacilities
		{
			get
			{
				List<OrgHeaderWrapper> result = null;
				if (IsAquacultureFacilityRequired)
				{
					result = AquacultureFacilities.Cast<USOMCAquacultureFacility>().Select(x => x.AquacultureFacilityWrapper).ToList();
					if (AquacultureFacilityWrapper != null)
					{
						result.Insert(0, AquacultureFacilityWrapper);
					}
				}
				return result;
			}
		}

		ZDecimal IOMCHeader.NetWeight
		{
			get { return US_NetWeight; }
		}

		ZString IOMCHeader.NetWeightUQ
		{
			get { return US_NetWeightUQ; }
		}

		ZString IOMCHeader.ConformanceDeclaration
		{
			get { return US_DeclarationCode; }
		}

		ICustomsBrokerDetails IOMCHeader.ExporterPGAContactInformation
		{
			get { return new CustomsBrokerDetailWrapper(((IPGAContactDetails)OrgHeaderWrapper.New(this.ExporterAddress))?.CompanyAddress, US_ExporterPGAContactName, US_ExporterPGAContactPhoneNo, US_ExporterPGAContactEmail); }
		}

		ICustomsBrokerDetails IOMCHeader.GovOfficialPGAContactInformation
		{
			get { return new CustomsBrokerDetailWrapper(((IPGAContactDetails)OrgHeaderWrapper.New(this.ResponsibleGovernmentOfficialAddress))?.CompanyAddress, US_OfficialPGAContactName, US_OfficialPGAContactPhoneNo, US_OfficialPGAContactEmail); }
		}

		ZDateTime IOMCHeader.ExporterCertificationDate
		{
			get { return US_ExporterCertificationDate; }
		}

		ZDateTime IOMCHeader.OfficialCertificationDate
		{
			get { return US_OfficialCertificationDate; }
		}

		#endregion

		#region IPGALineStatus

		ZString IPGALineStatus.PGALineStatusAgencyCode
		{
			get { return ACEGovernmentAgenciesCodeList.Codes.OMC; }
		}

		ZInt IPGALineStatus.PGALineNumber
		{
			get { return US_LineNo; }
		}

		CusDispositionCollection IPGALineStatus.PGALineCusDispositions
		{
			get
			{
				if (fCusDisposition == null)
				{
					fCusDisposition = new CusDispositionCollection(this);
					fCusDisposition.Load();
				}
				return fCusDisposition;
			}
		}
		CusDispositionCollection fCusDisposition;

		public ZString Status
		{
			get { return this.GetStatus(); }
		}

		public ZString StatusDesc
		{
			get { return ((ICusDispositionParent)this).GetStatusDescription(Status); }
		}

		public ZDateTime StatusDate
		{
			get { return this.GetStatusDate(); }
		}

		ZString ICusDispositionParent.Type
		{
			get { return CusDispositionTypeCodeList.Codes.USPGALineStatus; }
		}

		ZString ICusDispositionParent.ParentTableCode
		{
			get { return CusAddInfoSchema.Constants.Prefix; }
		}

		BusinessObject ICusDispositionParent.CollectionMaster
		{
			get { return this; }
		}

		ZString ICusDispositionParent.GetStatusDescription(ZString status)
		{
			return PGADispositionProviderExtensionMethods.GetDescriptionFromZZRefCusCodeList(Factory, status, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus);
		}

		#endregion

		#region FetchStrategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(OMCHeader businessObject)
				: base(businessObject)
			{
			}

			protected override void FetchForLoadChildEditableObjectsCore()
			{
				base.FetchForLoadChildEditableObjectsCore();
				Factory.AddFetchHint(typeof(CusDisposition), new ZQuery(CusDispositionSchema.CDI_Type, CusDispositionTypeCodeList.Codes.USPGALineStatus), new ZQuery(CusDispositionSchema.CDI_ParentID, BusinessObject.PK));
			}
		}

		#endregion
	}
}
