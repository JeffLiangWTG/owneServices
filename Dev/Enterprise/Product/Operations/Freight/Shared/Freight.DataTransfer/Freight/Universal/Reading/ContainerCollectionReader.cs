using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class ContainerCollectionReader<T> : DataObjectCollectionReader<Container, T> where T : CommonContainer
	{
		public ContainerCollectionReader(DataObjectList<Container> containers, IXmlImportLogger logger, UniversalObjectFactory factory, IBusinessObjectCollection containersCollection)
			: base(containers)
		{
			this.logger = Argument.NotNull(logger, "logger");
			this.factory = Argument.NotNull(factory, "factory");
			this.containersCollection = Argument.NotNull(containersCollection, "containersCollection");
		}

		protected readonly IXmlImportLogger logger;
		protected readonly UniversalObjectFactory factory;
		protected readonly IBusinessObjectCollection containersCollection;

		#region Implementation

		protected override T[] BusinessObjects
		{
			get { return businessObjects ?? (businessObjects = containersCollection.ToArray<T>()); }
		}

		T[] businessObjects;

		protected override T FindMatchingBusinessObject(Container dataObject)
		{
			return new ContainerBusinessObjectFinder<T>(dataObject).Find(BusinessObjects);
		}

		protected override T ReadIntoBusinessObject(Container dataObject, T container)
		{
			var reader = new ContainerDataObjectReader<T>(dataObject,
				logger, factory, data => container, data => (T)containersCollection.AddNew());

			return reader.ReadIntoBusinessObject();
		}

		protected override void AddToCollection(T container)
		{
			containersCollection.Add(container);
		}

		protected override void RemoveFromCollection(T container)
		{
			CheckIfContainerCanDelete(container);
			if (container.HasLinkedShipmentsWithDeclarations)
			{
				container.JC_JK = ZGuid.Empty;
				container.JC_OH_CFSClient = ZGuid.Empty;
			}
			else
			{
				containersCollection.Delete(container);
			}
		}

		void CheckIfContainerCanDelete(T container)
		{
			if (container.CanDelete)
			{
				return;
			}

			var containerNumber = container.JC_ContainerNum.IsEmpty ? $"{container.RefContainer?.RC_ContainerType} x {container.JC_Calc_ContainerCount}" : container.JC_ContainerNum.ToString();
			containerNumber = containerNumber.Trim();

			if (container.HasLinkedNonEditableSupplierBooking)
			{
				throw new DataObjectReadFailureException(ResString.GetMultilingualString(
					"6ca32a8c-863b-4b04-a9b2-28bf746328d4",
					"Container {0} is already allocated to an active Supplier Booking in status PLN or CNV. The Supplier Booking must be canceled in order to edit the container count or type on this container.",
					containerNumber
				));
			}
			else if (!container.CanDelete && !container.HasLinkedShipmentsWithDeclarations)
			{
				throw new DataObjectReadFailureException(ResString.GetMultilingualString(
				"20521701-11cd-4085-8f47-73291184d0fb",
				"Container {0} may not be deleted: {1}",
				containerNumber,
				container.ReasonForNotAbleToDelete
				));
			}
		}

		#endregion
	}
}
