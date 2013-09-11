using System;

namespace WorkTimeAutomaticControl.Exceptions
{
	//Exception for crash handler
	internal class CrashHandlerException : Exception
	{
		internal CrashHandlerException(string message)
			: base(message)
		{ }
	}
}
