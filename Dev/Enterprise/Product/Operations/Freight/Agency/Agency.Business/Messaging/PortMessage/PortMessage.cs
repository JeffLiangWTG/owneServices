
namespace Enterprise.Freight.Agency.Business
{
	using System.Collections.Generic;
	using System.Diagnostics;
	using CargoWise.ComponentModel;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Freight.Business;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Schema;

	public class PortMessage : AutoPortMessage
	{
		public PortMessage(JobVoyage voyage)
			: base(voyage.Factory)
		{
			this.Voyage = voyage;
		}

		#region Properties

		[List("Lookups.Port_List")]
		public override ZString Port
		{
			[DebuggerStepThrough]
			get { return base.Port; }
			set
			{
				if (base.Port != value)
				{
					base.Port = value;

					UpdateDirectionOnPortChanged();
				}
			}
		}

		protected virtual void UpdateDirectionOnPortChanged()
		{
			var list = Lookups.Direction_List;
			if (list.Count == 0)
			{
				Direction = ZString.Empty;
			}
			else if (list.Count == 1 || Direction.IsEmpty)
			{
				Direction = list[0].Code;
			}
		}

		[List("Lookups.Direction_List")]
		public override ZString Direction
		{
			get { return base.Direction; }
			set { base.Direction = value; }
		}

		[List("Lookups.MessageType_List")]
		public override ZString MessageType
		{
			get { return base.MessageType; }
			set { base.MessageType = value; }
		}

		[List("Lookups.Principal_List")]
		public override ZGuid PrincipalPK { get => base.PrincipalPK; set => base.PrincipalPK = value; }

		public PortMessageIssueCollection Issues
		{
			get
			{
				if (issues == null)
				{
					issues = new PortMessageIssueCollection();
				}

				return issues;
			}
		}

		public PortMessageLookups Lookups
		{
			get
			{
				if (lookups == null)
				{
					lookups = GetNewLookups();
				}

				return lookups;
			}
		}

		public JobVoyage Voyage { get; private set; }

		#endregion

		public virtual IEnumerable<BillOfLading> GetRelatedShipments(BusinessObjectFactory factory = null)
		{
			factory = factory ?? Factory;

			var filter = RelatedJobsHelper.GetRelatedBillOfLadingFilter(factory, Voyage, Port, Direction);
			var shipments = factory.Load<BillOfLading>(filter);

			AddFetchHints(shipments);

			return shipments;
		}

		#region Implementation

		protected override PortMessageValidation GetNewValidation()
		{
			return new PortMessageValidation(this);
		}

		protected virtual PortMessageLookups GetNewLookups()
		{
			return new PortMessageLookups(this);
		}

		#region RelatedShipments

		static void AddFetchHints(BillOfLading[] shipments)
		{
			foreach (BillOfLading shipment in shipments)
			{
				shipment.Factory.AddFetchHint(JobContainerSchema.JC_JS_FCLBookingOnlyLink, shipment.PK);
				shipment.Factory.AddFetchHint(new FetchHint(JobPackLinesSchema.JL_JS, shipment.PK, JobPackLinesSchema.JL_DetailedDescription, JobPackLinesSchema.JL_MarksAndNumbers));
				shipment.Factory.AddFetchHint(JobConsolTransportSchema.JW_ParentGUID, shipment.PK); // TODO: validation should not care about Transport leg's
				shipment.Factory.AddFetchHint(RefUNLOCOSchema.RL_Code, shipment.JS_RL_NKOrigin);
				shipment.Factory.AddFetchHint(RefUNLOCOSchema.RL_Code, shipment.JS_RL_NKDestination);
				shipment.Factory.AddFetchHint(JobShipmentSchema.JS_HouseBill, shipment.JS_HouseBill); // TODO: should not need to load shipments by housebill.
				shipment.Factory.AddFetchHint(OrgAddressSchema.PK, shipment.JS_OA_BookedShippingLineAddress);
				shipment.Factory.AddFetchHint(OrgHeaderSchema.PK, shipment.JS_OH_DeliveryAgent);
				shipment.Factory.AddFetchHint(OrgCusCodeSchema.OK_OA_PremisesAddress, shipment.JS_OA_BookedShippingLineAddress);
				shipment.Factory.AddFetchHint(OrgCusCodeSchema.OK_OH, shipment.JS_OH_DeliveryAgent);
				shipment.Factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, shipment.PK);
				shipment.Factory.AddFetchHint(JobConsolTransportSchema.JW_ParentGUID, shipment.PK);
				shipment.Factory.AddFetchHint(typeof(JobSailing), shipment.JS_JX);
				shipment.Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, shipment.PK);
				shipment.Factory.AddFetchHint(RefPackTypeSchema.F3_Code, shipment.JS_F3_NKPackType);

				shipment.Factory.AddFetchHint(JobHeaderSchema.Instance, new ZQuery(
					new ZQuery(JobHeaderSchema.JH_ParentID, shipment.PK),
					new ZQuery(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK)
				));
			}

			foreach (BillOfLading shipment in shipments)
			{
				foreach (Transport transport in shipment.Transports)
				{
					if (transport.JW_JX.IsValid)
					{
						shipment.Factory.AddFetchHint(JobSailingSchema.PK, transport.JW_JX);
					}
				}
			}

			foreach (BillOfLading shipment in shipments)
			{
				using (shipment.GetValidationSuspender())
				{
					var sailing = shipment.Sailing;
					if (sailing != null)
					{
						shipment.Factory.AddFetchHint(OrgAddressSchema.PK, sailing.JX_JB_ArrivalCTOAddress);
					}

					foreach (BillOfLadingPackLine packline in shipment.OuterPackLines)
					{
						shipment.Factory.AddFetchHint(JobContainerPackPivotSchema.J6_JL, packline.PK);
						shipment.Factory.AddFetchHint(RefPackTypeSchema.F3_Code, packline.JL_F3_NKPackType);
					}

					foreach (PackLine packline in ((CommonShipment)shipment).InnerPackLines) // we don't use inner pack lines, but validation does.
					{
						shipment.Factory.AddFetchHint(RefPackTypeSchema.F3_Code, packline.JL_F3_NKPackType);
					}

					foreach (BillOfLadingContainer container in shipment.RealContainers)
					{
						shipment.Factory.AddFetchHint(RefContainerStockSchema.R6_ContainerNum, container.JC_ContainerNum);
						shipment.Factory.AddFetchHint(JobContainerPackPivotSchema.J6_JC, container.PK);
						shipment.Factory.AddFetchHint(RefContainerSchema.PK, container.JC_RC);
						shipment.Factory.AddFetchHint(CusContainerSchema.CO_JC, container.PK); // TODO: validation should not care about CusContainer's
						shipment.Factory.AddFetchHint(OrgAddressSchema.PK, container.JC_OA_ArrivalContainerYardAddress);
						shipment.Factory.AddFetchHint(EDIMessageSchema.EM_LinkUniqueID, container.PK);
						shipment.Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, container.PK);
					}

					foreach (JobDocAddress address in shipment.DocAddresses)
					{
						shipment.Factory.AddFetchHint(OrgAddressSchema.PK, address.E2_OA_Address);
					}
				}
			}

			AddFetchHintsForSailings(shipments);
		}

		static void AddFetchHintsForSailings(BillOfLading[] shipments)
		{
			var sailings = new List<JobSailing>();

			foreach (BillOfLading shipment in shipments)
			{
				JobSailing sailing;

				if ((sailing = shipment.Sailing) != null)
				{
					sailings.Add(sailing);
				}

				foreach (Transport transport in shipment.Transports)
				{
					if ((sailing = transport.Sailing) != null)
					{
						sailings.Add(sailing);
					}
				}

				foreach (BillOfLadingContainer container in shipment.RealContainers)
				{
					if (container.ArrivalContainerYardAddress != null)
					{
						shipment.Factory.AddFetchHint(OrgHeaderSchema.PK, container.ArrivalContainerYardAddress.OA_OH);
						shipment.Factory.AddFetchHint(OrgCusCodeSchema.OK_OH, container.ArrivalContainerYardAddress.OA_OH);
					}
				}
			}

			foreach (JobSailing sailing in sailings)
			{
				OrgAddress ctoAddress;

				if ((ctoAddress = sailing.Destination.ArrivalCTOAddress) != null)
				{
					sailing.Factory.AddFetchHint(OrgHeaderSchema.PK, ctoAddress.OA_OH);
					sailing.Factory.AddFetchHint(OrgCusCodeSchema.PK, ctoAddress.OA_OH);
				}

				sailing.Factory.SeedQueryCache(JobSailingSchema.Constants.TableName, new ZQuery(JobSailingSchema.JX_JA, sailing.JX_JA));
				sailing.Factory.SeedQueryCache(JobSailingSchema.Constants.TableName, new ZQuery(JobSailingSchema.JX_JB, sailing.JX_JB));
				sailing.Factory.AddFetchHint(JobVoyOriginSchema.JA_JV, sailing.Origin.JA_JV);
				sailing.Factory.AddFetchHint(JobVoyDestinationSchema.JB_JV, sailing.Origin.JA_JV);
				sailing.Factory.AddFetchHint(JobVoyCountrySchema.J0_JV, sailing.Origin.JA_JV);
				sailing.Factory.AddFetchHint(JobTradeLaneVoyageSchema.NB_JV, sailing.Origin.JA_JV);
				sailing.Factory.AddFetchHint(RefVesselSchema.RV_Code, sailing.JX_JV_NKVessel);
			}
		}

		#endregion

		PortMessageIssueCollection issues;
		PortMessageLookups lookups;

		#endregion
	}
}


