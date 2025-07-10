using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Grid.Internal;

namespace Enterprise.MasterFiles.GUI
{
	public abstract class MultiEmailColumnStyle : ZMultiControlColumnStyle
	{
		protected MultiEmailColumnStyle(Func<ZMultiCombinationControl> combinationControl, ZMultiControlColumnStyleInfo columnInfo) : base(combinationControl, columnInfo)
		{
			InsertCurrentUsersEmailAddressHotKey.Register(this, GridControl);
		}

		public override bool ProcessCmdKey(ref Message m, Keys keyData)
		{
			return Hotkeys.ProcessCmdKey(this, keyData);
		}

		public override bool ShouldProcessCmdKey(ref Message m, Keys keyData)
		{
			return !ReadOnly;
		}
	}

	public abstract class MultiEmailColumnStyleInfo : ZMultiControlColumnStyleInfo
	{
		public string EmailAddressPropertyName { get; set; }
	}
}
