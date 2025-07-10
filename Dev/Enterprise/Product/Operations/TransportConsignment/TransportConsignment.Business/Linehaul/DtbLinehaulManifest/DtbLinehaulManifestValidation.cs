using CargoWise.EntityFramework;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbLinehaulManifestValidation : AutoDtbLinehaulManifestValidation
	{
		public DtbLinehaulManifestValidation(AutoDtbLinehaulManifest parent)
			: base(parent)
		{
		}

		new DtbLinehaulManifest Parent
		{
			get { return (DtbLinehaulManifest)base.Parent; }
		}

		protected override void CheckLHM_GS_NKDriver1()
		{
			base.CheckLHM_GS_NKDriver1();
			ListValidation.ErrorIfInvalidCode(Parent.LHM_GS_NKDriver1Info);
		}

		protected override void CheckLHM_GS_NKDriver2()
		{
			base.CheckLHM_GS_NKDriver2();
			ListValidation.ErrorIfInvalidCode(Parent.LHM_GS_NKDriver2Info);
		}

		protected override void CheckLHM_OA_OriginDepot()
		{
			base.CheckLHM_OA_OriginDepot();
			MandatoryValidation.CheckEntered(Parent.LHM_OA_OriginDepotInfo);
		}

		protected override void CheckLHM_OA_DestinationDepot()
		{
			base.CheckLHM_OA_DestinationDepot();
			MandatoryValidation.CheckEntered(Parent.LHM_OA_DestinationDepotInfo);
		}

		protected override void CheckLHM_Status()
		{
			base.CheckLHM_Status();
			MandatoryValidation.CheckEntered(Parent.LHM_StatusInfo);
			ListValidation.ErrorIfInvalidCode(Parent.LHM_StatusInfo);
		}

		protected override void CheckLHM_StartDateTimeUtc()
		{
			base.CheckLHM_StartDateTimeUtc();
			MandatoryValidation.CheckEntered(Parent.LHM_StartDateTimeUtcInfo);
		}

		public void ValidateTransportCompanyPK()
		{
			ValidateCalculatedProperty(Parent.TransportCompanyPKInfo);
		}

		protected void CheckTransportCompanyPK()
		{
			ListValidation.ErrorIfInvalidPK(Parent.TransportCompanyPKInfo);
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateTransportCompanyPK();
		}
	}
}
