using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public class AirBookingResponseConsolCostingImporter
	{
		public AirBookingResponseConsolCostingImporter(IXmlImportLogger logger, BusinessObjectFactory factory)
		{
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
			this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
		}

		readonly IXmlImportLogger logger;
		readonly BusinessObjectFactory factory;

		public string[] ImportConsolCosting(ConsolCosts consolCostsData, ForwardingConsol consol)
		{
			if (consol == null
				|| consolCostsData?.ConsolCostLineCollection == null
				|| consolCostsData.ConsolCostLineCollection.Count == 0)
			{
				return Array.Empty<string>();
			}

			var messagesForUser = new List<string>();
			var loadInConsolCosting = true;

			if (consol.IsGateway())
			{
				var companyOrgProxiesGuids = GlbCompany.CurrentCompany.GetAllOrgProxiesIncludingBranches().ToList();

				if (consol.IsSendingAgentGTT() && companyOrgProxiesGuids.Contains(consol.SendingForwarderPK))
				{
					loadInConsolCosting = false;
				}
				else if (consol.IsReceivingAgentGTA() && companyOrgProxiesGuids.Contains(consol.ReceivingForwarderPK))
				{
					loadInConsolCosting = false;
				}
			}

			Func<AccChargeCode, ConsolCostLine, string> addChargeFunc;
			Action criticalValidationFunc;

			if (loadInConsolCosting)
			{
				var apportionmentListing = new ApportionmentListing(factory, consol);
				var createdCosts = new List<JobConsolCost>();

				addChargeFunc = (chargeCodeBizO, costLine) => AddConsolCost(chargeCodeBizO, costLine, apportionmentListing, consol, createdCosts);
				criticalValidationFunc = () => RunCriticalValidation(logger, createdCosts, messagesForUser);
			}
			else
			{
				Job billingJob = null;
				try
				{
					billingJob = LoadOrCreateJob(consol);
					if (billingJob == null)
					{
						var message = Res.GetString("8140fe7d-bbd4-45e8-bd7d-ffae5aaec891",
							"Please make sure the Consol gateway billing is enabled.");
						logger?.Log(LogType.Error, message);
						return new[] { message };
					}
				}
				catch (JobCreationException)
				{
					var message = Res.GetString("8140fe7a-bbd4-45e8-bd7d-ffae5baec822", "Could not create job.");
					logger?.Log(LogType.Error, message);
					messagesForUser.Add(message);
					return messagesForUser.ToArray();
				}

				var createdJobCharges = new List<Charge>();

				addChargeFunc = (chargeCodeBizO, costLine) => AddJobCharge(chargeCodeBizO, costLine, consol, billingJob, createdJobCharges);
				criticalValidationFunc = () => RunCriticalValidation(logger, createdJobCharges, messagesForUser);
			}

			var universalMapping = new Dictionary<string, ZGuid>();

			foreach (var costLine in consolCostsData.ConsolCostLineCollection)
			{
				var chargeCodeBizO = GetChargeCode(costLine, universalMapping);
				if (chargeCodeBizO == null)
				{
					var message = Res.GetString("72970729-1be8-49eb-881f-04639e3e9cd6", "Could not find matched charge code of: {0}.", costLine.ChargeCode?.Code);

					logger?.Log(LogType.Warning, message);
					messagesForUser.Add(message);
					continue;
				}

				var errorMessage = addChargeFunc.Invoke(chargeCodeBizO, costLine);
				if (!string.IsNullOrEmpty(errorMessage))
				{
					messagesForUser.Add(errorMessage);
				}
			}

			criticalValidationFunc.Invoke();

			return messagesForUser.ToArray();
		}

		internal virtual Job LoadOrCreateJob(ForwardingConsol consol)
		{
			return consol.Job as Job ?? new JobHeader.Loader(consol).TryLoadOrCreateWithMutex() as Job;
		}

		internal virtual JobConsolCost AddApportionment(ApportionmentListing apportionmentListing)
		{
			return apportionmentListing.CostsCollection.TryAddNew();
		}

		AccChargeCode GetChargeCode(ConsolCostLine costLine, Dictionary<string, ZGuid> universalMapping)
		{
			AccChargeCode result = null;
			var uc = costLine.UniversalChargeCode?.Code ?? string.Empty;

			if (!string.IsNullOrWhiteSpace(uc) && !universalMapping.TryGetValue(uc, out _))
			{
				universalMapping[uc] = ZGuid.Empty;
				result = WiseRatesConverter.ConvertChargeCode(uc, factory).chargeCode;
				if (result != null)
				{
					universalMapping[uc] = result.PK;
				}
			}

			// When there is no mapping, try to get the charge code from the charge code's code
			return result ?? GetChargeCodeFromCode(costLine.ChargeCode?.Code ?? ZString.Empty);
		}

		AccChargeCode GetChargeCodeFromCode(string code)
		{
			if (string.IsNullOrWhiteSpace(code))
			{
				return null;
			}

			const string freightChargeCode = "FRT";

			if (code.Equals(freightChargeCode, StringComparison.OrdinalIgnoreCase))
			{
				return factory.Load<AccChargeCode>(Env.Registry.FreightChargeCode);
			}
			else
			{
				var query = new ZQuery(AccChargeCodeSchema.AC_GC, Env.CurrentCompanyPK);
				query.AddToFilter(AccChargeCodeSchema.AC_Code, code);
				return factory.LoadTop1<AccChargeCode>(query);
			}
		}

		string AddConsolCost(
			AccChargeCode chargeCode,
			ConsolCostLine costLine,
			ApportionmentListing apportionmentListing,
			ForwardingConsol consol,
			List<JobConsolCost> createdCosts)
		{
			JobConsolCost consolCost = null;
			try
			{
				consolCost = AddApportionment(apportionmentListing);
			}
			catch (JobCreationException)
			{
			}

			if (consolCost == null)
			{
				var message = Res.GetString("d3813eb7-56df-4cd2-98b3-82baba70b8a0", "Could not import consol costing.");
				logger?.Log(LogType.Warning, message);
				return message;
			}

			consolCost.E6_AC_ChargeCode = chargeCode.PK;
			consolCost.E6_CostReference = costLine.SupplierReference.GetValueOrDefault();
			consolCost.E6_OSCostAmount = costLine.CostOSAmount.GetValueOrDefault();
			consolCost.E6_RX_NKCurrency = costLine.CostOSCurrency?.Code ?? ZString.Empty;
			consolCost.E6_ApportionmentMethod = costLine.ApportionmentMethod.GetValueOrDefault();
			consolCost.E6_RatingBehaviour = costLine.RatingBehaviour?.Code ?? ZString.Empty;
			consolCost.CostCalculationDescription = GetCostCalculationDescription(chargeCode, costLine, consol);

			createdCosts.Add(consolCost);

			return string.Empty;
		}

		string AddJobCharge(
			AccChargeCode chargeCode,
			ConsolCostLine costLine,
			ForwardingConsol consol,
			Job billingJob,
			List<Charge> createdJobCharges)
		{
			var chargeCollection = billingJob.Charges;

			var jobCharge = chargeCollection.AddNew();
			jobCharge.JR_AC = chargeCode.PK;
			jobCharge.JR_LocalCostAmt = costLine.CostOSAmount.GetValueOrDefault();
			jobCharge.JR_RX_NKCostCurrency = costLine.CostOSCurrency?.Code ?? ZString.Empty;
			jobCharge.JR_IsSpotCost = !string.IsNullOrEmpty(costLine.RatingBehaviour?.Code);
			jobCharge.CostCalculationDescription = GetCostCalculationDescription(chargeCode, costLine, consol);

			createdJobCharges.Add(jobCharge);

			return string.Empty;
		}

		ZBlob GetCostCalculationDescription(AccChargeCode accChargeCode, ConsolCostLine costLine, ForwardingConsol consol)
		{
			var chargeCodeAcCode = accChargeCode.AC_Code;
			var chargeCodeAcDesc = accChargeCode.AC_DescMultilingual;
			var chargeCodeGroup = accChargeCode.AC_ChargeGroup;
			var universalChargeCode = costLine.UniversalChargeCode?.Code ?? ZString.Empty;

			var costCalculationDescription = $@"{chargeCodeAcCode}
{chargeCodeAcDesc}

Charge located in {costLine.Creditor?.Key}(AirlineConnect) Wise Costing - General Costing with the following details:
Mode:					{consol.TransportMode}
Charge Code Group:			{chargeCodeGroup}
Rate Provider:				XABE
Origin:					{consol.Transports.DepartureTransport.LoadPort.Code}
Destination:				{consol.Transports.ArrivalTransport.DiscPort.Code}
Universal Charge Codes:		{universalChargeCode}
Carrier Charge Codes:			{costLine.ChargeCode.Code}
Autorated for:				{consol.JobType} {consol.JobNumber}
(Master bill=""{consol.JK_MasterBillNum}"")
Autorating Date:				{consol.AutoratingDate}
Leg:					{consol.Transports.DepartureTransport.LoadPort.Code}-{consol.Transports.ArrivalTransport.DiscPort.Code}


User:					{costLine.CostOwner?.Name}
Time:					{ZDateTime.Now}
";

			if (chargeCodeGroup.IsEmpty)
			{
				costCalculationDescription = costCalculationDescription.Replace("Charge Code Group:\t\t\t\r\n", "");
			}

			if (universalChargeCode.IsEmpty)
			{
				costCalculationDescription = costCalculationDescription.Replace("Universal Charge Codes:\t\t\r\n", "");
			}

			if (consol.AutoratingDate.IsEmpty)
			{
				costCalculationDescription = costCalculationDescription.Replace("Autorating Date:\t\t\t\t\r\n", "");
			}

			if (costLine.CostOwner == null)
			{
				costCalculationDescription = costCalculationDescription.Replace("User:\t\t\t\t\t\r\n", "");
			}

			return ZBlob.FromUTF8(costCalculationDescription);
		}

		void RunCriticalValidation(IXmlImportLogger logger, IReadOnlyCollection<JobConsolCost> costs, List<string> messagesForUser)
		{
			foreach (var cost in costs)
			{
				if (cost is ISupportCriticalValidation criticalValidationSupporter
					&& !HasPassedCriticalValidation(logger, criticalValidationSupporter, messagesForUser, isConsolCost: true)
					|| cost.ApportionmentCharges.OfType<ISupportCriticalValidation>().Any(charge => !HasPassedCriticalValidation(logger, charge, messagesForUser, isConsolCost: true)))
				{
					cost.DeleteCostAndCharges();
				}
				else
				{
					var message = Res.GetString("09d58794-ceee-438e-9aa0-c479f16465f8", "Imported {0} {1} {2} {3}.",
						cost.HumanReadableName, cost.ChargeCode?.AC_Code, cost.E6_OSCostAmount, cost.E6_RX_NKCurrency);

					logger.Log(LogType.Information, message);
				}
			}
		}

		void RunCriticalValidation(IXmlImportLogger logger, IReadOnlyCollection<Charge> jobCahrges, List<string> messagesForUser)
		{
			foreach (Charge charge in jobCahrges)
			{
				if (charge is ISupportCriticalValidation criticalValidationSupporter &&
					!HasPassedCriticalValidation(logger, criticalValidationSupporter, messagesForUser, isConsolCost: false))
				{
					charge.Delete();
				}
				else
				{
					var message = Res.GetString("09d58794-ceee-438e-9aa0-c479f16465f8", "Imported {0} {1} {2} {3}.",
						charge.HumanReadableName, charge.ChargeCode?.AC_Code, charge.JR_LocalCostAmt, charge.JR_RX_NKCostCurrency);

					logger.Log(LogType.Information, message);
				}
			}
		}

		bool HasPassedCriticalValidation(IXmlImportLogger logger, ISupportCriticalValidation criticalValidationSupporter, List<string> messagesForUser, bool isConsolCost)
		{
			try
			{
				criticalValidationSupporter.CriticalValidation?.RunOnSavingCheck();
				criticalValidationSupporter.CriticalValidation?.RunAfterSavingCheck();
				return true;
			}
			catch (OnSavingCriticalCheckException exc)
			{
				var costTypeName = isConsolCost ? Res.GetString("ed856564-1b2e-444d-a923-14f422baca83", "Consol Cost") : Res.GetString("bbc8e0a4-9a3e-494a-b761-bfdc6aa9a75f", "Job Charge");
				var humanReadableName = criticalValidationSupporter is BusinessObject bizObj
					? (string)bizObj.HumanReadableName
					: costTypeName;

				var message = Res.GetString("f4e6da7a-ea3c-40ce-acf2-b33ea849ebf3", "{0} could not be imported because it would cause the following critical validation failure:\r\n{1}", humanReadableName, exc.Message);
				logger?.Log(LogType.Warning, message);
				messagesForUser.Add(message);
				return false;
			}
		}
	}
}
