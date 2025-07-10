using CargoWise.Types;
using Enterprise.Customs.Business.MessageBuilders.eManifest;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.eManifest.Messaging
{
	internal class TravelDocumentWrapper : ITravelDocument
	{
		public TravelDocumentWrapper(GenRegCertAccredMaintList doc)
		{
			this.doc = doc;
		}

		#region Implementation of ITravelDocument

		public ZString TravelDocumentType
		{
			get { return doc.XZ_Type; }
		}

		public ZString TravelDocumentNumber
		{
			get { return doc.XZ_RefNumber; }
		}

		public ZDate ExpiryDate
		{
			get { return doc.XZ_ExpiryOrDueDate.Date; }
		}

		public ZString CountryOfIssuance
		{
			get { return doc.XZ_RN_NKCountryOfIssuance; }
		}

		public ZString StateOrProvinceOfIssuance
		{
			get { return doc.XZ_StateOrProvinceOfIssuance; }
		}

		#endregion

		readonly GenRegCertAccredMaintList doc;
	}
}
