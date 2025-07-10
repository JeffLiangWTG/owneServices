using System.Xml.Serialization;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.DIS.Business
{
	public class DISCertificateData : AutoDISCertificateData
	{
		public DISCertificateData(BusinessObjectFactory factory)
			: this(factory, null)
		{
		}

		public DISCertificateData(BusinessObjectFactory factory, DISDocument document) : base(factory)
		{
			Document = document;
		}

		[XmlIgnore]
		internal DISDocument Document { get; }
	}
}
