using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsInventoryHeldCodeValidation : AutoWhsInventoryHeldCodeValidation
	{
		public WhsInventoryHeldCodeValidation(AutoWhsInventoryHeldCode parent)
			: base(parent)
		{
		}

		#region  CheckWHC_Code

		protected override void CheckWHC_Code()
		{
			base.CheckWHC_Code();

			MandatoryValidation.CheckEntered(Parent.WHC_CodeInfo);
			CheckHeldCodeIsUnique();
			CheckCodeDoesNotContainIllegalCharacterForEventReference();
			AddWarningIfChangingHeldCodeThatIsInUse(Parent.WHC_CodeInfo, Parent.WHC_CodeInfo.HasChanges);
		}

		void CheckHeldCodeIsUnique()
		{
			if (!Parent.WHC_CodeInfo.HasErrors())
			{
				var query = new ZQuery();
				query.AddToFilter(WhsInventoryHeldCodeSchema.WHC_Code, Parent.WHC_Code);
				query.AddToFilter(WhsInventoryHeldCodeSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

				if (Parent.Factory.LoadTop1<WhsInventoryHeldCode>(query) != null)
				{
					Parent.WHC_CodeInfo.AddError(Res.GetString("4c0db216-9d4d-4983-8fc5-dddd86449fce", "Hold Code {0} already exists.", Parent.WHC_Code));
				}
			}
		}

		void CheckCodeDoesNotContainIllegalCharacterForEventReference()
		{
			if (!Parent.WHC_CodeInfo.HasErrors() && Parent.WHC_Code.Contains('|'))
			{
				Parent.WHC_CodeInfo.AddError(Res.GetString("e5d3ef8f-8b17-4de6-bb87-add9f26a35b1", "Cannot use the '|' character in your Hold Code."));
			}
		}

		void AddWarningIfChangingHeldCodeThatIsInUse(ZPropertyInfo propertyInfo, bool hasChanges)
		{
			if (hasChanges && Parent.IsOriginalHeldCodeUsed())
			{
				propertyInfo.AddWarning(Res.GetString("4bd2fd0c-d75f-4f9a-902b-76c6c26dbc94", "Inventory exists with Hold Code {0}. Changing the Hold Code will not update existing Inventory.", Parent.WHC_CodeInfo.OriginalValue.ToString()));
			}
		}

		#endregion

		#region CheckWHC_Description

		protected override void CheckWHC_Description()
		{
			base.CheckWHC_Description();
			MandatoryValidation.CheckEntered(Parent.WHC_DescriptionInfo);
			TranslatableDataFieldAttribute.Validate(Parent.WHC_DescriptionInfo);
		}

		#endregion

		#region CheckWHC_OH_Client

		protected override void CheckWHC_OH_Client()
		{
			base.CheckWHC_OH_Client();

			AddWarningIfChangingHeldCodeThatIsInUse(Parent.WHC_OH_ClientInfo, Parent.WHC_OH_ClientInfo.HasChanges);
		}

		#endregion

		#region Implementation

		protected new WhsInventoryHeldCode Parent
		{
			get { return (WhsInventoryHeldCode)base.Parent; }
		}

		#endregion
	}
}
