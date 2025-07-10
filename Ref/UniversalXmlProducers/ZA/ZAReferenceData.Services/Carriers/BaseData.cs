using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;

namespace CargoWise.RefDbRepo.ZAReferenceData.Services.Carriers
{
	public class BaseData<T> : ISourceData
		where T : class
	{
		public DateTime PublicationDate { get; set; }
		public List<T> Data { get; set; }
	}
}
