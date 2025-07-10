using System.Collections.Generic;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class InvoiceRollupOrGroupCollection : RegistryBusinessObjectCollectionTemplate
	{
		public InvoiceRollupOrGroupCollection()
		{
		}

		public InvoiceRollupOrGroupCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new InvoiceRollupOrGroup this[int x]
		{
			get { return (InvoiceRollupOrGroup)Elements[x]; }
		}

		public new InvoiceRollupOrGroup AddNew()
		{
			return (InvoiceRollupOrGroup)base.AddNew();
		}

		protected override bool AllowRemoveCore
		{
			get
			{
				if (Count > 1)
				{
					return base.AllowRemoveCore;
				}
				else
				{
					return false;
				}
			}
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new InvoiceRollupOrGroupCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new InvoiceRollupOrGroup(CurrentFallbackLevel, CurrentFactory);
		}

		public InvoiceRollupOrGroup GetBestMatch(ZString serviceDirection, ZString transportMode, ZString mode, params ZString[] jobTypesInOrderOfPreference)
		{
			List<string> jobTypes = new List<string>();

			foreach (ZString jobType in jobTypesInOrderOfPreference)
			{
				if (jobType != OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code)
				{
					jobTypes.Add(jobType);
					if (jobType == JobInvoicingConsumerTypes.Shipment.Code || jobType == JobInvoicingConsumerTypes.Brokerage.Code)
					{
						jobTypes.Add(OrgInvoiceRollupOrGroupLookups.JobType_List.ShipmentAndBrokerage.Code);
					}
				}
			}

			jobTypes.Add(OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code);

			List<string> directions = new List<string>();
			if (serviceDirection != OrgConstants.ServiceDirection.Code.All)
			{
				directions.Add(serviceDirection);
			}
			directions.Add(OrgConstants.ServiceDirection.Code.All);

			List<string> modes = new List<string>();
			if (mode != OrgConstants.ModesForGroupOrSubTotal.Codes.All)
			{
				modes.Add(mode);
				var seaContainerModeList = ObjectFactory.Get<IFreightCodePairListProvider>().GetContainerModeList(OrgConstants.ModesForGroupOrSubTotal.Codes.Sea);
				if (transportMode == OrgConstants.ModesForGroupOrSubTotal.Codes.Sea && seaContainerModeList.ContainsCode(mode))
				{
					modes.Add(OrgConstants.ModesForGroupOrSubTotal.Codes.Sea);
				}
			}
			modes.Add(OrgConstants.ModesForGroupOrSubTotal.Codes.All);

			foreach (string jobTypeCode in jobTypes)
			{
				foreach (string serviceDirectionCode in directions)
				{
					foreach (string modeCode in modes)
					{
						InvoiceRollupOrGroup result = GetExactMatch(jobTypeCode, serviceDirectionCode, modeCode);
						if (result != null)
						{
							return result;
						}
					}
				}
			}

			return null;
		}

		InvoiceRollupOrGroup GetExactMatch(ZString jobType, ZString serviceDirection, ZString mode)
		{
			foreach (InvoiceRollupOrGroup element in this)
			{
				if (element.JobType == jobType && element.ServiceDirection == serviceDirection && element.TransportMode == mode)
				{
					return element;
				}
			}
			return null;
		}
	}
}
