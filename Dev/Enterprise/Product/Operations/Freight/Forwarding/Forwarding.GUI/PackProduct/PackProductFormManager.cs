using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public class PackProductFormManager : MultipleItemFormManager<ForwardingPackLine>
	{
		public PackProductFormManager(ZGrid grid, string bindingPrefix = "")
			: base(grid, bindingPrefix)
		{
		}

		protected override ZString LinkLabelText
		{
			get { return Res.GetString("673A9F18-58F7-4d4d-834B-DC9E0F9B1012", "Multiple Products Details..."); }
		}

		protected override ZString[] ColumnsToUpdate
		{
			get { return new ZString[] { "Products+PackProductManager+Value" }; }
		}

		protected override void AddCountChangedEventHandlerToItemsCollection(ForwardingPackLine parent, EventHandler eventHandler)
		{
			if (parent != null)
			{
				parent.Products.CountChanged -= eventHandler;
				parent.Products.CountChanged += eventHandler;
			}
		}

		protected override int CollectionCount(ForwardingPackLine parent)
		{
			return parent.Products.Count;
		}

		protected override void AddColumnsCore(ZGrid gridToAddTo, string bindingPrefixWithPlus)
		{
			var zMultiControlColumnStyleInfo1 = new ZMultiControlColumnStyleInfo();
			zMultiControlColumnStyleInfo1.ColumnName = bindingPrefixWithPlus + "Products+PackProductManager+Value";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = bindingPrefixWithPlus + "Products+PackProductManager+FieldColumnType";
			zMultiControlColumnStyleInfo1.BindToList = bindingPrefixWithPlus + "Products+OrgSupplierPartProductCodes";
			zMultiControlColumnStyleInfo1.GroupName = Res.GetData("eb3ccc47-7a11-4908-b4c3-a726ffebf98d", "Products");
			zMultiControlColumnStyleInfo1.Caption = Res.GetString("4DB92326-2C3A-4aef-A97E-2533741EB8E7", "Product Code");
			zMultiControlColumnStyleInfo1.IsVisible = false;
			gridToAddTo.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
		}

		protected override MultilingualString MenuCaption
		{
			get { return ResString.GetMultilingualString("1f9db2d1-37fc-4ffd-af46-0e890ff43c9e", "Multiple Products"); }
		}

		protected override Shortcut MenuShortcut
		{
			get { return Shortcut.CtrlP; }
		}

		protected override void ShowMultipleItemFormCore(ForwardingPackLine parent, Form parentForm)
		{
			ZFormModaliser.Show(new PackProductsForm(parent), parentForm);
		}
	}
}
