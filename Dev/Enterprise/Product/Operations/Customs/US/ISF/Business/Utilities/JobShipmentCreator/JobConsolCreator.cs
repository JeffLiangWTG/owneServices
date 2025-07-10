using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.ISF.Business
{
	public class JobConsolCreator
	{
		public JobConsolCreator(ISFHeaderRow headerRow)
		{
			Argument.NotNull(headerRow, "headerRow");
			this.headerRow = headerRow;
		}

		readonly ISFHeaderRow headerRow;

		public ForwardingConsol GetConsol(BusinessObjectFactory factory)
		{
			ForwardingConsol consol = headerRow.ConsolPK.IsValid ? factory.Load<ForwardingConsol>(headerRow.ConsolPK) : null;
			if (consol == null)
			{
				consol = factory.New<ForwardingConsol>();
				PopulateConsolData(consol);
			}
			PopulateContainerData(consol);
			return consol;
		}

		void PopulateConsolData(ForwardingConsol consol)
		{
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			CusISFBill masterBill = headerRow.MasterBill;
			if (masterBill != null && !masterBill.BB_BillNum.IsEmpty)
			{
				consol.JK_MasterBillNum = masterBill.BB_BillNum.Left(consol.JK_MasterBillNumInfo.MaxLength);
			}
			PopulateTransportDetails(consol);
		}

		void PopulateTransportDetails(ForwardingConsol consol)
		{
			var header = consol.Factory.Load<CusISFHeader>(headerRow.Header.PK);
			if (header != null)
			{
				consol.Transports.RemoveAndDeleteAll();
				Transport transportToBeDeleted = null;
				if (consol.Transports.Count > 0) // add by the system automatically
				{
					transportToBeDeleted = consol.Transports[0];
				}
				header.Transports.Sort(Transport.Schema.JW_LegOrder);
				foreach (Transport transport in header.Transports)
				{
					PopulateTransport(consol, transport);
				}
				if (transportToBeDeleted != null)
				{
					consol.Transports.RemoveAndDelete(transportToBeDeleted);
				}
				Transport importTransport = consol.Transports.ImportTransport;
				if (importTransport != null)
				{
					consol.JK_TransportMode = importTransport.JW_TransportMode;
					consol.JK_RL_NKLoadPort = importTransport.JW_RL_NKLoadPort;
					consol.JK_RL_NKDischargePort = importTransport.JW_RL_NKDiscPort;
					consol.JK_RL_NKLastForeignPort = importTransport.JW_RL_NKLoadPort;
				}

				consol.JK_ConsolMode = header.BF_TransportMode == TransportModeCodes.Codes.OceanVesselNonContainerized ? Core.Constants.ContainerModes.BreakBulk : Core.Constants.ContainerModes.FCL;
			}
		}

		void PopulateTransport(ForwardingConsol consol, Transport sourceTransport)
		{
			BusinessObjectCloneArgs args = new BusinessObjectCloneArgs(new string[]
			{
				Transport.Schema.JW_ParentGUID,
				Transport.Schema.JW_ParentType,
			});

			Transport transport = (Transport)sourceTransport.Clone(args);
			transport.ParentType = consol.GetType();
			transport.JW_ParentType = Core.Constants.TransportParentTypes.Consol;
			transport.JW_ParentGUID = consol.PK;
			consol.Transports.Add(transport);
		}

		void PopulateContainerData(ForwardingConsol consol)
		{
			foreach (ISFContainerRow containerRow in headerRow.Containers)
			{
				if (containerRow.ShouldCopy)
				{
					CusISFEquip equipment = containerRow.Container;
					if (equipment != null && !equipment.BE_ContainerNum.IsEmpty)
					{
						var container = consol.Containers.FindAnyByContainerNumber(equipment.BE_ContainerNum);
						if (container == null)
						{
							container = consol.Containers.AddNew();
							container.JC_ContainerNum = equipment.BE_ContainerNum;
						}

						if (container.JC_RC.IsEmpty)
						{
							var query = new ZDBOnlyQuery(typeof(RefContainer));
							query.AddToFilter(RefContainerSchema.RC_ISOType, equipment.BE_ContainerISO);
							query.AddSubQuery(RefContainerSchema.PK, RefContainer.GetContainerCodeFilter(Core.Constants.CountryCodes.UnitedStates, equipment.BE_EquipCode), JoinCondition.And);
							RefContainer[] containerTypes = consol.Factory.Load<RefContainer>(query);
							if (containerTypes.Length == 1)
							{
								container.JC_RC = containerTypes[0].PK;
							}
						}
					}
				}
			}
		}
	}
}
