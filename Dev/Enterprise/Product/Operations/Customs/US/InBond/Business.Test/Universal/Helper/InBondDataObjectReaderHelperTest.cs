using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.Customs.US.InBond.Business.Universal.Testing
{
	sealed class InBondDataObjectReaderHelperTest : DataTransfer.Universal.Testing.InBondDataObjectReaderHelperTest<CusInBondHeader>
	{
		public void TestGetCusInBondCargoDescCustomLabelsProvider()
		{
			Helper.SetCusInBondCargoDescCustomLabelsProvider((CusInBondHeader)InBondHeader);
			var provider = Helper.GetCusInBondCargoDescCustomLabelsProvider();
			AssertType<CusInBondCargoDescCustomLabelsProvider>(provider);
		}

		new InBondDataObjectReaderHelper Helper => (InBondDataObjectReaderHelper)base.Helper;

		protected override DataTransfer.Universal.InBondDataObjectReaderHelper CreateHelper(UniversalObjectFactory factory) => new InBondDataObjectReaderHelper(factory);
	}
}
