using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	class JobDocAddressUniqueIndexFailureHandler : IUniqueIndexFailureHandler
	{
		public JobDocAddressUniqueIndexFailureHandler(JobDocAddress address)
		{
			Argument.NotNull(address, "address");
			this.address = address;
		}

		readonly JobDocAddress address;

		public IEnumerable<string> HandledUniqueIndexNames
		{
			get { yield return JobDocAddressSchema.Constants.Indexes.NR_UC__E2_ParentID_E2_AddressType_E2_AddressSequence; }
		}

		public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
		{
			var addressParent = address.Parent;
			string message = GenerateMessage();
			address.Delete();
			if (addressParent != null)
			{
				addressParent.DocAddresses.Factory.ClearQueryCache();
				addressParent.DocAddresses.Load();
			}
			notifier.ReportError(message, Res.GetString("a4b8a29d-f6b4-4f60-9a1c-bd9d84e6bcbb", "Save Error"));
		}

		string GenerateMessage()
		{
			var filter = new ZDBOnlyQuery(typeof(JobDocAddress));
			filter.AddToFilter(JobDocAddressSchema.E2_ParentID, address.E2_ParentID);
			filter.AddToFilter(JobDocAddressSchema.E2_AddressType, address.E2_AddressType);
			filter.AddToFilter(JobDocAddressSchema.E2_AddressSequence, address.E2_AddressSequence);
			var existedAddress = address.Factory.LoadTop1<JobDocAddress>(filter);

			var changedProperties = new HashSet<string>();
			if (existedAddress != null)
			{
				foreach (ZPropertyInfo existedPropertyInfo in existedAddress.ZPropertyInfoHash)
				{
					if (existedPropertyInfo.IsPersistent)
					{
						var currentPropertyInfo = address.FindPropertyInfo(existedPropertyInfo.Name);
						if (currentPropertyInfo != null && !existedPropertyInfo.Value.Equals(currentPropertyInfo.Value))
						{
							changedProperties.Add(existedPropertyInfo.Name);
						}
					}
				}
			}

			return Res.GetString("a182d171-c0d4-43ad-8b8a-edd7635af1f5", "Another user has changed some address details, please review your changes and save again.\r\n{0}{1}{2}\r\n{3}",
				address.Parent != null ? address.Parent.HumanReadableName + ": " : string.Empty,
				address.AddressDescription,
				existedAddress != null ? "(" + existedAddress.E2_SystemCreateUser + " @ " + existedAddress?.E2_SystemCreateTimeUtc + ")" : string.Empty,
				string.Join("\r\n", changedProperties));
		}
	}
}
