using System;
using CargoWise.EntityFramework;

namespace Enterprise.Workflow.Business
{
	public class EDIMessageContentFilterDocumentValidation : ZValidation
	{
		public EDIMessageContentFilterDocumentValidation(EDIMessageContentFilterDocument parent)
			: base(parent)
		{
			Parent = parent;
		}

		protected EDIMessageContentFilterDocument Parent { get; }

		public override Type AutoValidationType => typeof(EDIMessageContentFilterDocument);

		public override void ValidateAll()
		{
			ValidateDocumentType();
		}

		#region Document Type

		public void ValidateDocumentType() => ValidateCalculatedProperty(Parent.DocumentTypeInfo);

		protected void CheckDocumentType()
		{
			MandatoryValidation.CheckEntered(Parent.DocumentTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.DocumentTypeInfo, Parent.Lookups.DocumentTypes);
		}

		#endregion
	}
}
