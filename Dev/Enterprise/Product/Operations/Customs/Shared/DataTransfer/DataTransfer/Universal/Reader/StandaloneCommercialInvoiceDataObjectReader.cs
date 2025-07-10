using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal.DataReaderExtensions;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class StandaloneCommercialInvoiceDataObjectReader : StandaloneCommercialInvoiceDataObjectReader<BaseJobComInvoiceHeader, BaseJobComInvoiceGroupHeader>
	{
		public StandaloneCommercialInvoiceDataObjectReader(UniversalShipment shipmentDataObject, UniversalCustoms.CommercialInvoiceHeader invoiceHeaderDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(shipmentDataObject, invoiceHeaderDataObject, logger, factory)
		{
		}

		protected override UniversalDataObjectReaderHelper CreateNewUniversalDataObjectReaderHelper()
		{
			return new UniversalDataObjectReaderHelper(factory, dataObject.GetTargetCountryCode(), dataObject.GetSourceCountryCode(), dataObject.GetDataProviderForCodeMapping());
		}

		protected override CommercialInvoiceHeaderDataObjectReader<BaseJobComInvoiceGroupHeader> CreateNewCommercialInvoiceHeaderDataObjectReader(BaseJobComInvoiceGroupHeader groupHeader, UniversalCustoms.CommercialInvoiceHeader invoiceData)
		{
			return new CommercialInvoiceHeaderDataObjectReader<BaseJobComInvoiceGroupHeader>(invoiceData, logger, Helper, groupHeader, invoiceType: typeof(BaseJobComInvoiceHeader));
		}
	}

	public abstract class StandaloneCommercialInvoiceDataObjectReader<TInvoiceHeader, TInvoiceGroupHeader> : ShipmentDataObjectReader<TInvoiceHeader>, IOrganisationDataObjectReaderSupporter, ITopLevelDataObjectReader, INotifications
		where TInvoiceHeader : BaseJobComInvoiceHeader
		where TInvoiceGroupHeader : BaseJobComInvoiceGroupHeader
	{
		protected StandaloneCommercialInvoiceDataObjectReader(UniversalShipment shipmentDataObject, UniversalCustoms.CommercialInvoiceHeader invoiceHeaderDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(shipmentDataObject, logger, factory)
		{
			this.invoiceHeaderDataObject = Argument.NotNull(invoiceHeaderDataObject, "invoiceHeaderDataObject");
		}

		public override DataContextType DataContextType
		{
			get { return DataContextType.CustomsCommercialInvoice; }
		}

		protected bool IsForGlobalTradeManagement
		{
			get
			{
				if (!isForGlobalTradeManagement.HasValue)
				{
					gtmRecipientRoleDataObject = dataObject.DataContext is IDataContextDataObject dataContext
						&& dataContext.DataTargetCollection is IEnumerable<IDataTargetDataObject> dataTargetCollection
						&& dataTargetCollection.Any(dt => dt.Type?.Equals(nameof(DataContextType.CustomsCommercialInvoice)) ?? false)
						&& dataContext.RecipientRoleCollection is IEnumerable<IRecipientRoleDataObject> recipientRoleCollection ?
						recipientRoleCollection.FirstOrDefault(rr => rr.Code.HasValue && rr.Code == RecipientRoleType.GTM) :
						null;
					isForGlobalTradeManagement = gtmRecipientRoleDataObject != null;
				}
				return isForGlobalTradeManagement.Value;
			}
		}
		bool? isForGlobalTradeManagement;
		IRecipientRoleDataObject gtmRecipientRoleDataObject;

		protected readonly UniversalCustoms.CommercialInvoiceHeader invoiceHeaderDataObject;

		protected UniversalDataObjectReaderHelper Helper
		{
			get { return helper ?? (helper = CreateNewUniversalDataObjectReaderHelper()); }
		}
		UniversalDataObjectReaderHelper helper;

		protected abstract UniversalDataObjectReaderHelper CreateNewUniversalDataObjectReaderHelper();

		protected override IMatchingBusinessEntityFinder<TInvoiceHeader> GetCombinedReferenceMatcher()
		{
			return null; // No Combined Reference MAtching has been implemented for Customs. Considering we're looking at replacing this with Reference and Party ID matching, is best not to implement.
		}

		protected OrgHeader Supplier
		{
			get
			{
				MatchSupplierAndBuyer();
				return supplier;
			}
		}
		OrgHeader supplier;

		protected OrgHeader Buyer
		{
			get
			{
				MatchSupplierAndBuyer();
				return buyer;
			}
		}
		OrgHeader buyer;

		void MatchSupplierAndBuyer()
		{
			if (!hasMatchMatchSupplierAndBuyerRun)
			{
				hasMatchMatchSupplierAndBuyerRun = true;
				buyer = GetMatchedBuyer();
				supplier = GetMatchedSupplier();
			}
		}
		bool hasMatchMatchSupplierAndBuyerRun;

		protected virtual OrgHeader GetMatchedSupplier()
		{
			OrgHeader result = null;
			var supplierData = invoiceHeaderDataObject.Supplier ?? invoiceHeaderDataObject.OrganizationAddressCollection.FindBestSupplierMatch();
			if (supplierData != null)
			{
				var supplierAddress = new OrganisationDataObjectReader(supplierData, logger, factory).GetMatched();
				result = supplierAddress == null ? null : supplierAddress.Header;
			}
			return result;
		}

		protected virtual OrgHeader GetMatchedBuyer()
		{
			OrgHeader result = null;
			var buyerData = invoiceHeaderDataObject.Buyer ?? invoiceHeaderDataObject.OrganizationAddressCollection.FindBestImporterMatch();
			if (buyerData != null)
			{
				var buyerAddress = new OrganisationDataObjectReader(buyerData, logger, factory).GetMatched();
				result = buyerAddress == null ? null : buyerAddress.Header;
			}
			return result;
		}

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(TInvoiceHeader targetBO)
		{
			var builder = new ZStringBuilder();
			builder.AppendIfNotEmpty(SupplierErrorMessage);
			builder.AppendIfNotEmpty(BuyerErrorMessage);
			if (invoiceHeaderDataObject.InvoiceNumber.GetValueOrDefault().IsEmpty)
			{
				builder.Append(Res.GetString("79FCA2A1-67EF-49FA-A482-2930058BE330", "{0} must not be empty.", "InvoiceNumber"));
			}
			return builder.ToStringWithNewLineBetweenAppends();
		}

		protected virtual ZString BuyerErrorMessage
		{
			get { return Buyer != null ? ZString.Empty : GetValidOrganisatioMessage((NoResString)"Buyer"); }
		}

		protected ZString GetValidOrganisatioMessage(ZString organisationType)
		{
			return Res.GetString("2114820B-714C-464B-A527-72A71D9D060A", "{0} is required for Standalone Commercial Invoice; no valid {0} was found.", organisationType);
		}

		protected virtual ZString SupplierErrorMessage
		{
			get { return Supplier != null ? ZString.Empty : GetValidOrganisatioMessage((NoResString)"Supplier"); }
		}

		protected override TInvoiceHeader GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			TInvoiceHeader result = null;
			var invoiceNumber = invoiceHeaderDataObject.InvoiceNumber.GetValueOrDefault();
			if (Supplier != null && Buyer != null && !invoiceNumber.IsEmpty)
			{
				var query = new ZQuery(JobComInvoiceHeaderSchema.JZ_JE, null);
				query.AddToFilter(JobComInvoiceHeaderSchema.JZ_OH_Supplier, Supplier.PK);
				query.AddToFilter(JobComInvoiceHeaderSchema.JZ_OH_Buyer, Buyer.PK);
				query.AddToFilter(JobComInvoiceHeaderSchema.JZ_InvoiceNumber, invoiceNumber);
				query.AddToFilter(JobComInvoiceHeaderSchema.JZ_GroupInvoice, ZBool.False);

				result = GetExistingMatchingAnyBranch(query);
			}
			return result;
		}

		TInvoiceHeader GetExistingMatchingAnyBranch(ZQuery invoiceQuery)
		{
			// Universal Architecture has already setup the correct environment
			var declarationBranchQuery = new ZQuery(JobComInvoiceHeaderSchema.JZ_GB, GlbCompany.CurrentCompany.Branches.GetPKs());
			declarationBranchQuery.AddToFilter(invoiceQuery);
			declarationBranchQuery.OrderBy = JobComInvoiceHeaderSchema.Constants.JZ_InvoiceDate + " desc";
			return factory.LoadTop1<TInvoiceHeader>(declarationBranchQuery);
		}

		protected override void PopulateBusinessObject(TInvoiceHeader targetBO)
		{
			currentInvoiceBO = null;
			using (UnitConverter.TemporarySetupCachedConvertion(factory.BOFactory))
			{
				var fakeDeclaration = new FakeDeclarationCreatorForInvoice(targetBO);
				var headerData = fakeDeclaration.HeaderData;
				headerData.JE_AutoWeightApportion = false;
				var declaration = headerData as BaseJobDeclaration;
				var topGroupInvoice = declaration?.TopGroupInvoice as TInvoiceGroupHeader;
				var reader = CreateNewCommercialInvoiceHeaderDataObjectReader(topGroupInvoice, invoiceHeaderDataObject);
				BaseJobComInvoiceHeader invoiceBO = targetBO;
				SetValue(invoiceBO, JobComInvoiceHeaderSchema.JZ_OH_Supplier, Supplier.PK);
				using (invoiceBO.SetterSuspender.SuspendEnforceSetting(BaseJobComInvoiceHeader.Schema.JZ_OH_Supplier))
				{
					SetValue(invoiceBO, JobComInvoiceHeaderSchema.JZ_OH_Buyer, Buyer.PK);
					using (invoiceBO.SetterSuspender.SuspendEnforceSetting(GetBuyerFieldsThatNeedToBeSuspend()))
					{
						var messageType = dataObject.MessageType.GetNullableCodeAsUpperCase() ?? headerData.JE_MessageType;
						if (declaration != null)
						{
							SetValue(declaration, JobDeclarationSchema.JE_MessageType, messageType);
						}
						invoiceBO.JZ_MessageType = messageType;
						using (invoiceBO.SetterSuspender.SuspendEnforceSetting(BaseJobComInvoiceHeader.Schema.JZ_MessageType))
						{
							reader.ReadIntoBusinessObject(ref invoiceBO, new CommercialInvoiceHeaderRelatedData(dataObject));
						}
					}
				}

				if (IsForGlobalTradeManagement)
				{
					if (gtmRecipientRoleDataObject.ServiceCode.HasValue && gtmRecipientRoleDataObject.ServiceCode.Value == ServiceCodeType.UCK)
					{
						((ICustomsFileParent)targetBO).UnlockFile(MessageRecipientPartyTypeList.Codes.GlobalTradeManagement);
					}
					else
					{
						((ICustomsFileParent)targetBO).LockFile(MessageRecipientPartyTypeList.Codes.GlobalTradeManagement);
						targetBO.Factory.Saved -= ExportCommercialInvoiceUXMLToeHubOnFactorySaved;
						targetBO.Factory.Saved += ExportCommercialInvoiceUXMLToeHubOnFactorySaved;
						currentInvoiceBO = targetBO;
					}
				}
			}
		}
		TInvoiceHeader currentInvoiceBO;

		void ExportCommercialInvoiceUXMLToeHubOnFactorySaved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			factory.Saved -= ExportCommercialInvoiceUXMLToeHubOnFactorySaved;
			if (savedSuccessfully && currentInvoiceBO != null)
			{
				string eHubIdForGtm = new RefSysConfig.Loader(factory).GetStringValue(RefSysConfigCodeGtm);
				if (string.IsNullOrEmpty(eHubIdForGtm))
				{
					logger.Log(Integration.LogType.Warning, "Unable to export Commercial Invoice to eHub for Global Trade Management as eHub ID configuration is missing.");
					return;
				}
				var manager = (IShipmentDataContextManager)currentInvoiceBO.GetUniversalDataContextManager();
				var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, currentInvoiceBO)));
				using (((IExternalFetchHintSupporter)factory).SetupCreator())
				{
					var dataObject = (UniversalShipment)writer.GetDataObject(currentInvoiceBO);
					var processor = ObjectFactory.New<IUniversalShipmentXmlWriter>(writer, currentInvoiceBO, eHubIdForGtm, string.Empty);
					processor.Process(this, CancellationToken.None);
				}
			}
		}

		void INotifications.Add(INotification notification)
		{
			logger.Log(Integration.LogType.Information, notification.Message);
		}

		ZString[] GetBuyerFieldsThatNeedToBeSuspend()
		{
			var result = new List<ZString>();
			result.Add(BaseJobComInvoiceHeader.Schema.JZ_OH_Buyer);
			if (ShouldJZ_OH_BuyerAndJZ_OA_BuyerAddressBelongingToSameOrganization)
			{
				result.Add(BaseJobComInvoiceHeader.Schema.JZ_OA_BuyerAddress);
			}
			return result.ToArray();
		}
		protected virtual bool ShouldJZ_OH_BuyerAndJZ_OA_BuyerAddressBelongingToSameOrganization => true;

		protected abstract CommercialInvoiceHeaderDataObjectReader<TInvoiceGroupHeader> CreateNewCommercialInvoiceHeaderDataObjectReader(TInvoiceGroupHeader groupHeader, UniversalCustoms.CommercialInvoiceHeader invoiceData);

		#region IOrganisationDataObjectReaderSupporter Members

		OrganisationDataObjectReader IOrganisationDataObjectReaderSupporter.CreateNewReader(OrganizationAddress addressData)
		{
			return new OrganisationDataObjectReader(addressData, logger, factory);
		}

		public const string RefSysConfigCodeGtm = "GTMMBX";

		#endregion
	}
}
