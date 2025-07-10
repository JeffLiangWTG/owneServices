using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Workflow.Business
{
	public class EDIMessageContentFilterDocument : NonPersistentBusinessObject
	{
		public EDIMessageContentFilterDocument(EDIMessageContentFilterSpec parent)
			: base(parent.Factory)
		{
			Parent = parent;
		}

		public EDIMessageContentFilterSpec Parent { get; }

		#region Properties

		#region DocumentType

		[XmlColumnProperty]
		[List("Lookups.DocumentTypes")]
		[ResourceStringData("EDIMessageContentFilterDocument.DocumentType", Caption = "Document Type", ShortCaption = "Doc Type")]
		public ZString DocumentType
		{
			get { return GetXmlColumnPropertyValue<ZString>(DocumentTypeInfo); }
			set { SetXmlColumnPropertyValue(DocumentTypeInfo, value.TrimEnd(), Validation.ValidateDocumentType); }
		}

		public ZPropertyInfo DocumentTypeInfo => GetZPropertyInfo(nameof(DocumentType));

		internal ZString DocManagerCode
		{
			get
			{
				switch (Parent.Schema)
				{
					case EDIMessageContentFilterLineSchemas.Codes.UniversalShipment:
						return Core.Constants.DocManagerCodes.Shipment;
					default:
						return ZString.Empty;
				}
			}
		}

		#endregion

		#region Description

		[ResourceStringData("EDIMessageContentFilterDocument.Description", Caption = "Description", ShortCaption = "Desc.")]
		public ZString Description => Lookups.DocumentTypes.GetDescriptionFromCode(DocumentType) ?? ZString.Empty;

		#endregion

		#region Reference

		[ResourceStringData("EDIMessageContentFilterDocument.ReferenceType", Caption = "Category Code", ShortCaption = "Category Code")]
		public ZString ReferenceType => Lookups.ReferenceTypes.GetDescriptionFromCode(DocumentType) ?? ZString.Empty;

		[ResourceStringData("EDIMessageContentFilterDocument.ReferenceDescription", Caption = "Category", ShortCaption = "Category")]
		public ZString ReferenceDescription => Lookups.ReferenceDescriptions.GetDescriptionFromCode(ReferenceType) ?? ZString.Empty;
		#endregion

		#endregion

		#region Lookups

		public EDIMessageContentFilterDocumentLookups Lookups => lookups ?? (lookups = new EDIMessageContentFilterDocumentLookups(this));
		EDIMessageContentFilterDocumentLookups lookups;

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public EDIMessageContentFilterDocumentValidation Validation
		{
			get { return GetNewValidation(); }
		}

		protected virtual EDIMessageContentFilterDocumentValidation GetNewValidation()
		{
			return new EDIMessageContentFilterDocumentValidation(this);
		}

		#endregion
	}
}
