using System.Globalization;
using System.Windows.Forms;
using Enterprise.Freight.LocalCartage.Business;

namespace Enterprise.Freight.LocalCartage.GUI
{
	class AddressSelectionMenu : MenuItem
	{
		public AddressSelectionMenu(CartageBindToLists.AddressSelectionElement element)
		{
			this.Element = element;
			Text = string.Format(CultureInfo.CurrentCulture, "{0} :: {1} :: {2}", element.OrgCode, element.AddressShortCode, element.AddressAsASingleLine);
		}
		public readonly CartageBindToLists.AddressSelectionElement Element;
	}
}
