using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules.DocumentScanning;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	internal class JobVoyageEDocsViaUniversalXmlSupport : IEDocsViaUniversalXmlSupport
	{
		public JobVoyageEDocsViaUniversalXmlSupport(ZString transportMode)
		{
			this.transportMode = transportMode;
		}

		readonly ZString transportMode;

		public BusinessObject LoadBusinessObjectFromCode(BusinessObjectFactory factory, ZString code)
		{
			Argument.NotNull(factory, nameof(factory));

			var query = new ZQuery(JobVoyageSchema.JV_SendersMessageReference, code)
				.AddToFilter(JobVoyageSchema.JV_AirSeaRoad, transportMode);
			return factory.LoadTop1<JobVoyage>(query);
		}

		public ZString ExpectedCodeFormat { get; } = "JVSendersMessageReference";
		public ZString ExampleCodeFormat { get; } = "J99999999";
	}
}
