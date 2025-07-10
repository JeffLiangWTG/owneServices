using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.MX
{
	sealed class HouseAirwayBillSpecialHandling : DocDataObject
	{
		public HouseAirwayBillSpecialHandling(object id) : base(id)
		{
		}

		#region CodeAndDescription

		public CodeDescription CodeAndDescription
		{
			get => codeAndDescription;
			set => codeAndDescription = SetChild(codeAndDescription, value);
		}

		CodeDescription codeAndDescription;

		#endregion Code
	}
}
