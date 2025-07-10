using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.TransportCommon.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefEquipmentValidation : AutoRefEquipmentValidation
	{
		public RefEquipmentValidation(AutoRefEquipment parent) : base(parent)
		{
		}

		new RefEquipment Parent
		{
			get { return (RefEquipment)base.Parent; }
		}

		protected override void CheckRQ_ShortCode()
		{
			base.CheckRQ_ShortCode();
			MandatoryValidation.CheckEntered(Parent.RQ_ShortCodeInfo);
		}

		protected override void CheckRQ_GeoProviderID()
		{
			base.CheckRQ_GeoProviderID();

			if (!Parent.RQ_GeoProviderType.IsEmpty && !Parent.RQ_GeoProviderID.IsEmpty)
			{
				var sameProviderIDQuery = new ZQuery(RefEquipmentSchema.RQ_GeoProviderType, Parent.RQ_GeoProviderType);
				sameProviderIDQuery.AddToFilter(RefEquipmentSchema.RQ_GeoProviderID, Parent.RQ_GeoProviderID);
				sameProviderIDQuery.AddToFilter(RefEquipmentSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

				if (Parent.Factory.Load<RefEquipment>(sameProviderIDQuery).Length > 0)
				{
					Parent.RQ_GeoProviderIDInfo.AddError(Res.GetString("33a939a4-3d7a-4091-bc12-1b275ebd6bf9", "This provider ID has already been used for another vehicle for the same provider type."));
				}
			}
		}

		protected override void CheckRQ_GeoProviderType()
		{
			base.CheckRQ_GeoProviderType();
			if (!string.IsNullOrEmpty(Parent.GPSWarning))
			{
				Parent.RQ_GeoProviderTypeInfo.AddWarning(Res.GetString("7e3f9886-76fe-47f2-b2ba-49558084f214", "{0} at {1} {2}", Parent.GPSWarning, ZDateTime.Now.ToShortDateString(), ZDateTime.Now.ToShortTimeString()));
			}
		}

		protected override void CheckRQ_Registration()
		{
			base.CheckRQ_Registration();
			MandatoryValidation.CheckEntered(Parent.RQ_RegistrationInfo);
		}

		protected override void CheckRQ_Description()
		{
			base.CheckRQ_Description();
			MandatoryValidation.CheckEntered(Parent.RQ_DescriptionInfo);
			TranslatableDataFieldAttribute.Validate(Parent.RQ_DescriptionInfo);
		}

		protected override void CheckRQ_RN_NKRegistrationCountry()
		{
			base.CheckRQ_RN_NKRegistrationCountry();
			if (!Parent.RQ_RN_NKRegistrationCountry.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.RQ_RN_NKRegistrationCountryInfo);
			}
		}

		protected override void CheckRQ_RegState()
		{
			base.CheckRQ_RegState();
			if (!Parent.RQ_RN_NKRegistrationCountry.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.RQ_RegStateInfo);
			}
		}

		protected override void CheckRQ_PackCapacity()
		{
			base.CheckRQ_PackCapacity();

			CompareValidation.CheckNumberNotNegative(Parent.RQ_PackCapacityInfo);
			ValidateRQ_F3_NKPackType();
		}

		protected override void CheckRQ_AddFlag1()
		{
			base.CheckRQ_AddFlag1();
			var transportRegistry = ObjectFactory.Get<ITransportRegistry>();
			if (string.IsNullOrEmpty(transportRegistry.EquipmentTruckSafe.Value) && Parent.RQ_AddFlag1)
			{
				base.Parent.RQ_AddFlag1Info.AddError(Res.GetString("d2a14adf-ea83-43ce-a99f-1dfa303e4158", "No TruckSafe number has been specified in the registry."));
			}
		}

		protected override void CheckRQ_F3_NKPackType()
		{
			base.CheckRQ_F3_NKPackType();
			ListValidation.ErrorIfInvalidCode(Parent.RQ_F3_NKPackTypeInfo);

			if (Parent.RQ_PackCapacity > 0)
			{
				MandatoryValidation.CheckEntered(Parent.RQ_F3_NKPackTypeInfo);
			}
		}

		protected override void CheckRQ_WeightCapacity()
		{
			base.CheckRQ_WeightCapacity();

			CompareValidation.CheckNumberNotNegative(Parent.RQ_WeightCapacityInfo);
			ValidateRQ_WeightUnit();
		}

		protected override void CheckRQ_TareWeight()
		{
			base.CheckRQ_TareWeight();

			CompareValidation.CheckNumberNotNegative(Parent.RQ_TareWeightInfo);
			ValidateRQ_WeightUnit();
		}

		protected override void CheckRQ_CubicCapacity()
		{
			base.CheckRQ_CubicCapacity();

			CompareValidation.CheckNumberNotNegative(Parent.RQ_CubicCapacityInfo);
			ValidateRQ_CubicUnit();
		}

		protected override void CheckRQ_WeightUnit()
		{
			base.CheckRQ_WeightUnit();
			ListValidation.ErrorIfInvalidCode(Parent.RQ_WeightUnitInfo);

			if (Parent.RQ_WeightCapacity > 0)
			{
				MandatoryValidation.CheckEntered(Parent.RQ_WeightUnitInfo);
			}

			if (Parent.RQ_TareWeight > 0)
			{
				MandatoryValidation.CheckEntered(Parent.RQ_WeightUnitInfo);
			}
		}

		protected override void CheckRQ_CubicUnit()
		{
			base.CheckRQ_CubicUnit();
			ListValidation.ErrorIfInvalidCode(Parent.RQ_CubicUnitInfo);

			if (Parent.RQ_CubicCapacity > 0)
			{
				MandatoryValidation.CheckEntered(Parent.RQ_CubicUnitInfo);
			}
		}

		protected override void CheckRQ_GS_NKPreferredDriver()
		{
			base.CheckRQ_GS_NKPreferredDriver();
			ListValidation.ErrorIfInvalidCode(Parent.RQ_GS_NKPreferredDriverInfo);

			if (Parent.RQ_IsVehicle && !Parent.RQ_GS_NKPreferredDriver.IsEmpty)
			{
				ZQuery otherCarsWithSameDriverQuery = new ZQuery(RefEquipmentSchema.RQ_IsVehicle, true);
				otherCarsWithSameDriverQuery.AddToFilter(RefEquipmentSchema.RQ_GS_NKPreferredDriver, Parent.RQ_GS_NKPreferredDriver);
				otherCarsWithSameDriverQuery.AddToFilter(RefEquipmentSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				otherCarsWithSameDriverQuery.OrderBy = RefEquipmentSchema.RQ_ShortCode.Name;

				RefEquipment[] otherCarsWithSameDriver = Parent.Factory.Load<RefEquipment>(otherCarsWithSameDriverQuery);

				if (otherCarsWithSameDriver.Length > 0)
				{
					ZString otherCarCodes = "";

					foreach (RefEquipment otherCar in otherCarsWithSameDriver)
					{
						otherCarCodes += otherCar.RQ_ShortCode + ", ";
					}

					otherCarCodes = otherCarCodes.SubstringSafe(0, otherCarCodes.Length - 2);

					Parent.RQ_GS_NKPreferredDriverInfo.AddWarning(Res.GetString("c6f7a94b-ba2d-48bd-b388-7a5804723328", "This Driver is already allocated to another vehicle {0}", otherCarCodes));
				}
			}
		}

		protected override void CheckRQ_PurchaseDateIsValidZDateTimeRange()
		{
			var limits = new TypeValidationLimits() { PastYearsBeforeError = 50 };
			TypeValidation.CheckValidZDateTimeRange(Parent.RQ_PurchaseDateInfo, limits);
		}
	}
}
