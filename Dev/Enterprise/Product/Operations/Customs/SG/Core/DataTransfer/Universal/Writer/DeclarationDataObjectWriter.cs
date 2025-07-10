using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.SG;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.Customs.DataTransfer.Universal;

namespace Enterprise.Customs.SG.V4.DataTransfer.Universal
{
	public class DeclarationDataObjectWriter : UniversalShipment.DeclarationDataObjectWriter
	{
		internal DeclarationDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected sealed override void AddTableFetchHintCreators(IExternalFetchHintSupporter externalFetchHintSupporter)
		{
			ExternalFetchHintSupporter = externalFetchHintSupporter;
			base.AddTableFetchHintCreators(externalFetchHintSupporter);
			var declarationPK = DeclarationRowForFetchHint.GetValue(JobDeclarationSchema.PK);
			externalFetchHintSupporter.AddFetchHint(new FetchHint(CusCodeDataSchema.CY_ParentID, declarationPK));
		}

		IExternalFetchHintSupporter ExternalFetchHintSupporter { get; set; }

		protected override IEnumerable<IFetchHint> GetJobComInvoiceLineRelatedFetchHints(IColumnIndexer row)
		{
			foreach (var fetchHint in base.GetJobComInvoiceLineRelatedFetchHints(row))
			{
				yield return fetchHint;
			}
			var invoiceLinePK = row.GetValue(JobComInvoiceLineSchema.PK);
			yield return new FetchHint(CusCodeDataSchema.CY_ParentID, invoiceLinePK);
			yield return new FetchHint(CusLineTariffDetailSchema.BZ_ParentID, invoiceLinePK);
			yield return new FetchHint(StmNoteSchema.ST_ParentID, invoiceLinePK);

			var tariff = row.GetValue(JobComInvoiceLineSchema.JI_Tariff);
			if (!tariff.IsEmpty && ExternalFetchHintSupporter is BusinessObjectFactory factory && factory != null)
			{
				yield return new ZQueryFetchHint(TariffViewSchema.Instance, TariffView.Loader.GetEffectiveTariffFilter(factory, Core.Constants.CountryCodes.Singapore, Customs.Universal.Constants.TariffTypes.HarmonizedSystem, tariff, ZDateTime.Today));
			}
		}

		protected override UniversalShipment.CommercialInvoiceHeaderDataObjectWriter GetNewCommercialInvoiceHeaderDataObjectWriter(Customs.Business.CusEntryHeader relatedEntry)
		{
			return new CommercialInvoiceHeaderDataObjectWriter(writeManager, helper, landedCostDataWriter, relatedEntry);
		}

		protected override void PopulateExtraOrganisation(BaseJobDeclaration declarationBO, Shipment declarationData, bool keepExistingData)
		{
			base.PopulateExtraOrganisation(declarationBO, declarationData, keepExistingData);
			if (declarationBO is JobDeclaration declaration && declaration.Shipment == null)
			{
				declarationData.AddOrgAddress(writeManager, declaration.Consignee, nameof(DocAddressType.ConsigneeDocumentaryAddress));
			}
		}

		protected override void PopulateCountrySpecificData(Shipment declarationData, BaseJobDeclaration declarationBO)
		{
			base.PopulateCountrySpecificData(declarationData, declarationBO);
			PopulateTradersRemarks(declarationData, declarationBO);
		}

		void PopulateTradersRemarks(Shipment declarationData, BaseJobDeclaration declarationBO)
		{
			if (declarationBO is JobDeclaration declaration && declaration.TradersRemarks.Any())
			{
				var customsReferences = declarationData.CustomsReferenceCollection ?? new List<CustomsReference>();
				foreach (TradersRemark remark in declaration.TradersRemarks)
				{
					var type = new CodeDescriptionPair() { Code = CusSupportingInfoTypeList.Codes.TradersRemarks, Description = CusSupportingInfoTypeList.Descriptions.TradersRemarks };
					var customsReference = new CustomsReference
					{
						Type = type,
						Reference = remark.CSI_Description,
						Order = remark.CSI_LineNo
					};
					customsReferences.Add(customsReference);
				}
				declarationData.SetCustomsReferenceCollection(() => customsReferences);
			}
		}
	}
}
