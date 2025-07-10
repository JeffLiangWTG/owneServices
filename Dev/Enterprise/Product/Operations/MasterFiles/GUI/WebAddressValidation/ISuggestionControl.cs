namespace Enterprise.MasterFiles.GUI.WebAddressValidation
{
	public interface ISuggestionControl
	{
		void SelectAddressAndClose();
		void MoveSelection(int amountToMove);
		void Close();
	}
}
