using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.V4.Module
{
	public class ClassificationFilterStripControl : Customs.Module.CusClassificationFilterControl
	{
		public ClassificationFilterStripControl(IBusinessObjectCollection gridCollection, ClassificationFilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
		}
	}
}
