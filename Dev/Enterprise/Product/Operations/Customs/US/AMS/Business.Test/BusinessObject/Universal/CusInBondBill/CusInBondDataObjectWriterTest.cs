using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.US.AMS.Business.Universal.Testing
{
	partial class CusInBondDataObjectWriterTest
	{
		void SetupCusInBondBill(CusInBondBill bill, ZString issuerCode, ZString billNumber)
		{
			SetupCusInBondBill(bill, issuerCode, billNumber, "N", "AUSYD", "60267", "SYDNEY", "10", "AUBRN", "60210", "USCHI", "3901", 100, "BAG", 200, "KG", 10, "M3", "CHK", "USPHL", "1101", new ZDate(2018, 3, 25), false, "W285");
			SetupDisposition(bill.DispositionCodes.AddNew(), "1A", new ZDateTime(2018, 3, 21, 11, 0, 0), 1);
			SetupDisposition(bill.DispositionCodes.AddNew(), "1B", new ZDateTime(2018, 3, 21, 12, 0, 0), 2);
		}

		void SetupCusInBondBill(CusInBondBill bill, ZString issuerCode, ZString billNumber, ZString billStatus, ZString portOfLading, ZString portOfLadingScheduleK, ZString placeOfReceipt,
			ZString transportMode, ZString lastForeignPort, ZString foreignPortKCode, ZString foreignPortOfContract, ZString foreignPortOfContractKCode, ZInt manifestQty, ZString manifestUQ,
			ZDecimal weight, ZString weightUQ, ZDecimal volumn, ZString volumnUQ, ZString transportPaymentMethod, ZString portOfUnlading, ZString portOfUnladingScheduleD, ZDate dateOfDischarge,
			ZBool masterInBodInd, ZString firms)
		{
			bill.B0_IssuerCode = issuerCode;
			bill.B0_MasterBillNumber = billNumber;
			bill.B0_BillStatus = billStatus;
			bill.B0_RL_NKPortOfLading = portOfLading;
			bill.B0_PortOfLadingKCode = portOfLadingScheduleK;
			bill.B0_PlaceOfReceipt = placeOfReceipt;
			bill.B0_TransportModeToPortOfLading = transportMode;
			bill.B0_RL_NKLastForeignPort = lastForeignPort;
			bill.B0_LastForeignPortKCode = foreignPortKCode;
			bill.B0_RL_NKForeignPortOfContract = foreignPortOfContract;
			bill.B0_ForeignPortOfContractKCode = foreignPortOfContractKCode;
			bill.B0_ManifestQty = manifestQty;
			bill.B0_ManifestUQ = manifestUQ;
			bill.B0_Weight = weight;
			bill.B0_WeightUQ = weightUQ;
			bill.B0_Volume = volumn;
			bill.B0_VolumeUQ = volumnUQ;
			bill.B0_TransportPaymentMethod = transportPaymentMethod;
			bill.B0_RL_NKInBondPortOfDest = portOfUnlading;
			bill.B0_ForeignPortOfUnladingKCode = portOfUnladingScheduleD;
			bill.B0_DateOfDischarge = dateOfDischarge;
			bill.B0_MasterInBondIndicator = masterInBodInd;
			bill.B0_Firms = firms;
			bill.ShipmentReferenceDetails.DeleteAll();
			bill.SecondaryNotifyParties.DeleteAll();
			bill.DispositionCodes.RemoveAndDeleteAll();
		}

		void AssertInBondBillContents(AdditionalBill billData, ZString issuerCode, ZString billNumber)
		{
			AssertInBondBillContents(billData, issuerCode, billNumber, "N", "AUSYD", "60267", "SYDNEY", "10", "AUBRN", "60210", "USCHI", "3901", 100, "BAG", 200, "KG", 10, "M3", "CHK", "USPHL", "1101", new ZDate(2018, 3, 25), "", "W285");
			var dispositionDataCollection = billData.AddInfoGroupCollection.Where(x => x.Type.GetCodeAsUpperCase() == CusAddInfoTypeAttribute.Codes.USDisposition).ToArray();
			AssertEquals("dispositionDataCollection.Length", 2, dispositionDataCollection.Length);
			AssertInBondDisposition(dispositionDataCollection[0], "1A", new ZDateTime(2018, 3, 21, 11, 0, 0), 1);
			AssertInBondDisposition(dispositionDataCollection[1], "1B", new ZDateTime(2018, 3, 21, 12, 0, 0), 2);
		}

		void AssertInBondBillContents(AdditionalBill billData, ZString issuerCode, ZString billNumber, ZString billStatus, ZString portOfLading, ZString portOfLadingScheduleK, ZString placeOfReceipt,
			ZString transportMode, ZString lastForeignPort, ZString foreignPortKCode, ZString foreignPortOfContract, ZString foreignPortOfContractKCode, ZInt manifestQty, ZString manifestUQ,
			ZDecimal weight, ZString weightUQ, ZDecimal volumn, ZString volumnUQ, ZString transportPaymentMethod, ZString portOfUnlading, ZString portOfUnladingScheduleD, ZDate dateOfDischarge,
			ZString masterInBodInd, ZString firms)
		{
			AssertNotNull("Precondition: billData", billData);

			CombineAssertions(() =>
			{
				AssertEquals("billData.BillType", WayBillTypeList.Codes.Master, billData.BillType.GetCodeAsUpperCase());
				AssertEquals("billData.BillNumber", billNumber, billData.BillNumber.GetValueOrDefault());
				AssertEquals("billData.NoOfPacks", manifestQty, (ZInt)billData.NoOfPacks.GetValueOrDefault());
				AssertEquals("billData.PackType", manifestUQ, billData.PackType.GetCodeAsUpperCase());

				AssertCollectionContains(billData.AddInfoCollection, Constants.Bill.AddInfo.IssuerCode, issuerCode);
				AssertCollectionContains(billData.AddInfoCollection, Constants.Bill.AddInfo.BillStatus, billStatus);
				AssertCollectionContains(billData.AddInfoCollection, Constants.Bill.AddInfo.PortOfLading, portOfLading);
				AssertCollectionContains(billData.AddInfoCollection, Constants.Bill.AddInfo.PortOfLadingScheduleK, portOfLadingScheduleK);
				AssertCollectionContains(billData.AddInfoCollection, Constants.Bill.AddInfo.PlaceOfReceiptScheduleD, placeOfReceipt);
				AssertCollectionContains(billData.AddInfoCollection, Constants.Bill.AddInfo.TransportModeToPortOfLading, transportMode);
				AssertCollectionContains(billData.AddInfoCollection, Constants.Bill.AddInfo.LastForeignPort, lastForeignPort);
				AssertCollectionContains(billData.AddInfoCollection, Constants.Bill.AddInfo.LastForeignPortScheduleK, foreignPortKCode);
				AssertCollectionContains(billData.AddInfoCollection, Constants.Bill.AddInfo.ForeignPortOfContract, foreignPortOfContract);
				AssertCollectionContains(billData.AddInfoCollection, Constants.Bill.AddInfo.ForeignPortOfContractScheduleK, foreignPortOfContractKCode);
				AssertCollectionContains(billData.AddInfoCollection, Constants.Bill.AddInfo.Weight, weight);
				AssertCollectionContains(billData.AddInfoCollection, Constants.Bill.AddInfo.WeightUnit, weightUQ);
				AssertCollectionContains(billData.AddInfoCollection, Constants.Bill.AddInfo.Volume, volumn);
				AssertCollectionContains(billData.AddInfoCollection, Constants.Bill.AddInfo.VolumeUnit, volumnUQ);
				AssertCollectionContains(billData.AddInfoCollection, Constants.Bill.AddInfo.PaymentMethod, transportPaymentMethod);
				AssertCollectionContains(billData.AddInfoCollection, Constants.Bill.AddInfo.PortOfUnlading, portOfUnlading);
				AssertCollectionContains(billData.AddInfoCollection, Constants.Bill.AddInfo.PortOfUnladingScheduleD, portOfUnladingScheduleD);
				AssertCollectionContains(billData.AddInfoCollection, Constants.Bill.AddInfo.EstimatedUnloadDate, dateOfDischarge);
				AssertCollectionContains(billData.AddInfoCollection, Constants.Bill.AddInfo.MasterInBondIndicator, masterInBodInd);
				AssertCollectionContains(billData.AddInfoCollection, Constants.Bill.AddInfo.FIRMS, firms);
			});
		}
	}
}
