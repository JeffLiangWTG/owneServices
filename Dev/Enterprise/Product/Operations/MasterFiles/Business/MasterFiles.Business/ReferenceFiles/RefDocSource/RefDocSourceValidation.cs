using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefDocSourceValidation : AutoRefDocSourceValidation
	{
		public RefDocSourceValidation(AutoRefDocSource parent) : base(parent)
		{
		}

		new RefDocSource Parent
		{
			get { return (RefDocSource)base.Parent; }
		}

		protected override void CheckRDS_Desc()
		{
			base.CheckRDS_Desc();
			MandatoryValidation.CheckEntered(Parent.RDS_DescInfo);
			if (Parent.RDS_Desc.Length < 4)
			{
				Parent.RDS_DescInfo.AddError(Res.GetString("57d05e33-33d8-4904-8ab1-3a8d140d54e7", "Source description must be at least 4 characters."));
			}
			TranslatableDataFieldAttribute.Validate(Parent.RDS_DescInfo);
		}

		protected override void CheckRDS_Code()
		{
			base.CheckRDS_Code();

			MandatoryValidation.CheckEntered(Parent.RDS_CodeInfo);

			if (Parent.RDS_Code.Length != 3)
			{
				Parent.RDS_CodeInfo.AddError(Res.GetString("25120a88-a35d-45f9-86d5-3767cad88766", "Document source must be 3 characters."));
			}

			if (!IsUnique())
			{
				Parent.RDS_CodeInfo.AddError(Res.GetString("07c28b58-50ca-47fd-a1b8-b52f88fff2c9", "Document source must be unique."));
			}

			if (Parent.RDS_Code.Contains(':'))
			{
				Parent.RDS_CodeInfo.AddError(Res.GetString("a9783546-97ee-438a-afa6-afd4e95852aa", "Document source cannot contain ':'."));
			}
		}

		internal bool IsUnique()
		{
			ZQuery filter = new ZQuery(RefDocSourceSchema.RDS_Code, SQLComparisonOperator.Equal, Parent.RDS_Code);
			filter.AddToFilter(JoinCondition.And, RefDocSourceSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			RefDocSourceCollection existingDocSources = new RefDocSourceCollection(Parent.Factory, filter);

			if (existingDocSources.Count > 0)
			{
				foreach (RefDocSource docSource in existingDocSources)
				{
					if (docSource.RDS_Code == Parent.RDS_Code)
					{ return false; }
				}
			}

			return true;
		}
	}
}
