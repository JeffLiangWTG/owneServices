using Enterprise.Core;
using Enterprise.Registry.Business.Testing;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.TransportCommon.Registry.Testing
{
	[TestedType(typeof(DateAndReference))]
	public class DateAndReferenceTest : RegistryBusinessObjectTemplateTestCase<DateAndReference>
	{
		#region Properties

		#region Code

		public void TestCode()
		{
			BizObj.Code = "DLV";
			AssertEquals("DLV", BizObj.Code);
		}

		#endregion

		#region Description

		public void TestDescription()
		{
			BizObj.Description = (NoResString)"Delivery";
			AssertEquals("Delivery", BizObj.Description);
		}

		#endregion

		#region BookingDirection

		public void TestBookingDirection()
		{
			BizObj.BookingDirection = nameof(DtbBookingDirection.DLV);
			AssertEquals(nameof(DtbBookingDirection.DLV), BizObj.BookingDirection);
		}

		#endregion

		#region InstructionType

		public void TestInstructionType()
		{
			BizObj.InstructionType = InstructionTypes.Codes.Delivery;
			AssertEquals(InstructionTypes.Codes.Delivery, BizObj.InstructionType);
		}

		#endregion

		#region OrganisationType

		public void TestOrganisationType()
		{
			BizObj.OrganisationType = DatesAndReference.Any;
			AssertEquals(DatesAndReference.Any, BizObj.OrganisationType);
		}

		#endregion

		#region ContainerMode

		public void TestContainerMode()
		{
			BizObj.ContainerMode = "LSE";
			AssertEquals("LSE", BizObj.ContainerMode);
		}

		#endregion

		#region AutoAdd

		public void TestAutoAdd()
		{
			BizObj.AutoAdd = true;
			AssertEquals(true, BizObj.AutoAdd);

			BizObj.AutoAdd = false;
			AssertEquals(false, BizObj.AutoAdd);
		}

		#endregion

		#region AllowActualDate

		public void TestAllowActualDate()
		{
			BizObj.AllowActualDate = true;
			AssertEquals(true, BizObj.AllowActualDate);

			BizObj.AllowActualDate = false;
			AssertEquals(false, BizObj.AllowActualDate);
		}

		#endregion

		#region TestAllowSlotDate

		public void TestAllowSlotDate()
		{
			BizObj.AllowSlotDate = true;
			AssertEquals(true, BizObj.AllowSlotDate);

			BizObj.AllowSlotDate = false;
			AssertEquals(false, BizObj.AllowSlotDate);
		}

		#endregion

		#region AllowEstimatedDate

		public void TestAllowEstimatedDate()
		{
			BizObj.AllowEstimatedDate = true;
			AssertEquals(true, BizObj.AllowEstimatedDate);

			BizObj.AllowEstimatedDate = false;
			AssertEquals(false, BizObj.AllowEstimatedDate);
		}

		#endregion

		#region AllowRequiredFromDate

		public void TestAllowRequiredFromDate()
		{
			BizObj.AllowRequiredFromDate = true;
			AssertEquals(true, BizObj.AllowRequiredFromDate);

			BizObj.AllowRequiredFromDate = false;
			AssertEquals(false, BizObj.AllowRequiredFromDate);
		}

		#endregion

		#region AllowRequiredFromLabel

		public void TestAllowRequiredFromLabel()
		{
			BizObj.AllowRequiredFromDate = false;
			BizObj.AllowRequiredFromDate = true;
			AssertEquals("Required From", BizObj.AllowRequiredFromLabel);

			BizObj.AllowRequiredFromLabel = (NoResString)"123456";
			AssertEquals("123456", BizObj.AllowRequiredFromLabel);

			BizObj.AllowRequiredFromDate = false;
			AssertEquals("", BizObj.AllowRequiredFromLabel);
		}

		#endregion

		#region AllowRequiredToDate

		public void TestAllowRequiredToDate()
		{
			BizObj.AllowRequiredToDate = true;
			AssertEquals(true, BizObj.AllowRequiredToDate);

			BizObj.AllowRequiredToDate = false;
			AssertEquals(false, BizObj.AllowRequiredToDate);
		}

		#endregion

		#region AllowRequiredToLabel

		public void TestAllowRequiredToLabel()
		{
			BizObj.AllowRequiredToDate = false;
			BizObj.AllowRequiredToDate = true;
			AssertEquals("Required To", BizObj.AllowRequiredToLabel);

			BizObj.AllowRequiredToLabel = (NoResString)"123456";
			AssertEquals("123456", BizObj.AllowRequiredToLabel);

			BizObj.AllowRequiredToDate = false;
			AssertEquals("", BizObj.AllowRequiredToLabel);
		}

		#endregion

		#region AllowReference

		public void TestAllowReference()
		{
			BizObj.AllowReference = true;
			AssertEquals(true, BizObj.AllowReference);

			BizObj.AllowReference = false;
			AssertEquals(false, BizObj.AllowReference);
		}

		#endregion

		#region AllowReceivedBy

		public void TestAllowReceivedBy()
		{
			BizObj.AllowReceivedBy = true;
			AssertEquals(true, BizObj.AllowReceivedBy);

			BizObj.AllowReceivedBy = false;
			AssertEquals(false, BizObj.AllowReceivedBy);
		}

		#endregion

		#endregion

		#region Validation

		#region ValidationCode

		public void TestValidationCode()
		{
			var dateAndReferences = new DateAndReferenceCollection();
			var dateAndReference1 = dateAndReferences.AddNew();
			var dateAndReference2 = dateAndReferences.AddNew();
			dateAndReference1.Code = (NoResString)"PIC";
			dateAndReference1.OrganisationType = "CFS";
			dateAndReference2.Code = (NoResString)"PIC";
			dateAndReference2.OrganisationType = DatesAndReference.Any;
			dateAndReference1.ValidateCode();
			AssertNoErrors(dateAndReference1.CodeInfo);

			dateAndReference2.OrganisationType = "CFS";
			dateAndReference1.ValidateCode();
			AssertHasError(dateAndReference1.CodeInfo, "Code with Booking Direction, Instruction Type, Organization Type and Mode must be unique.");
		}

		#endregion

		#region ValidationDescription

		public void TestValidationDescription()
		{
			var dateAndReferences = new DateAndReferenceCollection();
			var dateAndReference = dateAndReferences.AddNew();

			dateAndReference.ValidateDescription();
			AssertHasErrors(dateAndReference.DescriptionInfo);

			dateAndReference.Description = (NoResString)"Description";
			dateAndReference.ValidateDescription();
			AssertNoErrors(dateAndReference.DescriptionInfo);
		}

		#endregion

		#region TestValidateForUniqueCodeAndDescription

		public void TestValidateForUniqueCodeAndDescription()
		{
			var dateAndReferences = new DateAndReferenceCollection();
			var dateAndReference = dateAndReferences.AddNew();
			dateAndReference.Code = "ABC";
			dateAndReference.Description = (NoResString)"ABCD";

			var dateAndReferenceWithMatchingCodeAndDescription = dateAndReferences.AddNew();
			dateAndReferenceWithMatchingCodeAndDescription.Code = "ABC";
			dateAndReferenceWithMatchingCodeAndDescription.Description = (NoResString)"ABCD";
			AssertNoErrors(dateAndReferenceWithMatchingCodeAndDescription.DescriptionInfo);

			var dateAndReferenceWithMatchingCodeAndDifferentDescription = dateAndReferences.AddNew();
			dateAndReferenceWithMatchingCodeAndDifferentDescription.Code = "ABC";
			dateAndReferenceWithMatchingCodeAndDifferentDescription.Description = (NoResString)"ABCD1";
			AssertHasError(dateAndReferenceWithMatchingCodeAndDifferentDescription.DescriptionInfo, "This confirmation code ABC already exists with description ABCD. Description must be unique per code.");
		}

		#endregion

		#region ValidationBookingDirection

		public void TestValidationBookingDirection()
		{
			var dateAndReferences = new DateAndReferenceCollection();
			var dateAndReference1 = dateAndReferences.AddNew();
			var dateAndReference2 = dateAndReferences.AddNew();
			dateAndReference1.Code = "PIC";
			dateAndReference1.BookingDirection = Constants.CartageDirection.Destination;
			dateAndReference2.Code = "PIC";
			dateAndReference2.BookingDirection = DatesAndReference.Any;
			dateAndReference1.ValidateBookingDirection();
			AssertNoErrors(dateAndReference1.BookingDirectionInfo);

			dateAndReference2.BookingDirection = Constants.CartageDirection.Destination;
			dateAndReference1.ValidateBookingDirection();
			AssertHasError(dateAndReference1.BookingDirectionInfo, "Code with Booking Direction, Instruction Type, Organization Type and Mode must be unique.");
		}

		#endregion

		#region ValidationInstructionType

		public void TestValidationInstructionType()
		{
			var dateAndReferences = new DateAndReferenceCollection();
			var dateAndReference1 = dateAndReferences.AddNew();
			var dateAndReference2 = dateAndReferences.AddNew();
			dateAndReference1.Code = "PIC";
			dateAndReference1.InstructionType = InstructionTypes.Codes.Delivery;
			dateAndReference2.Code = "PIC";
			dateAndReference2.InstructionType = DatesAndReference.Any;
			dateAndReference1.ValidateInstructionType();
			AssertNoErrors(dateAndReference1.InstructionTypeInfo);

			dateAndReference2.InstructionType = InstructionTypes.Codes.Delivery;
			dateAndReference1.ValidateInstructionType();
			AssertHasError(dateAndReference1.InstructionTypeInfo, "Code with Booking Direction, Instruction Type, Organization Type and Mode must be unique.");
		}

		#endregion

		#region ValidationOrganisationType

		public void TestValidationOrganisationType()
		{
			var dateAndReferences = new DateAndReferenceCollection();
			var dateAndReference1 = dateAndReferences.AddNew();
			var dateAndReference2 = dateAndReferences.AddNew();
			dateAndReference1.Code = "PIC";
			dateAndReference1.OrganisationType = "CFS";
			dateAndReference2.Code = "PIC";
			dateAndReference2.OrganisationType = DatesAndReference.Any;
			dateAndReference1.ValidateOrganisationType();
			AssertNoErrors(dateAndReference1.OrganisationTypeInfo);

			dateAndReference2.OrganisationType = "CFS";
			dateAndReference1.ValidateOrganisationType();
			AssertHasError(dateAndReference1.OrganisationTypeInfo, "Code with Booking Direction, Instruction Type, Organization Type and Mode must be unique.");
		}

		#endregion

		#region ValidationContainerMode

		public void TestValidationContainerMode()
		{
			var dateAndReferences = new DateAndReferenceCollection();
			var dateAndReference1 = dateAndReferences.AddNew();
			var dateAndReference2 = dateAndReferences.AddNew();
			dateAndReference1.Code = "PIC";
			dateAndReference1.OrganisationType = "CFS";
			dateAndReference1.ContainerMode = "LSE";
			dateAndReference2.Code = "PIC";
			dateAndReference2.OrganisationType = "CFS";
			dateAndReference2.ContainerMode = DatesAndReference.Any;
			dateAndReference1.ValidateContainerMode();
			AssertNoErrors(dateAndReference1.ContainerModeInfo);

			dateAndReference2.ContainerMode = "LSE";
			dateAndReference1.ValidateContainerMode();
			AssertHasError(dateAndReference1.ContainerModeInfo, "Code with Booking Direction, Instruction Type, Organization Type and Mode must be unique.");
		}

		#endregion

		#region ValidationAllowActualDate

		public void TestValidationAllowActualDate()
		{
			var dateAndReference = new DateAndReference();
			AssertEquals(false, dateAndReference.AllowActualDate);
			AssertNoErrors(dateAndReference.AllowActualDateInfo);

			dateAndReference.ValidateAllowFields(dateAndReference.AllowActualDateInfo);
			AssertHasError(dateAndReference.AllowActualDateInfo, "At least one attribute must be editable.");

			dateAndReference.AllowActualDate = true;
			AssertNoErrors(dateAndReference.AllowActualDateInfo);
		}

		#endregion

		#region ValidationAllowEstimatedDate

		public void TestValidationAllowEstimatedDate()
		{
			var dateAndReference = new DateAndReference();
			AssertEquals(false, dateAndReference.AllowEstimatedDate);
			AssertNoErrors(dateAndReference.AllowEstimatedDateInfo);

			dateAndReference.ValidateAllowFields(dateAndReference.AllowEstimatedDateInfo);
			AssertHasError(dateAndReference.AllowEstimatedDateInfo, "At least one attribute must be editable.");

			dateAndReference.AllowEstimatedDate = true;
			AssertNoErrors(dateAndReference.AllowEstimatedDateInfo);
		}

		#endregion

		#region ValidationAllowSlotDate

		public void TestValidationAllowSlotDate()
		{
			var dateAndReference = new DateAndReference();
			AssertEquals(false, dateAndReference.AllowSlotDate);
			AssertNoErrors(dateAndReference.AllowSlotDateInfo);

			dateAndReference.ValidateAllowFields(dateAndReference.AllowSlotDateInfo);
			AssertHasError(dateAndReference.AllowSlotDateInfo, "At least one attribute must be editable.");

			dateAndReference.AllowSlotDate = true;
			AssertNoErrors(dateAndReference.AllowSlotDateInfo);
		}

		#endregion

		#region ValidationAllowRequiredFromDate

		public void TestValidationAllowRequiredFromDate()
		{
			var dateAndReference = new DateAndReference();
			AssertEquals(false, dateAndReference.AllowRequiredFromDate);
			AssertNoErrors(dateAndReference.AllowRequiredFromDateInfo);

			dateAndReference.ValidateAllowFields(dateAndReference.AllowRequiredFromDateInfo);
			AssertHasError(dateAndReference.AllowRequiredFromDateInfo, "At least one attribute must be editable.");

			dateAndReference.AllowRequiredFromDate = true;
			AssertNoErrors(dateAndReference.AllowRequiredFromDateInfo);
		}

		#endregion

		#region ValidationAllowRequiredToDate

		public void TestValidationAllowRequiredToDate()
		{
			var dateAndReference = new DateAndReference();
			AssertEquals(false, dateAndReference.AllowRequiredToDate);
			AssertNoErrors(dateAndReference.AllowRequiredToDateInfo);

			dateAndReference.ValidateAllowFields(dateAndReference.AllowRequiredToDateInfo);
			AssertHasError(dateAndReference.AllowRequiredToDateInfo, "At least one attribute must be editable.");

			dateAndReference.AllowRequiredToDate = true;
			AssertNoErrors(dateAndReference.AllowRequiredToDateInfo);
		}

		#endregion

		#region ValidationAllowReference

		public void TestValidationAllowReference()
		{
			var dateAndReference = new DateAndReference();
			AssertEquals(false, dateAndReference.AllowReference);
			AssertNoErrors(dateAndReference.AllowReferenceInfo);

			dateAndReference.ValidateAllowFields(dateAndReference.AllowReferenceInfo);
			AssertHasError(dateAndReference.AllowReferenceInfo, "At least one attribute must be editable.");

			dateAndReference.AllowReference = true;
			AssertNoErrors(dateAndReference.AllowReferenceInfo);
		}

		#endregion

		#region ValidationAllowReceivedBy

		public void TestValidationAllowReceivedBy()
		{
			var dateAndReference = new DateAndReference();
			AssertEquals(false, dateAndReference.AllowReceivedBy);
			AssertNoErrors(dateAndReference.AllowReceivedByInfo);

			dateAndReference.ValidateAllowFields(dateAndReference.AllowReceivedByInfo);
			AssertHasError(dateAndReference.AllowReceivedByInfo, "At least one attribute must be editable.");

			dateAndReference.AllowReceivedBy = true;
			AssertNoErrors(dateAndReference.AllowReceivedByInfo);
		}

		#endregion

		#region TestRunPreSaveValidation

		public void TestRunPreSaveValidation()
		{
			var dateAndReferences = new DateAndReferenceCollection();
			var dateAndReferenceANY = dateAndReferences.AddNew();
			dateAndReferenceANY.Code = "CUS";
			dateAndReferenceANY.Description = (NoResString)"Custom";
			dateAndReferenceANY.AllowActualDate = true;

			var dateAndReference = dateAndReferences.AddNew(); // booking direction, instruction type, organisation code and container mode is set to ANY
			dateAndReference.Code = "CUS";
			dateAndReference.Description = (NoResString)"Custom";
			dateAndReference.RunPreSaveValidation();
			AssertHasError(dateAndReference.CodeInfo, "Code with Booking Direction, Instruction Type, Organization Type and Mode must be unique.");
			AssertNoErrors(dateAndReference.DescriptionInfo);
			AssertHasError(dateAndReference.BookingDirectionInfo, "Code with Booking Direction, Instruction Type, Organization Type and Mode must be unique.");
			AssertHasError(dateAndReference.InstructionTypeInfo, "Code with Booking Direction, Instruction Type, Organization Type and Mode must be unique.");
			AssertHasError(dateAndReference.OrganisationTypeInfo, "Code with Booking Direction, Instruction Type, Organization Type and Mode must be unique.");
			AssertHasError(dateAndReference.ContainerModeInfo, "Code with Booking Direction, Instruction Type, Organization Type and Mode must be unique.");
			AssertHasError(dateAndReference.AllowActualDateInfo, "At least one attribute must be editable.");
			AssertHasError(dateAndReference.AllowEstimatedDateInfo, "At least one attribute must be editable.");
			AssertHasError(dateAndReference.AllowSlotDateInfo, "At least one attribute must be editable.");
			AssertHasError(dateAndReference.AllowRequiredFromDateInfo, "At least one attribute must be editable.");
			AssertHasError(dateAndReference.AllowRequiredToDateInfo, "At least one attribute must be editable.");
			AssertHasError(dateAndReference.AllowReferenceInfo, "At least one attribute must be editable.");
			AssertHasError(dateAndReference.AllowReceivedByInfo, "At least one attribute must be editable.");

			dateAndReference.BookingDirection = Constants.CartageDirection.Import;
			dateAndReference.RunPreSaveValidation();
			AssertNoErrors(dateAndReference.CodeInfo);
			AssertNoErrors(dateAndReference.DescriptionInfo);
			AssertNoErrors(dateAndReference.BookingDirectionInfo);
			AssertNoErrors(dateAndReference.InstructionTypeInfo);
			AssertNoErrors(dateAndReference.OrganisationTypeInfo);
			AssertNoErrors(dateAndReference.ContainerModeInfo);
			AssertHasError(dateAndReference.AllowActualDateInfo, "At least one attribute must be editable.");
			AssertHasError(dateAndReference.AllowEstimatedDateInfo, "At least one attribute must be editable.");
			AssertHasError(dateAndReference.AllowSlotDateInfo, "At least one attribute must be editable.");
			AssertHasError(dateAndReference.AllowRequiredFromDateInfo, "At least one attribute must be editable.");
			AssertHasError(dateAndReference.AllowRequiredToDateInfo, "At least one attribute must be editable.");
			AssertHasError(dateAndReference.AllowReferenceInfo, "At least one attribute must be editable.");
			AssertHasError(dateAndReference.AllowReceivedByInfo, "At least one attribute must be editable.");

			dateAndReference.AllowActualDate = true;
			dateAndReference.RunPreSaveValidation();
			AssertNoErrors(dateAndReference.CodeInfo);
			AssertNoErrors(dateAndReference.DescriptionInfo);
			AssertNoErrors(dateAndReference.BookingDirectionInfo);
			AssertNoErrors(dateAndReference.InstructionTypeInfo);
			AssertNoErrors(dateAndReference.OrganisationTypeInfo);
			AssertNoErrors(dateAndReference.ContainerModeInfo);
			AssertNoErrors(dateAndReference.AllowActualDateInfo);
			AssertNoErrors(dateAndReference.AllowEstimatedDateInfo);
			AssertNoErrors(dateAndReference.AllowSlotDateInfo);
			AssertNoErrors(dateAndReference.AllowRequiredFromDateInfo);
			AssertNoErrors(dateAndReference.AllowRequiredToDateInfo);
			AssertNoErrors(dateAndReference.AllowReferenceInfo);
			AssertNoErrors(dateAndReference.AllowReceivedByInfo);
		}

		#endregion

		#endregion

		#region Lists

		#region TestBookingDirections

		public void TestBookingDirections()
		{
			var expected = new CodeDescriptionPairList();
			expected.AddPair("ANY", "Applies to ANY Booking Direction, unless overridden.");

			foreach (CodeDescriptionPair direction in new Directions().List)
			{
				expected.AddPair(direction.Code, string.Format("Applies to '{0}' Only", direction.Description));
			}

			AssertContainsExactElementsInAnyOrder(expected, BizObj.BookingDirections);
		}

		#endregion

		#region TestInstructionTypes

		public void TestInstructionTypes()
		{
			var dateAndReferences = new DateAndReferenceCollection();
			var dateAndReference = dateAndReferences.AddNew();

			var expectedInstructionTypes = new CodeDescriptionPairList();
			expectedInstructionTypes.AddPair(DatesAndReference.Any, "Applies to ANY Instruction Types, unless overridden.");

			foreach (CodeDescriptionPair instructionType in new InstructionTypes().List)
			{
				expectedInstructionTypes.AddPair(instructionType.Code, string.Format("Applies to '{0}' Only", instructionType.Description));
			}

			AssertContainsExactElementsInAnyOrder(expectedInstructionTypes, dateAndReference.InstructionTypes);
		}

		#endregion

		#region TestOrganisationType

		public void TestOrganisationTypes()
		{
			var dateAndReferences = new DateAndReferenceCollection();
			var dateAndReference = dateAndReferences.AddNew();

			var expected = new CodeDescriptionPairList();
			expected.AddPair(DatesAndReference.Any, "Applies to ANY Organization Types, unless overridden.");

			foreach (CodeDescriptionPair orgType in OrganisationTypesList.Instance)
			{
				expected.AddPair(orgType.Code, string.Format("Applies to '{0}' Only", orgType.Description));
			}

			AssertContainsExactElementsInAnyOrder(expected, dateAndReference.OrganisationTypes);
		}

		#endregion

		#region TestContainerModes

		public void TestContainerModes()
		{
			var expected = new CodeDescriptionPairList();
			expected.AddPair(DatesAndReference.Any, "Applies to ANY Container Modes, unless overridden.");
			expected.AddPair(Constants.CartageContainerMode.Loose, string.Format("Applies to Container Mode of '{0}' Only.", Constants.CartageContainerModeDescription.Loose));
			expected.AddPair(Constants.CartageContainerMode.Containerized, string.Format("Applies to Container Mode of '{0}' Only.", Constants.CartageContainerModeDescription.Containerized));

			AssertContainsExactElementsInAnyOrder(expected, BizObj.ContainerModes);
		}

		#endregion

		#endregion

		#region ReadOnly

		public void TestAllowRequiredFromLabel_ReadOnly()
		{
			BizObj.AllowRequiredFromDate = true;
			AssertEquals("Read Only should be false", false, BizObj.AllowRequiredFromLabelInfo.ReadOnly);

			BizObj.AllowRequiredFromDate = false;
			AssertEquals("Read only should be true", true, BizObj.AllowRequiredFromLabelInfo.ReadOnly);
		}

		public void TestAllowRequiredToLabel_ReadOnly()
		{
			BizObj.AllowRequiredToDate = true;
			AssertEquals("Read Only should be false.", false, BizObj.AllowRequiredToLabelInfo.ReadOnly);

			BizObj.AllowRequiredToDate = false;
			AssertEquals("Read Only should be true.", true, BizObj.AllowRequiredToLabelInfo.ReadOnly);
		}

		#endregion

		#region Implementation

		protected override DateAndReference GetBusinessObjectToClone()
		{
			return new DateAndReference();
		}

		protected override DateAndReference GetBusinessObjectToSerialise()
		{
			return new DateAndReference();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected new DateAndReference BizObj
		{
			get { return base.BizObj; }
		}

		#endregion
	}
}
