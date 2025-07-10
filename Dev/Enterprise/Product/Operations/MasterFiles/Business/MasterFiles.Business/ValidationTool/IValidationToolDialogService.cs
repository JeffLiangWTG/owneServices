namespace Enterprise.MasterFiles.Business;

public interface IValidationToolDialogService
{
	bool Proceed(NonPersistentValidationFailure validationFailure);
}
