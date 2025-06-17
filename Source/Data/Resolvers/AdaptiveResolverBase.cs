///////////////////////////////////////////////////////
/// Filename: AdaptiveResolverBase.cs
/// Date: June 16, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;

namespace EppNet.Data
{
    public abstract class AdaptiveResolverBase<TNative, TComponent> : Resolver<TNative>
        where TComponent : struct
    {

        public readonly int NumComponents;

        protected AdaptiveResolverBase(int numComponents, bool autoAdvance = true)
            : base(autoAdvance)
        {
            this.NumComponents = numComponents;
        }

        protected AdaptiveResolverBase(int numComponents, int size, bool autoAdvance = true)
            : base(size, autoAdvance)
        {
            this.NumComponents = numComponents;
        }

        protected AdaptiveResolverBase(int numComponents, int size)
            : base(size)
        {
            this.NumComponents = numComponents;
        }

        public abstract void CopyTo(in TNative native, Span<TComponent> data);
        public abstract TNative ToNative(Span<TComponent> data);

    }
}
