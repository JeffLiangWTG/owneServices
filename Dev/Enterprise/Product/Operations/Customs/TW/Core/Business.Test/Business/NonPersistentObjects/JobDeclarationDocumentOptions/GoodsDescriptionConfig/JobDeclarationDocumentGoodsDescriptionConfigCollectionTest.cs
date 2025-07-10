using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(JobDeclarationDocumentGoodsDescriptionConfigCollection))]
	sealed class JobDeclarationDocumentGoodsDescriptionConfigCollectionTest : NonPersistentBusinessObjectCollectionTestCase<JobDeclarationDocumentGoodsDescriptionConfigCollection>
	{
		protected override JobDeclarationDocumentGoodsDescriptionConfigCollection GetCollectionToTest()
		{
			return new JobDeclarationDocumentGoodsDescriptionConfigCollection(JobDeclarationDocumentAddressConfig);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var collection = GetCollectionToTest();
			return collection.AddNew();
		}

		JobDeclarationDocumentAddressConfig JobDeclarationDocumentAddressConfig
		{
			get
			{
				if (jobDeclarationDocumentAddressConfig == null)
				{
					var declaration = Factory.NewWithValidTestData<JobDeclaration>();
					jobDeclarationDocumentAddressConfig = new JobDeclarationDocumentAddressConfig(declaration);
				}

				return jobDeclarationDocumentAddressConfig;
			}
		}

		JobDeclarationDocumentAddressConfig jobDeclarationDocumentAddressConfig;
	}
}
