using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.UNDGCommonData)]
	public class UNDGCommonDataCollection : ActiveBusinessObjectCollection<UNDGCommonData>
	{
		public UNDGCommonDataCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public UNDGCommonDataCollection(BusinessObjectFactory factory, ZString type)
			: base(factory)
		{
			AdditionalFilter = new ZQuery(UNDGCommonDataSchema.DC_Type, type);

			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Type", "Property", type));
		}

		public UNDGCommonDataCollection(UNDGSubstance substance, string commonDataType, string attributeDataType)
			: this(substance, commonDataType, new[] { attributeDataType })
		{
		}

		public UNDGCommonDataCollection(UNDGSubstance substance, string commonDataType, IEnumerable<string> attributeDataTypes, Func<UNDGCommonData, ZString> getAttributeTypeFromCommonData = null)
			: base(substance.Factory, new UNDGCommonDataSubstanceRelationship(substance, commonDataType, attributeDataTypes, getAttributeTypeFromCommonData))
		{
			Substance = substance;
			CommonDataType = commonDataType;

			Substance.DetailsLanguageInfo.ValueChanged += delegate
			{ RefreshForLanguage(); };
			RefreshForLanguage();
		}

		readonly UNDGSubstance Substance;
		readonly string CommonDataType;

		#region Implementation

		void RefreshForLanguage()
		{
			AdditionalFilter = AdditionalFilterWithoutLanguage ?? new ZQuery();
			if (!Substance.DetailsLanguage.IsEmpty)
			{
				AdditionalFilter.AddToFilter(UNDGCommonDataSchema.DC_Language, Substance.DetailsLanguage);
			}
		}

		protected override void SetDefaultsForNewElementCore(UNDGCommonData newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.DC_Type = CommonDataType;
		}

		protected override bool AllowNew
		{
			get { return Substance == null || !Substance.DG_IsSystem; }
		}

		internal void ResetFilterFromAttributes(IEnumerable<ViewUNDGAttribute> attributes)
		{
			var indexes = attributes.ToArray().Select(x => x.DA_Index);
			var query = new ZQuery(UNDGCommonDataSchema.DC_Index, indexes);
			query.AddToFilter(UNDGCommonDataSchema.DC_Type, CommonDataType);

			AdditionalFilterWithoutLanguage = query;

			if (!Substance.DetailsLanguage.IsEmpty)
			{
				query.AddToFilter(UNDGCommonDataSchema.DC_Language, Substance.DetailsLanguage);
			}

			AdditionalFilter = query;
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		ZQuery AdditionalFilterWithoutLanguage;

		public override void Delete(UNDGCommonData businessObject)
		{
			if (Substance != null)
			{
				Relationship.RemoveFromRelationship(businessObject);
			}

			base.Delete(businessObject);
		}

		#endregion
	}
}
