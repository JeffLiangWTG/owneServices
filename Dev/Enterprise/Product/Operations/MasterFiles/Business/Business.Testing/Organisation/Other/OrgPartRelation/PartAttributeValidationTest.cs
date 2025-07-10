using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.Warehouse;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class PartAttributeValidationTest : TestCaseWithFactory
	{
		#region TestCheckExpiryDate

		public void TestCheckExpiryDate()
		{
			using (Dummy.SuspendValidationTesting())
			{
				// expiry date not set on org or product so no errors should be generated
				Dummy.Z0_DateInfo.ClearAllNotifications();
				Dummy.Z0_Date = ZDateTime.Empty;
				Validation.CheckExpiryDate(Org, Part, Dummy.Z0_DateInfo);
				AssertNoPartAttributeNotifications(Dummy.Z0_DateInfo);

				Dummy.Z0_Date = ZDateTime.Today;
				Validation.CheckExpiryDate(Org, Part, Dummy.Z0_DateInfo);
				AssertShouldNotHaveAttributeNotification(Dummy.Z0_DateInfo, "Expiry Date");

				Org.PartAttributeManager.SetProductToUseAttribute(Part, 4, true);

				// expiry date set on product but not org so no errors should not be generated
				Dummy.Z0_Date = ZDateTime.Empty;
				Validation.CheckExpiryDate(Org, Part, Dummy.Z0_DateInfo);
				AssertShouldNotHaveAttributeNotification(Dummy.Z0_DateInfo, "Expiry Date");

				Dummy.Z0_Date = ZDateTime.Today;
				Validation.CheckExpiryDate(Org, Part, Dummy.Z0_DateInfo);
				AssertShouldNotHaveAttributeNotification(Dummy.Z0_DateInfo, "Expiry Date");

				Org.MiscServ.OM_IMUseExpiryDate = true;
				// expiry date now set on both org and product so errors should be generated

				Dummy.Z0_Date = ZDateTime.Empty;
				Validation.CheckExpiryDate(Org, Part, Dummy.Z0_DateInfo);
				AssertHasPartAttributeNotification(Dummy.Z0_DateInfo, "Please enter an Expiry date.");

				Dummy.Z0_Date = ZDateTime.Today;
				Validation.CheckExpiryDate(Org, Part, Dummy.Z0_DateInfo);
				AssertNoPartAttributeNotifications(Dummy.Z0_DateInfo);
			}
		}

		#endregion

		#region TestCheckPackingDate

		public void TestCheckPackingDate()
		{
			using (Dummy.SuspendValidationTesting())
			{
				// packing date not set on org or product so no errors should not be generated
				Dummy.Z0_DateInfo.ClearAllNotifications();
				Dummy.Z0_Date = ZDateTime.Empty;
				Validation.CheckPackingDate(Org, Part, Dummy.Z0_DateInfo);
				AssertNoPartAttributeNotifications(Dummy.Z0_DateInfo);

				Dummy.Z0_Date = ZDateTime.Today;
				Validation.CheckPackingDate(Org, Part, Dummy.Z0_DateInfo);
				AssertShouldNotHaveAttributeNotification(Dummy.Z0_DateInfo, "Packing Date");

				Org.PartAttributeManager.SetProductToUseAttribute(Part, 5, true);

				// packing date set on product but not org so no errors should not be generated
				Dummy.Z0_Date = ZDateTime.Empty;
				Validation.CheckPackingDate(Org, Part, Dummy.Z0_DateInfo);
				AssertShouldNotHaveAttributeNotification(Dummy.Z0_DateInfo, "Packing Date");

				Dummy.Z0_Date = ZDateTime.Today;
				Validation.CheckPackingDate(Org, Part, Dummy.Z0_DateInfo);
				AssertShouldNotHaveAttributeNotification(Dummy.Z0_DateInfo, "Packing Date");

				Org.MiscServ.OM_IMUsePackingDate = true;
				// packing date now set on both org and product so errors should be generated

				Dummy.Z0_Date = ZDateTime.Empty;
				Validation.CheckPackingDate(Org, Part, Dummy.Z0_DateInfo);
				AssertHasPartAttributeNotification(Dummy.Z0_DateInfo, "Please enter a Packing date.");

				Dummy.Z0_Date = ZDateTime.Today;
				Validation.CheckPackingDate(Org, Part, Dummy.Z0_DateInfo);
				AssertNoPartAttributeNotifications(Dummy.Z0_DateInfo);
			}
		}

		#endregion

		#region TestNoErrorsWithNonMandatoryAttribute

		public void TestNoErrorsWithNonMandatoryAttribute()
		{
			using (Dummy.SuspendValidationTesting())
			{
				Dummy.Z0_DescriptionInfo.ClearAllNotifications();
				Dummy.Z0_Description = "";
				Validation.CheckAttribute(Org, Part, Dummy.Z0_DescriptionInfo, 1);
				AssertNoPartAttributeNotifications(Dummy.Z0_DescriptionInfo);

				Dummy.Z0_Description = "1";
				Validation.CheckAttribute(Org, Part, Dummy.Z0_DescriptionInfo, 1);
				AssertShouldNotHaveAttributeNotification(Dummy.Z0_DescriptionInfo, 1);

				Org.PartAttributeManager.SetProductToUseAttribute(Part, 1, true);
				Org.MiscServ.OM_IMPartAttrib1Name = "Model";
				Org.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.NonMandatory;

				Dummy.Z0_Description = "2";
				Validation.CheckAttribute(Org, Part, Dummy.Z0_DescriptionInfo, 1);
				AssertNoPartAttributeNotifications(Dummy.Z0_DescriptionInfo);

				Dummy.Z0_Description = "";
				Validation.CheckAttribute(Org, Part, Dummy.Z0_DescriptionInfo, 1);
				AssertNoPartAttributeNotifications(Dummy.Z0_DescriptionInfo);
			}
		}

		#endregion

		#region TestCheckAttribute

		public void TestCheckAttribute()
		{
			Org.CompanyData.OB_IMUsedBondedWhs = false;
			var dummy = Factory.New<DummyWithWarehouseAndOrderManagerProperty>();

			for (var a = 1; a < 6; a++)
			{
				AssertMandatoryAttribute(dummy, a);
			}
		}

		#endregion

		#region TestCheckSerialNumber

		public void TestCheckSerialNumber_NullOrg()
		{
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			var dummy = Factory.New<DummyWithWarehouseAndOrderManagerProperty>();

			var whsNonExposedFeatureMock = new Mock<IWhsNonExposedFeature>();
			using (ObjectFactory.Substitute(whsNonExposedFeatureMock.Object))
			{
				Validation.CheckSerialNumber(null, part, dummy.Z0_DescriptionInfo);
				AssertNoPartAttributeNotifications(dummy.Z0_DescriptionInfo);
			}
		}

		public void TestCheckSerialNumber_NullPart()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var dummy = Factory.New<DummyWithWarehouseAndOrderManagerProperty>();

			var whsNonExposedFeatureMock = new Mock<IWhsNonExposedFeature>();
			using (ObjectFactory.Substitute(whsNonExposedFeatureMock.Object))
			{
				Validation.CheckSerialNumber(org, null, dummy.Z0_DescriptionInfo);
				AssertNoPartAttributeNotifications(dummy.Z0_DescriptionInfo);
			}
		}

		#endregion

		#region TestCheckSerialNumber_MandatoryAttributeCheck

		public void TestCheckSerialNumber_MandatoryAttributeCheck()
		{
			Org.CompanyData.OB_IMUsedBondedWhs = false;
			var dummy = Factory.New<DummyWithWarehouseAndOrderManagerProperty>();
			TestCheckSerialNumber_MandatoryAttributeCheckCore(dummy);
		}

		public void TestCheckSerialNumber_WithUseBondedWarehouseAutomation()
		{
			Org.CompanyData.OB_IMUsedBondedWhs = true;
			var dummy = Factory.New<DummyWithWarehouseAndOrderManagerProperty>();
			TestCheckSerialNumber_MandatoryAttributeCheckCore(dummy);
		}

		void TestCheckSerialNumber_MandatoryAttributeCheckCore(DummyWithWarehouseAndOrderManagerProperty dummy)
		{
			using (dummy.SuspendValidationTesting())
			{
				dummy.Z0_DescriptionInfo.ClearAllNotifications();
				dummy.Z0_Description = "";
				Validation.CheckSerialNumber(Org, Part, dummy.Z0_DescriptionInfo);
				AssertNoPartAttributeNotifications(dummy.Z0_DescriptionInfo);

				dummy.Z0_Description = "1";
				Validation.CheckSerialNumber(Org, Part, dummy.Z0_DescriptionInfo);
				AssertShouldNotHaveAttributeNotification(dummy.Z0_DescriptionInfo, "Serial Number");

				dummy.WI_Calculated = "1";
				Validation.CheckSerialNumber(Org, Part, dummy.WI_CalculatedInfo);
				AssertShouldNotHaveAttributeNotification(dummy.Z0_DescriptionInfo, "Serial Number");

				dummy.JO_Calculated = "1";
				Validation.CheckSerialNumber(Org, Part, dummy.JO_CalculatedInfo);
				AssertShouldNotHaveAttributeNotification(dummy.Z0_DescriptionInfo, "Serial Number");

				// attribute set on part but not org
				Org.PartAttributeManager.SetProductToUseAttribute(Part, 6, true);

				dummy.Z0_Description = "";
				Validation.CheckSerialNumber(Org, Part, dummy.Z0_DescriptionInfo);
				AssertNoPartAttributeNotifications(dummy.Z0_DescriptionInfo);

				dummy.Z0_Description = "1";
				Validation.CheckSerialNumber(Org, Part, dummy.Z0_DescriptionInfo);
				AssertShouldNotHaveAttributeNotification(dummy.Z0_DescriptionInfo, "Serial Number");

				// part attribute now set on both org and part so errors should be generated
				Org.MiscServ.OM_IMUseSerialNumber = true;

				dummy.Z0_Description = "";
				Validation.CheckSerialNumber(Org, Part, dummy.Z0_DescriptionInfo);
				AssertHasPartAttributeNotification(dummy.Z0_DescriptionInfo, "Please enter a Serial Number.");

				dummy.Z0_Description = "1";
				Validation.CheckSerialNumber(Org, Part, dummy.Z0_DescriptionInfo);
				AssertNoPartAttributeNotifications(dummy.Z0_DescriptionInfo);

				Org.PartAttributeManager.SetProductToUseAttribute(Part, 6, false);
				ClearOrgNameAndType();
			}
		}

		#endregion

		#region TestCheckAttributeWithUseBondedWarehouseAutomation

		public void TestCheckAttributeWithUseBondedWarehouseAutomation()
		{
			Org.CompanyData.OB_IMUsedBondedWhs = true;

			var dummy = Factory.New<DummyWithWarehouseAndOrderManagerProperty>();
			for (int a = 1; a < 5; a++)
			{
				AssertMandatoryAttribute(dummy, a);
			}
		}

		#endregion

		#region TestCheckExpiryDateDefinitionForOrganisation

		public void TestCheckExpiryDateDefinitionForOrganisation()
		{
			InsertOrgHeaderIntoDB(Org, 1);

			OrgSupplierPart part1 = Factory.NewWithValidTestData<OrgSupplierPart>();
			OrgPartRelation relation1 = part1.RelatedOrganisations.AddNew();
			part1.OP_PartNum = "P2";
			relation1.OU_OH = Org.PK;
			relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			Factory.Save();

			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			PartAttributeValidation validation = new PartAttributeValidation();

			using (dummy.SuspendValidationTesting())
			{
				// simple check to make sure a null org doesnt crash the routine
				validation.CheckExpiryDateDefinitionForOrganisation(Factory, null, dummy.Z0_BoolInfo);

				dummy.Z0_Bool = true; // turning the expiry date definition on
				Factory.Save();
				validation.CheckExpiryDateDefinitionForOrganisation(Factory, Org, dummy.Z0_BoolInfo);
				AssertNoErrors("can always turn on", dummy.Z0_BoolInfo);

				Relation.OU_UseExpiryDate = true;
				relation1.OU_UseExpiryDate = true;
				Factory.Save();
				dummy.Z0_Bool = false; // turning the expiry date definition off
				validation.CheckExpiryDateDefinitionForOrganisation(Factory, Org, dummy.Z0_BoolInfo);
				AssertHasErrors("cannot turn off if a product is using expiry date", dummy.Z0_BoolInfo);

				Relation.OU_UseExpiryDate = false;
				Factory.Save();
				validation.CheckExpiryDateDefinitionForOrganisation(Factory, Org, dummy.Z0_BoolInfo);
				AssertHasErrors("still cannot turn off as one relation still uses expiry date", dummy.Z0_BoolInfo);

				relation1.OU_UseExpiryDate = false;
				Factory.Save();
				validation.CheckExpiryDateDefinitionForOrganisation(Factory, Org, dummy.Z0_BoolInfo);
				AssertHasErrors("cant turn off now as no relations use expiry date", dummy.Z0_BoolInfo);
			}
		}

		#endregion

		#region TestCheckExpiryDateDefinitionForPart

		public void TestCheckExpiryDateDefinitionForPart()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var whs = helper.CreateWarehouse("WH1", "A", 2, 1);
			var client1 = Factory.Load<OrgHeader>(helper.CreateClient("CLIENT1"));
			var client2 = Factory.Load<OrgHeader>(helper.CreateClient("CLIENT2"));
			var client3 = Factory.Load<OrgHeader>(helper.CreateClient("CLIENT3"));
			var client4 = Factory.Load<OrgHeader>(helper.CreateClient("CLIENT4"));
			var part = (OrgSupplierPart)helper.CreateProduct(client1.PK, "P1");

			var relation1 = part.RelatedOrganisations.AddOwner(client2);
			SetOrgMiscServAttribs(client2, PartAttributeTypeList.Codes.NonMandatory);
			SetOrgMiscServDates(client2, true);
			SetPartRelationAttribs(relation1, true);
			SetPartRelationDates(relation1, true);

			var relation2 = part.RelatedOrganisations.AddOwner(client3);

			var relation3 = part.RelatedOrganisations.AddOwner(client4);
			SetOrgMiscServAttribs(client4, PartAttributeTypeList.Codes.NonMandatory);
			SetPartRelationAttribs(relation2, true);
			SetPartRelationAttribs(relation3, true);

			var receive1PK = helper.CreateWhsReceive(client2.PK, whs.PK, "R1", new NotificationBuffer());
			var inventory1PK = helper.CreateWhsReceiveInventoryLine(receive1PK, part.PK, 10m, ZDate.Today.AddDays(10), ZDate.Today.AddDays(-10), "PA1", "PA2", "PA3", "KEY-1");
			helper.WhsReceiveAllocateLocationsMock(receive1PK);
			helper.FinaliseDocketWithoutUserConfirmation(receive1PK);

			var receive2PK = helper.CreateWhsReceive(client3.PK, whs.PK, "R2", new NotificationBuffer());
			var inventory2PK = helper.CreateWhsReceiveInventoryLine(receive2PK, part.PK, 0m, "");
			helper.FinaliseDocketWithoutUserConfirmation(receive2PK);

			var receive3PK = helper.CreateWhsReceive(client4.PK, whs.PK, "R3", new NotificationBuffer());
			var inventory3PK = helper.CreateWhsReceiveInventoryLine(receive3PK, part.PK, 10m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "PA3", "KEY-1");
			helper.WhsReceiveAllocateLocationsMock(receive3PK);
			helper.FinaliseDocketWithoutUserConfirmation(receive3PK);

			Factory.Save();

			var dummy = Factory.New<DummyBusinessObject>();
			var validation = new PartAttributeValidation();
			using (dummy.SuspendValidationTesting())
			{
				validation.CheckPackingDateDefinitionForPart(Factory, null, null, dummy.Z0_BoolInfo);

				dummy.Z0_Bool = true;
				validation.CheckExpiryDateDefinitionForPart(Factory, client1, part, dummy.Z0_BoolInfo);
				AssertNoErrors("this org has no inventory so should not get an error", dummy.Z0_BoolInfo);
				validation.CheckExpiryDateDefinitionForPart(Factory, client2, part, dummy.Z0_BoolInfo);
				AssertNoErrors("this org has inventory with expiry dates so should not get an error", dummy.Z0_BoolInfo);
				validation.CheckExpiryDateDefinitionForPart(Factory, client3, part, dummy.Z0_BoolInfo);
				AssertNoErrors("this org has inventory with expiry dates but no current units so should not get error", dummy.Z0_BoolInfo);
				validation.CheckExpiryDateDefinitionForPart(Factory, client4, part, dummy.Z0_BoolInfo);
				AssertHasErrors("this org has inventory with no expiry dates so should get error", dummy.Z0_BoolInfo);

				dummy.Z0_Bool = false;
				validation.CheckExpiryDateDefinitionForPart(Factory, client1, part, dummy.Z0_BoolInfo);
				AssertNoErrors("this org has no inventory so should not get an error", dummy.Z0_BoolInfo);
				validation.CheckExpiryDateDefinitionForPart(Factory, client3, part, dummy.Z0_BoolInfo);
				AssertNoErrors("this org has inventory with expiry dates but no current units so should not get an error", dummy.Z0_BoolInfo);
				validation.CheckExpiryDateDefinitionForPart(Factory, client4, part, dummy.Z0_BoolInfo);
				AssertNoErrors("this org has inventory with no expiry dates so should not get error", dummy.Z0_BoolInfo);
				validation.CheckExpiryDateDefinitionForPart(Factory, client2, part, dummy.Z0_BoolInfo);
				AssertHasErrors("this org has inventory with expiry dates so should get an error", dummy.Z0_BoolInfo);
			}
		}

		#endregion

		#region TestCheckPackingDateDefinitionForOrganisation

		public void TestCheckPackingDateDefinitionForOrganisation()
		{
			InsertOrgHeaderIntoDB(Org, 1);

			OrgSupplierPart part1 = Factory.NewWithValidTestData<OrgSupplierPart>();
			OrgPartRelation relation1 = part1.RelatedOrganisations.AddNew();
			part1.OP_PartNum = "P2";
			relation1.OU_OH = Org.PK;
			relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			Factory.Save();

			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			PartAttributeValidation validation = new PartAttributeValidation();

			using (dummy.SuspendValidationTesting())
			{
				// simple check to make sure a null org doesnt crash the routine
				validation.CheckPackingDateDefinitionForOrganisation(Factory, null, dummy.Z0_BoolInfo);

				dummy.Z0_Bool = true; // turning the packing date definition on
				Factory.Save();
				validation.CheckPackingDateDefinitionForOrganisation(Factory, Org, dummy.Z0_BoolInfo);
				AssertNoErrors("can always turn on", dummy.Z0_BoolInfo);

				Relation.OU_UsePackingDate = true;
				relation1.OU_UsePackingDate = true;
				Factory.Save();
				dummy.Z0_Bool = false; // turning the packing date definition off
				validation.CheckPackingDateDefinitionForOrganisation(Factory, Org, dummy.Z0_BoolInfo);
				AssertHasErrors("cannot turn off if a product is using packing date", dummy.Z0_BoolInfo);

				Relation.OU_UsePackingDate = false;
				Factory.Save();
				validation.CheckPackingDateDefinitionForOrganisation(Factory, Org, dummy.Z0_BoolInfo);
				AssertHasErrors("still cannot turn off as one relation still uses packing date", dummy.Z0_BoolInfo);

				relation1.OU_UsePackingDate = false;
				Factory.Save();
				validation.CheckPackingDateDefinitionForOrganisation(Factory, Org, dummy.Z0_BoolInfo);
				AssertHasErrors("cant turn off now as no relations use packing date", dummy.Z0_BoolInfo);
			}
		}

		#endregion

		#region TestCheckPackingDateDefinitionForPart

		public void TestCheckPackingDateDefinitionForPart()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var whs = helper.CreateWarehouse("WH1", "A", 2, 1);
			var client1 = Factory.Load<OrgHeader>(helper.CreateClient("CLIENT1"));
			var client2 = Factory.Load<OrgHeader>(helper.CreateClient("CLIENT2"));
			var client3 = Factory.Load<OrgHeader>(helper.CreateClient("CLIENT3"));
			var client4 = Factory.Load<OrgHeader>(helper.CreateClient("CLIENT4"));
			var part = (OrgSupplierPart)helper.CreateProduct(client1.PK, "P1");

			var relation1 = part.RelatedOrganisations.AddOwner(client2);
			SetOrgMiscServAttribs(client2, PartAttributeTypeList.Codes.NonMandatory);
			SetOrgMiscServDates(client2, true);
			SetPartRelationAttribs(relation1, true);
			SetPartRelationDates(relation1, true);

			var relation2 = part.RelatedOrganisations.AddOwner(client3);

			var relation3 = part.RelatedOrganisations.AddOwner(client4);
			SetOrgMiscServAttribs(client4, PartAttributeTypeList.Codes.NonMandatory);
			SetPartRelationAttribs(relation2, true);
			SetPartRelationAttribs(relation3, true);

			var receive1PK = helper.CreateWhsReceive(client2.PK, whs.PK, "R1", new NotificationBuffer());
			var inventory1PK = helper.CreateWhsReceiveInventoryLine(receive1PK, part.PK, 10m, ZDate.Today.AddDays(10), ZDate.Today.AddDays(-10), "PA1", "PA2", "PA3", "KEY-1");
			helper.WhsReceiveAllocateLocationsMock(receive1PK);
			helper.FinaliseDocketWithoutUserConfirmation(receive1PK);

			var receive2PK = helper.CreateWhsReceive(client3.PK, whs.PK, "R2", new NotificationBuffer());
			var inventory2PK = helper.CreateWhsReceiveInventoryLine(receive2PK, part.PK, 0m, "");
			helper.FinaliseDocketWithoutUserConfirmation(receive2PK);

			var receive3PK = helper.CreateWhsReceive(client4.PK, whs.PK, "R3", new NotificationBuffer());
			var inventory3PK = helper.CreateWhsReceiveInventoryLine(receive3PK, part.PK, 10m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "PA3", "KEY-1");
			helper.WhsReceiveAllocateLocationsMock(receive3PK);
			helper.FinaliseDocketWithoutUserConfirmation(receive3PK);

			Factory.Save();

			var dummy = Factory.New<DummyBusinessObject>();
			var validation = new PartAttributeValidation();
			using (dummy.SuspendValidationTesting())
			{
				validation.CheckPackingDateDefinitionForPart(Factory, null, null, dummy.Z0_BoolInfo);

				dummy.Z0_Bool = true;
				validation.CheckPackingDateDefinitionForPart(Factory, client1, part, dummy.Z0_BoolInfo);
				AssertNoErrors("this org has no inventory so should not get an error", dummy.Z0_BoolInfo);
				validation.CheckPackingDateDefinitionForPart(Factory, client2, part, dummy.Z0_BoolInfo);
				AssertNoErrors("this org has inventory with packing dates so should not get an error", dummy.Z0_BoolInfo);
				validation.CheckPackingDateDefinitionForPart(Factory, client3, part, dummy.Z0_BoolInfo);
				AssertNoErrors("this org has inventory with packing dates but no current units so should not get error", dummy.Z0_BoolInfo);
				validation.CheckPackingDateDefinitionForPart(Factory, client4, part, dummy.Z0_BoolInfo);
				AssertHasErrors("this org has inventory with no packing dates so should get error", dummy.Z0_BoolInfo);

				dummy.Z0_Bool = false;
				validation.CheckPackingDateDefinitionForPart(Factory, client1, part, dummy.Z0_BoolInfo);
				AssertNoErrors("this org has no inventory so should not get an error", dummy.Z0_BoolInfo);
				validation.CheckPackingDateDefinitionForPart(Factory, client3, part, dummy.Z0_BoolInfo);
				AssertNoErrors("this org has inventory with packing dates but no current units so should not get an error", dummy.Z0_BoolInfo);
				validation.CheckPackingDateDefinitionForPart(Factory, client4, part, dummy.Z0_BoolInfo);
				AssertNoErrors("this org has inventory with no packing dates so should not get error", dummy.Z0_BoolInfo);
				validation.CheckPackingDateDefinitionForPart(Factory, client2, part, dummy.Z0_BoolInfo);
				AssertHasErrors("this org has inventory with packing dates so should get an error", dummy.Z0_BoolInfo);
			}
		}

		#endregion

		#region TestCheckSerialNumberDefinitionForOrganisation

		public void TestCheckSerialNumberDefinitionForOrganisation()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();

			var part1 = Factory.NewWithValidTestData<OrgSupplierPart>();
			part1.OP_PartNum = "P2";

			var part2 = Factory.NewWithValidTestData<OrgSupplierPart>();
			part2.OP_PartNum = "P3";

			var relation1 = part1.RelatedOrganisations.AddNew();
			relation1.OU_OH = org1.PK;
			relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			var relation2 = part2.RelatedOrganisations.AddNew();
			relation2.OU_OH = org1.PK;
			relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			Factory.Save();

			var dummy = Factory.New<DummyBusinessObject>();
			var validation = new PartAttributeValidation();

			using (dummy.SuspendValidationTesting())
			{
				// simple check to make sure a null org doesnt crash the routine
				validation.CheckSerialNumberDefinitionForOrganisation(Factory, null, dummy.Z0_BoolInfo);

				dummy.Z0_Bool = true; // turning the serial number definition on
				Factory.Save();
				validation.CheckSerialNumberDefinitionForOrganisation(Factory, org1, dummy.Z0_BoolInfo);
				AssertNoErrors("Can always turn on.", dummy.Z0_BoolInfo);

				relation1.OU_UseSerialNumber = true;
				relation2.OU_UseSerialNumber = true;
				Factory.Save();

				dummy.Z0_Bool = false; // turning the serial number definition off
				validation.CheckSerialNumberDefinitionForOrganisation(Factory, org1, dummy.Z0_BoolInfo);
				AssertHasError("Cannot turn off if a product is using serial number.", dummy.Z0_BoolInfo, @"This attribute is being used by at least one product with a relationship to this organization.
These product relationships must have this attribute disabled before you can disable it for the organization.
See Product Entry.");

				relation1.OU_UseSerialNumber = false;
				dummy.Z0_Bool = false;
				Factory.Save();
				validation.CheckSerialNumberDefinitionForOrganisation(Factory, org1, dummy.Z0_BoolInfo);
				AssertHasError("Still cannot turn off because a product is using serial number.", dummy.Z0_BoolInfo, @"This attribute is being used by at least one product with a relationship to this organization.
These product relationships must have this attribute disabled before you can disable it for the organization.
See Product Entry.");

				relation2.OU_UseSerialNumber = false;
				dummy.Z0_Bool = false;
				Factory.Save();
				validation.CheckSerialNumberDefinitionForOrganisation(Factory, org1, dummy.Z0_BoolInfo);
				AssertNoErrors("Can turn off now as no relations use serial number.", dummy.Z0_BoolInfo);
			}
		}

		#endregion

		#region TestCheckSerialNumberDefinitionForPart

		public void TestCheckSerialNumberDefinitionForPart()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var whs = helper.CreateWarehouse("WH1", "A", 2, 1);
			var client1 = Factory.Load<OrgHeader>(helper.CreateClient("CLIENT1"));
			var client2 = Factory.Load<OrgHeader>(helper.CreateClient("CLIENT2"));
			var client3 = Factory.Load<OrgHeader>(helper.CreateClient("CLIENT3"));
			var client4 = Factory.Load<OrgHeader>(helper.CreateClient("CLIENT4"));
			var part = (OrgSupplierPart)helper.CreateProduct(client1.PK, "P1");

			var relation1 = part.RelatedOrganisations.FindFirstByOrganisationPK(client1.PK);

			var relation2 = part.RelatedOrganisations.AddOwner(client2);
			client2.MiscServ.OM_IMUseSerialNumber = true;
			relation2.OU_UseSerialNumber = true;

			var relation3 = part.RelatedOrganisations.AddOwner(client3);
			relation3.OU_UseSerialNumber = true;

			var relation4 = part.RelatedOrganisations.AddOwner(client4);
			client4.MiscServ.OM_IMUseSerialNumber = true;
			relation4.OU_UseSerialNumber = true;

			var receive1PK = helper.CreateWhsReceive(client2.PK, whs.PK, "R1", new NotificationBuffer());
			var line1 = (IWhsInventoryView)helper.CreateWhsReceiveInventoryLine(receive1PK, part.PK, 1m, ZDate.Empty, ZDate.Empty, "", "", "", "KEY-1");
			line1.WI_SerialNumber = "SER1";
			helper.WhsReceiveAllocateLocationsMock(receive1PK);
			helper.FinaliseDocketWithoutUserConfirmation(receive1PK);

			var receive2PK = helper.CreateWhsReceive(client3.PK, whs.PK, "R2", new NotificationBuffer());
			var line2 = (IWhsInventoryView)helper.CreateWhsReceiveInventoryLine(receive2PK, part.PK, 0m, ZDate.Empty, ZDate.Empty, "", "", "", "KEY-1");
			line2.WI_SerialNumber = "SER2";
			helper.FinaliseDocketWithoutUserConfirmation(receive2PK);

			var receive3PK = helper.CreateWhsReceive(client4.PK, whs.PK, "R3", new NotificationBuffer());
			var line3 = (IWhsInventoryView)helper.CreateWhsReceiveInventoryLine(receive3PK, part.PK, 1m, ZDate.Empty, ZDate.Empty, "", "", "", "KEY-1");
			line3.WI_SerialNumber = "SER3";
			helper.WhsReceiveAllocateLocationsMock(receive3PK);
			helper.FinaliseDocketWithoutUserConfirmation(receive3PK);

			Factory.Save();
			line3.WI_SerialNumber = ""; // hack to set serial to empty
			Factory.Save();

			var dummy = Factory.New<DummyBusinessObject>();
			var validation = new PartAttributeValidation();

			using (dummy.SuspendValidationTesting())
			{
				validation.CheckSerialNumberDefinitionForPart(Factory, null, dummy.Z0_BoolInfo, WhsDocketLineSchema.WE_SerialNumber);
				AssertNoErrors("No errors when a relation is not provided.", dummy.Z0_BoolInfo);

				validation.CheckSerialNumberDefinitionForPart(Factory, relation1, dummy.Z0_BoolInfo, null);
				AssertNoErrors("No errors when a column is not provided.", dummy.Z0_BoolInfo);

				dummy.Z0_Bool = true;
				validation.CheckSerialNumberDefinitionForPart(Factory, relation1, dummy.Z0_BoolInfo, WhsDocketLineSchema.WE_SerialNumber);
				AssertNoErrors("this org has no inventory so should not get an error", dummy.Z0_BoolInfo);

				validation.CheckSerialNumberDefinitionForPart(Factory, relation2, dummy.Z0_BoolInfo, WhsDocketLineSchema.WE_SerialNumber);
				AssertNoErrors("this org has inventory with serial number so should not get an error", dummy.Z0_BoolInfo);

				validation.CheckSerialNumberDefinitionForPart(Factory, relation3, dummy.Z0_BoolInfo, WhsDocketLineSchema.WE_SerialNumber);
				AssertNoErrors("this org has inventory with serial number but no current units so should not get error", dummy.Z0_BoolInfo);

				dummy.Z0_Bool = false;
				validation.CheckSerialNumberDefinitionForPart(Factory, relation1, dummy.Z0_BoolInfo, WhsDocketLineSchema.WE_SerialNumber);
				AssertNoErrors("this org has no inventory so should not get an error", dummy.Z0_BoolInfo);

				validation.CheckSerialNumberDefinitionForPart(Factory, relation2, dummy.Z0_BoolInfo, WhsDocketLineSchema.WE_SerialNumber);
				AssertHasError("this org has available inventory with serial number so should get an error", dummy.Z0_BoolInfo,
					"There is current inventory using this attribute. This inventory must be removed from the warehouse before this attribute can be disabled.");

				dummy.Z0_BoolInfo.ClearAllNotifications();
				validation.CheckSerialNumberDefinitionForPart(Factory, relation3, dummy.Z0_BoolInfo, WhsDocketLineSchema.WE_SerialNumber);
				AssertNoErrors("this org has inventory with serial number but no current units so should not get an error", dummy.Z0_BoolInfo);

				relation4.OU_IsSerialNumberReleaseCaptured = true;
				validation.CheckSerialNumberDefinitionForPart(Factory, relation4, dummy.Z0_BoolInfo, WhsDocketLineSchema.WE_SerialNumber);
				AssertHasError("this org has inventory with no attributes but is release captured so should get an error", dummy.Z0_BoolInfo,
					"There is current inventory using this attribute. This inventory must be removed from the warehouse before this attribute can be disabled.");

				relation4.OU_IsSerialNumberReleaseCaptured = false;
				dummy.Z0_BoolInfo.ClearAllNotifications();
				validation.CheckSerialNumberDefinitionForPart(Factory, relation2, dummy.Z0_BoolInfo, WhsDocketLineSchema.WE_SerialNumber);
				AssertHasError("this org has inventory with attributes so should get an error", dummy.Z0_BoolInfo,
					"There is current inventory using this attribute. This inventory must be removed from the warehouse before this attribute can be disabled.");
			}
		}

		public void TestCheckSerialNumberDefinitionForPart_StockNotCurrent()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var whs = helper.CreateWarehouse("WH1", "A", 2, 1);
			var client1 = Factory.Load<OrgHeader>(helper.CreateClient("CLIENT1"));
			var client2 = Factory.Load<OrgHeader>(helper.CreateClient("CLIENT2"));
			var part = (OrgSupplierPart)helper.CreateProduct(client1.PK, "P1");

			var relation1 = part.RelatedOrganisations.FindFirstByOrganisationPK(client1.PK);
			client1.MiscServ.OM_IMUseSerialNumber = true;
			relation1.OU_UseSerialNumber = true;

			var relation2 = part.RelatedOrganisations.AddOwner(client2);
			client2.MiscServ.OM_IMUseSerialNumber = true;
			relation2.OU_UseSerialNumber = true;

			var adjustedOutReceivePK = helper.CreateWhsReceive(client1.PK, whs.PK, "R1", new NotificationBuffer());
			var adjustedOutReceive = Factory.Load<IWhsReceive>(adjustedOutReceivePK);
			var line1 = (IWhsInventoryView)helper.CreateWhsReceiveInventoryLine(adjustedOutReceivePK, part.PK, 1m, ZDate.Empty, ZDate.Empty, "", "", "", "");
			line1.WI_SerialNumber = "SER1";
			helper.WhsReceiveAllocateLocationsMock(adjustedOutReceivePK);
			helper.FinaliseDocketWithoutUserConfirmation(adjustedOutReceivePK);

			Factory.Save();

			var adjustment = (IWhsDocket)helper.CreateWhsAdjustment(client1.PK, whs.PK, "AdjOut", new NotificationBuffer());
			var adjustmentLinePK = helper.CreateWhsAdjustmentLine(adjustment.PK, part.PK, -1m, line1.WI_WL);
			var adjustmentLine = Factory.Load<IWhsDocketLine>(adjustmentLinePK);
			adjustmentLine.WE_SerialNumber = "SER1";
			helper.FinaliseDocketWithoutUserConfirmation(adjustment.PK);

			Factory.Save();

			var pickedReceive = helper.CreateWhsReceive(client2.PK, whs.PK, "R2", new NotificationBuffer());
			var line2 = (IWhsInventoryView)helper.CreateWhsReceiveInventoryLine(pickedReceive, part.PK, 1m, ZDate.Empty, ZDate.Empty, "", "", "", "");
			line2.WI_SerialNumber = "SER2";
			helper.WhsReceiveAllocateLocationsMock(pickedReceive);
			helper.FinaliseDocketWithoutUserConfirmation(pickedReceive);

			Factory.Save();

			var orderPK = helper.CreateWhsOrder(client2.PK, whs.PK, "O1", null);
			var orderLinePK = helper.CreateWhsOrderLine(orderPK, part.PK, 1m);
			var pickPK = helper.CreateWhsPick(new[] { orderPK });
			var pickLine = helper.GetPickLines(pickPK).Single();
			helper.PickAndMakeInTransitTransfer(pickLine, ZDateTime.Now);

			Factory.Save();

			var dummy = Factory.New<DummyBusinessObject>();
			var validation = new PartAttributeValidation();

			using (dummy.SuspendValidationTesting())
			{
				dummy.Z0_Bool = true;
				validation.CheckSerialNumberDefinitionForPart(Factory, relation1, dummy.Z0_BoolInfo, WhsDocketLineSchema.WE_SerialNumber);
				AssertNoErrors("This org has no current inventory", dummy.Z0_BoolInfo);

				validation.CheckSerialNumberDefinitionForPart(Factory, relation2, dummy.Z0_BoolInfo, WhsDocketLineSchema.WE_SerialNumber);
				AssertNoErrors("This org has no current inventory", dummy.Z0_BoolInfo);

				dummy.Z0_Bool = false;
				validation.CheckSerialNumberDefinitionForPart(Factory, relation1, dummy.Z0_BoolInfo, WhsDocketLineSchema.WE_SerialNumber);
				AssertNoErrors("This org has no current inventory", dummy.Z0_BoolInfo);

				validation.CheckSerialNumberDefinitionForPart(Factory, relation2, dummy.Z0_BoolInfo, WhsDocketLineSchema.WE_SerialNumber);
				AssertHasError(dummy.Z0_BoolInfo, "There is current inventory using this attribute. This inventory must be removed from the warehouse before this attribute can be disabled.");

				helper.FinaliseDocketWithoutUserConfirmation(orderPK);
				helper.FinalisePick(pickPK);
				Factory.Save();

				dummy.Z0_BoolInfo.ClearAllNotifications();
				validation.CheckSerialNumberDefinitionForPart(Factory, relation2, dummy.Z0_BoolInfo, WhsDocketLineSchema.WE_SerialNumber);
				AssertNoErrors("This org has no current inventory", dummy.Z0_BoolInfo);
			}
		}

		public void TestCheckSerialNumberDefinitionForPartNoUnfinalisedReceives()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var whs = helper.CreateWarehouse("WH1", "A", 2, 1);
			var client1 = Factory.Load<OrgHeader>(helper.CreateClient("CLIENT1"));

			client1.MiscServ.OM_IMUseSerialNumber = true;

			var part = (OrgSupplierPart)helper.CreateProduct(client1.PK, "P1");
			var relation = part.RelatedOrganisations[0];

			relation.OU_UseSerialNumber = true;

			AssertNoErrors(relation.OU_UseSerialNumberInfo);
			AssertEquals("OWN", relation.OU_Relationship);

			Factory.Save();

			var receive1PK = helper.CreateWhsReceive(client1.PK, whs.PK, "R1", new NotificationBuffer());
			var inventory1 = (IWhsInventoryView)helper.CreateWhsReceiveInventoryLine(receive1PK, part.PK, 1m, ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventory1.WI_SerialNumber = "SER";

			IWhsReceive receive1 = (IWhsReceive)Factory.Load(ObjectFactory.GetType<IWhsReceive>(), receive1PK);
			AssertNotEquals("FIN", receive1.WD_DocketStatus);
			Factory.Save();

			AssertNoErrors(relation.OU_UseSerialNumberInfo);

			relation.OU_UseSerialNumber = false;
			AssertNoErrors("Should not return error as the receive is not finalised yet.", relation.OU_UseSerialNumberInfo);

			relation.OU_UseSerialNumber = true;
			AssertNoErrors("Should not return error", relation.OU_UseSerialNumberInfo);

			helper.WhsReceiveAllocateLocationsMock(receive1PK);
			helper.FinaliseDocketWithoutUserConfirmation(receive1PK);
			Factory.Save();

			receive1 = (IWhsReceive)Factory.Load(ObjectFactory.GetType<IWhsReceive>(), receive1PK);
			AssertEquals("FIN", receive1.WD_DocketStatus);

			relation.OU_UseSerialNumber = false;
			AssertHasErrors("Should return error as the receive is now finalised.", relation.OU_UseSerialNumberInfo);

			relation.OU_UseSerialNumber = true;
			AssertNoErrors("Should not return error even though the receive is finalised.", relation.OU_UseSerialNumberInfo);
		}

		#endregion

		#region TestCheckAttributeDefinitionForOrganisation

		public void TestCheckAttributeDefinitionForOrganisation()
		{
			InsertOrgHeaderIntoDB(Org, 1);

			OrgSupplierPart part1 = Factory.NewWithValidTestData<OrgSupplierPart>();
			OrgPartRelation relation1 = part1.RelatedOrganisations.AddNew();
			part1.OP_PartNum = "P2";
			relation1.OU_OH = Org.PK;
			relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			Factory.Save();

			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			PartAttributeValidation validation = new PartAttributeValidation();

			using (dummy.SuspendValidationTesting())
			{
				// simple check to make sure a null org doesnt crash the routine
				validation.CheckAttributeNameDefinitionForOrganisation(Factory, null, dummy.Z0_DescriptionInfo, dummy.Z0_CodeInfo, OrgPartRelationSchema.OU_UsePartAttrib1);

				dummy.Z0_Description = "Attrib1Name";
				dummy.Z0_Code = new ZString(new PartAttributeTypeList()[0].Code);
				validation.CheckAttributeNameDefinitionForOrganisation(Factory, Org, dummy.Z0_DescriptionInfo, dummy.Z0_CodeInfo, OrgPartRelationSchema.OU_UsePartAttrib1);
				validation.CheckAttributeTypeDefinitionForOrganisation(Factory, Org, dummy.Z0_CodeInfo, dummy.Z0_DescriptionInfo, OrgPartRelationSchema.OU_UsePartAttrib1);
				AssertNoErrors("can always turn on", dummy.Z0_DescriptionInfo);
				AssertNoErrors("can always turn on", dummy.Z0_CodeInfo);

				Relation.OU_UsePartAttrib1 = true;
				relation1.OU_UsePartAttrib1 = true;
				Factory.Save();
				dummy.Z0_Description = ZString.Empty;
				dummy.Z0_Code = ZString.Empty;
				validation.CheckAttributeNameDefinitionForOrganisation(Factory, Org, dummy.Z0_DescriptionInfo, dummy.Z0_CodeInfo, OrgPartRelationSchema.OU_UsePartAttrib1);
				validation.CheckAttributeTypeDefinitionForOrganisation(Factory, Org, dummy.Z0_CodeInfo, dummy.Z0_DescriptionInfo, OrgPartRelationSchema.OU_UsePartAttrib1);
				AssertHasErrors("cannot disable as some relationships have attribute enabled", dummy.Z0_DescriptionInfo);
				AssertHasErrors("cannot disable as some relationships have attribute enabled", dummy.Z0_CodeInfo);

				Relation.OU_UsePartAttrib1 = false;
				relation1.OU_UsePartAttrib1 = false;
				Factory.Save();
				dummy.Z0_DescriptionInfo.ClearAllNotifications();
				dummy.Z0_CodeInfo.ClearAllNotifications();
				validation.CheckAttributeNameDefinitionForOrganisation(Factory, Org, dummy.Z0_DescriptionInfo, dummy.Z0_CodeInfo, OrgPartRelationSchema.OU_UsePartAttrib1);
				validation.CheckAttributeTypeDefinitionForOrganisation(Factory, Org, dummy.Z0_CodeInfo, dummy.Z0_DescriptionInfo, OrgPartRelationSchema.OU_UsePartAttrib1);
				AssertNoErrors("can disable as no relationships have attribute enabled", dummy.Z0_DescriptionInfo);
				AssertNoErrors("can disable as no relationships have attribute enabled", dummy.Z0_CodeInfo);
			}
		}

		#endregion

		#region TestCheckAttributeDefinitionForOrganisationCore

		public void TestCheckAttributeDefinitionForOrganisationCore()
		{
			InsertOrgHeaderIntoDB(Org, 1);
			OrgHeader org2 = InsertOrgHeaderIntoDB(null, 2);
			OrgHeader org3 = InsertOrgHeaderIntoDB(null, 3);
			OrgHeader org4 = InsertOrgHeaderIntoDB(null, 4);

			Relation.OU_OH = Org.PK;
			Relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			Relation.OU_UsePartAttrib1 = true;

			org2.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.Mandatory;
			Part.RelatedOrganisations.AddOwner(org2).OU_UsePartAttrib1 = true;
			Part.RelatedOrganisations.AddOwner(org3).OU_UsePartAttrib1 = true;
			Part.RelatedOrganisations.AddOwner(org4).OU_UsePartAttrib1 = true;

			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var whs = helper.CreateWarehouse("WH1", "A", 3, 1);
			Factory.Save();

			var receive1PK = helper.CreateWhsReceive(org2.PK, whs.PK, "R1", new NotificationBuffer());
			helper.CreateWhsReceiveInventoryLine(receive1PK, Part.PK, 1m, ZDate.Empty, ZDate.Empty, "PA1", "", "", "");
			helper.WhsReceiveAllocateLocationsMock(receive1PK);
			helper.FinaliseDocketWithoutUserConfirmation(receive1PK);

			var receive2PK = helper.CreateWhsReceive(org3.PK, whs.PK, "R2", new NotificationBuffer());
			helper.CreateWhsReceiveInventoryLine(receive2PK, Part.PK, 0m, "A-2");
			helper.FinaliseDocketWithoutUserConfirmation(receive2PK);

			var receive3PK = helper.CreateWhsReceive(org4.PK, whs.PK, "R3", new NotificationBuffer());
			helper.CreateWhsReceiveInventoryLine(receive3PK, Part.PK, 1m, "A-3");
			helper.FinaliseDocketWithoutUserConfirmation(receive3PK);

			Factory.Save();

			var dummy = Factory.New<DummyBusinessObject>();
			var validation = new PartAttributeValidation();
			using (dummy.SuspendValidationTesting())
			{
				dummy.Z0_Code = new ZString(PartAttributeTypeList.Codes.NonMandatory);
				dummy.Z0_Description = "Attrib1Name";

				Factory.Save();

				dummy.Z0_Code = new ZString(PartAttributeTypeList.Codes.Mandatory);
				validation.CheckAttributeTypeDefinitionForOrganisation(Factory, Org, dummy.Z0_CodeInfo, dummy.Z0_DescriptionInfo, OrgPartRelationSchema.OU_UsePartAttrib1);
				AssertNoErrors("this org has no inventory so should not get error", dummy.Z0_CodeInfo);
				validation.CheckAttributeTypeDefinitionForOrganisation(Factory, org2, dummy.Z0_CodeInfo, dummy.Z0_DescriptionInfo, OrgPartRelationSchema.OU_UsePartAttrib1);
				AssertNoErrors("this org has inventory with attributes should not get error", dummy.Z0_CodeInfo);
				validation.CheckAttributeTypeDefinitionForOrganisation(Factory, org3, dummy.Z0_CodeInfo, dummy.Z0_DescriptionInfo, OrgPartRelationSchema.OU_UsePartAttrib1);
				AssertNoErrors("this org has inventory with no attributes, but not finalized so should not get error", dummy.Z0_CodeInfo);
				validation.CheckAttributeTypeDefinitionForOrganisation(Factory, org4, dummy.Z0_CodeInfo, dummy.Z0_DescriptionInfo, OrgPartRelationSchema.OU_UsePartAttrib1);
				AssertHasErrors("this org has inventory with no attributes so should get error", dummy.Z0_CodeInfo);
			}
		}

		#endregion

		#region TestCheckAttributeDefinitionForPart

		public void TestCheckAttributeDefinitionForPart()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var whs = helper.CreateWarehouse("WH1", "A", 2, 1);
			var client1 = Factory.Load<OrgHeader>(helper.CreateClient("CLIENT1"));
			var client2 = Factory.Load<OrgHeader>(helper.CreateClient("CLIENT2"));
			var client3 = Factory.Load<OrgHeader>(helper.CreateClient("CLIENT3"));
			var client4 = Factory.Load<OrgHeader>(helper.CreateClient("CLIENT4"));
			var part = (OrgSupplierPart)helper.CreateProduct(client1.PK, "P1");

			var relation1 = part.RelatedOrganisations.FindFirstByOrganisationPK(client1.PK);

			var relation2 = part.RelatedOrganisations.AddOwner(client2);
			SetOrgMiscServAttribs(client2, PartAttributeTypeList.Codes.NonMandatory);
			SetOrgMiscServDates(client2, true);
			client2.MiscServ.OM_IMUseSerialNumber = true;
			SetPartRelationAttribs(relation2, true);
			relation2.OU_UseSerialNumber = true;
			SetPartRelationDates(relation2, true);

			var relation3 = part.RelatedOrganisations.AddOwner(client3);

			var relation4 = part.RelatedOrganisations.AddOwner(client4);
			client4.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.NonMandatory;
			SetPartRelationAttribs(relation3, true);
			SetPartRelationAttribs(relation4, true);

			var receive1PK = helper.CreateWhsReceive(client2.PK, whs.PK, "R1", new NotificationBuffer());
			var inventory1PK = helper.CreateWhsReceiveInventoryLine(receive1PK, part.PK, 1m, ZDate.Today.AddDays(10), ZDate.Today.AddDays(-10), "PA1", "PA2", "PA3", "KEY-1");
			inventory1PK[WhsInventoryViewSchema.Constants.WI_SerialNumber] = "SN1";
			helper.WhsReceiveAllocateLocationsMock(receive1PK);
			helper.FinaliseDocketWithoutUserConfirmation(receive1PK);

			var receive2PK = helper.CreateWhsReceive(client3.PK, whs.PK, "R2", new NotificationBuffer());
			var inventory2PK = helper.CreateWhsReceiveInventoryLine(receive2PK, part.PK, 0m, "A-1-1");
			helper.FinaliseDocketWithoutUserConfirmation(receive2PK);

			var receive3PK = helper.CreateWhsReceive(client4.PK, whs.PK, "R3", new NotificationBuffer());
			var inventory3PK = helper.CreateWhsReceiveInventoryLine(receive3PK, part.PK, 10m, ZDate.Empty, ZDate.Empty, "", "", "", "KEY-1");
			helper.WhsReceiveAllocateLocationsMock(receive3PK);
			helper.FinaliseDocketWithoutUserConfirmation(receive3PK);

			Factory.Save();

			var dummy = Factory.New<DummyBusinessObject>();
			var validation = new PartAttributeValidation();

			using (dummy.SuspendValidationTesting())
			{
				validation.CheckAttributeDefinitionForPart(Factory, null, 0, dummy.Z0_BoolInfo, null);

				dummy.Z0_Bool = true;
				validation.CheckAttributeDefinitionForPart(Factory, relation1, 1, dummy.Z0_BoolInfo, WhsDocketLineSchema.WE_PartAttrib1);
				AssertNoErrors("this org has no inventory so should not get an error", dummy.Z0_BoolInfo);

				validation.CheckAttributeDefinitionForPart(Factory, relation2, 1, dummy.Z0_BoolInfo, WhsDocketLineSchema.WE_PartAttrib1);
				AssertNoErrors("this org has inventory with attributes so should not get an error", dummy.Z0_BoolInfo);

				validation.CheckAttributeDefinitionForPart(Factory, relation3, 1, dummy.Z0_BoolInfo, WhsDocketLineSchema.WE_PartAttrib1);
				AssertNoErrors("this org has inventory with attributes but no current units so should not get error", dummy.Z0_BoolInfo);

				validation.CheckAttributeDefinitionForPart(Factory, relation4, 2, dummy.Z0_BoolInfo, WhsDocketLineSchema.WE_PartAttrib2);
				AssertNoErrors("this org has inventory with no attributes but the attribute type is non mandatory so should not get error", dummy.Z0_BoolInfo);

				relation4.OU_IsPartAttrib1ReleaseCaptured = true;
				validation.CheckAttributeDefinitionForPart(Factory, relation4, 1, dummy.Z0_BoolInfo, WhsDocketLineSchema.WE_PartAttrib1);
				AssertNoErrors("this org has inventory with no attributes but the attribute is release captured (i.e no attributes on receipt)", dummy.Z0_BoolInfo);

				relation4.OU_IsPartAttrib1ReleaseCaptured = false;
				validation.CheckAttributeDefinitionForPart(Factory, relation4, 1, dummy.Z0_BoolInfo, WhsDocketLineSchema.WE_PartAttrib1);
				AssertHasError("this org has inventory with no attributes so should get error", dummy.Z0_BoolInfo,
					"There is current inventory which is NOT using this attribute. This inventory must be removed from the warehouse before this attribute can be enabled.");

				dummy.Z0_Bool = false;
				validation.CheckAttributeDefinitionForPart(Factory, relation1, 1, dummy.Z0_BoolInfo, WhsDocketLineSchema.WE_PartAttrib1);
				AssertNoErrors("this org has no inventory so should not get an error", dummy.Z0_BoolInfo);

				validation.CheckAttributeDefinitionForPart(Factory, relation3, 1, dummy.Z0_BoolInfo, WhsDocketLineSchema.WE_PartAttrib1);
				AssertNoErrors("this org has inventory with attributes but no current units so should not get an error", dummy.Z0_BoolInfo);

				validation.CheckAttributeDefinitionForPart(Factory, relation4, 1, dummy.Z0_BoolInfo, WhsDocketLineSchema.WE_PartAttrib1);
				AssertNoErrors("this org has inventory with no attributes so should not get error", dummy.Z0_BoolInfo);

				relation4.OU_IsPartAttrib1ReleaseCaptured = true;
				validation.CheckAttributeDefinitionForPart(Factory, relation4, 1, dummy.Z0_BoolInfo, WhsDocketLineSchema.WE_PartAttrib1);
				AssertHasError("this org has inventory with no attributes but is release captured so should get an error", dummy.Z0_BoolInfo,
					"There is current inventory using this attribute. This inventory must be removed from the warehouse before this attribute can be disabled.");

				relation4.OU_IsPartAttrib1ReleaseCaptured = false;
				dummy.Z0_BoolInfo.ClearAllNotifications();
				validation.CheckAttributeDefinitionForPart(Factory, relation2, 1, dummy.Z0_BoolInfo, WhsDocketLineSchema.WE_PartAttrib1);
				AssertHasError("this org has inventory with attributes so should get an error", dummy.Z0_BoolInfo,
					"There is current inventory using this attribute. This inventory must be removed from the warehouse before this attribute can be disabled.");

				dummy.Z0_BoolInfo.ClearAllNotifications();
				validation.CheckAttributeDefinitionForPart(Factory, relation2, 2, dummy.Z0_BoolInfo, WhsDocketLineSchema.WE_PartAttrib2);
				AssertHasError("this org has inventory with attributes - an attribute type of non mandatory should still get an error", dummy.Z0_BoolInfo,
					"There is current inventory using this attribute. This inventory must be removed from the warehouse before this attribute can be disabled.");

				dummy.Z0_BoolInfo.ClearAllNotifications();
				validation.CheckAttributeDefinitionForPart(Factory, null, 2, dummy.Z0_BoolInfo, WhsDocketLineSchema.WE_PartAttrib2);
				AssertNoErrors("No Relation was specified, shouldn't check", dummy.Z0_BoolInfo);
			}
		}

		public void TestCheckAttributeDefinitionForPartNoUnfinalisedReceives()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var whs = helper.CreateWarehouse("WH1", "A", 2, 1);
			var client1 = Factory.Load<OrgHeader>(helper.CreateClient("CLIENT1"));

			SetOrgMiscServAttribs(client1, PartAttributeTypeList.Codes.Mandatory);
			SetOrgMiscServDates(client1, true);

			var part = (OrgSupplierPart)helper.CreateProduct(client1.PK, "P1");
			var relation = part.RelatedOrganisations[0];

			SetPartRelationAttribs(relation, true);
			SetPartRelationDates(relation, true);

			AssertNoErrors(relation.OU_UsePartAttrib1Info);
			AssertNoErrors(relation.OU_UsePartAttrib2Info);
			AssertNoErrors(relation.OU_UsePartAttrib3Info);
			AssertNoErrors(relation.OU_UsePackingDateInfo);
			AssertNoErrors(relation.OU_UseExpiryDateInfo);
			AssertEquals("OWN", relation.OU_Relationship);

			Factory.Save();

			var receive1PK = helper.CreateWhsReceive(client1.PK, whs.PK, "R1", new NotificationBuffer());
			var inventory1 = helper.CreateWhsReceiveInventoryLine(receive1PK, part.PK, 10m, ZDate.Today.AddDays(1), ZDate.Today.AddDays(-1), "PA1", "PA2", "PA3", "");

			IWhsReceive receive1 = (IWhsReceive)Factory.Load(ObjectFactory.GetType<IWhsReceive>(), receive1PK);
			AssertNotEquals("FIN", receive1.WD_DocketStatus);
			Factory.Save();

			AssertNoErrors(relation.OU_UsePartAttrib1Info);
			AssertNoErrors(relation.OU_UsePartAttrib2Info);
			AssertNoErrors(relation.OU_UsePartAttrib3Info);
			AssertNoErrors(relation.OU_UsePackingDateInfo);
			AssertNoErrors(relation.OU_UseExpiryDateInfo);

			SetPartRelationAttribs(relation, false);
			SetPartRelationDates(relation, false);

			AssertNoErrors("Should not return error as the receive is not finalised yet.", relation.OU_UsePartAttrib1Info);
			AssertNoErrors("Should not return error as the receive is not finalised yet.", relation.OU_UsePartAttrib2Info);
			AssertNoErrors("Should not return error as the receive is not finalised yet.", relation.OU_UsePartAttrib3Info);
			AssertNoErrors("Should not return error as the receive is not finalised yet.", relation.OU_UseExpiryDateInfo);
			AssertNoErrors("Should not return error as the receive is not finalised yet.", relation.OU_UsePackingDateInfo);

			SetPartRelationAttribs(relation, true);
			SetPartRelationDates(relation, true);

			AssertNoErrors("Should not return error", relation.OU_UsePartAttrib1Info);
			AssertNoErrors("Should not return error", relation.OU_UsePartAttrib2Info);
			AssertNoErrors("Should not return error", relation.OU_UsePartAttrib3Info);
			AssertNoErrors("Should not return error", relation.OU_UseExpiryDateInfo);
			AssertNoErrors("Should not return error", relation.OU_UsePackingDateInfo);

			helper.WhsReceiveAllocateLocationsMock(receive1PK);
			helper.FinaliseDocketWithoutUserConfirmation(receive1PK);
			Factory.Save();

			receive1 = (IWhsReceive)Factory.Load(ObjectFactory.GetType<IWhsReceive>(), receive1PK);
			AssertEquals("FIN", receive1.WD_DocketStatus);

			SetPartRelationAttribs(relation, false);
			SetPartRelationDates(relation, false);

			AssertHasErrors("Should return error as the receive is now finalised.", relation.OU_UsePartAttrib1Info);
			AssertHasErrors("Should return error as the receive is now finalised.", relation.OU_UsePartAttrib2Info);
			AssertHasErrors("Should return error as the receive is now finalised.", relation.OU_UsePartAttrib3Info);
			AssertHasErrors("Should return error as the receive is now finalised.", relation.OU_UseExpiryDateInfo);
			AssertHasErrors("Should return error as the receive is now finalised.", relation.OU_UsePackingDateInfo);

			SetPartRelationAttribs(relation, true);
			SetPartRelationDates(relation, true);

			AssertNoErrors("Should not return error even though the receive is finalised.", relation.OU_UsePartAttrib1Info);
			AssertNoErrors("Should not return error even though the receive is finalised.", relation.OU_UsePartAttrib2Info);
			AssertNoErrors("Should not return error even though the receive is finalised.", relation.OU_UsePartAttrib3Info);
			AssertNoErrors("Should not return error even though the receive is finalised.", relation.OU_UseExpiryDateInfo);
			AssertNoErrors("Should not return error even though the receive is finalised.", relation.OU_UsePackingDateInfo);
		}

		public void TestCheckUsePartAttribDefinitionForPart()
		{
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			var relation = part.RelatedOrganisations.AddNew();
			part.OP_PartNum = "PARTNUM";
			relation.OU_OH = Org.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			SetOrgMiscServAttribs(Org, PartAttributeTypeList.Codes.VIN);

			AssertNoError(relation.OU_UsePartAttrib1Info, "You can only use one VIN attribute for the organization on the product.");
			AssertNoError(relation.OU_UsePartAttrib2Info, "You can only use one VIN attribute for the organization on the product.");
			AssertNoError(relation.OU_UsePartAttrib3Info, "You can only use one VIN attribute for the organization on the product.");

			relation.OU_UsePartAttrib1 = true;
			AssertNoError(relation.OU_UsePartAttrib1Info, "You can only use one VIN attribute for the organization on the product.");
			AssertNoError(relation.OU_UsePartAttrib2Info, "You can only use one VIN attribute for the organization on the product.");
			AssertNoError(relation.OU_UsePartAttrib3Info, "You can only use one VIN attribute for the organization on the product.");

			relation.OU_UsePartAttrib2 = true;
			AssertHasError(relation.OU_UsePartAttrib1Info, "You can only use one VIN attribute for the organization on the product.");
			AssertHasError(relation.OU_UsePartAttrib2Info, "You can only use one VIN attribute for the organization on the product.");
			AssertNoError(relation.OU_UsePartAttrib3Info, "You can only use one VIN attribute for the organization on the product.");

			relation.OU_UsePartAttrib3 = true;
			AssertHasError(relation.OU_UsePartAttrib1Info, "You can only use one VIN attribute for the organization on the product.");
			AssertHasError(relation.OU_UsePartAttrib2Info, "You can only use one VIN attribute for the organization on the product.");
			AssertHasError(relation.OU_UsePartAttrib3Info, "You can only use one VIN attribute for the organization on the product.");

			relation.OU_UsePartAttrib1 = false;
			AssertNoError(relation.OU_UsePartAttrib1Info, "You can only use one VIN attribute for the organization on the product.");
			AssertHasError(relation.OU_UsePartAttrib2Info, "You can only use one VIN attribute for the organization on the product.");
			AssertHasError(relation.OU_UsePartAttrib3Info, "You can only use one VIN attribute for the organization on the product.");

			relation.OU_UsePartAttrib2 = false;
			AssertNoError(relation.OU_UsePartAttrib1Info, "You can only use one VIN attribute for the organization on the product.");
			AssertNoError(relation.OU_UsePartAttrib2Info, "You can only use one VIN attribute for the organization on the product.");
			AssertNoError(relation.OU_UsePartAttrib3Info, "You can only use one VIN attribute for the organization on the product.");
		}

		public void TestCheckAttributeDefinitionForPartHasErrorIfChanged_SerialNumber()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var whs = helper.CreateWarehouse("WH1", "A", 2, 1);
			var client1 = Factory.Load<OrgHeader>(helper.CreateClient("CLIENT1"));

			client1.MiscServ.OM_IMUseSerialNumber = true;
			var part = (OrgSupplierPart)helper.CreateProduct(client1.PK, "P1");
			var relation = part.RelatedOrganisations[0];
			relation.OU_UseSerialNumber = true;

			var part2 = (OrgSupplierPart)helper.CreateProduct(client1.PK, "P2");
			var relation2 = part2.RelatedOrganisations[0];
			Factory.Save();

			var receive1PK = helper.CreateWhsReceive(client1.PK, whs.PK, "R1", new NotificationBuffer());
			var inventory = helper.CreateWhsReceiveInventoryLine(receive1PK, part.PK, 1m, ZDate.Empty, ZDate.Empty, "", "", "", "");
			var inventory2 = helper.CreateWhsReceiveInventoryLine(receive1PK, part2.PK, 1m, ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventory[WhsInventoryViewSchema.Constants.WI_SerialNumber] = "SN1";
			helper.WhsReceiveAllocateLocationsMock(receive1PK);
			helper.FinaliseDocketWithoutUserConfirmation(receive1PK);
			Factory.Save();
			// hack to set serial number to empty
			inventory[WhsInventoryViewSchema.Constants.WI_SerialNumber] = "";
			Factory.Save();

			AssertEquals("Should have error", true, PartAttributeValidation.CheckAttributeDefinitionForPartHasErrorIfChanged(Factory, client1, part, WhsDocketLineSchema.WE_SerialNumber, true));

			relation2.OU_IsSerialNumberReleaseCaptured = true;
			AssertEquals("Should have error", true, PartAttributeValidation.CheckAttributeDefinitionForPartHasErrorIfChanged(Factory, client1, part2, WhsDocketLineSchema.WE_SerialNumber, false));
		}

		#endregion

		#region TestPartAttributeDefinitionForOrganisationForMandatoryValues

		public void TestPartAttributeDefinitionForOrganisationForMandatoryValues()
		{
			PartAttributeValidation validation = new PartAttributeValidation();
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();

			using (dummy.SuspendValidationTesting())
			{
				dummy.Z0_Description = ZString.Empty;
				dummy.Z0_Code = ZString.Empty;
				validation.CheckAttributeNameDefinitionForOrganisation(Factory, Org, dummy.Z0_DescriptionInfo, dummy.Z0_CodeInfo, OrgPartRelationSchema.OU_UsePartAttrib1);
				validation.CheckAttributeTypeDefinitionForOrganisation(Factory, Org, dummy.Z0_CodeInfo, dummy.Z0_DescriptionInfo, OrgPartRelationSchema.OU_UsePartAttrib1);
				AssertNoErrors("blank name and type should not cause error", dummy.Z0_DescriptionInfo);
				AssertNoErrors("blank name and type should not cause error", dummy.Z0_CodeInfo);

				dummy.Z0_Code = new ZString(new PartAttributeTypeList()[0].Code);
				validation.CheckAttributeTypeDefinitionForOrganisation(Factory, Org, dummy.Z0_CodeInfo, dummy.Z0_DescriptionInfo, OrgPartRelationSchema.OU_UsePartAttrib1);
				AssertNoErrors("blank name and setting type should not cause error", dummy.Z0_CodeInfo);
				dummy.Z0_DescriptionInfo.ClearAllNotifications();

				dummy.Z0_Description = new ZString("Name");
				validation.CheckAttributeNameDefinitionForOrganisation(Factory, Org, dummy.Z0_DescriptionInfo, dummy.Z0_CodeInfo, OrgPartRelationSchema.OU_UsePartAttrib1);
				AssertNoErrors("name not blank type not blank should not cause error", dummy.Z0_DescriptionInfo);

				dummy.Z0_Code = ZString.Empty;
				validation.CheckAttributeTypeDefinitionForOrganisation(Factory, Org, dummy.Z0_CodeInfo, dummy.Z0_DescriptionInfo, OrgPartRelationSchema.OU_UsePartAttrib1);
				AssertHasErrors("setting type blank with something in name should cause error", dummy.Z0_CodeInfo);

				dummy.Z0_Code = "XXX";
				dummy.Z0_Description = ZString.Empty;
				validation.CheckAttributeTypeDefinitionForOrganisation(Factory, Org, dummy.Z0_CodeInfo, dummy.Z0_DescriptionInfo, OrgPartRelationSchema.OU_UsePartAttrib1);
				AssertNoErrors("setting name blank with something in type should cause error", dummy.Z0_DescriptionInfo);
			}
		}

		#endregion

		#region TestCheckAttributeNameForDuplication

		public void TestCheckAttributeNameForDuplication()
		{
			PartAttributeValidation validation = new PartAttributeValidation();
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();

			using (dummy.SuspendValidationTesting())
			{
				dummy.Z0_Description = ZString.Empty;
				dummy.Z0_Code = ZString.Empty;
				validation.CheckAttributeNameForDuplication(dummy.Z0_DescriptionInfo, dummy.Z0_CodeInfo);
				AssertNoErrors("Everything blank, so no problems", dummy.Z0_DescriptionInfo);

				dummy.Z0_Code = "Hello";
				dummy.Z0_Description = "Goodbye";
				validation.CheckAttributeNameForDuplication(dummy.Z0_DescriptionInfo, dummy.Z0_CodeInfo);
				AssertNoErrors("Different values, so no problems", dummy.Z0_DescriptionInfo);

				dummy.Z0_Description = "Hello";
				validation.CheckAttributeNameForDuplication(dummy.Z0_DescriptionInfo, dummy.Z0_CodeInfo);
				AssertHasErrors("Same values, should have error", dummy.Z0_DescriptionInfo);
			}
		}

		#endregion

		#region TestCheckAttributeTypeDefinitionForOrganisation_WhenOM_IMPartAttribTypeIsUsed

		public void TestCheckAttributeTypeDefinitionForOrganisation_WhenOM_IMPartAttribTypeIsUsed()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var whs = helper.CreateWarehouse("WH1", "A", 2, 1);
			var client = Factory.Load<OrgHeader>(helper.CreateClient("CLIENT1"));
			var part1 = (OrgSupplierPart)helper.CreateProduct(client.PK, "PR1");
			var part2 = (OrgSupplierPart)helper.CreateProduct(client.PK, "PR2");

			var receive = helper.CreateWhsReceive(client.PK, whs.PK, "R1", new NotificationBuffer());
			var inventory1 = helper.CreateWhsReceiveInventoryLine(receive, part1.PK, 10m, "");
			var inventory2 = helper.CreateWhsReceiveInventoryLine(receive, part2.PK, 10m, "");

			TestCheckAttributeTypeDefinitionForOrganisation_WhenOM_IMPartAttribTypeIsUsedCore(client.MiscServ.OM_IMPartAttrib1TypeInfo, part1.RelatedOrganisations[0].OU_UsePartAttrib1Info);
			TestCheckAttributeTypeDefinitionForOrganisation_WhenOM_IMPartAttribTypeIsUsedCore(client.MiscServ.OM_IMPartAttrib2TypeInfo, part1.RelatedOrganisations[0].OU_UsePartAttrib2Info);
			TestCheckAttributeTypeDefinitionForOrganisation_WhenOM_IMPartAttribTypeIsUsedCore(client.MiscServ.OM_IMPartAttrib3TypeInfo, part1.RelatedOrganisations[0].OU_UsePartAttrib3Info);
		}

		void TestCheckAttributeTypeDefinitionForOrganisation_WhenOM_IMPartAttribTypeIsUsedCore(ZPropertyInfo orgAttribInfo, ZPropertyInfo partRelationAttribInfo)
		{
			orgAttribInfo.Value = (ZString)PartAttributeTypeList.Codes.NonMandatory;
			Factory.Save();
			AssertEquals("Precondition - Part Attribute 1 Type should have no errors.", false, orgAttribInfo.HasErrors());

			orgAttribInfo.Value = (ZString)PartAttributeTypeList.Codes.Mandatory;
			AssertEquals("None of the products use Part Attribute 1, so there should be no problems in changing Part Attribute 1 Type.", false, orgAttribInfo.HasErrors());
			orgAttribInfo.Value = (ZString)PartAttributeTypeList.Codes.NonMandatory; // clean up

			partRelationAttribInfo.Value = ZBool.True; // the part now uses this attribute
			Factory.Save();
			orgAttribInfo.Value = (ZString)PartAttributeTypeList.Codes.Mandatory;
			AssertEquals("There is inventory with stock that has no attribute setup, system should not allow to change Type of Attribute until the stock is disposed off.", true, orgAttribInfo.HasErrors());
		}

		#endregion

		#region Implementation

		void SetOrgMiscServAttribs(OrgHeader client, string value)
		{
			client.MiscServ.OM_IMPartAttrib1Type = value;
			client.MiscServ.OM_IMPartAttrib2Type = value;
			client.MiscServ.OM_IMPartAttrib3Type = value;
		}

		void SetOrgMiscServDates(OrgHeader client, bool value)
		{
			client.MiscServ.OM_IMUseExpiryDate = value;
			client.MiscServ.OM_IMUsePackingDate = value;
		}

		void SetPartRelationAttribs(OrgPartRelation relation, bool value)
		{
			relation.OU_UsePartAttrib1 = value;
			relation.OU_UsePartAttrib2 = value;
			relation.OU_UsePartAttrib3 = value;
		}

		void SetPartRelationDates(OrgPartRelation relation, bool value)
		{
			relation.OU_UseExpiryDate = value;
			relation.OU_UsePackingDate = value;
		}

		void AssertMandatoryAttribute(DummyWithWarehouseAndOrderManagerProperty dummy, int n)
		{
			int attributeNumber = (n > 3) ? n - 3 : n;

			// attribute not set on org or product so no errors should not be generated
			using (dummy.SuspendValidationTesting())
			{
				dummy.Z0_DescriptionInfo.ClearAllNotifications();
				dummy.Z0_Description = "";
				Validation.CheckAttribute(Org, Part, dummy.Z0_DescriptionInfo, attributeNumber);
				AssertNoPartAttributeNotifications(dummy.Z0_DescriptionInfo);

				dummy.Z0_Description = "1";
				Validation.CheckAttribute(Org, Part, dummy.Z0_DescriptionInfo, attributeNumber);
				AssertShouldNotHaveAttributeNotification(dummy.Z0_DescriptionInfo, attributeNumber);

				dummy.WI_Calculated = "1";
				Validation.CheckAttribute(Org, Part, dummy.WI_CalculatedInfo, attributeNumber);
				AssertShouldNotHaveAttributeNotification(dummy.WI_CalculatedInfo, attributeNumber);

				dummy.JO_Calculated = "1";
				Validation.CheckAttribute(Org, Part, dummy.JO_CalculatedInfo, attributeNumber);
				AssertShouldNotHaveAttributeNotification(dummy.JO_CalculatedInfo, attributeNumber);

				Org.PartAttributeManager.SetProductToUseAttribute(Part, attributeNumber, true);
				// attribute set on part but not org

				dummy.Z0_Description = "";
				Validation.CheckAttribute(Org, Part, dummy.Z0_DescriptionInfo, attributeNumber);
				AssertNoPartAttributeNotifications(dummy.Z0_DescriptionInfo);

				dummy.Z0_Description = "1";
				Validation.CheckAttribute(Org, Part, dummy.Z0_DescriptionInfo, attributeNumber);
				AssertShouldNotHaveAttributeNotification(dummy.Z0_DescriptionInfo, attributeNumber);

				SetOrgNameAndType(n);
				// part attribute now set on both org and part so errors should be generated

				dummy.Z0_Description = "";
				Validation.CheckAttribute(Org, Part, dummy.Z0_DescriptionInfo, attributeNumber);
				AssertHasPartAttributeNotification(dummy.Z0_DescriptionInfo, "Please enter a " + Org.PartAttributeManager.PartAttributeName(attributeNumber) + ".");

				dummy.Z0_Description = "1";
				Validation.CheckAttribute(Org, Part, dummy.Z0_DescriptionInfo, attributeNumber);
				AssertNoPartAttributeNotifications(dummy.Z0_DescriptionInfo);

				Org.PartAttributeManager.SetProductToUseAttribute(Part, attributeNumber, false);
				ClearOrgNameAndType();
			}
		}

		class DummyWithWarehouseAndOrderManagerProperty : DummyBusinessObject
		{
			public DummyWithWarehouseAndOrderManagerProperty(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }
			public ZString WI_Calculated { get; set; }
			public ZPropertyInfo WI_CalculatedInfo { get { return GetZPropertyInfo(nameof(WI_Calculated)); } }
			public ZString JO_Calculated { get; set; }
			public ZPropertyInfo JO_CalculatedInfo { get { return GetZPropertyInfo(nameof(JO_Calculated)); } }
		}

		void AssertShouldNotHaveAttributeNotification(ZPropertyInfo info, int attributeNumber)
		{
			AssertShouldNotHaveAttributeNotification(info, $"Part Attrib. {attributeNumber}", $"Part Attribute {attributeNumber}");
		}

		void AssertShouldNotHaveAttributeNotification(ZPropertyInfo info, string attributeName, string attributeNameLong = null)
		{
			if (Org.CompanyData.OB_IMUsedBondedWhs)
			{
				AssertHasPartAttributeNotification(info, $"The part 'P1' is not set up to use {(attributeNameLong ?? attributeName)} with the Importer 'IMPORTER'. Please either remove the value '1' from the {(attributeNameLong ?? attributeName)} field, or set up the Product and Importer to use {(attributeNameLong ?? attributeName)}.");
			}
			else if (!string.IsNullOrEmpty(info.Value.ToString()) && CheckAttributeWhenNotEmpty(info))
			{
				AssertHasPartAttributeNotification(info, $"{attributeName} is not specified on the Product Master. Please do not enter a value.");
			}
			else
			{
				AssertNoPartAttributeNotifications(info);
			}
		}

		protected virtual bool CheckAttributeWhenNotEmpty(ZPropertyInfo info) => info.Name.StartsWith("W", false, Culture.Invariant); // This is dodgy, but just matching the existing production code.

		OrgHeader InsertOrgHeaderIntoDB(OrgHeader org, int i)
		{
			if (org == null)
			{
				org = Factory.NewWithValidTestData<OrgHeader>();
			}

			org.OH_Code = "TESTCO" + i.ToString();
			org.OH_FullName = "TEST COMPANY " + i.ToString();
			OrgAddress address = org.Addresses.AddNew();
			address.FillWithValidTestData();
			return org;
		}

		void SetOrgNameAndType(int n)
		{
			switch (n)
			{
				case 1:
					Org.MiscServ.OM_IMPartAttrib1Name = "Some Number";
					Org.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.Mandatory;
					break;
				case 2:
					Org.MiscServ.OM_IMPartAttrib2Name = "Some Number";
					Org.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.VIN;
					break;
				case 3:
					Org.MiscServ.OM_IMPartAttrib3Name = "Some Number";
					Org.MiscServ.OM_IMPartAttrib3Type = PartAttributeTypeList.Codes.VIN;
					break;
				case 4:
					Org.MiscServ.OM_IMPartAttrib1Name = "Some Number";
					Org.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.BatchNumber;
					break;
				case 5:
					Org.MiscServ.OM_IMPartAttrib2Name = "Some Number";
					Org.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.JulianBatchNumber;
					break;
			}
		}

		void ClearOrgNameAndType()
		{
			Org.MiscServ.OM_IMPartAttrib1Name = "";
			Org.MiscServ.OM_IMPartAttrib1Type = "";
			Org.MiscServ.OM_IMPartAttrib2Name = "";
			Org.MiscServ.OM_IMPartAttrib2Type = "";
			Org.MiscServ.OM_IMPartAttrib3Name = "";
			Org.MiscServ.OM_IMPartAttrib3Type = "";
		}

		protected void AssertNoPartAttributeNotifications(ZPropertyInfo info) => AssertNoPartAttributeNotifications(string.Empty, info);
		protected virtual void AssertNoPartAttributeNotifications(string message, ZPropertyInfo info) => AssertNoErrors(message, info);

		protected void AssertHasPartAttributeNotification(ZPropertyInfo info, string notificationExpectedToBeFound) => AssertHasPartAttributeNotification(string.Empty, info, notificationExpectedToBeFound);
		protected virtual void AssertHasPartAttributeNotification(string message, ZPropertyInfo info, string notificationExpectedToBeFound) => AssertHasError(message, info, notificationExpectedToBeFound);

		protected virtual void AssertHasPartAttributeNotifications(string message, ZPropertyInfo info) => AssertHasErrors(message, info);

		protected override void SetUp()
		{
			base.SetUp();

			Org = Factory.NewWithValidTestData<OrgHeader>();
			Org.OH_FullName = "IMPORTER";
			Part = Factory.NewWithValidTestData<OrgSupplierPart>();
			Relation = Part.RelatedOrganisations.AddNew();
			Part.OP_PartNum = "P1";
			Part.OP_Desc = "P1DESC";
			Relation.OU_OH = Org.PK;
			Relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			Dummy = Factory.New<DummyBusinessObject>();
		}

		OrgHeader Org;
		OrgSupplierPart Part;
		OrgPartRelation Relation;
		DummyBusinessObject Dummy;

		PartAttributeValidation Validation => GetNewValidationCore();
		protected virtual PartAttributeValidation GetNewValidationCore() => new PartAttributeValidation();

		#endregion
	}
}
