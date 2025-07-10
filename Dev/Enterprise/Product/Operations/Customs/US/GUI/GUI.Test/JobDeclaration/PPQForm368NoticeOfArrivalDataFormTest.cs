using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(PPQForm368NoticeOfArrivalDataForm))]
	sealed class PPQForm368NoticeOfArrivalDataFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			var noticeOfArrivalData = new PPQForm368NoticeOfArrivalData(declaration);
			return new PPQForm368NoticeOfArrivalDataForm(noticeOfArrivalData);
		}
	}
}
