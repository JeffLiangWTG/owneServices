using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public sealed class CommercialInfoMatcher
	{
		public CommercialInfoMatcher(BaseJobDeclaration declaration, IXmlImportLogger logger, UniversalObjectFactory factory, CommercialInfo commercialInfo, CollectionContent? commercialInvoiceCollectionContent)
		{
			this.declaration = declaration;
			this.logger = logger;
			this.factory = factory;
			this.commercialInfo = commercialInfo;
			this.isCommercialInvoiceCollectionContentPartial = commercialInvoiceCollectionContent.GetValueOrDefault() == CollectionContent.Partial;
		}

		public bool HasMatchedResult => MatchedInvoices.Any();

		public IEnumerable<BaseJobComInvoiceHeader> GetInvoices()
		{
			return MatchedInvoices.Values;
		}

		public BaseJobComInvoiceHeader GetInvoice(CommercialInvoiceHeader invoiceHeaderDataObject)
		{
			BaseJobComInvoiceHeader result = invoiceHeaderDataObject != null && MatchedInvoices.TryGetValue(invoiceHeaderDataObject, out result)
				? result
				: null;

			return result;
		}

		public bool TryGetMatchedSupplier(OrganizationAddress address, out OrgAddress matchedAddress)
		{
			return MatchedSuppliers.TryGetValue(address, out matchedAddress);
		}

		#region Implement

		BaseJobComInvoiceHeader GetMatchedInvoiceHeader(CommercialInvoiceHeader invoiceDataObject, BaseJobComInvoiceGroupHeader groupHeader)
		{
			var invoiceNumber = invoiceDataObject.InvoiceNumber.GetValueOrDefault();

			var query = new ZQuery(JobComInvoiceHeaderSchema.JZ_JE, declaration.PK);
			query.AddToFilter(JobComInvoiceHeaderSchema.JZ_GroupInvoice, ZBool.False);
			query.AddToFilter(JobComInvoiceHeaderSchema.JZ_InvoiceNumber, invoiceNumber);
			query.FetchOnlyFromLocalCache = !declaration.IsInDatabase;
			query.OrderBy = JobComInvoiceHeaderSchema.Constants.JZ_SystemCreateTimeUtc;

			var matchInvoiceHeaders = declaration.Factory.Load<BaseJobComInvoiceHeader>(query).Where(x => !matchedCommercialInvoiceHeader.ContainsKey(x)).ToArray();
			BaseJobComInvoiceHeader result = null;
			if (matchInvoiceHeaders.Length == 1)
			{
				result = matchInvoiceHeaders[0];
			}
			else
			{
				result = matchInvoiceHeaders.FirstOrDefault(c => IsInvoiceMatchedFromAddress(c, invoiceDataObject.Supplier));
				if (result == null && groupHeader != null)
				{
					result = matchInvoiceHeaders.FirstOrDefault(c => c.JZ_JZ_GroupInvoiceFK == groupHeader.PK);
				}

				if (result == null)
				{
					result = matchInvoiceHeaders.FirstOrDefault();
				}
			}

			if (result != null)
			{
				matchedCommercialInvoiceHeader.Add(result, invoiceDataObject);
			}
			return result;
		}

		void PopulateInvoices(CommercialInfo info, BaseJobComInvoiceGroupHeader groupHeader)
		{
			var invoiceDataObjects = info.CommercialInvoiceCollection;

			if (invoiceDataObjects != null)
			{
				foreach (var invoiceDataObject in invoiceDataObjects)
				{
					if (isCommercialInvoiceCollectionContentPartial || (invoiceDataObject.CommercialInvoiceLineCollection?.Content).HasValue)
					{
						var bestMatchedInvoice = GetMatchedInvoiceHeader(invoiceDataObject, groupHeader);
						if (bestMatchedInvoice != null)
						{
							matchedInvoices.Add(invoiceDataObject, bestMatchedInvoice);
						}
					}
				}
			}

			var subGroupInfos = info.SubGroupCollection;

			if (groupHeader != null && subGroupInfos != null)
			{
				foreach (var subGroupInfo in subGroupInfos)
				{
					var subInvoiceDataObjects = subGroupInfo?.CommercialInvoiceCollection;

					if (subInvoiceDataObjects != null && subInvoiceDataObjects.Any())
					{
						var subGroup = groupHeader
							.JobComInvoiceGroupHeaders
							.Find(new ZQuery(JobComInvoiceHeaderSchema.JZ_InvoiceNumber, subGroupInfo?.Name.GetValueOrDefault()))
							.FirstOrDefault() as BaseJobComInvoiceGroupHeader;

						PopulateInvoices(subGroupInfo, subGroup);
					}
				}
			}
		}

		bool IsInvoiceMatchedFromAddress(BaseJobComInvoiceHeader invoice, OrganizationAddress address)
		{
			var result = true;

			if (address != null)
			{
				if (!MatchedSuppliers.TryGetValue(address, out var matchedAddress))
				{
					matchedAddress = new OrganisationDataObjectReader(address, logger, factory).GetMatched(invoice, OrganisationTypes.Consignor);
					MatchedSuppliers.Add(address, matchedAddress);
				}
				result = matchedAddress == null || matchedAddress.OA_OH == invoice.JZ_OH_Supplier;
			}

			return result;
		}

		Dictionary<CommercialInvoiceHeader, BaseJobComInvoiceHeader> MatchedInvoices
		{
			get
			{
				if (matchedInvoices == null)
				{
					matchedInvoices = new Dictionary<CommercialInvoiceHeader, BaseJobComInvoiceHeader>();
					matchedCommercialInvoiceHeader = new Dictionary<BaseJobComInvoiceHeader, CommercialInvoiceHeader>();
					PopulateInvoices(commercialInfo, declaration.TopGroupInvoice);
				}

				return matchedInvoices;
			}
		}
		Dictionary<CommercialInvoiceHeader, BaseJobComInvoiceHeader> matchedInvoices;
		Dictionary<BaseJobComInvoiceHeader, CommercialInvoiceHeader> matchedCommercialInvoiceHeader;

		Dictionary<OrganizationAddress, OrgAddress> MatchedSuppliers => matchedSuppliers ?? (matchedSuppliers = new Dictionary<OrganizationAddress, OrgAddress>());
		Dictionary<OrganizationAddress, OrgAddress> matchedSuppliers;

		readonly BaseJobDeclaration declaration;
		readonly CommercialInfo commercialInfo;
		readonly IXmlImportLogger logger;
		readonly UniversalObjectFactory factory;
		readonly bool isCommercialInvoiceCollectionContentPartial;

		#endregion
	}
}
