using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.MasterFiles.GUI
{
	sealed class DatabaseScopedConfigurationViewModel : NonPersistentBusinessObject, IDisposable, INotifyPropertyChanged
	{
		public DatabaseScopedConfigurationViewModel() : this(CreateContainers(Db.DatabaseName))
		{
		}

		public DatabaseScopedConfigurationViewModel(string mainDbName) : this(CreateContainers(mainDbName))
		{
		}

		internal DatabaseScopedConfigurationViewModel(DatabaseScopedConfigurationContainerCollection containers)
		{
			Containers = containers;
		}

		static DatabaseScopedConfigurationContainerCollection CreateContainers(string mainDbName)
		{
			var containers = new DatabaseScopedConfigurationContainerCollection();
			foreach (var db in GetAllExclusiveDatabases())
			{
				containers.Add(new DatabaseScopedConfigurationContainer(db.connection, db.dbName));
			}

			return containers;

			IEnumerable<(AdminConnection connection, string dbName)> GetAllExclusiveDatabases()
			{
				var connection = Db.NewAdminConnection(Db.ServerName, mainDbName);
				var exclusiveDbs = connection.GetDatabases(DatabaseType.AllExclusive);
				foreach (var dbName in exclusiveDbs)
				{
					yield return (connection, dbName);
				}

				if (!exclusiveDbs.Contains(Db.AuditDatabaseName, StringComparer.OrdinalIgnoreCase)
					&& TryGetBiServerConnectionIfNeeded(Db.AuditDatabaseName, DbRegistry.BiAuditServer.LoadValue(connection), out var auditConnection))
				{
					yield return (auditConnection, Db.AuditDatabaseName);
				}

				if (!exclusiveDbs.Contains(Db.EdwDatabaseName, StringComparer.OrdinalIgnoreCase)
					&& TryGetBiServerConnectionIfNeeded(Db.EdwDatabaseName, DbRegistry.BiDataWarehouseServer.LoadValue(connection), out var edwConnection))
				{
					yield return (edwConnection, Db.EdwDatabaseName);
				}
			}

			bool TryGetBiServerConnectionIfNeeded(string dbName, string serverName, out AdminConnection connection)
			{
				if (string.IsNullOrEmpty(serverName))
				{
					connection = null;
					return false;
				}

				var connection2 = Db.NewAdminConnection(serverName, Db.SqlMasterDb);
				if (connection2.DatabaseExists(dbName))
				{
					connection = connection2;
					return true;
				}

				connection2.Dispose();
				connection = null;
				return false;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		void RaisePropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public DatabaseScopedConfigurationContainerCollection Containers { get; }

		DatabaseScopedConfigurationContainer selectedDatabaseContainer;
		public DatabaseScopedConfigurationContainer SelectedDatabaseContainer
		{
			get => selectedDatabaseContainer;
			private set
			{
				if (selectedDatabaseContainer == value)
				{
					return;
				}

				UnRegisterEditableChildObject(selectedDatabaseContainer);
				selectedDatabaseContainer = value;
				RegisterEditableChildObject(value);
				RaisePropertyChanged(nameof(SelectedDatabaseContainer));
			}
		}

		ZString selectedDatabase;

		[ResourceStringData("24A81F94-4F5C-4BD7-9962-CE237DE436F3", Caption = "Selected Databases", ShortCaption = "Databases", MediumCaption = "Databases", FullDescription = "Selected databases for configuration")]
		[List(nameof(Containers))]
		public ZString SelectedDatabase
		{
			get
			{
				return selectedDatabase;
			}
			set
			{
				SetNonPersistentPropertyValue(SelectedDatabaseInfo, ref selectedDatabase, value);
				SelectedDatabaseContainer = Containers.Where(x => x.Code == selectedDatabase).FirstOrDefault();
			}
		}

		public ZPropertyInfo SelectedDatabaseInfo => GetZPropertyInfo(nameof(SelectedDatabase));

		public bool CanBeSavedPotentially
			=> Containers.Where(x => x.CanBeSavedPotentially).Any();

		public void Save()
		{
			var exceptions = new List<Exception>();

			foreach (var container in Containers.Where(x => x.CanBeSavedPotentially))
			{
				try
				{
					container.Save();
				}
				catch (AggregateException aggregateException)
				{
					exceptions.Add(aggregateException);
				}
			}

			if (exceptions.Count != 0)
			{
				throw new AggregateException(exceptions);
			}
		}

		public void Dispose()
		{
			Containers.Select(x => x.Connection)
				.Distinct()
				.ForEach(x => x.Dispose());
		}

		public sealed class DatabaseScopedConfigurationContainerCollection : NonPersistentBusinessObjectCollection<DatabaseScopedConfigurationContainer>
		{
			public DatabaseScopedConfigurationContainerCollection()
			{
			}

			protected override BusinessObject CreateNonPersistentBusinessObject()
			{
				throw new NotImplementedException();
			}

			protected override bool AllowNewCore => false;
			protected override bool AllowRemoveCore => false;
		}
	}
}
