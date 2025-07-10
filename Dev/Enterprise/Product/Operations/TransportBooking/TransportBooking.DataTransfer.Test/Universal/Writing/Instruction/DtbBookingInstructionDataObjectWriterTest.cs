using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Packing.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Writing;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.TransportBookings.DataTransfer.Universal.Testing
{
	class DtbBookingInstructionDataObjectWriterTest : DtbBookingTestCaseWithFactory
	{
		public void TestGetDataObject_BookingInstruction()
		{
			var refEquipment = Factory.New<RefEquipment>();
			refEquipment.RQ_ShortCode = "TRUCK";
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "Some CO";
			var bookingInstruction = Factory.New<DtbBookingInstruction>();
			bookingInstruction.KN_IsContainerRateable = true;
			bookingInstruction.KN_IsAuthorisedToLeave = true;
			bookingInstruction.KN_IsLooseRateable = true;
			bookingInstruction.KN_RQ_Equipment = refEquipment.PK;
			bookingInstruction.KN_Status = "AVL";
			bookingInstruction.KN_DropMode = "ASK";
			bookingInstruction.KN_InstructionType = "PIC";
			bookingInstruction.KN_Sequence = 5;
			bookingInstruction.KN_ServiceInstruction = "Polish";

			var instructionDataObject1 = new DtbBookingInstructionDataObjectWriter(null, new DataWritingManager(new DummyActionInfo())).GetDataObject(bookingInstruction);
			AssertEquals("instructionDataObject1.DropMode.Code", "ASK", instructionDataObject1.DropMode.Code);
			AssertEquals("instructionDataObject1.DropMode.Description", "Ask Client", instructionDataObject1.DropMode.Description);
			AssertEquals("instructionDataObject1.Type.Code", "PIC", instructionDataObject1.Type.Code);
			AssertEquals("instructionDataObject1.Type.Description", "Pickup", instructionDataObject1.Type.Description);
			AssertEquals("instructionDataObject1.Sequence", 5, instructionDataObject1.Sequence);
			AssertEquals("instructionDataObject1.ServiceInstruction", "Polish", instructionDataObject1.ServiceInstruction);
			AssertEquals("instructionDataObject1.Equipment", "TRUCK", instructionDataObject1.Equipment);
			AssertEquals("instructionDataObject1.IsContainerRateable", true, instructionDataObject1.IsContainerRateable);
			AssertEquals("instructionDataObject1.IsLooseRateable", true, instructionDataObject1.IsLooseRateable);
			AssertEquals("instructionDataObject1.KN_IsAuthorisedToLeave", true, instructionDataObject1.IsAuthorisedToLeave);
			AssertEquals("instructionDataObject1.Status.Code", "AVL", instructionDataObject1.Status.Code);
			AssertEquals("instructionDataObject1.Status.Description", "Available", instructionDataObject1.Status.Description);
			AssertNotNull(instructionDataObject1.Address);
			AssertNotNull(instructionDataObject1.Address.AddressType);
			AssertNull(instructionDataObject1.Address.CompanyName);

			bookingInstruction.Address.OrganisationPK = organisation.PK;

			var instructionDataObject2 = new DtbBookingInstructionDataObjectWriter(null, new DataWritingManager(new DummyActionInfo())).GetDataObject(bookingInstruction);
			AssertEquals("Some CO", instructionDataObject2.Address.CompanyName);
		}

		public void TestGetDataObject_BookingInstruction_CustomizedFields()
		{
			var today = ZDateTime.Today;
			var bookingInstruction = Factory.New<DtbBookingInstruction>();

			var emptyBookingInstructionDataObject = new DtbBookingInstructionDataObjectWriter(null, new DataWritingManager(new ActionInfo(RecipientRoleType.CTG, bookingInstruction))).GetDataObject(bookingInstruction);
			AssertNull("Precondition:", emptyBookingInstructionDataObject.CustomizedFieldCollection);

			bookingInstruction.SetUserDefinedValue("DATE", today);
			bookingInstruction.SetUserDefinedValue("BOOL", ZBool.True);
			bookingInstruction.SetUserDefinedValue("DECIMAL", new ZDecimal(123.456));
			bookingInstruction.SetUserDefinedValue("INT", new ZInt(123));
			bookingInstruction.SetUserDefinedValue("BYTE", new ZByte(234));
			bookingInstruction.SetUserDefinedValue("STRING", new ZString("HELLO"));
			bookingInstruction.SetUserDefinedValue("SHORT", new ZShort(345));

			var bookingInstructionDataObject = new DtbBookingInstructionDataObjectWriter(null, new DataWritingManager(new ActionInfo(RecipientRoleType.CTG, bookingInstruction))).GetDataObject(bookingInstruction);
			var customFieldCollection = bookingInstructionDataObject.CustomizedFieldCollection;
			AssertEquals("Not all Custom Fields were exported.", 7, customFieldCollection.Count);
			customFieldCollection.AssertCustomFieldWasExported(DataType.DateTime, "DATE", today.ToISO8601String());
			customFieldCollection.AssertCustomFieldWasExported(DataType.Boolean, "BOOL", "true");
			customFieldCollection.AssertCustomFieldWasExported(DataType.Decimal, "DECIMAL", "123.456");
			customFieldCollection.AssertCustomFieldWasExported(DataType.Integer, "INT", "123");
			customFieldCollection.AssertCustomFieldWasExported(DataType.Byte, "BYTE", "234");
			customFieldCollection.AssertCustomFieldWasExported(DataType.String, "STRING", "HELLO");
			customFieldCollection.AssertCustomFieldWasExported(DataType.Short, "SHORT", "345");
		}

		public void TestGetDataObject_BookingInstructionPkgDivot_Container()
		{
			var bookingInstruction = Factory.New<DtbBookingInstruction>();

			var packageJob = Factory.New<PkgPackageJob>();
			PkgPackage container1 = packageJob.Packages.AddNew("CNT");
			PkgPackage container2 = packageJob.Packages.AddNew("CNT");

			Instruction lightBookingInstructionDataObject = new DtbBookingInstructionDataObjectWriter(null, new DataWritingManager(new ActionInfo(RecipientRoleType.CTG, bookingInstruction))).GetDataObject(bookingInstruction);
			AssertNull("No container divots were set, so no InstructionContainerLinks should be created.", lightBookingInstructionDataObject.InstructionContainerLinkCollection);
			AssertNull("No package divots were set, so no InstructionPackingLineLinks should be created.", lightBookingInstructionDataObject.InstructionPackingLineLinkCollection);

			DtbBookingInstructionPkgDivot container1Divot = CreateDtbBookingInstructionPkgDivot(bookingInstruction, container1, 3);
			DtbBookingInstructionPkgDivot container2Divot = CreateDtbBookingInstructionPkgDivot(bookingInstruction, container2, 5);

			var linksDictionary = new Dictionary<ZGuid, ZInt>();
			linksDictionary.Add(container1.PK, 1);
			linksDictionary.Add(container2.PK, 2);

			Instruction heavyBookingInstructionDataObject = new DtbBookingInstructionDataObjectWriter(linksDictionary, new DataWritingManager(new ActionInfo(RecipientRoleType.CTG, bookingInstruction))).GetDataObject(bookingInstruction);
			InstructionContainerLink instructionContainer1LinkDataObject = FindInstructionContainerLink(heavyBookingInstructionDataObject.InstructionContainerLinkCollection, 1);
			InstructionContainerLink instructionContainer2LinkDataObject = FindInstructionContainerLink(heavyBookingInstructionDataObject.InstructionContainerLinkCollection, 2);
			AssertNull("No package divots were set, so no InstructionPackingLineLinks should be created.", heavyBookingInstructionDataObject.InstructionPackingLineLinkCollection);
			AssertEquals("2 container divots were set, so 2 InstructionContainerLinks should be created.", 2, heavyBookingInstructionDataObject.InstructionContainerLinkCollection.Count);

			AssertEquals("InstructionContainerLink.Link", 1, instructionContainer1LinkDataObject.ContainerLink);
			AssertEquals("InstructionContainerLink.Quantity", 3, instructionContainer1LinkDataObject.Quantity);
			AssertNull("No BookingConfirmations were set, so no Confirmations should be created.", instructionContainer1LinkDataObject.ConfirmationCollection);

			AssertEquals("InstructionContainerLink.Link", 2, instructionContainer2LinkDataObject.ContainerLink);
			AssertEquals("InstructionContainerLink.Quantity", 5, instructionContainer2LinkDataObject.Quantity);
			AssertNull("No BookingConfirmations were set, so no Confirmations should be created.", instructionContainer2LinkDataObject.ConfirmationCollection);
		}

		public void TestGetDataObject_BookingInstructionPkgDivot_ContainerConfirmations()
		{
			var bookingInstruction = Factory.New<DtbBookingInstruction>();
			var packageJob = Factory.New<PkgPackageJob>();
			var container1 = packageJob.Packages.AddNew("CNT");
			var container2 = packageJob.Packages.AddNew("CNT");
			var container1Divot = CreateDtbBookingInstructionPkgDivot(bookingInstruction, container1, 3);
			var container2Divot = CreateDtbBookingInstructionPkgDivot(bookingInstruction, container2, 5);

			var linksDictionary = new Dictionary<ZGuid, ZInt>();
			linksDictionary.Add(container1.PK, 1);
			linksDictionary.Add(container2.PK, 2);

			var allPackagesConfirmation = bookingInstruction.Confirmations.AddNew();
			allPackagesConfirmation.KK_ReferenceNum = "ALL_CONFIRM";

			var container1Confirmation = container1Divot.Confirmations.AddNew();
			container1Confirmation.KK_ReferenceNum = "CONT1_CONFIRM";

			var bookingInstructionDataObject = new DtbBookingInstructionDataObjectWriter(linksDictionary, new DataWritingManager(new DummyActionInfo())).GetDataObject(bookingInstruction);
			var instructionContainer1LinkDataObject = FindInstructionContainerLink(bookingInstructionDataObject.InstructionContainerLinkCollection, 1);
			var instructionContainer2LinkDataObject = FindInstructionContainerLink(bookingInstructionDataObject.InstructionContainerLinkCollection, 2);
			AssertEquals("Should have 2 confirmations 1 for all + 1 for a package.", 2, instructionContainer1LinkDataObject.ConfirmationCollection.Count);
			AssertEquals("All package confirmation is incorrect.", "ALL_CONFIRM", instructionContainer1LinkDataObject.ConfirmationCollection[0].Reference);
			AssertEquals("Package only confirmation is incorrect.", "CONT1_CONFIRM", instructionContainer1LinkDataObject.ConfirmationCollection[1].Reference);

			AssertEquals("Should have only 1 confirmation for all packages.", 1, instructionContainer2LinkDataObject.ConfirmationCollection.Count);
			AssertEquals("All package Confirmation is incorrect.", "ALL_CONFIRM", instructionContainer2LinkDataObject.ConfirmationCollection[0].Reference);
		}

		public void TestGetDataObject_BookingInstructionPkgDivot_PackingLine()
		{
			var bookingInstruction = Factory.New<DtbBookingInstruction>();

			var packageJob = Factory.New<PkgPackageJob>();
			PkgPackage package1 = packageJob.Packages.AddNew("PLT");
			PkgPackage package2 = packageJob.Packages.AddNew("BOX");

			Instruction lightBookingInstructionDataObject = new DtbBookingInstructionDataObjectWriter(null, new DataWritingManager(new ActionInfo(RecipientRoleType.CTG, bookingInstruction))).GetDataObject(bookingInstruction);
			AssertNull("No container divots were set, so no InstructionContainerLinks should be created.", lightBookingInstructionDataObject.InstructionContainerLinkCollection);
			AssertNull("No package divots were set, so no InstructionPackingLineLinks should be created.", lightBookingInstructionDataObject.InstructionPackingLineLinkCollection);

			DtbBookingInstructionPkgDivot package1Divot = CreateDtbBookingInstructionPkgDivot(bookingInstruction, package1, 3);
			DtbBookingInstructionPkgDivot package2Divot = CreateDtbBookingInstructionPkgDivot(bookingInstruction, package2, 5);

			var linksDictionary = new Dictionary<ZGuid, ZInt>();
			linksDictionary.Add(package1.PK, 1);
			linksDictionary.Add(package2.PK, 2);

			Instruction heavyBookingInstructionDataObject = new DtbBookingInstructionDataObjectWriter(linksDictionary, new DataWritingManager(new ActionInfo(RecipientRoleType.CTG, bookingInstruction))).GetDataObject(bookingInstruction);
			InstructionPackingLineLink instructionPackage1LinkDataObject = FindInstructionPackingLineLink(heavyBookingInstructionDataObject.InstructionPackingLineLinkCollection, 1);
			InstructionPackingLineLink instructionPackage2LinkDataObject = FindInstructionPackingLineLink(heavyBookingInstructionDataObject.InstructionPackingLineLinkCollection, 2);
			AssertNull("No container divots were set, so no InstructionContainerLinks should be created.", heavyBookingInstructionDataObject.InstructionContainerLinkCollection);
			AssertEquals("2 package divots were set, so 2 InstructionPackingLineLinks should be created.", 2, heavyBookingInstructionDataObject.InstructionPackingLineLinkCollection.Count);

			AssertEquals("InstructionContainerLink.Link", 1, instructionPackage1LinkDataObject.PackingLineLink);
			AssertEquals("InstructionContainerLink.Quantity", 3, instructionPackage1LinkDataObject.Quantity);
			AssertNull("No BookingConfirmations were set, so no Confirmations should be created.", instructionPackage1LinkDataObject.ConfirmationCollection);

			AssertEquals("InstructionContainerLink.Link", 2, instructionPackage2LinkDataObject.PackingLineLink);
			AssertEquals("InstructionContainerLink.Quantity", 5, instructionPackage2LinkDataObject.Quantity);
			AssertNull("No BookingConfirmations were set, so no Confirmations should be created.", instructionPackage2LinkDataObject.ConfirmationCollection);
		}

		public void TestGetDataObject_BookingInstructionPkgDivot_PackingLineConfirmations()
		{
			var bookingInstruction = Factory.New<DtbBookingInstruction>();
			var packageJob = Factory.New<PkgPackageJob>();
			var package1 = packageJob.Packages.AddNew("PLT");
			var package2 = packageJob.Packages.AddNew("BOX");
			var package1Divot = CreateDtbBookingInstructionPkgDivot(bookingInstruction, package1, 3);
			var package2Divot = CreateDtbBookingInstructionPkgDivot(bookingInstruction, package2, 5);

			var linksDictionary = new Dictionary<ZGuid, ZInt>();
			linksDictionary.Add(package1.PK, 1);
			linksDictionary.Add(package2.PK, 2);

			var allPackagesConfirmation = bookingInstruction.Confirmations.AddNew();
			allPackagesConfirmation.KK_ReferenceNum = "ALL_CONFIRM";

			var package1Confirmation = package1Divot.Confirmations.AddNew();
			package1Confirmation.KK_ReferenceNum = "PKG1_CONFIRM";

			var bookingInstructionDataObject = new DtbBookingInstructionDataObjectWriter(linksDictionary, new DataWritingManager(new ActionInfo(RecipientRoleType.CTG, bookingInstruction))).GetDataObject(bookingInstruction);
			var instructionPackage1LinkDataObject = FindInstructionPackingLineLink(bookingInstructionDataObject.InstructionPackingLineLinkCollection, 1);
			var instructionPackage2LinkDataObject = FindInstructionPackingLineLink(bookingInstructionDataObject.InstructionPackingLineLinkCollection, 2);
			AssertEquals("Should have 2 confirmations 1 for all + 1 for a package.", 2, instructionPackage1LinkDataObject.ConfirmationCollection.Count);
			AssertEquals("General Confirmation is incorrect.", "ALL_CONFIRM", instructionPackage1LinkDataObject.ConfirmationCollection[0].Reference);
			AssertEquals("Personal Confirmation is incorrect.", "PKG1_CONFIRM", instructionPackage1LinkDataObject.ConfirmationCollection[1].Reference);
		}

		public void TestGetDataObject_ExportsConfirmationsDirectlyOnInstruction_WithoutPackageDivots()
		{
			var instruction = Factory.New<DtbBookingInstruction>();
			var confirmation1 = instruction.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp);
			confirmation1.KK_ReferenceNum = "REF1";
			var confirmation2 = instruction.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp);
			confirmation2.KK_ReferenceNum = "REF2";

			var writer = new DtbBookingInstructionDataObjectWriter(null, new DataWritingManager(new DummyActionInfo()));
			var instructionDataObject = writer.GetDataObject(instruction);
			AssertNotNull(instructionDataObject);
			AssertNull(instructionDataObject.InstructionContainerLinkCollection);
			AssertNotNull(instructionDataObject.InstructionPackingLineLinkCollection);
			var packageDivot = instructionDataObject.InstructionPackingLineLinkCollection.Single();
			AssertNull("Dummy divot should not have PackingLine Link.", packageDivot.PackingLineLink);
			AssertEquals("Dummy divot should have zero quantity.", 0, packageDivot.Quantity);
			packageDivot.ConfirmationCollection.Single(o => o.DateDescription.GetValueOrDefault() == ConfirmationTypes.Codes.PickUp && o.Reference.GetValueOrDefault() == "REF1");
			packageDivot.ConfirmationCollection.Single(o => o.DateDescription.GetValueOrDefault() == ConfirmationTypes.Codes.PickUp && o.Reference.GetValueOrDefault() == "REF2");
		}

		public void TestGetDataObject_DivotsWithConfirmationsAndInstructionLevelConfirmation()
		{
			// I
			//    C - ALL Packages on the Instruction (divot qty total)
			//    D 8x plt of P 10x plt
			//       C 5x plt
			//    D 16x plt	of P 20x plt
			//       C 6x plt

			// should export as
			// Ip
			//    D 8x plt	- P 10x plt
			//       Slot date C 5x plt
			//       Pickup C 8x plt today
			//    D 16x plt	- P 20x plt
			//       C 6x plt
			//       C 16 plt today

			// should import as
			// I
			//    C - ALL (qty should be 0 in enterprise db, DO NOT SET IT. the getter will get 24 if your data is correct.)
			//    D 8x plt	- P 10x plt
			//       C 5x plt
			//    D 16x plt	- P 20x plt
			//       C 6x plt

			// Divot1.Confirmations.Where(c.qty = divot1.qty)  + Divot2.Confirmations + Divot3.Confirmations

			var instruction = Factory.New<DtbBookingInstruction>();
			var packageJob = Factory.New<PkgPackageJob>();
			var packageA = packageJob.Packages.AddNew("PLT", 10);
			var packageB = packageJob.Packages.AddNew("PLT", 20);

			var confirmationForInstruction = instruction.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp);
			confirmationForInstruction.KK_ReferenceNum = "IREF1";
			confirmationForInstruction.KK_Quantity = 24;

			var package1Divot = CreateDtbBookingInstructionPkgDivot(instruction, packageA, 8);
			var packageConfirmation1 = package1Divot.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp);
			packageConfirmation1.KK_ReferenceNum = "PKG1_CONFIRM";
			packageConfirmation1.KK_Quantity = 5;

			var package2Divot = CreateDtbBookingInstructionPkgDivot(instruction, packageB, 16);
			var packageConfirmation2 = package2Divot.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp);
			packageConfirmation2.KK_ReferenceNum = "PKG1_CONFIRM";
			packageConfirmation2.KK_Quantity = 6;

			var linksDictionary = new Dictionary<ZGuid, ZInt>();
			linksDictionary.Add(packageA.PK, 1);
			linksDictionary.Add(packageB.PK, 2);

			var writer = new DtbBookingInstructionDataObjectWriter(linksDictionary, new DataWritingManager(new DummyActionInfo()));
			var instructionDataObject = writer.GetDataObject(instruction);
			AssertNotNull(instructionDataObject);
			AssertNull(instructionDataObject.InstructionContainerLinkCollection);
			AssertEquals(2, instructionDataObject.InstructionPackingLineLinkCollection.Count);

			var instructionPackingLinkDataObject1 = FindInstructionPackingLineLink(instructionDataObject.InstructionPackingLineLinkCollection, 1);
			AssertEquals("InstructionPackingLinkDataObject.Link", 1, instructionPackingLinkDataObject1.PackingLineLink);
			AssertEquals("InstructionPackingLinkDataObject.Quantity", 8, instructionPackingLinkDataObject1.Quantity);
			AssertEquals(2, instructionPackingLinkDataObject1.ConfirmationCollection.Count);
			instructionPackingLinkDataObject1.ConfirmationCollection.Single(o => o.DateDescription.GetValueOrDefault() == ConfirmationTypes.Codes.PickUp
																					&& o.Reference.GetValueOrDefault() == "PKG1_CONFIRM"
																					&& o.Quantity == 5);
			instructionPackingLinkDataObject1.ConfirmationCollection.Single(o => o.DateDescription.GetValueOrDefault() == ConfirmationTypes.Codes.PickUp
																					&& o.Reference.GetValueOrDefault() == "IREF1"
																					&& o.Quantity == 8);

			var instructionPackingLinkDataObject2 = FindInstructionPackingLineLink(instructionDataObject.InstructionPackingLineLinkCollection, 2);
			AssertEquals("InstructionPackingLinkDataObject.Link", 2, instructionPackingLinkDataObject2.PackingLineLink);
			AssertEquals("InstructionPackingLinkDataObject.Quantity", 16, instructionPackingLinkDataObject2.Quantity);
			AssertEquals(2, instructionPackingLinkDataObject2.ConfirmationCollection.Count);
			instructionPackingLinkDataObject2.ConfirmationCollection.Single(o => o.DateDescription.GetValueOrDefault() == ConfirmationTypes.Codes.PickUp
																					&& o.Reference.GetValueOrDefault() == "PKG1_CONFIRM"
																					&& o.Quantity == 6);
			instructionPackingLinkDataObject2.ConfirmationCollection.Single(o => o.DateDescription.GetValueOrDefault() == ConfirmationTypes.Codes.PickUp
																		&& o.Reference.GetValueOrDefault() == "IREF1"
																		&& o.Quantity == 16);
		}

		InstructionContainerLink FindInstructionContainerLink(List<InstructionContainerLink> instructionContainerLinksList, ZInt containerLink)
		{
			foreach (InstructionContainerLink instructionContainerLink in instructionContainerLinksList)
			{
				if (instructionContainerLink.ContainerLink == containerLink)
				{
					return instructionContainerLink;
				}
			}
			Fail("No InstructionContainerLink were found with ContainerLink = " + containerLink);
			return null;
		}

		InstructionPackingLineLink FindInstructionPackingLineLink(List<InstructionPackingLineLink> instructionPackingLineLinksList, ZInt packingLineLink)
		{
			foreach (InstructionPackingLineLink instructionPackingLineLink in instructionPackingLineLinksList)
			{
				if (instructionPackingLineLink.PackingLineLink == packingLineLink)
				{
					return instructionPackingLineLink;
				}
			}
			Fail("No InstructionPackingLineLink were found with PackingLineLink = " + packingLineLink);
			return null;
		}

		DtbBookingInstructionPkgDivot CreateDtbBookingInstructionPkgDivot(DtbBookingInstruction bookingInstruction, PkgPackage package, ZInt quantity)
		{
			DtbBookingInstructionPkgDivot result = bookingInstruction.PackageDivots.AddNew();
			result.KD_KP_Package = package.PK;
			result.KD_Quantity = quantity;

			return result;
		}
	}
}
