
using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Business
{
	public class ProhibitedCodeCollection : CodeDataPairCollection, Integration.Customs.NZ.ICodeDataPairCollection
	{
		public ProhibitedCodeCollection(BusinessObjectFactory factory, ZPropertyInfo addInfoPropertyInfo)
			: base(factory, addInfoPropertyInfo)
		{
			MaxCountValidationEnable(3);
		}

		public new ProhibitedCode this[int index]
		{
			get { return (ProhibitedCode)base[index]; }
		}

		public new ProhibitedCode AddNew()
		{
			return (ProhibitedCode)base.AddNew();
		}

		protected override CodeDataPair CreateNewCodeInfo()
		{
			return new ProhibitedCode(Factory, this);
		}

		Integration.Customs.NZ.ICodeDataPair Integration.Customs.NZ.ICodeDataPairCollection.this[int index] => this[index];

		Integration.Customs.NZ.ICodeDataPair Integration.Customs.NZ.ICodeDataPairCollection.AddNew() => AddNew();
	}
}
