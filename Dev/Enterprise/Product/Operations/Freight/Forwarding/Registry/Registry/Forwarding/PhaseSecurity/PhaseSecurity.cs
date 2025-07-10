using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Freight.Forwarding.Registry
{
	[XmlSerializerAssembly("Enterprise.Freight.Forwarding.Registry.XmlSerializers")]
	public class PhaseSecurity : RegistryBusinessObjectTemplate, IPhaseSecurity
	{
		public PhaseSecurity()
			: this(new CodeDescriptionPairList())
		{
		}

		public PhaseSecurity(CodeDescriptionPairList ruleLocations)
			: this(ruleLocations, null)
		{
		}

		public PhaseSecurity(CodeDescriptionPairList ruleLocations, PhaseDependantsProvider dependantsProvider)
			: base()
		{
			this.ruleLocations = ruleLocations;
			this.DependantsProvider = dependantsProvider;
		}

		#region Schema

		public abstract class Schema
		{
			public const string IsEnabled = "IsEnabled";
		}

		#endregion

		#region IsEnabled

		public ZBool IsEnabled
		{
			get { return isEnabled; }
			set
			{
				SetNonPersistentPropertyValue(IsEnabledInfo, ref isEnabled, value);
			}
		}

		ZBool isEnabled;

		public ZPropertyInfo IsEnabledInfo
		{
			get { return GetZPropertyInfo(Schema.IsEnabled); }
		}

		#endregion

		#region Phases

		[BusinessObjectTestExclude]
		public PhaseCollection Phases
		{
			get { return phases ?? (Phases = new PhaseCollection(this)); }
			private set
			{
				phases = value;
				phases.Parent = this;
				RegisterEditableChildObject(phases);
			}
		}
		PhaseCollection phases;

		#endregion

		#region Lookups

		public CodeDescriptionPairList RuleLocations
		{
			get { return ruleLocations ?? (ruleLocations = new CodeDescriptionPairList()); }
		}
		CodeDescriptionPairList ruleLocations;

		public PhaseDependantsProvider DependantsProvider { get; internal set; }

		#endregion

		#region Xml Serialisation

		ZXmlSerializer PhasesSerialiser
		{
			get { return phasesSerialiser ?? (phasesSerialiser = ZXmlSerializer.New(typeof(PhaseCollection))); }
		}
		ZXmlSerializer phasesSerialiser;

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.IsEnabled, IsEnabled.ToString());
			PhasesSerialiser.Serialize(writer, Phases);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			IsEnabled = new ZBool(reader.ReadElementString(Schema.IsEnabled));
			Phases = (PhaseCollection)PhasesSerialiser.Deserialize(reader);
		}

		#endregion

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			PhaseSecurity result = new PhaseSecurity(RuleLocations, DependantsProvider);
			result.Phases = (PhaseCollection)Phases.Clone(fallbackLevel, factory);
			return result;
		}

		#endregion

		#region IPhaseSecurity Members

		bool IPhaseSecurity.IsEnabled
		{
			get { return IsEnabled; }
		}

		IEnumerable<IPhase> IPhaseSecurity.Phases
		{
			get { return Phases.Cast<IPhase>(); }
		}

		#endregion
	}
}
