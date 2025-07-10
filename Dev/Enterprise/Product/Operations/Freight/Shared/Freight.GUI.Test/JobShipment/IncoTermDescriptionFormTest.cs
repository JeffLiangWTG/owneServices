using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.GUI.Testing
{
	[TestedType(typeof(IncoTermDescriptionForm))]
	sealed class IncoTermDescriptionFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new IncoTermDescriptionForm("EXW");
		}

		public override bool AllowUntranslatableFormTitle()
		{
			return true;
		}

		[TestDate(2011, 1, 1)]
		public void TestDescriptionAndWithoutURL_Post2011()
		{
			var incotermCodePairs = new IncoTermsCodeDescriptionPairList();

			using (IncoTermDescriptionForm form = new IncoTermDescriptionForm("ZUB"))
			{
				AssertEquals("Title incorrect.", "Incoterms", form.Title);
				AssertEquals("Description incorrect.", "A blank or invalid Incoterm was specified.", form.Description);
			}

			foreach (CodeDescriptionPair incotermPair in incotermCodePairs)
			{
				using (var form = new IncoTermDescriptionForm(incotermPair.Code))
				{
					AssertEquals("Title incorrect.", GetTitleForForm(incotermPair.Code), form.Title);
					AssertEquals("Description incorrect.", "For more information on Incoterms refer to the International Chamber of Commerce website.", form.Description);
				}
			}

			string GetTitleForForm(string code)
			{
				return IncotermValidation.Instance.IncotermsExpiringIn2011.Contains(code)
					? (code + " (obsolete) - " + incotermCodePairs.GetDescriptionFromCode(code))
					: (code + " - " + incotermCodePairs.GetDescriptionFromCode(code));
			}
		}
	}
}
