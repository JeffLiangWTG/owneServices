using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.GUI.Testing
{
	[TestedType(typeof(HarmonisedCodeForm))]
	sealed class HarmonisedCodeFormTest : ZFormBasherTest
	{
		protected override System.Windows.Forms.Form GetFormToBashCore()
		{
			return new HarmonisedCodeForm(Factory.New<DummyWithHarmonisedCodes>());
		}

		class DummyWithHarmonisedCodes : PackLine
		{
			public DummyWithHarmonisedCodes(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public new JobPackLineHarmonisedCodeCollection HarmonisedCodes
			{
				get
				{
					if (harmonisedCodes == null)
					{
						harmonisedCodes = new JobPackLineHarmonisedCodeCollection(this);
					}
					return harmonisedCodes;
				}
			}
			JobPackLineHarmonisedCodeCollection harmonisedCodes;
		}
	}
}
