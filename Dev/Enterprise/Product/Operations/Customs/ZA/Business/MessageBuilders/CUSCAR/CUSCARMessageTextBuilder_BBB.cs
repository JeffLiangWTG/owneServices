using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Edifact.D16A.Elements;
using Enterprise.Edifact.D16A.Messages.CUSCAR;
using Enterprise.Edifact.D16A.Segments;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.CUSCAR
{
	class CUSCARMessageTextBuilder_BBB : CUSCARMessageTextBuilder_TransportModeSEA
	{
		public CUSCARMessageTextBuilder_BBB(CUSCARMessage edifactMessage, ICusCarHeader dataSource, MessageSubTypes subType, ZString billIssuerCode)
			: base(edifactMessage, dataSource, subType, billIssuerCode)
		{
		}

		protected override void AddHeaderPartyGroupingFZ()
		{
		}

		protected override ZBool RequiresGET_ManifestType => ZBool.True;

		protected override void CreateGEI_CargoType()
		{
			var cmode = source.CustomsCodeForContainerMode;
			var gei = edifactMessage.GEI.InstantiateAChildAndAddItToChildrenCollection();
			gei.ProcessingInformationCodeQualifier = ProcessingInformationCodeQualifierList.ConsignmentTypeInformation;
			gei.ProcessingIndicator.ProcessingIndicatorDescriptionCode = ProcessingIndicatorDescriptionCodeList.GetFromString(cmode);
			gei.ProcessingIndicator.CodeListIdentificationCode = "122";
			gei.ProcessingIndicator.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.MutuallyDefined;
		}

		protected override bool RequiresContainers => false;

		protected override void SetLineGroup10SgpNumberOfPackages(ICusCarPackage pack, SGPSegment sgp)
		{
			sgp.PackageQuantity = pack.NumberOfPacks.ToString();   // repetition with above? See **** 
		}

		protected override ZBool RequiresLOC104_DepotOfUnpack(ZString depotOfUnpack) => IsDepotOfUnpackAndTerminalOfDischargeMandatory && !depotOfUnpack.IsEmpty;

		protected override ZBool RequiresLOC65_TerminalOfDischarge(ZString terminalOfDischarge) => !terminalOfDischarge.IsEmpty;

		protected override ZString GetBillsMasterBillDocumentNumber()
		{
			return source.ManifestNumber;
		}

		protected override ZBool RequiresSG4_TranshimentDetails => IsTranshipment;
	}
}
