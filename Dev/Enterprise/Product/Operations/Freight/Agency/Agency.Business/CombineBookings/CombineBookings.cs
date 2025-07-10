using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	public class CombineBookings : NonPersistentBusinessObject, IObsoleteValidation
	{
		public CombineBookings(AgencyBooking masterBooking)
			: base(masterBooking.Factory)
		{
			this.masterBooking = masterBooking;
			this.masterBooking.SetReadOnlyIncludingChildren(true);
		}

		#region Related BusinessObjects

		public AgencyBooking MasterBooking
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return masterBooking; }
		}

		public AgencyBookingCollection OtherBookings
		{
			get
			{
				if (otherBookings == null)
				{
					ICollectionRelationship relationship = new AdhocCollectionRelationship(typeof(AgencyBooking));
					otherBookings = new AgencyBookingCollection(Factory, relationship, false);
				}
				return otherBookings;
			}
		}
		AgencyBookingCollection otherBookings;

		#endregion

		#region Operations

		public void Combine(BusinessObjectFactory factory)
		{
			AgencyBooking mBooking = factory.Load<AgencyBooking>(MasterBooking.PK);
			AgencyBooking[] oBookings = factory.Load<AgencyBooking>(new ZQuery(JobShipmentSchema.PK, Array.ConvertAll(OtherBookings.ToArray(), (b) => b.PK)));

			foreach (AgencyBooking otherBooking in oBookings)
			{
				Combine(mBooking, otherBooking);
			}

			mBooking.HasChanges = true;
		}

		#endregion

		#region Strategies

		public CombineBookingsLookups Lookups
		{
			get { return lookups ?? (lookups = new CombineBookingsLookups(this)); }
		}
		CombineBookingsLookups lookups;

		#endregion

		#region Implementation

		static void Combine(AgencyBooking masterBooking, AgencyBooking otherBooking)
		{
			ZDecimal convertedWeight = Constants.Weight.Convert(otherBooking.JS_ActualWeight, otherBooking.JS_UnitOfWeight, masterBooking.JS_UnitOfWeight);
			ZDecimal convertedVolume = Constants.Volume.Convert(otherBooking.JS_ActualVolume, otherBooking.JS_UnitOfVolume, masterBooking.JS_UnitOfVolume);

			ZDecimal newWeight = convertedWeight + masterBooking.JS_ActualWeight;
			ZDecimal newVolume = convertedVolume + masterBooking.JS_ActualVolume;

			Combine_ContainersAndPacklines(masterBooking, otherBooking);
			Combine_Notes(masterBooking, otherBooking);

			masterBooking.JS_ActualWeight = newWeight;
			masterBooking.JS_ActualVolume = newVolume;

			otherBooking.WorkflowItems.Load();
			otherBooking.JS_IsCancelled = true;
		}
		static void Combine_ContainersAndPacklines(AgencyBooking masterBooking, AgencyBooking otherBooking)
		{
			Dictionary<ZGuid, ZGuid> containerPKMapping = new Dictionary<ZGuid, ZGuid>();

			BusinessObjectCloneArgs copyContainerArgs = new BusinessObjectCloneArgs();
			copyContainerArgs.AddExcludedColumns(new string[]
			{
				JobContainerSchema.Constants.JC_JS_FCLBookingOnlyLink,
			});

			BusinessObjectCloneArgs copyPacklineArgs = new BusinessObjectCloneArgs();
			copyPacklineArgs.AddExcludedColumns(new string[]
			{
				JobPackLinesSchema.Constants.JL_JS,
				AgencyBookingPackLine.Schema.JL_JC,
			});

			foreach (AgencyBookingContainer container in otherBooking.BookedContainers)
			{
				AgencyBookingContainer containerCopy = (AgencyBookingContainer)container.Clone(copyContainerArgs);
				containerPKMapping.Add(container.PK, containerCopy.PK);

				masterBooking.BookedContainers.Add(containerCopy);
			}

			foreach (AgencyBookingContainer realContainer in otherBooking.RealContainers)
			{
				AgencyBookingContainer containerCopy = (AgencyBookingContainer)realContainer.Clone(copyContainerArgs);
				containerPKMapping.Add(realContainer.PK, containerCopy.PK);

				masterBooking.RealContainers.Add(containerCopy);
			}

			foreach (AgencyBookingPackLine packline in otherBooking.OuterPackLines)
			{
				AgencyBookingPackLine packlineCopy = (AgencyBookingPackLine)packline.Clone(copyPacklineArgs);

				masterBooking.OuterPackLines.Add(packlineCopy);

				ZGuid jC;
				if (containerPKMapping.TryGetValue(packline.JL_JC, out jC))
				{
					packlineCopy.JL_JC = jC;
				}
			}
		}
		static void Combine_Notes(AgencyBooking masterBooking, AgencyBooking otherBooking)
		{
			BusinessObjectCloneArgs copyNoteArgs = new BusinessObjectCloneArgs();
			copyNoteArgs.AddExcludedColumns(new string[]
			{
				StmNoteSchema.Constants.ST_ParentID,
				StmNoteSchema.Constants.ST_Table,
			});

			foreach (StmNote note in otherBooking.Notes.GetAllNotes())
			{
				if (masterBooking.Notes.FindByDescription(note.ST_Description).Length == 0)
				{
					StmNote noteCopy = (StmNote)note.Clone(copyNoteArgs);
					masterBooking.Notes.Add(noteCopy);
				}
			}
		}

		#endregion

		readonly AgencyBooking masterBooking;
	}
}
