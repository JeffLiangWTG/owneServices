using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.Documents.Testing.CPT
{
	sealed class CargoReceiptAdviceBuilderTest : TestCaseWithFactory
	{
		public void TestBuild()
		{
			var shipment = CreateShipment();
			var builder = new CargoReceiptAdviceBuilder(shipment);
			var cargoReceiptAdvice = builder.Build();

			AssertEquals("HIRReference", "SHP001", cargoReceiptAdvice.HIRReference);
			AssertEquals("MarksAndNumbers", "marks & numbers", cargoReceiptAdvice.MarksAndNumbers);

			AssertEquals("InterimReceipt", "interimReceipt123", cargoReceiptAdvice.InterimReceipt);
			AssertEquals("InterimReceiptDate", new ZDateTime(2022, 3, 8, 10, 58, 51), cargoReceiptAdvice.InterimReceiptDate);

			AssertionHelper.AssertAddressData(shipment.BookingPartyDocumentaryAddress, cargoReceiptAdvice.BookingParty);
			AssertionHelper.AssertAddressData(shipment.ExportReceivingDepot, cargoReceiptAdvice.DepartureCFSAddress);

			AssertEquals("PackingLines.Count", 2, cargoReceiptAdvice.PackingLines.Count);
			AssertEquals("PackingLine1", 1, cargoReceiptAdvice.PackingLines.ElementAt(0).Quantity);
			AssertEquals("PackingLine1", 100M, cargoReceiptAdvice.PackingLines.ElementAt(0).Weight.Value);
			AssertEquals("PackingLine1", 300M, cargoReceiptAdvice.PackingLines.ElementAt(0).Volume.Value);
			AssertEquals("PackingLine2", 2, cargoReceiptAdvice.PackingLines.ElementAt(1).Quantity);
			AssertEquals("PackingLine2", 200M, cargoReceiptAdvice.PackingLines.ElementAt(1).Weight.Value);
			AssertEquals("PackingLine2", 600M, cargoReceiptAdvice.PackingLines.ElementAt(1).Volume.Value);
		}

		#region Implementation

		ForwardingShipment CreateShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SH0001000";
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment.JS_HouseBill = "HOUSEBILL001";
			shipment.JS_RL_NKOrigin = "MXTIJ";
			shipment.JS_RL_NKDestination = "MXCJS";
			shipment.JS_RL_NKLoadPort = "MXTCT";
			shipment.JS_RL_NKDischargePort = "MXELP";
			shipment.JS_E_DEP = ZDate.Today.AddDays(1);
			shipment.JS_E_ARV = ZDate.Today.AddDays(2);
			shipment.JS_MarksAndNumbers = "marks & numbers";
			shipment.JS_BookingReference = "BKG000001";

			shipment.JS_InterimReceipt = "interimReceipt123";
			shipment.JS_A_RCV = new ZDateTime(2022, 3, 8, 10, 58, 51);

			var messageReference = Factory.New<CusEntryNumber>();
			messageReference.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			messageReference.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			messageReference.CE_EntryType = CustomsReferenceNumberType.eHubInterchangeReference.HIR;
			messageReference.CE_EntryNum = "SHP001";
			shipment.Numbers.Add(messageReference);

			shipment.OuterPackLines.RemoveAndDeleteAll();
			PopulatePackingLine(shipment, 1, 100, 300);
			PopulatePackingLine(shipment, 2, 200, 300);

			PopulateShipmentAddresses(shipment);

			Factory.Save();

			return shipment;
		}

		void PopulateShipmentAddresses(ForwardingShipment shipment)
		{
			var cfs = Factory.New<OrgHeader>();
			cfs.OH_FullName = "CONSPA";
			cfs.OH_RL_NKClosestPort = "FRMAR";
			cfs.MainAddress.Address1 = "Unit 15";
			cfs.MainAddress.Address2 = "5 Lost Lane";
			cfs.MainAddress.City = "Marseille";
			cfs.MainAddress.Postcode = "2000";
			cfs.MainAddress.OA_RN_NKCountryCode = "AU";
			shipment.JS_OA_ExportReceivingDepot = cfs.MainAddress.PK;

			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.OH_FullName = "YUMMY";
			bookingParty.OH_RL_NKClosestPort = "FRPAR";
			bookingParty.MainAddress.Address1 = "Unit 200";
			bookingParty.MainAddress.Address2 = "55 Why Lane";
			bookingParty.MainAddress.City = "Paris";
			bookingParty.MainAddress.Postcode = "2000";
			bookingParty.MainAddress.OA_RN_NKCountryCode = "AU";

			shipment.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;
		}

		void PopulatePackingLine(ForwardingShipment shipment, ZInt count, ZDecimal weight, ZDecimal volume)
		{
			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_PackageCount = count;
			packline.JL_F3_NKPackType = Constants.PkgUnit.Pallet;
			packline.JL_ActualWeight = weight;
			packline.JL_ActualWeightUQ = Constants.Weight.Kilograms;
			packline.JL_ActualVolume = volume;
			packline.JL_ActualVolumeUQ = Constants.Volume.CubicMetres;
			packline.JL_Length = 10;
			packline.JL_Width = 10;
			packline.JL_Height = 3;
			packline.JL_UnitOfDimension = "M";
			packline.JL_HarmonisedCode = "WHISKY";
			packline.JL_RefNumber = "REF001";
			packline.JL_ExportRefNumber = "REF002";
			packline.JL_DetailedDescription = "DetailedDescription";
			packline.JL_MarksAndNumbers = "MarksAndNumbers";
			packline.JL_Description = "Description";
		}

		#endregion
	}
}
