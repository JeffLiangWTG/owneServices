using System.Collections.ObjectModel;
using CargoWise.Customs.US.MessageContracts.Interfaces;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	[CodeAlive("Will be used in future WI")]
	public class CrewInfoTypeProvider : ICrewInfoType
	{
		public IManifestStringType CrewTypeCode => throw new System.NotImplementedException();

		public IManifestStringType FirstName => throw new System.NotImplementedException();

		public IManifestStringType LastName => throw new System.NotImplementedException();

		public IManifestStringType MiddleInitial => throw new System.NotImplementedException();

		public IManifestStringType DateOfBirth => throw new System.NotImplementedException();

		public IManifestStringType Gender => throw new System.NotImplementedException();

		public IManifestStringType CitizenshipCountryCode => throw new System.NotImplementedException();

		public IManifestStringType AddressLine1 => throw new System.NotImplementedException();

		public IManifestStringType AddressLine2 => throw new System.NotImplementedException();

		public IManifestStringType AddressLine3 => throw new System.NotImplementedException();

		public IManifestStringType CityName => throw new System.NotImplementedException();

		public IManifestStringType StateCode => throw new System.NotImplementedException();

		public IManifestStringType PostalCode => throw new System.NotImplementedException();

		public IManifestStringType PhoneNumber => throw new System.NotImplementedException();

		public Collection<IDocumentInfoType> DocumentInfoList => throw new System.NotImplementedException();

		public Collection<IErrorType> ResponseMessage => throw new System.NotImplementedException();
	}
}
