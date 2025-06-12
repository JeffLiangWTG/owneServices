
using System;
using System.IO;

// Copied from C:\dev\Enterprise\Product\Operations\Customs\ASYCUDA\ASYCUDA.Business\CAR\XmlToCar
// I cannot branch from $/Dev to $/eServcies, so copying instead.
// Please view original file and this file in KDiff to see changes. 
namespace CargoWise.eHub.Products.AsycudaCustoms.Common
{
	public class VectorToCar
	{
		public void createCar(Vector paramVector, Stream paramStream)
		{
			int i = 0;
			try
			{
				var localDataOutputStream = new DataOutputStreamWrapper(paramStream);

				localDataOutputStream.writeBytes(((Vector)paramVector.elementAt(0)).elementAt(0).ToString());
				localDataOutputStream.writeByte(i);

				localDataOutputStream.writeBytes(((Vector)paramVector.elementAt(0)).elementAt(1).ToString());
				localDataOutputStream.write(i);
				int k;
				for (int loopJ1 = 2; loopJ1 < 5; loopJ1++)
				{
					localDataOutputStream.writeByte(Integer.valueOf(((Vector)paramVector.elementAt(0)).elementAt(loopJ1).ToString()).byteValue());
					for (k = 1; k <= 3; k++)
					{
						localDataOutputStream.writeByte(i);
					}
				}
				for (int loopJ2 = 5; loopJ2 < ((Vector)paramVector.elementAt(0)).size(); loopJ2++)
				{
					localDataOutputStream.writeBytes(((Vector)paramVector.elementAt(0)).elementAt(loopJ2).ToString());
					localDataOutputStream.writeByte(i);
				}
				var j = ((Vector)paramVector.elementAt(1)).size() - 1;

				localDataOutputStream.writeBytes(Integer.valueOf(j).ToString());
				localDataOutputStream.writeByte(i);
				int m;
				if (j != 0)
				{
					for (k = 1; k < ((Vector)paramVector.elementAt(1)).size(); k++)
					{
						for (m = 0; m < 10; m++)
						{
							localDataOutputStream.writeBytes(((Vector)((Vector)paramVector.elementAt(1)).elementAt(k)).elementAt(m).ToString());
							localDataOutputStream.writeByte(i);
						}
					}
				}
				if (((Vector)paramVector.elementAt(2)).size() > 0)
				{
					for (k = 0; k < ((Vector)paramVector.elementAt(2)).size(); k++)
					{
						for (m = 0; m < 71; m++)
						{
							localDataOutputStream.writeBytes(((Vector)((Vector)paramVector.elementAt(2)).elementAt(k)).elementAt(m).ToString());
							localDataOutputStream.writeByte(i);
						}
					}
					if (((Vector)paramVector.elementAt(3)).size() > 0)
					{
						for (k = 0; k < ((Vector)paramVector.elementAt(3)).size(); k++)
						{
							for (m = 0; m < 101; m++)
							{
								localDataOutputStream.writeBytes(((Vector)((Vector)paramVector.elementAt(3)).elementAt(k)).elementAt(m).ToString());
								localDataOutputStream.writeByte(i);
							}
						}
					}
				}
			}
			catch (Exception localIOException)
			{
				throw localIOException;
			}
		}
	}
}