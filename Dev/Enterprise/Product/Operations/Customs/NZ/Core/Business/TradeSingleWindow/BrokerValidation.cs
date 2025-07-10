using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	public class BrokerValidation
	{
		public BrokerValidation(IDeclarant declarant)
		{
			this.declarant = Argument.NotNull(declarant, "declarant");
		}
		readonly IDeclarant declarant;

		public ZString ValidateAgentAndBrokerDetails(bool checkDeclarantId = true)
		{
			var brokerErrors = new ZStringBuilder();

			if (string.IsNullOrEmpty(NZCustomsDataRegistry.Instance.NZBrokerageID.Value))
			{
				brokerErrors.Append(MissingBrokerIdMessage);
			}

			if (checkDeclarantId && declarant.DeclarantID.IsEmpty)
			{
				brokerErrors.Append(MissingDeclarantIdMessage);
			}

			if (!declarant.Communications.Any())
			{
				brokerErrors.Append(MissingDeclarantCommunicationsMessage);
			}

			return brokerErrors.ToStringWithNewLineBetweenAppends();
		}

		public const string MissingBrokerIdMessage = "This company does not have a Broker ID recorded in the system registry.\r\nPlease edit the Registry details to add your Companys Unique Broker ID Number.\r\n";
		public const string MissingDeclarantIdMessage = "You have not set up your Declarant code to enable sending of this declaration.\r\nPlease edit your staff record to add your Declarant code to the Brokerage tab.\r\n";
		public const string MissingDeclarantCommunicationsMessage = "You have not set a means of communication in your staff record, which is required to enable sending of this declaration.\r\nPlease edit your staff record to add at least 1 of your telephone number, mobile number, fax and/or email address.\r\n";
	}
}
