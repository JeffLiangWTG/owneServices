using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	public partial class CusEntryInstruction : AutoTWCusEntryInstruction, Integration.Customs.TW.ICusEntryInstruction, Integration.Customs.ICusCodeDataTypeSupporter, ISequenceNumberHeader
	{
		public CusEntryInstruction(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema
		public new class Schema : AutoTWCusEntryInstruction.Schema
		{
			public const string TW_ICIExamLocation = "TW_ICIExamLocation";
			public const string TW_ICIExamTime = "TW_ICIExamTime";
			public const string UCRNumber = "UCRNumber";
			public const int UCRNumberMaxLength = 35;
			public const string UCROverride = "UCROverride";
		}
		#endregion

		#region override Properties
		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryInstruction|TW_CustomsOffice", Caption = "Office of Receipt", FullDescription = "The Office of Receipt of the declaration. It's used to generate the first 2 digits of the entry number.")]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.CustomsOfficeList))]
		public override ZString CEI_CustomsOffice
		{
			get => base.CEI_CustomsOffice;
			set
			{
				var oldValue = CEI_CustomsOffice;
				base.CEI_CustomsOffice = value;
				if (!IsCopying && CEI_CustomsOffice != oldValue)
				{
					SetDefaultValueForGoodsLocation();
					SetDefaultInvoiceLines();
					SetDefaultValueForBoxNumber();

					var declaration = JobDeclaration;
					if (declaration != null)
					{
						declaration.MarkAsNeedingValidation();
						if (declaration.IsExport)
						{
							declaration.Validation.ValidateJE_OH_Supplier();
						}
						else if (declaration.IsImport)
						{
							declaration.Validation.ValidateJE_OH_Importer();
						}
					}
					if (!IsValidationSuspended)
					{
						JobDeclaration?.Validation.ValidateJE_LocationOfGoods();
					}
					TW_TradersRemarksInfo.RefreshBinding();
				}
			}
		}

		internal void SetDefaultValueForGoodsLocation()
		{
			var locationOfGoodsCode = RegistryHelper.GetDefaultGoodsLocation(CEI_CustomsOffice, JobDeclaration?.JE_MessageType ?? ZString.Empty);
			if (!locationOfGoodsCode.IsEmpty)
			{
				CEI_GoodsLocation = locationOfGoodsCode;
			}
		}

		public void SetZeroTW_DaysOfDelayedDeclarationIfNeeded()
		{
			if (!IsDelayedDeclarationFeeApplicable)
			{
				CEI_DaysOfDelayedDeclaration = ZInt.Zero;
			}
		}

		public ZZRefCusCodeListCombined CustomsOffice => TWRefCusCodeListLoader.GetCustomsOffice(Factory, CEI_CustomsOffice, DateOfValuation);

		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.GoodsLocationCollection))]
		[RelatedBusinessObject("GoodsLocation")]
		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryInstruction|TW_GoodsLocation", Caption = "Export Location")]
		public override ZString CEI_GoodsLocation { get => base.CEI_GoodsLocation; set => base.CEI_GoodsLocation = value; }

		public ZDateTime DateOfValuation => JobDeclaration?.DateOfValuation ?? ZDateTime.Today;

		public ZZRefCusCodeListCombined GoodsLocation => TWRefCusCodeListLoader.GetLocationOfGoods(Factory, CEI_GoodsLocation, DateOfValuation);

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryInstruction|TW_ExamMode", Caption = "Exam. Mode", FullDescription = "The examination mode of the declaration. The default can be set against the Importer's/Exporter's Organization > Details > Configuration > Taiwan.")]
		public override ZString CEI_ExamMode { get => base.CEI_ExamMode; set => base.CEI_ExamMode = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryInstruction|TW_PrintDutyMemo", Caption = "Print Duty Memo", FullDescription = "Tick the box if it is required to print the duty memo for declaration.")]
		public override ZBool CEI_PrintDutyMemo { get => base.CEI_PrintDutyMemo; set => base.CEI_PrintDutyMemo = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryInstruction|TW_WaiverOfExemption", Caption = "Waiver of Exemption", FullDescription = "Tick the box if De Minimis exemption is waived for declaration.")]
		public override ZBool CEI_WaiverOfExemption { get => base.CEI_WaiverOfExemption; set => base.CEI_WaiverOfExemption = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryInstruction|TW_WHSTradeReferenceNo", Caption = "Trade Reference No.", FullDescription = "The reference number of the bonded goods that are reported to customs monthly.")]
		public override ZString CEI_WHSTradeReferenceNo { get => base.CEI_WHSTradeReferenceNo; set => base.CEI_WHSTradeReferenceNo = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryInstruction|CEI_WHSMonth", Caption = "Month", FullDescription = "The month of the bonded goods From/To bonded warehouse.")]
		public override ZString CEI_WHSMonth { get => base.CEI_WHSMonth; set => base.CEI_WHSMonth = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryInstruction|CEI_RORPaymentMethod", Caption = "ROR Payment Method", FullDescription = "Indicate the payment method of all ROR duty treatment.")]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.RORPaymentMethodList))]
		public override ZString CEI_RORPaymentMethod { get => base.CEI_RORPaymentMethod; set => base.CEI_RORPaymentMethod = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryInstruction|TW_ReasonForDuty", Caption = "Reason for Added Duty", FullDescription = "The reason code of duty to be made up for the bonded goods.")]
		public override ZString CEI_ReasonForDuty
		{
			get => base.CEI_ReasonForDuty;
			set
			{
				var oldValue = CEI_ReasonForDuty;
				base.CEI_ReasonForDuty = value;
				if (!IsCopying && oldValue != CEI_ReasonForDuty)
				{
					SetDefaultInvoiceLines();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryInstruction|TW_DaysOfDelayedDeclaration", Caption = "Days of Delayed Declaration", ShortCaption = "Days of Delay", FullDescription = "Calculating the days for late declaration fee.")]
		[ReadOnlyMember(nameof(TW_DaysOfDelayedDeclarationReadOnly))]
		public override ZInt CEI_DaysOfDelayedDeclaration { get => base.CEI_DaysOfDelayedDeclaration; set => base.CEI_DaysOfDelayedDeclaration = value; }

		public void ResetCalculateDaysOfDelayedDeclaration()
		{
			CEI_DaysOfDelayedDeclaration = DaysOfDelayed;
		}

		public ZInt DaysOfDelayed => Factory.GetValue(ref daysOfDelayedCached, () =>
		{
			var eta = JobDeclaration?.JE_DateAtFinalDestination ?? ZDateTime.Empty;
			var dateOfDeclaration = CEI_DateForDuty.IsEmpty ? ZDateTime.Now : CEI_DateForDuty;
			return DutyCalculationHelper.CalculateDaysDelayedDeclaration(dateOfDeclaration, eta);
		});

		CachedProperty<ZInt> daysOfDelayedCached;

		public bool TW_DaysOfDelayedDeclarationReadOnly => !IsDelayedDeclarationFeeApplicable;

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryInstruction|CEI_Style", Caption = "Declaration Type", FullDescription = "The declaration type of entry.")]
		[MaxLength(2)]
		public override ZString CEI_Style
		{
			get { return base.CEI_Style; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(CusEntryInstruction.Schema.CEI_Style))
				{
					var oldValue = base.CEI_Style;
					base.CEI_Style = value;
					if (!IsCopying && CEI_Style != oldValue)
					{
						if (oldValue != CEI_Style)
						{
							CEI_StyleDescriptionInfo.RefreshBinding();
							SetDefaultInvoiceLines();
							SetDutyRefundBaseOnFinalDestinationAndDeclarationType();
							SetZeroTW_DaysOfDelayedDeclarationIfNeeded();
						}
						if (!IsValidationSuspended)
						{
							JobDeclaration?.Validation.ValidateJE_LocationOfGoods();
							JobDeclaration?.MarkAsNeedingValidation();
							Validation.ValidateCEI_ReasonForDuty();
							InvoiceLines?.Cast<JobComInvoiceLine>().ForEach(x =>
							{
								x.Validation.ValidateJI_Procedure();
								x.Validation.ValidateJI_Model();
								x.Validation.ValidateJI_BrandName();
								x.Validation.ValidateJI_CustomsOwnerPartNo();
								x.Validation.ValidateJI_Compositions();
							});
							JobDeclaration?.Validation.ValidateJE_ContainerMode();
						}
					}
				}
			}
		}

		public void SetDefaultCEI_Style()
		{
			if (JobDeclaration != null)
			{
				if (JobDeclaration.IsExport)
				{
					CEI_Style = Constants.DeclarationTypes.Export.G5;
				}
				else if (JobDeclaration.IsImport)
				{
					CEI_Style = Constants.DeclarationTypes.Import.G1;
				}
			}
		}

		void SetDefaultInvoiceLines()
		{
			InvoiceLines?.OfType<JobComInvoiceLine>().ToList().ForEach(i =>
			{
				i.SetDefaultCusValueConvRatioIfNeeded();
				i.MarkAsNeedingValidation();
			});
		}

		public ZBool IsShippingFromFactoryToDutyLevyingArea
		{
			get
			{
				var fTW_ReasonForDuty = CEI_ReasonForDuty;
				return fTW_ReasonForDuty == ReasonforDutyList.Codes.FinishedProductDomesticSales
					|| fTW_ReasonForDuty == ReasonforDutyList.Codes.PortEnterpriseSupplementary
					|| fTW_ReasonForDuty == ReasonforDutyList.Codes.Other;
			}
		}

		public ZBool IsShippedFromFreeTradeZoneToDutyLevyingArea
		{
			get
			{
				var fTW_ReasonForDuty = CEI_ReasonForDuty;
				return fTW_ReasonForDuty == ReasonforDutyList.Codes.SupplementaryEquipment
					|| fTW_ReasonForDuty == ReasonforDutyList.Codes.PoorDiscSupplementary
					|| fTW_ReasonForDuty == ReasonforDutyList.Codes.StolenFinishedGoodsSupplementary
					|| fTW_ReasonForDuty == ReasonforDutyList.Codes.StolenEquipmentSupplementary
					|| fTW_ReasonForDuty == ReasonforDutyList.Codes.PortEnterpriseSupplementary;
			}
		}

		internal void SetDutyRefundBaseOnFinalDestinationAndDeclarationType()
		{
			if (DutyRefundFixedAsFalse)
			{
				CEI_DutyRefund = false;
			}
		}

		public ZPropertyInfo CEI_StyleDescriptionInfo => GetZPropertyInfo(Constants.CEI_StyleDescription);

		public ZString CEI_StyleDescription => Lookups.StyleList.GetDescriptionFromCode(CEI_Style);

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryInstruction|CEI_OA_Warehouse", Caption = "Address", FullDescription = "The organization code of From Bonded Warehouse.")]
		public override ZGuid CEI_OA_Warehouse
		{
			get { return base.CEI_OA_Warehouse; }
			set
			{
				var oldValue = base.CEI_OA_Warehouse;
				base.CEI_OA_Warehouse = value;
				if (!IsCopying && oldValue != value)
				{
					if (!IsValidationSuspended)
					{
						JobDeclaration?.MarkAsNeedingValidation();
						InvoiceLines?.ForEach(x =>
						{
							x.Validation.ValidateJI_Procedure();
							x.MarkAsNeedingValidation();
						});
					}
					TW_TradersRemarksInfo.RefreshBinding();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryInstruction|CEI_OA_Warehouse2", Caption = "Address", FullDescription = "The organization code of To Bonded warehouse.")]
		public override ZGuid CEI_OA_Warehouse2
		{
			get => base.CEI_OA_Warehouse2;
			set
			{
				var oldValue = base.CEI_OA_Warehouse2;
				base.CEI_OA_Warehouse2 = value;
				if (!IsCopying && oldValue != value)
				{
					if (!IsValidationSuspended)
					{
						JobDeclaration?.MarkAsNeedingValidation();
						InvoiceLines?.ForEach(x => x.MarkAsNeedingValidation());
						JobDeclaration?.Validation.ValidateJE_ContainerMode();
					}
					TW_TradersRemarksInfo.RefreshBinding();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryInstruction|CEI_DateForDuty", Caption = "Declaration Date", FullDescription = "When the declaration is created, the declaration date is defaulted to today.")]
		public override ZDateTime CEI_DateForDuty
		{
			get => base.CEI_DateForDuty;
			set
			{
				var oldValue = base.CEI_DateForDuty;
				base.CEI_DateForDuty = value;
				if (!IsCopying && CEI_DateForDuty != oldValue)
				{
					CEI_DateForDutyInfo.RefreshBinding(oldValue);
					TW_TradersRemarksInfo.RefreshBinding();
					if (!IsValidationSuspended)
					{
						JobDeclaration?.MarkAsNeedingValidation();
						JobDeclaration?.Invoices?.ForEach(x => x.MarkAsNeedingValidation());
						JobDeclaration?.InvoiceLines?.ForEach(x => x.MarkAsNeedingValidation());
					}
				}
			}
		}

		[ReadOnlyMember(nameof(DutyRefundFixedAsFalse))]
		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryInstruction|TW_DutyRefund", Caption = "Request Duty Refund", FullDescription = "Indicates if the duty refund is requested for the imported raw materials.")]
		public override ZBool CEI_DutyRefund
		{
			get => base.CEI_DutyRefund;
			set
			{
				var oldValue = CEI_DutyRefund;
				base.CEI_DutyRefund = value;
				if (!IsCopying && oldValue != CEI_DutyRefund && TW_BillOfMaterials_ReadOnly)
				{
					CEI_BillOfMaterials = false;
				}
			}
		}

		public bool DutyRefundFixedAsFalse
		{
			get
			{
				var declarationType = CEI_Style;
				var finalDestination = JobDeclaration?.FinalDestination?.RL_RN_NKCountryCode.ToString();

				return declarationType == Constants.DeclarationTypes.Export.F5 && finalDestination != Core.Constants.CountryCodes.Taiwan;
			}
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryInstruction|TW_IsCoPackaged", Caption = "Co-Packaged", FullDescription = "The package is composed of multiple packages.")]
		public override ZBool CEI_IsCoPackaged { get => base.CEI_IsCoPackaged; set => base.CEI_IsCoPackaged = value; }

		[ReadOnlyMember(nameof(TW_BillOfMaterials_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryInstruction|TW_BillOfMaterials", Caption = "Attach Bill of Materials", FullDescription = "Indicates if the list of raw materials used in export products and the information of their suppliers is attached.")]
		public override ZBool CEI_BillOfMaterials
		{
			get => base.CEI_BillOfMaterials;
			set
			{
				var oldValue = CEI_BillOfMaterials;
				base.CEI_BillOfMaterials = value;
				if (!IsCopying && oldValue != CEI_BillOfMaterials)
				{
					if (!CEI_BillOfMaterials)
					{
						CEI_BOMPageCount = ZInt.Zero;
					}
				}
			}
		}

		public bool TW_BillOfMaterials_ReadOnly => !CEI_DutyRefund;

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryInstruction|TW_BOMPageCount", Caption = "BoM Page Count", FullDescription = "The page count of the attached list of raw materials used in export products and the information of their suppliers.")]
		public override ZInt CEI_BOMPageCount { get => base.CEI_BOMPageCount; set => base.CEI_BOMPageCount = value; }

		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.Organisations))]
		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryInstruction|CEI_OH_Owner", Caption = "New Owner")]
		public override ZGuid CEI_OH_Owner
		{
			get => base.CEI_OH_Owner;
			set
			{
				var oldValue = CEI_OH_Owner;
				base.CEI_OH_Owner = value;
				if (!IsCopying && oldValue != CEI_OH_Owner)
				{
					InvoiceLines?.Cast<JobComInvoiceLine>().ToList().ForEach(invoiceLine =>
					{
						invoiceLine.JI_NewOwnerPartNo = ZString.Empty;
						invoiceLine.NewOwnerProductSyncManager.Refresh();
						invoiceLine.MarkAsNeedingValidation();
					});
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryInstruction|TW_PackageDescription", Caption = "Package Description", FullDescription = "The package description of the shipment.")]
		public override ZString CEI_PackageDescription { get => base.CEI_PackageDescription; set => base.CEI_PackageDescription = value; }

		#region CEI_BoxNumber
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.BoxNumberList))]
		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryInstruction|TW_BoxNumber", Caption = "Box Number", FullDescription = "The unique Box Number issued by customs to the customs broker.")]
		public override ZString CEI_BoxNumber
		{
			get => base.CEI_BoxNumber;
			set
			{
				base.CEI_BoxNumber = value;
				TW_TradersRemarksInfo.RefreshBinding();
			}
		}

		void SetDefaultValueForBoxNumber()
		{
			CEI_BoxNumber = RegistryHelper.GetDefaultValueForBoxNumber(CEI_CustomsOffice);
		}
		#endregion
		#endregion

		#region new Properties

		#region Examination

		[MaxLength(4)]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.ExaminationZoneList))]
		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryInstruction|TW_ICIExamLocation", Caption = "Examination Zone", FullDescription = "The code of examination zone on declaration.")]
		public ZString TW_ICIExamLocation
		{
			get
			{
				return GetExamService()?.ES_SubLocation ?? ZString.Empty;
			}
			set
			{
				CheckMaximumLength(TW_ICIExamLocationInfo, value);
				var service = GetExamService(createIfNotExist: !value.IsEmpty);
				if (service != null && service.ES_SubLocation != value)
				{
					service.ES_SubLocation = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateTW_ICIExamLocation();
					}
					TW_ICIExamLocationInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo TW_ICIExamLocationInfo => GetZPropertyInfo(Schema.TW_ICIExamLocation);

		public bool TW_ICIExamLocation_ReadOnly => EntryNumber.IsEmpty;

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryInstruction|TW_ICIExamTime", Caption = "Examination Date", FullDescription = "The cargo examination date requested for the declaration.")]
		public ZDateTime TW_ICIExamTime
		{
			get
			{
				return GetExamService()?.ES_Booked ?? ZDateTime.Empty;
			}
			set
			{
				var service = GetExamService(createIfNotExist: !value.IsEmpty);
				if (service != null && service.ES_Booked != value)
				{
					service.ES_Booked = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateTW_ICIExamTime();
					}
					TW_ICIExamTimeInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo TW_ICIExamTimeInfo => GetZPropertyInfo(Schema.TW_ICIExamTime);

		public bool TW_ICIExamTime_ReadOnly => EntryNumber.IsEmpty;

		ZString EntryNumber => EntryHeader?.EntryNumber ?? ZString.Empty;

		JobService GetExamService(bool createIfNotExist = false)
		{
			var entryNumber = EntryNumber;
			if (!entryNumber.IsEmpty)
			{
				if (fExamService == null || fExamService.IsDeleted || fExamService.ES_ServiceCode != ServiceTypes.CommodityInspection || fExamService.ES_ServiceNote != entryNumber)
				{
					var query = new ZQuery(JobServiceSchema.ES_ServiceCode, ServiceTypes.CommodityInspection);
					query.AddToFilter(JobServiceSchema.ES_ServiceNote, entryNumber);

					fExamService = (JobService)JobDeclaration.DocsAndCartage.Services.Find(query).FirstOrDefault(s => !s.IsDeleted);
				}

				if (fExamService == null && createIfNotExist)
				{
					fExamService = JobDeclaration.DocsAndCartage.Services.AddNew();
					fExamService.ES_ServiceCode = ServiceTypes.CommodityInspection;
					fExamService.ES_ServiceNote = entryNumber;
				}
			}

			return fExamService;
		}
		JobService fExamService;

		#endregion

		#region TW_TradersRemarks

		[ReadOnlyMember(nameof(TW_TradersRemarksReadOnly))]
		[MaxLength(5000)]
		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryInstruction|TradersRemarksNote", Caption = "Traders Remarks", FullDescription = "Trader's remarks on declaration.")]
		public ZString TW_TradersRemarks
		{
			get
			{
				var result = ZString.Empty;
				if (TW_OverrideTradersRemarks)
				{
					result = TW_TradersRemarksNote.ST_NoteText;
				}
				else
				{
					if (JobDeclaration != null)
					{
						var isImport = JobDeclaration.IsImport;
						var importerOrSupplierHeader = isImport ? JobDeclaration.Importer : JobDeclaration.Supplier;
						ZString bondedID;
						if (importerOrSupplierHeader != null)
						{
							var documentaryAddress = isImport ? JobDeclaration.ImporterDocumentaryAddress : JobDeclaration.SupplierDocumentaryAddress;
							bondedID = documentaryAddress?.CBPCode ?? ZString.Empty;
							result = SharedHelper.GetPOCorPOADocNumber(this, importerOrSupplierHeader, bondedID);
						}

						if (result.IsEmpty)
						{
							var declarationType = isImport ? Constants.DeclarationTypes.Import.D2 : Constants.DeclarationTypes.Export.D5;
							if (CEI_Style == declarationType)
							{
								bondedID = FromWarehouseCode;
								if (!bondedID.IsEmpty && Warehouse.Header is OrgHeader warehoseHeader)
								{
									result = SharedHelper.GetPOCorPOADocNumber(this, warehoseHeader, bondedID);
								}
							}
						}
					}
				}
				return result;
			}
			set
			{
				var oldValue = TW_TradersRemarks;
				CheckMaximumLength(TW_TradersRemarksInfo, value);
				if (oldValue != value)
				{
					var note = TW_TradersRemarksNote;
					if (note == null || note.IsDeleting)
					{
						note = Notes.AddNew(true, PredefinedNoteTypes.Instance.TWTradersRemarks.Description, ZString.Empty);
					}
					note.IgnoreValidationSuspended = false;
					note.ST_NoteText = value;
					HasChanges = true;
					TW_TradersRemarksInfo.RefreshBinding();
					if (!IsValidationSuspended)
					{
						Validation.ValidateTW_TradersRemarks();
					}
				}
			}
		}
		public ZPropertyInfo TW_TradersRemarksInfo => GetZPropertyInfo(nameof(TW_TradersRemarks));
		public ZBool TW_TradersRemarksReadOnly => !TW_OverrideTradersRemarks;

		public ZBool TW_OverrideTradersRemarks
		{
			get
			{
				return TW_TradersRemarksNote != null;
			}
			set
			{
				var note = TW_TradersRemarksNote;
				if (!value && note != null)
				{
					note.Delete();
					HasChanges = true;
					TW_TradersRemarksInfo.RefreshBinding();
				}
				else if (value && note == null)
				{
					Notes.AddNew(true, PredefinedNoteTypes.Instance.TWTradersRemarks.Description, TW_TradersRemarks);
					HasChanges = true;
					TW_TradersRemarksInfo.RefreshBinding();
				}
			}
		}

		[ChildEditable(true)]
		StmNote TW_TradersRemarksNote
		{
			get
			{
				if (fTW_TradersRemarksNote == null || fTW_TradersRemarksNote.IsDeleted)
				{
					fTW_TradersRemarksNote = Notes.FindByDescription(PredefinedNoteTypes.Instance.TWTradersRemarks.Description).FirstOrDefault(x => !x.IsDeleted);
				}
				return fTW_TradersRemarksNote;
			}
		}
		StmNote fTW_TradersRemarksNote;

		#endregion

		#region AttachedDoc

		[MaxLength(35)]
		public ZString TW_AttachedDoc1 => GetDocumentNumber(1)?.CY_Data ?? ZString.Empty;

		[MaxLength(35)]
		public ZString TW_AttachedDoc2 => GetDocumentNumber(2)?.CY_Data ?? ZString.Empty;

		[MaxLength(35)]
		public ZString TW_AttachedDoc3 => GetDocumentNumber(3)?.CY_Data ?? ZString.Empty;

		CusEntryInstructionDocument GetDocumentNumber(int order) => DocumentNumbers.Where(x => x.CY_Order == order).SingleOrDefault();

		#endregion

		[ChildEditable(true)]
		public CusEntryInstructionDocumentCollection DocumentNumbers
		{
			get
			{
				if (documentNumbers == null)
				{
					documentNumbers = new CusEntryInstructionDocumentCollection(this);
					documentNumbers.Load();
					documentNumbers.DocumentNumbersChanged += DocumentNumbers_Changed;
					RegisterEditableChildObject(documentNumbers);
				}
				return documentNumbers;
			}
		}
		CusEntryInstructionDocumentCollection documentNumbers;

		void DocumentNumbers_Changed(object sender, EventArgs e)
		{
			AttachedDocumentNumbersAsStringInfo.RefreshBinding();
		}

		[BusinessObjectTestExclude]
		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryInstruction|AttachedDocumentNumbersAsString", Caption = "Attached Document No.", FullDescription = "The attached document numbers for the declaration. You can use commas (,) to separate multiple document numbers (up to three numbers). Or you can click \"More…\" to open the pop-up window.")]
		public ZString AttachedDocumentNumbersAsString { get => DocumentNumbers.DocumentNumbersAsString; set => DocumentNumbers.DocumentNumbersAsString = value; }

		public ZPropertyInfo AttachedDocumentNumbersAsStringInfo => GetZPropertyInfo(nameof(AttachedDocumentNumbersAsString));

		#endregion

		[ChildEditable(true)]
		public CusTWControllingMessageHeaderCollection ControllingMessageHeaders => fControllingMessageHeaders ?? (fControllingMessageHeaders = GetControllingMessageHeaders());
		CusTWControllingMessageHeaderCollection fControllingMessageHeaders;

		CusTWControllingMessageHeaderCollection GetControllingMessageHeaders()
		{
			var result = new CusTWControllingMessageHeaderCollection(this);
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}

		[ChildEditable(true)]
		public DeclarationDuplicateCollection DeclarationDuplicates
		{
			get
			{
				if (declarationDuplicatesCollection == null)
				{
					declarationDuplicatesCollection = new DeclarationDuplicateCollection(this);
					declarationDuplicatesCollection.Load();
					RegisterEditableChildObject(declarationDuplicatesCollection);
				}
				return declarationDuplicatesCollection;
			}
		}

		DeclarationDuplicateCollection declarationDuplicatesCollection;

		IDictionary<ZString, Type> Integration.Customs.ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusCodeDataTypeList.Codes.DeclarationDuplicate, typeof(DeclarationDuplicate));
			result.Add(CusCodeDataTypeList.Codes.AttachedDocumentNumber, typeof(CusEntryInstructionDocument));
			return result;
		}

		public IEnumerable<IBusinessObjectFetchStrategy> GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				var fNoteTypes = base.NoteTypesCore;
				fNoteTypes.Add(PredefinedNoteTypes.Instance.TWTradersRemarks);
				return fNoteTypes;
			}
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (saveSucceeded)
			{
				DocumentNumbers.ClearHasChanges();
			}
		}

		const int MinimumInvoiceLineCountThresholdForMerge = 50;

		public bool IsInvoiceLinesMergingAllowed
		{
			get
			{
				bool ceiStyleAllowingMerge = CEI_Style == Constants.DeclarationTypes.Import.G1
					|| CEI_Style == Constants.DeclarationTypes.Import.G2
					|| CEI_Style == Constants.DeclarationTypes.Import.G7
					|| CEI_Style == Constants.DeclarationTypes.Export.G3
					|| CEI_Style == Constants.DeclarationTypes.Export.G5;

				return CEI_ExamMode == ExamModeList.Codes.WrittenReview
					&& ceiStyleAllowingMerge
					&& InvoiceLines.Length > MinimumInvoiceLineCountThresholdForMerge;
			}
		}

		internal ZBool IsDelayedDeclarationFeeApplicable => (JobDeclaration?.IsImport ?? ZBool.False) && CEI_Style != Constants.DeclarationTypes.Import.D2 && CEI_Style != Constants.DeclarationTypes.Import.F2;

		public bool HasWarehouse2CCPAddress
		{
			get
			{
				var regNo = Warehouse2.GetCustomsRegNo(OrgCusCode.CodeTypes.ControlledPremisesID);
				return !regNo.IsEmpty;
			}
		}

		#region UCR Details
		CusEntryNumber UCRCusEntryNumber => Factory.GetValue(ref fUCRCusEntryNumber, () =>
		{
			var ucrEntryNum = CusEntryNumber.LoadOrCreate(this, CusEntryNumberTypes.Standard.UniqueConsignementReference, Core.Constants.CountryCodes.Taiwan);
			RegisterEditableChildObject(ucrEntryNum);
			return ucrEntryNum;
		});

		CachedProperty<CusEntryNumber> fUCRCusEntryNumber;

		[MaxLength(Schema.UCRNumberMaxLength)]
		[ReadOnlyMember(nameof(UCRNumber_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryInstruction|UCRNumber", Caption = "UCR Number", FullDescription = "The unique tracking number of the shipment on declaration.")]
		public ZString UCRNumber
		{
			get { return UCRCusEntryNumber.CE_EntryNum; }
			set
			{
				var oldValue = UCRNumber;
				var hasChanged = oldValue != value;
				if (hasChanged)
				{
					UCRCusEntryNumber.CE_EntryNum = value;
					UCRNumberInfo.RefreshBinding(oldValue);
					Validation.ValidateUCRNumber();
				}
			}
		}

		public ZPropertyInfo UCRNumberInfo => GetZPropertyInfo(Schema.UCRNumber);

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryInstruction|UCROverride", Caption = "UCR Override")]
		[ReadOnlyMember(nameof(UCROverride_ReadOnly))]
		public ZBool UCROverride
		{
			get
			{
				return UCROverride_ReadOnly || !UCRCusEntryNumber.CE_EntryIsSystemGenerated;
			}
			set
			{
				var oldValue = UCROverride;
				UCRCusEntryNumber.CE_EntryIsSystemGenerated = !value;
				if (oldValue != UCROverride)
				{
					UCRNumber = ZString.Empty;
					UCROverrideInfo.RefreshBinding(oldValue);
					UCRNumberInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo UCROverrideInfo
		{
			get { return GetZPropertyInfo(Schema.UCROverride); }
		}

		public bool UCRNumber_ReadOnly => !UCROverride;

		public bool UCROverride_ReadOnly => !JobDeclaration?.IsExport ?? true;

		public void UpdateUCR()
		{
			var entryHeader = EntryHeader;
			if (entryHeader != null)
			{
				var fUCRNumber = UCRNumber;
				var fUCRPrefix = UCRHelper.GetUCRPrefix(this);
				if (UCRNumber_ReadOnly && !entryHeader.HasBeenLodgedAtCustoms && !entryHeader.IsWaitingForResponse &&
					(fUCRNumber.IsEmpty || fUCRPrefix.IsEmpty || !fUCRNumber.StartsWith(fUCRPrefix, StringComparison.OrdinalIgnoreCase)))
				{
					UCRNumber = UCRHelper.CalculateUCR(this);
				}
			}
		}

		UCRHelper UCRHelper => fUCRHelper ?? (fUCRHelper = new UCRHelper());
		UCRHelper fUCRHelper;

		public void ResetUCRNumber()
		{
			UCROverride = UCROverride_ReadOnly;
			if (!UCROverride)
			{
				UCRNumber = ZString.Empty;
			}
		}
		#endregion

		public override ZGuid CEI_JE
		{
			get => base.CEI_JE;
			set
			{
				var oldValue = CEI_JE;
				base.CEI_JE = value;
				if (!IsInDatabase)
				{
					UCROverride = UCROverride_ReadOnly;
				}
				if (oldValue != CEI_JE && !IsCopying)
				{
					ControllingMessageHeaders.MarkAsNeedingValidation();
					JobDeclaration?.Invoices?.ForEach(x => x.MarkAsNeedingValidation());
					JobDeclaration?.InvoiceLines?.ForEach(x => x.MarkAsNeedingValidation());
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryInstruction|CEI_MergeBy", Caption = "Merge By")]
		public override ZString CEI_MergeBy { get => base.CEI_MergeBy; set => base.CEI_MergeBy = value; }

		public bool HasValidControllingMsgHeader => ControllingMessageHeaders.Cast<CusTWControllingMessageHeader>().Any(x => !x.IsEmpty);

		protected override bool SupportsCloneCore() => true;

		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.BondedWarehouseTypeList))]
		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryInstruction|FromWarehouseCodeType", Caption = "Bonded ID", FullDescription = "The goods location code issued by customs to From Bonded warehouse.")]
		public ZString FromWarehouseCodeType => GetOrgCusCode(Warehouse)?.OK_CodeType ?? ZString.Empty;

		public ZPropertyInfo FromWarehouseCodeTypeInfo => GetZPropertyInfo(nameof(FromWarehouseCodeType));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryInstruction|FromWarehouseVATCode", Caption = "VAT", FullDescription = "The VAT number of From Bonded warehouse.")]
		public ZString FromWarehouseVATCode => GetVATCode(Warehouse);

		public ZPropertyInfo FromWarehouseVATCodeInfo => GetZPropertyInfo(nameof(FromWarehouseVATCode));

		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.BondedWarehouseTypeList))]
		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryInstruction|ToWarehouseCodeType", Caption = "Bonded ID", FullDescription = "The goods location code issued by customs to To Bonded warehouse.")]
		public ZString ToWarehouseCodeType => GetOrgCusCode(Warehouse2)?.OK_CodeType ?? ZString.Empty;

		public ZPropertyInfo ToWarehouseCodeTypeInfo => GetZPropertyInfo(nameof(ToWarehouseCodeType));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryInstruction|ToWarehouseVATCode", Caption = "VAT", FullDescription = "The VAT number of the To Bonded warehouse.")]
		public ZString ToWarehouseVATCode => GetVATCode(Warehouse2);

		public ZPropertyInfo ToWarehouseVATCodeInfo => GetZPropertyInfo(nameof(ToWarehouseVATCode));

		public override ZString FromWarehouseCode => GetOrgCusCode(Warehouse)?.OK_CustomsRegNo ?? ZString.Empty;

		public override ZString ToWarehouseCode => GetOrgCusCode(Warehouse2)?.OK_CustomsRegNo ?? ZString.Empty;

		OrgCusCode GetOrgCusCode(OrgAddress address)
		{
			OrgCusCode cacheValue = null;
			if (address != null && address.PK.IsValid)
			{
				var cacheKey = string.Format(CultureInfo.InvariantCulture, "WareHouseOrgCusCode{0}", address.PK.ToString());
				cacheValue = Factory.GetCachedValue(cacheKey, () =>
				{
					return address.GetWareHouseOrgCusCode();
				});
			}

			return cacheValue;
		}

		public ZBool IsBillOfMaterialsEnable => JobDeclaration != null && JobDeclaration.IsExport && CEI_BillOfMaterials;

		ZString GetVATCode(OrgAddress address)
		{
			ZString cacheValue = null;
			if (address != null && address.PK.IsValid)
			{
				var cacheKey = string.Format(CultureInfo.InvariantCulture, "OrgCusVATCode{0}", address.PK.ToString());
				cacheValue = Factory.GetCachedValue(cacheKey, () =>
				{
					return address.Header?.GetOrgCusCode(new string[] { OrgCusCode.CodeTypes.VATCode })?.OK_CustomsRegNo ?? ZString.Empty;
				});
			}

			return cacheValue;
		}

		#region ISequenceNumberHeader Members

		IEnumerable<ISequenceNumberLine> ISequenceNumberHeader.Lines => ControllingMessageHeaders.Cast<ISequenceNumberLine>();

		internal ShortSequenceNumberGenerator ControllingMessageHeaderNumberGenerator => controllingMessageHeaderNumberGenerator ?? (controllingMessageHeaderNumberGenerator = new ShortSequenceNumberGenerator(this));
		ShortSequenceNumberGenerator controllingMessageHeaderNumberGenerator;

		public IDisposable GetControllingMessageHeaderNumberRenumberingSuspender() => ControllingMessageHeaderNumberGenerator.GetLineNumberSuspender();

		#endregion
	}
}
