using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.Environment;
using Enterprise.Freight.GUI;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DevTools;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.QuotedBookings.GUI
{
	public partial class PreAllocationForm : ZChildForm
	{
		public PreAllocationForm(PreAllocation preAllocation)
			: base(preAllocation)
		{
			InitializeComponent();
			PreAllocation = preAllocation;
			this.WarningLabel.Text = Res.GetString("PreAllocationForm|WarningLabel.Text", "Warning! <Count> Booking records have been created. Please ensure the above data is correct before saving.");
			this.JS_GoodsDescriptionBoundTextBox.ButtonText = Res.GetString("a03a349f-8774-42fe-bfc8-309816d9813b", "Details");
		}

		public PreAllocationForm()
		{
			InitializeComponent();
			this.WarningLabel.Text = Res.GetString("PreAllocationForm|WarningLabel.Text", "Warning! <Count> Booking records have been created. Please ensure the above data is correct before saving.");
			this.JS_GoodsDescriptionBoundTextBox.ButtonText = Res.GetString("a03a349f-8774-42fe-bfc8-309816d9813b", "Details");
		}

		#region PreAllocation

		PreAllocation PreAllocation
		{
			get { return preAllocation; }
			set { preAllocation = value; }
		}
		PreAllocation preAllocation;

		#endregion

		#region Overrides

		public override string FormVerb
		{
			get
			{
				return ZString.Empty;
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			AutoAddPreviousNextButtons = false;
			base.OnLoad(e);
			PrintButton.Enabled = false;

			MissingResourceStringChecker.ExcludeFromTest(PickupDocAddressControl);
			MissingResourceStringChecker.ExcludeFromTest(DeliveryDocAddressControl);
		}

		#endregion
		#region Button Clicks

		void SaveAndCloseButton_Click(object sender, EventArgs e)
		{
			ValidateAndCreate();
		}

#if DEBUG
		internal
#endif
		void PrintButton_Click(object sender, EventArgs e)
		{
			Print();
		}

		void TheCancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void zButton1_Click(object sender, EventArgs e)
		{
			PreAllocation.GenerateHouseBills();
		}

#if DEBUG
		internal
#endif
		void SaveButton_Click(object sender, EventArgs e)
		{
			try
			{
				PreAllocation.Factory.Save();
				PrintButton.Enabled = true;
				TheCancelButton.Text = Res.GetString("00f13bce-5545-471c-9889-98e181919e87", "Close");
				SaveButton.Enabled = false;
				WarningLabel.Text = Res.GetString("PreAllocationForm|WarningLabel.SaveButtonClick", "Save Complete. Print the Pre-Allocated House Bills by using the Print button.");
				DisplayMode = ODisplayMode.NewSaved;
			}
			catch (ZCannotSaveException saveException)
			{
				Globals.Message.ShowError(saveException.Message, saveException.Heading);
			}
		}

		#endregion

		#region ValidateAndCreate

		void ValidateAndCreate()
		{
			PreAllocation.RunPreSaveValidation();
			if (PreAllocation.HasErrors)
			{
				ShowErrorsDialog();
			}
			else
			{
				Create();
			}
		}

		#endregion

		#region Create

		void Create()
		{
			if (PreAllocation.State == PreAllocation.PreAllocationState.PrintOnly)
			{
				Globals.Message.ShowError(Res.GetString("d3bb95e8-9446-425c-a1a1-ca85fbed273f", "Already Pre-Allocated."));
			}
			else
			{
				PreAllocation.Create();
				RefreshButton.Enabled = false;
				CreateButton.Enabled = false;
				SaveButton.Enabled = true;

				string transportMode = preAllocation.QuotedBooking.TransportMode;
				WarningLabel.Text = Res.GetString("PreAllocationForm|WarningLabel.Create", "Warning! {0} Booking records have been created. Please ensure the above data is correct before saving.", PreAllocation.HouseBillCount);
				if (transportMode == Core.Constants.TransportModes.Sea || transportMode == Core.Constants.TransportModes.Rail)
				{
					WarningLabel.Text += " " + Res.GetString("PreAllocationForm|hblWarning.Create", "Including HBL Type.");
				}
				WarningLabel.Visible = true;
			}
		}

		#endregion

		#region Print

		void Print()
		{
			if (PreAllocation.State != PreAllocation.PreAllocationState.PrintOnly)
			{
				Globals.Message.ShowError(Res.GetString("391f5f16-4753-4640-833e-190857ff4761", "Please Create and Save before printing."));
				return;
			}

			DocumentCommand documentCommand = CreateDocumentCommand(GetMenuName());

			if (documentCommand != null)
			{
				using (var guiManager = ObjectFactory.Get<IDocumentDeliveryRestrictionGUIManager>())
				{
					documentCommand.CreditControlledDocumentDeliveryGUIManager = guiManager;
					DocumentPrintSet set = new DocumentPrintSet(documentCommand, new UserControlProviderList());
					set.RunWithDeliveryForm(AllowedDeliveryOptions.All, Env.Security.None);
				}
			}
		}

		protected DocumentCommand CreateDocumentCommand(string documentName)
		{
			ZString menuName = documentName;
			DocumentZQuery filter = new DocumentZQuery(StmMenuItemSchema.SU_MenuName, menuName);
			filter.AddToFilter(StmMenuItemSchema.SU_BusinessContext, BusinessContext.PreAllocation);

			DocumentCommand documentCommand = PreAllocation.Factory.LoadTop1<DocumentCommand>(filter);

			if (documentCommand != null)
			{
				documentCommand.Parent = PreAllocation;
			}

			return documentCommand;
		}

		protected string GetMenuName()
		{
			string menuName = null;
			string transportMode = preAllocation.QuotedBooking.TransportMode;

			if (PreAllocation.QuotedBooking.IsDomesticFreight && (transportMode == Core.Constants.TransportModes.Air || transportMode == Core.Constants.TransportModes.Road))
			{
				menuName = DocumentNames.DomesticHAWB;
			}
			else if (transportMode == Core.Constants.TransportModes.Air)
			{
				menuName = PreAllocation.IsPrePrinted ? DocumentNames.NeutralHAWB : DocumentNames.LaserHAWB;
			}
			else
			{
				menuName = PreAllocation.IsPrePrinted ? DocumentNames.BillOfLadingToPreprinted : DocumentNames.BillOfLading;
			}

			return menuName;
		}

		#region SuppressResourceStringsCheckRegion

		public static class DocumentNames
		{
			public const string DomesticHAWB = "Domestic HAWB";
			public const string NeutralHAWB = "Neutral HAWB";
			public const string LaserHAWB = "Laser HAWB";
			public const string BillOfLadingToPreprinted = "Bill Of Lading To Preprinted";
			public const string BillOfLading = "Bill Of Lading";
		}

		#endregion

		#endregion

		protected override void PopulateDevTools(List<IDevTool> tools)
		{
			base.PopulateDevTools(tools);
			tools.Add(new DocumentCustomizationTool());
		}

#if DEBUG
		public void CreateMockPreAllocationForTest()
		{
			HouseBillCountCalcEdit.CalcValue = 1;
			ValidateAndCreate();
		}
#endif
	}
}
