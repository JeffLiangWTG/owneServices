using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input
{
	public partial class OGAFD02 : Messaging.Business.MessageBuildingBlocks.Input.Abstract.OGAFD02, IBIRDOGALineRecord
	{
		#region IBIRDOGALineRecord Members

		void IBIRDOGALineRecord.Update(IOGALine ogaLine, INotifications notifications)
		{
			FDA fdaLine = (FDA)ogaLine;

			//Unit1Quantity is the outermost UQ if five UQs exist etc
			if (!Unit5Measure.IsEmpty)
			{
				fdaLine.US_FDAQty5 = Unit1Quantity;
				fdaLine.US_FDAMeasure5 = Unit1Measure;

				fdaLine.US_FDAQty4 = Unit2Quantity;
				fdaLine.US_FDAMeasure4 = Unit2Measure;

				fdaLine.US_FDAQty3 = Unit3Quantity;
				fdaLine.US_FDAMeasure3 = Unit3Measure;

				fdaLine.US_FDAQty2 = Unit4Quantity;
				fdaLine.US_FDAMeasure2 = Unit4Measure;

				fdaLine.US_FDAQty1 = Unit5Quantity;
				fdaLine.US_FDAMeasure1 = Unit5Measure;
			}
			else if (!Unit4Measure.IsEmpty)
			{
				fdaLine.US_FDAQty5 = 0m;
				fdaLine.US_FDAMeasure5 = ZString.Empty;

				fdaLine.US_FDAQty4 = Unit1Quantity;
				fdaLine.US_FDAMeasure4 = Unit1Measure;

				fdaLine.US_FDAQty3 = Unit2Quantity;
				fdaLine.US_FDAMeasure3 = Unit2Measure;

				fdaLine.US_FDAQty2 = Unit3Quantity;
				fdaLine.US_FDAMeasure2 = Unit3Measure;

				fdaLine.US_FDAQty1 = Unit4Quantity;
				fdaLine.US_FDAMeasure1 = Unit4Measure;
			}
			else if (!Unit3Measure.IsEmpty)
			{
				fdaLine.US_FDAQty5 = 0m;
				fdaLine.US_FDAMeasure5 = ZString.Empty;

				fdaLine.US_FDAQty4 = 0m;
				fdaLine.US_FDAMeasure4 = ZString.Empty;

				fdaLine.US_FDAQty3 = Unit1Quantity;
				fdaLine.US_FDAMeasure3 = Unit1Measure;

				fdaLine.US_FDAQty2 = Unit2Quantity;
				fdaLine.US_FDAMeasure2 = Unit2Measure;

				fdaLine.US_FDAQty1 = Unit3Quantity;
				fdaLine.US_FDAMeasure1 = Unit3Measure;
			}
			else if (!Unit2Measure.IsEmpty)
			{
				fdaLine.US_FDAQty5 = 0m;
				fdaLine.US_FDAMeasure5 = ZString.Empty;

				fdaLine.US_FDAQty4 = 0m;
				fdaLine.US_FDAMeasure4 = ZString.Empty;

				fdaLine.US_FDAQty3 = 0m;
				fdaLine.US_FDAMeasure3 = ZString.Empty;

				fdaLine.US_FDAQty2 = Unit1Quantity;
				fdaLine.US_FDAMeasure2 = Unit1Measure;

				fdaLine.US_FDAQty1 = Unit2Quantity;
				fdaLine.US_FDAMeasure1 = Unit2Measure;
			}
			else
			{
				fdaLine.US_FDAQty5 = 0m;
				fdaLine.US_FDAMeasure5 = ZString.Empty;

				fdaLine.US_FDAQty4 = 0m;
				fdaLine.US_FDAMeasure4 = ZString.Empty;

				fdaLine.US_FDAQty3 = 0m;
				fdaLine.US_FDAMeasure3 = ZString.Empty;

				fdaLine.US_FDAQty2 = 0m;
				fdaLine.US_FDAMeasure2 = ZString.Empty;

				fdaLine.US_FDAQty1 = Unit1Quantity;
				fdaLine.US_FDAMeasure1 = Unit1Measure;
			}
		}

		#endregion
	}
}
