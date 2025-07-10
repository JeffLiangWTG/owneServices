using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Registry.Testing
{
	[TestedType(typeof(PhaseRule))]
	public class PhaseRuleTest : RegistryBusinessObjectTemplateTestCase<PhaseRule>
	{
		public void TestValidateUniqueDepartmentAndLocation()
		{
			var errorMessage = "XXX Phase - Same Department and Location have already been set for this Phase Code.";
			var phase = new Phase();
			phase.Code = "XXX";

			var rule1 = phase.Rules.AddNew();
			rule1.DepartmentPK = GlbDepartment.CurrentDepartment.PK;
			rule1.Location = "Any Location";

			var rule2 = phase.Rules.AddNew();
			rule2.DepartmentPK = ZGuid.NewZGuid();
			rule2.Location = "Any Location";

			rule1.RunPreSaveValidation();

			AssertNoError(rule1.DepartmentPKInfo, errorMessage);
			AssertNoError(rule2.DepartmentPKInfo, errorMessage);

			AssertNoError(rule1.LocationInfo, errorMessage);
			AssertNoError(rule2.LocationInfo, errorMessage);

			rule2.DepartmentPK = GlbDepartment.CurrentDepartment.PK;

			rule1.RunPreSaveValidation();
			rule2.RunPreSaveValidation();

			AssertHasError("Duplicate Department+Location", rule1.DepartmentPKInfo, errorMessage);
			AssertHasError("Duplicate Department+Location", rule2.DepartmentPKInfo, errorMessage);

			AssertHasError("Duplicate Department+Location", rule1.LocationInfo, errorMessage);
			AssertHasError("Duplicate Department+Location", rule2.LocationInfo, errorMessage);
		}

		public void TestValidateDepartmentPK()
		{
			PhaseRule rule = new PhaseRule();
			AssertNoErrors("Precondition", rule.DepartmentPKInfo);

			rule.DepartmentPK = ZGuid.NewZGuid();
			AssertHasErrors("Invalid department", rule.DepartmentPKInfo);

			rule.DepartmentPK = Factory.LoadTop1<GlbDepartment>(new ZQuery()).PK;
			AssertNoErrors("Invalid department", rule.DepartmentPKInfo);
		}

		public void TestValidateLocation()
		{
			CodeDescriptionPairList locations = new CodeDescriptionPairList();
			locations.AddPair("AAA", "some location");
			locations.AddPair("BBB", "another location");

			PhaseRule rule = new PhaseSecurity(locations).Phases.AddNew().Rules.AddNew();
			rule.Location = "XXX";
			AssertHasErrors("Invalid location", rule.LocationInfo);

			rule.Location = "AAA";
			AssertNoErrors("No errors", rule.LocationInfo);

			rule.Location = "BBB";
			AssertNoErrors("No errors", rule.LocationInfo);

			rule.Location = "";
			AssertHasErrors("Invalid location", rule.LocationInfo);
		}

		public void TestPreSaveValidation()
		{
			CodeDescriptionPairList locations = new CodeDescriptionPairList();
			locations.AddPair("AAA", "some location");

			PhaseRule rule = new PhaseSecurity(locations).Phases.AddNew().Rules.AddNew();
			rule.Location = "AAA";
			rule.RunPreSaveValidation();
			AssertNoErrors(rule);

			rule.DepartmentPK = ZGuid.NewZGuid();
			rule.RunPreSaveValidation();
			AssertEquals(true, rule.HasErrors());

			rule.DepartmentPK = ZGuid.Empty;
			rule.RunPreSaveValidation();
			AssertNoErrors(rule);

			rule.Location = "XXX";
			rule.RunPreSaveValidation();
			AssertEquals(true, rule.HasErrors());
		}

		public void TestIPhaseRule()
		{
			var rule = new PhaseRule();
			rule.DepartmentPK = ZGuid.NewZGuid();
			rule.Location = "hello";

			var dependant1 = rule.Dependants.AddNew();
			dependant1.Name = "Sookie Stackhouse";
			dependant1.IsReadOnly = true;

			var dependant2 = rule.Dependants.AddNew();
			dependant2.Name = "Bill Compton";
			dependant2.IsReadOnly = true;

			var dependant3 = rule.Dependants.AddNew();
			dependant3.Name = "Eric Northman";
			dependant3.IsMandatory = true;

			var iRule = rule as IPhaseRule;
			AssertEquals(rule.DepartmentPK, iRule.DepartmentPK);
			AssertEquals("hello", iRule.Location);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "Sookie Stackhouse", "Bill Compton" }, iRule.ReadOnlyDependants.Select(x => x.Name));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "Eric Northman" }, iRule.MandatoryDependants.Select(x => x.Name));
		}

		public void TestClone_ReadOnlyDependants()
		{
			var rule = new PhaseRule();

			var dependant1 = rule.Dependants.AddNew();
			dependant1.Name = "Rachel Berry";
			dependant1.IsReadOnly = true;

			var dependant2 = rule.Dependants.AddNew();
			dependant2.Name = "Will Schuester";
			dependant2.IsReadOnly = true;

			var dependant3 = rule.Dependants.AddNew();
			dependant3.Name = "Dmitry is a Gleek!";
			dependant3.IsMandatory = true;

			var clonedRule = (PhaseRule)rule.Clone(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), rule.Factory);
			AssertContainsExactElementsInAnyOrder("Read Only collection cloned", new ZString[] { "Rachel Berry", "Will Schuester" }, clonedRule.ReadOnlyDependants.Cast<PhaseDependant>().Select(x => x.Name));
			AssertContainsExactElementsInAnyOrder("Mandatory collection cloned", new ZString[] { "Dmitry is a Gleek!" }, clonedRule.MandatoryDependants.Cast<PhaseDependant>().Select(x => x.Name));
		}

		public void TestDependantsText()
		{
			PhaseRule rule = new PhaseRule();
			AssertEquals("Precondition", 0, rule.Dependants.Count);
			AssertEquals("No customization", rule.DependantsText);

			rule.Dependants.AddNew();
			AssertEquals("Properties customized", rule.DependantsText);

			rule.Dependants.RemoveAll();
			AssertEquals("No customization", rule.DependantsText);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new PhaseRule();
		}

		protected override PhaseRule GetBusinessObjectToClone()
		{
			return (PhaseRule)GetNewBusinessObject();
		}

		protected override PhaseRule GetBusinessObjectToSerialise()
		{
			return (PhaseRule)GetNewBusinessObject();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		#endregion
	}
}
