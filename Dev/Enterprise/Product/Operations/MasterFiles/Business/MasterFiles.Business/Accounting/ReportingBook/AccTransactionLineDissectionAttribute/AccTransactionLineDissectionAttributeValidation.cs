using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccTransactionLineDissectionAttributeValidation : AutoAccTransactionLineDissectionAttributeValidation
	{
		public AccTransactionLineDissectionAttributeValidation(AutoAccTransactionLineDissectionAttribute parent) : base(parent)
		{
		}

		protected override void CheckALD_AttributeValue()
		{
			base.CheckALD_AttributeValue();
			if (!Parent.ALD_AttributeValueInfo.ReadOnly)
			{
				ListValidation.ErrorIfInvalidCode(Parent.ALD_AttributeValueInfo);
				CheckValueMandatory(Parent.ALD_AttributeValueInfo);
			}
		}
		protected override void CheckALD_AttributeValueID()
		{
			base.CheckALD_AttributeValueID();
			if (!Parent.ALD_AttributeValueIDInfo.ReadOnly && !Parent.ALD_AttributeValueIDInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidPK(Parent.ALD_AttributeValueIDInfo);
				CheckValueMandatory(Parent.ALD_AttributeValueIDInfo);
			}
		}

		void CheckValueMandatory(ZPropertyInfo info)
		{
			if (info.Value.IsEmpty)
			{
				var chartsWithSeparateNumber = GetChartsWithSeparateNumberingDissection();
				if (chartsWithSeparateNumber.Any())
				{
					var chartsWithSeparateNumberString = string.Join(", ", chartsWithSeparateNumber);
					info.AddError(GetErrorMessageForValueMandatory(chartsWithSeparateNumberString));
				}
			}
		}

		string[] GetChartsWithSeparateNumberingDissection()
		{
			var charts = System.Array.Empty<string>();

			if (Parent.TransactionLine?.GLHeader != null)
			{
				charts = Parent.TransactionLine.GLHeader.AlternateGLAccountDissections.Cast<AccAlternateGLAccountDissection>().Where(x => x.ADC_Attribute == Parent.ALD_Attribute && x.ADC_SeparateNumbering).Select(x => "'" + x.AlternateChart.AAC_Code + "'").ToArray();
			}

			return charts;
		}

		string GetErrorMessageForValueMandatory(string charts) => Res.GetString("0F550633-EA40-4FD4-B5BF-B5C0EBBD7E27", "A value must be recorded for attribute '{0}' as this is needed to locate the Alternate Account Number for {1} Chart.", Parent.ALD_Attribute, charts);
	}
}
