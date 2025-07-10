using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface IComplianceSequencePresentationProvider
	{
		ZString CanReactivate(AccComplianceSequence complianceSequence);
	}

	public class ComplianceSequencePresentationProvider : IComplianceSequencePresentationProvider
	{
		public ZString CanReactivate(AccComplianceSequence complianceSequence)
		{
			if (complianceSequence == null)
			{
				return ZString.Empty;
			}

			if (complianceSequence.XD_PermanentDisableTimeUtc.IsValid)
			{
				return Res.GetString("BF18BCA6-2C37-412C-BDF2-7BA2B809129C", "The database was restored recently. According to compliance policies, this Compliance Sequence Book was disabled. And it cannot be reactivated.");
			}

			if (!AccountingMasterFilesRegistry.Instance.AllowReActivationOfInactiveComplianceBook.Value)
			{
				return Res.GetString("8b695e8a-5475-4fd9-ac4e-954c2eae6a89", @"Compliance book {0} cannot be activated due to registry setting.
This is controlled by this registry: {1}.", complianceSequence.XD_Code, AccountingMasterFilesRegistry.Instance.AllowReActivationOfInactiveComplianceBook.Location());
			}

			return ZString.Empty;
		}
	}
}
