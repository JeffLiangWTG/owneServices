using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.CarbonEmissions.Integration;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.CarbonEmissions.Business
{
	public static class CO2eHelper
	{
		public const string CO2eCalculationID = "EMISSION_CALCULATOR";

		public static bool IsApiEnabled => FreightDataRegistry.Instance.CO2eUserRequestProcessingMethod.Value == CO2eUserRequestProcessingMethodCodeList.Codes.Api;

		public static ZString GetFormattedCO2e(ZDecimal cO2e)
		{
			var result = Utilities.FormatNumberNationalWithGroupSeparators((decimal)cO2e, 3);
			var decimalSeparatorChar = Culture.CurrentCompanyCountryCulture.NumberFormat.NumberDecimalSeparator[0];
			if (result.Contains(decimalSeparatorChar))
			{
				result = result.TrimEnd('0').TrimEnd(decimalSeparatorChar);
			}

			return result;
		}

		public static ITopLevelDataObjectWriter GetCO2eRequestDataObjectWriter(BusinessObject bizo, params object[] arguments)
		{
			bool hasHostSupporter = arguments.Any(arg => arg is ICO2eCalculationSupporter);

			var writingManager = new DataWritingManager(
				new ManualCO2eCalculationActionInfo(bizo),
				schema: UniversalXmlSchema.Version_2012_11_DO_NOT_USE);

			var args = new List<object>();

			if (arguments.Length == 0 || (hasHostSupporter && arguments.Length == 1))
			{
				args.Add(writingManager);
			}

			args.AddRange(arguments);

			var providers = ObjectFactory.Get<Hashtable>("CO2eRequestDataObjectWriters");
			var providerHandle = (ObjectHandle)providers[bizo.TablePrefix];
			return (ITopLevelDataObjectWriter)providerHandle?.GetObject(args.ToArray());
		}

		public static void AddFirstLeg(this List<PrePostCarriageLegWrapper> legs, IPrePostCarriageLocation from, IPrePostCarriageLocation to, string transportMode = default)
		{
			if (from is { IsEmpty: false } && to is { IsEmpty: false } && !from.Equals(to))
			{
				legs.Insert(0, new PrePostCarriageLegWrapper(from, to, transportMode));
			}
		}

		public static void AddLastLeg(this List<PrePostCarriageLegWrapper> legs, IPrePostCarriageLocation from, IPrePostCarriageLocation to, string transportMode = default)
		{
			if (from is { IsEmpty: false } && to is { IsEmpty: false } && !from.Equals(to))
			{
				legs.Add(new PrePostCarriageLegWrapper(from, to, transportMode));
			}
		}

		public static bool IsDoorPickup(ZString hblDeliveryMode)
		{
			return hblDeliveryMode == Core.Constants.HBLDeliveryModes.Codes.DOOR_CY
				|| hblDeliveryMode == Core.Constants.HBLDeliveryModes.Codes.DOOR_CFS
				|| hblDeliveryMode == Core.Constants.HBLDeliveryModes.Codes.DOOR_ARPT
				|| hblDeliveryMode == Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR
				|| hblDeliveryMode == Core.Constants.HBLDeliveryModes.Codes.DOOR_PORT;
		}

		public static bool IsCFSPickup(ZString hblDeliveryMode)
		{
			return hblDeliveryMode == Core.Constants.HBLDeliveryModes.Codes.CFS_CY
				|| hblDeliveryMode == Core.Constants.HBLDeliveryModes.Codes.CFS_CFS
				|| hblDeliveryMode == Core.Constants.HBLDeliveryModes.Codes.CFS_ARPT
				|| hblDeliveryMode == Core.Constants.HBLDeliveryModes.Codes.CFS_DOOR;
		}

		public static bool IsDoorDelivery(ZString hblDeliveryMode)
		{
			return hblDeliveryMode == Core.Constants.HBLDeliveryModes.Codes.CY_DOOR
				|| hblDeliveryMode == Core.Constants.HBLDeliveryModes.Codes.CFS_DOOR
				|| hblDeliveryMode == Core.Constants.HBLDeliveryModes.Codes.ARPT_DOOR
				|| hblDeliveryMode == Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR
				|| hblDeliveryMode == Core.Constants.HBLDeliveryModes.Codes.PORT_DOOR;
		}

		public static bool IsCFSDelivery(ZString hblDeliveryMode)
		{
			return hblDeliveryMode == Core.Constants.HBLDeliveryModes.Codes.CY_CFS
				|| hblDeliveryMode == Core.Constants.HBLDeliveryModes.Codes.CFS_CFS
				|| hblDeliveryMode == Core.Constants.HBLDeliveryModes.Codes.ARPT_CFS
				|| hblDeliveryMode == Core.Constants.HBLDeliveryModes.Codes.DOOR_CFS;
		}

		public static ZString? GetCO2eStatusShortDescription(ZString? code)
		{
			return ListHelper.GetDescription(code ?? ZString.Empty, new CO2eStatusList())?.Split(':').FirstOrDefault();
		}

		public static void HookUpdateToNotCurrentEvents<T, E>(ICO2eProvider supporter,
			BusinessObjectCollection<E> collection,
			Func<T> oldValue,
			Action<T> action,
			Func<ICO2eProvider, T> propertyAccessor,
			Func<E, List<ZPropertyInfo>> infosGetter,
			Func<EventArgs, CO2eStatusChangedReason> reasonGetter)
			where E : BusinessObject
		{
			void OnPropertyChanged(object s, EventArgs e)
			{
				var newValue = propertyAccessor(supporter);
				if (EqualityComparer<T>.Default.Equals(oldValue(), newValue))
				{
					return;
				}

				supporter.UpdateCO2eStatusToNotCurrent(reasonGetter(e));
				action(newValue);
			}

			collection.Cast<E>().ForEach(bizo => infosGetter(bizo).ForEach(info => info.ValueChanged += OnPropertyChanged));
			collection.CountChanged += (s, e) =>
			{
				var supporterBizo = supporter as BusinessObject;

				if (e.BizObject is not E bizo || supporterBizo.IsDeleted || supporterBizo.IsDeleting)
				{
					return;
				}

				OnPropertyChanged(s, e);
				if (e.ItemAdded)
				{
					infosGetter(bizo).ForEach(info => info.ValueChanged += OnPropertyChanged);
				}
				else if (e.ItemRemoved)
				{
					infosGetter(bizo).ForEach(info => info.ValueChanged -= OnPropertyChanged);
				}
			};
		}
	}
}
