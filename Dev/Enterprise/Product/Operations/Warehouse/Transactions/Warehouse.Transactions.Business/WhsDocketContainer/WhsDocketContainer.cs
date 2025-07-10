using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

using Enterprise.Freight.Common.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	[DependentBusinessObject(typeof(WhsReceive), "Containers")]
	public sealed class WhsDocketContainer : AutoWhsDocketContainer, ICartageContainer
	{
		public WhsDocketContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Business Object Overrides

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		#region Related Business Objects

		public WhsDocket Docket
		{
			get { return Factory.Load<WhsDocket>(WC_WD); }
		}

		#endregion

		#region Properties

		[List("Lookups.RefContainers")]
		public override ZGuid WC_RC
		{
			get { return base.WC_RC; }
			set { base.WC_RC = value; }
		}

		#region WC_ContainerNum

		public override ZString WC_ContainerNum
		{
			get { return base.WC_ContainerNum; }
			set
			{
				base.WC_ContainerNum = value;

				var docket = Docket;
				if (docket != null && !docket.IsDeleted)
				{
					// tested in validation
					foreach (WhsDocketContainer container in docket.Containers)
					{
						if (!IsValidationSuspended)
						{
							container.Validation.ValidateWC_ContainerNum();
						}
					}
				}
			}
		}

		#endregion

		#endregion

		#region ICartageContainer Members

		ZString ICartageContainer.ContainerMode
		{
			get { return ""; }
		}

		ZString ICartageContainer.ContainerNumber
		{
			get { return WC_ContainerNum; }
		}

		ZGuid ICartageContainer.ContainerRC
		{
			get { return WC_RC; }
		}

		ZGuid ICartageContainer.JobContainerPK
		{
			get { return ZGuid.Empty; }
		}

		IReadOnlyCollection<ICartageLooseCargo> ICartageContainer.LooseCargo => Array.Empty<ICartageLooseCargo>();

		ZDecimal ICartageContainer.NetWeight
		{
			get { return (Docket != null) ? Docket.WD_WeightSentUserEntered / Docket.Containers.Count : 0m; }
		}

		ZString ICartageContainer.Seal
		{
			get { return WC_SealNum; }
		}

		#endregion
	}
}
