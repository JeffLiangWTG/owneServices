using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;

namespace CargoWise.eHub.DataModel.Common
{
	public class ContextBase : DbContext
	{
		public ContextBase()
		{
			int commandTimeout;
			if (int.TryParse(ConfigurationManager.AppSettings[this.GetType().Name + ".CommandTimeout"], out commandTimeout))
			{
				this.CommandTimeout = commandTimeout;
			}
		}

		public ContextBase(string connectionString)
			: base(connectionString)
		{
		}

		public virtual new Database Database
		{
			get
			{
				throw new NotImplementedException("Database property cannot be accessed directly. Please use ContextBase methods and properties instead.");
			}
		}

		public virtual IDbTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
		{
			this.transaction = base.Database.BeginTransaction(isolationLevel);
			return this.transaction.UnderlyingTransaction;
		}
		DbContextTransaction transaction;

		public virtual int ExecuteSqlCommand(string sql, params object[] parameters)
		{
			return base.Database.ExecuteSqlCommand(sql, parameters);
		}

		public virtual IDbConnection Connection
		{
			get
			{
				return base.Database.Connection;
			}
		}

		public virtual int? CommandTimeout
		{
			get
			{
				return base.Database.CommandTimeout;
			}
			set
			{
				base.Database.CommandTimeout = value;
			}
		}

		public virtual IEnumerable<T> SqlQuery<T>(string sql, params object[] parameters)
		{
			return base.Database.SqlQuery<T>(sql, parameters);
		}

		public virtual Action<string> Log
		{
			get { return base.Database.Log; }
			set { base.Database.Log = value; }
		}
	}
}
