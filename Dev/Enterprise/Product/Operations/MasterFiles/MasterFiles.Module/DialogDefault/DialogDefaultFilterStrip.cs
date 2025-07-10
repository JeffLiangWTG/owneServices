using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using Enterprise.MasterFiles.Module.DialogDefault.OwnerFilter;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module.DialogDefault
{
	class DialogDefaultFilterStrip : ZFilterStrip
	{
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		protected override Control[] GetCurrentFilterControls(ZArchitecture.Business.ModuleFilter currentModuleFilter)
		{
			if (currentModuleFilter is DialogDefaultOwnerModuleFilter)
			{
				var item = new DialogDefaultOwnerControl();

				try
				{
					PreferredHeight = item.Height;

					return new Control[] { item };
				}
				catch
				{
					try
					{
						item.Dispose();
					}
					catch { }
					throw;
				}
			}

			return base.GetCurrentFilterControls(currentModuleFilter);
		}
	}
}
