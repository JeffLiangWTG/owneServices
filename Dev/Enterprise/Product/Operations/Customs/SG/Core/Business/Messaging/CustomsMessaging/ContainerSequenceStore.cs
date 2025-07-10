using System.Collections.Generic;

namespace Enterprise.Customs.SG.V4.Business
{
	public class ContainerSequenceStore : IContainerSequenceStore
	{
		public ContainerSequenceStore(Dictionary<string, int> sequenceData)
		{
			internalDictionary = sequenceData ?? new Dictionary<string, int>();
		}

		readonly Dictionary<string, int> internalDictionary;

		public int GetSequenceNumber(string containerId)
		{
			return (internalDictionary.ContainsKey(containerId)) ? internalDictionary[containerId] : -1;
		}

		public int HighestSequenceNumber
		{
			get
			{
				if (highestSequenceNumber == null)
				{
					int maxNumber = 0;

					foreach (var seqNumber in internalDictionary.Values)
					{
						if (seqNumber > maxNumber)
						{
							maxNumber = seqNumber;
						}
					}

					highestSequenceNumber = maxNumber;
				}

				return highestSequenceNumber.Value;
			}
		}
		int? highestSequenceNumber;
	}
}
