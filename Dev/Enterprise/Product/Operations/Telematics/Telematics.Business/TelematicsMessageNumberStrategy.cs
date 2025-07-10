using System;
using CargoWise.Data;
using Enterprise.Environment;
using Enterprise.Messaging.Business;

namespace Enterprise.Telematics.Business
{
	public class TelematicsMessageNumberStrategy : IMessageNumberStrategy
	{
		public TelematicsMessageNumberStrategy(IDbConnected connectedObject)
		{
			if (connectedObject == null)
			{
				throw new ArgumentNullException(nameof(connectedObject));
			}

			this.connectedObject = connectedObject;
		}

		readonly IDbConnected connectedObject;

		#region IMessageNumberStrategy

		public string GetMessageReferenceNumber()
		{
			return Env.NumberFountains.TelematicsEDIMessageNumber.GetNextFormatted(connectedObject);
		}

		#endregion
	}
}
