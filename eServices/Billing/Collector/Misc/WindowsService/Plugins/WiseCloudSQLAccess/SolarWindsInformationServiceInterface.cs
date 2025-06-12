public partial class InformationServiceClient : System.ServiceModel.ClientBase<InformationService>, IInformationServiceClient
{
}

public interface IInformationServiceClient
{
	System.ServiceModel.Description.ClientCredentials ClientCredentials { get; }
	System.Xml.XmlElement QueryXml(string query, schemas.solarwinds.com._2007._08.informationservice.propertybag.dictionary parameters);
	void Open();
	void Close();
}
