using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class GenerateOrderFromInventoryActionMethodApplicator : GenerateOrderFromExistingInventoryActionMethodApplicator
	{
		public GenerateOrderFromInventoryActionMethodApplicator(BusinessObjectFactory factory)
			: base(Res.GetString("6aaa43a2-d6e7-4100-8661-6a79c952942e", "Generate Order"), factory) // text used for logging
		{
		}

		#region Generate Order

		protected override WhsDocketLine[] GetDocketLineList(IOperationalActionSectionLog log, BusinessObject[] bizOList)
		{
			var result = new List<WhsDocketLine>();

			foreach (var bizO in bizOList)
			{
				var inventoryView = bizO as WhsInventoryView;
				if (inventoryView != null)
				{
					result.Add(inventoryView.InDocketLine);
				}
			}

			return result.ToArray();
		}

		#endregion
	}
}
