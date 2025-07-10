using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal.DataReaderExtensions;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
	public abstract class CusInBondHeaderDataObjectReader<THeader, TBill, TMoveHeader, TMoveDetail, TContainer, TCommodity> : ShipmentDataObjectReader<THeader>
		where THeader : Customs.Business.CusInBondHeader
		where TBill : Customs.Business.CusInBondBill
		where TMoveHeader : CusInBondMoveHeader
		where TMoveDetail : CusInBondMoveDetail
		where TContainer : Customs.Business.CusInBondContainer
		where TCommodity : Customs.Business.CusInBondCargoDesc
	{
		protected CusInBondHeaderDataObjectReader(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ICusInBondParent parentBO)
			: base(dataObject, logger, factory)
		{
			this.parentBO = parentBO;
		}
		readonly ICusInBondParent parentBO;

		protected abstract ZString CusInBondApplicationCode { get; }

		protected override IMatchingBusinessEntityFinder<THeader> GetCombinedReferenceMatcher()
		{
			return null; // No Combined Reference MAtching has been implemented for InBond. Considering we're looking at replacing this with Reference and Party ID matching, is best not to implement.
		}

		protected override THeader GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			THeader result = null;
			if (parentBO != null)
			{
				result = parentBO.GetInBondHeader(CusInBondApplicationCode) as THeader;
			}

			if (result == null)
			{
				result = GetMatchedExistingInBondHeader();
			}

			return result;
		}

		protected abstract THeader GetMatchedExistingInBondHeader();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		protected ZString[] InBondNumbers
		{
			get
			{
				if (inBondNumbers == null)
				{
					if (dataObject.InBondMoveHeaderCollection != null && dataObject.InBondMoveHeaderCollection.Count > 0)
					{
						inBondNumbers = dataObject.InBondMoveHeaderCollection.Where(i => i.EntryNumberCollection != null)
							.Select(x => x.EntryNumberCollection.FirstOrDefault(e => e.Type.GetCodeAsUpperCase() == Business.CusEntryHeaderMessageTypeList.Codes.InBond))
							.Where(n => n != null && !n.Number.GetValueOrDefault().IsEmpty)
							.Select(e => e.Number.Value).ToArray();
					}
					else
					{
						inBondNumbers = Array.Empty<ZString>();
					}
				}
				return inBondNumbers;
			}
		}
		ZString[] inBondNumbers;

		protected sealed override THeader GetNewBusinessObject()
		{
			THeader result = null;
			if (parentBO != null)
			{
				var mutex = new ZGlobalMutex(InBondMutexID, parentBO.PK.ToString() + GlbCompany.CurrentCompany.GC_RN_NKCountryCode.ToString());

				if (!mutex.Lock())
				{
					throw new InvalidOperationException(ZString.Format("Could not create a new {0}; someone else is already in the process of creating an {0} for {1} ({2}).", InBondDescription, parentBO.ParentType, parentBO.JobNumber));
				}
				else
				{
					factory.CleanupAfterSaving += (a, b) => { if (mutex.HasLock) { mutex.Unlock(); } };
				}
				result = factory.New<THeader>();
				result.BH_ParentTableCode = parentBO.TablePrefix;
				result.BH_ParentID = parentBO.PK;
			}
			else
			{
				result = base.GetNewBusinessObject();
			}

			return result;
		}

		protected abstract MutexID InBondMutexID { get; }
		protected abstract ZString InBondDescription { get; }

		protected override void PopulateBusinessObject(THeader headerBO)
		{
			if (CheckUpdateDataIsAllowed(headerBO))
			{
				PopulateBusinessObjectCore(headerBO, null);
			}
		}

		protected void PopulateBusinessObjectCore(THeader headerBO, TMoveHeader warehouseMoveHeader)
		{
			((ISupportDataImporting)headerBO).IsImportingData = true;
			var headerRow = GetColumnIndexer(headerBO);
			var delaySetters = IsDefaultingEnabled ? new Dictionary<string, ValueSetter>() : null;
			var isStandAlone = headerBO.BH_ParentID.IsEmpty;
			if (!isStandAlone && !headerRow.GetValue(CusInBondHeaderSchema.BH_OverrideFreightDefaults))
			{
				SetValue(headerRow, CusInBondHeaderSchema.BH_OverrideFreightDefaults, ZBool.True, delaySetters);
			}
			FillBranch(headerRow, delaySetters);
			SetValue(headerRow, CusInBondHeaderSchema.BH_LloydsNumber, dataObject.LloydsIMO, delaySetters);
			SetValue(headerRow, CusInBondHeaderSchema.BH_VoyageNumber, dataObject.VoyageFlightNo, delaySetters);
			SetValue(headerRow, CusInBondHeaderSchema.BH_ImportTransportMode, GetTransportMode(), delaySetters);
			SetValue(headerRow, CusInBondHeaderSchema.BH_ImportConveyanceName, dataObject.VesselName, delaySetters);
			SetValue(headerRow, CusInBondHeaderSchema.BH_ImportConveyanceCountry, dataObject.VesselCountryOfRegistration, delaySetters);
			FillDataFromAddInfos(headerRow, delaySetters);
			FillDates(headerRow, delaySetters);
			if (isStandAlone)
			{
				FillNotes(headerBO);
			}
			FillInBondSpecificData(headerRow, delaySetters, headerBO);
			delaySetters.SetValueInSpecificOrder(GetHeaderSettingOrder(headerBO));
			FillBills(headerBO);
			FillInBondMovements(headerBO, warehouseMoveHeader);
		}

		protected virtual ZString? GetTransportMode()
		{
			ZString? result = null;

			if (dataObject.TransportMode != null)
			{
				switch (dataObject.TransportMode.GetCodeAsUpperCase())
				{
					case Enterprise.Customs.US.Business.TransportTypeList.Codes.Air:
						result = Business.InBondTransportModeCodes.Codes.AirNonContainer;
						break;
					case Enterprise.Customs.US.Business.TransportTypeList.Codes.Rail:
						result = Business.InBondTransportModeCodes.Codes.RailNonContainer;
						break;
					case Enterprise.Customs.US.Business.TransportTypeList.Codes.Truck:
						result = Business.InBondTransportModeCodes.Codes.TruckNonContainer;
						break;
					case Enterprise.Customs.US.Business.TransportTypeList.Codes.Sea:
						var isContainerized = dataObject.CustomsContainerMode != null && dataObject.CustomsContainerMode.GetCodeAsUpperCase() == Enterprise.Customs.US.Business.ContainerModeList.Codes.Containerized;
						result = isContainerized ? Business.InBondTransportModeCodes.Codes.VesselContainer : Business.InBondTransportModeCodes.Codes.VesselNonContainer;
						break;
					case Enterprise.Customs.US.Business.TransportTypeList.Codes.FixedTransportInstallations:
						result = Business.InBondTransportModeCodes.Codes.FixedTransportInstallations;
						break;
				}
			}
			return result;
		}

		protected virtual IEnumerable<ZString> GetHeaderSettingOrder(THeader header)
		{
			return Array.Empty<ZString>();
		}

		void FillBranch(IColumnIndexer headerRow, Dictionary<string, ValueSetter> delaySetters)
		{
			ZGuid branchPK = dataObject.GetBranchPK(factory.BOFactory);
			if (branchPK.IsValid)
			{
				SetValue(headerRow, CusInBondHeaderSchema.BH_GB, branchPK, delaySetters);
			}
		}

		protected virtual void FillDataFromAddInfos(IColumnIndexer headerRow, Dictionary<string, ValueSetter> delaySetters)
		{
		}

		protected virtual void FillDates(IColumnIndexer headerRow, Dictionary<string, ValueSetter> delaySetters)
		{
		}

		protected virtual void FillNotes(THeader headerBO)
		{
		}

		protected virtual void FillBills(THeader headerBO)
		{
			if (dataObject.AdditionalBillCollection != null)
			{
				Helper.MarkUnprocessedExistingBillsFor(headerBO);
				foreach (var additionalBillDataObject in dataObject.AdditionalBillCollection)
				{
					var bill = InBondBillDataObjectReader(additionalBillDataObject, logger, factory, Helper, headerBO).ReadIntoBusinessObject();
					Helper.MarkProcessed(bill);
					Helper.CollectBillLink(bill, additionalBillDataObject);
				}
				Helper.DeleteUnprocessedBillsFor(headerBO, logger);
			}
		}

		protected virtual void FillInBondMovements(THeader headerBO, TMoveHeader warehouseMovement)
		{
			if (dataObject.InBondMoveHeaderCollection != null)
			{
				Helper.CollectContainerPackingLineAndCommercialInvoiceLineDetails(dataObject);
				if (warehouseMovement == null)
				{
					Helper.MarkUnprocessedExistingMovementsFor(headerBO);
					foreach (var inBondMoveHeaderDataObject in dataObject.InBondMoveHeaderCollection)
					{
						var inBondMoveHeader = InBondMoveHeaderDataObjectReader(inBondMoveHeaderDataObject, logger, Helper, headerBO).ReadIntoBusinessObject();
						Helper.MarkProcessed(inBondMoveHeader);
					}
					Helper.DeleteUnprocessedMovementsFor(headerBO, logger);
				}
			}
		}

		protected virtual void FillInBondSpecificData(IColumnIndexer headerRow, Dictionary<string, ValueSetter> delaySetters, THeader headerBO)
		{
		}

		public virtual bool CheckUpdateDataIsAllowed(THeader header)
		{
			var result = true;
			if (header.IsInDatabase && IsMessagingActive(header))
			{
				result = false;
				logger.LogBoth(LogType.Warning, Res.GetString("B4E5EFEE-F1D9-4079-99FA-43E455A5A5F1", "{0} data will not be updated as there is an active messaging.", header.HumanReadableName));
			}
			return result;
		}

		bool IsMessagingActive(THeader header)
		{
			var result = false;
			var inBondMoveHeaderQuery = new ZQuery(CusInBondMoveHeaderSchema.BM_BH, header.PK);
			foreach (var inBondMoveHeader in factory.Load<TMoveHeader>(inBondMoveHeaderQuery))
			{
				var messageQuery = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, inBondMoveHeader.PK);
				if (factory.LoadTop1<CBPEDIMessage>(messageQuery) != null)
				{
					result = true;
					break;
				}
			}
			return result;
		}

		protected InBondDataObjectReaderHelper Helper
		{
			get { return readerhelper ?? (readerhelper = GetInBondDataObjectReaderHelperCore()); }
		}
		InBondDataObjectReaderHelper readerhelper;

		protected virtual InBondDataObjectReaderHelper GetInBondDataObjectReaderHelperCore()
		{
			return new InBondDataObjectReaderHelper(factory);
		}

		protected abstract CusInBondBillDataObjectReader<TBill> InBondBillDataObjectReader(AdditionalBill additionalBillDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, InBondDataObjectReaderHelper helper, THeader headerBO);
		protected abstract CusInBondMoveHeaderDataObjectReader<TMoveHeader, TMoveDetail, TContainer, TCommodity> InBondMoveHeaderDataObjectReader(InBondMoveHeader inBondMoveHeaderDataObject, IXmlImportLogger logger, InBondDataObjectReaderHelper helper, THeader headerBO);
	}
}
