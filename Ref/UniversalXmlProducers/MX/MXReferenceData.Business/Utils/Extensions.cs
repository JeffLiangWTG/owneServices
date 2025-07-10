using System.Xml.Linq;

namespace CargoWise.RefDbRepo.MXReferenceData.Business
{
	public static class Extensions
	{
		public static string GetElementValueAsString(this XElement element, string elementName, int maxLength)
		{
			var value = element?.Element(elementName)?.Value;
			if (maxLength == 0)
			{
				return value;
			}
			else if (value != null && value.Length > maxLength)
			{
				value = value.Substring(0, maxLength);
			}

			return value;
		}
	}
}
