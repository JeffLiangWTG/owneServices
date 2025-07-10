using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business.N5203
{
	public class Constituent : IConstituent
	{
		public Constituent(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = invoiceLine;
		}

		public ZString ElementDescription => invoiceLine.JI_Compositions.ExcludeNonValidXMLCharacters();

		public ZString LevelID => ZString.Empty;

		public ZString Thickness => ZString.Empty;

		readonly JobComInvoiceLine invoiceLine;
	}
}
