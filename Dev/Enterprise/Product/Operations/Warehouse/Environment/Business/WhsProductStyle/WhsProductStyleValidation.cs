using System;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsProductStyleValidation : AutoWhsProductStyleValidation
	{
		public WhsProductStyleValidation(AutoWhsProductStyle parent)
			: base(parent)
		{
		}

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateRow();
		}

		#endregion

		#region CheckWST_Code

		protected override void CheckWST_Code()
		{
			base.CheckWST_Code();

			var info = Parent.WST_CodeInfo;
			MandatoryValidation.CheckEntered(info);

			if (!info.HasErrors())
			{
				var productStyleWithSameCodeQuery = new ZQuery(WhsProductStyleSchema.WST_Code, Parent.WST_Code);
				productStyleWithSameCodeQuery.AddToFilter(WhsProductStyleSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

				var productStyleWithSameCode = Factory.LoadTop1<WhsProductStyle>(productStyleWithSameCodeQuery);
				if (productStyleWithSameCode != null)
				{
					info.AddError(Res.GetString("WhsProductStyleValidation|SameCode", "Product Style '{0}' is already using the Code '{1}'.", productStyleWithSameCode.WST_Description, productStyleWithSameCode.WST_Code));
				}
			}
		}

		#endregion

		#region CheckWST_Description

		protected override void CheckWST_Description()
		{
			base.CheckWST_Description();
			MandatoryValidation.CheckEntered(Parent.WST_DescriptionInfo);
		}

		#endregion

		#region CheckWST_OH_Owner

		protected override void CheckWST_OH_Owner()
		{
			base.CheckWST_OH_Owner();
			var style = Parent;
			var info = style.WST_OH_OwnerInfo;
			var ownerPK = (ZGuid)style.WST_OH_OwnerInfo.Value;
			if (!info.HasErrors() && style.Colours.Count > 0 && style.Sizes.Count > 0)
			{
				var products = style.GetProducts();
				var productsWithADifferentOwner = products.Where(p => p.RelatedOrganisations.Cast<OrgPartRelation>().Any(r => (r.OU_Relationship == OrgPartRelation.RelationshipTypes.Owner || r.OU_Relationship == OrgPartRelation.RelationshipTypes.Both) && r.OU_OH != ownerPK));
				if (productsWithADifferentOwner.Any())
				{
					var msg = new ZStringBuilder(Res.GetString("WhsProductStyle|StyleHasBeenUsedInAProduct", "Owner cannot be changed as it is being used on the following products:"));
					Array.ForEach(productsWithADifferentOwner.OrderBy(p => p.OP_PartNum).ToArray(), p => msg.Append(string.Format(CultureInfo.InvariantCulture, "{0} - {1}", p.OP_PartNum, p.OP_Desc)));
					info.AddError(msg.ToStringWithNewLineBetweenAppends().Trim());
				}
			}
		}

		#endregion

		#region ValidateRow

		void ValidateRow()
		{
			ValidateAtLeastOneColour();
			ValidateAtLeastOneSize();
		}

		#region ValidateAtLeastOneColour

		public void ValidateAtLeastOneColour()
		{
			var errorMessage = Res.GetString("WhsProductStyleValidation|AtLeastOneColour", "At least One Color is required.");
			Parent.RemoveRowError(errorMessage);

			if (Parent.Colours.Count == 0)
			{
				Parent.AddRowError(errorMessage);
			}
		}

		#endregion

		#region ValidateAtLeastOneSize

		public void ValidateAtLeastOneSize()
		{
			var errorMessage = Res.GetString("WhsProductStyleValidation|AtLeastOneSize", "At least One Size is required.");
			Parent.RemoveRowError(errorMessage);

			if (Parent.Sizes.Count == 0)
			{
				Parent.AddRowError(errorMessage);
			}
		}

		#endregion

		#endregion

		#region Parent

		protected new WhsProductStyle Parent
		{
			get { return (WhsProductStyle)base.Parent; }
		}

		#endregion

		#region Factory

		BusinessObjectFactory Factory
		{
			get { return Parent.Factory; }
		}

		#endregion
	}
}
