using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CargoWise.eHub.Portal.Models
{
	[Serializable]
	public class SelectValueView
	{
		public Guid? Id { get; set; }
		public string Code { get; set; }
		public string Name { get; set; }
		public string Type { get; set; }
		public int MinLetters { get; private set; }

		public SelectValueView()
		{
			MinLetters = Constants.MinLettersToStartClientSearch;
		}

		public string DisplayName
		{
			get
			{
				if (!String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(Code)) return String.Format("{0} ({1})", Name, Code);
				else return "";
			}
		}
	}
}