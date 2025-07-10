using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.eTail.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using DummyLogger = Enterprise.UniversalDataBuss.Integration.DummyLogger;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.eTail.DataTransfer.Testing
{
	public class HVLVConsignmentDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestReadConsignmentsFromSubShipments_ShouldNotCreateHVLVItemCollectionForNonChangedConsignments()
		{
			var consolBO = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipmentBO = consolBO.Shipments.AddNew();
			shipmentBO.JS_ShipmentType = "HVL";
			shipmentBO.JS_HouseBill = "654321";

			var header = shipmentBO.GetOrCreateHVLVConsignmentHeader();

			var consignment1 = header.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "HVC001";
			consignment1.Items.AddNew();

			var consignment2 = header.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "HVC002";
			consignment2.Items.AddNew();

			var consignment3 = header.Consignments.AddNew();
			consignment3.HVC_ConsignmentId = "HVC003";
			consignment3.Items.AddNew();

			Factory.SaveForTesting();

			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.ShipmentType = new CodeDescriptionPair() { Code = "HVL" };
			shipmentDataObject.WayBillType = new WayBillType() { Code = "HWB" };
			shipmentDataObject.WayBillNumber = "654321";

			var subShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			subShipment.DataContext = DataContextFactory.New();
			subShipment.DataContext.AddDataTarget(DataContextType.HVLVConsignment, "HVC001");
			subShipment.TotalWeight = 10;
			subShipment.TotalWeightUnit = new UnitOfWeight() { Code = "T" };

			shipmentDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			shipmentDataObject.SubShipmentCollection.Add(subShipment);

			var newFactory = new UniversalObjectFactory();

			var initialCollectionCount = HVLVItemCollection.GetCollectionCount(newFactory.BOFactory, header.HCH_ClusterKey);
			AssertEquals("pre-condition", 0, initialCollectionCount);

			var loadedConsol = newFactory.BOFactory.Load<ForwardingConsol>(consolBO.PK);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, new DummyLogger(), newFactory, ChildShipmentsParent.ToChildShipmentsParent(loadedConsol));
			reader.ReadIntoBusinessObject();

			var increasedCollectionCount = HVLVItemCollection.GetCollectionCount(newFactory.BOFactory, header.HCH_ClusterKey) - initialCollectionCount;
			AssertEquals("Should create just 1 more HVLVItemCollection for XUS import as we only update 1 HVLVConsignment", 1, increasedCollectionCount);
		}

		public void TestReadConsignmentsFromSubShipments_ShouldNotCalculateChargeable()
		{
			var consolBO = Factory.NewWithValidTestData<ForwardingConsol>();

			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.ShipmentType = new CodeDescriptionPair() { Code = "HVL" };

			var subShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			shipmentDataObject.SubShipmentCollection.Add(subShipment);

			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packingLine.PackQty = 1;
			packingLine.Weight = 1234;

			subShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine });

			var reader = new ShipmentDataObjectReader(shipmentDataObject, new DummyLogger(), Factory, ChildShipmentsParent.ToChildShipmentsParent(consolBO));
			reader.ReadIntoBusinessObject();

			var consignment = Factory.Load<HVLVConsignment>(new ZQuery()).Single();
			AssertEquals("Chargeable not calculated", "Not Calculated", consignment.ChargeableForDisplay);
		}

		public void TestCalculateChargeableNotRunWhenImportingFromXml()
		{
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					AddressType = nameof(DocAddressType.ReturnAddress),
					CompanyName = "Test Company Name",
					Address1 = "99 Consignee Road",
					Address2 = "Downtown",
					City = "New York",
					State = "NY",
					Postcode = "12345",
					Country = new Country { Code = "US" },
					Contact = "Moo ray",
					Email = "murray.hewitt@usconsulate.gov.nz",
					Phone = "7",
					Mobile = "+1234567890",
					Fax = "+0987654321"
				}
			});

			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packingLine.PackQty = 1;
			packingLine.Weight = 1234;

			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine });
			shipment.PackingLineCollection.Content = CollectionContent.Complete;

			Factory.SaveForTesting();

			var consignment = new HVLVConsignmentDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			AssertEquals("Chargeable not calculated", "Not Calculated", consignment.ChargeableForDisplay);
		}

		public void TestReadInstructions()
		{
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetInstructionCollection(() => new DataObjectList<Instruction>
				{
					new Instruction { ServiceInstruction = "Leave at back" }
				});

			Factory.SaveForTesting();

			var consignment = new HVLVConsignmentDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			AssertEquals("Leave at back", consignment.HVC_ConsigneeInstructions);
		}

		public void TestReadNotes()
		{
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetNoteCollection(() => new DataObjectList<Note>
			{
				new Note()
				{
					Description = "Description",
					NoteText = "Leave at back"
				}
			});
			Factory.SaveForTesting();

			var consignment = new HVLVConsignmentDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			AssertEquals(1, consignment.Notes.FindByDescription("Description").Length);
			AssertEquals("Leave at back", consignment.Notes.FindByDescription("Description")[0].ST_NoteText);
		}

		public void TestPopulateAdditionalReferencesRemovesCustomsReferenceNumbersWhenCollectionContentIsComplete()
		{
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					AddressType = nameof(DocAddressType.ReturnAddress),
					CompanyName = "Test Company Name",
					Address1 = "99 Consignee Road",
					Address2 = "Downtown",
					City = "New York",
					State = "NY",
					Postcode = "12345",
					Country = new Country { Code = "US" },
					Contact = "Moo ray",
					Email = "murray.hewitt@usconsulate.gov.nz",
					Phone = "7",
					Mobile = "+1234567890",
					Fax = "+0987654321"
				}
			});

			shipment.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>()
				{
					new AdditionalReference()
					{
						Type = new EntryType { Code = "ISF", Description = "Importer Security Filing" },
						ReferenceNumber = "1234"
					},
					new AdditionalReference()
					{
						Type = new EntryType { Code = "ISF", Description = "Importer Security Filing" },
						ReferenceNumber = "2345"
					}
				});

			shipment.AdditionalReferenceCollection.Content = CollectionContent.Complete;

			Factory.SaveForTesting();

			var consignment = new HVLVConsignmentDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			AssertEquals(2, consignment.CustomsReferenceNumbers?.Count);

			shipment.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>()
				{
					new AdditionalReference()
					{
						Type = new EntryType { Code = "ISF", Description = "Importer Security Filing" },
						ReferenceNumber = "3456"
					}
				});

			consignment = new HVLVConsignmentDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			AssertEquals("PopulateAdditionalReferences removes existing additional references when AdditionalReferenceCollection is Complete", 1, consignment.CustomsReferenceNumbers?.Count);
		}

		public void TestPopulateGoodsValueAndValueIsPositiveNumberButGoodsCurrencyIsEmpty_ShouldThrowException()
		{
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.GoodsValue = 130;
			shipment.GoodsValueCurrency = new Currency();

			Factory.SaveForTesting();

			var expectedExceptionMessage = string.Join(System.Environment.NewLine, new[]
			{
				"Consignment goods value currency must not be empty when goods value is defined"
			});

			AssertExceptionThrown(typeof(DataObjectReadFailureException),
				expectedExceptionMessage,
				() => new HVLVConsignmentDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject());
		}

		public void TestPopulateGoodsValueButValueIsNegativeNumber_ShouldThrowException()
		{
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.GoodsValue = -1000;
			shipment.GoodsValueCurrency = new Currency();

			Factory.SaveForTesting();

			var expectedExceptionMessage = string.Join(System.Environment.NewLine, new[]
			{
				"Consignment goods value must be greater than or equal to 0, current value is: -1000"
			});

			AssertExceptionThrown(typeof(DataObjectReadFailureException),
				expectedExceptionMessage,
				() => new HVLVConsignmentDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject());
		}

		public void TestPopulateGoodsValueButCurrencyCodeIsInvalid_ShouldThrowException()
		{
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.GoodsValue = 1000;
			shipment.GoodsValueCurrency = new Currency();
			shipment.GoodsValueCurrency.Code = "BCD";
			shipment.GoodsValueCurrency.Description = "Test-Description";

			Factory.SaveForTesting();

			var expectedExceptionMessage = string.Join(System.Environment.NewLine, new[]
			{
				"Consignment goods value currency is invalid, invalid currency is:BCD"
			});

			AssertExceptionThrown(typeof(DataObjectReadFailureException),
				expectedExceptionMessage,
				() => new HVLVConsignmentDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject());
		}

		public void TestPopulateTransportValueAndInsuranceValue()
		{
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.InsuranceValue = 2000;
			shipment.TransportValue = 3000;

			Factory.SaveForTesting();

			var consignment = new HVLVConsignmentDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			CombineAssertions(() =>
			{
				AssertEquals((ZDecimal)2000, consignment.HVC_InsuranceValue);
				AssertEquals((ZDecimal)3000, consignment.HVC_TransportValue);
			});
		}

		public void TestPopulateOrganizationsCaching()
		{
			var arrivalCFS = Factory.NewWithValidTestData<OrgHeader>();
			arrivalCFS.OH_Code = "ARVCFS";

			var lastMileDelivery = Factory.NewWithValidTestData<OrgHeader>();
			lastMileDelivery.OH_Code = "LCLDLVRY";

			var lastMileCarrierBookingAgent = Factory.NewWithValidTestData<OrgHeader>();
			lastMileCarrierBookingAgent.OH_Code = "BKGAGT1";

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				new OrganizationAddress { AddressType = nameof(DocAddressType.ArrivalCFSAddress), OrganizationCode = "ARVCFS" },
				new OrganizationAddress { AddressType = AddressTypes.DeliveryLocalCartage, OrganizationCode = "LCLDLVRY" },
				new OrganizationAddress { AddressType = nameof(DocAddressType.CarrierBookingAgent), OrganizationCode = "BKGAGT1" }
			});

			Factory.SaveForTesting();

			var arrivalCacheKey = getCacheKey(nameof(DocAddressType.ArrivalCFSAddress), arrivalCFS.OH_Code);
			OrgAddress arrivalCFSDataObject;

			var lastMileDeliveryCacheKey = getCacheKey(AddressTypes.DeliveryLocalCartage, lastMileDelivery.OH_Code);
			OrgAddress lastMileDeliveryDataObject;

			var lastMileCarrierCacheKey = getCacheKey(nameof(DocAddressType.CarrierBookingAgent), lastMileCarrierBookingAgent.OH_Code);
			OrgAddress lastMileCarrierBookingAgentDataObject;

			Factory.BOFactory.TryGetValueFromCacheOnly(arrivalCacheKey, out arrivalCFSDataObject);
			Factory.BOFactory.TryGetValueFromCacheOnly(lastMileDeliveryCacheKey, out lastMileDeliveryDataObject);
			Factory.BOFactory.TryGetValueFromCacheOnly(lastMileCarrierCacheKey, out lastMileCarrierBookingAgentDataObject);

			CombineAssertions("OrgAddresses are uncached", () =>
			{
				AssertEquals("pre-condition arrivalCFSDataObject is null and uncached", null, arrivalCFSDataObject);
				AssertEquals("pre-condition lastMileDeliveryDataObject is null and uncached", null, lastMileDeliveryDataObject);
				AssertEquals("pre-condition lastMileCarrierBookingAgentDataObject is null and uncached", null, lastMileCarrierBookingAgentDataObject);
			});

			var consignment = new HVLVConsignmentDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();

			Factory.BOFactory.TryGetValueFromCacheOnly(arrivalCacheKey, out arrivalCFSDataObject);
			Factory.BOFactory.TryGetValueFromCacheOnly(lastMileDeliveryCacheKey, out lastMileDeliveryDataObject);
			Factory.BOFactory.TryGetValueFromCacheOnly(lastMileCarrierCacheKey, out lastMileCarrierBookingAgentDataObject);

			CombineAssertions("OrgAddresses are cached", () =>
			{
				AssertType<OrgAddress>("post-condition arrivalCFSDataObject is cached", arrivalCFSDataObject);
				AssertType<OrgAddress>("post-condition lastMileDeliveryDataObject is cached", lastMileDeliveryDataObject);
				AssertType<OrgAddress>("post-condition lastMileCarrierBookingAgentDataObject is cached", lastMileCarrierBookingAgentDataObject);
			});

			consignment = new HVLVConsignmentDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();

			string getCacheKey(string address, string code)
			{
				return nameof(HVLVConsignmentDataObjectReader) + "_" + address + code;
			}
		}

		#region PopulateReturnLocationDetails

		public void TestPopulateReturnLocationDetails_WhenOverrideIsNull_MatchingAddressAndContactExist_ShouldUseMatchedData()
		{
			TestPopulateReturnLocationDetails_WhenMatchingAddressAndContactExist_ShouldUseMatchedData(null);
		}

		public void TestPopulateReturnLocationDetails_WhenOverrideIsFalse_MatchingAddressAndContactExist_ShouldUseMatchedData()
		{
			TestPopulateReturnLocationDetails_WhenMatchingAddressAndContactExist_ShouldUseMatchedData(false);
		}

		void TestPopulateReturnLocationDetails_WhenMatchingAddressAndContactExist_ShouldUseMatchedData(ZBool? addressOverride)
		{
			var matchingOrgCode = "ORG001";
			var matchingContact = "Sales";
			var returnLocation = CreateOrgHeader(matchingOrgCode, matchingContact, companyName, address1, address2, city, state, postcode, countryCode, email, phone, mobile, fax);
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				CreateOrganizationAddress(DocAddressType.ReturnAddress, addressOverride, matchingOrgCode, matchingContact)
			});

			var consignment = new HVLVConsignmentDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			AssertReturnLocation(consignment, returnLocation.MainAddress.PK, companyName, address1, address2, city, state, postcode, countryCode, matchingContact, email, phone, mobile, fax);
		}

		public void TestPopulateReturnLocationDetails_WhenOverrideIsNull_NoMatchingContactExists_ShouldUseXUSContact()
		{
			TestPopulateReturnLocationDetails_WhenNoMatchingContactExists_ShouldUseXUSContact(null);
		}

		public void TestPopulateReturnLocationDetails_WhenOverrideIsFalse_NoMatchingContactExists_ShouldUseXUSContact()
		{
			TestPopulateReturnLocationDetails_WhenNoMatchingContactExists_ShouldUseXUSContact(null);
		}

		void TestPopulateReturnLocationDetails_WhenNoMatchingContactExists_ShouldUseXUSContact(ZBool? addressOverride)
		{
			var matchingOrgCode = "ORG001";
			var returnLocation = CreateOrgHeader(matchingOrgCode, "Unmatched Contact", companyName, address1, address2, city, state, postcode, countryCode);
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				CreateOrganizationAddress(DocAddressType.ReturnAddress, addressOverride, matchingOrgCode, contactName, "", "", "", "", "", "", "", email, phone, mobile, fax)
			});

			var consignment = new HVLVConsignmentDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			AssertReturnLocation(consignment, returnLocation.MainAddress.PK, companyName, address1, address2, city, state, postcode, countryCode, contactName, email, phone, mobile, fax);
		}

		public void TestPopulateReturnLocationDetails_WhenOverrideIsNull_NoMatchingAddressExists_ShouldUseXUSData()
		{
			TestPopulateReturnLocationDetails_WhenNoMatchingAddressExists_ShouldUseXUSData(null);
		}

		public void TestPopulateReturnLocationDetails_WhenOverrideIsFalse_NoMatchingAddressExists_ShouldUseXUSData()
		{
			TestPopulateReturnLocationDetails_WhenNoMatchingAddressExists_ShouldUseXUSData(null);
		}

		void TestPopulateReturnLocationDetails_WhenNoMatchingAddressExists_ShouldUseXUSData(ZBool? addressOverride)
		{
			var returnLocation = CreateOrgHeader("UNMATCH");
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				CreateOrganizationAddress(DocAddressType.ReturnAddress, addressOverride, orgCode, contactName, companyName, address1, address2, city, state, postcode, countryCode, email, phone, mobile, fax)
			});

			var consignment = new HVLVConsignmentDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			AssertReturnLocation(consignment, null, companyName, address1, address2, city, state, postcode, countryCode, contactName, email, phone, mobile, fax);
		}

		public void TestPopulateReturnLocationDetails_WhenOverrideIsTrue_ShouldUseXUSData()
		{
			var returnLocation = CreateOrgHeader(orgCode);
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				CreateOrganizationAddress(DocAddressType.ReturnAddress, true, orgCode, contactName, companyName, address1, address2, city, state, postcode, countryCode, email, phone, mobile, fax)
			});

			var consignment = new HVLVConsignmentDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			AssertReturnLocation(consignment, null, companyName, address1, address2, city, state, postcode, countryCode, contactName, email, phone, mobile, fax);
		}

		public void TestPopulateReturnLocationDetails_WhenConsignmentExists_ShouldUpdateAddress()
		{
			var returnLocation = CreateOrgHeader(orgCode);
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				CreateOrganizationAddress(DocAddressType.ReturnAddress, true, orgCode, contactName, companyName, address1, address2, city, state, postcode, countryCode, email, phone, mobile, fax)
			});

			var existingConsignment = Factory.NewWithValidTestData<HVLVConsignment>();
			existingConsignment.HVC_OA_ReturnLocation = returnLocation.MainAddress.PK;

			var consignment = new HVLVConsignmentDataObjectReader(shipment, new DummyLogger(), Factory, existingConsignment).ReadIntoBusinessObject();
			AssertReturnLocation(consignment, null, companyName, address1, address2, city, state, postcode, countryCode, contactName, email, phone, mobile, fax);
		}

		void AssertReturnLocation(
			HVLVConsignment consignment,
			ZGuid? addressPk,
			string name,
			string address1,
			string address2,
			string city,
			string state,
			string postcode,
			string countryCode,
			string contact,
			string email,
			string phone,
			string mobile,
			string fax)
		{
			CombineAssertions(() =>
			{
				if (addressPk != null)
				{
					AssertEquals("Address PK should be set", addressPk.Value, consignment.HVC_OA_ReturnLocation);
				}
				else
				{
					AssertEquals("Address PK should be null", true, consignment.HVC_OA_ReturnLocation.IsEmpty);
				}

				AssertEquals("Name", name, consignment.HVC_ReturnName);
				AssertEquals("Address1", address1, consignment.HVC_ReturnAddress1);
				AssertEquals("Address2", address2, consignment.HVC_ReturnAddress2);
				AssertEquals("City", city, consignment.HVC_ReturnCity);
				AssertEquals("State", state, consignment.HVC_ReturnState);
				AssertEquals("Postcode", postcode, consignment.HVC_ReturnPostcode);
				AssertEquals("CountryCode", countryCode, consignment.HVC_RN_NKReturnCountryCode);

				AssertEquals("Contact", contact, consignment.HVC_ReturnContact);
				AssertEquals("Email", email, consignment.HVC_ReturnEmail);
				AssertEquals("Phone", phone, consignment.HVC_ReturnPhone);
				AssertEquals("Mobile", mobile, consignment.HVC_ReturnMobile);
				AssertEquals("Fax", fax, consignment.HVC_ReturnFax);
			});
		}

		#endregion

		#region PopulateConsignee

		public void TestPopulateConsignee_WhenOverrideIsNull_MatchingAddressAndContactExist_ShouldUseMatchedData()
		{
			TestPopulateConsignee_WhenMatchingAddressAndContactExist_ShouldUseMatchedData(null);
		}

		public void TestPopulateConsignee_WhenOverrideIsFalse_MatchingAddressAndContactExist_ShouldUseMatchedData()
		{
			TestPopulateConsignee_WhenMatchingAddressAndContactExist_ShouldUseMatchedData(false);
		}

		void TestPopulateConsignee_WhenMatchingAddressAndContactExist_ShouldUseMatchedData(ZBool? addressOverride)
		{
			var matchingOrgCode = "ORG001";
			var matchingContact = "Sales";
			var consignee = CreateOrgHeader(matchingOrgCode, matchingContact, companyName, address1, address2, city, state, postcode, countryCode, email, phone, mobile, fax);
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				CreateOrganizationAddress(DocAddressType.ConsigneeDocumentaryAddress, addressOverride, matchingOrgCode, matchingContact)
			});

			var consignment = new HVLVConsignmentDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			AssertConsigneeAddress(consignment, consignee.MainAddress.PK, companyName, address1, address2, city, state, postcode, countryCode, matchingContact, email, phone, mobile, fax);
		}

		public void TestPopulateConsignee_WhenOverrideIsNull_NoMatchingContactExists_ShouldUseXUSContact()
		{
			TestPopulateConsignee_WhenNoMatchingContactExists_ShouldUseXUSContact(null);
		}

		public void TestPopulateConsignee_WhenOverrideIsFalse_NoMatchingContactExists_ShouldUseXUSContact()
		{
			TestPopulateConsignee_WhenNoMatchingContactExists_ShouldUseXUSContact(null);
		}

		void TestPopulateConsignee_WhenNoMatchingContactExists_ShouldUseXUSContact(ZBool? addressOverride)
		{
			var matchingOrgCode = "ORG001";
			var consignee = CreateOrgHeader(matchingOrgCode, "Unmatched Contact", companyName, address1, address2, city, state, postcode, countryCode);
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				CreateOrganizationAddress(DocAddressType.ConsigneeDocumentaryAddress, addressOverride, matchingOrgCode, contactName, "", "", "", "", "", "", "", email, phone, mobile, fax)
			});

			var consignment = new HVLVConsignmentDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			AssertConsigneeAddress(consignment, consignee.MainAddress.PK, companyName, address1, address2, city, state, postcode, countryCode, contactName, email, phone, mobile, fax);
		}

		public void TestPopulateConsignee_WhenOverrideIsNull_NoMatchingAddressExists_ShouldUseXUSData()
		{
			TestPopulateConsignee_WhenNoMatchingAddressExists_ShouldUseXUSData(null);
		}

		public void TestPopulateConsignee_WhenOverrideIsFalse_NoMatchingAddressExists_ShouldUseXUSData()
		{
			TestPopulateConsignee_WhenNoMatchingAddressExists_ShouldUseXUSData(null);
		}

		void TestPopulateConsignee_WhenNoMatchingAddressExists_ShouldUseXUSData(ZBool? addressOverride)
		{
			var consignee = CreateOrgHeader("UNMATCH");
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				CreateOrganizationAddress(DocAddressType.ConsigneeDocumentaryAddress, addressOverride, orgCode, contactName, companyName, address1, address2, city, state, postcode, countryCode, email, phone, mobile, fax)
			});

			var consignment = new HVLVConsignmentDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			AssertConsigneeAddress(consignment, null, companyName, address1, address2, city, state, postcode, countryCode, contactName, email, phone, mobile, fax);
		}

		public void TestPopulateConsignee_WhenOverrideIsTrue_ShouldUseXUSData()
		{
			var consignee = CreateOrgHeader(orgCode);
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				CreateOrganizationAddress(DocAddressType.ConsigneeDocumentaryAddress, true, orgCode, contactName, companyName, address1, address2, city, state, postcode, countryCode, email, phone, mobile, fax)
			});

			var consignment = new HVLVConsignmentDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			AssertConsigneeAddress(consignment, null, companyName, address1, address2, city, state, postcode, countryCode, contactName, email, phone, mobile, fax);
		}

		public void TestPopulateConsignee_WhenConsignmentExists_ShouldUpdateAddress()
		{
			var consignee = CreateOrgHeader(orgCode);
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				CreateOrganizationAddress(DocAddressType.ConsigneeDocumentaryAddress, true, orgCode, contactName, companyName, address1, address2, city, state, postcode, countryCode, email, phone, mobile, fax)
			});

			var existingConsignment = Factory.NewWithValidTestData<HVLVConsignment>();
			existingConsignment.HVC_OA_ConsigneeAddress = consignee.MainAddress.PK;

			var consignment = new HVLVConsignmentDataObjectReader(shipment, new DummyLogger(), Factory, existingConsignment).ReadIntoBusinessObject();
			AssertConsigneeAddress(consignment, null, companyName, address1, address2, city, state, postcode, countryCode, contactName, email, phone, mobile, fax);
		}

		void AssertConsigneeAddress(
			HVLVConsignment consignment,
			ZGuid? addressPk,
			string name,
			string address1,
			string address2,
			string city,
			string state,
			string postcode,
			string countryCode,
			string contact,
			string email,
			string phone,
			string mobile,
			string fax)
		{
			CombineAssertions(() =>
			{
				if (addressPk != null)
				{
					AssertEquals("Address PK should be set", addressPk.Value, consignment.HVC_OA_ConsigneeAddress);
				}
				else
				{
					AssertEquals("Address PK should be null", true, consignment.HVC_OA_ConsigneeAddress.IsEmpty);
				}

				AssertEquals("Name", name, consignment.HVC_ConsigneeName);
				AssertEquals("Address1", address1, consignment.HVC_ConsigneeAddress1);
				AssertEquals("Address2", address2, consignment.HVC_ConsigneeAddress2);
				AssertEquals("City", city, consignment.HVC_ConsigneeCity);
				AssertEquals("State", state, consignment.HVC_ConsigneeState);
				AssertEquals("Postcode", postcode, consignment.HVC_ConsigneePostcode);
				AssertEquals("CountryCode", countryCode, consignment.HVC_RN_NKConsigneeCountryCode);

				AssertEquals("Contact", contact, consignment.HVC_ConsigneeContact);
				AssertEquals("Email", email, consignment.HVC_ConsigneeEmail);
				AssertEquals("Phone", phone, consignment.HVC_ConsigneePhone);
				AssertEquals("Mobile", mobile, consignment.HVC_ConsigneeMobile);
				AssertEquals("Fax", fax, consignment.HVC_ConsigneeFax);
			});
		}

		#endregion

		#region PopulateShipper

		public void TestPopulateShipper_WhenOverrideIsNull_MatchingAddressAndContactExist_ShouldUseMatchedData()
		{
			TestPopulateShipper_WhenMatchingAddressAndContactExist_ShouldUseMatchedData(null);
		}

		public void TestPopulateShipper_WhenOverrideIsFalse_MatchingAddressAndContactExist_ShouldUseMatchedData()
		{
			TestPopulateShipper_WhenMatchingAddressAndContactExist_ShouldUseMatchedData(false);
		}

		void TestPopulateShipper_WhenMatchingAddressAndContactExist_ShouldUseMatchedData(ZBool? addressOverride)
		{
			var matchingOrgCode = "ORG001";
			var matchingContact = "Sales";
			var shipper = CreateOrgHeader(matchingOrgCode, matchingContact, companyName, address1, address2, city, state, postcode, countryCode, email, phone, mobile, fax);
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				CreateOrganizationAddress(DocAddressType.ConsignorDocumentaryAddress, addressOverride, matchingOrgCode, matchingContact)
			});

			var consignment = new HVLVConsignmentDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			AssertShipperAddress(consignment, shipper.MainAddress.PK, companyName, address1, address2, city, state, postcode, countryCode, matchingContact, email, phone, mobile, fax);
		}

		public void TestPopulateShipper_WhenOverrideIsNull_NoMatchingContactExists_ShouldUseXUSContact()
		{
			TestPopulateShipper_WhenNoMatchingContactExists_ShouldUseXUSContact(null);
		}

		public void TestPopulateShipper_WhenOverrideIsFalse_NoMatchingContactExists_ShouldUseXUSContact()
		{
			TestPopulateShipper_WhenNoMatchingContactExists_ShouldUseXUSContact(null);
		}

		void TestPopulateShipper_WhenNoMatchingContactExists_ShouldUseXUSContact(ZBool? addressOverride)
		{
			var matchingOrgCode = "ORG001";
			var shipper = CreateOrgHeader(matchingOrgCode, "Unmatched Contact", companyName, address1, address2, city, state, postcode, countryCode);
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				CreateOrganizationAddress(DocAddressType.ConsignorDocumentaryAddress, addressOverride, matchingOrgCode, contactName, "", "", "", "", "", "", "", email, phone, mobile, fax)
			});

			var consignment = new HVLVConsignmentDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			AssertShipperAddress(consignment, shipper.MainAddress.PK, companyName, address1, address2, city, state, postcode, countryCode, contactName, email, phone, mobile, fax);
		}

		public void TestPopulateShipper_WhenOverrideIsNull_NoMatchingAddressExists_ShouldUseXUSData()
		{
			TestPopulateShipper_WhenNoMatchingAddressExists_ShouldUseXUSData(null);
		}

		public void TestPopulateShipper_WhenOverrideIsFalse_NoMatchingAddressExists_ShouldUseXUSData()
		{
			TestPopulateShipper_WhenNoMatchingAddressExists_ShouldUseXUSData(null);
		}

		void TestPopulateShipper_WhenNoMatchingAddressExists_ShouldUseXUSData(ZBool? addressOverride)
		{
			var shipper = CreateOrgHeader("UNMATCH");
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				CreateOrganizationAddress(DocAddressType.ConsignorDocumentaryAddress, addressOverride, orgCode, contactName, companyName, address1, address2, city, state, postcode, countryCode, email, phone, mobile, fax)
			});

			var consignment = new HVLVConsignmentDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			AssertShipperAddress(consignment, null, companyName, address1, address2, city, state, postcode, countryCode, contactName, email, phone, mobile, fax);
		}

		public void TestPopulateShipper_WhenOverrideIsTrue_ShouldUseXUSData()
		{
			var shipper = CreateOrgHeader(orgCode);
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				CreateOrganizationAddress(DocAddressType.ConsignorDocumentaryAddress, true, orgCode, contactName, companyName, address1, address2, city, state, postcode, countryCode, email, phone, mobile, fax)
			});

			var consignment = new HVLVConsignmentDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			AssertShipperAddress(consignment, null, companyName, address1, address2, city, state, postcode, countryCode, contactName, email, phone, mobile, fax);
		}

		public void TestPopulateShipper_WhenConsignmentExists_ShouldUpdateAddress()
		{
			var shipper = CreateOrgHeader(orgCode);
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				CreateOrganizationAddress(DocAddressType.ConsignorDocumentaryAddress, true, orgCode, contactName, companyName, address1, address2, city, state, postcode, countryCode, email, phone, mobile, fax)
			});

			var existingConsignment = Factory.NewWithValidTestData<HVLVConsignment>();
			existingConsignment.HVC_OA_ShipperAddress = shipper.MainAddress.PK;

			var consignment = new HVLVConsignmentDataObjectReader(shipment, new DummyLogger(), Factory, existingConsignment).ReadIntoBusinessObject();
			AssertShipperAddress(consignment, null, companyName, address1, address2, city, state, postcode, countryCode, contactName, email, phone, mobile, fax);
		}

		void AssertShipperAddress(
			HVLVConsignment consignment,
			ZGuid? addressPk,
			string name,
			string address1,
			string address2,
			string city,
			string state,
			string postcode,
			string countryCode,
			string contact,
			string email,
			string phone,
			string mobile,
			string fax)
		{
			CombineAssertions(() =>
			{
				if (addressPk != null)
				{
					AssertEquals("Address PK should be set", addressPk.Value, consignment.HVC_OA_ShipperAddress);
				}
				else
				{
					AssertEquals("Address PK should be null", true, consignment.HVC_OA_ShipperAddress.IsEmpty);
				}

				AssertEquals("Name", name, consignment.HVC_ShipperName);
				AssertEquals("Address1", address1, consignment.HVC_ShipperAddress1);
				AssertEquals("Address2", address2, consignment.HVC_ShipperAddress2);
				AssertEquals("City", city, consignment.HVC_ShipperCity);
				AssertEquals("State", state, consignment.HVC_ShipperState);
				AssertEquals("Postcode", postcode, consignment.HVC_ShipperPostcode);
				AssertEquals("CountryCode", countryCode, consignment.HVC_RN_NKShipperCountryCode);

				AssertEquals("Contact", contact, consignment.HVC_ShipperContact);
				AssertEquals("Email", email, consignment.HVC_ShipperEmail);
				AssertEquals("Phone", phone, consignment.HVC_ShipperPhone);
				AssertEquals("Mobile", mobile, consignment.HVC_ShipperMobile);
				AssertEquals("Fax", fax, consignment.HVC_ShipperFax);
			});
		}

		#endregion

		OrgHeader CreateOrgHeader(
			string orgCode = "",
			string contactName = "",
			string companyName = "",
			string address1 = "",
			string address2 = "",
			string city = "",
			string state = "",
			string postcode = "",
			string countryCode = "",
			string email = "",
			string phone = "",
			string mobile = "",
			string fax = "")
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = orgCode;

			var address = consignee.MainAddress;
			address.CompanyName = companyName;
			address.Address1 = address1;
			address.Address2 = address2;
			address.City = city;
			address.State = state;
			address.Postcode = postcode;
			address.OA_RN_NKCountryCode = countryCode;

			var contact = consignee.Contacts.AddNew();
			contact.OC_ContactName = contactName;
			contact.OC_Email = email;
			contact.OC_Phone = phone;
			contact.OC_Mobile = mobile;
			contact.OC_Fax = fax;

			Factory.SaveForTesting();

			return consignee;
		}

		OrganizationAddress CreateOrganizationAddress(
			DocAddressType docAddressType,
			ZBool? overrideAddress = null,
			string orgCode = "",
			string contact = "",
			string companyName = "",
			string address1 = "",
			string address2 = "",
			string city = "",
			string state = "",
			string postcode = "",
			string countryCode = "",
			string email = "",
			string phone = "",
			string mobile = "",
			string fax = "")
		{
			return new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = docAddressType.ToString(),
				AddressOverride = overrideAddress,
				OrganizationCode = orgCode,
				CompanyName = companyName,
				Address1 = address1,
				Address2 = address2,
				City = city,
				State = state,
				Postcode = postcode,
				Country = new Country { Code = countryCode },
				Contact = contact,
				Email = email,
				Phone = phone,
				Mobile = mobile,
				Fax = fax
			};
		}

		const string orgCode = "TEST01";
		const string companyName = "Test Company Name";
		const string address1 = "99 Consignee Road";
		const string address2 = "Downtown";
		const string city = "New York";
		const string state = "NY";
		const string postcode = "12345";
		const string countryCode = "US";
		const string contactName = "Moo ray";
		const string email = "murray.hewitt@usconsulate.gov.nz";
		const string phone = "+1111111111";
		const string mobile = "+1234567890";
		const string fax = "+0987654321";
	}
}
