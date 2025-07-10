//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsPutawayGroupValidation
//
//    This class should be used for overriding validation in AutoWhsPutawayGroupValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsPutawayGroupValidation : AutoWhsPutawayGroupValidation
	{
		public WhsPutawayGroupValidation(AutoWhsPutawayGroup parent)
			: base(parent)
		{
		}

		#region CheckWPG_Code

		protected override void CheckWPG_Code()
		{
			base.CheckWPG_Code();
			MandatoryValidation.CheckEntered(Parent.WPG_CodeInfo);
			CheckIsUnique(Parent.WPG_CodeInfo, WhsPutawayGroupSchema.WPG_Code);
		}

		void CheckIsUnique(ZPropertyInfo info, SchemaColumn column)
		{
			if (!info.HasErrors())
			{
				var value = info.Value;
				var query = new ZQuery();
				query.AddToFilter(column, value);
				query.AddToFilter(WhsPutawayGroupSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

				if (Parent.Factory.LoadTop1<WhsPutawayGroup>(query) != null)
				{
					info.AddError(Res.GetString("08dffb4d-592c-48e0-8845-5a2cbdda8b87", "Putaway Group {0} '{1}' already exists.", info.HumanReadableName, value));
				}
			}
		}

		#endregion

		#region CheckWPG_Description

		protected override void CheckWPG_Description()
		{
			base.CheckWPG_Description();
			MandatoryValidation.CheckEntered(Parent.WPG_DescriptionInfo);
			CheckIsUnique(Parent.WPG_DescriptionInfo, WhsPutawayGroupSchema.WPG_Description);
		}

		#endregion
	}
}
