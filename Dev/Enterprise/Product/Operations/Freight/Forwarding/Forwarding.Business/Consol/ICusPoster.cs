namespace Enterprise.Freight.Forwarding.Business
{
	public interface ICusPoster<TBusinessObject>
		 where TBusinessObject : ForwardingConsol
	{
		void PostAndSetSACFlag(TBusinessObject consol);
	}
}
