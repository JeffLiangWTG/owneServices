using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;

namespace Enterprise.Freight.DataTransfer.Testing
{
	class ShipmentUXmlReWriterHackBeingReplacedSoonTest : TestCaseWithFactory
	{
		public void TestWriteXML_UniversalShipment()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				var shipment = new Shipment();
				shipment.DataContext = GetFilledInDataContext(UniversalXmlInfo.Namespace_2012_11);

				using (var stream = (SubStreamableStream)new MemoryStream())
				{
					var writer = new ShipmentUXmlReWriterHackBeingReplacedSoon("http://www.cargowise.com/Schemas/Universal/2012/11/BookingConfirmation/1");
					writer.WriteXML(shipment, stream, UniversalXmlInfo.Namespace_2012_11);

					using (var reader = new StreamReader(stream))
					{
						var result = reader.ReadToEnd();
						AssertMultilineASCIIEquals("Serialized UniversalShipment", UniversalShipmentWithDataContext_2012_11.Trim(), result);
					}
				}
			}
		}

		public void TestWriteXML_CarrierUniversal()
		{
			var dataContext = GetFilledInDataContext(UniversalXmlInfo.Namespace_2011_11);
			using (Env.TemporarilySetNullEnvironmentInstanceForTesting())
			{
				var shipment = new Shipment();
				shipment.DataContext = dataContext;

				using (var stream = (SubStreamableStream)new MemoryStream())
				{
					var writer = new ShipmentUXmlReWriterHackBeingReplacedSoon("http://www.cargowise.com/Schemas/Universal/2012/11/BookingConfirmation/1");
					writer.WriteXML(shipment, stream, UniversalXmlInfo.Namespace_2011_11);

					using (var reader = new StreamReader(stream))
					{
						var result = reader.ReadToEnd();
						AssertMultilineASCIIEquals("Serialized UniversalShipment", UniversalShipmentWithDataContext_2011_11.Trim(), result);
					}
				}
			}
		}

		static IDataContextDataObject GetFilledInDataContext(string nameSpace = null)
		{
			var dataContext = DataContextFactory.New(nameSpace ?? SchemaVersionManager.Current.Namespace);
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.SetWorkflowInfo(new WorkflowInfo()
			{
				EventReference = "TriggerRef",
				EventType = new CodeDescriptionPair() { Code = "EVT", Description = "My Event" },
				EventUser = new Staff() { Code = "ATF", Name = "Awesome Top Fella" },
				EventBranch = new Branch() { Code = "UBR", Name = "Uber branch" },
				EventDepartment = new Department() { Code = "TDE", Name = "Uber department" },
				ActionPurpose = new CodeDescriptionPair() { Code = "ACT", Description = "My Action" },
				TriggerDescription = "Describe me a Trigger",
				TriggerCount = 2,
				TriggerDate = new ZDateTimeOffset(2012, 4, 13),
				TriggerReference = "*TriggerRef*",
				TriggerType = TriggerType.Manual,
				RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.BRO } }
			});
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "JOB01010101");

			var documentName = "TEST DOCUMENT";

			var purposeList = new CodeDescriptionPairList();
			purposeList.AddPair("TES", "Test Description");

			var isSystemDefined = false;

			dataContext.SetDocumentaryOverride(documentName, "TES", purposeList, isSystemDefined, 2, 1);

			return dataContext;
		}

		const string UniversalShipmentWithDataContext_2012_11 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11/BookingConfirmation/1"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key>JOB01010101</Key>
        <Type>DummyBusinessObject</Type>
      </DataSource>

      <DocumentaryOverride>
        <DataVersion>2</DataVersion>
        <DocumentName>TEST DOCUMENT</DocumentName>
        <IsSystemDefined>false</IsSystemDefined>
        <Purpose Description=""Test Description"">TES</Purpose>
        <SubmissionVersion>1</SubmissionVersion>
      </DocumentaryOverride>
      <Workflow>
        <ActionPurpose Description=""My Action"">ACT</ActionPurpose>
        <Company>
          <Code>EDI</Code>
          <Country Name=""Australia"">AU</Country>
          <Name>Eagle Datamation International</Name>
        </Company>
        <EventBranch Name=""Uber branch"">UBR</EventBranch>
        <EventDepartment Name=""Uber department"">TDE</EventDepartment>
        <EventReference>TriggerRef</EventReference>
        <EventType Description=""My Event"">EVT</EventType>
        <EventUser Name=""Awesome Top Fella"">ATF</EventUser>
        <TriggerCount>2</TriggerCount>
        <TriggerDate>2012-04-13T00:00:00.000+10:00</TriggerDate>
        <TriggerDescription>Describe me a Trigger</TriggerDescription>
        <TriggerReference>*TriggerRef*</TriggerReference>
        <TriggerType>Manual</TriggerType>

        <RecipientRoleCollection>
          <RecipientRole Description=""Broker"">BRO</RecipientRole>
        </RecipientRoleCollection>
      </Workflow>
    </DataContext>
  </Shipment>
</UniversalShipment>
";

		const string UniversalShipmentWithDataContext_2011_11 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11/BookingConfirmation/1"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>DummyBusinessObject</Type>
          <Key>JOB01010101</Key>
        </DataSource>
      </DataSourceCollection>

      <ActionPurpose>
        <Code>ACT</Code>
        <Description>My Action</Description>
      </ActionPurpose>
      <Company>
        <Code>EDI</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Eagle Datamation International</Name>
      </Company>
      <DataProvider>EDIDATEDI</DataProvider>
      <DocumentaryOverride>
        <DataVersion>2</DataVersion>
        <DocumentName>TEST DOCUMENT</DocumentName>
        <IsSystemDefined>false</IsSystemDefined>
        <Purpose>
          <Code>TES</Code>
          <Description>Test Description</Description>
        </Purpose>
        <SubmissionVersion>1</SubmissionVersion>
      </DocumentaryOverride>
      <EnterpriseID>EDI</EnterpriseID>
      <EventBranch>
        <Code>UBR</Code>
        <Name>Uber branch</Name>
      </EventBranch>
      <EventDepartment>
        <Code>TDE</Code>
        <Name>Uber department</Name>
      </EventDepartment>
      <EventReference>TriggerRef</EventReference>
      <EventType>
        <Code>EVT</Code>
        <Description>My Event</Description>
      </EventType>
      <EventUser>
        <Code>ATF</Code>
        <Name>Awesome Top Fella</Name>
      </EventUser>
      <ServerID>DAT</ServerID>
      <TriggerCount>2</TriggerCount>
      <TriggerDate>2012-04-13T00:00:00.000+10:00</TriggerDate>
      <TriggerDescription>Describe me a Trigger</TriggerDescription>
      <TriggerReference>*TriggerRef*</TriggerReference>
      <TriggerType>Manual</TriggerType>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>BRO</Code>
          <Description>Broker</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>
  </Shipment>
</UniversalShipment>
";
	}
}
