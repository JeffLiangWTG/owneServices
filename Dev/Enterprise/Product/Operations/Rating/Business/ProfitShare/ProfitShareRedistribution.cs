using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Rating.Business
{
	public class ProfitShareRedistribution : AutoProfitShareRedistribution, IStmNoteParent
	{
		public ProfitShareRedistribution(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ConsolidationProfitShareCollection ConsolProfitShares
		{
			get
			{
				if (consolProfitShares == null)
				{
					consolProfitShares = new ConsolidationProfitShareCollection(this);
					consolProfitShares.Load();
				}
				return consolProfitShares;
			}
		}
		ConsolidationProfitShareCollection consolProfitShares;

		public ProfitShareRedistributionRuleCollection ProfitShareRules
		{
			get
			{
				if (profitShareRules == null)
				{
					profitShareRules = new ProfitShareRedistributionRuleCollection(this);
					profitShareRules.Load();
				}
				return profitShareRules;
			}
		}
		ProfitShareRedistributionRuleCollection profitShareRules;

		public override void OnSaving()
		{
			if (!IsDeleted && !IsInDatabase)
			{
				PopulateFormattedNumberPropertyIfRequired(PSR_BatchNumberInfo, Env.NumberFountains.GetProfitShareRedistributionNumber());
			}
			base.OnSaving();
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			PSR_GC_Company = GlbCompany.CurrentCompany.PK;
		}

		#region NoteTypesCore

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				NoteTypeCollection fNoteTypes = base.NoteTypesCore;
				fNoteTypes.Add(PredefinedNoteTypes.Instance.ProfitShareRedistributionAuditLog);

				return fNoteTypes;
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				if (!PSR_BatchNumber.IsEmpty)
				{
					return Enterprise.Rating.Business.Res.GetString("FE5F2A15-2A0F-4BAA-8635-DD78496DB585", "Profit Redistribution") + " " + PSR_BatchNumber;
				}

				return base.HumanReadableNameCore;
			}
		}

		public IStmNoteParent NotesParentBO
		{
			get { return this; }
		}

		#endregion

		#region Test Data
#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			PSR_BatchNumber = ZString.Empty;
			PSR_RX_NKCurrency = Env.CurrentCompany.LocalCurrency.Code;
		}
#endif
		#endregion
	}
}
