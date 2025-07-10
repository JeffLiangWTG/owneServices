using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public sealed class USCRule : AutoUSCRule
	{
		public USCRule(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZString U0_Code
		{
			get { return base.U0_Code; }
			set
			{
				bool hasChanged = base.U0_Code != value;
				base.U0_Code = value;
				if (hasChanged)
				{
					fRuleCodeDescription = null;
				}
			}
		}
		public ZString RuleCodeDescription
		{
			get
			{
				if (fRuleCodeDescription == null)
				{
					fRuleCodeDescription = new TariffRuleList().GetDescriptionFromCode(U0_Code);
					if (fRuleCodeDescription == null)
					{
						fRuleCodeDescription = string.Empty;
					}
				}
				return fRuleCodeDescription;
			}
		}
		string fRuleCodeDescription;

		public ZPropertyInfo RuleCodeDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(RuleCodeDescription)); }
		}

		[ChildEditable(true)]
		public USCTariffRuleCollection Tariffs
		{
			get
			{
				if (fTariffRules == null)
				{
					fTariffRules = new USCTariffRuleCollection(this);
					RegisterEditableChildObject(fTariffRules);
				}
				return fTariffRules;
			}
		}
		USCTariffRuleCollection fTariffRules;

		public override void Delete()
		{
			Tariffs.DeleteAll();
			base.Delete();
		}

		public override bool ReadOnly
		{
			get { return true; }
		}

		protected override ZString HumanReadableNameCore => Res.GetString("BC1F86AF-7A22-4BE2-A543-FFC2A8BEAA90", "Rule {0}", U0_Code);
	}
}
