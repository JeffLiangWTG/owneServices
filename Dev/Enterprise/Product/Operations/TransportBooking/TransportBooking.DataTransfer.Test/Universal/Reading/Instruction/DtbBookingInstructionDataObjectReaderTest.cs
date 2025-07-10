using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.DataTransfer.Universal;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.TransportBookings.DataTransfer.Universal.Testing
{
	sealed class DtbBookingInstructionDataObjectReaderTest : OrganizationAddressTestHelper
	{
		[TestDate(2011, 1, 1)]
		public void TestWorkflowCustomFields()
		{
			var processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate.P0_ProcessType = "TBI";
			processTaskTemplate.P0_IsActive = true;

			var genCustomColumnString = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnString.XC_Name = "Textual context";
			genCustomColumnString.XC_Type = "STR";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnString);

			var genCustomColumnDate = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnDate.XC_Name = "First Date";
			genCustomColumnDate.XC_Type = "DAT";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnDate);

			var genCustomColumnDecimal = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnDecimal.XC_Name = "Deci Deca";
			genCustomColumnDecimal.XC_Type = "DEC";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnDecimal);

			var genCustomColumnBool = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnBool.XC_Name = "Flagger";
			genCustomColumnBool.XC_Type = "BOO";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnBool);

			var genCustomColumnInt = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnInt.XC_Name = "Integer Mate";
			genCustomColumnInt.XC_Type = "INT";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnInt);

			var genCustomColumnInt2 = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnInt2.XC_Name = "Integraler";
			genCustomColumnInt2.XC_Type = "INT";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnInt2);

			var instructionDataObject = new Instruction(DefaultDataObjectWriterStrategy.TestInstance);
			instructionDataObject.SetCustomizedFieldCollection(() => new List<CustomizedField>());
			instructionDataObject.CustomizedFieldCollection.Add("Textual context", new ZString("HELLO"));
			instructionDataObject.CustomizedFieldCollection.Add("First Date", new ZDateTime(2011, 1, 1));
			instructionDataObject.CustomizedFieldCollection.Add("Deci Deca", new ZDecimal(0.3));
			instructionDataObject.CustomizedFieldCollection.Add("Flagger", ZBool.True);
			instructionDataObject.CustomizedFieldCollection.Add("Integer Mate", new ZInt(42));

			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			Factory.SaveForTesting();

			var reader = new DtbBookingInstructionDataObjectReader(instructionDataObject, Logger, Factory, booking);
			var instruction = reader.ReadIntoBusinessObject();

			CombineAssertions(delegate
			{
				var customFields = instruction.GetUserDefinedValues();
				var result = "";
				foreach (var customField in customFields)
				{
					result += customField.PropertyName + " - " + customField.Value + "\r\n";
				}

				AssertMultilineASCIIEquals("All Custom Fields should have been imported with none extra", @"
Deci Deca - 0.3
First Date - 01-Jan-11 00:00:00
Flagger - Y
Integer Mate - 42
Textual context - HELLO
			".Trim(), result);

				AssertEquals(false, Logger.HasErrors);
				AssertEquals(false, Logger.HasWarnings);
			});
		}

		public void TestMatchingExistingInstructionOnSequence()
		{
			var instructionDataObject = new Instruction(DefaultDataObjectWriterStrategy.TestInstance);
			instructionDataObject.Sequence = 2;

			var booking = Helper.CreateBooking();
			var matchingInstruction = booking.Instructions.AddNew();
			matchingInstruction.KN_Sequence = 2;
			var nonMatchingInstruction = booking.Instructions.AddNew();
			nonMatchingInstruction.KN_Sequence = 3;

			var reader = new DtbBookingInstructionDataObjectReader(instructionDataObject, Logger, Factory, booking);
			var instruction = reader.ReadIntoBusinessObject();
			AssertEquals(matchingInstruction.PK, instruction.PK);
		}

		public void TestConfirmationsAndDivots()
		{
			int containerLink1 = 1;
			int containerLink2 = 3;
			int nonExistingContainerLink = 10;
			int packageLink1 = 1;
			int packageLink2 = 2;
			int nonExistingPackageLink = 10;

			var consolidation = Helper.CreateConsolidation();
			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.SetContainerCollection(() => new DataObjectList<Container>());
			universalShipment.ContainerCollection.Add(new Container { ContainerNumber = "CONT1", ContainerType = new ContainerType { Code = "20GP" }, Link = containerLink1 });
			universalShipment.ContainerCollection.Add(new Container { ContainerCount = 1, ContainerNumber = "CONT2", ContainerType = new ContainerType { Code = "20GP" }, Link = containerLink2 });
			universalShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>());
			universalShipment.PackingLineCollection.Add(new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 3, ReferenceNumber = "", Link = packageLink1 });
			universalShipment.PackingLineCollection.Add(new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 6, ReferenceNumber = "", Link = packageLink2 });

			var packageJobReader = new PkgPackageJobDataObjectReader(universalShipment, Logger, Factory, consolidation);
			var packageJob = packageJobReader.ReadIntoBusinessObject();
			var package1 = packageJob.Packages.Single(p => p.KP_PackageQty == 3);
			var package2 = packageJob.Packages.Single(p => p.KP_PackageQty == 6);
			var container1 = packageJob.FindPackageByRawBarcodeOrAddNewIfSSCC("CONT1");
			var container2 = packageJob.FindPackageByRawBarcodeOrAddNewIfSSCC("CONT2");

			var instructionDataObject = new Instruction(DefaultDataObjectWriterStrategy.TestInstance);

			// Add Container Links
			AddConfirmationContainerLink(instructionDataObject, nonExistingContainerLink, 1, "DONOTCREATE1"); // Has no container to link to
			AddConfirmationContainerLink(instructionDataObject, null, 1, "DONOTCREATE2"); // Has no container link

			var containerPackageDivot1 = AddConfirmationContainerLink(instructionDataObject, containerLink1, 1, "CREF1");
			var containerPackageDivot2 = AddConfirmationContainerLink(instructionDataObject, containerLink2, 2, "CREF2-1");
			containerPackageDivot2.ConfirmationCollection[0].Quantity = 1;
			containerPackageDivot2.ConfirmationCollection.Add(new Confirmation { Quantity = 1, Reference = "CREF2-2" }); // Split Containers onto 2 confirmations

			containerPackageDivot1.ConfirmationCollection.Add(new Confirmation { Quantity = 1, Reference = "IREF1" });
			containerPackageDivot1.ConfirmationCollection.Add(new Confirmation { Quantity = 1, Reference = "IREF2" });
			containerPackageDivot2.ConfirmationCollection.Add(new Confirmation { Quantity = 2, Reference = "IREF1" });
			containerPackageDivot2.ConfirmationCollection.Add(new Confirmation { Quantity = 2, Reference = "IREF2" });

			// Add Package Links
			AddConfirmationPackageLink(instructionDataObject, nonExistingPackageLink, 1, "DONOTCREATE3"); // Has no package to link to
			AddConfirmationPackageLink(instructionDataObject, null, 1, "DONOTCREATE4"); // Has no package link

			var loosePackingPackageDivot1 = AddConfirmationPackageLink(instructionDataObject, packageLink1, 3, "PREF1");
			var loosePackingPackageDivot2 = AddConfirmationPackageLink(instructionDataObject, packageLink2, 5, "PREF2");

			loosePackingPackageDivot1.ConfirmationCollection.Add(new Confirmation { Quantity = 3, Reference = "IREF1" });
			loosePackingPackageDivot1.ConfirmationCollection.Add(new Confirmation { Quantity = 3, Reference = "IREF2" });
			loosePackingPackageDivot2.ConfirmationCollection.Add(new Confirmation { Quantity = 5, Reference = "IREF1" });
			loosePackingPackageDivot2.ConfirmationCollection.Add(new Confirmation { Quantity = 5, Reference = "IREF2" });

			// Read in Data Object
			var booking = Helper.CreateBooking();
			var reader = new DtbBookingInstructionDataObjectReader(instructionDataObject, Logger, Factory, booking, packageJobReader);
			var instruction = reader.ReadIntoBusinessObject();
			instruction.PackageDivots.ApplySort(DtbBookingInstructionPkgDivotSchema.KD_Quantity.Name, System.ComponentModel.ListSortDirection.Ascending);
			AssertEquals(4, instruction.PackageDivots.Count);
			AssertEquals(7, instruction.Confirmations.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "Qty:11 Ref:IREF1", "Qty:11 Ref:IREF2" }, GetConfirmationsAsFormattedStrings(instruction.Confirmations.Where(c => c.PackageDivot == null)));

			var containerDivot1 = instruction.PackageDivots[0];
			AssertEquals(container1.PK, containerDivot1.KD_KP_Package);
			AssertEquals(1, containerDivot1.KD_Quantity);
			AssertContainsExactElementsInAnyOrder(new[] { "Qty:1 Ref:CREF1" }, GetConfirmationsAsFormattedStrings(containerDivot1.ConfirmationsDivotOnly));

			var containerDivot2 = instruction.PackageDivots[1];
			AssertEquals(container2.PK, containerDivot2.KD_KP_Package);
			AssertEquals(2, containerDivot2.KD_Quantity);
			AssertContainsExactElementsInAnyOrder(new[] { "Qty:1 Ref:CREF2-1", "Qty:1 Ref:CREF2-2" }, GetConfirmationsAsFormattedStrings(containerDivot2.ConfirmationsDivotOnly));

			var packageDivot1 = instruction.PackageDivots[2];
			AssertEquals(package1.PK, packageDivot1.KD_KP_Package);
			AssertEquals(3, packageDivot1.KD_Quantity);
			AssertContainsExactElementsInAnyOrder(new[] { "Qty:3 Ref:PREF1" }, GetConfirmationsAsFormattedStrings(packageDivot1.ConfirmationsDivotOnly));

			var packageDivot2 = instruction.PackageDivots[3];
			AssertEquals(package2.PK, packageDivot2.KD_KP_Package);
			AssertEquals(5, packageDivot2.KD_Quantity);
			AssertContainsExactElementsInAnyOrder(new[] { "Qty:5 Ref:PREF2" }, GetConfirmationsAsFormattedStrings(packageDivot2.ConfirmationsDivotOnly));
		}

		IEnumerable<string> GetConfirmationsAsFormattedStrings(IEnumerable<DtbBookingConfirmation> confirmations)
		{
			return confirmations.Select(c => string.Format("Qty:{0} Ref:{1}", c.KK_Quantity, c.KK_ReferenceNum));
		}

		InstructionContainerLink AddConfirmationContainerLink(Instruction instructionDataObject, ZInt? containerLink, ZInt divotQuantity, ZString confirmationReference)
		{
			var instructionContainerLink = new InstructionContainerLink { ContainerLink = containerLink, Quantity = divotQuantity };
			instructionContainerLink.ConfirmationCollection = new List<Confirmation> { new Confirmation { Quantity = divotQuantity, Reference = confirmationReference } };

			if (instructionDataObject.InstructionContainerLinkCollection == null)
			{
				instructionDataObject.SetInstructionContainerLinkCollection(() => new List<InstructionContainerLink>());
			}

			instructionDataObject.InstructionContainerLinkCollection.Add(instructionContainerLink);
			return instructionContainerLink;
		}

		InstructionPackingLineLink AddConfirmationPackageLink(Instruction instructionDataObject, ZInt? packingLineLink, ZInt divotQuantity, ZString confirmationReference)
		{
			return AddConfirmationPackageLink(instructionDataObject, packingLineLink, divotQuantity, divotQuantity, confirmationReference);
		}

		InstructionPackingLineLink AddConfirmationPackageLink(Instruction instructionDataObject, ZInt? packingLineLink, ZInt divotQuantity, ZInt? confirmationQuantity,
			ZString confirmationReference)
		{
			var instructionPackageLink = new InstructionPackingLineLink { PackingLineLink = packingLineLink, Quantity = divotQuantity };

			if (instructionPackageLink.ConfirmationCollection == null)
			{
				instructionPackageLink.ConfirmationCollection = new List<Confirmation>();
			}
			var confirmation = new Confirmation { Quantity = confirmationQuantity, Reference = confirmationReference };
			instructionPackageLink.ConfirmationCollection.Add(confirmation);

			if (instructionDataObject.InstructionPackingLineLinkCollection == null)
			{
				instructionDataObject.SetInstructionPackingLineLinkCollection(() => new List<InstructionPackingLineLink>());
			}
			instructionDataObject.InstructionPackingLineLinkCollection.Add(instructionPackageLink);

			return instructionPackageLink;
		}

		public void TestConfirmationTimeZone()
		{
			// setup Orgs / Time Zones
			var now = new ZDateTime(ZDateTime.Now.Year, 1, 2); // January - Sydney and Brisbane differ in Time Zones
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "NZAKL";

			var sydneyOrg = Factory.New<OrgHeader>();
			sydneyOrg.OH_FullName = "Sydney Co";
			sydneyOrg.MainAddress.OA_Address1 = "1 ABCDE ST";
			sydneyOrg.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			sydneyOrg.MainAddress.OA_City = "Sydney";
			sydneyOrg.MainAddress.OA_RN_NKCountryCode = "AU";

			var brisbaneOrg = Factory.New<OrgHeader>();
			brisbaneOrg.OH_FullName = "Brisbane Co";
			brisbaneOrg.OH_RL_NKClosestPort = "AUBNE";
			brisbaneOrg.MainAddress.OA_Address1 = "1 FGHIJ ST";
			brisbaneOrg.MainAddress.OA_City = "Brisbane";
			brisbaneOrg.MainAddress.OA_RN_NKCountryCode = "AU";

			Factory.SaveForTesting();

			// setup Job Info
			int packageLink1 = 1;
			var consolidation = Helper.CreateConsolidation();
			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>());
			universalShipment.PackingLineCollection.Add(new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 3, ReferenceNumber = "PACK1", Link = packageLink1 });

			var packageJobReader = new PkgPackageJobDataObjectReader(universalShipment, Logger, Factory, consolidation);
			var packageJob = packageJobReader.ReadIntoBusinessObject();
			var package1 = packageJob.FindPackageByRawBarcodeOrAddNewIfSSCC("PACK1");

			// setup Instructions with different time zones
			var sydneyInstructionDO = CreateInstructionWithConfirmation(now, packageLink1, CreateAddress(sydneyOrg.OH_Code, "Sydney Co", "1 ABCDE ST", "Sydney", "AU", "AUSTRALIA"));
			var brisbaneInstructionDO = CreateInstructionWithConfirmation(now, packageLink1, CreateAddress(brisbaneOrg.OH_Code, "Brisbane Co", "1 FGHIJ ST", "Brisbane", "AU", "AUSTRALIA"));
			var noAddressInstructionDO = CreateInstructionWithConfirmation(now, packageLink1, null);

			ReadAndAssertConfirmation(consolidation, packageJobReader, sydneyInstructionDO, sydneyOrg.OH_Code, now, -11);
			ReadAndAssertConfirmation(consolidation, packageJobReader, brisbaneInstructionDO, brisbaneOrg.OH_Code, now, -10);
			ReadAndAssertConfirmation(consolidation, packageJobReader, noAddressInstructionDO, "", now, -13); // NZAKL
		}

		void ReadAndAssertConfirmation(DtbBookingConsolidation consol, PkgPackageJobDataObjectReader packageJobReader, Instruction instructionDO, ZString orgCode, ZDateTime now, ZInt zoneOffSet)
		{
			var booking = consol.Bookings.AddNew();
			var reader = new DtbBookingInstructionDataObjectReader(instructionDO, Logger, Factory, booking, packageJobReader);
			var instructionBO = reader.ReadIntoBusinessObject();

			AssertEquals(orgCode, instructionBO.Address.Organisation != null ? instructionBO.Address.Organisation.OH_Code.ToString() : "");
			AssertEquals(1, instructionBO.Confirmations.Count);

			var confirmation = instructionBO.Confirmations[0];
			AssertEquals(now, confirmation.KK_Estimated);
			AssertEquals(now.AddHours(zoneOffSet), confirmation.KK_EstimatedUtc);
			AssertEquals(now.AddDays(1), confirmation.KK_RequiredFrom);
			AssertEquals(now.AddDays(1).AddHours(zoneOffSet), confirmation.KK_RequiredFromUtc);
			AssertEquals(now.AddDays(2), confirmation.KK_RequiredTo);
			AssertEquals(now.AddDays(2).AddHours(zoneOffSet), confirmation.KK_RequiredToUtc);
		}

		Instruction CreateInstructionWithConfirmation(ZDateTime now, int packingLineLink, OrganizationAddress address)
		{
			var instruction = new Instruction(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Address = address,
			};
			instruction.SetInstructionPackingLineLinkCollection(() => new List<InstructionPackingLineLink>()
			{
				new InstructionPackingLineLink
				{
					Quantity = 1,
					PackingLineLink = packingLineLink,
					ConfirmationCollection = new List<Confirmation>()
					{
						new Confirmation()
						{
							EstimatedDate = now,
							RequiredFromDate = now.AddDays(1),
							RequiredToDate = now.AddDays(2)
						}
					}
				}
			});
			return instruction;
		}

		OrganizationAddress CreateAddress(ZString orgCode, ZString companyName, ZString address1, ZString city, ZString countryCode, ZString countryName)
		{
			return new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.LocalCartageExporter),
				OrganizationCode = orgCode,
				CompanyName = companyName,
				Address1 = address1,
				City = city,
				Country = new Country() { Code = countryCode, Name = countryName },
			};
		}

		public void TestBasicFieldMappings()
		{
			new OrganisationDataObjectReader(GetNewAddressData_CRAHOLSYD(DocAddressType.LocalCartageCFS), new TestErrorLogger(), Factory).GetMatchedOrNewForTesting();
			Factory.SaveForTesting();

			var equipment = Factory.New<RefEquipment>();
			equipment.RQ_ShortCode = "TRUCK";

			var instructionDataObject = new Instruction(DefaultDataObjectWriterStrategy.TestInstance);
			instructionDataObject.Address = GetNewAddressData_CRAHOLSYD(DocAddressType.LocalCartageCFS);
			instructionDataObject.DropMode = new DropMode { Code = "ASK" };
			instructionDataObject.Equipment = "TRUCK";
			instructionDataObject.IsContainerRateable = true;
			instructionDataObject.IsAuthorisedToLeave = true;
			instructionDataObject.IsLooseRateable = true;
			instructionDataObject.Sequence = 10;
			instructionDataObject.ServiceInstruction = "Service Instruction";
			instructionDataObject.Type = new CodeDescriptionPair { Code = InstructionTypes.Codes.Multi };

			var booking = GetNewBooking();
			var reader = GetNewReader(instructionDataObject, Logger, Factory, booking);
			var instruction = reader.ReadIntoBusinessObject();
			instruction.DocAddresses.Load(); // Have to reload into memory to reflect the delete + Add in Reader

			CombineAssertions(() =>
			{
				AssertJobDocAddressContentMatches_CRAHOLSYDWithoutGovRegNumAndType(instruction.Address);
				AssertEquals("instruction.KN_DropMode", "ASK", instruction.KN_DropMode);
				AssertEquals("instruction.KN_InstructionType", InstructionTypes.Codes.Multi, instruction.KN_InstructionType);
				AssertEquals("instruction.KN_IsContainerRateable", true, instruction.KN_IsContainerRateable);
				AssertEquals("instruction.KN_IsLooseRateable", true, instruction.KN_IsLooseRateable);
				AssertEquals("instruction.KN_IsAuthorisedToLeave", true, instruction.KN_IsAuthorisedToLeave);
				AssertEquals("instruction.KN_RQ_Equipment", equipment.PK, instruction.KN_RQ_Equipment);
				AssertEquals("instruction.KN_Sequence", 10, instruction.KN_Sequence);
				AssertEquals("instruction.KN_ServiceInstruction", "Service Instruction", instruction.KN_ServiceInstruction);
			});
		}

		public void TestAddress()
		{
			var booking = GetNewBooking();
			var instructionDataObject = new Instruction(DefaultDataObjectWriterStrategy.TestInstance) { Address = new OrganizationAddress { AddressType = nameof(DocAddressType.LocalCartageExporter) } };
			var reader1 = GetNewReader(instructionDataObject, Logger, Factory, booking);
			var instruction1 = reader1.ReadIntoBusinessObject();
			AssertEquals(OrganisationTypesList.Codes.CNR, instruction1.OrganisationType);
			instruction1.KN_KM_BookingMovement = booking.GetValue(DtbBookingSchema.PK);
			instruction1.KN_Sequence = 1; // this would have been done by the Transport Reader
			Factory.SaveForTesting();

			var instructionInOtherFactory = new BusinessObjectFactory().Load<DtbBookingInstruction>(instruction1.PK);
			AssertEquals("Should save JobDocAddress to Database.", instruction1.Address.PK, instructionInOtherFactory.Address.PK);
			AssertEquals(true, instruction1.Address.IsEmpty);

			instructionDataObject.Address.Address1 = "Some Street";
			instructionDataObject.Address.CompanyName = "Some Co";
			instructionDataObject.Address.State = "Some State";
			instructionDataObject.Address.City = "Some City";
			var reader2 = GetNewReader(instructionDataObject, Logger, Factory, booking);
			var instruction2 = reader2.ReadIntoBusinessObject();
			instruction2.KN_KM_BookingMovement = booking.GetValue(DtbBookingSchema.PK);
			instruction2.KN_Sequence = 1; // this would have been done by the Transport Reader
			AssertEquals(OrganisationTypesList.Codes.CNR, instruction2.OrganisationType);
			AssertEquals(true, instruction2.Address.E2_AddressOverride);
			AssertEquals("Some Street", instruction2.Address.E2_Address1);
			AssertEquals("Some City", instruction2.Address.E2_City);
			AssertEquals("Some Co", instruction2.Address.E2_CompanyName);
			AssertEquals("Some State", instruction2.Address.E2_State);
			Factory.SaveForTesting();

			var query = new ZQuery(JobDocAddressSchema.E2_ParentID, instruction2.PK);
			AssertEquals("Should only have one DocAddress.", 1, Factory.BOFactory.GetDatabaseCount(typeof(JobDocAddress), query));
		}

		public void TestAddress_IsProperlyDeleted()
		{
			var booking = GetNewBooking();
			var pickUpAddress = GetNewAddressData_CRAHOLSYD(nameof(DocAddressType.LocalCartageExporter));
			new OrganisationDataObjectReader(pickUpAddress, Logger, Factory).GetMatchedOrNewForTesting();
			Factory.SaveForTesting();

			var instructionDataObject = new Instruction(DefaultDataObjectWriterStrategy.TestInstance) { Address = pickUpAddress, Type = new CodeDescriptionPair { Code = InstructionTypes.Codes.PickUp } };
			var reader1 = GetNewReader(instructionDataObject, Logger, Factory, booking);
			var instruction1 = reader1.ReadIntoBusinessObject();
			AssertEquals(OrganisationTypesList.Codes.CNR, instruction1.OrganisationType);
			AssertAddressContentMatches_CRAHOLSYD(instruction1.Address.Address);

			instruction1.KN_KM_BookingMovement = booking.GetValue(DtbBookingSchema.PK);
			instruction1.KN_Sequence = 1; // this would have been done by the Transport Reader
			Factory.SaveForTesting();

			var newPickUpAddress = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.LocalCartageExporter));
			new OrganisationDataObjectReader(newPickUpAddress, Logger, Factory).GetMatchedOrNewForTesting();
			Factory.SaveForTesting();

			var newfactory1 = new UniversalObjectFactory();
			var bookingInNewFactory = (DtbBooking)newfactory1.Load(booking.GetType(), booking.PK);
			var query1 = new ZQuery(JobDocAddressSchema.E2_ParentID, instruction1.PK);
			var poke = newfactory1.Load<JobDocAddress>(query1); // load the bizO into the factory to mix the rows and bizOs

			instructionDataObject.Address = newPickUpAddress;
			var reader2 = GetNewReader(instructionDataObject, Logger, newfactory1, bookingInNewFactory);
			var instruction2 = reader2.ReadIntoBusinessObject();
			AssertEquals(OrganisationTypesList.Codes.CNR, instruction2.OrganisationType);
			AssertAddressContentMatches_INTHEMSYD(instruction2.Address.Address);
			instruction2.KN_KM_BookingMovement = booking.GetValue(DtbBookingSchema.PK);
			instruction2.KN_Sequence = 1; // this would have been done by the Transport Reader
			newfactory1.SaveForTesting();

			var newfactory2 = new UniversalObjectFactory();
			var query2 = new ZQuery(JobDocAddressSchema.E2_ParentID, instruction2.PK);
			var docAddresses = newfactory2.Load<JobDocAddress>(query2);
			AssertEquals("Should only have one DocAddress.", 1, docAddresses.Length);
		}

		public void TestAddress_DoesNotUseUnmatchedOrgWhenAddressIsEmpty()
		{
			SetUseUnmatchedOrganisationForMatchingRegistry(true);

			var booking = GetNewBooking();
			var deliveryAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.LocalCartageImporter) };
			var instructionDataObject = new Instruction(DefaultDataObjectWriterStrategy.TestInstance) { Address = deliveryAddress };
			var reader = GetNewReader(instructionDataObject, Logger, Factory, booking);
			var instruction = reader.ReadIntoBusinessObject();

			AssertEquals("Instruction Address should be empty.", CargoWise.Types.ZGuid.Empty, instruction.Address.E2_OA_Address);
			AssertEquals("Instruction Address should be empty.", false, instruction.Address.E2_AddressOverride);
			AssertEquals("Instruction should have correct Address Type.", OrganisationTypesList.Codes.CNE, instruction.OrganisationType);
		}

		public void TestUnmatchedOrgNotesAreStoredForUnmatchedInstructionAddresses()
		{
			SetUseUnmatchedOrganisationForMatchingRegistry(true);

			AssertUnmatchedOrgNotesAreStoredForAddressType(DocAddressType.LocalCartageExporter, "Consignor");
			AssertUnmatchedOrgNotesAreStoredForAddressType(DocAddressType.LocalCartageImporter, "Consignee");
			AssertUnmatchedOrgNotesAreStoredForAddressType(DocAddressType.LocalCartageCFS, "CFS");
			AssertUnmatchedOrgNotesAreStoredForAddressType(DocAddressType.LocalCartageCTO, "CTO");
			AssertUnmatchedOrgNotesAreStoredForAddressType(DocAddressType.LocalCartageYard, "CYD");
		}

		void AssertUnmatchedOrgNotesAreStoredForAddressType(DocAddressType orgType, string orgTypeString)
		{
			var booking = GetNewBooking();
			var initialUnmatchedOrganisationNotes = booking.Notes.FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description);
			AssertEquals("Precondition", 0, initialUnmatchedOrganisationNotes.Length);

			var instructionDataObject = new Instruction(DefaultDataObjectWriterStrategy.TestInstance);
			instructionDataObject.Address = GetNewAddressData_CRAHOLSYD(orgType);

			var reader = GetNewReader(instructionDataObject, Logger, Factory, booking);
			var instruction = reader.ReadIntoBusinessObject();
			instruction.DocAddresses.Load(); // Have to reload into memory to reflect the delete + Add in Reader
			AssertEquals(true, instruction.Address.Organisation.IsSystemDefinedOrganisation);

			var unmatchedOrganisationNotes = booking.Notes.FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description);
			AssertEquals("Unmatched organisation note should have been created", 1, unmatchedOrganisationNotes.Length);

			AssertMultilineASCIIEquals("Note should be added to the booking", string.Format(@"Organisation Type: {0}
Owner Code: 
EDI Code: CRAHOLSYD
Organisation Name: CRACKERJACK HOLDINGS
Address Line 1: 1804 Fudrucker Way
Address Line 2: 
City: BOTANY
Post Code: 2035
State or Province: NSW
Country: AU
Doc Address Type:", orgTypeString), unmatchedOrganisationNotes[0].ST_NoteText.TrimEnd());
		}

		DtbBookingInstructionDataObjectReader GetNewReader(Instruction instructionDataObject,
			IXmlImportLogger logger, UniversalObjectFactory factory, DtbBooking booking)
		{
			return new DtbBookingInstructionDataObjectReader(instructionDataObject, logger, factory, booking);
		}

		DtbBooking GetNewBooking()
		{
			return Helper.CreateBooking();
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory.BOFactory)); }
		}

		TransportBookingTestHelper helper;
	}
}
