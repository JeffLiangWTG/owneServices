using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Freight.Forwarding.Registry
{
	[XmlSerializerAssembly("Enterprise.Freight.Forwarding.Registry.XmlSerializers")]
	public class Phase : RegistryBusinessObject, IPhase
	{
		public Phase()
		{
		}

		public Phase(PhaseSecurity parent)
		{
			Parent = parent;
		}

		public override MultilingualString ReasonForNotAbleToDelete => reasonForNotAbleToDelete;
		MultilingualString reasonForNotAbleToDelete;

		public override bool CanDelete => !HasDPSUpdateDefined;

		public PhaseSecurity Parent { get; set; }

		#region Phase Rules

		[BusinessObjectTestExclude]
		public PhaseRuleCollection Rules
		{
			get { return rules ?? (Rules = new PhaseRuleCollection(this)); }
			private set
			{
				rules = value;
				rules.Parent = this;
				RegisterEditableChildObject(rules);
			}
		}
		PhaseRuleCollection rules;

		#endregion

		#region Validation

		protected override void ValidateCodeCore()
		{
			if (Code.Length < 3)
			{
				CodeInfo.AddError(Res.GetString("eddf8a97-24a7-425b-9720-94cf588c3ccd", "Code should have 3 letters."));
			}

			if (Code == PhaseConstants.Phase.ALL)
			{
				CodeInfo.AddError(Res.GetString("f3fa06a2-720e-4616-ade2-48986b19fd7c", "You can't customize security for the {0} code.", Code));
			}
		}

		protected override void ValidateDescriptionCore()
		{
			MandatoryValidation.CheckEntered(DescriptionInfo, Res.GetString("cbf0184c-1323-4c79-8d58-d9f3bb3dd90b", "Phase Name"));
		}

		protected override void RunPreSaveValidationCore()
		{
			ClearRowNotifications();
			base.RunPreSaveValidationCore();

			if (Rules.Count == 0)
			{
				AddRowError(Res.GetString("e85552c7-6869-46db-9164-26042a2abb7d", "You should have at least one rule for the phase."));
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get { return (NoResString)"Phase"; }     // HumanReadableName identifier
		}

		#endregion

		#region Xml Serialisation

		ZXmlSerializer RulesSerialiser
		{
			get { return rulesSerialiser ?? (rulesSerialiser = ZXmlSerializer.New(typeof(PhaseRuleCollection))); }
		}
		ZXmlSerializer rulesSerialiser;

		protected override void WriteMoreElements(XmlWriter writer)
		{
			RulesSerialiser.Serialize(writer, Rules);
		}

		protected override void ReadMoreElements(XmlReader reader)
		{
			Rules = (PhaseRuleCollection)RulesSerialiser.Deserialize(reader);
		}

		#endregion

		#region Implementation

		protected override int MaxDescriptionLength
		{
			get { return 256; }
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			Phase result = new Phase(Parent);
			result.Rules = (PhaseRuleCollection)Rules.Clone(fallbackLevel, factory);
			return result;
		}

		#endregion

		#region IPhase Members

		ZString IPhase.Code
		{
			get { return Code; }
		}

		ZString IPhase.Description
		{
			get { return Description; }
		}

		IEnumerable<IPhaseRule> IPhase.Rules
		{
			get { return Rules.Cast<IPhaseRule>(); }
		}

		public bool HasDPSUpdateDefined
		{
			get
			{
				var result = false;
				if (Parent != null && Parent.DependantsProvider != null)
				{
					DpsStatusUpdateSettingRegistryItem dpsRegistryItem = null;
					if (Parent.DependantsProvider is ConsolPhaseDependantsProvider)
					{
						dpsRegistryItem = ForwardingConfigurationRegistry.Instance.ConsolDpsStatusUpdateSetting;
					}
					else if (Parent.DependantsProvider is ShipmentPhaseDependantsProvider)
					{
						dpsRegistryItem = ForwardingConfigurationRegistry.Instance.ShipmentDpsStatusUpdateSetting;
					}
					else
					{
						var key = GetType().FullName + "PhaseParentTypeUnKnown";
						ErrorReporter.ReportOnce(key, "Parent Phase Security DependantsProvider type should be either ConsolPhaseDependantsProvider or ShipmentPhaseDependantsProvider.");
						return result;
					}

					if (dpsRegistryItem.Value.JobUpdateSettings.Cast<JobPhaseSetting>().Any(s => s.ShouldUpdate && s.Code == Code))
					{
						reasonForNotAbleToDelete = ResString.GetMultilingualString("bde86c10-3c1b-44d4-9175-df080bcf3ccc", "This phase is selected as a DPS Updating Phase, please un-select it under \"{0}\" before deleting it.", ((IMultilingualRegistryItem)dpsRegistryItem).LocationMultilingual);
						result = true;
					}
				}

				return result;
			}
		}

		#endregion
	}
}
