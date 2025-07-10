using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Edifact.D16A.Messages.CUSCAR;
using Enterprise.Edifact.D16A.Segments;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.CUSCAR
{
	class CUSCARMessageTextBuilder_COM : CUSCARMessageTextBuilder_TransportModeSEA
	{
		public CUSCARMessageTextBuilder_COM(CUSCARMessage edifactMessage, ICusCarHeader dataSource, MessageSubTypes subType, ZString billIssuerCode)
			: base(edifactMessage, dataSource, subType, billIssuerCode)
		{
		}

		protected override void AddHeaderPartyGroupingFZ() { }

		protected override void SetLineGroup10SgpNumberOfPackages(ICusCarPackage pack, SGPSegment sgp)
		{
			sgp.PackageQuantity = pack.NumberOfPacks.ToString();
		}

		protected override ZString GetBillsMasterBillDocumentNumber()
		{
			return source.ManifestNumber;
		}

		protected override ZBool RequiresMEA_GrossMass => ZBool.True;

		protected override ZBool RequiresMEA_VerifiedGrossMass => ZBool.True;
	}
}
