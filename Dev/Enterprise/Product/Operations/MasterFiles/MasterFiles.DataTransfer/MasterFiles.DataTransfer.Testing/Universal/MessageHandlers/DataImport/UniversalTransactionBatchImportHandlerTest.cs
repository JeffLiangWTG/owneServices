using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	class UniversalTransactionBatchImportHandlerTest : TestCaseWithFactory
	{
		[TestDate(2021, 03, 15)]
		public void TestProcess_ProcessedOK()
		{
			TestProcess_Core(processedOKXML, (processingResult, message) =>
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
				AssertNotNull(processingResult);
				AssertEquals(HttpXmlResultStatusList.Codes.ProcessedOK, processingResult.Status);
			});
		}

		[TestDate(2021, 03, 15)]
		public void TestProcess_Discarded()
		{
			TestProcess_Core(discardedXML, (processingResult, message) =>
			{
				AssertEquals(EDIMessageStatusList.Codes.Discarded, message.EM_Status);
				AssertNotNull(processingResult);
				AssertEquals(HttpXmlResultStatusList.Codes.ProcessedOK, processingResult.Status);
			});
		}

		void TestProcess_Core(string xml, Action<IHttpXmlProcessingResult, EDIMessage> assert)
		{
			var handler = new UniversalTransactionBatchImportHandler(new XmlSessionTracker(new SimpleLogger()));
			var request = handler.CreateRequestMessage();
			AssertEquals("Precondition", EDIMessageStatusList.Codes.Recognised, ((EDIMessage)request).EM_Status);
			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				new StreamWriter(stream) { AutoFlush = true }.Write(xml);
				request.SetMessageTextSource(stream);
				request.Save();
			}

			using ((request as BusinessObject).Factory.AddDisposableService())
			using (var processingResult = handler.Process(request))
			{
				request.Save();
				var message = (EDIMessage)request;
				message.ReloadSafe();
				assert(processingResult, message);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			CreateAUDBankAccount();
			CreateTestPeriod();
			Factory.Save();
		}

		void CreateTestPeriod()
		{
			var period = Factory.New<AccPeriodManagement>();
			period.AM_GC_Company = GlbCompany.CurrentCompany.PK;
			period.AM_Year = 2021;
			period.AM_Period = 202103;
			period.AM_StartDate = new ZDateTime(2021, 03, 01);
			period.AM_EndDate = new ZDateTime(2021, 03, 31);
		}

		void CreateAUDBankAccount()
		{
			var fAUDBankAccount = Factory.LoadTop1<AccBankAccount>(new ZQuery(AccBankAccountSchema.AB_Code, "ZHSBCAUD"));
			if (fAUDBankAccount == null)
			{
				var header = Factory.NewWithValidTestData<AccGLHeader>();
				header.AG_AccountNum = "ZAUDAcc";
				fAUDBankAccount = Factory.New<AccBankAccount>();
				fAUDBankAccount.AB_Code = "ZHSBCAUD";
				fAUDBankAccount.AB_GB = GlbBranch.CurrentBranch.PK;
				fAUDBankAccount.AB_Desc = "HSBC AUD ACCT";
				fAUDBankAccount.AB_AG = header.PK;
				fAUDBankAccount.AB_BankName = "HSBC";
				fAUDBankAccount.AB_BankAbbreviation = "AUD";
				fAUDBankAccount.AB_BSB = "123456";
				fAUDBankAccount.AB_AccountNum = "12345678";
				fAUDBankAccount.AB_RX_NKAccountCurrency = "AUD";
			}
		}

		#region XML

		readonly string processedOKXML = @"<UniversalTransactionBatch xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<TransactionBatch>
		<DataContext>
			<EnterpriseID>EDI</EnterpriseID>
			<ServerID>DAT</ServerID>
			<Company>
				<Code>EDI</Code>
				<Name>Eagle Datamation International</Name>
			</Company>
		</DataContext>
		<TransactionCollection>
			<Transaction>
				<DataContext>
					<DataTargetCollection>
						<DataTarget>
							<Type>AccountingReceipt</Type>
						</DataTarget>
					</DataTargetCollection>
				</DataContext>
				<Ledger>AR</Ledger>
				<TransactionType>REC</TransactionType>
				<TransactionDate>2021-03-15T14:12:00</TransactionDate>
				<PostDate>2021-03-15T14:12:00</PostDate>
				<OrganizationAddress>
					<AddressType>ConsigneeDocumentaryAddress</AddressType>
					<AddressShortCode>Pick Up Address</AddressShortCode>
					<OrganizationCode>BAROPT</OrganizationCode>
					<Address1>12 COOLIBAH DRIVE</Address1>
					<Address2></Address2>
					<AddressOverride>false</AddressOverride>
					<City>PALM BEACH</City>
					<CompanyName>BARZ OPTICS</CompanyName>
					<Country>
						<Code>AU</Code>
						<Name>Australia</Name>
					</Country>
					<Port>
						<Code>AUBNE</Code>
						<Name>Brisbane</Name>
					</Port>
					<Postcode>4221</Postcode>
					<State>QLD</State>
				</OrganizationAddress>
				<Description>AR REC</Description>
				<PaymentOrReceiptType>CSH</PaymentOrReceiptType>
				<BankAccount>ZHSBCAUD</BankAccount>
				<CheckBookCode></CheckBookCode>
				<CheckNumberOrPaymentRef>CASH</CheckNumberOrPaymentRef>
				<OSCurrency>
					<Code>AUD</Code>
					<Description>Australian Dollar</Description>
				</OSCurrency>
				<OSExGSTVATAmount>100.0000</OSExGSTVATAmount>
				<OSTotal>100.0000</OSTotal>
				<LocalCurrency>
					<Code>AUD</Code>
					<Description>Australian Dollar</Description>
				</LocalCurrency>
				<LocalExVATAmount>100.0000</LocalExVATAmount>
				<LocalTotal>100.0000</LocalTotal>
				<Branch>
					<Code>BNE</Code>
					<Name>BN - AUBNE</Name>
				</Branch>
				<Department>
					<Code>BRN</Code>
					<Name>Branch</Name>
				</Department>
				<CheckDrawer></CheckDrawer>
				<DrawerBank></DrawerBank>
				<DrawerBranch></DrawerBranch>
				<PostingJournalCollection>
					<PostingJournal></PostingJournal>
				</PostingJournalCollection>
			</Transaction>
		</TransactionCollection>
	</TransactionBatch>
</UniversalTransactionBatch>";

		readonly string discardedXML = @"
<UniversalTransactionBatch xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<TransactionBatch>
		<TransactionCollection>
			<Transaction>
				<DataContext>
					<DataTargetCollection>
						<DataTarget>
							<Type>AccountingReceipt</Type>
						</DataTarget>
					</DataTargetCollection>
				</DataContext>
				<Ledger>AR</Ledger>
				<TransactionType>REC</TransactionType>
				<TransactionDate>2021-03-15T14:12:00</TransactionDate>
				<PostDate>2021-03-15T14:12:00</PostDate>
				<OrganizationAddress>
				</OrganizationAddress>
				<Description>AR REC</Description>
				<PaymentOrReceiptType>CSH</PaymentOrReceiptType>
				<BankAccount>ZHSBCAUD</BankAccount>
				<CheckBookCode></CheckBookCode>
				<CheckNumberOrPaymentRef>CASH</CheckNumberOrPaymentRef>
				<OSCurrency>
					<Code>AUD</Code>
					<Description>Australian Dollar</Description>
				</OSCurrency>
				<OSExGSTVATAmount>100.0000</OSExGSTVATAmount>
				<OSTotal>100.0000</OSTotal>
				<LocalCurrency>
					<Code>AUD</Code>
					<Description>Australian Dollar</Description>
				</LocalCurrency>
				<LocalExVATAmount>100.0000</LocalExVATAmount>
				<LocalTotal>100.0000</LocalTotal>
				<Branch>
					<Code>BNE</Code>
					<Name>BN - AUBNE</Name>
				</Branch>
				<Department>
					<Code>BRN</Code>
					<Name>Branch</Name>
				</Department>
				<CheckDrawer></CheckDrawer>
				<DrawerBank></DrawerBank>
				<DrawerBranch></DrawerBranch>
				<PostingJournalCollection>
					<PostingJournal></PostingJournal>
				</PostingJournalCollection>
			</Transaction>
			<Transaction>
				<DataContext>
					<DataTargetCollection>
						<DataTarget>
							<Type>AccountingReceipt</Type>
						</DataTarget>
					</DataTargetCollection>
				</DataContext>
				<Ledger>AR</Ledger>
				<TransactionType>REC</TransactionType>
				<TransactionDate>2021-03-15T14:12:00</TransactionDate>
				<PostDate>2021-03-15T14:12:00</PostDate>
				<OrganizationAddress>
					<AddressType>ConsigneeDocumentaryAddress</AddressType>
					<AddressShortCode>Pick Up Address</AddressShortCode>
					<OrganizationCode>BAROPT</OrganizationCode>
					<Address1>12 COOLIBAH DRIVE</Address1>
					<Address2></Address2>
					<AddressOverride>false</AddressOverride>
					<City>PALM BEACH</City>
					<CompanyName>BARZ OPTICS</CompanyName>
					<Country>
						<Code>AU</Code>
						<Name>Australia</Name>
					</Country>
					<Port>
						<Code>AUBNE</Code>
						<Name>Brisbane</Name>
					</Port>
					<Postcode>4221</Postcode>
					<State>QLD</State>
				</OrganizationAddress>
				<Description>AR REC</Description>
				<PaymentOrReceiptType>CSH</PaymentOrReceiptType>
				<BankAccount>ZHSBCAUD</BankAccount>
				<CheckBookCode></CheckBookCode>
				<CheckNumberOrPaymentRef>CASH</CheckNumberOrPaymentRef>
				<OSCurrency>
					<Code>AUD</Code>
					<Description>Australian Dollar</Description>
				</OSCurrency>
				<OSExGSTVATAmount>100.0000</OSExGSTVATAmount>
				<OSTotal>100.0000</OSTotal>
				<LocalCurrency>
					<Code>AUD</Code>
					<Description>Australian Dollar</Description>
				</LocalCurrency>
				<LocalExVATAmount>100.0000</LocalExVATAmount>
				<LocalTotal>100.0000</LocalTotal>
				<Branch>
					<Code>BNE</Code>
					<Name>BN - AUBNE</Name>
				</Branch>
				<Department>
					<Code>BRN</Code>
					<Name>Branch</Name>
				</Department>
				<CheckDrawer></CheckDrawer>
				<DrawerBank></DrawerBank>
				<DrawerBranch></DrawerBranch>
				<PostingJournalCollection>
					<PostingJournal></PostingJournal>
				</PostingJournalCollection>
			</Transaction>
		</TransactionCollection>
	</TransactionBatch>
</UniversalTransactionBatch>";

		#endregion
	}
}
