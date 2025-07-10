namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class FZEventActionLookups
	{
		public FZEventActionLookups(FZEventAction action)
		{
			this.action = action;
		}

		readonly FZEventAction action;

		public FTZActionCodeList ActionCodeList
		{
			get
			{
				return action.Factory.GetCachedValue("ActionCodeList" + action.EventType,
					delegate
					{
						var result = new FTZActionCodeList();

						if (action.EventType == FZEventType.Concur)
						{
							result.RemoveCode(FTZActionCodeList.Codes.D);
							result.RemoveCode(FTZActionCodeList.Codes.F);
							result.RemoveCode(FTZActionCodeList.Codes.G);
							result.RemoveCode(FTZActionCodeList.Codes.H);
							result.RemoveCode(FTZActionCodeList.Codes.I);
							result.RemoveCode(FTZActionCodeList.Codes.J);
							result.RemoveCode(FTZActionCodeList.Codes.K);
							result.RemoveCode(FTZActionCodeList.Codes.L);
							result.RemoveCode(FTZActionCodeList.Codes.M);
						}
						else if (action.EventType == FZEventType.Delivery)
						{
							result.RemoveCode(FTZActionCodeList.Codes.A);
							result.RemoveCode(FTZActionCodeList.Codes.B);
							result.RemoveCode(FTZActionCodeList.Codes.C);
							result.RemoveCode(FTZActionCodeList.Codes.D);
							result.RemoveCode(FTZActionCodeList.Codes.F);
							result.RemoveCode(FTZActionCodeList.Codes.I);
							result.RemoveCode(FTZActionCodeList.Codes.J);
							result.RemoveCode(FTZActionCodeList.Codes.K);
							result.RemoveCode(FTZActionCodeList.Codes.L);
							result.RemoveCode(FTZActionCodeList.Codes.M);
						}
						return result;
					});
			}
		}

		public FTZUnconcurrenceReasonCodeList ReasonCodeList => action.Factory.GetCachedValue<FTZUnconcurrenceReasonCodeList>();
	}
}
