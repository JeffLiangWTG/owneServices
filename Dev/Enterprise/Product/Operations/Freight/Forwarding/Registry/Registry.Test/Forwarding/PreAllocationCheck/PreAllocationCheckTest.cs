using CargoWise.ComponentModel;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Registry.Testing
{
	[TestedType(typeof(PreAllocationCheck))]
	public class PreAllocationCheckTest : RegistryBusinessObjectTemplateTestCase<PreAllocationCheck>
	{
		public void TestMeasureReadOnly()
		{
			PreAllocationCheck check = new PreAllocationCheck();
			AssertEquals(true, check.MeasureInfo.ReadOnly);

			check.Measure = "XXX";
			AssertEquals(true, check.MeasureInfo.ReadOnly);
		}

		public void TestActionBooleanProperties()
		{
			PreAllocationCheck check = new PreAllocationCheck();

			check.Action = PreAllocationCheck.Actions.None;
			AssertEquals(true, check.IsNone);
			AssertEquals(false, check.IsWarning);
			AssertEquals(false, check.IsRestriction);

			check.Action = PreAllocationCheck.Actions.Warning;
			AssertEquals(false, check.IsNone);
			AssertEquals(true, check.IsWarning);
			AssertEquals(false, check.IsRestriction);

			check.Action = PreAllocationCheck.Actions.Restriction;
			AssertEquals(false, check.IsNone);
			AssertEquals(false, check.IsWarning);
			AssertEquals(true, check.IsRestriction);
		}

		public void TestSettingActionToNoneResetsPercentage()
		{
			PreAllocationCheck check = new PreAllocationCheck();
			check.Percentage = 50m;
			check.Action = PreAllocationCheck.Actions.Warning;
			AssertEquals("Precondition", 50m, check.Percentage);

			check.Action = PreAllocationCheck.Actions.None;
			AssertEquals("Percentage was reset to 0", 0m, check.Percentage);
		}

		public void TestPercentageReadOnly()
		{
			PreAllocationCheck check = new PreAllocationCheck();
			AssertEquals(false, check.PercentageInfo.ReadOnly);

			check.Action = PreAllocationCheck.Actions.None;
			AssertEquals(true, check.PercentageInfo.ReadOnly);

			check.Action = PreAllocationCheck.Actions.Warning;
			AssertEquals(false, check.PercentageInfo.ReadOnly);

			check.Action = PreAllocationCheck.Actions.Restriction;
			AssertEquals(false, check.PercentageInfo.ReadOnly);
		}

		public void TestValidateAction()
		{
			PreAllocationCheck check = new PreAllocationCheck();
			check.Action = PreAllocationCheck.Actions.None;
			AssertNoErrors(check.ActionInfo);

			check.Action = PreAllocationCheck.Actions.Warning;
			AssertNoErrors(check.ActionInfo);

			check.Action = PreAllocationCheck.Actions.Restriction;
			AssertNoErrors(check.ActionInfo);

			check.Action = "XXX";
			AssertHasErrors(check.ActionInfo);
		}

		public void TestValidatePercentage()
		{
			PreAllocationCheck check = new PreAllocationCheck();
			check.Percentage = 32m;
			AssertNoErrors(check.PercentageInfo);

			check.Percentage = 0m;
			AssertHasErrors(check.PercentageInfo);

			check.Action = PreAllocationCheck.Actions.None;
			check.ValidatePercentage();
			AssertNoErrors(check.PercentageInfo);

			check.Percentage = 100m;
			AssertNoErrors(check.PercentageInfo);

			check.Percentage = -1m;
			AssertHasErrors(check.PercentageInfo);

			check.Percentage = 101m;
			AssertHasErrors(check.PercentageInfo);
		}

		public void TestPreSaveValidation()
		{
			PreAllocationCheck check = new PreAllocationCheck();
			check.Action = PreAllocationCheck.Actions.Warning;
			check.Percentage = 32m;
			check.RunPreSaveValidation();
			AssertNoErrors(check);

			check.Percentage = 120m;
			check.RunPreSaveValidation();
			AssertEquals(true, check.HasErrors());

			check.Percentage = 20m;
			check.RunPreSaveValidation();
			AssertNoErrors(check);

			check.Action = "XXX";
			check.RunPreSaveValidation();
			AssertEquals(true, check.HasErrors());
		}

		#region Implementation

		protected override PreAllocationCheck GetBusinessObjectToClone()
		{
			return new PreAllocationCheck();
		}

		protected override PreAllocationCheck GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
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
