using System;
using System.IO;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.WorkflowManager;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Workflow.Testing
{
	public class UniversalXmlTestFileWriterTest : TestCaseWithFactory
	{
		public void TestEventExportWritesAnXmlFile()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());

			var trigger = ((IWorkflowProvider)shipment).WorkflowItems.AddNew();
			trigger.P9_Description = "Somehing";
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.AvailableToCode;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;
			action.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.AsPerPayload;

			Factory.Save();

			var dataWriter = trigger.WorkflowDescriptor.GetTestFileWriter(action, shipment);

			using (var tempDirectory = new TempDirectory())
			{
				var shipmentReference = shipment[JobShipmentSchema.JS_UniqueConsignRef].ToString();
				var exportResult = dataWriter.Export(tempDirectory.DirectoryName);

				AssertNotNull("exportResult", exportResult);
				CombineAssertions("exportResult", delegate
				{
					AssertContains("Text", string.Format("UniversalEvent from [{0}] saved", shipmentReference), exportResult.Message);
					var messageLines = exportResult.Message.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
					AssertEquals("messageLines.Length", 2, messageLines.Length);
					var fileName = messageLines[1];
					AssertEquals(string.Format("File.Exists(\"{0}\")", fileName), true, File.Exists(fileName));
					AssertContains("fileName", tempDirectory.DirectoryName, fileName);
					AssertEquals("fileName.EndsWith(\".xml\") failed on: " + fileName, true, fileName.EndsWith(".xml"));
					var fileText = File.ReadAllText(fileName);
					AssertContains("Trigger Count is never zero even for samples.", "<TriggerCount>1</TriggerCount>", fileText);
				});
			}
		}

		public void TestUniversalXmlTestFileWriter_dataWriterGetter_GetDataObject_WhenNull()
		{
			var topLevelDataObjectWriterMock = new Mock<ITopLevelDataObjectWriter>(MockBehavior.Strict);
			var dummyBO = (BusinessObject)Factory.New<IDummyWithWorkflow>();
			var writer = new UniversalXmlTestFileWriter(new ActionInfo(RecipientRoleType.AAD, dummyBO), _ => topLevelDataObjectWriterMock.Object, dummyBO);

			topLevelDataObjectWriterMock.Setup(x => x.RootElementName).Returns(string.Empty);
			topLevelDataObjectWriterMock.Setup(x => x.GetDataObject(It.IsAny<BusinessObject>())).Returns((ITopLevelDataObject)null);

			using (var tempfile = TempFile.New())
			{
				AssertExceptionThrown(
						typeof(InvalidOperationException),
						"Data Context for the DummyBusinessObject data object was empty. The Data Context should always be created by the top level data object.",
						() => writer.Export(tempfile.Filename));
			}
		}

		public void TestExportXMLFileNameIsValid()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			shipment[JobShipmentSchema.JS_UniqueConsignRef] = @"S\h/i:p*m?e|n<t";

			var trigger = ((IWorkflowProvider)shipment).WorkflowItems.AddNew();
			trigger.P9_Description = "Somehing";
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.AvailableToCode;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;
			action.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.AsPerPayload;

			Factory.Save();

			var dataWriter = trigger.WorkflowDescriptor.GetTestFileWriter(action, shipment);

			using (var tempDirectory = new TempDirectory())
			{
				var shipmentReference = shipment[JobShipmentSchema.JS_UniqueConsignRef].ToString();
				var exportResult = dataWriter.Export(tempDirectory.DirectoryName);

				AssertNotNull("exportResult", exportResult);
				CombineAssertions("exportResult", delegate
				{
					var messageLines = exportResult.Message.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
					AssertEquals("messageLines.Length", 2, messageLines.Length);
					var fileName = messageLines[1];
					AssertEquals(string.Format("File.Exists(\"{0}\")", fileName), true, File.Exists(fileName));
					AssertContains("fileName", tempDirectory.DirectoryName, fileName);
					AssertContains("fileName", "S_h_i_p_m_e_n_t", fileName);
					AssertEquals("fileName.EndsWith(\".xml\") failed on: " + fileName, true, fileName.EndsWith(".xml"));
				});
			}
		}

		[TestDate(2016, 7, 21, 16, 21, 22, 567)]
		public void TestEventExportWritesAttachedDocumentToXmlFileOnlyWhenActionIsXUE()
		{
			var docSource = Factory.NewWithValidTestData<RefDocSource>();
			docSource.RDS_Code = "AZA";
			docSource.RDS_Desc = "Aaaaa!";
			Factory.Save();

			AssertWhetherAttachedDocumentIsWrittenUnderActionTypeSpecified(true, WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXMLWithEDoc);
			AssertWhetherAttachedDocumentIsWrittenUnderActionTypeSpecified(false, WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML);
		}

		void AssertWhetherAttachedDocumentIsWrittenUnderActionTypeSpecified(bool isAttachedDocumentWritten, string actionType)
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			var eDoc = ((IDocManagerSupport)shipment).DocManagerInfo.AddFileOrDocument(new byte[] { 1, 2, 3 }, "TestCartageAdviceFile", "CAD", false, Guid.Empty, Guid.Empty, Guid.Empty, documentSource: "AZA");
			ZString reference = "CAD|" + eDoc.UniqueKey.ToString();

			var trigger = ((IWorkflowProvider)shipment).WorkflowItems.AddNew();
			trigger.P9_Description = "Somehing";
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.AvailableToCode;
			trigger.TriggerConditions.TriggerConditionValue = reference;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = actionType;
			action.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.AsPerPayload;

			((IDocManagerSupport)shipment).DocManagerInfo.MasterFactory.Save();
			Factory.Save();

			var dataWriter = trigger.WorkflowDescriptor.GetTestFileWriter(action, shipment);

			using (var tempDirectory = new TempDirectory())
			{
				var shipmentReference = shipment[JobShipmentSchema.JS_UniqueConsignRef].ToString();
				var exportResult = dataWriter.Export(tempDirectory.DirectoryName);

				AssertNotNull("exportResult", exportResult);
				CombineAssertions("exportResult", delegate
				{
					AssertContains("Text", string.Format("UniversalEvent from [{0}] saved", shipmentReference), exportResult.Message);
					var messageLines = exportResult.Message.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
					AssertEquals("messageLines.Length", 2, messageLines.Length);
					var fileName = messageLines[1];
					AssertEquals(string.Format("File.Exists(\"{0}\")", fileName), true, File.Exists(fileName));

					var fileText = File.ReadAllText(fileName);

					var attachedDocument =
$@"<AttachedDocumentCollection>
      <AttachedDocument>
        <FileName>TestCartageAdviceFile</FileName>
        <ImageData>AQID</ImageData>
        <Type>
          <Code>CAD</Code>
          <Description>Cartage Advice</Description>
        </Type>
        <DocumentID>{eDoc.UniqueKey}</DocumentID>
        <FileSizeInBytes>3</FileSizeInBytes>
        <IsPublished>true</IsPublished>
        <SaveDateUTC>2016-07-21T16:21:22.567</SaveDateUTC>
        <SavedBy>
          <Code>E</Code>
          <Name>CargoWise Support</Name>
        </SavedBy>
        <Source>
          <Code>AZA</Code>
          <Description>Aaaaa!</Description>
        </Source>
        <VisibleBranchCode></VisibleBranchCode>
        <VisibleCompanyCode></VisibleCompanyCode>
        <VisibleDepartmentCode></VisibleDepartmentCode>
      </AttachedDocument>
    </AttachedDocumentCollection>";

					AssertEquals(isAttachedDocumentWritten, fileText.Contains(attachedDocument));
				});
			}
		}

		[TestDate(2016, 7, 21, 16, 21, 22, 567)]
		public void TestWriteCompanyBranchDepartmentSpecificDocument()
		{
			var docSource = Factory.NewWithValidTestData<RefDocSource>();
			docSource.RDS_Code = "AZA";
			docSource.RDS_Desc = "Aaaaa!";
			Factory.Save();

			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			var eDoc = ((IDocManagerSupport)shipment).DocManagerInfo.AddFileOrDocument(new byte[] { 1, 2, 3 }, "TestCartageAdviceFile", "CAD", false, GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), documentSource: "");
			ZString reference = "CAD|" + eDoc.UniqueKey.ToString();

			var trigger = ((IWorkflowProvider)shipment).WorkflowItems.AddNew();
			trigger.P9_Description = "Somehing";
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.AvailableToCode;
			trigger.TriggerConditions.TriggerConditionValue = reference;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXMLWithEDoc;
			action.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.AsPerPayload;

			((IDocManagerSupport)shipment).DocManagerInfo.MasterFactory.Save();
			Factory.Save();

			var dataWriter = trigger.WorkflowDescriptor.GetTestFileWriter(action, shipment);

			using (var tempDirectory = new TempDirectory())
			{
				var shipmentReference = shipment[JobShipmentSchema.JS_UniqueConsignRef].ToString();
				var exportResult = dataWriter.Export(tempDirectory.DirectoryName);

				AssertNotNull("exportResult", exportResult);
				CombineAssertions("exportResult", delegate
				{
					AssertContains("Text", string.Format("UniversalEvent from [{0}] saved", shipmentReference), exportResult.Message);
					var messageLines = exportResult.Message.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
					AssertEquals("messageLines.Length", 2, messageLines.Length);
					var fileName = messageLines[1];
					AssertEquals(string.Format("File.Exists(\"{0}\")", fileName), true, File.Exists(fileName));

					var fileText = File.ReadAllText(fileName);

					var attachedDocument =
$@"<AttachedDocumentCollection>
      <AttachedDocument>
        <FileName>TestCartageAdviceFile</FileName>
        <ImageData>AQID</ImageData>
        <Type>
          <Code>CAD</Code>
          <Description>Cartage Advice</Description>
        </Type>
        <DocumentID>{eDoc.UniqueKey}</DocumentID>
        <FileSizeInBytes>3</FileSizeInBytes>
        <IsPublished>true</IsPublished>
        <SaveDateUTC>2016-07-21T16:21:22.567</SaveDateUTC>
        <SavedBy>
          <Code>E</Code>
          <Name>CargoWise Support</Name>
        </SavedBy>
        <Source>
          <Code></Code>
          <Description></Description>
        </Source>
        <VisibleBranchCode>{eDoc.VisibleBranchCode}</VisibleBranchCode>
        <VisibleCompanyCode>{eDoc.VisibleCompanyCode}</VisibleCompanyCode>
        <VisibleDepartmentCode>{eDoc.VisibleDepartmentCode}</VisibleDepartmentCode>
      </AttachedDocument>
    </AttachedDocumentCollection>";

					AssertContains(attachedDocument, fileText);
				});
			}
		}
	}
}
