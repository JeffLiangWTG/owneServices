using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public class CO2eProgressForm : ProgressFormManager
	{
		public CO2eProgressForm(Action cancel)
		{
			IsProgressBarVisible = false;
			IsCancelButtonVisible = true;
			Cancelled += (sender, e) => cancel();
			Start();
		}
	}
}
