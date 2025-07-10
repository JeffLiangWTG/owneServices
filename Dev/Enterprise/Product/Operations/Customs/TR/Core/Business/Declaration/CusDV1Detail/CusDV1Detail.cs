using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class CusDV1Detail : EU.Business.Declaration.CusDV1Detail
	{
		public CusDV1Detail(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override EU.Business.Declaration.CusDV1DetailValidation GetNewValidation() => new CusDV1DetailValidation(this);
		public new CusDV1DetailValidation Validation => (CusDV1DetailValidation)base.Validation;

		[MaxLength(20)]
		public override ZString DV1_ContractNumber { get => base.DV1_ContractNumber; set => base.DV1_ContractNumber = value; }

		[ResourceStringData("95DB9F4A-EA98-4628-9CE4-F54773715DBC", Caption = "Close Approximation")]
		public override ZString DV1_CloseApproximation { get => base.DV1_CloseApproximation; set => base.DV1_CloseApproximation = value; }

		[ResourceStringData("735B01A5-8226-4532-8096-A0C836E11547", Caption = "Relation Details")]
		public override ZString DV1_RelationDetails { get => base.DV1_RelationDetails; set => base.DV1_RelationDetails = value; }

		[ResourceStringData("7447DE9E-AB09-40A3-B895-CE4CA988816A", Caption = "Restriction Details")]
		public override ZString DV1_RestrictionConsiderationDetails { get => base.DV1_RestrictionConsiderationDetails; set => base.DV1_RestrictionConsiderationDetails = value; }

		[ResourceStringData("3859295D-7567-4BB8-9E96-85DA9C524079", Caption = "License Details")]
		public override ZString DV1_RoyaltiesLicenceDetails { get => base.DV1_RoyaltiesLicenceDetails; set => base.DV1_RoyaltiesLicenceDetails = value; }

		[ResourceStringData("EAE78BF0-A3F1-4890-9C2E-A2670A94EC85", Caption = "Resale Details")]
		public override ZString DV1_ResaleDetails { get => base.DV1_ResaleDetails; set => base.DV1_ResaleDetails = value; }

		[MaxLength(50)]
		[ResourceStringData("EBF2DE0C-A4C9-4C59-9B2D-3C8D2E558F42", Caption = "Place")]
		public override ZString DV1_Place { get => base.DV1_Place; set => base.DV1_Place = value; }

		[ResourceStringData("55C6A9CF-DCCE-435F-8672-175E4B316E22", Caption = "Customs Decision Number")]
		public override ZString DV1_CustomsDecisionNumber { get => base.DV1_CustomsDecisionNumber; set => base.DV1_CustomsDecisionNumber = value; }

		[ResourceStringData("EF0F4C28-89DD-4A2B-8602-7D004389CD35", Caption = "Customs Decision Date")]
		public override ZDate DV1_CustomsDecisionDate { get => base.DV1_CustomsDecisionDate; set => base.DV1_CustomsDecisionDate = value; }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			DV1_CloseApproximation = YesNoList.Codes.No;
		}
	}
}
