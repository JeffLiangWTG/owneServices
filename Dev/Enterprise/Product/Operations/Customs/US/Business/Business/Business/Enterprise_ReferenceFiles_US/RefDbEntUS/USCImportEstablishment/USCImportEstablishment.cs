using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.RefDbEntUS
{
	public class USCImportEstablishment : AutoUSCImportEstablishment
	{
		public USCImportEstablishment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override void Delete()
		{
			Factory.Load<USCImportEstablishmentAlternateName>(new ZQuery(USCImportEstablishmentAlternateNameSchema.IA_IE, PK)).DeleteAll();
			base.Delete();
		}
	}
}
