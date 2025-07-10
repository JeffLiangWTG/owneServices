//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgAddressAdditionalInfoValidation
//
//    This class should be used for overriding validation in AutoOrgAddressAdditionalInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgAddressAdditionalInfoValidation : AutoOrgAddressAdditionalInfoValidation
	{
		public OrgAddressAdditionalInfoValidation(AutoOrgAddressAdditionalInfo parent) : base(parent)
		{
		}

		protected override void CheckOAI_IsPrimary()
		{
			base.CheckOAI_IsPrimary();

			if (Parent.Address?.AdditionalInfos.Count(info => info.OAI_IsPrimary) != 1)
			{
				Parent.OAI_IsPrimaryInfo.AddError(Res.GetString("2E5F3B0E-F654-4078-8E5E-D1287A773A60", "Must specified one main address additional information."));
			}
			else
			{
				foreach (OrgAddressAdditionalInfo info in Parent.Address?.AdditionalInfos.Where(info => info.OAI_IsPrimaryInfo.HasErrors()))
				{
					info.Validation.ValidateOAI_IsPrimary();
				}
			}
		}

		protected override void CheckOAI_AdditionalInfo()
		{
			base.CheckOAI_AdditionalInfo();

			MandatoryValidation.CheckEntered(Parent.OAI_AdditionalInfoInfo);

			if (Parent.Address?.AdditionalInfos.Any(info => info.OAI_AdditionalInfo.EqualsIgnoringCase(Parent.OAI_AdditionalInfo) && info.PK != Parent.PK) ?? false)
			{
				Parent.OAI_AdditionalInfoInfo.AddError(Res.GetString("308A7E61-86ED-42B9-AC90-CBB4D87650D1", "Must specified a unique address additional information."));
			}
		}
	}
}
