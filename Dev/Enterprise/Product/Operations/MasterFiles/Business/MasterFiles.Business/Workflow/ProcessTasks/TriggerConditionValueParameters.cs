using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Workflow;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public class TriggerConditionValueParameters : NonPersistentBusinessObject, IObsoleteValidation
	{
		public TriggerConditionValueParameters(string eventCode, string reference)
		{
			CompleteText = reference;
			this.eventCode = eventCode;
		}

		readonly string eventCode;

		#region Parameter Collection

		public TriggerConditionValueParameterCollection ParameterCollection
		{
			get
			{
				if (parameterCollection == null)
				{
					parameterCollection = new TriggerConditionValueParameterCollection(eventCode);
					RegisterEditableChildObject(parameterCollection);
				}
				return parameterCollection;
			}
		}

		TriggerConditionValueParameterCollection parameterCollection;

		#endregion

		#region CompleteText

		public ZString CompleteText
		{
			get { return GenerateReference(); }
			set { PopulateParemeters(value); }
		}

		#endregion

		#region GenerateReference

		ZString GenerateReference()
		{
			if (ParameterCollection.Any())
			{
				var paramsAsString = ParameterCollection.Cast<TriggerConditionValueParameter>().OrderBy(p => p.Code).Select(p => string.Format("{0}={1}", p.Code, p.ParamValue));
				return string.Join(",", paramsAsString);
			}
			return string.Empty;
		}

		#endregion

		#region PopulateParemeters

		void PopulateParemeters(ZString reference)
		{
			ParameterCollection.RemoveAndDeleteAll();
			parameterCollection.AddRange(GetParameterCollection(EventLogReferenceBuilder.GetParametersFromText(reference, 0, ",", "=")));
		}

		IEnumerable<TriggerConditionValueParameter> GetParameterCollection(IEnumerable<KeyValuePair<string, string>> parameters)
		{
			return from parameter in parameters
				   where parameter.Key.Length <= Parameter.Schema.CodeMaxLength
				   select new TriggerConditionValueParameter(eventCode, parameter.Key, parameter.Value);
		}

		#endregion
	}
}
