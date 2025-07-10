using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class AddInfoJobComInvoiceLine : AddInfo, IInvoiceLineProvider
	{
		public AddInfoJobComInvoiceLine(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty)
		{
		}

		protected override IBusinessObjectFetchStrategy GetFetchStrategy()
		{
			if (IsExport)
			{
				return new ExportFetch(this);
			}
			else
			{
				return new ImportFetch(this);
			}
		}

		class ExportFetch : BusinessObjectFetchStrategy
		{
			public ExportFetch(AddInfoJobComInvoiceLine line)
				: base(line)
			{
			}
		}

		class ImportFetch : BusinessObjectFetchStrategy
		{
			public ImportFetch(AddInfoJobComInvoiceLine line)
				: base(line)
			{
			}
		}

		public override ZString US_TariffType
		{
			get { return base.US_TariffType; }
			set
			{
				bool isDiff = base.US_TariffType != value;
				base.US_TariffType = value;
				if (isDiff && !IsCopying && isInitialised)
				{
					MarkJobDeclaration();
				}
			}
		}

		public override ZString US_SecondarySPI
		{
			get { return base.US_SecondarySPI; }
			set
			{
				bool isDiff = base.US_SecondarySPI != value;
				base.US_SecondarySPI = value;
				if (isDiff && !IsCopying && isInitialised)
				{
					InvoiceLine.MarkAsNeedingValidation();
					if (InvoiceLine.InvoiceHeader != null)
					{
						InvoiceLine.InvoiceHeader.MarkAsNeedingValidation();
					}
				}
			}
		}

		public override ZDecimal US_98ValueInvCurr
		{
			get
			{
				return base.US_98ValueInvCurr;
			}
			set
			{
				bool isDiff = base.US_98ValueInvCurr != value;
				base.US_98ValueInvCurr = value;
				if (isDiff && !IsCopying && isInitialised)
				{
					if (InvoiceLine.InvoiceHeader != null)
					{
						InvoiceLine.InvoiceHeader.MarkAsNeedingValidation();
					}
				}
			}
		}

		[ReadOnlyMember(nameof(US_UC_NKCountryOfExport_ReadOnly))]
		public override ZString US_UC_NKCountryOfExport
		{
			get { return base.US_UC_NKCountryOfExport; }
			set { base.US_UC_NKCountryOfExport = value; }
		}

		public bool US_UC_NKCountryOfExport_ReadOnly
		{
			get { return InvoiceLine.IsSecondaryTariffLine; }
		}

		[ReadOnlyMember(nameof(US_UC_NKCountryOfOrigin_ReadOnly))]
		public override ZString US_UC_NKCountryOfOrigin
		{
			get { return base.US_UC_NKCountryOfOrigin; }
			set
			{
				base.US_UC_NKCountryOfOrigin = value;
				if (isInitialised && InvoiceLine != null && InvoiceLine.InvoiceHeader != null)
				{
					InvoiceLine.InvoiceHeader.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString US_APHISInd
		{
			get => base.US_APHISInd;
			set
			{
				var hasChanged = US_APHISInd != value;
				base.US_APHISInd = value;
				if (!IsCopying && hasChanged)
				{
					MarkJobDeclaration();
				}
			}
		}

		public override ZString US_AMSInd
		{
			get => base.US_AMSInd;
			set
			{
				var hasChanged = US_AMSInd != value;
				base.US_AMSInd = value;
				if (!IsCopying && hasChanged)
				{
					MarkJobDeclaration();
				}
			}
		}

		public override ZString US_DOTIndicator
		{
			get => base.US_DOTIndicator;
			set
			{
				var hasChanged = US_DOTIndicator != value;
				base.US_DOTIndicator = value;
				if (!IsCopying && hasChanged)
				{
					MarkJobDeclaration();
				}
			}
		}

		public override ZString US_VNEInd
		{
			get => base.US_VNEInd;
			set
			{
				var hasChanged = US_VNEInd != value;
				base.US_VNEInd = value;
				if (!IsCopying && hasChanged)
				{
					MarkJobDeclaration();
				}
			}
		}

		public override ZString US_TTBInd
		{
			get => base.US_TTBInd;
			set
			{
				var hasChanged = US_TTBInd != value;
				base.US_TTBInd = value;
				if (!IsCopying && hasChanged)
				{
					MarkJobDeclaration();
				}
			}
		}

		public override ZString US_TSCAInd
		{
			get => base.US_TSCAInd;
			set
			{
				var hasChanged = US_TSCAInd != value;
				base.US_TSCAInd = value;
				if (!IsCopying && hasChanged)
				{
					MarkJobDeclaration();
				}
			}
		}

		public override ZString US_PSTIndicator
		{
			get => base.US_PSTIndicator;
			set
			{
				var hasChanged = US_PSTIndicator != value;
				base.US_PSTIndicator = value;
				if (!IsCopying && hasChanged)
				{
					MarkJobDeclaration();
				}
			}
		}

		public override ZString US_HFCInd
		{
			get => base.US_HFCInd;
			set
			{
				var hasChanged = US_HFCInd != value;
				base.US_HFCInd = value;
				if (!IsCopying && hasChanged)
				{
					MarkJobDeclaration();
				}
			}
		}

		public override ZString US_ODSInd
		{
			get => base.US_ODSInd;
			set
			{
				var hasChanged = US_ODSInd != value;
				base.US_ODSInd = value;
				if (!IsCopying && hasChanged)
				{
					MarkJobDeclaration();
				}
			}
		}

		public override ZString US_NOPInd
		{
			get => base.US_NOPInd;
			set
			{
				var hasChanged = US_NOPInd != value;
				base.US_NOPInd = value;
				if (!IsCopying && hasChanged)
				{
					MarkJobDeclaration();
				}
			}
		}

		public override ZString US_ATFInd
		{
			get => base.US_ATFInd;
			set
			{
				var hasChanged = US_ATFInd != value;
				base.US_ATFInd = value;
				if (!IsCopying && hasChanged)
				{
					MarkJobDeclaration();
				}
			}
		}

		public override ZString US_CPSCInd
		{
			get => base.US_CPSCInd;
			set
			{
				var hasChanged = US_CPSCInd != value;
				base.US_CPSCInd = value;
				if (!IsCopying && hasChanged)
				{
					MarkJobDeclaration();
				}
			}
		}

		public override ZString US_FCCIndicator
		{
			get => base.US_FCCIndicator;
			set
			{
				var hasChanged = US_FCCIndicator != value;
				base.US_FCCIndicator = value;
				if (!IsCopying && hasChanged)
				{
					MarkJobDeclaration();
				}
			}
		}

		public override ZString US_FDAIndicator
		{
			get => base.US_FDAIndicator;
			set
			{
				var hasChanged = US_FDAIndicator != value;
				base.US_FDAIndicator = value;
				if (!IsCopying && hasChanged)
				{
					MarkJobDeclaration();
				}
			}
		}

		public override ZString US_OMCInd
		{
			get => base.US_OMCInd;
			set
			{
				var hasChanged = US_OMCInd != value;
				base.US_OMCInd = value;
				if (!IsCopying && hasChanged)
				{
					MarkJobDeclaration();
				}
			}
		}

		public override ZString US_DEAInd
		{
			get => base.US_DEAInd;
			set
			{
				var hasChanged = US_DEAInd != value;
				base.US_DEAInd = value;
				if (!IsCopying && hasChanged)
				{
					MarkJobDeclaration();
				}
			}
		}

		public override ZString US_DDTCInd
		{
			get => base.US_DDTCInd;
			set
			{
				var hasChanged = US_DDTCInd != value;
				base.US_DDTCInd = value;
				if (!IsCopying && hasChanged)
				{
					MarkJobDeclaration();
				}
			}
		}

		public override ZString US_NMFSSIMPInd
		{
			get => base.US_NMFSSIMPInd;
			set
			{
				var hasChanged = US_NMFSSIMPInd != value;
				base.US_NMFSSIMPInd = value;
				if (!IsCopying && hasChanged)
				{
					MarkJobDeclaration();
				}
			}
		}

		public override ZString US_NMFSAMRInd
		{
			get => base.US_NMFSAMRInd;
			set
			{
				var hasChanged = US_NMFSAMRInd != value;
				base.US_NMFSAMRInd = value;
				if (!IsCopying && hasChanged)
				{
					MarkJobDeclaration();
				}
			}
		}

		public override ZString US_NMFSHMSInd
		{
			get => base.US_NMFSHMSInd;
			set
			{
				var hasChanged = US_NMFSHMSInd != value;
				base.US_NMFSHMSInd = value;
				if (!IsCopying && hasChanged)
				{
					MarkJobDeclaration();
				}
			}
		}

		public override ZString US_NMFS370Ind
		{
			get => base.US_NMFS370Ind;
			set
			{
				var hasChanged = US_NMFS370Ind != value;
				base.US_NMFS370Ind = value;
				if (!IsCopying && hasChanged)
				{
					MarkJobDeclaration();
				}
			}
		}

		public override ZString US_LaceyIndicator
		{
			get => base.US_LaceyIndicator;
			set
			{
				var hasChanged = US_LaceyIndicator != value;
				base.US_LaceyIndicator = value;
				if (!IsCopying && hasChanged)
				{
					MarkJobDeclaration();
				}
			}
		}

		public override ZString US_FWSInd
		{
			get => base.US_FWSInd;
			set
			{
				var hasChanged = US_FWSInd != value;
				base.US_FWSInd = value;
				if (!IsCopying && hasChanged)
				{
					MarkJobDeclaration();
				}
			}
		}

		public override ZString US_FSISInd
		{
			get => base.US_FSISInd;
			set
			{
				var hasChanged = US_FSISInd != value;
				base.US_FSISInd = value;
				if (!IsCopying && hasChanged)
				{
					MarkJobDeclaration();
				}
			}
		}

		public bool US_UC_NKCountryOfOrigin_ReadOnly
		{
			get { return InvoiceLine.IsSecondaryTariffLine; }
		}

		[ReadOnlyMember(nameof(US_SPI_ReadOnly))]
		public override ZString US_SPI
		{
			get { return base.US_SPI; }
			set { base.US_SPI = value; }
		}

		public bool US_SPI_ReadOnly
		{
			get { return InvoiceLine.IsCombinedLine() && InvoiceLine.IsChildLine; }
		}

		[ReadOnlyMember(nameof(US_TransactionsRelated_ReadOnly))]
		public override ZString US_TransactionsRelated
		{
			get { return base.US_TransactionsRelated; }
			set { base.US_TransactionsRelated = value; }
		}

		public bool US_TransactionsRelated_ReadOnly
		{
			get { return InvoiceLine.IsSecondaryTariffLine; }
		}

		#region US_SetInd
		public override ZString US_SetInd
		{
			get { return base.US_SetInd; }
			set
			{
				var hasChanges = base.US_SetInd != value;
				base.US_SetInd = value;
				var invoiceLine = InvoiceLine;
				if (hasChanges && !IsCopying && isInitialised && invoiceLine != null)
				{
					var invoiceHeader = invoiceLine.InvoiceHeader;
					if (invoiceHeader != null && !invoiceHeader.IsMarkingAsNeedingValidationSuspended)
					{
						invoiceHeader.MarkAsNeedingValidation();
						MarkJobDeclaration();
					}
				}
			}
		}
		#endregion

		public override ZString US_ZoneStatus
		{
			get { return base.US_ZoneStatus; }
			set
			{
				bool hasChanged = (value != US_ZoneStatus);
				base.US_ZoneStatus = value;
				if (hasChanged && !IsCopying && isInitialised)
				{
					MarkJobDeclaration();
				}
			}
		}

		public override ZString US_DDTCTrackingStatus
		{
			get { return base.US_DDTCTrackingStatus; }
			set
			{
				var oldValue = US_DDTCTrackingStatus;
				base.US_DDTCTrackingStatus = value;
				if (isInitialised && !IsCopying)
				{
					var newValue = US_DDTCTrackingStatus;
					if (oldValue != newValue)
					{
						InvoiceLine.DDTCDataCorrection.OnStatusUpdated(oldValue, newValue);
					}
				}
			}
		}

		public override ZString US_ODSTrackingStatus
		{
			get { return base.US_ODSTrackingStatus; }
			set
			{
				var oldValue = US_ODSTrackingStatus;
				base.US_ODSTrackingStatus = value;
				if (isInitialised && !IsCopying)
				{
					var newValue = US_ODSTrackingStatus;
					if (oldValue != newValue)
					{
						InvoiceLine.ODSDataCorrection.OnStatusUpdated(oldValue, newValue);
					}
				}
			}
		}

		public override ZString US_TSCATrackingStatus
		{
			get { return base.US_TSCATrackingStatus; }
			set
			{
				var oldValue = US_TSCATrackingStatus;
				base.US_TSCATrackingStatus = value;
				if (isInitialised && !IsCopying)
				{
					var newValue = US_TSCATrackingStatus;
					if (oldValue != newValue)
					{
						InvoiceLine.TSCADataCorrection.OnStatusUpdated(oldValue, newValue);
					}
				}
			}
		}

		void MarkJobDeclaration()
		{
			var declaration = Declaration;
			if (declaration != null && !declaration.IsMarkingAsNeedingValidationSuspended)
			{
				declaration.MarkAsNeedingValidation();
			}
		}

		#region US_SupTariff
		public override ZString US_SupTariff
		{
			get { return base.US_SupTariff; }
			set
			{
				bool hasChanged = (value != US_SupTariff);
				base.US_SupTariff = value;
				if (hasChanged && !IsCopying && isInitialised)
				{
					MarkJobDeclaration();
				}
			}
		}
		#endregion

		[MaxLength(20)]
		public override ZString US_WHSEntryNumber
		{
			get { return base.US_WHSEntryNumber; }
			set { base.US_WHSEntryNumber = value; }
		}

		public JobComInvoiceLine InvoiceLine
		{
			get { return (JobComInvoiceLine)base.Parent; }
		}

		public new AddInfoJobComInvoiceLineLookups Lookups
		{
			get { return (AddInfoJobComInvoiceLineLookups)base.Lookups; }
		}

		public new AddInfoJobComInvoiceLineValidation Validation
		{
			get { return (AddInfoJobComInvoiceLineValidation)base.Validation; }
		}

		protected override USAddInfoLookups GetNewLookups()
		{
			return new AddInfoJobComInvoiceLineLookups(this);
		}

		protected override USAddInfoValidation GetNewValidation()
		{
			var invoiceHeader = InvoiceLine.InvoiceHeader;
			if (invoiceHeader == null || invoiceHeader.IsExport || !(invoiceHeader.JobDeclaration is JobDeclaration declaration))
			{
				return new ExportAddInfoJobComInvoiceLineValidation(this);
			}
			else if (declaration.IsDrawback)
			{
				if (declaration.IsACEDrawback)
				{
					return new ACEDrawbackAddInfoJobComInvoiceLineValidation(this);
				}
				else
				{
					return new ACSDrawbackAddInfoJobComInvoiceLineValidation(this);
				}
			}
			else if (declaration.IsRecon)
			{
				return new ReconAddInfoJobComInvoiceLineValidation(this);
			}
			else if (declaration.IsFTZAdmission)
			{
				return new FTZAddInfoJobComInvoiceLineValidation(this);
			}
			else
			{
				if (declaration.IsACE)
				{
					return new ACEImportAddInfoJobComInvoiceLineValidation(this);
				}
				else
				{
					return new ACSImportAddInfoJobComInvoiceLineValidation(this);
				}
			}
		}

		protected override bool IsExportCore
		{
			get { return InvoiceLine.IsExport; }
		}

		protected override ZString GetTransportMode()
		{
			return InvoiceLine.Declaration.JE_TransportMode;
		}

		protected override SchemaColumn[] ColumnsForFastSearch
		{
			get
			{
				return new SchemaColumn[]
				{
					USAddInfoSchema.US_SPI
				};
			}
		}
	}
}
