using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsProductStyleColourValidation : AutoWhsProductStyleColourValidation
	{
		public WhsProductStyleColourValidation(AutoWhsProductStyleColour parent)
			: base(parent)
		{
		}

		#region CheckWSC_Code

		protected override void CheckWSC_Code()
		{
			base.CheckWSC_Code();
			var info = Parent.WSC_CodeInfo;
			MandatoryValidation.CheckEntered(info);

			if (!info.HasErrors() && Parent.ProductStyle != null)
			{
				var duplicate = Parent.ProductStyle.Colours.FirstOrDefault(c => c.WSC_Code.ToUpper() == Parent.WSC_Code.ToUpper() && c.PK != Parent.PK);
				if (duplicate != null)
				{
					info.AddError(Res.GetString("WhsProductStyleColourValidation|SameCode", "Product Style Color '{0}' is already using the Code '{1}'.", duplicate.WSC_Description, duplicate.WSC_Code));
				}
			}
		}

		#endregion

		#region CheckWSC_Description

		protected override void CheckWSC_Description()
		{
			base.CheckWSC_Description();
			MandatoryValidation.CheckEntered(Parent.WSC_DescriptionInfo);
		}

		#endregion

		#region Parent

		protected new WhsProductStyleColour Parent
		{
			get { return (WhsProductStyleColour)base.Parent; }
		}

		#endregion

		#region ShouldValidateFKToCancelledRecord

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
			=> info.Name != WhsProductStyleColourSchema.Constants.WSC_WST_ProductStyle && base.ShouldValidateFKToCancelledRecord(info);

		#endregion
	}
}
