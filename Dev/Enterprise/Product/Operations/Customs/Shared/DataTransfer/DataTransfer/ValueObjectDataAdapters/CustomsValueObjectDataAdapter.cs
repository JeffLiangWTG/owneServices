using CargoWise.EntityFramework;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.DataTransfer
{
	public abstract class CustomsBusinessObjectValueObjectDataAdapter<TBusinessObject, TValueObject> : ValueObjectDataAdapter<TBusinessObject, TValueObject>
		where TBusinessObject : BusinessObject
		where TValueObject : IValueObject
	{
		public CustomsBusinessObjectValueObjectDataAdapter()
		{
			AddInfoDataTransferTool = new AddInfoDataTransferTool(AddInfoPrefix, ShouldFieldBeExported);
		}

		protected readonly AddInfoDataTransferTool AddInfoDataTransferTool;

		protected virtual string AddInfoPrefix
		{
			get { return ""; }
		}

		protected virtual void ImportAddInfos(ZPropertyInfoHashtable zPropertyInfoHash, Xsd.AdditionalCustomsInformationCollection addCustomsDetails, IValueObjectImportContext context)
		{
			AddInfoDataTransferTool.ImportAddInfos(zPropertyInfoHash, addCustomsDetails, context);
		}

		protected void ExportAddInfos(Xsd.AdditionalCustomsInformationCollection xmlAddInfo, ZPropertyInfoHashtable zPropertyInfoHash)
		{
			AddInfoDataTransferTool.ExportAddInfos(xmlAddInfo, zPropertyInfoHash);
		}

		protected virtual bool ShouldFieldBeExported(ZPropertyInfo info)
		{
			return true;
		}
	}
}
