using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusBrokerageBoxNumberRegistryItem))]
	sealed class CusBrokerageBoxNumberRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<CusBrokerageBoxNumberCollection>
	{
		protected override StronglyTypedRegistryItem<CusBrokerageBoxNumberCollection, CusBrokerageBoxNumberCollection> GetNewRegistryItem() => new CusBrokerageBoxNumberRegistryItem("", null, null, null, RegistryStorageFlags.Company);
		protected override CusBrokerageBoxNumberCollection ValidValue
		{
			get
			{
				var collection = new CusBrokerageBoxNumberCollection();
				var element1 = collection.AddNew();
				element1.BoxNumber = "600";
				element1.CustomsOfficeArea = TaiwanCustomsDistrictList.Codes.A;
				element1.IsDefaultBoxNumber = ZBool.True;
				var element2 = collection.AddNew();
				element2.BoxNumber = "100";
				element2.CustomsOfficeArea = TaiwanCustomsDistrictList.Codes.B;
				element2.IsDefaultBoxNumber = ZBool.True;
				return collection;
			}
		}
	}
}
