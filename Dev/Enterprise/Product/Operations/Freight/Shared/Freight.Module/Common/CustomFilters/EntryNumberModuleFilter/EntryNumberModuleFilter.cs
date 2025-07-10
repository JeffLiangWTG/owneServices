using System;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Module
{
	public delegate ZQuery GetEntryNumberQuery(SQLComparisonOperator comparisonOperator, ZString entryType, ZString entryNumber);

	public class EntryNumberModuleFilter : ModuleTextBaseFilter
	{
		public EntryNumberModuleFilter(ZString description, GetEntryNumberQuery queryDelegate)
			: base(description, queryDelegate)
		{
		}

		#region Properties

		[List("EntryTypeList")]
		[MaxLength(4)]
		public ZString EntryType
		{
			get { return CMRExportExemptionCodes.Get4CharCode(entryType); }
			set
			{
				ZString newValue = CMRExportExemptionCodes.Get3CharCode(value);
				if (entryType != newValue)
				{
					CheckMaximumLength(EntryTypeInfo, newValue);
					entryType = newValue;

					if (CusEntryNumberTypes.IsExemptionCode(EntryType))
					{
						Property = ZString.Empty;
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateEntryType();
					}

					EntryTypeInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo EntryTypeInfo
		{
			get { return GetZPropertyInfo(nameof(EntryType)); }
		}

		protected override bool Property_ReadOnly
		{
			get { return CusEntryNumberTypes.IsExemptionCode(EntryType) || base.Property_ReadOnly; }
		}

		#endregion

		#region Lookups

		public CodeDescriptionPairList EntryTypeList
		{
			get { return entryTypeList ?? (entryTypeList = CusEntryNumberTypes.CountrySpecificCustomsEntryNumberTypeList(LookupsFactory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, false)); }
		}

		BusinessObjectFactory LookupsFactory
		{
			get { return Factory ?? lookupsFactory ?? (lookupsFactory = new BusinessObjectFactory()); }
		}

		BusinessObjectFactory lookupsFactory;

		#endregion

		#region BusinessObject Overrides

		protected override void ClearCore()
		{
			base.ClearCore();
			EntryType = ZString.Empty;
		}

		protected override bool IsEmptyCore => base.IsEmptyCore && EntryType.IsEmpty;

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			base.DeserializePropertiesFromXml(reader);
			EntryType = reader.ReadElementString("EntryType");
		}

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);
			writer.WriteElementString("EntryType", EntryType);
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new EntryNumberModuleFilterValidation(this);
		}

		public new EntryNumberModuleFilterValidation Validation
		{
			get { return (EntryNumberModuleFilterValidation)base.Validation; }
		}

		protected override object[] QueryDelegateParameters
		{
			get { return new object[] { SqlComparisonOperator, EntryType, Property }; }
		}

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			throw new NotSupportedException("The Entry Number module filter does not support common filters.");
		}

		#endregion

		#region Implementation

		ZString entryType;
		CodeDescriptionPairList entryTypeList;

		#endregion
	}
}
