using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI;

public sealed class ValidationToolDialogService : IValidationToolDialogService
{
	public bool Proceed(NonPersistentValidationFailure validationFailure)
	{
		if (validationFailure is null)
		{
			return false;
		}

		var popup = new ValidationToolFailurePopup(validationFailure);
		return ZFormModaliser.ShowDialogAndDispose(popup) == DialogResult.Yes;
	}
}
