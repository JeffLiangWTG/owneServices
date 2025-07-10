
using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class JobDocAddressCCPValidation : JobDocAddressValidation
	{
		public JobDocAddressCCPValidation(JobDocAddress docAddress)
			: base(docAddress)
		{
			this.docAddress = docAddress;
		}
		readonly JobDocAddress docAddress;

		protected override void CheckOrganisationPK()
		{
			base.CheckOrganisationPK();

			if (docAddress != null)
			{
				if (docAddress.Address == null)
				{
					if (docAddress.Organisation != null &&
						!docAddress.Organisation.CustomsCodes.Cast<OrgCusCode>()
							.Any(x => x.OK_CodeType == "CCP" && x.OK_OA_PremisesAddress.IsEmpty && !x.OK_CustomsRegNo.IsEmpty))
					{
						Parent.OrganisationPKInfo.AddError(Res.GetString("87E9F27F-BA16-417B-B90E-663774463845", "There must be a premise code with a blank address"));
					}
				}
				else if (docAddress.Address.LocalControlledPremisesID.IsEmpty)
				{
					Parent.OrganisationPKInfo.AddError(Res.GetString("AB7A9662-8849-461B-8EC8-9513C83A313B", "This selected Pack Depot Address does not have a linked Customs Controlled Premise code"));
				}
			}
		}
	}
}
