using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.CreditControl.Business
{
	public interface ICreditControlledDocumentDelivery : ICreditControlledBusinessObject, IRelatedJobNumber, IIdentified
	{
		bool IsDPSFreightMovementRestricted { get; }
		bool IsAviationSecurityFreightMovementRestricted { get; }
		ScreeningParty[] GetScreeningParties();
		OrgHeader[] OrganisationsForCreditChecks { get; }
		string DescriptionOfOrganisationBeingCheckedForCredit { get; }
		void RaiseOnGetDocumentLogin(SecurityLoginEventArgs e);
		event EventHandler<SecurityLoginEventArgs> GetDocumentLogin;
		CustomMessageBoxCallback DocumentLoginMessageBoxCallback { get; set; }
		Logs Logs { get; }
	}
}
