using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsProductValidation : ZValidation
	{
		public WhsProductValidation(WhsProduct parent)
			: base(parent)
		{
			Parent = parent;
		}

		readonly WhsProduct Parent;

		#region ValidateAll

		public override void ValidateAll()
		{
			ValidateProductStylePK();
			ValidateProductStyleColourPK();
			ValidateProductStyleClassificationPK();
			ValidateProductStyleSizePK();
		}

		#endregion

		#region ValidateProductStylePK

		public void ValidateProductStylePK()
		{
			ValidateCalculatedProperty(Parent.ProductStylePKInfo);
		}

		protected void CheckProductStylePK()
		{
			ListValidation.ErrorIfInvalidPK(Parent.ProductStylePKInfo);
			TypeValidation.CheckValidGuid(Parent.ProductStylePKInfo);
			CheckPartHasOnlyOneProductOwner(Parent.ProductStylePKInfo);
		}

		void CheckPartHasOnlyOneProductOwner(ZPropertyInfo productStylePKInfo)
		{
			if (!productStylePKInfo.HasErrors())
			{
				var style = Parent.ProductStyle;
				if (style != null)
				{
					var ownerRelationships = GetOwnerRelationships(Parent.Parent);
					var ownerRelationshipCount = ownerRelationships.Length;
					if (ownerRelationshipCount > 1)
					{
						productStylePKInfo.AddError(Res.GetString("1336b837-6daa-418a-8f6a-a9cbe74aa301", "Product must have only one Owner."));
					}
					else if (ownerRelationshipCount == 0)
					{
						productStylePKInfo.AddError(Res.GetString("730d6694-759c-475c-b4f5-90e1337d8e57", "Product must have an owner to use Styles."));
					}
					else if (style.WST_OH_Owner != ownerRelationships[0].OU_OH)
					{
						productStylePKInfo.AddError(Res.GetString("3bd9a1a8-762b-41e8-881f-70213869b254", "Product Style owner must be the owner of the product."));
					}
				}
			}
		}

		static OrgPartRelation[] GetOwnerRelationships(OrgSupplierPart part)
		{
			return part.RelatedOrganisations.Cast<OrgPartRelation>().Where(r => r.OU_Relationship == OrgPartRelation.RelationshipTypes.Owner || r.OU_Relationship == OrgPartRelation.RelationshipTypes.Both).ToArray();
		}

		#endregion

		#region ValidateProductStyleColourPK

		public void ValidateProductStyleColourPK()
		{
			ValidateCalculatedProperty(Parent.ProductStyleColourPKInfo);
		}

		protected void CheckProductStyleColourPK()
		{
			if (Parent.ProductStyle != null)
			{
				MandatoryValidation.CheckEntered(Parent.ProductStyleColourPKInfo);
			}
			ListValidation.ErrorIfInvalidPK(Parent.ProductStyleColourPKInfo);
			TypeValidation.CheckValidGuid(Parent.ProductStyleColourPKInfo);
			CheckIsDuplicate(Parent.ProductStyleColourPKInfo);
		}

		#endregion

		#region ValidateProductStyleClassificationPK

		public void ValidateProductStyleClassificationPK()
		{
			ValidateCalculatedProperty(Parent.ProductStyleClassificationPKInfo);
		}

		protected void CheckProductStyleClassificationPK()
		{
			var style = Parent.ProductStyle;
			if (style != null && style.Classifications.Count > 0)
			{
				MandatoryValidation.CheckEntered(Parent.ProductStyleClassificationPKInfo);
			}
			ListValidation.ErrorIfInvalidPK(Parent.ProductStyleClassificationPKInfo);
			TypeValidation.CheckValidGuid(Parent.ProductStyleClassificationPKInfo);
			CheckIsDuplicate(Parent.ProductStyleClassificationPKInfo);
		}

		#endregion

		#region ValidateProductStyleSizePK

		public void ValidateProductStyleSizePK()
		{
			ValidateCalculatedProperty(Parent.ProductStyleSizePKInfo);
		}

		protected void CheckProductStyleSizePK()
		{
			if (Parent.ProductStyle != null)
			{
				MandatoryValidation.CheckEntered(Parent.ProductStyleSizePKInfo);
			}
			ListValidation.ErrorIfInvalidPK(Parent.ProductStyleSizePKInfo);
			TypeValidation.CheckValidGuid(Parent.ProductStyleSizePKInfo);
			CheckIsDuplicate(Parent.ProductStyleSizePKInfo);
		}

		#endregion

		#region CheckIsDuplicate

		void CheckIsDuplicate(ZPropertyInfo info)
		{
			if (!info.HasErrors() && Parent.ProductStyleColourPK.IsValid && Parent.ProductStyleSizePK.IsValid)
			{
				var query = new ZQuery(OrgSupplierPartSchema.OP_WSC_WhsProductStyleColour, Parent.ProductStyleColourPK);
				query.AddToFilter(OrgSupplierPartSchema.OP_WSZ_WhsProductStyleSize, Parent.ProductStyleSizePK);
				if (Parent.ProductStyleClassificationPK.IsValid)
				{
					query.AddToFilter(OrgSupplierPartSchema.OP_WSS_WhsProductStyleClassification, Parent.ProductStyleClassificationPK);
				}
				else
				{
					query.AddEmptyAsNullToFilter(OrgSupplierPartSchema.OP_WSS_WhsProductStyleClassification, Parent.ProductStyleClassificationPK);
				}
				query.AddToFilter(OrgSupplierPartSchema.OP_IsActive, true);
				query.AddToFilter(OrgSupplierPartSchema.PK, SQLComparisonOperator.NotEqual, Parent.Parent.PK);
				var duplicate = Parent.Factory.LoadTop1<OrgSupplierPart>(query);
				if (duplicate != null)
				{
					var duplicateProduct = WhsProduct.GetWhsProduct(duplicate);
					var style = duplicateProduct.ProductStyle != null ? duplicateProduct.ProductStyle.WST_Code : ZString.Empty;
					var colour = duplicateProduct.ProductStyleColour != null ? duplicateProduct.ProductStyleColour.WSC_Code : ZString.Empty;
					var classification = duplicateProduct.ProductStyleClassification != null ? duplicateProduct.ProductStyleClassification.WSS_Code : ZString.Empty;
					var size = duplicateProduct.ProductStyleSize != null ? duplicateProduct.ProductStyleSize.WSZ_Size : ZString.Empty;
					info.AddError(Res.GetString("6443097f-7463-4583-a9fc-96bb37b43435", "The active Product '{0}' is already using the Style '{1}', Color '{2}', Size '{3}' and Classification '{4}'.", duplicate.OP_PartNum, style, colour, size, classification));
				}
			}
		}

		#endregion

		#region AutoValidationType

		public override Type AutoValidationType
		{
			get { return typeof(WhsProductValidation); }
		}

		#endregion
	}
}
