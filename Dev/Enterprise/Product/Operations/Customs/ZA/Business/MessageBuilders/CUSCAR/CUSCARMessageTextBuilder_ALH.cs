using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Edifact.D16A.Elements;
using Enterprise.Edifact.D16A.Messages.CUSCAR;
using Enterprise.Edifact.D16A.Segments;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.CUSCAR
{
	class CUSCARMessageTextBuilder_ALH : CUSCARMessageTextBuilder_TransportModeSEA
	{
		public CUSCARMessageTextBuilder_ALH(CUSCARMessage edifactMessage, ICusCarHeader dataSource, MessageSubTypes subType, string billIssuerCode) : base(edifactMessage, dataSource, subType, billIssuerCode)
		{
		}

		protected override void CreateGroup4DTM369_LoadingDate(SegmentGroup4 sg4)
		{
			var dtm = sg4.DTM.InstantiateAChildAndAddItToChildrenCollection();
			dtm.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier = DateOrTimeOrPeriodFunctionCodeQualifierList.DateAndOrTimeOfHandlingEstimated; //369
			dtm.DateTimePeriod.DateOrTimeOrPeriodText = source.EstimatedTimeOfLoading.ToString("yyyyMMddhhmm", CultureInfo.InvariantCulture);
			dtm.DateTimePeriod.DateOrTimeOrPeriodFormatCode = DateOrTimeOrPeriodFormatCodeList.Ccyymmddhhmm; // 203
		}

		protected override ZString GetBillsMasterBillTypeCode()
		{
			return source.AgentType == Core.Constants.AgentType.CoLoad ? "PBL" : "BOL";
		}

		protected override ZString GetBillsMasterBillDocumentNumber()
		{
			return source.AgentType == Core.Constants.AgentType.CoLoad ? source.MasterBol : source.ManifestNumber;
		}

		protected override ZBool RequiresLOC104_DepotOfUnpack(ZString depotOfUnpack) => IsDepotOfUnpackAndTerminalOfDischargeMandatory;

		protected override ZBool RequiresLOC65_TerminalOfDischarge(ZString terminalOfDischarge) => !terminalOfDischarge.IsEmpty;

		protected override void CreateGEI_CallPurpose() { }

		protected override ZBool RequiresMEA_GrossMass => ZBool.True;

		protected override ZBool RequiresMEA_VerifiedGrossMass => ZBool.True;

		protected override void AddHeaderPartyCarrierDEG()
		{
			var party = source.GetParties(billIssuerCode).FirstOrDefault(p => p.PartyType == PartyType.ReportingCarrier_DEG);
			if (party != null)
			{
				var sg2 = edifactMessage.Group2.InstantiateAChildAndAddItToChildrenCollection();
				var nad = sg2.NAD.InstantiateAChildAndAddItToChildrenCollection();
				nad.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.GetFromString(PartyFunctionCodeQualifierList.ConsortiumCarrierMaritime);
				nad.PartyIdentificationDetails.PartyIdentifier = party.IdentificationCode;
				nad.PartyName.PartyName1 = party.PartyName;
			}
		}

		protected override void AddLineGroup7Parties(SegmentGroup8 sg8, ICusCarLine bill)
		{
			base.AddLineGroup7Parties(sg8, bill);
			var partyFunctionCodeQualifier = PartyFunctionCodeQualifierList.GetFromString(PartyFunctionCodeQualifierList.FreightForwarder);
			var proxy = AsycudaManifestHeaderExtensions.ZaOrgProxyForManifestMessaging(Factory);
			var sg11 = sg8.Group11.InstantiateAChildAndAddItToChildrenCollection();
			var nad = sg11.NAD.InstantiateAChildAndAddItToChildrenCollection();
			nad.PartyFunctionCodeQualifier = partyFunctionCodeQualifier;
			nad.NameAndAddress.NameAndAddressDescription1 = proxy.OH_FullName.Left(35);
			nad.NameAndAddress.NameAndAddressDescription2 = proxy.MainAddress.Address1.Left(35);
			nad.NameAndAddress.NameAndAddressDescription3 = proxy.MainAddress.City.Left(35);
			nad.NameAndAddress.NameAndAddressDescription4 = proxy.MainAddress.State.Left(35);
			nad.NameAndAddress.NameAndAddressDescription5 = proxy.MainAddress.Postcode.Left(35);
		}

		protected override void SetLineGroup10SgpNumberOfPackages(ICusCarPackage pack, SGPSegment sgp)
		{
			sgp.PackageQuantity = pack.NumberOfPacks.ToString();
		}

		protected override ZBool RequiresNAD_Street(PartyFunctionCodeQualifierList partyFunctionCodeQualifier) => false;
	}
}
