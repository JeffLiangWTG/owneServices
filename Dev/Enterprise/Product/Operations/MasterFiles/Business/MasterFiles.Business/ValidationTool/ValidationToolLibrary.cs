using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using Enterprise.Workflow.Integration;

namespace Enterprise.MasterFiles.Business;

public sealed class ValidationToolLibrary : MacroLibrary
{
	readonly BusinessObjectFactory factory;

	public ValidationToolLibrary(BusinessObjectFactory factory)
	{
		this.factory = factory;
	}

	protected override IEnumerable<IHandler> MacroHandlers
	{
		get
		{
			yield return new Handler<Func<BusinessObject, IEnumerable, bool>>(
				nameof(ValidateDocumentAttributes),
				ValidateDocumentAttributesDescription,
				(bizo, requiredDocuments) => ValidateDocumentAttributes(bizo, requiredDocuments),
				$"{nameof(ValidateDocumentAttributes)}({{RequiredDocuments}})");
			yield return new Handler<Func<IMacroScope, BusinessObject, MacroClosure, bool>>(
				nameof(ValidateDocumentAttributes),
				ValidateFilteredDocumentAttributesDescription,
				(scope, bizo, filter) => ValidateDocumentAttributes(scope, bizo, filter),
				$"{nameof(ValidateDocumentAttributes)}({{RequiredDocuments}})");
		}
	}

	bool ValidateDocumentAttributes(BusinessObject businessObject, IEnumerable requiredDocuments)
	{
		if (businessObject == null || requiredDocuments == null)
		{
			return false;
		}

		using (var macroContext = ObjectFactory.Get<IWorkflowMacroContextDecider>().GetDefaultWorkflowMacroContext(factory, businessObject))
		{
			var macroEvaluator = ObjectFactory.Get<IWorkflowMacroValueEvaluator>();
			return requiredDocuments.Cast<JobRequiredDocument>().All(x => x.Attributes.All(y => y.IsCustomAttribute && (macroEvaluator.EvaluateBooleanExpression(factory, macroContext, y.D0_AttribDisplayValue) ?? false)));
		} 
	}

	bool ValidateDocumentAttributes(IMacroScope scope, BusinessObject businessObject, MacroClosure filter)
	{
		if (businessObject == null)
		{
			return false;
		}

		using var elementScope = new MacroScope(scope);
		var collection = (IEnumerable)filter.Invoke(elementScope);
		return ValidateDocumentAttributes(businessObject, collection);
	}

	#region SuppressResourceStringsCheckRegion

	const string ValidateDocumentAttributesDescription = "Validates if all custom attributes of the given required documents match the specified business object.";

	const string ValidateFilteredDocumentAttributesDescription = "Validates if all custom attributes of the given filtered required documents match the specified business object.";

	#endregion
}
