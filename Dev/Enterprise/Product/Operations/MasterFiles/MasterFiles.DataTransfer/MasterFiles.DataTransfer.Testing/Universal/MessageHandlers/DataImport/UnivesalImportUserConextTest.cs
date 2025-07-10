using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.DataTransfer.Testing.Universal.MessageHandlers.DataImport
{
	class UnivesalImportUserConextTest : TestCaseWithFactory
	{
		public void TestXmlUniversalImport_SavesWithCorrectContext()
		{
			var context = CreateNewUserContext();

			var dataContext = FormattableString.Invariant($@"<DataContext>
				      <DataTargetCollection>
				        <DataTarget>
				          <Type>ForwardingShipment</Type>
				          <Key>S00009001</Key>
				        </DataTarget>
				      </DataTargetCollection>
				      <EnterpriseID>{GlbCompany.CurrentCompany.LicenceEnterpriseCode}</EnterpriseID>
				      <ServerID>{GlbCompany.CurrentCompany.LicenceServerID}</ServerID>
				      <Company>
				        <Code>{context.Company.Code}</Code>
				      </Company>
				    </DataContext>");

			var result = ImportXmlAndAssertContextOnSave(dataContext, context.Company.Code, context.Branch.Code, User.InterchangeUserCode);

			AssertResponseContains(result, "ProcessingLog", "Linked Event to Shipment S00009001.");
			AssertResponseContains(result, "ProcessingStatusCode", "PRS");
		}

		public void TestXmlUniversalImport_CompanyWithNoActiveBranches()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var dataContext = FormattableString.Invariant($@"<DataContext>
				      <DataTargetCollection>
				        <DataTarget>
				          <Type>ForwardingShipment</Type>
				          <Key>S00009001</Key>
				        </DataTarget>
				      </DataTargetCollection>
				      <EnterpriseID>{GlbCompany.CurrentCompany.LicenceEnterpriseCode}</EnterpriseID>
				      <ServerID>{GlbCompany.CurrentCompany.LicenceServerID}</ServerID>
				      <Company>
				        <Code>{company.GC_Code}</Code>
				      </Company>
				    </DataContext>");

			var result = ImportXmlAndAssertContextOnSave(dataContext, GlbCompany.CurrentCompany.GC_Code, GlbBranch.CurrentBranch.GB_Code, GlbStaff.CurrentUser.GS_Code, false);

			AssertContains(result, $"Error - Message Rejected as Company '{company.GC_Code}' has no active branches.\r\nMessage Rejected.\r\nError - Delivery failed due to validation exception.");
		}

		public void TestXmlUniversalImport_InvalidCompany()
		{
			var context = CreateNewUserContext();

			var dataContext = FormattableString.Invariant($@"<DataContext>
				      <DataTargetCollection>
				        <DataTarget>
				          <Type>ForwardingShipment</Type>
				          <Key>S00009001</Key>
				        </DataTarget>
				      </DataTargetCollection>
				      <EnterpriseID>{GlbCompany.CurrentCompany.LicenceEnterpriseCode}</EnterpriseID>
				      <ServerID>{GlbCompany.CurrentCompany.LicenceServerID}</ServerID>
				      <Company>
				        <Code>BAD</Code>
				      </Company>
				    </DataContext>");

			//As the specified company does not exist the company is taken from the branch that created the message (EDIMessage.EM_GB)
			var result = ImportXmlAndAssertContextOnSave(dataContext, context.Company.Code, context.Branch.Code, User.InterchangeUserCode, contextForCreatingMessage: context);

			AssertResponseContains(result, "ProcessingLog", "Linked Event to Shipment S00009001.");
			AssertResponseContains(result, "ProcessingStatusCode", "PRS");
		}

		public void TestXmlUniversalImport_InvalidEnterpriseID()
		{
			var dataContext = FormattableString.Invariant($@"<DataContext>
				      <DataTargetCollection>
				        <DataTarget>
				          <Type>ForwardingShipment</Type>
				          <Key>S00009001</Key>
				        </DataTarget>
				      </DataTargetCollection>
				      <EnterpriseID>BAD</EnterpriseID>
				      <ServerID>{GlbCompany.CurrentCompany.LicenceServerID}</ServerID>
				      <Company>
				        <Code>{GlbCompany.CurrentCompany.GC_Code}</Code>
				      </Company>
				    </DataContext>");

			var result = ImportXmlAndAssertContextOnSave(dataContext, GlbCompany.CurrentCompany.GC_Code, GlbBranch.CurrentBranch.GB_Code, GlbStaff.CurrentUser.GS_Code, false);

			AssertContains("Error - DataContext EnterpriseId was specified, however it does not match the system EnterpriseId.", result);
		}

		public void TestXmlUniversalImport_InvalidServerID()
		{
			var dataContext = FormattableString.Invariant($@"<DataContext>
				      <DataTargetCollection>
				        <DataTarget>
				          <Type>ForwardingShipment</Type>
				          <Key>S00009001</Key>
				        </DataTarget>
				      </DataTargetCollection>
				      <EnterpriseID>{GlbCompany.CurrentCompany.LicenceEnterpriseCode}</EnterpriseID>
				      <ServerID>BAD</ServerID>
				      <Company>
				        <Code>{GlbCompany.CurrentCompany.GC_Code}</Code>
				      </Company>
				    </DataContext>");

			var result = ImportXmlAndAssertContextOnSave(dataContext, GlbCompany.CurrentCompany.GC_Code, GlbBranch.CurrentBranch.GB_Code, GlbStaff.CurrentUser.GS_Code, false);

			AssertContains("Error - DataContext ServerId was specified, however it does not match the system ServerId.", result);
		}

		public void TestXmlUniversalImport_NoTargetSpecified()
		{
			var context = CreateNewUserContext();

			var dataContext = FormattableString.Invariant($@"<DataContext>
				      <DataTargetCollection>
				        <DataTarget>
				          <Type>ForwardingShipment</Type>
				          <Key>S00009001</Key>
				        </DataTarget>
				      </DataTargetCollection>
				    </DataContext>");

			//the company is taken from the branch that created the message (EDIMessage.EM_GB)
			var result = ImportXmlAndAssertContextOnSave(dataContext, context.Company.Code, context.Branch.Code, User.InterchangeUserCode, contextForCreatingMessage: context);

			AssertResponseContains(result, "ProcessingLog", "Linked Event to Shipment S00009001.");
			AssertResponseContains(result, "ProcessingStatusCode", "PRS");
		}

		string ImportXmlAndAssertContextOnSave(string dataContext, string company, string branch, string user, bool shouldSaveFactory = true, IUserContext contextForCreatingMessage = null)
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00009001";
			Factory.Save();

			var requestXml = FormattableString.Invariant($@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
				  <Event>
				    DATACONTEXT
				    <EventTime>2016-06-09T12:32:12.42</EventTime>
				    <EventType>DDI</EventType>
				    <EventReference>PAL</EventReference>
				  </Event>
				</UniversalEvent>").Replace("DATACONTEXT", dataContext);

			IHttpXmlRequestResponse request = null;
			var sessionTracker = new XmlSessionTracker(new SimpleLogger());
			var handler = new UniversalEventImportHandler(sessionTracker);

			using (contextForCreatingMessage == null ? null : Env.Instance.SetTemporaryUserContext(contextForCreatingMessage))
			{
				request = handler.CreateRequestMessage();
				using (var stream = (SubStreamableStream)new MemoryStream())
				{
					new StreamWriter(stream) { AutoFlush = true }.Write(requestXml);
					request.SetMessageTextSource(stream);
					request.Save();
				}
			}

			bool wasSaved = false;
			BusinessObjectFactory.SetOnFactorySaveHookForTest(factory =>
			{
				if (factory.NameForDebugging.Equals("Publish Universal Xml Internally"))
				{
					CombineAssertions("Save must happen in the specied context", () =>
					{
						AssertEquals(company, GlbCompany.CurrentCompany.GC_Code);
						AssertEquals(branch, GlbBranch.CurrentBranch.GB_Code);
						AssertEquals(user, GlbStaff.CurrentUser.GS_Code);
					});
					wasSaved = true;
				}
			});

			using ((request as BusinessObject).Factory.AddDisposableService())
			using (var processingResult = handler.Process(request))
			{
				AssertEquals("The factory should/shouldn't be saved in this case", shouldSaveFactory, wasSaved);
				AssertNotNull(processingResult);

				if (processingResult.Status != "ERR")
				{
					processingResult.ResponseMessageText.Position = 0;
					return new StreamReader(processingResult.ResponseMessageText).ReadToEnd();
				}
				else
				{
					return sessionTracker.ToString();
				}
			}
		}

		void AssertResponseContains(string result, string type, string value)
		{
			AssertContains($@"<Type>{type}</Type>
        <Value>{value}</Value>", result);
		}

		IUserContext CreateNewUserContext()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			var user = Factory.NewWithValidTestData<GlbStaff>();
			branch.GB_GC = company.PK;
			Factory.Save();

			return new UserContext(user.GS_LoginName, branch.PK.ToGuid(), department.PK.ToGuid());
		}
	}
}
