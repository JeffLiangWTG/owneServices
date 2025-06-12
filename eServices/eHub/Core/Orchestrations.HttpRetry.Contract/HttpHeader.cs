using System;
using System.Text;

namespace CargoWise.eHub.Core.Orchestrations.HttpRetry.Contract
{
	[Serializable]
	public class HttpHeader
	{
		public static HttpHeader Create(string key, string value)
		{
			return new HttpHeader
			{
				Key = key,
				Value = value
			};
		}

		public string Key { get; set; }

		public string Value { get; set; }

		public override string ToString()
		{
			var stringBuilder = new StringBuilder()
				.Append("[HttpHeader] ")
				.AppendFormat("Key=[{0}] ", Key)
				.AppendFormat("Value=[{0}]", Value);

			return stringBuilder.ToString();
		}
	}
}
