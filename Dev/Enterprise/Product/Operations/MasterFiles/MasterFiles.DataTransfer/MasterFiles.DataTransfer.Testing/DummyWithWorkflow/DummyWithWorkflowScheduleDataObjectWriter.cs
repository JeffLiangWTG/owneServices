using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.DataTransfer.Testing
{
	public class DummyWithWorkflowScheduleDataObjectWriter : ITopLevelDataObjectWriter
	{
		public ZString EDIMessageSubType
		{
			get { return "XUL"; }
		}

		public ITopLevelDataObject GetDataObject(BusinessObject sourceBO)
		{
			return new DummyDO();
		}

		public ZString RootElementName
		{
			get { return "UniversalDummyDO"; }
		}

		public DataContextType TopLevelDataContextType
		{
			get { return DataContextType.DummyBusinessObject; }
		}

		public void SetDataWritingManager(IDataWritingManager dataWritingManager)
		{
		}
	}
}