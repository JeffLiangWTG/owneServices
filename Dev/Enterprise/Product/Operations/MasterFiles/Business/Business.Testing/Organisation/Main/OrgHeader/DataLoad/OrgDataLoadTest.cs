using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Organizations.CodeGeneration;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.OrgDataLoad;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgDataLoad))]
	public class OrgDataLoadTest : DataLoadTestCase<OrgDataLoad>
	{
		#region TestImportingRegistrationDetails
		public void TestImportingRegistrationDetails()
		{
			using (TempFile testFileName = TempFile.New(Env.TempPath, "csv"))
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Name,Address1,Address2,City,State,PostCode,UNLOCO,Country,PortCity,Phone,Fax,Email,Web,RegNo,CorpCode,Debtor,Creditor,Consignee,Consignor,Forwarder,Broker,Carrier,ShipLine,Airline,LocalTransport,SalesLead,Services,Competitor,Contact,Title,Email,Phone,Mobile,Fax,DebtorCode,DebtorGroup,DebtorSettleGroup,Currency,CreditLimit,CreditRating,GST,INV_TERMS_STANDARD,INV_DAYS_STANDARD,INV_TERMS_DISBURSEMENT,INV_DAYS_DISBURSEMENT,CreditorGroup,CustomsAgent,PostAddress1,PostAddress2,PostCity,PostState,PostPostCode,DeliverAddress1,DeliverAddress2,DeliverCity,DeliverState,DeliverPostCode,Bank,AccountName,AccountNo,BSB,CCD,CSC,SCC,CCP,CCC,CMP,WorkNotes,HandlingNotes,DeliveryNotes,ARNotes,ARCreditNotes,APNotes,ContactSourceType,CONTACTDATEDETAILSVERIFIED,CONTACTSALUTATION,LANGUAGE,MAINADDRESSLANGUAGE,POSTALADDRESSLANGUAGE,DELIVERYADDRESSLANGUAGE,REGDETAIL1,REGDETAIL2,REGDETAIL3,REGDETAIL4,REGDETAIL5,REGDETAIL6,REGDETAIL7,REGDETAIL8,REGDETAIL9,REGDETAIL10,REGDETAIL11,REGDETAIL12,REGDETAIL13,REGDETAIL14,REGDETAIL15,REGDETAIL16,REGDETAIL17,REGDETAIL18,REGDETAIL19,REGDETAIL20,REGDETAIL21,REGDETAIL22,REGDETAIL23,REGDETAIL24,REGDETAIL25,REGDETAIL26,REGDETAIL27,REGDETAIL28,REGDETAIL29,REGDETAIL30,REGDETAIL31,REGDETAIL32,REGDETAIL33,REGDETAIL34,REGDETAIL35,REGDETAIL36,REGDETAIL37,REGDETAIL38,REGDETAIL39,REGDETAIL40,REGDETAIL41,REGDETAIL42,REGDETAIL43,REGDETAIL44,REGDETAIL45,REGDETAIL46,REGDETAIL47,REGDETAIL48,REGDETAIL49,REGDETAIL50,REGDETAIL51,REGDETAIL52,REGDETAIL53,REGDETAIL54,REGDETAIL55,REGDETAIL56,REGDETAIL57,REGDETAIL58,REGDETAIL59,REGDETAIL60,REGDETAIL61,REGDETAIL62,REGDETAIL63,REGDETAIL64,REGDETAIL65,REGDETAIL66,REGDETAIL67,REGDETAIL68,REGDETAIL69,REGDETAIL70,REGDETAIL71,REGDETAIL72,REGDETAIL73,REGDETAIL74,REGDETAIL75,REGDETAIL76,REGDETAIL77,REGDETAIL78,REGDETAIL79,REGDETAIL80,REGDETAIL81,REGDETAIL82,REGDETAIL83,REGDETAIL84,REGDETAIL85,REGDETAIL86,REGDETAIL87,REGDETAIL88,REGDETAIL89,REGDETAIL90,REGDETAIL91,REGDETAIL92,REGDETAIL93,REGDETAIL94,REGDETAIL95,REGDETAIL96,REGDETAIL97,REGDETAIL98,REGDETAIL99");
					sw.WriteLine("Z321Test4,Z321-Test Name,Address1,Address2,City,QLD,Post Code,AUBNE,Country,Port City,Phone,Fax,Email,Web,24-587345000,Government Corporation Code,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Contact Name,Contact Job Title,Contact Email,Contact Work Phone,Contact Mobile,Contact Fax,Z321Test2,Debtor Acc Group,Z321Test2,HKD,1500,CR3,Y,,0,,0,CG1,Z321Test2,Postal Address1,Postal Address2,Postal City,Postal State,Postal Post Code,Deliver Address1,Deliver Address2,Deliver City,Deliver State,Deliver Post Code,Bank Name,Account Name,Account Number,BSB Number,Customs Client Code,Customs Supplier Code,Customs Supplier Code (CMR),Customs Controlled Premises Code,Customs Carrier Code,Customs Manifest Provider Code,Work Notes,Handling Instructions,Delivery Instructions,A/R Account Management Notes,A/R Credit Management Notes,A/P Account Management Notes,,,,,,,,US~E1~NUMBERE1,US~E2~NUMBERE2,US~E3~NUMBERE3,US~E4~NUMBERE4,US~E5~NUMBERE5,US~E6~NUMBERE6,US~E7~NUMBERE7,US~E8~NUMBERE8,US~E9~NUMBERE9,US~E10~NUMBERE10,US~E11~NUMBERE11,US~E12~NUMBERE12,US~E13~NUMBERE13,US~E14~NUMBERE14,US~E15~NUMBERE15,US~E16~NUMBERE16,US~E17~NUMBERE17,US~E18~NUMBERE18,US~E19~NUMBERE19,US~E20~NUMBERE20,US~E21~NUMBERE21,US~E22~NUMBERE22,US~E23~NUMBERE23,US~E24~NUMBERE24,US~E25~NUMBERE25,US~E26~NUMBERE26,US~E27~NUMBERE27,US~E28~NUMBERE28,US~E29~NUMBERE29,US~E30~NUMBERE30,US~E31~NUMBERE31,US~E32~NUMBERE32,US~E33~NUMBERE33,US~E34~NUMBERE34,US~E35~NUMBERE35,US~E36~NUMBERE36,US~E37~NUMBERE37,US~E38~NUMBERE38,US~E39~NUMBERE39,US~E40~NUMBERE40,US~E41~NUMBERE41,US~E42~NUMBERE42,US~E43~NUMBERE43,US~E44~NUMBERE44,US~E45~NUMBERE45,US~E46~NUMBERE46,US~E47~NUMBERE47,US~E48~NUMBERE48,US~E49~NUMBERE49,US~E50~NUMBERE50,US~E51~NUMBERE51,US~E52~NUMBERE52,US~E53~NUMBERE53,US~E54~NUMBERE54,US~E55~NUMBERE55,US~E56~NUMBERE56,US~E57~NUMBERE57,US~E58~NUMBERE58,US~E59~NUMBERE59,US~E60~NUMBERE60,US~E61~NUMBERE61,US~E62~NUMBERE62,US~E63~NUMBERE63,US~E64~NUMBERE64,US~E65~NUMBERE65,US~E66~NUMBERE66,US~E67~NUMBERE67,US~E68~NUMBERE68,US~E69~NUMBERE69,US~E70~NUMBERE70,US~E71~NUMBERE71,US~E72~NUMBERE72,US~E73~NUMBERE73,US~E74~NUMBERE74,US~E75~NUMBERE75,US~E76~NUMBERE76,US~E77~NUMBERE77,US~E78~NUMBERE78,US~E79~NUMBERE79,US~E80~NUMBERE80,US~E81~NUMBERE81,US~E82~NUMBERE82,US~E83~NUMBERE83,US~E84~NUMBERE84,US~E85~NUMBERE85,US~E86~NUMBERE86,US~E87~NUMBERE87,US~E88~NUMBERE88,US~E89~NUMBERE89,US~E90~NUMBERE90,US~E91~NUMBERE91,US~E92~NUMBERE92,US~E93~NUMBERE93,US~E94~NUMBERE94,US~E95~NUMBERE95,US~E96~NUMBERE96,US~E97~NUMBERE97,US~E98~NUMBERE98,US~E99~NUMBERE99");
					sw.WriteLine("Z321Test4,Z321-Test Name,Address1,Address2,City,QLD,Post Code,AUBNE,Country,Port City,Phone,Fax,Email,Web,24-587345000,Government Corporation Code,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Contact Name,Contact Job Title,Contact Email,Contact Work Phone,Contact Mobile,Contact Fax,Z321Test2,Debtor Acc Group,Z321Test2,HKD,1500,CR3,Y,,0,,0,CG1,Z321Test2,Postal Address1,Postal Address2,Postal City,Postal State,Postal Post Code,Deliver Address1,Deliver Address2,Deliver City,Deliver State,Deliver Post Code,Bank Name,Account Name,Account Number,BSB Number,Customs Client Code,Customs Supplier Code,Customs Supplier Code (CMR),Customs Controlled Premises Code,Customs Carrier Code,Customs Manifest Provider Code,Work Notes,Handling Instructions,Delivery Instructions,A/R Account Management Notes,A/R Credit Management Notes,A/P Account Management Notes,,,,,,,,US~EIN~24-587345000,US~MID~BOB1231,US~E50~NUMBERE50,US~E60~NUMBERE61,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,");
					sw.WriteLine("Z321Test5,Z321-Test Co Name,Test Address1,Test Address2, ,NSW,2000,AUSYD,,,99994444,99995555,test@email.au,www.test.com.au,234322,,Y,N,Y,Y,N,N,N,N,N,N,N,N,N,,,,,,,,,,AUD,5000,CR2,Y,,0,,0,,,,,,,,,,,,,ANZ,,787458,012-045,,,,,,,Test notes,,,,,,,,,,,,,US~EIN~EINNUMBER23,US~MID~BOB8894,US~E50~NUMBERE365,US~E60~NUMBERE985,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,");
					sw.WriteLine("Z321Test6,Z321-TestOrg,Addr1,Addr2,City,,2020,AUSYD,,,98250011,,,,,,N,Y,N,N,N,N,N,N,N,N,N,N,N,John Smith,Manager,,98250011,1481123456,98250013,,,,AUD,1500,,Y,,0,,0,,,,,,,,,,,,,ANZ,,457983723,012-344,,,,,,,,,,,,,,,,,,,,US~EIN~24-587345000,US~MID~BOB1231,US~E50~NUMBERE50,US~E60~NUMBERE61,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,");
				}

				DoImportForTestImportingRegistrationDetails(testFileName);
			}
		}

		protected virtual void DoImportForTestImportingRegistrationDetails(TempFile testFileName)
		{
			loader.ImportOrganisationData(testFileName.Filename, false);
			AssertEquals(2, loader.RunCounters.RecordsToImportPlusHeader);
			AssertEquals(5, loader.RunCounters.RecsCreated);
			AssertEquals(0, loader.RunCounters.RecsUpdated);
			AssertEquals(1, loader.RunCounters.RecsExcluded);
			AssertEquals(6, loader.Log.Count);
		}
		#endregion

		#region TestImportGSTFlags
		public void TestImportGSTFlags()
		{
			using (TempFile testFileName = TempFile.New(Env.TempPath, "csv"))
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Name,Address1,Address2,City,State,PostCode,UNLOCO,Country,PortCity,Phone,Fax,Email,Web,RegNo,CorpCode,Debtor,Creditor,Consignee,Consignor,Forwarder,Broker,Carrier,ShipLine,Airline,LocalTransport,SalesLead,Services,Competitor,Contact,Title,Email,Phone,Mobile,Fax,DebtorCode,DebtorGroup,DebtorSettleGroup,Currency,CreditLimit,CreditRating,GST,INV_TERMS_STANDARD,INV_DAYS_STANDARD,INV_TERMS_DISBURSEMENT,INV_DAYS_DISBURSEMENT,CreditorGroup,CustomsAgent,PostAddress1,PostAddress2,PostCity,PostState,PostPostCode,DeliverAddress1,DeliverAddress2,DeliverCity,DeliverState,DeliverPostCode,Bank,AccountName,AccountNo,BSB,CCD,CSC,SCC,CCP,CCC,CMP,WorkNotes,HandlingNotes,DeliveryNotes,ARNotes,ARCreditNotes,APNotes,ContactSourceType,CONTACTDATEDETAILSVERIFIED,CONTACTSALUTATION,LANGUAGE,MAINADDRESSLANGUAGE,POSTALADDRESSLANGUAGE,DELIVERYADDRESSLANGUAGE,REGDETAIL1,REGDETAIL2");
					sw.WriteLine("Z321Test1,Z321-Test Name,Address1,Address2,City,QLD,2000,AUSYD,AU,,99994444,99995555,test@email.au,www.test.com.au,234322,,Y,N,Y,Y,N,N,N,N,N,N,N,N,N,,,,,,,,,,AUD,5000,CR2,N,,0,,0,,,,,,,,,,,,,ANZ,,787458,012-045,,,,,,,Test notes,,,,,,,,,,,,,US~EIN~EINNUMBER23,US~MID~BOB8894"); //is debtor, GST flag (after CR2) = N
					sw.WriteLine("Z321Test2,Z322-Test Name,Address1,Address2,City,QLD,2000,AUSYD,AU,,99994444,99995555,test@email.au,www.test.com.au,234322,,N,Y,Y,Y,N,N,N,N,N,N,N,N,N,,,,,,,,,,AUD,5000,CR2,N,,0,,0,,,,,,,,,,,,,ANZ,,787458,012-045,,,,,,,Test notes,,,,,,,,,,,,,US~EIN~EINNUMBER23,US~MID~BOB8894"); //is creditor, GST flag (after CR2) = N
				}

				AssertNull("PreCondition: Organisation doesn't exist", FindOrgByFullName("Z321-Test Name"));
				AssertNull("PreCondition: Organisation doesn't exist", FindOrgByFullName("Z322-Test Name"));

				DoImportForTestImportGSTFlags(testFileName);

				var org1 = FindOrgByFullName("Z321-Test Name");
				var org2 = FindOrgByFullName("Z322-Test Name");
				AssertNotNull("PostCondition: Organisation doesn't exist", org1);
				AssertNotNull("PostCondition: Organisation doesn't exist", org2);

				Assert(!org1.CompanyData.IsARTaxApplicable);
				Assert(org1.CompanyData.IsAPTaxApplicable);

				Assert(org2.CompanyData.IsARTaxApplicable);
				Assert(!org2.CompanyData.IsAPTaxApplicable);
			}
		}

		protected virtual void DoImportForTestImportGSTFlags(TempFile testFileName)
		{
			loader.ImportOrganisationData(testFileName.Filename, false);
		}
		#endregion

		#region TestImportingRegistrationDetailThatRequiresAPremisesAddress
		public void TestImportingRegistrationDetailThatRequiresAPremisesAddress()
		{
			using (TempFile testFileName = TempFile.New(Env.TempPath, "csv"))
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Name,Address1,Address2,City,State,PostCode,UNLOCO,Country,PortCity,Phone,Fax,Email,Web,RegNo,CorpCode,Debtor,Creditor,Consignee,Consignor,Forwarder,Broker,Carrier,ShipLine,Airline,LocalTransport,SalesLead,Services,Competitor,Contact,Title,Email,Phone,Mobile,Fax,DebtorCode,DebtorGroup,DebtorSettleGroup,Currency,CreditLimit,CreditRating,GST,INV_TERMS_STANDARD,INV_DAYS_STANDARD,INV_TERMS_DISBURSEMENT,INV_DAYS_DISBURSEMENT,CreditorGroup,CustomsAgent,PostAddress1,PostAddress2,PostCity,PostState,PostPostCode,DeliverAddress1,DeliverAddress2,DeliverCity,DeliverState,DeliverPostCode,Bank,AccountName,AccountNo,BSB,CCD,CSC,SCC,CCP,CCC,CMP,WorkNotes,HandlingNotes,DeliveryNotes,ARNotes,ARCreditNotes,APNotes,ContactSourceType,CONTACTDATEDETAILSVERIFIED,CONTACTSALUTATION,LANGUAGE,MAINADDRESSLANGUAGE,POSTALADDRESSLANGUAGE,DELIVERYADDRESSLANGUAGE,REGDETAIL1,REGDETAIL2");
					sw.WriteLine("Z321Test1,Z321-Test Name,Address1,Address2,City,QLD,2000,AUSYD,AU,,99994444,99995555,test@email.au,www.test.com.au,234322,,Y,N,Y,Y,N,N,N,N,N,N,N,N,N,,,,,,,,,,AUD,5000,CR2,Y,,0,,0,,,,,,,,,,,,,ANZ,,787458,012-045,,,,,,,Test notes,,,,,,,,,,,,,US~EIN~EINNUMBER23,US~MID~BOB8894");
				}

				AssertNull("PreCondition: Organisation doesn't exist", FindOrgByFullName("Z321-Test Name"));

				DoImportForTestImportingRegistrationDetailThatRequiresAPremisesAddress(testFileName);

				var org = FindOrgByFullName("Z321-Test Name");
				AssertNotNull("Z321Test1 organisation was imported", org);

				var cusCode = org.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry("EIN", "US");
				AssertEquals("Test importing US~EIN~EINNUMBER23", "EINNUMBER23", cusCode.OK_CustomsRegNo);
				AssertEquals("EIN should not need an address", ZGuid.Empty, cusCode.OK_OA_PremisesAddress);
				cusCode = org.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry("MID", "US");
				AssertEquals("Test importing US~MID~BOB8894", "BOB8894", cusCode.OK_CustomsRegNo);
				AssertEquals("MIN should need an address", org.MainAddress.PK, cusCode.OK_OA_PremisesAddress);
			}
		}

		protected virtual void DoImportForTestImportingRegistrationDetailThatRequiresAPremisesAddress(TempFile testFileName)
		{
			loader.ImportOrganisationData(testFileName.Filename, false);
		}

		#endregion

		#region TestNewRegDetailColumnsDoesNotCauseDuplicateRegistrationDataIfSpecifiedInMandatoryColumns
		public void TestNewRegDetailColumnsDoesNotCauseDuplicateRegistrationDataIfSpecifiedInMandatoryColumns()
		{
			using (TempFile testFileName = TempFile.New(Env.TempPath, "csv"))
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Name,Address1,Address2,City,State,PostCode,UNLOCO,Country,PortCity,Phone,Fax,Email,Web,RegNo,CorpCode,Debtor,Creditor,Consignee,Consignor,Forwarder,Broker,Carrier,ShipLine,Airline,LocalTransport,SalesLead,Services,Competitor,Contact,Title,Email,Phone,Mobile,Fax,DebtorCode,DebtorGroup,DebtorSettleGroup,Currency,CreditLimit,CreditRating,GST,INV_TERMS_STANDARD,INV_DAYS_STANDARD,INV_TERMS_DISBURSEMENT,INV_DAYS_DISBURSEMENT,CreditorGroup,CustomsAgent,PostAddress1,PostAddress2,PostCity,PostState,PostPostCode,DeliverAddress1,DeliverAddress2,DeliverCity,DeliverState,DeliverPostCode,Bank,AccountName,AccountNo,BSB,CCD,CSC,SCC,CCP,CCC,CMP,WorkNotes,HandlingNotes,DeliveryNotes,ARNotes,ARCreditNotes,APNotes,ContactSourceType,CONTACTDATEDETAILSVERIFIED,CONTACTSALUTATION,LANGUAGE,MAINADDRESSLANGUAGE,POSTALADDRESSLANGUAGE,DELIVERYADDRESSLANGUAGE,REGDETAIL1,REGDETAIL2,REGDETAIL3,REGDETAIL4,REGDETAIL5,REGDETAIL6,REGDETAIL7,REGDETAIL8");
					sw.WriteLine("Z321Test0,Z321-Test Name,Address1,Address2,City,QLD,Post Code,AUBNE,Country,Port City,Phone,Fax,Email,Web,24-587345000,Government Corporation Code,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Contact Name,Contact Job Title,Contact Email,Contact Work Phone,Contact Mobile,Contact Fax,Z321Test2,Debtor Acc Group,Z321Test2,HKD,1500,CR3,Y,,0,,0,CG1,Z321Test2,Postal Address1,Postal Address2,Postal City,Postal State,Postal Post Code,Deliver Address1,Deliver Address2,Deliver City,Deliver State,Deliver Post Code,Bank Name,Account Name,Account Number,BSB Number,Customs Client Code,Customs Supplier Code,Customs Supplier Code (CMR),Customs Controlled Premises Code,Customs Carrier Code,Customs Manifest Provider Code,Work Notes,Handling Instructions,Delivery Instructions,A/R Account Management Notes,A/R Credit Management Notes,A/P Account Management Notes,,,,,,,,AU~CCD~CCD789464,AU~CSC~CSC789464,AU~CID~CID789464,AU~CCC~CCC789464,AU~CMP~CMP789464,AU~CCP~CCP789464,AU~LSC~LSC789464,AU~BRN~BRN789464");
				}

				DoImportForTestNewRegDetailColumnsDoesNotCauseDuplicateRegistrationDataIfSpecifiedInMandatoryColumns(testFileName);

				var expectedCodesAndValues = new Dictionary<ZString, ZString>();
				expectedCodesAndValues.Add("CCD", "CCD789464");
				expectedCodesAndValues.Add("CSC", "CSC789464");
				expectedCodesAndValues.Add("CID", "CID789464");
				expectedCodesAndValues.Add("CCC", "CCC789464");
				expectedCodesAndValues.Add("CMP", "CMP789464");
				expectedCodesAndValues.Add("CCP", "CCP789464");
				expectedCodesAndValues.Add("LSC", "LSC789464");
				expectedCodesAndValues.Add("BRN", "BRN789464");

				var org = LoadOrganisation("Z321-Test Name", "Address1");

				foreach (var pair in expectedCodesAndValues)
				{
					var cusCodes = org.CustomsCodes.GetOrgCusCodesForCodeAndCountry(pair.Key, "AU");
					AssertEquals(1, cusCodes.Length);
					AssertEquals(pair.Value, cusCodes[0].OK_CustomsRegNo);
				}
			}
		}

		protected virtual void DoImportForTestNewRegDetailColumnsDoesNotCauseDuplicateRegistrationDataIfSpecifiedInMandatoryColumns(TempFile testFileName)
		{
			loader.ImportOrganisationData(testFileName.Filename, false);
		}
		#endregion

		#region TestCannotImportIfRegDetailContainsMissingOrInvalidFormat
		public void TestCannotImportIfRegDetailContainsMissingOrInvalidFormat()
		{
			using (TempFile testFileName = TempFile.New(Env.TempPath, "csv"))
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Name,Address1,Address2,City,State,PostCode,UNLOCO,Country,PortCity,Phone,Fax,Email,Web,RegNo,CorpCode,Debtor,Creditor,Consignee,Consignor,Forwarder,Broker,Carrier,ShipLine,Airline,LocalTransport,SalesLead,Services,Competitor,Contact,Title,Email,Phone,Mobile,Fax,DebtorCode,DebtorGroup,DebtorSettleGroup,Currency,CreditLimit,CreditRating,GST,INV_TERMS_STANDARD,INV_DAYS_STANDARD,INV_TERMS_DISBURSEMENT,INV_DAYS_DISBURSEMENT,CreditorGroup,CustomsAgent,PostAddress1,PostAddress2,PostCity,PostState,PostPostCode,DeliverAddress1,DeliverAddress2,DeliverCity,DeliverState,DeliverPostCode,Bank,AccountName,AccountNo,BSB,CCD,CSC,SCC,CCP,CCC,CMP,WorkNotes,HandlingNotes,DeliveryNotes,ARNotes,ARCreditNotes,APNotes,ContactSourceType,CONTACTDATEDETAILSVERIFIED,CONTACTSALUTATION,LANGUAGE,MAINADDRESSLANGUAGE,POSTALADDRESSLANGUAGE,DELIVERYADDRESSLANGUAGE,REGDETAIL1");
					sw.WriteLine("Z321TEST0,Z321-Test0 Name,Address1,Address2,City,QLD,Post Code,AUBNE,Country,Port City,Phone,Fax,Email,Web,24-587345000,Government Corporation Code,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Contact Name,Contact Job Title,Contact Email,Contact Work Phone,Contact Mobile,Contact Fax,Z321Test2,Debtor Acc Group,Z321Test2,HKD,1500,CR3,Y,,0,,0,CG1,Z321Test2,Postal Address1,Postal Address2,Postal City,Postal State,Postal Post Code,Deliver Address1,Deliver Address2,Deliver City,Deliver State,Deliver Post Code,Bank Name,Account Name,Account Number,BSB Number,Customs Client Code,Customs Supplier Code,Customs Supplier Code (CMR),Customs Controlled Premises Code,Customs Carrier Code,Customs Manifest Provider Code,Work Notes,Handling Instructions,Delivery Instructions,A/R Account Management Notes,A/R Credit Management Notes,A/P Account Management Notes,,,,,,,,US.MID~NEWMID1234");
					sw.WriteLine("Z321TEST1,Z321-Test1 Name,Address1,Address2,City,QLD,Post Code,AUBNE,Country,Port City,Phone,Fax,Email,Web,24-587345000,Government Corporation Code,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Contact Name,Contact Job Title,Contact Email,Contact Work Phone,Contact Mobile,Contact Fax,Z321Test2,Debtor Acc Group,Z321Test2,HKD,1500,CR3,Y,,0,,0,CG1,Z321Test2,Postal Address1,Postal Address2,Postal City,Postal State,Postal Post Code,Deliver Address1,Deliver Address2,Deliver City,Deliver State,Deliver Post Code,Bank Name,Account Name,Account Number,BSB Number,Customs Client Code,Customs Supplier Code,Customs Supplier Code (CMR),Customs Controlled Premises Code,Customs Carrier Code,Customs Manifest Provider Code,Work Notes,Handling Instructions,Delivery Instructions,A/R Account Management Notes,A/R Credit Management Notes,A/P Account Management Notes,,,,,,,,~MID~NEWMID5678");
					sw.WriteLine("Z321TEST2,Z321-Test2 Name,Address1,Address2,City,QLD,Post Code,AUBNE,Country,Port City,Phone,Fax,Email,Web,24-587345000,Government Corporation Code,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Contact Name,Contact Job Title,Contact Email,Contact Work Phone,Contact Mobile,Contact Fax,Z321Test2,Debtor Acc Group,Z321Test2,HKD,1500,CR3,Y,,0,,0,CG1,Z321Test2,Postal Address1,Postal Address2,Postal City,Postal State,Postal Post Code,Deliver Address1,Deliver Address2,Deliver City,Deliver State,Deliver Post Code,Bank Name,Account Name,Account Number,BSB Number,Customs Client Code,Customs Supplier Code,Customs Supplier Code (CMR),Customs Controlled Premises Code,Customs Carrier Code,Customs Manifest Provider Code,Work Notes,Handling Instructions,Delivery Instructions,A/R Account Management Notes,A/R Credit Management Notes,A/P Account Management Notes,,,,,,,,US~~NEWMID9012");
					sw.WriteLine("Z321TEST3,Z321-Test3 Name,Address1,Address2,City,QLD,Post Code,AUBNE,Country,Port City,Phone,Fax,Email,Web,24-587345000,Government Corporation Code,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Contact Name,Contact Job Title,Contact Email,Contact Work Phone,Contact Mobile,Contact Fax,Z321Test2,Debtor Acc Group,Z321Test2,HKD,1500,CR3,Y,,0,,0,CG1,Z321Test2,Postal Address1,Postal Address2,Postal City,Postal State,Postal Post Code,Deliver Address1,Deliver Address2,Deliver City,Deliver State,Deliver Post Code,Bank Name,Account Name,Account Number,BSB Number,Customs Client Code,Customs Supplier Code,Customs Supplier Code (CMR),Customs Controlled Premises Code,Customs Carrier Code,Customs Manifest Provider Code,Work Notes,Handling Instructions,Delivery Instructions,A/R Account Management Notes,A/R Credit Management Notes,A/P Account Management Notes,,,,,,,,US~MID~");
					sw.WriteLine("Z321TEST4,Z321-Test4 Name,Address1,Address2,City,QLD,Post Code,AUBNE,Country,Port City,Phone,Fax,Email,Web,24-587345000,Government Corporation Code,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Contact Name,Contact Job Title,Contact Email,Contact Work Phone,Contact Mobile,Contact Fax,Z321Test2,Debtor Acc Group,Z321Test2,HKD,1500,CR3,Y,,0,,0,CG1,Z321Test2,Postal Address1,Postal Address2,Postal City,Postal State,Postal Post Code,Deliver Address1,Deliver Address2,Deliver City,Deliver State,Deliver Post Code,Bank Name,Account Name,Account Number,BSB Number,Customs Client Code,Customs Supplier Code,Customs Supplier Code (CMR),Customs Controlled Premises Code,Customs Carrier Code,Customs Manifest Provider Code,Work Notes,Handling Instructions,Delivery Instructions,A/R Account Management Notes,A/R Credit Management Notes,A/P Account Management Notes,,,,,,,,USA~MID~");
				}

				DoImportForTestCannotImportIfRegDetailContainsMissingOrInvalidFormat(testFileName);

				string error = String.Format(errorRowIdentifier(0) + " excluded: data is inconsistent with required format Organization [Code: Z321TEST0, Name: Z321-Test0 Name] has an invalid registration detail (US.MID~NEWMID1234) in column 'REGDETAIL1'; valid registration detail should be in the following format: Country/Region Of Issue~Registration Type~Registration Number e.g. US~MID~MID123456");
				Assert("Log should contain: " + error, LogContains(error));

				error = String.Format(errorRowIdentifier(1) + " excluded: data is inconsistent with required format Organization [Code: Z321TEST1, Name: Z321-Test1 Name] has an invalid registration detail (~MID~NEWMID5678) in column 'REGDETAIL1'; valid registration detail should be in the following format: Country/Region Of Issue~Registration Type~Registration Number e.g. US~MID~MID123456");
				Assert("Log should contain: " + error, LogContains(error));

				error = String.Format(errorRowIdentifier(2) + " excluded: data is inconsistent with required format Organization [Code: Z321TEST2, Name: Z321-Test2 Name] has an invalid registration detail (US~~NEWMID9012) in column 'REGDETAIL1'; valid registration detail should be in the following format: Country/Region Of Issue~Registration Type~Registration Number e.g. US~MID~MID123456");
				Assert("Log should contain: " + error, LogContains(error));

				AssertNotNull("Post: Organisation wasn't imported [Z321-Test3 Name]", FindOrgByFullName("Z321-Test3 Name"));

				AssertNull("Post: Organisation should not be imported [Z321-Test0 Name]", FindOrgByFullName("Z321-Test0 Name"));
				AssertNull("Post: Organisation should not be imported [Z321-Test1 Name]", FindOrgByFullName("Z321-Test1 Name"));
				AssertNull("Post: Organisation should not be imported [Z321-Test2 Name]", FindOrgByFullName("Z321-Test2 Name"));

				error = String.Format(errorRowIdentifier(4) + " excluded: data is inconsistent with required format Organization [Code: Z321TEST4, Name: Z321-Test4 Name] has an invalid country/region code (USA) in column 'REGDETAIL1': country/region code too long. Please ensure that it's only 2 characters long.");
				Assert("Log should contain: " + error, LogContains(error));
			}
		}

		protected virtual void DoImportForTestCannotImportIfRegDetailContainsMissingOrInvalidFormat(TempFile testFileName)
		{
			loader.ImportOrganisationData(testFileName.Filename, false);
		}

		protected virtual bool LogContains(string error)
		{
			return loader.Log.Any(entry => entry == error);
		}

		protected virtual string errorRowIdentifier(int rowNumber)
		{
			string errorMessageIdentity = "Row " + (rowNumber + 2);
			return errorMessageIdentity;
		}
		#endregion

		#region TestImportOrganisation
		public void TestImportOrganisations()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					OutputHeaderLine(sw);
					sw.WriteLine("Z321Test2,Z321-Test Co Name,Test Address1,Test Address2, ,NSW,2000,AUSYD,AU,,99994444,99995555,test@email.au,www.test.com.au,999988885599,,Y,N,Y,Y,N,Y,N,N,N,N,N,N,N,,,,,,,,,,AUD,5000,CR2,Y,PER,14,INV,7,,,,,,,,,,,,,ANZ,,787458,012-045,,,,,,,Test notes,,,,,,25-12-2005,Monseiur,,,,,,N,,Y,N");
					sw.WriteLine("Z321Test1,Z321-Test Name,Address1,Address2,City,QLD,POST CODE,AUBNE,Country/Region,Port City,PHONE,FAX,Email,Web,Business Reg No,Government Corporation Code,Y,Y,Y,Y,Y,Y,Y,N,Y,Y,Y,Y,Y,Contact Name,Contact Job Title,Contact Email,CONTACT WORK PHONE,CONTACT MOBILE,CONTACT FAX,Z321Test2,Debtor Acc Group,Z321Test2,HKD,1500,CR3,Y,,0,,0,CG1,Z321Test2,Postal Address1,Postal Address2,Postal City,Postal State,Postal Post Code,Deliver Address1,Deliver Address2,Deliver City,Deliver State,Deliver Post Code,Bank Name,Account Name,Account Number,BSB Number,Customs Client Code,Customs Supplier Code,Customs Supplier Code (CMR),Customs Controlled Premises Code,Customs Carrier Code,Customs Manifest Provider Code,Bad Work Notes,Nice Goods Handling Instructions,Great Import Delivery Instructions,Fantastic A/R Account Management Notes,Interesting A/R Credit Management Note,Useless A/P Account Management Notes,050908_EXI_PRS,20051225,Senor,,,,,,Y,N,Y");
					sw.WriteLine("Z321Test3,Z321-TestOrg,Addr1,Addr2,City,,2020,AUSYD,AU,,98250011,,,,,,N,Y,N,N,N,N,N,N,N,N,N,N,N,John Smith,Manager,,98250011,01481123456,98250013,,,,AUD,1500,,Y,,0,,0,,,,,,,,,,,,,ANZ,,457983723,012-344,,,,,,,,,,,,,,");
					sw.Flush();
				}

				DoImportForTestImportOrganisation(testFileName);

				OrgHeader[] createdOrgs = FindOrgByFullNameStartsWith("Z321-Test");
				AssertEquals("There should have been 3 organisations created", 3, createdOrgs.Length);

				OrgHeader org1 = createdOrgs[1];
				AssertEquals("Z321-Test Name", org1.OH_FullName);
				AssertEquals("Address1", org1.MainAddress.OA_Address1);
				AssertEquals("Address2", org1.MainAddress.OA_Address2);
				AssertEquals("City", org1.MainAddress.OA_City);
				AssertEquals("QLD", org1.MainAddress.OA_State);
				AssertEquals("POST CODE", org1.MainAddress.OA_PostCode);
				AssertEquals("AUBNE", org1.OH_RL_NKClosestPort);
				AssertEquals("PHONE", org1.MainAddress.OA_Phone);
				AssertEquals("FAX", org1.MainAddress.OA_Fax);
				AssertEquals("Email", org1.MainAddress.OA_Email);
				AssertEquals("Web", org1.MainWebURL.PU_URL);
				Assert("Organisation should be active", org1.OH_IsActive);
				Assert("Organisation should be Debtor", org1.OH_IsDebtor);
				Assert("Organisation should be Consignee", org1.OH_IsConsignee);
				Assert("Organisation should be Consignor", org1.OH_IsConsignor);
				Assert("Organisation should be Shipping Provider", org1.OH_IsShippingProvider);
				Assert("Organisation should not be Shipping Line", !org1.OH_IsShippingLine);
				Assert("Organisation should be Airline", org1.OH_IsAirLine);
				Assert("Organisation should be Local Transport", org1.OH_IsLocalTransport);
				Assert("Organisation should be Forwarder", org1.OH_IsForwarder);
				Assert("Organisation should be Broker", org1.OH_IsBroker);
				Assert("Organisation should be Services", org1.OH_IsMiscFreightServices);
				Assert("Organisation should be Competitor", org1.OH_IsCompetitor);
				Assert("Organisation should be Sales Lead", org1.OH_IsSalesLead);
				Assert("Organisation should not be Controlling Agent", !org1.OH_IsControllingAgent);
				Assert("Organisation should be Controlling Customer", org1.OH_IsControllingCustomer);
				ZString defaultStringCurrency = "HKD";
				AssertEquals("Default A/R Currency should be set", defaultStringCurrency, org1.CompanyData.OB_RX_NKARDDefltCurrency);
				AssertEquals("Default A/P Currency should be set", defaultStringCurrency, org1.CompanyData.OB_RX_NKAPDefltCurrency);
				AssertEquals("Default Forwarding Currency should be set", defaultStringCurrency, org1.MiscServ.OM_RX_NKFWDefCurrency);
				AssertEquals("Should have been 3 address records created for this organisation", 3, org1.Addresses.Count);
				AssertEquals("Should be 1 Element in the AccountDetailsCollection", 1, org1.CompanyData.AccountDetailsCollection.Count);
				AccAPAccountDetails accountDetails = org1.CompanyData.AccountDetailsCollection[0];
				AssertEquals("Bank Name", accountDetails.A1_BankName);
				AssertEquals("Account Name", accountDetails.A1_AccountName);
				AssertEquals("Account Number", accountDetails.A1_BankAccount);
				AssertEquals("BSB Number", accountDetails.A1_BankBsb);

				Assert("A/P GST should be applicable", org1.CompanyData.IsAPTaxApplicable);
				Assert("A/R GST should be applicable", org1.CompanyData.IsARTaxApplicable);
				AssertEquals("Credit Limit", 1500M, org1.MiscServ.OM_ARCreditLimit);
				AssertEquals("Credit Rating", "CR3", org1.MiscServ.OM_ARCreditRating);
				AssertEquals("ARTerms.Count", 1, org1.CompanyData.ARTerms.Count);
				AssertEquals("Default Inv Type", "ALL", org1.CompanyData.ARTerms[0].PY_InvoiceClass);
				AssertEquals("Default Inv Terms", "COD", org1.CompanyData.ARTerms[0].PY_InvoiceTerm);
				AssertEquals("Default Inv Term Days", (short)0, org1.CompanyData.ARTerms[0].PY_InvoiceDays);
				AssertEquals(1, org1.Contacts.Count);
				OrgContact orgContact1 = org1.Contacts[0];
				AssertEquals(org1.PK, orgContact1.OC_OH);
				AssertEquals("Contact Email", orgContact1.OC_Email);
				AssertEquals("EML", orgContact1.OC_NotifyMode);
				AssertEquals("Contact Name", orgContact1.OC_ContactName);
				AssertEquals("Contact Job Title", orgContact1.OC_Title);
				AssertEquals("CONTACT WORK PHONE", orgContact1.OC_Phone);
				AssertEquals("CONTACT MOBILE", orgContact1.OC_Mobile);
				AssertEquals("CONTACT FAX", orgContact1.OC_Fax);
				AssertEquals("Contact Source", "050908_EXI_PRS", orgContact1.OC_ContactSource);
				AssertEquals("Date Details Verified", new ZDateTime(2005, 12, 25), orgContact1.OC_DetailsVerified);
				AssertEquals("Contact Salutation", "Senor", orgContact1.OC_Salutation);
				Assert("Organisation should be Warehouse", org1.OH_IsWarehouseClient);
				AssertEquals("Config details should have been created", 9, org1.CustomsCodes.Count);
				Assert("Notes should have been added", org1.Notes.HasNotes);
				AssertEquals("Crs Account Group", CreditorGroup.PK.ToGuid(), org1.MiscServ.OM_OG_APCreditorGroup);

				foreach (StmNote note in org1.Notes.GetAllNotes())
				{
					Assert("Invalid note description: " + note.ST_Description, PredefinedNoteTypes.Instance.NoteTypeByDescription(note.ST_Description) != null);
				}

				AssertEquals("Invalid Internal Work Notes", "Bad Work Notes", GetNoteByDescription("Internal Work Notes", org1.Notes.GetAllNotes()));
				AssertEquals("Invalid Goods Handling Instructions", "Nice Goods Handling Instructions", GetNoteByDescription("Goods Handling Instructions", org1.Notes.GetAllNotes()));
				AssertEquals("Invalid Import Delivery Instructions", "Great Import Delivery Instructions", GetNoteByDescription("Import Delivery Instructions", org1.Notes.GetAllNotes()));
				AssertEquals("Invalid A/R Account Management Notes", "Fantastic A/R Account Management Notes", GetNoteByDescription("A/R Account Management Notes", org1.Notes.GetAllNotes()));
				AssertEquals("Invalid A/R Credit Management Note", "Interesting A/R Credit Management Note", GetNoteByDescription("A/R Credit Management Note", org1.Notes.GetAllNotes()));
				AssertEquals("Invalid A/P Account Management Notes", "Useless A/P Account Management Notes", GetNoteByDescription("A/P Account Management Notes", org1.Notes.GetAllNotes()));

				OrgHeader org2 = createdOrgs[0];
				AssertEquals("Z321-Test Co Name", org2.OH_FullName);
				AssertEquals("Test Address1", org2.MainAddress.OA_Address1);
				AssertEquals("Test Address2", org2.MainAddress.OA_Address2);
				AssertEquals("NSW", org2.MainAddress.OA_State);
				AssertEquals("AUSYD", org2.OH_RL_NKClosestPort);
				AssertEquals("2000", org2.MainAddress.OA_PostCode);
				AssertEquals("99994444", org2.MainAddress.OA_Phone);
				AssertEquals("99995555", org2.MainAddress.OA_Fax);
				AssertEquals("test@email.au", org2.MainAddress.OA_Email);
				AssertEquals("www.test.com.au", org2.MainWebURL.PU_URL);
				AssertEquals(true, org2.OH_IsActive);
				Assert("Organisation should be Debtor", org2.OH_IsDebtor);
				AssertEquals("ARTerms.Count", 2, org2.CompanyData.ARTerms.Count);
				AssertEquals("Standard Inv Type", "ALL", org2.CompanyData.ARTerms[0].PY_InvoiceClass);
				AssertEquals("Standard Inv Terms", "PER", org2.CompanyData.ARTerms[0].PY_InvoiceTerm);
				AssertEquals("Standard Inv Term Days", (short)14, org2.CompanyData.ARTerms[0].PY_InvoiceDays);
				AssertEquals("Disbursment Inv Type", "DSB", org2.CompanyData.ARTerms[1].PY_InvoiceClass);
				AssertEquals("Disbursment Inv Terms", "INV", org2.CompanyData.ARTerms[1].PY_InvoiceTerm);
				AssertEquals("Disbursment Inv Term Days", (short)7, org2.CompanyData.ARTerms[1].PY_InvoiceDays);
				AssertEquals("Organisation should NOT be Warehouse", false, org2.OH_IsWarehouseClient);
				AssertEquals(0, org2.Contacts.Count);
				AssertEquals("9 related parties", 9, org1.AllRelatedParties.Count);
				AssertEquals(org2.PK, org1.DeliveryFreightBillTo.PK);
				AssertEquals(org2.PK, org1.DeliveryCustomsBillTo.PK);
				AssertEquals(org2.PK, org1.PickupFreightBillTo.PK);
				AssertEquals(org2.PK, org1.PickupCustomsBillTo.PK);
				AssertEquals(org2.PK, org1.DeliverySeaCustomsBroker.PK);
				AssertEquals(org2.PK, org1.DeliveryAirCustomsBroker.PK);
				AssertEquals(org2.PK, org1.PickupSeaCustomsBroker.PK);
				AssertEquals(org2.PK, org1.PickupAirCustomsBroker.PK);
				AssertEquals("linked settlement group organisation", org2.PK, org1.ARSettlementGroupPK);

				OrgHeader org3 = createdOrgs[2];
				AssertEquals(1, org3.Contacts.Count);
				OrgContact contactWithNoEmail = org3.Contacts[0];
				AssertEquals(org3.PK, contactWithNoEmail.OC_OH);
				AssertEquals("", contactWithNoEmail.OC_Email);
				AssertEquals("PRN", contactWithNoEmail.OC_NotifyMode);
				AssertEquals("John Smith", contactWithNoEmail.OC_ContactName);
				AssertEquals("Manager", contactWithNoEmail.OC_Title);
				AssertEquals("98250011", contactWithNoEmail.OC_Phone);
				AssertEquals("01481123456", contactWithNoEmail.OC_Mobile);
				AssertEquals("98250013", contactWithNoEmail.OC_Fax);
				AssertEquals("Organisation should NOT be Warehouse", false, org2.OH_IsWarehouseClient);
			}
		}

		public void TestImportOrganizationWithoutBankAccountDetails()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					OutputHeaderLine(sw);
					sw.WriteLine("Z321Test2,Z321-Test Co Name,Address1,Address2,City,QLD,POST CODE,AUBNE,Country,Port City,PHONE,FAX,Email,Web,Business Reg No,Government Corporation Code,Y,Y,Y,Y,Y,Y,Y,N,Y,Y,Y,Y,Y,Contact Name,Contact Job Title,Contact Email,CONTACT WORK PHONE,CONTACT MOBILE,CONTACT FAX,Z321Test2,Debtor Acc Group,Z321Test2,HKD,1500,CR3,Y,,0,,0,CG1,Z321Test2,Postal Address1,Postal Address2,Postal City,Postal State,Postal Post Code,Deliver Address1,Deliver Address2,Deliver City,Deliver State,Deliver Post Code,,,,,Customs Client Code,Customs Supplier Code,Customs Supplier Code (CMR),Customs Controlled Premises Code,Customs Carrier Code,Customs Manifest Provider Code,Bad Work Notes,Nice Goods Handling Instructions,Great Import Delivery Instructions,Fantastic A/R Account Management Notes,Interesting A/R Credit Management Note,Useless A/P Account Management Notes,050908_EXI_PRS,20051225,Senor,,,,,,Y");
					sw.WriteLine("Z321Test1,Z321-Test Name,Address1,Address2,City,QLD,POST CODE,AUBNE,Country,Port City,PHONE,FAX,Email,Web,Business Reg No,Government Corporation Code,Y,Y,Y,Y,Y,Y,Y,N,Y,Y,Y,Y,Y,Contact Name,Contact Job Title,Contact Email,CONTACT WORK PHONE,CONTACT MOBILE,CONTACT FAX,Z321Test2,Debtor Acc Group,Z321Test2,HKD,1500,CR3,Y,,0,,0,CG1,Z321Test2,Postal Address1,Postal Address2,Postal City,Postal State,Postal Post Code,Deliver Address1,Deliver Address2,Deliver City,Deliver State,Deliver Post Code,Bank Name,Account Name,Account Number,BSB Number,Customs Client Code,Customs Supplier Code,Customs Supplier Code (CMR),Customs Controlled Premises Code,Customs Carrier Code,Customs Manifest Provider Code,Bad Work Notes,Nice Goods Handling Instructions,Great Import Delivery Instructions,Fantastic A/R Account Management Notes,Interesting A/R Credit Management Note,Useless A/P Account Management Notes,050908_EXI_PRS,20051225,Senor,,,,,,Y");
					sw.WriteLine("Z321Test3,Z321-TestOrg,Address1,Address2,City,QLD,POST CODE,AUBNE,Country,Port City,PHONE,FAX,Email,Web,Business Reg No,Government Corporation Code,Y,Y,Y,Y,Y,Y,Y,N,Y,Y,Y,Y,Y,Contact Name,Contact Job Title,Contact Email,CONTACT WORK PHONE,CONTACT MOBILE,CONTACT FAX,Z321Test2,Debtor Acc Group,Z321Test2,HKD,1500,CR3,Y,,0,,0,CG1,Z321Test2,Postal Address1,Postal Address2,Postal City,Postal State,Postal Post Code,Deliver Address1,Deliver Address2,Deliver City,Deliver State,Deliver Post Code,Bank Name,,Account Number,BSB Number,Customs Client Code,Customs Supplier Code,Customs Supplier Code (CMR),Customs Controlled Premises Code,Customs Carrier Code,Customs Manifest Provider Code,Bad Work Notes,Nice Goods Handling Instructions,Great Import Delivery Instructions,Fantastic A/R Account Management Notes,Interesting A/R Credit Management Note,Useless A/P Account Management Notes,050908_EXI_PRS,20051225,Senor,,,,,,Y");
					sw.Flush();
				}

				OrgHeader[] createdOrgs = FindOrgByFullNameStartsWith("Z321-Test");
				AssertEquals("There should have been 0 organisations created", 0, createdOrgs.Length);

				loader.ImportOrganisationData(testFileName.Filename, true);

				createdOrgs = FindOrgByFullNameStartsWith("Z321-Test");
				AssertEquals("There should have been 3 organisations created", 3, createdOrgs.Length);

				OrgHeader org0 = createdOrgs[0];
				AssertEquals(0, org0.CompanyData.AccountDetailsCollection.Count);

				OrgHeader org1 = createdOrgs[1];
				AssertEquals(1, org1.CompanyData.AccountDetailsCollection.Count);

				OrgHeader org2 = createdOrgs[2];
				AssertEquals(1, org2.CompanyData.AccountDetailsCollection.Count);
			}
		}

		protected virtual void DoImportForTestImportOrganisation(TempFile testFileName)
		{
			loader.ImportOrganisationData(testFileName.Filename, true);
			AssertEquals(1, loader.RunCounters.RecordsToImportPlusHeader);
			AssertEquals(4, loader.RunCounters.RecsCreated);
			AssertEquals(0, loader.RunCounters.RecsUpdated);
			AssertEquals(0, loader.RunCounters.RecsExcluded);
			AssertEquals(5, loader.Log.Count);
		}
		#endregion

		#region TestImportOrganisationLanguageCodes

		public void TestImportOrganisationLanguageCodes()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					OutputHeaderLine(sw);
					sw.WriteLine("Z321Test1,Z321-Test Name,Address1,Address2,City,QLD,Post Code,AUBNE,Country/Region,Port City,Phone,Fax,Email,Web,Business Reg No,Government Corporation Code,Y,Y,Y,Y,Y,Y,Y,N,Y,Y,Y,Y,Y,Contact Name,Contact Job Title,Contact Email,Contact Work Phone,Contact Mobile,Contact Fax,Z321Test2,Debtor Acc Group,Z321Test2,HKD,1500,CR3,Y,,0,,0,CG1,Z321Test2,Postal Address1,Postal Address2,Postal City,Postal State,Postal Post Code,Deliver Address1,Deliver Address2,Deliver City,Deliver State,Deliver Post Code,Bank Name,Account Name,Account Number,BSB Number,Customs Client Code,Customs Supplier Code,Customs Supplier Code (CMR),Customs Controlled Premises Code,Customs Carrier Code,Customs Manifest Provider Code,Work Notes,Goods Handling Instructions,Delivery Instructions,A/R Account Management Notes,A/R Credit Management Notes,A/P Account Management Notes,050908_EXI_PRS,20051225,Senor,EN-US,DE-DE,FR-FR,ZH-CN");
					sw.WriteLine("Z321Test2,Z321-Test Co Name,Test Address1,Test Address2, ,NSW,2000,AUSYD,AU,,99994444,99995555,test@email.au,www.test.com.au,999988885599,,Y,N,Y,Y,N,Y,N,N,N,N,N,N,N,,,,,,,,,,AUD,5000,CR2,Y,PER,14,INV,7,,,,,,,,,,,,,ANZ,,787458,012-045,,,,,,,Test notes,,,,,,25-12-2005,Monseiur,,DE-DE");
					sw.WriteLine("Z321Test3,Z321-TestOrg,Addr1,Addr2,City,,2020,AUSYD,AU,,98250011,,,,,,N,Y,N,N,N,N,N,N,N,N,N,N,N,John Smith,Manager,,98250011,01481123456,98250013,,,,AUD,1500,,Y,,0,,0,,,,,,,,,,,,,ANZ,,457983723,012-344,,,,,,,,,,,,,,,,,,");
					sw.Flush();
				}

				DoImportForTestImportOrganisationLanguageCodes(testFileName);

				OrgHeader[] organisationsCreated = FindOrgByFullNameStartsWith("Z321-Test");
				AssertEquals("There should have been 3 organisations created", 3, organisationsCreated.Length);

				OrgHeader org1 = FindOrgByFullName("Z321-Test Name");
				AssertEquals("Z321-Test Name", org1.OH_FullName);
				AssertEquals("EN-US", org1.OH_Language);
				AssertEquals("DE-DE", org1.Addresses[0].OA_Language);
				AssertEquals("ZH-CN", org1.Addresses[1].OA_Language);
				AssertEquals("FR-FR", org1.Addresses[2].OA_Language);

				OrgHeader org2 = FindOrgByFullName("Z321-Test Co Name");
				AssertEquals("DE-DE", org2.OH_Language);
				AssertEquals("EN", org2.Addresses[0].OA_Language);

				OrgHeader org3 = FindOrgByFullName("Z321-TestOrg");
				AssertEquals("EN", org3.OH_Language);
				AssertEquals("EN", org3.Addresses[0].OA_Language);
			}
		}

		protected virtual void DoImportForTestImportOrganisationLanguageCodes(TempFile testFileName)
		{
			loader.ImportOrganisationData(testFileName.Filename, true);
			AssertEquals(1, loader.RunCounters.RecordsToImportPlusHeader);
			AssertEquals(4, loader.RunCounters.RecsCreated);
			AssertEquals(0, loader.RunCounters.RecsUpdated);
			AssertEquals(0, loader.RunCounters.RecsExcluded);
			AssertEquals(5, loader.Log.Count);
		}

		#endregion

		#region TestImportOrganisationsWithInconsistentData

		public void TestImportOrganisationsWithInconsistentData()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					OutputHeaderLine(sw);
					sw.WriteLine(String.Format("Z321Test1,Z321-Test Co Name,Test Address1,Test Address2, ,NSW,2000,AUSYD,AU,,99994444,99995555,test@email.au,www.test.com.au,999988885599,,Y,Y,Y,Y,N,N,N,N,N,N,N,N,N,,,,,,,,,,AUD,5000,CR2,Y,PAY,,DSB,,CGN,,,,,,,,,,,,ANZ,,787458,012-045,,,,,,,Test notes,,,,,,,,"));
					sw.WriteLine(String.Format("Z321Test2,Z321-Test Data Missing Co.,15 Drewry Lane,Wolloomooloo, ,NSW,2000,AUSYD,AU,,87561122,87561123,email@email.au,www.test.com.au,99 123 456 789,,Y,N,Y,Y,N,N,N,N,N,N,N,,,,,,,,,,,,AUD,5000,,,,"));
					sw.WriteLine(String.Format("Z321TestAAA,This data is completely erroneous and should be excluded,Z321-Test Data Missing Co.,15 Drewry Lane,Wolloomooloo, ,NSW,2000,AUSYD,,,87561122,87561123,email@email.au,www.test.com.au,99 123 456 789,,Y,N,Y,Y,N,N,N,N,N,N,N,,,,,,,,,,,,AUD,5000,,,,"));
					sw.WriteLine(String.Format("Z321Test3,Z321-Test More Data P/L,1822 Hume Hwy,Liverpool, ,NSW,2726,AUSYD,AU,,95556622,95556633,test@email.au,www.test.com.au,99444555666,,Y,N,Y,Y,N,N,N,N,N,N,N,,,,,,,,,,,,AUD,5000,CR2,Y,,0,,0,,,,,,,,,,,,,NAB,OurCoAccount,278655,044-039,,,"));
					sw.Flush();
				}

				DoImportForTestImportOrganisationsWithInconsistentData(testFileName);

				OrgHeader[] organisations = FindOrgByFullNameStartsWith("Z321-Test");
				AssertEquals("There should have been 3 organisations created", 3, organisations.Length);

				OrgHeader org1 = organisations[0];
				AssertEquals("Z321-Test Co Name", org1.OH_FullName);
				AssertEquals("Test Address1", org1.MainAddress.OA_Address1);
				AssertEquals("Test Address2", org1.MainAddress.OA_Address2);
				AssertEquals("NSW", org1.MainAddress.OA_State);
				AssertEquals("AUSYD", org1.OH_RL_NKClosestPort);
				AssertEquals("2000", org1.MainAddress.OA_PostCode);
				AssertEquals("99994444", org1.MainAddress.OA_Phone);
				AssertEquals("99995555", org1.MainAddress.OA_Fax);
				AssertEquals("test@email.au", org1.MainAddress.OA_Email);
				AssertEquals("www.test.com.au", org1.MainWebURL.PU_URL);
				AssertEquals(true, org1.OH_IsActive);
				Assert("Organisation should be Debtor", org1.OH_IsDebtor);
				AssertEquals("ARTerms.Count", 1, org1.CompanyData.ARTerms.Count);
				AssertEquals("Standard Inv Type", "ALL", org1.CompanyData.ARTerms[0].PY_InvoiceClass);
				AssertEquals("Standard Inv Terms should default to COD when invalid", "COD", org1.CompanyData.ARTerms[0].PY_InvoiceTerm);
				AssertEquals("Standard Inv Term Days should default to 0 if blank", (short)0, org1.CompanyData.ARTerms[0].PY_InvoiceDays);
				AssertEquals(0, org1.Contacts.Count);
			}
		}

		protected virtual void DoImportForTestImportOrganisationsWithInconsistentData(TempFile testFileName)
		{
			loader.ImportOrganisationData(testFileName.Filename, true);
			AssertEquals(5, loader.RunCounters.RecordsToImportPlusHeader);
			AssertEquals(3, loader.RunCounters.RecsCreated);
			AssertEquals(0, loader.RunCounters.RecsUpdated);
			AssertEquals(1, loader.RunCounters.RecsExcluded);
			AssertEquals(3, loader.Log.Count);
		}

		#endregion

		#region TestImportOrganisationsWithInvalidNumericData
		public void TestImportOrganisationsWithInvalidNumericData()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					OutputHeaderLine(sw);
					sw.WriteLine(String.Format("Z321Test1,Z321-Test Invalid Credit Limit,Test Address1,Test Address2, ,NSW,2000,AUSYD,AU,,99994444,99995555,test@email.au,www.test.com.au,999988885599,,Y,Y,Y,Y,N,N,N,N,N,N,N,N,N,,,,,,,,,,AUD,invalid numeric field,CR2,Y,PAY,,DSB,,CGNew,,,,,,,,,,,,ANZ,,787458,012-045,,,,,,,Test notes,,,,,,,,"));
					sw.WriteLine(String.Format("Z321Test2,Z321-Test Invalid Std Inv Days,Test Address1,Test Address2, ,NSW,2000,AUSYD,AU,,99994444,99995555,test@email.au,www.test.com.au,999988885599,,Y,N,Y,Y,N,N,N,N,N,N,N,N,N,,,,,,,,,,AUD,5000,CR2,Y,PER,invalid numeric field,INV,7,,,,,,,,,,,,,ANZ,,787458,012-045,,,,,,,Test notes,,,,,,,,"));
					sw.WriteLine(String.Format("Z321Test2,Z321-Test Invalid Disbursement Inv Days,Test Address1,Test Address2, ,NSW,2000,AUSYD,AU,,99994444,99995555,test@email.au,www.test.com.au,999988885599,,Y,N,Y,Y,N,N,N,N,N,N,N,N,N,,,,,,,,,,AUD,5000,CR2,Y,PER,14,INV,invalid numeric field,,,,,,,,,,,,,ANZ,,787458,012-045,,,,,,,Test notes,,,,,,,,"));
					sw.Flush();
				}

				DoImportForTestImportOrganisationsWithInvalidNumericData(testFileName);

				TestImportOrganisationsWithInvalidNumericDataAsserts();
			}
		}

		protected virtual void TestImportOrganisationsWithInvalidNumericDataAsserts()
		{
			AssertEquals("Should be Invalid Credit Limit error:", "Row 2 excluded: data is inconsistent with required format - Field No. 40, CreditLimit", loader.Log[1]);
			AssertEquals("Should be Invalid Std Inv Days error:", "Row 3 excluded: data is inconsistent with required format - Field No. 44, Inv_Days_Standard", loader.Log[2]);
			AssertEquals("Should be Invalid Disb Inv Days error:", "Row 4 excluded: data is inconsistent with required format - Field No. 46, Inv_Days_Disbursement", loader.Log[3]);
		}

		protected virtual void DoImportForTestImportOrganisationsWithInvalidNumericData(TempFile testFileName)
		{
			loader.ImportOrganisationData(testFileName.Filename, true);
			AssertEquals(4, loader.RunCounters.RecordsToImportPlusHeader);
			AssertEquals(0, loader.RunCounters.RecsCreated);
			AssertEquals(0, loader.RunCounters.RecsUpdated);
			AssertEquals(3, loader.RunCounters.RecsExcluded);
			AssertEquals(5, loader.Log.Count);
		}
		#endregion

		#region TestImportDataWithNoOrgName

		public void TestImportDataWithNoOrgName()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					OutputHeaderLine(sw);
					sw.WriteLine("Z321Test1,,Address1,Address2,City,QLD,Post Code,AUBNE,Country/Region,Port City,Phone,Fax,Email,Web,Business Reg No,Government Corporation Code,Y,Y,Y,Y,Y,Y,Y,N,Y,Y,Y,Y,Y,Contact Name,Contact Job Title,Contact Email,Contact Work Phone,Contact Mobile,Contact Fax,Z321Test2,Debtor Acc Group,Z321Test2,HKD,1500,CR3,Y,,0,,0,CG1,Z321Test2,Postal Address1,Postal Address2,Postal City,Postal State,Postal Post Code,Deliver Address1,Deliver Address2,Deliver City,Deliver State,Deliver Post Code,Bank Name,Account Name,Account Number,BSB Number,Customs Client Code,Customs Supplier Code,Customs Supplier Code (CMR),Customs Controlled Premises Code,Customs Carrier Code,Customs Manifest Provider Code,Bad Work Notes,Nice Goods Handling Instructions,Great Import Delivery Instructions,Fantastic A/R Account Management Notes,Interesting A/R Credit Management Note,Useless A/P Account Management Notes,050908_EXI_PRS,20051225,Senor,,,,,,Y");
					sw.WriteLine("Z321Test2,Z321-TestOrg,Addr1,Addr2,City,,2020,AUSYD,AU,,98250011,,,,,,N,Y,N,N,N,N,N,N,N,N,N,N,N,John Smith,Manager,,98250011,01481123456,98250013,,,,AUD,1500,,Y,,0,,0,,,,,,,,,,,,,ANZ,,457983723,012-344,,,,,,,,,,,,,,");
					sw.Flush();
				}

				DoImportForTestImportDataWithNoOrgName(testFileName);
			}
		}

		protected virtual void DoImportForTestImportDataWithNoOrgName(TempFile testFileName)
		{
			loader.ImportOrganisationData(testFileName.Filename, true);
			AssertEquals(1, loader.RunCounters.RecordsToImportPlusHeader);
			AssertEquals(3, loader.RunCounters.RecsCreated);
			AssertEquals(0, loader.RunCounters.RecsUpdated);
			AssertEquals(0, loader.RunCounters.RecsExcluded);
			AssertEquals(7, loader.Log.Count);
		}

		#endregion

		#region TestImportDataWithNoOrgCodes

		public void TestImportDataWithNoOrgCodes()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					OutputHeaderLine(sw);
					sw.WriteLine(String.Format(",Z123-testdata:Atlab Image Co,87 Mars Rd,Lane Cove,,NSW,2066,,AU,,99060100-MakeSureThisGetsTruncatedTo20Chars,94188727,email@email.au,www.atlab.com.au,ABN,,,,,,,,,,,,,,,Ray Meaney-Test,CEO,ray_meaney@atlab.com.au,9906-0101,0418123456"));
					sw.WriteLine(String.Format(",Z123-testdata:Atari Australia pty ltd,32 Bowden St,Alexandria,,NSW,2015,,AU,,83036800,83036800,email,www.webaddress.au,00112345612,,,,,,,,,,,,,,,Leon Jenningswilliams,Director,leon.jennings@atari.com"));
					sw.WriteLine(String.Format(",Z123-testdata:Atari Aust.,55 Bowden St,Alexandria,,NSW,,,AU,2015,83036855,83036856,email,web,GSTNO,,,,,,,,,,,,,,,John Gibonssmithjones,Director of Financial Accounting and Management,contact email,contact phone,contact mobile"));
					//the following transaction should fail as it is a duplicate
					sw.WriteLine(String.Format(",Z123-testdata:Atlab Image Co,87 Mars Rd,Lane Cove,,NSW,2066,,AU,,99060100-MakeSureThisGetsTruncatedTo20Chars,94188727,email@email,www.atlab.com.au,001123456,,,,,,,,,,,,,,,Ray Meaney-Test,Mr,ray_meaney@atlab.com.au,99060101,0414123456"));
					//this one should be created as the address is different
					sw.WriteLine(String.Format(",Z123-testdata:Atlab Image Co,Warehouse 3,250 Botany Rd,Mascot,NSW,2022,,AU,,99060100-MakeSureThisGetsTruncatedTo20Chars,94188727,email@email,www.atlab.com.au,ABN,,,,,,,,,,,,,,,Ray Meaney-Test,Mr,ray_meaney@atlab.com.au,99060101,0414123456"));
					// this one should be created with AddressNotOnFile
					sw.WriteLine(String.Format(",Z123-testdata:Atlab Photo,Test Address1,Test Address2, ,NSW,2000,AUSYD,AU,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,"));
					// this one should not be created as it is a duplicate
					sw.WriteLine(String.Format(",Z123-testdata:Atlab Photo,Test Address1,Test Address2, ,NSW,2000,AUSYD,AU,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,Y,,,,,,,,,,,,,,,,,,,,,"));
					sw.Flush();
				}

				DoImportAndAssertForTestImportDataWithNoOrgCodes(testFileName);
				var factory = new BusinessObjectFactory();
				var checkFilter = new ZQuery(OrgHeaderSchema.OH_FullName, SQLComparisonOperator.StartsWith, "Z123-testdata:");
				BusinessObject[] enterpriseOrganisationsCreated = factory.Load(typeof(OrgHeader), checkFilter);
				AssertEquals("There should have been 5 organisations created", 5, enterpriseOrganisationsCreated.Length);

				// update again with same file should not create any new organisations
				DoImportAndAssertForTestImportDataWithNoOrgCodesAgain(testFileName);
				checkFilter = new ZQuery(OrgHeaderSchema.OH_FullName, SQLComparisonOperator.StartsWith, "Z123-testdata:");
				enterpriseOrganisationsCreated = factory.Load(typeof(OrgHeader), checkFilter);
				AssertEquals("There should still have only been 5 organisations created", 5, enterpriseOrganisationsCreated.Length);

				var orgFilter = new ZQuery(OrgHeaderSchema.OH_FullName, "Z123-testdata:Atlab Image Co");
				var organisations = (OrgHeader[])factory.Load(typeof(OrgHeader), orgFilter);
				Assert("Should have been 2 organisations with this name created", organisations.Length == 2);
				Assert("Companies with same name need to create unique short codes", organisations[0].OH_Code != organisations[1].OH_Code);

				orgFilter = new ZQuery(OrgHeaderSchema.OH_FullName, "Z123-testdata:Atlab Photo");
				organisations = factory.Load<OrgHeader>(orgFilter);
				AssertEquals(1, organisations.Length);

				var orgWithNoDuplicates = organisations[0];
				AssertEquals("Z123-testdata:Atlab Photo", orgWithNoDuplicates.OH_FullName);
				AssertEquals("Test Address1", orgWithNoDuplicates.MainAddress.OA_Address1);
				AssertEquals("Test Address2", orgWithNoDuplicates.MainAddress.OA_Address2);
				AssertEquals("NSW", orgWithNoDuplicates.MainAddress.OA_State);
				AssertEquals("AUSYD", orgWithNoDuplicates.OH_RL_NKClosestPort);
				AssertEquals("2000", orgWithNoDuplicates.MainAddress.OA_PostCode);
			}
		}

		protected virtual void DoImportAndAssertForTestImportDataWithNoOrgCodes(TempFile testFileName)
		{
			loader.ImportOrganisationData(testFileName.Filename, true);
			AssertEquals(8, loader.RunCounters.RecordsToImportPlusHeader);
			AssertEquals(5, loader.RunCounters.RecsCreated);
			AssertEquals(0, loader.RunCounters.RecsUpdated);
			AssertEquals(2, loader.RunCounters.RecsExcluded);
			AssertEquals(4, loader.Log.Count);
		}

		protected virtual void DoImportAndAssertForTestImportDataWithNoOrgCodesAgain(TempFile testFileName)
		{
			var newLoader = new OrgDataLoadForTest();
			newLoader.ImportOrganisationData(testFileName.Filename, true);
			AssertEquals(8, newLoader.RunCounters.RecordsToImportPlusHeader);
			AssertEquals(0, newLoader.RunCounters.RecsCreated);
			AssertEquals(0, newLoader.RunCounters.RecsUpdated);
			AssertEquals(7, newLoader.RunCounters.RecsExcluded);
			AssertEquals(9, newLoader.Log.Count);
		}

		#endregion

		#region TestOrgCodeIsUnique

		public void TestOrgCodeIsUnique()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					OutputHeaderLine(sw);
					sw.WriteLine(String.Format("001,Z123-testdata:Atari Australia pty ltd,32 Bowden St,Alexandria,Sydney,NSW,2015,,AU,,83036800,83036800,email,www.webaddress.au,00112345612,,,"));
					sw.WriteLine(String.Format("002,Z123-testdata:Atari Aust.,55 Bowden St,Alexandria,,NSW,2015,,AU,,83036855,83036856,email,web,GSTNO,,,"));
					sw.WriteLine(String.Format(",Z123-testdata:Atari Aus P/L,22 Bowden St,Alexandria,Sydney, ,2015,,AU,,83036800,83036800,email,www.webaddress.au,00112345612,,,"));
					sw.WriteLine(String.Format(",Z123-testdata:Atari Austria,17 Lonestrausser,Vienna,, ,17905,,AU,,+ 22 49036855,83036856,email,web,GSTNO,,,"));
					sw.Flush();
				}

				DoImportForTestOrgCodeIsUnique(testFileName);

				ZQuery checkFilter = new ZQuery(OrgHeaderSchema.OH_FullName, SQLComparisonOperator.StartsWith, "Z123-testdata:");
				BusinessObject[] enterpriseOrganisationsCreated = Factory.Load(typeof(OrgHeader), checkFilter);
				AssertEquals("There should have been 4 organisations created", 4, enterpriseOrganisationsCreated.Length);

				Assert("Companies with like names need to create unique short codes", LoadOrganisation("Z123-testdata:Atari Australia pty ltd", "32 Bowden St").OH_Code != LoadOrganisation("Z123-testdata:Atari Aust.", "55 Bowden St").OH_Code);
				Assert("Companies with like names need to create unique short codes - manually generated", LoadOrganisation("Z123-testdata:Atari Aus P/L", "22 Bowden St").OH_Code != LoadOrganisation("Z123-testdata:Atari Austria", "17 Lonestrausser").OH_Code);
			}
		}

		protected virtual void DoImportForTestOrgCodeIsUnique(TempFile testFileName)
		{
			loader.ImportOrganisationData(testFileName.Filename, true);
			AssertEquals(5, loader.RunCounters.RecordsToImportPlusHeader);
			AssertEquals(4, loader.RunCounters.RecsCreated);
			AssertEquals(0, loader.RunCounters.RecsUpdated);
			AssertEquals(0, loader.RunCounters.RecsExcluded);
			AssertEquals(2, loader.Log.Count);
		}

		#endregion

		#region TestImportOrganisationWithDataExceededMaxLength
		public void TestImportOrganisationWithDataExceededMaxLength()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					OutputHeaderLine(sw);
					sw.WriteLine(String.Format("Z321Test1,Z321-Test Co Name,Test Address1,Test Address2, ,NSW,2000,AUSYD,AU,,99994444,99995555,test@email.au,www.test.com.au,999988885599,,Y,Y,Y,Y,N,N,N,N,N,N,N,N,N,JohnSmithJnr,,,,,,,,,AUD,5000,CR2,Y,PAY,,DSB,,CGNew,,,,,,,,,,,,Account Name Goes Here 25 THIS TEXT SHOULD BE TRIMMED OFF,Account Name Goes Here 25 THIS TEXT SHOULD BE TRIMMED OFF,787458,012-045,,,,,,,Test notes,,,,,,Max of 20 characters THIS TEXT SHOULD BE TRIMMED OFF,,A max 50 characters is permitted for this property THIS TEXT SHOULD BE TRIMMED OFF,EN-US,MAX TRIM"));
					sw.Flush();
				}

				DoImportForTestImportOrganisationWithDataExceededMaxLength(testFileName);

				ZQuery checkFilter = new ZQuery(OrgHeaderSchema.OH_FullName, SQLComparisonOperator.StartsWith, "Z321-Test");
				BusinessObject[] enterpriseOrganisationsCreated = Factory.Load(typeof(OrgHeader), checkFilter);
				AssertEquals("There should have been one organisations created", 1, enterpriseOrganisationsCreated.Length);

				Assert(enterpriseOrganisationsCreated[0] is OrgHeader);

				OrgHeader org1 = LoadOrganisation("Z321-Test Co Name", "Test Address1");
				AssertEquals("Z321-Test Co Name", org1.OH_FullName);
				AssertEquals("Test Address1", org1.MainAddress.OA_Address1);
				AssertEquals("Test Address2", org1.MainAddress.OA_Address2);
				AssertEquals("NSW", org1.MainAddress.OA_State);
				AssertEquals("AUSYD", org1.OH_RL_NKClosestPort);
				AssertEquals("2000", org1.MainAddress.OA_PostCode);
				AssertEquals("99994444", org1.MainAddress.OA_Phone);
				AssertEquals("99995555", org1.MainAddress.OA_Fax);
				AssertEquals("test@email.au", org1.MainAddress.OA_Email);
				AssertEquals("www.test.com.au", org1.MainWebURL.PU_URL);
				AssertEquals(true, org1.OH_IsActive);
				AssertEquals("Field must be trimmed to max length, 25", "Account Name Goes Here 25", org1.MiscServ.OM_ARPreviousChequeDrawerBank.ToString());
				AssertEquals("Field must be trimmed to max length, 25", "Account Name Goes Here 25", org1.MiscServ.OM_ARPreviousChequeDrawerBankBranch.ToString());
				AssertEquals("Field must be trimmed to max length, 7", "EN-US", org1.OH_Language.ToString());
				AssertEquals("Field must be trimmed to max length, 7", "MAX TRI", org1.MainAddress.OA_Language.ToString());

				AssertEquals(1, org1.Contacts.Count);
				ZQuery contactFilter = new ZQuery(OrgContactSchema.OC_ContactName, "JohnSmithJnr");
				contactFilter.AddToFilter(OrgContactSchema.OC_OH, org1.PK);
				OrgContact organisationContact = Factory.LoadTop1<OrgContact>(contactFilter);
				AssertEquals("Field must be trimmed to max length, 20", "Max of 20 characters", organisationContact.OC_ContactSource.ToString());
				AssertEquals("Field must be trimmed to max length, 50", "A max 50 characters is permitted for this property", organisationContact.OC_Salutation.ToString());
			}
		}

		protected virtual void DoImportForTestImportOrganisationWithDataExceededMaxLength(TempFile testFileName)
		{
			loader.ImportOrganisationData(testFileName.Filename, true);
			AssertEquals(2, loader.RunCounters.RecordsToImportPlusHeader);
			AssertEquals(1, loader.RunCounters.RecsCreated);
			AssertEquals(0, loader.RunCounters.RecsUpdated);
			AssertEquals(0, loader.RunCounters.RecsExcluded);
			AssertEquals(2, loader.Log.Count);
		}
		#endregion

		#region TestGenerationOfPortCode
		public void TestGenerationOfPortCode()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					OutputHeaderLine(sw);
					sw.WriteLine(String.Format("Z321Test1,Z321-Test AUSYD,Test Address1,Test Address2,,NSW,2000,,Australia,Sydney,99994444,99995555,test@email.au,www.test.com.au,999988885599,,Y,N,Y,Y,N,N,N,N,N,N,N,N,N,,,,,,,,,,AUD,5000,CR2,Y,,0,,0,,,,,,,,,,,,,ANZ,,787458,012-045,,,,,,,Test notes,,,,,,,,"));
					sw.WriteLine(String.Format("Z321Test2,Z321-Test AUBTY,15 Drewry Lane,Wolloomooloo,,NSW,2000,,AUSTRALIA,PORT BOTANY,87561122,87561123,email@email.au,www.test.com.au,99 123 456 789,,Y,N,Y,Y,N,N,N,N,N,N,N,,,,,,,,,,,,AUD,5000,,,,"));
					sw.WriteLine(String.Format("Z321Test3,Z321-Test NZ,1822 Airport Dve.,Mangere,Auckland,,,,New Zealand,,95556622,95556633,test@email.au,www.test.com.au,99444555666,,Y,N,Y,Y,N,N,N,N,N,N,N,,,,,,,,,,,,AUD,5000,CR2,Y,,0,,0,,,,,,,,,,,,,NAB,OurCoAccount,278655,044-039,,,"));
					sw.WriteLine(String.Format("Z321Test4,Z321-Test No Port,17 George St,Sydney,,,,,AU,,95556622,95556633,test@email.au,www.test.com.au,99444555666,,Y,N,Y,Y,N,N,N,N,N,N,N,,,,,,,,,,,,AUD,5000,CR2,Y,,0,,0,,,,,,,,,,,,,NAB,OurCoAccount,278655,044-039,,,"));
					sw.WriteLine(String.Format("Z321Test5,Z321-Test Invalid Country/Region,1 Martin Pl.,Sydney,,,,,Aust.,,95556622,95556633,test@email.au,www.test.com.au,99444555666,,Y,N,Y,Y,N,N,N,N,N,N,N,,,,,,,,,,,,AUD,5000,CR2,Y,,0,,0,,,,,,,,,,,,,NAB,OurCoAccount,278655,044-039,,,"));
					sw.WriteLine(String.Format("Z321Test6,Z321-Test ISO Country/Region Code,100 Martin Pl.,Sydney,,,,,AU,,95556622,95556633,test@email.au,www.test.com.au,99444555666,,Y,N,Y,Y,N,N,N,N,N,N,N,,,,,,,,,,,,AUD,5000,CR2,Y,,0,,0,,,,,,,,,,,,,NAB,OurCoAccount,278655,044-039,,,"));
					sw.WriteLine(String.Format("Z321Test7,Z321-Test ISO Country/Region Code & Port,100 St. Georges Tce.,Perth,,WA,6000,,AU,Perth,99994444,99995555,test@email.au,www.test.com.au,999988885599,,Y,N,Y,Y,N,N,N,N,N,N,N,N,N,,,,,,,,,,AUD,5000,CR2,Y,,0,,0,,,,,,,,,,,,,ANZ,,787458,012-045,,,,,,,Test notes,,,,,,,,"));
					sw.WriteLine(String.Format("Z321Test8,Z321-Test No Port just State,20 Flinders Ln.,Melbourne,,Vic.,,,AU,,95556622,95556633,test@email.au,www.test.com.au,99444555666,,Y,N,Y,Y,N,N,N,N,N,N,N,,,,,,,,,,,,AUD,5000,CR2,Y,,0,,0,,,,,,,,,,,,,NAB,OurCoAccount,278655,044-039,,,"));
					sw.WriteLine(String.Format("Z321Test9,Z321-Test Valid Country/Region Invalid City,20 Flinders Ln.,Melbourne,,,,,AU,Mel,95556622,95556633,test@email.au,www.test.com.au,99444555666,,Y,N,Y,Y,N,N,N,N,N,N,N,,,,,,,,,,,,AUD,5000,CR2,Y,,0,,0,,,,,,,,,,,,,NAB,OurCoAccount,278655,044-039,,,"));
					sw.Flush();
				}

				DoImportForTestGenerationOfPortCode(testFileName);

				OrgHeader[] organisations = FindOrgByFullNameStartsWith("Z321-Test");
				AssertEquals("There should have been 9 organisations created", 9, organisations.Length);
				AssertEquals("AUSYD", LoadOrganisation("Z321-Test AUSYD", "Test Address1").OH_RL_NKClosestPort);
				AssertEquals("AUPBT", LoadOrganisation("Z321-Test AUBTY", "15 Drewry Lane").OH_RL_NKClosestPort);
				AssertEquals("NZZZZ", LoadOrganisation("Z321-Test NZ", "1822 Airport Dve.").OH_RL_NKClosestPort);
				AssertEquals("AUZZZ", LoadOrganisation("Z321-Test No Port", "17 George St").OH_RL_NKClosestPort);
				AssertEquals("ZZZZZ", LoadOrganisation("Z321-Test Invalid Country/Region", "1 Martin Pl.").OH_RL_NKClosestPort);
				AssertEquals("AUZZZ", LoadOrganisation("Z321-Test ISO Country/Region Code", "100 Martin Pl.").OH_RL_NKClosestPort);
				AssertEquals("AUPER", LoadOrganisation("Z321-Test ISO Country/Region Code & Port", "100 St. Georges Tce.").OH_RL_NKClosestPort);
				AssertEquals("AUMEL", LoadOrganisation("Z321-Test No Port just State", "20 Flinders Ln.").OH_RL_NKClosestPort);
				AssertEquals("AUZZZ", LoadOrganisation("Z321-Test Valid Country/Region Invalid City", "20 Flinders Ln.").OH_RL_NKClosestPort);
			}
		}

		protected virtual void DoImportForTestGenerationOfPortCode(TempFile testFileName)
		{
			loader.ImportOrganisationData(testFileName.Filename, true);
			AssertEquals(10, loader.RunCounters.RecordsToImportPlusHeader);
			AssertEquals(9, loader.RunCounters.RecsCreated);
			AssertEquals(0, loader.RunCounters.RecsUpdated);
			AssertEquals(0, loader.RunCounters.RecsExcluded);
			AssertEquals(2, loader.Log.Count);
		}
		#endregion

		#region TestDrAndCrGroupsAreCreated
		public void TestDrAndCrGroupsAreCreated()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					OutputHeaderLine(sw);
					sw.WriteLine(String.Format("Z321Test1,Z321-Test Co Name,Test Address1,Address2,City,QLD,Post Code,AUBNE,Country/Region,Port City,Phone,Fax,Email,Web,Business Reg No,Government Corporation Code,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Contact Name,Contact Job Title,Contact Email,Contact Work Phone,Contact Mobile,Contact Fax,Z321Test2,ARG,Z321Test2,HKD,1500,CR3,Y,,0,,0,APG,Z321Test2,Postal Address1,Postal Address2,Postal City,Postal State,Postal Post Code,Deliver Address1,Deliver Address2,Deliver City,Deliver State,Deliver Post Code,Bank Name,Account Name,Account Number,BSB Number,Customs Client Code,Customs Supplier Code,Customs Supplier Code (CMR),Customs Controlled Premises Code,Customs Carrier Code,Customs Manifest Provider Code,Work Notes,Goods Handling Instructions,Delivery Instructions,A/R Account Management Notes,A/R Credit Management Notes,A/P Account Management Notes,,,"));
					sw.Flush();
				}

				DoImportForTestDrAndCrGroupsAreCreated(testFileName);
				OrgHeader org1 = LoadOrganisation("Z321-Test Co Name", "Test Address1");

				Assert("Debtor Group should have been created & linked", !org1.CompanyData.OB_OJ_ARDebtorGroup.IsEmpty);
				OrgDebtorGroup drGroup = Factory.Load<OrgDebtorGroup>(org1.CompanyData.OB_OJ_ARDebtorGroup);
				AssertEquals("Debtor Group Code", "ARG", drGroup.OJ_Code);

				Assert("Creditor Group should have been created & linked", !org1.CompanyData.OB_OG_APCreditorGroup.IsEmpty);
				OrgCreditorGroup crGroup = Factory.Load<OrgCreditorGroup>(org1.CompanyData.OB_OG_APCreditorGroup);
				AssertEquals("Creditor Group Code", "APG", crGroup.OG_Code);
			}
		}

		protected virtual void DoImportForTestDrAndCrGroupsAreCreated(TempFile testFileName)
		{
			loader.ImportOrganisationData(testFileName.Filename, true);
		}
		#endregion

		#region TestDrAndCrGroupsAreObtainedFromRegistryDefault
		public void TestDrAndCrGroupsAreObtainedFromRegistryDefault()
		{
			OrganisationsDataRegistry.Instance.UseARSettlementGroupCreditLimit.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.ARAccountGroup).Returns(DebtorGroup.PK.ToGuid());
			mock.Setup(m => m.APAccountGroup).Returns(CreditorGroup.PK.ToGuid());
			using (ObjectFactory.Substitute(mock.Object))
			{
				using (TempFile testFileName = TempFile.New())
				{
					using (StreamWriter sw = new StreamWriter(testFileName.Filename))
					{
						OutputHeaderLine(sw);
						sw.WriteLine(String.Format("Z321Test1,Z321-Test Co Name,Test Address1,Address2,City,QLD,Post Code,AUBNE,Country/Region,Port City,Phone,Fax,Email,Web,Business Reg No,Government Corporation Code,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Contact Name,Contact Job Title,Contact Email,Contact Work Phone,Contact Mobile,Contact Fax,Z321Test2,,Z321Test2,HKD,1500,CR3,Y,,0,,0,,Z321Test2,Postal Address1,Postal Address2,Postal City,Postal State,Postal Post Code,Deliver Address1,Deliver Address2,Deliver City,Deliver State,Deliver Post Code,Bank Name,Account Name,Account Number,BSB Number,Customs Client Code,Customs Supplier Code,Customs Supplier Code (CMR),Customs Controlled Premises Code,Customs Carrier Code,Customs Manifest Provider Code,Work Notes,Goods Handling Instructions,Delivery Instructions,A/R Account Management Notes,A/R Credit Management Notes,A/P Account Management Notes,,,"));
						sw.Flush();
					}

					DoImportForTestDrAndCrGroupsAreObtainedFromRegistryDefault(testFileName);
					OrgHeader org1 = LoadOrganisation("Z321-Test Co Name", "Test Address1");

					Assert("Debtor Group should have been linked to registry default", !org1.CompanyData.OB_OJ_ARDebtorGroup.IsEmpty);
					AssertEquals("Should be linked to registry DR Group", DebtorGroup.PK, org1.CompanyData.OB_OJ_ARDebtorGroup);

					Assert("Creditor Group should have been linked to registry default", !org1.CompanyData.OB_OG_APCreditorGroup.IsEmpty);
					AssertEquals("Should be linked to registry CR Group", CreditorGroup.PK, org1.CompanyData.OB_OG_APCreditorGroup);
				}
			}
		}

		protected virtual void DoImportForTestDrAndCrGroupsAreObtainedFromRegistryDefault(TempFile testFileName)
		{
			loader.ImportOrganisationData(testFileName.Filename, true);
		}

		public void TestDefaultDebtorGroup()
		{
			OrgDebtorGroup debtor = Factory.LoadFromNaturalKey<OrgDebtorGroup>(OrgDebtorGroupSchema.OJ_Code, "INT");
			var debtorPK = debtor.PK.ToString();
			debtorPK.ToUpper();

			StmData stmData = Factory.New<StmData>();
			stmData.SD_Name = "ARAccountGroup";
			stmData.SD_Type = RegistryDataTypes.GuidType.Code;
			stmData.SD_IsLogged = false;
			stmData.SD_IsCancelled = false;
			stmData.SD_GuidValue = debtor.PK;
			stmData.SD_BinaryValue = new ZBlob(System.Text.Encoding.UTF8.GetBytes(stmData.SD_GuidValue.ToString()));
			Factory.Save();

			CargoWise.Data.DbCommand cmd = TestConnection.Command("UPDATE dbo.StmData SET SD_GuidValue = '" + debtorPK + "', SD_BinaryValue = cast(cast(SD_GuidValue AS NVARCHAR(MAX)) AS VARBINARY(MAX)) WHERE SD_Name = 'ARACCOUNTGROUP'");
			cmd.ExecuteNonQuery();

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					OutputHeaderLine(sw);
					sw.WriteLine("Z321Test1,Z321-Test Co Name,Address1,Address2,Brisbane,QLD,4000,,Australia,Port City,Phone,Fax,Email,Web,Business Reg No,Government Corporation Code,Y,Y,Y,Y,Y,Y,Y,N,Y,Y,Y,Y,Y,Contact Name,Contact Job Title,Contact Email,Contact Work Phone,Contact Mobile,Contact Fax,Z321Test2,,Z321Test2,HKD,1500,CR3,Y,,0,,0,CG1,Z321Test2,Postal Address1,Postal Address2,Postal City,Postal State,Postal Post Code,Deliver Address1,Deliver Address2,Deliver City,Deliver State,Deliver Post Code,Bank Name,Account Name,Account Number,BSB Number,Customs Client Code,Customs Supplier Code,Customs Supplier Code (CMR),Customs Controlled Premises Code,Customs Carrier Code,Customs Manifest Provider Code,Bad Work Notes,Nice Goods Handling Instructions,Great Import Delivery Instructions,Fantastic A/R Account Management Notes,Interesting A/R Credit Management Note,Useless A/P Account Management Notes,050908_EXI_PRS,20051225,Senor,,,,,,Y");
					sw.WriteLine("Z321Test2,Z321-Test Name,Test Address1,Test Address2, ,NSW,2000,,AU,,99994444,99995555,test@email.au,www.test.com.au,999988885599,,Y,N,Y,Y,N,Y,N,N,N,N,N,N,N,,,,,,,,,,AUD,5000,CR2,Y,PER,14,INV,7,,,,,,,,,,,,,ANZ,,787458,012-045,,,,,,,Test notes,,,,,,25-12-2005,Monseiur,,,,,,N");
					sw.WriteLine("Z321Test3,Z321-TestOrg,Addr1,Addr2,City,,2020,AUSYD,AU,,98250011,,,,,,N,Y,N,N,N,N,N,N,N,N,N,N,N,John Smith,Manager,,98250011,01481123456,98250013,,,,AUD,1500,,Y,,0,,0,,,,,,,,,,,,,ANZ,,457983723,012-344,,,,,,,,,,,,,,");
					sw.Flush();
				}

				DoImportForTestImportOrganisation(testFileName);

				OrgHeader[] createdOrgs = FindOrgByFullNameStartsWith("Z321-Test");
				AssertEquals("There should have been 3 organisations created", 3, createdOrgs.Length);

				OrgHeader org1 = createdOrgs[0];
				AssertEquals("Z321-Test Co Name", org1.OH_FullName);
				AssertNotEquals(Guid.Empty, org1.MiscServ.OM_OJ_ARDebtorGroup);
				AssertEquals("INT", Factory.Load<OrgDebtorGroup>(org1.MiscServ.OM_OJ_ARDebtorGroup).OJ_Code);
				OrgARTerms termsAR = org1.CompanyData.CreateOrLoadARTerm(OrgARTermsLookups.InvoiceTypes.All.Code);
				AssertNotNull(termsAR);
				AssertEquals("COD", termsAR.PY_InvoiceTerm);
				AssertEquals((short)0, termsAR.PY_InvoiceDays);
				termsAR = org1.CompanyData.CreateOrLoadARTerm(OrgARTermsLookups.InvoiceTypes.DSB.Code);
				AssertNotNull(termsAR);
				AssertEquals("COD", termsAR.PY_InvoiceTerm);
				AssertEquals((short)0, termsAR.PY_InvoiceDays);
			}
		}

		public void TestDefaultCreditorGroup()
		{
			OrgCreditorGroup creditor = Factory.LoadFromNaturalKey<OrgCreditorGroup>(OrgCreditorGroupSchema.OG_Code, "INT");
			var creditorPK = creditor.PK.ToString();
			creditorPK.ToUpper();

			StmData stmData = Factory.New<StmData>();
			stmData.SD_Name = "APAccountGroup";
			stmData.SD_Type = RegistryDataTypes.GuidType.Code;
			stmData.SD_IsLogged = false;
			stmData.SD_IsCancelled = false;
			stmData.SD_GuidValue = creditor.PK;
			stmData.SD_BinaryValue = new ZBlob(System.Text.Encoding.UTF8.GetBytes(stmData.SD_GuidValue.ToString()));
			Factory.Save();

			CargoWise.Data.DbCommand cmd = TestConnection.Command("UPDATE dbo.StmData SET SD_GuidValue = '" + creditorPK + "',SD_BinaryValue = cast(cast(SD_GuidValue AS NVARCHAR(MAX)) AS VARBINARY(MAX)) WHERE SD_Name = 'APACCOUNTGROUP'");
			cmd.ExecuteNonQuery();

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					OutputHeaderLine(sw);
					sw.WriteLine("Z321Test1,Z321-Test Co Name,Address1,Address2,Brisbane,QLD,4000,,Australia,Port City,Phone,Fax,Email,Web,Business Reg No,Government Corporation Code,Y,Y,Y,Y,Y,Y,Y,N,Y,Y,Y,Y,Y,Contact Name,Contact Job Title,Contact Email,Contact Work Phone,Contact Mobile,Contact Fax,,CG1,,HKD,1500,CR3,Y,,0,,0,,Z321Test2,Postal Address1,Postal Address2,Postal City,Postal State,Postal Post Code,Deliver Address1,Deliver Address2,Deliver City,Deliver State,Deliver Post Code,Bank Name,Account Name,Account Number,BSB Number,Customs Client Code,Customs Supplier Code,Customs Supplier Code (CMR),Customs Controlled Premises Code,Customs Carrier Code,Customs Manifest Provider Code,Bad Work Notes,Nice Goods Handling Instructions,Great Import Delivery Instructions,Fantastic A/R Account Management Notes,Interesting A/R Credit Management Note,Useless A/P Account Management Notes,050908_EXI_PRS,20051225,Senor,,,,,,Y");
					sw.WriteLine("Z321Test2,Z321-Test Name,Test Address1,Test Address2, ,NSW,2000,,AU,,99994444,99995555,test@email.au,www.test.com.au,999988885599,,Y,N,Y,Y,N,Y,N,N,N,N,N,N,N,,,,,,,,CG1,,AUD,5000,CR2,Y,PER,14,INV,7,,,,,,,,,,,,,ANZ,,787458,012-045,,,,,,,Test notes,,,,,,25-12-2005,Monseiur,,,,,,N");
					sw.WriteLine("Z321Test3,Z321-TestOrg,Addr1,Addr2,City,,2020,AUSYD,AU,,98250011,,,,,,Y,N,Y,Y,N,Y,N,N,N,N,N,N,N,,,,,,,,CG1,,AUD,5000,CR2,Y,PER,14,INV,7,,,,,,,,,,,,,ANZ,,787458,012-045,,,,,,,Test notes,,,,,,25-12-2005,Monseiur,,,,,,N");
					sw.Flush();
				}

				DoImportForTestImportOrganisation(testFileName);

				OrgHeader[] createdOrgs = FindOrgByFullNameStartsWith("Z321-Test");
				AssertEquals("There should have been 3 organisations created", 3, createdOrgs.Length);

				OrgHeader org1 = createdOrgs[0];
				AssertEquals("Z321-Test Co Name", org1.OH_FullName);
				AssertNotEquals(Guid.Empty, org1.MiscServ.OM_OG_APCreditorGroup);
				AssertEquals("INT", (Factory.Load<OrgCreditorGroup>(org1.MiscServ.OM_OG_APCreditorGroup)).OG_Code);
			}
		}

		#endregion

		#region TestUSIdentificationCodes
		public void TestUSIdentificationCodes()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.UnitedStates);

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					OutputHeaderLine(sw);
					sw.WriteLine(String.Format("NAOMI-EIN,TEST ORG WITH EIN,TEST ADDRESS,,CHICAGO,IL,60056,USCHI,USA,CHICAGO,18473645600,,,www.cargowise.com,24-587345000,,Y,Y,Y,Y,Y,Y,,,,,,,,Naomi Black,CEO,naomi.black@cargowise.com,18474340176,13122152110,,,TPY,,USD,,,,INV,7,COD,,TPY,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,"));
					sw.WriteLine(String.Format("NAOMI-SSN,TEST ORG WITH SSN,TEST ADDRESS,,CHICAGO,IL,60056,USCHI,USA,CHICAGO,18473645600,,,www.cargowise.com,123-12-1234,,Y,Y,Y,Y,Y,Y,,,,,,,,Naomi Black,CEO,naomi.black@cargowise.com,18474340176,13122152110,,,TPY,,USD,,,,INV,7,COD,,TPY,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,"));
					sw.WriteLine(String.Format("NAOMI-CBP,TEST ORG WITH CBP,TEST ADDRESS,,CHICAGO,IL,60056,USCHI,USA,CHICAGO,18473645600,,,www.cargowise.com,061234-12345,,Y,Y,Y,Y,Y,Y,,,,,,,,Naomi Black,CEO,naomi.black@cargowise.com,18474340176,13122152110,,,TPY,,USD,,,,INV,7,COD,,TPY,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,"));
					sw.WriteLine(String.Format("NAOMI-UNK,TEST ORG UNKNOWN CODE,TEST ADDRESS,,CHICAGO,IL,60056,USCHI,USA,CHICAGO,18473645600,,,www.cargowise.com,06123412345,,Y,Y,Y,Y,Y,Y,,,,,,,,Naomi Black,CEO,naomi.black@cargowise.com,18474340176,13122152110,,,TPY,,USD,,,,INV,7,COD,,TPY,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,"));
					sw.Flush();
				}

				DoImportForTestUSIdentificationCodes(testFileName);

				OrgHeader[] organisations = FindOrgByFullNameStartsWith("TEST ORG");
				AssertEquals("There should have been 4 organisations created", 4, organisations.Length);

				OrgHeader org1 = LoadOrganisation("TEST ORG WITH EIN", "TEST ADDRESS");
				AssertNotNull("Org Data line 1 imported", org1);
				AssertEquals(2, org1.CustomsCodes.Count);
				AssertEquals("US", org1.CustomsCodes[0].OK_RN_NKCodeCountry);
				AssertEquals("Ord ID should be EIN", OrgCusCode.USACodeTypes.EmployerIdentificationNumber, org1.CustomsCodes[0].OK_CodeType);

				OrgHeader org2 = LoadOrganisation("TEST ORG WITH SSN", "TEST ADDRESS");
				AssertNotNull("Org Data line 2 imported", org2);
				AssertEquals(2, org2.CustomsCodes.Count);
				AssertEquals("US", org2.CustomsCodes[0].OK_RN_NKCodeCountry);
				AssertEquals("Org ID should be SSN", OrgCusCode.USACodeTypes.SocialSecurityNumber, org2.CustomsCodes[0].OK_CodeType);

				OrgHeader org3 = LoadOrganisation("TEST ORG WITH CBP", "TEST ADDRESS");
				AssertNotNull("Org Data line 3 imported", org3);
				AssertEquals(2, org3.CustomsCodes.Count);
				AssertEquals("US", org3.CustomsCodes[0].OK_RN_NKCodeCountry);
				AssertEquals("Org ID should be CBP No", OrgCusCode.USACodeTypes.CBPAssignedNumber, org3.CustomsCodes[0].OK_CodeType);

				OrgHeader org4 = LoadOrganisation("TEST ORG UNKNOWN CODE", "TEST ADDRESS");
				AssertNotNull("Org Data line 4 imported", org4);
				AssertEquals(2, org4.CustomsCodes.Count);
				AssertEquals("US", org4.CustomsCodes[0].OK_RN_NKCodeCountry);
				AssertEquals("Should be no OrgID type", "", org4.CustomsCodes[0].OK_CodeType);
			}
		}

		protected virtual void DoImportForTestUSIdentificationCodes(TempFile testFileName)
		{
			loader.ImportOrganisationData(testFileName.Filename, true);
		}
		#endregion

		#region TestImportWithoutLastColumn
		public void TestImportWithoutLastColumn()
		{
			Guid creditorGroupPK = CreditorGroup.PK.ToGuid();

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					OutputHeaderLine(sw, false);
					sw.WriteLine(String.Format("Z321Test3,Z321-TestOrg,Addr1,Addr2,City,,2020,AUSYD,AU,,98250011,,,,,,N,Y,N,N,N,N,N,N,N,N,N,N,N,John Smith,Manager,,98250011,01481123456,98250013,,,,AUD,1500,,Y,,0,,0,,,,,,,,,,,,,ANZ,,457983723,012-344,,,,,,,,,,,,,,"));
					sw.Flush();
				}

				DoImportForTestImportWithoutLastColumn(testFileName);

				OrgHeader[] organisation = FindOrgByFullNameStartsWith("Z321-Test");
				AssertEquals("1 org should be created", 1, organisation.Length);
			}
		}

		protected virtual void DoImportForTestImportWithoutLastColumn(TempFile testFileName)
		{
			loader.ImportOrganisationData(testFileName.Filename, true);
		}
		#endregion

		#region TestImportWithRelatedOrgs
		public void TestImportWithRelatedOrgs()
		{
			using (TempFile testFileName = TempFile.New(Env.TempPath, "csv"))
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Name,Address1,Address2,City,State,PostCode,UNLOCO,Country,PortCity,Phone,Fax,Email,Web,RegNo,CorpCode,Debtor,Creditor,Consignee,Consignor,Forwarder,Broker,Carrier,ShipLine,Airline,LocalTransport,SalesLead,Services,Competitor,Contact,Title,Email,Phone,Mobile,Fax,DebtorCode,DebtorGroup,DebtorSettleGroup,Currency,CreditLimit,CreditRating,GST,INV_TERMS_STANDARD,INV_DAYS_STANDARD,INV_TERMS_DISBURSEMENT,INV_DAYS_DISBURSEMENT,CreditorGroup,CustomsAgent,PostAddress1,PostAddress2,PostCity,PostState,PostPostCode,DeliverAddress1,DeliverAddress2,DeliverCity,DeliverState,DeliverPostCode,Bank,AccountName,AccountNo,BSB,CCD,CSC,SCC,CCP,CCC,CMP,WorkNotes,HandlingNotes,DeliveryNotes,ARNotes,ARCreditNotes,APNotes,ContactSourceType,CONTACTDATEDETAILSVERIFIED,CONTACTSALUTATION,LANGUAGE,MAINADDRESSLANGUAGE,POSTALADDRESSLANGUAGE,DELIVERYADDRESSLANGUAGE,REGDETAIL1,REGDETAIL2,REGDETAIL3,REGDETAIL4,REGDETAIL5,REGDETAIL6,REGDETAIL7,REGDETAIL8,REGDETAIL9,REGDETAIL10,REGDETAIL11,REGDETAIL12,REGDETAIL13,REGDETAIL14,REGDETAIL15,REGDETAIL16,REGDETAIL17,REGDETAIL18,REGDETAIL19,REGDETAIL20,REGDETAIL21,REGDETAIL22,REGDETAIL23,REGDETAIL24,REGDETAIL25,REGDETAIL26,REGDETAIL27,REGDETAIL28,REGDETAIL29,REGDETAIL30,REGDETAIL31,REGDETAIL32,REGDETAIL33,REGDETAIL34,REGDETAIL35,REGDETAIL36,REGDETAIL37,REGDETAIL38,REGDETAIL39,REGDETAIL40,REGDETAIL41,REGDETAIL42,REGDETAIL43,REGDETAIL44,REGDETAIL45,REGDETAIL46,REGDETAIL47,REGDETAIL48,REGDETAIL49,REGDETAIL50,REGDETAIL51,REGDETAIL52,REGDETAIL53,REGDETAIL54,REGDETAIL55,REGDETAIL56,REGDETAIL57,REGDETAIL58,REGDETAIL59,REGDETAIL60,REGDETAIL61,REGDETAIL62,REGDETAIL63,REGDETAIL64,REGDETAIL65,REGDETAIL66,REGDETAIL67,REGDETAIL68,REGDETAIL69,REGDETAIL70,REGDETAIL71,REGDETAIL72,REGDETAIL73,REGDETAIL74,REGDETAIL75,REGDETAIL76,REGDETAIL77,REGDETAIL78,REGDETAIL79,REGDETAIL80,REGDETAIL81,REGDETAIL82,REGDETAIL83,REGDETAIL84,REGDETAIL85,REGDETAIL86,REGDETAIL87,REGDETAIL88,REGDETAIL89,REGDETAIL90,REGDETAIL91,REGDETAIL92,REGDETAIL93,REGDETAIL94,REGDETAIL95,REGDETAIL96,REGDETAIL97,REGDETAIL98,REGDETAIL99");
					sw.WriteLine("Z321TEST3,Z321-Test Name,Address1,Address2,City,QLD,Post Code,AUBNE,Country,Port City,Phone,Fax,Email,Web,24-587345000,Government Corporation Code,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Contact Name,Contact Job Title,Contact Email,Contact Work Phone,Contact Mobile,Contact Fax,Z321Test2,Debtor Acc Group,Z321Test2,HKD,1500,CR3,Y,,0,,0,CG1,Z321Test2,Postal Address1,Postal Address2,Postal City,Postal State,Postal Post Code,Deliver Address1,Deliver Address2,Deliver City,Deliver State,Deliver Post Code,Bank Name,Account Name,Account Number,BSB Number,Customs Client Code,Customs Supplier Code,Customs Supplier Code (CMR),Customs Controlled Premises Code,Customs Carrier Code,Customs Manifest Provider Code,Work Notes,Handling Instructions,Delivery Instructions,A/R Account Management Notes,A/R Credit Management Notes,A/P Account Management Notes,,,,,,,,US~E1~NUMBERE1,US~E2~NUMBERE2,US~E3~NUMBERE3,US~E4~NUMBERE4,US~E5~NUMBERE5,US~E6~NUMBERE6,US~E7~NUMBERE7,US~E8~NUMBERE8,US~E9~NUMBERE9,US~E10~NUMBERE10,US~E11~NUMBERE11,US~E12~NUMBERE12,US~E13~NUMBERE13,US~E14~NUMBERE14,US~E15~NUMBERE15,US~E16~NUMBERE16,US~E17~NUMBERE17,US~E18~NUMBERE18,US~E19~NUMBERE19,US~E20~NUMBERE20,US~E21~NUMBERE21,US~E22~NUMBERE22,US~E23~NUMBERE23,US~E24~NUMBERE24,US~E25~NUMBERE25,US~E26~NUMBERE26,US~E27~NUMBERE27,US~E28~NUMBERE28,US~E29~NUMBERE29,US~E30~NUMBERE30,US~E31~NUMBERE31,US~E32~NUMBERE32,US~E33~NUMBERE33,US~E34~NUMBERE34,US~E35~NUMBERE35,US~E36~NUMBERE36,US~E37~NUMBERE37,US~E38~NUMBERE38,US~E39~NUMBERE39,US~E40~NUMBERE40,US~E41~NUMBERE41,US~E42~NUMBERE42,US~E43~NUMBERE43,US~E44~NUMBERE44,US~E45~NUMBERE45,US~E46~NUMBERE46,US~E47~NUMBERE47,US~E48~NUMBERE48,US~E49~NUMBERE49,US~E50~NUMBERE50,US~E51~NUMBERE51,US~E52~NUMBERE52,US~E53~NUMBERE53,US~E54~NUMBERE54,US~E55~NUMBERE55,US~E56~NUMBERE56,US~E57~NUMBERE57,US~E58~NUMBERE58,US~E59~NUMBERE59,US~E60~NUMBERE60,US~E61~NUMBERE61,US~E62~NUMBERE62,US~E63~NUMBERE63,US~E64~NUMBERE64,US~E65~NUMBERE65,US~E66~NUMBERE66,US~E67~NUMBERE67,US~E68~NUMBERE68,US~E69~NUMBERE69,US~E70~NUMBERE70,US~E71~NUMBERE71,US~E72~NUMBERE72,US~E73~NUMBERE73,US~E74~NUMBERE74,US~E75~NUMBERE75,US~E76~NUMBERE76,US~E77~NUMBERE77,US~E78~NUMBERE78,US~E79~NUMBERE79,US~E80~NUMBERE80,US~E81~NUMBERE81,US~E82~NUMBERE82,US~E83~NUMBERE83,US~E84~NUMBERE84,US~E85~NUMBERE85,US~E86~NUMBERE86,US~E87~NUMBERE87,US~E88~NUMBERE88,US~E89~NUMBERE89,US~E90~NUMBERE90,US~E91~NUMBERE91,US~E92~NUMBERE92,US~E93~NUMBERE93,US~E94~NUMBERE94,US~E95~NUMBERE95,US~E96~NUMBERE96,US~E97~NUMBERE97,US~E98~NUMBERE98,US~E99~NUMBERE99");
					sw.WriteLine("Z321TEST4,Z321 Name,Address1,Address2,City,QLD,Post Code,AUBNE,Country,Port City,Phone,Fax,Email,Web,24-587345000,Government Corporation Code,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Contact Name,Contact Job Title,Contact Email,Contact Work Phone,Contact Mobile,Contact Fax,Z321Test5,Debtor Acc Group,Z321Test5,HKD,1500,CR3,Y,,0,,0,CG1,Z321Test3,Postal Address1,Postal Address2,Postal City,Postal State,Postal Post Code,Deliver Address1,Deliver Address2,Deliver City,Deliver State,Deliver Post Code,Bank Name,Account Name,Account Number,BSB Number,Customs Client Code,Customs Supplier Code,Customs Supplier Code (CMR),Customs Controlled Premises Code,Customs Carrier Code,Customs Manifest Provider Code,Work Notes,Handling Instructions,Delivery Instructions,A/R Account Management Notes,A/R Credit Management Notes,A/P Account Management Notes,,,,,,,,US~EIN~24-587345000,US~MID~BOB1231,US~E50~NUMBERE50,US~E60~NUMBERE61,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,");
					sw.WriteLine("Z321TEST5,Z321-Test Co Name,Test Address1,Test Address2, ,NSW,2000,AUSYD,AU,,99994444,99995555,test@email.au,www.test.com.au,234322,,Y,N,Y,Y,N,N,N,N,N,N,N,N,N,,,,,,,,,,AUD,5000,CR2,Y,,0,,0,,,,,,,,,,,,,ANZ,,787458,012-045,,,,,,,Test notes,,,,,,,,,,,,,US~EIN~EINNUMBER23,US~MID~BOB8894,US~E50~NUMBERE365,US~E60~NUMBERE985,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,");
					sw.WriteLine("Z321TEST6,Z321-TestOrg,Addr1,Addr2,City,,2020,AUSYD,AU,,98250011,,,,,,N,Y,N,N,N,N,N,N,N,N,N,N,N,John Smith,Manager,,98250011,1481123456,98250013,,,,AUD,1500,,Y,,0,,0,,,,,,,,,,,,,ANZ,,457983723,012-344,,,,,,,,,,,,,,,,,,,,US~EIN~24-587345000,US~MID~BOB1231,US~E50~NUMBERE50,US~E60~NUMBERE61,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,");
				}

				DoImportForTestImportWithRelatedOrgs(testFileName);

				var org1 = FindOrgByFullName("Z321-Test Name");
				AssertNotNull("Z321Test3 organisation was imported", org1);
				OrgHeader relatedParty1 = org1.GetRelatedParty(RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, ZString.Empty);
				AssertNull("CustomsAgent", relatedParty1);

				var org2 = FindOrgByFullName("Z321 Name");
				AssertNotNull("Z321Test4 organisation was imported", org2);
				OrgHeader relatedParty2 = org2.GetRelatedParty(RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, ZString.Empty);
				AssertNotNull("CustomsAgent", relatedParty2);
				AssertEquals("The customs agent should be Z321TEST3", "Z321TEST3", relatedParty2.LegacyCode);

				OrgHeader relatedParty3 = org2.GetRelatedParty(RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, RelatedPartyDirectionList.Codes.Delivery);
				AssertNotNull("Debtor", relatedParty3);
				AssertEquals("The debtor should be Z321Test5", "Z321TEST5", relatedParty3.LegacyCode);
			}
		}

		protected virtual void DoImportForTestImportWithRelatedOrgs(TempFile testFileName)
		{
			loader.ImportOrganisationData(testFileName.Filename, false);
			AssertEquals(2, loader.RunCounters.RecordsToImportPlusHeader);
			AssertEquals(6, loader.RunCounters.RecsCreated);
			AssertEquals(0, loader.RunCounters.RecsUpdated);
			AssertEquals(0, loader.RunCounters.RecsExcluded);
			AssertEquals(5, loader.Log.Count);
		}
		#endregion

		public void TestCanImportCSVWithoutOptonalColumns()
		{
			AssertThatCSVHeadingLineIsValid(true, Array.Empty<string>());
		}

		public void TestCanImportCSVWithAllOptonalColumns()
		{
			var list = new List<string>();
			for (int i = 1; i < 100; i++)
			{
				list.Add("REGDETAIL" + i);
			}

			list.Add("BANKCURRENCY");
			list.Add("WAREHOUSE");

			AssertThatCSVHeadingLineIsValid(true, list);
		}

		public void TestCanImportCSVWithOptionalColumnsNotInASpecificOrder()
		{
			AssertThatCSVHeadingLineIsValid(true, new string[] { "REGDETAIL4", "BANKCURRENCY", "REGDETAIL3", "WAREHOUSE" });
		}

		public void TestCannotImportWhenDuplicatedColumns()
		{
			AssertThatCSVHeadingLineIsValid(false, new string[] { "REGDETAIL1", "REGDETAIL2", "REGDETAIL1" });
			AssertThatCSVHeadingLineIsValid(false, new string[] { loader.ValidMandatoryFileHeaderColumnsExposed[0] });
		}

		public void TestCannotImportREGDETAILOver99()
		{
			AssertThatCSVHeadingLineIsValid(false, new string[] { "REGDETAIL1", "REGDETAIL10", "REGDETAIL99", "REGDETAIL100" });
			AssertThatCSVHeadingLineIsValid(true, new string[] { "REGDETAIL1", "REGDETAIL10", "REGDETAIL99" });
		}

		[ExpectException(typeof(FileNotFoundException))]
		public void TestValidationOfFile()
		{
			DirectoryInfo directory = new DirectoryInfo(System.Environment.CurrentDirectory);
			string dummyFilePath = directory.FullName + "\\non-existant file";
			loader.ImportOrganisationData(dummyFilePath, true);
		}

		public void TestValidationOfContent_GarbageHeader()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("This is the header for the file.");
					sw.WriteLine("This is the first part - invalid text");
					sw.Flush();
				}
				loader.ImportOrganisationData(testFileName.Filename, true);
				AssertEquals(2, loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(0, loader.RunCounters.RecsCreated);
				AssertEquals(0, loader.RunCounters.RecsUpdated);
				AssertEquals(0, loader.RunCounters.RecsExcluded);
				AssertEquals(3, loader.Log.Count);
				AssertEquals("FileHeaderIsValid", false, loader.FileHeaderIsValid);
			}
		}

		public void TestValidationOfContent_HeaderColumnCountIncorrect()
		{
			AssertThatCSVHeadingLineIsValid(false, new string[] { "REGDETAIL1", "HELLOWORLD" });
		}

		public void TestImportPhone()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					OutputHeaderLine(sw);
					sw.WriteLine(String.Format("Z321Test1,Z321-Test Name,Address1,Address2,City,QLD,Post Code,AUBNE,Country,Port City,,Fax,Email,Web,Business Reg No,Government Corporation Code,Y,Y,Y,Y,Y,Y,Y,N,Y,Y,Y,Y,Y,Contact Name,Contact Job Title,Contact Email,Contact Work Phone,Contact Mobile,Contact Fax,Z321Test2,Debtor Acc Group,Z321Test2,HKD,1500,CR3,Y,,0,,0,CG1,Z321Test2,Postal Address1,Postal Address2,Postal City,Postal State,Postal Post Code,Deliver Address1,Deliver Address2,Deliver City,Deliver State,Deliver Post Code,Bank Name,Account Name,Account Number,BSB Number,Customs Client Code,Customs Supplier Code,Customs Supplier Code (CMR),Customs Controlled Premises Code,Customs Carrier Code,Customs Manifest Provider Code,Work Notes,Goods Handling Instructions,Delivery Instructions,A/R Account Management Notes,A/R Credit Management Notes,A/P Account Management Notes,050908_EXI_PRS,20051225,Senor,,,,,,Y"));
					sw.Flush();
				}
				loader.ImportOrganisationData(testFileName.Filename, true);
				OrgHeader org1 = LoadOrganisation("Z321-Test Name", "Address1");

				AssertEquals("Should stay blank, not n/a", "", org1.MainAddress.OA_Phone);
			}
		}

		public void TestOrgCode()
		{
			var algorithm = new OrgCodeAlgorithm();
			algorithm.AlgorithmType = OrgCodeAlgorithmType.Default;
			algorithm.RegenerateOrgCodeOnChanges = true;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Order = 0;
			algorithm.Elements[OrgCodeElementDescription.LastName].Order = 0;
			algorithm.Elements[OrgCodeElementDescription.UnlocoCode].Order = 0;
			algorithm.Elements[OrgCodeElementDescription.CodeSpecificUniqueNumber].Order = 1;
			algorithm.Elements[OrgCodeElementDescription.CodeSpecificUniqueNumber].Length = 7;
			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);

			using (var tempFile = TempFile.New())
			{
				using (var sw = new StreamWriter(tempFile.Filename))
				{
					OutputHeaderLine(sw);
					sw.WriteLine(",Super Duper,72 O'Riordan St,Alexandria,Sydney,NSW,2015,AUSYD,,,83036800,83036800,email,www.webaddress.au,00112345612,,,");
					sw.Flush();
				}

				loader.ImportOrganisationData(tempFile.Filename, true);
				AssertEquals(2, loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(1, loader.RunCounters.RecsCreated);
				AssertEquals(0, loader.RunCounters.RecsUpdated);
				AssertEquals(0, loader.RunCounters.RecsExcluded);
				AssertEquals(2, loader.Log.Count);

				var createdOrg = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, "Super Duper"));
				AssertNotNull("There should have been organisation created", createdOrg);

				AssertEquals("0000001", createdOrg.OH_Code);
			}
		}

		public void TestUniqueOrgCodeIsIdentifiedWhenRetainingExistingCodes()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					OutputHeaderLine(sw);
					sw.WriteLine(String.Format("Code001,Z123-testdata:Atari Australia pty ltd,32 Bowden St,Alexandria,Sydney,NSW,2015,AUSYD,,,83036800,83036800,email,www.webaddress.au,00112345612,,Y,Y,Y,Y,N,N,N,N,N,N,N,N,Leon Jenningswilliams,Director,leon.jennings@atari.com,,,,,,,,"));
					sw.WriteLine(String.Format("Code002,Z123-testdata:Atari Aust.,55 Bowden St,Alexandria,,NSW,2015,AUSYD,,,83036855,83036856,email,web,GSTNO,,Y,Y,Y,Y,N,N,N,N,N,N,N,N,John Gibonssmithjones,Director of Financial Accounting and Management,contact email,contact phone,contact mobile,Y,Y,Notes,,,"));
					sw.WriteLine(String.Format(",Z123-testdata:Atari Aus P/L,22 Bowden St,Alexandria,Sydney,,2015,,,,83036800,83036800,email,www.webaddress.au,00112345612,,Y,Y,Y,Y,N,N,N,N,N,N,N,N,Leon Jenningswilliams,Director,leon.jennings@atari.com,,,,,,,,"));
					sw.WriteLine(String.Format(",Z123-testdata:Atari Austria,17 Lonestrausser,St Veit an der Golsen,,,,,,17905,+ 22 49036855,83036856,email,web,GSTNO,,N,N,Y,Y,N,N,N,N,N,N,N,N,John Gibonssmithjones,Director of Financial Accounting and Management,contact email,contact phone,contact mobile,Y,Y,Notes,,,"));
					sw.WriteLine(String.Format("ABCSYD,ABC Company,Level 1,100 Main Road,Sydney,NSW,2340,AUSYD,,,02-23459000,02-23459001,email@test.com.au,www.web.com.au,41065894724,65894724,Y,Y,Y,Y,Y,,,N,N,N,N,Y,Y,John Smith,Director,john@test.com.au,0412 456789,0412 456790,faxnumber,HAMHAM,,,NZD,1000,,Y,,0,,0,,HAMHAM,PostAddress1,PostAddress2,PostCity,PostState,PostPostCode,DeliverAddress1,DeliverAddress2,DeliverCity,DeliverState,DeliverPostCode,Bank,AccountName,AccountNo,BSB,CCD,CSC,SCC,CCP,CCC,CMP,WorkNotes,HandlingNotes,DeliveryNotes,ARNotes,ARCreditNotes,AP Notes,,,"));
					sw.WriteLine(String.Format("HAMHAM,Hamburger,Dammer Deich,Hauptstr 100,Hamburg,,20000,,Germany,Hamburg,040 456000,041 456000,,,,,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,N,N,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,"));
					sw.WriteLine(String.Format("XXXSYD,XXX Company,Level 1,100 Main Road,Sydney,NSW,2340,AUSYD,,,02-23459000,02-23459001,email@test.com.au,www.web.com.au,41065894724,65894724,Y,Y,Y,Y,Y,N,N,N,N,N,N,Y,Y,John Smith,Director,,0412 456789,0412 456790,faxnumber,HAMHAM,,,HKD,1000,,Y,,0,,0,,HAMHAM,PostAddress1,PostAddress2,PostCity,PostState,PostPostCode,DeliverAddress1,DeliverAddress2,DeliverCity,DeliverState,DeliverPostCode,Bank,AccountName,AccountNo,BSB,CCD,CSC,SCC,CCP,CCC,CMP,WorkNotes,HandlingNotes,DeliveryNotes,ARNotes,ARCreditNotes,AP Notes,,,"));
					sw.Flush();
				}

				loader.ImportOrganisationData(testFileName.Filename, false);
				AssertEquals(4, loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(11, loader.RunCounters.RecsCreated);
				AssertEquals(0, loader.RunCounters.RecsUpdated);
				AssertEquals(0, loader.RunCounters.RecsExcluded);
				AssertEquals(6, loader.Log.Count);
				AssertEquals("OrgCode should have been retained", "XXXSYD", LoadOrganisation("XXX Company", "Level 1").OH_Code);
				AssertEquals("OrgCode should have been retained", "HAMHAM", LoadOrganisation("Hamburger", "Dammer Deich").OH_Code);
				AssertEquals("OrgCode should have been retained", "ABCSYD", LoadOrganisation("ABC Company", "Level 1").OH_Code);
				Assert("OrgCode should be Code001", LoadOrganisation("Z123-testdata:Atari Australia pty ltd", "32 Bowden St").OH_Code == "CODE001");
				Assert("Companies with like names need to create unique short codes", LoadOrganisation("Z123-testdata:Atari Aust.", "55 Bowden St").OH_Code != LoadOrganisation("Z123-testdata:Atari Australia pty ltd", "32 Bowden St").OH_Code);
				Assert("Companies with like names need to create unique short codes - manually generated", LoadOrganisation("Z123-testdata:Atari Aus P/L", "22 Bowden St").OH_Code != LoadOrganisation("Z123-testdata:Atari Austria", "17 Lonestrausser").OH_Code);
			}
		}

		public void TestFindOrgFromLegacyCode()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					OutputHeaderLine(sw);
					sw.WriteLine(String.Format("Z321Test1,Z321-Test Name,Address1,Address2,City,QLD,Post Code,AUBNE,Country,Port City,Phone,Fax,Email,Web,Business Reg No,Government Corporation Code,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Contact Name,Contact Job Title,Contact Email,Contact Work Phone,Contact Mobile,Contact Fax,Z321Test2,Debtor Acc Group,Z321Test2,HKD,1500,CR3,Y,,0,,0,,Z321Test2,Postal Address1,Postal Address2,Postal City,Postal State,Postal Post Code,Deliver Address1,Deliver Address2,Deliver City,Deliver State,Deliver Post Code,Bank Name,Account Name,Account Number,BSB Number,Customs Client Code,Customs Supplier Code,Customs Supplier Code (CMR),Customs Controlled Premises Code,Customs Carrier Code,Customs Manifest Provider Code,Work Notes,Goods Handling Instructions,Delivery Instructions,A/R Account Management Notes,A/R Credit Management Notes,A/P Account Management Notes,,,"));
					sw.Flush();
				}

				loader.ImportOrganisationData(testFileName.Filename, true);
				AssertEquals(1, loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(2, loader.RunCounters.RecsCreated);
				AssertEquals(0, loader.RunCounters.RecsUpdated);
				AssertEquals(0, loader.RunCounters.RecsExcluded);
				AssertEquals(5, loader.Log.Count);

				//Re-run the same file again - the organisation should be found from the legacy code previously created and excluded from update
				loader.ImportOrganisationData(testFileName.Filename, true);
				AssertEquals(2, loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(2, loader.RunCounters.RecsCreated);
				AssertEquals(0, loader.RunCounters.RecsUpdated);
				AssertEquals(1, loader.RunCounters.RecsExcluded);
				AssertEquals(11, loader.Log.Count);
			}
		}

		public void TestProcessDataForThisLineUserMessages()
		{
			var newLoader = new OrgDataLoadForTest();
			newLoader.RunCounters.CurrentRow = 5;
			newLoader.RunCounters.RecsExcluded = 0;
			newLoader.ProcessDataForThisLineExposed(new OCsvLine("Test Data"));
			AssertEquals(1, newLoader.Log.Count);
			AssertEquals(1, newLoader.RunCounters.RecsExcluded);
			AssertEquals("Row 5 excluded: data is inconsistent with required format ", newLoader.Log[0]);
		}

		public void TestProcessExtractedDataUserMessages()
		{
			var orgData = loader.ExtractOrganisationData(new OCsvLine("Z321Test1,Z321-Test Name,Address1,Address2,City,QLD,Post Code,AUBNE,Country,Port City,Phone,Fax,Email,Web,Business Reg No,Government Corporation Code,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Contact Name,Contact Job Title,Contact Email,Contact Work Phone,Contact Mobile,Contact Fax,Z321Test2,Debtor Acc Group,Z321Test2,HKD,1500,CR3,Y,,0,,0,,Z321Test2,Postal Address1,Postal Address2,Postal City,Postal State,Postal Post Code,Deliver Address1,Deliver Address2,Deliver City,Deliver State,Deliver Post Code,Bank Name,Account Name,Account Number,BSB Number,Customs Client Code,Customs Supplier Code,Customs Supplier Code (CMR),Customs Controlled Premises Code,Customs Carrier Code,Customs Manifest Provider Code,Work Notes,Goods Handling Instructions,Delivery Instructions,A/R Account Management Notes,A/R Credit Management Notes,A/P Account Management Notes"));
			loader.ProcessExtractedDataExposed(orgData);
			loader.RunCounters.RecsExcluded = 0;
			loader.ProcessExtractedDataExposed(orgData);
			AssertEquals(1, loader.Log.Count);
			AssertEquals(1, loader.RunCounters.RecsExcluded);
			AssertEquals((NoResString)$"Organization excluded: {orgData.OrgCode} / {orgData.OrgName} - already exists in {Core.Constants.ProductName} table", loader.Log[0]);
		}

		public void TestUpdateExcelOrgLinksUserMessages()
		{
			var newLoader = new OrgDataLoadForTest();
			newLoader.RunCounters.CurrentRow = 0;
			newLoader.OrganisationLinks.Add(new OrgLinks());
			newLoader.UpdateExcelOrgLinks();
			AssertEquals(4, newLoader.Log.Count);
			AssertEquals("Updating organization links:", newLoader.Log[0]);
			AssertEquals(string.Format("{0} / Organization not found on linking parse", ""), newLoader.Log[1]);
			AssertEquals("Updating organization links completed:", newLoader.Log[2]);
		}

		public void TestDisplayLinkedDataTotalsUserMessages()
		{
			var newLoader = new OrgDataLoadForTest();
			newLoader.RunCounters.CurrentRow = 5;
			newLoader.DisplayLinkedDataTotals();
			AssertEquals(1, newLoader.Log.Count);
			AssertEquals(System.Environment.NewLine + (NoResString)string.Format("T O T A L : Organizations updated with linked records = {0}", 0) + System.Environment.NewLine, newLoader.Log[0]);
		}

		public void TestFormattedDisplayMessage()
		{
			var newLoader = new OrgDataLoadForTest();
			newLoader.RunCounters.CurrentRow = 6;
			newLoader.DisplayFormattedLogMessage("TestORG", "There was a failure");
			AssertEquals(1, newLoader.Log.Count);
			AssertEquals((NoResString)string.Format("{0} Organization: {1}  {2}", "Line 6:", "TestORG", "There was a failure"), newLoader.Log[0]);
		}

		public void TestCorpCodeForGB()
		{
			var orgData = loader.ExtractOrganisationData(new OCsvLine(",000 V.i.p. Star,121433 Moscow  Russia,Minskayav ST  22/35 Tel.+7 095 9958062,RUSSIA,,,RU,RU,,7959958062,7,,,,1234567,N,N,Y,Y,N,N,N,N,N,N,N,N,N,,,,,,,,TPY,,GBP,,,,,,INV,0,TPY,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,"));
			loader.ProcessExtractedDataExposed(orgData);
			OrgHeader org = loader.FindOrganisationIfItExistsExposed(orgData);
			AssertNotNull(org);
			AssertEquals(1, org.CustomsCodes.Count);
			AssertEquals("AU", org.CustomsCodes[0].OK_RN_NKCodeCountry);
			AssertEquals("GCR", org.CustomsCodes[0].OK_CodeType);

			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.UnitedKingdom);
			loader = new OrgDataLoadForTest();

			loader.ProcessExtractedDataExposed(orgData);
			org = loader.FindOrganisationIfItExistsExposed(orgData);
			AssertNotNull(org);
			AssertEquals(0, org.CustomsCodes.Count);
		}

		public void TestOrgWithEmptyAddressExcludedWhenImportFromCSV()
		{
			var orgData = loader.ExtractOrganisationData(new OCsvLine("ababa,Blankaddresstest01,TestAddress1,,,,,,AU,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,"));
			AssertNotNull(orgData);
			AssertEquals("TestAddress1", orgData.MainAddress.Address1);
			AssertNull(loader.DataError.ErrorMsg);

			orgData = loader.ExtractOrganisationData(new OCsvLine("ababa,Blankaddresstest01,,,,,,,AU,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,"));
			AssertNull(orgData);
			AssertEquals("- cannot import an Organization with blank address data", loader.DataError.ErrorMsg);
		}

		public void TestAddressLengthAtLeast4OrReject()
		{
			var orgData = loader.ExtractOrganisationData(new OCsvLine("ababa,Blankaddresstest01,Address1MoreThan4Chars,,,,,,AU,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,"));
			AssertNotNull(orgData);
			AssertEquals("Address1MoreThan4Chars", orgData.MainAddress.Address1);
			AssertNull(loader.DataError.ErrorMsg);

			orgData = loader.ExtractOrganisationData(new OCsvLine("ababa,Blankaddresstest01,4-1,,,,,,AU,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,"));
			AssertNull(orgData);
			AssertEquals("- cannot import an Organization with address less than 4 characters", loader.DataError.ErrorMsg);
		}

		public void TestImportOrgNameInMixedCase()
		{
			bool value = Env.Registry.OrgAllowMixedCase;
			try
			{
				Env.Registry.SetOrgAllowMixedCase(false);
				CsvOrg orgData = loader.ExtractOrganisationData(new OCsvLine(",Test mixed Name,121433 Moscow  Russia,Minskayav ST  22/35 Tel.+7 095 9958062,RUSSIA,,,RU,RU,,7959958062,7,,,,,N,N,Y,Y,N,N,N,N,N,N,N,N,N,,,,,,,,TPY,,GBP,,,,,,INV,0,TPY,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,"));
				loader.ProcessExtractedDataExposed(orgData);
				OrgHeader org = loader.FindOrganisationIfItExistsExposed(orgData);
				AssertNotNull(org);
				AssertEquals("OrgAllowMixedCase is false, so org name should be upper case", "TEST MIXED NAME", org.OH_FullName);

				Env.Registry.SetOrgAllowMixedCase(true);
				loader = new OrgDataLoadForTest();
				loader.ProcessExtractedDataExposed(orgData);
				org = loader.FindOrganisationIfItExistsExposed(orgData);
				AssertNotNull(org);
				AssertEquals("OrgAllowMixedCase is true, so org name should stay mixed case", "Test mixed Name", org.OH_FullName);
			}
			finally
			{
				Env.Registry.SetOrgAllowMixedCase(value);
			}
		}

		public void TestRegistrationCodeTypeDuringExtractProcess()
		{
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.UnitedKingdom, "GB", "VAT");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.Sweden, "SE", "VAT");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.Australia, "AU", "ABN");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.Italy, "IT", "COD");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.SriLanka, "LK", "SVT");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.Ethiopia, "ET", "TIN");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.Uganda, "UG", "TIN");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.Zambia, "ZM", "TIN");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.CostaRica, "CR", "GBR");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.Guatemala, "GT", "NIT");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.Nigeria, "NG", "TIN");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.Venezuela, "VE", "RIF");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.Ecuador, "EC", "RUC");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.ElSalvador, "SV", "NRC");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.Paraguay, "PY", "RUC");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.Uruguay, "UY", "RUT");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.Latvia, "LV", "PVN");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.Lithuania, "LT", "PVM");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.Slovenia, "SI", "DDV");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.Azerbaijan, "AZ", "VAT");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.Kenya, "KE", "VAT");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.Mauritius, "MU", "VAT");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.Bolivia, "BO", "NIT");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.FrenchPolynesia, "PF", "TAH");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.Honduras, "HN", "RTN");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.Nicaragua, "NI", "RUC");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.Portugal, "PT", "IVA");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.Mongolia, "MN", "VAT");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.Slovakia, "SK", "DPH");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.Botswana, "BW", "VAT");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.Kazakhstan, "KZ", "VAT");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.Tanzania, "TZ", "VRN");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.Mali, "ML", "NIF");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.Zimbabwe, "ZW", "VAT");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.Lebanon, "LB", "VAT");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.Senegal, "SN", "NIN");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.CoteDivoire, "CI", "NCC");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.Cameroon, "CM", "NIU");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.Mozambique, "MZ", "NUI");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.EquatorialGuinea, "GQ", "NIF");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.DominicanRepublic, "DO", "RNC");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.Yemen, "YE", "GST");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.Somalia, "SO", "GCR");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.Panama, "PA", "RUC");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.Algeria, "DZ", "NIF");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.Malawi, "MW", "TIN");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.Niger, "NE", "NIF");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.PuertoRico, "PR", "NRC");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.Kiribati, "KI", "TIN");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.BurkinaFaso, "BF", "IFU");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.Nepal, "NP", "VAT");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.Iran, "IR", "VAT");
			AssertRegistrationCodeTypeDuringExtractProcess(Enterprise.Core.Constants.CountryCodes.NewCaledonia, "NC", "TGC");
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			Env.Registry.SetOrgAllowMixedCase(true);
			Env.Registry.SetOrgUsePhoneNumberFormatting(false);
			loader = new OrgDataLoadForTest();
		}

		protected override OrgDataLoad GetNewDataLoader() => new OrgDataLoad();

		OrgDataLoadForTest loader;

		void OutputHeaderLine(StreamWriter sw)
		{
			OutputHeaderLine(sw, true);
		}

		void OutputHeaderLine(StreamWriter sw, bool withOptionalCols)
		{
			string optionalCols = withOptionalCols ? ",BankCurrency,Warehouse,ControllingAgent,ControllingCustomer" : "";
			sw.WriteLine(String.Format("Code,Name,Address1,Address2,City,State,PostCode,UNLOCO,Country,PortCity,Phone,Fax,Email,Web,RegNo,CorpCode,Debtor,Creditor,Consignee,Consignor,Forwarder,Broker,Carrier,ShipLine,Airline,LocalTransport,SalesLead,Services,Competitor,Contact,Title,Email,Phone,Mobile,Fax,DebtorCode,DebtorGroup,DebtorSettleGroup,Currency,CreditLimit,CreditRating,GST,INV_TERMS_STANDARD,INV_DAYS_STANDARD,INV_TERMS_DISBURSEMENT,INV_DAYS_DISBURSEMENT,CreditorGroup,CustomsAgent,PostAddress1,PostAddress2,PostCity,PostState,PostPostCode,DeliverAddress1,DeliverAddress2,DeliverCity,DeliverState,DeliverPostCode,Bank,AccountName,AccountNo,BSB,CCD,CSC,SCC,CCP,CCC,CMP,WorkNotes,HandlingNotes,DeliveryNotes,ARNotes,ARCreditNotes,APNotes,ContactSourceType,ContactDateDetailsVerified,ContactSalutation,Language,Mainaddresslanguage,Postaladdresslanguage,Deliveryaddresslanguage" + optionalCols));
		}

		string GetNoteByDescription(string desc, BusinessObjectCollection notes)
		{
			string noteText = "";
			foreach (StmNote note in notes)
			{
				if (note.ST_Description == desc)
				{
					noteText = note.ST_NoteDataAsText;
					break;
				}
			}

			return noteText;
		}

		OrgDebtorGroup DebtorGroup
		{
			get
			{
				if (fDebtorGroup == null)
				{
					fDebtorGroup = Factory.New<OrgDebtorGroup>();
					fDebtorGroup.OJ_Code = "DG1";
					fDebtorGroup.OJ_Desc = "Test Debtor Group";
					Factory.Save();
				}

				return fDebtorGroup;
			}
		}
		OrgDebtorGroup fDebtorGroup;

		OrgCreditorGroup CreditorGroup
		{
			get
			{
				if (fCreditorGroup == null)
				{
					fCreditorGroup = Factory.LoadFromNaturalKey<OrgCreditorGroup>(OrgCreditorGroupSchema.OG_Code, "CG1");

					if (fCreditorGroup == null)
					{
						fCreditorGroup = Factory.New<OrgCreditorGroup>();
						fCreditorGroup.OG_Code = "CG1";
						fCreditorGroup.OG_Desc = "Test Creditor Group";
						Factory.Save();
					}
				}

				return fCreditorGroup;
			}
		}
		OrgCreditorGroup fCreditorGroup;

		OrgHeader LoadOrganisation(ZString lookupOrg, ZString address)
		{
			ZDBOnlyQuery dBOnlyQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			dBOnlyQuery.AddToFilter(OrgHeaderSchema.OH_FullName, lookupOrg);
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.OA_OH);
			subQuery.AddToFilter(OrgAddressSchema.OA_Address1, address);
			dBOnlyQuery.AddSubQuery(subQuery, JoinCondition.And);
			OrgHeader enterpriseOrganisation = Factory.LoadTop1<OrgHeader>(dBOnlyQuery);
			AssertNotNull(enterpriseOrganisation);

			return enterpriseOrganisation;
		}

		OrgHeader FindOrgByFullName(ZString fullName)
		{
			return Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, fullName));
		}

		OrgHeader[] FindOrgByFullNameStartsWith(ZString fullNameStartsWith)
		{
			return Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, SQLComparisonOperator.StartsWith, fullNameStartsWith) { OrderBy = OrgHeaderSchema.OH_FullName.Name });
		}

		void AssertThatCSVHeadingLineIsValid(bool isHeaderLineValid, IEnumerable<string> optionalValues)
		{
			var list = new List<string>(loader.ValidMandatoryFileHeaderColumnsExposed);
			list.AddRange(optionalValues);
			var builder = new ZStringBuilder(list);
			var query = new ZQuery(OrgHeaderSchema.OH_Code, "Z321Test4");
			AssertEquals("PreCondition: Organisation doesn't exist", 0, Factory.Load<OrgHeader>(query).Length);

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(builder.ToStringWithDelimiterBetweenAppends(","));
					sw.WriteLine("Z321Test4,Z321-Test Name,Address1,Address2,City,QLD,Post Code,AUBNE,Country/Region,Port City,Phone,Fax,Email,Web,24-587345000,Government Corporation Code,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Contact Name,Contact Job Title,Contact Email,Contact Work Phone,Contact Mobile,Contact Fax,Z321Test2,Debtor Acc Group,Z321Test2,HKD,1500,CR3,Y,,0,,0,CG1,Z321Test2,Postal Address1,Postal Address2,Postal City,Postal State,Postal Post Code,Deliver Address1,Deliver Address2,Deliver City,Deliver State,Deliver Post Code,Bank Name,Account Name,Account Number,BSB Number,Customs Client Code,Customs Supplier Code,Customs Supplier Code (CMR),Customs Controlled Premises Code,Customs Carrier Code,Customs Manifest Provider Code,Work Notes,Handling Instructions,Delivery Instructions,A/R Account Management Notes,A/R Credit Management Notes,A/P Account Management Notes");
				}

				OrgDataLoad orgDataLoad = new OrgDataLoad();
				loader.ImportOrganisationData(testFileName.Filename, false);
				var organisationCount = Factory.Load<OrgHeader>(query).Length;
				if (isHeaderLineValid)
				{
					AssertEquals("Post: Organisation was imported", 1, organisationCount);
				}
				else
				{
					AssertEquals("Post: Organisation wasn't imported", 0, organisationCount);
				}
			}

			AssertEquals("FileHeaderIsValid", isHeaderLineValid, loader.FileHeaderIsValid);
		}

		void AssertRegistrationCodeTypeDuringExtractProcess(string country, string expectedCountryCode, string expectedCodeType)
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Sweden);
			CsvOrg orgData = loader.ExtractOrganisationData(new OCsvLine(",Games Workshop,121433 London,King str,UNITED KINGDOM,,,GB,GB,,7959958062,7,,,321,,N,N,Y,Y,N,N,N,N,N,N,N,N,N,,,,,,,,TPY,,GBP,,,,,,INV,0,TPY,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,"));
			GlbCompany.CurrentCompany.SetCountry(country);
			loader = new OrgDataLoadForTest();
			loader.ProcessExtractedDataExposed(orgData);
			OrgHeader org = loader.FindOrganisationIfItExistsExposed(orgData);
			AssertNotNull(org);
			AssertEquals(1, org.CustomsCodes.Count);
			AssertEquals(expectedCountryCode, org.CustomsCodes[0].OK_RN_NKCodeCountry);
			AssertEquals(expectedCodeType, org.CustomsCodes[0].OK_CodeType);
		}

		sealed class OrgDataLoadForTest : OrgDataLoad
		{
			public OrgDataLoadForTest() : base()
			{
			}

			internal OrgHeader FindOrganisationIfItExistsExposed(CsvOrg extractedData) => FindOrganisationIfItExists(extractedData);

			internal string[] ValidMandatoryFileHeaderColumnsExposed => ValidMandatoryFileHeaderColumns;

			internal Guid ProcessExtractedDataExposed(CsvOrg orgData) => ProcessExtractedData(orgData);

			internal void ProcessDataForThisLineExposed(OCsvLine line) => ProcessDataForThisLine(line);
		}
	}
}
