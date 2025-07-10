using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class IMProductValueDefaultOptionValidation : ZValidation
	{
		public IMProductValueDefaultOptionValidation(IMProductValueDefaultOption productValueDefaultOption)
			: base(productValueDefaultOption)
		{
			this.Parent = productValueDefaultOption;
			this.ZValidationInternals = this;
		}
		readonly IMProductValueDefaultOption Parent;
		readonly IValidationInternals ZValidationInternals;

		public override Type AutoValidationType => typeof(IMProductValueDefaultOption);

		public override void ValidateAll()
		{
			ValidateFieldType();
		}

		public void ValidateFieldType()
		{
			ZValidationInternals.Validate(Parent.FieldTypeInfo, GetFieldTypeValidationInvoker());
		}

		RunValidationInvoker GetFieldTypeValidationInvoker() => delegate { CheckFieldType(); };

		public void CheckFieldType()
		{
			ListValidation.ErrorIfInvalidCode(Parent.FieldTypeInfo);

			if (!Parent.FieldTypeInfo.HasErrors())
			{
				var collection = Parent.ParentCollections.Count > 0 ? Parent.ParentCollections.First() as IMProductValueDefaultOptionCollection : null;
				if (collection != null && collection.Cast<IMProductValueDefaultOption>().Any(x => x.FieldType == Parent.FieldType && x != Parent))
				{
					Parent.FieldTypeInfo.AddError(DuplicatedError);
				}
			}
		}

		internal static string DuplicatedError => ResString.GetMultilingualString("d55b7367-5b04-442b-8a2c-804fc6a933e7", "The field types cannot be duplicated.");
	}
}
