using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Workflow.Business
{
	public class EDIMessageContentFilterDocumentLookups : ZLookups
	{
		public EDIMessageContentFilterDocumentLookups(EDIMessageContentFilterDocument parent)
			: base(parent)
		{
			Parent = parent;
		}

		public new EDIMessageContentFilterDocument Parent { get; }

		public IRefDocTypeCollection RefDocTypes
		{
			get
			{
				UpdateLookupCacheIfRequired();
				return refDocTypes;
			}
		}
		IRefDocTypeCollection refDocTypes;

		public ICodeDescriptionPairList DocumentTypes
		{
			get
			{
				UpdateLookupCacheIfRequired();
				return documentTypes;
			}
		}
		ICodeDescriptionPairList documentTypes;

		public ICodeDescriptionPairList ReferenceTypes
		{
			get
			{
				UpdateLookupCacheIfRequired();
				return referenceTypes;
			}
		}
		ICodeDescriptionPairList referenceTypes;

		public ICodeDescriptionPairList ReferenceDescriptions
		{
			get
			{
				return referenceDescriptions = referenceDescriptions ?? new CodeDescriptionPairList(OLookUpEditType.ReferenceTypes);
			}
		}
		ICodeDescriptionPairList referenceDescriptions;

		void UpdateLookupCacheIfRequired()
		{
			if (!Parent.DocManagerCode.IsEmpty && Parent.DocManagerCode == docManagerCode)
			{
				return;
			}

			var descriptions = new CodeDescriptionPairList();
			var references = new CodeDescriptionPairList();

			if (!Parent.DocManagerCode.IsEmpty)
			{
				var helper = ObjectFactory.Get<IDocumentScanningHelper>();
				refDocTypes = helper.GetDocTypesFromJobType(Parent.DocManagerCode, Factory, true);

				foreach (var type in refDocTypes.Cast<IRefDocType>())
				{
					descriptions.AddPair(type.RT_DocType, type.RT_DescMultilingual);
					references.AddPair(type.RT_DocType, type.RT_ReferenceType);
				}
			}

			docManagerCode = Parent.DocManagerCode;
			referenceTypes = references;
			documentTypes = descriptions;
		}
		ZString docManagerCode;
	}
}
