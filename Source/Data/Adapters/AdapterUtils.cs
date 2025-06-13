///////////////////////////////////////////////////////
/// Filename: AdapterUtils.cs
/// Date: June 13, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;

namespace EppNet.Data
{

    public static class AdapterUtils
    {
        public static TAdapter PutComponent<TAdapter, TComponent, TNative>(
            in TAdapter input,
            int index,
            TComponent value)
            where TAdapter : struct, IAdapter<TAdapter, TComponent, TNative>
            where TComponent : unmanaged, IComparable, IComparable<TComponent>
            where TNative : struct
        {
            var mutable = input;
            mutable[index] = value;
            return mutable;
        }

    }

}