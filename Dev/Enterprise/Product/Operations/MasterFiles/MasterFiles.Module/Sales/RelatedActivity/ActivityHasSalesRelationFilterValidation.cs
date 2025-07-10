using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Module
{
	public class ActivityHasSalesRelationFilterValidation : HasSalesRelationFilterValidation
	{
		public ActivityHasSalesRelationFilterValidation(ActivityHasSalesRelationFilter parent)
			: base(parent)
		{
		}

		#region ValidateBizObjPK

		public void ValidateBizObjPK()
		{
			ValidateCalculatedProperty(((ActivityHasSalesRelationFilter)Parent).BizObjPKInfo);
		}

		protected void CheckBizObjPK()
		{
			if (!Parent.TypeProperty.IsEmpty
				&& Parent.TypeProperty != SalesRelationActivityFilterHelper.AnySalesRelationTypeCode
				&& !Parent.TypePropertyInfo.GetErrors().Any())
			{
				ListValidation.ErrorIfInvalidPK(((ActivityHasSalesRelationFilter)Parent).BizObjPKInfo);
			}
		}

		#endregion

		public override void ValidateAll()
		{
			ValidateTypeProperty();
			ValidateBizObjPK();
		}

		public override Type AutoValidationType
		{
			get { return GetType(); }
		}
	}
}
