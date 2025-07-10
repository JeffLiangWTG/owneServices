using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Xml;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Workflow;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using static Enterprise.Core.SharedConstants;

namespace Enterprise.Services.ServiceHost
{
	[GlowTicketAuthentication]
	public class CustomFieldsController : ApiController
	{
		[Route("api/customfields/{prefix}/{pk:guid}")]
		public IHttpActionResult GetCustomFieldDefinitionsByGuid(string prefix, Guid pk)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var factory = new BusinessObjectFactory() { NameForDebugging = "Custom Fields Web Service", RefreshEnabled = false };
				if (factory.Load(prefix, pk) is not ICustomFieldProvider provider)
				{
					return NotFound();
				}

				var result = new List<object>();
				var bizo = provider.GetCustomBusinessObject();
				var customProperties = ((ICustomPropertyContainer)bizo).CustomProperties.OrderBy(x => x.CustomColumnDefinition.Sequence);
				foreach (var customProperty in customProperties)
				{
					if (customProperty.CustomColumnDefinition.RuleDefinitionReference.HasValue)
					{
						factory.AddFetchHint(typeof(GenCustomAddOnRule), customProperty.CustomColumnDefinition.RuleDefinitionReference.Value);
					}
				}

				foreach (var customProperty in customProperties)
				{
					var item = new
					{
						customProperty.CustomColumnDefinition.Name,
						customProperty.CustomColumnDefinition.Type,
						Caption = customProperty.CustomColumnDefinition.NameLocalized,
						MetaData = GetApplicableMetaData(factory, customProperty.CustomColumnDefinition.RuleDefinitionReference, GetCustomFieldOrigin(customProperty.GetType().Name))
					};
					result.Add(item);
				}

				return Json(result, new JsonSerializerSettings
				{
					ContractResolver = new CamelCasePropertyNamesContractResolver()
				});
			}
		}

		[Route("api/customfields/{prefix}/{languageCode?}")]
		public IHttpActionResult GetCustomFieldDefinitions(string prefix, string languageCode = Languages.English)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var processType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(prefix, reportUnknownPrefix: false);
				var workflowDescriptors = WorkflowDescriptors.Instance.Values.Where(x => x.WorkflowProviderType == processType);

				var factory = new BusinessObjectFactory() { NameForDebugging = "Custom Fields Web Service", RefreshEnabled = false };
				var customColumnsProvider = ObjectFactory.New<ICustomColumnsProvider>();

				var genCustomColumnDefinitions = workflowDescriptors.SelectMany(d =>
				{
					var customColumnDefinitions = customColumnsProvider.GetCustomColumnDefinitions(factory, d.Code);
					return customColumnDefinitions;
				}).Cast<GenCustomColumnDefinition>();

				var result = new List<object>();
				foreach (var genCustomColumnDefinition in genCustomColumnDefinitions)
				{
#pragma warning disable EDI007 // Customizable Data Translation Rule we want the non translated name
					result.Add(new
					{
						Name = genCustomColumnDefinition.XC_Name,
						Type = genCustomColumnDefinition.XC_Type,
						Caption = genCustomColumnDefinition.XC_NameMultilingual.ToString(languageCode),
						MetaData = GetApplicableMetaData(factory, genCustomColumnDefinition.XC_XR, CustomFieldOrigin.GenCustomColumnDefinition)
					});
#pragma warning restore EDI007 // Customizable Data Translation Rule
				}

				return Json(result, new JsonSerializerSettings
				{
					ContractResolver = new CamelCasePropertyNamesContractResolver()
				});
			}
		}

		static Dictionary<string, object> GetApplicableMetaData(BusinessObjectFactory factory, ZGuid? ruleID, CustomFieldOrigin customFieldOrigin)
		{
			var metaData = new Dictionary<string, object>
			{
				{ nameof(customFieldOrigin), Enum.GetName(typeof(CustomFieldOrigin), customFieldOrigin) }
			};

			if (ruleID == null || ruleID.Value.IsEmpty)
			{
				return metaData;
			}

			var ruleset = factory.Load<GenCustomAddOnRule>(ruleID.Value);
			XElement element;
			try
			{
				element = XElement.Parse(ruleset.XR_SourceCode);
			}
			catch (XmlException)
			{
				return metaData;
			}

			new RuleFactory().PopulateMetaData(metaData, element);
			return metaData;
		}

		static CustomFieldOrigin GetCustomFieldOrigin(string type)
		{
			return type switch
			{
				"WrappedUserDefinedCustomProperty" => CustomFieldOrigin.OrgCustomLabels,
				_ => CustomFieldOrigin.GenCustomColumnDefinition,
			};
		}

		sealed class RuleFactory : RuleFactory<CustomAddOnRule>
		{
			public void PopulateMetaData(IDictionary<string, object> metaData, XElement element)
			{
				foreach (var rule in FromXml(element))
				{
					if (rule.IsEnabled)
					{
						rule.PopulateMetaData(metaData);
					}
				}
			}

			protected override IRuleProvider<IEnumerable<KeyValuePair<string, string>>> InvalidCodeRuleProvider
				=> new RuleProvider<IEnumerable<KeyValuePair<string, string>>>(GetInvalidCodeMetaData);

			void GetInvalidCodeMetaData(IDictionary<string, object> metaData, IEnumerable<KeyValuePair<string, string>> valuePairs)
			{
				var list = valuePairs.ToArray();
				var maxLength = list.Length > 0 ? list.Max(x => x.Key.Length) : 0;
				metaData.Add(nameof(maxLength), maxLength);
				metaData.Add(nameof(list), list.Select(x => new { Code = x.Key, Desc = x.Value }));
			}

			protected override IRuleProvider<string> DateTimeFormatRuleProvider
				=> new RuleProvider<string>((metadata, dateTimeFormat) => metadata.Add(nameof(dateTimeFormat), dateTimeFormat));

			protected override IRuleProvider<CreateEventRuleArgs> CreateEventRuleProvider
				=> new RuleProvider<CreateEventRuleArgs>(null);

			protected override CustomAddOnRule NewCheckEnteredRule()
				=> new(metaData => metaData.Add((NoResString)"mandatory", true)); // metadata name, should not be translated

			sealed class RuleProvider<T> : IRuleProvider<T>
			{
				public RuleProvider(Action<IDictionary<string, object>, T> metaDataPopulator)
				{
					this.metaDataPopulator = metaDataPopulator;
				}

				readonly Action<IDictionary<string, object>, T> metaDataPopulator;

				public CustomAddOnRule Create(T args) => new(metaData => metaDataPopulator?.Invoke(metaData, args));

				public T GetArgs(CustomAddOnRule rule) => throw new NotImplementedException();
			}
		}

		sealed class CustomAddOnRule : ICustomAddOnRule
		{
			public CustomAddOnRule(Action<IDictionary<string, object>> metaDataPopulator)
			{
				this.metaDataPopulator = metaDataPopulator;
			}

			readonly Action<IDictionary<string, object>> metaDataPopulator;

			public string Code => null;
			public bool IsEnabled { get; set; }

			public void PopulateMetaData(IDictionary<string, object> metaData)
			{
				metaDataPopulator?.Invoke(metaData);
			}
		}

		enum CustomFieldOrigin
		{
			GenCustomColumnDefinition,
			OrgCustomLabels,
		}
	}
}
