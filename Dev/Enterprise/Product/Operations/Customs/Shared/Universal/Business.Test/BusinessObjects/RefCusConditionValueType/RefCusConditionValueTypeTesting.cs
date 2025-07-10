using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusConditionValueType))]
	public class RefCusConditionValueTypeTesting : EnterpriseBusinessObjectTestCase
	{
		#region Overrides of BusinessObjectBaseTestCase
		protected override BusinessObject GetNewBusinessObject()
		{
			return new RefCusConditionCreatorForTest(Factory).CreateRefCusConditionValueType();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}
		#endregion
	}
}
