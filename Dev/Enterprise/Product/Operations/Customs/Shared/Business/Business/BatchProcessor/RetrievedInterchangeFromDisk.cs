using System;
using System.IO;
using CargoWise.Types;

namespace Enterprise.Customs.Business.BatchProcessor
{
	/// <summary>
	/// Summary description for RetrievedInterchangeFromDisk.
	/// </summary>
	public class RetrievedInterchangeFromDisk : BaseRetrievedInterchange
	{
		public override ZDateTime RetrievedTime
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (!Filename.IsEmpty)
				{
					if (File.Exists(Filename))
					{
						try
						{
							result = File.GetCreationTime(Filename);
						}
						catch (Exception e) when (
							e is ArgumentException ||
							e is NotSupportedException ||
							e is UnauthorizedAccessException ||
							e is PathTooLongException)
						{ }
					}
				}
				return result;
			}
		}

		public ZString Filename = ZString.Empty;
	}
}
