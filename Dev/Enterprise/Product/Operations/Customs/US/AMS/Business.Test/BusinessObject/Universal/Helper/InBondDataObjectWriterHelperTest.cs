namespace Enterprise.Customs.US.AMS.Business.Universal.Testing
{
	sealed class InBondDataObjectWriterHelperTest : DataTransfer.Universal.Testing.InBondDataObjectWriterHelperTest<CusInBondHeader>
	{
		public void TestDirectionTypes()
		{
			AssertEquals(typeof(DirectionTypeList), Helper.DirectionTypes.GetType());
		}

		new InBondDataObjectWriterHelper Helper => (InBondDataObjectWriterHelper)base.Helper;

		protected override DataTransfer.Universal.InBondDataObjectWriterHelper CreateHelper(CusInBondHeader header)
		{
			return new InBondDataObjectWriterHelper(header);
		}
	}
}
