using System;

namespace OcmPoc.Components.Paralleliser
{
	static class Display
    {
		static readonly object consoleLock = new object();

	    public static void WriteLine(object value)
	    {
		    //WriteLine(value.ToString());
	    }

	    public static void WriteLine(string text)
	    {
		    //WriteLine(ConsoleColor.Gray, text);
	    }

	    public static void WriteLine(ConsoleColor colour, string text)
	    {
		    lock (consoleLock)
		    {
			    var oldColour = Console.ForegroundColor;
			    Console.ForegroundColor = colour;
			    Console.WriteLine(text);
			    Console.ForegroundColor = oldColour;
		    }
	    }
	}
}
