using System.Collections.Generic;

namespace OcmPoc.Infrastructure.MessageQueue.Rabbit
{
	public class HeaderBindingSpec : BindingSpec
	{
		public HeaderBindingSpec(string exchangeName)
			: base(exchangeName)
		{
			BindArguments = new Dictionary<string, object>();
			MatchAll();
		}

		public override IDictionary<string, object> BindArguments { get; }

		public HeaderBindingSpec Add(string header)
		{
			BindArguments[header] = "";
			return this;
		}

		public HeaderBindingSpec Add(string header, object value)
		{
			BindArguments[header] = value;
			return this;
		}

		public HeaderBindingSpec MatchAll()
		{
			BindArguments["x-match"] = "all";
			return this;
		}

		public HeaderBindingSpec MathcAny()
		{
			BindArguments["x-match"] = "any";
			return this;
		}
	}
}
