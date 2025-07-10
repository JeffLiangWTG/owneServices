using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Scanning;

namespace Enterprise.Packing.GUI
{
	public partial class PackOrUnpackItemsDialog : ZChildForm, INotifications, INotificationSubscriberQueryUser
	{
		#region Construction

		public PackOrUnpackItemsDialog(ItemsBusinessObject parent, bool isScanPacking, bool isUserEnteringQty)
			: base(parent)
		{
			Argument.NotNull(parent, "parent");

			IsUnpack = parent is UnpackItemsBusinessObject;
			IsScanPacking = isScanPacking;
			IsUserEnteringQty = isUserEnteringQty;

			if (IsScanPacking && IsUserEnteringQty)
			{
				var scanningItemsBizO = BizO as IScanningItemsBusinessObject;
				IsUserEnteringTUNQty = scanningItemsBizO != null && scanningItemsBizO.IsTUN;
			}

			InitializeComponent();
			InitializeForPackOrUnpack();
			HookEvents();

			MainStatusBar.Visible = false;
		}

		void InitializeForPackOrUnpack()
		{
			AddPackOrRemoveColumn();
			CommonOkButton.Text = OkText;
		}

		ItemsBusinessObject BizO
		{
			get { return (ItemsBusinessObject)DataSource; }
		}

		protected readonly bool IsUnpack;
		protected readonly bool IsScanPacking;
		protected readonly bool IsUserEnteringQty;
		protected readonly bool IsUserEnteringTUNQty;

		#region Columns

		void AddPackOrRemoveColumn()
		{
			if (!IsScanPacking || (IsUserEnteringQty && !IsUserEnteringTUNQty)) // for TUN, the user instead enters the Package (TUN) amount.
			{
				var column = new ZCalcEditColumnStyleInfo();
				column.BindToDecimalPlaces = null;
				column.IsMandatory = true;
				ControlDpiScalingHelper.SetWidth(ref column, 80, true);
				column.CaptionResourceString = IsUnpack ? Res.GetData("PackItemsDialog|e8dc33a0-ae00-4609-a26a-205b8e018751", "Qty", "Remove Qty", "Qty to Remove") :
															Res.GetData("PackItemsDialog|7abe5f3e-df9c-4154-b141-535cef219541", "Qty", "Pack Qty", "Qty to Pack");
				column.ColumnName = IsUnpack ? PackableItemParentWrapper.Schema.ProposedRemoveQty : PackableItemParentWrapper.Schema.ProposedPackQty;
				column.IsReadOnly = IsUserEnteringTUNQty;
				Grid.ColumnStyles.Add(column);
			}
		}

		#endregion

		#endregion

		#region FormHeading

		public override string FormHeading
		{
			get { return BizO.ProposedPackUnpackDescription + "..."; }
		}

		#endregion

		#region Hook / Unhook Events

		void HookEvents()
		{
			Grid.AfterBind += new EventHandler(Grid_AfterBind);

			// if scan packing a single item, we are in Qty Scan mode. The below then lets the user type in a Qty and press Enter to Pack.
			if (IsScanPacking && BizO.PackableItemParentsForBinding.Count == 1)
			{
				Grid.EnterKeyPressed += (sender, e) =>
				{
					AcceptButton.PerformClick();
					e.Handled = true; // without this, the value will not be correctly committed
				};
			}
		}

		void UnhookEvents()
		{
			if (barcodes != null)
			{
				barcodes.NonSystemBarcodeScanned -= new EventHandler<BarcodeScanEventArgs>(NonSystemBarcodeScanned);
			}

			if (Grid != null)
			{
				Grid.AfterBind -= new EventHandler(Grid_AfterBind);

				if (Grid.ListManager != null)
				{
					Grid.Enter -= new EventHandler(Grid_Enter);
					Grid.ListManager.PositionChanged -= new EventHandler(ListManager_PositionChanged);
				}
			}
		}

		#endregion

		#region OnShown / AfterGridBind

		// on shown

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			EnableScanning(Barcodes, null, GridPanel); // no reason to not always enable this.
			InitialiseGUI();
			ResizeGridToFitForm();
		}

		void ResizeGridToFitForm()
		{
			if (!PackageQtyTextBox.Visible)
			{
				ControlDpiScalingHelper.SetHeight(GridPanel, GridPanel.Height + GridPanel.Top - PackageDropEdit.Top, false);
				ControlDpiScalingHelper.SetTop(ref GridPanel, PackageDropEdit.Top, false);
			}
		}

		// after bind

		void Grid_AfterBind(object sender, EventArgs e)
		{
			AddCustomPropertyColumnsToGrid();
		}

		void AddCustomPropertyColumnsToGrid()
		{
			var dataSource = ((ItemsBusinessObject)Grid.DataSource).PackableItemParentsForBinding;
			if (dataSource != null)
			{
				var customColumnsInitializer = new ZActiveGridCustomColumnsInitializer(
					Grid, dataSource, Res.GetData("f41db3c1-f1e6-4d71-96bb-3ae9aa1c6ba9", "Extra Details"), isVisible: true, isReadonly: true);

				customColumnsInitializer.HookCollection();
			}
		}

		#endregion

		#region Flags

		bool IsPackingIntoExistingPackages
		{
			get
			{
				var packItemsBizO = DataSource as PackItemsBusinessObject;
				return (packItemsBizO != null && packItemsBizO.IsPackableIntoExistingPackages);
			}
		}

		#endregion

		#region InitialiseGUI

		void InitialiseGUI()
		{
			UpdatePackTypeDropListVisibility();
			InitialiseGuiForScanning();
		}

		void UpdatePackTypeDropListVisibility()
		{
			if (!IsUserEnteringTUNQty && (IsUnpack || IsPackingIntoExistingPackages))
			{
				PackageQtyTextBox.Visible = false;
				PackageDropEdit.Visible = false;
			}
			else
			{
				PackageQtyTextBox.Visible = !IsScanPacking || IsUserEnteringTUNQty;
				PackageDropEdit.Visible = !IsScanPacking || IsUserEnteringTUNQty;
				PackageDropEdit.Enabled = !IsUserEnteringTUNQty;
			}
		}

		void InitialiseGuiForScanning()
		{
			if (IsScanPacking)
			{
				switch (BizO.Mode)
				{
					case PackOrUnpackMode.Attribs: // Scan all mode and products with attributes
					case PackOrUnpackMode.TUN_Qty_Attribs: // Scan Quantity mode with attributes on products and scanned barcode has TUN
						DisableLineQtyEntryAndEnableRowSelection();
						ShowScanningMessageToScanNextAttrubte(BizO, true);
						break;

					case PackOrUnpackMode.TUN_Qty: // No attributes for products but in TUN Qty mode eg. 2 UNT = 1PLT 
						var tunCode = ((IScanningItemsBusinessObject)BizO).TUNCode;
						ShowScanningMessage(Res.GetString("6ffc402a-58ca-4e4e-bc8c-c17329161732", "Enter or Scan the {0} Quantity to {1}.", tunCode, OkText), NotificationTypes.None, showOkButton: false);
						PackageQtyTextBox.Focus();
						break;

					case PackOrUnpackMode.Qty: // Scan quanity for non TUN barcodes.
											   // TUN means there is unit conversions specified for the product barcode.
						ShowScanningMessageToScanNextAttrubte(BizO, true);
						Grid.Focus(); // lets the user type immediately
						break;

					default:
						throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "PackOrUnpackMode {0} is not supported by the Pack/Unpack Dialog.", BizO.Mode));
				}
			}
		}

		void DisableLineQtyEntryAndEnableRowSelection()
		{
			// disallow qty entry. we must allow selection however, in case the attribute barcode is unscannable, and so we enable selection via whole-row.
			Grid.ReadOnly = true;
			Grid.IsWholeRowSelectedOnClick = true;
			Grid.Enter += new EventHandler(Grid_Enter);
			Grid.ListManager.PositionChanged += new EventHandler(ListManager_PositionChanged);
		}

		#endregion

		#region Scanning

		#region Barcodes

		BarcodeManager Barcodes
		{
			get
			{
				if (barcodes == null)
				{
					barcodes = new BarcodeManager();
					barcodes.NonSystemBarcodeScanned += new EventHandler<BarcodeScanEventArgs>(NonSystemBarcodeScanned);
				}
				return barcodes;
			}
		}

		BarcodeManager barcodes;

		#endregion

		#region Non-System Barcodes (eg Product Attribute)

		bool IsRebuildingPackableItemParents;

		void NonSystemBarcodeScanned(object sender, BarcodeScanEventArgs e)
		{
			using (new DisposableAction(() => IsRebuildingPackableItemParents = true, () => IsRebuildingPackableItemParents = false))
			{
				var bizO = BizO as IScanningItemsBusinessObject;
				if (bizO != null)
				{
					HandleAttributeBarcodeScanForPacking(e.Barcode, bizO);
				}
			}
		}

		void HandleAttributeBarcodeScanForPacking(string barcode, IScanningItemsBusinessObject bizO)
		{
			if (bizO.AddFilterIfValidAttribute(barcode))
			{
				if (BizO.CanAutoApplyChanges)
				{
					HandleOkButton();
				}
				else
				{
					ShowScanningMessageToScanNextAttrubte(BizO, false);
				}
			}
			else if (BizO.CanScanPackQuanity) // If there is only one line then we may be able to scan quantity
			{
				UpdateScannedQuantity(barcode);
			}
			else
			{
				ShowScanningMessage(Res.GetString("2b61b124-d97c-45c5-8abe-134563ff4919", "Barcode {0} is not a valid Attribute.", barcode), NotificationTypes.Error);
			}
		}

		void UpdateScannedQuantity(string barcode)
		{
			var nonNumericCharacters = new Regex(@"[^0-9.]");
			var isValidQty = Decimal.TryParse(nonNumericCharacters.Replace(barcode, ""), out decimal quantity);
			if (!isValidQty)
			{
				ShowScanningMessage(Res.GetString("6e802156-c777-42a7-8d57-5b3c6ca2e781", "Quantity scanned is invalid. Enter or Scan valid Quantity to {0}.", OkText), NotificationTypes.Error);
			}
			else if (quantity > Int32.MaxValue)
			{
				ShowScanningMessage(Res.GetString("4ff0fedc-7fae-4d47-8240-db2b0168b008", "The number '{0}' you have scanned is too large. The maximum value allowed is '{1}'.", quantity, Int32.MaxValue), NotificationTypes.Error);
			}
			else
			{
				if (BizO.Mode == PackOrUnpackMode.TUN_Qty || BizO.Mode == PackOrUnpackMode.TUN_Qty_Attribs)
				{
					BizO.PackageQtyToCreate = (ZInt)quantity;
				}
				else
				{
					BizO.SetPackOrRemoveQuantity(BizO.PackableItemParentsForBinding.Single<PackableItemParentWrapper>(), quantity);
				}

				HandleOkButton();
			}
		}

		void ShowScanningMessageToScanNextAttrubte(ItemsBusinessObject bizO, bool isInitialisingGui)
		{
			if (BizO.CanScanPackQuanity)
			{
				ShowScanningMessage(Res.GetString("68986041-19ad-412a-ad20-067a0f266aa7",
							"Enter or Scan the Quantity to {0}.", OkText), NotificationTypes.None, showOkButton: false);
			}
			else if (bizO.Mode == PackOrUnpackMode.Attribs || bizO.Mode == PackOrUnpackMode.TUN_Qty_Attribs)
			{   // There won't be one line. When it down to one line we will pack it directly. This is the scan all mode for products with Attributes
				var message = isInitialisingGui ?
								Res.GetString("3cc00cf5-18a7-4596-b23f-801659b96c92", "Scan the Item Attribute or select it in the Grid and click {0}.", OkText) :
								Res.GetString("c3c0776a-03b4-497b-aa38-9256e9ad3cea", "Scan the next Item Attribute or select it in the Grid and click {0}.", OkText);

				ShowScanningMessage(message, NotificationTypes.None, showOkButton: false);
			}
			else if (bizO.Mode == PackOrUnpackMode.Qty)
			{
				ShowScanningMessage(Res.GetString("24ba3ed0-273f-436d-994d-3eb075602ff0",
						"Type in the Quantity to {0}. Alternatively, scan attributes until there is a single line, then scan the Quantity to {0}.", OkText), NotificationTypes.None, showOkButton: false);
				Grid.Focus();
			}
		}

		#endregion

		#region Row Selection when Scanning

		void Grid_Enter(object sender, EventArgs e)
		{
			if (Grid.ListManager.Position == 0)
			{
				SelectPackableItemParent(Grid.ListManager.GetCurrent() as PackableItemParentWrapper);
			}
		}

		void ListManager_PositionChanged(object sender, EventArgs e)
		{
			SelectPackableItemParent(Grid.ListManager.GetCurrent() as PackableItemParentWrapper);
		}

		#endregion

		#region IncrementSelectedPackableItemParentQty

		void SelectPackableItemParent(PackableItemParentWrapper wrapper)
		{
			if (!IsRebuildingPackableItemParents)
			{
				((IScanningItemsBusinessObject)BizO).SelectPackableItemParent(wrapper);
			}
		}

		#endregion

		#endregion

		#region OK

		void CommonOkButton_Click(object sender, EventArgs e)
		{
			HandleOkButton();
		}

		protected void HandleOkButton()
		{
			if (BizO.RunValidationAndApplyChanges())
			{
				DialogResult = DialogResult.OK;
				Close();
			}
			else if (IsScanPacking)
			{
				ShowScanningMessage(BizO.GetErrors().GetFirst().Message, NotificationTypes.Error);
			}
			else
			{
				ShowErrorsDialog();
			}
		}

		protected string OkText
		{
			get { return BizO.ProposedPackUnpackDescription; }
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
				UnhookEvents();
			}

			base.Dispose(disposing);
		}

		#endregion

		//

		#region INotifications

		void INotifications.Add(INotification notification)
		{
			Globals.Message.Show(notification);
		}

		#endregion

		#region INotificationSubscriberQueryUser

		void INotificationSubscriberQueryUser.QueryUser(IQueryUserEventArgs e)
		{
			var args = (QueryUserMsgBoxEventArgs)e;
			args.Response = Globals.Message.Show(args.Message, args.Caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
		}

		#endregion
	}

	#region ItemsGrid

	internal class ItemsGrid : ZGrid
	{
		public event EventHandler<KeyEventArgs> EnterKeyPressed;

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			bool result = base.ProcessCmdKey(ref msg, keyData);

			if (keyData == Keys.Enter && EnterKeyPressed != null)
			{
				var e = new KeyEventArgs(keyData);
				EnterKeyPressed(this, e);
				result = e.Handled;
			}

			return result;
		}
	}

	#endregion
}
