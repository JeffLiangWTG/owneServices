using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	[CodeProperty(USDeclarationFSISLine.Schema.US_HealthCertificateNumber), DescriptionProperty(USDeclarationFSISLine.Schema.US_CommercialDescription)]
	public class USDeclarationFSISLine : USFSISLine
	{
		public USDeclarationFSISLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		public JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Parent; }
		}

		public override ZString US_CommercialDescription
		{
			get { return base.US_CommercialDescription; }
			set
			{
				var oldValue = US_CommercialDescription;
				base.US_CommercialDescription = value;

				if (oldValue != US_CommercialDescription && !IsCopying)
				{
					UpdateValueOnInvoiceLineIfRequired(USFSISLine.Schema.US_CommercialDescription, US_CommercialDescription);
				}
			}
		}

		public override ZDateTime US_DateOfInspection
		{
			get { return base.US_DateOfInspection; }
			set
			{
				var oldValue = US_DateOfInspection;
				base.US_DateOfInspection = value;

				if (oldValue != US_DateOfInspection && !IsCopying)
				{
					UpdateValueOnInvoiceLineIfRequired(USFSISLine.Schema.US_DateOfInspection, US_DateOfInspection);
				}
			}
		}

		public override ZString US_ExportingEstNo
		{
			get { return base.US_ExportingEstNo; }
			set
			{
				var oldValue = US_ExportingEstNo;
				base.US_ExportingEstNo = value;

				if (oldValue != US_ExportingEstNo && !IsCopying)
				{
					UpdateValueOnInvoiceLineIfRequired(USFSISLine.Schema.US_ExportingEstNo, US_ExportingEstNo);
				}
			}
		}

		public override ZString US_HealthCertificateNumber
		{
			get { return base.US_HealthCertificateNumber; }
			set
			{
				var oldValue = US_HealthCertificateNumber;

				if (oldValue != value && !IsCopying)
				{
					UpdateValueOnInvoiceLineIfRequired(USFSISLine.Schema.US_HealthCertificateNumber, value);
				}
				base.US_HealthCertificateNumber = value;
			}
		}

		public override ZString US_ImportingEstNo
		{
			get { return base.US_ImportingEstNo; }
			set
			{
				var oldValue = US_ImportingEstNo;
				base.US_ImportingEstNo = value;

				if (oldValue != US_ImportingEstNo && !IsCopying)
				{
					UpdateValueOnInvoiceLineIfRequired(USFSISLine.Schema.US_ImportingEstNo, US_ImportingEstNo);
				}
			}
		}

		public override ZString US_IntendedUseCode
		{
			get { return base.US_IntendedUseCode; }
			set
			{
				var oldValue = US_IntendedUseCode;
				base.US_IntendedUseCode = value;

				if (oldValue != US_IntendedUseCode && !IsCopying)
				{
					UpdateValueOnInvoiceLineIfRequired(USFSISLine.Schema.US_IntendedUseCode, US_IntendedUseCode);
				}
			}
		}

		public override ZString US_ProductID
		{
			get { return base.US_ProductID; }
			set
			{
				var oldValue = US_ProductID;
				base.US_ProductID = value;

				if (oldValue != US_ProductID && !IsCopying)
				{
					UpdateValueOnInvoiceLineIfRequired(USFSISLine.Schema.US_ProductID, US_ProductID);
				}
			}
		}

		public override ZString US_ProductIDQualifier
		{
			get { return base.US_ProductIDQualifier; }
			set
			{
				var oldValue = US_ProductIDQualifier;
				base.US_ProductIDQualifier = value;

				if (oldValue != US_ProductIDQualifier && !IsCopying)
				{
					UpdateValueOnInvoiceLineIfRequired(USFSISLine.Schema.US_ProductIDQualifier, US_ProductIDQualifier);
				}
			}
		}

		public override ZString US_SealNumbers
		{
			get { return base.US_SealNumbers; }
			set
			{
				var oldValue = US_SealNumbers;
				base.US_SealNumbers = value;

				if (oldValue != US_SealNumbers && !IsCopying)
				{
					UpdateValueOnInvoiceLineIfRequired(USFSISLine.Schema.US_SealNumbers, US_SealNumbers);
				}
			}
		}

		public override ZString US_UC_NKCertificateIssuerCountry
		{
			get { return base.US_UC_NKCertificateIssuerCountry; }
			set
			{
				var oldValue = US_UC_NKCertificateIssuerCountry;
				base.US_UC_NKCertificateIssuerCountry = value;

				if (oldValue != US_UC_NKCertificateIssuerCountry && !IsCopying)
				{
					UpdateValueOnInvoiceLineIfRequired(USFSISLine.Schema.US_UC_NKCertificateIssuerCountry, US_UC_NKCertificateIssuerCountry);
				}
			}
		}

		public override ZString US_UC_NKCountryOfOrigin
		{
			get { return base.US_UC_NKCountryOfOrigin; }
			set
			{
				var oldValue = US_UC_NKCountryOfOrigin;
				base.US_UC_NKCountryOfOrigin = value;

				if (oldValue != US_UC_NKCountryOfOrigin && !IsCopying)
				{
					UpdateValueOnInvoiceLineIfRequired(USFSISLine.Schema.US_UC_NKCountryOfOrigin, US_UC_NKCountryOfOrigin);
				}
			}
		}

		public override ZString US_CertifyingIndividual
		{
			get
			{
				return base.US_CertifyingIndividual;
			}
			set
			{
				var hasChanges = base.US_CertifyingIndividual != value;
				base.US_CertifyingIndividual = value;

				if (hasChanges && !IsCopying)
				{
					var declaration = Declaration;
					if (declaration != null)
					{
						if (value == PartyTypeList.Codes.Importer)
						{
							var importerWrapper = declaration.IORWrapper as IPGAContactDetails;
							if (importerWrapper != null)
							{
								US_PGAContactName = importerWrapper.Name;
								US_PGAContactPhoneNo = importerWrapper.PhoneNumber.SubstringSafe(0, AutoUSFSISLineAddInfo.Schema.US_PGAContactPhoneNoMaxLength);
								US_PGAContactEmail = importerWrapper.EmailAddress;
							}
						}
						else if (value == PartyTypeList.Codes.CustomsBroker)
						{
							US_PGAContactName = declaration.US_FDAContactName;
							US_PGAContactPhoneNo = declaration.US_FDAContactPhoneNo.SubstringSafe(0, AutoUSFSISLineAddInfo.Schema.US_PGAContactPhoneNoMaxLength);
							US_PGAContactEmail = declaration.US_FDAContactEmail;
						}
						UpdateValueOnInvoiceLineIfRequired(USFSISLine.Schema.US_CertifyingIndividual, US_CertifyingIndividual);
					}
				}
			}
		}

		public override ZString US_PGAContactName
		{
			get { return base.US_PGAContactName; }
			set
			{
				var oldValue = US_PGAContactName;
				base.US_PGAContactName = value;

				if (oldValue != US_PGAContactName && !IsCopying)
				{
					UpdateValueOnInvoiceLineIfRequired(USFSISLine.Schema.US_PGAContactName, US_PGAContactName);
				}
			}
		}

		public override ZString US_PGAContactPhoneNo
		{
			get { return base.US_PGAContactPhoneNo; }
			set
			{
				var oldValue = US_PGAContactPhoneNo;
				base.US_PGAContactPhoneNo = value;

				if (oldValue != US_PGAContactPhoneNo && !IsCopying)
				{
					UpdateValueOnInvoiceLineIfRequired(USFSISLine.Schema.US_PGAContactPhoneNo, US_PGAContactPhoneNo);
				}
			}
		}

		public override ZString US_PGAContactEmail
		{
			get { return base.US_PGAContactEmail; }
			set
			{
				var oldValue = US_PGAContactEmail;
				base.US_PGAContactEmail = value;

				if (oldValue != US_PGAContactEmail && !IsCopying)
				{
					UpdateValueOnInvoiceLineIfRequired(USFSISLine.Schema.US_PGAContactEmail, US_PGAContactEmail);
				}
			}
		}

		void UpdateValueOnInvoiceLineIfRequired(string fieldName, IZType effectiveValue)
		{
			InvoiceLineFSISLines.ToList().ForEach(fsisLine => fsisLine[fieldName] = effectiveValue);
		}

		IEnumerable<USInvoiceLineFSISLine> InvoiceLineFSISLines
		{
			get
			{
				if (fsisLines == null)
				{
					fsisLines = Enumerable.Empty<USInvoiceLineFSISLine>();

					var declaration = Declaration;
					if (declaration != null)
					{
						fsisLines = declaration.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.FSISLines).Cast<USInvoiceLineFSISLine>().Where(fsisLine => fsisLine.US_HealthCertificateNumber.EqualsIgnoringCase(US_HealthCertificateNumber));
					}
				}
				return fsisLines;
			}
		}

		IEnumerable<USInvoiceLineFSISLine> fsisLines;

		public void RefreshInvoiceLineFSISLines()
		{
			fsisLines = null;
		}

		public new USDeclarationFSISLineValidation Validation
		{
			get { return (USDeclarationFSISLineValidation)base.Validation; }
		}

		protected override CusAddInfoValidation GetNewValidation()
		{
			return new USDeclarationFSISLineValidation(this);
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var result = (USDeclarationFSISLine)base.CloneInternal(args);
			ClearValuesAfterClone(result);
			return result;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			B7_Type = CusAddInfoTypeAttribute.Codes.USDeclarationFSISCertificate;
		}
	}
}
