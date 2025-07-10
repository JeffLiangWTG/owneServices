using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccChargeApportionmentMethodOverrideValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateAAM_ApportionmentMethod()
		{
			var apportionmentMethodOverride = Factory.NewWithValidTestData<AccChargeApportionmentMethodOverride>();
			AssertEquals(AllocationMethod.Manual, apportionmentMethodOverride.AAM_ApportionmentMethod);
			AssertCommonValidation(apportionmentMethodOverride.AAM_ApportionmentMethodInfo);
		}

		public void TestValidateAAM_ConsolType()
		{
			var apportionmentMethodOverride = Factory.NewWithValidTestData<AccChargeApportionmentMethodOverride>();
			AssertEquals("ALL", apportionmentMethodOverride.AAM_ConsolType);
			AssertCommonValidation(apportionmentMethodOverride.AAM_ConsolTypeInfo);
			AssertIsDuplicateOf(apportionmentMethodOverride.AAM_ConsolTypeInfo.Name, new ZString("ALL"), new ZString("DRT"));
		}

		public void TestValidateAAM_ContainerMode()
		{
			var apportionmentMethodOverride = Factory.NewWithValidTestData<AccChargeApportionmentMethodOverride>();
			AssertEquals("ALL", apportionmentMethodOverride.AAM_ConsolType);
			AssertCommonValidation(apportionmentMethodOverride.AAM_ContainerModeInfo);
			AssertIsDuplicateOf(apportionmentMethodOverride.AAM_ContainerModeInfo.Name, new ZString("ALL"), new ZString("LCL"));
		}

		public void TestValidateAAM_Direction()
		{
			var apportionmentMethodOverride = Factory.NewWithValidTestData<AccChargeApportionmentMethodOverride>();
			AssertEquals("ALL", apportionmentMethodOverride.AAM_Direction);
			AssertCommonValidation(apportionmentMethodOverride.AAM_DirectionInfo);
			AssertIsDuplicateOf(apportionmentMethodOverride.AAM_DirectionInfo.Name, new ZString("ALL"), new ZString("EXP"));
		}

		public void TestValidateAAM_Module()
		{
			var apportionmentMethodOverride = Factory.NewWithValidTestData<AccChargeApportionmentMethodOverride>();
			AssertEquals("ALL", apportionmentMethodOverride.AAM_Module);
			AssertCommonValidation(apportionmentMethodOverride.AAM_ModuleInfo);
			AssertIsDuplicateOf(apportionmentMethodOverride.AAM_ModuleInfo.Name, new ZString("ALL"), new ZString("FOR"));
		}

		public void TestValidateAAM_TransportMode()
		{
			var apportionmentMethodOverride = Factory.NewWithValidTestData<AccChargeApportionmentMethodOverride>();
			AssertEquals("ALL", apportionmentMethodOverride.AAM_TransportMode);
			AssertCommonValidation(apportionmentMethodOverride.AAM_TransportModeInfo);
			AssertIsDuplicateOf(apportionmentMethodOverride.AAM_TransportModeInfo.Name, new ZString("ALL"), new ZString("AIR"));
		}

		void AssertCommonValidation(ZPropertyInfo propertyInfo)
		{
			AssertEquals("Percondition", 0, propertyInfo.Notifications.Count());
			propertyInfo.Value = ZString.Empty;
			AssertEquals(1, propertyInfo.Notifications.Count());
			AssertHasErrorContaining(propertyInfo, "Please enter a");

			propertyInfo.Value = new ZString("ERR");
			AssertEquals(1, propertyInfo.Notifications.Count());
			AssertHasErrorContaining(propertyInfo, "Enter a valid");

			AssertEquals("Percondition", 3, propertyInfo.MaxLength);
			AssertExceptionThrown($"The maximum length of '{propertyInfo.Name}' has been exceeded. The maximum length of this property is 3 characters, but 4 were entered. New value: 1234. Old value:",
				typeof(Exception),
				() => { propertyInfo.Value = new ZString("1234"); });
			ErrorReporter.Clear();
		}

		void AssertIsDuplicateOf(string propertyInfoName, IZType value1, IZType value2)
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var apportionmentMethodOverride1 = chargeCode.ApportionmentMethodOverrides.AddNew();
			var apportionmentMethodOverride2 = chargeCode.ApportionmentMethodOverrides.AddNew();

			apportionmentMethodOverride1.FindPropertyInfo(propertyInfoName).Value = value1;
			apportionmentMethodOverride2.FindPropertyInfo(propertyInfoName).Value = value1;
			AssertIsDuplicateOfCore(true, true);

			apportionmentMethodOverride1.FindPropertyInfo(propertyInfoName).Value = new ZString("ERR");
			apportionmentMethodOverride2.FindPropertyInfo(propertyInfoName).Value = new ZString("ERR");
			AssertIsDuplicateOfCore(true, false);
			AssertHasErrorContaining(apportionmentMethodOverride1.FindPropertyInfo(propertyInfoName), "Enter a valid");
			AssertHasErrorContaining(apportionmentMethodOverride2.FindPropertyInfo(propertyInfoName), "Enter a valid");

			apportionmentMethodOverride1.FindPropertyInfo(propertyInfoName).Value = value1;
			apportionmentMethodOverride2.FindPropertyInfo(propertyInfoName).Value = value2;
			AssertIsDuplicateOfCore(false, false);
			AssertNoErrors(apportionmentMethodOverride1.FindPropertyInfo(propertyInfoName));
			AssertNoErrors(apportionmentMethodOverride2.FindPropertyInfo(propertyInfoName));

			void AssertIsDuplicateOfCore(bool isDuplicate, bool hasRowError)
			{
				AssertEquals(isDuplicate, apportionmentMethodOverride1.IsDuplicateOf(apportionmentMethodOverride2));
				AssertEquals(isDuplicate, apportionmentMethodOverride2.IsDuplicateOf(apportionmentMethodOverride1));

				((IBusinessObjectInternals)apportionmentMethodOverride1).Validate(propertyInfoName);
				((IBusinessObjectInternals)apportionmentMethodOverride2).Validate(propertyInfoName);
				if (hasRowError)
				{
					AssertHasRowError(apportionmentMethodOverride1, AccChargeApportionmentMethodOverrideValidation.IsDuplicateErrorString);
					AssertHasRowError(apportionmentMethodOverride2, AccChargeApportionmentMethodOverrideValidation.IsDuplicateErrorString);
				}
				else
				{
					AssertNoRowError(apportionmentMethodOverride1, AccChargeApportionmentMethodOverrideValidation.IsDuplicateErrorString);
					AssertNoRowError(apportionmentMethodOverride2, AccChargeApportionmentMethodOverrideValidation.IsDuplicateErrorString);
				}
			}
		}

		public void TestModuleSmartChangeValidation()
		{
			AssertCaseDTB();
			AssertCaseTRW();

			void AssertCaseDTB()
			{
				var apportionmentMethodOverride = Factory.NewWithValidTestData<AccChargeApportionmentMethodOverride>();
				apportionmentMethodOverride.AAM_ConsolType = "OTH";
				apportionmentMethodOverride.AAM_Direction = "OTH";

				apportionmentMethodOverride.AAM_Module = "DTB";
				AssertEquals(apportionmentMethodOverride.AAM_Direction, "ALL");
				AssertNoError(apportionmentMethodOverride.AAM_DirectionInfo, "Enter a valid Direction.");
				AssertEquals(apportionmentMethodOverride.AAM_ConsolType, "ALL");
				AssertNoError(apportionmentMethodOverride.AAM_ConsolTypeInfo, "Enter a valid Consol Type.");

				apportionmentMethodOverride.AAM_Direction = "OTH";
				AssertHasError(apportionmentMethodOverride.AAM_DirectionInfo, "Enter a valid Direction.");
				apportionmentMethodOverride.AAM_ConsolType = "OTH";
				AssertHasError(apportionmentMethodOverride.AAM_ConsolTypeInfo, "Enter a valid Consol Type.");

				apportionmentMethodOverride.AAM_Module = "FOR";
				apportionmentMethodOverride.RunPreSaveValidation();
				AssertNoError(apportionmentMethodOverride.AAM_DirectionInfo, "Enter a valid Direction.");
				AssertNoError(apportionmentMethodOverride.AAM_ConsolTypeInfo, "Enter a valid Consol Type.");
			}

			void AssertCaseTRW()
			{
				var apportionmentMethodOverride = Factory.NewWithValidTestData<AccChargeApportionmentMethodOverride>();
				apportionmentMethodOverride.AAM_ConsolType = "OTH";
				apportionmentMethodOverride.AAM_Direction = "OTH";
				apportionmentMethodOverride.AAM_TransportMode = "SEA";
				apportionmentMethodOverride.AAM_ContainerMode = "OTH";

				apportionmentMethodOverride.AAM_Module = "TRW";
				AssertEquals(apportionmentMethodOverride.AAM_Direction, "ALL");
				AssertNoError(apportionmentMethodOverride.AAM_DirectionInfo, "Enter a valid Direction.");
				AssertEquals(apportionmentMethodOverride.AAM_ConsolType, "ALL");
				AssertNoError(apportionmentMethodOverride.AAM_ConsolTypeInfo, "Enter a valid Consol Type.");
				AssertEquals(apportionmentMethodOverride.AAM_TransportMode, "ALL");
				AssertNoError(apportionmentMethodOverride.AAM_TransportModeInfo, "Enter a valid Transport Mode.");
				AssertEquals(apportionmentMethodOverride.AAM_ContainerMode, "ALL");
				AssertNoError(apportionmentMethodOverride.AAM_ContainerModeInfo, "Enter a valid Container Mode.");

				apportionmentMethodOverride.AAM_Direction = "OTH";
				AssertHasError(apportionmentMethodOverride.AAM_DirectionInfo, "Enter a valid Direction.");
				apportionmentMethodOverride.AAM_ConsolType = "OTH";
				AssertHasError(apportionmentMethodOverride.AAM_ConsolTypeInfo, "Enter a valid Consol Type.");
				apportionmentMethodOverride.AAM_TransportMode = "SEA";
				AssertHasError(apportionmentMethodOverride.AAM_TransportModeInfo, "Enter a valid Transport Mode.");
				apportionmentMethodOverride.AAM_ContainerMode = "OTH";
				AssertHasError(apportionmentMethodOverride.AAM_ContainerModeInfo, "Enter a valid Container Mode.");

				apportionmentMethodOverride.AAM_Module = "FOR";
				apportionmentMethodOverride.RunPreSaveValidation();
				AssertNoError(apportionmentMethodOverride.AAM_DirectionInfo, "Enter a valid Direction.");
				AssertNoError(apportionmentMethodOverride.AAM_ConsolTypeInfo, "Enter a valid Consol Type.");
				AssertNoError(apportionmentMethodOverride.AAM_TransportModeInfo, "Enter a valid Transport Mode.");
				AssertNoError(apportionmentMethodOverride.AAM_ContainerModeInfo, "Enter a valid Container Mode.");
			}
		}
	}
}
