using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.AccAlternateGLAccountDissectionLookups;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.MasterFiles.Business
{
	public class AccAlternateGLAccountDissectionValidation : AutoAccAlternateGLAccountDissectionValidation
	{
		public AccAlternateGLAccountDissectionValidation(AutoAccAlternateGLAccountDissection parent) : base(parent)
		{
		}

		protected override void CheckADC_AAC_AlternateChart()
		{
			if (Parent.ADC_AAC_AlternateChartInfo.HasChanges && Parent.OriginalHasLinkedAlternateAccounts)
			{
				Parent.ADC_AAC_AlternateChartInfo.AddError(CannotChangeChartErrorMessage);
			}

			if (!Parent.ADC_AAC_AlternateChartInfo.HasErrors() && (Parent.ADC_AAC_AlternateChartInfo.HasChanges || !Parent.IsInDatabase) && Parent.HasLinkedAlternateAccounts)
			{
				Parent.ADC_AAC_AlternateChartInfo.AddError(CannotAddErrorMessage);
			}

			if (!Parent.ADC_AAC_AlternateChartInfo.HasErrors() && (Parent.AlternateChart == null || !Parent.AlternateChart.IsInDatabase))
			{
				Parent.ADC_AAC_AlternateChartInfo.AddError(Res.GetString("97A8F56D-3A2B-4CDA-AC99-80C78B24A6EC", "Enter a valid Chart."));
			}
		}

		#region Check ADC_Attribute

		protected override void CheckADC_Attribute()
		{
			var propertyDescription = (IMultilingualString)ResString.GetMultilingualString("EB6754B0-9C25-459D-9FEE-CF68D8FACD89", "Attribute");
			MandatoryValidation.CheckEntered(Parent.ADC_AttributeInfo, propertyDescription);

			if (Parent.ADC_AttributeInfo.HasChanges && Parent.OriginalHasLinkedAlternateAccounts)
			{
				Parent.ADC_AttributeInfo.AddError(CannotChangeAttrbuteErrorMessage);
			}

			if (!Parent.ADC_AttributeInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(Parent.ADC_AttributeInfo, propertyDescription);
			}

			if (!Parent.ADC_AttributeInfo.HasErrors() && Parent.AlternateChart != null && Parent.AlternateChart.AAC_IsGlobal && AccAlternateGLAccountDissectionLookups.GetNonGlobalAttributeList().ContainsCode(Parent.ADC_Attribute))
			{
				Parent.ADC_AttributeInfo.AddError(Res.GetString("300AF085-C29C-4519-9731-7BE5A2534602", "This is a company-specific dissection attribute that cannot be selected for a Global chart."));
			}

			if (!Parent.ADC_AttributeInfo.HasErrors() && Parent.GLHeader != null)
			{
				var parentGLHeaderPK = Parent.GLHeader.PK.ToGuid();
				var dissectionsForGLAccount = Parent.GLHeader.AlternateGLAccountDissections;
				if (dissectionsForGLAccount.Cast<AccAlternateGLAccountDissection>().Any(x => x.PK != Parent.PK && x.ADC_AAC_AlternateChart == Parent.ADC_AAC_AlternateChart && x.ADC_Attribute == Parent.ADC_Attribute))
				{
					Parent.ADC_AttributeInfo.AddError(Res.GetString("813249FA-94AE-4E21-B059-0D4FBA7B7675", "Chart and Attribute values combination should not be duplicated."));
				}

				if (Parent.ADC_Attribute == NonGlobalAttributeCode.ORG && ARAPControlAccounts.Contains(parentGLHeaderPK) && dissectionsForGLAccount.Cast<AccAlternateGLAccountDissection>().Any(x => x.PK != Parent.PK && x.ADC_AAC_AlternateChart == Parent.ADC_AAC_AlternateChart))
				{
					Parent.ADC_AttributeInfo.AddError(Res.GetString("D23AE118-8E9F-4679-80B3-3003DFA7F8A0", "The 'ORG' attribute cannot be used in combination with other attributes."));
				}

				if (!Parent.ADC_AttributeInfo.HasErrors() && Parent.ADC_Attribute == NonGlobalAttributeCode.ORG && !ARAPControlAccounts.Contains(parentGLHeaderPK))
				{
					Parent.ADC_AttributeInfo.AddError(Res.GetString("58C2B09C-BF7B-4BB2-8CA3-75888938A4C4", "The 'ORG' attribute can only be used for GL Accounts specified in Accounting > General Ledger Defaults > Control Account > AR Control Account OR AP Control Account."));
				}

				if (!Parent.ADC_AttributeInfo.HasErrors() && Parent.ADC_Attribute == AlternateGLAccountAttributeCode.TIC && !GSTRegistryItems.Contains(parentGLHeaderPK))
				{
					Parent.ADC_AttributeInfo.AddError(Res.GetString("EE66D986-3018-4743-AE7B-53FDB0E24A25", "The 'TIC' attribute can only be used for GL Account specified in Accounting > General Ledger Defaults > Control Account > Pending Tax Input Control Account or Pending Tax Output Control Account or Reportable Tax Input Control Account or Reportable Tax Output Control Account."));
				}

				if (!Parent.ADC_AttributeInfo.HasErrors() && Parent.ADC_Attribute == AlternateGLAccountAttributeCode.SPR && ARAPControlAccounts.Contains(parentGLHeaderPK))
				{
					Parent.ADC_AttributeInfo.AddError(Res.GetString("AE8A58C8-3839-4986-ADBD-9604DE6FB413", "The 'SPR' attribute can not be used for GL Accounts specified in Accounting > General Ledger Defaults > Control Account > AR Control Account OR AP Control Account."));
				}
			}
		}

		readonly Guid[] ARAPControlAccounts =
		{
			ObjectFactory.Get<IAccounting>().ARControlAccount,
			ObjectFactory.Get<IAccounting>().APControlAccount,
		};

		readonly Guid[] GSTRegistryItems =
		{
			ObjectFactory.Get<IAccounting>().GSTInputControlAccount,
			ObjectFactory.Get<IAccounting>().GSTOutputControlAccount,
			ObjectFactory.Get<IAccounting>().PendingGSTInputControlAccount,
			ObjectFactory.Get<IAccounting>().PendingGSTOutputControlAccount,
		};

		#endregion

		protected override void CheckADC_SeparateNumbering()
		{
			if (Parent.GLHeader != null)
			{
				if (Parent.ADC_SeparateNumberingInfo.HasChanges && Parent.OriginalHasLinkedAlternateAccounts)
				{
					Parent.ADC_SeparateNumberingInfo.AddError(CannotChangeSeparateNumberingMessage);
				}

				var dissectionsForGLAccount = Parent.GLHeader.AlternateGLAccountDissections;
				if (dissectionsForGLAccount.Cast<AccAlternateGLAccountDissection>().Any(x => x.PK != Parent.PK && x.ADC_AAC_AlternateChart == Parent.ADC_AAC_AlternateChart && x.ADC_Attribute != Parent.ADC_Attribute && x.ADC_SeparateNumbering != Parent.ADC_SeparateNumbering))
				{
					Parent.ADC_SeparateNumberingInfo.AddError(Res.GetString("7B31676D-5469-4E8F-AEAC-C618C19E943C", "The \"Separate Numbering\" setting for all attributes of an alternate chart must be the same."));
				}

				if (!Parent.ADC_SeparateNumberingInfo.HasErrors() && ObjectFactory.Get<IAccounting>().IsNotAllowedForSeparateNumbering(Parent.ADC_AG_GLHeader) && Parent.ADC_SeparateNumbering)
				{
					Parent.ADC_SeparateNumberingInfo.AddError(Res.GetString("10944A58-3AF9-4273-9F75-A1B646C078EC", "The \"Separate Numbering\" setting is not allowed in this GL Account."));
				}
			}
		}

		ResourceString CannotChangeSeparateNumberingMessage => ResString.GetMultilingualString("8DD2ED35-6C8F-4897-ACCF-10677A2BF863",
			"{0}\r\nYou can delete all Alternate Accounts currently linked to the Parent Account<{1}>, then adjust the dissection configuration.\r\nOtherwise, please create a new Alternate Chart and new set of Alternate Accounts.",
			Parent.ADC_SeparateNumbering ? (NoResString)"You cannot tick 'Separate Numbering' check box of this Chart because there is one Alternate Account relating to the GL Account." : (NoResString)"You cannot untick 'Separate Numbering' check box of this Chart because there is at least one Alternate Account relating to the existing attribute combinations.",
			Parent.GLHeader.AccountNum);

		ResourceString CannotChangeAttrbuteErrorMessage => ResString.GetMultilingualString(
			"2F0DE686-88E9-4334-8E25-01705C662C58",
			"You cannot change the attribute to this Chart because there is at least one Alternate Account relating to the existing attribute combinations.\r\nYou can delete all Alternate Accounts currently linked to the Parent Account<{0}>, then adjust the dissection configuration.\r\nOtherwise, please create a new Alternate Chart and new set of Alternate Accounts.",
			Parent.GLHeader.AccountNum);

		ResourceString CannotChangeChartErrorMessage => ResString.GetMultilingualString(
			"63F9405D-8DD6-4FF4-9A82-26527D5AE248",
			"You cannot change the chart to this Chart because there is at least one Alternate Account relating to the existing attribute combinations.\r\nYou can delete all Alternate Accounts currently linked to the Parent Account<{0}>, then adjust the dissection configuration.\r\nOtherwise, please create a new Alternate Chart and new set of Alternate Accounts.",
			Parent.GLHeader.AccountNum);

		ResourceString CannotAddErrorMessage => ResString.GetMultilingualString(
			"F8DE0701-C5C5-4E9A-A1CC-FCA25B043EF4",
			"You cannot add a new attribute to this Chart because there is at least one Alternate Account relating to the existing attribute combinations.\r\nYou can delete all Alternate Accounts currently linked to the Parent Account<{0}>, then adjust the dissection configuration.\r\nOtherwise, please create a new Alternate Chart and new set of Alternate Accounts.",
			Parent.GLHeader.AccountNum);

		protected new AccAlternateGLAccountDissection Parent => (AccAlternateGLAccountDissection)base.Parent;
	}
}
