using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI
{
	public class ReferenceNumberFormManager : MultipleItemFormManager<PackLine>
	{
		public ReferenceNumberFormManager(ZGrid grid, string bindingPrefix = "")
			: base(grid, bindingPrefix)
		{
			columnsToUpdate = new List<ZString>();
		}

		readonly List<ZString> columnsToUpdate;

		protected override ZString LinkLabelText
		{
			get { return Res.GetString("BAC6D728-E06B-42A1-8712-2BB3D9846AA7", "Multiple Reference Numbers Details..."); }
		}

		protected override ZString[] ColumnsToUpdate
		{
			get { return columnsToUpdate.ToArray(); }
		}

		protected override void AddCountChangedEventHandlerToItemsCollection(PackLine parent, EventHandler eventHandler)
		{
			if (parent != null)
			{
				parent.CusEntryNums.CountChanged -= eventHandler;
				parent.CusEntryNums.CountChanged += eventHandler;
			}
		}

		protected override int CollectionCount(PackLine parent)
		{
			return parent.CusEntryNums.Count;
		}

		protected override void AddColumnsCore(ZGrid gridToAddTo, string bindingPrefixWithPlus)
		{
		}

		public void RemoveAllColumns(ZGrid gridToAddTo)
		{
			columnsToUpdate.Clear();
		}

		protected override MultilingualString MenuCaption
		{
			get { return ResString.GetMultilingualString("5A8E6F97-19E7-4116-8D96-40C3D1A7C1A2", "Reference Numbers"); }
		}

		protected override Shortcut MenuShortcut
		{
			get { return Shortcut.CtrlR; }
		}

		protected override void ShowMultipleItemFormCore(PackLine parent, Form parentForm)
		{
			ZFormModaliser.Show(new ReferenceNumberForm(parent), parentForm);
		}
	}
}
