using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN
{
	sealed class ContainerLoadPlanContainer : DocDataObject
	{
		public ContainerLoadPlanContainer(ContainerLoadPlan loadPlan, CommonContext context, object identifier)
			: base(identifier)
		{
			this.loadPlan = Argument.NotNull(loadPlan, nameof(loadPlan));
			this.context = context;
		}

		readonly ContainerLoadPlan loadPlan;
		readonly CommonContext context;

		#region Number

		public ZString Number
		{
			get => number;
			set
			{
				if (SetNonPersistentPropertyValue(NumberInfo, ref number, value))
				{
					Validate(NumberInfo);
				}
			}
		}
		ZString number;

		public ZPropertyInfo NumberInfo => GetZPropertyInfo(nameof(Number));

		#endregion

		#region Mode

		public ICodeDescription Mode
		{
			get => mode;
			set => mode = SetChild(mode, value);
		}

		ICodeDescription mode;

		#endregion

		#region ContainerType

		public IContainerType ContainerType
		{
			get => containerType;
			set => containerType = SetChild(containerType, value);
		}

		IContainerType containerType;

		#endregion

		#region PackDate

		public ZDateTime PackDate
		{
			get => packDate;
			set
			{
				if (SetNonPersistentPropertyValue(PackDateInfo, ref packDate, value))
				{
					Validate(PackDateInfo);
				}
			}
		}

		ZDateTime packDate;

		public ZPropertyInfo PackDateInfo => GetZPropertyInfo(nameof(PackDate));

		#endregion

		#region TradeFlag

		public ZString TradeFlag
		{
			get => loadPlan.TradeFlag;
			set => loadPlan.TradeFlag = value;
		}

		public ZPropertyInfo TradeFlagInfo => GetWrappedZPropertyInfo(nameof(TradeFlag), x => loadPlan.TradeFlagInfo);

		#endregion

		#region Seal

		public ZString Seal
		{
			get => seal;
			set
			{
				if (SetNonPersistentPropertyValue(SealInfo, ref seal, value))
				{
					Validate(SealInfo);
				}
			}
		}

		ZString seal;

		public ZPropertyInfo SealInfo => GetZPropertyInfo(nameof(Seal));

		#endregion

		#region SecondSeal

		public ZString SecondSeal
		{
			get => secondSeal;
			set
			{
				if (SetNonPersistentPropertyValue(SecondSealInfo, ref secondSeal, value))
				{
					Validate(SecondSealInfo);
				}
			}
		}

		ZString secondSeal;

		public ZPropertyInfo SecondSealInfo => GetZPropertyInfo(nameof(SecondSeal));

		#endregion

		#region ThirdSeal

		public ZString ThirdSeal
		{
			get => thirdSeal;
			set
			{
				if (SetNonPersistentPropertyValue(ThirdSealInfo, ref thirdSeal, value))
				{
					Validate(ThirdSealInfo);
				}
			}
		}

		ZString thirdSeal;

		public ZPropertyInfo ThirdSealInfo => GetZPropertyInfo(nameof(ThirdSeal));

		#endregion

		#region GoodsDetails

		[ReadOnly(true)]
		public ZString GoodsDetails
		{
			get => goodsDetails;
			set
			{
				if (SetNonPersistentPropertyValue(GoodsDetailsInfo, ref goodsDetails, value))
				{
					Validate(GoodsDetailsInfo);
				}
			}
		}
		ZString goodsDetails;

		public ZPropertyInfo GoodsDetailsInfo => GetZPropertyInfo(nameof(GoodsDetails));

		#endregion

		#region VoyageFlightNumber

		public ZString VoyageFlightNumber
		{
			get => loadPlan.VoyageFlightNumber;
			set => loadPlan.VoyageFlightNumber = value;
		}

		public ZPropertyInfo VoyageFlightNumberInfo => GetWrappedZPropertyInfo(nameof(VoyageFlightNumber), x => loadPlan.VoyageFlightNumberInfo);

		#endregion

		#region TransitBerthCode

		public ZString TransitBerthCode
		{
			get => loadPlan.TransitBerthCode;
			set => loadPlan.TransitBerthCode = value;
		}

		public ZPropertyInfo TransitBerthCodeInfo => GetWrappedZPropertyInfo(nameof(TransitBerthCode), x => loadPlan.TransitBerthCodeInfo);

		#endregion

		#region ContainerError

		public ZString ContainerError
		{
			get => containerError;
			set
			{
				if (SetNonPersistentPropertyValue(ContainerErrorInfo, ref containerError, value))
				{
				}
			}
		}

		ZString containerError;

		public ZPropertyInfo ContainerErrorInfo => GetZPropertyInfo(nameof(ContainerError));

		#endregion

		#region SetTemperature

		public Measurement SetTemperature
		{
			get => setTemperature;
			set => setTemperature = SetChild(setTemperature, value);
		}

		Measurement setTemperature;

		#endregion

		#region Quantity

		public ZInt PackCount
		{
			get => Groups.Sum(x => x.Quantity);
		}

		#endregion

		#region Weight

		public Measurement Weight
		{
			get => new Measurement
			{
				Value = Groups.Sum(x => x.Weight.Value),
				Unit = new CodeDescription(context.WeightUnits)
				{
					Code = Core.Constants.Weight.Kilograms
				}
			};
		}

		#endregion

		#region Groups

		public IReadOnlyCollection<ContainerLoadPlanSOGrouping> Groups
		{
			get => groups;
			set => groups = SetChildCollection(groups, value ?? System.Array.Empty<ContainerLoadPlanSOGrouping>());
		}

		IReadOnlyCollection<ContainerLoadPlanSOGrouping> groups;

		#endregion

		public IUnloco PortOfLoading => loadPlan.PortOfLoading;
		public IUnloco PortOfDischarge => loadPlan.PortOfDischarge;
		public IUnloco PortOfTranship => loadPlan.PortOfTranship;
		public IUnloco PlaceOfDelivery => loadPlan.PlaceOfDelivery;

		public IMeasurement GrossWeight { get; set; }
		public IMeasurement TareWeight { get; set; }

		public Address SendingAgent => loadPlan.SendingAgent;
		public Address CurrentUser => loadPlan.CurrentUser;
		public Address Carrier => loadPlan.Carrier;
		public Address DepartureCFSAddress => loadPlan.DepartureCFSAddress;

		public IVessel Vessel => loadPlan.Vessel;

		#region IsNonOperativeReefer

		public ZBool IsNonOperativeReefer
		{
			get => isNonOperativeReefer;
			set
			{
				if (SetNonPersistentPropertyValue(IsNonOperativeReeferInfo, ref isNonOperativeReefer, value))
				{
					Validate(IsNonOperativeReeferInfo);
				}
			}
		}

		ZBool isNonOperativeReefer;

		public ZPropertyInfo IsNonOperativeReeferInfo => GetZPropertyInfo(nameof(IsNonOperativeReefer));

		#endregion
	}
}
