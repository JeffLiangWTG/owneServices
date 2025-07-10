using System.IO;
using System.Reflection;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.Customs.ZA.Business;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.CodeMapping;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.UniversalDataBuss.XmlIO.XmlReading;
using Enterprise.ZArchitecture.Schema;
using Shipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.ZA.DataTransfer.Universal.Testing
{
	sealed class AsycudaManifestHeaderDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestReaderDataObject_New()
		{
			var factory = new BusinessObjectFactory();
			AsycudaManifestHeaderDataObjectWriterTest.CreateOrganisation(factory, "1", "CARRIER");
			AsycudaManifestHeaderDataObjectWriterTest.CreateOrganisation(factory, "2", "DECONSOLIDATOR");
			AsycudaManifestHeaderDataObjectWriterTest.CreateOrganisation(factory, "3", "DISCHARGE");
			AsycudaManifestHeaderDataObjectWriterTest.CreateOrganisation(factory, "4", "AGENT");
			factory.Save();
			var headerCount = AsycudaManifestHeaderCount;
			var shipmentData = LoadShipmentFromXmlFile(Factory.BOFactory, GetSampleZAOutTurnUniversalXml_NEW);
			var reader = new AsycudaManifestHeaderDataObjectReader(shipmentData, Logger, Factory);
			reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();
			AssertUpdatedAsycudaManifestHeader("OGM0000001", "1");
			AssertEquals("Should create 1 header", headerCount + 1, AsycudaManifestHeaderCount);
		}

		public void TestReaderDataObject_Update()
		{
			var factory1 = new BusinessObjectFactory();
			var zaCompany = AsycudaManifestHeaderDataObjectWriterTest.CreateZABranch(factory1);
			factory1.Save();
			using (Enterprise.Environment.DisposableEnvironment.ForCompany(zaCompany.GC_Code))
			{
				var factory = new BusinessObjectFactory();
				AsycudaManifestHeaderDataObjectWriterTest.CreateOrganisation(factory, "1", "CARRIER");
				AsycudaManifestHeaderDataObjectWriterTest.CreateOrganisation(factory, "2", "DECONSOLIDATOR");
				AsycudaManifestHeaderDataObjectWriterTest.CreateOrganisation(factory, "3", "DISCHARGE");
				AsycudaManifestHeaderDataObjectWriterTest.CreateOrganisation(factory, "4", "AGENT");
				var bo = AsycudaManifestHeaderDataObjectWriterTest.CreateAsycudaManifestHeader(factory, "OGM0000001", "9");
				factory.Save();
				AssertUpdatedAsycudaManifestHeader("OGM0000001", "9");
			}

			var headerCount = AsycudaManifestHeaderCount;
			var shipmentData = LoadShipmentFromXmlFile(Factory.BOFactory, GetSampleZAOutTurnUniversalXml_UPDATE);
			var reader = new AsycudaManifestHeaderDataObjectReader(shipmentData, Logger, Factory);
			reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();
			AssertUpdatedAsycudaManifestHeader("OGM0000001", "1");
			AssertEquals("Should create 1 header", headerCount, AsycudaManifestHeaderCount);
		}

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReaderDataObjectDoesntUpdateIfMessagingExists()
		{
			var headerBO = Factory.New<AsycudaManifestHeader>();
			headerBO.AMA_JobReference = "OGM0000001";
			headerBO.AMA_Voyage = "DOOMED";
			var message = headerBO.Messages.AddNew();
			message.EM_ApplicationCode = "OUT";
			var inboundMessage = Factory.Load<EDIMessage>(message.PK);
			inboundMessage.EM_MessageType = "ZA";
			inboundMessage.EM_MessageSubType = "XXX";
			inboundMessage.EM_MessageText = "XXX";
			inboundMessage.EM_ReceiveTransmit = "RCV";
			Factory.SaveForTesting();
			inboundMessage.EM_ReceiveTransmit = "TRX";
			Factory.SaveForTesting();
			AssertEquals("Pre-req set business object value to something not in uXml", headerBO.AMA_Voyage, "DOOMED");
			headerBO = ProcessQueuedUniversalShipmentMessage(BaseSourcePath + @"Enterprise\Product\Operations\Customs\ZA\DataTransfer.Test\Universal\TestFiles\SampleZAOutTurnUniversalXml_UPDATE.xml", "OGM0000001");
			AssertEquals("uXml should not be processed because there are messages", headerBO.AMA_Voyage, "DOOMED");
		}

		AsycudaManifestHeader ProcessQueuedUniversalShipmentMessage(ZString filePath, ZString jobReference)
		{
			var message = GetQueuedUniversalShipmentMessage(File.ReadAllText(filePath));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			var newFactory = new BusinessObjectFactory();
			var query = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
			query.AddToFilter(AsycudaManifestHeaderSchema.AMA_JobReference, jobReference);
			var headerBO = newFactory.LoadTop1<AsycudaManifestHeader>(query);
			headerBO.Reload();
			return headerBO;
		}

		ZInt AsycudaManifestHeaderCount => GetAsycudaManifestHeaderCount();
		ZInt GetAsycudaManifestHeaderCount()
		{
			var factory = new BusinessObjectFactory();
			var query = new ZQuery();
			query.AddToFilter(AsycudaManifestHeaderSchema.AMA_RN_NKCountry, Core.Constants.CountryCodes.SouthAfrica);
			query.AddToFilter(AsycudaManifestHeaderSchema.AMA_ApplicationCode, "OUT");
			var headers = factory.Load<AsycudaManifestHeader>(query);
			return headers.Length;
		}

		void AssertUpdatedAsycudaManifestHeader(ZString jobReference, ZString suffix)
		{
			var factory = new BusinessObjectFactory();
			var query = new ZQuery();
			query.AddToFilter(AsycudaManifestHeaderSchema.AMA_JobReference, jobReference);
			query.AddToFilter(AsycudaManifestHeaderSchema.AMA_RN_NKCountry, Core.Constants.CountryCodes.SouthAfrica);
			query.AddToFilter(AsycudaManifestHeaderSchema.AMA_ApplicationCode, "OUT");
			var headerBO = factory.LoadTop1<AsycudaManifestHeader>(query);
			AssertEquals($"{jobReference}", headerBO.AMA_JobReference);
			AssertEquals($"VESSEL{suffix}", headerBO.AMA_VesselName);
			AssertEquals($"VOYAGE{suffix}", headerBO.AMA_Voyage);
			AssertOrganisationAndAddress(headerBO.AMA_OA_Carrier, "1", "CARRIER");
			AssertOrganisationAndAddress(headerBO.AMA_OA_ShippingAgent, "4", "AGENT");
			AssertEquals("SEA", headerBO.AMA_TransportMode);
			AssertEquals("OUT", headerBO.AMA_ApplicationCode);
			AssertEquals("CNT", headerBO.AMA_ContainerMode);
			AssertEquals("DRT", headerBO.AMA_AgentType);
			AssertOrganisationAndAddress(headerBO.AMA_OA_DeconsolidateAddress, "2", "DECONSOLIDATOR");
			AssertOrganisationAndAddress(headerBO.AMA_OA_DischargeTerminalAddress, "3", "DISCHARGE");
			AssertEquals("EXP", headerBO.AMA_Nature);
			AssertEquals("VOR", headerBO.AMA_ManifestType);
			AssertEquals("BFN", headerBO.AMA_CustomsOffice);
			AssertEquals("ZA", headerBO.AMA_RN_NKCountry);
			AssertEquals("1", headerBO.ExcessIndicator);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(+3), headerBO.FullyLoadedUnloadedDate);
			AssertEquals("DGI", headerBO.GateInOutMessageType);
			AssertEquals($"PARENTBILL{suffix}", headerBO.ParentBill);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(+4), headerBO.UnpackedDate);
			AssertEquals($"MASTERBILL{suffix}", headerBO.MasterBill.ABL_BillNumber);
			AssertEquals($"BOOKING{suffix}", headerBO.MasterBill.ABL_CarrierReference);
			AssertEquals("BOL", headerBO.MasterBill.ABL_BolType);
			AssertEquals(ZDate.BrettsBirthday, headerBO.MasterBill.ABL_BillIssueDate);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(+1), headerBO.MasterBill.ABL_E_DEP);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(+2), headerBO.MasterBill.ABL_E_ARV);
			AssertEquals($"ZAPL{suffix}", headerBO.MasterBill.ABL_RL_NKPortOfLoading);
			AssertEquals($"ZAPU{suffix}", headerBO.MasterBill.ABL_RL_NKPortOfDischarge);
			AssertEquals("04", headerBO.OutturnProvider);
			AssertEquals(2, headerBO.Bills.Count);
			AssertHouseBill(headerBO.Bills[0], "1", "1", "2", "1", "2");
			AssertHouseBill(headerBO.Bills[1], "2", "3", "4", "3", "4");
			AssertEquals(4, headerBO.Containers.Count);
			AssertContainer(headerBO.Containers[0], "20NOR", "FCL", "CAR", "1");
			AssertContainer(headerBO.Containers[1], "20NOR", "LCL", "AGT", "2");
			AssertContainer(headerBO.Containers[2], "40GP", "FCL", "CUS", "3");
			AssertContainer(headerBO.Containers[3], "40GP", "LCL", "TOR", "4");
		}

		void AssertHouseBill(AsycudaBill houseBill, ZString houseSuffix, ZString containerSuffix1, ZString containerSuffix2, ZString packSuffix1, ZString packSuffix2)
		{
			AssertEquals($"HOUSEBILL{houseSuffix}", houseBill.ABL_BillNumber);
			AssertEquals("HWB", houseBill.ABL_BolType);
			AssertEquals($"ISSUER{houseSuffix}", houseBill.ABL_BillIssuer);
			AssertEquals($"MRN{houseSuffix}", houseBill.MRN);
			AssertEquals($"LRN{houseSuffix}", houseBill.LRN);
			AssertEquals($"CPC{houseSuffix}", houseBill.CustomsCPC);
			AssertEquals($"ST{houseSuffix}", houseBill.ABL_BillStatus);
			AssertEquals(2, houseBill.Packs.Count);
			AssertPack(houseBill.Packs[0], containerSuffix1, packSuffix1);
			AssertPack(houseBill.Packs[1], containerSuffix2, packSuffix2);
		}

		void AssertContainer(AsycudaContainer container, ZString containerTypePK, ZString emptyFullIndicator, ZString sealingPartyType, ZString suffix)
		{
			AssertEquals($"CONTAINER{suffix}", container.ACN_ContainerNumber);
			AssertEquals(containerTypePK, container.ContainerType.RC_Code);
			AssertEquals(emptyFullIndicator, container.ACN_EmptyFullIndicator);
			AssertEquals($"SEAL{suffix}", container.ACN_Seal1);
			AssertEquals(sealingPartyType, container.ACN_SealingPartyType);
			AssertEquals(new ZDateTime(1998, 8, 9, 1, 5, 0), container.ContUnpackTime);
			AssertEquals(new ZDateTime(1998, 8, 9, 2, 5, 0), container.GateInOutDate);
		}

		void AssertPack(AsycudaPack pack, ZString containerSuffix, ZString packSuffix)
		{
			var intValue = ZInt.Parse(packSuffix);
			var decimalValue = ZDecimal.Parse(packSuffix + ".99");
			var outturnGoodsDescription = packSuffix == "3" ? "CONTENTSSHOULDBEDESC3" : $"CONTENTSFOUNDTOBEDESC{packSuffix}";
			AssertEquals($"CONTAINER{containerSuffix}", pack.Container.ACN_ContainerNumber);
			AssertEquals($"B{packSuffix}", pack.Outturn.C5_CargoType);
			AssertEquals($"{packSuffix}", pack.Outturn.C5_PackageCondition);
			AssertEquals($"{packSuffix}", pack.Outturn.ExcessShortInd);
			AssertEquals(outturnGoodsDescription, pack.Outturn.C5_GoodsDescription);
			AssertEquals(decimalValue, pack.APA_Weight);
			AssertEquals("KG", pack.APA_WeightUQ);
			AssertEquals(decimalValue, pack.APA_Volume);
			AssertEquals("L", pack.APA_VolumeUQ);
			AssertEquals($"MARKSANDNUMBERS{packSuffix}", pack.APA_MarksAndNumbers);
			AssertEquals(intValue, pack.APA_PackQty);
			AssertEquals($"{packSuffix}A", pack.APA_PackUQ);
			AssertEquals(intValue, pack.Outturn.C5_PackagesOutturned);
			AssertEquals($"PACKCONDITIONDESC{packSuffix}", pack.Outturn.PackCondDesc);
			AssertEquals($"GOODSDESC{packSuffix}", pack.APA_GoodsDescription);
			AssertEquals($"CONTENTSSHOULDBEDESC{packSuffix}", pack.Outturn.ContShouldBe);
			AssertEquals(decimalValue, pack.Outturn.C5_WeightOutturned);
			AssertEquals("KG", pack.Outturn.C5_WeightOutturnedUQ);
			AssertEquals(decimalValue, pack.Outturn.C5_VolumeOutturned);
			AssertEquals("L", pack.Outturn.C5_VolumeOutturnedUQ);
			AssertEquals(expected: true, pack.Outturn.C5_SealIntactIndicator);
		}

		void AssertOrganisationAndAddress(ZGuid addressPk, string suffix, string traderName)
		{
			var address = Factory.Load<MasterFiles.Business.OrgAddress>(addressPk);
			AssertNotNull($"Null {traderName} address", address);
			AssertEquals(traderName + suffix, address.Header.OH_FullName);
			AssertEquals($"GBXX{suffix}", address.Header.OH_RL_NKClosestPort);
			AssertEquals($"ADDRESS1{suffix}", address.OA_Address1);
			AssertEquals($"ADDRESS2{suffix}", address.OA_Address2);
			AssertEquals($"CITY{suffix}", address.OA_City);
			AssertEquals($"STATE{suffix}", address.OA_State);
			AssertEquals($"POST XX{suffix}", address.OA_PostCode);
		}

		static Shipment LoadShipmentFromXmlFile(BusinessObjectFactory factory, string filename)
		{
			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(filename)))
			{
				var logger = new TestErrorLogger();
				var mapper = new CodeMappingManager(logger);
				new XmlReader(mapper).ReadXML(universalShipment, stream, logger);
				mapper.UpdateMappedCodes(universalShipment, factory);
			}

			return universalShipment;
		}

		static string GetSampleZAOutTurnUniversalXml_NEW
		{
			get
			{
				using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Enterprise.Customs.ZA.DataTransfer.Universal.Testing.Universal.TestFiles.SampleZAOutTurnUniversalXml_NEW.xml"))
				{
					using (var sr = new StreamReader(stream))
					{
						return sr.ReadToEnd();
					}
				}
			}
		}

		static string GetSampleZAOutTurnUniversalXml_UPDATE
		{
			get
			{
				using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Enterprise.Customs.ZA.DataTransfer.Universal.Testing.Universal.TestFiles.SampleZAOutTurnUniversalXml_UPDATE.xml"))
				{
					using (var sr = new StreamReader(stream))
					{
						return sr.ReadToEnd();
					}
				}
			}
		}
	}
}
