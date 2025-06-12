using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace CargoWise.eHub.Portal.Models
{
	public class MessageTypeFilterViewModel
	{
		public string Code { get; set; }
		private int selectedFormatID = -1;

		public List<SelectListItem> Formats { get; set; }

		public int SelectedFormatID
		{
			get
			{
				return selectedFormatID;
			}
			set
			{
				selectedFormatID = value;
			}
		}

		public void Fill()
		{
			Formats = new List<SelectListItem>();
			Formats.Add(new SelectListItem() { Value = "0", Text = "EDI" });
			Formats.Add(new SelectListItem(){ Value = "1", Text="XML"});
			Formats.Add(new SelectListItem() { Value = "2", Text = "FlatFile" });
		}
	}
}
