using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal.DataReaderExtensions;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using OrgAddress = Enterprise.MasterFiles.Business.OrgAddress;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;
using ValueSetter = Enterprise.UniversalDataBuss.DataObjects.Core.ValueSetter;

namespace Enterprise.Customs.DataTransfer.Universal.AirManifest
{
	public class CusMAWBDataObjectReader : CusMAWBDataObjectReader<CusMAWB, CusHAWB, AirManifestDataObjectReaderHelper>
	{
		public CusMAWBDataObjectReader(UniversalShipment mawbDataObject, UniversalShipment hVLVShipperConsolidation, IXmlImportLogger logger, UniversalObjectFactory factory, ZString applicationCode, bool singleHAWBCheck = false)
			: base(mawbDataObject, hVLVShipperConsolidation, logger, factory, applicationCode, singleHAWBCheck)
		{
		}

		protected override AirManifestDataObjectReaderHelper CreateNewUniversalDataObjectReaderHelper()
		{
			return new AirManifestDataObjectReaderHelper(factory, dataObject.GetTargetCountryCode());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected override IEnumerable<CusHAWBDataObjectReader<CusMAWB, CusHAWB, AirManifestDataObjectReaderHelper>> GetNewCusHAWBDataObjectReaders(UniversalShipment shipmentDataObject, UniversalShipment parentShipmentDataObject, CusMAWB mawb)
		{
			yield return new CusHAWBDataObjectReader(shipmentDataObject, MAWBDataObject, logger, Helper, mawb, null, IsHVLV, singleHAWBCheck);
		}
	}

	public abstract class CusMAWBDataObjectReader<TMAWB, THAWB, THelper> : ShipmentDataObjectReader<TMAWB>, IOrganisationDataObjectReaderSupporter
		where TMAWB : CusMAWB
		where THAWB : CusHAWB
		where THelper : AirManifestDataObjectReaderHelper
	{
		protected CusMAWBDataObjectReader(UniversalShipment mawbDataObject, UniversalShipment hVLVShipperConsolidation, IXmlImportLogger logger, UniversalObjectFactory factory, ZString applicationCode, bool singleHAWBCheck = false)
			: base(mawbDataObject, logger, factory)
		{
			this.applicationCode = Argument.NotNullOrEmpty(applicationCode, "applicationCode");
			this.singleHAWBCheck = singleHAWBCheck;
			this.HVLVShipperConsolidation = hVLVShipperConsolidation;
		}

		public override DataContextType DataContextType => DataContextType.AirManifest;

		protected override CharacterCase StringValueCharacterCase => CharacterCase.Upper;

		protected readonly ZString applicationCode;
		protected readonly bool singleHAWBCheck;
		protected readonly UniversalShipment HVLVShipperConsolidation;

		protected ZBool IsHVLV
		{
			get
			{
				return (this.dataObject.DataContext?.EventType?.Code.GetValueOrDefault() ?? ZString.Empty) == ZArchitecture.Business.Events.HVLVReadyCode
						|| HVLVShipperConsolidation != null;
			}
		}

		protected override TMAWB GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			return null;
		}

		protected override IMatchingBusinessEntityFinder<TMAWB> GetCombinedReferenceMatcher()
		{
			return null; // No Combined Reference MAtching has been implemented for Customs. Considering we're looking at replacing this with Reference and Party ID matching, is best not to implement.
		}

		protected sealed override void PopulateBusinessObject(TMAWB mawb)
		{
			var valueSetters = GetValueSetters(mawb).ToArray();
			if (CheckUpdateMAWBDataIsAllowed(mawb, valueSetters))
			{
				foreach (var delaySetter in valueSetters)
				{
					delaySetter.SetValue();
				}
				FillNotes(mawb);
			}
			FillHAWBs(mawb);
		}

		protected IEnumerable<ValueSetter> GetValueSetters(TMAWB mawb)
		{
			var result = new Dictionary<string, ValueSetter>();

			var mawbRow = GetColumnIndexer(mawb);

			SetValue(mawbRow, CusMAWBSchema.CM_ApplicationCode, applicationCode, result);
			SetValue(mawbRow, CusMAWBSchema.CM_IsCTOMAWB, ZBool.False, result);
			if (dataObject.WayBillNumber.HasValue)
			{
				SetValue(mawbRow, CusMAWBSchema.CM_MAWB, dataObject.GetMasterBill(), result);
			}
			SetValue(mawbRow, CusMAWBSchema.CM_FlightNo, dataObject.VoyageFlightNo, result);
			SetValue(mawbRow, CusMAWBSchema.CM_RL_NKLoadPort, dataObject.PortOfLoading, result);
			SetValue(mawbRow, CusMAWBSchema.CM_RL_NKDischargePort, dataObject.PortOfDischarge, result);
			SetValue(mawbRow, CusMAWBSchema.CM_RL_NKFirstArrivalPort, dataObject.PortOfFirstArrival, result);
			SetValue(mawbRow, CusMAWBSchema.CM_Folio, dataObject.Folio, result);
			FillBranch(mawbRow, result);
			FillDates(mawb, result);
			FillResponsiblePartyDetails(mawb, result);
			FillCountrySpecificDetails(mawb, result);
			FillDepotAddress(mawb, result);
			FillGoodsLocation(mawb, result);
			return result.Values;
		}

		void FillGoodsLocation(IColumnIndexer mawbRow, Dictionary<string, ValueSetter> valueSetters)
		{
			OrganizationAddress goodsLocationData = null;

			if (dataObject.OrganizationAddressCollection != null)
			{
				goodsLocationData = dataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.GoodsLocation));
			}
			if (goodsLocationData == null)
			{
				var shipmentDO = MAWBDataObject;
				if (shipmentDO.OrganizationAddressCollection != null)
				{
					goodsLocationData = shipmentDO.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.GoodsLocation));
				}
			}

			if (goodsLocationData != null)
			{
				var goodsLocationAddress = GetOrgAddressBO(goodsLocationData, logger.TopLevelDataContext);
				if (goodsLocationAddress != null)
				{
					SetValue(mawbRow, CusMAWBSchema.CM_OA_GoodsLocation, goodsLocationAddress.PK, valueSetters);
				}
			}
		}

		void FillDepotAddress(IColumnIndexer mawbRow, Dictionary<string, ValueSetter> valueSetters)
		{
			if (dataObject.OrganizationAddressCollection != null)
			{
				var depotAddressData = dataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.ArrivalCFSAddress));
				if (depotAddressData != null)
				{
					var depotAddress = GetOrgAddressBO(depotAddressData, logger.TopLevelDataContext);
					if (depotAddress != null)
					{
						SetValue(mawbRow, CusMAWBSchema.CM_OA_UnpackDepotAddress, depotAddress.PK, valueSetters);
					}
				}
			}
		}

		OrgAddress GetOrgAddressBO(OrganizationAddress orgAddressData, IDataContextDataObject topLevelDataContext)
		{
			if (topLevelDataContext != null && topLevelDataContext.CodesMappedToTarget)
			{
				var orgAddress = orgAddressData.GetMatchedUsingCodes(factory.BOFactory);
				if (orgAddress != null)
				{
					OrganisationDataObjectReader.LogSuccessfulMatch(orgAddress.Header, logger);
					return orgAddress;
				}
			}

			return new OrganisationDataObjectReader(orgAddressData, logger, factory).GetMatched();
		}

		protected virtual bool CheckUpdateMAWBDataIsAllowed(TMAWB mawb, IEnumerable<ValueSetter> valueSetters)
		{
			return true;
		}

		protected THelper Helper
		{
			get { return helper ?? (helper = CreateNewUniversalDataObjectReaderHelper()); }
		}
		THelper helper;

		protected abstract THelper CreateNewUniversalDataObjectReaderHelper();

		protected UniversalShipment MAWBDataObject => HVLVShipperConsolidation ?? dataObject;

		protected virtual void FillHAWBs(TMAWB mawb)
		{
			var hawbs = new List<THAWB>();

			UniversalShipment mawbDataObject = MAWBDataObject;
			if (mawbDataObject.SubShipmentCollection != null)
			{
				if (!singleHAWBCheck)
				{
					Helper.MarkUnprocessedExistingBillsFor(mawb);
				}
				foreach (var hawbDataObject in mawbDataObject.SubShipmentCollection)
				{
					foreach (var hawbReader in GetNewCusHAWBDataObjectReaders(hawbDataObject, mawbDataObject, mawb))
					{
						var bill = hawbReader.ReadIntoBusinessObject();
						Helper.MarkProcessed(bill);
						hawbs.Add(bill);
					}
				}
				if (!singleHAWBCheck)
				{
					Helper.DeleteUnprocessedBillsFor(mawb, logger);
				}

				mawb.ChildBills.Reload(false);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected abstract IEnumerable<CusHAWBDataObjectReader<TMAWB, THAWB, THelper>> GetNewCusHAWBDataObjectReaders(UniversalShipment shipmentDataObject, UniversalShipment parentShipmentDataObject, TMAWB mawb);

		void FillBranch(IColumnIndexer mawbRow, Dictionary<string, ValueSetter> delaySetters)
		{
			ZGuid branchPK = dataObject.GetBranchPK(factory.BOFactory);
			if (branchPK.IsValid)
			{
				SetValue(mawbRow, CusMAWBSchema.CM_GB, branchPK, delaySetters);
			}
		}

		protected virtual void FillResponsiblePartyDetails(TMAWB mawb, Dictionary<string, ValueSetter> valueSetters)
		{
			var mawbRow = GetColumnIndexer(mawb);
			ZString? responsiblePartyID = null;
			ZGuid? responsiblePartyPK = null;
			if (dataObject.AdditionalReferenceCollection != null)
			{
				responsiblePartyID = ZString.Empty;
				var responsiblePartyIDData = dataObject.AdditionalReferenceCollection.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == Constants.AdditionalReference.EntryType.Codes.ResponsiblePartyID);
				if (responsiblePartyIDData != null)
				{
					responsiblePartyID = responsiblePartyIDData.ReferenceNumber;
					responsiblePartyPK = ZGuid.Empty;
				}
			}

			if (dataObject.OrganizationAddressCollection != null)
			{
				var responsiblePartyAddress = dataObject.OrganizationAddressCollection.FirstOrDefault(ResponsiblePartyAddressType);
				if (responsiblePartyAddress != null)
				{
					responsiblePartyPK = ZGuid.Empty;
					var responsibleParty = new OrganisationDataObjectReader(responsiblePartyAddress, logger, factory).GetMatched();
					if (responsibleParty != null && responsibleParty.OA_OH != OrgHeader.UnmatchedOrganisationPK)
					{
						responsiblePartyPK = responsibleParty.OA_OH;
						var orgResponsiblePartyID = GetResponsiblePartyID(responsiblePartyPK.Value, responsiblePartyID);
						if (responsiblePartyID.HasValue)
						{
							if (!orgResponsiblePartyID.HasValue || orgResponsiblePartyID.Value != responsiblePartyID.Value)
							{
								responsiblePartyPK = ZGuid.Empty;
							}
						}
						else if (orgResponsiblePartyID.HasValue)
						{
							responsiblePartyID = orgResponsiblePartyID;
						}
					}
				}
			}
			if (responsiblePartyPK.HasValue)
			{
				SetValue(mawbRow, CusMAWBSchema.CM_OH_ResponsibleParty, responsiblePartyPK, valueSetters);
			}
			if (responsiblePartyID.HasValue)
			{
				SetValue(mawbRow, CusMAWBSchema.CM_ResponsiblePartyID, responsiblePartyID, valueSetters);
			}
		}

		protected virtual string ResponsiblePartyAddressType
		{
			get { return AddressTypes.ResponsibleParty; }
		}

		protected virtual ZString? GetResponsiblePartyID(ZGuid responsiblePartyPK, ZString? responsiblePartyID)
		{
			return responsiblePartyID;
		}

		protected virtual void FillCountrySpecificDetails(TMAWB mawb, Dictionary<string, ValueSetter> valueSetters)
		{
			var coLoadBill = dataObject.GetColoadBill(HVLVShipperConsolidation);
			if (coLoadBill.HasValue)
			{
				var mawbRow = GetColumnIndexer(mawb);
				SetValue(mawbRow, CusMAWBSchema.CM_MasterHouseBill, coLoadBill.Value, valueSetters);
			}
		}

		protected virtual void FillDates(TMAWB mawb, Dictionary<string, ValueSetter> valueSetters)
		{
			var mawbRow = GetColumnIndexer(mawb);
			if (dataObject.DateCollection != null && dataObject.DateCollection.Count > 0)
			{
				FillDates(mawbRow, dataObject.DateCollection, ZBool.False, valueSetters, new DateTypeSchemaColumnMap(CusMAWBSchema.CM_DateOfFirstArrival, DateType.FirstArrivalInCountry));
			}
			SetValue(mawbRow, CusMAWBSchema.CM_DepartureDate, dataObject.GetLoadingDateForAir(GetDepartureFlight(mawb)), valueSetters);
			SetValue(mawbRow, CusMAWBSchema.CM_ArrivalDate, dataObject.GetDischargeDateForAir(GetArrivalFlight(mawb)), valueSetters);
		}

		protected virtual TransportLeg GetDepartureFlight(TMAWB mawb) { return null; }
		protected virtual TransportLeg GetArrivalFlight(TMAWB mawb) { return null; }

		void FillNotes(TMAWB mawb)
		{
			if (dataObject.NoteCollection != null)
			{
				// TODO : Handle Linked to Consol
				new NotesCollectionReader(dataObject.NoteCollection, logger, factory, mawb).ReadIntoCollection();
			}
		}

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(TMAWB mawb)
		{
			var builder = new ZStringBuilder();
			if (dataObject.WayBillType.GetCodeAsUpperCase() != WayBillTypeList.Codes.Master)
			{
				builder.Append(Res.GetString("B25B9F6C-874A-46E6-8196-C5634360BA91", "{1} must be '{0}'.", WayBillTypeList.Codes.Master, "WayBillType.Code"));
			}
			if (dataObject.WayBillNumber.GetValueOrDefault().IsEmpty)
			{
				builder.Append(Res.GetString("91EFAB76-6BD3-4442-A749-765CD5CA81C4", "{0} must not be empty.", "WayBillNumber"));
			}
			if (builder.IsEmpty && singleHAWBCheck && HasMultipleHAWBs())
			{
				builder.Append(Res.GetString("8BCDDE18-F78C-484D-8194-71727CDE90B0", "Only one House Bill ({0} element) is allowed for '{1}' importation.", "SubShipmentCollection", nameof(DataContextType.AirManifestLine)));
			}
			return builder.ToStringWithNewLineBetweenAppends();
		}

		bool HasMultipleHAWBs()
		{
			var mawbDataObject = HVLVShipperConsolidation ?? dataObject;
			return mawbDataObject.SubShipmentCollection != null && dataObject.SubShipmentCollection.Count > 1;
		}

		#region IOrganisationDataObjectReaderSupporter Members

		OrganisationDataObjectReader IOrganisationDataObjectReaderSupporter.CreateNewReader(OrganizationAddress addressData)
		{
			return new OrganisationDataObjectReader(addressData, logger, factory);
		}

		#endregion
	}
}
