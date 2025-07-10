using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(InquiryDataLoad))]
	sealed class InquiryDataLoadTest : DataLoadTestCase<InquiryDataLoad>
	{
		public void TestNotesMaxLength()
		{
			const int maxBytesForStmNoteData = 2147483646;
			const int maxNumberOfBytesPerUtf8Char = 4;

			var theoreticalMaximum = maxBytesForStmNoteData / maxNumberOfBytesPerUtf8Char;  // won't ever reach this because of out-of-memory exceptions
			Assert(string.Format("InquiryDataLoad.MaxNotesLength should be less than {0}", theoreticalMaximum), theoreticalMaximum > InquiryDataLoad.NotesMaxLength);
		}

		[TestUtcOffset(10, 0, 0)]
		[TestDate(2012, 2, 12)]
		public void TestImportInquiryData()
		{
			var referringOrg = Factory.NewWithValidTestData<OrgHeader>();
			var referToOrg = Factory.NewWithValidTestData<OrgHeader>();
			var referringContact = referringOrg.Contacts.AddNew();
			var referToContact = referToOrg.Contacts.AddNew();
			referringOrg.OH_Code = "ORGCODE12345";
			referToOrg.OH_Code = "ORGCODE56789";
			referringContact.OC_ContactName = "Jenny";
			referToContact.OC_ContactName = "Tom";
			Factory.Save();

			var sources = new CodeDescriptionBoolRelatedItemCollection();
			sources.Add("TMK", null, true);
			OrganisationsDataRegistry.Instance.OpportunitySource.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, sources);

			var leadInterests = new CodeDescriptionBoolCollection();
			leadInterests.Add("HOT", null, true);
			OrganisationsDataRegistry.Instance.SalesEnquiryLeadInterests.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, leadInterests);

			using (var testFile = TempFile.New())
			{
				using (var streamWriter = new StreamWriter(testFile.Filename))
				{
					streamWriter.WriteLine(GetValidHeaderLine());
					streamWriter.WriteLine("TESTCOMPANYNAME1,3A Pound St,,WEST IPSWICH,QLD,4305,AUSYD,,Myrene,0738128363,,,,,,,-Transport Storage,INQ,ORGCODE12345,Jenny,,ORGCODE56789,Tom");
					streamWriter.WriteLine("TESTCOMPANYNAME2,Locked Bag 67,Unlock Key 76,WETHERILL PARK DC,NSW,1851,AU,23112936991,Stephen,02 9513 0300,sbrown@1stfleet.com.au,0412345678,02 9756 5370,TMK,Purchased List,F.L,Business Services|Road Freight|Storage|Transport,WEB,,,HOT,ORGCODE12345,");
					streamWriter.WriteLine("TESTCOMPANYNAME3,,,,,,,,Edward,,,,,,,,,,,,,,");
					streamWriter.Flush();
				}

				var inquiryDataLoad = new InquiryDataLoad();
				inquiryDataLoad.ImportInquiryData(testFile.Filename);

				AssertEquals(4, inquiryDataLoad.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(3, inquiryDataLoad.RunCounters.RecsCreated);
				AssertEquals(0, inquiryDataLoad.RunCounters.RecsUpdated);
				AssertEquals(0, inquiryDataLoad.RunCounters.RecsExcluded);
				AssertEquals(2, inquiryDataLoad.Log.Count);

				var testInquiriesFilter = new ZQuery(OrgColdCallRegisterSchema.O1_CompanyName, SQLComparisonOperator.StartsWith, "TESTCOMPANYNAME");
				testInquiriesFilter.OrderBy = OrgColdCallRegisterSchema.O1_CompanyName.Name;
				var inquiriesImported = Factory.Load<SalesEnquiry>(testInquiriesFilter);
				AssertEquals("There should have been 3 inquiries imported", 3, inquiriesImported.Length);

				var inquiry1 = inquiriesImported[0];
				AssertEquals("OPN", inquiry1.O1_LeadStatus);
				AssertEquals(new ZDateTime(2012, 2, 12), inquiry1.O1_LeadCalledDate);
				AssertEquals("TESTCOMPANYNAME1", inquiry1.O1_CompanyName);
				AssertEquals("3A Pound St", inquiry1.O1_Address1);
				AssertEquals("", inquiry1.O1_Address2);
				AssertEquals("WEST IPSWICH", inquiry1.O1_City);
				AssertEquals("QLD", inquiry1.O1_State);
				AssertEquals("4305", inquiry1.O1_PostCode);
				AssertEquals("AUSYD", inquiry1.O1_PortOrCountry);
				AssertEquals("", inquiry1.O1_BusinessRegNo);
				AssertEquals("Myrene", inquiry1.O1_ContactName);
				AssertEquals("0738128363", inquiry1.O1_Phone);
				AssertEquals("", inquiry1.O1_Email);
				AssertEquals("", inquiry1.O1_Mobile);
				AssertEquals("", inquiry1.O1_Fax);
				AssertEquals("", inquiry1.O1_LeadSource);
				AssertEquals("", inquiry1.O1_OpportunitySourceDetails);
				AssertEquals("", inquiry1.O1_GS_NKRepAssigned);
				AssertEquals(ORtfTextUtil.TextToRtf("-Transport Storage"), inquiry1.EnquiryNotesContent.ToUTF8());
				AssertEquals("INQ", inquiry1.O1_EnquiryType);
				AssertEquals(referringOrg.PK, inquiry1.O1_OH_SourceOfLead);
				AssertEquals(referringContact.PK, inquiry1.O1_OC_ReferringContact);
				AssertEquals("", inquiry1.O1_InterestLevel);
				AssertEquals(referToOrg.PK, inquiry1.O1_OH_ReferTo);
				AssertEquals(referToContact.PK, inquiry1.O1_OC_ReferToContact);

				var inquiry2 = inquiriesImported[1];
				AssertEquals("OPN", inquiry2.O1_LeadStatus);
				AssertEquals(new ZDateTime(2012, 2, 12), inquiry2.O1_LeadCalledDate);
				AssertEquals("TESTCOMPANYNAME2", inquiry2.O1_CompanyName);
				AssertEquals("Locked Bag 67", inquiry2.O1_Address1);
				AssertEquals("Unlock Key 76", inquiry2.O1_Address2);
				AssertEquals("WETHERILL PARK DC", inquiry2.O1_City);
				AssertEquals("NSW", inquiry2.O1_State);
				AssertEquals("1851", inquiry2.O1_PostCode);
				AssertEquals("AU", inquiry2.O1_PortOrCountry);
				AssertEquals("23112936991", inquiry2.O1_BusinessRegNo);
				AssertEquals("Stephen", inquiry2.O1_ContactName);
				AssertEquals("02 9513 0300", inquiry2.O1_Phone);
				AssertEquals("sbrown@1stfleet.com.au", inquiry2.O1_Email);
				AssertEquals("0412345678", inquiry2.O1_Mobile);
				AssertEquals("02 9756 5370", inquiry2.O1_Fax);
				AssertEquals("TMK", inquiry2.O1_LeadSource);
				AssertEquals("Purchased List", inquiry2.O1_OpportunitySourceDetails);
				AssertEquals("F.L", inquiry2.O1_GS_NKRepAssigned);
				AssertEquals(ORtfTextUtil.TextToRtf("Business Services|Road Freight|Storage|Transport"), inquiry2.EnquiryNotesContent.ToUTF8());
				AssertEquals("WEB", inquiry2.O1_EnquiryType);
				AssertEquals(ZGuid.Empty, inquiry2.O1_OH_SourceOfLead);
				AssertEquals(ZGuid.Empty, inquiry2.O1_OC_ReferringContact);
				AssertEquals("HOT", inquiry2.O1_InterestLevel);
				AssertEquals(referringOrg.PK, inquiry2.O1_OH_ReferTo);
				AssertEquals(ZGuid.Empty, inquiry2.O1_OC_ReferToContact);

				var inquiry3 = inquiriesImported[2];
				AssertEquals("OPN", inquiry3.O1_LeadStatus);
				AssertEquals(new ZDateTime(2012, 2, 12), inquiry3.O1_LeadCalledDate);
				AssertEquals("TESTCOMPANYNAME3", inquiry3.O1_CompanyName);
				AssertEquals("", inquiry3.O1_Address1);
				AssertEquals("", inquiry3.O1_Address2);
				AssertEquals("", inquiry3.O1_City);
				AssertEquals("", inquiry3.O1_State);
				AssertEquals("", inquiry3.O1_PostCode);
				AssertEquals("", inquiry3.O1_PortOrCountry);
				AssertEquals("", inquiry3.O1_BusinessRegNo);
				AssertEquals("Edward", inquiry3.O1_ContactName);
				AssertEquals("", inquiry3.O1_Phone);
				AssertEquals("", inquiry3.O1_Email);
				AssertEquals("", inquiry3.O1_Mobile);
				AssertEquals("", inquiry3.O1_Fax);
				AssertEquals("", inquiry3.O1_LeadSource);
				AssertEquals("", inquiry3.O1_OpportunitySourceDetails);
				AssertEquals("", inquiry3.O1_GS_NKRepAssigned);
				AssertEquals(ZBlob.FromUTF8(ORtfTextUtil.TextToRtf(ZBlob.Empty.ToUTF8())), inquiry3.EnquiryNotesContent);
				AssertEquals("", inquiry3.O1_EnquiryType);
				AssertEquals(ZGuid.Empty, inquiry3.O1_OH_SourceOfLead);
				AssertEquals(ZGuid.Empty, inquiry3.O1_OC_ReferringContact);
				AssertEquals("", inquiry3.O1_InterestLevel);
				AssertEquals(ZGuid.Empty, inquiry3.O1_OH_ReferTo);
				AssertEquals(ZGuid.Empty, inquiry3.O1_OC_ReferToContact);
			}
		}

		public void TestImportInquiryData_WithInvalidHeader()
		{
			using (var testFile = TempFile.New())
			{
				using (var streamWriter = new StreamWriter(testFile.Filename))
				{
					streamWriter.WriteLine("My,Invalid,Header");
					streamWriter.WriteLine("TESTCOMPANYNAME,,");
					streamWriter.Flush();
				}

				var inquiryDataLoad = new InquiryDataLoad();
				inquiryDataLoad.ImportInquiryData(testFile.Filename);

				AssertEquals(2, inquiryDataLoad.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(0, inquiryDataLoad.RunCounters.RecsCreated);
				AssertEquals(0, inquiryDataLoad.RunCounters.RecsUpdated);
				AssertEquals(0, inquiryDataLoad.RunCounters.RecsExcluded);
				AssertEquals(3, inquiryDataLoad.Log.Count);
				AssertEquals("FileHeaderIsValid", false, inquiryDataLoad.FileHeaderIsValid);

				var testCompanyQuery = new ZQuery(OrgColdCallRegisterSchema.O1_CompanyName, "TESTCOMPANYNAME");
				AssertEquals("There should be no inquiries imported", 0, Factory.Load<SalesEnquiry>(testCompanyQuery).Length);
			}
		}

		public void TestImportInquiryData_WithInvalidData()
		{
			using (var testFile = TempFile.New())
			{
				using (var streamWriter = new StreamWriter(testFile.Filename))
				{
					streamWriter.WriteLine(GetValidHeaderLine());
					streamWriter.WriteLine("TESTCOMPANYNAME1,,,,,,Australia,,Edward,,,,,,,,");
					streamWriter.WriteLine("TESTCOMPANYNAME2,Locked Bag 67,Unlock Key 76,WETHERILL PARK DC,NSW,1851,AU,23112936991,Stephen,02 9513 0300,sbrown@1stfleet.com.au,0412345678,02 9756 5370,TMK,Purchased List,F.L,Business Services|Road Freight|Storage|Transport");
					streamWriter.WriteLine("TESTCOMPANYNAME3,,,,,,Australia,,Edward,,,,,,,Andrew,");
					streamWriter.Flush();
				}

				var inquiryDataLoad = new InquiryDataLoad();
				inquiryDataLoad.ImportInquiryData(testFile.Filename);

				AssertEquals(4, inquiryDataLoad.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(1, inquiryDataLoad.RunCounters.RecsCreated);
				AssertEquals(0, inquiryDataLoad.RunCounters.RecsUpdated);
				AssertEquals(2, inquiryDataLoad.RunCounters.RecsExcluded);
				AssertEquals(4, inquiryDataLoad.Log.Count);

				var testCompanyName2Query = new ZQuery(OrgColdCallRegisterSchema.O1_CompanyName, "TESTCOMPANYNAME2");
				AssertEquals("Only TESTCOMPANYNAME2 should have been imported", 1, Factory.Load<SalesEnquiry>(testCompanyName2Query).Length);

				var testCompanyName3Query = new ZQuery(OrgColdCallRegisterSchema.O1_CompanyName, "TESTCOMPANYNAME3");
				AssertEquals("TESTCOMPANYNAME3 should not be imported", 0, Factory.Load<SalesEnquiry>(testCompanyName3Query).Length);
				AssertCollectionContains("Row 4 excluded... data is inconsistent with required format - Field No. 7 (Country/Region) must contain a country/region code with a length of two characters or a port code with a length of five characters - Field No. 16 (Sales_Rep) must contain an initial with length of three or less", inquiryDataLoad.Log);
			}
		}

		public void TestImportInquiryData_WithInvalidPortOrCountry()
		{
			using (var testFile = TempFile.New())
			{
				using (var streamWriter = new StreamWriter(testFile.Filename))
				{
					streamWriter.WriteLine(GetValidHeaderLine());
					streamWriter.WriteLine("TESTCOMPANYNAME,,,,,,Australia,,Edward,,,,,,,,");
					streamWriter.Flush();
				}

				var inquiryDataLoad = new InquiryDataLoad();
				inquiryDataLoad.ImportInquiryData(testFile.Filename);

				AssertEquals(2, inquiryDataLoad.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(0, inquiryDataLoad.RunCounters.RecsCreated);
				AssertEquals(0, inquiryDataLoad.RunCounters.RecsUpdated);
				AssertEquals(1, inquiryDataLoad.RunCounters.RecsExcluded);
				AssertEquals(3, inquiryDataLoad.Log.Count);

				var testCompanyNameQuery = new ZQuery(OrgColdCallRegisterSchema.O1_CompanyName, "TESTCOMPANYNAME");
				AssertEquals("TESTCOMPANYNAME should not be imported", 0, Factory.Load<SalesEnquiry>(testCompanyNameQuery).Length);
				AssertCollectionContains("Row 2 excluded... data is inconsistent with required format - Field No. 7 (Country/Region) must contain a country/region code with a length of two characters or a port code with a length of five characters", inquiryDataLoad.Log);
			}
		}

		public void TestImportInquiryData_WithInvalidSalesRep()
		{
			using (var testFile = TempFile.New())
			{
				using (var streamWriter = new StreamWriter(testFile.Filename))
				{
					streamWriter.WriteLine(GetValidHeaderLine());
					streamWriter.WriteLine("TESTCOMPANYNAME,,,,,,,,Edward,,,,,,,Andrew,");
					streamWriter.Flush();
				}

				var inquiryDataLoad = new InquiryDataLoad();
				inquiryDataLoad.ImportInquiryData(testFile.Filename);

				AssertEquals(2, inquiryDataLoad.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(0, inquiryDataLoad.RunCounters.RecsCreated);
				AssertEquals(0, inquiryDataLoad.RunCounters.RecsUpdated);
				AssertEquals(1, inquiryDataLoad.RunCounters.RecsExcluded);
				AssertEquals(3, inquiryDataLoad.Log.Count);

				var testCompanyNameQuery = new ZQuery(OrgColdCallRegisterSchema.O1_CompanyName, "TESTCOMPANYNAME");
				AssertEquals("TESTCOMPANYNAME should not be imported", 0, Factory.Load<SalesEnquiry>(testCompanyNameQuery).Length);
				AssertCollectionContains("Row 2 excluded... data is inconsistent with required format - Field No. 16 (Sales_Rep) must contain an initial with length of three or less", inquiryDataLoad.Log);
			}
		}

		public void TestImportInquiryData_WithInvalidType()
		{
			using (var testFile = TempFile.New())
			{
				using (var streamWriter = new StreamWriter(testFile.Filename))
				{
					streamWriter.WriteLine(GetValidHeaderLine());
					streamWriter.WriteLine("TESTCOMPANYNAME,,,,,,,,Edward,,,,,,,,,ZZZ");
					streamWriter.Flush();
				}

				var inquiryDataLoad = new InquiryDataLoad();
				inquiryDataLoad.ImportInquiryData(testFile.Filename);

				AssertEquals(2, inquiryDataLoad.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(0, inquiryDataLoad.RunCounters.RecsCreated);
				AssertEquals(0, inquiryDataLoad.RunCounters.RecsUpdated);
				AssertEquals(1, inquiryDataLoad.RunCounters.RecsExcluded);
				AssertEquals(3, inquiryDataLoad.Log.Count);

				var testCompanyNameQuery = new ZQuery(OrgColdCallRegisterSchema.O1_CompanyName, "TESTCOMPANYNAME");
				AssertEquals("TESTCOMPANYNAME should not be imported", 0, Factory.Load<SalesEnquiry>(testCompanyNameQuery).Length);
				AssertCollectionContains("Row 2 excluded... data is inconsistent with required format - Field No. 17 (Type) must be either 'INQ' or one of the types specified in the 'Sales & Marketing/Inquiry Manager/Type' registry item", inquiryDataLoad.Log);
			}
		}

		public void TestImportInquiryData_WithInvalidReferringOrgCode()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORGCODE12345";
			Factory.Save();

			using (var testFile = TempFile.New())
			{
				using (var streamWriter = new StreamWriter(testFile.Filename))
				{
					streamWriter.WriteLine(GetValidHeaderLine());
					streamWriter.WriteLine("TESTCOMPANYNAME1,,,,,,,,Edward,,,,,,,,,,ORGCODE12345,,");
					streamWriter.WriteLine("TESTCOMPANYNAME2,,,,,,,,Edward,,,,,,,,,,ABCDEFGHI123,,");
					streamWriter.Flush();
				}

				var inquiryDataLoad = new InquiryDataLoad();
				inquiryDataLoad.ImportInquiryData(testFile.Filename);

				AssertEquals(3, inquiryDataLoad.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(1, inquiryDataLoad.RunCounters.RecsCreated);
				AssertEquals(0, inquiryDataLoad.RunCounters.RecsUpdated);
				AssertEquals(1, inquiryDataLoad.RunCounters.RecsExcluded);
				AssertEquals("2 default messages + message(s) of excluded rows", 3, inquiryDataLoad.Log.Count);

				var testCompanyName1Query = new ZQuery(OrgColdCallRegisterSchema.O1_CompanyName, "TESTCOMPANYNAME1");
				AssertEquals("Only TESTCOMPANYNAME1 should have been imported", 1, Factory.Load<SalesEnquiry>(testCompanyName1Query).Length);

				var testCompanyName2Query = new ZQuery(OrgColdCallRegisterSchema.O1_CompanyName, "TESTCOMPANYNAME2");
				AssertEquals("TESTCOMPANYNAME2 should not be imported", 0, Factory.Load<SalesEnquiry>(testCompanyName2Query).Length);
				AssertCollectionContains("Row 3 excluded... data is inconsistent with required format - Field No. 18 (Ref_Org_Code) must reference an existing Organization", inquiryDataLoad.Log);
			}
		}

		public void TestImportInquiryData_WithInvalidReferringContactName()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "ORGCODE12345";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "ORGCODE54321";
			var contact = org2.Contacts.AddNew();
			contact.OC_ContactName = "Apple";

			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_Code = "RANDOMORG123";
			Factory.Save();

			using (var testFile = TempFile.New())
			{
				using (var streamWriter = new StreamWriter(testFile.Filename))
				{
					streamWriter.WriteLine(GetValidHeaderLine());
					streamWriter.WriteLine("TESTCOMPANYNAME1,,,,,,,,Edward,,,,,,,,,,,HELLO,");
					streamWriter.WriteLine("TESTCOMPANYNAME2,,,,,,,,Edward,,,,,,,,,,ABCDEFGHI123,Banana,");
					streamWriter.WriteLine("TESTCOMPANYNAME3,,,,,,,,Edward,,,,,,,,,,ORGCODE12345,Banana,");
					streamWriter.WriteLine("TESTCOMPANYNAME4,,,,,,,,Edward,,,,,,,,,,ORGCODE54321,Apple,");
					streamWriter.WriteLine("TESTCOMPANYNAME5,,,,,,,,Edward,,,,,,,,,,RANDOMORG123,Pear,NOT");
					streamWriter.Flush();
				}

				var inquiryDataLoad = new InquiryDataLoad();
				inquiryDataLoad.ImportInquiryData(testFile.Filename);

				AssertEquals(6, inquiryDataLoad.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(2, inquiryDataLoad.RunCounters.RecsCreated);
				AssertEquals(0, inquiryDataLoad.RunCounters.RecsUpdated);
				AssertEquals(3, inquiryDataLoad.RunCounters.RecsExcluded);
				AssertEquals("2 default messages + message(s) of excluded rows", 5, inquiryDataLoad.Log.Count);

				var testCompanyName1Query = new ZQuery(OrgColdCallRegisterSchema.O1_CompanyName, "TESTCOMPANYNAME1");
				AssertEquals("TESTCOMPANYNAME1 should not be imported", 0, Factory.Load<SalesEnquiry>(testCompanyName1Query).Length);
				AssertCollectionContains("Row 2 excluded... data is inconsistent with required format - Field No. 18 (Ref_Org_Code) must reference an existing Organization before Field No. 19 (Ref_Contact_Name) can be specified", inquiryDataLoad.Log);

				var testCompanyName2Query = new ZQuery(OrgColdCallRegisterSchema.O1_CompanyName, "TESTCOMPANYNAME2");
				AssertEquals("TESTCOMPANYNAME2 should not be imported", 0, Factory.Load<SalesEnquiry>(testCompanyName2Query).Length);
				AssertCollectionContains("Row 3 excluded... data is inconsistent with required format - Field No. 18 (Ref_Org_Code) must reference an existing Organization - Field No. 18 (Ref_Org_Code) must reference an existing Organization before Field No. 19 (Ref_Contact_Name) can be specified", inquiryDataLoad.Log);

				var testCompanyName3Query = new ZQuery(OrgColdCallRegisterSchema.O1_CompanyName, "TESTCOMPANYNAME3");
				AssertEquals("TESTCOMPANYNAME3 should have been imported", 1, Factory.Load<SalesEnquiry>(testCompanyName3Query).Length);

				var testCompanyName4Query = new ZQuery(OrgColdCallRegisterSchema.O1_CompanyName, "TESTCOMPANYNAME4");
				AssertEquals("TESTCOMPANYNAME4 should have been imported", 1, Factory.Load<SalesEnquiry>(testCompanyName4Query).Length);

				var testCompanyName5Query = new ZQuery(OrgColdCallRegisterSchema.O1_CompanyName, "TESTCOMPANYNAME5");
				AssertEquals("TESTCOMPANYNAME5 should not be imported", 0, Factory.Load<SalesEnquiry>(testCompanyName5Query).Length);

				AssertEquals("Org1 contact has been created", 1, org1.Contacts.Count);
				AssertEquals("Org1 contact has been created - name is correct", "Banana", org1.Contacts[0].OC_ContactName);

				AssertEquals("Org2 contact did not get redundantly created", 1, org2.Contacts.Count);

				AssertEquals("Org3 contact should not be created", 0, org3.Contacts.Count);
			}
		}

		public void TestImportInquiryData_WithNewReferringContactName()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORGCODE12345";
			Factory.Save();

			using (var testFile = TempFile.New())
			{
				using (var streamWriter = new StreamWriter(testFile.Filename))
				{
					streamWriter.WriteLine(GetValidHeaderLine());
					streamWriter.WriteLine("TESTCOMPANYNAME,,,,,,,,Edward,,,,,,,,,,ORGCODE12345,New Contact,");
					streamWriter.Flush();
				}

				var inquiryDataLoad = new InquiryDataLoad();
				inquiryDataLoad.ImportInquiryData(testFile.Filename);

				AssertEquals(2, inquiryDataLoad.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(1, inquiryDataLoad.RunCounters.RecsCreated);
				AssertEquals(0, inquiryDataLoad.RunCounters.RecsUpdated);
				AssertEquals(0, inquiryDataLoad.RunCounters.RecsExcluded);
				AssertEquals("2 default messages + message(s) of excluded rows", 2, inquiryDataLoad.Log.Count);

				SalesEnquiry enquiry = Factory.LoadTop1<SalesEnquiry>(new ZQuery(OrgColdCallRegisterSchema.O1_CompanyName, "TESTCOMPANYNAME"));
				AssertNotNull("TESTCOMPANYNAME should be imported", enquiry);
				AssertNotEquals("Contact is linked to enquiry", ZGuid.Empty, enquiry.O1_OC_ReferringContact);
				AssertEquals("Correct contact is linked to enquiry", "New Contact", Factory.Load<OrgContact>(enquiry.O1_OC_ReferringContact).OC_ContactName);
			}
		}

		public void TestImportInquiryData_WithInvalidReferToOrgCode()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORGCODE12345";
			Factory.Save();

			using (var testFile = TempFile.New())
			{
				using (var streamWriter = new StreamWriter(testFile.Filename))
				{
					streamWriter.WriteLine(GetValidHeaderLine());
					streamWriter.WriteLine("TESTCOMPANYNAME1,,,,,,,,Edward,,,,,,,,,,,,,ORGCODE12345,");
					streamWriter.WriteLine("TESTCOMPANYNAME2,,,,,,,,Edward,,,,,,,,,,,,,ABCDEFGHI123,");
					streamWriter.Flush();
				}

				var inquiryDataLoad = new InquiryDataLoad();
				inquiryDataLoad.ImportInquiryData(testFile.Filename);

				AssertEquals(3, inquiryDataLoad.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(1, inquiryDataLoad.RunCounters.RecsCreated);
				AssertEquals(0, inquiryDataLoad.RunCounters.RecsUpdated);
				AssertEquals(1, inquiryDataLoad.RunCounters.RecsExcluded);
				AssertEquals("2 default messages + message(s) of excluded rows", 3, inquiryDataLoad.Log.Count);

				var testCompanyName1Query = new ZQuery(OrgColdCallRegisterSchema.O1_CompanyName, "TESTCOMPANYNAME1");
				AssertEquals("Only TESTCOMPANYNAME1 should have been imported", 1, Factory.Load<SalesEnquiry>(testCompanyName1Query).Length);

				var testCompanyName2Query = new ZQuery(OrgColdCallRegisterSchema.O1_CompanyName, "TESTCOMPANYNAME2");
				AssertEquals("TESTCOMPANYNAME2 should not be imported", 0, Factory.Load<SalesEnquiry>(testCompanyName2Query).Length);
				AssertCollectionContains("Row 3 excluded... data is inconsistent with required format - Field No. 21 (Ref_To_Org_Code) must reference an existing Organization", inquiryDataLoad.Log);
			}
		}

		public void TestImportInquiryData_WithInvalidReferToContactName()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "ORGCODE12345";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "ORGCODE54321";
			var contact = org2.Contacts.AddNew();
			contact.OC_ContactName = "Apple";

			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_Code = "RANDOMORG123";
			Factory.Save();

			using (var testFile = TempFile.New())
			{
				using (var streamWriter = new StreamWriter(testFile.Filename))
				{
					streamWriter.WriteLine(GetValidHeaderLine());
					streamWriter.WriteLine("TESTCOMPANYNAME1,,,,,,,,Edward,,,,,,,,,,,,,,HELLO");
					streamWriter.WriteLine("TESTCOMPANYNAME2,,,,,,,,Edward,,,,,,,,,,,,,ABCDEFGHI123,Banana");
					streamWriter.WriteLine("TESTCOMPANYNAME3,,,,,,,,Edward,,,,,,,,,,,,,ORGCODE12345,Banana");
					streamWriter.WriteLine("TESTCOMPANYNAME4,,,,,,,,Edward,,,,,,,,,,,,,ORGCODE54321,Apple");
					streamWriter.WriteLine("TESTCOMPANYNAME5,,,,,,,,Edward,,,,,,,,,,,,NOT,RANDOMORG123,Pear");
					streamWriter.Flush();
				}

				var inquiryDataLoad = new InquiryDataLoad();
				inquiryDataLoad.ImportInquiryData(testFile.Filename);

				AssertEquals(6, inquiryDataLoad.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(2, inquiryDataLoad.RunCounters.RecsCreated);
				AssertEquals(0, inquiryDataLoad.RunCounters.RecsUpdated);
				AssertEquals(3, inquiryDataLoad.RunCounters.RecsExcluded);
				AssertEquals("2 default messages + message(s) of excluded rows", 5, inquiryDataLoad.Log.Count);

				var testCompanyName1Query = new ZQuery(OrgColdCallRegisterSchema.O1_CompanyName, "TESTCOMPANYNAME1");
				AssertEquals("TESTCOMPANYNAME1 should not be imported", 0, Factory.Load<SalesEnquiry>(testCompanyName1Query).Length);
				AssertCollectionContains("Row 2 excluded... data is inconsistent with required format - Field No. 21 (Ref_To_Org_Code) must reference an existing Organization before Field No. 22 (Ref_To_Contact_Name) can be specified", inquiryDataLoad.Log);

				var testCompanyName2Query = new ZQuery(OrgColdCallRegisterSchema.O1_CompanyName, "TESTCOMPANYNAME2");
				AssertEquals("TESTCOMPANYNAME2 should not be imported", 0, Factory.Load<SalesEnquiry>(testCompanyName2Query).Length);
				AssertCollectionContains("Row 3 excluded... data is inconsistent with required format - Field No. 21 (Ref_To_Org_Code) must reference an existing Organization - Field No. 21 (Ref_To_Org_Code) must reference an existing Organization before Field No. 22 (Ref_To_Contact_Name) can be specified", inquiryDataLoad.Log);

				var testCompanyName3Query = new ZQuery(OrgColdCallRegisterSchema.O1_CompanyName, "TESTCOMPANYNAME3");
				AssertEquals("TESTCOMPANYNAME3 should have been imported", 1, Factory.Load<SalesEnquiry>(testCompanyName3Query).Length);

				var testCompanyName4Query = new ZQuery(OrgColdCallRegisterSchema.O1_CompanyName, "TESTCOMPANYNAME4");
				AssertEquals("TESTCOMPANYNAME4 should have been imported", 1, Factory.Load<SalesEnquiry>(testCompanyName4Query).Length);

				var testCompanyName5Query = new ZQuery(OrgColdCallRegisterSchema.O1_CompanyName, "TESTCOMPANYNAME5");
				AssertEquals("TESTCOMPANYNAME5 should not be imported", 0, Factory.Load<SalesEnquiry>(testCompanyName5Query).Length);

				AssertEquals("Org1 contact has been created", 1, org1.Contacts.Count);
				AssertEquals("Org1 contact has been created - name is correct", "Banana", org1.Contacts[0].OC_ContactName);
				AssertEquals("Org2 contact did not get redundantly created", 1, org2.Contacts.Count);
				AssertEquals("Org3 contact should not be created", 0, org3.Contacts.Count);
			}
		}

		public void TestImportInquiryData_WithNewReferToContactName()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORGCODE12345";
			Factory.Save();

			using (var testFile = TempFile.New())
			{
				using (var streamWriter = new StreamWriter(testFile.Filename))
				{
					streamWriter.WriteLine(GetValidHeaderLine());
					streamWriter.WriteLine("TESTCOMPANYNAME,,,,,,,,Edward,,,,,,,,,,,,,ORGCODE12345,New Contact");
					streamWriter.Flush();
				}

				var inquiryDataLoad = new InquiryDataLoad();
				inquiryDataLoad.ImportInquiryData(testFile.Filename);

				AssertEquals(2, inquiryDataLoad.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(1, inquiryDataLoad.RunCounters.RecsCreated);
				AssertEquals(0, inquiryDataLoad.RunCounters.RecsUpdated);
				AssertEquals(0, inquiryDataLoad.RunCounters.RecsExcluded);
				AssertEquals("2 default messages + message(s) of excluded rows", 2, inquiryDataLoad.Log.Count);

				SalesEnquiry enquiry = Factory.LoadTop1<SalesEnquiry>(new ZQuery(OrgColdCallRegisterSchema.O1_CompanyName, "TESTCOMPANYNAME"));
				AssertNotNull("TESTCOMPANYNAME should be imported", enquiry);
				AssertNotEquals("Contact is linked to enquiry", ZGuid.Empty, enquiry.O1_OC_ReferToContact);
				AssertEquals("Correct contact is linked to enquiry", "New Contact", Factory.Load<OrgContact>(enquiry.O1_OC_ReferToContact).OC_ContactName);
			}
		}

		public void TestImportInquiryData_WithInvalidSource()
		{
			var collection = new CodeDescriptionBoolRelatedItemCollection();
			collection.Add("WTF", (NoResString)"Wise Tech Forum", true);
			collection.Add("ENC", (NoResString)"Encyclopedia", false);
			OrganisationsDataRegistry.Instance.OpportunitySource.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			using (var testFile = TempFile.New())
			{
				using (var streamWriter = new StreamWriter(testFile.Filename))
				{
					streamWriter.WriteLine(GetValidHeaderLine());
					streamWriter.WriteLine("TESTCOMPANYNAME1,,,,,,,,Edward,,,,,WTF,,,,,,,");
					streamWriter.WriteLine("TESTCOMPANYNAME2,,,,,,,,Edward,,,,,ENC,,,,,,,");
					streamWriter.WriteLine("TESTCOMPANYNAME3,,,,,,,,Edward,,,,,WAT,,,,,,,");
					streamWriter.Flush();
				}

				var inquiryDataLoad = new InquiryDataLoad();
				inquiryDataLoad.ImportInquiryData(testFile.Filename);

				AssertEquals(4, inquiryDataLoad.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(2, inquiryDataLoad.RunCounters.RecsCreated);
				AssertEquals(0, inquiryDataLoad.RunCounters.RecsUpdated);
				AssertEquals(1, inquiryDataLoad.RunCounters.RecsExcluded);
				AssertEquals("2 default messages + message(s) of excluded rows", 3, inquiryDataLoad.Log.Count);

				var testCompanyName1Query = new ZQuery(OrgColdCallRegisterSchema.O1_CompanyName, "TESTCOMPANYNAME1");
				AssertEquals("TESTCOMPANYNAME1 should have been imported", 1, Factory.Load<SalesEnquiry>(testCompanyName1Query).Length);

				var testCompanyName2Query = new ZQuery(OrgColdCallRegisterSchema.O1_CompanyName, "TESTCOMPANYNAME2");
				AssertEquals("TESTCOMPANYNAME2 should have been imported", 1, Factory.Load<SalesEnquiry>(testCompanyName2Query).Length);

				var testCompanyName3Query = new ZQuery(OrgColdCallRegisterSchema.O1_CompanyName, "TESTCOMPANYNAME3");
				AssertEquals("TESTCOMPANYNAME3 should not have been imported", 0, Factory.Load<SalesEnquiry>(testCompanyName3Query).Length);
				AssertCollectionContains(string.Format("Row 4 excluded... data is inconsistent with required format - Field No. 13 (Source) must be one of the types specified in the '{0}' registry item", OrganisationsDataRegistry.Instance.OpportunitySource.Caption), inquiryDataLoad.Log);
			}
		}

		public void TestImportInquiryData_WithInvalidLeadInterest()
		{
			var collection = new CodeDescriptionBoolCollection();
			collection.Add("HOT", (NoResString)"Hot", true);
			collection.Add("CLD", (NoResString)"Cold", false);
			OrganisationsDataRegistry.Instance.SalesEnquiryLeadInterests.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			using (var testFile = TempFile.New())
			{
				using (var streamWriter = new StreamWriter(testFile.Filename))
				{
					streamWriter.WriteLine(GetValidHeaderLine());
					streamWriter.WriteLine("TESTCOMPANYNAME1,,,,,,,,Edward,,,,,,,,,,,,HOT");
					streamWriter.WriteLine("TESTCOMPANYNAME2,,,,,,,,Edward,,,,,,,,,,,,COO");
					streamWriter.WriteLine("TESTCOMPANYNAME3,,,,,,,,Edward,,,,,,,,,,,,CLD");
					streamWriter.Flush();
				}

				var inquiryDataLoad = new InquiryDataLoad();
				inquiryDataLoad.ImportInquiryData(testFile.Filename);

				AssertEquals(4, inquiryDataLoad.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(2, inquiryDataLoad.RunCounters.RecsCreated);
				AssertEquals(0, inquiryDataLoad.RunCounters.RecsUpdated);
				AssertEquals(1, inquiryDataLoad.RunCounters.RecsExcluded);
				AssertEquals("2 default messages + message(s) of excluded rows", 3, inquiryDataLoad.Log.Count);

				var testCompanyName1Query = new ZQuery(OrgColdCallRegisterSchema.O1_CompanyName, "TESTCOMPANYNAME1");
				AssertEquals("TESTCOMPANYNAME1 should have been imported", 1, Factory.Load<SalesEnquiry>(testCompanyName1Query).Length);

				var testCompanyName2Query = new ZQuery(OrgColdCallRegisterSchema.O1_CompanyName, "TESTCOMPANYNAME2");
				AssertEquals("TESTCOMPANYNAME2 should not have been imported", 0, Factory.Load<SalesEnquiry>(testCompanyName2Query).Length);
				AssertCollectionContains(string.Format("Row 3 excluded... data is inconsistent with required format - Field No. 20 (Lead_Interest) must be one of the types specified in the '{0}' registry item", OrganisationsDataRegistry.Instance.SalesEnquiryLeadInterests.Caption), inquiryDataLoad.Log);

				var testCompanyName3Query = new ZQuery(OrgColdCallRegisterSchema.O1_CompanyName, "TESTCOMPANYNAME3");
				AssertEquals("TESTCOMPANYNAME3 should have been imported", 1, Factory.Load<SalesEnquiry>(testCompanyName3Query).Length);
			}
		}

		public void TestImportInquiryData_WithoutCompanyAndContactNameData()
		{
			using (var testFile = TempFile.New())
			{
				using (var streamWriter = new StreamWriter(testFile.Filename))
				{
					streamWriter.WriteLine(GetValidHeaderLine());
					streamWriter.WriteLine("TESTCOMPANYNAME,,,,,,,,Edward,,,,,,,,");
					streamWriter.WriteLine("Q,,,,,,,,S,,,,,,,,");
					streamWriter.WriteLine(",,,,,,,,,,,,,,,,");
					streamWriter.Flush();
				}

				var inquiryDataLoad = new InquiryDataLoad();
				inquiryDataLoad.ImportInquiryData(testFile.Filename);

				AssertEquals(4, inquiryDataLoad.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(2, inquiryDataLoad.RunCounters.RecsCreated);
				AssertEquals(0, inquiryDataLoad.RunCounters.RecsUpdated);
				AssertEquals(1, inquiryDataLoad.RunCounters.RecsExcluded);
				AssertEquals(3, inquiryDataLoad.Log.Count);
				AssertCollectionContains("Row 4 excluded... data is inconsistent with required format - Field No. 1 (Company Name) must contain value with a length of at least one character - Field No. 8 (Contact Name) must contain value with a length of at least one character", inquiryDataLoad.Log);
			}
		}

		#region Implementation

		string GetValidHeaderLine()
		{
			return "COMPANY,ADDRESS1,ADDRESS2,CITY,STATE,POSTCODE,COUNTRY,BUS_REG_NO,CONTACT_NAME,PHONE,EMAIL,MOBILE,FAX,SOURCE,DETAILS,SALES_REP,NOTES,TYPE,REF_ORG_CODE,REF_CONTACT_NAME,LEAD_INTEREST,REF_TO_ORG_CODE,REF_TO_CONTACT_NAME";
		}

		protected override InquiryDataLoad GetNewDataLoader()
		{
			return new InquiryDataLoad();
		}

		#endregion
	}
}
