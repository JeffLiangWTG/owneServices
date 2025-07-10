using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Workflow.Business
{
	public class EDIMessageContentFilterLineLookups : ZLookups
	{
		const string TheOnlySupportRepeatableSchemaElement = "SubShipmentCollection";

		public EDIMessageContentFilterLineLookups(EDIMessageContentFilterLine parent)
			: base(parent)
		{
			Parent = parent;
		}
		public new EDIMessageContentFilterLine Parent { get; }

		public CodeDescriptionPairList AllSchemaElements
		{
			get
			{
				switch (Parent.Schema)
				{
					case EDIMessageContentFilterLineSchemas.Codes.UniversalShipment:
						return Factory.GetCachedValue("UniversalShipmentCollectionTypes", () => UniversalXmlContentFilterApplicator.GetPropertyList(typeof(UniversalDataBuss.DataObjects.Universal.Shipment)));
					case EDIMessageContentFilterLineSchemas.Codes.UniversalEvent:
						return Factory.GetCachedValue("UniversalEventCollectionTypes", () => UniversalXmlContentFilterApplicator.GetPropertyList(typeof(UniversalDataBuss.DataObjects.Universal.Event)));
					case EDIMessageContentFilterLineSchemas.Codes.UniversalTransaction:
						return Factory.GetCachedValue("UniversalTransactionCollectionTypes", () => UniversalXmlContentFilterApplicator.GetPropertyList(typeof(UniversalDataBuss.DataObjects.Accounting.TransactionBatch)));
					default:
						return new CodeDescriptionPairList();
				}
			}
		}

		public CodeDescriptionPairList SchemaElements
		{
			get
			{
				var allElements = AllSchemaElements;
				if (allElements.Count > 0)
				{
					var hash = RemoveTheExistingSchemaElementsExceptSubShipmentCollection();
					var elements = new CodeDescriptionPairList();
					elements.AddRange(allElements.Cast<CodeDescriptionPair>().Where(p => !hash.Contains(p.Code)).ToList());
					return elements;
				}
				else
				{
					return allElements;
				}
			}
		}

		HashSet<string> RemoveTheExistingSchemaElementsExceptSubShipmentCollection()
		{
			var hash = new HashSet<string>(Parent.Parent.Lines.Cast<EDIMessageContentFilterLine>().Select(l => l.SchemaElement.ToString()));
			hash.Remove(Parent.SchemaElement);
			hash.Remove(TheOnlySupportRepeatableSchemaElement);
			return hash;
		}

		public CodeDescriptionPairList DataContexts
		{
			get
			{
				switch (Parent.SchemaElement)
				{
					case TheOnlySupportRepeatableSchemaElement:
						return Factory.GetCachedValue("UniversalShipmentDataContexts", () => UniversalValidationRuleSetLookups.DataContexts);
					default:
						return new CodeDescriptionPairList();
				}
			}
		}
	}
}
