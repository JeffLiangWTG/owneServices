using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;


namespace CargoWise.eHub.BizTalkAdapters.Common.UI
{
    public class TextBoxUITypeEditor : UITypeEditor
    {
        private IWindowsFormsEditorService service = null;
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
                this.service = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
                if (null != this.service)
                {
                    this.dialog = new TextBox();
                    if (value != null)
                        this.dialog.TextBoxData = (string)value;
                    if (this.service.ShowDialog(this.dialog) == DialogResult.OK)
                    {
                        value = this.dialog.TextBoxData;
                    }
                }
            }
            return value;
        }
    }
}
