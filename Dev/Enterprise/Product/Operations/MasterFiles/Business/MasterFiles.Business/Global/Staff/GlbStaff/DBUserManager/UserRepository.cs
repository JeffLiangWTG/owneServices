using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class UserRepository : NonPersistentBusinessObject, IObsoleteValidation
	{
		public UserRepository(BusinessObjectFactory factory, string sqlPassword)
			: base(factory)
		{
			this.sqlPassword = sqlPassword;
		}

		public bool ValidateLogin(out string reason)
		{
			reason = null;
			try
			{
				if (GlbStaff.CurrentUser.IsDatabaseDeveloper)
				{
					if (Db.Connection.DatabaseExists(DbUserManager.UserRepositoryDb))
					{
						using (var conn = GetDbConnection())
						{
							conn.EnsureIsOpen();
							return true;
						}
					}
					else
					{
						reason = (NoResString)string.Format("{0} does not exist.", DbUserManager.UserRepositoryDb);
					}
				}
				else
				{
					reason = (NoResString)"Your staff record is not configured for database developer access. Contact your system administrator.";
				}

				return false;
			}
			catch (SqlException ex)
			{
				if (new DbErrorMatch(ex).ExceptionType == DbErrorType.LoginFailedForUser)
				{
					reason = ex.Message;
					if (reason.Contains((NoResString)"user ''")) //https://learn.microsoft.com/en-us/sql/relational-databases/errors-events/mssqlserver-18456-database-engine-error?view=sql-server-ver16
					{
						reason = (NoResString)string.Format(
							@"{0}

An empty string means that SQL Server tried to hand off the credentials to the Local Security Authority Subsystem Service (LSASS) but couldn't because of some problem. Either LSASS wasn't available, or the domain controller couldn't be contacted.

Check the event logs on the client and the server for any network-related or Active Directory-related messages that were logged around the time of the failure. If you find any, work with your domain administrator to fix the issues.",
							reason);
					}
					return false;
				}
				else
				{
					throw;
				}
			}
			catch (ArgumentException ex) when (ex.Message.Contains((NoResString)"The value's length for key 'password' exceeds it's limit of '128'."))
			{
				return false;
			}
		}

		readonly string sqlPassword;

		#region Load database objects

		public IEnumerable<IDatabaseObject> GetMainDbObjects()
		{
			return LoadDatabaseObjectList<MainDatabaseObjectInfo>();
		}

		public IEnumerable<IDatabaseObject> GetUserRepositoryDbObjects()
		{
			return LoadDatabaseObjectList<UserRepositoryDatabaseObjectInfo>();
		}

		List<IDatabaseObject> LoadDatabaseObjectList<T>() where T : DatabaseObjectInfo, new()
		{
			var dbName = new T().BaseDatabase;
			var supportedTypes = new T().SupportedTypeList;
			var sqlText = String.Format(UserRepositoryScripts.ObjectListSql, dbName, supportedTypes);
			var result = new List<IDatabaseObject>();

			using (var cmd = MainConnection.Command(sqlText))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					result.Add(new T()
					{
						ObjectId = Convert.ToInt32(reader["ObjectId"]),
						ObjectSchema = reader["ObjectSchema"].ToString(),
						ObjectType = reader["ObjectType"].ToString(),
						ObjectName = reader["ObjectName"].ToString()
					});
				}
			}

			return result;
		}

		#endregion

		#region Execute SQL

		public void Execute(string sql)
		{
			if (GlbStaff.CurrentUser.IsDatabaseDeveloper)
			{
				if (Db.Connection.DatabaseExists(DbUserManager.UserRepositoryDb))
				{
					using (var userRepConn = GetDbConnection())
					{
						ExecuteWithErrorHandling(userRepConn, sql);
					}
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("4dcbf2db-7b98-419d-b5ca-d687e8a51f16", "The User Repository Database does not exist. Please untick and re-tick your Database Reader (and Developer) access in your staff profile. Contact your administrator if you don't have access to change it.")
						, Res.GetString("0aa78d9f-e899-4cb3-9812-401ef75348eb", "Unable to execute this action"));
					return;
				}
			}
			else
			{
				Globals.Message.ShowError("'" + GlbStaff.CurrentUser.GS_LoginName + "' " + Res.GetString("ED9F7A79 -EFFD-4F84-B397-7E827EFB7DE7", "is not defined as a Database Developer. Please ensure you are set as a Database Developer in your staff profile. Contact your administrator if you don't have access to change it.")
					, Res.GetString("0aa78d9f-e899-4cb3-9812-401ef75348eb", "Unable to execute this action"));
				return;
			}
		}

		internal DbConnection GetDbConnection()
		{
			var integratedSecurityEnabled = ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled;

			if (integratedSecurityEnabled)
			{
				return Db.NewExtraConnectionIntegratedSecurityEnabled(Db.ServerName, DbUserManager.UserRepositoryDb);
			}
			else
			{
				var loginPrefix = DbUserRepository.GetStaffDbLoginFullPrefix(Db.DatabaseName);
				var login = new ZString(loginPrefix + GlbStaff.CurrentUser.GS_LoginName).SubstringSafe(0, 128);
				return Db.NewExtraConnection(Db.ServerName, DbUserManager.UserRepositoryDb, login, sqlPassword);
			}
		}

		void ExecuteWithErrorHandling(DbConnection connection, string sql)
		{
			try
			{
				ExecuteInTransaction(connection, sql);
			}
			catch (SqlException ex)
			{
				var errorType = new DbErrorMatch(ex).ExceptionType;
				if (errorType == DbErrorType.ObjectAlreadyExists)
				{
					var message = Res.GetString("d093e0cb-4dfd-42b9-9640-1fc2726ef282",
						"{0} Note that names used in the main database are reserved, and cannot be used in the user repository.", ex.Message);
					throw new Exception(message, ex);
				}
				else if (errorType == DbErrorType.PermissionDeniedInDatabase
					|| errorType == DbErrorType.LoginIsNotAbleToAccessDatabaseUnderCurrentSecurityContext)
				{
					var message = Res.GetString("6be44579-51fc-43b1-ba37-bfc2258082d1",
						"{0} Note that you only have permissions on the user repository database.", ex.Message);
					throw new Exception(message, ex);
				}
				else
				{
					throw;
				}
			}
		}

		static void ExecuteInTransaction(DbConnection connection, string sqlText)
		{
			using (var manager = connection.BeginTransactionWithManager())
			{
				using (var command = connection.Command(sqlText))
				{
					command.ExecuteNonQuery();
				}

				manager.CommitTransaction();
			}
		}

		#endregion

		#region Database objects

		public interface IDatabaseObject
		{
			string GetCreateScript();
			string ObjectSchema { get; }
			string ObjectType { get; }
			string ObjectName { get; }
		}

		public interface IUserRepositoryDatabaseObject : IDatabaseObject
		{
			void Drop();
		}

		abstract class DatabaseObjectInfo : IDatabaseObject
		{
			internal abstract string BaseDatabase { get; }
			internal abstract string SupportedTypeList { get; }

			internal int ObjectId { get; set; }
			public string ObjectSchema { get; internal set; }
			public string ObjectType { get; internal set; }
			public string ObjectName { get; internal set; }

			string IDatabaseObject.GetCreateScript()
			{
				string sql;
				string formatItem1;
				string formatItem2;
				if (ObjectType == "USER_TABLE")
				{
					sql = UserRepositoryScripts.TableDefinitionSql;
					formatItem1 = ObjectName;
					formatItem2 = ObjectSchema;
				}
				else
				{
					sql = UserRepositoryScripts.OtherDefinitionSql;
					formatItem1 = ObjectId.ToString();
					formatItem2 = "";
				}

				using (var command = Db.Connection.Command(String.Format(sql, BaseDatabase, formatItem1, formatItem2)))
				{
					var result = command.ExecuteScalar();
					if (result == null)
					{
						return Res.GetString("605610dc-88ad-4400-b5a2-12949e19d9ec", "[{0}] not found", ObjectName);
					}

					if (result == DBNull.Value)
					{
						return Res.GetString("953f77e5-f48c-4e12-b35e-0b1f10cc959e", "[{0}] may not be retrievable due to insufficient access rights; the text is encrypted.", ObjectName);
					}

					return RemoveHeaderComments(result.ToString().Replace("[NewLine]", System.Environment.NewLine));
				}
			}

			protected virtual string RemoveHeaderComments(string objectScript)
			{
				return objectScript;
			}
		}

		class MainDatabaseObjectInfo : DatabaseObjectInfo
		{
			internal override string BaseDatabase
			{
				get { return Db.DatabaseName; }
			}

			internal override string SupportedTypeList
			{
				get { return (NoResString)"'FN', 'IF', 'TF', 'V', 'P'"; }
			}

			protected override string RemoveHeaderComments(string objectScript)
			{
				return SuppressHeaderCommentRegex.Replace(objectScript, "").Trim();
			}

			static readonly Regex SuppressHeaderCommentRegex = new Regex(
				@"^.*(?=\bCREATE\s+(FUNCTION|PROC(EDURE)?|VIEW)\b)",
				RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
		}

		class UserRepositoryDatabaseObjectInfo : DatabaseObjectInfo, IUserRepositoryDatabaseObject
		{
			internal override string BaseDatabase
			{
				get { return DbUserManager.UserRepositoryDb; }
			}

			internal override string SupportedTypeList
			{
				get { return (NoResString)"'FN', 'IF', 'TF', 'V', 'P', 'U'"; }
			}

			void IUserRepositoryDatabaseObject.Drop()
			{
				var sqlDrop = String.Format((NoResString)"DROP {0} [{1}].[{2}]", SqlObjectType, ObjectSchema, ObjectName);
				var sqlExec = String.Format((NoResString)"EXEC [{0}]..sp_executesql N'{1}'", DbUserManager.UserRepositoryDb, DataUtils.EscapeSingleQuotes(sqlDrop));
				ExecuteInTransaction(Db.Connection, sqlExec);
			}

			string SqlObjectType
			{
				get
				{
					switch (ObjectType)
					{
						case "VIEW":
						case "PROCEDURE":
							return ObjectType;
						case "USER_TABLE":
							return "TABLE";
						case "USER_TYPE":
							return "TYPE";
						default:
							return "FUNCTION";
					}
				}
			}
		}

		#endregion

		DbConnection MainConnection
		{
			get { return ((IDbConnected)Factory).Connection; }
		}
	}
}
