using CargoWise.EntityFramework;
using CargoWise.EventReference;
using CargoWise.Types;

namespace Enterprise.Freight.Agency.Business
{
	public class ReleaseInstanceCollection : NonPersistentBusinessObjectCollection<ReleaseInstance>
	{
		public ReleaseInstanceCollection(ReleaseHeader header)
		{
			this.header = header;
		}

		public ReleaseInstance AddIfNotExists(ZGuid addressPK, ZString releaseNumber, ZString releaseType)
		{
			ReleaseInstance instance = Find(addressPK, releaseNumber);

			if (instance == null)
			{
				instance = AddNew();
				instance.ReleaseNumber = releaseNumber;
				instance.ContainerYardAddress = addressPK;
				instance.ReleaseType = releaseType;
			}
			else if (releaseType != instance.ReleaseType)
			{
				instance.ReleaseType = Constants.EventReferenceReleaseTypes.Codes.Revised;
			}

			return instance;
		}

		public ReleaseInstance Find(ZGuid addressPK, ZString releaseNumber)
		{
			foreach (ReleaseInstance instance in this)
			{
				if (instance.ReleaseNumber == releaseNumber && instance.ContainerYardAddress == addressPK)
				{
					return instance;
				}
			}
			return null;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ReleaseInstance(header);
		}

		readonly ReleaseHeader header;
	}
}



