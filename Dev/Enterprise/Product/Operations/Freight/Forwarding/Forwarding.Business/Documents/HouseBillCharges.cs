using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business;

public class HouseBillCharges
{
	public HouseBillCharges(ForwardingShipment shipment)
	{
		this.shipment = Argument.NotNull(shipment, nameof(shipment));
		accounting = ObjectFactory.Get<IAccounting>();
		consignor = shipment.Consignor;
		consignee = shipment.Consignee;
		controllingCustomer = shipment.ControllingCustomer;

		PrintChargesBilledToLocalClientAtDestAsCollect = PrintChargesBilledToLocalClientAtDestAsCollectHelper.IsEnabled(
			shipment.JS_TransportMode,
			shipment.JS_RL_NKOrigin.SubstringSafe(0, 2),
			shipment.JS_RL_NKDestination.SubstringSafe(0, 2),
			GetCommunityRegionsCompany(true)?.GC_RN_NKCountryCode ?? ZString.Empty);
	}

	readonly OrgHeader consignor;
	readonly OrgHeader consignee;
	readonly OrgHeader controllingCustomer;

	#region JobHeaderAtPort

	public JobHeader JobHeaderAtOrigin
	{
		get
		{
			if (jobHeaderAtOrigin == null)
			{
				jobHeaderAtOrigin = GetJobHeaderAtPort(true);
			}

			return jobHeaderAtOrigin;
		}
	}
	JobHeader jobHeaderAtOrigin;

	JobHeader JobHeaderAtDestination
	{
		get
		{
			if (jobHeaderAtDestination == null)
			{
				jobHeaderAtDestination = GetJobHeaderAtPort(false);
			}

			return jobHeaderAtDestination;
		}
	}
	JobHeader jobHeaderAtDestination;

	JobHeader GetJobHeaderAtPort(bool isOrigin)
	{
		if (shipment.IsDeleted)
		{
			return null;
		}

		AddCompanyFetchHints();

		// There can be multiple companies. Product requirement is to get the one that meets the defintion of "Origin/(Destination)" and job header was created first
		var companyGroupings = GetHomeBranchCompanies(isOrigin)
				.Select(c => new CompanyGrouping { Company = c, GroupType = CompanyGroupType.HomeBranch })
			.Union(GetCountryCompanies(isOrigin)
				.Select(c => new CompanyGrouping { Company = c, GroupType = CompanyGroupType.Country }))
			.Union(GetCommunityRegionsCompanies(isOrigin)
				.Select(c => new CompanyGrouping { Company = c, GroupType = CompanyGroupType.CommunityRegion }))
			.Union(GetExtraPortCompanies(isOrigin)
				.Select(c => new CompanyGrouping { Company = c, GroupType = CompanyGroupType.ExtraPort }))
			.ToList();

		return GetJobHeaderWithValidCharges(companyGroupings, isOrigin);
	}

	JobHeader GetJobHeaderWithValidCharges(IEnumerable<CompanyGrouping> companyGroupings, bool isOrigin)
	{
		var companies = companyGroupings
			.Select(cg => cg.Company)
			.Distinct();

		AddJobFetchHints(companies);

		var jobs = companies
			.Select(shipment.GetJob)
			.Where(job => job != null)
			.OrderBy(job => job.JH_SystemCreateTimeUtc)
			.ThenBy(job => job.JH_JobLocalReference)
			.ToList();

		AddJobChargeFetchHints(jobs);

		var validJobs = new List<ValidJobWithCharges>();
		foreach (var job in jobs)
		{
			var charges = GetChargesFromJobHeader(job);

			if (charges.Any(c => IsChargeValid(c, job, isOrigin)))
			{
				validJobs.Add(new ValidJobWithCharges(job, charges));
			}
		}

		if (!validJobs.Any())
		{
			return null;
		}

		return PrioritizeJobs(validJobs, companyGroupings, isOrigin);
	}

	void AddCompanyFetchHints()
	{
		shipment.Factory.AddFetchHint(typeof(GlbCompany), GetCountryCompaniesQuery(shipment, true));
		shipment.Factory.AddFetchHint(typeof(GlbCompany), GetCountryCompaniesQuery(shipment, false));
	}

	void AddJobFetchHints(IEnumerable<GlbCompany> companies)
	{
		foreach (var company in companies)
		{
			var jobQuery = new ZQuery();
			jobQuery.AddToFilter(JobHeaderSchema.JH_ParentID, shipment.PK);
			jobQuery.AddToFilter(JobHeaderSchema.JH_GC, company.PK);
			jobQuery.AddToFilter(JobHeaderSchema.JH_IsActive, true);
			shipment.Factory.AddFetchHint(typeof(JobHeader), jobQuery);
		}
	}

	void AddJobChargeFetchHints(IEnumerable<JobHeader> jobs)
	{
		foreach (var job in jobs)
		{
			var chargeQuery = new ZQuery();
			chargeQuery.AddToFilter(JobChargeSchema.JR_JH, job.PK);
			shipment.Factory.AddFetchHint(typeof(JobCharge), chargeQuery);
		}
	}

	#region Job Selection Based On Charge Rules

	bool IsChargeValid(JobCharge c, JobHeader job, bool isOrigin)
	{
		if (isOrigin)
		{
			return c.SellAmount > 0
				&& c.SellAccount != null
				&& (
					c.SellAccount == job.AgentCollect
					|| (!IsOrgProxy(c.SellAccount)
						|| IsOrgProxy(c.SellAccount)
							&& (c.SellAccount == consignor
								|| c.SellAccount == controllingCustomer
								|| IsIFTToRelatedParty(c.SellAccount, controllingCustomer)
								|| IsIFTToRelatedParty(c.SellAccount, consignor)))
				);
		}

		return c.SellAmount > 0
			&& c.SellAccount != null
			&& c.SellAccount == job.LocalCharges;
	}

	JobHeader PrioritizeJobs(List<ValidJobWithCharges> validJobs, IEnumerable<CompanyGrouping> companyGroupings, bool isOrigin)
	{
		if (validJobs.Count <= 1)
		{
			return validJobs.Select(j => j.Job).FirstOrDefault();
		}

		ValidJobWithCharges bestJob = null;
		int highestScore = int.MinValue;

		foreach (var jobWithCharges in validJobs)
		{
			int score = CalculateJobScore(jobWithCharges, companyGroupings, isOrigin);

			if (score > highestScore)
			{
				highestScore = score;
				bestJob = jobWithCharges;
			}
		}

		return bestJob?.Job;
	}

	int CalculateJobScore(ValidJobWithCharges jobWithCharges, IEnumerable<CompanyGrouping> companyGroupings, bool isOrigin)
	{
		// If MORE than one Company meets ALL 3 Sets of Criteria:
		// 1. Company has Job Header & Charges AND
		// 2. Company meets one of those ‘Location’ criteria AND
		// 3. Company has at least one Charge meeting those Charges criteria

		var consignorConsignee = isOrigin ? consignor : consignee;
		var originDestination = isOrigin ? shipment.JS_RL_NKOrigin : shipment.JS_RL_NKDestination;
		var countryCode = originDestination.SubstringSafe(0, 2);

		var score = 0;

		// Then use the following priority sequence to identify the Origin/(Destination) Company:

		// 1. Sell Amount > 0 with Debtor && Local Client = Shipment > IFT related to Controlling Customer
		if (jobWithCharges.Charges.Any(c => c.SellAccount == jobWithCharges.Job.LocalCharges
			&& IsIFTToRelatedParty(c.SellAccount, shipment.ControllingCustomer)))
		{
			score += 100;
		}

		// 2. Sell Amount > 0 Charge with Debtor && Local Client = Shipment > IFT related to Consignor/(Consignee)
		if (jobWithCharges.Charges.Any(c => c.SellAccount == jobWithCharges.Job.LocalCharges
			&& IsIFTToRelatedParty(c.SellAccount, consignorConsignee)))
		{
			score += 90;
		}

		// 3. Sell Amount > 0 Charge with Debtor && Local Client = Shipment > Controlling Customer
		if (jobWithCharges.Charges.Any(c => c.SellAccount == jobWithCharges.Job.LocalCharges
			&& c.SellAccount == shipment.ControllingCustomer))
		{
			score += 80;
		}

		// 4. Sell Amount > 0 Charge with Debtor && Local Client = Shipment > Consignor/(Consignee)
		if (jobWithCharges.Charges.Any(c => c.SellAccount == jobWithCharges.Job.LocalCharges
			&& c.SellAccount == consignorConsignee))
		{
			score += 70;
		}

		// 5. Shipment’s Origin/(Destination) = Job Header > Branch > Home Port
		if (jobWithCharges.Job.Branch.GB_RL_NKHomePort == originDestination)
		{
			score += 60;
		}

		// 6. Company is under the same Country of the Origin/(Destination)
		if (jobWithCharges.Job.Company.GC_RN_NKCountryCode == countryCode)
		{
			score += 50;
		}

		// 7. Country of the Origin/(Destination) is under the Community Region setup of the Company
		if (companyGroupings.Any(cg => cg.GroupType == CompanyGroupType.CommunityRegion && cg.Company == jobWithCharges.Job.Company))
		{
			score += 40;
		}

		// 8. Shipment’s Origin/(Destination) found in Job Header > Branch > Additional Related Ports
		if (companyGroupings.Any(cg => cg.GroupType == CompanyGroupType.ExtraPort && cg.Company == jobWithCharges.Job.Company))
		{
			score += 30;
		}

		// Additional rules if `isOrigin` is true
		if (isOrigin)
		{
			// Rule 9: Sell Amount > 0 with Debtor = Local Client and NOT Organisation Proxy
			if (jobWithCharges.Charges.Any(c => c.SellAccount == jobWithCharges.Job.LocalCharges && !IsOrgProxy(c.SellAccount)))
			{
				score += 20;
			}

			// Rule 10: Sell Amount > 0 with Debtor = Overseas Agent
			if (jobWithCharges.Charges.Any(c => c.SellAccount == jobWithCharges.Job.AgentCollect))
			{
				score += 10;
			}
		}

		return score;
	}

	bool IsIFTToRelatedParty(OrgHeader debtor, OrgHeader orgHeader)
	{
		if (debtor == null || orgHeader == null)
		{
			return false;
		}

		var key = (debtor.PK, orgHeader.PK);

		if (iftToRelatedPartyCache.TryGetValue(key, out var isRelated))
		{
			return isRelated;
		}

		isRelated = orgHeader.AllRelatedParties.Cast<OrgRelatedParty>().Any((OrgRelatedParty rp) =>
			rp.PR_PartyType == RelatedPartyTypeList.Codes.InvoiceFreightJobsTo
			&& debtor.PK == rp.PR_OH_RelatedParty);
		iftToRelatedPartyCache[key] = isRelated;

		return isRelated;
	}

	readonly Dictionary<(ZGuid, ZGuid), bool> iftToRelatedPartyCache = new();

	bool IsOrgProxy(OrgHeader orgHeader)
	{
		if (orgHeader == null)
		{
			return false;
		}

		return orgHeader.IsProxyOrgOfAnyCompany(false);
	}

	#endregion

	#region Company Queries

	IEnumerable<GlbCompany> GetCountryCompanies(bool isOrigin)
	{
		return shipment.Factory.Load<GlbCompany>(GetCountryCompaniesQuery(shipment, isOrigin)).ToList();
	}

	static ZQuery GetCountryCompaniesQuery(ForwardingShipment shipment, bool isOrigin)
	{
		var countryCode = (isOrigin ? shipment.JS_RL_NKOrigin : shipment.JS_RL_NKDestination).SubstringSafe(0, 2);
		var companyQuery = new ZQuery(GlbCompanySchema.GC_RN_NKCountryCode, countryCode);
		companyQuery.AddToFilter(GlbCompanySchema.GC_IsActive, ZBool.True);
		return companyQuery;
	}

	GlbCompany GetCommunityRegionsCompany(bool isOrigin)
	{
		return GetCommunityRegionsCompanies(isOrigin).FirstOrDefault();
	}

	GlbCompany[] GetCommunityRegionsCompanies(bool isOrigin)
	{
		var countryCode = (isOrigin ? shipment.JS_RL_NKOrigin : shipment.JS_RL_NKDestination).SubstringSafe(0, 2);
		var country = shipment.Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, countryCode));

		if (country == null)
		{
			return Array.Empty<GlbCompany>();
		}

		var communityRegionsQuery = new ZQuery(StmDataSchema.SD_Name, "CommunityRegionsForDirectionCalculation");
		var items = shipment.Factory.Load<StmData>(communityRegionsQuery);
		var filteredItems = items.Where(item =>
		{
			var regionCountryPKs = ConvertBinaryValueToString(item.SD_BinaryValue);
			return regionCountryPKs.Contains(country.PK.ToString());
		});

		if (!filteredItems.Any())
		{
			return Array.Empty<GlbCompany>();
		}

		var companyPks = filteredItems.Select(item => item.SD_Owner);
		return shipment.Factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.PK, companyPks));
	}

	GlbCompany[] GetHomeBranchCompanies(bool isOrigin)
	{
		var portCode = isOrigin ? shipment.JS_RL_NKOrigin : shipment.JS_RL_NKDestination;

		var branchSubQuery = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.PK, JobHeaderSchema.JH_GB);
		branchSubQuery.AddToFilter(GlbBranchSchema.GB_RL_NKHomePort, portCode);

		var jobSubQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_GC);
		jobSubQuery.AddSubQuery(branchSubQuery, JoinCondition.And);
		jobSubQuery.AddToFilter(JobHeaderSchema.JH_ParentID, shipment.PK);
		jobSubQuery.AddToFilter(JobHeaderSchema.JH_IsActive, ZBool.True);

		var companyQuery = new ZDBOnlyQuery(typeof(GlbCompany));
		companyQuery.AddSubQuery(jobSubQuery, JoinCondition.And);

		return shipment.Factory.Load<GlbCompany>(companyQuery);
	}

	GlbCompany[] GetExtraPortCompanies(bool isOrigin)
	{
		var portCode = isOrigin ? shipment.JS_RL_NKOrigin : shipment.JS_RL_NKDestination;

		var extraPortSubQuery = new ZDBOnlySubQuery(typeof(GlbBranchExtraPorts), GlbBranchExtraPortsSchema.GY_GB);
		extraPortSubQuery.AddToFilter(GlbBranchExtraPortsSchema.GY_RL_NKAdditionalBranchRelatedPort, portCode);

		var branchSubQuery = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.PK, JobHeaderSchema.JH_GB);
		branchSubQuery.AddSubQuery(extraPortSubQuery, JoinCondition.And);

		var jobSubQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_GC);
		jobSubQuery.AddSubQuery(branchSubQuery, JoinCondition.And);
		jobSubQuery.AddToFilter(JobHeaderSchema.JH_ParentID, shipment.PK);
		jobSubQuery.AddToFilter(JobHeaderSchema.JH_IsActive, ZBool.True);

		var companyQuery = new ZDBOnlyQuery(typeof(GlbCompany));
		companyQuery.AddSubQuery(jobSubQuery, JoinCondition.And);

		return shipment.Factory.Load<GlbCompany>(companyQuery);
	}

	#endregion

	#endregion

	#region GetCharges

	public ZBool PrintChargesBilledToLocalClientAtDestAsCollect { get; }

	public JobCharge[] GetCharges(bool isHBLOrHAWB = true)
	{
		if (JobHeaderAtOrigin == null && isHBLOrHAWB)
		{
			return GetChargesAtDestination(true);
		}

		var originCharges = GetChargesAtOrigin();

		if (!PrintChargesBilledToLocalClientAtDestAsCollect)
		{
			return originCharges;
		}

		var collectCharges = GetChargesAtDestination(false);
		if (collectCharges.Length == 0)
		{
			return originCharges;
		}

		var prepaidCharges = originCharges.Where(c => c.JR_OH_SellAccount == JobHeaderAtOrigin.LocalChargesPK).ToList();
		var charges = new List<JobCharge>();
		charges.AddRange(prepaidCharges);
		charges.AddRange(collectCharges);

		return charges.ToArray();
	}

	public JobCharge[] GetChargesAtOrigin()
	{
		return GetChargesAtPort(true);
	}

	public JobCharge[] GetChargesAtDestination(bool isJobDestinationOnly)
	{
		if (!isJobDestinationOnly)
		{
			return GetChargesAtDestinationFromOriginJob();
		}

		return GetChargesAtPort(false);
	}

	JobCharge[] GetChargesAtPort(bool isOrigin)
	{
		var jobHeader = isOrigin ? JobHeaderAtOrigin : JobHeaderAtDestination;

		if (jobHeader == null)
		{
			return Array.Empty<JobCharge>();
		}

		// If we are not in the company which owns prepaid/collect (origin/destination) charges, then we need to impersonate this company
		// and load charges using its context.
		if (jobHeader.JH_GC != GlbCompany.CurrentCompany.PK && jobHeader.Company.FirstActiveBranch != null)
		{
			using (Env.Instance.SuppressSwitchContextCheck())
			using (DisposableEnvironment.ForBranch(jobHeader.Company.FirstActiveBranch.PK.ToGuid()))
			{
				// Load using readonly factory to avoid any modifications to other company charges
				var newFactory = new ReadOnlyBusinessObjectFactory();
				var headerInNewFactory = newFactory.Load<JobHeader>(jobHeader.PK);
				return GetChargesFromJobHeader(headerInNewFactory);
			}
		}
		else
		{
			var charges = GetChargesFromJobHeader(jobHeader);
			return charges;
		}
	}

	#endregion

	#region Destination Charges

	/// <summary>
	///		Retrieves the charges applicable at the destination for the current shipment.
	/// </summary>
	/// <returns>
	///		An enumerable collection of JobCharge objects representing the charges at the destination.
	/// </returns>
	/// <remarks>
	///		This method attempts to retrieve charges in the following order:
	///			1. Existing charges auto-rated by the overseas agent.
	///			2. If no charges are found, it attempts to auto-rate charges on behalf of the overseas company
	///			   (only if we are in the origin company).
	///
	///		The method ensures that appropriate charges are always returned, even if they need to be
	///		calculated or derived from other sources.
	/// </remarks>
	JobCharge[] GetChargesAtDestinationFromOriginJob()
	{
		var charges = Array.Empty<JobCharge>();

		if (JobHeaderAtOrigin == null)
		{
			return charges;
		}

		var destinationCompany = GetCompanyFromDestinationAgent(JobHeaderAtOrigin.AgentCollectPK);

		if (destinationCompany != null)
		{
			var jobHeaderAtDestination = shipment.GetJob(destinationCompany);
			if (jobHeaderAtDestination != null)
			{
				// Try to load existing charges autorated by the overseas agent
				charges = GetChargesFromJobHeader(jobHeaderAtDestination, jobHeaderAtDestination.LocalCharges);
			}

			charges = AutoRateOverseasJobIfNecessary(charges, destinationCompany);
		}

		return charges;
	}

	JobCharge[] AutoRateOverseasJobIfNecessary(JobCharge[] charges, GlbCompany destinationCompany)
	{
		if (!charges.Any() && GlbCompany.CurrentCompany.PK != destinationCompany.PK)
		{
			// Charges are not yet autorated. Well, we can do it on behalf of the overseas company...
			// ... but only if we are not in destination company already, otherwise the user should autorate
			// rates in a normal way.
			//
			// Also, please note that the way it is implemented, we only autorated destination
			// job header from the origin company and not the other way around. There were not strict arguments against
			// this, it is just not common to generate HWB from the destination side and thus autorated the origin job header.
			// But, if somebody requests this, then this functionality can be revised.
			return AutoRateOverseasJob(destinationCompany);
		}

		return charges;
	}

	internal JobCharge[] AutoRateOverseasJob(GlbCompany overseasAgentCompany)
	{
		var branch = overseasAgentCompany.FirstActiveBranch;
		if (branch == null)
		{
			return Array.Empty<JobCharge>();
		}

		using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
		{
			// We do it in a separate factory so that changes don't get saved. All we need is overseas company charges to
			// display them on HBL. We don't want to affect overseas company job and charges as it is wrong from accounting perspective.
			var newFactory = new ReadOnlyBusinessObjectFactory();
			var shipmentInNewFactory = newFactory.Load<ForwardingShipment>(shipment.PK);

			var loader = new JobHeader.Loader(shipmentInNewFactory);
			var header = loader.TryLoadOrCreateWithMutex(branch);
			if (header == null)
			{
				return Array.Empty<JobCharge>();
			}

			try
			{
				var logger = new Logger();
				var jobAutoRater = ObjectFactory.Get<IJobAutoRater>();
				jobAutoRater.AutoRate(shipmentInNewFactory, logger, new AutoRateOptions(autoRateRevenue: true));

				var charges = GetChargesFromJobHeader(header, header.LocalCharges);

				ShowAutoratingLog(overseasAgentCompany, charges, logger.ToString());
				return charges;
			}
			catch (Exception)
			{
				return Array.Empty<JobCharge>();
			}
			finally
			{
				header.DisposeAndPreventSave();
			}
		}
	}

	void ShowAutoratingLog(GlbCompany overseasAgentCompany, JobCharge[] charges, string ratingLog)
	{
		var title = ResString.GetMultilingualString("2b4f92df-29db-40f6-b533-e122679e4444", "Autorating Log");
		var message = ResString.GetMultilingualString("368db2a0-bb66-463d-831e-3d2c56dc1923",
			@"Charges intended to be billed by Destination Company {0} have not yet been created at Destination.
	Autorating Revenue is run for the Destination Company {0} with {1} Charges found and printed as Collect Charges. ({1} is the number of Charges found.)

	This process will not create Job Header nor charges under the Destination Company.

	{2}", overseasAgentCompany.GC_Code, charges.Length, ratingLog);
		shipment.ShowMessageOnGUI(title, message);
	}

	GlbCompany GetCompanyFromDestinationAgent(ZGuid destinationAgentPK)
	{
		if (destinationAgentPK.IsEmpty)
		{
			return null;
		}

		var originCompanyPK = JobHeaderAtOrigin?.Company?.PK ?? ZGuid.Empty;

		var companyQuery = new ZQuery(GlbCompanySchema.GC_OH_OrgProxy, destinationAgentPK);
		companyQuery.AddToFilter(GlbCompanySchema.GC_IsActive, true);
		companyQuery.AddToFilter(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, originCompanyPK);

		var overseasAgentCompany = shipment.Factory.LoadTop1<GlbCompany>(companyQuery);
		if (overseasAgentCompany != null)
		{
			return overseasAgentCompany;
		}

		var branchQuery = new ZQuery(GlbBranchSchema.GB_OH_OrgProxy, destinationAgentPK);
		branchQuery.AddToFilter(GlbBranchSchema.GB_IsActive, true);

		var overseasAgentBranch = shipment.Factory.LoadTop1<GlbBranch>(branchQuery);
		if (overseasAgentBranch != null && (overseasAgentBranch.Company?.PK ?? ZGuid.Empty) != originCompanyPK)
		{
			return overseasAgentBranch.Company;
		}

		return null;
	}

	#endregion

	#region Implementation

	JobCharge[] GetChargesFromJobHeader(JobHeader header, OrgHeader sellAccount = null)
	{
		if (header == null)
		{
			return Array.Empty<JobCharge>();
		}

		var query = new ZQuery(JobChargeSchema.JR_JH, header.PK);
		var chargesFromJobHeader = header.Factory.Load<JobCharge>(query);

		return chargesFromJobHeader
			.Where(c => !(c.IsRevenuePostedWithAutoJobRevenueJournal || c.IsRevenuePostedWithManualJobRevenueJournal))
			.Where(c => sellAccount == null || c.JR_OH_SellAccount == sellAccount.PK)
			.Where(c => c.JR_AC != accounting.ProfitShareChargeCode)
			.OrderBy(c => c.JR_DisplaySequence)
			.ThenBy(c => c.JR_SystemLastEditTimeUtc)
			.ToArray();
	}

	class ValidJobWithCharges
	{
		public ValidJobWithCharges(JobHeader job, IEnumerable<JobCharge> charges)
		{
			Job = job;
			Charges = charges;
		}

		public JobHeader Job { get; set; }
		public IEnumerable<JobCharge> Charges { get; set; }
	}

	string ConvertBinaryValueToString(ZBlob binaryValue) => System.Text.Encoding.Unicode.GetString(binaryValue);

	class CompanyGrouping
	{
		public GlbCompany Company { get; set; }
		public CompanyGroupType GroupType { get; set; }
	}

	enum CompanyGroupType
	{
		HomeBranch,
		Country,
		CommunityRegion,
		ExtraPort
	}

	readonly IAccounting accounting;
	readonly ForwardingShipment shipment;

	#endregion
}
