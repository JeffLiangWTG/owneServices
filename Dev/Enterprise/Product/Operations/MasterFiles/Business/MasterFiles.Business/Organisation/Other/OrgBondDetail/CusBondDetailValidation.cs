//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusBondDetailValidation
//
//    This class should be used for overriding validation in AutoCusBondDetailValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class CusBondDetailValidation : AutoCusBondDetailValidation
	{
		public CusBondDetailValidation(AutoCusBondDetail parent) : base(parent)
		{
		}

		protected override void CheckPW_CPH_Guarantee()
		{
			base.CheckPW_CPH_Guarantee();

			var parent = Parent;
			var type = parent.PW_BondType;
			var guarantee = parent.PW_CPH_Guarantee;
			if (type == GuaranteeBondTypeList.Codes.SingleTransaction && !guarantee.IsEmpty)
			{
				if (activities.Contains(parent.PW_ActivityCode))
				{
					var query = new ZQuery();
					query.AddToFilter(CusBondDetailSchema.PW_BondType, GuaranteeBondTypeList.Codes.SingleTransaction);
					query.AddToFilter(CusBondDetailSchema.PW_CPH_Guarantee, guarantee);
					query.AddToFilter(CusBondDetailSchema.PW_ActivityCode, activities);
					query.AddToFilter(CusBondDetailSchema.PK, SQLComparisonOperator.NotEqual, parent.PK);
					if (parent.Factory.ExistsInDatabase(CusBondDetailSchema.Constants.TableName, query))
					{
						parent.PW_CPH_GuaranteeInfo.AddError(Res.GetString("501FE535-153E-4B2B-8F21-99168869E761", "The Guarantee with same Type(STB) and Activity(CON or OAC) already exists."));
					}
				}
			}
		}

		readonly HashSet<string> activities = new HashSet<string> { GuaranteeActivityCodeList.Codes.ConsumesGuarantee, GuaranteeActivityCodeList.Codes.ConsumeAndRelease };
	}
}
