using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Bonded
{
	public class WhsBondedJobNumberUpdater
	{
		public WhsBondedJobNumberUpdater(BusinessObjectFactory factory)
		{
			Factory = factory ?? throw new ArgumentNullException(nameof(factory));
		}

		public void Update(ZGuid declarationPK, ZString declarationReference)
		{
			if (!declarationPK.IsValid)
			{
				throw new ArgumentException("DeclarationPK is empty");
			}

			if (declarationReference.IsEmpty)
			{
				throw new ArgumentException("DeclarationReference is empty");
			}

			UpdateExWarehouseTransaction(declarationPK, declarationReference);
		}

		void UpdateExWarehouseTransaction(ZGuid declarationPK, ZString declarationReference)
		{
			var orders = Factory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_ExWhsJobGuid, declarationPK));
			var orderIndex = 0;
			foreach (var order in orders)
			{
				if (order.IsAttachedToPick || order.IsFinalised)
				{
					++orderIndex;
					var refLen = declarationReference.Length;
					if (order.WD_ExternalReference.Left(refLen) != declarationReference)
					{
						order.WD_ExternalReference = declarationReference + "-" + orderIndex.ToString(Culture.Invariant);
					}

					foreach (var orderLine in order.Lines)
					{
						orderLine.CustomsData.WB_DeclarationReference = declarationReference;
					}
				}
			}
		}

		readonly BusinessObjectFactory Factory;
	}
}
