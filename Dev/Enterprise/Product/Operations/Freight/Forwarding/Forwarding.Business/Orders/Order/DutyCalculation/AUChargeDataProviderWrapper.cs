using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	class AUChargeDataProviderWrapper : IDeclarationChargeProvider
	{
		public AUChargeDataProviderWrapper(Order order, SetDutyResultDelegate setDutyResult, SetFeeResultDelegate setFeeResult)
		{
			this.order = order;
			this.SetDutyResult = setDutyResult;
			this.SetFeeResult = setFeeResult;

			this.lineDutyDataWrappers = new Dictionary<ZGuid, AUDutyDataWrapper>();
			PopulateLineDutyDataWrappers();
		}

		const string Deminimus = "DEM";
		readonly Order order;
		readonly SetDutyResultDelegate SetDutyResult;
		readonly SetFeeResultDelegate SetFeeResult;
		readonly Dictionary<ZGuid, AUDutyDataWrapper> lineDutyDataWrappers;

		public AUDutyDataWrapper GetLineDutyWrapper(OrderLine orderLine)
		{
			return lineDutyDataWrappers[orderLine.PK];
		}

		#region IDeclarationChargeProvider Members

		bool IDeclarationChargeProvider.IsS162ATemporaryImport
		{
			get { return false; }
		}

		bool IDeclarationChargeProvider.IsSOFADeclaration
		{
			get { return false; }
		}

		ZDate IDeclarationChargeProvider.EffectiveDutyDate
		{
			get
			{
				var result = order.GetMilestoneActualDate(Events.Arrival);

				if (result.IsEmpty)
				{
					result = order.GetMilestoneEstimatedDate(Events.Arrival);
				}

				if (result.IsEmpty)
				{
					result = ZDateTime.Today;
				}

				return result.Date;
			}
		}

		int IDeclarationChargeProvider.NumberOfFCLContainers
		{
			get { return GetNumberOfContainers(Core.Constants.ContainerModes.FCL); }
		}

		int IDeclarationChargeProvider.NumberOfFCXContainers
		{
			get { return 0; } // FCX is not a valid mode on Order.
		}

		int IDeclarationChargeProvider.NumberOfLCLContainers
		{
			get { return GetNumberOfContainers(Core.Constants.ContainerModes.LCL); }
		}

		int GetNumberOfContainers(string containerMode)
		{
			int result = 0;

			if (order.JD_ContainerMode == containerMode)
			{
				foreach (OrderContainer container in order.PlannedContainers)
				{
					result += container.J1_ContainerCount;
				}
			}

			return result;
		}

		TransportModeEnum IDeclarationChargeProvider.TransportMode
		{
			get
			{
				TransportModeEnum result = TransportModeEnum.Undefined;

				if (!order.JD_TransportMode.IsEmpty)
				{
					switch (order.JD_TransportMode)
					{
						case Core.Constants.TransportModes.Air:
							result = TransportModeEnum.Air;
							break;

						case Core.Constants.TransportModes.Sea:
							result = TransportModeEnum.Sea;
							break;

						case Core.Constants.TransportModes.Mail:
							result = TransportModeEnum.Post;
							break;

						default:
							result = TransportModeEnum.Other;
							break;
					}
				}

				return result;
			}
		}

		ZDecimal IDeclarationChargeProvider.N10CustomsValue
		{
			get { return order.TotalCustomsValue; }
		}

		ZDecimal IDeclarationChargeProvider.N20CustomsValue
		{
			get { return ZDecimal.Zero; }
		}

		ZDecimal IDeclarationChargeProvider.N30CustomsValue
		{
			get { return ZDecimal.Zero; }
		}

		bool IDeclarationChargeProvider.IsExemptedFromCustomsAndQuarantineFees
		{
			get { return order.IsExemptedFromCustomsAndQuarantineFees; }
		}
		#endregion

		#region IHeaderFeeData Members

		IEnumerable<Customs.Common.ILineDutyData> Customs.Common.IHeaderFeeData.Lines
		{
			get
			{
				List<AUDutyDataWrapper> dutyDataList = new List<AUDutyDataWrapper>(lineDutyDataWrappers.Values);

				System.Collections.IComparer comparer = new OrderLineComparer();
				dutyDataList.Sort(delegate(AUDutyDataWrapper x, AUDutyDataWrapper y)
				{ return comparer.Compare(x.OrderLine, y.OrderLine); });

				foreach (AUDutyDataWrapper dutyData in dutyDataList)
				{
					if (dutyData.IsProductClassified)
					{
						yield return dutyData;
					}
				}
			}
		}

		void Customs.Common.IHeaderFeeData.SetFeeResult(ZString feeType, ZDecimal feeAmount)
		{
			SetFeeResult(order.PK, feeType, feeAmount);
		}

		#endregion

		#region Implementation

		bool IsSACEntry
		{
			get
			{
				var factory = order?.Factory ?? new BusinessObjectFactory();
				var deminimus = factory.GetCachedValue((NoResString)"Deminimus", // Key used in Factory Cache
				() =>
				{
					return new RefCusTaxOrFee.Loader(factory).LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.Australia, Deminimus, ZDate.Today)?.ZZF_Value ?? ZDecimal.Zero;
				});
				return order.TotalCustomsValue <= deminimus;
			}
		}

		void PopulateLineDutyDataWrappers()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			foreach (OrderLine orderLine in order.NonCancelledOrderLines)
			{
				AUDutyDataWrapper dutyData = new AUDutyDataWrapper(orderLine, IsSACEntry, factory, SetDutyResult, SetFeeResult);

				lineDutyDataWrappers[orderLine.PK] = dutyData;
			}
		}

		#endregion
	}
}
