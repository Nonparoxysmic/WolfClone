using System;
using Microsoft.Xna.Framework;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace WolfClone.FirstPerson.Engine;

internal abstract class Tile
{
    protected static float _epsilon = 1e-6f;

    protected GridTile _parent;
    protected int _parentGridX;
    protected int _parentGridY;

    public abstract Color CastRay(Vector3 localPosition, Vector3 direction, float length);

    public virtual Tile GetTile(Vector3 localPosition, out Vector3 resultLocalPosition)
    {
        resultLocalPosition = localPosition;
        return this;
    }
}

internal class EmptyTile : Tile
{
    public EmptyTile(int gridX, int gridY, GridTile parent)
    {
        _parent = parent;
        _parentGridX = gridX;
        _parentGridY = gridY;
    }

    public override Color CastRay(Vector3 position, Vector3 direction, float length)
    {
        // Calculate the distances required to hit each boundary of the cube.
        // If direction is 0 in an axis, we make the distance infinite. (The ray is parallel.)
        float tx = direction.X > 0 ? (1 - position.X) / direction.X :
                   (direction.X < 0 ? (0 - position.X) / direction.X :
                                      float.PositiveInfinity);

        float ty = direction.Y > 0 ? (1 - position.Y) / direction.Y :
                   (direction.Y < 0 ? (0 - position.Y) / direction.Y :
                                      float.PositiveInfinity);

        float tz = direction.Z > 0 ? (1 - position.Z) / direction.Z :
                   (direction.Z < 0 ? (0 - position.Z) / direction.Z :
                                      float.PositiveInfinity);

        // Find the minimum travel distance from the three axes.
        float travelLength = Math.Min(tx, Math.Min(ty, tz));

        // Compute the exit position in the cube's local coordinate system.
        Vector3 exitPositionLocal = position + direction * travelLength;

        // If the remaining ray length isn’t long enough to reach the cube boundary,
        // return a base color, otherwise continue the raycast in the parent.
        if (travelLength > length)
        {
            return Color.Black;
        }

        // Convert the cube's local exit position to the parent's coordinate system.
        // Each cube occupies a unit square in the parent's grid tile, so we add the cube’s grid offset.
        Vector3 exitPositionGlobal = new(_parentGridX + exitPositionLocal.X,
                                         _parentGridY + exitPositionLocal.Y,
                                         exitPositionLocal.Z);

        // Delegate to the parent GridTile with the converted coordinates and remaining length.
        return _parent.CastRay(exitPositionGlobal, direction, length - travelLength);
    }
}

internal class BlockTile : Tile
{
    public int ID { get; }

    public BlockTile(int gridX, int gridY, GridTile parent, int id)
    {
        _parent = parent;
        _parentGridX = gridX;
        _parentGridY = gridY;
        ID = id;
    }

    public override Color CastRay(Vector3 position, Vector3 direction, float length)
    {
        // TEMPORARY
        return ID switch
        {
            1 => Color.Blue,
            2 => Color.Green,
            3 => Color.Red,
            4 => Color.Yellow,
            _ => Color.Magenta,
        };
    }
}

internal class GridTile : Tile
{
    public int Width { get; private set; }
    public int Height => Width;

    private readonly Tile[,] _children;

    public GridTile(int gridX, int gridY, GridTile parent, int size)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(size, 1);
        _parent = parent;
        _parentGridX = gridX;
        _parentGridY = gridY;
        Width = (int)Math.Pow(2, size);
        _children = new Tile[Width, Width];
    }

    public Tile this[int x, int y]
    {
        get => _children[x, y];
        set => _children[x, y] = value;
    }

    public override Color CastRay(Vector3 position, Vector3 direction, float length)
    {
        // Before processing, adjust the position slightly when it lies on a boundary.
        if (Math.Abs(position.X - Math.Round(position.X)) < _epsilon)
        {
            position.X += direction.X > 0 ? _epsilon : -_epsilon;
        }
        if (Math.Abs(position.Y - Math.Round(position.Y)) < _epsilon)
        {
            position.Y += direction.Y > 0 ? _epsilon : -_epsilon;
        }
        //if (Math.Abs(position.Z - Math.Round(position.Z)) < _epsilon)
        //{
        //    position.Z += direction.Z > 0 ? _epsilon : -_epsilon;
        //}

        // First, check for vertical termination: the floor or ceiling.
        if (position.Z <= 0.0f)
        {
            // The ray has hit the ceiling.
            return Color.CornflowerBlue;
        }
        if (position.Z >= 1.0f)
        {
            // The ray has hit the floor.
            return Color.SaddleBrown;
        }

        // Determine the cell (cube) indices in the grid tile.
        int cellX = (int)Math.Floor(position.X);
        int cellY = (int)Math.Floor(position.Y);

        // If the computed cell indices are outside the bounds of the grid,
        // it means the ray has left this grid tile; handle that case as needed.
        if (cellX < 0 || cellX >= Width || cellY < 0 || cellY >= Width)
        {
            // If the grid tile is part of a larger grid, convert the position to the parent's coordinate system.
            if (_parent != null)
            {
                // Parent's coordinates: add this tile’s offset to the local position.
                Vector3 parentPosition = new(_parentGridX + position.X,
                                             _parentGridY + position.Y,
                                             position.Z);
                return _parent.CastRay(parentPosition, direction, length);
            }
            // Otherwise, if no parent exists, return a default color.
            return Color.Black;
        }

        // Compute the local coordinates for the target cube.
        // Each cube’s local coordinate system runs from 0 to 1.
        Vector3 localPos = new(position.X - cellX, position.Y - cellY, position.Z);

        // Delegate the raycasting call to the appropriate child cube,
        // passing along the new local position and remaining ray length.
        return _children[cellX, cellY].CastRay(localPos, direction, length);
    }

    public override Tile GetTile(Vector3 localPosition, out Vector3 resultLocalPosition)
    {
        int x = (int)(localPosition.X * Width);
        int y = (int)(localPosition.Y * Height);
        Tile target = _children[x, y];
        Vector3 targetLocal = new(localPosition.X * Width - x, localPosition.Y * Height - y, localPosition.Z);
        return target.GetTile(targetLocal, out resultLocalPosition);
    }
}
