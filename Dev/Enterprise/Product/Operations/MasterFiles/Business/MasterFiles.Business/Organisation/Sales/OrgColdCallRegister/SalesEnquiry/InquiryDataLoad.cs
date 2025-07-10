using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class InquiryDataLoad : DataLoad
	{
		public InquiryDataLoad()
		{
		}

		public void ImportInquiryData(string dataLocation)
		{
			ImportData(dataLocation, (NoResString)"Inquiry");
		}

		#region CSV File

		#region CSV Header

		string[] validFileHeaderColumns;
		string[] ValidFileHeaderColumns
		{
			get { return validFileHeaderColumns ?? (validFileHeaderColumns = GetNewValidFileHeaderColumns()); }
		}

		string[] GetNewValidFileHeaderColumns()
		{
			return new string[] {
							"COMPANY",
							"ADDRESS1",
							"ADDRESS2",
							"CITY",
							"STATE",
							"POSTCODE",
							"COUNTRY",
							"BUS_REG_NO",
							"CONTACT_NAME",
							"PHONE",
							"EMAIL",
							"MOBILE",
							"FAX",
							"SOURCE",
							"DETAILS",
							"SALES_REP",
							"NOTES",
							"TYPE",
							"REF_ORG_CODE",
							"REF_CONTACT_NAME",
							"LEAD_INTEREST",
							"REF_TO_ORG_CODE",
							"REF_TO_CONTACT_NAME"
			};
		}

		#endregion

		#region CSV Template

		public override string CSVTemplateHeading
		{
			get { return string.Join(",", ValidFileHeaderColumns); }
		}

		#endregion

		class CsvInquiry
		{
			public ZString Company;
			public ZString Address1;
			public ZString Address2;
			public ZString City;
			public ZString State;
			public ZString Postcode;
			public ZString PortOrCountry;
			public ZString BusRegNo;
			public ZString ContactName;
			public ZString Phone;
			public ZString Email;
			public ZString Mobile;
			public ZString Fax;
			public ZString Source;
			public ZString Details;
			public ZString SalesRep;
			public ZString Notes;
			public ZString Type;
			public ZString RefOrgCode;
			public ZString RefContactName;
			public ZString LeadInterest;
			public ZString RefToOrgCode;
			public ZString RefToContactName;
		}

		#endregion

		#region Validation

		protected override bool IsFileHeaderValid(OCsvLine line)
		{
			int numberOfValidColumns = ValidFileHeaderColumns.Length;
			if (line.FieldValues.Length != numberOfValidColumns)
			{
				return false;
			}

			for (var i = 0; i < numberOfValidColumns; i++)
			{
				if (!ValidFileHeaderColumns[i].Equals(line.FieldValues[i], StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}
			}

			return true;
		}

		#endregion

		#region Process Csv Data

		protected override void ProcessDataForThisLine(OCsvLine line)
		{
			var results = ExtractCsvInquiryData(line);
			CsvInquiry inquiryData = results.Inquiry;
			Guid transactionPk = Guid.Empty;

			if (inquiryData != null)
			{
				transactionPk = ImportCsvInquiryData(inquiryData);
			}
			else
			{
				RunCounters.RecsExcluded++;
				DisplayLogMessage(Res.GetString("3fd01b16-5295-4c45-836b-94655bd28675", "Row {0} excluded... data is inconsistent with required format - {1}", RunCounters.CurrentRow.ToString(), String.Join(" - ", results.Errors)));
			}

			UpdateAndDisplayIfRequired(transactionPk, OrgColdCallRegisterSchema.Constants.TableName);
		}

		#region Extract Csv Data

		struct ExtractCsvInquiryDataResults
		{
			public ExtractCsvInquiryDataResults(CsvInquiry inquiry, IList<string> errors)
			{
				Inquiry = inquiry;
				Errors = errors;
			}

			public readonly CsvInquiry Inquiry;
			public readonly IList<string> Errors;
		}

		ExtractCsvInquiryDataResults ExtractCsvInquiryData(OCsvLine line)
		{
			var errors = new List<string>();
			var csvInquiry = new CsvInquiry();

			int totalFieldValuesCount = line.FieldValues.Length;
			if (totalFieldValuesCount > 0)
			{
				csvInquiry.Company = TrimmedZString(line.FieldValues[0], OrgColdCallRegisterSchema.O1_CompanyName.MaxLength);
				if (csvInquiry.Company.Length == 0)
				{
					errors.Add(Res.GetString("170a566b-c731-4f8e-b399-5265799f650b", "Field No. 1 (Company Name) must contain value with a length of at least one character"));
				}
			}
			if (totalFieldValuesCount > 1)
			{ csvInquiry.Address1 = TrimmedZString(line.FieldValues[1], OrgColdCallRegisterSchema.O1_Address1.MaxLength); }
			if (totalFieldValuesCount > 2)
			{ csvInquiry.Address2 = TrimmedZString(line.FieldValues[2], OrgColdCallRegisterSchema.O1_Address2.MaxLength); }
			if (totalFieldValuesCount > 3)
			{ csvInquiry.City = TrimmedZString(line.FieldValues[3], OrgColdCallRegisterSchema.O1_City.MaxLength); }
			if (totalFieldValuesCount > 4)
			{ csvInquiry.State = TrimmedZString(line.FieldValues[4], OrgColdCallRegisterSchema.O1_State.MaxLength); }
			if (totalFieldValuesCount > 5)
			{ csvInquiry.Postcode = TrimmedZString(line.FieldValues[5], OrgColdCallRegisterSchema.O1_PostCode.MaxLength); }

			if (totalFieldValuesCount > 6)
			{
				var portOrCountry = TrimmedZString(line.FieldValues[6]);
				if (portOrCountry.Length == 2)
				{
					csvInquiry.PortOrCountry = portOrCountry;
				}
				else if (portOrCountry.Length == 5 && portOrCountry.IsLettersAndNumbersOnlyOrEmpty)
				{
					csvInquiry.PortOrCountry = portOrCountry;
				}
				else if (portOrCountry.Length > 0)
				{
					errors.Add(Res.GetString("951f5f23-147a-40c4-a9df-4a2bdae0b18f", "Field No. 7 (Country/Region) must contain a country/region code with a length of two characters or a port code with a length of five characters"));
				}
			}

			if (totalFieldValuesCount > 7)
			{ csvInquiry.BusRegNo = TrimmedZString(line.FieldValues[7], OrgColdCallRegisterSchema.O1_BusinessRegNo.MaxLength); }
			if (totalFieldValuesCount > 8)
			{
				csvInquiry.ContactName = TrimmedZString(line.FieldValues[8], OrgColdCallRegisterSchema.O1_ContactName.MaxLength);
				if (csvInquiry.ContactName.Length == 0)
				{
					errors.Add(Res.GetString("2e474af8-0580-4b01-90ff-8732293eb904", "Field No. 8 (Contact Name) must contain value with a length of at least one character"));
				}
			}
			if (totalFieldValuesCount > 9)
			{ csvInquiry.Phone = TrimmedZString(line.FieldValues[9], OrgColdCallRegisterSchema.O1_Phone.MaxLength); }
			if (totalFieldValuesCount > 10)
			{ csvInquiry.Email = TrimmedZString(line.FieldValues[10], OrgColdCallRegisterSchema.O1_Email.MaxLength); }
			if (totalFieldValuesCount > 11)
			{ csvInquiry.Mobile = TrimmedZString(line.FieldValues[11], OrgColdCallRegisterSchema.O1_Mobile.MaxLength); }
			if (totalFieldValuesCount > 12)
			{ csvInquiry.Fax = TrimmedZString(line.FieldValues[12], OrgColdCallRegisterSchema.O1_Fax.MaxLength); }
			if (totalFieldValuesCount > 13)
			{
				var source = TrimmedZString(line.FieldValues[13], OrgColdCallRegisterSchema.O1_LeadSource.MaxLength);
				if (source.IsEmpty || SourceList.ContainsCode(source))
				{
					csvInquiry.Source = source;
				}
				else
				{
					errors.Add(Res.GetString("DC6B5348-835C-4C8A-93F3-ED4D7D16E3BE", "Field No. 13 (Source) must be one of the types specified in the '{0}' registry item", OrganisationsDataRegistry.Instance.OpportunitySource.Caption));
				}
			}
			if (totalFieldValuesCount > 14)
			{ csvInquiry.Details = TrimmedZString(line.FieldValues[14], OrgColdCallRegisterSchema.O1_OpportunitySourceDetails.MaxLength); }

			if (totalFieldValuesCount > 15)
			{
				var salesRep = TrimmedZString(line.FieldValues[15]);
				if (salesRep.Length <= 3)
				{
					csvInquiry.SalesRep = salesRep;
				}
				else
				{
					errors.Add(Res.GetString("2d0474d3-626a-42fc-8ac4-1d3722c9992f", "Field No. 16 (Sales_Rep) must contain an initial with length of three or less"));
				}
			}

			if (totalFieldValuesCount > 16)
			{ csvInquiry.Notes = TrimmedZString(line.FieldValues[16], NotesMaxLength); }
			if (totalFieldValuesCount > 17)
			{
				var type = TrimmedZString(line.FieldValues[17], OrgColdCallRegisterSchema.O1_EnquiryType.MaxLength);
				if (type.IsEmpty || EnquiryTypeList.ContainsCode(type))
				{
					csvInquiry.Type = type;
				}
				else
				{
					errors.Add(Res.GetString("dd7fbbee-13ea-4cf7-81da-eefbc2d85014", "Field No. 17 (Type) must be either '{0}' or one of the types specified in the '{1}/{2}' registry item", SalesEnquiry.Codes.SalesEnquiry, OrganisationsDataRegistry.Instance.SalesEnquiryTypeList.Category, OrganisationsDataRegistry.Instance.SalesEnquiryTypeList.Caption));
				}
			}
			if (totalFieldValuesCount > 18)
			{
				var refOrgCode = TrimmedZString(line.FieldValues[18], OrgHeaderSchema.OH_Code.MaxLength);
				if (refOrgCode.IsEmpty || HasValidOrgCode(refOrgCode))
				{
					csvInquiry.RefOrgCode = refOrgCode;
				}
				else
				{
					errors.Add(Res.GetString("192B08BB-3600-4958-8EBC-389C6A399AF4", "Field No. 18 ({0}) must reference an existing Organization", "Ref_Org_Code")); // TestSpelling does not like 'Org'
				}
			}
			if (totalFieldValuesCount > 19)
			{
				var refContactName = TrimmedZString(line.FieldValues[19], OrgContactSchema.OC_ContactName.MaxLength);
				if (!refContactName.IsEmpty && csvInquiry.RefOrgCode.IsEmpty)
				{
					errors.Add(Res.GetString("CFD15CCA-29CA-4010-8DC4-6A5BC9662A61", "Field No. 18 ({0}) must reference an existing Organization before Field No. 19 (Ref_Contact_Name) can be specified", "Ref_Org_Code")); // TestSpelling does not like 'Org'
				}
				else
				{
					csvInquiry.RefContactName = refContactName;
				}
			}
			if (totalFieldValuesCount > 20)
			{
				var leadInterest = TrimmedZString(line.FieldValues[20], OrgColdCallRegisterSchema.O1_InterestLevel.MaxLength);
				if (leadInterest.IsEmpty || LeadInterestList.ContainsCode(leadInterest))
				{
					csvInquiry.LeadInterest = leadInterest;
				}
				else
				{
					errors.Add(Res.GetString("94DAE074-FBB4-4966-8231-0FF3E4BB5FCE", "Field No. 20 (Lead_Interest) must be one of the types specified in the '{0}' registry item", OrganisationsDataRegistry.Instance.SalesEnquiryLeadInterests.Caption));
				}
			}
			if (totalFieldValuesCount > 21)
			{
				var refToOrgCode = TrimmedZString(line.FieldValues[21], OrgHeaderSchema.OH_Code.MaxLength);
				if (refToOrgCode.IsEmpty || HasValidOrgCode(refToOrgCode))
				{
					csvInquiry.RefToOrgCode = refToOrgCode;
				}
				else
				{
					errors.Add(Res.GetString("51B4A45D-88C2-4D25-8E8B-9328D8CE70A7", "Field No. 21 ({0}) must reference an existing Organization", "Ref_To_Org_Code")); // TestSpelling does not like 'Org'
				}
			}
			if (totalFieldValuesCount > 22)
			{
				var refToContactName = TrimmedZString(line.FieldValues[22], OrgContactSchema.OC_ContactName.MaxLength);
				if (!refToContactName.IsEmpty && csvInquiry.RefToOrgCode.IsEmpty)
				{
					errors.Add(Res.GetString("F1865077-86F9-44C4-9475-7052567A6833", "Field No. 21 ({0}) must reference an existing Organization before Field No. 22 (Ref_To_Contact_Name) can be specified", "Ref_To_Org_Code")); // TestSpelling does not like 'Org'
				}
				else
				{
					csvInquiry.RefToContactName = refToContactName;
				}
			}

			return new ExtractCsvInquiryDataResults((errors.Count == 0) ? csvInquiry : null, errors);
		}

		CodeDescriptionPairList enquiryTypeList;
		CodeDescriptionPairList EnquiryTypeList
		{
			get { return enquiryTypeList ?? (enquiryTypeList = SalesEnquiryLookups.GetAllEnquiryTypes()); }
		}

		ReadOnlyCodeDescriptionPairList sourceList;
		ReadOnlyCodeDescriptionPairList SourceList
		{
			get { return sourceList ?? (sourceList = SalesEnquiryLookups.GetSourceList()); }
		}

		CodeDescriptionPairList leadInterestList;
		CodeDescriptionPairList LeadInterestList
		{
			get { return leadInterestList ?? (leadInterestList = SalesEnquiryLookups.GetAllLeadInterests()); }
		}

		bool HasValidOrgCode(string orgCode)
		{
			return GetOrgFromCode(orgCode) != null;
		}

		OrgHeader GetOrgFromCode(string orgCode)
		{
			return Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, orgCode));
		}

		OrgContact CreateReferralContactIfNeeded(string orgCode, string contactName)
		{
			if (string.IsNullOrEmpty(contactName))
			{
				return null;
			}
			var referralOrg = GetOrgFromCode(orgCode);
			var referralContact = FindMatchingContact(referralOrg, contactName);
			if (referralOrg != null && referralContact == null)
			{
				referralContact = referralOrg.Contacts.AddNew();
				referralContact.OC_ContactName = contactName;
			}
			return referralContact;
		}

		OrgContact FindMatchingContact(OrgHeader org, string contactName)
		{
			OrgContact result = null;

			if (org != null)
			{
				var query = new ZQuery(OrgContactSchema.OC_ContactName, contactName);
				var matchingContacts = org.Contacts.Find(query);
				if (matchingContacts.Length > 0)
				{
					result = (OrgContact)matchingContacts[0];
				}
			}

			return result;
		}

		#endregion

		#region Import Csv Data

		Guid ImportCsvInquiryData(CsvInquiry inquiryData)
		{
			Guid transactionPk = Guid.Empty;
			try
			{
				var newInquiry = Factory.New<SalesEnquiry>();
				ImportCsvInquiryDataIntoInquiry(newInquiry, inquiryData);
				transactionPk = newInquiry.PK.ToGuid();

				RunCounters.RecsToUpdate++;
				RunCounters.RecsCreated++;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				RunCounters.RecsExcluded++;
				DisplayFormattedLogMessage(inquiryData.Company, ex.Message);
			}

			return transactionPk;
		}

		void ImportCsvInquiryDataIntoInquiry(SalesEnquiry inquiry, CsvInquiry inquiryData)
		{
			inquiry.O1_LeadStatus = SalesEnquiryStatusCodeList.Codes.Open;
			inquiry.O1_LeadCalledDate = ZDateTime.UtcNow;

			inquiry.O1_CompanyName = Env.Registry.OrgAllowMixedCase ? inquiryData.Company : inquiryData.Company.ToUpper();
			inquiry.O1_Address1 = inquiryData.Address1;
			inquiry.O1_Address2 = inquiryData.Address2;
			inquiry.O1_City = inquiryData.City;
			inquiry.O1_State = inquiryData.State;
			inquiry.O1_PostCode = inquiryData.Postcode;
			inquiry.O1_PortOrCountry = inquiryData.PortOrCountry;
			inquiry.O1_BusinessRegNo = inquiryData.BusRegNo;
			inquiry.O1_ContactName = inquiryData.ContactName;
			inquiry.O1_Phone = inquiryData.Phone;
			inquiry.O1_Email = inquiryData.Email;
			inquiry.O1_Mobile = inquiryData.Mobile;
			inquiry.O1_Fax = inquiryData.Fax;
			inquiry.O1_LeadSource = inquiryData.Source;
			inquiry.O1_OpportunitySourceDetails = inquiryData.Details;
			inquiry.O1_GS_NKRepAssigned = inquiryData.SalesRep;
			inquiry.O1_EnquiryType = inquiryData.Type;

			var referringOrg = GetOrgFromCode(inquiryData.RefOrgCode);
			var referringContact = CreateReferralContactIfNeeded(inquiryData.RefOrgCode, inquiryData.RefContactName);
			var referToOrg = GetOrgFromCode(inquiryData.RefToOrgCode);
			var referToContact = CreateReferralContactIfNeeded(inquiryData.RefToOrgCode, inquiryData.RefToContactName);

			inquiry.O1_OH_SourceOfLead = referringOrg != null ? referringOrg.PK : ZGuid.Empty;
			inquiry.O1_OC_ReferringContact = referringContact != null ? referringContact.PK : ZGuid.Empty;
			inquiry.O1_InterestLevel = inquiryData.LeadInterest;
			inquiry.O1_OH_ReferTo = referToOrg != null ? referToOrg.PK : ZGuid.Empty;
			inquiry.O1_OC_ReferToContact = referToContact != null ? referToContact.PK : ZGuid.Empty;
			LoadNotes(inquiry, inquiryData.Notes);
		}

		ZString TrimmedZString(string value)
		{
			return new ZString(value).Trim();
		}

		ZString TrimmedZString(string value, int maxLength)
		{
			return TrimmedZString(value).SubstringSafe(0, maxLength);
		}

		void LoadNotes(SalesEnquiry inquiry, ZString notes)
		{
			if (!notes.IsEmpty)
			{
				inquiry.EnquiryNotesContent = ZBlob.FromUTF8(notes);
			}
		}

		internal const int NotesMaxLength = 100000000;

		#endregion

		#endregion

		#region Notifications

		void DisplayFormattedLogMessage(ZString company, string detailedExceptionMessage)
		{
			var ouputRowNo = Res.GetString("635c087e-ee88-4bae-93b4-80d5a2310ac3", "Line {0}:", RunCounters.CurrentRow.ToString());
			var logMessage = Res.GetString("48e013c8-b9ea-498b-b556-1f1dfacce965", "{0} Company: {1}  {2}", ouputRowNo, company, detailedExceptionMessage);

			DisplayLogMessage(logMessage);
		}

		#endregion

	}
}
