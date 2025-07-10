using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class AdditionalService : DocDataObject, IAdditionalService
	{
		#region Create

		public static IReadOnlyCollection<AdditionalService> Create(IContext context, JobServiceDependentCollection services)
		{
			if (context == null
				|| services == null)
			{
				return System.Array.Empty<AdditionalService>();
			}

			return services
				.OfType<JobService>()
				.Select(service =>
			{
				return new AdditionalService()
				{
					ServiceCode = new CodeDescription(service.Lookups.JobServiceType_List)
					{
						Code = service.ES_ServiceCode
					},
					Booked = service.ES_Booked,
					Completed = service.ES_Completed,
					Duration = service.ES_Duration,
					ServiceCount = service.ES_ServiceCount,
					ServiceNote = service.ES_ServiceNote,
					References = service.ES_References,
					Contractor = AddressBuilder.Create(context, service.Contractor?.MainAddress),
					Location = AddressBuilder.Create(context, service.Location)
				};
			}).ToArray();
		}

		#endregion

		#region ServiceCode

		public ICodeDescription ServiceCode
		{
			get => serviceCode;
			set => serviceCode = SetChild(serviceCode, value);
		}

		ICodeDescription serviceCode;

		#endregion

		#region Location

		public IAddress Location
		{
			get => location;
			set => location = SetChild(location, value);
		}

		IAddress location;

		#endregion

		#region Contractor

		public IAddress Contractor
		{
			get => contractor;
			set => contractor = SetChild(contractor, value);
		}

		IAddress contractor;

		#endregion

		#region Booked

		public ZDateTime Booked
		{
			get => booked;
			set
			{
				if (SetNonPersistentPropertyValue(BookedInfo, ref booked, value))
				{
					Validate(BookedInfo);
				}
			}
		}

		ZDateTime booked;

		public ZPropertyInfo BookedInfo => GetZPropertyInfo(nameof(Booked));

		#endregion

		#region Completed

		public ZDateTime Completed
		{
			get => completed;
			set
			{
				if (SetNonPersistentPropertyValue(CompletedInfo, ref completed, value))
				{
					Validate(CompletedInfo);
				}
			}
		}

		ZDateTime completed;

		public ZPropertyInfo CompletedInfo => GetZPropertyInfo(nameof(Completed));

		#endregion

		#region Duration

		public ZDateTime Duration
		{
			get => duration;
			set
			{
				if (SetNonPersistentPropertyValue(DurationInfo, ref duration, value))
				{
					Validate(DurationInfo);
				}
			}
		}

		ZDateTime duration;

		public ZPropertyInfo DurationInfo => GetZPropertyInfo(nameof(Duration));

		#endregion

		#region ServiceCount

		public ZDecimal ServiceCount
		{
			get => serviceCount;
			set
			{
				if (SetNonPersistentPropertyValue(ServiceCountInfo, ref serviceCount, value))
				{
					Validate(ServiceCountInfo);
				}
			}
		}

		ZDecimal serviceCount;

		public ZPropertyInfo ServiceCountInfo => GetZPropertyInfo(nameof(ServiceCount));

		#endregion

		#region ServiceNote

		public ZString ServiceNote
		{
			get => serviceNote;
			set
			{
				if (SetNonPersistentPropertyValue(ServiceNoteInfo, ref serviceNote, value))
				{
					Validate(ServiceNoteInfo);
				}
			}
		}

		ZString serviceNote;

		public ZPropertyInfo ServiceNoteInfo => GetZPropertyInfo(nameof(ServiceNote));

		#endregion

		#region References

		public ZString References
		{
			get => references;
			set
			{
				if (SetNonPersistentPropertyValue(ReferencesInfo, ref references, value))
				{
					Validate(ReferencesInfo);
				}
			}
		}

		ZString references;

		public ZPropertyInfo ReferencesInfo => GetZPropertyInfo(nameof(References));

		#endregion
	}
}
