using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public abstract class WhsDocketProcessTasks : ProcessTask, Enterprise.Integration.Warehouse.IWhsDocketProcessTask
	{
		#region Constructors

		protected WhsDocketProcessTasks(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#endregion

		#region Parent

		protected override Type ParentType
		{
			get { return typeof(WhsDocket); }
		}

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion
	}
}
