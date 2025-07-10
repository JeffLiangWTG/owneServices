using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules.DocumentScanning;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	internal class NeutralMastersEDocsViaUniversalXmlSupport : IEDocsViaUniversalXmlSupport
	{
		public BusinessObject LoadBusinessObjectFromCode(BusinessObjectFactory factory, ZString code)
		{
			Argument.NotNull(factory, nameof(factory));

			var query = new ZQuery(JobMawbSchema.JM_Airline3DigitPrefix, code.Substring(0, code.Length >= 3 ? 3 : code.Length))
				.AddToFilter(JobMawbSchema.JM_MAWB, code.Substring(code.Length > 3 ? 3 : 0));
			return factory.LoadTop1<JobMawb>(query);
		}

		public ZString ExpectedCodeFormat { get; } = "JM_Airline3DigitPrefix+JM_MAWB";
		public ZString ExampleCodeFormat { get; } = "08199999999";
	}
}
