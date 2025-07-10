using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public abstract class AddInfoBOTest : NonPersistentBusinessObjectTestCase
	{
		#region TestLookups
		public void TestLookups()
		{
			AddInfo addInfo = (AddInfo)GetNewBusinessObject();
			AssertEquals("Lookups", GetExpectedLookupsType(), addInfo.Lookups.GetType());
		}

		protected abstract Type GetExpectedLookupsType();
		#endregion
		#region TestValidation
		public void TestValidation()
		{
			AddInfo addInfo = (AddInfo)GetNewBusinessObject();
			AssertEquals("Validation", GetExpectedValidationType(), addInfo.Validation.GetType());
		}

		protected abstract Type GetExpectedValidationType();
		#endregion
	}
}
