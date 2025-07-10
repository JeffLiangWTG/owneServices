using System;

namespace Enterprise.Rating.GUI
{
	[System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "2.0.0.0")]
	[System.Diagnostics.DebuggerNonUserCodeAttribute()]
	[System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
	internal class BulkUpdateWizardImages
	{
		[ThreadStatic] static System.Resources.ResourceManager resourceMan;
		[ThreadStatic] static System.Globalization.CultureInfo resourceCulture;

		[System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		internal BulkUpdateWizardImages()
		{
		}

		[System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
		internal static System.Resources.ResourceManager ResourceManager
		{
			get
			{
				if (ReferenceEquals(resourceMan, null))
				{
					System.Resources.ResourceManager temp = new System.Resources.ResourceManager("Enterprise.Rating.GUI.BulkRateUpdate.BulkUpdateWizard.BulkUpdateWizardImages", typeof(BulkUpdateWizardImages).Assembly);
					resourceMan = temp;
				}
				return resourceMan;
			}
		}

		[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
		internal static System.Globalization.CultureInfo Culture
		{
			get { return resourceCulture; }
			set { resourceCulture = value; }
		}

		internal static System.Drawing.Bitmap WizardFinishImage
		{
			get
			{
				object obj = ResourceManager.GetObject("WizardFinishImage", resourceCulture);
				return ((System.Drawing.Bitmap)(obj));
			}
		}

		internal static System.Drawing.Bitmap WizardStartImage
		{
			get
			{
				object obj = ResourceManager.GetObject("WizardStartImage", resourceCulture);
				return ((System.Drawing.Bitmap)(obj));
			}
		}
	}
}
