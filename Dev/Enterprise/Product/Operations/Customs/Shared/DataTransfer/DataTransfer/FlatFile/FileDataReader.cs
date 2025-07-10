namespace Enterprise.Customs.DataTransfer
{
	public abstract class FileDataReader
	{
		public FileDataReader(string fileName)
		{
			this.FileName = fileName;
		}

		public abstract string[][] Records { get; }
		protected readonly string FileName;
	}
}
