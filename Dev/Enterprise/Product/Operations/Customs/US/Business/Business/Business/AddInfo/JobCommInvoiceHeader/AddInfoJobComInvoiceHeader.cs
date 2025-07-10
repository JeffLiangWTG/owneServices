using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;

namespace Enterprise.Customs.US.Business
{
	public class AddInfoJobComInvoiceHeader : AddInfo
	{
		public AddInfoJobComInvoiceHeader(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty)
		{
		}

		public JobComInvoiceHeader InvoiceHeader
		{
			get { return (JobComInvoiceHeader)base.Parent; }
		}

		public new AddInfoJobComInvoiceHeaderLookups Lookups
		{
			get { return (AddInfoJobComInvoiceHeaderLookups)base.Lookups; }
		}

		public new AddInfoJobComInvoiceHeaderValidation Validation
		{
			get { return (AddInfoJobComInvoiceHeaderValidation)base.Validation; }
		}

		public override ZString US_RoutedTransaction
		{
			get { return base.US_RoutedTransaction; }
			set
			{
				var oldValue = US_RoutedTransaction;
				base.US_RoutedTransaction = value;
				if (!IsCopying && isInitialised && oldValue != US_RoutedTransaction)
				{
					InvoiceHeader.MarkAsNeedingValidation();
					MarkJobDeclaration();
				}
			}
		}

		public override ZString US_ZoneStatus
		{
			get { return base.US_ZoneStatus; }
			set
			{
				bool hasChanged = (value != US_ZoneStatus);
				base.US_ZoneStatus = value;

				if (hasChanged && !IsCopying && isInitialised)
				{
					MarkJobComInvoiceLines();
					MarkJobDeclaration();

					if (US_ZoneStatus != ZoneStatusList.Codes.PrivilegedForeign)
					{
						US_PrivilegedStatusDate = ZDateTime.Empty;
					}
				}
			}
		}

		public override ZString US_EntryType
		{
			get { return base.US_EntryType; }
			set
			{
				bool hasChanges = base.US_EntryType != value;
				base.US_EntryType = value;
				if (hasChanges && !IsCopying && isInitialised)
				{
					MarkJobComInvoiceLines();
				}
			}
		}

		public override ZBool US_IsLineGrouping
		{
			get { return base.US_IsLineGrouping; }
			set
			{
				bool hasChanges = base.US_IsLineGrouping != value;
				base.US_IsLineGrouping = value;
				if (hasChanges && !IsCopying && isInitialised)
				{
					MarkJobComInvoiceLines();
				}
			}
		}

		public override ZString US_PaymentTerms
		{
			get { return base.US_PaymentTerms; }
			set
			{
				bool isDiff = base.US_PaymentTerms != value;
				base.US_PaymentTerms = value;
				if (isDiff && !IsCopying && isInitialised)
				{
					US_PaymentTermsDesc = Lookups.US_PaymentTermsList.GetDescriptionFromCode(US_PaymentTerms);
				}
			}
		}

		public override ZString US_ExportCode
		{
			get { return base.US_ExportCode; }
			set
			{
				var oldValue = base.US_ExportCode;
				base.US_ExportCode = value;
				if (!IsCopying && isInitialised && oldValue != US_ExportCode && IsExport)
				{
					MarkJobComInvoiceLines();
				}
			}
		}

		public override ZString US_UC_NKCountryOfExport
		{
			get { return base.US_UC_NKCountryOfExport; }
			set
			{
				bool hasChanged = value != US_UC_NKCountryOfExport;
				base.US_UC_NKCountryOfExport = value;
				if (!IsCopying && hasChanged && isInitialised)
				{
					MarkJobComInvoiceLines();
				}
			}
		}

		public override ZString US_UC_NKCountryOfOrigin
		{
			get { return base.US_UC_NKCountryOfOrigin; }
			set
			{
				bool hasChanged = value != US_UC_NKCountryOfOrigin;
				base.US_UC_NKCountryOfOrigin = value;
				if (!IsCopying && hasChanged && isInitialised)
				{
					MarkJobComInvoiceLines();
				}
			}
		}

		public override ZString US_SPI
		{
			get { return base.US_SPI; }
			set
			{
				bool hasChanged = value != US_SPI;
				base.US_SPI = value;
				if (!IsCopying && hasChanged && isInitialised)
				{
					MarkJobComInvoiceLines();
				}
			}
		}

		public override ZString US_SecondarySPI
		{
			get { return base.US_SecondarySPI; }
			set
			{
				var oldValue = US_SecondarySPI;
				base.US_SecondarySPI = value;
				if (!IsCopying && isInitialised && oldValue != US_SecondarySPI)
				{
					MarkJobComInvoiceLines();
				}
			}
		}

		public override ZString US_TariffType
		{
			get { return base.US_TariffType; }
			set
			{
				var oldValue = US_TariffType;
				base.US_TariffType = value;
				if (!IsCopying && isInitialised && oldValue != US_TariffType)
				{
					MarkJobComInvoiceLines();
					MarkJobDeclaration();
				}
			}
		}

		public override ZDateTime US_DateOfExport
		{
			get => base.US_DateOfExport;
			set
			{
				var hasChanged = US_DateOfExport != value;
				base.US_DateOfExport = value;
				if (!IsCopying && isInitialised && hasChanged)
				{
					MarkJobComInvoiceLines();
				}
			}
		}

		public override ZDateTime US_DateOfExportFromCountryOfOrigin
		{
			get { return base.US_DateOfExportFromCountryOfOrigin; }
			set
			{
				var oldValue = US_DateOfExportFromCountryOfOrigin;

				base.US_DateOfExportFromCountryOfOrigin = value;

				if (!IsCopying && isInitialised && oldValue != US_DateOfExportFromCountryOfOrigin)
				{
					MarkJobComInvoiceLines();
				}
			}
		}

		public override ZString US_ECCN
		{
			get { return base.US_ECCN; }
			set
			{
				var hasChanges = base.US_ECCN != value;
				base.US_ECCN = value;
				if (hasChanges && !IsCopying && isInitialised)
				{
					MarkJobComInvoiceLines();
				}
			}
		}

		public override ZString US_LicenseType
		{
			get { return base.US_LicenseType; }
			set
			{
				var hasChanges = base.US_LicenseType != value;
				base.US_LicenseType = value;
				if (hasChanges && !IsCopying && isInitialised)
				{
					MarkJobComInvoiceLines();
				}
			}
		}

		public bool US_IsScheduleKTermsOfDeliveryLocation
		{
			get { return US_TermsOfDeliveryLocationIndicator == TermsOfDeliveryLocationCodeIndicators.Codes.ScheduleK; }
		}

		public bool US_IsScheduleDTermsOfDeliveryLocation
		{
			get { return US_TermsOfDeliveryLocationIndicator == TermsOfDeliveryLocationCodeIndicators.Codes.ScheduleD; }
		}

		public bool US_IsOtherTermsOfDeliveryLocation
		{
			get { return US_TermsOfDeliveryLocationIndicator == TermsOfDeliveryLocationCodeIndicators.Codes.Other; }
		}

		public bool US_IsISOCountryCodeTermsOfDeliveryLocation
		{
			get { return US_TermsOfDeliveryLocationIndicator == TermsOfDeliveryLocationCodeIndicators.Codes.ISOCountryCode; }
		}

		void MarkJobComInvoiceLines()
		{
			var invoiceHeader = this.InvoiceHeader;
			if (invoiceHeader != null && !invoiceHeader.IsMarkingAsNeedingValidationSuspended)
			{
				invoiceHeader.JobComInvoiceLines.MarkAsNeedingValidation();
			}
		}

		void MarkJobDeclaration()
		{
			var declaration = InvoiceHeader.JobDeclaration;
			if (declaration != null && !declaration.IsMarkingAsNeedingValidationSuspended)
			{
				declaration.MarkAsNeedingValidation();
			}
		}

		protected override USAddInfoLookups GetNewLookups()
		{
			return new AddInfoJobComInvoiceHeaderLookups(this);
		}

		protected override USAddInfoValidation GetNewValidation()
		{
			ZString messageType = InvoiceHeader.JZ_MessageType;

			if (messageType == JobMessageTypeList.Codes.Export)
			{
				return new ExportAddInfoJobComInvoiceHeaderValidation(this);
			}
			else if (messageType == JobMessageTypeList.Codes.Drawback)
			{
				return new DrawbackAddInfoJobComInvoiceHeaderValidation(this);
			}
			else if (messageType == JobMessageTypeList.Codes.Import)
			{
				return new FormalImportAddInfoJobComInvoiceHeaderValidation(this);
			}
			else if (messageType == JobMessageTypeList.Codes.Recon)
			{
				return new ReconAddInfoJobComInvoiceHeaderValidation(this);
			}
			else if (messageType == JobMessageTypeList.Codes.FTZ)
			{
				return new FTZAddInfoJobComInvoiceHeaderValidation(this);
			}
			return new AddInfoJobComInvoiceHeaderValidation(this);
		}

		protected override ZString GetTransportMode()
		{
			var declaration = InvoiceHeader.JobDeclaration;
			return declaration != null ? declaration.JE_TransportMode : (ZString)Core.Constants.TransportModes.Unknown;
		}

		protected override bool IsExportCore
		{
			get
			{
				var declaration = InvoiceHeader.JobDeclaration;
				return declaration != null ? (bool)declaration.IsExport : (bool)InvoiceHeader.IsExport;
			}
		}
	}
}
