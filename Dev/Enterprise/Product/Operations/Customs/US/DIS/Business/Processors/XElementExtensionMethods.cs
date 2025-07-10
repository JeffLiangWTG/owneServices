using System;
using System.Linq;
using System.Xml.Linq;

namespace Enterprise.Customs.US.DIS.Business
{
	static class XElementExtensionMethods
	{
		public static bool Matches(this XElement element, string localName)
		{
			return element.Name.LocalName.Equals(localName, StringComparison.InvariantCultureIgnoreCase);
		}

		public static bool HasFailed(this XElement processingResultElement)
		{
			bool result = true;

			var xStatus = processingResultElement.Elements().FirstOrDefault(x => x.Matches(EDIMessage.Constants.ProcessingStatus));
			if (xStatus != null)
			{
				result = xStatus.Value.Equals(FailedStatus, StringComparison.InvariantCultureIgnoreCase);
			}

			return result;
		}

		const string FailedStatus = "Failed";
	}
}
