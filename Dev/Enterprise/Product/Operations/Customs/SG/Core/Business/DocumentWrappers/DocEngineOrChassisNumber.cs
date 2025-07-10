using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business.PermitPrinting;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.SG.V4.Business
{
	public class DocEngineOrChassisNumber : DocumentWrapper
	{
		protected DocEngineOrChassisNumber(IEngineOrChassisNumber engineOrChassisNumber, BusinessObjectFactory factoryToWrap)
			: base(engineOrChassisNumber, factoryToWrap)
		{
			this.engineOrChassisNumber = engineOrChassisNumber;
		}

		readonly IEngineOrChassisNumber engineOrChassisNumber;

		public static DocEngineOrChassisNumber New(IEngineOrChassisNumber engineOrChassisNumber, BusinessObjectFactory factoryToWrap)
		{
			return engineOrChassisNumber != null ? new DocEngineOrChassisNumber(engineOrChassisNumber, factoryToWrap) : null;
		}

		public ZString SequenceNumber => engineOrChassisNumber.SequenceNumber;

		public ZString Number => engineOrChassisNumber.Number;
	}

	public class DocEngineOrChassisNumberCollection : DocumentWrapperCollection
	{
		public DocEngineOrChassisNumberCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocEngineOrChassisNumber this[int index]
		{
			get { return (DocEngineOrChassisNumber)base[index]; }
		}
	}
}
