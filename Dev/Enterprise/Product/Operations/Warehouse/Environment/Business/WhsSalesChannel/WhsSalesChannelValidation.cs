//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsSalesChannelValidation
//
//    This class should be used for overriding validation in AutoWhsSalesChannelValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsSalesChannelValidation : AutoWhsSalesChannelValidation
	{
		public WhsSalesChannelValidation(AutoWhsSalesChannel parent)
			: base(parent)
		{
		}

		#region CheckWSH_Code

		protected override void CheckWSH_Code()
		{
			base.CheckWSH_Code();
			MandatoryValidation.CheckEntered(Parent.WSH_CodeInfo);
			CheckIsUnique(Parent.WSH_CodeInfo, WhsSalesChannelSchema.WSH_Code);
		}

		void CheckIsUnique(ZPropertyInfo info, SchemaColumn column)
		{
			if (!info.HasErrors())
			{
				var value = info.Value;
				var query = new ZQuery();
				query.AddToFilter(column, value);
				query.AddToFilter(WhsSalesChannelSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

				if (Parent.Factory.LoadTop1<WhsSalesChannel>(query) != null)
				{
					info.AddError(Res.GetString("8228e582-7bce-491c-be11-ddea795767fd", "Sales Channel {0} '{1}' already exists.", info.HumanReadableName, value));
				}
			}
		}

		#endregion

		#region CheckWSH_Description

		protected override void CheckWSH_Description()
		{
			base.CheckWSH_Description();
			MandatoryValidation.CheckEntered(Parent.WSH_DescriptionInfo);
			CheckIsUnique(Parent.WSH_DescriptionInfo, WhsSalesChannelSchema.WSH_Description);
		}

		#endregion
	}
}
