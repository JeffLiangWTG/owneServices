using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Services.ServiceHost.Tests
{
	public class DocManagerDataContextTest : eAdaptorEndToEndTest, IDocManagerDataContextTester
	{
		#region Helper Methods

		void AddEdoc(IDocManagerSupport docManager, ZString eDocName)
		{
			var docManagerInfo = docManager.DocManagerInfo;
			docManagerInfo.AddFileOrDocument(new ZBlob(new byte[] { 1, 2, 3 }), eDocName, "INV");
			docManagerInfo.MasterFactory.Save();
		}

		string PostUniversalDocumentRequest(ZString docManagerCode, ZString jobCode)
		{
			var dataContext = DataContextCreator.Create(DataContextType.DocManager, GetContextKey(docManagerCode, jobCode));
			var documentRequest = UniversalDocumentRequestCreator.Create(dataContext);
			return PostRequest(documentRequest);
		}

		string GetContextKey(string docManagerCode, string jobCode) => $"{docManagerCode} {jobCode}";

		#endregion

		#region Success cases

		public void TestDocManagerExport_Shipment()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			var shipmentNumber = shipment.JS_UniqueConsignRef = "S00009001";
			Factory.Save();

			AssertEDocExported(shipment as IDocManagerSupport, shipmentNumber);
		}

		public void TestDocManagerExport_ProcessTask()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00009001";
			var processTask = ((IWorkflowProvider)shipment).WorkflowItems.Tasks.AddNew();
			Factory.Save();

			AssertEDocExported(processTask, processTask.P9_TaskID);
		}

		public void TestDocManagerExport_Declaration()
		{
			var declaration = Factory.New<Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_DeclarationReference = "B0001";
			Factory.Save();

			AssertEDocExported(declaration as IDocManagerSupport, declaration.JE_DeclarationReference);
		}

		public void TestDocManagerExport_JobWithSpaceInKey()
		{
			var job = Factory.NewWithValidTestData<GlbPerson>();
			job.PER_FullName = "Conor McGregor";
			Factory.Save();

			AssertEDocExported(job, job.PER_FullName);
		}

		public void TestDocManagerExport_JobWithIEDocsViaUniversalXmlSupport()
		{
			var job = Factory.New<IRefLocalLanguage>();
			job.RA_Code = "AA";
			job.RA_RN_NKCountryCode = "CN";
			job.RA_Description = "Test Language1";
			Factory.Save();

			AssertEDocExported(job as IDocManagerSupport, "AA-CN");
		}

		public void TestDocManagerExport_JobThatDoesNotHaveADataContextManager()
		{
			var bizo = Factory.New<AccBankAccount>();
			bizo.FillWithValidTestData();
			Factory.Save();

			AssertNull("Pre condition: Bizo must not have a UniversalDataContextManager", bizo.GetUniversalDataContextManager());

			var response = AssertEDocExported(bizo, bizo.AB_Code);
			AssertContains($"<Type>{DataContextType.DocManager}</Type>", response);
			AssertContains($"<Key>{GetContextKey(DocManagerCodes.BankAccount, bizo.AB_Code)}</Key>", response);
		}

		public void ExportEdocsViaUniversalXml(BusinessObject bizo)
		{
			if (bizo is IDocManagerSupport docManager)
			{
				string bizoCode;

				try
				{
					bizo.FillWithValidTestData();
					bizo.SuspendValidation();
					bizo.Factory.Save();
					bizoCode = CodePropertyAttribute.CodeFromBusinessObject(bizo);
				}
				catch (Exception)
				{
					ErrorReporter.Clear();
					return;
				}

				var docManagerCode = docManager.DocManagerInfo.DocManagerCode;
				var assemblyData = AssemblyDataLookup.GetAssemblyDataFromDocManagerCode(docManagerCode);
				if (assemblyData != null && assemblyData.GetEDocsViaUniversalXmlSupport() == null)
				{
					var fileName = "Stellarsphere.txt";
					var canImport = ImportEDocMessage($"[{docManagerCode} {RefDocTypes.MiscellaneousDocument} {bizoCode}] {fileName}", "SGVsbG8sIFdvcmxkIQ==");
					ErrorReporter.Clear();

					if (canImport && !string.IsNullOrEmpty(bizoCode))
					{
						var response = PostUniversalDocumentRequest(docManagerCode, bizoCode);
						AssertContains($"<FileName>{fileName}</FileName>", response);
					}
				}
			}
		}

		string AssertEDocExported(IDocManagerSupport parent, ZString jobNumber)
		{
			var docManagerInfo = parent.DocManagerInfo;
			var eDocName = "File1";
			AddEdoc(parent, eDocName);

			var response = PostUniversalDocumentRequest(docManagerInfo.DocManagerCode, jobNumber);
			AssertContains($"<FileName>{eDocName}</FileName>", response);
			return response;
		}

		bool ImportEDocMessage(string fileName, string imageData)
		{
			var dataContext = DataContextCreator.Create(DataContextType.DocManager, string.Empty, companyCode: GlbBranch.CurrentBranch.GB_Code);
			var attachedDocument = AttachedDocumentCreator.Create(
				fileName: fileName,
				base64ImageData: imageData,
				documentType: string.Empty,
				isPublished: true
			);

			var universalEvent = UniversalEventCreator.Create(dataContext, Events.DocumentImportedCode);
			UniversalEventCreator.SetAttachedDocumentCollection(universalEvent, attachedDocument);

			var responseBody = PostRequest(universalEvent);
			return responseBody.Contains("Successfully Added eDoc");
		}

		#endregion

		#region Failure Cases

		public void TestNoJobMatchingKey()
		{
			var dataContext = DataContextCreator.Create(DataContextType.DocManager, GetContextKey(DocManagerCodes.Shipment, "S0001"));
			var documentRequest = UniversalDocumentRequestCreator.Create(dataContext);

			var response = PostRequest(documentRequest);
			AssertContains($"There is no business object", response);
		}

		public void TestNoJobMatchingKey_ForIEDocsViaUniversalXmlSupport()
		{
			var dataContext = DataContextCreator.Create(DataContextType.DocManager, GetContextKey(DocManagerCodes.LocalLanguage, "S0001"));
			var documentRequest = UniversalDocumentRequestCreator.Create(dataContext);

			var response = PostRequest(documentRequest);
			AssertContains("Expecting a hint explaining the expected key format", $"Expecting a key in the format", response);
		}

		public void TestMatchingJobWithNoDocuments()
		{
			var job = Factory.New<Forwarding.IForwardingShipment>();
			var jobNumber = job.JS_UniqueConsignRef = "S00009001";
			Factory.Save();

			var response = PostUniversalDocumentRequest(DocManagerCodes.Shipment, jobNumber);
			AssertContains($"Data source was found, but no eDocs", response);
		}

		public void TestNonUniqueJobCode()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00009001";
			var processTask1 = ((IWorkflowProvider)shipment).WorkflowItems.Tasks.AddNew();
			var processTask2 = ((IWorkflowProvider)shipment).WorkflowItems.Tasks.AddNew();
			processTask1.P9_TaskID = processTask2.P9_TaskID = "T0001";
			Factory.Save();

			var response = PostUniversalDocumentRequest(DocManagerCodes.ProcessTask, processTask1.P9_TaskID);
			AssertContains($"Type {DocManagerCodes.ProcessTask} exists in multiple records.", response);
		}

		public void TestInvalidDocManagerKey()
		{
			AssertInvalidDocManagerKey("___");
			AssertInvalidDocManagerKey("___ ABC");
			AssertInvalidDocManagerKey("SH ABC");
		}

		void AssertInvalidDocManagerKey(ZString key)
		{
			var dataContext = DataContextCreator.Create(DataContextType.DocManager, key);
			var documentRequest = UniversalDocumentRequestCreator.Create(dataContext);

			var response = PostRequest(documentRequest);
			AssertContains($"Invalid Context Key", response);
		}

		#endregion
	}
}
