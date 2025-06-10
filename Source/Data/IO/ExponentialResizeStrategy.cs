///////////////////////////////////////////////////////
/// Filename: ExponentialResizeStrategy.cs
/// Date: June 9, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

public sealed class ExponentialResizeStrategy : IResizeStrategy
{

    public int Multiplier { get; }

    public ExponentialResizeStrategy(int multiplier = 2)
    {
        this.Multiplier = multiplier;
    }

    public int Resize(int neededBytes) =>
        neededBytes * Multiplier;

}