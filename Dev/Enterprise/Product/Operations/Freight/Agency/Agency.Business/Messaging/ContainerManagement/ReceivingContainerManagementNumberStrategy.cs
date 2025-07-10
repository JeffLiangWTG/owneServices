using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Messaging.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class ReceivingContainerManagementNumberStrategy : IMessageNumberStrategy
	{
		public ReceivingContainerManagementNumberStrategy(BusinessObjectFactory factory, string sender)
		{
			this.factory = Argument.NotNull(factory, "factory");
			this.sender = sender;
		}

		readonly BusinessObjectFactory factory;
		readonly string sender;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "used for number generation not need to localise")]
		public const string ReceiverCode = "Enterprise";

		#region IMessageNumberStrategy Members

		public string GetMessageReferenceNumber()
		{
			return Env.NumberFountains.EDIFACTNumberFountain("M", sender, ReceiverCode).GetNextFormatted(factory);
		}

		#endregion
	}
}
