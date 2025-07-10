namespace CargoWise.RefDbRepo.ILReferenceData.Business
{
	public class UnknownTableException : System.Exception
	{
		public UnknownTableException() : base()
		{
		}
		public UnknownTableException(string message) : base(message)
		{
		}

		public UnknownTableException(string message, System.Exception e) : base(message, e)
		{
		}
	}
}
