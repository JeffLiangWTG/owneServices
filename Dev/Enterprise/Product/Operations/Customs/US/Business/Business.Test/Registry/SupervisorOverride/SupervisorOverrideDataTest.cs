using System;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(SupervisorOverrideData))]
	sealed class SupervisorOverrideDataTest : RegistryBusinessObjectTemplateTestCase<SupervisorOverrideData>
	{
		public void TestCloneNominatedMessageErrors()
		{
			var supervisorOverrideData = new SupervisorOverrideData();
			supervisorOverrideData.NominatedMessageErrors.AddNew().FieldName = "Test";

			var cloned = (SupervisorOverrideData)supervisorOverrideData.Clone(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
			var clonedMessageErrors = cloned.NominatedMessageErrors;
			AssertNotNull(clonedMessageErrors);
			AssertEquals(1, clonedMessageErrors.Count);
			AssertEquals("Test", clonedMessageErrors[0].FieldName);
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override SupervisorOverrideData GetBusinessObjectToClone()
		{
			SupervisorOverrideData result = new SupervisorOverrideData();
			result.FillWithValidTestData();
			return result;
		}

		protected override SupervisorOverrideData GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		#endregion
	}
}
