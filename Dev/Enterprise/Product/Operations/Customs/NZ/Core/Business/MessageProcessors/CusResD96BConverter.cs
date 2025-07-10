using Enterprise.Edifact;
using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D96B.Messages.CUSRES;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NZ.Business.MessageProcessors
{
	/// <summary>
	/// EDIMessage is converted into D96B
	/// </summary>
	public class CusResD96BConverter
	{
		public CusResD96BConverter(EDIMessage eDIMessage)
		{
			this.eDIMessage = eDIMessage;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
		public CUSRESMessage CusResD96B
		{
			get
			{
				CUSRESMessage result = null;
				SegmentGroup segmentGroup = eDIMessage.GetAutoEdifactMessageUsingNamedFactory(NzEdifactMessageFactory.NZCMessageFactory, new UNOACharacterSet());
				if (segmentGroup != null)
				{
					if (segmentGroup is CUSRESMessage)
					{
						result = (CUSRESMessage)segmentGroup;
					}
					else
					{
						CUSRESMessage cUSRESD96B = new CUSRESMessage();
						cUSRESD96B.Parse(new UNOACharacterSet(), segmentGroup.ToString(new UNOACharacterSet()));
						result = cUSRESD96B;
					}
				}
				return result;
			}
		}

		protected EDIMessage eDIMessage;
	}
}
