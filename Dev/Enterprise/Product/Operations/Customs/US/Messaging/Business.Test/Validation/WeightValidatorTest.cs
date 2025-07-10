using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	sealed class WeightValidatorTest : TestCaseWithFactory
	{
		public void TestValidateWeight()
		{
			var validator = new WeightValidator();
			DummyBusinessObject obj = Factory.New<DummyBusinessObject>();
			validator.ValidateWeight(ZWeight.Empty, obj.Z0_AnotherDecimalInfo);
			AssertHasMessageError(obj.Z0_AnotherDecimalInfo, ValidationConstants.Weight.WeightMustBeGreaterThanZero);
			obj.Z0_AnotherDecimalInfo.ClearAllNotifications();
			validator.ValidateWeight(new ZWeight(99999999999m, Core.Constants.Weight.Pounds), obj.Z0_AnotherDecimalInfo);
			AssertNoMessageError(obj.Z0_AnotherDecimalInfo, ValidationConstants.Weight.WeightMustBeGreaterThanZero);
			AssertHasMessageErrorContaining(obj.Z0_AnotherDecimalInfo, "is greater than the maximum allowed pounds '9,999,999,999'.");
			obj.Z0_AnotherDecimalInfo.ClearAllNotifications();
			validator.ValidateWeight(new ZWeight(425.56m, Core.Constants.Weight.Pounds), obj.Z0_AnotherDecimalInfo);
			AssertNoMessageErrorContaining(obj.Z0_AnotherDecimalInfo, "is greater than the maximum allowed pounds '9,999,999,999'.");
			AssertHasWarning(obj.Z0_AnotherDecimalInfo, ValidationConstants.Weight.WeightUQInWholePounds);
			obj.Z0_AnotherDecimalInfo.ClearAllNotifications();
			validator.ValidateWeight(new ZWeight(8987544m, Core.Constants.Weight.Pounds), obj.Z0_AnotherDecimalInfo);
			AssertNoWarning(obj.Z0_AnotherDecimalInfo, ValidationConstants.Weight.WeightUQInWholePounds);
			obj.Z0_AnotherDecimalInfo.ClearAllNotifications();
			validator.ValidateWeight(new ZWeight(99999999999m, Core.Constants.Weight.Kilograms), obj.Z0_AnotherDecimalInfo);
			AssertHasMessageErrorContaining(obj.Z0_AnotherDecimalInfo, "is greater than the maximum allowed kilograms '9,999,999,999'.");
			obj.Z0_AnotherDecimalInfo.ClearAllNotifications();
			validator.ValidateWeight(new ZWeight(425.56m, Core.Constants.Weight.Kilograms), obj.Z0_AnotherDecimalInfo);
			AssertNoMessageErrorContaining(obj.Z0_AnotherDecimalInfo, "is greater than the maximum allowed kilograms '9,999,999,999'.");
			AssertHasWarning(obj.Z0_AnotherDecimalInfo, ValidationConstants.Weight.WeightUQInWholeKilograms);
			obj.Z0_AnotherDecimalInfo.ClearAllNotifications();
			validator.ValidateWeight(new ZWeight(8987544m, Core.Constants.Weight.Kilograms), obj.Z0_AnotherDecimalInfo);
			AssertNoWarning(obj.Z0_AnotherDecimalInfo, ValidationConstants.Weight.WeightUQInWholeKilograms);
			obj.Z0_AnotherDecimalInfo.ClearAllNotifications();
			validator.ValidateWeight(new ZWeight(999999999m, Core.Constants.Weight.Kilotonnes), obj.Z0_AnotherDecimalInfo);
			AssertHasMessageErrorContaining(obj.Z0_AnotherDecimalInfo, "is greater than the maximum allowed kilograms '9,999,999,999'.");
			obj.Z0_AnotherDecimalInfo.ClearAllNotifications();
			validator.ValidateWeight(new ZWeight(425.56m, Core.Constants.Weight.Grams), obj.Z0_AnotherDecimalInfo);
			AssertNoMessageErrorContaining(obj.Z0_AnotherDecimalInfo, "is greater than the maximum allowed kilograms '9,999,999,999'.");
			AssertNoWarning(obj.Z0_AnotherDecimalInfo, ValidationConstants.Weight.WeightUQInWholeKilograms);
			if (ErrorReporter.LastKeyReported == "CheckValidationActionClear:Z0_AnotherDecimal")
			{
				ErrorReporter.Clear();
			}
		}

		public void TestValidateWeightUQ()
		{
			var validator = new WeightValidator();
			DummyBusinessObject obj = Factory.New<DummyBusinessObject>();
			validator.ValidateWeightUQ(new ZWeight(425.56m, ZString.Empty), obj.Z0_VarCharMaxInfo);
			AssertHasMessageErrorContaining(obj.Z0_VarCharMaxInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoWarning(obj.Z0_VarCharMaxInfo, ValidationConstants.Weight.WeightUQTypeAllowed);
			foreach (string code in new string[] { Core.Constants.Weight.Kilograms, Core.Constants.Weight.Pounds })
			{
				obj.Z0_VarCharMaxInfo.ClearAllNotifications();
				validator.ValidateWeightUQ(new ZWeight(425.56m, code), obj.Z0_VarCharMaxInfo);
				AssertNoWarning(obj.Z0_VarCharMaxInfo, ValidationConstants.Weight.WeightUQTypeAllowed);
			}

			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.Weight);
			list.RemoveCode(Core.Constants.Weight.Kilograms);
			list.RemoveCode(Core.Constants.Weight.Pounds);
			foreach (CodeDescriptionPair pair in list)
			{
				obj.Z0_VarCharMaxInfo.ClearAllNotifications();
				validator.ValidateWeightUQ(new ZWeight(425.56m, pair.Code), obj.Z0_VarCharMaxInfo);
				AssertHasWarning(obj.Z0_VarCharMaxInfo, ValidationConstants.Weight.WeightUQTypeAllowed);
			}

			if (ErrorReporter.LastKeyReported == "CheckValidationActionClear:Z0_VarCharMax")
			{
				ErrorReporter.Clear();
			}
		}
	}
}
