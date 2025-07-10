using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class GenerateOrderFromReceiveActionMethodApplicator : GenerateOrderFromExistingInventoryActionMethodApplicator
	{
		public GenerateOrderFromReceiveActionMethodApplicator(BusinessObjectFactory factory)
			: base(Res.GetString("020ffe35-a9be-44e8-9a85-ec7ac6de6bf1", "Generate Order"), factory) // text used for logging
		{
		}

		#region Generate Order

		const string OutputTextFormat = "{0} {1} - {2}";

		protected override WhsDocketLine[] GetDocketLineList(IOperationalActionSectionLog log, BusinessObject[] bizoList)
		{
			var result = new List<WhsDocketLine>();

			foreach (var bizO in bizoList)
			{
				if (bizO is WhsReceive receive)
				{
					if (receive.IsCreatedFromPickByBOM)
					{
						log.NotifyFormat(
							OperationalActionLogErrorLevel.Warning,
							OutputTextFormat,
							receive.HumanReadableName,
							GetDocketIdLink(receive),
							Res.GetString("0c78d2b3-3df7-44a9-ac87-761019c39dd6", "You cannot generate Orders for Receives created from Pick Orders."));
					}
					else
					{
						result.AddRange(receive.Lines.ToArray<WhsDocketLine>());
					}
				}
			}

			return result.ToArray();
		}

		#endregion
	}
}
