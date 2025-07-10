using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.CFS.Business
{
	public static class LabelsName
	{
		public static string ImportLabel
		{
			get { return (NoResString)"Import Label"; }
		}

		public static string OnForwardingLabel
		{
			get { return (NoResString)"On Forwarding Label"; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Document Name")]
		public static string TranshipmentLabel
		{
			get { return "Transhipment Label"; }
		}
	}
}
