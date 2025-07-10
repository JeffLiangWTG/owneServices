using Enterprise.Core;
using Enterprise.Integration.Packing;

namespace Enterprise.Packing.Business.Testing
{
	class PackageTemplatePackTypeWrapperTest : PackingTestCaseWithFactory
	{
		public void TestConstructor()
		{
			var packType = Helper.CreateRefPackType("1", "2", 1m, 3m, 5m, Constants.Length.Metres, 7m, Constants.Weight.Kilograms);

			var wrapper = (IPackageTemplate)new PackageTemplatePackTypeWrapper(packType);
			AssertEquals(nameof(IPackageTemplate.Height), 1m, wrapper.Height);
			AssertEquals(nameof(IPackageTemplate.Length), 3m, wrapper.Length);
			AssertEquals(nameof(IPackageTemplate.Width), 5m, wrapper.Width);
			AssertEquals(nameof(IPackageTemplate.TareWeight), 7m, wrapper.TareWeight);

			AssertEquals(nameof(IPackageTemplate.DimensionUQ), Constants.Length.Metres, wrapper.DimensionUQ);
			AssertEquals(nameof(IPackageTemplate.WeightUQ), Constants.Weight.Kilograms, wrapper.WeightUQ);

			AssertNull(nameof(IPackageTemplate.Volume), wrapper.Volume);
			AssertEquals(nameof(IPackageTemplate.WeightUQ), string.Empty, wrapper.VolumeUQ);
		}
	}
}
