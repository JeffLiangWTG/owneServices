using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface ITriggerActionProvider
	{
		ZString ReasonForDoNotTriggerAction { get; }
	}
}
