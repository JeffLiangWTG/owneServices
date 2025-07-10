using System.Collections.ObjectModel;
using CargoWise.Customs.US.MessageContracts.Interfaces;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	[CodeAlive("Will be used in future WI")]
	public class EventInfoTypeProvider : IEventInfoType
	{
		public IManifestStringType EventType => throw new System.NotImplementedException();

		public IManifestStringType EventPortCode => throw new System.NotImplementedException();

		public IManifestStringType EventDateTime => throw new System.NotImplementedException();

		public IManifestStringType EventReferenceData => throw new System.NotImplementedException();

		public Collection<IErrorType> ResponseMessage => throw new System.NotImplementedException();
	}
}
