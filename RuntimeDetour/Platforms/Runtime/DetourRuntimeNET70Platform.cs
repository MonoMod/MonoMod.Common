using MonoMod.RuntimeDetour;
using MonoMod.RuntimeDetour.Platforms;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace MonoMod.RuntimeDetour.Platforms {
    // This is based on the NET 6.0 implementation since they are almost identical, save for some DomainAssembly/PEAssembly restructuring and the JIT GUID change
#if !MONOMOD_INTERNAL
    public
#endif
    class DetourRuntimeNET70Platform : DetourRuntimeNET60Platform {

        // see DetourRuntimeNET60Platform
        public static new readonly Guid JitVersionGuid = new Guid("f2a217c4-2a69-4308-99ce-8292c6763776");

        protected override unsafe void MakeAssemblySystemAssembly(Assembly assembly) {
            // see base method for more info

            IntPtr domAssembly = (IntPtr) _runtimeAssemblyPtrField.GetValue(assembly);
            
            // DomainAssembly is now located at src/coreclr/vm/domainassembly.h with the Assembly* at the beginning
            IntPtr pAssembly = *(IntPtr*)domAssembly;
            
            // Assembly in src/coreclr/src/vm/assembly.hpp
            int pAssemOffset =
                IntPtr.Size + // PTR_BaseDomain        m_pDomain;
                IntPtr.Size + // PTR_ClassLoader       m_pClassLoader;
                IntPtr.Size + // PTR_MethodDesc        m_pEntryPoint;
                IntPtr.Size + // PTR_Module            m_pManifest;
                0; // here is out PEAssembly* (manifestFile)

            IntPtr peAssembly = *(IntPtr*) (((byte*) pAssembly) + pAssemOffset);

            // PEAssembly was moved to src/coreclr/vm/peassembly.h, most fields were removed
            int peAssemOffset =
                IntPtr.Size + // VTable ptr
                // PEAssembly
                IntPtr.Size + // PTR_PEImage              m_PEImage;
                sizeof(int) + // BOOL                     m_MDImportIsRW_Debugger_Use_Only; // i'm pretty sure that these bools are sizeof(int)
                                                                                            // but they might not be, and it might vary (that would be a pain in the ass)
                IntPtr.Size + // IMDInternalImport       *m_pMDImport or m_pMDImport_UseAccessor, not really sure
                IntPtr.Size + // IMetaDataImport2        *m_pImporter;
                IntPtr.Size + // IMetaDataEmit           *m_pEmitter;
                sizeof(long) + // Volatile<LONG>           m_refCount; // fuck C long
                + 0;          // bool                     m_isSystem

            int* flags = (int*) (((byte*) peAssembly) + peAssemOffset);
            *flags = 1;
        }
    }
}
