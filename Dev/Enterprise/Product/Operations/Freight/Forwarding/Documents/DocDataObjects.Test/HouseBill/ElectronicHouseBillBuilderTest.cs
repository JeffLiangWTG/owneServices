using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Core.Constants;
using static Enterprise.Freight.Forwarding.Documents.Testing.AssertionHelper;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	sealed class ElectronicHouseBillBuilderTest : TestCaseWithFactory
	{
		#region TestBuild

		public void TestBuild()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "GNACC";
			shipment.JS_ElectronicBillOfLadingVersion = 2;
			shipment.JS_ElectronicBillOfLadingTerms = "NTR";
			shipment.JS_ElectronicBillOfLadingType = "STR";

			var houseBill = new ElectronicHouseBillBuilder(shipment).Build(null);

			Assert(houseBill.IsElectronicBOL);
			AssertEquals((ZShort)2, houseBill.ElectronicBillOfLadingVersion);
			AssertEquals("NTR", houseBill.BillTerms.Code);
			AssertEquals("STR", houseBill.BillType.Code);
		}

		#endregion

		#region TestPopulateAddresses

		public void TestPopulateAddresses_CurrentUser()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "GNACC";

			var houseBill = new ElectronicHouseBillBuilder(shipment).Build(null);

			AssertCurrentUserAddressData(houseBill.CurrentUser);
		}

		public void TestPopulateAddresses_ElectronicBillOfLadingShipper()
		{
			AssertWithEnabledElectronicBOL(delegate ()
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "GNACC";
				shipment.JS_HouseBill = "S00001001";
				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

				var address = Factory.New<OrgHeader>();
				address.OH_FullName = "BLACKPINK";
				address.OH_RL_NKClosestPort = "CNSHG";
				address.MainAddress.Address1 = "Unit 124";
				address.MainAddress.Address2 = "24 Nanjing Road";
				address.MainAddress.City = "ShangHai";
				address.MainAddress.Postcode = "0023";
				address.MainAddress.StateCode = "SH";
				address.MainAddress.OA_RN_NKCountryCode = "CN";
				address.MainAddress.OA_Fax = "+45 65 43 21 01";
				address.MainAddress.OA_Phone = "+45 65 43 21 01";
				address.MainAddress.OA_Email = "test@BLACKPINK.com";

				shipment.ConsignorDocumentaryAddress.OrganisationPK = address.PK;

				var houseBill = new ElectronicHouseBillBuilder(shipment).Build(null);
				AssertAddressData(shipment.ShipperDocAddress, houseBill.ElectronicBillOfLadingShipper);
			});
		}

		public void TestPopulateAddresses_ElectronicBillOfLadingConsignee()
		{
			AssertWithEnabledElectronicBOL(delegate ()
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "GNACC";
				shipment.JS_HouseBill = "S00001001";
				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

				var address = Factory.New<OrgHeader>();
				address.OH_FullName = "BLACKPINK";
				address.OH_RL_NKClosestPort = "CNSHG";
				address.MainAddress.Address1 = "Unit 124";
				address.MainAddress.Address2 = "24 Nanjing Road";
				address.MainAddress.City = "ShangHai";
				address.MainAddress.Postcode = "0023";
				address.MainAddress.StateCode = "SH";
				address.MainAddress.OA_RN_NKCountryCode = "CN";
				address.MainAddress.OA_Fax = "+45 65 43 21 01";
				address.MainAddress.OA_Phone = "+45 65 43 21 01";
				address.MainAddress.OA_Email = "test@BLACKPINK.com";

				shipment.JS_ElectronicBillOfLadingConsigneeDocAddress.OrganisationPK = address.PK;

				var houseBill = new ElectronicHouseBillBuilder(shipment).Build(null);
				AssertAddressData(shipment.JS_ElectronicBillOfLadingConsigneeDocAddress, houseBill.ElectronicBillOfLadingConsignee);
			});
		}

		public void TestPopulateAddresses_ElectronicBillOfLadingToOrder()
		{
			AssertWithEnabledElectronicBOL(delegate ()
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "GNACC";
				shipment.JS_HouseBill = "S00001001";
				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

				var address = Factory.New<OrgHeader>();
				address.OH_FullName = "BLACKPINK";
				address.OH_RL_NKClosestPort = "CNSHG";
				address.MainAddress.Address1 = "Unit 124";
				address.MainAddress.Address2 = "24 Nanjing Road";
				address.MainAddress.City = "ShangHai";
				address.MainAddress.Postcode = "0023";
				address.MainAddress.StateCode = "SH";
				address.MainAddress.OA_RN_NKCountryCode = "CN";
				address.MainAddress.OA_Fax = "+45 65 43 21 01";
				address.MainAddress.OA_Phone = "+45 65 43 21 01";
				address.MainAddress.OA_Email = "test@BLACKPINK.com";

				shipment.JS_ElectronicBillOfLadingToOrderDocAddress.OrganisationPK = address.PK;

				var houseBill = new ElectronicHouseBillBuilder(shipment).Build(null);
				AssertAddressData(shipment.JS_ElectronicBillOfLadingToOrderDocAddress, houseBill.ElectronicBillOfLadingToOrder);
			});
		}

		public void TestPopulateAddresses_Holder()
		{
			AssertWithEnabledElectronicBOL(delegate ()
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "GNACC";
				shipment.JS_HouseBill = "S00001001";
				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

				var address = Factory.New<OrgHeader>();
				address.OH_FullName = "BLACKPINK";
				address.OH_RL_NKClosestPort = "CNSHG";
				address.MainAddress.Address1 = "Unit 124";
				address.MainAddress.Address2 = "24 Nanjing Road";
				address.MainAddress.City = "ShangHai";
				address.MainAddress.Postcode = "0023";
				address.MainAddress.StateCode = "SH";
				address.MainAddress.OA_RN_NKCountryCode = "CN";
				address.MainAddress.OA_Fax = "+45 65 43 21 01";
				address.MainAddress.OA_Phone = "+45 65 43 21 01";
				address.MainAddress.OA_Email = "test@BLACKPINK.com";

				shipment.HolderDocAddress.OrganisationPK = address.PK;

				var houseBill = new ElectronicHouseBillBuilder(shipment).Build(null);
				AssertAddressData(shipment.HolderDocAddress, houseBill.Holder);
			});
		}

		public void TestPopulateAddresses_SurrenderParty()
		{
			AssertWithEnabledElectronicBOL(delegate ()
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "GNACC";
				shipment.JS_HouseBill = "S00001001";
				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

				var address = Factory.New<OrgHeader>();
				address.OH_FullName = "BLACKPINK";
				address.OH_RL_NKClosestPort = "CNSHG";
				address.MainAddress.Address1 = "Unit 124";
				address.MainAddress.Address2 = "24 Nanjing Road";
				address.MainAddress.City = "ShangHai";
				address.MainAddress.Postcode = "0023";
				address.MainAddress.StateCode = "SH";
				address.MainAddress.OA_RN_NKCountryCode = "CN";
				address.MainAddress.OA_Fax = "+45 65 43 21 01";
				address.MainAddress.OA_Phone = "+45 65 43 21 01";
				address.MainAddress.OA_Email = "test@BLACKPINK.com";

				shipment.SurrenderPartyDocAddress.OrganisationPK = address.PK;

				var houseBill = new ElectronicHouseBillBuilder(shipment).Build(null);
				AssertAddressData(shipment.SurrenderPartyDocAddress, houseBill.SurrenderParty);
			});
		}

		#endregion

		#region TestPopulateAmendmentRequestID

		public void TestPopulateAmendmentRequestID()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SH0001";
			shipment.JS_TransportMode = "SEA";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZCHC";
			shipment.JS_ElectronicBillOfLadingStatus = FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillAmendmentInProgress;

			var consol1 = shipment.Consols.AddNew();
			consol1.JK_UniqueConsignRef = "CONSOL0001";
			consol1.JK_TransportMode = "ROA";
			consol1.JK_RL_NKLoadPort = "AUMEL";
			consol1.JK_RL_NKDischargePort = "AUSYD";

			var eventParameters = new[]
			{
				new KeyValuePair<string, string>(
					CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type,
					BillStatusUpdatedTypes.AmendmentRequested),
				new KeyValuePair<string, string>(
					CargoWise.EventReference.Constants.EventReferenceParameters.Codes.RequestNumber, "9645")
			};
			shipment.Logs.AddNew(Events.BillStatusUpdated, ZDateTimeOffset.UtcToday, eventParameters);
			Factory.Save();

			var houseBill = new ElectronicHouseBillBuilder(shipment).Build(null);
			AssertEquals("9645", houseBill.AmendmentRequestID);

			shipment.Logs.AddNew(Events.BillStatusUpdated, ZDateTimeOffset.UtcToday.AddSeconds(10), new[]
			{
					new KeyValuePair<string, string>(
						CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type,
						BillStatusUpdatedTypes.AmendmentRequested),
					new KeyValuePair<string, string>(
						CargoWise.EventReference.Constants.EventReferenceParameters.Codes.RequestNumber, "9646")
				});
			shipment.Logs.AddNew(Events.BillStatusUpdated, ZDateTimeOffset.UtcToday.AddSeconds(15), new[]
			{
					new KeyValuePair<string, string>(
						CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type,
						BillStatusUpdatedTypes.AmendmentGranted),
					new KeyValuePair<string, string>(
						CargoWise.EventReference.Constants.EventReferenceParameters.Codes.RequestNumber, "9647")
				});
			shipment.Logs.AddNew(Events.BillingJobEdit, ZDateTimeOffset.UtcToday.AddSeconds(20), new[]
			{
					new KeyValuePair<string, string>(
						CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type,
						BillStatusUpdatedTypes.AmendmentRequested),
					new KeyValuePair<string, string>(
						CargoWise.EventReference.Constants.EventReferenceParameters.Codes.RequestNumber, "9648")
				});
			Factory.Save();

			houseBill = new ElectronicHouseBillBuilder(shipment).Build(null);
			AssertEquals("9646", houseBill.AmendmentRequestID);

			shipment.JS_ElectronicBillOfLadingStatus = ZString.Empty;
			Factory.Save();

			houseBill = new ElectronicHouseBillBuilder(shipment).Build(null);
			AssertNullOrEmpty(houseBill.AmendmentRequestID);
		}

		#endregion

		#region TestCheckElectronicBOLMinimumRequirements

		public void TestCheckElectronicBOLMinimumRequirements()
		{
			AssertWithEnabledElectronicBOL(delegate ()
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "GNACC";
				shipment.JS_HouseBill = "S00001001";
				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

				var address = Factory.New<OrgHeader>();
				address.OH_FullName = "BLACKPINK";
				address.OH_RL_NKClosestPort = "CNSHG";
				address.MainAddress.Address1 = "Unit 124";
				address.MainAddress.Address2 = "24 Nanjing Road";
				address.MainAddress.City = "ShangHai";
				address.MainAddress.Postcode = "0023";
				address.MainAddress.StateCode = "SH";
				address.MainAddress.OA_RN_NKCountryCode = "CN";
				address.MainAddress.OA_Fax = "+45 65 43 21 01";
				address.MainAddress.OA_Phone = "+45 65 43 21 01";
				address.MainAddress.OA_Email = "test@BLACKPINK.com";

				shipment.JS_ElectronicBillOfLadingType = Core.Constants.BillOfLadingBillType.Codes.Straight;

				shipment.ConsigneeDocumentaryAddress.OrganisationPK = address.PK;
				shipment.HolderDocAddress.OrganisationPK = shipment.ConsigneeDocumentaryAddress.OrganisationPK;

				shipment.ConsignorDocumentaryAddress.OrganisationPK = address.PK;

				shipment.JS_ElectronicBillOfLadingConsigneeDocAddress.OrganisationPK = address.PK;

				shipment.SurrenderPartyDocAddress.OrganisationPK = address.PK;

				var houseBill = new ElectronicHouseBillBuilder(shipment).Build(null);
				AssertHasMessageError(houseBill.ErrorPlaceHolderInfo, @"There are validation errors on this form due to missing mandatory information. Please correct these errors before publishing.
First Holder: An Organization selected from here must have a valid Bolero Entity Identifier (TRI) recorded.
Shipper: Shipper must have a valid Bolero Entity Identifier (TRI) to proceed with publishing an Electronic Bill of Lading.
Consignee: The consignee must have a valid Bolero Entity Identifier (TRI) to proceed with publishing a 'Straight Bill'.
Surrender Party: An Organization selected from here must have a valid Bolero Entity Identifier (TRI) recorded.");

				var orgCusCode = address.CustomsCodes.AddNew();
				orgCusCode.OK_CustomsRegNo = "123456";
				orgCusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.BoleroTitleRegisterID;

				houseBill = new ElectronicHouseBillBuilder(shipment).Build(null);
				AssertNoErrors(houseBill.ErrorPlaceHolderInfo);

				orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.AgentCode;
				shipment.JS_ElectronicBillOfLadingType = Core.Constants.BillOfLadingBillType.Codes.ToOrder;
				shipment.JS_ElectronicBillOfLadingToOrderDocAddress.OrganisationPK = address.PK;

				houseBill = new ElectronicHouseBillBuilder(shipment).Build(null);
				AssertHasMessageError(houseBill.ErrorPlaceHolderInfo, @"There are validation errors on this form due to missing mandatory information. Please correct these errors before publishing.
First Holder: An Organization selected from here must have a valid Bolero Entity Identifier (TRI) recorded.
Shipper: Shipper must have a valid Bolero Entity Identifier (TRI) to proceed with publishing an Electronic Bill of Lading.
To Order: The 'To Order' party must have a valid Bolero Entity Identifier (TRI) to proceed with publishing a 'To Order Bill'.
Surrender Party: An Organization selected from here must have a valid Bolero Entity Identifier (TRI) recorded.");

				orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.BoleroTitleRegisterID;

				houseBill = new ElectronicHouseBillBuilder(shipment).Build(null);
				AssertNoErrors(houseBill.ErrorPlaceHolderInfo);

				shipment.JS_ElectronicBillOfLadingType = ZString.Empty;
				shipment.JS_ElectronicBillOfLadingTerms = ZString.Empty;

				houseBill = new ElectronicHouseBillBuilder(shipment).Build(null);
				AssertHasMessageError(houseBill.ErrorPlaceHolderInfo, @"There are validation errors on this form due to missing mandatory information. Please correct these errors before publishing.
Bill Type: Please enter Bill Type.
Bill Terms: Please enter Bill Terms.");

				shipment.JS_ElectronicBillOfLadingType = Core.Constants.BillOfLadingBillType.Codes.ToOrder;
				shipment.JS_ElectronicBillOfLadingTerms = Core.Constants.BillOfLadingBillTerms.Codes.Transferable;

				houseBill = new ElectronicHouseBillBuilder(shipment).Build(null);
				AssertNoErrors(houseBill.ErrorPlaceHolderInfo);
			});
		}

		#endregion

		#region AssertWithEnableBoleroEHBLIntegration

		void AssertWithEnabledElectronicBOL(Action assertAction)
		{
			Env.Security.MaintainShipmentAllowPublisheHBL.IsAllowed = true;

			var boleroEBLConfiguration = new BoleroEBLConfiguration()
			{
				EnableEBLIntegration = true,
				GalileoEndPointUrl = "http://test.test",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test",
				GalileoTestAudience = Guid.NewGuid().ToString()
			};

			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BoleroTitleRegisterID, "123456789");

			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			{
				assertAction();
			}
		}

		#endregion
	}
}
