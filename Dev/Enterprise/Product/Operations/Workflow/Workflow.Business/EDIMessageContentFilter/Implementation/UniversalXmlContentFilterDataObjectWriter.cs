using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Workflow.Integration;

namespace Enterprise.Workflow.Business
{
	public class UniversalXmlContentFilterDataObjectWriterFactory : IUniversalXmlContentFilterDataObjectWriterFactory
	{
		public IDataObjectWriterStrategy Load(BusinessObjectFactory factory, ZString purposeCode, EDIMessageContentFilterSchemaType type)
		{
			var filter = EDIMessageContentFilter.Load(factory, purposeCode);
			if (filter != null)
			{
				return new UniversalXmlContentFilterDataObjectWriter(filter, type);
			}
			else
			{
				return DefaultDataObjectWriterStrategy.Instance;
			}
		}

		public IDataObjectWriterStrategy Load(IMessageProfile messageProfile)
		{
			if (messageProfile != null && messageProfile.IsValid())
			{
				return new UniversalXmlContentFilterDataObjectWriter(messageProfile);
			}
			else
			{
				return DefaultDataObjectWriterStrategy.Instance;
			}
		}

		public IDataObjectWriterStrategy Load(BusinessObjectFactory factory, ZString purposeCode, ZString actionType)
		{
			return Load(factory, purposeCode, UniversalConstantConverter.GetSchemaTypeFromActionType(actionType));
		}
	}

	public class UniversalXmlContentFilterDataObjectWriter : IDataObjectWriterStrategy
	{
		readonly HashSet<string> elements;
		readonly bool isExclude;

		internal UniversalXmlContentFilterDataObjectWriter(EDIMessageContentFilter filter, EDIMessageContentFilterSchemaType type)
		{
			var spec = UniversalConstantConverter.GetSpec(filter, type);
			isExclude = spec.IsExclude;
			elements = spec.Lines.Cast<EDIMessageContentFilterLine>().Select(l => l.SchemaElement.ToString()).ToHashSet(StringComparer.OrdinalIgnoreCase);
		}

		internal UniversalXmlContentFilterDataObjectWriter(IMessageProfile messageProfile)
		{
			isExclude = messageProfile.GetFilterType() == SchemaFilterType.Exclude;
			elements = messageProfile.GetFilterElements()
				.Where(e => e.Value == null || !e.Value.Any())
				.Select(e => e.Key)
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.ToHashSet(StringComparer.OrdinalIgnoreCase);
		}

		public bool IsAllowSet(string fieldName)
		{
			return !(isExclude ^ !elements.Contains(fieldName));
		}
	}
}
