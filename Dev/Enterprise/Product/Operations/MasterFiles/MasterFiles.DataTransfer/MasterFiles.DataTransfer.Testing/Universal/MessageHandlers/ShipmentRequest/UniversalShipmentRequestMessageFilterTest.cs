using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Workflow.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static NUnit.Framework.XmlAssertions;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	public class UniversalShipmentRequestMessageFilterTest : TestCaseWithFactory
	{
		public void TestMessageProfile_InvalidFilterType()
		{
			var messageFilter = @"<MessageProfile>
					<SchemaFilter>
						<Type>XYZ</Type>
						<FilterCollection>
							<Filter>
								<ElementName>ShipmentCollection</ElementName>
							</Filter>
							<Filter>
								<ElementName>AddInfoCollection</ElementName>
							</Filter>
						</FilterCollection>
					</SchemaFilter>
				</MessageProfile>";

			AssertShipmentRequestWithInvalidMessageProfile(messageFilter, "PRS", "<SchemaFilter>.<Type> - Invalid value [XYZ], valid values are [Include, Exclude].");
		}

		public void TestMessageProfile_MissingSchema()
		{
			var messageFilter = @"<MessageProfile>
				</MessageProfile>";

			AssertShipmentRequestWithInvalidMessageProfile(messageFilter, "PRS", "");
		}

		void AssertShipmentRequestWithInvalidMessageProfile(string messageProfileXml, string expectedStatus, string expectedLogs)
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S0001010";
			Factory.Save();

			var xml = string.Format(universalShipmentRequestTemplate, shipment.JS_UniqueConsignRef, messageProfileXml, "");

			var (result, logs) = ProcessRequest(xml);
			var resultXml = GetResultXml(result);

			AssertEquals(expectedStatus, result.Status);
			AssertContains(expectedLogs, logs);

			var collectionProperties = GetCollectionsFromUniversalShipment(resultXml);
			AssertGreaterThanOrEqualTo("No filter should be applied when the messsage profile is invalid", collectionProperties.Count(), 3);
		}

		public void TestMessageFilter_IncludeFilter()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S0001010";
			Factory.Save();

			var xml = $@"<UniversalShipmentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
			  <ShipmentRequest>
			    <DataContext>
			      <DataTargetCollection>
			        <DataTarget>
			          <Type>ForwardingShipment</Type>
			          <Key>{shipment.JS_UniqueConsignRef}</Key>
			        </DataTarget>
			      </DataTargetCollection>
			      <Company>
			        <Code>DEM</Code>
			      </Company>
			      <EnterpriseID>EDI</EnterpriseID>
			      <ServerID>DAT</ServerID>
			    </DataContext>
				<MessageProfile>
					<SchemaFilter>
						<Type>Include</Type>
						<FilterCollection>
							<Filter>
								<ElementName>PackingLineCollection</ElementName>
							</Filter>
							<Filter>
								<ElementName>DateCollection</ElementName>
							</Filter>
							<Filter>
								<ElementName>NOTACollection</ElementName>
							</Filter>
							<Filter>
								<ElementName></ElementName>
							</Filter>
							<Filter>
							</Filter>
						</FilterCollection>
					</SchemaFilter>
				</MessageProfile>
			  </ShipmentRequest>
			</UniversalShipmentRequest>";

			var (result, logs) = ProcessRequest(xml);
			var resultXml = GetResultXml(result);
			Assert(!logs.Contains("Warning"));

			var collectionProperties = GetCollectionsFromUniversalShipment(resultXml);
			AssertContainsExactElementsInAnyOrder(new[] { "DateCollection", "PackingLineCollection" }, collectionProperties);
		}

		public void TestMessageFilter_CustomizedFieldCollection()
		{
			var workflowTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			workflowTemplate.P0_ProcessType = "SHP";
			workflowTemplate.P0_SubType1 = "SEA";
			var customField1 = workflowTemplate.GenCustomColumnDefinitions.AddNew();
			customField1.XC_Name = "TestJobNo1";
			customField1.XC_Type = AddOnColumnDataType.Codes.String;
			Factory.Save();

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S0001010";
			shipment.JS_TransportMode = "SEA";
			var customBizo = ((ICustomFieldProvider)shipment).GetCustomBusinessObject();
			var propertyName = CustomPropertyHelper.GeneratePropertyIdentifier(customField1.XC_Name, typeof(ZString));
			var propertyValue = "CUSTOM VALUE";
			customBizo[propertyName] = propertyValue;

			Factory.Save();

			var xml = $@"<UniversalShipmentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
			  <ShipmentRequest>
			    <DataContext>
			      <DataTargetCollection>
			        <DataTarget>
			          <Type>ForwardingShipment</Type>
			          <Key>{shipment.JS_UniqueConsignRef}</Key>
			        </DataTarget>
			      </DataTargetCollection>
			      <Company>
			        <Code>DEM</Code>
			      </Company>
			      <EnterpriseID>EDI</EnterpriseID>
			      <ServerID>DAT</ServerID>
			    </DataContext>
				<MessageProfile>
					<SchemaFilter>
						<Type>FILTERTYPE</Type>
						<FilterCollection>
							<Filter>
								<ElementName>CustomizedFieldCollection</ElementName>
							</Filter>
						</FilterCollection>
					</SchemaFilter>
				</MessageProfile>
			  </ShipmentRequest>
			</UniversalShipmentRequest>";

			var (result, _) = ProcessRequest(xml.Replace("FILTERTYPE", "INCLUDE"));
			var resultXml = GetResultXml(result);

			AssertContains("CustomizedFieldCollection", resultXml);

			(result, _) = ProcessRequest(xml.Replace("FILTERTYPE", "EXCLUDE"));
			resultXml = GetResultXml(result);

			AssertNotContains("CustomizedFieldCollection", resultXml);
		}

		public void TestMessageFilter_IncludeFilterWithDuplicates()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S0001010";
			Factory.Save();

			var xml = $@"<UniversalShipmentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
			  <ShipmentRequest>
			    <DataContext>
			      <DataTargetCollection>
			        <DataTarget>
			          <Type>ForwardingShipment</Type>
			          <Key>{shipment.JS_UniqueConsignRef}</Key>
			        </DataTarget>
			      </DataTargetCollection>
			      <Company>
			        <Code>DEM</Code>
			      </Company>
			      <EnterpriseID>EDI</EnterpriseID>
			      <ServerID>DAT</ServerID>
			    </DataContext>
				<MessageProfile>
					<SchemaFilter>
						<Type>Include</Type>
						<FilterCollection>
							<Filter>
								<ElementName>PackingLineCollection</ElementName>
							</Filter>
							<Filter>
								<ElementName>PackingLineCollection</ElementName>
							</Filter>
						</FilterCollection>
					</SchemaFilter>
				</MessageProfile>
			  </ShipmentRequest>
			</UniversalShipmentRequest>";

			var (result, logs) = ProcessRequest(xml);
			var resultXml = GetResultXml(result);
			Assert(!logs.Contains("Warning"));

			var collectionProperties = GetCollectionsFromUniversalShipment(resultXml);
			AssertContainsExactElementsInAnyOrder(new[] { "PackingLineCollection" }, collectionProperties);
		}

		public void TestMessageFilter_IncludeFilterWithNoCollection()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S0001010";
			Factory.Save();

			var xml = $@"<UniversalShipmentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
			  <ShipmentRequest>
			    <DataContext>
			      <DataTargetCollection>
			        <DataTarget>
			          <Type>ForwardingShipment</Type>
			          <Key>{shipment.JS_UniqueConsignRef}</Key>
			        </DataTarget>
			      </DataTargetCollection>
			      <Company>
			        <Code>DEM</Code>
			      </Company>
			      <EnterpriseID>EDI</EnterpriseID>
			      <ServerID>DAT</ServerID>
			    </DataContext>
				<MessageProfile>
					<SchemaFilter>
						<Type>Include</Type>
					</SchemaFilter>
				</MessageProfile>
			  </ShipmentRequest>
			</UniversalShipmentRequest>";

			var (result, logs) = ProcessRequest(xml);
			var resultXml = GetResultXml(result);
			Assert(!logs.Contains("Warning"));

			var collectionProperties = GetCollectionsFromUniversalShipment(resultXml);
			AssertEquals("No collections are included", 0, collectionProperties.Count());
		}

		public void TestMessageFilter_ExcludeFilter()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S0001010";
			Factory.Save();

			var xml = $@"<UniversalShipmentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
			  <ShipmentRequest>
			    <DataContext>
			      <DataTargetCollection>
			        <DataTarget>
			          <Type>ForwardingShipment</Type>
			          <Key>{shipment.JS_UniqueConsignRef}</Key>
			        </DataTarget>
			      </DataTargetCollection>
			      <Company>
			        <Code>DEM</Code>
			      </Company>
			      <EnterpriseID>EDI</EnterpriseID>
			      <ServerID>DAT</ServerID>
			    </DataContext>
				<MessageProfile>
					<SchemaFilter>
						<Type>Exclude</Type>
						<FilterCollection>
							<Filter>
								<ElementName>PackingLineCollection</ElementName>
							</Filter>
							<Filter>
								<ElementName>DateCollection</ElementName>
							</Filter>
							<Filter>
								<ElementName>NOTACollection</ElementName>
							</Filter>
						</FilterCollection>
					</SchemaFilter>
				</MessageProfile>
			  </ShipmentRequest>
			</UniversalShipmentRequest>";

			var (result, logs) = ProcessRequest(xml);
			var resultXml = GetResultXml(result);
			Assert(!logs.Contains("Warning"));

			var collectionProperties = GetCollectionsFromUniversalShipment(resultXml);
			AssertEquals(false, collectionProperties.Contains("DateCollection"));
			AssertEquals(false, collectionProperties.Contains("PackingLineCollection"));
			AssertEquals(true, collectionProperties.Contains("DataSourceCollection"));
		}

		public void TestValidMessageProfilePurposeCode()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S0001010";

			var filter = Factory.New<EDIMessageContentFilter>();
			filter.ECF_Name = "ABC";
			filter.UniversalShipment.AdditionalConfiguration.PrimaryDataSource = EDIMessageContentPrimaryDataSource.Codes.Shipment;
			filter.UniversalShipment.FilterType = EDIMessageContentFilterTypes.Codes.Include;
			var filterLine = filter.UniversalShipment.Lines.AddNew();
			filterLine.SchemaElement = "DateCollection";

			var purpose = Factory.New<EDIMessagePurpose>();
			purpose.EMP_Code = "FFF";
			purpose.EMP_Description = "FFF";
			purpose.EMP_ECF_Filter = filter.PK;

			Factory.Save();

			var messageProfile = @"<MessageProfile>
					<SchemaFilter>
						<Type>Exclude</Type>
						<FilterCollection>
							<Filter>
								<ElementName>DateCollection</ElementName>
							</Filter>
						</FilterCollection>
					</SchemaFilter>
				</MessageProfile>";

			var xml = string.Format(universalShipmentRequestTemplate, shipment.JS_UniqueConsignRef, messageProfile, purpose.EMP_Code);

			var (result, logs) = ProcessRequest(xml);
			var resultXml = GetResultXml(result);

			var collectionProperties = GetCollectionsFromUniversalShipment(resultXml);
			AssertContainsExactElementsInAnyOrder(new[] { "DateCollection", }, collectionProperties);
			AssertCollectionNotContains(new[] { "DataSourceCollection" }, collectionProperties);
			AssertEquals(string.Format("Purpose Code '{0}' is linked to '{1}' EDI Message Profile, processing using EDI Message Profile '{1}'", purpose.EMP_Code, filter.ECF_Name), logs);
		}

		public void TestMessageProfilePurposeCodeWithoutEDIMessageContentFilter()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S0001010";

			var purpose = Factory.New<EDIMessagePurpose>();
			purpose.EMP_Code = "FFF";
			purpose.EMP_Description = "FFF";

			Factory.Save();

			var messageProfile = @"<MessageProfile>
					<SchemaFilter>
						<Type>Include</Type>
						<FilterCollection>
							<Filter>
								<ElementName>DateCollection</ElementName>
							</Filter>
						</FilterCollection>
					</SchemaFilter>
				</MessageProfile>";

			var xml = string.Format(universalShipmentRequestTemplate, shipment.JS_UniqueConsignRef, messageProfile, purpose.EMP_Code);

			var (result, logs) = ProcessRequest(xml);
			var resultXml = GetResultXml(result);

			var collectionProperties = GetCollectionsFromUniversalShipment(resultXml);
			AssertContainsExactElementsInAnyOrder(new[] { "DateCollection", }, collectionProperties);
			AssertEquals(string.Format("Warning - Purpose Code '{0}' is not linked to an EDI Message Profile\r\nFalling back to Schema Filter", purpose.EMP_Code), logs);
		}

		public void TestMessageProfileInvalidPurposeCode()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S0001010";

			Factory.Save();

			var messageProfile = @"<MessageProfile>
					<SchemaFilter>
						<Type>Include</Type>
						<FilterCollection>
							<Filter>
								<ElementName>DateCollection</ElementName>
							</Filter>
						</FilterCollection>
					</SchemaFilter>
				</MessageProfile>";

			var xml = string.Format(universalShipmentRequestTemplate, shipment.JS_UniqueConsignRef, messageProfile, "FFF");

			var (result, logs) = ProcessRequest(xml);
			var resultXml = GetResultXml(result);

			var collectionProperties = GetCollectionsFromUniversalShipment(resultXml);
			AssertContainsExactElementsInAnyOrder(new[] { "DateCollection", }, collectionProperties);
			AssertEquals("Purpose Code 'FFF' does not exist\r\nFalling back to Schema Filter", logs);
		}

		public void TestEDIMessageContentFilterPrimaryDataSource()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S0001010";

			var declaration = Factory.New<Customs.IBaseJobDeclaration>();
			declaration.JE_DeclarationReference = "D0001010";
			declaration.JE_JS = shipment.PK;
			declaration.JE_GC = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "DEM")).PK;

			var filter = Factory.New<EDIMessageContentFilter>();
			filter.ECF_Name = "ABC";
			filter.UniversalShipment.AdditionalConfiguration.PrimaryDataSource = EDIMessageContentPrimaryDataSource.Codes.Brokerage;

			var purpose = Factory.New<EDIMessagePurpose>();
			purpose.EMP_Code = "FFF";
			purpose.EMP_Description = "FFF";
			purpose.EMP_ECF_Filter = filter.PK;

			Factory.Save();

			var xml = string.Format(universalShipmentRequestTemplate, shipment.JS_UniqueConsignRef, "", purpose.EMP_Code);

			var (result, _) = ProcessRequest(xml);
			var resultXml = GetResultXml(result);

			AssertIsXml(resultXml)
				.HavingExactlyOneDescendantNode(node => node.WithName("DataSourceCollection")
					.HavingExactlyOneChildNode(node => node.WithName("DataSource"))
					.HavingExactlyOneChildNode(node => node.WithName("DataSource")
						.HavingExactlyOneChildNode(node => node.WithName("Type")
							.WithValue("CustomsDeclaration"))));
		}

		public void TestMessageFilter_QuotedBooking()
		{
			var bizo = Factory.New<Forwarding.IForwardingShipment>();
			bizo.JS_UniqueConsignRef = "S00001010";
			bizo.JS_BookingReference = "BLATTICUS";
			bizo.JS_IsBooking = true;
			bizo.JS_IsForwardRegistered = false;
			Factory.Save();

			var xml = $@"<UniversalShipmentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
			  <ShipmentRequest>
			    <DataContext>
			      <DataTargetCollection>
			        <DataTarget>
			          <Type>ForwardingBooking</Type>
			          <Key>{bizo.JS_UniqueConsignRef}</Key>
			        </DataTarget>
			      </DataTargetCollection>
			      <Company>
			        <Code>DEM</Code>
			      </Company>
			      <EnterpriseID>EDI</EnterpriseID>
			      <ServerID>DAT</ServerID>
			    </DataContext>
				<MessageProfile>
					<SchemaFilter>
						<Type>Include</Type>
						<FilterCollection>
							<Filter>
								<ElementName>PackingLineCollection</ElementName>
							</Filter>
							<Filter>
								<ElementName>DateCollection</ElementName>
							</Filter>
							<Filter>
								<ElementName>NOTACollection</ElementName>
							</Filter>
							<Filter>
								<ElementName></ElementName>
							</Filter>
							<Filter>
							</Filter>
						</FilterCollection>
					</SchemaFilter>
				</MessageProfile>
			  </ShipmentRequest>
			</UniversalShipmentRequest>";

			var (result, logs) = ProcessRequest(xml);
			var resultXml = GetResultXml(result);
			Assert(!logs.Contains("Warning"));

			var collectionProperties = GetCollectionsFromUniversalShipment(resultXml);
			AssertContainsExactElementsInAnyOrder(new[] { "DateCollection", "PackingLineCollection" }, collectionProperties);
		}

		public void TestMessageFilter_Consol()
		{
			var bizo = Factory.New<Forwarding.IForwardingConsol>();
			((BusinessObject)bizo).FillWithValidTestData();
			bizo.JK_UniqueConsignRef = "C00001010";
			Factory.Save();

			var xml = $@"<UniversalShipmentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
			  <ShipmentRequest>
			    <DataContext>
			      <DataTargetCollection>
			        <DataTarget>
			          <Type>ForwardingConsol</Type>
			          <Key>{bizo.JK_UniqueConsignRef}</Key>
			        </DataTarget>
			      </DataTargetCollection>
			      <Company>
			        <Code>DEM</Code>
			      </Company>
			      <EnterpriseID>EDI</EnterpriseID>
			      <ServerID>DAT</ServerID>
			    </DataContext>
				<MessageProfile>
					<SchemaFilter>
						<Type>Include</Type>
						<FilterCollection>
							<Filter>
								<ElementName>MilestoneCollection</ElementName>
							</Filter>
							<Filter>
								<ElementName>TransportLegCollection</ElementName>
							</Filter>
							<Filter>
								<ElementName>NOTACollection</ElementName>
							</Filter>
							<Filter>
								<ElementName></ElementName>
							</Filter>
							<Filter>
							</Filter>
						</FilterCollection>
					</SchemaFilter>
				</MessageProfile>
			  </ShipmentRequest>
			</UniversalShipmentRequest>";

			var (result, logs) = ProcessRequest(xml);
			var resultXml = GetResultXml(result);
			Assert(!logs.Contains("Warning"));

			var collectionProperties = GetCollectionsFromUniversalShipment(resultXml);
			AssertContainsExactElementsInAnyOrder(new[] { "MilestoneCollection", "TransportLegCollection" }, collectionProperties);
		}

		public void TestWriterStrategy()
		{
			var schemaFilter = new SchemaFilter();
			schemaFilter.Type = SchemaFilterType.Include;
			var messageProfile = new MessageProfile();
			messageProfile.SchemaFilter = schemaFilter;

			var handler = new UniversalShipmentRequestHandler_InternalsExposed(new XmlSessionTracker(new SimpleLogger()));
			var writerStrategy = handler.GetDataObjectWriterStrategyExposed(messageProfile);
			AssertEquals(false, writerStrategy is DefaultDataObjectWriterStrategy);

			writerStrategy = handler.GetDataObjectWriterStrategyExposed(null);
			AssertEquals(true, writerStrategy is DefaultDataObjectWriterStrategy);
		}

		(IHttpXmlProcessingResult, ZString logs) ProcessRequest(ZString xml, UniversalShipmentRequestHandler requestHandler = null)
		{
			var xmlSessionTracker = new XmlSessionTracker(new SimpleLogger());
			var handler = requestHandler ?? new UniversalShipmentRequestHandler(xmlSessionTracker);
			var request = handler.CreateRequestMessage();
			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				new StreamWriter(stream) { AutoFlush = true }.Write(xml);
				request.SetMessageTextSource(stream);
				request.Save();
			}

			var response = handler.Process(request);
			request.Save();

			return (response, xmlSessionTracker.ToString());
		}

		IEnumerable<string> GetCollectionsFromUniversalShipment(string xml)
		{
			var regex = new Regex("</.*Collection>");
			foreach (var match in regex.Matches(xml))
			{
				var str = match.ToString();
				yield return str.Substring(2, str.Length - 3);
			}
		}

		ZString GetResultXml(IHttpXmlProcessingResult result)
		{
			using (var streamReader = new StreamReader(result.ResponseMessageText))
			{
				return streamReader.ReadToEnd();
			}
		}

		readonly string universalShipmentRequestTemplate = @"<UniversalShipmentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
			  <ShipmentRequest>
			    <DataContext>
			      <DataTargetCollection>
			        <DataTarget>
			          <Type>ForwardingShipment</Type>
			          <Key>{0}</Key>
			        </DataTarget>
			      </DataTargetCollection>
			      <Company>
			        <Code>DEM</Code>
			      </Company>
			      <EnterpriseID>EDI</EnterpriseID>
			      <ServerID>DAT</ServerID>
			      <ActionPurpose>
                    <Code>{2}</Code>
                  </ActionPurpose>
			    </DataContext>
				{1}
			  </ShipmentRequest>
			</UniversalShipmentRequest>";
	}

	class UniversalShipmentRequestHandler_InternalsExposed : UniversalShipmentRequestHandler
	{
		public UniversalShipmentRequestHandler_InternalsExposed(IXmlSessionTracker xmlSessionTracker) : base(xmlSessionTracker)
		{
		}

		public IDataObjectWriterStrategy GetDataObjectWriterStrategyExposed(IMessageProfile messageProfile) => GetWriterStrategy(messageProfile);
	}
}
