using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.TW.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.TW.DataTransfer.Universal
{
	public class TWJobDeclarationAdditionalAddInfoGroupCollectionDataObjectWriter : IAdditionalAddInfoGroupCollectionDataObjectWriter
	{
		public TWJobDeclarationAdditionalAddInfoGroupCollectionDataObjectWriter(BusinessObject declaration)
		{
			jobDeclaration = declaration as JobDeclaration;
		}

		readonly JobDeclaration jobDeclaration;

		public IEnumerable<AddInfoGroup> CreateCollection()
		{
			foreach (var gui in jobDeclaration.GovernmentUniformInvoices.Cast<GovernmentUniformInvoiceData>())
			{
				yield return new AddInfoGroup()
				{
					Type = new CodeDescriptionPair()
					{
						Code = CusCodeDataTypeList.Codes.GOVUniformInvoice,
						Description = CusCodeDataTypeList.Descriptions.GOVUniformInvoice
					},
					AddInfoCollection = new List<AddInfo>()
					{
						new AddInfo()
						{
							Key = Constants.AddInfoKeys.GOVUniformInvoice.GovernmentUniformInvoiceNumber,
							Value = gui.CY_Code
						},
						new AddInfo()
						{
							Key = Constants.AddInfoKeys.GOVUniformInvoice.GovernmentUniformInvoiceAmount,
							Value = gui.Amount.ToString()
						}
					}
				};
			}
		}
	}
}
