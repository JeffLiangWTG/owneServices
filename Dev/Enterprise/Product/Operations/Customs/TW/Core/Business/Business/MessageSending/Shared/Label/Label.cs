using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business.MessageSending
{
	public class Label : ILabel
	{
		public Label(ZString statusNameCode, IEnumerable<ILabelDetail> labelDetails)
		{
			StatusNameCode = statusNameCode;
			LabelDetails = labelDetails;
		}

		public ZString StatusNameCode { get; }

		public IEnumerable<ILabelDetail> LabelDetails { get; }

		public static IEnumerable<ILabel> GetLabels(CusTWProductLabelRangeCollection labelRanges) => labelRanges.Cast<CusTWProductLabelRange>().OrderBy(x => x.TW0_Status).GroupBy(x => x.TW0_Status).Select(x => new Label(x.Key, x.Select(ld => new LabelDetail(ld))));
	}
}
