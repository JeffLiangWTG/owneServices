using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsReceiveProductSummaryValidation : ZValidation
	{
		public WhsReceiveProductSummaryValidation(WhsReceiveProductSummary parent)
			: base(parent)
		{
		}

		public override Type AutoValidationType => typeof(WhsReceiveProductSummaryValidation);

		WhsReceiveProductSummary Parent => (WhsReceiveProductSummary)ParentFilter;

		public override void ValidateAll()
		{
			ValidateReceivedQuantity();
		}

		#region ValidateReceivedQuantity

		public void ValidateReceivedQuantity()
		{
			ValidateCalculatedProperty(Parent.ReceivedQuantityInfo);
		}

		protected void CheckReceivedQuantity()
		{
			var orgPartRelation = Parent.Product?.RelatedOrganisations.FindByOrganisationPKAndRelationship(Parent.ClientPk, OrgPartRelation.RelationshipTypes.Owner);
			if (!Parent.IsBlindReceive)
			{
				var preventReceivingOvers = false;
				var receiveOverageTolerancePercent = 0m;
				if (orgPartRelation != null && orgPartRelation.OU_PreventReceivingOvers)
				{
					preventReceivingOvers = true;
					receiveOverageTolerancePercent = orgPartRelation.OU_ReceiveOverageTolerancePercent;
				}
				else if (Parent.ClientPk.IsValid)
				{
					var client = Parent.Client;
					if (client != null)
					{
						var clientParams = WhsClientParams.GetClientParams(client);
						var whsClientParameterByWarehouseCollection = clientParams.ClientParametersByWarehouse;
						var whsClientParameterByWarehouse = whsClientParameterByWarehouseCollection.FindWithEmptyFallback(Parent.ClientPk, Parent.WarehousePK, Parent.ReceiveCategory);
						if (whsClientParameterByWarehouse != null && whsClientParameterByWarehouse.WY_PreventReceivingOvers)
						{
							preventReceivingOvers = true;
							receiveOverageTolerancePercent = whsClientParameterByWarehouse.WY_ReceiveOverageTolerancePercent;
						}
					}
				}

				if (preventReceivingOvers)
				{
					var allowedQty = Parent.ExpectedQuantity * (1m + receiveOverageTolerancePercent / 100m);
					if (Parent.ReceivedQuantity > allowedQty)
					{
						Parent.ReceivedQuantityInfo.AddError(Res.GetString("40cac259-94b1-40e3-bbd6-6e03d5ab5020", "Received quantity of product '{0}' exceeds the allowed quantity.", Parent.ProductCode));
					}
				}
			}
		}

		#endregion
	}
}
