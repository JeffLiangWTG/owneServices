using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[DependentBusinessObject(typeof(AccGLHeader), "AlternateGLAccountDissections")]
	public class AccAlternateGLAccountDissection : AutoAccAlternateGLAccountDissection
	{
		public AccAlternateGLAccountDissection(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[List("Lookups.AttributeList")]
		public override ZString ADC_Attribute
		{
			get
			{
				return base.ADC_Attribute;
			}
			set
			{
				base.ADC_Attribute = value;

				if (GLHeader != null)
				{
					var dissections = GLHeader.AlternateGLAccountDissections;
					if (!IsValidationSuspended)
					{
						dissections.Cast<AccAlternateGLAccountDissection>().ForEach(x => x.Validation.ValidateADC_Attribute());
					}
				}
			}
		}

		public override ZBool ADC_SeparateNumbering
		{
			get { return base.ADC_SeparateNumbering; }
			set
			{
				base.ADC_SeparateNumbering = value;

				if (GLHeader != null)
				{
					var dissections = GLHeader.AlternateGLAccountDissections;
					if (!IsValidationSuspended)
					{
						dissections.Cast<AccAlternateGLAccountDissection>().ForEach(x => x.Validation.ValidateADC_SeparateNumbering());
					}
				}
			}
		}

		public string AttributeDescription => Lookups.AttributeList.GetDescriptionFromCode(ADC_Attribute);

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return Res.GetString("85575764-C745-40AB-B4DB-B80E8876EC0D", "Dissection Configuration");
			}
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			ResetOriginalHasLinkedAlternateAccounts();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (saveSucceeded)
			{
				ResetOriginalHasLinkedAlternateAccounts();
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			ResetOriginalHasLinkedAlternateAccounts();
			base.RunPreSaveValidationCore();
		}

		public override bool CanDelete => !OriginalHasLinkedAlternateAccounts;

		public bool OriginalHasLinkedAlternateAccounts => originalHasLinkedAlternateAccounts ??= HasLinkedAlternateAccountsCore(ADC_AG_GLHeader, (ZGuid)ADC_AAC_AlternateChartInfo.OriginalValue);

		public void ResetOriginalHasLinkedAlternateAccounts()
		{
			originalHasLinkedAlternateAccounts = null;
		}

		public bool HasLinkedAlternateAccounts => HasLinkedAlternateAccountsCore(ADC_AG_GLHeader, ADC_AAC_AlternateChart);

		bool? originalHasLinkedAlternateAccounts;

		bool HasLinkedAlternateAccountsCore(ZGuid glHeaderPk, ZGuid alternateChartPk)
		{
			if (glHeaderPk.IsEmpty || alternateChartPk.IsEmpty)
			{
				return false;
			}

			var query = new ZQuery();
			query.AddToFilter(AccAlternateGLAccountAttributeSchema.AAA_AG_GLHeader, glHeaderPk);
			query.AddToFilter(AccAlternateGLAccountAttributeSchema.AAA_AAC_AlternateChart, alternateChartPk);
			return Factory.Exists(typeof(AccAlternateGLAccountAttribute), query);
		}

		public override MultilingualString ReasonForNotAbleToDelete => ResString.GetMultilingualString(
			"EF0AE6BA-EC20-41BB-BE24-42479BE1FD0",
			"You cannot delete the attribute of this Chart because there is at least one Alternate Account relating to the existing attribute combinations.\r\nYou can delete all Alternate Accounts currently linked to the Parent Account<{0}>, then adjust the dissection configuration.\r\nOtherwise, please create a new Alternate Chart and new set of Alternate Accounts.", GLHeader.AccountNum);
	}
}
