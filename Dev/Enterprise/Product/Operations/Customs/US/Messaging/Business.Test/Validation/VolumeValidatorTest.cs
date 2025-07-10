using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	sealed class VolumeValidatorTest : TestCaseWithFactory
	{
		public void TestValidateVolume()
		{
			var validator = new VolumeValidator();
			DummyBusinessObject obj = Factory.New<DummyBusinessObject>();
			validator.ValidateVolume(ZVolume.Empty, obj.Z0_AnotherDecimalInfo);
			AssertNoMessageError(obj.Z0_AnotherDecimalInfo, ValidationConstants.Volume.VolumeMustBeGreaterThanZero);
			obj.Z0_AnotherDecimalInfo.ClearAllNotifications();
			validator.ValidateVolume(new ZVolume(-12m, ""), obj.Z0_AnotherDecimalInfo);
			AssertHasMessageError(obj.Z0_AnotherDecimalInfo, ValidationConstants.Volume.VolumeMustBeGreaterThanZero);
			obj.Z0_AnotherDecimalInfo.ClearAllNotifications();
			validator.ValidateVolume(new ZVolume(validator.MaximumWholeVolumeAllowed, Core.Constants.Volume.CubicFeet), obj.Z0_AnotherDecimalInfo);
			AssertNoMessageError(obj.Z0_AnotherDecimalInfo, ValidationConstants.Volume.VolumeMustBeGreaterThanZero);
			AssertNoMessageErrorContaining(obj.Z0_AnotherDecimalInfo, "is greater than the maximum allowed cubic feet '9,999,999,999'.");
			obj.Z0_AnotherDecimalInfo.ClearAllNotifications();
			validator.ValidateVolume(new ZVolume(validator.MaximumWholeVolumeAllowed + 1, Core.Constants.Volume.CubicFeet), obj.Z0_AnotherDecimalInfo);
			AssertNoMessageError(obj.Z0_AnotherDecimalInfo, ValidationConstants.Volume.VolumeMustBeGreaterThanZero);
			AssertHasMessageErrorContaining(obj.Z0_AnotherDecimalInfo, "is greater than the maximum allowed cubic feet '9,999,999,999'.");
			obj.Z0_AnotherDecimalInfo.ClearAllNotifications();
			validator.ValidateVolume(new ZVolume(425.56m, Core.Constants.Volume.CubicFeet), obj.Z0_AnotherDecimalInfo);
			AssertNoMessageErrorContaining(obj.Z0_AnotherDecimalInfo, "is greater than the maximum allowed cubic feet '9,999,999,999'.");
			AssertHasWarning(obj.Z0_AnotherDecimalInfo, ValidationConstants.Volume.VolumeUQInWholeCubicFeet);
			obj.Z0_AnotherDecimalInfo.ClearAllNotifications();
			validator.ValidateVolume(new ZVolume(8987544m, Core.Constants.Volume.CubicFeet), obj.Z0_AnotherDecimalInfo);
			AssertNoWarning(obj.Z0_AnotherDecimalInfo, ValidationConstants.Volume.VolumeUQInWholeCubicFeet);
			obj.Z0_AnotherDecimalInfo.ClearAllNotifications();
			validator.ValidateVolume(new ZVolume(validator.MaximumWholeVolumeAllowed, Core.Constants.Volume.CubicMetres), obj.Z0_AnotherDecimalInfo);
			AssertNoMessageErrorContaining(obj.Z0_AnotherDecimalInfo, "is greater than the maximum allowed cubic meters '9,999,999,999'.");
			obj.Z0_AnotherDecimalInfo.ClearAllNotifications();
			validator.ValidateVolume(new ZVolume(validator.MaximumWholeVolumeAllowed + 1, Core.Constants.Volume.CubicMetres), obj.Z0_AnotherDecimalInfo);
			AssertHasMessageErrorContaining(obj.Z0_AnotherDecimalInfo, "is greater than the maximum allowed cubic meters '9,999,999,999'.");
			obj.Z0_AnotherDecimalInfo.ClearAllNotifications();
			validator.ValidateVolume(new ZVolume(425.56m, Core.Constants.Volume.CubicMetres), obj.Z0_AnotherDecimalInfo);
			AssertNoMessageErrorContaining(obj.Z0_AnotherDecimalInfo, "is greater than the maximum allowed cubic meters '9,999,999,999'.");
			AssertHasWarning(obj.Z0_AnotherDecimalInfo, ValidationConstants.Volume.VolumeUQInWholeCubicMetres);
			obj.Z0_AnotherDecimalInfo.ClearAllNotifications();
			validator.ValidateVolume(new ZVolume(8987544m, Core.Constants.Volume.CubicMetres), obj.Z0_AnotherDecimalInfo);
			AssertNoWarning(obj.Z0_AnotherDecimalInfo, ValidationConstants.Volume.VolumeUQInWholeCubicMetres);
			obj.Z0_AnotherDecimalInfo.ClearAllNotifications();
			validator.ValidateVolume(new ZVolume(validator.MaximumWholeVolumeAllowed, Core.Constants.Volume.MegaLitre), obj.Z0_AnotherDecimalInfo);
			AssertHasMessageErrorContaining(obj.Z0_AnotherDecimalInfo, "is greater than the maximum allowed cubic meters '9,999,999,999'.");
			obj.Z0_AnotherDecimalInfo.ClearAllNotifications();
			validator.ValidateVolume(new ZVolume(425.56m, Core.Constants.Volume.Litre), obj.Z0_AnotherDecimalInfo);
			AssertNoMessageErrorContaining(obj.Z0_AnotherDecimalInfo, "is greater than the maximum allowed cubic meters '9,999,999,999'.");
			AssertNoWarning(obj.Z0_AnotherDecimalInfo, ValidationConstants.Volume.VolumeUQInWholeCubicMetres);
			if (ErrorReporter.LastKeyReported == "Validation:Z0_AnotherDecimal")
			{
				ErrorReporter.Clear();
			}
		}

		public void TestValidateVolumeUQ()
		{
			var validator = new VolumeValidator();
			DummyBusinessObject obj = Factory.New<DummyBusinessObject>();
			validator.ValidateVolumeUQ(new ZVolume(425.56m, ZString.Empty), obj.Z0_VarCharMaxInfo);
			AssertHasMessageErrorContaining(obj.Z0_VarCharMaxInfo, MandatoryValidation.YouHaveNotEntered);
			obj.Z0_VarCharMaxInfo.ClearAllNotifications();
			if (ErrorReporter.LastKeyReported == "CheckValidationActionClear:Z0_VarCharMax")
			{
				ErrorReporter.Clear();
			}
		}
	}
}
