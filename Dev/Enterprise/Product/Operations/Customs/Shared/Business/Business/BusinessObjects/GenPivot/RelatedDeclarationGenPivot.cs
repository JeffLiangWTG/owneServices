using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	#region RelatedDeclarationGenPivot

	public class RelatedDeclarationGenPivot : CustomsGenPivot
	{
		public RelatedDeclarationGenPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public BaseJobDeclaration JobDeclaration
		{
			get
			{
				if (declaration == null || declaration.IsDeleted || declaration.PK != XX_Relation2ID)
				{
					declaration = Factory.Load<BaseJobDeclaration>(XX_Relation2ID);
				}
				return declaration;
			}
		}
		BaseJobDeclaration declaration;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			XX_Relation1TableCode = JobComInvoiceHeaderSchema.Constants.Prefix;
			XX_Relation2TableCode = JobDeclarationSchema.Constants.Prefix;
		}
	}

	#endregion

	#region InvoiceRelatedDeclarationGenPivot

	public class InvoiceRelatedDeclarationGenPivot : RelatedDeclarationGenPivot, Integration.Customs.IInvoiceRelatedDeclarationGenPivot
	{
		public const string RelationType = GenPivotTypeDecider.Types.InvoiceRelatedDeclarationGenPivot;
		public InvoiceRelatedDeclarationGenPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			XX_RelationType = RelationType;
		}

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public ZQuery GetQuery(BaseJobDeclaration declaration)
			{
				var result = new ZQuery();
				result.AddToFilter(GenPivotSchema.XX_Relation2ID, declaration.PK);
				var genPivotQuery = new ZQuery();
				genPivotQuery.AddToFilter(GenPivotSchema.XX_RelationType, RelationType);
				genPivotQuery.AddToFilter(GenPivotSchema.XX_Relation1TableCode, JobComInvoiceHeaderSchema.Constants.Prefix);
				genPivotQuery.AddToFilter(GenPivotSchema.XX_Relation2TableCode, JobDeclarationSchema.Constants.Prefix);
				result.AddToFilter(genPivotQuery);
				result.FetchOnlyFromLocalCache = !declaration.IsInDatabase;
				return result;
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(InvoiceRelatedDeclarationGenPivot);
			}
		}
	}
	#endregion

	#region GroupRelatedDeclarationGenPivot

	public class GroupRelatedDeclarationGenPivot : RelatedDeclarationGenPivot, Integration.Customs.IGroupRelatedDeclarationGenPivot
	{
		public const string RelationType = GenPivotTypeDecider.Types.GroupRelatedDeclarationGenPivot;
		public GroupRelatedDeclarationGenPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			XX_RelationType = RelationType;
		}

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public ZQuery GetQuery(BaseJobDeclaration declaration)
			{
				var result = new ZQuery();
				result.AddToFilter(GenPivotSchema.XX_Relation2ID, declaration.PK);
				result.AddToFilter(GenPivotSchema.XX_Relation1TableCode, JobComInvoiceHeaderSchema.Constants.Prefix);
				result.AddToFilter(GenPivotSchema.XX_Relation2TableCode, JobDeclarationSchema.Constants.Prefix);
				result.AddToFilter(GenPivotSchema.XX_RelationType, RelationType);
				result.FetchOnlyFromLocalCache = !declaration.IsInDatabase;
				return result;
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(GroupRelatedDeclarationGenPivot);
			}
		}
	}

	#endregion
}
