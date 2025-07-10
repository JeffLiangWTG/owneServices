using System;
using System.IO;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(ImportOrganisationsFromCSVForm))]
	sealed class ImportOrganisationsFromCSVFormTest : DataLoaderFormTestCase
	{
		protected override ImportFromCSVForm GetNewImportFromCSVFormCore()
		{
			return new ImportOrganisationsFromCSVForm();
		}

		protected override string CountryCode
		{
			get { return "ER"; }
		}

		[RequiresSTA]
		public void TestFormHeading()
		{
			using (ImportOrganisationsFromCSVForm testForm = new ImportOrganisationsFromCSVForm())
			{
				testForm.Show();
				AssertEquals("Form text", "Import Organization Data", testForm.Text);
			}
		}

		public void TestConfirmOKLoadData()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);

			using (TempFile tempFile = TempFile.NewWithExtension("csv"))
			{
				PopulateTestFile(tempFile);

				using (ImportOrganisationsFromCSVForm testForm = new ImportOrganisationsFromCSVForm())
				{
					testForm.Show();
					testForm.FileNameTextBox.Text = tempFile.Filename;
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					testForm.StartButton.PerformClick();

					AssertNotNull("PreCondition: Message Shown", UnitTestUserNotification.Instance.LastMessage);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasQuestion);
					AssertEquals($"Please Note: Only Organizations not already found in {Core.Constants.ProductName} will be loaded.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Import Complete.", testForm.MainStatusBar.Text);
					Assert(testForm.CopyLogToClipboardButton.Enabled);
					Assert(testForm.CopyLogToClipboardButton.Visible);
					Assert(testForm.CloseButton.Enabled);
					Assert(testForm.CloseButton.Visible);
					Assert(testForm.OutputListBox.Items.Count > 0);

					string logData = testForm.GetLog();
					Assert("Log Data not as expected", logData.StartsWith("Organisations to Import = 3"));

					string logDataOutputFileName = null;
					try
					{
						logDataOutputFileName = testForm.CreateLogInDataDirectory(logData);
						Assert(logDataOutputFileName.Length > 0);
						Assert(logDataOutputFileName != "Not Created");
					}
					finally
					{
						File.Delete(logDataOutputFileName);
					}
				}
			}
		}

		void PopulateTestFile(TempFile tempFile)
		{
			OrgHeader testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			OrgHeader testSupplier1 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));

			using (StreamWriter sw = new StreamWriter(tempFile.Filename))
			{
				sw.WriteLine("Code,Name,Address1,Address2,City,State,PostCode,UNLOCO,Country,PortCity,Phone,Fax,Email,Web,RegNo,CorpCode,Debtor,Creditor,Consignee,Consignor,Forwarder,Broker,Carrier,ShipLine,Airline,LocalTransport,SalesLead,Services,Competitor,Contact,Title,Email,Phone,Mobile,Fax,DebtorCode,DebtorGroup,DebtorSettleGroup,Currency,CreditLimit,CreditRating,GST,INV_TERMS_STANDARD,INV_DAYS_STANDARD,INV_TERMS_DISBURSEMENT,INV_DAYS_DISBURSEMENT,CreditorGroup,CustomsAgent,PostAddress1,PostAddress2,PostCity,PostState,PostPostCode,DeliverAddress1,DeliverAddress2,DeliverCity,DeliverState,DeliverPostCode,Bank,AccountName,AccountNo,BSB,CCD,CSC,SCC,CCP,CCC,CMP,WorkNotes,HandlingNotes,DeliveryNotes,ARNotes,ARCreditNotes,APNotes,ContactSourceType,CONTACTDATEDETAILSVERIFIED,CONTACTSALUTATION,LANGUAGE,MAINADDRESSLANGUAGE,POSTALADDRESSLANGUAGE,DELIVERYADDRESSLANGUAGE");
				sw.WriteLine(String.Format("Z321Test1,Z321-Test Name,Address1,Address2,City,QLD,Post Code,AUBNE,Country,Port City,Phone,Fax,Email,Web,Business Reg No,Government Corporation Code,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Y,Contact Name,Contact Job Title,Contact Email,Contact Work Phone,Contact Mobile,Contact Fax,Z321Test2,Debtor Acc Group,Z321Test2,HKD,1500,CR3,Y,,0,,0,CG1,Z321Test2,Postal Address1,Postal Address2,Postal City,Postal State,Postal Post Code,Deliver Address1,Deliver Address2,Deliver City,Deliver State,Deliver Post Code,Bank Name,Account Name,Account Number,BSB Number,Customs Client Code,Customs Supplier Code,Customs Supplier Code (CMR),Customs Controlled Premises Code,Customs Carrier Code,Customs Manifest Provider Code,Work Notes,Handling Instructions,Delivery Instructions,A/R Account Management Notes,A/R Credit Management Notes,A/P Account Management Notes,,,,"));
				sw.WriteLine(String.Format("Z321Test2,Z321-Test Co Name,Test Address1,Test Address2, ,NSW,2000,AUSYD,,,99994444,99995555,test@email.au,www.test.com.au,999988885599,,Y,N,Y,Y,N,N,N,N,N,N,N,N,N,,,,,,,,,,AUD,5000,CR2,Y,,0,,0,,,,,,,,,,,,,ANZ,,787458,012-045,,,,,,,Test notes,,,,,,,,,"));
				sw.WriteLine(String.Format("Z321Test3,Z321-TestOrg,Addr1,Addr2,City,,2020,AUSYD,,,98250011,,,,,,N,Y,N,N,N,N,N,N,N,N,N,N,N,John Smith,Manager,,98250011,01481123456,98250013,,,,AUD,1500,,Y,,0,,0,,,,,,,,,,,,,ANZ,,457983723,012-344,,,,,,,,,,,,,,,,"));
				sw.Flush();
			}
		}
	}
}
