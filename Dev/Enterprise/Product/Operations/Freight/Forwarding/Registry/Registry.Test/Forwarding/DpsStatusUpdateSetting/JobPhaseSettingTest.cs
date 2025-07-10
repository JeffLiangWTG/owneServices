using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Registry.Testing
{
	[TestedType(typeof(JobPhaseSetting))]
	public class JobPhaseSettingTest : RegistryBusinessObjectTestCaseBase
	{
		public void TestJobPhaseSetting()
		{
			var setting = new JobPhaseSetting("ABC", (NoResString)"Desc", false);
			AssertEquals("ABC", setting.Code);
			AssertEquals("Desc", setting.Description.GetUnresolvedString());
			Assert(!setting.ShouldUpdate);
		}

		public void TestPhaseAndJobPhaseSettingHasTheSameDescriptionMaxLength()
		{
			var phase = new Phase();
			var phaseSetting = new JobPhaseSetting();
			AssertEquals(phase.Description_MaxLength, phaseSetting.Description_MaxLength);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new JobPhaseSetting();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return (JobPhaseSetting)GetNewBusinessObject();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return (JobPhaseSetting)GetNewBusinessObject();
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
