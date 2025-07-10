using Enterprise.DataTransfer.Business;

namespace Enterprise.Customs.DataTransfer
{
	public class DeclarationXmlDataTransferDirector : XmlDataTransferDirector
	{
		public DeclarationXmlDataTransferDirector(DeclarationValueObjectDataAdapter adapter, bool checkLicence)
			: base(adapter, checkLicence)
		{
		}

		public new DeclarationValueObjectDataAdapter Adapter
		{
			get { return (DeclarationValueObjectDataAdapter)base.Adapter; }
		}

		#region Import

		protected override XmlDataImporter NewXmlDataImporter()
		{
			return new DeclarationXmlDataImporter(Adapter);
		}

		#endregion
	}
}
