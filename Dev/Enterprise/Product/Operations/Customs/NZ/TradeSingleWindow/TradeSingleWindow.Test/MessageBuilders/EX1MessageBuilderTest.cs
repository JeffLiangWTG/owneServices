using System.Collections.Generic;
using System.Linq;
using CargoWise.IO;
using CargoWise.Types;
using Moq;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders.Testing
{
	class EX1MessageBuilderTest : TSWMessageBuilderTest
	{
		public void TestEX1MessageOriginal()
		{
			var tswAttachment1 = new Mock<ITSWAttachment>();
			tswAttachment1.Setup(m => m.DocType).Returns("CDO");
			tswAttachment1.Setup(m => m.FileName).Returns("TEST1.PDF");

			var tswAttachment2 = new Mock<ITSWAttachment>();
			tswAttachment2.Setup(m => m.DocType).Returns("INV");
			tswAttachment2.Setup(m => m.FileName).Returns("TEST2.PDF");

			var carrierMock = new Mock<IOrganisationSimple>();
			carrierMock.Setup(m => m.Name).Returns("CARRIER_NAME");

			var supportingDocument1Mock = new Mock<ITSWAttachment>();
			supportingDocument1Mock.Setup(m => m.DocType).Returns("CDO");
			supportingDocument1Mock.Setup(m => m.FileName).Returns("TESTFILE1.PDF");

			var supportingDocument2Mock = new Mock<ITSWAttachment>();
			supportingDocument2Mock.Setup(m => m.DocType).Returns("INV");
			supportingDocument2Mock.Setup(m => m.FileName).Returns("TESTFILE1.XLSX");

			iAdditionalInformationMock = new Mock<IAdditionalInformation>();
			iAdditionalInformationMock.Setup(m => m.SupportingDocuments).Returns(new List<ITSWAttachment>() { supportingDocument1Mock.Object, supportingDocument2Mock.Object });
			iAdditionalInformationMock.Setup(m => m.FreeText).Returns("ADDITIONALINFORMATION_FREETEXT");
			iAdditionalInformationMock.Setup(m => m.ManualOverrideText).Returns("ADDITIONALINFORMATION_MANUALOVERRIDETEXT");
			iAdditionalInformationMock.Setup(m => m.AdditionalStatementText).Returns("ADDITIONALINFORMATION_ADDITIONALSTATEMENTTEXT");

			var exchangeRate1Mock = new Mock<ICurrency>();
			exchangeRate1Mock.Setup(m => m.ExchangeRateIndicator).Returns("EXCHANGERATE1_EXCHANGERATEINDICATOR");
			exchangeRate1Mock.Setup(m => m.CurrencyCode).Returns("NZD");
			exchangeRate1Mock.Setup(m => m.ExchangeRate).Returns(2.123);

			var exchangeRate2Mock = new Mock<ICurrency>();
			exchangeRate2Mock.Setup(m => m.ExchangeRateIndicator).Returns("EXCHANGERATE2_EXCHANGERATEINDICATOR");
			exchangeRate2Mock.Setup(m => m.CurrencyCode).Returns("NZD");
			exchangeRate2Mock.Setup(m => m.ExchangeRate).Returns(3.123);

			var packaging1Mock = new Mock<IPackaging>();
			packaging1Mock.Setup(m => m.NumberOfPackages).Returns(3);
			packaging1Mock.Setup(m => m.MessageSequence).Returns(4);
			packaging1Mock.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			packaging1Mock.Setup(m => m.PackageType).Returns("PACKAGING1_PACKAGETYPE");

			var packaging2Mock = new Mock<IPackaging>();
			packaging2Mock.Setup(m => m.NumberOfPackages).Returns(3);
			packaging2Mock.Setup(m => m.MessageSequence).Returns(4);
			packaging2Mock.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			packaging2Mock.Setup(m => m.PackageType).Returns("PACKAGING2_PACKAGETYPE");

			var container1Mock = new Mock<ITransportEquipment>();
			container1Mock.Setup(m => m.MessageSequence).Returns(1);
			container1Mock.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			container1Mock.Setup(m => m.IsPallet).Returns(true);
			container1Mock.Setup(m => m.Size).Returns("CONTAINER1_SIZE");
			container1Mock.Setup(m => m.Status).Returns("CONTAINER1_STATUS");
			container1Mock.Setup(m => m.ContainerNumber).Returns("CONTAINER1_CONTAINERNUMBER");
			container1Mock.Setup(m => m.SealNumbers).Returns(new List<ZString>()
			{ "CONTAINER1_SEALNUMBERS1", "CONTAINER1_SEALNUMBERS2" });
			container1Mock.Setup(m => m.RelatedPackages).Returns(new List<ZGuid>()
			{ ZGuid.NewZGuid(), ZGuid.NewZGuid() });

			var container2Mock = new Mock<ITransportEquipment>();
			container2Mock.Setup(m => m.MessageSequence).Returns(2);
			container2Mock.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			container2Mock.Setup(m => m.IsPallet).Returns(true);
			container2Mock.Setup(m => m.Size).Returns("CONTAINER2_SIZE");
			container2Mock.Setup(m => m.Status).Returns("CONTAINER2_STATUS");
			container2Mock.Setup(m => m.ContainerNumber).Returns("CONTAINER2_CONTAINERNUMBER");
			container2Mock.Setup(m => m.SealNumbers).Returns(new List<ZString>()
			{ "CONTAINER2_SEALNUMBERS1", "CONTAINER2_SEALNUMBERS2" });
			container2Mock.Setup(m => m.RelatedPackages).Returns(new List<ZGuid>()
			{ ZGuid.NewZGuid(), ZGuid.NewZGuid() });

			var exporterMock = new Mock<IOrganisation>();
			exporterMock.Setup(m => m.CustomsClientCode).Returns("EXPORTER_CUSTOMSCLIENTCODE");

			var importerMock = new Mock<IOrganisation>();
			importerMock.Setup(m => m.Name).Returns("IMPORTER_NAME");
			importerMock.Setup(m => m.City).Returns("IMPORTER_CITY");
			importerMock.Setup(m => m.CountryCode).Returns("IMPORTER_COUNTRYCODE");
			importerMock.Setup(m => m.CountryRegion).Returns("IMPORTER_COUNTRYREGION");
			importerMock.Setup(m => m.Address).Returns("IMPORTER_ADDRESS");
			importerMock.Setup(m => m.PostCode).Returns("123456789");

			var dutyTaxFee1Mock = new Mock<IDutyTaxFee>();
			dutyTaxFee1Mock.Setup(m => m.DutyTaxFeeType).Returns("CUD");
			dutyTaxFee1Mock.Setup(m => m.CurrencyCode).Returns("DUTYTAXFEE1_CURRENCYCODE");
			dutyTaxFee1Mock.Setup(m => m.Amount).Returns(9m);

			var dutyTaxFee2Mock = new Mock<IDutyTaxFee>();
			dutyTaxFee2Mock.Setup(m => m.DutyTaxFeeType).Returns("TOT");
			dutyTaxFee2Mock.Setup(m => m.CurrencyCode).Returns("DUTYTAXFEE2_CURRENCYCODE");
			dutyTaxFee2Mock.Setup(m => m.Amount).Returns(9m);

			var iCommunication1Mock = new Mock<ICommunication>();
			iCommunication1Mock.Setup(m => m.ContactDetail).Returns("ICOMMUNICATION1_CONTACTDETAIL");
			iCommunication1Mock.Setup(m => m.ContactType).Returns("EM");

			var iCommunication2Mock = new Mock<ICommunication>();
			iCommunication2Mock.Setup(m => m.ContactDetail).Returns("ICOMMUNICATION1_CONTACTDETAIL");
			iCommunication2Mock.Setup(m => m.ContactType).Returns("EM");

			var declarantMock = new Mock<IDeclarant>();
			declarantMock.Setup(m => m.DeclarantID).Returns("DECLARANT_DECLARANTID");
			declarantMock.Setup(m => m.Communications).Returns(new List<ICommunication>()
			{ iCommunication1Mock.Object, iCommunication2Mock.Object });

			var iOtherInfo1Mock = new Mock<IOtherInfo>();
			iOtherInfo1Mock.Setup(m => m.Code).Returns("IOTHERINFO1_CODE");
			iOtherInfo1Mock.Setup(m => m.Data).Returns("IOTHERINFO1_DATA");

			var iOtherInfo2Mock = new Mock<IOtherInfo>();
			iOtherInfo2Mock.Setup(m => m.Code).Returns("IOTHERINFO2_CODE");
			iOtherInfo2Mock.Setup(m => m.Data).Returns("IOTHERINFO2_DATA");

			var iClassification1Mock = new Mock<IClassification>();
			iClassification1Mock.Setup(m => m.Classification).Returns("ICLASSIFICATION1_CLASSIFICATION");
			iClassification1Mock.Setup(m => m.ClassificationTypeCode).Returns("ICLASSIFICATION1_CLASSIFICATIONTYPECODE");

			var iClassification2Mock = new Mock<IClassification>();
			iClassification2Mock.Setup(m => m.Classification).Returns("ICLASSIFICATION2_CLASSIFICATION");
			iClassification2Mock.Setup(m => m.ClassificationTypeCode).Returns("ICLASSIFICATION2_CLASSIFICATIONTYPECODE");

			var temperaturesMock = new Mock<ITemperatureRequirements>();
			temperaturesMock.Setup(m => m.StorageTemp).Returns(6m);
			temperaturesMock.Setup(m => m.MinStorageTemp).Returns(6m);
			temperaturesMock.Setup(m => m.MaxStorageTemp).Returns(6m);

			var iPackaging1Mock = new Mock<IPackaging>();
			iPackaging1Mock.Setup(m => m.ShippingMarks).Returns("IPACKAGING1_SHIPPINGMARKS");
			iPackaging1Mock.Setup(m => m.NumberOfPackages).Returns(3);
			iPackaging1Mock.Setup(m => m.PackageType).Returns("IPACKAGING1_SHIPPINGMARKS");
			iPackaging1Mock.Setup(m => m.PackageVolumeInMTQ).Returns(7m);

			var iPackaging2Mock = new Mock<IPackaging>();
			iPackaging2Mock.Setup(m => m.ShippingMarks).Returns("IPACKAGING2_SHIPPINGMARKS");
			iPackaging2Mock.Setup(m => m.NumberOfPackages).Returns(3);
			iPackaging2Mock.Setup(m => m.PackageType).Returns("IPACKAGING2_SHIPPINGMARKS");
			iPackaging2Mock.Setup(m => m.PackageVolumeInMTQ).Returns(7m);

			var iProduct1Mock = new Mock<IProduct>();
			iProduct1Mock.Setup(m => m.Id).Returns("IPRODUCT1_ID");
			iProduct1Mock.Setup(m => m.IdType).Returns("IPRODUCT1_IDTYPE");

			var iProduct2Mock = new Mock<IProduct>();
			iProduct2Mock.Setup(m => m.Id).Returns("IPRODUCT2_ID");
			iProduct2Mock.Setup(m => m.IdType).Returns("IPRODUCT2_IDTYPE");

			var item1Mock = new Mock<IGoodsItems>();
			item1Mock.Setup(m => m.Permits).Returns(new List<ZString>()
			{ "ITEM1_PERMITS1", "ITEM2_PERMITS2" });
			item1Mock.Setup(m => m.ProhibitedCodes).Returns(new List<ZString>()
			{ "ITEM1_PROHIBITEDCODES1", "ITEM1_PROHIBITEDCODES2" });
			item1Mock.Setup(m => m.OtherInfoCodes).Returns(new List<IOtherInfo>()
			{ iOtherInfo1Mock.Object, iOtherInfo2Mock.Object });
			item1Mock.Setup(m => m.GoodsDescription).Returns("ITEM1_GOODSDESCRIPTION");
			item1Mock.Setup(m => m.ValueInForeignCurrency).Returns(6m);
			item1Mock.Setup(m => m.ForeignCurrencyCode).Returns("ITEM1_FOREIGNCURRENCYCODE");
			item1Mock.Setup(m => m.Classifications).Returns(new List<IClassification>()
			{ iClassification1Mock.Object, iClassification2Mock.Object });
			item1Mock.Setup(m => m.BrandName).Returns("ITEM1_BRANDNAME");
			item1Mock.Setup(m => m.CommonName).Returns("ITEM1_COMMONNAME");
			item1Mock.Setup(m => m.RegisteredName).Returns("ITEM1_REGISTEREDNAME");
			item1Mock.Setup(m => m.TradeName).Returns("ITEM1_TRADENAME");
			item1Mock.Setup(m => m.Temperatures).Returns(temperaturesMock.Object);
			item1Mock.Setup(m => m.ContainerNumbers).Returns(new List<ZString>()
			{ "ITEM1_CONTAINERNUMBERS1", "ITEM1_CONTAINERNUMBERS2" });
			item1Mock.Setup(m => m.ItemGrossWeightInKGM).Returns(6m);
			item1Mock.Setup(m => m.ItemNetWeightInKGM).Returns(3m);
			item1Mock.Setup(m => m.StatisticalQtyUnit).Returns("ITEM1_STATISTICALQTYUNIT");
			item1Mock.Setup(m => m.StatisticalQty).Returns(1m);
			item1Mock.Setup(m => m.SupplementaryQty).Returns(10m);
			item1Mock.Setup(m => m.OriginCountry).Returns("ITEM1_ORIGINCOUNTRY");
			item1Mock.Setup(m => m.Packaging).Returns(new List<IPackaging>()
			{ iPackaging1Mock.Object, iPackaging2Mock.Object });
			item1Mock.Setup(m => m.UsedGoods).Returns(true);
			item1Mock.Setup(m => m.GeneticallyModified).Returns(true);
			item1Mock.Setup(m => m.SupplementaryQtyUnit).Returns("BDU");
			item1Mock.Setup(m => m.LineDutyTaxFees).Returns(new List<IDutyTaxFee>()
			{ dutyTaxFee1Mock.Object });
			item1Mock.Setup(m => m.Products).Returns(new List<IProduct>()
			{ iProduct1Mock.Object, iProduct2Mock.Object });

			var item2Mock = new Mock<IGoodsItems>();
			item2Mock.Setup(m => m.Permits).Returns(new List<ZString>()
			{ "ITEM2_PERMITS1", "ITEM2_PERMITS2" });
			item2Mock.Setup(m => m.ProhibitedCodes).Returns(new List<ZString>()
			{ "ITEM2_PROHIBITEDCODES1", "ITEM2_PROHIBITEDCODES2" });
			item2Mock.Setup(m => m.OtherInfoCodes).Returns(new List<IOtherInfo>()
			{ iOtherInfo1Mock.Object, iOtherInfo2Mock.Object });
			item2Mock.Setup(m => m.GoodsDescription).Returns("ITEM2_GOODSDESCRIPTION");
			item2Mock.Setup(m => m.ValueInForeignCurrency).Returns(6m);
			item2Mock.Setup(m => m.ForeignCurrencyCode).Returns("ITEM2_FOREIGNCURRENCYCODE");
			item2Mock.Setup(m => m.Classifications).Returns(new List<IClassification>()
			{ iClassification1Mock.Object, iClassification2Mock.Object });
			item2Mock.Setup(m => m.BrandName).Returns("ITEM2_BRANDNAME");
			item2Mock.Setup(m => m.CommonName).Returns("ITEM2_COMMONNAME");
			item2Mock.Setup(m => m.RegisteredName).Returns("ITEM2_REGISTEREDNAME");
			item2Mock.Setup(m => m.TradeName).Returns("ITEM2_TRADENAME");
			item2Mock.Setup(m => m.Temperatures).Returns(temperaturesMock.Object);
			item2Mock.Setup(m => m.ContainerNumbers).Returns(new List<ZString>()
			{ "ITEM2_CONTAINERNUMBERS1", "ITEM2_CONTAINERNUMBERS2" });
			item2Mock.Setup(m => m.ItemGrossWeightInKGM).Returns(6m);
			item2Mock.Setup(m => m.ItemNetWeightInKGM).Returns(3m);
			item2Mock.Setup(m => m.StatisticalQtyUnit).Returns("ITEM2_STATISTICALQTYUNIT");
			item2Mock.Setup(m => m.StatisticalQty).Returns(1m);
			item2Mock.Setup(m => m.SupplementaryQty).Returns(10m);
			item2Mock.Setup(m => m.OriginCountry).Returns("ITEM2_ORIGINCOUNTRY");
			item2Mock.Setup(m => m.Packaging).Returns(new List<IPackaging>()
			{ iPackaging1Mock.Object, iPackaging2Mock.Object });
			item2Mock.Setup(m => m.UsedGoods).Returns(true);
			item2Mock.Setup(m => m.GeneticallyModified).Returns(true);
			item2Mock.Setup(m => m.SupplementaryQtyUnit).Returns("BDU");
			item2Mock.Setup(m => m.LineDutyTaxFees).Returns(new List<IDutyTaxFee>()
			{ dutyTaxFee1Mock.Object });
			item2Mock.Setup(m => m.Products).Returns(new List<IProduct>()
			{ iProduct1Mock.Object, iProduct2Mock.Object });

			var iInvoice1Mock = new Mock<IInvoice>();
			iInvoice1Mock.Setup(m => m.InvoiceDate).Returns(new ZDateTime(2018, 11, 7));
			iInvoice1Mock.Setup(m => m.InvoiceNumber).Returns("IINVOICE1_INVOICENUMBER");

			var iInvoice2Mock = new Mock<IInvoice>();
			iInvoice2Mock.Setup(m => m.InvoiceDate).Returns(new ZDateTime(2015, 4, 2));
			iInvoice2Mock.Setup(m => m.InvoiceNumber).Returns("IINVOICE2_INVOICENUMBER");

			var notifyParty1Mock = new Mock<IOrganisation>();
			notifyParty1Mock.Setup(m => m.Name).Returns("NOTIFYPARTY1_NAME");
			notifyParty1Mock.Setup(m => m.CustomsClientCode).Returns("NOTIFYPARTY1_CUSTOMSCLIENTCODE");
			notifyParty1Mock.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1Mock.Object, iCommunication2Mock.Object });

			var notifyParty2Mock = new Mock<IOrganisation>();
			notifyParty2Mock.Setup(m => m.Name).Returns("NOTIFYPARTY2_NAME");
			notifyParty2Mock.Setup(m => m.CustomsClientCode).Returns("NOTIFYPARTY2_CUSTOMSCLIENTCODE");
			notifyParty2Mock.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1Mock.Object, iCommunication2Mock.Object });

			var stuffingEstablishment1Mock = new Mock<IOrganisation>();
			stuffingEstablishment1Mock.Setup(m => m.Name).Returns("STUFFINGESTABLISHMENT1_NAME");
			stuffingEstablishment1Mock.Setup(m => m.City).Returns("STUFFINGESTABLISHMENT1_CITY");
			stuffingEstablishment1Mock.Setup(m => m.CountryCode).Returns("STUFFINGESTABLISHMENT1_COUNTRYCODE");
			stuffingEstablishment1Mock.Setup(m => m.CountryRegion).Returns("STUFFINGESTABLISHMENT1_COUNTRYREGION");
			stuffingEstablishment1Mock.Setup(m => m.Address).Returns("STUFFINGESTABLISHMENT1_ADDRESS");
			stuffingEstablishment1Mock.Setup(m => m.PostCode).Returns("123456789");

			var stuffingEstablishment2Mock = new Mock<IOrganisation>();
			stuffingEstablishment2Mock.Setup(m => m.Name).Returns("STUFFINGESTABLISHMENT2_NAME");
			stuffingEstablishment2Mock.Setup(m => m.City).Returns("STUFFINGESTABLISHMENT2_CITY");
			stuffingEstablishment2Mock.Setup(m => m.CountryCode).Returns("STUFFINGESTABLISHMENT2_COUNTRYCODE");
			stuffingEstablishment2Mock.Setup(m => m.CountryRegion).Returns("STUFFINGESTABLISHMENT2_COUNTRYREGION");
			stuffingEstablishment2Mock.Setup(m => m.Address).Returns("STUFFINGESTABLISHMENT2_ADDRESS");
			stuffingEstablishment2Mock.Setup(m => m.PostCode).Returns("123456789");

			var deliverToPartyMock = new Mock<IOrganisation>();
			deliverToPartyMock.Setup(m => m.Name).Returns("DELIVERTOPARTY_NAME");
			deliverToPartyMock.Setup(m => m.City).Returns("DELIVERTOPARTY_CITY");
			deliverToPartyMock.Setup(m => m.CountryCode).Returns("DELIVERTOPARTY_COUNTRYCODE");
			deliverToPartyMock.Setup(m => m.CountryRegion).Returns("DELIVERTOPARTY_COUNTRYREGION");
			deliverToPartyMock.Setup(m => m.Address).Returns("DELIVERTOPARTY_ADDRESS");
			deliverToPartyMock.Setup(m => m.PostCode).Returns("123456789");

			var goodsShipmentMock = new Mock<IGoodsShipment>();
			goodsShipmentMock.Setup(m => m.NatureOfTransaction).Returns("GOODSSHIPMENT_NATUREOFTRANSACTION");
			goodsShipmentMock.Setup(m => m.LocationOfGoods).Returns("GOODSSHIPMENT_LOCATIONOFGOODS");
			goodsShipmentMock.Setup(m => m.PortOfLoading).Returns("GOODSSHIPMENT_PORTOFLOADING");
			goodsShipmentMock.Setup(m => m.PortOfDischarge).Returns("GOODSSHIPMENT_PORTOFDISCHARGE");
			goodsShipmentMock.Setup(m => m.Items).Returns(new List<IGoodsItems>() { item1Mock.Object, item2Mock.Object });
			goodsShipmentMock.Setup(m => m.Invoices).Returns(new List<IInvoice>() { iInvoice1Mock.Object, iInvoice2Mock.Object });
			goodsShipmentMock.Setup(m => m.NotifyParties).Returns(new List<IOrganisationSimple>() { notifyParty1Mock.Object, notifyParty2Mock.Object });
			goodsShipmentMock.Setup(m => m.StuffingEstablishments).Returns(new List<IOrganisation>() { stuffingEstablishment1Mock.Object, stuffingEstablishment2Mock.Object });
			goodsShipmentMock.Setup(m => m.CustomsControlledArea).Returns("GOODSSHIPMENT_CUSTOMSCONTROLLEDAREA");
			goodsShipmentMock.Setup(m => m.NotifyPartyCodes).Returns(new ZString[] { "1111A", "ARRYN" });
			goodsShipmentMock.Setup(m => m.DeliverToParty).Returns(deliverToPartyMock.Object);

			var iExportDeclarationMock = new Mock<IExportDeclaration>();
			iExportDeclarationMock.Setup(m => m.TSWReferenceNumber).Returns("ENTRY12345");
			iExportDeclarationMock.Setup(m => m.SenderReferenceNumber).Returns("C00001165");
			iExportDeclarationMock.Setup(m => m.IsSea).Returns(true);
			iExportDeclarationMock.Setup(m => m.CraftName).Returns("CRAFTNAME");
			iExportDeclarationMock.Setup(m => m.FlightNo).Returns("FLIGHTNO");
			iExportDeclarationMock.Setup(m => m.LloydsNo).Returns("LLOYDSNO");
			iExportDeclarationMock.Setup(m => m.VoyageNo).Returns("VOYAGENO123");
			iExportDeclarationMock.Setup(m => m.Carrier).Returns(carrierMock.Object);
			iExportDeclarationMock.Setup(m => m.IsSea).Returns(true);
			iExportDeclarationMock.Setup(m => m.IsAir).Returns(false);
			iExportDeclarationMock.Setup(m => m.IsMail).Returns(false);
			iExportDeclarationMock.Setup(m => m.TotalGrossWeightInKGM).Returns(0.2m);
			iExportDeclarationMock.Setup(m => m.SubmitterCode).Returns("SUBMITTERCODE");
			iExportDeclarationMock.Setup(m => m.HandlingInformation).Returns("HANDLINGINFORMATION");
			iExportDeclarationMock.Setup(m => m.BrokerCode).Returns("BROKERCODE");
			iExportDeclarationMock.Setup(m => m.PaymentType).Returns("PAYMENTTYPE");
			iExportDeclarationMock.Setup(m => m.DateOfExport).Returns(new ZDateTime(2018, 11, 10));
			iExportDeclarationMock.Setup(m => m.IsContainerised).Returns(true);
			iExportDeclarationMock.Setup(m => m.PreviousDocumentNo).Returns("PREVIOUSDOCUMENTNO");
			iExportDeclarationMock.Setup(m => m.PreviousDocumentType).Returns("PREVIOUSDOCUMENTTYPE");
			iExportDeclarationMock.Setup(m => m.MessageType).Returns("E41");
			iExportDeclarationMock.Setup(m => m.IsCompletionEntry).Returns(true);
			iExportDeclarationMock.Setup(m => m.AdditionalInformation).Returns(iAdditionalInformationMock.Object);
			iExportDeclarationMock.Setup(m => m.Permits).Returns(new List<ZString>() { "PERMIT1", "PERMIT2" });
			iExportDeclarationMock.Setup(m => m.ExchangeRates).Returns(new List<ICurrency>() { exchangeRate1Mock.Object, exchangeRate2Mock.Object });
			iExportDeclarationMock.Setup(m => m.Packaging).Returns(new List<IPackaging>() { packaging1Mock.Object, packaging2Mock.Object });
			iExportDeclarationMock.Setup(m => m.Equipment).Returns(new List<ITransportEquipment>() { container1Mock.Object, container2Mock.Object });
			iExportDeclarationMock.Setup(m => m.Exporter).Returns(exporterMock.Object);
			iExportDeclarationMock.Setup(m => m.Importer).Returns(importerMock.Object);
			iExportDeclarationMock.Setup(m => m.DutyTaxFees).Returns(new List<IDutyTaxFee>() { dutyTaxFee1Mock.Object, dutyTaxFee2Mock.Object });
			iExportDeclarationMock.Setup(m => m.Declarant).Returns(declarantMock.Object);
			iExportDeclarationMock.Setup(m => m.Permits).Returns(new List<ZString>() { "PERMIT1", "PERMIT2" });
			iExportDeclarationMock.Setup(m => m.OtherReferencedDocuments).Returns(new List<IOtherInfo>() { iOtherInfo1Mock.Object, iOtherInfo2Mock.Object });
			iExportDeclarationMock.Setup(m => m.OtherInfoCodes).Returns(new List<IOtherInfo>() { iOtherInfo1Mock.Object, iOtherInfo2Mock.Object });
			iExportDeclarationMock.Setup(m => m.GoodsShipment).Returns(goodsShipmentMock.Object);

			var houseBillChild1Mock = new Mock<IAssociatedTransportDocument>();
			PopulateChildHouseBillMock(houseBillChild1Mock, "1");
			var houseBillChild2Mock = new Mock<IAssociatedTransportDocument>();
			PopulateChildHouseBillMock(houseBillChild2Mock, "2");
			var houseBillChild3Mock = new Mock<IAssociatedTransportDocument>();
			PopulateChildHouseBillMock(houseBillChild3Mock, "3");
			var houseBillChild4Mock = new Mock<IAssociatedTransportDocument>();
			PopulateChildHouseBillMock(houseBillChild4Mock, "4");
			var houseBillChild5Mock = new Mock<IAssociatedTransportDocument>();
			PopulateChildHouseBillMock(houseBillChild5Mock, "5");
			var houseBillChild6Mock = new Mock<IAssociatedTransportDocument>();
			PopulateChildHouseBillMock(houseBillChild6Mock, "6");
			var houseBillChild7Mock = new Mock<IAssociatedTransportDocument>();
			PopulateChildHouseBillMock(houseBillChild7Mock, "7");
			var houseBillChild8Mock = new Mock<IAssociatedTransportDocument>();
			PopulateChildHouseBillMock(houseBillChild8Mock, "8");

			var houseBill1Mock = new Mock<IMasterBillTransportDocument>();
			PopulateHouseBillMock(houseBill1Mock, "1", houseBillChild1Mock.Object.PK, houseBillChild2Mock.Object.PK);
			var houseBill2Mock = new Mock<IMasterBillTransportDocument>();
			PopulateHouseBillMock(houseBill2Mock, "2", houseBillChild3Mock.Object.PK, houseBillChild4Mock.Object.PK);
			var houseBill3Mock = new Mock<IMasterBillTransportDocument>();
			PopulateHouseBillMock(houseBill3Mock, "3", houseBillChild5Mock.Object.PK, houseBillChild6Mock.Object.PK);
			var houseBill4Mock = new Mock<IMasterBillTransportDocument>();
			PopulateHouseBillMock(houseBill4Mock, "4", houseBillChild7Mock.Object.PK, houseBillChild8Mock.Object.PK);

			iExportDeclarationMock.Setup(m => m.AllBills).Returns(new List<IAssociatedTransportDocument>()
			{ houseBill1Mock.Object, houseBill2Mock.Object, houseBill3Mock.Object, houseBill4Mock.Object });

			var masterBill1Mock = new Mock<IMasterBillTransportDocument>();
			PopulateMasterBillMock(masterBill1Mock, "1", houseBill1Mock.Object.PK, houseBill2Mock.Object.PK);
			var masterBill2Mock = new Mock<IMasterBillTransportDocument>();
			PopulateMasterBillMock(masterBill2Mock, "2", houseBill3Mock.Object.PK, houseBill4Mock.Object.PK);

			iExportDeclarationMock.Setup(m => m.MasterBills).Returns(new List<IMasterBillTransportDocument>()
			{ masterBill1Mock.Object, masterBill2Mock.Object });
			ex1Builder = new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original);
			embeddedResourceRetriever = new EmbeddedResourceRetriever();

			AssertMultilineASCIIEquals(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TestEX1MessageOriginal.txt")), ex1Builder.GetXMLMessage());
			iExportDeclarationMock.Setup(m => m.Carrier);
			var unexpectedXML = @"<Carrier>
    <Name>CARRIER_NAME</Name>
  </Carrier>";
			ex1Builder = new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original);
			AssertNotContains(unexpectedXML, ex1Builder.GetXMLMessage());
			iExportDeclarationMock.Setup(m => m.MessageType).Returns("E40");
			unexpectedXML = @"<DutyTaxFee>
    <TypeCode>CUD</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""AUD"">9</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>TOT</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""AUD"">9</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>";
			AssertNotContains(unexpectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original).GetXMLMessage());
		}

		public void TestEX1MessageCompletion()
		{
			var carrierMock = new Mock<IOrganisationSimple>();
			carrierMock.Setup(m => m.Name).Returns("CARRIER_NAME");

			var supportingDocument1Mock = new Mock<ITSWAttachment>();
			supportingDocument1Mock.Setup(m => m.DocType).Returns("CDO");
			supportingDocument1Mock.Setup(m => m.FileName).Returns("TESTFILE1.PDF");

			var supportingDocument2Mock = new Mock<ITSWAttachment>();
			supportingDocument2Mock.Setup(m => m.DocType).Returns("INV");
			supportingDocument2Mock.Setup(m => m.FileName).Returns("TESTFILE1.XLSX");

			var iAdditionalInformationMock = new Mock<IAdditionalInformation>();
			iAdditionalInformationMock.Setup(m => m.AdditionalStatementText).Returns("ADDITIONALINFORMATION_ADDITIONALSTATEMENTTEXT");
			iAdditionalInformationMock.Setup(m => m.ManualOverrideText).Returns("ADDITIONALINFORMATION_MANUALOVERRIDETEXT");
			iAdditionalInformationMock.Setup(m => m.FreeText).Returns("ADDITIONALINFORMATION_FREETEXT");
			iAdditionalInformationMock.Setup(m => m.SupportingDocuments).Returns(new List<ITSWAttachment>() { supportingDocument1Mock.Object, supportingDocument2Mock.Object });

			var exchangeRate1Mock = new Mock<ICurrency>();
			exchangeRate1Mock.Setup(m => m.ExchangeRateIndicator).Returns("EXCHANGERATE1_EXCHANGERATEINDICATOR");
			exchangeRate1Mock.Setup(m => m.CurrencyCode).Returns("NZD");
			exchangeRate1Mock.Setup(m => m.ExchangeRate).Returns(2.123);

			var exchangeRate2Mock = new Mock<ICurrency>();
			exchangeRate2Mock.Setup(m => m.ExchangeRateIndicator).Returns("EXCHANGERATE2_EXCHANGERATEINDICATOR");
			exchangeRate2Mock.Setup(m => m.CurrencyCode).Returns("NZD");
			exchangeRate2Mock.Setup(m => m.ExchangeRate).Returns(3.123);

			var packaging1Mock = new Mock<IPackaging>();
			packaging1Mock.Setup(m => m.NumberOfPackages).Returns(3);
			packaging1Mock.Setup(m => m.MessageSequence).Returns(4);
			packaging1Mock.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			packaging1Mock.Setup(m => m.PackageType).Returns("PACKAGING1_PACKAGETYPE");

			var packaging2Mock = new Mock<IPackaging>();
			packaging2Mock.Setup(m => m.NumberOfPackages).Returns(3);
			packaging2Mock.Setup(m => m.MessageSequence).Returns(4);
			packaging2Mock.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			packaging2Mock.Setup(m => m.PackageType).Returns("PACKAGING2_PACKAGETYPE");

			var container1Mock = new Mock<ITransportEquipment>();
			container1Mock.Setup(m => m.MessageSequence).Returns(1);
			container1Mock.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			container1Mock.Setup(m => m.IsPallet).Returns(true);
			container1Mock.Setup(m => m.Size).Returns("CONTAINER1_SIZE");
			container1Mock.Setup(m => m.Status).Returns("CONTAINER1_STATUS");
			container1Mock.Setup(m => m.ContainerNumber).Returns("CONTAINER1_CONTAINERNUMBER");
			container1Mock.Setup(m => m.SealNumbers).Returns(new List<ZString>() { "CONTAINER1_SEALNUMBERS1", "CONTAINER1_SEALNUMBERS2" });
			container1Mock.Setup(m => m.RelatedPackages).Returns(new List<ZGuid>() { ZGuid.NewZGuid(), ZGuid.NewZGuid() });

			var container2Mock = new Mock<ITransportEquipment>();
			container2Mock.Setup(m => m.MessageSequence).Returns(2);
			container2Mock.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			container2Mock.Setup(m => m.IsPallet).Returns(true);
			container2Mock.Setup(m => m.Size).Returns("CONTAINER2_SIZE");
			container2Mock.Setup(m => m.Status).Returns("CONTAINER2_STATUS");
			container2Mock.Setup(m => m.ContainerNumber).Returns("CONTAINER2_CONTAINERNUMBER");
			container2Mock.Setup(m => m.SealNumbers).Returns(new List<ZString>() { "CONTAINER2_SEALNUMBERS1", "CONTAINER2_SEALNUMBERS2" });
			container2Mock.Setup(m => m.RelatedPackages).Returns(new List<ZGuid>() { ZGuid.NewZGuid(), ZGuid.NewZGuid() });

			var exporterMock = new Mock<IOrganisation>();
			exporterMock.Setup(m => m.CustomsClientCode).Returns("EXPORTER_CUSTOMSCLIENTCODE");

			var importerMock = new Mock<IOrganisation>();
			importerMock.Setup(m => m.Name).Returns("IMPORTER_NAME");
			importerMock.Setup(m => m.City).Returns("IMPORTER_CITY");
			importerMock.Setup(m => m.CountryCode).Returns("IMPORTER_COUNTRYCODE");
			importerMock.Setup(m => m.CountryRegion).Returns("IMPORTER_COUNTRYREGION");
			importerMock.Setup(m => m.Address).Returns("IMPORTER_ADDRESS");
			importerMock.Setup(m => m.PostCode).Returns("123456789");

			var dutyTaxFee1Mock = new Mock<IDutyTaxFee>();
			dutyTaxFee1Mock.Setup(m => m.DutyTaxFeeType).Returns("CUD");
			dutyTaxFee1Mock.Setup(m => m.CurrencyCode).Returns("DUTYTAXFEE1_CURRENCYCODE");
			dutyTaxFee1Mock.Setup(m => m.Amount).Returns(9m);

			var dutyTaxFee2Mock = new Mock<IDutyTaxFee>();
			dutyTaxFee2Mock.Setup(m => m.DutyTaxFeeType).Returns("TOT");
			dutyTaxFee2Mock.Setup(m => m.CurrencyCode).Returns("DUTYTAXFEE2_CURRENCYCODE");
			dutyTaxFee2Mock.Setup(m => m.Amount).Returns(9m);

			var iCommunication1Mock = new Mock<ICommunication>();
			iCommunication1Mock.Setup(m => m.ContactDetail).Returns("ICOMMUNICATION1_CONTACTDETAIL");
			iCommunication1Mock.Setup(m => m.ContactType).Returns("EM");

			var iCommunication2Mock = new Mock<ICommunication>();
			iCommunication2Mock.Setup(m => m.ContactDetail).Returns("ICOMMUNICATION1_CONTACTDETAIL");
			iCommunication2Mock.Setup(m => m.ContactType).Returns("EM");

			var declarantMock = new Mock<IDeclarant>();
			declarantMock.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1Mock.Object, iCommunication2Mock.Object });
			declarantMock.Setup(m => m.DeclarantID).Returns("DECLARANT_DECLARANTID");

			var iOtherInfo1Mock = new Mock<IOtherInfo>();
			iOtherInfo1Mock.Setup(m => m.Code).Returns("IOTHERINFO1_CODE");
			iOtherInfo1Mock.Setup(m => m.Data).Returns("IOTHERINFO1_DATA");

			var iOtherInfo2Mock = new Mock<IOtherInfo>();
			iOtherInfo2Mock.Setup(m => m.Code).Returns("IOTHERINFO2_CODE");
			iOtherInfo2Mock.Setup(m => m.Data).Returns("IOTHERINFO2_DATA");

			var iClassification1Mock = new Mock<IClassification>();
			iClassification1Mock.Setup(m => m.Classification).Returns("ICLASSIFICATION1_CLASSIFICATION");
			iClassification1Mock.Setup(m => m.ClassificationTypeCode).Returns("ICLASSIFICATION1_CLASSIFICATIONTYPECODE");

			var iClassification2Mock = new Mock<IClassification>();
			iClassification2Mock.Setup(m => m.Classification).Returns("ICLASSIFICATION2_CLASSIFICATION");
			iClassification2Mock.Setup(m => m.ClassificationTypeCode).Returns("ICLASSIFICATION2_CLASSIFICATIONTYPECODE");

			var temperaturesMock = new Mock<ITemperatureRequirements>();
			temperaturesMock.Setup(m => m.StorageTemp).Returns(6m);
			temperaturesMock.Setup(m => m.MinStorageTemp).Returns(6m);
			temperaturesMock.Setup(m => m.MaxStorageTemp).Returns(6m);

			var iPackaging1Mock = new Mock<IPackaging>();
			iPackaging1Mock.Setup(m => m.ShippingMarks).Returns("IPACKAGING1_SHIPPINGMARKS");
			iPackaging1Mock.Setup(m => m.NumberOfPackages).Returns(3);
			iPackaging1Mock.Setup(m => m.PackageType).Returns("IPACKAGING1_SHIPPINGMARKS");
			iPackaging1Mock.Setup(m => m.PackageVolumeInMTQ).Returns(7m);

			var iPackaging2Mock = new Mock<IPackaging>();
			iPackaging2Mock.Setup(m => m.ShippingMarks).Returns("IPACKAGING2_SHIPPINGMARKS");
			iPackaging2Mock.Setup(m => m.NumberOfPackages).Returns(3);
			iPackaging2Mock.Setup(m => m.PackageType).Returns("IPACKAGING2_SHIPPINGMARKS");
			iPackaging2Mock.Setup(m => m.PackageVolumeInMTQ).Returns(7m);

			var iProduct1Mock = new Mock<IProduct>();
			iProduct1Mock.Setup(m => m.Id).Returns("IPRODUCT1_ID");
			iProduct1Mock.Setup(m => m.IdType).Returns("IPRODUCT1_IDTYPE");

			var iProduct2Mock = new Mock<IProduct>();
			iProduct2Mock.Setup(m => m.Id).Returns("IPRODUCT2_ID");
			iProduct2Mock.Setup(m => m.IdType).Returns("IPRODUCT2_IDTYPE");

			var item1Mock = new Mock<IGoodsItems>();
			item1Mock.Setup(m => m.Permits).Returns(new List<ZString>() { "ITEM1_PERMITS1", "ITEM2_PERMITS2" });
			item1Mock.Setup(m => m.ProhibitedCodes).Returns(new List<ZString>() { "ITEM1_PROHIBITEDCODES1", "ITEM1_PROHIBITEDCODES2" });
			item1Mock.Setup(m => m.OtherInfoCodes).Returns(new List<IOtherInfo>() { iOtherInfo1Mock.Object, iOtherInfo2Mock.Object });
			item1Mock.Setup(m => m.GoodsDescription).Returns("ITEM1_GOODSDESCRIPTION");
			item1Mock.Setup(m => m.ValueInForeignCurrency).Returns(6m);
			item1Mock.Setup(m => m.ForeignCurrencyCode).Returns("ITEM1_FOREIGNCURRENCYCODE");
			item1Mock.Setup(m => m.Classifications).Returns(new List<IClassification>() { iClassification1Mock.Object, iClassification2Mock.Object });
			item1Mock.Setup(m => m.BrandName).Returns("ITEM1_BRANDNAME");
			item1Mock.Setup(m => m.CommonName).Returns("ITEM1_COMMONNAME");
			item1Mock.Setup(m => m.RegisteredName).Returns("ITEM1_REGISTEREDNAME");
			item1Mock.Setup(m => m.TradeName).Returns("ITEM1_TRADENAME");
			item1Mock.Setup(m => m.Temperatures).Returns(temperaturesMock.Object);
			item1Mock.Setup(m => m.ContainerNumbers).Returns(new List<ZString>() { "ITEM1_CONTAINERNUMBERS1", "ITEM1_CONTAINERNUMBERS2" });
			item1Mock.Setup(m => m.ItemGrossWeightInKGM).Returns(6m);
			item1Mock.Setup(m => m.ItemNetWeightInKGM).Returns(3m);
			item1Mock.Setup(m => m.StatisticalQtyUnit).Returns("ITEM1_STATISTICALQTYUNIT");
			item1Mock.Setup(m => m.StatisticalQty).Returns(1m);
			item1Mock.Setup(m => m.SupplementaryQty).Returns(10m);
			item1Mock.Setup(m => m.OriginCountry).Returns("ITEM1_ORIGINCOUNTRY");
			item1Mock.Setup(m => m.Packaging).Returns(new List<IPackaging>() { iPackaging1Mock.Object, iPackaging2Mock.Object });
			item1Mock.Setup(m => m.UsedGoods).Returns(true);
			item1Mock.Setup(m => m.GeneticallyModified).Returns(true);
			item1Mock.Setup(m => m.SupplementaryQtyUnit).Returns("BDU");
			item1Mock.Setup(m => m.LineDutyTaxFees).Returns(new List<IDutyTaxFee>() { dutyTaxFee1Mock.Object });
			item1Mock.Setup(m => m.Products).Returns(new List<IProduct>() { iProduct1Mock.Object, iProduct2Mock.Object });

			var item2Mock = new Mock<IGoodsItems>();
			item2Mock.Setup(m => m.Permits).Returns(new List<ZString>() { "ITEM2_PERMITS1", "ITEM2_PERMITS2" });
			item2Mock.Setup(m => m.ProhibitedCodes).Returns(new List<ZString>() { "ITEM2_PROHIBITEDCODES1", "ITEM2_PROHIBITEDCODES2" });
			item2Mock.Setup(m => m.OtherInfoCodes).Returns(new List<IOtherInfo>() { iOtherInfo1Mock.Object, iOtherInfo2Mock.Object });
			item2Mock.Setup(m => m.GoodsDescription).Returns("ITEM2_GOODSDESCRIPTION");
			item2Mock.Setup(m => m.ValueInForeignCurrency).Returns(6m);
			item2Mock.Setup(m => m.ForeignCurrencyCode).Returns("ITEM2_FOREIGNCURRENCYCODE");
			item2Mock.Setup(m => m.Classifications).Returns(new List<IClassification>() { iClassification1Mock.Object, iClassification2Mock.Object });
			item2Mock.Setup(m => m.BrandName).Returns("ITEM2_BRANDNAME");
			item2Mock.Setup(m => m.CommonName).Returns("ITEM2_COMMONNAME");
			item2Mock.Setup(m => m.RegisteredName).Returns("ITEM2_REGISTEREDNAME");
			item2Mock.Setup(m => m.TradeName).Returns("ITEM2_TRADENAME");
			item2Mock.Setup(m => m.Temperatures).Returns(temperaturesMock.Object);
			item2Mock.Setup(m => m.ContainerNumbers).Returns(new List<ZString>() { "ITEM2_CONTAINERNUMBERS1", "ITEM2_CONTAINERNUMBERS2" });
			item2Mock.Setup(m => m.ItemGrossWeightInKGM).Returns(6m);
			item2Mock.Setup(m => m.ItemNetWeightInKGM).Returns(3m);
			item2Mock.Setup(m => m.StatisticalQtyUnit).Returns("ITEM2_STATISTICALQTYUNIT");
			item2Mock.Setup(m => m.StatisticalQty).Returns(1m);
			item2Mock.Setup(m => m.SupplementaryQty).Returns(10m);
			item2Mock.Setup(m => m.OriginCountry).Returns("ITEM2_ORIGINCOUNTRY");
			item2Mock.Setup(m => m.Packaging).Returns(new List<IPackaging>() { iPackaging1Mock.Object, iPackaging2Mock.Object });
			item2Mock.Setup(m => m.UsedGoods).Returns(true);
			item2Mock.Setup(m => m.GeneticallyModified).Returns(true);
			item2Mock.Setup(m => m.SupplementaryQtyUnit).Returns("BDU");
			item2Mock.Setup(m => m.LineDutyTaxFees).Returns(new List<IDutyTaxFee>() { dutyTaxFee1Mock.Object });
			item2Mock.Setup(m => m.Products).Returns(new List<IProduct>() { iProduct1Mock.Object, iProduct2Mock.Object });

			var iInvoice1Mock = new Mock<IInvoice>();
			iInvoice1Mock.Setup(m => m.InvoiceDate).Returns(new ZDateTime(2018, 11, 7));
			iInvoice1Mock.Setup(m => m.InvoiceNumber).Returns("IINVOICE1_INVOICENUMBER");

			var iInvoice2Mock = new Mock<IInvoice>();
			iInvoice2Mock.Setup(m => m.InvoiceDate).Returns(new ZDateTime(2015, 4, 2));
			iInvoice2Mock.Setup(m => m.InvoiceNumber).Returns("IINVOICE2_INVOICENUMBER");

			var notifyParty1Mock = new Mock<IOrganisation>();
			notifyParty1Mock.Setup(m => m.Name).Returns("NOTIFYPARTY1_NAME");
			notifyParty1Mock.Setup(m => m.CustomsClientCode).Returns("NOTIFYPARTY1_CUSTOMSCLIENTCODE");
			notifyParty1Mock.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1Mock.Object, iCommunication2Mock.Object });

			var notifyParty2Mock = new Mock<IOrganisation>();
			notifyParty2Mock.Setup(m => m.Name).Returns("NOTIFYPARTY2_NAME");
			notifyParty2Mock.Setup(m => m.CustomsClientCode).Returns("NOTIFYPARTY2_CUSTOMSCLIENTCODE");
			notifyParty2Mock.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1Mock.Object, iCommunication2Mock.Object });

			var stuffingEstablishment1Mock = new Mock<IOrganisation>();
			stuffingEstablishment1Mock.Setup(m => m.Name).Returns("STUFFINGESTABLISHMENT1_NAME");
			stuffingEstablishment1Mock.Setup(m => m.City).Returns("STUFFINGESTABLISHMENT1_CITY");
			stuffingEstablishment1Mock.Setup(m => m.CountryCode).Returns("STUFFINGESTABLISHMENT1_COUNTRYCODE");
			stuffingEstablishment1Mock.Setup(m => m.CountryRegion).Returns("STUFFINGESTABLISHMENT1_COUNTRYREGION");
			stuffingEstablishment1Mock.Setup(m => m.Address).Returns("STUFFINGESTABLISHMENT1_ADDRESS");
			stuffingEstablishment1Mock.Setup(m => m.PostCode).Returns("123456789");

			var stuffingEstablishment2Mock = new Mock<IOrganisation>();
			stuffingEstablishment2Mock.Setup(m => m.Name).Returns("STUFFINGESTABLISHMENT2_NAME");
			stuffingEstablishment2Mock.Setup(m => m.City).Returns("STUFFINGESTABLISHMENT2_CITY");
			stuffingEstablishment2Mock.Setup(m => m.CountryCode).Returns("STUFFINGESTABLISHMENT2_COUNTRYCODE");
			stuffingEstablishment2Mock.Setup(m => m.CountryRegion).Returns("STUFFINGESTABLISHMENT2_COUNTRYREGION");
			stuffingEstablishment2Mock.Setup(m => m.Address).Returns("STUFFINGESTABLISHMENT2_ADDRESS");
			stuffingEstablishment2Mock.Setup(m => m.PostCode).Returns("123456789");

			var deliverToPartyMock = new Mock<IOrganisation>();
			deliverToPartyMock.Setup(m => m.Name).Returns("DELIVERTOPARTY_NAME");
			deliverToPartyMock.Setup(m => m.City).Returns("DELIVERTOPARTY_CITY");
			deliverToPartyMock.Setup(m => m.CountryCode).Returns("DELIVERTOPARTY_COUNTRYCODE");
			deliverToPartyMock.Setup(m => m.CountryRegion).Returns("DELIVERTOPARTY_COUNTRYREGION");
			deliverToPartyMock.Setup(m => m.Address).Returns("DELIVERTOPARTY_ADDRESS");
			deliverToPartyMock.Setup(m => m.PostCode).Returns("123456789");

			var goodsShipmentMock = new Mock<IGoodsShipment>();
			goodsShipmentMock.Setup(m => m.NatureOfTransaction).Returns("GOODSSHIPMENT_NATUREOFTRANSACTION");
			goodsShipmentMock.Setup(m => m.LocationOfGoods).Returns("GOODSSHIPMENT_LOCATIONOFGOODS");
			goodsShipmentMock.Setup(m => m.PortOfLoading).Returns("GOODSSHIPMENT_PORTOFLOADING");
			goodsShipmentMock.Setup(m => m.PortOfDischarge).Returns("GOODSSHIPMENT_PORTOFDISCHARGE");
			goodsShipmentMock.Setup(m => m.Items).Returns(new List<IGoodsItems>() { item1Mock.Object, item2Mock.Object });
			goodsShipmentMock.Setup(m => m.Invoices).Returns(new List<IInvoice>() { iInvoice1Mock.Object, iInvoice2Mock.Object });
			goodsShipmentMock.Setup(m => m.NotifyParties).Returns(new List<IOrganisationSimple>() { notifyParty1Mock.Object, notifyParty2Mock.Object });
			goodsShipmentMock.Setup(m => m.StuffingEstablishments).Returns(new List<IOrganisation>() { stuffingEstablishment1Mock.Object, stuffingEstablishment2Mock.Object });
			goodsShipmentMock.Setup(m => m.CustomsControlledArea).Returns("GOODSSHIPMENT_CUSTOMSCONTROLLEDAREA");
			goodsShipmentMock.Setup(m => m.NotifyPartyCodes).Returns(new ZString[] { "1111A", "ARRYN" });
			goodsShipmentMock.Setup(m => m.DeliverToParty).Returns(deliverToPartyMock.Object);

			var iExportDeclarationMock = new Mock<IExportDeclaration>();
			iExportDeclarationMock.Setup(m => m.TSWReferenceNumber).Returns("ENTRY12345");
			iExportDeclarationMock.Setup(m => m.SenderReferenceNumber).Returns("C00001165");
			iExportDeclarationMock.Setup(m => m.IsSea).Returns(true);
			iExportDeclarationMock.Setup(m => m.CraftName).Returns("CRAFTNAME");
			iExportDeclarationMock.Setup(m => m.FlightNo).Returns("FLIGHTNO");
			iExportDeclarationMock.Setup(m => m.LloydsNo).Returns("LLOYDSNO");
			iExportDeclarationMock.Setup(m => m.VoyageNo).Returns("VOYAGENO123");
			iExportDeclarationMock.Setup(m => m.Carrier).Returns(carrierMock.Object);
			iExportDeclarationMock.Setup(m => m.IsSea).Returns(true);
			iExportDeclarationMock.Setup(m => m.IsAir).Returns(false);
			iExportDeclarationMock.Setup(m => m.IsMail).Returns(false);
			iExportDeclarationMock.Setup(m => m.TotalGrossWeightInKGM).Returns(0.2m);
			iExportDeclarationMock.Setup(m => m.SubmitterCode).Returns("SUBMITTERCODE");
			iExportDeclarationMock.Setup(m => m.HandlingInformation).Returns("HANDLINGINFORMATION");
			iExportDeclarationMock.Setup(m => m.BrokerCode).Returns("BROKERCODE");
			iExportDeclarationMock.Setup(m => m.PaymentType).Returns("PAYMENTTYPE");
			iExportDeclarationMock.Setup(m => m.DateOfExport).Returns(new ZDateTime(2018, 11, 10));
			iExportDeclarationMock.Setup(m => m.IsContainerised).Returns(true);
			iExportDeclarationMock.Setup(m => m.PreviousDocumentNo).Returns("PREVIOUSDOCUMENTNO");
			iExportDeclarationMock.Setup(m => m.PreviousDocumentType).Returns("PREVIOUSDOCUMENTTYPE");
			iExportDeclarationMock.Setup(m => m.MessageType).Returns("E41");
			iExportDeclarationMock.Setup(m => m.IsCompletionEntry).Returns(true);
			iExportDeclarationMock.Setup(m => m.AdditionalInformation).Returns(iAdditionalInformationMock.Object);
			iExportDeclarationMock.Setup(m => m.Permits).Returns(new List<ZString>() { "PERMIT1", "PERMIT2" });
			iExportDeclarationMock.Setup(m => m.ExchangeRates).Returns(new List<ICurrency>() { exchangeRate1Mock.Object, exchangeRate2Mock.Object });
			iExportDeclarationMock.Setup(m => m.Packaging).Returns(new List<IPackaging>() { packaging1Mock.Object, packaging2Mock.Object });
			iExportDeclarationMock.Setup(m => m.Equipment).Returns(new List<ITransportEquipment>() { container1Mock.Object, container2Mock.Object });
			iExportDeclarationMock.Setup(m => m.Exporter).Returns(exporterMock.Object);
			iExportDeclarationMock.Setup(m => m.Importer).Returns(importerMock.Object);
			iExportDeclarationMock.Setup(m => m.DutyTaxFees).Returns(new List<IDutyTaxFee>() { dutyTaxFee1Mock.Object, dutyTaxFee2Mock.Object });
			iExportDeclarationMock.Setup(m => m.Declarant).Returns(declarantMock.Object);
			iExportDeclarationMock.Setup(m => m.Permits).Returns(new List<ZString>() { "PERMIT1", "PERMIT2" });
			iExportDeclarationMock.Setup(m => m.OtherReferencedDocuments).Returns(new List<IOtherInfo>() { iOtherInfo1Mock.Object, iOtherInfo2Mock.Object });
			iExportDeclarationMock.Setup(m => m.OtherInfoCodes).Returns(new List<IOtherInfo>() { iOtherInfo1Mock.Object, iOtherInfo2Mock.Object });
			iExportDeclarationMock.Setup(m => m.GoodsShipment).Returns(goodsShipmentMock.Object);

			var houseBillChild1Mock = new Mock<IAssociatedTransportDocument>();
			PopulateChildHouseBillMock(houseBillChild1Mock, "1");
			var houseBillChild2Mock = new Mock<IAssociatedTransportDocument>();
			PopulateChildHouseBillMock(houseBillChild2Mock, "2");
			var houseBillChild3Mock = new Mock<IAssociatedTransportDocument>();
			PopulateChildHouseBillMock(houseBillChild3Mock, "3");
			var houseBillChild4Mock = new Mock<IAssociatedTransportDocument>();
			PopulateChildHouseBillMock(houseBillChild4Mock, "4");
			var houseBillChild5Mock = new Mock<IAssociatedTransportDocument>();
			PopulateChildHouseBillMock(houseBillChild5Mock, "5");
			var houseBillChild6Mock = new Mock<IAssociatedTransportDocument>();
			PopulateChildHouseBillMock(houseBillChild6Mock, "6");
			var houseBillChild7Mock = new Mock<IAssociatedTransportDocument>();
			PopulateChildHouseBillMock(houseBillChild7Mock, "7");
			var houseBillChild8Mock = new Mock<IAssociatedTransportDocument>();
			PopulateChildHouseBillMock(houseBillChild8Mock, "8");

			var houseBill1Mock = new Mock<IMasterBillTransportDocument>();
			PopulateHouseBillMock(houseBill1Mock, "1", houseBillChild1Mock.Object.PK, houseBillChild2Mock.Object.PK);
			var houseBill2Mock = new Mock<IMasterBillTransportDocument>();
			PopulateHouseBillMock(houseBill2Mock, "2", houseBillChild3Mock.Object.PK, houseBillChild4Mock.Object.PK);
			var houseBill3Mock = new Mock<IMasterBillTransportDocument>();
			PopulateHouseBillMock(houseBill3Mock, "3", houseBillChild5Mock.Object.PK, houseBillChild6Mock.Object.PK);
			var houseBill4Mock = new Mock<IMasterBillTransportDocument>();
			PopulateHouseBillMock(houseBill4Mock, "4", houseBillChild7Mock.Object.PK, houseBillChild8Mock.Object.PK);

			iExportDeclarationMock.Setup(m => m.AllBills).Returns(new List<IAssociatedTransportDocument>()
			{ houseBill1Mock.Object, houseBill2Mock.Object, houseBill3Mock.Object, houseBill4Mock.Object });

			var masterBill1Mock = new Mock<IMasterBillTransportDocument>();
			PopulateMasterBillMock(masterBill1Mock, "1", houseBill1Mock.Object.PK, houseBill2Mock.Object.PK);
			var masterBill2Mock = new Mock<IMasterBillTransportDocument>();
			PopulateMasterBillMock(masterBill2Mock, "2", houseBill3Mock.Object.PK, houseBill4Mock.Object.PK);

			iExportDeclarationMock.Setup(m => m.MasterBills).Returns(new List<IMasterBillTransportDocument>()
			{ masterBill1Mock.Object, masterBill2Mock.Object });
			ex1Builder = new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original);
			embeddedResourceRetriever = new EmbeddedResourceRetriever();

			AssertMultilineASCIIEquals(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TestEX1MessageCompletion.txt")), new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Completion).GetXMLMessage());
			iExportDeclarationMock.Setup(m => m.Carrier);
			var unexpectedXML = @"<Carrier>
    <Name>CARRIER_NAME</Name>
  </Carrier>";
			AssertNotContains(unexpectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Completion).GetXMLMessage());
			iExportDeclarationMock.Setup(m => m.MessageType).Returns("E40");
			unexpectedXML = @"<DutyTaxFee>
    <TypeCode>CUD</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""AUD"">9</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>TOT</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""AUD"">9</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>";
			AssertNotContains(unexpectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Completion).GetXMLMessage());
		}

		public void TestEX1MessageReplace()
		{
			var carrierMock = new Mock<IOrganisationSimple>();
			carrierMock.Setup(m => m.Name).Returns("CARRIER_NAME");

			var supportingDocument1Mock = new Mock<ITSWAttachment>();
			supportingDocument1Mock.Setup(m => m.DocType).Returns("CDO");
			supportingDocument1Mock.Setup(m => m.FileName).Returns("TESTFILE1.PDF");

			var supportingDocument2Mock = new Mock<ITSWAttachment>();
			supportingDocument2Mock.Setup(m => m.DocType).Returns("INV");
			supportingDocument2Mock.Setup(m => m.FileName).Returns("TESTFILE1.XLSX");

			var iAdditionalInformationMock = new Mock<IAdditionalInformation>();
			iAdditionalInformationMock.Setup(m => m.AdditionalStatementText).Returns("ADDITIONALINFORMATION_ADDITIONALSTATEMENTTEXT");
			iAdditionalInformationMock.Setup(m => m.ManualOverrideText).Returns("ADDITIONALINFORMATION_MANUALOVERRIDETEXT");
			iAdditionalInformationMock.Setup(m => m.FreeText).Returns("ADDITIONALINFORMATION_FREETEXT");
			iAdditionalInformationMock.Setup(m => m.SupportingDocuments).Returns(new List<ITSWAttachment>() { supportingDocument1Mock.Object, supportingDocument2Mock.Object });

			var exchangeRate1Mock = new Mock<ICurrency>();
			exchangeRate1Mock.Setup(m => m.ExchangeRateIndicator).Returns("EXCHANGERATE1_EXCHANGERATEINDICATOR");
			exchangeRate1Mock.Setup(m => m.CurrencyCode).Returns("NZD");
			exchangeRate1Mock.Setup(m => m.ExchangeRate).Returns(2.123);

			var exchangeRate2Mock = new Mock<ICurrency>();
			exchangeRate2Mock.Setup(m => m.ExchangeRateIndicator).Returns("EXCHANGERATE2_EXCHANGERATEINDICATOR");
			exchangeRate2Mock.Setup(m => m.CurrencyCode).Returns("NZD");
			exchangeRate2Mock.Setup(m => m.ExchangeRate).Returns(3.123);

			var packaging1Mock = new Mock<IPackaging>();
			packaging1Mock.Setup(m => m.NumberOfPackages).Returns(3);
			packaging1Mock.Setup(m => m.MessageSequence).Returns(4);
			packaging1Mock.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			packaging1Mock.Setup(m => m.PackageType).Returns("PACKAGING1_PACKAGETYPE");

			var packaging2Mock = new Mock<IPackaging>();
			packaging2Mock.Setup(m => m.NumberOfPackages).Returns(3);
			packaging2Mock.Setup(m => m.MessageSequence).Returns(4);
			packaging2Mock.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			packaging2Mock.Setup(m => m.PackageType).Returns("PACKAGING2_PACKAGETYPE");

			var container1Mock = new Mock<ITransportEquipment>();
			container1Mock.Setup(m => m.MessageSequence).Returns(1);
			container1Mock.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			container1Mock.Setup(m => m.IsPallet).Returns(true);
			container1Mock.Setup(m => m.Size).Returns("CONTAINER1_SIZE");
			container1Mock.Setup(m => m.Status).Returns("CONTAINER1_STATUS");
			container1Mock.Setup(m => m.ContainerNumber).Returns("CONTAINER1_CONTAINERNUMBER");
			container1Mock.Setup(m => m.SealNumbers).Returns(new List<ZString>() { "CONTAINER1_SEALNUMBERS1", "CONTAINER1_SEALNUMBERS2" });
			container1Mock.Setup(m => m.RelatedPackages).Returns(new List<ZGuid>() { ZGuid.NewZGuid(), ZGuid.NewZGuid() });

			var container2Mock = new Mock<ITransportEquipment>();
			container2Mock.Setup(m => m.MessageSequence).Returns(2);
			container2Mock.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			container2Mock.Setup(m => m.IsPallet).Returns(true);
			container2Mock.Setup(m => m.Size).Returns("CONTAINER2_SIZE");
			container2Mock.Setup(m => m.Status).Returns("CONTAINER2_STATUS");
			container2Mock.Setup(m => m.ContainerNumber).Returns("CONTAINER2_CONTAINERNUMBER");
			container2Mock.Setup(m => m.SealNumbers).Returns(new List<ZString>() { "CONTAINER2_SEALNUMBERS1", "CONTAINER2_SEALNUMBERS2" });
			container2Mock.Setup(m => m.RelatedPackages).Returns(new List<ZGuid>() { ZGuid.NewZGuid(), ZGuid.NewZGuid() });

			var exporterMock = new Mock<IOrganisation>();
			exporterMock.Setup(m => m.CustomsClientCode).Returns("EXPORTER_CUSTOMSCLIENTCODE");

			var importerMock = new Mock<IOrganisation>();
			importerMock.Setup(m => m.Name).Returns("IMPORTER_NAME");
			importerMock.Setup(m => m.City).Returns("IMPORTER_CITY");
			importerMock.Setup(m => m.CountryCode).Returns("IMPORTER_COUNTRYCODE");
			importerMock.Setup(m => m.CountryRegion).Returns("IMPORTER_COUNTRYREGION");
			importerMock.Setup(m => m.Address).Returns("IMPORTER_ADDRESS");
			importerMock.Setup(m => m.PostCode).Returns("123456789");

			var dutyTaxFee1Mock = new Mock<IDutyTaxFee>();
			dutyTaxFee1Mock.Setup(m => m.DutyTaxFeeType).Returns("CUD");
			dutyTaxFee1Mock.Setup(m => m.CurrencyCode).Returns("DUTYTAXFEE1_CURRENCYCODE");
			dutyTaxFee1Mock.Setup(m => m.Amount).Returns(9m);

			var dutyTaxFee2Mock = new Mock<IDutyTaxFee>();
			dutyTaxFee2Mock.Setup(m => m.DutyTaxFeeType).Returns("TOT");
			dutyTaxFee2Mock.Setup(m => m.CurrencyCode).Returns("DUTYTAXFEE2_CURRENCYCODE");
			dutyTaxFee2Mock.Setup(m => m.Amount).Returns(9m);

			var iCommunication1Mock = new Mock<ICommunication>();
			iCommunication1Mock.Setup(m => m.ContactDetail).Returns("ICOMMUNICATION1_CONTACTDETAIL");
			iCommunication1Mock.Setup(m => m.ContactType).Returns("EM");

			var iCommunication2Mock = new Mock<ICommunication>();
			iCommunication2Mock.Setup(m => m.ContactDetail).Returns("ICOMMUNICATION1_CONTACTDETAIL");
			iCommunication2Mock.Setup(m => m.ContactType).Returns("EM");

			var declarantMock = new Mock<IDeclarant>();
			declarantMock.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1Mock.Object, iCommunication2Mock.Object });
			declarantMock.Setup(m => m.DeclarantID).Returns("DECLARANT_DECLARANTID");

			var iOtherInfo1Mock = new Mock<IOtherInfo>();
			iOtherInfo1Mock.Setup(m => m.Code).Returns("IOTHERINFO1_CODE");
			iOtherInfo1Mock.Setup(m => m.Data).Returns("IOTHERINFO1_DATA");

			var iOtherInfo2Mock = new Mock<IOtherInfo>();
			iOtherInfo2Mock.Setup(m => m.Code).Returns("IOTHERINFO2_CODE");
			iOtherInfo2Mock.Setup(m => m.Data).Returns("IOTHERINFO2_DATA");

			var iClassification1Mock = new Mock<IClassification>();
			iClassification1Mock.Setup(m => m.Classification).Returns("ICLASSIFICATION1_CLASSIFICATION");
			iClassification1Mock.Setup(m => m.ClassificationTypeCode).Returns("ICLASSIFICATION1_CLASSIFICATIONTYPECODE");

			var iClassification2Mock = new Mock<IClassification>();
			iClassification2Mock.Setup(m => m.Classification).Returns("ICLASSIFICATION2_CLASSIFICATION");
			iClassification2Mock.Setup(m => m.ClassificationTypeCode).Returns("ICLASSIFICATION2_CLASSIFICATIONTYPECODE");

			var temperaturesMock = new Mock<ITemperatureRequirements>();
			temperaturesMock.Setup(m => m.StorageTemp).Returns(6m);
			temperaturesMock.Setup(m => m.MinStorageTemp).Returns(6m);
			temperaturesMock.Setup(m => m.MaxStorageTemp).Returns(6m);

			var iPackaging1Mock = new Mock<IPackaging>();
			iPackaging1Mock.Setup(m => m.ShippingMarks).Returns("IPACKAGING1_SHIPPINGMARKS");
			iPackaging1Mock.Setup(m => m.NumberOfPackages).Returns(3);
			iPackaging1Mock.Setup(m => m.PackageType).Returns("IPACKAGING1_SHIPPINGMARKS");
			iPackaging1Mock.Setup(m => m.PackageVolumeInMTQ).Returns(7m);

			var iPackaging2Mock = new Mock<IPackaging>();
			iPackaging2Mock.Setup(m => m.ShippingMarks).Returns("IPACKAGING2_SHIPPINGMARKS");
			iPackaging2Mock.Setup(m => m.NumberOfPackages).Returns(3);
			iPackaging2Mock.Setup(m => m.PackageType).Returns("IPACKAGING2_SHIPPINGMARKS");
			iPackaging2Mock.Setup(m => m.PackageVolumeInMTQ).Returns(7m);

			var iProduct1Mock = new Mock<IProduct>();
			iProduct1Mock.Setup(m => m.Id).Returns("IPRODUCT1_ID");
			iProduct1Mock.Setup(m => m.IdType).Returns("IPRODUCT1_IDTYPE");

			var iProduct2Mock = new Mock<IProduct>();
			iProduct2Mock.Setup(m => m.Id).Returns("IPRODUCT2_ID");
			iProduct2Mock.Setup(m => m.IdType).Returns("IPRODUCT2_IDTYPE");

			var item1Mock = new Mock<IGoodsItems>();
			item1Mock.Setup(m => m.Permits).Returns(new List<ZString>() { "ITEM1_PERMITS1", "ITEM2_PERMITS2" });
			item1Mock.Setup(m => m.ProhibitedCodes).Returns(new List<ZString>() { "ITEM1_PROHIBITEDCODES1", "ITEM1_PROHIBITEDCODES2" });
			item1Mock.Setup(m => m.OtherInfoCodes).Returns(new List<IOtherInfo>() { iOtherInfo1Mock.Object, iOtherInfo2Mock.Object });
			item1Mock.Setup(m => m.GoodsDescription).Returns("ITEM1_GOODSDESCRIPTION");
			item1Mock.Setup(m => m.ValueInForeignCurrency).Returns(6m);
			item1Mock.Setup(m => m.ForeignCurrencyCode).Returns("ITEM1_FOREIGNCURRENCYCODE");
			item1Mock.Setup(m => m.Classifications).Returns(new List<IClassification>() { iClassification1Mock.Object, iClassification2Mock.Object });
			item1Mock.Setup(m => m.BrandName).Returns("ITEM1_BRANDNAME");
			item1Mock.Setup(m => m.CommonName).Returns("ITEM1_COMMONNAME");
			item1Mock.Setup(m => m.RegisteredName).Returns("ITEM1_REGISTEREDNAME");
			item1Mock.Setup(m => m.TradeName).Returns("ITEM1_TRADENAME");
			item1Mock.Setup(m => m.Temperatures).Returns(temperaturesMock.Object);
			item1Mock.Setup(m => m.ContainerNumbers).Returns(new List<ZString>() { "ITEM1_CONTAINERNUMBERS1", "ITEM1_CONTAINERNUMBERS2" });
			item1Mock.Setup(m => m.ItemGrossWeightInKGM).Returns(6m);
			item1Mock.Setup(m => m.ItemNetWeightInKGM).Returns(3m);
			item1Mock.Setup(m => m.StatisticalQtyUnit).Returns("ITEM1_STATISTICALQTYUNIT");
			item1Mock.Setup(m => m.StatisticalQty).Returns(1m);
			item1Mock.Setup(m => m.SupplementaryQty).Returns(10m);
			item1Mock.Setup(m => m.OriginCountry).Returns("ITEM1_ORIGINCOUNTRY");
			item1Mock.Setup(m => m.Packaging).Returns(new List<IPackaging>() { iPackaging1Mock.Object, iPackaging2Mock.Object });
			item1Mock.Setup(m => m.UsedGoods).Returns(true);
			item1Mock.Setup(m => m.GeneticallyModified).Returns(true);
			item1Mock.Setup(m => m.SupplementaryQtyUnit).Returns("BDU");
			item1Mock.Setup(m => m.LineDutyTaxFees).Returns(new List<IDutyTaxFee>() { dutyTaxFee1Mock.Object });
			item1Mock.Setup(m => m.Products).Returns(new List<IProduct>() { iProduct1Mock.Object, iProduct2Mock.Object });

			var item2Mock = new Mock<IGoodsItems>();
			item2Mock.Setup(m => m.Permits).Returns(new List<ZString>() { "ITEM2_PERMITS1", "ITEM2_PERMITS2" });
			item2Mock.Setup(m => m.ProhibitedCodes).Returns(new List<ZString>() { "ITEM2_PROHIBITEDCODES1", "ITEM2_PROHIBITEDCODES2" });
			item2Mock.Setup(m => m.OtherInfoCodes).Returns(new List<IOtherInfo>() { iOtherInfo1Mock.Object, iOtherInfo2Mock.Object });
			item2Mock.Setup(m => m.GoodsDescription).Returns("ITEM2_GOODSDESCRIPTION");
			item2Mock.Setup(m => m.ValueInForeignCurrency).Returns(6m);
			item2Mock.Setup(m => m.ForeignCurrencyCode).Returns("ITEM2_FOREIGNCURRENCYCODE");
			item2Mock.Setup(m => m.Classifications).Returns(new List<IClassification>() { iClassification1Mock.Object, iClassification2Mock.Object });
			item2Mock.Setup(m => m.BrandName).Returns("ITEM2_BRANDNAME");
			item2Mock.Setup(m => m.CommonName).Returns("ITEM2_COMMONNAME");
			item2Mock.Setup(m => m.RegisteredName).Returns("ITEM2_REGISTEREDNAME");
			item2Mock.Setup(m => m.TradeName).Returns("ITEM2_TRADENAME");
			item2Mock.Setup(m => m.Temperatures).Returns(temperaturesMock.Object);
			item2Mock.Setup(m => m.ContainerNumbers).Returns(new List<ZString>() { "ITEM2_CONTAINERNUMBERS1", "ITEM2_CONTAINERNUMBERS2" });
			item2Mock.Setup(m => m.ItemGrossWeightInKGM).Returns(6m);
			item2Mock.Setup(m => m.ItemNetWeightInKGM).Returns(3m);
			item2Mock.Setup(m => m.StatisticalQtyUnit).Returns("ITEM2_STATISTICALQTYUNIT");
			item2Mock.Setup(m => m.StatisticalQty).Returns(1m);
			item2Mock.Setup(m => m.SupplementaryQty).Returns(10m);
			item2Mock.Setup(m => m.OriginCountry).Returns("ITEM2_ORIGINCOUNTRY");
			item2Mock.Setup(m => m.Packaging).Returns(new List<IPackaging>() { iPackaging1Mock.Object, iPackaging2Mock.Object });
			item2Mock.Setup(m => m.UsedGoods).Returns(true);
			item2Mock.Setup(m => m.GeneticallyModified).Returns(true);
			item2Mock.Setup(m => m.SupplementaryQtyUnit).Returns("BDU");
			item2Mock.Setup(m => m.LineDutyTaxFees).Returns(new List<IDutyTaxFee>() { dutyTaxFee1Mock.Object });
			item2Mock.Setup(m => m.Products).Returns(new List<IProduct>() { iProduct1Mock.Object, iProduct2Mock.Object });

			var iInvoice1Mock = new Mock<IInvoice>();
			iInvoice1Mock.Setup(m => m.InvoiceDate).Returns(new ZDateTime(2018, 11, 7));
			iInvoice1Mock.Setup(m => m.InvoiceNumber).Returns("IINVOICE1_INVOICENUMBER");

			var iInvoice2Mock = new Mock<IInvoice>();
			iInvoice2Mock.Setup(m => m.InvoiceDate).Returns(new ZDateTime(2015, 4, 2));
			iInvoice2Mock.Setup(m => m.InvoiceNumber).Returns("IINVOICE2_INVOICENUMBER");

			var notifyParty1Mock = new Mock<IOrganisation>();
			notifyParty1Mock.Setup(m => m.Name).Returns("NOTIFYPARTY1_NAME");
			notifyParty1Mock.Setup(m => m.CustomsClientCode).Returns("NOTIFYPARTY1_CUSTOMSCLIENTCODE");
			notifyParty1Mock.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1Mock.Object, iCommunication2Mock.Object });

			var notifyParty2Mock = new Mock<IOrganisation>();
			notifyParty2Mock.Setup(m => m.Name).Returns("NOTIFYPARTY2_NAME");
			notifyParty2Mock.Setup(m => m.CustomsClientCode).Returns("NOTIFYPARTY2_CUSTOMSCLIENTCODE");
			notifyParty2Mock.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1Mock.Object, iCommunication2Mock.Object });

			var stuffingEstablishment1Mock = new Mock<IOrganisation>();
			stuffingEstablishment1Mock.Setup(m => m.Name).Returns("STUFFINGESTABLISHMENT1_NAME");
			stuffingEstablishment1Mock.Setup(m => m.City).Returns("STUFFINGESTABLISHMENT1_CITY");
			stuffingEstablishment1Mock.Setup(m => m.CountryCode).Returns("STUFFINGESTABLISHMENT1_COUNTRYCODE");
			stuffingEstablishment1Mock.Setup(m => m.CountryRegion).Returns("STUFFINGESTABLISHMENT1_COUNTRYREGION");
			stuffingEstablishment1Mock.Setup(m => m.Address).Returns("STUFFINGESTABLISHMENT1_ADDRESS");
			stuffingEstablishment1Mock.Setup(m => m.PostCode).Returns("123456789");

			var stuffingEstablishment2Mock = new Mock<IOrganisation>();
			stuffingEstablishment2Mock.Setup(m => m.Name).Returns("STUFFINGESTABLISHMENT2_NAME");
			stuffingEstablishment2Mock.Setup(m => m.City).Returns("STUFFINGESTABLISHMENT2_CITY");
			stuffingEstablishment2Mock.Setup(m => m.CountryCode).Returns("STUFFINGESTABLISHMENT2_COUNTRYCODE");
			stuffingEstablishment2Mock.Setup(m => m.CountryRegion).Returns("STUFFINGESTABLISHMENT2_COUNTRYREGION");
			stuffingEstablishment2Mock.Setup(m => m.Address).Returns("STUFFINGESTABLISHMENT2_ADDRESS");
			stuffingEstablishment2Mock.Setup(m => m.PostCode).Returns("123456789");

			var deliverToPartyMock = new Mock<IOrganisation>();
			deliverToPartyMock.Setup(m => m.Name).Returns("DELIVERTOPARTY_NAME");
			deliverToPartyMock.Setup(m => m.City).Returns("DELIVERTOPARTY_CITY");
			deliverToPartyMock.Setup(m => m.CountryCode).Returns("DELIVERTOPARTY_COUNTRYCODE");
			deliverToPartyMock.Setup(m => m.CountryRegion).Returns("DELIVERTOPARTY_COUNTRYREGION");
			deliverToPartyMock.Setup(m => m.Address).Returns("DELIVERTOPARTY_ADDRESS");
			deliverToPartyMock.Setup(m => m.PostCode).Returns("123456789");

			var goodsShipmentMock = new Mock<IGoodsShipment>();
			goodsShipmentMock.Setup(m => m.NatureOfTransaction).Returns("GOODSSHIPMENT_NATUREOFTRANSACTION");
			goodsShipmentMock.Setup(m => m.LocationOfGoods).Returns("GOODSSHIPMENT_LOCATIONOFGOODS");
			goodsShipmentMock.Setup(m => m.PortOfLoading).Returns("GOODSSHIPMENT_PORTOFLOADING");
			goodsShipmentMock.Setup(m => m.PortOfDischarge).Returns("GOODSSHIPMENT_PORTOFDISCHARGE");
			goodsShipmentMock.Setup(m => m.Items).Returns(new List<IGoodsItems>() { item1Mock.Object, item2Mock.Object });
			goodsShipmentMock.Setup(m => m.Invoices).Returns(new List<IInvoice>() { iInvoice1Mock.Object, iInvoice2Mock.Object });
			goodsShipmentMock.Setup(m => m.NotifyParties).Returns(new List<IOrganisationSimple>() { notifyParty1Mock.Object, notifyParty2Mock.Object });
			goodsShipmentMock.Setup(m => m.StuffingEstablishments).Returns(new List<IOrganisation>() { stuffingEstablishment1Mock.Object, stuffingEstablishment2Mock.Object });
			goodsShipmentMock.Setup(m => m.CustomsControlledArea).Returns("GOODSSHIPMENT_CUSTOMSCONTROLLEDAREA");
			goodsShipmentMock.Setup(m => m.NotifyPartyCodes).Returns(new ZString[] { "1111A", "ARRYN" });
			goodsShipmentMock.Setup(m => m.DeliverToParty).Returns(deliverToPartyMock.Object);

			var iExportDeclarationMock = new Mock<IExportDeclaration>();
			iExportDeclarationMock.Setup(m => m.TSWReferenceNumber).Returns("ENTRY12345");
			iExportDeclarationMock.Setup(m => m.SenderReferenceNumber).Returns("C00001165");
			iExportDeclarationMock.Setup(m => m.IsSea).Returns(true);
			iExportDeclarationMock.Setup(m => m.CraftName).Returns("CRAFTNAME");
			iExportDeclarationMock.Setup(m => m.FlightNo).Returns("FLIGHTNO");
			iExportDeclarationMock.Setup(m => m.LloydsNo).Returns("LLOYDSNO");
			iExportDeclarationMock.Setup(m => m.VoyageNo).Returns("VOYAGENO123");
			iExportDeclarationMock.Setup(m => m.Carrier).Returns(carrierMock.Object);
			iExportDeclarationMock.Setup(m => m.IsSea).Returns(true);
			iExportDeclarationMock.Setup(m => m.IsAir).Returns(false);
			iExportDeclarationMock.Setup(m => m.IsMail).Returns(false);
			iExportDeclarationMock.Setup(m => m.TotalGrossWeightInKGM).Returns(0.2m);
			iExportDeclarationMock.Setup(m => m.SubmitterCode).Returns("SUBMITTERCODE");
			iExportDeclarationMock.Setup(m => m.HandlingInformation).Returns("HANDLINGINFORMATION");
			iExportDeclarationMock.Setup(m => m.BrokerCode).Returns("BROKERCODE");
			iExportDeclarationMock.Setup(m => m.PaymentType).Returns("PAYMENTTYPE");
			iExportDeclarationMock.Setup(m => m.DateOfExport).Returns(new ZDateTime(2018, 11, 10));
			iExportDeclarationMock.Setup(m => m.IsContainerised).Returns(true);
			iExportDeclarationMock.Setup(m => m.PreviousDocumentNo).Returns("PREVIOUSDOCUMENTNO");
			iExportDeclarationMock.Setup(m => m.PreviousDocumentType).Returns("PREVIOUSDOCUMENTTYPE");
			iExportDeclarationMock.Setup(m => m.MessageType).Returns("E41");
			iExportDeclarationMock.Setup(m => m.IsCompletionEntry).Returns(true);
			iExportDeclarationMock.Setup(m => m.AdditionalInformation).Returns(iAdditionalInformationMock.Object);
			iExportDeclarationMock.Setup(m => m.Permits).Returns(new List<ZString>() { "PERMIT1", "PERMIT2" });
			iExportDeclarationMock.Setup(m => m.ExchangeRates).Returns(new List<ICurrency>() { exchangeRate1Mock.Object, exchangeRate2Mock.Object });
			iExportDeclarationMock.Setup(m => m.Packaging).Returns(new List<IPackaging>() { packaging1Mock.Object, packaging2Mock.Object });
			iExportDeclarationMock.Setup(m => m.Equipment).Returns(new List<ITransportEquipment>() { container1Mock.Object, container2Mock.Object });
			iExportDeclarationMock.Setup(m => m.Exporter).Returns(exporterMock.Object);
			iExportDeclarationMock.Setup(m => m.Importer).Returns(importerMock.Object);
			iExportDeclarationMock.Setup(m => m.DutyTaxFees).Returns(new List<IDutyTaxFee>() { dutyTaxFee1Mock.Object, dutyTaxFee2Mock.Object });
			iExportDeclarationMock.Setup(m => m.Declarant).Returns(declarantMock.Object);
			iExportDeclarationMock.Setup(m => m.Permits).Returns(new List<ZString>() { "PERMIT1", "PERMIT2" });
			iExportDeclarationMock.Setup(m => m.OtherReferencedDocuments).Returns(new List<IOtherInfo>() { iOtherInfo1Mock.Object, iOtherInfo2Mock.Object });
			iExportDeclarationMock.Setup(m => m.OtherInfoCodes).Returns(new List<IOtherInfo>() { iOtherInfo1Mock.Object, iOtherInfo2Mock.Object });
			iExportDeclarationMock.Setup(m => m.GoodsShipment).Returns(goodsShipmentMock.Object);

			var houseBillChild1Mock = new Mock<IAssociatedTransportDocument>();
			PopulateChildHouseBillMock(houseBillChild1Mock, "1");
			var houseBillChild2Mock = new Mock<IAssociatedTransportDocument>();
			PopulateChildHouseBillMock(houseBillChild2Mock, "2");
			var houseBillChild3Mock = new Mock<IAssociatedTransportDocument>();
			PopulateChildHouseBillMock(houseBillChild3Mock, "3");
			var houseBillChild4Mock = new Mock<IAssociatedTransportDocument>();
			PopulateChildHouseBillMock(houseBillChild4Mock, "4");
			var houseBillChild5Mock = new Mock<IAssociatedTransportDocument>();
			PopulateChildHouseBillMock(houseBillChild5Mock, "5");
			var houseBillChild6Mock = new Mock<IAssociatedTransportDocument>();
			PopulateChildHouseBillMock(houseBillChild6Mock, "6");
			var houseBillChild7Mock = new Mock<IAssociatedTransportDocument>();
			PopulateChildHouseBillMock(houseBillChild7Mock, "7");
			var houseBillChild8Mock = new Mock<IAssociatedTransportDocument>();
			PopulateChildHouseBillMock(houseBillChild8Mock, "8");

			var houseBill1Mock = new Mock<IMasterBillTransportDocument>();
			PopulateHouseBillMock(houseBill1Mock, "1", houseBillChild1Mock.Object.PK, houseBillChild2Mock.Object.PK);
			var houseBill2Mock = new Mock<IMasterBillTransportDocument>();
			PopulateHouseBillMock(houseBill2Mock, "2", houseBillChild3Mock.Object.PK, houseBillChild4Mock.Object.PK);
			var houseBill3Mock = new Mock<IMasterBillTransportDocument>();
			PopulateHouseBillMock(houseBill3Mock, "3", houseBillChild5Mock.Object.PK, houseBillChild6Mock.Object.PK);
			var houseBill4Mock = new Mock<IMasterBillTransportDocument>();
			PopulateHouseBillMock(houseBill4Mock, "4", houseBillChild7Mock.Object.PK, houseBillChild8Mock.Object.PK);

			iExportDeclarationMock.Setup(m => m.AllBills).Returns(new List<IAssociatedTransportDocument>()
			{ houseBill1Mock.Object, houseBill2Mock.Object, houseBill3Mock.Object, houseBill4Mock.Object });

			var masterBill1Mock = new Mock<IMasterBillTransportDocument>();
			PopulateMasterBillMock(masterBill1Mock, "1", houseBill1Mock.Object.PK, houseBill2Mock.Object.PK);
			var masterBill2Mock = new Mock<IMasterBillTransportDocument>();
			PopulateMasterBillMock(masterBill2Mock, "2", houseBill3Mock.Object.PK, houseBill4Mock.Object.PK);

			iExportDeclarationMock.Setup(m => m.MasterBills).Returns(new List<IMasterBillTransportDocument>()
			{ masterBill1Mock.Object, masterBill2Mock.Object });
			ex1Builder = new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original);
			embeddedResourceRetriever = new EmbeddedResourceRetriever();

			AssertMultilineASCIIEquals(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TestEX1MessageReplace.txt")), new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Replace).GetXMLMessage());
			iExportDeclarationMock.Setup(m => m.Carrier);
			var unexpectedXML = @"<Carrier>
    <Name>CARRIER_NAME</Name>
  </Carrier>";
			AssertNotContains(unexpectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Replace).GetXMLMessage());
			iExportDeclarationMock.Setup(m => m.MessageType).Returns("E40");
			unexpectedXML = @"<DutyTaxFee>
    <TypeCode>CUD</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""AUD"">9</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>TOT</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""AUD"">9</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>";
			AssertNotContains(unexpectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Replace).GetXMLMessage());
		}

		public void TestEX1MessageCancel()
		{
			SetUpMocks();
			ex1Builder = new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Cancel);
			AssertMultilineASCIIEquals(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TestEX1MessageCancel.txt")), ex1Builder.GetXMLMessage());
		}

		public void TestTotalGrossWeightIsRounded()
		{
			SetUpMocks();
			iExportDeclarationMock.Setup(m => m.TotalGrossWeightInKGM).Returns(5.6m);
			AssertContains(@"<TotalGrossMassMeasure unitCode=""KGM"">6</TotalGrossMassMeasure>", ex1Builder.GetXMLMessage());
		}

		public void TestGrossWeightOnLineIsRounded()
		{
			SetUpMocks();
			item1Mock.Reset();
			item1Mock.Setup(m => m.ItemGrossWeightInKGM).Returns(2.12345);
			AssertContains(@"<GrossMassMeasure unitCode=""KGM"">2.123</GrossMassMeasure>", ex1Builder.GetXMLMessage());
		}

		public void TestGoodsDescriptionMaxLength()
		{
			SetUpMocks();
			var expectedResult = "<Description>2007 FORD FPV PURSUIT   VIN  6FPAAAJGCM7K62816  I, THE UNDERSIGNED, BEING THE IMPORTER OF THE VEHICLE DECLARED IN THIS IMPORT ENTRY, UNDERTAKE THAT SHOULD I SELL OR OTHERWISE DISPOSE OF THE VEHICLE WITHIN 2 YEARS FROM THE DATE OF IMPORTATION I WILL I</Description>";
			var unexpectedResult = "2007 FORD FPV PURSUIT   VIN  6FPAAAJGCM7K62816  I, THE UNDERSIGNED, BEING THE IMPORTER OF THE VEHICLE DECLARED IN THIS IMPORT ENTRY, UNDERTAKE THAT SHOULD I SELL OR OTHERWISE DISPOSE OF THE VEHICLE WITHIN 2 YEARS FROM THE DATE OF IMPORTATION I WILL IMMEDIATELY PAY NZ CUSTOMS THE SUM OF $1743.15, OR ANY LESSER AMOUNT THAT MAY BE REQUIRED.   .................................................... NAME AND SIGNATURE OF IMPORTER";
			item1Mock.Reset();
			item1Mock.Setup(m => m.GoodsDescription).Returns(unexpectedResult);
			AssertContains(expectedResult, ex1Builder.GetXMLMessage());
		}

		public void TestPopulateReferenceNo()
		{
			SetUpMocks();
			var expectedXML = "<ID>ENTRY12345</ID>";
			//Setup is Original
			AssertNotContains(expectedXML, ex1Builder.GetXMLMessage());
			AssertContains(expectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Replace).GetXMLMessage());
			AssertNotContains(expectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Completion).GetXMLMessage());
		}

		public void TestPopulateMessageType()
		{
			SetUpMocks();
			AssertContains("<TypeCode>E41</TypeCode>", ex1Builder.GetXMLMessage());
		}

		public void TestPopulateSendersRef()
		{
			SetUpMocks();
			AssertContains("<FunctionalReferenceID>C00001165</FunctionalReferenceID>", ex1Builder.GetXMLMessage());
		}

		public void TestPopulateTransType()
		{
			SetUpMocks();
			AssertContains("<FunctionCode>9</FunctionCode>", ex1Builder.GetXMLMessage());
			AssertContains("<FunctionCode>1</FunctionCode>", new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Cancel).GetXMLMessage());
			AssertContains("<FunctionCode>4</FunctionCode>", new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Change).GetXMLMessage());
			AssertContains("<FunctionCode>22</FunctionCode>", new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Completion).GetXMLMessage());
			AssertContains("<FunctionCode />", new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.None).GetXMLMessage());
			AssertContains("<FunctionCode>5</FunctionCode>", new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Replace).GetXMLMessage());
		}

		public void TestPopulateGrossWeight()
		{
			SetUpMocks();
			var expectedXML = @"<TotalGrossMassMeasure unitCode=""KGM"">1</TotalGrossMassMeasure>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
			AssertNotContains(expectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Cancel).GetXMLMessage());
		}

		public void TestPopulateSubmitter()
		{
			SetUpMocks();
			var expectedXML = @"<Submitter>
    <ID>SUBMITTERCODE</ID>
  </Submitter>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
		}

		public void TestPopulateAdditionalDocs()
		{
			SetUpMocks();
			var expectedXML = @"<AdditionalDocument>
    <ID>PERMIT1</ID>
    <TypeCode>PER</TypeCode>
  </AdditionalDocument>
  <AdditionalDocument>
    <ID>PERMIT2</ID>
    <TypeCode>PER</TypeCode>
  </AdditionalDocument>
  <AdditionalDocument>
    <ID>IOTHERINFO1_DATA</ID>
    <TypeCode>IOTHERINFO1_CODE</TypeCode>
  </AdditionalDocument>
  <AdditionalDocument>
    <ID>IOTHERINFO2_DATA</ID>
    <TypeCode>IOTHERINFO2_CODE</TypeCode>
  </AdditionalDocument>
  <AdditionalDocument>
    <CategoryCode>CDO</CategoryCode>
    <ImageBinaryObject mimeCode=""application/pdf"" filename=""TESTFILE1.PDF"">ATTACHED</ImageBinaryObject>
  </AdditionalDocument>
  <AdditionalDocument>
    <CategoryCode>INV</CategoryCode>
    <ImageBinaryObject mimeCode=""application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"" filename=""TESTFILE1.XLSX"">ATTACHED</ImageBinaryObject>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
		}

		public void TestPopulateAdditionalDocsWithNullAdditionalInformation()
		{
			SetUpMocks();
			var expectedXML = @"<AdditionalDocument>
    <ID>PERMIT1</ID>
    <TypeCode>PER</TypeCode>
  </AdditionalDocument>
  <AdditionalDocument>
    <ID>PERMIT2</ID>
    <TypeCode>PER</TypeCode>
  </AdditionalDocument>
  <AdditionalDocument>
    <ID>IOTHERINFO1_DATA</ID>
    <TypeCode>IOTHERINFO1_CODE</TypeCode>
  </AdditionalDocument>
  <AdditionalDocument>
    <ID>IOTHERINFO2_DATA</ID>
    <TypeCode>IOTHERINFO2_CODE</TypeCode>
  </AdditionalDocument>
  <AdditionalDocument>
    <CategoryCode>SUPPORTINGDOCUMENT1_DOCTYPE</CategoryCode>
    <ImageBinaryObject mimeCode=""application/pdf"" filename=""SUPPORTINGDOCUMENT1_FILENAME"">ATTACHED</ImageBinaryObject>
  </AdditionalDocument>
  <AdditionalDocument>
    <CategoryCode>SUPPORTINGDOCUMENT2_DOCTYPE</CategoryCode>
    <ImageBinaryObject mimeCode=""application/pdf"" filename=""SUPPORTINGDOCUMENT2_FILENAME"">ATTACHED</ImageBinaryObject>
  </AdditionalDocument>";
			AssertNotContains(expectedXML, ex1Builder.GetXMLMessage());
		}

		public void TestPopulateAdditionalInfoOriginal()
		{
			SetUpMocks();
			AssertContains(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TestEX1PopulateAdditionalInfo.txt")), ex1Builder.GetXMLMessage());

			iAdditionalInformationMock.Setup(m => m.FreeText).Returns(ZString.Empty);
			var unexpectedXML = @"<AdditionalInformation>
    <Content>ADDITIONALINFORMATION_FREETEXT</Content>
  </AdditionalInformation>";
			AssertNotContains(unexpectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original).GetXMLMessage());
			iAdditionalInformationMock.Setup(m => m.ManualOverrideText).Returns(ZString.Empty);
			unexpectedXML = @"<AdditionalInformation>
    <RequestOverrideCode>Y</RequestOverrideCode>
    <StatementDescription>ADDITIONALINFORMATION_MANUALOVERRIDETEXT</StatementDescription>
    <StatementTypeCode>ALP</StatementTypeCode>
  </AdditionalInformation>";
			AssertNotContains(unexpectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original).GetXMLMessage());
			iExportDeclarationMock.Setup(m => m.HandlingInformation).Returns(ZString.Empty);
			unexpectedXML = @"<AdditionalInformation>
    <StatementDescription>HANDLINGINFORMATION</StatementDescription>
    <StatementTypeCode>HAN</StatementTypeCode>
  </AdditionalInformation>";
			AssertNotContains(unexpectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original).GetXMLMessage());
		}

		public void TestPopulateAdditionalInfoReplace()
		{
			SetUpMocks();
			AssertContains(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TestEX1PopulateAdditionalInfo.txt")), new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Replace).GetXMLMessage());

			iAdditionalInformationMock.Setup(m => m.FreeText).Returns(ZString.Empty);
			var unexpectedXML = @"<AdditionalInformation>
    <Content>ADDITIONALINFORMATION_FREETEXT</Content>
  </AdditionalInformation>";
			AssertNotContains(unexpectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Replace).GetXMLMessage());
			iAdditionalInformationMock.Setup(m => m.ManualOverrideText).Returns(ZString.Empty);
			unexpectedXML = @"<AdditionalInformation>
    <RequestOverrideCode>Y</RequestOverrideCode>
    <StatementDescription>ADDITIONALINFORMATION_MANUALOVERRIDETEXT</StatementDescription>
    <StatementTypeCode>ALP</StatementTypeCode>
  </AdditionalInformation>";
			AssertNotContains(unexpectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Replace).GetXMLMessage());
			iExportDeclarationMock.Setup(m => m.HandlingInformation).Returns(ZString.Empty);
			unexpectedXML = @"<AdditionalInformation>
    <StatementDescription>HANDLINGINFORMATION</StatementDescription>
    <StatementTypeCode>HAN</StatementTypeCode>
  </AdditionalInformation>";
			AssertNotContains(unexpectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Replace).GetXMLMessage());
		}

		public void TestPopulateAdditionalInfoCompletion()
		{
			SetUpMocks();
			AssertContains(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TestEX1PopulateAdditionalInfo.txt")), new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Completion).GetXMLMessage());
			iAdditionalInformationMock.Setup(m => m.FreeText).Returns(ZString.Empty);
			var unexpectedXML = @"<AdditionalInformation>
    <Content>ADDITIONALINFORMATION_FREETEXT</Content>
  </AdditionalInformation>";
			AssertNotContains(unexpectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Completion).GetXMLMessage());
			iAdditionalInformationMock.Setup(m => m.ManualOverrideText).Returns(ZString.Empty);
			unexpectedXML = @"<AdditionalInformation>
    <RequestOverrideCode>Y</RequestOverrideCode>
    <StatementDescription>ADDITIONALINFORMATION_MANUALOVERRIDETEXT</StatementDescription>
    <StatementTypeCode>ALP</StatementTypeCode>
  </AdditionalInformation>";
			AssertNotContains(unexpectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Completion).GetXMLMessage());
			iExportDeclarationMock.Setup(m => m.HandlingInformation).Returns(ZString.Empty);
			unexpectedXML = @"<AdditionalInformation>
    <StatementDescription>HANDLINGINFORMATION</StatementDescription>
    <StatementTypeCode>HAN</StatementTypeCode>
  </AdditionalInformation>";
			AssertNotContains(unexpectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Completion).GetXMLMessage());
		}

		public void TestPopulateAdditionalInfoCancel()
		{
			SetUpMocks();
			AssertContains(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TestEX1PopulateAdditionalInfoCancel.txt")), new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Cancel).GetXMLMessage());
		}

		public void TestPopulateAdditionalInfoWithNullAdditionalInformation()
		{
			var carrierMock = new Mock<IOrganisationSimple>();
			carrierMock.Setup(m => m.Name).Returns("CARRIER_NAME");

			var supportingDocument1Mock = new Mock<ITSWAttachment>();
			supportingDocument1Mock.Setup(m => m.DocType).Returns("CDO");
			supportingDocument1Mock.Setup(m => m.FileName).Returns("TESTFILE1.PDF");

			var supportingDocument2Mock = new Mock<ITSWAttachment>();
			supportingDocument2Mock.Setup(m => m.DocType).Returns("INV");
			supportingDocument2Mock.Setup(m => m.FileName).Returns("TESTFILE1.XLSX");

			var iAdditionalInformationMock = new Mock<IAdditionalInformation>();
			iAdditionalInformationMock.Setup(m => m.AdditionalStatementText).Returns("ADDITIONALINFORMATION_ADDITIONALSTATEMENTTEXT");
			iAdditionalInformationMock.Setup(m => m.ManualOverrideText).Returns("ADDITIONALINFORMATION_MANUALOVERRIDETEXT");
			iAdditionalInformationMock.Setup(m => m.FreeText).Returns("ADDITIONALINFORMATION_FREETEXT");
			iAdditionalInformationMock.Setup(m => m.SupportingDocuments).Returns(new List<ITSWAttachment>() { supportingDocument1Mock.Object, supportingDocument2Mock.Object });

			var exchangeRate1Mock = new Mock<ICurrency>();
			exchangeRate1Mock.Setup(m => m.ExchangeRateIndicator).Returns("EXCHANGERATE1_EXCHANGERATEINDICATOR");
			exchangeRate1Mock.Setup(m => m.CurrencyCode).Returns("NZD");
			exchangeRate1Mock.Setup(m => m.ExchangeRate).Returns(2.123);

			var exchangeRate2Mock = new Mock<ICurrency>();
			exchangeRate2Mock.Setup(m => m.ExchangeRateIndicator).Returns("EXCHANGERATE2_EXCHANGERATEINDICATOR");
			exchangeRate2Mock.Setup(m => m.CurrencyCode).Returns("NZD");
			exchangeRate2Mock.Setup(m => m.ExchangeRate).Returns(3.123);

			var packaging1Mock = new Mock<IPackaging>();
			packaging1Mock.Setup(m => m.NumberOfPackages).Returns(3);
			packaging1Mock.Setup(m => m.MessageSequence).Returns(4);
			packaging1Mock.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			packaging1Mock.Setup(m => m.PackageType).Returns("PACKAGING1_PACKAGETYPE");

			var packaging2Mock = new Mock<IPackaging>();
			packaging2Mock.Setup(m => m.NumberOfPackages).Returns(3);
			packaging2Mock.Setup(m => m.MessageSequence).Returns(4);
			packaging2Mock.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			packaging2Mock.Setup(m => m.PackageType).Returns("PACKAGING2_PACKAGETYPE");

			var container1Mock = new Mock<ITransportEquipment>();
			container1Mock.Setup(m => m.MessageSequence).Returns(1);
			container1Mock.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			container1Mock.Setup(m => m.IsPallet).Returns(true);
			container1Mock.Setup(m => m.Size).Returns("CONTAINER1_SIZE");
			container1Mock.Setup(m => m.Status).Returns("CONTAINER1_STATUS");
			container1Mock.Setup(m => m.ContainerNumber).Returns("CONTAINER1_CONTAINERNUMBER");
			container1Mock.Setup(m => m.SealNumbers).Returns(new List<ZString>() { "CONTAINER1_SEALNUMBERS1", "CONTAINER1_SEALNUMBERS2" });
			container1Mock.Setup(m => m.RelatedPackages).Returns(new List<ZGuid>() { ZGuid.NewZGuid(), ZGuid.NewZGuid() });

			var container2Mock = new Mock<ITransportEquipment>();
			container2Mock.Setup(m => m.MessageSequence).Returns(2);
			container2Mock.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			container2Mock.Setup(m => m.IsPallet).Returns(true);
			container2Mock.Setup(m => m.Size).Returns("CONTAINER2_SIZE");
			container2Mock.Setup(m => m.Status).Returns("CONTAINER2_STATUS");
			container2Mock.Setup(m => m.ContainerNumber).Returns("CONTAINER2_CONTAINERNUMBER");
			container2Mock.Setup(m => m.SealNumbers).Returns(new List<ZString>() { "CONTAINER2_SEALNUMBERS1", "CONTAINER2_SEALNUMBERS2" });
			container2Mock.Setup(m => m.RelatedPackages).Returns(new List<ZGuid>() { ZGuid.NewZGuid(), ZGuid.NewZGuid() });

			var exporterMock = new Mock<IOrganisation>();
			exporterMock.Setup(m => m.CustomsClientCode).Returns("EXPORTER_CUSTOMSCLIENTCODE");
			var importerMock = new Mock<IOrganisation>();
			importerMock.Setup(m => m.Name).Returns("IMPORTER_NAME");
			importerMock.Setup(m => m.City).Returns("IMPORTER_CITY");
			importerMock.Setup(m => m.CountryCode).Returns("IMPORTER_COUNTRYCODE");
			importerMock.Setup(m => m.CountryRegion).Returns("IMPORTER_COUNTRYREGION");
			importerMock.Setup(m => m.Address).Returns("IMPORTER_ADDRESS");
			importerMock.Setup(m => m.PostCode).Returns("123456789");

			var dutyTaxFee1Mock = new Mock<IDutyTaxFee>();
			dutyTaxFee1Mock.Setup(m => m.DutyTaxFeeType).Returns("CUD");
			dutyTaxFee1Mock.Setup(m => m.CurrencyCode).Returns("DUTYTAXFEE1_CURRENCYCODE");
			dutyTaxFee1Mock.Setup(m => m.Amount).Returns(9m);

			var dutyTaxFee2Mock = new Mock<IDutyTaxFee>();
			dutyTaxFee2Mock.Setup(m => m.DutyTaxFeeType).Returns("TOT");
			dutyTaxFee2Mock.Setup(m => m.CurrencyCode).Returns("DUTYTAXFEE2_CURRENCYCODE");
			dutyTaxFee2Mock.Setup(m => m.Amount).Returns(9m);

			var iCommunication1Mock = new Mock<ICommunication>();
			iCommunication1Mock.Setup(m => m.ContactDetail).Returns("ICOMMUNICATION1_CONTACTDETAIL");
			iCommunication1Mock.Setup(m => m.ContactType).Returns("EM");

			var iCommunication2Mock = new Mock<ICommunication>();
			iCommunication2Mock.Setup(m => m.ContactDetail).Returns("ICOMMUNICATION1_CONTACTDETAIL");
			iCommunication2Mock.Setup(m => m.ContactType).Returns("EM");

			var declarantMock = new Mock<IDeclarant>();
			declarantMock.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1Mock.Object, iCommunication2Mock.Object });
			declarantMock.Setup(m => m.DeclarantID).Returns("DECLARANT_DECLARANTID");

			var iOtherInfo1Mock = new Mock<IOtherInfo>();
			iOtherInfo1Mock.Setup(m => m.Code).Returns("IOTHERINFO1_CODE");
			iOtherInfo1Mock.Setup(m => m.Data).Returns("IOTHERINFO1_DATA");

			var iOtherInfo2Mock = new Mock<IOtherInfo>();
			iOtherInfo2Mock.Setup(m => m.Code).Returns("IOTHERINFO2_CODE");
			iOtherInfo2Mock.Setup(m => m.Data).Returns("IOTHERINFO2_DATA");

			var iClassification1Mock = new Mock<IClassification>();
			iClassification1Mock.Setup(m => m.Classification).Returns("ICLASSIFICATION1_CLASSIFICATION");
			iClassification1Mock.Setup(m => m.ClassificationTypeCode).Returns("ICLASSIFICATION1_CLASSIFICATIONTYPECODE");

			var iClassification2Mock = new Mock<IClassification>();
			iClassification2Mock.Setup(m => m.Classification).Returns("ICLASSIFICATION2_CLASSIFICATION");
			iClassification2Mock.Setup(m => m.ClassificationTypeCode).Returns("ICLASSIFICATION2_CLASSIFICATIONTYPECODE");

			var temperaturesMock = new Mock<ITemperatureRequirements>();
			temperaturesMock.Setup(m => m.StorageTemp).Returns(6m);
			temperaturesMock.Setup(m => m.MinStorageTemp).Returns(6m);
			temperaturesMock.Setup(m => m.MaxStorageTemp).Returns(6m);

			var iPackaging1Mock = new Mock<IPackaging>();
			iPackaging1Mock.Setup(m => m.ShippingMarks).Returns("IPACKAGING1_SHIPPINGMARKS");
			iPackaging1Mock.Setup(m => m.NumberOfPackages).Returns(3);
			iPackaging1Mock.Setup(m => m.PackageType).Returns("IPACKAGING1_SHIPPINGMARKS");
			iPackaging1Mock.Setup(m => m.PackageVolumeInMTQ).Returns(7m);

			var iPackaging2Mock = new Mock<IPackaging>();
			iPackaging2Mock.Setup(m => m.ShippingMarks).Returns("IPACKAGING2_SHIPPINGMARKS");
			iPackaging2Mock.Setup(m => m.NumberOfPackages).Returns(3);
			iPackaging2Mock.Setup(m => m.PackageType).Returns("IPACKAGING2_SHIPPINGMARKS");
			iPackaging2Mock.Setup(m => m.PackageVolumeInMTQ).Returns(7m);

			var iProduct1Mock = new Mock<IProduct>();
			iProduct1Mock.Setup(m => m.Id).Returns("IPRODUCT1_ID");
			iProduct1Mock.Setup(m => m.IdType).Returns("IPRODUCT1_IDTYPE");

			var iProduct2Mock = new Mock<IProduct>();
			iProduct2Mock.Setup(m => m.Id).Returns("IPRODUCT2_ID");
			iProduct2Mock.Setup(m => m.IdType).Returns("IPRODUCT2_IDTYPE");

			var item1Mock = new Mock<IGoodsItems>();
			item1Mock.Setup(m => m.Permits).Returns(new List<ZString>() { "ITEM1_PERMITS1", "ITEM2_PERMITS2" });
			item1Mock.Setup(m => m.ProhibitedCodes).Returns(new List<ZString>() { "ITEM1_PROHIBITEDCODES1", "ITEM1_PROHIBITEDCODES2" });
			item1Mock.Setup(m => m.OtherInfoCodes).Returns(new List<IOtherInfo>() { iOtherInfo1Mock.Object, iOtherInfo2Mock.Object });
			item1Mock.Setup(m => m.GoodsDescription).Returns("ITEM1_GOODSDESCRIPTION");
			item1Mock.Setup(m => m.ValueInForeignCurrency).Returns(6m);
			item1Mock.Setup(m => m.ForeignCurrencyCode).Returns("ITEM1_FOREIGNCURRENCYCODE");
			item1Mock.Setup(m => m.Classifications).Returns(new List<IClassification>() { iClassification1Mock.Object, iClassification2Mock.Object });
			item1Mock.Setup(m => m.BrandName).Returns("ITEM1_BRANDNAME");
			item1Mock.Setup(m => m.CommonName).Returns("ITEM1_COMMONNAME");
			item1Mock.Setup(m => m.RegisteredName).Returns("ITEM1_REGISTEREDNAME");
			item1Mock.Setup(m => m.TradeName).Returns("ITEM1_TRADENAME");
			item1Mock.Setup(m => m.Temperatures).Returns(temperaturesMock.Object);
			item1Mock.Setup(m => m.ContainerNumbers).Returns(new List<ZString>() { "ITEM1_CONTAINERNUMBERS1", "ITEM1_CONTAINERNUMBERS2" });
			item1Mock.Setup(m => m.ItemGrossWeightInKGM).Returns(6m);
			item1Mock.Setup(m => m.ItemNetWeightInKGM).Returns(3m);
			item1Mock.Setup(m => m.StatisticalQtyUnit).Returns("ITEM1_STATISTICALQTYUNIT");
			item1Mock.Setup(m => m.StatisticalQty).Returns(1m);
			item1Mock.Setup(m => m.SupplementaryQty).Returns(10m);
			item1Mock.Setup(m => m.OriginCountry).Returns("ITEM1_ORIGINCOUNTRY");
			item1Mock.Setup(m => m.Packaging).Returns(new List<IPackaging>() { iPackaging1Mock.Object, iPackaging2Mock.Object });
			item1Mock.Setup(m => m.UsedGoods).Returns(true);
			item1Mock.Setup(m => m.GeneticallyModified).Returns(true);
			item1Mock.Setup(m => m.SupplementaryQtyUnit).Returns("BDU");
			item1Mock.Setup(m => m.LineDutyTaxFees).Returns(new List<IDutyTaxFee>() { dutyTaxFee1Mock.Object });
			item1Mock.Setup(m => m.Products).Returns(new List<IProduct>() { iProduct1Mock.Object, iProduct2Mock.Object });

			var item2Mock = new Mock<IGoodsItems>();
			item2Mock.Setup(m => m.Permits).Returns(new List<ZString>() { "ITEM2_PERMITS1", "ITEM2_PERMITS2" });
			item2Mock.Setup(m => m.ProhibitedCodes).Returns(new List<ZString>() { "ITEM2_PROHIBITEDCODES1", "ITEM2_PROHIBITEDCODES2" });
			item2Mock.Setup(m => m.OtherInfoCodes).Returns(new List<IOtherInfo>() { iOtherInfo1Mock.Object, iOtherInfo2Mock.Object });
			item2Mock.Setup(m => m.GoodsDescription).Returns("ITEM2_GOODSDESCRIPTION");
			item2Mock.Setup(m => m.ValueInForeignCurrency).Returns(6m);
			item2Mock.Setup(m => m.ForeignCurrencyCode).Returns("ITEM2_FOREIGNCURRENCYCODE");
			item2Mock.Setup(m => m.Classifications).Returns(new List<IClassification>() { iClassification1Mock.Object, iClassification2Mock.Object });
			item2Mock.Setup(m => m.BrandName).Returns("ITEM2_BRANDNAME");
			item2Mock.Setup(m => m.CommonName).Returns("ITEM2_COMMONNAME");
			item2Mock.Setup(m => m.RegisteredName).Returns("ITEM2_REGISTEREDNAME");
			item2Mock.Setup(m => m.TradeName).Returns("ITEM2_TRADENAME");
			item2Mock.Setup(m => m.Temperatures).Returns(temperaturesMock.Object);
			item2Mock.Setup(m => m.ContainerNumbers).Returns(new List<ZString>() { "ITEM2_CONTAINERNUMBERS1", "ITEM2_CONTAINERNUMBERS2" });
			item2Mock.Setup(m => m.ItemGrossWeightInKGM).Returns(6m);
			item2Mock.Setup(m => m.ItemNetWeightInKGM).Returns(3m);
			item2Mock.Setup(m => m.StatisticalQtyUnit).Returns("ITEM2_STATISTICALQTYUNIT");
			item2Mock.Setup(m => m.StatisticalQty).Returns(1m);
			item2Mock.Setup(m => m.SupplementaryQty).Returns(10m);
			item2Mock.Setup(m => m.OriginCountry).Returns("ITEM2_ORIGINCOUNTRY");
			item2Mock.Setup(m => m.Packaging).Returns(new List<IPackaging>() { iPackaging1Mock.Object, iPackaging2Mock.Object });
			item2Mock.Setup(m => m.UsedGoods).Returns(true);
			item2Mock.Setup(m => m.GeneticallyModified).Returns(true);
			item2Mock.Setup(m => m.SupplementaryQtyUnit).Returns("BDU");
			item2Mock.Setup(m => m.LineDutyTaxFees).Returns(new List<IDutyTaxFee>() { dutyTaxFee1Mock.Object });
			item2Mock.Setup(m => m.Products).Returns(new List<IProduct>() { iProduct1Mock.Object, iProduct2Mock.Object });

			var iInvoice1Mock = new Mock<IInvoice>();
			iInvoice1Mock.Setup(m => m.InvoiceDate).Returns(new ZDateTime(2018, 11, 7));
			iInvoice1Mock.Setup(m => m.InvoiceNumber).Returns("IINVOICE1_INVOICENUMBER");

			var iInvoice2Mock = new Mock<IInvoice>();
			iInvoice2Mock.Setup(m => m.InvoiceDate).Returns(new ZDateTime(2015, 4, 2));
			iInvoice2Mock.Setup(m => m.InvoiceNumber).Returns("IINVOICE2_INVOICENUMBER");

			var notifyParty1Mock = new Mock<IOrganisation>();
			notifyParty1Mock.Setup(m => m.Name).Returns("NOTIFYPARTY1_NAME");
			notifyParty1Mock.Setup(m => m.CustomsClientCode).Returns("NOTIFYPARTY1_CUSTOMSCLIENTCODE");
			notifyParty1Mock.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1Mock.Object, iCommunication2Mock.Object });

			var notifyParty2Mock = new Mock<IOrganisation>();
			notifyParty2Mock.Setup(m => m.Name).Returns("NOTIFYPARTY2_NAME");
			notifyParty2Mock.Setup(m => m.CustomsClientCode).Returns("NOTIFYPARTY2_CUSTOMSCLIENTCODE");
			notifyParty2Mock.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1Mock.Object, iCommunication2Mock.Object });

			var stuffingEstablishment1Mock = new Mock<IOrganisation>();
			stuffingEstablishment1Mock.Setup(m => m.Name).Returns("STUFFINGESTABLISHMENT1_NAME");
			stuffingEstablishment1Mock.Setup(m => m.City).Returns("STUFFINGESTABLISHMENT1_CITY");
			stuffingEstablishment1Mock.Setup(m => m.CountryCode).Returns("STUFFINGESTABLISHMENT1_COUNTRYCODE");
			stuffingEstablishment1Mock.Setup(m => m.CountryRegion).Returns("STUFFINGESTABLISHMENT1_COUNTRYREGION");
			stuffingEstablishment1Mock.Setup(m => m.Address).Returns("STUFFINGESTABLISHMENT1_ADDRESS");
			stuffingEstablishment1Mock.Setup(m => m.PostCode).Returns("123456789");

			var stuffingEstablishment2Mock = new Mock<IOrganisation>();
			stuffingEstablishment2Mock.Setup(m => m.Name).Returns("STUFFINGESTABLISHMENT2_NAME");
			stuffingEstablishment2Mock.Setup(m => m.City).Returns("STUFFINGESTABLISHMENT2_CITY");
			stuffingEstablishment2Mock.Setup(m => m.CountryCode).Returns("STUFFINGESTABLISHMENT2_COUNTRYCODE");
			stuffingEstablishment2Mock.Setup(m => m.CountryRegion).Returns("STUFFINGESTABLISHMENT2_COUNTRYREGION");
			stuffingEstablishment2Mock.Setup(m => m.Address).Returns("STUFFINGESTABLISHMENT2_ADDRESS");
			stuffingEstablishment2Mock.Setup(m => m.PostCode).Returns("123456789");

			var deliverToPartyMock = new Mock<IOrganisation>();
			deliverToPartyMock.Setup(m => m.Name).Returns("DELIVERTOPARTY_NAME");
			deliverToPartyMock.Setup(m => m.City).Returns("DELIVERTOPARTY_CITY");
			deliverToPartyMock.Setup(m => m.CountryCode).Returns("DELIVERTOPARTY_COUNTRYCODE");
			deliverToPartyMock.Setup(m => m.CountryRegion).Returns("DELIVERTOPARTY_COUNTRYREGION");
			deliverToPartyMock.Setup(m => m.Address).Returns("DELIVERTOPARTY_ADDRESS");
			deliverToPartyMock.Setup(m => m.PostCode).Returns("123456789");

			var goodsShipmentMock = new Mock<IGoodsShipment>();
			goodsShipmentMock.Setup(m => m.NatureOfTransaction).Returns("GOODSSHIPMENT_NATUREOFTRANSACTION");
			goodsShipmentMock.Setup(m => m.LocationOfGoods).Returns("GOODSSHIPMENT_LOCATIONOFGOODS");
			goodsShipmentMock.Setup(m => m.PortOfLoading).Returns("GOODSSHIPMENT_PORTOFLOADING");
			goodsShipmentMock.Setup(m => m.PortOfDischarge).Returns("GOODSSHIPMENT_PORTOFDISCHARGE");
			goodsShipmentMock.Setup(m => m.Items).Returns(new List<IGoodsItems>() { item1Mock.Object, item2Mock.Object });
			goodsShipmentMock.Setup(m => m.Invoices).Returns(new List<IInvoice>() { iInvoice1Mock.Object, iInvoice2Mock.Object });
			goodsShipmentMock.Setup(m => m.NotifyParties).Returns(new List<IOrganisationSimple>() { notifyParty1Mock.Object, notifyParty2Mock.Object });
			goodsShipmentMock.Setup(m => m.StuffingEstablishments).Returns(new List<IOrganisation>() { stuffingEstablishment1Mock.Object, stuffingEstablishment2Mock.Object });
			goodsShipmentMock.Setup(m => m.CustomsControlledArea).Returns("GOODSSHIPMENT_CUSTOMSCONTROLLEDAREA");
			goodsShipmentMock.Setup(m => m.NotifyPartyCodes).Returns(new ZString[] { "1111A", "ARRYN" });
			goodsShipmentMock.Setup(m => m.DeliverToParty).Returns(deliverToPartyMock.Object);

			var iExportDeclarationMock = new Mock<IExportDeclaration>();
			iExportDeclarationMock.Setup(m => m.TSWReferenceNumber).Returns("ENTRY12345");
			iExportDeclarationMock.Setup(m => m.SenderReferenceNumber).Returns("C00001165");
			iExportDeclarationMock.Setup(m => m.IsSea).Returns(true);
			iExportDeclarationMock.Setup(m => m.CraftName).Returns("CRAFTNAME");
			iExportDeclarationMock.Setup(m => m.FlightNo).Returns("FLIGHTNO");
			iExportDeclarationMock.Setup(m => m.LloydsNo).Returns("LLOYDSNO");
			iExportDeclarationMock.Setup(m => m.VoyageNo).Returns("VOYAGENO123");
			iExportDeclarationMock.Setup(m => m.Carrier).Returns(carrierMock.Object);
			iExportDeclarationMock.Setup(m => m.IsSea).Returns(true);
			iExportDeclarationMock.Setup(m => m.IsAir).Returns(false);
			iExportDeclarationMock.Setup(m => m.IsMail).Returns(false);
			iExportDeclarationMock.Setup(m => m.TotalGrossWeightInKGM).Returns(0.2m);
			iExportDeclarationMock.Setup(m => m.SubmitterCode).Returns("SUBMITTERCODE");
			iExportDeclarationMock.Setup(m => m.HandlingInformation).Returns("HANDLINGINFORMATION");
			iExportDeclarationMock.Setup(m => m.BrokerCode).Returns("BROKERCODE");
			iExportDeclarationMock.Setup(m => m.PaymentType).Returns("PAYMENTTYPE");
			iExportDeclarationMock.Setup(m => m.DateOfExport).Returns(new ZDateTime(2018, 11, 10));
			iExportDeclarationMock.Setup(m => m.IsContainerised).Returns(true);
			iExportDeclarationMock.Setup(m => m.PreviousDocumentNo).Returns("PREVIOUSDOCUMENTNO");
			iExportDeclarationMock.Setup(m => m.PreviousDocumentType).Returns("PREVIOUSDOCUMENTTYPE");
			iExportDeclarationMock.Setup(m => m.MessageType).Returns("E41");
			iExportDeclarationMock.Setup(m => m.IsCompletionEntry).Returns(true);
			iExportDeclarationMock.Setup(m => m.AdditionalInformation).Returns(iAdditionalInformationMock.Object);
			iExportDeclarationMock.Setup(m => m.Permits).Returns(new List<ZString>() { "PERMIT1", "PERMIT2" });
			iExportDeclarationMock.Setup(m => m.ExchangeRates).Returns(new List<ICurrency>() { exchangeRate1Mock.Object, exchangeRate2Mock.Object });
			iExportDeclarationMock.Setup(m => m.Packaging).Returns(new List<IPackaging>() { packaging1Mock.Object, packaging2Mock.Object });
			iExportDeclarationMock.Setup(m => m.Equipment).Returns(new List<ITransportEquipment>() { container1Mock.Object, container2Mock.Object });
			iExportDeclarationMock.Setup(m => m.Exporter).Returns(exporterMock.Object);
			iExportDeclarationMock.Setup(m => m.Importer).Returns(importerMock.Object);
			iExportDeclarationMock.Setup(m => m.DutyTaxFees).Returns(new List<IDutyTaxFee>() { dutyTaxFee1Mock.Object, dutyTaxFee2Mock.Object });
			iExportDeclarationMock.Setup(m => m.Declarant).Returns(declarantMock.Object);
			iExportDeclarationMock.Setup(m => m.Permits).Returns(new List<ZString>() { "PERMIT1", "PERMIT2" });
			iExportDeclarationMock.Setup(m => m.OtherReferencedDocuments).Returns(new List<IOtherInfo>() { iOtherInfo1Mock.Object, iOtherInfo2Mock.Object });
			iExportDeclarationMock.Setup(m => m.OtherInfoCodes).Returns(new List<IOtherInfo>() { iOtherInfo1Mock.Object, iOtherInfo2Mock.Object });
			iExportDeclarationMock.Setup(m => m.GoodsShipment).Returns(goodsShipmentMock.Object);

			var houseBillChild1Mock = new Mock<IAssociatedTransportDocument>();
			PopulateChildHouseBillMock(houseBillChild1Mock, "1");
			var houseBillChild2Mock = new Mock<IAssociatedTransportDocument>();
			PopulateChildHouseBillMock(houseBillChild2Mock, "2");
			var houseBillChild3Mock = new Mock<IAssociatedTransportDocument>();
			PopulateChildHouseBillMock(houseBillChild3Mock, "3");
			var houseBillChild4Mock = new Mock<IAssociatedTransportDocument>();
			PopulateChildHouseBillMock(houseBillChild4Mock, "4");
			var houseBillChild5Mock = new Mock<IAssociatedTransportDocument>();
			PopulateChildHouseBillMock(houseBillChild5Mock, "5");
			var houseBillChild6Mock = new Mock<IAssociatedTransportDocument>();
			PopulateChildHouseBillMock(houseBillChild6Mock, "6");
			var houseBillChild7Mock = new Mock<IAssociatedTransportDocument>();
			PopulateChildHouseBillMock(houseBillChild7Mock, "7");
			var houseBillChild8Mock = new Mock<IAssociatedTransportDocument>();
			PopulateChildHouseBillMock(houseBillChild8Mock, "8");

			var houseBill1Mock = new Mock<IMasterBillTransportDocument>();
			PopulateHouseBillMock(houseBill1Mock, "1", houseBillChild1Mock.Object.PK, houseBillChild2Mock.Object.PK);
			var houseBill2Mock = new Mock<IMasterBillTransportDocument>();
			PopulateHouseBillMock(houseBill2Mock, "2", houseBillChild3Mock.Object.PK, houseBillChild4Mock.Object.PK);
			var houseBill3Mock = new Mock<IMasterBillTransportDocument>();
			PopulateHouseBillMock(houseBill3Mock, "3", houseBillChild5Mock.Object.PK, houseBillChild6Mock.Object.PK);
			var houseBill4Mock = new Mock<IMasterBillTransportDocument>();
			PopulateHouseBillMock(houseBill4Mock, "4", houseBillChild7Mock.Object.PK, houseBillChild8Mock.Object.PK);

			iExportDeclarationMock.Setup(m => m.AllBills).Returns(new List<IAssociatedTransportDocument>()
			{ houseBill1Mock.Object, houseBill2Mock.Object, houseBill3Mock.Object, houseBill4Mock.Object });

			var masterBill1Mock = new Mock<IMasterBillTransportDocument>();
			PopulateMasterBillMock(masterBill1Mock, "1", houseBill1Mock.Object.PK, houseBill2Mock.Object.PK);
			var masterBill2Mock = new Mock<IMasterBillTransportDocument>();
			PopulateMasterBillMock(masterBill2Mock, "2", houseBill3Mock.Object.PK, houseBill4Mock.Object.PK);

			iExportDeclarationMock.Setup(m => m.MasterBills).Returns(new List<IMasterBillTransportDocument>()
			{ masterBill1Mock.Object, masterBill2Mock.Object });

			ex1Builder = new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original);
			embeddedResourceRetriever = new EmbeddedResourceRetriever();

			iExportDeclarationMock.Setup(m => m.AdditionalInformation);
			AssertNotContains(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TestEX1PopulateAdditionalInfo.txt")), ex1Builder.GetXMLMessage());
		}

		public void TestFreeTextInfo()
		{
			SetUpMocks();
			var expectedXML = @"<AdditionalInformation>
    <Content>ADDITIONALINFORMATION_FREETEXT</Content>
  </AdditionalInformation>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
		}

		public void TestExchangeRateIndicator()
		{
			SetUpMocks();
			var expectedXML = @"<AdditionalInformation>
    <StatementCode>EXCHANGERATE1_EXCHANGERATEINDICATOR</StatementCode>
    <StatementTypeCode>ERI</StatementTypeCode>
    <Pointer>
      <DocumentSectionCode>42A</DocumentSectionCode>
    </Pointer>
    <Pointer>
      <SequenceNumeric>1</SequenceNumeric>
      <DocumentSectionCode>40A</DocumentSectionCode>
    </Pointer>
  </AdditionalInformation>
  <AdditionalInformation>
    <StatementCode>EXCHANGERATE2_EXCHANGERATEINDICATOR</StatementCode>
    <StatementTypeCode>ERI</StatementTypeCode>
    <Pointer>
      <DocumentSectionCode>42A</DocumentSectionCode>
    </Pointer>
    <Pointer>
      <SequenceNumeric>2</SequenceNumeric>
      <DocumentSectionCode>40A</DocumentSectionCode>
    </Pointer>
  </AdditionalInformation>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
		}

		public void TestManualOverrideInfo()
		{
			SetUpMocks();
			var expectedXML = @"<AdditionalInformation>
    <RequestOverrideCode>Y</RequestOverrideCode>
    <StatementDescription>ADDITIONALINFORMATION_MANUALOVERRIDETEXT</StatementDescription>
    <StatementTypeCode>ALP</StatementTypeCode>
  </AdditionalInformation>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
		}

		public void TestChangeCancelReason()
		{
			SetUpMocks();
			var expectedXML = @"<AdditionalInformation>
    <StatementDescription>ADDITIONALINFORMATION_ADDITIONALSTATEMENTTEXT</StatementDescription>
    <StatementTypeCode>AES</StatementTypeCode>
  </AdditionalInformation>";
			AssertNotContains(expectedXML, ex1Builder.GetXMLMessage());
			AssertContains(expectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Replace).GetXMLMessage());
			AssertContains(expectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Cancel).GetXMLMessage());
		}

		public void TestOtherInfoCodes()
		{
			SetUpMocks();
			var expectedXML = @"<AdditionalInformation>
        <StatementCode>IOTHERINFO1_CODE</StatementCode>
        <StatementDescription>IOTHERINFO1_DATA</StatementDescription>
        <StatementTypeCode>OIN</StatementTypeCode>
      </AdditionalInformation>
      <AdditionalInformation>
        <StatementCode>IOTHERINFO2_CODE</StatementCode>
        <StatementDescription>IOTHERINFO2_DATA</StatementDescription>
        <StatementTypeCode>OIN</StatementTypeCode>
      </AdditionalInformation>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
			iOtherInfo1Mock.Reset();
			iOtherInfo1Mock.Setup(m => m.Data).Returns(ZString.Empty);
			AssertNotContains(@"<StatementDescription>IOTHERINFO1_DATA</StatementDescription>", new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original).GetXMLMessage());
		}

		public void TestHandlingInfo()
		{
			SetUpMocks();
			var expectedXML = @"<AdditionalInformation>
    <StatementDescription>HANDLINGINFORMATION</StatementDescription>
    <StatementTypeCode>HAN</StatementTypeCode>
  </AdditionalInformation>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
		}

		public void TestPopulateAgent()
		{
			SetUpMocks();
			var expectedXML = @"<Agent>
    <ID>BROKERCODE</ID>
    <RoleCode>CB</RoleCode>
  </Agent>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
			AssertNotContains(expectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Cancel).GetXMLMessage());
		}

		public void TestPopulateBorderTMSea()
		{
			SetUpMocks();
			var expectedXML = @"BorderTransportMeans>
    <Name>CRAFTNAME</Name>
    <ID>LLOYDSNO</ID>
    <TypeCode>1</TypeCode>
    <JourneyID>VOYAGENO123</JourneyID>
  </BorderTransportMeans>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
			AssertNotContains(expectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Cancel).GetXMLMessage());
		}

		public void TestPopulateBorderTMAir()
		{
			SetUpMocks();
			iExportDeclarationMock.Setup(m => m.IsSea).Returns(false);
			iExportDeclarationMock.Setup(m => m.IsAir).Returns(true);
			var expectedXML = @"<BorderTransportMeans>
    <Name>FLIGHTNO</Name>
    <TypeCode>4</TypeCode>
  </BorderTransportMeans>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
			AssertNotContains(expectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Cancel).GetXMLMessage());
		}

		public void TestPopulateBorderTMMail()
		{
			SetUpMocks();
			iExportDeclarationMock.Setup(m => m.IsSea).Returns(false);
			iExportDeclarationMock.Setup(m => m.IsMail).Returns(true);
			var expectedXML = @"<BorderTransportMeans>
    <TypeCode>5</TypeCode>
  </BorderTransportMeans>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
			AssertNotContains(expectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Cancel).GetXMLMessage());
		}

		public void TestFlightNoIsCapitalized()
		{
			SetUpMocks();
			iExportDeclarationMock.Setup(m => m.IsSea).Returns(false);
			iExportDeclarationMock.Setup(m => m.IsAir).Returns(true);
			iExportDeclarationMock.Setup(m => m.FlightNo).Returns("nz450");
			var expectedBorderTransportMeansXML = @"<BorderTransportMeans>
    <Name>NZ450</Name>
    <TypeCode>4</TypeCode>
  </BorderTransportMeans>";
			AssertContains("Flight Number should be in Uppercase", expectedBorderTransportMeansXML, ex1Builder.GetXMLMessage());
		}

		public void TestCraftAndVoyageNumberIsCapitalized()
		{
			SetUpMocks();
			iExportDeclarationMock.Setup(m => m.CraftName).Returns("China Lady");
			iExportDeclarationMock.Setup(m => m.DepartureDate).Returns(new ZDateTime(2025, 04, 25));
			iExportDeclarationMock.Setup(m => m.LloydsNo).Returns("5892347");
			iExportDeclarationMock.Setup(m => m.VoyageNo).Returns("43s");
			var expectedBorderTransportMeansXML = @"<BorderTransportMeans>
    <Name>CHINA LADY</Name>
    <ID>5892347</ID>
    <TypeCode>1</TypeCode>
    <JourneyID>43S</JourneyID>
  </BorderTransportMeans>";
			AssertContains(expectedBorderTransportMeansXML, ex1Builder.GetXMLMessage());
		}

		public void TestPopulateCarrier()
		{
			SetUpMocks();
			var expectedXML = @"<Carrier>
    <Name>CARRIER_NAME</Name>
  </Carrier>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
			AssertNotContains(expectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Cancel).GetXMLMessage());
		}

		public void TestPopulateCurrencies()
		{
			SetUpMocks();
			var expectedXML = @"<CurrencyExchange>
    <RateNumeric>2.12</RateNumeric>
    <CurrencyTypeCode>NZD</CurrencyTypeCode>
  </CurrencyExchange>
  <CurrencyExchange>
    <RateNumeric>3.12</RateNumeric>
    <CurrencyTypeCode>NZD</CurrencyTypeCode>
  </CurrencyExchange>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
			AssertNotContains(expectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Cancel).GetXMLMessage());
		}

		public void TestPopulateDeclarant()
		{
			SetUpMocks();
			var expectedXML = @"<Declarant>
    <ID>DECLARANT_DECLARANTID</ID>
    <Communication>
      <ID>ICOMMUNICATION1_CONTACTDETAIL</ID>
      <TypeID>EM</TypeID>
    </Communication>
    <Communication>
      <ID>ICOMMUNICATION1_CONTACTDETAIL</ID>
      <TypeID>EM</TypeID>
    </Communication>
  </Declarant>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
		}

		public void TestPopulateDutyTaxFees()
		{
			SetUpMocks();
			var expectedXML = @"<DutyTaxFee>
    <TypeCode>CUD</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""AUD"">9</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>GST</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">0</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>TOT</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""AUD"">9</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
			dutyTaxFee1Mock.Setup(m => m.DutyTaxFeeType).Returns("CUD");
			dutyTaxFee2Mock.Setup(m => m.DutyTaxFeeType).Returns("CUD");
			expectedXML = @"<DutyTaxFee>
    <TypeCode>CUD</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""AUD"">9</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>CUD</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""AUD"">9</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>";
			AssertContains(expectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original).GetXMLMessage());
			dutyTaxFee1Mock.Setup(m => m.DutyTaxFeeType).Returns("ABC");
			dutyTaxFee2Mock.Setup(m => m.DutyTaxFeeType).Returns("DEF");
			expectedXML = @"<DutyTaxFee>
    <TypeCode>ABC</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""AUD"">9</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>DEF</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""AUD"">9</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>";
			AssertContains(expectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original).GetXMLMessage());
			dutyTaxFee1Mock.Setup(m => m.DutyTaxFeeType).Returns("TOT");
			dutyTaxFee2Mock.Setup(m => m.DutyTaxFeeType).Returns("CUD");
			expectedXML = @"<DutyTaxFee>
    <TypeCode>TOT</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""AUD"">9</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>CUD</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""AUD"">9</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>";
			AssertContains(expectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original).GetXMLMessage());
		}

		public void TestHeaderDutyTaxFee()
		{
			SetUpMocks();
			var expectedXML = @"<DutyTaxFee>
          <TypeCode>CUD</TypeCode>
          <Payment>
            <TaxAssessedAmount currencyID=""AUD"">9</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
		}

		public void TestPopulateGSTForDrawback()
		{
			SetUpMocks();
			/*
			 *	GST element in DutyTaxFee must be provided for Drawback entry even if zero
			 *	assert xml generated contains GST element
			 */
			var expectedXML = @"<DutyTaxFee>
    <TypeCode>GST</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">0</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
		}

		public void TestFallbackPopulatesDutyTaxFeesForDrawback()
		{
			SetUpMocks();
			dutyTaxFee1Mock.Setup(m => m.DutyTaxFeeType).Returns("CUD");
			dutyTaxFee1Mock.Setup(m => m.CurrencyCode).Returns("NZD");
			dutyTaxFee1Mock.Setup(m => m.Amount).Returns(25.17m);
			dutyTaxFee2Mock.Setup(m => m.DutyTaxFeeType).Returns("GST");
			dutyTaxFee2Mock.Setup(m => m.CurrencyCode).Returns("NZD");
			dutyTaxFee2Mock.Setup(m => m.Amount).Returns(8.92m);
			var dutyTaxFee3Mock = new Mock<IDutyTaxFee>();
			dutyTaxFee3Mock.Setup(m => m.DutyTaxFeeType).Returns("TOT");
			dutyTaxFee3Mock.Setup(m => m.CurrencyCode).Returns("NZD");
			dutyTaxFee3Mock.Setup(m => m.Amount).Returns(34.09m);
			item1Mock.Setup(m => m.LineDutyTaxFees).Returns(new List<IDutyTaxFee>() { dutyTaxFee1Mock.Object, dutyTaxFee2Mock.Object, dutyTaxFee3Mock.Object });
			iExportDeclarationMock.Setup(m => m.DutyTaxFees).Returns(new List<IDutyTaxFee>() { dutyTaxFee1Mock.Object, dutyTaxFee2Mock.Object, dutyTaxFee3Mock.Object });
			var expectedXML = @"<DutyTaxFee>
    <TypeCode>CUD</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">25.17</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>GST</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">8.92</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>TOT</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">34.09</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>";
			AssertContains(expectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original).GetXMLMessage());
			dutyTaxFee1Mock.Setup(m => m.DutyTaxFeeType).Returns("CUD");
			dutyTaxFee1Mock.Setup(m => m.CurrencyCode).Returns("NZD");
			dutyTaxFee1Mock.Setup(m => m.Amount).Returns(0m);
			dutyTaxFee2Mock.Setup(m => m.DutyTaxFeeType).Returns("GST");
			dutyTaxFee2Mock.Setup(m => m.CurrencyCode).Returns("NZD");
			dutyTaxFee2Mock.Setup(m => m.Amount).Returns(0m);
			dutyTaxFee3Mock = new Mock<IDutyTaxFee>();
			dutyTaxFee3Mock.Setup(m => m.DutyTaxFeeType).Returns("TOT");
			dutyTaxFee3Mock.Setup(m => m.CurrencyCode).Returns("NZD");
			dutyTaxFee3Mock.Setup(m => m.Amount).Returns(0m);
			item1Mock.Setup(m => m.LineDutyTaxFees).Returns(new List<IDutyTaxFee>() { dutyTaxFee1Mock.Object, dutyTaxFee2Mock.Object, dutyTaxFee3Mock.Object });
			iExportDeclarationMock.Setup(m => m.DutyTaxFees).Returns(new List<IDutyTaxFee>() { dutyTaxFee1Mock.Object, dutyTaxFee2Mock.Object, dutyTaxFee3Mock.Object });
			expectedXML = @"<DutyTaxFee>
    <TypeCode>CUD</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">0</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>GST</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">0</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>TOT</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">0</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>";
			AssertContains("Duty (CUD), GST (GST) and Total Taxes/Fees (TOT) elements need to be present for a Drawback entry even if no charge/fee exists. i.e. values are zero", expectedXML, ex1Builder.GetXMLMessage());
		}

		public void TestPopulateExporter()
		{
			SetUpMocks();
			var expectedXML = @"<Exporter>
    <ID>EXPORTER_CUSTOMSCLIENTCODE</ID>
  </Exporter>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
			AssertNotContains(expectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Cancel).GetXMLMessage());
		}

		public void TestPopulatePackaging()
		{
			SetUpMocks();
			var expectedXML = @"<Packaging>
        <SequenceNumeric>1</SequenceNumeric>
        <MarksNumbersID>IPACKAGING1_SHIPPINGMARKS</MarksNumbersID>
        <QuantityQuantity>3</QuantityQuantity>
        <TypeCode>IPACKAGING1_SHIPPINGMARKS</TypeCode>
        <VolumeMeasure unitCode=""MTQ"">7</VolumeMeasure>
      </Packaging>
      <Packaging>
        <SequenceNumeric>2</SequenceNumeric>
        <MarksNumbersID>IPACKAGING2_SHIPPINGMARKS</MarksNumbersID>
        <QuantityQuantity>3</QuantityQuantity>
        <TypeCode>IPACKAGING2_SHIPPINGMARKS</TypeCode>
        <VolumeMeasure unitCode=""MTQ"">7</VolumeMeasure>
      </Packaging>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
			AssertNotContains(expectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Cancel).GetXMLMessage());
		}

		public void TestPopulatePreviousDocument()
		{
			SetUpMocks();
			var expectedXML = @"<PreviousDocument>
    <ID>PREVIOUSDOCUMENTNO</ID>
    <TypeCode>PREVIOUSDOCUMENTTYPE</TypeCode>
  </PreviousDocument>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
			AssertNotContains(expectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Cancel).GetXMLMessage());
			iExportDeclarationMock.Setup(m => m.IsCompletionEntry).Returns(false);
			AssertNotContains(expectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original).GetXMLMessage());
		}

		public void TestPopulateGoodsShipmentSea()
		{
			SetUpMocks();
			AssertContains(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TestEX1PopulateGoodsShipmentSea.txt")), ex1Builder.GetXMLMessage());
			goodsShipmentMock.Setup(m => m.DeliverToParty).Returns(importerMock.Object);
			var unexpectedXML = @"<DeliveryDestination>
      <Name>DELIVERTOPARTY_NAME</Name>
      <Address>
        <CityName>DELIVERTOPARTY_CITY</CityName>
        <CountryCode>DELIVERTOPARTY_COUNTRYCODE</CountryCode>
        <CountrySubDivisionName>DELIVERTOPARTY_COUNTRYREGION</CountrySubDivisionName>
        <Line>DELIVERTOPARTY_ADDRESS</Line>
        <PostcodeID>DELIVERTOPARTY_POSTCODE</PostcodeID>
      </Address>
    </DeliveryDestination>";
			AssertNotContains(unexpectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original).GetXMLMessage());
			AssertNotContains(unexpectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original).GetXMLMessage());
			iExportDeclarationMock.Setup(m => m.IsContainerised).Returns(false);
			unexpectedXML = @"<StuffingEstablishment>
      <Name>STUFFINGESTABLISHMENT1_NAME</Name>
      <Address>
        <CityName>STUFFINGESTABLISHMENT1_CITY</CityName>
        <CountryCode>STUFFINGESTABLISHMENT1_COUNTRYCODE</CountryCode>
        <CountrySubDivisionName>STUFFINGESTABLISHMENT1_COUNTRYREGION</CountrySubDivisionName>
        <Line>STUFFINGESTABLISHMENT1_ADDRESS</Line>
        <PostcodeID>123456789</PostcodeID>
      </Address>
    </StuffingEstablishment>
    <StuffingEstablishment>
      <Name>STUFFINGESTABLISHMENT2_NAME</Name>
      <Address>
        <CityName>STUFFINGESTABLISHMENT2_CITY</CityName>
        <CountryCode>STUFFINGESTABLISHMENT2_COUNTRYCODE</CountryCode>
        <CountrySubDivisionName>STUFFINGESTABLISHMENT2_COUNTRYREGION</CountrySubDivisionName>
        <Line>STUFFINGESTABLISHMENT2_ADDRESS</Line>
        <PostcodeID>123456789</PostcodeID>
      </Address>
    </StuffingEstablishment>";
			AssertNotContains(unexpectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original).GetXMLMessage());
			goodsShipmentMock.Setup(m => m.CustomsControlledArea).Returns(ZString.Empty);
			unexpectedXML = @"<Warehouse>
      <ID>GOODSSHIPMENT_CUSTOMSCONTROLLEDAREA</ID>
    </Warehouse>";
			AssertNotContains(unexpectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original).GetXMLMessage());
		}

		public void TestPopulateGoodsShipmentMail()
		{
			SetUpMocks();
			iExportDeclarationMock.Setup(m => m.IsSea).Returns(false);
			iExportDeclarationMock.Setup(m => m.IsMail).Returns(true);
			AssertContains(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TestEX1PopulateGoodsShipmentMail.txt")), ex1Builder.GetXMLMessage());
			goodsShipmentMock.Setup(m => m.DeliverToParty).Returns(importerMock.Object);
			var unexpectedXML = @"<DeliveryDestination>
      <Name>DELIVERTOPARTY_NAME</Name>
      <Address>
        <CityName>DELIVERTOPARTY_CITY</CityName>
        <CountryCode>DELIVERTOPARTY_COUNTRYCODE</CountryCode>
        <CountrySubDivisionName>DELIVERTOPARTY_COUNTRYREGION</CountrySubDivisionName>
        <Line>DELIVERTOPARTY_ADDRESS</Line>
        <PostcodeID>DELIVERTOPARTY_POSTCODE</PostcodeID>
      </Address>
    </DeliveryDestination>";
			AssertNotContains(unexpectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original).GetXMLMessage());
			AssertNotContains(unexpectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original).GetXMLMessage());
			unexpectedXML = @"<Warehouse>
      <ID>GOODSSHIPMENT_CUSTOMSCONTROLLEDAREA</ID>
    </Warehouse>";
			goodsShipmentMock.Setup(m => m.CustomsControlledArea).Returns(ZString.Empty);
			AssertNotContains(unexpectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original).GetXMLMessage());
		}

		public void TestPopulateGoodsShipmentAir()
		{
			SetUpMocks();
			iExportDeclarationMock.Setup(m => m.IsSea).Returns(false);
			iExportDeclarationMock.Setup(m => m.IsAir).Returns(true);
			AssertContains(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TestEX1PopulateGoodsShipmentAir.txt")), ex1Builder.GetXMLMessage());

			goodsShipmentMock.Setup(m => m.DeliverToParty).Returns(importerMock.Object);
			var unexpectedXML = @"<DeliveryDestination>
      <Name>DELIVERTOPARTY_NAME</Name>
      <Address>
        <CityName>DELIVERTOPARTY_CITY</CityName>
        <CountryCode>DELIVERTOPARTY_COUNTRYCODE</CountryCode>
        <CountrySubDivisionName>DELIVERTOPARTY_COUNTRYREGION</CountrySubDivisionName>
        <Line>DELIVERTOPARTY_ADDRESS</Line>
        <PostcodeID>DELIVERTOPARTY_POSTCODE</PostcodeID>
      </Address>
    </DeliveryDestination>";
			AssertNotContains(unexpectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original).GetXMLMessage());
			AssertNotContains(unexpectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original).GetXMLMessage());
			unexpectedXML = @"<Warehouse>
      <ID>GOODSSHIPMENT_CUSTOMSCONTROLLEDAREA</ID>
    </Warehouse>";
			goodsShipmentMock.Setup(m => m.CustomsControlledArea).Returns(ZString.Empty);
			AssertNotContains(unexpectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original).GetXMLMessage());
		}

		public void TestPopulateConsignmentSea()
		{
			SetUpMocks();
			AssertContains(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TestEX1PopulateConsignmentSea.txt")), ex1Builder.GetXMLMessage());

			goodsShipmentMock.Setup(m => m.LocationOfGoods).Returns(ZString.Empty);
			var unexpectedXML = @"<GoodsLocation>
        <ID>GOODSSHIPMENT_LOCATIONOFGOODS</ID>
      </GoodsLocation>";
			AssertNotContains(unexpectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original).GetXMLMessage());
			iExportDeclarationMock.Setup(m => m.IsContainerised).Returns(false);
			unexpectedXML = @"<TransportEquipment>
        <SequenceNumeric>1</SequenceNumeric>
        <CharacteristicCode>16</CharacteristicCode>
        <ID>1</ID>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>0</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>0</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
      </TransportEquipment>
      <TransportEquipment>
        <SequenceNumeric>2</SequenceNumeric>
        <CharacteristicCode>16</CharacteristicCode>
        <ID>2</ID>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>0</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>0</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
      </TransportEquipment>";
			AssertNotContains(unexpectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original).GetXMLMessage());
		}

		public void TestPopulateConsignmentAir()
		{
			SetUpMocks();
			iExportDeclarationMock.Setup(m => m.IsSea).Returns(false);
			iExportDeclarationMock.Setup(m => m.IsAir).Returns(true);
			AssertContains(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TestEX1PopulateConsignmentAir.txt")), ex1Builder.GetXMLMessage());

			iExportDeclarationMock.Setup(m => m.IsContainerised).Returns(false);
			var unexpectedXML = @"<TransportEquipment>
        <SequenceNumeric>1</SequenceNumeric>
        <CharacteristicCode>16</CharacteristicCode>
        <ID>1</ID>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>0</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>0</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
      </TransportEquipment>
      <TransportEquipment>
        <SequenceNumeric>2</SequenceNumeric>
        <CharacteristicCode>16</CharacteristicCode>
        <ID>2</ID>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>0</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>0</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
      </TransportEquipment>";
			AssertNotContains(unexpectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original).GetXMLMessage());
		}

		public void TestPopulateConsignmentMail()
		{
			SetUpMocks();
			iExportDeclarationMock.Setup(m => m.IsSea).Returns(false);
			iExportDeclarationMock.Setup(m => m.IsMail).Returns(true);
			AssertContains(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TestEX1PopulateConsignmentMail.txt")), ex1Builder.GetXMLMessage());

			iExportDeclarationMock.Setup(m => m.IsContainerised).Returns(false);
			var unexpectedXML = @"<TransportEquipment>
        <SequenceNumeric>1</SequenceNumeric>
        <CharacteristicCode>16</CharacteristicCode>
        <ID>1</ID>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>0</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>0</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
      </TransportEquipment>
      <TransportEquipment>
        <SequenceNumeric>2</SequenceNumeric>
        <CharacteristicCode>16</CharacteristicCode>
        <ID>2</ID>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>0</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>0</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
      </TransportEquipment>";
			AssertNotContains(unexpectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original).GetXMLMessage());
		}

		public void TestPopulateLocationOfGoods()
		{
			SetUpMocks();
			var expectedXML = @"<GoodsLocation>
        <ID>GOODSSHIPMENT_LOCATIONOFGOODS</ID>
      </GoodsLocation>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
			goodsShipmentMock.Setup(m => m.LocationOfGoods).Returns(ZString.Empty);
			AssertNotContains(expectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original).GetXMLMessage());
			expectedXML = @"<GoodsLocation>
        <ID />
      </GoodsLocation>";
			iExportDeclarationMock.Setup(m => m.IsSea).Returns(false);
			iExportDeclarationMock.Setup(m => m.IsMail).Returns(true);
			goodsShipmentMock.Setup(m => m.LocationOfGoods).Returns(ZString.Empty);
			AssertContains(expectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original).GetXMLMessage());
			iExportDeclarationMock.Setup(m => m.IsMail).Returns(false);
			iExportDeclarationMock.Setup(m => m.IsAir).Returns(true);
			AssertContains(expectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original).GetXMLMessage());
		}

		public void TestPopulatePortOfLoading()
		{
			SetUpMocks();
			var expectedXML = @"<LoadingLocation>
        <ID>GOODSSHIPMENT_PORTOFLOADING</ID>
      </LoadingLocation>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
		}

		public void TestPopulateBillDetailsWithoutMasterBills()
		{
			SetUpMocks();
			iExportDeclarationMock.Setup(m => m.MasterBills).Returns(Enumerable.Empty<IMasterBillTransportDocument>());
			AssertContains(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TestEX1PopulateBillDetailsWithoutMasterBills.txt")), ex1Builder.GetXMLMessage());
		}

		public void TestPopulateSingleBillDetails()
		{
			SetUpMocks();
			AssertContains(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TestEX1PopulateSingleBillDetails.txt")), ex1Builder.GetXMLMessage());
			masterBill1Mock.Setup(m => m.ChildBills).Returns(Enumerable.Empty<ZGuid>());
			masterBill2Mock.Setup(m => m.ChildBills).Returns(Enumerable.Empty<ZGuid>());
			masterBill1Mock.Setup(m => m.RelatedEquipment).Returns(Enumerable.Empty<ZGuid>());
			masterBill1Mock.Setup(m => m.RelatedPackages).Returns(Enumerable.Empty<ZGuid>());
			var expectedXML = @"<TransportContractDocument>
        <ID>MASTERBILL1_BILLNUMBER</ID>
        <TypeCode>MASTERBILL1_BILLTYPE</TypeCode>
      </TransportContractDocument>
      <TransportContractDocument>
        <ID>MASTERBILL2_BILLNUMBER</ID>
        <TypeCode>MASTERBILL2_BILLTYPE</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>28A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>0</SequenceNumeric>
          <DocumentSectionCode>31B</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>0</SequenceNumeric>
          <DocumentSectionCode>31B</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>0</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>0</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>";
			AssertContains(expectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original).GetXMLMessage());
		}

		public void TestPopulateSingleBillDetailsWithoutMasterBillsForMail()
		{
			SetUpMocks();
			iExportDeclarationMock.Setup(m => m.MasterBills).Returns(Enumerable.Empty<IMasterBillTransportDocument>());
			iExportDeclarationMock.Setup(m => m.IsMail).Returns(true);
			AssertContains(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TestEX1PopulateBillDetailsWithoutMasterBillsForMail.txt")), ex1Builder.GetXMLMessage());
		}

		public void TestPopulateSingleBillDetailsWithoutMasterBillsAndRelatedEquipment()
		{
			SetUpMocks();
			iExportDeclarationMock.Setup(m => m.MasterBills).Returns(Enumerable.Empty<IMasterBillTransportDocument>());
			iExportDeclarationMock.Setup(m => m.IsMail).Returns(false);
			houseBill1Mock.Setup(m => m.RelatedEquipment).Returns(Enumerable.Empty<ZGuid>());
			var expectedXML = @"<TransportContractDocument>
        <ID>MASTERBILL1_BILLNUMBER</ID>
        <TypeCode>MASTERBILL1_BILLTYPE</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>28A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>30B</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>30B</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>";
			AssertNotContains(expectedXML, ex1Builder.GetXMLMessage());
		}

		public void TestPopulateSingleBillDetailsWithoutMasterBillsRelatedEquipmentAndRelatedPackages()
		{
			SetUpMocks();
			iExportDeclarationMock.Setup(m => m.MasterBills).Returns(Enumerable.Empty<IMasterBillTransportDocument>());
			iExportDeclarationMock.Setup(m => m.IsMail).Returns(false);
			houseBill1Mock.Setup(m => m.RelatedEquipment).Returns(Enumerable.Empty<ZGuid>());
			houseBill1Mock.Setup(m => m.RelatedPackages).Returns(Enumerable.Empty<ZGuid>());
			houseBill2Mock.Setup(m => m.RelatedPackages).Returns(Enumerable.Empty<ZGuid>());
			houseBill3Mock.Setup(m => m.RelatedPackages).Returns(Enumerable.Empty<ZGuid>());
			houseBill4Mock.Setup(m => m.RelatedPackages).Returns(Enumerable.Empty<ZGuid>());
			var expectedXML = @"<Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>0</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>0</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
</TransportContractDocument>";
			AssertNotContains(expectedXML, ex1Builder.GetXMLMessage());
		}

		public void TestPopulateBillDetailsSea()
		{
			SetUpMocks();
			AssertContains(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TestEX1PopulateBillDetailsSea.txt")), ex1Builder.GetXMLMessage());
		}

		public void TestPopulateBillDetailsMail()
		{
			SetUpMocks();
			iExportDeclarationMock.Setup(m => m.IsMail).Returns(true);
			AssertContains(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TestEX1PopulateBillDetailsMail.txt")), ex1Builder.GetXMLMessage());
		}

		public void TestPopulateTransportContractPointersWhenNoMasterBill()
		{
			SetUpMocks();
			var expectedXML = @"<Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>28A</DocumentSectionCode>
        </Pointer>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
		}

		public void TestGoodsLocation()
		{
			SetUpMocks();
			goodsShipmentMock.Reset();
			goodsShipmentMock.Setup(m => m.LocationOfGoods).Returns("NZWLG");
			var expectedElement = @"<GoodsLocation>
        <ID>NZWLG</ID>
      </GoodsLocation>";
			AssertContains(expectedElement, ex1Builder.GetXMLMessage());
		}

		public void TestPopulateTransportContractPointers()
		{
			SetUpMocks();
			var expectedXML = @"<Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>30B</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>30B</DocumentSectionCode>
        </Pointer>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
		}

		public void TestPopulateTransportEquipmentPointers()
		{
			SetUpMocks();
			var expectedXML = @"<Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>28A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>0</SequenceNumeric>
          <DocumentSectionCode>30B</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>0</SequenceNumeric>
          <DocumentSectionCode>30B</DocumentSectionCode>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
		}

		public void TestPopulatePackagingPointers()
		{
			SetUpMocks();
			masterBill1Mock.Setup(m => m.ChildBills).Returns(Enumerable.Empty<ZGuid>());
			var expectedXML = @"        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>0</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>0</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
		}

		public void TestPopulateContainerDetailsNotPallets()
		{
			SetUpMocks();
			var expectedXML = @"<TransportEquipment>
        <SequenceNumeric>1</SequenceNumeric>
        <CharacteristicCode>16</CharacteristicCode>
        <ID>1</ID>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>0</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>0</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
      </TransportEquipment>
      <TransportEquipment>
        <SequenceNumeric>2</SequenceNumeric>
        <CharacteristicCode>16</CharacteristicCode>
        <ID>2</ID>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>0</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>0</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
      </TransportEquipment>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
		}

		public void TestPopulateContainerDetailsArePallets()
		{
			SetUpMocks();
			container1Mock.Setup(m => m.IsPallet).Returns(false);
			var expectedXML = @"<TransportEquipment>
        <SequenceNumeric>1</SequenceNumeric>
        <CharacteristicCode>CONTAINER1_SIZE</CharacteristicCode>
        <FullnessCode>CONTAINER1_STATUS</FullnessCode>
        <ID>CONTAINER1_CONTAINERNUMBER</ID>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>0</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>0</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
        <Seal>
          <SequenceNumeric>1</SequenceNumeric>
          <ID>CONTAINER1_SEALNUMBERS1</ID>
        </Seal>
        <Seal>
          <SequenceNumeric>2</SequenceNumeric>
          <ID>CONTAINER1_SEALNUMBERS2</ID>
        </Seal>
      </TransportEquipment>
      <TransportEquipment>
        <SequenceNumeric>2</SequenceNumeric>
        <CharacteristicCode>16</CharacteristicCode>
        <ID>2</ID>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>0</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>0</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
      </TransportEquipment>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
		}

		public void TestPopulateEquipmentPointers()
		{
			SetUpMocks();
			var expectedXML = @"<TransportEquipment>
        <SequenceNumeric>1</SequenceNumeric>
        <CharacteristicCode>16</CharacteristicCode>
        <ID>1</ID>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>0</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>0</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
      </TransportEquipment>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
		}

		public void TestPopulatePortOfDischarge()
		{
			SetUpMocks();
			var expectedXML = @"<UnloadingLocation>
        <ID>GOODSSHIPMENT_PORTOFDISCHARGE</ID>
      </UnloadingLocation>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
		}

		public void TestPopulateDeliveryParty()
		{
			SetUpMocks();
			var expectedXML = @"<DeliveryDestination>
      <Name>DELIVERTOPARTY_NAME</Name>
      <Address>
        <CityName>DELIVERTOPARTY_CITY</CityName>
        <CountryCode>DELIVERTOPARTY_COUNTRYCODE</CountryCode>
        <CountrySubDivisionName>DELIVERTOPARTY_COUNTRYREGION</CountrySubDivisionName>
        <Line>DELIVERTOPARTY_ADDRESS</Line>
        <PostcodeID>123456789</PostcodeID>
      </Address>
    </DeliveryDestination>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
			goodsShipmentMock.Reset();
			goodsShipmentMock.Setup(m => m.DeliverToParty).Returns(importerMock.Object);
			AssertNotContains(expectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original).GetXMLMessage());
		}

		public void TestPopulateImporter()
		{
			SetUpMocks();
			var expectedXML = @"<Importer>
      <Name>IMPORTER_NAME</Name>
      <Address>
        <CityName>IMPORTER_CITY</CityName>
        <CountryCode>IMPORTER_COUNTRYCODE</CountryCode>
        <CountrySubDivisionName>IMPORTER_COUNTRYREGION</CountrySubDivisionName>
        <Line>IMPORTER_ADDRESS</Line>
        <PostcodeID>123456789</PostcodeID>
      </Address>
    </Importer>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
			iExportDeclarationMock.Setup(m => m.Importer).Returns((IOrganisation)null);
			AssertNotContains(expectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original).GetXMLMessage());
		}

		public void TestPopulateInvoices()
		{
			SetUpMocks();
			var expectedXML = @"<Invoice>
      <IssueDateTime formatCode=""102"">20181107</IssueDateTime>
      <ID>IINVOICE1_INVOICENUMBER</ID>
      <SequenceNumeric>1</SequenceNumeric>
    </Invoice>
    <Invoice>
      <IssueDateTime formatCode=""102"">20150402</IssueDateTime>
      <ID>IINVOICE2_INVOICENUMBER</ID>
      <SequenceNumeric>2</SequenceNumeric>
    </Invoice>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
		}

		public void TestPopulateNotifyParties()
		{
			SetUpMocks();
			var expectedXML = @"<NotifyParty>
      <Name>NOTIFYPARTY1_NAME</Name>
      <RoleCode>N2</RoleCode>
      <Communication>
        <ID>ICOMMUNICATION1_CONTACTDETAIL</ID>
        <TypeID>EM</TypeID>
      </Communication>
    </NotifyParty>
    <NotifyParty>
      <Name>NOTIFYPARTY2_NAME</Name>
      <RoleCode>N2</RoleCode>
      <Communication>
        <ID>ICOMMUNICATION1_CONTACTDETAIL</ID>
        <TypeID>EM</TypeID>
      </Communication>
    </NotifyParty>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
			notifyParty1Mock.Reset();
			notifyParty1Mock.Setup(m => m.Name).Returns(ZString.Empty);
			AssertNotContains(@"<Name>NOTIFYPARTY1_NAME</Name>", new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original).GetXMLMessage());
			notifyParty1Mock.Reset();
			notifyParty1Mock.Setup(m => m.CustomsClientCode).Returns(ZString.Empty);
			AssertNotContains(@"<ID>NOTIFYPARTY1_CUSTOMSCLIENTCODE</ID>", new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original).GetXMLMessage());
			iCommunication1Mock.Reset();
			iCommunication1Mock.Setup(m => m.ContactType).Returns("ICOMMUNICATION1_CONTACTTYPE");
			AssertNotContains(@"<ID>NOTIFYPARTY1_CUSTOMSCLIENTCODE</ID>", new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original).GetXMLMessage());
		}

		public void TestPopulateNotifyPartiesWithoutNotifyParties()
		{
			SetUpMocks();
			goodsShipmentMock.Reset();
			goodsShipmentMock.Setup(m => m.NotifyParties).Returns(Enumerable.Empty<IOrganisationSimple>());
			var expectedXML = @"<NotifyParty>
      <Name>NOTIFYPARTY1_NAME</Name>
      <ID>NOTIFYPARTY1_CUSTOMSCLIENTCODE</ID>
      <RoleCode>N2</RoleCode>
      <Communication />
    </NotifyParty>
    <NotifyParty>
      <Name>NOTIFYPARTY2_NAME</Name>
      <ID>NOTIFYPARTY2_CUSTOMSCLIENTCODE</ID>
      <RoleCode>N2</RoleCode>
      <Communication />
    </NotifyParty>";
			AssertNotContains(expectedXML, ex1Builder.GetXMLMessage());
		}

		public void TestPopulateStuffingEstablishments()
		{
			SetUpMocks();
			var expectedXML = @"<StuffingEstablishment>
      <Name>STUFFINGESTABLISHMENT1_NAME</Name>
      <Address>
        <CityName>STUFFINGESTABLISHMENT1_CITY</CityName>
        <CountryCode>STUFFINGESTABLISHMENT1_COUNTRYCODE</CountryCode>
        <CountrySubDivisionName>STUFFINGESTABLISHMENT1_COUNTRYREGION</CountrySubDivisionName>
        <Line>STUFFINGESTABLISHMENT1_ADDRESS</Line>
        <PostcodeID>123456789</PostcodeID>
      </Address>
    </StuffingEstablishment>
    <StuffingEstablishment>
      <Name>STUFFINGESTABLISHMENT2_NAME</Name>
      <Address>
        <CityName>STUFFINGESTABLISHMENT2_CITY</CityName>
        <CountryCode>STUFFINGESTABLISHMENT2_COUNTRYCODE</CountryCode>
        <CountrySubDivisionName>STUFFINGESTABLISHMENT2_COUNTRYREGION</CountrySubDivisionName>
        <Line>STUFFINGESTABLISHMENT2_ADDRESS</Line>
        <PostcodeID>123456789</PostcodeID>
      </Address>
    </StuffingEstablishment>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
			goodsShipmentMock.Reset();
			goodsShipmentMock.Setup(m => m.StuffingEstablishments).Returns(Enumerable.Empty<IOrganisation>());
			AssertNotContains(expectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original).GetXMLMessage());
		}

		public void TestPopulateCustomsControlledArea()
		{
			SetUpMocks();
			var expectedXML = @"<Warehouse>
      <ID>GOODSSHIPMENT_CUSTOMSCONTROLLEDAREA</ID>
    </Warehouse>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
		}

		public void TestPopulateGoodsItems()
		{
			SetUpMocks();
			AssertContains(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TestEX1PopulateGoodsItemDetails.txt")), ex1Builder.GetXMLMessage());
		}

		public void TestPopulatePermits()
		{
			SetUpMocks();
			var expectedXML = @"<AdditionalDocument>
        <ID>ITEM1_PERMITS1</ID>
        <TypeCode>PER</TypeCode>
      </AdditionalDocument>
      <AdditionalDocument>
        <ID>ITEM2_PERMITS2</ID>
        <TypeCode>PER</TypeCode>
      </AdditionalDocument>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
		}

		public void TestPopulateAdditionalInformation()
		{
			SetUpMocks();
			var expectedXML = @"<AdditionalInformation>
        <StatementCode>ITEM1_PROHIBITEDCODES1</StatementCode>
        <StatementTypeCode>PRO</StatementTypeCode>
      </AdditionalInformation>
      <AdditionalInformation>
        <StatementCode>ITEM1_PROHIBITEDCODES2</StatementCode>
        <StatementTypeCode>PRO</StatementTypeCode>
      </AdditionalInformation>
      <AdditionalInformation>
        <StatementCode>IOTHERINFO1_CODE</StatementCode>
        <StatementDescription>IOTHERINFO1_DATA</StatementDescription>
        <StatementTypeCode>OIN</StatementTypeCode>
      </AdditionalInformation>
      <AdditionalInformation>
        <StatementCode>IOTHERINFO2_CODE</StatementCode>
        <StatementDescription>IOTHERINFO2_DATA</StatementDescription>
        <StatementTypeCode>OIN</StatementTypeCode>
      </AdditionalInformation>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
			iOtherInfo1Mock.Reset();
			iOtherInfo1Mock.Setup(m => m.Data).Returns(ZString.Empty);
			AssertNotContains(@"<StatementDescription>IOTHERINFO1_DATA</StatementDescription>", new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original).GetXMLMessage());
		}

		public void TestPopulateCommodityDataDutyTaxFree()
		{
			SetUpMocks();
			iExportDeclarationMock.Setup(m => m.MessageType).Returns("RANDOMTYPE");
			var expectedXML = @"<DutyTaxFee>
    <TypeCode>CUD</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""AUD"">9</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>GST</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">0</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>TOT</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""AUD"">9</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>";
			AssertNotContains(expectedXML, ex1Builder.GetXMLMessage());
		}

		public void TestPopulateCommodityDataWithNoProducts()
		{
			SetUpMocks();
			item1Mock.Reset();
			item2Mock.Reset();
			var expectedXML = @"<Product>
          <ID>IPRODUCT1_ID</ID>
          <IdentifierTypeCode>IPRODUCT1_IDTYPE</IdentifierTypeCode>
        </Product>
        <Product>
          <ID>IPRODUCT2_ID</ID>
          <IdentifierTypeCode>IPRODUCT2_IDTYPE</IdentifierTypeCode>
        </Product>";
			AssertNotContains(expectedXML, ex1Builder.GetXMLMessage());
		}

		public void TestPopulateCommodityDataWithNoTemperatures()
		{
			SetUpMocks();
			item1Mock.Reset();
			item2Mock.Reset();
			var expectedXML = @"<Temperature>
          <StorageRequirementMeasure unitCode=""CEL"">6</StorageRequirementMeasure>
          <MinimumStorageRequirementMeasure unitCode=""CEL"">6</MinimumStorageRequirementMeasure>
          <MaximumStorageRequirementMeasure unitCode=""CEL"">6</MaximumStorageRequirementMeasure>
        </Temperature>";
			AssertNotContains(expectedXML, ex1Builder.GetXMLMessage());
		}

		public void TestPopulateItemClassification()
		{
			SetUpMocks();
			var expectedXML = @"<Classification>
          <ID>ICLASSIFICATION1_CLASSIFICATION</ID>
          <IdentificationTypeCode>ICLASSIFICATION1_CLASSIFICATIONTYPECODE</IdentificationTypeCode>
        </Classification>
        <Classification>
          <ID>ICLASSIFICATION2_CLASSIFICATION</ID>
          <IdentificationTypeCode>ICLASSIFICATION2_CLASSIFICATIONTYPECODE</IdentificationTypeCode>
        </Classification>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
		}

		public void TestPopulateLineDutyTaxFees()
		{
			SetUpMocks();
			var expectedXML = @"<DutyTaxFee>
    <TypeCode>CUD</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""AUD"">9</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>GST</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">0</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>TOT</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""AUD"">9</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
			dutyTaxFee1Mock.Reset();
			dutyTaxFee1Mock.Setup(m => m.DutyTaxFeeType).Returns("ABC");
			dutyTaxFee2Mock.Reset();
			dutyTaxFee2Mock.Setup(m => m.DutyTaxFeeType).Returns("EFG");
			AssertNotContains(expectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original).GetXMLMessage());
		}

		public void TestItemDutyTaxFee()
		{
			SetUpMocks();
			var expectedXML = @"<DutyTaxFee>
          <TypeCode>CUD</TypeCode>
          <Payment>
            <TaxAssessedAmount currencyID=""AUD"">9</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
		}

		public void TestPopulateProducts()
		{
			SetUpMocks();
			var expectedXML = @"<Product>
          <ID>IPRODUCT1_ID</ID>
          <IdentifierTypeCode>IPRODUCT1_IDTYPE</IdentifierTypeCode>
        </Product>
        <Product>
          <ID>IPRODUCT2_ID</ID>
          <IdentifierTypeCode>IPRODUCT2_IDTYPE</IdentifierTypeCode>
        </Product>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
			iProduct1Mock.Reset();
			iProduct1Mock.Setup(m => m.Id).Returns(ZString.Empty);
			expectedXML = @"<Product>
          <ID>IPRODUCT1_ID</ID>
          <IdentifierTypeCode>IPRODUCT1_IDTYPE</IdentifierTypeCode>
        </Product>";
			AssertNotContains(expectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original).GetXMLMessage());
		}

		public void TestPopulateProductNames()
		{
			SetUpMocks();
			var expectedXML = @"<ProductName>
          <Name>ITEM1_BRANDNAME</Name>
          <NameQualifierCode>223</NameQualifierCode>
        </ProductName>
        <ProductName>
          <Name>ITEM1_COMMONNAME</Name>
          <NameQualifierCode>226</NameQualifierCode>
        </ProductName>
        <ProductName>
          <Name>ITEM1_REGISTEREDNAME</Name>
          <NameQualifierCode>55</NameQualifierCode>
        </ProductName>
        <ProductName>
          <Name>ITEM1_TRADENAME</Name>
          <NameQualifierCode>57</NameQualifierCode>
        </ProductName>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
			item1Mock.Reset();
			item1Mock.Setup(m => m.BrandName).Returns(ZString.Empty);
			expectedXML = @"<ProductName>
          <Name>ITEM1_BRANDNAME</Name>
          <NameQualifierCode>223</NameQualifierCode>
        </ProductName>";
			AssertNotContains(expectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original).GetXMLMessage());
			item1Mock.Reset();
			item1Mock.Setup(m => m.CommonName).Returns(ZString.Empty);
			expectedXML = @"<ProductName>
          <Name>ITEM1_COMMONNAME</Name>
          <NameQualifierCode>226</NameQualifierCode>
        </ProductName>";
			AssertNotContains(expectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original).GetXMLMessage());
			item1Mock.Reset();
			item1Mock.Setup(m => m.RegisteredName).Returns(ZString.Empty);
			expectedXML = @"<ProductName>
          <Name>ITEM1_REGISTEREDNAME</Name>
          <NameQualifierCode>55</NameQualifierCode>
        </ProductName>";
			AssertNotContains(expectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original).GetXMLMessage());
			item1Mock.Reset();
			item1Mock.Setup(m => m.TradeName).Returns(ZString.Empty);
			expectedXML = @"<ProductName>
          <Name>ITEM1_TRADENAME</Name>
          <NameQualifierCode>57</NameQualifierCode>
        </ProductName>";
			AssertNotContains(expectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original).GetXMLMessage());
		}

		public void TestProductName()
		{
			SetUpMocks();
			var expectedXML = @"<ProductName>
          <Name>ITEM1_BRANDNAME</Name>
          <NameQualifierCode>223</NameQualifierCode>
        </ProductName>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
		}

		public void TestPopulateProductCharacteristics()
		{
			SetUpMocks();
			var expectedXML = @"<ProductCharacteristics>
          <CharacteristicTypeCode>61</CharacteristicTypeCode>
          <CharacteristicQualifierCode>Y</CharacteristicQualifierCode>
        </ProductCharacteristics>
        <ProductCharacteristics>
          <CharacteristicTypeCode>211</CharacteristicTypeCode>
          <CharacteristicQualifierCode>Y</CharacteristicQualifierCode>
        </ProductCharacteristics>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
			item1Mock.Reset();
			item1Mock.Setup(m => m.UsedGoods).Returns(false);
			item2Mock.Reset();
			item2Mock.Setup(m => m.UsedGoods).Returns(false);
			expectedXML = @"<ProductCharacteristics>
          <CharacteristicTypeCode>61</CharacteristicTypeCode>
          <CharacteristicQualifierCode>Y</CharacteristicQualifierCode>
        </ProductCharacteristics>";
			AssertNotContains(expectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original).GetXMLMessage());
			item1Mock.Reset();
			item1Mock.Setup(m => m.GeneticallyModified).Returns(false);
			item2Mock.Reset();
			item2Mock.Setup(m => m.GeneticallyModified).Returns(false);
			expectedXML = @"<ProductCharacteristics>
          <CharacteristicTypeCode>211</CharacteristicTypeCode>
          <CharacteristicQualifierCode>Y</CharacteristicQualifierCode>
        </ProductCharacteristics>";
			AssertNotContains(expectedXML, new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original).GetXMLMessage());
		}

		public void TestProductCharacteristic()
		{
			SetUpMocks();
			var expectedXML = @"<ProductCharacteristics>
          <CharacteristicTypeCode>61</CharacteristicTypeCode>
          <CharacteristicQualifierCode>Y</CharacteristicQualifierCode>
        </ProductCharacteristics>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
		}

		public void TestPopulateTemperatureData()
		{
			SetUpMocks();
			var expectedXML = @"<Temperature>
          <StorageRequirementMeasure unitCode=""CEL"">6</StorageRequirementMeasure>
          <MinimumStorageRequirementMeasure unitCode=""CEL"">6</MinimumStorageRequirementMeasure>
          <MaximumStorageRequirementMeasure unitCode=""CEL"">6</MaximumStorageRequirementMeasure>
        </Temperature>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
		}

		public void TestPopulateItemTransportEquipment()
		{
			SetUpMocks();
			var expectedXML = @"<TransportEquipment>
          <SequenceNumeric>1</SequenceNumeric>
          <ID>ITEM1_CONTAINERNUMBERS1</ID>
        </TransportEquipment>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
		}

		public void TestPopulateMeasures()
		{
			SetUpMocks();
			var expectedXML = @"<GoodsMeasure>
        <GrossMassMeasure unitCode=""KGM"">6</GrossMassMeasure>
        <NetNetWeightMeasure unitCode=""KGM"">3</NetNetWeightMeasure>
        <TariffQuantity unitCode=""BDU"">1</TariffQuantity>
      </GoodsMeasure>
      <GoodsMeasure>
        <TariffQuantity unitCode=""BDU"">10</TariffQuantity>
      </GoodsMeasure>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
			item1Mock.Reset();
			item1Mock.Setup(m => m.StatisticalQtyUnit).Returns(ZString.Empty);
			item2Mock.Reset();
			item2Mock.Setup(m => m.StatisticalQtyUnit).Returns(ZString.Empty);
			AssertNotContains(@"<TariffQuantity unitCode=""BDU"">1</TariffQuantity>", new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original).GetXMLMessage());
			item1Mock.Reset();
			item1Mock.Setup(m => m.SupplementaryQty).Returns(0);
			item2Mock.Reset();
			item2Mock.Setup(m => m.SupplementaryQty).Returns(0);
			AssertNotContains(@"<TariffQuantity unitCode=""BDU"">10</TariffQuantity>", new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original).GetXMLMessage());
		}

		public void TestPopulateOrigin()
		{
			SetUpMocks();
			var expectedXML = @"<Origin>
        <CountryCode>ITEM1_ORIGINCOUNTRY</CountryCode>
      </Origin>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
		}

		public void TestPopulateGAGIPackaging()
		{
			SetUpMocks();
			var expectedXML = @"<Packaging>
        <SequenceNumeric>1</SequenceNumeric>
        <MarksNumbersID>IPACKAGING1_SHIPPINGMARKS</MarksNumbersID>
        <QuantityQuantity>3</QuantityQuantity>
        <TypeCode>IPACKAGING1_SHIPPINGMARKS</TypeCode>
        <VolumeMeasure unitCode=""MTQ"">7</VolumeMeasure>
      </Packaging>
      <Packaging>
        <SequenceNumeric>2</SequenceNumeric>
        <MarksNumbersID>IPACKAGING2_SHIPPINGMARKS</MarksNumbersID>
        <QuantityQuantity>3</QuantityQuantity>
        <TypeCode>IPACKAGING2_SHIPPINGMARKS</TypeCode>
        <VolumeMeasure unitCode=""MTQ"">7</VolumeMeasure>
      </Packaging>";
			AssertContains(expectedXML, ex1Builder.GetXMLMessage());
		}

		void SetUpMocks()
		{
			iExportDeclarationMock = new Mock<IExportDeclaration>();
			iExportDeclarationMock.Setup(m => m.TSWReferenceNumber).Returns("ENTRY12345");
			iExportDeclarationMock.Setup(m => m.SenderReferenceNumber).Returns("C00001165");
			iExportDeclarationMock.Setup(m => m.IsSea).Returns(true);
			iExportDeclarationMock.Setup(m => m.CraftName).Returns("CRAFTNAME");
			iExportDeclarationMock.Setup(m => m.FlightNo).Returns("FLIGHTNO");
			iExportDeclarationMock.Setup(m => m.LloydsNo).Returns("LLOYDSNO");
			iExportDeclarationMock.Setup(m => m.VoyageNo).Returns("VOYAGENO123");
			var carrierMock = new Mock<IOrganisationSimple>();
			carrierMock.Setup(m => m.Name).Returns("CARRIER_NAME");
			iExportDeclarationMock.Setup(m => m.Carrier).Returns(carrierMock.Object);
			iExportDeclarationMock.Setup(m => m.IsSea).Returns(true);
			iExportDeclarationMock.Setup(m => m.IsAir).Returns(false);
			iExportDeclarationMock.Setup(m => m.IsMail).Returns(false);
			iExportDeclarationMock.Setup(m => m.TotalGrossWeightInKGM).Returns(0.2m);
			iExportDeclarationMock.Setup(m => m.SubmitterCode).Returns("SUBMITTERCODE");
			iExportDeclarationMock.Setup(m => m.HandlingInformation).Returns("HANDLINGINFORMATION");
			iExportDeclarationMock.Setup(m => m.BrokerCode).Returns("BROKERCODE");
			iExportDeclarationMock.Setup(m => m.PaymentType).Returns("PAYMENTTYPE");
			iExportDeclarationMock.Setup(m => m.DateOfExport).Returns(new ZDateTime(2018, 11, 10));
			iExportDeclarationMock.Setup(m => m.IsContainerised).Returns(true);
			iExportDeclarationMock.Setup(m => m.PreviousDocumentNo).Returns("PREVIOUSDOCUMENTNO");
			iExportDeclarationMock.Setup(m => m.PreviousDocumentType).Returns("PREVIOUSDOCUMENTTYPE");
			iExportDeclarationMock.Setup(m => m.MessageType).Returns("E41");
			iExportDeclarationMock.Setup(m => m.IsCompletionEntry).Returns(true);
			var supportingDocument1Mock = new Mock<ITSWAttachment>();
			supportingDocument1Mock.Setup(m => m.DocType).Returns("CDO");
			supportingDocument1Mock.Setup(m => m.FileName).Returns("TESTFILE1.PDF");
			var supportingDocument2Mock = new Mock<ITSWAttachment>();
			supportingDocument2Mock.Setup(m => m.DocType).Returns("INV");
			supportingDocument2Mock.Setup(m => m.FileName).Returns("TESTFILE1.XLSX");
			iAdditionalInformationMock = new Mock<IAdditionalInformation>();
			iAdditionalInformationMock.Setup(m => m.AdditionalStatementText).Returns("ADDITIONALINFORMATION_ADDITIONALSTATEMENTTEXT");
			iAdditionalInformationMock.Setup(m => m.ManualOverrideText).Returns("ADDITIONALINFORMATION_MANUALOVERRIDETEXT");
			iAdditionalInformationMock.Setup(m => m.FreeText).Returns("ADDITIONALINFORMATION_FREETEXT");
			iAdditionalInformationMock.Setup(m => m.SupportingDocuments).Returns(new List<ITSWAttachment>() { supportingDocument1Mock.Object, supportingDocument2Mock.Object });
			iExportDeclarationMock.Setup(m => m.AdditionalInformation).Returns(iAdditionalInformationMock.Object);
			iExportDeclarationMock.Setup(m => m.Permits).Returns(new List<ZString>() { "PERMIT1", "PERMIT2" });
			var exchangeRate1Mock = new Mock<ICurrency>();
			var exchangeRate2Mock = new Mock<ICurrency>();
			exchangeRate1Mock.Setup(m => m.ExchangeRateIndicator).Returns("EXCHANGERATE1_EXCHANGERATEINDICATOR");
			exchangeRate1Mock.Setup(m => m.CurrencyCode).Returns("NZD");
			exchangeRate1Mock.Setup(m => m.ExchangeRate).Returns(2.123);
			exchangeRate2Mock.Setup(m => m.ExchangeRateIndicator).Returns("EXCHANGERATE2_EXCHANGERATEINDICATOR");
			exchangeRate2Mock.Setup(m => m.CurrencyCode).Returns("NZD");
			exchangeRate2Mock.Setup(m => m.ExchangeRate).Returns(3.123);
			iExportDeclarationMock.Setup(m => m.ExchangeRates).Returns(new List<ICurrency>() { exchangeRate1Mock.Object, exchangeRate2Mock.Object });
			var packaging1Mock = new Mock<IPackaging>();
			packaging1Mock.Setup(m => m.NumberOfPackages).Returns(3);
			packaging1Mock.Setup(m => m.MessageSequence).Returns(4);
			packaging1Mock.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			packaging1Mock.Setup(m => m.PackageType).Returns("PACKAGING1_PACKAGETYPE");
			var packaging2Mock = new Mock<IPackaging>();
			packaging2Mock.Setup(m => m.NumberOfPackages).Returns(3);
			packaging2Mock.Setup(m => m.MessageSequence).Returns(4);
			packaging2Mock.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			packaging2Mock.Setup(m => m.PackageType).Returns("PACKAGING2_PACKAGETYPE");
			iExportDeclarationMock.Setup(m => m.Packaging).Returns(new List<IPackaging>() { packaging1Mock.Object, packaging2Mock.Object });
			container1Mock = new Mock<ITransportEquipment>();
			container1Mock.Setup(m => m.MessageSequence).Returns(1);
			container1Mock.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			container1Mock.Setup(m => m.IsPallet).Returns(true);
			container1Mock.Setup(m => m.Size).Returns("CONTAINER1_SIZE");
			container1Mock.Setup(m => m.Status).Returns("CONTAINER1_STATUS");
			container1Mock.Setup(m => m.ContainerNumber).Returns("CONTAINER1_CONTAINERNUMBER");
			container1Mock.Setup(m => m.SealNumbers).Returns(new List<ZString>() { "CONTAINER1_SEALNUMBERS1", "CONTAINER1_SEALNUMBERS2" });
			container1Mock.Setup(m => m.RelatedPackages).Returns(new List<ZGuid>() { ZGuid.NewZGuid(), ZGuid.NewZGuid() });
			var container2Mock = new Mock<ITransportEquipment>();
			container2Mock.Setup(m => m.MessageSequence).Returns(2);
			container2Mock.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			container2Mock.Setup(m => m.IsPallet).Returns(true);
			container2Mock.Setup(m => m.Size).Returns("CONTAINER2_SIZE");
			container2Mock.Setup(m => m.Status).Returns("CONTAINER2_STATUS");
			container2Mock.Setup(m => m.ContainerNumber).Returns("CONTAINER2_CONTAINERNUMBER");
			container2Mock.Setup(m => m.SealNumbers).Returns(new List<ZString>() { "CONTAINER2_SEALNUMBERS1", "CONTAINER2_SEALNUMBERS2" });
			container2Mock.Setup(m => m.RelatedPackages).Returns(new List<ZGuid>() { ZGuid.NewZGuid(), ZGuid.NewZGuid() });
			iExportDeclarationMock.Setup(m => m.Equipment).Returns(new List<ITransportEquipment>() { container1Mock.Object, container2Mock.Object });
			var exporterMock = new Mock<IOrganisation>();
			exporterMock.Setup(m => m.CustomsClientCode).Returns("EXPORTER_CUSTOMSCLIENTCODE");
			iExportDeclarationMock.Setup(m => m.Exporter).Returns(exporterMock.Object);
			importerMock = new Mock<IOrganisation>();
			importerMock.Setup(m => m.Name).Returns("IMPORTER_NAME");
			importerMock.Setup(m => m.City).Returns("IMPORTER_CITY");
			importerMock.Setup(m => m.CountryCode).Returns("IMPORTER_COUNTRYCODE");
			importerMock.Setup(m => m.CountryRegion).Returns("IMPORTER_COUNTRYREGION");
			importerMock.Setup(m => m.Address).Returns("IMPORTER_ADDRESS");
			importerMock.Setup(m => m.PostCode).Returns("123456789");
			iExportDeclarationMock.Setup(m => m.Importer).Returns(importerMock.Object);
			dutyTaxFee1Mock = new Mock<IDutyTaxFee>();
			dutyTaxFee1Mock.Setup(m => m.DutyTaxFeeType).Returns("CUD");
			dutyTaxFee1Mock.Setup(m => m.CurrencyCode).Returns("DUTYTAXFEE1_CURRENCYCODE");
			dutyTaxFee1Mock.Setup(m => m.Amount).Returns(9m);
			dutyTaxFee2Mock = new Mock<IDutyTaxFee>();
			dutyTaxFee2Mock.Setup(m => m.DutyTaxFeeType).Returns("TOT");
			dutyTaxFee2Mock.Setup(m => m.CurrencyCode).Returns("DUTYTAXFEE2_CURRENCYCODE");
			dutyTaxFee2Mock.Setup(m => m.Amount).Returns(9m);
			iExportDeclarationMock.Setup(m => m.DutyTaxFees).Returns(new List<IDutyTaxFee>() { dutyTaxFee1Mock.Object, dutyTaxFee2Mock.Object });
			iCommunication1Mock = new Mock<ICommunication>();
			iCommunication1Mock.Setup(m => m.ContactDetail).Returns("ICOMMUNICATION1_CONTACTDETAIL");
			iCommunication1Mock.Setup(m => m.ContactType).Returns("EM");
			var iCommunication2Mock = new Mock<ICommunication>();
			iCommunication2Mock.Setup(m => m.ContactDetail).Returns("ICOMMUNICATION1_CONTACTDETAIL");
			iCommunication2Mock.Setup(m => m.ContactType).Returns("EM");
			var declarantMock = new Mock<IDeclarant>();
			declarantMock.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1Mock.Object, iCommunication2Mock.Object });
			declarantMock.Setup(m => m.DeclarantID).Returns("DECLARANT_DECLARANTID");
			iExportDeclarationMock.Setup(m => m.Declarant).Returns(declarantMock.Object);
			iExportDeclarationMock.Setup(m => m.Permits).Returns(new List<ZString>() { "PERMIT1", "PERMIT2" });
			iOtherInfo1Mock = new Mock<IOtherInfo>();
			iOtherInfo1Mock.Setup(m => m.Code).Returns("IOTHERINFO1_CODE");
			iOtherInfo1Mock.Setup(m => m.Data).Returns("IOTHERINFO1_DATA");
			var iOtherInfo2Mock = new Mock<IOtherInfo>();
			iOtherInfo2Mock.Setup(m => m.Code).Returns("IOTHERINFO2_CODE");
			iOtherInfo2Mock.Setup(m => m.Data).Returns("IOTHERINFO2_DATA");
			iExportDeclarationMock.Setup(m => m.OtherReferencedDocuments).Returns(new List<IOtherInfo>() { iOtherInfo1Mock.Object, iOtherInfo2Mock.Object });
			iExportDeclarationMock.Setup(m => m.OtherInfoCodes).Returns(new List<IOtherInfo>() { iOtherInfo1Mock.Object, iOtherInfo2Mock.Object });
			var iClassification1Mock = new Mock<IClassification>();
			iClassification1Mock.Setup(m => m.Classification).Returns("ICLASSIFICATION1_CLASSIFICATION");
			iClassification1Mock.Setup(m => m.ClassificationTypeCode).Returns("ICLASSIFICATION1_CLASSIFICATIONTYPECODE");
			var iClassification2Mock = new Mock<IClassification>();
			iClassification2Mock.Setup(m => m.Classification).Returns("ICLASSIFICATION2_CLASSIFICATION");
			iClassification2Mock.Setup(m => m.ClassificationTypeCode).Returns("ICLASSIFICATION2_CLASSIFICATIONTYPECODE");
			var temperaturesMock = new Mock<ITemperatureRequirements>();
			temperaturesMock.Setup(m => m.StorageTemp).Returns(6m);
			temperaturesMock.Setup(m => m.MinStorageTemp).Returns(6m);
			temperaturesMock.Setup(m => m.MaxStorageTemp).Returns(6m);
			var iPackaging1Mock = new Mock<IPackaging>();
			iPackaging1Mock.Setup(m => m.ShippingMarks).Returns("IPACKAGING1_SHIPPINGMARKS");
			iPackaging1Mock.Setup(m => m.NumberOfPackages).Returns(3);
			iPackaging1Mock.Setup(m => m.PackageType).Returns("IPACKAGING1_SHIPPINGMARKS");
			iPackaging1Mock.Setup(m => m.PackageVolumeInMTQ).Returns(7m);
			var iPackaging2Mock = new Mock<IPackaging>();
			iPackaging2Mock.Setup(m => m.ShippingMarks).Returns("IPACKAGING2_SHIPPINGMARKS");
			iPackaging2Mock.Setup(m => m.NumberOfPackages).Returns(3);
			iPackaging2Mock.Setup(m => m.PackageType).Returns("IPACKAGING2_SHIPPINGMARKS");
			iPackaging2Mock.Setup(m => m.PackageVolumeInMTQ).Returns(7m);
			iProduct1Mock = new Mock<IProduct>();
			iProduct1Mock.Setup(m => m.Id).Returns("IPRODUCT1_ID");
			iProduct1Mock.Setup(m => m.IdType).Returns("IPRODUCT1_IDTYPE");
			var iProduct2Mock = new Mock<IProduct>();
			iProduct2Mock.Setup(m => m.Id).Returns("IPRODUCT2_ID");
			iProduct2Mock.Setup(m => m.IdType).Returns("IPRODUCT2_IDTYPE");
			item1Mock = new Mock<IGoodsItems>();
			item1Mock.Setup(m => m.Permits).Returns(new List<ZString>() { "ITEM1_PERMITS1", "ITEM2_PERMITS2" });
			item1Mock.Setup(m => m.ProhibitedCodes).Returns(new List<ZString>() { "ITEM1_PROHIBITEDCODES1", "ITEM1_PROHIBITEDCODES2" });
			item1Mock.Setup(m => m.OtherInfoCodes).Returns(new List<IOtherInfo>() { iOtherInfo1Mock.Object, iOtherInfo2Mock.Object });
			item1Mock.Setup(m => m.GoodsDescription).Returns("ITEM1_GOODSDESCRIPTION");
			item1Mock.Setup(m => m.ValueInForeignCurrency).Returns(6m);
			item1Mock.Setup(m => m.ForeignCurrencyCode).Returns("ITEM1_FOREIGNCURRENCYCODE");
			item1Mock.Setup(m => m.Classifications).Returns(new List<IClassification>() { iClassification1Mock.Object, iClassification2Mock.Object });
			item1Mock.Setup(m => m.BrandName).Returns("ITEM1_BRANDNAME");
			item1Mock.Setup(m => m.CommonName).Returns("ITEM1_COMMONNAME");
			item1Mock.Setup(m => m.RegisteredName).Returns("ITEM1_REGISTEREDNAME");
			item1Mock.Setup(m => m.TradeName).Returns("ITEM1_TRADENAME");
			item1Mock.Setup(m => m.Temperatures).Returns(temperaturesMock.Object);
			item1Mock.Setup(m => m.ContainerNumbers).Returns(new List<ZString>() { "ITEM1_CONTAINERNUMBERS1", "ITEM1_CONTAINERNUMBERS2" });
			item1Mock.Setup(m => m.ItemGrossWeightInKGM).Returns(6m);
			item1Mock.Setup(m => m.ItemNetWeightInKGM).Returns(3m);
			item1Mock.Setup(m => m.StatisticalQtyUnit).Returns("ITEM1_STATISTICALQTYUNIT");
			item1Mock.Setup(m => m.StatisticalQty).Returns(1m);
			item1Mock.Setup(m => m.SupplementaryQty).Returns(10m);
			item1Mock.Setup(m => m.OriginCountry).Returns("ITEM1_ORIGINCOUNTRY");
			item1Mock.Setup(m => m.Packaging).Returns(new List<IPackaging>() { iPackaging1Mock.Object, iPackaging2Mock.Object });
			item1Mock.Setup(m => m.UsedGoods).Returns(true);
			item1Mock.Setup(m => m.GeneticallyModified).Returns(true);
			item1Mock.Setup(m => m.SupplementaryQtyUnit).Returns("BDU");
			item1Mock.Setup(m => m.LineDutyTaxFees).Returns(new List<IDutyTaxFee>() { dutyTaxFee1Mock.Object });
			item1Mock.Setup(m => m.Products).Returns(new List<IProduct>() { iProduct1Mock.Object, iProduct2Mock.Object });
			item2Mock = new Mock<IGoodsItems>();
			item2Mock.Setup(m => m.Permits).Returns(new List<ZString>() { "ITEM2_PERMITS1", "ITEM2_PERMITS2" });
			item2Mock.Setup(m => m.ProhibitedCodes).Returns(new List<ZString>() { "ITEM2_PROHIBITEDCODES1", "ITEM2_PROHIBITEDCODES2" });
			item2Mock.Setup(m => m.OtherInfoCodes).Returns(new List<IOtherInfo>() { iOtherInfo1Mock.Object, iOtherInfo2Mock.Object });
			item2Mock.Setup(m => m.GoodsDescription).Returns("ITEM2_GOODSDESCRIPTION");
			item2Mock.Setup(m => m.ValueInForeignCurrency).Returns(6m);
			item2Mock.Setup(m => m.ForeignCurrencyCode).Returns("ITEM2_FOREIGNCURRENCYCODE");
			item2Mock.Setup(m => m.Classifications).Returns(new List<IClassification>() { iClassification1Mock.Object, iClassification2Mock.Object });
			item2Mock.Setup(m => m.BrandName).Returns("ITEM2_BRANDNAME");
			item2Mock.Setup(m => m.CommonName).Returns("ITEM2_COMMONNAME");
			item2Mock.Setup(m => m.RegisteredName).Returns("ITEM2_REGISTEREDNAME");
			item2Mock.Setup(m => m.TradeName).Returns("ITEM2_TRADENAME");
			item2Mock.Setup(m => m.Temperatures).Returns(temperaturesMock.Object);
			item2Mock.Setup(m => m.ContainerNumbers).Returns(new List<ZString>() { "ITEM2_CONTAINERNUMBERS1", "ITEM2_CONTAINERNUMBERS2" });
			item2Mock.Setup(m => m.ItemGrossWeightInKGM).Returns(6m);
			item2Mock.Setup(m => m.ItemNetWeightInKGM).Returns(3m);
			item2Mock.Setup(m => m.StatisticalQtyUnit).Returns("ITEM2_STATISTICALQTYUNIT");
			item2Mock.Setup(m => m.StatisticalQty).Returns(1m);
			item2Mock.Setup(m => m.SupplementaryQty).Returns(10m);
			item2Mock.Setup(m => m.OriginCountry).Returns("ITEM2_ORIGINCOUNTRY");
			item2Mock.Setup(m => m.Packaging).Returns(new List<IPackaging>() { iPackaging1Mock.Object, iPackaging2Mock.Object });
			item2Mock.Setup(m => m.UsedGoods).Returns(true);
			item2Mock.Setup(m => m.GeneticallyModified).Returns(true);
			item2Mock.Setup(m => m.SupplementaryQtyUnit).Returns("BDU");
			item2Mock.Setup(m => m.LineDutyTaxFees).Returns(new List<IDutyTaxFee>() { dutyTaxFee1Mock.Object });
			item2Mock.Setup(m => m.Products).Returns(new List<IProduct>() { iProduct1Mock.Object, iProduct2Mock.Object });
			var iInvoice1Mock = new Mock<IInvoice>();
			iInvoice1Mock.Setup(m => m.InvoiceDate).Returns(new ZDateTime(2018, 11, 7));
			iInvoice1Mock.Setup(m => m.InvoiceNumber).Returns("IINVOICE1_INVOICENUMBER");
			var iInvoice2Mock = new Mock<IInvoice>();
			iInvoice2Mock.Setup(m => m.InvoiceDate).Returns(new ZDateTime(2015, 4, 2));
			iInvoice2Mock.Setup(m => m.InvoiceNumber).Returns("IINVOICE2_INVOICENUMBER");
			notifyParty1Mock = new Mock<IOrganisation>();
			notifyParty1Mock.Setup(m => m.Name).Returns("NOTIFYPARTY1_NAME");
			notifyParty1Mock.Setup(m => m.CustomsClientCode).Returns("NOTIFYPARTY1_CUSTOMSCLIENTCODE");
			notifyParty1Mock.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1Mock.Object, iCommunication2Mock.Object });
			var notifyParty2Mock = new Mock<IOrganisation>();
			notifyParty2Mock.Setup(m => m.Name).Returns("NOTIFYPARTY2_NAME");
			notifyParty2Mock.Setup(m => m.CustomsClientCode).Returns("NOTIFYPARTY2_CUSTOMSCLIENTCODE");
			notifyParty2Mock.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1Mock.Object, iCommunication2Mock.Object });
			var stuffingEstablishment1Mock = new Mock<IOrganisation>();
			stuffingEstablishment1Mock.Setup(m => m.Name).Returns("STUFFINGESTABLISHMENT1_NAME");
			stuffingEstablishment1Mock.Setup(m => m.City).Returns("STUFFINGESTABLISHMENT1_CITY");
			stuffingEstablishment1Mock.Setup(m => m.CountryCode).Returns("STUFFINGESTABLISHMENT1_COUNTRYCODE");
			stuffingEstablishment1Mock.Setup(m => m.CountryRegion).Returns("STUFFINGESTABLISHMENT1_COUNTRYREGION");
			stuffingEstablishment1Mock.Setup(m => m.Address).Returns("STUFFINGESTABLISHMENT1_ADDRESS");
			stuffingEstablishment1Mock.Setup(m => m.PostCode).Returns("123456789");
			var stuffingEstablishment2Mock = new Mock<IOrganisation>();
			stuffingEstablishment2Mock.Setup(m => m.Name).Returns("STUFFINGESTABLISHMENT2_NAME");
			stuffingEstablishment2Mock.Setup(m => m.City).Returns("STUFFINGESTABLISHMENT2_CITY");
			stuffingEstablishment2Mock.Setup(m => m.CountryCode).Returns("STUFFINGESTABLISHMENT2_COUNTRYCODE");
			stuffingEstablishment2Mock.Setup(m => m.CountryRegion).Returns("STUFFINGESTABLISHMENT2_COUNTRYREGION");
			stuffingEstablishment2Mock.Setup(m => m.Address).Returns("STUFFINGESTABLISHMENT2_ADDRESS");
			stuffingEstablishment2Mock.Setup(m => m.PostCode).Returns("123456789");
			goodsShipmentMock = new Mock<IGoodsShipment>();
			goodsShipmentMock.Setup(m => m.NatureOfTransaction).Returns("GOODSSHIPMENT_NATUREOFTRANSACTION");
			goodsShipmentMock.Setup(m => m.LocationOfGoods).Returns("GOODSSHIPMENT_LOCATIONOFGOODS");
			goodsShipmentMock.Setup(m => m.PortOfLoading).Returns("GOODSSHIPMENT_PORTOFLOADING");
			goodsShipmentMock.Setup(m => m.PortOfDischarge).Returns("GOODSSHIPMENT_PORTOFDISCHARGE");
			goodsShipmentMock.Setup(m => m.Items).Returns(new List<IGoodsItems>() { item1Mock.Object, item2Mock.Object });
			goodsShipmentMock.Setup(m => m.Invoices).Returns(new List<IInvoice>() { iInvoice1Mock.Object, iInvoice2Mock.Object });
			goodsShipmentMock.Setup(m => m.NotifyParties).Returns(new List<IOrganisationSimple>() { notifyParty1Mock.Object, notifyParty2Mock.Object });
			goodsShipmentMock.Setup(m => m.StuffingEstablishments).Returns(new List<IOrganisation>() { stuffingEstablishment1Mock.Object, stuffingEstablishment2Mock.Object });
			goodsShipmentMock.Setup(m => m.CustomsControlledArea).Returns("GOODSSHIPMENT_CUSTOMSCONTROLLEDAREA");
			goodsShipmentMock.Setup(m => m.NotifyPartyCodes).Returns(new ZString[] { "1111A", "ARRYN" });
			var deliverToPartyMock = new Mock<IOrganisation>();
			deliverToPartyMock.Setup(m => m.Name).Returns("DELIVERTOPARTY_NAME");
			deliverToPartyMock.Setup(m => m.City).Returns("DELIVERTOPARTY_CITY");
			deliverToPartyMock.Setup(m => m.CountryCode).Returns("DELIVERTOPARTY_COUNTRYCODE");
			deliverToPartyMock.Setup(m => m.CountryRegion).Returns("DELIVERTOPARTY_COUNTRYREGION");
			deliverToPartyMock.Setup(m => m.Address).Returns("DELIVERTOPARTY_ADDRESS");
			deliverToPartyMock.Setup(m => m.PostCode).Returns("123456789");
			goodsShipmentMock.Setup(m => m.DeliverToParty).Returns(deliverToPartyMock.Object);
			iExportDeclarationMock.Setup(m => m.GoodsShipment).Returns(goodsShipmentMock.Object);
			var houseBillChild1Mock = new Mock<IAssociatedTransportDocument>();
			PopulateChildHouseBillMock(houseBillChild1Mock, "1");
			var houseBillChild2Mock = new Mock<IAssociatedTransportDocument>();
			PopulateChildHouseBillMock(houseBillChild2Mock, "2");
			var houseBillChild3Mock = new Mock<IAssociatedTransportDocument>();
			PopulateChildHouseBillMock(houseBillChild3Mock, "3");
			var houseBillChild4Mock = new Mock<IAssociatedTransportDocument>();
			PopulateChildHouseBillMock(houseBillChild4Mock, "4");
			var houseBillChild5Mock = new Mock<IAssociatedTransportDocument>();
			PopulateChildHouseBillMock(houseBillChild5Mock, "5");
			var houseBillChild6Mock = new Mock<IAssociatedTransportDocument>();
			PopulateChildHouseBillMock(houseBillChild6Mock, "6");
			var houseBillChild7Mock = new Mock<IAssociatedTransportDocument>();
			PopulateChildHouseBillMock(houseBillChild7Mock, "7");
			var houseBillChild8Mock = new Mock<IAssociatedTransportDocument>();
			PopulateChildHouseBillMock(houseBillChild8Mock, "8");
			houseBill1Mock = new Mock<IMasterBillTransportDocument>();
			PopulateHouseBillMock(houseBill1Mock, "1", houseBillChild1Mock.Object.PK, houseBillChild2Mock.Object.PK);
			houseBill2Mock = new Mock<IMasterBillTransportDocument>();
			PopulateHouseBillMock(houseBill2Mock, "2", houseBillChild3Mock.Object.PK, houseBillChild4Mock.Object.PK);
			houseBill3Mock = new Mock<IMasterBillTransportDocument>();
			PopulateHouseBillMock(houseBill3Mock, "3", houseBillChild5Mock.Object.PK, houseBillChild6Mock.Object.PK);
			houseBill4Mock = new Mock<IMasterBillTransportDocument>();
			PopulateHouseBillMock(houseBill4Mock, "4", houseBillChild7Mock.Object.PK, houseBillChild8Mock.Object.PK);
			iExportDeclarationMock.Setup(m => m.AllBills).Returns(new List<IAssociatedTransportDocument>() { houseBill1Mock.Object, houseBill2Mock.Object, houseBill3Mock.Object, houseBill4Mock.Object });
			masterBill1Mock = new Mock<IMasterBillTransportDocument>();
			PopulateMasterBillMock(masterBill1Mock, "1", houseBill1Mock.Object.PK, houseBill2Mock.Object.PK);
			masterBill2Mock = new Mock<IMasterBillTransportDocument>();
			PopulateMasterBillMock(masterBill2Mock, "2", houseBill3Mock.Object.PK, houseBill4Mock.Object.PK);
			iExportDeclarationMock.Setup(m => m.MasterBills).Returns(new List<IMasterBillTransportDocument>() { masterBill1Mock.Object, masterBill2Mock.Object });
			ex1Builder = new EX1MessageBuilder(iExportDeclarationMock.Object, TSWTransactionTypes.Original);
			embeddedResourceRetriever = new EmbeddedResourceRetriever();
		}

		EX1MessageBuilder ex1Builder;
		Mock<IExportDeclaration> iExportDeclarationMock;
		Mock<IAdditionalInformation> iAdditionalInformationMock;
		Mock<IOtherInfo> iOtherInfo1Mock;
		Mock<IDutyTaxFee> dutyTaxFee1Mock;
		Mock<IDutyTaxFee> dutyTaxFee2Mock;
		Mock<IGoodsShipment> goodsShipmentMock;
		Mock<IOrganisation> importerMock;
		Mock<IMasterBillTransportDocument> houseBill1Mock;
		Mock<IMasterBillTransportDocument> houseBill2Mock;
		Mock<IMasterBillTransportDocument> houseBill3Mock;
		Mock<IMasterBillTransportDocument> houseBill4Mock;
		Mock<IMasterBillTransportDocument> masterBill1Mock;
		Mock<IMasterBillTransportDocument> masterBill2Mock;
		Mock<ITransportEquipment> container1Mock;
		Mock<ICommunication> iCommunication1Mock;
		Mock<IOrganisation> notifyParty1Mock;
		Mock<IGoodsItems> item1Mock;
		Mock<IGoodsItems> item2Mock;
		Mock<IProduct> iProduct1Mock;
		EmbeddedResourceRetriever embeddedResourceRetriever;

		void PopulateChildHouseBillMock(Mock<IAssociatedTransportDocument> houseBillChildMock, ZString number)
		{
			houseBillChildMock.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			houseBillChildMock.Setup(m => m.BillNumber).Returns($"HOUSEBILLCHILD{number}_BILLNUMBER");
			houseBillChildMock.Setup(m => m.BillType).Returns($"HOUSEBILLCHILD{number}_BILLTYPE");
			houseBillChildMock.Setup(m => m.MessageSequence).Returns(new ZInt(number));
		}

		void PopulateHouseBillMock(Mock<IMasterBillTransportDocument> houseBillMock, ZString number, ZGuid houseBillChild1PK, ZGuid houseBillChild2PK)
		{
			houseBillMock.Setup(m => m.BillNumber).Returns($"HOUSEBILL{number}_BILLNUMBER");
			houseBillMock.Setup(m => m.BillType).Returns($"HOUSEBILL{number}_BILLTYPE");
			houseBillMock.Setup(m => m.MessageSequence).Returns(new ZInt(number));
			houseBillMock.Setup(m => m.RelatedEquipment).Returns(new List<ZGuid>() { ZGuid.NewZGuid(), ZGuid.NewZGuid() });
			houseBillMock.Setup(m => m.RelatedPackages).Returns(new List<ZGuid>() { ZGuid.NewZGuid(), ZGuid.NewZGuid() });
			houseBillMock.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			houseBillMock.Setup(m => m.ChildBills).Returns(new List<ZGuid>() { houseBillChild1PK, houseBillChild2PK });
		}

		void PopulateMasterBillMock(Mock<IMasterBillTransportDocument> masterBillMock, ZString number, ZGuid houseBill1PK, ZGuid houseBill2PK)
		{
			masterBillMock.Setup(m => m.BillNumber).Returns($"MASTERBILL{number}_BILLNUMBER");
			masterBillMock.Setup(m => m.BillType).Returns($"MASTERBILL{number}_BILLTYPE");
			masterBillMock.Setup(m => m.ChildBills).Returns(new List<ZGuid>() { houseBill1PK, houseBill2PK });
			masterBillMock.Setup(m => m.RelatedPackages).Returns(new List<ZGuid>() { ZGuid.NewZGuid(), ZGuid.NewZGuid() });
			masterBillMock.Setup(m => m.RelatedEquipment).Returns(new List<ZGuid>() { ZGuid.NewZGuid(), ZGuid.NewZGuid() });
			masterBillMock.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
		}
	}
}
