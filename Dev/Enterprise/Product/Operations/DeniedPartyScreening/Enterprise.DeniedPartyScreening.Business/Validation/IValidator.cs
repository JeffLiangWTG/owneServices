namespace Enterprise.DeniedPartyScreening.Business
{
	interface IValidator<in T>
	{
		void Validate(T entity);
	}
}
