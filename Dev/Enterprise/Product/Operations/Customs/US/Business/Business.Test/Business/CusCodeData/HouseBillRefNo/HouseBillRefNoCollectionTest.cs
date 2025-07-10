using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(HouseBillRefNoCollection))]
	sealed class HouseBillRefNoCollectionTest : Customs.Business.Testing.CusCodeDataCollectionTest<HouseBillRefNo>
	{
		protected override Customs.Business.CusCodeDataCollection<HouseBillRefNo> GetCusCodeDataCollection() => new HouseBillRefNoCollection(Bill);

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<HouseBillRefNo>();
			result.CY_ParentID = Bill.PK;
			result.CY_ParentTableCode = "CU";
			return result;
		}

		Bill bill;
		Bill Bill
		{
			get
			{
				if (bill == null)
				{
					JobDeclaration declaration = Factory.New<JobDeclaration>();
					bill = declaration.Bills.AddNew();
				}

				return bill;
			}
		}
	}
}
