///////////////////////////////////////////////////////
/// Filename: IResizeStrategy.cs
/// Date: June 9, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

public interface IResizeStrategy
{

    /// <summary>
    /// Takes in the specified number of bytes needed and outputs a
    /// result that adheres to this strategy.
    /// </summary>
    /// <param name="neededBytes"></param>
    /// <returns></returns>

    int Resize(int neededBytes);

}