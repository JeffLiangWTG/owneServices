using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.Business.MessagingProcess.Testing
{
	public class CreditControlledDocumentDeliveryImplForTest : DummyBizObjWithMessages, ICreditControlledDocumentDelivery
	{
		public CreditControlledDocumentDeliveryImplForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			topLevelBO = factory.New<DummyEnterpriseBusinessObject>();
		}
		readonly DummyEnterpriseBusinessObject topLevelBO;

		public bool IsDPSFreightMovementRestrictedForTesting { get; set; }
		public bool IsDPSFreightMovementRestricted => IsDPSFreightMovementRestrictedForTesting;

		public bool IsAviationSecurityFreightMovementRestricted => false;

		public OrgHeader[] OrganisationsForCreditChecks => Array.Empty<OrgHeader>();

		public string DescriptionOfOrganisationBeingCheckedForCredit => "Credit Check Org";

		public CustomMessageBoxCallback DocumentLoginMessageBoxCallback { get; set; }

		public Logs Logs => topLevelBO.Logs;

		public string[] JobNumber => new[] { "JOB-001" };

		public ZGuid Identifier => ZGuid.ParseSafe("9D1F8759-6EB7-4F5A-9795-25DD4C712A02");

		event EventHandler<SecurityLoginEventArgs> ICreditControlledDocumentDelivery.GetDocumentLogin
		{
			add { fGetDocumentLogin += value; }
			remove { fGetDocumentLogin -= value; }
		}
		event EventHandler<SecurityLoginEventArgs> fGetDocumentLogin;

		public ScreeningParty[] GetScreeningParties() => Array.Empty<ScreeningParty>();

		public void RaiseOnGetDocumentLogin(SecurityLoginEventArgs e)
		{
			fGetDocumentLogin?.Invoke(this, e);
		}
	}
}
