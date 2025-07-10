using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportConsignment.GUI // this is not for the modules, it is an embedded filter control on a form
{
	[ToolboxItem(false)]
#if DEBUG // For Designer quirk when dealing with abstract controls
	public class DtbChildFilterControl : ZFilterStripCommonControl
#else
	public abstract class DtbChildFilterControl : ZFilterStripCommonControl
#endif
	{
		protected DtbChildFilterControl(FilterStripBusinessObject filterStripBizO)
			: base(filterStripBizO)
		{
			IsFilterVisible = false;
		}

		#region Perform Search

		protected override ZBool ShouldPerformSearch()
		{
			return Visible && Grid.List is IBusinessObjectCollection; // Ensure is active and is bound to a collection (i.e. Not first search triggered before form is shown if relevant registry setting is on)
		}

		#endregion

		#region For Designer
#if DEBUG

		protected DtbChildFilterControl() // Required for designing sub classes
			: base()
		{
		}

#endif
		#endregion
	}
}
