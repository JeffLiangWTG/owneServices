using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.DataTransfer.Testing
{
	public class DummyWithWorkflowTransactionDataObjectWriter : ITopLevelDataObjectWriter
	{
		public ZString EDIMessageSubType
		{
			get { return "XXX"; }
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
