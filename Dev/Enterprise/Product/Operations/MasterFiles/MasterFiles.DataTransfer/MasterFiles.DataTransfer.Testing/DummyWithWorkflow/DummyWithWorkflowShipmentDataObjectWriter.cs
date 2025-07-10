using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.DataTransfer.Testing
{
	public class DummyWithWorkflowShipmentDataObjectWriter : ITopLevelDataObjectWriter
	{
		public ZString EDIMessageSubType
		{
			get { return EDIMessageSubTypeList.Codes.XmlUniversalShipment; }
		}

		public virtual ITopLevelDataObject GetDataObject(BusinessObject sourceBO)
		{
			var dummy = sourceBO as DummyWithWorkflow;
			bool useRealShipment = dummy != null && dummy.UseRealShipmentForInternalUniversalXMLSending;
			return useRealShipment
				? ((ITopLevelDataObjectWriter)new UniversalXmlWorkflowProcessorTest.DummyShipmentDataObjectWriter()).GetDataObject(sourceBO)
				: new DummyDO();
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
