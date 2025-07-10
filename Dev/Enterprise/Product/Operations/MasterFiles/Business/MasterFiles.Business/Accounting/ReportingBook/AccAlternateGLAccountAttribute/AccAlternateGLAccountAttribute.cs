using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.MasterFiles.Business
{
	public class AccAlternateGLAccountAttribute : AutoAccAlternateGLAccountAttribute
	{
		public AccAlternateGLAccountAttribute(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZString ValueDescription => GetDescription();

		string GetDescription()
		{
			var result = string.Empty;

			switch (AAA_Attribute)
			{
				case AlternateGLAccountAttributeCode.ORG:
					var org = Factory.Load<OrgHeader>(AAA_AttributeValueID);
					result = org.OH_Code;
					break;
				case AlternateGLAccountAttributeCode.OCG:
					result = OCGList.GetDescriptionFromCode(AAA_Value);
					break;
				case AlternateGLAccountAttributeCode.LFE:
					result = LFEList.GetDescriptionFromCode(AAA_Value);
					break;
				case AlternateGLAccountAttributeCode.LFO:
					result = LFOList.GetDescriptionFromCode(AAA_Value);
					break;
				case AlternateGLAccountAttributeCode.TIC:
					result = TICList.GetDescriptionFromCode(AAA_Value);
					break;
				case AlternateGLAccountAttributeCode.SPR:
					result = SPRList.GetDescriptionFromCode(AAA_Value);
					break;
			}

			return result;
		}

		protected override ZString HumanReadableShortcutNameCore => string.Join(" - ", AlternateGLAccount.AGA_AccountNum, AlternateGLAccount.AGA_Description, GLHeader.AG_AccountNum);

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			var newFactory = new BusinessObjectFactory();
			var glHeader = newFactory.NewWithValidTestData<AccGLHeader>();
			var account = newFactory.NewWithValidTestData<AccAlternateGLAccount>();
			newFactory.Save();

			var attributes = account.AlternateGLAccountAttributes;
			attributes.Add(this);
			this.AAA_AAC_AlternateChart = account.AGA_AAC_AlternateChart;
			this.AAA_AG_GLHeader = glHeader.PK;
			this.AAA_Sequence = attributes.Count;
			this.AAA_Attribute = "LFE";
			this.AAA_Value = "WEU";
		}
#endif
	}
}
