using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class WHSPackLineCollection : DependentBusinessObjectCollection<WHSPackLine, JobDeclaration>
	{
		public WHSPackLineCollection(JobDeclaration declaration)
			: base(declaration, new ZQuery(CusAddInfoSchema.B7_Type, CusAddInfoTypeAttribute.Codes.USWHSPackLine))
		{
		}

		public WHSPackLine AddNew(WHSPack whsPack)
		{
			var result = AddNew();
			if (whsPack != null)
			{
				result.US_B7_WHSPack = whsPack.PK;
			}
			return result;
		}

		public WHSPackLine AddNew(JobComInvoiceLine invoiceLine)
		{
			var result = AddNew();
			if (invoiceLine != null)
			{
				result.US_JI_InvoiceLine = invoiceLine.PK;
			}
			return result;
		}

		#region Implementation

		protected override CargoWise.Schema.SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return CusAddInfoSchema.B7_ParentID; }
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var line = (WHSPackLine)child;
			using (line.GetValidationSuspender())
			using (line.SuspendSettingHasChanges())
			{
				if (Count == 0)
				{
					SetDefaultForFirstLine(line);
				}
				else if (Count > 0)
				{
					var previousLine = this[Count - 1];

					SetDefaultFromPreviousLine(previousLine, line);
				}
			}
		}

		void SetDefaultForFirstLine(WHSPackLine firstLine)
		{
			if (Master.WHSPacks.Count > 0)
			{
				firstLine.US_B7_WHSPack = Master.WHSPacks[0].PK;
			}
		}

		void SetDefaultFromPreviousLine(WHSPackLine previousLine, WHSPackLine currentLine)
		{
			currentLine.US_B7_WHSPack = previousLine.US_B7_WHSPack;
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			var line = (WHSPackLine)bizOAdded;
			var invoiceLine = line.InvoiceLine;
			if (invoiceLine != null)
			{
				invoiceLine.RefreshWHSPackLines();
			}
		}

		#endregion
	}
}
