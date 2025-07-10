using System.Collections.Generic;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Registry
{
	public class PhaseSecurityRegistryItem : TranslatableRegistryItem<PhaseSecurity, PhaseSecurity>
	{
		public PhaseSecurityRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, CodeDescriptionPairList ruleLocations, PhaseDependantsProvider dependantsProvider)
			: base(new RegistryItemImpl(name, category, caption, hint, new PhaseSecurityRegistryDataType(ruleLocations, dependantsProvider), RegistryStorageFlags.System, new PhaseSecurity(ruleLocations, dependantsProvider)))
		{
		}

		public override bool IsTranslatable
		{
			get { return true; }
		}

		protected override PhaseSecurity Convert(PhaseSecurity value)
		{
			foreach (Phase item in value.Phases)
			{
				item.Description = GetMultilingualString(item.EnglishDescription);
			}
			return value;
		}

		public override IEnumerable<ResourceString> DefaultStrings
		{
			get { return System.Array.Empty<ResourceString>(); }
		}

		public override IEnumerable<string> GetCaptions(PhaseSecurity value)
		{
			foreach (Phase item in value.Phases)
			{
				yield return item.EnglishDescription;
			}
		}

		public override int MaxLength
		{
			get { return 256; }
		}
	}

	[RegistryEditor("Enterprise.Freight.Forwarding.GUI.PhaseSecurityRegistryItemEditor, Enterprise.Freight.Forwarding.GUI")]
	class PhaseSecurityRegistryDataType : NonPersistentBusinessObjectRegistryDataType<PhaseSecurity>
	{
		public PhaseSecurityRegistryDataType()
			: this(new CodeDescriptionPairList(), null)
		{
		}

		public PhaseSecurityRegistryDataType(CodeDescriptionPairList ruleLocations, PhaseDependantsProvider dependantsProvider)
		{
			this.ruleLocations = ruleLocations;
			this.DependantsProvider = dependantsProvider;
		}

		protected override PhaseSecurity DeserialiseCore(byte[] value)
		{
			PhaseSecurity result = base.DeserialiseCore(value);
			result.RuleLocations.AddRange(RuleLocations);
			result.DependantsProvider = DependantsProvider;

			return result;
		}

		protected CodeDescriptionPairList RuleLocations
		{
			get { return ruleLocations ?? (ruleLocations = new CodeDescriptionPairList()); }
		}
		CodeDescriptionPairList ruleLocations;

		protected PhaseDependantsProvider DependantsProvider { get; set; }
	}
}
