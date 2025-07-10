namespace Enterprise.MasterFiles.Business
{
	public class ProcessTaskStatusChangeModeTracker
	{
		string currentChangeModeCode;

		public ProcessTaskStatusChangeModeTracker()
		{
			Clear();
		}

		public void Clear() => currentChangeModeCode = ProcessTaskStatusChangeModeCodeList.Codes.Other;
		public void SetCurrent(string changeModeCode) => currentChangeModeCode = changeModeCode;
		public string Current => currentChangeModeCode;
	}
}
