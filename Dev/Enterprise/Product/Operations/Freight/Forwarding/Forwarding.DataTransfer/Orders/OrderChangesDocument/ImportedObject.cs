using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public abstract class ImportedObject : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Amended Properties

		public ImportedProperty AmendedProperty1 { get { return GetAmendedPropertyToShow(1); } }
		public ImportedProperty AmendedProperty2 { get { return GetAmendedPropertyToShow(2); } }
		public ImportedProperty AmendedProperty3 { get { return GetAmendedPropertyToShow(3); } }
		public ImportedProperty AmendedProperty4 { get { return GetAmendedPropertyToShow(4); } }
		public ImportedProperty AmendedProperty5 { get { return GetAmendedPropertyToShow(5); } }
		public ImportedProperty AmendedProperty6 { get { return GetAmendedPropertyToShow(6); } }
		public ImportedProperty AmendedProperty7 { get { return GetAmendedPropertyToShow(7); } }
		public ImportedProperty AmendedProperty8 { get { return GetAmendedPropertyToShow(8); } }
		public ImportedProperty AmendedProperty9 { get { return GetAmendedPropertyToShow(9); } }
		public ImportedProperty AmendedProperty10 { get { return GetAmendedPropertyToShow(10); } }
		public ImportedProperty AmendedProperty11 { get { return GetAmendedPropertyToShow(11); } }
		public ImportedProperty AmendedProperty12 { get { return GetAmendedPropertyToShow(12); } }
		public ImportedProperty AmendedProperty13 { get { return GetAmendedPropertyToShow(13); } }
		public ImportedProperty AmendedProperty14 { get { return GetAmendedPropertyToShow(14); } }
		public ImportedProperty AmendedProperty15 { get { return GetAmendedPropertyToShow(15); } }
		public ImportedProperty AmendedProperty16 { get { return GetAmendedPropertyToShow(16); } }
		public ImportedProperty AmendedProperty17 { get { return GetAmendedPropertyToShow(17); } }
		public ImportedProperty AmendedProperty18 { get { return GetAmendedPropertyToShow(18); } }
		public ImportedProperty AmendedProperty19 { get { return GetAmendedPropertyToShow(19); } }
		public ImportedProperty AmendedProperty20 { get { return GetAmendedPropertyToShow(20); } }
		public ImportedProperty AmendedProperty21 { get { return GetAmendedPropertyToShow(21); } }
		public ImportedProperty AmendedProperty22 { get { return GetAmendedPropertyToShow(22); } }
		public ImportedProperty AmendedProperty23 { get { return GetAmendedPropertyToShow(23); } }
		public ImportedProperty AmendedProperty24 { get { return GetAmendedPropertyToShow(24); } }
		public ImportedProperty AmendedProperty25 { get { return GetAmendedPropertyToShow(25); } }
		public ImportedProperty AmendedProperty26 { get { return GetAmendedPropertyToShow(26); } }
		public ImportedProperty AmendedProperty27 { get { return GetAmendedPropertyToShow(27); } }
		public ImportedProperty AmendedProperty28 { get { return GetAmendedPropertyToShow(28); } }
		public ImportedProperty AmendedProperty29 { get { return GetAmendedPropertyToShow(29); } }
		public ImportedProperty AmendedProperty30 { get { return GetAmendedPropertyToShow(30); } }
		public ImportedProperty AmendedProperty31 { get { return GetAmendedPropertyToShow(31); } }
		public ImportedProperty AmendedProperty32 { get { return GetAmendedPropertyToShow(32); } }
		public ImportedProperty AmendedProperty33 { get { return GetAmendedPropertyToShow(33); } }
		public ImportedProperty AmendedProperty34 { get { return GetAmendedPropertyToShow(34); } }
		public ImportedProperty AmendedProperty35 { get { return GetAmendedPropertyToShow(35); } }
		public ImportedProperty AmendedProperty36 { get { return GetAmendedPropertyToShow(36); } }
		public ImportedProperty AmendedProperty37 { get { return GetAmendedPropertyToShow(37); } }
		public ImportedProperty AmendedProperty38 { get { return GetAmendedPropertyToShow(38); } }
		public ImportedProperty AmendedProperty39 { get { return GetAmendedPropertyToShow(39); } }
		public ImportedProperty AmendedProperty40 { get { return GetAmendedPropertyToShow(40); } }
		public ImportedProperty AmendedProperty41 { get { return GetAmendedPropertyToShow(41); } }
		public ImportedProperty AmendedProperty42 { get { return GetAmendedPropertyToShow(42); } }
		public ImportedProperty AmendedProperty43 { get { return GetAmendedPropertyToShow(43); } }
		public ImportedProperty AmendedProperty44 { get { return GetAmendedPropertyToShow(44); } }
		public ImportedProperty AmendedProperty45 { get { return GetAmendedPropertyToShow(45); } }
		public ImportedProperty AmendedProperty46 { get { return GetAmendedPropertyToShow(46); } }
		public ImportedProperty AmendedProperty47 { get { return GetAmendedPropertyToShow(47); } }
		public ImportedProperty AmendedProperty48 { get { return GetAmendedPropertyToShow(48); } }
		public ImportedProperty AmendedProperty49 { get { return GetAmendedPropertyToShow(49); } }
		public ImportedProperty AmendedProperty50 { get { return GetAmendedPropertyToShow(50); } }

		public abstract ImportedProperty[] GetAllImportedProperties();
		public abstract List<string> PropertiesToShowAlways { get; }
		public abstract List<string> OtherPropertiesToShow { get; }

		#endregion

		#region Implementation

		ImportedProperty GetAmendedPropertyToShow(int item)
		{
			int index = item - 1;
			return index < AmendedPropertiesToShow.Length ? AmendedPropertiesToShow[index] : null;
		}

		ImportedProperty[] AmendedPropertiesToShow
		{
			get
			{
				if (amendedPropertiesToShow == null)
				{
					List<ImportedProperty> list = new List<ImportedProperty>();
					foreach (ImportedProperty importedProperty in GetAllImportedProperties())
					{
						if (ShowProperty(importedProperty))
						{
							list.Add(importedProperty);
						}
					}
					amendedPropertiesToShow = list.ToArray();
				}
				return amendedPropertiesToShow;
			}
		}
		ImportedProperty[] amendedPropertiesToShow;

		bool ShowProperty(ImportedProperty property)
		{
			return ModifiedProperties.Contains(property.Name)
				|| PropertiesToShowAlways.Contains(property.Name)
				|| OtherPropertiesToShow.Contains(property.Name);
		}

		internal List<string> ModifiedProperties
		{
			get
			{
				if (modifiedProperties == null)
				{
					modifiedProperties = new List<string>();
					foreach (ImportedProperty property in GetAllImportedProperties())
					{
						if (property.State == ImportedPropertyState.Modified)
						{
							modifiedProperties.Add(property.Name);
						}
					}
				}
				return modifiedProperties;
			}
		}
		List<string> modifiedProperties;

		#endregion
	}
}
