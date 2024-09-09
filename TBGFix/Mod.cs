using System.Drawing;
using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.X86;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;

namespace TBGFix;

public unsafe class Mod : IMod {
    private WeakReference<IReloadedHooks>? hooksRef;
    private IHook<GetSystemInfoDelegate>? getSystemInfoHook;
    private ILoggerV1? logger;

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern IntPtr LoadLibraryW([MarshalAs(UnmanagedType.LPWStr)] string lpFileName);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
    public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

    [Function(CallingConventions.Stdcall)]
    public delegate void GetSystemInfoDelegate(SystemInfo* lpSystemInfo);

    [StructLayout(LayoutKind.Sequential)]
    public struct SystemInfo {
        public ushort wProcessorArchitecture;
        public ushort wReserved;
        public uint dwPageSize;
        public nint lpMinimumApplicationAddress;
        public nint lpMaximumApplicationAddress;
        public nint dwActiveProcessorMask;
        public uint dwNumberOfProcessors;
        public uint dwProcessorType;
        public uint dwAllocationGranularity;
        public ushort wProcessorLevel;
        public ushort wProcessorRevision;
    }

    public void Start(IModLoaderV1 loader) {
        this.hooksRef = loader.GetController<IReloadedHooks>();
        this.logger = loader.GetLogger();
        if (this.hooksRef is not null && this.hooksRef.TryGetTarget(out var hooks)) {
            var kernel32 = LoadLibraryW("kernel32");
            var getSystemInfo = GetProcAddress(kernel32, "GetSystemInfo");
            this.getSystemInfoHook = hooks.CreateHook<GetSystemInfoDelegate>(this.GetSystemInfoDetour, getSystemInfo);
            this.getSystemInfoHook = this.getSystemInfoHook.Activate();
            this.getSystemInfoHook.Enable();
            this.logger!.PrintMessage("GetSystemInfo hooked", Color.Green);
        } else {
            this.logger!.PrintMessage("rip", Color.Red);
        }
    }

    public void GetSystemInfoDetour(SystemInfo* lpSystemInfo) {
        this.getSystemInfoHook!.OriginalFunction(lpSystemInfo);
        const int max = 4;
        if (lpSystemInfo->dwNumberOfProcessors >= max) {
            lpSystemInfo->dwNumberOfProcessors = max;
        }
    }

    public void Suspend() { }
    public void Resume() { }
    public bool CanSuspend() => false;

    public void Unload() { }
    public bool CanUnload() => false;

    public Action Disposing => () => { };
}
