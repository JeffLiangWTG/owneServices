using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using CargoWise.eHub.Portal.Models.eHubTransactions;

namespace CargoWise.eHub.DataModel.Business.Validation
{
	public class RemoteValidationAttribute : RemoteAttribute
	{
		public static IeHubTransactionsContext Context { get; set; }

		public RemoteValidationAttribute(string routeName)
			: base()
		{
		}

		public RemoteValidationAttribute(string action, string controller)
			: base(action, controller)
		{
		}

		public RemoteValidationAttribute(string action, string controller, string areaName)
			: base(action, controller, areaName)
		{
		}

		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			string controllerName = this.RouteData["controller"].ToString() + "Controller";
			string actionName = this.RouteData["action"].ToString();
			IEnumerable<string> additionalFields = this.AdditionalFields.Split(',');

			Type controllerType = null;
			AppDomain.CurrentDomain.GetAssemblies()
				.FirstOrDefault(assembly =>
				{
					return assembly.GetTypes()
						.Any(type =>
						{
							if (type.Name == controllerName)
							{
								controllerType = type;
								return true;
							}
							return false;
						});
				});

			List<object> propertyValues = new List<object>();
			propertyValues.Add(value);
			propertyValues.AddRange(additionalFields.Select<string, object>(additionalField =>
			{
				PropertyInfo property = validationContext.ObjectType.GetProperty(additionalField);
				return property != null ? property.GetValue(validationContext.ObjectInstance, null) : null;
			}));

			if (controllerType != null)
			{
				object instance = Activator.CreateInstance(controllerType);

				if (Context != null)
				{
					var context = controllerType.GetProperty("Context");
					context.SetValue(instance, Context);
				}

				MethodInfo method = controllerType.GetMethod(actionName);

				if (method != null)
				{
					ActionResult response = (ActionResult)method.Invoke(instance, propertyValues.ToArray());

					if (response is JsonResult)
					{
						bool isAvailable = false;
						JsonResult json = (JsonResult)response;
						string jsonData = json.Data.ToString();

						bool.TryParse(jsonData, out isAvailable);

						if (!isAvailable)
						{
							return new ValidationResult(this.FormatErrorMessage(validationContext.DisplayName));
						}
						else
						{
							return ValidationResult.Success;
						}
					}
				}
			}
			return ValidationResult.Success;
		}
	}
}
