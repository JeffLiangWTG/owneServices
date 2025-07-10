using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Shared;

namespace Enterprise.Tracking.Business
{
	public enum DataState
	{
		Original,
		Updated
	}

	public class PropertyChangeInfo
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
		public const string Empty = "*empty*";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
		public const string Changed = "was changed";

		#region Constructors

		public PropertyChangeInfo(MultilingualString humanReadableName)
		{
			this.humanReadableName = humanReadableName;
		}

		public PropertyChangeInfo(MultilingualString humanReadableName, ZString originalValue, ZString updatedValue)
		{
			this.humanReadableName = humanReadableName;
			OriginalValue = originalValue;
			UpdatedValue = updatedValue;
		}

		#endregion

		#region Static Formatting Methods

		public static ZString Format(IZType value)
		{
			ZString result;

			if (value.IsEmpty)
			{
				result = Empty;
			}
			else if (value is ZDateTime)
			{
				result = WebDateTimeFormatter.GetFormattedDate((ZDateTime)value, ZDateTimePickerFormat.Long);
			}
			else if (value is ZDateTimeOffset)
			{
				result = WebDateTimeFormatter.GetFormattedDate((ZDateTimeOffset)value, ZDateTimePickerFormat.Long);
			}
			else if (value is ZBool)// for grids that were changed
			{
				result = (ZBool)value ? Changed : "";
			}
			else
			{
				result = value.ToString();
			}

			return result;
		}

		public static MultilingualString Format(INumericZType number, ZString code, ReadOnlyCodeDescriptionPairList descriptionList)
		{
			return MultilingualString.Join(" ", (NoResString)number.ToString(), Format(code, descriptionList));
		}

		public static MultilingualString Format(ZString code, ReadOnlyCodeDescriptionPairList descriptionList)
		{
			return descriptionList.GetMultilingualDescriptionFromCode(code) ?? (NoResString)"";
		}

		#endregion

		#region Human Readable Name

		public MultilingualString HumanReadableName
		{
			get { return humanReadableName; }
		}

		readonly MultilingualString humanReadableName;

		#endregion

		#region Original Value

		public ZString OriginalValue
		{
			get { return originalValueMultilingual; }
			set { originalValueMultilingual = (NoResString)value; }
		}

		public MultilingualString OriginalValueMultilingual
		{
			get { return originalValueMultilingual; }
			set { originalValueMultilingual = value; }
		}

		MultilingualString originalValueMultilingual = (NoResString)"";

		#endregion

		#region Updated Value

		public ZString UpdatedValue
		{
			get { return updatedValueMultilingual; }
			set { updatedValueMultilingual = (NoResString)value; }
		}

		public MultilingualString UpdatedValueMultilingual
		{
			get { return updatedValueMultilingual; }
			set { updatedValueMultilingual = value; }
		}

		MultilingualString updatedValueMultilingual = (NoResString)"";

		#endregion

		#region HasChanges

		public bool HasChanges
		{
			get { return OriginalValue != UpdatedValue; }
		}

		#endregion
	}
}
