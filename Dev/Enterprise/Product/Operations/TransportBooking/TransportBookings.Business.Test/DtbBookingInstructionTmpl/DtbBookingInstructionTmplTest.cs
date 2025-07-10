using System;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Business.Testing;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Business.Testing
{
	[TestedType(typeof(DtbBookingInstructionTmpl))]
	public class DtbBookingInstructionTmplBizOTest : DtbTransportBusinessObjectTestCase
	{
		protected override Type ExpectedLookupsType
		{
			get { return typeof(DtbBookingInstructionTmplLookups); }
		}

		protected override Type ExpectedValidationType
		{
			get { return typeof(DtbBookingInstructionTmplValidation); }
		}

		public void TestTemplate()
		{
			var template = Factory.New<DtbBookingTmpl>();
			var instructionTemplate = (DtbBookingInstructionTmpl)GetNewBusinessObject();
			instructionTemplate.K2_KT_BookingTmpl = template.PK;
			AssertEquals(template, instructionTemplate.Template);
		}
	}

	public class DtbBookingInstructionTmplTest : DtbBookingTestCaseWithFactory
	{
		public void TestIsSystem()
		{
			var template = Helper.CreateTransportBookingTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import);
			var instruction = Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CTO, InstructionTypes.Codes.PickUp);
			AssertEquals(false, instruction.ReadOnly);

			template.KT_IsSystem = true;
			AssertEquals(true, instruction.IsSystem);
		}

		public void TestCanDelete()
		{
			var template = Helper.CreateTransportBookingTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import);
			var instruction = template.Instructions.AddNew();
			AssertEquals(true, instruction.CanDelete);

			template.KT_IsSystem = true;
			AssertEquals(false, instruction.CanDelete);
			AssertEquals("This Instruction cannot be deleted because the Transport Booking Template is System Defined.", instruction.ReasonForNotAbleToDelete);

			AssertEquals("Standalone Instruction", true, Factory.New<DtbBookingInstructionTmpl>().CanDelete);
		}

		public void TestReadOnly()
		{
			var template = Helper.CreateTransportBookingTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import);
			var instruction = template.Instructions.AddNew();
			AssertEquals(false, instruction.ReadOnly);
			AssertEquals(false, instruction.K2_SequenceInfo.ReadOnly);
			AssertEquals(false, instruction.K2_OrgTypeInfo.ReadOnly);
			AssertEquals(false, instruction.K2_InstructionTypeInfo.ReadOnly);
			AssertEquals(false, instruction.K2_DropModeInfo.ReadOnly);
			AssertEquals(false, instruction.K2_IsContainerRateableInfo.ReadOnly);
			AssertEquals(false, instruction.K2_IsLooseRateableInfo.ReadOnly);
			AssertEquals(false, instruction.K2_PackageTypeInfo.ReadOnly);

			template.KT_IsSystem = true;
			AssertEquals(false, instruction.ReadOnly);
			AssertEquals(true, instruction.K2_SequenceInfo.ReadOnly);
			AssertEquals(true, instruction.K2_OrgTypeInfo.ReadOnly);
			AssertEquals(true, instruction.K2_InstructionTypeInfo.ReadOnly);
			AssertEquals(false, instruction.K2_DropModeInfo.ReadOnly);
			AssertEquals(true, instruction.K2_IsContainerRateableInfo.ReadOnly);
			AssertEquals(true, instruction.K2_IsLooseRateableInfo.ReadOnly);
			AssertEquals(true, instruction.K2_PackageTypeInfo.ReadOnly);

			var standalone = Factory.New<DtbBookingInstructionTmpl>();
			AssertEquals("Standalone Instruction", false, standalone.ReadOnly);
			AssertEquals(false, standalone.ReadOnly);
			AssertEquals(false, standalone.K2_SequenceInfo.ReadOnly);
			AssertEquals(false, standalone.K2_OrgTypeInfo.ReadOnly);
			AssertEquals(false, standalone.K2_InstructionTypeInfo.ReadOnly);
			AssertEquals(false, standalone.K2_DropModeInfo.ReadOnly);
			AssertEquals(false, standalone.K2_IsContainerRateableInfo.ReadOnly);
			AssertEquals(false, standalone.K2_IsLooseRateableInfo.ReadOnly);
			AssertEquals(false, standalone.K2_PackageTypeInfo.ReadOnly);
		}

		public void TestFetchStrategy()
		{
			var template = Helper.CreateTransportBookingTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import);
			var instruction = template.Instructions.AddNew();
			AssertNotNull(instruction.FetchStrategy);
			AssertEquals(typeof(DtbBookingInstructionTmplFetchStrategy), instruction.FetchStrategy.GetType());
		}

		public void TestGetBusinessObjectBaseTypeFromTablePrefix()
		{
			var prefix = DtbBookingInstructionTmplSchema.Constants.Prefix;
			var businessObjectType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(prefix, false);
			AssertEquals("Should supply correct type based on the table prefix", typeof(DtbBookingInstructionTmpl), businessObjectType);
			AssertNotEquals("Should not supply deprecated abstract type", typeof(DtbTransportInstructionTmpl), businessObjectType);
		}

		// No interface called IDtbTransportInstructionTmpl, therefore no need for TestGetTypeFromObjectFactory or TestCreateFromInterface
	}
}
