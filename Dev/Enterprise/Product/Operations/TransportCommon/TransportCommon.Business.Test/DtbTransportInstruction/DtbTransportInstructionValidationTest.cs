using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.TransportCommon.Business.Testing
{
	public abstract class DtbTransportInstructionValidationTest : BusinessObjectValidationTestCase
	{
		#region TestKN_DropMode

		public void TestKN_DropMode()
		{
			var instruction = GetNewInstruction();
			instruction.Validation.ValidateKN_DropMode();
			AssertNoErrors("Precondition", instruction.KN_DropModeInfo);

			instruction.KN_DropMode = "XXX";
			AssertHasError(instruction.KN_DropModeInfo, "Enter a valid Drop Mode.");

			instruction.KN_DropMode = "SDL";
			AssertNoErrors(instruction.KN_DropModeInfo);
		}

		#endregion

		#region TestKN_InstructionType

		public void TestKN_InstructionType()
		{
			var instruction = GetNewInstruction();
			instruction.Validation.ValidateKN_InstructionType();
			AssertHasErrors(instruction.KN_InstructionTypeInfo);

			instruction.KN_InstructionType = "XXX";
			AssertHasErrors(instruction.KN_InstructionTypeInfo);

			instruction.KN_InstructionType = "PIC";
			AssertNoErrors(instruction.KN_InstructionTypeInfo);
		}

		#endregion

		#region TestOrganisationType

		public void TestOrganisationType()
		{
			var instruction = GetNewInstruction();
			instruction.Validation.ValidateOrganisationType();
			AssertHasError(instruction.OrganisationTypeInfo, "Please enter an Organization Type.");

			instruction.OrganisationType = "XXX";
			AssertHasError(instruction.OrganisationTypeInfo, "Enter a valid Organization Type.");

			instruction.OrganisationType = "CTO";
			AssertNoErrors(instruction.OrganisationTypeInfo);
		}

		#endregion

		#region TestAuthorisedToLeave

		readonly string errorMsgNoCNE = "There is no Consignee Address for this job, this means you cannot give the Authority to Leave.";
		readonly string errorMsgNoCNR = "There is no Consignor Address for this job, this means you cannot give the Authority to Leave.";
		readonly string errorMsgNoCM = "There is no Client/Billing Party Address for this job, this means you cannot give the Authority to Leave.";
		readonly string errorMsgATLFalseCNE = "The consignee/delivery address has specified that they do not provide Authority to Leave, which means you cannot give the Authority to Leave for this booking.";
		readonly string errorMsgATLFalseCNR = "The consignor/pickup address has specified that they do not provide Authority to Leave, which means you cannot give the Authority to Leave for this booking.";
		readonly string errorMsgATLFalseCM = "The local client address has specified that they do not provide Authority to Leave, which means you cannot give the Authority to Leave for this booking.";
		readonly string errorMsgIsPickup = "Authority to Leave is only available to be given for consignee deliveries.";
		readonly string errorMsgIsContainer = "Authority to Leave is not available to be given when delivering containers.";

		public void TestAuthorisedToLeave()
		{
			var data = ATLTestData();
			data.SetUpOrganizationTestData();
			data.SetUpTransportTestData(GetNewTransport(), GetNewTransport());

			AssertEquals("Should default to pickup address", data.PickupAddress1.PK, data.PickupInstruction1.Address.E2_OA_Address);
			AssertEquals("Should default to delivery address", data.DeliveryAddress1.PK, data.DeliveryInstruction1.Address.E2_OA_Address);
			AssertEquals("ATL default value is false", false, data.DeliveryInstruction1.KN_IsAuthorisedToLeave);
			data.PickupAddress1.OA_AuthorityToLeave = AuthorityToLeaveOptions.Codes.NO;
			data.DeliveryAddress1.OA_AuthorityToLeave = AuthorityToLeaveOptions.Codes.NO;
			data.ClientAddress1.OA_AuthorityToLeave = AuthorityToLeaveOptions.Codes.NO;

			data.DeliveryInstruction1.KN_IsAuthorisedToLeave = true;
			AssertHasError("User should receive error because CNE's ATL is false", data.DeliveryInstruction1.KN_IsAuthorisedToLeaveInfo, errorMsgATLFalseCNE);
			AssertHasError("User should receive error because CNR's ATL is false", data.DeliveryInstruction1.KN_IsAuthorisedToLeaveInfo, errorMsgATLFalseCNR);
			AssertHasError("User should receive error because CM's ATL is false", data.DeliveryInstruction1.KN_IsAuthorisedToLeaveInfo, errorMsgATLFalseCM);

			data.PickupAddress2.OA_AuthorityToLeave = AuthorityToLeaveOptions.Codes.YES;
			data.DeliveryAddress2.OA_AuthorityToLeave = AuthorityToLeaveOptions.Codes.YES;
			data.ClientAddress2.OA_AuthorityToLeave = AuthorityToLeaveOptions.Codes.YES;

			data.DeliveryInstruction2.KN_IsAuthorisedToLeave = true;
			AssertNoErrors("User should NOT receive any errors because all ATL's are set to true", data.DeliveryInstruction2.KN_IsAuthorisedToLeaveInfo);
		}

		public void TestAuthorisedToLeave_ConsignorYESAndConsigneeYESAndClientYES()
		{
			string[] noErrors = System.Array.Empty<string>();
			AssertAuthorityToLeave(AuthorityToLeaveOptions.Codes.YES, AuthorityToLeaveOptions.Codes.YES, AuthorityToLeaveOptions.Codes.YES, noErrors);
		}

		public void TestAuthorisedToLeave_ConsignorYESAndConsigneeYESAndClientNO()
		{
			string[] errors = { errorMsgATLFalseCM };
			AssertAuthorityToLeave(AuthorityToLeaveOptions.Codes.YES, AuthorityToLeaveOptions.Codes.YES, AuthorityToLeaveOptions.Codes.NO, errors);
		}

		public void TestAuthorisedToLeave_ConsignorYESAndConsigneeNOAndClientYES()
		{
			string[] errors = { errorMsgATLFalseCNE };
			AssertAuthorityToLeave(AuthorityToLeaveOptions.Codes.YES, AuthorityToLeaveOptions.Codes.NO, AuthorityToLeaveOptions.Codes.YES, errors);
		}

		public void TestAuthorisedToLeave_ConsignorYESAndConsigneeNOAndClientNO()
		{
			string[] errors = { errorMsgATLFalseCNE, errorMsgATLFalseCM };
			AssertAuthorityToLeave(AuthorityToLeaveOptions.Codes.YES, AuthorityToLeaveOptions.Codes.NO, AuthorityToLeaveOptions.Codes.NO, errors);
		}

		public void TestAuthorisedToLeave_ConsignorNOAndConsigneeYESAndClientYES()
		{
			string[] errors = { errorMsgATLFalseCNR };
			AssertAuthorityToLeave(AuthorityToLeaveOptions.Codes.NO, AuthorityToLeaveOptions.Codes.YES, AuthorityToLeaveOptions.Codes.YES, errors);
		}

		public void TestAuthorisedToLeave_ConsignorNOAndConsigneeYESAndClientNO()
		{
			string[] errors = { errorMsgATLFalseCNR, errorMsgATLFalseCM };
			AssertAuthorityToLeave(AuthorityToLeaveOptions.Codes.NO, AuthorityToLeaveOptions.Codes.YES, AuthorityToLeaveOptions.Codes.NO, errors);
		}

		public void TestAuthorisedToLeave_ConsignorNOAndConsigneeNOAndClientYES()
		{
			string[] errors = { errorMsgATLFalseCNR, errorMsgATLFalseCNE };
			AssertAuthorityToLeave(AuthorityToLeaveOptions.Codes.NO, AuthorityToLeaveOptions.Codes.NO, AuthorityToLeaveOptions.Codes.YES, errors);
		}

		public void TestAuthorisedToLeave_ConsignorNOAndConsigneeNOAndClientNO()
		{
			string[] errors = { errorMsgATLFalseCNR, errorMsgATLFalseCNE, errorMsgATLFalseCM };
			AssertAuthorityToLeave(AuthorityToLeaveOptions.Codes.NO, AuthorityToLeaveOptions.Codes.NO, AuthorityToLeaveOptions.Codes.NO, errors);
		}

		void AssertAuthorityToLeave(string consignorATL, string consigneeATL, string clientATL, string[] expectedValidationErrors)
		{
			var data = ATLTestData();
			data.SetUpOrganizationTestData();
			data.PickupAddress1.OA_AuthorityToLeave = consignorATL;
			data.DeliveryAddress1.OA_AuthorityToLeave = consigneeATL;
			data.ClientAddress1.OA_AuthorityToLeave = clientATL;
			Factory.Save();

			data.SetUpTransportTestData(GetNewTransport(), GetNewTransport());
			data.DeliveryInstruction1.KN_IsAuthorisedToLeave = true;
			data.DeliveryInstruction1.Validation.ValidateKN_IsAuthorisedToLeave();

			foreach (var error in expectedValidationErrors)
			{
				AssertHasError(data.DeliveryInstruction1.KN_IsAuthorisedToLeaveInfo, error);
			}

			AssertEquals("The only errors found on the delivery instruction should be the ones that were expected", data.DeliveryInstruction1.Notifications.Count(), expectedValidationErrors.Length);
			data.DeliveryInstruction1.KN_IsAuthorisedToLeave = false;
			data.DeliveryInstruction1.Validation.ValidateKN_IsAuthorisedToLeave();
			AssertNoErrors(data.DeliveryInstruction1.KN_IsAuthorisedToLeaveInfo);
		}

		OrgAddress AddAddressToOrganisation(OrgHeader organisation, ZString address1, OrgAddressType addressType)
		{
			var address = organisation.Addresses.AddNew();

			address.AddressCapability.SetCapabilityEnabled(addressType);
			address.AddressCapability.SetIsMainAddress(addressType);
			address.OA_Address1 = address1;

			return address;
		}

		public void TestValidationErrorsWhenAddressIsNull()
		{
			//Setup Instructions
			var transport = GetNewTransport();
			var pickupInstruction = (DtbTransportInstruction)transport.Instructions.AddNew();
			var deliveryInstruction = (DtbTransportInstruction)transport.Instructions.AddNew();

			//Setup Consignor
			var pickupOrganisation = Factory.New<OrgHeader>();
			pickupOrganisation.OH_Code = "PICSYD";
			pickupInstruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			pickupInstruction.OrganisationType = OrganisationTypesList.Codes.CNR;

			//Setup Consignee
			var deliveryOrganisation = Factory.New<OrgHeader>();
			deliveryOrganisation.OH_Code = "DELSYD";
			deliveryInstruction.KN_InstructionType = InstructionTypes.Codes.Delivery;
			deliveryInstruction.OrganisationType = OrganisationTypesList.Codes.CNE;

			//Setup Client
			var clientOrganisation = Factory.New<OrgHeader>();
			clientOrganisation.OH_Code = "CLISYD";
			Factory.Save();

			AssertEquals("Precondition: KN_IsAuthorisedToLeave should be false.", false, deliveryInstruction.KN_IsAuthorisedToLeave);
			deliveryInstruction.Validation.ValidateKN_IsAuthorisedToLeave();
			AssertNoErrors("Assert only returns errors if KN_IsAuthorisedToLeave is set to true", deliveryInstruction.KN_IsAuthorisedToLeaveInfo);
			deliveryInstruction.KN_IsAuthorisedToLeave = true;

			//Assert correct valudation occurs
			AssertHasError("User should receive an error after attempting to change ATL to true since CNR's address is null", deliveryInstruction.KN_IsAuthorisedToLeaveInfo, errorMsgNoCNR);
			AssertHasError("User should receive an error after attempting to change ATL to true since CNE's address is null", deliveryInstruction.KN_IsAuthorisedToLeaveInfo, errorMsgNoCNE);
			AssertHasError("User should receive an error after attempting to change ATL to true since CM's address is null", deliveryInstruction.KN_IsAuthorisedToLeaveInfo, errorMsgNoCM);
			AssertEquals("Since there is no CNR address, the user should not receive an error stating that the CNR's ATL is false", false, deliveryInstruction.KN_IsAuthorisedToLeaveInfo.HasError(errorMsgATLFalseCNR));
			AssertEquals("Since there is no CNE address, the user should not receive an error stating that the CNE's ATL is false", false, deliveryInstruction.KN_IsAuthorisedToLeaveInfo.HasError(errorMsgATLFalseCNE));
			AssertEquals("Since there is no CM address, the user should not receive an error stating that the CM's ATL is false", false, deliveryInstruction.KN_IsAuthorisedToLeaveInfo.HasError(errorMsgATLFalseCM));

			//Setup Consignor Address and re-test validation
			var pickupAddress = AddAddressToOrganisation(pickupOrganisation, "Pickup Addy", OrgAddressType.Pickup);
			pickupInstruction.Address.OrganisationPK = pickupOrganisation.PK;
			AssertEquals("User should NOT receive an error after attempting to change ATL to true since CNR's address is NO LONGER null", false, deliveryInstruction.KN_IsAuthorisedToLeaveInfo.HasError(errorMsgNoCNR));

			//Setup Consignee Address and re-test validation
			var deliveryAddress = AddAddressToOrganisation(deliveryOrganisation, "Delivery Addy", OrgAddressType.Delivery);
			deliveryInstruction.Address.OrganisationPK = deliveryOrganisation.PK;
			AssertEquals("User should NOT receive an error after attempting to change ATL to true since CNE's address is NO LONGER null", false, deliveryInstruction.KN_IsAuthorisedToLeaveInfo.HasError(errorMsgNoCNE));

			//Setup Client Address and re-test validation
			var clientAddress = AddAddressToOrganisation(clientOrganisation, "Client Addy", OrgAddressType.Miscellaneous);
			var job = new JobHeader.Loader(transport).TryCreate();
			job.JH_OA_LocalChargesAddr = clientAddress.PK;
			clientAddress = deliveryInstruction.Booking.BillingPartyOrLocalClientAddress;
			AssertEquals("User should NOT receive an error after attempting to change ATL to true since CM's address is NO LONGER null", false, deliveryInstruction.KN_IsAuthorisedToLeaveInfo.HasError(errorMsgNoCM));
		}

		public void TestValidationIfIsPickup()
		{
			var data = ATLTestData();
			data.SetUpOrganizationTestData();
			data.SetUpTransportTestData(GetNewTransport(), GetNewTransport());
			data.DeliveryInstruction1.KN_IsAuthorisedToLeave = true;
			AssertNoError(data.DeliveryInstruction1.KN_IsAuthorisedToLeaveInfo, errorMsgIsPickup);

			data.DeliveryInstruction1.KN_InstructionType = InstructionTypes.Codes.PickUp;
			data.DeliveryInstruction1.KN_IsAuthorisedToLeave = true;
			data.DeliveryInstruction1.Validation.ValidateKN_IsAuthorisedToLeave();
			AssertHasError(data.DeliveryInstruction1.KN_IsAuthorisedToLeaveInfo, errorMsgIsPickup);
		}

		public void TestValidationIfHasContainer()
		{
			// Setup Data
			var data = ATLTestData();
			data.SetUpOrganizationTestData();
			data.SetUpTransportTestData(GetNewTransport(), GetNewTransport());
			AssertEquals("Precondition: Package count should be 0", 0, data.DeliveryInstruction1.DivotsWithPackages.Typed.Count());
			data.DeliveryInstruction1.KN_IsAuthorisedToLeave = true;
			AssertNoError("There are no packages for this delivery, therefore the user should not receive an error regarding containers/packages", data.DeliveryInstruction1.KN_IsAuthorisedToLeaveInfo, errorMsgIsContainer);

			// Add package
			var package = data.Transport1.PackageJob.Packages.AddNew("BOX", 1);
			data.DeliveryInstruction1.DivotsWithPackages.AddPackage(package);
			AssertEquals("Precondition: There should be 1 package on the instruction", 1, data.DeliveryInstruction1.DivotsWithPackages.Typed.Count());
			data.DeliveryInstruction1.KN_IsAuthorisedToLeave = true;
			AssertNoError("There are no containers for this delivery, therefore the user should not receive an error regarding containers", data.DeliveryInstruction1.KN_IsAuthorisedToLeaveInfo, errorMsgIsContainer);

			// Add container
			var container = data.Transport1.PackageJob.Packages.AddNew("CNT", 1);
			data.DeliveryInstruction1.DivotsWithPackages.AddPackage(container);
			AssertEquals("Precondition: There should be 2 packages on the instruction", 2, data.DeliveryInstruction1.DivotsWithPackages.Typed.Count());
			data.DeliveryInstruction1.KN_IsAuthorisedToLeave = true;
			data.DeliveryInstruction1.Validation.ValidateKN_IsAuthorisedToLeave();
			AssertHasError("A container has been added for delivery, therefore the user should be unable to give authority to leave and should receive an error", data.DeliveryInstruction1.KN_IsAuthorisedToLeaveInfo, errorMsgIsContainer);
		}

		#endregion

		#region TestValidateAll

		public void TestValidateAll()
		{
			var instruction = GetNewInstruction();
			instruction.OrganisationType = "CTO";
			AssertNoErrors("Precondition", instruction.OrganisationTypeInfo);

			using (instruction.GetValidationSuspender())
			{
				instruction.OrganisationType = "XXX";
			}

			instruction.Validation.ValidateAll();
			AssertHasError(instruction.OrganisationTypeInfo, "Enter a valid Organization Type.");
		}

		#endregion

		#region Implementation

		protected DtbTransportInstructionTest<DtbTransportInstruction>.ATLTestData ATLTestData()
		{
			return new DtbTransportInstructionTest<DtbTransportInstruction>.ATLTestData(Helper);
		}

		protected abstract DtbTransportInstruction GetNewInstruction();

		protected abstract DtbTransport GetNewTransport();

		protected TransportCommonTestHelper Helper
		{
			get { return helper ?? (helper = GetNewTestHelper()); }
		}

		protected virtual TransportCommonTestHelper GetNewTestHelper()
		{
			return new TransportCommonTestHelper(Factory);
		}

		TransportCommonTestHelper helper;

		#endregion
	}
}
