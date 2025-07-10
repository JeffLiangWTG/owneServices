using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Business.Testing
{
	[TestedType(typeof(DivotsWithPackagesCollection))]
	class DivotsWithPackagesCollectionTest : ActiveBusinessObjectCollectionTestCase<DivotsWithPackagesCollection>
	{
		public void TestConstructorDoesNotAllowNullInstruction()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new DivotsWithPackagesCollection(null));
		}

		public void TestAllowNew_Instruction()
		{
			var instruction = GetNewInstruction();
			var packages = new DivotsWithPackagesCollection(instruction);
			AssertEquals(false, ((IBindingList)packages).AllowNew);
		}

		public void TestAddRemove_Instruction()
		{
			var instruction = GetNewInstruction();
			var packages = new DivotsWithPackagesCollection(instruction);
			AssertEquals("Precondition", 0, packages.Count);

			var packageDivot = instruction.PackageDivots.AddNew();
			AssertEquals("divot doesn't have a package yet, so should still be 0", 0, packages.Packages.Count());

			var package = Factory.New<PkgPackage>();
			packageDivot.KD_KP_Package = package.PK;
			AssertContainsExactElementsInAnyOrder("after setting the package it should have found it, if not, check to see if listening onto kd_kp", new[] { package }, packages.Packages);

			var packageDivot2 = instruction.PackageDivots.AddNew();
			var package2 = Factory.New<PkgPackage>();
			packageDivot2.KD_KP_Package = package2.PK;
			AssertContainsExactElementsInAnyOrder("should now have 2", new[] { package, package2 }, packages.Packages);

			packageDivot2.KD_KP_Package = ZGuid.Empty;
			AssertContainsExactElementsInAnyOrder("should now only have the first one", new[] { package }, packages.Packages);

			packageDivot.KD_KN_BookingInstruction = ZGuid.Empty;
			AssertEquals("Disconnect from instruction instead, should now have none", 0, packages.Packages.Count());
		}

		public void TestAddPackage()
		{
			var instruction = GetNewInstruction();
			var packages = new DivotsWithPackagesCollection(instruction);
			var package = Factory.New<PkgPackage>();
			packages.AddPackage(null);
			AssertEquals(0, instruction.PackageDivots.Count);

			packages.AddPackage(package);
			AssertEquals(1, instruction.PackageDivots.Count);
			AssertEquals(package.PK, instruction.PackageDivots[0].KD_KP_Package);

			packages.AddPackage(package);
			AssertEquals(1, instruction.PackageDivots.Count);
		}

		public void TestAddPackageForSubInstructionThrowsException()
		{
			var helper = new TransportBookingTestHelper(Factory);
			var masterBooking = helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			Factory.Save();

			var subBooking = helper.CreateBooking();
			var package = helper.CreatePackage("LOOSE1");
			subBooking.PackageJob.Packages.Add(package);
			masterBooking.SubBookings.Add(subBooking);
			Factory.Save();

			var masterInstruction = masterBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var subInstruction = subBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			subInstruction.KN_KN_MasterBookingInstruction = masterInstruction.PK;

			var subDivots = new DivotsWithPackagesCollection(subInstruction);
			AssertExceptionThrown<InvalidOperationException>("DivotsWithPackagesCollection.AddPackage() should throw an InvalidOperationException", "Cannot call AddPackage() on divots for sub instruction", () => subDivots.AddPackage(package));
		}

		public void TestAddPackages()
		{
			var address = Factory.New<OrgAddress>();
			address.OA_AIREquipmentNeeded = "DM1";
			address.OA_LCLEquipmentNeeded = "DM2";
			address.OA_FCLEquipmentNeeded = "DM3";

			var instruction = GetNewInstruction();
			instruction.Address.E2_OA_Address = address.PK;
			instruction.KN_DropMode = "";

			var dropModeChangedHitCount = 0;
			instruction.KN_DropModeInfo.ValueChanged += (sender, e) => dropModeChangedHitCount++;

			var packages = new DivotsWithPackagesCollection(instruction);
			var package1 = Factory.New<PkgPackage>();
			var package2 = Factory.New<PkgPackage>();
			packages.AddPackages(new[] { package1, package2 });
			AssertContainsExactElementsInAnyOrder(new[] { package1, package2 }, packages.Packages);
			AssertEquals("Should have defaulted Drop Mode.", "DM2", instruction.KN_DropMode);
			AssertEquals("Should only have changed Drop Mode once.", 1, dropModeChangedHitCount);
		}

		public void TestAddPackagesForSubInstructionThrowsException()
		{
			var helper = new TransportBookingTestHelper(Factory);
			var masterBooking = helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			Factory.Save();

			var subBooking = helper.CreateBooking();
			var package = helper.CreatePackage("LOOSE1");
			subBooking.PackageJob.Packages.Add(package);
			masterBooking.SubBookings.Add(subBooking);
			Factory.Save();

			var masterInstruction = masterBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var subInstruction = subBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			subInstruction.KN_KN_MasterBookingInstruction = masterInstruction.PK;

			var subDivots = new DivotsWithPackagesCollection(subInstruction);
			AssertExceptionThrown<InvalidOperationException>("DivotsWithPackagesCollection.AddPackage() should throw an InvalidOperationException", "Cannot call AddPackages() on divots for sub instruction", () => subDivots.AddPackages(new[] { package }));
		}

		public void TestAddPackages_UpdatesQuantityIfReAddingExistingPackage()
		{
			var instruction = GetNewInstruction();
			var package = Factory.New<PkgPackage>();
			package.KP_PackageQty = 10;

			var packages = new DivotsWithPackagesCollection(instruction);
			packages.AddPackage(package);
			AssertContainsExactElementsInAnyOrder(new[] { package }, packages.Packages);
			AssertEquals("Quantity should be based on Package.", 10, packages.Single().KD_Quantity);

			package.KP_PackageQty = 1;
			packages.AddPackages(new[] { package });
			AssertContainsExactElementsInAnyOrder(new[] { package }, packages.Packages);
			AssertEquals("Quantity should be updated.", 1, packages.Single().KD_Quantity);
		}

		public void TestContains()
		{
			var instruction = GetNewInstruction();
			var packages = new DivotsWithPackagesCollection(instruction);
			var package = Factory.New<PkgPackage>();
			AssertEquals(false, packages.Contains(package));

			var divot = packages.AddNew();
			AssertEquals(false, packages.Contains(package));

			divot.KD_KP_Package = package.PK;
			AssertEquals(true, packages.Contains(package));

			divot.Delete();
			AssertEquals(false, packages.Contains(package));
		}

		public void TestIndexer()
		{
			var instruction = GetNewInstruction();
			var packages = new DivotsWithPackagesCollection(instruction);
			var divot = packages.AddNew();
			DtbBookingInstructionPkgDivot indexerDivot = packages[0];
			AssertEquals(divot, indexerDivot);
		}

		public void TestPackages()
		{
			var instruction = GetNewInstruction();
			var packages = new DivotsWithPackagesCollection(instruction);
			var package1 = Factory.New<PkgPackage>();
			var package2 = Factory.New<PkgPackage>();

			var divot1 = packages.AddNew();
			packages.AddNew();
			var divot3 = packages.AddNew();
			divot1.KD_KP_Package = package1.PK;
			divot3.KD_KP_Package = package2.PK;
			AssertContainsExactElementsInAnyOrder(new[] { package1, package2 }, packages.Packages);

			package2.Delete();
			AssertContainsExactElementsInAnyOrder(new[] { package1 }, packages.Packages);
		}

		public void TestDivotsWithPackagesOnSubInstruction()
		{
			var helper = new TransportBookingTestHelper(Factory);
			var masterBooking = helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			Factory.Save();

			var subBooking1 = helper.CreateBooking();
			var subBooking1Container = helper.CreatePackageContainer("CONT1");
			var subBooking1ContainerInnerLoosePackage = helper.CreatePackage("CONT1_IN");
			var subBooking1LoosePackage = helper.CreatePackage("LOOSE1");
			subBooking1.PackageJob.Packages.Add(subBooking1Container);
			subBooking1Container.Packages.Add(subBooking1ContainerInnerLoosePackage);
			subBooking1.PackageJob.Packages.Add(subBooking1LoosePackage);

			var subBooking2 = helper.CreateBooking();
			var subBooking2Container = helper.CreatePackageContainer("CONT2");
			var subBooking2ContainerInnerLoosePackage = helper.CreatePackage("CONT2_IN");
			var subBooking2LoosePackage = helper.CreatePackage("LOOSE2");
			subBooking2.PackageJob.Packages.Add(subBooking2Container);
			subBooking2Container.Packages.Add(subBooking2ContainerInnerLoosePackage);
			subBooking2.PackageJob.Packages.Add(subBooking2LoosePackage);
			masterBooking.SubBookings.AddRange(new[] { subBooking1, subBooking2 });
			Factory.Save();

			var masterInstruction = masterBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var subBooking1Instruction = subBooking1.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			subBooking1Instruction.KN_KN_MasterBookingInstruction = masterInstruction.PK;
			var subBooking2Instruction = subBooking2.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			subBooking2Instruction.KN_KN_MasterBookingInstruction = masterInstruction.PK;
			var masterInstructionContainer1Divot = helper.CreatePackageDivot(masterInstruction, subBooking1Container);
			var masterInstructionContainer1InnerPackageDivot = helper.CreatePackageDivot(masterInstruction, subBooking1ContainerInnerLoosePackage);
			var masterInstructionLoosePackage1Divot = helper.CreatePackageDivot(masterInstruction, subBooking1LoosePackage);
			var masterInstructionContainer2Divot = helper.CreatePackageDivot(masterInstruction, subBooking2Container);
			var masterInstructionContainer2InnerPackageDivot = helper.CreatePackageDivot(masterInstruction, subBooking2ContainerInnerLoosePackage);
			var masterInstructionLoosePackage2Divot = helper.CreatePackageDivot(masterInstruction, subBooking2LoosePackage);
			Factory.Save();

			var subBooking1DivotsWithPackages = new DivotsWithPackagesCollection(subBooking1Instruction);
			var divotsForSubBooking1 = subBooking1DivotsWithPackages.Select(d => (instructionPK: d.Instruction.PK, packagePK: d.Package.PK)).ToArray();
			var expectedDivotsForSubBooking1 = new (ZGuid instructionPK, ZGuid packagePK)[]
			{
				(instructionPK: masterInstruction.PK, packagePK: subBooking1Container.PK),
				(instructionPK: masterInstruction.PK, packagePK: subBooking1ContainerInnerLoosePackage.PK),
				(instructionPK: masterInstruction.PK, packagePK: subBooking1LoosePackage.PK),
			};

			var subBooking2DivotsWithPackages = new DivotsWithPackagesCollection(subBooking2Instruction);
			var divotsForSubBooking2 = subBooking2DivotsWithPackages.Select(d => (instructionPK: d.Instruction.PK, packagePK: d.Package.PK)).ToArray();
			var expectedDivotsForSubBooking2 = new (ZGuid instructionPK, ZGuid packagePK)[]
			{
				(instructionPK: masterInstruction.PK, packagePK: subBooking2Container.PK),
				(instructionPK: masterInstruction.PK, packagePK: subBooking2ContainerInnerLoosePackage.PK),
				(instructionPK: masterInstruction.PK, packagePK: subBooking2LoosePackage.PK),
			};

			CombineAssertions("Check divots in DivotsWithPackagesCollection for sub-booking instructions go from masterInstruction to the packages on the subBooking", () =>
			{
				AssertContainsExactElementsInAnyOrder("Divots in DivotsWithPackagesCollection for subBooking1 should be the divots between masterInstruction and subBooking1's outer and inner packages", expectedDivotsForSubBooking1, divotsForSubBooking1);
				AssertContainsExactElementsInAnyOrder("Divots in DivotsWithPackagesCollection for subBooking2 should be the divots between masterInstruction and subBooking2's outer and inner packages", expectedDivotsForSubBooking2, divotsForSubBooking2);
			});
		}

		public void TestRemovePackage()
		{
			var instruction = GetNewInstruction();
			var packages = new DivotsWithPackagesCollection(instruction);
			var package1 = Factory.New<PkgPackage>();
			var package2 = Factory.New<PkgPackage>();
			var divot1 = instruction.PackageDivots.AddNew();
			divot1.KD_KP_Package = package1.PK;
			var divot2 = instruction.PackageDivots.AddNew();

			packages.RemovePackage(null);
			AssertContainsExactElementsInAnyOrder(new[] { divot1, divot2 }, instruction.PackageDivots);

			packages.RemovePackage(package2);
			AssertContainsExactElementsInAnyOrder(new[] { divot1, divot2 }, instruction.PackageDivots);

			packages.RemovePackage(package1);
			AssertEquals(1, instruction.PackageDivots.Count);
			AssertContainsExactElementsInAnyOrder(new[] { divot2 }, instruction.PackageDivots);
		}

		public void TestRemovePackageForSubInstructionThrowsException()
		{
			var helper = new TransportBookingTestHelper(Factory);
			var masterBooking = helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			Factory.Save();

			var subBooking = helper.CreateBooking();
			var package = helper.CreatePackage("LOOSE1");
			subBooking.PackageJob.Packages.Add(package);
			masterBooking.SubBookings.Add(subBooking);
			Factory.Save();

			var masterInstruction = masterBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var subInstruction = subBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			subInstruction.KN_KN_MasterBookingInstruction = masterInstruction.PK;
			var masterInstructionDivot = helper.CreatePackageDivot(masterInstruction, package);
			var subDivots = new DivotsWithPackagesCollection(subInstruction);
			AssertExceptionThrown<InvalidOperationException>("DivotsWithPackagesCollection.RemovePackage() should throw an InvalidOperationException", "Cannot call RemovePackage() on divots for sub instruction", () => subDivots.RemovePackage(package));
		}

		public void TestRemovePackage_PackageDetatchedFromRow()
		{
			var instruction = GetNewInstruction();
			var packages = new DivotsWithPackagesCollection(instruction);
			var rowFactory = ((IBusinessObjectFactoryInternals)Factory).RowFactory;
			var package1 = new PkgPackage(Factory, rowFactory.New(PkgPackageSchema.Constants.TableName));
			var package2 = new PkgPackage(Factory, rowFactory.New(PkgPackageSchema.Constants.TableName));
			var divot1 = instruction.PackageDivots.AddNew();
			divot1.KD_KP_Package = package1.PK;
			var divot2 = instruction.PackageDivots.AddNew();

			packages.RemovePackage(null);
			AssertContainsExactElementsInAnyOrder(new[] { divot1, divot2 }, instruction.PackageDivots);

			packages.RemovePackage(package2);
			AssertContainsExactElementsInAnyOrder(new[] { divot1, divot2 }, instruction.PackageDivots);

			packages.RemovePackage(package1);
			AssertEquals(1, instruction.PackageDivots.Count);
			AssertContainsExactElementsInAnyOrder(new[] { divot2 }, instruction.PackageDivots);
		}

		public void TestTyped()
		{
			var instruction = GetNewInstruction();
			var packages = new DivotsWithPackagesCollection(instruction);
			var divot1 = packages.AddNew();
			var divot2 = packages.AddNew();
			AssertContainsExactElementsInAnyOrder(new[] { divot1, divot2 }, packages.Typed);
		}

		public void TestSettingDelayInstructionUpdatesFromAddingDivotsSkipsOnPackageAdded()
		{
			var (booking, container, deliveryToCNEInstruction3, pickupConfirmation1, deliveryToCNEConfirmation3, containerReturnDeliveryConfirmation4) = CoreDelayAndAfterPackagesAddedTestSetup();

			using (ActiveBusinessObjectCollection.DelayListChangedEvents(Factory))
			{
				booking.DelayInstructionUpdatesFromAddingDivots = true;
				deliveryToCNEInstruction3.DivotsWithPackages.AddPackage(container);
			}

			CombineAssertions($"Check that all updates in OnPackageAdded() have been delayed by setting {nameof(booking)}.DelayInstructionUpdatesFromAddingDivots", () =>
			{
				AssertEquals($"{nameof(pickupConfirmation1)} should not have KK_IsEmptyContainer set true", false, pickupConfirmation1.KK_IsEmptyContainer);
				AssertEquals($"{nameof(deliveryToCNEConfirmation3)} should not have its KK_IsEmptyContainer set true", false, deliveryToCNEConfirmation3.KK_IsEmptyContainer);
				AssertEquals($"{nameof(containerReturnDeliveryConfirmation4)} should not have its KK_IsEmptyContainer set true (due to the delay being active)", false, containerReturnDeliveryConfirmation4.KK_IsEmptyContainer);
				AssertEquals($"{nameof(booking)}.KM_IsHazardous should not have been set true (due to the delay being active)", false, booking.KM_IsHazardous);
				AssertEquals($"{nameof(booking)}.KM_RequiresRefrigeration should not have been set true (due to the delay being active)", false, booking.KM_RequiresRefrigeration);
			});
		}

		public void TestAfterPackagesAdded()
		{
			var (booking, container, deliveryToCNEInstruction3, pickupConfirmation1, deliveryToCNEConfirmation3, containerReturnDeliveryConfirmation4) = CoreDelayAndAfterPackagesAddedTestSetup();

			using (ActiveBusinessObjectCollection.DelayListChangedEvents(Factory))
			{
				booking.DelayInstructionUpdatesFromAddingDivots = true;
				deliveryToCNEInstruction3.DivotsWithPackages.AddPackage(container);
			}
			booking.DelayInstructionUpdatesFromAddingDivots = false;

			CombineAssertions($"Precondition: Check that all updates in OnPackageAdded() have been delayed by setting {nameof(booking)}.DelayInstructionUpdatesFromAddingDivots", () =>
			{
				AssertEquals($"Precondition: {nameof(pickupConfirmation1)}.KK_IsEmptyContainer has not been set true", false, pickupConfirmation1.KK_IsEmptyContainer);
				AssertEquals($"Precondition: {nameof(deliveryToCNEConfirmation3)}.KK_IsEmptyContainer has not been set true", false, deliveryToCNEConfirmation3.KK_IsEmptyContainer);
				AssertEquals($"Precondition: {nameof(containerReturnDeliveryConfirmation4)}.KK_IsEmptyContainer has been set false by test setup (to help show that the act of calling AfterPackagesAdded() sets it back to true)", false, containerReturnDeliveryConfirmation4.KK_IsEmptyContainer);
				AssertEquals($"Precondition: {nameof(booking)}.KM_IsHazardous has been set false by test setup (to help show that the act of calling AfterPackagesAdded() sets it back to true)", false, booking.KM_IsHazardous);
				AssertEquals($"Precondition: {nameof(booking)}.KM_RequiresRefrigeration has been set false by test setup (to help show that the act of calling AfterPackagesAdded() sets it back to true)", false, booking.KM_RequiresRefrigeration);
			});

			DivotsWithPackagesCollection.AfterPackagesAdded(deliveryToCNEInstruction3, new PkgPackage[] { container } );

			CombineAssertions("Check that all updates in AfterPackagesAdded() have run", () =>
			{
				AssertEquals($"{nameof(pickupConfirmation1)}.KK_IsEmptyContainer should still not be set true", false, pickupConfirmation1.KK_IsEmptyContainer);
				AssertEquals($"{nameof(deliveryToCNEConfirmation3)}.KK_IsEmptyContainer should still not be set true", false, deliveryToCNEConfirmation3.KK_IsEmptyContainer);
				AssertEquals($"{nameof(containerReturnDeliveryConfirmation4)}.KK_IsEmptyContainer should have been set true", true, containerReturnDeliveryConfirmation4.KK_IsEmptyContainer);
				AssertEquals($"{nameof(booking)}.KM_IsHazardous should have been set true", true, booking.KM_IsHazardous);
				AssertEquals($"{nameof(booking)}.KM_RequiresRefrigeration should have been set true", true, booking.KM_RequiresRefrigeration);
			});
		}

		public void TestOnPackageAdded()
		{
			var (booking, container, deliveryToCNEInstruction3, pickupConfirmation1, deliveryToCNEConfirmation3, containerReturnDeliveryConfirmation4) = CoreDelayAndAfterPackagesAddedTestSetup();

			CombineAssertions($"Precondition: Before running AddPackage() check values have not yet been set", () =>
			{
				AssertEquals($"Precondition: {nameof(pickupConfirmation1)}.KK_IsEmptyContainer has not been set true", false, pickupConfirmation1.KK_IsEmptyContainer);
				AssertEquals($"Precondition: {nameof(deliveryToCNEConfirmation3)}.KK_IsEmptyContainer has not been set true", false, deliveryToCNEConfirmation3.KK_IsEmptyContainer);
				AssertEquals($"Precondition: {nameof(containerReturnDeliveryConfirmation4)}.KK_IsEmptyContainer has been set false by test setup (to help show that the act of calling OnPackageAdded() sets it back to true)", false, containerReturnDeliveryConfirmation4.KK_IsEmptyContainer);
				AssertEquals($"Precondition: {nameof(booking)}.KM_IsHazardous has been set false by test setup (to help show that the act of calling OnPackageAdded() sets it back to true)", false, booking.KM_IsHazardous);
				AssertEquals($"Precondition: {nameof(booking)}.KM_RequiresRefrigeration has been set false by test setup (to help show that the act of calling OnPackageAdded() sets it back to true)", false, booking.KM_RequiresRefrigeration);
			});

			deliveryToCNEInstruction3.DivotsWithPackages.AddPackage(container);

			CombineAssertions("Check that all updates in OnPackageAdded() [run after divot instruction is set in AddPackage()] have run", () =>
			{
				AssertEquals($"{nameof(pickupConfirmation1)}.KK_IsEmptyContainer should still not have been set true", false, pickupConfirmation1.KK_IsEmptyContainer);
				AssertEquals($"{nameof(deliveryToCNEConfirmation3)}.KK_IsEmptyContainer should still not have been set true", false, deliveryToCNEConfirmation3.KK_IsEmptyContainer);
				AssertEquals($"{nameof(containerReturnDeliveryConfirmation4)}.KK_IsEmptyContainer should have been set true", true, containerReturnDeliveryConfirmation4.KK_IsEmptyContainer);
				AssertEquals($"{nameof(booking)}.KM_IsHazardous should have been set true", true, booking.KM_IsHazardous);
				AssertEquals($"{nameof(booking)}.KM_RequiresRefrigeration should have been set true", true, booking.KM_RequiresRefrigeration);
			});
		}

		(DtbBooking booking, PkgPackage container, DtbBookingInstruction deliveryToCNEInstruction3, DtbBookingConfirmation pickupConfirmation1, DtbBookingConfirmation deliveryToCNEConfirmation3, DtbBookingConfirmation containerReturnDeliveryConfirmation4) CoreDelayAndAfterPackagesAddedTestSetup()
		{
			var helper = new TransportBookingTestHelper(Factory);
			var booking = helper.CreateBooking();
			booking.ConsolidationSingleJob.KB_JobDirection = "DLV";
			booking.KM_Direction = "DST";
			booking.KM_KT_NKBookingTemplate = "IFUD";
			AssertEquals("Adding the template 'IFUD' should have added four instructions", 4, booking.Instructions.Count);

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

			AssertEquals($"Precondition: {nameof(booking)}.KM_IsHazardous was set to true after divots on hazardous package {nameof(containerInnerPackage1Hazardous)} were assigned to instructions 1, 2 and 4", true, booking.KM_IsHazardous);
			AssertEquals($"Precondition: {nameof(booking)}.KM_RequiresRefrigeration was set to true after divots on package requiring refrigeration {nameof(containerInnerPackage2Refrigerated)} were assigned to instructions 1, 2 and 4", true, booking.KM_RequiresRefrigeration);
			booking.KM_IsHazardous = false;
			booking.KM_RequiresRefrigeration = false;

			var pickupConfirmation1 = pickupInstruction1.Confirmations.Single();
			var deliveryToCNEConfirmation3 = deliveryToCNEInstruction3.Confirmations.Single();
			var containerReturnDeliveryConfirmation4 = containerReturnDeliveryInstruction4.Confirmations[0];
			AssertEquals($"Precondition: {nameof(containerReturnDeliveryConfirmation4)}.KK_IsEmptyContainer was set true by call to SetIsEmptyContainer triggered during setup", true, containerReturnDeliveryConfirmation4.KK_IsEmptyContainer);
			containerReturnDeliveryConfirmation4.KK_IsEmptyContainer = false;

			return (booking, container, deliveryToCNEInstruction3, pickupConfirmation1, deliveryToCNEConfirmation3, containerReturnDeliveryConfirmation4);
		}

		public void TestAfterPackagesAddedThrowsOnInvalidArguments()
		{
			var instruction = Factory.New<DtbBookingInstruction>();

			AssertExceptionThrown<ArgumentNullException>("AfterPackagesAdded() should fail if packages parameter is null", () => DivotsWithPackagesCollection.AfterPackagesAdded(instruction, null));
			AssertExceptionThrown<ArgumentNullException>("AfterPackagesAdded() should fail if instruction parameter is null", () => DivotsWithPackagesCollection.AfterPackagesAdded(null, Array.Empty<PkgPackage>()));
		}

		public void TestAfterPackagesCalledWithNoPackagesStillUpdatesIsHazardousAndRequiresRefrigeration()
		{
			var helper = new TransportBookingTestHelper(Factory);
			var booking = helper.CreateBooking();
			booking.ConsolidationSingleJob.KB_JobDirection = "DLV";
			booking.KM_Direction = "DST";
			var instruction = booking.Instructions.AddNew(instructionType: "PIC", organisationType: "CNE");

			var container = helper.CreatePackage("CONT1", 1, "CNT");
			var containerInnerPackage1Haz = helper.CreatePackage("CONT1_IN1_HAZ", 1);
			containerInnerPackage1Haz.UNDGs.AddNew();
			var containerInnerPackage2Refrig = helper.CreatePackage("CONT1_IN2_REFRIG", 1);
			containerInnerPackage2Refrig.KP_RequiresTemperatureControl = true;

			booking.PackageJob.Packages.Add(container);
			container.Packages.Add(containerInnerPackage1Haz);
			container.Packages.Add(containerInnerPackage2Refrig);

			booking.KM_IsHazardous = true;
			booking.KM_RequiresRefrigeration = true;

			// simulate what happens when DefaultPackagesCore() called with zero packages
			DivotsWithPackagesCollection.AfterPackagesAdded(instruction, Array.Empty<PkgPackage>());

			CombineAssertions("Check that booking updated IsHazardous and RequiresRefrigeration flags back to false", () =>
			{
				AssertEquals(false, booking.KM_IsHazardous);
				AssertEquals(false, booking.KM_RequiresRefrigeration);
			});
		}

		protected override DivotsWithPackagesCollection GetCollectionToTest()
		{
			return new DivotsWithPackagesCollection(Instruction);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			// Relationship doesn't support AddNew ... So setup package and return 

			var packageDivot = Factory.New<DtbBookingInstructionPkgDivot>();
			var package = Factory.New<PkgPackage>();
			packageDivot.KD_KP_Package = package.PK;
			return packageDivot;
		}

		DtbBookingInstruction Instruction
		{
			get { return instruction ?? (instruction = GetNewInstruction()); }
		}

		DtbBookingInstruction GetNewInstruction()
		{
			return Factory.New<DtbBookingInstruction>();
		}

		DtbBookingInstruction instruction;
	}

	[TestedType(typeof(PackageCollectionForBooking))]
	public class DtbBookingPackageRelationshipTest : ActiveBusinessObjectCollectionTestCase<PackageCollectionForBooking>
	{
		public void TestAddRemove()
		{
			var booking = Booking;
			var packages = new PackageCollectionForBooking(booking);
			AssertEquals("Precondition", 0, packages.Count);

			var instruction = booking.Instructions.AddNew();
			AssertEquals("Precondition", 0, packages.Count);

			var packageDivot = instruction.PackageDivots.AddNew();
			AssertEquals("divot doesn't have a package yet, so should still be 0", 0, packages.Count);

			var package = Factory.New<PkgPackage>();
			packageDivot.KD_KP_Package = package.PK;
			AssertContainsExactElementsInAnyOrder("after setting the package it should have found it, if not, check to see if listening onto kd_kp", new PkgPackage[] { package }, packages);

			var packageDivot2 = instruction.PackageDivots.AddNew();
			var package2 = Factory.New<PkgPackage>();
			packageDivot2.KD_KP_Package = package2.PK;
			AssertContainsExactElementsInAnyOrder("should now have 2", new PkgPackage[] { package, package2 }, packages);

			packageDivot2.KD_KP_Package = ZGuid.Empty;
			AssertContainsExactElementsInAnyOrder("should now only have the first one", new PkgPackage[] { package }, packages);

			packageDivot.KD_KN_BookingInstruction = ZGuid.Empty;
			AssertEquals("Disconnect from instruction instead, should now have none", 0, packages.Count);

			var instruction3 = booking.Instructions.AddNew();
			var packageDivot3 = instruction3.PackageDivots.AddNew();
			var package3 = Factory.New<PkgPackage>();
			packageDivot3.KD_KP_Package = package3.PK;
			AssertContainsExactElementsInAnyOrder("after setting the package it should have found it, if not, check to see if listening onto kd_kp", new PkgPackage[] { package3 }, packages);
		}

		protected override PackageCollectionForBooking GetCollectionToTest()
		{
			return new PackageCollectionForBooking(Booking);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			// Relationship doesn't support AddNew ... So setup package and return 
			var packageDivot = Instruction.PackageDivots.AddNew();
			var package = Factory.New<PkgPackage>();
			packageDivot.KD_KP_Package = package.PK;

			return package;
		}

		DtbBooking Booking
		{
			get { return booking ?? (booking = GetNewBookingBizO()); }
		}
		DtbBooking booking;

		DtbBookingInstruction Instruction
		{
			get { return instruction ?? (instruction = Booking.Instructions.AddNew()); }
		}
		DtbBookingInstruction instruction;

		DtbBooking GetNewBookingBizO()
		{
			return Helper.CreateBooking();
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}
		TransportBookingTestHelper helper;
	}

	[TestedType(typeof(PackageCollectionForBooking))]
	public class DtbBookingPackageCollectionForTransportTest : ActiveBusinessObjectCollectionTestCase<PackageCollectionForBooking>
	{
		public void TestAllowNew_Booking()
		{
			var booking = GetNewBookingBizO();
			var packages = new PackageCollectionForBooking(booking);
			AssertEquals(false, ((IBindingList)packages).AllowNew);
		}

		public void TestAddRemove_Movement()
		{
			var booking = GetNewBookingBizO();
			var packages = new PackageCollectionForBooking(booking);
			AssertEquals("Precondition", 0, packages.Count);

			var instruction = booking.Instructions.AddNew();
			AssertEquals("Precondition", 0, packages.Count);

			var packageDivot = instruction.PackageDivots.AddNew();
			AssertEquals("divot doesn't have a package yet, so should still be 0", 0, packages.Count);

			var package = Factory.New<PkgPackage>();
			packageDivot.KD_KP_Package = package.PK;
			AssertContainsExactElementsInAnyOrder("after setting the package it should have found it, if not, check to see if listening onto kd_kp", new PkgPackage[] { package }, packages);

			var packageDivot2 = instruction.PackageDivots.AddNew();
			var package2 = Factory.New<PkgPackage>();
			packageDivot2.KD_KP_Package = package2.PK;
			AssertContainsExactElementsInAnyOrder("should now have 2", new PkgPackage[] { package, package2 }, packages);

			packageDivot2.KD_KP_Package = ZGuid.Empty;
			AssertContainsExactElementsInAnyOrder("should now only have the first one", new PkgPackage[] { package }, packages);

			packageDivot.KD_KN_BookingInstruction = ZGuid.Empty;
			AssertEquals("Disconnect from instruction instead, should now have none", 0, packages.Count);
		}

		protected override PackageCollectionForBooking GetCollectionToTest()
		{
			return new PackageCollectionForBooking(Booking);
		}

		DtbBooking Booking
		{
			get { return booking ?? (booking = GetNewBookingBizO()); }
		}
		DtbBooking booking;

		DtbBookingInstruction Instruction
		{
			get { return instruction ?? (instruction = Booking.Instructions.AddNew()); }
		}
		DtbBookingInstruction instruction;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			// Relationship doesn't support AddNew ... So setup package and return 
			var packageDivot = Instruction.PackageDivots.AddNew();
			var package = Factory.New<PkgPackage>();
			packageDivot.KD_KP_Package = package.PK;
			return package;
		}

		DtbBooking GetNewBookingBizO()
		{
			return Helper.CreateBooking();
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}
		TransportBookingTestHelper helper;
	}
}
