using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging
{
	internal class CFGUniversalCustomsDataRegistration<T, TAttribute>
		where TAttribute : AssemblyMetaDataAttributeWithTypeAndMessageType
	{
		public string MessageType { get; }
		public IDataCreator<T> Creator { get; }

		public CFGUniversalCustomsDataRegistration(string messageType, IDataCreator<T> creator)
		{
			MessageType = messageType;
			Creator = creator;
		}

		internal static IEnumerable<CFGUniversalCustomsDataRegistration<T, TAttribute>> GetAssemblyRegistrations() => AssemblyMetaDataReader.GetAttributes<TAttribute>()
					.Select(attribute =>
						new CFGUniversalCustomsDataRegistration<T, TAttribute>(
							attribute.MessageType,
							new UniversalCustomsDataCreator<T>(attribute)));

		internal static IEnumerable<CFGUniversalCustomsDataRegistration<T, TAttribute>> AssertUniqueMessageTypes(IEnumerable<CFGUniversalCustomsDataRegistration<T, TAttribute>> registrations)
		{
			var allRegistrations = registrations.ToList();
			var duplicatingRegistrations = allRegistrations
				.Select(x => new { MessageType = x.MessageType.ToUpperInvariant(), ProcessorRegistration = x })
				.GroupBy(
					x => x.MessageType,
					x => x.ProcessorRegistration,
					(groupMessageType, groupProcessorRegistrations) =>
						new
						{
							MessageType = groupMessageType,
							Count = groupProcessorRegistrations.Count(),
							ProcessorRegistrations = groupProcessorRegistrations.ToArray()
						})
				.Where(g => g.Count > 1)
				.Select(g => $"{g.MessageType} ({string.Join(",", g.ProcessorRegistrations.Select(x => x.Creator.Create().GetType().FullName))})")
				.ToList();

			return duplicatingRegistrations.Count == 0
				? allRegistrations
				: throw new DuplicatingUCMCodeException(string.Join("; ", duplicatingRegistrations), typeof(TAttribute), "MessageType");
		}
	}
}
