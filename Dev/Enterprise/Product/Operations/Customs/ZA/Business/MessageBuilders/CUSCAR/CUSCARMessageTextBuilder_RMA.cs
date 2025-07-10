using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Edifact.D16A.Elements;
using Enterprise.Edifact.D16A.Messages.CUSCAR;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.CUSCAR
{
	public class CUSCARMessageTextBuilder_RMA : CUSCARMessageTextBuilder
	{
		public CUSCARMessageTextBuilder_RMA(CUSCARMessage edifactMessage, ICusCarHeader dataSource, MessageSubTypes subType, string billIssuerCode) : base(edifactMessage, dataSource, subType, billIssuerCode)
		{
		}

		protected override void AddHeaderRFF_ACL() { }

		protected override ZBool RequiresGET_ManifestType => ZBool.True;

		protected override ZBool RequiresGroup4Loc60_PlaceOfDischarge => false;

		protected override void CreateGEI_CallPurpose() { }

		protected override void CreateGEI_CargoType()
		{
			var cmode = source.CustomsCodeForContainerMode;
			var gei = edifactMessage.GEI.InstantiateAChildAndAddItToChildrenCollection();
			gei.ProcessingInformationCodeQualifier = ProcessingInformationCodeQualifierList.ConsignmentTypeInformation;
			gei.ProcessingIndicator.ProcessingIndicatorDescriptionCode = ProcessingIndicatorDescriptionCodeList.GetFromString(cmode);
			gei.ProcessingIndicator.CodeListIdentificationCode = "122";
			gei.ProcessingIndicator.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.MutuallyDefined;
		}

		protected override ZBool RequiresMEA_GrossMass => ZBool.True;

		protected override ZBool RequiresMEA_VerifiedGrossMass => ZBool.True;

		protected override ZBool RequiresLOC8_PlaceOfDestination => ZBool.False;

		protected override ZBool RequiresLOC9_PlaceOfLoading => ZBool.False;

		protected override ZBool RequiresLOC80_PlaceOfDespatch(ZString placeOfDespatch) => ZBool.False;
	}
}
