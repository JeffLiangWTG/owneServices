using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Packing.Business;

namespace Enterprise.TransportCommon.Business.Testing
{
	public abstract class DtbTransportValidationTest : BusinessObjectValidationTestCase
	{
		#region TestCheckKM_KT_NKBookingTemplate

		public void TestCheckKM_KT_NKBookingTemplate()
		{
			var transport = GetNewTransport();
			transport.Validation.ValidateKM_KT_NKBookingTemplate();
			AssertNoErrors("Booking Template is not a mandatory field.", transport.KM_KT_NKBookingTemplateInfo);

			transport.KM_KT_NKBookingTemplate = "NVLD";
			AssertHasError(transport.KM_KT_NKBookingTemplateInfo, "Enter a valid Template.");

			transport.KM_KT_NKBookingTemplate = "IFCX"; // Import FCL, CNR to CNE
			AssertNoErrors(transport.KM_KT_NKBookingTemplateInfo);
		}

		#endregion

		#region TestCheckKM_IsHazardous

		public void TestCheckKM_IsHazardous()
		{
			var transport = GetNewTransport();
			var packageJob = GetPackageJob(transport);
			var package = packageJob.Packages.AddNew();

			var expectedNoDGUsedErrorMessage = "No packages with Dangerous Goods have been assigned to Instructions.";
			var expectedDGUsedErrorMessage = "There are packages with Dangerous Goods assigned to Instructions. Is Hazardous needs to be checked.";

			// No packages attached
			AssertNoError(transport.KM_IsHazardousInfo, expectedNoDGUsedErrorMessage);
			AssertNoError(transport.KM_IsHazardousInfo, expectedDGUsedErrorMessage);

			transport.KM_IsHazardous = true;
			AssertHasError(transport.KM_IsHazardousInfo, expectedNoDGUsedErrorMessage);
			AssertNoError(transport.KM_IsHazardousInfo, expectedDGUsedErrorMessage);

			package.UNDGs.AddNew();
			transport.KM_IsHazardous = false;
			transport.KM_IsHazardous = true;
			AssertHasError(transport.KM_IsHazardousInfo, expectedNoDGUsedErrorMessage);
			AssertNoError(transport.KM_IsHazardousInfo, expectedDGUsedErrorMessage);

			// With package with Dangerous Goods.
			((DtbTransportInstruction)transport.Instructions.AddNew()).DivotsWithPackages.AddPackage(package);
			transport.KM_IsHazardous = false;
			transport.KM_IsHazardous = true;
			AssertNoError(transport.KM_IsHazardousInfo, expectedNoDGUsedErrorMessage);
			AssertNoError(transport.KM_IsHazardousInfo, expectedDGUsedErrorMessage);

			transport.KM_IsHazardous = false;
			AssertNoError(transport.KM_IsHazardousInfo, expectedNoDGUsedErrorMessage);
			AssertHasError(transport.KM_IsHazardousInfo, expectedDGUsedErrorMessage);
		}

		public void TestCheckKM_IsHazardous_Recursively()
		{
			var transport = GetNewTransport();
			var packageJob = GetPackageJob(transport);
			var packageTopLevel = packageJob.Packages.AddNew();
			var packageChild = packageTopLevel.Packages.AddNew();

			var expectedNoDGUsedErrorMessage = "No packages with Dangerous Goods have been assigned to Instructions.";
			var expectedDGUsedErrorMessage = "There are packages with Dangerous Goods assigned to Instructions. Is Hazardous needs to be checked.";

			// No packages attached
			AssertNoError(transport.KM_IsHazardousInfo, expectedNoDGUsedErrorMessage);
			AssertNoError(transport.KM_IsHazardousInfo, expectedDGUsedErrorMessage);

			transport.KM_IsHazardous = true;
			AssertHasError(transport.KM_IsHazardousInfo, expectedNoDGUsedErrorMessage);
			AssertNoError(transport.KM_IsHazardousInfo, expectedDGUsedErrorMessage);

			packageChild.UNDGs.AddNew();
			transport.KM_IsHazardous = false;
			transport.KM_IsHazardous = true;
			AssertHasError(transport.KM_IsHazardousInfo, expectedNoDGUsedErrorMessage);
			AssertNoError(transport.KM_IsHazardousInfo, expectedDGUsedErrorMessage);

			// With package with Dangerous Goods.
			((DtbTransportInstruction)transport.Instructions.AddNew()).DivotsWithPackages.AddPackage(packageTopLevel);
			transport.KM_IsHazardous = false;
			transport.KM_IsHazardous = true;
			AssertNoError(transport.KM_IsHazardousInfo, expectedNoDGUsedErrorMessage);
			AssertNoError(transport.KM_IsHazardousInfo, expectedDGUsedErrorMessage);

			transport.KM_IsHazardous = false;
			AssertNoError(transport.KM_IsHazardousInfo, expectedNoDGUsedErrorMessage);
			AssertHasError(transport.KM_IsHazardousInfo, expectedDGUsedErrorMessage);
		}

		#endregion

		#region TestCheckKM_RequiresRefrigeration

		public void TestCheckKM_RequiresRefrigeration()
		{
			var transport = GetNewTransport();
			var packageJob = GetPackageJob(transport);
			var package = packageJob.Packages.AddNew();

			var expectedRequireNoRefrigerationErrorMessage = "No packages that Require Refrigeration have been assigned to Instructions.";
			var expectedRequireRefrigerationErrorMessage = "There are packages that Require Refrigeration assigned to Instructions. Require Refrigeration need to be checked.";

			// No packages attached
			AssertNoError(transport.KM_RequiresRefrigerationInfo, expectedRequireNoRefrigerationErrorMessage);
			AssertNoError(transport.KM_RequiresRefrigerationInfo, expectedRequireRefrigerationErrorMessage);

			transport.KM_RequiresRefrigeration = true;
			AssertHasError(transport.KM_RequiresRefrigerationInfo, expectedRequireNoRefrigerationErrorMessage);
			AssertNoError(transport.KM_RequiresRefrigerationInfo, expectedRequireRefrigerationErrorMessage);

			package.KP_RequiresTemperatureControl = true;
			transport.KM_RequiresRefrigeration = false;
			transport.KM_RequiresRefrigeration = true;
			AssertHasError(transport.KM_RequiresRefrigerationInfo, expectedRequireNoRefrigerationErrorMessage);
			AssertNoError(transport.KM_RequiresRefrigerationInfo, expectedRequireRefrigerationErrorMessage);

			// With package with temperature control required
			((DtbTransportInstruction)transport.Instructions.AddNew()).DivotsWithPackages.AddPackage(package);
			transport.KM_RequiresRefrigeration = false;
			transport.KM_RequiresRefrigeration = true;
			AssertNoError(transport.KM_RequiresRefrigerationInfo, expectedRequireNoRefrigerationErrorMessage);
			AssertNoError(transport.KM_RequiresRefrigerationInfo, expectedRequireRefrigerationErrorMessage);

			transport.KM_RequiresRefrigeration = false;
			AssertNoError(transport.KM_RequiresRefrigerationInfo, expectedRequireNoRefrigerationErrorMessage);
			AssertHasError(transport.KM_RequiresRefrigerationInfo, expectedRequireRefrigerationErrorMessage);

			// With container with no temperature control required
			package.KP_RequiresTemperatureControl = false;
			package.KP_F3_NKPackType = Constants.PkgUnit.Container;
			transport.KM_RequiresRefrigeration = true;
			AssertHasError(transport.KM_RequiresRefrigerationInfo, expectedRequireNoRefrigerationErrorMessage);
			AssertNoError(transport.KM_RequiresRefrigerationInfo, expectedRequireRefrigerationErrorMessage);

			// With container with temperature control required
			package.Container.K0_IsControlledAtmosphere = true;
			transport.KM_RequiresRefrigeration = false;
			transport.KM_RequiresRefrigeration = true;
			AssertNoError(transport.KM_RequiresRefrigerationInfo, expectedRequireNoRefrigerationErrorMessage);
			AssertNoError(transport.KM_RequiresRefrigerationInfo, expectedRequireRefrigerationErrorMessage);

			transport.KM_RequiresRefrigeration = false;
			AssertNoError(transport.KM_RequiresRefrigerationInfo, expectedRequireNoRefrigerationErrorMessage);
			AssertHasError(transport.KM_RequiresRefrigerationInfo, expectedRequireRefrigerationErrorMessage);
		}

		public void TestCheckKM_RequiresRefrigeration_Recursively()
		{
			var transport = GetNewTransport();
			var packageJob = GetPackageJob(transport);
			var packageTopLevel = packageJob.Packages.AddNew();
			var packageChild = packageTopLevel.Packages.AddNew();

			var expectedRequireNoRefrigerationErrorMessage = "No packages that Require Refrigeration have been assigned to Instructions.";
			var expectedRequireRefrigerationErrorMessage = "There are packages that Require Refrigeration assigned to Instructions. Require Refrigeration need to be checked.";

			// No packages attached
			AssertNoError(transport.KM_RequiresRefrigerationInfo, expectedRequireNoRefrigerationErrorMessage);
			AssertNoError(transport.KM_RequiresRefrigerationInfo, expectedRequireRefrigerationErrorMessage);

			transport.KM_RequiresRefrigeration = true;
			AssertHasError(transport.KM_RequiresRefrigerationInfo, expectedRequireNoRefrigerationErrorMessage);
			AssertNoError(transport.KM_RequiresRefrigerationInfo, expectedRequireRefrigerationErrorMessage);

			packageChild.KP_RequiresTemperatureControl = true;
			transport.KM_RequiresRefrigeration = false;
			AssertNoError(transport.KM_RequiresRefrigerationInfo, expectedRequireNoRefrigerationErrorMessage);
			AssertNoError(transport.KM_RequiresRefrigerationInfo, expectedRequireRefrigerationErrorMessage);

			// With package with temperature control required
			((DtbTransportInstruction)transport.Instructions.AddNew()).DivotsWithPackages.AddPackage(packageTopLevel);
			transport.KM_RequiresRefrigeration = true;
			AssertNoError(transport.KM_RequiresRefrigerationInfo, expectedRequireNoRefrigerationErrorMessage);
			AssertNoError(transport.KM_RequiresRefrigerationInfo, expectedRequireRefrigerationErrorMessage);

			transport.KM_RequiresRefrigeration = false;
			AssertNoError(transport.KM_RequiresRefrigerationInfo, expectedRequireNoRefrigerationErrorMessage);
			AssertHasError(transport.KM_RequiresRefrigerationInfo, expectedRequireRefrigerationErrorMessage);

			// With container with no temperature control required
			packageChild.KP_RequiresTemperatureControl = false;
			packageChild.KP_F3_NKPackType = Constants.PkgUnit.Container;
			transport.KM_RequiresRefrigeration = true;
			packageChild.KP_RequiresTemperatureControl = false;
			AssertHasError(transport.KM_RequiresRefrigerationInfo, expectedRequireNoRefrigerationErrorMessage);
			AssertNoError(transport.KM_RequiresRefrigerationInfo, expectedRequireRefrigerationErrorMessage);

			// With container with temperature control required
			packageChild.Container.K0_IsControlledAtmosphere = true;
			transport.KM_RequiresRefrigeration = true;
			AssertNoError(transport.KM_RequiresRefrigerationInfo, expectedRequireNoRefrigerationErrorMessage);
			AssertNoError(transport.KM_RequiresRefrigerationInfo, expectedRequireRefrigerationErrorMessage);

			transport.KM_RequiresRefrigeration = false;
			AssertNoError(transport.KM_RequiresRefrigerationInfo, expectedRequireNoRefrigerationErrorMessage);
			AssertHasError(transport.KM_RequiresRefrigerationInfo, expectedRequireRefrigerationErrorMessage);
		}

		#endregion

		#region TestValidateKM_RS_NKServiceLevel

		public void TestValidateKM_RS_NKServiceLevel()
		{
			var transport = GetNewTransport();
			transport.KM_RS_NKServiceLevel = "";
			AssertNoNotifications(transport.KM_RS_NKServiceLevelInfo);

			transport.KM_RS_NKServiceLevel = "ABC";
			AssertHasError(transport.KM_RS_NKServiceLevelInfo, "Enter a valid Service Level.");

			transport.KM_RS_NKServiceLevel = "STD";
			AssertNoErrors("STD is a system default service level.", transport.KM_RS_NKServiceLevelInfo);

			transport.KM_RS_NKServiceLevel = "AAA";
			AssertHasError(transport.KM_RS_NKServiceLevelInfo, "Enter a valid Service Level.");

			transport.KM_RS_NKServiceLevel = "";
			AssertNoNotifications(transport.KM_RS_NKServiceLevelInfo);
		}

		#endregion

		#region Implementation

		protected abstract DtbTransport GetNewTransport();
		protected abstract PkgPackageJob GetPackageJob(DtbTransport transport);

		#endregion
	}
}
