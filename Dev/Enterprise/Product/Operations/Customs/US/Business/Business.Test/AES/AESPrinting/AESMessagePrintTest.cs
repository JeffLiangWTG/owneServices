using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.AES.Testing
{
	[TestedType(typeof(AESMessagePrint))]
	sealed class AESMessagePrintTest : AESPrintTest
	{
		public void TestConstructorAndPropertiesForAESTIR()
		{
			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "3901", "CHICAGO, IL", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "2709", "LONG BEACH, CA", startDate, endDate);
			newFactory.Save();

			AssertEquals("AESTIR messages must not print AESDirect on the Transmission document", "AES Shipment Record", AESPrintForAESTir.TransmittedVia);
			AssertEquals("Shipment Reference Number", "B00153967", AESPrintForAESTir.ShipmentReferenceNumber);
			AssertEquals("ITN", "", AESPrintForAESTir.ITN);
			AssertEquals("Departure Date", new ZDate(2010, 8, 17), AESPrintForAESTir.DepartureDate);
			AssertEquals("Transportation Reference Number should have been picked up from first SC3 segment", "OB903847", AESPrintForAESTir.TransportationReferenceNo);
			AssertEquals("State Of Origin", "ILLINOIS (IL)", AESPrintForAESTir.StateOfOrigin);
			AssertEquals("Country Of Destination", "HONG KONG (HK)", AESPrintForAESTir.CountryOfDestination);
			AssertEquals("Port Of Export", "LONG BEACH, CA (2709)", AESPrintForAESTir.PortOfExport);
			AssertEquals("Mode of Transportation", "Vessel, Container (11)", AESPrintForAESTir.ModeOfTransportation);
			AssertEquals("Carrier SCAC", "KKLU", AESPrintForAESTir.CarrierSCAC);
			AssertEquals("Conveyance Name", "KAGA", AESPrintForAESTir.ConveyanceName);
			AssertEquals("Routed Transaction Indicator", "No", AESPrintForAESTir.RoutedTransactionIndicator);
			AssertEquals("Related Companies Indicator", "No", AESPrintForAESTir.RelatedCompaniesIndicator);
			AssertEquals("Hazardous Indicator", "No", AESPrintForAESTir.HazardousIndicator);
			AssertEquals("USPPI Name", "ABC EXPORTS USA", AESPrintForAESTir.USPPIName);
			AssertEquals("USPPI ID Number", "91013199000 (EIN)", AESPrintForAESTir.USPPIIDNumber);
			AssertEquals("USPPI Contact Name", "GARY ODEA", AESPrintForAESTir.USPPIContactName);
			AssertEquals("USPPI Phone", "6452535520", AESPrintForAESTir.USPPIPhone);
			AssertEquals("USPPI Cargo Origin Line 1", "ALTERNATIVE PICKUP ADDRESS", AESPrintForAESTir.USPPICargoOriginLine1);
			AssertEquals("USPPI Cargo Origin Line 2", "TEST ADDRESS 2", AESPrintForAESTir.USPPICargoOriginLine2);
			AssertEquals("USPPI Cargo Origin Line 3", "MADISON WI US 53562", AESPrintForAESTir.USPPICargoOriginLine3);
			AssertEquals("Ultimate Consignee Name", "MUSIC TRADING ONLINE", AESPrintForAESTir.UltimateConsigneeName);
			AssertEquals("Ultimate Consignee Contact", ZString.Empty, AESPrintForAESTir.UltimateConsigneeContact);
			AssertEquals("Ultimate Consignee Phone", ZString.Empty, AESPrintForAESTir.UltimateConsigneePhone);
			AssertEquals("Ultimate Consignee Address 1", "14TH FLOOR, LU PLAZA", AESPrintForAESTir.UltimateConsigneeAddress1);
			AssertEquals("Ultimate Consignee Address 2", "2 WING YUP STREET, KWUN TONG", AESPrintForAESTir.UltimateConsigneeAddress2);
			AssertEquals("Ultimate Consignee Address 3", "HONGKONG HK", AESPrintForAESTir.UltimateConsigneeAddress3);
			AssertEquals("Freight Forwarder Name", "CARGOWISE INC", AESPrintForAESTir.FreightForwarderName);
			AssertEquals("Freight Forwarder ID Number", "56999999900 (EIN)", AESPrintForAESTir.FreightForwarderIDNumber);
			AssertEquals("Freight Forwarder Contact", "", AESPrintForAESTir.FreightForwarderContact);
			AssertEquals("Freight Forwarder Phone", "8475551212", AESPrintForAESTir.FreightForwarderPhone);
			AssertEquals("Freight Forwarder Address 1", "1699 WALL STREET", AESPrintForAESTir.FreightForwarderAddress1);
			AssertEquals("Freight Forwarder Address 2", "TEST ADDRR 2", AESPrintForAESTir.FreightForwarderAddress2);
			AssertEquals("Freight Forwarder Address 3", "MOUNTPROSPECT IL US 60056", AESPrintForAESTir.FreightForwarderAddress3);

			AssertEquals("Intermediate Consignee Name", "SOME COMPANY NAME IN", AESPrintForAESTir.IntermediateConsigneeName);
			AssertEquals("Intermediate Consignee Contact", "BRENDON PAINE", AESPrintForAESTir.IntermediateConsigneeContact);
			AssertEquals("Intermediate Consignee Phone", "1111111111", AESPrintForAESTir.IntermediateConsigneePhone);
			AssertEquals("Intermediate Consignee Address 1", "9 ERICA ROAD DURBANVILLE HILLS C", AESPrintForAESTir.IntermediateConsigneeAddress1);
			AssertEquals("Intermediate Consignee Address 2", "APE TOWN", AESPrintForAESTir.IntermediateConsigneeAddress2);
			AssertEquals("Intermediate Consignee Address 3", "SOUTH AFRICA ZA", AESPrintForAESTir.IntermediateConsigneeAddress3);
			AssertEquals(1, AESPrintForAESTir.Commodities.Count);

			var outgoingMessage = Factory.NewWithValidTestData<AESTIREDIMessage>();
			outgoingMessage.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			outgoingMessage.EM_LinkUniqueID = Entry.PK;
			outgoingMessage.EM_MessageType = ApplicationIdentifierCodeList.AES.CommodityShipment;
			outgoingMessage.EM_MessageText = "B  91013199000E          ABC EXPORTS USA                                        SC1N11HKILKKLUB00153967        AKAGA                   2 58201270920100817 N    SC270                                   N                                       SC3                             OB903847                                        SC3APLU0398476   K9378                                                          N0191013199000EEABC EXPORTS USA               GARY         ODEA                 N02ALTERNATIVE PICKUP ADDRESS      TEST ADDRESS 2                  6452535520   N03MADISON                  WIUS53562                                           N0156999999900EFCARGOWISE INC                                                   N021699 WALL STREET                TEST ADDRR 2                    8475551212   N03MOUNTPROSPECT            ILUS60056                                           N01            CMUSIC TRADING ONLINE                                           NN0214TH FLOOR, LU PLAZA                                                         N03HONGKONG                   HK                                                CL1OS 0001MISC HABERDASERY ITEMS                                  AC33D         CL26302600020NO 00000029400000117590KG 00000003960000000400     NLR             Y  91013199000E          ABC EXPORTS USA";

			var aesPrintForAESTir = new AESMessagePrint(outgoingMessage);
			AssertEquals("Ultimate Consignee Name", "MUSIC TRADING ONLINE", aesPrintForAESTir.UltimateConsigneeName);
			AssertEquals("Ultimate Consignee Contact", ZString.Empty, aesPrintForAESTir.UltimateConsigneeContact);
			AssertEquals("Ultimate Consignee Phone", ZString.Empty, aesPrintForAESTir.UltimateConsigneePhone);
			AssertEquals("Ultimate Consignee Address 1", "14TH FLOOR, LU PLAZA", aesPrintForAESTir.UltimateConsigneeAddress1);
			AssertEquals("Ultimate Consignee Address 2", "HONGKONG HK", aesPrintForAESTir.UltimateConsigneeAddress2);
			AssertEquals("Ultimate Consignee Address 3", ZString.Empty, aesPrintForAESTir.UltimateConsigneeAddress3);

			var outgoingMessage2 = Factory.NewWithValidTestData<AESTIREDIMessage>();
			outgoingMessage2.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			outgoingMessage2.EM_LinkUniqueID = Entry.PK;
			outgoingMessage2.EM_MessageType = ApplicationIdentifierCodeList.AES.CommodityShipment;
			outgoingMessage2.EM_MessageText = "B  91013199000E          ABC EXPORTS USA                                        SC1N11HKILKKLUB00153967        AKAGA                   2 58201270920100817 N    SC270                                   N                                       SC3                             OB903847                                        SC3APLU0398476   K9378                                                          N0191013199000ECABC EXPORTS USA               GARY         ODEA                 N02ALTERNATIVE PICKUP ADDRESS      TEST ADDRESS 2                  6452535520   N03MADISON                  WIUS53562              D                            N0156999999900ECCARGOWISE INC                                                   N021699 WALL STREET                TEST ADDRR 2                    8475551212   N03MADISON                  WIUS53562              D                            N01            CMUSIC TRADING ONLINE                                           NN0214TH FLOOR, LU PLAZA            2 WING YUP STREET, KWUN TONG                 N03MADISON                  WIUS53562              D                            N01            ISOME COMPANY NAME IN          BRENDON      PAINE               NN029 ERICA ROAD DURBANVILLE HILLS CAPE TOWN                        1111111111   N03MADISON                  WIUS53562              D                            CL1OS 0001MISC HABERDASERY ITEMS                                  AC33D         CL26302600020NO 00000029400000117590KG 00000003960000000400     NLR             Y  91013199000E          ABC EXPORTS USA";
			var aesPrintForAESTir2 = new AESMessagePrint(outgoingMessage2);
			AssertEquals("Ultimate Consignee Type", "Direct Consumer", aesPrintForAESTir2.UltimateConsigneeTypeFormat);
		}

		public void TestPrintingFromMessageHandlesMultipleSC3Segments()
		{
			AssertEquals("Shipment Reference Number", "S00013433", AESTIRPrint.ShipmentReferenceNumber);
			AssertEquals("Transportation Reference Number should be printed even when multiple SC3 segments are in message", "1JAXWA0567", AESTIRPrint.TransportationReferenceNo);
		}

		protected override AESPrint GetObjectForTest() => AESPrintForAESTir;

		protected override BusinessObject GetNewBusinessObject() => AESPrintForAESTir;

		AESMessagePrint AESPrintForAESTir
		{
			get
			{
				if (aesPrintForAESTir == null)
				{
					var outgoingMessage = Factory.NewWithValidTestData<AESTIREDIMessage>();
					outgoingMessage.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
					outgoingMessage.EM_LinkUniqueID = Entry.PK;
					outgoingMessage.EM_MessageType = ApplicationIdentifierCodeList.AES.CommodityShipment;
					outgoingMessage.EM_MessageText = "B  91013199000E          ABC EXPORTS USA                                        SC1N11HKILKKLUB00153967        AKAGA                   2 58201270920100817 N    SC270                                   N                                       SC3                             OB903847                                        SC3APLU0398476   K9378                                                          N0191013199000EEABC EXPORTS USA               GARY         ODEA                 N02ALTERNATIVE PICKUP ADDRESS      TEST ADDRESS 2                  6452535520   N03MADISON                  WIUS53562                                           N0156999999900EFCARGOWISE INC                                                   N021699 WALL STREET                TEST ADDRR 2                    8475551212   N03MOUNTPROSPECT            ILUS60056                                           N01            CMUSIC TRADING ONLINE                                           NN0214TH FLOOR, LU PLAZA            2 WING YUP STREET, KWUN TONG                 N03HONGKONG                   HK                                                N01            ISOME COMPANY NAME IN          BRENDON      PAINE               NN029 ERICA ROAD DURBANVILLE HILLS CAPE TOWN                        1111111111   N03SOUTH AFRICA               ZA                                                CL1OS 0001MISC HABERDASERY ITEMS                                  AC33D         CL26302600020NO 00000029400000117590KG 00000003960000000400     NLR             Y  91013199000E          ABC EXPORTS USA";
					aesPrintForAESTir = new AESMessagePrint(outgoingMessage);
				}

				return aesPrintForAESTir;
			}
		}
		AESMessagePrint aesPrintForAESTir;

		AESMessagePrint AESTIRPrint
		{
			get
			{
				if (aesTIRPrint == null)
				{
					var outgoingMessage = Factory.NewWithValidTestData<AESTIREDIMessage>();
					outgoingMessage.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
					outgoingMessage.EM_LinkUniqueID = Entry.PK;
					outgoingMessage.EM_MessageType = ApplicationIdentifierCodeList.AES.CommodityShipment;
					outgoingMessage.EM_MessageText = "B  63128746400E          AMMUNITION ACCESSORIES INC./ A                         SC1N11BRIDSUDUS00013433        ACAP PALMERSTON         2 35177300120111112 Y    SC270                                   Y                                       SC3                             1JAXWA0567                                      SC3GRIU4201301   0785182                                                        N0163128746400EEAMMUNITION ACCESSORIES INC./ AAMBER        RICE                 N022299 SNAKE RIVER AVE                                            2087994499   N03LEWISTON                 IDUS83501                                           N0174193897500EFBNSF LOGISTICS INTERNATIONAL, MARY         WOOD                 N02612 E. DALLAS RD                SUITE 400                       8173107601   N03GRAPEVINE                TXUS76051                                           N01            CHILTI DO BRAZIL COMERCIAL LTDA                                 NN02CENTRO EMPRESARIAL TAMBORE      AVENIDA CECI, 426 / CNPJ 65.000.             N03BARUERI                    BR06460120                                        CL1OS 0001CARTRIDGES FOR RIVETING OR SIMILAR TOOLS OR F           AC33D         CL29306304138THS00000064000000148288   000000000000000113401C992NLR             Y  63128746400E          AMMUNITION ACCESSORIES INC./ A";

					aesTIRPrint = new AESMessagePrint(outgoingMessage);
				}
				return aesTIRPrint;
			}
		}
		AESMessagePrint aesTIRPrint;
	}
}
