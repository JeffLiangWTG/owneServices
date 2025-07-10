namespace Enterprise.Freight.Agency.Business
{
	public interface ICMMOrganisationData
	{
		string Code { get; }
		CMMOrganisationType CodeType { get; }
	}
}
