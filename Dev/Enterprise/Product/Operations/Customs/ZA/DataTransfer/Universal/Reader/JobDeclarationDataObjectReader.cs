using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.ZA.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.ZA.DataTransfer.Universal
{
	public class JobDeclarationDataObjectReader : JobDeclarationDataObjectReader<JobDeclaration, Bill, CusContainer, JobComInvoiceGroupHeader>
	{
		internal JobDeclarationDataObjectReader(UniversalShipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment shipment = null)
			: base(declarationDataObject, logger, factory, shipment)
		{
		}

		protected new UniversalDataObjectReaderHelper Helper
		{
			get { return (UniversalDataObjectReaderHelper)base.Helper; }
		}

		protected override CommercialInvoiceHeaderDataObjectReader<JobComInvoiceGroupHeader> CreateNewCommercialInvoiceHeaderDataObjectReader(JobComInvoiceGroupHeader groupHeader, UniversalCustoms.CommercialInvoiceHeader invoiceData, UniversalShipment dataObject, ILandedCostDataReader landedCostDataReader)
		{
			return new CommercialInvoiceHeaderDataObjectReader(invoiceData, logger, Helper, groupHeader, dataObject, landedCostDataReader);
		}

		protected override AdditionalBillDataObjectReader<Bill> GetNewAdditionalBillDataObjectReader(AdditionalBill additionalBillDataObject, AdditionalBillDataProvider<Bill> additionalBillDataProvider, BillDetail primaryMasterBillDetail, BillDetail primaryHouseBillDetail)
		{
			return new AdditionalBillDataObjectReader<Bill>(additionalBillDataObject, logger, Helper, additionalBillDataProvider, primaryMasterBillDetail, primaryHouseBillDetail);
		}

		protected override CustomsContainerDataObjectReader<JobDeclaration, CusContainer> GetNewCustomsContainerDataObjectReader(Container containerDataObject, JobDeclaration declaration, ILandedCostDataReader landedCostDataReader)
		{
			return new CustomsContainerDataObjectReader<JobDeclaration, CusContainer>(containerDataObject, logger, Helper, declaration, landedCostDataReader);
		}

		protected override AddInfoDataObjectReader GetNewAddInfoDataObjectReaderForDeclaration(JobDeclaration declaration)
		{
			return new AddInfoDataObjectReader<JobDeclaration>(logger, Helper, JobDeclarationSchema.JE_AddInfo, ZAJobDeclarationSchema.Instance);
		}

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(JobDeclaration declaration)
		{
			var result = base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(declaration);

			if (declaration != null && declaration.ActiveEntryHeaders.OfType<CusEntryHeader>().Any(x => x.Messages.Count > 0))
			{
				result = ValidationConstants.Declaration.CustomsHasCommenced(declaration.JE_DeclarationReference);
			}
			return result;
		}

		protected override ZString? GetLocationAtClearanceCore(CodeDescriptionPair35Char locationAtClearance)
		{
			return locationAtClearance == null ? null : locationAtClearance.Code;
		}

		protected override void AddEntryInstructionFetchHint(Customs.Business.CusEntryInstruction instruction)
		{
			var query = GetRefCusProcedureQuery(instruction.CountryCode);
			query.AddToFilter(RefCusProcedureSchema.ZZ6_ProcedureCode, instruction.CEI_Style);
			query.AddToFilter(RefCusProcedureSchema.ZZ6_PreviousProcedureCode, string.Empty);
			factory.BOFactory.AddFetchHint(RefCusProcedureSchema.Instance, query);
		}

		protected override void AddFetchHintsRelatedToEntryInstruction(JobDeclaration declaration, List<UniversalCustoms.EntryInstruction> sourceCollection, UniversalShipment dataObject)
		{
			var commercialInfo = dataObject.CommercialInfo;
			if (commercialInfo.CommercialInvoiceCollection != null)
			{
				foreach (var invoiceData in commercialInfo.CommercialInvoiceCollection)
				{
					if (invoiceData.CommercialInvoiceLineCollection != null)
					{
						foreach (var invoiceLineData in invoiceData.CommercialInvoiceLineCollection)
						{
							var entryStyle = sourceCollection.Where(x => x.Link == invoiceLineData.EntryInstructionLink).Select(x => x.Style).FirstOrDefault();

							if (entryStyle.HasValue)
							{
								var query = GetRefCusProcedureQuery(declaration.CountryCode);
								query.AddToFilter(RefCusProcedureSchema.ZZ6_ProcedureCode, entryStyle);
								query.AddToFilter(RefCusProcedureSchema.ZZ6_PreviousProcedureCode, SQLComparisonOperator.NotEqual, string.Empty);
								query.AddToFilter(RefCusProcedureSchema.ZZ6_ShipmentType, SQLComparisonOperator.Contains, declaration.JE_MessageType);
								factory.BOFactory.AddFetchHint(RefCusProcedureSchema.Instance, query);
							}
						}
					}
				}
			}

			foreach (var instructionData in sourceCollection)
			{
				if (instructionData.Style.HasValue)
				{
					var query = GetRefCusProcedureQuery(declaration.CountryCode);
					query.AddToFilter(RefCusProcedureSchema.ZZ6_ProcedureCode, instructionData.Style);
					query.AddToFilter(RefCusProcedureSchema.ZZ6_PreviousProcedureCode, string.Empty);
					factory.BOFactory.AddFetchHint(RefCusProcedureSchema.Instance, query);
				}
			}
		}

		ZQuery GetRefCusProcedureQuery(ZString countryCode)
		{
			var query = new ZQuery(RefCusProcedureSchema.ZZ6_ZZZ_NKDataGrouping, countryCode);
			query.AddToFilter(RefCusProcedureSchema.ZZ6_StartDate, SQLComparisonOperator.LessThanOrEqualTo, ZDateTime.Today);
			query.AddToFilter(RefCusProcedureSchema.ZZ6_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.Today);
			return query;
		}
	}
}
