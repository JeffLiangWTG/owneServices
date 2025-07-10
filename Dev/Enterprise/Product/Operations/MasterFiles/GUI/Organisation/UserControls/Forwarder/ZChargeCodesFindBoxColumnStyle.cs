using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.MasterFiles.GUI
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
	public class ZChargeCodesFindBoxColumnStyle : ZCodeFindBoxColumnStyle
	{
		public ZChargeCodesFindBoxColumnStyle(ZChargeCodesFindBoxColumnStyleInfo columnInfo)
			: base(() => new ZGridChargeCodesFindBox(), columnInfo)
		{
		}

		protected ZChargeCodesFindBoxColumnStyle(Func<ZGridChargeCodesFindBox> gridGuidMultiFindBox, ZChargeCodesFindBoxColumnStyleInfo columnInfo)
			: base(gridGuidMultiFindBox, columnInfo)
		{
		}

		internal ZGridChargeCodesFindBox MultiFindBox
		{
			get { return (ZGridChargeCodesFindBox)base.FindBox; }
		}

		protected override void HookControlEvents()
		{
			base.HookControlEvents();
			MultiFindBox.PopupSelected += GuidMultiFindBox_PopupSelected;
		}

		protected override void UnHookControlEvents()
		{
			MultiFindBox.PopupSelected -= GuidMultiFindBox_PopupSelected;
			base.UnHookControlEvents();
		}

		void GuidMultiFindBox_PopupSelected(object sender, EmbeddedModulePopup.SelectedEventArgs e)
		{
			IsEditing = true;
		}

#if DEBUG

		internal protected new string ColumnTextAtRow(CurrencyManager source, int rowNum) {
			return base.ColumnTextAtRow(source, rowNum);
		}

#endif

		#region Implementation

		bool initialised;

		protected override object GetColumnValueAtRow(CurrencyManager source, int rowNum)
		{
			object result = "";
			if (initialised) // prevents DataGridTextBox displaying GUID
			{
				result = base.GetColumnValueAtRow(source, rowNum);
			}

			return result;
		}

		protected override void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string instantText, bool cellIsVisible)
		{
			initialised = false;
			base.Edit(source, rowNum, bounds, readOnly, instantText, cellIsVisible);

			readOnly = IsCellReadOnly(source, rowNum);
			if (readOnly && !(parentDataGrid is ZDisplayGrid))
			{
				InitialiseEditControlForNewPositionWithCurrent(source, readOnly);
				TextBox.Text = ColumnTextAtRow(source, rowNum);
				TextBox.SelectAll();
			}
		}

		protected override void UpdateUI(CurrencyManager source, int rowNum, string displayText)
		{
			TextBox.Text = ColumnTextAtRow(source, rowNum);
		}

		protected override void SetValueInFindBox(CurrencyManager source, int rowNum)
		{
			FindBox.Code = ColumnTextAtRow(source, rowNum);
		}

		protected override void OnGettingColumnTextAtRow(CurrencyManager source, int rowNum)
		{
			base.OnGettingColumnTextAtRow(source, rowNum);
			initialised = true;
		}

		#endregion
	}

	public class ZChargeCodesFindBoxColumnStyleInfo : ZCodeFindBoxColumnStyleInfo
	{
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Type ColumnStyleType
		{
			get { return typeof(ZChargeCodesFindBoxColumnStyle); }
		}
	}
}
