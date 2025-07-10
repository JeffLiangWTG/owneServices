using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.PortHubs.Business;
using Enterprise.Integration.Packing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Rating.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Business.Testing;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using CodeDescriptionPair = Enterprise.ZArchitecture.Core.CodeDescriptionPair;

namespace Enterprise.TransportBookings.Business.Test
{
	[TestedType(typeof(DtbBookingInstruction))]
	public class DtbBookingInstructionBizOTest : DtbTransportBusinessObjectTestCase
	{
		// persistent

		public void TestKN_InstructionType_RefreshesConNoteNo()
		{
			var instruction = (DtbBookingInstruction)GetNewBusinessObject();

			instruction.KN_InstructionType = InstructionTypes.Codes.Delivery;
			instruction.ConNoteNo = "abc";
			AssertEquals("Precondition", "abc", instruction.ConNoteNo);

			bool instructionConNoteNoRefreshBindingFired = false;
			instruction.ConNoteNoInfo.ValueChanged += (sender, e) => instructionConNoteNoRefreshBindingFired = true;
			instruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			AssertEquals(true, instructionConNoteNoRefreshBindingFired);
			AssertEquals("ConNote # should be empty when Instruction is Pickup.", "", instruction.ConNoteNo);
		}

		// calculated

		public void TestConNoteNo()
		{
			var instruction = (DtbBookingInstruction)GetNewBusinessObject();
			instruction.KN_InstructionType = InstructionTypes.Codes.Delivery;
			AssertEquals("Precondition", false, instruction.Confirmations.Any(c => c.IsConNoteNo));
			AssertEquals("", instruction.ConNoteNo);

			instruction.ConNoteNo = "abc";
			AssertEquals("abc", instruction.ConNoteNo);
			instruction.Confirmations.Single(c => c.IsConNoteNo); // should auto-create a Confirmation

			instruction.ConNoteNo = "xyz";
			AssertEquals("xyz", instruction.ConNoteNo);
			instruction.Confirmations.Single(c => c.IsConNoteNo); // should re-use existing Confirmation

			// pickup -- should *not* pull connote #
			instruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			AssertEquals("", instruction.ConNoteNo);
			instruction.KN_InstructionType = InstructionTypes.Codes.Delivery; // cleanup

			// multiple con note # confirmations
			instruction.Confirmations.AddNew(ConfirmationTypes.Codes.ConNoteNo);
			AssertEquals("Multiple ConNote # Confirmations exist, even if empty, we should not use them.", "Many", instruction.ConNoteNo);

			// should *not* allow setting when multiple connote #'s exist as we don't know which confirmation to set
			AssertExceptionThrown(typeof(ArgumentException), () => instruction.ConNoteNo = "123");
		}

		public void TestConNoteNo_ReadOnly()
		{
			var instruction = (DtbBookingInstruction)GetNewBusinessObject();

			instruction.KN_InstructionType = InstructionTypes.Codes.Multi;
			AssertEquals(false, instruction.ConNoteNoInfo.ReadOnly);

			instruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			AssertEquals("ConNote # should not be entered for a Pickup.", true, instruction.ConNoteNoInfo.ReadOnly);

			instruction.KN_InstructionType = InstructionTypes.Codes.Delivery;
			AssertEquals(false, instruction.ConNoteNoInfo.ReadOnly);

			instruction.Confirmations.AddNew(ConfirmationTypes.Codes.ConNoteNo);
			AssertEquals(false, instruction.ConNoteNoInfo.ReadOnly);

			// multiple con note # confirmations
			instruction.Confirmations.AddNew(ConfirmationTypes.Codes.ConNoteNo);
			AssertEquals("Multiple ConNote # Confirmations exist, Instruction ConNote # should be ReadOnly.", true, instruction.ConNoteNoInfo.ReadOnly);
		}

		protected override Type ExpectedLookupsType
		{
			get { return typeof(DtbBookingInstructionLookups); }
		}

		public void TestGetAddress()
		{
			var booking = Factory.New<DtbBooking>();
			var instruction = booking.Instructions.AddNew();

			var organisation = Helper.CreateOrganisation("QWESYD");
			var pickupAddress = Helper.AddAddressToOrganisation(organisation, "Pickup Addy", OrgAddressType.Pickup);
			var officeAddress = Helper.AddAddressToOrganisation(organisation, "Office Addy", OrgAddressType.Office);
			var deliveryAddress = Helper.AddAddressToOrganisation(organisation, "Delivery Addy", OrgAddressType.Delivery);

			instruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			instruction.Address.OrganisationPK = organisation.PK;
			AssertEquals("Should default to pickupAddress", pickupAddress.PK, instruction.GetAddress(DocAddressType.TransportCompanyDocumentaryAddress).E2_OA_Address);
		}

		public void TestOnDocAddressChanged()
		{
			//Setup
			var booking = Helper.CreateBooking();
			var cneOrg = Helper.CreateOrganisation("AAA");
			var cnrOrg = Helper.CreateOrganisation("BBB");
			var unrelatedOrg = Helper.CreateOrganisation("CCC");
			var cnrInstruction = Helper.CreateInstruction(booking, TransportStatuses.Codes.PickedUp);
			var cnrInstruction2 = Helper.CreateInstruction(booking, TransportStatuses.Codes.PickedUp);
			var cneInstruction = Helper.CreateInstruction(booking, TransportStatuses.Codes.Delivered);
			var cneInstruction2 = Helper.CreateInstruction(booking, TransportStatuses.Codes.Delivered);
			var unrelatedInstruction = Helper.CreateInstruction(booking, TransportStatuses.Codes.Delivered);

			cneOrg.SupplierLinks.AddNew(cnrOrg);
			cneInstruction.OrganisationType = OrganisationTypesList.Codes.CNE;
			cnrInstruction.OrganisationType = OrganisationTypesList.Codes.CNR;
			unrelatedInstruction.OrganisationType = OrganisationTypesList.Codes.CFS;

			//Test CNE OnDocAddressChanged
			cneInstruction.Address.OrganisationPK = cneOrg.PK;
			AssertEquals(cnrOrg.PK, cnrInstruction.Address.OrganisationPK);
			AssertEquals(true, unrelatedInstruction.Address.E2_OA_Address.IsEmpty);
			AssertEquals("Only set the first Consignor", true, cnrInstruction2.Address.E2_OA_Address.IsEmpty);
			AssertEquals("Only set the first Consignee", true, cneInstruction2.Address.E2_OA_Address.IsEmpty);

			//Clear Fields
			cneInstruction.Address.OrganisationPK = ZGuid.Empty;
			cnrInstruction.Address.OrganisationPK = ZGuid.Empty;

			//Test CNR OnDocAddressChanged
			cnrInstruction.Address.OrganisationPK = cnrOrg.PK;
			AssertEquals(cneOrg.PK, cneInstruction.Address.OrganisationPK);
			AssertEquals(true, unrelatedInstruction.Address.E2_OA_Address.IsEmpty);
			AssertEquals("Only set the first Consignor", true, cnrInstruction2.Address.E2_OA_Address.IsEmpty);
			AssertEquals("Only set the first Consignee", true, cneInstruction2.Address.E2_OA_Address.IsEmpty);

			//Clear Fields
			cneInstruction.Address.OrganisationPK = ZGuid.Empty;
			cnrInstruction.Address.OrganisationPK = ZGuid.Empty;

			//Test Only CNR or CNE affect OnDocAddressChanged of Related instructions.
			unrelatedInstruction.Address.OrganisationPK = unrelatedOrg.PK;
			AssertEquals(true, cnrInstruction.Address.E2_OA_Address.IsEmpty);
			AssertEquals(true, cneInstruction.Address.E2_OA_Address.IsEmpty);
			AssertEquals("Only set the first Consignor", true, cnrInstruction2.Address.E2_OA_Address.IsEmpty);
			AssertEquals("Only set the first Consignee", true, cneInstruction2.Address.E2_OA_Address.IsEmpty);
		}

		protected override Type ExpectedValidationType
		{
			get { return typeof(DtbBookingInstructionValidation); }
		}

		public void TestDeleteDivotIfInstructionIsDeletedInAnotherEnterpriseInstance()
		{
			Factory.RefreshEnabled = false;

			var consol = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consol);
			var pickupInstruction = Helper.CreateInstruction(booking, "PIC");
			var deliveryInstruction = Helper.CreateInstruction(booking, "DLV");
			var packageJob = consol.PackageJob;
			var package = Helper.CreatePackage("P1", 1);
			packageJob.Packages.Add(package);
			var pickupDivot = Helper.CreatePackageDivot(pickupInstruction, package);
			var deliveryDivot = Helper.CreatePackageDivot(deliveryInstruction, package);

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			otherFactory.RefreshEnabled = false;

			var deliveryInstruction_DeleteFactory = otherFactory.Load<DtbBookingInstruction>(deliveryInstruction.PK);
			var deliveryDivot_DeleteFactory = otherFactory.Load<DtbBookingInstructionPkgDivot>(deliveryDivot.PK);
			deliveryInstruction_DeleteFactory.Delete();
			otherFactory.Save();
			Assert("Delivery Instruction should be deleted in DeleteFactory.", deliveryInstruction_DeleteFactory.IsDeleted);
			Assert("Delivery Divot should be deleted in DeleteFactory.", deliveryDivot_DeleteFactory.IsDeleted);

			deliveryInstruction.KN_DropMode = "XXX";
			try
			{
				Factory.Save();
				Assert("Should have failed save with a concurrency error.", false);
			}
			catch (ZSaveConcurrencyException ex)
			{
				ZExceptionReporting.HandleZSaveConcurrencyException(ex, NotificationHandler.Instance, false);
				Assert("Delivery Instruction should be deleted in the Factory that had to merge.", deliveryInstruction.IsDeleted);
				Assert("Delivery Divot should be deleted in the Factory that had to merge.", deliveryDivot.IsDeleted);
			}
		}

		public void TestDeleteConfirmationsIfInstructionIsDeletedInAnotherEnterprise()
		{
			Factory.RefreshEnabled = false;

			var consol = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consol);
			var deliveryInstruction = Helper.CreateInstruction(booking, "DLV");
			var confirmation = deliveryInstruction.Confirmations.AddNew();

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			otherFactory.RefreshEnabled = false;

			var deliveryInstruction_DeleteFactory = otherFactory.Load<DtbBookingInstruction>(deliveryInstruction.PK);
			var confirmation_DeleteFactory = otherFactory.Load<DtbBookingConfirmation>(confirmation.PK);
			deliveryInstruction_DeleteFactory.Delete();
			otherFactory.Save();
			Assert("Precondition: Delivery Instruction should be deleted in DeleteFactory.", deliveryInstruction_DeleteFactory.IsDeleted);
			Assert("Precondition: Confirmation should be deleted in DeleteFactory", confirmation_DeleteFactory.IsDeleted);

			deliveryInstruction.KN_DropMode = "XXX";
			try
			{
				Factory.Save();
				Assert("Should have failed save with a concurrency error.", false);
			}
			catch (ZSaveConcurrencyException ex)
			{
				ZExceptionReporting.HandleZSaveConcurrencyException(ex, NotificationHandler.Instance, false);
			}
			Assert("Delivery Instruction should be deleted in the Factory that had to merge.", deliveryInstruction.IsDeleted);
			Assert("Confirmation should be deleted in the Factory that had to merge.", confirmation.IsDeleted);
		}

		public void TestDeleteDocAddressesIfInstructionIsDeletedInAnotherEnterprise()
		{
			Factory.RefreshEnabled = false;

			var consol = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consol);
			var deliveryInstruction = Helper.CreateInstruction(booking, "DLV");
			var jobDocAddress = deliveryInstruction.Address;

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			otherFactory.RefreshEnabled = false;

			var deliveryInstruction_DeleteFactory = otherFactory.Load<DtbBookingInstruction>(deliveryInstruction.PK);
			var jobDocAddress_DeleteFactory = otherFactory.Load<JobDocAddress>(jobDocAddress.PK);
			deliveryInstruction_DeleteFactory.Delete();
			otherFactory.Save();
			Assert("Precondition: Delivery Instruction should be deleted in DeleteFactory.", deliveryInstruction_DeleteFactory.IsDeleted);
			Assert("Precondition: JobDocAddress should be deleted in DeleteFactory", jobDocAddress_DeleteFactory.IsDeleted);

			deliveryInstruction.KN_DropMode = "XXX";
			try
			{
				Factory.Save();
				Assert("Should have failed save with a concurrency error.", false);
			}
			catch (ZSaveConcurrencyException ex)
			{
				ZExceptionReporting.HandleZSaveConcurrencyException(ex, NotificationHandler.Instance, false);
			}
			Assert("Delivery Instruction should be deleted in the Factory that had to merge.", deliveryInstruction.IsDeleted);
			Assert("Confirmation should be deleted in the Factory that had to merge.", jobDocAddress.IsDeleted);
		}

		public void TestSequenceInstructionsIfInstructionIsDeletedInAnotherEnterprise()
		{
			Factory.RefreshEnabled = false;

			var consol = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consol);
			var pickupInstruction = Helper.CreateInstruction(booking, "PIC");
			var deliveryInstruction = Helper.CreateInstruction(booking, "DLV");

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			otherFactory.RefreshEnabled = false;

			var pickupInstruction_DeleteFactory = otherFactory.Load<DtbBookingInstruction>(pickupInstruction.PK);
			var deliveryInstruction_DeleteFactory = otherFactory.Load<DtbBookingInstruction>(deliveryInstruction.PK);
			pickupInstruction_DeleteFactory.Delete();
			otherFactory.Save();
			Assert("Precondition: Pickup Instruction should be deleted in DeleteFactory.", pickupInstruction_DeleteFactory.IsDeleted);
			AssertEquals("Precondition: Delivery Instruction should have its sequence number updated in DeleteFactory", 1, deliveryInstruction_DeleteFactory.KN_Sequence);

			pickupInstruction.KN_DropMode = "XXX";
			try
			{
				Factory.Save();
				Assert("Should have failed save with a concurrency error.", false);
			}
			catch (ZSaveConcurrencyException ex)
			{
				ZExceptionReporting.HandleZSaveConcurrencyException(ex, NotificationHandler.Instance, false);
			}
			Assert("Pickup Instruction should be deleted in the Factory that had to merge.", pickupInstruction.IsDeleted);
			AssertEquals("Delivery Instruction should have its sequence number changed in the Factory that had to merge.", deliveryInstruction_DeleteFactory.KN_Sequence, deliveryInstruction.KN_Sequence);
		}

		public void TestDeleteInstruction_IfDeletedInAnotherEnterpriseInstance_DeletesDivot_DoesNotCauseDeletedRowInaccessibleException()
		{
			Factory.RefreshEnabled = false;

			var consol = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consol);
			var deliveryInstruction = Helper.CreateInstruction(booking, "DLV");
			var packageJob = consol.PackageJob;
			var package = Helper.CreatePackage("P1", 1);
			packageJob.Packages.Add(package);
			var deliveryDivot = Helper.CreatePackageDivot(deliveryInstruction, package);

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			otherFactory.RefreshEnabled = false;

			var deliveryInstruction_OtherFactory = otherFactory.Load<DtbBookingInstruction>(deliveryInstruction.PK);
			var deliveryDivot_OtherFactory = otherFactory.Load<DtbBookingInstructionPkgDivot>(deliveryDivot.PK);
			deliveryInstruction_OtherFactory.Delete();
			otherFactory.Save();
			Assert("Precondition: Delivery Instruction should be deleted in OtherFactory.", deliveryInstruction_OtherFactory.IsDeleted);
			Assert("Precondition: Delivery Divot should be deleted in OtherFactory.", deliveryDivot_OtherFactory.IsDeleted);

			deliveryInstruction.Delete();
			try
			{
				Factory.Save();
				Assert("Should have failed save with a concurrency error, so the user gets given a warning about another user making changes.", false);
			}
			catch (ZSaveConcurrencyException ex)
			{
				ZExceptionReporting.HandleZSaveConcurrencyException(ex, NotificationHandler.Instance, false);
			}
			Assert("Delivery Instruction should be deleted in the Factory that had to merge.", deliveryInstruction.IsDeleted);
			Assert("Delivery Divot should be deleted in the Factory that had to merge.", deliveryDivot.IsDeleted);
		}

		public void TestDeleteInstruction_IfDeletedInAnotherEnterpriseInstance_DeletesForDataRefreshVariousCollectionsAndResequencesInstructions()
		{
			Factory.RefreshEnabled = false;

			var consol = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consol);
			var pickupInstruction = Helper.CreateInstruction(booking, "PIC");
			var deliveryInstruction = Helper.CreateInstruction(booking, "DLV");
			var confirmation = pickupInstruction.Confirmations.AddNew();
			var jobDocAddress = pickupInstruction.Address;

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			otherFactory.RefreshEnabled = false;

			var pickupInstruction_OtherFactory = otherFactory.Load<DtbBookingInstruction>(pickupInstruction.PK);
			var deliveryInstruction_OtherFactory = otherFactory.Load<DtbBookingInstruction>(deliveryInstruction.PK);
			var confirmation_OtherFactory = otherFactory.Load<DtbBookingConfirmation>(confirmation.PK);
			var jobDocAddress_OtherFactory = otherFactory.Load<JobDocAddress>(jobDocAddress.PK);
			pickupInstruction_OtherFactory.Delete();
			otherFactory.Save();
			Assert("Precondition: Pickup Instruction should be deleted in OtherFactory.", pickupInstruction_OtherFactory.IsDeleted);
			Assert("Precondition: Confirmation should be deleted in OtherFactory.", confirmation_OtherFactory.IsDeleted);
			Assert("Precondition: JobDocAddress should be deleted in OtherFactory.", jobDocAddress_OtherFactory.IsDeleted);
			AssertEquals("Precondition: Delivery Instruction should have its sequence number updated in DeleteFactory", 1, deliveryInstruction_OtherFactory.KN_Sequence);

			pickupInstruction.Delete();
			try
			{
				Factory.Save();
				Assert("Should have failed save with a concurrency error, so the user gets given a warning about another user making changes.", false);
			}
			catch (ZSaveConcurrencyException ex)
			{
				ZExceptionReporting.HandleZSaveConcurrencyException(ex, NotificationHandler.Instance, false);
			}
			Assert("Pickup Instruction should be deleted in the Factory that had to merge.", pickupInstruction.IsDeleted);
			Assert("Confirmation should be deleted in the Factory that had to merge.", confirmation.IsDeleted);
			Assert("JobDocAddress should be deleted in the Factory that had to merge.", jobDocAddress.IsDeleted);
			AssertEquals("Delivery Instruction should have correct sequence number in the Factory that had to merge.", deliveryInstruction_OtherFactory.KN_Sequence, deliveryInstruction.KN_Sequence);
		}

		public void TestIDocAddresses_GetCanOverrideCheckpoint()
		{
			var booking = Factory.New<DtbBooking>();
			var instruction = booking.Instructions.AddNew();
			var iInstruction = (IDocAddresses)instruction;
			AssertEquals(Env.Security.DtbBookingMISCDetails, iInstruction.GetCanOverrideCheckpoint(null));

			var jobDocAddress = Factory.New<JobDocAddress>();
			jobDocAddress.E2_AddressType = DocAddressTypes.Codes.LocalCartageCFS;
			AssertEquals(Env.Security.DtbBookingMISCDetailsCFS, iInstruction.GetCanOverrideCheckpoint(jobDocAddress));

			jobDocAddress.E2_AddressType = DocAddressTypes.Codes.LocalCartageCTO;
			AssertEquals(Env.Security.DtbBookingMISCDetailsCTO, iInstruction.GetCanOverrideCheckpoint(jobDocAddress));

			jobDocAddress.E2_AddressType = DocAddressTypes.Codes.LocalCartageExporter;
			AssertEquals(Env.Security.DtbBookingMISCDetailsConsignor, iInstruction.GetCanOverrideCheckpoint(jobDocAddress));

			jobDocAddress.E2_AddressType = DocAddressTypes.Codes.LocalCartageImporter;
			AssertEquals(Env.Security.DtbBookingMISCDetailsConsignee, iInstruction.GetCanOverrideCheckpoint(jobDocAddress));

			jobDocAddress.E2_AddressType = DocAddressTypes.Codes.LocalCartageYard;
			AssertEquals(Env.Security.DtbBookingMISCDetailsContainerYard, iInstruction.GetCanOverrideCheckpoint(jobDocAddress));

			jobDocAddress.E2_AddressType = DocAddressTypes.Codes.LocalCartageService;
			AssertEquals(Env.Security.DtbBookingMISCDetailsOtherServiceProvider, iInstruction.GetCanOverrideCheckpoint(jobDocAddress));

			jobDocAddress.E2_AddressType = DocAddressTypes.Codes.LocalCartageMSC;
			AssertEquals(Env.Security.DtbBookingMISCDetailsMiscAddress, iInstruction.GetCanOverrideCheckpoint(jobDocAddress));
		}

		public void TestDefaultValues()
		{
			var booking = Factory.New<DtbBookingInstruction>();
			AssertEquals(TransportStatuses.Codes.Available, booking.KN_Status);
		}

		/// <summary>
		/// [Category]		[Add]
		/// Containers		Containers
		/// Both			Containers and their child Loose + Top Level Loose (custom requires top level loose, because they don't ALWAYS pack containers, sometimes they do though!)
		/// Outer			Containers and Top Level Loose
		/// Loose			Loose inside containers and Top Level Loose
		/// </summary>
		public void TestDefaultPackages()
		{
			var booking = GetNewBooking();
			var org = Helper.CreateOrganisation("AAA");
			var instruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNE, org.Addresses.MainAddress);
			var instructionToGetPkgFrom = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, org.Addresses.MainAddress);
			var container = Helper.CreatePackage("CONT", 1, "CNT");
			var containerInnerPackage = Helper.CreatePackage("CONT_IN", 1);
			var containerInnerInnerPackage = Helper.CreatePackage("CONT_IN_IN", 1);
			var container2 = Helper.CreatePackage("CONT2", 1, "CNT");
			var container2InnerPackage = Helper.CreatePackage("CONT2_IN", 1);
			var container2InnerInnerPackage = Helper.CreatePackage("CONT2_IN_IN", 1);
			var container3 = Helper.CreatePackage("CONT3", 1, "CNT");
			var topLevelPackage = Helper.CreatePackage("TOP", 1);
			var topInnerPackage = Helper.CreatePackage("TOP_IN", 1);

			// Package tree
			booking.PackageJob.Packages.Add(container);
			booking.PackageJob.Packages.Add(container2);
			booking.PackageJob.Packages.Add(container3);
			booking.PackageJob.Packages.Add(topLevelPackage);
			container.Packages.Add(containerInnerPackage);
			containerInnerPackage.Packages.Add(containerInnerInnerPackage);
			container2.Packages.Add(container2InnerPackage);
			container2InnerPackage.Packages.Add(container2InnerInnerPackage);
			topLevelPackage.Packages.Add(topInnerPackage);

			AssertPackages(instruction, "", new[] { container, topLevelPackage }, Array.Empty<PkgPackage>());
			AssertPackages(instruction, PackageCategories.Codes.Containers, new[] { container }, new[] { container });
			AssertPackages(instruction, PackageCategories.Codes.Containers, new[] { container, topLevelPackage }, new[] { container });
			AssertPackages(instruction, PackageCategories.Codes.Containers, new[] { topLevelPackage }, new[] { container, container2, container3 }); // none, fall back
			AssertPackages(instruction, PackageCategories.Codes.Outers, new[] { topLevelPackage }, new[] { topLevelPackage });
			AssertPackages(instruction, PackageCategories.Codes.Outers, new[] { container, topLevelPackage }, new[] { container, topLevelPackage });
			AssertPackages(instruction, PackageCategories.Codes.Outers, new[] { container }, new[] { container });
			AssertPackages(instruction, PackageCategories.Codes.Both, new[] { topLevelPackage }, new[] { topLevelPackage });
			AssertPackages(instruction, PackageCategories.Codes.Both, new[] { container, topLevelPackage }, new[] { container, containerInnerPackage, topLevelPackage });
			AssertPackages(instruction, PackageCategories.Codes.Both, new[] { container }, new[] { container, containerInnerPackage });
			AssertPackages(instruction, PackageCategories.Codes.Loose, new[] { topLevelPackage }, new[] { topLevelPackage });
			AssertPackages(instruction, PackageCategories.Codes.Loose, new[] { container, topLevelPackage }, new[] { containerInnerPackage, topLevelPackage });
			AssertPackages(instruction, PackageCategories.Codes.Loose, new[] { container }, new[] { containerInnerPackage });
			AssertPackages(instruction, PackageCategories.Codes.Loose, new[] { container3 }, new[] { topLevelPackage, containerInnerPackage, container2InnerPackage }); // none, fall back

			// fall back to package job when none
			AssertPackages(instruction, "", Array.Empty<PkgPackage>(), Array.Empty<PkgPackage>());
			AssertPackages(instruction, PackageCategories.Codes.Containers, Array.Empty<PkgPackage>(), new[] { container, container2, container3 });
			AssertPackages(instruction, PackageCategories.Codes.Outers, Array.Empty<PkgPackage>(), new[] { container, container2, container3, topLevelPackage });
			AssertPackages(instruction, PackageCategories.Codes.Both, Array.Empty<PkgPackage>(), new[] { container, container2, container3, containerInnerPackage, container2InnerPackage, topLevelPackage });
			AssertPackages(instruction, PackageCategories.Codes.Loose, Array.Empty<PkgPackage>(), new[] { topLevelPackage, containerInnerPackage, container2InnerPackage });
		}

		public void TestDefaultPackagesForValidPossiblePackagesShouldMakeCorrectUpdatesAfterDivotAssignment()
		{
			var booking = Helper.CreateBooking();
			booking.ConsolidationSingleJob.KB_JobDirection = "DLV";
			booking.KM_Direction = "DST";
			booking.KM_KT_NKBookingTemplate = "IFDS";
			var pickupInstruction = booking.Instructions.Single(i => i.KN_InstructionType == "PIC" && i.OrganisationType == "CNR" && i.PackageCategory == "LSE");
			var consignor = Helper.CreateOrganisation("CNR");
			consignor.MainAddress.OA_LCLEquipmentNeeded = "DM1";
			pickupInstruction.Address.OrganisationPK = consignor.PK;

			var container1 = Helper.CreatePackageContainer("CONT1");
			var container1Package1Hazardous = Helper.CreatePackage("CONT1_IN1_HAZ");
			container1Package1Hazardous.UNDGs.AddNew();
			var container1Package2 = Helper.CreatePackage("CONT1_IN2");
			booking.PackageJob.Packages.Add(container1);
			container1.Packages.Add(container1Package1Hazardous);
			container1.Packages.Add(container1Package2);

			var container2 = Helper.CreatePackageContainer("CONT1");
			var container2Package1Refrigerated = Helper.CreatePackage("CONT1_IN1_REFRIG");
			container2Package1Refrigerated.KP_RequiresTemperatureControl = true;
			var container2Package2 = Helper.CreatePackage("CONT1_IN2");
			booking.PackageJob.Packages.Add(container2);
			container2.Packages.Add(container2Package1Refrigerated);
			container2.Packages.Add(container2Package2);

			pickupInstruction.Confirmations.DeleteAll();

			CombineAssertions("Precondition: Check initial setup details for test", () =>
			{
				Assert($"Precondition: should be no assigned packages yet on {nameof(pickupInstruction)}", !pickupInstruction.DivotsWithPackages.Any());
				Assert($"Precondition: {nameof(pickupInstruction)} has no confirmations (as set up before to show that after the act it has been set true by the aftermath of DefaultPackages())", !pickupInstruction.Confirmations.Any());
				AssertEquals($"Precondition: {nameof(pickupInstruction)}.KN_DropMode has not been set yet", string.Empty, pickupInstruction.KN_DropMode);
				AssertEquals($"Precondition: {nameof(booking)}.KM_IsHazardous has not been set yet", false, booking.KM_IsHazardous);
				AssertEquals($"Precondition: {nameof(booking)}.KM_RequiresRefrigeration has not been set yet", false, booking.KM_RequiresRefrigeration);
			});

			var divotsWithPackagesListChanged = 0;
			((IBindingList)pickupInstruction.DivotsWithPackages).ListChanged += (object sender, ListChangedEventArgs e) =>
			{
				divotsWithPackagesListChanged++;
			};

			pickupInstruction.DefaultPackages(booking.PackageJob.Packages);

			CombineAssertions("Check that DefaultPackages() ran successfully and only called DivotsWithPackages.ListChanged once", () =>
			{
				AssertEquals($"{nameof(pickupInstruction)}.DivotsWithPackages.ListChanged was only called once", 1, divotsWithPackagesListChanged);

				AssertEquals($"{nameof(pickupInstruction)} should have 4 package divots", 4, pickupInstruction.DivotsWithPackages.Count);

				foreach (var divot in pickupInstruction.DivotsWithPackages)
				{
					divot.Validation.ValidateKD_KN_BookingInstruction();
					AssertEquals("Divot.KD_KN_BookingInstruction should not have any validation errors or warnings caused by duplicate divots (multiple divots pointing to the same instruction and package)", false, divot.KD_KN_BookingInstructionInfo.HasNotifications());
					divot.Validation.ValidateKD_KP_Package();
					AssertEquals("Divot.KD_KP_Package should not have any validation errors or warnings caused by duplicate divots (multiple divots pointing to the same instruction and package)", false, divot.KD_KP_PackageInfo.HasNotifications());
				}

				AssertEquals($"{nameof(pickupInstruction)} should have had its confirmation re-created", 1, pickupInstruction.Confirmations.Count);
				AssertEquals($"{nameof(pickupInstruction)}.KN_DropMode should have been set", "DM1", pickupInstruction.KN_DropMode);
				AssertEquals($"{nameof(booking)}.IsHazardous should have been set as one package assigned ({nameof(container1Package1Hazardous)}) is hazardous", true, booking.KM_IsHazardous);
				AssertEquals($"{nameof(booking)}.RequiresRefrigeration should have been set as one package assigned ({nameof(container2Package1Refrigerated)}) requires refrigeration", true, booking.KM_RequiresRefrigeration);
			});
		}

		public void TestDefaultPackagesForValidPossiblePackagesShouldUpdateIsEmptyContainerAfterDivotAssignment()
		{
			var booking = Helper.CreateBooking();
			booking.ConsolidationSingleJob.KB_JobDirection = "DLV";
			booking.KM_Direction = "DST";
			booking.KM_KT_NKBookingTemplate = "IFUD";
			AssertEquals("Precondition: Adding the template should have added four instructions", 4, booking.Instructions.Count);

			var pickupInstruction1 = booking.Instructions.Single(i => i.KN_Sequence == 1 && i.KN_InstructionType == "PIC" && i.OrganisationType == "CTO" && i.PackageCategory == "CNT");
			var multiInstruction2 = booking.Instructions.Single(i => i.KN_Sequence == 2 && i.KN_InstructionType == "MLT" && i.OrganisationType == "CFS" && i.PackageCategory == "BTH");
			var deliveryToCNEInstruction3 = booking.Instructions.Single(i => i.KN_Sequence == 3 && i.KN_InstructionType == "DLV" && i.OrganisationType == "CNE" && i.PackageCategory == "LSE");
			var containerReturnDeliveryInstruction4 = booking.Instructions.Single(i => i.KN_Sequence == 4 && i.KN_InstructionType == "DLV" && i.OrganisationType == "CYD" && i.PackageCategory == "CNT");

			var container = helper.CreatePackage("CONT1", 1, "CNT");
			var containerInnerPackage1Hazardous = helper.CreatePackage("CONT1_IN1_HAZ", 1);
			containerInnerPackage1Hazardous.UNDGs.AddNew();
			var containerInnerPackage2Refrigerated = helper.CreatePackage("CONT1_IN2_REFRIG", 1);
			containerInnerPackage2Refrigerated.KP_RequiresTemperatureControl = true;

			booking.PackageJob.Packages.Add(container);
			container.Packages.Add(containerInnerPackage1Hazardous);
			container.Packages.Add(containerInnerPackage2Refrigerated);

			pickupInstruction1.DivotsWithPackages.AddPackage(container);
			multiInstruction2.DivotsWithPackages.AddPackage(container);
			containerReturnDeliveryInstruction4.DivotsWithPackages.AddPackage(container);

			var containerReturnDeliveryConfirmation4 = containerReturnDeliveryInstruction4.Confirmations[0];
			AssertEquals($"Precondition: {nameof(containerReturnDeliveryConfirmation4)}.KK_IsEmptyContainer was set true by call to SetIsEmptyContainer triggered during setup", true, containerReturnDeliveryConfirmation4.KK_IsEmptyContainer);
			containerReturnDeliveryConfirmation4.KK_IsEmptyContainer = false;

			deliveryToCNEInstruction3.Confirmations.DeleteAll();

			AssertEquals($"Precondition: {nameof(booking)}.KM_IsHazardous was set to true after divots on hazardous package {nameof(containerInnerPackage1Hazardous)} were assigned to instructions 1, 2 and 4", true, booking.KM_IsHazardous);
			AssertEquals($"Precondition: {nameof(booking)}.KM_RequiresRefrigeration was set to true after divots on package requiring refrigeration {nameof(containerInnerPackage2Refrigerated)} were assigned to instructions 1, 2 and 4", true, booking.KM_RequiresRefrigeration);
			booking.KM_IsHazardous = false;
			booking.KM_RequiresRefrigeration = false;

			var pickupConfirmation1 = pickupInstruction1.Confirmations.Single();

			CombineAssertions("Precondition: Check initial setup details for test", () =>
			{
				Assert($"Precondition: should be no assigned packages yet on {nameof(deliveryToCNEInstruction3)}", !deliveryToCNEInstruction3.DivotsWithPackages.Any());
				AssertEquals($"Precondition: {nameof(pickupConfirmation1)}.KK_IsEmptyContainer is false", false, pickupConfirmation1.KK_IsEmptyContainer);
				Assert($"Precondition: {nameof(deliveryToCNEInstruction3)} has no confirmations (as set up before to show that after the act it has been set true by the aftermath of DefaultPackages())", !deliveryToCNEInstruction3.Confirmations.Any());
				AssertEquals($"Precondition: {nameof(containerReturnDeliveryConfirmation4)}.KK_IsEmptyContainer is set to false during setup (to help show that the act of calling DefaultPackages() sets it back to true)", false, containerReturnDeliveryConfirmation4.KK_IsEmptyContainer);
				AssertEquals($"Precondition: {nameof(booking)}.KM_IsHazardous is set to false during setup (to help show that the act of calling DefaultPackages() sets it back to true)", false, booking.KM_IsHazardous);
				AssertEquals($"Precondition: {nameof(booking)}.KM_RequiresRefrigeration is set to false during setup (to help show that the act of calling DefaultPackages() sets it back to true)", false, booking.KM_RequiresRefrigeration);
			});

			var divotsWithPackagesListChanged = 0;
			((IBindingList)deliveryToCNEInstruction3.DivotsWithPackages).ListChanged += (object sender, ListChangedEventArgs e) =>
			{
				divotsWithPackagesListChanged++;
			};

			deliveryToCNEInstruction3.DefaultPackages(booking.PackageJob.Packages);

			CombineAssertions("Check that DefaultPackages() ran successfully and only called DivotsWithPackages.ListChanged once", () =>
			{
				AssertEquals($"{nameof(deliveryToCNEInstruction3)}.DivotsWithPackages.ListChanged was only called once", 1, divotsWithPackagesListChanged);

				AssertEquals($"{nameof(deliveryToCNEInstruction3)} should have 2 package divots", 2, deliveryToCNEInstruction3.DivotsWithPackages.Count);

				foreach (var divot in deliveryToCNEInstruction3.DivotsWithPackages)
				{
					divot.Validation.ValidateKD_KN_BookingInstruction();
					AssertEquals("Divot.KD_KN_BookingInstruction should not have any validation errors or warnings caused by duplicate divots (multiple divots pointing to the same instruction and package)", false, divot.KD_KN_BookingInstructionInfo.HasNotifications());
					divot.Validation.ValidateKD_KP_Package();
					AssertEquals("Divot.KD_KP_Package should not have any validation errors or warnings caused by duplicate divots (multiple divots pointing to the same instruction and package)", false, divot.KD_KP_PackageInfo.HasNotifications());
				}

				AssertEquals($"{nameof(pickupConfirmation1)}.KK_IsEmptyContainer should not have been set true", false, pickupConfirmation1.KK_IsEmptyContainer);
				AssertEquals($"{nameof(deliveryToCNEInstruction3)} should have had its confirmation created", 1, deliveryToCNEInstruction3.Confirmations.Count);
				var deliveryToCNEConfirmation3 = deliveryToCNEInstruction3.Confirmations.Single();
				AssertEquals($"{nameof(deliveryToCNEConfirmation3)}.KK_IsEmptyContainer should not have been set true", false, deliveryToCNEConfirmation3.KK_IsEmptyContainer);
				AssertEquals($"{nameof(containerReturnDeliveryConfirmation4)}.KK_IsEmptyContainer should have been set true", true, containerReturnDeliveryConfirmation4.KK_IsEmptyContainer);
				AssertEquals($"{nameof(booking)}.IsHazardous should have been set as one package assigned {nameof(containerInnerPackage1Hazardous)} is hazardous", true, booking.KM_IsHazardous);
				AssertEquals($"{nameof(booking)}.RequiresRefrigeration should have been set as one package assigned {nameof(containerInnerPackage2Refrigerated)} requires refrigeration", true, booking.KM_RequiresRefrigeration);
			});
		}

		public void TestDefaultPackagesWithoutFallback()
		{
			var booking = GetNewBooking();
			var org = Helper.CreateOrganisation("AAA");
			var instruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNE, org.Addresses.MainAddress);
			var instructionToGetPkgFrom = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, org.Addresses.MainAddress);
			var container = Helper.CreatePackage("CONT", 1, "CNT");
			var containerInnerPackage = Helper.CreatePackage("CONT_IN", 1);
			var containerInnerInnerPackage = Helper.CreatePackage("CONT_IN_IN", 1);
			var container2 = Helper.CreatePackage("CONT2", 1, "CNT");
			var container2InnerPackage = Helper.CreatePackage("CONT2_IN", 1);
			var container2InnerInnerPackage = Helper.CreatePackage("CONT2_IN_IN", 1);
			var container3 = Helper.CreatePackage("CONT3", 1, "CNT");
			var topLevelPackage = Helper.CreatePackage("TOP", 1);
			var topInnerPackage = Helper.CreatePackage("TOP_IN", 1);

			booking.PackageJob.Packages.Add(container);
			booking.PackageJob.Packages.Add(container2);
			booking.PackageJob.Packages.Add(container3);
			booking.PackageJob.Packages.Add(topLevelPackage);
			container.Packages.Add(containerInnerPackage);
			containerInnerPackage.Packages.Add(containerInnerInnerPackage);
			container2.Packages.Add(container2InnerPackage);
			container2InnerPackage.Packages.Add(container2InnerInnerPackage);
			topLevelPackage.Packages.Add(topInnerPackage);

			AssertPackagesWithoutFallback(instruction, "", new[] { container, topLevelPackage }, Array.Empty<PkgPackage>());
			AssertPackagesWithoutFallback(instruction, PackageCategories.Codes.Containers, new[] { container }, new[] { container });
			AssertPackagesWithoutFallback(instruction, PackageCategories.Codes.Containers, new[] { container, topLevelPackage }, new[] { container });
			AssertPackagesWithoutFallback(instruction, PackageCategories.Codes.Containers, new[] { topLevelPackage }, Array.Empty<PkgPackage>()); // none, no fall back
			AssertPackagesWithoutFallback(instruction, PackageCategories.Codes.Outers, new[] { topLevelPackage }, new[] { topLevelPackage });
			AssertPackagesWithoutFallback(instruction, PackageCategories.Codes.Outers, new[] { container, topLevelPackage }, new[] { container, topLevelPackage });
			AssertPackagesWithoutFallback(instruction, PackageCategories.Codes.Outers, new[] { container }, new[] { container });
			AssertPackagesWithoutFallback(instruction, PackageCategories.Codes.Both, new[] { topLevelPackage }, new[] { topLevelPackage });
			AssertPackagesWithoutFallback(instruction, PackageCategories.Codes.Both, new[] { container, topLevelPackage }, new[] { container, containerInnerPackage, topLevelPackage });
			AssertPackagesWithoutFallback(instruction, PackageCategories.Codes.Both, new[] { container }, new[] { container, containerInnerPackage });
			AssertPackagesWithoutFallback(instruction, PackageCategories.Codes.Loose, new[] { topLevelPackage }, new[] { topLevelPackage });
			AssertPackagesWithoutFallback(instruction, PackageCategories.Codes.Loose, new[] { container, topLevelPackage }, new[] { containerInnerPackage, topLevelPackage });
			AssertPackagesWithoutFallback(instruction, PackageCategories.Codes.Loose, new[] { container }, new[] { containerInnerPackage });
			AssertPackagesWithoutFallback(instruction, PackageCategories.Codes.Loose, new[] { container3 }, Array.Empty<PkgPackage>()); // none, no fall back

			// do not fall back to package job when none
			AssertPackagesWithoutFallback(instruction, "", Array.Empty<PkgPackage>(), Array.Empty<PkgPackage>());
			AssertPackagesWithoutFallback(instruction, PackageCategories.Codes.Containers, Array.Empty<PkgPackage>(), Array.Empty<PkgPackage>());
			AssertPackagesWithoutFallback(instruction, PackageCategories.Codes.Outers, Array.Empty<PkgPackage>(), Array.Empty<PkgPackage>());
			AssertPackagesWithoutFallback(instruction, PackageCategories.Codes.Both, Array.Empty<PkgPackage>(), Array.Empty<PkgPackage>());
			AssertPackagesWithoutFallback(instruction, PackageCategories.Codes.Loose, Array.Empty<PkgPackage>(), Array.Empty<PkgPackage>());
		}

		public void TestDefaultPackagesWithoutFallbackWithNoPackagesShouldUpdateHazardousAndRefrigerationFlagsBackToFalse()
		{
			var booking = Helper.CreateBooking();
			booking.ConsolidationSingleJob.KB_JobDirection = "DLV";
			booking.KM_Direction = "DST";
			var instruction = booking.Instructions.AddNew(instructionType: "PIC", organisationType: "CNE");
			instruction.PackageCategory = "LSE";

			var container = helper.CreatePackage("CONT1", 1, "CNT");
			var containerInnerPackage1Hazardous = helper.CreatePackage("CONT1_IN1_HAZ", 1);
			containerInnerPackage1Hazardous.UNDGs.AddNew();
			var containerInnerPackage2Refrigerated = helper.CreatePackage("CONT1_IN2_REFRIG", 1);
			containerInnerPackage2Refrigerated.KP_RequiresTemperatureControl = true;

			booking.PackageJob.Packages.Add(container);
			container.Packages.Add(containerInnerPackage1Hazardous);
			container.Packages.Add(containerInnerPackage2Refrigerated);

			instruction.DefaultPackagesWithoutFallback(new[] { containerInnerPackage1Hazardous, containerInnerPackage2Refrigerated });

			CombineAssertions($"Precondition: check {nameof(booking)} KM_IsHazardous and KM_RequiresRefrigeration flags", () =>
			{
				AssertEquals(true, booking.KM_IsHazardous);
				AssertEquals(true, booking.KM_RequiresRefrigeration);
			});

			instruction.DefaultPackagesWithoutFallback(Array.Empty<PkgPackage>());

			CombineAssertions($"Check that DefaultPackages() set {nameof(booking)} KM_IsHazardous and KM_RequiresRefrigeration flags back to false", () =>
			{
				AssertEquals(false, booking.KM_IsHazardous);
				AssertEquals(false, booking.KM_RequiresRefrigeration);
			});
		}

		public void TestParentContainerLinksAndAddresses_WhenFirstContainerMatchesInstructionAddress_ForPickupInstruction()
		{
			var booking = GetNewBooking();
			var org = Helper.CreateOrganisation("AAA");
			var instruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CYD, org.Addresses.MainAddress);
			var container1 = Helper.CreatePackage("CONT1", 1, "CNT");
			var container2 = Helper.CreatePackage("CONT2", 1, "CNT");
			var container3 = Helper.CreatePackage("CONT3", 1, "CNT");
			instruction.OrganisationType = OrganisationTypesList.Codes.CYD;
			instruction.PackageCategory = PackageCategories.Codes.Containers;
			var parentContainerLinksAndAddresses = new List<(ZInt? link, List<(ZString? addressCode, ZString? addressType)>, bool shouldFallbackToShipmentContainerYard)>
			{
				(1, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: AddressTypes.ContainerYardEmptyPickupAddress) }, shouldFallbackToShipmentContainerYard: false ),
				(2, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: "XXXX", addressType: AddressTypes.ContainerYardEmptyPickupAddress) }, shouldFallbackToShipmentContainerYard: false ),
				(3, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: AddressTypes.ContainerYardEmptyReturnAddress) }, shouldFallbackToShipmentContainerYard: false )
			};

			booking.ParentContainerLinksAndAddresses = parentContainerLinksAndAddresses;
			booking.PackageContainerLinks = new Dictionary<ZInt, PkgPackage>() {
				{ 1, container1 },
				{ 2, container2 },
				{ 3, container3 }
			};

			instruction.DefaultPackages(new[] { container1, container2, container3 });
			AssertEquals("Expected packages attached to instruction", 1, instruction.PackageDivots.Count);
			AssertEquals("Only container1 has correct address code and type", "CONT1", instruction.PackageDivots[0].PackageID);
		}

		public void TestParentContainerLinksAndAddresses_WhenBothContainersMatchInstructionAddressButOnlySecondContainerIsPassedIn_ForPickupInstruction()
		{
			var booking = GetNewBooking();
			var org = Helper.CreateOrganisation("AAA");
			var instruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CYD, org.Addresses.MainAddress);
			var container1 = Helper.CreatePackage("CONT1", 1, "CNT");
			var container2 = Helper.CreatePackage("CONT2", 1, "CNT");
			var container3 = Helper.CreatePackage("CONT3", 1, "CNT");
			instruction.OrganisationType = OrganisationTypesList.Codes.CYD;
			instruction.PackageCategory = PackageCategories.Codes.Containers;
			var parentContainerLinksAndAddresses = new List<(ZInt? link, List<(ZString? addressCode, ZString? addressType)>, bool shouldFallbackToShipmentContainerYard)>
			{
				(1, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: AddressTypes.ContainerYardEmptyPickupAddress) }, shouldFallbackToShipmentContainerYard: false ),
				(2, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: AddressTypes.ContainerYardEmptyPickupAddress) }, shouldFallbackToShipmentContainerYard: false ),
				(3, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: AddressTypes.ContainerYardEmptyReturnAddress) }, shouldFallbackToShipmentContainerYard: false )
			};

			booking.ParentContainerLinksAndAddresses = parentContainerLinksAndAddresses;
			booking.PackageContainerLinks = new Dictionary<ZInt, PkgPackage>() {
				{ 1, container1 },
				{ 2, container2 },
				{ 3, container3 }
			};

			instruction.DefaultPackages(new[] { container2 });
			AssertEquals("Expected packages attached to instruction", 1, instruction.PackageDivots.Count);
			AssertEquals("Only container2 was passed in as a possible package", "CONT2", instruction.PackageDivots[0].PackageID);
		}

		public void TestParentContainerLinksAndAddresses_WhenBothContainersMatchInstructionAddress_ForPickupInstruction()
		{
			var booking = GetNewBooking();
			var org = Helper.CreateOrganisation("AAA");
			var instruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CYD, org.Addresses.MainAddress);
			var container1 = Helper.CreatePackage("CONT1", 1, "CNT");
			var container2 = Helper.CreatePackage("CONT2", 1, "CNT");
			var container3 = Helper.CreatePackage("CONT3", 1, "CNT");
			instruction.OrganisationType = OrganisationTypesList.Codes.CYD;
			instruction.PackageCategory = PackageCategories.Codes.Containers;
			var parentContainerLinksAndAddresses = new List<(ZInt? link, List<(ZString? addressCode, ZString? addressType)>, bool shouldFallbackToShipmentContainerYard)>
			{
				(1, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: AddressTypes.ContainerYardEmptyPickupAddress) }, shouldFallbackToShipmentContainerYard: false ),
				(2, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: AddressTypes.ContainerYardEmptyPickupAddress) }, shouldFallbackToShipmentContainerYard: false ),
				(3, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: AddressTypes.ContainerYardEmptyReturnAddress) }, shouldFallbackToShipmentContainerYard: false )
			};

			booking.ParentContainerLinksAndAddresses = parentContainerLinksAndAddresses;
			booking.PackageContainerLinks = new Dictionary<ZInt, PkgPackage>() {
				{ 1, container1 },
				{ 2, container2 },
				{ 3, container3 }
			};

			instruction.DefaultPackages(new[] { container1, container2 });
			AssertEquals("Expected packages attached to instruction", 2, instruction.PackageDivots.Count);
			AssertEquals("container1 was passed in as a possible package with matching address details", true, instruction.PackageDivots.ToList().Select(p => p.PackageID).Contains("CONT1"));
			AssertEquals("container2 was passed in as a possible package with matching address details", true, instruction.PackageDivots.ToList().Select(p => p.PackageID).Contains("CONT2"));
		}

		public void TestParentContainerLinksAndAddresses_WhenFirstContainerMatchesInstructionAddress_ForDeliveryInstruction()
		{
			var booking = GetNewBooking();
			var org = Helper.CreateOrganisation("AAA");
			Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CYD, org.Addresses.MainAddress);
			var instruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CYD, org.Addresses.MainAddress);
			var container1 = Helper.CreatePackage("CONT1", 1, "CNT");
			var container2 = Helper.CreatePackage("CONT2", 1, "CNT");
			var container3 = Helper.CreatePackage("CONT3", 1, "CNT");
			instruction.OrganisationType = OrganisationTypesList.Codes.CYD;
			instruction.PackageCategory = PackageCategories.Codes.Containers;
			var parentContainerLinksAndAddresses = new List<(ZInt? link, List<(ZString? addressCode, ZString? addressType)>, bool shouldFallbackToShipmentContainerYard)>
			{
				(1, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: AddressTypes.ContainerYardEmptyReturnAddress) }, shouldFallbackToShipmentContainerYard: false ),
				(2, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: "XXXX", addressType: AddressTypes.ContainerYardEmptyReturnAddress) }, shouldFallbackToShipmentContainerYard: false ),
				(3, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: AddressTypes.ContainerYardEmptyPickupAddress) }, shouldFallbackToShipmentContainerYard: false )
			};

			booking.ParentContainerLinksAndAddresses = parentContainerLinksAndAddresses;
			booking.PackageContainerLinks = new Dictionary<ZInt, PkgPackage>() {
				{ 1, container1 },
				{ 2, container2 },
				{ 3, container3 }
			};

			instruction.DefaultPackages(new[] { container1, container2, container3 });
			AssertEquals("Expected packages attached to instruction", 1, instruction.PackageDivots.Count);
			AssertEquals("Only container1 has correct address code and type", "CONT1", instruction.PackageDivots[0].PackageID);
		}

		public void TestParentContainerLinksAndAddresses_WhenBothContainersMatchInstructionAddressButOnlySecondContainerIsPassedIn_ForDeliveryInstruction()
		{
			var booking = GetNewBooking();
			var org = Helper.CreateOrganisation("AAA");
			Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CYD, org.Addresses.MainAddress);
			var instruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CYD, org.Addresses.MainAddress);
			var container1 = Helper.CreatePackage("CONT1", 1, "CNT");
			var container2 = Helper.CreatePackage("CONT2", 1, "CNT");
			var container3 = Helper.CreatePackage("CONT3", 1, "CNT");
			instruction.OrganisationType = OrganisationTypesList.Codes.CYD;
			instruction.PackageCategory = PackageCategories.Codes.Containers;
			var parentContainerLinksAndAddresses = new List<(ZInt? link, List<(ZString? addressCode, ZString? addressType)>, bool shouldFallbackToShipmentContainerYard)>
			{
				(1, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: AddressTypes.ContainerYardEmptyReturnAddress) }, shouldFallbackToShipmentContainerYard: false ),
				(2, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: AddressTypes.ContainerYardEmptyReturnAddress) }, shouldFallbackToShipmentContainerYard: false ),
				(3, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: AddressTypes.ContainerYardEmptyPickupAddress) }, shouldFallbackToShipmentContainerYard: false )
			};

			booking.ParentContainerLinksAndAddresses = parentContainerLinksAndAddresses;
			booking.PackageContainerLinks = new Dictionary<ZInt, PkgPackage>() {
				{ 1, container1 },
				{ 2, container2 },
				{ 3, container3 }
			};

			instruction.DefaultPackages(new[] { container2 });
			AssertEquals("Expected packages attached to instruction", 1, instruction.PackageDivots.Count);
			AssertEquals("Only container2 was passed in as a possible package", "CONT2", instruction.PackageDivots[0].PackageID);
		}

		public void TestParentContainerLinksAndAddresses_WhenBothContainersMatchInstructionAddress_ForDeliveryInstruction()
		{
			var booking = GetNewBooking();
			var org = Helper.CreateOrganisation("AAA");
			Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CYD, org.Addresses.MainAddress);
			var instruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CYD, org.Addresses.MainAddress);
			var container1 = Helper.CreatePackage("CONT1", 1, "CNT");
			var container2 = Helper.CreatePackage("CONT2", 1, "CNT");
			var container3 = Helper.CreatePackage("CONT3", 1, "CNT");
			instruction.OrganisationType = OrganisationTypesList.Codes.CYD;
			instruction.PackageCategory = PackageCategories.Codes.Containers;
			var parentContainerLinksAndAddresses = new List<(ZInt? link, List<(ZString? addressCode, ZString? addressType)>, bool shouldFallbackToShipmentContainerYard)>
			{
				(1, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: AddressTypes.ContainerYardEmptyReturnAddress) }, shouldFallbackToShipmentContainerYard: false ),
				(2, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: AddressTypes.ContainerYardEmptyReturnAddress) }, shouldFallbackToShipmentContainerYard: false ),
				(3, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: AddressTypes.ContainerYardEmptyPickupAddress) }, shouldFallbackToShipmentContainerYard: false )
			};

			booking.ParentContainerLinksAndAddresses = parentContainerLinksAndAddresses;
			booking.PackageContainerLinks = new Dictionary<ZInt, PkgPackage>() {
				{ 1, container1 },
				{ 2, container2 },
				{ 3, container3 }
			};

			instruction.DefaultPackages(new[] { container1, container2 });
			AssertEquals("Expected packages attached to instruction", 2, instruction.PackageDivots.Count);
			AssertEquals("container1 was passed in as a possible package with matching address details", true, instruction.PackageDivots.ToList().Select(p => p.PackageID).Contains("CONT1"));
			AssertEquals("container2 was passed in as a possible package with matching address details", true, instruction.PackageDivots.ToList().Select(p => p.PackageID).Contains("CONT2"));
		}

		public void TestParentContainerLinksAndAddresses_WhenShouldFallbackToShipmentContainerYardIsTrue_AndCorrectAddressTypeExists()
		{
			var booking = GetNewBooking();
			var org = Helper.CreateOrganisation("AAA");
			var instruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CYD, org.Addresses.MainAddress);
			var container1 = Helper.CreatePackage("CONT1", 1, "CNT");
			var container2 = Helper.CreatePackage("CONT2", 1, "CNT");
			var container3 = Helper.CreatePackage("CONT3", 1, "CNT");
			instruction.OrganisationType = OrganisationTypesList.Codes.CYD;
			instruction.PackageCategory = PackageCategories.Codes.Containers;
			var parentContainerLinksAndAddresses = new List<(ZInt? link, List<(ZString? addressCode, ZString? addressType)>, bool shouldFallbackToShipmentContainerYard)>
			{
				(1, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: (ZString?)nameof(DocAddressType.CustomsContainerYardAddress)) }, shouldFallbackToShipmentContainerYard: true ),
				(2, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: AddressTypes.ContainerYardEmptyPickupAddress) }, shouldFallbackToShipmentContainerYard: false ),
				(3, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: AddressTypes.ContainerYardEmptyReturnAddress) }, shouldFallbackToShipmentContainerYard: true )
			};

			booking.ParentContainerLinksAndAddresses = parentContainerLinksAndAddresses;
			booking.PackageContainerLinks = new Dictionary<ZInt, PkgPackage>() {
				{ 1, container1 },
				{ 2, container2 },
				{ 3, container3 }
			};

			instruction.DefaultPackages(new[] { container1, container2, container3 });
			AssertEquals("Expected packages attached to instruction", 2, instruction.PackageDivots.Count);
			AssertEquals("container1 has a matching shipment address and shouldFallbackToShipmentContainerYard is true", "CONT1", instruction.PackageDivots[0].PackageID);
			AssertEquals("container2 has a matching address", "CONT2", instruction.PackageDivots[1].PackageID);
		}

		public void TestParentContainerLinksAndAddresses_WhenShouldFallbackToShipmentContainerYardIsTrue_AndCorrectAddressTypeDoesNotExist()
		{
			var booking = GetNewBooking();
			var org = Helper.CreateOrganisation("AAA");
			var instruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CYD, org.Addresses.MainAddress);
			var container1 = Helper.CreatePackage("CONT1", 1, "CNT");
			var container2 = Helper.CreatePackage("CONT2", 1, "CNT");
			var container3 = Helper.CreatePackage("CONT3", 1, "CNT");
			instruction.OrganisationType = OrganisationTypesList.Codes.CYD;
			instruction.PackageCategory = PackageCategories.Codes.Containers;
			var parentContainerLinksAndAddresses = new List<(ZInt? link, List<(ZString? addressCode, ZString? addressType)>, bool shouldFallbackToShipmentContainerYard)>
			{
				(1, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: AddressTypes.ContainerYardEmptyPickupAddress) }, shouldFallbackToShipmentContainerYard: true ),
				(2, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: AddressTypes.ContainerYardEmptyPickupAddress) }, shouldFallbackToShipmentContainerYard: false ),
				(3, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: AddressTypes.ContainerYardEmptyReturnAddress) }, shouldFallbackToShipmentContainerYard: true )
			};

			booking.ParentContainerLinksAndAddresses = parentContainerLinksAndAddresses;
			booking.PackageContainerLinks = new Dictionary<ZInt, PkgPackage>() {
				{ 1, container1 },
				{ 2, container2 },
				{ 3, container3 }
			};

			instruction.DefaultPackages(new[] { container1, container2, container3 });
			AssertEquals("Expected packages attached to instruction", 1, instruction.PackageDivots.Count);
			AssertEquals("Only container2 has a matching address and no fallback to shipment", "CONT2", instruction.PackageDivots[0].PackageID);
		}

		public void TestParentContainerLinksAndAddresses_ShouldMatchBlankAddress()
		{
			var booking = GetNewBooking();
			var instruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CYD, null);
			var container1 = Helper.CreatePackage("CONT1", 1, "CNT");
			var container2 = Helper.CreatePackage("CONT2", 1, "CNT");
			var container3 = Helper.CreatePackage("CONT3", 1, "CNT");
			instruction.OrganisationType = OrganisationTypesList.Codes.CYD;
			instruction.PackageCategory = PackageCategories.Codes.Containers;
			var parentContainerLinksAndAddresses = new List<(ZInt? link, List<(ZString? addressCode, ZString? addressType)>, bool shouldFallbackToShipmentContainerYard)>
			{
				(1, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: null, addressType: AddressTypes.ContainerYardEmptyPickupAddress) }, shouldFallbackToShipmentContainerYard: false ),
				(2, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: "XXXX", addressType: AddressTypes.ContainerYardEmptyPickupAddress) }, shouldFallbackToShipmentContainerYard: true ),
				(3, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: null, addressType: AddressTypes.ContainerYardEmptyReturnAddress) }, shouldFallbackToShipmentContainerYard: false )
			};

			booking.ParentContainerLinksAndAddresses = parentContainerLinksAndAddresses;
			booking.PackageContainerLinks = new Dictionary<ZInt, PkgPackage>() {
				{ 1, container1 },
				{ 2, container2 },
				{ 3, container3 }
			};

			instruction.DefaultPackages(new[] { container1, container2, container3 });
			AssertEquals("Expected packages attached to instruction", 1, instruction.PackageDivots.Count);
			AssertEquals("Only container1 matches the instruction's address code and type", "CONT1", instruction.PackageDivots[0].PackageID);
		}

		public void TestParentIsHazardousWhenHazardousPackageAssignedToInstruction()
		{
			var booking = Helper.CreateBooking();
			var instruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CYD, null);
			var container1 = Helper.CreatePackage("CONT1", 1, "CNT");
			container1.KP_KJ_ParentPackageJob = booking.PackageJob.PK;
			container1.UNDGs.AddNew();

			instruction.DivotsWithPackages.AddPackage(container1);

			AssertEquals("Parent booking should be marked as hazardous", true, booking.KM_IsHazardous);
		}

		public void TestParentIsHazardousWhenHazardousPackageUnassignedFromInstruction()
		{
			var booking = Helper.CreateBooking();
			booking.KM_IsHazardous = true;
			var instruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CYD, null);
			var container1 = Helper.CreatePackage("CONT1", 1, "CNT");
			container1.KP_KJ_ParentPackageJob = booking.PackageJob.PK;

			instruction.DivotsWithPackages.AddPackage(container1);

			AssertEquals("Parent booking should not be marked as hazardous", false, booking.KM_IsHazardous);
		}

		public void TestParentIsRefrigerationRequiredWhenRefrigeratedPackageAssignedToInstruction()
		{
			var booking = Helper.CreateBooking();
			var instruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CYD, null);
			var container1 = Helper.CreatePackage("CONT1", 1, "CNT");
			container1.KP_KJ_ParentPackageJob = booking.PackageJob.PK;
			container1.KP_RequiresTemperatureControl = true;
			container1.Container.K0_IsControlledAtmosphere = true;

			instruction.DivotsWithPackages.AddPackage(container1);

			AssertEquals("Parent booking should be marked as needing refrigeration", true, booking.KM_RequiresRefrigeration);
		}

		public void TestParentIsRefrigerationRequiredWhenRefrigeratedPackageUnassignedFromInstruction()
		{
			var booking = Helper.CreateBooking();
			booking.KM_RequiresRefrigeration = true;
			var instruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CYD, null);
			var container1 = Helper.CreatePackage("CONT1", 1, "CNT");
			container1.KP_KJ_ParentPackageJob = booking.PackageJob.PK;

			instruction.DivotsWithPackages.AddPackage(container1);

			AssertEquals("Parent booking should not be marked as needing refrigeration", false, booking.KM_RequiresRefrigeration);
		}

		public void TestPackageContainerLinks_WhenMultipleContainersExistWithSameID()
		{
			var booking = GetNewBooking();
			var org = Helper.CreateOrganisation("AAA");
			var instruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CYD, org.Addresses.MainAddress);
			var container1 = Helper.CreatePackage("CONT", 1, "CNT");
			var container2 = Helper.CreatePackage("CONT", 1, "CNT");
			var container3 = Helper.CreatePackage("CONT", 1, "CNT");
			instruction.OrganisationType = OrganisationTypesList.Codes.CYD;
			instruction.PackageCategory = PackageCategories.Codes.Containers;
			var parentContainerLinksAndAddresses = new List<(ZInt? link, List<(ZString? addressCode, ZString? addressType)>, bool shouldFallbackToShipmentContainerYard)>
			{
				(1, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: AddressTypes.ContainerYardEmptyPickupAddress) }, shouldFallbackToShipmentContainerYard: false ),
				(2, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: (ZString?)nameof(DocAddressType.CustomsContainerYardAddress)) }, shouldFallbackToShipmentContainerYard: true ),
				(3, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: AddressTypes.ContainerYardEmptyReturnAddress) }, shouldFallbackToShipmentContainerYard: false )
			};
			booking.ParentContainerLinksAndAddresses = parentContainerLinksAndAddresses;

			booking.PackageContainerLinks = new Dictionary<ZInt, PkgPackage>
			{
				{ 1, container1 },
				{ 2, container2 },
				{ 3, container3 }
			};

			instruction.DefaultPackages(new[] { container1, container2, container3 });
			AssertEquals("Expected packages attached to instruction", 2, instruction.PackageDivots.Count);
			AssertContainsExactElementsInAnyOrder("container3 has the wrong address type, so should not be attached.", new List<string> { container1.PK.ToString(), container2.PK.ToString() }, instruction.PackageDivots.ToList().Select(d => d.Package.PK.ToString()));
		}

		void AssertPackages(DtbBookingInstruction instruction, string category, PkgPackage[] possiblePackages, PkgPackage[] expected)
		{
			Array.ForEach(instruction.DivotsWithPackages.Packages.ToArray(), p => instruction.DivotsWithPackages.RemovePackage(p));
			instruction.PackageCategory = category;
			instruction.DefaultPackages(possiblePackages);
			AssertContainsExactElementsInAnyOrder(instruction.DivotsWithPackages.Packages, expected);
		}

		void AssertPackagesWithoutFallback(DtbBookingInstruction instruction, string category, PkgPackage[] possiblePackages, PkgPackage[] expected)
		{
			Array.ForEach(instruction.DivotsWithPackages.Packages.ToArray(), p => instruction.DivotsWithPackages.RemovePackage(p));
			instruction.PackageCategory = category;
			instruction.DefaultPackagesWithoutFallback(possiblePackages);
			AssertContainsExactElementsInAnyOrder(instruction.DivotsWithPackages.Packages, expected);
		}

		public void TestDefaultPackages_WhenSuspendSettingPackages()
		{
			var booking = GetNewBooking();
			var org = Helper.CreateOrganisation("AAA");
			var instruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNE, org.Addresses.MainAddress);
			var instructionToGetPkgFrom = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, org.Addresses.MainAddress);
			var container = Helper.CreatePackage("CONT", 1, "CNT");
			var containerInnerPackage = Helper.CreatePackage("CONT_IN", 1);
			var containerInnerInnerPackage = Helper.CreatePackage("CONT_IN_IN", 1);
			var container2 = Helper.CreatePackage("CONT2", 1, "CNT");
			var container2InnerPackage = Helper.CreatePackage("CONT2_IN", 1);
			var container2InnerInnerPackage = Helper.CreatePackage("CONT2_IN_IN", 1);
			var container3 = Helper.CreatePackage("CONT3", 1, "CNT");
			var topLevelPackage = Helper.CreatePackage("TOP", 1);
			var topInnerPackage = Helper.CreatePackage("TOP_IN", 1);

			// Package tree
			booking.PackageJob.Packages.Add(container);
			booking.PackageJob.Packages.Add(container2);
			booking.PackageJob.Packages.Add(container3);
			booking.PackageJob.Packages.Add(topLevelPackage);
			container.Packages.Add(containerInnerPackage);
			containerInnerPackage.Packages.Add(containerInnerInnerPackage);
			container2.Packages.Add(container2InnerPackage);
			container2InnerPackage.Packages.Add(container2InnerInnerPackage);
			topLevelPackage.Packages.Add(topInnerPackage);

			AssertPackages_WhenSuspendSettingPackages(instruction, PackageCategories.Codes.Containers, new[] { container, topLevelPackage });
			AssertPackages_WhenSuspendSettingPackages(instruction, PackageCategories.Codes.Containers, new[] { container });
			AssertPackages_WhenSuspendSettingPackages(instruction, PackageCategories.Codes.Containers, new[] { container, topLevelPackage });
			AssertPackages_WhenSuspendSettingPackages(instruction, PackageCategories.Codes.Containers, new[] { topLevelPackage });
			AssertPackages_WhenSuspendSettingPackages(instruction, PackageCategories.Codes.Outers, new[] { topLevelPackage });
			AssertPackages_WhenSuspendSettingPackages(instruction, PackageCategories.Codes.Outers, new[] { container, topLevelPackage });
			AssertPackages_WhenSuspendSettingPackages(instruction, PackageCategories.Codes.Outers, new[] { container });
			AssertPackages_WhenSuspendSettingPackages(instruction, PackageCategories.Codes.Both, new[] { topLevelPackage });
			AssertPackages_WhenSuspendSettingPackages(instruction, PackageCategories.Codes.Both, new[] { container, topLevelPackage });
			AssertPackages_WhenSuspendSettingPackages(instruction, PackageCategories.Codes.Both, new[] { container });
			AssertPackages_WhenSuspendSettingPackages(instruction, PackageCategories.Codes.Loose, new[] { topLevelPackage });
			AssertPackages_WhenSuspendSettingPackages(instruction, PackageCategories.Codes.Loose, new[] { container, topLevelPackage });
			AssertPackages_WhenSuspendSettingPackages(instruction, PackageCategories.Codes.Loose, new[] { container });
			AssertPackages_WhenSuspendSettingPackages(instruction, PackageCategories.Codes.Loose, new[] { container3 });
		}

		void AssertPackages_WhenSuspendSettingPackages(DtbBookingInstruction instruction, string category, PkgPackage[] possiblePackages)
		{
			instruction.PackageCategory = category;
			Array.ForEach(instruction.DivotsWithPackages.Packages.ToArray(), p => instruction.DivotsWithPackages.RemovePackage(p));
			AssertEquals(0, instruction.DivotsWithPackages.Count);
			using (instruction.Booking.SuspendSettingPackages())
			{
				instruction.DefaultPackages(possiblePackages);
				AssertEquals(0, instruction.DivotsWithPackages.Count);
			}
		}

		public void TestDefaultPackageCategoryIfEmptyWithMatchingTemplateWithNonBlankPackageType()
		{
			var booking = GetNewBooking();
			booking.KM_KT_NKBookingTemplate = "EFPL";
			Helper.AssertEFPLTemplateHasFourInstructionsWithSpecificPackageCategories(isPreConditionAssert: true);
			AssertEquals("Precondition: Should now have four instructions from template", 4, booking.Instructions.Count);
			var instruction = booking.Instructions.FirstOrDefault(i => i.KN_Sequence == 1);
			instruction.PackageCategory = string.Empty;

			instruction.DefaultPackageCategoryIfEmpty();
			AssertEquals("After call to insruction.DefaultPackageCategoryIfEmpty(), should have re-set instruction PackageCategory to the package type of the corresponding template instruction", PackageCategories.Codes.Loose, instruction.PackageCategory);
		}

		public void TestDefaultPackageCategoryIfEmptyWithMatchingTemplateWithBlankPackageType()
		{
			var templateCode = "XYZ1";
			CreateBookingTemplateWithBlankPackageCategoryInstructions(templateCode);
			var booking = GetNewBooking();
			booking.KM_KT_NKBookingTemplate = templateCode;
			AssertEquals("Precondition: Should now have two instructions from template", 2, booking.Instructions.Count);
			var instruction = booking.Instructions.FirstOrDefault(i => i.KN_Sequence == 1);
			instruction.PackageCategory = string.Empty;

			instruction.DefaultPackageCategoryIfEmpty();
			AssertEquals("After call to insruction.DefaultPackageCategoryIfEmpty(), should have re-set instruction PackageCategory to 'OUT'", PackageCategories.Codes.Outers, instruction.PackageCategory);
		}

		public void TestDefaultPackageCategoryIfEmptyWithNoMatchingTemplate()
		{
			var templateCode = "EFPL";
			var booking = GetNewBooking();
			booking.KM_KT_NKBookingTemplate = templateCode;

			var templateQuery = new ZQuery(DtbBookingTmplSchema.KT_Code, templateCode);
			var template = Factory.LoadTop1<DtbBookingTmpl>(templateQuery);
			Assert("Booking template EFPL does not have any instructions with sequence 5", !template.Instructions.Any(i => i.K2_Sequence == 5));

			var instruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			instruction.KN_Sequence = 5;
			AssertEquals("Precondition: instruction.PackageCategory is empty", ZString.Empty, instruction.PackageCategory);

			instruction.DefaultPackageCategoryIfEmpty();
			AssertEquals("After call to insruction.DefaultPackageCategoryIfEmpty(), should have re-set instruction PackageCategory to the 'OUT'", PackageCategories.Codes.Outers, instruction.PackageCategory);
		}

		public void TestDefaultPackageCategoryIfEmptyWithNoTemplate()
		{
			var booking = GetNewBooking();
			var instruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			instruction.KN_Sequence = 1;
			AssertEquals("Precondition: instruction.PackageCategory is empty", ZString.Empty, instruction.PackageCategory);

			instruction.DefaultPackageCategoryIfEmpty();
			AssertEquals("After call to insruction.DefaultPackageCategoryIfEmpty(), should have re-set instruction PackageCategory to the 'OUT'", PackageCategories.Codes.Outers, instruction.PackageCategory);
		}

		public void TestDefaultPackageCategoryIfEmptyDoesNotUpdatePackageCategoryIfNotEmpty()
		{
			var booking = GetNewBooking();
			booking.KM_KT_NKBookingTemplate = "EFPL";
			Helper.AssertEFPLTemplateHasFourInstructionsWithSpecificPackageCategories(isPreConditionAssert: true);
			AssertEquals("Precondition: Should now have four instructions from template", 4, booking.Instructions.Count);
			var instruction = booking.Instructions.FirstOrDefault(i => i.KN_Sequence == 1);
			AssertEquals(FormattableString.Invariant($"Precondition: instruction has PackageCategory '{PackageCategories.Codes.Loose}' from template"), PackageCategories.Codes.Loose, instruction.PackageCategory);
			instruction.PackageCategory = PackageCategories.Codes.Containers;

			instruction.DefaultPackageCategoryIfEmpty();
			AssertEquals("After call to insruction.DefaultPackageCategoryIfEmpty(), should have left instruction PackageCategory as it was before the call", PackageCategories.Codes.Containers, instruction.PackageCategory);
		}

		void CreateBookingTemplateWithBlankPackageCategoryInstructions(ZString bookingTemplateCode)
		{
			var bookingTemplate = Helper.CreateTransportBookingTemplate(bookingTemplateCode, "Test Template with Blank Package Category Instructions", "ORG");

			var bookingTemplateInstruction1 = bookingTemplate.Instructions.AddNew();
			bookingTemplateInstruction1.K2_InstructionType = "PIC";
			bookingTemplateInstruction1.K2_Sequence = 1;

			var bookingTemplateInstruction2 = bookingTemplate.Instructions.AddNew();
			bookingTemplateInstruction2.K2_InstructionType = "DLV";
			bookingTemplateInstruction2.K2_Sequence = 2;

			Factory.Save();
		}

		public void TestDepotForAddress()
		{
			var transportJob = GetNewBooking();

			// instruction
			var instruction = Helper.CreateInstruction(transportJob, InstructionTypes.Codes.PickUp);
			var zoneA = Helper.CreateZone("Zone A");
			instruction.KN_TZ_DomesticZone = zoneA.PK;
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.Addresses.AddNew();
			address.FillWithValidTestData();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;

			// PortHubSelection
			var portHubSelection1 = Factory.NewWithValidTestData<PortHubSelection>();
			portHubSelection1.TY_OA_DepotAddress = address.PK;
			portHubSelection1.TY_Direction = "PIC";
			portHubSelection1.TY_RatingFreightMode = "ALL";

			var portHubZonePivot1 = Factory.New<PortHubZonePivot>();
			portHubZonePivot1.TX_TY_Hub = portHubSelection1.PK;
			portHubZonePivot1.TX_TZ_Zone = zoneA.PK;

			Factory.Save();
			AssertNull(instruction.DepotForAddress);

			branch.GB_OH_OrgProxy = orgHeader.PK;
			orgHeader.CompanyData.OB_GB_ControllingBranch = ZGuid.Empty;
			Factory.Save();
			AssertEquals("Checks for branch with org proxy first", branch, instruction.DepotForAddress);

			branch.GB_OH_OrgProxy = ZGuid.Empty;
			orgHeader.CompanyData.OB_GB_ControllingBranch = branch.PK;
			Factory.Save();
			AssertEquals("Then checks for controlling branch on depot org", branch, instruction.DepotForAddress);

			branch2.GB_OH_OrgProxy = orgHeader.PK;
			orgHeader.CompanyData.OB_GB_ControllingBranch = branch.PK;
			Factory.Save();
			AssertEquals("Priority is correct", branch2, instruction.DepotForAddress);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestFindDepotAddressAndCutOffTime()
		{
			var organisation = Helper.CreateOrganisation("ORGA");
			var pickupOrganisationAddress = organisation.MainAddress;
			organisation.MainAddress.OA_PostCode = "2001";
			organisation.MainAddress.OA_RL_NKRelatedPortCode = (Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"))).RL_Code;

			var zone = Helper.CreateZone("ZONE");
			Helper.AddPostCodesToZone(zone, "2001", "3000");

			var depotOrganisation = Helper.CreateOrganisation("DEP");
			var portHubSellection = Helper.CreatePortHub(depotOrganisation.MainAddress);
			Helper.SetPortHubDirection(portHubSellection, "PIC");
			var pivot = Helper.CreatePortHubZonePivot(portHubSellection, zone);
			pivot.TX_PickupCutOffTimeVariance = 60;

			Factory.Save();

			var transportJob = GetNewBooking();
			var instruction = Helper.CreateInstruction(transportJob, InstructionTypes.Codes.PickUp);
			instruction.Address.OrganisationPK = organisation.PK;

			AssertEquals("12:00", instruction.PortHubZonePivotPickupCutOffTime.ToString("HH:mm"));
			AssertEquals(depotOrganisation.MainAddress.PK, instruction.FindDepotAddress().PK);
		}

		public void TestAddress()
		{
			var booking = (DtbBooking)Factory.New(ExpectedTransportType);
			var instruction = booking.Instructions.AddNew();
			AssertNotNull(instruction.Address);
			AssertEquals(DocAddressType.None, instruction.Address.DocAddressType);

			foreach (CodeDescriptionPair pair in OrganisationTypesList.Instance)
			{
				AssertAddressMatchedOrganisationType(instruction, pair.Code);
			}

			var organisation = Helper.CreateOrganisation("QWESYD");
			var pickupAddress = Helper.AddAddressToOrganisation(organisation, "Pickup Addy", OrgAddressType.Pickup);
			var officeAddress = Helper.AddAddressToOrganisation(organisation, "Office Addy", OrgAddressType.Office);
			var deliveryAddress = Helper.AddAddressToOrganisation(organisation, "Delivery Addy", OrgAddressType.Delivery);

			instruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			instruction.Address.OrganisationPK = organisation.PK;
			AssertEquals("Should default to pickupAddress", pickupAddress.PK, instruction.Address.E2_OA_Address);

			using (booking.GetValidationSuspender()) // not valid to have no Pickup in consignments
			{
				booking.KM_Direction = Constants.CartageDirection.Origin;
				instruction.KN_InstructionType = InstructionTypes.Codes.Multi;
				instruction.Address.OrganisationPK = ZGuid.Empty;
				instruction.Address.OrganisationPK = organisation.PK;
				AssertEquals("Should default to pickupAddress", pickupAddress.PK, instruction.Address.E2_OA_Address);

				instruction.KN_InstructionType = InstructionTypes.Codes.Delivery;
				instruction.Address.OrganisationPK = ZGuid.Empty;
				instruction.Address.OrganisationPK = organisation.PK;
				AssertEquals("Should default to deliveryAddress", deliveryAddress.PK, instruction.Address.E2_OA_Address);

				booking.KM_Direction = Constants.CartageDirection.Destination;
				instruction.KN_InstructionType = InstructionTypes.Codes.Multi;
				instruction.Address.OrganisationPK = ZGuid.Empty;
				instruction.Address.OrganisationPK = organisation.PK;
				AssertEquals("Should default to deliveryAddress", deliveryAddress.PK, instruction.Address.E2_OA_Address);
			}
		}

		void AssertAddressMatchedOrganisationType(DtbBookingInstruction instruction, ZString orgType)
		{
			var docAddressType = CartageOrgTypeAddressTypeConverter.GetDocAddressTypeFromOrgType(orgType);
			AssertNotEquals("only a valid orgType should be passed in", DocAddressType.None, docAddressType);

			instruction.OrganisationType = orgType;
			AssertEquals(docAddressType, instruction.Address.DocAddressType);
		}

		public void TestBooking()
		{
			var booking = (DtbBooking)Factory.New(ExpectedTransportType);
			var instruction = (DtbBookingInstruction)GetNewBusinessObject();
			instruction.KN_KM_BookingMovement = booking.PK;

			AssertEquals(booking, instruction.Booking);
			AssertEquals(ExpectedTransportType, instruction.Booking.GetType());
		}

		Type ExpectedTransportType
		{
			get { return typeof(DtbBooking); }
		}

		public void TestUpdateInstructionInfoOnlyUpdateConfirmationsWithSameType()
		{
			var booking = GetNewBooking();
			var instruction = booking.Instructions.AddNew();
			instruction.KN_InstructionType = InstructionTypes.Codes.Multi;

			var pickUpConfirmation = instruction.Confirmations.Cast<DtbBookingConfirmation>().ToArray().FirstOrDefault(c => c.IsPickUp);
			var deliveryConfirmation = instruction.Confirmations.Cast<DtbBookingConfirmation>().ToArray().FirstOrDefault(c => c.IsDelivery);
			pickUpConfirmation = pickUpConfirmation ?? instruction.Confirmations.AddNew();
			deliveryConfirmation = deliveryConfirmation ?? instruction.Confirmations.AddNew();
			var slotConfirmation = instruction.Confirmations.AddNew();

			pickUpConfirmation.KK_ConfirmationType = ConfirmationTypes.Codes.PickUp;
			deliveryConfirmation.KK_ConfirmationType = ConfirmationTypes.Codes.Delivery;

			AssertEquals("Confirmations with no date.", ZDateTime.Empty, instruction.ReqFrom);

			pickUpConfirmation.KK_RequiredFrom = ZDateTime.Now;
			deliveryConfirmation.KK_RequiredFrom = ZDateTime.Now.AddDays(1);

			AssertEquals("Should take latest date.", deliveryConfirmation.KK_RequiredFrom, instruction.ReqFrom);

			instruction.ReqFrom = ZDateTime.Now.AddDays(2);

			AssertNotEquals("Pickup confirmation should not be updated.", pickUpConfirmation.KK_RequiredFrom, instruction.ReqFrom);
			AssertEquals("Delivery confirmation should be updated.", deliveryConfirmation.KK_RequiredFrom, instruction.ReqFrom);
		}

		public void TestConfirmations()
		{
			var instruction = (DtbBookingInstruction)GetNewBusinessObject();
			instruction.Confirmations.AddNew();
			AssertEquals(typeof(DtbBookingConfirmationCollection), instruction.Confirmations.GetType());
			AssertEquals(true, instruction.IsRegisteredEditableChildObject(instruction.Confirmations));
		}

		public void TestDivotsWithPackages()
		{
			var instruction = (DtbBookingInstruction)GetNewBusinessObject();
			AssertEquals(typeof(DivotsWithPackagesCollection), instruction.DivotsWithPackages.GetType());

			var package = Factory.New<PkgPackage>();
			var divot = instruction.PackageDivots.AddNew();
			divot.KD_KP_Package = package.PK;
			AssertContainsExactElementsInAnyOrder(new[] { package }, instruction.DivotsWithPackages.Packages);
		}

		public void TestPackageDivots()
		{
			var instruction = (DtbBookingInstruction)GetNewBusinessObject();
			AssertNotNull(instruction.PackageDivots);
			AssertEquals(typeof(DtbBookingInstructionPkgDivotCollection), instruction.PackageDivots.GetType());

			instruction.PackageDivots.AddNew();
			AssertEquals(true, instruction.IsRegisteredEditableChildObject(instruction.PackageDivots));
		}

		public void TestDescription()
		{
			var booking = GetNewBooking();
			var pickupInstruction = booking.Instructions.AddNew();
			var deliveryInstruction = booking.Instructions.AddNew();
			AssertEquals("??? @ ??? - ???", deliveryInstruction.Description);

			pickupInstruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			deliveryInstruction.KN_InstructionType = InstructionTypes.Codes.Delivery;
			deliveryInstruction.Address.E2_OA_Address = Helper.CreateOrganisation("ABCSYD").MainAddress.PK;
			deliveryInstruction.OrganisationType = OrganisationTypesList.Codes.CNE; // Consignments will default org type based on instruction, which delivery is CNR
			AssertEquals("DLV @ CNE - ABCSYD", deliveryInstruction.Description);

			deliveryInstruction.Address.E2_AddressOverride = true;
			AssertEquals("DLV @ CNE - ???", deliveryInstruction.Description);

			deliveryInstruction.Address.E2_CompanyName = "Hello";
			AssertEquals("DLV @ CNE - Hello", deliveryInstruction.Description);

			deliveryInstruction.KN_InstructionType = "";
			AssertEquals("??? @ CNE - Hello", deliveryInstruction.Description);
		}

		public void TestZone()
		{
			var instruction = (DtbBookingInstruction)GetNewBusinessObject();
			AssertNull(instruction.Zone);

			var zone = Helper.CreateZone("Zone1");
			instruction.KN_TZ_DomesticZone = zone.PK;
			AssertEquals(zone, instruction.Zone);
		}

		public void TestOnDocAddressChanged_ZoneTypes()
		{
			var zoneRating = Helper.CreateZone("ZoneRating", RatingConstants.RatingZoneTypes.Rating);
			Helper.AddPostCodesToZone(zoneRating, "1000", "1100");

			var zoneOperations = Helper.CreateZone("ZoneOperations", RatingConstants.RatingZoneTypes.Operations);
			Helper.AddPostCodesToZone(zoneOperations, "1200", "1300");

			var zoneAll = Helper.CreateZone("ZoneAll", RatingConstants.RatingZoneTypes.All);
			Helper.AddPostCodesToZone(zoneAll, "2000", "2100");

			var ratingCityAddress = Helper.CreateOrganisation("City123").MainAddress;
			ratingCityAddress.OA_Address1 = "123 Rating Street";
			ratingCityAddress.OA_City = "RatingCity";
			ratingCityAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			ratingCityAddress.OA_PostCode = "1001";

			var operationCityAddress = Helper.CreateOrganisation("City456").MainAddress;
			operationCityAddress.OA_Address1 = "456 Operation Street";
			operationCityAddress.OA_City = "OperationCity";
			operationCityAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			operationCityAddress.OA_PostCode = "1201";

			var allCityAddress = Helper.CreateOrganisation("City2000").MainAddress;
			allCityAddress.OA_Address1 = "2000 All Street";
			allCityAddress.OA_City = "AllCity";
			allCityAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			allCityAddress.OA_PostCode = "2001";

			Factory.Save();

			var booking = (DtbBooking)Factory.New(ExpectedTransportType);
			var instruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			AssertNull("Precondition", instruction.Zone);

			instruction.Address.E2_OA_Address = ratingCityAddress.PK;
			AssertNull("Should still no zone, cause exclude Rating zone.", instruction.Zone);

			instruction.Address.E2_OA_Address = operationCityAddress.PK;
			AssertEquals("Should match to zoneOperations as it contains the operationCityAddress.", zoneOperations, instruction.Zone);

			instruction.Address.E2_OA_Address = allCityAddress.PK;
			AssertNull("Cannot match to All Zone as a operations zone exists for the same location and will always be prefered", instruction.Zone);

			instruction.Address.E2_OA_Address = ratingCityAddress.PK;
			AssertNull("Should still no zone, cause exclude Rating zone.", instruction.Zone);
		}

		// persistent

		public void TestKN_KM_BookingMovement_UpdatesBookingStatus()
		{
			var booking = GetNewBooking();
			AssertEquals("Precondition", TransportStatuses.Codes.Available, booking.KM_Status);

			var pickUpInstruction = Factory.New<DtbBookingInstruction>();
			pickUpInstruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			pickUpInstruction.KN_Status = TransportStatuses.Codes.PickedUp;

			booking.Instructions.Add(pickUpInstruction);
			AssertEquals(TransportStatuses.Codes.PickedUp, booking.KM_Status);

			// ensure instruction delete updates the booking status
			pickUpInstruction.Delete();
			AssertEquals(TransportStatuses.Codes.Available, booking.KM_Status);
		}

		public void TestKN_DropMode()
		{
			var instruction = Factory.New<DtbBookingInstruction>();
			AssertEquals("", instruction.KN_DropMode);

			instruction.KN_DropMode = "HSL";
			AssertEquals("HSL", instruction.KN_DropMode);

			instruction.KN_DropMode = "ABC";
			AssertEquals("ABC", instruction.KN_DropMode);

			instruction.KN_DropMode = "";
			AssertEquals("", instruction.KN_DropMode);
		}

		public void TestKN_Sequence()
		{
			AssertEquals(true, ((DtbBookingInstruction)GetNewBusinessObject()).KN_SequenceInfo.ReadOnly);
		}

		public void TestKN_Status()
		{
			var booking = GetNewBooking();
			var pickUpInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var deliveryInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);

			// status should always be readonly
			AssertEquals(TransportStatuses.Codes.Available, pickUpInstruction.KN_Status);
			AssertEquals(true, pickUpInstruction.KN_StatusInfo.ReadOnly);

			// changing the instruction status should recalculate the booking status
			pickUpInstruction.KN_Status = TransportStatuses.Codes.PickedUp;
			AssertEquals(TransportStatuses.Codes.PickedUp, booking.KM_Status);

			deliveryInstruction.KN_Status = TransportStatuses.Codes.Delivered;
			AssertEquals(TransportStatuses.Codes.Delivered, booking.KM_Status);
		}

		// calculated

		public void TestAuthorisedToLeave_Log()
		{
			var booking = GetNewBooking();
			var pickupOrganisation = Helper.CreateOrganisation("PICSYD");
			var pickupAddress = Helper.AddAddressToOrganisation(pickupOrganisation, "Pickup Addy", OrgAddressType.Pickup);

			var deliveryOrganisation = Helper.CreateOrganisation("DELSYD");
			var deliveryAddress = Helper.AddAddressToOrganisation(deliveryOrganisation, "Delivery Addy", OrgAddressType.Delivery);

			var pickupInstruction = Helper.CreateInstruction(booking);
			pickupInstruction.Address.OrganisationPK = pickupOrganisation.PK;
			pickupInstruction.OrganisationType = OrganisationTypesList.Codes.CNR;
			AssertEquals("Should default to pickup address", pickupAddress.PK, pickupInstruction.Address.E2_OA_Address);

			var deliveryInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			deliveryInstruction.Address.OrganisationPK = deliveryOrganisation.PK;
			deliveryInstruction.OrganisationType = OrganisationTypesList.Codes.CNE;
			AssertEquals("Should default to delivery address", deliveryAddress.PK, deliveryInstruction.Address.E2_OA_Address);
			AssertEquals("ATL default value is false", false, deliveryInstruction.KN_IsAuthorisedToLeave);

			Factory.Save();
			AssertEquals("Should be no logs, as false is default.", false, deliveryInstruction.Logs.GetAllLogs().Cast<StmALog>().Any(l => l.SL_SE_NKEvent == Events.Authorised.Code));

			deliveryInstruction.KN_IsAuthorisedToLeave = true;
			Factory.Save();

			var message = "SE_Code Log Events in the collection: " + string.Join(",", deliveryInstruction.Logs.GetAllLogs().Cast<StmALog>().Select(l => l.Event.SE_Code));
			AssertEquals(message, 1, deliveryInstruction.Logs.GetAllLogs().Count);
			var authorisedLog = deliveryInstruction.Logs.GetAllLogs().Cast<StmALog>().Single();
			AssertEquals(true, deliveryInstruction.KN_IsAuthorisedToLeave);
			AssertEquals("The log recently added should be ATH", Events.Authorised.Code, authorisedLog.Event.SE_Code);
			AssertEquals("Free Text Reference.", "", authorisedLog.ReferenceFreeText);
			AssertEquals("Event Reference Parameter Type", "Authorised To Leave", authorisedLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type]);

			deliveryInstruction.KN_IsAuthorisedToLeave = false;
			Factory.Save();

			message = "SE_Code Log Events in the collection: " + string.Join(",", deliveryInstruction.Logs.GetAllLogs().Cast<StmALog>().Select(l => l.Event.SE_Code));
			AssertEquals(message, 2, deliveryInstruction.Logs.GetAllLogs().Count);
			var authorisationWithdrawnLog = deliveryInstruction.Logs.GetAllLogs().Where(l => l != authorisedLog).Cast<StmALog>().Single();
			AssertEquals(false, deliveryInstruction.KN_IsAuthorisedToLeave);
			AssertEquals("The log recently added should be ATW", Events.AuthorisationWithdrawn.Code, authorisationWithdrawnLog.Event.SE_Code);
			AssertEquals("Free Text Reference.", "", authorisationWithdrawnLog.ReferenceFreeText);
			AssertEquals("Event Reference Parameter Type", "Authorised To Leave", authorisationWithdrawnLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type]);
		}

		public void TestDefaultAuthorisedToLeave_ConsignorYESAndConsigneeYESAndClientYES()
		{
			CheckDefaultAuthorityToLeave(AuthorityToLeaveOptions.Codes.YES, AuthorityToLeaveOptions.Codes.YES, AuthorityToLeaveOptions.Codes.YES, true);
		}

		public void TestDefaultAuthorisedToLeave_ConsignorYESAndConsigneeYESAndClientNO()
		{
			CheckDefaultAuthorityToLeave(AuthorityToLeaveOptions.Codes.YES, AuthorityToLeaveOptions.Codes.YES, AuthorityToLeaveOptions.Codes.NO, false);
		}

		public void TestDefaultAuthorisedToLeave_ConsignorYESAndConsigneeNOAndClientYES()
		{
			CheckDefaultAuthorityToLeave(AuthorityToLeaveOptions.Codes.YES, AuthorityToLeaveOptions.Codes.NO, AuthorityToLeaveOptions.Codes.YES, false);
		}

		public void TestDefaultAuthorisedToLeave_ConsignorYESAndConsigneeNOAndClientNO()
		{
			CheckDefaultAuthorityToLeave(AuthorityToLeaveOptions.Codes.YES, AuthorityToLeaveOptions.Codes.NO, AuthorityToLeaveOptions.Codes.NO, false);
		}

		public void TestDefaultAuthorisedToLeave_ConsignorNOAndConsigneeYESAndClientYES()
		{
			CheckDefaultAuthorityToLeave(AuthorityToLeaveOptions.Codes.NO, AuthorityToLeaveOptions.Codes.YES, AuthorityToLeaveOptions.Codes.YES, false);
		}

		public void TestDefaultAuthorisedToLeave_ConsignorNOAndConsigneeYESAndClientNO()
		{
			CheckDefaultAuthorityToLeave(AuthorityToLeaveOptions.Codes.NO, AuthorityToLeaveOptions.Codes.YES, AuthorityToLeaveOptions.Codes.NO, false);
		}

		public void TestDefaultAuthorisedToLeave_ConsignorNOAndConsigneeNOAndClientYES()
		{
			CheckDefaultAuthorityToLeave(AuthorityToLeaveOptions.Codes.NO, AuthorityToLeaveOptions.Codes.NO, AuthorityToLeaveOptions.Codes.YES, false);
		}

		public void TestDefaultAuthorisedToLeave_ConsignorNOAndConsigneeNOAndClientNO()
		{
			CheckDefaultAuthorityToLeave(AuthorityToLeaveOptions.Codes.NO, AuthorityToLeaveOptions.Codes.NO, AuthorityToLeaveOptions.Codes.NO, false);
		}

		void CheckDefaultAuthorityToLeave(string consignorATL, string consigneeATL, string clientATL, bool expectedDefault)
		{
			//Setup data
			var data = new ATLTestData(Helper);
			data.SetUpOrganizationTestData();
			data.PickupAddress1.OA_AuthorityToLeave = consignorATL;
			data.DeliveryAddress1.OA_AuthorityToLeave = consigneeATL;
			data.ClientAddress1.OA_AuthorityToLeave = clientATL;
			data.SetUpBookingTestData(GetNewBooking(), GetNewBooking());

			//Run test
			AssertEquals("Precondition: Check expectedDefault is correct", expectedDefault, data.DeliveryInstruction1.DefaultAuthorisedToLeave);
			AssertEquals("Check that ATL has been defaulted to the correct value", expectedDefault, data.DeliveryInstruction1.KN_IsAuthorisedToLeave);
			AssertEquals("Pickup instruction should never be true for ATL", false, data.PickupInstruction1.KN_IsAuthorisedToLeave);
		}

		public void TestAuthorisedToLeave_ResetAfterOrgTypeChanged()
		{
			var booking = GetNewBooking();
			var pickInstruction = booking.Instructions.AddNew();
			var deliveryInstruction = booking.Instructions.AddNew();

			var pickOrganisation = Helper.CreateOrganisation("PICSYD");
			var pickupAddress = Helper.AddAddressToOrganisation(pickOrganisation, "Pickup Addy", OrgAddressType.Pickup);

			var deliveryOrganisation = Helper.CreateOrganisation("DELSYD");
			var deliveryAddress = Helper.AddAddressToOrganisation(deliveryOrganisation, "Delivery Addy", OrgAddressType.Delivery);

			pickInstruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			pickInstruction.Address.OrganisationPK = pickOrganisation.PK;
			pickInstruction.OrganisationType = OrganisationTypesList.Codes.CNR;
			AssertEquals("Should default to pickup address", pickupAddress.PK, pickInstruction.Address.E2_OA_Address);

			deliveryInstruction.KN_InstructionType = InstructionTypes.Codes.Delivery;
			deliveryInstruction.Address.OrganisationPK = deliveryOrganisation.PK;
			deliveryInstruction.OrganisationType = OrganisationTypesList.Codes.CNE;
			AssertEquals("Should default to delivery address", deliveryAddress.PK, deliveryInstruction.Address.E2_OA_Address);

			deliveryInstruction.KN_IsAuthorisedToLeave = true;

			Factory.Save();

			AssertEquals("ATL should be Ture", true, deliveryInstruction.KN_IsAuthorisedToLeave);

			deliveryInstruction.OrganisationType = OrganisationTypesList.Codes.WHS;

			AssertEquals("Should be set back to default(false) since wrong organization type.", false, deliveryInstruction.KN_IsAuthorisedToLeave);
		}

		public class ATLTestData
		{
			public ATLTestData(TransportBookingTestHelper helper)
			{
				this.helper = helper;
			}

			readonly TransportBookingTestHelper helper;

			internal void SetUpOrganizationTestData()
			{
				PickupOrganisation1 = helper.CreateOrganisation("PIC1SYD");
				PickupOrganisation1.OrganisationTypes = OrganisationTypes.Consignor;
				PickupAddress1 = helper.AddAddressToOrganisation(PickupOrganisation1, "Pickup1 Addy", OrgAddressType.Pickup);

				PickupOrganisation2 = helper.CreateOrganisation("PIC2SYD");
				PickupOrganisation2.OrganisationTypes = OrganisationTypes.Consignor;
				PickupAddress2 = helper.AddAddressToOrganisation(PickupOrganisation2, "Pickup2 Addy", OrgAddressType.Pickup);

				DeliveryOrganisation1 = helper.CreateOrganisation("DEL1SYD");
				DeliveryOrganisation1.OrganisationTypes = OrganisationTypes.Consignee;
				DeliveryAddress1 = helper.AddAddressToOrganisation(DeliveryOrganisation1, "Delivery1 Addy", OrgAddressType.Delivery);

				DeliveryOrganisation2 = helper.CreateOrganisation("DEL2SYD");
				DeliveryOrganisation2.OrganisationTypes = OrganisationTypes.Consignee;
				DeliveryAddress2 = helper.AddAddressToOrganisation(DeliveryOrganisation2, "Delivery2 Addy", OrgAddressType.Delivery);

				PickupOrganizationLink1 = PickupOrganisation1.BuyerLinks.AddNew(DeliveryOrganisation1);
				PickupOrganizationLink2 = PickupOrganisation2.BuyerLinks.AddNew(DeliveryOrganisation2);
				DeliveryOrganisation1.SupplierLinks.Add(PickupOrganizationLink1);
				DeliveryOrganisation2.SupplierLinks.Add(PickupOrganizationLink2);

				ClientOrganisation1 = helper.CreateOrganisation("CLI1SYD");
				ClientOrganisation1.OrganisationTypes = OrganisationTypes.TransportClient;
				ClientAddress1 = helper.AddAddressToOrganisation(ClientOrganisation1, "Client1 Addy", OrgAddressType.Miscellaneous);

				ClientOrganisation2 = helper.CreateOrganisation("CLI2SYD");
				ClientOrganisation2.OrganisationTypes = OrganisationTypes.TransportClient;
				ClientAddress2 = helper.AddAddressToOrganisation(ClientOrganisation2, "Client2 Addy", OrgAddressType.Miscellaneous);
			}

			internal void SetUpBookingTestData(DtbBooking booking1, DtbBooking booking2)
			{
				Booking1 = booking1;
				Booking2 = booking2;
				PickupInstruction1 = Booking1.Instructions.AddNew();
				DeliveryInstruction1 = Booking1.Instructions.AddNew();
				PickupInstruction2 = Booking2.Instructions.AddNew();
				DeliveryInstruction2 = Booking2.Instructions.AddNew();

				PickupInstruction1.KN_InstructionType = InstructionTypes.Codes.PickUp;
				PickupInstruction1.Address.OrganisationPK = PickupOrganisation1.PK;
				PickupInstruction1.OrganisationType = OrganisationTypesList.Codes.CNR;
				AssertEquals("Should default to pickup address1", PickupAddress1.PK, PickupInstruction1.Address.E2_OA_Address);

				PickupInstruction2.KN_InstructionType = InstructionTypes.Codes.PickUp;
				PickupInstruction2.Address.OrganisationPK = PickupOrganisation2.PK;
				PickupInstruction2.OrganisationType = OrganisationTypesList.Codes.CNR;
				AssertEquals("Should default to pickup address2", PickupAddress2.PK, PickupInstruction2.Address.E2_OA_Address);

				DeliveryInstruction1.KN_InstructionType = InstructionTypes.Codes.Delivery;
				DeliveryInstruction1.Address.OrganisationPK = DeliveryOrganisation1.PK;
				DeliveryInstruction1.OrganisationType = OrganisationTypesList.Codes.CNE;
				AssertEquals("Should default to delivery address1", DeliveryAddress1.PK, DeliveryInstruction1.Address.E2_OA_Address);

				DeliveryInstruction2.KN_InstructionType = InstructionTypes.Codes.Delivery;
				DeliveryInstruction2.Address.OrganisationPK = DeliveryOrganisation2.PK;
				DeliveryInstruction2.OrganisationType = OrganisationTypesList.Codes.CNE;
				AssertEquals("Should default to delivery address2", DeliveryAddress2.PK, DeliveryInstruction2.Address.E2_OA_Address);

				Job1 = new JobHeader.Loader(Booking1).TryCreate();
				Job1.JH_OA_LocalChargesAddr = ClientAddress1.PK;
				ClientAddress1 = DeliveryInstruction1.Booking.BillingPartyOrLocalClientAddress;

				Job2 = new JobHeader.Loader(Booking2).TryCreate();
				Job2.JH_OA_LocalChargesAddr = ClientAddress2.PK;
				ClientAddress2 = DeliveryInstruction2.Booking.BillingPartyOrLocalClientAddress;
			}

			internal DtbBooking Booking1;
			internal DtbBooking Booking2;
			internal DtbBookingInstruction PickupInstruction1;
			internal DtbBookingInstruction PickupInstruction2;
			internal DtbBookingInstruction DeliveryInstruction1;
			internal DtbBookingInstruction DeliveryInstruction2;
			internal OrgHeader PickupOrganisation1;
			internal OrgHeader PickupOrganisation2;
			internal OrgHeader DeliveryOrganisation1;
			internal OrgHeader DeliveryOrganisation2;
			internal OrgHeader ClientOrganisation1;
			internal OrgHeader ClientOrganisation2;
			internal OrgSupplierBuyerLink PickupOrganizationLink1;
			internal OrgSupplierBuyerLink PickupOrganizationLink2;
			internal OrgAddress PickupAddress1;
			internal OrgAddress PickupAddress2;
			internal OrgAddress DeliveryAddress1;
			internal OrgAddress DeliveryAddress2;
			internal OrgAddress ClientAddress1;
			internal OrgAddress ClientAddress2;
			internal JobHeader Job1;
			internal JobHeader Job2;
		}

		public void TestPackageCategory()
		{
			var booking = GetNewBooking();
			var container = booking.ConsolidationSingleJob.PackageJob.Packages.AddNew("CNT");
			var box = container.Packages.AddNew("BOX");
			var instruction = booking.Instructions.AddNew();
			instruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			instruction.PackageCategory = "CNT";
			AssertContainsExactElementsInAnyOrder(new[] { container }, instruction.DivotsWithPackages.Packages);

			instruction.PackageCategory = "LSE";
			AssertContainsExactElementsInAnyOrder(new[] { box }, instruction.DivotsWithPackages.Packages);

			instruction.PackageCategory = "BTH";
			AssertContainsExactElementsInAnyOrder(new[] { container, box }, instruction.DivotsWithPackages.Packages);
		}

		public void TestOrganisationType()
		{
			var booking = GetNewBooking();
			var instruction = booking.Instructions.AddNew();

			AssertOrganisationTypeSaves(instruction, "");

			foreach (CodeDescriptionPair pair in OrganisationTypesList.Instance)
			{
				AssertOrganisationTypeSaves(instruction, pair.Code);
			}
		}

		void AssertOrganisationTypeSaves(DtbBookingInstruction instruction, ZString orgType)
		{
			instruction.OrganisationType = orgType;
			AssertEquals("OrganisationType was just set, so should be the same", orgType, instruction.OrganisationType);

			Factory.Save();

			var freshFactory = new BusinessObjectFactory();
			var instruction_InFreshFactory = freshFactory.Load<DtbBookingInstruction>(instruction.PK);
			AssertEquals("Should have loaded with the same value", orgType, instruction_InFreshFactory.OrganisationType);
		}

		public void TestReqFrom()
		{
			var booking = GetNewBooking();
			var instruction = booking.Instructions.AddNew();
			AssertEquals("No confirmations, no date.", ZDateTime.Empty, instruction.ReqFrom);

			var pickUpConfirmation = instruction.Confirmations.AddNew();
			var deliveryConfirmation = instruction.Confirmations.AddNew();
			pickUpConfirmation.KK_ConfirmationType = ConfirmationTypes.Codes.PickUp;
			deliveryConfirmation.KK_ConfirmationType = ConfirmationTypes.Codes.Delivery;
			AssertEquals("Confirmations with no date.", ZDateTime.Empty, instruction.ReqFrom);

			pickUpConfirmation.KK_RequiredFrom = ZDateTime.Now;
			deliveryConfirmation.KK_RequiredFrom = ZDateTime.Now.AddDays(1);
			AssertEquals("Should take latest date.", deliveryConfirmation.KK_RequiredFrom, instruction.ReqFrom);
		}

		public void TestReqTo()
		{
			var booking = GetNewBooking();
			var instruction = booking.Instructions.AddNew();
			AssertEquals("No confirmations, no date.", ZDateTime.Empty, instruction.ReqTo);

			var pickUpConfirmation = instruction.Confirmations.AddNew();
			var deliveryConfirmation = instruction.Confirmations.AddNew();
			pickUpConfirmation.KK_ConfirmationType = ConfirmationTypes.Codes.PickUp;
			deliveryConfirmation.KK_ConfirmationType = ConfirmationTypes.Codes.Delivery;
			AssertEquals("Confirmations with no date.", ZDateTime.Empty, instruction.ReqTo);

			pickUpConfirmation.KK_RequiredTo = ZDateTime.Now;
			deliveryConfirmation.KK_RequiredTo = ZDateTime.Now.AddDays(1);
			AssertEquals("Should take latest date.", deliveryConfirmation.KK_RequiredTo, instruction.ReqTo);
		}

		public void TestHasAContainer()
		{
			var instruction = (DtbBookingInstruction)GetNewBusinessObject();
			AssertEquals("Since there are no packages on the instruction, the instruction cannot have a container", false, instruction.HasAContainer);

			var container = Factory.New<PkgPackage>();
			container.KP_F3_NKPackType = "CNT";
			instruction.DivotsWithPackages.AddPackage(container);
			AssertEquals("A container has been added as a package to the instruction, therefore the instruction should have a container", true, instruction.HasAContainer);

			var package = Factory.New<PkgPackage>();
			package.KP_F3_NKPackType = "BOX";
			instruction.DivotsWithPackages.AddPackage(package);
			AssertEquals("Only one package on the instruction needs to be a container for 'HasAContainer' to return true", true, instruction.HasAContainer);
		}

		public void TestIsDelivery()
		{
			AssertFlag("IsDelivery", InstructionTypes.Codes.Delivery, InstructionTypes.Codes.PickUp);
		}

		public void TestIsLoose()
		{
			var instruction = (DtbBookingInstruction)GetNewBusinessObject();
			AssertEquals(false, instruction.IsLoose);

			var container = Factory.New<PkgPackage>();
			container.KP_F3_NKPackType = "CNT";

			instruction.DivotsWithPackages.AddPackage(container);
			AssertEquals(false, instruction.IsLoose);

			var package = Factory.New<PkgPackage>();
			instruction.DivotsWithPackages.AddPackage(package);
			AssertEquals("All packs need to be loose", false, instruction.IsLoose);

			var containerDivot = instruction.PackageDivots.Cast<DtbBookingInstructionPkgDivot>().First(d => d.KD_KP_Package == container.PK);
			containerDivot.Delete();
			AssertEquals("All packs are now loose", true, instruction.IsLoose);
		}

		public void TestIsPickUp()
		{
			AssertFlag("IsPickUp", InstructionTypes.Codes.PickUp, InstructionTypes.Codes.Delivery);
		}

		public void TestIsMulti()
		{
			AssertFlag("IsMulti", InstructionTypes.Codes.Multi, InstructionTypes.Codes.Delivery);
		}

		public void TestIsOrgFlags()
		{
			AssertIsOrgFlags("CFS", "CNR", (i) => i.IsDepot);
			AssertIsOrgFlags("CTO", "CNR", (i) => i.IsCTO);
			AssertIsOrgFlags("CYD", "CNR", (i) => i.IsCYD);
		}

		void AssertIsOrgFlags(ZString org, ZString not, Func<DtbBookingInstruction, ZBool> isOrg)
		{
			var booking = GetNewBooking();
			var instruction = booking.Instructions.AddNew();
			instruction.OrganisationType = not;
			AssertEquals(false, isOrg(instruction));

			instruction.OrganisationType = org;
			AssertEquals(true, isOrg(instruction));

			instruction.OrganisationType = not;
			AssertEquals(false, isOrg(instruction));
		}

		public void TestIsOwnDepot()
		{
			var booking = GetNewBooking();
			var instruction = booking.Instructions.AddNew();
			instruction.OrganisationType = "CNR";
			AssertEquals(false, instruction.IsOwnDepot);

			instruction.OrganisationType = "CFS";
			AssertEquals(false, instruction.IsOwnDepot);

			instruction.KN_InstructionType = InstructionTypes.Codes.Multi;
			AssertEquals(true, instruction.IsOwnDepot);

			instruction.OrganisationType = "CNE";
			AssertEquals(false, instruction.IsOwnDepot);
		}

		protected void AssertFlag(ZString flagPropertyName, ZString validCode, ZString invalidCode)
		{
			var instruction = (DtbBookingInstruction)GetNewBusinessObject();
			AssertEquals(false, instruction[flagPropertyName]);

			instruction.KN_InstructionType = validCode;
			AssertEquals(true, instruction[flagPropertyName]);

			instruction.KN_InstructionType = invalidCode;
			AssertEquals(false, instruction[flagPropertyName]);
		}

		public void TestSetDropModeFromInstructionAddress()
		{
			var address = Factory.New<OrgAddress>();
			address.OA_AIREquipmentNeeded = "DM1";
			address.OA_LCLEquipmentNeeded = "DM2";
			address.OA_FCLEquipmentNeeded = "DM3";

			var containerPackage = Helper.CreatePackage("123", 1, Constants.PkgUnit.Container);
			var loosePackage = Helper.CreatePackage("456", 1, Constants.PkgUnit.Box);

			var booking = GetNewBooking();
			var instruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNE, address);
			instruction.KN_DropMode = "TST";
			instruction.DefaultDropMode();
			AssertEquals("If instruction have a Drop Mode preset, then it should NOT be overridden from Address.", "TST", instruction.KN_DropMode);

			instruction.KN_DropMode = "";
			instruction.DefaultDropMode();
			AssertEquals("If instruction doesn't have a Drop Mode and have no packages attached, then it should NOT be set from Address.", "", instruction.KN_DropMode);

			instruction.DivotsWithPackages.AddPackage(loosePackage); // adds Divot
			AssertEquals("If instruction doesn't have a Drop Mode and have only loose packages attached, then it should default LCL drop mode from Address.", "DM2", instruction.KN_DropMode);

			instruction.KN_DropMode = "";
			instruction.DivotsWithPackages.AddPackage(containerPackage);
			AssertEquals("If instruction doesn't have a Drop Mode and has loose and container packages attached, then it should default FCL drop mode from Address.", "DM3", instruction.KN_DropMode);

			instruction.KN_DropMode = "";
			instruction.PackageDivots.Cast<DtbBookingInstructionPkgDivot>().Single(d => d.KD_KP_Package == loosePackage.PK).Delete();
			AssertEquals("If instruction doesn't have a Drop Mode and has container packages attached, then it should default FCL drop mode from Address.", "DM3", instruction.KN_DropMode);

			instruction.KN_DropMode = "TST";
			instruction.DefaultDropMode();
			AssertEquals("If instruction have a Drop Mode set and packages attached, then it should not override preset drop mode from an Address.", "TST", instruction.KN_DropMode);
		}

		public void TestSuspendDefaultingDropMode()
		{
			var address = Factory.New<OrgAddress>();
			address.OA_AIREquipmentNeeded = "DM1";
			address.OA_LCLEquipmentNeeded = "DM2";
			address.OA_FCLEquipmentNeeded = "DM3";

			var booking = GetNewBooking();
			var instruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNE, address);
			instruction.KN_DropMode = "";

			using (instruction.SuspendDefaultingDropMode())
			{
				var loosePackage = Helper.CreatePackage("456", 1, Constants.PkgUnit.Box);
				instruction.DivotsWithPackages.AddPackage(loosePackage);
				AssertEquals("Drop mode defaulting is suspended, should remain empty.", "", instruction.KN_DropMode);

				using (instruction.SuspendDefaultingDropMode())
				{
					instruction.DefaultDropMode();
					AssertEquals("Drop mode defaulting is suspended, should remain empty.", "", instruction.KN_DropMode);
				}

				instruction.DefaultDropMode();
				AssertEquals("Drop mode defaulting is suspended, should remain empty.", "", instruction.KN_DropMode);
			}

			instruction.DefaultDropMode();
			AssertEquals("If instruction doesn't have a Drop Mode and have only loose packages attached, then it should default LCL drop mode from Address.", "DM2", instruction.KN_DropMode);
		}

		public void TestDelete_DeletesConfirmations()
		{
			var booking = GetNewBooking();
			var instruction = booking.Instructions.AddNew();
			var confirmation = instruction.Confirmations.AddNew();
			AssertEquals("Precondition", false, confirmation.IsDeleted);

			instruction.Delete();
			AssertEquals(true, instruction.IsDeleted);
			AssertEquals(true, confirmation.IsDeleted);
		}

		public void TestDelete_DeletesDocAddress()
		{
			var booking = GetNewBooking();
			var instruction = booking.Instructions.AddNew();
			var jobDocAddress = instruction.Address;
			AssertEquals("Precondition", false, jobDocAddress.IsDeleted);

			instruction.Delete();
			AssertEquals(true, instruction.IsDeleted);
			AssertEquals(true, jobDocAddress.IsDeleted);
		}

		public void TestDelete_Sequence()
		{
			var booking = GetNewBooking();
			var instruction1 = booking.Instructions.AddNew();
			var instruction2 = booking.Instructions.AddNew();
			var instruction3 = booking.Instructions.AddNew();
			var instruction4 = booking.Instructions.AddNew();
			var instruction5 = booking.Instructions.AddNew();

			AssertEquals("Precondition", 1, instruction1.KN_Sequence);
			AssertEquals("Precondition", 2, instruction2.KN_Sequence);
			AssertEquals("Precondition", 3, instruction3.KN_Sequence);
			AssertEquals("Precondition", 4, instruction4.KN_Sequence);
			AssertEquals("Precondition", 5, instruction5.KN_Sequence);

			instruction2.Delete();
			AssertEquals(1, instruction1.KN_Sequence);
			AssertEquals(2, instruction3.KN_Sequence);
			AssertEquals(3, instruction4.KN_Sequence);
			AssertEquals(4, instruction5.KN_Sequence);

			using (((IBusinessObjectCollection)booking.Instructions).SuspendListChanged())
			{
				instruction4.Delete();
				AssertEquals(1, instruction1.KN_Sequence);
				AssertEquals(2, instruction3.KN_Sequence);
				AssertEquals(3, instruction5.KN_Sequence);

				instruction3.Delete();
				AssertEquals(1, instruction1.KN_Sequence);
				AssertEquals(2, instruction5.KN_Sequence);
			}
		}

		public void TestSuspendOnDocAddressChanged()
		{
			var data = new ATLTestData(Helper);
			data.SetUpOrganizationTestData();

			data.PickupAddress1.OA_AuthorityToLeave = AuthorityToLeaveOptions.Codes.YES;
			data.DeliveryAddress1.OA_AuthorityToLeave = AuthorityToLeaveOptions.Codes.YES;
			data.ClientAddress1.OA_AuthorityToLeave = AuthorityToLeaveOptions.Codes.YES;
			data.SetUpBookingTestData(GetNewBooking(), GetNewBooking());

			data.PickupInstruction1.Address.E2_OA_Address = data.PickupAddress1.PK;
			data.DeliveryInstruction1.Address.E2_OA_Address = ZGuid.Empty;
			AssertEquals("Without the Delivery Address set, Is Authorised to leave should not be set.", false, data.DeliveryInstruction1.KN_IsAuthorisedToLeave);

			data.DeliveryInstruction1.Address.E2_OA_Address = data.DeliveryAddress1.PK;
			AssertEquals("Precondition: Setting Delivery Address should update Is Authorised to leave.", true, data.DeliveryInstruction1.KN_IsAuthorisedToLeave);

			data.DeliveryInstruction1.Address.E2_OA_Address = ZGuid.Empty;
			AssertEquals("Precondition: Setting Delivery Address should update Is Authorised to leave.", false, data.DeliveryInstruction1.KN_IsAuthorisedToLeave);

			using (data.DeliveryInstruction1.SuspendOnDocAddressChanged())
			{
				data.DeliveryInstruction1.Address.E2_OA_Address = data.DeliveryAddress1.PK;
				AssertEquals("When DocAddressChanged is suspended, Is Authorised to leave should *not* be updated.", false, data.DeliveryInstruction1.KN_IsAuthorisedToLeave);
			}
		}

		public void TestUniversalCopyAttributes()
		{
			var instruction = Factory.New<DtbBookingInstruction>();

			var componentType = instruction.GetType();
			var organisationTypeInfo = BusinessObjectToCopyTemplateReflectionHelper.GetProperties(componentType).First(info => info.Name == "OrganisationType");
			AssertEquals("OrganisationType should have UniversalCopyAlwaysCopyPropertyAttribute.CopyMode.AtTheBeginning attribute.", true, organisationTypeInfo.GetCustomAttributes(typeof(UniversalCopyAlwaysCopyPropertyAttribute), true)
				.Cast<UniversalCopyAlwaysCopyPropertyAttribute>().Any(attr => attr.Mode == UniversalCopyAlwaysCopyPropertyAttribute.CopyMode.AtTheBeginning));

			var docAddressesInfo = BusinessObjectToCopyTemplateReflectionHelper.GetProperties(componentType).First(info => info.Name == "DocAddresses");
			AssertEquals("DocAddresses collection should have UniversalCopyCollectionEntityAttribute.", true, docAddressesInfo.GetCustomAttributes(typeof(UniversalCopyCollectionEntityAttribute), true).First() != null);
		}

		public void TestPropertyWithUniversalCopyAttribute()
		{
			var instruction = Factory.New<DtbBookingInstruction>();
			var componentType = instruction.GetType();

			var propertyInfo = BusinessObjectToCopyTemplateReflectionHelper.GetProperties(componentType).First(info => info.Name == "KN_InstructionType");
			AssertEquals("KN_InstructionType should have UniversalCopyAlwaysCopyPropertyAttribute.CopyMode.AtTheBeginning attribute.", true, propertyInfo.GetCustomAttributes(typeof(UniversalCopyAlwaysCopyPropertyAttribute), true)
				.Cast<UniversalCopyAlwaysCopyPropertyAttribute>().Any(attr => attr.Mode == UniversalCopyAlwaysCopyPropertyAttribute.CopyMode.AtTheBeginning));

			var propertyrelatedInfo = BusinessObjectToCopyTemplateReflectionHelper.GetProperties(componentType).First(info => info.Name == "Address");
			AssertEquals("Address should have UniversalCopyRelatedEntityAttribute.", true, propertyrelatedInfo.GetCustomAttributes(typeof(UniversalCopyRelatedEntityAttribute), true).Cast<UniversalCopyRelatedEntityAttribute>().Any(attr => attr.DisableCopyMethodLink && attr.CommaSeparatedSkipPropertiesNames == "E2_ParentID"));
		}

		public void TestUpdateStatusWithDeletedDtbBookingInstruction()
		{
			var booking = GetNewBooking();
			var instruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);

			instruction.Delete();

			Assert(instruction.IsDeleted);
			AssertNoExceptionThrown(() => instruction.UpdateStatus());
		}

		public void TestUpdateStatus()
		{
			var booking = GetNewBooking();
			var instruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);

			instruction.KN_Status = "";
			AssertEquals("Precondition", "", instruction.KN_Status);

			instruction.UpdateStatus();
			AssertEquals(TransportStatuses.Codes.Available, instruction.KN_Status);

			booking = Helper.CreateBooking();
			instruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);

			var pickUpConfirmation = instruction.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp);
			AssertEquals("PickUp Confirmation exists but has no actual date, Status should fallback to Available.", TransportStatuses.Codes.Available, instruction.KN_Status);

			pickUpConfirmation.KK_Actual = ZDateTime.Now;
			instruction.UpdateStatus();
			AssertEquals(TransportStatuses.Codes.PickedUp, instruction.KN_Status);

			var deliveryConfirmation = instruction.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);
			deliveryConfirmation.KK_Actual = ZDateTime.Now;
			instruction.UpdateStatus();
			AssertEquals("Delivery Confirmation exists but the Instruction is a PickUp, Status should be PickUp.", TransportStatuses.Codes.PickedUp, instruction.KN_Status);

			instruction.KN_InstructionType = InstructionTypes.Codes.Delivery;
			instruction.UpdateStatus();
			AssertEquals(TransportStatuses.Codes.Delivered, instruction.KN_Status);
		}

		public void TestDocAddresses_WhenInstructionIsSub()
		{
			var subInstruction = Helper.CreateInstruction(Helper.CreateBooking());
			var masterInstruction = Helper.CreateInstruction(Helper.CreateBooking());
			subInstruction.KN_KN_MasterBookingInstruction = masterInstruction.PK;

			Factory.Save();

			AssertEquals("Sub Instruction should refer to its Master Instruction for DocAddresses.", masterInstruction.DocAddresses, subInstruction.DocAddresses);
		}

		public void TestDocAddresses_WhenInstructionIsNotSub()
		{
			var instruction = Helper.CreateInstruction(Helper.CreateBooking());
			AssertEquals("Precondition: Instruction is not a sub instruction.", false, instruction.IsSub);

			AssertEquals(typeof(JobDocAddressDependentCollection), instruction.DocAddresses.GetType());
			AssertEquals(true, instruction.IsRegisteredEditableChildObject(instruction.DocAddresses));
		}

		public void TestDocAddresses_WhenMasterInstructionChanges()
		{
			var instruction = Helper.CreateInstruction(Helper.CreateBooking());
			Factory.Save();

			AssertEquals("Precondition: KN_KN_MasterBookingInstruction should be an empty Guid.", ZGuid.Empty, instruction.KN_KN_MasterBookingInstruction);
			AssertNotNull("DocAddresses should not return null.", instruction.DocAddresses);
			AssertEquals("DocAddresses should be a child of the Instruction.", true, instruction.IsRegisteredEditableChildObject(instruction.DocAddresses));

			var masterInstruction1 = Helper.CreateInstruction(Helper.CreateBooking());
			masterInstruction1.KN_IsMaster = true;
			masterInstruction1.KN_MasterBookingVersion = 1;
			instruction.KN_KN_MasterBookingInstruction = masterInstruction1.PK;
			instruction.KN_MasterBookingVersion = 1;
			Factory.Save();

			AssertEquals("Instruction should refer to its Master Instruction for DocAddresses.", masterInstruction1.DocAddresses, instruction.DocAddresses);
			AssertEquals("DocAddresses should be registered as an editable child object of the sub Instruction.", true, instruction.IsRegisteredEditableChildObject(instruction.DocAddresses));

			var masterInstruction2 = Helper.CreateInstruction(Helper.CreateBooking());
			masterInstruction2.KN_IsMaster = true;
			masterInstruction2.KN_MasterBookingVersion = 1;
			instruction.KN_KN_MasterBookingInstruction = masterInstruction2.PK;
			Factory.Save();

			AssertEquals("Instruction should refer to its new Master Instruction for DocAddresses.", masterInstruction2.DocAddresses, instruction.DocAddresses);
			AssertEquals("DocAddresses should be registered as an editable child object of the sub Instruction.", true, instruction.IsRegisteredEditableChildObject(instruction.DocAddresses));

			instruction.KN_KN_MasterBookingInstruction = ZGuid.Empty;

			AssertNotNull("DocAddresses should not return null.", instruction.DocAddresses);
			AssertEquals("DocAddresses should be a child of the Instruction.", true, instruction.IsRegisteredEditableChildObject(instruction.DocAddresses));
			AssertNotEquals("Instruction should not refer to the first Master Instruction for DocAddresses.", masterInstruction1.DocAddresses, instruction.DocAddresses);
			AssertNotEquals("Instruction should not refer to the second Master Instruction for DocAddresses.", masterInstruction2.DocAddresses, instruction.DocAddresses);
		}

		public void TestIDocAddresses_DocAddresses()
		{
			var booking = (DtbBooking)Factory.New(ExpectedTransportType);
			var instruction = booking.Instructions.AddNew();
			instruction.Address.E2_OA_Address = Helper.CreateOrganisation("ABCSYD").MainAddress.PK;
			AssertContainsExactElementsInAnyOrder(new JobDocAddress[] { instruction.Address }, instruction.DocAddresses);
			AssertEquals(true, instruction.IsRegisteredEditableChildObject(instruction.DocAddresses));
		}

		public void TestIDocAddresses_SupportedAddressTypes()
		{
			var booking = (DtbBooking)Factory.New(ExpectedTransportType);
			var instruction = booking.Instructions.AddNew();
			var iInstruction = (IDocAddresses)instruction;

			var expected = new List<DocAddressType>();
			expected.Add(CartageOrgTypeAddressTypeConverter.GetDocAddressTypeFromOrgType(""));

			foreach (CodeDescriptionPair pair in OrganisationTypesList.Instance)
			{
				expected.Add(CartageOrgTypeAddressTypeConverter.GetDocAddressTypeFromOrgType(pair.Code));
			}

			AssertContainsExactElementsInAnyOrder(expected, iInstruction.SupportedAddressTypes);
		}

		public void TestIDocAddresses_CanDeleteAddress()
		{
			var booking = (DtbBooking)Factory.New(ExpectedTransportType);
			var instruction = booking.Instructions.AddNew();
			var iInstruction = (IDocAddresses)instruction;

			AssertEquals("Want to always have an Instruction Address.", false, iInstruction.CanDeleteAddress(instruction.Address));
		}

		public void TestIDocAddresses_GetDocAddressRequirement()
		{
			var booking = (DtbBooking)Factory.New(ExpectedTransportType);
			var instruction = booking.Instructions.AddNew();
			var iInstruction = (IDocAddresses)instruction;

			foreach (CodeDescriptionPair docAddressTypePair in OrganisationTypesList.Instance)
			{
				var docAddressType = CartageOrgTypeAddressTypeConverter.GetDocAddressTypeFromOrgType(docAddressTypePair.Code);

				foreach (CodeDescriptionPair instructionTypePair in new InstructionTypes().List)
				{
					instruction.KN_InstructionType = instructionTypePair.Code;

					AddressType reqAddressTypeWhenPickup;
					AddressType reqAddressTypeWhenDelivery;
					AddressType reqAddressTypeWhenLocal;

					switch (instructionTypePair.Code)
					{
						case InstructionTypes.Codes.PickUp:
							reqAddressTypeWhenPickup = AddressType.PIC;
							reqAddressTypeWhenDelivery = AddressType.PIC;
							reqAddressTypeWhenLocal = AddressType.PIC;
							break;

						case InstructionTypes.Codes.Delivery:
							reqAddressTypeWhenPickup = AddressType.DLV;
							reqAddressTypeWhenDelivery = AddressType.DLV;
							reqAddressTypeWhenLocal = AddressType.DLV;
							break;

						case InstructionTypes.Codes.Multi:
							reqAddressTypeWhenPickup = AddressType.PIC;
							reqAddressTypeWhenDelivery = AddressType.DLV;
							reqAddressTypeWhenLocal = AddressType.NoDefault;
							break;

						default:
							reqAddressTypeWhenPickup = AddressType.NoDefault;
							reqAddressTypeWhenDelivery = AddressType.NoDefault;
							reqAddressTypeWhenLocal = AddressType.NoDefault;
							break;
					}

					booking.KM_Direction = Constants.CartageDirection.Origin;
					AssertRequirement(docAddressType, reqAddressTypeWhenPickup, iInstruction);

					booking.KM_Direction = Constants.CartageDirection.Export;
					AssertRequirement(docAddressType, reqAddressTypeWhenPickup, iInstruction);

					booking.KM_Direction = Constants.CartageDirection.Destination;
					AssertRequirement(docAddressType, reqAddressTypeWhenDelivery, iInstruction);

					booking.KM_Direction = Constants.CartageDirection.Import;
					AssertRequirement(docAddressType, reqAddressTypeWhenDelivery, iInstruction);

					booking.KM_Direction = Constants.CartageDirection.Local;
					AssertRequirement(docAddressType, reqAddressTypeWhenLocal, iInstruction);
				}
			}
		}

		void AssertRequirement(DocAddressType docAddressType, AddressType reqAddressType, IDocAddresses iInstruction)
		{
			var requirement = iInstruction.GetDocAddressRequirement(docAddressType);

			AssertEquals(docAddressType, requirement.DefaultDocAddressType);
			AssertEquals(ContactType.LocalTransport, requirement.DefaultContactType);
			AssertEquals(reqAddressType, requirement.DefaultAddressType);
			AssertEquals(true, requirement.SaveEvenIfBlank);
		}

		public void TestIDocAddresses_PiggyBackedDocAddressValidation()
		{
			TestIDocAddresses_PiggyBackedDocAddressValidationCore();
		}

		protected virtual void TestIDocAddresses_PiggyBackedDocAddressValidationCore()
		{
			var booking = (DtbBooking)Factory.New(ExpectedTransportType);
			var instruction = booking.Instructions.AddNew();
			var iInstruction = (IDocAddresses)instruction;
			AssertNull(iInstruction.PiggyBackedDocAddressValidation(null));
		}

		public void TestIDocAddresses_GetOrgHeaderList()
		{
			var booking = (DtbBooking)Factory.New(ExpectedTransportType);
			var instruction = booking.Instructions.AddNew();
			var iInstruction = (IDocAddresses)instruction;

			// implemented doc address types
			AssertEquals(typeof(DepotCollection), iInstruction.GetOrgHeaderList(DocAddressType.LocalCartageCFS).GetType());
			AssertEquals(typeof(CTOCollection), iInstruction.GetOrgHeaderList(DocAddressType.LocalCartageCTO).GetType());
			AssertEquals(typeof(ContainerYardCollection), iInstruction.GetOrgHeaderList(DocAddressType.LocalCartageYard).GetType());
			AssertEquals(typeof(ConsigneeCollection), iInstruction.GetOrgHeaderList(DocAddressType.LocalCartageImporter).GetType());
			AssertEquals(typeof(ConsignorCollection), iInstruction.GetOrgHeaderList(DocAddressType.LocalCartageExporter).GetType());

			// not implemented doc address types
			AssertEquals(typeof(OrgHeaderCollection), iInstruction.GetOrgHeaderList(DocAddressType.LocalCartageMSC).GetType());
			AssertEquals(typeof(OrgHeaderCollection), iInstruction.GetOrgHeaderList(DocAddressType.LocalCartageWarehouse).GetType());
			AssertEquals(typeof(OrgHeaderCollection), iInstruction.GetOrgHeaderList(DocAddressType.LocalCartageService).GetType());
			AssertEquals(typeof(OrgHeaderCollection), iInstruction.GetOrgHeaderList(DocAddressType.None).GetType());
		}

		public void TestIDocAddresses_OnDocAddressChangedSetsUtcDates()
		{
			var year = ZDateTime.Now.Year;

			var date = new ZDateTime(year, 9, 27);
			var auOrg = Helper.CreateOrganisation("AU");
			var usOrg = Helper.CreateOrganisation("US");
			var instruction = Helper.CreateInstruction(GetNewBooking());
			var confirmation = Helper.CreateConfirmation(instruction, ConfirmationTypes.Codes.PickUp);

			auOrg.OH_RL_NKClosestPort = "AUSYD";
			usOrg.OH_RL_NKClosestPort = "USSFO";

			instruction.Address.E2_OA_Address = auOrg.MainAddress.PK;
			confirmation.KK_RequiredFrom = date;
			confirmation.KK_RequiredTo = date.AddHours(1);
			confirmation.KK_Estimated = date.AddHours(2);
			AssertEquals("Precondition", date.AddHours(-10), confirmation.KK_RequiredFromUtc);
			AssertEquals("Precondition", date.AddHours(-9), confirmation.KK_RequiredToUtc);
			AssertEquals("Precondition", date.AddHours(-8), confirmation.KK_EstimatedUtc);

			instruction.Address.E2_OA_Address = usOrg.MainAddress.PK;
			AssertEquals("After address change, Req From Utc should be updated.", date.AddHours(7), confirmation.KK_RequiredFromUtc);
			AssertEquals("After address change, Req To Utc should be updated.", date.AddHours(8), confirmation.KK_RequiredToUtc);
			AssertEquals("After address change, Estimated Utc should be updated.", date.AddHours(9), confirmation.KK_EstimatedUtc);
		}

		public void TestIConsignmentAddress_Properties()
		{
			var booking = GetNewBooking();
			var instruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var equipment = Factory.New<RefEquipment>();

			instruction.KN_RQ_Equipment = equipment.PK;
			instruction.KN_DropMode = Constants.LCLAIREquipmentNeeded.Premise;
			instruction.KN_Status = TransportStatuses.Codes.Available;
			instruction.KN_ServiceInstruction = "Test Notes";

			AssertEquals(Constants.LCLAIREquipmentNeeded.Premise, instruction.DropMode);
			AssertEquals(TransportStatuses.Codes.Available, instruction.Status);
			AssertEquals("Test Notes", instruction.ServiceInstruction);
			AssertEquals(equipment, instruction.Equipment);
			AssertEquals(InstructionTypes.Codes.PickUp, instruction.ConsignmentAddressType);
		}

		public void TestIsSub()
		{
			var instruction = Helper.CreateInstruction(Helper.CreateBooking());

			AssertEquals("Precondition: KN_KN_MasterBookingInstruction should be an empty Guid.", ZGuid.Empty, instruction.KN_KN_MasterBookingInstruction);
			AssertEquals("IsSub should return false.", false, instruction.IsSub);

			instruction.KN_KN_MasterBookingInstruction = ZGuid.NewZGuid();
			AssertEquals("IsSub should return true.", true, instruction.IsSub);
		}

		public void TestMasterBookingInstruction()
		{
			var instruction = Helper.CreateInstruction(Helper.CreateBooking());
			Factory.Save();

			AssertEquals("Precondition: KN_KN_MasterBookingInstruction should be an empty Guid.", ZGuid.Empty, instruction.KN_KN_MasterBookingInstruction);
			AssertEquals("MasterBookingInstruction should return null.", null, instruction.MasterBookingInstruction);

			var masterInstruction1 = Helper.CreateInstruction(Helper.CreateBooking());
			masterInstruction1.KN_IsMaster = true;
			masterInstruction1.KN_MasterBookingVersion = 1;
			instruction.KN_KN_MasterBookingInstruction = masterInstruction1.PK;
			instruction.KN_MasterBookingVersion = 1;
			Factory.Save();

			AssertEquals("MasterBookingInstruction should return the master booking.", masterInstruction1.PK, instruction.MasterBookingInstruction.PK);

			var masterInstruction2 = Helper.CreateInstruction(Helper.CreateBooking());
			masterInstruction2.KN_IsMaster = true;
			masterInstruction2.KN_MasterBookingVersion = 1;
			instruction.KN_KN_MasterBookingInstruction = masterInstruction2.PK;
			Factory.Save();

			AssertEquals("MasterBookingInstruction should return the second master booking.", masterInstruction2.PK, instruction.MasterBookingInstruction.PK);

			instruction.KN_KN_MasterBookingInstruction = ZGuid.Empty;

			AssertEquals("MasterBookingInstruction should return null.", null, instruction.MasterBookingInstruction);
		}

		public void TestNewInstructionSetsMasterFlagAndVersionIfParentBookingIsMaster()
		{
			var booking = GetNewBooking();
			booking.KM_IsMaster = booking.ConsolidationSingleJob.KB_IsMaster = true;
			booking.KM_MasterBookingVersion = booking.ConsolidationSingleJob.KB_MasterBookingVersion = (short)1;

			var instruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			instruction.FillWithValidTestData();

			Factory.Save();

			CombineAssertions("Master booking fields on instruction should be set correctly for a master booking as parent Booking is a master booking", () =>
			{
				AssertEquals("KN_IsMaster should be set to true as parent Booking.KM_IsMaster == true", true, instruction.KN_IsMaster);
				AssertEquals("KN_MasterBookingVersion should be set to 1 as parent Booking.KM_IsMaster == true", (short)1, instruction.KN_MasterBookingVersion);
			});
		}

		public void TestNewInstructionBlanksMasterFlagAndVersionIfParentBookingIsNotMaster()
		{
			var booking = GetNewBooking();
			CombineAssertions("Precondition: Master booking fields on booking should have defaulted to not master booking", () =>
			{
				AssertEquals("Precondition: KM_IsMaster defaults to false on DtbBooking", false, booking.KM_IsMaster);
				AssertEquals("Precondition: KM_MasterBookingVersion defaults to 0 on DtbBooking", (short)0, booking.KM_MasterBookingVersion);
				AssertEquals("Precondition: KM_KM_MasterBooking defaults to empty on DtbBooking", ZGuid.Empty, booking.KM_KM_MasterBooking);
			});
			var instruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			instruction.FillWithValidTestData();

			Factory.Save();

			CombineAssertions("Master booking fields on instruction should be set correctly for a non-master booking as parent Booking is a master booking", () =>
			{
				AssertEquals("KN_IsMaster should be set to false as parent Booking.KM_IsMaster == false", false, instruction.KN_IsMaster);
				AssertEquals("KN_MasterBookingVersion should be set to 0 as parent Booking.KM_IsMaster == false", (short)0, instruction.KN_MasterBookingVersion);
			});
		}

		public void TestNewInstructionBlanksMasterFlagAndVersionIfParentBookingIsNotMasterEvenIfInstructionMasterFieldsIncorrectlySet()
		{
			var booking = GetNewBooking();
			CombineAssertions("Precondition: Master booking fields on booking should have defaulted to not master booking", () =>
			{
				AssertEquals("Precondition: KM_IsMaster defaults to false on DtbBooking", false, booking.KM_IsMaster);
				AssertEquals("Precondition: KM_MasterBookingVersion defaults to 0 on DtbBooking", (short)0, booking.KM_MasterBookingVersion);
				AssertEquals("Precondition: KM_KM_MasterBooking defaults to empty on DtbBooking", ZGuid.Empty, booking.KM_KM_MasterBooking);
			});
			var instruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			instruction.FillWithValidTestData();
			instruction.KN_IsMaster = true;
			instruction.KN_MasterBookingVersion = (short)10;

			Factory.Save();

			CombineAssertions("Master booking fields on instruction should be set correctly for a non-master booking as parent Booking is a master booking", () =>
			{
				AssertEquals("KN_IsMaster should be set to false as parent Booking.KM_IsMaster == false", false, instruction.KN_IsMaster);
				AssertEquals("KN_MasterBookingVersion should be set to 0 as parent Booking.KM_IsMaster == false", (short)0, instruction.KN_MasterBookingVersion);
			});
		}

		public void TestSubBookingInstructionUpdatesMasterBookingVersionIfNewAndNotYetSet()
		{
			var masterConsolidation = Helper.CreateConsolidation();
			var masterBooking = Helper.CreateBooking(masterConsolidation);
			masterBooking.KM_IsMaster = true;
			Factory.Save();
			var masterBookingInstruction = masterBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			masterBookingInstruction.FillWithValidTestData();
			masterBookingInstruction.KN_IsMaster = true;
			Factory.Save();

			var subConsolidation = Helper.CreateConsolidation();
			subConsolidation.KB_KB_MasterBookingConsolidation = masterConsolidation.PK;
			var subBooking = Helper.CreateBooking(subConsolidation);
			subBooking.KM_KM_MasterBooking = masterBooking.PK;
			var subBookingInstruction = subBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			subBookingInstruction.FillWithValidTestData();
			subBookingInstruction.KN_KN_MasterBookingInstruction = masterBookingInstruction.PK;
			subConsolidation.KB_MasterBookingVersion = subBooking.KM_MasterBookingVersion = (short)1;

			AssertEquals("Precondition: sub booking instruction MasterBookingVersion has not yet been set (still 0)", ZShort.Zero, subBookingInstruction.KN_MasterBookingVersion);
			Factory.Save();

			AssertEquals("Sub booking instruction MasterBookingVersion should have updated to 1", (short)1, subBookingInstruction.KN_MasterBookingVersion);
		}

		public void TestSubBookingInstructionDoesNotUpdateMasterBookingVersionIfNewButAlreadySet()
		{
			var masterConsolidation = Helper.CreateConsolidation();
			var masterBooking = Helper.CreateBooking(masterConsolidation);
			masterBooking.KM_IsMaster = true;
			Factory.Save();
			var masterBookingInstruction = masterBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			masterBookingInstruction.FillWithValidTestData();
			masterBookingInstruction.KN_IsMaster = true;
			Factory.Save();

			masterConsolidation.KB_MasterBookingVersion = masterBooking.KM_MasterBookingVersion = masterBookingInstruction.KN_MasterBookingVersion = (short)2;

			var subConsolidation = Helper.CreateConsolidation();
			subConsolidation.KB_KB_MasterBookingConsolidation = masterConsolidation.PK;
			subConsolidation.KB_MasterBookingVersion = (short)1;
			var subBooking = Helper.CreateBooking(subConsolidation);
			subBooking.KM_KM_MasterBooking = masterBooking.PK;
			subBooking.KM_MasterBookingVersion = (short)1;
			var subBookingInstruction = subBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			subBookingInstruction.FillWithValidTestData();
			subBookingInstruction.KN_KN_MasterBookingInstruction = masterBookingInstruction.PK;
			subBookingInstruction.KN_MasterBookingVersion = (short)2;
			Factory.Save();

			AssertEquals("Sub booking instruction MasterBookingVersion should have stayed at 2 (process that creates sub probably populated it from the master)", (short)2, subBookingInstruction.KN_MasterBookingVersion);
		}

		public void TestDelete_ShouldDeleteSubInstructions()
		{
			var subInstruction = Helper.CreateInstruction(Helper.CreateBooking());
			var masterInstruction = Helper.CreateInstruction(Helper.CreateBooking());
			subInstruction.KN_KN_MasterBookingInstruction = masterInstruction.PK;

			Factory.Save();

			masterInstruction.Delete();

			Assert(subInstruction.IsDeleted);
		}

		DtbBooking GetNewBooking()
		{
			return Helper.CreateBooking();
		}

		protected new TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}

		TransportBookingTestHelper helper;
	}

	[TestedType(typeof(DtbBookingInstruction))]
	sealed class DtbBookingInstructionMasterBookingEntityTest : BaseIDtbMasterBookingEntityTest
	{
		protected override IEnumerable<SchemaColumn> ReplicatedColumns => DtbMasterBookingReplication.DtbBookingInstructionReplicatedColumns;

		protected override IEnumerable<SchemaColumn> NonReplicatedColumns => new SchemaColumn[]
		{
			DtbBookingInstructionSchema.PK,
			DtbBookingInstructionSchema.KN_SystemCreateUser,
			DtbBookingInstructionSchema.KN_SystemCreateBranch,
			DtbBookingInstructionSchema.KN_SystemCreateDepartment,
			DtbBookingInstructionSchema.KN_SystemCreateTimeUtc,
			DtbBookingInstructionSchema.KN_SystemLastEditUser,
			DtbBookingInstructionSchema.KN_SystemLastEditTimeUtc,
			DtbBookingInstructionSchema.KN_IsMaster,
			DtbBookingInstructionSchema.KN_MasterBookingVersion,
			DtbBookingInstructionSchema.KN_KN_MasterBookingInstruction,
			DtbBookingInstructionSchema.KN_KM_BookingMovement,
		};

		protected override ITableSchema tableSchema => DtbBookingInstructionSchema.Instance;

		public void TestMasterBookingInstructionUpdateToSequenceUpdatesMasterBookingVersion()
		{
			CoreTestMasterBookingInstructionUpdateToReplicationFieldUpdatesMasterBookingVersion(DtbBookingInstructionSchema.KN_Sequence, 2);
		}

		public void TestMasterBookingInstructionUpdateToInstructionTypeUpdatesMasterBookingVersion()
		{
			CoreTestMasterBookingInstructionUpdateToReplicationFieldUpdatesMasterBookingVersion(DtbBookingInstructionSchema.KN_InstructionType, "DLV");
		}

		public void TestMasterBookingInstructionUpdateToDropModeUpdatesMasterBookingVersion()
		{
			CoreTestMasterBookingInstructionUpdateToReplicationFieldUpdatesMasterBookingVersion(DtbBookingInstructionSchema.KN_DropMode, "ASK");
		}

		public void TestMasterBookingInstructionUpdateToServiceInstructionUpdatesMasterBookingVersion()
		{
			CoreTestMasterBookingInstructionUpdateToReplicationFieldUpdatesMasterBookingVersion(DtbBookingInstructionSchema.KN_ServiceInstruction, "Some other service instruction");
		}

		public void TestMasterBookingInstructionUpdateToStatusUpdatesMasterBookingVersion()
		{
			CoreTestMasterBookingInstructionUpdateToReplicationFieldUpdatesMasterBookingVersion(DtbBookingInstructionSchema.KN_Status, BookingInstructionStatuses.Codes.Incomplete);
		}

		public void TestMasterBookingInstructionUpdateToEquipmentUpdatesMasterBookingVersion()
		{
			var equipment = Factory.NewWithValidTestData<RefEquipment>();
			Factory.Save();
			CoreTestMasterBookingInstructionUpdateToReplicationFieldUpdatesMasterBookingVersion(DtbBookingInstructionSchema.KN_RQ_Equipment, equipment.PK);
		}

		public void TestMasterBookingInstructionUpdateToIsContainerRateableUpdatesMasterBookingVersion()
		{
			CoreTestMasterBookingInstructionUpdateToReplicationFieldUpdatesMasterBookingVersion(DtbBookingInstructionSchema.KN_IsContainerRateable, true);
		}

		public void TestMasterBookingInstructionUpdateToIsLooseRateableUpdatesMasterBookingVersion()
		{
			CoreTestMasterBookingInstructionUpdateToReplicationFieldUpdatesMasterBookingVersion(DtbBookingInstructionSchema.KN_IsLooseRateable, true);
		}

		public void TestMasterBookingInstructionUpdateToDomesticZoneUpdatesMasterBookingVersion()
		{
			var transportProvider = Factory.NewWithValidTestData<RateTransportProvider>();
			var domesticZone = Factory.NewWithValidTestData<RateTransportZone>();
			domesticZone.TZ_TP = transportProvider.PK;
			Factory.Save();
			CoreTestMasterBookingInstructionUpdateToReplicationFieldUpdatesMasterBookingVersion(DtbBookingInstructionSchema.KN_TZ_DomesticZone, domesticZone.PK);
		}

		public void TestMasterBookingInstructionUpdateToIsAuthorisedToLeaveUpdatesMasterBookingVersion()
		{
			CoreTestMasterBookingInstructionUpdateToReplicationFieldUpdatesMasterBookingVersion(DtbBookingInstructionSchema.KN_IsAuthorisedToLeave, true);
		}

		void CoreTestMasterBookingInstructionUpdateToReplicationFieldUpdatesMasterBookingVersion(SchemaColumn columnChanged, object updatedValue)
		{
			var bookingInstruction = CreateBookingInstructionForMasterBookingReplicationTests(true);

			var previousMasterBookingVersion = bookingInstruction.KN_MasterBookingVersion;

			bookingInstruction[columnChanged] = updatedValue;
			var propertyInfo = bookingInstruction.FindPropertyInfo(columnChanged.Name);
			CombineAssertions("Preconditions for master booking version update", () =>
			{
				Assert("Precondition: BookingInstruction.HasChanges is true", bookingInstruction.HasChanges);
				Assert("Precondition: BookingInstruction." + columnChanged.Name + " value has changed", propertyInfo.HasChanges);
			});
			Factory.Save();

			AssertGreaterThan("On a Master Booking Instruction, update to " + columnChanged.Name + " should update master booking version", bookingInstruction.KN_MasterBookingVersion, previousMasterBookingVersion);
		}

		public void TestNonMasterBookingInstructionUpdatesDoNotUpdateMasterBookingVersion()
		{
			var bookingInstruction = CreateBookingInstructionForMasterBookingReplicationTests(false);

			AssertEquals("Precondition: Non-master booking instruction should have KN_MasterBookingVersion of 0", (short)0, bookingInstruction.KN_MasterBookingVersion);

			var equipment = Factory.NewWithValidTestData<RefEquipment>();
			var transportProvider = Factory.NewWithValidTestData<RateTransportProvider>();
			var domesticZone = Factory.NewWithValidTestData<RateTransportZone>();
			domesticZone.TZ_TP = transportProvider.PK;
			Factory.Save();

			bookingInstruction.KN_Sequence = 2;
			bookingInstruction.KN_InstructionType = "DLV";
			bookingInstruction.KN_DropMode = "ASK";
			bookingInstruction.KN_ServiceInstruction = "Some other instruction";
			bookingInstruction.KN_Status = BookingInstructionStatuses.Codes.Incomplete;
			bookingInstruction.KN_RQ_Equipment = equipment.PK;
			bookingInstruction.KN_IsContainerRateable = true;
			bookingInstruction.KN_IsLooseRateable = true;
			bookingInstruction.KN_TZ_DomesticZone = domesticZone.PK;
			bookingInstruction.KN_IsAuthorisedToLeave = true;

			Assert("Precondition: BookingInstruction.HasChanges is true", bookingInstruction.HasChanges);
			Factory.Save();

			AssertEquals("On a Non-master booking instruction, any updates should leave master booking version at 0", (short)0, bookingInstruction.KN_MasterBookingVersion);
		}

		public void TestSubInstructionDoesNotCreateDivotsOrConfirmationsOnSave()
		{
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			var subBooking = Helper.CreateBooking();
			subBooking.KM_IsMaster = false;

			var package = subBooking.PackageJob.Packages.AddNew();
			masterBooking.SubBookings.Add(subBooking);
			Factory.Save();

			var masterBookingInstruction = masterBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var subBookingInstruction = subBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			subBookingInstruction.KN_KN_MasterBookingInstruction = masterBookingInstruction.PK;
			masterBookingInstruction.DivotsWithPackages.AddPackage(package);
			AssertNoExceptionThrown("Should not throw exception on save - no longer calling AssignPackages() OnSaving", () => Factory.Save());

			var queryToCheckPhysicalDivotsOnSubInstruction = new ZDBOnlyQuery(typeof(DtbBookingInstructionPkgDivot));
			queryToCheckPhysicalDivotsOnSubInstruction.AddToFilter(DtbBookingInstructionPkgDivotSchema.KD_KN_BookingInstruction, subBookingInstruction.PK);
			var physicalDivotsOnSubInstruction = Factory.Load<DtbBookingInstructionPkgDivot>(queryToCheckPhysicalDivotsOnSubInstruction);
			AssertEquals("Should have been no DtbBookingInstructionPkgDivot records linking directly to subBookingInstruction", 0, physicalDivotsOnSubInstruction.Length);
			AssertEquals("Should be no confirmations created yet on sub", 0, subBookingInstruction.Confirmations.Count);

			var actualDivotsOnSub = subBookingInstruction.DivotsWithPackages.Select(d => (instructionPK: d.KD_KN_BookingInstruction, packagePK: d.KD_KP_Package)).ToArray();
			var expectedDivotsOnSub = new[] { (instructionPK: masterBookingInstruction.PK, packagePK: package.PK) };
			AssertContainsExactElementsInAnyOrder("Sub Instruction should have a divot between Master Instruction and package.", expectedDivotsOnSub, actualDivotsOnSub);
		}

		DtbBookingInstruction CreateBookingInstructionForMasterBookingReplicationTests(bool isMaster)
		{
			var booking = Helper.CreateBooking();
			booking.KM_IsMaster = isMaster;
			booking.KM_MasterBookingVersion = (short)(isMaster ? 1 : 0);

			var bookingInstruction = booking.Instructions.AddNew();
			bookingInstruction.KN_IsMaster = isMaster;
			bookingInstruction.KN_MasterBookingVersion = (short)(isMaster ? 1 : 0);
			bookingInstruction.KN_Sequence = 1;
			bookingInstruction.KN_InstructionType = "PIC";
			bookingInstruction.KN_DropMode = "ANY";
			bookingInstruction.KN_ServiceInstruction = string.Empty;
			bookingInstruction.KN_Status = BookingInstructionStatuses.Codes.Available;
			bookingInstruction.KN_RQ_Equipment = ZGuid.Empty;
			bookingInstruction.KN_IsContainerRateable = false;
			bookingInstruction.KN_IsLooseRateable = false;
			bookingInstruction.KN_TZ_DomesticZone = ZGuid.Empty;
			bookingInstruction.KN_IsAuthorisedToLeave = false;
			Factory.Save();

			return bookingInstruction;
		}
	}

	public class DtbBookingInstructionTest : DtbBookingTestCaseWithFactory
	{
		// persistent

		public void TestKN_InstructionType()
		{
			var booking = Helper.CreateBooking();
			var instruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var confirmation = instruction.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp);
			confirmation.KK_Actual = ZDateTime.Now;
			AssertEquals("Precondition", TransportStatuses.Codes.PickedUp, instruction.KN_Status);

			instruction.KN_InstructionType = InstructionTypes.Codes.Delivery;
			AssertEquals(TransportStatuses.Codes.Available, instruction.KN_Status);
		}

		public void TestChangeInstructionType()
		{
			var booking = Helper.CreateBooking();
			var instruction = Helper.CreateInstruction(booking, "", "", null);
			AssertEquals("No confirmations should be created without booking direction, instruction type, Org. Type and Packages.", 0, instruction.Confirmations.Count);

			instruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			AssertEquals("No confirmations should be created without booking direction, Org. Type and Packages.", 0, instruction.Confirmations.Count);
			instruction.KN_InstructionType = ""; // clean up

			booking.KM_Direction = Constants.CartageDirection.Origin;
			instruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			AssertEquals("No confirmations should be created without Org. Type and Packages.", 0, instruction.Confirmations.Count);
			instruction.KN_InstructionType = ""; // clean up

			instruction.OrganisationType = "CTO";
			instruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			AssertEquals("One confirmation should be created without Packages.", 1, instruction.Confirmations.Count);

			var divotCreatedFromInstruction = Helper.CreatePackageDivot(instruction);
			Helper.CreatePackage("p1", divotCreatedFromInstruction);
			AssertEquals("Only a pick up confirmation should be created.", 1, instruction.Confirmations.Count);
			instruction.Confirmations.Single(c => c.KK_ConfirmationType == ConfirmationTypes.Codes.PickUp);

			instruction.KN_InstructionType = InstructionTypes.Codes.Delivery;
			AssertEquals("One additional confirmation for deleivery should be added.", 2, instruction.Confirmations.Count);
			instruction.Confirmations.Single(c => c.KK_ConfirmationType == ConfirmationTypes.Codes.PickUp);
			instruction.Confirmations.Single(c => c.KK_ConfirmationType == ConfirmationTypes.Codes.Delivery);

			instruction.KN_InstructionType = InstructionTypes.Codes.Multi;
			AssertEquals("No confirmations should be added as there is no matching defaul confirmations exist with Auto Add enabled.", 2, instruction.Confirmations.Count);
			instruction.Confirmations.Single(c => c.KK_ConfirmationType == ConfirmationTypes.Codes.PickUp);
			instruction.Confirmations.Single(c => c.KK_ConfirmationType == ConfirmationTypes.Codes.Delivery);
		}

		public void TestConfirmations_GetAndCreateDefaultConfirmationsIfNotExists()
		{
			var booking = Helper.CreateBooking();
			var instruction = Helper.CreateInstruction(booking, "", "", null);

			AssertConfirmationsAllDates(instruction, InstructionTypes.Codes.PickUp, ConfirmationTypes.Codes.PickUp);
			AssertConfirmationsAllDates(instruction, InstructionTypes.Codes.Delivery, ConfirmationTypes.Codes.Delivery);
			AssertConfirmationsAllDates(instruction, InstructionTypes.Codes.Multi, ConfirmationTypes.Codes.Delivery);
		}

		void AssertConfirmationsAllDates(DtbBookingInstruction instruction, string instructionType, string expectedConfirmationType)
		{
			AssertConfirmations(instructionType, instruction, (date) => instruction.Actual = date, () => { return instruction.Confirmations.Single().KK_Actual; }, expectedConfirmationType);
			AssertConfirmations(instructionType, instruction, (date) => instruction.Estimated = date, () => { return instruction.Confirmations.Single().KK_Estimated; }, expectedConfirmationType);
			AssertConfirmations(instructionType, instruction, (date) => instruction.ReqFrom = date, () => { return instruction.Confirmations.Single().KK_RequiredFrom; }, expectedConfirmationType);
			AssertConfirmations(instructionType, instruction, (date) => instruction.ReqTo = date, () => { return instruction.Confirmations.Single().KK_RequiredTo; }, expectedConfirmationType);
		}

		void AssertConfirmations(string instructionType, DtbBookingInstruction instruction, Action<DateTime> setInstructionDate, Func<ZDateTime> getConfirmationDate, string expectedConfirmationType)
		{
			var date = DateTime.Now;
			instruction.Confirmations.DeleteAll();
			instruction.KN_InstructionType = instructionType;
			AssertEquals("Precondtion", 0, instruction.Confirmations.Count);
			setInstructionDate(date);
			AssertEquals("Create new instruction and copy the date.", 1, instruction.Confirmations.Count);
			AssertEquals(expectedConfirmationType, instruction.Confirmations.Single().KK_ConfirmationType);
			AssertEquals(date, getConfirmationDate());
			setInstructionDate(date);
			AssertEquals("Should only create when does not exists. (Create once)", 1, instruction.Confirmations.Count);
		}

		public void TestChangeInstructionType_WithExisingConfirmationForSinglePackage()
		{
			var booking = Helper.CreateBooking();
			var instruction = Helper.CreateInstruction(booking, "", "", null);
			booking.KM_Direction = Constants.CartageDirection.Origin;
			instruction.OrganisationType = "CTO";
			instruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			var divotPackage1 = Helper.CreatePackageDivot(instruction);
			var divotPackage2 = Helper.CreatePackageDivot(instruction);
			Helper.CreatePackage("p1", divotPackage1);
			Helper.CreatePackage("p2", divotPackage2);
			AssertEquals("Precondtion", 1, instruction.Confirmations.Count);
			var pickUpConfirmation = instruction.Confirmations.Single(c => c.KK_ConfirmationType == ConfirmationTypes.Codes.PickUp);
			AssertEquals("Precondtion", instruction.PK, pickUpConfirmation.ParentID_InstructionOrPackageDivot);

			pickUpConfirmation.ParentID_InstructionOrPackageDivot = divotPackage1.PK;
			instruction.KN_InstructionType = InstructionTypes.Codes.Delivery;
			AssertEquals("One additional confirmation for deleivery should be added.", 2, instruction.Confirmations.Count);
			instruction.Confirmations.Single(c => c.KK_ConfirmationType == ConfirmationTypes.Codes.PickUp && c.ParentID_InstructionOrPackageDivot == divotPackage1.PK);
			instruction.Confirmations.Single(c => c.KK_ConfirmationType == ConfirmationTypes.Codes.Delivery && c.ParentID_InstructionOrPackageDivot == instruction.PK);

			instruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			AssertEquals("One additional confirmation for pick up should be added since the existing one is not for all packages.", 3, instruction.Confirmations.Count);
			instruction.Confirmations.Single(c => c.KK_ConfirmationType == ConfirmationTypes.Codes.PickUp && c.ParentID_InstructionOrPackageDivot == divotPackage1.PK);
			instruction.Confirmations.Single(c => c.KK_ConfirmationType == ConfirmationTypes.Codes.PickUp && c.ParentID_InstructionOrPackageDivot == divotPackage2.PK);
			instruction.Confirmations.Single(c => c.KK_ConfirmationType == ConfirmationTypes.Codes.Delivery && c.ParentID_InstructionOrPackageDivot == instruction.PK);
		}

		public void TestUpdateStatus_WhenConfirmationsOnPackages()
		{
			// create consolidated booking with 2 boxes and 1 booking.
			var consolidation = Helper.CreateConsolidation();
			var box1 = consolidation.PackageJob.Packages.AddNew("BOX");
			var box2 = consolidation.PackageJob.Packages.AddNew("BOX");
			var booking = consolidation.Bookings.AddNew();

			// assign the packages to an instruction
			var instruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			instruction.DivotsWithPackages.AddPackage(box1);
			instruction.DivotsWithPackages.AddPackage(box2);

			var divot1 = instruction.PackageDivots.Single(d => d.KD_KP_Package == box1.PK);
			var divot2 = instruction.PackageDivots.Single(d => d.KD_KP_Package == box2.PK);

			// add 1 completed PickUp confirmation to box 1
			var box1_pickUp_conf = divot1.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp);
			box1_pickUp_conf.KK_Actual = ZDateTime.Now;
			AssertEquals("Only 1 of 2 Packages has a completed PickUp Confirmation, Status should fallback to Available.", TransportStatuses.Codes.Available, instruction.KN_Status);

			// add 1 incomplete PickUp confirmation to box 2
			var box2_pickUp_conf = divot2.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp);
			AssertEquals("Only 1 of 2 Packages has a completed PickUp Confirmation, Status should fallback to Available.", TransportStatuses.Codes.Available, instruction.KN_Status);

			// complete the pickUp confirmation on box 2
			box2_pickUp_conf.KK_Actual = ZDateTime.Now;
			AssertEquals(TransportStatuses.Codes.PickedUp, instruction.KN_Status);

			// change the instruction to DLV and add 1 completed Delivery confirmation to box 1, and 1 incomplete Delivery confirmation to box 2
			instruction.KN_InstructionType = InstructionTypes.Codes.Delivery;
			var box1_delivery_conf = divot1.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);
			var box2_delivery_conf = divot2.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);
			box1_delivery_conf.KK_Actual = ZDateTime.Now;
			AssertEquals("Only 1 of 2 Packages has a completed Delivery Confirmation, Status should fallback to PickUp.", TransportStatuses.Codes.Available, instruction.KN_Status);

			// complete the delivery confirmation on box 2
			box2_delivery_conf.KK_Actual = ZDateTime.Now;
			AssertEquals(TransportStatuses.Codes.Delivered, instruction.KN_Status);
		}

		public void TestUpdateStatus_EditWhileNonCommitted()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			var instruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);

			var confirmations = (IBindingList)instruction.Confirmations;       //
			var confirmation = (DtbBookingConfirmation)confirmations.AddNew(); // simulate adding a non-committed row

			confirmation.KK_ConfirmationType = ConfirmationTypes.Codes.PickUp;
			confirmation.KK_Actual = ZDateTime.Now;
			AssertEquals("Instruction FK added, works without event when adding a non-committed", "PIC", instruction.KN_Status);

			((ICancelAddNew)confirmations).EndNew(0);
			AssertEquals("PIC", instruction.KN_Status);
		}

		// calculated

		public void TestChangeOrganizationType()
		{
			var booking = Helper.CreateBooking();
			var instruction = Helper.CreateInstruction(booking, "", "", null);
			booking.KM_Direction = Constants.CartageDirection.Origin;
			instruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			instruction.OrganisationType = "CFS";
			AssertEquals("Precondition", 1, instruction.Confirmations.Count);

			var divotCreatedFromInstruction = Helper.CreatePackageDivot(instruction);
			Helper.CreatePackage("p1", divotCreatedFromInstruction);
			AssertEquals("One confirmation for pick up should be created.", 1, instruction.Confirmations.Count);
			instruction.Confirmations.Single(c => c.KK_ConfirmationType == ConfirmationTypes.Codes.PickUp);

			instruction.OrganisationType = "CTO";
			AssertEquals("No additional confirmation for Slot should be added.", 1, instruction.Confirmations.Count);
			instruction.Confirmations.Single(c => c.KK_ConfirmationType == ConfirmationTypes.Codes.PickUp);

			instruction.KN_InstructionType = "WHS";
			AssertEquals("No confirmations should be added as there is no matching default confirmations exist with Auto Add enabled.", 1, instruction.Confirmations.Count);
			instruction.Confirmations.Single(c => c.KK_ConfirmationType == ConfirmationTypes.Codes.PickUp);
		}

		public void TestChangeOrganizationType_WithExistingConfirmationForPackage()
		{
			var booking = Helper.CreateBooking();
			var instruction = Helper.CreateInstruction(booking, "", "", null);
			booking.KM_Direction = Constants.CartageDirection.Origin;
			instruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			instruction.OrganisationType = "CFS";
			var divot1 = Helper.CreatePackageDivot(instruction);
			var divot2 = Helper.CreatePackageDivot(instruction);
			Helper.CreatePackage("p1", divot1);
			Helper.CreatePackage("p1", divot2);
			AssertEquals("Precondtion", 1, instruction.Confirmations.Count);
			var confirmationForPickup = instruction.Confirmations.Single(c => c.KK_ConfirmationType == ConfirmationTypes.Codes.PickUp);
			AssertEquals("Precondition", instruction.PK, confirmationForPickup.ParentID_InstructionOrPackageDivot);

			confirmationForPickup.ParentID_InstructionOrPackageDivot = divot1.PK;
			instruction.OrganisationType = "CTO";
			AssertEquals("One additional confirmation for pick up should be added.", 2, instruction.Confirmations.Count);
			instruction.Confirmations.Single(c => c.KK_ConfirmationType == ConfirmationTypes.Codes.PickUp && c.ParentID_InstructionOrPackageDivot == divot1.PK);
			instruction.Confirmations.Single(c => c.KK_ConfirmationType == ConfirmationTypes.Codes.PickUp && c.ParentID_InstructionOrPackageDivot == divot2.PK);

			instruction.KN_InstructionType = "WHS";
			AssertEquals("No confirmations should be added as there is no matching default confirmations exist with Auto Add enabled.", 2, instruction.Confirmations.Count);
			instruction.Confirmations.Single(c => c.KK_ConfirmationType == ConfirmationTypes.Codes.PickUp && c.ParentID_InstructionOrPackageDivot == divot1.PK);
			instruction.Confirmations.Single(c => c.KK_ConfirmationType == ConfirmationTypes.Codes.PickUp && c.ParentID_InstructionOrPackageDivot == divot2.PK);
		}

		public void TestChangeOrganizationType_SetsDefaultAddress_RespectsBookingContainerLinksAndNumbers()
		{
			using (var dummyWriterDecider = ObjectFactory.Get<IDtbParentInfoLoader>().SetDummyWriterDecider(populateActualDates: true))
			{
				var day = ZDateTime.Today;
				var container1 = Factory.New<PkgPackage>();
				var container2 = Factory.New<PkgPackage>();
				container1.KP_F3_NKPackType = "CNT";
				container1.KP_PackageID = "CONT1";

				container2.KP_F3_NKPackType = "CNT";
				container2.KP_PackageID = "CONT2";

				var callsToUpdateAddressWithContainerReferences = new List<Tuple<JobDocAddress, bool, IEnumerable<ZInt>, IEnumerable<ZString>>>();
				var consolidationParentInfo = new Mock<IDtbParentInfo>();
				// for some strange reason if there were no calls the verifiable check on the enumerable parameters throws a StackOverflow exception, so I've opted to verify the call to the new override of UpdateAddress() the 'hard way'
				consolidationParentInfo
					.Setup(p => p.UpdateAddress(It.IsAny<JobDocAddress>(), It.IsAny<bool>(), It.IsAny<IEnumerable<ZInt>>(), It.IsAny<IEnumerable<ZString>>()))
					.Callback((JobDocAddress address, bool allowOverride, IEnumerable<ZInt> links, IEnumerable<ZString> numbers) =>
					{
						callsToUpdateAddressWithContainerReferences.Add(Tuple.Create(address, allowOverride, links, numbers));
					});
				consolidationParentInfo.Setup(p => p.GetDatesAndReferences(It.IsAny<ZString>(), It.IsAny<ZString>(), It.IsAny<IPkgPackage>(), It.IsAny<IReadOnlyDictionary<PkgPackage, ZString>>())).Returns(DatesAndReference.Empty);
				consolidationParentInfo.SetupGet(p => p.DropMode).Returns((ZString?)null);
				var consolidation = Helper.CreateConsolidation();
				consolidation.SetParentInfo(consolidationParentInfo.Object);
				consolidation.KB_JobDirection = TransportCommon.Shared.Directions.GetDirectionCodeFromDtbBookingDirection(DtbBookingDirection.DLV);

				var booking = consolidation.Bookings.AddNew();

				var containerList = new List<UniversalDataBuss.DataObjects.Universal.Container>()
				{
					new UniversalDataBuss.DataObjects.Universal.Container() { ContainerNumber = "CONT2", Link = 2 }
				};

				booking.SetContainerLinksAndNumbers(containerList);
				var instruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp, "CFS");
				var calls = callsToUpdateAddressWithContainerReferences.Count;
				AssertGreaterThanOrEqualTo("Override of UpdateAddress with container links and numbers should be called at least once", calls, callsToUpdateAddressWithContainerReferences.Count);

				var lastCall = callsToUpdateAddressWithContainerReferences.Last();
				AssertNotNull("UpdateAddress() should have been called with a non-null list of container links", lastCall.Item3);
				AssertEquals("UpdateAddress() should have been called with one container link", 1, lastCall.Item3.Count());
				AssertEquals("UpdateAddress() should have been called with a container link 2", 2, lastCall.Item3.Single());

				AssertNotNull("UpdateAddress() should have been called with a non-null list of container numbers", lastCall.Item4);
				AssertEquals("UpdateAddress() should have been called with one container number", 1, lastCall.Item4.Count());
				AssertEquals("UpdateAddress() should have been called with container number 'CONT2'", "CONT2", lastCall.Item4.Single());
			}
		}

		public void TestStatusDescription()
		{
			var booking = Helper.CreateBooking();
			var instruction = booking.Instructions.AddNew();
			AssertEquals(TransportStatuses.Descriptions.Available, instruction.StatusDescription);

			instruction.KN_Status = TransportStatuses.Codes.Held;
			AssertEquals(TransportStatuses.Descriptions.Held, instruction.StatusDescription);

			instruction.KN_Status = "xXx";
			AssertEquals("", instruction.StatusDescription);
		}

		// view panel

		public void TestEstimated()
		{
			var booking = Helper.CreateBooking();
			var instruction = booking.Instructions.AddNew();
			AssertEquals("No confirmations, no date.", ZDateTime.Empty, instruction.Estimated);

			var pickUpConfirmation = instruction.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp);
			var deliveryConfirmation = instruction.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);
			AssertEquals("Confirmations with no date.", ZDateTime.Empty, instruction.Estimated);

			pickUpConfirmation.KK_Estimated = ZDateTime.Now;
			deliveryConfirmation.KK_Estimated = ZDateTime.Now.AddDays(1);
			AssertEquals("Should take latest date.", deliveryConfirmation.KK_Estimated, instruction.Estimated);
		}

		public void TestActual()
		{
			var booking = Helper.CreateBooking();
			var instruction = booking.Instructions.AddNew();
			AssertEquals("No confirmations, no date.", ZDateTime.Empty, instruction.Actual);

			var pickUpConfirmation = instruction.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp);
			var deliveryConfirmation = instruction.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);
			AssertEquals("Confirmations with no date.", ZDateTime.Empty, instruction.Actual);

			pickUpConfirmation.KK_Actual = ZDateTime.Now;
			deliveryConfirmation.KK_Actual = ZDateTime.Now.AddDays(1);
			AssertEquals("Should take latest date.", deliveryConfirmation.KK_Actual, instruction.Actual);
		}

		public void TestSignedBy()
		{
			var booking = Helper.CreateBooking();
			var instruction = booking.Instructions.AddNew();
			AssertEquals("No confirmations, no date.", "", instruction.SignedBy);

			var pickUpConfirmation = instruction.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp);
			var deliveryConfirmation = instruction.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);
			AssertEquals("Confirmations with no date.", "", instruction.SignedBy);

			pickUpConfirmation.KK_Actual = ZDateTime.Now;
			pickUpConfirmation.KK_ReceivedBy = "Bob";
			deliveryConfirmation.KK_Actual = ZDateTime.Now.AddDays(1);
			deliveryConfirmation.KK_ReceivedBy = "Ted";
			AssertEquals("Should take latest date.", "Ted", instruction.SignedBy);
		}

		public void TestPackageCategoryDescription()
		{
			var booking = Helper.CreateBooking();
			var instruction = booking.Instructions.AddNew();
			AssertEquals("Should be blank", "", instruction.PackageCategoryDescription);

			instruction.PackageCategory = "CNT";
			AssertEquals("Should be Containers", "Containers", instruction.PackageCategoryDescription);

			instruction.PackageCategory = "LSE";
			AssertEquals("Should be Containers", "Loose Outer Packages", instruction.PackageCategoryDescription);

			instruction.PackageCategory = "BTH";
			AssertEquals("Should be Containers", "Containers and their Loose Outer Packages", instruction.PackageCategoryDescription);
		}

		public void TestPackageCategoryDescription_Loaded()
		{
			var consolidation = Helper.CreateConsolidation();
			var container = Helper.CreatePackageContainer("");
			consolidation.PackageJob.Packages.Add(container);
			var box = container.Packages.AddNew("BOX");
			var booking = consolidation.Bookings.AddNew();
			var instruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			instruction.DivotsWithPackages.AddPackage(container);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedInstruction = newFactory.Load<DtbBookingInstruction>(instruction.PK);
			AssertEquals("Should be Containers", "Containers", loadedInstruction.PackageCategoryDescription);

			instruction.DivotsWithPackages.RemovePackage(container);
			instruction.DivotsWithPackages.AddPackage(box);
			Factory.Save();
			newFactory = new BusinessObjectFactory();
			loadedInstruction = newFactory.Load<DtbBookingInstruction>(instruction.PK);
			AssertEquals("Should be Containers", "Loose Outer Packages", loadedInstruction.PackageCategoryDescription);

			instruction.DivotsWithPackages.AddPackage(container);
			Factory.Save();
			newFactory = new BusinessObjectFactory();
			loadedInstruction = newFactory.Load<DtbBookingInstruction>(instruction.PK);
			AssertEquals("Should be Containers", "Containers and their Loose Outer Packages", loadedInstruction.PackageCategoryDescription);
		}

		public void TestIsComplete()
		{
			var instruction = Factory.New<DtbBookingInstruction>();
			AssertEquals(false, instruction.IsComplete);
			AssertEquals(false, instruction.IsCompleteDeliveryOnly);

			instruction.KN_InstructionType = InstructionTypes.Codes.Delivery;
			instruction.KN_Status = TransportStatuses.Codes.PickedUp; // should never really happen
			AssertEquals(false, instruction.IsComplete);
			AssertEquals(false, instruction.IsCompleteDeliveryOnly);

			instruction.KN_InstructionType = InstructionTypes.Codes.Delivery;
			instruction.KN_Status = TransportStatuses.Codes.Delivered;
			AssertEquals(true, instruction.IsComplete);
			AssertEquals(true, instruction.IsCompleteDeliveryOnly);

			instruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			instruction.KN_Status = TransportStatuses.Codes.PickedUp;
			AssertEquals(true, instruction.IsComplete);
			AssertEquals(false, instruction.IsCompleteDeliveryOnly);

			instruction.KN_InstructionType = InstructionTypes.Codes.Multi;//DeliveryAndPickup;
			instruction.KN_Status = TransportStatuses.Codes.Delivered;
			AssertEquals(false, instruction.IsComplete);
			AssertEquals(true, instruction.IsCompleteDeliveryOnly);

			instruction.KN_InstructionType = InstructionTypes.Codes.Multi;//DeliveryAndPickup;
			instruction.KN_Status = TransportStatuses.Codes.PickedUp;
			AssertEquals(true, instruction.IsComplete);
			AssertEquals(true, instruction.IsCompleteDeliveryOnly);
		}

		public void TestIsContainerised()
		{
			var instruction = Factory.New<DtbBookingInstruction>();
			AssertEquals(false, instruction.IsContainerised);

			var package = Factory.New<PkgPackage>();
			instruction.DivotsWithPackages.AddPackage(package);
			AssertEquals(false, instruction.IsContainerised);

			var container = Factory.New<PkgPackage>();
			container.KP_F3_NKPackType = "CNT";
			instruction.DivotsWithPackages.AddPackage(container);
			AssertEquals("All packs need to be containerised", false, instruction.IsContainerised);

			var packageDivot = instruction.PackageDivots.First(d => d.KD_KP_Package == package.PK);
			packageDivot.Delete();
			AssertEquals("All packs are now containerised", true, instruction.IsContainerised);
		}

		public void TestIsEmptyYard()
		{
			var instruction = Factory.New<DtbBookingInstruction>();
			AssertEquals(false, instruction.IsEmptyYard);

			instruction.OrganisationType = OrganisationTypesList.Codes.CYD;
			AssertEquals(true, instruction.IsEmptyYard);
		}

		public void TestClone()
		{
			var org = Helper.CreateOrganisation("ORG");
			var booking = Helper.CreateBooking();
			var instruction = booking.Instructions.AddNew();
			instruction.OrganisationType = "CFS";
			instruction.Address.E2_OA_Address = org.MainAddress.PK;

			var allConfirmation = instruction.Confirmations.AddNew();
			allConfirmation.KK_ConfirmationType = ConfirmationTypes.Codes.PickUp;
			var packageDivot = instruction.PackageDivots.AddNew();
			var packageConfirmation = Factory.New<DtbBookingConfirmation>();
			packageConfirmation.KK_KD_BookingInstructionPkgDivot = packageDivot.PK;

			AssertEquals(true, instruction.SupportsClone());

			var clone = (DtbBookingInstruction)instruction.Clone();
			AssertNotEquals("Should have its new sequence", instruction.KN_Sequence, clone.KN_Sequence);
			AssertEquals("Should have cloned the docaddress", "ORG", clone.Address.Organisation.OH_Code);
			AssertEquals("Should have cloned the org type", "CFS", clone.OrganisationType);
			AssertEquals("Do not clone Packages or their divots", 0, clone.PackageDivots.Count);
			AssertEquals("Should have cloned 1 confirmation", 1, clone.Confirmations.Count);
			AssertEquals(ConfirmationTypes.Codes.PickUp, clone.Confirmations[0].KK_ConfirmationType);
		}

		public void TestSplit()
		{
			var org = Helper.CreateOrganisation("ORG");
			var booking = Helper.CreateBooking();
			var cfsInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, "CFS", org.MainAddress);
			AssertEquals("Ensure no split has occured yet", 1, booking.Instructions.Count);

			var divot_p1 = Helper.CreatePackageDivot(cfsInstruction, 1);
			var p1 = Helper.CreatePackage("p1", divot_p1);
			var cfsConfirmation_ALL = Helper.CreateConfirmation(cfsInstruction, ConfirmationTypes.Codes.PickUp);
			var cfsConfirmation_p1 = Helper.CreateConfirmation(divot_p1, ConfirmationTypes.Codes.Delivery);

			AssertEquals("Ensure booking has packages collection setup", 1, booking.Packages_PackageView.Count);

			// setup package view - An instruction with a divot to a package
			booking.SetInstructionViewAndSelectedPackage(TransportBookingInstructionView.Package, booking.Packages_PackageView.Find(p1));

			cfsInstruction.OrganisationType = "CTO";
			AssertEquals("Ensure change has been made", "CTO", cfsInstruction.OrganisationType);
			AssertEquals("Ensure no split has occured cause there is only 1 package", 1, booking.Instructions.Count);

			cfsInstruction.OrganisationType = "CFS";
			AssertEquals("Ensure change has been made again", "CFS", cfsInstruction.OrganisationType);
			AssertEquals("Ensure no split has occured cause there is only 1 package", 1, booking.Instructions.Count);

			// An instruction with 2 Divots, each with a different Package
			var divot_p2 = Helper.CreatePackageDivot(cfsInstruction, 1);
			var p2 = Helper.CreatePackage("p2", divot_p2);
			var cfsConfirmation_p2 = Helper.CreateConfirmation(divot_p2, ConfirmationTypes.Codes.Delivery);
			AssertEquals("Booking still has 1 Instruction", 1, booking.Instructions.Count);
			AssertEquals("Booking still has the CFS Instruction", cfsInstruction, booking.Instructions[0]);
			AssertEquals("Divot p1 links to the correct instruction", cfsInstruction, divot_p1.Instruction);
			AssertEquals("Divot p2 links to the correct instruction", cfsInstruction, divot_p2.Instruction);
			AssertEquals("Confirmation ALL links to the correct instruction", cfsInstruction, cfsConfirmation_ALL.Instruction);
			AssertEquals("Confirmation p1 links to the correct instruction", cfsInstruction, cfsConfirmation_p1.Instruction);
			AssertEquals("Confirmation p2 links to the correct instruction", cfsInstruction, cfsConfirmation_p2.Instruction);

			// p1 is selected in the view. So changing here will only change the instruction for p1. All Confirmations should be cloned.
			cfsInstruction.OrganisationType = "CTO";
			AssertEquals("Ensure change has been made", "CTO", cfsInstruction.OrganisationType);
			AssertEquals("Ensure a split has occured as p2 should have it's own instruction", 2, booking.Instructions.Count);
			var newInstruction = booking.Instructions[1];
			AssertEquals("Divot p1 links to the correct instruction", cfsInstruction, divot_p1.Instruction);
			AssertEquals("Divot p2 links to the correct instruction", newInstruction, divot_p2.Instruction);
			AssertEquals("Confirmation ALL links to the correct instruction", cfsInstruction, cfsConfirmation_ALL.Instruction);
			AssertEquals("Confirmation p1 links to the correct instruction", cfsInstruction, cfsConfirmation_p1.Instruction);
			AssertEquals("Confirmation p2 links to the correct instruction", newInstruction, cfsConfirmation_p2.Instruction);
			AssertEquals("The New Instruction should have a cloned ALL confirmation", 1, newInstruction.Confirmations.Count(c => c.PackageDivot == null));

			AssertEquals("New Instruction should have the original value CFS", "CFS", divot_p2.Instruction.OrganisationType);
			AssertEquals("p2 confirmation should have moved with package", divot_p2.Instruction, cfsConfirmation_p2.Instruction);
			AssertEquals("p2 should also have an ALL confirmation", 2, divot_p2.Instruction.Confirmations.Count);
		}

		public void TestFetchStrategy()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			var instruction = booking.Instructions.AddNew();
			AssertNotNull(instruction.FetchStrategy);
			AssertEquals(typeof(DtbBookingInstructionFetchStrategy), instruction.FetchStrategy.GetType());
		}

		public void TestDefaultDatesAndReferencesFromParent()
		{
			using (var dummyWriterDecider = ObjectFactory.Get<IDtbParentInfoLoader>().SetDummyWriterDecider(populateActualDates: true))
			{
				var day = ZDateTime.Today;

				// dif org type
				// same description + same org Type + dif container and loose but loose has same dates

				var cont1 = Factory.New<PkgPackage>();
				var cont2 = Factory.New<PkgPackage>();
				var loose1 = Factory.New<PkgPackage>();
				var loose2 = Factory.New<PkgPackage>();

				cont1.KP_F3_NKPackType = "CNT";
				cont1.KP_PackageID = "CONT123";

				cont2.KP_F3_NKPackType = "CNT";
				cont2.KP_PackageID = "CONT456";

				loose1.KP_F3_NKPackType = "PLT";
				loose1.KP_PackageID = "Loose1";

				loose2.KP_F3_NKPackType = "PLT";
				loose2.KP_PackageID = "Loose2";

				var data = TransportBookingTestCache.Instance.Data;

				var transportBookingParent = Factory.New<DummyWithDtbBooking>();
				var consolidation = Helper.CreateConsolidation();
				consolidation.KB_ParentID = transportBookingParent.PK;
				consolidation.KB_ParentTableCode = transportBookingParent.TablePrefix;
				consolidation.KB_JobDirection = Enterprise.TransportCommon.Shared.Directions.GetDirectionCodeFromDtbBookingDirection(DtbBookingDirection.DLV);

				var booking = consolidation.Bookings.AddNew();

				// Add CTO Instruction - should have no confimations assgined
				var instruction = booking.Instructions.AddNew();
				instruction.OrganisationType = OrganisationTypesList.Codes.CTO;
				AssertEquals("Has No Packages assigned", 0, instruction.Confirmations.Count);

				// Add container 1 to CTO Instruction - Container 1 has a Pickup d&r - but because it's only 1 package so it should be ALL (attached to the instruction)
				instruction.DivotsWithPackages.AddPackage(cont1);
				var divotCTO_Cont1 = instruction.PackageDivots.Single(d => d.KD_KP_Package == cont1.PK);
				AssertEquals(1, instruction.Confirmations.Count);
				AssertConfirmation(instruction.Confirmations, ConfirmationTypes.Codes.PickUp, ZDateTime.Empty, data["container1.FCLWharfGateOut"], data["container1.FCLAvailable"], data["container1.FCLStorageCommences"], data["container1.ContainerImportDORelease"], data["container1.ArrivalSlotDateTime"], data["container1.ArrivalSlotReference"].ToString()); // container overrides shipment/transport dates
			}
		}

		public void TestDefaultDatesAndReferencesFromParentWhsReceive()
		{
			var day = ZDateTime.Today;
			var parentReceive = (BusinessObject)Factory.New<IWhsReceive>();
			parentReceive[WhsDocketSchema.WD_ETD] = day;
			parentReceive[WhsDocketSchema.WD_ETA] = day.AddDays(1);
			parentReceive[WhsDocketSchema.WD_ArrivalDate] = day.AddDays(2);

			var consolidation = Helper.CreateConsolidation(parentReceive as IDtbBookingParent);
			var booking = consolidation.Bookings.AddNew();
			var instruction1 = booking.Instructions.AddNew();
			instruction1.KN_InstructionType = "PIC";
			instruction1.OrganisationType = OrganisationTypesList.Codes.CNR;
			AssertEquals("Precondition", 1, instruction1.Confirmations.Count);
			AssertConfirmation(instruction1.Confirmations, ConfirmationTypes.Codes.PickUp, day, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZString.Empty, ZDateTime.Empty, ZString.Empty);

			var instruction2 = booking.Instructions.AddNew();
			instruction2.KN_InstructionType = "DLV";
			instruction2.OrganisationType = OrganisationTypesList.Codes.WHS;
			AssertEquals("Precondition", 1, instruction2.Confirmations.Count);
			AssertConfirmation(instruction2.Confirmations, ConfirmationTypes.Codes.Delivery, day.AddDays(1), day.AddDays(2), ZDateTime.Empty, ZDateTime.Empty, ZString.Empty, ZDateTime.Empty, ZString.Empty);

			var instruction3 = booking.Instructions.AddNew();
			instruction3.KN_InstructionType = "DLV";
			AssertEquals("Precondition", 0, instruction3.Confirmations.Count);
			((ISupportDataImporting)booking).IsImportingData = true;
			instruction3.OrganisationType = OrganisationTypesList.Codes.WHS;
			AssertEquals("No confirmations should be added when there are no packages, because they will be added later in the UXML Import.", 0, instruction3.Confirmations.Count);
		}

		void AssertConfirmation(DtbBookingConfirmationCollection confirms, ZString confirmationTypeCode,
			IZType expectEstimated, IZType expectActual, IZType expectReqFrom,
			IZType expectReqTo, IZType expectReference, IZType expectSlotDateTime, ZString expectSlotReference)
		{
			var foundConfirms = confirms.Where(c => c.KK_ConfirmationType == confirmationTypeCode);
			AssertEquals(1, foundConfirms.Count());

			var confirm = foundConfirms.First();

			AssertEquals("Estimated", expectEstimated, confirm.KK_Estimated);
			AssertEquals("Actual", expectActual, confirm.KK_Actual);
			AssertEquals("ReqFrom", expectReqFrom, confirm.KK_RequiredFrom);
			AssertEquals("ReqTo", expectReqTo, confirm.KK_RequiredTo);
			AssertEquals("Reference", expectReference, confirm.KK_ReferenceNum);
			AssertEquals("SlotDateTime", expectSlotDateTime, confirm.KK_SlotDateTime);
			AssertEquals("SlotReference", expectSlotReference, confirm.KK_SlotReference);
		}

		[TestedType(typeof(DtbBookingInstruction))]
		class CustomFieldsTest : TestICustomFieldProvider
		{
			protected override BusinessObject GetBizo()
			{
				var helper = new TransportBookingTestHelper(Factory);
				var template = helper.CreateTransportBookingTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import);
				helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CTO, InstructionTypes.Codes.PickUp);
				helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CNE, InstructionTypes.Codes.Delivery);
				helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CYD, InstructionTypes.Codes.Delivery);

				var booking = helper.CreateBooking();
				booking.KM_KT_NKBookingTemplate = template.KT_Code;

				return booking.Instructions[0];
			}
		}

		public void TestICustomFieldProvider_GetCustomBusinessObject()
		{
			var template = Helper.CreateTransportBookingTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import);
			Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CTO, InstructionTypes.Codes.PickUp);
			Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CNE, InstructionTypes.Codes.Delivery);
			Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CYD, InstructionTypes.Codes.Delivery);

			var booking = Helper.CreateBooking();
			booking.KM_KT_NKBookingTemplate = template.KT_Code;

			var instruction = booking.Instructions[0];

			var workflowTemplate = Helper.CreateWorkflowTemplate(WorkflowDescriptors.DtbBookingInstructionWorkflowDescriptorCode);
			Helper.AddCustomField(workflowTemplate, "stringField", AddOnColumnDataType.Codes.String);
			Helper.AddCustomField(workflowTemplate, "intField", AddOnColumnDataType.Codes.Integer);
			Helper.AddCustomField(workflowTemplate, "dateTimeField", AddOnColumnDataType.Codes.Datetime);
			Helper.AddCustomField(workflowTemplate, "boolField", AddOnColumnDataType.Codes.Boolean);

			Factory.Save();

			var transportBookingInstructionProvider = (ICustomFieldProvider)instruction;
			var transportBookingInstructionCustomBizo = transportBookingInstructionProvider.GetCustomBusinessObject();
			var transportBookingInstructionDynamicBizo = (IDynamicBusinessObject)transportBookingInstructionCustomBizo;

			AssertNotNull(transportBookingInstructionDynamicBizo.GetProperty("__STRINGFIELD__prop__ZString"));
			AssertNotNull(transportBookingInstructionDynamicBizo.GetProperty("__INTFIELD__prop__ZInt"));
			AssertNotNull(transportBookingInstructionDynamicBizo.GetProperty("__DATETIMEFIELD__prop__ZDateTime"));
			AssertNotNull(transportBookingInstructionDynamicBizo.GetProperty("__BOOLFIELD__prop__ZBool"));
		}

		public void TestCustomBusinessObject_SubDoesNotLookToMasterForCustomFields()
		{
			var subInstruction = Helper.CreateInstruction(Helper.CreateBooking());
			var masterInstruction = Helper.CreateInstruction(Helper.CreateBooking());
			subInstruction.KN_KN_MasterBookingInstruction = masterInstruction.PK;

			var masterCustomFieldProvider = masterInstruction as ICustomFieldProvider;
			var subCustomFieldProvider = subInstruction as ICustomFieldProvider;

			AssertEquals("Precondition: subInstruction IsSub", true, subInstruction.IsSub);
			AssertEquals("Sub Instruction should refer to itself for custom fields.", subInstruction.PK, subCustomFieldProvider.GetCustomBusinessObject().Parent.PK);
		}

		public void TestContainerProperties()
		{
			// Instruction with no packages/containers
			var booking = Helper.CreateBooking();
			var instruction = booking.Instructions.AddNew();
			AssertEquals("", instruction.PackageContainerID);
			AssertEquals(0m, instruction.PackageQty);
			AssertEquals("", instruction.PackageType);
			AssertEquals("", instruction.ContainerType);
			AssertEquals("", instruction.ContainerMode);

			// Instruction with package but no container
			var package1 = Helper.CreatePackage("AAA", 1, "XXX");
			instruction.DivotsWithPackages.AddPackage(package1);
			AssertEquals("AAA", instruction.PackageContainerID);
			AssertEquals(1m, instruction.PackageQty);
			AssertEquals("XXX", instruction.PackageType);
			AssertEquals("", instruction.ContainerType);
			AssertEquals("", instruction.ContainerMode);

			// Instruction with container
			package1.KP_F3_NKPackType = "CNT";
			var ref20GP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			package1.Container.K0_RC_ContainerType = ref20GP.PK;
			package1.Container.K0_ContainerMode = "DAV";
			AssertEquals("AAA", instruction.PackageContainerID);
			AssertEquals(1m, instruction.PackageQty);
			AssertEquals("CNT", instruction.PackageType);
			AssertEquals("20GP", instruction.ContainerType);
			AssertEquals("DAV", instruction.ContainerMode);

			// Instruction with package and container with different types
			var package2 = Helper.CreatePackage("", 10, "BOX");
			instruction.DivotsWithPackages.AddPackage(package2);
			AssertEquals("Many", instruction.PackageContainerID);
			AssertEquals(11m, instruction.PackageQty);
			AssertEquals("Many", instruction.PackageType);
			AssertEquals("20GP", instruction.ContainerType);
			AssertEquals("DAV", instruction.ContainerMode);

			// Instruction with package and container with same types
			package2.KP_PackageID = "BBB";
			package2.KP_PackageQty = 1;
			package2.KP_F3_NKPackType = "CNT";
			package2.Container.K0_RC_ContainerType = ref20GP.PK;
			package2.Container.K0_ContainerMode = "DAV";
			AssertEquals("AAA, BBB", instruction.PackageContainerID);
			AssertEquals(2m, instruction.PackageQty);
			AssertEquals("CNT", instruction.PackageType);
			AssertEquals("20GP", instruction.ContainerType);
			AssertEquals("DAV", instruction.ContainerMode);

			package2.KP_PackageID = "";
			AssertEquals("AAA", instruction.PackageContainerID);

			package1.KP_PackageID = "";
			AssertEquals("", instruction.PackageContainerID);
		}

		public void TestCreateDefaultConfirmation_WithoutPackageDivot()
		{
			var instructionWithoutBooking = Helper.CreateInstruction();
			instructionWithoutBooking.CreateDefaultConfirmation();
			AssertEquals("No confirmations should be created without a booking.", 0, instructionWithoutBooking.Confirmations.Count);

			var booking = Helper.CreateBooking();
			var instruction = Helper.CreateInstruction(booking, "", "", null);
			instruction.CreateDefaultConfirmation();
			AssertEquals("No confirmations should be created without booking direction, instruction type, Org. Type and Packages.", 0, instruction.Confirmations.Count);

			instruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			instruction.CreateDefaultConfirmation();
			AssertEquals("No confirmations should be created without booking direction, Org. Type and Packages.", 0, instruction.Confirmations.Count);

			instruction.OrganisationType = "CTO";
			instruction.CreateDefaultConfirmation();
			AssertEquals("No confirmations should be created without Packages.", 0, instruction.Confirmations.Count);

			var divot1 = Helper.CreatePackageDivot(instruction);
			Helper.CreatePackage("p1", divot1);
			var divot2 = Helper.CreatePackageDivot(instruction);
			Helper.CreatePackage("p2", divot2);
			AssertEquals("No confirmations should be created booking direction.", 0, instruction.Confirmations.Count);

			booking.KM_Direction = Constants.CartageDirection.Origin;
			instruction.CreateDefaultConfirmation();
			AssertEquals("One confirmation should be created.", 1, instruction.Confirmations.Count);
			var pickupConfirmation = instruction.Confirmations.Single(c => c.KK_ConfirmationType == ConfirmationTypes.Codes.PickUp);

			pickupConfirmation.ParentID_InstructionOrPackageDivot = divot1.PK;
			instruction.CreateDefaultConfirmation();
			AssertEquals("Two confirmations should be created.", 2, instruction.Confirmations.Count);
			instruction.Confirmations.Single(c => c.KK_ConfirmationType == ConfirmationTypes.Codes.PickUp && c.ParentID_InstructionOrPackageDivot == divot1.PK);
			instruction.Confirmations.Single(c => c.KK_ConfirmationType == ConfirmationTypes.Codes.PickUp && c.ParentID_InstructionOrPackageDivot == divot2.PK);
		}

		public void TestCreateDefaultConfirmation_WithPackageDivot()
		{
			var booking = Helper.CreateBooking();
			var instruction = Helper.CreateInstruction(booking, "", "", null);
			instruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			instruction.OrganisationType = "CTO";
			var divot1 = Helper.CreatePackageDivot(instruction);
			var divot2 = Helper.CreatePackageDivot(instruction);
			Helper.CreatePackage("p1", divot1);
			Helper.CreatePackage("p3", divot2);
			booking.KM_Direction = Constants.CartageDirection.Origin;
			AssertEquals("Precondtion", 0, instruction.Confirmations.Count);

			instruction.CreateDefaultConfirmation();
			AssertEquals("One confirmation should be created.", 1, instruction.Confirmations.Count);
			var pickupConfirmation = instruction.Confirmations.Single(c => c.KK_ConfirmationType == ConfirmationTypes.Codes.PickUp);

			pickupConfirmation.ParentID_InstructionOrPackageDivot = divot1.PK;
			instruction.CreateDefaultConfirmation();
			AssertEquals("Two confirmations should be created.", 2, instruction.Confirmations.Count);
			instruction.Confirmations.Single(c => c.KK_ConfirmationType == ConfirmationTypes.Codes.PickUp && c.ParentID_InstructionOrPackageDivot == divot1.PK);
			instruction.Confirmations.Single(c => c.KK_ConfirmationType == ConfirmationTypes.Codes.PickUp && c.ParentID_InstructionOrPackageDivot == divot2.PK);
		}

		public void TestCreateDefaultConfirmation_WhenInstructionIsSub()
		{
			var booking = Helper.CreateBooking();
			var instruction = Helper.CreateInstruction(booking, "", "", null);
			instruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			instruction.OrganisationType = "CTO";
			var divot1 = Helper.CreatePackageDivot(instruction);
			var divot2 = Helper.CreatePackageDivot(instruction);
			Helper.CreatePackage("p1", divot1);
			Helper.CreatePackage("p3", divot2);
			booking.KM_Direction = Constants.CartageDirection.Origin;
			instruction.KN_KN_MasterBookingInstruction = ZGuid.NewZGuid();

			Assert("Precondition: Instruction should be a sub.", instruction.IsSub);
			AssertEquals("Precondition: No confirmations should exist yet.", 0, instruction.Confirmations.Count);

			instruction.CreateDefaultConfirmation();

			AssertEquals("No confirmations should have been created.", 0, instruction.Confirmations.Count);
		}

		public void TestCreateDefaultConfirmation_WhenInstructionIsNotSub()
		{
			var booking = Helper.CreateBooking();
			var instruction = Helper.CreateInstruction(booking, "", "", null);
			instruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			instruction.OrganisationType = "CTO";
			var divot1 = Helper.CreatePackageDivot(instruction);
			var divot2 = Helper.CreatePackageDivot(instruction);
			Helper.CreatePackage("p1", divot1);
			Helper.CreatePackage("p3", divot2);
			booking.KM_Direction = Constants.CartageDirection.Origin;

			AssertEquals("Precondition: Instruction should not be a sub.", false, instruction.IsSub);
			AssertEquals("Precondition: No confirmations should exist yet.", 0, instruction.Confirmations.Count);

			instruction.CreateDefaultConfirmation();

			AssertEquals("A confirmation should have been created.", 1, instruction.Confirmations.Count);
		}

		public void TestGetParentDropMode()
		{
			var transportBookingParent = (BusinessObject)Factory.New<IWhsReceive>();
			transportBookingParent[WhsDocketSchema.WD_DropMode] = "HSL";

			var consolidation = Helper.CreateConsolidation(transportBookingParent as IDtbBookingParent);
			var booking1 = Helper.CreateBooking(consolidation);
			var instruction1 = booking1.Instructions.AddNew();
			instruction1.OrganisationType = OrganisationTypesList.Codes.CNE;
			AssertEquals("Precondition", "", instruction1.KN_DropMode);
			instruction1.Address.OrganisationPK = Helper.CreateOrganisation("AAA").PK; // poke OnDocAddressChanged
			AssertEquals("HSL", instruction1.KN_DropMode);

			var booking2 = Helper.CreateBooking(consolidation);
			var instruction2 = booking1.Instructions.AddNew();
			instruction2.OrganisationType = OrganisationTypesList.Codes.CNR;
			AssertEquals("Precondition", "", instruction2.KN_DropMode);
			instruction2.Address.OrganisationPK = Helper.CreateOrganisation("BBB").PK;
			AssertEquals("HSL", instruction2.KN_DropMode);

			var booking3 = Helper.CreateBooking(consolidation);
			var instruction3 = booking3.Instructions.AddNew();
			instruction3.OrganisationType = OrganisationTypesList.Codes.WHS;
			AssertEquals("Precondition", "", instruction3.KN_DropMode);
			instruction3.Address.OrganisationPK = Helper.CreateOrganisation("CCC").PK;
			AssertEquals("HSL", instruction3.KN_DropMode);
		}

		public void TestSettingWrongOrgTypeForBookingWithParentJob()
		{
			var org = Helper.CreateOrganisation("CNR1");
			var shipment = Factory.New<Integration.Forwarding.IForwardingShipment>();
			var booking = Helper.CreateBooking();
			booking.ConsolidationSingleJob.KB_ParentID = shipment.PK;
			booking.ConsolidationSingleJob.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var instruction = Helper.CreateInstruction(booking, "PIC", "CNR", org.MainAddress);
			AssertNoExceptionThrown(() => instruction.OrganisationType = "XXX");
		}

		[TestDate(2024, 1, 1)]
		public void TestUpdateCO2eStatusToNotCurrent_WhenOrganisationTypeChanged_FromCYD_ToCYD()
		{
			// Arrange
			var booking = Helper.CreateBooking();
			var instruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			booking.SetCO2eStatus(CO2eStatusList.Codes.Current);
			Factory.Save();
			AssertNoWarning(booking.TotalCO2eForBindingInfo, CO2eTestHelper.CO2Warning);

			// Act & Assert
			CO2eTestHelper.AssertNoCO2Warning(booking, () => instruction.OrganisationType = LocalCartageJobOrgTypeList.Codes.CNR);
			CO2eTestHelper.AssertHasCO2Warning(booking, () => instruction.OrganisationType = LocalCartageJobOrgTypeList.Codes.CYD, "OrganisationType [CNR]->[CYD]");

			TestDateAttribute.AddMinutes(1);
			CO2eTestHelper.AssertHasCO2Warning(booking, () => instruction.OrganisationType = LocalCartageJobOrgTypeList.Codes.CTO, "OrganisationType [CYD]->[CTO]");
			CO2eTestHelper.AssertNoCO2Warning(booking, () => instruction.OrganisationType = LocalCartageJobOrgTypeList.Codes.CFS);
			CO2eTestHelper.AssertNoCO2Warning(booking, () => instruction.OrganisationType = LocalCartageJobOrgTypeList.Codes.CNE);
		}

		[TestDate(2024, 1, 1)]
		public void TestUpdateCO2eStatusToNotCurrent_WhenAddressChanged()
		{
			// Arrange
			var booking = Helper.CreateBooking();
			var instruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			booking.SetCO2eStatus(CO2eStatusList.Codes.Current);
			Factory.Save();
			AssertNoWarning(booking.TotalCO2eForBindingInfo, CO2eTestHelper.CO2Warning);

			// Act & Assert
			CO2eTestHelper.AssertHasCO2Warning(booking, () => instruction.Address.E2_OA_Address = Helper.CreateOrganisation("ORG").MainAddress.PK, "Address");

			TestDateAttribute.AddMinutes(1);
			CO2eTestHelper.AssertHasCO2Warning(booking, () => instruction.Address.E2_OA_Address = Guid.Empty, "Address");
		}

		[TestDate(2024, 1, 1)]
		public void TestUpdateCO2eStatusToNotCurrent_WhenInstructionTypeChanged()
		{
			// Arrange
			var booking = Helper.CreateBooking();
			var instruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			booking.SetCO2eStatus(CO2eStatusList.Codes.Current);
			Factory.Save();
			AssertNoWarning(booking.TotalCO2eForBindingInfo, CO2eTestHelper.CO2Warning);

			// Act & Assert
			CO2eTestHelper.AssertHasCO2Warning(booking, () => instruction.KN_InstructionType = InstructionTypes.Codes.Delivery, "KN_InstructionType [PIC]->[DLV]");

			Factory.Save();
			TestDateAttribute.AddMinutes(1);
			CO2eTestHelper.AssertHasCO2Warning(booking, () => instruction.KN_InstructionType = InstructionTypes.Codes.PickUp, "KN_InstructionType [DLV]->[PIC]");
		}

		public void TestUpdateCO2eStatusToNotCurrent_WhenAddOrDeleteInstruction()
		{
			// Arrange
			var booking = Helper.CreateBooking();
			var instruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			booking.SetCO2eStatus(CO2eStatusList.Codes.Current);
			Factory.Save();
			AssertNoWarning(booking.TotalCO2eForBindingInfo, CO2eTestHelper.CO2Warning);

			// Act & Assert
			CO2eTestHelper.AssertHasCO2Warning(booking, () => booking.Instructions.AddNew(), "Instruction added");

			TestDateAttribute.AddMinutes(1);
			CO2eTestHelper.AssertHasCO2Warning(booking, () => instruction.Delete(), "Instruction removed");
		}

		public void TestGetBusinessObjectBaseTypeFromTablePrefix()
		{
			var prefix = DtbBookingInstructionSchema.Constants.Prefix;
			var businessObjectType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(prefix, false);
			AssertEquals("Should supply correct type based on the table prefix", typeof(DtbBookingInstruction), businessObjectType);
			AssertNotEquals("Should not supply deprecated abstract type", typeof(DtbTransportInstruction), businessObjectType);
		}

		// No interface called IDtbTransportInstruction, therefore no need for TestGetTypeFromObjectFactory or TestCreateFromInterface
	}

	[TestedType(typeof(DtbBookingInstruction))]
	public class DtbBookingInstructionWorkflowProviderTest : WorkflowProviderTest<DtbBookingInstruction, DtbBookingInstructionProcessTaskCollection>
	{
		protected override DtbBookingInstruction GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			return booking.Instructions.AddNew();
		}

		protected override ZString ExpectedWorkflowType
		{
			get { return WorkflowDescriptors.DtbBookingInstructionWorkflowDescriptorCode; }
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}

		TransportBookingTestHelper helper;
	}
}
