using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists;
using Enterprise.Customs.NZ.Business.MAFeBACCa.Interfaces;
using Enterprise.Customs.NZ.Business.MAFeBACCa.MessageBuilders;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa
{
	public partial class MAFMessagingBO : NonPersistentBusinessObject, IObsoleteValidation, Customs.Business.IAddInfoManager, IImportDeclaration, IGoodsShipment
	{
		#region Schema

		public static class Schema
		{
			public const string ZX_ProcessingOffice = "ZX_ProcessingOffice";
			public const string ZX_ConsignmentType = "ZX_ConsignmentType";
			public const string ZX_IsMAFAuditRequiredByCustoms = "ZX_IsMAFAuditRequiredByCustoms";
			public const string ZX_IsCustomsXRayRequired = "ZX_IsCustomsXRayRequired";
			public const string ZX_IsCustomsCashClient = "ZX_IsCustomsCashClient";
			public const string ZX_CargoType = "ZX_CargoType";
			public const string ZX_MeasurementUQ = "ZX_MeasurementUQ";
			public const string ZX_MeasurementValue = "ZX_MeasurementValue";
			public const string ZX_ConsignmentNumber = "ZX_ConsignmentNumber";
			public const string ZX_ReceiptNumber = "ZX_ReceiptNumber";
			public const string ZX_PaymentMethod = "ZX_PaymentMethod";
			public const string ZX_AccountHolder = "ZX_AccountHolder";
			public const string ZX_AccountNumber = "ZX_AccountNumber";
			public const string ZX_MsgTransportMode = "ZX_MsgTransportMode";

			public const string ZX_MessagingStatus = "ZX_MessagingStatus";
			public const string ZX_MessagingStatusDescription = "ZX_MessagingStatusDescription";

			public const string ZX_Comments = "ZX_Comments";

			public const string EffectiveProcessingOfficeDescription = "EffectiveProcessingOfficeDescription";
			public const string EffectiveConsignmentTypeDescription = "EffectiveConsignmentTypeDescription";
			public const string EffectiveCargoTypeDescription = "EffectiveCargoTypeDescription";
			public const string EffectiveIsMAFAuditRequiredByCustomsDescription = "EffectiveIsMAFAuditRequiredByCustomsDescription";
			public const string EffectiveIsCustomsXRayRequiredDescription = "EffectiveIsCustomsXRayRequiredDescription";
			public const string EffectiveIsCustomsCashClientDescription = "EffectiveIsCustomsCashClientDescription";
			public const string EffectiveMeasurementValue = "EffectiveMeasurementValue";
			public const string EffectiveMeasurementUQ = "EffectiveMeasurementUQ";

			public const string ZX_ImporterContactName = "ZX_ImporterContactName";
			public const string ZX_ImporterContactPhone = "ZX_ImporterContactPhone";
			public const string ZX_ImporterContactFax = "ZX_ImporterContactFax";
			public const string ZX_ImporterContactEmail = "ZX_ImporterContactEmail";
			public const string ZX_ExporterContactName = "ZX_ExporterContactName";
			public const string ZX_ExporterContactPhone = "ZX_ExporterContactPhone";
			public const string ZX_ExporterContactFax = "ZX_ExporterContactFax";
			public const string ZX_ExporterContactEmail = "ZX_ExporterContactEmail";

			public const string EffectiveImporterContactName = "EffectiveImporterContactName";
			public const string EffectiveImporterContactPhone = "EffectiveImporterContactPhone";
			public const string EffectiveImporterContactFax = "EffectiveImporterContactFax";
			public const string EffectiveImporterContactEmail = "EffectiveImporterContactEmail";
			public const string EffectiveExporterContactName = "EffectiveExporterContactName";
			public const string EffectiveExporterContactPhone = "EffectiveExporterContactPhone";
			public const string EffectiveExporterContactFax = "EffectiveExporterContactFax";
			public const string EffectiveExporterContactEmail = "EffectiveExporterContactEmail";
		}

		#endregion

		public MAFMessagingBO(IMAFPlugInSupport dataSource)
			: base(dataSource.Master.Factory)
		{
			PlugInSupport = dataSource;
			PlugInSupport.Master.RegisterEditableChildObject(this);
		}

		#region AddInfo Properties

		#region ZX_ProcessingOffice

		[List(nameof(Lookups) + "." + nameof(MAFMessagingBOLookups.MAFProcessingOffices))]
		public ZString ZX_ProcessingOffice
		{
			get { return AddInfo.ZN_MAF_ProcessingOffice; }
			set { AddInfo.ZN_MAF_ProcessingOffice = value; }
		}

		public ZPropertyInfo ZX_ProcessingOfficeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZX_ProcessingOffice, x => DummyAddInfoForWrappedZPropertyInfos.ZN_MAF_ProcessingOfficeInfo); }
		}

		public ZString EffectiveProcessingOffice
		{
			get
			{
				var list = Lookups.MAFProcessingOffices;
				return list.GetEnumValue(ZX_ProcessingOffice).HasValue ? ZX_ProcessingOffice : list.GetDefaultOfficeCodeFromUNLOCO(PlugInSupport.ProcessingOffice);
			}
		}

		public ZString EffectiveProcessingOfficeDescription
		{
			get { return Lookups.MAFProcessingOffices.GetDescriptionFromCode(EffectiveProcessingOffice); }
		}

		public ZPropertyInfo EffectiveProcessingOfficeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.EffectiveProcessingOfficeDescription); }
		}

		#endregion

		#region ZX_Comments

		public ZString ZX_Comments
		{
			get { return CommentsNoteManager.Value; }
			set
			{
				CommentsNoteManager.Value = value;
				ZX_CommentsInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo ZX_CommentsInfo
		{
			get { return GetZPropertyInfo(Schema.ZX_Comments); }
		}

		public int ZX_Comments_MaxLength
		{
			get { return PredefinedNoteTypes.Instance.CustomsQuarantineMessagingRemarks.TextOnlyMaxLength; }
		}

		ProxiedNotePropertyManager CommentsNoteManager
		{
			get { return commentsNoteManager ?? (commentsNoteManager = new ProxiedNotePropertyManager(PlugInSupport.Master, PredefinedNoteTypes.Instance.CustomsQuarantineMessagingRemarks)); }
		}
		ProxiedNotePropertyManager commentsNoteManager;

		#endregion

		#region ZX_ConsignmentType

		[List(nameof(Lookups) + "." + nameof(MAFMessagingBOLookups.ConsignmentTypes))]
		public ZString ZX_ConsignmentType
		{
			get { return AddInfo.ZN_MAF_ConsignmentType; }
			set { AddInfo.ZN_MAF_ConsignmentType = value; }
		}

		public ZPropertyInfo ZX_ConsignmentTypeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZX_ConsignmentType, x => DummyAddInfoForWrappedZPropertyInfos.ZN_MAF_ConsignmentTypeInfo); }
		}

		public ZString EffectiveConsignmentType
		{
			get
			{
				var list = Lookups.ConsignmentTypes;
				return list.GetEnumValue(ZX_ConsignmentType).HasValue ? ZX_ConsignmentType : PlugInSupport.ConsignmentType;
			}
		}

		public ZString EffectiveConsignmentTypeDescription
		{
			get { return Lookups.ConsignmentTypes.GetDescriptionFromCode(EffectiveConsignmentType); }
		}

		public ZPropertyInfo EffectiveConsignmentTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.EffectiveConsignmentTypeDescription); }
		}

		#endregion

		#region ZX_IsMAFAuditRequiredByCustoms

		[List(nameof(Lookups) + "." + nameof(MAFMessagingBOLookups.YesNoUnknowns))]
		public ZString ZX_IsMAFAuditRequiredByCustoms
		{
			get { return AddInfo.ZN_MAF_IsMAFAuditRequiredByCustoms; }
			set { AddInfo.ZN_MAF_IsMAFAuditRequiredByCustoms = value; }
		}

		public ZPropertyInfo ZX_IsMAFAuditRequiredByCustomsInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZX_IsMAFAuditRequiredByCustoms, x => DummyAddInfoForWrappedZPropertyInfos.ZN_MAF_IsMAFAuditRequiredByCustomsInfo); }
		}

		public ZString EffectiveIsMAFAuditRequiredByCustomsDescription
		{
			get
			{
				var result = Lookups.YesNoUnknowns.GetDescriptionFromCode(ZX_IsMAFAuditRequiredByCustoms);
				return string.IsNullOrEmpty(result) ? YesNoUnknownList.Descriptions.Unknown : result;
			}
		}

		public ZPropertyInfo EffectiveIsMAFAuditRequiredByCustomsDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.EffectiveIsMAFAuditRequiredByCustomsDescription); }
		}

		#endregion

		#region ZX_IsCustomsXRayRequired

		[List(nameof(Lookups) + "." + nameof(MAFMessagingBOLookups.YesNoUnknowns))]
		public ZString ZX_IsCustomsXRayRequired
		{
			get { return AddInfo.ZN_MAF_IsCustomsXRayRequired; }
			set { AddInfo.ZN_MAF_IsCustomsXRayRequired = value; }
		}

		public ZPropertyInfo ZX_IsCustomsXRayRequiredInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZX_IsCustomsXRayRequired, x => DummyAddInfoForWrappedZPropertyInfos.ZN_MAF_IsCustomsXRayRequiredInfo); }
		}

		public ZString EffectiveIsCustomsXRayRequiredDescription
		{
			get
			{
				var result = Lookups.YesNoUnknowns.GetDescriptionFromCode(ZX_IsCustomsXRayRequired);
				return string.IsNullOrEmpty(result) ? YesNoUnknownList.Descriptions.Unknown : result;
			}
		}

		public ZPropertyInfo EffectiveIsCustomsXRayRequiredDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.EffectiveIsCustomsXRayRequiredDescription); }
		}

		#endregion

		#region ZX_IsCustomsCashClient

		[List(nameof(Lookups) + "." + nameof(MAFMessagingBOLookups.YesNoUnknowns))]
		public ZString ZX_IsCustomsCashClient
		{
			get { return AddInfo.ZN_MAF_IsCustomsCashClient; }
			set { AddInfo.ZN_MAF_IsCustomsCashClient = value; }
		}

		public ZPropertyInfo ZX_IsCustomsCashClientInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZX_IsCustomsCashClient, x => DummyAddInfoForWrappedZPropertyInfos.ZN_MAF_IsCustomsCashClientInfo); }
		}

		public ZString EffectiveIsCustomsCashClientDescription
		{
			get
			{
				var result = Lookups.YesNoUnknowns.GetDescriptionFromCode(ZX_IsCustomsCashClient);
				return string.IsNullOrEmpty(result) ? YesNoUnknownList.Descriptions.Unknown : result;
			}
		}

		public ZPropertyInfo EffectiveIsCustomsCashClientDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.EffectiveIsCustomsCashClientDescription); }
		}

		#endregion

		#region ZX_CargoType

		[List(nameof(Lookups) + "." + nameof(MAFMessagingBOLookups.CargoTypes))]
		public ZString ZX_CargoType
		{
			get { return AddInfo.ZN_MAF_CargoType; }
			set { AddInfo.ZN_MAF_CargoType = value; }
		}

		public ZPropertyInfo ZX_CargoTypeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZX_CargoType, x => DummyAddInfoForWrappedZPropertyInfos.ZN_MAF_CargoTypeInfo); }
		}

		public ZString EffectiveCargoType
		{
			get
			{
				var list = Lookups.CargoTypes;
				return list.GetEnumValue(ZX_CargoType).HasValue ? ZX_CargoType : PlugInSupport.CargoType;
			}
		}

		public ZString EffectiveCargoTypeDescription
		{
			get { return Lookups.CargoTypes.GetDescriptionFromCode(EffectiveCargoType); }
		}

		public ZPropertyInfo EffectiveCargoTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.EffectiveCargoTypeDescription); }
		}

		#endregion

		#region ZX_MeasurementUQ

		[List(nameof(Lookups) + "." + nameof(MAFMessagingBOLookups.MeasurementUQs))]
		public ZString ZX_MeasurementUQ
		{
			get { return AddInfo.ZN_MAF_MeasurementUQ; }
			set { AddInfo.ZN_MAF_MeasurementUQ = value; }
		}

		public ZPropertyInfo ZX_MeasurementUQInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZX_MeasurementUQ, x => DummyAddInfoForWrappedZPropertyInfos.ZN_MAF_MeasurementUQInfo); }
		}

		[List(nameof(Lookups) + "." + nameof(MAFMessagingBOLookups.MeasurementUQs))]
		public ZString EffectiveMeasurementUQ
		{
			get { return IsMeasurementValid ? ZX_MeasurementUQ : IsFallbackMeasurementValid ? PlugInSupport.MeasurementUQ : ZString.Empty; }
		}

		public ZPropertyInfo EffectiveMeasurementUQInfo
		{
			get { return GetZPropertyInfo(Schema.EffectiveMeasurementUQ); }
		}

		bool IsMeasurementValid
		{
			get { return ZX_MeasurementValue > 0 && Lookups.MeasurementUQs.ContainsCode(ZX_MeasurementUQ); }
		}

		bool IsFallbackMeasurementValid
		{
			get { return PlugInSupport.MeasurementValue > 0 && Lookups.MeasurementUQs.ContainsCode(PlugInSupport.MeasurementUQ); }
		}

		#endregion

		#region ZX_MeasurementValue

		public ZInt ZX_MeasurementValue
		{
			get { return AddInfo.ZN_MAF_MeasurementValue; }
			set { AddInfo.ZN_MAF_MeasurementValue = value; }
		}

		public ZPropertyInfo ZX_MeasurementValueInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZX_MeasurementValue, x => DummyAddInfoForWrappedZPropertyInfos.ZN_MAF_MeasurementValueInfo); }
		}

		public ZInt EffectiveMeasurementValue
		{
			get { return IsMeasurementValid ? ZX_MeasurementValue : IsFallbackMeasurementValid ? PlugInSupport.MeasurementValue : ZInt.Zero; }
		}

		public ZPropertyInfo EffectiveMeasurementValueInfo
		{
			get { return GetZPropertyInfo(Schema.EffectiveMeasurementValue); }
		}

		#endregion

		#region ZX_ConsignmentNumber

		[ReadOnly(true)]
		public ZString ZX_ConsignmentNumber
		{
			get { return AddInfo.ZN_MAF_ConsignmentNumber; }
			set
			{
				AddInfo.ZN_MAF_ConsignmentNumber = value;
				ZX_ConsignmentNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ZX_ConsignmentNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZX_ConsignmentNumber, x => DummyAddInfoForWrappedZPropertyInfos.ZN_MAF_ConsignmentNumberInfo); }
		}

		#endregion

		#region ZX_ReceiptNumber

		[ReadOnly(true)]
		public ZString ZX_ReceiptNumber
		{
			get { return AddInfo.ZN_MAF_ReceiptNumber; }
			set
			{
				AddInfo.ZN_MAF_ReceiptNumber = value;
				ZX_ReceiptNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ZX_ReceiptNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZX_ReceiptNumber, x => DummyAddInfoForWrappedZPropertyInfos.ZN_MAF_ReceiptNumberInfo); }
		}

		#endregion

		#region ZX_PaymentMethod

		[List(nameof(Lookups) + "." + nameof(MAFMessagingBOLookups.MAFPaymentMethods))]
		public ZString ZX_PaymentMethod
		{
			get { return AddInfo.ZN_MAF_PaymentMethod; }
			set
			{
				AddInfo.ZN_MAF_PaymentMethod = value;

				if (value != MAFPaymentMethodList.Codes.Account)
				{
					ZX_AccountNumber = ZString.Empty;
					ZX_AccountHolder = ZString.Empty;
				}
			}
		}

		public ZPropertyInfo ZX_PaymentMethodInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZX_PaymentMethod, x => DummyAddInfoForWrappedZPropertyInfos.ZN_MAF_PaymentMethodInfo); }
		}

		#endregion

		#region ZX_AccountHolder

		public ZString ZX_AccountHolder
		{
			get { return AddInfo.ZN_MAF_AccountHolder; }
			set
			{
				AddInfo.ZN_MAF_AccountHolder = value;

				if (!value.IsEmpty)
				{
					ZX_PaymentMethod = MAFPaymentMethodList.Codes.Account;
				}
			}
		}

		public ZPropertyInfo ZX_AccountHolderInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZX_AccountHolder, x => DummyAddInfoForWrappedZPropertyInfos.ZN_MAF_AccountHolderInfo); }
		}

		#endregion

		#region ZX_AccountNumber

		public ZString ZX_AccountNumber
		{
			get { return AddInfo.ZN_MAF_AccountNumber; }
			set
			{
				AddInfo.ZN_MAF_AccountNumber = value;

				if (!value.IsEmpty)
				{
					ZX_PaymentMethod = MAFPaymentMethodList.Codes.Account;
				}
			}
		}

		public ZPropertyInfo ZX_AccountNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZX_AccountNumber, x => DummyAddInfoForWrappedZPropertyInfos.ZN_MAF_AccountNumberInfo); }
		}

		#endregion

		#region ZX_MessagingStatus

		[List(nameof(Lookups) + "." + nameof(MAFMessagingBOLookups.MessagingStatuses))]
		public ZString ZX_MessagingStatus
		{
			get { return AddInfo.ZN_MAF_MessagingStatus; }
			set
			{
				AddInfo.ZN_MAF_MessagingStatus = value;
				ZX_MessagingStatusDescriptionInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ZX_MessagingStatusInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZX_MessagingStatus, x => DummyAddInfoForWrappedZPropertyInfos.ZN_MAF_MessagingStatusInfo); }
		}

		public ZString ZX_MessagingStatusDescription
		{
			get
			{
				ZString result = Lookups.TSW_IPIMessagingStatuses.GetDescriptionFromCode(ZX_MessagingStatus);
				if (result.IsEmpty)
				{
					result = Lookups.MessagingStatuses.GetDescriptionFromCode(ZX_MessagingStatus);  // kept for old eBACCa jobs display
				}

				return result;
			}
		}

		public ZPropertyInfo ZX_MessagingStatusDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.ZX_MessagingStatusDescription); }
		}

		#endregion

		#region ZX_ImporterContactName

		public ZString ZX_ImporterContactName
		{
			get { return AddInfo.ZN_MAF_ImpContactName; }
			set { AddInfo.ZN_MAF_ImpContactName = value; }
		}

		public ZPropertyInfo ZX_ImporterContactNameInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZX_ImporterContactName, x => DummyAddInfoForWrappedZPropertyInfos.ZN_MAF_ImpContactNameInfo); }
		}

		public ZString EffectiveImporterContactName
		{
			get
			{
				var importer = PlugInSupport.Importer;
				return !ZX_ImporterContactName.IsEmpty || IsImporterContactDetailsOverriden || importer == null
						? ZX_ImporterContactName : importer.ContactName;
			}
		}

		public ZPropertyInfo EffectiveImporterContactNameInfo
		{
			get { return GetZPropertyInfo(Schema.EffectiveImporterContactName); }
		}

		#endregion

		#region ZX_ImporterContactPhone

		public ZString ZX_ImporterContactPhone
		{
			get { return AddInfo.ZN_MAF_ImpContactPhone; }
			set { AddInfo.ZN_MAF_ImpContactPhone = value; }
		}

		public ZPropertyInfo ZX_ImporterContactPhoneInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZX_ImporterContactPhone, x => DummyAddInfoForWrappedZPropertyInfos.ZN_MAF_ImpContactPhoneInfo); }
		}

		public ZString EffectiveImporterContactPhone
		{
			get
			{
				var importer = PlugInSupport.Importer;
				return IsImporterContactDetailsOverriden || importer == null ? ZX_ImporterContactPhone
						: !ZX_ImporterContactName.IsEmpty ? importer.Phone : importer.ContactPhone;
			}
		}

		public ZPropertyInfo EffectiveImporterContactPhoneInfo
		{
			get { return GetZPropertyInfo(Schema.EffectiveImporterContactPhone); }
		}

		bool IsImporterContactDetailsOverriden
		{
			get { return !(ZX_ImporterContactPhone.IsEmpty && ZX_ImporterContactFax.IsEmpty && ZX_ImporterContactEmail.IsEmpty); }
		}

		#endregion

		#region ZX_ImporterContactFax

		public ZString ZX_ImporterContactFax
		{
			get { return AddInfo.ZN_MAF_ImpContactFax; }
			set { AddInfo.ZN_MAF_ImpContactFax = value; }
		}

		public ZPropertyInfo ZX_ImporterContactFaxInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZX_ImporterContactFax, x => DummyAddInfoForWrappedZPropertyInfos.ZN_MAF_ImpContactFaxInfo); }
		}

		public ZString EffectiveImporterContactFax
		{
			get
			{
				var importer = PlugInSupport.Importer;
				return IsImporterContactDetailsOverriden || importer == null ? ZX_ImporterContactFax
						: !ZX_ImporterContactName.IsEmpty ? importer.Fax : importer.ContactFax;
			}
		}

		public ZPropertyInfo EffectiveImporterContactFaxInfo
		{
			get { return GetZPropertyInfo(Schema.EffectiveImporterContactFax); }
		}

		#endregion

		#region ZX_ImporterContactEmail

		public ZString ZX_ImporterContactEmail
		{
			get { return AddInfo.ZN_MAF_ImpContactEmail; }
			set { AddInfo.ZN_MAF_ImpContactEmail = value; }
		}

		public ZPropertyInfo ZX_ImporterContactEmailInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZX_ImporterContactEmail, x => DummyAddInfoForWrappedZPropertyInfos.ZN_MAF_ImpContactEmailInfo); }
		}

		public ZString EffectiveImporterContactEmail
		{
			get
			{
				var importer = PlugInSupport.Importer;
				return IsImporterContactDetailsOverriden || importer == null ? ZX_ImporterContactEmail
						: !ZX_ImporterContactName.IsEmpty ? importer.Email : importer.ContactEmail;
			}
		}

		public ZPropertyInfo EffectiveImporterContactEmailInfo
		{
			get { return GetZPropertyInfo(Schema.EffectiveImporterContactEmail); }
		}

		#endregion

		#region ZX_ExporterContactName

		public ZString ZX_ExporterContactName
		{
			get { return AddInfo.ZN_MAF_ExpContactName; }
			set { AddInfo.ZN_MAF_ExpContactName = value; }
		}

		public ZPropertyInfo ZX_ExporterContactNameInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZX_ExporterContactName, x => DummyAddInfoForWrappedZPropertyInfos.ZN_MAF_ExpContactNameInfo); }
		}

		public ZString EffectiveExporterContactName
		{
			get
			{
				var exporter = PlugInSupport.Exporter;
				return !ZX_ExporterContactName.IsEmpty || IsExporterContactDetailsOverriden || exporter == null
						? ZX_ExporterContactName : exporter.ContactName;
			}
		}

		public ZPropertyInfo EffectiveExporterContactNameInfo
		{
			get { return GetZPropertyInfo(Schema.EffectiveExporterContactName); }
		}

		#endregion

		#region ZX_ExporterContactPhone

		public ZString ZX_ExporterContactPhone
		{
			get { return AddInfo.ZN_MAF_ExpContactPhone; }
			set { AddInfo.ZN_MAF_ExpContactPhone = value; }
		}

		public ZPropertyInfo ZX_ExporterContactPhoneInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZX_ExporterContactPhone, x => DummyAddInfoForWrappedZPropertyInfos.ZN_MAF_ExpContactPhoneInfo); }
		}

		public ZString EffectiveExporterContactPhone
		{
			get
			{
				var exporter = PlugInSupport.Exporter;
				return IsExporterContactDetailsOverriden || exporter == null ? ZX_ExporterContactPhone
						: !ZX_ExporterContactName.IsEmpty ? exporter.Phone : exporter.ContactPhone;
			}
		}

		public ZPropertyInfo EffectiveExporterContactPhoneInfo
		{
			get { return GetZPropertyInfo(Schema.EffectiveExporterContactPhone); }
		}

		bool IsExporterContactDetailsOverriden
		{
			get { return !(ZX_ExporterContactPhone.IsEmpty && ZX_ExporterContactFax.IsEmpty && ZX_ExporterContactEmail.IsEmpty); }
		}

		#endregion

		#region ZX_ExporterContactFax

		public ZString ZX_ExporterContactFax
		{
			get { return AddInfo.ZN_MAF_ExpContactFax; }
			set { AddInfo.ZN_MAF_ExpContactFax = value; }
		}

		public ZPropertyInfo ZX_ExporterContactFaxInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZX_ExporterContactFax, x => DummyAddInfoForWrappedZPropertyInfos.ZN_MAF_ExpContactFaxInfo); }
		}

		public ZString EffectiveExporterContactFax
		{
			get
			{
				var exporter = PlugInSupport.Exporter;
				return IsExporterContactDetailsOverriden || exporter == null ? ZX_ExporterContactFax
						: !ZX_ExporterContactName.IsEmpty ? exporter.Fax : exporter.ContactFax;
			}
		}

		public ZPropertyInfo EffectiveExporterContactFaxInfo
		{
			get { return GetZPropertyInfo(Schema.EffectiveExporterContactFax); }
		}

		#endregion

		#region ZX_ExporterContactEmail

		public ZString ZX_ExporterContactEmail
		{
			get { return AddInfo.ZN_MAF_ExpContactEmail; }
			set { AddInfo.ZN_MAF_ExpContactEmail = value; }
		}

		public ZPropertyInfo ZX_ExporterContactEmailInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZX_ExporterContactEmail, x => DummyAddInfoForWrappedZPropertyInfos.ZN_MAF_ExpContactEmailInfo); }
		}

		public ZString EffectiveExporterContactEmail
		{
			get
			{
				var exporter = PlugInSupport.Exporter;
				return IsExporterContactDetailsOverriden || exporter == null ? ZX_ExporterContactEmail
						: !ZX_ExporterContactName.IsEmpty ? exporter.Email : exporter.ContactEmail;
			}
		}

		public ZPropertyInfo EffectiveExporterContactEmailInfo
		{
			get { return GetZPropertyInfo(Schema.EffectiveExporterContactEmail); }
		}

		#endregion

		#region DummyAddInfoForWrappedZPropertyInfos

		NZAddInfo DummyAddInfoForWrappedZPropertyInfos
		{
			get { return AddInfo ?? (dummyAddInfoForStaticWrappedPropertyInfoCache ?? (dummyAddInfoForStaticWrappedPropertyInfoCache = new NZAddInfo(this))); }
		}

		NZAddInfo dummyAddInfoForStaticWrappedPropertyInfoCache;

		#endregion

		#endregion

		#region Payment Details

		public ZString PaymentTypeToBeSent
		{
			get
			{
				return !IsPaymentMethodOverriden && IsAccountDetailsSet(PlugInSupport.AccountDetails) ? (ZString)MAFPaymentMethodList.Codes.Account
						: ZX_PaymentMethod.IsEmpty ? (ZString)MAFPaymentMethodList.Codes.Cash : ZX_PaymentMethod;
			}
		}

		public ZString AccountNumberToBeSent
		{
			get { return IsPaymentMethodOverriden ? ZX_AccountNumber : PlugInSupport.AccountDetails.AccountNumber; }
		}

		public ZString AccountHolderNameToBeSent
		{
			get { return IsPaymentMethodOverriden ? ZX_AccountHolder : PlugInSupport.AccountDetails.AccountHolderName; }
		}

		public ZString PaymentDetailsSent
		{
			get
			{
				var result = new ZStringBuilder();
				string paymentTypeToBeSent = PaymentTypeToBeSent;
				result.Append(paymentTypeToBeSent + " - " + Lookups.MAFPaymentMethods.GetDescriptionFromCode(paymentTypeToBeSent));

				if (!IsPaymentMethodOverriden)
				{
					var accountDetails = PlugInSupport.AccountDetails;
					if (!IsAccountDetailsSet(accountDetails))
					{
						result.Append("NB: " + accountDetails.WhereToSetupAccountDetailsDescription);
					}
					else
					{
						result.Append(accountDetails.AccountNumber + " / " + accountDetails.AccountHolderName);
						result.Append("NB: Defaulted from " + accountDetails.AccountHolderDescription + ".");
					}
				}
				else if (paymentTypeToBeSent == MAFPaymentMethodList.Codes.Account)
				{
					if (!ZX_AccountNumber.IsEmpty && !ZX_AccountHolder.IsEmpty)
					{
						result.Append(ZX_AccountNumber + " / " + ZX_AccountHolder);
					}
				}
				return result.ToStringWithNewLineBetweenAppends();
			}
		}

		bool IsPaymentMethodOverriden
		{
			get
			{
				return (!ZX_PaymentMethod.IsEmpty && ZX_PaymentMethod != MAFPaymentMethodList.Codes.Account)
					   || !ZX_AccountNumber.IsEmpty || !ZX_AccountHolder.IsEmpty;
			}
		}

		bool IsAccountDetailsSet(IMAFAccountDetails details)
		{
			return !details.AccountHolderName.IsEmpty && !details.AccountNumber.IsEmpty;
		}

		#endregion

		#region Collections

		#region Lookups

		public MAFMessagingBOLookups Lookups
		{
			get { return lookups ?? (lookups = new MAFMessagingBOLookups(this)); }
		}

		MAFMessagingBOLookups lookups;

		#endregion

		#region eDocs

		internal AvailableEDocList AvailableEDocs => PlugInSupport.DocManagerInfo.MasterFactory.GetValue(ref availableEDocsCache, () => new AvailableEDocList(PlugInSupport.DocManagerInfo.AllEDocs));
		CachedProperty<AvailableEDocList> availableEDocsCache;

		#endregion

		#region Messages

		public NZMMessageCollection Messages
		{
			get
			{
				if (messages == null)
				{
					messages = new NZMMessageCollection(this);
					messages.Load();
					messages.Sort(EDIMessageSchema.EM_SystemCreateTimeUtc.Name);
					RegisterEditableChildObject(messages);
				}
				return messages;
			}
		}

		NZMMessageCollection messages;

		#endregion

		#region TSW Messages

		public NZCMessageCollection TSWMessages
		{
			get
			{
				if (tswMessages == null)
				{
					tswMessages = Consol != null ? new NZCMessageCollection(Consol) : new NZCMessageCollection(this);
					tswMessages.Load();
					tswMessages.Sort(EDIMessageSchema.EM_SystemCreateTimeUtc.Name);
					RegisterEditableChildObject(tswMessages);
				}
				return tswMessages;
			}
		}

		NZCMessageCollection tswMessages;

		#endregion

		#region Files

		[ChildEditable(true)]
		public CusAddInfoCollection<MAFFile> Files
		{
			get
			{
				if (files == null)
				{
					files = new NZMAFFileCollection(this);
					files.Load();
					RegisterEditableChildObject(files);
				}
				return files;
			}
		}
		CusAddInfoCollection<MAFFile> files;

		#endregion

		#endregion

		#region Implementation

		public void ResetToOriginal()
		{
			ClearStatusAndReferences();

			var lastMessage = (NZMMessage)Messages.GetLastMessage(NZMMessage.ApplicationCodes.NewZealandMAFeBACCa);
			if (lastMessage != null && lastMessage.IsTransmitMessage && lastMessage.EM_Status != NZMMessage.Status.Acknowledged)
			{
				lastMessage.EM_Status = NZMMessage.Status.Cancelled;
			}

			PlugInSupport.Logs.AddNew(Events.ResetEntryMessageItemFunction, "Reset eBACCa to Original");
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			if (!saveSucceeded)
			{
				if (PlugInSupport.Master.IsInDatabase)
				{
					ZX_MessagingStatus = (ZString)ZX_MessagingStatusInfo.OriginalValue;
				}
				else
				{
					ClearStatusAndReferences();
				}
			}
			base.OnFactorySaved(saveSucceeded);
		}

		void ClearStatusAndReferences()
		{
			ZX_MessagingStatus = ZString.Empty;
			ZX_ConsignmentNumber = ZString.Empty;
			ZX_ReceiptNumber = ZString.Empty;
		}

		NZAddInfo AddInfo
		{
			get { return addInfo ?? (addInfo = PlugInSupport == null ? null : PlugInSupport.AddInfo); }
		}
		NZAddInfo addInfo;

		public IMAFPlugInSupport PlugInSupport { get; private set; }

		#endregion

		#region IAddInfoManager Members

		Customs.Business.IAddInfo Customs.Business.IAddInfoManager.AddInfo
		{
			get { return DummyAddInfoForWrappedZPropertyInfos; }
		}

		#endregion

		#region TSW IPI Message Interfaces Implementaion

		public ForwardingConsol Consol
		{
			get
			{
				if (consol == null)
				{
					consol = PlugInSupport.Master as ForwardingConsol;
				}

				return consol;
			}
		}
		ForwardingConsol consol;

		#region IImportDeclaration Implementation

		ZBool IImportDeclaration.IsPeriodicImport => false;

		ZBool IImportDeclaration.IsMiscImporter => false;

		Integration.IJobDocAddress IImportDeclaration.MiscImporterAddress
		{
			get
			{
				if (fMiscImporterAddress == null)
				{
					fMiscImporterAddress = Factory.New<JobDocAddress>();
					fMiscImporterAddress.E2_CompanyName = "";
					fMiscImporterAddress.E2_Contact = ZX_ImporterContactName;
				}

				return fMiscImporterAddress;
			}
		}
		Integration.IJobDocAddress fMiscImporterAddress;

		ZDateTime IImportDeclaration.DateOfImport
		{
			get
			{
				var result = ZDateTime.Empty;
				if (Consol.JK_DatePortOfFirstArrival.IsValid)
				{
					result = Consol.JK_DatePortOfFirstArrival;
				}
				else
				{
					var importTransport = Consol.Transports.ImportTransport;
					if (importTransport != null)
					{
						result = importTransport.JW_ATA;
						if (result == ZDateTime.Empty)
						{
							result = importTransport.JW_ETA;
						}
					}
				}

				return result;
			}
		}

		ZDateTime IImportDeclaration.ImportPeriod => ZDateTime.Empty;

		IOrganisation IImportDeclaration.Importer
		{
			get
			{
				if (fImporter == null)
				{
					fImporter = OrgHeaderWrapper.New(Consol.ReceivingForwarder);
				}

				return fImporter;
			}
		}
		IOrganisation fImporter;

		IGoodsShipment IImportDeclaration.GoodsShipment => this;

		ZBool IDeclaration.IsSea => Consol.IsSea;

		ZBool IDeclaration.IsAir => Consol.IsAir;

		ZBool IDeclaration.IsMail => false;

		ZBool IDeclaration.IsContainerised => IsContainerisedForIPI;

		ZBool IDeclaration.IsCompletionEntry => false;

		ZBool IDeclaration.HasContainersOrPallets => IsContainerisedForIPI;

		ZString IDeclaration.MessageType => NZ.TradeSingleWindow.MessageTypeList.Codes.IPI;

		ZString IDeclaration.PaymentType => ZString.Empty;

		ZString IDeclaration.SenderReferenceNumber => Consol.JobNumber;

		ZString IDeclaration.TSWReferenceNumber => ZX_ConsignmentNumber;

		IAdditionalInformation IDeclaration.AdditionalInformation => null; // not used from here, passed directly into message builder from menu

		IEnumerable<IOtherInfo> IDeclaration.OtherInfoCodes => Enumerable.Empty<IOtherInfo>();

		IEnumerable<ZString> IDeclaration.Permits => Enumerable.Empty<ZString>();

		IEnumerable<IOtherInfo> IDeclaration.OtherReferencedDocuments => Enumerable.Empty<IOtherInfo>();

		ZString IDeclaration.HandlingInformation => ZString.Empty;

		ZString IDeclaration.MPIAccountDetails
		{
			get
			{
				var result = ZString.Empty;
				if (!IsPaymentMethodOverriden)
				{
					var accountDetails = PlugInSupport.AccountDetails;
					if (IsAccountDetailsSet(accountDetails))
					{
						result = accountDetails.AccountNumber + "," + accountDetails.AccountHolderName;
					}
				}
				else if (!ZX_AccountNumber.IsEmpty && !ZX_AccountHolder.IsEmpty)
				{
					result = ZX_AccountNumber + "," + ZX_AccountHolder;
				}

				return result;
			}
		}

		ZInt IDeclaration.TransactionType => 9;

		ZDecimal IDeclaration.TotalGrossWeightInKGM => Core.Constants.Weight.ConvertSafe(Consol.JK_TotalShipmentWeight, Consol.JK_TotalShipmentWeightUnit, Core.Constants.Weight.Kilograms);

		ZString IDeclaration.TotalGrossWeightUnit => "KGM";

		ZString IDeclaration.BrokerCode
		{
			get
			{
				ZString brokerCode = NZCustomsDataRegistry.Instance.NZBrokerageID.Value.ToUpperInvariant();
				if (brokerCode.Length < 9)
				{
					brokerCode = brokerCode.PadLeft(9, '0');
				}
				return brokerCode;
			}
		}

		ZString IDeclaration.PremiseID => Consol.IsSea ? SeaCFS : ZString.Empty;

		ZString SeaCFS
		{
			get
			{
				var depotAddress = Consol.JK_OA_UnpackDepotAddress;
				var depotOrg = (OrgHeader)Consol.JK_OA_UnpackDepotAddress_ZAddress?.OrgHeader;
				var orgCustomsCodes = depotOrg?.CustomsCodes.Find(c => c.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.NewZealand && c.OK_CodeType == OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility);
				var atfCode = orgCustomsCodes?.FirstOrDefault(c => c.OK_OA_PremisesAddress == depotAddress) ?? orgCustomsCodes?.FirstOrDefault(c => c.OK_OA_PremisesAddress.IsEmpty);
				return atfCode?.OK_CustomsRegNo ?? ZString.Empty;
			}
		}

		ZString IDeclaration.CraftName => Consol.Vessel?.RV_Name ?? ZString.Empty;

		ZString IDeclaration.LloydsNo => Consol.Vessel?.RV_LloydsNumber ?? ZString.Empty;

		ZString IDeclaration.VoyageNo => Consol.JK_JX_JV_VoyageFlight;

		ZString IDeclaration.FlightNo => Consol.JK_JX_JV_VoyageFlight;

		IEnumerable<ICurrency> IDeclaration.ExchangeRates => null;

		ZDateTime IDeclaration.DepartureDate => ZDateTime.Empty;

		IOrganisationSimple IDeclaration.Carrier
		{
			get
			{
				if (fCarrier == null)
				{
					fCarrier = OrgHeaderWrapper.New(Consol.ShippingLine);
				}

				return fCarrier;
			}
		}
		IOrganisation fCarrier;

		IDeclarant IDeclaration.Declarant => null;

		IEnumerable<IDutyTaxFee> IDeclaration.DutyTaxFees => null;

		IEnumerable<IMasterBillTransportDocument> IDeclaration.MasterBills
		{
			get
			{
				yield return new MasterBillWrapper(Consol);
			}
		}

		IEnumerable<IAssociatedTransportDocument> IDeclaration.AllBills
		{
			get
			{
				foreach (ForwardingShipment shipment in Consol.Shipments)
				{
					if (shipment.JS_ShipmentType != Core.Constants.ShipmentTypes.AssemblyMaster)
					{
						yield return new HouseBillWrapper(shipment);
					}
				}
			}
		}

		IEnumerable<ITransportEquipment> IDeclaration.Equipment
		{
			get
			{
				foreach (ForwardingShipment shipment in Consol.Shipments)
				{
					if (shipment.JS_ShipmentType != Core.Constants.ShipmentTypes.AssemblyMaster)
					{
						foreach (PackLine outerPack in shipment.OuterPackLines)
						{
							foreach (ForwardingContainer container in outerPack.Containers)
							{
								if (container.JC_JK == Consol.PK && !container.JC_ContainerNum.IsEmpty)
								{
									if (shipment.JS_ShipmentType == Core.Constants.ShipmentTypes.CoLoadMaster)
									{
										yield return new TradeSingleWindow.ContainerWrapper(shipment.PK, container);
									}
									else
									{
										yield return new TradeSingleWindow.ContainerWrapper(outerPack.PK, container);
									}
								}
							}
						}
					}
				}
			}
		}

		IEnumerable<IPackaging> IDeclaration.Packaging
		{
			get
			{
				foreach (ForwardingShipment shipment in Consol.Shipments)
				{
					if (shipment.JS_ShipmentType != Core.Constants.ShipmentTypes.AssemblyMaster)
					{
						yield return new ShipmentPackaging(shipment);
					}
				}
			}
		}

		ZString IDeclaration.PreviousDocumentNo => null;

		ZString IDeclaration.PreviousDocumentType => null;

		ZString ITSWSubmitter.SubmitterCode
		{
			get
			{
				var submitterCode = NZCustomsDataRegistry.Instance.NZBrokerageID.Value.ToUpperInvariant();
				if (submitterCode.Length < 9)
				{
					submitterCode = submitterCode.PadLeft(9, '0');
				}

				return submitterCode;
			}
		}

		public ZBool IsContainerisedForIPI => !IsAirConsolOrULD && Consol.Containers.Count > 0;
		public ZBool IsAirConsolOrULD => Consol.IsAir || Consol.JK_ConsolMode == "ULD";

		#endregion

		#region IGoodsShipment Implementation

		ZString IGoodsShipment.ShipmentOrigin => Consol.JK_RL_NKLoadPort.SubstringSafe(0, 2);

		ZString IGoodsShipment.NatureOfTransaction => NZ.TradeSingleWindow.NatureOfTransactionList.Codes.N90;

		ZBool IGoodsShipment.MAFContainerDeclaration => IsContainerisedForIPI;

		IEnumerable<ZString> IGoodsShipment.MAFContainerStatements
		{
			get
			{
				if (((IGoodsShipment)this).MAFContainerDeclaration)
				{
					foreach (ForwardingContainer container in Consol.Containers)
					{
						yield return container.JC_ContainerNum;
					}
				}
			}
		}

		IEnumerable<ZString> IGoodsShipment.MPIApprovedSystemNumbers => Enumerable.Empty<ZString>();

		ZString IGoodsShipment.LocationOfGoods => Consol.JK_OA_UnpackDepotAddress_ZAddress.GetCCPOrATFCode();

		ZString IGoodsShipment.PortOfLoading => Consol.JK_JX_JA_RL_NKPortOfLoading;

		ZString IGoodsShipment.PortOfDischarge => Consol.JK_JX_JB_RL_NKPortOfDischarge;

		ZDecimal IGoodsShipment.FreightCostsInNZD => 0m;

		ZString IGoodsShipment.FreightApportionmentMethod => ZString.Empty;

		IOrganisation IGoodsShipment.DeliverToParty => null;

		ZString IGoodsShipment.CustomsControlledArea => ZString.Empty;

		IEnumerable<IGoodsItems> IGoodsShipment.Items   // => Enumerable.Empty<IGoodsItems>();
		{
			get
			{
				yield return new GoodsItem(Consol);
			}
		}

		IEnumerable<IInvoice> IGoodsShipment.Invoices
		{
			get
			{
				yield return new InvoiceHeader("1", "FOB");
			}
		}

		IEnumerable<IOrganisationSimple> IGoodsShipment.NotifyParties => null;

		IEnumerable<ZString> IGoodsShipment.NotifyPartyCodes => Enumerable.Empty<ZString>();

		IEnumerable<IOrganisation> IGoodsShipment.Sellers => null;

		IEnumerable<IOrganisation> IGoodsShipment.StuffingEstablishments => null;

		IEnumerable<IOrganisation> IGoodsShipment.Suppliers
		{
			get
			{
				yield return OrgHeaderWrapper.New(Consol.SendingForwarder);
			}
		}

		#endregion

		#region GoodsItem

		class GoodsItem : IGoodsItems
		{
			public GoodsItem(ForwardingConsol consol)
			{
				this.consol = consol;
			}
			readonly ForwardingConsol consol;

			public ZDecimal ValueForDutyInNZD => throw new NotImplementedException();

			public ZString TransitionalFacilityCode
			{
				get
				{
					var atfCode = ZString.Empty;
					var mafOrg = MAFOrganisationWrapper.GetMAFOrganisation(consol.UnpackDepotAddress, OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility, null, true);
					if (mafOrg != null)
					{
						var cusCode = OrgCusCode.Load(consol.Factory, OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility, "NZ", consol.UnpackDepotAddress.Header.PK, consol.UnpackDepotAddress.PK);
						if (cusCode != null)
						{
							atfCode = cusCode.OK_CustomsRegNo;
						}
					}

					return atfCode;
				}
			}

			public ZString GoodsDescription => "FAK Container";

			public ZString LotNumber => throw new NotImplementedException();

			public ZDate DateMarking => throw new NotImplementedException();

			public ZBool HasForeignCurrency => throw new NotImplementedException();

			public ZDecimal ValueInForeignCurrency => throw new NotImplementedException();

			public ZString ForeignCurrencyCode => throw new NotImplementedException();

			public ZString IntendedUse => throw new NotImplementedException();

			public ZString IntendedUseCode => throw new NotImplementedException();

			public IEnumerable<IClassification> Classifications => throw new NotImplementedException();

			public IEnumerable<IConstituent> Constituents => throw new NotImplementedException();

			public ZString PreferenceClaimed => throw new NotImplementedException();

			public IEnumerable<IDutyTaxFee> LineDutyTaxFees => throw new NotImplementedException();

			public IOrganisation Grower => throw new NotImplementedException();

			public IEnumerable<ZString> RoutingCountryCodes => throw new NotImplementedException();

			public IOrganisation Manufacturer => throw new NotImplementedException();

			public IOrganisation Producer => throw new NotImplementedException();

			public IEnumerable<IProduct> Products => throw new NotImplementedException();

			public ZString BrandName => throw new NotImplementedException();

			public ZString CommonName => throw new NotImplementedException();

			public ZString RegisteredName => throw new NotImplementedException();

			public ZString TradeName => throw new NotImplementedException();

			public ZBool UsedGoods => throw new NotImplementedException();

			public ZBool GeneticallyModified => throw new NotImplementedException();

			public ZString ExportCountry => consol.JK_RL_NKLoadPort.SubstringSafe(0, 2);

			public ITemperatureRequirements Temperatures => throw new NotImplementedException();

			public IEnumerable<ZString> ContainerNumbers => throw new NotImplementedException();

			public IOrganisationSimple TreatmentProvider => throw new NotImplementedException();

			public ZDecimal ItemGrossWeightInKGM => Core.Constants.Weight.ConvertSafe(consol.JK_TotalShipmentWeight, consol.JK_TotalShipmentWeightUnit, Core.Constants.Weight.Kilograms);

			public ZDecimal ItemNetWeightInKGM => ItemGrossWeightInKGM;

			public ZDecimal StatisticalQty => throw new NotImplementedException();

			public ZString StatisticalQtyUnit => ZString.Empty;

			public ZDecimal SupplementaryQty => ZDecimal.Zero;

			public ZString SupplementaryQtyUnit => ZString.Empty;

			public ZString OriginCountry => consol.JK_RL_NKLoadPort.Left(2);

			public ZString OriginRegion => ZString.Empty;

			public IEnumerable<IPackaging> Packaging => throw new NotImplementedException();

			public IEnumerable<IValuationAdjustment> Adjustments => throw new NotImplementedException();

			public IEnumerable<ZString> Permits => throw new NotImplementedException();

			public IEnumerable<ZString> ProhibitedCodes => throw new NotImplementedException();

			public IEnumerable<IOtherInfo> OtherInfoCodes => throw new NotImplementedException();

			public ZString RelationshipIndicator => "135"; // Parties are not related

			public ZInt SupplierLineIsRelatedTo => 1;

			public ZBool IsPartsRelated => throw new NotImplementedException();

			public ZString IsGSTPrePaid => throw new NotImplementedException();

			public ZString VendorIdentifier => throw new NotImplementedException();
		}

		#endregion

		#region InvoiceHeader

		class InvoiceHeader : IInvoice
		{
			public InvoiceHeader(ZString invNumber, ZString incoTerm)
			{
				this.invNumber = invNumber;
				this.incoTerm = incoTerm;
			}
			readonly ZString invNumber;
			readonly ZString incoTerm;

			public ZString InvoiceNumber
			{
				get { return invNumber; }
			}

			public ZString IncoTerms
			{
				get { return incoTerm; }
			}

			public ZDateTime InvoiceDate
			{
				get { return ZDateTime.Empty; }
			}
		}

		#endregion

		public class MasterBillWrapper : IMasterBillTransportDocument
		{
			public MasterBillWrapper(ForwardingConsol consol)
			{
				this.consol = consol;
			}

			readonly ForwardingConsol consol;

			IEnumerable<ZGuid> IMasterBillTransportDocument.ChildBills
			{
				get
				{
					foreach (ForwardingShipment shipment in consol.Shipments)
					{
						if (shipment.JS_ShipmentType != Core.Constants.ShipmentTypes.AssemblyMaster)
						{
							yield return shipment.PK;
						}
					}
				}
			}

			ZString IAssociatedTransportDocument.BillNumber
			{
				get { return consol.JK_MasterBillNum; }
			}

			ZString IAssociatedTransportDocument.BillType => Enterprise.Customs.NZ.TradeSingleWindow.BillTypeList.Codes.MB;

			IEnumerable<ZGuid> IAssociatedTransportDocument.RelatedEquipment
			{
				get
				{
					if (!consol.IsAir && consol.JK_ConsolMode != "ULD")
					{
						foreach (ForwardingContainer container in consol.Containers)
						{
							yield return container.PK;
						}
					}
				}
			}

			IEnumerable<ZGuid> IAssociatedTransportDocument.RelatedPackages
			{
				get
				{
					foreach (ForwardingShipment shipment in consol.Shipments)
					{
						if (shipment.JS_ShipmentType != Core.Constants.ShipmentTypes.AssemblyMaster)
						{
							yield return shipment.PK;
						}
					}
				}
			}

			ZInt IAssociatedTransportDocument.MessageSequence
			{
				get { return fMessageSequence; }
				set { fMessageSequence = value; }
			}
			ZInt fMessageSequence;

			ZGuid IAssociatedTransportDocument.PK
			{
				get { return consol.PK; }
			}
		}

		public class HouseBillWrapper : IAssociatedTransportDocument
		{
			public HouseBillWrapper(ForwardingShipment shipment)
			{
				this.shipment = shipment;
			}

			readonly ForwardingShipment shipment;

			ForwardingConsol Consol => shipment.ArrivalConsol;

			ZBool IsAirConsolOrULD => Consol != null && (Consol.IsAir || Consol.JK_ConsolMode == "ULD");

			public ZString BillNumber => shipment.JS_HouseBill;

			public ZString BillType => shipment.IsAir ? NZ.TradeSingleWindow.BillTypeList.Codes.HWB : NZ.TradeSingleWindow.BillTypeList.Codes.BM;

			public IEnumerable<ZGuid> RelatedPackages
			{
				get
				{
					if (shipment.JS_ShipmentType != Core.Constants.ShipmentTypes.AssemblyMaster)
					{
						foreach (PackLine outerPack in shipment.OuterPackLines)
						{
							if (IsAirConsolOrULD || outerPack.Containers.Count == 0)
							{
								yield return outerPack.Shipment.PK;
							}
						}
					}
				}
			}

			public IEnumerable<ZGuid> RelatedEquipment
			{
				get
				{
					if (shipment.JS_ShipmentType == Core.Constants.ShipmentTypes.CoLoadMaster)
					{
						yield return shipment.PK;
					}
					else if (shipment.JS_ShipmentType != Core.Constants.ShipmentTypes.AssemblyMaster)
					{
						foreach (ForwardingPackLine outerPack in shipment.OuterPackLines)
						{
							if (outerPack.Containers.Count > 0)
							{
								yield return outerPack.PK;
							}
						}
					}
				}
			}

			public ZInt MessageSequence
			{
				get { return fMessageSequence; }
				set { fMessageSequence = value; }
			}
			ZInt fMessageSequence;

			public ZGuid PK => shipment.PK;
		}

		#region Packaging

		internal class ShipmentPackaging : IPackaging
		{
			public ShipmentPackaging(ForwardingShipment shipment)
			{
				this.shipment = shipment;
			}

			readonly ForwardingShipment shipment;

			public ZString ShippingMarks
			{
				get { return shipment.JS_MarksAndNumbers; }
			}

			public ZInt NumberOfPackages
			{
				get
				{
					var noOfPkgs = ZInt.Zero;
					foreach (PackLine pack in shipment.OuterPackLines)
					{
						noOfPkgs += pack.JL_PackageCount;
					}

					return noOfPkgs;
				}
			}

			public ZString PackageType
			{
				get
				{
					var packageType = ZString.Empty;
					if (shipment.OuterPackLines.Count > 0)
					{
						packageType = PackageTypeConverter.GetCustomsTSWPackagingType(shipment.Factory, shipment.OuterPackLines[0].JL_F3_NKPackType);
					}

					return packageType.IsEmpty ? new ZString("PK") : packageType;
				}
			}

			public ZString PackingMaterialDesc
			{
				get { return string.Empty; }
			}

			public ZDecimal PackageVolumeInMTQ
			{
				get { return ZDecimal.Zero; }
			}

			public ZInt MessageSequence
			{
				get { return fMessageSequence; }
				set { fMessageSequence = value; }
			}
			ZInt fMessageSequence;

			public ZString RelatedHB
			{
				get { return shipment.JS_HouseBill; }
			}

			public ZString RelatedContainer
			{
				get
				{
					var result = ZString.Empty;
					if (shipment.OuterPackLines.Count > 0)
					{
						result = shipment.OuterPackLines[0].JL_Calc_ContainerNum;
					}

					return result;
				}
			}

			public ZGuid PK
			{
				get { return shipment.PK; }
			}
		}

		#endregion

		#endregion
	}
}
