using System;
using CargoWise.ComponentModel;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Orders.GUI
{
	public partial class PreAdviceExportToForwardingJobForm : ZChildForm
	{
		public PreAdviceExportToForwardingJobForm(JobShipmentPreplanning preAdvice, JobShipmentPreplanning.OrderShipmentCreationMode creationMode, INotifications notificationSubscriber)
			: base(preAdvice)
		{
			InitializeComponent();
			this.NotificationSubscriber = notificationSubscriber;
			this.CreationMode = creationMode;
		}
		readonly JobShipmentPreplanning.OrderShipmentCreationMode CreationMode;
		readonly INotifications NotificationSubscriber;

		public new JobShipmentPreplanning BusinessEntity
		{
			get { return (JobShipmentPreplanning)base.BusinessEntity; }
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			BusinessEntity.CreateConsolAndShipment(CreationMode, NotificationSubscriber, PreAdviceConversionHelper.ShowFormWithNewlyCreatedConsol());

			if (BusinessEntity.Validation.HasAirConsolMatchesWithSameMAWB)
			{
				Globals.Message.ShowWarning(Res.GetString("c9735963-7770-4971-b20b-79bbd8904ecc", "An air consol has been created for this Master Bill. Same Master Bill can be used only for one air consol. This form will now close."));
				Close();
			}
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}
	}
}
