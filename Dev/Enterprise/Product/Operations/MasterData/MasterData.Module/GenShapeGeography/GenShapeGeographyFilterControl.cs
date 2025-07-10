using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterData.Module
{
	[CodeAlive("Implementing the classes for a new module, code will be referenced later")]
	public partial class GenShapeGeographyFilterControl : ZFilterStripControl
	{
		public GenShapeGeographyFilterControl()
		{
			InitializeComponent();
		}

		public GenShapeGeographyFilterControl(IBusinessObjectCollection gridCollection, GenShapeGeographyFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
