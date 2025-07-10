using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.TW.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using static Enterprise.Customs.TW.DataTransfer.Constants;
using EZC = Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.DataTransfer.Universal
{
	public class TWInvoiceLineAdditionalAddInfoGroupCollectionDataObjectWriter : IAdditionalAddInfoGroupCollectionDataObjectWriter
	{
		public TWInvoiceLineAdditionalAddInfoGroupCollectionDataObjectWriter(JobComInvoiceLine invoiceLine, TWDataObjectWriterHelper helper)
		{
			this.invoiceLine = invoiceLine;
			this.helper = helper;
		}

		readonly JobComInvoiceLine invoiceLine;
		readonly TWDataObjectWriterHelper helper;

		public IEnumerable<AddInfoGroup> CreateCollection()
		{
			var addInfos = new List<AddInfo>();
			foreach (var pk in invoiceLine.GetLinkControllingAgencyPKs())
			{
				var link = helper.GetAllocatedControllingMessageHeaderLink(pk);
				if (!link.IsEmpty)
				{
					addInfos.Add(new AddInfo { Key = Constants.AddInfoKeys.ControllingMessage.ControllingMessageLink, Value = link });
				}
			}
			if (addInfos.Any())
			{
				yield return new AddInfoGroup()
				{
					Type = controllingMessageLinksType,
					AddInfoCollection = addInfos
				};
			}
		}

		readonly CodeDescriptionPair controllingMessageLinksType = new CodeDescriptionPair() { Code = AddInfoGroupTypeCodes.ControllingMessageLinksType, Description = (EZC.NoResString)"Controlling Message Links" };
	}
}
