using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.DocumentEngineIntegration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class JobDeclarationDrawbackSupporter : NonPersistentBusinessObject, IDrawbackSummary, IObsoleteValidation, IBrokerSignatureProvider, IACEDrawbackSummary, IVisualizerNoteSupporter, ISourceIdentifierProvider
	{
		public JobDeclarationDrawbackSupporter(JobDeclaration jobDeclaration, UpdateActionCode actionCode)
		{
			this.jobDeclaration = jobDeclaration;
			this.jobDeclaration.DocumentSupporter.DocumentPrinted += OnNoticeOfIntentPrinted;
			this.actionCode = actionCode;
		}
		readonly JobDeclaration jobDeclaration;
		readonly UpdateActionCode actionCode;

		#region Drawback Claim Document Members

		public ZString HasMPFRefundRequested
		{
			get { return TotalMPF > 0 ? (ZString)"Y" : ZString.Empty; }
		}

		public ZString FormattedDeclarationNumber
		{
			get { return EntryFilerCode + '-' + jobDeclaration.DeclarationNumber.SubstringSafe(0, 7) + "-" + jobDeclaration.DeclarationNumber.SubstringSafe(7, 1); }
		}

		public ZDecimal TotalMPF
		{
			get { return TotalsCalculated ? totalClaimMPF.Round(2) : ZDecimal.Zero; }
		}

		public ZDecimal TotalOtherFees
		{
			get { return TotalsCalculated ? totalClaimOther.Round(2) : ZDecimal.Zero; }
		}

		public ZDecimal TotalHMF
		{
			get { return TotalsCalculated ? totalClaimHMF.Round(2) : ZDecimal.Zero; }
		}

		public ZDecimal TotalPuertoRicoDrawback
		{
			get { return jobDeclaration.IsACEDrawback ? (TotalsCalculated ? totalClaimPuertoRico.Round(2) : ZDecimal.Zero) : jobDeclaration.US_DRWTotalPRDC; }
		}

		public ZDecimal TotalDrawbackClaimed
		{
			get
			{
				return decimal.Round(TotalPuertoRicoDrawback
					+ TotalHMF
					+ TotalMPF
					+ TotalClaimTax
					+ TotalOtherFees
					+ TotalClaimDuty, 2);
			}
		}

		public ZString FilingMethodManual
		{
			get
			{
				return jobDeclaration.US_DRWFilingMethod == "M" ? "X" : "";
			}
		}

		public ZString FilingMethodABI
		{
			get
			{
				return jobDeclaration.US_DRWFilingMethod == "A" ? "X" : "";
			}
		}

		public ZString NAFTADrawbackYes
		{
			get
			{
				return jobDeclaration.US_NAFTAClaimInd ? "X" : "";
			}
		}

		public ZString NAFTADrawbackNo
		{
			get
			{
				return !jobDeclaration.US_NAFTAClaimInd ? "X" : "";
			}
		}

		public ZString ESPYes
		{
			get
			{
				return jobDeclaration.US_ExporterSummaryInd ? "X" : "";
			}
		}

		public ZString ESPNo
		{
			get
			{
				return !jobDeclaration.US_ExporterSummaryInd ? "X" : "";
			}
		}

		public ZString AcceleratedClaim
		{
			get
			{
				return jobDeclaration.US_AcceleratedClaimInd ? "X" : "";
			}
		}

		public ZString WPNInd
		{
			get
			{
				return jobDeclaration.US_WaiverNoticeInd ? "X" : "";
			}
		}

		public ZString SectionVNAFTASameCondition
		{
			get
			{
				var isUnusedMerchandiseDrawback = false;

				if (jobDeclaration.IsACEDrawback)
				{
					isUnusedMerchandiseDrawback = jobDeclaration.US_EntryType == ACEDrawbackProvisionsList.Codes._08;
				}
				else
				{
					isUnusedMerchandiseDrawback = jobDeclaration.US_EntryType == EntryTypeList.Codes.DirectIdentificationUnusedMerchandiseDrawback;
				}

				return isUnusedMerchandiseDrawback && !jobDeclaration.US_NAFTADrawbackCountry.IsEmpty ? "X" : "";
			}
		}

		public ZString SectionVUnused
		{
			get
			{
				var isUnusedMerchandiseDrawback = false;

				if (jobDeclaration.IsACEDrawback)
				{
					isUnusedMerchandiseDrawback = jobDeclaration.US_EntryType == ACEDrawbackProvisionsList.Codes._08;
				}
				else
				{
					isUnusedMerchandiseDrawback = jobDeclaration.US_EntryType == EntryTypeList.Codes.DirectIdentificationUnusedMerchandiseDrawback;
				}

				return isUnusedMerchandiseDrawback && jobDeclaration.US_NAFTADrawbackCountry.IsEmpty ? "X" : "";
			}
		}

		public ZString SectionVInterchangeable
		{
			get { return IsRejectedMerchandiseDrawback ? "X" : string.Empty; }
		}

		public ZString SectionVNonConform
		{
			get { return IsRejectedMerchandiseDrawback && jobDeclaration.US_DRWRejectedMerchandiseReason == DrawbackRejectedMerchandiseReasonList.Codes.NCS ? "X" : ""; }
		}

		public ZString SectionVDefective
		{
			get { return IsRejectedMerchandiseDrawback && jobDeclaration.US_DRWRejectedMerchandiseReason == DrawbackRejectedMerchandiseReasonList.Codes.DTI ? "X" : ""; }
		}

		public ZString SectionVNoConcent
		{
			get { return IsRejectedMerchandiseDrawback && jobDeclaration.US_DRWRejectedMerchandiseReason == DrawbackRejectedMerchandiseReasonList.Codes.SWC ? "X" : ""; }
		}

		public ZString SectionVSameKind
		{
			get { return IsManufacturingDrawback ? "X" : string.Empty; }
		}

		public ZString SectionVManufacture
		{
			get { return ""; }
		}

		public ZString SectionVPOA
		{
			get { return ""; }
		}

		public ZString SectionVOfficer
		{
			get { return ""; }
		}

		public ZString SectionVBroker
		{
			get { return ""; }
		}

		public ZString DrawbackSection
		{
			get
			{
				return jobDeclaration.US_DRWSection;
			}
		}

		public ZString ClaimantName
		{
			get
			{
				return jobDeclaration.Importer != null ? jobDeclaration.ImporterName : ZString.Empty;
			}
		}

		public ZString ClaimantAddress
		{
			get
			{
				return jobDeclaration.Importer != null ? jobDeclaration.ImporterDocumentaryAddress.AddressAsASingleLine : ZString.Empty;
			}
		}

		public ZString CodeOfBranchOnDeclaration
		{
			get
			{
				return jobDeclaration.Branch != null ? jobDeclaration.Branch.GB_Code : ZString.Empty;
			}
		}

		///
		/// For All Company Name & Address details on CBP forms: use Customs Address of Record details first, then fallback to Original details... Refer WI00030524
		///
		public ZString ContactDetails
		{
			get
			{
				StringBuilder result = new StringBuilder();
				result.Append(ContactNameAndAddress);
				result.Append("\r\n PH ");
				result.Append(ContactPhoneNumber);
				result.Append(" FX ");
				result.Append(ContactFAXNumber);
				result.Append(" EML ");
				result.Append(ContactEmail);
				return result.ToString();
			}
		}

		internal InvoiceLineCompleteCollection ImportSectionInvoiceLines
		{
			get
			{
				if (importSectionInvoiceLines == null)
				{
					ZQuery query = jobDeclaration.Is7552 ? new ZQuery() : new ZQuery(JobComInvoiceLineSchema.JI_AddInfo, SQLComparisonOperator.Contains, "DRWIsForImportSection=Y");
					importSectionInvoiceLines = new InvoiceLineCompleteCollection(jobDeclaration);
					importSectionInvoiceLines.LoadWithMoreFiltering(query);
					importSectionInvoiceLines.Sort(new JobComInvoiceLineComparer());
				}
				return importSectionInvoiceLines;
			}
		}
		InvoiceLineCompleteCollection importSectionInvoiceLines;

		public Drawback7551DocLinePageCollection DrawbackImportSectionLines
		{
			get
			{
				if (drawbackImportSectionLines == null)
				{
					drawbackImportSectionLines = new Drawback7551ImportDocLineCollection(ImportSectionInvoiceLines, Factory);
					drawbackImportSectionLines.LoadDrawback7551DocLines(jobDeclaration);
				}

				return drawbackImportSectionLines;
			}
		}
		Drawback7551DocLinePageCollection drawbackImportSectionLines;

		internal InvoiceLineCompleteCollection ExportSectionInvoiceLines
		{
			get
			{
				if (exportSectionInvoiceLines == null)
				{
					ZQuery query = new ZQuery(JobComInvoiceLineSchema.JI_AddInfo, SQLComparisonOperator.Contains, "DRWIsForExportSection=Y");
					exportSectionInvoiceLines = new InvoiceLineCompleteCollection(jobDeclaration);
					exportSectionInvoiceLines.LoadWithMoreFiltering(query);
					exportSectionInvoiceLines.Sort(new JobComInvoiceLineComparer());
				}
				return exportSectionInvoiceLines;
			}
		}
		InvoiceLineCompleteCollection exportSectionInvoiceLines;

		public Drawback7551DocLinePageCollection DrawbackExportSectionLines
		{
			get
			{
				if (drawbackExportSectionLines == null)
				{
					drawbackExportSectionLines = new Drawback7551ExportDocLineCollection(ExportSectionInvoiceLines, Factory);
					drawbackExportSectionLines.LoadDrawback7551DocLines(jobDeclaration);
				}

				return drawbackExportSectionLines;
			}
		}
		Drawback7551DocLinePageCollection drawbackExportSectionLines;

		public Drawback7551DocLinePageCollection DrawbackManufacturingSectionLines
		{
			get
			{
				if (drawbackManufacturingSectionLines == null)
				{
					drawbackManufacturingSectionLines = new Drawback7551DocLinePageCollection(ManufacturingSectionInvoiceLines, Factory);
					drawbackManufacturingSectionLines.LoadDrawback7551DocLines(jobDeclaration);
				}

				return drawbackManufacturingSectionLines;
			}
		}
		Drawback7551DocLinePageCollection drawbackManufacturingSectionLines;

		internal InvoiceLineCompleteCollection ManufacturingSectionInvoiceLines
		{
			get
			{
				if (manufacturingSectionInvoiceLines == null)
				{
					var query = new ZQuery(JobComInvoiceLineSchema.JI_AddInfo, SQLComparisonOperator.Contains, "DRWIsForManufacturerSection=Y");
					manufacturingSectionInvoiceLines = new InvoiceLineCompleteCollection(jobDeclaration);
					manufacturingSectionInvoiceLines.LoadWithMoreFiltering(query);
					manufacturingSectionInvoiceLines.Sort(new JobComInvoiceLineComparer());
				}
				return manufacturingSectionInvoiceLines;
			}
		}
		InvoiceLineCompleteCollection manufacturingSectionInvoiceLines;

		internal InvoiceLineCompleteCollection ChronologicalSummaryInvoiceLines
		{
			get
			{
				if (chronologicalSummaryInvoiceLines == null)
				{
					ZQuery query = new ZQuery(JobComInvoiceLineSchema.JI_AddInfo, SQLComparisonOperator.Contains, "DRWIsForExportSection=Y");
					chronologicalSummaryInvoiceLines = new InvoiceLineCompleteCollection(jobDeclaration);
					chronologicalSummaryInvoiceLines.LoadWithMoreFiltering(query);
					chronologicalSummaryInvoiceLines.Sort(JobComInvoiceLine.Schema.US_DRWExportDate, System.ComponentModel.ListSortDirection.Ascending);
				}
				return chronologicalSummaryInvoiceLines;
			}
		}
		InvoiceLineCompleteCollection chronologicalSummaryInvoiceLines;

		public Drawback7551DocLinePageCollection ChronologicalSummarySectionLines
		{
			get
			{
				if (chronologicalSummarySectionLines == null)
				{
					chronologicalSummarySectionLines = new Drawback7551DocLinePageCollection(ChronologicalSummaryInvoiceLines, Factory);
					chronologicalSummarySectionLines.LoadDrawback7551DocLines(jobDeclaration);
				}
				return chronologicalSummarySectionLines;
			}
		}
		Drawback7551DocLinePageCollection chronologicalSummarySectionLines;

		public ZString PeriodCoveredFrom
		{
			get
			{
				return jobDeclaration.US_DRWDatePeriodFrom.ToString("MMddyy");
			}
		}

		public ZString PeriodCoveredTo
		{
			get
			{
				return jobDeclaration.US_DRWDatePeriodTo.ToString("MMddyy");
			}
		}

		#endregion

		#region Delivery Certificate for Drawback Purpose Members

		public ZString CertificateOfDelivery
		{
			get
			{
				return jobDeclaration.US_DRWPurpose == DrawbackDeclarationPurposeList.Codes.CD ? "X" : "";
			}
		}

		public ZString CertificateOfManufactureAndDelivery
		{
			get
			{
				return jobDeclaration.US_DRWPurpose == DrawbackDeclarationPurposeList.Codes.CM ? "X" : "";
			}
		}

		public ZString TransfereeAddress
		{
			get
			{
				if (jobDeclaration.Transferee != null)
				{
					return (new AddressFormatter(Factory, jobDeclaration.Transferee, GlbCompany.CurrentCompany, false)).PostalAddressAsASingleLine();
				}

				return ZString.Empty;
			}
		}

		public ZString ContactName
		{
			get
			{
				var customsAddressOfRecord = jobDeclaration.BrokerCustomsAddressOfRecord;
				ZString work = ZString.Empty;
				StringBuilder result = new StringBuilder();
				result.Append(GlbStaff.CurrentUser.GS_FullName);
				result.Append(", ");

				if (customsAddressOfRecord != null)
				{
					work = customsAddressOfRecord.EffectiveCompanyNameTruncated;
				}
				else
				{
					if (work.IsEmpty)
					{
						work = GlbBranch.CurrentBranch.OrgProxy != null ? GlbBranch.CurrentBranch.OrgProxy.OH_FullNameTruncated : ZString.Empty;
					}

					if (work.IsEmpty)
					{
						work = GlbCompany.CurrentCompany.GC_Name;
					}
				}
				result.Append(work);
				return result.ToString();
			}
		}

		public ZString BrokerContactName
		{
			get
			{
				return GlbStaff.CurrentUser.GS_FullName.ToString();
			}
		}

		public ZString BrokerContactAddressOne
		{
			get
			{
				var customsAddressOfRecord = jobDeclaration.BrokerCustomsAddressOfRecord;
				var result = GlbBranch.CurrentBranch.GB_Address1;
				if (customsAddressOfRecord != null)
				{
					result = customsAddressOfRecord.OA_Address1;
				}
				else if (GlbBranch.CurrentBranch.OrgProxy != null)
				{
					result = GlbBranch.CurrentBranch.OrgProxy.MainAddress.OA_Address1;
				}

				return result;
			}
		}

		public ZString BrokerContactAddressTwo
		{
			get
			{
				var customsAddressOfRecord = jobDeclaration.BrokerCustomsAddressOfRecord;
				var result = GlbBranch.CurrentBranch.GB_Address2;
				if (customsAddressOfRecord != null)
				{
					result = customsAddressOfRecord.OA_Address2;
				}
				else if (GlbBranch.CurrentBranch.OrgProxy != null)
				{
					result = GlbBranch.CurrentBranch.OrgProxy.MainAddress.OA_Address2;
				}

				return result;
			}
		}

		public ZString BrokerContactCompanyName
		{
			get
			{
				var customsAddressOfRecord = jobDeclaration.BrokerCustomsAddressOfRecord;
				var result = GlbBranch.CurrentBranch.CompanyName;
				if (customsAddressOfRecord != null)
				{
					result = customsAddressOfRecord.CompanyName;
				}
				else if (GlbBranch.CurrentBranch.OrgProxy != null)
				{
					result = GlbBranch.CurrentBranch.OrgProxy.MainAddress.CompanyName;
				}

				return result;
			}
		}

		public ZString BrokerContactCity
		{
			get
			{
				var customsAddressOfRecord = jobDeclaration.BrokerCustomsAddressOfRecord;
				var result = GlbBranch.CurrentBranch.City;
				if (customsAddressOfRecord != null)
				{
					result = customsAddressOfRecord.City;
				}
				else if (GlbBranch.CurrentBranch.OrgProxy != null)
				{
					result = GlbBranch.CurrentBranch.OrgProxy.MainAddress.City;
				}
				return result;
			}
		}

		public ZString BrokerContactState
		{
			get
			{
				var customsAddressOfRecord = jobDeclaration.BrokerCustomsAddressOfRecord;
				var result = GlbBranch.CurrentBranch.State;
				if (customsAddressOfRecord != null)
				{
					result = customsAddressOfRecord.State;
				}
				else if (GlbBranch.CurrentBranch.OrgProxy != null)
				{
					result = GlbBranch.CurrentBranch.OrgProxy.MainAddress.State;
				}
				return result;
			}
		}

		public ZString BrokerContactZip
		{
			get
			{
				var customsAddressOfRecord = jobDeclaration.BrokerCustomsAddressOfRecord;
				var result = GlbBranch.CurrentBranch.Postcode;
				if (customsAddressOfRecord != null)
				{
					result = customsAddressOfRecord.Postcode;
				}
				else if (GlbBranch.CurrentBranch.OrgProxy != null)
				{
					result = GlbBranch.CurrentBranch.OrgProxy.MainAddress.Postcode;
				}
				return result;
			}
		}

		public ZString ContactAddress
		{
			get
			{
				var customsAddressOfRecord = jobDeclaration.BrokerCustomsAddressOfRecord;
				StringBuilder result = new StringBuilder();
				if (customsAddressOfRecord != null)
				{
					result.Append(customsAddressOfRecord.AddressAsASingleLineWithoutCompanyName);
				}
				else if (GlbBranch.CurrentBranch.OrgProxy != null)
				{
					result.Append(GlbBranch.CurrentBranch.OrgProxy.MainAddress.AddressAsASingleLineWithoutCompanyName);
				}
				else
				{
					result.Append(GlbBranch.CurrentBranch.GB_Address1);
					if (!GlbBranch.CurrentBranch.GB_Address2.IsEmpty)
					{
						result.Append(", ");
						result.Append(GlbBranch.CurrentBranch.GB_Address2);
					}
					result.Append(" ");
					result.Append(GlbBranch.CurrentBranch.GB_City);
					if (!GlbBranch.CurrentBranch.GB_State.IsEmpty)
					{
						result.Append(" ");
						result.Append(GlbBranch.CurrentBranch.GB_State);
					}
					if (!GlbBranch.CurrentBranch.GB_PostCode.IsEmpty)
					{
						result.Append(" ");
						result.Append(GlbBranch.CurrentBranch.GB_PostCode);
					}
				}
				return result.ToString();
			}
		}

		public ZString ContactNameAndAddress
		{
			get
			{
				var result = new ZStringBuilder();
				result.AppendIfNotEmpty(ContactName);
				result.AppendIfNotEmpty(ContactAddress);
				return result.ToStringWithDelimiterBetweenAppends(", ");
			}
		}

		public ZString ContactPhoneNumber
		{
			get
			{
				var result = GlbStaff.CurrentUser.GS_WorkPhone;
				var customsAddressOfRecord = jobDeclaration.BrokerCustomsAddressOfRecord;
				if (result.IsEmpty && customsAddressOfRecord != null)
				{
					result = customsAddressOfRecord.OA_Phone;
				}
				else
				{
					if (result.IsEmpty)
					{
						result = GlbBranch.CurrentBranch.GB_Phone;
					}

					if (result.IsEmpty)
					{
						result = GlbCompany.CurrentCompany.GC_Phone;
					}
				}

				return result;
			}
		}

		public ZString ContactPhoneNumberExtension => GlbStaff.CurrentUser.GS_WorkExtension;

		public ZString ContactFAXNumber
		{
			get
			{
				var result = GlbStaff.CurrentUser.GS_FaxNum;
				var customsAddressOfRecord = jobDeclaration.BrokerCustomsAddressOfRecord;
				if (result.IsEmpty && customsAddressOfRecord != null)
				{
					result = customsAddressOfRecord.OA_Fax;
				}
				else
				{
					if (result.IsEmpty)
					{
						result = GlbBranch.CurrentBranch.GB_Fax;
					}

					if (result.IsEmpty)
					{
						result = GlbCompany.CurrentCompany.GC_Fax;
					}
				}

				return result;
			}
		}

		public ZString ContactEmail
		{
			get
			{
				var result = GlbStaff.CurrentUser.GS_EmailAddress;
				var customsAddressOfRecord = jobDeclaration.BrokerCustomsAddressOfRecord;
				if (result.IsEmpty && jobDeclaration.BrokerCustomsAddressOfRecord != null)
				{
					result = jobDeclaration.BrokerCustomsAddressOfRecord.OA_Email;
				}
				else
				{
					if (result.IsEmpty)
					{
						result = GlbBranch.CurrentBranch.GB_Email;
					}

					if (result.IsEmpty)
					{
						result = GlbCompany.CurrentCompany.GC_Email;
					}
				}
				return result;
			}
		}

		public ZString CompanyName
		{
			get
			{
				var customsAddressOfRecord = jobDeclaration.BrokerCustomsAddressOfRecord;
				ZString work = ZString.Empty;
				StringBuilder result = new StringBuilder();

				if (customsAddressOfRecord != null)
				{
					work = customsAddressOfRecord.EffectiveCompanyNameTruncated;
				}
				else
				{
					if (work.IsEmpty)
					{
						work = GlbBranch.CurrentBranch.OrgProxy != null ? GlbBranch.CurrentBranch.OrgProxy.OH_FullNameTruncated : ZString.Empty;
					}

					if (work.IsEmpty)
					{
						work = GlbCompany.CurrentCompany.GC_Name;
					}
				}
				result.Append(work);
				return result.ToString();
			}
		}

		#endregion

		#region IDrawbackSummary Message Members

		public new bool HasChanges
		{
			get { return jobDeclaration.HasChanges; }
		}

		public new BusinessObjectFactory Factory
		{
			get { return jobDeclaration.Factory; }
		}

		public bool IsABIFiled
		{
			get { return jobDeclaration.US_DRWFilingMethod == DrawbackMethodOfFilingList.Codes.ABI; }
		}

		public ZString EntryFilerCode
		{
			get { return jobDeclaration.US_EntryFilerCode; }
		}

		public ZString ProcessingOfficeCode
		{
			get { return USCustomsDataRegistry.Instance.BRecordOfficeCode.GetValueWithoutFallback(jobDeclaration.RegistryCompanyPK, Guid.Empty, Guid.Empty); }
		}

		public ZString MessageStatus
		{
			get { return jobDeclaration.JE_MessageStatus; }
			set { jobDeclaration.JE_MessageStatus = value; }
		}

		public ZString EntryStatus
		{
			get { return jobDeclaration.JE_EntryStatus; }
			set { jobDeclaration.JE_EntryStatus = value; }
		}

		public void AddMessage(MQEDIMessage message)
		{
			jobDeclaration.Messages.Add(message);
		}

		public ZString DeleteCode
		{
			get { return actionCode == UpdateActionCode.Delete ? "D" : ""; }
		}

		public ZString ClaimNumber
		{
			get { return MQEDIMessage.USEntryFilerEntryNumberPlaceHolder; }
		}

		public ZInt ClaimType
		{
			get { return ZInt.ParseSafe(jobDeclaration.US_EntryType, -1); }
		}

		public ZString ClaimPort
		{
			get { return jobDeclaration.US_ClaimPort; }
		}

		public ZDate EstimatedClaimDate
		{
			get { return jobDeclaration.US_EstimatedEntryDate.Date; }
		}

		public ZString ClaimantIdentification
		{
			get
			{
				return jobDeclaration.Importer == null ? ZString.Empty : jobDeclaration.Importer.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(Core.Constants.CountryCodes.UnitedStates, new ZString[] { OrgCusCode.USACodeTypes.EmployerIdentificationNumber, OrgCusCode.USACodeTypes.SocialSecurityNumber, OrgCusCode.USACodeTypes.CBPAssignedNumber });
			}
		}

		public ZString ClaimantIdentification4Print => OrgHeaderWrapper.GetCustomsCode(jobDeclaration.Importer, new ZString[] { OrgCusCode.USACodeTypes.EmployerIdentificationNumber, OrgCusCode.USACodeTypes.CBPAssignedNumber });

		public ZString DrawbackTeam
		{
			get { return jobDeclaration.US_TeamNo; }
		}

		public ZString BondType
		{
			get { return jobDeclaration.US_BondType; }
		}

		public ZString SuretyCode
		{
			get { return jobDeclaration.US_SuretyCode; }
		}

		public ZString NAFTAClaimIndicator
		{
			get { return jobDeclaration.US_NAFTAClaimInd ? "1" : ""; }
		}

		public ZString GovernmentClaimIndicator
		{
			get { return ""; }
		}

		public ZString AcceleratedClaimIndicator
		{
			get { return jobDeclaration.US_AcceleratedClaimInd ? "Y" : "N"; }
		}

		public ZString ExporterSummaryProcedureIndicator
		{
			get { return jobDeclaration.US_ExporterSummaryInd ? "1" : ""; }
		}

		public ZString WaiverOfPriorNoticeIndicator
		{
			get { return jobDeclaration.US_WaiverNoticeInd ? "1" : ""; }
		}

		public ZString PreInspectionIndicator
		{
			get { return jobDeclaration.US_PreInspectionInd ? "1" : ""; }
		}

		public ZString AgentBroker
		{
			get { return jobDeclaration.CBPF4811ReferenceNumber; }
		}

		public ZString BrokerReferenceNumber
		{
			get { return jobDeclaration.JE_DeclarationReference.Right(9); }
		}

		public ZString LicensePort
		{
			get { return jobDeclaration.US_PreparerDistrictPort; }
		}

		public ZString Description
		{
			get { return jobDeclaration.JE_GoodsDescription; }
		}

		public ZDate EarliestExportDate
		{
			get { return jobDeclaration.US_EarliestExportDate.Date; }
		}

		public ZString PetroleumClaimIndicator
		{
			get { return jobDeclaration.US_PetroleumClaimInd ? "1" : ""; }
		}

		public ZString NAFTADrawbackCountryCode
		{
			get { return jobDeclaration.US_NAFTADrawbackCountry; }
		}

		public ZString FirstContractNumber
		{
			get { return jobDeclaration.ContractNumbers.Count > 0 ? jobDeclaration.ContractNumbers[0].ContractNumberCode : ZString.Empty; }
		}

		public IEnumerable<IDrawbackContractNumber> ExtraContractNumbers
		{
			get
			{
				for (int i = 1; i < jobDeclaration.ContractNumbers.Count; i++)
				{
					if (!jobDeclaration.ContractNumbers[i].CY_Data.IsEmpty)
					{
						yield return jobDeclaration.ContractNumbers[i];
					}
				}
			}
		}

		IEnumerable<ZString> DistinctTrailerTariffs
		{
			get
			{
				var tariffNumbers = new List<ZString>();
				foreach (JobComInvoiceLine line in jobDeclaration.InvoiceLines)
				{
					if (line.IsForImportSectionOfDrawback)
					{
						if (!line.JI_Tariff.IsEmpty && !tariffNumbers.Contains(line.JI_Tariff))
						{
							tariffNumbers.Add(line.JI_Tariff);
						}
						foreach (DrawbackAdditionalImportTariffNumber tariff in line.DrawbackAdditionalImportTariffNumbers.OfType<DrawbackAdditionalImportTariffNumber>().OrderBy(x => x.US_LineNo))
						{
							if (!tariff.US_Tariff.IsEmpty && !tariffNumbers.Contains(tariff.US_Tariff))
							{
								tariffNumbers.Add(tariff.US_Tariff);
							}
						}
					}
				}
				return tariffNumbers;
			}
		}

		public IEnumerable<IDrawbackTrailerTariff> TrailerTariffs
		{
			get
			{
				foreach (var group in DistinctTrailerTariffs.ToGroupsOf(5))
				{
					yield return new DrawbackTrailerTariff(group);
				}
			}
		}

		public class DrawbackTrailerTariff : IDrawbackTrailerTariff
		{
			public DrawbackTrailerTariff(IEnumerable<ZString> tariffs)
			{
				var count = tariffs.Count();
				if (count > 4)
				{ AdditionalTariffNumber3 = tariffs.ElementAt(4); }
				if (count > 3)
				{ AdditionalTariffNumber2 = tariffs.ElementAt(3); }
				if (count > 2)
				{ AdditionalTariffNumber1 = tariffs.ElementAt(2); }
				if (count > 1)
				{ AdditionalTariffNumber = tariffs.ElementAt(1); }
				if (count > 0)
				{ FirstTariffNumber = tariffs.ElementAt(0); }
			}

			public ZString FirstTariffNumber { get; private set; }
			public ZString AdditionalTariffNumber { get; private set; }
			public ZString AdditionalTariffNumber1 { get; private set; }
			public ZString AdditionalTariffNumber2 { get; private set; }
			public ZString AdditionalTariffNumber3 { get; private set; }
		}

		IEnumerable<ZString> DistinctTrailerScheduleBNumbers
		{
			get
			{
				var scheduleBNumbers = new List<ZString>();
				foreach (JobComInvoiceLine line in jobDeclaration.InvoiceLines)
				{
					if (line.IsForExportSectionOfDrawback)
					{
						if (!line.US_ExportTariff.IsEmpty && !scheduleBNumbers.Contains(line.US_ExportTariff))
						{
							scheduleBNumbers.Add(line.US_ExportTariff);
						}
						foreach (DrawbackAdditionalExportTariffNumber tariff in line.DrawbackAdditionalExportTariffNumbers.OfType<DrawbackAdditionalExportTariffNumber>().OrderBy(x => x.CY_Order))
						{
							if (!tariff.CY_Data.IsEmpty && !scheduleBNumbers.Contains(tariff.CY_Data))
							{
								scheduleBNumbers.Add(tariff.CY_Data);
							}
						}
					}
				}
				return scheduleBNumbers;
			}
		}

		public IEnumerable<IDrawbackTrailerScheduleBNumber> TrailerScheduleBNumbers
		{
			get
			{
				foreach (var group in DistinctTrailerScheduleBNumbers.ToGroupsOf(5))
				{
					yield return new DrawbackTrailerScheduleBNumber(group);
				}
			}
		}

		public class DrawbackTrailerScheduleBNumber : IDrawbackTrailerScheduleBNumber
		{
			public DrawbackTrailerScheduleBNumber(IEnumerable<ZString> tariffs)
			{
				var count = tariffs.Count();
				if (count > 4)
				{ AdditionalScheduleBNumber3 = tariffs.ElementAt(4); }
				if (count > 3)
				{ AdditionalScheduleBNumber2 = tariffs.ElementAt(3); }
				if (count > 2)
				{ AdditionalScheduleBNumber1 = tariffs.ElementAt(2); }
				if (count > 1)
				{ AdditionalScheduleBNumber = tariffs.ElementAt(1); }
				if (count > 0)
				{ FirstScheduleBNumber = tariffs.ElementAt(0); }
			}

			public ZString FirstScheduleBNumber { get; private set; }
			public ZString AdditionalScheduleBNumber { get; private set; }
			public ZString AdditionalScheduleBNumber1 { get; private set; }
			public ZString AdditionalScheduleBNumber2 { get; private set; }
			public ZString AdditionalScheduleBNumber3 { get; private set; }
		}

		public IEnumerable<IDrawbackImportClaim> ImportClaims
		{
			get
			{
				Dictionary<ZString, DrawbackImportClaim> drawbackImportClaims = new Dictionary<ZString, DrawbackImportClaim>();
				foreach (JobComInvoiceLine line in jobDeclaration.InvoiceLines)
				{
					IDrawbackImportClaim lineCached = line;
					if (line.US_DRWCertOfManufacture.IsEmpty)
					{
						DrawbackImportClaim existingValue;
						if (drawbackImportClaims.TryGetValue(lineCached.DrawbackImportEntry, out existingValue))
						{
							existingValue.DrawbackClaimDuty += lineCached.DrawbackClaimDuty;
							existingValue.DrawbackClaimTax += lineCached.DrawbackClaimTax;
						}
						else
						{
							DrawbackImportClaim newValue = new DrawbackImportClaim();
							newValue.DrawbackImportEntry = lineCached.DrawbackImportEntry;
							newValue.DrawbackImportEntryPort = lineCached.DrawbackImportEntryPort;
							newValue.DrawbackImportEntryDate = lineCached.DrawbackImportEntryDate;
							newValue.CMCDIndicator = lineCached.CMCDIndicator;
							newValue.DrawbackClaimDuty = lineCached.DrawbackClaimDuty;
							newValue.DrawbackClaimTax = lineCached.DrawbackClaimTax;
							drawbackImportClaims.Add(lineCached.DrawbackImportEntry, newValue);
						}
					}
				}
				foreach (DrawbackImportClaim drawbackImportClaim in drawbackImportClaims.Values)
				{
					yield return drawbackImportClaim;
				}
			}
		}

		public class DrawbackImportClaim : IDrawbackImportClaim
		{
			ZString drawbackImportEntry;
			ZString drawbackImportEntryPort;
			ZDate drawbackImportEntryDate;
			ZString cMCDIndicator;
			ZDecimal drawbackClaimDuty;
			ZDecimal drawbackClaimTax;

			public ZString DrawbackImportEntry
			{
				get { return drawbackImportEntry; }
				set { drawbackImportEntry = value; }
			}

			public ZString DrawbackImportEntryPort
			{
				get { return drawbackImportEntryPort; }
				set { drawbackImportEntryPort = value; }
			}

			public ZDate DrawbackImportEntryDate
			{
				get { return drawbackImportEntryDate; }
				set { drawbackImportEntryDate = value; }
			}

			public ZString CMCDIndicator
			{
				get { return cMCDIndicator; }
				set { cMCDIndicator = value; }
			}

			public ZDecimal DrawbackClaimDuty
			{
				get { return drawbackClaimDuty; }
				set { drawbackClaimDuty = value; }
			}

			public ZDecimal DrawbackClaimTax
			{
				get { return drawbackClaimTax; }
				set { drawbackClaimTax = value; }
			}
		}

		public IEnumerable<IDrawbackManufactureClaim> ManufactureClaims
		{
			get
			{
				Dictionary<ZString, DrawbackManufactureClaim> drawbackManufactureClaims = new Dictionary<ZString, DrawbackManufactureClaim>();
				foreach (JobComInvoiceLine line in jobDeclaration.InvoiceLines)
				{
					IDrawbackManufactureClaim lineCached = line;
					if (!line.US_DRWCertOfManufacture.IsEmpty)
					{
						DrawbackManufactureClaim existingValue;
						if (drawbackManufactureClaims.TryGetValue(lineCached.CertificateOfManufactureNumber, out existingValue))
						{
							existingValue.DrawbackClaimDuty += lineCached.DrawbackClaimDuty;
							existingValue.DrawbackClaimTax += lineCached.DrawbackClaimTax;
							existingValue.DrawbackManufactureQuantity += lineCached.DrawbackManufactureQuantity;
						}
						else
						{
							DrawbackManufactureClaim newValue = new DrawbackManufactureClaim();
							newValue.CertificateOfManufactureNumber = lineCached.CertificateOfManufactureNumber;
							newValue.CertificateOfManufacturePort = lineCached.CertificateOfManufacturePort;
							newValue.DrawbackClaimDuty = lineCached.DrawbackClaimDuty;
							newValue.DrawbackClaimTax = lineCached.DrawbackClaimTax;
							newValue.DrawbackManufactureQuantity = lineCached.DrawbackManufactureQuantity;
							newValue.DrawbackManufactureUnitOfMeasure = lineCached.DrawbackManufactureUnitOfMeasure;
							newValue.DescriptionForBlock41 = lineCached.DescriptionForBlock41;
							drawbackManufactureClaims.Add(lineCached.CertificateOfManufactureNumber, newValue);
						}
					}
				}
				foreach (DrawbackManufactureClaim drawbackManufactureClaim in drawbackManufactureClaims.Values)
				{
					yield return drawbackManufactureClaim;
				}
			}
		}

		public class DrawbackManufactureClaim : IDrawbackManufactureClaim
		{
			ZString certificateOfManufactureNumber;
			ZString certificateOfManufacturePort;
			ZDecimal drawbackClaimDuty;
			ZDecimal drawbackClaimTax;
			ZDecimal drawbackManufactureQuantity;
			ZString drawbackManufactureUnitOfMeasure;
			ZString descriptionForBlock41;

			public ZString CertificateOfManufactureNumber
			{
				get { return certificateOfManufactureNumber; }
				set { certificateOfManufactureNumber = value; }
			}

			public ZString CertificateOfManufacturePort
			{
				get { return certificateOfManufacturePort; }
				set { certificateOfManufacturePort = value; }
			}

			public ZDecimal DrawbackClaimDuty
			{
				get { return drawbackClaimDuty; }
				set { drawbackClaimDuty = value; }
			}

			public ZDecimal DrawbackClaimTax
			{
				get { return drawbackClaimTax; }
				set { drawbackClaimTax = value; }
			}

			public ZDecimal DrawbackManufactureQuantity
			{
				get { return drawbackManufactureQuantity; }
				set { drawbackManufactureQuantity = value; }
			}

			public ZString DrawbackManufactureUnitOfMeasure
			{
				get { return drawbackManufactureUnitOfMeasure; }
				set { drawbackManufactureUnitOfMeasure = value; }
			}

			public ZString DescriptionForBlock41
			{
				get { return descriptionForBlock41; }
				set { descriptionForBlock41 = value; }
			}
		}

		public IEnumerable<IDrawbackNAFTATariff> NAFTATariffs
		{
			get
			{
				foreach (JobComInvoiceLine line in jobDeclaration.InvoiceLines)
				{
					foreach (DrawbackNAFTA tariff in line.DrawbackNAFTAs)
					{
						yield return tariff;
					}
				}
			}
		}

		ZDecimal totalClaimDuty;
		ZDecimal totalClaimTax;
		ZDecimal totalClaimMPF;
		ZDecimal totalClaimHMF;
		ZDecimal totalClaimOther;
		ZDecimal totalClaimPuertoRico;

		ZDecimal totalAdjClaimDuty;
		ZDecimal totalAdjClaimTax;
		ZDecimal totalAdjClaimMPF;
		ZDecimal totalAdjClaimHMF;
		ZDecimal totalAdjClaimOther;

		ZDecimal totalNAFTADuty;
		ZDecimal totalUSDNAFTADuty;

		ZDecimal grandTotalDutyAmount;
		ZDecimal grandTotalUserFeeAmount;
		ZDecimal grandTotalIRTaxAmount;

		CachedProperty<ZBool> totalsCalculated;
		public ZBool TotalsCalculated
		{
			get
			{
				if (totalsCalculated == null)
				{
					totalsCalculated = new CachedProperty<ZBool>(Factory, delegate
					{
						totalClaimDuty = 0m;
						totalClaimTax = 0m;
						totalClaimMPF = 0m;
						totalClaimHMF = 0m;
						totalClaimOther = 0m;
						totalClaimPuertoRico = 0m;

						totalAdjClaimDuty = 0m;
						totalAdjClaimTax = 0m;
						totalAdjClaimMPF = 0m;
						totalAdjClaimHMF = 0m;
						totalAdjClaimOther = 0m;

						totalNAFTADuty = 0m;
						totalUSDNAFTADuty = 0m;

						grandTotalDutyAmount = 0m;
						grandTotalUserFeeAmount = 0m;
						grandTotalIRTaxAmount = 0m;

						var shouldClaim99Percent = !jobDeclaration.Is7552;
						var externalFetchHintSupporter = (IExternalFetchHintSupporter)Factory;

						using (externalFetchHintSupporter.SetupCreator())
						{
							externalFetchHintSupporter.AddTableFetchHintCreator(JobComInvoiceLineSchema.Instance, GetJobComInvoiceLineFetchHints);
							foreach (JobComInvoiceLine line in jobDeclaration.InvoiceLines)
							{
								if (line.IsForImportSectionOfDrawback)
								{
									totalClaimDuty += shouldClaim99Percent ? line._99ClaimedDuty : line.ClaimedDuty;
									totalClaimTax += shouldClaim99Percent ? line._99ClaimedTax : line.ClaimedTax;
									totalClaimMPF += shouldClaim99Percent ? line._99ClaimedMPF : line.ClaimedMPF;
									totalClaimHMF += shouldClaim99Percent ? line._99ClaimedHMF : line.ClaimedHMF;

									totalAdjClaimDuty += line.AdjClaimDuty;
									totalAdjClaimTax += line.AdjClaimTax;
									totalAdjClaimMPF += line.AdjClaimMPF;
									totalAdjClaimHMF += line.AdjClaimHMF;

									grandTotalDutyAmount += (shouldClaim99Percent ? line._99ClaimedDuty : line.ClaimedDuty) + line.AdjClaimDuty;
									grandTotalIRTaxAmount += (shouldClaim99Percent ? line._99ClaimedTax : line.ClaimedTax) + line.AdjClaimTax;
									grandTotalUserFeeAmount += (shouldClaim99Percent ? line._99ClaimedMPF : line.ClaimedMPF) + line.AdjClaimMPF;
									grandTotalUserFeeAmount += (shouldClaim99Percent ? line._99ClaimedHMF : line.ClaimedHMF) + line.AdjClaimHMF;

									if (jobDeclaration.IsACEDrawback)
									{
										foreach (DrawbackOtherFee otherFee in line.DrawbackOtherFees)
										{
											var fee = otherFee.US_FeeType;
											var amount = shouldClaim99Percent ? otherFee._99ClaimedAmount : otherFee.ClaimedAmount;
											var adjAmount = otherFee.AdjClaimAmount;

											if (DrawbackOtherFeeTypesList.IsGrandTotalDutyAmountFee(fee))
											{
												grandTotalDutyAmount += amount;
												grandTotalDutyAmount += adjAmount;
											}
											else if (DrawbackOtherFeeTypesList.IsGrandTotalIRTaxAmountFee(fee))
											{
												grandTotalIRTaxAmount += amount;
												grandTotalIRTaxAmount += adjAmount;
											}
											else
											{
												grandTotalUserFeeAmount += amount;
												grandTotalUserFeeAmount += adjAmount;
											}

											if (DrawbackOtherFeeTypesList.IsOtherFee(fee))
											{
												totalClaimOther += amount;
												totalAdjClaimOther += adjAmount;
											}

											if (DrawbackOtherFeeTypesList.IsPuertoRicoFee(fee))
											{
												totalClaimPuertoRico += amount;
											}
										}
									}
									else
									{
										totalClaimOther += shouldClaim99Percent ? line._99ClaimedOtherFees : line.ClaimedOtherFees;
									}

									foreach (DrawbackNAFTA drawbackNAFTA in line.DrawbackNAFTAs)
									{
										totalNAFTADuty += drawbackNAFTA.US_DRWNAFTACountryImportDuty;
										totalUSDNAFTADuty += drawbackNAFTA.US_DRWEquivalentUSDollarAmountOfNAFTACountryDuty;
									}
								}
							}
						}
						return true;
					});
				}
				return totalsCalculated.Value;
			}
		}

		static IEnumerable<IFetchHint> GetJobComInvoiceLineFetchHints(IColumnIndexer invLine)
		{
			var linePK = invLine.GetValue(JobComInvoiceLineSchema.PK);
			if (linePK.IsValid)
			{
				yield return new FetchHint(CusAddInfoSchema.B7_ParentID, linePK);
			}
		}

		public ZDecimal TotalClaim
		{
			get { return decimal.Round(TotalClaimDuty + TotalClaimTax, 2); }
		}

		public ZDecimal TotalAdjDutyClaimAmount => TotalsCalculated ? totalAdjClaimDuty : ZDecimal.Zero;
		public ZDecimal TotalAdjTaxClaimAmount => TotalsCalculated ? totalAdjClaimTax : ZDecimal.Zero;
		public ZDecimal TotalAdjHMFClaimAmount => TotalsCalculated ? totalAdjClaimHMF : ZDecimal.Zero;
		public ZDecimal TotalAdjMPFClaimAmount => TotalsCalculated ? totalAdjClaimMPF : ZDecimal.Zero;
		public ZDecimal TotalAdjOtherClaimAmount => TotalsCalculated ? totalAdjClaimOther : ZDecimal.Zero;

		public ZDecimal TotalClaimDuty
		{
			get
			{
				return TotalsCalculated ? totalClaimDuty.Round(2) : ZDecimal.Zero;
			}
		}

		public ZDecimal TotalClaimTax
		{
			get
			{
				return TotalsCalculated ? totalClaimTax.Round(2) : ZDecimal.Zero;
			}
		}

		public ZDecimal TotalNAFTACountryImportDuty
		{
			get
			{
				return TotalsCalculated ? totalNAFTADuty : ZDecimal.Zero;
			}
		}

		public ZDecimal TotalUSDollarEquivalentOfNAFTACountryDuty
		{
			get
			{
				return TotalsCalculated ? totalUSDNAFTADuty : ZDecimal.Zero;
			}
		}

		#endregion
		#region JobComInvoiceLineComparer class

		class JobComInvoiceLineComparer : IComparer<JobComInvoiceLine>
		{
			public int Compare(JobComInvoiceLine x, JobComInvoiceLine y)
			{
				if (x == null)
				{
					return -1;
				}

				if (y == null)
				{
					return 1;
				}

				int result = x.US_DRWEntryDate.CompareTo(y.US_DRWEntryDate);
				if (result == 0)
				{
					result = x.JI_LineNo.CompareTo(y.JI_LineNo);
				}
				return result;
			}
		}

		#endregion

		#region Calculation Exhibits Members

		public ZString ClientReference
		{
			get { return jobDeclaration.JE_OwnerRef; }
		}

		public ZString HasDutyCalculationLines
		{
			get { return DutyCalculationLines.Count > 0 ? ZString.Empty : (ZString)"N"; }
		}

		public ZString HasTaxCalculationLines
		{
			get { return TaxCalculationLines.Count > 0 ? ZString.Empty : (ZString)"N"; }
		}

		public ZString HasMPFCalculationLines
		{
			get { return MPFCalculationLines.Count > 0 ? ZString.Empty : (ZString)"N"; }
		}

		public ZString HasHMFCalculationLines
		{
			get { return HMFCalculationLines.Count > 0 ? ZString.Empty : (ZString)"N"; }
		}

		public ZString HasOtherFeesCalculationLines
		{
			get { return OtherFeesCalculationLines.Count > 0 ? ZString.Empty : (ZString)"N"; }
		}

		public DutyCalculationExhibitDocLineCollection DutyCalculationLines
		{
			get { return fDutyCalculationLines ?? (fDutyCalculationLines = new DutyCalculationExhibitDocLineCollection(this)); }
		}
		DutyCalculationExhibitDocLineCollection fDutyCalculationLines;

		public CalculationExhibitDocLineCollection TaxCalculationLines
		{
			get { return fTaxCalculationLines ?? (fTaxCalculationLines = new CalculationExhibitDocLineCollection(this, l => l.Claims.IRTaxClaim)); }
		}
		CalculationExhibitDocLineCollection fTaxCalculationLines;

		public CalculationExhibitDocLineCollection HMFCalculationLines
		{
			get { return fHMFCalculationLines ?? (fHMFCalculationLines = new CalculationExhibitDocLineCollection(this, l => l.Claims.HMFClaim)); }
		}
		CalculationExhibitDocLineCollection fHMFCalculationLines;

		public CalculationExhibitDocLineCollection MPFCalculationLines
		{
			get { return fMPFCalculationLines ?? (fMPFCalculationLines = new CalculationExhibitDocLineCollection(this, l => l.Claims.MPFClaim)); }
		}
		CalculationExhibitDocLineCollection fMPFCalculationLines;

		public CalculationExhibitDocLineCollection OtherFeesCalculationLines
		{
			get { return fOtherFeesCalculationLines ?? (fOtherFeesCalculationLines = new CalculationExhibitDocLineCollection(this, l => l.Claims.OtherFeesClaim)); }
		}
		CalculationExhibitDocLineCollection fOtherFeesCalculationLines;

		#endregion

		#region Drawback Summary document

		public DrawbackSummaryTariffDocLineCollection DocSummaryImportTariffCollection
		{
			get { return fDocSummaryImportTariffCollection ?? (fDocSummaryImportTariffCollection = new DrawbackSummaryTariffDocLineCollection(DistinctTrailerTariffs)); }
		}
		DrawbackSummaryTariffDocLineCollection fDocSummaryImportTariffCollection;

		public DrawbackSummaryTariffDocLineCollection DocSummaryExportTariffCollection
		{
			get { return fDocSummaryExportTariffCollection ?? (fDocSummaryExportTariffCollection = new DrawbackSummaryTariffDocLineCollection(DistinctTrailerScheduleBNumbers)); }
		}
		DrawbackSummaryTariffDocLineCollection fDocSummaryExportTariffCollection;

		public DrawbackSummaryDocLineCollection DocSummaryLineCollection
		{
			get { return fDocSummaryLineCollection ?? (fDocSummaryLineCollection = new DrawbackSummaryDocLineCollection(this)); }
		}
		DrawbackSummaryDocLineCollection fDocSummaryLineCollection;

		#region DrawbackSummaryDocLine class

		public class DrawbackSummaryDocLine : NonPersistentBusinessObject, IObsoleteValidation
		{
			public DrawbackSummaryDocLine(ZString importEntryOrCMDNumber, JobDeclarationDrawbackSupporter supporter)
				: base(supporter.Factory)
			{
				this.ImportEntryOrCMDNumber = importEntryOrCMDNumber;				
				var relatedInvoiceLines = supporter.importSectionInvoiceLines.Cast<JobComInvoiceLine>().Where(l => l.DrawbackEntryNoOrCMDNo == importEntryOrCMDNumber);
				if (relatedInvoiceLines.Any())
				{
					var randomLine = relatedInvoiceLines.First();
					this.Port = randomLine.US_DRWPort;
					this.LiqDate = randomLine.US_DRWEntryDate.IsValid ? randomLine.US_DRWEntryDate.Date : ZDate.Empty;
				}
				this._99Duty = relatedInvoiceLines.Sum(l => l._99ClaimedDuty);
				this._99MPF = relatedInvoiceLines.Sum(l => l._99ClaimedMPF);
				this._99HMF = relatedInvoiceLines.Sum(l => l._99ClaimedHMF);
				this.Tax = relatedInvoiceLines.Sum(l => l._99ClaimedTax);
				this._99OtherFees = relatedInvoiceLines.Sum(l => l._99ClaimedOtherFees);
			}

			public DrawbackSummaryDocLine(JobComInvoiceLine invoiceLine)
			{
				this.ImportEntryOrCMDNumber = invoiceLine.DrawbackEntryNoOrCMDNo;
				this.Port = invoiceLine.US_DRWPort;
				this.LiqDate = invoiceLine.US_DRWEntryDate.IsValid ? invoiceLine.US_DRWEntryDate.Date : ZDate.Empty;
				this._99Duty = invoiceLine._99ClaimedDuty;
				this._99MPF = invoiceLine._99ClaimedMPF;
				this._99HMF = invoiceLine._99ClaimedHMF;
				this.Tax = invoiceLine._99ClaimedTax;
				this._99OtherFees = invoiceLine._99ClaimedOtherFees;
			}

			public ZString ImportEntryOrCMDNumber { get; private set; }
			public ZString Port { get; private set; }
			public ZDate LiqDate { get; private set; }
			public ZDecimal _99Duty { get; private set; }
			public ZDecimal _99MPF { get; private set; }
			public ZDecimal _99HMF { get; private set; }
			public ZDecimal Tax { get; private set; }
			public ZDecimal _99OtherFees { get; private set; }
		}

		#endregion

		#region DrawbackSummaryDocLineCollection class

		public class DrawbackSummaryDocLineCollection : NonPersistentBusinessObjectCollection<DrawbackSummaryDocLine>
		{
			public DrawbackSummaryDocLineCollection(JobDeclarationDrawbackSupporter supporter)
				: base(supporter.Factory)
			{
				foreach (var entryNoOrCMDNo in (from JobComInvoiceLine line in supporter.ImportSectionInvoiceLines
												where !line.DrawbackEntryNoOrCMDNo.IsEmpty
												select line.DrawbackEntryNoOrCMDNo).Distinct())
				{
					Add(new DrawbackSummaryDocLine(entryNoOrCMDNo, supporter));
				}

				foreach (var line in (from JobComInvoiceLine line in supporter.ImportSectionInvoiceLines
									  where line.DrawbackEntryNoOrCMDNo.IsEmpty
									  select line))
				{
					Add(new DrawbackSummaryDocLine(line));
				}
			}

			protected override BusinessObject CreateNonPersistentBusinessObject()
			{
				throw new NotImplementedException();
			}
			protected override bool AllowNewCore => false;
		}

		#endregion

		#region DrawbackSummaryTariffDocLine class

		public class DrawbackSummaryTariffDocLine : NonPersistentBusinessObject, IObsoleteValidation
		{
			public DrawbackSummaryTariffDocLine(IEnumerable<ZString> hTSUSArray)
			{
				var count = hTSUSArray.Count();
				if (count > 6)
				{ HTSUS7 = hTSUSArray.ElementAt(6); }
				if (count > 5)
				{ HTSUS6 = hTSUSArray.ElementAt(5); }
				if (count > 4)
				{ HTSUS5 = hTSUSArray.ElementAt(4); }
				if (count > 3)
				{ HTSUS4 = hTSUSArray.ElementAt(3); }
				if (count > 2)
				{ HTSUS3 = hTSUSArray.ElementAt(2); }
				if (count > 1)
				{ HTSUS2 = hTSUSArray.ElementAt(1); }
				if (count > 0)
				{ HTSUS1 = hTSUSArray.ElementAt(0); }
			}

			public ZString HTSUS1 { get; private set; }
			public ZString HTSUS2 { get; private set; }
			public ZString HTSUS3 { get; private set; }
			public ZString HTSUS4 { get; private set; }
			public ZString HTSUS5 { get; private set; }
			public ZString HTSUS6 { get; private set; }
			public ZString HTSUS7 { get; private set; }
		}

		#endregion

		#region DrawbackSummaryTariffDocLineCollection class
		public class DrawbackSummaryTariffDocLineCollection : NonPersistentBusinessObjectCollection<DrawbackSummaryTariffDocLine>
		{
			public DrawbackSummaryTariffDocLineCollection(IEnumerable<ZString> tariffs)
			{
				foreach (var group in tariffs.ToGroupsOf(7))
				{
					Add(new DrawbackSummaryTariffDocLine(group));
				}
			}

			protected override BusinessObject CreateNonPersistentBusinessObject()
			{
				throw new NotImplementedException();
			}
			protected override bool AllowNewCore => false;
		}

		#endregion

		#endregion

		#region Drawback Notice of Intent

		public ZString Preparer
		{
			get { return BrokerProvider.Signatory != null ? BrokerProvider.Signatory.GS_FullName : ZString.Empty; }
		}

		public ZString Title
		{
			get
			{
				var result = ZString.Empty;

				if (jobDeclaration.IsAttorneyInFact)
				{
					result = "ATTY-IN-FACT";
				}
				else
				{
					var broker = BrokerProvider.Signatory;
					if (broker != null)
					{
						result = broker.GS_Title;
					}
				}

				return result;
			}
		}

		public ZString IsFilledAsSameConditionUnderNAFTA
		{
			get
			{
				var isFilledAsSameConditionUnderNAFTA = false;

				if (jobDeclaration.IsACEDrawback)
				{
					isFilledAsSameConditionUnderNAFTA = jobDeclaration.US_DRWSameCondNAFTA;
				}
				else if (jobDeclaration.US_EntryType == EntryTypeList.Codes.OtherDrawback)
				{
					isFilledAsSameConditionUnderNAFTA = jobDeclaration.US_DRWSection.Replace(" ", "").ToLower().Contains("3333");
				}

				return isFilledAsSameConditionUnderNAFTA ? YesNoDefaultList.Codes.Yes : string.Empty;
			}
		}

		public ZString IsFilledAsDistilledSpiritsWineBeers
		{
			get
			{
				var isFilledAsDistilledSpiritsWineBeers = false;
				if (jobDeclaration.IsACEDrawback)
				{
					isFilledAsDistilledSpiritsWineBeers = ACEDrawbackProvisionsList.Is5062(jobDeclaration.US_EntryType);
				}
				else if (jobDeclaration.US_EntryType == EntryTypeList.Codes.OtherDrawback)
				{
					isFilledAsDistilledSpiritsWineBeers = jobDeclaration.US_DRWSection.Replace(" ", "").ToLower().Contains("5062");
				}

				return isFilledAsDistilledSpiritsWineBeers ? YesNoDefaultList.Codes.Yes : string.Empty;
			}
		}

		public ZString US_DRWRejectedMerchandiseReason
		{
			get { return jobDeclaration.US_DRWRejectedMerchandiseReason; }
		}

		public DrawbackNoticeOfIntentDocLineCollection DrawbackNoticeOfIntentDocLines
		{
			get { return fDrawbackNoticeOfIntentDocLines ?? (fDrawbackNoticeOfIntentDocLines = new DrawbackNoticeOfIntentDocLineCollection(this)); }
		}
		DrawbackNoticeOfIntentDocLineCollection fDrawbackNoticeOfIntentDocLines;

		void OnNoticeOfIntentPrinted(object sender, DocumentEngineCore.DocumentSupport.DocumentPrintedEventArgs e)
		{
			if (e.MenuItem.SU_MenuName == Enterprise.Customs.Common.US.DocumentNames.DrawbackNoticeOfIntentMenuItem)
			{
				foreach (DrawbackNoticeOfIntentLineToPrint lineToPrint in jobDeclaration.LinesToPrint)
				{
					if (lineToPrint.ShouldBePrinted)
					{
						lineToPrint.InvoiceLine.US_DRWNoticeOfIntentLastPrint = ZDateTime.Now;
					}
				}
			}
		}

		public ZDateTime PrintDate
		{
			get { return ZDateTime.Now; }
		}

		public Image BrokerSignatureImage
		{
			get { return BrokerProvider.BrokerSignature; }
		}

		public ZBool IsRejectedMerchandiseDrawback
		{
			get
			{
				var isRejectedMerchandiseDrawback = false;

				if (jobDeclaration.IsACEDrawback)
				{
					isRejectedMerchandiseDrawback = ACEDrawbackProvisionsList.IsRejectedMerchandise(jobDeclaration.US_EntryType);
				}
				else
				{
					isRejectedMerchandiseDrawback = jobDeclaration.US_EntryType == EntryTypeList.Codes.RejectedMerchandiseDrawback;
				}

				return isRejectedMerchandiseDrawback;
			}
		}

		public ZBool IsUnusedMerchandiseDrawback_J1
		{
			get
			{
				var isDirectIdentification = false;

				if (jobDeclaration.IsACEDrawback)
				{
					isDirectIdentification = jobDeclaration.US_EntryType == ACEDrawbackProvisionsList.Codes._08 || jobDeclaration.US_EntryType == ACEDrawbackProvisionsList.Codes._15;
				}
				else
				{
					isDirectIdentification = jobDeclaration.US_EntryType == EntryTypeList.Codes.DirectIdentificationUnusedMerchandiseDrawback;
				}

				return isDirectIdentification;
			}
		}

		public ZBool IsUnusedMerchandiseDrawback_J2
		{
			get
			{
				var isSubstitution = false;

				if (jobDeclaration.IsACEDrawback)
				{
					isSubstitution = jobDeclaration.US_EntryType == ACEDrawbackProvisionsList.Codes._09 || jobDeclaration.US_EntryType == ACEDrawbackProvisionsList.Codes._16;
				}
				else
				{
					isSubstitution = jobDeclaration.US_EntryType == EntryTypeList.Codes.SubstitutionUnusedMerchandiseDrawback;
				}

				return isSubstitution;
			}
		}

		public ZBool IsManufacturingDrawback
		{
			get
			{
				var isDirectIdentificationManufacturingOrSubstitutionManufacturing = false;

				if (jobDeclaration.IsACEDrawback)
				{
					isDirectIdentificationManufacturingOrSubstitutionManufacturing = ACEDrawbackProvisionsList.IsDirectIdentificationManufacturing(jobDeclaration.US_EntryType) || ACEDrawbackProvisionsList.IsSubstitutionManufacturing(jobDeclaration.US_EntryType);
				}
				else
				{
					isDirectIdentificationManufacturingOrSubstitutionManufacturing = jobDeclaration.US_EntryType == EntryTypeList.Codes.DirectIdentificationManufacturingDrawback || jobDeclaration.US_EntryType == EntryTypeList.Codes.SubstitutionManufacturerDrawback;
				}

				return isDirectIdentificationManufacturingOrSubstitutionManufacturing;
			}
		}

		#region DrawbackNoticeOfIntentDocLine class

		public class DrawbackNoticeOfIntentDocLine : DrawbackDocLine
		{
			public DrawbackNoticeOfIntentDocLine(JobComInvoiceLine invoiceLine)
				: base(invoiceLine)
			{
				this.ExporterOrDestroyerAddress = invoiceLine.ExporterOrDestroyer.Address1;
				this.ExporterOrDestroyerAddressTwo = invoiceLine.ExporterOrDestroyer.Address2;
				this.ExporterOrDestroyerCity = invoiceLine.ExporterOrDestroyer.City;
				this.ExporterOrDestroyerState = invoiceLine.ExporterOrDestroyer.State;
				this.ExporterOrDestroyerZip = invoiceLine.ExporterOrDestroyer.Postcode;
				this.ExporterOrDestroyerIDNumber = OrgHeaderWrapper.GetCustomsCode(invoiceLine.ExporterOrDestroyer.Organisation, new ZString[] { OrgCusCode.USACodeTypes.EmployerIdentificationNumber, OrgCusCode.USACodeTypes.CBPAssignedNumber });
				this.LocationOfMerchandise = invoiceLine.LocationOfMerchandise.E2_CompanyNameTruncated + "\n" + invoiceLine.LocationOfMerchandise.AddressAsASingleLineWithoutCompanyName;
				this.MethodOfDestruction = invoiceLine.US_DRWMethodOfDestruction;
				this.LocationOfDestruction = invoiceLine.LocationOfDestruction.E2_CompanyNameTruncated + "\n" + invoiceLine.LocationOfDestruction.AddressAsASingleLineWithoutCompanyName;
				this.DrawbackAmount = ((ZDecimal)(invoiceLine._99ClaimedDuty + invoiceLine._99ClaimedHMF + invoiceLine._99ClaimedMPF + invoiceLine._99ClaimedTax + invoiceLine._99ClaimedOtherFees)).Round(2);
				var portOfExport = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, invoiceLine.US_DRWIntendedPortOfExport, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);
				this.IntendedPortOfExport = portOfExport != null ? (ZString)(portOfExport.ZZD_Code + " - " + portOfExport.ZZD_Description) : ZString.Empty;
				this.TENo = invoiceLine.US_DRWTENo;
				var exportingCarrier = invoiceLine.ExportingCarrier;
				ExportingCarrierName = exportingCarrier != null ? exportingCarrier.UI_Name : ZString.Empty;
			}

			public ZString ExporterOrDestroyerAddress { get; private set; }
			public ZString ExporterOrDestroyerIDNumber { get; private set; }
			public ZString LocationOfMerchandise { get; private set; }
			public ZString MethodOfDestruction { get; private set; }
			public ZString LocationOfDestruction { get; private set; }
			public ZDecimal DrawbackAmount { get; private set; }
			public ZString IntendedPortOfExport { get; private set; }
			public ZString TENo { get; private set; }
			public ZString ExportingCarrierName { get; private set; }
			public ZString ExporterOrDestroyerAddressTwo { get; private set; }
			public ZString ExporterOrDestroyerCity { get; private set; }
			public ZString ExporterOrDestroyerState { get; private set; }
			public ZString ExporterOrDestroyerZip { get; private set; }
		}

		#endregion

		#region DrawbackNoticeOfIntentDocLineCollection class

		public class DrawbackNoticeOfIntentDocLineCollection : NonPersistentBusinessObjectCollection<DrawbackNoticeOfIntentDocLine>
		{
			public DrawbackNoticeOfIntentDocLineCollection(JobDeclarationDrawbackSupporter supporter)
				: base(supporter.Factory)
			{
				foreach (DrawbackNoticeOfIntentLineToPrint lineToPrint in supporter.jobDeclaration.LinesToPrint)
				{
					if (lineToPrint.ShouldBePrinted)
					{
						Add(new DrawbackNoticeOfIntentDocLine(lineToPrint.InvoiceLine));
					}
				}
			}

			protected override BusinessObject CreateNonPersistentBusinessObject()
			{
				throw new NotImplementedException();
			}
			protected override bool AllowNewCore => false;
		}

		public ZDecimal DrawbackTotalAmount
		{
			get
			{
				var result = ZDecimal.Zero;
				foreach (DrawbackNoticeOfIntentDocLine one in DrawbackNoticeOfIntentDocLines)
				{
					result += one.DrawbackAmount;
				}
				return result;
			}
		}

		#endregion

		#region DrawbackNoticeOfIntentLineToPrintCollection class

		internal class DrawbackNoticeOfIntentLineToPrintCollection : LineToPrintCollection
		{
			public DrawbackNoticeOfIntentLineToPrintCollection(JobDeclaration declaration)
				: base(declaration)
			{
			}

			protected override void GetLinesToPrint(BaseJobDeclaration declaration)
			{
				foreach (JobComInvoiceLine invoiceLine in ((JobDeclaration)declaration).DrawbackSupporter.ExportSectionInvoiceLines)
				{
					var lineToPrint = new DrawbackNoticeOfIntentLineToPrint(invoiceLine);
					Add(lineToPrint);
				}
			}

			protected override Dictionary<string, ResourceStringData> GetColumnCaptionResourceStringDictionaryCore()
			{
				var result = new Dictionary<string, ResourceStringData>() { };
				result.Add(OrganisationColName, Res.GetData("311701EB-8993-464D-9CF8-1CD6DA27DA76", "Exporter/Destroyer"));
				result.Add(IdentifierColName, Res.GetData("20AE6CB1-C052-49C6-804D-422CE940696B", "Line No."));
				return result;
			}
		}

		#endregion

		#region DrawbackNoticeOfIntentLineToPrint class
#if DEBUG
		public
#endif
		class DrawbackNoticeOfIntentLineToPrint : LineToPrint
		{
			public DrawbackNoticeOfIntentLineToPrint(JobComInvoiceLine invoiceLine)
				: base(invoiceLine)
			{
			}
			internal JobComInvoiceLine InvoiceLine
			{
				get { return (JobComInvoiceLine)bizObj; }
			}

			public override ZString Identifier
			{
				get { return InvoiceLine.JI_LineNo.ToString(); }
			}

			public override ZDateTime LastPrintDate
			{
				get { return InvoiceLine.US_DRWNoticeOfIntentLastPrint; }
			}

			public override ZString Organisation
			{
				get
				{
					var exporterOrDestroyer = InvoiceLine.ExporterOrDestroyer.Organisation;
					return exporterOrDestroyer != null ? exporterOrDestroyer.OH_Code : ZString.Empty;
				}
			}
		}

		#endregion

		#endregion

		#region IBrokerSignatureProvider Members

		Guid IBrokerSignatureProvider.RegistryBranchPK
		{
			get { return jobDeclaration.RegistryBranchPK; }
		}

		Guid IBrokerSignatureProvider.RegistryCompanyPK
		{
			get { return jobDeclaration.RegistryCompanyPK; }
		}

		bool IBrokerSignatureProvider.HasCurrentElectronicRelease
		{
			get { return jobDeclaration.HasCurrentElectronicRelease; }
		}

		GlbStaff IBrokerSignatureProvider.CusAgent
		{
			get { return jobDeclaration.CusAgent; }
		}

		BusinessObjectFactory IBrokerSignatureProvider.Factory
		{
			get { return Factory; }
		}

		PrintBrokerSignatureProvider BrokerProvider
		{
			get { return fBrokerProvider ?? (fBrokerProvider = new PrintBrokerSignatureProvider(this)); }
		}
		PrintBrokerSignatureProvider fBrokerProvider;

		#endregion

		#region IACEDrawbackSummary Members

		BusinessObjectFactory IACEDrawbackSummary.Factory
		{
			get { return jobDeclaration.Factory; }
		}

		void IACEDrawbackSummary.AddMessage(MQEDIMessage message)
		{
			jobDeclaration.Messages.Add(message);
		}

		ZString IACEDrawbackSummary.ProcessingOfficeCode
		{
			get { return jobDeclaration.ProcessingOfficeCode; }
		}

		ZString IACEDrawbackSummary.ProcessingPort
		{
			get { return jobDeclaration.US_PreparerDistrictPort; }
		}

		ZString IACEDrawbackSummary.ActionRequestCode
		{
			get { return UpdateActionCodeConverter.ConvertToString(actionCode); }
		}

		ZString IACEDrawbackSummary.EntryFilerCode
		{
			get { return jobDeclaration.US_EntryFilerCode; }
		}

		ZString IACEDrawbackSummary.EntryNumber
		{
			get { return MQEDIMessage.USEntryNumberPlaceHolder; }
		}

		ZString IACEDrawbackSummary.ClaimPort
		{
			get { return jobDeclaration.US_ClaimPort; }
		}

		ZString IACEDrawbackSummary.BrokerReferenceNumber
		{
			get { return jobDeclaration.BrokerReferenceNumberCore; }
		}

		ZString IACEDrawbackSummary.ClaimType
		{
			get { return jobDeclaration.US_EntryType; }
		}

		ZString IACEDrawbackSummary.BondWaiverIndicator
		{
			get { return jobDeclaration.US_BondType == BondTypeList.Codes.NoBondRequired || !jobDeclaration.US_BondWaiverCode.IsEmpty ? "0" : string.Empty; }
		}

		ZString IACEDrawbackSummary.BondWaiverReasonCode
		{
			get { return jobDeclaration.US_BondWaiverCode; }
		}

		ZString IACEDrawbackSummary.AcceleratedClaimIndicator
		{
			get { return jobDeclaration.US_AcceleratedClaimInd ? "Y" : string.Empty; }
		}

		ZString IACEDrawbackSummary.OneTimeWaiverIndicator
		{
			get { return jobDeclaration.US_DRWOneTimeWaiverInd ? "Y" : string.Empty; }
		}

		ZString IACEDrawbackSummary.WavierOfPriorNoticeIndicator
		{
			get { return jobDeclaration.US_WaiverNoticeInd ? "Y" : string.Empty; }
		}

		ZString IACEDrawbackSummary.CommercialInterchangeability
		{
			get { return jobDeclaration.US_DRWCommRuling; }
		}

		ZString IACEDrawbackSummary.ElectronicPetroleumCertification
		{
			get { return jobDeclaration.US_DRWElectPetroleumCert ? "X" : string.Empty; }
		}

		ZString IACEDrawbackSummary.ElectronicManufacturingPetroleumCertification
		{
			get { return jobDeclaration.US_DRWElectManufPetroleumCert ? "X" : string.Empty; }
		}

		ZString IACEDrawbackSummary.OilSpillTaxCertification
		{
			get { return jobDeclaration.US_DRWOilSpillTaxCert ? "X" : string.Empty; }
		}

		ZString IACEDrawbackSummary.NAFTADrawbackClaimIndicator
		{
			get { return jobDeclaration.US_NAFTAClaimInd ? "X" : string.Empty; }
		}

		ZString IACEDrawbackSummary.USMCADrawbackClaimIndicator
		{
			get { return jobDeclaration.US_USMCAClaimInd ? "X" : string.Empty; }
		}

		ZString IACEDrawbackSummary.ImporterOfRecordNumber
		{
			get { return jobDeclaration.ImporterOfRecordNumber; }
		}

		ZString IACEDrawbackSummary.NotifyParty4811Number
		{
			get { return OrgHeaderWrapper.GetCustomsRelatedCode(jobDeclaration.NotifyParty, OrgMatchedCustomsRegNoType.EIN); }
		}

		ZString IACEDrawbackSummary.IntendedPort
		{
			get { return jobDeclaration.US_DRWIntendedPortOfExport; }
		}

		ZString IACEDrawbackSummary.ExaminationWitnessIndicator
		{
			get
			{
				var dRWDestructionResult = jobDeclaration.US_DRWDestructionResult;
				return dRWDestructionResult == DrawbackDestructionResultCodes.Codes.Discrepant || dRWDestructionResult == DrawbackDestructionResultCodes.Codes.NonDiscrepant ? "X" : string.Empty;
			}
		}

		ZString IACEDrawbackSummary.LocationOfDestruction
		{
			get { return jobDeclaration.US_DRWLocatOfDest; }
		}

		ZDecimal IACEDrawbackSummary.GrandTotalDuty
		{
			get { return TotalsCalculated ? grandTotalDutyAmount : ZDecimal.Zero; }
		}

		ZDecimal IACEDrawbackSummary.GrandTotalUserFee
		{
			get { return TotalsCalculated ? grandTotalUserFeeAmount : ZDecimal.Zero; }
		}

		ZDecimal IACEDrawbackSummary.GrandTotalIRTax
		{
			get { return TotalsCalculated ? grandTotalIRTaxAmount : ZDecimal.Zero; }
		}

		ZBool IACEDrawbackSummary.IsTFTEARequired
		{
			get { return jobDeclaration.IsDrawbackTFTEA; }
		}

		ZString IACEDrawbackSummary.SubstitutedUnusedWineCertification
		{
			get { return jobDeclaration.US_DRWUnUsedWine ? "X" : string.Empty; }
		}

		ZString IACEDrawbackSummary.BillOfMaterialsFormulaCertification
		{
			get { return jobDeclaration.US_DRWBillOfFormula ? "X" : string.Empty; }
		}

		ZString IACEDrawbackSummary.CertificationForValuationOfDestroyedMerchandise
		{
			get { return jobDeclaration.US_DRWDestroyedValuation ? "X" : string.Empty; }
		}

		ZString IACEDrawbackSummary.RetailSalesSubstitution
		{
			get { return jobDeclaration.USD_RetailSalesSubstitutionIndicator ? "X" : string.Empty; }
		}

		ZString IACEDrawbackSummary.SuperfundTaxCertification
		{
			get { return jobDeclaration.US_DRWSuperfundInd ? "X" : string.Empty; }
		}

		ZString IACEDrawbackSummary.ResultsOfExamination
		{
			get { return jobDeclaration.US_DRWDestructionResult == DrawbackDestructionResultCodes.Codes.Waived ? ZString.Empty : jobDeclaration.US_DRWDestructionResult; }
		}

		IEnumerable<IACEDrawbackBondInfo> IACEDrawbackSummary.BondDetails
		{
			get
			{
				var bondType = jobDeclaration.US_BondType;
				if (!bondType.IsEmpty && bondType != BondTypeList.Codes.NoBondRequired)
				{
					yield return new ACEDrawbackBondDetails(bondType, jobDeclaration.US_BondDesignationCode, jobDeclaration.US_SuretyCode, jobDeclaration.US_BondAmount, jobDeclaration.US_BondProducerAccNo);
				}
			}
		}

		IEnumerable<IACEDrawbackImportClaim> IACEDrawbackSummary.ImportsEntrySummaryDetails
		{
			get
			{
				foreach (JobComInvoiceLine invoiceLine in jobDeclaration.InvoiceLines)
				{
					if (invoiceLine.US_DRWIsForImportSection)
					{
						yield return invoiceLine;
					}
				}
			}
		}

		IEnumerable<IACEDrawbackManufactureClaim> IACEDrawbackSummary.ManufacturedArticles
		{
			get
			{
				foreach (JobComInvoiceLine invoiceLine in jobDeclaration.InvoiceLines)
				{
					if (invoiceLine.US_DRWIsForManufacturerSection)
					{
						yield return invoiceLine;
					}
				}
			}
		}

		IEnumerable<IACEDrawbackExportClaim> IACEDrawbackSummary.ExportArticles
		{
			get
			{
				foreach (JobComInvoiceLine invoiceLine in jobDeclaration.InvoiceLines)
				{
					if (invoiceLine.US_DRWIsForExportSection)
					{
						yield return invoiceLine;
					}
				}
			}
		}

		IEnumerable<IACEDrawbackNoticeOfIntent> IACEDrawbackSummary.NoticeOfIntentDetais
		{
			get
			{
				if (!jobDeclaration.US_DRWProcName.IsEmpty || !jobDeclaration.US_DRWProcBadge.IsEmpty || !jobDeclaration.US_DRWProcPhone.IsEmpty || !jobDeclaration.US_DRWProcDate.IsEmpty)
				{
					yield return new ACEDrawbackNoticeOfIntent("P", jobDeclaration.US_DRWProcName, jobDeclaration.US_DRWProcBadge, jobDeclaration.US_DRWProcPhone, jobDeclaration.US_DRWProcDate);
				}

				if (!jobDeclaration.US_DRWExamName.IsEmpty || !jobDeclaration.US_DRWExamBadge.IsEmpty || !jobDeclaration.US_DRWExamPhone.IsEmpty || !jobDeclaration.US_DRWExamDate.IsEmpty)
				{
					yield return new ACEDrawbackNoticeOfIntent("E", jobDeclaration.US_DRWExamName, jobDeclaration.US_DRWExamBadge, jobDeclaration.US_DRWExamPhone, jobDeclaration.US_DRWExamDate);
				}
			}
		}

		IEnumerable<IACEDrawbackNAFATTariff> IACEDrawbackSummary.NAFTADetails
		{
			get
			{
				foreach (JobComInvoiceLine line in jobDeclaration.InvoiceLines)
				{
					foreach (DrawbackNAFTA tariff in line.DrawbackNAFTAs)
					{
						yield return tariff;
					}
				}
			}
		}

		IEnumerable<IACEDrawbackTFTEAClaim> IACEDrawbackSummary.TFTEADetails
		{
			get
			{
				if (jobDeclaration.IsDrawbackTFTEA)
				{
					foreach (JobComInvoiceLine invoiceLine in jobDeclaration.InvoiceLines)
					{
						if (invoiceLine.US_DRWIsForExportSection)
						{
							yield return invoiceLine;
						}
					}
				}
			}
		}

		IEnumerable<IACEDrawbackRevenueTotals> IACEDrawbackSummary.RevenueTotals
		{
			get
			{
				if (jobDeclaration.TotalDutyClaimAmount > 0)
				{
					yield return new ACEDrawbackRevenueTotals(DrawbackOtherFeeTypesList.Codes.DrawbackDuty, jobDeclaration.TotalDutyClaimAmount + jobDeclaration.TotalAdjDutyClaimAmount);
				}

				if (jobDeclaration.TotalTaxClaimAmount > 0)
				{
					yield return new ACEDrawbackRevenueTotals(DrawbackOtherFeeTypesList.Codes.DrawbackTaxes, jobDeclaration.TotalTaxClaimAmount + jobDeclaration.TotalAdjTaxClaimAmount);
				}

				if (jobDeclaration.TotalHMFClaimAmount > 0)
				{
					yield return new ACEDrawbackRevenueTotals(DrawbackOtherFeeTypesList.Codes.DrawbackHMF, jobDeclaration.TotalHMFClaimAmount + jobDeclaration.TotalAdjHMFClaimAmount);
				}

				if (jobDeclaration.TotalMPFClaimAmount > 0)
				{
					yield return new ACEDrawbackRevenueTotals(DrawbackOtherFeeTypesList.Codes.DrawbackMPF, jobDeclaration.TotalMPFClaimAmount + jobDeclaration.TotalAdjMPFClaimAmount);
				}

				var shouldClaim99Percent = !jobDeclaration.Is7552;
				var otherFeeCodes = Factory.GetCachedValue<DrawbackOtherFeeTypesList>();
				foreach (var feeCode in otherFeeCodes.GetAllCodesZString())
				{
					var result = ZDecimal.Zero;
					foreach (JobComInvoiceLine invoiceLine in jobDeclaration.InvoiceLines)
					{
						if (invoiceLine.IsForImportSectionOfDrawback)
						{
							var otherFee = invoiceLine.DrawbackOtherFees[feeCode];
							if (otherFee != null)
							{
								result += shouldClaim99Percent ? otherFee._99ClaimedAmount : otherFee.ClaimedAmount;
								result += otherFee.AdjClaimAmount;
							}
						}
					}

					if (result > 0m)
					{
						yield return new ACEDrawbackRevenueTotals(feeCode, result);
					}
				}
			}
		}

		IEnumerable<IACEDrawbackTrackingNumberLine> IACEDrawbackSummary.TrackingNumberLines
		{
			get { return jobDeclaration.InvoiceLines.OfType<IACEDrawbackTrackingNumberLine>(); }
		}

		#endregion

		#region IVisualizerNoteSupporter members

		ZGuid IVisualizerNoteSupporter.PK => jobDeclaration.PK;

		ZGuid IVisualizerNoteSupporter.ChildBusinessObjectPK => ZGuid.Empty;

		string IVisualizerNoteSupporter.TableCode => JobDeclarationSchema.Constants.Prefix;

		#endregion

		#region ISourceIdentifierProvider members

		public ZGuid SourceIdentifier => jobDeclaration.PK;

		#endregion

		#region Expired and Revised Date

		public ZString CBP7553ExpDate => new RefSysConfig.Loader(Factory).GetStringValue("CBP7553ED");

		public ZString CBP7553RevDate => new RefSysConfig.Loader(Factory).GetStringValue("CBP7553RD");

		#endregion
	}
}
