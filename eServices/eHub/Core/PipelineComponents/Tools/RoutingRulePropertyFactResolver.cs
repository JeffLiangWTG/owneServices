using System;
using System.Linq;
using eServices.eHubRoutingRuleEngine;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Core.PipelineComponents.Tools
{
	public class RoutingRulePropertyFactResolver : IFactResolver
	{
		readonly IBaseMessage message;

		public RoutingRulePropertyFactResolver(IBaseMessage message)
		{
			if (message == null) throw new ArgumentNullException("message");

			this.message = message;
		}

		public void Resolve(Fact[] facts)
		{
			foreach (var fact in facts.Where(f => f.Type == "PROPERTY"))
			{
				var propertyParts = fact.Query.Split('#');
				object value = this.message.Context.Read(propertyParts[1], propertyParts[0]);
				fact.Value = value == null ? String.Empty : value.ToString();
			}
		}
	}
}
