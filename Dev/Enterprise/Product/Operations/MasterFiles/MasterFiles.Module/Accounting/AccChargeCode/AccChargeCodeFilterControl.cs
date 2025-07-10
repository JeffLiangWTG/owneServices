using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class AccChargeCodeFilterControl : ZFilterStripControl
	{
		public enum Mode
		{
			Normal,
			Global,
			Consolidation
		}

		public AccChargeCodeFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject, Mode mode)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			AdjustColumnsForGlobalOrLocal(mode);
		}

		void AdjustColumnsForGlobalOrLocal(Mode mode)
		{
			string[] removeList = System.Array.Empty<string>();
			switch (mode)
			{
				case Mode.Global:
					removeList = new[] { "IsLinkedToGlobalChargeCode", "IsDifferentToGlobalChargeCode", "Company+GC_Name", "Company+GC_Code", "AC_AT_GSTRate", "AC_AW_WithholdingTaxRate" };
					break;
				case Mode.Normal:
					removeList = new[] { "Company+GC_Name", "Company+GC_Code" };
					break;
			}

			if (!AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.Value || mode == Mode.Global)
			{
				System.Array.Resize(ref removeList, removeList.Length + 1);
				removeList[removeList.Length - 1] = "AC_GovtChargeCode";
			}

			foreach (ZGridColumnInfo columnStyle in this.grid.ColumnStyles.ToArray())
			{
				if (removeList.Contains(columnStyle.ColumnName))
				{
					this.grid.ColumnStyles.Remove(columnStyle);
				}
			}
		}
	}
}
