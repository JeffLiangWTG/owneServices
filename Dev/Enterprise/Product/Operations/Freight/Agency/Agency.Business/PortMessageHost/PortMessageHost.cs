using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class PortMessageHost : AutoPortMessageHost, IEDIMessageCollectionProvider
	{
		public PortMessageHost(ISailingEndPoint endPoint)
			: base(endPoint.Factory)
		{
			this.endPoint = endPoint;
		}

		#region Properties

		#region InnerPK

		public override ZGuid EndPointPK
		{
			get { return endPoint.PK; }
		}

		#endregion

		#region Port

		public override ZString Port
		{
			get { return endPoint.Port; }
		}

		#endregion

		#region EstDate

		public override ZDateTime EstDate
		{
			get { return endPoint.EstimatedDate; }
		}

		#endregion

		#region Direction

		public override ZString Direction
		{
			get { return endPoint.Direction; }
		}

		#endregion

		#endregion

		#region Related Business Objects

		public EDIMessageCollection Messages
		{
			get
			{
				if (fMessages == null)
				{
					fMessages = new PortMessageCollection((BusinessObject)endPoint, endPoint.Factory);
					fMessages.Load();
					fMessages.IsManagedForDataRefresh = true;
				}
				return fMessages;
			}
		}
		EDIMessageCollection fMessages;

		#endregion

		readonly ISailingEndPoint endPoint;
	}
}



