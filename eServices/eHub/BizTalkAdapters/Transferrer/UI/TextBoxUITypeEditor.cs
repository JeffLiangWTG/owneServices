using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace CargoWise.eHub.BizTalkAdapters.Transferrer.UI
{
	public class TextBoxUITypeEditor : UITypeEditor
	{
		private IWindowsFormsEditorService service;
		private TextBox dialog;

		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			if (null != context && null != context.Instance)
			{
				return UITypeEditorEditStyle.Modal;
			}
			return base.GetEditStyle(context);
		}

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (null != context && null != context.Instance && null != provider)
			{
				service = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
				if (null != service)
				{
					dialog = new TextBox();
					if (value != null)
						dialog.TextBoxData = (string)value;
					if (service.ShowDialog(dialog) == DialogResult.OK)
					{
						value = dialog.TextBoxData;
					}
				}
			}
			return value;
		}
	}
}
