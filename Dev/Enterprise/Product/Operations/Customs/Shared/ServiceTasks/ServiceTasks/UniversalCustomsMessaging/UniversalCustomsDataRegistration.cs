using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging
{
	internal class UniversalCustomsDataRegistration<T, TAttribute>
		where TAttribute : AssemblyMetaDataAttributeWithTypeAndApplicationCode
	{
		public string ApplicationCode { get; }
		public IDataCreator<T> Creator { get; }

		public UniversalCustomsDataRegistration(string applicationCode, IDataCreator<T> creator)
		{
			ApplicationCode = applicationCode;
			Creator = creator;
		}

		internal static IEnumerable<UniversalCustomsDataRegistration<T, TAttribute>> GetAssemblyRegistrations() => AssemblyMetaDataReader.GetAttributes<TAttribute>()
					.Select(attribute =>
						new UniversalCustomsDataRegistration<T, TAttribute>(
							attribute.ApplicationCode,
							new UniversalCustomsDataCreator<T>(attribute)));

		internal static IEnumerable<UniversalCustomsDataRegistration<T, TAttribute>> AssertUniqueApplicationCodes(IEnumerable<UniversalCustomsDataRegistration<T, TAttribute>> registrations)
		{
			var allRegistrations = registrations.ToList();
			var duplicatingRegistrations = allRegistrations
				.Select(x => new { ApplicationCode = x.ApplicationCode.ToUpperInvariant(), ProcessorRegistration = x })
				.GroupBy(
					x => x.ApplicationCode,
					x => x.ProcessorRegistration,
					(groupApplicationCode, groupProcessorRegistrations) =>
						new
						{
							ApplicationCode = groupApplicationCode,
							Count = groupProcessorRegistrations.Count(),
							ProcessorRegistrations = groupProcessorRegistrations.ToArray()
						})
				.Where(g => g.Count > 1)
				.Select(g => $"{g.ApplicationCode} ({string.Join(",", g.ProcessorRegistrations.Select(x => x.Creator.Create().GetType().FullName))})")
				.ToList();

			return duplicatingRegistrations.Count == 0
				? allRegistrations
				: throw new DuplicatingUCMCodeException(string.Join("; ", duplicatingRegistrations), typeof(TAttribute), "ApplicationCode");
		}
	}
}
