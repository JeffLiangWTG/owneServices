using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Module
{
	public class UtcOffsetFilter : ModuleTextFilter
	{
		public UtcOffsetFilter(ZString description, GetUtfOffsetQueryDelegate queryDelegate, IEnumerable<ZShort> utcOffsetList)
			: base(description, queryDelegate)
		{
			UtcOffsetList = utcOffsetList;
			var utcOffsetPairList = GetOffsetFromUtcPairList(utcOffsetList);
			UtcOffsetFromList = utcOffsetPairList;
			UtcOffsetToList = utcOffsetPairList;
		}

		#region Property

		public ReadOnlyCodeDescriptionPairList UtcOffsetFromList { get; }

		public ReadOnlyCodeDescriptionPairList UtcOffsetToList { get; }

		IEnumerable<ZShort> UtcOffsetList { get; }

		[List("UtcOffsetFromList")]
		public ZString UtcOffsetFrom
		{
			get { return utcOffsetFrom; }
			set
			{
				if (utcOffsetFrom != value)
				{
					utcOffsetFrom = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateAll();
					}
					UtcOffsetFromInfo.RefreshBinding();
				}
			}
		}

		[List("UtcOffsetToList")]
		public ZString UtcOffsetTo
		{
			get { return utcOffsetTo; }
			set
			{
				if (utcOffsetTo != value)
				{
					utcOffsetTo = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateAll();
					}
					UtcOffsetToInfo.RefreshBinding();
				}
			}
		}

		ZString utcOffsetTo;
		ZString utcOffsetFrom;

		public ZPropertyInfo UtcOffsetFromInfo
		{
			get { return GetZPropertyInfo(nameof(UtcOffsetFrom)); }
		}

		public ZPropertyInfo UtcOffsetToInfo
		{
			get { return GetZPropertyInfo(nameof(UtcOffsetTo)); }
		}

		ReadOnlyCodeDescriptionPairList GetOffsetFromUtcPairList(IEnumerable<ZShort> utcOffsetList)
		{
			var pairs = utcOffsetList.Select(x =>
			{
				var offsetInHours = (ZDecimal)((ZDecimal)x / (ZDecimal)60);
				return new CodeDescriptionPair(x.ToString("0"), offsetInHours.ToString("UTC+0.00;UTC-0.00"));
			});
			var result = new CodeDescriptionPairList();

			foreach (var pair in pairs)
			{
				result.Add(pair);
			}

			return result;
		}

		#endregion

		#region Validation

		public new UtcOffsetFilterValidation Validation
		{
			get { return (UtcOffsetFilterValidation)base.Validation; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new UtcOffsetFilterValidation(this, UtcOffsetList);
		}

		#endregion

		#region Clear / IsEmpty / Default

		protected override void ClearCore()
		{
			UtcOffsetFrom = string.Empty;
			UtcOffsetTo = string.Empty;
		}

		protected override bool IsEmptyCore => utcOffsetFrom.IsEmpty && utcOffsetTo.IsEmpty;

		protected override FilterCategory DefaultCategory => FilterCategories.Dates;

		#endregion

		#region Query

		public delegate ZQuery GetUtfOffsetQueryDelegate(ZString utcOffsetFrom, ZString utcOffsetTo);

		protected override object[] QueryDelegateParameters
		{
			get { return new object[] { UtcOffsetFrom, UtcOffsetTo }; }
		}

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			throw new NotSupportedException();
		}

		#endregion

		#region Serialization

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			writer.WriteElementString("UtcOffsetFrom", UtcOffsetFrom);
			writer.WriteElementString("UtcOffsetTo", UtcOffsetTo);
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			if (reader.Name == "UtcOffsetFrom")
			{
				UtcOffsetFrom = reader.ReadElementString("UtcOffsetFrom");
			}

			if (reader.Name == "UtcOffsetTo")
			{
				UtcOffsetTo = reader.ReadElementString("UtcOffsetTo");
			}
		}

		#endregion

		#region GetNewCommonModuleFilter / CopyPersistantValuesFromFilter
		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			throw new NotSupportedException();
		}

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			var filter = (UtcOffsetFilter)filterToCopyFrom;
			UtcOffsetFrom = filter.UtcOffsetFrom;
			UtcOffsetTo = filter.UtcOffsetTo;
		}

		#endregion

		#region Test Data Setup
#if DEBUG
		protected override void FillWithValidTestFilterValueCore()
		{
		}
#endif
		#endregion

		public override bool IsExpensiveQuery => false;
	}
}
