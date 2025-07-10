using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class MultipleDataDataCaptionSourceTest : TestCaseWithFactory
	{
		public void TestGetMaxLength()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_NVarChar = "123";
			dummy.Z0_Code = "456";
			var source = new MultipleDataCaptionSource(new[] { dummy.Z0_NVarCharInfo, dummy.Z0_CodeInfo }, ZString.Empty);

			AssertEquals(dummy.Z0_CodeInfo.MaxLength, source.GetMaxLength("456"));
			AssertEquals(dummy.Z0_NVarCharInfo.MaxLength, source.GetMaxLength("123"));
		}

		public void TestGetRuntimeCaptions()
		{
			var obj1 = Factory.New<TranslatableBizObj>();
			obj1.Z0_Code = "AAA";
			var obj2 = Factory.New<TranslatableBizObj>();
			obj2.Z0_Code = "BBB";
			var obj3 = Factory.New<TranslatableBizObj>();
			obj3.Z0_Code = "CCC";
			var obj4 = Factory.New<TranslatableBizObj>();
			obj4.Z0_Code = "BBB";
			Factory.Save();

			var source = new MultipleDataCaptionSource(new[] { obj1.Z0_CodeInfo, obj2.Z0_CodeInfo, obj3.Z0_CodeInfo, obj4.Z0_CodeInfo }, ZString.Empty);
			var captions = string.Join("\r\n", source.GetRuntimeCaptions().Select(x => $"{x.ResourceKey}-{x.EnglishText}"));
			var expectedCaptions =
@"Z0_Code@AAA  $QUFB-AAA
Z0_Code@BBB  $QkJC-BBB
Z0_Code@CCC  $Q0ND-CCC";
			AssertEquals(expectedCaptions, captions);

			source = new MultipleDataCaptionSource(new[] { obj1.Z0_CodeInfo, obj2.Z0_CodeInfo }, ZString.Empty);
			captions = string.Join("\r\n", source.GetRuntimeCaptions().Select(x => $"{x.ResourceKey}-{x.EnglishText}"));
			expectedCaptions =
@"Z0_Code@AAA  $QUFB-AAA
Z0_Code@BBB  $QkJC-BBB";
			AssertEquals(expectedCaptions, captions);
		}
	}
}
