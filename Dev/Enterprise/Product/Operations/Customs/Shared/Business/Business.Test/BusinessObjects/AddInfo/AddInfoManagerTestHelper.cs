using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	public sealed class AddInfoManagerTestHelper : TestCaseWithFactory
	{
		public static void AssertWrappedProperty(IAddInfoManager manager, string propertyName, string wrapperPropertyName, IZType validValue)
		{
			const string format = "The value of {0} should be {1} on {2}";

			var addInfoParent = (BusinessObject)manager;
			var addInfo = manager.AddInfo;

			var propertyInfo = (ZWrappedPropertyInfo)addInfoParent.ZPropertyInfoHash
				.GetPropertyInfos(PropertyInfoTypes.Wrapping)
				.Cast<ZPropertyInfo>()
				.First(c => c.Name == propertyName);

			var wrapperInfo = (addInfo is BusinessObject addInfoBizo ? addInfoBizo : addInfoParent).ZPropertyInfoHash.GetPropertySafe(wrapperPropertyName);

			propertyInfo.ClearValue();
			addInfoParent.Factory.Save();

			Assert(string.Format(CultureInfo.InvariantCulture, format, propertyName, "empty", addInfoParent.HumanReadableName), propertyInfo.Value.IsEmpty);
			Assert(string.Format(CultureInfo.InvariantCulture, format, wrapperPropertyName, "empty", addInfo.GetType().Name), wrapperInfo.Value.IsEmpty);

			propertyInfo.Value = validValue;
			addInfoParent.Factory.Save();

			AssertEquals(string.Format(CultureInfo.InvariantCulture, format, propertyName, validValue, addInfoParent.HumanReadableName), validValue, propertyInfo.Value);
			AssertEquals(string.Format(CultureInfo.InvariantCulture, format, wrapperPropertyName, validValue, addInfo.GetType().Name), validValue, wrapperInfo.Value);

			manager.AddInfo.UpdateAddInfoFromString(ZString.Empty);
			addInfoParent.Factory.Save();

			Assert(string.Format(CultureInfo.InvariantCulture, format, propertyName, "empty", addInfoParent.HumanReadableName), propertyInfo.Value.IsEmpty);
			Assert(string.Format(CultureInfo.InvariantCulture, format, wrapperPropertyName, "empty", addInfo.GetType().Name), wrapperInfo.Value.IsEmpty);

			manager.AddInfo.SetValue(wrapperPropertyName, validValue.ToString());
			addInfoParent.Factory.Save();

			AssertEquals(string.Format(CultureInfo.InvariantCulture, format, propertyName, validValue, addInfoParent.HumanReadableName), validValue, propertyInfo.Value);
			AssertEquals(string.Format(CultureInfo.InvariantCulture, format, wrapperPropertyName, validValue, addInfo.GetType().Name), validValue, wrapperInfo.Value);
		}
	}
}
