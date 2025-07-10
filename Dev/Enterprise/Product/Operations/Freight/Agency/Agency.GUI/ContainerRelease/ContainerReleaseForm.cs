using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.DevTools;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using BusinessContext = CargoWise.Definitions.BusinessContext;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Agency.GUI
{
	public sealed partial class ContainerReleaseForm : ZChildForm
	{
		public ContainerReleaseForm(ReleaseHeader header)
			: base(header)
		{
			InitializeComponent();

			if (!this.IsDesignMode())
			{
				containerReleaseControl.ExposeSendMessage(header);
			}
		}

		#region Show

		public static void Show(Form parentForm, AgencyBooking booking, bool isReplacement)
		{
			if (!booking.IsInDatabase || booking.HasChanges)
			{
				Globals.Message.Show(Res.GetString("6a4f6df9-6866-42c7-800d-d2e6ae411075", "This booking has not been saved."));
			}
			else
			{
				BusinessObjectFactory newFactory = new BusinessObjectFactory();
				ShowCore(parentForm, newFactory.Load<AgencyBooking>(booking.PK), isReplacement);
			}
		}
		static void ShowCore(Form parentForm, AgencyBooking booking, bool isReplacement)
		{
			if (booking.JS_PackingMode != Constants.ContainerModes.FCL)
			{
				Globals.Message.Show(Res.GetString("5ce6da23-833b-4780-8b28-a89617e73d80", "This booking is not FCL."));
			}
			else if (booking.BookedContainers.Count == 0)
			{
				Globals.Message.Show(Res.GetString("7a25dbda-5364-4fa1-b2fc-24ec39e2b00e", "No containers have been entered against this booking."));
			}
			else if (booking.JS_CFSReference.IsEmpty)
			{
				Globals.Message.Show(Res.GetString("5d5e5bbc-56b5-48f9-bf96-2667bcdd0b0f", "Booking reference not entered yet."));
			}
			else
			{
				ReleaseHeader header = new ReleaseHeader(booking, isReplacement);
				header.Init();

				if (isReplacement)
				{
					if (header.Lookups.ReleaseNumbers.Count == 0)
					{
						Globals.Message.Show(Res.GetString("23c431b5-fab7-4e6e-8bbc-458834e6ebc5", "No containers have been released yet."));
					}
					else
					{
						ShowCore(parentForm, header);
					}
				}
				else
				{
					if (header.Details.Count == 0)
					{
						Globals.Message.Show(Res.GetString("56aa0ad6-b974-4126-90ae-8cc8ed019752", "All non-shipper owned containers have been released."));
					}
					else
					{
						ShowCore(parentForm, header);
					}
				}
			}
		}

		static void ShowCore(Form parentForm, ReleaseHeader header)
		{
			var form = new ContainerReleaseForm(header);
			ZFormModaliser.Show(form, parentForm);
		}

		#endregion

		#region Form Overrides

		public override string FormVerb
		{
			get { return ""; }
		}

		protected override ZMessageBox CreateErrorMessageBox(IBusiness businessEntityForValidation, bool includeIgnoreOption)
		{
			return new ZErrorMessageBox(businessEntityForValidation, Res.GetString("ef4799c5-50b6-4a07-8762-c5f59bdd24b1", "release"), Res.GetString("3a15e88f-2941-4d03-8596-86f7c5756cdc", "complete"), Res.GetString("3b5cbb61-7913-4a4a-894b-7acbcd97def8", "completed"), includeIgnoreOption);
		}

		protected override void PopulateDevTools(List<IDevTool> tools)
		{
			base.PopulateDevTools(tools);
			tools.Add(new ReleaseDocumentCustomisationDevTool());
		}

		#endregion

		#region Implementation

		bool ValidateAndRelease()
		{
			Header.RunPreSaveValidation();

			if (Header.HasErrors)
			{
				ShowErrorsDialog();
				return false;
			}
			else
			{
				return Release();
			}
		}

		bool Release()
		{
			bool shouldContinue = true;
			var notificationsHandler = new NotificationsHandler();

			Header.DoRelease(notificationsHandler);

			if (Header.IncludeMessage)
			{
				if (notificationsHandler.Notifications.Count > 0)
				{
					Globals.Message.Show(string.Join(System.Environment.NewLine, notificationsHandler.Notifications.Select(x => x.Message)));
				}
				else
				{
					Globals.Message.Show(Res.GetString("ef946965-95ab-426a-924b-18a96cee3e5a", "Message Sent."));
				}
			}

			try
			{
				Header.Factory.Save();
			}
			catch (ZSaveException ex)
			{
				shouldContinue = false;
				ZExceptionReporting.HandleSaveException(ex);
			}

			if (shouldContinue)
			{
				DeliverDocument();
			}

			return shouldContinue;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
		void DeliverDocument()
		{
			const string Release = "Container Release";
			const string ReleaseHack = "Container Release HACK";

			ZQuery filter = new DocumentZQuery(nameof(BusinessContext.ContainerRelease));
			filter.AddToFilter(new DocumentZQuery(StmMenuItemSchema.SU_MenuName, new string[] { Release, ReleaseHack }));

			DocumentCommand[] commandList = Header.Factory.Load<DocumentCommand>(filter);
			DocumentCommand command = Array.Find(commandList, (c) => c.SU_MenuName == Release);
			DocumentCommand commandHack = Array.Find(commandList, (c) => c.SU_MenuName == ReleaseHack);

			foreach (ReleaseInstance instance in Header.Instances)
			{
				command.Parent = instance;
				commandHack.Parent = instance;

				DocAutoDelivery autoDelivery = new DocAutoDelivery();
				UserControlProviderList providerList = new UserControlProviderList();

				using (DocumentPack pack = new DocumentPack(command, instance, providerList, command))
				{
					DeliveryInstructions instructions = new DeliveryInstructions(pack);
					instructions.AllowAutoDelivery = true;
					instructions.Destination = DeliveryInstructionDestination.Auto;
					instructions.Recipients.RemoveAll();
					instructions.Recipients.AddRange(autoDelivery.GetDeliveryContacts(command, instance.DocumentSupporter));
					instructions.Recipients.AddRange(autoDelivery.GetDeliveryContacts(commandHack, instance.DocumentSupporter));

					using (DocumentPrintSet set = new DocumentPrintSet(command, providerList))
					{
						set.RunWithPartialInstructions(AllowedDeliveryOptions.All, instructions, Env.Security.None);
					}
				}
			}
		}

		ReleaseHeader Header
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (ReleaseHeader)base.BusinessEntity; }
		}

		#endregion

		#region Events

		void cancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void releaseButton_Click(object sender, EventArgs e)
		{
			if (ValidateAndRelease())
			{
				Close();
			}
		}

		void selectAllButton_Click(object sender, EventArgs e)
		{
			Header.DoSelectAll();
		}

		#endregion
	}
}

#region Test
#if DEBUG

#region Testing Members

namespace Enterprise.Freight.Agency.GUI
{
	using System.Diagnostics.CodeAnalysis;

	partial class ContainerReleaseForm
	{
		[SuppressMessage("Microsoft.Design", "CA1030", Justification = "This definitely should not be an event")]
		public void FireReleaseButton()
		{
			releaseButton.PerformClick();
		}
	}
}

#endregion



#endif
#endregion
