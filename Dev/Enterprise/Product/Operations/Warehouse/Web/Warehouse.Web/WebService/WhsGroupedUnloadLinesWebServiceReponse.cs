using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public class WhsGroupedUnloadLinesWebServiceResponse : WebServiceResponse
	{
		#region Constructors

		public WhsGroupedUnloadLinesWebServiceResponse()
			: base()
		{
			PalletID = string.Empty;
		}

		#endregion

		#region Properties

		public WhsGroupedUnloadLineInfoCollection GroupedLines
		{
			get { return groupedLines ?? (groupedLines = new WhsGroupedUnloadLineInfoCollection()); }
			set { groupedLines = value; }
		}

		WhsGroupedUnloadLineInfoCollection groupedLines;

		public string PalletID { get; set; }

		#endregion
	}
}
