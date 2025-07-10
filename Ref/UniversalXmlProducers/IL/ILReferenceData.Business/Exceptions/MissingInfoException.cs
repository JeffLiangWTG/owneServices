namespace CargoWise.RefDbRepo.ILReferenceData.Business
{
	public class MissingInfoException : System.Exception
	{
		public MissingInfoException() : base()
		{
		}
		public MissingInfoException(string message) : base(message)
		{
		}

		public MissingInfoException(string message, System.Exception e) : base(message, e)
		{
		}
	}
}
