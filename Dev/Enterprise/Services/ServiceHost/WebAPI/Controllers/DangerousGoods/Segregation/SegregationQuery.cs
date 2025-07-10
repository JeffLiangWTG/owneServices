using System;

namespace Enterprise.Services.ServiceHost.WebAPI.Controllers.DangerousGoods.Segregation
{
	public class SegregationQuery
	{
		public string[] Standards { get; set; }
		public Guid[] Ids { get; set; }
	}
}
