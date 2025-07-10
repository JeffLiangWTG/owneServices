using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(RefPremisesGateCode.Schema.R5_PremisesGateCode), DescriptionProperty(RefPremisesGateCode.Schema.R5_PremisesGateDescription)]
	public class RefPremisesGateCode : AutoRefPremisesGateCode
	{
		public RefPremisesGateCode(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties
		[List("Lookups.R5_DataProvider_List")]
		public override ZString R5_DataProvider
		{
			get
			{
				return base.R5_DataProvider;
			}
			set
			{
				base.R5_DataProvider = value;
			}
		}

		[List("Lookups.R5_OrgRegCode_List")]
		public override ZString R5_OrgRegCodeType
		{
			get
			{
				return base.R5_OrgRegCodeType;
			}
			set
			{
				base.R5_OrgRegCodeType = value;
			}
		}

		protected override ZString HumanReadableNameCore => Res.GetString("0DCF955B-D2F4-4C84-8C3F-E81DD4A87152", "Premises Codes - {0}(Provider: {1})", CalculateShortcutName(), R5_DataProvider);

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region Validation

		protected override RefPremisesGateCodeValidation GetNewValidation()
		{
			return new RefPremisesGateCodeValidation(this);
		}

		#endregion

		#region ICanDelete

		public override bool CanDelete
		{
			get
			{
				return !R5_IsSystem;
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				return ResString.GetMultilingualString("5028a35f-eadd-4089-a0af-953e29ff6489", "System defined Premises Gate Code cannot be deleted");
			}
		}

		#endregion

		#region Test Data
#if DEBUG

		protected override BusinessObjectTestDataHelper NewBusinessObjectTestDataHelper()
		{
			return new MyBusinessObjectTestDataHelper();
		}

		sealed class MyBusinessObjectTestDataHelper : BusinessObjectTestDataHelper
		{
			protected override string GetUniqueStringForProperty(ZPropertyInfo property, System.ComponentModel.PropertyDescriptor[] propertyPath, int maxLength)
			{
				if (property.Name == RefPremisesGateCodeSchema.Constants.R5_OrgRegCodeType
					|| property.Name == RefPremisesGateCodeSchema.Constants.R5_DataProvider)
				{
					return new ZString(PremiseGateCodeDataProviderList.Codes.OneStop);
				}

				return base.GetUniqueStringForProperty(property, propertyPath, maxLength);
			}
		}
#endif
		#endregion
	}
}
