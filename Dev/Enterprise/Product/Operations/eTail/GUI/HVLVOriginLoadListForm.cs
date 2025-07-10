using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Definitions.Ecommerce;
using CargoWise.EntityFramework;
using Enterprise.eTail.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.eTail.GUI
{
	public partial class HVLVOriginLoadListForm : ZTemplateForm, ICustomerServiceMenuSectionCodeOverridable
	{
		public HVLVOriginLoadListForm(HVLVOriginLoadList originLoadList)
			: base(originLoadList)
		{
			InitializeComponent();
			PlugIns.Add(ControllerIDs.DtbBooking);
			workflowTabPage.Initialize(originLoadList);

			if (HVLVOriginLoadList.IsFunctionalTesting)
			{
				itemsGrid.AllowDrop = OriginLoadList.HVL_Status == HVLVOriginLoadListStatus.Codes.Open;
				itemsGrid.DragEnter += ItemsGrid_DragIn;
				itemsGrid.DragOver += ItemsGrid_DragIn;
				itemsGrid.DragDrop += ItemsGrid_DragDrop;
			}
			else
			{
				ItemsTabPage.TabVisible = false;
			}
		}

		#region ICustomerServiceMenuSectionCodeOverridable Members

		string ICustomerServiceMenuSectionCodeOverridable.SectionCode =>
			ModuleTreeCustomerServiceMenuSectionList.Codes.System;

		#endregion

		public override string FormCaption => OriginLoadList.HumanReadableName;

		protected override bool ShowAuditTab => true;

		HVLVOriginLoadList OriginLoadList => (HVLVOriginLoadList)BusinessEntity;

		void IncoTermExplainButton_Click(object sender, EventArgs e)
		{
			var originLoadList = OriginLoadList;
			if (originLoadList != null && !originLoadList.HVL_INCOInfo.HasErrors())
			{
				ZFormModaliser.Show(new Freight.GUI.IncoTermDescriptionForm(originLoadList.HVL_INCO), ParentForm as ZForm);
			}
		}

		#region Item Grid Drag & Drop

		void ItemsGrid_DragDrop(object sender, DragEventArgs e)
		{
			var format = e.Data.GetFormats().FirstOrDefault();
			if (format != null)
			{
				var data = e.Data.GetData(format);
				Drop(data);
			}
		}

		void Drop(object obj)
		{
			if (obj != null)
			{
				if (obj is HVLVItem item)
				{
					AllocateItem(item);
				}
				else if (obj is HVLVConsignment consignment)
				{
					foreach (HVLVItem childItem in consignment.Items)
					{
						AllocateItem(childItem);
					}
				}
				else if (obj is BusinessObject[] bizos)
				{
					foreach (var bizo in bizos)
					{
						Drop(bizo);
					}
				}
			}
		}

		void ItemsGrid_DragIn(object sender, DragEventArgs e)
		{
			e.Effect = DragDropEffects.Link;
		}

		void AllocateItem(HVLVItem droppedItem)
		{
			if (OriginLoadList.Factory.ImportFromAnotherFactory(droppedItem) is HVLVItem item && item.HVI_HVL_LoadList.IsEmpty)
			{
				OriginLoadList.RegisterEditableChildObject(item);
				OriginLoadList.Items.Add(item);
			}
		}

		#endregion
	}
}
