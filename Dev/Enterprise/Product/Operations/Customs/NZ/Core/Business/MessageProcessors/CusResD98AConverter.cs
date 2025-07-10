using Enterprise.Edifact;
using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D98A.Messages.CUSRES;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NZ.Business.MessageProcessors
{
	/// <summary>
	/// EDIMessage is converted into D98A
	/// </summary>
	public class CusResD98AConverter
	{
		public CusResD98AConverter(EDIMessage eDIMessage)
		{
			this.eDIMessage = eDIMessage;
		}

		public CUSRESMessage CusResD98A
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
						CUSRESMessage cUSRESD98A = new CUSRESMessage();
						cUSRESD98A.Parse(new UNOACharacterSet(), segmentGroup.ToString(new UNOACharacterSet()));
						result = cUSRESD98A;
					}
				}
				return result;
			}
		}

		protected EDIMessage eDIMessage;
	}
}
