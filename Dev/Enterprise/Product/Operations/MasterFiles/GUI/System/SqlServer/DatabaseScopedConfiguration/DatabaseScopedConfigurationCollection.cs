using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.GUI
{
	sealed class DatabaseScopedConfigurationCollection : NonPersistentBusinessObjectCollection<DatabaseScopedConfiguration>, IEnumerable<DatabaseScopedConfiguration>
	{
		public DatabaseScopedConfigurationCollection()
		{
		}

		public DatabaseScopedConfiguration Find(int id)
		{
			return Find(x => x.ConfigurationId == id).Single();
		}

		public bool CanBeSavedPotentially
		{
			get
			{
				var hasSavableItem = false;

				foreach (DatabaseScopedConfiguration configuration in Elements)
				{
					if (configuration.HasErrors)
					{
						return false;
					}

					if (configuration.CanBeSavedPotentially)
					{
						hasSavableItem = true;
					}
				}

				return hasSavableItem;
			}
		}

		public IEnumerator<DatabaseScopedConfiguration> GetEnumerator()
		{
			return Elements.Cast<DatabaseScopedConfiguration>().GetEnumerator();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotImplementedException();
		}

		protected override bool AllowNewCore => false;
		protected override bool AllowRemoveCore => false;
	}
}
