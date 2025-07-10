using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Recruiter;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class GlbPersonModule : ZFilterGridModule, IOperationalActionSupportable
	{
		public GlbPersonModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		public override ModuleIdentifier ID => ModuleIDs.GlbPerson;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.PersonIntelligence;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.PersonIntelligence;

		public override bool AllowDefaultActivateDeactivate => true;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(selectedBusinessObject == null ? ControllerIDs.GlbPersonNew : ControllerIDs.GlbPerson);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new GlbPersonFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new GlbPersonFilterControl(GridCollection, (GlbPersonFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new GlbPersonCollection(Factory);
		}

		public override bool SupportsWorkflow
		{
			get { return true; }
		}

		public override string WorkflowType
		{
			get { return WorkflowDescriptors.GlbPersonDescriptorCode; }
		}

		public override BusinessContext[] BusinessContexts => new[] { BusinessContext.GlbPerson };

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems());

			result.Add(new ZMenuItem(ResString.GetMultilingualString("54D93DE0-7AD8-4CBF-8EE1-9D22F0526C18", "Bulk Create Accreditation Attempts"), OnSyncAccreditationClick));

			result.Add(new ZMenuItem(ResString.GetMultilingualString("627AE09B-B5CE-4299-AA4E-B50575F244C9", "Merge"), OnMergeClick));

			return result.ToArray();
		}

		void OnMergeClick(object sender, EventArgs e)
		{
			if (!Env.Security.PersonIntelligenceEdit.IsAllowed)
			{
				Env.Security.PersonIntelligenceEdit.ShowError();
				return;
			}

			const int minimumParticipants = 2;

			if (Grid.SelectedElements.Length < minimumParticipants)
			{
				Globals.Message.Show(ResString.GetMultilingualString("C141A263-6D21-41AA-89D3-779F3B0C0225", "Please select at least 2 or more Persons."));
			}
			else
			{
				var retainedPerson = ((GlbPerson)Grid.List[Grid.CurrentRowIndex]);

				if (Grid.SelectedElements.Contains(retainedPerson))
				{
					var dissolvedPersons = new List<GlbPerson>();

					dissolvedPersons.AddRange(Grid.SelectedElements.Cast<GlbPerson>().Where(person => person.PK != retainedPerson.PK));

					ObjectFactory.Get<IMasterDataProviderGUI>().OpenPersonMergeSummaryForm(retainedPerson, dissolvedPersons);

					retainedPerson.Reload();
				}
				else
				{
					Globals.Message.Show(ResString.GetMultilingualString("4CD8B6EE-1908-4C6C-AF2D-71B2DF0B2F77", "None of the selected rows have been set as the retained person. Please use CTRL + Click to set the retained person"));
				}
			}
		}

		IAccreditationUpdaterForPerson updater;
		void OnSyncAccreditationClick(object sender, EventArgs e)
		{
			if (!Env.Security.GlbAccreditationAttemptEdit.IsAllowed)
			{
				Env.Security.GlbAccreditationAttemptEdit.ShowError();
				return;
			}

			var rows = Grid.GetSelectedRows();
			if (rows.Length == 0)
			{
				Globals.Message.Show(ResString.GetMultilingualString("900976B9-E711-4327-B791-FCB59A8DFC99", "Please select a Person."));
				return;
			}

			if (updater == null)
			{
				updater = ObjectFactory.Get<IAccreditationUpdaterForPerson>();
			}

			updater.SetPersons(rows.OfType<IGlbPerson>().ToArray());
			var updaterForm = ObjectFactory.Get<IAccreditationUpdaterForm>("IAccreditationUpdaterForm", updater);
			ZFormModaliser.ShowDialogAndDispose((Form)updaterForm);
		}

		#region IOperationalActionSupportable Members

		OperationalActionSupporter IOperationalActionSupportable.OperationalActionSupporter
		{
			get { return new GlbPersonOperationalActionSupporter(); }
		}

		#endregion
	}
}
