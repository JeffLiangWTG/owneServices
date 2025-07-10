using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DataTransfer.Universal.Outturn
{
	public class CusOutturnHeaderDataObjectReader<TCusOutturnHeader, TCusOutturn> : ShipmentDataObjectReader<TCusOutturnHeader>
		where TCusOutturnHeader : CusOutturnHeader
		where TCusOutturn : CusOutturn
	{
		public CusOutturnHeaderDataObjectReader(Shipment shipment, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(shipment, logger, factory)
		{
		}

		#region override
		public override DataContextType DataContextType => DataContextType.SeaCargoOutturn;

		protected override IMatchingBusinessEntityFinder<TCusOutturnHeader> GetCombinedReferenceMatcher() => null;

		protected override TCusOutturnHeader GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			var result = default(TCusOutturnHeader);
			if (LloydsIMO.HasValue && PremiseID.HasValue && dataObject.VoyageFlightNo.HasValue)
			{
				var query = new ZDBOnlyQuery(typeof(TCusOutturnHeader));
				query.AddToFilter(CusOutturnHeaderSchema.C6_LloydsIMO, LloydsIMO.Value);
				query.AddToFilter(CusOutturnHeaderSchema.C6_OutturningPremiseID, PremiseID.Value);
				query.AddToFilter(CusOutturnHeaderSchema.C6_VoyageNum, dataObject.VoyageFlightNo.Value);
				result = factory.LoadTop1<TCusOutturnHeader>(query);
			}
			return result;
		}

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(TCusOutturnHeader outturnHeader)
		{
			var reason = ZString.Empty;

			if (outturnHeader == null && !LloydsIMO.HasValue && !PremiseID.HasValue && !dataObject.VoyageFlightNo.HasValue)
			{
				reason = Res.GetString("C81F00A4-931E-4EC0-89FE-18223933C466", "Cannot create new Sea Cargo Outturn when neither Vessel Lloyds, Voyage Number or Premise ID is specified.");
			}
			else
			{
				var premiseID = PremiseID ?? outturnHeader?.C6_OutturningPremiseID ?? ZString.Empty;
				var lloydsIMO = LloydsIMO ?? outturnHeader?.C6_LloydsIMO ?? ZString.Empty;
				var voyageNum = dataObject.VoyageFlightNo ?? outturnHeader?.C6_VoyageNum ?? ZString.Empty;
				reason = CheckDuplicateJob(premiseID, lloydsIMO, voyageNum, outturnHeader);
			}
			return reason;
		}

		ZString CheckDuplicateJob(ZString premiseID, ZString lloydsIMO, ZString voyageNum, TCusOutturnHeader outturnHeader)
		{
			var reason = new ZStringBuilder();
			var query = new ZDBOnlyQuery(typeof(TCusOutturnHeader));
			query.AddToFilter(CusOutturnHeaderSchema.C6_OutturningPremiseID, premiseID);
			query.AddToFilter(CusOutturnHeaderSchema.C6_LloydsIMO, lloydsIMO);
			query.AddToFilter(CusOutturnHeaderSchema.C6_VoyageNum, voyageNum);
			if (outturnHeader != null)
			{
				query.AddToFilter(CusOutturnHeaderSchema.PK, SQLComparisonOperator.NotEqual, outturnHeader.PK);
			}
			var existedOutturnHeader = factory.LoadTop1<TCusOutturnHeader>(query);

			if (existedOutturnHeader != null)
			{
				reason.Append(Res.GetString("09D8F8D7-A196-4A2A-A9DB-67458E949E7E", "A Sea Cargo Outturn {0} already exists with the same Vessel '{1}', Voyage '{2}' and Premise ID '{3}'.",
					existedOutturnHeader.C6_SendersMessageReference, lloydsIMO, voyageNum, premiseID));
				if (outturnHeader != null)
				{
					reason.Append(Res.GetString("CD340DE4-CC36-45E4-91AB-5DC007F55DD3", "Can't update Sea Cargo Outturn {0}", outturnHeader.C6_SendersMessageReference));
				}
			}
			return reason.ToStringWithDelimiterBetweenAppends(" ");
		}

		protected override void PopulateBusinessObject(TCusOutturnHeader targetBO)
		{
			using (SuspendSetters(targetBO))
			{
				var headerRow = GetColumnIndexer(targetBO);
				SetValue(headerRow, CusOutturnHeaderSchema.C6_VoyageNum, dataObject.VoyageFlightNo);

				FillVessel(headerRow, targetBO);
				FillDates(headerRow);
				FillAdditionalReferences(headerRow);
				FillPremiseAddress(headerRow, targetBO);
				FillCountrySpecificDetails(headerRow);
				FillSubShipments(targetBO);
			}
		}
		#endregion

		#region Fill business object
		protected virtual void FillCountrySpecificDetails(IColumnIndexer headerRow)
		{
		}

		void FillSubShipments(TCusOutturnHeader outturnHeader)
		{
			var subshipments = dataObject.SubShipmentCollection;
			if (subshipments != null && subshipments.Count > 0)
			{
				Helper.MarkUnprocessedExistingBillsFor(outturnHeader, ForwardingShipmentPK);
				foreach (var subshipment in subshipments)
				{
					var outturn = FillSubShipmentCore(subshipment, logger, factory, outturnHeader);
					Helper.MarkProcessed(outturn);
				}
				Helper.DeleteUnprocessedBillsFor(outturnHeader, logger);
			}
		}

		protected ZGuid? ForwardingShipmentPK => forwardingShipmentPK ?? (forwardingShipmentPK = GetForwardingShipmentPk());
		ZGuid? forwardingShipmentPK;

		protected virtual ZGuid GetForwardingShipmentPk()
		{
			return ZGuid.Empty;
		}

		protected virtual TCusOutturn FillSubShipmentCore(Shipment shipment, IXmlImportLogger logger, UniversalObjectFactory factory, TCusOutturnHeader outturnHeader)
		{
			var reader = new CusOutturnDataObjectReader<TCusOutturn>(shipment, logger, factory, outturnHeader);
			var outturn = reader.ReadIntoBusinessObject();
			return outturn;
		}

		protected virtual void FillVessel(IColumnIndexer headerRow, TCusOutturnHeader outturnHeader)
		{
			SetValue(headerRow, CusOutturnHeaderSchema.C6_VesselName, dataObject.VesselName);
			SetValue(headerRow, CusOutturnHeaderSchema.C6_LloydsIMO, LloydsIMO);
		}

		void FillDates(IColumnIndexer headerRow)
		{
			if (dataObject.DateCollection != null && dataObject.DateCollection.Count > 0)
			{
				FillDates(headerRow, dataObject.DateCollection, ZBool.False,
					new DateTypeSchemaColumnMap(CusOutturnHeaderSchema.C6_DateOfArrival, new[] { DateType.Arrival }));
			}
		}

		void FillAdditionalReferences(IColumnIndexer headerRow)
		{
			var additionalReferences = dataObject.AdditionalReferenceCollection;
			var customsCountryCode = GlbCompany.CurrentCompany.Country.Code;
			FillAdditionalReferencesCore(headerRow, additionalReferences, CusOutturnHeaderSchema.C6_ResponsiblePartyID, Constants.AdditionalReference.EntryType.Codes.ResponsiblePartyID, customsCountryCode);
		}

		void FillAdditionalReferencesCore(IColumnIndexer headerRow, IEnumerable<AdditionalReference> additionalReferenceCollection, SchemaStringColumn column, ZString entryType, ZString contextInformation)
		{
			var additionalReference = additionalReferenceCollection?.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == entryType && x.ContextInformation.GetValueOrDefault() == contextInformation);
			if (additionalReference != null)
			{
				SetValue(headerRow, column, additionalReference.ReferenceNumber);
			}
		}

		protected virtual void FillPremiseAddress(IColumnIndexer headerRow, TCusOutturnHeader outturnHeader)
		{
			SetValue(headerRow, CusOutturnHeaderSchema.C6_OA_OutturningPremise, MatchedPremiseAddressPK);
			SetValue(headerRow, CusOutturnHeaderSchema.C6_OutturningPremiseID, PremiseID);
		}

		#endregion

		#region SuspendSetters
		IDisposable SuspendSetters(TCusOutturnHeader outturnHeader)
		{
			return outturnHeader.SetterSuspender.SuspendSetting(GetOutturnHeaderPropertiesToSuspendSetting().ToArray());
		}
		protected virtual IEnumerable<ZString> GetOutturnHeaderPropertiesToSuspendSetting() => Array.Empty<ZString>();
		#endregion

		#region Premise
		protected ZGuid? MatchedPremiseAddressPK
		{
			get
			{
				CalculatePremisseAddressPK();
				return matchedPremiseAddressPKCached;
			}
		}
		ZGuid? matchedPremiseAddressPKCached;

		void CalculatePremisseAddressPK()
		{
			if (!hasCalculatePremisseAddressPK)
			{
				hasCalculatePremisseAddressPK = false;
				matchedPremiseAddressPKCached = CalculatePremisseAddressPKCore();
			}
		}
		bool hasCalculatePremisseAddressPK;

		protected virtual ZGuid? CalculatePremisseAddressPKCore()
		{
			ZGuid? result = null;
			var premiseAddress = dataObject.OrganizationAddressCollection?.FirstOrDefault(AddressTypes.ArrivalCFSAddress);
			if (premiseAddress != null)
			{
				var matchedPremiseAddress = new OrganisationDataObjectReader(premiseAddress, logger, factory).GetMatched();
				result = matchedPremiseAddress != null && matchedPremiseAddress.OA_OH != OrgHeader.UnmatchedOrganisationPK ? matchedPremiseAddress.PK : ZGuid.Empty;
			}
			return result;
		}

		protected ZString? PremiseID
		{
			get
			{
				CalculatePremiseID();
				return premiseIDCached;
			}
		}
		ZString? premiseIDCached;

		void CalculatePremiseID()
		{
			if (!hasCalculatePremiseID)
			{
				hasCalculatePremiseID = true;
				premiseIDCached = CalculatePremiseIDCore();
			}
		}
		bool hasCalculatePremiseID;

		protected virtual ZString? CalculatePremiseIDCore()
		{
			ZString? result = null;
			var additionalReference = dataObject.AdditionalReferenceCollection?.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == Constants.AdditionalReference.EntryType.Codes.ControlledPremiseID
					&& x.ContextInformation.GetValueOrDefault() == GlbCompany.CurrentCompany.Country.Code);
			if (additionalReference != null)
			{
				result = additionalReference.ReferenceNumber;
			}
			return result;
		}
		#endregion

		#region LloydsIMO
		protected ZString? LloydsIMO
		{
			get
			{
				CalculateLloyds();
				return lloydsIMOCached;
			}
		}
		ZString? lloydsIMOCached;

		void CalculateLloyds()
		{
			if (!hasCalculatedLloyds)
			{
				hasCalculatedLloyds = true;
				lloydsIMOCached = CalculateLloydsCore();
			}
		}
		bool hasCalculatedLloyds;

		protected virtual ZString? CalculateLloydsCore() => dataObject.LloydsIMO;
		#endregion

		#region Helper
		protected SeaOutturnDataObjectReaderHelper Helper => helper ?? (helper = new SeaOutturnDataObjectReaderHelper(factory, GlbCompany.CurrentCompany.Country.Code));
		SeaOutturnDataObjectReaderHelper helper;
		#endregion
	}
}
