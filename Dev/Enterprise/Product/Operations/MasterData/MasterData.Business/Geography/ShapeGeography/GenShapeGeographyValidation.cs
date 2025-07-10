using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class GenShapeGeographyValidation : AutoGenShapeGeographyValidation
	{
		public GenShapeGeographyValidation(AutoGenShapeGeography parent) : base(parent)
		{
		}

		protected override void CheckSHG_Type()
		{
			base.CheckSHG_Type();

			ListValidation.ErrorIfInvalidCode(Parent.SHG_TypeInfo);
			CheckDuplicateTypeAndName(Parent.SHG_TypeInfo);
		}

		protected override void CheckSHG_Name()
		{
			base.CheckSHG_Name();
			CheckDuplicateTypeAndName(Parent.SHG_NameInfo);
		}

		void CheckDuplicateTypeAndName(ZPropertyInfo info)
		{
			if (!string.IsNullOrEmpty(Parent.SHG_Type) && !string.IsNullOrEmpty(Parent.SHG_Name) && !info.HasErrors())
			{
				var query = new ZDBOnlyQuery(typeof(GenShapeGeography));
				query.AddToFilter(GenShapeGeographySchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				query.AddToFilter(GenShapeGeographySchema.SHG_Type, Parent.SHG_Type);
				query.AddToFilter(GenShapeGeographySchema.SHG_Name, Parent.SHG_Name);

				var duplicateShape = Parent.Factory.Load<GenShapeGeography>(query);
				if (duplicateShape.Any())
				{
					info.AddError(Res.GetString("ab1459bb-e905-3edb-a46c-1c8d0a7fd096", "Name: {0} + Type: {1} already exists in the database.", Parent.SHG_Name, Parent.SHG_Type));
				}
			}
		}

		protected override void CheckSHG_ParentID()
		{
			base.CheckSHG_ParentID();

			if (!Parent.SHG_ParentTableCode.IsEmpty && Parent.Lookups.SHG_ParentTableCode_List.ContainsCode(Parent.SHG_ParentTableCode))
			{
				MandatoryValidation.CheckEntered(Parent.SHG_ParentIDInfo);
			}
		}

		protected override void CheckSHG_ParentTableCode()
		{
			base.CheckSHG_ParentTableCode();

			ListValidation.ErrorIfInvalidCode(Parent.SHG_ParentTableCodeInfo);
		}
	}
}
