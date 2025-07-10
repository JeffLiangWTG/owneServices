using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Module
{
	public partial class EntryLineFilterControl : Customs.Module.EntryLineFilterControl
	{
		public EntryLineFilterControl()
		{
			InitializeComponent();
			InitializeGridColumns();
		}

		public EntryLineFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject)
			: base(gridCollection, filterStripBusinessObject)
		{
			InitializeComponent();
			InitializeGridColumns();
		}

		void InitializeGridColumns()
		{
			var firstArrivalDateColumn = FilteredGrid.GetColumnStyle("Declaration+JE_DateOfFirstArrival");
			if (firstArrivalDateColumn != null)
			{
				FilteredGrid.ColumnStyles.Remove(firstArrivalDateColumn);
			}

			var declarationDateColumn = FilteredGrid.GetColumnStyle("Header+DeclarationDate");
			if (declarationDateColumn != null)
			{
				FilteredGrid.ColumnStyles.Remove(declarationDateColumn);
			}

			var arrivalDateColumn = FilteredGrid.GetColumnStyle("Declaration+JE_DateOfArrival");
			if (arrivalDateColumn != null)
			{
				arrivalDateColumn.Caption = "Date at Disc. Port";
			}

			var entryZDateEdit = new ZArchitecture.ZDateEditColumnStyleInfo();
			entryZDateEdit.Caption = "Date at Entry Port";
			entryZDateEdit.IsVisible = false;
			entryZDateEdit.ColumnName = "Declaration+US_EntryDate";
			entryZDateEdit.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short;
			ControlDpiScalingHelper.SetWidth(ref entryZDateEdit, 100, true);
			FilteredGrid.ColumnStyles.Add(entryZDateEdit);

			var entrySchedD = new ZArchitecture.ZTextBoxColumnStyleInfo();
			entrySchedD.Caption = "Entry Port";
			entrySchedD.IsVisible = false;
			entrySchedD.ColumnName = "Declaration+US_SchDEntry";
			ControlDpiScalingHelper.SetWidth(ref entrySchedD, 60, true);
			FilteredGrid.ColumnStyles.Add(entrySchedD);

			var arrivalSchedD = new ZArchitecture.ZTextBoxColumnStyleInfo();
			arrivalSchedD.Caption = "Disc. Port";
			arrivalSchedD.IsVisible = false;
			arrivalSchedD.ColumnName = "Declaration+US_SchDArrival";
			ControlDpiScalingHelper.SetWidth(ref arrivalSchedD, 60, true);
			FilteredGrid.ColumnStyles.Add(arrivalSchedD);

			var importerOfRecordZGuidFindBox = new ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			importerOfRecordZGuidFindBox.BindToList = "Declaration.Lookups.ImportersList";
			importerOfRecordZGuidFindBox.Caption = "Importer Of Record";
			importerOfRecordZGuidFindBox.IsVisible = false;
			importerOfRecordZGuidFindBox.ColumnName = "Declaration+IOROrgPK";
			ControlDpiScalingHelper.SetWidth(ref importerOfRecordZGuidFindBox, 110, true);
			FilteredGrid.ColumnStyles.Add(importerOfRecordZGuidFindBox);
		}
	}
}
