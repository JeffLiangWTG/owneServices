using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.RefDbEntUS
{
	public class USCImportEstablishmentAlternateName : AutoUSCImportEstablishmentAlternateName
	{
		public USCImportEstablishmentAlternateName(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[RelatedBusinessObject("Header")]
		public override ZGuid IA_IE
		{
			get { return base.IA_IE; }
			set { base.IA_IE = value; }
		}

		public USCImportEstablishment Header
		{
			get { return Factory.Load<USCImportEstablishment>(IA_IE); }
		}
	}
}
