using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.TariffValidation;

namespace Enterprise.Customs.NZ.Business
{
	public class PermitCodeCollection : CodeDataPairCollection, Integration.Customs.NZ.ICodeDataPairCollection
	{
		public PermitCodeCollection(BusinessObjectFactory factory, ZPropertyInfo addInfoPropertyInfo)
			: base(factory, addInfoPropertyInfo)
		{
			MaxCountValidationEnable(ParentDeclaration != null ? 10 : 5);
		}

		public new PermitCode this[int index]
		{
			get { return (PermitCode)base[index]; }
		}

		public new PermitCode AddNew()
		{
			return (PermitCode)base.AddNew();
		}

		protected override CodeDataPair CreateNewCodeInfo()
		{
			return new PermitCode(Factory, this);
		}

		protected override void OnCountChanged(CollectionCountChangedEventArgs e)
		{
			base.OnCountChanged(e);
			if (isInitialised)
			{
				var validationData = Parent as ITariffValidationData ?? (Parent as NZAddInfo)?.ParentObject as ITariffValidationData;
				validationData?.ValidateTariffCode();
			}
		}

		Integration.Customs.NZ.ICodeDataPair Integration.Customs.NZ.ICodeDataPairCollection.this[int index] => this[index];

		Integration.Customs.NZ.ICodeDataPair Integration.Customs.NZ.ICodeDataPairCollection.AddNew() => AddNew();
	}
}
