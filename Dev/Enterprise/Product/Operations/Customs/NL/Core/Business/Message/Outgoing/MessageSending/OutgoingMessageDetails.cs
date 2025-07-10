using System;

namespace Enterprise.Customs.NL.Business;

public class OutgoingMessageDetails
{
	public OutgoingMessageDetails(Type messageBuilderType, Type wrapperType)
	{
		MessageBuilderType = messageBuilderType;
		WrapperType = wrapperType;
	}

	public Type MessageBuilderType { get; }

	public Type WrapperType { get; }
}
