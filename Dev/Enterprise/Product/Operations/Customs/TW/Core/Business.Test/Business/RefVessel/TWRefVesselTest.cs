using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(TWRefVessel))]
	sealed class TWRefVesselTest : RefVesselZZTest
	{
		public void TestCodeAndDescriptionProperty_ForCodeFindBox()
		{
			var vessel = Factory.New<TWRefVessel>();
			vessel.RV_Code = "RvCode";
			vessel.RV_RadioCallSign = "12345";
			vessel.RV_LloydsNumber = "54321";

			var codeProperty = CodePropertyAttribute.CodePropertyNameFromType(GetExpectedBusinessObjectType());
			var descriptionProperty = DescriptionPropertyAttribute.DescriptionPropertyNameFromType(GetExpectedBusinessObjectType());

			AssertEquals(nameof(vessel.RV_Code), codeProperty);
			AssertEquals(nameof(vessel.RV_RadioCallSign), descriptionProperty);
			AssertEquals("RvCode", CodePropertyAttribute.CodeFromBusinessObject(vessel));
			AssertEquals("12345", DescriptionPropertyAttribute.DescriptionFromBusinessObject(vessel));
		}
	}
}
