using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Edifact.D16A.Elements;
using Enterprise.Edifact.D16A.Messages.CUSCAR;
using Enterprise.Edifact.D16A.Segments;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.CUSCAR
{
	class CUSCARMessageTextBuilder_COH : CUSCARMessageTextBuilder_TransportModeSEA
	{
		public CUSCARMessageTextBuilder_COH(CUSCARMessage edifactMessage, ICusCarHeader dataSource, MessageSubTypes subType, ZString billIssuerCode)
			: base(edifactMessage, dataSource, subType, billIssuerCode)
		{
		}

		protected override ZString GetBillsMasterBillTypeCode()
		{
			return source.AgentType == Core.Constants.AgentType.CoLoad ? "PBL" : "BOL";
		}

		protected override void SetLineGroup10SgpNumberOfPackages(ICusCarPackage pack, SGPSegment sgp)
		{
			sgp.PackageQuantity = pack.NumberOfPacks.ToString();   // repetition with above? See **** 
		}

		protected override ZString GetBillsMasterBillDocumentNumber()
		{
			return source.AgentType == Core.Constants.AgentType.CoLoad ? source.MasterBol : source.ManifestNumber;
		}

		protected override ZBool RequiresLOC104_DepotOfUnpack(ZString depotOfUnpack) => IsDepotOfUnpackAndTerminalOfDischargeMandatory;

		protected override ZBool RequiresLOC65_TerminalOfDischarge(ZString terminalOfDischarge) => !terminalOfDischarge.IsEmpty;

		protected override void CreateGroup4Loc42(SegmentGroup4 sg4)
		{
			var customsOffice = source.CustomsOffice;
			var placeOfExit = source.PlaceOfExit;
			if (!placeOfExit.IsEmpty && placeOfExit != customsOffice)
			{
				var loc42PlaceOfExit = sg4.LOC.InstantiateAChildAndAddItToChildrenCollection();
				loc42PlaceOfExit.LocationFunctionCodeQualifier = LocationFunctionCodeQualifierList.ConsignmentExitCustomsOfficeLocation; //42
				loc42PlaceOfExit.LocationIdentification.LocationIdentifier = placeOfExit;
			}
		}

		protected override ZBool RequiresMEA_GrossMass => ZBool.True;

		protected override ZBool RequiresMEA_VerifiedGrossMass => ZBool.True;
	}
}
