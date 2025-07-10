using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.Declaration.FetchStrategies
{
	public class JobDeclarationFetchStrategy : Customs.Business.FetchStrategies.BaseJobDeclarationFetchStrategy
	{
		public JobDeclarationFetchStrategy(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected new JobDeclaration BusinessObject
		{
			get { return (JobDeclaration)base.BusinessObject; }
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, BusinessObject.PK);
		}

		protected override void FetchForViewDeclaration(TableColumn[] columns)
		{
			base.FetchForViewDeclaration(columns);
			var pk = BusinessObject.PK;
			Factory.AddFetchHint(CusEntryHeaderSchema.CH_JE, pk);
		}

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			base.FetchForLoadChildEditableObjectsCore();
			Factory.AddFetchHint(CusAddInfoSchema.B7_ParentID, BusinessObject.PK);
			Factory.AddFetchHint(EDIMessageSchema.EM_LinkUniqueID, BusinessObject.PK);
		}

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();
			foreach (JobComInvoiceLine invoiceLine in BusinessObject.InvoiceLines)
			{
				var tariff = invoiceLine.JI_Tariff;
				if (!tariff.IsEmpty)
				{
					var tariffHint = UniversalTariffHelper.GetTariffFetchHint(Factory, tariff);
					Factory.AddFetchHint(tariffHint.table, tariffHint.query);

					if (tariff.Length == 14 && (BusinessObject.IsImport || BusinessObject.IsExport || BusinessObject.IsMiscellaneous))
					{
						foreach (int length in new int[] { 14, 10, 7, 4, 2 })
						{
							Factory.AddFetchHint(typeof(NZCTariffsPermitsApplyTo), NZCTariffsPermitsApplyToSchema.U6_TariffPortion, tariff.Left(length));
						}
					}
				}
			}
		}

		protected override void FetchForMergeCore()
		{
			dateForDutyRate = null;
			classificationQueries = new Dictionary<string, ZQuery>();
			base.FetchForMergeCore();
			classificationQueries = null;
			dateForDutyRate = null;
		}

		Dictionary<string, ZQuery> classificationQueries;

		ZDateTime DateForDutyRate
		{
			get
			{
				if (!dateForDutyRate.HasValue)
				{
					dateForDutyRate = BusinessObject.DateForDutyRate;
					if (dateForDutyRate.Value.IsEmpty)
					{
						dateForDutyRate = ZDateTime.Today;
					}
				}
				return dateForDutyRate.Value;
			}
		}
		ZDateTime? dateForDutyRate;

		protected override void AddMergeFetchHintsAfterInvoiceLines()
		{
			base.AddMergeFetchHintsAfterInvoiceLines();
			foreach (var pair in classificationQueries)
			{
				if (NZCustomsDataRegistry.Instance.UseRefDatabaseData.Value)
				{
					foreach (var tariff in Factory.Load<TariffView>(pair.Value))
					{
						var levyHint = UniversalTariffHelper.GetLevyRateFetchHint(Factory, tariff.PK, DateForDutyRate);
						if (levyHint.query != null)
						{
							Factory.AddFetchHint(levyHint.table, levyHint.query);
						}
					}
				}
				else
				{
					foreach (var classification in Factory.Load<NZCClassification>(pair.Value))
					{
						Factory.AddFetchHint(NZCClassificationLevyRateSchema.Instance, NZCClassificationLevyRate.GetLevyRatesQuery(classification.PK, DateForDutyRate));
					}
				}
			}
		}

		protected override void AddMergeFetchHintsFor(Customs.Business.BaseJobComInvoiceLine invoiceLine)
		{
			base.AddMergeFetchHintsFor(invoiceLine);
			var tariff = invoiceLine.JI_Tariff;
			if (!tariff.IsEmpty)
			{
				var tariffHintWithDate = UniversalTariffHelper.GetTariffFetchHint(Factory, tariff, DateForDutyRate);
				var classificationQuery = tariffHintWithDate.query;
				var key = classificationQuery.LiteralTextADO;
				if (!classificationQueries.ContainsKey(key))
				{
					classificationQueries.Add(key, classificationQuery);
					Factory.AddFetchHint(tariffHintWithDate.table, classificationQuery);
				}

				var tariffHint = UniversalTariffHelper.GetTariffFetchHint(Factory, tariff);
				Factory.AddFetchHint(tariffHint.table, tariffHint.query);

				if (tariff.Length == 14 && (BusinessObject.IsImport || BusinessObject.IsExport || BusinessObject.IsMiscellaneous))
				{
					foreach (int length in new int[] { 14, 10, 7, 4, 2 })
					{
						Factory.AddFetchHint(typeof(NZCTariffsPermitsApplyTo), NZCTariffsPermitsApplyToSchema.U6_TariffPortion, tariff.Left(length));
					}
				}
			}
		}
	}
}
