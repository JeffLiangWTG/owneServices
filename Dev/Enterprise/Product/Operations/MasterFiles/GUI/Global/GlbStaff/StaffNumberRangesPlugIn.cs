using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.PlugIn;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWise.ResourceStrings;
using CargoWise.Windows.UI;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.MasterFiles.GUI
{
	public class StaffNumberRangesPlugIn : ZPlugIn
	{
		public StaffNumberRangesPlugIn(GlbStaff staff)
			: base(staff)
		{
			Enabled = GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Mexico;
		}

		public override string Name => Res.GetString("E07B037C-668F-467F-823E-0EACD388FB90", "Number Ranges");

		protected override ZBool HasUserControl => true;

		protected override IBusiness GetBusinessEntityForPlugIn() => HostBusinessEntity;

		protected override Control GetNewUserControl() => new NumberRangesUserControl();

		protected override LicenceCheckpoint LicenceCheckPoint => Env.Licence.Core;
	}
}
