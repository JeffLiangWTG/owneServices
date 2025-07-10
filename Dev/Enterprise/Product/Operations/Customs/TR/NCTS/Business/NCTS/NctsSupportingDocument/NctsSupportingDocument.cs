using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class NctsSupportingDocument : EU.NCTS.Business.NctsSupportingDocument
	{
		public NctsSupportingDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override CusSupportingInfoLookups GetNewPhase4Lookups() => new NctsSupportingDocumentLookups(this);

		protected override CusSupportingInfoLookups GetNewPhase5Lookups() => new NctsSupportingDocumentLookups(this);

		public new NctsSupportingDocumentLookups Lookups => (NctsSupportingDocumentLookups)base.Lookups;
	}
}
