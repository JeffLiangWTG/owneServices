using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.Business
{
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	[DependentBusinessObject(typeof(CusStatementHeader), "StatementLines")]
	public class CusStatementLine : BaseCusStatementLine, Integration.Customs.TR.ICusStatementLine
	{
		public CusStatementLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new CusStatementHeader StatementHeader
		{
			get { return (CusStatementHeader)base.StatementHeader; }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			B3_Status = StatementLineStatusList.Codes.NRL;
		}

		public new CusStatementLineLookups Lookups => (CusStatementLineLookups)base.Lookups;

		protected override Customs.Business.CusStatementLineLookups GetNewLookups() => new CusStatementLineLookups(this);

		[ResourceStringData("66A7198A-FE8B-4CE3-A5D4-84B6E986EC0C", Caption = "Job Number")]
		public override ZString B3_BrokerReference { get => base.B3_BrokerReference; set => base.B3_BrokerReference = value; }

		[ResourceStringData("16C70299-0C7F-4A95-B294-8BC3BCBD8825", Caption = "Entry Type")]
		[List(nameof(Lookups) + "." + nameof(CusStatementLineLookups.StatementLineEntryTypeList))]
		public override ZString B3_EntryType { get => base.B3_EntryType; set => base.B3_EntryType = value; }

		[ResourceStringData("1B03F333-D35E-43A0-9B6A-9495D3E08004", Caption = "Entry Number")]
		public override ZString B3_EntryNum { get => base.B3_EntryNum; set => base.B3_EntryNum = value; }

		[ResourceStringData("7E6F6F08-E15D-49BD-A7DE-2824B1B6BC34", Caption = "Entry Date")]
		public override ZDate B3_EntryDate { get => base.B3_EntryDate; set => base.B3_EntryDate = value; }

		[ResourceStringData("4F90F86C-B5A8-467D-9A6D-10BD20D34672", Caption = "Associated Entry")]
		public override ZString B3_AssociatedEntry { get => base.B3_AssociatedEntry; set => base.B3_AssociatedEntry = value; }

		[ResourceStringData("1DCB9AF9-EE02-4AB9-BDC0-BFC75BB19C85", Caption = "Line Status")]
		[List(nameof(Lookups) + "." + nameof(CusStatementLineLookups.StatementLineStatusList))]
		public override ZString B3_Status { get => base.B3_Status; set => base.B3_Status = value; }

		[ResourceStringData("91A0F402-7491-4F74-A6FF-272C3F378911", Caption = "Stamp Duty Total")]
		public override ZDecimal B3_CustomsFeesTotal { get => base.B3_CustomsFeesTotal; set => base.B3_CustomsFeesTotal = value; }

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return IsStatementLineReadOnly || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		public ZBool IsStatementLineReadOnly => !(B3_EntryType == StatementLineEntryTypeList.Codes.MIS || B3_EntryType == StatementLineEntryTypeList.Codes.ACC || B3_EntryType == ZString.Empty || B3_AssociatedEntry == ZString.Empty) && IsInDatabase;

		[ChildEditable(true)]
		public CusStatementLineChargeCollection Charges
		{
			get
			{
				if (charges == null)
				{
					charges = new CusStatementLineChargeCollection(this);
					RegisterEditableChildObject(charges);
				}
				return charges;
			}
		}
		CusStatementLineChargeCollection charges;

		public override void Delete()
		{
			Charges.DeleteAll();
			base.Delete();
		}

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(CusStatementLine statementLine)
				: base(statementLine)
			{
			}

			CusStatementLine StatementLine
			{
				get { return (CusStatementLine)BusinessObject; }
			}

			protected override void FetchForLoadChildEditableObjectsCore()
			{
				base.FetchForLoadChildEditableObjectsCore();
				Factory.AddFetchHint(CusStatementLineChargeSchema.B4_B3, StatementLine.PK);
			}
		}

		#endregion
	}
}
