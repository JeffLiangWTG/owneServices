using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.Customs.US.LVS.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.US.DataTransfer.Universal.Constants;

namespace Enterprise.Customs.US.LVS.DataTransfer.Test
{
	public class USLVConsignmentDataReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestStringValueCharacterCaseIsUpper()
		{
			var characterCasingProperty = typeof(USLVConsignmentDataReader).GetProperty(
				"StringValueCharacterCase",
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty);
			var reader = new USLVConsignmentDataReader(new Shipment(DefaultDataObjectWriterStrategy.TestInstance), new DummyLogger(), Factory, null, null);
			var characterCasingValue = characterCasingProperty.GetValue(reader);

			AssertEquals(ZArchitecture.Environment.CharacterCase.Upper, characterCasingValue);
		}

		public void TestULB_NumberOfPacksFallback()
		{
			var shipment = SetupShipment(out var _);
			shipment.TotalNoOfPieces = 16;
			shipment.TotalNoOfPacks = 22;
			var consignment = new USLVConsignmentDataReader(shipment, new DummyLogger(), Factory, null, null).ReadIntoBusinessObject();
			AssertEquals((short)22, consignment.ULB_NumberOfPacks);

			shipment.TotalNoOfPacks = null;
			consignment = new USLVConsignmentDataReader(shipment, new DummyLogger(), Factory, null, null).ReadIntoBusinessObject();
			AssertEquals((short)16, consignment.ULB_NumberOfPacks);
		}

		public void TestNoAddressMatchingWhenAddressIsOverriden()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "ORG$$1";
			var consigneeAddress = consignee.MainAddress;
			consigneeAddress.Address1 = "overhere";
			var consignmentDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var addressDataObject = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				OrganizationCode = "ORG$$1",
				AddressType = Constants.AddressTypes.UltimateConsignee,
				Address1 = "somewhere"
			};
			consignmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>()
			{
				addressDataObject
			});

			Assert(!addressDataObject.AddressOverride.GetValueOrDefault());
			var consignment = new USLVConsignmentDataReader(consignmentDataObject, new DummyLogger(), Factory, null, null).ReadIntoBusinessObject();
			AssertNotNull(consignment.Consignee);
			AssertEquals(consigneeAddress.PK, consignment.Consignee.PK);
			AssertEquals("overhere", consignment.Consignee.Address1);

			addressDataObject.AddressOverride = true;
			consignment = new USLVConsignmentDataReader(consignmentDataObject, new DummyLogger(), Factory, null, null).ReadIntoBusinessObject();
			AssertNull(consignment.Consignee);
			AssertEquals("SOMEWHERE", consignment.ULB_ConsigneeAddress1);
		}

		public void TestReadIntoBusinessObject()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var shipment = SetupShipment(out var pk);
			var consignment = new USLVConsignmentDataReader(shipment, new DummyLogger(), Factory, null, null).ReadIntoBusinessObject();
			clearance.CusUSLVConsignments.Add(consignment);
			Factory.SaveForTesting();
			Assert(consignment.IsInDatabase);
			AssertConsignments(consignment, pk);
		}

		#region Implementation

		void AssertConsignments(CusUSLVConsignment cusUSLVConsignment, ZGuid pk)
		{
			AssertConsignment(cusUSLVConsignment, "KG", pk);
			AssertCusUSLVItems(cusUSLVConsignment.CusUSLVItems.Cast<CusUSLVItem>().ToList());
		}

		void AssertConsignment(CusUSLVConsignment cusUSLVConsignment, string packType, ZGuid pk)
		{
			CombineAssertions(() =>
			{
				AssertEquals("OR0000", cusUSLVConsignment.ULB_OwnerReferenceNumber);
				AssertEquals("WBN0000", cusUSLVConsignment.ULB_HouseBill);
				AssertEquals("CN001", cusUSLVConsignment.ULB_EquipmentNumber);
				AssertEquals(3, cusUSLVConsignment.ULB_NumberOfPacks.ToZInt());
				AssertEquals(packType, cusUSLVConsignment.ULB_PackType);
				AssertNotEquals("Entry number is unique and calculated by system", "MC123", cusUSLVConsignment.CE_EntryNum);

				AssertEquals("123A", cusUSLVConsignment.ULB_HouseBillIssuerSCAC);
				AssertEquals(true, cusUSLVConsignment.ULB_NonAMSIndicator);
				AssertEquals(ZGuid.Empty, cusUSLVConsignment.ULB_OA_Consignee);
				AssertEquals("BOB", cusUSLVConsignment.ULB_ConsigneeQualifier);
				AssertEquals("DYLAN", cusUSLVConsignment.ULB_ConsigneeIdentifier);

				AssertEquals(pk, cusUSLVConsignment.ULB_OA_Seller);
				AssertEquals("C NAME", cusUSLVConsignment.ULB_ConsigneeName);
				AssertEquals("C ADDRESS 1", cusUSLVConsignment.ULB_ConsigneeAddress1);
				AssertEquals("C ADDRESS 2", cusUSLVConsignment.ULB_ConsigneeAddress2);
				AssertEquals("CITY 1", cusUSLVConsignment.ULB_ConsigneeCity);
				AssertEquals("A0001", cusUSLVConsignment.ULB_ConsigneePostCode);
				AssertEquals("CHI", cusUSLVConsignment.ULB_ConsigneeState);
				AssertEquals("US", cusUSLVConsignment.ULB_RN_NKConsigneeCountry);
			});
		}

		void AssertCusUSLVItems(List<CusUSLVItem> cusUSLVItems)
		{
			CombineAssertions(() =>
			{
				AssertEquals(3, cusUSLVItems.Count);
				AssertCusUSLVItem(cusUSLVItems.First(u => u.ULI_Tariff == "01"), 0);
				AssertCusUSLVItem(cusUSLVItems.First(u => u.ULI_Tariff == "02"), 1);
				AssertCusUSLVItem(cusUSLVItems.First(u => u.ULI_Tariff == "03"), 2);
			});
		}

		void AssertCusUSLVItem(CusUSLVItem cusUSLVItem, int index)
		{
			if (index == 0)
			{
				AssertEquals("AAA", cusUSLVItem.ULI_GoodsDescription);
				AssertEquals(10m, cusUSLVItem.ULI_GoodsValue);
				AssertEquals("USD", cusUSLVItem.ULI_RX_NKCurrency);
				AssertEquals("US", cusUSLVItem.ULI_RN_NKCountryOfOrigin);
				AssertEquals("AA", cusUSLVItem.ULI_PartNo);
			}
			else if (index == 1)
			{
				AssertEquals("BBB", cusUSLVItem.ULI_GoodsDescription);
				AssertEquals(20m, cusUSLVItem.ULI_GoodsValue);
				AssertEquals("AUD", cusUSLVItem.ULI_RX_NKCurrency);
				AssertEquals("AU", cusUSLVItem.ULI_RN_NKCountryOfOrigin);
				AssertEquals("BB", cusUSLVItem.ULI_PartNo);
			}
			else if (index == 2)
			{
				AssertEquals("CCC", cusUSLVItem.ULI_GoodsDescription);
				AssertEquals(30m, cusUSLVItem.ULI_GoodsValue);
				AssertEquals("AUD", cusUSLVItem.ULI_RX_NKCurrency);
				AssertEquals("US", cusUSLVItem.ULI_RN_NKCountryOfOrigin);
				AssertEquals("CC", cusUSLVItem.ULI_PartNo);
			}

			AssertEquals(index == 0, cusUSLVItem.ULI_AntiDumping);
			AssertEquals(index == 0, cusUSLVItem.ULI_Countervailing);
			AssertEquals(index == 0 ? 2 : 0, cusUSLVItem.CusUSLVItemPGAs.Count);

			if (index == 0)
			{
				var oga1 = cusUSLVItem.CusUSLVItemPGAs.Cast<CusUSLVItemPGA>().First(u => u.ULP_DisclaimReason == "A");
				var oga2 = cusUSLVItem.CusUSLVItemPGAs.Cast<CusUSLVItemPGA>().First(u => u.ULP_DisclaimReason == "B");
				AssertEquals("NHT", oga1.ULP_Agency);
				AssertEquals("TTB", oga2.ULP_Agency);
				AssertEquals("OFF", oga1.ULP_AgencyProgram);
				AssertEquals("TOB", oga2.ULP_AgencyProgram);
			}
		}

		#region Setup

		Shipment SetupShipment(out ZGuid pk)
		{
			var seller = Factory.NewWithValidTestData<OrgHeader>();
			seller.MainAddress.Address1 = "89 Bill To Street";
			seller.Contacts.AddNew().OC_ContactName = "Bill 3";
			pk = seller.MainAddress.PK;

			Factory.SaveForTesting();

			#region Setup Basic Info

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.OwnerRef = "OR0000";
			shipment.WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.House, Description = WayBillTypeList.Descriptions.House };
			shipment.WayBillNumber = "WBN0000";
			shipment.TotalNoOfPacks = 3;
			shipment.TotalNoOfPacksPackageType = new PackageType() { Code = "KG" };

			#endregion

			#region Setup Commercial Info

			shipment.CommercialInfo = GetCommercialInfo();

			#endregion

			#region Setup Address Collection

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());

			shipment.OrganizationAddressCollection.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = Constants.AddressTypes.UltimateConsignee,
				CompanyName = "C Name",
				Address1 = "C Address 1",
				Address2 = "C Address 2",
				City = "City 1",
				Postcode = "A0001",
				State = "CHI",
				Country = Country.New(Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.UnitedStates))
			});

			shipment.OrganizationAddressCollection.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = AddressType.Seller,
				OrganizationCode = seller.OH_Code,
				Contact = seller.Contacts[0].OC_ContactName
			});

			#endregion

			#region Setup Container Collection

			shipment.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new Container
				{
					ContainerNumber = "CN001"
				}
			});

			#endregion

			#region Setup Add Info Collection

			shipment.SetAddInfoCollection(() => new List<AddInfo>
			{
				new AddInfo
				{
					Key = LVSConstants.AddInfoConstants.WayBillIssuerSCAC,
					Value = "123A"
				},
				new AddInfo
				{
					Key = LVSConstants.AddInfoConstants.NonAMS,
					Value = YesNoList.Codes.Yes
				},
				new AddInfo
				{
					Key = LVSConstants.AddInfoConstants.ConsigneeType,
					Value = "Bob"
				},
				new AddInfo
				{
					Key = LVSConstants.AddInfoConstants.ConsigneeReference,
					Value = "Dylan"
				}
			});

			#endregion

			#region Setup Entry Number Collection

			shipment.SetEntryNumberCollection(() => new List<UniversalDataBuss.DataObjects.Universal.EntryNumber>
			{
				new UniversalDataBuss.DataObjects.Universal.EntryNumber
				{
					Number = "MC123"
				}
			});

			#endregion

			return shipment;
		}

		CommercialInfo GetCommercialInfo()
		{
			var commercialInfo = new CommercialInfo();
			var commercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>();
			var invoiceHeader1 = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{
				InvoiceCurrency = new Currency
				{
					Code = "USD"
				}
			};

			var invoiceHeader2 = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{
				InvoiceCurrency = new Currency
				{
					Code = "AUD"
				}
			};

			commercialInvoiceCollection.Add(invoiceHeader1);
			commercialInvoiceCollection.Add(invoiceHeader2);

			invoiceHeader1.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>()
			{
				new CommercialInvoiceLine()
				{
					CountryOfOrigin = new Country { Code = "US" },
					Description = "AAA",
					HarmonisedCode = "TIF01",
					LinePrice = 10,
					LineNo = 1,
					AddInfoCollection = GetInvoiceLineAddInfo(0),
					PartNo = "AA"
				}
			});

			invoiceHeader2.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>()
			{
				new CommercialInvoiceLine
				{
					CountryOfOrigin = new Country { Code = "AU" },
					Description = "BBB",
					HarmonisedCode = "TIF02",
					LinePrice = 20,
					LineNo = 2,
					AddInfoCollection = GetInvoiceLineAddInfo(1),
					PartNo = "BB"
				},
				new CommercialInvoiceLine
				{
					CountryOfOrigin = new Country { Code = "US" },
					Description = "CCC",
					HarmonisedCode = "TIF03",
					LinePrice = 30,
					LineNo = 3,
					AddInfoCollection = GetInvoiceLineAddInfo(1),
					PartNo = "CC"
				}
			});

			commercialInfo.CommercialInvoiceCollection = commercialInvoiceCollection;
			return commercialInfo;
		}

		List<AddInfo> GetInvoiceLineAddInfo(int index)
		{
			var listAddInfo = new List<AddInfo>
			{
				new AddInfo
				{
					Key = LVSConstants.AddInfoConstants.ADD_NA,
					Value = index == 0 ? YesNoList.Codes.Yes : YesNoList.Codes.No,
				},
				new AddInfo
				{
					Key = LVSConstants.AddInfoConstants.CVD_NA,
					Value = index == 0 ? YesNoList.Codes.Yes : YesNoList.Codes.No,
				}
			};

			if (index == 0)
			{
				var agencyAnalysis = new List<AddInfo>
				{
					new AddInfo
					{
						Key = LVSConstants.AddInfoConstants.NHTDisclaimReason,
						Value = "A"
					},
					new AddInfo
					{
						Key = LVSConstants.AddInfoConstants.TTBDisclaimReason,
						Value = "B"
					}
				};

				listAddInfo.AddRange(agencyAnalysis);
			}

			return listAddInfo;
		}

		#endregion

		#endregion
	}
}
