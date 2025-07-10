using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.TransportBooking;
using Enterprise.Integration.TransportConsignment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Security;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework.TestHelper;

namespace Enterprise.TransportCommon.Business.Testing
{
	public abstract class DtbTransportTest : DtbTransportBusinessObjectTestCase
	{
		#region TestLoadingAsWrongTypeWithoutFactorySave

		public void TestLoadingAsWrongTypeWithoutFactorySave()
		{
			var typesToLoad = new[] { ObjectFactory.GetType<IDtbBooking>(), ObjectFactory.GetType<IDtbBookingConsignment>() };
			AssertLoadingAsWrongType(typesToLoad, factorySaveBeforeLoad: false);
		}

		#endregion

		#region TestLoadingAsWrongTypeWithFactorySave

		public void TestLoadingAsWrongTypeWithFactorySave()
		{
			var typesToLoad = new[] { ObjectFactory.GetType<IDtbBooking>(), ObjectFactory.GetType<IDtbBookingConsignment>() };
			AssertLoadingAsWrongType(typesToLoad, factorySaveBeforeLoad: true);
		}

		#endregion

		#region Metadata

		protected override Type ExpectedMetadataType
		{
			get
			{
				return typeof(Metadata.Business.DtbTransport);
			}
		}

		#endregion

		#region Related Entities

		#region TestBusinessObjectsWithRelatedEventsCore

		public void TestBusinessObjectsWithRelatedEventsCore()
		{
			var transport = (DtbTransport)GetNewBusinessObject();
			var instructionA = (DtbTransportInstruction)transport.Instructions.AddNew();
			var instructionB = (DtbTransportInstruction)transport.Instructions.AddNew();
			var confirmationA1 = instructionA.Confirmations.AddNew();
			var confirmationA2 = instructionA.Confirmations.AddNew();
			var confirmationB1 = instructionB.Confirmations.AddNew();
			var confirmationB2 = instructionB.Confirmations.AddNew();

			AssertContainsExactElementsInAnyOrder(new BusinessObject[] { confirmationA1, confirmationA2, confirmationB1, confirmationB2, instructionA, instructionB }, transport.BusinessObjectsWithRelatedEvents);
		}

		#endregion

		#region TestIAdditionalReferenceNumberTypeProvider

		public void TestIAdditionalReferenceNumberTypeProvider()
		{
			var transport = (DtbTransport)GetNewBusinessObject();

			var additionalReferenceNumberLookups = transport.AdditionalReferenceNumbers.AddNew().Lookups;
			var additionalReferenceNumberTypes = additionalReferenceNumberLookups.GetType().GetProperty("AdditionalReferenceNumberTypes").GetValue(additionalReferenceNumberLookups, null);

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					"ETB|External Transport Booking Number|Y",
					"TRF|Transport Reference Number|Y",
					"CIN|Commercial Invoice Number|Y",
					"CLR|Customer Reference Number|Y",
					"HSB|House Bill|Y",
					"MAB|Master Bill|N",
					"ORD|Order Number|N",
					"BPR|Booking Party Reference|Y",
				}.Concat(ExpectedAdditionalReferenceNumberTypesCore),
				((CodeDescriptionPairList)additionalReferenceNumberTypes)
				.Cast<TransportReferenceNumberType>()
				.Select((number) => String.Format("{0}|{1}|{2}", number.Code, number.Description, number.IsUnique))
				.ToArray());
		}

		protected virtual string[] ExpectedAdditionalReferenceNumberTypesCore
		{
			get { return Array.Empty<string>(); }
		}

		#endregion

		#region TestBookingTemplate

		public void TestBookingTemplate()
		{
			var template = CreateTransportTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import);
			var ctoInstructionTemplate = Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CTO, InstructionTypes.Codes.PickUp);
			var cneInstructionTemplate = Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CNE, InstructionTypes.Codes.Delivery);
			var cydInstructionTemplate = Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CYD, InstructionTypes.Codes.Delivery);

			var consolidation = (DtbTransportConsolidation)Factory.New(ExpectedConsolidationType);
			var booking = (DtbTransport)GetNewBusinessObject();

			booking.KM_KB_Booking = consolidation.PK;
			AssertNull(booking.BookingTemplate);

			booking.KM_KT_NKBookingTemplate = template.KT_Code;
			AssertEquals(template, booking.BookingTemplate);
		}

		protected abstract Type ExpectedTemplateType { get; }

		#endregion

		#region TestConsolidationSingleJob

		public void TestConsolidationSingleJob()
		{
			var consolidation = (DtbTransportConsolidation)Factory.New(ExpectedConsolidationType);
			var transport = (DtbTransport)Factory.New(TestedTypeHelper.GetTestedType(GetType()));

			transport.KM_KB_Booking = consolidation.PK;

			AssertEquals(consolidation, transport.ConsolidationSingleJob);
			AssertEquals(ExpectedConsolidationType, transport.ConsolidationSingleJob.GetType());
		}

		protected abstract Type ExpectedConsolidationType { get; }

		#endregion

		#region TestInstructions

		public void TestInstructions()
		{
			var transport = (DtbTransport)GetNewBusinessObject();

			transport.Instructions.AddNew();
			AssertEquals(ExpectedInstructionCollectionType, transport.Instructions.GetType());
			AssertEquals("Instructions should be registered editable on Transport.", true, transport.IsRegisteredEditableChildObject(transport.Instructions));
		}

		protected abstract Type ExpectedInstructionCollectionType { get; }

		#endregion

		#region TestJob

		public void TestJob()
		{
			var transportJob = GetSaveableTransportJob();
			AssertNull(transportJob.Job);

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = transportJob.PK;
			job.JH_ParentTableCode = DtbBookingSchema.Constants.Prefix;
			AssertNotNull(transportJob.Job);
		}

		#endregion

		#region TestCancelDeletesJobHeader

		public void TestCancelDeletesJobHeader()
		{
			var booking = GetSaveableTransportJob();

			var job = new JobHeader.Loader(booking).TryLoadOrCreate();
			job.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery()).PK;
			AssertEquals(true, ((ZString)booking.CanCancel()).IsEmpty);
			AssertNotNull(booking.Job);
			Factory.Save();

			booking.IsCancelled = true;
			Factory.Save();

			AssertNull("Expected JobHeader is removed.", booking.Job);
			AssertEquals(true, job.IsCancelled);
		}

		#endregion

		#region TestCancelDeactivatesJobHeader

		public void TestCancelDeactivatesJobHeader()
		{
			var booking = GetSaveableTransportJob();

			var job = new JobHeader.Loader(booking).TryLoadOrCreate();
			job.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery()).PK;
			AssertEquals(true, ((ZString)booking.CanCancel()).IsEmpty);
			AssertNotNull(booking.Job);
			Factory.Save();

			booking.IsCancelled = true;
			Factory.Save();

			AssertNull("Expected JobHeader is removed.", booking.Job);
			AssertEquals(true, job.IsCancelled);
		}

		#endregion

		#region TestPackages

		public void TestPackages()
		{
			var transport = (DtbTransport)GetNewBusinessObject();

			AssertEquals(0, transport.AssignedPackages.Count);

			var instruction = (DtbTransportInstruction)transport.Instructions.AddNew();
			AssertEquals(0, transport.AssignedPackages.Count);

			var packageDivot = (DtbTransportInstructionPkgDivot)instruction.PackageDivots.AddNew();
			AssertEquals(0, transport.AssignedPackages.Count);

			var package = Factory.New<PkgPackage>();
			AssertEquals(0, transport.AssignedPackages.Count);

			packageDivot.KD_KP_Package = package.PK;
			AssertContainsExactElementsInAnyOrder(new PkgPackage[] { package }, transport.AssignedPackages);

			var instruction2 = (DtbTransportInstruction)transport.Instructions.AddNew();
			var packageDivot2 = (DtbTransportInstructionPkgDivot)instruction2.PackageDivots.AddNew();
			var package2 = Factory.New<PkgPackage>();
			packageDivot2.KD_KP_Package = package2.PK;
			AssertContainsExactElementsInAnyOrder(new PkgPackage[] { package, package2 }, transport.AssignedPackages);
		}

		#endregion

		#region TestPackages_PackageView

		public void TestPackages_PackageView()
		{
			var transport = (DtbTransport)GetNewBusinessObject();

			AssertEquals(0, transport.Packages_PackageView.Count);
			AssertEquals(true, transport.IsRegisteredEditableChildObject(transport.Packages_PackageView));

			var instruction = (DtbTransportInstruction)transport.Instructions.AddNew();
			AssertEquals(0, transport.Packages_PackageView.Count);

			var packageDivot = (DtbTransportInstructionPkgDivot)instruction.PackageDivots.AddNew();
			AssertEquals(0, transport.Packages_PackageView.Count);

			var package = Factory.New<PkgPackage>();
			AssertEquals(0, transport.Packages_PackageView.Count);

			packageDivot.KD_KP_Package = package.PK;
			AssertContainsExactElementsInAnyOrder(new PkgPackage[] { package }, Array.ConvertAll(transport.Packages_PackageView.ToArray<Package_PackageView>(), p => p.Package));

			var instruction2 = (DtbTransportInstruction)transport.Instructions.AddNew();
			var packageDivot2 = (DtbTransportInstructionPkgDivot)instruction2.PackageDivots.AddNew();
			var package2 = Factory.New<PkgPackage>();
			packageDivot2.KD_KP_Package = package2.PK;
			AssertContainsExactElementsInAnyOrder(new PkgPackage[] { package, package2 }, Array.ConvertAll(transport.Packages_PackageView.ToArray<Package_PackageView>(), p => p.Package));
		}

		#endregion

		#region TestPackageDivots

		public void TestPackageDivots()
		{
			var transport = (DtbTransport)GetNewBusinessObject();

			AssertEquals(0, transport.PackageDivots.Count);

			var instruction = (DtbTransportInstruction)transport.Instructions.AddNew();
			AssertEquals(0, transport.PackageDivots.Count);

			var packageDivot = (DtbTransportInstructionPkgDivot)instruction.PackageDivots.AddNew();
			AssertContainsExactElementsInAnyOrder(new DtbTransportInstructionPkgDivot[] { packageDivot }, transport.PackageDivots);

			var instruction2 = (DtbTransportInstruction)transport.Instructions.AddNew();
			AssertContainsExactElementsInAnyOrder(new DtbTransportInstructionPkgDivot[] { packageDivot }, transport.PackageDivots);

			var packageDivot2 = (DtbTransportInstructionPkgDivot)instruction2.PackageDivots.AddNew();
			AssertContainsExactElementsInAnyOrder(new DtbTransportInstructionPkgDivot[] { packageDivot, packageDivot2 }, transport.PackageDivots);
		}

		#endregion

		#region Notes

		#region TestNoteTypes

		public void TestNoteTypes()
		{
			var transport = (DtbTransport)GetNewBusinessObject();

			AssertContainsExactElementsInAnyOrder(new[]
			{
				PredefinedNoteTypes.Instance.AutoRatingAuditLog,
				PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation,
				PredefinedNoteTypes.Instance.HandlingInstructions,
				PredefinedNoteTypes.Instance.UnmatchedOrgDetails,
				PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes
			}, transport.NoteTypes);
		}

		#endregion

		#region TestNoteContextsForRelatedNotes

		public void TestNoteContextsForRelatedNotes_Module()
		{
			var transport = (DtbTransport)GetNewBusinessObject();
			var noteContexts = transport.GetNoteContextsForRelatedNotesForTest();

			AssertEquals("Cartage should load notes for 'Transport' module only.", StmNoteContextModule.A | StmNoteContextModule.T, noteContexts.Module);
		}

		public void TestNoteContextsForRelatedNotes_Direction()
		{
			CreateAndAssertNoteContextDirection(Constants.CartageDirection.Export, true, false);
			CreateAndAssertNoteContextDirection(Constants.CartageDirection.Origin, true, false);
			CreateAndAssertNoteContextDirection(Constants.CartageDirection.Import, false, true);
			CreateAndAssertNoteContextDirection(Constants.CartageDirection.Destination, false, true);
			CreateAndAssertNoteContextDirection(Constants.CartageDirection.LineHaul, false, false);
			CreateAndAssertNoteContextDirection(Constants.CartageDirection.Local, false, false);
			CreateAndAssertNoteContextDirection("", false, false);
		}

		void CreateAndAssertNoteContextDirection(ZString direction, bool hasExport, bool hasImport)
		{
			var transport = (DtbTransport)GetNewBusinessObject();

			transport.KM_Direction = direction;
			var noteContexts = transport.GetNoteContextsForRelatedNotesForTest();
			AssertEquals("Direction: All", true, noteContexts.Direction.HasFlag(StmNoteContextDirection.A));
			AssertEquals("Direction: Import", hasImport, noteContexts.Direction.HasFlag(StmNoteContextDirection.I));
			AssertEquals("Direction: Export", hasExport, noteContexts.Direction.HasFlag(StmNoteContextDirection.E));
			AssertEquals("Direction: Import and Export", hasImport || hasExport, noteContexts.Direction.HasFlag(StmNoteContextDirection.B));
			AssertEquals("Direction: Domestic", true, noteContexts.Direction.HasFlag(StmNoteContextDirection.D));
			AssertEquals("Direction: Cross Trade", false, noteContexts.Direction.HasFlag(StmNoteContextDirection.X));
			AssertEquals("Direction: All Forwarding", false, noteContexts.Direction.HasFlag(StmNoteContextDirection.F));
			AssertEquals("Direction: Other / Warehouse Out", false, noteContexts.Direction.HasFlag(StmNoteContextDirection.O));
			AssertEquals("Direction: Warehouse In", false, noteContexts.Direction.HasFlag(StmNoteContextDirection.R));
		}

		public void TestNoteContextsForRelatedNotes_FreightMode()
		{
			CreateAndAssertNoteContextFreightMode(true, false);
			CreateAndAssertNoteContextFreightMode(false, true);
			CreateAndAssertNoteContextFreightMode(true, true);
			CreateAndAssertNoteContextFreightMode(false, false);
		}

		void CreateAndAssertNoteContextFreightMode(bool hasContainers, bool hasLoose)
		{
			var transport = (DtbTransport)GetNewBusinessObject();
			var instruction = (DtbTransportInstruction)transport.Instructions.AddNew();

			if (hasContainers)
			{
				var package = Helper.CreatePackage("", 1, "CNT");
				Helper.CreatePackageDivot(instruction, package, 1);
			}

			if (hasLoose)
			{
				var package = Helper.CreatePackage("", 1, "PLT");
				Helper.CreatePackageDivot(instruction, package, 1);
			}

			var noteContexts = transport.GetNoteContextsForRelatedNotesForTest();
			AssertEquals("FreightMode: All", true, noteContexts.FreightMode.HasFlag(StmNoteContextFreightMode.A));
			AssertEquals("FreightMode: Sea", false, noteContexts.FreightMode.HasFlag(StmNoteContextFreightMode.S));
			AssertEquals("FreightMode: FCL", hasContainers, noteContexts.FreightMode.HasFlag(StmNoteContextFreightMode.F));
			AssertEquals("FreightMode: LCL", hasLoose, noteContexts.FreightMode.HasFlag(StmNoteContextFreightMode.L));
			AssertEquals("FreightMode: Air", false, noteContexts.FreightMode.HasFlag(StmNoteContextFreightMode.I));
			AssertEquals("FreightMode: Road", true, noteContexts.FreightMode.HasFlag(StmNoteContextFreightMode.R));
			AssertEquals("FreightMode: Rail", false, noteContexts.FreightMode.HasFlag(StmNoteContextFreightMode.W));
			AssertEquals("FreightMode: Air and Sea", false, noteContexts.FreightMode.HasFlag(StmNoteContextFreightMode.B));
			AssertEquals("FreightMode: Warehouse Orders", false, noteContexts.FreightMode.HasFlag(StmNoteContextFreightMode.O));
			AssertEquals("FreightMode: Warehouse Transfers", false, noteContexts.FreightMode.HasFlag(StmNoteContextFreightMode.T));
			AssertEquals("FreightMode: Warehouse Adjustments", false, noteContexts.FreightMode.HasFlag(StmNoteContextFreightMode.D));
			AssertEquals("FreightMode: Warehouse Periodic Billing", false, noteContexts.FreightMode.HasFlag(StmNoteContextFreightMode.P));
		}

		#endregion

		#endregion

		#endregion

		#region TestSetDefaultValues

		public void TestSetDefaultValues()
		{
			var transportJob = (DtbTransport)GetNewBusinessObject();
			AssertEquals(TransportStatuses.Codes.Available, transportJob.KM_Status);

			AssertEquals(GlbBranch.CurrentBranch.PK, transportJob.KM_GB_Branch);
		}

		#endregion

		#region TestDefaultConfirmations

		public void TestDefaultConfirmations()
		{
			// template
			var template = CreateTransportTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import, RatingFreightModes.Codes.Containerised);
			var ctoInstructionTemplate = Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CTO, InstructionTypes.Codes.PickUp, true, false, PackageCategories.Codes.Containers);
			var cneInstructionTemplate = Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CNE, InstructionTypes.Codes.Delivery, true, true, PackageCategories.Codes.Both);
			var cydInstructionTemplate = Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CYD, InstructionTypes.Codes.Delivery, false, true, PackageCategories.Codes.Outers);

			// consol and booking
			var consolidation = (DtbTransportConsolidation)Factory.New(ExpectedConsolidationType);
			var transport = (DtbTransport)consolidation.Bookings.AddNew();

			// assign packages first
			var packageJob = transport.PackageJob;
			var container = Helper.CreatePackage("CONT", 1, "CNT");
			var containerInnerPackage = Helper.CreatePackage("CONT_IN", 1);
			var containerInnerInnerPackage = Helper.CreatePackage("CONT_IN_IN", 1);
			var topLevelPackage = Helper.CreatePackage("TOP", 1);
			var topInnerPackage = Helper.CreatePackage("TOP_IN", 1);
			packageJob.Packages.Add(container);
			packageJob.Packages.Add(topLevelPackage);
			container.Packages.Add(containerInnerPackage);
			containerInnerPackage.Packages.Add(containerInnerInnerPackage);
			topLevelPackage.Packages.Add(topInnerPackage);

			// set template
			transport.KM_KT_NKBookingTemplate = "IFCL";

			// assert Packages and Confirmations assigned
			AssertEquals("FCL Import", transport.KM_Description);
			AssertEquals(Constants.CartageDirection.Import, transport.KM_Direction);
			AssertEquals(RatingFreightModes.Codes.Containerised, transport.KM_RatingFreightMode);

			var instructions = transport.Instructions.Cast<DtbTransportInstruction>();
			var cto = instructions.First(i => i.KN_Sequence == 1);
			var cne = instructions.First(i => i.KN_Sequence == 2);
			var cyd = instructions.First(i => i.KN_Sequence == 3);
			AssertContainsExactElementsInAnyOrder(new[] { container }, cto.DivotsWithPackages.Packages);
			AssertContainsExactElementsInAnyOrder(new[] { container, containerInnerPackage, topLevelPackage }, cne.DivotsWithPackages.Packages);
			AssertContainsExactElementsInAnyOrder(new[] { container, topLevelPackage }, cyd.DivotsWithPackages.Packages);
			AssertNotNull(cto.Confirmations.Cast<DtbTransportConfirmation>().Single(c => c.IsPickUp));
			AssertNotNull(cne.Confirmations.Cast<DtbTransportConfirmation>().Single(c => c.IsDelivery));
			AssertNotNull(cyd.Confirmations.Cast<DtbTransportConfirmation>().Single(c => c.IsDelivery));
		}

		#endregion

		#region Properties

		// persistent

		#region TestKM_IsActive

		public void TestKM_IsActive()
		{
			var transport = GetSaveableTransportJob();
			AssertEquals("Precondition", TransportStatuses.Codes.Available, transport.KM_Status);
			AssertEquals("Precondition", true, transport.KM_IsActive);

			transport.KM_IsActive = false;
			AssertEquals("Precondition", TransportStatuses.Codes.Deactivated, transport.KM_Status);
			AssertEquals("Precondition", false, transport.KM_IsActive);

			transport.KM_IsActive = true;
			AssertEquals("Precondition", TransportStatuses.Codes.Available, transport.KM_Status);
			AssertEquals("Precondition", true, transport.KM_IsActive);
		}

		#endregion

		#region TestKM_KT_NKBookingTemplate

		public void TestKM_KT_NKBookingTemplate()
		{
			var template = CreateTransportTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import, RatingFreightModes.Codes.Containerised);

			var ctoInstructionTemplate = Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CTO, InstructionTypes.Codes.PickUp, true, false, PackageCategories.Codes.Containers);
			var cneInstructionTemplate = Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CNE, InstructionTypes.Codes.Delivery, true, true, PackageCategories.Codes.Both);
			var cydInstructionTemplate = Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CYD, InstructionTypes.Codes.Delivery, false, true, PackageCategories.Codes.Outers);
			cneInstructionTemplate.K2_DropMode = "SDL";

			var consolidation = (DtbTransportConsolidation)Factory.New(ExpectedConsolidationType);
			var transport = (DtbTransport)consolidation.Bookings.AddNew();
			transport.KM_KT_NKBookingTemplate = "IFCL";

			AssertEquals("FCL Import", transport.KM_Description);
			AssertEquals(Constants.CartageDirection.Import, transport.KM_Direction);
			AssertEquals(RatingFreightModes.Codes.Containerised, transport.KM_RatingFreightMode);
			AssertInstructionAndTemplate(ctoInstructionTemplate, transport.Instructions[0]);
			AssertInstructionAndTemplate(cneInstructionTemplate, transport.Instructions[1]);
			AssertInstructionAndTemplate(cydInstructionTemplate, transport.Instructions[2]);

			transport.KM_KT_NKBookingTemplate = "";
			AssertEquals("", transport.KM_Description);
		}

		protected void AssertInstructionAndTemplate(DtbTransportInstructionTmpl template, DtbTransportInstruction instruction)
		{
			AssertEquals(template.K2_Sequence, instruction.KN_Sequence);
			AssertEquals(template.K2_InstructionType, instruction.KN_InstructionType);
			AssertEquals(template.K2_OrgType, instruction.OrganisationType);
			AssertEquals(template.K2_PackageType, instruction.PackageCategory);
			AssertEquals(template.K2_DropMode, instruction.KN_DropMode);
			AssertEquals(template.K2_IsContainerRateable, instruction.KN_IsContainerRateable);
			AssertEquals(template.K2_IsLooseRateable, instruction.KN_IsLooseRateable);
		}

		#endregion

		#region TestKM_KT_NKBookingTemplate_CallsRefreshBindingAfterCreationOfInstructions

		public void TestKM_KT_NKBookingTemplate_CallsRefreshBindingAfterCreationOfInstructions()
		{
			var template = CreateTransportTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import, RatingFreightModes.Codes.Containerised);

			var ctoInstructionTemplate = Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CTO, InstructionTypes.Codes.PickUp, true, false);
			var cneInstructionTemplate = Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CNE, InstructionTypes.Codes.Delivery, true, true);
			var cydInstructionTemplate = Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CYD, InstructionTypes.Codes.Delivery, false, true);

			bool instructionsWereCreatedBeforeRefreshBinding = false;
			var consolidation = (DtbTransportConsolidation)Factory.New(ExpectedConsolidationType);
			var transport = (DtbTransport)consolidation.Bookings.AddNew();
			transport.KM_KT_NKBookingTemplateInfo.ValueChanged += (sender, e) => instructionsWereCreatedBeforeRefreshBinding = transport.Instructions.Count == 3;
			transport.KM_KT_NKBookingTemplate = "IFCL";

			AssertEquals("Should have created the Instructions from the new Template before Refresh Binding", true, instructionsWereCreatedBeforeRefreshBinding);
			AssertEquals("IFCL", transport.KM_KT_NKBookingTemplate);
			AssertEquals("FCL Import", transport.KM_Description);
			AssertEquals(Constants.CartageDirection.Import, transport.KM_Direction);
			AssertEquals(RatingFreightModes.Codes.Containerised, transport.KM_RatingFreightMode);
			AssertInstructionAndTemplate(ctoInstructionTemplate, transport.Instructions[0]);
			AssertInstructionAndTemplate(cneInstructionTemplate, transport.Instructions[1]);
			AssertInstructionAndTemplate(cydInstructionTemplate, transport.Instructions[2]);
		}

		#endregion

		#region TestKM_KT_NKBookingTemplate_DoesNotCreateInstructionsIfSemaphoreIsSuspended

		public void TestKM_KT_NKBookingTemplate_DoesNotCreateInstructionsIfSemaphoreIsSuspended()
		{
			var template = CreateTransportTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import, RatingFreightModes.Codes.Containerised);

			var ctoInstructionTemplate = Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CTO, InstructionTypes.Codes.PickUp, true, false);
			var cneInstructionTemplate = Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CNE, InstructionTypes.Codes.Delivery, true, true);
			var cydInstructionTemplate = Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CYD, InstructionTypes.Codes.Delivery, false, true);

			var booking = (DtbTransport)GetNewBusinessObject();

			using (new SemaphoreManager(booking.AddingInstructionsFromTemplateSemaphore))
			{
				booking.KM_KT_NKBookingTemplate = "IFCL";
			}

			AssertEquals("IFCL", booking.KM_KT_NKBookingTemplate);
			AssertEquals("FCL Import", booking.KM_Description);
			AssertEquals(Constants.CartageDirection.Import, booking.KM_Direction);
			AssertEquals(RatingFreightModes.Codes.Containerised, booking.KM_RatingFreightMode);
			AssertEquals(0, booking.Instructions.Count);
		}

		#endregion

		#region TestKM_KT_NKBookingTemplateChange_PopupMessageIfNoPackageCanBeAssignedAfterChange

		public void TestKM_KT_NKBookingTemplateChange_PopupMessageIfNoPackageCanBeAssignedForNewTemplate()
		{
			var consolidation = (DtbTransportConsolidation)Factory.New(ExpectedConsolidationType);
			var transport = (DtbTransport)consolidation.Bookings.AddNew();
			var notify = new TestNotify();
			transport.ConsolidationSingleJob.NotificationManager.Push(notify);
			AssertEquals(0, transport.AssignedPackages.Count);
			AssertNull("No Warning message to be raised.", notify.LastNotification);

			var packageJob = transport.PackageJob;
			var package1 = packageJob.Packages.AddNew();
			package1.KP_PackageID = "PKG00001";
			var package2 = packageJob.Packages.AddNew();
			package2.KP_PackageID = "PKG00002";
			var package3 = packageJob.Packages.AddNew();
			package3.KP_PackageID = "PKG00003";

			var templateLoose = CreateTransportTemplate("ILLL", "LCL Import", Constants.CartageDirection.Import, RatingFreightModes.Codes.Loose);
			Helper.AddInstructionToTemplate(templateLoose, OrganisationTypesList.Codes.CFS, InstructionTypes.Codes.PickUp, false, true, PackageCategories.Codes.Outers);
			Helper.AddInstructionToTemplate(templateLoose, OrganisationTypesList.Codes.CNE, InstructionTypes.Codes.Delivery, false, true, PackageCategories.Codes.Outers);
			transport.KM_KT_NKBookingTemplate = "ILLL";

			AssertEquals(3, transport.AssignedPackages.Count);

			var template = CreateTransportTemplate("IFFF", "FCL Import", Constants.CartageDirection.Import, RatingFreightModes.Codes.Containerised);
			Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CFS, InstructionTypes.Codes.PickUp, false, true, PackageCategories.Codes.Containers);
			Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CNE, InstructionTypes.Codes.Delivery, false, true, PackageCategories.Codes.Containers);
			transport.KM_KT_NKBookingTemplate = "IFFF";

			AssertEquals(0, transport.AssignedPackages.Count);
			AssertNotNull("Warning message should be raised.", transport.ConsolidationSingleJob.NotificationSubscriber);
			Assert(notify.LastNotification.Message.Contains("Could not find Containers/Outer Packages based on Instruction Template Package Type. See Packages Tab."));
		}

		public void TestKM_KT_NKBookingTemplateChange_DoNotPopupMessageIfNoPackageAttachedToConsolidationPackageJob()
		{
			var consolidation = (DtbTransportConsolidation)Factory.New(ExpectedConsolidationType);
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = consolidation.PK;

			var transport = (DtbTransport)consolidation.Bookings.AddNew();
			var notify = new TestNotify();
			transport.ConsolidationSingleJob.NotificationManager.Push(notify);
			AssertEquals(0, transport.AssignedPackages.Count);
			AssertNull("No Warning message to be raised.", notify.LastNotification);

			var template = CreateTransportTemplate("IFFF", "FCL Import", Constants.CartageDirection.Import, RatingFreightModes.Codes.Containerised);
			Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CFS, InstructionTypes.Codes.PickUp, false, true, PackageCategories.Codes.Containers);
			Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CNE, InstructionTypes.Codes.Delivery, false, true, PackageCategories.Codes.Containers);
			transport.KM_KT_NKBookingTemplate = "IFFF";

			AssertEquals(0, transport.AssignedPackages.Count);
			AssertNull("No Warning message to be raised.", notify.LastNotification);

			var templateIFCL = CreateTransportTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import, RatingFreightModes.Codes.Containerised);
			Helper.AddInstructionToTemplate(templateIFCL, OrganisationTypesList.Codes.CTO, InstructionTypes.Codes.PickUp, true, false, PackageCategories.Codes.Containers);
			Helper.AddInstructionToTemplate(templateIFCL, OrganisationTypesList.Codes.CNE, InstructionTypes.Codes.Delivery, true, true, PackageCategories.Codes.Both);
			Helper.AddInstructionToTemplate(templateIFCL, OrganisationTypesList.Codes.CYD, InstructionTypes.Codes.Delivery, false, true, PackageCategories.Codes.Outers);
			transport.KM_KT_NKBookingTemplate = "IFCL";

			AssertEquals(0, transport.AssignedPackages.Count);
			AssertNull("No Warning message to be raised.", notify.LastNotification);
		}

		#endregion

		#region TestKM_KT_NKBookingTemplateChange_OnlyReAssignPreExistingPackages

		public void TestKM_KT_NKBookingTemplateChange_OnlyReAssignPreExistingPackages()
		{
			var consolidation = (DtbTransportConsolidation)Factory.New(ExpectedConsolidationType);
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = consolidation.PK;
			var package1 = packageJob.Packages.AddNew();
			package1.KP_PackageID = "PKG00001";
			var package2 = packageJob.Packages.AddNew();
			package2.KP_PackageID = "PKG00002";
			var package3 = packageJob.Packages.AddNew();
			package3.KP_PackageID = "PKG00003";

			var transport = (DtbTransport)consolidation.Bookings.AddNew();
			AssertEquals(0, transport.AssignedPackages.Count);

			var picInstruction = Helper.CreateInstruction(transport, InstructionTypes.Codes.PickUp);
			var dlvInstruction = Helper.CreateInstruction(transport, InstructionTypes.Codes.Delivery);
			Helper.CreatePackageDivot(picInstruction, package1, 1);
			Helper.CreatePackageDivot(dlvInstruction, package1, 1);
			AssertEquals(1, transport.AssignedPackages.Count);

			Helper.CreatePackageDivot(picInstruction, package3, 1);
			Helper.CreatePackageDivot(dlvInstruction, package3, 1);
			AssertEquals(2, transport.AssignedPackages.Count);

			var template = CreateTransportTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import, RatingFreightModes.Codes.Containerised);
			Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CTO, InstructionTypes.Codes.PickUp, true, false, PackageCategories.Codes.Containers);
			Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CNE, InstructionTypes.Codes.Delivery, true, true, PackageCategories.Codes.Both);
			Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CYD, InstructionTypes.Codes.Delivery, false, true, PackageCategories.Codes.Outers);
			transport.KM_KT_NKBookingTemplate = "IFCL";

			AssertEquals(2, transport.AssignedPackages.Count);
			AssertContainsExactElementsInAnyOrder(new PkgPackage[] { package1, package3 }, transport.AssignedPackages);
		}

		public void TestKM_KT_NKBookingTemplateChange_OnlyReAssignPreExistingPackages_ContainersToLoose()
		{
			var consolidation = (DtbTransportConsolidation)Factory.New(ExpectedConsolidationType);
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = consolidation.PK;

			var container1 = packageJob.Packages.AddNew(Constants.PkgUnit.Container, "CONT0001");
			var container2 = packageJob.Packages.AddNew(Constants.PkgUnit.Container, "CONT0002");

			var package1 = container1.Packages.AddNew();
			var package2 = container2.Packages.AddNew();
			package1.KP_PackageID = "PKG00002";
			package2.KP_PackageID = "PKG00003";

			var transport = (DtbTransport)consolidation.Bookings.AddNew();
			AssertEquals(0, transport.AssignedPackages.Count);

			var picInstruction = Helper.CreateInstruction(transport, InstructionTypes.Codes.PickUp);
			var dlvInstruction = Helper.CreateInstruction(transport, InstructionTypes.Codes.Delivery);
			Helper.CreatePackageDivot(picInstruction, container1, 1);
			Helper.CreatePackageDivot(dlvInstruction, container1, 1);
			AssertEquals(1, transport.AssignedPackages.Count);

			var template = CreateTransportTemplate("ILLL", "LCL Import", Constants.CartageDirection.Import, RatingFreightModes.Codes.Loose);
			Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CFS, InstructionTypes.Codes.PickUp, false, true, PackageCategories.Codes.Loose);
			Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CNE, InstructionTypes.Codes.Delivery, false, true, PackageCategories.Codes.Loose);
			transport.KM_KT_NKBookingTemplate = "ILLL";

			AssertEquals(1, transport.AssignedPackages.Count);
			AssertContainsExactElementsInAnyOrder(new PkgPackage[] { package1 }, transport.AssignedPackages);
		}

		public void TestKM_KT_NKBookingTemplateChange_OnlyReAssignPreExistingPackages_LooseToContainer()
		{
			var consolidation = (DtbTransportConsolidation)Factory.New(ExpectedConsolidationType);
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = consolidation.PK;

			var container1 = packageJob.Packages.AddNew(Constants.PkgUnit.Container, "CONT0001");
			var container2 = packageJob.Packages.AddNew(Constants.PkgUnit.Container, "CONT0002");

			var package1 = container1.Packages.AddNew();
			var package2 = container2.Packages.AddNew();
			package1.KP_PackageID = "PKG00002";
			package2.KP_PackageID = "PKG00003";

			var transport = (DtbTransport)consolidation.Bookings.AddNew();
			AssertEquals(0, transport.AssignedPackages.Count);

			var picInstruction = Helper.CreateInstruction(transport, InstructionTypes.Codes.PickUp);
			var dlvInstruction = Helper.CreateInstruction(transport, InstructionTypes.Codes.Delivery);
			Helper.CreatePackageDivot(picInstruction, package1, 1);
			Helper.CreatePackageDivot(dlvInstruction, package1, 1);
			AssertEquals(1, transport.AssignedPackages.Count);

			var template = CreateTransportTemplate("IFFF", "FCL Import", Constants.CartageDirection.Import, RatingFreightModes.Codes.Containerised);
			Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CFS, InstructionTypes.Codes.PickUp, false, true, PackageCategories.Codes.Containers);
			Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CNE, InstructionTypes.Codes.Delivery, false, true, PackageCategories.Codes.Containers);
			transport.KM_KT_NKBookingTemplate = "IFFF";

			AssertEquals(1, transport.AssignedPackages.Count);
			AssertContainsExactElementsInAnyOrder(new PkgPackage[] { container1 }, transport.AssignedPackages);
		}

		#endregion

		// calculated

		#region TestStatusDescription

		public void TestStatusDescription()
		{
			var transportJob = GetSaveableTransportJob();
			AssertEquals(TransportStatuses.Descriptions.Available, transportJob.StatusDescription);

			transportJob.KM_Status = TransportStatuses.Codes.Held;
			AssertEquals(TransportStatuses.Descriptions.Held, transportJob.StatusDescription);

			transportJob.KM_Status = "xXx";
			AssertEquals("", transportJob.StatusDescription);
		}

		#endregion

		#region TransportBookingPartyReference

		public void TransportBookingPartyReference()
		{
			var transportJob = GetSaveableTransportJob();
			AssertEquals("", transportJob.TransportBookingPartyReference);

			var reference = transportJob.AdditionalReferenceNumbers.AddNew();
			reference.CE_EntryNum = "ABC";
			AssertEquals("", transportJob.TransportBookingPartyReference);

			reference.CE_EntryType = TransportCommonAdditionalReferenceTypes.Codes.ExternalTransportBookingNumber;
			AssertEquals("Use Additional Reference over own TB, we send events directly to sending tb.", "ABC", transportJob.TransportBookingPartyReference);
		}

		#endregion

		#region TestBookedByOrganisationPK

		public void TestBookedByOrganisationPK()
		{
			var booking = GetSaveableTransportJob();
			AssertEquals(ZGuid.Empty, booking.BookedByOrganisationPK);

			var org = Factory.New<OrgHeader>();
			booking.ConsolidationSingleJob.BookedByAddress.OrganisationPK = org.PK;
			AssertEquals(org.PK, booking.BookedByOrganisationPK);
		}

		#endregion

		#region TestLocalClient

		public void TestLocalClient()
		{
			var booking = GetSaveableTransportJob();
			AssertEquals("Neither Billing Client nor ClientReqBillToParty exist, Local Client should be empty.", true, booking.LocalClient.IsEmpty);

			var bOrg = Factory.New<OrgHeader>();
			booking.ConsolidationSingleJob.BookedByAddress.OrganisationPK = bOrg.PK;
			Assert(booking.LocalClient.IsEmpty);

			var clientReqBillToPartyOrg = Helper.CreateOrganisation("CRBP");
			var billingParty = booking.BillingPartyAddress; // Poke it to create CRB
			var clientReqBillToPartyAddress = booking.DocAddresses.FindByDocAddressType(DocAddressType.ClientRequestedBillingParty);
			AssertEquals(false, clientReqBillToPartyAddress.Requirement.CanOverride);
			clientReqBillToPartyAddress.E2_OA_Address = clientReqBillToPartyOrg.MainAddress.PK;
			AssertEquals("No Billing Client exists, Local Client should fallback to the ClientReqBillToParty Org.", clientReqBillToPartyOrg.PK, booking.LocalClient);

			var billingOrg = Helper.CreateOrganisation("BILLC");
			var job = new JobHeader.Loader(booking).TryCreate();
			job.LocalChargesAddr.OA_OH = billingOrg.PK;
			AssertEquals("Local Client should come from the Billing Job.", billingOrg.PK, booking.LocalClient);
		}

		#endregion

		#region TestBillingPartyOrLocalClientPK

		public void TestBillingPartyOrLocalClientPK()
		{
			var org1 = Helper.CreateOrganisation("O1");
			var org2 = Helper.CreateOrganisation("O2");
			var transport = GetSaveableTransportJob();
			AssertEquals("Precondition", ZGuid.Empty, transport.BillingPartyAddress.E2_OA_Address);
			AssertEquals(ZGuid.Empty, transport.BillingPartyOrLocalClientPK);

			transport.BillingPartyOrLocalClientPK = org1.MainAddress.PK;
			AssertEquals(org1.MainAddress.PK, transport.BillingPartyAddress.E2_OA_Address);
			AssertEquals(org1.MainAddress.PK, transport.BillingPartyOrLocalClientPK);

			transport.BillingPartyOrLocalClientPK = org2.MainAddress.PK;
			AssertEquals(org2.MainAddress.PK, transport.BillingPartyAddress.E2_OA_Address);
			AssertEquals(org2.MainAddress.PK, transport.BillingPartyOrLocalClientPK);

			new JobHeader.Loader(transport).TryCreate();
			AssertEquals(org2.MainAddress.PK, transport.Job.JH_OA_LocalChargesAddr);
			AssertEquals(org2.MainAddress.PK, transport.BillingPartyOrLocalClientPK);
			AssertEquals(org2.MainAddress.PK, transport.BillingPartyAddress.E2_OA_Address);

			transport.BillingPartyOrLocalClientPK = org1.MainAddress.PK;
			AssertEquals(org1.MainAddress.PK, transport.Job.JH_OA_LocalChargesAddr);
			AssertEquals(org1.MainAddress.PK, transport.BillingPartyOrLocalClientPK);
			AssertEquals(org2.MainAddress.PK, transport.BillingPartyAddress.E2_OA_Address);

			transport.BillingPartyOrLocalClientPK = org2.MainAddress.PK;
			AssertEquals(org2.MainAddress.PK, transport.Job.JH_OA_LocalChargesAddr);
			AssertEquals(org2.MainAddress.PK, transport.BillingPartyOrLocalClientPK);
			AssertEquals(org2.MainAddress.PK, transport.BillingPartyAddress.E2_OA_Address);
		}

		#endregion

		#region TestBillingPartyOrLocalClientAddress

		public void TestBillingPartyOrLocalClientAddress()
		{
			var org1 = Helper.CreateOrganisation("O1");
			var transport = GetSaveableTransportJob();
			AssertEquals("Precondition", ZGuid.Empty, transport.BillingPartyAddress.E2_OA_Address);
			AssertEquals(ZGuid.Empty, transport.BillingPartyOrLocalClientPK);

			// Job is null
			transport.BillingPartyOrLocalClientPK = org1.MainAddress.PK;
			AssertEquals("Since job is null, BillingPartyOrLocalClientAddress should be equal to BillingPartyAddress.Address", transport.BillingPartyAddress.Address, transport.BillingPartyOrLocalClientAddress);

			// Create job
			var job = new JobHeader.Loader(transport).TryCreate();
			job.JH_OA_LocalChargesAddr = org1.MainAddress.PK;
			AssertEquals("Since job is not null, BillingPartyOrLocalClientAddress should be equal to job.LocalChargesAddr", job.LocalChargesAddr, transport.BillingPartyOrLocalClientAddress);
		}

		#endregion

		#region TestBillingPartyOrLocalClientPK_ZAddress

		public void TestBillingPartyOrLocalClientPK_ZAddress()
		{
			var transport = GetSaveableTransportJob();
			var zAddressForTransportWithoutJobHeader = transport.BillingPartyOrLocalClientPK_ZAddress;
			Assert(zAddressForTransportWithoutJobHeader.IsOrgVisible);
			AssertEquals(AddressType.ARM, zAddressForTransportWithoutJobHeader.DefaultAddressType);
		}

		#endregion

		#region TestBillingPartyOrLocalClientPKInfo

		public void TestBillingPartyOrLocalClientPKInfo()
		{
			var transport = GetSaveableTransportJob();
			AssertEquals(transport.BillingPartyAddress.E2_OA_AddressInfo, ((ZWrappedPropertyInfo)transport.BillingPartyOrLocalClientPKInfo).InnerInfo);

			var job = new JobHeader.Loader(transport).TryCreate();
			AssertEquals(transport.Job.JH_OA_LocalChargesAddrInfo, ((ZWrappedPropertyInfo)transport.BillingPartyOrLocalClientPKInfo).InnerInfo);
		}

		#endregion

		// rating

		#region TestGetTotalLoosePackageQuantity

		public void TestGetTotalLoosePackageQuantity()
		{
			var transportJob = GetSaveableTransportJob();
			AssertEquals(0, transportJob.GetTotalLoosePackageQuantity());

			var frmInstruction = Helper.CreateInstruction(transportJob, InstructionTypes.Codes.PickUp);
			var dlvInstruction = Helper.CreateInstruction(transportJob, InstructionTypes.Codes.Delivery);

			var package10 = Helper.CreatePackage("", null, 10, 11, 12);
			var package20 = Helper.CreatePackage("", null, 20, 21, 22);
			var package30 = Helper.CreatePackage("", null, 30, 31, 32);

			Helper.CreatePackageDivot(frmInstruction, package10, 10);
			Helper.CreatePackageDivot(frmInstruction, package10, 10);
			Helper.CreatePackageDivot(dlvInstruction, package20, 20);
			Helper.CreatePackageDivot(dlvInstruction, package20, 20);

			AssertEquals(30, transportJob.GetTotalLoosePackageQuantity());

			var cont1 = Helper.CreatePackageContainer("CONT1", null, 1, 40);
			Helper.CreatePackageDivot(frmInstruction, cont1, 1);
			AssertEquals(30, transportJob.GetTotalLoosePackageQuantity());
		}

		#endregion

		#region TestGetTotalLoosePackageUnit

		public void TestGetTotalLoosePackageUnit()
		{
			var transportJob = GetSaveableTransportJob();
			AssertEquals("", transportJob.GetTotalLoosePackageUnit());

			var frmInstruction = Helper.CreateInstruction(transportJob, InstructionTypes.Codes.PickUp);
			var dlvInstruction = Helper.CreateInstruction(transportJob, InstructionTypes.Codes.Delivery);

			var package1 = Helper.CreatePackage("", 1, Constants.PkgUnit.Keg);
			var package2 = Helper.CreatePackage("", 1, Constants.PkgUnit.Keg);

			Helper.CreatePackageDivot(frmInstruction, package1, 1);
			Helper.CreatePackageDivot(frmInstruction, package1, 1);
			Helper.CreatePackageDivot(dlvInstruction, package2, 1);
			Helper.CreatePackageDivot(dlvInstruction, package2, 1);

			AssertEquals(Constants.PkgUnit.Keg, transportJob.GetTotalLoosePackageUnit());

			var package30 = Helper.CreatePackage("", 1, Constants.PkgUnit.Pallet);
			Helper.CreatePackageDivot(frmInstruction, package30, 1);
			AssertEquals(Constants.PkgUnit.Package, transportJob.GetTotalLoosePackageUnit());
		}

		#endregion

		#region TestGetTotalLoosePackageWeight

		public void TestGetTotalLoosePackageWeight()
		{
			var transportJob = GetSaveableTransportJob();
			AssertEquals(0m, transportJob.GetTotalLoosePackageWeight().Amount);
			AssertEquals(Constants.Weight.Kilograms, transportJob.GetTotalLoosePackageWeight().Unit);

			var frmInstruction = Helper.CreateInstruction(transportJob, InstructionTypes.Codes.PickUp);
			var dlvInstruction = Helper.CreateInstruction(transportJob, InstructionTypes.Codes.Delivery);

			var package10 = Helper.CreatePackage("", null, 10, 11, 12);
			var package20 = Helper.CreatePackage("", null, 20, 21, 22);
			var package30 = Helper.CreatePackage("", null, 30, 31, 32);

			package10.KP_WeightUQ = Constants.Weight.Kilograms;
			package20.KP_WeightUQ = Constants.Weight.Pounds;

			Helper.CreatePackageDivot(frmInstruction, package10, 10);
			Helper.CreatePackageDivot(frmInstruction, package20, 20);
			Helper.CreatePackageDivot(dlvInstruction, package10, 10);
			Helper.CreatePackageDivot(dlvInstruction, package20, 20);

			AssertEquals(20.52544m, transportJob.GetTotalLoosePackageWeight().Amount);
			AssertEquals(Constants.Weight.Kilograms, transportJob.GetTotalLoosePackageWeight().Unit);

			var cont1 = Helper.CreatePackageContainer("CONT1", null, 1, 40);
			Helper.CreatePackageDivot(frmInstruction, cont1, 1);
			AssertEquals(20.52544m, transportJob.GetTotalLoosePackageWeight().Amount);
			AssertEquals(Constants.Weight.Kilograms, transportJob.GetTotalLoosePackageWeight().Unit);
		}

		#endregion

		#region TestGetTotalLoosePackageWeightWithInvalidUnits

		public void TestGetTotalLoosePackageWeightWithInvalidUnits()
		{
			var transportJob = GetSaveableTransportJob();
			AssertEquals(0m, transportJob.GetTotalLoosePackageWeight().Amount);
			AssertEquals(Constants.Weight.Kilograms, transportJob.GetTotalLoosePackageWeight().Unit);

			CreatePackageDivots(transportJob, false);

			AssertNoExceptionThrown("All weights used in calculation of TotalLoosePackageWeight should be valid", () => { var weight = transportJob.GetTotalLoosePackageWeight(); });
			AssertEquals(new ZWeight(50, "KG"), transportJob.GetTotalLoosePackageWeight());
		}

		#endregion

		#region TestGetTotalLoosePackageVolume

		public void TestGetTotalLoosePackageVolume()
		{
			var transportJob = GetSaveableTransportJob();
			AssertEquals(0m, transportJob.GetTotalLoosePackageVolume().Amount);
			AssertEquals(Constants.Volume.CubicMetres, transportJob.GetTotalLoosePackageVolume().Unit);

			var frmInstruction = Helper.CreateInstruction(transportJob, InstructionTypes.Codes.PickUp);
			var dlvInstruction = Helper.CreateInstruction(transportJob, InstructionTypes.Codes.Delivery);

			var package10 = Helper.CreatePackage("", null, 10, 11, 12);
			var package20 = Helper.CreatePackage("", null, 20, 21, 22);
			var package30 = Helper.CreatePackage("", null, 30, 31, 32);

			package10.KP_VolumeUQ = Constants.Volume.CubicMetres;
			package20.KP_VolumeUQ = Constants.Volume.CubicFeet;

			Helper.CreatePackageDivot(frmInstruction, package10, 10);
			Helper.CreatePackageDivot(frmInstruction, package10, 10);
			Helper.CreatePackageDivot(dlvInstruction, package20, 20);
			Helper.CreatePackageDivot(dlvInstruction, package20, 20);

			AssertEquals(12.622971m, transportJob.GetTotalLoosePackageVolume().Amount);
			AssertEquals(Constants.Volume.CubicMetres, transportJob.GetTotalLoosePackageVolume().Unit);

			var cont1 = Helper.CreatePackageContainer("CONT1", null, 1, 40);
			Helper.CreatePackageDivot(frmInstruction, cont1, 1);
			AssertEquals(12.622971m, transportJob.GetTotalLoosePackageVolume().Amount);
			AssertEquals(Constants.Volume.CubicMetres, transportJob.GetTotalLoosePackageVolume().Unit);
		}

		#endregion

		#region TestGetTotalLoosePackageVolumeWithInvalidUnits

		public void TestGetTotalLoosePackageVolumeWithInvalidUnits()
		{
			var transportJob = GetSaveableTransportJob();
			AssertEquals(0m, transportJob.GetTotalLoosePackageVolume().Amount);
			AssertEquals(Constants.Volume.CubicMetres, transportJob.GetTotalLoosePackageVolume().Unit);

			CreatePackageDivots(transportJob, false);

			AssertNoExceptionThrown("All volumes used in calculation of TotalLoosePackageVolume should be valid", () => transportJob.GetTotalLoosePackageVolume());
			AssertEquals(new ZVolume(100, "M3"), transportJob.GetTotalLoosePackageVolume());
		}

		#endregion

		#region TestGetTotalContainerisedWeight

		public void TestGetTotalContainerisedWeight()
		{
			var transportJob = GetSaveableTransportJob();
			var frmInstruction = Helper.CreateInstruction(transportJob, InstructionTypes.Codes.PickUp);
			var dlvInstruction = Helper.CreateInstruction(transportJob, InstructionTypes.Codes.Delivery);
			var cont1 = Helper.CreatePackageContainer("CONT1", null, 1, 40);
			var cont2 = Helper.CreatePackageContainer("CONT2", null, 1, 50, 20, "40GP");

			Helper.CreatePackageDivot(frmInstruction, cont1, 1);
			Helper.CreatePackageDivot(frmInstruction, cont2, 1);
			Helper.CreatePackageDivot(dlvInstruction, cont1, 1);
			Helper.CreatePackageDivot(dlvInstruction, cont2, 1);

			AssertEquals(90m, transportJob.GetTotalContainerisedWeight().Amount);
			AssertEquals(Constants.Weight.Kilograms, transportJob.GetTotalContainerisedWeight().Unit);
		}

		#endregion

		#region TestGetTotalContainerisedWeightWithInvalidUnits

		public void TestGetTotalContainerisedWeightWithInvalidUnits()
		{
			var transportJob = GetSaveableTransportJob();
			AssertEquals(0m, transportJob.GetTotalContainerisedWeight().Amount);
			AssertEquals(Constants.Weight.Kilograms, transportJob.GetTotalContainerisedWeight().Unit);

			CreatePackageDivots(transportJob, true);

			AssertNoExceptionThrown("All weights used in calculation of TotalContainerisedWeight should be valid", () => transportJob.GetTotalContainerisedWeight());
			AssertEquals(new ZWeight(50, "KG"), transportJob.GetTotalContainerisedWeight());
		}

		#endregion

		#region TestGetTotalContainerisedVolume

		public void TestGetTotalContainerisedVolume()
		{
			var transportJob = GetSaveableTransportJob();
			var frmInstruction = Helper.CreateInstruction(transportJob, InstructionTypes.Codes.PickUp);
			var dlvInstruction = Helper.CreateInstruction(transportJob, InstructionTypes.Codes.Delivery);
			var cont1 = Helper.CreatePackageContainer("CONT1", null, 1, 40);
			var cont2 = Helper.CreatePackageContainer("CONT2", null, 1, 50, 20, "40GP");

			Helper.CreatePackageDivot(frmInstruction, cont1, 1);
			Helper.CreatePackageDivot(frmInstruction, cont2, 1);
			Helper.CreatePackageDivot(dlvInstruction, cont1, 1);
			Helper.CreatePackageDivot(dlvInstruction, cont2, 1);

			AssertEquals(20m, transportJob.GetTotalContainerisedVolume().Amount);
			AssertEquals(Constants.Volume.CubicMetres, transportJob.GetTotalContainerisedVolume().Unit);
		}

		#endregion

		#region TestGetTotalContainerisedVolumeWithInvalidUnits

		public void TestGetTotalContainerisedVolumeWithInvalidUnits()
		{
			var transportJob = GetSaveableTransportJob();
			AssertEquals(0m, transportJob.GetTotalContainerisedVolume().Amount);
			AssertEquals(Constants.Volume.CubicMetres, transportJob.GetTotalContainerisedVolume().Unit);

			CreatePackageDivots(transportJob, true);

			AssertNoExceptionThrown("All volumes used in calculation of TotalContainerisedVolume should be valid", () => transportJob.GetTotalContainerisedVolume());
			AssertEquals(new ZVolume(100, "M3"), transportJob.GetTotalContainerisedVolume());
		}

		#endregion

		void CreatePackageDivots(DtbTransport transportJob, bool containerised)
		{
			var frmInstruction = Helper.CreateInstruction(transportJob, InstructionTypes.Codes.PickUp);
			var dlvInstruction = Helper.CreateInstruction(transportJob, InstructionTypes.Codes.Delivery);

			List<PkgPackage> packages = CreatePackagesWithInvalid(containerised);

			foreach (PkgPackage p in packages)
			{
				Helper.CreatePackageDivot(frmInstruction, p, 1);
				Helper.CreatePackageDivot(dlvInstruction, p, 1);
			}
		}

		List<PkgPackage> CreatePackagesWithInvalid(bool containerised)
		{
			var packages = new List<PkgPackage>();
			if (containerised)
			{
				packages.Add(MakeInvalid(Helper.CreatePackageContainer("ContainerPackage1 (Invalid)", null, 1, 100, 200)));
				packages.Add(Helper.CreatePackageContainer("ContainerPackage2", null, 1, 50, 100));
			}
			else
			{
				packages.Add(MakeInvalid(Helper.CreatePackage("Package1 (Invalid)", null, 1, 100, 200)));
				packages.Add(Helper.CreatePackage("Package2", null, 1, 50, 100));
			}
			return packages;
		}

		PkgPackage MakeInvalid(PkgPackage package)
		{
			package.KP_VolumeUQ = Constants.Weight.MetricCarat;  //'MC'
			package.KP_WeightUQ = Constants.Volume.CubicMetres;  //'M3'

			return package;
		}

		#endregion

		#region Flags

		#region TestIsAutoLogged

		public void TestIsAutoLogged()
		{
			AssertEquals(true, ((IAutoAdminLogTarget)Factory.New(TestedTypeHelper.GetTestedType(GetType()))).IsAutoAdminBusinessObjectLoggerEnabled);
		}

		#endregion

		// status

		#region TestIsDelivered

		public void TestIsDelivered()
		{
			AssertFlag("IsDelivered", DtbBookingSchema.KM_Status, TransportStatuses.Codes.Delivered, TransportStatuses.Codes.Available);
		}

		#endregion

		// direction

		#region TestIsPickupDirection

		public void TestIsPickupDirection()
		{
			AssertFlag("IsPickupDirection", DtbBookingSchema.KM_Direction, Constants.CartageDirection.Origin, Constants.CartageDirection.Destination);
			AssertFlag("IsPickupDirection", DtbBookingSchema.KM_Direction, Constants.CartageDirection.Export, Constants.CartageDirection.Import);
		}

		#endregion

		#region TestIsDeliveryDirection

		public void TestIsDeliveryDirection()
		{
			AssertFlag("IsDeliveryDirection", DtbBookingSchema.KM_Direction, Constants.CartageDirection.Destination, Constants.CartageDirection.Origin);
			AssertFlag("IsDeliveryDirection", DtbBookingSchema.KM_Direction, Constants.CartageDirection.Import, Constants.CartageDirection.Export);
		}

		#endregion

		#region TestIsLocalDirection

		public void TestIsLocalDirection()
		{
			AssertFlag("IsLocalDirection", DtbBookingSchema.KM_Direction, Constants.CartageDirection.Local, Constants.CartageDirection.LineHaul);
		}

		#endregion

		#region TestIsAnyPackageHazardous

		public void TestIsAnyPackageHazardous()
		{
			var transport = GetSaveableTransportJob();
			AssertEquals(false, transport.IsAnyPackageHazardous);

			var packageJob = transport.PackageJob;
			var container = Helper.CreatePackage("CONT", 1, "CNT");
			packageJob.Packages.Add(container);
			var containerInnerPackage = Helper.CreatePackage("CONT_IN", 1);
			container.Packages.Add(containerInnerPackage);
			AssertEquals(false, transport.IsAnyPackageHazardous);

			containerInnerPackage.UNDGs.AddNew();
			AssertEquals("Package is not assigned yet.", false, transport.IsAnyPackageHazardous);

			var picInstruction = Helper.CreateInstruction(transport, InstructionTypes.Codes.PickUp);
			var dlvInstruction = Helper.CreateInstruction(transport, InstructionTypes.Codes.Delivery);
			Helper.CreatePackageDivot(picInstruction, container, 1);
			Helper.CreatePackageDivot(dlvInstruction, container, 1);
			AssertEquals("Assigned Package.", 1, transport.AssignedPackages.Count);

			AssertEquals(true, transport.IsAnyPackageHazardous);
		}

		#endregion

		#region TestIsAnyPackageRequiresRefrigeration

		public void TestIsAnyPackageRequiresRefrigeration()
		{
			var transport = GetSaveableTransportJob();
			AssertEquals("Precondition: RequiresRefridgeration should be false", false, transport.IsAnyPackageRequiresRefridgeration);

			var packageJob = transport.PackageJob;
			var container = Helper.CreatePackage("CONT", 1, "CNT");
			packageJob.Packages.Add(container);
			var containerInnerPackage = Helper.CreatePackage("CONT_IN", 1);
			container.Packages.Add(containerInnerPackage);
			AssertEquals("RequiresRefridgeration still false.", false, transport.IsAnyPackageRequiresRefridgeration);

			containerInnerPackage.KP_RequiresTemperatureControl = true;
			AssertEquals("Package is not assigned yet, RequiresRefridgeration still false", false, transport.IsAnyPackageRequiresRefridgeration);

			var picInstruction = Helper.CreateInstruction(transport, InstructionTypes.Codes.PickUp);
			var dlvInstruction = Helper.CreateInstruction(transport, InstructionTypes.Codes.Delivery);
			Helper.CreatePackageDivot(picInstruction, container, 1);
			Helper.CreatePackageDivot(dlvInstruction, container, 1);
			AssertEquals("Assigned Package.", 1, transport.AssignedPackages.Count);

			AssertEquals("Package now assigned, RequiresRefridgeration should be true.", true, transport.IsAnyPackageRequiresRefridgeration);
		}

		#endregion

		#region AssertFlag

		protected void AssertFlag(ZString flagPropertyName, SchemaColumn codeColumn, ZString validCode, ZString invalidCode)
		{
			var booking = (DtbTransport)GetNewBusinessObject();

			booking[codeColumn.Name] = "";
			AssertEquals(false, booking[flagPropertyName]);

			booking[codeColumn.Name] = validCode;
			AssertEquals(true, booking[flagPropertyName]);

			booking[codeColumn.Name] = invalidCode;
			AssertEquals(false, booking[flagPropertyName]);
		}

		#endregion

		// rating

		#region TestIsContainerisedOnly

		public void TestIsContainerisedOnly()
		{
			var transportJob = GetSaveableTransportJob();
			AssertEquals(false, transportJob.IsContainerisedOnly);

			var container = Factory.New<PkgPackage>();
			container.KP_F3_NKPackType = Constants.PkgUnit.Container;
			var instruction = Helper.CreateInstruction(transportJob);
			Helper.CreatePackageDivot(instruction, container, 1);
			AssertEquals(true, transportJob.IsContainerisedOnly);

			var package = Factory.New<PkgPackage>();
			Helper.CreatePackageDivot(instruction, package, 1);
			AssertEquals(false, transportJob.IsContainerisedOnly);
		}

		#endregion

		#region TestIsLooseOnly

		public void TestIsLooseOnly()
		{
			var transportJob = GetSaveableTransportJob();
			AssertEquals(false, transportJob.IsLooseOnly);

			var package = Factory.New<PkgPackage>();
			var instruction = Helper.CreateInstruction(transportJob);
			Helper.CreatePackageDivot(instruction, package, 1);
			AssertEquals(true, transportJob.IsLooseOnly);

			var container = Factory.New<PkgPackage>();
			container.KP_F3_NKPackType = Constants.PkgUnit.Container;
			Helper.CreatePackageDivot(instruction, container, 1);
			AssertEquals(false, transportJob.IsLooseOnly);
		}

		#endregion

		#region TestIsContainerised

		public void TestIsContainerised()
		{
			var transportJob = GetSaveableTransportJob();
			AssertEquals(false, transportJob.IsContainerised);

			var container = Factory.New<PkgPackage>();
			container.KP_F3_NKPackType = Constants.PkgUnit.Container;
			var instruction = Helper.CreateInstruction(transportJob);
			Helper.CreatePackageDivot(instruction, container, 1);
			AssertEquals(true, transportJob.IsContainerised);

			var package = Factory.New<PkgPackage>();
			Helper.CreatePackageDivot(instruction, container, 1);
			AssertEquals(true, transportJob.IsContainerised);
		}

		#endregion

		#region TestIsLoose

		public void TestIsLoose()
		{
			var transportJob = GetSaveableTransportJob();
			AssertEquals(false, transportJob.IsLoose);

			var package = Factory.New<PkgPackage>();
			var instruction = Helper.CreateInstruction(transportJob);
			Helper.CreatePackageDivot(instruction, package, 1);
			AssertEquals(true, transportJob.IsLoose);

			var container = Factory.New<PkgPackage>();
			container.KP_F3_NKPackType = Constants.PkgUnit.Container;
			Helper.CreatePackageDivot(instruction, container, 1);
			AssertEquals(true, transportJob.IsLoose);
		}

		#endregion

		#region TestIsFTL

		public void TestIsFTL()
		{
			var transportJob = GetSaveableTransportJob();
			AssertEquals(false, transportJob.IsFTL);

			var container = Factory.New<PkgPackage>();
			container.KP_F3_NKPackType = Constants.PkgUnit.Container;
			var instruction = Helper.CreateInstruction(transportJob);
			Helper.CreatePackageDivot(instruction, container, 1);
			AssertEquals(false, transportJob.IsFTL);

			container.Container.K0_ContainerMode = Constants.ContainerModes.FTL;
			AssertEquals("Has Container marked as FTL.", true, transportJob.IsFTL);

			container.Container.K0_ContainerMode = "";
			AssertEquals(false, transportJob.IsFTL);

			var roadContainerType = Factory.New<RefContainer>();
			roadContainerType.RC_ShippingMode = RefContainerLookups.ShippingModes.Road;
			var seaContainerType = Factory.New<RefContainer>();
			seaContainerType.RC_ShippingMode = RefContainerLookups.ShippingModes.Sea;

			container.Container.K0_RC_ContainerType = seaContainerType.PK;
			AssertEquals(false, transportJob.IsFTL);

			container.Container.K0_RC_ContainerType = roadContainerType.PK;
			AssertEquals("Has container type that is of type Road.", true, transportJob.IsFTL);
		}

		#endregion

		#region TestIsFCL

		public void TestIsFCL()
		{
			var transportJob = GetSaveableTransportJob();
			AssertEquals(false, transportJob.IsFCL);

			var container = Factory.New<PkgPackage>();
			container.KP_F3_NKPackType = Constants.PkgUnit.Container;
			var instruction = Helper.CreateInstruction(transportJob);
			Helper.CreatePackageDivot(instruction, container, 1);
			AssertEquals(true, transportJob.IsFCL);

			container.Container.K0_ContainerMode = Constants.ContainerModes.FTL;
			AssertEquals("Has Container marked as FTL so is not FCL", false, transportJob.IsFCL);

			container.Container.K0_ContainerMode = "";
			AssertEquals(true, transportJob.IsFCL);

			var roadContainerType = Factory.New<RefContainer>();
			roadContainerType.RC_ShippingMode = RefContainerLookups.ShippingModes.Road;
			var seaContainerType = Factory.New<RefContainer>();
			seaContainerType.RC_ShippingMode = RefContainerLookups.ShippingModes.Sea;

			container.Container.K0_RC_ContainerType = seaContainerType.PK;
			AssertEquals(true, transportJob.IsFCL);

			container.Container.K0_RC_ContainerType = roadContainerType.PK;
			AssertEquals("Has container type that is of type Road so is not FCL", false, transportJob.IsFCL);
		}

		#endregion

		#region TestIsPort

		public void TestIsPort()
		{
			var transportJob = GetSaveableTransportJob();
			AssertEquals(false, transportJob.IsPort);

			var cto = (DtbTransportInstruction)transportJob.Instructions.AddNew();
			cto.OrganisationType = "CTO";
			AssertEquals(true, transportJob.IsPort);

			cto.OrganisationType = "CNR";
			AssertEquals(false, transportJob.IsPort);

			cto.OrganisationType = "CNE";
			AssertEquals(false, transportJob.IsPort);

			cto.OrganisationType = "CFS";
			AssertEquals(false, transportJob.IsPort);

			cto.OrganisationType = "CYD";
			AssertEquals(true, transportJob.IsPort);

			cto.OrganisationType = "WHS";
			AssertEquals(false, transportJob.IsPort);
		}

		#endregion

		#region TestIsFCLPort

		public void TestIsFCLPort()
		{
			var transportJob = GetSaveableTransportJob();
			AssertEquals(false, transportJob.IsFCLPort);

			var container = Factory.New<PkgPackage>();
			container.KP_F3_NKPackType = Constants.PkgUnit.Container;
			var instruction = Helper.CreateInstruction(transportJob);
			Helper.CreatePackageDivot(instruction, container, 1);
			AssertEquals(false, transportJob.IsFCLPort);

			var cto = (DtbTransportInstruction)transportJob.Instructions.AddNew();
			cto.OrganisationType = "CTO";
			AssertEquals(true, transportJob.IsFCLPort);

			container.Container.K0_ContainerMode = Constants.ContainerModes.FTL;
			AssertEquals("Has Container marked as FTL so is not FCL", false, transportJob.IsFCLPort);

			container.Container.K0_ContainerMode = "";
			AssertEquals(true, transportJob.IsFCLPort);

			var roadContainerType = Factory.New<RefContainer>();
			roadContainerType.RC_ShippingMode = RefContainerLookups.ShippingModes.Road;
			var seaContainerType = Factory.New<RefContainer>();
			seaContainerType.RC_ShippingMode = RefContainerLookups.ShippingModes.Sea;

			container.Container.K0_RC_ContainerType = seaContainerType.PK;
			AssertEquals(true, transportJob.IsFCLPort);

			container.Container.K0_RC_ContainerType = roadContainerType.PK;
			AssertEquals("Has container type that is of type Road so is not FCL", false, transportJob.IsFCLPort);

			container.Container.K0_RC_ContainerType = seaContainerType.PK;
			cto.OrganisationType = "CFS";
			AssertEquals(false, transportJob.IsFCLPort);

			cto.OrganisationType = "CYD";
			AssertEquals(true, transportJob.IsFCLPort);
		}

		#endregion

		#endregion

		#region TestUpdateStatus

		public void TestUpdateStatus()
		{
			TestUpdateStatusCore();
		}

		protected abstract void TestUpdateStatusCore();

		#endregion

		#region TestSuspendSettingPackages

		public void TestSuspendSettingPackages()
		{
			var transport = (DtbTransport)GetNewBusinessObject();

			AssertEquals(false, transport.IsSettingPackagesSuspended);

			using (transport.SuspendSettingPackages())
			{
				AssertEquals(true, transport.IsSettingPackagesSuspended);
			}

			AssertEquals(false, transport.IsSettingPackagesSuspended);
		}

		#endregion

		#region FetchStrategy

		public abstract void TestFetchStrategy();

		#endregion

		#region Save

		#region TestSave_PopulateUnqiueIDIfNeeded

		public void TestSave_PopulateUnqiueIDIfNeeded()
		{
			var consolidation = (DtbTransportConsolidation)Factory.New(ExpectedConsolidationType);
			var transport = (DtbTransport)consolidation.Bookings.AddNew();
			AssertEquals("Precondition", "", transport.KM_JobID);

			Factory.Save();
			AssertEquals(ExpectedJobNumber, transport.KM_JobID);
		}

		protected abstract ZString ExpectedJobNumber { get; }

		#endregion

		#endregion

		#region TestUniversalCopyAttributes

		public void TestUniversalCopyAttributes()
		{
			var transport = (DtbTransport)Factory.New(TestedTypeHelper.GetTestedType(GetType()));
			var componentType = transport.GetType();
			var km_kb_bookingInfo = BusinessObjectToCopyTemplateReflectionHelper.GetProperties(componentType).First(info => info.Name == DtbBookingSchema.Constants.KM_KB_Booking);
			AssertEquals("KM_KB_Booking should have UniversalCopyAlwaysCopyPropertyAttribute.CopyMode.AtTheBeginning attribute.", true, km_kb_bookingInfo.GetCustomAttributes(typeof(UniversalCopyAlwaysCopyPropertyAttribute), true)
				.Cast<UniversalCopyAlwaysCopyPropertyAttribute>().Any(attr => attr.Mode == UniversalCopyAlwaysCopyPropertyAttribute.CopyMode.AtTheBeginning));
		}

		#endregion

		#region IDocAddresses Members

		#region TestIDocAddresses_CanDeleteAddress

		public void TestIDocAddresses_CanDeleteAddress()
		{
			IDocAddresses transport = (DtbTransport)GetNewBusinessObject();

			AssertEquals(false, transport.CanDeleteAddress(null));
		}

		#endregion

		#region TestIDocAddresses_DocAddresses

		public void TestIDocAddresses_DocAddresses()
		{
			var transport = (DtbTransport)GetNewBusinessObject();

			AssertEquals(typeof(JobDocAddressDependentCollection), transport.DocAddresses.GetType());
			AssertEquals(true, transport.IsRegisteredEditableChildObject(transport.DocAddresses));
		}

		#endregion

		#region TestIDocAddresses_GetCanOverrideCheckpoint

		public void TestIDocAddresses_GetCanOverrideCheckpoint()
		{
			IDocAddresses transport = (DtbTransport)GetNewBusinessObject();

			AssertEquals(ExpectedCanOverrideCheckpoint, transport.GetCanOverrideCheckpoint(null));
		}

		protected abstract SecurityCheckpoint ExpectedCanOverrideCheckpoint { get; }

		#endregion

		#region TestIDocAddresses_GetDocAddressRequirement

		public void TestIDocAddresses_GetDocAddressRequirement()
		{
			TestIDocAddresses_GetDocAddressRequirementCore();
		}

		protected abstract void TestIDocAddresses_GetDocAddressRequirementCore();

		#endregion

		#region TestIDocAddresses_GetOrgHeaderList

		public void TestIDocAddresses_GetOrgHeaderList()
		{
			IDocAddresses transport = (DtbTransport)GetNewBusinessObject();

			AssertEquals(typeof(DebtorCollection), transport.GetOrgHeaderList(DocAddressType.ClientRequestedBillingParty).GetType());
			TestIDocAddresses_GetOrgHeaderListCore();
		}

		protected virtual void TestIDocAddresses_GetOrgHeaderListCore()
		{
		}

		#endregion

		#region TestIDocAddresses_SupportedAddressTypes

		public void TestIDocAddresses_SupportedAddressTypes()
		{
			IDocAddresses transport = (DtbTransport)GetNewBusinessObject();

			AssertContainsExactElementsInAnyOrder(ExpectedSupportedDocAddressTypes, transport.SupportedAddressTypes);
		}

		protected virtual DocAddressType[] ExpectedSupportedDocAddressTypes
		{
			get { return new[] { DocAddressType.ClientRequestedBillingParty }; }
		}

		#endregion

		#endregion

		#region IDocManagerSupport Members

		public void TestIDocManagerSupport_DocManagerInfo()
		{
			var transport = (DtbTransport)Factory.New(TestedTypeHelper.GetTestedType(GetType()));
			var iConsignmentDocManager = (IDocManagerSupport)transport;
			AssertNotNull(iConsignmentDocManager.DocManagerInfo);
			AssertEquals(ExpectedDocManagerInfoType, iConsignmentDocManager.DocManagerInfo.GetType());
			AssertEquals(ExpectedDocManagerCode, iConsignmentDocManager.DocManagerInfo.DocManagerCode);
		}

		protected abstract Type ExpectedDocManagerInfoType { get; }
		protected abstract string ExpectedDocManagerCode { get; }

		#endregion

		#region IDocumentSupportable Members

		public void TestIDocumentSupportable_DocumentSupporter()
		{
			var transport = (DtbTransport)Factory.New(TestedTypeHelper.GetTestedType(GetType()));
			var iConsignmentDocumentSupportable = (IDocumentSupportable)transport;
			AssertNotNull(iConsignmentDocumentSupportable.DocumentSupporter);
			AssertEquals(ExpectedDocumentSupporterType, iConsignmentDocumentSupportable.DocumentSupporter.GetType());
		}

		protected abstract Type ExpectedDocumentSupporterType { get; }

		#endregion

		#region IEDocsProvider Members

		public void TestIEDocsProvider_GetEDocsProviderSupporter()
		{
			var transport = (DtbTransport)Factory.New(TestedTypeHelper.GetTestedType(GetType()));
			var iConsignmentEDocsProvider = (IEDocsProvider)transport;
			AssertNotNull(iConsignmentEDocsProvider.GetEDocsProviderSupporter());
			AssertEquals(typeof(JobInvoicingEDocsProviderSupporter), iConsignmentEDocsProvider.GetEDocsProviderSupporter().GetType());
		}

		#endregion

		#region IJobInvoicingPlugIn Members

		public void TestIJobInvoicingPlugIn()
		{
			var transport = GetSaveableTransportJob();
			IJobInvoicingPlugIn invoicingPlugIn = transport;
			AssertEquals(ExpectedInvoicingSupporterType, invoicingPlugIn.InvoicingSupporter.GetType());
		}

		protected abstract Type ExpectedInvoicingSupporterType { get; }

		#region IJobHeaderParent Members

		public void TestIJobHeaderParent()
		{
			IJobHeaderParent parent = GetSaveableTransportJob();
			AssertEquals(true, parent.AllowInvoiceDeletion);
			AssertEquals("", parent.JobNumber);
			AssertNoExceptionThrown(() => parent.OnJobCreating(null));
			AssertNoExceptionThrown(() => parent.OnJobDeleting(null));

			parent.SetJobNumberFieldOnSaving();
			AssertNotEquals("", parent.JobNumber);
		}

		#endregion

		#region JobCreatedEventHandler

		public void JobCreatedEventHandler()
		{
			var transportJob = GetSaveableTransportJob();
			bool isJobCreatedEventCalled = false;
			transportJob.JobCreated += delegate
			{ isJobCreatedEventCalled = true; };
			AssertEquals("Precondition", false, isJobCreatedEventCalled);

			((IJobHeaderParent)transportJob).OnJobCreated(null);
			AssertEquals(true, isJobCreatedEventCalled);
		}

		#endregion

		#region IJobHeaderParentCore Members

		public void TestIJobHeaderParentCore()
		{
			var transport = GetSaveableTransportJob();
			var parentCore = transport as IJobHeaderParentCore;
			AssertEquals(transport.PK, parentCore.PK);
			AssertEquals(transport.TableName, parentCore.TableName);
			AssertEquals(false, parentCore.IsInDatabase);
			AssertEquals(transport.Factory, parentCore.Factory);

			Factory.Save();
			AssertEquals(true, parentCore.IsInDatabase);
		}

		#endregion

		#region IJobNumber Members

		public void TestIJobNumber()
		{
			var transport = GetSaveableTransportJob();
			var jobNumber = transport as IJobNumber;
			Factory.Save();
			AssertEquals(false, transport.KM_JobID.IsEmpty);
			AssertEquals(transport.KM_JobID, jobNumber.JobNumber);
		}

		#endregion

		protected abstract DtbTransport GetSaveableTransportJob();

		#endregion

		#region IAddress Members

		public void TestGetAddress()
		{
			var transport = GetSaveableTransportJob();
			var organisation = Helper.CreateOrganisation("QWESYD");
			var pickupAddress = Helper.AddAddressToOrganisation(organisation, "Pickup Addy", OrgAddressType.Pickup);
			var officeAddress = Helper.AddAddressToOrganisation(organisation, "Office Addy", OrgAddressType.Office);
			var deliveryAddress = Helper.AddAddressToOrganisation(organisation, "Delivery Addy", OrgAddressType.Delivery);

			SetUpAddress(organisation, transport);
			var address = transport.GetAddress(DocAddressType);

			AssertEquals(officeAddress.PK, address.E2_OA_Address);
		}

		protected abstract DocAddressType DocAddressType { get; }
		protected abstract void SetUpAddress(OrgHeader organization, DtbTransport transport);

		#endregion

		#region Implementation

		DtbTransportTmpl CreateTransportTemplate(ZString code, ZString description, ZString direction, string ratingFreightMode = "")
		{
			var template = (DtbTransportTmpl)Factory.New(ExpectedTemplateType);
			template.KT_Code = code;
			template.KT_Description = description;
			template.KT_Direction = direction;
			template.KT_RatingFreightMode = ratingFreightMode;
			return template;
		}

		#endregion

		#region TestNotify

		class TestNotify : INotifications
		{
			void INotifications.Add(INotification notification)
			{
				Notifications.Add(notification);
				if (notification != null)
				{
					LastNotification = notification;
				}
			}

			public List<INotification> Notifications = new List<INotification>();
			public INotification LastNotification { get; private set; }
		}

		#endregion

		#region Job Header Deactivationtion Tests

		public void TestJobHeaderIsNotDeactivatedWhenFactoryHasInvoicingPlugInGUIBusinessContext()
		{
			AssertJobHeaderDeactivation(true);
		}

		public void TestJobHeaderIsDeactivatedWhenFactoryDoesNotHaveInvoicingPlugInGUIBusinessContext()
		{
			AssertJobHeaderDeactivation(false);
		}

		void AssertJobHeaderDeactivation(bool hasInvoicingPlugInGUIContext)
		{
			var transport = GetSaveableTransportJob();
			var jobLoader = new JobHeader.Loader(transport);
			var job = jobLoader.TryCreate();
			Factory.Save();

			transport.IsCancelled = true;
			if (hasInvoicingPlugInGUIContext)
			{
				Factory.SetContext(BusinessContext.InvoicingPlugInGUI);
			}

			var assertionMessage1 = string.Format("transport {0} have InvoicingPluginGUI Business Context", hasInvoicingPlugInGUIContext ? "should" : "should not");
			var assertionMessage2 = string.Format("Job {0} be deactivated by OnSaving method", hasInvoicingPlugInGUIContext ? "should not" : "should");

			Assert("Deactivating transport, IsCancelled flag should be set to true", transport.IsCancelled);
			Assert("Deactivating transport, IsCancelledInfo should have changes", transport.IsCancelledHasChanged);
			AssertEquals(assertionMessage1, hasInvoicingPlugInGUIContext, transport.HasContext(BusinessContext.InvoicingPlugInGUI));
			Assert("Job is not yet deactivated", !job.IsCancelled);

			Factory.Save();

			AssertEquals(assertionMessage2, !hasInvoicingPlugInGUIContext, job.IsCancelled);
		}

		#endregion
	}
}
