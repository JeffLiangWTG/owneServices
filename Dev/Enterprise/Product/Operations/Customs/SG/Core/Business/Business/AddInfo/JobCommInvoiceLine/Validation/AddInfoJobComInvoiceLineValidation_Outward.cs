using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.V4.Business
{
	public abstract class AddInfoJobComInvoiceLineValidation_Outward : AddInfoJobComInvoiceLineValidation_CUSDEC
	{
		public AddInfoJobComInvoiceLineValidation_Outward(AddInfoJobComInvoiceLine parent)
			: base(parent)
		{
		}

		protected override void CheckSG_CategoryCode()
		{
			base.CheckSG_CategoryCode();
			ValidationStrategicValue(Parent.SG_CategoryCodeInfo, "Category Code");
		}

		protected override void CheckSG_EndUseCode1()
		{
			base.CheckSG_EndUseCode1();
			ValidationStrategicValue(Parent.SG_EndUseCode1Info, "End-use Code 1");
		}

		protected override void CheckSG_EndUseCode2()
		{
			base.CheckSG_EndUseCode2();
			ValidationStrategicValue(Parent.SG_EndUseCode2Info, "End-use Code 2");
		}

		protected override void CheckSG_EndUseCode3()
		{
			base.CheckSG_EndUseCode3();
			ValidationStrategicValue(Parent.SG_EndUseCode3Info, "End-use Code 3");
		}

		protected override void CheckSG_EndUseDescription()
		{
			base.CheckSG_EndUseDescription();
			ValidationStrategicValue(Parent.SG_EndUseDescriptionInfo, "End-use Description");
		}

		void ValidationStrategicValue(ZPropertyInfo info, string description)
		{
			if (Parent.InvoiceLine.SG_IsStrategic)
			{
				MandatoryValidation.MessageErrorIfNotEntered(info);
			}
			else
			{
				MandatoryValidation.WarnIfIsEntered(info, description + " unless required for Strategic Goods");
			}
		}
	}
}
