using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	public class MessageErrorCalculator : Messaging.Business.MessageErrorCalculator
	{
		public MessageErrorCalculator(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override ZString GetLongDescriptionCore(ZString errorCode, ZString initialDescription)
		{
			ZString result = initialDescription;
			MessageError abiError = ABIError.Instance.GetErrorInfoByCode(errorCode);
			if (abiError != null)
			{
				result += System.Environment.NewLine + System.Environment.NewLine + abiError.Narrative;
			}
			return result;
		}

		protected override bool IsAbnormalityMessageReportingDisable
		{
			get { return USCustomsDataRegistry.Instance.DisableAbnormalityMessageReporting.Value; }
		}
	}
}
