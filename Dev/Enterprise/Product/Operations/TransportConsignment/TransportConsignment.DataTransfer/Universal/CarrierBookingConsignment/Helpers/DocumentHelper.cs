using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportConsignment.Business;

namespace Enterprise.TransportConsignment.DataTransfer.Universal.Helpers
{
	public static class DocumentHelper
	{
		public static void AddDocumentFromUniversalResponse(string responseBody, DtbConsignment carrierBooking, string labelName = "")
		{
			var (shipment, _) = XmlHelper.DeserializeUniversalShipment(responseBody);
			var docManagerInfo = carrierBooking.DocManagerInfoCore();
			var completed = new Dictionary<string, int>();
			foreach (var document in shipment.AttachedDocumentCollection)
			{
				if (document.ImageData.Length == 0 || !document.Type.Code.HasValue)
				{
					continue;
				}
				var code = document.Type.Code.GetValueOrDefault();
				completed[code] = completed.TryGetValue(code, out var val) ? val + 1 : 0;
				using (document.ImageData)
				{
					docManagerInfo.AddFileOrDocument(document.ImageData, labelName.Length != 0 ? labelName : GenerateLabelName(code, completed[code] > 0 ? completed[code].ToString() : ""), code);
				}
			}
			docManagerInfo.Save();
		}

		public static string GenerateLabelName(string type, string suffix)
		{
			return $"{type}_{ZDateTime.Now:yyyyMMdd_HHmmss}{(string.IsNullOrEmpty(suffix) ? "" : '_' + suffix)}.pdf";
		}
	}
}
