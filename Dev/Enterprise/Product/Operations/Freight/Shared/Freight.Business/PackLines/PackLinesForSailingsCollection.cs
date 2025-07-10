using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	/// <summary>
	/// Summary description for PackLinesForSailingsCollection.
	/// </summary>
	public class PackLinesForSailingsCollection : InternalPackLineNonDependentCollection
	{
		public PackLinesForSailingsCollection(JobSailingCollection masterCollection)
			: base(masterCollection.Factory, new ZQuery(JobPackLinesSchema.PK, ZGuid.Empty))
		{
			fMasterCollection = masterCollection;
		}

		public PackLinesForSailingsCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public PackLinesForSailingsCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public override void Load()
		{
			base.Load();
			foreach (JobSailing sailing in MasterCollection)
			{
				foreach (CommonShipment shipment in sailing.Shipments)
				{
					foreach (PackLine line in shipment.OuterPackLines)
					{
						Add(line);
					}
				}
			}
		}

		#region JobSailing

		public JobSailingCollection MasterCollection
		{
			get
			{
				if (fMasterCollection == null)
				{
					fMasterCollection = new JobSailingCollection(fMasterCollection.Factory);
				}
				return fMasterCollection;
			}
		}

		#endregion

		#region Implementation

		JobSailingCollection fMasterCollection;

		#endregion

	}
}
