using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CargoWise.eHub.Portal.Models
{
	public class MessageTypeView
	{
		public Guid? Id { get; set; }
		public string Name { get; set; }
		public string Type { get; set; }
		public int MinLetters { get; private set; }

		public MessageTypeView()
		{
			MinLetters = Constants.MinLettersToStartMessageTypeSearch;
		}
	}
}