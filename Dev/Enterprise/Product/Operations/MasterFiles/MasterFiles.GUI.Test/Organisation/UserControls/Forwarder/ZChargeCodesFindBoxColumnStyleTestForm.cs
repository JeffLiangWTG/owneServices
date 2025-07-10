using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	public partial class ZChargeCodesFindBoxColumnStyleTestForm : ZForm
	{
		public ZChargeCodesFindBoxColumnStyleTestForm(BusinessObject bizObj, bool addMultiFindColumnFirst)
			: base(bizObj)
		{
			this.addMultiFindColumnFirst = addMultiFindColumnFirst;
		}

		readonly bool addMultiFindColumnFirst;
	}
}
