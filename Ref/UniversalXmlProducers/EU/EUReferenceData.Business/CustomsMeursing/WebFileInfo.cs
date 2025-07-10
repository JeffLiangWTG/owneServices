using System;

namespace CargoWise.RefDbRepo.EUReferenceData.CustomsMeursing.Business
{
	public class WebFileInfo
	{
		public string FileName { get; set; }
		public DateTime ModifiedDate { get; set; }
		public string Month { get; set; }
		public string Year { get; set; }
		public string DownloadPath { get; set; }
	}
}
