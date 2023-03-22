using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MonoMod.RuntimeDetour.Platforms {
#if !MONOMOD_INTERNAL
    public
#endif
    unsafe class DetourNativeLoongArch64Platform : IDetourNativePlatform {
        public enum DetourType : byte {
            LoongArch64,
        }

        private static readonly uint[] DetourSizes = {
            4 + 4 + 8,
        };

        public void Apply(NativeDetourData detour) {
            int offs = 0;

            // pcaddi $r21,3
            detour.Method.Write(ref offs, (byte) 0x75);
            detour.Method.Write(ref offs, (byte) 0x00);
            detour.Method.Write(ref offs, (byte) 0x00);
            detour.Method.Write(ref offs, (byte) 0x18);
            // ld.d  $r21,$r21,0
            detour.Method.Write(ref offs, (byte) 0xb5);
            detour.Method.Write(ref offs, (byte) 0x02);
            detour.Method.Write(ref offs, (byte) 0xc0);
            detour.Method.Write(ref offs, (byte) 0x28);
            // jirl $r0, $r21, 0
            detour.Method.Write(ref offs, (byte) 0xA0);
            detour.Method.Write(ref offs, (byte) 0x02);
            detour.Method.Write(ref offs, (byte) 0x00);
            detour.Method.Write(ref offs, (byte) 0x4C);
            // <to>
            detour.Method.Write(ref offs, (ulong) detour.Target);
        }

        public void Copy(IntPtr src, IntPtr dst, byte type) {
            *(uint*) ((long) dst) = *(uint*) ((long) src);
            *(uint*) ((long) dst + 4) = *(uint*) ((long) src + 4);
            *(ulong*) ((long) dst + 8) = *(ulong*) ((long) src + 8);
        }

        public NativeDetourData Create(IntPtr from, IntPtr to, byte? type = null) {
            NativeDetourData detour = new NativeDetourData {
                Method = (IntPtr) ((long) from & ~0x1),
                Target = (IntPtr) ((long) to & ~0x1)
            };
            detour.Size = DetourSizes[0];
            return detour;
        }

        public void FlushICache(IntPtr src, uint size) {
        }

        public void Free(NativeDetourData detour) {
            // No extra data.
        }

        public void MakeExecutable(IntPtr src, uint size) {
            // no-op.
        }

        public void MakeReadWriteExecutable(IntPtr src, uint size) {
            // no-op.
        }

        public void MakeWritable(IntPtr src, uint size) {
            // no-op.
        }

        public IntPtr MemAlloc(uint size) {
            return Marshal.AllocHGlobal((int) size);
        }

        public void MemFree(IntPtr ptr) {
            Marshal.FreeHGlobal(ptr);
        }

        private readonly byte[] _FlushCache64 = { 0x85, 0x94, 0x10, 0x00, 0x0c, 0xf0, 0xff, 0x02, 0x84, 0xb0, 0x14, 0x00, 0x85, 0x14, 0x00, 0x6c, 0x86, 0x00, 0x80, 0x03, 0x00, 0x00, 0x72, 0x38, 0xc6, 0x10, 0xc0, 0x02, 0xc5, 0xf8, 0xff, 0x6b, 0x00, 0x00, 0x72, 0x38, 0x85, 0x10, 0x00, 0x6c, 0x00, 0x00, 0x72, 0x38, 0x84, 0x10, 0xc0, 0x02, 0x85, 0xf8, 0xff, 0x6b, 0x00, 0x00, 0x72, 0x38, 0x20, 0x00, 0x00, 0x4c};
    }
}
