using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IDocumentFieldAttributeFinder
	{
		IDocumentFieldDefinitionCollection FindProperties(Type objectType);
	}

	public interface IDocumentFieldDefinitionCollection : IBusinessObjectCollection
	{
		new IDocumentField this[int i] { get; }
	}

	public interface IDocumentField
	{
		ZString FieldName { get; }
		ZPropertyInfo FieldNameInfo { get; }

		ZString FieldDescription { get; }
		ZPropertyInfo FieldDescriptionInfo { get; }
	}
}
