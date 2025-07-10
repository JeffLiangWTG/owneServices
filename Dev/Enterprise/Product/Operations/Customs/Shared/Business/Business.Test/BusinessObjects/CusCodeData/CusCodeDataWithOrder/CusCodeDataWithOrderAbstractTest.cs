namespace Enterprise.Customs.Business.Testing
{
	public abstract class CusCodeDataWithOrderAbstractTest<T> : CusCodeDataTest<T> where T : CusCodeDataWithOrder
	{
		protected abstract string ExpectedCusCodeDataType { get; }

		public void TestSetDefaultValues()
		{
			var code = Factory.New<T>();

			AssertEquals("CY_Type", ExpectedCusCodeDataType, code.CY_Type);
		}
	}
}
