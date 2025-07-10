using System.Collections.Generic;
using System.Linq;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Freight.LocalCartage.Service.Testing
{
	[TestedType(typeof(GPSUpdateCartageTask))]
	public class GPSUpdateCartageTaskTest : ServiceTaskTestCase<GPSUpdateCartageTask>
	{
		public void TestHostedServiceBusinessObjectBindingAttribute()
		{
			var hostedServiceBusinessObjectBindingAttributes = AssemblyMetaDataReader.GetAttributes<HostedServiceBusinessObjectBindingAttribute>();
			var serviceCodes = hostedServiceBusinessObjectBindingAttributes.Select(s => s.ServiceTaskCode);
			AssertCollectionContains("UCT service task should have HostedServiceBusinessObjectBindingAttribute added.", "UCT", serviceCodes);
		}

		public void TestServiceTaskIsNudgedByLocations()
		{
			var assembly = typeof(GPSUpdateCartageTask).Assembly;
			var attributes = assembly.GetCustomAttributes(typeof(HostedServiceBusinessObjectBindingAttribute), false);
			var result = attributes.Cast<HostedServiceBusinessObjectBindingAttribute>().Count(attribute => attribute.ServiceTaskCode == GPSUpdateCartageTask.Code && attribute.Table == "GlbDeviceLocation");
			AssertEquals(1, result);
		}

		public void TestServiceTaskIsRegistered()
		{
			var assembly = typeof(GPSUpdateCartageTask).Assembly;
			var attributes = assembly.GetCustomAttributes(typeof(HostedServiceAttribute), false);
			var result = attributes.Cast<HostedServiceAttribute>().Count(attribute => attribute.Code == GPSUpdateCartageTask.Code);
			AssertEquals(1, result);
		}

		public void TestServiceTaskHasCorrectAttributes()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes().Single();
			CombineAssertions("Service Task should have correct hosted service attributes", () =>
			{
				AssertEquals("MinimumPeriod should be correct", "5minutes", hostedServiceAttributes.MinimumPeriod);
				AssertEquals("Description should be correct", "GPS Update Port Transport Task", hostedServiceAttributes.Description);
				AssertEquals("Should not be configured to run in any branch. Rationale: GPSCartageLegUpdater converts UTC to local time, which is affected by the current branch.", false, hostedServiceAttributes.CanRunInAnyBranch);
			});
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();
	}
}
