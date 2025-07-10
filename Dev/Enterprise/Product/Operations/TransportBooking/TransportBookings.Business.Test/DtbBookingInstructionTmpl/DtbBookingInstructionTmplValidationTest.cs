using CargoWise.ComponentModel;
using Enterprise.Core;
using Enterprise.TransportCommon.Business.Testing;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.TransportBookings.Business.Testing
{
	class DtbBookingInstructionTmplValidationTest : DtbTransportInstructionTmplValidationTest
	{
		public void TestK2_OrgType()
		{
			var template = Helper.CreateTransportBookingTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import);
			var instruction = template.Instructions.AddNew();
			instruction.Validation.ValidateK2_OrgType();
			AssertHasError(instruction.K2_OrgTypeInfo, "Please enter an Organization Type.");

			instruction.K2_OrgType = "XXX";
			AssertHasError(instruction.K2_OrgTypeInfo, "Enter a valid Organization Type.");

			instruction.K2_OrgType = "CTO";
			AssertNoErrors(instruction.K2_OrgTypeInfo);
		}

		public void TestK2_InstructionType()
		{
			var template = Helper.CreateTransportBookingTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import);
			var instruction = template.Instructions.AddNew();
			instruction.Validation.ValidateK2_InstructionType();
			AssertHasError(instruction.K2_InstructionTypeInfo, "Please enter an Instruction Type.");

			instruction.K2_InstructionType = "XXX";
			AssertHasError(instruction.K2_InstructionTypeInfo, "Enter a valid Instruction Type.");

			instruction.K2_InstructionType = "PIC";
			AssertNoErrors(instruction.K2_InstructionTypeInfo);
		}

		public void TestK2_Sequence()
		{
			var template = Helper.CreateTransportBookingTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import);
			var instruction1 = template.Instructions.AddNew();
			var instruction2 = template.Instructions.AddNew();
			var instruction3 = template.Instructions.AddNew();
			AssertNoErrors(instruction1.K2_SequenceInfo);
			AssertNoErrors(instruction2.K2_SequenceInfo);
			AssertNoErrors(instruction3.K2_SequenceInfo);

			instruction1.K2_Sequence = 0;
			AssertHasError(instruction1.K2_SequenceInfo, "The first Instruction should have a sequence number of 1.");

			instruction1.K2_Sequence = 1;
			instruction3.K2_Sequence = 4;
			AssertNoErrors(instruction1.K2_SequenceInfo);
			AssertHasError(instruction3.K2_SequenceInfo, "Sequence must be sequential integers, continuously increasing by one.");

			instruction3.K2_Sequence = 3;
			AssertNoErrors(instruction1.K2_SequenceInfo);
			AssertNoErrors(instruction2.K2_SequenceInfo);
			AssertNoErrors(instruction3.K2_SequenceInfo);
		}

		public void TestK2_DropMode()
		{
			var template = Helper.CreateTransportBookingTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import);
			var instruction = template.Instructions.AddNew();
			AssertNoErrors(instruction.K2_DropModeInfo);

			instruction.K2_DropMode = "XXX";
			AssertHasError(instruction.K2_DropModeInfo, "Enter a valid Drop Mode.");

			instruction.K2_DropMode = "SDL";
			AssertNoErrors(instruction.K2_DropModeInfo);
		}

		public void TestK2_IsContainerRateable()
		{
			var template = Helper.CreateTransportBookingTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import, RatingFreightModes.Codes.Both);
			var picInstruction = template.Instructions.AddNew();
			var dlvInstruction = template.Instructions.AddNew();
			var thirdInstruction = template.Instructions.AddNew();

			picInstruction.K2_IsContainerRateable = true;
			AssertEquals(true, picInstruction.K2_IsContainerRateableInfo.HasErrors());

			dlvInstruction.K2_IsContainerRateable = true;
			AssertEquals(false, picInstruction.K2_IsContainerRateableInfo.HasErrors());
			AssertEquals(false, dlvInstruction.K2_IsContainerRateableInfo.HasErrors());

			thirdInstruction.K2_IsContainerRateable = true;
			AssertEquals(false, picInstruction.K2_IsContainerRateableInfo.HasErrors());
			AssertEquals(false, dlvInstruction.K2_IsContainerRateableInfo.HasErrors());
			AssertEquals(false, dlvInstruction.K2_IsContainerRateableInfo.HasErrors());

			thirdInstruction.K2_IsContainerRateable = false;
			AssertEquals(false, picInstruction.K2_IsContainerRateableInfo.HasErrors());
			AssertEquals(false, dlvInstruction.K2_IsContainerRateableInfo.HasErrors());
			AssertEquals(false, dlvInstruction.K2_IsContainerRateableInfo.HasErrors());
		}

		public void TestK2_IsLooseRateable()
		{
			var template = Helper.CreateTransportBookingTemplate("ILCL", "LCL Import", Constants.CartageDirection.Import, RatingFreightModes.Codes.Both);
			var picInstruction = template.Instructions.AddNew();
			var dlvInstruction = template.Instructions.AddNew();
			var thirdInstruction = template.Instructions.AddNew();

			picInstruction.K2_IsLooseRateable = true;
			AssertEquals(true, picInstruction.K2_IsLooseRateableInfo.HasErrors());

			dlvInstruction.K2_IsLooseRateable = true;
			AssertEquals(false, picInstruction.K2_IsLooseRateableInfo.HasErrors());
			AssertEquals(false, dlvInstruction.K2_IsLooseRateableInfo.HasErrors());

			thirdInstruction.K2_IsLooseRateable = true;
			AssertEquals(false, picInstruction.K2_IsLooseRateableInfo.HasErrors());
			AssertEquals(false, dlvInstruction.K2_IsLooseRateableInfo.HasErrors());
			AssertEquals(false, dlvInstruction.K2_IsLooseRateableInfo.HasErrors());

			thirdInstruction.K2_IsLooseRateable = false;
			AssertEquals(false, picInstruction.K2_IsLooseRateableInfo.HasErrors());
			AssertEquals(false, dlvInstruction.K2_IsLooseRateableInfo.HasErrors());
			AssertEquals(false, dlvInstruction.K2_IsLooseRateableInfo.HasErrors());
		}

		public void TestK2_PackageType()
		{
			var template = Helper.CreateTransportBookingTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import);
			var instruction = template.Instructions.AddNew();
			AssertNoErrors(instruction.K2_PackageTypeInfo);

			instruction.K2_PackageType = "XXX";
			AssertHasError(instruction.K2_PackageTypeInfo, "Enter a valid Package Type.");

			instruction.K2_PackageType = "CNT";
			AssertNoErrors(instruction.K2_PackageTypeInfo);
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}
		TransportBookingTestHelper helper;
	}
}
