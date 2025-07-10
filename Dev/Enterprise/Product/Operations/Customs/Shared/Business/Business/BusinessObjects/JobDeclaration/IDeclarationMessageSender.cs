namespace Enterprise.Customs.Business
{
	/// <summary>
	///  What is the purpose of the CusDec?  A new/amended/cancelled declaration?
	/// </summary>
	public abstract class CusdecMessageFunction
	{
		public class New : CusdecMessageFunction { }
		public class Amended : CusdecMessageFunction { }
		public class Deleted : CusdecMessageFunction { }
	}

	public interface IDeclarationMessageSender
	{
		void Send(BaseJobDeclaration declaration, ISendsMessagesToCustoms sendMessagesToCustoms, CusdecMessageFunction functionOfMessageNewAmendedDeleted);
	}
}
