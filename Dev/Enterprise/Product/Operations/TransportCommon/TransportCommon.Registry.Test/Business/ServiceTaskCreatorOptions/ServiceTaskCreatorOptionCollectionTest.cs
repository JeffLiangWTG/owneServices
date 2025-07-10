using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using Enterprise.TransportCommon.Shared;
using NUnit.Framework;

namespace Enterprise.TransportCommon.Registry.Testing
{
	[TestedType(typeof(ServiceTaskCreatorOptionCollection))]
	public class ServiceTaskCreatorOptionCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ServiceTaskCreatorOptionCollection>
	{
		#region Defaults

		public void TestGetDefault()
		{
			var defaultValue = ServiceTaskCreatorOptionCollection.GetDefault();
			AssertEquals(4, defaultValue.Count);

			AssertJobTemplateDefault(defaultValue[0], AutoCreatorContainerModes.Codes.Loose, AutoCreatorTargetModules.Codes.PortTransport, false);
			AssertJobTemplateDefault(defaultValue[1], AutoCreatorContainerModes.Codes.Container, AutoCreatorTargetModules.Codes.PortTransport, false);
			AssertJobTemplateDefault(defaultValue[2], AutoCreatorContainerModes.Codes.FTL, AutoCreatorTargetModules.Codes.PortTransport, false);
			AssertJobTemplateDefault(defaultValue[3], AutoCreatorContainerModes.Codes.MixedCargo, AutoCreatorTargetModules.Codes.PortTransport, false);
		}

		void AssertJobTemplateDefault(ServiceTaskCreatorOption serviceTaskOption, ZString containerMode, ZString targetModule, ZBool isSystemDefined)
		{
			AssertEquals(containerMode, serviceTaskOption.ContainerMode);
			AssertEquals(targetModule, serviceTaskOption.TargetModule);
			AssertEquals(isSystemDefined, serviceTaskOption.IsSystemDefined);
		}

		#endregion

		#region AllowNew

		public void TestAllowNew()
		{
			AssertEquals("Must NOT allow new rows", false, Collection.AllowNew);
		}

		#endregion

		#region GetBookingTemplate

		public void TestGetBookingTemplate()
		{
			var defaultValue = ServiceTaskCreatorOptionCollection.GetDefault();

			var option = defaultValue.Cast<ServiceTaskCreatorOption>().FirstOrDefault(o => o.ContainerMode == AutoCreatorContainerModes.Codes.Loose);
			option.TargetModule = AutoCreatorTargetModules.Codes.LandTransportConsignment;

			AssertEquals(AutoCreatorTargetModules.Codes.LandTransportConsignment, defaultValue.GetTargetModule(AutoCreatorContainerModes.Codes.Loose));
			AssertEquals(AutoCreatorTargetModules.Codes.PortTransport, defaultValue.GetTargetModule(AutoCreatorContainerModes.Codes.Container));
			AssertEquals(AutoCreatorTargetModules.Codes.PortTransport, defaultValue.GetTargetModule(AutoCreatorContainerModes.Codes.FTL));
			AssertEquals(AutoCreatorTargetModules.Codes.PortTransport, defaultValue.GetTargetModule(AutoCreatorContainerModes.Codes.MixedCargo));
		}

		#endregion

		#region Overrides

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override ServiceTaskCreatorOptionCollection GetCollectionToTest()
		{
			return new ServiceTaskCreatorOptionCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ServiceTaskCreatorOption();
		}

		#endregion
	}
}
