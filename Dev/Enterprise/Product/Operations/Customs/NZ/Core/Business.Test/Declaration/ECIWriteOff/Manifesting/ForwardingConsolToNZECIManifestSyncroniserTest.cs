using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting.Testing
{
	using Enterprise.Freight.Business;

	public class ForwardingConsolToNZECIManifestSyncroniserTest : TestCaseWithFactory
	{
		public void TestCanSyncronise()
		{
			NZCustomsDataRegistry.Instance.UpdateAttachedManifestedECIsWhenConsolDetailsChange.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save(); // Got to save, or HasChanges never fires.

			JobDeclaration declaration1 = GetLinkedDeclaration(consol, JobMessageTypeList.Codes.Import
					, JobMessageSubTypeList.Codes.WriteOff, "M00009199-1", GlbBranch.CurrentBranch);
			SetDecValues(declaration1, new ZDateTime(2007, 3, 1), new ZDateTime(2007, 3, 2), "333-45678905", "QF333", "333", null, "USTHR", "NZTHR");

			JobDeclaration declaration2 = GetLinkedDeclaration(consol, JobMessageTypeList.Codes.Export
					, JobMessageSubTypeList.Codes.WriteOff, "M00009200-1", GlbBranch.CurrentBranch);
			SetDecValues(declaration2, new ZDateTime(2007, 3, 1), new ZDateTime(2007, 3, 2), "333-45678905", "QF333", "333", null, "USTHR", "NZTHR");

			var syncroniser = new ForwardingConsolToNZECIManifestSyncroniser();

			syncroniser.Syncronise(consol);
			AssertDecValues(declaration1, new ZDateTime(2007, 3, 1), new ZDateTime(2007, 3, 2), "333-45678905", "QF333", "333", null, "USTHR", "NZTHR");
			AssertDecValues(declaration2, new ZDateTime(2007, 3, 1), new ZDateTime(2007, 3, 2), "333-45678905", "QF333", "333", null, "USTHR", "NZTHR");

			SetConsolValues(consol, new ZDateTime(2007, 1, 1), new ZDateTime(2007, 1, 2), "123-45678905", "QF343", "FOLION", org1, "USLAX", "NZCHC");
			syncroniser.Syncronise(consol);
			AssertDecValues(declaration1, new ZDateTime(2007, 1, 1), new ZDateTime(2007, 1, 2), "123-45678905", "QF343", "FOLION", org1, "USLAX", "NZCHC");
			AssertDecValues(declaration2, new ZDateTime(2007, 1, 1), new ZDateTime(2007, 1, 2), "123-45678905", "QF343", "FOLION", org1, "USTHR", "NZTHR"); // Ports not updated as there is no Export Leg.

			SetConsolValues(consol, new ZDateTime(2007, 2, 1), new ZDateTime(2007, 2, 2), "456-12378905", "QF212", "NOFOGO", org2, "NZAKL", "USCHC");
			syncroniser.Syncronise(consol);
			AssertDecValues(declaration1, new ZDateTime(2007, 2, 1), new ZDateTime(2007, 2, 2), "456-12378905", "QF212", "NOFOGO", org2, "USLAX", "NZCHC"); // Ports not updated as there is no Import Leg.
			AssertDecValues(declaration2, new ZDateTime(2007, 2, 1), new ZDateTime(2007, 2, 2), "456-12378905", "QF212", "NOFOGO", org2, "NZAKL", "USCHC");
		}

		public void TestCantSyncroniseByDefault()
		{
			AssertEquals("Precondition: UpdateAttachedManifestedECIsWhenConsolDetailsChange.Value", false, NZCustomsDataRegistry.Instance.UpdateAttachedManifestedECIsWhenConsolDetailsChange.Value);

			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save(); // Got to save, or HasChanges never fires.

			JobDeclaration declaration1 = GetLinkedDeclaration(consol, JobMessageTypeList.Codes.Import
					, JobMessageSubTypeList.Codes.WriteOff, "M00009199-1", GlbBranch.CurrentBranch);
			SetDecValues(declaration1, new ZDateTime(2007, 3, 1), new ZDateTime(2007, 3, 2), "333-45678905", "QF333", "333", null, "USTHR", "NZTHR");

			SetConsolValues(consol, new ZDateTime(2007, 1, 1), new ZDateTime(2007, 1, 2), "123-45678905", "QF343", "FOLION", org1, "USLAX", "NZCHC");

			var syncroniser = new ForwardingConsolToNZECIManifestSyncroniser();
			syncroniser.Syncronise(consol);
			AssertDecValues(declaration1, new ZDateTime(2007, 3, 1), new ZDateTime(2007, 3, 2), "333-45678905", "QF333", "333", null, "USTHR", "NZTHR");
		}

		public void TestConsolDoesNotUpdateAllECIDeclarationsWhenNoLinkedShipments()
		{
			NZCustomsDataRegistry.Instance.UpdateAttachedManifestedECIsWhenConsolDetailsChange.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save(); // Got to save, or HasChanges never fires.
			SetConsolValues(consol, new ZDateTime(2007, 1, 1), new ZDateTime(2007, 1, 2), "123-45678905", "QF343", "FOLION", Factory.NewWithValidTestData<OrgHeader>(), "USLAX", "NZCHC");

			var declaration1 = GetDeclaration(JobMessageTypeList.Codes.Import, JobMessageSubTypeList.Codes.WriteOff, "M00009199-1", GlbBranch.CurrentBranch);
			SetDecValues(declaration1, new ZDateTime(2007, 3, 1), new ZDateTime(2007, 3, 2), "333-45678905", "QF333", "333", null, "USTHR", "NZTHR");

			var declaration2 = GetDeclaration(JobMessageTypeList.Codes.Export, JobMessageSubTypeList.Codes.WriteOff, "M00009200-1", GlbBranch.CurrentBranch);
			SetDecValues(declaration2, new ZDateTime(2007, 3, 1), new ZDateTime(2007, 3, 2), "333-45678905", "QF333", "333", null, "USTHR", "NZTHR");

			var syncroniser = new ForwardingConsolToNZECIManifestSyncroniser();
			syncroniser.Syncronise(consol);
			AssertDecValues(declaration1, new ZDateTime(2007, 3, 1), new ZDateTime(2007, 3, 2), "333-45678905", "QF333", "333", null, "USTHR", "NZTHR");
			AssertDecValues(declaration2, new ZDateTime(2007, 3, 1), new ZDateTime(2007, 3, 2), "333-45678905", "QF333", "333", null, "USTHR", "NZTHR");
		}

		JobDeclaration GetLinkedDeclaration(ForwardingConsol consol
		, ZString messageType, ZString messageSubType, ZString declarationReference, GlbBranch branch)
		{
			var shipment = consol.Shipments.AddNew();
			shipment.FillWithValidTestData();
			var result = GetDeclaration(messageType, messageSubType, declarationReference, branch);
			result.JE_OverrideFreightDefaults = true;
			result.JE_JS = shipment.PK;
			return result;
		}

		JobDeclaration GetDeclaration(ZString messageType, ZString messageSubType, ZString declarationReference, GlbBranch branch)
		{
			var result = Factory.New<JobDeclaration>();
			result.JE_MessageType = messageType;
			result.JE_MessageSubType = messageSubType;
			result.JE_DeclarationReference = declarationReference;
			result.JE_GB = branch.PK;
			return result;
		}

		void SetConsolValues(ForwardingConsol consol, ZDateTime departureDate, ZDateTime arrivalDate, ZString masterBill
				, ZString voyageOrFlightNo, ZString vesselName, OrgHeader shippingLine, ZString loadPort, ZString discPort)
		{
			Transport consolTransport = consol.MostInterestingTransportForBinding[0];
			consol.SetDefaultShippingLineAddress(shippingLine);
			consol.JK_MasterBillNum = masterBill;
			consolTransport.JW_ETD = departureDate;
			consolTransport.JW_ETA = arrivalDate;
			consolTransport.JW_VoyageFlight = voyageOrFlightNo;
			consolTransport.JW_Vessel = vesselName;
			consolTransport.JW_RL_NKLoadPort = loadPort;
			consolTransport.JW_RL_NKDiscPort = discPort;
		}

		void SetDecValues(JobDeclaration declaration, ZDateTime departureDate, ZDateTime arrivalDate, ZString masterBill
				, ZString voyageOrFlightNo, ZString vesselName, OrgHeader shippingLine, ZString loadPort, ZString discPort)
		{
			declaration.JE_ExportDate = departureDate;
			declaration.JE_DateOfArrival = arrivalDate;
			declaration.JE_MasterBill = masterBill;
			declaration.JE_VoyageFlightNo = voyageOrFlightNo;
			declaration.JE_VesselName = vesselName;
			declaration.JE_OH_ShippingLine = shippingLine == null ? ZGuid.Empty : shippingLine.PK;
			declaration.JE_RL_NKPortOfLoading = loadPort;
			declaration.JE_RL_NKPortOfArrival = discPort;
		}

		void AssertDecValues(JobDeclaration declaration, ZDateTime departureDate, ZDateTime arrivalDate, ZString masterBill
				, ZString voyageOrFlightNo, ZString vesselName, OrgHeader shippingLine, ZString loadPort, ZString discPort)
		{
			AssertEquals("declaration.JE_ExportDate", departureDate, declaration.JE_ExportDate);
			AssertEquals("declaration.JE_DateOfArrival", arrivalDate, declaration.JE_DateOfArrival);
			AssertEquals("declaration.JE_MasterBill", masterBill, declaration.JE_MasterBill);
			AssertEquals("declaration.JE_VoyageFlightNo", voyageOrFlightNo, declaration.JE_VoyageFlightNo);
			AssertEquals("declaration.JE_VesselName", vesselName, declaration.JE_VesselName);
			AssertEquals("declaration.JE_OH_ShippingLine", shippingLine, declaration.ShippingLine);
			AssertEquals("declaration.JE_RL_NKPortOfLoading", loadPort, declaration.JE_RL_NKPortOfLoading);
			AssertEquals("declaration.JE_RL_NKPortOfArrival", discPort, declaration.JE_RL_NKPortOfArrival);
		}
	}
}
