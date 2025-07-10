using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(VPBAmountCodeDataCollection))]
	sealed class VPBAmountCodeDataCollectionTest : CusCodeDataCollectionTest<VPBAmountCodeData>
	{
		protected override CusCodeDataCollection<VPBAmountCodeData> GetCusCodeDataCollection() => new VPBAmountCodeDataCollection(Factory.New<CUSDECEDIMessage>());

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<VPBAmountCodeData>();
			var message = Factory.New<CUSDECEDIMessage>();
			result.CY_ParentID = message.PK;
			result.CY_ParentTableCode = message.TablePrefix;
			return result;
		}
	}
}
