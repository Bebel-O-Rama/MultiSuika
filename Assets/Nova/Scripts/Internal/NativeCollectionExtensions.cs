﻿// Copyright (c) Supernova Technologies LLC
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Nova.Compat
{
    /// <summary>
    /// We need these in compat because the API changes to return T* (instead of void*)
    /// which breaks the pre-compiled assembly in the trial.
    /// </summary>
    internal unsafe static class NativeCollectionExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* GetRawPtr<T>(this NativeList<T> list) where T : unmanaged
        {
            return (T*)list.GetUnsafePtr();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* GetRawReadonlyPtr<T>(this NativeList<T> list) where T : unmanaged
        {
            return (T*)list.GetUnsafeReadOnlyPtr();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* GetRawPtr<T>(this NativeReference<T> reference) where T : unmanaged
        {
            return (T*)reference.GetUnsafePtr();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* GetRawPtrWithoutChecks<T>(this NativeReference<T> reference) where T : unmanaged
        {
            return (T*)reference.GetUnsafePtrWithoutChecks();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* GetRawReadonlyPtr<T>(this NativeReference<T> reference) where T : unmanaged
        {
            return (T*)reference.GetUnsafeReadOnlyPtr();
        }
    }
}

