using System;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(DefaultContainerModes))]
	sealed class DefaultContainerModesTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestValidateTransportMode()
		{
			BizObj.RunPreSaveValidation();
			AssertEquals("TransportMode should have errors", true, BizObj.TransportModeInfo.HasErrors());

			BizObj.TransportMode = "";
			AssertEquals("TransportMode should have errors", true, BizObj.TransportModeInfo.HasErrors());

			BizObj.TransportMode = "AIR";
			AssertEquals("TransportMode should not have any errors", false, BizObj.TransportModeInfo.HasErrors());
		}

		public void TestContainerMode()
		{
			BizObj.RunPreSaveValidation();
			AssertHasErrors(BizObj.ContainerModeInfo);

			BizObj.TransportMode = "";
			BizObj.ContainerMode = "";
			AssertHasErrors(BizObj.ContainerModeInfo);

			BizObj.TransportMode = "";
			BizObj.ContainerMode = Constants.ContainerModes.FCL;
			AssertHasError(BizObj.ContainerModeInfo, "Enter a valid selection.");

			BizObj.TransportMode = Constants.TransportModes.Air;
			BizObj.ContainerMode = Constants.ContainerModes.BreakBulk;
			AssertHasError(BizObj.ContainerModeInfo, "Enter a valid selection.");

			BizObj.TransportMode = Constants.TransportModes.Sea;
			BizObj.ContainerMode = Constants.ContainerModes.BreakBulk;
			AssertNoErrors(BizObj.ContainerModeInfo);
		}

		public void TestContainerModeLookup()
		{
			BizObj.TransportMode = string.Empty;
			AssertEquals(0, BizObj.ContainerModeList.Count);

			AssertConstainerModeForTransport(Constants.TransportModes.Air);
			AssertConstainerModeForTransport(Constants.TransportModes.Sea);
			AssertConstainerModeForTransport(Constants.TransportModes.SeaAir);
			AssertConstainerModeForTransport(Constants.TransportModes.AirSea);
			AssertConstainerModeForTransport(Constants.TransportModes.Road);
			AssertConstainerModeForTransport(Constants.TransportModes.Rail);
			AssertConstainerModeForTransport(Constants.TransportModes.Courier);
			AssertConstainerModeForTransport("INV"); //invalid transport mode
		}

		void AssertConstainerModeForTransport(string transportMode)
		{
			BizObj.TransportMode = transportMode;
			var expectedList = FreightCodePairLists.JS_PackingModeList(transportMode);

			var expected = GetSummaryFromContainerList(transportMode, expectedList);
			var actual = GetSummaryFromContainerList(transportMode, BizObj.ContainerModeList);

			AssertEquals("Lookup for container mode should be relevant to transport mode selection.", expected, actual);
		}

		string GetSummaryFromContainerList(string transportMode, CodeDescriptionPairList list)
		{
			var summary = new StringBuilder();
			summary.AppendLine(transportMode + ":");

			foreach (CodeDescriptionPair mode in list)
			{
				var containerMode = mode.Code + " - " + mode.Description;
				summary.AppendLine(containerMode);
			}

			return summary.ToString();
		}

		[ExpectException(typeof(RegistryValidationException))]
		public void TestDuplicateRegistryEntry()
		{
			var collection = new DefaultContainerModesCollection();

			var registryContainerModes = collection.AddNew();
			registryContainerModes.TransportMode = "AIR";
			registryContainerModes.ContainerMode = "ULD";

			FreightConfigurationRegistry.Instance.DefaultContainerModes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			var value = FreightConfigurationRegistry.Instance.DefaultContainerModes.Value;
			Assert("Validation should pass", !registryContainerModes.HasErrors);

			var duplicateRegistry = collection.AddNew();
			duplicateRegistry.TransportMode = "AIR";
			duplicateRegistry.ContainerMode = "ULD";

			FreightConfigurationRegistry.Instance.DefaultContainerModes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
		}

		#region Test ZPropertyInfos

		public void TestNewZPropertyInfos()
		{
			TestZPropertyInfo(BizObj.TransportModeInfo, DefaultContainerModes.Schema.TransportMode, 3);
			TestZPropertyInfo(BizObj.ContainerModeInfo, DefaultContainerModes.Schema.ContainerMode);
		}

		void TestZPropertyInfo(ZPropertyInfo propertyInfo, string expectedName)
		{
			AssertNotNull("ZPropertyInfo for " + propertyInfo.Name + " was null", propertyInfo);
			AssertEquals("PropertyInfo.Name", expectedName, propertyInfo.Name);
		}

		void TestZPropertyInfo(ZPropertyInfo propertyInfo, string expectedName, int expectedMaxLength)
		{
			TestZPropertyInfo(propertyInfo, expectedName);
			AssertEquals("PropertyInfo.MaxLength", expectedMaxLength, propertyInfo.MaxLength);
		}

		#endregion

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			BizObj.TransportMode = "AIR";
			BizObj.ContainerMode = "ULD";

			return BizObj;
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		new DefaultContainerModes BizObj
		{
			get { return (DefaultContainerModes)base.BizObj; }
		}

		#endregion
	}
}
