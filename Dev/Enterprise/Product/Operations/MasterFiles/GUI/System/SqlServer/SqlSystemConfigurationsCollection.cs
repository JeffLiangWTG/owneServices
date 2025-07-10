using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.GUI
{
	public class SqlSystemConfigurationsCollection : NonPersistentBusinessObjectCollection<SqlSystemConfiguration>
	{
		public virtual IEnumerable<SqlSystemConfiguration> Configurations => Elements.Cast<SqlSystemConfiguration>();

		public virtual IEnumerable<SqlSystemConfiguration> ChangedConfigs => Configurations.Where(x => x.HasProposedChange);

		public virtual bool HasProposedChanges => Configurations.Any(x => x.HasProposedChange);

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new SqlSystemConfiguration();
		}

		public override void Add(BusinessObject businessObject)
		{
			base.Add(businessObject);

			if (businessObject is SqlSystemConfiguration configuration)
			{
				configuration.PropertyChanged += ConfigurationOnPropertyChanged;
			}
		}

		void ConfigurationOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(SqlSystemConfiguration.ProposedValue))
			{
				HasProposedChangesChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		public event EventHandler HasProposedChangesChanged;

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;
	}
}
