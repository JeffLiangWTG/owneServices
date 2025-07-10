using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterpise.Customs.TW.DataTransfer.Testing
{
	[TestedType(typeof(TWJobDeclarationDataObjectReader))]
	sealed class TWJobDeclarationDataObjectReaderTest : DataObjectReaderTest
	{
		[ExpectNoExceptions]
		public void TestFillBondedFactories()
		{
			var newFactory = new BusinessObjectFactory();
			var dataObjectReaderTestHelper = new DataObjectReaderTestHelper(new UniversalObjectFactory(newFactory));
			var org1 = dataObjectReaderTestHelper.CreateOrganisation("Bonded Factory Name1", "XX1");
			var mainAddress1 = org1.Addresses.MainAddress;
			mainAddress1.AddressCode = "Address Code 1";
			mainAddress1.Address1 = "115 TI YU CHANG ROAD";
			mainAddress1.Address2 = "FANGSHAN DISTRICT 1";
			var org2 = dataObjectReaderTestHelper.CreateOrganisation("Bonded Factory Name2", "XX2");
			var mainAddress2 = org2.Addresses.MainAddress;
			mainAddress2.AddressCode = "Address Code 2";
			mainAddress2.Address1 = "116 TI YU CHANG ROAD";
			mainAddress2.Address2 = "FANGSHAN DISTRICT 2";
			var org3 = dataObjectReaderTestHelper.CreateOrganisation("Bonded Factory Name3", "XX3");
			var mainAddress3 = org3.Addresses.MainAddress;
			mainAddress3.AddressCode = "Address Code 3";
			mainAddress3.Address1 = "117 TI YU CHANG ROAD";
			mainAddress3.Address2 = "FANGSHAN DISTRICT 3";
			newFactory.Save();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Enterprise.Core.Constants.CountryCodes.Taiwan))
			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = "IMP";
				declaration.JE_MessageSubType = "CUS";
				declaration.JE_ApplicationCode = "BLT";
				var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
				var declarationData = SetupDeclaration(ZString.Empty, ZString.Empty);
				var organizationAddress1 = declarationData.AddOrgAddress(writeManager, org1, Enterprise.Customs.TW.DataTransfer.Constants.AddressTypes.PreviousBondedFactory);
				var organizationAddress2 = declarationData.AddOrgAddress(writeManager, org2, Enterprise.Customs.TW.DataTransfer.Constants.AddressTypes.PreviousBondedFactory);
				organizationAddress2.AddressOverride = true;
				var organizationAddress3 = declarationData.AddOrgAddress(writeManager, org3, Enterprise.Customs.TW.DataTransfer.Constants.AddressTypes.PreviousBondedFactory);
				var reader = new TWJobDeclarationDataObjectReader(declarationData, logger, Factory);
				var declarationResult = reader.ReadIntoBusinessObject();
				var bondedFactories = declarationResult.BondedFactories;
				NUnit.Framework.Assert.That(bondedFactories.Count, Is.EqualTo(2));
				NUnit.Framework.Assert.That(bondedFactories[0].OrganisationPK, Is.EqualTo(org1.PK), "PreviousBondedFactory1 OrganisationPK");
				NUnit.Framework.Assert.That(bondedFactories[0].E2_OA_Address, Is.EqualTo(mainAddress1.PK), "PreviousBondedFactory1 E2_OA_Address");
				NUnit.Framework.Assert.That(bondedFactories[1].OrganisationPK, Is.EqualTo(org3.PK), "PreviousBondedFactory2 OrganisationPK");
				NUnit.Framework.Assert.That(bondedFactories[1].E2_OA_Address, Is.EqualTo(mainAddress3.PK), "PreviousBondedFactory2 E2_OA_Address");
			}
		}

		[ExpectNoExceptions]
		public void TestCustomsEntryInstructionDataObjectReader()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_MessageSubType = "CUS";
			declaration.JE_ApplicationCode = "BLT";
			var declarationData = SetupDeclaration(ZString.Empty, ZString.Empty);
			var reader = new TWJobDeclarationDataObjectReaderForTest(declarationData, logger, Factory);
			NUnit.Framework.Assert.That(reader.GetCustomsEntryInstructionDataObjectReader(new EntryInstruction(), declaration), Is.TypeOf<TWEntryInstructionDataObjectReader>());
		}

		[ExpectNoExceptions]
		public void TestInvoiceHeaderDataObjectReader()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_MessageSubType = "CUS";
			declaration.JE_ApplicationCode = "BLT";
			var declarationData = SetupDeclaration(ZString.Empty, ZString.Empty);
			var reader = new TWJobDeclarationDataObjectReaderForTest(declarationData, logger, Factory);
			NUnit.Framework.Assert.That(reader.GetInvoiceHeaderDataObjectReader(declaration.JobComInvoiceGroupHeaders[0], new CommercialInvoiceHeader(), declarationData, null), Is.TypeOf<TWInvoiceHeaderDataObjectReader>());
		}

		[ExpectNoExceptions]
		public void TestFillOrganizationsCore()
		{
			var notifyParty = CreateOrganisation("NotifyParty 1", "NTP1");
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Enterprise.Core.Constants.CountryCodes.Taiwan))
			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = "IMP";
				declaration.JE_MessageSubType = "CUS";
				declaration.JE_ApplicationCode = "BLT";
				var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
				var declarationData = SetupDeclaration(ZString.Empty, ZString.Empty);
				declarationData.AddOrgAddress(writeManager, notifyParty, "NotifyParty");
				var reader = new TWJobDeclarationDataObjectReader(declarationData, logger, Factory);
				var result = reader.ReadIntoBusinessObject();
				NUnit.Framework.Assert.That(result.JE_OH_NotifyParty, Is.EqualTo(notifyParty.PK), "NotifyParty");
			}
		}

		Shipment SetupDeclaration(ZString messageType, ZString messageSubType)
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
			return new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				MessageType = new CodeDescriptionPair()
				{ Code = messageType },
				MessageSubType = new CodeDescriptionPair()
				{ Code = messageSubType }
			};
		}

		[ExpectNoExceptions]
		public void TestPopulateAdditionalInfoForAdditionalBill()
		{
			using (TWCustomsDataRegistry.Instance.EnableCustomsDeclarationPackingList.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Enterprise.Core.Constants.CountryCodes.Taiwan))
			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = "IMP";
				declaration.JE_MessageSubType = "CUS";
				declaration.JE_ApplicationCode = "BLT";
				var declarationData = SetupDeclaration(ZString.Empty, ZString.Empty);
				declarationData.WayBillType = new WayBillType { Code = "MWB" };
				declarationData.WayBillNumber = "MWB001";
				declarationData.SetAdditionalBillCollection(() => new List<AdditionalBill> { new AdditionalBill { BillType = new WayBillType { Code = "CNN" }, BillNumber = "AAAAAAAA" } });
				declarationData.SetPackingLineCollection(() => new DataObjectList<PackingLine> { new PackingLine { BillType = new WayBillType { Code = "CNN" }, BillNumber = "AAAAAAAA" } });
				var reader = new TWJobDeclarationDataObjectReader(declarationData, logger, Factory);
				var result = reader.ReadIntoBusinessObject();
				Factory.SaveForTesting();
				var newDeclarationBO = new BusinessObjectFactory().Load<JobDeclaration>(result.PK);
				NUnit.Framework.Assert.That(newDeclarationBO.Bills.Count, Is.EqualTo(2));
				var bill = newDeclarationBO.Bills[0];
				NUnit.Framework.Assert.That(newDeclarationBO.Bills.Cast<Enterprise.Customs.TW.Business.Bill>().Any(x => x.CU_BillType == "CN" && x.CU_BillNum == "AAAAAAAA"), Is.True);
				NUnit.Framework.Assert.That(newDeclarationBO.Bills.Cast<Enterprise.Customs.TW.Business.Bill>().Any(x => x.CU_BillType == "MB" && x.CU_BillNum == "MWB001"), Is.True);
				NUnit.Framework.Assert.That(newDeclarationBO.JE_MasterBill, Is.EqualTo("MWB001").Using(CustomComparers.TypeComparison));
			}
		}

		[ExpectNoExceptions]
		public void TestFillSupplierWhenAddressOverrideIsTrue()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Enterprise.Core.Constants.CountryCodes.Taiwan))
			{
				var message = GetQueuedUniversalShipmentMessage(Enterprise.Customs.TW.DataTransfer.Constants.ShipmentDeclarationWithSupplierImporterDocumentaryDocAddressAndTheAddressOverrideIsTrue);
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);
				var declaration = Factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001000"));
				var supplierDocumentaryAddress = declaration.SupplierDocumentaryAddress;
				NUnit.Framework.Assert.That(supplierDocumentaryAddress.E2_AddressOverride, Is.EqualTo(true).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(supplierDocumentaryAddress.E2_Address1, Is.EqualTo("ADDRESS 121").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(supplierDocumentaryAddress.E2_Address2, Is.EqualTo("ADDRESS 2223").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(supplierDocumentaryAddress.E2_AdditionalAddressInformation, Is.EqualTo("FFF1222").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(supplierDocumentaryAddress.E2_City, Is.EqualTo("SYN").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(supplierDocumentaryAddress.E2_CompanyName, Is.EqualTo("4B ELEVATOR COMPONENTS LIMITED").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(supplierDocumentaryAddress.E2_Contact, Is.EqualTo("JOHN CHATFIELD").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(supplierDocumentaryAddress.Country.Code, Is.EqualTo("TW").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(supplierDocumentaryAddress.E2_Email, Is.EqualTo("XXT1@234.COM").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(supplierDocumentaryAddress.E2_Phone, Is.EqualTo("+61225253235").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(supplierDocumentaryAddress.E2_State, Is.EqualTo("NSW").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(supplierDocumentaryAddress.E2_Postcode, Is.EqualTo("000000").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(supplierDocumentaryAddress.IDCodeType, Is.EqualTo("VAT").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(supplierDocumentaryAddress.IDCode, Is.EqualTo("12348881").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(supplierDocumentaryAddress.AEOCodeType, Is.EqualTo("AEO").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(supplierDocumentaryAddress.AEOCode, Is.EqualTo("123456").Using(CustomComparers.TypeComparison));
				var localAddress = supplierDocumentaryAddress.LocalAddress;
				NUnit.Framework.Assert.That(localAddress.E2_Address1, Is.EqualTo("測試地址1 LOCAL1").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(localAddress.E2_Address2, Is.EqualTo("測試地址2").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(localAddress.E2_AdditionalAddressInformation, Is.EqualTo("額外地址 LOCAL").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(localAddress.E2_City, Is.EqualTo("TAIPEI").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(localAddress.E2_CompanyName, Is.EqualTo("MXXX 測試3").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(localAddress.Country.Code, Is.EqualTo("TW").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(localAddress.E2_Postcode, Is.EqualTo("105").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(localAddress.E2_State, Is.EqualTo("QLD").Using(CustomComparers.TypeComparison));
			}
		}

		[ExpectNoExceptions]
		public void TestFillImporterWhenAddressOverrideIsTrue()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Enterprise.Core.Constants.CountryCodes.Taiwan))
			{
				var message = GetQueuedUniversalShipmentMessage(Enterprise.Customs.TW.DataTransfer.Constants.ShipmentDeclarationWithSupplierImporterDocumentaryDocAddressAndTheAddressOverrideIsTrue);
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);
				var declaration = Factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001000"));
				var importerDocumentaryAddress = declaration.ImporterDocumentaryAddress;
				NUnit.Framework.Assert.That(importerDocumentaryAddress.E2_AddressOverride, Is.EqualTo(true).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(importerDocumentaryAddress.E2_Address1, Is.EqualTo("TAIBEI MINSHENG3").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(importerDocumentaryAddress.E2_Address2, Is.EqualTo("NO.92").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(importerDocumentaryAddress.E2_AdditionalAddressInformation, Is.EqualTo("TAIPEI3").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(importerDocumentaryAddress.E2_City, Is.EqualTo("LAX").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(importerDocumentaryAddress.E2_CompanyName, Is.EqualTo("WISETECH GLOBAL4").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(importerDocumentaryAddress.E2_Contact, Is.EqualTo("THE IMPORT MANAGER1").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(importerDocumentaryAddress.Country.Code, Is.EqualTo("US").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(importerDocumentaryAddress.E2_Email, Is.EqualTo("XXX@SW2.COM").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(importerDocumentaryAddress.E2_Phone, Is.EqualTo("156591685250").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(importerDocumentaryAddress.E2_State, Is.EqualTo("AL").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(importerDocumentaryAddress.E2_Postcode, Is.EqualTo("106").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(importerDocumentaryAddress.IDCodeType, Is.EqualTo("VAT").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(importerDocumentaryAddress.IDCode, Is.EqualTo("11122225").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(importerDocumentaryAddress.AEOCodeType, Is.EqualTo("AEO").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(importerDocumentaryAddress.AEOCode, Is.EqualTo("1112585222").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(importerDocumentaryAddress.TPCCodeType, Is.EqualTo("TPC").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(importerDocumentaryAddress.TPCCode, Is.EqualTo("344455").Using(CustomComparers.TypeComparison));
				var localAddress = importerDocumentaryAddress.LocalAddress;
				NUnit.Framework.Assert.That(localAddress.E2_Address1, Is.EqualTo("台北市民生東路4段133號3F-2").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(localAddress.E2_Address2, Is.EqualTo("XX2").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(localAddress.E2_AdditionalAddressInformation, Is.EqualTo("XX54").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(localAddress.E2_City, Is.EqualTo("鎮4").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(localAddress.E2_CompanyName, Is.EqualTo("慧咨環球1").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(localAddress.Country.Code, Is.EqualTo("TW").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(localAddress.E2_Postcode, Is.EqualTo("503").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(localAddress.E2_State, Is.EqualTo("CYQ").Using(CustomComparers.TypeComparison));
			}
		}

		[ExpectNoExceptions]
		public void TestPopulateJobDocAddressNumbers()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Enterprise.Core.Constants.CountryCodes.Taiwan))
			{
				var message = GetQueuedUniversalShipmentMessage(Enterprise.Customs.TW.DataTransfer.Constants.ShipmentDeclarationWithSupplierImporterDocumentaryDocAddressAndTheAddressOverrideIsTrue);
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);
				var declaration = Factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001000"));
				var importerDocumentaryAddress = declaration.ImporterDocumentaryAddress;
				NUnit.Framework.Assert.That(importerDocumentaryAddress.IDCodeType, Is.EqualTo("VAT").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(importerDocumentaryAddress.IDCode, Is.EqualTo("11122225").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(importerDocumentaryAddress.AEOCodeType, Is.EqualTo("AEO").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(importerDocumentaryAddress.AEOCode, Is.EqualTo("1112585222").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(importerDocumentaryAddress.TPCCodeType, Is.EqualTo("TPC").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(importerDocumentaryAddress.TPCCode, Is.EqualTo("344455").Using(CustomComparers.TypeComparison));
			}
		}

		[ExpectNoExceptions]
		public void TestFillUCRNumber()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Enterprise.Core.Constants.CountryCodes.Taiwan))
			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var declarationData = SetupDeclaration("EXP", ZString.Empty);
				declarationData.SetEntryNumberCollection(() => new List<Enterprise.UniversalDataBuss.DataObjects.Universal.EntryNumber>());
				declarationData.EntryNumberCollection.Add(new Enterprise.UniversalDataBuss.DataObjects.Universal.EntryNumber()
				{
					Number = "UCRNumber1",
					Type = new EntryType() { Code = CusEntryNumberTypes.Standard.UniqueConsignementReference },
					EntryIsSystemGenerated = false
				});

				var reader = new TWJobDeclarationDataObjectReader(declarationData, logger, Factory);
				var result = reader.ReadIntoBusinessObject();
				CombineAssertions(() =>
				{
					NUnit.Framework.Assert.That(result.CusEntryInstruction.UCRNumber, Is.EqualTo("UCRNumber1").Using(CustomComparers.TypeComparison));
					NUnit.Framework.Assert.That(result.CusEntryInstruction.UCROverride, Is.EqualTo(true).Using(CustomComparers.TypeComparison));
				});
			}
		}

		class TWJobDeclarationDataObjectReaderForTest : TWJobDeclarationDataObjectReader
		{
			internal TWJobDeclarationDataObjectReaderForTest(Shipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment shipment = null) : base(declarationDataObject, logger, factory, shipment)
			{
			}

			public CustomsEntryInstructionDataObjectReader GetCustomsEntryInstructionDataObjectReader(EntryInstruction entryInstructionDataObject, JobDeclaration declaration)
			{
				return CreateCustomsEntryInstructionDataObjectReader(entryInstructionDataObject, declaration);
			}

			public CommercialInvoiceHeaderDataObjectReader<JobComInvoiceGroupHeader> GetInvoiceHeaderDataObjectReader(JobComInvoiceGroupHeader groupHeader, CommercialInvoiceHeader invoiceData, Shipment dataObject, ILandedCostDataReader landedCostDataReader)
			{
				return base.CreateNewCommercialInvoiceHeaderDataObjectReader(groupHeader, invoiceData, dataObject, landedCostDataReader);
			}
		}
	}
}
