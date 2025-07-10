using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsProductStyleClassificationValidation : AutoWhsProductStyleClassificationValidation
	{
		public WhsProductStyleClassificationValidation(AutoWhsProductStyleClassification parent)
			: base(parent)
		{
		}

		#region CheckWSS_Code

		protected override void CheckWSS_Code()
		{
			base.CheckWSS_Code();
			var info = Parent.WSS_CodeInfo;
			MandatoryValidation.CheckEntered(info);

			if (!info.HasErrors() && Parent.ProductStyle != null)
			{
				var duplicate = Parent.ProductStyle.Classifications.FirstOrDefault(c => c.WSS_Code.ToUpper() == Parent.WSS_Code.ToUpper() && c.PK != Parent.PK);
				if (duplicate != null)
				{
					info.AddError(Res.GetString("WhsProductStyleClassificationValidation|SameCode", "Product Style Classification '{0}' is already using the Code '{1}'.", duplicate.WSS_Description, duplicate.WSS_Code));
				}
			}
		}

		#endregion

		#region CheckWSS_Description

		protected override void CheckWSS_Description()
		{
			base.CheckWSS_Description();
			MandatoryValidation.CheckEntered(Parent.WSS_DescriptionInfo);
		}

		#endregion

		#region Parent

		protected new WhsProductStyleClassification Parent
		{
			get { return (WhsProductStyleClassification)base.Parent; }
		}

		#endregion

		#region ShouldValidateFKToCancelledRecord

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
			=> info.Name != WhsProductStyleClassificationSchema.Constants.WSS_WST_ProductStyle && base.ShouldValidateFKToCancelledRecord(info);

		#endregion
	}
}
