using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(EntryProcessingPortsMapping))]
	sealed class EntryProcessingPortsMappingTest : RegistryBusinessObjectTemplateTestCase<EntryProcessingPortsMapping>
	{
		public void TestValidateEntryPort()
		{
			EntryProcessingPortsMappingCollection coll = new EntryProcessingPortsMappingCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
			EntryProcessingPortsMapping mapping = coll.AddNew();
			mapping.EntryPort = "";
			AssertHasErrorContaining(mapping.EntryPortInfo, "Please enter");
			mapping.EntryPort = "9980";
			AssertNoErrorContaining(mapping.EntryPortInfo, "Please enter");
			AssertHasErrorContaining(mapping.EntryPortInfo, ListValidation.InvalidCodeError);
			mapping.EntryPort = "3901";
			AssertNoErrorContaining(mapping.EntryPortInfo, ListValidation.InvalidCodeError);
			mapping = coll.AddNew();
			mapping.EntryPort = "3901";
			AssertHasError(mapping.EntryPortInfo, EntryProcessingPortsMapping.DuplicateEntryPort);
			mapping.EntryPort = "3902";
			AssertNoError(mapping.EntryPortInfo, EntryProcessingPortsMapping.DuplicateEntryPort);
		}

		public void TestValidateProcessingPort()
		{
			EntryProcessingPortsMappingCollection coll = new EntryProcessingPortsMappingCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
			EntryProcessingPortsMapping mapping = coll.AddNew();
			mapping.ProcessingPort = "";
			AssertHasErrorContaining(mapping.ProcessingPortInfo, "Please enter");
			mapping.ProcessingPort = "9980";
			AssertNoErrorContaining(mapping.ProcessingPortInfo, "Please enter");
			AssertHasErrorContaining(mapping.ProcessingPortInfo, ListValidation.InvalidCodeError);
			mapping.EntryPort = "1111";
			mapping.ProcessingPort = "3901";
			AssertNoErrorContaining(mapping.ProcessingPortInfo, ListValidation.InvalidCodeError);
			AssertHasError(mapping.ProcessingPortInfo, EntryProcessingPortsMapping.EntryPortPrcessingPortShouldBeInTheSameDistrict);
			mapping.ProcessingPort = "1100";
			AssertNoError(mapping.ProcessingPortInfo, EntryProcessingPortsMapping.EntryPortPrcessingPortShouldBeInTheSameDistrict);
			mapping.EntryPort = "1001";
			mapping.ProcessingPort = "4601";
			AssertNoError(mapping.ProcessingPortInfo, EntryProcessingPortsMapping.EntryPortPrcessingPortShouldBeInTheSameDistrict);
			mapping.ProcessingPort = "4701";
			AssertNoError(mapping.ProcessingPortInfo, EntryProcessingPortsMapping.EntryPortPrcessingPortShouldBeInTheSameDistrict);
			mapping.ProcessingPort = "1001";
			AssertNoError(mapping.ProcessingPortInfo, EntryProcessingPortsMapping.EntryPortPrcessingPortShouldBeInTheSameDistrict);
			mapping.EntryPort = "4701";
			mapping.ProcessingPort = "4601";
			AssertNoError(mapping.ProcessingPortInfo, EntryProcessingPortsMapping.EntryPortPrcessingPortShouldBeInTheSameDistrict);
			mapping.ProcessingPort = "4701";
			AssertNoError(mapping.ProcessingPortInfo, EntryProcessingPortsMapping.EntryPortPrcessingPortShouldBeInTheSameDistrict);
			mapping.ProcessingPort = "1001";
			AssertNoError(mapping.ProcessingPortInfo, EntryProcessingPortsMapping.EntryPortPrcessingPortShouldBeInTheSameDistrict);
		}

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override BusinessObject GetNewBusinessObject()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "3901", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "3902", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			EntryProcessingPortsMappingCollection coll = new EntryProcessingPortsMappingCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
			EntryProcessingPortsMapping result = coll.AddNew();
			result.EntryPort = "3901";
			result.ProcessingPort = "3902";
			return result;
		}

		protected override EntryProcessingPortsMapping GetBusinessObjectToClone() => (EntryProcessingPortsMapping)GetNewBusinessObject();

		protected override EntryProcessingPortsMapping GetBusinessObjectToSerialise() => GetBusinessObjectToClone();
	}
}
