using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Common;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class OrderSetCarrierAndCarrierServiceLevelActionMethodApplicator : WhsOperationalActionMethodApplicator
	{
		public OrderSetCarrierAndCarrierServiceLevelActionMethodApplicator(BusinessObjectFactory factory)
			: base(Res.GetString("74183324-dac0-45fd-8fdd-e91541c80a2d", "Set Carrier And Carrier Service Level"), factory)
		{
		}

		const string OutputTextFormat = "{0} {1} - {2}"; // eg. Order 123 - was successfully updated.

		#region Set Carrier Service Levels

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] orders)
		{
			if (orders.Length > 0)
			{
				var updatedOrders = new List<WhsOrder>();
				AddFetchHints(orders);
				Validation.ValidateAll();
				if (HasErrors)
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Error, Res.GetString("28807965-fd28-4b55-8aef-2af20f522b57", "There are errors that need to be corrected before this action can be run. {0}", this.GetErrors().ToUniqueMessageListString()));
				}
				else
				{
					log.SetSectionProgressMax(orders.Length);

					foreach (WhsOrder order in orders)
					{
						if (SetCarrierAndCarrierServiceLevel(order, log))
						{
							updatedOrders.Add(order);
						}
						log.BumpSectionProgress();
					}
				}

				AddPreSaveFetchHints(updatedOrders);
			}
		}

		void AddFetchHints(IEnumerable<BusinessObject> orders)
		{
			var factory = orders.FirstOrDefault()?.Factory;
			foreach (var order in orders)
			{
				factory.AddFetchHint(JobDocAddressSchema.Instance, FetchHintsHelper.GetJobDocAddressQuery(WhsDocketSchema.Constants.Prefix, order.PK));
				factory.AddFetchHint(WhsLoadOrderSchema.WOV_WD_Docket, order.PK);
			}
		}

		void AddPreSaveFetchHints(IEnumerable<BusinessObject> orders)
		{
			var company = GlbCompany.CurrentCompany;
			var factory = orders.FirstOrDefault()?.Factory;
			foreach (var order in orders)
			{
				var jobHeaderQuery = new ZQuery();
				jobHeaderQuery.AddToFilter(JobHeaderSchema.JH_ParentID, order.PK);
				jobHeaderQuery.AddToFilter(JobHeaderSchema.JH_GC, company.PK);
				factory.AddFetchHint(JobHeaderSchema.Instance, jobHeaderQuery);
			}
		}

		bool SetCarrierAndCarrierServiceLevel(WhsOrder order, IOperationalActionSectionLog log)
		{
			var orderLink = GetDocketIdLink(order);
			if (order.WD_DocketStatus != DocketStatus.Codes.Entered)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, OutputTextFormat, order.Description, orderLink, Res.GetString("5462b87e-3910-4120-b58d-b83f92869379", "does not have a status of entered. Carrier Service Level can only be updated on Entered Orders."));
			}
			else if (order.IsOrderAssignedToLoad)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, OutputTextFormat, order.Description, orderLink, Res.GetString("156ee3f0-b5a7-4a69-a7b1-91dc151e7434", "The Carrier Service Level cannot be updated while the order is assigned to a load."));
			}
			else
			{
				order.TransportCoPK = carrierPK;
				order.WD_PL_NKCarrierServiceLevel = carrierServiceLevel;
				log.NotifyFormat(OperationalActionLogErrorLevel.Informational, OutputTextFormat, order.Description, orderLink, Res.GetString("ceef3552-9821-4704-9aab-d222611345db", "was successfully updated."));
			}

			return order.HasChanges;
		}

		#endregion

		#region Settings

		public abstract class Schema
		{
			public const string CarrierPK = nameof(CarrierPK);
			public const string CarrierServiceLevel = nameof(CarrierServiceLevel);
		}

		[List(nameof(Carriers))]
		[ResourceStringData("OrderSetCarrierAndCarrierServiceLevelActionMethodApplicator|CarrierPK", Caption = "Carrier")]
		public ZGuid CarrierPK
		{
			get => carrierPK;
			set
			{
				if (carrierPK != value)
				{
					SetNonPersistentPropertyValue(CarrierPKInfo, ref carrierPK, value);
					Validation.ValidateAll();
				}
			}
		}
		ZGuid carrierPK;

		public ZPropertyInfo CarrierPKInfo => GetZPropertyInfo(Schema.CarrierPK);

		[List(nameof(CarrierServiceLevels))]
		[ResourceStringData("OrderSetCarrierAndCarrierServiceLevelActionMethodApplicator|CarrierServiceLevel", Caption = "Carrier Service Level")]
		public ZString CarrierServiceLevel
		{
			get => carrierServiceLevel;
			set
			{
				SetNonPersistentPropertyValue(CarrierServiceLevelInfo, ref carrierServiceLevel, value);
				Validation.ValidateNKCarrierServiceLevel();
			}
		}
		ZString carrierServiceLevel;

		public ZPropertyInfo CarrierServiceLevelInfo => GetZPropertyInfo(Schema.CarrierServiceLevel);

		public CarrierCollection Carriers => carriers ?? (carriers = new CarrierCollection(Factory));
		CarrierCollection carriers;

		public OrgCarrierServiceLevelCollection CarrierServiceLevels
		{
			get => Factory.GetCachedValue(FormattableString.Invariant($"{CarrierServiceLevelsCacheKey}|{CarrierPK}"), GetCarrierServiceLevelByCarrier); // Key used in factory cache
		}

		OrgCarrierServiceLevelCollection GetCarrierServiceLevelByCarrier()
		{
			var transportCo = Factory.Load<OrgHeader>(CarrierPK);

			var carrierServiceLevels = (transportCo == null) ?
				new OrgCarrierServiceLevelCollection(Factory, false, false) :
				new OrgCarrierServiceLevelCollection(transportCo.MiscServ);

			carrierServiceLevels.Load();

			return carrierServiceLevels;
		}

		const string CarrierServiceLevelsCacheKey = "OrderSetCarrierAndCarrierServiceLevelActionMethodApplicator|CarrierServiceLevel";

		#endregion

		#region Validation

		public OrderSetCarrierAndCarrierServiceLevelActionMethodApplicatorValdidation Validation => new OrderSetCarrierAndCarrierServiceLevelActionMethodApplicatorValdidation(this);

		#endregion
	}

	#region OrderSetCarrierAndCarrierServiceLevelActionMethodApplicatorValdidation

	public class OrderSetCarrierAndCarrierServiceLevelActionMethodApplicatorValdidation : ZValidation
	{
		#region constructor

		public OrderSetCarrierAndCarrierServiceLevelActionMethodApplicatorValdidation(OrderSetCarrierAndCarrierServiceLevelActionMethodApplicator parent)
			: base(parent)
		{
			Parent = parent;
			ZValidationInternals = this;
		}

		#endregion

		#region WD_DocketStatus

		public void ValidateNKCarrierServiceLevel()
		{
			ZValidationInternals.Validate(Parent.CarrierServiceLevelInfo, CheckNKCarrierServiceLevel);
		}

		protected void CheckNKCarrierServiceLevel()
		{
			ListValidation.ErrorIfInvalidCode(Parent.CarrierServiceLevelInfo);
		}

		#endregion

		#region CheckCarrierPK

		public void ValidateCarrierPK()
		{
			ZValidationInternals.Validate(Parent.CarrierPKInfo, CheckCarrierPK);
		}

		protected void CheckCarrierPK()
		{
			if (!Parent.CarrierPK.IsEmpty)
			{
				if (!Parent.CarrierPK.IsValid)
				{
					Parent.CarrierPKInfo.AddError(Res.GetString("9c44f14c-d5e1-4a31-833b-4f13971877d9", "Please enter a valid Carrier."));
				}
				else
				{
					ListValidation.ErrorIfInvalidPK(Parent.CarrierPKInfo);
				}
			}
		}

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			ValidateCarrierPK();
			ValidateNKCarrierServiceLevel();
		}

		#endregion

		#region Implementation

		public override Type AutoValidationType => typeof(OrderSetCarrierAndCarrierServiceLevelActionMethodApplicator);

		readonly OrderSetCarrierAndCarrierServiceLevelActionMethodApplicator Parent;
		readonly IValidationInternals ZValidationInternals;

		#endregion
	}

	#endregion
}
