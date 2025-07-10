using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsRMAOrder : NonPersistentBusinessObject
	{
		#region Constructor

		WhsRMAOrder(WhsOrder parentOrder, IEnumerable<WhsOrderLine> orderLines)
			: base(parentOrder.Factory)
		{
			Argument.NotNull(parentOrder, nameof(parentOrder));
			Argument.NotNull(parentOrder.Pick, nameof(parentOrder.Pick));
			Argument.NotNull(orderLines, nameof(orderLines));

			if (!parentOrder.Pick.IsFinalised)
			{
				throw new ArgumentException(Res.GetString("e7b5c2f4-042a-4e3b-867d-357d3461b6c4", "Must finalize Pick for Parent Order first."));
			}
			else if (!orderLines.Any())
			{
				throw new ArgumentException(Res.GetString("00fcbdb3-66ea-429f-814e-ddd237f8324f", "Must pass in Order Lines."));
			}

			OrderPK = parentOrder.PK;

			WhsRMAHelper.AddFetchHint(orderLines, parentOrder.Factory);
			Lines = new WhsRMAOrderLineCollection(parentOrder.Factory, WhsRMAHelper.GetWhsRMAOrderLines(orderLines, parentOrder));
		}

		public static WhsRMAOrder GetNew(WhsOrder parentOrder, IEnumerable<WhsOrderLine> orderLines)
		{
			var result = new WhsRMAOrder(parentOrder, orderLines);
			result.RegisterEditableChildObject(result.Lines);
			return result;
		}

		readonly ZGuid OrderPK;

		#endregion

		#region Related Entities

		#region Order

		public WhsOrder Order
		{
			get { return Factory.Load<WhsOrder>(OrderPK); }
		}

		#endregion

		#region Lines

		[ChildEditable(true)]
		public WhsRMAOrderLineCollection Lines { get; }

		#endregion

		#endregion

		#region GenerateRMAReceivesMessage

		public String GenerateRMAReceivesMessage()
		{
			var generatedReceiveList = new List<WhsReceive>();
			var originalOrder = Order;
			foreach (var rmaLineGroup in Lines.Cast<WhsRMAOrderLine>().Where(l => l.QuantityToReturn > 0m).GroupBy(l => l.WhsOverride))
			{
				var receive = Factory.New<WhsReceive>();
				receive.IsAutoCreatingReceive = true;
				receive.WD_DocketSubType = CodeLists.ReceiveType.Codes.Returns;
				receive.WD_OH_Client = originalOrder.WD_OH_Client;
				var warehousePK = rmaLineGroup.Key.IsEmpty ? originalOrder.WD_WW_Whs : rmaLineGroup.Key;
				receive.WD_WW_Whs = warehousePK;
				receive.WD_ExternalReference = originalOrder.WD_ExternalReference;
				receive.WD_WD_ParentDocket = originalOrder.PK;
				receive.IsUniqueExternalReferenceCreatedOnSave = true;
				generatedReceiveList.Add(receive);
				foreach (var rmaLine in rmaLineGroup)
				{
					WhsRMAHelper.BuildReceiveLine(receive, rmaLine);
				}
			}

			Factory.Save();

			var list = string.Join("\r\n", generatedReceiveList.OrderBy(r => r.WD_DocketID).Select(r => r.WD_DocketID));
			var msg = Res.GetString("c63a2879-9c15-47ef-921f-4dbfef5371b8", "Following Receives have been generated and can be accessed on Order [{0}] - Related Jobs tab:\r\n{1}", originalOrder.WD_DocketID, list);
			return msg;
		}

		#endregion
	}
}
