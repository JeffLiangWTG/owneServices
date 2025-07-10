using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.eTail.DataTransfer.Testing
{
	public static class CommandTestHelper
	{
		internal static ReadOnlyCollection<BusinessObject> RunCreateCommand(BaseHVLVRelatedJobCommand command, out string error)
		{
			if (!command.CanCreate)
			{
				error = "command is not in create state";
				return EmptyCollection();
			}
			var (successful, resultCollection) = ExecuteCommandsConverter(command, out error);
			if (!successful)
			{
				return EmptyCollection();
			}
			error = string.Empty;
			return resultCollection;
		}

		internal static ReadOnlyCollection<BusinessObject> RunOpenCommand(BaseHVLVRelatedJobCommand command, out string error)
		{
			if (!command.CanOpen)
			{
				error = "command is not in open state";
				return EmptyCollection();
			}
			error = string.Empty;
			return new ReadOnlyCollection<BusinessObject>(command.ActiveRelatedCustomsJobs.ToList<BusinessObject>());
		}

		internal static ReadOnlyCollection<BusinessObject> RunSyncCommand(BaseHVLVRelatedJobCommand command, out string error)
		{
			if (!command.CanSync)
			{
				error = "command is not in sync state";
				return EmptyCollection();
			}
			var (successful, resultCollection) = ExecuteCommandsConverter(command, out error);
			if (!successful)
			{
				return EmptyCollection();
			}
			error = string.Empty;
			return resultCollection;
		}

		static (bool successful, ReadOnlyCollection<BusinessObject> resultCollection) ExecuteCommandsConverter(BaseHVLVRelatedJobCommand command, out string error)
		{
			var converter = command.Converter;
			var result = converter.TryConvert(out error);
			converter.Shipment.Factory.Save();
			return (result, converter.CustomsRelatedBusinessCollection);
		}

		static ReadOnlyCollection<BusinessObject> EmptyCollection()
		{
			return new ReadOnlyCollection<BusinessObject>(new List<BusinessObject>());
		}
	}
}
