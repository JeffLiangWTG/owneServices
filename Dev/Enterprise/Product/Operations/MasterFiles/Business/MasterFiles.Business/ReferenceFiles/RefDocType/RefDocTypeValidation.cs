using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefDocTypeValidation : AutoRefDocTypeValidation
	{
		public RefDocTypeValidation(AutoRefDocType parent)
			: base(parent)
		{
		}

		new RefDocType Parent
		{
			get { return (RefDocType)base.Parent; }
		}

		protected override void CheckRT_Desc()
		{
			base.CheckRT_Desc();
			MandatoryValidation.CheckEntered(Parent.RT_DescInfo);
			if (Parent.RT_Desc.Length < 4)
			{
				Parent.RT_DescInfo.AddError(Res.GetString("62ab873d-90fb-4499-baef-e898179f9f44", "Type description must be at least 4 characters."));
			}
			TranslatableDataFieldAttribute.Validate(Parent.RT_DescInfo);
		}

		protected override void CheckRT_DocType()
		{
			base.CheckRT_DocType();

			MandatoryValidation.CheckEntered(Parent.RT_DocTypeInfo);

			if (!Parent.RT_IsSystem && Parent.RT_DocType.Length != 3)
			{
				Parent.RT_DocTypeInfo.AddError(Res.GetString("cc21cda9-6240-478d-9d36-9a240804ce85", "Document type must be 3 characters."));
			}

			if (!IsUniqueAndNotInALLCategory())
			{
				Parent.RT_DocTypeInfo.AddError(Res.GetString("006eaa8e-398e-411f-b91e-07ff68544bd8", "Document type must be unique, and not the same as a document type from the ALL reference type category."));
			}

			if (Parent.RT_DocType.Contains(':'))
			{
				Parent.RT_DocTypeInfo.AddError(Res.GetString("00d20631-4498-4110-b8e2-5c90e9d9370f", "Document type cannot contain ':'."));
			}
		}

		[SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", Justification = "We're using the 'forceCompileToHappen' variable to force the macro to actually compile into an expression, but we dont need the result")]
		protected override void CheckRT_LogMacro()
		{
			base.CheckRT_LogMacro();

			if (!Parent.RT_LogMacro.IsEmpty)
			{
				var macro = Parent.LogMacroExecutor.GetCompiledLogMacro();
				var forceCompileToHappen = macro.Expression;

				if (macro.Errors.Any())
				{
					var errors = MakeBulletPoints(macro.Errors.Select(error => error.Message));
					Parent.RT_LogMacroInfo.AddError(Res.GetString("e2a86969-726b-443d-9df9-b38cc0756285", "There were errors when compiling your macro. \r\n{0}", errors));
				}
			}
		}

		string MakeBulletPoints(IEnumerable<string> points)
		{
			var sb = new StringBuilder();
			foreach (var item in points)
			{
				sb.AppendFormat(" - {0}\r\n", item);
			}

			return sb.ToString().TrimEnd();
		}

		public StmMenuTemplatePivot[] GetReferencingMenuTemplatePivots()
		{
			return Parent.Factory.Load<StmMenuTemplatePivot>(new ZQuery(StmMenuTemplatePivotSchema.SI_RT_DocType, Parent.PK));
		}

		public StmMenuEDocs[] GetReferencingMenuEDocs()
		{
			return Parent.Factory.Load<StmMenuEDocs>(new ZQuery(StmMenuEDocsSchema.SX_RT_DocType, Parent.PK));
		}

		public bool CheckIfViolatingUniqueIndexStmMenuEDocs(ZGuid sx_su, ZGuid fk)
		{
			ZQuery query = new ZQuery(StmMenuEDocsSchema.SX_RT_DocType, fk);
			query.AddToFilter(JoinCondition.And, StmMenuEDocsSchema.SX_SU, sx_su);
			return (Parent.Factory.Load<StmMenuEDocs>(query)).Length > 0;
		}

		protected override void CheckRT_ReferenceType()
		{
			base.CheckRT_ReferenceType();

			MandatoryValidation.CheckEntered(Parent.RT_ReferenceTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.RT_ReferenceTypeInfo);

			if (Parent.RT_ReferenceType.Length != 3)
			{
				Parent.RT_ReferenceTypeInfo.AddError(Res.GetString("cf636a41-b76c-43b8-8c68-561b551b9c13", "Reference type must be 3 characters."));
			}

			if (Parent.RT_ReferenceType.Contains(':'))
			{
				Parent.RT_ReferenceTypeInfo.AddError(Res.GetString("4531fb32-c123-47a3-99d1-ab84a3c12d6d", "Reference type cannot contain ':'."));
			}
		}

		internal bool IsUniqueAndNotInALLCategory()
		{
			ZQuery filter = new ZQuery(RefDocTypeSchema.RT_DocType, SQLComparisonOperator.Equal, Parent.RT_DocType);
			filter.AddToFilter(JoinCondition.And, RefDocTypeSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			RefDocTypeCollection existingDocTypes = new RefDocTypeCollection(Parent.Factory, filter);

			if (existingDocTypes.Count > 0)
			{
				foreach (RefDocType docType in existingDocTypes)
				{
					if (docType.RT_ReferenceType == "ALL")
					{
						return false;
					}

					if (docType.RT_DocType == Parent.RT_DocType && docType.RT_ReferenceType == Parent.RT_ReferenceType)
					{
						return false;
					}
				}
			}

			return true;
		}

		protected override void CheckRT_SE_NKDocumentReceivedEvent()
		{
			base.CheckRT_SE_NKDocumentReceivedEvent();
			ListValidation.ErrorIfInvalidCode(Parent.RT_SE_NKDocumentReceivedEventInfo);
		}

		public void ValidateRT_OverrideVersions()
		{
			ValidateCalculatedProperty(Parent.RT_OverrideVersionsInfo);
		}

		protected void CheckRT_OverrideVersions()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.RT_OverrideVersionsInfo);
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateRT_OverrideVersions();
		}
	}
}
