using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Principal;
using Microsoft.Win32.SafeHandles;

namespace UnspedHealth.Core.Utilities;

[SupportedOSPlatform("WINDOWS")]
public static class ImpersonationHelper
{
    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    private static extern bool CloseHandle(IntPtr handle);

    private const int LOGON32_LOGON_NEW_CREDENTIALS = 9;
    private const int LOGON32_PROVIDER_DEFAULT = 0;

    /// <summary>
    /// Senkron işleri farklı kullanıcı yetkisiyle çalıştırır.
    /// </summary>
    public static void RunAs(string domain, string username, string password, Action action)
    {
        IntPtr userHandle = IntPtr.Zero;
        try
        {
            if (LogonUser(username, domain, password, LOGON32_LOGON_NEW_CREDENTIALS, LOGON32_PROVIDER_DEFAULT, ref userHandle))
            {
                WindowsIdentity.RunImpersonated(new SafeAccessTokenHandle(userHandle), action);
            }
            else
            {
                throw new UnauthorizedAccessException($"LogonUser hatası: {Marshal.GetLastWin32Error()}");
            }
        }
        finally
        {
            // Handle'ı kapatmak sızıntıyı önler.
            if (userHandle != IntPtr.Zero) CloseHandle(userHandle);
        }
    }

    /// <summary>
    /// Asenkron (Task) işleri farklı kullanıcı yetkisiyle çalıştırır.
    /// </summary>
    public static async Task RunAsAsync(string domain, string username, string password, Func<Task> action)
    {
        IntPtr userHandle = IntPtr.Zero;
        try
        {
            if (LogonUser(username, domain, password, LOGON32_LOGON_NEW_CREDENTIALS, LOGON32_PROVIDER_DEFAULT, ref userHandle))
            {
                await WindowsIdentity.RunImpersonatedAsync(new SafeAccessTokenHandle(userHandle), action);
            }
            else
            {
                throw new UnauthorizedAccessException($"LogonUser Async hatası: {Marshal.GetLastWin32Error()}");
            }
        }
        finally
        {
            if (userHandle != IntPtr.Zero) CloseHandle(userHandle);
        }
    }
}