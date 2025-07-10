using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class DocTypeCategoryQuery : ZQuery
	{
		public DocTypeCategoryQuery(BusinessObjectFactory factory, ZString refType)
		{
			SetupFilter(refType);
		}

		void SetupFilter(ZString refType)
		{
			AddRefType(Constants.ReferenceTypes.All);
			AddRefType(refType);
		}

		public void AddRefType(ZString refType)
		{
			if (!refType.IsEmpty)
			{
				if (!DataRegistry.Instance.ProductivityWiseModeEnabled ||
					(refType != Constants.ReferenceTypes.ClientSupplierRelationship
						&& refType != Constants.ReferenceTypes.SupplyChainLogistics))
				{
					AddToFilter(JoinCondition.Or, RefDocTypeSchema.RT_ReferenceType, SQLComparisonOperator.Equal, refType);
				}
			}
		}
	}
}
