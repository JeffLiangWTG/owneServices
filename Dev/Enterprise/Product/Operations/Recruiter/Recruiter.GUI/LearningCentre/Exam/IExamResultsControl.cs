using System;
using System.Drawing;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Recruiter.GUI
{
	interface IExamResultsControl
	{
		ZGrid AnswersGrid { get; }
	}

	static class IExamResultsControlExtensions
	{
		public static void HookColourDecidingEventHandler(this IExamResultsControl control)
		{
			control.AnswersGrid.ColourDeciding += new EventHandler<ColourDecidingEventArgs>(AnswersGrid_ColourDeciding);
		}

		public static void UnhookColourDecidingEventHandler(this IExamResultsControl control)
		{
			control.AnswersGrid.ColourDeciding -= new EventHandler<ColourDecidingEventArgs>(AnswersGrid_ColourDeciding);
		}

		static void AnswersGrid_ColourDeciding(object sender, ColourDecidingEventArgs e)
		{
			var submittedAnswer = e.ObjectAtRow as ILearningCentreSubmittedAnswer;
			if (submittedAnswer.IsPopulated)
			{
				e.Colour = (!submittedAnswer.IsAnsweredCorrectly)
					? Color.LightPink
					: Color.LightGreen;
			}
			else
			{
				e.Colour = Color.LightSteelBlue;
			}
		}
	}
}

// Tested in instances
