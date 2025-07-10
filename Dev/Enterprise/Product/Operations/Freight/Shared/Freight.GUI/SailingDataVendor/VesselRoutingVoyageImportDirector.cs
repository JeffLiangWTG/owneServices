using System.Windows.Forms;
using Enterprise.Freight.SailingDataVendor.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.SailingDataVendor.GUI
{
	public class VesselRoutingVoyageImportDirector
	{
		public VesselRoutingVoyageImportDirector(Form parentForm, VesselRoutingVoyage[] voyages)
		{
			this.ParentForm = parentForm;
			this.Voyages = voyages;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		public bool Import()
		{
			using (VesselRoutingVoyageImportForm form = new VesselRoutingVoyageImportForm())
			{
				ShowImportForm(form);
				Importer.Import(form);

				form.NotifyImportCompleted();
				OnImportCompleted();
				while (form.Visible)
				{
					Application.DoEvents();
				}
				return form.KeepTickedPortPairs;
			}
		}

		protected virtual void ShowImportForm(VesselRoutingVoyageImportForm form)
		{
			ZFormModaliser.Show(form, ParentForm);
		}

		protected virtual void OnImportCompleted()
		{
		}

		#region Implementation

		readonly VesselRoutingVoyage[] Voyages;
		readonly Form ParentForm;

		VesselRoutingVoyageImporter Importer
		{
			get
			{
				if (fImporter == null)
				{
					fImporter = new VesselRoutingVoyageImporter(Voyages);
				}
				return fImporter;
			}
		}
		VesselRoutingVoyageImporter fImporter;

		#endregion
	}
}
