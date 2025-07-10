using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Constants = Enterprise.Core.Constants;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.LocalCartage.DataTransfer.Universal.Testing
{
	public class LocalTransportDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestNotes()
		{
			var cartage = Factory.New<CommonCartage>();
			var noteBO1 = cartage.Notes.AddNew(true, "CAT EATER!!", "Feee-lix the cat, what a wonderful-wonderful cat.");
			noteBO1.ST_NoteType = nameof(StmNoteVisibility.PUB);
			var noteBO2 = cartage.Notes.AddNew(false, "Internal Work Notes", "Flintstones, meet the Flintstones.");
			noteBO2.ST_NoteContext = "DEB";
			var localTransportData = new LocalTransportDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, cartage))).GetDataObject(cartage);
			AssertNotNull("localTransportData", localTransportData);
			AssertNotNull("localTransportData.NoteCollection", localTransportData.NoteCollection);
			AssertEquals("localTransportData.NoteCollection.Count", 2, localTransportData.NoteCollection.Count);
			var note1 = localTransportData.NoteCollection[0];
			CombineAssertions(delegate
			{
				AssertEquals("note1.Description", "CAT EATER!!", note1.Description);
				AssertEquals("note1.IsCustomDescription", ZBool.True, note1.IsCustomDescription);
				AssertEquals("note1.NoteText", "Feee-lix the cat, what a wonderful-wonderful cat.", note1.NoteText);
				AssertEquals("note1.NoteContext.Code", "AAA", note1.NoteContext.Code);
				AssertEquals("note1.NoteContext.Description", "Module: A - All, Direction: A - All, Freight: A - All", note1.NoteContext.Description);
				AssertEquals("note1.Visibility.Code", "PUB", note1.Visibility.Code);
				AssertEquals("note1.Visibility.Description", "CLIENT-VISIBLE", note1.Visibility.Description);
			});
			var note2 = localTransportData.NoteCollection[1];
			CombineAssertions(delegate
			{
				AssertEquals("note2.Description", "Internal Work Notes", note2.Description);
				AssertEquals("note2.IsCustomDescription", ZBool.False, note2.IsCustomDescription);
				AssertEquals("note2.NoteText", "Flintstones, meet the Flintstones.", note2.NoteText);
				AssertEquals("note2.NoteContext.Code", "DEB", note2.NoteContext.Code);
				AssertEquals("note2.NoteContext.Description", "Module: D - Customs/Declarations, Direction: E - Export, Freight: B - Air and Sea", note2.NoteContext.Description);
				AssertEquals("note2.Visibility.Code", "INT", note2.Visibility.Code);
				AssertEquals("note2.Visibility.Description", "INTERNAL", note2.Visibility.Description);
			});
		}

		public void TestPopulateModes_NoJobType()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = "";
			cartage.JJ_ShippingTransportMode = Constants.TransportModes.Air;
			cartage.JJ_ContainerMode = Constants.CartageContainerMode.Loose;
			cartage.JJ_Direction = Constants.CartageDirection.Import;
			var localTransportData = new LocalTransportDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, cartage))).GetDataObject(cartage);
			AssertNotNull("localTransportData", localTransportData);
			AssertEquals("TransportMode", Constants.TransportModes.Air, localTransportData.TransportMode.Code);
			AssertEquals("TransportBookingDirection", Constants.CartageDirection.Import, localTransportData.TransportBookingDirection.Code);
			AssertEquals("ContainerMode", Constants.CartageContainerMode.Loose, localTransportData.ContainerMode.Code);
		}

		public void TestGetDataObject_CustomizedFields()
		{
			var today = ZDateTime.Today;
			var cartage = Factory.New<CommonCartage>();
			var localTransportDataObject1 = new LocalTransportDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, cartage))).GetDataObject(cartage);
			AssertNull("Precondition:", localTransportDataObject1.CustomizedFieldCollection);
			cartage.SetUserDefinedValue("DATE", today);
			cartage.SetUserDefinedValue("BOOL", ZBool.True);
			cartage.SetUserDefinedValue("DECIMAL", new ZDecimal(123.456));
			cartage.SetUserDefinedValue("INT", new ZInt(123));
			cartage.SetUserDefinedValue("BYTE", new ZByte(234));
			cartage.SetUserDefinedValue("STRING", new ZString("HELLO"));
			cartage.SetUserDefinedValue("SHORT", new ZShort(345));
			var localTransportDataObject2 = new LocalTransportDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, cartage))).GetDataObject(cartage);
			var customFieldCollection = localTransportDataObject2.CustomizedFieldCollection;
			AssertEquals("Custom Fields were exported.", 7, customFieldCollection.Count);
			customFieldCollection.AssertCustomFieldWasExported(DataType.DateTime, "DATE", today.ToISO8601String());
			customFieldCollection.AssertCustomFieldWasExported(DataType.Boolean, "BOOL", "true");
			customFieldCollection.AssertCustomFieldWasExported(DataType.Decimal, "DECIMAL", "123.456");
			customFieldCollection.AssertCustomFieldWasExported(DataType.Integer, "INT", "123");
			customFieldCollection.AssertCustomFieldWasExported(DataType.Byte, "BYTE", "234");
			customFieldCollection.AssertCustomFieldWasExported(DataType.String, "STRING", "HELLO");
			customFieldCollection.AssertCustomFieldWasExported(DataType.Short, "SHORT", "345");
		}

		public void TestWriteDataObject()
		{
			var now = ZDateTime.Now;
			var dayFromNow = now.AddDays(1);
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLUnpackLooseToCNE, 3);
			var job = new JobHeader.Loader(cartage).TryLoadOrCreate();
			var client = Helper.CreateOrgHeader("AAABBB", "1 Blue Street");
			job.JH_OA_LocalChargesAddr = client.MainAddress.PK;
			cartage.JJ_OrderReferenceNumber = "order1";
			cartage.JJ_QuoteNumber = "quote1";
			cartage.JJ_WaybillNumber = "waybill1";
			cartage.JJ_DropMode = Constants.FCLEquipmentNeeded.SideLoader;
			cartage.JJ_EstimatedPickup = now.AddHours(1);
			cartage.JJ_EstimatedDelivery = now.AddHours(2);
			cartage.JJ_A_JCL = now.AddHours(3);
			SetupAdditionalReference(cartage.AdditionalReferenceNumbers.AddNew(), "OTH", "hello");
			SetupAdditionalReference(cartage.AdditionalReferenceNumbers.AddNew(), "TRA", "transportIDDD");
			var cto = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageCTO, "CTO", "Wharf st", "2000", "Sydney", "AUSYD", false);
			var cfs = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageCFS, "CFS", "Depot st", "2000", "Sydney", "AUSYD", false);
			var cne = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageImporter, "CNE", "Consignee st", "2000", "Sydney", "AUSYD", false);
			var cne2 = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageImporter, "CNE2", "Consignee2 st", "2000", "Sydney", "AUSYD", false);
			var cyd = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageYard, "CYD", "Yard st", "2000", "Sydney", "AUSYD", false);
			var container1 = Helper.SetupBookedMove(cartage.GetBookedMoves(cartage.Containers.ElementAt(0))[0], cto, cne, now);
			var container2 = Helper.SetupBookedMove(cartage.GetBookedMoves(cartage.Containers.ElementAt(1))[0], cto, cne, now.AddMinutes(10));
			var container3 = Helper.SetupBookedMove(cartage.GetBookedMoves(cartage.Containers.ElementAt(2))[0], cto, cne, now.AddMinutes(20));
			var package1 = Helper.SetupBookedMove(cartage.LooseBookedMoves[0], cfs, cne, dayFromNow);
			var package2 = Helper.SetupBookedMove(cartage.LooseBookedMoves[1], cfs, cne, dayFromNow.AddMinutes(10));
			var package3 = Helper.SetupBookedMove(cartage.LooseBookedMoves[2], cfs, cne2, dayFromNow.AddMinutes(20));
			container1.Container.JC_ContainerNum = "CONT1";
			container2.Container.JC_ContainerNum = "CONT2";
			container3.Container.JC_ContainerNum = "CONT3";
			Helper.SetupBookedMove(package1, 10, Constants.PkgUnit.Bag, 11, Constants.Weight.Kilograms, 12, Constants.Volume.CubicMetres);
			Helper.SetupBookedMove(package2, 20, Constants.PkgUnit.Box, 21, Constants.Weight.Pounds, 22, Constants.Volume.CubicFeet);
			Helper.SetupBookedMove(package3, 30, Constants.PkgUnit.Pallet, 31, Constants.Weight.Ounces, 32, Constants.Volume.CubicInches);
			container1.EW_DropMode = "";
			container2.EW_DropMode = Constants.FCLEquipmentNeeded.WaitForUnpack;
			container3.EW_DropMode = "";
			package1.EW_DropMode = "";
			package2.EW_DropMode = "";
			package3.EW_DropMode = Constants.LCLAIREquipmentNeeded.Premise;
			container1.CartageLegs.DeleteAll();
			container2.CartageLegs.DeleteAll();
			container3.CartageLegs.DeleteAll();
			package1.CartageLegs.DeleteAll();
			package2.CartageLegs.DeleteAll();
			package3.CartageLegs.DeleteAll();
			var container1leg1 = Helper.SetupLeg(container1.CartageLegs.AddNew(), cto, null, cfs, now);
			var container1leg2 = Helper.SetupLeg(container1.CartageLegs.AddNew(), cfs, null, cne, now.AddMinutes(10));
			var container1leg3 = Helper.SetupLeg(container1.CartageLegs.AddNew(), cne, null, cfs, now.AddMinutes(20));
			var container1leg4 = Helper.SetupLeg(container1.CartageLegs.AddNew(), cfs, null, cyd, now.AddMinutes(30));
			var container2leg1 = Helper.SetupLeg(container2.CartageLegs.AddNew(), cto, cne, cyd, now.AddMinutes(40));
			var container3leg1 = Helper.SetupLeg(container3.CartageLegs.AddNew(), cto, null, cne, now.AddMinutes(50));
			var container3leg2 = Helper.SetupLeg(container3.CartageLegs.AddNew(), cne, null, cyd, now.AddMinutes(60));
			var package1leg1 = Helper.SetupLeg(package1.CartageLegs.AddNew(), cfs, null, cne, dayFromNow);
			var package2leg1 = Helper.SetupLeg(package2.CartageLegs.AddNew(), cfs, null, cne, dayFromNow.AddMinutes(10));
			var package3leg1 = Helper.SetupLeg(package3.CartageLegs.AddNew(), cfs, null, cne2, dayFromNow.AddMinutes(20));
			var writer = new LocalTransportDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, cartage)));
			var cartageDataObject = writer.GetDataObject(cartage);
			AssertNotNull("Precondition: cartageDataObject", cartageDataObject);
			AssertEquals("order1", cartageDataObject.Order.OrderNumber);
			AssertEquals("quote1", cartageDataObject.QuoteNumber);
			AssertEquals("waybill1", cartageDataObject.WayBillNumber);
			AssertEquals(Constants.FCLEquipmentNeeded.SideLoader, cartageDataObject.LocalTransportEquipmentNeeded.Code);
			AssertEquals(now.AddHours(1), cartageDataObject.DateCollection.First(d => d.IsEstimate.Value && DateType.LocalTransportPickup.Equals(d.Type)).Value);
			AssertEquals(now.AddHours(2), cartageDataObject.DateCollection.First(d => d.IsEstimate.Value && DateType.LocalTransportDelivery.Equals(d.Type)).Value);
			AssertEquals(now.AddHours(3), cartageDataObject.DateCollection.First(d => !d.IsEstimate.Value && DateType.LocalTransportCompleted.Equals(d.Type)).Value);
			AssertEquals(2, cartageDataObject.AdditionalReferenceCollection.Count);
			AssertAdditionalReference(cartageDataObject.AdditionalReferenceCollection[0], "OTH", "hello");
			AssertAdditionalReference(cartageDataObject.AdditionalReferenceCollection[1], "TRA", "transportIDDD");
			AssertEquals(CollectionContent.Complete, cartageDataObject.ContainerCollection.Content);
			AssertEquals("Addresses", 1, cartageDataObject.OrganizationAddressCollection.Count);
			var localClientAddress = cartageDataObject.OrganizationAddressCollection[0];
			AssertEquals("localClientAddress.AddressType", "SendersLocalClient", localClientAddress.AddressType);
			AssertEquals("Client Address 1", client.MainAddress.OA_Address1, localClientAddress.Address1);
			var instructionDataObjects = cartageDataObject.InstructionCollection.OrderBy(i => i.Sequence);
			var ctoInstruction = AssertInstruction(instructionDataObjects.ElementAt(0), cto, 1, 3, 0, "");
			var cfsInstruction = AssertInstruction(instructionDataObjects.ElementAt(1), cfs, 2, 1, 3, "");
			var cneInstruction = AssertInstruction(instructionDataObjects.ElementAt(2), cne, 3, 2, 2, "");
			var cfs2Instruction = AssertInstruction(instructionDataObjects.ElementAt(3), cfs, 4, 1, 0, "");
			var cydInstruction = AssertInstruction(instructionDataObjects.ElementAt(4), cyd, 5, 2, 0, "");
			var cneWaitInstruction = AssertInstruction(instructionDataObjects.ElementAt(5), cne, 6, 1, 0, Constants.FCLEquipmentNeeded.WaitForUnpack);
			var cydWaitInstruction = AssertInstruction(instructionDataObjects.ElementAt(6), cyd, 7, 1, 0, ""); // could do better merging of instruction chains
			var cne2Instruction = AssertInstruction(instructionDataObjects.ElementAt(7), cne2, 8, 0, 1, Constants.LCLAIREquipmentNeeded.Premise);
			var containerDataObject1 = AssertContainer(cartageDataObject, "CONT1");
			var containerDataObject2 = AssertContainer(cartageDataObject, "CONT2");
			var containerDataObject3 = AssertContainer(cartageDataObject, "CONT3");
			var packageDataObject1 = BookedMovePackageDataObjectWriterTest.AssertPackage(cartageDataObject, 10, Constants.PkgUnit.Bag, 11, Constants.Weight.Kilograms, 12, Constants.Volume.CubicMetres);
			var packageDataObject2 = BookedMovePackageDataObjectWriterTest.AssertPackage(cartageDataObject, 20, Constants.PkgUnit.Box, 21, Constants.Weight.Pounds, 22, Constants.Volume.CubicFeet);
			var packageDataObject3 = BookedMovePackageDataObjectWriterTest.AssertPackage(cartageDataObject, 30, Constants.PkgUnit.Pallet, 31, Constants.Weight.Ounces, 32, Constants.Volume.CubicInches);
			var ctoContainer1 = AssertContainerLink(ctoInstruction, containerDataObject1, 1, ZDateTime.Empty, now, 0, 0, 1, 1);
			var ctoContainer2 = AssertContainerLink(ctoInstruction, containerDataObject2, 1, ZDateTime.Empty, now.AddMinutes(40), 0, 0, 5, 1);
			var ctoContainer3 = AssertContainerLink(ctoInstruction, containerDataObject3, 1, ZDateTime.Empty, now.AddMinutes(50), 0, 0, 6, 1);
			var cfsContainer1 = AssertContainerLink(cfsInstruction, containerDataObject1, 1, now.AddMinutes(4), now.AddMinutes(10), 1, 2, 2, 1);
			var cfsPackage1 = AssertPackingLineLink(cfsInstruction, packageDataObject1, 10, ZDateTime.Empty, dayFromNow, 0, 0, 8, 1);
			var cfsPackage2 = AssertPackingLineLink(cfsInstruction, packageDataObject2, 20, ZDateTime.Empty, dayFromNow.AddMinutes(10), 0, 0, 9, 1);
			var cfsPackage3 = AssertPackingLineLink(cfsInstruction, packageDataObject3, 30, ZDateTime.Empty, dayFromNow.AddMinutes(20), 0, 0, 10, 1);
			var cneContainer1 = AssertContainerLink(cneInstruction, containerDataObject1, 1, now.AddMinutes(14), now.AddMinutes(20), 2, 2, 3, 1);
			var cneContainer3 = AssertContainerLink(cneInstruction, containerDataObject3, 1, now.AddMinutes(54), now.AddMinutes(60), 6, 2, 7, 1);
			var cnePackage1 = AssertPackingLineLink(cneInstruction, packageDataObject1, 10, dayFromNow.AddMinutes(4), ZDateTime.Empty, 8, 2, 0, 0);
			var cnePackage2 = AssertPackingLineLink(cneInstruction, packageDataObject2, 20, dayFromNow.AddMinutes(14), ZDateTime.Empty, 9, 2, 0, 0);
			var cfs2Container1 = AssertContainerLink(cfs2Instruction, containerDataObject1, 1, now.AddMinutes(24), now.AddMinutes(30), 3, 2, 4, 1);
			var cydContainer1 = AssertContainerLink(cydInstruction, containerDataObject1, 1, now.AddMinutes(34), ZDateTime.Empty, 4, 2, 0, 0);
			var cydContainer3 = AssertContainerLink(cydInstruction, containerDataObject3, 1, now.AddMinutes(64), ZDateTime.Empty, 7, 2, 0, 0);
			var cneWaitContainer2 = AssertContainerLink(cneWaitInstruction, containerDataObject2, 1, now.AddMinutes(42), ZDateTime.Empty, 5, 2, 5, 3, true);
			var cydWaitContainer2 = AssertContainerLink(cydWaitInstruction, containerDataObject2, 1, now.AddMinutes(44), ZDateTime.Empty, 5, 4, 0, 0);
			var cne2Package3 = AssertPackingLineLink(cne2Instruction, packageDataObject3, 30, dayFromNow.AddMinutes(24), ZDateTime.Empty, 10, 2, 0, 0);
		}

		Instruction AssertInstruction(Instruction instruction, JobDocAddress docAddress, ZInt sequence, ZInt containers, ZInt packageLines, ZString dropMode)
		{
			AssertEquals(sequence, instruction.Sequence);
			AssertEquals(dropMode, instruction.DropMode != null ? instruction.DropMode.Code : ZString.Empty);
			AssertAddress(instruction.Address, docAddress);
			if (containers == 0)
			{
				AssertNull(instruction.InstructionContainerLinkCollection);
			}
			else
			{
				AssertEquals(containers, instruction.InstructionContainerLinkCollection.Count);
			}

			if (packageLines == 0)
			{
				AssertNull(instruction.InstructionPackingLineLinkCollection);
			}
			else
			{
				AssertEquals(packageLines, instruction.InstructionPackingLineLinkCollection.Count);
			}

			return instruction;
		}

		Container AssertContainer(UniversalShipment cartageDataObject, ZString containerNumber)
		{
			var container = cartageDataObject.ContainerCollection.FirstOrDefault(c => c.ContainerNumber.Equals(containerNumber));
			return container;
		}

		InstructionContainerLink AssertContainerLink(Instruction instruction, Container container, ZInt qty, ZDateTime deliverActualTime, ZDateTime pickupActualTime, int dlvLegLink, int dlvLegSequence, int picLegLink, int picLegSequence, bool isWait = false)
		{
			var link = instruction.InstructionContainerLinkCollection.Find(l => l.ContainerLink == container.Link);
			AssertEquals(qty, link.Quantity);
			AssertConfirmation(link.ConfirmationCollection, deliverActualTime, pickupActualTime, dlvLegLink, dlvLegSequence, picLegLink, picLegSequence, isWait);
			return link;
		}

		InstructionPackingLineLink AssertPackingLineLink(Instruction instruction, PackingLine package, ZInt qty, ZDateTime deliverActualTime, ZDateTime pickupActualTime, int dlvLegLink, int dlvLegSequence, int picLegLink, int picLegSequence, bool isWait = false)
		{
			var link = instruction.InstructionPackingLineLinkCollection.Find(l => l.PackingLineLink == package.Link);
			AssertEquals(qty, link.Quantity);
			AssertConfirmation(link.ConfirmationCollection, deliverActualTime, pickupActualTime, dlvLegLink, dlvLegSequence, picLegLink, picLegSequence, isWait);
			return link;
		}

		void AssertAddress(OrganizationAddress addressData, JobDocAddress docAddress)
		{
			AssertEquals("Code", docAddress.Organisation.OH_Code, addressData.OrganizationCode);
			AssertEquals("Address1", docAddress.E2_Address1, addressData.Address1);
			AssertEquals("City", docAddress.E2_City, addressData.City);
			AssertEquals("Postcode", docAddress.E2_Postcode, addressData.Postcode);
			AssertEquals("AddressType", docAddress.DocAddressType.ToString(), addressData.AddressType);
		}

		void AssertAdditionalReference(AdditionalReference additionalReference, ZString type, ZString number)
		{
			AssertEquals(type, additionalReference.Type.GetCodeAsUpperCase());
			AssertEquals(number, additionalReference.ReferenceNumber.GetValueOrDefault());
		}

		void AssertConfirmation(List<Confirmation> confirmations, ZDateTime deliverActualTime, ZDateTime pickupActualTime, int dlvLegLink, int dlvLegSequence, int picLegLink, int picLegSequence, bool isWait)
		{
			if (!deliverActualTime.IsEmpty)
			{
				var deliveryConfirm = confirmations.FirstOrDefault(c => c.DateDescription.Equals(ConfirmationTypes.Codes.Delivery));
				AssertEquals(deliverActualTime.AddMinutes(isWait ? -10 : -12), deliveryConfirm.EstimatedDate);
				AssertEquals(deliverActualTime.AddMinutes(isWait ? -9 : -11), deliveryConfirm.EstimatedOutDate);
				AssertEquals(deliverActualTime.AddMinutes(1), deliveryConfirm.ActualDate);
				AssertEquals(deliverActualTime.AddMinutes(2), deliveryConfirm.ActualOutDate);
				AssertEquals(dlvLegLink, deliveryConfirm.LegLink);
			}

			if (isWait && pickupActualTime.IsEmpty)
			{
				pickupActualTime = deliverActualTime;
			}

			if (!pickupActualTime.IsEmpty)
			{
				pickupActualTime = pickupActualTime.IsEmpty ? deliverActualTime : pickupActualTime;
				var pickupConfirm = confirmations.FirstOrDefault(c => c.DateDescription.Equals(ConfirmationTypes.Codes.PickUp));
				AssertEquals(pickupActualTime.AddMinutes(-10), pickupConfirm.EstimatedDate);
				AssertEquals(pickupActualTime.AddMinutes(-9), pickupConfirm.EstimatedOutDate);
				AssertEquals(pickupActualTime.AddMinutes(1), pickupConfirm.ActualDate);
				AssertEquals(pickupActualTime.AddMinutes(2), pickupConfirm.ActualOutDate);
				AssertEquals(picLegLink, pickupConfirm.LegLink);
			}
		}

		LocalCartageTestHelper Helper
		{
			get
			{
				return helper ?? (helper = new LocalCartageTestHelper(Factory));
			}
		}

		LocalCartageTestHelper helper;

		protected override void SetUp()
		{
			CommonCartageBehaviorStrategyProvider.SetProvider(Factory, new CartageBehaviorStrategyProvider());
			base.SetUp();
		}

		public static void SetupAdditionalReference(CusEntryNumber additionalReference, ZString type, ZString number)
		{
			additionalReference.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			additionalReference.CE_EntryType = type;
			additionalReference.CE_EntryNum = number;
		}
	}
}
