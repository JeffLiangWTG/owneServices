using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NL.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.NL.GUI;

public class OrganisationConsigneePlugIn : ZPlugIn
{
	public OrganisationConsigneePlugIn(OrgHeader organisation)
		: base(organisation)
	{
	}

	public override string Name => Res.GetString("d982f06e-3cf2-482e-8bc3-d131d1bd443a", "NL Deferral");

	protected override ZBool HasUserControl => true;

	NLOrgImpAddInfo AddInfo => addInfo ?? (addInfo = NLOrgImpAddInfo.Get((OrgHeader)HostBusinessEntity));
	NLOrgImpAddInfo addInfo;

	protected override ZArchitecture.GUI.ZTabPagePlugIn GetTabPage()
	{
		var result = base.GetTabPage();
		result.Added += delegate
		{
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Netherlands)
			{
				var parent = (TabControl)result.Parent;
				parent.SelectedTab = result;
			}
		};
		return result;
	}

	protected override Control GetNewUserControl() => new OrganisationConsigneePlugInUserControl();

	protected override IBusiness GetBusinessEntityForPlugIn() => AddInfo;

	protected override LicenceCheckpoint LicenceCheckPoint => Env.Licence.Core;
}
