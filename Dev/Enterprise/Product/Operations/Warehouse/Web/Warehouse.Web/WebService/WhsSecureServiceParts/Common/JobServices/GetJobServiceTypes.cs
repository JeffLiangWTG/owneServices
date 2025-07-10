using System;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region GetJobServiceTypes

		[WebMethod(Description = "Gets Job Service types")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public JobServiceTypesWebServiceResponse GetJobServiceTypes(Guid jobPK, JobServiceSupporterStrategy jobServiceSupporterStrategy, string clientCode)
		{
			return HandleWebServiceRequest<JobServiceTypesWebServiceResponse>(r => GetJobServiceTypesCore(r, jobPK, jobServiceSupporterStrategy, clientCode));
		}

		void GetJobServiceTypesCore(JobServiceTypesWebServiceResponse response, Guid jobPK, JobServiceSupporterStrategy jobServiceSupporterStrategy, string clientCode)
		{
			response.JobServiceTypes = LoadValidJobServiceTypes(clientCode, jobPK, jobServiceSupporterStrategy);
		}

		JobServiceTypesInfoCollection LoadValidJobServiceTypes(string clientCode, Guid jobPK, JobServiceSupporterStrategy jobServiceSupporterStrategy)
		{
			var jobServiceSupporter = WebServiceHelper.GetJobServiceSupporter(Factory, jobPK, jobServiceSupporterStrategy);
			var codeDescriptionPairInfos = new JobServiceTypesInfoCollection();
			var jobService = Factory.New<WhsJobService>(); // To ensure we use same List as we do in Enterprise

			try
			{
				var jobServiceLookups = new WhsJobServiceLookups(jobService);
				var jobServicesFromRegistry = jobServiceLookups.JobServiceType_List.ToArray();

				#region Load Codes with Client Rates Set Up

				var clientQuery = new ZDBOnlySubQuery(typeof(OrgHeader), RatingHeaderSchema.TH_OH);
				clientQuery.AddToFilter(OrgHeaderSchema.OH_Code, clientCode);

				var ratingHeaderQuery = new ZDBOnlySubQuery(typeof(RatingHeader), RateEntrySchema.TI_TH);
				ratingHeaderQuery.AddSubQuery(clientQuery, JoinCondition.And);

				var startDateQuery = new ZQuery();
				startDateQuery.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_RateStartDate, SQLComparisonOperator.LessThanOrEqualTo, ZDateTime.Now);

				var endDateQuery = new ZQuery(RateEntrySchema.TI_RateEndDate, null);
				endDateQuery.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_RateEndDate, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.Now);

				var rateEntryQuery = new ZDBOnlySubQuery(typeof(RateEntry), RateLinesSchema.TL_TI);
				rateEntryQuery.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_ParentID, null);
				rateEntryQuery.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_ParentID, WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode).PK);
				rateEntryQuery.AddToFilter(startDateQuery, JoinCondition.And);
				rateEntryQuery.AddToFilter(endDateQuery, JoinCondition.And);
				rateEntryQuery.AddSubQuery(ratingHeaderQuery, JoinCondition.And);

				var rateLineQuery = new ZDBOnlySubQuery(typeof(RateLine), RateLinesSchema.TL_AC);
				rateLineQuery.AddSubQuery(rateEntryQuery, JoinCondition.And);

				var chargeCodeQuery = new ZDBOnlyQuery(typeof(AccChargeCode));
				chargeCodeQuery.AddToFilter(AccChargeCodeSchema.AC_ChargeGroup, GetChargeGroupFromJobServiceSupporterStrategy(jobServiceSupporterStrategy));
				chargeCodeQuery.AddSubQuery(rateLineQuery, JoinCondition.And);

				var chargeSubGroupsWithClientRatesSetUp = Factory.Load<AccChargeCode>(chargeCodeQuery).Select(c => c.AC_ChargeSubGroup);

				#endregion

				foreach (var jobServiceType in jobServicesFromRegistry)
				{
					var serviceCode = chargeSubGroupsWithClientRatesSetUp.FirstOrDefault(j => j.EqualsIgnoringCase(jobServiceType.Code));
					if (serviceCode != ZString.Empty)
					{
						var existingJobService = jobServiceSupporter.Services.GetServices(serviceCode).FirstOrDefault();
						codeDescriptionPairInfos.Add(new JobServiceTypesInfo(jobServiceType) { ExistingJobService = existingJobService != null ? new WhsJobServiceInfo(existingJobService) : null });
					}
				}
			}
			finally
			{
				jobService.Delete();
			}

			return codeDescriptionPairInfos;
		}

		#region GetChargeGroupFromJobServiceSupporterStrategy

		string GetChargeGroupFromJobServiceSupporterStrategy(JobServiceSupporterStrategy jobServiceSupporterStrategy)
		{
			switch (jobServiceSupporterStrategy)
			{
				case JobServiceSupporterStrategy.WhsReceive:
					return JobInvoicingConsumerTypes.WarehouseInwards.Code;
				case JobServiceSupporterStrategy.WhsPickLine:
					return JobInvoicingConsumerTypes.WarehouseOutwards.Code;
				default:
					return "";
			}
		}

		#endregion

		#endregion
	}
}
