using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace WolfClone.FirstPerson.Engine;

internal class Player
{
    public Vector2 Position { get; private set; }
    public Vector2 Direction { get; private set; }

    public Player(float posX, float posY, float dirX = 1, float dirY = 0)
    {
        Position = new Vector2(posX, posY);
        Direction = new Vector2(dirX, dirY);
        Direction = Vector2.Normalize(Direction);
    }

    public void DebugMovement(float deltaTime, Vector2 input)
    {
        float moveSpeed = 5.0f;
        float rotationSpeed = 3.0f;

        float moveStep = -input.Y * moveSpeed * deltaTime;
        Position += new Vector2(moveStep * Direction.X, moveStep * Direction.Y);

        float rotAngle = input.X * rotationSpeed * deltaTime;
        Direction = Vector2.Rotate(Direction, rotAngle);
        Direction = Vector2.Normalize(Direction);
    }
}
