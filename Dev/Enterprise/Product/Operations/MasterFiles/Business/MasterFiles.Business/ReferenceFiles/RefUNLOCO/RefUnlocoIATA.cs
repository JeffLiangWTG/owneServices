using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(RefUNLOCOSchema.Constants.RL_IATA), DescriptionProperty(RefUNLOCOSchema.Constants.RL_NameWithDiacriticals)]
	public class RefUnlocoIATA : RefUNLOCO
	{
		public RefUnlocoIATA(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }
	}
}
