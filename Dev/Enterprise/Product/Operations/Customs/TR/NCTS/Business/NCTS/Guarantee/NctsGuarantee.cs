using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class NctsGuarantee : EU.NCTS.Business.NctsGuarantee
	{
		public NctsGuarantee(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Lookups

		public new NctsGuaranteeLookups Lookups => (NctsGuaranteeLookups)base.Lookups;

		protected override MasterFiles.Business.CusBondDetailLookups GetNewLookups() => new NctsGuaranteeLookups(this);

		#endregion
	}
}
