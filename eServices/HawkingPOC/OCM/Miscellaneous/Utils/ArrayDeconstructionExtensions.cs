namespace OcmPoc.Utils
{
	public static class ArrayDeconstructionExtensions
    {
		public static void Deconstruct<T>(this T[] items, out T item0, out T item1)
		{
			item0 = items[0];
			item1 = items[1];
		}
    }
}
