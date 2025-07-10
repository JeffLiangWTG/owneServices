using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class MultiJobDeclarationHeader : NonPersistentBusinessObject, IObsoleteValidation
	{
		public MultiJobDeclarationHeader(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public MultiJobDeclarationCollection Collection
		{
			get
			{
				if (collection == null)
				{
					collection = new MultiJobDeclarationCollection(Declaration, Factory);
					RegisterEditableChildObject(collection);
				}
				return collection;
			}
		}
		MultiJobDeclarationCollection collection;

		public BaseJobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = new BusinessObjectFactory().New<BaseJobDeclaration>();
					RegisterEditableChildObject(declaration);
				}
				return declaration;
			}
		}

		BaseJobDeclaration declaration;

		protected virtual BaseJobDeclaration CreateDeclaration()
		{
			BaseJobDeclaration result = Factory.New<BaseJobDeclaration>();
			return result;
		}
	}
}
