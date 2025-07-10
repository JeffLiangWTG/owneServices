using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Packing.Business
{
	public class PkgPackageValidationForUnfinalisedPackageJob : PkgPackageValidation
	{
		public PkgPackageValidationForUnfinalisedPackageJob(PkgPackage parent)
			: base(parent)
		{
		}

		#region ValidateKP_PackageQty

		protected override void CheckKP_PackageQty()
		{
			base.CheckKP_PackageQty();

			if (Parent.OuterPackage.KP_KJ_ParentPackageJob.IsValid)
			{
				var parentPackage = Parent.ParentPackage;
				if (parentPackage != null && parentPackage.KP_PackageQty > 0)
				{
					// We do not call Validation.ValidateKP_PackageQty() because we do not want to recursively Validate PackageQty,
					// ValidatePackageQtyInfo simply runs the supplied validation for the supplied package only.
					ValidatePackageQtyInfo(parentPackage, CheckPackageQtyDivisibility);
				}

				CheckPackageQtyDivisibility(Parent);
			}
		}

		void CheckPackageQtyDivisibility(PkgPackage package)
		{
			if (package.KP_PackageQty > 0)
			{
				var innerPackageQtySum = package.Packages.Sum(p => p.KP_PackageQty);
				if (innerPackageQtySum % package.KP_PackageQty != 0)
				{
					package.KP_PackageQtyInfo.AddError(Res.GetString("e9f4791a-b52c-40a2-a0ce-57e2d338fc53", "The sum of All inner package quantities must be divisible by the outer package quantity."));
				}
			}
		}

		#endregion

		#region ValidateKP_F3_NKPackType

		protected override void CheckKP_F3_NKPackType()
		{
			base.CheckKP_F3_NKPackType();

			if (!Parent.KP_F3_NKPackTypeInfo.HasErrors())
			{
				MandatoryValidation.CheckEntered(Parent.KP_F3_NKPackTypeInfo);

				if (!Parent.KP_F3_NKPackTypeInfo.HasErrors())
				{
					ListValidation.ErrorIfInvalidCode(Parent.KP_F3_NKPackTypeInfo);
				}

				if (!Parent.KP_F3_NKPackTypeInfo.HasErrors() && Parent.IsContainer && !Parent.IsOuter)
				{
					Parent.KP_F3_NKPackTypeInfo.AddError(Res.GetString("da08b119-27b3-4140-88bc-728ca7d7df13",
						"Containers cannot be packed into other Packages."));
				}
			}
		}

		#endregion

		#region ValidateKP_Weight

		protected override void CheckKP_Weight()
		{
			base.CheckKP_Weight();
			if (Parent.KP_Weight < 0)
			{
				Parent.KP_WeightInfo.AddError(Res.GetString("0d484823-bdd3-45c5-b384-47341712995b", "Package Weight cannot be less than 0."));
			}
		}

		#endregion

		#region ValidateKP_Volume

		protected override void CheckKP_Volume()
		{
			base.CheckKP_Volume();
			if (Parent.KP_Volume < 0)
			{
				Parent.KP_VolumeInfo.AddError(Res.GetString("77edc2c4-74d5-4640-8bd6-20c70026dfdc", "Package Volume cannot be less than 0."));
			}
		}

		#endregion

		#region ValidateKP_Length

		protected override void CheckKP_Length()
		{
			base.CheckKP_Length();
			if (Parent.KP_Length < 0)
			{
				Parent.KP_LengthInfo.AddError(Res.GetString("2970613c-58b0-4c28-b172-bbcf4c997533", "Package Length cannot be less than 0."));
			}
		}

		#endregion

		#region ValidateKP_Width

		protected override void CheckKP_Width()
		{
			base.CheckKP_Width();
			if (Parent.KP_Width < 0)
			{
				Parent.KP_WidthInfo.AddError(Res.GetString("200db75c-da4e-42a0-b4f2-5fd0ac2ac7a9", "Package Width cannot be less than 0."));
			}
		}

		#endregion

		#region ValidateKP_Height

		protected override void CheckKP_Height()
		{
			base.CheckKP_Height();
			if (Parent.KP_Height < 0)
			{
				Parent.KP_HeightInfo.AddError(Res.GetString("70ef76c5-9581-48df-b69c-46b261b8deb2", "Package Height cannot be less than 0."));
			}
		}

		#endregion

		#region ValidateKP_WeightUQ

		protected override void CheckKP_WeightUQ()
		{
			base.CheckKP_WeightUQ();

			if (Parent.KP_Weight > 0)
			{
				MandatoryValidation.CheckEntered(Parent.KP_WeightUQInfo);
			}
			if (!Parent.KP_WeightUQInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(Parent.KP_WeightUQInfo);
			}
		}

		#endregion

		#region ValidateKP_VolumeUQ

		protected override void CheckKP_VolumeUQ()
		{
			base.CheckKP_VolumeUQ();

			if (Parent.KP_Volume > 0)
			{
				MandatoryValidation.CheckEntered(Parent.KP_VolumeUQInfo);
			}
			if (!Parent.KP_VolumeUQInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(Parent.KP_VolumeUQInfo);
			}
		}

		#endregion

		#region ValidateKP_DimensionUQ

		protected override void CheckKP_DimensionUQ()
		{
			base.CheckKP_DimensionUQ();

			if (Parent.KP_Length > 0 || Parent.KP_Width > 0 || Parent.KP_Height > 0)
			{
				MandatoryValidation.CheckEntered(Parent.KP_DimensionUQInfo);
			}
			if (!Parent.KP_DimensionUQInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(Parent.KP_DimensionUQInfo);
			}
		}

		#endregion

		#region ValidateKP_PackageID

		protected override void CheckKP_PackageID()
		{
			base.CheckKP_PackageID();

			if (!Parent.KP_PackageID.IsEmpty && Parent.KP_PackageQty > 1)
			{
				Parent.KP_PackageIDInfo.AddError(Res.GetString("10c1a4e1-ac99-4e9c-aca6-d55d9773e42f",
					"A Package ID is for a single Package. Either change the Package Qty to 1 or remove the Package ID."));
			}
			else if (!Parent.IsPackageIdValid)
			{
				Parent.KP_PackageIDInfo.AddError(Res.GetString("fcfbb74d-c988-4705-994a-6c5bc834d25f",
					"Package ID '{0}' is assigned to another package (IDs must be unique per Job).", Parent.KP_PackageID));
			}
			else
			{
				CheckKP_PackageID_WhenContainer();
			}
		}

		protected void CheckKP_PackageID_WhenContainer()
		{
			if (Parent.IsContainer)
			{
				ContainerNumberValidation.AddNotificationIfInvalid(Parent.KP_PackageIDInfo, Parent.PackageJob?.ParentJob?.NotificationTypeForInvalidContainerNumber ?? NotificationTypes.None);
				if (Parent.KP_PackageID.Length > 12)
				{
					Parent.KP_PackageIDInfo.AddError(Res.GetString("7d866605-25a4-4e2b-b332-a486e2b64deb", "The Container Package ID cannot be more than 12 characters."));
				}
			}
		}

		#endregion

		#region ValidateKP_PreviousPackageID

		protected override void CheckKP_PreviousPackageID()
		{
			base.CheckKP_PreviousPackageID();

			CheckKP_PreviousPackageID_OnlyHasValueIfPackageIdHasValue();
			CheckKP_PreviousPackageID_DoesNotHaveTheSameValueAsPackageID();
		}

		void CheckKP_PreviousPackageID_OnlyHasValueIfPackageIdHasValue()
		{
			if (!Parent.KP_PreviousPackageIDInfo.HasErrors() && !Parent.KP_PreviousPackageID.IsEmpty && Parent.KP_PackageID.IsEmpty)
			{
				Parent.KP_PreviousPackageIDInfo.AddError(Res.GetString("6fbba680-b445-49be-af21-bf1fb38009ad", "Previous Package ID should not be entered if there is no new Package ID."));
			}
		}

		void CheckKP_PreviousPackageID_DoesNotHaveTheSameValueAsPackageID()
		{
			if (!Parent.KP_PreviousPackageIDInfo.HasErrors() && !Parent.KP_PreviousPackageID.IsEmpty && Parent.KP_PackageID.EqualsIgnoringCase(Parent.KP_PreviousPackageID))
			{
				Parent.KP_PreviousPackageIDInfo.AddError(Res.GetString("554c6b17-5f78-4e3e-be39-441cd89cc32c", "Previous Package ID cannot be the same as the current Package ID."));
			}
		}

		#endregion

		#region ValidateKP_IsReleased

		protected override void CheckKP_ReleasedTimeUtc()
		{
			base.CheckKP_ReleasedTimeUtc();

			if (Parent.IsReleased && !Parent.IsOuter)
			{
				Parent.KP_ReleasedTimeUtcInfo.AddError(Res.GetString("2ca7ddcc-7e58-4a46-85ba-1a65c6ac229b",
					"An Inner Package cannot be Released. Either Move this package up to the top level, or Cancel it's Release."));
			}
		}

		#endregion

		#region ValidateKP_IsReleasedViaJob

		protected override void CheckKP_IsReleasedViaJob()
		{
			base.CheckKP_IsReleasedViaJob();

			if (Parent.KP_IsReleasedViaJob && !Parent.IsOuter)
			{
				Parent.KP_IsReleasedViaJobInfo.AddError(Res.GetString("924b147c-7f16-4568-872a-8697fbd518de",
					"An Inner Package cannot be Released. Either Move this package up to the top level or Delete it."));
			}
		}

		#endregion

		#region ValidateKP_RH_NKCommodityCode

		protected override void CheckKP_RH_NKCommodityCode()
		{
			base.CheckKP_RH_NKCommodityCode();
			ListValidation.ErrorIfInvalidCode(Parent.KP_RH_NKCommodityCodeInfo);
		}

		#endregion

		// temperature

		#region ValidateKP_RequiredTemperatureMinimum

		protected override void CheckKP_RequiredTemperatureMinimum()
		{
			base.CheckKP_RequiredTemperatureMinimum();

			if (!Parent.KP_RequiredTemperatureMinimumInfo.HasErrors() && IsTemperatureInvalid)
			{
				Parent.KP_RequiredTemperatureMinimumInfo.AddError(Res.GetString("ac258832-d59d-4b40-9d50-5ee71cbf912d",
					"The Minimum Temperature cannot be greater than the Maximum Temperature."));
			}
		}

		#endregion

		#region ValidateKP_RequiredTemperatureMaximum

		protected override void CheckKP_RequiredTemperatureMaximum()
		{
			base.CheckKP_RequiredTemperatureMaximum();

			if (!Parent.KP_RequiredTemperatureMaximumInfo.HasErrors() && IsTemperatureInvalid)
			{
				Parent.KP_RequiredTemperatureMaximumInfo.AddError(Res.GetString("9c32e6bc-8077-477f-9744-480a0882fa74",
					"The Maximum Temperature cannot be less than the Minimum Temperature."));
			}
		}

		#endregion

		#region ValidateKP_RequiredTemperatureUnit

		protected override void CheckKP_RequiredTemperatureUnit()
		{
			base.CheckKP_RequiredTemperatureUnit();

			if (Parent.KP_RequiresTemperatureControl)
			{
				MandatoryValidation.CheckEntered(Parent.KP_RequiredTemperatureUnitInfo);
			}

			if (!Parent.KP_RequiredTemperatureUnitInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(Parent.KP_RequiredTemperatureUnitInfo);
			}
		}

		#endregion

		#region ValidateKP_TareWeight

		protected override void CheckKP_TareWeight()
		{
			base.CheckKP_TareWeight();
			if (Parent.KP_TareWeight < 0)
			{
				Parent.KP_TareWeightInfo.AddError(Res.GetString("0b1f20fd-79df-48f9-ad52-af1d6dcacaa2", "Tare Weight cannot be less than 0."));
			}
		}

		#endregion

		#region ValidateKP_DunnageWeight

		protected override void CheckKP_DunnageWeight()
		{
			base.CheckKP_DunnageWeight();
			if (Parent.KP_DunnageWeight < 0)
			{
				Parent.KP_DunnageWeightInfo.AddError(Res.GetString("BFC77CC2-6155-4152-990C-A285DF611248", "Dunnage Weight cannot be less than 0."));
			}
		}

		#endregion

		bool IsTemperatureInvalid
		{
			get { return Parent.KP_RequiredTemperatureMinimum > Parent.KP_RequiredTemperatureMaximum; }
		}

		// calculated

		#region ValidateGoodsWeight

		protected override void CheckGoodsWeight()
		{
			base.CheckGoodsWeight();

			if (Parent.GoodsWeight < 0)
			{
				Parent.GoodsWeightInfo.AddError(Res.GetString("48b48956-9fae-4b6e-b126-67a905f1abcd", "Goods Weight, calculated by Package Weight({0}{1}) - Package Tare Weight({2}{1}), cannot be less than 0.", Parent.KP_Weight, Parent.KP_WeightUQ, Parent.KP_TareWeight));
			}
		}

		#endregion
	}
}

