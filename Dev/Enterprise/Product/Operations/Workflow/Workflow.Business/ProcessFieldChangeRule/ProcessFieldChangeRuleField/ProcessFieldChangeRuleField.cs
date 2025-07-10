using System.ComponentModel;
using System.Data;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Workflow.Integration;

namespace Enterprise.Workflow.Business
{
	public class ProcessFieldChangeRuleField : AutoProcessFieldChangeRuleField, IProcessFieldChangeRuleField
	{
		public ProcessFieldChangeRuleField(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[RelatedBusinessObject("Parent")]
		public override ZGuid PFL_PFR { get => base.PFL_PFR; set => base.PFL_PFR = value; }

		[List("Lookups.FieldAndTableNames")]
		public override ZString PFL_FieldName
		{
			get => base.PFL_FieldName;
			set
			{
				var i = Lookups.Fields.IndexOfCode(value);
				if (i != -1)
				{
					base.PFL_FieldName = Lookups.Fields[i].Code; //ensure case matches schema
				}
				else
				{
					base.PFL_FieldName = value;
				}
			}
		}

		public ZString PFL_TableCode
		{
			get
			{
				var split = PFL_FieldName.Split("_");
				return split.Length >= 2 ? split[0] : ZString.Empty;
			}
		}

		public bool IsBlacklisted
		{
			get
			{
#if DEBUG
				if (TurnOffBlacklistForTest.Value)
				{ return false; }
#endif

				return !Lookups.Fields.ContainsCode(PFL_FieldName);
			}
		}
		public ZString FieldDisplayName => Lookups.Fields.GetDescriptionFromCode(PFL_FieldName);

		public ZString TableDisplayName => Lookups.Tables.GetDescriptionFromCode(PFL_FieldName);

		public ProcessFieldChangeRule Parent => Factory.Load<ProcessFieldChangeRule>(PFL_PFR);

		public override string ToString() => FieldDisplayName;

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			PFL_FieldName = "ZZ_Description";
			base.FillWithValidTestDataCore(kind, propertyPath);
		}

		public static Overridable<bool> TurnOffBlacklistForTest { get; } = new Overridable<bool>(false);
#endif
	}
}
