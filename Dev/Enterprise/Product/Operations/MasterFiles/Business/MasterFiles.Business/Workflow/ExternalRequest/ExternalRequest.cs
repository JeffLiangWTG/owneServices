using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using  Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
namespace Enterprise.MasterFiles.Business
{
	public class ExternalRequest : AutoExternalRequest, INumberFountainConsumer
	{
		public ExternalRequest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region INumberFountainConsumer

		ZString INumberFountainEntityWithID.ID
		{
			get => REQ_RequestID;
			set => REQ_RequestID = value;
		}

		INumberFountainProxy INumberFountainConsumer.Fountain => Environment.Env.NumberFountains.ExternalRequestID;

		#endregion
	}
}
