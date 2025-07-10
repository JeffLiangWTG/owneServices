using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing.ProfitShare;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI.Consol.ProfitShareRedistribution
{
	public partial class ProfitShareRedistributionLogForm : ZChildForm
	{
		public ProfitShareRedistributionLogForm(ForwardingProfitShareRedistribution profitShareRedistribution)
		{
			Argument.NotNull(profitShareRedistribution, nameof(profitShareRedistribution));
			ProfitShareRedistribution = profitShareRedistribution;

			InitializeComponent();

			Text = Res.GetString("3e7cd350-68f7-45dc-bbbb-bca41174fffb", "Profit Shares Redistribution");
			LogsGroupBox.Text = Res.GetString("b4494aa7-942d-4797-9b7f-50d24e2a789e", "Logs");

			HyperlinkActions = new HyperlinkActionCollection();
			Logger = new ProfitShareRedistributionLogger();
			Logger.IndividualCompleted += OnShipmentCompleted;
			Logger.AllCompleted += OnAllShipmentsCompleted;
			Logger.OnLogging += OnLogging;

			SetProgressMax(ProfitShareRedistribution.Shipments.Count);
			UpdateProgressText();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				Logger.IndividualCompleted -= OnShipmentCompleted;
				Logger.AllCompleted -= OnAllShipmentsCompleted;
				Logger.OnLogging -= OnLogging;

				Logger.Dispose();
				ProgressBar.Dispose();
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		/// <summary>
		/// Holds the result of redistribution process
		/// </summary>
		public bool Result { get; set; }

		void OnShipmentCompleted(object sender, ProfitShareRedistributedEventArgs eventArgs)
		{
			completedShipments.Add(eventArgs.ShipmentPK);
			BumpProgress();
		}
		// Not sure if needed. Please remove when no use.
		readonly List<ZGuid> completedShipments = new List<ZGuid>();

		void OnAllShipmentsCompleted(object sender, EventArgs eventArgs)
		{
			ProgressBar.Value = ProgressBar.Maximum;
			UpdateProgressText(hasCompleted: true);
			IsProcessing = false;
		}

		void OnLogging(object sender, ProfitShareRedistributeLoggingEventArgs eventArgs)
		{
			var logLevel = eventArgs.Type;
			var message = eventArgs.Message;

			// Let's handle LogType.Debug later
			if (logLevel <= LogType.Information)
			{
				var builder = new RtfStringBuilder(HyperlinkActions);
				builder.SetForgroundColour(ColourForErrorLevel(logLevel));
				builder.Append(message);
				builder.AppendNewLine();
				builder.SetForgroundColour(null);

				RichTextActionManager.AppendBuilderContent(LogRichTextBox, builder);
			}
		}

		protected override void OnShown(EventArgs eventArgs)
		{
			Result = false;
			var redistributionProcess = new GatewayProfitRedistributionProcessor(ProfitShareRedistribution, Logger);
			IsProcessing = true;
			Result = redistributionProcess.Process();
		}

		protected override void OnClosing(CancelEventArgs eventArgs)
		{
			if (IsProcessing)
			{
				eventArgs.Cancel = true;
				Globals.Message.ShowInformation(Res.GetString("d3d4f2a6-2c3f-487b-8d65-44ef190fe5f2", "Please wait for the redistribution to complete."));
			}
			else
			{
				base.OnClosing(eventArgs);
				eventArgs.Cancel = false;
			}
		}

		void BumpProgress()
		{
			if (ProgressBar.Value < ProgressBar.Maximum)
			{
				ProgressBar.Value++;
				UpdateProgressText();
			}
		}

		void SetProgressMax(int maximum)
		{
			ProgressBar.Value = 0;
			ProgressBar.Maximum = maximum;
		}

		void UpdateProgressText(bool hasCompleted = false)
		{
			ProgressGroupBox.Text = hasCompleted
				? Res.GetString("060176fa-ac48-4f35-a03b-c2369b645ca1", "Completed")
				: Res.GetString("67864059-d5a0-4ca0-96a9-4a2b3f84a5d3", "{0} of {1}", ProgressBar.Value, ProgressBar.Maximum);
		}

		static Color? ColourForErrorLevel(LogType logLevel)
		{
			switch (logLevel)
			{
				case LogType.Debug:
					return Color.DarkViolet;
				case LogType.Warning:
					return Color.DarkOrange;
				case LogType.Error:
					return Color.DarkRed;
				default:
					return null;
			}
		}

		ProfitShareRedistributionLogger Logger { get; }
		readonly HyperlinkActionCollection HyperlinkActions;
		readonly ForwardingProfitShareRedistribution ProfitShareRedistribution;
		bool IsProcessing;
	}
}
