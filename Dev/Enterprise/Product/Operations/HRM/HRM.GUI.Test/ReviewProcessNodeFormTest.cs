using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.HRM.Common;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.HRM.GUI.Test
{
	[TestedType(typeof(ReviewProcessNodeForm))]
	public class ReviewProcessNodeFormTest : ZFormBasherTest
	{
		public void TestNoEdocs()
		{
			using var form = GetFormToBash();
			Assert("Disallow any viewing or editing of edocs through CW1, users must use HRMS and its appropriate entity security", !form.PlugIns.IsPlugInAvailable(ControllerIDs.eDocsPlugIn));
		}

		protected new ReviewProcessNodeForm GetFormToBash()
			=> (ReviewProcessNodeForm)base.GetFormToBash();

		protected override Form GetFormToBashCore()
		{
			var bizo = Factory.NewWithValidTestData<ReviewProcessNode>();
			Factory.Save();

			return new ReviewProcessNodeForm(bizo);
		}

		public void TestNoMentionOfNodes()
		{
			var node = Factory.NewWithValidTestData<ReviewProcessNode>();
			using (var form = new ReviewProcessNodeFormForTest(node))
			{
				form.Show();
				AssertEquals("Manager Review", form.CaptionResourceString.Caption);
				AssertEquals("Review", form.GuidFindBoxControlsExposed[0].CaptionResourceString.Caption);
				AssertEquals("Parent Review", form.GuidFindBoxControlsExposed[1].CaptionResourceString.Caption);
			}
		}

		class ReviewProcessNodeFormForTest : ReviewProcessNodeForm
		{
			public ReviewProcessNodeFormForTest(ReviewProcessNode reviewProcessNode)
				: base(reviewProcessNode)
			{
			}

			internal IReadOnlyList<ZGuidFindBox> GuidFindBoxControlsExposed
			{
				get { return new List<ZGuidFindBox>() { ReviewProcessFindBox, ParentReviewNodeFindBox }; }
			}
		}
	}
}
