using CargoWise.ComponentModel;
using Enterprise.Registry.Business.Testing;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.TransportCommon.Registry.Testing
{
	[TestedType(typeof(ServiceTaskCreatorOption))]
	public class ServiceTaskCreatorOptionTest : RegistryBusinessObjectTemplateTestCase<ServiceTaskCreatorOption>
	{
		#region Properties and Validation

		public void TestTargetModule()
		{
			AssertEquals("", BizObj.TargetModule);

			BizObj.ValidateTargetModule();
			AssertEquals(true, BizObj.TargetModuleInfo.HasErrors());

			BizObj.TargetModule = "XXX";
			AssertEquals(true, BizObj.TargetModuleInfo.HasErrors());

			BizObj.TargetModule = AutoCreatorTargetModules.Codes.PortTransport;
			AssertEquals(AutoCreatorTargetModules.Codes.PortTransport, BizObj.TargetModule);
			AssertEquals(false, BizObj.TargetModuleInfo.ReadOnly);

			BizObj.IsSystemDefined = true;
			AssertEquals(true, BizObj.TargetModuleInfo.ReadOnly);
		}

		#endregion

		#region Lists

		#region TestTargetModules

		public void TestTargetModules()
		{
			var expected = new CodeDescriptionPairList();
			expected.AddPair(AutoCreatorTargetModules.Codes.PortTransport, AutoCreatorTargetModules.Descriptions.PortTransport);
			expected.AddPair(AutoCreatorTargetModules.Codes.LandTransportConsignment, AutoCreatorTargetModules.Descriptions.LandTransportConsignment);

			AssertContainsExactElementsInAnyOrder(expected, BizObj.TargetModules);
		}

		#endregion

		#endregion

		#region Implementation

		protected override ServiceTaskCreatorOption GetBusinessObjectToClone()
		{
			return new ServiceTaskCreatorOption();
		}

		protected override ServiceTaskCreatorOption GetBusinessObjectToSerialise()
		{
			return new ServiceTaskCreatorOption();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected new ServiceTaskCreatorOption BizObj
		{
			get { return base.BizObj; }
		}

		#endregion
	}
}
