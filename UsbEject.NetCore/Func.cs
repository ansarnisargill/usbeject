// UsbEject version 2.0 May 2017
// written by Simon Mourier <email: simon [underscore] mourier [at] hotmail [dot] com>
// updated by Dmitry Shechtman

#if NET20
namespace UsbEject
{
    /// <summary>
    /// An alternative to <c>System.Func</c> which is not present in .NET 2.0.
    /// </summary>
    public delegate TResult Func<out TResult>();
}
#endif