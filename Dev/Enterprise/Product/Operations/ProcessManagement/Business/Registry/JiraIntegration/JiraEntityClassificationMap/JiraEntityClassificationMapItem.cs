using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ProcessManagement.Business
{
	[XmlSerializerAssembly("Enterprise.ProcessManagement.Business.XmlSerializers")]
	public abstract class JiraEntityClassificationMapItem : RegistryBusinessObjectTemplate<JiraEntityClassificationMapItemValidation>
	{
		#region Properties

		#region JiraEntityName

		public virtual ZString JiraEntityName
		{
			get => jiraEntityName;
			set
			{
				SetNonPersistentPropertyValue(JiraEntityNameInfo, ref jiraEntityName, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateJiraEntityName();
				}
			}
		}

		ZString jiraEntityName;

		public ZPropertyInfo JiraEntityNameInfo => GetZPropertyInfo(nameof(JiraEntityName));

		#endregion

		#region SelectionCriterionFieldName

		[List(nameof(SelectionCriterionFieldNames))]
		public virtual ZString SelectionCriterionFieldName
		{
			get => selectionCriterionFieldName;
			set
			{
				SetNonPersistentPropertyValue(SelectionCriterionFieldNameInfo, ref selectionCriterionFieldName, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateSelectionCriterionFieldName();
				}
			}
		}

		ZString selectionCriterionFieldName;

		public ZPropertyInfo SelectionCriterionFieldNameInfo => GetZPropertyInfo(nameof(SelectionCriterionFieldName));

		public ICodeDescriptionPairList SelectionCriterionFieldNames
		{
			get
			{
				if (selectionCriterionFieldNames == null)
				{
					selectionCriterionFieldNames = new CodeDescriptionPairList();

					foreach (var tuple in GetSelectionCriteriaFieldNamesAndDescriptions())
					{
						selectionCriterionFieldNames.AddPair(tuple.Item1, tuple.Item2);
					}
				}

				return selectionCriterionFieldNames;
			}
		}
		CodeDescriptionPairList selectionCriterionFieldNames;

		public IEnumerable<string> SelectionCriterionFields => selectionCriterionFields ?? (selectionCriterionFields = ((CodeDescriptionPairList)SelectionCriterionFieldNames).GetAllCodes());
		IEnumerable<string> selectionCriterionFields;

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "I considered it, but no.")]
		protected abstract IEnumerable<Tuple<string, string>> GetSelectionCriteriaFieldNamesAndDescriptions();

		protected abstract IEnumerable<string> GetSelectionCriteriaFieldNames();

		#endregion

		#region SelectionCriterionFieldValue

		[MaxLength(3)]
		public virtual ZString SelectionCriterionFieldValue
		{
			get => selectionCriterionFieldValue;
			set
			{
				SetNonPersistentPropertyValue(SelectionCriterionFieldValueInfo, ref selectionCriterionFieldValue, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateSelectionCriterionFieldValue();
				}
			}
		}

		ZString selectionCriterionFieldValue;

		public ZPropertyInfo SelectionCriterionFieldValueInfo => GetZPropertyInfo(nameof(SelectionCriterionFieldValue));

		#endregion

		#endregion

		#region RegistryBusinessObjectTemplate Overrides

		protected override JiraEntityClassificationMapItemValidation GetNewValidation() => new JiraEntityClassificationMapItemValidation(this);

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var item = CreateNewItemForClone();

			item.JiraEntityName = JiraEntityName;
			item.SelectionCriterionFieldName = SelectionCriterionFieldName;
			item.SelectionCriterionFieldValue = SelectionCriterionFieldValue;

			return item;
		}

		protected abstract JiraEntityClassificationMapItem CreateNewItemForClone();

		protected override void WriteElements(XmlWriter writer)
		{
			writer.WriteElementString(nameof(JiraEntityName), JiraEntityName);
			writer.WriteElementString(nameof(SelectionCriterionFieldName), SelectionCriterionFieldName);
			writer.WriteElementString(nameof(SelectionCriterionFieldValue), SelectionCriterionFieldValue);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			JiraEntityName = reader.ReadElementString(nameof(JiraEntityName));
			SelectionCriterionFieldName = reader.ReadElementString(nameof(SelectionCriterionFieldName));
			SelectionCriterionFieldValue = reader.ReadElementString(nameof(SelectionCriterionFieldValue));
		}

		#endregion
	}
}
