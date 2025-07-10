using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ProcessManagement.Business
{
	public class WorkRequestValidation : AutoWorkRequestValidation
	{
		public WorkRequestValidation(AutoWorkRequest parent)
			: base(parent)
		{
		}

		protected override void CheckWKR_Summary()
		{
			base.CheckWKR_Summary();

			MandatoryValidation.CheckEntered(Parent.WKR_SummaryInfo);
		}

		protected override void CheckWKR_OC_Client()
		{
			base.CheckWKR_OC_Client();

			var client = Parent.Client;

			if (client != null)
			{
				if (!EmailAddressValidation.IsEmailAddressValidAndNotEmpty(Parent.Client.OC_Email))
				{
					Parent.WKR_OC_ClientInfo.AddError(InvalidEmailOnClientErrorMessage);
				}

				var workRequest = Parent as WorkRequest;
				if (!workRequest.OrganisationPK.IsEmpty && client.ParentOrg.PK != workRequest.OrganisationPK)
				{
					var workRequestOrg = Parent.Factory.Load<OrgHeader>(workRequest.OrganisationPK);
					var selectedOrganisation = workRequestOrg?.OH_Code ?? ZString.Empty;

					if (selectedOrganisation.IsEmpty)
					{
						Parent.WKR_OC_ClientInfo.AddError(Res.GetString("18C7D4C3-B819-489E-888C-11494CDB21AB", "Cannot select Client from invalid Organization."));
					}
					else
					{
						var clientOrg = Parent.Factory.Load<OrgHeader>(client.ParentOrg.PK);
						Parent.WKR_OC_ClientInfo.AddError(
							Res.GetString(
								"A7586B64-2099-44B7-8D34-FB55E02B6FCA",
								"The client's organization '{0}' is different from the selected organization '{1}'.",
								clientOrg?.OH_Code ?? ZString.Empty,
								selectedOrganisation
							)
						);
					}
				}
			}
		}

		protected override void CheckWKR_SelectionCriteria1()
		{
			base.CheckWKR_SelectionCriteria1();

			ListValidation.ErrorIfInvalidCode(Parent.WKR_SelectionCriteria1Info);
		}

		protected override void CheckWKR_SelectionCriteria2()
		{
			base.CheckWKR_SelectionCriteria2();

			ListValidation.ErrorIfInvalidCode(Parent.WKR_SelectionCriteria2Info);
		}

		protected override void CheckWKR_SelectionCriteria3()
		{
			base.CheckWKR_SelectionCriteria3();

			ListValidation.ErrorIfInvalidCode(Parent.WKR_SelectionCriteria3Info);
		}

		protected override void CheckWKR_SelectionCriteria4()
		{
			base.CheckWKR_SelectionCriteria4();

			ListValidation.ErrorIfInvalidCode(Parent.WKR_SelectionCriteria4Info);
		}

		protected override void CheckWKR_SelectionCriteria5()
		{
			base.CheckWKR_SelectionCriteria5();

			ListValidation.ErrorIfInvalidCode(Parent.WKR_SelectionCriteria5Info);
		}

		public void ValidateOrganisationPK()
		{
			var workRequest = Parent as WorkRequest;
			ValidateCalculatedProperty(workRequest.OrganisationPKInfo);
		}

		protected void CheckOrganisationPK()
		{
			var workRequest = Parent as WorkRequest;
			ListValidation.ErrorIfInvalidPK(workRequest.OrganisationPKInfo, workRequest.Lookups.Organisations);
		}

		protected override void CheckWKR_RN_NKCountry()
		{
			base.CheckWKR_RN_NKCountry();

			ListValidation.ErrorIfInvalidCode(
				ResString.GetMultilingualString(
					"D3A666EA-3ABF-485E-AA30-335033543880",
					"The country code '{0}' does not represent a valid Country. Please choose a valid Country, or create a new Country in the Maintain -> Locations -> Countries Module",
					Parent.WKR_RN_NKCountry),
				Parent.WKR_RN_NKCountryInfo);
		}

		static string InvalidEmailOnClientErrorMessage => Res.GetString("8f74bf5f-1c54-4e63-a978-b98da5cd9adb", "The selected Client does not have an email address, so cannot take part in eConversation. Please choose a Client with an email address.");
	}
}
