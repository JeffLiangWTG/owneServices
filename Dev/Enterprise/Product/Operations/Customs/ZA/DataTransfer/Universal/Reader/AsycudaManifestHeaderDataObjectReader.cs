using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.ZA.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using Shipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.ZA.DataTransfer.Universal
{
	public class AsycudaManifestHeaderDataObjectReader : ShipmentDataObjectReader<AsycudaManifestHeader>
	{
		public AsycudaManifestHeaderDataObjectReader(Shipment headerData, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(headerData, logger, factory)
		{
			this.asycudaManifestHeaderData = Argument.NotNull(headerData, "ShipmentDataObject");
			Argument.NotNull(logger, "Logger");
			Argument.NotNull(factory, "Factory");
			helper = new UniversalDataObjectReaderHelper(factory);
		}

		public override DataContextType DataContextType => DataContextType.ZAOutTurn;

		protected override IMatchingBusinessEntityFinder<AsycudaManifestHeader> GetCombinedReferenceMatcher() => null;

		protected override AsycudaManifestHeader GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			AsycudaManifestHeader asycudaManifestHeader = null;
			if (!GetJobReference(asycudaManifestHeaderData).IsEmpty)
			{
				var branchSubQuery = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.PK);
				branchSubQuery.AddToFilter(GlbBranchSchema.GB_GC, Env.CurrentCompanyPK);

				var query = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
				query.AddToFilter(AsycudaManifestHeaderSchema.AMA_JobReference, GetJobReference(asycudaManifestHeaderData));
				query.AddToFilter(AsycudaManifestHeaderSchema.AMA_ApplicationCode, AsycudaManifestHeader.ApplicationCode_Out);
				query.AddSubQuery(AsycudaManifestHeaderSchema.AMA_GB, branchSubQuery, JoinCondition.And);
				asycudaManifestHeader = factory.LoadTop1<AsycudaManifestHeader>(query);
			}
			return asycudaManifestHeader;
		}

		protected override AsycudaManifestHeader GetNewBusinessObject()
		{
			var asycudaManifestHeader = factory.New<AsycudaManifestHeader>();
			asycudaManifestHeader.AMA_RN_NKCountry = Enterprise.Core.Constants.CountryCodes.SouthAfrica;
			asycudaManifestHeader.AMA_ApplicationCode = AsycudaManifestHeader.ApplicationCode_Out;
			return asycudaManifestHeader;
		}

		protected override void PopulateBusinessObject(AsycudaManifestHeader headerBO)
		{
			if (IsPopulateBusinessObjectAllowed(headerBO))
			{
				PopulateAsycudaManifestHeader(headerBO);
			}
		}

		void PopulateAsycudaManifestHeader(AsycudaManifestHeader headerBO)
		{
			var headerRow = GetColumnIndexer(headerBO);

			new GenAddOnColumnCollectionDataObjectReader(logger).ReadIntoBusinessObject(asycudaManifestHeaderData.AddInfoCollection, helper.GetAsycudaManifestHeaderGenAddOnColumnList(headerBO), headerBO);

			FillOrgAddress(headerRow, DocAddressType.ControllingAgent, AsycudaManifestHeaderSchema.AMA_OA_ShippingAgent);
			FillOrgAddress(headerRow, DocAddressType.Carrier, AsycudaManifestHeaderSchema.AMA_OA_Carrier);
			FillOrgAddress(headerRow, DocAddressType.CustomsContainerYardAddress, AsycudaManifestHeaderSchema.AMA_OA_DeconsolidateAddress);
			FillOrgAddress(headerRow, DocAddressType.CustomsContainerTerminalOperatorAddress, AsycudaManifestHeaderSchema.AMA_OA_DischargeTerminalAddress);

			FillContainers(headerBO);

			SetValue(headerRow, AsycudaManifestHeaderSchema.AMA_ContainerMode, asycudaManifestHeaderData.ContainerMode?.Code);
			SetValue(headerRow, AsycudaManifestHeaderSchema.AMA_VesselName, asycudaManifestHeaderData.VesselName);
			SetValue(headerRow, AsycudaManifestHeaderSchema.AMA_TransportMode, asycudaManifestHeaderData.TransportMode?.Code);
			SetValue(headerRow, AsycudaManifestHeaderSchema.AMA_AgentType, asycudaManifestHeaderData.DeclarantType?.Code);
			SetValue(headerRow, AsycudaManifestHeaderSchema.AMA_Voyage, asycudaManifestHeaderData.VoyageFlightNo);
			SetValue(headerRow, AsycudaManifestHeaderSchema.AMA_CustomsOffice, asycudaManifestHeaderData.CustomsOffice?.Code);
			SetValue(headerRow, AsycudaManifestHeaderSchema.AMA_Nature, asycudaManifestHeaderData.ShipmentType?.Code);
			SetValue(headerRow, AsycudaManifestHeaderSchema.AMA_ManifestType, asycudaManifestHeaderData.ExportGoodsType?.Code);

			FillMasterBill(headerBO);
			FillHouseBills(headerBO);
		}

		void FillOrgAddress(IColumnIndexer headerRow, DocAddressType docAddressType, SchemaGuidColumn schemaGuidColumn)
		{
			var addressData = asycudaManifestHeaderData.OrganizationAddressCollection.FirstOrDefault(docAddressType.ToString());
			if (addressData != null)
			{
				var addressBO = new OrganisationDataObjectReader(addressData, logger, factory).GetMatched();
				if (addressBO != null)
				{
					SetValue(headerRow, schemaGuidColumn, addressBO.PK);
				}
			}
		}

		void FillMasterBill(AsycudaManifestHeader header)
		{
			var masterBill = header?.MasterBill;
			if (masterBill != null)
			{
				var masterBillRow = GetColumnIndexer(masterBill);
				SetValue(masterBillRow, AsycudaBillSchema.ABL_BillNumber, asycudaManifestHeaderData.WayBillNumber);
				SetValue(masterBillRow, AsycudaBillSchema.ABL_CarrierReference, asycudaManifestHeaderData.QuoteNumber);
				SetDate(masterBillRow, asycudaManifestHeaderData.DateCollection, DateType.BillIssued, AsycudaBillSchema.ABL_BillIssueDate);
				SetDate(masterBillRow, asycudaManifestHeaderData.DateCollection, DateType.Arrival, AsycudaBillSchema.ABL_E_ARV);
				SetDate(masterBillRow, asycudaManifestHeaderData.DateCollection, DateType.Departure, AsycudaBillSchema.ABL_E_DEP);
				SetValue(masterBillRow, AsycudaBillSchema.ABL_RL_NKPortOfDischarge, asycudaManifestHeaderData.PortOfDischarge);
				SetValue(masterBillRow, AsycudaBillSchema.ABL_RL_NKPortOfLoading, asycudaManifestHeaderData.PortOfLoading);
				SetValue(masterBillRow, AsycudaBillSchema.ABL_GoodsLocation, asycudaManifestHeaderData.LocationAtClearance);
			}
		}

		void FillHouseBills(AsycudaManifestHeader header)
		{
			if (asycudaManifestHeaderData.SubShipmentCollection != null && asycudaManifestHeaderData.SubShipmentCollection.Count > 0)
			{
				foreach (var billData in asycudaManifestHeaderData.SubShipmentCollection)
				{
					new AsycudaBillDataObjectReader(billData, logger, factory, header, helper).ReadIntoBusinessObject();
				}
			}
		}

		void SetDate(IColumnIndexer row, List<Date> dateCollection, DateType dateType, SchemaDateTimeColumn column)
		{
			if (dateCollection != null && dateCollection.Count > 0)
			{
				FillDates(row, asycudaManifestHeaderData.DateCollection, ZBool.False, new DateTypeSchemaColumnMap(column, dateType));
			}
		}

		void FillContainers(AsycudaManifestHeader header)
		{
			if (asycudaManifestHeaderData.ContainerCollection != null)
			{
				helper.ContainersReaderHelper.MarkUnprocessedExistingObjectFor(factory, header);
				foreach (var containerData in asycudaManifestHeaderData.ContainerCollection)
				{
					var container = new AsycudaContainerDataObjectReader(containerData, logger, factory, header, helper).ReadIntoBusinessObject();
					helper.ContainersReaderHelper.MarkProcessed(container);
				}
				helper.ContainersReaderHelper.DeleteUnprocessedObjectsFor(header, logger);
			}
		}

		bool IsPopulateBusinessObjectAllowed(AsycudaManifestHeader header)
		{
			if (header.IsInDatabase && header.IsMessagingActive)
			{
				logger.LogBoth(LogType.Warning, ZString.Format(cannotBeUpdatedMessage, header.AMA_JobReference));
				return false;
			}
			return true;
		}

		readonly ZString cannotBeUpdatedMessage = Res.GetString("F7C25CAD-3F78-48CB-81A2-DB31B9222B0C", "Header data on Outturn Gate in/out Job '{0}' cannot be updated as this job has been submitted to Customs.");

		static ZString GetJobReference(Shipment headerData) => headerData.GetMatchingDataTarget(DataContextType.ZAOutTurn)?.Key ?? ZString.Empty;

		readonly Shipment asycudaManifestHeaderData;
		readonly UniversalDataObjectReaderHelper helper;
	}
}
