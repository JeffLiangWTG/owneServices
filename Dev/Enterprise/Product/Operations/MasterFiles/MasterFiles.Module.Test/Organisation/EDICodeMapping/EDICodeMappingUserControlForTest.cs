using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class EDICodeMappingUserControlForTest : EDICodeMappingUserControl
	{
		public EDICodeMappingUserControlForTest(IBusinessObjectCollection gridCollection, EDICodeMappingFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
		}

		public ZFilterStrip NewZFilterStripForTest() => NewZFilterStrip();
	}
}
