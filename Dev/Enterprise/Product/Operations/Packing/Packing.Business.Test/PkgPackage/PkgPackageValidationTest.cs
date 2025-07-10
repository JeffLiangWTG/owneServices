using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.TransportBooking;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Packing.Business.Testing
{
	public class PkgPackageValidationTest : PackingBusinessObjectValidationTestCase
	{
		#region TestDoNotValidateOnFinalizedJob

		public void TestDoNotValidateOnFinalizedJob()
		{
			var dummyPackingParent = Factory.New<DummyWithPacking>();
			dummyPackingParent.JobNoForPackingParent = "abc";

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(dummyPackingParent);
			var package = packageJob.Packages.AddNew();
			packageJob.KJ_IsFinalized = true;
			package.KP_F3_NKPackType = "NO";
			package.KP_PackageQty = 1;
			package.KP_Weight = 0;
			package.KP_Width = 0;
			package.KP_Height = 0;
			package.KP_WeightUQ = "XX";
			package.KP_VolumeUQ = "XX";
			package.KP_DimensionUQ = "XX";
			package.KP_RH_NKCommodityCode = "XX";
			package.KP_RequiredTemperatureMinimum = -1;
			package.KP_RequiredTemperatureMaximum = -1;
			package.KP_RequiredTemperatureUnit = "X";

			AssertNoErrors(package.KP_PackageQtyInfo);
			AssertNoErrors(package.KP_F3_NKPackTypeInfo);
			AssertNoErrors(package.KP_WeightInfo);
			AssertNoErrors(package.KP_VolumeInfo);
			AssertNoErrors(package.KP_LengthInfo);
			AssertNoErrors(package.KP_WidthInfo);
			AssertNoErrors(package.KP_HeightInfo);
			AssertNoErrors(package.KP_WeightUQInfo);
			AssertNoErrors(package.KP_VolumeUQInfo);
			AssertNoErrors(package.KP_DimensionUQInfo);
			AssertNoErrors(package.KP_PackageIDInfo);
			AssertNoErrors(package.KP_PreviousPackageIDInfo);
			AssertNoErrors(package.KP_ReleasedTimeUtcInfo);
			AssertNoErrors(package.KP_IsReleasedViaJobInfo);
			AssertNoErrors(package.KP_RH_NKCommodityCodeInfo);
			AssertNoErrors(package.KP_RequiredTemperatureMinimumInfo);
			AssertNoErrors(package.KP_RequiredTemperatureMaximumInfo);
			AssertNoErrors(package.KP_RequiredTemperatureUnitInfo);

			// Need to clear these of invalid values, otherwise DB constraint will cause error
			package.KP_WeightUQ = "";
			package.KP_VolumeUQ = "";
			package.KP_DimensionUQ = "";

			Factory.Save();
			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var packageInNewFactory = newFactory.Load<PkgPackage>(package.PK);
			packageInNewFactory.Validation.ValidateAll();
			AssertNoErrors(packageInNewFactory.KP_PackageQtyInfo);
			AssertNoErrors(packageInNewFactory.KP_F3_NKPackTypeInfo);
			AssertNoErrors(packageInNewFactory.KP_WeightInfo);
			AssertNoErrors(packageInNewFactory.KP_VolumeInfo);
			AssertNoErrors(packageInNewFactory.KP_LengthInfo);
			AssertNoErrors(packageInNewFactory.KP_WidthInfo);
			AssertNoErrors(packageInNewFactory.KP_HeightInfo);
			AssertNoErrors(packageInNewFactory.KP_WeightUQInfo);
			AssertNoErrors(packageInNewFactory.KP_VolumeUQInfo);
			AssertNoErrors(packageInNewFactory.KP_DimensionUQInfo);
			AssertNoErrors(packageInNewFactory.KP_PackageIDInfo);
			AssertNoErrors(packageInNewFactory.KP_PreviousPackageIDInfo);
			AssertNoErrors(packageInNewFactory.KP_ReleasedTimeUtcInfo);
			AssertNoErrors(packageInNewFactory.KP_IsReleasedViaJobInfo);
			AssertNoErrors(packageInNewFactory.KP_RH_NKCommodityCodeInfo);
			AssertNoErrors(packageInNewFactory.KP_RequiredTemperatureMinimumInfo);
			AssertNoErrors(packageInNewFactory.KP_RequiredTemperatureMaximumInfo);
			AssertNoErrors(packageInNewFactory.KP_RequiredTemperatureUnitInfo);
		}

		#endregion

		#region TestValidateKP_PackageQty

		public void TestValidateKP_PackageQty()
		{
			var dummyPackingParent = Factory.New<DummyWithPacking>();
			dummyPackingParent.JobNoForPackingParent = "abc";

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(dummyPackingParent);
			var package = packageJob.Packages.AddNew();

			package.KP_PackageQty = 1;
			AssertNoErrors(package.KP_PackageQtyInfo);

			package.KP_PackageQty = 0;
			AssertHasError(package.KP_PackageQtyInfo, "Package quantity cannot be less than 1");

			package.KP_PackageQty = -1;
			AssertHasError(package.KP_PackageQtyInfo, "Package quantity cannot be less than 1");
		}

		#endregion

		#region TestCheckKP_IsReleased

		public void TestCheckKP_IsReleased()
		{
			var dummyPackingParent = Factory.New<DummyWithPacking>();
			dummyPackingParent.JobNoForPackingParent = "abc";

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(dummyPackingParent);
			var package = packageJob.Packages.AddNew();

			package.KP_IsHeld = false;
			package.KP_ReleasedTimeUtc = ZDateTime.UtcNow;
			AssertNoErrors(package.KP_PackageQtyInfo);

			package.KP_ReleasedTimeUtc = ZDateTime.Empty;
			package.KP_IsHeld = false;
			AssertNoErrors(package.KP_PackageQtyInfo);

			package.KP_IsHeld = true;
			package.KP_ReleasedTimeUtc = ZDateTime.Empty;
			AssertNoErrors(package.KP_ReleasedTimeUtcInfo);

			package.KP_IsHeld = true;
			package.KP_ReleasedTimeUtc = ZDateTime.UtcNow;
			AssertHasError(package.KP_ReleasedTimeUtcInfo, "A package that is held cannot be released");
		}

		#endregion

		#region TestCheckKP_PackageID

		public void TestCheckKP_PackageID()
		{
			var dtbBookingConsolidationPackingParent = Factory.New<IDtbBookingConsolidation>();
			var booking = Factory.NewWithValidTestData(ObjectFactory.GetType<IDtbBooking>());
			booking[DtbBookingSchema.KM_KB_Booking] = dtbBookingConsolidationPackingParent.PK;
			var instruction = Factory.New<IDtbBookingInstruction>();
			instruction.KN_KM_BookingMovement = booking.PK;
			var instructionDivot = instruction.PackageDivots.AddNew();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(dtbBookingConsolidationPackingParent as IPackingParent);
			var package = packageJob.Packages.AddNew();
			package.KP_F3_NKPackType = Constants.PkgUnit.Container;
			dtbBookingConsolidationPackingParent.KB_JobDirection = nameof(DtbBookingDirection.DLV);
			instruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			instruction.OrganisationType = OrganisationTypesList.Codes.CTO;
			instruction.KN_Sequence = 1;
			package.KP_PackageID = "";
			instructionDivot.KD_KP_Package = package.PK;

			AssertEquals("Precondition: IsSendingXUSToCTO is false", false, ((IDtbBooking)booking).IsSendingXUSToCTO);
			AssertNoNotifications("Precondition: Package ID doesn't have any kind of notification as ValidateForSendingXUSToCTO has not been run", package.KP_F3_NKPackTypeInfo);
			booking[DtbBookingSchema.KM_Status] = "ACR"; // TransportStatuses.Codes.ActionRequired. Simulate ValidateForSendingXUSToCTO being run.
			AssertEquals("Precondition: IsSendingXUSToCTO is true", true, ((IDtbBooking)booking).IsSendingXUSToCTO);
			package.Validation.ValidateKP_PackageID();

			var expectedErrorMessage = "Container must have a container number.";

			AssertHasMessageError("Should have message error for Package ID", package.KP_PackageIDInfo, expectedErrorMessage);

			instructionDivot.KD_KP_Package = ZGuid.Empty;
			package.Validation.ValidateKP_PackageID();
			AssertNoNotifications("Package ID should not have any notifications as the package is not assigned to an instruction", package.KP_PackageIDInfo);

			instructionDivot.KD_KP_Package = package.PK;
			package.KP_F3_NKPackType = Constants.PkgUnit.Package;
			package.Validation.ValidateKP_PackageID();
			AssertNoNotifications("Package ID should not have any notifications as the package is not a container", package.KP_PackageIDInfo);

			package.KP_F3_NKPackType = Constants.PkgUnit.Container;
			dtbBookingConsolidationPackingParent.KB_JobDirection = nameof(DtbBookingDirection.PIC);
			package.Validation.ValidateKP_PackageID();
			AssertNoNotifications("Package ID should not have any notifications as the consolidation direction is not 'DLV'", package.KP_PackageIDInfo);

			dtbBookingConsolidationPackingParent.KB_JobDirection = nameof(DtbBookingDirection.DLV);
			instruction.OrganisationType = OrganisationTypesList.Codes.CYD;
			package.Validation.ValidateKP_PackageID();
			AssertNoNotifications("Package ID should not have any notifications as the organisation type is not 'CTO'", package.KP_PackageIDInfo);

			instruction.OrganisationType = OrganisationTypesList.Codes.CTO;
			instruction.KN_InstructionType = InstructionTypes.Codes.Delivery;
			package.Validation.ValidateKP_PackageID();
			AssertNoNotifications("Package ID should not have any notifications as the instruction type is not 'PIC'", package.KP_PackageIDInfo);
		}

		#endregion

		#region TestCheckKP_F3_NKPackType

		public void TestCheckKP_F3_NKPackType()
		{
			var dtbBookingConsolidationPackingParent = Factory.New<IDtbBookingConsolidation>();
			var booking = Factory.NewWithValidTestData(ObjectFactory.GetType<IDtbBooking>());
			booking[DtbBookingSchema.KM_KB_Booking] = dtbBookingConsolidationPackingParent.PK;
			var instruction = Factory.New<IDtbBookingInstruction>();
			instruction.KN_KM_BookingMovement = booking.PK;
			var instructionDivot = instruction.PackageDivots.AddNew();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(dtbBookingConsolidationPackingParent as IPackingParent);
			var package = packageJob.Packages.AddNew();
			package.KP_F3_NKPackType = Constants.PkgUnit.Package;
			instructionDivot.KD_KP_Package = package.PK;

			var expectedErrorMessage = "This package must be a container.";

			AssertEquals("Precondition: IsSendingXUSToCTO is false", false, ((IDtbBooking)booking).IsSendingXUSToCTO);
			AssertNoNotifications("Precondition: Package Type doesn't have any kind of notification as ValidateForSendingXUSToCTO has not been run", package.KP_F3_NKPackTypeInfo);
			booking[DtbBookingSchema.KM_Status] = "ACR"; // TransportStatuses.Codes.ActionRequired. Simulate ValidateForSendingXUSToCTO being run.
			AssertEquals("Precondition: IsSendingXUSToCTO is true", true, ((IDtbBooking)booking).IsSendingXUSToCTO);
			package.Validation.ValidateKP_F3_NKPackType();

			AssertHasMessageError("Should have message error for Package Type", package.KP_F3_NKPackTypeInfo, expectedErrorMessage);

			package.KP_F3_NKPackType = Constants.PkgUnit.Container;

			AssertNoNotifications("Package Type should not have any notifications as the package is a container", package.KP_F3_NKPackTypeInfo);
		}

		#endregion

		#region TestCheckRTUSLabelPrinterPK

		public void TestCheckRTUSLabelPrinterPK()
		{
			var package = Factory.New<PkgPackage>();
			AssertNoErrors("Precondition", package.RTUSLabelPrinterPKInfo);

			package.RTUSLabelPrinterPK = ZGuid.Invalid;
			AssertHasError(package.RTUSLabelPrinterPKInfo, "Enter a valid Carrier Label Printer.");

			package.RTUSLabelPrinterPK = ZGuid.Empty;
			AssertNoErrors(package.RTUSLabelPrinterPKInfo);
		}

		#endregion

		#region TestReleasedPackageCannotBeHold_CheckInDB

		[ExpectNoExceptions]
		public void TestReleasedPackageCannotBeHold_CheckInDB()
		{
			ReleasedPackageCannotBeHeldCore(isReleased: false, isHeld: false, expectError: false);
			ReleasedPackageCannotBeHeldCore(isReleased: true, isHeld: false, expectError: false);
			ReleasedPackageCannotBeHeldCore(isReleased: false, isHeld: true, expectError: false);
			ReleasedPackageCannotBeHeldCore(isReleased: true, isHeld: true, expectError: true);
		}

		void ReleasedPackageCannotBeHeldCore(bool isReleased, bool isHeld, bool expectError)
		{
			var dummyPackingParent = Factory.New<DummyWithPacking>();
			dummyPackingParent.JobNoForPackingParent = "abc";

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(dummyPackingParent);
			var package = packageJob.Packages.AddNew();

			package.KP_IsHeld = isHeld;
			package.KP_ReleasedTimeUtc = isReleased ? ZDateTime.UtcNow : ZDateTime.Empty;

			if (expectError)
			{
				Factory.SuspendValidation(); // in order to make sure database does checking if bypass the validation
				NUnit.Framework.Assert.That(() => Factory.Save(), CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "The INSERT statement conflicted with the CHECK constraint \"Constraint_KP_IsHeld_KP_ReleasedTimeUtc\"", true), "Should not be able to save when package is held and is released");
				Factory.ResumeValidation();
			}
			else
			{
				AssertNoExceptionThrown(() => Factory.Save());
			}
		}

		#endregion

		#region TestShouldValidateFKToCancelledRecord

		public void TestShouldValidateFKToCancelledRecord()
		{
			var package = Factory.New<PkgPackage>();
			var validation = new TestPkgPackageValidation(package);
			AssertEquals("Pack Type cannot be cancelled.", false, validation.ShouldValidateFKToCancelledRecordExposed(package.KP_F3_NKPackTypeInfo));
			AssertEquals("Package Job cannot be cancelled.", false, validation.ShouldValidateFKToCancelledRecordExposed(package.KP_KJ_ParentPackageJobInfo));
			AssertEquals("Parent Package cannot be cancelled.", false, validation.ShouldValidateFKToCancelledRecordExposed(package.KP_KP_ParentPackageInfo));
			AssertEquals("Parent Closed By cannot be cancelled.", false, validation.ShouldValidateFKToCancelledRecordExposed(package.KP_GS_NKClosedByInfo));
			AssertEquals("Parent Released By cannot be cancelled.", false, validation.ShouldValidateFKToCancelledRecordExposed(package.KP_GS_NKReleasedByInfo));

			foreach (var propertyInfo in package.ZPropertyInfoHash
				.Cast<ZPropertyInfo>()
				.Where(p => p.IsPersistent
					&& p.Name != nameof(package.KP_F3_NKPackType)
					&& p.Name != nameof(package.KP_KJ_ParentPackageJob)
					&& p.Name != nameof(package.KP_KP_ParentPackage)
					&& p.Name != nameof(package.KP_GS_NKClosedBy)
					&& p.Name != nameof(package.KP_GS_NKReleasedBy)))
			{
				AssertEquals("All other properties should just return base condition of true.", true, validation.ShouldValidateFKToCancelledRecordExposed(propertyInfo));
			}
		}

		#endregion

		public void TestValidateAll()
		{
			var package = Factory.New<PkgPackage>();
			AssertNoErrors("Precondition", package.RTUSLabelPrinterPKInfo);

			using (package.GetValidationSuspender())
			{
				package.RTUSLabelPrinterPK = ZGuid.Invalid;
			}

			AssertNoErrors("Precondition", package.RTUSLabelPrinterPKInfo);

			package.Validation.ValidateAll();
			AssertHasError(package.RTUSLabelPrinterPKInfo, "Enter a valid Carrier Label Printer.");
		}

		class TestPkgPackageValidation : PkgPackageValidation
		{
			public TestPkgPackageValidation(PkgPackage parent)
				: base(parent)
			{
			}

			public bool ShouldValidateFKToCancelledRecordExposed(ZPropertyInfo info) => ShouldValidateFKToCancelledRecord(info);
		}
	}
}
