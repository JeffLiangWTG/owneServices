using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefContainerValidation : AutoRefContainerValidation
	{
		public RefContainerValidation(AutoRefContainer parent) : base(parent)
		{
		}

		new RefContainer Parent
		{
			get { return (RefContainer)base.Parent; }
		}

		protected override void CheckRC_FreightRateClass()
		{
			base.CheckRC_FreightRateClass();
			ListValidation.ErrorIfInvalidCode(Parent.RC_FreightRateClassInfo);
		}

		protected override void CheckRC_ISOEquipmentSizeTypeCode()
		{
			base.CheckRC_ISOEquipmentSizeTypeCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.RC_ISOEquipmentSizeTypeCodeInfo);
		}

		protected override void CheckRC_HandlingRateClass()
		{
			base.CheckRC_HandlingRateClass();
			ListValidation.ErrorIfInvalidCode(Parent.RC_HandlingRateClassInfo);
		}

		protected override void CheckRC_StorageClass()
		{
			base.CheckRC_StorageClass();
			ListValidation.ErrorIfInvalidCode(Parent.RC_StorageClassInfo);
		}

		protected override void CheckRC_CubicCapacity()
		{
			base.CheckRC_CubicCapacity();

			if (Parent.RC_CubicCapacity < 0)
			{
				Parent.RC_CubicCapacityInfo.AddError(Res.GetString("3400c837-373e-47ae-8f84-23555b7d1f6b", "Cubic Capacity cannot be a value less than zero."));
			}
		}

		protected override void CheckRC_ContainerType()
		{
			base.CheckRC_ContainerType();

			if (Parent.IsSeaContainer)
			{
				MandatoryValidation.CheckEntered(Parent.RC_ContainerTypeInfo);
			}

			ListValidation.ErrorIfInvalidCode(Parent.RC_ContainerTypeInfo);
		}

		protected override void CheckRC_GrossWeight()
		{
			base.CheckRC_GrossWeight();

			if (Parent.RC_GrossWeight < 0)
			{
				Parent.RC_GrossWeightInfo.AddError(Res.GetString("15335495-3bdc-4e0d-ba51-26e285d6781e", "Gross Weight cannot be a value less than zero."));
			}

			if (Parent.RC_GrossWeight < Parent.RC_TareWeight)
			{
				Parent.RC_GrossWeightInfo.AddError(Res.GetString("a4d10d75-32df-4287-b7ba-f6536e3aca68", "Gross Weight cannot be a value less than Tare Weight."));
			}
		}

		protected override void CheckRC_Code()
		{
			base.CheckRC_Code();
			MandatoryValidation.CheckEntered(Parent.RC_CodeInfo);

			if (Parent.RC_Code.Length > 0)
			{
				if (!CheckMinLength(Parent.RC_Code, 4) && !Parent.IsAirContainer)
				{
					Parent.RC_CodeInfo.AddError(Res.GetString("b728be91-0f15-45ea-b5fe-84e1df845fc0", "Code must be at least 4 characters long."));
				}
				else if (!CheckMinLength(Parent.RC_Code, 3))
				{
					Parent.RC_CodeInfo.AddError(Res.GetString("9204a2e5-b3be-18b9-40a2-c3726f899c90", "Code must be at least 3 characters long."));
				}

				ZQuery sQLFilter = new ZQuery();
				sQLFilter.AddToFilter(RefContainerSchema.RC_Code, Parent.RC_Code);
				sQLFilter.AddToFilter(RefContainerSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

				RefContainer[] duplicates = Parent.Factory.Load<RefContainer>(sQLFilter);

				if (duplicates.Length != 0)
				{
					Parent.RC_CodeInfo.AddError(Res.GetString("3acb6a9e-237e-4823-b625-4aa3415380ff", "The specified container code {0} already exists.", Parent.RC_Code.ToString()));
				}
			}

			ValidateRC_ISOType();
		}

		protected override void CheckRC_Description()
		{
			base.CheckRC_Description();
			MandatoryValidation.CheckEntered(Parent.RC_DescriptionInfo);

			if (!CheckMinLength(Parent.RC_Description, 4))
			{
				Parent.RC_DescriptionInfo.AddError(Res.GetString("ac10a99b-beca-42b7-a328-d669d257dbf1", "Description must be at least 4 characters long."));
			}

			TranslatableDataFieldAttribute.Validate(Parent.RC_DescriptionInfo);
		}

		protected override void CheckRC_Length()
		{
			base.CheckRC_Length();

			if (Parent.RC_Length < 0)
			{
				Parent.RC_LengthInfo.AddError(Res.GetString("be806f0a-78ad-4bf9-8ddc-a778d81f4e38", "Length cannot be a value less than zero."));
			}
		}

		protected override void CheckRC_Width()
		{
			base.CheckRC_Width();

			if (Parent.RC_Width < 0)
			{
				Parent.RC_WidthInfo.AddError(Res.GetString("110b4ad2-b620-40cf-8742-d95ac733b77a", "Width cannot be a value less than zero."));
			}
		}

		protected override void CheckRC_Height()
		{
			base.CheckRC_Height();

			if (Parent.RC_Height < 0)
			{
				Parent.RC_HeightInfo.AddError(Res.GetString("8ad4daea-1b0c-4d54-8997-6d7135c8c245", "Height cannot be a value less than zero."));
			}
		}

		protected override void CheckRC_TareWeight()
		{
			base.CheckRC_TareWeight();

			if (Parent.RC_TareWeight < 0)
			{
				Parent.RC_TareWeightInfo.AddError(Res.GetString("fc9ffad5-dcc4-4824-98ac-4ecb7f178fc6", "Tare Weight cannot be a value less than zero."));
			}
		}

		protected override void CheckRC_TEU()
		{
			base.CheckRC_TEU();

			if (Parent.RC_TEU < 0)
			{
				Parent.RC_TEUInfo.AddError(Res.GetString("b9aad0d8-e7a6-48c0-bd85-a89df2505ae3", "TEU cannot be a value less than zero."));
			}
		}

		protected override void CheckRC_ISOType()
		{
			base.CheckRC_ISOType();

			var isoType = Parent.RC_ISOType;
			if (!isoType.IsEmpty)
			{
				var code = Parent.RC_Code;
				if (!Parent.Lookups.ISOTypes.Cast<ContainerISOType>().Any(x => x.ISOCode == isoType))
				{
					Parent.RC_ISOTypeInfo.AddWarning(ResString.GetMultilingualString("E1B52D5D-A4A2-429E-9487-7462EDC64182", "Please enter a valid ISO type."));
				}
				if (!code.IsEmpty)
				{
					var existingRecordQuery = new ZQuery(RefContainerSchema.RC_ContainerType, Parent.RC_ContainerType);
					existingRecordQuery.AddToFilter(RefContainerSchema.RC_ISOType, isoType);
					existingRecordQuery.AddToFilter(RefContainerSchema.RC_Code, SQLComparisonOperator.NotEqual, code);
					if (Parent.Factory.Exists(typeof(RefContainer), existingRecordQuery))
					{
						Parent.RC_ISOTypeInfo.AddWarning(Res.GetString("571E705F-B8AD-4CFE-B4E6-B9242AA64724", "ISO Type must be unique per container and Container Type (Dry, Refrigerated, etc.). Non-unique ISO type could cause EDI errors. To use the same ISO Type on more than one container record, amend the Container Type."));
					}
				}
			}
		}

		protected override void CheckRC_IATARateClass()
		{
			base.CheckRC_IATARateClass();
			if (Parent.IsAirContainer)
			{
				MandatoryValidation.CheckEntered(Parent.RC_IATARateClassInfo);
			}

			ListValidation.ErrorIfInvalidCode(Parent.RC_IATARateClassInfo);
		}

		bool CheckMinLength(string inValue, int minLength)
		{
			bool result = false;

			if (inValue.Length >= minLength)
			{
				result = true;
			}

			return result;
		}
	}
}
