using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.TransportCommon.Registry
{
	public class DateAndReferenceRegistryItem : TranslatableRegistryItem<DateAndReferenceCollection, DateAndReferenceCollection>
	{
		public DateAndReferenceRegistryItem(
				string name,
				MultilingualString category,
				MultilingualString caption,
				MultilingualString hint,
				RegistryStorageFlags storage,
				DateAndReferenceCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new DateAndReferenceRegistryDataType(), storage, defaultValue))
		{
			this.defaultDateAndReferences = defaultValue;
		}

		#region Convert

		protected override DateAndReferenceCollection Convert(DateAndReferenceCollection value)
		{
			foreach (DateAndReference item in value)
			{
				item.Description = GetMultilingualString(item.EnglishDescription);
				item.AllowRequiredFromLabel = GetMultilingualString(item.EnglishAllowRequiredFromLabel);
				item.AllowRequiredToLabel = GetMultilingualString(item.EnglishAllowRequiredToLabel);
			}
			return value;
		}

		#endregion

		#region TranslatableRegistryItem Members

		readonly DateAndReferenceCollection defaultDateAndReferences;

		public override IEnumerable<ResourceString> DefaultStrings
		{
			get
			{
				var descriptions = defaultDateAndReferences
													.Cast<DateAndReference>()
													.Select(i => i.Description as ResourceString)
													.Where(s => !string.IsNullOrWhiteSpace(s?.GetUnresolvedString()));

				var requiredFroms = defaultDateAndReferences
														.Cast<DateAndReference>()
														.Select(i => i.AllowRequiredFromLabel as ResourceString)
														.Where(s => !string.IsNullOrWhiteSpace(s?.GetUnresolvedString()));

				var requiredTos = defaultDateAndReferences
													.Cast<DateAndReference>()
													.Select(i => i.AllowRequiredToLabel as ResourceString)
													.Where(s => !string.IsNullOrWhiteSpace(s?.GetUnresolvedString()));

				return descriptions.Concat(requiredFroms).Concat(requiredTos);
			}
		}

		public override IEnumerable<string> GetCaptions(DateAndReferenceCollection value)
		{
			var descriptions = value.Cast<DateAndReference>()
									.Select(i => i.Description.ToString().Trim()).Distinct()
									.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct();

			var requiredFroms = value.Cast<DateAndReference>()
									 .Select(i => i.AllowRequiredFromLabel.ToString().Trim()).Distinct()
									 .Where(s => !string.IsNullOrWhiteSpace(s)).Distinct();

			var requiredTos = value.Cast<DateAndReference>()
								   .Select(i => i.AllowRequiredToLabel.ToString().Trim()).Distinct()
								   .Where(s => !string.IsNullOrWhiteSpace(s)).Distinct();

			return descriptions.Concat(requiredFroms).Concat(requiredTos);
		}

		public override bool IsTranslatable
		{
			get { return true; }
		}

		public override int MaxLength
		{
			get { return DateAndReference.MaxAllowRequiredFromLabelLength; }
		}

		#endregion
	}

	[RegistryEditor("Enterprise.TransportCommon.GUI.Registry.DateAndReferenceRegistryItemEditor, Enterprise.TransportCommon.GUI")]
	public class DateAndReferenceRegistryDataType : NonPersistentBusinessObjectRegistryDataType<DateAndReferenceCollection>
	{
	}
}
