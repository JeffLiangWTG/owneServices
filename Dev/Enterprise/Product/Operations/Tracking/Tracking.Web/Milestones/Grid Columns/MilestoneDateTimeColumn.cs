using System.Web.UI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	/// <summary>
	/// Column to present DateTime values in the grid
	/// Proper implementation should involve inheriting from Template column
	/// </summary>
	public class MilestoneDateTimeColumn : ZDateTimeColumn
	{
		public MilestoneDateTimeColumn(string headerText, string bindTo)
			: base(headerText, bindTo)
		{
		}

		public MilestoneDateTimeColumn(string headerText, string bindTo, ZDateTimePickerFormat dateFormat)
			: base(headerText, bindTo, dateFormat)
		{
		}

		protected override ITemplate GetItemTemplateCore()
		{
			return new MilestoneDateTimeColumnEditItemTemplate(this);
		}

		protected override ITemplate GetEditItemTemplateCore()
		{
			return new MilestoneDateTimeColumnEditItemTemplate(this);
		}
	}
}
