using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class NctsAdditionalInfo : EU.NCTS.Business.NctsAdditionalInfo
	{
		public NctsAdditionalInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new INctsAdditionalInfoLookups Lookups => (INctsAdditionalInfoLookups)base.Lookups;

		protected override Customs.Business.CusSupportingInfoLookups GetNewPhase4Lookups() => new NctsAdditionalInfoPhase4Lookups(this);

		protected override Customs.Business.CusSupportingInfoLookups GetNewPhase5Lookups() => new NctsAdditionalInfoPhase5Lookups(this);

		[List(nameof(Lookups) + "." + nameof(INctsAdditionalInfoLookups.AdditionalInfoCodesList))]
		public override ZString CSI_Code
		{
			get => base.CSI_Code;
			set => base.CSI_Code = value;
		}
	}
}
