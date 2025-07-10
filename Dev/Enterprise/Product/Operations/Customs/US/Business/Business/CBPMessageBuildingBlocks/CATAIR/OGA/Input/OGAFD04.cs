using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input
{
	public partial class OGAFD04 : Messaging.Business.MessageBuildingBlocks.Input.Abstract.OGAFD04, IBIRDOGALineRecord
	{
		#region IBIRDOGALineRecord Members

		void IBIRDOGALineRecord.Update(IOGALine ogaLine, INotifications notifications)
		{
			FDA fdaLine = (FDA)ogaLine;

			if (!Unit6Measure.IsEmpty && Unit6Quantity > 0)
			{
				fdaLine.US_FDAQty6 = fdaLine.US_FDAQty5;
				fdaLine.US_FDAMeasure6 = fdaLine.US_FDAMeasure5;

				fdaLine.US_FDAQty5 = fdaLine.US_FDAQty4;
				fdaLine.US_FDAMeasure5 = fdaLine.US_FDAMeasure4;

				fdaLine.US_FDAQty4 = fdaLine.US_FDAQty3;
				fdaLine.US_FDAMeasure4 = fdaLine.US_FDAMeasure3;

				fdaLine.US_FDAQty3 = fdaLine.US_FDAQty2;
				fdaLine.US_FDAMeasure3 = fdaLine.US_FDAMeasure2;

				fdaLine.US_FDAQty2 = fdaLine.US_FDAQty1;
				fdaLine.US_FDAMeasure2 = fdaLine.US_FDAMeasure1;

				fdaLine.US_FDAQty1 = Unit6Quantity;
				fdaLine.US_FDAMeasure1 = Unit6Measure;
			}

			//TODO Joo contact when WI00012421 is checked in
		}

		#endregion
	}
}
