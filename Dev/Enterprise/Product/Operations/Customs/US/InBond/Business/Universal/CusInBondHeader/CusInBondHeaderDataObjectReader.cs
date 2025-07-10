using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using ValueSetter = Enterprise.UniversalDataBuss.DataObjects.Core.ValueSetter;

namespace Enterprise.Customs.US.InBond.Business.Universal
{
	public class CusInBondHeaderDataObjectReader : DataTransfer.Universal.CusInBondHeaderDataObjectReader<CusInBondHeader, CusInBondBill, CusInBondMoveHeader, CusInBondMoveDetail, CusInBondContainer, CusInBondCargoDesc>
	{
		public CusInBondHeaderDataObjectReader(Shipment headerDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ICusInBondParent parentBO)
			: base(headerDataObject, logger, factory, parentBO)
		{
		}

		public override DataContextType DataContextType
		{
			get { return DataContextType.InBond; }
		}

		protected override ZString CusInBondApplicationCode
		{
			get { return CusInBondApplicationCodeList.Codes.InBond; }
		}

		protected override MutexID InBondMutexID
		{
			get { return MutexIDs.InBondBeingCreatedForJob; }
		}

		protected override ZString InBondDescription
		{
			get { return "In-Bond"; }
		}

		protected override CusInBondHeader GetMatchedExistingInBondHeader()
		{
			CusInBondHeader result = null;
			existingMatched = null;
			if (InBondNumbers.Length > 0)
			{
				var headerQuery = new ZDBOnlyQuery(typeof(CusInBondHeader));
				headerQuery.AddToFilter(CusInBondHeaderSchema.BH_ApplicationCode, CusInBondApplicationCode);
				var moveHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
				var inBondNumberQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
				inBondNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, Enterprise.Customs.US.Business.CusEntryHeaderMessageTypeList.Codes.InBond);
				inBondNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, InBondNumbers);
				moveHeaderQuery.AddSubQuery(inBondNumberQuery, JoinCondition.And);
				headerQuery.AddSubQuery(moveHeaderQuery, JoinCondition.And);
				headerQuery.OrderBy = CusInBondHeaderSchema.BH_SystemCreateTimeUtc.Name;
				existingMatched = factory.Load<CusInBondHeader>(headerQuery);
				if (existingMatched.Length == 1)
				{
					result = existingMatched[0];
				}
			}
			return result;
		}
		CusInBondHeader[] existingMatched;

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(CusInBondHeader targetBO)
		{
			var result = base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(targetBO);
			if (result.IsEmpty && existingMatched != null && existingMatched.Length > 1)
			{
				result = "Multiple jobs were matched with InBond Number (" + string.Join(", ", InBondNumbers.Select(x => "'" + x + "'")) + ")";
			}
			return result;
		}

		protected new InBondDataObjectReaderHelper Helper
		{
			get { return (InBondDataObjectReaderHelper)base.Helper; }
		}

		protected override void FillInBondSpecificData(IColumnIndexer headerRow, Dictionary<string, ValueSetter> delaySetters, CusInBondHeader headerBO)
		{
			base.FillInBondSpecificData(headerRow, delaySetters, headerBO);
			FillImporterAndSupplier(headerRow, delaySetters);
			FillFirstExportDetail(headerRow, delaySetters);
		}

		internal void PopulateMainData(CusInBondHeader header, CusInBondMoveHeader warehouseMovement)
		{
			PopulateBusinessObjectCore(header, warehouseMovement);
		}

		protected override IEnumerable<ZString> GetHeaderSettingOrder(CusInBondHeader header)
		{
			yield return ColumnValueSetter.GetKey(header.PK, CusInBondHeaderSchema.BH_OverrideFreightDefaults);
			yield return ColumnValueSetter.GetKey(header.PK, CusInBondHeaderSchema.BH_OA_Importer);
			yield return ColumnValueSetter.GetKey(header.PK, CusInBondHeaderSchema.BH_OH_Supplier);
			yield return ColumnValueSetter.GetKey(header.PK, CusInBondHeaderSchema.BH_GB);
			yield return ColumnValueSetter.GetKey(header.PK, CusInBondHeaderSchema.BH_HeaderType);
			yield return ColumnValueSetter.GetKey(header.PK, CusInBondHeaderSchema.BH_FTZMove);
			yield return ColumnValueSetter.GetKey(header.PK, CusInBondHeaderSchema.BH_FIRMS);
			yield return ColumnValueSetter.GetKey(header.PK, CusInBondHeaderSchema.BH_ImportTransportMode);
			yield return ColumnValueSetter.GetKey(header.PK, CusInBondHeaderSchema.BH_LloydsNumber);
			yield return ColumnValueSetter.GetKey(header.PK, CusInBondHeaderSchema.BH_VoyageNumber);
			yield return ColumnValueSetter.GetKey(header.PK, CusInBondHeaderSchema.BH_CarrierSCAC);
			yield return ColumnValueSetter.GetKey(header.PK, CusInBondHeaderSchema.BH_ImportConveyanceCountry);
			yield return ColumnValueSetter.GetKey(header.PK, CusInBondHeaderSchema.BH_ImportConveyanceName);
			yield return ColumnValueSetter.GetKey(header.PK, CusInBondHeaderSchema.BH_ImportLoadPortKCode);
			yield return ColumnValueSetter.GetKey(header.PK, CusInBondHeaderSchema.BH_SailingDate);
			yield return ColumnValueSetter.GetKey(header.PK, CusInBondHeaderSchema.BH_RN_NKFirstExportCountry);
			yield return ColumnValueSetter.GetKey(header.PK, CusInBondHeaderSchema.BH_FirstExportDate);
			yield return ColumnValueSetter.GetKey(header.PK, CusInBondHeaderSchema.BH_PortUnladingDCode);
			yield return ColumnValueSetter.GetKey(header.PK, CusInBondHeaderSchema.BH_ETA);
		}

		void FillFirstExportDetail(IColumnIndexer headerRow, Dictionary<string, ValueSetter> delaySetters)
		{
			if (dataObject.TransportLegCollection != null)
			{
				var firstLeg = dataObject.TransportLegCollection.FirstOrDefault(x => x.LegOrder.GetValueOrDefault() == 1) ?? dataObject.TransportLegCollection.FirstOrDefault();
				if (firstLeg == null)
				{
					SetValue(headerRow, CusInBondHeaderSchema.BH_FirstExportDate, ZDateTime.Empty, delaySetters);
					SetValue(headerRow, CusInBondHeaderSchema.BH_RN_NKFirstExportCountry, ZString.Empty, delaySetters);
				}
				else
				{
					SetValue(headerRow, CusInBondHeaderSchema.BH_FirstExportDate, firstLeg.ActualDeparture.HasValue ? firstLeg.ActualDeparture : firstLeg.EstimatedDeparture, delaySetters);
					if (firstLeg.PortOfLoading != null)
					{
						SetValue(headerRow, CusInBondHeaderSchema.BH_RN_NKFirstExportCountry, firstLeg.PortOfLoading.Code.GetValueOrDefault().Left(2), delaySetters);
					}
				}
			}
		}

		protected override void FillDataFromAddInfos(IColumnIndexer headerRow, Dictionary<string, ValueSetter> delaySetters)
		{
			base.FillDataFromAddInfos(headerRow, delaySetters);
			if (dataObject.AddInfoCollection != null)
			{
				SetValue(headerRow, CusInBondHeaderSchema.BH_FTZMove, dataObject.AddInfoCollection.GetZBoolValue(Constants.Header.AddInfo.FTZMove), delaySetters);
				SetValue(headerRow, CusInBondHeaderSchema.BH_CarrierSCAC, dataObject.AddInfoCollection.GetZStringValue(Constants.Header.AddInfo.UI_NKCarrierSCAC), delaySetters);
				SetValue(headerRow, CusInBondHeaderSchema.BH_PortUnladingDCode, dataObject.AddInfoCollection.GetZStringValue(Constants.Header.AddInfo.SchDArrival), delaySetters);
				SetValue(headerRow, CusInBondHeaderSchema.BH_FIRMS, dataObject.AddInfoCollection.GetZStringValue(Constants.Header.AddInfo.US_NKLocationOfGoods), delaySetters);
				SetValue(headerRow, CusInBondHeaderSchema.BH_HeaderType, dataObject.AddInfoCollection.GetZStringValue(Constants.Header.AddInfo.InBondMode), delaySetters);
				SetValue(headerRow, CusInBondHeaderSchema.BH_ImportLoadPortKCode, dataObject.AddInfoCollection.GetZStringValue(Constants.Header.AddInfo.SchDLoading), delaySetters);
			}
		}

		void FillImporterAndSupplier(IColumnIndexer headerRow, Dictionary<string, ValueSetter> delaySetters)
		{
			if (dataObject.OrganizationAddressCollection != null)
			{
				var importerAddress = dataObject.OrganizationAddressCollection.FindBestImporterMatch();
				if (importerAddress != null)
				{
					var reader = new OrganisationDataObjectReader(importerAddress, logger, factory);
					var orgAddress = reader.GetMatched();
					SetValue(headerRow, CusInBondHeaderSchema.BH_OA_Importer, orgAddress == null ? ZGuid.Empty : orgAddress.PK, delaySetters);
				}
				var supplierAddress = dataObject.OrganizationAddressCollection.FindBestSupplierMatch();
				if (supplierAddress != null)
				{
					var reader = new OrganisationDataObjectReader(supplierAddress, logger, factory);
					var orgAddress = reader.GetMatched();
					SetValue(headerRow, CusInBondHeaderSchema.BH_OH_Supplier, orgAddress == null ? ZGuid.Empty : orgAddress.OA_OH, delaySetters);
				}
			}
		}

		protected override void FillDates(IColumnIndexer headerRow, Dictionary<string, ValueSetter> delaySetters)
		{
			base.FillDates(headerRow, delaySetters);

			if (dataObject.DateCollection != null && dataObject.DateCollection.Count > 0)
			{
				FillDates(headerRow, dataObject.DateCollection, ZBool.False,
					new DateTypeSchemaColumnMap(CusInBondHeaderSchema.BH_SailingDate, new[] { DateType.Departure, DateType.LoadingDate }),
					new DateTypeSchemaColumnMap(CusInBondHeaderSchema.BH_ETA, new[] { DateType.Arrival, DateType.DischargeDate }));
			}
		}

		protected override DataTransfer.Universal.InBondDataObjectReaderHelper GetInBondDataObjectReaderHelperCore()
		{
			return new InBondDataObjectReaderHelper(factory);
		}

		protected override void FillInBondMovements(CusInBondHeader headerBO, CusInBondMoveHeader warehouseMovement)
		{
			if (dataObject.InBondMoveHeaderCollection != null)
			{
				Helper.CollectContainerPackingLineAndCommercialInvoiceLineDetails(dataObject);
				if (warehouseMovement == null)
				{
					Helper.MarkUnprocessedExistingMovementsFor(headerBO);
					Helper.SetCusInBondCargoDescCustomLabelsProvider(headerBO);
					foreach (var inBondMoveHeaderDataObject in dataObject.InBondMoveHeaderCollection)
					{
						var inBondMoveHeader = new CusInBondMoveHeaderDataObjectReader(inBondMoveHeaderDataObject, logger, Helper, headerBO).ReadIntoBusinessObject();
						Helper.MarkProcessed(inBondMoveHeader);
					}
					Helper.DeleteUnprocessedMovementsFor(headerBO, logger);
				}
				else
				{
					if (dataObject.InBondMoveHeaderCollection.Count != 1)
					{
						throw new DataObjectReadFailureException("InBondMoveHeaderCollection must have 1 element for Warehouse InBond movement");
					}
					FillWarehouseAddress(warehouseMovement);
					new CusInBondMoveHeaderDataObjectReader(dataObject.InBondMoveHeaderCollection[0], logger, Helper, headerBO).PopulateMainData(warehouseMovement);
				}
			}
		}

		void FillWarehouseAddress(CusInBondMoveHeader warehouseMovementBO)
		{
			if (dataObject.OrganizationAddressCollection != null)
			{
				var warehouseAddress = dataObject.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == nameof(DocAddressType.CustomsWarehouseAddress));
				if (warehouseAddress != null)
				{
					var reader = new OrganisationDataObjectReader(warehouseAddress, logger, factory);
					var orgAddress = reader.GetMatched();
					var moveHeaderRow = GetColumnIndexer(warehouseMovementBO);
					SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_OA_WarehouseAddress, orgAddress == null ? ZGuid.Empty : orgAddress.PK);
				}
			}
		}

		protected override void FillNotes(CusInBondHeader headerBO)
		{
			base.FillNotes(headerBO);
			if (dataObject.NoteCollection != null)
			{
				new NotesCollectionReader(dataObject.NoteCollection, logger, factory, headerBO).ReadIntoCollection();
			}
		}

		protected override DataTransfer.Universal.CusInBondBillDataObjectReader<CusInBondBill> InBondBillDataObjectReader(AdditionalBill additionalBillDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, DataTransfer.Universal.InBondDataObjectReaderHelper helper, CusInBondHeader headerBO)
		{
			return new CusInBondBillDataObjectReader(additionalBillDataObject, logger, factory, Helper, headerBO);
		}

		protected override DataTransfer.Universal.CusInBondMoveHeaderDataObjectReader<CusInBondMoveHeader, CusInBondMoveDetail, CusInBondContainer, CusInBondCargoDesc> InBondMoveHeaderDataObjectReader(UniversalCustoms.InBondMoveHeader inBondMoveHeaderDataObject, IXmlImportLogger logger, DataTransfer.Universal.InBondDataObjectReaderHelper helper, CusInBondHeader headerBO)
		{
			return new CusInBondMoveHeaderDataObjectReader(inBondMoveHeaderDataObject, logger, Helper, headerBO);
		}
	}
}
