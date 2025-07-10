using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business.MessageSending
{
	public class LabelDetail : ILabelDetail
	{
		public LabelDetail(CusTWProductLabelRange label)
		{
			this.label = Argument.NotNull(label, nameof(label));
		}

		readonly CusTWProductLabelRange label;

		ZString ILabelDetail.EndNumber => label.TW0_EndNumber;

		ZString ILabelDetail.StartNumber => label.TW0_StartNumber;

		ZString ILabelDetail.Track => label.TW0_RunNumber;

		ZString ILabelDetail.Year => label.TW0_Year;
	}
}
