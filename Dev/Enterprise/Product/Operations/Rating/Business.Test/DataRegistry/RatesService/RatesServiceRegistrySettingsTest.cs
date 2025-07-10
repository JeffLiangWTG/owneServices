using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(RatesServiceRegistrySettings))]
	public class RatesServiceRegistrySettingsTest : RegistryBusinessObjectTemplateTestCase
	{
		RatesServiceRegistrySettings ratesServiceRateSelector;

		public void TestValidateTransportModeBasedOnRateProvider_NoErrors()
		{
			ratesServiceRateSelector = new RatesServiceRegistrySettings { TransportMode = Core.Constants.TransportModes.Sea };
			AssertNoErrors(ratesServiceRateSelector.TransportModeInfo);

			ratesServiceRateSelector.TransportMode = Core.Constants.TransportModes.Air;
			AssertNoErrors(ratesServiceRateSelector.TransportModeInfo);
		}

		public void TestValidateContainerModeBasedOnTransportMode_CWSupport()
		{
			AssertValidateContainerModeBasedOnTransportMode(Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.FCL);
			AssertValidateContainerModeBasedOnTransportMode(Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.LCL);
			AssertValidateContainerModeBasedOnTransportMode(Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.BuyersConsol);
			AssertValidateContainerModeBasedOnTransportMode(Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.Groupage);
			AssertValidateContainerModeBasedOnTransportMode(Core.Constants.TransportModes.Air, Core.Constants.ContainerModes.ULD);
			AssertValidateContainerModeBasedOnTransportMode(Core.Constants.TransportModes.Air, Core.Constants.ContainerModes.Loose);
			AssertValidateContainerModeBasedOnTransportMode(Core.Constants.TransportModes.Air, Core.Constants.ContainerModes.BuyersConsol);

			AssertValidateContainerModeBasedOnTransportMode(Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.ULD, true);
			AssertValidateContainerModeBasedOnTransportMode(Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.Loose, true);
			AssertValidateContainerModeBasedOnTransportMode(Core.Constants.TransportModes.Air, Core.Constants.ContainerModes.FCL, true);
			AssertValidateContainerModeBasedOnTransportMode(Core.Constants.TransportModes.Air, Core.Constants.ContainerModes.LCL, true);
			AssertValidateContainerModeBasedOnTransportMode(Core.Constants.TransportModes.Air, Core.Constants.ContainerModes.Groupage, true);
		}

		public void TestValidateContainerModeBasedOnTransportMode_RegularUser()
		{
			var regularUser = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			using (Env.SetTemporaryUserContext(regularUser.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				AssertValidateContainerModeBasedOnTransportMode(Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.FCL);
				AssertValidateContainerModeBasedOnTransportMode(Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.LCL);
				AssertValidateContainerModeBasedOnTransportMode(Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.BuyersConsol);
				AssertValidateContainerModeBasedOnTransportMode(Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.Groupage);
				AssertValidateContainerModeBasedOnTransportMode(Core.Constants.TransportModes.Air, Core.Constants.ContainerModes.ULD);
				AssertValidateContainerModeBasedOnTransportMode(Core.Constants.TransportModes.Air, Core.Constants.ContainerModes.Loose);
				AssertValidateContainerModeBasedOnTransportMode(Core.Constants.TransportModes.Air, Core.Constants.ContainerModes.BuyersConsol);

				AssertValidateContainerModeBasedOnTransportMode(Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.ULD, true);
				AssertValidateContainerModeBasedOnTransportMode(Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.Loose, true);
				AssertValidateContainerModeBasedOnTransportMode(Core.Constants.TransportModes.Air, Core.Constants.ContainerModes.FCL, true);
				AssertValidateContainerModeBasedOnTransportMode(Core.Constants.TransportModes.Air, Core.Constants.ContainerModes.LCL, true);
				AssertValidateContainerModeBasedOnTransportMode(Core.Constants.TransportModes.Air, Core.Constants.ContainerModes.Groupage, true);
			}
		}

		void AssertValidateContainerModeBasedOnTransportMode(string transportMode, string containerMode, bool hasError = false)
		{
			ratesServiceRateSelector = new RatesServiceRegistrySettings { TransportMode = transportMode, ContainerMode = containerMode };

			var actualHasError = ratesServiceRateSelector.ContainerModeInfo.HasErrors();
			var expectedMessage = $"{transportMode}-{containerMode} is {(hasError ? "not" : "")} supported";

			AssertEquals(expectedMessage, hasError, actualHasError);
		}

		public void TestBlankFieldsInDropDown()
		{
			ratesServiceRateSelector = new RatesServiceRegistrySettings();
			AssertNoErrors(ratesServiceRateSelector.TransportModeInfo);
			AssertNoErrors(ratesServiceRateSelector.ContainerModeInfo);

			ratesServiceRateSelector.TransportMode = "";
			AssertHasError(ratesServiceRateSelector.TransportModeInfo, "Please enter a Transport Mode.");

			ratesServiceRateSelector.ContainerMode = "";
			AssertHasError(ratesServiceRateSelector.ContainerModeInfo, "Please enter a Container Mode.");
		}

		public void TestReadOnly_DifferentUsers_ExistingAndNewSettings()
		{
			AssertReadOnly_DifferentUsers_ExistingAndNewSettings(
				Core.Constants.TransportModes.Air,
				Core.Constants.ContainerModes.Loose);

			AssertReadOnly_DifferentUsers_ExistingAndNewSettings(
				Core.Constants.TransportModes.Air,
				Core.Constants.ContainerModes.ULD);

			AssertReadOnly_DifferentUsers_ExistingAndNewSettings(
				Core.Constants.TransportModes.Sea,
				Core.Constants.ContainerModes.FCL);

			AssertReadOnly_DifferentUsers_ExistingAndNewSettings(
				Core.Constants.TransportModes.Sea,
				Core.Constants.ContainerModes.BuyersConsol);

			AssertReadOnly_DifferentUsers_ExistingAndNewSettings(
				Core.Constants.TransportModes.Sea,
				Core.Constants.ContainerModes.Groupage);

			AssertReadOnly_DifferentUsers_ExistingAndNewSettings(
				Core.Constants.TransportModes.Sea,
				Core.Constants.ContainerModes.LCL);

			AssertReadOnly_DifferentUsers_ExistingAndNewSettings(
			Core.Constants.TransportModes.Air,
			Core.Constants.ContainerModes.BuyersConsol);
		}

		void AssertReadOnly_DifferentUsers_ExistingAndNewSettings(
			string transportMode,
			string containerMode,
			bool expectedLoadedItemReadOnlyStatus = false,
			string expectedLoadedItemReadOnlyStatusReason = "Existing setting should be editable.")
		{
			AssertEquals("Precondition: Current user is CWSupport", ZBool.True, Env.CurrentUser.IsSupportUser);

			var setting1 = new RatesServiceRegistrySettings();
			setting1.TransportMode = transportMode;
			setting1.ContainerMode = containerMode;

			AssertEquals(
				"All newly created settings are writable when a CWSupport user makes them",
				false,
				setting1.ReadOnly);

			var setting2 = (RatesServiceRegistrySettings)setting1.Clone(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
			AssertEquals("CWSupport can edit any loaded settings", false, setting2.ReadOnly);

			var regularUser = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			using (Env.SetTemporaryUserContext(regularUser.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				AssertEquals("Precondition: Current user is a regular user", ZBool.False, Env.CurrentUser.IsSupportUser);

				var setting3 = (RatesServiceRegistrySettings)setting1.Clone(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
				AssertEquals(expectedLoadedItemReadOnlyStatusReason, expectedLoadedItemReadOnlyStatus, setting3.ReadOnly);

				var setting4 = new RatesServiceRegistrySettings();
				setting4.TransportMode = transportMode;
				setting4.ContainerMode = containerMode;
				AssertEquals(
					"All newly created settings are writable when a regular user makes them",
					false,
					setting4.ReadOnly);
			}
		}

		public void TestReadOnly_RegularUser_WhenChangeValidSettingsToInvalid()
		{
			var regularUser = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			using (Env.SetTemporaryUserContext(regularUser.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				AssertEquals("Precondition: Current user is a regular user", ZBool.False, Env.CurrentUser.IsSupportUser);

				var setting1 = new RatesServiceRegistrySettings();
				setting1.TransportMode = Core.Constants.TransportModes.Air;
				setting1.ContainerMode = Core.Constants.ContainerModes.Loose;
				AssertEquals("newly created valid settings should be writeable", false, setting1.ReadOnly);

				var setting2 = (RatesServiceRegistrySettings)setting1.Clone(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
				AssertEquals("loaded valid settings should be writeable", false, setting2.ReadOnly);
				var setting3 = (RatesServiceRegistrySettings)setting1.Clone(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
				AssertEquals("loaded valid settings should be writeable", false, setting3.ReadOnly);

				setting1.ContainerMode = Core.Constants.ContainerModes.ULD;
				AssertEquals("newly created setting should be writable when a regular user edits them", false, setting1.ReadOnly);

				setting1.TransportMode = Core.Constants.TransportModes.Sea;
				setting1.ContainerMode = Core.Constants.ContainerModes.LCL;
				AssertEquals("newly created setting should be writable when a regular user edits them", false, setting1.ReadOnly);

				setting2.TransportMode = Core.Constants.TransportModes.Air;
				setting2.ContainerMode = Core.Constants.ContainerModes.ULD;
				AssertEquals("loaded setting should be writable when a regular user edits them", false, setting2.ReadOnly);

				setting3.TransportMode = Core.Constants.TransportModes.Sea;
				setting3.ContainerMode = Core.Constants.ContainerModes.LCL;
				AssertEquals("loaded settings should be writable when a regular user edits them", false, setting3.ReadOnly);
			}
		}

		public void TestValidateContainerModes_DifferentUsers_ExistingAndNewSettings()
		{
			AssertValidateContainerModes_DifferentUsers_ExistingAndNewSettings(
				Core.Constants.TransportModes.Air,
				Core.Constants.ContainerModes.Loose);

			AssertValidateContainerModes_DifferentUsers_ExistingAndNewSettings(
				Core.Constants.TransportModes.Air,
				Core.Constants.ContainerModes.ULD);

			AssertValidateContainerModes_DifferentUsers_ExistingAndNewSettings(
				Core.Constants.TransportModes.Sea,
				Core.Constants.ContainerModes.FCL);

			AssertValidateContainerModes_DifferentUsers_ExistingAndNewSettings(
				Core.Constants.TransportModes.Sea,
				Core.Constants.ContainerModes.BuyersConsol);

			AssertValidateContainerModes_DifferentUsers_ExistingAndNewSettings(
				Core.Constants.TransportModes.Sea,
				Core.Constants.ContainerModes.Groupage);

			AssertValidateContainerModes_DifferentUsers_ExistingAndNewSettings(
				Core.Constants.TransportModes.Sea,
				Core.Constants.ContainerModes.LCL);

			AssertValidateContainerModes_DifferentUsers_ExistingAndNewSettings(
				Core.Constants.TransportModes.Air,
				Core.Constants.ContainerModes.BuyersConsol);
		}

		void AssertValidateContainerModes_DifferentUsers_ExistingAndNewSettings(
			string transportMode,
			string containerMode,
			bool expectedNewItemValidationError = false,
			string expectedNewItemValidationErrorReason = "New setting should be valid.")
		{
			var validationErrorMessage = $"{transportMode}-{containerMode} is not a currently supported option.";

			var setting1 = new RatesServiceRegistrySettings();
			setting1.TransportMode = transportMode;
			setting1.ContainerMode = containerMode;

			AssertEquals(
				"CWSupport User can add any combination",
				false,
				setting1.ContainerModeInfo.HasError(validationErrorMessage)
			);

			var setting2 = (RatesServiceRegistrySettings)setting1.Clone(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
			AssertEquals(
				"CWSupport User can edit any loaded combination",
				false,
				setting2.ContainerModeInfo.HasError(validationErrorMessage)
			);

			var regularUser = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			using (Env.SetTemporaryUserContext(regularUser.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var setting3 = (RatesServiceRegistrySettings)setting1.Clone(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
				AssertEquals(
					"Regular user has no validation error on loaded settings",
					false,
					setting3.ContainerModeInfo.HasError(validationErrorMessage)
				);

				var setting4 = new RatesServiceRegistrySettings();
				setting4.TransportMode = transportMode;
				setting4.ContainerMode = containerMode;
				AssertEquals(
					expectedNewItemValidationErrorReason,
					expectedNewItemValidationError,
					setting4.ContainerModeInfo.HasError(validationErrorMessage)
				);
			}
		}

		public void TestValidateContainerModes_RegularUser_WhenChangeValidSettingsToInvalid()
		{
			var regularUser = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			using (Env.SetTemporaryUserContext(regularUser.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				AssertEquals("Precondition: Current user is a regular user", ZBool.False, Env.CurrentUser.IsSupportUser);

				var setting1 = new RatesServiceRegistrySettings();
				setting1.TransportMode = Core.Constants.TransportModes.Air;
				setting1.ContainerMode = Core.Constants.ContainerModes.Loose;
				AssertEquals("Valid setting should not have validation error when regular user makes it", false, setting1.ContainerModeInfo.HasErrors());

				var setting2 = (RatesServiceRegistrySettings)setting1.Clone(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
				AssertEquals("Valid setting should not have validation error when regular user loads it", false, setting2.ContainerModeInfo.HasErrors());

				var setting3 = (RatesServiceRegistrySettings)setting1.Clone(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
				AssertEquals("Valid setting should not have validation error when regular user loads it", false, setting3.ContainerModeInfo.HasErrors());

				setting1.ContainerMode = Core.Constants.ContainerModes.ULD;
				AssertEquals("Valid setting should not have validation error when regular user edits valid new setting", false, setting1.ContainerModeInfo.HasErrors());

				setting2.TransportMode = Core.Constants.TransportModes.Air;
				setting2.ContainerMode = Core.Constants.ContainerModes.ULD;
				AssertEquals("Valid setting should not have validation error when regular user edits valid loaded setting", false, setting2.ContainerModeInfo.HasErrors());
			}
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new RatesServiceRegistrySettings();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
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
