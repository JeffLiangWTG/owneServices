using System.Linq;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Module
{
	public delegate ZQuery GetDGClassDGSubstanceQueryDelegate(SQLComparisonOperator dgOperator, ZString dgClass, ZString dgSubstance);

	public class DGClassDGSubstanceFilter : ModuleTextBaseFilter
	{
		#region Construction

		public DGClassDGSubstanceFilter(ZString description, GetDGClassDGSubstanceQueryDelegate queryDelegate)
			: base(description, queryDelegate)
		{
		}

		DGClassDGSubstanceFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		#endregion

		#region Properties

		#region DGClass

		[List(nameof(DGClasses))]
		[MaxLength(AutoUNDGDataItem.Schema.DI_IMOClassMaxLength)]
		public ZString DGClass
		{
			get { return DGClass_ReadOnly ? ZString.Empty : dgClass; }
			set
			{
				if (SetNonPersistentPropertyValue(DGClassInfo, ref dgClass, value))
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateDGClass();
					}

					FillDGClassIfNeccessary();
				}
			}
		}
		ZString dgClass;

		public ZPropertyInfo DGClassInfo
		{
			get { return this.GetZPropertyInfo(nameof(DGClass)); }
		}

		public bool DGClass_ReadOnly => Property_ReadOnly;

		#endregion

		#region DGSubstance

		[List(nameof(DGSubstances))]
		public ZString DGSubstance
		{
			get { return DGSubstance_ReadOnly ? ZString.Empty : dgSubstance; }
			set
			{
				if (SetNonPersistentPropertyValue(DGSubstanceInfo, ref dgSubstance, value))
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateDGSubstance();
					}

					FillDGClassIfNeccessary();
				}
			}
		}
		ZString dgSubstance;

		public ZPropertyInfo DGSubstanceInfo
		{
			get
			{
				return this.GetZPropertyInfo(nameof(DGSubstance));
			}
		}

		public bool DGSubstance_ReadOnly => Property_ReadOnly;

		#endregion

		#endregion

		#region Lists

		public CodeDescriptionPairList DGClasses => UNDGDataItemLookups.GetDGClassList(new BusinessObjectFactory());

		public UNDGSubstanceCollection DGSubstances
		{
			get { return dgSubstances ?? (dgSubstances = new UNDGSubstanceCollection(new BusinessObjectFactory(), new ZQuery())); }
		}
		UNDGSubstanceCollection dgSubstances;

		#endregion

		#region Validation

		public new DGClassDGSubstanceFilterValidation Validation
		{
			get { return ((DGClassDGSubstanceFilterValidation)(base.Validation)); }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new DGClassDGSubstanceFilterValidation(this);
		}

		#endregion

		#region DGClass Pre-Fill

		void FillDGClassIfNeccessary()
		{
			var updatedDGClass = GetDGClassFromSubstance(DGSubstance);
			if (!DGSubstance.IsEmpty && !DGClass.IsEmpty && DGClass != updatedDGClass)
			{
				DGClass = updatedDGClass;
			}
		}

		string GetDGClassFromSubstance(string substance)
		{
			var variant = string.Empty;
			var unno = substance;

			if (substance.Length >= 5)
			{
				variant = substance.Substring(4);
				unno = substance.Substring(0, 4);
			}

			var undgSubstance = UNDGSubstanceLoader.LoadSubstances(new BusinessObjectFactory(), unno, variant).FirstOrDefault();
			return undgSubstance?.DG_Class ?? string.Empty;
		}

		#endregion

		#region Implementation

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			base.CopyPersistantValuesFromFilter(filterToCopyFrom);
			DGClassDGSubstanceFilter source = ((DGClassDGSubstanceFilter)(filterToCopyFrom));
			source.dgSubstance = dgSubstance;
			source.dgClass = dgClass;
		}

		protected override void ClearCore()
		{
			base.ClearCore();
			DGClass = string.Empty;
			DGSubstance = string.Empty;
		}

		protected override FilterCategory DefaultCategory
		{
			get { return FilterCategories.ModesAndTypes; }
		}

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			return new DGClassDGSubstanceFilter(category, parentCollection);
		}

		public override bool HasComparisonOperator => true;

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);
			writer.WriteElementString("DGClass", DGClass);
			writer.WriteElementString("DGSubstance", DGSubstance);
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			base.DeserializePropertiesFromXml(reader);
			DGClass = reader.ReadElementString("DGClass");
			DGSubstance = reader.ReadElementString("DGSubstance");
		}

		protected override bool IsEmptyCore => base.IsEmptyCore && DGClass.IsEmpty && DGSubstance.IsEmpty;

		protected override object[] QueryDelegateParameters
		{
			get { return new object[] { SqlComparisonOperator, DGClass, DGSubstance }; }
		}

		#endregion
	}
}
