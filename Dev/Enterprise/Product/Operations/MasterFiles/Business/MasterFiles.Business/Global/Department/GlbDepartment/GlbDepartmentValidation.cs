using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbDepartmentValidation : AutoGlbDepartmentValidation
	{
		public GlbDepartmentValidation(AutoGlbDepartment parent) : base(parent)
		{
		}

		#region Check GE_Code

		protected override void CheckGE_Code()
		{
			base.CheckGE_Code();
			MandatoryValidation.CheckEntered(Parent.GE_CodeInfo);
			if (!Parent.GE_CodeInfo.HasErrors())
			{
				if (Parent.GE_Code.Length != 3)
				{
					Parent.GE_CodeInfo.AddError(Res.GetString("ba0ec6e4-c591-4b7f-b7ce-cb34d0c5ce90", "Code must be 3 characters long"));
				}
			}
			if (!Parent.GE_CodeInfo.HasErrors())
			{
				CheckGE_CodeIsAlphaNumeric();
			}
			if (!Parent.GE_CodeInfo.HasErrors())
			{
				ZQuery filter = new ZQuery(GlbDepartmentSchema.GE_Code, Parent.GE_Code);
				filter.AddToFilter(JoinCondition.And, GlbDepartmentSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				GlbDepartment dept = (GlbDepartment)Parent.Factory.LoadTop1(typeof(GlbDepartment), filter);
				if (dept != null)
				{
					if (dept.GE_SystemCode && DataRegistry.Instance.ProductivityWiseModeEnabled)
					{
						Parent.GE_CodeInfo.AddError(Res.GetString("635F6954-8C1A-49F4-9F4E-540DD73CADCD", "Code is reserved for system usage."));
					}
					else
					{
						Parent.GE_CodeInfo.AddError(Res.GetString("3cbd05a8-976b-4f6a-8542-17b286a89049", "Code must be unique."));
					}
				}
			}
		}

		#endregion

		#region Check GE_CodeIsAlphaNumeric

		protected void CheckGE_CodeIsAlphaNumeric()
		{
			Regex nonAlphaNumericRegex = new Regex(@"^[a-z0-9]+\s*$", RegexOptions.IgnoreCase);
			if (!nonAlphaNumericRegex.IsMatch(Parent.GE_Code))
			{
				Parent.GE_CodeInfo.AddError(Res.GetString("0ddfecce-2066-4a5d-a061-bf678653a2dc", "{0} is not a valid Code.", Parent.GE_Code));
			}
		}

		#endregion

		#region Check GE_Desc

		protected override void CheckGE_Desc()
		{
			base.CheckGE_Desc();
			MandatoryValidation.CheckEntered(Parent.GE_DescInfo);
			TranslatableDataFieldAttribute.Validate(Parent.GE_DescInfo);

			if (!Parent.GE_DescInfo.HasErrors())
			{
				if (Parent.GE_Desc.Length < 4)
				{
					Parent.GE_DescInfo.AddError(Res.GetString("a4919d7b-fa70-4c16-baa1-fb890a2971f5", "Description must be at least 4 characters long."));
				}
			}
		}

		#endregion

		#region Check GE_GE

		protected override void CheckGE_GE()
		{
			base.CheckGE_GE();
			if (!Parent.GE_SystemCode)
			{
				MandatoryValidation.CheckEntered(Parent.GE_GEInfo);
			}
			if (Parent.GE_GE == Parent.PK)
			{
				Parent.GE_GEInfo.AddError(Res.GetString("abb07b9e-6501-40d7-89d9-71eac5108f18", "A Department cannot be a parent of itself."));
			}
		}
		#endregion

		#region Check GE_IsActive

		protected override void CheckGE_IsActive()
		{
			base.CheckGE_IsActive();
			if (!Parent.GE_IsActive)
			{
				ZQuery query = new ZQuery(GlbDepartmentSchema.GE_IsActive, true);
				query.AddToFilter(GlbDepartmentSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				if (!Parent.Factory.ExistsInDatabase(BusinessObjectFactory.GetTableNameFromType(typeof(GlbDepartment)), query))
				{
					Parent.GE_IsActiveInfo.AddError(Res.GetString("d9da7f55-a616-4461-af34-a04dcede8024", "You cannot deactivate all Departments as no one will be able to login after that. Please leave at least one active Department."));
				}
			}
		}

		#endregion
	}
}
