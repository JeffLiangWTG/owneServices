using System;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.PL.NCTS.Business.Message.MessageProviders.Common;

public class EnquiryProvider : IEnquiry
{
	public EnquiryProvider(MessageSendingObject messageSendingObject)
	{
		this.messageSendingObject = Argument.NotNull(messageSendingObject, nameof(messageSendingObject));
	}

	readonly MessageSendingObject messageSendingObject;

	public DateTime? TC11DeliveryDate => CachedValueHelper.GetValue(ref tc11DeliveryDate, () => !messageSendingObject.TC11DeliveryDate.IsEmpty ?
		DateTime.ParseExact(messageSendingObject.TC11DeliveryDate.ToBestReadableDateTimeString(), (NoResString)"dd MMM yyyy HH:mm", null) : null);
	CachedValue<DateTime?> tc11DeliveryDate;

	public string Text => messageSendingObject.AdditionalText;
}
