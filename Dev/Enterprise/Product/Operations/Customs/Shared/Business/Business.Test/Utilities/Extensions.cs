using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	public static class Extensions
	{
		public static void AssertEffectivePropertyWhenChildEntered(this ZPropertyInfo parentPropertyInfo, IZType parentValue, ZPropertyInfo childPropertyInfo, IZType childValue)
		{
			Assertion.Assert(childPropertyInfo.Value.IsEmpty);
			parentPropertyInfo.Value = parentValue;
			Assertion.AssertEquals(parentValue, childPropertyInfo.Value);
			var bizObj = childPropertyInfo.BizObj;
			Assertion.AssertEquals(ZString.Empty, ((IBusinessObjectInternals)bizObj).Row[childPropertyInfo.Name]);
			childPropertyInfo.Value = childValue;
			Assertion.AssertEquals(childValue, childPropertyInfo.Value);
			Assertion.AssertEquals(childValue, ((IBusinessObjectInternals)bizObj).Row[childPropertyInfo.Name]);
		}

		public static void AssertEffectivePropertyWhenParentChanged(this ZPropertyInfo parentPropertyInfo, IZType parentValue, ZPropertyInfo childPropertyInfo, IZType childValue)
		{
			parentPropertyInfo.Value = parentValue;
			childPropertyInfo.Value = childValue;
			var bizObj = childPropertyInfo.BizObj;
			Assertion.AssertEquals(childValue, ((IBusinessObjectInternals)bizObj).Row[childPropertyInfo.Name]);
			parentPropertyInfo.Value = childValue;
			Assertion.AssertEquals(ZString.Empty, ((IBusinessObjectInternals)bizObj).Row[childPropertyInfo.Name]);
		}
	}
}
