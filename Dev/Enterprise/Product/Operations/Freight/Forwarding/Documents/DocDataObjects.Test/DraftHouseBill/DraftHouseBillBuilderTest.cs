using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class DraftHouseBillBuilderTest : TestCaseWithFactory
	{
		#region Build

		public void TestBuild_RegularScenario()
		{
			var consol = CreateConsol();
			Factory.Save();

			ImportDraftBillOfLadingMessage(xmlMessageRegularScenario);
			var builder = new DraftHouseBillBuilder(consol, GetBuilderParameters(consol));
			var draftHouseBill = builder.Build();

			AssertHeaderFields(draftHouseBill);
			AssertAddresses(draftHouseBill);
			AssertContainers(draftHouseBill);
			AssertFreightCharges(draftHouseBill);
		}

		public void TestBuild_FallbackScenario()
		{
			var consol = CreateConsol();
			Factory.Save();

			ImportDraftBillOfLadingMessage(xmlMessageFallBackScenario);
			var builder = new DraftHouseBillBuilder(consol, GetBuilderParameters(consol));
			var draftHouseBill = builder.Build();

			AssertEquals("Is Draft", true, draftHouseBill.IsDraft);
			AssertEquals("Carrier Name", "HLCU", draftHouseBill.CarrierAgent);

			const string freightCharges = @"Freight Charges: Prepaid
Destination Haulage: Prepaid
Destination Port Charge: Prepaid
";
			AssertEquals("Freight Charges", freightCharges, draftHouseBill.FreightCharges);
		}

		public void TestBuild_GetAdditionalData()
		{
			var consol = CreateConsol();
			Factory.Save();

			ImportDraftBillOfLadingMessage(xmlMessageFallBackScenario);
			var builder = new DraftHouseBillBuilder(consol, GetBuilderParameters(consol));
			var draftHouseBill = builder.Build();

			AssertEquals("Is Draft", true, draftHouseBill.IsDraft);
			AssertEquals("Carrier Name", "HLCU", draftHouseBill.CarrierAgent);

			const string freightCharges = @"Freight Charges: Prepaid
Destination Haulage: Prepaid
Destination Port Charge: Prepaid
";
			AssertEquals("Freight Charges", freightCharges, draftHouseBill.FreightCharges);

			builder = new DraftHouseBillBuilder(consol, GetBuilderParameters(consol, false));
			draftHouseBill = builder.Build();

			AssertEquals("Is Draft", true, draftHouseBill.IsDraft);
			AssertEquals("Carrier Name", "HLCU", draftHouseBill.CarrierAgent);
			AssertEquals("Freight Charges", freightCharges, draftHouseBill.FreightCharges);
		}

		public void TestBuild_EmptyUniversalShipment_NoExceptionThrown()
		{
			var consol = CreateConsol();
			Factory.Save();

			ImportDraftBillOfLadingMessage(xmlMessageEmptyUniversalShipment);
			var builder = new DraftHouseBillBuilder(consol, GetBuilderParameters(consol));

			AssertNoExceptionThrown(() => builder.Build());
		}

		void AssertHeaderFields(DraftHouseBill draftHouseBill)
		{
			AssertEquals("Is Draft", true, draftHouseBill.IsDraft);
			AssertEquals("Carrier Name", "HAPAG-LLOYD", draftHouseBill.CarrierAgent);
			AssertEquals("Booking No.", "23380355", draftHouseBill.CarrierBookingReference);
			AssertEquals("B/L No.", "HLCUSZX2209BHYC4", draftHouseBill.HouseBillNumber);
			AssertEquals("Export References", "C09140756", draftHouseBill.ExportReference);
			AssertEquals("Shipper's Reference", "CON0000001084", draftHouseBill.ShippersReference);
			AssertEquals("Forwarding Agent Reference", "CYYY00692899", draftHouseBill.FreightForwarderReference);
			AssertEquals("Shipment Method", "FCL", draftHouseBill.ContainerMode.Code);
			AssertEquals("Delivery Mode", "PTP", draftHouseBill.DeliveryMode);
			AssertEquals("Service Contract No.", "64645802", draftHouseBill.CarrierContractNumber);
			AssertEquals("Pre-Carriage By", "Sea", draftHouseBill.PreCarriageBy);
			AssertEquals("Vessal & Voyage No.", "DACHAN BAY EXPRESS 004W", string.Concat(draftHouseBill.VesselName, " ", draftHouseBill.Voyage));
			AssertEquals("Lloyd's No.", "XX456", draftHouseBill.LloydsIMO);
			AssertEquals("Place of Receipt", "BEANR-Antwerpen", string.Concat(draftHouseBill.PlaceOfReceipt.Code, "-", draftHouseBill.PlaceOfReceipt.Name));
			AssertEquals("Place of Delivery", "AUSYD-Sydney", string.Concat(draftHouseBill.PlaceOfDelivery.Code, "-", draftHouseBill.PlaceOfDelivery.Name));
			AssertEquals("Port of Load", "DEHAM-Hamburg", string.Concat(draftHouseBill.PortOfLoading.Code, "-", draftHouseBill.PortOfLoading.Name));
			AssertEquals("Port of Discharge", "FRPAR-Paris", string.Concat(draftHouseBill.PortOfDischarge.Code, "-", draftHouseBill.PortOfDischarge.Name));
		}

		void AssertAddresses(DraftHouseBill draftHouseBill)
		{
			AssertAddress(draftHouseBill.Shipper, "QWERTY", "123 QWERTY STREET", "Building X", "ANDORRA LA VELLA", "07", "AD", "Andorra", "1234", "Jos Vermeulen", "987654610", "12345678900", "Jos.Vermeulen@qwerty.com");
			AssertAddress(draftHouseBill.Consignee, "AZERTY", "123 AZERTY STREET", "Building X", "ANDORRA LA VELLA", "07", "AD", "Andorra", "1234", "Jos Vermeulen", "987654610", "12345678900", "Jos.Vermeulen@AZERTY.com");
			AssertAddress(draftHouseBill.NotifyParty, "HollekeBolleke", "123 HollekeBolleke STREET", "Building X", "ANDORRA LA VELLA", "07", "AD", "Andorra", "1234", "Jos Vermeulen", "987654610", "12345678900", "Jos.Vermeulen@HollekeBolleke.com");
		}

		void AssertAddress(IAddress address, ZString companyName, ZString address1, ZString address2, ZString city, ZString state, ZString countryCode, ZString countryName, ZString postcode, ZString contact, ZString phone, ZString fax, ZString email)
		{
			AssertEquals("Company Name", companyName, address.CompanyName);
			AssertEquals("Address 1", address1, address.AddressLine1);
			AssertEquals("Address 2", address2, address.AddressLine2);
			AssertEquals("City", city, address.City);
			AssertEquals("State", state, address.State);
			AssertEquals("Country Code", countryCode, address.Country.Code);
			AssertEquals("Country Name", countryName, address.Country.Name);
			AssertEquals("Postcode", postcode, address.Postcode);
			AssertEquals("Contact", contact, address.Contact);
			AssertEquals("Phone", phone, address.Phone);
			AssertEquals("Fax", fax, address.Fax);
			AssertEquals("Email", email, address.Email);
		}

		void AssertContainers(DraftHouseBill draftHouseBill)
		{
			const string goodsDescription1 = @"AAA packline 2
11 BAG of 0001C Danger Technicals Class A SUB1 SUB2 10 C c.c. PG III Y Y F-B S-X Contact Jos Vermeulen +33456784512 Net Weight 142.01 KG
";
			AssertContainer(draftHouseBill.Containers.ToList()[0], "AAA", "SEAL1", "SEAL2", "SEAL3", "20FR", "22P1", 272m, "KG", 456m, "M", "PLT", 2, "Marks 785\r\n", goodsDescription1);
			const string goodsDescription2 = @"BBB packline 2
BBB packline 1
11 BAG of 0001C Danger Technicals Class A SUB1 SUB2 10 C c.c. PG III Y Y F-B S-X Contact Jos Vermeulen +33456784512 Net Weight 142.01 KG
";
			AssertContainer(draftHouseBill.Containers.ToList()[1], "BBB", "S1", "S2", "S3", "20FR", "22P1", 785, "KG", 13, "M", "PKG", 7, "Marks 785\r\n", goodsDescription2);
		}

		void AssertContainer(DraftHouseBillContainer container, ZString containerNumber, ZString seal, ZString secondSeal, ZString thirdSeal, ZString typeCode, ZString isoCode, ZDecimal grossWeight, ZString grossWeightUnit, ZDecimal volumeCapacity, ZString volumeCapacityUnit, ZString packType, ZInt packCount, ZString marksAndNumbers, ZString goodsDescription)
		{
			AssertEquals("Container Number", containerNumber, container.Number);
			AssertEquals("Seal", seal, container.Seal);
			AssertEquals("SecondSeal", secondSeal, container.SecondSeal);
			AssertEquals("ThirdSeal", thirdSeal, container.ThirdSeal);
			AssertEquals("Type Code", typeCode, container.Type.Code);
			AssertEquals("ISO Code", isoCode, container.Type.ISOCode);
			AssertEquals("Grossweight", grossWeight, container.GrossWeight.Value);
			AssertEquals("Grossweight Unit", grossWeightUnit, container.GrossWeight.Unit.Code);
			AssertEquals("VolumeCapacity", volumeCapacity, container.VolumeCapacity.Value);
			AssertEquals("VolumeCapacity Unit", volumeCapacityUnit, container.VolumeCapacity.Unit.Code);
			AssertEquals("PackType", packType, container.PackType.Code);
			AssertEquals("PackCount", packCount, container.PackCount);
			AssertEquals("MarksAndNumbers", marksAndNumbers, container.MarksAndNumbers);
			AssertEquals("GoodsDescription", goodsDescription, container.GoodsDescription);
		}

		void AssertFreightCharges(DraftHouseBill draftHouseBill)
		{
			const string freightCharges = @"Freight Charges: Prepaid
Destination Haulage: Prepaid
Destination Port Charge: Prepaid
Origin Haulage: Collect
Origin Port Charge: Collect
";
			AssertEquals("Freight Charges", freightCharges, draftHouseBill.FreightCharges);
			AssertEquals("Freight Payable At", "BEANR-Antwerpen", string.Concat(draftHouseBill.FreightPayableAt.Code, "-", draftHouseBill.FreightPayableAt.Name));
			AssertEquals("Declared Value", 452m, draftHouseBill.DeclaredValueOfGoods.Amount);
			AssertEquals("Declared Value Currency", "EUR", draftHouseBill.DeclaredValueOfGoods.Currency.Code);
			AssertEquals("Place and Date of Issue", "AUSYD-Sydney 2024-05-11", string.Concat(draftHouseBill.PlaceOfIssue.Code, "-", draftHouseBill.PlaceOfIssue.Name, " ", draftHouseBill.DateOfIssue.ToString("yyyy-MM-dd")));
			AssertEquals("Shipped On Board Date", "2024-05-11", draftHouseBill.ShippedOnBoard.Date.ToString("yyyy-MM-dd"));
			const string clause = @"NoteText123
Something Else
HollekeBollekeRiebesolleke";
			AssertEquals("Bill Of Lading Clauses", clause, draftHouseBill.Clause);
		}

		#endregion

		#region	Visibility

		public void TestFormFilterMacro()
		{
			var multiModalQuery = new ZQuery()
				.AddToFilter(ZArchitecture.Schema.StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Contains, "Draft Bill Of Lading")
				.AddToFilter(ZArchitecture.Schema.StmMenuItemSchema.SU_BusinessContext, nameof(CargoWise.Definitions.BusinessContext.Consol))
				.AddToFilter(ZArchitecture.Schema.StmMenuItemSchema.SU_MenuPath, "Electronic Messaging/Carrier Messaging");
			var menuItems = Factory.Load<StmMenuItem>(multiModalQuery);

			AssertEquals(1, menuItems.Length);

			var menuItem = menuItems.First();
			var filterCondition = menuItem.SU_FilterList;
			AssertContains($"{menuItem.SU_MenuName}|{menuItem.PK} - filter contains correct condition",
				"@env.GetRegistryItem(\"ENABLEDRAFTBILLOFLADINGFORM\")",
				filterCondition);
		}

		#endregion

		#region Implementation

		ForwardingConsol CreateConsol()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001274";
			consol.JK_MasterBillNum = "BL091042970";
			consol.JK_AgentsReference = "AR001";
			consol.JK_BookingReference = "BR001";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_CarrierContractNumber = "11111";

			var shipper = CreateOrgHeaderWithAddress("BR1", "Consignor BR", "Consignor Address1 BR", "Consignor Address2 BR");
			consol.MasterBillShipperOverrideDocumentaryAddress.E2_OA_Address = shipper.MainAddress.PK;

			var carrier = CreateOrgHeaderWithAddress("BR2", "Carrier BR", "Carrier Address1 BR", "Carrier Address2 BR");
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var notifyParty = CreateOrgHeaderWithAddress("BR4", "NotifyParty BR", "NotifyParty Address1 BR", "NotifyParty Address2 BR");
			consol.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;

			var nacNumber = consol.Numbers.AddNew();
			nacNumber.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ContractNamedAccount;
			nacNumber.CE_EntryNum = "22222";

			var shipment = consol.Shipments.AddNew();
			var packingLine1 = shipment.OuterPackLines.AddNew();
			packingLine1.JL_PackageCount = 1;

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "1234567";
			container1.JC_ContainerCount = 2;
			container1.PackLines.Add(packingLine1);

			var packingLine2 = shipment.OuterPackLines.AddNew();
			packingLine2.JL_PackageCount = 1;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = string.Empty;
			container2.JC_ContainerCount = 2;
			container2.PackLines.Add(packingLine2);

			var transport = consol.Transports[0];
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport.JW_TransportType = Core.Constants.TransportPlanningType.OnForwarding;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "SGSIN";
			transport.JW_Vessel = "Dragon";
			transport.JW_VoyageFlight = "111";
			transport.JW_ETD = new ZDateTime(2018, 12, 1);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport2.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport2.JW_RL_NKLoadPort = "SGSIN";
			transport2.JW_RL_NKDiscPort = "CNSHA";
			transport2.JW_Vessel = "Black Hole";
			transport2.JW_VoyageFlight = "222";

			Factory.Save();

			return consol;
		}

		OrgHeader CreateOrgHeaderWithAddress(ZString code, ZString name, ZString address1, ZString address2)
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = code;
			orgHeader.OH_FullName = name;
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			orgHeader.MainAddress.Address1 = address1;
			orgHeader.MainAddress.Address2 = address2;
			orgHeader.MainAddress.City = "Sydney";
			orgHeader.MainAddress.Postcode = "2022";
			orgHeader.MainAddress.OA_RN_NKCountryCode = "AU";
			return orgHeader;
		}

		IDocDataObjectParameters GetBuilderParameters(ForwardingConsol consol, bool includeData = true)
		{
			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			var menuItem = CreateMenuItem(DataContext.HouseBill);

			return new DummyDocDataObjectParameters
			{
				Data = includeData ? supporter.GetAdditionalData(consol, menuItem).Right : null
			};
		}

		StmMenuItem CreateMenuItem(string context)
		{
			var template = Factory.New<StmTemplate>();
			template.SO_DataContext = context;

			var menuItem = Factory.New<DocumentCommand>();
			var pivot = Factory.New<StmMenuTemplatePivot>();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = menuItem.PK;

			return menuItem;
		}

		void ImportDraftBillOfLadingMessage(string xmlMessage)
		{
			var message = Factory.CreateDraftBillOfLadingMessage(xmlMessage);

			var logger = new UniversalXmlImportLogger();
			var universalFactory = new UniversalObjectFactory(Factory);

			message.ProcessUniversalMessage(universalFactory, logger);
			AssertContains("<DocumentName>Draft Bill Of Lading</DocumentName>", message.EM_MessageText, true);

			var logs = string.Join(" ", logger.Logs.Select(l => l.Message));
			AssertContains("Universal Shipment data was linked to Consol C00001274", logs, true);
		}

		sealed class UniversalXmlImportLogger : ISimpleLogger
		{
			public IEnumerable<ISimpleLog> Logs => logs;

			readonly List<ISimpleLog> logs = new List<ISimpleLog>();

			public void Clear() => logs.Clear();

			public void Log(LogType type, string message)
			{
				if (!string.IsNullOrWhiteSpace(message))
				{
					logs.Add(new SimpleLog(type, message));
				}
			}
		}

		const string xmlMessageRegularScenario = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Shipment>
		<DataContext>
			<Action>LinkOnly</Action>
			<DocumentaryOverride>
				<DocumentName>Draft Bill Of Lading</DocumentName>
			</DocumentaryOverride>
			<DataTargetCollection>
				<DataTarget>
					<Key>C00001274</Key>
					<Type>ForwardingConsol</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<BookingConfirmationReference>23380355</BookingConfirmationReference>
		<WayBillNumber>HLCUSZX2209BHYC4</WayBillNumber>
		<ContainerMode>FCL</ContainerMode>
		<DeliveryMode>PTP</DeliveryMode>
		<VesselName>DACHAN BAY EXPRESS</VesselName>
		<VoyageFlightNo>004W</VoyageFlightNo>
		<LloydsIMO>XX456</LloydsIMO>
		<PortOfDischarge>FRPAR</PortOfDischarge>
		<PortOfLoading>DEHAM</PortOfLoading>
		<PlaceOfReceipt>BEANR</PlaceOfReceipt>
		<PlaceOfDelivery>AUSYD</PlaceOfDelivery>
		<PlaceOfIssue>AUSYD</PlaceOfIssue>
		<GoodsValue>452</GoodsValue>
		<GoodsValueCurrency>EUR</GoodsValueCurrency>
		<AdditionalReferenceCollection>
			<AdditionalReference>
				<Type Description=""Shipper's Reference"">SHP</Type>
				<ReferenceNumber>CON0000001084</ReferenceNumber>
			</AdditionalReference>
			<AdditionalReference>
				<Type Description=""Freight Forwarder Reference"">FFW</Type>
				<ReferenceNumber>CYYY00692899</ReferenceNumber>
			</AdditionalReference>
			<AdditionalReference>
				<Type Description=""eHub Interchange ID"">HID</Type>
				<ReferenceNumber>C09140756</ReferenceNumber>
			</AdditionalReference>
			<AdditionalReference>
				<Type Description=""Carrier Contract Number"">CON</Type>
				<ReferenceNumber>64645802</ReferenceNumber>
			</AdditionalReference>
		</AdditionalReferenceCollection>
		<AddInfoCollection>
			<AddInfo>
				<Key>FreightPayableAt_Code</Key>
				<Value>BEANR</Value>
			</AddInfo>
			<AddInfo>
				<Key>FreightPayableAt_Name</Key>
				<Value>Antwerpen</Value>
			</AddInfo>
		</AddInfoCollection>
		<DateCollection>
			<Date>
				<Type>BillIssued</Type>
				<IsEstimate>false</IsEstimate>
				<Value>2024-05-11T00:00:00</Value>
			</Date>
			<Date>
				<Type>ShippedOnBoard</Type>
				<IsEstimate>false</IsEstimate>
				<Value>2024-05-11T00:00:00</Value>
			</Date>
		</DateCollection>
		<NoteCollection Content=""Partial"">
			<Note>
				<Description>Bill Clause Notes</Description>
				<IsCustomDescription>false</IsCustomDescription>
				<NoteText>NoteText123
Something Else
HollekeBollekeRiebesolleke</NoteText>
			</Note>
		</NoteCollection>
		<OrganizationAddressCollection>
			<OrganizationAddress>
				<AddressType>ShippingLineAddress</AddressType>
				<CompanyName>HAPAG-LLOYD</CompanyName>
				<Address1>Ballindamm 25</Address1>
				<Address2>Building X</Address2>
				<AddressOverride>false</AddressOverride>
				<City>Frankfurt</City>
				<Postcode>D-20095</Postcode>
				<State>Westfalen</State>
				<Country>DE</Country>
				<Contact>Jos Vermeulen</Contact>
				<Email>Jos.Vermeulen@hapag-lloyd.com</Email>
				<Fax>12345678900</Fax>
				<Phone>987654610</Phone>
				<RegistrationNumberCollection>
					<RegistrationNumber>
						<Type Description=""Standard Carrier Alpha Code (Sea)"">CCC</Type>
						<CountryOfIssue Name=""United States"">US</CountryOfIssue>
						<Value>HLCU</Value>
					</RegistrationNumber>
				</RegistrationNumberCollection>
			</OrganizationAddress>
			<OrganizationAddress>
				<AddressType>ConsignorDocumentaryAddress</AddressType>
				<CompanyName>QWERTY</CompanyName>
				<Address1>123 QWERTY STREET</Address1>
				<Address2>Building X</Address2>
				<AddressOverride>false</AddressOverride>
				<City>ANDORRA LA VELLA</City>
				<Postcode>1234</Postcode>
				<State>07</State>
				<Country>AD</Country>
				<Contact>Jos Vermeulen</Contact>
				<Email>Jos.Vermeulen@qwerty.com</Email>
				<Fax>12345678900</Fax>
				<Phone>987654610</Phone>
			</OrganizationAddress>
			<OrganizationAddress>
				<AddressType>ConsigneeDocumentaryAddress</AddressType>
				<CompanyName>AZERTY</CompanyName>
				<Address1>123 AZERTY STREET</Address1>
				<Address2>Building X</Address2>
				<AddressOverride>false</AddressOverride>
				<City>ANDORRA LA VELLA</City>
				<Postcode>1234</Postcode>
				<State>07</State>
				<Country>AD</Country>
				<Contact>Jos Vermeulen</Contact>
				<Email>Jos.Vermeulen@AZERTY.com</Email>
				<Fax>12345678900</Fax>
				<Phone>987654610</Phone>
			</OrganizationAddress>
			<OrganizationAddress>
				<AddressType>NotifyParty</AddressType>
				<CompanyName>HollekeBolleke</CompanyName>
				<Address1>123 HollekeBolleke STREET</Address1>
				<Address2>Building X</Address2>
				<AddressOverride>false</AddressOverride>
				<City>ANDORRA LA VELLA</City>
				<Postcode>1234</Postcode>
				<State>07</State>
				<Country>AD</Country>
				<Contact>Jos Vermeulen</Contact>
				<Email>Jos.Vermeulen@HollekeBolleke.com</Email>
				<Fax>12345678900</Fax>
				<Phone>987654610</Phone>
			</OrganizationAddress>
		</OrganizationAddressCollection>
		<PaymentHandlingInstructionCollection>
			<PaymentHandlingInstruction>
				<Category Description=""Freight"">FRT</Category>
				<PaymentMethod Description=""Prepaid"">PPD</PaymentMethod>
			</PaymentHandlingInstruction>
			<PaymentHandlingInstruction>
				<Category Description=""Destination Haulage"">DHC</Category>
				<PaymentMethod Description=""Prepaid"">PPD</PaymentMethod>
			</PaymentHandlingInstruction>
			<PaymentHandlingInstruction>
				<Category Description=""Destination Port"">DPC</Category>
				<PaymentMethod Description=""Prepaid"">PPD</PaymentMethod>
			</PaymentHandlingInstruction>
			<PaymentHandlingInstruction>
				<Category Description=""Origin Haulage"">OHC</Category>
				<PaymentMethod Description=""Collect"">CCX</PaymentMethod>
			</PaymentHandlingInstruction>
			<PaymentHandlingInstruction>
				<Category Description=""Origin Port"">OPC</Category>
				<PaymentMethod Description=""Collect"">CCX</PaymentMethod>
			</PaymentHandlingInstruction>
		</PaymentHandlingInstructionCollection>
		<ContainerCollection>
			<Container>
				<Link>1</Link>
				<ContainerNumber>AAA</ContainerNumber>
				<ContainerCount>1</ContainerCount>
				<ContainerType>
					<Code>20FR</Code>
					<Category Description=""Flat Rack"">FLT</Category>
					<Description>Twenty foot flatrack</Description>
					<ISOCode>22P1</ISOCode>
				</ContainerType>
				<GrossWeight>272</GrossWeight>
				<WeightUnit>KG</WeightUnit>
				<VolumeCapacity>456</VolumeCapacity>
				<VolumeUnit>M</VolumeUnit>
				<Seal>SEAL1</Seal>
				<SealPartyType Description=""Carrier"">CAR</SealPartyType>
				<SecondSeal>SEAL2</SecondSeal>
				<SecondSealPartyType Description=""Customs"">CUS</SecondSealPartyType>
				<ThirdSeal>SEAL3</ThirdSeal>
				<ThirdSealPartyType Description=""Terminal"">CTP</ThirdSealPartyType>
			</Container>
			<Container>
				<Link>2</Link>
				<ContainerNumber>BBB</ContainerNumber>
				<ContainerCount>1</ContainerCount>
				<ContainerType>
					<Code>20FR</Code>
					<Category Description=""Flat Rack"">FLT</Category>
					<Description>Twenty foot flatrack</Description>
					<ISOCode>22P1</ISOCode>
				</ContainerType>
				<GrossWeight>785</GrossWeight>
				<WeightUnit>KG</WeightUnit>
				<VolumeCapacity>13</VolumeCapacity>
				<VolumeUnit>M</VolumeUnit>
				<Seal>S1</Seal>
				<SealPartyType Description=""Carrier"">CAR</SealPartyType>
				<SecondSeal>S2</SecondSeal>
				<SecondSealPartyType Description=""Customs"">CUS</SecondSealPartyType>
				<ThirdSeal>S3</ThirdSeal>
				<ThirdSealPartyType Description=""Terminal"">CTP</ThirdSealPartyType>
			</Container>
		</ContainerCollection>
		<PackingLineCollection>
			<PackingLine>
				<PackQty>4</PackQty>
				<PackType Description=""Pallet"">PLT</PackType>
				<DetailedDescription>BBB packline 2</DetailedDescription>
				<MarksAndNos>Marks 785</MarksAndNos>
				<UNDGCollection>
					<UNDG>
						<PackQty>11</PackQty>
						<PackType>BAG</PackType>
						<UNDGCode>0001C</UNDGCode>
						<ProperShippingName>Danger</ProperShippingName>
						<TechicalName>Technicals</TechicalName>
						<IMOClass>A</IMOClass>
						<SubLabel1>SUB1</SubLabel1>
						<SubLabel2>SUB2</SubLabel2>
						<FlashPoint>10</FlashPoint>
						<PackingGroup>III</PackingGroup>
						<MarinePollutant>Y</MarinePollutant>
						<PackedInLimitedQuantity>true</PackedInLimitedQuantity>
						<EmergencyScheduleFire>F-B</EmergencyScheduleFire>
						<EmergencyScheduleSpillage>S-X</EmergencyScheduleSpillage>
						<Weight>142.01</Weight>
						<WeightUQ Description=""Kilograms"">KG</WeightUQ>
						<Contact>
							<FullName>Jos Vermeulen</FullName>
							<Phone>+33456784512</Phone>
						</Contact>
						<Standard>IAT</Standard>
					</UNDG>
				</UNDGCollection>
				<PackingLineCollection>
					<PackingLine>
						<ContainerLink>2</ContainerLink>
						<ContainerNumber>BBB</ContainerNumber>
						<PackQty>4</PackQty>
						<PackType Description=""Pallet"">PLT</PackType>
						<DetailedDescription>BBB packline 2</DetailedDescription>
						<MarksAndNos>Marks 785</MarksAndNos>
						<UNDGCollection>
							<UNDG>
								<PackQty>11</PackQty>
								<PackType>BAG</PackType>
								<UNDGCode>0001C</UNDGCode>
								<ProperShippingName>Danger</ProperShippingName>
								<TechicalName>Technicals</TechicalName>
								<IMOClass>A</IMOClass>
								<SubLabel1>SUB1</SubLabel1>
								<SubLabel2>SUB2</SubLabel2>
								<FlashPoint>10</FlashPoint>
								<PackingGroup>III</PackingGroup>
								<MarinePollutant>Y</MarinePollutant>
								<PackedInLimitedQuantity>true</PackedInLimitedQuantity>
								<EmergencyScheduleFire>F-B</EmergencyScheduleFire>
								<EmergencyScheduleSpillage>S-X</EmergencyScheduleSpillage>
								<Weight>142.01</Weight>
								<WeightUQ Description=""Kilograms"">KG</WeightUQ>
								<Contact>
									<FullName>Jos Vermeulen</FullName>
									<Phone>+33456784512</Phone>
								</Contact>
								<Standard>IAT</Standard>
							</UNDG>
						</UNDGCollection>
					</PackingLine>
					<PackingLine>
						<ContainerLink>1</ContainerLink>
						<ContainerNumber>AAA</ContainerNumber>
						<PackQty>2</PackQty>
						<PackType Description=""Pallet"">PLT</PackType>
						<DetailedDescription>AAA packline 2</DetailedDescription>
						<MarksAndNos>Marks 785</MarksAndNos>
						<UNDGCollection>
							<UNDG>
								<PackQty>11</PackQty>
								<PackType>BAG</PackType>
								<UNDGCode>0001C</UNDGCode>
								<ProperShippingName>Danger</ProperShippingName>
								<TechicalName>Technicals</TechicalName>
								<IMOClass>A</IMOClass>
								<SubLabel1>SUB1</SubLabel1>
								<SubLabel2>SUB2</SubLabel2>
								<FlashPoint>10</FlashPoint>
								<PackingGroup>III</PackingGroup>
								<MarinePollutant>Y</MarinePollutant>
								<PackedInLimitedQuantity>true</PackedInLimitedQuantity>
								<EmergencyScheduleFire>F-B</EmergencyScheduleFire>
								<EmergencyScheduleSpillage>S-X</EmergencyScheduleSpillage>
								<Weight>142.01</Weight>
								<WeightUQ Description=""Kilograms"">KG</WeightUQ>
								<Contact>
									<FullName>Jos Vermeulen</FullName>
									<Phone>+33456784512</Phone>
								</Contact>
								<Standard>IAT</Standard>
							</UNDG>
						</UNDGCollection>
					</PackingLine>
					<PackingLine>
						<ContainerLink>2</ContainerLink>
						<ContainerNumber>BBB</ContainerNumber>
						<PackQty>3</PackQty>
						<PackType Description=""Pallet"">PLT</PackType>
						<DetailedDescription>BBB packline 1</DetailedDescription>
						<MarksAndNos>Marks 785</MarksAndNos>
						<UNDGCollection>
							<UNDG>
								<PackQty>11</PackQty>
								<PackType>BAG</PackType>
								<UNDGCode>0001C</UNDGCode>
								<ProperShippingName>Danger</ProperShippingName>
								<TechicalName>Technicals</TechicalName>
								<IMOClass>A</IMOClass>
								<SubLabel1>SUB1</SubLabel1>
								<SubLabel2>SUB2</SubLabel2>
								<FlashPoint>10</FlashPoint>
								<PackingGroup>III</PackingGroup>
								<MarinePollutant>Y</MarinePollutant>
								<PackedInLimitedQuantity>true</PackedInLimitedQuantity>
								<EmergencyScheduleFire>F-B</EmergencyScheduleFire>
								<EmergencyScheduleSpillage>S-X</EmergencyScheduleSpillage>
								<Weight>142.01</Weight>
								<WeightUQ Description=""Kilograms"">KG</WeightUQ>
								<Contact>
									<FullName>Jos Vermeulen</FullName>
									<Phone>+33456784512</Phone>
								</Contact>
								<Standard>IAT</Standard>
							</UNDG>
						</UNDGCollection>
					</PackingLine>
				</PackingLineCollection>
			</PackingLine>
		</PackingLineCollection>
		<TransportLegCollection>
			<TransportLeg>
				<PortOfDischarge>DEWVN</PortOfDischarge>
				<PortOfLoading>CNDCB</PortOfLoading>
				<LegOrder>1</LegOrder>
				<EstimatedArrival>2022-11-11T03:00:00</EstimatedArrival>
				<EstimatedDeparture>2022-10-15T20:00:00</EstimatedDeparture>
				<LegType>PreCarriage</LegType>
				<TransportMode>Sea</TransportMode>
				<VGMCutOff>2022-10-13T17:00:00</VGMCutOff>
				<DocumentCutOff>2022-10-12T16:00:00</DocumentCutOff>
				<FCLCutOff>2022-10-13T17:00:00</FCLCutOff>
				<VesselLloydsIMO>9539664</VesselLloydsIMO>
				<VesselName>DACHAN BAY EXPRESS</VesselName>
				<VoyageFlightNo>004W</VoyageFlightNo>
				<Carrier>
					<AddressType>Carrier</AddressType>
					<RegistrationNumberCollection>
						<RegistrationNumber>
							<Type>CCC</Type>
							<CountryOfIssue>US</CountryOfIssue>
							<Value>HLCU</Value>
						</RegistrationNumber>
					</RegistrationNumberCollection>
				</Carrier>
			</TransportLeg>
		</TransportLegCollection>
	</Shipment>
</UniversalShipment>";

		const string xmlMessageFallBackScenario = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Shipment>
		<DataContext>
			<Action>LinkOnly</Action>
			<DocumentaryOverride>
				<DocumentName>Draft Bill Of Lading</DocumentName>
			</DocumentaryOverride>
			<DataTargetCollection>
				<DataTarget>
					<Key>C00001274</Key>
					<Type>ForwardingConsol</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<OrganizationAddressCollection>
			<OrganizationAddress>
				<AddressType>ShippingLineAddress</AddressType>
				<CompanyName></CompanyName>
				<Address1>Ballindamm 25</Address1>
				<Address2>Building X</Address2>
				<AddressOverride>false</AddressOverride>
				<City>Frankfurt</City>
				<Postcode>D-20095</Postcode>
				<State>Westfalen</State>
				<Country>DE</Country>
				<Contact>Jos Vermeulen</Contact>
				<Email>Jos.Vermeulen@hapag-lloyd.com</Email>
				<Fax>12345678900</Fax>
				<Phone>987654610</Phone>
				<RegistrationNumberCollection>
					<RegistrationNumber>
						<Type Description=""Standard Carrier Alpha Code (Sea)"">CCC</Type>
						<CountryOfIssue Name=""United States"">US</CountryOfIssue>
						<Value>HLCU</Value>
					</RegistrationNumber>
				</RegistrationNumberCollection>
			</OrganizationAddress>
		</OrganizationAddressCollection>
		<PaymentHandlingInstructionCollection>
			<PaymentHandlingInstruction>
				<Category Description=""Freight"">FRT</Category>
				<PaymentMethod Description=""Prepaid"">PPD</PaymentMethod>
			</PaymentHandlingInstruction>
			<PaymentHandlingInstruction>
				<Category Description=""Destination Haulage"">DHC</Category>
				<PaymentMethod Description=""Prepaid"">PPD</PaymentMethod>
			</PaymentHandlingInstruction>
			<PaymentHandlingInstruction>
				<Category Description=""Destination Port"">DPC</Category>
				<PaymentMethod Description=""Prepaid"">PPD</PaymentMethod>
			</PaymentHandlingInstruction>
		</PaymentHandlingInstructionCollection>
	</Shipment>
</UniversalShipment>";

		const string xmlMessageEmptyUniversalShipment = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Shipment>
		<DataContext>
			<Action>LinkOnly</Action>
			<DocumentaryOverride>
				<DocumentName>Draft Bill Of Lading</DocumentName>
			</DocumentaryOverride>
			<DataTargetCollection>
				<DataTarget>
					<Key>C00001274</Key>
					<Type>ForwardingConsol</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
	</Shipment>
</UniversalShipment>";

		#endregion
	}
}
