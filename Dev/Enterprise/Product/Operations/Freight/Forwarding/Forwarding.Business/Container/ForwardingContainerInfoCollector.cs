using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	internal class ForwardingContainerInfoCollector
	{
		public ForwardingContainerInfoCollector(BusinessObject bizo)
		{
			var dataRow = ((INeedRow)bizo).Row;
			var dataTable = dataRow.Table;
			dataTable.RowDeleting += DataRowDeleting_EventHandler;
		}

		public void DataRowDeleting_EventHandler(object sender, DataRowChangeEventArgs e)
		{
			if (e.Row.RowState == DataRowState.Deleted || e.Row.RowState == DataRowState.Detached)
			{
				return;
			}

			var pk = e.Row.Field<Guid>(JobContainerSchema.PK.Name);
			lastDeletedStack[pk] = new StackTrace().ToString();
		}

		public string GetLastRowDeletedStackTrace(Guid pk)
		{
			if (!lastDeletedStack.ContainsKey(pk))
			{
				return (NoResString)"No row deleted stack trace found"; // Error Reporter
			}

			return lastDeletedStack[pk];
		}

		readonly Dictionary<Guid, string> lastDeletedStack = new Dictionary<Guid, string>();
	}
}
