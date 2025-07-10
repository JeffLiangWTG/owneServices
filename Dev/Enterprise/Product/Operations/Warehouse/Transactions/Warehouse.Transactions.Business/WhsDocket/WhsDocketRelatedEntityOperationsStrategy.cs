using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsDocketRelatedEntityOperationsStrategy
	{
		public WhsDocketRelatedEntityOperationsStrategy(WhsDocket parent)
		{
			Parent = Argument.NotNull(parent, "parent");
		}

		protected readonly WhsDocket Parent;

		#region EventLogExists

		public bool EventLogExists(ZString code)
		{
			return EventLogExistsCore(code);
		}

		protected virtual bool EventLogExistsCore(ZString code)
		{
			return (Parent.Logs.Find(GetLogFilter(code)).Length > 0);
		}

		protected ZQuery GetLogFilter(ZString code)
		{
			var logFilter = new ZQuery(StmALogSchema.SL_Parent, Parent.PK);
			logFilter.AddToFilter(StmALogSchema.SL_SE_NKEvent, code);
			logFilter.AddToFilter(StmALogSchema.SL_IsCancelled, false);
			logFilter.AddToFilter(StmALogSchema.SL_IsEstimate, false);

			return logFilter;
		}

		#endregion

		#region GetCurrentMaxLineNo

		public ZShort GetCurrentMaxLineNo(ZShort maxLineNo)
		{
			return GetCurrentMaxLineNoCore(maxLineNo);
		}

		protected virtual ZShort GetCurrentMaxLineNoCore(ZShort maxLineNo)
		{
			if (maxLineNo == 0)
			{
				foreach (WhsDocketLine line in GetAllDocketLines())
				{
					if (maxLineNo < line.WE_LineNo)
					{
						maxLineNo = line.WE_LineNo;
					}
				}
			}

			return maxLineNo;
		}

		WhsDocketLineCollection GetAllDocketLines()
		{
			var pickableDocket = Parent as WhsPickableDocket;

			return (pickableDocket != null)
				? pickableDocket.AllLines
				: Parent.Lines;
		}

		#endregion
	}
}
