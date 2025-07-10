using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Registry
{
	public abstract class PhaseDependantsProvider
	{
		#region Parent Type

		protected abstract Type ParentType { get; }

		#endregion

		protected BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}

		BusinessObjectFactory factory;

		protected virtual string ParentWorkflowType
		{
			get { return string.Empty; }
		}

		public IEnumerable<IPhaseDependant> GetWorkflowFields()
		{
			return !string.IsNullOrWhiteSpace(ParentWorkflowType)
				? GetWorkflowFields(ParentWorkflowType)
				: Enumerable.Empty<IPhaseDependant>();
		}

		[SuppressMessage("Enterprise.Globalization", "EDI007:CustomizableDataTranslationRule")]
		IEnumerable<IPhaseDependant> GetWorkflowFields(string workflowType)
		{
			var queryTemplates = new ZDBOnlySubQuery(typeof(ProcessTaskTemplate), ProcessTaskTemplateSchema.PK);
			queryTemplates.AddToFilter(ProcessTaskTemplateSchema.P0_ProcessType, workflowType);
			queryTemplates.AddToFilter(ProcessTaskTemplateSchema.P0_IsActive, true);

			var queryDefinitions = new ZDBOnlyQuery(typeof(GenCustomColumnDefinition));
			queryDefinitions.AddSubQuery(GenCustomColumnDefinitionSchema.XC_ParentID, queryTemplates, JoinCondition.And);

			return Factory.Load<GenCustomColumnDefinition>(queryDefinitions)
				.GroupBy(def => new { def.XC_Name, def.XC_NameMultilingual, def.XC_Type })
				.Select(def => new PropertyDependant(CustomPropertyHelper.GeneratePropertyIdentifier(def.Key.XC_Name, AddOnColumnDataType.GetTypeFromCode(def.Key.XC_Type)), string.Format(CultureInfo.InvariantCulture, @"{0} ({1})", def.Key.XC_NameMultilingual, def.Key.XC_Type)));
		}

		#region IZTypeProperties

		public IEnumerable<IPhaseDependant> GetIZTypeProperties()
		{
			foreach (IPhaseDependant dependant in GetIZTypeProperties(ParentType))
			{
				yield return dependant;
			}
		}

		protected IEnumerable<IPhaseDependant> GetIZTypeProperties(Type typeToReflect)
		{
			List<PropertyDependant> result = new List<PropertyDependant>();
			if (typeToReflect != null)
			{
				ZString tableName = BusinessObjectFactory.HasTableName(typeToReflect) ? BusinessObjectFactory.GetTableNameFromType(typeToReflect) : "";

				foreach (PropertyInfo propertyInfo in typeToReflect.GetProperties())
				{
					if (typeof(IZType).IsAssignableFrom(propertyInfo.PropertyType) && propertyInfo.CanWrite)
					{
						result.Add(new PropertyDependant(propertyInfo.Name, propertyInfo.Name));
					}
				}
			}

			return result;
		}

		#endregion

		#region ChildDependants

		public virtual IEnumerable<IPhaseDependant> GetChildDependants()
		{
			return Enumerable.Empty<IPhaseDependant>();
		}

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an intended method signature")]
		public virtual Dictionary<IPhaseDependant, IEnumerable<IPhaseDependant>> GetChildExpandableDependants()
		{
			return new Dictionary<IPhaseDependant, IEnumerable<IPhaseDependant>>();
		}

		protected IEnumerable<IPhaseDependant> GetIZTypePropertiesPrefixedWithName(ZString childName, Type childType)
		{
			List<PropertyDependant> childProperties = new List<PropertyDependant>();
			foreach (IPhaseDependant childProperty in GetIZTypeProperties(childType))
			{
				ZString nameWithPrefix = childName + "." + childProperty.Name;
				childProperties.Add(new PropertyDependant(nameWithPrefix, childProperty.Description));
			}

			return childProperties;
		}

		#endregion

		#region PlugInDependants

		public virtual IEnumerable<IPhaseDependant> GetPlugInDependants()
		{
			return Enumerable.Empty<IPhaseDependant>();
		}

		#endregion

		#region IPhaseDependant Implemetation Classes

		public class PropertyDependant : PhaseReadOnlyDependantImpl
		{
			public PropertyDependant(ZString name, ZString description)
				: base(name, description, PhaseConstants.DependantType.Property)
			{
			}
		}

		public class TypeDependant : PhaseReadOnlyDependantImpl
		{
			public TypeDependant(ZString name, ZString description)
				: base(name, description, PhaseConstants.DependantType.TypeName)
			{
			}
		}

		public class PhaseReadOnlyDependantImpl : IPhaseDependant
		{
			public PhaseReadOnlyDependantImpl(ZString name, ZString description, ZString dependantType)
			{
				Name = Argument.NotNullOrEmpty(name, "name");
				Description = description;
				DependantType = dependantType;
			}

			public PhaseReadOnlyDependantImpl(IPhaseDependant dependant)
			{
				if (dependant != null)
				{
					Name = dependant.Name;
					Description = dependant.Description;
					DependantType = dependant.DependantType;
					IsMandatory = dependant.IsMandatory;
					IsReadOnly = dependant.IsReadOnly;
				}
			}

			public ZString Name { get; set; }
			public ZString Description { get; set; }
			public ZString DependantType { get; set; }
			public ZBool IsMandatory { get; set; }
			public ZBool IsReadOnly { get; set; }
		}

		#endregion
	}
}
