namespace Enterprise.MasterFiles.Integration
{
	using CargoWise.Types;

	public interface ICertificateTypeCollection
	{
		void AddNew(ZString code, ZString description, bool isMandatory, bool isUnique, bool isSystem, ZString alertType);
	}
}
