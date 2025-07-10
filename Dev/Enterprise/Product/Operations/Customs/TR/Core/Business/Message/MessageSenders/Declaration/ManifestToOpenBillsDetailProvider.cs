using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.TR.MessageContracts.Interfaces.Declaration;
using Enterprise.Customs.TR.Business.Declaration;

namespace Enterprise.Customs.TR.Business
{
	public class ManifestToOpenBillsDetailProvider : IOpeningTransportBills
	{
		public ManifestToOpenBillsDetailProvider(ManifestToOpenBill manifestToOpenBill)
		{
			ManifestToOpenBill = Argument.NotNull(manifestToOpenBill, nameof(manifestToOpenBill));
		}
		ManifestToOpenBill ManifestToOpenBill { get; }

		public string OpeningTransportBillNo => ManifestToOpenBill.TPD_DocumentNumber;

		public IReadOnlyCollection<IOpeningTransportBillLines> OpeningTransportBillLines
		{
			get
			{
				if (fOpeningTransportBillLines == null)
				{
					var list = new List<IOpeningTransportBillLines>();
					foreach (var pack in ManifestToOpenBill.Packs)
					{
						list.Add(new ManifestToOpenPackProvider(pack));
					}
					fOpeningTransportBillLines = list.ToArray();
				}
				return fOpeningTransportBillLines;
			}
		}
		IReadOnlyCollection<IOpeningTransportBillLines> fOpeningTransportBillLines;
	}
}
