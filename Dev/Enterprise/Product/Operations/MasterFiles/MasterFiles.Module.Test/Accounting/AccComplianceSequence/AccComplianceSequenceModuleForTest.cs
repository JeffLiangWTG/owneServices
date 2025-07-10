using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class AccComplianceSequenceModuleForTest : AccComplianceSequenceModule
	{
		public AccComplianceSequenceModuleForTest()
		{
		}

		public IFilterControl NewFilterControl
		{
			get
			{
				return GetNewFilterControl();
			}
		}

		public IBusinessObjectCollection NewGridCollection
		{
			get { return GetNewGridCollection(); }
		}

		public FilterBusinessObject NewFilterBusinessObject
		{
			get
			{
				return GetNewFilterBusinessObject();
			}
		}
	}
}
