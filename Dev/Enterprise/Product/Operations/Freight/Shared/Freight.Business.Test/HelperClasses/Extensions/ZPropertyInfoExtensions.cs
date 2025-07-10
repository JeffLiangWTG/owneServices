using System;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ZPropertyInfoExtensions : TestCaseWithFactory
	{
		public void TestNoValueEntered()
		{
			using (FreightDataRegistry.Instance.VesselInReferenceFileMandatoryOnShipments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				CreateValidateVesselIsValidTestData();

				dummyBusinessObject.Z0_VarCharMax = string.Empty;
				dummyBusinessObject.Z0_VarCharMaxInfo.ValidateVesselIsValid(() => vessel);
				AssertNoErrors("No Errors", dummyBusinessObject.Z0_VarCharMaxInfo);
			}
		}

		public void TestErrorNoValidVessel()
		{
			using (FreightDataRegistry.Instance.VesselInReferenceFileMandatoryOnShipments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				CreateValidateVesselIsValidTestData();

				dummyBusinessObject.Z0_VarCharMaxInfo.ValidateVesselIsValid(() => null);
				AssertHasError(dummyBusinessObject.Z0_VarCharMaxInfo, "Please enter a valid Vessel.");
			}
		}

		public void TestErrorInactiveVessel()
		{
			using (FreightDataRegistry.Instance.VesselInReferenceFileMandatoryOnShipments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				CreateValidateVesselIsValidTestData();
				vessel.RV_IsActive = false;

				dummyBusinessObject.Z0_VarCharMaxInfo.ValidateVesselIsValid(() => vessel);
				AssertHasError(dummyBusinessObject.Z0_VarCharMaxInfo, "This Vessel is inactive.");
			}
		}

		public void TestWarningNoReferenceFileForVessel()
		{
			using (FreightDataRegistry.Instance.VesselInReferenceFileMandatoryOnShipments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				CreateValidateVesselIsValidTestData();

				dummyBusinessObject.Z0_VarCharMaxInfo.ValidateVesselIsValid(() => null);
				AssertHasWarning(dummyBusinessObject.Z0_VarCharMaxInfo, "Warning: No reference file for this Vessel was found.");
			}
		}

		public void TestWarningInactiveVessel()
		{
			using (FreightDataRegistry.Instance.VesselInReferenceFileMandatoryOnShipments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				CreateValidateVesselIsValidTestData();
				vessel.RV_IsActive = false;

				dummyBusinessObject.Z0_VarCharMaxInfo.ValidateVesselIsValid(() => vessel);
				AssertHasWarning(dummyBusinessObject.Z0_VarCharMaxInfo, "Warning: This Vessel is inactive.");
			}
		}

		void RemoveChangedValidationOutsideOfCheckWarning()
		{
			if (ErrorReporter.TotalErrorCount == 1 && ErrorReporter.LastMessageReported.StartsWith("Attempt to change validation on a property info outside of its Check method"))
			{
				ErrorReporter.Clear();
			}
		}

		void CreateValidateVesselIsValidTestData()
		{
			vessel = Factory.New<RefVessel>();
			dummyBusinessObject = Factory.New<DummyBusinessObject>();
			dummyBusinessObject.Z0_VarCharMax = "SOME TEXT";
		}

		protected override void TearDown()
		{
			base.TearDown();
			RemoveChangedValidationOutsideOfCheckWarning();
		}

		DummyBusinessObject dummyBusinessObject;
		RefVessel vessel;
	}
}
