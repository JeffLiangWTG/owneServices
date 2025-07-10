using System;
using System.Collections.Generic;
using CargoWise.Definitions.Ecommerce;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.GUI.Testing
{
	[TestedType(typeof(UpdateHVLVStatusApplicator))]
	public class UpdateHVLVStatusApplicatorTest : OperationalActionMethodApplicatorTest
	{
		public void TestUpdateHVLVStatus()
		{
			using (HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

				var consignment1 = Factory.New<HVLVConsignment>();
				consignment1.HVC_WaybillNumber = "REF1";
				consignment1.HVC_Status = HVLVConsignmentStatus.Codes.Delivered;
				consignment1.HVC_HCH_Header = consignmentHeader.PK;

				var item1 = Factory.New<IHVLVItem>();
				item1.HVI_HVC_Consignment = consignment1.PK;
				item1.HVI_JS_LoadedOnShipment = shipment.PK;

				var consignment2 = Factory.New<HVLVConsignment>();
				consignment2.HVC_WaybillNumber = "REF2";
				consignment2.HVC_HCH_Header = consignmentHeader.PK;
				consignment2.HVC_Status = HVLVConsignmentStatus.Codes.Confirmed;

				var item2 = Factory.New<IHVLVItem>();
				item2.HVI_HVC_Consignment = consignment2.PK;
				item2.HVI_JS_LoadedOnShipment = shipment.PK;

				var consignment3 = Factory.New<HVLVConsignment>();
				consignment3.HVC_WaybillNumber = "REF3";
				consignment3.HVC_HCH_Header = consignmentHeader.PK;
				consignment3.HVC_Status = HVLVConsignmentStatus.Codes.ArrivedAtDestination;

				var item3 = Factory.New<IHVLVItem>();
				item3.HVI_HVC_Consignment = consignment3.PK;
				item3.HVI_JS_LoadedOnShipment = shipment.PK;

				Factory.Save();

				AssertEquals("Precondition: Consignment ID defaulted", "REF1", consignment1.HVC_ConsignmentId);
				AssertEquals("Precondition: Item ID defaulted", "REF1", item1.HVI_ItemId);
				AssertEquals("Precondition: Consignment ID defaulted", "REF2", consignment2.HVC_ConsignmentId);
				AssertEquals("Precondition: Item ID defaulted", "REF2", item2.HVI_ItemId);
				AssertEquals("Precondition: Consignment ID defaulted", "REF3", consignment3.HVC_ConsignmentId);
				AssertEquals("Precondition: Item ID defaulted", "REF3", item3.HVI_ItemId);

				Applicator.StatusCode = HVLVConsignmentStatus.Codes.Confirmed;

				var expected = new[]
				{
					"INFO: Processing shipment EBM22Q33TU475BXH3P60:\n",
					"INFO: \tProcessing Consignment with Consignment ID REF1\nINFO: \t\tSkipped, because consignment status is Delivered.\n",
					"INFO: \tProcessing Consignment with Consignment ID REF2\nINFO: \t\tSkipped, because consignment status is the same.\n",
					"INFO: \tProcessing Consignment with Consignment ID REF3\nINFO: \tProcessed!\n",
					"INFO: Shipment Processed\r\n"
				};

				ApplyApplicatorWhereLogOrderIsUnimportant(new BusinessObject[] { shipment }, expected);

				AssertEquals(HVLVConsignmentStatus.Codes.Delivered, consignment1.HVC_Status);
				AssertEquals(Applicator.StatusCode, consignment2.HVC_Status);
				AssertEquals(Applicator.StatusCode, consignment3.HVC_Status);
			}
		}

		public void TestIncorrectTypeMessage()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.StandardHouse;

			Factory.Save();

			Applicator.StatusCode = HVLVConsignmentStatus.Codes.Confirmed;

			ZString expected = "INFO: Processing shipment EBM22Q33TU475BXH3P60:\n" +
			"INFO: Shipment is not of HVL type, skipped.";
			ApplyApplicator(new BusinessObject[] { shipment }, expected);
		}

		public void TestUpdateHVLVValidation()
		{
			Applicator.StatusCode = ZString.Empty;
			AssertHasError(Applicator.StatusCodeInfo, "Please enter a value.");

			Applicator.StatusCode = "CCC";
			AssertHasError(Applicator.StatusCodeInfo, "Enter a valid selection.");

			Applicator.StatusCode = HVLVConsignmentStatus.Codes.Confirmed;
			AssertNoErrors(Applicator.StatusCodeInfo);
		}

		public void TestHVLVConsignmentStatuses_AreTheSameAsTheConstants_InForwarding()
		{
			var codes = typeof(HVLVConsignmentStatus.Codes).GetFields();
			var descriptions = typeof(HVLVConsignmentStatus.Descriptions).GetFields();
			var descriptionList = new List<string>();
			var updater = new UpdateHVLVStatusApplicator();

			foreach (var field in descriptions)
			{
				descriptionList.Add(field.GetValue(null).ToString());
			}

			CombineAssertions("Pre-conditions", delegate
			{
				AssertEquals(codes.Length, descriptions.Length);
				AssertEquals(codes.Length, updater.StatusCodeList.GetAllCodes().Length);
			});

			foreach (var field in codes)
			{
				var code = field.GetValue(null);
				AssertEquals(true, updater.StatusCodeList.ContainsCode(code));

				var description = updater.StatusCodeList.GetDescriptionFromCode(code.ToString());
				AssertEquals(true, descriptionList.Contains(description));
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new UpdateHVLVStatusApplicator();
		}

		new UpdateHVLVStatusApplicator Applicator => (UpdateHVLVStatusApplicator)base.Applicator;
	}
}
