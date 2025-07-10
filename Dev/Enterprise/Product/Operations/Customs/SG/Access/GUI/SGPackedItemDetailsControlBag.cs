using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.SG.Access.GUI
{
	public class SGPackedItemDetailsControlBag : ControlBag
	{
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "There is no way to change static backing field of this property.")]
		public static SGPackedItemDetailsControlBag Instance { get; } = new SGPackedItemDetailsControlBag();

		SGPackedItemDetailsControlBag()
		{
			GoodsTypeDropEdit = RegisterControl(nameof(SGPackedItemDetailsCountrySpecificUserControl.GoodsTypeDropEdit));
			SGEdiTariffFindBox = RegisterControl(nameof(SGPackedItemDetailsCountrySpecificUserControl.SGEdiTariffFindBox));
			GSTPaidDropEdit = RegisterControl(nameof(SGPackedItemDetailsCountrySpecificUserControl.GSTPaidDropEdit));
		}

		protected override Control CreateTemplate() => new SGPackedItemDetailsCountrySpecificUserControl();

		public ControlReference GoodsTypeDropEdit { get; }

		public ControlReference SGEdiTariffFindBox { get; }

		public ControlReference GSTPaidDropEdit { get; }
	}
}
