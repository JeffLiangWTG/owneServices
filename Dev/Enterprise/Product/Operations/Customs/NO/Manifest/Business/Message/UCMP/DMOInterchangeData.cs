using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.NO.Manifest.Business;

sealed class DMOInterchangeData
{
	public string InterchangeType { get; set; }

	public ZString MessageText { get; set; }

	public IDictionary<string, string> MessageAttributes { get; } = new Dictionary<string, string>();

	public ZStringBuilder ErrorBuilder { get; } = new ();
}
