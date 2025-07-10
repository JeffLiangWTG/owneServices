using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.GUI.Testing
{
	[TestedType(typeof(MiscOptionsControlBag))]
	sealed class MiscOptionsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(MiscOptionsControlBag.ManifestToOpenUserControl);
				yield return nameof(MiscOptionsControlBag.ExportersUnionInfoUserControl);
				yield return nameof(MiscOptionsControlBag.GuaranteeInfoOptionsSeparatorUserControl);
				yield return nameof(MiscOptionsControlBag.GuaranteeGuidFindBox);
				yield return nameof(MiscOptionsControlBag.BondTypeDropEdit);
				yield return nameof(MiscOptionsControlBag.ReferenceNumberTextBox);
				yield return nameof(MiscOptionsControlBag.GuaranteeDescriptionTextBox);
				yield return nameof(MiscOptionsControlBag.DedicatedAmountCalcEdit);
				yield return nameof(MiscOptionsControlBag.RatioCalcEdit);
				yield return nameof(MiscOptionsControlBag.AmountCalcEdit);
				yield return nameof(MiscOptionsControlBag.SupportingInformationUserControl);
				yield return nameof(MiscOptionsControlBag.TotalAmountsSeparatorUserControl);
				yield return nameof(MiscOptionsControlBag.InvoiceCountCalcEdit);
				yield return nameof(MiscOptionsControlBag.TotalInvoiceAmountLocalCurrencyControl);
				yield return nameof(MiscOptionsControlBag.TotalFreeOnBoardLocalCurrencyControl);
				yield return nameof(MiscOptionsControlBag.TotalFreightLocalCurrencyControl);
				yield return nameof(MiscOptionsControlBag.TotalInsuranceLocalCurrencyControl);
				yield return nameof(MiscOptionsControlBag.TotalOverseasLocalCurrencyControl);
				yield return nameof(MiscOptionsControlBag.LocalTotalChargesLocalCurrencyControl);
			}
		}

		protected override ControlBag GetControlBagForTesting() => MiscOptionsControlBag.Instance;
	}
}
