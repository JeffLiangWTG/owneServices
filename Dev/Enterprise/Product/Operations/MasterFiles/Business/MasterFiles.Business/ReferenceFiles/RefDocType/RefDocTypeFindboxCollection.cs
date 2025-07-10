using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefDocTypeFindboxCollection : RefDocTypeCollection
	{
		public RefDocTypeFindboxCollection(BusinessObjectFactory factory, ZString defaultReferenceType)
			: base(factory)
		{
			this.defaultReferenceType = defaultReferenceType.IsEmpty ? new ZString("ALL") : defaultReferenceType;
			AdditionalFilter = GetReferenceTypeFilter();
		}
		readonly ZString defaultReferenceType;

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			if (defaultReferenceType != "ALL")
			{
				errors.Add(Res.GetString("d7b77e68-192d-4eff-b606-69dddf9c8a8d", "A Document Type selected from here must have a Reference Type of [{0}] or [ALL] selected.", defaultReferenceType));
			}
			else
			{
				errors.Add(Res.GetString("d7b77e68-192d-4eff-b606-69dddf9c8a8e", "A Document Type selected from here must have a Reference Type of [ALL] selected."));
			}
		}

		ZQuery GetReferenceTypeFilter()
		{
			ZQuery query = new ZQuery(RefDocTypeSchema.RT_ReferenceType, "ALL");
			if (defaultReferenceType != "ALL")
			{
				if (DataRegistry.Instance.ProductivityWiseModeEnabled
					&& (defaultReferenceType == Enterprise.Core.Constants.ReferenceTypes.ClientSupplierRelationship
					|| defaultReferenceType == Enterprise.Core.Constants.ReferenceTypes.SupplyChainLogistics))
				{
					query.AddToFilter(RefDocTypeSchema.RT_ReferenceType, SQLComparisonOperator.NotEqual, Enterprise.Core.Constants.ReferenceTypes.ClientSupplierRelationship);
					query.AddToFilter(RefDocTypeSchema.RT_ReferenceType, SQLComparisonOperator.NotEqual, Enterprise.Core.Constants.ReferenceTypes.SupplyChainLogistics);
				}
				else
				{
					query.AddToFilter(JoinCondition.Or, RefDocTypeSchema.RT_ReferenceType, defaultReferenceType);
				}
			}
			return query;
		}

		protected override void SetDefaultsForNewElementCore(RefDocType docType)
		{
			base.SetDefaultsForNewElementCore(docType);
			docType.RT_ReferenceType = defaultReferenceType;
		}
	}
}
