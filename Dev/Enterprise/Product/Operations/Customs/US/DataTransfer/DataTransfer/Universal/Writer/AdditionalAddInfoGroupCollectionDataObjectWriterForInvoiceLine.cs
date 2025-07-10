using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	class AdditionalAddInfoGroupCollectionDataObjectWriterForInvoiceLine : IAdditionalAddInfoGroupCollectionDataObjectWriter
	{
		public AdditionalAddInfoGroupCollectionDataObjectWriterForInvoiceLine(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = Argument.NotNull(invoiceLine, "invoiceLine");
		}
		readonly JobComInvoiceLine invoiceLine;

		#region IAdditionalAddInfoGroupCollectionDataObjectWriter Members

		public IEnumerable<UniversalCustoms.AddInfoGroup> CreateCollection()
		{
			if (invoiceLine.JI_BondedWhsQuantity.IsEmpty)
			{
				foreach (var packLine in invoiceLine.WHSPackLines)
				{
					var pack = packLine.WHSPack;
					if (pack != null)
					{
						yield return new UniversalCustoms.AddInfoGroup()
						{
							Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.USWHSPackLine, Description = CusAddInfoTypeListProvider.AdditionalAddInfoType.InvoiceLinePackData.CodeDescription },
							AddInfoCollection = new List<UniversalDataBuss.DataObjects.Universal.AddInfo>(new[]
							{
								CreateAddInfo(CusAddInfoTypeListProvider.AdditionalAddInfoType.InvoiceLinePackData.PackageID, Enterprise.Customs.US.Business.AddInfo.GetStringRepresentation(pack.US_PackageReference)),
								CreateAddInfo(CusAddInfoTypeListProvider.AdditionalAddInfoType.InvoiceLinePackData.PackedQty, Enterprise.Customs.US.Business.AddInfo.GetStringRepresentation(packLine.US_PackedQty))
							})
						};
					}
				}
			}
			else
			{
				yield return new UniversalCustoms.AddInfoGroup()
				{
					Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.USWHSPackLine, Description = CusAddInfoTypeListProvider.AdditionalAddInfoType.InvoiceLinePackData.CodeDescription },
					AddInfoCollection = new List<UniversalDataBuss.DataObjects.Universal.AddInfo>(new[]
							{
								CreateAddInfo(CusAddInfoTypeListProvider.AdditionalAddInfoType.InvoiceLinePackData.IsSimplePackagingStyle, YesNoDefaultList.Codes.Yes),
								CreateAddInfo(CusAddInfoTypeListProvider.AdditionalAddInfoType.InvoiceLinePackData.PackageQty,  Enterprise.Customs.US.Business.AddInfo.GetStringRepresentation(invoiceLine.JI_BondedWhsQuantity)),
								CreateAddInfo(CusAddInfoTypeListProvider.AdditionalAddInfoType.InvoiceLinePackData.PackedQty, Enterprise.Customs.US.Business.AddInfo.GetStringRepresentation(invoiceLine.JI_InvoiceQuantity))
							})
				};
			}
		}

		UniversalDataBuss.DataObjects.Universal.AddInfo CreateAddInfo(ZString key, ZString value)
		{
			return new UniversalDataBuss.DataObjects.Universal.AddInfo() { Key = key, Value = value };
		}

		#endregion
	}
}
