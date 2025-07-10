using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class AccTransactionLineDissectionAttribute : AutoAccTransactionLineDissectionAttribute
	{
		public AccTransactionLineDissectionAttribute(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region ALD_AttributeValue

		[List("Lookups.AttributeValueList")]
		public override ZString ALD_AttributeValue { get => base.ALD_AttributeValue; set => base.ALD_AttributeValue = value; }

		protected bool ALD_AttributeValue_ReadOnly
		{
			get { return Lookups.AlternateGLAccountAttributeCodeWithID.Contains(ALD_Attribute.ToString()); }
		}

		#endregion

		#region ALD_AttributeValue

		[List("Lookups.AttributeValueIDCollection")]
		public override ZGuid ALD_AttributeValueID { get => base.ALD_AttributeValueID; set => base.ALD_AttributeValueID = value; }

		protected bool ALD_AttributeValueID_ReadOnly
		{
			get { return !Lookups.AlternateGLAccountAttributeCodeWithID.Contains(ALD_Attribute.ToString()); }
		}

		#endregion

		public ZString ApplicableAlternateChart
		{
			get
			{
				var result = ZString.Empty;

				if (TransactionLine != null && TransactionLine.GLHeader != null && !string.IsNullOrEmpty(ALD_Attribute))
				{
					var dissectionWithAttribute = TransactionLine.GLHeader.AlternateGLAccountDissections.Cast<AccAlternateGLAccountDissection>().Where(x => x.ADC_Attribute == ALD_Attribute);

					if (dissectionWithAttribute.Any())
					{
						result = string.Join(", ", dissectionWithAttribute.Select(x => x.AlternateChart.AAC_Code + " - " + x.AlternateChart.AAC_Description));
					}
				}

				return result;
			}
		}

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			var transactionLines = Factory.NewWithValidTestData<AccTransactionLines>();

			base.FillWithValidTestDataCore(kind, propertyPath);

			ALD_AL_TransactionLine = transactionLines.PK;
			ALD_Attribute = "OCG";
			ALD_AttributeValue = "TPY";
		}

#endif

	}
}
