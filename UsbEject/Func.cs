#if NET20
namespace UsbEject
{
    /// <summary>
    /// An alternative to <c>System.Func</c> which is not present in .NET 2.0.
    /// </summary>
    public delegate TResult Func<out TResult>();
}
#endif