using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.QuotedBookings.Business
{
	public class PackContainerHelper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public PackContainerHelper(JobSailing jobSailing)
			: base(jobSailing.Factory)
		{
			fSailing = jobSailing;
		}

		#region Sailing

		public JobSailing Sailing
		{
			get { return fSailing; }
		}

		readonly JobSailing fSailing;

		#endregion

		#region Containers

		#region SailingContainerCollection

		public ForwardingContainerCollection Containers
		{
			get
			{
				if (fContainers == null)
				{
					fContainers = new ForwardingContainerCollection(Sailing, Factory);
					fContainers.Load(new ZQuery(new ZQuery(JobContainerSchema.JC_JX, SQLComparisonOperator.Equal, Sailing.PK),
						JoinCondition.And, new ZQuery(JobContainerSchema.JC_JK, SQLComparisonOperator.Equal, null)));
					RegisterEditableChildObject(fContainers);
				}
				return fContainers;
			}
		}

		ForwardingContainerCollection fContainers;

		public ForwardingContainer AddBookingContainer(ZString containerNum)
		{
			ForwardingContainer container = null;

			ZBool containerAlreadyExists = false;
			foreach (ForwardingContainer existingContainer in Containers)
			{
				if (existingContainer.JC_ContainerNum == containerNum)
				{
					containerAlreadyExists = true;
					break;
				}
			}
			if (!containerAlreadyExists)
			{
				container = fContainers.AddNew();
				container.JC_ContainerNum = containerNum.ToUpper();
			}
			return container;
		}

		/// <summary>
		/// Determines a name for the new container and adds it to the containers collection.
		/// </summary>
		/// <returns>The sailing container created, null if a container wasn't created</returns>
		public ForwardingContainer AddBookingContainer()
		{
			ZInt potentialContainerNumber = Containers.Count;
			ZString newContainerName;

			ForwardingContainer newContainer = null;
			while (newContainer == null)
			{
				potentialContainerNumber++;
				newContainerName = "CONTAINER" + potentialContainerNumber.ToString();
				newContainer = AddBookingContainer(newContainerName);
			}
			return newContainer;
		}

		#endregion

		#endregion

	}
}
