using Enterprise.Customs.US.ISF.Business;
using Enterprise.DocumentWrappers.GenericWrappers.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.DocumentWrappers.Testing
{
	[TestedType(typeof(ISFCustomsEntryWrapperCollection))]
	sealed class ISFCustomsEntryWrapperCollectionTest : CustomsEntryWrapperCollectionTest
	{
		public void TestLoadFromCusISFHeader()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.BF_CustomsReference = "XJD43434434";

			var collection = new ISFCustomsEntryWrapperCollection(header, Factory);
			AssertEquals("collection.Count", 1, collection.Count);
		}

		public override void TestCustomsEntryWrapperTypeIndexer()
		{
			var countryCode = Core.Constants.CountryCodes.Eritrea;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				base.TestCustomsEntryWrapperTypeIndexer();
			}
		}
	}
}
