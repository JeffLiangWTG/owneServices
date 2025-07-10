using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class JobDeclarationSupportingDocSendingObjectCollection : NonPersistentBusinessObjectCollection<SupportingDocSendingObject>
	{
		public JobDeclarationSupportingDocSendingObjectCollection(BaseJobDeclaration declaration) : base(declaration.Factory)
		{
			this.declaration = declaration;
		}

		protected BaseJobDeclaration declaration;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return CreateSupportingDocSendingObject();
		}

		protected virtual BusinessObject CreateSupportingDocSendingObject()
		{
			return SupportingDocSendingObject.New(declaration);
		}
	}
}
