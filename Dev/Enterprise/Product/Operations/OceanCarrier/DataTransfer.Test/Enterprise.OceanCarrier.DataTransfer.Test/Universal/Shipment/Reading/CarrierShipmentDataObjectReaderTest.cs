using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.OceanCarrier.Business;
using Enterprise.OceanCarrier.DataTransfer.Universal.Shipment;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using Currency = Enterprise.UniversalDataBuss.DataObjects.Universal.Currency;
using UniversalContainer = Enterprise.UniversalDataBuss.DataObjects.Universal.Container;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.OceanCarrier.DataTransfer.Universal.Testing
{
	sealed class CarrierShipmentDataObjectReaderTest : OrganizationAddressTestHelper
	{
		public void TestDataContextType()
		{
			var reader = new CarrierShipmentDataObjectReader(new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance), Logger, Factory);
			AssertEquals(DataContextType.CarrierShipment, reader.DataContextType);
		}

		public void TestPopulateBusinessObject_CarrierShipmentIsProcessedAsUpdate()
		{
			CreateCarrierShipment();
			CreateOrganizationAddress();
			Factory.SaveForTesting();

			var dataObject = SetUniversalShipment("CSH001");
			dataObject.DataContext = SetDataContextDataObject("CSH001");
			dataObject = SetOrganizationAddressCollection(dataObject);
			var container1 = SetContainer("CNT003", "Container 1", 1);
			dataObject.SetContainerCollection(() => new DataObjectList<UniversalContainer>());
			dataObject.ContainerCollection.Add(container1);

			var packingLineBreakBulk = SetPackingLine("CNT003", "Packing Line 1", "REF001", 1);
			dataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>());
			dataObject.PackingLineCollection.Add(packingLineBreakBulk);

			var reader = CreateNewReader(dataObject);
			var shipmentFromReader = reader.ReadIntoBusinessObject();

			AssertEquals(expected: false, reader.IsInsert);
			AssertCarrierShipmentHeader(dataObject, shipmentFromReader);
			AssertParties(dataObject, shipmentFromReader);
			AssertContainers(dataObject, shipmentFromReader);
			AssertPackingLines(dataObject, shipmentFromReader, isTopLevel: ZBool.False);
		}

		public void TestPopulateBusinessObject_CarrierShipmentIsProcessedAsInsert()
		{
			var customisation = new BillOfLadingNumberCustomisation
			{
				Categories = NumberCustomisationElementCategories.Standard | NumberCustomisationElementCategories.LinerAgency
			};
			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.BranchCode].Include = true;
			OceanCarrierDataRegistry.Instance.OceanCarrierShipmentReferenceNumberFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, customisation);

			var dataObject = SetUniversalShipment(ZString.Empty);
			dataObject.DataContext = SetDataContextDataObject(ZString.Empty);
			dataObject = SetOrganizationAddressCollection(dataObject);

			var container1 = SetContainer("CNT001", "Container 1", 1);
			var container2 = SetContainer("CNT002", "Container 2", 2);
			dataObject.SetContainerCollection(() => new DataObjectList<UniversalContainer>());
			dataObject.ContainerCollection.Add(container1);
			dataObject.ContainerCollection.Add(container2);

			var packingLineBreakBulk = SetPackingLine("CNT001", "Packing Line 1", "REF001", 1);
			var packingLineBreakBulk2 = SetPackingLine("CNT002", "Packing Line 2", "REF002", 2);
			dataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>());
			dataObject.PackingLineCollection.Add(packingLineBreakBulk);
			dataObject.PackingLineCollection.Add(packingLineBreakBulk2);

			var reader = CreateNewReader(dataObject);
			var shipmentFromReader = reader.ReadIntoBusinessObject();

			AssertEquals(expected: true, reader.IsInsert);
			AssertCarrierShipmentHeader(dataObject, shipmentFromReader);
			AssertParties(dataObject, shipmentFromReader);
			AssertContainers(dataObject, shipmentFromReader);
			AssertPackingLines(dataObject, shipmentFromReader, isTopLevel: ZBool.False);
		}

		public void TestGetExistingBusinessObject()
		{
			var shipment = CreateCarrierShipment();
			Factory.SaveForTesting();

			var dataObject = SetUniversalShipment("CSH001");
			dataObject.DataContext = SetDataContextDataObject("CSH001");
			var reader = CreateNewReader(dataObject);
			AssertEquals(shipment, ((ITopLevelDataObjectReader)reader).GetExistingBusinessObject());

			dataObject = SetUniversalShipment("CSH001");
			dataObject.DataContext = SetDataContextDataObject("");
			reader = CreateNewReader(dataObject);
			AssertEquals(null, ((ITopLevelDataObjectReader)reader).GetExistingBusinessObject());

			dataObject = SetUniversalShipment("CSH002");
			dataObject.DataContext = SetDataContextDataObject("CSH002");
			reader = CreateNewReader(dataObject);
			AssertExceptionThrown<DataObjectReadFailureException>("Match couldn't be found for CarrierShipment with Key CSH002", () => reader.ReadIntoBusinessObject());
		}

		public void TestPopulateParties_WithOrgAddress()
		{
			var orgHeader1 = SetOrgHeader("TST01", "Booking Party", isConsignee: false, isConsignor: false);
			var expectedOrgAddress1 = SetOrgAddress(orgHeader1, "TST01", "Booking Party", "Booking Party Address 1", "Booking Party Address 2");
			var orgHeader2 = SetOrgHeader("TST02", "Notify Party", isConsignee: false, isConsignor: false);
			SetOrgAddress(orgHeader2, "TST02", "Notify Party", "Notify Party Address 1", "Notify Party Address 2");
			Factory.SaveForTesting();

			var dataObject = SetUniversalShipment("CSH001");
			dataObject.DataContext = SetDataContextDataObject("CSH001");

			var bookingParty = SetOrganizationAddress("TST01", "Booking Party", nameof(DocAddressType.BookingPartyDocumentaryAddress), "Booking Party Address 1", "Booking Party Address 2", addressOverride: false);
			var notifyParty = SetOrganizationAddress("TST02", "Notify Party", nameof(DocAddressType.NotifyParty), "Notify Party Address 1", "Notify Party Address 2", addressOverride: true);
			dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			dataObject.OrganizationAddressCollection.Add(bookingParty);
			dataObject.OrganizationAddressCollection.Add(notifyParty);

			var reader = CreateNewReader(dataObject);
			var shipment = CreateCarrierShipment();
			var shipmentFromReader = reader.PopulateParties(shipment);

			var actualBookingPartyNotOverride =
				shipmentFromReader.DocAddresses.FindByDocAddressType(DocAddressType.BookingPartyDocumentaryAddress);
			var actualNotifyPartyOverride =
				shipmentFromReader.DocAddresses.FindByDocAddressType(DocAddressType.NotifyParty);
			var expectedOrganizationAddress =
				dataObject.OrganizationAddressCollection.FirstOrDefault(x =>
					x.AddressType?.ToString() == nameof(DocAddressType.NotifyParty));

			CombineAssertions(() =>
			{
				AssertEquals(actualBookingPartyNotOverride.E2_OA_Address, expectedOrgAddress1.PK);
				if (expectedOrganizationAddress == null)
				{
					return;
				}
				AssertEquals(expected: true, actualNotifyPartyOverride.E2_AddressOverride);
				AssertEquals(expectedOrganizationAddress.Address1, actualNotifyPartyOverride.E2_Address1);
				AssertEquals(expectedOrganizationAddress.City, actualNotifyPartyOverride.E2_City);
				AssertEquals(expectedOrganizationAddress.CompanyName, actualNotifyPartyOverride.E2_CompanyName);
				AssertEquals(expectedOrganizationAddress.Contact, actualNotifyPartyOverride.E2_Contact);
				AssertEquals(expectedOrganizationAddress.Country.Code, actualNotifyPartyOverride.E2_RN_NKCountryCode);
				AssertEquals(expectedOrganizationAddress.Email, actualNotifyPartyOverride.E2_Email);
				AssertEquals(expectedOrganizationAddress.Phone, actualNotifyPartyOverride.E2_Phone);
				AssertEquals(expectedOrganizationAddress.Postcode, actualNotifyPartyOverride.E2_Postcode);
				AssertEquals(expectedOrganizationAddress.State.Code, actualNotifyPartyOverride.E2_State);
			});
		}

		public void TestPopulateParties_AsFreeTextAddress()
		{
			var dataObject = SetUniversalShipment("CSH001");
			dataObject.DataContext = SetDataContextDataObject("CSH001");

			var bookingParty = SetOrganizationAddress("", "Free Text: Booking Party", nameof(DocAddressType.BookingPartyDocumentaryAddress), "Booking Party Address 1", "Booking Party Address 2", addressOverride: true);
			var notifyParty = SetOrganizationAddress("", "Free Text: Notify Party", nameof(DocAddressType.NotifyParty), "Notify Party Address 1", "Notify Party Address 2", addressOverride: false);
			dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			dataObject.OrganizationAddressCollection.Add(bookingParty);
			dataObject.OrganizationAddressCollection.Add(notifyParty);

			var reader = CreateNewReader(dataObject);
			var shipment = CreateCarrierShipment();
			var shipmentFromReader = reader.PopulateParties(shipment);

			AssertParties(dataObject, shipmentFromReader, isAddressFreeText: true);
		}

		public void TestGetReasonForNotAbleToUpdateFromDataSourceOrTargetBO()
		{
			var shipmentHeader = CreateCarrierShipment();
			Factory.SaveForTesting();

			var dataObject = SetUniversalShipment("CSH001");
			dataObject.DataContext = SetDataContextDataObject("CSH001");
			dataObject.GoodsValueCurrency = new Currency { Code = "" };

			AssertLogMessage(dataObject, $"The XML document includes the Goods Value:{dataObject.GoodsValue}. However, the Goods Value Currency has not been specified.", LogType.Error);
			AssertEquals(expected: true, Logger.HasErrors);

			dataObject.GoodsValueCurrency = new Currency { Code = "AUD" };
			var container1 = SetContainer("", "Container 1", 0);
			dataObject.SetContainerCollection(() => new DataObjectList<UniversalContainer>());
			dataObject.ContainerCollection.Add(container1);
			AssertLogMessage(dataObject, "Container link must be valid value in CSH001.", LogType.Warning);

			container1.Link = 1;
			var container2 = SetContainer("", "Container 2", 1);
			dataObject.ContainerCollection.Add(container2);
			AssertLogMessage(dataObject, "Duplicated container link 1.", LogType.Warning);

			container1.Link = 2;
			AssertLogMessage(dataObject, "", LogType.Warning);
		}

		public void TestPopulateContainers()
		{
			CreateCarrierShipment();
			CreateOrganizationAddress();
			Factory.SaveForTesting();

			var dataObject = SetUniversalShipment("CSH001");
			dataObject.DataContext = SetDataContextDataObject("CSH001");
			var container1 = SetContainer("CNT001", "Container 1", 1);
			var container2 = SetContainer("CNT002", "Container 2", 2);
			dataObject.SetContainerCollection(() => new DataObjectList<UniversalContainer>());
			dataObject.ContainerCollection.Add(container1);
			dataObject.ContainerCollection.Add(container2);

			var reader = CreateNewReader(dataObject);
			var shipmentFromReader = reader.ReadIntoBusinessObject();

			AssertContainers(dataObject, shipmentFromReader);
		}

		public void TestPopulateContainerLink()
		{
			CreateCarrierShipment();
			Factory.SaveForTesting();

			var dataObject = SetUniversalShipment("CSH001");
			dataObject.DataContext = SetDataContextDataObject("CSH001");
			var container1 = SetContainer("CNT001", "Container 1", 1);
			dataObject.SetContainerCollection(() => new DataObjectList<UniversalContainer>());
			dataObject.ContainerCollection.Add(container1);

			var packingLineBreakBulk = SetPackingLine("CNT001", "Packing Line 1", "REF001", 1);
			dataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>());
			dataObject.PackingLineCollection.Add(packingLineBreakBulk);

			var containerLinkManager = new CarrierShipmentContainerLinkManager();
			var reader = CreateNewReader(dataObject);
			var shipmentFromReader = reader.ReadIntoBusinessObject();

			containerLinkManager.CollectContainerLink(shipmentFromReader.Cargoes[1], dataObject.ContainerCollection[0]);
			AssertEquals(shipmentFromReader.Cargoes[1].PK, containerLinkManager.GetContainer(1).PK);
		}

		public void TestPopulateBreakBulk()
		{
			CreateCarrierShipment();
			Factory.SaveForTesting();

			var dataObject = SetUniversalShipment("CSH001");
			dataObject.DataContext = SetDataContextDataObject("CSH001");

			var packingLineBreakBulk = SetPackingLine("", "Packing Line 1", "REF001", null);
			dataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>());
			dataObject.PackingLineCollection.Add(packingLineBreakBulk);

			var reader = CreateNewReader(dataObject);
			var shipmentFromReader = reader.ReadIntoBusinessObject();

			AssertPackingLines(dataObject, shipmentFromReader);
		}

		public void TestPopulateNestedCargo_ContainerAsParent_BreakBulkAsChild()
		{
			CreateCarrierShipment();
			Factory.SaveForTesting();

			var dataObject = SetUniversalShipment("CSH001");
			dataObject.DataContext = SetDataContextDataObject("CSH001");

			var container1 = SetContainer("CNT001", "Container 1", 1);
			dataObject.SetContainerCollection(() => new DataObjectList<UniversalContainer>());
			dataObject.ContainerCollection.Add(container1);

			var packingLineBreakBulk = SetPackingLine("CNT001", "Packing Line 1", "REF001", 1);
			dataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>());
			dataObject.PackingLineCollection.Add(packingLineBreakBulk);

			var reader = CreateNewReader(dataObject);
			var shipmentFromReader = reader.ReadIntoBusinessObject();
			var containerLinkManager = new CarrierShipmentContainerLinkManager();
			AssertContainers(dataObject, shipmentFromReader);
			AssertPackingLines(dataObject, shipmentFromReader, isTopLevel: ZBool.False);
			containerLinkManager.CollectContainerLink(shipmentFromReader.Cargoes[1], dataObject.ContainerCollection[0]);
			AssertContainerLink(containerLinkManager.GetContainer(1).PK, shipmentFromReader.Cargoes[1].PK, shipmentFromReader.Cargoes[2].PK);
		}

		void AssertCarrierShipmentHeader(UniversalShipment expectedValueFromXml, CarrierShipmentHeader actualValueFromReader)
		{
			AssertEquals(expectedValueFromXml.BookingConfirmationReference, actualValueFromReader.CSH_CarrierShipmentReference);
			AssertEquals(expectedValueFromXml.GoodsValue, actualValueFromReader.CSH_DeclaredValue);
			AssertEquals(expectedValueFromXml.GoodsValueCurrency.Code, actualValueFromReader.CSH_RX_NKDeclaredValueCurrency);
			AssertEquals(expectedValueFromXml.PlaceOfDelivery.Code, actualValueFromReader.CSH_RL_NKPlaceOfDeliveryCode);
			AssertEquals(expectedValueFromXml.PlaceOfReceipt.Code, actualValueFromReader.CSH_RL_NKPlaceOfReceiptCode);
			AssertEquals(expectedValueFromXml.PortOfDischarge.Code, actualValueFromReader.CSH_RL_NKPortOfDischargeCode);
			AssertEquals(expectedValueFromXml.PortOfLoading.Code, actualValueFromReader.CSH_RL_NKPortOfLoadingCode);
			AssertEquals(expectedValueFromXml.PortOfOrigin.Code, actualValueFromReader.CSH_RL_NKPortOfOriginCode);
			AssertEquals(expectedValueFromXml.PortOfDestination.Code, actualValueFromReader.CSH_RL_NKPortOfDestinationCode);
			AssertEquals(expectedValueFromXml.WayBillNumber, actualValueFromReader.CSH_RequestedTransportDocumentReference);
			AssertEquals(expectedValueFromXml.WayBillType.Code, actualValueFromReader.CSH_RequestedTransportDocumentType);
		}

		void AssertParties(UniversalShipment expectedValueFromXml, CarrierShipmentHeader actualValueFromReader, bool isAddressFreeText = false)
		{
			foreach (var expectedOrganizationAddress in expectedValueFromXml.OrganizationAddressCollection)
			{
				var expectedAddressType = GetCodeFromDocAddressType(expectedOrganizationAddress.AddressType ?? ZString.Empty);
				var actualOrganizationAddress =
					actualValueFromReader.DocAddresses.FindByDocAddressType(GetDocAddressTypeFromCode(expectedAddressType));
				var expectedAddressOverride = isAddressFreeText ? true : expectedOrganizationAddress.AddressOverride;

				if (actualOrganizationAddress != null)
				{
					AssertEquals(expectedAddressType, actualOrganizationAddress.E2_AddressType);
					AssertEquals(expectedAddressOverride, actualOrganizationAddress.E2_AddressOverride);
					AssertEquals(expectedOrganizationAddress.Address1, actualOrganizationAddress.E2_Address1);
					AssertEquals(expectedOrganizationAddress.Address2, actualOrganizationAddress.E2_Address2);
					AssertEquals(expectedOrganizationAddress.City, actualOrganizationAddress.E2_City);
					AssertEquals(expectedOrganizationAddress.CompanyName, actualOrganizationAddress.E2_CompanyName);
					AssertEquals(expectedOrganizationAddress.Contact, actualOrganizationAddress.E2_Contact);
					AssertEquals(expectedOrganizationAddress.Country.Code, actualOrganizationAddress.E2_RN_NKCountryCode);
					AssertEquals(expectedOrganizationAddress.Email, actualOrganizationAddress.E2_Email);
					AssertEquals(expectedOrganizationAddress.Phone, actualOrganizationAddress.E2_Phone);
					AssertEquals(expectedOrganizationAddress.Postcode, actualOrganizationAddress.E2_Postcode);
					AssertEquals(expectedOrganizationAddress.State.Code, actualOrganizationAddress.E2_State);
				}
			}
		}

		void AssertContainers(UniversalShipment expectedValueFromXml, CarrierShipmentHeader actualValueFromReader)
		{
			foreach (var expectedContainer in expectedValueFromXml.ContainerCollection)
			{
				var containerNumber = expectedContainer.ContainerNumber;
				var actualCargo = actualValueFromReader.GetMatchedContainerList(containerNumber);
				if (actualCargo == null)
				{
					continue;
				}

				AssertEquals(expected: ZBool.True, actualCargo.CSC_IsTopLevel);
				AssertEquals(expectedContainer.ContainerCount, actualCargo.CSC_PieceCount);
				AssertEquals(expectedContainer.ContainerType?.ISOCode, actualCargo.ChargeableEquipmentType.RC_Code);
				AssertEquals(expectedContainer.ContainerNumber, actualCargo.CSC_EquipmentNo);
				AssertEquals(expectedContainer.Commodity.Code, actualCargo.CSC_RH_NKCommodityCode);
				AssertEquals(expectedContainer.GoodsDescription, actualCargo.CSC_DescriptionOfGoods);
				AssertEquals(expectedContainer.IsEmptyContainer, actualCargo.CSC_IsEmpty);
				AssertEquals(expectedContainer.IsShipperOwned, actualCargo.CSC_IsShipperOwned);
				AssertEquals(expectedContainer.IsNonOperating, actualCargo.CSC_ReeferNonOperated);
				AssertEquals(expectedContainer.GoodsWeight, actualCargo.CSC_CargoWeight);
				AssertEquals(expectedContainer.WeightUnit.Code, actualCargo.CSC_UnitOfWeight);
				AssertEquals(expectedContainer.DunnageWeight, actualCargo.CSC_DunnageWeight);
				AssertEquals(expectedContainer.TareWeight, actualCargo.CSC_EquipmentTareWeight);
				AssertEquals(expectedContainer.OverhangBack, actualCargo.CSC_OOGDoor);
				AssertEquals(expectedContainer.OverhangFront, actualCargo.CSC_OOGFront);
				AssertEquals(expectedContainer.OverhangHeight, actualCargo.CSC_OOGTop);
				AssertEquals(expectedContainer.OverhangLeft, actualCargo.CSC_OOGLeft);
				AssertEquals(expectedContainer.OverhangRight, actualCargo.CSC_OOGRight);
				AssertEquals(expectedContainer.LengthUnit?.Code, actualCargo.CSC_OOGUnit);
				AssertEquals(expectedContainer.Seal, actualCargo.CSC_SealNumber1);
				AssertEquals(expectedContainer.SecondSeal, actualCargo.CSC_SealNumber2);
				AssertEquals(expectedContainer.ThirdSeal, actualCargo.CSC_SealNumber3);
				AssertEquals(expectedContainer.SealPartyType?.Code, actualCargo.CSC_SealParty1);
				AssertEquals(expectedContainer.SecondSealPartyType?.Code, actualCargo.CSC_SealParty2);
				AssertEquals(expectedContainer.ThirdSealPartyType?.Code, actualCargo.CSC_SealParty3);

				var additionalSealNumbers = new[] { actualCargo.CSC_SealNumber4, actualCargo.CSC_SealNumber5, actualCargo.CSC_SealNumber6 };
				AssertContainsExactElementsInExactOrder("Additional Seal Numbers", additionalSealNumbers, expectedContainer.AdditionalSealNumberCollection.Select(s => s.Number.ToString()));

				AssertEquals(expectedContainer.GrossWeightVerificationDateTime, actualCargo.CSC_VGMWeighingDateTime.ToZDateTime());
				AssertEquals(expectedContainer.GrossWeightVerificationType?.Code, actualCargo.CSC_VGMWeighingMethod);
				AssertEquals("ANY", actualCargo.CSC_DeliveryDrayage);
				AssertEquals("ANY", actualCargo.CSC_ReceiptDrayage);
			}
		}

		void AssertPackingLines(UniversalShipment expectedValueFromXml, CarrierShipmentHeader actualValueFromReader, bool isTopLevel = true)
		{
			foreach (var expectedPackingLine in expectedValueFromXml.PackingLineCollection)
			{
				var referenceNumber = expectedPackingLine.ReferenceNumber;
				var actualCargo = actualValueFromReader.GetMatchedBreakBulkList(referenceNumber);
				if (actualCargo == null)
				{
					continue;
				}

				AssertEquals(isTopLevel, actualCargo.CSC_IsTopLevel);
				AssertEquals(expectedPackingLine.PackQty, Convert.ToInt64(actualCargo.CSC_PieceCount));
				AssertEquals(expectedPackingLine.PackType.Code, actualCargo.CSC_F3_NKPackType);
				AssertEquals(expectedPackingLine.Commodity.Code, actualCargo.CSC_RH_NKCommodityCode);
				AssertEquals(expectedPackingLine.DetailedDescription, actualCargo.CSC_DescriptionOfGoods);
				AssertEquals(expectedPackingLine.ReferenceNumber, actualCargo.CSC_IdentificationReference);
				AssertEquals(!expectedPackingLine.NonStackable, actualCargo.CSC_IsStackable);
				AssertEquals(expectedPackingLine.Weight, actualCargo.CSC_CargoWeight);
				AssertEquals(expectedPackingLine.WeightUnit.Code, actualCargo.CSC_UnitOfWeight);
				AssertEquals(expectedPackingLine.Length, actualCargo.CSC_ChargeableLength);
				AssertEquals(expectedPackingLine.LengthUnit.Code, actualCargo.CSC_ChargeableUnitOfDimension);
				AssertEquals(expectedPackingLine.Width, actualCargo.CSC_ChargeableWidth);
				AssertEquals(expectedPackingLine.Height, actualCargo.CSC_ChargeableHeight);
				AssertEquals(expectedPackingLine.DunnageWeight, actualCargo.CSC_DunnageWeight);
				AssertEquals(string.Empty, actualCargo.CSC_DeliveryDrayage);
				AssertEquals(string.Empty, actualCargo.CSC_ReceiptDrayage);
			}
		}

		void AssertContainerLink(ZGuid expectedContainerPk, ZGuid actualContainerPk, ZGuid actualBreakBulkPk)
		{
			AssertEquals(expectedContainerPk, actualContainerPk);

			var parentChildCargoLink = Factory.LoadTop1<CarrierShipmentCargoLink>(
				new ZQuery(CarrierShipmentCargoLinkSchema.CCK_CSC_Parent, actualContainerPk)
					.AddToFilter(CarrierShipmentCargoLinkSchema.CCK_CSC_Child, actualBreakBulkPk));

			AssertNotNull(parentChildCargoLink);
		}

		void AssertLogMessage(UniversalShipment universalShipment, string expectedMessage, LogType logType)
		{
			Logger.ClearLogs();
			var reader = CreateNewReader(universalShipment);
			reader.ReadIntoBusinessObject();
			if (logType == LogType.Error)
			{
				AssertContains(expectedMessage, Logger.GetErrors());
			}
			else
			{
				AssertContains(expectedMessage, Logger.GetWarnings());
			}
		}

		UniversalShipment SetUniversalShipment(ZString? shipmentReference)
		{
			var universalShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.BookingConfirmationReference = shipmentReference;
			universalShipment.GoodsValue = 1000;
			universalShipment.GoodsValueCurrency = new Currency() { Code = "AUD" };
			universalShipment.PlaceOfDelivery = new UNLOCO { Code = "EGDAM" };
			universalShipment.PlaceOfReceipt = new UNLOCO { Code = "AUSYD" };
			universalShipment.PortOfDischarge = new UNLOCO { Code = "USORF" };
			universalShipment.PortOfLoading = new UNLOCO { Code = "DEBRV" };
			universalShipment.PortOfOrigin = new UNLOCO { Code = "DEBRV" };
			universalShipment.PortOfDestination = new UNLOCO { Code = "USORF" };
			universalShipment.WayBillNumber = "HWB12345";
			universalShipment.WayBillType = new WayBillType() { Code = "SWB" };

			return universalShipment;
		}

		IDataContextDataObject SetDataContextDataObject(ZString? key)
		{
			var dataContext = DataContextFactory.New();
			dataContext.AddDataTarget(DataContextType.CarrierShipment, key);

			return dataContext;
		}

		UniversalShipment SetOrganizationAddressCollection(UniversalShipment dataObject)
		{
			var bookingParty = SetOrganizationAddress("TST01", "Booking Party", nameof(DocAddressType.BookingPartyDocumentaryAddress), "Booking Party Address 1", "Booking Party Address 2");
			var notifyParty = SetOrganizationAddress("TST02", "Notify Party", nameof(DocAddressType.NotifyParty), "Notify Party Address 1", "Notify Party Address 2");
			var consigneeParty = SetOrganizationAddress("TST03", "Consignee Party", nameof(DocAddressType.ConsigneeDocumentaryAddress), "Consignee Party Address 1", "Consignee Party Address 2");
			var consignorParty = SetOrganizationAddress("TST04", "Consignor Party", nameof(DocAddressType.ConsignorDocumentaryAddress), "Consignor Party Address 1", "Consignor Party Address 2");

			dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			dataObject.OrganizationAddressCollection.Add(bookingParty);
			dataObject.OrganizationAddressCollection.Add(notifyParty);
			dataObject.OrganizationAddressCollection.Add(consigneeParty);
			dataObject.OrganizationAddressCollection.Add(consignorParty);

			return dataObject;
		}

		OrgHeader SetOrgHeader(ZString orgHeaderCode, ZString orgHeaderName, bool isConsignee, bool isConsignor)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = orgHeaderCode;
			orgHeader.OH_FullName = orgHeaderName;
			orgHeader.OH_IsConsignee = isConsignee;
			orgHeader.OH_IsConsignor = isConsignor;
			var contact = orgHeader.Contacts.AddNew();
			contact.OC_ContactName = "John Doe";
			contact.OC_Email = "test@test.com";
			contact.OC_Phone = "+61295606543";
			contact.OC_Mobile = "+61295606543";

			return orgHeader;
		}

		OrgAddress SetOrgAddress(OrgHeader orgHeader, ZString shortCode, ZString companyName, ZString address1, ZString address2)
		{
			var orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.OA_Code = shortCode;
			orgAddress.OA_CompanyNameOverride = companyName;
			orgAddress.Address1 = address1;
			orgAddress.Address1 = address2;
			orgAddress.City = "SYDNEY";
			orgAddress.Postcode = "2222";
			orgAddress.OA_State = "NSW";
			orgAddress.SetBaseOA_RL_NKRelatedPortCode("AUSYD");
			return orgAddress;
		}

		OrganizationAddress SetOrganizationAddress(ZCodeMappedZString organizationCode, ZString companyName, ZString docAddressType, ZString address1, ZString address2, bool addressOverride = true)
		{
			var organizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressOverride = addressOverride,
				OrganizationCode = organizationCode,
				CompanyName = companyName,
				AddressType = docAddressType,
				Address1 = address1,
				Address2 = address2,
				City = "SYDNEY",
				Contact = "John Doe",
				Country = new Country() { Code = "AU", Name = "AUSTRALIA" },
				Email = "test@test.com",
				Mobile = "+61295606543",
				Phone = "+61295606543",
				Postcode = "2222",
				State = new OrganizationAddressState() { Code = "NSW" }
			};

			return organizationAddress;
		}

		UniversalContainer SetContainer(ZString containerNumber, ZString goodsDescription, ZInt link)
		{
			var container = new UniversalContainer(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ContainerCount = 1,
				ContainerType = new ContainerType { Code = "20GP", ISOCode = "20GP" },
				Link = link,
				ContainerNumber = containerNumber,
				Commodity = new Commodity { Code = "PALL" },
				GoodsDescription = goodsDescription,
				IsEmptyContainer = false,
				IsShipperOwned = false,
				IsNonOperating = false,
				GoodsWeight = 1000,
				WeightUnit = new UnitOfWeight { Code = "KG" },
				GrossWeight = 2000,
				DunnageWeight = 2000,
				TareWeight = 2000,
				OverhangBack = 10,
				OverhangFront = 10,
				OverhangHeight = 10,
				OverhangLeft = 10,
				OverhangRight = 10,
				LengthUnit = new UnitOfLength { Code = "M" },
				Seal = "Seal1",
				SecondSeal = "Seal2",
				ThirdSeal = "Seal3",
				SealPartyType = new CodeDescriptionPair { Code = "C" },
				SecondSealPartyType = new CodeDescriptionPair { Code = "C" },
				ThirdSealPartyType = new CodeDescriptionPair { Code = "C" },
				GrossWeightVerificationDateTime = new ZDateTime(2024, 02, 14, 10, 05, 30),
				GrossWeightVerificationType = new CodeDescriptionPair { Code = "WTA" },
			};

			container = SetAdditionalSealNumberCollection(container);

			return container;
		}

		UniversalContainer SetAdditionalSealNumberCollection(UniversalContainer container)
		{
			container.SetAdditionalSealNumberCollection(() => new List<SealNumber>());
			container.AdditionalSealNumberCollection.Add(new SealNumber { Number = "Seal4" });
			container.AdditionalSealNumberCollection.Add(new SealNumber { Number = "Seal4" });
			container.AdditionalSealNumberCollection.Add(new SealNumber { Number = "Seal5" });

			return container;
		}

		PackingLine SetPackingLine(ZString containerNumber, ZString detailedDescription, ZString referenceNumber, ZInt? containerLink)
		{
			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				PackType = new PackageType { Code = "BAG" },
				Commodity = new Commodity { Code = "PALL" },
				ContainerLink = containerLink,
				ContainerNumber = containerNumber,
				DetailedDescription = detailedDescription,
				ReferenceNumber = referenceNumber,
				NonStackable = false,
				Weight = 1000,
				WeightUnit = new UnitOfWeight { Code = "KG" },
				Length = 10,
				LengthUnit = new UnitOfLength { Code = "M" },
				Width = 10,
				Height = 10,
				DunnageWeight = 500,
			};

			return packingLine;
		}

		CarrierShipmentHeader CreateCarrierShipment()
		{
			var shipment = Factory.New<CarrierShipmentHeader>();
			shipment.CSH_CarrierShipmentReference = "CSH001";
			shipment.Cargoes.AddNew();
			shipment.Cargoes[0].CSC_EquipmentNo = "CNT003";
			shipment.Cargoes[0].CSC_CargoMovementTypeDestination = "FCL";
			shipment.Cargoes[0].CSC_CargoMovementTypeOrigin = "FCL";
			shipment.Cargoes[0].CSC_DeliveryDrayage = "ANY";
			shipment.Cargoes[0].CSC_ReceiptDrayage = "ANY";

			return shipment;
		}

		void CreateOrganizationAddress()
		{
			var orgHeader1 = SetOrgHeader("TST01", "Booking Party", isConsignee: false, isConsignor: false);
			var orgHeader2 = SetOrgHeader("TST02", "Notify Party", isConsignee: false, isConsignor: false);
			var orgHeader3 = SetOrgHeader("TST03", "Consignee Party", isConsignee: true, isConsignor: false);
			var orgHeader4 = SetOrgHeader("TST04", "Consignor Party", isConsignee: false, isConsignor: true);

			SetOrgAddress(orgHeader1, "TST01", "Booking Party", "Booking Party Address 1", "Booking Party Address 2");
			SetOrgAddress(orgHeader2, "TST02", "Notify Party", "Notify Party Address 1", "Notify Party Address 2");
			SetOrgAddress(orgHeader3, "TST03", "Consignee Party", "Consignee Party Address 1", "Consignee Party Address 2");
			SetOrgAddress(orgHeader4, "TST04", "Consignor Party", "Consignor Party Address 1", "Consignor Party Address 2");
		}

		public ZString GetCodeFromDocAddressType(ZString addressType)
		{
			switch (addressType)
			{
				case nameof(DocAddressType.BookingPartyDocumentaryAddress):
					return AutoDocAddressTypes.Codes.BookingPartyDocumentaryAddress;
				case nameof(DocAddressType.ConsignorDocumentaryAddress):
					return AutoDocAddressTypes.Codes.ConsignorDocumentaryAddress;
				case nameof(DocAddressType.ConsigneeDocumentaryAddress):
					return AutoDocAddressTypes.Codes.ConsigneeDocumentaryAddress;
				case nameof(DocAddressType.NotifyParty):
					return AutoDocAddressTypes.Codes.NotifyParty;
			}

			return ZString.Empty;
		}

		public DocAddressType GetDocAddressTypeFromCode(ZString addressType)
		{
			switch (addressType)
			{
				case AutoDocAddressTypes.Codes.BookingPartyDocumentaryAddress:
				case nameof(DocAddressType.BookingPartyDocumentaryAddress):
					return DocAddressType.BookingPartyDocumentaryAddress;
				case AutoDocAddressTypes.Codes.ConsignorDocumentaryAddress:
				case nameof(DocAddressType.ConsignorDocumentaryAddress):
					return DocAddressType.ConsignorDocumentaryAddress;
				case AutoDocAddressTypes.Codes.ConsigneeDocumentaryAddress:
				case nameof(DocAddressType.ConsigneeDocumentaryAddress):
					return DocAddressType.ConsigneeDocumentaryAddress;
				case AutoDocAddressTypes.Codes.NotifyParty:
				case nameof(DocAddressType.NotifyParty):
					return DocAddressType.NotifyParty;
			}

			return DocAddressType.None;
		}

		CarrierShipmentDataObjectReader CreateNewReader(UniversalShipment universalShipment)
		{
			var reader = new CarrierShipmentDataObjectReader(universalShipment, Logger, Factory);

			return reader;
		}
	}
}
