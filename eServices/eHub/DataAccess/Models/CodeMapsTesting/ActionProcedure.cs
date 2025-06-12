using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CargoWise.eHub.DataAccess.Models.CodeMapsTesting
{
	public class ActionProcedure
	{
		public string Procedure { get; set; }
		public string OutputParm { get; set; }
		public List<string> InputParms { get; set; }
		public string Result { get; set; }

		public ActionProcedure()
		{
		}
	}
}
