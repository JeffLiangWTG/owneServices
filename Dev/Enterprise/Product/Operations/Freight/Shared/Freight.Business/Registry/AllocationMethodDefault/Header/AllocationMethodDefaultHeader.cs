using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Business
{
	[XmlSerializerAssembly("Enterprise.Freight.XmlSerializers")]
	public class AllocationMethodDefaultHeader : AutoAllocationMethodDefaultHeader
	{
		public AllocationMethodDefaultHeader() { }

		public AllocationMethodDefaultHeader(BusinessObjectFactory factory)
			: base(factory) { }

		[List("Lookups.AllocationMethods")]
		public override ZString DefaultAllocationMethod
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.DefaultAllocationMethod; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.DefaultAllocationMethod = value; }
		}

		public override void ValidateDefaultAllocationMethod()
		{
			base.ValidateDefaultAllocationMethod();
			MandatoryValidation.CheckEntered(DefaultAllocationMethodInfo);
			ListValidation.ErrorIfInvalidCode(DefaultAllocationMethodInfo, Lookups.AllocationMethods);
		}

		[ChildEditable(true)]
		public AllocationMethodDefaultRuleCollection Rules
		{
			get
			{
				if (rules == null)
				{
					rules = new AllocationMethodDefaultRuleCollection(CurrentFactory);
					RegisterEditableChildObject(rules);
				}

				return rules;
			}
		}
		AllocationMethodDefaultRuleCollection rules;

		public AllocationMethodDefaultHeaderLookups Lookups
		{
			get { return lookups ?? (lookups = new AllocationMethodDefaultHeaderLookups(this)); }
		}
		AllocationMethodDefaultHeaderLookups lookups;

		public new BusinessObjectFactory CurrentFactory
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.CurrentFactory; }
		}

		public string GetAllocationMethod(string countryCode)
		{
			foreach (AllocationMethodDefaultRule rule in Rules)
			{
				if (rule.CountryCode == countryCode)
				{
					return rule.AllocationMethod;
				}
			}

			return DefaultAllocationMethod;
		}

		#region Implementation

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			DefaultAllocationMethod = AllocationMethodList.Codes.NotSet;
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new AllocationMethodDefaultHeader(factory);
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			AllocationMethodDefaultHeader headerClone = (AllocationMethodDefaultHeader)clone;
			base.CopyValuesToClone(headerClone);

			foreach (AllocationMethodDefaultRule rule in Rules)
			{
				AllocationMethodDefaultRule ruleClone = headerClone.Rules.AddNew();
				ruleClone.CountryCode = rule.CountryCode;
				ruleClone.AllocationMethod = rule.AllocationMethod;
			}
		}

		protected override void ReadRules(XmlReader reader)
		{
			Rules.RemoveAll();

			if (reader.IsEmptyElement)
			{
				reader.Skip();
			}
			else
			{
				reader.ReadStartElement();

				while (reader.IsStartElement("Rule"))
				{
					AllocationMethodDefaultRule rule = Rules.AddNew();
					((IXmlSerializable)rule).ReadXml(reader);
				}

				reader.ReadEndElement();
			}
		}

		protected override void WriteRules(XmlWriter writer)
		{
			foreach (AllocationMethodDefaultRule rule in Rules)
			{
				writer.WriteStartElement("Rule");
				((IXmlSerializable)rule).WriteXml(writer);
				writer.WriteEndElement();
			}
		}

		#endregion
	}
}
