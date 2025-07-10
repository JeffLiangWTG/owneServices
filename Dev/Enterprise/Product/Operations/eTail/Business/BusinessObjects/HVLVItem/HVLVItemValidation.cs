//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoHVLVItemValidation
//
//    This class should be used for overriding validation in AutoHVLVItemValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.eTail.Business
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Freight.Business;
	using Enterprise.Freight.Forwarding.Business;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Schema;
	using ZArchitecture.Core;

	public class HVLVItemValidation : AutoHVLVItemValidation
	{
		public HVLVItemValidation(AutoHVLVItem parent)
			: base(parent)
		{ }

		new HVLVItem Parent
		{
			get { return (HVLVItem)base.Parent; }
		}

		public override void ValidateAll()
		{
			if (Parent.HasChanges || Parent.Consignment?.HasChanges == true)
			{
				base.ValidateAll();
			}
		}

		protected override void CheckHVI_ItemId()
		{
			if (Parent.IsInDatabase)
			{
				CheckEnteredForCountryAndDirection(Parent.HVI_ItemIdInfo);
			}

			DiacriticsValidation.WarnIfContainsAnyDiacritics(Parent.HVI_ItemIdInfo);
		}

		protected override void CheckHVI_ShipperReference()
		{
			if (Parent.HVI_IsActive)
			{
				var query = new ZQuery(HVLVItemSchema.HVI_HVC_Consignment, Parent.HVI_HVC_Consignment);
				query.AddToFilter(new ZQuery(HVLVItemSchema.HVI_IsActive, true));

				var activeItems = Parent.Factory.Load<HVLVItem>(query);

				if (activeItems.Any())
				{
					PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.HVI_ShipperReferenceInfo, activeItems);
				}
			}

			DiacriticsValidation.WarnIfContainsAnyDiacritics(Parent.HVI_ShipperReferenceInfo);
		}

		protected override void CheckHVI_F3_NKPackType()
		{
			ListValidation.ErrorIfInvalidCode(Parent.HVI_F3_NKPackTypeInfo, Parent.Lookups.PackTypes);

			if (Parent.Shipment != null && Parent.Shipment.IsSea && !Parent.HVI_IsUnmanifestedAtDestination)
			{
				CheckEnteredForCountryAndDirection(Parent.HVI_F3_NKPackTypeInfo);
			}
		}

		protected override void CheckHVI_ManifestedWeight()
		{
			CompareValidation.CheckNumberNotNegative(Parent.HVI_ManifestedWeightInfo);

			if (Parent.Shipment != null)
			{
				var manifestedWeight = Parent.HVI_ManifestedWeightInfo.Value;
				if (!manifestedWeight.IsEmpty
					&& Parent.HasItemLines
					&& !manifestedWeight.Equals(Parent.ItemLinesTotalWeight))
				{
					Parent.HVI_ManifestedWeightInfo.AddWarning(Res.GetString("00683692-8668-4df2-9604-23226c61881f", "Manifested weight does not match Item Line Gross Weights."));
				}
				else if ((Parent.Shipment.IsAir || Parent.Shipment.IsSea) && !Parent.HVI_IsUnmanifestedAtDestination)
				{
					CheckEnteredForCountryAndDirection(Parent.HVI_ManifestedWeightInfo);
				}
			}
		}

		protected override void CheckHVI_ActualWeight()
		{
			CompareValidation.CheckNumberNotNegative(Parent.HVI_ActualWeightInfo);
		}

		protected override void CheckHVI_ManifestedVolume()
		{
			CompareValidation.CheckNumberNotNegative(Parent.HVI_ManifestedVolumeInfo);

			if (Parent.Shipment != null && Parent.Shipment.IsSea && !Parent.HVI_IsUnmanifestedAtDestination)
			{
				CheckEnteredForCountryAndDirection(Parent.HVI_ManifestedVolumeInfo);
			}
		}

		protected override void CheckHVI_ContainerNumber()
		{
			if (Parent.Shipment != null && Parent.Shipment.IsSea)
			{
				var containerNumber = Parent.HVI_ContainerNumber;
				if (!containerNumber.IsEmpty)
				{
					if (containerNumber.Length > 12)
					{
						Parent.HVI_ContainerNumberInfo.AddError(Res.GetString("6b1984d8-e1a2-4a5a-8edc-c1fed742cffe", "The container number can be no more than 12 characters long."));
					}

					ContainerNumberValidation.WarnIfInvalid(Parent.HVI_ContainerNumberInfo);

					if (!Parent.Lookups.ContainerNumber_List.ContainsCode(containerNumber))
					{
						Parent.HVI_ContainerNumberInfo.AddWarning(Res.GetString("21f8ee88-4c91-41b7-b0af-a7c12436a6a1", "The container number does not exist on any related consols."));
					}
				}
				else
				{
					Parent.HVI_ContainerNumberInfo.AddWarning(Res.GetString("3c64e382-5447-472b-a951-5bf752cc370f", "The container number should not be empty."));
				}
			}
		}

		protected override void CheckHVI_Height()
		{
			CompareValidation.CheckNumberNotNegative(Parent.HVI_HeightInfo);
		}

		protected override void CheckHVI_Length()
		{
			CompareValidation.CheckNumberNotNegative(Parent.HVI_LengthInfo);
		}

		protected override void CheckHVI_Width()
		{
			CompareValidation.CheckNumberNotNegative(Parent.HVI_WidthInfo);
		}

		protected override void CheckHVI_UnitOfDimension()
		{
			if (Parent.HVI_Width > 0 || Parent.HVI_Height > 0 || Parent.HVI_Length > 0)
			{
				MandatoryValidation.CheckEntered(Parent.HVI_UnitOfDimensionInfo);
			}

			ListValidation.ErrorIfInvalidCode(Parent.HVI_UnitOfDimensionInfo, Parent.Lookups.HVI_UnitOfDimensionList);
		}

		protected override void CheckHVI_ActualVolume()
		{
			CompareValidation.CheckNumberNotNegative(Parent.HVI_ActualVolumeInfo);

			if (Parent.HVI_Width != 0 && Parent.HVI_Height != 0 && Parent.HVI_Length != 0)
			{
				var volumeUnit = Parent.Consignment != null ? Parent.Consignment.HVC_VolumeUQ : ZString.Empty;
				var calculatedVolume = FreightUtilities.CalculateVolume(
					Parent.HVI_ActualVolume,
					1,
					Parent.HVI_Length,
					Parent.HVI_Width,
					Parent.HVI_Height,
					Parent.HVI_UnitOfDimension,
					volumeUnit,
					HVLVItemSchema.HVI_ActualVolume.Scale);

				if (Utilities.Round(calculatedVolume, HVLVItemSchema.HVI_ActualVolume.Scale) !=
					Utilities.Round(Parent.HVI_ActualVolume, HVLVItemSchema.HVI_ActualVolume.Scale))
				{
					Parent.HVI_ActualVolumeInfo.AddWarning(
						Res.GetString("5cdd24a8-0d52-4b10-8b14-d012c2fb5993", "The actual volume does not match the volume calculated by the length x width x height."));
				}
			}
		}

		protected override void CheckHVI_GoodsDescription()
		{
			DiacriticsValidation.WarnIfContainsAnyDiacritics(Parent.HVI_GoodsDescriptionInfo);
		}

		protected override void CheckHVI_CurrentBarcode()
		{
			DiacriticsValidation.WarnIfContainsAnyDiacritics(Parent.HVI_CurrentBarcodeInfo);
		}

		protected override void CheckHVI_IsActive()
		{
			if (Parent.Consignment != null)
			{
				if (Parent.HVI_IsActive && !Parent.Consignment.HVC_IsActive)
				{
					Parent.HVI_IsActiveInfo.AddError(Res.GetString("d84bfed7-6735-44c9-a831-499259d2a863", "Item cannot be active as it is attached to an inactive consignment."));
				}
				else if (Parent.Consignment.HVC_IsActive && Parent.Consignment.Items.OfType<HVLVItem>().All(item => !item.HVI_IsActive))
				{
					Parent.HVI_IsActiveInfo.AddError(Res.GetString("53a2927d-7aa0-4747-a709-e9fd192fcc35", "An active Consignment requires at least one active item."));
				}
			}
		}

		static IReadOnlyCollection<string> mandatoryPropertiesForAUExport => new HashSet<string>
		{
			HVLVItemSchema.Constants.HVI_ManifestedWeight,
			HVLVItemSchema.Constants.HVI_F3_NKPackType,
			HVLVItemSchema.Constants.HVI_ManifestedVolume
		};

		void CheckEnteredForCountryAndDirection(ZPropertyInfo propertyInfo)
		{
			CheckEnteredForCountryAndDirectionCore(propertyInfo, propInfo => MandatoryValidation.CheckEntered(propertyInfo));
		}

		void CheckEnteredForCountryAndDirectionCore(ZPropertyInfo propertyInfo, Action<ZPropertyInfo> mandatoryValidation)
		{
			if (Parent.Consignment?.ManifestedOnShipment is ForwardingShipment manifestedOnShipment
				&& manifestedOnShipment.JS_RL_NKOrigin.Left(2) == Core.Constants.CountryCodes.Australia
				&& manifestedOnShipment.IsExport())
			{
				if (mandatoryPropertiesForAUExport.Contains(propertyInfo.Name))
				{
					mandatoryValidation.Invoke(propertyInfo);
				}
			}
			else
			{
				mandatoryValidation.Invoke(propertyInfo);
			}
		}
	}
}
