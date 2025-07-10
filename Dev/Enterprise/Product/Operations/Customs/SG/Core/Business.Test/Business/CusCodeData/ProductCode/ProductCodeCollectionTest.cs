using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(ProductCodeCollection))]
	public class ProductCodeCollectionTest : CusCodeDataCollectionTest<ProductCode>
	{
		public void TestMaxCount()
		{
			ProductCodeCollection productCodeCollection = new ProductCodeCollection(Classification);
			AssertEquals("The maximum count is 5", 5, productCodeCollection.MaxCount);
		}

		protected override CusCodeDataCollection<ProductCode> GetCusCodeDataCollection()
		{
			return new ProductCodeCollection(Classification);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			ProductCode productCode = Factory.New<ProductCode>();
			productCode.CY_ParentID = Classification.PK;
			productCode.CY_ParentTableCode = Classification.TablePrefix;
			return productCode;
		}

		Classification Classification
		{
			get
			{
				return classification ?? (classification = Factory.New<Classification>());
			}
		}

		Classification classification;
	}
}
