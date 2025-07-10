using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.Testing;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing;
using Enterprise.Customs.Common.SG;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.Customs.SG.Access.Business;
using Enterprise.Customs.SG.Registry;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.ZArchitecture.Schema;
using AddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.SG.V4.DataTransfer.Universal.Testing
{
	partial class DeclarationDataObjectReaderForGlobalManifestTest : AsycudaWriterTest
	{
		public void TestCreateCustomsDeclarationForOrganization()
		{
			PrepareCusCodeDataForTesting();
			Factory.SaveForTesting();

			var header = AsycudaManifestHeaderTestHelper.CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("SGVLI", "MGE", Factory.BOFactory);
			header.AMA_ManifestType = SGManifestTypes.Codes.MGE;
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_MasterBill = "MB2";
			var bill = header.Bills[0];
			Factory.SaveForTesting();

			var declaration = bill.CreateCustomsDeclaration() as JobDeclaration;
			var shipperPK = bill.Shipper.OA_OH;
			AssertEquals(shipperPK, declaration.JE_OH_Supplier);
			AssertEquals(shipperPK, declaration.JE_OH_Exporter);

			header.AMA_ManifestType = "MGI";
			Factory.SaveForTesting();
			declaration = bill.CreateCustomsDeclaration() as JobDeclaration;
			var consigneePK = bill.Consignee.OA_OH;
			AssertEquals(consigneePK, declaration.JE_OH_Importer);
			AssertEquals(consigneePK, declaration.JE_OH_Consignee);
		}

		protected override void SetUp()
		{
			registryDisposable = SGCustomsDataRegistry.Instance.ACCESSEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			base.SetUp();
		}

		protected override void TearDown()
		{
			base.TearDown();
			registryDisposable.Dispose();
		}

		IDisposable registryDisposable;
	}

	partial class DeclarationDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestFillAddInfoValuesWithMappingKey()
		{
			declarationDataObject.SetAddInfoCollection(() => new List<AddInfo>()
			{
				new AddInfo() { Key = Business.AddInfo.GetDBNameMapped(SGAddInfoSchema.Constants.SG_OutwardTransportMode), Value = "AIR" },
				new AddInfo() { Key = Business.AddInfo.GetDBNameMapped(SGAddInfoSchema.Constants.SG_OutwardHAWB), Value = "OHB20201021001" },
				new AddInfo() { Key = Business.AddInfo.GetDBNameMapped(SGAddInfoSchema.Constants.SG_OutwardMAWB), Value = "OMB20201021002" },
				new AddInfo() { Key = Business.AddInfo.GetDBNameMapped(SGAddInfoSchema.Constants.SG_OutwardVesselName), Value = "Safmarine Nahoon" },
				new AddInfo() { Key = Business.AddInfo.GetDBNameMapped(SGAddInfoSchema.Constants.SG_OutwardVoyageFlightNo), Value = "QF20201022" },
			});

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals("SG_OutwardHAWB", "OHB20201021001", declarationBO.SG_OutwardHAWB);
				AssertEquals("SG_OutwardMAWB", "OMB20201021002", declarationBO.SG_OutwardMAWB);
				AssertEquals("SG_OutwardVesselName", "Safmarine Nahoon", declarationBO.SG_OutwardVesselName);
				AssertEquals("SG_OutwardVoyageFlightNo", "QF20201022", declarationBO.SG_OutwardVoyageFlightNo);
				AssertEquals("SG_OutwardTransportMode", "AIR", declarationBO.SG_OutwardTransportMode);
			});
		}

		public void TestOrganizations()
		{
			var importer = CreateOrganisation("IMPORTER", "IMPTEST");
			var supplier = CreateOrganisation("SUPPLIER", "SUPTEST");
			var buyer = CreateOrganisation("BUYER", "BUYTEST");
			var claimant = CreateOrganisation("CLAIMANT", "CLAIMTEST");
			var handlingAgent = CreateOrganisation("HANDLING AGENT", "HNDAGTTEST");
			var inwardCarrierAgent = CreateOrganisation("INWARD CARRIER AGENT", "INWDTEST");
			var outwardCarrierAgent = CreateOrganisation("OUTWARD CARRIER AGENT", "OUTWDTEST");

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			var declarationDataObject = SetupDeclaration(null, "AMASTER", new WayBillType() { Code = WayBillTypeList.Codes.Master, Description = WayBillTypeList.Descriptions.Master });
			declarationDataObject.AddOrgAddress(writeManager, importer, nameof(DocAddressType.ImporterDocumentaryAddress));
			declarationDataObject.AddOrgAddress(writeManager, supplier, nameof(DocAddressType.SupplierDocumentaryAddress));
			declarationDataObject.AddOrgAddress(writeManager, buyer, nameof(DocAddressType.BuyerDocumentaryAddress));
			declarationDataObject.AddOrgAddress(writeManager, claimant, nameof(DocAddressType.ClaimantAddress));
			declarationDataObject.AddOrgAddress(writeManager, handlingAgent, nameof(DocAddressType.CarrierHandlingAgent));
			declarationDataObject.AddOrgAddress(writeManager, inwardCarrierAgent, nameof(DocAddressType.InwardCarrierAgent));
			declarationDataObject.AddOrgAddress(writeManager, outwardCarrierAgent, nameof(DocAddressType.OutwardCarrierAgent));

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals(importer.PK, declarationBO.JE_OH_Importer);
			AssertEquals(supplier.PK, declarationBO.JE_OH_Supplier);
			AssertEquals(buyer.PK, declarationBO.JE_OH_Buyer);
			AssertEquals(claimant.PK, declarationBO.JE_OH_Claimant);
			AssertEquals(handlingAgent.PK, declarationBO.JE_OH_HandlingAgent);
			AssertEquals(inwardCarrierAgent.PK, declarationBO.JE_OH_InwardCarrierAgent);
			AssertEquals(outwardCarrierAgent.PK, declarationBO.OutwardShippingLineForwarderPK);
		}

		public void TestFillTradersRemarks()
		{
			var type = new CodeDescriptionPair()
			{
				Code = CusSupportingInfoTypeList.Codes.TradersRemarks,
				Description = CusSupportingInfoTypeList.Descriptions.TradersRemarks
			};
			declarationDataObject.SetCustomsReferenceCollection(() => new List<CustomsReference>()
			{
				new CustomsReference()
				{
					Type = type,
					Reference = "TEST TRADER REMARKS 1",
					Order = 1,
				},
				new CustomsReference()
				{
					Type = type,
					Reference = "TEST TRADER REMARKS 2",
					Order = 2,
				}
			});

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals(2, declarationBO.TradersRemarks.Count);
			AssertEquals("TEST TRADER REMARKS 1", declarationBO.TradersRemarks[0].CSI_Description);
			AssertEquals(1, declarationBO.TradersRemarks[0].CSI_LineNo);
			AssertEquals("TEST TRADER REMARKS 2", declarationBO.TradersRemarks[1].CSI_Description);
			AssertEquals(2, declarationBO.TradersRemarks[1].CSI_LineNo);
		}

		public void TestFillAddInfoUpdatesNote()
		{
			var declarationToLoad = Factory.New<JobDeclaration>();
			var tradersRemarks = declarationToLoad.TradersRemarks.AddNew();
			tradersRemarks.CSI_Description = "EXISTING REMARKS TO BE OVERWRITTEN";
			tradersRemarks.CSI_LineNo = 1;
			Factory.SaveForTesting();

			AssertEquals("EXISTING REMARKS TO BE OVERWRITTEN", declarationToLoad.TradersRemarks[0].CSI_Description);
			declarationDataObject.DataContext.DataTargetCollection.First().Key = declarationToLoad.JE_DeclarationReference;
			declarationDataObject.SetCustomsReferenceCollection(() => new List<CustomsReference>()
			{
				new CustomsReference()
				{
					Type = new CodeDescriptionPair()
					{
						Code = CusSupportingInfoTypeList.Codes.TradersRemarks,
						Description = CusSupportingInfoTypeList.Descriptions.TradersRemarks
					},
					Reference = "NEW UPDATED REMARKS",
					Order = 1,
				}
			});
			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals(1, declarationBO.TradersRemarks.Count);
			AssertEquals("NEW UPDATED REMARKS", declarationBO.TradersRemarks[0].CSI_Description);
			AssertEquals(1, declarationBO.TradersRemarks[0].CSI_LineNo);
			Factory.SaveForTesting();
			declarationToLoad.Reload();
			AssertEquals(1, declarationBO.TradersRemarks.Count);
			AssertEquals("NEW UPDATED REMARKS", declarationBO.TradersRemarks[0].CSI_Description);
			AssertEquals(1, declarationBO.TradersRemarks[0].CSI_LineNo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declarationDataObject = SetupDeclaration(ZString.Empty, ZString.Empty);
		}
		Shipment declarationDataObject;
	}
}
