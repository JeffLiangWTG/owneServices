using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.TR.MessageContracts.Interfaces.Declaration;
using CargoWise.Types;
using Enterprise.Customs.TR.Business.Declaration;

namespace Enterprise.Customs.TR.Business
{
	public class ManifestToOpenBillsHeaderProvider : ISummaryDeclaration
	{
		public ManifestToOpenBillsHeaderProvider(ManifestToOpenHeader manifestToOpenHeader)
		{
			ManifestToOpenHeader = Argument.NotNull(manifestToOpenHeader, nameof(manifestToOpenHeader));

			Bill = ManifestToOpenHeader.Bills.LastOrDefault();
		}

		ManifestToOpenHeader ManifestToOpenHeader { get; }
		ManifestToOpenBill Bill { get; }

		public IReadOnlyCollection<IOpeningTransportBills> OpeningTransportBills
		{
			get
			{
				if (fOpeningTransportBillLines == null)
				{
					var list = new List<IOpeningTransportBills>();

					foreach (var bill in ManifestToOpenHeader.Bills)
					{
						list.Add(new ManifestToOpenBillsDetailProvider(bill));
					}
					fOpeningTransportBillLines = list.ToArray();
				}
				return fOpeningTransportBillLines;
			}
		}
		IReadOnlyCollection<IOpeningTransportBills> fOpeningTransportBillLines;

		public string SummaryDeclarationNo => ManifestToOpenHeader.CE_EntryNum;
		public string SummaryDeclarationProcessContent => Bill != null && Bill.TPD_IncludeAllItems ? CusEntryMessageConstants.ScopeOfTransaction.IncludeAllItems : CusEntryMessageConstants.ScopeOfTransaction.NotIncludeAllItems;
		public string InWarehouse => Bill != null && Bill.TPD_IsInWarehouse ? CusEntryMessageConstants.TurkishAnswers.Yes : CusEntryMessageConstants.TurkishAnswers.No;
		public string OtherNature => Bill != null && Bill.TPD_IsOtherProcedure ? CusEntryMessageConstants.TurkishAnswers.Yes : CusEntryMessageConstants.TurkishAnswers.No;
		public string Description => ZString.Empty;
	}
}
