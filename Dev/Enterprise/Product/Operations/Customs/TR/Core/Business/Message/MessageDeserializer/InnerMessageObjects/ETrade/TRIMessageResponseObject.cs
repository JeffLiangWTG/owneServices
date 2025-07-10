using System.Collections.Generic;
using System.Xml;
using CargoWise.Types;

namespace Enterprise.Customs.TR.Business
{
	public class TRIMessageResponseObject : InnerMessageObjectBase
	{
		public TRIMessageResponseObject(XmlDocument xmlDocument) : base(xmlDocument)
		{
			var errorNodes = xmlDocument.GetElementByPath(new (string, string)[] { (ErrorsText, string.Empty) });

			if (errorNodes != null)
			{
				var errorsList = new List<Error>();
				foreach (XmlNode node in errorNodes)
				{
					errorsList.Add(new Error()
					{
						Description = TryGetInnerText(node, ErrorDescText)
					});
				}
				Errors = errorsList;
			}
		}

		public List<Error> Errors { get; }

		public class Error
		{
			public ZString Code { get; set; }
			public ZString Description { get; set; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Localised string")]
		const string ErrorsText = "Hatalar";
		const string ErrorDescText = "HataAciklamasi";
	}
}
