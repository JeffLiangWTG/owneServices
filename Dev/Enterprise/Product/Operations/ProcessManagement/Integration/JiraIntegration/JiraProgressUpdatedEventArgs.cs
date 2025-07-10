using System;

namespace Enterprise.ProcessManagement.Integration
{
	public class JiraProgressUpdatedEventArgs : EventArgs
	{
		public string Status { get; }
		public int Percentage { get; }

		public JiraProgressUpdatedEventArgs(string status, int percentage)
		{
			Status = status;
			Percentage = percentage;
		}
	}
}
