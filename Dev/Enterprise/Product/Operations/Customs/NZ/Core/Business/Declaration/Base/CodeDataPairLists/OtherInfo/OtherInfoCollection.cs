
using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Business
{
	public abstract class OtherInfoCollection : CodeDataPairCollection, Integration.Customs.NZ.ICodeDataPairCollection
	{
		public OtherInfoCollection(BusinessObjectFactory factory, ZPropertyInfo addInfoPropertyInfo)
			: base(factory, addInfoPropertyInfo)
		{
			MaxCountValidationEnable(ParentDeclaration != null ? 10 : 5);
		}

		Integration.Customs.NZ.ICodeDataPair Integration.Customs.NZ.ICodeDataPairCollection.this[int index] => this[index];

		Integration.Customs.NZ.ICodeDataPair Integration.Customs.NZ.ICodeDataPairCollection.AddNew() => AddNew();
	}
}
