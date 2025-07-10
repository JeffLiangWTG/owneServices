using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.AES.Testing
{
	[TestedType(typeof(AESTIRMessageCommodityLine))]
	sealed class AESTIRMessageCommodityLineTest : AESPrintCommodityLineTest
	{
		public override void TestLicenseTypeDescription()
		{
			var messageHeaderPrintingObject = new AESMessagePrint(OutgoingMessageInTIRFormat);
			var messageLineForPrinting = (AESTIRMessageCommodityLine)messageHeaderPrintingObject.Commodities[0];
			AssertEquals("License Type description", "B191396  C30 (C30)", messageLineForPrinting.LicenseTypeDescription);
		}

		public void TestPropertiesForAESTIR()
		{
			var messageHeaderPrintingObject = new AESMessagePrint(OutgoingMessageInTIRFormat);
			AssertEquals(2, messageHeaderPrintingObject.Commodities.Count);
			var onlyOneMessageLineForPrinting = (AESTIRMessageCommodityLine)messageHeaderPrintingObject.Commodities[0];

			AssertEquals("Line Number", "1", onlyOneMessageLineForPrinting.LineNo);
			AssertEquals("Export Code", "OS", onlyOneMessageLineForPrinting.ExportCode);
			AssertEquals("Tariff Description", "WRIST WATCHES, ELECTRICALLY OPERATED, WITH CA", onlyOneMessageLineForPrinting.TariffDescription);
			AssertEquals("Tariff Number", "9101110000", onlyOneMessageLineForPrinting.TariffNo);
			AssertEquals("Quantity", 100m, onlyOneMessageLineForPrinting.Qty);
			AssertEquals("UQ", "NO", onlyOneMessageLineForPrinting.UQ);
			AssertEquals("Gross Wt", 55m, onlyOneMessageLineForPrinting.GrossWt);
			AssertEquals("KG", onlyOneMessageLineForPrinting.GrossWtUQ);
			AssertEquals("Customs Value in whole US dollars", 10950m, onlyOneMessageLineForPrinting.Value);
			AssertEquals("Origin Indicator", "D", onlyOneMessageLineForPrinting.OriginIndicator);
			AssertEquals("License Type", "C30", onlyOneMessageLineForPrinting.LicenseType);
			AssertEquals("Vehicle Indicator", "Yes", onlyOneMessageLineForPrinting.VehicleIndicator);
			AssertEquals("License Type description", "B191396  C30 (C30)", onlyOneMessageLineForPrinting.LicenseTypeDescription);
			AssertEquals("Export License number", "B191396", onlyOneMessageLineForPrinting.ExportLicenseNo);
			AssertEquals("License value", "5", onlyOneMessageLineForPrinting.LicenseValue);
			AssertEquals("ECCN", "0A001", onlyOneMessageLineForPrinting.ECCN);
			AssertEquals("ITAR Exemption Code", "123.16B2", onlyOneMessageLineForPrinting.ITARExemptionNo);
			AssertEquals("Military Equipment indicator", "N", onlyOneMessageLineForPrinting.MilitaryEquipmentIndicator);
			AssertEquals("Party Certification indecator", "Y", onlyOneMessageLineForPrinting.PartyCertificationIndicator);
			AssertEquals("Registration Number", "GFD453", onlyOneMessageLineForPrinting.RegistrationNo);
			AssertEquals("USML Category Code", USMLCategoryCodes.Codes.AuxiliaryMilitaryEquipment, onlyOneMessageLineForPrinting.USMLCategoryCode);
			AssertEquals("USML Category description", USMLCategoryCodes.Descriptions.AuxiliaryMilitaryEquipment, onlyOneMessageLineForPrinting.USMLCategoryDescription);
			AssertEquals("DDTC Qty", "500 CSE", onlyOneMessageLineForPrinting.DDTCQuantityWithUnitOfMeasure);
			AssertEquals("Vehicle Title Number", "FDSF34535", onlyOneMessageLineForPrinting.VehicleTitleNumber);
			AssertEquals("Vehicle ID Type description", "VIN", onlyOneMessageLineForPrinting.VehicleIDTypeDescription);
			AssertEquals("Vehicle Title State description", "American Samoa (also country AS) (AS)", onlyOneMessageLineForPrinting.VehicleTitleStateDescription);
			AssertEquals("Vehicle ID", "543545D", onlyOneMessageLineForPrinting.VehicleID);
		}

		AESTIREDIMessage OutgoingMessageInTIRFormat
		{
			get
			{
				if (outgoingMessageInTIRFormat == null)
				{
					outgoingMessageInTIRFormat = Factory.NewWithValidTestData<AESTIREDIMessage>();
					outgoingMessageInTIRFormat.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
					outgoingMessageInTIRFormat.EM_LinkUniqueID = Entry.PK;
					outgoingMessageInTIRFormat.EM_MessageText = "B  91013199000E          ABC EXPORTS USA                                        SC1N11HKILKKLUB00153967        AKAGA                   2 58201270920100817 N    SC270                                   N                                       SC3                             OB903847                                        SC3APLU0398476   K9378                                                          N0191013199000EEABC EXPORTS USA               GARY         ODEA                 N02ALTERNATIVE PICKUP ADDRESS                                      6452535520   N03MADISON                  WIUS53562                                           N0156999999900EFCARGOWISE INC                                                   N021699 WALL STREET                                                8475551212   N03MOUNTPROSPECT            ILUS60056                                           N01            CMUSIC TRADING ONLINE                                           NN0214TH FLOOR, LU PLAZA            2 WING YUP STREET, KWUN TONG                 N03HONGKONG                   HK                                                CL1OS 0001WRIST WATCHES, ELECTRICALLY OPERATED, WITH CA         56AC30D         CL29101110000NO 00000001000000010950   000000000000000000550A001B191396         ODT123.16B2    GFD453NY13        CSE0000500                                     EV1543545D                  VFDSF34535      AS                                  CL1OS 0002WATCH CASE, BAT POW, AU, A                            56AC30D         CL29102111020NO 00000000300000003000   000000000000000000220A001B191396         Y  91013199000E          ABC EXPORTS USA";
				}
				return outgoingMessageInTIRFormat;
			}
		}
		AESTIREDIMessage outgoingMessageInTIRFormat;

		#region Overrides

		protected override AESPrintCommodityLine GetLineForTest()
		{
			var headerPrintingObject = new AESMessagePrint(OutgoingMessageInTIRFormat);
			return (AESTIRMessageCommodityLine)headerPrintingObject.Commodities[0];
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AESTIRMessageCommodityLine(Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(Factory, new ZString[] { USAESLicenseCode.Codes.C30 });
		}

		#endregion
	}
}
