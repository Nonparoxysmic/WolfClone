using Vector2D = Microsoft.Xna.Framework.Vector2;

namespace WolfClone.FirstPerson.Engine;

internal class Player
{
    public Vector2D Position { get; private set; }
    public Vector2D Direction { get; private set; }

    public Player(float posX, float posY, float dirX = 1, float dirY = 0)
    {
        Position = new Vector2D(posX, posY);
        Direction = new Vector2D(dirX, dirY);
        Direction = Vector2D.Normalize(Direction);
    }

    public void DebugMovement(float deltaTime, Vector2D input)
    {
        float moveSpeed = 5.0f;
        float rotationSpeed = 3.0f;

        float moveStep = -input.Y * moveSpeed * deltaTime;
        Position += new Vector2D(moveStep * Direction.X, moveStep * Direction.Y);

        float rotAngle = input.X * rotationSpeed * deltaTime;
        Direction = Vector2D.Rotate(Direction, rotAngle);
        Direction = Vector2D.Normalize(Direction);
    }
}
