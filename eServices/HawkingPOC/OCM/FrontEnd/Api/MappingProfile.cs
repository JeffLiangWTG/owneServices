using AutoMapper.Configuration;
using OcmPoc.FrontEnd.Core.Dtos;
using OcmPoc.Infrastructure.MessageInterfaces.Entities;
using OcmPoc.Infrastructure.MessageInterfaces.Queueing;

namespace OcmPoc.FrontEnd.Api
{
	public class MappingProfile : MapperConfigurationExpression
	{
		public MappingProfile()
		{
			CreateMap<MessageFlow, MessageFlowDto>();
			CreateMap<MessageEvent, MessageEventDto>();

			CreateMap<MessageFlow, QueueItem>()
				.ForMember(qi => qi.MessageFlowId, opts => opts.MapFrom(mc => mc.Id))
				.ForMember(qi => qi.MessageId, opts => opts.MapFrom(mc => mc.InitialMessageId));

			CreateMap<QueueItem, ReceivedMessageDto>();
		}
	}
}
