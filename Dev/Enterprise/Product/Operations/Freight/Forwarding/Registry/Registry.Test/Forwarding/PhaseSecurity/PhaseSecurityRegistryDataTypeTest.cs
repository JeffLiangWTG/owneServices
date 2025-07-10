using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Registry.Testing
{
	[TestedType(typeof(PhaseSecurityRegistryDataType))]
	class PhaseSecurityRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<PhaseSecurityRegistryDataType>
	{
		public void TestStringsAreInternedDuringDeserialisation()
		{
			var locations = new CodeDescriptionPairList();
			locations.AddPair("Sydney", "AU");
			locations.AddPair("Auckland", "NZ");

			var phaseSecurity = new PhaseSecurity(locations);

			var phaseA = phaseSecurity.Phases.AddNew();
			phaseA.Code = "AAA";
			phaseA.Description = (NoResString)"AAA description";

			var ruleA1 = phaseA.Rules.AddNew();
			ruleA1.DepartmentPK = ZGuid.Empty;
			ruleA1.Location = "Sydney";

			Action<PhaseRule, string> addDependant = (rule, dependantName) =>
			{
				var dependant = rule.Dependants.AddNew();
				dependant.DependantType = PhaseConstants.DependantType.Property;
				dependant.Name = dependantName;
				dependant.Description = dependantName + "Description";
				dependant.IsReadOnly = true;
			};

			addDependant(ruleA1, "First");
			addDependant(ruleA1, "Second");
			addDependant(ruleA1, "Third");

			var ruleA2 = phaseA.Rules.AddNew();
			ruleA2.DepartmentPK = ZGuid.Empty;
			ruleA2.Location = "Auckland";

			addDependant(ruleA2, "First");

			var phaseB = phaseSecurity.Phases.AddNew();
			phaseB.Code = "BBB";
			phaseB.Description = (NoResString)"BBB description";

			var ruleB1 = phaseB.Rules.AddNew();
			ruleB1.DepartmentPK = ZGuid.Empty;
			ruleB1.Location = "Sydney";

			addDependant(ruleB1, "First");
			addDependant(ruleB1, "Second");
			addDependant(ruleB1, "Random");

			var dataType = new PhaseSecurityRegistryDataType();

			byte[] bytes = dataType.Serialise(phaseSecurity);
			var deserialisedRegistryItem = dataType.Deserialise(bytes);

			var dependantsA1 = deserialisedRegistryItem.Phases.Cast<Phase>().First(phase => phase.Code == "AAA")
				.Rules.Cast<PhaseRule>().First(rule => rule.Location == "Sydney")
				.ReadOnlyDependants.Cast<PhaseDependant>();

			AssertContainsExactElementsInAnyOrder(new ZString[] { "First", "Second", "Third" }, dependantsA1.Select(d => d.Name));

			var dependantsA2 = deserialisedRegistryItem.Phases.Cast<Phase>().First(phase => phase.Code == "AAA")
				.Rules.Cast<PhaseRule>().First(rule => rule.Location == "Auckland")
				.ReadOnlyDependants.Cast<PhaseDependant>();

			AssertContainsExactElementsInAnyOrder(new ZString[] { "First" }, dependantsA2.Select(d => d.Name));

			var dependantsB1 = deserialisedRegistryItem.Phases.Cast<Phase>().First(phase => phase.Code == "BBB")
				.Rules.Cast<PhaseRule>().First(rule => rule.Location == "Sydney")
				.ReadOnlyDependants.Cast<PhaseDependant>();

			AssertContainsExactElementsInAnyOrder(new ZString[] { "First", "Second", "Random" }, dependantsB1.Select(d => d.Name));

			Action<PhaseDependant, PhaseDependant> assertStringsAreInterned = (dependant1, dependant2) =>
			{
				AssertEquals("Prerequisite", dependant1.Name, dependant2.Name);

				Assert(ReferenceEquals(dependant1.DependantType.ToString(), dependant2.DependantType.ToString()));
				Assert(ReferenceEquals(dependant1.Name.ToString(), dependant2.Name.ToString()));
				Assert(ReferenceEquals(dependant1.Description.ToString(), dependant2.Description.ToString()));
			};

			CombineAssertions("All identical strings should be interned", () =>
				{
					var a1_First = dependantsA1.First(d => d.Name == "First");
					var a1_Second = dependantsA1.First(d => d.Name == "Second");

					var a2_First = dependantsA2.First(d => d.Name == "First");

					var b1_First = dependantsB1.First(d => d.Name == "First");
					var b1_Second = dependantsB1.First(d => d.Name == "Second");

					assertStringsAreInterned(a1_First, a2_First);
					assertStringsAreInterned(a1_First, b1_First);
					assertStringsAreInterned(a1_Second, b1_Second);
				});
		}

		#region Implementation

		protected override PhaseSecurityRegistryDataType GetNewDataType()
		{
			return new PhaseSecurityRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "PhaseSecurityRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var locations = new CodeDescriptionPairList();
			locations.AddPair("Chisinau", "The capital of");
			locations.AddPair("Moldova", "Republic");
			locations.AddPair("Sydney", "Australia");

			var phaseSecurity = new PhaseSecurity(locations);
			phaseSecurity.IsEnabled = true;

			var phase1 = phaseSecurity.Phases.AddNew();
			phase1.Code = "AAA";
			phase1.Description = (NoResString)"AAA description";

			var rule11 = phase1.Rules.AddNew();
			rule11.DepartmentPK = GlbDepartment.CurrentDepartment.PK;
			rule11.Location = "Chisinau";

			var rule12 = phase1.Rules.AddNew();
			rule12.DepartmentPK = ZGuid.Empty;
			rule12.Location = "Moldova";

			var phase2 = phaseSecurity.Phases.AddNew();
			phase2.Code = "BBB";
			phase2.Description = (NoResString)"BBB description";

			var rule21 = phase2.Rules.AddNew();
			rule21.DepartmentPK = GlbDepartment.CurrentDepartment.PK;
			rule21.Location = "Sydney";

			var dependant1 = rule21.Dependants.AddNew();
			dependant1.DependantType = PhaseConstants.DependantType.Property;
			dependant1.Name = "SookieStackhouse";
			dependant1.Description = "Telepathic Fairy Waitress";
			dependant1.IsReadOnly = true;

			var dependant2 = rule21.Dependants.AddNew();
			dependant2.DependantType = PhaseConstants.DependantType.TypeName;
			dependant2.Name = "EricNorthman";
			dependant2.Description = "Viking Vampire";
			dependant2.IsMandatory = true;

			#region ByteArrayValue

			byte[] byteArrayValue = new byte[]
{
60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,
0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,80,0,104,0,97,0,115,0,101,0,83,0,101,0,99,0,117,0,114,0,105,0,116,0,121,0,62,0,60,0,73,0,115,0,69,0,110,0,97,0,98,0,108,0,101,0,100,0,62,0,89,
0,60,0,47,0,73,0,115,0,69,0,110,0,97,0,98,0,108,0,101,0,100,0,62,0,60,0,80,0,104,0,97,0,115,0,101,0,115,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,105,0,61,0,34,0,104,0,116,
0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,45,0,105,0,110,
0,115,0,116,0,97,0,110,0,99,0,101,0,34,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,100,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,
0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,34,0,62,0,60,0,80,0,104,0,97,0,115,0,101,0,62,0,60,0,67,0,111,0,100,0,101,0,77,
0,97,0,120,0,76,0,101,0,110,0,103,0,116,0,104,0,62,0,51,0,60,0,47,0,67,0,111,0,100,0,101,0,77,0,97,0,120,0,76,0,101,0,110,0,103,0,116,0,104,0,62,0,60,0,67,0,111,0,100,0,101,0,62,0,65,
0,65,0,65,0,60,0,47,0,67,0,111,0,100,0,101,0,62,0,60,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,65,0,65,0,65,0,32,0,100,0,101,0,115,0,99,0,114,0,105,0,112,
0,116,0,105,0,111,0,110,0,60,0,47,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,60,0,82,0,117,0,108,0,101,0,115,0,62,0,60,0,80,0,104,0,97,0,115,0,101,0,82,0,117,
0,108,0,101,0,62,0,60,0,68,0,101,0,112,0,97,0,114,0,116,0,109,0,101,0,110,0,116,0,80,0,75,0,62,0,56,0,54,0,98,0,98,0,49,0,99,0,50,0,50,0,45,0,48,0,56,0,54,0,53,0,45,0,52,0,54,
0,56,0,53,0,45,0,57,0,57,0,54,0,101,0,45,0,100,0,53,0,54,0,99,0,98,0,100,0,49,0,51,0,54,0,52,0,57,0,49,0,60,0,47,0,68,0,101,0,112,0,97,0,114,0,116,0,109,0,101,0,110,0,116,0,80,
0,75,0,62,0,60,0,76,0,111,0,99,0,97,0,116,0,105,0,111,0,110,0,62,0,67,0,104,0,105,0,115,0,105,0,110,0,97,0,117,0,60,0,47,0,76,0,111,0,99,0,97,0,116,0,105,0,111,0,110,0,62,0,60,0,80,
0,104,0,97,0,115,0,101,0,68,0,101,0,112,0,101,0,110,0,100,0,97,0,110,0,116,0,115,0,32,0,47,0,62,0,60,0,47,0,80,0,104,0,97,0,115,0,101,0,82,0,117,0,108,0,101,0,62,0,60,0,80,0,104,0,97,
0,115,0,101,0,82,0,117,0,108,0,101,0,62,0,60,0,68,0,101,0,112,0,97,0,114,0,116,0,109,0,101,0,110,0,116,0,80,0,75,0,62,0,48,0,48,0,48,0,48,0,48,0,48,0,48,0,48,0,45,0,48,0,48,0,48,
0,48,0,45,0,48,0,48,0,48,0,48,0,45,0,48,0,48,0,48,0,48,0,45,0,48,0,48,0,48,0,48,0,48,0,48,0,48,0,48,0,48,0,48,0,48,0,48,0,60,0,47,0,68,0,101,0,112,0,97,0,114,0,116,0,109,
0,101,0,110,0,116,0,80,0,75,0,62,0,60,0,76,0,111,0,99,0,97,0,116,0,105,0,111,0,110,0,62,0,77,0,111,0,108,0,100,0,111,0,118,0,97,0,60,0,47,0,76,0,111,0,99,0,97,0,116,0,105,0,111,0,110,
0,62,0,60,0,80,0,104,0,97,0,115,0,101,0,68,0,101,0,112,0,101,0,110,0,100,0,97,0,110,0,116,0,115,0,32,0,47,0,62,0,60,0,47,0,80,0,104,0,97,0,115,0,101,0,82,0,117,0,108,0,101,0,62,0,60,
0,47,0,82,0,117,0,108,0,101,0,115,0,62,0,60,0,47,0,80,0,104,0,97,0,115,0,101,0,62,0,60,0,80,0,104,0,97,0,115,0,101,0,62,0,60,0,67,0,111,0,100,0,101,0,77,0,97,0,120,0,76,0,101,0,110,
0,103,0,116,0,104,0,62,0,51,0,60,0,47,0,67,0,111,0,100,0,101,0,77,0,97,0,120,0,76,0,101,0,110,0,103,0,116,0,104,0,62,0,60,0,67,0,111,0,100,0,101,0,62,0,66,0,66,0,66,0,60,0,47,0,67,
0,111,0,100,0,101,0,62,0,60,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,66,0,66,0,66,0,32,0,100,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,60,
0,47,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,60,0,82,0,117,0,108,0,101,0,115,0,62,0,60,0,80,0,104,0,97,0,115,0,101,0,82,0,117,0,108,0,101,0,62,0,60,0,68,
0,101,0,112,0,97,0,114,0,116,0,109,0,101,0,110,0,116,0,80,0,75,0,62,0,56,0,54,0,98,0,98,0,49,0,99,0,50,0,50,0,45,0,48,0,56,0,54,0,53,0,45,0,52,0,54,0,56,0,53,0,45,0,57,0,57,
0,54,0,101,0,45,0,100,0,53,0,54,0,99,0,98,0,100,0,49,0,51,0,54,0,52,0,57,0,49,0,60,0,47,0,68,0,101,0,112,0,97,0,114,0,116,0,109,0,101,0,110,0,116,0,80,0,75,0,62,0,60,0,76,0,111,
0,99,0,97,0,116,0,105,0,111,0,110,0,62,0,83,0,121,0,100,0,110,0,101,0,121,0,60,0,47,0,76,0,111,0,99,0,97,0,116,0,105,0,111,0,110,0,62,0,60,0,80,0,104,0,97,0,115,0,101,0,68,0,101,0,112,
0,101,0,110,0,100,0,97,0,110,0,116,0,115,0,62,0,60,0,80,0,104,0,97,0,115,0,101,0,68,0,101,0,112,0,101,0,110,0,100,0,97,0,110,0,116,0,62,0,60,0,68,0,101,0,112,0,101,0,110,0,100,0,97,0,110,
0,116,0,84,0,121,0,112,0,101,0,62,0,80,0,82,0,79,0,60,0,47,0,68,0,101,0,112,0,101,0,110,0,100,0,97,0,110,0,116,0,84,0,121,0,112,0,101,0,62,0,60,0,78,0,97,0,109,0,101,0,62,0,83,0,111,
0,111,0,107,0,105,0,101,0,83,0,116,0,97,0,99,0,107,0,104,0,111,0,117,0,115,0,101,0,60,0,47,0,78,0,97,0,109,0,101,0,62,0,60,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,
0,62,0,84,0,101,0,108,0,101,0,112,0,97,0,116,0,104,0,105,0,99,0,32,0,70,0,97,0,105,0,114,0,121,0,32,0,87,0,97,0,105,0,116,0,114,0,101,0,115,0,115,0,60,0,47,0,68,0,101,0,115,0,99,0,114,
0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,60,0,73,0,115,0,77,0,97,0,110,0,100,0,97,0,116,0,111,0,114,0,121,0,62,0,78,0,60,0,47,0,73,0,115,0,77,0,97,0,110,0,100,0,97,0,116,0,111,0,114,
0,121,0,62,0,60,0,73,0,115,0,82,0,101,0,97,0,100,0,79,0,110,0,108,0,121,0,62,0,89,0,60,0,47,0,73,0,115,0,82,0,101,0,97,0,100,0,79,0,110,0,108,0,121,0,62,0,60,0,47,0,80,0,104,0,97,
0,115,0,101,0,68,0,101,0,112,0,101,0,110,0,100,0,97,0,110,0,116,0,62,0,60,0,80,0,104,0,97,0,115,0,101,0,68,0,101,0,112,0,101,0,110,0,100,0,97,0,110,0,116,0,62,0,60,0,68,0,101,0,112,0,101,
0,110,0,100,0,97,0,110,0,116,0,84,0,121,0,112,0,101,0,62,0,84,0,89,0,80,0,60,0,47,0,68,0,101,0,112,0,101,0,110,0,100,0,97,0,110,0,116,0,84,0,121,0,112,0,101,0,62,0,60,0,78,0,97,0,109,
0,101,0,62,0,69,0,114,0,105,0,99,0,78,0,111,0,114,0,116,0,104,0,109,0,97,0,110,0,60,0,47,0,78,0,97,0,109,0,101,0,62,0,60,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,
0,62,0,86,0,105,0,107,0,105,0,110,0,103,0,32,0,86,0,97,0,109,0,112,0,105,0,114,0,101,0,60,0,47,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,60,0,73,0,115,0,77,
0,97,0,110,0,100,0,97,0,116,0,111,0,114,0,121,0,62,0,89,0,60,0,47,0,73,0,115,0,77,0,97,0,110,0,100,0,97,0,116,0,111,0,114,0,121,0,62,0,60,0,73,0,115,0,82,0,101,0,97,0,100,0,79,0,110,
0,108,0,121,0,62,0,78,0,60,0,47,0,73,0,115,0,82,0,101,0,97,0,100,0,79,0,110,0,108,0,121,0,62,0,60,0,47,0,80,0,104,0,97,0,115,0,101,0,68,0,101,0,112,0,101,0,110,0,100,0,97,0,110,0,116,
0,62,0,60,0,47,0,80,0,104,0,97,0,115,0,101,0,68,0,101,0,112,0,101,0,110,0,100,0,97,0,110,0,116,0,115,0,62,0,60,0,47,0,80,0,104,0,97,0,115,0,101,0,82,0,117,0,108,0,101,0,62,0,60,0,47,
0,82,0,117,0,108,0,101,0,115,0,62,0,60,0,47,0,80,0,104,0,97,0,115,0,101,0,62,0,60,0,47,0,80,0,104,0,97,0,115,0,101,0,115,0,62,0,60,0,47,0,80,0,104,0,97,0,115,0,101,0,83,0,101,0,99,
0,117,0,114,0,105,0,116,0,121,0,62,0
};

			#endregion

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(phaseSecurity, byteArrayValue)
			};
		}

		#endregion
	}
}
