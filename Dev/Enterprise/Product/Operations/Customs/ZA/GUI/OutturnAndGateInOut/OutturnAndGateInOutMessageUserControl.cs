using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.GUI
{
	public partial class OutturnAndGateInOutMessageUserControl : ZUserControl
	{
		public OutturnAndGateInOutMessageUserControl()
		{
			InitializeComponent();

			var queryInterchangeCreator = new QueryInterchangeCreator(zGridMessage);
			queryInterchangeCreator.AddColumnAndMenuForQuery(true);
		}
	}
}
