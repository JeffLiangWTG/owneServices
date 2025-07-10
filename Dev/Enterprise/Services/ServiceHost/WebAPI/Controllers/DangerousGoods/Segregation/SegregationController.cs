using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using CargoWise.Definitions.Freight.DangerousGoods.SegregationApi;
using CargoWise.EntityFramework;

namespace Enterprise.Services.ServiceHost.WebAPI.Controllers.DangerousGoods.Segregation
{
	[GlowTicketAuthentication]
	[RoutePrefix("api/dangerousgoods/segregation")]
	public class SegregationController : ApiController
	{
		readonly ISegregationRulesManager _segregationRulesManager;
		readonly ISegregationQueryValidator _segregationQueryValidator;
		readonly IDatabaseService _databaseService;

		public SegregationController() : this(CreateSegregationRulesManager(), new SegregationQueryValidator(), CreateDatabaseService())
		{
		}

		public SegregationController(ISegregationRulesManager segregationRulesManager, ISegregationQueryValidator segregationQueryValidator, IDatabaseService databaseService)
		{
			_segregationRulesManager = segregationRulesManager;
			_segregationQueryValidator = segregationQueryValidator;
			_databaseService = databaseService;
		}

		[HttpPost]
		[Route("check")]
		public IHttpActionResult Check([FromBody] SegregationQuery segregationQuery)
		{
			var (isQueryValid, validationMessage) = _segregationQueryValidator.IsQueryValid(segregationQuery);
			if (!isQueryValid)
			{
				return BadRequest(validationMessage);
			}

			try
			{
				_databaseService.PreloadData(segregationQuery.Ids, segregationQuery.Standards);
				var dgPairInfo = _segregationRulesManager.Check(segregationQuery.Standards, segregationQuery.Ids, _databaseService.Find);

				return Ok(new
				{
					DGPairInfo = dgPairInfo
				});
			}
			catch (DataNotFoundException exception)
			{
				return BadRequest(exception.Message);
			}
			catch (DataCorruptionException exception)
			{
				return InternalServerError(new Exception(exception.Message));
			}
		}

		[HttpPost]
		[Route("checkUnsaved")]
		public IHttpActionResult CheckUnsaved([FromBody] SegregationEntityQuery segregationEntityQuery)
		{
			var (isQueryValid, validationMessage) = _segregationQueryValidator.IsEntityQueryValid(segregationEntityQuery);
			if (!isQueryValid)
			{
				return BadRequest(validationMessage);
			}

			try
			{
				_databaseService.PreloadData(segregationEntityQuery.UNDGDataItemDTOs);
				var dgPairInfo = _segregationRulesManager.Check(segregationEntityQuery.Standards, segregationEntityQuery.UNDGDataItemDTOs, _databaseService.Find);

				return Ok(new
				{
					DGPairInfo = dgPairInfo
				});
			}
			catch (DataNotFoundException exception)
			{
				return BadRequest(exception.Message);
			}
			catch (DataCorruptionException exception)
			{
				return InternalServerError(new Exception(exception.Message));
			}
		}

		static ISegregationRulesManager CreateSegregationRulesManager()
		{
			var rules = GetSegregationRules();
			return new SegregationRulesManager(rules);
		}

		static IEnumerable<ISegregationRule> GetSegregationRules()
		{
			var rules = new List<ISegregationRule>();

			var ruleTypes = Assembly.GetExecutingAssembly()
				.GetTypes()
				.Where(type => typeof(ISegregationRule).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface)
				.ToList();

			foreach (var ruleType in ruleTypes)
			{
				var rule = (ISegregationRule)Activator.CreateInstance(ruleType);
				rules.Add(rule);
			}

			return rules;
		}

		static IDatabaseService CreateDatabaseService()
		{
			var factory = new BusinessObjectFactory();
			var databaseService = new DatabaseService(factory);
			return databaseService;
		}
	}
}
